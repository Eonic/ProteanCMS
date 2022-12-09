Imports System.Xml
Imports System.Web.HttpUtility
Imports System.Web.Configuration
Imports System.IO
Imports System.Collections
Imports System.Data
Imports System.Data.SqlClient
Imports System

Partial Public Class Cms
    Partial Public Class Cart

        Partial Public Class Subscriptions

            Public Class Modules

                Public Event OnError(ByVal sender As Object, ByVal err As Protean.Tools.Errors.ErrorEventArgs)
                Private Const mcModuleName As String = "Protean.Cms.Membership.Modules"

                Public Sub New()

                    'do nowt

                End Sub

                Public Sub UpdateSubscriptionPaymentMethod(ByRef myWeb As Protean.Cms, ByRef contentNode As XmlElement)

                    Dim pseudoCart As New Protean.Cms.Cart(myWeb)
                    Dim pseudoOrder As Protean.Models.Order = Nothing

                    Dim ewCmd As String = myWeb.moRequest("subCmd2")
                    Dim ContentDetailElmt As XmlElement = myWeb.moPageXml.CreateElement("ContentDetail")
                    Dim SelectedPaymentMethod As String = String.Empty
                    Dim bPaymentMethodUpdated As Boolean = False

                    Try
processFlow:
                        Select Case ewCmd

                            Case ""
                                'Confirm Subscription Details Form
                                Dim oSubForm As Protean.Cms.Cart.Subscriptions.Forms = New Protean.Cms.Cart.Subscriptions.Forms(myWeb)
                                Dim confSubForm As XmlElement = oSubForm.xFrmConfirmSubscription(myWeb.moRequest("subId"))

                                Dim oSubmitBtn As XmlElement = confSubForm.SelectSingleNode("group/submit")
                                Dim buttonRef As String = oSubmitBtn.GetAttribute("ref")

                                If bPaymentMethodUpdated Then
                                    oSubForm.addNote(oSubForm.moXformElmt, Protean.xForm.noteTypes.Help, "Thank you, your payment method has been updated", True, "term4060")
                                End If

                                SelectedPaymentMethod = myWeb.moRequest("ewSubmit")
                                If SelectedPaymentMethod = "Update Payment Details" Then
                                    SelectedPaymentMethod = myWeb.moRequest("cPaymentMethod")
                                End If


                                If SelectedPaymentMethod <> "" Then ' equates to is submitted
                                    oSubForm.updateInstanceFromRequest()
                                    oSubForm.validate()
                                    Dim dRenewalDate As Date = CDate(oSubForm.Instance.SelectSingleNode("tblSubscription/dExpireDate").InnerText)
                                    Dim nFirstPayment As Double = 0

                                    If dRenewalDate < Now() Then
                                        nFirstPayment = CDbl(oSubForm.Instance.SelectSingleNode("tblSubscription/cSubXml/Content/SubscriptionPrices/Price[@type='sale']").InnerText)
                                        Dim oSub As New Subscriptions
                                        dRenewalDate = oSub.SubscriptionEndDate(dRenewalDate, oSubForm.Instance.SelectSingleNode("tblSubscription/cSubXml/Content"))
                                        If dRenewalDate < Now() Then
                                            oSubForm.valid = False
                                            oSubForm.addNote(oSubForm.moXformElmt, Protean.xForm.noteTypes.Alert, "Your subscription has gone beyond the date it can be renewed you must get a new subscription.", True, "term4060")

                                        End If
                                    End If

                                    If oSubForm.valid Then
                                        ewCmd = "PaymentForm"
                                        pseudoOrder = New Protean.Models.Order(myWeb.moPageXml)

                                        pseudoOrder.PaymentMethod = SelectedPaymentMethod
                                        Dim RandGen As New Random

                                        pseudoOrder.TransactionRef = "SUB" & CDbl(oSubForm.Instance.SelectSingleNode("tblSubscription/nSubKey").InnerText) & "-" & CDbl("0" & oSubForm.Instance.SelectSingleNode("tblSubscription/nPaymentMethodId").InnerText) & "-" & RandGen.Next(1000, 9999).ToString

                                        pseudoOrder.firstPayment = nFirstPayment
                                        pseudoOrder.repeatPayment = CDbl(oSubForm.Instance.SelectSingleNode("tblSubscription/cSubXml/Content/SubscriptionPrices/Price[@type='sale']").InnerText)
                                        pseudoOrder.delayStart = False ' IIf(oSubForm.Instance.SelectSingleNode("tblSubscription/cSubXml/Content/SubscriptionPrices/@delayStart").InnerText = "true", True, False)
                                        pseudoOrder.startDate = dRenewalDate
                                        pseudoOrder.repeatInterval = oSubForm.Instance.SelectSingleNode("tblSubscription/cSubXml/Content/Duration/Unit").InnerText
                                        pseudoOrder.repeatLength = CInt(oSubForm.Instance.SelectSingleNode("tblSubscription/cSubXml/Content/Duration/Length").InnerText)

                                        pseudoOrder.SetAddress(oSubForm.Instance.SelectSingleNode("Contact/cContactName").InnerText,
                                        oSubForm.Instance.SelectSingleNode("Contact/cContactEmail").InnerText,
                                        oSubForm.Instance.SelectSingleNode("Contact/cContactTel").InnerText,
                                        oSubForm.Instance.SelectSingleNode("Contact/cContactTelCountryCode").InnerText,
                                        oSubForm.Instance.SelectSingleNode("Contact/cContactCompany").InnerText,
                                        oSubForm.Instance.SelectSingleNode("Contact/cContactAddress").InnerText,
                                        oSubForm.Instance.SelectSingleNode("Contact/cContactCity").InnerText,
                                        oSubForm.Instance.SelectSingleNode("Contact/cContactState").InnerText,
                                        oSubForm.Instance.SelectSingleNode("Contact/cContactZip").InnerText,
                                        oSubForm.Instance.SelectSingleNode("Contact/cContactCountry").InnerText)

                                        GoTo processFlow
                                    Else
                                        ContentDetailElmt.AppendChild(confSubForm)
                                    End If

                                Else
                                    ContentDetailElmt.AppendChild(confSubForm)
                                End If


                            Case "PaymentForm"
                                'Call Payment Form
                                'restore the order details from the session
                                If pseudoOrder Is Nothing Then
                                    pseudoOrder = myWeb.moSession("pseudoOrder")
                                    SelectedPaymentMethod = pseudoOrder.xml.GetAttribute("paymentMethod")
                                Else
                                    myWeb.moSession("pseudoOrder") = pseudoOrder
                                End If


                                Dim oPayProv As New Providers.Payment.BaseProvider(myWeb, SelectedPaymentMethod)

                                Dim ccPaymentXform As Protean.xForm = New Protean.xForm(myWeb.msException)

                                pseudoCart.mcPagePath = pseudoCart.mcCartURL & myWeb.mcPagePath

                                ccPaymentXform = oPayProv.Activities.GetPaymentForm(myWeb, pseudoCart, pseudoOrder.xml, "?subCmd=updateSubPayment&subCmd2=PaymentForm&subId=" & myWeb.moRequest("subId"))

                                '

                                If ccPaymentXform.valid Then
                                    ewCmd = "UpdateSubscription"
                                    'Cancel Old Payment Method
                                    Dim oldPaymentId As Long = myWeb.moDbHelper.ExeProcessSqlScalar("select nPaymentMethodId from tblSubscription where nSubKey = " & myWeb.moRequest("subId"))
                                    Dim oSubs As New Protean.Cms.Cart.Subscriptions(myWeb)
                                    oSubs.CancelPaymentMethod(oldPaymentId)
                                    'Set new payment method Id
                                    myWeb.moDbHelper.ExeProcessSql("update tblSubscription set nPaymentMethodId=" & pseudoCart.mnPaymentId & "where nSubKey = " & myWeb.moRequest("subId"))
                                    ewCmd = ""
                                    bPaymentMethodUpdated = True
                                    GoTo processFlow
                                Else
                                    myWeb.moPageXml.FirstChild.AppendChild(pseudoOrder.xml)
                                    ContentDetailElmt.AppendChild(ccPaymentXform.moXformElmt)
                                End If

                            Case "UpdateSubscription"
                                'Save updated subscription details
                                ContentDetailElmt.SetAttribute("status", "SubscriptionUpdated")
                                'Cancel old method

                        End Select

                        myWeb.moPageXml.FirstChild.AppendChild(ContentDetailElmt)
                    Catch ex As Exception
                        returnException(myWeb.msException, mcModuleName, "UpdateSubscriptionPaymentMethod", ex, "", "", gbDebug)
                        'Return Nothing
                    End Try

                End Sub

                Public Sub ManageUserSubscriptions(ByRef myWeb As Protean.Cms, ByRef contentNode As XmlElement)

                    Dim sSql As String
                    Dim oDS As DataSet
                    Dim oDr As DataRow
                    Dim oElmt As XmlElement
                    Dim listSubs As Boolean = True
                    Dim listDetail As Boolean = True
                    Try

                        If myWeb.mnUserId > 0 Then

                            sSql = "select a.nStatus as status, nSubKey as id, cSubName as name, cSubXml, dStartDate, a.dPublishDate, a.dExpireDate, nPeriod as period, cPeriodUnit as periodUnit, nValueNet as value, cRenewalStatus as renewalStatus, pay.nPayMthdKey as paymentMethodId, pay.cPayMthdProviderName as providerName, pay.cPayMthdProviderRef as providerRef, pay.cPayMthdDetailXml" &
                                " from tblSubscription sub INNER JOIN tblAudit a ON sub.nAuditId = a.nAuditKey " &
                                " LEFT OUTER JOIN tblCartPaymentMethod pay on pay.nPayMthdKey = sub.nPaymentMethodId " &
                                " where sub.nDirId = " & myWeb.mnUserId


                            Select Case myWeb.moRequest("subCmd")
                                Case "updateSubPayment"
                                    UpdateSubscriptionPaymentMethod(myWeb, contentNode)
                                    listSubs = False
                                Case "cancelSub", "cancelSubscription"
                                    Dim oSubs As New Protean.Cms.Cart.Subscriptions(myWeb)
                                    oSubs.CancelSubscription(myWeb.moRequest("subId"))
                                Case "details"
                                    myWeb.moContentDetail = myWeb.moPageXml.CreateElement("ContentDetail")
                                    contentNode = myWeb.moContentDetail
                                    myWeb.moPageXml.DocumentElement.AppendChild(contentNode)
                                    myWeb.mnArtId = myWeb.moRequest("subId")
                                    sSql = sSql & " and nSubKey = " & myWeb.moRequest("subId")
                                    listSubs = True
                            End Select


                            If listSubs Then
                                oDS = myWeb.moDbHelper.GetDataSet(sSql, "tblSubscription", "OrderList")
                                For Each oDr In oDS.Tables(0).Rows
                                    oElmt = myWeb.moPageXml.CreateElement("Subscription")
                                    oElmt.InnerXml = oDr("cSubXml")
                                    oElmt.SetAttribute("status", oDr("status").ToString())
                                    oElmt.SetAttribute("id", oDr("id").ToString())
                                    oElmt.SetAttribute("name", oDr("name").ToString())
                                    oElmt.SetAttribute("startDate", xmlDate(oDr("dStartDate")))
                                    oElmt.SetAttribute("publishDate", xmlDate(oDr("dPublishDate")))
                                    oElmt.SetAttribute("expireDate", xmlDate(oDr("dExpireDate")))
                                    oElmt.SetAttribute("period", oDr("period").ToString())
                                    oElmt.SetAttribute("periodUnit", oDr("periodUnit").ToString())
                                    oElmt.SetAttribute("value", oDr("value").ToString())
                                    oElmt.SetAttribute("renewalStatus", oDr("renewalStatus").ToString())
                                    oElmt.SetAttribute("providerName", oDr("providerName").ToString())
                                    oElmt.SetAttribute("providerRef", oDr("providerRef").ToString())


                                    If oDr("providerName").ToString() <> "" Then
                                        Dim oPayProv As New Providers.Payment.BaseProvider(myWeb, oDr("providerName").ToString())

                                        Dim paymentStatus As String
                                        Try
                                            paymentStatus = oPayProv.Activities.CheckStatus(myWeb, oDr("paymentMethodId").ToString())
                                            oElmt.SetAttribute("paymentStatus", paymentStatus)
                                            Dim oPaymentMethodDetails As XmlElement = myWeb.moPageXml.CreateElement("PaymentMethodDetails")
                                            oPaymentMethodDetails.InnerXml = oPayProv.Activities.GetMethodDetail(myWeb, oDr("paymentMethodId").ToString())
                                            oElmt.AppendChild(oPaymentMethodDetails)
                                        Catch ex2 As Exception
                                            oElmt.SetAttribute("paymentStatus", "error")
                                        End Try
                                    Else
                                        oElmt.SetAttribute("paymentStatus", "unknown")
                                    End If

                                    'Get Payment Method Details
                                    Dim oPaymentMethod As XmlElement = myWeb.moPageXml.CreateElement("PaymentMethod")
                                    If Not IsDBNull(oDr("paymentMethodId")) Then
                                        oPaymentMethod.InnerXml = myWeb.moDbHelper.getObjectInstance(dbHelper.objectTypes.CartPaymentMethod, oDr("paymentMethodId"))
                                    End If
                                    ' oPaymentMethod.InnerXml = CStr(oDr("cPayMthdDetailXml") & "")
                                    oElmt.AppendChild(oPaymentMethod)

                                    contentNode.AppendChild(oElmt)
                                Next
                            End If

                            If listDetail Then

                                'show the renewal/payment history

                            End If


                        End If

                    Catch ex As Exception
                        returnException(myWeb.msException, mcModuleName, "ManageUserSubscriptions", ex, "", "", gbDebug)
                        'Return Nothing
                    End Try

                End Sub

                Public Sub Subscribe(ByRef myWeb As Protean.Cms, ByRef contentNode As XmlElement)

                    Dim listSubs As Boolean = True
                    Dim sProcessInfo As String = "Subscribe"
                    Try

                        'First we check if free trail

                        If CInt("0" & contentNode.SelectSingleNode("Prices/Price[@type='sale']").InnerText) = 0 And CInt("0" & contentNode.SelectSingleNode("SubscriptionPrices/Price[@type='sale']").InnerText) = 0 Then

                            If myWeb.mnUserId > 0 Then

                                If myWeb.moRequest("subCmd") = "Subscribe" Then

                                    Dim oSubs As New Protean.Cms.Cart.Subscriptions(myWeb)
                                    oSubs.AddUserSubscription(myWeb.mnArtId, myWeb.mnUserId)

                                    'Email site owner with new subscription details
                                    'send registration confirmation
                                    Dim oUserElmt As XmlElement = myWeb.GetUserXML(myWeb.mnUserId)
                                    Dim oMsg As Messaging = New Messaging(myWeb.msException)
                                    Dim oUserEmail As XmlElement = oUserElmt.SelectSingleNode("Email")
                                    Dim fromName As String = myWeb.moConfig("SiteAdminName")
                                    Dim fromEmail As String = myWeb.moConfig("SiteAdminEmail")
                                    Dim recipientEmail As String = ""
                                    If Not oUserEmail Is Nothing Then recipientEmail = oUserEmail.InnerText

                                    'send an email to the new registrant
                                    Dim ofs As New Protean.fsHelper()
                                    Dim xsltPath As String = ofs.checkCommonFilePath(myWeb.moConfig("ProjectPath") & "/xsl/email/subscribeTrial.xsl")

                                    Dim EmailContent As XmlElement = myWeb.moPageXml.CreateElement("Page")
                                    EmailContent.AppendChild(contentNode.CloneNode(True))
                                    EmailContent.AppendChild(oUserElmt)

                                    If IO.File.Exists(myWeb.goServer.MapPath(xsltPath)) Then


                                        Dim SubjectLine As String = "Your Trial Subscription"

                                        If Not recipientEmail = "" Then
                                            sProcessInfo = oMsg.emailer(EmailContent, xsltPath, fromName, fromEmail, recipientEmail, SubjectLine, "Message Sent", "Message Failed")
                                        End If
                                        'send an email to the webadmin

                                    End If

                                    recipientEmail = myWeb.moConfig("SiteAdminEmail")
                                    Dim xsltPath2 As String = ofs.checkCommonFilePath(myWeb.moConfig("ProjectPath") & "/xsl/email/subscribeTrialAlert.xsl")

                                    If IO.File.Exists(myWeb.goServer.MapPath(xsltPath2)) Then
                                        Dim SubjectLine As String = "New Trial Subscription"
                                        sProcessInfo = oMsg.emailer(EmailContent, xsltPath2, "New User", recipientEmail, fromEmail, SubjectLine, "Message Sent", "Message Failed")
                                    End If
                                    'Adding user to group could have added them to a triggered email list.

                                    oMsg = Nothing

                                    'Redirect user to page
                                    myWeb.msRedirectOnEnd = myWeb.mcPagePath

                                End If

                            Else
                                'If not registered we need to do so

                                Dim newContent As XmlElement = myWeb.moPageXml.CreateElement("Content")
                                newContent.SetAttribute("type", "xform")

                                Dim memberMods As New Membership.Modules()
                                memberMods.Register(myWeb, newContent)

                                contentNode.AppendChild(newContent)

                            End If

                        End If

                    Catch ex As Exception
                        returnException(myWeb.msException, mcModuleName, "ManageUserSubscriptions", ex, sProcessInfo, "", gbDebug)
                        'Return Nothing
                    End Try

                End Sub


                Public Sub CheckUpgrade(ByRef myWeb As Protean.Cms, ByRef contentNode As XmlElement)

                    Dim sProcessInfo As String = "CheckUpgrade"
                    Try

                        'Check User Logged on
                        If myWeb.mnUserId > 0 Then

                            Dim oSub As New Subscriptions(myWeb)
                            'Upgrade Price
                            oSub.UpdateSubscriptionPrice(contentNode, myWeb.mnUserId)

                        End If



                    Catch ex As Exception
                        returnException(myWeb.msException, mcModuleName, "CheckUpgrade", ex, sProcessInfo, "", gbDebug)
                        'Return Nothing
                    End Try

                End Sub

            End Class

        End Class
    End Class
End Class
