Imports System.Xml
Imports System.Web.HttpUtility
Imports System.Web.Configuration
Imports System.IO
Imports System.Collections
Imports System.Data
Imports System.Data.SqlClient
Imports VB = Microsoft.VisualBasic
Imports Protean
Imports Protean.Cms.Cart
Imports System.Net
Imports WorldPay.Sdk

'https://github.com/Worldpay/worldpay-lib-dotnet
'

Public Class WorldPayProvider

    Public Sub New()
        'do nothing
    End Sub

    Public Sub Initiate(ByRef _AdminXforms As Object, ByRef _AdminProcess As Object, ByRef _Activities As Object, ByRef MemProvider As Object, ByRef myWeb As Cms)
        Try
            MemProvider.AdminXforms = New AdminXForms(myWeb)
            MemProvider.AdminProcess = New AdminProcess(myWeb)
            MemProvider.AdminProcess.oAdXfm = MemProvider.AdminXforms
            MemProvider.Activities = New Activities()
        Catch ex As Exception
            returnException("WorldPayProvider", "Initiate", ex, "", "", gbDebug)
        End Try


    End Sub

    Public Class AdminXForms
        Inherits Cms.Admin.AdminXforms
        Private Const mcModuleName As String = "Protean.Providers.WorldPay.AdminXForms"

        Sub New(ByRef aWeb As Cms)
            MyBase.New(aWeb)
        End Sub

    End Class

    Public Class AdminProcess
        Inherits Cms.Admin

        Dim _oAdXfm As Protean.Providers.Payment.WorldPayProvider.AdminXForms

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
        Private Const mcModuleName As String = "Providers.Payment.WorldPayProvider.Activities"
        Private myWeb As Protean.Cms
        Private moPaymentCfg = WebConfigurationManager.GetWebApplicationSection("protean/payment")
        Private oWPCfg As XmlNode
        Private _ServiceKey As String
        Private _ClientKey As String
        Private _opperationMode As String
        Private cMethodName As String = "WorldPay"

        Sub New()
            oWPCfg = moPaymentCfg.SelectSingleNode("provider[@name='WorldPay']")
            _opperationMode = LCase(oWPCfg.SelectSingleNode("opperationMode").Attributes("value").Value())
            Select Case _opperationMode
                Case "live"
                    _ServiceKey = oWPCfg.SelectSingleNode("ServiceKeyLive").Attributes("value").Value()
                    _ClientKey = oWPCfg.SelectSingleNode("ClientKeyLive").Attributes("value").Value()
                Case Else
                    _ServiceKey = oWPCfg.SelectSingleNode("ServiceKeyTest").Attributes("value").Value()
                    _ClientKey = oWPCfg.SelectSingleNode("ClientKeyTest").Attributes("value").Value()
            End Select
        End Sub

        Public Function CancelPayments(ByRef oWeb As Protean.Cms, ByRef nPaymentProviderRef As String) As String
            Dim cProcessInfo As String = ""

            Dim moPaymentCfg = WebConfigurationManager.GetWebApplicationSection("protean/payment")
            Dim oWorldPayCfg As XmlNode

            Try
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12

                oWorldPayCfg = moPaymentCfg.SelectSingleNode("provider[@name='WorldPay']")
                '  _merchantId = oWorldPayCfg.SelectSingleNode("merchantId").Attributes("value").Value()
                '  _merchantToken = oWorldPayCfg.SelectSingleNode("merchantToken").Attributes("value").Value()
                '  _sandboxToken = oWorldPayCfg.SelectSingleNode("merchantSandboxToken").Attributes("value").Value()

                '  _opperationMode = oWorldPayCfg.SelectSingleNode("opperationMode").Attributes("value").Value()

                Return "No Action Required"

            Catch ex As Exception
                returnException(mcModuleName, "CancelPayments", ex, "", cProcessInfo, gbDebug)
                Return ""
            End Try

        End Function


        Public Function CheckStatus(ByRef myWeb As Protean.Cms, ByRef cPaymentProviderId As Long) As String
            Dim cProcessInfo As String = ""
            Try
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12

                Dim sPaymentMethodXml As String

                sPaymentMethodXml = myWeb.moDbHelper.GetDataValue("select cPayMthdDetailXml from tblCartPaymentMethod where nPayMthdKey = " & cPaymentProviderId)

                Dim PaymentXml As New XmlDocument
                PaymentXml.LoadXml(sPaymentMethodXml)

                Dim ccExpire As String = PaymentXml.DocumentElement.SelectSingleNode("expireDate").InnerText
                Dim ccExpireArr As String() = ccExpire.Split(" ")
                Dim DaysInMonth As Integer = Date.DaysInMonth(ccExpireArr(1), ccExpireArr(0))
                Dim LastDayInMonthDate As Date = New Date(ccExpireArr(1), ccExpireArr(0), DaysInMonth)

                If LastDayInMonthDate > Today Then
                    Return "Expires " & niceDate(LastDayInMonthDate)
                Else
                    Return "Card Expired"
                End If

            Catch ex As Exception
                returnException(mcModuleName, "CheckStatus", ex, "", cProcessInfo, gbDebug)
                Return ""
            End Try

        End Function

        Public Function CollectPayment(ByRef myWeb As Protean.Cms, ByVal nPaymentMethodId As Long, ByVal Amount As Double, ByVal CurrencyCode As String, ByVal PaymentDescription As String, ByRef oCart As Protean.Cms.Cart) As String
            Dim orderResponse As WorldPay.Sdk.Models.OrderResponse
            Dim wpClient = New WorldpayRestClient("https://api.worldpay.com/v1", _ServiceKey)
            Dim cProcessInfo As String = ""
            Try
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
                'Get the token
                Dim sToken As String = myWeb.moDbHelper.GetDataValue("select cPayMthdProviderRef from tblCartPaymentMethod where nPayMthdKey = " & nPaymentMethodId)

                Dim orderRequest = New WorldPay.Sdk.Models.OrderRequest() With {
                .token = sToken,
                .orderType = "RECURRING",
                .orderDescription = PaymentDescription,
                .amount = CInt(Amount * 100),
                .currencyCode = CurrencyCode
                }
                orderResponse = wpClient.GetOrderService().Create(orderRequest)

                Return "Success"

            Catch ex As Exception
                returnException(mcModuleName, "CollectPayment", ex, "", cProcessInfo, gbDebug)
                Return "Payment Error"
            End Try
        End Function

        Public Function GetPaymentForm(ByRef oWeb As Protean.Cms, ByRef oCart As Cms.Cart, ByRef oOrder As XmlElement, Optional returnCmd As String = "cartCmd=SubmitPaymentDetails") As xForm
            Dim cProcessInfo As String = ""

            Try
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12

                myWeb = oWeb
                Dim moCartConfig As System.Collections.Specialized.NameValueCollection = oCart.moCartConfig
                Dim mcPaymentMethod As String = oCart.mcPaymentMethod

                Dim sSubmitPath = oCart.mcPagePath & returnCmd
                Dim ccXform As xForm
                Dim oEwProv As Protean.Cms.Cart.PaymentProviders = New PaymentProviders(myWeb)

                Dim mnCartId = oCart.mnCartId
                Dim err_msg As String = ""

                Dim repeatAmt As Double = CDbl("0" & oOrder.GetAttribute("repeatPrice"))
                Dim repeatInterval As String = LCase(oOrder.GetAttribute("repeatInterval"))
                Dim repeatLength As Integer = CDbl("0" & oOrder.GetAttribute("repeatLength"))
                Dim delayStart As Boolean = IIf(LCase(oOrder.GetAttribute("delayStart")) = "true", True, False)

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

                Dim purchaseDesc As String = "Make Payment of " & FormatNumber(oEwProv.mnPaymentAmount, 2) & " " & oWPCfg.SelectSingleNode("currency").Attributes("value").Value & " by Credit/Debit Card"
                If repeatAmt > 0 Then
                    purchaseDesc = "repeat payment of " & FormatNumber(repeatAmt, 2) & " " & oWPCfg.SelectSingleNode("currency").Attributes("value").Value & " by credit/debit card"
                    If oEwProv.mnPaymentAmount > 0 Then
                        purchaseDesc = "Make initial payment of " & FormatNumber(oEwProv.mnPaymentAmount, 2) & " " & oWPCfg.SelectSingleNode("currency").Attributes("value").Value & " and then " & purchaseDesc
                    Else
                        purchaseDesc = "Make " & purchaseDesc
                    End If
                End If

                ccXform = oEwProv.creditCardXform(oOrder, "PayForm", sSubmitPath, oWPCfg.SelectSingleNode("cardsAccepted").Attributes("value").Value, True, purchaseDesc, False)

                'if xform is valid or we have a 3d secure passback
                If ccXform.valid Or Not myWeb.moRequest("PaRes") = "" Then

                    Dim wpClient = New WorldpayRestClient("https://api.worldpay.com/v1", _ServiceKey)
                    Dim PaymentRef As String = ""
                    Dim OrderSuccess As Boolean = False
                    Dim AuthCode As String = ""
                    Try
                        Dim orderResponse As WorldPay.Sdk.Models.OrderResponse
                        Dim tokenResponse As WorldPay.Sdk.Models.TokenResponse

                        If myWeb.moRequest("PaRes") = "" Then
                            Dim GivenName As String = ""
                            Dim Email As String = ""
                            Dim aGivenName() As String
                            'Billing Address
                            Dim billingAddress = New WorldPay.Sdk.Models.Address()
                            Dim oCartAdd As XmlElement = oOrder.SelectSingleNode("Contact[@type='Billing Address']")
                            If Not oCartAdd Is Nothing Then
                                GivenName = oCartAdd.SelectSingleNode("GivenName").InnerText
                                Email = oCartAdd.SelectSingleNode("Email").InnerText
                                aGivenName = Split(oCartAdd.SelectSingleNode("GivenName").InnerText, " ")
                                If Not oCartAdd.SelectSingleNode("Company") Is Nothing Then
                                    billingAddress.address1 = oCartAdd.SelectSingleNode("Company").InnerText
                                    If Not oCartAdd.SelectSingleNode("Street") Is Nothing Then billingAddress.address2 = oCartAdd.SelectSingleNode("Street").InnerText
                                Else
                                    If Not oCartAdd.SelectSingleNode("Street") Is Nothing Then billingAddress.address1 = oCartAdd.SelectSingleNode("Street").InnerText

                                End If
                                If Not oCartAdd.SelectSingleNode("City") Is Nothing Then billingAddress.city = Left(oCartAdd.SelectSingleNode("City").InnerText, 25)
                                If Not oCartAdd.SelectSingleNode("State") Is Nothing Then billingAddress.state = oCartAdd.SelectSingleNode("State").InnerText
                                If Not oCartAdd.SelectSingleNode("PostalCode") Is Nothing Then billingAddress.postalCode = oCartAdd.SelectSingleNode("PostalCode").InnerText
                                If Not oCartAdd.SelectSingleNode("Country") Is Nothing Then billingAddress.countryCode = getCountryISONum(oCartAdd.SelectSingleNode("Country").InnerText)
                                If Not oCartAdd.SelectSingleNode("Telephone") Is Nothing Then billingAddress.telephoneNumber = oCartAdd.SelectSingleNode("Telephone").InnerText
                            End If

                            Dim deliveryAddress = New WorldPay.Sdk.Models.DeliveryAddress()
                            oCartAdd = oOrder.SelectSingleNode("Contact[@type='Delivery Address']")
                            If Not oCartAdd Is Nothing Then
                                aGivenName = Split(oCartAdd.SelectSingleNode("GivenName").InnerText, " ")
                                deliveryAddress.firstName = aGivenName(0)
                                deliveryAddress.lastName = aGivenName(aGivenName.Length - 1)

                                If Not oCartAdd.SelectSingleNode("Company") Is Nothing Then
                                    deliveryAddress.address1 = oCartAdd.SelectSingleNode("Company").InnerText
                                    If Not oCartAdd.SelectSingleNode("Street") Is Nothing Then deliveryAddress.address2 = oCartAdd.SelectSingleNode("Street").InnerText
                                Else
                                    If Not oCartAdd.SelectSingleNode("Street") Is Nothing Then deliveryAddress.address1 = oCartAdd.SelectSingleNode("Street").InnerText

                                End If
                                If Not oCartAdd.SelectSingleNode("City") Is Nothing Then deliveryAddress.city = Left(oCartAdd.SelectSingleNode("City").InnerText, 25)
                                If Not oCartAdd.SelectSingleNode("State") Is Nothing Then deliveryAddress.state = oCartAdd.SelectSingleNode("State").InnerText
                                If Not oCartAdd.SelectSingleNode("PostalCode") Is Nothing Then deliveryAddress.postalCode = oCartAdd.SelectSingleNode("PostalCode").InnerText
                                If Not oCartAdd.SelectSingleNode("Country") Is Nothing Then deliveryAddress.countryCode = getCountryISONum(oCartAdd.SelectSingleNode("Country").InnerText)
                                If Not oCartAdd.SelectSingleNode("Telephone") Is Nothing Then deliveryAddress.telephoneNumber = oCartAdd.SelectSingleNode("Telephone").InnerText
                            End If

                            Dim paymentMethod = New WorldPay.Sdk.Models.CardRequest() With {
                            .cardNumber = myWeb.moRequest("creditCard/number"),
                            .name = GivenName,
                            .expiryMonth = CInt(Left(myWeb.moRequest("creditCard/expireDate"), 2)),
                            .expiryYear = CInt(Right(myWeb.moRequest("creditCard/expireDate"), 4)),
                            .cvc = myWeb.moRequest("creditCard/CV2"),
                            .type = "Card"
                        }
                            If myWeb.moRequest("creditCard/issueNumber") <> "" Then
                                paymentMethod.issueNumber = myWeb.moRequest("creditCard/issueNumber")
                            End If
                            If myWeb.moRequest("creditCard/issueDate") <> "" Then
                                paymentMethod.startMonth = Left(myWeb.moRequest("creditCard/issueDate"), 2)
                                paymentMethod.startYear = Right(myWeb.moRequest("creditCard/issueDate"), 4)
                            End If

                            If oEwProv.mnPaymentAmount > 0 Then
                                Dim orderRequest = New WorldPay.Sdk.Models.OrderRequest() With {
                                    .amount = CInt(oEwProv.mnPaymentAmount * 100),
                                .currencyCode = oCart.mcCurrency,
                                .settlementCurrency = oCart.mcCurrency,
                                .name = GivenName,
                                .billingAddress = billingAddress,
                                .deliveryAddress = deliveryAddress,
                                .paymentMethod = paymentMethod,
                                .orderDescription = oEwProv.mcPaymentOrderDescription,
                                .customerOrderCode = mnCartId,
                                .shopperEmailAddress = Email,
                                .shopperAcceptHeader = myWeb.moRequest.Headers("Accept"),
                                .shopperUserAgent = myWeb.moRequest.ServerVariables("HTTP_USER_AGENT"),
                                .shopperIpAddress = myWeb.moRequest.ServerVariables("REMOTE_ADDR"),
                                .shopperSessionId = myWeb.moSession.SessionID
                                }

                                If repeatAmt > 0 Then
                                    orderRequest.reusable = True
                                    orderRequest.orderType = "RECURRING"
                                    'recuring orders cannont be 3D Secured
                                Else
                                    orderRequest.orderType = "ECOM"
                                    If LCase(oWPCfg.SelectSingleNode("secure3d").Attributes("value").Value) = "on" Then
                                        orderRequest.is3DSOrder = True ' needs to be a config value
                                        If _opperationMode = "test" Then
                                            orderRequest.name = "3D"
                                        End If
                                    End If
                                End If
                                orderResponse = wpClient.GetOrderService().Create(orderRequest)
                            Else

                                Dim oTokenRequest = New WorldPay.Sdk.Models.TokenRequest() With {
                                    .clientKey = _ClientKey,
                                    .paymentMethod = paymentMethod,
                                    .reusable = True
                                }

                                Dim authSvc As AuthService = wpClient.GetAuthService()

                                tokenResponse = wpClient.GetTokenService().Create(oTokenRequest)
                            End If

                        Else
                            Dim threedsinfo As New WorldPay.Sdk.Models.ThreeDSecureInfo() With {
                                .shopperAcceptHeader = myWeb.moRequest.Headers("Accept"),
                                .shopperUserAgent = myWeb.moRequest.ServerVariables("HTTP_USER_AGENT"),
                                .shopperIpAddress = myWeb.moRequest.ServerVariables("REMOTE_ADDR"),
                                .shopperSessionId = myWeb.moSession.SessionID
                            }
                            orderResponse = wpClient.GetOrderService().Authorize(myWeb.moSession("WPCode"), myWeb.moRequest("PaRes"), threedsinfo)
                        End If
                        If orderResponse Is Nothing Then
                            OrderSuccess = True
                            PaymentRef = tokenResponse.token
                            'AuthCode = tokenResponse.orderCode
                        Else
                            If orderResponse.paymentStatus = WorldPay.Sdk.Enums.OrderStatus.PRE_AUTHORIZED Then
                                Dim sRedirectURL As String
                                sRedirectURL = moCartConfig("SecureURL") & "?cartCmd=SubmitPaymentDetails&3dsec=return"
                                ' err_msg = cMethodName & vbLf
                                err_msg = err_msg & "This card has subscribed to 3D Secure. You will now be re-directed to your banks website for further verification."
                                ccXform = oEwProv.xfrmSecure3D(orderResponse.redirectURL, myWeb.moSession.SessionID, orderResponse.oneTime3DsToken, sRedirectURL)
                                ccXform.addNote("creditCard", xForm.noteTypes.Alert, err_msg, True)
                                'save the credit card details in the sessions
                                myWeb.moSession("WPCode") = orderResponse.orderCode
                                For Each cItem As String In myWeb.moRequest.Form.Keys
                                    myWeb.moSession.Add(cItem, myWeb.moRequest.Form.Item(cItem))
                                Next
                                ccXform.valid = False

                            ElseIf orderResponse.paymentStatus = WorldPay.Sdk.Enums.OrderStatus.SUCCESS Then
                                OrderSuccess = True
                                PaymentRef = orderResponse.token
                                AuthCode = orderResponse.orderCode
                            Else
                                err_msg = "WorldPay" & vbLf & orderResponse.paymentStatus & ": " & orderResponse.paymentStatusReason
                                ccXform.addNote(ccXform.moXformElmt, xForm.noteTypes.Alert, err_msg)
                                ccXform.valid = False

                                'if not valid return from 3DS then popout of window back to standard form.
                                If myWeb.moRequest("3dsec") = "return" And Not myWeb.moRequest("PaRes") = "" Then
                                    Dim sRedirectURL As String
                                    sRedirectURL = moCartConfig("SecureURL") & "?cartCmd=SubmitPaymentDetails"
                                    ccXform = Me.xfrmSecure3DReturn(sRedirectURL)
                                End If

                                Throw New WorldpayException("Expected order status PRE_AUTHORIZED")
                            End If
                        End If

                    Catch e As WorldpayException
                        If gbDebug Then
                            err_msg = "WorldPay" & vbLf & "Error code:" + e.apiError.customCode & ": " & vbLf & "Error description: " + e.apiError.description & vbLf & "Error message: " + e.apiError.message
                        Else
                            err_msg = e.apiError.message
                        End If
                        ccXform.addNote(ccXform.moXformElmt, xForm.noteTypes.Alert, err_msg)
                        ccXform.valid = False
                    End Try

                    If OrderSuccess Then

                        Dim oeResponseElmt As XmlElement = ccXform.Instance.OwnerDocument.CreateElement("Response")
                        oeResponseElmt.SetAttribute("AuthCode", AuthCode)
                        ccXform.Instance.FirstChild.AppendChild(oeResponseElmt)

                        oCart.mnPaymentId = oEwProv.savePayment(myWeb.mnUserId, "WorldPay", PaymentRef, cMethodName, ccXform.Instance.FirstChild, Now, False, oEwProv.mnPaymentAmount)

                        If myWeb.moRequest("3dsec") = "return" And Not myWeb.moRequest("PaRes") = "" Then
                            'Create a form to submit back and get fully valid result
                            'redirect to show invoice
                            Dim sRedirectURL As String
                            sRedirectURL = moCartConfig("SecureURL") & returnCmd & "&3dsec=showInvoice"
                            ccXform = Me.xfrmSecure3DReturn(sRedirectURL)
                            myWeb.moSession("PaRes") = myWeb.moRequest("PaRes")
                        Else
                            ccXform.valid = True
                        End If
                        err_msg = cMethodName & ":WP_Response = Valid"

                    End If


                    'Update Seller Notes:
                    Dim sSql As String = "select * from tblCartOrder where nCartOrderKey = " & mnCartId
                    Dim oDs As DataSet
                    Dim oRow As DataRow
                    oDs = myWeb.moDbHelper.getDataSetForUpdate(sSql, "Order", "Cart")
                    For Each oRow In oDs.Tables("Order").Rows
                        If myWeb.moRequest("3dsec") = "return" And Not myWeb.moRequest("PaRes") = "" Then
                            oRow("cPaymentRef") = PaymentRef
                            oRow("cSellerNotes") = oRow("cSellerNotes") & vbLf & Today & " " & TimeOfDay & ": changed to: (Payment Received) " & vbLf & "comment: " & err_msg & vbLf
                        Else
                            oRow("cSellerNotes") = oRow("cSellerNotes") & vbLf & Today & " " & TimeOfDay & ": changed to: (Payment Failed) " & vbLf & "comment: " & err_msg & vbLf
                        End If
                    Next
                    myWeb.moDbHelper.updateDataset(oDs, "Order")

                Else
                    'if we are redirected after 3Dsecure then we are valid
                    If myWeb.moRequest("3dsec") = "showInvoice" And Not myWeb.moSession("PaRes") = "" Then
                        ccXform.valid = True
                    End If
                End If



                Return ccXform


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
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12

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

                sSql = "select cLocationISOa2 from tblCartShippingLocations where cLocationNameFull Like '" & sCountry & "' or cLocationNameShort Like '" & sCountry & "'"
                oDr = myWeb.moDbHelper.getDataReader(sSql)
                If oDr.HasRows Then
                    While oDr.Read
                        strReturn = oDr("cLocationISOa2")
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

    End Class




End Class
