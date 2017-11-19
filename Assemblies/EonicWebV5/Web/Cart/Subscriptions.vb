Imports System.Xml
Imports System.Web.HttpUtility
Imports System.Web.Configuration
Imports System.IO
Imports System.Collections
Imports System.Data
Imports System.Data.SqlClient
Imports System

Partial Public Class Web
    Partial Public Class Cart

        Public Class Subscriptions

            Private mcModuleName As String = "Subscriptions"

            Dim myWeb As Web
            Dim myCart As Web.Cart

            Public Sub New(ByRef aWeb As Web)
                myWeb = aWeb
                myCart = myWeb.moCart
            End Sub

            Public Sub New(ByRef aCart As Web.Cart)
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
                    Dim oDS As DataSet = myWeb.moDbHelper.getDataSet("Select * From tblCartProductCategories WHERE cCatSchemaName = 'Subscription'", "SubscriptionGroups")
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
                    nID = myWeb.moDbHelper.exeProcessSQLScalar(cSQL)
                    'if it does then fine, just return id
                    If nID > 0 Then Return nID
                    'if not need to get the last order number
                    cSQL = "SELECT nCatProductRelKey, nDisplayOrder FROM tblCartCatProductRelations WHERE nCatId = " & nSubGroup & " ORDER BY nDisplayOrder DESC"
                    nID = myWeb.moDbHelper.exeProcessSQLScalar(cSQL)
                    'add to group as bottom
                    cSQL = "INSERT INTO tblCartCatProductRelations (nContentId, nCatId, nDisplayOrder, nAuditId) VALUES (" & nSubId & ", " & nSubGroup & ", " & nID + 1 & ", " & myWeb.moDbHelper.getAuditId & ")"
                    nID = myWeb.moDbHelper.getIdInsertSQL(cSQL)
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
                    Dim cSQL As String = "SELECT tblCartItem.nCartItemKey, tblContent.nContentKey, tblContent.cContentXmlDetail, tblCartCatProductRelations.nCatId, tblCartCatProductRelations.nDisplayOrder" & _
                    " FROM tblCartItem INNER JOIN" & _
                    " tblContent ON tblCartItem.nItemId = tblContent.nContentKey LEFT OUTER JOIN" & _
                    " tblCartCatProductRelations ON tblContent.nContentKey = tblCartCatProductRelations.nContentId" & _
                    " WHERE (tblCartItem.nCartOrderId = " & nCartID & ") AND (tblContent.cContentSchemaName = N'Subscription')" & _
                    " ORDER BY tblCartItem.nCartItemKey"
                    '" ORDER BY tblCartCatProductRelations.nDisplayOrder"
                    Dim oDS As DataSet = myWeb.moDbHelper.getDataSet(cSQL, "Subs")
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

                                        If oDR2("nCatId") = oDR("nCatId") And _
                                        Not oDR2("nContentKey") = oDR("nContentKey") And _
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
                                myWeb.moDbHelper.exeProcessSQL(cSQL)
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
                    Dim oDS As DataSet = myWeb.moDbHelper.getDataSet(cSQL, "Content")
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

                    oDS = myWeb.moDbHelper.getDataSet(cSQL, "Content")
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

            Private Function SubscriptionEndDate(ByVal dStart As Date, ByVal oSubDetailElmt As XmlElement) As Date
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
                                Return dStart.AddDays(cDuration)
                            Case "Week"
                                Return dStart.AddDays(cDuration * 7)
                            Case "Month"
                                Return dStart.AddMonths(cDuration)
                            Case "Year"
                                Return dStart.AddYears(cDuration)
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
                Dim oSubConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("eonic/subscriptions")
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

                        Dim cSQL As String = "SELECT tblContent.nContentKey, tblContent.cContentXmlBrief" &
                        " FROM tblContent INNER JOIN tblCartItem ON tblContent.nContentKey = tblCartItem.nItemId" &
                        " WHERE (tblCartItem.nCartOrderId = " & nCartId & ") AND (tblContent.cContentSchemaName IN (" & SubscrptionSchemaTypes & "))"

                        Dim oDS As DataSet = myWeb.moDbHelper.GetDataSet(cSQL, "Subs")
                        If oDS.Tables("Subs").Rows.Count > 0 Then
                            Dim oDR As DataRow
                            For Each oDR In oDS.Tables("Subs").Rows
                                AddUserSubscription(oDR("nContentKey"), nSubUserId, nPaymentMethodId)
                                cLastSubXml = oDR("cContentXmlBrief")
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

            Public Sub AddUserSubscription(ByVal nSubscriptionID As Integer, ByVal nSubUserId As Integer, Optional ByVal nPaymentMethodId As Integer = 0)
                Try
                    'same sort of process as getting prices.
                    'this time we need to replace if regrading
                    'add a new one for renewals to start the next day (for the scheduler to pickup)
                    'add new ones straight in.

                    'First we'll get the xml for this subscription
                    Dim cSQL As String = "SELECT tblContent.*, tblCartCatProductRelations.nCatId FROM tblContent LEFT OUTER JOIN tblCartCatProductRelations ON tblContent.nContentKey = tblCartCatProductRelations.nContentId where tblContent.nContentKey = " & nSubscriptionID
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

                    oDS = myWeb.moDbHelper.GetDataSet(cSQL, "Content")
                    If oDS.Tables("Content").Rows.Count = 0 Then
                        'No Other subscriptions or subscriptions in the same group
                        'so we just add it
                        AddSubscription(nSubscriptionID, oCurSubElmt.SelectSingleNode("Content"), Now, SubscriptionEndDate(Now, oCurSubElmt.SelectSingleNode("Content")), nSubUserId, nPaymentMethodId)
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
                            'RemoveSubscription(oDS.Tables("Content").Rows(0)("nSubscriptionKey"), oNewElmt.SelectSingleNode("Content"), nSubUserId)
                            'myWeb.moDbHelper.DeleteObject(dbHelper.objectTypes.Subscription, oDS.Tables("Content").Rows(0)("nSubKey"))
                            Dim oRows As DataRow
                            For Each oRows In oDS.Tables("Content").Rows
                                CancelSubscription(oRows("nSubKey"))
                            Next

                            'need to get the details of the current new one
                            cSQL = "Select * From tblContent Where nContentKey = " & nSubscriptionID
                            Dim oDS2 As DataSet = myWeb.moDbHelper.GetDataSet(cSQL, "Content")
                            For Each oDC In oDS2.Tables("Content").Columns
                                oDC.ColumnMapping = MappingType.Attribute
                            Next
                            oDS2.Tables("Content").Columns("cContentXmlBrief").ColumnMapping = MappingType.Hidden
                            oDS2.Tables("Content").Columns("cContentXmlDetail").ColumnMapping = MappingType.SimpleContent
                            oExXML.InnerXml = Replace(Replace(oDS2.GetXml, "&gt;", ">"), "&lt;", "<")
                            oNewElmt = oExXML.DocumentElement.FirstChild

                            AddSubscription(nSubscriptionID, oNewElmt.SelectSingleNode("Content"), Now, SubscriptionEndDate(Now, oNewElmt.SelectSingleNode("Content")), nSubUserId, nPaymentMethodId)
                        Else
                            'its a renewal
                            'add
                            Dim dRenStart As Date = GetRenewalDate(oNewElmt.GetAttribute("nSubKey"))
                            dRenStart = dRenStart.AddDays(1)
                            AddSubscription(nSubscriptionID, oNewElmt.SelectSingleNode("Content"), dRenStart, SubscriptionEndDate(dRenStart, oNewElmt.SelectSingleNode("Content")), nSubUserId, nPaymentMethodId)
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
                    Dim cSQL As String = "SELECT tblSubscription.nSubKey,tblSubscription.cRenewalStatus, tblSubscription.nSubContentId, tblSubscription.cSubXML, tblAudit.nStatus, tblAudit.dPublishDate, tblAudit.dExpireDate" & _
                    " FROM tblSubscription INNER JOIN" & _
                    " tblAudit ON tblSubscription.nAuditId = tblAudit.nAuditKey" & _
                    " WHERE tblSubscription.nDirId = " & nSubUserId

                    Dim oDS As DataSet = myWeb.moDbHelper.getDataSet(cSQL, "Subscriptions", "Subscriptions")
                    If oDS.Tables("Subscriptions").Rows.Count > 0 Then
                        oDS.Tables("Subscriptions").Columns("nSubKey").ColumnMapping = MappingType.Attribute
                        oDS.Tables("Subscriptions").Columns("nSubContentId").ColumnMapping = MappingType.Attribute
                        oDS.Tables("Subscriptions").Columns("cRenewalStatus").ColumnMapping = MappingType.Attribute
                        oDS.Tables("Subscriptions").Columns("nStatus").ColumnMapping = MappingType.Attribute
                        'oDS.Tables("Subscriptions").Columns("dStartDate").ColumnMapping = MappingType.Attribute
                        'oDS.Tables("Subscriptions").Columns("dEndDate").ColumnMapping = MappingType.Attribute
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

            Public Sub AddSubscription(ByVal nid As Integer, ByVal oSubDetailElmt As XmlElement, ByVal dStart As Date, ByVal dFinish As Date, ByVal nSubUserId As Integer, Optional ByVal nPaymentMethodId As Integer = 0)
                Try
                    Dim oXml As XmlDocument = New XmlDocument
                    Dim oInstance As XmlElement = oXml.CreateElement("instance")
                    Dim oElmt As XmlNode = oXml.CreateElement("tblSubscriptions")
                    addNewTextNode("nDirId", oElmt, nSubUserId)
                    addNewTextNode("nSubContentId", oElmt, nid)
                    Dim oElmt2 As XmlElement = addNewTextNode("cSubXml", oElmt)
                    oElmt2.InnerXml = oSubDetailElmt.OuterXml
                    addNewTextNode("dStartDate", oElmt, Eonic.xmlDate(dStart))


                    addNewTextNode("cSubName", oElmt, oSubDetailElmt.SelectSingleNode("Name").InnerText)
                    addNewTextNode("nPeriod", oElmt, oSubDetailElmt.SelectSingleNode("Duration/Length").InnerText)
                    addNewTextNode("cPeriodUnit", oElmt, oSubDetailElmt.SelectSingleNode("Duration/Unit").InnerText)
                    addNewTextNode("nValueNet", oElmt, CDbl("0" & oSubDetailElmt.SelectSingleNode("SubscriptionPrices/Price[@type='sale']").InnerText))
                    addNewTextNode("nPaymentMethodId", oElmt, nPaymentMethodId)
                    addNewTextNode("bPaymentMethodActive", oElmt, "true")
                    addNewTextNode("nMinimumTerm", oElmt, oSubDetailElmt.SelectSingleNode("Duration/MinimumTerm").InnerText)
                    addNewTextNode("nRenewalTerm", oElmt, oSubDetailElmt.SelectSingleNode("Duration/RenewalTerm").InnerText)
                    addNewTextNode("cRenewalStatus", oElmt, oSubDetailElmt.SelectSingleNode("Type").InnerText)


                    'addNewTextNode("dEndDate", oElmt, dFinish)
                    addNewTextNode("nAuditId", oElmt, myWeb.moDbHelper.getAuditId(1, myWeb.mnUserId, "Subscription", dStart, dFinish, Now))
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

            Public Sub CancelSubscription(ByVal nId As Integer)
                Try

                    Dim SubInstance As New XmlDocument()
                    SubInstance.LoadXml(myWeb.moDbHelper.getObjectInstance(dbHelper.objectTypes.Subscription, nId))

                    Dim editElmt As XmlElement = SubInstance.DocumentElement

                    editElmt.SelectSingleNode("nStatus").InnerText = dbHelper.Status.Rejected
                    editElmt.SelectSingleNode("cRenewalStatus").InnerText = "Cancelled"

                    editElmt.SelectSingleNode("dUpdateDate").InnerText = xmlDate(Now())
                    editElmt.SelectSingleNode("nUpdateDirId").InnerText = myWeb.mnUserId

                    'Cancel the payment method
                    CancelPaymentMethod(editElmt.SelectSingleNode("nPaymentMethodId").InnerText)

                    'We only remove user from groups (this needs to happen by schduler to remove once expired)

                    myWeb.moDbHelper.setObjectInstance(dbHelper.objectTypes.Subscription, SubInstance.DocumentElement, nId)

                    'Email the site owner to inform of cancelation !!!

                Catch ex As Exception
                    returnException(mcModuleName, "CancelSubscription", ex, "", "", gbDebug)
                End Try
            End Sub

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

            Public Function SubcriptionReminders() As Boolean
                'send reminders to out for closing subscriptions
                Try
                    Dim oSubConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("eonic/subscriptions")
                    Dim oDT1 As String = oSubConfig("ReminderDue1")
                    Dim oDT2 As String = oSubConfig("ReminderDue2")
                    Dim oDT3 As String = oSubConfig("ReminderDue3")
                    Dim cSQL As String = "SELECT tblSubscription.*,DATEDIFF(dd,tblAudit.dExpireDate,getdate()) as renewaldays, tblDirectory.cDirXml, tblCartPaymentMethod.*, tblAudit.*" &
                    " FROM tblSubscription " &
                    " INNER JOIN tblDirectory ON tblSubscription.nDirId = tblDirectory.nDirKey " &
                    " INNER JOIN tblCartPaymentMethod ON tblCartPaymentMethod.nPayMthdKey = tblSubscription.nPaymentMethodId " &
                    " INNER JOIN tblAudit ON tblSubscription.nAuditId = tblAudit.nAuditKey"
                    Dim cCriteria As String = ""
                    If IsNumeric(oDT1) Then
                        If Not cCriteria = "" Then cCriteria &= " OR "
                        cCriteria &= "(tblAudit.dExpireDate > " & Eonic.Tools.Database.SqlDate(Now.AddDays(oDT1)) & " AND tblAudit.dExpireDate < " & Eonic.Tools.Database.SqlDate(Now.AddDays(oDT1 + 1)) & " AND NOT(cRenewalStatus LIKE 'Renewed'))"
                    End If

                    If IsNumeric(oDT2) Then
                        If Not cCriteria = "" Then cCriteria &= " OR "
                        cCriteria &= "(tblAudit.dExpireDate > " & Eonic.Tools.Database.SqlDate(Now.AddDays(oDT2)) & " AND tblAudit.dExpireDate < " & Eonic.Tools.Database.SqlDate(Now.AddDays(oDT2 + 1)) & "  AND NOT(cRenewalStatus LIKE 'Renewed'))"
                    End If

                    If IsNumeric(oDT3) Then
                        If Not cCriteria = "" Then cCriteria &= " OR "
                        cCriteria &= "(tblAudit.dExpireDate > " & Eonic.Tools.Database.SqlDate(Now.AddDays(oDT3)) & " AND tblAudit.dExpireDate < " & Eonic.Tools.Database.SqlDate(Now.AddDays(oDT3 + 1)) & "  AND NOT(cRenewalStatus LIKE 'Renewed'))"
                    End If


                    If Not cCriteria = "" Then cCriteria &= " OR "
                    cCriteria &= "(tblAudit.dExpireDate > " & Eonic.Tools.Database.SqlDate(Now) & " AND tblAudit.dExpireDate < " & Eonic.Tools.Database.SqlDate(Now.AddDays(1)) & "  AND NOT(cRenewalStatus LIKE 'Renewed'))"


                    If Not cCriteria = "" Then
                        cCriteria = " WHERE (" & cCriteria & ")"
                        cCriteria &= "AND (((SELECT COUNT(nSubContentId) FROM tblSubscription Subs" &
                        "  WHERE (nSubKey = tblSubscription.nSubKey) AND (nDirId = tblSubscription.nDirId))) = 1) "
                        cSQL &= cCriteria
                    Else
                        'no reminders set so no emails
                        Return True
                    End If


                    'thats the sql sorted
                    'now to make it xml
                    Dim oDS As DataSet = myWeb.moDbHelper.getDataSet(cSQL, "Content")
                    Dim oDC As DataColumn
                    For Each oDC In oDS.Tables("Content").Columns
                        oDC.ColumnMapping = MappingType.Attribute
                    Next

                    'oDS.Tables("Content").Columns("cDirXml").ColumnMapping = MappingType.Hidden
                    oDS.Tables("Content").Columns("cSubXML").ColumnMapping = MappingType.SimpleContent
                    '  oDS.Tables("Content").Columns("cDirXml").ColumnMapping = MappingType.SimpleContent
                    '  oDS.Tables("Content").Columns("cPayMthdDetailXml").ColumnMapping = MappingType.SimpleContent


                    Dim oXML As New XmlDocument
                    'oXML.InnerXml = Replace(Replace(oDS.GetXml, "&gt;", ">"), "&lt;", "<")
                    oXML.InnerXml = oDS.GetXml

                    Dim oTmpElmt As XmlElement = oXML.CreateElement("Subscriptions")
                    oTmpElmt.InnerXml = oXML.DocumentElement.InnerXml
                    'lets get the reminder text and the email address
                    Dim oElmt As XmlElement
                    Dim i As Integer = 0
                    For Each oElmt In oTmpElmt.SelectNodes("Content")

                        Dim cInnerXML As String = oElmt.InnerText
                        oElmt.InnerText = ""
                        oElmt.InnerXml = Replace(Replace(cInnerXML, "&gt;", ">"), "&lt;", "<")
                        'UserXML
                        Dim oUserDetailElmt As XmlElement = oXML.CreateElement("User")
                        oUserDetailElmt.InnerXml = Replace(Replace(oElmt.GetAttribute("cDirXml"), "&gt;", ">"), "&lt;", "<")
                        oElmt.SetAttribute("cDirXml", "")
                        oElmt.AppendChild(oUserDetailElmt)

                        'PaymentDetails XML
                        Dim oPayDetailsElmt As XmlElement = oXML.CreateElement("PaymentDetails")
                        oPayDetailsElmt.InnerXml = Replace(Replace(oElmt.GetAttribute("cPayMthdDetailXml"), "&gt;", ">"), "&lt;", "<")
                        oElmt.SetAttribute("cPayMthdDetailXml", "")
                        oElmt.AppendChild(oPayDetailsElmt)

                        'Check payment method is still valid
                        If oElmt.GetAttribute("cPayMthdProviderName") <> "" Then
                            Dim oPayProv As New Providers.Payment.BaseProvider(myWeb, oElmt.GetAttribute("cPayMthdProviderName"))

                            Dim paymentStatus As String
                            Try
                                paymentStatus = oPayProv.Activities.CheckStatus(myWeb, oElmt.GetAttribute("cPayMthdProviderRef"))
                                oPayDetailsElmt.SetAttribute("paymentStatus", paymentStatus)
                            Catch ex2 As Exception
                                oPayDetailsElmt.SetAttribute("paymentStatus", "error")
                            End Try
                        Else
                            oPayDetailsElmt.SetAttribute("paymentStatus", "unknown")
                        End If
                        oElmt.AppendChild(oPayDetailsElmt)

                        Dim oMessager As New Eonic.Messaging
                        Dim oUserElmt As XmlElement = oTmpElmt.OwnerDocument.CreateElement("User")
                        oUserElmt.InnerXml = Replace(Replace(oDS.Tables("Content").Rows(i)("cDirXml"), "&gt;", ">"), "&lt;", "<")

                        Dim cRetMessage As String = oMessager.emailer(oElmt, oSubConfig("ReminderXSL"), oSubConfig("SubscriptionEmailName"), oSubConfig("SubscriptionEmail"), oUserElmt.SelectSingleNode("User/Email").InnerText, oSubConfig("ReminderSubject"))

                        i += 1
                    Next
                    Return True
                Catch ex As Exception
                    returnException(mcModuleName, "SubcriptionReminders", ex, "", "", gbDebug)
                    Return False
                End Try
            End Function

            Public Function CheckExpiringSubscriptions() As Boolean
                'checks upcoming renewals that dont have another set up waiting and sends emails
                'based on settings from config and the subscription
                Try
                    Dim cSQL As String = "SELECT tblDirectorySubscriptions.*, tblCartCatProductRelations.nCatId, tblAudit.*" & _
                    " FROM tblDirectorySubscriptions INNER JOIN" & _
                    " tblDirectory ON tblDirectorySubscriptions.nUserId = tblDirectory.nDirKey INNER JOIN" & _
                    " tblAudit ON tblDirectorySubscriptions.nAuditId = tblAudit.nAuditKey LEFT OUTER JOIN" & _
                    " tblCartCatProductRelations ON tblDirectorySubscriptions.nSubscriptionId = tblCartCatProductRelations.nContentId" & _
                    " WHERE tblAudit.dExpireDate <= " & Eonic.Tools.Database.SqlDate(Now)
                    Dim oDS As DataSet = myWeb.moDbHelper.getDataSet(cSQL, "Content")
                    Dim oDC As DataColumn
                    For Each oDC In oDS.Tables("Content").Columns
                        oDC.ColumnMapping = MappingType.Attribute
                    Next
                    oDS.Tables("Content").Columns("cSubscriptionXML").ColumnMapping = MappingType.SimpleContent


                    Dim oXML As New XmlDocument
                    oXML.InnerXml = Replace(Replace(oDS.GetXml, "&gt;", ">"), "&lt;", "<")

                    Dim oTmpElmt As XmlElement = oXML.CreateElement("Subscriptions")
                    oTmpElmt.InnerXml = oXML.DocumentElement.InnerXml

                    Dim oElmt As XmlElement 'Initial loop

                    For Each oElmt In oTmpElmt.SelectNodes("Content")
                        Dim nID As Integer = oElmt.GetAttribute("nSubscriptionKey")
                        Dim nGroup As Integer = CInt(0 & oElmt.GetAttribute("nCatId"))
                        Dim nUser As Integer = oElmt.GetAttribute("nUserId")
                        'check the date so we can see if its an expiry or a the next in line
                        If CDate(oElmt.GetAttribute("dExpireDate")) <= Now Then
                            'its one coming up for renewal
                            'so we need to check if it get auto paid and renewed
                            Dim oChkElmt As XmlElement = oElmt.SelectSingleNode("Content/Type")
                            If oChkElmt.InnerXml = "Rolling" Then
                                RepeatSubscriptions(nID, nUser)
                            End If
                            'then delete it
                            myWeb.moDbHelper.DeleteObject(dbHelper.objectTypes.Subscription, nID)

                            'We simply want to remove the user from the group to let the subscription lapse.
                        End If
                    Next
                    Return True
                Catch ex As Exception
                    returnException(mcModuleName, "CheckExpiringSubscriptions", ex, "", "", gbDebug)
                    Return False
                End Try
            End Function

            Public Function RepeatSubscriptions(ByVal nID As Integer, ByVal nSubUserId As Integer) As Boolean
                Try
                    'will set up new subscriptions and deduct payments

                    'need to do this after we have added the code to save payment codes
                    Dim cSQL As String = "SELECT TOP 1 tblDirectorySubscriptions.nUserId, tblCartPaymentMethod.nPayMthdKey, tblDirectorySubscriptions.nSubscriptionId, tblAudit.dExpireDate" & _
                    " FROM tblDirectorySubscriptions INNER JOIN" & _
                    " tblCartPaymentMethod ON tblDirectorySubscriptions.nUserId = tblCartPaymentMethod.nPayMthdUserId INNER JOIN" & _
                    " tblAudit ON tblCartPaymentMethod.nAuditId = tblAudit.nAuditKey" & _
                    " WHERE (tblAudit.dExpireDate > " & Eonic.Tools.Database.SqlDate(Now) & ") AND (tblDirectorySubscriptions.nUserId = " & nSubUserId & ")" & _
                    " ORDER BY tblCartPaymentMethod.nPayMthdKey DESC"
                    '" WHERE (tblCartPaymentMethod.dPayMthdExpire > " & sqlDate(Now) & ")" & _
                    Dim oDR As SqlDataReader = myWeb.moDbHelper.getDataReader(cSQL)
                    Dim nUser As Integer = 0
                    Dim nPayment As Integer = 0
                    Dim nContentId As Integer = 0
                    Do While oDR.Read
                        nUser = oDR(0)
                        nPayment = oDR(1)
                        nContentId = oDR(2)
                    Loop
                    If nUser = 0 Or nPayment = 0 Or nContentId = 0 Then Exit Function
                    'check the subscription still exists.
                    cSQL = "SELECT * " & _
                    " FROM tblContent INNER JOIN " & _
                    " tblAudit ON tblContent.nAuditId = tblAudit.nAuditKey " & _
                    " WHERE nContentKey = " & nContentId & _
                    " and nStatus = 1 " & _
                    " and (dPublishDate is null or dPublishDate = 0 or dPublishDate <= " & Eonic.Tools.Database.SqlDate(Now) & " )" & _
                    " and (dExpireDate is null or dExpireDate = 0 or dExpireDate >= " & Eonic.Tools.Database.SqlDate(Now) & " )"

                    Dim oDS As DataSet = myWeb.moDbHelper.getDataSet(cSQL, "Item", "RepeatSubscription")
                    If oDS.Tables("Item").Rows.Count > 0 Then
                        Dim oDC As DataColumn
                        For Each oDC In oDS.Tables("Item").Columns
                            oDC.ColumnMapping = MappingType.Attribute
                        Next
                        oDS.Tables("Item").Columns("cContentXmlBrief").ColumnMapping = MappingType.Hidden
                        oDS.Tables("Item").Columns("cContentXmlDetail").ColumnMapping = MappingType.SimpleContent
                        'fine
                        'lets get the amount
                        If myCart Is Nothing Then myCart = New Cart(myWeb)
                        myCart.mnEwUserId = nUser
                        Dim oXML As New XmlDocument
                        Dim oCartElmt As XmlElement = oXML.CreateElement("Order")

                        Dim nPrice As Double = CartSubscriptionPrice(nContentId, nUser)
                        myCart.CreateNewCart(oCartElmt)
                        myCart.AddItem(nContentId, 1, Nothing)
                        myCart.GetCart(oCartElmt)
                        Dim oEwProv As New PaymentProviders(myWeb)

                        '##Setting up PaymentProviders
                        oEwProv.mcCurrency = IIf(myCart.mcCurrencyCode = "", myCart.mcCurrency, myCart.mcCurrencyCode)
                        oEwProv.mcCurrencySymbol = myCart.mcCurrencySymbol

                        oEwProv.mnPaymentAmount = nPrice
                        oEwProv.mcPaymentOrderDescription = "Repeat Ref: An online purchase from: " & myCart.mcSiteURL & " on " & niceDate(Now) & " " & TimeValue(Now) '"Ref:" & mcOrderNoPrefix & CStr(mnCartId) & " An online purchase from: " & mcSiteURL & " on " & niceDate(Now) & " " & TimeValue(Now)
                        oEwProv.mnCartId = myCart.mnCartId
                        oEwProv.mcPaymentOrderDescription = "Repeat Ref:" & myCart.OrderNoPrefix & CStr(myCart.mnCartId) & " An online purchase from: " & myCart.mcSiteURL & " on " & niceDate(Now) & " " & TimeValue(Now)
                        oEwProv.mcOrderRef = myCart.OrderNoPrefix & myCart.mnCartId

                        '#########
                        oXML = New XmlDocument
                        oXML.InnerXml = Replace(Replace(oDS.GetXml, "&gt;", ">"), "&lt;", "<")
                        Dim oElmt As XmlElement = oXML.DocumentElement.FirstChild
                        oElmt.SetAttribute("price", nPrice)
                        oElmt.SetAttribute("quantity", 1)

                        Dim cResult As String = oEwProv.payRepeat(nPayment, oXML.DocumentElement, "")
                        If cResult = "" Then
                            'paid
                            'need to setup the subscription
                            myCart.mnProcessId = Cart.cartProcess.Complete
                            myCart.GetCart(oCartElmt)
                            myCart.addDateAndRef(oElmt)
                            myCart.emailReceipts(oElmt)
                            myCart.updateCart("Cart")
                            AddUserSubscription(nContentId, nUser)

                        Else
                            'not paid
                            'cant do allot here really
                            myCart.mnProcessId = Cart.cartProcess.Failed
                            myCart.GetCart(oCartElmt)
                        End If
                    End If
                    Return True
                Catch ex As Exception
                    returnException(mcModuleName, "RepeatSubscriptions", ex, "", "", gbDebug)
                    Return False
                End Try
            End Function

#End Region
            Public Class Forms
                Inherits xForm

                Private Const mcModuleName As String = "Eonic.Web.Cart.Subscriptions.Forms"
                'Private Const gbDebug As Boolean = True
                Public moDbHelper As dbHelper
                Public goConfig As System.Collections.Specialized.NameValueCollection ' = WebConfigurationManager.GetWebApplicationSection("eonic/web")
                Public moCartConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("eonic/cart")
                Public mbAdminMode As Boolean = False
                Public moRequest As System.Web.HttpRequest

                ' Error Handling hasn't been formally set up for AdminXforms so this is just for method invocation found in xfrmEditContent
                Shadows Event OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)

                Private Sub _OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs) Handles Me.OnError
                    returnException(e.ModuleName, e.ProcedureName, e.Exception, "", e.AddtionalInformation, gbDebug)
                End Sub

                'Public myWeb As Eonic.Web

                Public Sub New(ByRef aWeb As Eonic.Web)
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

                                Dim PaymentMethodId As Long = CLng(Me.Instance.SelectSingleNode("tblSubscription/nPaymentMethodId").InnerText)

                                Me.Instance.InnerXml = Me.Instance.InnerXml & _
                                                       moDbHelper.getObjectInstance(dbHelper.objectTypes.CartPaymentMethod, PaymentMethodId) & _
                                                       myWeb.GetUserXML(myWeb.mnUserId).SelectSingleNode("Contacts/Contact[cContactType='Billing Address']").OuterXml

                                Dim PaymentOptionsSelect = Me.moXformElmt.SelectSingleNode("descendant-or-self::select1[@bind='cPaymentMethod']")

                                Dim PaymentAmount As Double = CDbl("0" & Me.Instance.SelectSingleNode("tblSubscription/nValueNet").InnerText)
                                Dim PaymentMethod As String = "0" & Me.Instance.SelectSingleNode("tblCartPaymentMethod/cPayMthdProviderName").InnerText

                                Dim oPay As PaymentProviders
                                Dim bDeny As Boolean = False
                                oPay = New PaymentProviders(myWeb)
                                oPay.mcCurrency = moCartConfig("Currency")

                                oPay.getPaymentMethods(Me, PaymentOptionsSelect, PaymentAmount, "")


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
                            If valid = False Then
                                myWeb.mnUserId = 0
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

                Public Event OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)
                Private Const mcModuleName As String = "Eonic.Web.Membership.Modules"

                Public Sub New()

                    'do nowt

                End Sub

                Public Sub UpdateSubscriptionPaymentMethod(ByRef myWeb As Eonic.Web, ByRef contentNode As XmlElement)

                    Dim pseudoCart As New Eonic.Web.Cart(myWeb)
                    Dim pseudoOrder As Eonic.Web.Cart.Order

                    Dim ewCmd As String = myWeb.moRequest("ewCmd2")
                    Dim ContentDetailElmt As XmlElement = myWeb.moPageXml.CreateElement("ContentDetail")
                    Dim SelectedPaymentMethod As String
processFlow:
                    Select Case ewCmd

                        Case ""
                            'Confirm Subscription Details Form
                            Dim oSubForm As Eonic.Web.Cart.Subscriptions.Forms = New Eonic.Web.Cart.Subscriptions.Forms(myWeb)
                            Dim confSubForm As XmlElement = oSubForm.xFrmConfirmSubscription(myWeb.moRequest("subId"))

                            If oSubForm.isSubmitted Then
                                oSubForm.updateInstanceFromRequest()
                                oSubForm.validate()
                                SelectedPaymentMethod = myWeb.moRequest("cPaymentMethod")

                                If oSubForm.valid Then
                                    ewCmd = "PaymentForm"
                                    pseudoOrder = New Eonic.Web.Cart.Order(myWeb)
                                    pseudoOrder.PaymentMethod = SelectedPaymentMethod
                                    pseudoOrder.TransactionRef = CDbl(oSubForm.Instance.SelectSingleNode("tblSubscription/nSubKey").InnerText) & "_" & Guid.NewGuid().ToString

                                    pseudoOrder.firstPayment = 0 ' CDbl(oSubForm.Instance.SelectSingleNode("tblSubscription/cSubXml/Content/Prices/Price[@type='sale']").InnerText)
                                    pseudoOrder.repeatPayment = CDbl(oSubForm.Instance.SelectSingleNode("tblSubscription/cSubXml/Content/SubscriptionPrices/Price[@type='sale']").InnerText)
                                    pseudoOrder.delayStart = True ' IIf(oSubForm.Instance.SelectSingleNode("tblSubscription/cSubXml/Content/SubscriptionPrices/@delayStart").InnerText = "true", True, False)
                                    pseudoOrder.startDate = CDate(oSubForm.Instance.SelectSingleNode("tblSubscription/dExpireDate").InnerText)
                                    pseudoOrder.repeatInterval = oSubForm.Instance.SelectSingleNode("tblSubscription/cSubXml/Content/Duration/Unit").InnerText
                                    pseudoOrder.repeatLength = CInt(oSubForm.Instance.SelectSingleNode("tblSubscription/cSubXml/Content/Duration/Length").InnerText)

                                    pseudoOrder.SetAddress(oSubForm.Instance.SelectSingleNode("Contact/cContactName").InnerText, _
                                    oSubForm.Instance.SelectSingleNode("Contact/cContactEmail").InnerText, _
                                    oSubForm.Instance.SelectSingleNode("Contact/cContactCompany").InnerText, _
                                    oSubForm.Instance.SelectSingleNode("Contact/cContactAddress").InnerText, _
                                    oSubForm.Instance.SelectSingleNode("Contact/cContactCity").InnerText, _
                                    oSubForm.Instance.SelectSingleNode("Contact/cContactState").InnerText, _
                                    oSubForm.Instance.SelectSingleNode("Contact/cContactZip").InnerText, _
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

                            Dim ccPaymentXform As xForm = New xForm

                            pseudoCart.mcPagePath = pseudoCart.mcCartURL & myWeb.mcPagePath

                            ccPaymentXform = oPayProv.Activities.GetPaymentForm(myWeb, pseudoCart, pseudoOrder.xml, "?ewCmd=updateSubPayment&ewCmd2=PaymentForm&subId=" & myWeb.moRequest("subId"))

                            If ccPaymentXform.valid Then
                                ewCmd = "UpdateSubscription"
                                'Cancel Old Payment Method
                                Dim oldPaymentId As Long = myWeb.moDbHelper.ExeProcessSqlScalar("select nPaymentMethodId from tblSubscription where nSubKey = " & myWeb.moRequest("subId"))
                                Dim oSubs As New Eonic.Web.Cart.Subscriptions(myWeb)
                                oSubs.CancelPaymentMethod(oldPaymentId)
                                'Set new payment method Id
                                myWeb.moDbHelper.ExeProcessSql("update tblSubscription set nPaymentMethodId=" & pseudoCart.mnPaymentId & "where nSubKey = " & myWeb.moRequest("subId"))
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

                End Sub

                Public Sub ManageUserSubscriptions(ByRef myWeb As Eonic.Web, ByRef contentNode As XmlElement)

                    Dim sSql As String
                    Dim oDS As DataSet
                    Dim oDr As DataRow
                    Dim oElmt As XmlElement
                    Dim listSubs As Boolean = True
                    Try

                        If myWeb.mnUserId > 0 Then

                            Select Case myWeb.moRequest("ewCmd")
                                Case "updateSubPayment"
                                    UpdateSubscriptionPaymentMethod(myWeb, contentNode)
                                    listSubs = False
                                Case "cancelSub"
                                    Dim oSubs As New Eonic.Web.Cart.Subscriptions(myWeb)
                                    oSubs.CancelSubscription(myWeb.moRequest("subId"))

                            End Select


                            If listSubs Then

                                sSql = "select tblAudit.nStatus as status, nSubKey as id, cSubName as name, cSubXml, dStartDate, nPeriod as period, cPeriodUnit as periodUnit, nValueNet as value, cRenewalStatus as renewalStatus, pay.cPayMthdProviderName as providerName, pay.cPayMthdProviderRef as providerRef" & _
                                " from tblSubscription sub INNER JOIN tblAudit ON sub.nAuditId = tblAudit.nAuditKey " & _
                                " LEFT OUTER JOIN tblCartPaymentMethod pay on pay.nPayMthdKey = sub.nPaymentMethodId " & _
                                " where sub.nDirId = " & myWeb.mnUserId

                                oDS = myWeb.moDbHelper.GetDataSet(sSql, "tblSubscription", "OrderList")
                                For Each oDr In oDS.Tables(0).Rows
                                    oElmt = myWeb.moPageXml.CreateElement("Subscription")
                                    oElmt.InnerXml = oDr("cSubXml")
                                    oElmt.SetAttribute("status", oDr("status").ToString())
                                    oElmt.SetAttribute("id", oDr("id").ToString())
                                    oElmt.SetAttribute("name", oDr("name").ToString())
                                    oElmt.SetAttribute("startDate", xmlDate(oDr("dStartDate")))
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
                                            paymentStatus = oPayProv.Activities.CheckStatus(myWeb, oDr("providerRef").ToString())
                                            oElmt.SetAttribute("paymentStatus", paymentStatus)
                                        Catch ex2 As Exception
                                            oElmt.SetAttribute("paymentStatus", "error")
                                        End Try
                                    Else
                                        oElmt.SetAttribute("paymentStatus", "unknown")
                                    End If


                                    contentNode.AppendChild(oElmt)
                                Next
                            End If


                        End If

                    Catch ex As Exception
                        returnException(mcModuleName, "ManageUserSubscriptions", ex, "", "", gbDebug)
                        'Return Nothing
                    End Try

                End Sub

                Public Sub Subscribe(ByRef myWeb As Eonic.Web, ByRef contentNode As XmlElement)

                    Dim listSubs As Boolean = True
                    Dim sProcessInfo As String
                    Try

                        'First we check if free trail

                        If CInt("0" & contentNode.SelectSingleNode("Prices/Price[@type='sale']").InnerText) = 0 And CInt("0" & contentNode.SelectSingleNode("SubscriptionPrices/Price[@type='sale']").InnerText) = 0 Then

                            If myWeb.mnUserId > 0 Then

                                If myWeb.moRequest("ewCmd") = "Subscribe" Then

                                    Dim oSubs As New Eonic.Web.Cart.Subscriptions(myWeb)
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
                                    Dim ofs As New Eonic.fsHelper()
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


                Public Sub CheckUpgrade(ByRef myWeb As Eonic.Web, ByRef contentNode As XmlElement)

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
