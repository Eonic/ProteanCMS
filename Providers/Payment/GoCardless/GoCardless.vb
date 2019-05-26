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




Public Class GoCardlessProvider

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
        Private Const mcModuleName As String = "Providers.Providers.Eonic.AdminXForms"

        Sub New(ByRef aWeb As Cms)
            MyBase.New(aWeb)
        End Sub

    End Class

    Public Class AdminProcess
        Inherits Cms.Admin

        Dim _oAdXfm As Protean.Providers.Payment.GoCardlessProvider.AdminXForms

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
        Private Const mcModuleName As String = "Providers.Payment.GoCardless.Activities"
        Private myWeb As Protean.Cms

        Public Function GetClient() As GoCardless.GoCardlessClient

            Dim moPaymentCfg = WebConfigurationManager.GetWebApplicationSection("protean/payment")
            Dim oGoCardlessCfg As XmlNode
            Dim _opperationMode As String
            Dim cProcessInfo As String = ""
            Try
                oGoCardlessCfg = moPaymentCfg.SelectSingleNode("provider[@name='GoCardless']")
                _opperationMode = oGoCardlessCfg.SelectSingleNode("opperationMode").Attributes("value").Value()

                '  Dim acctDetails As New GoCardlessSdk.AccountDetails()
                Dim oClientApi As GoCardless.GoCardlessClient = Nothing
                Select Case LCase(_opperationMode)
                    Case "live"
                        oClientApi = GoCardless.GoCardlessClient.Create(oGoCardlessCfg.SelectSingleNode("AccessKey").Attributes("value").Value())
                    Case Else
                        oClientApi = GoCardless.GoCardlessClient.Create(oGoCardlessCfg.SelectSingleNode("SandboxAccessKey").Attributes("value").Value(), GoCardless.GoCardlessClient.Environment.SANDBOX)
                End Select

                Return oClientApi

            Catch ex As Exception
                returnException(mcModuleName, "CancelPayments", ex, "", cProcessInfo, gbDebug)
                Return Nothing
            End Try

        End Function

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
                '  Dim acctDetails As New GoCardlessSdk.AccountDetails()
                Dim oClientApi As GoCardless.GoCardlessClient = GetClient()
                Try
                    Dim tSubResp As Task(Of GoCardless.Services.SubscriptionResponse) = oClientApi.Subscriptions.CancelAsync(nPaymentProviderRef)

                    Dim oSubResp As GoCardless.Services.SubscriptionResponse = tSubResp.Result

                    Return oSubResp.Subscription.Status

                Catch ex As Exception
                    'very tricky code to return error message
                    Dim inner As Object = ex.InnerException
                    Dim myTask As Task(Of String) = inner.responseMessage.content.ReadAsStringAsync()
                    Dim ErrorMsg As String = myTask.Result
                    ' returnException(mcModuleName, "CancelPayments", ex, "", ErrorMsg, gbDebug)
                    Return Nothing
                End Try





            Catch ex As Exception
                returnException(mcModuleName, "CancelPayments", ex, "", cProcessInfo, gbDebug)
                Return ""
            End Try

        End Function


        Public Function CheckStatus(ByRef oWeb As Protean.Cms, ByRef nPaymentProviderId As Long) As String
            Dim cProcessInfo As String = ""
            Try

                Dim oClientApi As GoCardless.GoCardlessClient = GetClient()
                Try

                    Dim cPayMthdProviderRef As String
                    cPayMthdProviderRef = oWeb.moDbHelper.GetDataValue("select cPayMthdProviderRef from tblCartPaymentMethod where nPayMthdKey = " & nPaymentProviderId)

                    Dim tSubResp As Task(Of GoCardless.Services.SubscriptionResponse) = oClientApi.Subscriptions.GetAsync(cPayMthdProviderRef)

                    Dim oSubResp As GoCardless.Services.SubscriptionResponse = tSubResp.Result

                    Return oSubResp.Subscription.Status

                Catch ex As Exception
                    'very tricky code to return error message
                    Dim inner As Object = ex.InnerException
                    Dim myTask As Task(Of String) = inner.responseMessage.content.ReadAsStringAsync()
                    Dim ErrorMsg As String = myTask.Result
                    ' returnException(mcModuleName, "CheckStatus", ex, "", ErrorMsg, gbDebug)
                    Return ErrorMsg
                End Try


            Catch ex As Exception
                returnException(mcModuleName, "CheckStatus", ex, "", cProcessInfo, gbDebug)
                Return ""
            End Try

        End Function

        Public Function GetPaymentForm(ByRef oWeb As Protean.Cms, ByRef oCart As Cms.Cart, ByRef oOrder As XmlElement, Optional returnCmd As String = "cartCmd=SubmitPaymentDetails") As xForm
            Dim cProcessInfo As String = ""
            Dim moCartConfig As System.Collections.Specialized.NameValueCollection = oCart.moCartConfig
            Dim oGoCardlessCfg As XmlNode
            Dim moPaymentCfg = WebConfigurationManager.GetWebApplicationSection("protean/payment")
            Dim sRedirectURL As String = ""
            Dim _opperationMode As String = ""
            Dim bProMode As Boolean = False
            Dim _delayStart As Boolean = False
            Dim cPaymentStatus As String = ""

            myWeb = oWeb
            Try

                oGoCardlessCfg = moPaymentCfg.SelectSingleNode("provider[@name='GoCardless']")
                _opperationMode = oGoCardlessCfg.SelectSingleNode("opperationMode").Attributes("value").Value()

                If LCase(oGoCardlessCfg.SelectSingleNode("proMode").Attributes("value").Value()) = "on" Then
                    bProMode = True
                End If

                Dim orderAmt As Double = oOrder.GetAttribute("total")
                If oOrder.GetAttribute("payableType") = "" Then
                    orderAmt = oOrder.GetAttribute("total")
                Else
                    orderAmt = oOrder.GetAttribute("payableAmount")
                End If
                Dim repeatAmt As Double = CDbl("0" & oOrder.GetAttribute("repeatPrice"))
                Dim repeatInterval As String = LCase(oOrder.GetAttribute("repeatInterval"))
                Dim repeatLength As Integer = CDbl("0" & oOrder.GetAttribute("repeatLength"))
                Dim delayStart As Boolean = IIf(LCase(oOrder.GetAttribute("delayStart")) = "true", True, False)
                Dim startDate As Date

                If IsDate(oOrder.GetAttribute("startDate")) Then
                    startDate = CDate(oOrder.GetAttribute("startDate"))
                End If

                Dim sType As String = "Billing Address"
                Dim oCartAdd As XmlElement = oOrder.SelectSingleNode("Contact[@type='" & sType & "']")
                Dim orderEmail As String = oCartAdd.SelectSingleNode("Email").InnerText

                Dim OrderId As String = moCartConfig("OrderNoPrefix") & CStr(oCart.mnCartId)
                Dim Description As String = "Ref:" & oCart.OrderNoPrefix & CStr(oCart.mnCartId) & " An online purchase from: " & oCart.mcSiteURL & " on " & niceDate(Now) & " " & TimeValue(Now)

                Dim Street As String = oCartAdd.SelectSingleNode("Street").InnerText
                Dim CityName As String = oCartAdd.SelectSingleNode("City").InnerText
                Dim StateOrProvince As String = oCartAdd.SelectSingleNode("State").InnerText
                Dim PostalCode As String = oCartAdd.SelectSingleNode("PostalCode").InnerText
                Dim LastName As String = ""
                Dim FirstName As String = ""
                Dim MiddleName As String = ""
                Dim Suffix As String = ""
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
                        Suffix = aGivenName(3)
                End Select


                Dim gcXform As Protean.Cms.xForm = New Protean.Cms.xForm(myWeb)

                'Check if customer allready has a mandate
                Dim MandateRef As String = ""
                Dim ProcessPayment As Boolean = False
                Dim gcCmd As String = myWeb.moRequest("gcCmd")

                If myWeb.mnUserId > 0 Then
                    Dim userElmt As XmlElement = myWeb.moPageXml.SelectSingleNode("Page/User")
                    MandateRef = userElmt.GetAttribute("goCardlessMandate")
                    If MandateRef <> "" Then
                        'do we want to test if mandate is active
                        Dim oClientApi As GoCardless.GoCardlessClient = GetClient()
                        Try
                            Dim mandateResp As GoCardless.Services.MandateResponse = oClientApi.Mandates.GetAsync(MandateRef).Result
                            Dim mandate As GoCardless.Resources.Mandate = mandateResp.Mandate
                            Select Case mandate.Status
                                Case GoCardless.Resources.MandateStatus.Active
                                    gcCmd = "MandateExists"
                                Case GoCardless.Resources.MandateStatus.PendingSubmission
                                    gcCmd = "MandateExists"
                                Case Else
                                    'do nothing a new mandate will be created
                            End Select

                        Catch ex As Exception
                            'very tricky code to return error message
                            Dim inner As Object = ex.InnerException
                            Dim myTask As Task(Of String) = inner.responseMessage.content.ReadAsStringAsync()
                            Dim ErrorMsg As String = myTask.Result
                            'but do nothing a new mandate will be created
                        End Try




                    End If
                End If

                Select Case gcCmd
                    Case "MandateExists"
                        'do Nothing but process anyhow
                        'we could check the mandate and see if the customer wants to reuse
                        ProcessPayment = True

                    Case "callback", "return"

                        Dim oClientApi As GoCardless.GoCardlessClient = GetClient()

                        Dim redirectFlowRequest = New GoCardless.Services.RedirectFlowCompleteRequest() With {
                            .SessionToken = myWeb.moSession.SessionID
                        }

                        Dim redirectFlowResponse As GoCardless.Services.RedirectFlowResponse = oClientApi.RedirectFlows.CompleteAsync(myWeb.moRequest("redirect_flow_id"), redirectFlowRequest).Result



                        If redirectFlowResponse.ResponseMessage.IsSuccessStatusCode Then

                            'OK we have a mandate... lets create a subscription
                            MandateRef = redirectFlowResponse.RedirectFlow.Links.Mandate

                            'Save mandate against User
                            If myWeb.mnUserId > 0 Then
                                Dim userDoc As New XmlDocument
                                Dim InstanceElmt As XmlElement = userDoc.CreateElement("Instance")
                                userDoc.AppendChild(InstanceElmt)
                                InstanceElmt.InnerXml = myWeb.moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.Directory, oCart.mnEwUserId)
                                Dim userElmt As XmlElement = userDoc.SelectSingleNode("descendant-or-self::User")
                                userElmt.SetAttribute("goCardlessMandate", MandateRef)
                                myWeb.moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Directory, userDoc.DocumentElement, oCart.mnEwUserId)
                            End If

                            ProcessPayment = True

                        Else
                            gcXform.moPageXML = oWeb.moPageXml
                            gcXform.NewFrm("PayForm")
                            gcXform.addNote("PaymentDetails/accountID", Protean.xForm.noteTypes.Hint, "This response has failed.")
                            gcXform.valid = False

                        End If

                    Case "cancel"

                        gcXform.moPageXML = oWeb.moPageXml
                        gcXform.NewFrm("PayForm")


                        gcXform.submission("GoCardless Cancelled", oCart.mcPagePath & "cartCmd=Confirm", "POST", "")
                        'ccXform.submission(formname, action, "POST", "return form_check(this);")
                        'create the instance
                        Dim oFrmInstance As XmlElement = gcXform.moPageXML.CreateElement("PaymentDetails")
                        gcXform.Instance.AppendChild(oFrmInstance)
                        gcXform.Instance.FirstChild.AppendChild(gcXform.moPageXML.CreateElement("accountID"))
                        gcXform.Instance.FirstChild.AppendChild(gcXform.moPageXML.CreateElement("accountComments"))
                        ' create the UI
                        Dim oFrmGroup As XmlElement = gcXform.addGroup(gcXform.moXformElmt, "creditCard", "creditCard", "Pay by Bank Transfer")

                        gcXform.addNote("PaymentDetails/accountID", Protean.xForm.noteTypes.Hint, "You cancelled this transaction please select another payment method.")

                        gcXform.addSubmit(oFrmGroup, "GoCardless Cancelled", "Go Back", "placeOrder")

                        gcXform.valid = False

                    Case Else

                        Dim sSubmitPath = oCart.mcPagePath & returnCmd
                        Dim cCurrentURL As String = sSubmitPath
                        Dim ReturnURL As String = moCartConfig("SecureURL") & "?cartCmd=SubmitPaymentDetails&gcCmd=return" 'Return to pay selector page 
                        Dim CancelURL As String = moCartConfig("SecureURL") & "?cartCmd=SubmitPaymentDetails&gcCmd=cancel" 'Return to pay selector 
                        Dim CallbackURL As String = moCartConfig("SecureURL") & "?cartCmd=SubmitPaymentDetails&gcCmd=callback" 'Return to pay selector 

                        Dim oClientApi As GoCardless.GoCardlessClient = GetClient()

                        Dim prefilledCustomer = New GoCardless.Services.RedirectFlowCreateRequest.RedirectFlowPrefilledCustomer() With {
                            .AddressLine1 = Street,
                            .City = CityName,
                            .Region = StateOrProvince,
                            .PostalCode = PostalCode,
                            .Email = orderEmail,
                            .GivenName = FirstName,
                            .FamilyName = LastName
                        }

                        Dim redirectFlowRequest = New GoCardless.Services.RedirectFlowCreateRequest() With {
                            .Description = Description,
                            .SessionToken = myWeb.moSession.SessionID,
                            .SuccessRedirectUrl = ReturnURL,
                            .PrefilledCustomer = prefilledCustomer
                        }
                        Try
                            Dim redirectFlowResponse = oClientApi.RedirectFlows.CreateAsync(redirectFlowRequest).Result


                            Dim redirectFlow As GoCardless.Resources.RedirectFlow = redirectFlowResponse.RedirectFlow

                            Dim redirectUrl As String = redirectFlow.RedirectUrl

                            gcXform.moPageXML = oWeb.moPageXml
                            gcXform.NewFrm("PayForm")
                            gcXform.valid = False

                            myWeb.msRedirectOnEnd = redirectUrl

                        Catch ex As Exception
                            'very tricky code to return error message
                            Dim inner As Object = ex.InnerException
                            Dim myTask As Task(Of String) = inner.responseMessage.content.ReadAsStringAsync()
                            Dim ErrorMsg As String = myTask.Result
                            returnException(mcModuleName, "GetPaymentForm", ex, "", ErrorMsg, gbDebug)
                            Return Nothing
                        End Try
                End Select

                If ProcessPayment Then

                    Dim orderPrefix As String = ""
                    Dim cPaymentDetails As String = ""
                    Dim oPayElmt As XmlElement = Nothing

                    Select Case LCase(_opperationMode)
                        Case "live"
                            orderPrefix = moCartConfig("OrderNoPrefix")
                        Case Else
                            orderPrefix = "TEST-" & moCartConfig("OrderNoPrefix")
                    End Select

                    Dim oClientApi As GoCardless.GoCardlessClient = GetClient()

                    If repeatLength > 0 Then

                        'This is a subscription
                        Dim subRequest = New GoCardless.Services.SubscriptionCreateRequest()
                        subRequest.Name = oCart.mcSiteURL & " " & Description

                        subRequest.Currency = oGoCardlessCfg.SelectSingleNode("currency").Attributes("value").Value()

                        ' subRequest.AppFee = 0
                        subRequest.Amount = CLng(repeatAmt * 100)
                        subRequest.Interval = repeatLength
                        If bProMode Then
                            subRequest.PaymentReference = orderPrefix & CStr(oCart.mnCartId)
                        End If
                        Select Case repeatInterval
                            Case "day"
                                subRequest.IntervalUnit = GoCardless.Services.SubscriptionCreateRequest.SubscriptionIntervalUnit.Weekly
                            Case "week"
                                subRequest.IntervalUnit = GoCardless.Services.SubscriptionCreateRequest.SubscriptionIntervalUnit.Weekly
                            Case "month"
                                subRequest.IntervalUnit = GoCardless.Services.SubscriptionCreateRequest.SubscriptionIntervalUnit.Monthly
                            Case "year"
                                subRequest.IntervalUnit = GoCardless.Services.SubscriptionCreateRequest.SubscriptionIntervalUnit.Yearly
                        End Select

                        Dim InitialPayment As Long = 0
                        If delayStart Then
                            InitialPayment = CLng(orderAmt * 100)
                            Select Case repeatInterval
                                Case "day"
                                    subRequest.StartDate = xmlDate(DateAdd(DateInterval.Day, repeatLength, Now()))
                                Case "week"
                                    subRequest.StartDate = xmlDate(DateAdd(DateInterval.WeekOfYear, repeatLength, Now()))
                                Case "month"
                                    subRequest.StartDate = xmlDate(DateAdd(DateInterval.Month, repeatLength, Now()))
                                Case "year"
                                    subRequest.StartDate = xmlDate(DateAdd(DateInterval.Year, repeatLength, Now()))
                            End Select
                        Else
                            subRequest.StartDate = xmlDate(Now())
                            If orderAmt > 0 Then
                                InitialPayment = CLng(orderAmt + repeatAmt * 100)
                            Else
                                InitialPayment = CLng(orderAmt * 100)
                            End If
                        End If



                        subRequest.Links = New GoCardless.Services.SubscriptionCreateRequest.SubscriptionLinks() With {
                                .Mandate = MandateRef
                        }

                        If InitialPayment > 0 Then
                            Dim payReq As New GoCardless.Services.PaymentCreateRequest()
                            payReq.Amount = InitialPayment
                            Select Case oGoCardlessCfg.SelectSingleNode("currency").Attributes("value").Value()
                                Case "GBP"
                                    payReq.Currency = GoCardless.Services.PaymentCreateRequest.PaymentCurrency.GBP
                                Case "EUR"
                                    payReq.Currency = GoCardless.Services.PaymentCreateRequest.PaymentCurrency.EUR
                                Case "SEK"
                                    payReq.Currency = GoCardless.Services.PaymentCreateRequest.PaymentCurrency.SEK
                            End Select
                            payReq.Description = oCart.mcSiteURL & " Initial Payment " & Description
                            payReq.Links = New GoCardless.Services.PaymentCreateRequest.PaymentLinks() With {
                                .Mandate = MandateRef
                            }

                            Try
                                Dim payRes As GoCardless.Services.PaymentResponse = oClientApi.Payments.CreateAsync(payReq).Result
                                cPaymentDetails = payRes.Payment.Id & " "
                                cPaymentStatus = payRes.Payment.Status.ToString()

                            Catch ex As Exception
                                'very tricky code to return error message
                                Dim inner As Object = ex.InnerException
                                Dim myTask As Task(Of String) = inner.responseMessage.content.ReadAsStringAsync()
                                Dim ErrorMsg As String = myTask.Result
                                returnException(mcModuleName, "GetPaymentForm", ex, "", ErrorMsg, gbDebug)
                                Return Nothing
                            End Try
                        End If
                        If repeatAmt > 0 Then
                            Dim subResponse As GoCardless.Services.SubscriptionResponse
                            Try
                                subResponse = oClientApi.Subscriptions.CreateAsync(subRequest).Result
                            Catch ex As Exception
                                'very tricky code to return error message
                                Dim inner As Object = ex.InnerException
                                Dim myTask As Task(Of String) = inner.responseMessage.content.ReadAsStringAsync()
                                Dim ErrorMsg As String = myTask.Result
                                returnException(mcModuleName, "GetPaymentForm", ex, "", ErrorMsg, gbDebug)
                                Return Nothing
                            End Try
                            Dim ourSub As GoCardless.Resources.Subscription = subResponse.Subscription
                            cPaymentStatus = ourSub.Status.ToString()
                            cPaymentDetails = ourSub.Id
                        End If

                        ' Temporarily add the payment details to the cart so we can email them and show them
                        Dim oElmt As XmlElement
                        oPayElmt = myWeb.moPageXml.CreateElement("PaymentDetails")
                        oElmt = myWeb.moPageXml.CreateElement("Ref")
                        oElmt.InnerText = cPaymentDetails
                        oPayElmt.AppendChild(oElmt)
                        oElmt = myWeb.moPageXml.CreateElement("Type")
                        oElmt.InnerText = cPaymentStatus
                        oPayElmt.AppendChild(oElmt)

                        gcXform.moPageXML = oWeb.moPageXml
                        gcXform.NewFrm("PayForm")
                        gcXform.valid = True
                    Else
                        'Doing a single payment
                        Dim payRequest = New GoCardless.Services.PaymentCreateRequest()
                        payRequest.Amount = CInt(orderAmt * 100)
                        payRequest.Currency = GoCardless.Services.PaymentCreateRequest.PaymentCurrency.GBP
                        payRequest.Description = Description
                        If bProMode Then
                            payRequest.Reference = orderPrefix & CStr(oCart.mnCartId)
                        End If
                        payRequest.Links = New GoCardless.Services.PaymentCreateRequest.PaymentLinks() With {
                                 .Mandate = MandateRef
                            }
                        Dim payResponse As GoCardless.Services.PaymentResponse = oClientApi.Payments.CreateAsync(payRequest).Result

                        If payResponse.Payment.Status = GoCardless.Resources.PaymentStatus.PendingSubmission Then

                            cPaymentDetails = payResponse.Payment.Id

                            ' Temporarily add the payment details to the cart so we can email them and show them
                            Dim oElmt As XmlElement
                            oPayElmt = myWeb.moPageXml.CreateElement("PaymentDetails")
                            oElmt = myWeb.moPageXml.CreateElement("Ref")
                            oElmt.InnerText = cPaymentDetails
                            oPayElmt.AppendChild(oElmt)
                            oElmt = myWeb.moPageXml.CreateElement("Type")
                            oElmt.InnerText = payResponse.Payment.Status
                            oPayElmt.AppendChild(oElmt)
                            gcXform.moPageXML = oWeb.moPageXml
                            gcXform.NewFrm("PayForm")
                            gcXform.valid = True
                        Else
                            'handling of error
                            gcXform.moPageXML = oWeb.moPageXml
                            gcXform.NewFrm("PayForm")

                            gcXform.addNote("PaymentDetails/accountID", Protean.xForm.noteTypes.Hint, "This response has failed.")

                            gcXform.valid = False
                        End If
                    End If

                    If gcXform.valid Then
                        'Update Seller Notes:
                        Dim sSql As String = "select * from tblCartOrder where nCartOrderKey = " & oCart.mnCartId
                        Dim oDs As DataSet = myWeb.moDbHelper.getDataSetForUpdate(sSql, "Order", "Cart")
                        For Each oRow In oDs.Tables("Order").Rows
                            oRow("cSellerNotes") = oRow("cSellerNotes") & vbLf & Today _
                        & " " & TimeOfDay & ": changed to: (Order Placed)" & vbLf _
                        & vbLf & cPaymentDetails
                        Next
                        myWeb.moDbHelper.updateDataset(oDs, "Order")
                        oCart.mnPaymentId = myWeb.moDbHelper.savePayment(oCart.mnCartId, myWeb.mnUserId, "GoCardless", cPaymentDetails, "GoCardless", oPayElmt, Now, False, orderAmt) '0 amount paid as yet
                    End If
                End If


                Return gcXform


            Catch ex As Exception
                returnException(mcModuleName, "GetPaymentForm", ex, "", cProcessInfo, gbDebug)
                Return Nothing
            End Try
        End Function




    End Class

    Async Function GetSub(oClientApi As GoCardless.GoCardlessClient, subRequest As GoCardless.Services.SubscriptionCreateRequest) As Task(Of GoCardless.Resources.Subscription)

        Dim subscriptionResponse As GoCardless.Services.SubscriptionResponse = Await oClientApi.Subscriptions.CreateAsync(subRequest)
        Dim subscription As GoCardless.Resources.Subscription = subscriptionResponse.Subscription

        Return subscription

    End Function

End Class
