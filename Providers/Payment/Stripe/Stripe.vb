Imports System.Xml
Imports System.Web.HttpUtility
Imports System.Web
Imports System.Web.Configuration
Imports System.IO
Imports System.Collections
Imports System.Data
Imports System.Data.SqlClient
Imports VB = Microsoft.VisualBasic
Imports Protean
Imports Protean.Cms.Cart
Imports System.Net
Imports System.Threading.Tasks
Imports Stripe




Public Class StripeProvider

    Public Sub New()
        'do nothing
    End Sub

    Public Sub Initiate(ByRef _AdminXforms As Object, ByRef _AdminProcess As Object, ByRef _Activities As Object, ByRef MemProvider As Object, ByRef myWeb As Protean.Cms)
        Try
            MemProvider.AdminXforms = New AdminXForms(myWeb)
            MemProvider.AdminProcess = New AdminProcess(myWeb)
            MemProvider.AdminProcess.oAdXfm = MemProvider.AdminXforms
            MemProvider.Activities = New Activities()
        Catch ex As Exception
            returnException("GoCardlessProvider", "Initiate", ex, "", "", gbDebug)
        End Try


    End Sub

    Public Class AdminXForms
        Inherits Cms.Admin.AdminXforms
        Private Const mcModuleName As String = "Providers.Providers.Protean.AdminXForms"

        Sub New(ByRef aWeb As Cms)
            MyBase.New(aWeb)
        End Sub

    End Class

    Public Class AdminProcess
        Inherits Cms.Admin

        Dim _oAdXfm As Protean.Providers.Payment.StripeProvider.AdminXForms

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
        Private Const mcModuleName As String = "Providers.Payment.Stripe.Activities"
        Private myWeb As Protean.Cms


        Public Function CollectPayment(ByRef oWeb As Protean.Cms, ByVal nPaymentMethodId As Long, ByVal Amount As Double, ByVal CurrencyCode As String, ByVal PaymentDescription As String, ByRef oCart As Protean.Cms.Cart) As String
            Dim cProcessInfo As String = ""
            Try

                'Do nothing because recurring payments are automatic

                Return "Success"

            Catch ex As Exception
                returnException(mcModuleName, "CollectPayment", ex, "", cProcessInfo, gbDebug)
                Return "Payment Error"
            End Try
        End Function

        Public Function CancelPayments(ByRef oWeb As Protean.Cms, ByRef nPaymentProviderRef As String) As String
            Dim cProcessInfo As String = ""

            Try



            Catch ex As Exception
                returnException(mcModuleName, "CancelPayments", ex, "", cProcessInfo, gbDebug)
                Return ""
            End Try

        End Function


        Public Function CheckStatus(ByRef oWeb As Protean.Cms, ByRef nPaymentProviderId As Long) As String
            Dim cProcessInfo As String = ""
            Try




            Catch ex As Exception
                returnException(mcModuleName, "CheckStatus", ex, "", cProcessInfo, gbDebug)
                Return ""
            End Try

        End Function

        Public Function GetPaymentForm(ByRef oWeb As Protean.Cms, ByRef oCart As Cms.Cart, ByRef oOrder As XmlElement, Optional returnCmd As String = "cartCmd=SubmitPaymentDetails") As xForm
            Dim cProcessInfo As String = ""
            Dim moCartConfig As System.Collections.Specialized.NameValueCollection = oCart.moCartConfig
            Dim oStripeCfg As XmlNode
            Dim moPaymentCfg = WebConfigurationManager.GetWebApplicationSection("protean/payment")
            Dim sRedirectURL As String = ""
            Dim _opperationMode As String = ""
            Dim bProMode As Boolean = False
            Dim _delayStart As Boolean = False
            Dim cPaymentStatus As String = ""
            Dim bSecure3d As Boolean
            myWeb = oWeb
            Try
                oStripeCfg = moPaymentCfg.SelectSingleNode("provider[@name='Stripe']")
                _opperationMode = oStripeCfg.SelectSingleNode("OpperationMode").Attributes("value").Value()
                bSecure3d = IIf(oStripeCfg.SelectSingleNode("secure3d").Attributes("value").Value() = "on", True, False)
                Dim clientSecret As String = ""
                'handling of error
                Dim payXform As Protean.Cms.xForm = New Protean.Cms.xForm(myWeb)
                payXform.moPageXML = oWeb.moPageXml
                payXform.NewFrm("StripePayForm")

                Dim frmGroup As XmlElement = payXform.addGroup(payXform.moXformElmt, "stipeform")

                Dim paymentDesc As String = "Ref:" & oCart.OrderNoPrefix & CStr(oCart.mnCartId) & " An online purchase from: " & oCart.mcSiteURL & " on " & niceDate(Now) & " " & TimeValue(Now)

                ' Set your secret key: remember to change this to your live secret key in production
                ' See your keys here: https :  //dashboard.stripe.com/account/apikeys
                StripeConfiguration.SetApiKey(oStripeCfg.SelectSingleNode("SecretKey").Attributes("value").Value())


                Dim piDelAdd = New AddressOptions With {
                            .Line1 = oOrder.SelectSingleNode("Contact[@type='Delivery Address']/Street").InnerText,
                            .Line2 = "",
                            .City = oOrder.SelectSingleNode("Contact[@type='Delivery Address']/City").InnerText,
                            .State = oOrder.SelectSingleNode("Contact[@type='Delivery Address']/State").InnerText,
                            .PostalCode = oOrder.SelectSingleNode("Contact[@type='Delivery Address']/PostalCode").InnerText,
                            .Country = oOrder.SelectSingleNode("Contact[@type='Delivery Address']/Country").InnerText
                        }

                Dim piShip = New ShippingOptions With {
                            .Name = "Delivery Address",
                            .Address = piDelAdd
                            }


                Dim piChargeShipping = New ChargeShippingOptions With {
                    .Name = oOrder.SelectSingleNode("Contact[@type='Delivery Address']/GivenName").InnerText,
                    .Address = piDelAdd,
                    .Phone = oOrder.SelectSingleNode("Contact[@type='Delivery Address']/Telephone").InnerText,
                    .Carrier = oOrder.GetAttribute("shippingDesc")
                }

                Dim piCustomerId As String = myWeb.moSession("stripeCustomer")
                If piCustomerId = Nothing Then
                    Dim piCustomer = New CustomerCreateOptions With {
                            .Description = oOrder.SelectSingleNode("Contact[@type='Billing Address']/GivenName").InnerText,
                            .Email = oOrder.SelectSingleNode("Contact[@type='Billing Address']/Email").InnerText,
                            .Shipping = piShip
                        }
                    Dim piService = New CustomerService()
                    Dim Customer As Customer = piService.Create(piCustomer)
                    piCustomerId = Customer.Id
                    myWeb.moSession("stripeCustomer") = piCustomerId
                End If

                Dim itemDict As New Dictionary(Of String, String)
                Dim itemElmt As XmlElement
                For Each itemElmt In oOrder.SelectNodes("Item")
                    itemDict.Add(itemElmt.GetAttribute("quantity") & " x " & itemElmt.SelectSingleNode("Name").InnerText, itemElmt.GetAttribute("itemTotal"))
                Next
                itemDict.Add(oOrder.GetAttribute("shippingDesc"), oOrder.GetAttribute("shippingCost"))

                If myWeb.moRequest("stripeToken") <> "" Then
                    ' Token Is created using Checkout Or Elements!
                    Dim token As String = myWeb.moRequest("stripeToken")

                    If token = "success" Then
                        'This is already validated and payment collected.
                        token = myWeb.moSession("stripeCustomer")
                        payXform.valid = True
                    Else
                        'this is a real token and we need to complete the payment.
                        'create a card against the customer
                        Dim piCardOptions As New CardCreateOptions()
                        piCardOptions.SourceToken = token
                        Dim piCardSvc As New CardService()
                        Dim piCard As Card = piCardSvc.Create(piCustomerId, piCardOptions)

                        Dim options = New ChargeCreateOptions()
                        options.Amount = CStr(CDbl(oOrder.GetAttribute("total")) * 100)
                        options.Currency = oStripeCfg.SelectSingleNode("currency").Attributes("value").Value()
                        options.Description = paymentDesc
                        options.SourceId = piCard.Id
                        options.CustomerId = piCustomerId
                        options.Shipping = piChargeShipping
                        options.Metadata = itemDict

                        Dim service = New ChargeService()
                        Try
                            Dim charge As Charge = service.Create(options)
                            payXform.valid = True
                        Catch ex As Exception
                            payXform.addNote(frmGroup, Protean.xForm.noteTypes.Alert, ex.Message)
                            payXform.valid = False
                        End Try
                    End If

                    If payXform.valid Then
                            Dim oPayElmt As XmlElement
                            Dim oElmt As XmlElement
                            oPayElmt = myWeb.moPageXml.CreateElement("PaymentDetails")
                            oElmt = myWeb.moPageXml.CreateElement("Ref")
                            oElmt.InnerText = token
                            oPayElmt.AppendChild(oElmt)
                            oElmt = myWeb.moPageXml.CreateElement("Type")
                            oElmt.InnerText = cPaymentStatus
                            oPayElmt.AppendChild(oElmt)

                            'Update Seller Notes:
                            Dim sSql As String = "select * from tblCartOrder where nCartOrderKey = " & oCart.mnCartId
                            Dim oDs As DataSet = myWeb.moDbHelper.getDataSetForUpdate(sSql, "Order", "Cart")
                            For Each oRow In oDs.Tables("Order").Rows
                                oRow("cSellerNotes") = oRow("cSellerNotes") & vbLf & Today _
                    & " " & TimeOfDay & ": changed to: (Order Placed)" & vbLf _
                    & vbLf & paymentDesc
                            Next
                            myWeb.moDbHelper.updateDataset(oDs, "Order")
                            oCart.mnPaymentId = myWeb.moDbHelper.savePayment(oCart.mnCartId, myWeb.mnUserId, "Stripe", token, "CreditCard", oPayElmt, Now, False, oOrder.GetAttribute("total")) '0 amount paid as yet
                            'Clear customerId from session
                            myWeb.moSession("stripeCustomer") = Nothing
                        End If

                    End If

                    If Not payXform.valid Then
                    If bSecure3d Then
                        Dim paymentIntents = New PaymentIntentService()
                        Dim piOptions = New PaymentIntentCreateOptions With {
                            .Amount = CLng(CDbl(oOrder.GetAttribute("total")) * 100),
                            .Currency = oStripeCfg.SelectSingleNode("currency").Attributes("value").Value(),
                            .AllowedSourceTypes = New List(Of String) From {
                                "card"
                            },
                            .CustomerId = piCustomerId,
                            .Shipping = piChargeShipping
                        }
                        Dim pi As PaymentIntent = paymentIntents.Create(piOptions)
                        clientSecret = pi.ClientSecret

                    End If

                    Dim stripeInstance As XmlElement = myWeb.moPageXml.CreateElement("instance")
                    Protean.xmlTools.addElement(stripeInstance, "src", "https://checkout.stripe.com/checkout.js")
                    Protean.xmlTools.addElement(stripeInstance, "data-key", oStripeCfg.SelectSingleNode("PublishableKey").Attributes("value").Value())
                    Protean.xmlTools.addElement(stripeInstance, "data-amount", CStr(CDbl(oOrder.GetAttribute("total")) * 100))
                    Protean.xmlTools.addElement(stripeInstance, "data-name", oCart.mcSiteURL)
                    Protean.xmlTools.addElement(stripeInstance, "data-description", "Ref:" & oCart.OrderNoPrefix & CStr(oCart.mnCartId) & " An online purchase from: " & oCart.mcSiteURL & " on " & niceDate(Now) & " " & TimeValue(Now))
                    Protean.xmlTools.addElement(stripeInstance, "data-image", oStripeCfg.SelectSingleNode("CartLogo").Attributes("value").Value())
                    Protean.xmlTools.addElement(stripeInstance, "data-locale", oStripeCfg.SelectSingleNode("Locale").Attributes("value").Value())
                    Protean.xmlTools.addElement(stripeInstance, "data-zip-code", oStripeCfg.SelectSingleNode("ZipCode").Attributes("value").Value())
                    Protean.xmlTools.addElement(stripeInstance, "data-currency", LCase(oStripeCfg.SelectSingleNode("currency").Attributes("value").Value()))
                    Protean.xmlTools.addElement(stripeInstance, "data-cardholder-name", oOrder.SelectSingleNode("Contact[@type='Billing Address']/GivenName").InnerText)

                    If clientSecret <> "" Then
                        Protean.xmlTools.addElement(stripeInstance, "data-secret", clientSecret)
                    End If


                    payXform.Instance = stripeInstance
                    payXform.submission("StripePayForm", returnCmd, "POST")
                    Dim cardholderElmt As XmlElement = payXform.addInput(frmGroup, "cardholder-name", False, "Cardholder Name", "cardholder-name")
                    payXform.addValue(cardholderElmt, oOrder.SelectSingleNode("Contact[@type='Billing Address']/GivenName").InnerText)
                    '  payXform.addNote(frmGroup, Protean.xForm.noteTypes.Hint, "This form needs to be overridden with stripe XSLT template.")
                    payXform.valid = False
                End If


                Return payXform

            Catch ex As Exception
                returnException(mcModuleName, "GetPaymentForm", ex, "", cProcessInfo, gbDebug)
                Return Nothing
            End Try
        End Function




    End Class



End Class
