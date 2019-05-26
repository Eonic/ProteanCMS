Imports System.Xml
Imports System.Web.HttpUtility
Imports System.Web.Configuration
Imports System.IO
Imports System.Collections
Imports System.Data
Imports System.Data.SqlClient
Imports VB = Microsoft.VisualBasic
Imports Eonic


Public Class SuperCover

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

        Dim _oAdXfm As Eonic.Providers.Payment.SuperCover.AdminXForms

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
        Private Const mcModuleName As String = "Providers.Payment.SuperCover.Activities"



        Public Function GetPaymentForm(ByRef oWeb As Eonic.Web, ByRef oCart As Web.Cart, ByRef oOrder As XmlElement) As xForm

            PerfMon.Log("Providers.Payment.SuperCover.Activities", "GetPaymentForm")

            Dim myWeb As Eonic.Web = oWeb
            Dim ccXform As Eonic.Web.xForm = New Eonic.Web.xForm(myWeb)
            Dim oFrmGroup As XmlElement
            Dim cPaymentDetails As String = ""
            Dim paymentXForm As String = "payBySuperCover"

            'Get the payment options into a hashtable
            Dim oAccountCfg As XmlNode
            Dim moPaymentCfg As XmlNode
            Dim formname As String = "SuperCoverForm"
            Dim action As String = oCart.mcPagePath & "cartCmd=SubmitPaymentDetails"
            Dim processInfo As String = ""
            Try
                'Reference together the root Xml from objects
                ccXform.moPageXML = myWeb.moPageXml

                'Get the payment options xml object
                moPaymentCfg = WebConfigurationManager.GetWebApplicationSection("eonic/payment")
                oAccountCfg = moPaymentCfg.SelectSingleNode("provider[@name='SuperCover']")

                If oAccountCfg Is Nothing Then
                    Err.Raise(1003, "SuperCover", "The SuperCover provider section is yet to be added to the Eonic.Config")
                End If

                If Not oOrder.SelectSingleNode("Item/productDetail/Prices[@period='M']") Is Nothing Then
                    'Paying Monthly -  Collect Direct Debit Details.
                    'Load in the DirectDebit Form.
                    paymentXForm = "/xforms/cart/directDebit.xml"
                    If Not ccXform.load(paymentXForm) Then
                        ccXform.NewFrm(formname)
                        oFrmGroup = ccXform.addGroup(ccXform.moXformElmt, "notes", , "Missing File: " & paymentXForm)
                    Else
                        'Add Submission Buttons
                        If ccXform.moXformElmt.SelectSingleNode("model/instance/submission") Is Nothing Then
                            ccXform.submission(formname, action, "POST", "return form_check(this);")
                        End If

                        If ccXform.isSubmitted Then
                            ccXform.updateInstanceFromRequest()
                            ccXform.validate()
                        End If

                        If InStr(myWeb.moRequest("valid"), "true?oid=") Then
                            'returned back from supercover - OK lets move on.
                            myWeb.moDbHelper.UpdateSellerNotes(oCart.mnCartId, "Confirmed from SuperCover ref:" & Replace(myWeb.moRequest("valid"), "true?oid=", ""))
                            ccXform.valid = True
                        Else
                            ccXform.addValues()
                            If ccXform.valid = True Then

                                ' Temporarily add the payment details to the cart so we can email them and show them
                                Dim oPayElmt As XmlElement
                                oPayElmt = oCart.moPageXml.CreateElement("PaymentDetails")
                                oPayElmt.InnerXml = ccXform.Instance.SelectSingleNode("PaymentDetails").InnerXml
                                oOrder.AppendChild(oPayElmt)
                                For Each oPayElmt2 In ccXform.Instance.SelectNodes("PaymentDetails/*")
                                    cPaymentDetails = cPaymentDetails & oPayElmt2.Name & ": " & oPayElmt2.InnerText
                                Next
                                myWeb.moDbHelper.UpdateSellerNotes(oCart.mnCartId, cPaymentDetails)
                                myWeb.moDbHelper.savePayment(oCart.mnCartId, myWeb.mnUserId, "Direct Debit", oCart.mnCartId, "Direct Debit", oPayElmt, Now, False, 0)

                                'We have the direct debit details so now we return a form to redirect to supercover.
                                paymentXForm = "/xforms/cart/directDebitRedirect.xml"
                                If Not ccXform.load(paymentXForm) Then
                                    ccXform.NewFrm(formname)
                                    oFrmGroup = ccXform.addGroup(ccXform.moXformElmt, "notes", , "Missing File: " & paymentXForm)
                                Else
                                    ccXform.Instance.ReplaceChild(oOrder.CloneNode(True), ccXform.Instance.SelectSingleNode("Order"))
                                    ccXform.addValues()
                                End If
                                ccXform.valid = False
                            Else
                                ccXform.valid = False
                            End If
                        End If
                    End If


                Else
                    'https://www.mobileinsurance.co.uk/Get-A-Quote/?cartCmd=SubmitPaymentDetails&valid=true&orderreference=qqtp507427c9b6cdf&cplt=1&valid=0

                    'Paying Yearly - Redirect straight to SuperCover to redirect to EPDQ
                    'Dim paymentProviderResponse As String = myWeb.moRequest("status") & ""
                    'processInfo = "PPR:" & paymentProviderResponse
                    If myWeb.moRequest("valid") = "1" Or myWeb.moRequest("valid") = "0" Then
                        'returned back from supercover - OK lets move on.
                        processInfo &= " - valid form"
                        myWeb.moDbHelper.UpdateSellerNotes(oCart.mnCartId, "Confirmed from SuperCover ref:" & myWeb.moRequest("orderreference"))
                        ccXform.valid = True
                    ElseIf myWeb.moRequest("valid") <> "" Then
                        processInfo &= " - invalid response"
                        myWeb.moDbHelper.UpdateSellerNotes(oCart.mnCartId, "Payment Provider Valid response:" & myWeb.moRequest.ServerVariables("QUERY_STRING"))
                        ccXform.NewFrm(formname)
                        oFrmGroup = ccXform.addGroup(ccXform.moXformElmt, "notes", , "Payment Provider Valid response:" & myWeb.moRequest.ServerVariables("QUERY_STRING"))
                    Else
                        'Load Up the Redirect form.
                        paymentXForm = "/xforms/cart/creditCardRedirect.xml"
                        If Not ccXform.load(paymentXForm) Then
                            ccXform.NewFrm(formname)
                            oFrmGroup = ccXform.addGroup(ccXform.moXformElmt, "notes", , "Missing File: " & paymentXForm)
                        Else
                            ccXform.Instance.ReplaceChild(oOrder.CloneNode(True), ccXform.Instance.SelectSingleNode("Order"))
                            ccXform.addValues()
                        End If
                    End If


                End If

                Return ccXform

            Catch ex As Exception
                returnException(mcModuleName, "payOnAccount", ex, "", processInfo, gbDebug)
                Return Nothing
            End Try
        End Function
    End Class


End Class

