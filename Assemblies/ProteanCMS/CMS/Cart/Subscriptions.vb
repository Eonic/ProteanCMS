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

        Public Class Subscriptions

            Private mcModuleName As String = "Subscriptions"
            Private oSubConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/subscriptions")

            Dim myWeb As Cms
            Dim myCart As Cms.Cart

            Public Sub New(ByRef aWeb As Cms)
                myWeb = aWeb
                myCart = myWeb.moCart
            End Sub

            Public Sub New(ByRef aCart As Cms.Cart)
                myCart = aCart
                myWeb = myCart.myWeb
            End Sub

            Public Sub New()
                'wont do anything here
            End Sub

#Region "Admin"

            Public Sub ListSubscriptions(ByRef oParentElmt As XmlElement)
                Try


                    'List Subscription groups and thier subscriptions.
                    Dim oDS As DataSet = myWeb.moDbHelper.GetDataSet("Select * From tblCartProductCategories WHERE cCatSchemaName = 'Subscription'", "SubscriptionGroups")
                    myWeb.moDbHelper.addTableToDataSet(oDS, "SELECT tblContent.*, tblCartCatProductRelations.nCatId FROM tblContent LEFT OUTER JOIN tblCartCatProductRelations ON tblContent.nContentKey = tblCartCatProductRelations.nContentId WHERE (tblContent.cContentSchemaName = 'Subscription') Order By tblCartCatProductRelations.nDisplayOrder", "Subscriptions")
                    If oDS.Tables.Count = 2 Then
                        oDS.Relations.Add("GrpRel", oDS.Tables("SubscriptionGroups").Columns("nCatKey"), oDS.Tables("Subscriptions").Columns("nCatId"), False)
                        oDS.Relations("GrpRel").Nested = True
                    End If
                    Dim oDT As DataTable
                    Dim oDC As DataColumn
                    For Each oDT In oDS.Tables
                        For Each oDC In oDT.Columns
                            oDC.ColumnMapping = MappingType.Attribute
                        Next
                    Next
                    If oDS.Tables.Contains("Subscriptions") Then
                        oDS.Tables("Subscriptions").Columns("cContentXmlBrief").ColumnMapping = MappingType.Hidden
                        oDS.Tables("Subscriptions").Columns("nCatId").ColumnMapping = MappingType.Hidden
                        oDS.Tables("Subscriptions").Columns("cContentXmlDetail").ColumnMapping = MappingType.SimpleContent
                    End If
                    Dim oXML As New XmlDocument
                    oXML.InnerXml = Replace(Replace(oDS.GetXml, "&lt;", "<"), "&gt;", ">")
                    Dim oElmt As XmlElement
                    For Each oElmt In oXML.DocumentElement.SelectNodes("*")
                        oParentElmt.AppendChild(oParentElmt.OwnerDocument.ImportNode(oElmt, True))
                    Next

                Catch ex As Exception
                    returnException(mcModuleName, "ListSubscriptions", ex, "", "", gbDebug)
                End Try
            End Sub

            Public Function SubscriptionToGroup(ByVal nSubId As Integer, ByVal nSubGroup As Integer) As Integer
                Try
                    Dim cSQL As String = ""
                    Dim nID As Integer = 0
                    'check if exists
                    cSQL = "SELECT nCatProductRelKey FROM tblCartCatProductRelations WHERE nContentId = " & nSubId & " AND nCatId = " & nSubGroup
                    nID = myWeb.moDbHelper.ExeProcessSqlScalar(cSQL)
                    'if it does then fine, just return id
                    If nID > 0 Then Return nID
                    'if not need to get the last order number
                    cSQL = "SELECT nCatProductRelKey, nDisplayOrder FROM tblCartCatProductRelations WHERE nCatId = " & nSubGroup & " ORDER BY nDisplayOrder DESC"
                    nID = myWeb.moDbHelper.ExeProcessSqlScalar(cSQL)
                    'add to group as bottom
                    cSQL = "INSERT INTO tblCartCatProductRelations (nContentId, nCatId, nDisplayOrder, nAuditId) VALUES (" & nSubId & ", " & nSubGroup & ", " & nID + 1 & ", " & myWeb.moDbHelper.getAuditId & ")"
                    nID = myWeb.moDbHelper.GetIdInsertSql(cSQL)
                    Return nID
                Catch ex As Exception
                    returnException(mcModuleName, "SubscriptionToGroup", ex, "", "", gbDebug)
                End Try
            End Function

            Public Sub ListSubscribers(ByRef oParentElmt As XmlElement)
                Try
                    Dim sSql As String = "select dir.cDirName, dir.cDirXml, sub.*, a.* from tblSubscription sub inner join tblDirectory dir on dir.nDirKey = sub.nDirId inner join tblAudit a on a.nAuditKey = sub.nAuditId  where sub.nSubContentId = " & myWeb.moRequest("id")

                    'List Subscription groups and thier subscriptions.
                    Dim oDS As DataSet = myWeb.moDbHelper.GetDataSet(sSql, "Subscribers")
                    Dim oXML As New XmlDocument

                    oXML.InnerXml = Replace(Replace(oDS.GetXml, "&lt;", "<"), "&gt;", ">")
                    Dim oElmt As XmlElement
                    Dim sContent As String

                    For Each oElmt In oXML.SelectNodes("descendant-or-self::cDirXml | descendant-or-self::cSubXml")
                        sContent = oElmt.InnerXml
                        If sContent <> "" Then
                            oElmt.InnerXml = sContent
                        End If
                    Next

                    Dim oElmt2 As XmlElement
                    For Each oElmt2 In oXML.DocumentElement.SelectNodes("*")
                        oParentElmt.AppendChild(oParentElmt.OwnerDocument.ImportNode(oElmt2, True))
                    Next

                Catch ex As Exception
                    returnException(mcModuleName, "ListSubscriptions", ex, "", "", gbDebug)
                End Try
            End Sub


            Public Sub ListUpcomingRenewals(ByRef oParentElmt As XmlElement, Optional expiredMarginDays As Int16 = -5, Optional renewRangePeriod As String = "month", Optional renewRangeCount As Int16 = 12)
                Try

                    Dim ExpireRange As String = ""
                    Select Case LCase(renewRangePeriod)
                        Case "month"
                            ExpireRange = sqlDate(Now().AddMonths(renewRangeCount * 1))
                        Case "week"
                            ExpireRange = sqlDate(Now().AddDays(renewRangeCount * 7))
                        Case "day"
                            ExpireRange = sqlDate(Now().AddDays(renewRangeCount * 1))
                    End Select

                    Dim sSql As String = "select dir.cDirName, dir.cDirXml, sub.*, pay.cPayMthdProviderName, pay.cPayMthdCardType,pay.cPayMthdDescription, pay.cPayMthdDetailXml, a.* from tblSubscription sub" _
                        & " inner join tblDirectory dir on dir.nDirKey = sub.nDirId" _
                        & " inner join tblAudit a on a.nAuditKey = sub.nAuditId" _
                        & " LEFT OUTER JOIN tblCartPaymentMethod pay on sub.nPaymentMethodId = pay.nPayMthdKey" _
                        & " where a.dExpireDate >= " & sqlDate(Now().AddDays(expiredMarginDays)) & "and a.dExpireDate <= " & ExpireRange _
                        & " and sub.cRenewalStatus = 'Rolling' order by a.dExpireDate"

                    'List Subscription groups and thier subscriptions.
                    Dim oDS As DataSet = myWeb.moDbHelper.GetDataSet(sSql, "Subscribers")
                    Dim oXML As New XmlDocument

                    oXML.InnerXml = Replace(Replace(oDS.GetXml, "&lt;", "<"), "&gt;", ">")
                    Dim oElmt As XmlElement
                    Dim sContent As String

                    For Each oElmt In oXML.SelectNodes("descendant-or-self::cDirXml | descendant-or-self::cSubXml | descendant-or-self::cPayMthdDetailXml")
                        sContent = oElmt.InnerXml
                        If sContent <> "" Then
                            oElmt.InnerXml = sContent
                        End If
                    Next

                    Dim oElmt2 As XmlElement
                    For Each oElmt2 In oXML.DocumentElement.SelectNodes("*")
                        oParentElmt.AppendChild(oParentElmt.OwnerDocument.ImportNode(oElmt2, True))
                    Next

                Catch ex As Exception
                    returnException(mcModuleName, "ListSubscriptions", ex, "", "", gbDebug)
                End Try
            End Sub

            Public Sub ListRecentRenewals(ByRef oParentElmt As XmlElement)
                Try
                    Dim sSql As String = "select dir.cDirName, dir.cDirXml, sub.*, pay.cPayMthdProviderName, pay.cPayMthdCardType,pay.cPayMthdDescription, pay.cPayMthdDetailXml, a.* from tblSubscription sub" _
                        & " inner join tblDirectory dir on dir.nDirKey = sub.nDirId" _
                        & " inner join tblAudit a on a.nAuditKey = sub.nAuditId" _
                        & " LEFT OUTER JOIN tblCartPaymentMethod pay on sub.nPaymentMethodId = pay.nPayMthdKey" _
                        & " where a.dExpireDate >= " & sqlDate(Now().AddDays(-5)) & "and a.dExpireDate <= " & sqlDate(Now().AddMonths(3)) _
                        & " and sub.cRenewalStatus = 'Rolling' order by a.dExpireDate"

                    'List Subscription groups and thier subscriptions.
                    Dim oDS As DataSet = myWeb.moDbHelper.GetDataSet(sSql, "Subscribers")
                    Dim oXML As New XmlDocument

                    oXML.InnerXml = Replace(Replace(oDS.GetXml, "&lt;", "<"), "&gt;", ">")
                    Dim oElmt As XmlElement
                    Dim sContent As String

                    For Each oElmt In oXML.SelectNodes("descendant-or-self::cDirXml | descendant-or-self::cSubXml | descendant-or-self::cPayMthdDetailXml")
                        sContent = oElmt.InnerXml
                        If sContent <> "" Then
                            oElmt.InnerXml = sContent
                        End If
                    Next

                    Dim oElmt2 As XmlElement
                    For Each oElmt2 In oXML.DocumentElement.SelectNodes("*")
                        oParentElmt.AppendChild(oParentElmt.OwnerDocument.ImportNode(oElmt2, True))
                    Next

                Catch ex As Exception
                    returnException(mcModuleName, "ListSubscriptions", ex, "", "", gbDebug)
                End Try
            End Sub

            Public Sub ListExpiredSubscriptions(ByRef oParentElmt As XmlElement, Optional expiredMarginDays As Int16 = 0, Optional renewRangePeriod As String = "", Optional renewRangeCount As Int16 = 0)
                Try

                    Dim ExpireRange As String = ""
                    Select Case LCase(renewRangePeriod)
                        Case "month"
                            ExpireRange = sqlDate(Now().AddMonths(renewRangeCount * -1))
                        Case "week"
                            ExpireRange = sqlDate(Now().AddDays(renewRangeCount * -7))
                        Case "day"
                            ExpireRange = sqlDate(Now().AddDays(renewRangeCount * -1))
                    End Select


                    Dim sSql As String = "select dir.cDirName, dir.cDirXml, sub.*, pay.cPayMthdProviderName, pay.cPayMthdCardType,pay.cPayMthdDescription, pay.cPayMthdDetailXml, a.* from tblSubscription sub" _
                        & " inner join tblDirectory dir on dir.nDirKey = sub.nDirId" _
                        & " inner join tblAudit a on a.nAuditKey = sub.nAuditId" _
                        & " LEFT OUTER JOIN tblCartPaymentMethod pay on sub.nPaymentMethodId = pay.nPayMthdKey"

                    If ExpireRange <> "" Then
                        sSql = sSql & " where a.dExpireDate >= " & ExpireRange & "and a.dExpireDate <= " & sqlDate(Now().AddDays(expiredMarginDays * -1))

                    Else
                        sSql = sSql & " where a.dExpireDate <= " & sqlDate(Now().AddDays(expiredMarginDays * -1))

                    End If

                    sSql = sSql & " and sub.cRenewalStatus <> 'Cancelled'  order by a.dExpireDate desc"

                    'List Subscription groups and thier subscriptions.
                    Dim oDS As DataSet = myWeb.moDbHelper.GetDataSet(sSql, "Subscribers")
                    Dim oXML As New XmlDocument

                    oXML.InnerXml = Replace(Replace(oDS.GetXml, "&lt;", "<"), "&gt;", ">")
                    Dim oElmt As XmlElement
                    Dim sContent As String

                    For Each oElmt In oXML.SelectNodes("descendant-or-self::cDirXml | descendant-or-self::cSubXml | descendant-or-self::cPayMthdDetailXml")
                        sContent = oElmt.InnerXml
                        If sContent <> "" Then
                            oElmt.InnerXml = sContent
                        End If
                    Next

                    Dim oElmt2 As XmlElement
                    For Each oElmt2 In oXML.DocumentElement.SelectNodes("*")
                        oParentElmt.AppendChild(oParentElmt.OwnerDocument.ImportNode(oElmt2, True))
                    Next

                Catch ex As Exception
                    returnException(mcModuleName, "ListSubscriptions", ex, "", "", gbDebug)
                End Try
            End Sub

            Public Sub ListCancelledSubscriptions(ByRef oParentElmt As XmlElement)
                Try
                    Dim sSql As String = "select dir.cDirName, dir.cDirXml, sub.*, pay.cPayMthdProviderName, pay.cPayMthdCardType,pay.cPayMthdDescription, pay.cPayMthdDetailXml, a.* from tblSubscription sub" _
                        & " inner join tblDirectory dir on dir.nDirKey = sub.nDirId" _
                        & " inner join tblAudit a on a.nAuditKey = sub.nAuditId" _
                        & " LEFT OUTER JOIN tblCartPaymentMethod pay on sub.nPaymentMethodId = pay.nPayMthdKey" _
                        & " where sub.cRenewalStatus = 'Cancelled'  order by a.dExpireDate desc"

                    'List Subscription groups and thier subscriptions.
                    Dim oDS As DataSet = myWeb.moDbHelper.GetDataSet(sSql, "Subscribers")
                    Dim oXML As New XmlDocument

                    oXML.InnerXml = Replace(Replace(oDS.GetXml, "&lt;", "<"), "&gt;", ">")
                    Dim oElmt As XmlElement
                    Dim sContent As String

                    For Each oElmt In oXML.SelectNodes("descendant-or-self::cDirXml | descendant-or-self::cSubXml | descendant-or-self::cPayMthdDetailXml")
                        sContent = oElmt.InnerXml
                        If sContent <> "" Then
                            oElmt.InnerXml = sContent
                        End If
                    Next

                    Dim oElmt2 As XmlElement
                    For Each oElmt2 In oXML.DocumentElement.SelectNodes("*")
                        oParentElmt.AppendChild(oParentElmt.OwnerDocument.ImportNode(oElmt2, True))
                    Next

                Catch ex As Exception
                    returnException(mcModuleName, "ListSubscriptions", ex, "", "", gbDebug)
                End Try
            End Sub

            Public Function GetSubscriptionDetail(ByRef oParentElmt As XmlElement, ByVal nSubId As Integer) As XmlElement
                Dim sSQL As String
                Dim oDs As DataSet
                Dim oDr As DataRow
                Dim oElmt As XmlElement
                Try

                    If oParentElmt Is Nothing Then
                        oParentElmt = myWeb.moPageXml.CreateElement("Subscription")
                    End If

                    sSQL = "select a.nStatus as status,  nSubKey as id, nSubContentId as contentId, nDirId as userId, nOrderId as orderId, cSubName as name, cSubXml, dStartDate, a.dPublishDate, a.dExpireDate, a.cDescription as cancelReason,a.dUpdateDate as cancelDate,a.nUpdateDirId as cancelUserId, nPeriod as period, cPeriodUnit as periodUnit, nValueNet as value, cRenewalStatus as renewalStatus, pay.nPayMthdKey as providerId,pay.cPayMthdProviderName as providerName, pay.cPayMthdProviderRef as providerRef" &
                            " from tblSubscription sub INNER JOIN tblAudit a ON sub.nAuditId = a.nAuditKey " &
                            " LEFT OUTER JOIN tblCartPaymentMethod pay on pay.nPayMthdKey = sub.nPaymentMethodId " &
                            " where sub.nSubKey = " & nSubId
                    myWeb.moContentDetail = myWeb.moPageXml.CreateElement("ContentDetail")

                    oDs = myWeb.moDbHelper.GetDataSet(sSQL, "tblSubscription", "OrderList")
                    For Each oDr In oDs.Tables(0).Rows
                        oElmt = myWeb.moPageXml.CreateElement("Subscription")
                        oElmt.InnerXml = oDr("cSubXml")
                        oElmt.SetAttribute("status", oDr("status").ToString())
                        oElmt.SetAttribute("id", oDr("id").ToString())
                        oElmt.SetAttribute("contentId", oDr("contentId").ToString())
                        oElmt.SetAttribute("userId", oDr("userId").ToString())
                        oElmt.SetAttribute("orderId", oDr("orderId").ToString())
                        oElmt.SetAttribute("name", oDr("name").ToString())
                        oElmt.SetAttribute("startDate", xmlDate(oDr("dStartDate")))
                        oElmt.SetAttribute("publishDate", xmlDate(oDr("dPublishDate")))
                        oElmt.SetAttribute("expireDate", xmlDate(oDr("dExpireDate")))
                        oElmt.SetAttribute("period", oDr("period").ToString())
                        oElmt.SetAttribute("periodUnit", Trim(oDr("periodUnit").ToString()))
                        oElmt.SetAttribute("value", oDr("value").ToString())
                        oElmt.SetAttribute("renewalStatus", oDr("renewalStatus").ToString())
                        oElmt.SetAttribute("providerId", oDr("providerId").ToString())
                        oElmt.SetAttribute("providerName", oDr("providerName").ToString())
                        oElmt.SetAttribute("providerRef", oDr("providerRef").ToString())
                        oElmt.SetAttribute("cancelReason", oDr("cancelReason").ToString())
                        oElmt.SetAttribute("cancelDate", oDr("cancelDate").ToString())
                        oElmt.SetAttribute("cancelUserId", oDr("cancelUserId").ToString())

                        If oDr("providerName").ToString() <> "" Then
                            Dim oPayProv As New Providers.Payment.BaseProvider(myWeb, oDr("providerName").ToString())
                            Dim paymentStatus As String
                            Try
                                paymentStatus = oPayProv.Activities.CheckStatus(myWeb, oDr("providerId"))
                                oElmt.SetAttribute("paymentStatus", paymentStatus)
                            Catch ex2 As Exception
                                oElmt.SetAttribute("paymentStatus", "error")
                            End Try
                        Else
                            oElmt.SetAttribute("paymentStatus", "unknown")
                        End If

                        'Get user Info
                        oElmt.AppendChild(myWeb.GetUserXML(oDr("userId")))

                        'Get the renewal Info
                        sSQL = "select a.dPublishDate as startDate, a.dExpireDate as endDate, sub.nPaymentMethodId as payMthdId,  pay.cPayMthdProviderName as providerName, sub.xNotesXml, sub.nOrderId as orderId" &
                            " from tblSubscriptionRenewal sub INNER JOIN tblAudit a ON sub.nAuditId = a.nAuditKey " &
                            " LEFT OUTER JOIN tblCartPaymentMethod pay on pay.nPayMthdKey = sub.nPaymentMethodId " &
                            " where sub.nSubId = " & nSubId

                        oDs = myWeb.moDbHelper.GetDataSet(sSQL, "Renewal", "Renewals")
                        If Not oDs Is Nothing Then
                            oDs.Tables(0).Columns("startDate").ColumnMapping = Data.MappingType.Attribute
                            oDs.Tables(0).Columns("endDate").ColumnMapping = Data.MappingType.Attribute
                            oDs.Tables(0).Columns("providerName").ColumnMapping = Data.MappingType.Attribute
                            oDs.Tables(0).Columns("orderId").ColumnMapping = Data.MappingType.Attribute
                            oDs.Tables(0).Columns("payMthdId").ColumnMapping = Data.MappingType.Attribute
                            oDs.Tables(0).Columns("xNotesXml").ColumnMapping = Data.MappingType.SimpleContent

                            Dim elmtRenewals As XmlElement = myWeb.moPageXml.CreateElement("Renewals")
                            elmtRenewals.InnerXml = oDs.GetXml()
                            For Each renewalElmt As XmlElement In elmtRenewals.SelectNodes("Renewals/Renewal")
                                renewalElmt.InnerXml = renewalElmt.InnerText
                            Next
                            oElmt.AppendChild(elmtRenewals.FirstChild)
                        End If

                        oParentElmt.AppendChild(oElmt)
                    Next
                    Return oParentElmt
                Catch ex As Exception
                    returnException(mcModuleName, "GetSubscriptionDetail", ex, "", "", gbDebug)
                End Try
            End Function

            Public Sub ListRenewalAlerts(ByRef oParentElmt As XmlElement, Optional ByVal bProcess As Boolean = False)
                Try
                    Dim moReminderCfg As XmlElement = WebConfigurationManager.GetWebApplicationSection("protean/subscriptionReminders")
                    oParentElmt.InnerXml = moReminderCfg.OuterXml
                    Dim ProcessedCount As Long = 0
                    Dim oReminder As XmlElement

                    If myWeb.moRequest("process") = "all" Then
                        bProcess = True
                    End If

                    For Each oReminder In oParentElmt.SelectNodes("subscriptionReminders/reminder")

                        Select Case oReminder.GetAttribute("action")
                            Case "renewalreminder", "renew"
                                'Select the subscriptions that are caught up in this case
                                ListUpcomingRenewals(oReminder, 0, oReminder.GetAttribute("period"), oReminder.GetAttribute("count"))
                                Dim subxml As XmlElement
                                For Each subxml In oReminder.SelectNodes("Subscribers")
                                    Dim force As Boolean = False
                                    If bProcess Then
                                        force = True
                                    End If
                                    Dim ingoreIfPaymentActive As Boolean = False
                                    Dim actionResult As String
                                    If myWeb.moRequest("SendId") = subxml.SelectSingleNode("nSubKey").InnerText Then
                                        force = True
                                    End If
                                    If oReminder.GetAttribute("invalidPaymentOnly") Then
                                        ingoreIfPaymentActive = True
                                    End If
                                    actionResult = RenewalAction(CLng(subxml.SelectSingleNode("nSubKey").InnerText), oReminder.GetAttribute("action"), ProcessedCount, oReminder.GetAttribute("name"), force, ingoreIfPaymentActive)
                                    subxml.SetAttribute("actionResult", actionResult)
                                Next

                            Case "expire", "expired"
                                ListExpiredSubscriptions(oReminder, 0, oReminder.GetAttribute("period"), oReminder.GetAttribute("count"))
                                Dim subxml As XmlElement
                                For Each subxml In oReminder.SelectNodes("Subscribers")
                                    Dim force As Boolean = False
                                    If bProcess Then
                                        force = True
                                    End If
                                    Dim ingoreIfPaymentActive As Boolean = False
                                    Dim actionResult As String
                                    If myWeb.moRequest("SendId") = subxml.SelectSingleNode("nSubKey").InnerText Then
                                        force = True
                                    End If
                                    If oReminder.GetAttribute("invalidPaymentOnly") Then
                                        ingoreIfPaymentActive = True
                                    End If
                                    actionResult = RenewalAction(CLng(subxml.SelectSingleNode("nSubKey").InnerText), oReminder.GetAttribute("action"), ProcessedCount, oReminder.GetAttribute("name"), force, ingoreIfPaymentActive)
                                    subxml.SetAttribute("actionResult", actionResult)
                                Next

                        End Select

                    Next

                Catch ex As Exception
                    returnException(mcModuleName, "GetSubscriptionDetail", ex, "", "", gbDebug)
                End Try
            End Sub

            Public Function RenewalAction(ByRef SubId As Long, ByVal Action As String, ByRef ProcessedCount As Long, ByVal messageType As String, ByVal force As Boolean, ByVal ingoreIfPaymentActive As Boolean) As String
                Dim actionResult As String = ""
                ProcessedCount = ProcessedCount + 1

                Try
                    Dim SubXml As XmlElement = GetSubscriptionDetail(Nothing, SubId)
                    SubXml.SetAttribute("messageType", messageType)
                    SubXml.SetAttribute("action", Action)
                    Dim UserEmail As String = SubXml.SelectSingleNode("Subscription/User/Email").InnerText
                    Dim UserId As String = SubXml.SelectSingleNode("Subscription/User/@id").InnerText
                    Dim oMessager As New Protean.Messaging

                    Dim PaymentActive As Boolean = False
                    If SubXml.SelectSingleNode("Subscription/@paymentStatus").InnerText = "active" Then
                        PaymentActive = True
                    End If

                    Select Case Action
                        Case "renewalreminder"

                            Dim sSql As String = "Select dDateTime from tblActivityLog where nUserDirId = " & UserId & " and nOtherId = " & SubId & " and cActivityDetail like '" & SqlFmt(messageType) & "'"
                            Dim actionDate As DateTime = myWeb.moDbHelper.GetDataValue(sSql)

                            If actionDate = "#1/1/0001 12:00:00 AM#" Or (force And gbDebug) Then

                                If PaymentActive And ingoreIfPaymentActive Then
                                    actionResult = "not required"
                                Else
                                    If force Then
                                        Dim cRetMessage As String = oMessager.emailer(SubXml, oSubConfig("ReminderXSL"), oSubConfig("SubscriptionEmailName"), oSubConfig("SubscriptionEmail"), UserEmail, "")
                                        myWeb.moDbHelper.logActivity(dbHelper.ActivityType.SubscriptionAlert, UserId, 0, 0, SubId, messageType, False)
                                        actionResult = "sent"
                                    Else
                                        actionResult = "not sent"
                                    End If
                                End If
                            Else
                                actionResult = actionDate
                            End If

                        Case "renew"
                            If force Then
                                Select Case RenewSubscription(SubXml, True)
                                    Case "Success"
                                        actionResult = "Renewed"
                                    Case "Failed"
                                        actionResult = "Renewal Failed"
                                        SubXml.SetAttribute("actionResult", actionResult)
                                        Dim cRetMessage As String = oMessager.emailer(SubXml, oSubConfig("ReminderXSL"), oSubConfig("SubscriptionEmailName"), oSubConfig("SubscriptionEmail"), UserEmail, "")
                                End Select
                            End If
                        Case "expire"
                            actionResult = ExpireSubscription(SubId, "Scheduled Expiration")
                        Case "expired"

                    End Select

                    Return actionResult

                Catch ex As Exception
                    returnException(mcModuleName, "RenewalAction", ex, "", "", gbDebug)
                    Return ex.Message
                End Try


            End Function


#End Region

#Region "Cart"

            Public Sub UpdateSubscriptionsTotals(ByRef oCartXml As XmlElement)
                'add repeating payment instructions to order.
                Dim oelmt As XmlElement

                Dim repeatPrice As Double
                Dim repeatInterval As String
                Dim repeatFrequency As Integer = 1
                Dim interval As String
                Dim length As Integer
                Dim minimumTerm As Integer
                Dim renewalTerm As Integer
                Dim delayStart As String
                Dim vatAmt As Double

                Dim mbRoundup As Boolean = False
                Try

                    Dim VatRate As Double = oCartXml.GetAttribute("vatRate")


                    For Each oelmt In oCartXml.SelectNodes("descendant-or-self::Item[productDetail/SubscriptionPrices]")

                        repeatPrice = repeatPrice + CDbl("0" & oelmt.SelectSingleNode("productDetail/SubscriptionPrices/Price[@type='sale']").InnerText)
                        repeatInterval = oelmt.SelectSingleNode("productDetail/PaymentUnit").InnerText
                        repeatFrequency = 1
                        If Not oelmt.SelectSingleNode("productDetail/PaymentFrequency") Is Nothing Then
                            If IsNumeric(oelmt.SelectSingleNode("productDetail/PaymentFrequency").InnerText) Then
                                repeatFrequency = oelmt.SelectSingleNode("productDetail/PaymentFrequency").InnerText
                            End If
                        End If
                        interval = oelmt.SelectSingleNode("productDetail/Duration/Unit").InnerText
                        length = CInt("0" & oelmt.SelectSingleNode("productDetail/Duration/Length").InnerText)
                        minimumTerm = oelmt.SelectSingleNode("productDetail/Duration/MinimumTerm").InnerText
                        renewalTerm = oelmt.SelectSingleNode("productDetail/Duration/RenewalTerm").InnerText
                        Dim SubPrices As XmlElement = oelmt.SelectSingleNode("productDetail/SubscriptionPrices")
                        delayStart = SubPrices.GetAttribute("delayStart")
                        vatAmt = Round(repeatPrice * (VatRate / 100), , , mbRoundup)
                    Next
                    repeatPrice = FormatNumber(repeatPrice + vatAmt, Microsoft.VisualBasic.TriState.True, Microsoft.VisualBasic.TriState.False, Microsoft.VisualBasic.TriState.False)
                    oCartXml.SetAttribute("repeatPrice", repeatPrice)
                    oCartXml.SetAttribute("repeatVAT", FormatNumber(vatAmt, 2, Microsoft.VisualBasic.TriState.True, Microsoft.VisualBasic.TriState.False, Microsoft.VisualBasic.TriState.False))
                    oCartXml.SetAttribute("repeatInterval", repeatInterval)
                    oCartXml.SetAttribute("repeatFrequency", repeatFrequency)
                    oCartXml.SetAttribute("interval", interval)
                    oCartXml.SetAttribute("repeatLength", length)
                    oCartXml.SetAttribute("repeatMinimumTerm", minimumTerm)
                    oCartXml.SetAttribute("repeatRenewalTerm", renewalTerm)
                    oCartXml.SetAttribute("delayStart", delayStart)
                    oCartXml.SetAttribute("startDate", xmlDate(Now()))

                    'oCartXml.SetAttribute("payableAmount", oCartXml.GetAttribute("total") - SubscriptionPrice(repeatPrice, repeatInterval, length, interval, xmlDate(Now())))
                    'Payable amount should be the setup cost TS commented out the above line 01/11/2017

                    oCartXml.SetAttribute("payableAmount", oCartXml.GetAttribute("total"))

                    oCartXml.SetAttribute("payableType", "Initial Payment")

                Catch ex As Exception
                    returnException(mcModuleName, "UpdateSubsTotals", ex, "", "", gbDebug)
                End Try
            End Sub

            Public Function CheckCartForSubscriptions(ByVal nCartID As Integer, ByVal nSubUserId As Integer) As Boolean
                Try


                    'this will:
                    '1) Make sure there is only 1 subscription per subscritpion group (will remove the least valuable)
                    '2) Change all subscription quantities to 1 (you dont want more)
                    '3) Return true if there are subscription and user is logged in, OR, no subscriptions. Returns false if there are subscriptions but not logged in
                    Dim cSQL As String = "SELECT tblCartItem.nCartItemKey, tblContent.nContentKey, tblContent.cContentXmlDetail, tblCartCatProductRelations.nCatId, tblCartCatProductRelations.nDisplayOrder" &
                    " FROM tblCartItem INNER JOIN" &
                    " tblContent ON tblCartItem.nItemId = tblContent.nContentKey LEFT OUTER JOIN" &
                    " tblCartCatProductRelations ON tblContent.nContentKey = tblCartCatProductRelations.nContentId" &
                    " WHERE (tblCartItem.nCartOrderId = " & nCartID & ") AND (tblContent.cContentSchemaName = N'Subscription')" &
                    " ORDER BY tblCartItem.nCartItemKey"
                    '" ORDER BY tblCartCatProductRelations.nDisplayOrder"
                    Dim oDS As DataSet = myWeb.moDbHelper.GetDataSet(cSQL, "Subs")
                    If oDS.Tables("Subs").Rows.Count > 0 Then
                        'create a copy table for the end results
                        Dim oDT As DataTable = oDS.Tables("Subs").Copy
                        oDT.TableName = "ActualSubs"
                        oDS.Tables.Add(oDT)

                        Dim oDR As DataRow
                        Dim oDR2 As DataRow
                        'first lets see if it is the only one in that group
                        For Each oDR In oDS.Tables("Subs").Rows
                            If IsNumeric(oDR("nCatId")) Then
                                'It has a category so we go through the actual table and remove others of a lower value
RedoCheck:
                                For Each oDR2 In oDS.Tables("ActualSubs").Rows
                                    If Not oDR2.RowState = DataRowState.Deleted Then

                                        If oDR2("nCatId") = oDR("nCatId") And
                                        Not oDR2("nContentKey") = oDR("nContentKey") And
                                        oDR2("nCartItemKey") < oDR("nCartItemKey") Then
                                            'oDR2("nDisplayOrder") < oDR("nDisplayOrder") Then

                                            myCart.RemoveItem(oDR2("nCartItemKey"))
                                            oDR2.Delete()
                                            GoTo RedoCheck
                                        End If
                                    End If
                                Next
                            End If
                        Next
                        'now we go through and make sure all quantities are 0
                        For Each oDR2 In oDS.Tables("ActualSubs").Rows
                            If Not oDR2.RowState = DataRowState.Deleted Then
                                cSQL = "UPDATE tblCartItem SET nQuantity = 1 WHERE nCartItemKey = " & oDR2("nCartItemKey")
                                myWeb.moDbHelper.ExeProcessSql(cSQL)
                            End If
                        Next
                        'now check the user is logged in
                        If nSubUserId > 0 Then
                            Return True
                        Else
                            Return False
                        End If
                    Else
                        'no subscriptions
                        Return False
                    End If
                Catch ex As Exception
                    returnException(mcModuleName, "CheckCartForSubscriptions", ex, "", "", gbDebug)
                End Try
            End Function

            Public Function CartSubscriptionPrice(ByVal nSubscriptionID As Integer, ByVal nSubUserId As Integer) As Double
                Try
                    Dim nTotalPrice As Double = 0

                    'first we need to fin out if its:
                    '1) New (No existing ones in the same group or none at all)
                    '2) Renewal (so it tacks onto the end)
                    '3) Upgrade/Downgrade (so it takes the remaining credit and starts it from today

                    'First we'll get the xml for this subscription
                    Dim cSQL As String = "SELECT tblContent.*, tblCartCatProductRelations.nCatId FROM tblContent Left Outer JOIN tblCartCatProductRelations ON tblContent.nContentKey = tblCartCatProductRelations.nContentId where tblContent.nContentKey = " & nSubscriptionID
                    Dim oDS As DataSet = myWeb.moDbHelper.GetDataSet(cSQL, "Content")
                    Dim oDC As DataColumn
                    For Each oDC In oDS.Tables("Content").Columns
                        oDC.ColumnMapping = MappingType.Attribute
                    Next
                    oDS.Tables("Content").Columns("cContentXmlBrief").ColumnMapping = MappingType.Hidden
                    oDS.Tables("Content").Columns("cContentXmlDetail").ColumnMapping = MappingType.SimpleContent
                    Dim oXML As New XmlDocument

                    oXML.InnerXml = Replace(Replace(oDS.GetXml, "&gt;", ">"), "&lt;", "<")
                    Dim oCurSubElmt As XmlElement = oXML.DocumentElement.FirstChild
                    Dim cGroup As String = ""
                    If Not oCurSubElmt Is Nothing Then
                        cGroup = oCurSubElmt.GetAttribute("nCatId")
                    End If

                    'Ok, lets get any other  subscriptions in the same  group
                    cSQL = "Select tblSubscription.*, tblAudit.*, tblCartCatProductRelations.nCatId" &
                    " FROM tblSubscription INNER JOIN tblAudit ON tblSubscription.nAuditId = tblAudit.nAuditKey LEFT OUTER JOIN" &
                    " tblCartCatProductRelations ON tblSubscription.nSubContentId = tblCartCatProductRelations.nContentId"
                    If cGroup = "" Then
                        cSQL &= " WHERE tblSubscription.nDirId = " & nSubUserId & " AND tblSubscription.nSubContentId = " & nSubscriptionID
                    Else
                        cSQL &= " WHERE (tblSubscription.nDirId = " & nSubUserId & ") AND (tblSubscription.nSubContentId = " & nSubscriptionID &
                        " OR tblCartCatProductRelations.nCatId = " & cGroup & ")"
                    End If
                    cSQL &= " AND tblSubscription.cRenewalStatus <> 'Cancelled' AND tblAudit.dExpireDate > " & sqlDate(Now) & " AND tblAudit.dPublishDate < " & sqlDate(Now) & " order by tblAudit.dExpireDate DESC"

                    oDS = myWeb.moDbHelper.GetDataSet(cSQL, "Content")
                    If oDS.Tables("Content").Rows.Count = 0 Then
                        'No Other subscriptions or subscriptions in the same group
                        Dim nPrice As Double = myCart.getProductPricesByXml(oCurSubElmt.InnerXml, "", 1)
                        Dim nRptPrice As Double = myCart.getProductPricesByXml(oCurSubElmt.InnerXml, "", 1, "SubscriptionPrices")
                        Dim nPaymentFrequency As Integer = 1
                        If Not oCurSubElmt.SelectSingleNode("Content/PaymentFrequency") Is Nothing Then
                            If IsNumeric(oCurSubElmt.SelectSingleNode("Content/PaymentFrequency").InnerText) Then
                                nPaymentFrequency = oCurSubElmt.SelectSingleNode("Content/PaymentFrequency").InnerText
                            End If
                        End If
                        If oCurSubElmt.SelectSingleNode("Content/SubscriptionPrices/@delayStart").Value = "true" Then
                            nTotalPrice = nPrice
                        Else
                            nTotalPrice = nPrice + SubscriptionPrice(nRptPrice, oCurSubElmt.SelectSingleNode("Content/PaymentUnit").InnerText, nPaymentFrequency, oCurSubElmt.SelectSingleNode("Content/Duration/Unit").InnerText, Now)
                        End If


                    Else
                        'okies, there is some stuff in here.
                        'now there should only be one.
                        'Either the same or one in the same group
                        Dim oExXML As New XmlDocument
                        For Each oDC In oDS.Tables("Content").Columns
                            oDC.ColumnMapping = MappingType.Attribute
                        Next
                        'oDS.Tables("Content").Columns("cContentXmlBrief").ColumnMapping = MappingType.Hidden
                        'oDS.Tables("Content").Columns("cContentXmlDetail").ColumnMapping = MappingType.SimpleContent
                        oDS.Tables("Content").Columns("cSubXML").ColumnMapping = MappingType.SimpleContent
                        oExXML.InnerXml = Replace(Replace(oDS.GetXml, "&gt;", ">"), "&lt;", "<")
                        Dim oNewElmt As XmlElement = oXML.DocumentElement.FirstChild
                        Dim oOldElmt As XmlElement = oExXML.DocumentElement.FirstChild
                        Dim nCurPrice As Double = myCart.getProductPricesByXml(oNewElmt.InnerXml, "", 1)
                        nTotalPrice = SubscriptionPrice(nCurPrice, oNewElmt.SelectSingleNode("Content/PaymentUnit").InnerText, oNewElmt.SelectSingleNode("Content/Duration/Length").InnerText, oNewElmt.SelectSingleNode("Content/Duration/Unit").InnerText, CDate(oOldElmt.GetAttribute("dExpireDate")))
                        If Not oDS.Tables("Content").Rows(0)("nSubContentId") = nSubscriptionID Then
                            nTotalPrice -= UpgradeCredit(myCart.getProductPricesByXml(oOldElmt.InnerXml, "", 1), oOldElmt.SelectSingleNode("Content/PaymentUnit").InnerText, Now, CDate(oOldElmt.GetAttribute("dExpireDate")))
                        End If
                    End If
                    Return nTotalPrice
                Catch ex As Exception
                    returnException(mcModuleName, "CartSubscriptionPrice", ex, "", "", gbDebug)
                End Try
            End Function

            Public Sub UpdateSubscriptionPrice(ByVal oSubscriptionXml As XmlElement, ByVal nSubUserId As Integer)
                Try
                    Dim nTotalPrice As Double = 0
                    Dim nSubscriptionId As Integer = oSubscriptionXml.GetAttribute("id")
                    'first we need to find out if its:
                    '1) New (No existing ones in the same group or none at all)
                    '2) Renewal (so it tacks onto the end)
                    '3) Upgrade/Downgrade (so it takes the remaining credit and starts it from today

                    'First we'll get the xml for this subscription
                    Dim cSQL As String = "SELECT tblContent.*, tblCartCatProductRelations.nCatId FROM tblContent Left Outer JOIN tblCartCatProductRelations ON tblContent.nContentKey = tblCartCatProductRelations.nContentId where tblContent.nContentKey = " & nSubscriptionId
                    Dim oDS As DataSet = myWeb.moDbHelper.GetDataSet(cSQL, "Content")
                    Dim oDC As DataColumn
                    For Each oDC In oDS.Tables("Content").Columns
                        oDC.ColumnMapping = MappingType.Attribute
                    Next
                    oDS.Tables("Content").Columns("cContentXmlBrief").ColumnMapping = MappingType.Hidden
                    oDS.Tables("Content").Columns("cContentXmlDetail").ColumnMapping = MappingType.SimpleContent
                    Dim oXML As New XmlDocument

                    oXML.InnerXml = Replace(Replace(oDS.GetXml, "&gt;", ">"), "&lt;", "<")
                    Dim oCurSubElmt As XmlElement = oXML.DocumentElement.FirstChild
                    Dim cGroup As String = ""
                    If Not oCurSubElmt Is Nothing Then
                        cGroup = oCurSubElmt.GetAttribute("nCatId")
                    End If

                    'Ok, lets get any other  subscriptions in the same  group
                    cSQL = "Select tblSubscription.*, tblAudit.*, tblCartCatProductRelations.nCatId" &
                    " FROM tblSubscription INNER JOIN tblAudit ON tblSubscription.nAuditId = tblAudit.nAuditKey LEFT OUTER JOIN" &
                    " tblCartCatProductRelations ON tblSubscription.nSubContentId = tblCartCatProductRelations.nContentId"
                    If cGroup = "" Then
                        cSQL &= " WHERE tblSubscription.nDirId = " & nSubUserId & " AND tblSubscription.nSubContentId = " & nSubscriptionId
                    Else
                        cSQL &= " WHERE (tblSubscription.nDirId = " & nSubUserId & ") AND (tblSubscription.nSubContentId = " & nSubscriptionId &
                        " OR tblCartCatProductRelations.nCatId = " & cGroup & ")"
                    End If
                    cSQL &= " AND tblSubscription.cRenewalStatus <> 'Cancelled' AND tblAudit.dExpireDate > " & sqlDate(Now) & " AND tblAudit.dPublishDate < " & sqlDate(Now) & " order by tblAudit.dPublishDate DESC"

                    oDS = myWeb.moDbHelper.GetDataSet(cSQL, "Content")

                    If oDS.Tables("Content").Rows.Count = 0 Then

                        'No Other subscriptions or subscriptions in the same group

                    Else
                        'okies, there is some stuff in here.
                        'now there should only be one.
                        'Either the same or one in the same group
                        'latest valid subscription comes first and this is the one we upgrade.
                        Dim oExXML As New XmlDocument
                        For Each oDC In oDS.Tables("Content").Columns
                            oDC.ColumnMapping = MappingType.Attribute
                        Next
                        oDS.Tables("Content").Columns("cSubXML").ColumnMapping = MappingType.SimpleContent
                        oExXML.InnerXml = Replace(Replace(oDS.GetXml, "&gt;", ">"), "&lt;", "<")
                        Dim oNewElmt As XmlElement = oXML.DocumentElement.FirstChild
                        Dim oOldElmt As XmlElement = oExXML.DocumentElement.FirstChild
                        Dim nCurPrice As Double = myCart.getProductPricesByXml(oNewElmt.InnerXml, "", 1)
                        nTotalPrice = SubscriptionPrice(nCurPrice, oNewElmt.SelectSingleNode("Content/PaymentUnit").InnerText, oNewElmt.SelectSingleNode("Content/Duration/Length").InnerText, oNewElmt.SelectSingleNode("Content/Duration/Unit").InnerText, CDate(oOldElmt.GetAttribute("dExpireDate")))
                        If Not oDS.Tables("Content").Rows(0)("nSubContentId") = nSubscriptionId Then
                            nTotalPrice -= UpgradeCredit(myCart.getProductPricesByXml(oOldElmt.InnerXml, "", 1), oOldElmt.SelectSingleNode("Content/PaymentUnit").InnerText, Now, CDate(oOldElmt.GetAttribute("dExpireDate")))
                            Dim PricesNode As XmlElement = oSubscriptionXml.SelectSingleNode("Prices/Price[@type='sale']")
                            PricesNode.SetAttribute("originalPrice", PricesNode.InnerText)
                            PricesNode.SetAttribute("discountValue", CDbl(PricesNode.InnerText) - nTotalPrice)
                            PricesNode.SetAttribute("upgradeFrom", oOldElmt.SelectSingleNode("Content/Name").InnerText)
                            PricesNode.InnerText = nTotalPrice
                        End If
                    End If

                Catch ex As Exception
                    returnException(mcModuleName, "CartSubscriptionPrice", ex, "", "", gbDebug)
                End Try
            End Sub

            Public Function SubscriptionPrice(ByVal nPrice As Double, ByVal cPriceUnit As String, ByVal nDuration As Integer, ByVal cDurationUnit As String, ByVal dStart As Date) As Double
                'Gets the price of the subscription
                Try
                    Dim dFinish As Date
                    Select Case cDurationUnit
                        Case "Day"
                            dFinish = dStart.AddDays(nDuration)
                        Case "Week"
                            dFinish = dStart.AddDays(nDuration * 7)
                        Case "Month"
                            dFinish = dStart.AddMonths(nDuration)
                        Case "Year"
                            dFinish = dStart.AddYears(nDuration)
                        Case Else
                            dFinish = dStart
                    End Select

                    Dim nEndPrice As Double = 0
                    Select Case cPriceUnit
                        Case "Day"
                            nEndPrice = CInt(dFinish.Subtract(dStart).TotalDays) * nPrice
                        Case "Week"
                            nEndPrice = RoundUp(dFinish.Subtract(dStart).TotalDays / 7, 0, 0) * nPrice
                        Case "Month"
                            'this will be trickier
                            'need to step through each month
                            Dim dCurrent As Date = dStart
                            Dim nMonths As Integer = 0
                            dFinish = dFinish.AddDays(-1)
                            Do Until dCurrent >= dFinish
                                dCurrent = dCurrent.AddMonths(1)
                                nMonths += 1
                            Loop
                            nEndPrice = nMonths * nPrice
                        Case "Year"
                            'same as months
                            Dim dCurrent As Date = dStart
                            Dim nYears As Integer = 0
                            Do Until dCurrent >= dFinish
                                dCurrent = dCurrent.AddYears(1)
                                nYears += 1
                            Loop
                            nEndPrice = nYears * nPrice
                        Case Else
                            nEndPrice = 0
                    End Select
                    Return nEndPrice
                Catch ex As Exception
                    returnException(mcModuleName, "SubscriptionPrice", ex, "", "", gbDebug)
                    Return 0
                End Try
            End Function

            Private Function UpgradeCredit(ByVal nPrice As Double, ByVal cPriceUnit As String, ByVal dStart As Date, ByVal dFinish As Date) As Integer
                'gets the remaining credit of the old subscription
                'so that it may be subtracted from the new one
                Try
                    Dim nUnitsCredit As Double = 0
                    Select Case cPriceUnit
                        Case "Day"
                            nUnitsCredit = dFinish.Subtract(dStart).TotalDays
                        Case "Week"
                            nUnitsCredit = dFinish.Subtract(dStart).TotalDays / 7
                        Case "Month"
                            Dim dCurrent As Date = dStart
                            Dim nMonths As Integer = 0
                            Do Until dCurrent >= dFinish
                                dCurrent = dCurrent.AddMonths(1)
                                nMonths += 1
                            Loop
                            nUnitsCredit = nMonths
                        Case "Year"
                            'same as months
                            Dim dCurrent As Date = dStart
                            Dim divBy As String = "Month"

                            Select Case divBy
                                Case "Year"
                                    Dim nYears As Integer = 0
                                    'By Min Full Year
                                    Do Until dCurrent >= dFinish
                                        dCurrent = dCurrent.AddYears(1)
                                        nYears += 1
                                    Loop
                                    nUnitsCredit = nYears
                                Case "Month"
                                    Dim nMonths As Integer = 0
                                    'By Min Full Month
                                    Do Until dCurrent >= dFinish
                                        dCurrent = dCurrent.AddMonths(1)
                                        nMonths += 1
                                    Loop
                                    nUnitsCredit = nMonths / 12
                                Case "Day"
                                    Dim nDays As Integer = 0
                                    'By Min Full Month
                                    Do Until dCurrent >= dFinish
                                        dCurrent = dCurrent.AddDays(1)
                                        nDays += 1
                                    Loop
                                    nUnitsCredit = nDays / 365
                            End Select

                    End Select
                    nPrice = FormatNumber(nPrice * nUnitsCredit, 2)
                    Return nPrice

                Catch ex As Exception
                    returnException(mcModuleName, "UpgradeCredit", ex, "", "", gbDebug)
                End Try
            End Function

            Public Function SubscriptionEndDate(ByVal dStart As Date, ByVal oSubDetailElmt As XmlElement) As Date
                Try
                    Dim cDuration As Integer = 0
                    Dim cDurationUnit As String
                    If IsNumeric(oSubDetailElmt.SelectSingleNode("Duration/Length").InnerText) Then
                        cDuration = oSubDetailElmt.SelectSingleNode("Duration/Length").InnerText
                    End If
                    If cDuration = 0 Then
                        Return Nothing
                    Else
                        cDurationUnit = oSubDetailElmt.SelectSingleNode("Duration/Unit").InnerText
                        Select Case cDurationUnit
                            Case "Day"
                                Return dStart.AddDays(cDuration).AddDays(-1)

                            Case "Week"
                                Return dStart.AddDays(cDuration * 7).AddDays(-1)
                            Case "Month"
                                Return dStart.AddMonths(cDuration).AddDays(-1)
                            Case "Year"
                                Return dStart.AddYears(cDuration).AddDays(-1)
                        End Select
                    End If

                Catch ex As Exception
                    returnException(mcModuleName, "SubscriptionEndDate", ex, "", "", gbDebug)
                End Try
            End Function

            Public Function GetRenewalDate(ByVal nId As Integer) As Date
                Try
                    Dim cSQL As String = "SELECT tblAudit.dExpireDate FROM tblSubscription INNER JOIN tblAudit ON tblSubscription.nAuditId = tblAudit.nAuditKey where nSubKey = " & nId
                    Dim sRenew As String = myWeb.moDbHelper.ExeProcessSqlScalar(cSQL)
                    If IsDate(sRenew) Then
                        Dim dRenew As Date = sRenew
                        If dRenew < Now.Date Then dRenew = Now.AddDays(-1).Date
                        Return dRenew
                    Else

                    End If

                Catch ex As Exception
                    returnException(mcModuleName, "GetRenewalDate", ex, "", "", gbDebug)
                End Try
            End Function

            Public Overridable Sub AddUserSubscriptions(ByVal nCartId As Integer, ByVal nSubUserId As Integer, Optional ByVal nPaymentMethodId As Integer = 0)

                Dim cLastSubXml As String = ""
                Dim oSubConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/subscriptions")
                Dim SubscrptionSchemaTypes As String = "Subscription"
                Dim type As String
                Dim SubscrptionSchemaTypesTemp As String = ""
                Try
                    'check for subscriptions being turned on with no subscription config section set.
                    If Not oSubConfig Is Nothing Then

                        If oSubConfig("SubscrptionSchemaTypes") <> "" Then
                            SubscrptionSchemaTypes = oSubConfig("SubscrptionSchemaTypes")
                        End If

                        For Each type In Split(SubscrptionSchemaTypes, ",")
                            SubscrptionSchemaTypesTemp &= "'" & Trim(type) & "',"
                        Next

                        SubscrptionSchemaTypes = SubscrptionSchemaTypesTemp.TrimEnd(",")

                        Dim cSQL As String = "SELECT tblContent.nContentKey, tblContent.cContentXmlBrief, tblCartItem.xItemXml" &
                        " FROM tblContent INNER JOIN tblCartItem ON tblContent.nContentKey = tblCartItem.nItemId" &
                        " WHERE (tblCartItem.nCartOrderId = " & nCartId & ") AND (tblContent.cContentSchemaName IN (" & SubscrptionSchemaTypes & "))"

                        Dim oDS As DataSet = myWeb.moDbHelper.GetDataSet(cSQL, "Subs")
                        If oDS.Tables("Subs").Rows.Count > 0 Then
                            Dim oDR As DataRow
                            For Each oDR In oDS.Tables("Subs").Rows
                                Dim xItemDoc As New XmlDocument
                                If myWeb.moDbHelper.checkTableColumnExists("tblCartItem", "xItemXml") Then
                                    xItemDoc.LoadXml(oDR("xItemXml"))
                                Else
                                    xItemDoc.LoadXml(oDR("cContentXmlBrief"))
                                End If

                                'Add OrderNotes to Subscription XML
                                cSQL = "Select cClientNotes from tblCartOrder where nCartOrderKey=" & nCartId
                                Dim oNotes As XmlElement = xItemDoc.CreateElement("Notes")
                                Dim notes As String = CStr("" & myWeb.moDbHelper.ExeProcessSqlScalar(cSQL))
                                oNotes.InnerXml = notes
                                xItemDoc.FirstChild.AppendChild(oNotes)

                                AddUserSubscription(oDR("nContentKey"), nSubUserId, nPaymentMethodId, xItemDoc.DocumentElement, nCartId)

                                cLastSubXml = xItemDoc.OuterXml 'oDR("cContentXmlBrief")
                            Next
                        End If





                        ' Clear the cache
                        ' -- we need to also reload the menu and check if the page we came from is still accessible.
                        myWeb.moDbHelper.clearStructureCacheUser()

                        ' Reload the menu
                        Dim oMenu As XmlElement = myWeb.GetStructureXML(0, 0, 0, "Site", False, False, True, False, False, "MenuItem", "Menu")
                        Dim oCurrentMenu As XmlElement = myWeb.moPageXml.SelectSingleNode("/Page/Menu")
                        oCurrentMenu.ParentNode.ReplaceChild(oCurrentMenu.OwnerDocument.ImportNode(oMenu, True), oCurrentMenu)

                        ' Check if the current page is still valid.
                        If oMenu.SelectSingleNode("//MenuItem[@id=" & myWeb.mnPageId & "]") Is Nothing Then

                            Dim nRootId As Integer = CInt("0" & CStr(IIf(myWeb.moConfig("AuthenticatedRootPageId") Is Nothing, IIf(myWeb.moConfig("RootPageId") Is Nothing, 1, myWeb.moConfig("RootPageId")), myWeb.moConfig("AuthenticatedRootPageId"))))

                            ' Load in the last subscription
                            If String.IsNullOrEmpty(cLastSubXml) Then

                                ' No sub set the page to be the root id
                                myWeb.mnPageId = nRootId

                            Else

                                Dim oSub As XmlElement = myWeb.moPageXml.CreateElement("Subscription")
                                Try
                                    oSub.InnerXml = cLastSubXml
                                    If oSub.SelectSingleNode("//AccessPage[.!='']") Is Nothing Then
                                        myWeb.mnPageId = nRootId
                                    Else
                                        myWeb.mnPageId = oSub.SelectSingleNode("//AccessPage[.!='']").InnerText
                                    End If
                                Catch ex As Exception
                                    ' No xml sub set the page to be the root id
                                    myWeb.mnPageId = nRootId
                                End Try

                            End If

                            ' Set the page id.
                            myWeb.moPageXml.DocumentElement.SetAttribute("id", myWeb.mnPageId)
                            If myWeb.moConfig("UsePageIdsForURLs") = "on" Then
                                myWeb.moSession("returnPage") = "?pgid=" & myWeb.mnPageId
                            Else
                                Dim oMenuItem As XmlElement = oMenu.SelectSingleNode("//MenuItem[@id=" & myWeb.mnPageId & "]")
                                Dim cMenuUrl As String
                                If oMenuItem Is Nothing Then
                                    cMenuUrl = "/"
                                Else
                                    cMenuUrl = oMenuItem.GetAttribute("url")
                                    If myCart.mcSiteURL.EndsWith("/") Then cMenuUrl = cMenuUrl.TrimStart("/\".ToCharArray)
                                End If
                                myWeb.moSession("returnPage") = cMenuUrl

                            End If

                        End If
                    End If


                Catch ex As Exception
                    returnException(mcModuleName, "AddUserSubscriptions", ex, "", "", gbDebug)
                End Try

            End Sub

            Public Sub AddUserSubscription(ByVal nSubscriptionID As Integer, ByVal nSubUserId As Integer, Optional ByVal nPaymentMethodId As Integer = 0, Optional ByVal cartItemXml As XmlElement = Nothing, Optional ByVal nCartId As Long = Nothing)
                Try
                    'same sort of process as getting prices.
                    'this time we need to replace if regrading
                    'add a new one for renewals to start the next day (for the scheduler to pickup)
                    'add new ones straight in.

                    'First we'll get the xml for this subscription
                    Dim oCurSubElmt As XmlElement
                    Dim cSQL As String
                    Dim oDS As DataSet
                    Dim oDC As DataColumn
                    If Not cartItemXml Is Nothing Then
                        oCurSubElmt = cartItemXml
                    Else
                        cSQL = "SELECT tblContent.*, tblCartCatProductRelations.nCatId FROM tblContent LEFT OUTER JOIN tblCartCatProductRelations ON tblContent.nContentKey = tblCartCatProductRelations.nContentId where tblContent.nContentKey = " & nSubscriptionID
                        oDS = myWeb.moDbHelper.GetDataSet(cSQL, "Content")

                        For Each oDC In oDS.Tables("Content").Columns
                            oDC.ColumnMapping = MappingType.Attribute
                        Next
                        oDS.Tables("Content").Columns("cContentXmlBrief").ColumnMapping = MappingType.Hidden
                        oDS.Tables("Content").Columns("cContentXmlDetail").ColumnMapping = MappingType.SimpleContent
                        Dim oXML As New XmlDocument

                        oXML.InnerXml = Replace(Replace(oDS.GetXml, "&gt;", ">"), "&lt;", "<")
                        oCurSubElmt = oXML.DocumentElement.FirstChild
                    End If

                    Dim cGroup As String = ""
                    If Not oCurSubElmt Is Nothing Then
                        cGroup = oCurSubElmt.GetAttribute("nCatId")
                    End If
                    'Ok, lets get any other  subscriptions in the same  group

                    If cGroup = "" Then
                        cSQL = "SELECT tblSubscription.*, tblContent.* FROM tblContent INNER JOIN tblSubscription ON tblContent.nContentKey = tblSubscription.nSubContentId"
                        cSQL &= " WHERE tblSubscription.nDirId = " & nSubUserId & " AND tblSubscription.nSubContentId = " & nSubscriptionID
                    Else
                        cSQL = "SELECT tblSubscription.*, tblCartCatProductRelations.nCatId, tblContent.*" &
                                           " FROM tblContent INNER JOIN" &
                                           " tblCartCatProductRelations ON tblContent.nContentKey = tblCartCatProductRelations.nContentId RIGHT OUTER JOIN" &
                                           " tblSubscription ON tblCartCatProductRelations.nContentId = tblSubscription.nSubContentId RIGHT OUTER JOIN" &
                                           " tblAudit a ON tblSubscription.nAuditId = a.nAuditKey"
                        cSQL &= " WHERE (tblSubscription.nDirId = " & nSubUserId & ") AND (tblSubscription.nSubContentId = " & nSubscriptionID &
                                                " OR tblCartCatProductRelations.nCatId = " & cGroup & ") order by a.dExpireDate DESC"
                    End If

                    Dim SubStartDate As Date = Now
                    If oSubConfig("SubscriptionStartDateXpath") <> "" Then
                        Dim dateString As String = oCurSubElmt.SelectSingleNode(oSubConfig("SubscriptionStartDateXpath")).InnerText
                        If IsDate(dateString) Then
                            SubStartDate = CDate(oCurSubElmt.SelectSingleNode(oSubConfig("SubscriptionStartDateXpath")).InnerText)
                        End If
                    End If

                    Dim SubEndDate As Date = SubscriptionEndDate(SubStartDate, oCurSubElmt)

                    oDS = myWeb.moDbHelper.GetDataSet(cSQL, "Content")

                    If LCase(oSubConfig("AllowRenewals")) = "off" Then

                        AddSubscription(nSubscriptionID, oCurSubElmt, SubStartDate, SubEndDate, nSubUserId, nPaymentMethodId, nCartId)

                    Else

                        If oDS.Tables("Content").Rows.Count = 0 Then
                            'No Other subscriptions or subscriptions in the same group
                            'so we just add it
                            AddSubscription(nSubscriptionID, oCurSubElmt, SubStartDate, SubEndDate, nSubUserId, nPaymentMethodId, nCartId)
                        Else
                            'okies, there is some stuff in here.
                            'now there should only be one.
                            'Either the same or one in the same group
                            Dim oExXML As New XmlDocument
                            For Each oDC In oDS.Tables("Content").Columns
                                oDC.ColumnMapping = MappingType.Attribute
                            Next
                            oDS.Tables("Content").Columns("cContentXmlBrief").ColumnMapping = MappingType.Hidden
                            oDS.Tables("Content").Columns("cSubXml").ColumnMapping = MappingType.Hidden
                            oDS.Tables("Content").Columns("cContentXmlDetail").ColumnMapping = MappingType.SimpleContent
                            oExXML.InnerXml = Replace(Replace(oDS.GetXml, "&gt;", ">"), "&lt;", "<")
                            Dim oNewElmt As XmlElement = oExXML.DocumentElement.FirstChild
                            If IsDBNull(oDS.Tables("Content").Rows(0)("nContentKey")) Then
                                'its one with no group

                            ElseIf Not oDS.Tables("Content").Rows(0)("nContentKey") = nSubscriptionID Then
                                'its a regrade
                                'cancel earlier subscription
                                Dim oRows As DataRow
                                For Each oRows In oDS.Tables("Content").Rows
                                    CancelSubscription(oRows("nSubKey"))
                                Next
                                AddSubscription(nSubscriptionID, oCurSubElmt, SubStartDate, SubscriptionEndDate(Now, oCurSubElmt), nSubUserId, nPaymentMethodId)
                            Else
                                'its a renewal of an existing policy
                                'add
                                Dim dRenStart As Date = GetRenewalDate(oNewElmt.GetAttribute("nSubKey"))
                                dRenStart = dRenStart.AddDays(1)
                                AddSubscription(nSubscriptionID, oCurSubElmt, dRenStart, SubscriptionEndDate(dRenStart, oCurSubElmt), nSubUserId, nPaymentMethodId)
                            End If
                        End If

                    End If


                Catch ex As Exception
                    returnException(mcModuleName, "AddUserSubscription", ex, "", "", gbDebug)
                End Try

            End Sub

#End Region

#Region "DBHELPER"

            Public Sub AddSubscriptionToUserXML(ByRef oElmt As XmlElement, ByVal nSubUserId As Integer)
                Try
                    Dim cSQL As String = "SELECT s.nSubKey,s.cRenewalStatus, s.nSubContentId, s.dStartDate, s.nPeriod, s.cPeriodUnit, s.nValueNet, s.nPaymentMethodId, pm.cPayMthdProviderName, s.bPaymentMethodActive, a.nStatus, a.dPublishDate, a.dExpireDate, s.cSubXML" &
                    " FROM tblSubscription s INNER JOIN" &
                    " tblAudit a ON s.nAuditId = a.nAuditKey LEFT OUTER JOIN" &
                    " tblCartPaymentMethod pm On s.nPaymentMethodId = pm.nPayMthdKey" &
                    " WHERE s.nDirId = " & nSubUserId

                    Dim oDS As DataSet = myWeb.moDbHelper.GetDataSet(cSQL, "Subscriptions", "Subscription")
                    If oDS.Tables("Subscriptions").Rows.Count > 0 Then
                        oDS.Tables("Subscriptions").Columns("nSubKey").ColumnMapping = MappingType.Attribute
                        oDS.Tables("Subscriptions").Columns("nSubContentId").ColumnMapping = MappingType.Attribute
                        oDS.Tables("Subscriptions").Columns("cRenewalStatus").ColumnMapping = MappingType.Attribute
                        oDS.Tables("Subscriptions").Columns("dStartDate").ColumnMapping = MappingType.Attribute
                        'oDS.Tables("Subscriptions").Columns("dEndDate").ColumnMapping = MappingType.Attribute
                        oDS.Tables("Subscriptions").Columns("nPeriod").ColumnMapping = MappingType.Attribute
                        oDS.Tables("Subscriptions").Columns("cPeriodUnit").ColumnMapping = MappingType.Attribute
                        oDS.Tables("Subscriptions").Columns("nValueNet").ColumnMapping = MappingType.Attribute
                        oDS.Tables("Subscriptions").Columns("nPaymentMethodId").ColumnMapping = MappingType.Attribute
                        oDS.Tables("Subscriptions").Columns("cPayMthdProviderName").ColumnMapping = MappingType.Attribute
                        oDS.Tables("Subscriptions").Columns("bPaymentMethodActive").ColumnMapping = MappingType.Attribute
                        oDS.Tables("Subscriptions").Columns("nStatus").ColumnMapping = MappingType.Attribute
                        oDS.Tables("Subscriptions").Columns("dPublishDate").ColumnMapping = MappingType.Attribute
                        oDS.Tables("Subscriptions").Columns("dExpireDate").ColumnMapping = MappingType.Attribute
                        oDS.Tables("Subscriptions").Columns("cSubXML").ColumnMapping = MappingType.SimpleContent
                        Dim oSubElmt As XmlElement = oElmt.OwnerDocument.CreateElement("Subscriptions")
                        Dim oTmp As New XmlDocument
                        oTmp.InnerXml = Replace(Replace(oDS.GetXml, "&lt;", "<"), "&gt;", ">")
                        oSubElmt.InnerXml = oTmp.DocumentElement.InnerXml
                        oElmt.AppendChild(oSubElmt)
                    End If
                Catch ex As Exception
                    returnException(mcModuleName, "AddSubscriptionToUserXML", ex, "", "", gbDebug)
                End Try
            End Sub

            Public Sub AddSubscription(ByVal nid As Integer, ByVal oSubDetailElmt As XmlElement, ByVal dStart As Date, ByVal dFinish As Date, ByVal nSubUserId As Integer, Optional ByVal nPaymentMethodId As Integer = 0, Optional ByVal nCartId As Long = 0)
                Try
                    Dim oXml As XmlDocument = New XmlDocument
                    Dim oInstance As XmlElement = oXml.CreateElement("instance")
                    Dim oElmt As XmlNode = oXml.CreateElement("tblSubscription")
                    addNewTextNode("nDirId", oElmt, nSubUserId)
                    addNewTextNode("nSubContentId", oElmt, nid)
                    Dim oElmt2 As XmlElement = addNewTextNode("cSubXml", oElmt)
                    oElmt2.InnerXml = oSubDetailElmt.OuterXml
                    addNewTextNode("dStartDate", oElmt, Protean.xmlDate(dStart))


                    addNewTextNode("cSubName", oElmt, oSubDetailElmt.SelectSingleNode("Name").InnerText)
                    addNewTextNode("nPeriod", oElmt, oSubDetailElmt.SelectSingleNode("Duration/Length").InnerText)
                    addNewTextNode("cPeriodUnit", oElmt, oSubDetailElmt.SelectSingleNode("Duration/Unit").InnerText)
                    addNewTextNode("nValueNet", oElmt, CDbl("0" & oSubDetailElmt.SelectSingleNode("SubscriptionPrices/Price[@type='sale']").InnerText))
                    addNewTextNode("nPaymentMethodId", oElmt, nPaymentMethodId)
                    addNewTextNode("bPaymentMethodActive", oElmt, "true")
                    addNewTextNode("nMinimumTerm", oElmt, oSubDetailElmt.SelectSingleNode("Duration/MinimumTerm").InnerText)
                    addNewTextNode("nRenewalTerm", oElmt, oSubDetailElmt.SelectSingleNode("Duration/RenewalTerm").InnerText)
                    addNewTextNode("cRenewalStatus", oElmt, oSubDetailElmt.SelectSingleNode("Type").InnerText)
                    If myWeb.moDbHelper.checkTableColumnExists("tblSubscription", "nOrderId") Then
                        addNewTextNode("nOrderId", oElmt, nCartId)
                    End If

                    'addNewTextNode("dEndDate", oElmt, dFinish)
                    addNewTextNode("nAuditId", oElmt, )
                    ' addNewTextNode("nAuditId", oElmt, myWeb.moDbHelper.getAuditId(1, myWeb.mnUserId, "Subscription", dStart, dFinish, Now))
                    addNewTextNode("nAuditKey", oElmt, )
                    addNewTextNode("dPublishDate", oElmt, dStart)
                    addNewTextNode("dExpireDate", oElmt, dFinish)
                    addNewTextNode("dInsertDate", oElmt, )
                    addNewTextNode("nInsertDirId", oElmt, )
                    addNewTextNode("dUpdateDate", oElmt, )
                    addNewTextNode("nUpdateDirId", oElmt, )
                    addNewTextNode("nStatus", oElmt, 1)
                    addNewTextNode("cDescription", oElmt, )

                    oInstance.AppendChild(oElmt)
                    Dim nSubId As Integer = myWeb.moDbHelper.setObjectInstance(dbHelper.objectTypes.Subscription, oInstance)
                    If nSubId > 0 Then
                        For Each oElmt In oSubDetailElmt.SelectNodes("UserGroups/Group[@id!='']")
                            Dim nGrpID As Integer = oElmt.Attributes("id").Value
                            myWeb.moDbHelper.saveDirectoryRelations(nSubUserId, nGrpID)
                        Next
                    End If

                Catch ex As Exception
                    returnException(mcModuleName, "AddSubscription", ex, "", "", gbDebug)
                End Try
            End Sub

            Public Sub CancelSubscription(ByVal nId As Integer, Optional cReason As String = "")
                Try

                    Dim SubInstance As New XmlDocument()
                    SubInstance.LoadXml("<instance>" & myWeb.moDbHelper.getObjectInstance(dbHelper.objectTypes.Subscription, nId) & "</instance>")

                    Dim editElmt As XmlElement = SubInstance.DocumentElement.FirstChild

                    editElmt.SelectSingleNode("nStatus").InnerText = dbHelper.Status.Rejected
                    editElmt.SelectSingleNode("cRenewalStatus").InnerText = "Cancelled"

                    editElmt.SelectSingleNode("dUpdateDate").InnerText = xmlDate(Now())
                    editElmt.SelectSingleNode("nUpdateDirId").InnerText = myWeb.mnUserId
                    editElmt.SelectSingleNode("cDescription").InnerText = cReason

                    'Cancel the payment method
                    CancelPaymentMethod(editElmt.SelectSingleNode("nPaymentMethodId").InnerText)

                    'We only remove user from groups (this needs to happen by schduler to remove once expired)

                    myWeb.moDbHelper.setObjectInstance(dbHelper.objectTypes.Subscription, SubInstance.DocumentElement)

                    ExpireSubscriptionGroups(nId)

                    'Email the site owner to inform of cancelation !!!

                Catch ex As Exception
                    returnException(mcModuleName, "CancelSubscription", ex, "", "", gbDebug)
                End Try
            End Sub

            Public Function ExpireSubscription(ByVal nId As Integer, Optional cReason As String = "") As String
                Try

                    Dim SubInstance As New XmlDocument()
                    SubInstance.LoadXml("<instance>" & myWeb.moDbHelper.getObjectInstance(dbHelper.objectTypes.Subscription, nId) & "</instance>")

                    Dim editElmt As XmlElement = SubInstance.DocumentElement.FirstChild

                    editElmt.SelectSingleNode("nStatus").InnerText = dbHelper.Status.Rejected
                    editElmt.SelectSingleNode("cRenewalStatus").InnerText = "Fixed"

                    editElmt.SelectSingleNode("dUpdateDate").InnerText = xmlDate(Now())
                    editElmt.SelectSingleNode("nUpdateDirId").InnerText = myWeb.mnUserId
                    editElmt.SelectSingleNode("cDescription").InnerText = cReason

                    'Cancel the payment method
                    CancelPaymentMethod(editElmt.SelectSingleNode("nPaymentMethodId").InnerText)
                    myWeb.moDbHelper.setObjectInstance(dbHelper.objectTypes.Subscription, SubInstance.DocumentElement)

                    'Remove the user from any user groups
                    ExpireSubscriptionGroups(nId)

                    Return "Subscription Expired" & cReason

                Catch ex As Exception
                    returnException(mcModuleName, "ExpireSubscription", ex, "", "", gbDebug)
                    Return "Expiry Failed"
                End Try
            End Function

            Public Sub CancelPaymentMethod(ByVal nPaymentMethodId As Integer)

                Try
                    Dim PayInstance As New XmlDocument()
                    PayInstance.LoadXml(myWeb.moDbHelper.getObjectInstance(dbHelper.objectTypes.CartPaymentMethod, nPaymentMethodId))
                    Dim PaymentMethod As String = PayInstance.DocumentElement.SelectSingleNode("cPayMthdProviderName").InnerText
                    If PaymentMethod <> "" Then
                        Dim oPayProv As New Providers.Payment.BaseProvider(myWeb, PaymentMethod)
                        Dim paymentStatus As String
                        Try
                            paymentStatus = oPayProv.Activities.CancelPayments(myWeb, PayInstance.DocumentElement.SelectSingleNode("cPayMthdProviderRef").InnerText)
                        Catch ex2 As Exception
                            ' no payment method to cancel. to we email site owner.
                        End Try

                    End If
                Catch ex As Exception
                    returnException(mcModuleName, "CancelSubscription", ex, "", "", gbDebug)
                End Try
            End Sub

#End Region

#Region "Scheduler"

            Public Function SubcriptionReminders() As XmlElement
                'send reminders to out for closing subscriptions 
                Dim oSubConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/subscriptions")
                Dim moReminderCfg As XmlElement = WebConfigurationManager.GetWebApplicationSection("protean/subscriptionReminders")
                Dim moResponse As XmlElement = myWeb.moPageXml.CreateElement("Response")
                Try
                    'WRITE CODE TO CHECK WHEN LAST RUN
                    Dim sSQL As String = "select TOP 1 cActivityDetail from tblActivityLog where nActivityType = 300 and  dDateTime > " & sqlDateTime(DateAdd(DateInterval.Day, -1, Now())) & " order by dDateTime DESC"
                    Dim FeedCheck As String = myWeb.moDbHelper.ExeProcessSqlScalar(sSQL)
                    If FeedCheck <> "" Then
                        myWeb.moDbHelper.logActivity(dbHelper.ActivityType.SubscriptionProcessAttempt, myWeb.mnUserId, 0, 0, "Subscription Process Already Run:" & FeedCheck)
                        moResponse.InnerXml = "<Error>Subscription Process allready run</Error>"
                    Else
                        ' Only run daily
                        Dim LogId As Long = myWeb.moDbHelper.logActivity(dbHelper.ActivityType.SubscriptionProcess, myWeb.mnUserId, 0, 0, "Subscription Process Started")

                        'FIRST LETS CHECK FOR EXPIRING ACTIONS
                        'CheckExpiringSubscriptions()

                        Dim reminderNode As XmlElement
                        Dim oMessager As New Protean.Messaging

                        Dim cSQL As String = ""

                        If moReminderCfg Is Nothing Then
                            moResponse.InnerXml = "<Error>No Subscription Reminder Config Found</Error>"
                        Else
                            For Each reminderNode In moReminderCfg.SelectNodes("reminder")
                                cSQL = "SELECT tblSubscription.*,DATEDIFF(dd,tblAudit.dExpireDate,getdate()) as renewaldays, tblDirectory.cDirXml, tblCartPaymentMethod.*, tblAudit.*" &
                                                   " FROM tblSubscription  INNER JOIN tblAudit ON tblSubscription.nAuditId = tblAudit.nAuditKey " &
                                                   " INNER JOIN tblDirectory On tblSubscription.nDirId = tblDirectory.nDirKey" &
                                                   " INNER Join tblCartPaymentMethod ON tblSubscription.nPaymentMethodId = tblCartPaymentMethod.nPayMthdKey "

                                Dim count As Double = CDbl(reminderNode.GetAttribute("count"))
                                Dim startDate As String = Protean.Tools.Database.SqlDate(Now.AddDays((count * -1)))
                                Dim endDate As String = Protean.Tools.Database.SqlDate(Now.AddDays((count * -1) + 1))
                                Dim action As String = reminderNode.GetAttribute("action")

                                Select Case action
                                    Case "renewal"
                                        cSQL &= "where (tblAudit.dExpireDate >= " & startDate & " AND tblAudit.dExpireDate < " & endDate & " AND cRenewalStatus LIKE 'Rolling')"
                                    Case "expired"
                                        cSQL &= "where (tblAudit.dExpireDate >= " & startDate & " AND tblAudit.dExpireDate < " & endDate & " AND cRenewalStatus LIKE 'Expired')"
                                    Case "expire"
                                        cSQL &= "where (tblAudit.dExpireDate >= " & startDate & " AND tblAudit.dExpireDate < " & endDate & " AND cRenewalStatus LIKE 'Fixed')"
                                    Case "invalidPayment"
                                        cSQL &= "where (tblAudit.dExpireDate >= " & startDate & " AND tblAudit.dExpireDate < " & endDate & " AND cRenewalStatus LIKE 'Rolling' AND bPaymentMethodActive=0)"
                                End Select

                                'thats the sql sorted
                                'now to make it xml
                                Dim oDS As DataSet = myWeb.moDbHelper.GetDataSet(cSQL, "Subscriptions")
                                Dim ProcessedCount As Long = 0
                                If oDS.Tables("Subscriptions").Rows.Count > 0 Then

                                    Dim oDr As DataRow
                                    For Each oDr In oDS.Tables("Subscriptions").Rows
                                        ProcessedCount = ProcessedCount + 1
                                        Dim actionResult As String = ""
                                        Dim SubXml As XmlElement = GetSubscriptionDetail(Nothing, oDr("nSubKey"))
                                        SubXml.SetAttribute("messageType", reminderNode.GetAttribute("name"))
                                        SubXml.SetAttribute("action", action)
                                        Dim UserEmail As String = SubXml.SelectSingleNode("Subscription/User/Email").InnerText

                                        If action = "renewal" And count = 0 Then
                                            actionResult = RenewSubscription(SubXml, 1)
                                            If actionResult = "Failed" Then
                                                SubXml.SetAttribute("actionResult", actionResult)
                                                Dim cRetMessage As String = oMessager.emailer(SubXml, oSubConfig("ReminderXSL"), oSubConfig("SubscriptionEmailName"), oSubConfig("SubscriptionEmail"), UserEmail, reminderNode.GetAttribute("subject"))
                                            End If
                                        ElseIf action = "expire" And count = 0 Then
                                            actionResult = ExpireSubscription(CInt(oDr("nSubKey")), "Scheduled Expiration")
                                        Else
                                            Dim cRetMessage As String = oMessager.emailer(SubXml, oSubConfig("ReminderXSL"), oSubConfig("SubscriptionEmailName"), oSubConfig("SubscriptionEmail"), UserEmail, reminderNode.GetAttribute("subject"))
                                            actionResult = cRetMessage
                                        End If
                                        SubXml.SetAttribute("actionResult", actionResult)
                                        moResponse.AppendChild(SubXml)
                                    Next
                                End If
                                reminderNode.SetAttribute("updated", ProcessedCount)
                            Next
                        End If

                    End If

                    Return moResponse

                Catch ex As Exception
                    returnException(mcModuleName, "SubcriptionReminders", ex, "", "", gbDebug)
                    moReminderCfg.SetAttribute("exception", ex.Message)
                    moReminderCfg.SetAttribute("stackTrace", ex.StackTrace)
                    Return moReminderCfg
                End Try
            End Function

            Public Function RenewSubscription(ByVal SubKey As Long, bEmailClient As Boolean) As String

                'Get the subscription XML
                Dim SubXml As XmlElement = GetSubscriptionDetail(Nothing, SubKey)
                Return RenewSubscription(SubXml.FirstChild, bEmailClient)

            End Function

            Public Function RenewSubscription(ByVal SubXml As XmlElement, bEmailClient As Boolean) As String
                Dim cProcessInfo As String
                Try
                    Dim renewInterval As DateInterval = DateInterval.Day
                    Select Case SubXml.GetAttribute("periodUnit")
                        Case "Week"
                            renewInterval = DateInterval.WeekOfYear
                        Case "Year"
                            renewInterval = DateInterval.Year
                    End Select
                    Dim SubId As Long = SubXml.GetAttribute("id")

                    Dim dNewStart As Date = DateAdd(DateInterval.Day, 1, CDate(SubXml.GetAttribute("expireDate")))
                    Dim dNewEnd As Date = DateAdd(renewInterval, CInt(SubXml.GetAttribute("period")), CDate(SubXml.GetAttribute("expireDate")))

                    Dim Amount As Double = CDbl(SubXml.GetAttribute("value"))
                    Dim OrderId As Long = CLng(0 & SubXml.GetAttribute("orderId"))
                    Dim SubContentId As Long = SubXml.GetAttribute("contentId")
                    Dim UserId As Long = SubXml.GetAttribute("userId")
                    Dim SubName As String = SubXml.GetAttribute("name") & " Renewal"
                    Dim nPaymentMethodId As Long = SubXml.GetAttribute("providerId")

                    'Create the invoice
                    'Add quote to cart
                    myWeb.InitialiseCart()
                    myWeb.moCart.CreateCartElement(myWeb.moPageXml)
                    myWeb.moCart.mnEwUserId = UserId
                    myWeb.moCart.CreateNewCart(myWeb.moCart.moCartXml)
                    myWeb.moCart.SetPaymentMethod(nPaymentMethodId)
                    If Not SubXml.SelectSingleNode("Content/Notes") Is Nothing Then
                        myWeb.moCart.SetClientNotes(SubXml.SelectSingleNode("Content/Notes").InnerXml)
                    End If
                    Dim oSubContent As XmlElement = SubXml.SelectSingleNode("Content")
                    oSubContent.SetAttribute("renewal", "true")
                    oSubContent.SetAttribute("renewalStart", xmlDate(dNewStart))
                    oSubContent.SetAttribute("renewalEnd", xmlDate(dNewEnd))

                    myWeb.moCart.AddItem(SubContentId, 1, Nothing, SubName, Amount, oSubContent.OuterXml)

                    Dim billingId As Long = myWeb.moDbHelper.GetDataValue("select nContactKey from tblCartContact where cContactType = 'Billing Address' and nContactCartId = 0 and nContactDirId = " & UserId)
                    Dim deliveryId As Long = billingId
                    myWeb.moCart.useSavedAddressesOnCart(billingId, deliveryId)

                    'Collect the payment
                    Dim CurrencyCode As String = "GBP"
                    Dim PaymentDescription As String = "Renewal of "
                    Dim paymentStatus As String = ""

                    Dim PayInstance As New XmlDocument()
                    PayInstance.LoadXml(myWeb.moDbHelper.getObjectInstance(dbHelper.objectTypes.CartPaymentMethod, nPaymentMethodId))
                    Dim PaymentMethod As String = PayInstance.DocumentElement.SelectSingleNode("cPayMthdProviderName").InnerText

                    If PaymentMethod <> "" Then
                        Dim oPayProv As New Providers.Payment.BaseProvider(myWeb, PaymentMethod)
                        Try
                            paymentStatus = oPayProv.Activities.CollectPayment(myWeb, nPaymentMethodId, Amount, CurrencyCode, PaymentDescription, myWeb.moCart)
                        Catch ex2 As Exception
                            cProcessInfo = ex2.Message
                            ' no payment method to cancel. to we email site owner.
                        End Try
                    End If

                    If paymentStatus = "Success" Then
                        myWeb.moCart.mnProcessId = 6
                        myWeb.moCart.updateCart("Success")
                        myWeb.moCart.GetCart()
                        myWeb.moCart.addDateAndRef(myWeb.moCart.moCartXml.FirstChild, dNewStart)

                        'Send the invoice
                        If bEmailClient Then
                            myWeb.moCart.emailReceipts(myWeb.moCart.moCartXml)
                        End If

                        'myWeb.moCart.updateCart("Success")

                        myWeb.moCart.SaveCartXML(myWeb.moCart.moCartXml.FirstChild)

                        'On Success update subscription
                        Dim SubInstance As New XmlDocument()
                        SubInstance.LoadXml("<instance>" & myWeb.moDbHelper.getObjectInstance(dbHelper.objectTypes.Subscription, SubXml.GetAttribute("id")) & "</instance>")
                        Dim editElmt As XmlElement = SubInstance.DocumentElement.FirstChild

                        editElmt.SelectSingleNode("dExpireDate").InnerText = xmlDate(dNewEnd)

                        editElmt.SelectSingleNode("dUpdateDate").InnerText = xmlDate(Now())
                        editElmt.SelectSingleNode("nUpdateDirId").InnerText = myWeb.mnUserId
                        editElmt.SelectSingleNode("cDescription").InnerText = "Policy Renewed " & Now()

                        'We only remove user from groups (this needs to happen by schduler to remove once expired)

                        myWeb.moDbHelper.setObjectInstance(dbHelper.objectTypes.Subscription, SubInstance.DocumentElement)

                        'Create renewal record
                        Dim renewalInstance As New XmlDocument()
                        renewalInstance.LoadXml("<instance>" & myWeb.moDbHelper.getObjectInstance(dbHelper.objectTypes.SubscriptionRenewal, 0) & "</instance>")
                        Dim editElmt2 As XmlElement = renewalInstance.DocumentElement.FirstChild

                        editElmt2.SelectSingleNode("nSubId").InnerText = SubId
                        editElmt2.SelectSingleNode("nPaymentMethodId").InnerText = nPaymentMethodId
                        editElmt2.SelectSingleNode("nOrderId").InnerText = myWeb.moCart.mnCartId.ToString()
                        editElmt2.SelectSingleNode("nPaymentStatus").InnerText = "1"
                        editElmt2.SelectSingleNode("xNotesXml").InnerXml = SubInstance.DocumentElement.InnerXml
                        editElmt2.SelectSingleNode("dPublishDate").InnerText = xmlDate(dNewStart)
                        editElmt2.SelectSingleNode("dExpireDate").InnerText = xmlDate(dNewEnd)

                        myWeb.moDbHelper.setObjectInstance(dbHelper.objectTypes.SubscriptionRenewal, renewalInstance.DocumentElement)

                        Return "Success"

                    Else

                        myWeb.moCart.mnProcessId = 5
                        myWeb.moCart.updateCart("Failed")

                        'On Failure
                        'Record the failure
                        Dim renewalInstance As New XmlDocument()
                        renewalInstance.LoadXml("<instance>" & myWeb.moDbHelper.getObjectInstance(dbHelper.objectTypes.SubscriptionRenewal, 0) & "</instance>")
                        Dim editElmt2 As XmlElement = renewalInstance.DocumentElement.FirstChild

                        editElmt2.SelectSingleNode("nSubId").InnerText = SubId
                        editElmt2.SelectSingleNode("nPaymentMethodId").InnerText = nPaymentMethodId
                        editElmt2.SelectSingleNode("nPaymentStatus").InnerText = "0"
                        editElmt2.SelectSingleNode("xNotesXml").InnerText = "<error>" & paymentStatus & "</error>"

                        myWeb.moDbHelper.setObjectInstance(dbHelper.objectTypes.SubscriptionRenewal, renewalInstance.DocumentElement)

                        Return "Failed"

                        'ExpireSubscription(SubKey)

                    End If

                Catch ex As Exception
                    returnException(mcModuleName, "SubcriptionReminders", ex, "", "", gbDebug)
                    Return Nothing
                End Try


            End Function

            Public Sub ExpireSubscriptionGroups(ByVal SubKey As Long)
                Try
                    Dim SubXml As XmlElement = GetSubscriptionDetail(Nothing, SubKey)
                    ExpireSubscriptionGroups(SubXml.FirstChild)
                Catch ex As Exception
                    returnException(mcModuleName, "ExpireSubscriptionGroups", ex, "", "", gbDebug)
                End Try
            End Sub

            Public Sub ExpireSubscriptionGroups(ByVal SubXml As XmlElement)

                Try
                    Dim cSQL As String
                    Dim nSubId As Long = SubXml.GetAttribute("id")
                    Dim nUserId As Long = SubXml.GetAttribute("userId")

                    'Set status
                    'Remove member from groups subscription allows (checking first any other active subscriptions so we don't block the user if they have another active subscription at this time)

                    'gets a list of other active subscriptions
                    cSQL = "SELECT s.cSubXml" &
                                       " FROM tblSubscription s  INNER JOIN tblAudit a ON s.nAuditId = a.nAuditKey " &
                                       " WHERE a.nStatus = 1 and a.dExpireDate >= " & sqlDate(Now()) & " and a.dPublishDate <= " & sqlDate(Now()) & " AND s.nSubKey <> " & nSubId & " AND s.nDirId = " & nUserId

                    Dim aPermittedGroups(0) As Long
                    Dim oDS As DataSet = myWeb.moDbHelper.GetDataSet(cSQL, "Subscriptions")
                    Dim ProcessedCount As Long = 0
                    If oDS.Tables("Subscriptions").Rows.Count > 0 Then

                        Dim oDr As DataRow
                        For Each oDr In oDS.Tables("Subscriptions").Rows
                            Dim thisSubXml As New XmlDocument()
                            thisSubXml.LoadXml(oDr("cSubXml"))
                            Dim grpElmt As XmlElement
                            For Each grpElmt In thisSubXml.SelectNodes("Content/UserGroups/Group[@id!='']")
                                Array.Resize(aPermittedGroups, aPermittedGroups.Length + 1)
                                aPermittedGroups(aPermittedGroups.Length - 1) = grpElmt.GetAttribute("id")
                            Next
                        Next
                    End If

                    'steps through to remove groups from the current sub
                    Dim grpElmt2 As XmlElement
                    For Each grpElmt2 In SubXml.SelectNodes("Content/UserGroups/Group[@id!='']")
                        Dim grpId As Long = grpElmt2.GetAttribute("id")
                        'takes user out of 
                        Dim bDelete As Boolean = True
                        Dim compareGrpId As Long
                        For Each compareGrpId In aPermittedGroups
                            If compareGrpId = grpId Then
                                bDelete = False
                            End If
                        Next
                        If bDelete Then
                            myWeb.moDbHelper.maintainDirectoryRelation(grpId, nUserId, True, Now())
                        End If
                    Next

                    'Alert customer & merchant elsewhere

                Catch ex As Exception
                    returnException(mcModuleName, "ExpireSubscriptionGroups", ex, "", "", gbDebug)
                End Try

            End Sub


            'Public Function CheckExpiringSubscriptions() As Boolean

            '    ' TS ALL THIS NEEDS TO BE REWRITTEN AS THESE TABLES HAVE CHANGED...


            '    'checks upcoming renewals that dont have another set up waiting and sends emails
            '    'based on settings from config and the subscription
            '    Try
            '        Dim cSQL As String = "SELECT tblDirectorySubscriptions.*, tblCartCatProductRelations.nCatId, tblAudit.*" &
            '        " FROM tblDirectorySubscriptions INNER JOIN" &
            '        " tblDirectory ON tblDirectorySubscriptions.nUserId = tblDirectory.nDirKey INNER JOIN" &
            '        " tblAudit ON tblDirectorySubscriptions.nAuditId = tblAudit.nAuditKey LEFT OUTER JOIN" &
            '        " tblCartCatProductRelations ON tblDirectorySubscriptions.nSubscriptionId = tblCartCatProductRelations.nContentId" &
            '        " WHERE tblAudit.dExpireDate <= " & Protean.Tools.Database.SqlDate(Now)
            '        Dim oDS As DataSet = myWeb.moDbHelper.GetDataSet(cSQL, "Content")
            '        Dim oDC As DataColumn
            '        For Each oDC In oDS.Tables("Content").Columns
            '            oDC.ColumnMapping = MappingType.Attribute
            '        Next
            '        oDS.Tables("Content").Columns("cSubscriptionXML").ColumnMapping = MappingType.SimpleContent


            '        Dim oXML As New XmlDocument
            '        oXML.InnerXml = Replace(Replace(oDS.GetXml, "&gt;", ">"), "&lt;", "<")

            '        Dim oTmpElmt As XmlElement = oXML.CreateElement("Subscriptions")
            '        oTmpElmt.InnerXml = oXML.DocumentElement.InnerXml

            '        Dim oElmt As XmlElement 'Initial loop

            '        For Each oElmt In oTmpElmt.SelectNodes("Content")
            '            Dim nID As Integer = oElmt.GetAttribute("nSubscriptionKey")
            '            Dim nGroup As Integer = CInt(0 & oElmt.GetAttribute("nCatId"))
            '            Dim nUser As Integer = oElmt.GetAttribute("nUserId")
            '            'check the date so we can see if its an expiry or a the next in line
            '            If CDate(oElmt.GetAttribute("dExpireDate")) <= Now Then
            '                'its one coming up for renewal
            '                'so we need to check if it get auto paid and renewed
            '                Dim oChkElmt As XmlElement = oElmt.SelectSingleNode("Content/Type")
            '                If oChkElmt.InnerXml = "Rolling" Then
            '                    RepeatSubscriptions(nID, nUser)
            '                End If
            '                'then delete it (we don't want to delete just change status)
            '                'myWeb.moDbHelper.DeleteObject(dbHelper.objectTypes.Subscription, nID)

            '                'We simply want to remove the user from the group to let the subscription lapse.
            '            End If
            '        Next
            '        Return True
            '    Catch ex As Exception
            '        returnException(mcModuleName, "CheckExpiringSubscriptions", ex, "", "", gbDebug)
            '        Return False
            '    End Try
            'End Function

            'Public Function RepeatSubscriptions(ByVal nID As Integer, ByVal nSubUserId As Integer) As Boolean
            '    Try
            '        'This is no longer used Moved to RenewSubscription

            '        'will set up new subscriptions and deduct payments

            '        'need to do this after we have added the code to save payment codes
            '        Dim cSQL As String = "SELECT TOP 1 tblDirectorySubscriptions.nUserId, tblCartPaymentMethod.nPayMthdKey, tblDirectorySubscriptions.nSubscriptionId, tblAudit.dExpireDate" &
            '        " FROM tblDirectorySubscriptions INNER JOIN" &
            '        " tblCartPaymentMethod ON tblDirectorySubscriptions.nUserId = tblCartPaymentMethod.nPayMthdUserId INNER JOIN" &
            '        " tblAudit ON tblCartPaymentMethod.nAuditId = tblAudit.nAuditKey" &
            '        " WHERE (tblAudit.dExpireDate > " & Protean.Tools.Database.SqlDate(Now) & ") AND (tblDirectorySubscriptions.nUserId = " & nSubUserId & ")" &
            '        " ORDER BY tblCartPaymentMethod.nPayMthdKey DESC"
            '        '" WHERE (tblCartPaymentMethod.dPayMthdExpire > " & sqlDate(Now) & ")" & _
            '        Dim oDR As SqlDataReader = myWeb.moDbHelper.getDataReader(cSQL)
            '        Dim nUser As Integer = 0
            '        Dim nPayment As Integer = 0
            '        Dim nContentId As Integer = 0
            '        Do While oDR.Read
            '            nUser = oDR(0)
            '            nPayment = oDR(1)
            '            nContentId = oDR(2)
            '        Loop
            '        If nUser = 0 Or nPayment = 0 Or nContentId = 0 Then Exit Function
            '        'check the subscription still exists.
            '        cSQL = "SELECT * " &
            '        " FROM tblContent INNER JOIN " &
            '        " tblAudit ON tblContent.nAuditId = tblAudit.nAuditKey " &
            '        " WHERE nContentKey = " & nContentId &
            '        " and nStatus = 1 " &
            '        " and (dPublishDate is null or dPublishDate = 0 or dPublishDate <= " & Protean.Tools.Database.SqlDate(Now) & " )" &
            '        " and (dExpireDate is null or dExpireDate = 0 or dExpireDate >= " & Protean.Tools.Database.SqlDate(Now) & " )"

            '        Dim oDS As DataSet = myWeb.moDbHelper.GetDataSet(cSQL, "Item", "RepeatSubscription")
            '        If oDS.Tables("Item").Rows.Count > 0 Then
            '            Dim oDC As DataColumn
            '            For Each oDC In oDS.Tables("Item").Columns
            '                oDC.ColumnMapping = MappingType.Attribute
            '            Next
            '            oDS.Tables("Item").Columns("cContentXmlBrief").ColumnMapping = MappingType.Hidden
            '            oDS.Tables("Item").Columns("cContentXmlDetail").ColumnMapping = MappingType.SimpleContent
            '            'fine
            '            'lets get the amount
            '            If myCart Is Nothing Then myCart = New Cart(myWeb)
            '            myCart.mnEwUserId = nUser
            '            Dim oXML As New XmlDocument
            '            Dim oCartElmt As XmlElement = oXML.CreateElement("Order")

            '            Dim nPrice As Double = CartSubscriptionPrice(nContentId, nUser)
            '            myCart.CreateNewCart(oCartElmt)
            '            myCart.AddItem(nContentId, 1, Nothing)
            '            myCart.GetCart(oCartElmt)
            '            Dim oEwProv As New PaymentProviders(myWeb)

            '            '##Setting up PaymentProviders
            '            oEwProv.mcCurrency = IIf(myCart.mcCurrencyCode = "", myCart.mcCurrency, myCart.mcCurrencyCode)
            '            oEwProv.mcCurrencySymbol = myCart.mcCurrencySymbol

            '            oEwProv.mnPaymentAmount = nPrice
            '            oEwProv.mcPaymentOrderDescription = "Repeat Ref: An online purchase from: " & myCart.mcSiteURL & " on " & niceDate(Now) & " " & TimeValue(Now) '"Ref:" & mcOrderNoPrefix & CStr(mnCartId) & " An online purchase from: " & mcSiteURL & " on " & niceDate(Now) & " " & TimeValue(Now)
            '            oEwProv.mnCartId = myCart.mnCartId
            '            oEwProv.mcPaymentOrderDescription = "Repeat Ref:" & myCart.OrderNoPrefix & CStr(myCart.mnCartId) & " An online purchase from: " & myCart.mcSiteURL & " on " & niceDate(Now) & " " & TimeValue(Now)
            '            oEwProv.mcOrderRef = myCart.OrderNoPrefix & myCart.mnCartId

            '            '#########
            '            oXML = New XmlDocument
            '            oXML.InnerXml = Replace(Replace(oDS.GetXml, "&gt;", ">"), "&lt;", "<")
            '            Dim oElmt As XmlElement = oXML.DocumentElement.FirstChild
            '            oElmt.SetAttribute("price", nPrice)
            '            oElmt.SetAttribute("quantity", 1)

            '            Dim cResult As String = oEwProv.payRepeat(nPayment, oXML.DocumentElement, "")
            '            If cResult = "" Then
            '                'paid
            '                'need to setup the subscription
            '                myCart.mnProcessId = Cart.cartProcess.Complete
            '                myCart.GetCart(oCartElmt)
            '                myCart.addDateAndRef(oElmt)
            '                myCart.emailReceipts(oElmt)
            '                myCart.updateCart("Cart")
            '                AddUserSubscription(nContentId, nUser)

            '            Else
            '                'not paid
            '                'cant do allot here really
            '                myCart.mnProcessId = Cart.cartProcess.Failed
            '                myCart.GetCart(oCartElmt)
            '            End If
            '        End If
            '        Return True
            '    Catch ex As Exception
            '        returnException(mcModuleName, "RepeatSubscriptions", ex, "", "", gbDebug)
            '        Return False
            '    End Try
            'End Function

#End Region
            Public Class Forms
                Inherits xForm

                Private Const mcModuleName As String = "Protean.Cms.Cart.Subscriptions.Forms"
                'Private Const gbDebug As Boolean = True
                Public moDbHelper As dbHelper
                Public goConfig As System.Collections.Specialized.NameValueCollection ' = WebConfigurationManager.GetWebApplicationSection("protean/web")
                Public moCartConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/cart")
                Public mbAdminMode As Boolean = False
                Public moRequest As System.Web.HttpRequest

                ' Error Handling hasn't been formally set up for AdminXforms so this is just for method invocation found in xfrmEditContent
                Shadows Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)

                Private Sub _OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs) Handles Me.OnError
                    returnException(e.ModuleName, e.ProcedureName, e.Exception, "", e.AddtionalInformation, gbDebug)
                End Sub

                'Public myWeb As Protean.Cms

                Public Sub New(ByRef aWeb As Protean.Cms)
                    MyBase.New(aWeb)

                    PerfMon.Log("AdminXforms", "New")
                    Try
                        myWeb = aWeb
                        goConfig = myWeb.moConfig
                        moDbHelper = myWeb.moDbHelper
                        moRequest = myWeb.moRequest

                        MyBase.cLanguage = myWeb.mcPageLanguage

                    Catch ex As Exception
                        returnException(mcModuleName, "New", ex, "", "", gbDebug)
                    End Try
                End Sub

                Public Sub New()

                End Sub

                Public Overridable Function xFrmConfirmSubscription(ByVal SubscriptionId As Long, Optional ByVal FormName As String = "ConfirmSubscription") As XmlElement

                    ' Called to get XML for the User Logon.

                    Dim oFrmElmt As XmlElement
                    Dim cProcessInfo As String = ""
                    Dim bRememberMe As Boolean = False
                    Try
                        '

                        If mbAdminMode And myWeb.mnUserId = 0 Then GoTo BuildForm

                        'maCommonFolders is an array of folder locations used to look locally, then in wellardscommon and finally eoniccommon.
                        If Not Me.load("/xforms/subscription/" & FormName & ".xml", myWeb.maCommonFolders) Then
                            'If this does not load manually then build a form to do it.
                            GoTo BuildForm
                        Else
                            If SubscriptionId > 0 Then



                                Me.Instance.InnerXml = moDbHelper.getObjectInstance(dbHelper.objectTypes.Subscription, SubscriptionId)

                                Dim PaymentMethodId As Long = CLng("0" & Me.Instance.SelectSingleNode("tblSubscription/nPaymentMethodId").InnerText)

                                Me.Instance.InnerXml = Me.Instance.InnerXml &
                                                       moDbHelper.getObjectInstance(dbHelper.objectTypes.CartPaymentMethod, PaymentMethodId) &
                                                       myWeb.GetUserXML(myWeb.mnUserId).SelectSingleNode("Contacts/Contact[cContactType='Billing Address']").OuterXml

                                Dim PaymentOptionsSelect As XmlElement = Me.moXformElmt.SelectSingleNode("descendant-or-self::select1[@bind='cPaymentMethod']")

                                Dim PaymentAmount As Double = CDbl("0" & Me.Instance.SelectSingleNode("tblSubscription/nValueNet").InnerText)
                                Dim PaymentMethod As String = "0" & Me.Instance.SelectSingleNode("tblCartPaymentMethod/cPayMthdProviderName").InnerText

                                Dim xfrmGroup As XmlElement = PaymentOptionsSelect.SelectSingleNode("ancestor::group[1]")

                                Dim oPay As PaymentProviders
                                Dim bDeny As Boolean = False
                                oPay = New PaymentProviders(myWeb)
                                oPay.mcCurrency = moCartConfig("Currency")

                                If LCase(moCartConfig("PaymentTypeButtons")) = "on" Then
                                    'remove submit button
                                    Dim xSubmit As XmlElement = moXformElmt.SelectSingleNode("descendant-or-self::submit")
                                    If Not xSubmit Is Nothing Then xSubmit.ParentNode.RemoveChild(xSubmit)

                                    'add new submit button
                                    PaymentOptionsSelect.ParentNode.RemoveChild(PaymentOptionsSelect)
                                    'remove binding
                                    Dim PaymentBinding As XmlElement = moXformElmt.SelectSingleNode("descendant-or-self::bind[@id='cPaymentMethod']")
                                    PaymentBinding.ParentNode.RemoveChild(PaymentBinding)

                                    oPay.getPaymentMethodButtons(Me, xfrmGroup, PaymentAmount)

                                Else
                                    oPay.getPaymentMethods(Me, PaymentOptionsSelect, PaymentAmount, "")
                                End If

                            End If
                            GoTo Check
                        End If



BuildForm:
                        Me.NewFrm("Subscription")
                        Me.submission("Subscription", "", "post", "form_check(this)")

                        oFrmElmt = Me.addGroup(Me.moXformElmt, "Subscription", "", "No '/xforms/subscription/" & FormName & "' Form Specified")

Check:
                        ' MyBase.updateInstanceFromRequest()

                        If Me.isSubmitted Then
                            Me.validate()
                            If Me.valid Then
                                'do stuff here...

                            Else
                                valid = False
                            End If
                        End If

                        MyBase.addValues()
                        Return Me.moXformElmt


                    Catch ex As Exception
                        returnException(mcModuleName, "xFrmConfirmSubscription", ex, "", cProcessInfo, gbDebug)
                        Return Nothing
                    End Try
                End Function


            End Class

            Public Class Modules

                Public Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
                Private Const mcModuleName As String = "Protean.Cms.Membership.Modules"

                Public Sub New()

                    'do nowt

                End Sub

                Public Sub UpdateSubscriptionPaymentMethod(ByRef myWeb As Protean.Cms, ByRef contentNode As XmlElement)

                    Dim pseudoCart As New Protean.Cms.Cart(myWeb)
                    Dim pseudoOrder As Protean.Cms.Cart.Order

                    Dim ewCmd As String = myWeb.moRequest("subCmd2")
                    Dim ContentDetailElmt As XmlElement = myWeb.moPageXml.CreateElement("ContentDetail")
                    Dim SelectedPaymentMethod As String
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
                                        pseudoOrder = New Protean.Cms.Cart.Order(myWeb)
                                        pseudoOrder.PaymentMethod = SelectedPaymentMethod
                                        pseudoOrder.TransactionRef = "SUB" & CDbl(oSubForm.Instance.SelectSingleNode("tblSubscription/nSubKey").InnerText) & "-" & CDbl("0" & oSubForm.Instance.SelectSingleNode("tblSubscription/nPaymentMethodId").InnerText)

                                        pseudoOrder.firstPayment = nFirstPayment
                                        pseudoOrder.repeatPayment = CDbl(oSubForm.Instance.SelectSingleNode("tblSubscription/cSubXml/Content/SubscriptionPrices/Price[@type='sale']").InnerText)
                                        pseudoOrder.delayStart = True ' IIf(oSubForm.Instance.SelectSingleNode("tblSubscription/cSubXml/Content/SubscriptionPrices/@delayStart").InnerText = "true", True, False)
                                        pseudoOrder.startDate = dRenewalDate
                                        pseudoOrder.repeatInterval = oSubForm.Instance.SelectSingleNode("tblSubscription/cSubXml/Content/Duration/Unit").InnerText
                                        pseudoOrder.repeatLength = CInt(oSubForm.Instance.SelectSingleNode("tblSubscription/cSubXml/Content/Duration/Length").InnerText)

                                        pseudoOrder.SetAddress(oSubForm.Instance.SelectSingleNode("Contact/cContactName").InnerText,
                                        oSubForm.Instance.SelectSingleNode("Contact/cContactEmail").InnerText,
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

                                Dim ccPaymentXform As Protean.xForm = New Protean.xForm

                                pseudoCart.mcPagePath = pseudoCart.mcCartURL & myWeb.mcPagePath

                                ccPaymentXform = oPayProv.Activities.GetPaymentForm(myWeb, pseudoCart, pseudoOrder.xml, "?subCmd=updateSubPayment&subCmd2=PaymentForm&subId=" & myWeb.moRequest("subId"))

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
                                    ContentDetailElmt.AppendChild(ccPaymentXform.moXformElmt)
                                End If

                            Case "UpdateSubscription"
                                'Save updated subscription details
                                ContentDetailElmt.SetAttribute("status", "SubscriptionUpdated")
                                'Cancel old method

                        End Select

                        myWeb.moPageXml.FirstChild.AppendChild(ContentDetailElmt)
                    Catch ex As Exception
                        returnException(mcModuleName, "UpdateSubscriptionPaymentMethod", ex, "", "", gbDebug)
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
                        returnException(mcModuleName, "ManageUserSubscriptions", ex, "", "", gbDebug)
                        'Return Nothing
                    End Try

                End Sub

                Public Sub Subscribe(ByRef myWeb As Protean.Cms, ByRef contentNode As XmlElement)

                    Dim listSubs As Boolean = True
                    Dim sProcessInfo As String
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
                                    Dim oMsg As Messaging = New Messaging
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
                        returnException(mcModuleName, "ManageUserSubscriptions", ex, sProcessInfo, "", gbDebug)
                        'Return Nothing
                    End Try

                End Sub


                Public Sub CheckUpgrade(ByRef myWeb As Protean.Cms, ByRef contentNode As XmlElement)

                    Dim sProcessInfo As String
                    Try

                        'Check User Logged on
                        If myWeb.mnUserId > 0 Then

                            Dim oSub As New Subscriptions(myWeb)
                            'Upgrade Price
                            oSub.UpdateSubscriptionPrice(contentNode, myWeb.mnUserId)

                        End If



                    Catch ex As Exception
                        returnException(mcModuleName, "CheckUpgrade", ex, sProcessInfo, "", gbDebug)
                        'Return Nothing
                    End Try

                End Sub

            End Class

        End Class
    End Class
End Class
