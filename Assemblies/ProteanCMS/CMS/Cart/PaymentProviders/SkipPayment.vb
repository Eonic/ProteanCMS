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
        Public Class SkipPayment

            Private Const mcModuleName As String = "Providers.Payment.SkipPayment"

            Public Sub New()
                'do nothing
            End Sub

            Public Sub Initiate(ByRef _AdminXforms As Object, ByRef _AdminProcess As Object, ByRef _Activities As Object, ByRef PayProvider As Object, ByRef myWeb As Cms)
                Dim cProcessInfo As String = ""
                Try

                    PayProvider.AdminXforms = New AdminXForms(myWeb)
                    PayProvider.AdminProcess = New AdminProcess(myWeb)
                    '   PayProvider.AdminProcess.oAdXfm = PayProvider.AdminXforms
                    PayProvider.Activities = New Activities()

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "Initiate", ex, "", cProcessInfo, gbDebug)

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


                Public Function CollectPayment(ByRef oWeb As Protean.Cms, ByVal nPaymentMethodId As Long, ByVal Amount As Double, ByVal CurrencyCode As String, ByVal PaymentDescription As String, ByRef oCart As Protean.Cms.Cart) As String
                    Dim cProcessInfo As String = ""
                    Try

                        'Do nothing because recurring payments are automatic

                        Return "Success"

                    Catch ex As Exception
                        returnException(myWeb.msException, mcModuleName, "CollectPayment", ex, "", cProcessInfo, gbDebug)
                        Return "Payment Error"
                    End Try
                End Function

                Public Function GetPaymentForm(ByRef myWeb As Protean.Cms, ByRef oCart As Cms.Cart, ByRef oOrder As XmlElement, Optional returnCmd As String = "cartCmd=SubmitPaymentDetails") As Protean.xForm
                    PerfMon.Log("Protean.Providers.payment.SkipPayment", "GetPaymentForm")
                    Dim cProcessInfo As String = ""
                    Try

                        Dim ccXform As xForm = New Protean.xForm(myWeb.moCtx)

                        ccXform.NewFrm("SkipPayment")

                        'Dim ccXform As New Protean.xForm(myWeb.moCtx)
                        ccXform.valid = True
                        Return ccXform

                    Catch ex As Exception
                        returnException(myWeb.msException, mcModuleName, "GetPaymentForm", ex, "", cProcessInfo, gbDebug)
                        Return Nothing
                    End Try
                End Function




                Public Function CheckStatus(ByRef oWeb As Protean.Cms, ByRef nPaymentProviderRef As String) As String
                    Dim cProcessInfo As String = ""
                    Try

                        Return "Error - no value returned"


                    Catch ex As Exception
                        returnException(myWeb.msException, mcModuleName, "CheckStatus", ex, "", cProcessInfo, gbDebug)
                        Return ""
                    End Try

                End Function

                Public Function CancelPayments(ByRef oWeb As Protean.Cms, ByRef nPaymentProviderRef As String) As String
                    Dim cProcessInfo As String = ""

                    Try

                        Return CheckStatus(oWeb, nPaymentProviderRef)

                    Catch ex As Exception
                        returnException(myWeb.msException, mcModuleName, "CancelPayments", ex, "", cProcessInfo, gbDebug)
                        Return ""
                    End Try

                End Function

                Public Function AddPaymentButton(ByRef oOptXform As xForm, ByRef oFrmElmt As XmlElement, ByVal configXml As XmlElement, ByVal nPaymentAmount As Double, ByVal submissionValue As String, ByVal refValue As String) As Boolean

                    Try

                        Dim nMaxAmt As Double = CDbl("0" & configXml.SelectSingleNode("MaxValue").InnerText)
                        Dim nMinAmt As Double = CDbl("0" & configXml.SelectSingleNode("MinValue").InnerText)

                        If nMaxAmt <= nPaymentAmount And nMinAmt >= nPaymentAmount Then

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

                        End If

                    Catch ex As Exception

                    End Try
                End Function


            End Class
        End Class
    End Namespace
End Namespace



