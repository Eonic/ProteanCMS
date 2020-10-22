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
Imports Protean
Imports Protean.Cms.Cart
Imports System.Net
Imports CardinalCommerce

Namespace Providers
    Namespace Payment
        Public Class Pay360

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

                Dim _oAdXfm As Protean.Providers.Payment.Pay360.AdminXForms

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

                Private Const mcModuleName As String = "Providers.Payment.Pay360.Activities"
                Private myWeb As Protean.Cms
                Protected moPaymentCfg As XmlNode
                Private nTransactionMode As TransactionMode

                Enum TransactionMode
                    Live = 0
                    Test = 1
                    Fail = 2
                End Enum


                Private ScheduleIntervals() As String = {"yearly", "half-yearly", "quarterly", "monthly", "weekly", "daily"}



                Public Overloads Function GetPaymentForm(ByRef oWeb As Protean.Cms, ByRef oCart As Cms.Cart, ByRef oOrder As XmlElement, Optional returnCmd As String = "cartCmd=SubmitPaymentDetails") As xForm
                    PerfMon.Log("PaymentProviders", "GetPaymentForm")
                    Dim sSql As String

                    Dim ccXform As xForm

                    Dim Xform3dSec As xForm = Nothing

                    Dim bIsValid As Boolean
                    Dim err_msg As String = ""
                    Dim err_msg_log As String = ""
                    Dim sProcessInfo As String = ""
                    Dim aResponse() As String
                    Dim oDictResp As Hashtable
                    Dim cResponse As String

                    Dim oDictOpt As Hashtable = New Hashtable
                    Dim sSubmitPath As String = oCart.mcPagePath & returnCmd
                    Dim i As Integer
                    Dim nPos As Integer
                    Dim sOpts As String
                    Dim cOrderType As String
                    Dim bCv2 As Boolean = False
                    Dim b3DSecure As Boolean = False
                    Dim b3DAuthorised As Boolean = False
                    Dim sRedirectURL As String = ""
                    Dim sPaymentRef As String = ""

                    Dim cProcessInfo As String = "payGetPaymentForm"

                    'Get the payment options into a hashtable
                    Dim oPay360Cfg As XmlNode
                    Dim bSavePayment As Boolean = False
                    Dim bAllowSavePayment As Boolean = False
                    Dim sProfile As String = ""

                    Try
                        myWeb = oWeb
                        moPaymentCfg = WebConfigurationManager.GetWebApplicationSection("protean/payment")
                        oPay360Cfg = moPaymentCfg.SelectSingleNode("provider[@name='Pay360']")
                        oDictOpt = xmlTools.xmlToHashTable(oPay360Cfg, "value")


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

                        If sProfile <> "" Then
                            oPay360Cfg = moPaymentCfg.SelectSingleNode("provider[@name='Pay360' and @profile='" & sProfile & "']")
                        Else
                            oPay360Cfg = moPaymentCfg.SelectSingleNode("provider[@name='Pay360']")
                        End If

                        ' Get the payment options
                        'oPay360Cfg = moPaymentCfg.SelectSingleNode("provider[@name='SecPay']")
                        oDictOpt = xmlTools.xmlToHashTable(oPay360Cfg, "value")

                        Select Case oDictOpt("opperationMode")
                            Case "true"
                                nTransactionMode = TransactionMode.Test
                            Case "false"
                                nTransactionMode = TransactionMode.Fail
                            Case "live"
                                nTransactionMode = TransactionMode.Live
                        End Select

                        ' override the currency
                        If oDictOpt.ContainsKey("currency") Then oDictOpt.Remove("currency")
                        oDictOpt.Add("currency", oEwProv.mcCurrency)

                        ' Set common variables
                        If oDictOpt("validateCV2") = "on" Then bCv2 = True
                        If oDictOpt("secure3d") = "on" Then b3DSecure = True

                        If oDictOpt("allowSavePayment") = "on" Then bAllowSavePayment = True

                        'Load the Xform

                        ccXform = oEwProv.creditCardXform(oOrder, "PayForm", sSubmitPath, oDictOpt("cardsAccepted"), bCv2, "Make Payment of " & FormatNumber(oEwProv.mnPaymentAmount, 2) & " " & oDictOpt("currency") & " by Credit/Debit Card", b3DSecure, , , bAllowSavePayment)

                        If b3DSecure Then
                            'check for return from aquiring bank
                            If myWeb.moRequest("MD") <> "" Then
                                b3DAuthorised = True
                            End If
                        End If

                        If ccXform.valid Or b3DAuthorised Then


                            ' Set up card options
                            sOpts = "test_status=" & oDictOpt("opperationMode")
                            sOpts = sOpts & ",dups=false,card_type=" & myWeb.moRequest("creditCard/type")

                            ' Account for scheduled payments from the payment config.
                            Dim scheduleInterval As String = (oDictOpt("scheduleInterval") & "").ToLower
                            Dim scheduleMaxRepeats As String = oDictOpt("scheduleMaxRepeats") & ""
                            If Not String.IsNullOrEmpty(scheduleInterval) _
                                AndAlso Array.IndexOf(ScheduleIntervals, scheduleInterval) >= 0 Then


                                Dim maxRepeats As Integer
                                If Not String.IsNullOrEmpty(scheduleMaxRepeats) _
                                    AndAlso IsNumeric(scheduleMaxRepeats) _
                                    AndAlso Convert.ToInt16(scheduleMaxRepeats) > 0 Then
                                    maxRepeats = scheduleMaxRepeats
                                Else
                                    maxRepeats = -1
                                End If

                                ' We need to send through an immediate payment - ie, the actual payment
                                ' and then schedule the same payment based on the interval
                                Dim scheduleDate As Date
                                Select Case scheduleInterval

                                    Case "yearly"
                                        scheduleDate = Today.AddYears(1)
                                    Case "half-yearly"
                                        scheduleDate = Today.AddMonths(6)
                                    Case "quarterly"
                                        scheduleDate = Today.AddMonths(3)
                                    Case "monthly"
                                        scheduleDate = Today.AddMonths(1)
                                    Case "weekly"
                                        scheduleDate = Today.AddDays(7)
                                    Case "daily"
                                        scheduleDate = Today.AddDays(1)
                                End Select

                                sOpts &= ",repeat=" & Format(scheduleDate, "yyyyMMdd")
                                sOpts &= "/" & scheduleInterval
                                sOpts &= "/" & maxRepeats.ToString
                                sOpts &= ":" & oEwProv.mnPaymentAmount.ToString

                            End If


                            ' Currency - if no currency then use GBP
                            If oEwProv.mcCurrency <> "" Then
                                sOpts = sOpts & ",currency=" & UCase(oEwProv.mcCurrency)
                            Else
                                sOpts = sOpts & ",currency=GBP"
                            End If

                            ' Optional - CV2
                            If bCv2 Then
                                sOpts = sOpts & ",cv2=" & myWeb.moRequest("creditCard/CV2")
                            End If
                            ' Optional - 3DSecure
                            If oDictOpt("opperationMode") = "true" And b3DSecure Then
                                sOpts = sOpts & ",test_mpi_status=true"
                            End If

                            ' If test mode, then we must turn on cv2-avs checks - mandatory Visa mandate May 2009
                            If LCase(oDictOpt("opperationMode")) = "true" Or LCase(oDictOpt("opperationMode")) = "false" Then
                                sOpts = sOpts & ",default_cv2avs=ALL MATCH"
                            End If

                            If LCase(oDictOpt("transactionType")) = "defer" Or LCase(oDictOpt("transactionType")) = "deferred" Then
                                If IsNumeric(oDictOpt("ccDeferDays")) And IsNumeric(oDictOpt("dcDeferDays")) Then
                                    sOpts &= ",deferred=reuse:" & oDictOpt("ccDeferDays") & ":" & oDictOpt("dcDeferDays")
                                Else
                                    sOpts &= ",deferred=true"
                                End If
                            End If
                            If oDictOpt("digest") = "on" Then
                                Dim cDigest As String = ""
                                If Not oDictOpt("accountId") = "secpay" Then
                                    cDigest = CStr(oEwProv.mnCartId) & CStr(oEwProv.mnPaymentAmount) & CStr(oDictOpt("accountPassword"))
                                Else
                                    cDigest = "secpay"
                                End If
                                Dim encode As New System.Text.UnicodeEncoding
                                Dim inputDigest() As Byte = encode.GetBytes(cDigest)

                                Dim hash() As Byte
                                ' get hash
                                Dim md5 As New System.Security.Cryptography.MD5CryptoServiceProvider
                                hash = md5.ComputeHash(inputDigest)

                                ' convert hash value to hex string
                                Dim sb As New System.Text.StringBuilder
                                Dim outputByte As Byte
                                For Each outputByte In hash
                                    ' convert each byte to a Hexadecimal upper case string
                                    sb.Append(outputByte.ToString("x2"))
                                Next outputByte

                                sOpts &= ",digest=" & sb.ToString

                            End If

                            Dim oSecVpn As Paypoint.SECVPNClient = New Paypoint.SECVPNClient

                            cOrderType = oOrder.GetAttribute("orderType")
                            If Not (cOrderType <> "" And CStr(oDictOpt("UseOrderType")) = "true") Then cOrderType = CStr(oDictOpt("accountId"))
                            If Not b3DSecure Then
                                cResponse = oSecVpn.validateCardFull(cOrderType,
                                CStr(oDictOpt("accountPassword")),
                                CStr(oEwProv.mnCartId),
                                myWeb.moRequest.ServerVariables("REMOTE_ADDR"),
                                oEwProv.mcCardHolderName,
                                myWeb.moRequest("creditCard/number"),
                                CStr(oEwProv.mnPaymentAmount),
                                fmtSecPayDate(myWeb.moRequest("creditCard/expireDate")),
                                myWeb.moRequest("creditCard/issueNumber"),
                                fmtSecPayDate(myWeb.moRequest("creditCard/issueDate")),
                                getPay360Order(oOrder),
                                getPay360Address(oOrder, "Delivery Address"),
                                getPay360Address(oOrder, "Billing Address"),
                                sOpts)

                                bSavePayment = True

                            Else
                                If Not b3DAuthorised Then
                                    cResponse = oSecVpn.threeDSecureEnrolmentRequest(cOrderType,
                                                        CStr(oDictOpt("accountPassword")),
                                                        CStr(oEwProv.mnCartId),
                                                        myWeb.moRequest.ServerVariables("REMOTE_ADDR"),
                                                        oEwProv.mcCardHolderName,
                                                        myWeb.moRequest("creditCard/number"),
                                                        CStr(oEwProv.mnPaymentAmount),
                                                        fmtSecPayDate(myWeb.moRequest("creditCard/expireDate")),
                                                        myWeb.moRequest("creditCard/issueNumber"),
                                                        fmtSecPayDate(myWeb.moRequest("creditCard/issueDate")),
                                                        getPay360Order(oOrder),
                                                        getPay360Address(oOrder, "Delivery Address"),
                                                        getPay360Address(oOrder, "Billing Address"),
                                                        sOpts,
                                                        "0",
                                                        myWeb.moRequest.ServerVariables("HTTP_ACCEPT"),
                                                        myWeb.moRequest.ServerVariables("HTTP_USER_AGENT"),
                                                        "",
                                                        "",
                                                        "",
                                                        "",
                                                        "",
                                                        "")

                                    bSavePayment = True

                                Else
                                    'Pass the process back to Secpay
                                    cResponse = oSecVpn.threeDSecureAuthorisationRequest(cOrderType,
                                    CStr(oDictOpt("accountPassword")),
                                    CStr(oEwProv.mnCartId),
                                    myWeb.moRequest("MD"),
                                    myWeb.moRequest("PaRes"),
                                    sOpts)
                                    'Save in the session the MD and the instance to save

                                End If
                            End If

                            ' Parse the response
                            oDictResp = New Hashtable

                            'cResponse = Replace(oXMLHttp.responseXML.selectSingleNode("//node()[local-name()='validateCardFullReturn']").Text, "+", " ")
                            Dim cAuthCode As String = ""
                            If cResponse <> "" Then

                                aResponse = Split(Right(cResponse, Len(cResponse) - 1), "&")

                                For i = 0 To UBound(aResponse)
                                    Dim cPos As String = InStr(aResponse(i), "=")
                                    If IsNumeric(cPos) Then
                                        nPos = CInt(cPos)
                                        oDictResp.Add(Left(aResponse(i), nPos - 1), Right(aResponse(i), Len(aResponse(i)) - nPos))
                                    Else
                                        oDictResp.Add(Trim(aResponse(i)), "")
                                    End If
                                Next


                                If oDictResp("valid") = "true" Then
                                    If Not b3DSecure Then
                                        bIsValid = True
                                        err_msg = "Payment Authorised Ref: " & CStr(oEwProv.mnCartId)
                                        ccXform.valid = True

                                    Else
                                        Select Case oDictResp("mpi_status_code")
                                            Case "200"
                                                'we have to get the browser to redirect
                                                ' v4 change - don't explicitly redirect to /deafault.ashx - this breaks everything.
                                                ' AJG Remove defualt.ashx from RedirectUrl Compare this line to 4.1
                                                If oEwProv.moCartConfig("SecureURL").EndsWith("/") Then
                                                    sRedirectURL = oEwProv.moCartConfig("SecureURL") & "?cartCmd=Redirect3ds"
                                                Else
                                                    sRedirectURL = oEwProv.moCartConfig("SecureURL") & "/?cartCmd=Redirect3ds"
                                                End If

                                                Dim cleanACSURL As String = myWeb.goServer.UrlDecode(oDictResp("acs_url"))
                                                cleanACSURL = Replace(cleanACSURL, "&amp;", "&")

                                                bIsValid = False
                                                ccXform.valid = False
                                                err_msg = "customer redirected to:" & cleanACSURL

                                                'Save MD as paymentRef
                                                sPaymentRef = oDictResp("MD")
                                                'Save the payment instance in the session

                                                Xform3dSec = oEwProv.xfrmSecure3D(oDictResp("acs_url"), oDictResp("MD"), oDictResp("PaReq"), sRedirectURL)

                                            Case "212"
                                                'not subscribes to 3D Secure
                                                bIsValid = True
                                                err_msg = "Payment Authorised Ref: " & CStr(oEwProv.mnCartId)
                                                err_msg = err_msg & " 3D Secure:" & oDictResp("mpi_message")
                                                ccXform.valid = True

                                            Case "237"
                                                'Payer Authenticated
                                                bIsValid = True
                                                err_msg = "Payment Authorised Ref: " & CStr(oEwProv.mnCartId)
                                                err_msg = err_msg & " 3D Secure:" & oDictResp("mpi_message")
                                                ccXform.valid = True

                                            Case "236"
                                                ' Payer Declined 3D Secure but Proceeded to confirm 
                                                ' the(authentication)
                                                bIsValid = True
                                                err_msg = "Payment Authorised Ref: " & CStr(oEwProv.mnCartId)
                                                err_msg = err_msg & " 3D Secure:" & oDictResp("mpi_message")
                                                ccXform.valid = True


                                            Case "234"
                                                ' unable to verify erolement but secpay passes
                                                bIsValid = True
                                                err_msg = "Payment Authorised Ref: " & CStr(oEwProv.mnCartId)
                                                err_msg = err_msg & " 3D Secure:" & oDictResp("mpi_message")
                                                ccXform.valid = True


                                            Case "229"
                                                'Payer Not Authenticated
                                                bIsValid = False
                                                ccXform.valid = False
                                                err_msg = err_msg & " 3D Secure:" & oDictResp("mpi_message")

                                            Case Else
                                                If oDictResp("code") = "A" Then
                                                    bIsValid = True
                                                    err_msg = "Payment Authorised Ref: " & CStr(oEwProv.mnCartId)
                                                    err_msg = err_msg & " 3D Secure:" & oDictResp("mpi_message")
                                                    ccXform.valid = True
                                                Else
                                                    'Payer Not Authenticated
                                                    bIsValid = False
                                                    ccXform.valid = False
                                                    err_msg = err_msg & " 3D Secure:" & oDictResp("mpi_status_code") & " - " & oDictResp("mpi_message")
                                                End If
                                        End Select
                                    End If
                                Else
                                    ccXform.valid = False
                                    bIsValid = False
                                    err_msg_log = "Payment Failed : " & oDictResp("message") & " (Code::" & oDictResp("code") & ")"

                                    ' Produce nice format error messages.
                                    Select Case oDictResp("code")
                                        Case "N"
                                            err_msg = "The transaction was not authorised by your payment provider."
                                        Case "C"
                                            err_msg = "There was a comunication problem. Please try resubmitting your order later."
                                        Case "P:A"
                                            err_msg = "There was a system error - the amount was not supplied or invalid.  Please call for assistance."
                                        Case "P:X"
                                            err_msg = "There was a system error - not all the mandatory parameters were supplied.  Please call for assistance."
                                        Case "P:P"
                                            err_msg = "The payment has already been processed.  This is a duplicate payment, and will not be processed."
                                        Case "P:S"
                                            err_msg = "The start date is invalid.  Please check that you have entered your card details correctly."
                                        Case "P:E"
                                            err_msg = "The expiry date is invalid.  Please check that you have entered your card details correctly."
                                        Case "P:I"
                                            err_msg = "The issue number is invalid.  Please check that you have entered your card details correctly."
                                        Case "P:C"
                                            err_msg = "The card number supplied is invalid.  Please check that you have entered your card details correctly."
                                        Case "P:T"
                                            err_msg = "The card type does not match the card number entered.  Please check that you have entered your card details correctly."
                                        Case "P:N"
                                            err_msg = "There was a system error - the customer name was not supplied.  Please call for assistance."
                                        Case "P:M"
                                            err_msg = "There was a system error - the merchant account deos not exist or has not been registered.  Please call for assistance."
                                        Case "P:B"
                                            err_msg = "There was a system error - the merchant account for this card type does not exist.  Please call for assistance."
                                        Case "P:D"
                                            err_msg = "There was a system error - the merchant account for this currency does not exist.  Please call for assistance."
                                        Case "P:V"
                                            err_msg = "The security code is invalid. Please check that you have entered your card details correctly. The security code can be found on the back of your card and is the last 3 digits of the series of digits on the back."
                                        Case "P:R"
                                            err_msg = "There was a communication problem and the transaction has timed out.  Please try resubmitting your order later."
                                        Case "P:#"
                                            err_msg = "There was a system error - no encryption key has been set up against this account.  Please call for assistance."
                                        Case Else
                                            err_msg = "There was an unspecified error. Please call for assistance.(code::" & oDictResp("code") & " | " & oDictResp("message")
                                    End Select

                                    err_msg = "Payment Failed : " & err_msg

                                End If
                            Else
                                bIsValid = False
                                ccXform.valid = False
                                err_msg = "Payment Failed : no response from Pay360 check settings and password"
                            End If


                            If bSavePayment Then
                                'We Save the payment method prior to ultimate validation because we only have access to the request object at the point it is submitted

                                'only do this for a valid payment method
                                Dim oSaveElmt As XmlElement = ccXform.Instance.SelectSingleNode("creditCard/bSavePayment")
                                Debug.WriteLine(myWeb.moRequest("creditCard/expireDate"))
                                Dim oDate As New Date("20" & Right(myWeb.moRequest("creditCard/expireDate"), 2), CInt(Left(myWeb.moRequest("creditCard/expireDate"), 2)), 1)
                                oDate = oDate.AddMonths(1)
                                oDate = oDate.AddDays(-1)
                                Dim cMethodName As String = myWeb.moRequest("creditCard/type") & ": " & MaskString(myWeb.moRequest("creditCard/number"), "*", False, 4) & " Expires: " & oDate.ToShortDateString
                                cAuthCode = oDictResp("auth_code")

                                ' Dim oPay360Elmt As XmlElement = ccXform.Instance.OwnerDocument.CreateElement("Pay360")
                                ' oPay360Elmt.SetAttribute("AuthCode", cAuthCode)
                                ' ccXform.Instance.FirstChild.AppendChild(oPay360Elmt)

                                If Not oSaveElmt Is Nothing Then
                                    If oSaveElmt.InnerText = "true" And bIsValid Then
                                        oCart.mnPaymentId = oEwProv.savePayment(myWeb.mnUserId, "Pay360", oEwProv.mnCartId, cMethodName, ccXform.Instance.FirstChild, oDate, True, oEwProv.mnPaymentAmount)
                                    Else
                                        oCart.mnPaymentId = oEwProv.savePayment(myWeb.mnUserId, "Pay360", oEwProv.mnCartId, cMethodName, ccXform.Instance.FirstChild, Now, False, oEwProv.mnPaymentAmount)
                                    End If
                                Else
                                    oCart.mnPaymentId = oEwProv.savePayment(myWeb.mnUserId, "Pay360", oEwProv.mnCartId, cMethodName, ccXform.Instance.FirstChild, Now, False, oEwProv.mnPaymentAmount)
                                End If

                            End If

                            ccXform.addNote(ccXform.moXformElmt, xForm.noteTypes.Alert, err_msg)



                        Else
                            If ccXform.isSubmitted And ccXform.validationError = "" Then
                                err_msg = "Unknown Error: Please call"
                                ccXform.addNote(ccXform.moXformElmt, xForm.noteTypes.Alert, err_msg)
                            Else
                                err_msg = ccXform.validationError
                            End If
                            ccXform.valid = False
                        End If

                        If ccXform.isSubmitted Or b3DAuthorised Then
                            'Update Seller Notes:
                            sSql = "select * from tblCartOrder where nCartOrderKey = " & oEwProv.mnCartId
                            Dim oDs As DataSet
                            Dim oRow As DataRow
                            oDs = myWeb.moDbHelper.getDataSetForUpdate(sSql, "Order", "Cart")
                            For Each oRow In oDs.Tables("Order").Rows
                                If bIsValid Or b3DAuthorised Then
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
                        End If

                        If Not Xform3dSec Is Nothing Then
                            Return Xform3dSec
                        Else
                            Return ccXform
                        End If

                    Catch ex As Exception
                        returnException(myWeb.msException, mcModuleName, "GetPaymentForm", ex, "", cProcessInfo, gbDebug)
                        Return Nothing
                    End Try
                End Function


                Private Function getPay360Address(ByRef oRoot As XmlElement, ByRef sType As String) As String
                    PerfMon.Log("PaymentProviders", "getSecPayAddress")
                    Dim oCartAdd As XmlElement

                    Dim sAddress As String
                    Dim sPrefix As String
                    Dim cProcessInfo As String = "getSecPayAddress"
                    Try
                        sAddress = ""

                        oCartAdd = oRoot.SelectSingleNode("Contact[@type='" & sType & "']")

                        If sType = "Delivery" Then sType = "Shipping"

                        If Not oCartAdd Is Nothing Then

                            sPrefix = LCase(Left(sType, 4)) & "_"

                            'given name
                            If Not oCartAdd.SelectSingleNode("GivenName") Is Nothing Then
                                sAddress = sAddress & sPrefix & "name=" & Replace(oCartAdd.SelectSingleNode("GivenName").InnerText, ",", "&comma;") & ","
                            End If

                            If Not oCartAdd.SelectSingleNode("Company") Is Nothing Then
                                sAddress = sAddress & sPrefix & "company=" & Replace(oCartAdd.SelectSingleNode("Company").InnerText, ",", "&comma;") & ","
                            End If

                            Dim cStreet() As String = Split(oCartAdd.SelectSingleNode("Street").InnerText, ",")
                            Select Case UBound(cStreet)
                                Case 0
                                    If Not oCartAdd.SelectSingleNode("Street") Is Nothing Then
                                        sAddress = sAddress & sPrefix & "addr_1=" & cStreet(0) & ","
                                    End If
                                Case 1
                                    If Not oCartAdd.SelectSingleNode("Street") Is Nothing Then
                                        sAddress = sAddress & sPrefix & "addr_1=" & cStreet(0) & ","
                                        sAddress = sAddress & sPrefix & "addr_2=" & cStreet(1) & ","
                                    End If
                                Case Else
                                    If Not oCartAdd.SelectSingleNode("Street") Is Nothing Then
                                        sAddress = sAddress & sPrefix & "addr_1=" & cStreet(0) & ","
                                        'remove commas AVC doesn't like em.
                                        sAddress = sAddress & sPrefix & "addr_2=" & Replace(Replace(oCartAdd.SelectSingleNode("Street").InnerText, cStreet(0), ""), ",", " ") & ","
                                    End If
                            End Select

                            If Not oCartAdd.SelectSingleNode("City") Is Nothing Then
                                sAddress = sAddress & sPrefix & "city=" & Replace(oCartAdd.SelectSingleNode("City").InnerText, ",", "&comma;") & ","
                            End If

                            If Not oCartAdd.SelectSingleNode("State") Is Nothing Then
                                sAddress = sAddress & sPrefix & "state=" & Replace(oCartAdd.SelectSingleNode("State").InnerText, ",", "&comma;") & ","
                            End If

                            If Not oCartAdd.SelectSingleNode("Country") Is Nothing Then
                                sAddress = sAddress & sPrefix & "country=" & Replace(oCartAdd.SelectSingleNode("Country").InnerText, ",", "&comma;") & ","
                            End If

                            If Not oCartAdd.SelectSingleNode("PostalCode") Is Nothing Then
                                sAddress = sAddress & sPrefix & "post_code=" & Replace(oCartAdd.SelectSingleNode("PostalCode").InnerText, ",", "&comma;") & ","
                            End If

                            If Not oCartAdd.SelectSingleNode("Telephone") Is Nothing Then
                                sAddress = sAddress & sPrefix & "tel=" & Replace(oCartAdd.SelectSingleNode("Telephone").InnerText, ",", "&comma;") & ","
                            End If

                            If Not oCartAdd.SelectSingleNode("Email") Is Nothing Then
                                sAddress = sAddress & sPrefix & "email=" & Replace(oCartAdd.SelectSingleNode("Email").InnerText, ",", "&comma;") & ","
                            End If

                            If sAddress <> "" Then sAddress = Left(sAddress, Len(sAddress) - 1)

                        End If

                        Return sAddress
                    Catch ex As Exception
                        returnException(myWeb.msException, mcModuleName, "getSecPayAddress", ex, "", cProcessInfo, gbDebug)
                        Return Nothing
                    End Try

                End Function

                Private Function getPay360Order(ByRef oRoot As XmlElement) As String

                    PerfMon.Log("PaymentProviders", "getSecPayOrder")
                    Dim sOrder As String
                    Dim cProcessInfo As String = "getSecPayOrder"
                    Dim oItem As XmlElement
                    Try
                        sOrder = ""
                        For Each oItem In oRoot.SelectNodes("Item")
                            sOrder = sOrder & "prod=" & Replace(oItem.SelectSingleNode("Name").InnerText, " ", "_")
                            sOrder = sOrder & ", item_amount=" & oItem.SelectSingleNode("@price").InnerText & "x" & oItem.SelectSingleNode("@quantity").InnerText & ";"
                        Next
                        'add the shipping
                        If CInt("0" & oRoot.SelectSingleNode("@shippingCost").InnerText) > 0 Then
                            sOrder = sOrder & "prod=SHIPPING,item_amount=" & oRoot.SelectSingleNode("@shippingCost").InnerText & "x1;"
                        End If
                        'add the tax
                        If CInt("0" & oRoot.SelectSingleNode("@vatAmt").InnerText) > 0 Then
                            sOrder = sOrder & "prod=TAX,item_amount=" & oRoot.SelectSingleNode("@vatAmt").InnerText & "x1;"
                        End If

                        'strip the trailing semiColon
                        sOrder = Left(sOrder, Len(sOrder) - 1)
                        Return sOrder

                    Catch ex As Exception
                        returnException(myWeb.msException, mcModuleName, "getSecPayOrder", ex, "", cProcessInfo, gbDebug)
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
                        returnException(myWeb.msException, mcModuleName, "fmtSecPayDate", ex, "", cProcessInfo, gbDebug)
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
                        returnException(myWeb.msException, mcModuleName, "FullMoneyString", ex, "", "", gbDebug)
                        Return amount
                    End Try

                End Function



                Public Function CheckStatus(ByRef oWeb As Protean.Cms, ByRef nPaymentProviderRef As String) As String
                    Dim cProcessInfo As String = ""
                    ' Dim moPaymentCfg = WebConfigurationManager.GetWebApplicationSection("protean/payment")
                    '  Dim oSagePayV3Cfg As XmlNode
                    '  Dim nTransactionMode As TransactionMode

                    Try
                        'moPaymentCfg = WebConfigurationManager.GetWebApplicationSection("protean/payment")
                        'oPay360Cfg = moPaymentCfg.SelectSingleNode("provider[@name='Pay360']")
                        'oDictOpt = xmlTools.xmlToHashTable(oPay360Cfg, "value")

                        'Dim oSecVpn As Paypoint.SECVPNClient = New Paypoint.SECVPNClient
                        'Dim cResponse As String

                        'cResponse = oSecVpn.repeatCardFull(cOrderType,
                        '       CStr(oDictOpt("accountPassword")),
                        '       CStr(oEwProv.mnCartId),
                        '       myWeb.moRequest.ServerVariables("REMOTE_ADDR"),
                        '       oEwProv.mcCardHolderName,
                        '       myWeb.moRequest("creditCard/number"),
                        '       CStr(oEwProv.mnPaymentAmount),
                        '       fmtSecPayDate(myWeb.moRequest("creditCard/expireDate")),
                        '       myWeb.moRequest("creditCard/issueNumber"),
                        '       fmtSecPayDate(myWeb.moRequest("creditCard/issueDate")),
                        '       getPay360Order(oOrder),
                        '       getPay360Address(oOrder, "Delivery Address"),
                        '       getPay360Address(oOrder, "Billing Address"),
                        '       sOpts)

                        Return "Active"

                    Catch ex As Exception
                        returnException(myWeb.msException, mcModuleName, "CheckStatus", ex, "", cProcessInfo, gbDebug)
                        Return ""
                    End Try

                End Function

                Public Function CollectPayment(ByRef oWeb As Protean.Cms, ByVal nPaymentMethodId As Long, ByVal Amount As Double, ByVal CurrencyCode As String, ByVal PaymentDescription As String, ByRef oOrder As Protean.Cms.Cart) As String
                    Dim cProcessInfo As String = ""
                    Dim oPay360Cfg As XmlElement
                    Dim oDictOpt As Hashtable = New Hashtable
                    Dim sOpts As String
                    Try


                        moPaymentCfg = WebConfigurationManager.GetWebApplicationSection("protean/payment")
                        oPay360Cfg = moPaymentCfg.SelectSingleNode("provider[@name='Pay360']")
                        oDictOpt = xmlTools.xmlToHashTable(oPay360Cfg, "value")
                        Dim RemotePassword As String = ""
                        Dim sToken As String = myWeb.moDbHelper.GetDataValue("select cPayMthdProviderRef from tblCartPaymentMethod where nPayMthdKey = " & nPaymentMethodId)
                        Dim cardXml As New XmlDocument
                        cardXml.LoadXml(myWeb.moDbHelper.GetDataValue("select cPayMthdProviderRef from tblCartPaymentMethod where nPayMthdKey = " & nPaymentMethodId))

                        Dim cardExpireDate As String = cardXml.SelectSingleNode("creditCard/expireDate").InnerText
                        Dim cardType As String = cardXml.SelectSingleNode("creditCard/type").InnerText
                        Dim CV2 As String = cardXml.SelectSingleNode("creditCard/CV2").InnerText

                        sOpts = "test_status=" & oDictOpt("opperationMode")
                        sOpts = sOpts & ",dups=false,card_type=" & cardType

                        ' Currency - if no currency then use GBP
                        If oOrder.mcCurrency <> "" Then
                            sOpts = sOpts & ",currency=" & UCase(oOrder.mcCurrency)
                        Else
                            sOpts = sOpts & ",currency=GBP"
                        End If

                        If CV2 <> "" Then
                            sOpts = sOpts & ",cv2=" & CV2
                        End If

                        Dim oSecVpn As Paypoint.SECVPNClient = New Paypoint.SECVPNClient
                        Dim cResponse As String
                        cResponse = oSecVpn.repeatCardFull(CStr(oDictOpt("accountId")),
                               CStr(oDictOpt("accountPassword")),
                               sToken,
                               Amount,
                               oDictOpt("remotePassword"),
                               fmtSecPayDate(cardExpireDate),
                               oOrder.mnCartId,
                               sOpts
                               )

                        Dim aResponse As String() = Split(Right(cResponse, Len(cResponse) - 1), "&")
                        Dim i As Int16
                        Dim nPos As Int16
                        Dim oDictResp As New Hashtable
                        For i = 0 To UBound(aResponse)
                            Dim cPos As String = InStr(aResponse(i), "=")
                            If IsNumeric(cPos) Then
                                nPos = CInt(cPos)
                                oDictResp.Add(Left(aResponse(i), nPos - 1), Right(aResponse(i), Len(aResponse(i)) - nPos))
                            Else
                                oDictResp.Add(Trim(aResponse(i)), "")
                            End If
                        Next

                        Dim bIsValid As Boolean = False
                        Dim err_msg As String = ""

                        If oDictResp("valid") = "true" Then
                            Select Case oDictResp("mpi_status_code")

                                Case "212"
                                    'not subscribes to 3D Secure
                                    bIsValid = True
                                    err_msg = "Payment Authorised Ref: " & CStr(oOrder.mnCartId)
                                    err_msg = err_msg & " 3D Secure:" & oDictResp("mpi_message")

                                Case "237"
                                    'Payer Authenticated
                                    bIsValid = True
                                    err_msg = "Payment Authorised Ref: " & CStr(oOrder.mnCartId)
                                    err_msg = err_msg & " 3D Secure:" & oDictResp("mpi_message")

                                Case "236"
                                    ' Payer Declined 3D Secure but Proceeded to confirm 
                                    ' the(authentication)
                                    bIsValid = True
                                    err_msg = "Payment Authorised Ref: " & CStr(oOrder.mnCartId)
                                    err_msg = err_msg & " 3D Secure:" & oDictResp("mpi_message")



                                Case "234"
                                    ' unable to verify erolement but secpay passes
                                    bIsValid = True
                                    err_msg = "Payment Authorised Ref: " & CStr(oOrder.mnCartId)
                                    err_msg = err_msg & " 3D Secure:" & oDictResp("mpi_message")


                                Case "229"
                                    'Payer Not Authenticated
                                    bIsValid = False
                                    err_msg = err_msg & " 3D Secure:" & oDictResp("mpi_message")

                                Case Else
                                    If oDictResp("code") = "A" Then
                                        bIsValid = True
                                        err_msg = "Payment Authorised Ref: " & CStr(oOrder.mnCartId)
                                        err_msg = err_msg & " 3D Secure:" & oDictResp("mpi_message")

                                    Else
                                        'Payer Not Authenticated
                                        bIsValid = False
                                        err_msg = err_msg & " 3D Secure:" & oDictResp("mpi_status_code") & " - " & oDictResp("mpi_message")
                                    End If
                            End Select
                        End If

                        If bIsValid Then
                            Return "Success"
                        Else
                            Return err_msg
                        End If

                    Catch ex As Exception
                        returnException(myWeb.msException, mcModuleName, "CollectPayment", ex, "", cProcessInfo, gbDebug)
                        Return "Payment Error"
                    End Try
                End Function

                Public Function CancelPayments(ByRef oWeb As Protean.Cms, ByRef nPaymentProviderRef As String) As String
                    Dim cProcessInfo As String = ""
                    Dim moPaymentCfg = WebConfigurationManager.GetWebApplicationSection("protean/payment")
                    '   Dim oPay360Cfg As XmlNode

                    Try



                    Catch ex As Exception
                        returnException(myWeb.msException, mcModuleName, "CancelPayments", ex, "", cProcessInfo, gbDebug)
                        Return ""
                    End Try

                End Function

            End Class
        End Class
    End Namespace
End Namespace



