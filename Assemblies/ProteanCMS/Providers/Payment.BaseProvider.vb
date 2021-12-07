'***********************************************************************
' $Library:     Protean.Providers.messaging.base
' $Revision:    3.1  
' $Date:        2010-03-02
' $Author:      Trevor Spink (trevor@eonic.co.uk)
' &Website:     www.eonic.co.uk
' &Licence:     All Rights Reserved.
' $Copyright:   Copyright (c) 2002 - 2010 Eonic Ltd.
'***********************************************************************

Option Strict Off
Option Explicit On

Imports System
Imports System.Xml
Imports System.Web.HttpUtility
Imports System.Web.Configuration
Imports System.Configuration
Imports System.IO
Imports System.Collections
Imports System.Data
Imports System.Data.SqlClient
Imports System.Text.RegularExpressions
Imports System.Threading
Imports Protean.Cms
Imports Protean.Tools
Imports Protean.Tools.Xml
Imports Protean.Cms.Cart
Imports System.Net.Mail
Imports System.Reflection
Imports System.Net
Imports VB = Microsoft.VisualBasic

Namespace Providers
    Namespace Payment

        Public Class BaseProvider
            Private Const mcModuleName As String = "Protean.Providers.Payment.BaseProvider"

            Private _AdminXforms As Object
            Private _AdminProcess As Object
            Private _Activities As Object

            Protected moPaymentCfg As XmlNode

            Public Property AdminXforms() As Object
                Set(ByVal value As Object)
                    _AdminXforms = value
                End Set
                Get
                    Return _AdminXforms
                End Get
            End Property

            Public Property AdminProcess() As Object
                Set(ByVal value As Object)
                    _AdminProcess = value
                End Set
                Get
                    Return _AdminProcess
                End Get
            End Property

            Public Property Activities() As Object
                Set(ByVal value As Object)
                    _Activities = value
                End Set
                Get
                    Return _Activities
                End Get
            End Property

            Public Sub New(ByRef myWeb As Protean.Cms, ByVal ProviderName As String)
                Try
                    Dim calledType As Type
                    Dim oProviderCfg As XmlElement

                    moPaymentCfg = WebConfigurationManager.GetWebApplicationSection("protean/payment")
                    oProviderCfg = moPaymentCfg.SelectSingleNode("provider[@name='" & ProviderName & "']")

                    Dim ProviderClass As String = ""
                    If Not oProviderCfg Is Nothing Then
                        If oProviderCfg.HasAttribute("class") Then
                            ProviderClass = oProviderCfg.GetAttribute("class")
                        End If
                    Else
                        'Asssume Eonic Provider
                    End If

                    If ProviderClass = "" Then
                        ProviderClass = "Protean.Providers.Payment.DefaultProvider"
                        calledType = System.Type.GetType(ProviderClass, True)
                    Else
                        Dim moPrvConfig As Protean.ProviderSectionHandler = WebConfigurationManager.GetWebApplicationSection("protean/paymentProviders")
                        If Not moPrvConfig.Providers(ProviderClass) Is Nothing Then
                            Dim assemblyInstance As [Assembly] = [Assembly].Load(moPrvConfig.Providers(ProviderClass).Type)
                            calledType = assemblyInstance.GetType("Protean.Providers.Payment." & ProviderClass, True)
                        Else
                            calledType = System.Type.GetType("Protean.Providers.Payment." & ProviderClass, True)
                        End If

                    End If

                    Dim o As Object = Activator.CreateInstance(calledType)

                    Dim args(4) As Object
                    args(0) = _AdminXforms
                    args(1) = _AdminProcess
                    args(2) = _Activities
                    args(3) = Me
                    args(4) = myWeb

                    calledType.InvokeMember("Initiate", BindingFlags.InvokeMethod, Nothing, o, args)

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "New", ex, "", ProviderName & " Could Not be Loaded", gbDebug)
                End Try

            End Sub

        End Class

        Public Class DefaultProvider

            Public Sub New()
                'do nothing
            End Sub

            Public Sub Initiate(ByRef _AdminXforms As Object, ByRef _AdminProcess As Object, ByRef _Activities As Object, ByRef MemProvider As Object, ByRef myWeb As Protean.Cms)

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

                Dim _oAdXfm As Protean.Providers.Payment.DefaultProvider.AdminXForms

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
                Private Const mcModuleName As String = "Providers.Payment.Eonic.Activities"

                Public myWeb As Protean.Cms

                Public Sub New()

                End Sub

                Public Function GetPaymentForm(ByRef myWeb As Protean.Cms, ByRef oCart As Protean.Cms.Cart, ByRef oOrder As XmlElement, Optional returnCmd As String = "cartCmd=SubmitPaymentDetails") As xForm

                    Dim cBillingAddress As String
                    Dim moCartConfig As System.Collections.Specialized.NameValueCollection = oCart.moCartConfig
                    Dim mcPaymentMethod As String = oCart.mcPaymentMethod
                    Dim oEwProv As PaymentProviders = New PaymentProviders(oCart.myWeb)
                    Dim cProcessInfo As String = ""
                    Dim cRepeatPaymentError As String = ""

                    Try

                        oEwProv.mcCurrency = IIf(oCart.mcCurrencyCode = "", oCart.mcCurrency, oCart.mcCurrencyCode)
                        oEwProv.mcCurrencySymbol = oCart.mcCurrencySymbol
                        If oOrder.GetAttribute("payableType") = "" Then
                            oEwProv.mnPaymentAmount = oOrder.GetAttribute("total")
                        Else
                            If oOrder.GetAttribute("payableAmount") = "" Then
                                oEwProv.mnPaymentAmount = oOrder.GetAttribute("total")
                            Else
                                oEwProv.mnPaymentAmount = oOrder.GetAttribute("payableAmount")
                                oEwProv.mnPaymentMaxAmount = oOrder.GetAttribute("total")
                            End If
                            oEwProv.mcPaymentType = oOrder.GetAttribute("payableType")
                        End If
                        oEwProv.mnCartId = oCart.mnCartId
                        oEwProv.mcPaymentOrderDescription = "Ref:" & oCart.OrderNoPrefix & CStr(oCart.mnCartId) & " An online purchase from: " & oCart.mcSiteURL & " on " & niceDate(Now) & " " & TimeValue(Now)

                        If Not oOrder.SelectSingleNode("Contact[@type='Billing Address']/GivenName") Is Nothing Then
                            oEwProv.mcCardHolderName = oOrder.SelectSingleNode("Contact[@type='Billing Address']/GivenName").InnerText
                        End If

                        'Build the billing address string
                        cBillingAddress = getNodeValueByType(oOrder, "Contact[@type='Billing Address']/Street") & vbLf &
                            getNodeValueByType(oOrder, "Contact[@type='Billing Address']/City") & vbLf &
                            getNodeValueByType(oOrder, "Contact[@type='Billing Address']/State") & vbLf &
                            getNodeValueByType(oOrder, "Contact[@type='Billing Address']/Country") & vbLf

                        oEwProv.mcCardHolderAddress = cBillingAddress
                        oEwProv.moBillingContact = oOrder.SelectSingleNode("Contact[@type='Billing Address']")

                        oEwProv.mcCardHolderPostcode = getNodeValueByType(oOrder, "Contact[@type='Billing Address']/PostalCode")
                        oEwProv.mcOrderRef = oCart.OrderNoPrefix & oCart.mnCartId

                        'check products for a fullfillment date.
                        Dim oNode As XmlNode
                        Dim dFulfillment As Date = Now()
                        For Each oNode In oOrder.SelectNodes("Item/productDetail/FulfillmentDate[node()!='']")
                            If CDate(oNode.InnerText) > dFulfillment Then
                                dFulfillment = CDate(oNode.InnerText)
                            End If
                        Next

                        'check Notes for a fullfillment date
                        If Not (moCartConfig("FullfillmentDateXpath")) = "" Then
                            If oOrder.SelectSingleNode(moCartConfig("FullfillmentDateXpath")) Is Nothing Then
                                Err.Raise(1009, "invalidFullfilmentXpath", moCartConfig("FullfillmentDateXpath") & " is invalid.")
                            Else
                                dFulfillment = Date.Parse(oOrder.SelectSingleNode(moCartConfig("FullfillmentDateXpath")).Value)
                            End If
                        End If

                        oEwProv.mdFulfillmentDate = dFulfillment

                        Dim ccPaymentXform As xForm = New xForm(myWeb.msException)

                        Select Case mcPaymentMethod
                            'Case "ePDQ"
                            '    ccPaymentXform = oEwProv.PayePDQ(oOrder, oCart.mcPagePath & "cartCmd=SubmitPaymentDetails")
                            Case "SecPay", "PayPoint" ' Paypoint.Net
                                ccPaymentXform = oEwProv.paySecPay(oOrder, oCart.mcPagePath & returnCmd, oCart.mcPaymentProfile)
                            Case "PremiumCredit"
                                ccPaymentXform = oEwProv.payPremiumCredit(oOrder, oCart.mcPagePath & returnCmd)
                            Case "SecPayUkash"
                                ccPaymentXform = oEwProv.paySecPayUkash(oOrder, oCart.mcPagePath & returnCmd)
                            Case "WorldPay"
                                ccPaymentXform = oEwProv.payWorldPay(oOrder, oCart.mcPagePath & returnCmd, oCart.myWeb.mnPageId)
                            Case "NetBanx"
                                ccPaymentXform = oEwProv.payNetBanx(oOrder, oCart.mcPagePath & returnCmd, oCart.myWeb.mnPageId)
                            Case "SagePay", "ProTx"
                                ccPaymentXform = oEwProv.paySagePay(oOrder, oCart.mcPagePath & returnCmd)
                            Case "SecureTrading"
                                Dim cReturnMethod As String = "", cReturnParentTransRef As String = ""
                                ccPaymentXform = oEwProv.paySecureTrading(oOrder, oCart.mcPagePath & returnCmd, cReturnMethod, cReturnParentTransRef)
                                If cReturnMethod <> "" Then
                                    ccPaymentXform = oEwProv.paySecureTrading(oOrder, oCart.mcPagePath & returnCmd, cReturnMethod, cReturnParentTransRef)
                                End If
                            Case "MetaCharge", "PayPointMetaCharge"
                                ccPaymentXform = oEwProv.payMetaCharge(oOrder, oCart.mcPagePath & returnCmd)
                            Case "Secure Email", "SecureEmail"
                                oEwProv.mcCartEmailAddress = moCartConfig("MerchantEmail")
                                ccPaymentXform = oEwProv.paySecureEmail(oOrder, oCart.mcPagePath & returnCmd)
                            Case "Pay On Account", "PayOnAccount"
                                ccPaymentXform = oEwProv.payOnAccount(oOrder, oCart.mcPagePath & returnCmd)
                            Case "Save Order", "SaveOrder"
                                ccPaymentXform = oEwProv.saveOrder(oOrder, oCart.mcPagePath & returnCmd)
                            Case "Pay By Cash", "PayByCash"
                                ccPaymentXform = oEwProv.payByCash(oOrder, oCart.mcPagePath & returnCmd)
                            Case "AuthorizeNet"
                                ccPaymentXform = oEwProv.payAuthorizeNet(oOrder, oCart.mcPagePath & returnCmd)
                            Case "DirectDebitSecureEmail"
                                ccPaymentXform = oEwProv.payDirectDebitSecureEmail(oOrder, oCart.mcPagePath & returnCmd)

                            Case "Cheque"
                                ' to be done
                                ' Case "PayPalPro"(MOVED To PROVIDER Lib)
                              '  ccPaymentXform = oEwProv.payPayPalPro(oOrder, oCart.mcPagePath & "cartCmd=SubmitPaymentDetails", oCart.mcPaymentProfile)

                            Case "PayPalExpress"
                                If myWeb.moRequest("ppCmd") = "cancel" Then
                                    oCart.mcPaymentMethod = Nothing
                                    Dim ccXform As xForm = New Protean.xForm(myWeb.moCtx, myWeb.msException)
                                    ccXform.NewFrm("Return")
                                    ccXform.valid = False
                                    Return ccXform
                                Else
                                    ccPaymentXform = oEwProv.payPayPalExpress(oOrder, oCart.mcPagePath & returnCmd, oCart.mcPaymentProfile)
                                End If

                            Case Else
                                If InStr(mcPaymentMethod, "Repeat_") > 0 Then
                                    'get repeat id
                                    Dim cOld As String = ""
                                    Dim cNew As String = ""
                                    Dim i As Integer = 1
                                    Dim nStart As Integer = InStr(mcPaymentMethod, "Repeat_") + 6
                                    Do Until (Not IsNumeric(cNew) And Not cNew = "") Or (CInt(nStart) + (i - 1)) >= mcPaymentMethod.Length
                                        cOld = cNew
                                        cNew = mcPaymentMethod.Substring(nStart, i)
                                        i += 1
                                    Loop
                                    If cOld = "" Or (CInt(nStart) + (i - 1)) >= mcPaymentMethod.Length Then cOld = cNew
                                    'ok, cOld is the new id
                                    ccPaymentXform = oEwProv.RepeatConfirmation(cOld, oOrder, oCart.mcPagePath & returnCmd, cRepeatPaymentError)
                                    If ccPaymentXform.valid = True Then
                                        oEwProv.ValidatePaymentByCart(oCart.mnCartId, True)
                                    End If

                                Else
                                    ' TS 16 Jan 2020 we should never default to pay on account.

                                    '  ccPaymentXform = oEwProv.payOnAccount(oOrder, oCart.mcPagePath & returnCmd)
                                End If
                        End Select

                        oCart.mnPaymentId = oEwProv.savedPaymentId

                        If Not cRepeatPaymentError = "" Then
                            ccPaymentXform.addNote("RepeatError", xForm.noteTypes.Alert, cRepeatPaymentError, True)
                        End If

                        If Not (oOrder.SelectSingleNode("error/msg") Is Nothing) Then
                            oOrder.SelectSingleNode("error").PrependChild(oOrder.OwnerDocument.CreateElement("msg"))
                            oOrder.SelectSingleNode("error").FirstChild.InnerXml = "<strong>PAYMENT CANNOT PROCEED UNTIL QUANTITIES ARE ADJUSTED</strong>"
                        ElseIf ccPaymentXform.valid = True Then
                            'added ability to change the success ID in payment providers



                        End If

                        Return ccPaymentXform

                        oEwProv = Nothing

                    Catch ex As Exception
                        returnException(myWeb.msException, mcModuleName, "GetPaymentForm", ex, "", cProcessInfo, gbDebug)
                        Return Nothing
                    End Try

                End Function

                Function GetRedirect3dsForm(ByRef myWeb As Protean.Cms) As xForm
                    PerfMon.Log("EPDQ", "xfrmSecure3DReturn")
                    Dim moCartConfig As System.Collections.Specialized.NameValueCollection = myWeb.moCart.moCartConfig
                    Dim oXform As xForm = New Protean.Cms.xForm(myWeb.msException)
                    Dim oFrmInstance As XmlElement
                    Dim oFrmGroup As XmlElement
                    Dim RedirectURL As String

                    Dim cProcessInfo As String = "xfrmSecure3DReturn"
                    Try
                        oXform.moPageXML = myWeb.moPageXml

                        If moCartConfig("SecureURL").EndsWith("/") Then
                            RedirectURL = moCartConfig("SecureURL") & "?cartCmd=SubmitPaymentDetails&paymentMethod=" & myWeb.moCart.mcPaymentMethod
                        Else
                            RedirectURL = moCartConfig("SecureURL") & "/?cartCmd=SubmitPaymentDetails&paymentMethod=" & myWeb.moCart.mcPaymentMethod
                        End If

                        'create the instance
                        oXform.NewFrm("Secure3DReturn")
                        oXform.submission("Secure3DReturn", goServer.UrlDecode(RedirectURL), "POST", "return form_check(this);")
                        oFrmInstance = oXform.moPageXML.CreateElement("Secure3DReturn")
                        oXform.Instance.AppendChild(oFrmInstance)
                        oFrmGroup = oXform.addGroup(oXform.moXformElmt, "Secure3DReturn1", "Secure3DReturn1", "Redirecting... Please do not refresh")
                        Dim item As Object

                        For Each item In myWeb.moRequest.Form
                            Dim newInput As XmlNode = oXform.addInput(oFrmGroup, CStr(item), False, CStr(item), "hidden")
                            oXform.addValue(newInput, myWeb.moRequest.Form(CStr(item)))
                        Next

                        'build the form and the binds
                        'oXform.addDiv(oFrmGroup, "<SCRIPT LANGUAGE=""Javascript"">function onXformLoad(){document.Secure3DReturn.submit();};appendLoader(onXformLoad);</SCRIPT>")
                        oXform.addSubmit(oFrmGroup, "Secure3DReturn", "Continue", "ewSubmit")
                        oXform.addValues()
                        Return oXform

                    Catch ex As Exception
                        returnException(myWeb.msException, mcModuleName, "GetRedirect3dsForm", ex, "", cProcessInfo, gbDebug)
                        Return Nothing
                    End Try

                End Function


                Public Function CheckStatus(ByRef oWeb As Protean.Cms, ByRef nPaymentProviderRef As String) As String
                    Dim cProcessInfo As String = ""
                    Try

                        Return "Manual"

                    Catch ex As Exception
                        returnException(myWeb.msException, mcModuleName, "CheckStatus", ex, "", cProcessInfo, gbDebug)
                        Return ""
                    End Try

                End Function


                Public Function UpdatePaymentStatus(ByRef oWeb As Protean.Cms, ByRef nPaymentMethodKey As Long) As XmlElement
                    Dim cProcessInfo As String = ""
                    Try
                        Dim oDoc As XmlElement = oWeb.moPageXml.CreateElement("PaymentMethod")
                        oDoc.InnerXml = oWeb.moDbHelper.getObjectInstance(dbHelper.objectTypes.CartPaymentMethod, nPaymentMethodKey)
                        Return oDoc.FirstChild

                    Catch ex As Exception
                        returnException(myWeb.msException, mcModuleName, "UpdatePaymentStatus", ex, "", cProcessInfo, gbDebug)
                        Return Nothing
                    End Try

                End Function


                Public Function GetMethodDetail(ByRef oWeb As Protean.Cms, ByRef nPaymentProviderRef As String) As String
                    Dim cProcessInfo As String = ""
                    Try

                        Return "Not Available"

                    Catch ex As Exception
                        returnException(myWeb.msException, mcModuleName, "CheckStatus", ex, "", cProcessInfo, gbDebug)
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

                Public Sub ValidatePaymentByCart(ByVal nCartId As Integer, ByVal bValid As Boolean)
                    Try
                        'sets the validity of the payment
                        'get the audit id 
                        Dim cSQL As String = "SELECT tblAudit.nAuditKey FROM tblCartOrder INNER JOIN tblCartPaymentMethod ON tblCartOrder.nPayMthdId = tblCartPaymentMethod.nPayMthdKey INNER JOIN tblAudit ON tblCartPaymentMethod.nAuditId = tblAudit.nAuditKey WHERE tblCartOrder.nCartOrderKey = " & nCartId
                        Dim nAuditId As String = myWeb.moDbHelper.ExeProcessSqlScalar(cSQL)
                        If IsNumeric(nAuditId) Then

                            Dim oXml As XmlDocument = New XmlDocument
                            Dim oInstance As XmlElement = oXml.CreateElement("Instance")
                            Dim oElmt As XmlElement = oXml.CreateElement("tblAudit")
                            addNewTextNode("nAuditKey", oElmt, nAuditId)
                            addNewTextNode("nStatus", oElmt, IIf(bValid, 1, 0))
                            oInstance.AppendChild(oElmt)

                            myWeb.moDbHelper.setObjectInstance(dbHelper.objectTypes.Audit, oInstance, nAuditId)

                        End If
                    Catch ex As Exception
                        returnException(myWeb.msException, mcModuleName, "ValidatePaymentByCart", ex, "", "", gbDebug)
                    End Try
                End Sub



            End Class
        End Class

    End Namespace
End Namespace
