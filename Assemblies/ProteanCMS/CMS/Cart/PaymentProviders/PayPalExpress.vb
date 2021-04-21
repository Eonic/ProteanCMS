Imports System.Xml
Imports System.Web.HttpUtility
Imports System.Web.Configuration
Imports System.IO
Imports System.Collections
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.SqlClient
Imports VB = Microsoft.VisualBasic
Imports Eonic
Imports Protean.Cms.Cart
Imports System.Net
Imports PayPal.Api

Namespace Providers
    Namespace Payment
        Public Class PayPalExpressRest

            Public Sub New()
                'do nothing
            End Sub

            Public Sub Initiate(ByRef _AdminXforms As Object, ByRef _AdminProcess As Object, ByRef _Activities As Object, ByRef BaseProvider As Object, ByRef myWeb As Cms)

                BaseProvider.AdminXforms = New AdminXForms(myWeb)
                BaseProvider.AdminProcess = New AdminProcess(myWeb)
                BaseProvider.AdminProcess.oAdXfm = BaseProvider.AdminXforms
                BaseProvider.Activities = New Activities()

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

                Dim _oAdXfm As Protean.Providers.Payment.PayPalExpressRest.AdminXForms

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
                Inherits Protean.Providers.Payment.DefaultProvider.Activities

                Private Const mcModuleName As String = "Providers.Payment.PayPalPro.Activities"
                Private myWeb As Protean.Cms
                Protected moPaymentCfg As XmlNode
                Private nTransactionMode As TransactionMode

                Enum TransactionMode
                    Live = 0
                    Test = 1
                    Fail = 2
                End Enum

                Public Function GetPaymentForm(ByRef myWeb As Protean.Cms, ByRef oCart As Cms.Cart, ByRef oOrder As XmlElement, Optional returnCmd As String = "cartCmd=SubmitPaymentDetails") As xForm
                    PerfMon.Log("PaymentProviders", "payPayPalExpress")
                    Dim sSql As String

                    Dim ppXform As xForm = New xForm(myWeb.msException)

                    Dim Xform3dSec As xForm = Nothing

                    Dim bIsValid As Boolean
                    Dim err_msg As String = ""
                    Dim err_msg_log As String = ""
                    Dim sProcessInfo As String = ""
                    Dim cResponse As String = ""

                    Dim oDictOpt As Hashtable = New Hashtable

                    Dim bCv2 As Boolean = False
                    Dim b3DSecure As Boolean = False
                    Dim b3DAuthorised As Boolean = False
                    Dim sRedirectURL As String = ""
                    Dim sPaymentRef As String = ""

                    Dim cProcessInfo As String = "payPayPalExpress"

                    'Get the payment options into a hashtable
                    Dim oPayPalCfg As XmlNode
                    Dim bSavePayment As Boolean = False
                    Dim bAllowSavePayment As Boolean = False
                    Dim oItemElmt As XmlElement
                    Dim oOptElmt As XmlElement
                    Dim host As String = "www.paypal.com"
                    Dim mcCurrency As String

                    Try

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

                        ' If sProfile <> "" Then
                        ' oPayPalCfg= moPaymentCfg.SelectSingleNode("provider[@name='PayPalExpress' and @profile='" & sProfile & "']")
                        ' Else
                        moPaymentCfg = WebConfigurationManager.GetWebApplicationSection("protean/payment")
                        oPayPalCfg = moPaymentCfg.SelectSingleNode("provider[@name='PayPalExpressRest']")
                        ' End If

                        ' Get the payment options
                        'oPayPalCfg= moPaymentCfg.SelectSingleNode("provider[@name='SecPay']")
                        oDictOpt = xmlTools.xmlToHashTable(oPayPalCfg, "value")

                        Select Case oDictOpt("opperationMode").ToString()
                            Case "true"
                                nTransactionMode = TransactionMode.Test
                            Case "false"
                                nTransactionMode = TransactionMode.Fail
                            Case "live"
                                nTransactionMode = TransactionMode.Live
                        End Select

                        ' override the currency
                        If oDictOpt.ContainsKey("currency") Then
                            If oDictOpt("currency") <> "" Then
                                mcCurrency = oDictOpt("currency")
                            End If
                        End If

                        'Create the API Context
                        Dim AccessToken As String = New OAuthTokenCredential(oDictOpt("ClientId"), oDictOpt("ClientSecret")).GetAccessToken()
                        Dim ppApiCtx As APIContext = New APIContext(AccessToken)

                        'Load the Xform

                        Select Case myWeb.moRequest("ppCmd")
                            Case "callback", "return"

                                ''Case for Successful payment return
                                'Dim oGetExpChk As Protean.PayPalAPI.GetExpressCheckoutDetailsRequestType = New Protean.PayPalAPI.GetExpressCheckoutDetailsRequestType()
                                'oGetExpChk.Token = myWeb.moRequest("token")
                                'oGetExpChk.Version = "63.0"

                                'Dim oGetExpChkReq As New Protean.PayPalAPI.GetExpressCheckoutDetailsReq()
                                'oGetExpChkReq.GetExpressCheckoutDetailsRequest = oGetExpChk

                                ''Get the Transaction Details back
                                'Dim oExpChkResponse As Protean.PayPalAPI.GetExpressCheckoutDetailsResponseType = New Protean.PayPalAPI.GetExpressCheckoutDetailsResponseType
                                'oExpChkResponse = ppIface.GetExpressCheckoutDetails(ppProfile, oGetExpChkReq)

                                ''Confirm the Sale
                                'Dim oDoExpChkReqType As Protean.PayPalAPI.DoExpressCheckoutPaymentRequestType = New Protean.PayPalAPI.DoExpressCheckoutPaymentRequestType
                                'oDoExpChkReqType.Version = "63.0"

                                'Dim oDoExpChkReq As Protean.PayPalAPI.DoExpressCheckoutPaymentReq = New Protean.PayPalAPI.DoExpressCheckoutPaymentReq
                                'oDoExpChkReq.DoExpressCheckoutPaymentRequest = oDoExpChkReqType

                                'Dim oDoExpChkDetails As Protean.PayPalAPI.DoExpressCheckoutPaymentRequestDetailsType = New Protean.PayPalAPI.DoExpressCheckoutPaymentRequestDetailsType
                                'oDoExpChkReqType.DoExpressCheckoutPaymentRequestDetails = oDoExpChkDetails
                                'oDoExpChkDetails.Token = myWeb.moRequest("token")

                                'oDoExpChkDetails.PayerID = oExpChkResponse.GetExpressCheckoutDetailsResponseDetails.PayerInfo.PayerID
                                'oDoExpChkDetails.PaymentAction = getPPPaymentActionFromCode(oDictOpt("PaymentAction"))
                                'oDoExpChkDetails.PaymentDetails = oExpChkResponse.GetExpressCheckoutDetailsResponseDetails.PaymentDetails

                                'Dim oDoExpChkResponse As Protean.PayPalAPI.DoExpressCheckoutPaymentResponseType = New Protean.PayPalAPI.DoExpressCheckoutPaymentResponseType

                                ''TS added following up on this http://stackoverflow.com/questions/15817839/you-do-not-have-permissions-to-make-this-api-call-using-soap-for-dodirectpayment
                                ''ppProfile.Credentials.Subject = Replace(oDictOpt("accountId"), "_api1", "@")

                                'Try

                                '    oDoExpChkResponse = ppIface.DoExpressCheckoutPayment(ppProfile2, oDoExpChkReq)

                                '    Select Case oDoExpChkResponse.Ack
                                '        Case Protean.PayPalAPI.AckCodeType.Success
                                '            bIsValid = True

                                '            err_msg = "Paypal Payment Completed - Ref: " & CStr(mnCartId)
                                '            ppXform.moPageXML = moPageXml
                                '            ppXform.NewFrm("PayForm")
                                '            ppXform.valid = bIsValid

                                '            Dim cAccountName As String = oExpChkResponse.GetExpressCheckoutDetailsResponseDetails.PayerInfo.PayerID 'PayPal User

                                '            Dim oInstanceElmt As XmlElement = ppXform.Instance.OwnerDocument.CreateElement("instance")
                                '            Dim oPayPalElmt As XmlElement = ppXform.Instance.OwnerDocument.CreateElement("PayPalExpress")
                                '            oInstanceElmt.AppendChild(oPayPalElmt)
                                '            'Add stuff we want to save here...
                                '            oPayPalElmt.SetAttribute("payerId", cAccountName)

                                '            ppXform.Instance = oInstanceElmt

                                '            'update delivery address

                                '            'update notes ??
                                '            err_msg = "Notes:" & oExpChkResponse.GetExpressCheckoutDetailsResponseDetails.Note


                                '            savedPaymentId = savePayment(myWeb.mnUserId, "PayPalExpress", mnCartId, cAccountName, ppXform.Instance.FirstChild, Now, False, mnPaymentAmount)
                                '        Case Else
                                '            Dim ppError As Protean.PayPalAPI.ErrorType
                                '            For Each ppError In oDoExpChkResponse.Errors
                                '                err_msg = err_msg & " Error:" & ppError.ErrorCode
                                '                err_msg = err_msg & " Msg:" & ppError.LongMessage
                                '            Next
                                '            ppXform.moPageXML = moPageXml
                                '            ppXform.NewFrm("PayForm")
                                '            ppXform.valid = False
                                '            ppXform.addNote(ppXform.moXformElmt, xForm.noteTypes.Alert, err_msg)

                                '    End Select

                                'Catch ex As Exception
                                '    err_msg = err_msg & "Could not Connect to PayPal at this time please use an alternative payment method."
                                '                err_msg = err_msg & " Error:" & ex.Message

                                '                ppXform.moPageXML = myWeb.moPageXml
                                '                ppXform.NewFrm("PayForm")
                                '                ppXform.valid = False
                                '                ppXform.addNote(ppXform.moXformElmt, xForm.noteTypes.Alert, "Could not Connect to PayPal at this time please use an alternative payment method.")

                                '            End Try

                            Case Else

                                Dim cCurrentURL As String = oEwProv.moCartConfig("SecureURL") & "?" & returnCmd

                                Dim reDirURLs As New RedirectUrls() With {
                                    .return_url = cCurrentURL & "&ppCmd=return", 'Return to pay selector page 
                                    .cancel_url = cCurrentURL & "&ppCmd=cancel" 'Return to pay selector 
                                }

                                Dim oPayerInfo As New PayerInfo
                                Dim oCartAdd As XmlElement = oOrder.SelectSingleNode("Contact[@type='Billing']")
                                Dim aGivenName() As String = Split(oCartAdd.SelectSingleNode("GivenName").InnerText, " ")
                                Select Case UBound(aGivenName)

                                    Case 0
                                        oPayerInfo.last_name = aGivenName(0)
                                        oPayerInfo.first_name = aGivenName(0)
                                    Case 1
                                        oPayerInfo.first_name = aGivenName(0)
                                        oPayerInfo.last_name = aGivenName(1)
                                    Case 2
                                        oPayerInfo.first_name = aGivenName(0)
                                        oPayerInfo.middle_name = aGivenName(1)
                                        oPayerInfo.last_name = aGivenName(2)
                                    Case 3
                                        oPayerInfo.first_name = aGivenName(0)
                                        oPayerInfo.middle_name = aGivenName(1)
                                        oPayerInfo.last_name = aGivenName(2)
                                        oPayerInfo.suffix = aGivenName(3)
                                End Select
                                oPayerInfo.email = oCartAdd.SelectSingleNode("Email").InnerText
                                oPayerInfo.billing_address = New Address With {
                                    .line1 = IIf(oCartAdd.SelectSingleNode("Company").InnerText = "", oCartAdd.SelectSingleNode("Street").InnerText, oCartAdd.SelectSingleNode("Company").InnerText),
                                    .line2 = IIf(oCartAdd.SelectSingleNode("Company").InnerText = "", Nothing, oCartAdd.SelectSingleNode("Street").InnerText),
                                    .city = oCartAdd.SelectSingleNode("City").InnerText,
                                    .state = oCartAdd.SelectSingleNode("State").InnerText,
                                    .postal_code = oCartAdd.SelectSingleNode("PostalCode").InnerText,
                                    .phone = oCartAdd.SelectSingleNode("Tel").InnerText,
                                    .country_code = getCountryISONum(oCartAdd.SelectSingleNode("Country").InnerText)
                                }
                                oCartAdd = oOrder.SelectSingleNode("Contact[@type='Delivery Address']")
                                oPayerInfo.shipping_address = New Address With {
                                    .line1 = IIf(oCartAdd.SelectSingleNode("Company").InnerText = "", oCartAdd.SelectSingleNode("Street").InnerText, oCartAdd.SelectSingleNode("Company").InnerText),
                                    .line2 = IIf(oCartAdd.SelectSingleNode("Company").InnerText = "", Nothing, oCartAdd.SelectSingleNode("Street").InnerText),
                                    .city = oCartAdd.SelectSingleNode("City").InnerText,
                                    .state = oCartAdd.SelectSingleNode("State").InnerText,
                                    .postal_code = oCartAdd.SelectSingleNode("PostalCode").InnerText,
                                    .phone = oCartAdd.SelectSingleNode("Tel").InnerText,
                                    .country_code = getCountryISONum(oCartAdd.SelectSingleNode("Country").InnerText)
                                }
                                Dim oPayer As New Payer() With {
                                    .payment_method = "paypal",
                                    .payer_info = oPayerInfo
                                    }

                                Dim nShippingCost As Decimal = oOrder.GetAttribute("shippingCost")
                                Dim nTaxCost As Decimal = Round(oOrder.GetAttribute("vatAmt"), , , True)

                                If oOrder.GetAttribute("vatRate") > 0 And LCase(oDictOpt("VATonShipping")) = "on" Then
                                    Dim nShippingVat As Decimal = Round(nShippingCost * (oOrder.GetAttribute("vatRate") / 100), , , True)
                                    nTaxCost = nTaxCost - nShippingVat
                                    nShippingCost = nShippingCost + nShippingVat
                                End If

                                Dim oDetails As New Details() With {
                                    .tax = nTaxCost,
                                    .shipping = nShippingCost,
                                    .subtotal = oEwProv.mnPaymentAmount - nTaxCost - nShippingCost
                                }

                                Dim oAmount As New Amount() With {
                                    .currency = IIf(oCart.mcCurrencyCode = "", oCart.mcCurrency, oCart.mcCurrencyCode),
                                    .details = oDetails,
                                    .total = oEwProv.mnPaymentAmount
                                }

                                Dim oPayee As New Payee With {
                                    .email = "",
                                    .phone = New Phone With {
                                        .country_code = "+44",
                                        .national_number = ""
                                    }
                                }
                                Dim oItemListCol As List(Of Item) = New List(Of Item)()
                                For Each oItemElmt In oOrder.SelectNodes("Item")
                                    Dim oItem As New Item With {
                                        .name = Left(oItemElmt.SelectSingleNode("Name").InnerText, 120),
                                        .quantity = oItemElmt.GetAttribute("quantity"),
                                        .description = Left(oItemElmt.SelectSingleNode("Body").InnerText, 127),
                                        .tax = FormatNumber(CDbl("0" & oItemElmt.GetAttribute("itemTax")), 2),
                                        .currency = getCountryISONum(mcCurrency),
                                        .price = FormatNumber(oItemElmt.GetAttribute("itemTotal"), 2)
                                    }
                                    oItemListCol.Add(oItem)

                                    'loop through and add any options
                                    For Each oOptElmt In oItemElmt.SelectNodes("Item")
                                        Dim oItemOpt As New Item With {
                                         .name = Left(oOptElmt.SelectSingleNode("Name").InnerText, 120),
                                        .quantity = oOptElmt.GetAttribute("quantity"),
                                        .description = Left(oOptElmt.SelectSingleNode("Body").InnerText, 127),
                                        .tax = FormatNumber(CDbl("0" & oOptElmt.GetAttribute("itemTax")), 2),
                                        .currency = getCountryISONum(mcCurrency),
                                        .price = FormatNumber(oOptElmt.GetAttribute("itemTotal"), 2)
                                            }
                                        oItemListCol.Add(oItemOpt)
                                    Next
                                Next
                                Dim oItemList As New ItemList With {
                                    .items = oItemListCol
                                }

                                'build the transaction list / cart items
                                Dim oTrasnaction As New Transaction With {
                                    .amount = oAmount,
                                    .description = oCart.moCartConfig("OrderNoPrefix") & CStr(oEwProv.mnCartId) & " - " & oCartAdd.SelectSingleNode("GivenName").InnerText,
                                    .invoice_number = oCart.moCartConfig("OrderNoPrefix") & CStr(oEwProv.mnCartId),
                                    .payee = oPayee,
                                    .item_list = oItemList
                                }
                                Dim oTransactionList As List(Of Transaction) = New List(Of Transaction)()
                                oTransactionList.Add(oTrasnaction)

                                Dim oPayment As New PayPal.Api.Payment With {
                                    .intent = "sale",
                                    .payer = oPayer,
                                    .transactions = oTransactionList,
                                    .redirect_urls = reDirURLs
                                }

                                Dim oCreatePayment As PayPal.Api.Payment = oPayment.Create(ppApiCtx)


                                'If mcPaymentType = "deposit" Then

                                '    Dim oDiscountItem As Protean.PayPalAPI.PaymentDetailsItemType = New Protean.PayPalAPI.PaymentDetailsItemType
                                '    oDiscountItem.Name = "Final payment to be made later"
                                '    oDiscountItem.Quantity = "1"
                                '    'itemtax
                                '    Dim oItemTax As Protean.PayPalAPI.BasicAmountType = New Protean.PayPalAPI.BasicAmountType
                                '    oItemTax.currencyID = getPPCurencyFromCode(mcCurrency)
                                '    oItemTax.Value = "0"
                                '    oDiscountItem.Tax = oItemTax
                                '    'itemAmount
                                '    'Dim nItemPlusTax As Double = FormatNumber(oItemElmt.GetAttribute("price"), 2) + (CDbl("0" & oItemElmt.GetAttribute("itemTax")) / oItemElmt.GetAttribute("quantity"))
                                '    Dim oItemAmount As Protean.PayPalAPI.BasicAmountType = New Protean.PayPalAPI.BasicAmountType
                                '    oItemAmount.currencyID = getPPCurencyFromCode(mcCurrency)
                                '    oItemAmount.Value = FormatNumber(mnPaymentMaxAmount - mnPaymentAmount, 2) * -1
                                '    oDiscountItem.Amount = oItemAmount
                                '    oItemGroup(i) = oDiscountItem

                                'End If

                                Dim redirectUrl As String
                                Dim links As List(Of Links).Enumerator = oCreatePayment.links.GetEnumerator()
                                Do While links.MoveNext()
                                    Dim thislink As PayPal.Api.Links = links.Current
                                    If thislink.rel.ToLower().Trim() = "approval_url" Then
                                        redirectUrl = thislink.href
                                    End If
                                Loop

                                ppXform.NewFrm("PayForm")
                                ppXform.valid = False

                                'Select Case oExpChkResponse.Ack
                                '    Case Protean.PayPalAPI.AckCodeType.Success, Protean.PayPalAPI.AckCodeType.SuccessWithWarning
                                '        myWeb.msRedirectOnEnd = redirectUrl
                                '        err_msg = "Redirect to - " & redirectUrl
                                '    Case Else
                                '        Dim ppError As Protean.PayPalAPI.ErrorType
                                '        For Each ppError In oExpChkResponse.Errors
                                '            err_msg = err_msg & " Error:" & ppError.ErrorCode
                                '            err_msg = err_msg & " Msg:" & ppError.LongMessage
                                '        Next
                                '        ppXform.addNote(ppXform.moXformElmt, xForm.noteTypes.Alert, err_msg)
                                'End Select

                        End Select


                        'Update Seller Notes:

                        sSql = "select * from tblCartOrder where nCartOrderKey = " & oCart.mnCartId
                        Dim oDs As DataSet
                        Dim oRow As DataRow
                        oDs = myWeb.moDbHelper.getDataSetForUpdate(sSql, "Order", "Cart")
                        For Each oRow In oDs.Tables("Order").Rows
                            If bIsValid Then
                                oRow("cSellerNotes") = oRow("cSellerNotes") & vbLf & Today & " " & TimeOfDay & ": changed to: (Payment Received) " & vbLf & "comment: " & err_msg
                            Else
                                If err_msg.StartsWith("Redirect") Then
                                    oRow("cSellerNotes") = oRow("cSellerNotes") & vbLf & Today & " " & TimeOfDay & ": changed to: (User Redirected)" & vbLf & "comment: " & err_msg
                                Else
                                    oRow("cSellerNotes") = oRow("cSellerNotes") & vbLf & Today & " " & TimeOfDay & ": changed to: (Payment Failed) " & vbLf & "comment: " & err_msg
                                End If
                            End If
                        Next
                        myWeb.moDbHelper.updateDataset(oDs, "Order")

                        Return ppXform

                    Catch ex As Exception
                        returnException(myWeb.msException, mcModuleName, "GetPaymentForm", ex, "", cProcessInfo, gbDebug)
                        Return Nothing
                    End Try
                End Function

                Public Function AddPaymentButton(ByRef oOptXform As xForm, ByRef oFrmElmt As XmlElement, ByVal configXml As XmlElement, ByVal nPaymentAmount As Double, ByVal submissionValue As String, ByVal refValue As String) As Boolean
                    Try

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
                    Catch ex As Exception
                        returnException(myWeb.msException, mcModuleName, "AddPaymentButton", ex, "", "", gbDebug)
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
                        returnException(myWeb.msException, mcModuleName, "getCountryISOnum", ex, "", cProcessInfo, gbDebug)
                        Return Nothing
                    End Try
                End Function


                Public Function CheckStatus(ByRef myWeb As Protean.Cms, ByRef nPaymentProviderRef As String) As String
                    Dim cProcessInfo As String = ""

                    Dim oPayPalProCfg As XmlNode
                    Dim nTransactionMode As TransactionMode

                    Try
                        Return "not checked"


                    Catch ex As Exception
                        returnException(myWeb.msException, mcModuleName, "CheckStatus", ex, "", cProcessInfo, gbDebug)
                        Return ""
                    End Try

                End Function

                Public Function CancelPayments(ByRef myWeb As Protean.Cms, ByRef nPaymentProviderRef As String) As String
                    Dim cProcessInfo As String = ""
                    Dim oPayPalProCfg As XmlNode

                    Try

                        Return "Not Implemented"

                    Catch ex As Exception
                        returnException(myWeb.msException, mcModuleName, "CancelPayments", ex, "", cProcessInfo, gbDebug)
                        Return ""
                    End Try

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



