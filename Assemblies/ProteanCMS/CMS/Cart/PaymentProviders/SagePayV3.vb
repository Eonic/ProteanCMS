Imports System.Xml
Imports System.Web.HttpUtility
Imports System.Web.Configuration
Imports System.IO
Imports System.Collections
Imports System.Data
Imports System.Data.SqlClient
Imports System.Text
Imports System.Text.RegularExpressions
Imports VB = Microsoft.VisualBasic
Imports Eonic
Imports Protean.Cms.Cart
Imports System.Net
Imports CardinalCommerce

Namespace Providers
    Namespace Payment
        Public Class SagePayV3

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

                Dim _oAdXfm As Protean.Providers.Payment.SagePayV3.AdminXForms

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
                Inherits Protean.Providers.Payment.EonicProvider.Activities

                Private Const mcModuleName As String = "Providers.Payment.SagePayV3.Activities"
                Private myWeb As Protean.Cms
                Protected moPaymentCfg As XmlNode
                Private nTransactionMode As TransactionMode

                Enum TransactionMode
                    Live = 0
                    Test = 1
                    Fail = 2
                End Enum

                Public Overloads Function GetPaymentForm(ByRef oWeb As Protean.Cms, ByRef oCart As Cms.Cart, ByRef oOrder As XmlElement, Optional returnCmd As String = "cartCmd=SubmitPaymentDetails") As xForm
                    PerfMon.Log("Protean.Providers.payment.SagePayV3", "GetPaymentForm")
                    PerfMon.Log("PaymentProviders", "SagePayV3")

                    Dim oRequest As HttpWebRequest
                    Dim oResponse As HttpWebResponse
                    Dim oStream As System.IO.Stream
                    Dim oStreamReader As StreamReader

                    Dim nResult As Long

                    Dim cResponse As String
                    Dim cRequest As String
                    Dim oEncoding As New ASCIIEncoding
                    Dim byRequest As Byte()

                    Dim sSql As String

                    Dim cMessage As String

                    Dim ccXform As xForm

                    Dim bIsValid As Boolean = False
                    Dim err_msg As String = ""
                    Dim err_msg_log As String = ""
                    Dim sProcessInfo As String = ""
                    Dim oDictOpt As New Hashtable

                    Dim bCv2 As Boolean = False
                    Dim b3dSecure As Boolean = False
                    Dim oResponseDict As Hashtable = Nothing
                    Dim oCartAdd As XmlNode
                    Dim b3dAuthorised As Boolean = False

                    Dim oElmt As XmlElement

                    Dim oSagePayV3Cfg As XmlNode

                    Dim cProcessInfo As String = "SagePayV3Tx"
                    Dim sAPIVer As String = "3.00"
                    Dim sVSPUrl As String = ""
                    Dim sVSP3DSUrl As String = ""
                    Dim nAttemptCount As Long

                    Dim SagePayPartnerID As String = "C1B0075E-F99D-4E5D-8AE0-E66F13FD081E"

                    Dim cSellerNotes As String = ""


                    Dim bSavePayment As Boolean = False

                    Try
                        myWeb = oWeb
                        Dim cIPAddress As String = myWeb.moRequest.ServerVariables("REMOTE_ADDR")
                        Dim sSubmitPath As String = oCart.mcPagePath & returnCmd

                        'confirm TLS 1.2
                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12

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



                        If Not myWeb.moSession("attemptCount") Is Nothing Then
                            nAttemptCount = myWeb.moSession("attemptCount")
                            nAttemptCount = nAttemptCount + 1
                        Else
                            nAttemptCount = 1
                        End If

                        'Get the payment options into a hashtable
                        moPaymentCfg = WebConfigurationManager.GetWebApplicationSection("protean/payment")
                        oSagePayV3Cfg = moPaymentCfg.SelectSingleNode("provider[@name='SagePayV3']")
                        For Each oElmt In oSagePayV3Cfg.SelectNodes("*")
                            If Not oElmt.GetAttribute("value") Is Nothing Then
                                oDictOpt.Add(oElmt.Name, oElmt.GetAttribute("value"))
                            End If
                        Next

                        If oDictOpt("validateCV2") = "on" Then bCv2 = True
                        If oDictOpt("secure3d") <> "2" Then
                            b3dSecure = True
                            ' Check the IP Addresses for a block
                            If Not (oDictOpt("secure3dIpBlock") Is Nothing) Then
                                Dim oRE As Regex = New Regex("(,|^)" & Replace(cIPAddress, ".", "\.") & "(,|$)")
                                If oRE.IsMatch(oDictOpt("secure3dIpBlock")) Then b3dSecure = False
                            End If
                        End If

                        'sAPIVer = "2.22"

                        sAPIVer = "3.00"

                        Select Case oDictOpt("opperationMode")
                            Case "test", "true"
                                sVSPUrl = "https://test.sagepay.com/gateway/service/vspdirect-register.vsp"
                                sVSP3DSUrl = "https://test.sagepay.com/gateway/service/direct3dcallback.vsp"
                                nTransactionMode = TransactionMode.Test
                            Case "live"
                                sVSPUrl = "https://live.sagepay.com/gateway/service/vspdirect-register.vsp"
                                sVSP3DSUrl = "https://live.sagepay.com/gateway/service/direct3dcallback.vsp"
                                nTransactionMode = TransactionMode.Live
                        End Select


                        'Load the Xform
                        ccXform = oEwProv.creditCardXform(oOrder, "PayForm", sSubmitPath, oDictOpt("cardsAccepted"), True, "Make Payment of " & FormatNumber(oEwProv.mnPaymentAmount, 2) & " " & oDictOpt("currency") & " by Credit/Debit Card")

                        If b3dSecure Then
                            'check for return from aquiring bank
                            If myWeb.moRequest("PARes") <> "" Then
                                b3dAuthorised = True
                            End If
                        End If

                        Dim orderPrefix As String = ""

                        If oDictOpt("sendPrefix") = "true" Then
                            orderPrefix = IIf(nTransactionMode = TransactionMode.Test, "D-", "") & oEwProv.moCartConfig("OrderNoPrefix")
                        End If

                        If ccXform.valid = True Or b3dAuthorised Then
                            If b3dAuthorised Then
                                '3D Secure Resume
                                cRequest = "MD=" & myWeb.moRequest("MD") & "&"
                                cRequest = cRequest & "PARes=" & goServer.UrlEncode(myWeb.moRequest("PARes")) & "&"
                            Else
                                Dim oSaveElmt As XmlElement = ccXform.Instance.SelectSingleNode("creditCard/bSavePayment")
                                If Not oSaveElmt Is Nothing Then
                                    If oSaveElmt.InnerText = "true" Then
                                        bSavePayment = True
                                    End If
                                End If

                                'standard card validation request
                                cRequest = "VPSProtocol=" & sAPIVer & "&"
                                cRequest = cRequest & "TxType=" & oDictOpt("transactionType") & "&" ' 0=omited 1=full auth 2=pre-auth only
                                cRequest = cRequest & "Vendor=" & oDictOpt("accountId") & "&" ' 0=omited 1=full auth 2=pre-auth only
                                If nAttemptCount > 1 Then
                                    cRequest = cRequest & "VendorTXCode=" & goServer.UrlEncode(orderPrefix & CStr(oEwProv.mnCartId)) & "-" & nAttemptCount & "&"
                                Else
                                    cRequest = cRequest & "VendorTXCode=" & goServer.UrlEncode(orderPrefix & CStr(oEwProv.mnCartId)) & "&"
                                End If
                                cRequest = cRequest & "Amount=" & goServer.UrlEncode(CStr(FullMoneyString(oEwProv.mnPaymentAmount))) & "&"
                                cRequest = cRequest & "Currency=" & goServer.UrlEncode(oDictOpt("currency")) & "&"
                                cRequest = cRequest & "Description=" & goServer.UrlEncode(Left(oEwProv.mcPaymentOrderDescription, 90)) & "&"
                                cRequest = cRequest & "CardHolder=" & goServer.UrlEncode(oEwProv.mcCardHolderName) & "&"
                                cRequest = cRequest & "CardNumber=" & goServer.UrlEncode(FormatCreditCardNumber(myWeb.moRequest("creditCard/number"))) & "&"
                                ' If Not myWeb.moRequest("creditCard/issueDate") = "" Then
                                ' cRequest = cRequest & "StartDate=" & fmtSecPayDate(myWeb.moRequest("creditCard/issueDate")) & "&"
                                ' End If
                                cRequest = cRequest & "ExpiryDate=" & fmtSecPayDate(myWeb.moRequest("creditCard/expireDate")) & "&"
                                'cRequest = cRequest & "IssueNo=" & goServer.UrlEncode(myWeb.moRequest("creditCard/issueNumber")) & "&"
                                cRequest = cRequest & "CV2=" & goServer.UrlEncode(CStr(myWeb.moRequest("creditCard/CV2"))) & "&"
                                cRequest = cRequest & "CardType=" & goServer.UrlEncode(myWeb.moRequest("creditCard/type")) & "&"
                                'cRequest = cRequest & "Token=" & goServer.UrlEncode(myWeb.moRequest("creditCard/type")) & "&"

                                Dim FirstName As String = ""
                                Dim LastName As String = ""
                                Dim aGivenName() As String = Split(oEwProv.mcCardHolderName, " ")
                                Select Case UBound(aGivenName)
                                    Case 0
                                        LastName = aGivenName(0)
                                        FirstName = aGivenName(0)
                                    Case 1
                                        FirstName = aGivenName(0)
                                        LastName = aGivenName(1)
                                    Case 2
                                        FirstName = aGivenName(0)
                                        ' MiddleName = aGivenName(1)
                                        LastName = aGivenName(2)
                                    Case 3
                                        FirstName = aGivenName(0)
                                        'MiddleName = aGivenName(1)
                                        LastName = aGivenName(2)
                                        'Suffix = aGivenName(3)
                                End Select


                                cRequest = cRequest & "BillingSurname=" & LastName & "&"
                                cRequest = cRequest & "BillingFirstnames=" & FirstName & "&"


                                oCartAdd = oOrder.SelectSingleNode("Contact[@type='Billing Address']")

                                cRequest = cRequest & "BillingAddress1=" & goServer.UrlEncode(oCartAdd.SelectSingleNode("Street").InnerText) & "&"
                                ' cRequest = cRequest & "BillingAddress2=" & goServer.UrlEncode(oCartAdd.SelectSingleNode("Street2").InnerText) & "&"
                                cRequest = cRequest & "BillingCity=" & goServer.UrlEncode(oCartAdd.SelectSingleNode("City").InnerText) & "&"
                                cRequest = cRequest & "BillingPostCode=" & goServer.UrlEncode(oCartAdd.SelectSingleNode("PostalCode").InnerText) & "&"

                                cRequest = cRequest & "BillingCountry=" & goServer.UrlEncode(oEwProv.getCountryISO2Code(oCartAdd.SelectSingleNode("Country").InnerText)) & "&"
                                If oEwProv.getCountryISO2Code(oCartAdd.SelectSingleNode("Country").InnerText) = "US" Then
                                    cRequest = cRequest & "BillingState=" & goServer.UrlEncode(oCartAdd.SelectSingleNode("State").InnerText) & "&"
                                End If
                                cRequest = cRequest & "BillingPhone=" & goServer.UrlEncode(oCartAdd.SelectSingleNode("Telephone").InnerText) & "&"


                                oCartAdd = oOrder.SelectSingleNode("Contact[@type='Delivery Address']")
                                aGivenName = Split(oCartAdd.SelectSingleNode("GivenName").InnerText, " ")

                                Select Case UBound(aGivenName)
                                    Case 0
                                        LastName = aGivenName(0)
                                        FirstName = aGivenName(0)
                                    Case 1
                                        FirstName = aGivenName(0)
                                        LastName = aGivenName(1)
                                    Case 2
                                        FirstName = aGivenName(0)
                                        ' MiddleName = aGivenName(1)
                                        LastName = aGivenName(2)
                                    Case 3
                                        FirstName = aGivenName(0)
                                        'MiddleName = aGivenName(1)
                                        LastName = aGivenName(2)
                                        'Suffix = aGivenName(3)
                                End Select

                                cRequest = cRequest & "DeliverySurname=" & LastName & "&"
                                cRequest = cRequest & "DeliveryFirstnames=" & FirstName & "&"
                                cRequest = cRequest & "DeliveryAddress1=" & goServer.UrlEncode(oCartAdd.SelectSingleNode("Street").InnerText) & "&"
                                ' cRequest = cRequest & "BillingAddress2=" & goServer.UrlEncode(oCartAdd.SelectSingleNode("Street2").InnerText) & "&"
                                cRequest = cRequest & "DeliveryCity=" & goServer.UrlEncode(oCartAdd.SelectSingleNode("City").InnerText) & "&"
                                cRequest = cRequest & "DeliveryPostCode=" & goServer.UrlEncode(oCartAdd.SelectSingleNode("PostalCode").InnerText) & "&"
                                cRequest = cRequest & "DeliveryCountry=" & goServer.UrlEncode(oEwProv.getCountryISO2Code(oCartAdd.SelectSingleNode("Country").InnerText)) & "&"
                                If oEwProv.getCountryISO2Code(oCartAdd.SelectSingleNode("Country").InnerText) = "US" Then
                                    cRequest = cRequest & "DeliveryState=" & goServer.UrlEncode(oCartAdd.SelectSingleNode("State").InnerText) & "&"
                                End If

                                cRequest = cRequest & "DeliveryPhone=" & goServer.UrlEncode(oCartAdd.SelectSingleNode("Telephone").InnerText) & "&"

                                If oCartAdd.SelectSingleNode("Email").InnerText <> "" Then
                                    cRequest = cRequest & "CustomerEmail=" & goServer.UrlEncode(oCartAdd.SelectSingleNode("Email").InnerText) & "&"
                                Else
                                    cRequest = cRequest & "CustomerEmail=" & oDictOpt("MerchantEmail") & "&"
                                End If

                                cRequest = cRequest & "ApplyAVSCV2=" & oDictOpt("validateCV2") & "&"

                                ' This can be used to send the unique reference for the Partner that referred the Vendor to Sage Pay.
                                cRequest = cRequest & "ReferrerID=" & SagePayPartnerID & "&"

                                Dim cIP As String = myWeb.moRequest.ServerVariables("REMOTE_ADDR")
                                If cIP.Length < 4 Then cIP = "127.0.0.1"
                                cRequest = cRequest & "ClientIPAddress=" & cIP & "&"

                                cRequest = cRequest & "Apply3DSecure=" & oDictOpt("secure3d") & "&"

                                If bSavePayment Then
                                    cRequest = cRequest & "CreateToken=1&StoreToken=1&"
                                End If
                                Dim oBasketXml As New XmlDocument
                                Dim oBasketRoot As XmlElement = oBasketXml.CreateElement("basket")
                                oBasketXml.AppendChild(oBasketRoot)
                                Dim oItemElmt As XmlElement

                                For Each oItemElmt In oOrder.SelectNodes("Item")
                                    Dim oItemRoot As XmlElement = oBasketXml.CreateElement("item")
                                    Dim unitTaxAmount As Double = CDbl("0" & oItemElmt.GetAttribute("itemTax")) / CDbl("0" & oItemElmt.GetAttribute("quantity"))
                                    unitTaxAmount = Decimal.Round(unitTaxAmount, 2, MidpointRounding.AwayFromZero)

                                    Dim unitGrossAmount As Double = CDbl("0" & oItemElmt.GetAttribute("price")) + unitTaxAmount
                                    unitGrossAmount = Decimal.Round(unitGrossAmount, 2, MidpointRounding.AwayFromZero)

                                    Dim totalGrossAmount As Double = unitGrossAmount * CDbl("0" & oItemElmt.GetAttribute("quantity"))
                                    totalGrossAmount = Decimal.Round(totalGrossAmount, 2, MidpointRounding.AwayFromZero)

                                    Dim productDescription As String = oItemElmt.SelectSingleNode("Name").InnerText
                                    productDescription = Regex.Replace(productDescription, "[^A-Za-z0-9\-/]", "")
                                    If productDescription.Length > 100 Then
                                        productDescription = productDescription.Substring(0, 100)
                                    End If
                                    xmlTools.addNewTextNode("description", oItemRoot, productDescription)
                                    'If Not oItemElmt.SelectSingleNode("productDetail/StockCode") Is Nothing Then
                                    '    xmlTools.addNewTextNode("productSku", oItemRoot, oItemElmt.SelectSingleNode("productDetail/StockCode").InnerText)
                                    'End If
                                    'If Not oItemElmt.SelectSingleNode("productDetail/Manufacturer") Is Nothing Then
                                    '    xmlTools.addNewTextNode("productCode", oItemRoot, oItemElmt.SelectSingleNode("productDetail/Manufacturer").InnerText)
                                    'End If
                                    xmlTools.addNewTextNode("quantity", oItemRoot, oItemElmt.GetAttribute("quantity"))
                                    xmlTools.addNewTextNode("unitNetAmount", oItemRoot, oItemElmt.GetAttribute("price"))
                                    xmlTools.addNewTextNode("unitTaxAmount", oItemRoot, unitTaxAmount.ToString)
                                    xmlTools.addNewTextNode("unitGrossAmount", oItemRoot, unitGrossAmount.ToString)
                                    xmlTools.addNewTextNode("totalGrossAmount", oItemRoot, totalGrossAmount.ToString)
                                    oBasketRoot.AppendChild(oItemRoot)
                                Next

                                xmlTools.addNewTextNode("deliveryGrossAmount", oBasketRoot, oOrder.GetAttribute("shippingCost"))

                                cRequest = cRequest & "BasketXML=" & oBasketXml.OuterXml & "&"

                                cRequest = cRequest & "Website=" & goServer.UrlEncode(oEwProv.moCartConfig("SiteURL")) & "&"

                                'goSession("attemptCount") = nAttemptCount + 1
                            End If

                            ' Convert the request to bytes
                            If cRequest.EndsWith("&") Then cRequest = cRequest.Trim("&".ToCharArray())

                            byRequest = oEncoding.GetBytes(cRequest)

                            If b3dAuthorised Then
                                oRequest = HttpWebRequest.Create(sVSP3DSUrl)
                            Else
                                oRequest = HttpWebRequest.Create(sVSPUrl)
                            End If

                            oRequest.ContentType = "application/x-www-form-urlencoded"
                            oRequest.ContentLength = byRequest.Length
                            oRequest.Method = "POST"
                            oStream = oRequest.GetRequestStream
                            oStream.Write(byRequest, 0, byRequest.Length)
                            oStream.Close()

                            oResponse = oRequest.GetResponse()
                            oStream = oResponse.GetResponseStream()
                            oStreamReader = New StreamReader(oStream, System.Text.Encoding.UTF8)
                            cResponse = oStreamReader.ReadToEnd

                            oStreamReader.Close()
                            oResponse.Close()

                            myWeb.moSession("attemptCount") = nAttemptCount

                            ' Validate the response.

                            If cResponse = "" Or InStr(cResponse, "=") = 0 Then
                                err_msg = "There was a communications error."
                            Else
                                ' lets take the response and put it in a hash table
                                cProcessInfo = "Error translating response:" & cResponse
                                oResponseDict = UrlResponseToHashTable(cResponse, vbCrLf, "=", False)

                                nResult = oResponseDict("intStatus")

                                Select Case oResponseDict("Status").ToString

                                    Case "OK", "AUTHENTICATED", "REGISTERED"
                                        ' Successful Authorisation
                                        err_msg = "Payment was successful. Transaction ref: " & oResponseDict("VPSTxId")
                                        bIsValid = True

                                        myWeb.moSession("attemptCount") = Nothing

                                    Case "3DAUTH"

                                        'create an xform that automatically redirects to Aquiring Banks 3DS portal.
                                        'Save MD as paymentRef
                                        myWeb.moSession("VPSTxId") = oResponseDict("VPSTxId")
                                        myWeb.moSession("SecurityKey") = oResponseDict("SecurityKey")

                                        Dim sRedirectURL As String

                                        If oEwProv.moCartConfig("SecureURL").EndsWith("/") Then
                                            sRedirectURL = oEwProv.moCartConfig("SecureURL") & "?cartCmd=Redirect3ds"
                                        Else
                                            sRedirectURL = oEwProv.moCartConfig("SecureURL") & "/?cartCmd=Redirect3ds"
                                        End If

                                        bIsValid = False
                                        ccXform.valid = False

                                        err_msg_log = "ACS URL:" & oResponseDict("ACSURL") & " ACS URLDecoded:" & goServer.UrlDecode(oResponseDict("ACSURL")) & " MD:" & oResponseDict("MD") & " PAReq:" & oResponseDict("PAReq")
                                        err_msg = "This card has subscribed to 3D Secure. You will now be re-directed to your banks website for further verification."

                                        ccXform = oEwProv.xfrmSecure3D(goServer.UrlDecode(oResponseDict("ACSURL")), oResponseDict("MD"), oResponseDict("PAReq"), sRedirectURL)

                                    Case "MALFORMED", "INVALID", "NOTAUTHED", "REJECTED", "ERROR"
                                        ' Failed / Error Authorisation

                                        cMessage = oResponseDict("StatusDetail")

                                        If InStr(cMessage, "card number") > 0 Then ccXform.addNote("creditCard/number", xForm.noteTypes.Alert, "The card number given is not valid.")
                                        If InStr(cMessage, "IssueNumber") > 0 Then ccXform.addNote("creditCard/issueNumber", xForm.noteTypes.Alert, "The issue number is not valid - it may not be required for non-Switch/Solo cards.")
                                        If InStr(cMessage, "StartDate") > 0 Or InStr(cMessage, "Start Date") > 0 Then ccXform.addNote("creditCard/issueDate", xForm.noteTypes.Alert, "The issue date is not valid - it may not be required for Switch or Solo cards.")
                                        If InStr(cMessage, "ExpiryDate") > 0 Or InStr(cMessage, "Expiry Date") > 0 Then ccXform.addNote("creditCard/expireDate", xForm.noteTypes.Alert, "The expiry date is not valid.")

                                        err_msg_log = cMessage

                                        If gbDebug Then
                                            err_msg = "<br/>Full Request:" & goServer.HtmlEncode(goServer.UrlDecode(cRequest)) & "<br/>Full Response:" & goServer.HtmlEncode(goServer.UrlDecode(cResponse))
                                        Else
                                            err_msg = "There was an error processing this payment.<br/>  No payment has been made.<br/>  Please check the details you entered and try again, or call for assistance.<br/><br/>  The error returned fron the bank was :<br/><br/> " & goServer.HtmlEncode(goServer.UrlDecode(cMessage))
                                        End If

                                        ' goSession("attemptCount") = Nothing
                                    Case Else
                                        'Response not recognised.
                                        cMessage = oResponseDict("StatusDetail")

                                        If gbDebug Then
                                            err_msg = "<br/>Full Request:" & goServer.HtmlEncode(goServer.UrlDecode(cRequest)) & "<br/>Full Response:" & goServer.HtmlEncode(goServer.UrlDecode(cResponse))
                                        Else
                                            err_msg_log = "<br/>Full Request:" & goServer.HtmlEncode(goServer.UrlDecode(cRequest)) & "<br/>Full Response:" & goServer.HtmlEncode(goServer.UrlDecode(cResponse))
                                            err_msg = "There was an error processing this payment.  No payment has been made.  Please check the details you entered and try again, or call for assistance.  The error detail was : " & goServer.HtmlEncode(goServer.UrlDecode(cMessage))
                                        End If

                                        myWeb.moSession("attemptCount") = Nothing
                                End Select

                            End If

                            ccXform.addNote("creditCard", xForm.noteTypes.Alert, err_msg, True)

                            If bIsValid Then
                                cSellerNotes = Today & " " & TimeOfDay & ": changed to: (Payment Received) " & vbLf & "Transaction Ref:" & oResponseDict("VPSTxId") & vbLf & "comment: " & err_msg
                            Else
                                If err_msg_log = "" And err_msg <> "" Then
                                    err_msg_log = err_msg
                                End If
                                cSellerNotes = Today & " " & TimeOfDay & ": changed to: (Payment Failed) " & vbLf & "comment: " & err_msg_log
                            End If

                            ccXform.valid = bIsValid
                            'Save Payment

                            ' Set up the save payment.
                            ' Note from Ali - not really sure what's involved here and this falls over if 3d Secure is involved,
                            ' so need to have other options if coming back from 3d secure

                            If Not (b3dAuthorised) Then
                                Dim oDate As New Date("20" & Right(myWeb.moRequest("creditCard/expireDate"), 2), CInt(Left(myWeb.moRequest("creditCard/expireDate"), 2)), 1)
                                oDate = oDate.AddMonths(1)
                                oDate = oDate.AddDays(-1)
                                Dim cMethodName As String = myWeb.moRequest("creditCard/type") & ": " & MaskString(myWeb.moRequest("creditCard/number"), "*", False, 4) & " Expires: " & oDate.ToShortDateString

                                Dim oCustomElmt As XmlElement = ccXform.Instance.OwnerDocument.CreateElement("SagePayV3")

                                oCustomElmt.SetAttribute("VPSTxId", oResponseDict("VPSTxId"))
                                oCustomElmt.SetAttribute("SecurityKey", oResponseDict("SecurityKey"))
                                oCustomElmt.SetAttribute("TxAuthNo", oResponseDict("TxAuthNo"))
                                ccXform.Instance.FirstChild.AppendChild(oCustomElmt)


                                If bSavePayment And bIsValid Then
                                    oEwProv.savePayment(myWeb.mnUserId, "SagePayV3", oEwProv.mnCartId, cMethodName, ccXform.Instance.FirstChild, oDate, True, oEwProv.mnPaymentAmount)
                                Else
                                    oEwProv.savePayment(myWeb.mnUserId, "SagePayV3", oEwProv.mnCartId, cMethodName, ccXform.Instance.FirstChild, Now, False, oEwProv.mnPaymentAmount)
                                End If

                            End If

                        Else
                            If ccXform.isSubmitted Then
                                cSellerNotes = Today & " " & TimeOfDay & ": changed to: (Payment Form EonicWeb Validation Failed) " & vbLf & "comment: " & ccXform.validationError
                            Else
                                cSellerNotes = Today & " " & TimeOfDay & ": changed to: (Payment Form Presented) " & vbLf & "comment: " & err_msg_log
                            End If
                            ccXform.valid = False
                        End If

                        'Update Seller Notes:
                        sSql = "select * from tblCartOrder where nCartOrderKey = " & oEwProv.mnCartId
                        Dim oDs As DataSet
                        Dim oRow As DataRow
                        oDs = myWeb.moDbHelper.getDataSetForUpdate(sSql, "Order", "Cart")
                        For Each oRow In oDs.Tables("Order").Rows
                            oRow("cSellerNotes") = oRow("cSellerNotes") & vbLf & cSellerNotes
                        Next
                        myWeb.moDbHelper.updateDataset(oDs, "Order")

                        Return ccXform

                    Catch ex As Exception
                        returnException(mcModuleName, "GetPaymentForm", ex, "", cProcessInfo, gbDebug)
                        Return Nothing
                    End Try
                End Function

                Private Function fmtSecPayDate(ByVal sdate As String) As String
                    PerfMon.Log("PaymentProviders", "fmtSecPayDate")
                    Dim cProcessInfo As String = "fmtSecPayDate"
                    Dim strReturn As String = ""
                    Try
                        ' The dates are formatted "mm yyyy" - convert them to "mmyy"
                        If sdate <> "" Then strReturn = Left(sdate, 2) & Right(sdate, 2)
                        Return strReturn
                    Catch ex As Exception
                        returnException(mcModuleName, "fmtSecPayDate", ex, "", cProcessInfo, gbDebug)
                        Return ""

                    End Try
                End Function

                Public Function FormatCreditCardNumber(ByVal sCCNumber As String) As Long
                    PerfMon.Log("PaymentProviders", "FormatCreditCardNumber")
                    Dim cResult As String = ""
                    Dim oRE As Regex = New Regex("\D")
                    cResult = oRE.Replace(sCCNumber, "")
                    cResult = Replace(cResult, " ", "") 'also strip out spaces
                    If cResult = "" Then cResult = 0
                    Return cResult
                End Function

                Public Function FullMoneyString(ByVal amount As String) As String
                    Try
                        If Not amount.Contains(".") Then Return Replace(amount, ",", "")
                        amount = Replace(amount, ",", "")
                        Dim cAmounts() As String = Split(amount, ".")
                        If UBound(cAmounts) > 2 Then Return amount
                        amount = cAmounts(0) & "."
                        If cAmounts(1).Length < 2 Then
                            amount &= cAmounts(1) & "0"
                        Else
                            amount &= cAmounts(1)
                        End If
                        Return amount
                    Catch ex As Exception
                        returnException(mcModuleName, "FullMoneyString", ex, "", "", gbDebug)
                        Return amount
                    End Try

                End Function



                Public Function CheckStatus(ByRef oWeb As Protean.Cms, ByRef nPaymentProviderRef As String) As String
                    Dim cProcessInfo As String = ""
                    ' Dim moPaymentCfg = WebConfigurationManager.GetWebApplicationSection("protean/payment")
                    '  Dim oSagePayV3Cfg As XmlNode
                    '  Dim nTransactionMode As TransactionMode

                    Try



                    Catch ex As Exception
                        returnException(mcModuleName, "CheckStatus", ex, "", cProcessInfo, gbDebug)
                        Return ""
                    End Try

                End Function

                Public Function CancelPayments(ByRef oWeb As Protean.Cms, ByRef nPaymentProviderRef As String) As String
                    Dim cProcessInfo As String = ""
                    Dim moPaymentCfg = WebConfigurationManager.GetWebApplicationSection("protean/payment")
                    Dim oSagePayV3Cfg As XmlNode

                    Try



                    Catch ex As Exception
                        returnException(mcModuleName, "CancelPayments", ex, "", cProcessInfo, gbDebug)
                        Return ""
                    End Try

                End Function

            End Class
        End Class
    End Namespace
End Namespace



