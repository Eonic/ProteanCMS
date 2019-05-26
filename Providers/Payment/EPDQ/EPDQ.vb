Imports System.Xml
Imports System.Web.HttpUtility
Imports System.Web.Configuration
Imports System.IO
Imports System.Collections
Imports System.Data
Imports System.Data.SqlClient
Imports VB = Microsoft.VisualBasic
Imports Eonic
Imports Eonic.Web.Cart
Imports System.Net


Public Class EPDQ

    Public Sub New()
        'do nothing
    End Sub

    Public Sub Initiate(ByRef _AdminXforms As Object, ByRef _AdminProcess As Object, ByRef _Activities As Object, ByRef MemProvider As Object, ByRef myWeb As Web)

        MemProvider.AdminXforms = New AdminXForms(myWeb)
        MemProvider.AdminProcess = New AdminProcess(myWeb)
        MemProvider.AdminProcess.oAdXfm = MemProvider.AdminXforms
        MemProvider.Activities = New Activities()

    End Sub

    Public Class AdminXForms
        Inherits Web.Admin.AdminXforms
        Private Const mcModuleName As String = "Providers.Providers.Eonic.AdminXForms"

        Sub New(ByRef aWeb As Web)
            MyBase.New(aWeb)
        End Sub

    End Class

    Public Class AdminProcess
        Inherits Web.Admin

        Dim _oAdXfm As Eonic.Providers.Payment.EPDQ.AdminXForms

        Public Property oAdXfm() As Object
            Set(ByVal value As Object)
                _oAdXfm = value
            End Set
            Get
                Return _oAdXfm
            End Get
        End Property

        Sub New(ByRef aWeb As Web)
            MyBase.New(aWeb)
        End Sub
    End Class


    Public Class Activities
        Private Const mcModuleName As String = "Providers.Payment.EPDQ.Activities"
        Private myWeb As Eonic.Web


        Public Function GetPaymentForm(ByRef oWeb As Eonic.Web, ByRef oCart As Web.Cart, ByRef oOrder As XmlElement) As xForm
            Dim cProcessInfo As String = ""

            Try
                myWeb = oWeb
                Dim moCartConfig As System.Collections.Specialized.NameValueCollection = oCart.moCartConfig
                Dim mcPaymentMethod As String = oCart.mcPaymentMethod

                Dim sSubmitPath = oCart.mcPagePath & "cartCmd=SubmitPaymentDetails"
                Dim ccXform As xForm
                Dim oEwProv As Eonic.Web.Cart.PaymentProviders = New PaymentProviders(myWeb)

                Dim moPaymentCfg = WebConfigurationManager.GetWebApplicationSection("eonic/payment")
                Dim oePDQCfg As XmlElement = moPaymentCfg.SelectSingleNode("provider[@name='ePDQ']")

                Dim mnCartId = oCart.mnCartId
                Dim err_msg As String


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

                ccXform = oEwProv.creditCardXform(oOrder, "PayForm", sSubmitPath, oePDQCfg.SelectSingleNode("cardsAccepted").Attributes("value").Value, True, "Make Payment of " & FormatNumber(oEwProv.mnPaymentAmount, 2) & " " & oePDQCfg.SelectSingleNode("currency").Attributes("value").Value & " by Credit/Debit Card", False)

                'if xform is valid or we have a 3d secure passback
                If ccXform.valid Or Not myWeb.moRequest("PaRes") = "" Then
                    Dim oePDQ = New ePDQHelper

                    'Merchant Information
                    oePDQ.Merchant_SiteName = myWeb.moConfig("SiteName")
                    oePDQ.Merchant_SiteURL = moCartConfig("SiteURL")
                    oePDQ.Merchant_ClientId = oePDQCfg.SelectSingleNode("accountId").Attributes("value").Value
                    oePDQ.Merchant_UserName = oePDQCfg.SelectSingleNode("accountUsername").Attributes("value").Value
                    oePDQ.Merchant_Password = oePDQCfg.SelectSingleNode("accountPassword").Attributes("value").Value
                    oePDQ.Merchant_CountryISO3166 = ePDQHelper.CountriesNamesToISO3166.UnitedKingdom
                    If oePDQCfg.SelectSingleNode("secure3d").Attributes("value").Value = "on" Then
                        oePDQ.Merchant_Requires3DSecure = True ' needs to be a config value
                    End If
                    'ePDQ Settings
                    oePDQ.ePDQ_URL = New Uri(oePDQCfg.SelectSingleNode("address").Attributes("value").Value) 'ePDQ URL
                    oePDQ.ePDQ_Pipeline = ePDQHelper.Pipelines.Payment 'Usualluy Payment, could be a config value
                    oePDQ.ePDQ_TransactionType = ePDQHelper.TransactionTypes.Auth 'TransactionType, could be a config value
                    oePDQ.ePDQ_SetTransactionMode(oePDQCfg.SelectSingleNode("opperationMode").Attributes("value").Value) 'mode
                    '3D Secure
                    oePDQ.ThreeD_Return3DSecureURL = moCartConfig("SecureURL") & "?cartCmd=SubmitPaymentDetails" & "&3dsec=return" 'return address for 3dsecure
                    oePDQ.ThreeD_PaResult = myWeb.moRequest("PaRes") 'the passback information
                    'Order Details
                    oePDQ.Trans_Total = oEwProv.mnPaymentAmount 'amount to be paid
                    oePDQ.Trans_OrderReference = mnCartId 'cart reference

                    'Customer Details
                    oePDQ.Cux_HTTP_ACCEPT = myWeb.moRequest.ServerVariables("HTTP_ACCEPT") 'for 3dsecure
                    oePDQ.Cux_HTTP_USER_AGENT = myWeb.moRequest.ServerVariables("HTTP_USER_AGENT") 'for 3dsecure
                    'Billing Address
                    Dim oCartAdd As XmlElement = oOrder.SelectSingleNode("Contact[@type='Billing Address']")
                    If Not oCartAdd Is Nothing Then
                        If Not oCartAdd.SelectSingleNode("GivenName") Is Nothing Then oePDQ.Cux_BillingAddress.FullName = oCartAdd.SelectSingleNode("GivenName").InnerText
                        If Not oCartAdd.SelectSingleNode("Company") Is Nothing Then oePDQ.Cux_BillingAddress.Company = oCartAdd.SelectSingleNode("Company").InnerText
                        If Not oCartAdd.SelectSingleNode("Street") Is Nothing Then oePDQ.Cux_BillingAddress.Street1 = oCartAdd.SelectSingleNode("Street").InnerText
                        If Not oCartAdd.SelectSingleNode("City") Is Nothing Then oePDQ.Cux_BillingAddress.City = Left(oCartAdd.SelectSingleNode("City").InnerText, 25)
                        If Not oCartAdd.SelectSingleNode("State") Is Nothing Then oePDQ.Cux_BillingAddress.StateProv = oCartAdd.SelectSingleNode("State").InnerText
                        If Not oCartAdd.SelectSingleNode("PostalCode") Is Nothing Then oePDQ.Cux_BillingAddress.Postcode = oCartAdd.SelectSingleNode("PostalCode").InnerText
                        If Not oCartAdd.SelectSingleNode("Country") Is Nothing Then oePDQ.Cux_BillingAddress.Country = getCountryISONum(oCartAdd.SelectSingleNode("Country").InnerText)
                        If Not oCartAdd.SelectSingleNode("Email") Is Nothing Then oePDQ.Cux_BillingAddress.Email = oCartAdd.SelectSingleNode("Email").InnerText
                        If Not oCartAdd.SelectSingleNode("Telephone") Is Nothing Then oePDQ.Cux_BillingAddress.TelVoice = oCartAdd.SelectSingleNode("Telephone").InnerText
                        If Not oCartAdd.SelectSingleNode("Fax") Is Nothing Then oePDQ.Cux_BillingAddress.TelFax = oCartAdd.SelectSingleNode("Fax").InnerText
                    End If
                    'Shipping Address
                    oCartAdd = oOrder.SelectSingleNode("Contact[@type='Delivery Address']")
                    If Not oCartAdd Is Nothing Then
                        If Not oCartAdd.SelectSingleNode("GivenName") Is Nothing Then oePDQ.Cux_ShippingAddress.FullName = oCartAdd.SelectSingleNode("GivenName").InnerText
                        If Not oCartAdd.SelectSingleNode("Company") Is Nothing Then oePDQ.Cux_ShippingAddress.Company = oCartAdd.SelectSingleNode("Company").InnerText
                        If Not oCartAdd.SelectSingleNode("Street") Is Nothing Then oePDQ.Cux_ShippingAddress.Street1 = oCartAdd.SelectSingleNode("Street").InnerText
                        If Not oCartAdd.SelectSingleNode("City") Is Nothing Then oePDQ.Cux_ShippingAddress.City = Left(oCartAdd.SelectSingleNode("City").InnerText, 25)
                        If Not oCartAdd.SelectSingleNode("State") Is Nothing Then oePDQ.Cux_ShippingAddress.StateProv = oCartAdd.SelectSingleNode("State").InnerText
                        If Not oCartAdd.SelectSingleNode("PostalCode") Is Nothing Then oePDQ.Cux_ShippingAddress.Postcode = oCartAdd.SelectSingleNode("PostalCode").InnerText
                        If Not oCartAdd.SelectSingleNode("Country") Is Nothing Then oePDQ.Cux_ShippingAddress.Country = getCountryISONum(oCartAdd.SelectSingleNode("Country").InnerText)
                        If Not oCartAdd.SelectSingleNode("Email") Is Nothing Then oePDQ.Cux_ShippingAddress.Email = oCartAdd.SelectSingleNode("Email").InnerText
                        If Not oCartAdd.SelectSingleNode("Telephone") Is Nothing Then oePDQ.Cux_ShippingAddress.TelVoice = oCartAdd.SelectSingleNode("Telephone").InnerText
                        If Not oCartAdd.SelectSingleNode("Fax") Is Nothing Then oePDQ.Cux_ShippingAddress.TelFax = oCartAdd.SelectSingleNode("Fax").InnerText
                    End If
                    'Card Details
                    If Not oePDQ.ThreeD_PaResult = "" Then
                        oePDQ.Card_CV2 = myWeb.moSession("creditCard/CV2")
                        Try
                            oePDQ.Card_SetExpireDate(Left(myWeb.moSession("creditCard/expireDate"), 2), Right(myWeb.moSession("creditCard/expireDate"), 2))
                            oePDQ.Card_SetStartDate(Left(myWeb.moSession("creditCard/issueDate"), 2), Right(myWeb.moSession("creditCard/issueDate"), 2))
                        Catch ex As Exception
                            'Ignore
                        End Try
                        oePDQ.Card_Number = myWeb.moSession("creditCard/number")
                        oePDQ.Card_Type = myWeb.moSession("creditCard/type")
                    Else
                        oePDQ.Card_CV2 = myWeb.moRequest("creditCard/CV2")
                        Try
                            oePDQ.Card_SetExpireDate(Left(myWeb.moRequest("creditCard/expireDate"), 2), Right(myWeb.moRequest("creditCard/expireDate"), 2))
                            oePDQ.Card_SetStartDate(Left(myWeb.moRequest("creditCard/issueDate"), 2), Right(myWeb.moRequest("creditCard/issueDate"), 2))
                        Catch ex As Exception
                            'Ignore
                        End Try
                        oePDQ.Card_Number = myWeb.moRequest("creditCard/number")
                        oePDQ.Card_Type = myWeb.moRequest("creditCard/type")
                    End If
                    'Make The Request
                    Dim oDate As New Date("20" & Right(oePDQ.Card_Expires, 2), CInt(Left(oePDQ.Card_Expires, 2)), 1)
                    Dim cMethodName As String = System.Enum.GetName(GetType(ePDQHelper.CardTypes), oePDQ.Card_Type) & ": " & MaskString(oePDQ.Card_Number, "*", False, 4) & " Expires: " & oDate.ToShortDateString

                    oePDQ.RequestAuthorisation()
                    If oePDQ.ePDQ_Response.Valid Then
                        'We Save the payment method prior to ultimate validation because we only have access to the request object at the point it is submitted
                        'only do this for a valid payment method

                        oDate = oDate.AddMonths(1)
                        oDate = oDate.AddDays(-1)
                        Dim cAuthCode As String = oePDQ.ePDQ_Response.AuthorisationCode
                        Dim oePDQElmt As XmlElement = ccXform.Instance.OwnerDocument.CreateElement("ePDQ")
                        oePDQElmt.SetAttribute("AuthCode", cAuthCode)
                        ccXform.Instance.FirstChild.AppendChild(oePDQElmt)
                        oEwProv.savePayment(myWeb.mnUserId, "ePDQ", mnCartId, cMethodName, ccXform.Instance.FirstChild, Now, False, oEwProv.mnPaymentAmount)
                        If myWeb.moRequest("3dsec") = "return" And Not myWeb.moRequest("PaRes") = "" Then
                            'Create a form to submit back and get fully valid result
                            'redirect to show invoice
                            Dim sRedirectURL As String
                            sRedirectURL = moCartConfig("SecureURL") & "?cartCmd=SubmitPaymentDetails&3dsec=showInvoice"
                            ccXform = Me.xfrmSecure3DReturn(sRedirectURL)
                            myWeb.moSession("PaRes") = myWeb.moRequest("PaRes")
                        Else
                            ccXform.valid = True
                        End If
                        err_msg = cMethodName & ":ePDQ_Response = Valid"

                    ElseIf oePDQ.ThreeD_Requested3DSecure Then
                        Dim sRedirectURL As String
                        sRedirectURL = moCartConfig("SecureURL") & "?cartCmd=SubmitPaymentDetails&3dsec=return"
                        err_msg = cMethodName & vbLf
                        err_msg = err_msg & "This card has subscribed to 3D Secure. You will now be re-directed to your banks website for further verification."
                        ccXform = oEwProv.xfrmSecure3D(oePDQ.ThreeD_Requested3DSecureDetails.URL, oePDQ.ThreeD_Requested3DSecureDetails.MD, oePDQ.ThreeD_Requested3DSecureDetails.PaRes, oePDQ.ThreeD_Requested3DSecureDetails.TermUrl)
                        ccXform.addNote("creditCard", xForm.noteTypes.Alert, err_msg, True)
                        'save the credit card details in the sessions
                        For Each cItem As String In myWeb.moRequest.Form.Keys
                            myWeb.moSession.Add(cItem, myWeb.moRequest.Form.Item(cItem))
                        Next
                        ccXform.valid = False
                    Else
                        err_msg = cMethodName & vbLf & oePDQ.ePDQ_Response.ResponseCode & ": " & oePDQ.ePDQ_Response.Message
                        ccXform.addNote(ccXform.moXformElmt, xForm.noteTypes.Alert, err_msg)
                        ccXform.valid = False

                        'if not valid return from 3DS then popout of window back to standard form.
                        If myWeb.moRequest("3dsec") = "return" And Not myWeb.moRequest("PaRes") = "" Then
                            Dim sRedirectURL As String
                            sRedirectURL = moCartConfig("SecureURL") & "?cartCmd=SubmitPaymentDetails"
                            ccXform = Me.xfrmSecure3DReturn(sRedirectURL)
                        End If
                    End If

                    'Update Seller Notes:
                    Dim sSql As String = "select * from tblCartOrder where nCartOrderKey = " & mnCartId
                    Dim oDs As DataSet
                    Dim oRow As DataRow
                    oDs = myWeb.moDbHelper.getDataSetForUpdate(sSql, "Order", "Cart")
                    For Each oRow In oDs.Tables("Order").Rows
                        If myWeb.moRequest("3dsec") = "return" And Not myWeb.moRequest("PaRes") = "" Then
                            oRow("cPaymentRef") = oePDQ.ePDQ_Response.AuthorisationCode.ToString
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
            Dim oXform As xForm = New Eonic.Web.xForm
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

    End Class

    Public Class ePDQHelper
#Region "               Declarations"
        Private mcModuleName As String = "Eonic.Web.Cart.PaymentProviders.ePDQ"
        'Events
        Public Event OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)
        Private oList_Currencies As DataTable

#Region "                   Merchant"
        Private cMerchant_SiteName As String = ""
        Private cMerchant_SiteURL As String = ""
        Private cMerchant_UserName As String = ""
        Private cMerchant_Password As String = ""
        Private cMerchant_ClientId As String = ""
        Private cMerchant_Country As String = ""
        Private bMerchant_Requires3DSecure As Boolean = False
#End Region
#Region "                   ePDQ"
        Private oePDQ_URL As Uri
        'Pipeline?
        Private cePDQ_Pipeline As Pipelines = Pipelines.PaymentNoFraud
        'Transaction Mode
        Private cePDQ_Mode As TransactionMode = TransactionMode.T
        'Transaction Type 
        Private cePDQ_TransactionType As TransactionTypes = TransactionTypes.Auth
        'Response
        Private oePDQ_Response As ePDQResponse = New ePDQResponse("Authorisation Not Requested", False, Nothing, 0, "", "")
        Private oePDQ_SentDocument As XmlDocument
#End Region
#Region "                   3D Secure"
        Private c3D_ReturnURL As String = ""
        Private c3D_MerchatData As String = ""
        Private b3D_Requested3DSecure As Boolean = False
        Private o3D_Requested3DSecureDefinition As Requested3DSecureDefinition = Nothing

        'Input for resonse from 3dRedirect
        Private c3D_PaResULT As String

        Private c3D_AuthenticationResult As String = ""
        Private c3D_ACSVerificationID As String = ""
        Private c3D_ECI As String = ""
        Private c3D_XID As String = ""

#End Region
#Region "                   Customer Details"
        Private cCux_Email As String = ""
        Private cCux_HTTP_ACCEPT As String = ""
        Private cCux_HTTP_USER_AGENT As String = ""
        'Billing Address
        Dim oCux_BillingAddress As New Address(Address.addressType.Billing)
        'Shippingaddress
        Dim oCux_ShippingAddress As New Address(Address.addressType.Shipping)
#End Region
#Region "                   Transaction"
        'Currency
        Private cTrans_CurrencyCode As String = ""
        Private cTrans_CurrencyShort As String = ""
        Private cTrans_CurrencySymbol As String = ""
        Private cTrans_CurrencyDecimals As Integer = 0
        Private cTrans_Total As String = ""
        Private cTrans_OrderRef As String = ""
#End Region
#Region "                   Card"
        'Card Details
        Private cCard_Number As String = ""
        Private cCard_Expires As String = ""
        Private cCard_Cvv2Val As String = ""
        Private cCard_Cvv2Indicator As String = "1"
        Private cCard_IssueNum As String = ""
        Private cCard_StartDate As String = ""
        Private nCard_Type As CardTypes
#End Region
#End Region
#Region "               Properties"
#Region "                       Merchant"
        Public WriteOnly Property Merchant_SiteName() As String
            Set(ByVal value As String)
                cMerchant_SiteName = value
            End Set
        End Property

        Public WriteOnly Property Merchant_SiteURL() As String
            Set(ByVal value As String)
                cMerchant_SiteURL = value
            End Set
        End Property

        Public WriteOnly Property Merchant_UserName() As String
            Set(ByVal value As String)
                cMerchant_UserName = value
            End Set
        End Property
        Public WriteOnly Property Merchant_Password() As String
            Set(ByVal value As String)
                cMerchant_Password = value
            End Set
        End Property
        Public WriteOnly Property Merchant_ClientId() As String
            Set(ByVal value As String)
                cMerchant_ClientId = value
            End Set
        End Property
        Public Property Merchant_CountryAbbreviation() As CountriesISO3166Abbreviations
            Get
                Return cMerchant_Country
            End Get
            Set(ByVal value As CountriesISO3166Abbreviations)
                cMerchant_Country = value
            End Set
        End Property
        Public Property Merchant_CountryISO3166() As CountriesNamesToISO3166
            Get
                Return cMerchant_Country
            End Get
            Set(ByVal value As CountriesNamesToISO3166)
                cMerchant_Country = value
            End Set
        End Property
        Public Property Merchant_Requires3DSecure() As Boolean
            Get
                Return bMerchant_Requires3DSecure
            End Get
            Set(ByVal value As Boolean)
                bMerchant_Requires3DSecure = value
            End Set
        End Property
#End Region
#Region "                       ePDQ"

        'Pipeline?
        Public Property ePDQ_Pipeline() As Pipelines
            Get
                Return cePDQ_Pipeline
            End Get
            Set(ByVal value As Pipelines)
                cePDQ_Pipeline = value
            End Set
        End Property
        'Transaction Mode
        Public ReadOnly Property ePDQ_Mode() As TransactionMode
            Get
                Return cePDQ_Mode
            End Get
        End Property
        'Transaction Type 
        Public Property ePDQ_TransactionType() As TransactionTypes
            Get
                Return cePDQ_TransactionType
            End Get
            Set(ByVal value As TransactionTypes)
                cePDQ_TransactionType = value
            End Set
        End Property
        Public ReadOnly Property ePDQ_Response() As ePDQResponse
            Get
                Return oePDQ_Response
            End Get
        End Property

        Public Property ePDQ_URL() As Uri
            Get
                Return oePDQ_URL
            End Get
            Set(ByVal value As Uri)
                oePDQ_URL = value
            End Set
        End Property

        Public ReadOnly Property ePDQ_SentXML() As String
            Get
                Return oePDQ_SentDocument.OuterXml
            End Get
        End Property
#End Region
#Region "                       3D Secure"
        Public ReadOnly Property ThreeD_Requested3DSecure() As Boolean
            Get
                Return b3D_Requested3DSecure
            End Get
        End Property
        Public ReadOnly Property ThreeD_Requested3DSecureDetails() As Requested3DSecureDefinition
            Get
                Return o3D_Requested3DSecureDefinition
            End Get
        End Property
        Public Property ThreeD_Return3DSecureURL() As String
            Get
                Return c3D_ReturnURL
            End Get
            Set(ByVal value As String)
                c3D_ReturnURL = value
            End Set
        End Property
        Public Property ThreeD_Return3dMerchatData() As String
            Get
                Return c3D_MerchatData
            End Get
            Set(ByVal value As String)
                c3D_MerchatData = value
            End Set
        End Property
        Public Property ThreeD_PaResult() As String
            Set(ByVal value As String)
                c3D_PaResULT = value
            End Set
            Get
                Return c3D_PaResULT
            End Get
        End Property

        Public ReadOnly Property ThreeD_AuthenticationResult() As String
            Get
                Return c3D_AuthenticationResult
            End Get
        End Property
        Public ReadOnly Property ThreeD_ACSVerificationID() As String
            Get
                Return c3D_ACSVerificationID
            End Get
        End Property
        Public ReadOnly Property ThreeD_ECI() As String
            Get
                Return c3D_ECI
            End Get
        End Property
        Public ReadOnly Property ThreeD_XID() As String
            Get
                Return c3D_XID
            End Get
        End Property

#End Region
#Region "                       Transaction"

        Public Property Trans_CurrencyCode() As String
            Get
                Return cTrans_CurrencyCode
            End Get
            Set(ByVal value As String)
                SelectCurrency(value)
            End Set
        End Property
        Public Property Trans_CurrencyShort() As String
            Get
                Return cTrans_CurrencyShort
            End Get
            Set(ByVal value As String)
                SelectCurrency(, value)
            End Set
        End Property
        Public Property Trans_Currencysymbol() As String
            Get
                Return cTrans_CurrencySymbol
            End Get
            Set(ByVal value As String)
                SelectCurrency(, , value)
            End Set
        End Property
        Public ReadOnly Property Trans_CurrencyDecimals() As Integer
            Get
                Return cTrans_CurrencyDecimals
            End Get
        End Property

        Public Property Trans_Total() As String
            Get
                Return cTrans_Total
            End Get
            Set(ByVal value As String)
                cTrans_Total = Replace(FormatNumber(value, Me.Trans_CurrencyDecimals), ",", "")
            End Set
        End Property
        Public Property Trans_OrderReference() As String
            Get
                Return cTrans_OrderRef
            End Get
            Set(ByVal value As String)
                cTrans_OrderRef = value
            End Set
        End Property
#End Region
#Region "                       Card"

        'Card Details
        Public Property Card_Number() As String
            Get
                Return cCard_Number
            End Get
            Set(ByVal value As String)
                cCard_Number = Replace(value, " ", "")
            End Set
        End Property
        Public Property Card_Expires() As String
            Get
                Return cCard_Expires
            End Get
            Set(ByVal value As String)
                cCard_Expires = value
            End Set
        End Property
        Public Property Card_CV2() As String
            Get
                Return cCard_Cvv2Val
            End Get
            Set(ByVal value As String)
                cCard_Cvv2Val = value
            End Set
        End Property
        Public ReadOnly Property Card_IssueNum() As String
            Get
                Return cCard_IssueNum
            End Get
        End Property
        Public ReadOnly Property Card_StartDate() As String
            Get
                Return cCard_StartDate
            End Get
        End Property
        Public Property Card_Type() As CardTypes
            Get
                Return nCard_Type
            End Get
            Set(ByVal value As CardTypes)
                nCard_Type = value
            End Set
        End Property
#End Region
#Region "                       User"
        Public Property Cux_Email() As String
            Get
                Return cCux_Email
            End Get
            Set(ByVal value As String)
                cCux_Email = value
            End Set
        End Property
        Public Property Cux_HTTP_ACCEPT() As String
            Get
                Return cCux_HTTP_ACCEPT
            End Get
            Set(ByVal value As String)
                cCux_HTTP_ACCEPT = value
            End Set
        End Property
        Public Property Cux_HTTP_USER_AGENT() As String
            Get
                Return cCux_HTTP_USER_AGENT
            End Get
            Set(ByVal value As String)
                cCux_HTTP_USER_AGENT = value
            End Set
        End Property
        'Addresses
        'Billing Address
        Public Property Cux_BillingAddress() As Address
            Get
                Return oCux_BillingAddress
            End Get
            Set(ByVal value As Address)
                oCux_BillingAddress = value
            End Set
        End Property
        'Shippingaddress
        Public Property Cux_ShippingAddress() As Address
            Get
                Return oCux_ShippingAddress
            End Get
            Set(ByVal value As Address)
                oCux_ShippingAddress = value
            End Set
        End Property
#End Region




#End Region
#Region "               Enums"
        '<summary>
        'Transaction Modes
        '</summary>
        Public Enum TransactionMode
            '<summary>
            'Production mode. Indicates the transaction is real.
            '</summary>
            P
            '<summary>
            'Test mode. This value sends information contained in a test
            'OrderFormDoc to the credit card processor. This value
            'should be specified only when you have made
            'arrangements with the processor that you will be testing.
            'Transactions in test mode are processed by a different
            'component than those in production mode. For example, if
            'you use FDMS South to authorize transactions, the
            'CcxFdmsSouthAuth component performs this action in
            'production mode, while the CcxFdmsSouthAuthTest
            'component handles the transaction in test mode.
            '</summary>
            T
            'N Sends the transaction to the internal payment simulator. The
            'response depends on which value was specified:
            '<summary>
            'The simulator rejects the transaction.
            '</summary>
            N
            '<summary>
            'The simulator accepts the transaction.
            '</summary>
            Y
            '<summary>
            'The simulator randomly accepts or rejects the transaction.
            'The payment simulator also returns a random authorization
            'code and other normalized return codes.
            '</summary>
            R
            'FN For FraudShield transactions only.
            'Sends the transaction to the internal payment simulator. The
            'response depends on which value was specified:
            '<summary>
            'The simulator rejects the transaction.
            '</summary>
            FN
            '<summary>
            'The simulator accepts the transaction.
            '</summary>
            FY
        End Enum
        '<summary>
        'Pipelines
        '</summary>
        Public Enum Pipelines
            '<summary>
            ' CcxDataPreProcessor
            ' CcxPeriodicBilling
            ' CcxOcc
            ' CcxFraudShield
            ' CcxPayment
            ' CcxFraudShield
            ' CcxDigitalReceipts
            ' CcxPeriodicBilling
            ' FraudShield performs fraud checks before and after an
            ' order is sent to the CcxPayment component.
            '</summary>
            Payment
            '<summary>
            ' CcxDataPreProcessor
            ' CcxPeriodicBilling
            ' CcxOcc
            ' CcxFraudAnalyzer
            ' CcxFraudShield
            ' CcxPayment
            ' CcxFraudShield
            ' CcxDigitalReceipts
            ' CcxPeriodicBilling
            ' The FraudAnalyzer component must be installed to used
            ' this pipeline.
            '</summary>
            PaymentFA
            '<summary>
            ' CcxDataPreProcessor
            ' CcxPeriodicBilling
            ' CcxOcc
            ' CcxPayment
            ' CcxDigitalReceipts
            ' CcxPeriodicBilling
            '</summary>
            PaymentNoFraud
            '<summary>
            ' CcxDataPreProcessor
            ' CcxPayment
            '</summary>
            Settlement
            '<summary>
            ' CcxDataPreProcessor
            ' CcxInhouseShipping
            '</summary>
            Shipping
            '<summary>
            ' CcxDataPreProcessor
            ' CcxTax
            '</summary>
            Tax

        End Enum
        '<summary>
        'Transaction Types
        '</summary>
        Public Enum TransactionTypes
            '<summary>
            'Default
            '</summary>
            Auth
            PreAuth
        End Enum
        '<summary>
        'Credit/Debit Card Types
        '</summary>
        Public Enum CardTypes
            '<summary>
            'Visa
            '</summary>
            Visa = 1
            '<summary>
            'Mastercard
            '</summary>
            MasterCard = 2
            '<summary>
            'Discover
            '</summary>
            Discover = 3
            '<summary>
            'Diners Club
            '</summary>
            DinersClub = 4
            '<summary>
            'Carte Blanche
            '</summary>
            CarteBlanche = 5
            '<summary>
            'JCB/JCL
            '</summary>
            JCBJCL = 6
            '<summary>
            'enRoute
            '</summary>
            enRoute = 7
            '<summary>
            'American Express
            '</summary>
            AmericanExpress = 8
            '<summary>
            'Solo
            '</summary>
            Solo = 9
            '<summary>
            'UK Maestro
            '</summary>
            UKMaestro = 10
            '<summary>
            'Electron
            '</summary>
            Electron = 11
            '<summary>
            'Maestro
            '</summary>
            Maestro = 14
        End Enum

        Public Enum CountriesNamesToISO3166
            UnitedStates = 840     'United States
            Afghanistan = 4     'Afghanistan
            AlandIslands = 248     'Aland Islands
            Albania = 8     'Albania
            Algeria = 12     'Algeria
            AmericanSamoa = 16     'American Samoa
            Andorra = 20     'Andorra
            Angola = 24     'Angola
            Anguilla = 660     'Anguilla
            Antarctica = 10     'Antarctica
            AntiguaandBarbuda = 28     'Antigua and Barbuda
            Argentina = 32     'Argentina
            Armenia = 51     'Armenia
            Aruba = 533     'Aruba
            Australia = 36     'Australia
            Austria = 40     'Austria
            Azerbaijan = 31     'Azerbaijan
            Bahamas = 44     'Bahamas
            Bahrain = 48     'Bahrain
            Bangladesh = 50     'Bangladesh
            Barbados = 52     'Barbados
            Belarus = 112     'Belarus
            Belgium = 56     'Belgium
            Belize = 84     'Belize
            Benin = 204     'Benin
            Bermuda = 60     'Bermuda
            Bhutan = 64     'Bhutan
            Bolivia = 68     'Bolivia
            BosniaandHerzegovina = 70     'Bosnia and Herzegovina
            Botswana = 72     'Botswana
            BouvetIsland = 74     'Bouvet Island
            Brazil = 76     'Brazil
            BritishIndianOceanTerritory = 86     'British Indian Ocean Territory
            BruneiDarussalam = 96     'Brunei Darussalam
            Bulgaria = 100     'Bulgaria
            BurkinaFaso = 854     'Burkina Faso
            Burundi = 108     'Burundi
            Cambodia = 116     'Cambodia
            Cameroon = 120     'Cameroon
            Canada = 124     'Canada
            CapeVerde = 132     'Cape Verde
            CaymanIslands = 136     'Cayman Islands
            CentralAfricanRepublic = 140     'Central African Republic
            Chad = 148     'Chad
            ChannelIslands = 830     'Channel Islands
            Chile = 152     'Chile
            China = 156     'China
            ChristmasIsland = 162     'Christmas Island
            CocosKeelingIslands = 166     'Cocos (Keeling) Islands
            Colombia = 170     'Colombia
            Comoros = 174     'Comoros
            CongoDemocraticRepublicofthe = 180     'Congo - Democratic Republic of the
            Congo = 178     'Congo
            CookIslands = 184     'Cook Islands
            CostaRica = 188     'Costa Rica
            CotedIvoire = 384     'Cote d'Ivoire
            Croatia = 191     'Croatia
            Cuba = 192     'Cuba
            Cyprus = 196     'Cyprus
            CzechRepublic = 203     'Czech Republic
            Denmark = 208     'Denmark
            Djibouti = 262     'Djibouti
            Dominica = 212     'Dominica
            DominicanRepublic = 214     'Dominican Republic
            Ecuador = 218     'Ecuador
            Egypt = 818     'Egypt
            ElSalvador = 222     'El Salvador
            EquatorialGuinea = 226     'Equatorial Guinea
            Eritrea = 232     'Eritrea
            Estonia = 233     'Estonia
            Ethiopia = 231     'Ethiopia
            FalklandIslandsMalvinas = 238     'Falkland Islands (Malvinas)
            FaroeIslands = 234     'Faroe Islands
            Fiji = 242     'Fiji
            Finland = 246     'Finland
            France = 250     'France
            FranceMetropolitan = 249     'France - Metropolitan
            FrenchGuiana = 254     'French Guiana
            FrenchPolynesia = 258     'French Polynesia
            FrenchSouthernTerritories = 260     'French Southern Territories
            Gabon = 266     'Gabon
            Gambia = 270     'Gambia
            Georgia = 268     'Georgia
            Germany = 276     'Germany
            Ghana = 288     'Ghana
            Gibraltar = 292     'Gibraltar
            Greece = 300     'Greece
            Greenland = 304     'Greenland
            Grenada = 308     'Grenada
            Guadeloupe = 312     'Guadeloupe
            Guam = 316     'Guam
            Guatemala = 320     'Guatemala
            Guinea = 324     'Guinea
            GuineaBissau = 624     'Guinea-Bissau
            Guyana = 328     'Guyana
            Haiti = 332     'Haiti
            HeardIslandandMcDonaldIslands = 334     'Heard Island and McDonald Islands
            HolySeeVaticanCityState = 336     'Holy See (Vatican City State)
            Honduras = 340     'Honduras
            HongKong = 344     'Hong Kong
            Hungary = 348     'Hungary
            Iceland = 352     'Iceland
            India = 356     'India
            Indonesia = 360     'Indonesia
            IranIslamicRepublicof = 364     'Iran - Islamic Republic of
            Iraq = 368     'Iraq
            Ireland = 372     'Ireland
            IsleofMan = 833     'Isle of Man
            Israel = 376     'Israel
            Italy = 380     'Italy
            Jamaica = 388     'Jamaica
            Japan = 392     'Japan
            Jordan = 400     'Jordan
            Kazakhstan = 398     'Kazakhstan
            Kenya = 404     'Kenya
            Kiribati = 296     'Kiribati
            KoreaDemocraticPeoplesRepublicof = 408     'Korea - Democratic People's Republic of
            KoreaRepublicof = 410     'Korea - Republic of
            Kuwait = 414     'Kuwait
            Kyrgyzstan = 417     'Kyrgyzstan
            LaoPeoplesDemocraticRepublic = 418     'Lao People's Democratic Republic
            Latvia = 428     'Latvia
            Lebanon = 422     'Lebanon
            Lesotho = 426     'Lesotho
            Liberia = 430     'Liberia
            LibyanArabJamahiriya = 434     'Libyan Arab Jamahiriya
            Liechtenstein = 438     'Liechtenstein
            Lithuania = 440     'Lithuania
            Luxembourg = 442     'Luxembourg
            Macao = 446     'Macao
            MacedoniaTheFormerYugoslavRepublicof = 807     'Macedonia - The Former Yugoslav Republic of
            Madagascar = 450     'Madagascar
            Malawi = 454     'Malawi
            Malaysia = 458     'Malaysia
            Maldives = 462     'Maldives
            Mali = 466     'Mali
            Malta = 470     'Malta
            MarshallIslands = 584     'Marshall Islands
            Martinique = 474     'Martinique
            Mauritania = 478     'Mauritania
            Mauritius = 480     'Mauritius
            Mayotte = 175     'Mayotte
            Mexico = 484     'Mexico
            MicronesiaFederatedStatesof = 583     'Micronesia - Federated States of
            MoldovaRepublicof = 498     'Moldova - Republic of
            Monaco = 492     'Monaco
            Mongolia = 496     'Mongolia
            Montserrat = 500     'Montserrat
            Morocco = 504     'Morocco
            Mozambique = 508     'Mozambique
            Myanmar = 104     'Myanmar
            Namibia = 516     'Namibia
            Nauru = 520     'Nauru
            Nepal = 524     'Nepal
            Netherlands = 528     'Netherlands
            NetherlandsAntilles = 530     'Netherlands Antilles
            NewCaledonia = 540     'New Caledonia
            NewZealand = 554     'New Zealand
            Nicaragua = 558     'Nicaragua
            Niger = 562     'Niger
            Nigeria = 566     'Nigeria
            Niue = 570     'Niue
            NorfolkIsland = 574     'Norfolk Island
            NorthernMarianaIslands = 580     'Northern Mariana Islands
            Norway = 578     'Norway
            Oman = 512     'Oman
            Pakistan = 586     'Pakistan
            Palau = 585     'Palau
            PalestinianTerritoryOccupied = 275     'Palestinian Territory - Occupied
            Panama = 591     'Panama
            PapuaNewGuinea = 598     'Papua New Guinea
            Paraguay = 600     'Paraguay
            Peru = 604     'Peru
            Philippines = 608     'Philippines
            Pitcairn = 612     'Pitcairn
            Poland = 616     'Poland
            Portugal = 620     'Portugal
            PuertoRico = 630     'Puerto Rico
            Qatar = 634     'Qatar
            Reunion = 638     'Reunion
            Romania = 642     'Romania
            RussianFederation = 643     'Russian Federation
            Rwanda = 646     'Rwanda
            SaintHelena = 654     'Saint Helena
            SaintKittsandNevis = 659     'Saint Kitts and Nevis
            SaintLucia = 662     'Saint Lucia
            SaintPierreandMiquelon = 666     'Saint Pierre and Miquelon
            SaintVincentandtheGrenadines = 670     'Saint Vincent and the Grenadines
            Samoa = 882     'Samoa
            SanMarino = 674     'San Marino
            SaoTomeandPrincipe = 678     'Sao Tome and Principe
            SaudiArabia = 682     'Saudi Arabia
            Senegal = 686     'Senegal
            SerbiaandMontenegro = 891     'Serbia and Montenegro
            Seychelles = 690     'Seychelles
            SierraLeone = 694     'Sierra Leone
            Singapore = 702     'Singapore
            Slovakia = 703     'Slovakia
            Slovenia = 705     'Slovenia
            SolomonIslands = 90     'Solomon Islands
            Somalia = 706     'Somalia
            SouthAfrica = 710     'South Africa
            SouthGeorgiaandtheSouthSandwichIslands = 239     'South Georgia and the South Sandwich Islands
            Spain = 724     'Spain
            SriLanka = 144     'Sri Lanka
            Sudan = 736     'Sudan
            Suriname = 740     'Suriname
            SvalbardandJanMayen = 744     'Svalbard and Jan Mayen
            Swaziland = 748     'Swaziland
            Sweden = 752     'Sweden
            Switzerland = 756     'Switzerland
            SyrianArabRepublic = 760     'Syrian Arab Republic
            TaiwanProvinceofChina = 158     'Taiwan - Province of China
            Tajikistan = 762     'Tajikistan
            TanzaniaUnitedRepublicof = 834     'Tanzania - United Republic of
            Thailand = 764     'Thailand
            TimorLeste = 626     'Timor-Leste
            Togo = 768     'Togo
            Tokelau = 772     'Tokelau
            Tonga = 776     'Tonga
            TrinidadandTobago = 780     'Trinidad and Tobago
            Tunisia = 788     'Tunisia
            Turkey = 792     'Turkey
            Turkmenistan = 795     'Turkmenistan
            TurksandCaicosIslands = 796     'Turks and Caicos Islands
            Tuvalu = 798     'Tuvalu
            Uganda = 800     'Uganda
            Ukraine = 804     'Ukraine
            UnitedArabEmirates = 784     'United Arab Emirates
            UnitedKingdom = 826     'United Kingdom
            UnitedStatesMinorOutlyingIslands = 581     'United States Minor Outlying Islands
            Uruguay = 858     'Uruguay
            Uzbekistan = 860     'Uzbekistan
            Vanuatu = 548     'Vanuatu
            Venezuela = 862     'Venezuela
            VietNam = 704     'Viet Nam
            VirginIslandsBritish = 92     'Virgin Islands - British
            VirginIslandsUS = 850     'Virgin Islands - U.S.
            WallisandFutuna = 876     'Wallis and Futuna
            WesternSahara = 732     'Western Sahara
            Yemen = 887     'Yemen
            Zambia = 894     'Zambia
            Zimbabwe = 716     'Zimbabwe

        End Enum

        Public Enum CountriesISO3166Abbreviations
            US = 840     'United States
            AF = 4     'Afghanistan
            AX = 248     'Aland Islands
            AL = 8     'Albania
            DZ = 12     'Algeria
            [AS] = 16     'American Samoa
            AD = 20     'Andorra
            AO = 24     'Angola
            AI = 660     'Anguilla
            AQ = 10     'Antarctica
            AG = 28     'Antigua and Barbuda
            AR = 32     'Argentina
            AM = 51     'Armenia
            AW = 533     'Aruba
            AU = 36     'Australia
            AT = 40     'Austria
            AZ = 31     'Azerbaijan
            BS = 44     'Bahamas
            BH = 48     'Bahrain
            BD = 50     'Bangladesh
            BB = 52     'Barbados
            BY = 112     'Belarus
            BE = 56     'Belgium
            BZ = 84     'Belize
            BJ = 204     'Benin
            BM = 60     'Bermuda
            BT = 64     'Bhutan
            BO = 68     'Bolivia
            BA = 70     'Bosnia and Herzegovina
            BW = 72     'Botswana
            BV = 74     'Bouvet Island
            BR = 76     'Brazil
            IO = 86     'British Indian Ocean Territory
            BN = 96     'Brunei Darussalam
            BG = 100     'Bulgaria
            BF = 854     'Burkina Faso
            BI = 108     'Burundi
            KH = 116     'Cambodia
            CM = 120     'Cameroon
            CA = 124     'Canada
            CV = 132     'Cape Verde
            KY = 136     'Cayman Islands
            CF = 140     'Central African Republic
            TD = 148     'Chad
            XX = 830     'Channel Islands
            CL = 152     'Chile
            CN = 156     'China
            CX = 162     'Christmas Island
            CC = 166     'Cocos (Keeling) Islands
            CO = 170     'Colombia
            KM = 174     'Comoros
            CD = 180     'Congo - Democratic Republic of the
            CG = 178     'Congo
            CK = 184     'Cook Islands
            CR = 188     'Costa Rica
            CI = 384     'Cote d'Ivoire
            HR = 191     'Croatia
            CU = 192     'Cuba
            CY = 196     'Cyprus
            CZ = 203     'Czech Republic
            DK = 208     'Denmark
            DJ = 262     'Djibouti
            DM = 212     'Dominica
            [DO] = 214     'Dominican Republic
            EC = 218     'Ecuador
            EG = 818     'Egypt
            SV = 222     'El Salvador
            GQ = 226     'Equatorial Guinea
            ER = 232     'Eritrea
            EE = 233     'Estonia
            ET = 231     'Ethiopia
            FK = 238     'Falkland Islands (Malvinas)
            FO = 234     'Faroe Islands
            FJ = 242     'Fiji
            FI = 246     'Finland
            FR = 250     'France
            FX = 249     'France - Metropolitan
            GF = 254     'French Guiana
            PF = 258     'French Polynesia
            TF = 260     'French Southern Territories
            GA = 266     'Gabon
            GM = 270     'Gambia
            GE = 268     'Georgia
            DE = 276     'Germany
            GH = 288     'Ghana
            GI = 292     'Gibraltar
            GR = 300     'Greece
            GL = 304     'Greenland
            GD = 308     'Grenada
            GP = 312     'Guadeloupe
            GU = 316     'Guam
            GT = 320     'Guatemala
            GN = 324     'Guinea
            GW = 624     'Guinea-Bissau
            GY = 328     'Guyana
            HT = 332     'Haiti
            HM = 334     'Heard Island and McDonald Islands
            VA = 336     'Holy See (Vatican City State)
            HN = 340     'Honduras
            HK = 344     'Hong Kong
            HU = 348     'Hungary
            [IS] = 352     'Iceland
            [IN] = 356     'India
            ID = 360     'Indonesia
            IR = 364     'Iran - Islamic Republic of
            IQ = 368     'Iraq
            IE = 372     'Ireland
            IM = 833     'Isle of Man
            IL = 376     'Israel
            IT = 380     'Italy
            JM = 388     'Jamaica
            JP = 392     'Japan
            JO = 400     'Jordan
            KZ = 398     'Kazakhstan
            KE = 404     'Kenya
            KI = 296     'Kiribati
            KP = 408     'Korea - Democratic People's Republic of
            KR = 410     'Korea - Republic of
            KW = 414     'Kuwait
            KG = 417     'Kyrgyzstan
            LA = 418     'Lao People's Democratic Republic
            LV = 428     'Latvia
            LB = 422     'Lebanon
            LS = 426     'Lesotho
            LR = 430     'Liberia
            LY = 434     'Libyan Arab Jamahiriya
            LI = 438     'Liechtenstein
            LT = 440     'Lithuania
            LU = 442     'Luxembourg
            MO = 446     'Macao
            MK = 807     'Macedonia - The Former Yugoslav Republic of
            MG = 450     'Madagascar
            MW = 454     'Malawi
            MY = 458     'Malaysia
            MV = 462     'Maldives
            ML = 466     'Mali
            MT = 470     'Malta
            MH = 584     'Marshall Islands
            MQ = 474     'Martinique
            MR = 478     'Mauritania
            MU = 480     'Mauritius
            YT = 175     'Mayotte
            MX = 484     'Mexico
            FM = 583     'Micronesia - Federated States of
            MD = 498     'Moldova - Republic of
            MC = 492     'Monaco
            MN = 496     'Mongolia
            MS = 500     'Montserrat
            MA = 504     'Morocco
            MZ = 508     'Mozambique
            MM = 104     'Myanmar
            NA = 516     'Namibia
            NR = 520     'Nauru
            NP = 524     'Nepal
            NL = 528     'Netherlands
            AN = 530     'Netherlands Antilles
            NC = 540     'New Caledonia
            NZ = 554     'New Zealand
            NI = 558     'Nicaragua
            NE = 562     'Niger
            NG = 566     'Nigeria
            NU = 570     'Niue
            NF = 574     'Norfolk Island
            MP = 580     'Northern Mariana Islands
            NO = 578     'Norway
            OM = 512     'Oman
            PK = 586     'Pakistan
            PW = 585     'Palau
            PS = 275     'Palestinian Territory - Occupied
            PA = 591     'Panama
            PG = 598     'Papua New Guinea
            PY = 600     'Paraguay
            PE = 604     'Peru
            PH = 608     'Philippines
            PN = 612     'Pitcairn
            PL = 616     'Poland
            PT = 620     'Portugal
            PR = 630     'Puerto Rico
            QA = 634     'Qatar
            RE = 638     'Reunion
            RO = 642     'Romania
            RU = 643     'Russian Federation
            RW = 646     'Rwanda
            SH = 654     'Saint Helena
            KN = 659     'Saint Kitts and Nevis
            LC = 662     'Saint Lucia
            PM = 666     'Saint Pierre and Miquelon
            VC = 670     'Saint Vincent and the Grenadines
            WS = 882     'Samoa
            SM = 674     'San Marino
            ST = 678     'Sao Tome and Principe
            SA = 682     'Saudi Arabia
            SN = 686     'Senegal
            CS = 891     'Serbia and Montenegro
            SC = 690     'Seychelles
            SL = 694     'Sierra Leone
            SG = 702     'Singapore
            SK = 703     'Slovakia
            SI = 705     'Slovenia
            SB = 90     'Solomon Islands
            SO = 706     'Somalia
            ZA = 710     'South Africa
            GS = 239     'South Georgia and the South Sandwich Islands
            ES = 724     'Spain
            LK = 144     'Sri Lanka
            SD = 736     'Sudan
            SR = 740     'Suriname
            SJ = 744     'Svalbard and Jan Mayen
            SZ = 748     'Swaziland
            SE = 752     'Sweden
            CH = 756     'Switzerland
            SY = 760     'Syrian Arab Republic
            TW = 158     'Taiwan - Province of China
            TJ = 762     'Tajikistan
            TZ = 834     'Tanzania - United Republic of
            TH = 764     'Thailand
            TL = 626     'Timor-Leste
            TG = 768     'Togo
            TK = 772     'Tokelau
            [TO] = 776     'Tonga
            TT = 780     'Trinidad and Tobago
            TN = 788     'Tunisia
            TR = 792     'Turkey
            TM = 795     'Turkmenistan
            TC = 796     'Turks and Caicos Islands
            TV = 798     'Tuvalu
            UG = 800     'Uganda
            UA = 804     'Ukraine
            AE = 784     'United Arab Emirates
            GB = 826     'United Kingdom
            UM = 581     'United States Minor Outlying Islands
            UY = 858     'Uruguay
            UZ = 860     'Uzbekistan
            VU = 548     'Vanuatu
            VE = 862     'Venezuela
            VN = 704     'Viet Nam
            VG = 92     'Virgin Islands - British
            VI = 850     'Virgin Islands - U.S.
            WF = 876     'Wallis and Futuna
            EH = 732     'Western Sahara
            YE = 887     'Yemen
            ZM = 894     'Zambia
            ZW = 716     'Zimbabwe
        End Enum

        Public Enum ThreeDPayerSecurityLevel
            NotSupported = 0 'Payer Authentication is not supported by the
            'merchant. No liability shift will take place, and
            'the transaction will be treated as a standard ecommerce
            'transaction.
            'Not applicable
            SupportedNotEnrolled = 1 'Payer authentication supported, but the
            'cardholder is not enrolled. During 3-D Secure
            'authentication, it was determined that the
            'cardholder is not currently enrolled in a 3-D
            'Secure based authentication program for
            'Verified by Visa or SecureCode.
            'VERes.CH.enrolled = N
            SupportedEnrolledAuthenticated = 2 'Payer authentication supported.
            'Authentication succeeded. The transaction is
            'eligible for chargeback protection.
            'VERes.CH.enrolled = Y and
            'PARes.TX.status = Y
            'Implementing OrderFormDocs
            'ClearCommerce Engine API Reference and Guide 41
            SupportedEnrolledNotAuthenticated = 3 'Payer Authentication failed. Payer
            'authentication has failed for reasons such as
            'an incorrect password or an authentication
            'results validation failure.
            'Visa policies dictate that the transaction must
            'not be submitted, and an alternate payment
            'method should be requested. Attempting to
            'send in a transaction with this security level
            'will result in rejection by the ClearCommerce
            'Engine.
            'Failed MasterCard payer authentication
            'transactions can still be submitted. In most
            'cases, do not send any payer authentication
            'data. However, some processors allow payer
            'authentication values to be sent. Refer to the
            'appropriate Payment Reference for more
            'information about MasterCard support.
            'VERes.CH.enrolled = Y and
            'PARes.TX.status = N
            SupportedUnavailable = 4 'Authentication results are unavailable. The
            'merchant is enabled for payer authentication,
            'but no authentication results are available.
            'This may occur in a SecureCode environment
            'if no authentication token is available from the
            'authentication process, or in a 3-D Secure
            'environment if there is a failure in
            'communicating or interpreting results from any
            'part of the infrastructure, such as the Merchant
            'Server Plug-In (MPI), Directory Server (DS),
            'or Access Control Server (ACS).
            'VERes.CH.enrolled = Y and
            'PARes.TX.status = U·
            'VERes.CH.enrolled = U
            'A value of 4 may also occur under
            'any scenario where the merchant is
            'unable to contact the MPI or the MPI
            'is unable to contact a component of
            'the 3-D Secure network.
            SupportedNotParticipating = 5 'Payer authentication supported, but the card
            'is not in a participating BIN range. The
            'cardholder is not enrolled in a payer
            'authentication program.
            'VERes.CH.enrolled = N
            Attempts = 6 'For brands supporting attempts processing
            'using 3-D Secure. The cardholder was
            'authenticated using the 3-D Secure attempts
            'server. For 3-D Secure, the transaction is
            'eligible for chargeback protection.
        End Enum
#End Region
#Region "               Sub Classes/Structures"

        Public Structure ePDQResponse
            Private cMessage As String
            Private bValid As Boolean
            Private oResponseXML As XmlDocument
            Private nResponseCode As Integer
            Private cTransactionID As String
            Private cAuthorisationCode As String

            Public ReadOnly Property Message() As String
                Get
                    Return cMessage
                End Get
            End Property
            Public ReadOnly Property Valid() As Boolean
                Get
                    Return bValid
                End Get
            End Property
            Public ReadOnly Property ResponseXML() As XmlDocument
                Get
                    Return oResponseXML
                End Get
            End Property
            Public ReadOnly Property ResponseCode() As Integer
                Get
                    Return nResponseCode
                End Get
            End Property
            Public ReadOnly Property TransactionId() As String
                Get
                    Return cTransactionID
                End Get
            End Property
            Public ReadOnly Property AuthorisationCode() As String
                Get
                    Return cAuthorisationCode
                End Get
            End Property

            Public Sub New(ByVal Msg As String, ByVal Accepted As Boolean, ByVal Xml As XmlDocument, ByVal Code As String, ByVal Id As String, ByVal AuthCode As String)
                cMessage = Msg
                bValid = Accepted
                oResponseXML = Xml
                nResponseCode = Code
                cTransactionID = Id
                cAuthorisationCode = AuthCode
            End Sub

        End Structure

        'Addresses
        Public Class Address

            Private mcModuleName As String = "Eonic.Providers.Payment.ePDQ"
            'Events
            Public Event OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)


            'Minimum fields Required
            Private cFullName As String = ""
            Private cFirstName As String = ""
            Private cLastName As String = ""
            Private cTitle As String = ""
            Private cCompany As String = ""
            Private cStreet1 As String = ""
            Private cStreet2 As String = ""
            Private cStreet3 As String = ""
            Private cCity As String = ""
            Private cStateProv As String = ""
            Private cPostalCode As String = ""
            Private nCountry As Long = Nothing
            Private cEmail As String = ""
            Private cTelFax As String = ""
            Private cTelVoice As String = ""
            Private cTelVoice2 As String = ""
            Private cUrl As String = ""

            Private bAddressType As addressType

            Public Enum addressType

                Billing = 1
                Shipping = 2

            End Enum

            Public Property Email() As String
                Get
                    Return cEmail
                End Get
                Set(ByVal value As String)
                    cEmail = value
                End Set
            End Property

            Public Property TelFax() As String
                Get
                    Return cTelFax
                End Get
                Set(ByVal value As String)
                    cTelFax = value
                End Set
            End Property


            Public Property TelVoice() As String
                Get
                    Return cTelVoice
                End Get
                Set(ByVal value As String)
                    cTelVoice = value
                End Set
            End Property

            Public Property TelVoice2() As String
                Get
                    Return cTelVoice2
                End Get
                Set(ByVal value As String)
                    cTelVoice2 = value
                End Set
            End Property

            Public Property Url() As String
                Get
                    Return cUrl
                End Get
                Set(ByVal value As String)
                    cUrl = value
                End Set
            End Property

            Public Property FullName() As String
                Get
                    Return cFullName
                End Get
                Set(ByVal value As String)

                    'Dim aGivenName() As String = Split(value, " ")
                    'Select Case UBound(aGivenName)
                    '    Case 0
                    '        cLastName = aGivenName(0)
                    '        cFirstName = aGivenName(0)
                    '    Case 1
                    '        cFirstName = aGivenName(0)
                    '        cLastName = aGivenName(1)
                    '    Case 2
                    '        cFirstName = aGivenName(0) & aGivenName(1)
                    '        cLastName = aGivenName(2)
                    '    Case Else
                    '        cFirstName = value.Substring(0, value.IndexOf(aGivenName(UBound(aGivenName)) - 1))
                    '        cLastName = aGivenName(UBound(aGivenName))
                    'End Select

                    cFullName = value
                End Set
            End Property

            Public Property FirstName() As String
                Get
                    Return cFirstName
                End Get
                Set(ByVal value As String)
                    cFirstName = value
                End Set
            End Property

            Public Property LastName() As String
                Get
                    Return cLastName
                End Get
                Set(ByVal value As String)
                    cLastName = value
                End Set
            End Property

            Public Property Title() As String
                Get
                    Return cTitle
                End Get
                Set(ByVal value As String)
                    cTitle = value
                End Set
            End Property

            Public Property Company() As String
                Get
                    Return cCompany
                End Get
                Set(ByVal value As String)
                    cCompany = value
                End Set
            End Property

            Public Property Street1() As String
                Get
                    Return cStreet1
                End Get
                Set(ByVal value As String)
                    cStreet1 = value
                End Set
            End Property

            Public Property Street2() As String
                Get
                    Return cStreet2
                End Get
                Set(ByVal value As String)
                    cStreet2 = value
                End Set
            End Property

            Public Property Street3() As String
                Get
                    Return cStreet3
                End Get
                Set(ByVal value As String)
                    cStreet3 = value
                End Set
            End Property

            Public Property City() As String
                Get
                    Return cCity
                End Get
                Set(ByVal value As String)
                    cCity = value
                End Set
            End Property

            Public Property StateProv() As String
                Get
                    Return cStateProv
                End Get
                Set(ByVal value As String)
                    cStateProv = value
                End Set
            End Property

            Public Property Postcode() As String
                Get
                    Return cPostalCode
                End Get
                Set(ByVal value As String)
                    cPostalCode = value
                End Set
            End Property

            Public Property Country() As Integer
                Get
                    Return nCountry
                End Get
                Set(ByVal value As Integer)
                    nCountry = value
                End Set
            End Property



            Public Sub AppendAddress(ByRef Consumer As XmlElement)
                Try
                    Dim rootNodeName As String = "BillTo"
                    Select Case bAddressType
                        Case addressType.Billing
                            rootNodeName = "BillTo"
                        Case addressType.Shipping
                            rootNodeName = "ShipTo"
                    End Select

                    Dim Root As XmlElement = Eonic.Tools.Xml.addElement(Consumer, rootNodeName)
                    Dim Location As XmlElement = Eonic.Tools.Xml.addElement(Root, "Location")
                    If cEmail <> "" Then Eonic.Tools.Xml.addElement(Location, "Email", cEmail)
                    If cTelFax <> "" Then Eonic.Tools.Xml.addElement(Location, "TelFax", cTelFax)
                    If cTelVoice <> "" Then Eonic.Tools.Xml.addElement(Location, "TelVoice", cTelVoice)
                    If cTelVoice2 <> "" Then Eonic.Tools.Xml.addElement(Location, "TelVoice2", cTelVoice2)

                    Dim Address As XmlElement = Eonic.Tools.Xml.addElement(Location, "Address")
                    If cFirstName <> "" Then Eonic.Tools.Xml.addElement(Address, "FirstName", cFirstName)
                    If cLastName <> "" Then Eonic.Tools.Xml.addElement(Address, "LastName", cLastName)
                    If cFullName <> "" Then Eonic.Tools.Xml.addElement(Address, "Name", cFullName)
                    If cCompany <> "" Then Eonic.Tools.Xml.addElement(Address, "Company", cCompany)
                    If cStreet1 <> "" Then Eonic.Tools.Xml.addElement(Address, "Street1", cStreet1)
                    If cStreet2 <> "" Then Eonic.Tools.Xml.addElement(Address, "Street2", cStreet2)
                    If cStreet3 <> "" Then Eonic.Tools.Xml.addElement(Address, "Street3", cStreet3)
                    If cCity <> "" Then Eonic.Tools.Xml.addElement(Address, "City", cCity)
                    If cStateProv <> "" Then Eonic.Tools.Xml.addElement(Address, "StateProv", cStateProv)
                    If cPostalCode <> "" Then Eonic.Tools.Xml.addElement(Address, "PostalCode", cPostalCode)
                    If Not nCountry = Nothing Then Eonic.Tools.Xml.addElement(Address, "Country", nCountry)
                    If cTitle <> "" Then Eonic.Tools.Xml.addElement(Address, "Title", cTitle)

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "AppendAddress", ex, ""))
                End Try
            End Sub

            Public Sub New(ByVal oAddressType As addressType)
                bAddressType = oAddressType
            End Sub

        End Class

        Public Class Requested3DSecureDefinition

            Private cURL As String
            Private cPaRes As String
            Private cMD As String
            Private cTermUrl As String

            Public ReadOnly Property URL() As String
                Get
                    Return cURL
                End Get
            End Property
            Public ReadOnly Property PaRes() As String
                Get
                    Return cPaRes
                End Get
            End Property
            Public ReadOnly Property MD() As String
                Get
                    Return cMD
                End Get
            End Property
            Public ReadOnly Property TermUrl() As String
                Get
                    Return cTermUrl
                End Get
            End Property

            Public Sub New(ByVal RequestedURL As String, ByVal RequestedPaRes As String, ByVal RequestedMD As String, ByVal RequestedTermUrl As String)
                cURL = RequestedURL
                cPaRes = RequestedPaRes
                cMD = RequestedMD
                cTermUrl = RequestedTermUrl
            End Sub
        End Class
        Private Class AcceptAllCertificatePolicy
            Implements ICertificatePolicy

            Public Function CheckValidationResult(ByVal srvPoint As System.Net.ServicePoint, ByVal certificate As System.Security.Cryptography.X509Certificates.X509Certificate, ByVal request As System.Net.WebRequest, ByVal certificateProblem As Integer) As Boolean Implements System.Net.ICertificatePolicy.CheckValidationResult
                Return True
            End Function
        End Class
#End Region
#Region "               Private Procedures"

        Private Sub BuildCurrencyTable()
            Try
                Dim oDT As New DataTable
                oDT.Columns.Add(New DataColumn("Code", GetType(String)))
                oDT.Columns.Add(New DataColumn("Symbol", GetType(String)))
                oDT.Columns.Add(New DataColumn("Short", GetType(String)))
                oDT.Columns.Add(New DataColumn("Decimals", GetType(String)))
                oDT.Constraints.Add("PK", oDT.Columns("Code"), True)
                oList_Currencies = oDT
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "BuildCurrencyTable", ex, ""))
            End Try
        End Sub

        Private Sub DefaultCurrencies()
            Try
                AddToCurrencyCollection("826", "GBP", "£", 2)
                AddToCurrencyCollection("840", "USD", "$", 2)
                AddToCurrencyCollection("978", "EUR", "€", 2)
                AddToCurrencyCollection("392", "YEN", "¥", 0)
                SelectCurrency("826")
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "DefaultCurrencies", ex, ""))
            End Try
        End Sub

        Private Sub SelectCurrency(Optional ByVal Code As String = "", Optional ByVal [Short] As String = "", Optional ByVal Symbol As String = "")
            Try
                If Code = "" And [Short] = "" And Symbol = "" Then Throw New Exception("Invalid currency selected")
                Dim cWhere As String = ""
                If Not Code = "" Then cWhere = " Code = '" & Code & "'"
                If Not [Short] = "" Then cWhere = " Short = '" & [Short] & "'"
                If Not Symbol = "" Then cWhere = " Symbol = '" & Symbol & "'"

                Dim oDV As New DataView(oList_Currencies)
                oDV.RowFilter = cWhere
                Dim oRow As DataRow = oDV.ToTable.Rows(0)
                If oRow Is Nothing Then Throw New Exception("Invalid currency selected")
                cTrans_CurrencyCode = oRow(0)
                cTrans_CurrencyShort = oRow(1)
                cTrans_CurrencySymbol = oRow(2)
                cTrans_CurrencyDecimals = oRow(3)
            Catch IOOR As IndexOutOfRangeException
                Throw New Exception("Invalid currency selected")
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "SelectCurrency", ex, ""))
            End Try
        End Sub

        Private Function BuildGeneralXML() As XmlDocument
            Dim oSubXML As New XmlDocument
            Try
                Dim OrderFormDoc As XmlElement = EnginDoc(oSubXML)
                Dim Consumer As XmlElement = OrderFormDoc_Consumer(OrderFormDoc)
                oCux_BillingAddress.AppendAddress(Consumer)
                oCux_ShippingAddress.AppendAddress(Consumer)
                Consumer_PaymentMech(Consumer)
                OrderFormDoc_Transaction(OrderFormDoc)
                Return oSubXML
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "BuildGeneralXml", ex, ""))
                Return oSubXML
            End Try
        End Function

        Private Function EnginDoc(ByRef OwnerDoc As XmlDocument) As XmlElement
            Try
                OwnerDoc.AppendChild(OwnerDoc.CreateElement("EngineDocList"))

                Eonic.Tools.Xml.addElement(OwnerDoc.DocumentElement, "DocVersion", "1.0")

                Dim EngineDoc As XmlElement = Eonic.Tools.Xml.addElement(OwnerDoc.DocumentElement, "EngineDoc")

                Eonic.Tools.Xml.addElement(EngineDoc, "ContentType", "OrderFormDoc")

                Dim User As XmlElement = Eonic.Tools.Xml.addElement(EngineDoc, "User")
                Eonic.Tools.Xml.addElement(User, "Name", cMerchant_UserName)
                Eonic.Tools.Xml.addElement(User, "Password", cMerchant_Password)
                Eonic.Tools.Xml.addElement(User, "ClientId", cMerchant_ClientId).SetAttribute("DataType", "S32")

                Dim Instructions As XmlElement = Eonic.Tools.Xml.addElement(EngineDoc, "Instructions")
                Eonic.Tools.Xml.addElement(Instructions, "Pipeline", System.Enum.GetName(GetType(Pipelines), cePDQ_Pipeline))

                Dim OrderFormDoc As XmlElement = Eonic.Tools.Xml.addElement(EngineDoc, "OrderFormDoc")
                Return OrderFormDoc
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "EngineDoc", ex, ""))
                Return Nothing
            End Try
        End Function

        Private Function OrderFormDoc_Consumer(ByRef OrderFormDoc As XmlElement) As XmlElement
            Dim Consumer As XmlElement = Nothing
            Try

                Eonic.Tools.Xml.addElement(OrderFormDoc, "Mode", System.Enum.GetName(GetType(TransactionMode), cePDQ_Mode))
                Eonic.Tools.Xml.addElement(OrderFormDoc, "Id", cTrans_OrderRef)
                Eonic.Tools.Xml.addElement(OrderFormDoc, "Comments")

                Consumer = Eonic.Tools.Xml.addElement(OrderFormDoc, "Consumer")
                Eonic.Tools.Xml.addElement(Consumer, "Email", cCux_Email)

                Return Consumer
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "OrderFormDoc_Consumer", ex, ""))
                Return Consumer
            End Try
        End Function

        Private Sub OrderFormDoc_Transaction(ByVal OrderFormDoc As XmlElement)
            Try
                Dim Transaction As XmlElement = Eonic.Tools.Xml.addElement(OrderFormDoc, "Transaction")
                Eonic.Tools.Xml.addElement(Transaction, "Type", System.Enum.GetName(GetType(TransactionTypes), cePDQ_TransactionType))

                Dim CurrentTotals As XmlElement = Eonic.Tools.Xml.addElement(Transaction, "CurrentTotals")

                Dim Totals As XmlElement = Eonic.Tools.Xml.addElement(CurrentTotals, "Totals")
                Dim oAttElmt As XmlElement = Eonic.Tools.Xml.addElement(Totals, "Total", TotalPennies(cTrans_Total, Me.Trans_CurrencyDecimals))
                oAttElmt.SetAttribute("DataType", "Money")
                oAttElmt.SetAttribute("Currency", Trans_CurrencyCode)

                If Not Me.ThreeD_AuthenticationResult = "" Then
                    Dim cCardholderPresentCode As String = 7 'Quote: "This field indicates the type of transaction being processed. The default value is 7, other values are 8 (for recurring billing transactions), and 13 (for orders where Internet Authentication was used)"
                    If Not Me.ThreeD_ACSVerificationID = "" Then cCardholderPresentCode = 13
                    Eonic.Tools.Xml.addElement(Transaction, "CardholderPresentCode", cCardholderPresentCode)
                    Eonic.Tools.Xml.addElement(Transaction, "PayerSecurityLevel", Me.ThreeD_ECI)
                    Eonic.Tools.Xml.addElement(Transaction, "PayerAuthenticationCode", UrlEncode(Me.ThreeD_ACSVerificationID))
                    Eonic.Tools.Xml.addElement(Transaction, "PayerTxnId", UrlEncode(Me.ThreeD_XID))
                End If


            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "OrderFormDoc_Transaction", ex, ""))
            End Try
        End Sub

        Private Sub Consumer_PaymentMech(ByVal Consumer As XmlElement)
            Try
                Dim PaymentMech As XmlElement = Eonic.Tools.Xml.addElement(Consumer, "PaymentMech")

                Dim CreditCard As XmlElement = Eonic.Tools.Xml.addElement(PaymentMech, "CreditCard")
                '<Type DataType="S32">1</Type>
                Eonic.Tools.Xml.addElement(CreditCard, "Type", nCard_Type).SetAttribute("DataType", "S32")
                Eonic.Tools.Xml.addElement(CreditCard, "Number", cCard_Number)
                Dim oAttElmt As XmlElement
                If Not cCard_Expires Is "" Then
                    oAttElmt = Eonic.Tools.Xml.addElement(CreditCard, "Expires", cCard_Expires)
                    oAttElmt.SetAttribute("DataType", "ExpirationDate")
                    oAttElmt.SetAttribute("Locale", "840")
                End If
                If Not cCard_StartDate Is "" Then
                    oAttElmt = Eonic.Tools.Xml.addElement(CreditCard, "StartDate", cCard_StartDate)
                    oAttElmt.SetAttribute("DataType", "StartDate")
                    oAttElmt.SetAttribute("Locale", "840")
                End If
                If IsNumeric(cCard_IssueNum) Then
                    Eonic.Tools.Xml.addElement(CreditCard, "IssueNum", cCard_IssueNum)
                End If
                Eonic.Tools.Xml.addElement(CreditCard, "Cvv2Val", cCard_Cvv2Val)
                Eonic.Tools.Xml.addElement(CreditCard, "Cvv2Indicator", cCard_Cvv2Indicator)
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "OrderFormDoc_Consumer", ex, ""))
            End Try
        End Sub


        Private Function CreateResponse(ByVal cResponseXML As String) As Boolean
            Dim bAuth As Boolean = False
            Try
                Dim oResponseXML As New XmlDocument
                oResponseXML.LoadXml(cResponseXML)

                Dim ResourceId As Integer = oResponseXML.SelectSingleNode("/EngineDocList/EngineDoc/MessageList/Message/ResourceId").InnerText
                Dim DataState As Integer = oResponseXML.SelectSingleNode("/EngineDocList/EngineDoc/MessageList/Message/DataState").InnerText
                Dim Sev As Integer = oResponseXML.SelectSingleNode("/EngineDocList/EngineDoc/MessageList/Message/Sev").InnerText
                Dim ResponseText As String = oResponseXML.SelectSingleNode("/EngineDocList/EngineDoc/MessageList/Message/Text").InnerText
                Dim cTransId As String = ""
                Dim cAuthCode As String = ""

                If DataState < 6 And Sev < 6 And ResourceId = 1 Then
                    bAuth = True
                    cTransId = oResponseXML.SelectSingleNode("/EngineDocList/EngineDoc/OrderFormDoc/Id").InnerText
                    cAuthCode = oResponseXML.SelectSingleNode("/EngineDocList/EngineDoc/OrderFormDoc/Transaction/AuthCode").InnerText
                End If
                Dim oResult As New ePDQResponse(ResponseText, bAuth, oResponseXML, ResourceId, cTransId, cAuthCode)
                Me.oePDQ_Response = oResult
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "CreateResponse", ex, ""))
            End Try
            Return bAuth
        End Function

        Private Function CreateResponse(ByVal bAuth As Boolean, ByVal ResourceId As Integer, ByVal DataState As Integer, ByVal Sev As Integer, ByVal ResponseText As String, ByVal cTransId As String, ByVal cAuthCode As String) As Boolean
            Try
                Dim oResult As New ePDQResponse(ResponseText, bAuth, Nothing, ResourceId, cTransId, cAuthCode)
                Me.oePDQ_Response = oResult
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "CreateResponse", ex, ""))
            End Try
            Return bAuth
        End Function

        Private Function RequestCCAuthorisation() As Boolean
            Try
                Me.oePDQ_Response = Nothing
                Me.oePDQ_SentDocument = Nothing
                Dim cResponse As String 'Response From Server
                Dim oRequest As HttpWebRequest 'Request Object
                Dim oResponse As HttpWebResponse = Nothing 'Response Object
                Dim oResponseReader As IO.StreamReader 'Response Reader

                'Add The Document
                oRequest = DirectCast(WebRequest.Create(oePDQ_URL), HttpWebRequest)
                Dim oEnc As Text.UTF8Encoding = New Text.UTF8Encoding

                Dim oSentXML As New XmlDocument
                oSentXML.LoadXml(BuildGeneralXML.OuterXml)
                Me.oePDQ_SentDocument = oSentXML

                Dim oBody As Byte() = oEnc.GetBytes(oSentXML.OuterXml)
                oRequest.ContentLength = oBody.Length
                'Ease off the security policy
                'replace with new
                Dim curPolicy As System.Net.ICertificatePolicy = ServicePointManager.CertificatePolicy
                ServicePointManager.CertificatePolicy = New AcceptAllCertificatePolicy()

                oRequest.Credentials = New NetworkCredential(cMerchant_UserName, cMerchant_Password)
                oRequest.KeepAlive = False
                oRequest.Method = "POST"
                oRequest.ContentType = "application/x-www-form-urlencoded"

                Dim bodyStream As IO.Stream = oRequest.GetRequestStream
                bodyStream.Write(oBody, 0, oBody.Length)
                bodyStream.Close()

                'Request
                oResponse = DirectCast(oRequest.GetResponse(), HttpWebResponse)
                oResponseReader = New IO.StreamReader(oResponse.GetResponseStream())
                cResponse = oResponseReader.ReadToEnd
                oRequest.Abort()
                oResponse.Close()
                oRequest = Nothing
                oResponse = Nothing

                'set the security policy back
                ServicePointManager.CertificatePolicy = curPolicy

                Return CreateResponse(cResponse)
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "RequestAuthorisation", ex, ""))
                Return CreateResponse(False, -1, 9, 9, ex.ToString, "", "")
            End Try
        End Function

        Private Function Request3DAuthorisation() As Boolean
            Dim cFullError As String = ""
            Try
                Dim o3D As New XFMS_NET.XFMSClient
                Dim cErr As String = ""
                Dim cMsg As String = ""

                Dim moPaymentCfg = WebConfigurationManager.GetWebApplicationSection("eonic/payment")
                Dim oePDQCfg As XmlElement = moPaymentCfg.SelectSingleNode("provider[@name='ePDQ']")

                If Me.ThreeD_PaResult = "" Then

                    Dim QI As New XFMS_NET.QualifyingInfo
                    QI.deviceCategory = 0
                    QI.httpAccept = Me.Cux_HTTP_ACCEPT
                    QI.httpUserAgent = Me.Cux_HTTP_USER_AGENT

                    If cCard_Number.StartsWith("4") Then
                        'Card_No begins with 4 use visa
                        QI.acquirerBIN = oePDQCfg.SelectSingleNode("verifiedByVisaAquirerBin").Attributes("value").Value
                        QI.DSLoginID = oePDQCfg.SelectSingleNode("verifiedByVisaDsLoginId").Attributes("value").Value
                        QI.DSPassword = oePDQCfg.SelectSingleNode("verifiedByVisaDsPassword").Attributes("value").Value
                    ElseIf cCard_Number.StartsWith("5") Or cCard_Number.StartsWith("6") Then
                        'Card_No begins with 5 and 6 mastercard but not solo.
                        'QI.acquirerBIN = DBNull
                        QI.DSLoginID = oePDQCfg.SelectSingleNode("secureCodeDsLoginId").Attributes("value").Value
                        QI.DSPassword = ""
                    End If

                    Dim PI As New XFMS_NET.PurchaseInfo
                    Dim aExpires() As String = Split(Me.Card_Expires, "/")
                    PI.cardExpiryDate = aExpires(1) & aExpires(0)
                    PI.purchaseAmount = Me.Trans_Total
                    PI.purchaseCurrency = Me.Trans_CurrencyCode
                    PI.purchaseCurrencyExponent = Me.Trans_CurrencyDecimals
                    PI.purchaseDate = Now()
                    PI.merchantID = cMerchant_ClientId
                    PI.merchantName = cMerchant_SiteName
                    PI.merchantCountryCode = Me.Merchant_CountryISO3166
                    PI.merchantUrl = cMerchant_SiteURL

                    'Dim vXID, vPAReqMsg, vErrMsg, oResult As New Object
                    Dim vAuthRequiredResult As XFMS_NET.AuthRequiredCreatePAReqResult

                    Dim SI As New XFMS_NET.ServerInfo
                    SI.clientCertFile = "C:\Program Files\Arcot Systems\ssl\ClientCert.pem"
                    SI.clientKeyFile = "C:\Program Files\Arcot Systems\ssl\ClientKey.pem"
                    SI.connTimeout = "5"
                    Select Case oePDQCfg.SelectSingleNode("opperationMode").Attributes("value").Value
                        Case "P"
                            SI.host = "www.vbv2bmshost.co.uk"
                        Case Else
                            SI.host = "secure2.mde.epdq.co.uk"
                    End Select
                    SI.maxConns = "8"
                    SI.minConns = "4"
                    SI.port = "9707"
                    SI.sockTimeout = "30"
                    SI.transport = "ssl"
                    SI.trustedCACertFile = "C:\Program Files\Arcot Systems\ssl\ServerRootIntermediate1.pem"
                    Try
                        o3D.setSDKEnv(SI)
                    Catch ex As XFMS_NET.ErrorDetail
                        cFullError = ex.errMsg
                    End Try
                    Try
                        vAuthRequiredResult = o3D.getPAReqIfAuthReqEx(cCard_Number, QI, PI)
                    Catch ex As XFMS_NET.ErrorDetail
                        cFullError = ex.errMsg
                        CreateResponse(False, -1, 9, 9, cFullError, "", "")
                        Return False
                    End Try


                    'Dim cMSG As String = ""

                    If vAuthRequiredResult.authRequiredResult.authRequired = -1 Then
                        cMsg = "Card is inelligble for 3DSec program."
                        ' ECI is set to card-not-present value. Check if the card is eligible but not enrolled
                        Return True
                    ElseIf vAuthRequiredResult.authRequiredResult.authRequired = 0 Then
                        cMsg = "Card does NOT require authentication."
                        Return True
                        ' ECI is set to attempted authentication value indicated by card association
                    Else
                        If vAuthRequiredResult.authRequiredResult.authRequired = 1 Then
                            cMsg = "Card requires authentication. Customer is using a PC."
                            ' See xfortCreatePAReq() example code to proceed

                            b3D_Requested3DSecure = True
                            o3D_Requested3DSecureDefinition = New Requested3DSecureDefinition(vAuthRequiredResult.authRequiredResult.ACSUrl, vAuthRequiredResult.createPAReqResult.PAReqMsg, c3D_MerchatData, Me.ThreeD_Return3DSecureURL)
                            CreateResponse(False, -1, 9, 9, "Requires Authentication", "", "")

                            Return False
                        Else
                            If vAuthRequiredResult.authRequiredResult.authRequired = 2 Then
                                cMsg = "Card requires authentication. Customer is using a mobile device."
                                ' Save the strIssuerCert (issuer certificate) then use xfortCreatePAReq() to proceed
                            Else
                                ' correct behavior is to log an error
                                cMsg = "Unknown authRequired value: " & vAuthRequiredResult.authRequiredResult.authRequired
                            End If
                        End If
                    End If

                Else
                    'set the 3dsecure fields
                    Dim vAuthValidationResultEx As XFMS_NET.AuthValidationResultEx

                    Try
                        vAuthValidationResultEx = o3D.verifyAndUnpackPAResMsgEx(Me.ThreeD_PaResult, Me.Card_Number, "")
                    Catch ex As XFMS_NET.ErrorDetail
                        cFullError = ex.errMsg
                        CreateResponse(False, -1, 9, 9, cFullError, "", "")
                        Return False
                    End Try

                    'Check the result
                    If vAuthValidationResultEx.authenticationResult = 0 Or vAuthValidationResultEx.authenticationResult = 1 Then
                        'Wont check the XID vAuthValidationResultEx.XID
                        If vAuthValidationResultEx.purchaseAmount = Me.Trans_Total Then
                            'valid
                            c3D_AuthenticationResult = vAuthValidationResultEx.authenticationResult
                            c3D_ACSVerificationID = vAuthValidationResultEx.ACSVerificationID
                            c3D_ECI = vAuthValidationResultEx.ECI
                            c3D_XID = vAuthValidationResultEx.XID
                            Return True
                        Else
                            CreateResponse(False, -1, 9, 9, "The payment amounts do not match", "", "")
                            Return False
                        End If

                    Else
                        CreateResponse(False, -1, 9, 9, vAuthValidationResultEx.authenticationStatusMsg, "", "")
                        Return False
                    End If
                End If
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "Request3DAuthorisation", ex, cFullError))
                Return False
            End Try
        End Function

        Private Function TotalPennies(ByVal cAmount As String, ByVal cDecimalPlaces As Integer) As String
            Try
                Return Replace(Replace(FormatNumber(cAmount, cDecimalPlaces), ".", ""), ",", "")
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "TotalPennies", ex, ""))
                Return cAmount
            End Try
        End Function


#End Region
#Region "               Public Procedures"

        Public Sub New()
            Try
                BuildCurrencyTable()
                DefaultCurrencies()
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""))
            End Try

        End Sub

        Public Sub AddToCurrencyCollection(ByVal Code As String, ByVal [Short] As String, ByVal Symbol As String, ByVal Decimals As Integer)
            Try
                If Not Code.Length = 3 Or Not IsNumeric(Code) Then Throw New Exception("Currency Code is not in correct format. " & Code & " is Invalid")
                If Not [Short].Length = 3 Then Throw New Exception("Currency Short is not in the correct format. " & [Short] & " is Invalid")
                If Not Symbol.Length = 1 Then Throw New Exception("Currency Symbol is not in the correct format. " & Symbol & " is Invalid")
                If Not oList_Currencies.Rows.Find(Code) Is Nothing Then Throw New Exception("Currency Code " & Code & " already exists in Collection")
                Dim obj() As String = {Code, Symbol, [Short], Decimals}
                oList_Currencies.Rows.Add(obj)
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "AddToCurrencyCollection", ex, ""))
            End Try
        End Sub

        Public Sub Card_SetStartDate(ByVal Month As Integer, ByVal Year As Integer)
            Try
                Dim cMonth As String = ""
                Dim cYear As String = ""
                If Month < 10 Then cMonth = "0" & Month Else cMonth = Month
                If Year < 10 Then cYear = "0" & Year Else cYear = Right(Year, 2)
                cCard_StartDate = cMonth & "/" & cYear
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "SetStartDate", ex, ""))
            End Try
        End Sub

        Public Sub Card_SetExpireDate(ByVal Month As Integer, ByVal Year As Integer)
            Try
                Dim cMonth As String = ""
                Dim cYear As String = ""
                If Month < 10 Then cMonth = "0" & Month Else cMonth = Month
                If Year < 10 Then cYear = "0" & Year Else cYear = Right(Year, 2)
                cCard_Expires = cMonth & "/" & cYear
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "SetExpireDate", ex, ""))
            End Try
        End Sub

        Public Sub Card_SetCardType(ByVal cCardType As String)
            Try
                cCardType = Replace(cCardType, " ", "")
                cCardType = Replace(cCardType, "/", "")
                cCardType = Replace(cCardType, "\", "")
                cCardType = Replace(cCardType, "_", "")
                Try
                    nCard_Type = System.Enum.Parse(GetType(CardTypes), cCardType)
                Catch ex As Exception
                    Throw New Exception("Invalid Card Type " & cCardType & " supplied")
                End Try
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "SetCardType", ex, ""))
            End Try
        End Sub

        Public Sub ePDQ_SetTransactionMode(ByVal Mode As String)
            Try
                Try
                    cePDQ_Mode = System.Enum.Parse(GetType(TransactionMode), Mode)
                Catch ex As Exception
                    Throw New Exception("Invalid Transaction Mode " & cePDQ_Mode & " supplied")
                End Try
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "SetCardType", ex, ""))
            End Try
        End Sub

        Public Function RequestAuthorisation() As Boolean
            Try
                If bMerchant_Requires3DSecure Then
                    If Request3DAuthorisation() Then
                        Return RequestCCAuthorisation()
                    Else
                        Return False
                    End If
                Else
                    Return RequestCCAuthorisation()
                End If

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "RequestAuthorisation", ex, ""))
                Return False
            End Try
        End Function

#End Region


    End Class

End Class
