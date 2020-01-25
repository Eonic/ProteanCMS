Imports System.Xml
Imports System.Web.HttpUtility
Imports System.Web.Configuration
Imports System.IO
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.SqlClient
Imports VB = Microsoft.VisualBasic
Imports Protean.Cms.Cart
Imports System.Net
Imports CardinalCommerce
Imports System.IdentityModel.Tokens
Imports System.IdentityModel.Tokens.Jwt
Imports System.Security.Claims
Imports System.Text
Imports Newtonsoft.Json.Linq


Namespace Providers
    Namespace Payment
        Public Class PayPalPro

            Public Sub New()
                'do nothing
            End Sub

            Public Sub Initiate(ByRef _AdminXforms As Object, ByRef _AdminProcess As Object, ByRef _Activities As Object, ByRef MemProvider As Object, ByRef myWeb As Cms)

                MemProvider.AdminXforms = New AdminXForms(myWeb)
                MemProvider.AdminProcess = New AdminProcess(myWeb)
                MemProvider.AdminProcess.oAdXfm = MemProvider.AdminXforms
                MemProvider.Activities = New Activities()

            End Sub

            Public Class AdminXForms
                Inherits Cms.Admin.AdminXforms
                Private Const mcModuleName As String = "Providers.Providers.Eonic.AdminXForms"

                Sub New(ByRef aWeb As Cms)
                    MyBase.New(aWeb)
                End Sub

            End Class

            Public Class AdminProcess
                Inherits Cms.Admin

                Dim _oAdXfm As Protean.Providers.Payment.PayPalPro.AdminXForms

                Public Property oAdXfm() As Object
                    Set(ByVal value As Object)
                        _oAdXfm = value
                    End Set
                    Get
                        Return _oAdXfm
                    End Get
                End Property

                Sub New(ByRef aWeb As Cms)
                    MyBase.New(aWeb)
                End Sub
            End Class


            Public Class Activities

                Private Const mcModuleName As String = "Providers.Payment.PayPalPro.Activities"
                Private myWeb As Protean.Cms
                Protected moPaymentCfg As XmlNode
                Private nTransactionMode As TransactionMode

                Enum TransactionMode
                    Live = 0
                    Test = 1
                    Fail = 2
                End Enum


                Public Function GetPaymentForm(ByRef oWeb As Protean.Cms, ByRef oCart As Cms.Cart, ByRef oOrder As XmlElement, Optional returnCmd As String = "cartCmd=SubmitPaymentDetails") As xForm
                    PerfMon.Log("Protean.Providers.payment.PayPalPro", "GetPaymentForm")
                    Dim sSql As String

                    Dim ccXform As xForm

                    Dim Xform3dSec As xForm = Nothing

                    Dim bIsValid As Boolean
                    Dim err_msg As String = ""
                    Dim err_msg_log As String = ""
                    Dim sProcessInfo As String = ""
                    Dim cResponse As String

                    Dim oDictOpt As Hashtable = New Hashtable

                    Dim bCv2 As Boolean = False
                    Dim b3DSecure As Boolean = False
                    Dim b3DSecureV2 As Boolean = False
                    Dim b3DAuthorised As Boolean = False
                    Dim sRedirectURL As String = ""
                    Dim sPaymentRef As String = ""

                    Dim cProcessInfo As String = "PalPalPro"

                    'Get the payment options into a hashtable
                    Dim oPaymentCfg As XmlNode
                    Dim bSavePayment As Boolean = False
                    Dim bAllowSavePayment As Boolean = False
                    Dim sSubmitPath As String = oCart.mcPagePath & returnCmd

                    '3d Sec return values
                    Dim PAResStatus As String = ""
                    Dim Enrolled As String = ""
                    Dim ACSUrl As String = ""
                    Dim Payload As String = ""
                    Dim EciFlag As String = ""
                    Dim CAVV As String = ""
                    Dim Xid As String = ""
                    Dim PaymemtRef As String = ""
                    Dim cAuthCode As String = ""
                    Dim bValidateFirst As Boolean = False

                    Try
                        myWeb = oWeb
                        Dim oEwProv As Protean.Cms.Cart.PaymentProviders = New PaymentProviders(myWeb)

                        'Get values form cart
                        oEwProv.mcCurrency = IIf(oCart.mcCurrencyCode = "", oCart.mcCurrency, oCart.mcCurrencyCode)
                        oEwProv.mcCurrencySymbol = oCart.mcCurrencySymbol
                        If oOrder.GetAttribute("payableType") = "" Then
                            oEwProv.mnPaymentAmount = oOrder.GetAttribute("total")
                        Else
                            oEwProv.mnPaymentAmount = oOrder.GetAttribute("payableAmount")
                            oEwProv.mnPaymentMaxAmount = oOrder.GetAttribute("total")
                            oEwProv.mcPaymentType = oOrder.GetAttribute("payableType")
                        End If
                        oEwProv.mnCartId = oCart.mnCartId
                        oEwProv.mcPaymentOrderDescription = "Ref:" & oCart.OrderNoPrefix & CStr(oCart.mnCartId) & " An online purchase from: " & oCart.mcSiteURL & " on " & niceDate(Now) & " " & TimeValue(Now)
                        oEwProv.mcCardHolderName = oOrder.SelectSingleNode("Contact[@type='Billing Address']/GivenName").InnerText

                        'Repeat Payment Settings
                        Dim repeatAmt As Double = CDbl("0" & oOrder.GetAttribute("repeatPrice"))
                        Dim repeatInterval As String = LCase(oOrder.GetAttribute("repeatInterval"))
                        Dim repeatFrequency As Integer = CDbl("0" & oOrder.GetAttribute("repeatFrequency"))
                        Dim repeatLength As Integer = CDbl("0" & oOrder.GetAttribute("repeatLength"))
                        If repeatLength = 0 Then repeatLength = 1
                        Dim delayStart As Boolean = IIf(LCase(oOrder.GetAttribute("delayStart")) = "true", True, False)
                        Dim startDate As Date

                        If oOrder.GetAttribute("startDate") <> "" Then
                            startDate = CDate(oOrder.GetAttribute("startDate"))
                        Else
                            startDate = Now()
                        End If

                        moPaymentCfg = WebConfigurationManager.GetWebApplicationSection("protean/payment")

                        oPaymentCfg = moPaymentCfg.SelectSingleNode("provider[@name='PayPalPro']")
                        oDictOpt = xmlTools.xmlToHashTable(oPaymentCfg, "value")

                        Select Case CStr(oDictOpt("opperationMode"))
                            Case "true", "test"
                                nTransactionMode = TransactionMode.Test
                            Case "false"
                                nTransactionMode = TransactionMode.Fail
                            Case "live"
                                nTransactionMode = TransactionMode.Live
                        End Select

                        ' override the currency
                        ' If oDictOpt.ContainsKey("currency") Then oDictOpt.Remove("currency")
                        ' oDictOpt.Add("currency", oEwProv.mcCurrency)

                        'We want to use the currency specified in the payment method.
                        If oDictOpt("currency") <> oEwProv.mcCurrency Then
                            oEwProv.mcCurrency = oDictOpt("currency")
                        End If


                        ' Set common variables
                        If oDictOpt("validateCV2") = "on" Then bCv2 = True
                        If oDictOpt("secure3d") = "on" Then b3DSecure = True
                        If oDictOpt("secure3d") = "v2" Then b3DSecureV2 = True
                        ' Commented out because not allowed on this form
                        ' If oDictOpt("allowSavePayment") = "on" Then bAllowSavePayment = True

                        'Load the Xform
                        oEwProv.SetTransactionMode = nTransactionMode
                        Dim PaymentDescription As String = "Make Payment of " & FormatNumber(oEwProv.mnPaymentAmount, 2) & " " & oEwProv.mcCurrency & " by Credit/Debit Card"
                        If repeatAmt > 0 Then
                            PaymentDescription = "Make Repeat Payments of " & repeatAmt & " " & oEwProv.mcCurrency & " per " & repeatInterval & " by Credit/Debit Card"
                        End If
                        If repeatAmt > 0 And oEwProv.mnPaymentAmount > 0 Then
                            PaymentDescription = "Make Initial Payment of " & FormatNumber(oEwProv.mnPaymentAmount, 2) & " " & oEwProv.mcCurrency & " and then " & repeatAmt & " " & oEwProv.mcCurrency & " per " & repeatInterval & " by Credit/Debit Card"
                        End If

                        ccXform = oEwProv.creditCardXform(oOrder, "PayForm", sSubmitPath, oDictOpt("cardsAccepted"), bCv2, PaymentDescription, b3DSecure, , , bAllowSavePayment)

                        Dim sType As String = "Billing Address"
                        Dim oCartAdd As XmlElement = oOrder.SelectSingleNode("Contact[@type='" & sType & "']")

                        Dim ppRequestDetail As New Protean.PayPalAPI.DoDirectPaymentRequestDetailsType()
                        Dim ppRecuringRequestDetail As New Protean.PayPalAPI.CreateRecurringPaymentsProfileRequestDetailsType()



                        'only do this if we are not being redirected back
                        If ccXform.valid Then

                            'save some card details in session for later
                            myWeb.moSession("ExpireDate") = myWeb.moRequest("creditCard/expireDate")
                            myWeb.moSession("CardType") = myWeb.moRequest("creditCard/type")
                            myWeb.moSession("MaskedCard") = MaskString(myWeb.moRequest("creditCard/number"), "*", False, 4)

                            If b3DSecureV2 Then
                                Dim FirstName As String = ""
                                Dim MiddleName As String = ""
                                Dim LastName As String = ""
                                Dim aGivenName() As String = Split(oCartAdd.SelectSingleNode("GivenName").InnerText, " ")
                                Select Case UBound(aGivenName)
                                    Case 0
                                        LastName = aGivenName(0)
                                        FirstName = aGivenName(0)
                                    Case 1
                                        FirstName = aGivenName(0)
                                        LastName = aGivenName(1)
                                    Case 2
                                        FirstName = aGivenName(0)
                                        MiddleName = aGivenName(1)
                                        LastName = aGivenName(2)
                                    Case 3
                                        FirstName = aGivenName(0)
                                        MiddleName = aGivenName(1)
                                        LastName = aGivenName(2)
                                        'Suffix = aGivenName(3)
                                End Select

                                Dim Street1 As String = IIf(oCartAdd.SelectSingleNode("Company").InnerText = "", oCartAdd.SelectSingleNode("Street").InnerText, oCartAdd.SelectSingleNode("Company").InnerText)
                                Dim Street2 As String = ""
                                If oCartAdd.SelectSingleNode("Company").InnerText = "" Then
                                    Street2 = oCartAdd.SelectSingleNode("Street").InnerText
                                End If

                                Dim jwtPayload As Dictionary(Of String, Object) = New Dictionary(Of String, Object) From {
                                    {"Payload", New Dictionary(Of String, Object) From {
                                        {"Consumer", New Dictionary(Of String, Object) From {
                                            {"Email1", ""},
                                            {"Email2", ""},
                                            {"ShippingAddress", New Dictionary(Of String, Object) From {
                                                {"FullName", oCartAdd.SelectSingleNode("GivenName").InnerText},
                                                {"FirstName", FirstName},
                                                {"MiddleName", MiddleName},
                                                {"LastName", LastName},
                                                {"Address1", Street1},
                                                {"Address2", Street2},
                                                {"Address3", ""},
                                                {"City", oCartAdd.SelectSingleNode("City").InnerText},
                                                {"State", oCartAdd.SelectSingleNode("State").InnerText},
                                                {"PostalCode", oCartAdd.SelectSingleNode("PostalCode").InnerText},
                                                {"CountryCode", ""},
                                                {"Phone1", ""},
                                                {"Phone2", ""}
                                                }},
                                            {"BillingAddress", New Dictionary(Of String, Object) From {
                                                {"FullName", oCartAdd.SelectSingleNode("GivenName").InnerText},
                                                {"FirstName", FirstName},
                                                {"MiddleName", MiddleName},
                                                {"LastName", LastName},
                                                {"Address1", Street1},
                                                {"Address2", Street2},
                                                {"Address3", ""},
                                                {"City", oCartAdd.SelectSingleNode("City").InnerText},
                                                {"State", oCartAdd.SelectSingleNode("State").InnerText},
                                                {"PostalCode", oCartAdd.SelectSingleNode("PostalCode").InnerText},
                                                {"CountryCode", ""},
                                                {"Phone1", ""},
                                                {"Phone2", ""}
                                                }},
                                            {"Account", New Dictionary(Of String, Object) From {
                                                {"AccountNumber", myWeb.moRequest("creditCard/number").Trim},
                                                {"ExpirationMonth", CInt(Left(myWeb.moRequest("creditCard/expireDate"), 2))},
                                                {"ExpirationYear", CInt(Right(myWeb.moRequest("creditCard/expireDate"), 4))},
                                                {"CardCode", myWeb.moRequest("creditCard/CV2").Trim},
                                                {"NameOnAccount", oCartAdd.SelectSingleNode("GivenName").InnerText}
                                                }
                                            }}
                                        },
                                         {"OrderDetails", New Dictionary(Of String, Object) From {
                                            {"OrderNumber", CStr(oEwProv.mnCartId)},
                                            {"Amount", CStr(CInt(oEwProv.mnPaymentAmount * 100))},
                                            {"CurrencyCode", oDictOpt("centinalCurrencyCode")},
                                            {"TransactionId", CStr(oEwProv.mnCartId)}
                                            }
                                        }
                                    }}}



                                Dim jwtHelper As New Protean.Tools.IdentityModel.Tokens.JwtHelper
                                Dim sJwt As String = jwtHelper.GenerateJwt(
                                    oDictOpt("centinalAppKey"),
                                    oDictOpt("centinalAppId"),
                                    oDictOpt("OrgUnitId"),
                                    jwtPayload
                                )

                                Xform3dSec = oEwProv.xfrmSecure3Dv2(ACSUrl, sJwt, oDictOpt("SongbirdUrl"), sRedirectURL)

                            End If

                            If b3DSecure Then

                                'first we check if the card is enrolled

                                Dim strErrorNo As String = ""
                                Dim centinelRequest As CentinelRequest = New CentinelRequest()
                                Dim centinelResponse As CentinelResponse = New CentinelResponse()

                                centinelRequest.add("MsgType", "cmpi_lookup")
                                centinelRequest.add("Version", "1.7")

                                'centinelRequest.add("ProcessorId", "134-01")
                                'centinelRequest.add("MerchantId", "trevor@eonic.co.uk")
                                'centinelRequest.add("TransactionPwd", "soNIC14")

                                centinelRequest.add("ProcessorId", oDictOpt("centinalProcessorId"))
                                centinelRequest.add("MerchantId", oDictOpt("centinalMerchantId"))

                                If CStr(oDictOpt("centinalTransactionPwd") & "") <> "" Then
                                    centinelRequest.add("TransactionPwd", oDictOpt("centinalTransactionPwd"))
                                End If

                                centinelRequest.add("TransactionType", "C")
                                centinelRequest.add("Amount", CStr(CInt(oEwProv.mnPaymentAmount * 100))) 'in pence
                                centinelRequest.add("CurrencyCode", oDictOpt("centinalCurrencyCode"))
                                centinelRequest.add("CardNumber", myWeb.moRequest("creditCard/number").Trim)
                                centinelRequest.add("CardExpMonth", Left(myWeb.moRequest("creditCard/expireDate"), 2))
                                centinelRequest.add("CardExpYear", Right(myWeb.moRequest("creditCard/expireDate"), 4))
                                centinelRequest.add("OrderNumber", CStr(oEwProv.mnCartId))

                                Dim testRequest As String = centinelRequest.generatePayload(oDictOpt("centinalTransactionPwd"))

                                Try
                                    Dim centinelURL As String = "https://paypal.cardinalcommerce.com/maps/txns.asp"
                                    If nTransactionMode = TransactionMode.Test Then
                                        centinelURL = "https://centineltest.cardinalcommerce.com/maps/txns.asp"
                                    End If
                                    centinelResponse = centinelRequest.sendHTTP(centinelURL, CInt(oDictOpt("centinalTimeout")))

                                Catch ex As Exception
                                    strErrorNo = "9040"
                                    err_msg = "3D Secure Communication Error - 9040"
                                End Try

                                strErrorNo = centinelResponse.getValue("ErrorNo")
                                err_msg = centinelResponse.getValue("ErrorDesc")

                                If strErrorNo = "0" Then

                                    PAResStatus = centinelResponse.getValue("PAResStatus")
                                    Enrolled = centinelResponse.getValue("Enrolled")
                                    ACSUrl = centinelResponse.getValue("ACSUrl")
                                    Payload = centinelResponse.getValue("Payload")
                                    EciFlag = centinelResponse.getValue("EciFlag")
                                    CAVV = centinelResponse.getValue("Cavv")
                                    Xid = centinelResponse.getValue("Xid")

                                    If Enrolled = "Y" And Not (ACSUrl = "U" Or ACSUrl = "N") Then
                                        'Card is enrolled and interface is active.
                                        If oEwProv.moCartConfig("SecureURL").EndsWith("/") Then
                                            sRedirectURL = oEwProv.moCartConfig("SecureURL") & "?cartCmd=Redirect3ds"
                                        Else
                                            sRedirectURL = oEwProv.moCartConfig("SecureURL") & "/?cartCmd=Redirect3ds"
                                        End If
                                        Xform3dSec = oEwProv.xfrmSecure3D(ACSUrl, CStr(oEwProv.mnCartId), Payload, sRedirectURL)
                                    Else
                                        'Card is not enrolled continue as usual
                                        b3DAuthorised = True

                                    End If
                                Else
                                    ccXform.valid = False
                                    b3DAuthorised = False
                                End If

                                centinelRequest = Nothing
                                centinelResponse = Nothing
                            Else
                                b3DAuthorised = True
                            End If

                        End If

                        'create the payment request
                        If ccXform.valid Then
                            ''Setup the PayPal Payment Object.
                            Dim ppBillingAddress As Protean.PayPalAPI.AddressType = New Protean.PayPalAPI.AddressType()

                            ppBillingAddress.Street1 = IIf(oCartAdd.SelectSingleNode("Company").InnerText = "", oCartAdd.SelectSingleNode("Street").InnerText, oCartAdd.SelectSingleNode("Company").InnerText)
                            If oCartAdd.SelectSingleNode("Company").InnerText = "" Then
                                ppBillingAddress.Street2 = oCartAdd.SelectSingleNode("Street").InnerText
                            End If
                            ppBillingAddress.CityName = oCartAdd.SelectSingleNode("City").InnerText
                            ppBillingAddress.StateOrProvince = oCartAdd.SelectSingleNode("State").InnerText
                            ppBillingAddress.PostalCode = oCartAdd.SelectSingleNode("PostalCode").InnerText
                            ppBillingAddress.CountryName = oCartAdd.SelectSingleNode("Country").InnerText

                            Dim ppCardOwnerName As Protean.PayPalAPI.PersonNameType = New Protean.PayPalAPI.PersonNameType()
                            Dim aGivenName() As String = Split(oCartAdd.SelectSingleNode("GivenName").InnerText, " ")
                            Select Case UBound(aGivenName)
                                Case 0
                                    ppCardOwnerName.LastName = aGivenName(0)
                                    ppCardOwnerName.FirstName = aGivenName(0)
                                Case 1
                                    ppCardOwnerName.FirstName = aGivenName(0)
                                    ppCardOwnerName.LastName = aGivenName(1)
                                Case 2
                                    ppCardOwnerName.FirstName = aGivenName(0)
                                    ppCardOwnerName.MiddleName = aGivenName(1)
                                    ppCardOwnerName.LastName = aGivenName(2)
                                Case 3
                                    ppCardOwnerName.FirstName = aGivenName(0)
                                    ppCardOwnerName.MiddleName = aGivenName(1)
                                    ppCardOwnerName.LastName = aGivenName(2)
                                    ppCardOwnerName.Suffix = aGivenName(3)
                            End Select

                            Dim ppCardOwner As Protean.PayPalAPI.PayerInfoType = New Protean.PayPalAPI.PayerInfoType()
                            ppCardOwner.Address = ppBillingAddress
                            ppCardOwner.PayerName = ppCardOwnerName



                            Dim ppCreditCard As Protean.PayPalAPI.CreditCardDetailsType = New Protean.PayPalAPI.CreditCardDetailsType()

                            ppCreditCard.CardOwner = ppCardOwner

                            ppCreditCard.CVV2 = myWeb.moRequest("creditCard/CV2").Trim

                            ppCreditCard.ExpMonth = CInt(Left(myWeb.moRequest("creditCard/expireDate"), 2))
                            ppCreditCard.ExpYear = CInt(Right(myWeb.moRequest("creditCard/expireDate"), 4))
                            ppCreditCard.ExpMonthSpecified = True
                            ppCreditCard.ExpYearSpecified = True
                            ppCreditCard.CreditCardNumber = myWeb.moRequest("creditCard/number").Trim

                            Select Case UCase(myWeb.moRequest("creditCard/type"))
                                Case "AMEX", "AMERICAN EXPRESS"
                                    ppCreditCard.CreditCardType = PayPalAPI.CreditCardTypeType.Amex
                                Case "DISCOVER"
                                    ppCreditCard.CreditCardType = PayPalAPI.CreditCardTypeType.Discover
                                Case "MASTERCARD", "MASTER CARD"
                                    ppCreditCard.CreditCardType = PayPalAPI.CreditCardTypeType.MasterCard
                                Case "SOLO"
                                    ppCreditCard.CreditCardType = PayPalAPI.CreditCardTypeType.Solo
                                Case "SWITCH",
                                        ppCreditCard.CreditCardType = PayPalAPI.CreditCardTypeType.Switch
                                Case "MAESTRO"
                                    ppCreditCard.CreditCardType = PayPalAPI.CreditCardTypeType.Maestro
                                    'For Maestro
                                    ppCreditCard.StartMonth = CInt(Left(myWeb.moRequest("creditCard/issueDate"), 2))
                                    ppCreditCard.StartYear = CInt(Right(myWeb.moRequest("creditCard/issueDate"), 4))
                                    ppCreditCard.IssueNumber = CInt(myWeb.moRequest("creditCard/issueNumber"))
                                Case "VISA"
                                    ppCreditCard.CreditCardType = PayPalAPI.CreditCardTypeType.Visa
                            End Select

                            If b3DSecure Then
                                Dim pp3DsecReq As New Protean.PayPalAPI.ThreeDSecureRequestType()
                                pp3DsecReq.Xid = Xid
                                pp3DsecReq.MpiVendor3ds = Enrolled
                                pp3DsecReq.Cavv = CAVV
                                pp3DsecReq.Eci3ds = EciFlag
                                pp3DsecReq.AuthStatus3ds = PAResStatus
                                ppCreditCard.ThreeDSecureRequest = pp3DsecReq
                            End If

                            '' ###Details
                            '' Let's you specify details of a payment amount.
                            Dim ppDetails As New Protean.PayPalAPI.PaymentDetailsType()

                            '' ###Amount
                            '' Let's you specify a payment amount.
                            Dim ppAmount As New Protean.PayPalAPI.BasicAmountType()
                            Select Case UCase(oEwProv.mcCurrency)
                                Case "EUR"
                                    ppAmount.currencyID = Protean.PayPalAPI.CurrencyCodeType.EUR
                                Case "USD"
                                    ppAmount.currencyID = Protean.PayPalAPI.CurrencyCodeType.USD
                                Case "GBP"
                                    ppAmount.currencyID = Protean.PayPalAPI.CurrencyCodeType.GBP
                                Case Else
                                    ppAmount.currencyID = Protean.PayPalAPI.CurrencyCodeType.GBP
                            End Select

                            ' Total must be equal to sum of shipping, tax and subtotal.
                            ppAmount.Value = oEwProv.mnPaymentAmount

                            ppDetails.OrderTotal = ppAmount
                            ppDetails.InvoiceID = oCart.moCartConfig("OrderNoPrefix") & CStr(oEwProv.mnCartId)
                            ppDetails.PaymentAction = PayPalAPI.PaymentActionCodeType.Sale

                            ppRequestDetail.CreditCard = ppCreditCard
                            ppRequestDetail.PaymentDetails = ppDetails
                            ppRequestDetail.IPAddress = IIf(myWeb.moRequest.ServerVariables("REMOTE_ADDR") = "1", "88.97.12.224", myWeb.moRequest.ServerVariables("REMOTE_ADDR"))
                            ppRequestDetail.MerchantSessionId = myWeb.moSession.SessionID
                            ppRequestDetail.PaymentAction = PayPalAPI.PaymentActionCodeType.Sale

                            If repeatAmt > 0 Then
                                Dim ppScheduleDetails As New PayPalAPI.ScheduleDetailsType()
                                Dim ppPaymentPeriod As New PayPalAPI.BillingPeriodDetailsType()
                                Dim ppRepeatAmount As New Protean.PayPalAPI.BasicAmountType()
                                Select Case UCase(oEwProv.mcCurrency)
                                    Case "EUR"
                                        ppRepeatAmount.currencyID = Protean.PayPalAPI.CurrencyCodeType.EUR
                                    Case "USD"
                                        ppRepeatAmount.currencyID = Protean.PayPalAPI.CurrencyCodeType.USD
                                    Case "GBP"
                                        ppRepeatAmount.currencyID = Protean.PayPalAPI.CurrencyCodeType.GBP
                                    Case Else
                                        ppRepeatAmount.currencyID = Protean.PayPalAPI.CurrencyCodeType.GBP
                                End Select

                                ppRepeatAmount.Value = repeatAmt

                                ppScheduleDetails.AutoBillOutstandingAmount = PayPalAPI.AutoBillType.AddToNextBilling
                                ppPaymentPeriod.Amount = ppRepeatAmount
                                ppPaymentPeriod.BillingFrequency = repeatFrequency
                                Select Case repeatInterval
                                    Case "day"
                                        ppPaymentPeriod.BillingPeriod = PayPalAPI.BillingPeriodType.Day
                                    Case "week"
                                        ppPaymentPeriod.BillingPeriod = PayPalAPI.BillingPeriodType.Week
                                    Case "month"
                                        ppPaymentPeriod.BillingPeriod = PayPalAPI.BillingPeriodType.Month
                                    Case "year"
                                        ppPaymentPeriod.BillingPeriod = PayPalAPI.BillingPeriodType.Year
                                End Select
                                ppPaymentPeriod.TotalBillingCyclesSpecified = False
                                ppScheduleDetails.PaymentPeriod = ppPaymentPeriod
                                ppScheduleDetails.MaxFailedPaymentsSpecified = False
                                ppScheduleDetails.Description = oCart.moCartConfig("OrderNoPrefix") & CStr(oEwProv.mnCartId) & " - " & oCartAdd.SelectSingleNode("GivenName").InnerText

                                If oEwProv.mnPaymentAmount > 0 Then
                                    ppScheduleDetails.ActivationDetails = New PayPalAPI.ActivationDetailsType()
                                    ppScheduleDetails.ActivationDetails.InitialAmount = ppAmount
                                    ppScheduleDetails.ActivationDetails.FailedInitialAmountAction = PayPalAPI.FailedPaymentActionType.CancelOnFailure
                                Else
                                    bValidateFirst = True
                                End If

                                Dim ppRecuringProfileDetails As New PayPalAPI.RecurringPaymentsProfileDetailsType()

                                If delayStart Then
                                    Select Case repeatInterval
                                        Case "day"
                                            startDate = DateAdd(DateInterval.Day, repeatFrequency, Now())
                                        Case "week"
                                            startDate = DateAdd(DateInterval.WeekOfYear, repeatFrequency, Now())
                                        Case "month"
                                            startDate = DateAdd(DateInterval.Month, repeatFrequency, Now())
                                        Case "year"
                                            startDate = DateAdd(DateInterval.Year, repeatFrequency, Now())
                                    End Select
                                End If

                                ppRecuringProfileDetails.BillingStartDate = startDate
                                ppRecuringProfileDetails.SubscriberName = ppCardOwnerName.FirstName & " " & ppCardOwnerName.LastName
                                ppRecuringProfileDetails.SubscriberShippingAddress = ppBillingAddress
                                ppRecuringProfileDetails.ProfileReference = ""

                                ppRecuringRequestDetail.CreditCard = ppCreditCard
                                ppRecuringRequestDetail.ScheduleDetails = ppScheduleDetails
                                ppRecuringRequestDetail.RecurringPaymentsProfileDetails = ppRecuringProfileDetails

                            End If
                        End If

                        If b3DSecure Then
                            'check for return from aquiring bank
                            If myWeb.moRequest("MD") <> "" Then
                                b3DAuthorised = True
                                ppRequestDetail = myWeb.moSession("ppPayment")
                            Else
                                myWeb.moSession("ppPayment") = ppRequestDetail
                            End If
                        Else
                            If ccXform.valid Then
                                '3DSec turned off so force authorised logic
                                b3DAuthorised = True
                            End If
                        End If

                        If ccXform.valid Or b3DAuthorised Then

                            If b3DAuthorised Then

                                Try

                                    Dim ppProfile As New PayPalAPI.CustomSecurityHeaderType
                                    ppProfile.Credentials = New PayPalAPI.UserIdPasswordType
                                    ppProfile.Credentials.Username = oDictOpt("apiUsername")
                                    ppProfile.Credentials.Password = oDictOpt("apiPassword")
                                    ppProfile.Credentials.Signature = oDictOpt("apiSignature")

                                    'Create a service Binding in code

                                    Dim endpointAddress As String = "https://api-3t.paypal.com/2.0/"
                                    If nTransactionMode = TransactionMode.Test Then
                                        endpointAddress = "https://api-3t.sandbox.paypal.com/2.0/"
                                    End If
                                    Dim ppEndpointAddress As New System.ServiceModel.EndpointAddress(endpointAddress)

                                    Dim ppBinding As System.ServiceModel.BasicHttpBinding = getBinding()

                                    'Take the fixed payment

                                    If repeatAmt > 0 Then
                                        'Setup the recurring payment
                                        Dim ppIface As New PayPalAPI.PayPalAPIAAInterfaceClient(ppBinding, ppEndpointAddress)
                                        Dim AuthSuccess As Boolean = True

                                        If bValidateFirst Then
                                            'code to validate the credit card
                                            Dim ppIfaceAuth As New PayPalAPI.PayPalAPIAAInterfaceClient(ppBinding, ppEndpointAddress)

                                            Dim ppAuthRequest As New Protean.PayPalAPI.DoDirectPaymentRequestType()
                                            ppAuthRequest.Version = "51.0"
                                            ppAuthRequest.DoDirectPaymentRequestDetails = ppRequestDetail

                                            ppRequestDetail.PaymentAction = PayPalAPI.PaymentActionCodeType.Authorization

                                            Dim ppAuthPaymentReq As New PayPalAPI.DoDirectPaymentReq()

                                            ppAuthPaymentReq.DoDirectPaymentRequest = ppAuthRequest

                                            Dim ppAuthPaymentResponse As PayPalAPI.DoDirectPaymentResponseType

                                            ppAuthPaymentResponse = ppIfaceAuth.DoDirectPayment(ppProfile, ppAuthPaymentReq)

                                            Dim ppAuthPaymentStatus As PayPalAPI.PaymentStatusCodeType = ppAuthPaymentResponse.PaymentStatus

                                            If ppAuthPaymentResponse.Ack = PayPalAPI.AckCodeType.SuccessWithWarning Then
                                                AuthSuccess = True
                                            Else
                                                'case for auth failure
                                                bIsValid = False
                                                ccXform.valid = False

                                                cProcessInfo = ppAuthPaymentStatus.ToString

                                                If ppAuthPaymentResponse.Errors Is Nothing Then
                                                    err_msg = "Response Not handled " & cProcessInfo
                                                Else
                                                    Try
                                                        Dim ppErrors As PayPalAPI.ErrorType() = ppAuthPaymentResponse.Errors
                                                        Dim ppError As PayPalAPI.ErrorType

                                                        For Each ppError In ppErrors
                                                            err_msg = err_msg & ppError.ErrorCode & " - " & ppError.ShortMessage & " - " & ppError.LongMessage
                                                        Next
                                                    Catch ex As Exception
                                                        err_msg = "Response Not handled " & cProcessInfo
                                                    End Try
                                                End If
                                            End If
                                        End If

                                        If AuthSuccess Then

                                            Dim ppRecuringRequest As New Protean.PayPalAPI.CreateRecurringPaymentsProfileRequestType()
                                            ppRecuringRequest.Version = "51.0"
                                            ppRecuringRequest.CreateRecurringPaymentsProfileRequestDetails = ppRecuringRequestDetail

                                            Dim ppRecuringReq As New PayPalAPI.CreateRecurringPaymentsProfileReq

                                            ppRecuringReq.CreateRecurringPaymentsProfileRequest = ppRecuringRequest

                                            Dim ppRecuringResponse As PayPalAPI.CreateRecurringPaymentsProfileResponseType

                                            Dim ppProfile2 As New PayPalAPI.CustomSecurityHeaderType
                                            ppProfile2.Credentials = New PayPalAPI.UserIdPasswordType
                                            ppProfile2.Credentials.Username = oDictOpt("apiUsername")
                                            ppProfile2.Credentials.Password = oDictOpt("apiPassword")
                                            ppProfile2.Credentials.Signature = oDictOpt("apiSignature")

                                            ppRecuringResponse = ppIface.CreateRecurringPaymentsProfile(ppProfile2, ppRecuringReq)

                                            Dim ppRecuringResponseDetails As PayPalAPI.CreateRecurringPaymentsProfileResponseDetailsType
                                            ppRecuringResponseDetails = ppRecuringResponse.CreateRecurringPaymentsProfileResponseDetails

                                            Dim ppPaymentStatus As PayPalAPI.PaymentStatusCodeType = ppRecuringResponseDetails.ProfileStatus

                                            Select Case ppPaymentStatus
                                                Case PayPalAPI.PaymentStatusCodeType.Completed, PayPalAPI.PaymentStatusCodeType.Processed, PayPalAPI.PaymentStatusCodeType.None

                                                    cAuthCode = ppRecuringResponseDetails.ProfileID
                                                    If cAuthCode <> "" Then
                                                        bIsValid = True
                                                        PaymemtRef = CStr(ppRecuringResponseDetails.ProfileID)
                                                        err_msg = "Repeat Payment Authorised Ref " & CStr(ppRecuringResponseDetails.ProfileID)
                                                        ccXform.valid = True
                                                        bSavePayment = True
                                                    Else
                                                        bIsValid = False
                                                        ccXform.valid = False
                                                        Dim ppErrors As PayPalAPI.ErrorType() = ppRecuringResponse.Errors
                                                        Dim ppError As PayPalAPI.ErrorType

                                                        For Each ppError In ppErrors
                                                            err_msg = err_msg & ppError.LongMessage
                                                        Next
                                                    End If

                                                Case PayPalAPI.PaymentStatusCodeType.Failed, PayPalAPI.PaymentStatusCodeType.Expired

                                                    bIsValid = False
                                                    ccXform.valid = False
                                                    Dim ppErrors As PayPalAPI.ErrorType() = ppRecuringResponse.Errors
                                                    Dim ppError As PayPalAPI.ErrorType

                                                    For Each ppError In ppErrors
                                                        err_msg = err_msg & ppError.LongMessage
                                                    Next

                                                Case Else

                                                    bIsValid = False
                                                    ccXform.valid = False

                                                    cProcessInfo = ppPaymentStatus.ToString

                                                    If ppRecuringResponse.Errors Is Nothing Then
                                                        err_msg = "Response Not handled " & cProcessInfo
                                                    Else
                                                        Try
                                                            Dim ppErrors As PayPalAPI.ErrorType() = ppRecuringResponse.Errors
                                                            Dim ppError As PayPalAPI.ErrorType

                                                            For Each ppError In ppErrors
                                                                err_msg = err_msg & ppError.ErrorCode & " - " & ppError.ShortMessage & " - " & ppError.LongMessage
                                                            Next
                                                        Catch ex As Exception
                                                            err_msg = "Response Not handled " & cProcessInfo
                                                        End Try
                                                    End If

                                            End Select
                                        End If


                                    Else
                                        'Try a standard payment
                                        If oEwProv.mnPaymentAmount > 0 And err_msg = "" Then

                                            Dim ppIface2 As New PayPalAPI.PayPalAPIAAInterfaceClient(ppBinding, ppEndpointAddress)

                                            Dim ppRequest As New Protean.PayPalAPI.DoDirectPaymentRequestType()
                                            ppRequest.Version = "51.0"
                                            ppRequest.DoDirectPaymentRequestDetails = ppRequestDetail

                                            Dim ppPaymentReq As New PayPalAPI.DoDirectPaymentReq()

                                            ppPaymentReq.DoDirectPaymentRequest = ppRequest

                                            Dim ppPaymentResponse As PayPalAPI.DoDirectPaymentResponseType

                                            ppPaymentResponse = ppIface2.DoDirectPayment(ppProfile, ppPaymentReq)

                                            Dim ppPaymentStatus As PayPalAPI.PaymentStatusCodeType = ppPaymentResponse.PaymentStatus

                                            Dim responseState As String = ""

                                            Select Case ppPaymentStatus
                                                Case PayPalAPI.PaymentStatusCodeType.Completed, PayPalAPI.PaymentStatusCodeType.Processed, PayPalAPI.PaymentStatusCodeType.None

                                                    cAuthCode = ppPaymentResponse.TransactionID
                                                    If cAuthCode <> "" Then
                                                        bIsValid = True
                                                        PaymemtRef = CStr(ppPaymentResponse.TransactionID)
                                                        err_msg = "Payment Authorised Ref " & CStr(ppPaymentResponse.TransactionID)
                                                        ccXform.valid = True
                                                        bSavePayment = True
                                                    Else
                                                        bIsValid = False
                                                        ccXform.valid = False
                                                        Dim ppErrors As PayPalAPI.ErrorType() = ppPaymentResponse.Errors
                                                        Dim ppError As PayPalAPI.ErrorType

                                                        For Each ppError In ppErrors
                                                            err_msg = err_msg & ppError.ShortMessage & " - " & ppError.ErrorCode & " " & ppError.LongMessage
                                                        Next
                                                    End If

                                                Case PayPalAPI.PaymentStatusCodeType.Pending

                                                    cAuthCode = ppPaymentResponse.TransactionID
                                                    bIsValid = True
                                                    err_msg = "Payment Pending - Reason " & ppPaymentResponse.PendingReason.ToString & " - " & CStr(ppPaymentResponse.TransactionID)
                                                    ccXform.valid = True
                                                    bSavePayment = True

                                                Case PayPalAPI.PaymentStatusCodeType.Failed, PayPalAPI.PaymentStatusCodeType.Expired

                                                    bIsValid = False
                                                    ccXform.valid = False
                                                    Dim ppErrors As PayPalAPI.ErrorType() = ppPaymentResponse.Errors
                                                    Dim ppError As PayPalAPI.ErrorType

                                                    For Each ppError In ppErrors
                                                        err_msg = err_msg & ppError.LongMessage
                                                    Next

                                                Case Else

                                                    bIsValid = False
                                                    ccXform.valid = False

                                                    cProcessInfo = ppPaymentResponse.PaymentStatus.ToString

                                                    If ppPaymentResponse.Errors Is Nothing Then
                                                        err_msg = "Response Not handled " & cProcessInfo
                                                    Else
                                                        Try
                                                            Dim ppErrors As PayPalAPI.ErrorType() = ppPaymentResponse.Errors
                                                            Dim ppError As PayPalAPI.ErrorType

                                                            For Each ppError In ppErrors
                                                                err_msg = err_msg & ppError.ErrorCode & " - " & ppError.ShortMessage & " - " & ppError.LongMessage
                                                            Next
                                                        Catch ex As Exception
                                                            err_msg = "Response Not handled: " & cProcessInfo
                                                        End Try
                                                    End If

                                            End Select
                                        End If

                                    End If




                                Catch ex As Exception
                                    bIsValid = False
                                    ccXform.valid = False
                                    'Dim ppError As ErrorType

                                    err_msg = err_msg & " Error:" & ex.Message
                                    err_msg = err_msg & " Msg:" & ex.StackTrace

                                End Try
                            Else
                                ccXform.valid = False
                            End If
                            'bSavePayment = True
                        End If

                        If bSavePayment Then
                            'We Save the payment method prior to ultimate validation because we only have access to the request object at the point it is submitted

                            'only do this for a valid payment method
                            Dim oSaveElmt As XmlElement = ccXform.Instance.SelectSingleNode("creditCard/bSavePayment")
                            Debug.WriteLine(myWeb.moRequest("creditCard/expireDate"))
                            Dim oDate As New Date("20" & Right(myWeb.moSession("ExpireDate"), 2), CInt(Left(myWeb.moSession("ExpireDate"), 2)), 1)
                            oDate = oDate.AddMonths(1)
                            oDate = oDate.AddDays(-1)
                            Dim cMethodName As String = myWeb.moSession("CardType") & ": " & myWeb.moSession("MaskedCard") & " Expires: " & oDate.ToShortDateString
                            ' cAuthCode = oDictResp("auth_code")

                            Dim oPayPalElmt As XmlElement = ccXform.Instance.OwnerDocument.CreateElement("PayPal")
                            oPayPalElmt.SetAttribute("AuthCode", cAuthCode)
                            ccXform.Instance.FirstChild.AppendChild(oPayPalElmt)

                            If Not oSaveElmt Is Nothing Then
                                If oSaveElmt.InnerText = "true" And bIsValid Then
                                    'oCart.mnPaymentId = oEwProv.savePayment(myWeb.mnUserId, "PayPalPro", oEwProv.mnCartId, cMethodName, ccXform.Instance.FirstChild, oDate, True, oEwProv.mnPaymentAmount)
                                    oCart.mnPaymentId = myWeb.moDbHelper.savePayment(oCart.mnCartId, myWeb.mnUserId, "PayPalPro", cAuthCode, "Credit Card", ccXform.Instance.FirstChild, Now, False, oEwProv.mnPaymentAmount) '0 amount paid as yet
                                Else
                                    ' oCart.mnPaymentId = oEwProv.savePayment(myWeb.mnUserId, "PayPalPro", oEwProv.mnCartId, cMethodName, ccXform.Instance.FirstChild, Now, False, oEwProv.mnPaymentAmount)
                                    oCart.mnPaymentId = myWeb.moDbHelper.savePayment(oCart.mnCartId, myWeb.mnUserId, "PayPalPro", cAuthCode, "Credit Card", ccXform.Instance.FirstChild, Now, False, oEwProv.mnPaymentAmount) '0 amount paid as yet
                                End If
                            Else
                                ' oCart.mnPaymentId = oEwProv.savePayment(myWeb.mnUserId, "PayPalPro", oEwProv.mnCartId, cMethodName, ccXform.Instance.FirstChild, Now, False, oEwProv.mnPaymentAmount)
                                oCart.mnPaymentId = myWeb.moDbHelper.savePayment(oCart.mnCartId, myWeb.mnUserId, "PayPalPro", cAuthCode, "Credit Card", ccXform.Instance.FirstChild, Now, False, oEwProv.mnPaymentAmount) '0 amount paid as yet

                            End If

                            myWeb.moSession("ExpireDate") = Nothing
                            myWeb.moSession("CardType") = Nothing
                            myWeb.moSession("MaskedCard") = Nothing

                        End If

                        If err_msg <> "" Then
                            ccXform.addNote(ccXform.moXformElmt, xForm.noteTypes.Alert, err_msg)
                        End If

                        'Update Seller Notes:

                        sSql = "select * from tblCartOrder where nCartOrderKey = " & oEwProv.mnCartId
                        Dim oDs As DataSet
                        Dim oRow As DataRow
                        oDs = myWeb.moDbHelper.getDataSetForUpdate(sSql, "Order", "Cart")
                        For Each oRow In oDs.Tables("Order").Rows
                            If bIsValid Then
                                oRow("cSellerNotes") = oRow("cSellerNotes") & vbLf & Today & " " & TimeOfDay & ": changed to: (Payment Received) " & vbLf & "comment: " & err_msg & vbLf & "Full Response:' " & cResponse & "'"
                            Else
                                If err_msg_log = "" Then err_msg_log = err_msg
                                oRow("cSellerNotes") = oRow("cSellerNotes") & vbLf & Today & " " & TimeOfDay & ": changed to: (Payment Failed) " & vbLf & "comment: " & err_msg_log & vbLf & "Full Response:' " & cResponse & "'"
                            End If
                            If b3DSecure And bIsValid = False Then
                                oRow("cPaymentRef") = sPaymentRef
                            End If
                        Next

                        myWeb.moDbHelper.updateDataset(oDs, "Order")

                        If Not Xform3dSec Is Nothing Then

                            'Save the payment object into the session
                            Return Xform3dSec
                        Else
                            Return ccXform
                        End If

                    Catch ex As Exception
                        returnException(mcModuleName, "GetPaymentForm", ex, "", cProcessInfo, gbDebug)
                        Return Nothing
                    End Try
                End Function

                Function xfrmSecure3DReturn(ByVal acs_url As String) As xForm
                    PerfMon.Log("EPDQ", "xfrmSecure3DReturn")
                    Dim oXform As xForm = New Protean.Cms.xForm
                    Dim oFrmInstance As XmlElement
                    Dim oFrmGroup As XmlElement

                    Dim cProcessInfo As String = "xfrmSecure3D"
                    Try
                        oXform.moPageXML = myWeb.moPageXml

                        'create the instance
                        oXform.NewFrm("Secure3DReturn")
                        oXform.submission("Secure3DReturn", goServer.UrlDecode(acs_url), "POST", "return form_check(this);")
                        oFrmInstance = oXform.moPageXML.CreateElement("Secure3DReturn")
                        oXform.Instance.AppendChild(oFrmInstance)
                        oFrmGroup = oXform.addGroup(oXform.moXformElmt, "Secure3DReturn1", "Secure3DReturn1", "Redirect to 3D Secure")
                        'build the form and the binds
                        'oXform.addDiv(oFrmGroup, "<SCRIPT LANGUAGE=""Javascript"">function onXformLoad(){document.Secure3DReturn.submit();};appendLoader(onXformLoad);</SCRIPT>")
                        oXform.addSubmit(oFrmGroup, "Secure3DReturn", "Show Invoice", "ewSubmit")
                        oXform.addValues()
                        Return oXform

                    Catch ex As Exception
                        returnException(mcModuleName, "xfrmSecure3D", ex, "", cProcessInfo, gbDebug)
                        Return Nothing
                    End Try

                End Function

                Public Function getCountryISONum(ByRef sCountry As String) As String
                    PerfMon.Log(mcModuleName, "getCountryISONum")
                    Dim oDr As SqlDataReader
                    Dim sSql As String
                    Dim strReturn As String = ""
                    Dim cProcessInfo As String = "getCountryISONum"
                    Try

                        sSql = "select cLocationISOnum from tblCartShippingLocations where cLocationNameFull Like '" & sCountry & "' or cLocationNameShort Like '" & sCountry & "'"
                        oDr = myWeb.moDbHelper.getDataReader(sSql)
                        If oDr.HasRows Then
                            While oDr.Read
                                strReturn = oDr("cLocationISOnum")
                            End While
                        Else
                            strReturn = ""
                        End If

                        oDr.Close()
                        oDr = Nothing
                        Return strReturn
                    Catch ex As Exception
                        returnException(mcModuleName, "getCountryISOnum", ex, "", cProcessInfo, gbDebug)
                        Return Nothing
                    End Try
                End Function


                Public Function CheckStatus(ByRef oWeb As Protean.Cms, ByRef nPaymentProviderRef As String) As String
                    Dim cProcessInfo As String = ""

                    Dim oPayPalProCfg As XmlNode
                    Dim nTransactionMode As TransactionMode

                    Try
                        moPaymentCfg = WebConfigurationManager.GetWebApplicationSection("protean/payment")
                        oPayPalProCfg = moPaymentCfg.SelectSingleNode("provider[@name='PayPalPro']")

                        Dim ppProfile As New PayPalAPI.CustomSecurityHeaderType
                        ppProfile.Credentials = New PayPalAPI.UserIdPasswordType
                        ppProfile.Credentials.Username = oPayPalProCfg.SelectSingleNode("apiUsername").Attributes("value").Value()
                        ppProfile.Credentials.Password = oPayPalProCfg.SelectSingleNode("apiPassword").Attributes("value").Value()
                        ppProfile.Credentials.Signature = oPayPalProCfg.SelectSingleNode("apiSignature").Attributes("value").Value()


                        Select Case CStr(oPayPalProCfg.SelectSingleNode("opperationMode").Attributes("value").Value())
                            Case "true", "test"
                                nTransactionMode = TransactionMode.Test
                            Case "false"
                                nTransactionMode = TransactionMode.Fail
                            Case "live"
                                nTransactionMode = TransactionMode.Live
                        End Select

                        'Create a service Binding in code

                        Dim endpointAddress As String = "https://api-3t.paypal.com/2.0/"
                        If nTransactionMode = TransactionMode.Test Then
                            endpointAddress = "https://api-3t.sandbox.paypal.com/2.0/"
                        End If
                        Dim ppEndpointAddress As New System.ServiceModel.EndpointAddress(endpointAddress)

                        Dim ppBinding As System.ServiceModel.BasicHttpBinding = getBinding()

                        Dim ppGetRecurringPaymentsProfileDetailsReq As New PayPalAPI.GetRecurringPaymentsProfileDetailsReq

                        Dim ppGetRecurringPaymentsProfileDetailsRequestType As New PayPalAPI.GetRecurringPaymentsProfileDetailsRequestType
                        ppGetRecurringPaymentsProfileDetailsRequestType.ProfileID = nPaymentProviderRef
                        ppGetRecurringPaymentsProfileDetailsRequestType.Version = "51.0"

                        ppGetRecurringPaymentsProfileDetailsReq.GetRecurringPaymentsProfileDetailsRequest = ppGetRecurringPaymentsProfileDetailsRequestType

                        Dim ppIface As New PayPalAPI.PayPalAPIAAInterfaceClient(ppBinding, ppEndpointAddress)
                        Dim ppRecuringResponse As PayPalAPI.GetRecurringPaymentsProfileDetailsResponseType
                        Try
                            ppRecuringResponse = ppIface.GetRecurringPaymentsProfileDetails(ppProfile, ppGetRecurringPaymentsProfileDetailsReq)

                            Select Case ppRecuringResponse.GetRecurringPaymentsProfileDetailsResponseDetails.ProfileStatus
                                Case PayPalAPI.RecurringPaymentsProfileStatusType.ActiveProfile
                                    Return "active"
                                Case PayPalAPI.RecurringPaymentsProfileStatusType.CancelledProfile
                                    Return "cancelled"
                                Case PayPalAPI.RecurringPaymentsProfileStatusType.ExpiredProfile
                                    Return "expired"
                                Case PayPalAPI.RecurringPaymentsProfileStatusType.PendingProfile
                                    Return "pending"
                                Case PayPalAPI.RecurringPaymentsProfileStatusType.SuspendedProfile
                                    Return "suspended"
                                Case Else
                                    Return "Error - no value returned"
                            End Select
                        Catch ex As Exception
                            Return "Error - no value returned " & ex.Message
                        End Try


                    Catch ex As Exception
                        returnException(mcModuleName, "CheckStatus", ex, "", cProcessInfo, gbDebug)
                        Return ""
                    End Try

                End Function

                Public Function CancelPayments(ByRef oWeb As Protean.Cms, ByRef nPaymentProviderRef As String) As String
                    Dim cProcessInfo As String = ""
                    Dim oPayPalProCfg As XmlNode

                    Try

                        moPaymentCfg = WebConfigurationManager.GetWebApplicationSection("protean/payment")

                        oPayPalProCfg = moPaymentCfg.SelectSingleNode("provider[@name='PayPalPro']")

                        Dim ppProfile As New PayPalAPI.CustomSecurityHeaderType
                        ppProfile.Credentials = New PayPalAPI.UserIdPasswordType
                        ppProfile.Credentials.Username = oPayPalProCfg.SelectSingleNode("apiUsername").Attributes("value").Value()
                        ppProfile.Credentials.Password = oPayPalProCfg.SelectSingleNode("apiPassword").Attributes("value").Value()
                        ppProfile.Credentials.Signature = oPayPalProCfg.SelectSingleNode("apiSignature").Attributes("value").Value()


                        Select Case CStr(oPayPalProCfg.SelectSingleNode("opperationMode").Attributes("value").Value())
                            Case "true", "test"
                                nTransactionMode = TransactionMode.Test
                            Case "false"
                                nTransactionMode = TransactionMode.Fail
                            Case "live"
                                nTransactionMode = TransactionMode.Live
                        End Select

                        'Create a service Binding in code

                        Dim endpointAddress As String = "https://api-3t.paypal.com/2.0/"
                        If nTransactionMode = TransactionMode.Test Then
                            endpointAddress = "https://api-3t.sandbox.paypal.com/2.0/"
                        End If
                        Dim ppEndpointAddress As New System.ServiceModel.EndpointAddress(endpointAddress)

                        Dim ppBinding As System.ServiceModel.BasicHttpBinding = getBinding()

                        Dim ppManageRecurringPaymentsProfileStatusReq As New PayPalAPI.ManageRecurringPaymentsProfileStatusReq

                        Dim ppManageRecurringPaymentsProfileStatusRequestType As New PayPalAPI.ManageRecurringPaymentsProfileStatusRequestType
                        ppManageRecurringPaymentsProfileStatusReq.ManageRecurringPaymentsProfileStatusRequest = ppManageRecurringPaymentsProfileStatusRequestType
                        ppManageRecurringPaymentsProfileStatusRequestType.Version = "51.0"

                        Dim ppManageRecurringPaymentsProfileStatusRequestDetailsType As New PayPalAPI.ManageRecurringPaymentsProfileStatusRequestDetailsType
                        ppManageRecurringPaymentsProfileStatusRequestType.ManageRecurringPaymentsProfileStatusRequestDetails = ppManageRecurringPaymentsProfileStatusRequestDetailsType

                        ppManageRecurringPaymentsProfileStatusRequestDetailsType.ProfileID = nPaymentProviderRef
                        ppManageRecurringPaymentsProfileStatusRequestDetailsType.Action = PayPalAPI.StatusChangeActionType.Cancel


                        Dim ppIface As New PayPalAPI.PayPalAPIAAInterfaceClient(ppBinding, ppEndpointAddress)
                        Dim ppRecuringResponse As PayPalAPI.ManageRecurringPaymentsProfileStatusResponseType

                        ppRecuringResponse = ppIface.ManageRecurringPaymentsProfileStatus(ppProfile, ppManageRecurringPaymentsProfileStatusReq)

                        Return CheckStatus(oWeb, nPaymentProviderRef)

                    Catch ex As Exception
                        returnException(mcModuleName, "CancelPayments", ex, "", cProcessInfo, gbDebug)
                        Return ""
                    End Try

                End Function

                Public Function AddPaymentButton(ByRef oOptXform As xForm, ByRef oFrmElmt As XmlElement, ByVal configXml As XmlElement, ByVal nPaymentAmount As Double, ByVal submissionValue As String, ByVal refValue As String) As Boolean

                    Dim PaymentLabel As String = configXml.SelectSingleNode("description/@value").InnerText
                    'allow html in description node...
                    Dim bXmlLabel As Boolean = False

                    If configXml.SelectSingleNode("description").InnerXml <> "" Then
                        PaymentLabel = configXml.SelectSingleNode("description").InnerXml
                        bXmlLabel = True
                    End If

                    Dim iconclass As String = ""
                    If Not configXml.SelectSingleNode("icon/@value") Is Nothing Then
                        iconclass = configXml.SelectSingleNode("icon/@value").InnerText
                    End If

                    oOptXform.addSubmit(oFrmElmt, submissionValue, PaymentLabel, refValue, "pay-button pay-" & configXml.GetAttribute("name"), iconclass, configXml.GetAttribute("name"))

                End Function

                Public Function getBinding() As System.ServiceModel.BasicHttpBinding

                    Dim ppBinding As New System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.Transport)
                    ppBinding.MaxBufferSize = 65536
                    ppBinding.CloseTimeout = New System.TimeSpan(0, 2, 0)
                    ppBinding.OpenTimeout = New System.TimeSpan(0, 2, 0)
                    ppBinding.ReceiveTimeout = New System.TimeSpan(0, 10, 0)
                    ppBinding.SendTimeout = New System.TimeSpan(0, 2, 0)
                    ppBinding.AllowCookies = False
                    ppBinding.BypassProxyOnLocal = False
                    ppBinding.HostNameComparisonMode = ServiceModel.HostNameComparisonMode.StrongWildcard
                    ppBinding.MaxBufferSize = 65536
                    ppBinding.MaxBufferPoolSize = 524288
                    ppBinding.MaxReceivedMessageSize = 65536
                    ppBinding.MessageEncoding = ServiceModel.WSMessageEncoding.Text
                    ppBinding.TextEncoding = System.Text.Encoding.UTF8
                    ppBinding.TransferMode = ServiceModel.TransferMode.Buffered
                    ppBinding.UseDefaultWebProxy = True

                    ppBinding.ReaderQuotas.MaxDepth = 32
                    ppBinding.ReaderQuotas.MaxStringContentLength = 8192
                    ppBinding.ReaderQuotas.MaxArrayLength = 16384
                    ppBinding.ReaderQuotas.MaxBytesPerRead = 4096
                    ppBinding.ReaderQuotas.MaxNameTableCharCount = 65536

                    Return ppBinding
                End Function

            End Class
        End Class
    End Namespace
End Namespace



