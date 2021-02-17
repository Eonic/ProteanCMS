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
        Public Class Discount

            Dim moCartConfig As System.Collections.Specialized.NameValueCollection
            Dim moConfig As System.Collections.Specialized.NameValueCollection
            Dim mcPriceModOrder As String
            Dim mcUnitModOrder As String
            Dim mcModuleName As String = "Eonic.Discount"

            Dim bIsCartOn As Boolean = False
            Dim bIsQuoteOn As Boolean = False
            Public myWeb As Protean.Cms

            Dim myCart As Cart
            Dim mbRoundUp As Boolean
            Dim mbRoundDown As Boolean
            Dim mcGroups As String

            Public mcCurrency As String
            Public bHasPromotionalDiscounts As Boolean = False

            Private cPromotionalDiscounts As String = ","
            Private cVouchersUsed As String = ","

            Public Enum promoCodeType
                Open = 0
                SingleCode = 1 'was "12N"
                UseOnce = 2 'was "121"
                MultiCode = 3 'to be implemented later using tblCode
            End Enum

            Public Sub New(ByRef aWeb As Protean.Cms)
                PerfMon.Log("Discount", "New")
                Try
                    myWeb = aWeb
                    moConfig = myWeb.moConfig
                    moCartConfig = WebConfigurationManager.GetWebApplicationSection("protean/cart")
                    mcGroups = getGroupsByName()

                    If myWeb.HasSession Then
                        If myWeb.moSession("mcCurrency") = "" Then
                            'NB 19th Feb 2010 - Caused Consultant Portal to fall over here without these
                            'additional checks?
                            If Not moCartConfig Is Nothing Then
                                If Not moCartConfig("currency") Is Nothing Then
                                    myWeb.moSession("mcCurrency") = moCartConfig("currency")
                                End If
                            End If
                        End If
                        If myWeb.moSession("mcCurrency") = "" Then myWeb.moSession("mcCurrency") = "GBP"

                        mcCurrency = myWeb.moSession("mcCurrency")
                    End If

                    If Not moCartConfig Is Nothing Then
                        mbRoundUp = (LCase(moCartConfig("Roundup")) = "yes" Or LCase(moCartConfig("Roundup")) = "on")
                        mbRoundDown = IIf(LCase(moCartConfig("Roundup")) = "down", True, False)
                        mcPriceModOrder = moCartConfig("PriceModOrder")
                        mcUnitModOrder = moCartConfig("UnitModOrder")
                    End If

                    bIsCartOn = LCase(moConfig("Cart")) = "on"
                    bIsQuoteOn = LCase(moConfig("Quote")) = "on"

                    mcModuleName = "Eonic.Discount"
                    PerfMon.Log("Discount", "New-End")
                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "New", ex, "", "", gbDebug)
                End Try
            End Sub


            Public Sub New(ByRef aCart As Protean.Cms.Cart)
                PerfMon.Log("Discount", "New")
                Try
                    myWeb = aCart.myWeb
                    myCart = aCart
                    moConfig = myWeb.moConfig
                    moCartConfig = WebConfigurationManager.GetWebApplicationSection("protean/cart")

                    mbRoundUp = myCart.mbRoundup
                    mbRoundDown = myCart.mbRoundDown

                    If LCase(moConfig("Cart")) = "on" Then
                        bIsCartOn = True
                    End If

                    If LCase(moConfig("Quote")) = "on" Then
                        bIsQuoteOn = True
                    End If

                    If Not moCartConfig Is Nothing Then
                        mcPriceModOrder = moCartConfig("PriceModOrder")
                        mcUnitModOrder = moCartConfig("UnitModOrder")
                    End If

                    mcModuleName = "Eonic.Discount"
                    PerfMon.Log("Discount", "New-End")

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "New", ex, "", "", gbDebug)
                End Try
            End Sub


#Region "Discount Application"
            Function CheckDiscounts(ByVal oDsCart As DataSet, ByVal oCartXML As XmlElement, ByVal bFullCart As Boolean, ByVal oNotesElmt As XmlElement) As Decimal
                PerfMon.Log("Discount", "CheckDiscounts")
                If Not bIsCartOn And Not bIsQuoteOn Then Return 0
                Dim oDsDiscounts As DataSet
                'Dim cSQL As String
                'Dim sUserSql As String
                Dim strSQL As New Text.StringBuilder
                'Dim oDr As DataRow

                Dim DiscountApplyDate As DateTime = Now()
                'TS we should add logic here to get the invoiceDate from the xml if it exists. then we can apply historic discounts by refreshing the cartxml.


                Dim sSQLArr() As String = Nothing

                Dim nCount As Integer
                Dim dDisountAmount As Double = 0
                Dim xmlCartItem As XmlElement


                Try
                    Dim cUserGroupIds As String = getUserGroupIDs() 'get the user groups
                    Dim cPromoCodeUserEntered As String = getUserEnteredPromoCode(oNotesElmt, oCartXML) 'Check for promotional codes 

                    'get cart contentIds
                    Dim strItemIds As New Text.StringBuilder
                    For Each xmlCartItem In oCartXML.SelectNodes("Item")
                        strItemIds.Append(xmlCartItem.GetAttribute("contentId"))
                        strItemIds.Append(",")
                    Next
                    Dim cCartItemIds As String = strItemIds.ToString


                    If cCartItemIds <> "" Then
                        ''comment----------
                        '' we selecting all discounts applicable to all Items of the cart.
                        '' we passing through the cart item ids at any promocode that has been entered
                        '' we need to include all discount rules applicable to products by virtue of ther group memebership
                        '' we also needs to include all discount rules that applied to all products after checking they are not in a excluded group
                        '' we return the cart item id and discount that applied to it.
                        '' we may have multiple discount per product 
                        '' In future we may have multiple discount code , right now we can apply one per order.
                        ''
                        If Not (oCartXML.Attributes("InvoiceDateTime") Is Nothing) Then
                            DiscountApplyDate = oCartXML.Attributes("InvoiceDateTime").Value
                        End If






                        cCartItemIds = cCartItemIds.Remove(cCartItemIds.Length - 1)
                        If myWeb.moDbHelper.checkTableColumnExists("tblCartDiscountRules", "bAllProductExcludeGroups") Then
                            '' call stored procedure else existing code.
                            '' Passing parameter: cPromoCodeUserEntered,DiscountApplyDate,cUserGroupIds,nCartId
                            Dim param As New Hashtable
                            param.Add("PromoCodeEntered", cPromoCodeUserEntered)
                            param.Add("UserGroupIds", cUserGroupIds)
                            param.Add("CartOrderId", myCart.mnCartId)
                            param.Add("CartOrderDate", DiscountApplyDate)
                            oDsDiscounts = myWeb.moDbHelper.GetDataSet("spCheckDiscounts", "Discount", "Discounts", False, param, CommandType.StoredProcedure)

                        Else


                            'get the SQL together
                            strSQL.Append("SELECT tblCartDiscountRules.nDiscountKey, tblCartDiscountRules.nDiscountForeignRef, tblCartDiscountRules.cDiscountName, ")
                            strSQL.Append("tblCartDiscountRules.cDiscountCode, tblCartDiscountRules.bDiscountIsPercent, ")
                            strSQL.Append("tblCartDiscountRules.nDiscountCompoundBehaviour, tblCartDiscountRules.nDiscountValue, ")
                            strSQL.Append("tblCartDiscountRules.nDiscountMinPrice, tblCartDiscountRules.nDiscountMinQuantity, ")
                            strSQL.Append(" tblCartDiscountRules.nDiscountCat, tblCartDiscountRules.cAdditionalXML, tblCartDiscountRules.nAuditId, ")
                            strSQL.Append("tblCartCatProductRelations.nContentId, ")
                            strSQL.Append("tblCartDiscountRules.nDiscountCodeType, ")
                            strSQL.Append("tblCartDiscountRules.cDiscountUserCode, ")
                            strSQL.Append("ci.nCartItemKey ")
                            If cPromoCodeUserEntered <> "" Then
                                strSQL.Append(", dbo.fxn_checkDiscountCode(tblCartDiscountRules.nDiscountKey, '" & cPromoCodeUserEntered & "') as [CodeUsedId] ")
                            End If
                            strSQL.Append("FROM tblCartCatProductRelations ")
                            strSQL.Append("INNER JOIN tblCartDiscountProdCatRelations ON tblCartCatProductRelations.nCatId = tblCartDiscountProdCatRelations.nProductCatId ")
                            strSQL.Append("INNER JOIN tblCartDiscountRules ")
                            strSQL.Append("INNER JOIN tblCartDiscountDirRelations ON tblCartDiscountRules.nDiscountKey = tblCartDiscountDirRelations.nDiscountId ")
                            strSQL.Append("INNER JOIN tblAudit ON tblCartDiscountRules.nAuditId = tblAudit.nAuditKey ON tblCartDiscountProdCatRelations.nDiscountId = tblCartDiscountRules.nDiscountKey ")
                            strSQL.Append("INNER JOIN tblCartItem ci ON ci.nItemId = tblCartCatProductRelations.nContentId and ci.nCartOrderId = " & myCart.mnCartId)
                            strSQL.Append("WHERE (tblAudit.nStatus = 1) ")
                            strSQL.Append("AND (tblAudit.dExpireDate IS NULL OR tblAudit.dExpireDate >= " & sqlDate(DiscountApplyDate) & ")  ")
                            strSQL.Append("AND (tblAudit.dPublishDate IS NULL OR tblAudit.dPublishDate <= " & sqlDate(DiscountApplyDate) & ") ")
                            strSQL.Append("AND (tblCartDiscountDirRelations.nDirId IN (" & cUserGroupIds & ")) ")

                            If myWeb.moDbHelper.checkTableColumnExists("tblCartDiscountDirRelations", "nPermLevel") Then
                                'code to exclude denied discounts
                                strSQL.Append("AND (SELECT COUNT(dr2.nDiscountDirRelationKey) from tblCartDiscountDirRelations dr2" &
                                            " WHERE dr2.nDirId IN (" & cUserGroupIds & ")" &
                                             " AND nDiscountKey = dr2.nDiscountId" &
                                             " AND  dr2.nPermLevel = 0 ) = 0 ")
                            End If


                            strSQL.Append("AND (tblCartCatProductRelations.nContentId IN (" & cCartItemIds & ")) ")


                            '   If LCase(myCart.mcCartCmd) = "discounts" Or LCase(myCart.mcCartCmd) = "notes" Then
                            'return all
                            If cPromoCodeUserEntered <> "" Then
                                strSQL.Append("AND ((tblCartDiscountRules.cDiscountUserCode = '" & cPromoCodeUserEntered & "' and  tblCartDiscountRules.nDiscountCodeType IN (1,2))")
                                strSQL.Append("OR (tblCartDiscountRules.nDiscountCodeType = 3  and dbo.fxn_checkDiscountCode(tblCartDiscountRules.nDiscountKey, '" & cPromoCodeUserEntered & "') > 0))")
                            End If

                            '   Else
                            'strSQL.Append("AND ((tblCartDiscountRules.cDiscountUserCode = '' AND  tblCartDiscountRules.nDiscountCodeType = 0) ")
                            'If cPromoCodeUserEntered <> "" Then
                            'strSQL.Append("OR (tblCartDiscountRules.cDiscountUserCode = '" & cPromoCodeUserEntered & "' and  tblCartDiscountRules.nDiscountCodeType IN (1,2))")
                            '            strSQL.Append("OR (tblCartDiscountRules.nDiscountCodeType = 3  and dbo.fxn_checkDiscountCode(tblCartDiscountRules.nDiscountKey, '" & cPromoCodeUserEntered & "') > 0)")
                            ' End If
                            'strSQL.Append(")")
                            'End If

                            PerfMon.Log("Discount", "CheckDiscounts - StartQuery")
                            oDsDiscounts = myWeb.moDbHelper.GetDataSet(strSQL.ToString, "Discount", "Discounts")
                            PerfMon.Log("Discount", "CheckDiscounts - EndQuery")
                        End If

                        'TS: Add a union in here to add discount rule applied at an order level.



                        If oDsDiscounts Is Nothing Then
                            If cPromoCodeUserEntered <> "" Then
                                Dim oDiscountMessage As XmlElement = oCartXML.OwnerDocument.CreateElement("DiscountMessage")
                                oDiscountMessage.InnerXml = "<span class=""msg-1030"">The code you have provided is invalid for this transaction</span>"

                                oCartXML.AppendChild(oDiscountMessage)
                            End If
                            Return 0
                        Else

                            Dim oDc As DataColumn
                            For Each oDc In oDsDiscounts.Tables("Discount").Columns
                                If Not oDc.ColumnName = "cAdditionalXML" Then oDc.ColumnMapping = MappingType.Attribute
                            Next
                            oDsDiscounts.Tables("Discount").Columns("cAdditionalXML").ColumnMapping = MappingType.SimpleContent
                            'add a copy of the cart items table
                            '------------------------------------------------------------------------------------
                            If Not bFullCart Then
                                'If just a summary then cart xml does not have the items
                                '   add to Cart XML
                                If oDsCart.Tables("Item").Rows.Count > 0 Then
                                    'cart items
                                    oDsCart.Tables(0).Columns("nDiscountKey").ColumnMapping = Data.MappingType.Attribute
                                    oDsCart.Tables(0).Columns("nDiscountForeignRef").ColumnMapping = Data.MappingType.Attribute
                                    oDsCart.Tables(0).Columns(2).ColumnMapping = Data.MappingType.Attribute
                                    'oDsCart.Tables(0).Columns(3).ColumnMapping = Data.MappingType.Attribute
                                    oDsCart.Tables(0).Columns(4).ColumnMapping = Data.MappingType.Attribute
                                    oDsCart.Tables(0).Columns(5).ColumnMapping = Data.MappingType.Attribute
                                    oDsCart.Tables(0).Columns(6).ColumnMapping = Data.MappingType.Attribute
                                    oDsCart.Tables(0).Columns(7).ColumnMapping = Data.MappingType.Attribute
                                    oDsCart.Tables(0).Columns(8).ColumnMapping = Data.MappingType.Attribute

                                    If Not (oDsCart.Tables(0).Columns("CodeUsedId") Is Nothing) Then
                                        oDsCart.Tables(0).Columns("CodeUsedId").ColumnMapping = Data.MappingType.Attribute
                                    End If

                                    'cart contacts

                                    Dim oXml As XmlDataDocument
                                    oXml = New XmlDataDocument(oDsCart)
                                    oDsCart.EnforceConstraints = False

                                    oCartXML.InnerXml = oXml.FirstChild.InnerXml
                                End If
                            End If
                            '------------------------------------------------------------------------------------
                            oDsDiscounts.Tables.Add(oDsCart.Tables("Item").Copy())

                            'remove product's children as they mess with the discounts
                            Dim drItem As DataRow
                            Dim bDataTableChanged As Boolean = False
                            For Each drItem In oDsDiscounts.Tables("Item").Rows
                                If drItem.Item("nParentId") > 0 Then
                                    drItem.Delete()
                                    bDataTableChanged = True
                                End If
                            Next
                            If bDataTableChanged Then
                                oDsDiscounts.Tables("Item").AcceptChanges()
                            End If

                            'relate them
                            oDsDiscounts.Relations.Add("ItemDiscount", oDsDiscounts.Tables("Item").Columns("id"), oDsDiscounts.Tables("Discount").Columns("nCartItemKey"), False)
                            oDsDiscounts.Relations("ItemDiscount").Nested = True
                            'now make it into an xml document
                            Dim oDXML As New XmlDocument
                            Dim cXML As String = Replace(Replace(oDsDiscounts.GetXml, "&gt;", ">"), "&lt;", "<")
                            oDXML.InnerXml = cXML
                            oDXML.PreserveWhitespace = False

                            'now need to make sure there are no duplicates where multi groups exists
                            Dim oItemElmt As XmlElement
                            For Each oItemElmt In oDXML.SelectNodes("Discounts/Item")
                                Dim oDupElmt As XmlElement
                                Dim nDiscConts() As Integer = {0}
                                Dim cDiscConts As String = ","
                                For Each oDupElmt In oItemElmt.SelectNodes("Discount")

                                    If cDiscConts.Contains("," & oDupElmt.GetAttribute("nDiscountKey") & ",") Then
                                        oItemElmt.RemoveChild(oDupElmt)
                                    Else
                                        cDiscConts &= oDupElmt.GetAttribute("nDiscountKey") & ","
                                    End If

                                Next
                            Next

                            'Itterate through those that have a cDiscountUserCode
                            For Each oItemElmt In oDXML.SelectNodes("Discounts/Item/Discount[@cDiscountUserCode!='' or @nDiscountCodeType='3']")
                                bHasPromotionalDiscounts = True

                                Dim cDiscountUserCode As String = oItemElmt.GetAttribute("cDiscountUserCode").ToLower
                                Dim nDiscountCodeType As promoCodeType = oItemElmt.GetAttribute("nDiscountCodeType").ToLower

                                If nDiscountCodeType = promoCodeType.MultiCode Then
                                    If cPromoCodeUserEntered = "" Then
                                        oItemElmt.ParentNode.RemoveChild(oItemElmt)
                                    Else
                                        'do nothing we will process this rule because it matches the incoming query which contains the code.
                                    End If
                                Else
                                    If Not cDiscountUserCode = cPromoCodeUserEntered.ToLower Then
                                        oItemElmt.ParentNode.RemoveChild(oItemElmt)
                                    Else
                                        If nDiscountCodeType = promoCodeType.UseOnce Then
                                            If Not cPromotionalDiscounts.Contains("," & oItemElmt.GetAttribute("nDiscountKey") & ",") Then
                                                cPromotionalDiscounts &= oItemElmt.GetAttribute("nDiscountKey") & ","
                                            End If
                                        End If
                                    End If
                                End If

                            Next

                            For Each oItemElmt In oDXML.SelectNodes("Discounts/Item/Discount[@CodeUsedId!='']")

                                cVouchersUsed &= oItemElmt.GetAttribute("CodeUsedId") & ","

                            Next

                            'Price Modifiers
                            Dim cPriceModifiers() As String = {"Basic_Money", "Basic_Percent", "Break_Product"}

                            If Not mcPriceModOrder = "" Then cPriceModifiers = Split(mcPriceModOrder, ",")
                            Dim strcFreeShippingMethods As String = ""
                            Dim strbFreeGiftBox As String = ""

                            If Not (oDsDiscounts) Is Nothing Then
                                strcFreeShippingMethods = ""
                                Dim doc As New XmlDocument()
                                Dim ProductGroups As Boolean = 1

                                If (cPromoCodeUserEntered <> "") Then
                                    'getting productgroups value
                                    strSQL.Clear()
                                    strSQL.Append("Select cAdditionalXML From tblCartDiscountRules Where cDiscountUserCode = '" & cPromoCodeUserEntered & "'")

                                    oDsDiscounts = myWeb.moDbHelper.GetDataSet(strSQL.ToString, "Discount", "Discounts")
                                    If oDsDiscounts.Tables("Discount").Rows.Count > 0 Then
                                        Dim additionalInfo As String = "<additionalXml>" + oDsDiscounts.Tables("Discount").Rows(0)("cAdditionalXML") + "</additionalXml>"
                                        doc.LoadXml(additionalInfo)

                                        If (doc.InnerXml.Contains("cFreeShippingMethods")) Then
                                            strcFreeShippingMethods = doc.SelectSingleNode("additionalXml").SelectSingleNode("cFreeShippingMethods").InnerText
                                        End If
                                        If (doc.InnerXml.Contains("bFreeGiftBox")) Then
                                            strbFreeGiftBox = doc.SelectSingleNode("additionalXml").SelectSingleNode("bFreeGiftBox").InnerText
                                        End If
                                    End If

                                End If

                            End If


                            Dim nPriceCount As Integer = 0 'this counts where we are on the prices, shows the order we done them in
                            For nCount = 0 To UBound(cPriceModifiers)
                                Select Case cPriceModifiers(nCount)
                                    Case "Basic_Money"
                                        Discount_Basic_Money(oDXML, nPriceCount, strcFreeShippingMethods, strbFreeGiftBox)
                                    Case "Basic_Percent"
                                        Discount_Basic_Percent(oDXML, nPriceCount, strcFreeShippingMethods)
                                    Case "Break_Product"
                                        Discount_Break_Product(oDXML, nPriceCount)
                                End Select
                            Next
                            'these  need to be ordered since they are dependant
                            'on each other
                            Discount_XForPriceY(oDXML, nPriceCount)
                            Discount_CheapestDiscount(oDXML, nPriceCount)
                            Discount_Break_Group(oDXML, nPriceCount)
                            Dim nTotalSaved As Decimal = Discount_ApplyToCart(oCartXML, oDXML)

                            If Not bFullCart Then oCartXML.InnerXml = ""

                            Return nTotalSaved
                        End If
                        ''code to validate exchange functionality
                    ElseIf (cCartItemIds = "" And cPromoCodeUserEntered <> String.Empty) Then

                        strSQL.Append(" SELECT tblCartDiscountRules.cDiscountCode, tblCartDiscountRules.bDiscountIsPercent, ")
                        strSQL.Append("tblCartDiscountRules.nDiscountCompoundBehaviour, tblCartDiscountRules.nDiscountValue from tblCartDiscountRules where cDiscountCode='" + cPromoCodeUserEntered + "'")
                        oDsDiscounts = myWeb.moDbHelper.GetDataSet(strSQL.ToString, "Discount", "Discounts")


                        dDisountAmount = oDsDiscounts.Tables("Discount").Rows(0)("nDiscountValue")
                        Return dDisountAmount
                    Else

                        Return 0
                    End If

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "CheckDiscounts", ex, "", "", gbDebug)
                End Try
            End Function


            Private Function getUserEnteredPromoCode(ByRef xmlNotes As XmlElement, ByRef xmlCart As XmlElement) As String

                Dim cPromotionalCode As String = ""

                'try to get it from promocodeFromExternalRef in cart xml
                cPromotionalCode = xmlCart.GetAttribute("promocodeFromExternalRef")

                'try to get it from request
                If cPromotionalCode = "" Then
                    cPromotionalCode = myWeb.moRequest("promocode")
                End If

                'try to get it from cart xml
                If cPromotionalCode = "" Then
                    Dim oPromoElmt As XmlElement = xmlNotes.SelectSingleNode("//Notes/PromotionalCode")
                    If Not oPromoElmt Is Nothing Then cPromotionalCode = oPromoElmt.InnerText
                End If

                'try to get it from cart xform
                If cPromotionalCode = "" Then
                    'Dim oXform As xForm = New xForm
                    'oXform.moPageXML = myWeb.moPageXml
                    cPromotionalCode = myWeb.moRequest("//Notes/PromotionalCode")
                    'If Not oXform.Instance.SelectSingleNode("descendant-or-self::PromotionalCode") Is Nothing Then
                    '    cPromotionalCode = oXform.Instance.SelectSingleNode("descendant-or-self::PromotionalCode").InnerText
                    'End If
                End If


                Return "" & cPromotionalCode


            End Function

            Private Function getUserGroupIDs() As String

                Dim nNonAuthenticatedUsersGroupId As Integer
                Dim nAuthenticatedUsersGroupId As Integer
                Dim nAllsersGroupId As Integer = 0
                Dim strGroupIds As New Text.StringBuilder
                nNonAuthenticatedUsersGroupId = CInt("0" & myWeb.moConfig("NonAuthenticatedUsersGroupId"))
                nAuthenticatedUsersGroupId = CInt("0" & myWeb.moConfig("AuthenticatedUsersGroupId"))

                'start by adding the all user group, 0
                strGroupIds.Append(nAllsersGroupId.ToString)


                If myWeb.mnUserId > 0 Then
                    'user  logged in
                    Dim xNodeList As XmlNodeList
                    Dim xmlRole As XmlElement
                    strGroupIds.Append(",")
                    'add Authenticated User Group 
                    strGroupIds.Append(nAuthenticatedUsersGroupId.ToString)

                    Dim xUserXml As XmlElement = myWeb.moPageXml.SelectSingleNode("Page/User")
                    If xUserXml Is Nothing Then
                        xUserXml = myWeb.GetUserXML(myWeb.mnUserId)
                    End If
                    'get user's groups
                    xNodeList = xUserXml.SelectNodes("Group")
                    For Each xmlRole In xNodeList
                        strGroupIds.Append(",")
                        strGroupIds.Append(xmlRole.GetAttribute("id"))
                    Next

                Else
                    'user not logged in
                    strGroupIds.Append(",")
                    strGroupIds.Append(nNonAuthenticatedUsersGroupId.ToString)
                End If

                Return strGroupIds.ToString

            End Function

            Public Function Discount_ApplyToCart(ByRef oCartXML As XmlElement, ByVal oDiscountXml As XmlDocument) As Decimal
                PerfMon.Log("Discount", "Discount_ApplyToCart")
                Try
                    'for basic we need to loop through and apply the new price
                    'also link the discounts applied
                    Dim oPriceElmt As XmlElement
                    'for price
                    Dim oDiscountElmt As XmlElement
                    'for units
                    Dim oDiscountItemTest As XmlElement
                    Dim nDelIDs(0) As Integer
                    'the item
                    Dim oItemElmt As XmlElement
                    Dim nTotalSaved As Decimal = 0

                    For Each oItemElmt In oCartXML.SelectNodes("Item")
                        'Item ID
                        Dim nLineTotalSaving As Decimal
                        Dim nId As Integer = oItemElmt.GetAttribute("id")
                        oPriceElmt = oDiscountXml.SelectSingleNode("Discounts/Item[@id=" & nId & "]/DiscountPrice")
                        If Not oPriceElmt Is Nothing Then

                            'NB 16/02/2010 added rounding
                            Dim nNewUnitPrice As Decimal = Round(oPriceElmt.GetAttribute("UnitPrice"), , , mbRoundUp)
                            Dim nNewTotal As Decimal = oPriceElmt.GetAttribute("Total")
                            Dim nUnitSaving As Decimal = oPriceElmt.GetAttribute("UnitSaving")
                            nLineTotalSaving = oPriceElmt.GetAttribute("TotalSaving")


                            'set up new attreibutes and change price
                            oItemElmt.SetAttribute("id", nId)
                            'NB 16/02/2010 added rounding
                            oItemElmt.SetAttribute("originalPrice", Round(oItemElmt.GetAttribute("price"), , , mbRoundUp))
                            oItemElmt.SetAttribute("price", nNewUnitPrice)
                            oItemElmt.SetAttribute("unitSaving", nUnitSaving)
                            oItemElmt.SetAttribute("itemSaving", nLineTotalSaving)
                            oItemElmt.SetAttribute("itemTotal", nNewTotal)

                            'this will change
                            nTotalSaved += nLineTotalSaving



                            'now to add the discount items to the cart
                            For Each oDiscountElmt In oDiscountXml.SelectNodes("Discounts/Item[@id=" & nId & "]/Discount[((@nDiscountCat=1 or @nDiscountCat=2) and @Applied=1) or (@nDiscountCat=3)]")
                                oDiscountElmt.SetAttribute("AppliedToCart", 1)
                                oItemElmt.AppendChild(oItemElmt.OwnerDocument.ImportNode(oDiscountElmt.CloneNode(True), True))
                            Next
                            Dim oTmpElmt As XmlElement = oDiscountXml.SelectSingleNode("Discounts/Item[@id=" & nId & "]/DiscountPrice")
                            oItemElmt.AppendChild(oItemElmt.OwnerDocument.ImportNode(oTmpElmt.CloneNode(True), True))
                        End If

                        'That was the Price Modifiers done, now for the unit modifiers (not group yet)
                        'TS: I don't really understand this next bit, if anyone does can you document it better.

                        For Each oDiscountItemTest In oDiscountXml.SelectNodes("Discounts/Item[@id=" & nId & "]/DiscountItem")
                            If CDec(oDiscountItemTest.GetAttribute("TotalSaving")) <= CDec(oItemElmt.GetAttribute("itemTotal")) Then
                                oDiscountItemTest.SetAttribute("AppliedToCart", 1)
                                oItemElmt.AppendChild(oItemElmt.OwnerDocument.ImportNode(oDiscountItemTest.CloneNode(True), True))
                                Dim oDiscountInfo As XmlElement = oDiscountXml.SelectSingleNode("Discounts/Item[@id=" & nId & "]/Discount[@nDiscountKey=" & oDiscountItemTest.GetAttribute("nDiscountKey") & "]")
                                If Not oDiscountInfo Is Nothing Then
                                    oItemElmt.AppendChild(oItemElmt.OwnerDocument.ImportNode(oDiscountInfo.CloneNode(True), True))
                                End If
                                oItemElmt.SetAttribute("itemTotal", oItemElmt.GetAttribute("itemTotal") - oDiscountItemTest.GetAttribute("TotalSaving"))
                                nLineTotalSaving += CDec(oDiscountItemTest.GetAttribute("TotalSaving"))
                                nTotalSaved += CDec(oDiscountItemTest.GetAttribute("TotalSaving"))
                            Else
                                If IsNumeric(oDiscountItemTest.GetAttribute("nDiscountCat")) Then
                                    If oDiscountItemTest.GetAttribute("nDiscountCat") = 4 Then
                                        If nDelIDs(0) = 0 Then
                                            nDelIDs(0) = oDiscountItemTest.GetAttribute("nDiscountKey")
                                        Else
                                            ReDim Preserve nDelIDs(UBound(nDelIDs) + 1)
                                            nDelIDs(UBound(nDelIDs)) = oDiscountItemTest.GetAttribute("nDiscountKey")
                                        End If

                                    Else

                                        'Discount greater than quanity on this line.... should give discount overall

                                        oDiscountItemTest.SetAttribute("AppliedToCart", 1)
                                        oItemElmt.AppendChild(oItemElmt.OwnerDocument.ImportNode(oDiscountItemTest.CloneNode(True), True))
                                        Dim oDiscountInfo As XmlElement = oDiscountXml.SelectSingleNode("Discounts/Item[@id=" & nId & "]/Discount[@nDiscountKey=" & oDiscountItemTest.GetAttribute("nDiscountKey") & "]")
                                        If Not oDiscountInfo Is Nothing Then
                                            oItemElmt.AppendChild(oItemElmt.OwnerDocument.ImportNode(oDiscountInfo.CloneNode(True), True))
                                        End If
                                        oItemElmt.SetAttribute("itemTotal", oItemElmt.GetAttribute("itemTotal") - oDiscountItemTest.GetAttribute("TotalSaving"))
                                        nLineTotalSaving += CDec(oDiscountItemTest.GetAttribute("TotalSaving"))
                                        nTotalSaved += CDec(oDiscountItemTest.GetAttribute("TotalSaving"))
                                    End If


                                End If
                            End If
                        Next

                        'also need to save the total discount in the cart
                        'TS added in April 09
                        Dim cUpdtSQL As String = "UPDATE tblCartItem SET nDiscountValue = " & nLineTotalSaving & " WHERE nCartItemKey = " & nId
                        myWeb.moDbHelper.ExeProcessSql(cUpdtSQL)

                    Next
                    'End If


                    If Not nDelIDs(0) = 0 Then
                        Dim nIX As Integer
                        For nIX = 0 To UBound(nDelIDs)
                            Dim nDelElmt As XmlElement
                            For Each nDelElmt In oCartXML.SelectNodes("descendant-or-self::DiscountItem[@nDiscountKey=" & nDelIDs(nIX) & "] | descendant-or-self::Discount[@nDiscountKey=" & nDelIDs(nIX) & "]")
                                nDelElmt.ParentNode.RemoveChild(nDelElmt)
                            Next
                        Next
                    End If


                    Return nTotalSaved
                    'Thats all folks!
                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "Discount_ApplyToCart", ex, "", "", gbDebug)
                End Try
            End Function
#End Region

#Region "Discount Rule Application"

            Private Sub Discount_Basic_Money(ByRef oDiscountXML As XmlDocument, ByRef nPriceCount As Integer, ByRef cFreeShippingMethods As String, Optional ByRef strbFreeGiftBox As String = "")
                PerfMon.Log("Discount", "Discount_Basic_Money")
                'this will work basic monetary discounts
                Dim oItemLoop As XmlElement
                Dim oDiscountLoop As XmlElement
                Dim oPriceElmt As XmlElement
                Dim bApplyOnTotal As Boolean = False
                Dim RemainingAmountToDiscount As Double = 0
                Try
                    'loop through the items
                    For Each oItemLoop In oDiscountXML.SelectNodes("Discounts/Item")
                        'check for promotional codes

                        'look for new price element, if not one, create one
                        oPriceElmt = oItemLoop.SelectSingleNode("Item/DiscountPrice")
                        If oPriceElmt Is Nothing Then
                            'NB 16/02/2010
                            'Time to pull price out so we can round it, to avoid the multiple decimal place issues
                            Dim nPrice As Decimal
                            nPrice = Round((oItemLoop.GetAttribute("price")), , , mbRoundUp)

                            'set default attributes
                            oPriceElmt = oDiscountXML.CreateElement("DiscountPrice")
                            oPriceElmt.SetAttribute("OriginalUnitPrice", nPrice)
                            'oPriceElmt.SetAttribute("OriginalUnitPrice", oItemLoop.GetAttribute("price"))
                            oPriceElmt.SetAttribute("UnitPrice", nPrice)
                            'oPriceElmt.SetAttribute("UnitPrice", oItemLoop.GetAttribute("price"))
                            oPriceElmt.SetAttribute("Units", oItemLoop.GetAttribute("quantity"))
                            oPriceElmt.SetAttribute("Total", nPrice * oItemLoop.GetAttribute("quantity"))
                            'oPriceElmt.SetAttribute("Total", oItemLoop.GetAttribute("price") * oItemLoop.GetAttribute("quantity"))
                            oPriceElmt.SetAttribute("UnitSaving", 0)
                            oPriceElmt.SetAttribute("TotalSaving", 0)
                            oItemLoop.AppendChild(oPriceElmt)
                        End If
                        'loop through the basic money discounts'
                        For Each oDiscountLoop In oItemLoop.SelectNodes("Discount[@bDiscountIsPercent=0 and @nDiscountCat=1 and not(@Applied='1')]")
                            'now work out new unit prices etc

                            Dim nNewPrice As Decimal = oPriceElmt.GetAttribute("UnitPrice")
                            Dim AmountToDiscount As Decimal = oDiscountLoop.GetAttribute("nDiscountValue")
                            If oDiscountLoop.GetAttribute("nDiscountRemaining") <> "" Then
                                AmountToDiscount = oDiscountLoop.GetAttribute("nDiscountRemaining")
                            End If
                            nNewPrice = nNewPrice - (AmountToDiscount / oItemLoop.GetAttribute("quantity"))

                            If nNewPrice > 0 And bApplyOnTotal = False Then 'only apply it if its not gonna go below 0

                                Dim oPriceLine As XmlElement = oDiscountXML.CreateElement("DiscountPriceLine")

                                'this works the price out for this discount based on previous stuff
                                nPriceCount += 1
                                oPriceLine.SetAttribute("PriceOrder", nPriceCount)
                                oPriceLine.SetAttribute("nDiscountKey", oDiscountLoop.GetAttribute("nDiscountKey"))
                                oPriceLine.SetAttribute("UnitPrice", nNewPrice)
                                oPriceLine.SetAttribute("Total", nNewPrice * oPriceElmt.GetAttribute("Units"))
                                oPriceLine.SetAttribute("UnitSaving", oPriceElmt.GetAttribute("UnitPrice") - nNewPrice)
                                oPriceLine.SetAttribute("TotalSaving", oPriceElmt.GetAttribute("UnitSaving") * oPriceElmt.GetAttribute("Units"))

                                oPriceElmt.AppendChild(oPriceLine)

                                'this works the overall price
                                oPriceElmt.SetAttribute("UnitPrice", nNewPrice)
                                oPriceElmt.SetAttribute("Total", nNewPrice * oPriceElmt.GetAttribute("Units"))
                                oPriceElmt.SetAttribute("UnitSaving", oPriceElmt.GetAttribute("OriginalUnitPrice") - nNewPrice)
                                oPriceElmt.SetAttribute("TotalSaving", oPriceElmt.GetAttribute("UnitSaving") * oPriceElmt.GetAttribute("Units"))

                                'we will always apply these
                                Dim oDiscountElmt As XmlElement
                                For Each oDiscountElmt In oDiscountXML.SelectNodes("Discounts/Item/Discount[@nDiscountKey=" & oDiscountLoop.GetAttribute("nDiscountKey") & "]")

                                    'set shipping option after applied promocode
                                    If (cFreeShippingMethods <> "") Then
                                        myCart.updateGCgetValidShippingOptionsDS(cFreeShippingMethods)
                                    End If
                                    If (oDiscountLoop.SelectSingleNode("bApplyToOrder") IsNot Nothing) Then
                                        If (oDiscountLoop.SelectSingleNode("bApplyToOrder").InnerText.ToString() = "True") Then
                                            oDiscountElmt.SetAttribute("Applied", 1)
                                            If (AmountToDiscount = 0) Then
                                                bApplyOnTotal = True
                                            Else
                                                bApplyOnTotal = False
                                            End If
                                        End If
                                    End If
                                Next

                                'if apply on total is true in discount rule, set flag to true
                                'which will skip flag status to true.
                            Else

                                nNewPrice = 0

                                Dim oPriceLine As XmlElement = oDiscountXML.CreateElement("DiscountPriceLine")
                                nPriceCount += 1
                                oPriceLine.SetAttribute("PriceOrder", nPriceCount)
                                oPriceLine.SetAttribute("nDiscountKey", oDiscountLoop.GetAttribute("nDiscountKey"))
                                oPriceLine.SetAttribute("UnitPrice", nNewPrice)
                                oPriceLine.SetAttribute("Total", nNewPrice * oPriceElmt.GetAttribute("Units"))
                                oPriceLine.SetAttribute("UnitSaving", oPriceElmt.GetAttribute("UnitPrice"))
                                oPriceLine.SetAttribute("TotalSaving", oPriceElmt.GetAttribute("UnitPrice") * oPriceElmt.GetAttribute("Units"))

                                oPriceElmt.AppendChild(oPriceLine)

                                'this works the overall price
                                oPriceElmt.SetAttribute("UnitPrice", nNewPrice)
                                oPriceElmt.SetAttribute("Total", nNewPrice * oPriceElmt.GetAttribute("Units"))
                                oPriceElmt.SetAttribute("UnitSaving", oPriceElmt.GetAttribute("OriginalUnitPrice") - nNewPrice)
                                oPriceElmt.SetAttribute("TotalSaving", oPriceElmt.GetAttribute("UnitSaving") * oPriceElmt.GetAttribute("Units"))

                                'we will always apply these
                                oDiscountLoop.SetAttribute("Applied", 1)
                                RemainingAmountToDiscount = RemainingAmountToDiscount + oPriceLine.GetAttribute("TotalSaving")
                                Dim oDiscountElmt As XmlElement
                                'set the discount remianing if this rule is available on other products..
                                For Each oDiscountElmt In oDiscountXML.SelectNodes("Discounts/Item/Discount[@nDiscountKey=" & oDiscountLoop.GetAttribute("nDiscountKey") & "]")
                                    ''oDiscountElmt.SetAttribute("nDiscountRemaining", oDiscountLoop.GetAttribute("nDiscountValue") - oPriceLine.GetAttribute("TotalSaving"))

                                    If (oDiscountLoop.SelectSingleNode("bApplyToOrder") IsNot Nothing) Then
                                        If (oDiscountLoop.SelectSingleNode("bApplyToOrder").InnerText.ToString() = "True") Then
                                            If (AmountToDiscount = 0) Then
                                                bApplyOnTotal = True
                                            Else

                                                bApplyOnTotal = False
                                                oDiscountElmt.SetAttribute("nDiscountRemaining", oDiscountLoop.GetAttribute("nDiscountValue") - RemainingAmountToDiscount)
                                            End If
                                        End If
                                    Else
                                        oDiscountElmt.SetAttribute("nDiscountRemaining", oDiscountLoop.GetAttribute("nDiscountValue") - oPriceLine.GetAttribute("TotalSaving"))
                                    End If
                                Next
                            End If
                        Next


                        'set packaging option to giftbox after applied promocode
                        If (strbFreeGiftBox <> "" And oItemLoop.SelectSingleNode("Discount") IsNot Nothing) Then
                            myCart.updatePackagingForFreeGiftDiscount(oItemLoop.Attributes("id").Value)
                        End If
                    Next
                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "Discount_Basic_Money", ex, "", "", gbDebug)
                End Try
            End Sub

            Private Sub Discount_Break_Product(ByRef oDiscountXML As XmlDocument, ByRef nPriceCount As Integer)
                PerfMon.Log("Discount", "Discount_Break_Product")
                'this will work basic monetary discounts
                Dim oItemLoop As XmlElement
                Dim oPriceElmt As XmlElement
                Try
                    'loop through the items
                    For Each oItemLoop In oDiscountXML.SelectNodes("Discounts/Item")
                        'look for new price element, if not one, create one
                        oPriceElmt = oItemLoop.SelectSingleNode("DiscountPrice")
                        If oPriceElmt Is Nothing Then
                            'NB 16/02/2010
                            'Time to pull price out so we can round it, to avoid the multiple decimal place issues
                            Dim nPrice As Decimal
                            nPrice = Round((oItemLoop.GetAttribute("price")), , , mbRoundUp)

                            'set default attributes
                            oPriceElmt = oDiscountXML.CreateElement("DiscountPrice")
                            oPriceElmt.SetAttribute("OriginalUnitPrice", nPrice)
                            'oPriceElmt.SetAttribute("OriginalUnitPrice", oItemLoop.GetAttribute("price"))
                            oPriceElmt.SetAttribute("UnitPrice", nPrice)
                            'oPriceElmt.SetAttribute("UnitPrice", oItemLoop.GetAttribute("price"))
                            oPriceElmt.SetAttribute("Units", oItemLoop.GetAttribute("quantity"))
                            oPriceElmt.SetAttribute("Total", nPrice * oItemLoop.GetAttribute("quantity"))
                            'oPriceElmt.SetAttribute("Total", oItemLoop.GetAttribute("price") * oItemLoop.GetAttribute("quantity"))
                            oPriceElmt.SetAttribute("UnitSaving", 0)
                            oPriceElmt.SetAttribute("TotalSaving", 0)
                            oItemLoop.AppendChild(oPriceElmt)
                        End If

                        'Need to loop through all the breaks and get highest break
                        Dim oTmpLoop As XmlElement
                        Dim oPriceBreakElmt As XmlElement = Nothing
                        Dim oQuantityBreakElmt As XmlElement = Nothing
                        For Each oTmpLoop In oItemLoop.SelectNodes("Discount[@nDiscountCat=2]")
                            'here we go through and find the biggest discount
                            If Not oPriceBreakElmt Is Nothing Then
                                If oTmpLoop.GetAttribute("nDiscountMinPrice") <= oPriceElmt.GetAttribute("Total") _
                                And oTmpLoop.GetAttribute("nDiscountMinPrice") > oPriceBreakElmt.GetAttribute("nDiscountMinPrice") _
                                Then oPriceBreakElmt = oTmpLoop
                            Else
                                If IsNumeric(oTmpLoop.GetAttribute("nDiscountMinPrice")) And IsNumeric(oPriceElmt.GetAttribute("Total")) Then
                                    If CDec(oTmpLoop.GetAttribute("nDiscountMinPrice")) <= CDec(oPriceElmt.GetAttribute("Total")) Then
                                        oPriceBreakElmt = oTmpLoop
                                    End If
                                End If
                            End If

                            If Not oQuantityBreakElmt Is Nothing Then
                                If oTmpLoop.GetAttribute("nDiscountMinQuantity") <= oPriceElmt.GetAttribute("Units") _
                                And oTmpLoop.GetAttribute("nDiscountMinQuantity") > oQuantityBreakElmt.GetAttribute("nDiscountMinQuantity") _
                                Then oQuantityBreakElmt = oTmpLoop
                            Else
                                If IsNumeric(oTmpLoop.GetAttribute("nDiscountMinQuantity")) And oTmpLoop.GetAttribute("nDiscountMinQuantity") <= oPriceElmt.GetAttribute("Units") Then oQuantityBreakElmt = oTmpLoop
                            End If
                        Next

                        'which is going to be the bigger discount
                        Dim oTestElmt As XmlElement = Nothing
                        Dim oHighestElmt As XmlElement = Nothing
                        Dim nCurrentSaving As Decimal = 0

                        Dim nUnits As Long = oPriceElmt.GetAttribute("Units")
                        Dim nUnitPrice As Decimal = 0
                        Dim nTotal As Decimal = 0
                        Dim nUnitSaving As Decimal = 0
                        Dim nTotalSaving As Decimal = 0


                        Dim nTestStatus As Integer = 0 '0=Price, 1=Quantity
ResumeTest:
                        If nTestStatus = 2 Then GoTo FinishTest
                        If nTestStatus = 1 Then oTestElmt = oQuantityBreakElmt
                        If nTestStatus = 0 Then oTestElmt = oPriceBreakElmt
                        If oTestElmt Is Nothing Then GoTo NextTestLoop
                        'now the actual test
                        If Not oTestElmt Is Nothing Then
                            nUnitPrice = oPriceElmt.GetAttribute("UnitPrice")
                            'work out depending on value/percent
                            If oTestElmt.GetAttribute("bDiscountIsPercent") = 0 Then
                                nUnitPrice -= oTestElmt.GetAttribute("nDiscountValue")
                            Else
                                nUnitPrice = Round(nUnitPrice * ((100 - oTestElmt.GetAttribute("nDiscountValue")) / 100), , , mbRoundUp)
                            End If
                            'make the totals
                            nTotal = nUnitPrice * nUnits
                            nUnitSaving = oPriceElmt.GetAttribute("UnitPrice") - nUnitPrice
                            nTotalSaving = nUnitSaving * oPriceElmt.GetAttribute("Units")
                            'if its higher than current we make that the item to use
                            If nTotalSaving > nCurrentSaving Then
                                nCurrentSaving = nTotalSaving
                                oHighestElmt = oTestElmt
                            End If
                        End If
NextTestLoop:
                        nTestStatus += 1
                        GoTo ResumeTest
FinishTest:
                        If oHighestElmt Is Nothing Then GoTo NoDiscount
                        'Dim nNewPrice As Decimal = oPriceElmt.GetAttribute("UnitPrice")
                        'nNewPrice = nNewPrice - oHighestElmt.GetAttribute("nDiscountValue")
                        'If nNewPrice > 0 Then 'only apply it if its not gonna go below 0
                        Dim oPriceLine As XmlElement = oDiscountXML.CreateElement("DiscountPriceLine")
                        'this works the overall price
                        oPriceElmt.SetAttribute("UnitPrice", nUnitPrice)
                        oPriceElmt.SetAttribute("Total", nUnitPrice * nUnits)
                        oPriceElmt.SetAttribute("UnitSaving", oPriceElmt.GetAttribute("OriginalUnitPrice") - nUnitPrice)
                        oPriceElmt.SetAttribute("TotalSaving", oPriceElmt.GetAttribute("UnitSaving") * oPriceElmt.GetAttribute("Units"))
                        'this works the price out for this discount based on previous stuff
                        nPriceCount += 1
                        oPriceLine.SetAttribute("PriceOrder", nPriceCount)
                        oPriceLine.SetAttribute("nDiscountKey", oHighestElmt.GetAttribute("nDiscountKey"))
                        oPriceLine.SetAttribute("UnitPrice", nUnitPrice)
                        oPriceLine.SetAttribute("Total", nUnitPrice * nUnits)
                        oPriceLine.SetAttribute("UnitSaving", oPriceElmt.GetAttribute("OriginalUnitPrice") - nUnitPrice)
                        oPriceLine.SetAttribute("TotalSaving", oPriceElmt.GetAttribute("UnitSaving") * oPriceElmt.GetAttribute("Units"))

                        oPriceElmt.AppendChild(oPriceLine)
                        'we will always apply these
                        oHighestElmt.SetAttribute("Applied", 1)
                        'End If
NoDiscount:
                    Next
                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "Discount_Basic_Money", ex, "", "", gbDebug)
                End Try
            End Sub

            Private Sub Discount_Basic_Percent(ByRef oDiscountXML As XmlDocument, ByRef nPriceCount As Integer, ByRef cFreeShippingMethods As String)
                PerfMon.Log("Discount", "Discount_Basic_Percent")
                'this will work basic discount discounts
                Dim oItemLoop As XmlElement
                Dim oDiscountLoop As XmlElement
                Dim oPriceElmt As XmlElement
                Dim nPromocodeApplyFlag As Integer = 0
                Try
                    For Each oItemLoop In oDiscountXML.SelectNodes("Discounts/Item")

                        oPriceElmt = oItemLoop.SelectSingleNode("DiscountPrice")
                        If oPriceElmt Is Nothing Then
                            'NB 16/02/2010
                            'Time to pull price out so we can round it, to avoid the multiple decimal place issues
                            Dim nPrice As Decimal
                            nPrice = Round((oItemLoop.GetAttribute("price")), , , mbRoundUp)

                            oPriceElmt = oDiscountXML.CreateElement("Item/DiscountPrice")
                            oPriceElmt.SetAttribute("OriginalUnitPrice", nPrice)
                            'oPriceElmt.SetAttribute("OriginalUnitPrice", oItemLoop.GetAttribute("price"))
                            oPriceElmt.SetAttribute("UnitPrice", nPrice)
                            'oPriceElmt.SetAttribute("UnitPrice", oItemLoop.GetAttribute("price"))
                            oPriceElmt.SetAttribute("Units", oItemLoop.GetAttribute("quantity"))
                            oPriceElmt.SetAttribute("Total", nPrice * oItemLoop.GetAttribute("quantity"))
                            'oPriceElmt.SetAttribute("Total", oItemLoop.GetAttribute("price") * oItemLoop.GetAttribute("quantity"))
                            oPriceElmt.SetAttribute("UnitSaving", 0)
                            oPriceElmt.SetAttribute("TotalSaving", 0)
                            oItemLoop.AppendChild(oPriceElmt)
                        End If
                        For Each oDiscountLoop In oItemLoop.SelectNodes("Discount[@bDiscountIsPercent=1 and @nDiscountCat=1]")

                            Dim nNewPrice As Decimal = oPriceElmt.GetAttribute("UnitPrice")
                            nNewPrice = Round(nNewPrice * ((100 - oDiscountLoop.GetAttribute("nDiscountValue")) / 100), , , mbRoundUp)

                            Dim oPriceLine As XmlElement = oDiscountXML.CreateElement("DiscountPriceLine")

                            nPriceCount += 1
                            oPriceLine.SetAttribute("nDiscountKey", oDiscountLoop.GetAttribute("nDiscountKey"))
                            oPriceLine.SetAttribute("PriceOrder", nPriceCount)
                            oPriceLine.SetAttribute("UnitPrice", nNewPrice)
                            oPriceLine.SetAttribute("Total", nNewPrice * oPriceElmt.GetAttribute("Units"))
                            oPriceLine.SetAttribute("UnitSaving", oPriceElmt.GetAttribute("UnitPrice") - nNewPrice)
                            oPriceLine.SetAttribute("TotalSaving", oPriceElmt.GetAttribute("UnitSaving") * oPriceElmt.GetAttribute("Units"))
                            oPriceElmt.AppendChild(oPriceLine)

                            'this works the overall price

                            oPriceElmt.SetAttribute("UnitPrice", nNewPrice)
                            oPriceElmt.SetAttribute("Total", nNewPrice * oPriceElmt.GetAttribute("Units"))
                            oPriceElmt.SetAttribute("UnitSaving", oPriceElmt.GetAttribute("OriginalUnitPrice") - nNewPrice)
                            oPriceElmt.SetAttribute("TotalSaving", oPriceElmt.GetAttribute("UnitSaving") * oPriceElmt.GetAttribute("Units"))

                            oDiscountLoop.SetAttribute("Applied", 1)
                            nPromocodeApplyFlag = 1
                        Next
                    Next
                    'set shipping option after applying promocode
                    If (cFreeShippingMethods <> "" And nPromocodeApplyFlag = 1) Then
                        myCart.updateGCgetValidShippingOptionsDS(cFreeShippingMethods)
                    End If

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "Discount_Basic_Percent", ex, "", "", gbDebug)
                End Try
            End Sub

            Private Sub Discount_XForPriceY(ByRef oDiscountXML As XmlDocument, ByRef nPriceCount As Integer)
                PerfMon.Log("Discount", "Discount_XForPriceY")
                'this will work basic discount discounts
                Dim oItemLoop As XmlElement
                Dim oDiscountLoop As XmlElement
                Dim oPriceElmt As XmlElement
                Try
                    For Each oItemLoop In oDiscountXML.SelectNodes("Discounts/Item")
                        oPriceElmt = oItemLoop.SelectSingleNode("DiscountPrice")
                        If oPriceElmt Is Nothing Then
                            'NB 16/02/2010
                            'Time to pull price out so we can round it, to avoid the multiple decimal place issues
                            Dim nPrice As Decimal
                            nPrice = Round((oItemLoop.GetAttribute("price")), , , mbRoundUp)

                            oPriceElmt = oDiscountXML.CreateElement("Item/DiscountPrice")
                            oPriceElmt.SetAttribute("OriginalUnitPrice", nPrice)
                            'oPriceElmt.SetAttribute("OriginalUnitPrice", oItemLoop.GetAttribute("price"))
                            oPriceElmt.SetAttribute("UnitPrice", nPrice)
                            'oPriceElmt.SetAttribute("UnitPrice", oItemLoop.GetAttribute("price"))
                            oPriceElmt.SetAttribute("Units", oItemLoop.GetAttribute("quantity"))
                            oPriceElmt.SetAttribute("Total", nPrice * oItemLoop.GetAttribute("quantity"))
                            'oPriceElmt.SetAttribute("Total", oItemLoop.GetAttribute("price") * oItemLoop.GetAttribute("quantity"))
                            oPriceElmt.SetAttribute("UnitSaving", 0)
                            oPriceElmt.SetAttribute("TotalSaving", 0)
                            oItemLoop.AppendChild(oPriceElmt)
                        End If

                        For Each oDiscountLoop In oItemLoop.SelectNodes("Discount[@nDiscountCat=3]")
                            Dim nQX As Integer 'Quantity buying
                            Dim nQY As Integer 'For the price of
                            Dim nTotalQOff As Integer 'total number of units we need to deduct
                            Dim nQ As Integer 'Total Quanity

                            nQX = oDiscountLoop.GetAttribute("nDiscountMinQuantity")
                            nQY = oDiscountLoop.GetAttribute("nDiscountValue")
                            nQ = oItemLoop.GetAttribute("quantity")
                            Dim nQtotal As Integer = nQ
                            Dim ItemId As String = oItemLoop.GetAttribute("contentId")

                            'calculate quanties of other items with same ID in the cart, BUT ONLY THE LAST ONE... SO WE APPLY THE DISCOUNT ON THE LAST ITEM
                            ' If oItemLoop.SelectNodes("./preceding-sibling::Item[@contentId='" & ItemId & "']").Count > 0 And oItemLoop.SelectNodes("./following-sibling::Item[@contentId='" & ItemId & "']").Count = 0 Then
                            If oItemLoop.SelectNodes("./preceding-sibling::Item[@contentId='" & ItemId & "' and Discount[@nDiscountCat=3 and not(@Applied='1')]]").Count > 0 Then
                                Dim preceedingItems As XmlElement
                                For Each preceedingItems In oItemLoop.SelectNodes("./preceding-sibling::Item[@contentId='" & ItemId & "' and Discount[@nDiscountCat=3 and not(@Applied='1')]]")
                                    nQtotal = nQtotal + preceedingItems.GetAttribute("quantity")
                                Next
                            End If

                            nTotalQOff = (Split(nQtotal / nQX, ".")(0)) * (nQX - nQY)
                            If nTotalQOff > 0 Then
                                Dim oDiscount As XmlElement = oDiscountXML.CreateElement("DiscountItem")
                                oDiscount.SetAttribute("nDiscountKey", oDiscountLoop.GetAttribute("nDiscountKey")) 'ID
                                oDiscount.SetAttribute("nDiscountCat", 3)
                                oDiscount.SetAttribute("oldUnits", oPriceElmt.GetAttribute("Units")) 'Original Charged Units
                                oDiscount.SetAttribute("Units", nQ - nTotalQOff) 'Now charged Units
                                oDiscount.SetAttribute("oldTotal", oPriceElmt.GetAttribute("Total")) 'original total
                                oDiscount.SetAttribute("Total", oPriceElmt.GetAttribute("UnitPrice") * (nQ - nTotalQOff)) 'Now total
                                oDiscount.SetAttribute("TotalSaving", oPriceElmt.GetAttribute("Total") - (oPriceElmt.GetAttribute("UnitPrice") * (nQ - nTotalQOff))) 'total saving

                                oItemLoop.AppendChild(oDiscount)
                                Dim preceedingItems As XmlElement
                                For Each preceedingItems In oItemLoop.SelectNodes("./preceding-sibling::Item[@contentId='" & ItemId & "' and Discount[@nDiscountCat=3 and not(@Applied='1')]]")
                                    'set previous items as applied...
                                    Dim oDiscountloop2 As XmlElement
                                    For Each oDiscountloop2 In preceedingItems.SelectNodes("Discount[@nDiscountCat=3]")
                                        oDiscountloop2.SetAttribute("Applied", 1)
                                    Next
                                Next
                                oDiscountLoop.SetAttribute("Applied", 1)
                            End If
                        Next
                    Next
                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "Discount_xForPriceY", ex, "", "", gbDebug)
                End Try
            End Sub

            Private Sub Discount_CheapestDiscount(ByRef oDiscXml As XmlDocument, ByRef nPriceCount As Integer)

                PerfMon.Log("Discount", "Discount_CheapestFree")
                'this is going to be the wierdest one
                'we will need to loop through discounts first, then the items
                Dim oItemLoop As XmlElement = Nothing
                Dim oDiscountLoop As XmlElement = Nothing
                Dim oPriceElmt As XmlElement = Nothing
                Dim oDiscount As XmlElement = Nothing
                Dim oCurDiscount As XmlElement = Nothing
                Try
                    'need to record all the discount Ids
                    Dim cIDs As String = ","
                    For Each oDiscountLoop In oDiscXml.SelectNodes("descendant-or-self::Discount[@nDiscountCat='4']")
                        'For Each oDiscountLoop In oDiscountXML.SelectNodes("descendant-or-self::Discount[@type=4]")
                        If Not cIDs.Contains("," & oDiscountLoop.GetAttribute("nDiscountKey") & ",") Then
                            cIDs &= oDiscountLoop.GetAttribute("nDiscountKey") & ","
                        End If
                    Next
                    'now we have all the keys we can go though them and get the information we need
                    If cIDs = "," Then Exit Sub
                    cIDs = Right(cIDs, cIDs.Length - 1)
                    cIDs = Left(cIDs, cIDs.Length - 1)
                    Dim oIDs() As String = cIDs.Split(",")

                    Dim nMinItems As Integer = 1
                    Dim nDiscountMaxPrice As Decimal = 0
                    Dim nDiscountMinPrice As Decimal = 0


                    Dim nI As Integer

                    'Step through each individual discount rule
                    For nI = 0 To UBound(oIDs)
                        Dim nCheapestPrice As Decimal = 0 'records the cheapest price
                        Dim oCheapestItem As XmlElement = Nothing 'records the cheapest item
                        Dim oAllItems(0) As XmlElement 'gets all the items so we dont have to do the same loop
                        Dim nValueOfItems As Decimal = 0 'records the total value of the items to check
                        Dim nItemQty As Integer = 0
                        Dim nQualifyingItemsQty As Integer = 0
                        Dim nDiscountValue As Decimal
                        Dim bDiscountIsPercent As Boolean


                        Dim i As Integer = 0

                        oCurDiscount = oDiscXml.SelectSingleNode("Discounts/Item/Discount[@nDiscountKey=" & oIDs(nI) & "]")
                        nDiscountValue = oCurDiscount.GetAttribute("nDiscountValue")
                        bDiscountIsPercent = oCurDiscount.GetAttribute("bDiscountIsPercent")
                        'Set nMinItems
                        If IsNumeric(oCurDiscount.GetAttribute("nDiscountMinQuantity")) Then nMinItems = oCurDiscount.GetAttribute("nDiscountMinQuantity")
                        'Set nDiscountMinPrice
                        If IsNumeric(oCurDiscount.GetAttribute("nDiscountMinPrice")) Then nDiscountMinPrice = oCurDiscount.GetAttribute("nDiscountMinPrice")
                        'Set nDiscountMaxPrice
                        If IsNumeric(oCurDiscount.SelectSingleNode("nDiscountMaxPrice").InnerText) Then nDiscountMaxPrice = oCurDiscount.SelectSingleNode("nDiscountMaxPrice").InnerText

                        Dim aPriceArray(oDiscXml.SelectNodes("Discounts/Item[Discount/@nDiscountKey=" & oIDs(nI) & "]").Count - 1) As Double
                        'Step through each product in the cart associated with current rule
                        For Each oItemLoop In oDiscXml.SelectNodes("Discounts/Item[Discount/@nDiscountKey=" & oIDs(nI) & "]")

                            Dim bAllowedDiscount As Boolean = False
                            'Dim nCurrentUnitPrice As Decimal = oItemLoop.GetAttribute("price")

                            'NB 16/02/2010
                            'Time to pull price out so we can round it, to avoid the multiple decimal place issues
                            Dim nCurrentUnitPrice As Decimal
                            nCurrentUnitPrice = Round((oItemLoop.GetAttribute("price")), , , mbRoundUp)


                            oPriceElmt = oItemLoop.SelectSingleNode("DiscountPrice")
                            If oPriceElmt Is Nothing Then
                                oPriceElmt = oDiscXml.CreateElement("Item/DiscountPrice")
                                oPriceElmt.SetAttribute("discountId", oIDs(nI))
                                oPriceElmt.SetAttribute("OriginalUnitPrice", nCurrentUnitPrice)
                                oPriceElmt.SetAttribute("UnitPrice", nCurrentUnitPrice)
                                oPriceElmt.SetAttribute("Units", oItemLoop.GetAttribute("quantity"))
                                oPriceElmt.SetAttribute("Total", nCurrentUnitPrice * oItemLoop.GetAttribute("quantity"))
                                'oPriceElmt.SetAttribute("Total", oItemLoop.GetAttribute("price") * oItemLoop.GetAttribute("quantity"))
                                oPriceElmt.SetAttribute("UnitSaving", 0)
                                oPriceElmt.SetAttribute("TotalSaving", 0)
                                oItemLoop.AppendChild(oPriceElmt)
                            Else
                                nCurrentUnitPrice = oPriceElmt.GetAttribute("UnitPrice")
                            End If

                            'calculate the total value of items associated with this discount
                            nValueOfItems += nCurrentUnitPrice * oPriceElmt.GetAttribute("Units")
                            nItemQty += oPriceElmt.GetAttribute("Units")

                            If nCurrentUnitPrice > nDiscountMinPrice Then
                                nQualifyingItemsQty += oPriceElmt.GetAttribute("Units")
                            End If
                            'create an array of prices to discount that we can sort
                            aPriceArray(i) = nCurrentUnitPrice
                            i = i + 1
                            'add each item into an array for sorting by price.
                        Next

                        ' check that anything is below value
                        If Not aPriceArray Is Nothing Then
                            'sort the prices to discount
                            Array.Sort(aPriceArray)
                            Dim itemsToDiscount As Integer = Math.Floor(nQualifyingItemsQty / nMinItems)

                            Dim nLastPrice As Decimal = 0

                            For i = 0 To UBound(aPriceArray)
                                'step through prices cheapest first
                                If aPriceArray(i) > nLastPrice And aPriceArray(i) <= nDiscountMaxPrice And itemsToDiscount > 0 Then
                                    For Each oItemLoop In oDiscXml.SelectNodes("Discounts/Item[DiscountPrice/@UnitPrice=" & aPriceArray(i) & "]")

                                        nLastPrice = oPriceElmt.GetAttribute("UnitPrice")
                                        oPriceElmt = oItemLoop.SelectSingleNode("DiscountPrice")


                                        Dim nDiscountNumber As Long
                                        If oPriceElmt.GetAttribute("Units") > itemsToDiscount Then
                                            nDiscountNumber = itemsToDiscount
                                        Else
                                            nDiscountNumber = oPriceElmt.GetAttribute("Units")
                                        End If

                                        oDiscount = oDiscXml.CreateElement("DiscountItem")
                                        itemsToDiscount = itemsToDiscount - nDiscountNumber

                                        'define the by Unit Discounted Price
                                        Dim nUnitPriceOriginal As Decimal = oPriceElmt.GetAttribute("UnitPrice")
                                        Dim nUnitPriceDiscounted As Decimal
                                        If bDiscountIsPercent Then
                                            nUnitPriceDiscounted = nUnitPriceOriginal * ((100 - nDiscountValue) / 100)
                                        Else
                                            If nUnitPriceOriginal >= nDiscountValue Then
                                                nUnitPriceDiscounted = nUnitPriceOriginal - nDiscountValue
                                            Else
                                                nUnitPriceDiscounted = 0
                                            End If
                                        End If



                                        'nUnitPriceDiscounted = Math.Round(nUnitPriceDiscounted, 2)
                                        nUnitPriceDiscounted = Round(nUnitPriceDiscounted, , , mbRoundUp)

                                        Dim nUnitCount As Integer = oPriceElmt.GetAttribute("Units")

                                        If nDiscountNumber > 0 Then

                                            Dim nTotalSaving As Decimal = (nUnitPriceOriginal - nUnitPriceDiscounted) * nDiscountNumber
                                            oDiscount.SetAttribute("nDiscountKey", oIDs(nI)) 'ID
                                            oDiscount.SetAttribute("nDiscountCat", 4)
                                            oDiscount.SetAttribute("oldUnits", nUnitCount) 'Original Charged Units
                                            If nDiscountValue = 100 Then
                                                oDiscount.SetAttribute("Units", nUnitCount - nDiscountNumber) 'Now charged Units
                                            Else
                                                oDiscount.SetAttribute("Units", nUnitCount) 'Now charged Units
                                            End If

                                            oDiscount.SetAttribute("oldTotal", oPriceElmt.GetAttribute("Total")) 'original total
                                            oDiscount.SetAttribute("Total", oPriceElmt.GetAttribute("Total") - nTotalSaving) 'Now total
                                            oDiscount.SetAttribute("TotalSaving", nTotalSaving) 'total saving

                                        Else
                                            oDiscount.SetAttribute("nDiscountKey", oIDs(nI)) 'ID
                                            oDiscount.SetAttribute("nDiscountCat", 4)
                                            oDiscount.SetAttribute("oldUnits", nUnitCount) 'Original Charged Units
                                            oDiscount.SetAttribute("Units", nUnitCount) 'Now charged Units
                                            oDiscount.SetAttribute("oldTotal", oPriceElmt.GetAttribute("Total")) 'original total
                                            oDiscount.SetAttribute("Total", nUnitPriceOriginal * nUnitCount) 'Now total
                                            oDiscount.SetAttribute("TotalSaving", 0) 'total saving
                                        End If
                                        oItemLoop.AppendChild(oDiscount)
                                    Next
                                End If
                            Next
                        End If
                    Next
                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "Discount_CheapestFree", ex, "", "", gbDebug)
                End Try

            End Sub

            Private Sub Discount_Break_Group(ByRef oDiscountXML As XmlDocument, ByRef nPriceCount As Integer)
                PerfMon.Log("Discount", "Discount_Break_Group")
                Try
                    Dim oDiscount As XmlElement
                    Dim nTotalItems As Integer
                    Dim nTotalItemsValue As Decimal
                    Dim oPriceElmt As XmlElement
                    Dim oItem As XmlElement

                    Dim cIDs As String = ","
                    For Each oDiscount In oDiscountXML.SelectNodes("descendant-or-self::Discount[@nDiscountCat=5]")
                        If Not cIDs.Contains("," & oDiscount.GetAttribute("nDiscountKey") & ",") Then
                            cIDs &= oDiscount.GetAttribute("nDiscountKey") & ","
                        End If
                    Next
                    If cIDs = "," Then Exit Sub
                    cIDs = Right(cIDs, cIDs.Length - 1)
                    cIDs = Left(cIDs, cIDs.Length - 1)
                    Dim oIds() As String = Split(cIDs, ",")
                    Dim i As Integer = 0
                    For i = 0 To UBound(oIds)
                        For Each oDiscount In oDiscountXML.SelectNodes("descendant-or-self::Discount[@nDiscountKey=" & oIds(i) & "]")
                            For Each oItem In oDiscountXML.DocumentElement.SelectNodes("Item[Discount/@nDiscountKey=" & oDiscount.GetAttribute("nDiscountKey") & "]")
                                oPriceElmt = oItem.SelectSingleNode("DiscountPrice")
                                If oPriceElmt Is Nothing Then
                                    'NB 16/02/2010
                                    'Time to pull price out so we can round it, to avoid the multiple decimal place issues
                                    Dim nPrice As Decimal
                                    nPrice = Round(oItem.GetAttribute("price"), , , mbRoundUp)

                                    oPriceElmt = oDiscountXML.CreateElement("Item/DiscountPrice")
                                    oPriceElmt.SetAttribute("OriginalUnitPrice", nPrice)
                                    'oPriceElmt.SetAttribute("OriginalUnitPrice", oItem.GetAttribute("price"))
                                    oPriceElmt.SetAttribute("UnitPrice", nPrice)
                                    'oPriceElmt.SetAttribute("UnitPrice", oItem.GetAttribute("price"))
                                    oPriceElmt.SetAttribute("Units", oItem.GetAttribute("quantity"))
                                    oPriceElmt.SetAttribute("Total", nPrice * oItem.GetAttribute("quantity"))
                                    'oPriceElmt.SetAttribute("Total", oItem.GetAttribute("price") * oItem.GetAttribute("quantity"))
                                    oPriceElmt.SetAttribute("UnitSaving", 0)
                                    oPriceElmt.SetAttribute("TotalSaving", 0)
                                    oItem.AppendChild(oPriceElmt)
                                End If
                                nTotalItems += oPriceElmt.GetAttribute("Units")
                                nTotalItemsValue += (oPriceElmt.GetAttribute("UnitPrice") * oPriceElmt.GetAttribute("Units"))
                            Next
                            Dim nNewPrice As Decimal = 0


                            Dim nDQ As Integer = 0
                            If IsNumeric(oDiscount.GetAttribute("nDiscountMinQuantity")) Then nDQ = oDiscount.GetAttribute("nDiscountMinQuantity")
                            Dim nDT As Integer = 0
                            If IsNumeric(oDiscount.GetAttribute("nDiscountMinPrice")) Then nDT = oDiscount.GetAttribute("nDiscountMinPrice")
                            If (nTotalItems >= nDQ And nDQ > 0) Or (nTotalItemsValue >= nDT And nDT > 0) Then
                                Dim nDiscountedSoFar As Integer = 0
                                For Each oItem In oDiscountXML.DocumentElement.SelectNodes("Item[Discount/@nDiscountKey=" & oDiscount.GetAttribute("nDiscountKey") & "]")
                                    'if its a percentage we can just discount them all since its a flat rate.
                                    oPriceElmt = oItem.SelectSingleNode("DiscountPrice")

                                    If oDiscount.GetAttribute("bDiscountIsPercent") = 1 Then
                                        nNewPrice = Round((oPriceElmt.GetAttribute("UnitPrice") / 100) * (100 - oDiscount.GetAttribute("nDiscountValue")), , , mbRoundUp)
                                    Else
                                        nNewPrice = oPriceElmt.GetAttribute("UnitPrice") - oDiscount.GetAttribute("nDiscountValue")
                                    End If

                                    Dim oPriceLine As XmlElement = oDiscountXML.CreateElement("DiscountPriceLine")
                                    nPriceCount += 1
                                    oPriceLine.SetAttribute("nDiscountKey", oDiscount.GetAttribute("nDiscountKey"))
                                    oPriceLine.SetAttribute("PriceOrder", nPriceCount)
                                    oPriceLine.SetAttribute("UnitPrice", nNewPrice)
                                    oPriceLine.SetAttribute("Total", nNewPrice * oPriceElmt.GetAttribute("Units"))
                                    oPriceLine.SetAttribute("UnitSaving", oPriceElmt.GetAttribute("UnitPrice") - nNewPrice)
                                    oPriceLine.SetAttribute("TotalSaving", oPriceElmt.GetAttribute("UnitSaving") * oPriceElmt.GetAttribute("Units"))
                                    oPriceElmt.AppendChild(oPriceLine)

                                    'this works the overall price

                                    oPriceElmt.SetAttribute("UnitPrice", nNewPrice)
                                    oPriceElmt.SetAttribute("Total", nNewPrice * oPriceElmt.GetAttribute("Units"))
                                    oPriceElmt.SetAttribute("UnitSaving", oPriceElmt.GetAttribute("OriginalUnitPrice") - nNewPrice)
                                    oPriceElmt.SetAttribute("TotalSaving", oPriceElmt.GetAttribute("UnitSaving") * oPriceElmt.GetAttribute("Units"))
                                Next
                            End If
                            Exit For
                        Next
                        nTotalItems = 0
                        nTotalItemsValue = 0
                    Next
                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "Discount_Break Group", ex, "", "", gbDebug)
                End Try
            End Sub

#End Region

#Region "Promotional codes"
            Public Sub DisablePromotionalDiscounts()
                Try
                    If Not cPromotionalDiscounts = "," Then
                        Dim oIdArr() As String = Split(cPromotionalDiscounts, ",")
                        Dim i As Integer
                        For i = 0 To UBound(oIdArr)
                            If Not oIdArr(i) = "" Then
                                myWeb.moDbHelper.setObjectStatus(dbHelper.objectTypes.CartDiscountRules, dbHelper.Status.Hidden, oIdArr(i))
                            End If
                        Next
                    End If

                    If Not cVouchersUsed = "," Then
                        Dim oIdArr() As String = Split(cVouchersUsed, ",")
                        Dim i As Integer
                        For i = 0 To UBound(oIdArr)
                            If Not oIdArr(i) = "" Then
                                myWeb.moDbHelper.UseCode(CInt(oIdArr(i)), myWeb.mnUserId, myCart.mnCartId)

                            End If
                        Next
                    End If

                Catch ex As Exception

                End Try
            End Sub
#End Region

#Region "Content Procedures"

            Public Function AddDiscountCode(ByVal sCode As String) As String
                Dim cProcessInfo As String
                Dim sSql As String
                Dim strSQL As New Text.StringBuilder
                Dim oDs As DataSet
                Dim oRow As DataRow
                Dim sXmlContent As String
                Dim docOrder As New XmlDocument()
                Dim DiscountApplyDate As DateTime = Now()
                Dim oDsDiscounts As DataSet
                Dim doc As New XmlDocument()
                Dim oDiscountMessage As String = "The promo code you have provided is invalid for this transaction"
                Dim minimumOrderTotal As Double = 0
                Dim maximumOrderTotal As Double = 0
                Dim productGroups As Int32 = 0

                Dim applyToTotal As Boolean = False

                Dim cUserGroupIds As String = getUserGroupIDs() 'get the user groups
                Try
                    If myCart.mnCartId > 0 Then
                        sSql = "select * from tblCartOrder where nCartOrderKey=" & myCart.mnCartId
                        oDs = myWeb.moDbHelper.getDataSetForUpdate(sSql, "Order", "Cart")
                        sXmlContent = oDs.Tables(0).Rows(0)("cCartXml") & ""
                        docOrder.LoadXml(sXmlContent)

                        Dim orderTotal As Double = docOrder.SelectSingleNode("Order").Attributes("total").Value

                        If myWeb.moDbHelper.checkTableColumnExists("tblCartDiscountRules", "bAllProductExcludeGroups") Then
                            '' call stored procedure else existing code.
                            '' Passing parameter: cPromoCodeUserEntered,DiscountApplyDate,cUserGroupIds,nCartId
                            Dim param As New Hashtable
                            param.Add("PromoCodeEntered", sCode)
                            param.Add("UserGroupIds", cUserGroupIds)
                            param.Add("CartOrderId", myCart.mnCartId)
                            param.Add("CartOrderDate", DiscountApplyDate)
                            oDsDiscounts = myWeb.moDbHelper.GetDataSet("spCheckDiscounts", "Discount", "Discounts", False, param, CommandType.StoredProcedure)

                        Else

                            strSQL.Append("SELECT tblCartDiscountRules.nDiscountKey, tblCartDiscountRules.nDiscountForeignRef, tblCartDiscountRules.cDiscountName,  ")
                            strSQL.Append("tblCartDiscountRules.cDiscountCode, tblCartDiscountRules.bDiscountIsPercent, tblCartDiscountRules.nDiscountCompoundBehaviour,  ")
                            strSQL.Append("tblCartDiscountRules.nDiscountValue, tblCartDiscountRules.nDiscountMinPrice, tblCartDiscountRules.nDiscountMinQuantity,  ")
                            strSQL.Append("  tblCartDiscountRules.nDiscountCat, tblCartDiscountRules.cAdditionalXML, tblCartDiscountRules.nAuditId,  ")
                            strSQL.Append("tblCartDiscountRules.nDiscountCodeType, tblCartDiscountRules.cDiscountUserCode  ")
                            strSQL.Append("FROM tblCartDiscountRules  ")
                            strSQL.Append("INNER JOIN tblAudit ON tblCartDiscountRules.nAuditId = tblAudit.nAuditKey AND (tblAudit.nStatus = 1) ")
                            If sCode <> "" Then
                                strSQL.Append("WHERE tblCartDiscountRules.cDiscountCode= '" & sCode & "'")
                            End If
                            strSQL.Append("AND (tblAudit.dExpireDate IS NULL OR tblAudit.dExpireDate > " & sqlDate(DiscountApplyDate) & ")  ")
                            strSQL.Append("AND (tblAudit.dPublishDate IS NULL OR tblAudit.dPublishDate <= " & sqlDate(DiscountApplyDate) & ") ")


                            oDsDiscounts = myWeb.moDbHelper.GetDataSet(strSQL.ToString, "Discount", "Discounts")
                        End If

                        If oDsDiscounts.Tables(0).Rows.Count = 0 Then
                            If sCode <> "" Then
                                oDsDiscounts.Clear()
                                oDsDiscounts = Nothing
                                Return oDiscountMessage
                            End If

                        Else
                            Dim additionalInfo As String = "<additionalXml>" + oDsDiscounts.Tables("Discount").Rows(0)("cAdditionalXML") + "</additionalXml>"
                            doc.LoadXml(additionalInfo)

                            If (doc.InnerXml.Contains("nMinimumOrderValue")) Then
                                minimumOrderTotal = CDbl("0" & doc.SelectSingleNode("additionalXml").SelectSingleNode("nMinimumOrderValue").InnerText)
                            End If
                            If (doc.InnerXml.Contains("nMaximumOrderValue")) Then
                                maximumOrderTotal = CDbl("0" & doc.SelectSingleNode("additionalXml").SelectSingleNode("nMaximumOrderValue").InnerText)
                            End If

                            If (doc.InnerXml.Contains("bApplyToOrder")) Then
                                If (doc.SelectSingleNode("additionalXml").SelectSingleNode("bApplyToOrder").InnerText = "") Then
                                    applyToTotal = False
                                Else
                                    applyToTotal = Convert.ToBoolean(doc.SelectSingleNode("additionalXml").SelectSingleNode("bApplyToOrder").InnerText)
                                End If
                                If (maximumOrderTotal <> 0) Then
                                    If Not (orderTotal >= minimumOrderTotal And orderTotal <= maximumOrderTotal) Then
                                        oDsDiscounts.Clear()
                                        oDsDiscounts = Nothing
                                        Return oDiscountMessage
                                    End If
                                End If
                                If (applyToTotal) Then
                                    If (maximumOrderTotal <> 0) Then
                                        If Not (orderTotal >= minimumOrderTotal And orderTotal <= maximumOrderTotal) Then
                                            oDsDiscounts.Clear()
                                            oDsDiscounts = Nothing
                                            Return oDiscountMessage
                                        End If
                                    End If
                                End If
                            End If
                            oDsDiscounts.Clear()
                            oDsDiscounts = Nothing
                        End If
                        'myCart.moCartXml

                        For Each oRow In oDs.Tables("Order").Rows

                            'load existing notes from Cart
                            sXmlContent = oRow("cClientNotes") & ""
                            If sXmlContent = "" Then
                                sXmlContent = "<Notes><PromotionalCode/></Notes>"
                            End If
                            Dim NotesXml As New XmlDocument
                            NotesXml.LoadXml(sXmlContent)

                            If NotesXml.SelectSingleNode("Notes") Is Nothing Then
                                Dim notesElement As XmlElement = NotesXml.CreateElement("NoteInfo")
                                notesElement.InnerXml = "<Notes><PromotionalCode/></Notes>"
                                NotesXml.FirstChild.AppendChild(notesElement.FirstChild)
                            End If

                            If Not NotesXml.SelectSingleNode("//Notes/PromotionalCode[node()='" & sCode & "']") Is Nothing Then
                                'do nothing code exists
                            Else
                                If NotesXml.SelectSingleNode("//Notes/PromotionalCode") Is Nothing Then
                                    'add another promotional code
                                    Dim newElmt As XmlElement = NotesXml.CreateElement("PromotionalCode")
                                    NotesXml.SelectSingleNode("//Notes").AppendChild(newElmt)
                                Else
                                    If NotesXml.SelectSingleNode("//Notes/PromotionalCode").InnerText = "" Then
                                        NotesXml.SelectSingleNode("//Notes/PromotionalCode").InnerText = sCode
                                    Else
                                        'add another promotional code
                                        Dim newElmt As XmlElement = NotesXml.CreateElement("PromotionalCode")
                                        NotesXml.SelectSingleNode("//Notes").AppendChild(newElmt)
                                    End If
                                End If
                            End If
                            oRow("cClientNotes") = NotesXml.OuterXml
                        Next
                        myWeb.moDbHelper.updateDataset(oDs, "Order", True)
                        oDs.Clear()
                        oDs = Nothing

                        Return sCode
                    Else

                        Return ""
                    End If
                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "AddDiscountCode", ex, "", cProcessInfo, gbDebug)
                End Try
            End Function

            Public Sub getAvailableDiscounts(ByRef oRootElmt As XmlElement)
                PerfMon.Log("Discount", "getAvailableDiscounts")
                If Not bIsCartOn And Not bIsQuoteOn Then Exit Sub
                'Gets the discounts applicable to the listed products, filtered by user
                'and adds them under the relevant content. Will do this for both Brief and Detail
                Try
                    Dim strSQL As New Text.StringBuilder
                    Dim strItemIds As New Text.StringBuilder
                    Dim cItemIds As String

                    Dim nMode As Integer = 1
                    Dim oDS As DataSet
                    Dim oDiscountElmt As XmlElement
                    Dim oContentElmt As XmlElement

                    Dim cContentTypes As String = moConfig("ProductTypes")
                    If cContentTypes = "" Then cContentTypes = "Product,SKU"
                    Dim cUserGroupIds As String = getUserGroupIDs()

                    'Get the content types that are products with discounts on this site.

                    Dim cContentTypesArray() As String = cContentTypes.Split(",")
                    If cContentTypesArray.Length > 0 Then
                        cContentTypes = "[("
                        Dim i As Integer = 0
                        For Each cContentType As String In cContentTypesArray
                            cContentTypes &= "@type='" + cContentType.Trim() + "'"
                            i = i + 1
                            If i < cContentTypesArray.Length Then
                                cContentTypes &= " or "
                            Else
                                'cContentTypes &= "]"
                                cContentTypes &= ") and not(ancestor::Discounts)]"
                            End If
                        Next
                    Else
                        cContentTypes = ""
                    End If

                    'Get a list of the content ID's for all products on the site
                    For Each oContentElmt In oRootElmt.SelectNodes("descendant-or-self::Content" + cContentTypes)
                        If Not oContentElmt.GetAttribute("id") Is Nothing Then
                            strItemIds.Append(Protean.Tools.Database.SqlString(oContentElmt.GetAttribute("id").Trim()))
                            strItemIds.Append(",")
                        End If
                    Next
                    cItemIds = strItemIds.ToString
                    cItemIds = cItemIds.TrimEnd(",")



                    If Not cItemIds.Equals("") Then

                        strSQL.Append("SELECT tblCartDiscountRules.nDiscountKey as id, ")
                        strSQL.Append("tblCartDiscountRules.bDiscountIsPercent as isPercent, ")
                        strSQL.Append("tblCartDiscountRules.nDiscountValue as value, ")
                        strSQL.Append("tblCartDiscountRules.nDiscountCat as type, ")
                        strSQL.Append("tblCartCatProductRelations.nContentId as nContentId, ")
                        strSQL.Append("tblCartDiscountRules.nDiscountCodeType, ")
                        strSQL.Append("tblCartDiscountRules.cDiscountUserCode, ")
                        strSQL.Append("tblCartDiscountRules.cAdditionalXML ")
                        strSQL.Append("FROM tblCartCatProductRelations ")
                        strSQL.Append("INNER JOIN tblCartDiscountProdCatRelations ON tblCartCatProductRelations.nCatId = tblCartDiscountProdCatRelations.nProductCatId ")
                        strSQL.Append("INNER JOIN tblCartDiscountRules ")
                        strSQL.Append("INNER JOIN tblCartDiscountDirRelations ON tblCartDiscountRules.nDiscountKey = tblCartDiscountDirRelations.nDiscountId ")
                        strSQL.Append("INNER JOIN tblAudit ON tblCartDiscountRules.nAuditId = tblAudit.nAuditKey ON tblCartDiscountProdCatRelations.nDiscountId = tblCartDiscountRules.nDiscountKey ")
                        strSQL.Append("WHERE (tblAudit.nStatus = 1) ")
                        strSQL.Append("AND (tblAudit.dExpireDate IS NULL OR tblAudit.dExpireDate >= GETDATE())  ")
                        strSQL.Append("AND (tblAudit.dPublishDate IS NULL OR tblAudit.dPublishDate <= GETDATE()) ")
                        strSQL.Append("AND (tblCartDiscountDirRelations.nDirId IN (" & cUserGroupIds & ")) ")
                        strSQL.Append("AND (tblCartCatProductRelations.nContentId IN (" & cItemIds & ")) ")
                        strSQL.Append("AND (tblCartDiscountRules.cDiscountUserCode = '' AND  tblCartDiscountRules.nDiscountCodeType = 0) ")


                        'strSQL.Append("SELECT dr.nDiscountKey as id, dr.bDiscountIsPercent as isPercent, dr.nDiscountValue as value, dr.nDiscountCat as type, tblCartCatProductRelations.nContentId, dr.cAdditionalXML ")
                        'strSQL.Append("FROM tblCartCatProductRelations RIGHT OUTER JOIN ")
                        'strSQL.Append("tblCartDiscountProdCatRelations ON tblCartCatProductRelations.nCatId = tblCartDiscountProdCatRelations.nProductCatId RIGHT OUTER JOIN ")
                        'strSQL.Append("tblAudit RIGHT OUTER JOIN ")
                        'strSQL.Append("tblCartDiscountRules dr ON tblAudit.nAuditKey = dr.nAuditId LEFT OUTER JOIN" & _
                        '" tblCartDiscountDirRelations LEFT OUTER JOIN ")
                        'strSQL.Append("tblDirectoryRelation RIGHT OUTER JOIN ")
                        'strSQL.Append("tblDirectory ON tblDirectoryRelation.nDirParentId = tblDirectory.nDirKey ON tblCartDiscountDirRelations.nDirId = tblDirectory.nDirKey ON ")
                        'strSQL.Append("dr.nDiscountKey = tblCartDiscountDirRelations.nDiscountId ON ")
                        'strSQL.Append("tblCartDiscountProdCatRelations.nDiscountId = dr.nDiscountKey ")
                        'strSQL.Append(sSqlWhere & " ")
                        'strSQL.Append("and (tblCartCatProductRelations.nContentId IN (" & cContentIds & ")) ")
                        'strSQL.Append("and tblAudit.nStatus = 1 ")
                        'strSQL.Append("and (tblAudit.dPublishDate is null or tblAudit.dPublishDate = 0 or tblAudit.dPublishDate <= " & Protean.Tools.Database.SqlDate(Today) & " ) ")
                        'strSQL.Append("and (tblAudit.dExpireDate is null or tblAudit.dExpireDate = 0 or tblAudit.dExpireDate >= " & Protean.Tools.Database.SqlDate(Today.AddHours(23).AddMinutes(59).AddSeconds(59), True) & " ) ")
                        'strSQL.Append("AND (tr.cDiscountUserCode = '' AND  tr.nDiscountCodeType = 0) ")



                        PerfMon.Log("Discount", "getAvailableDiscounts-startGetDataset")

                        Dim sSql As String = strSQL.ToString

                        oDS = myWeb.moDbHelper.GetDataSet(sSql, "Discount", "Discounts")
                        PerfMon.Log("Discount", "getAvailableDiscounts-startEndDataset")

                        If oDS.Tables.Count = 0 Then Exit Sub
                        Dim oDC As DataColumn

                        For Each oDC In oDS.Tables("Discount").Columns
                            If Not oDC.ColumnName = "cAdditionalXML" Then oDC.ColumnMapping = MappingType.Attribute
                        Next
                        oDS.Tables("Discount").Columns("cAdditionalXML").ColumnMapping = MappingType.SimpleContent

                        PerfMon.Log("Discount", "getAvailableDiscounts-startGetDatasetXml")
                        Dim oXML As XmlElement = oRootElmt.OwnerDocument.CreateElement("DiscountsRoot")
                        oXML.InnerXml = Replace(Replace(oDS.GetXml, "&gt;", ">"), "&lt;", "<")
                        PerfMon.Log("Discount", "getAvailableDiscounts-endGetDatasetXml")


                        PerfMon.Log("Discount", "getAvailableDiscounts-startIterateContentNodes")

                        'NB : 19-01-2010 - this still runs many checks for non products
                        ' What does all this do? Nothing is updating the price until you buy it
                        ' It should be appending discounts to the content right? Then why isn't it

                        PerfMon.Log("Discount", "getAvailableDiscounts-startAddDiscountsToContent")
                        Dim nStep As Long = 0

                        For Each oDiscountElmt In oXML.SelectNodes("Discounts/Discount[not(descendant-or-self::cPromotionalCode/node()!='') or not(descendant-or-self::cPromotionalCode)]")

                            Dim cContentId As String = oDiscountElmt.GetAttribute("nContentId")

                            If Not String.IsNullOrEmpty(cContentId) Then
                                If IsNumeric(cContentId) Then
                                    Dim nContentId As Long = CType(cContentId, Long)
                                    'For Each oContentElmt In oRootElmt.SelectNodes("descendant-or-self::Content[@id=" & nContentId & "]")
                                    For Each oContentElmt In oRootElmt.SelectNodes("descendant-or-self::Content[@id=" & nContentId & " and not(ancestor::Discounts)]")
                                        'For Each oContentElmt In oRootElmt.SelectNodes("/Page/Contents/descendant-or-self::Content[@id=" & nContentId & "]")
                                        Dim oTmpCheck As XmlElement = oContentElmt.SelectSingleNode("Discount[@id=" & oDiscountElmt.GetAttribute("id") & "]")
                                        If oTmpCheck Is Nothing Then
                                            'TS adjusted so can be added more than once
                                            oContentElmt.AppendChild(oDiscountElmt.CloneNode(True))
                                        End If
                                        nStep = nStep + 1
                                    Next
                                End If
                            End If
                        Next

                        PerfMon.Log("Discount", "getAvailableDiscounts-endAddDiscountsToContent-" & nStep & " Contents with Discounts Added")

                        PerfMon.Log("Discount", "getAvailableDiscounts-startCalculateDiscounts")
                        'For Each oContentElmt In oRootElmt.SelectNodes("/Page/Contents/descendant-or-self::Content")
                        For Each oContentElmt In oRootElmt.SelectNodes("descendant-or-self::Content[Prices/Price[@currency='" & mcCurrency & "' and node()!=''] and Discount]")

                            Dim nContentId As String = oContentElmt.GetAttribute("id")
                            If nContentId = "" Then nContentId = 0

                            ' for showing basic discounts
                            Dim nNewPrice As Decimal = 0

                            Dim cPriceModifiers() As String = {"Basic_Money", "Basic_Percent", "Cheapest_Free"}
                            If Not mcPriceModOrder = "" Then cPriceModifiers = Split(mcPriceModOrder, ",")
                            Dim nI As Integer
                            Dim nPriceCount As Integer = 0
                            'this counts where we are on the prices, shows the order we done them in
                            For nI = 0 To UBound(cPriceModifiers)
                                Select Case cPriceModifiers(nI)
                                    Case "Basic_Money"
                                        For Each oDiscountElmt In oContentElmt.SelectNodes("Discount[@type=1]")
                                            If oDiscountElmt.GetAttribute("isPercent").Equals("0") Then
                                                applyDiscountsToPriceXml(oContentElmt, oDiscountElmt)
                                            End If
                                        Next
                                    Case "Basic_Percent"
                                        For Each oDiscountElmt In oContentElmt.SelectNodes("Discount[@type=1]")
                                            If oDiscountElmt.GetAttribute("isPercent").Equals("1") Then
                                                applyDiscountsToPriceXml(oContentElmt, oDiscountElmt)
                                            End If
                                        Next
                                    Case "Cheapest_Free"
                                        ' Here we want to show the other items available with this offer... Maybe !

                                End Select

                            Next
                        Next
                        PerfMon.Log("Discount", "getAvailableDiscounts-endCalculateDiscounts")
                    End If
                    PerfMon.Log("Discount", "getAvailableDiscounts-endIterateContentNodes")

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "getAvailableDiscounts", ex, "", "", gbDebug)
                End Try
            End Sub

            ''' <summary>
            ''' Returns an XML contain information about all of the discounts on the site.
            ''' Only used to inumberate discounts to display to customers. 
            ''' WE DO NOT WANT TO CALL THIS ON EVERY PAGE as it suffers from poor performance once a lot of discounts are enabled.
            ''' </summary>
            ''' <param name="PageElmt"></param>
            ''' <remarks></remarks>
            Public Overridable Sub getDiscountXML(ByRef PageElmt As XmlElement)
                PerfMon.Log("Discount", "start-getDiscountXML")
                If Not bIsCartOn And Not bIsQuoteOn Then Exit Sub
                Try
                    Dim nDiscountID As Integer = 0
                    If IsNumeric(myWeb.moRequest.QueryString("DiscountID")) Then nDiscountID = myWeb.moRequest.QueryString("DiscountID")
                    Dim sSQL As String
                    Dim oDS As DataSet
                    Dim oDiscounts As XmlElement = PageElmt.OwnerDocument.CreateElement("Discounts")
                    Dim oContent As XmlElement = PageElmt.OwnerDocument.CreateElement("Contents")
                    'get the discounts
                    sSQL = "SELECT dr.nDiscountKey as id, dr.cDiscountName as name, dr.cDiscountCode as code, dr.bDiscountIsPercent as isPercent, dr.nDiscountValue as value, dr.nDiscountMinPrice as minPrice, dr.nDiscountMinQuantity as minQty, dr.nDiscountCat as type, dr.cAdditionalXML" &
                    " FROM tblAudit RIGHT OUTER JOIN" &
                    " tblCartDiscountRules dr ON tblAudit.nAuditKey = dr.nAuditId LEFT OUTER JOIN" &
                    " tblCartDiscountDirRelations LEFT OUTER JOIN" &
                    " tblDirectoryRelation RIGHT OUTER JOIN" &
                    " tblDirectory ON tblDirectoryRelation.nDirParentId = tblDirectory.nDirKey ON tblCartDiscountDirRelations.nDirId = tblDirectory.nDirKey ON " &
                    " dr.nDiscountKey = tblCartDiscountDirRelations.nDiscountId" &
                    " WHERE (tblDirectoryRelation.nDirChildId = " & myWeb.mnUserId & " OR" &
                    " tblDirectoryRelation.nDirChildId IS NULL) AND (tblAudit.nStatus = 1) AND (tblAudit.dPublishDate IS NULL OR" &
                    " tblAudit.dPublishDate = 0 OR" &
                    " tblAudit.dPublishDate <= " & Protean.Tools.Database.SqlDate(Now) & ") AND (tblAudit.dExpireDate IS NULL OR" &
                    " tblAudit.dExpireDate = 0 Or tblAudit.dExpireDate >= " & Protean.Tools.Database.SqlDate(Now) & ")"
                    If nDiscountID > 0 Then
                        sSQL &= " AND dr.nDiscountKey = " & nDiscountID
                    End If
                    oDS = myWeb.moDbHelper.GetDataSet(sSQL, "Discount", "Discounts")
                    PerfMon.Log("Discount", "getDiscountXML-gotDiscounts")
                    Dim oDC As DataColumn
                    For Each oDC In oDS.Tables("Discount").Columns
                        oDC.ColumnMapping = MappingType.Attribute
                    Next
                    oDS.Tables("Discount").Columns("cAdditionalXML").ColumnMapping = MappingType.SimpleContent

                    oDiscounts.InnerXml = Replace(Replace(oDS.GetXml, "&gt;", ">"), "&lt;", "<")
                    oDiscounts = oDiscounts.FirstChild
                    'now the contents

                    ' TS modified to only pull in contents with currently active discounts

                    sSQL = "SELECT c.nContentKey AS id," &
                    " dbo.fxn_getContentParents(c.nContentKey) as parId " &
                    " , c.cContentForiegnRef AS ref, c.cContentName AS name, " &
                                " c.cContentSchemaName AS type, c.cContentXmlBrief AS content, a.nStatus AS status, a.dPublishDate AS publish, a.dExpireDate AS expire, a.nInsertDirId as owner, " &
                                " pcr.nDiscountId" &
                                " FROM tblContent c INNER JOIN" &
                                " tblAudit a ON c.nAuditId = a.nAuditKey INNER JOIN" &
                                " tblCartCatProductRelations ON c.nContentKey = tblCartCatProductRelations.nContentId INNER JOIN" &
                                " tblCartDiscountProdCatRelations pcr ON tblCartCatProductRelations.nCatId = pcr.nProductCatId" &
                                " INNER JOIN tblCartDiscountRules d ON pcr.nDiscountId  = d.nDiscountKey" &
                                " INNER JOIN tblAudit da ON d.nAuditId = da.nAuditKey " &
                                " WHERE (a.nStatus = 1) AND (a.dPublishDate IS NULL OR" &
                                " a.dPublishDate = 0 OR" &
                                " a.dPublishDate <= " & Protean.Tools.Database.SqlDate(Now) & ") AND (a.dExpireDate IS NULL OR" &
                                " a.dExpireDate = 0 OR" &
                                " a.dExpireDate >= " & Protean.Tools.Database.SqlDate(Now) & ")" &
                                " AND (da.nStatus = 1) AND (da.dPublishDate IS NULL OR" &
                                " da.dPublishDate = 0 OR" &
                                " da.dPublishDate <= " & Protean.Tools.Database.SqlDate(Now) & ") AND (da.dExpireDate IS NULL OR" &
                                " da.dExpireDate = 0 OR" &
                                " da.dExpireDate >= " & Protean.Tools.Database.SqlDate(Now) & ")"
                    oDS = New DataSet
                    oDS = myWeb.moDbHelper.GetDataSet(sSQL, "Content", "Contents")
                    PerfMon.Log("Discount", "getDiscountXML-gotContent")
                    oDS.Tables(0).Columns("nDiscountId").ColumnMapping = MappingType.Attribute
                    'now to add them to discounts 

                    myWeb.moDbHelper.AddDataSetToContent(oDS, oContent, myWeb.mnPageId, True, , , , False)

                    PerfMon.Log("Discount", "getDiscountXML-appendStart")

                    Dim oTmp As XmlElement
                    Dim oDisc As XmlElement

                    Dim appendCount As Integer = 0

                    For Each oTmp In oContent.SelectNodes("Content")
                        'oTmp.InnerText = Replace(Replace(oTmp.InnerText, "&gt;", ">"), "&lt;", "<")
                        Dim nDiscID As Integer = oTmp.GetAttribute("nDiscountId")
                        oDisc = oDiscounts.SelectSingleNode("Discount[@id=" & nDiscID & "]")
                        If Not oDisc Is Nothing Then
                            If oDisc.SelectSingleNode("Content[@id=" & oTmp.GetAttribute("id") & "]") Is Nothing Then
                                oDisc.AppendChild(oDisc.OwnerDocument.ImportNode(oTmp.CloneNode(True), True))
                            End If
                        End If
                        appendCount = appendCount + 1
                    Next
                    PerfMon.Log("Discount", "getDiscountXML appended " & appendCount & " Items")

                    PerfMon.Log("Discount", "end-getDiscountXML")
                    PageElmt.AppendChild(oDiscounts)

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "returnDocumentFromItem", ex, "", "", gbDebug)
                End Try
            End Sub

            Public Function RemoveDiscountCode() As String
                Dim cProcessInfo As String
                Dim sSql As String
                Dim oDs As DataSet
                Dim oRow As DataRow
                Dim sPromoCode As String = ""
                Try
                    'myCart.moCartXml
                    If myCart.mnCartId > 0 Then
                        sSql = "select * from tblCartOrder where nCartOrderKey=" & myCart.mnCartId
                        oDs = myWeb.moDbHelper.getDataSetForUpdate(sSql, "Order", "Cart")
                        Dim xmlNotes As XmlElement = Nothing
                        Dim xmlDoc As New XmlDocument

                        For Each oRow In oDs.Tables("Order").Rows
                            xmlDoc.LoadXml(oRow("cClientNotes"))
                            xmlNotes = xmlDoc.SelectSingleNode("Notes/PromotionalCode")

                            oRow("cClientNotes") = Nothing
                        Next
                        myWeb.moDbHelper.updateDataset(oDs, "Order", True)
                        oDs.Clear()
                        oDs = Nothing
                        If (xmlNotes IsNot Nothing) Then
                            sPromoCode = xmlNotes.InnerText
                        End If

                        UpdatePackagingforRemovePromoCode(myCart.mnCartId, sPromoCode)
                    End If
                    Return ""
                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "RemoveDiscountCode", ex, "", cProcessInfo, gbDebug)
                End Try
            End Function

            'update packaging from giftbox to standard when removing promocode
            Public Sub UpdatePackagingforRemovePromoCode(ByVal CartId As Integer, ByVal sPromoCode As String)
                Try
                    Dim sSQL, sValidtoremove As String
                    If (sPromoCode <> "") Then
                        sSQL = "select nDiscountKey from tblCartDiscountRules where cDiscountCode = '" & sPromoCode & "' and cAdditionalXML like '%<bFreeGiftBox>True</bFreeGiftBox>%'"
                        sValidtoremove = myWeb.moDbHelper.ExeProcessSqlScalar(sSQL)
                    End If

                    If (sValidtoremove <> "") Then
                        If (moConfig("DefaultPack") IsNot Nothing And moConfig("GiftPack") IsNot Nothing) Then
                            sSQL = ""
                            sSQL = "update tblcartitem set cItemName = '" & moConfig("DefaultPack") & "', nPrice = 0.00 where nParentId != 0 and  cItemName = '" & moConfig("GiftPack") & "' and nCartOrderId =" & CartId.ToString()
                            myWeb.moDbHelper.ExeProcessSql(sSQL)
                        End If
                    End If

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "UpdatePackagingforRemovePromoCode", ex, "", "", gbDebug)
                End Try
            End Sub
#End Region

#Region "Copied Cart Functions"

            'to use for contentprices

            Function getProductPricesByXml_OLD(ByVal cXml As String) As Double
                PerfMon.Log("Discount", "getProductPricesByXml")
                Dim cGroupXPath As String = ""
                Dim oProd As XmlNode = myWeb.moPageXml.CreateNode(XmlNodeType.Document, "", "product")
                Dim oPrices As XmlNodeList
                Dim oPrice As XmlNode
                Dim oDefaultPrice As XmlNode
                Dim nPrice As Double = 0.0#
                Dim cProcessInfo As String = ""
                Try
                    ' Load product xml
                    oProd.InnerXml = cXml
                    ' Get the Default Price - note if it does not exist, then this is menaingless.
                    oDefaultPrice = oProd.SelectSingleNode("Content/Prices/Default")
                    ' If logged on we need to search prices by group, otherwise use the default value.
                    If myWeb.mnUserId > 0 Then
                        ' Define the group xpath
                        If mcGroups <> "" Then
                            Dim sGroups As String = mcGroups
                            sGroups = Replace(sGroups, " ", "_")
                            cGroupXPath = "[self::" & Replace(sGroups, ",", " or self::") & "]"
                            ' Get the prices
                            oPrices = oProd.SelectNodes("Content/Prices/*" & cGroupXPath)
                            If Not (oDefaultPrice Is Nothing) Then
                                ' Get the minimum price on offer
                                For Each oPrice In oPrices
                                    If IsNumeric(oPrice.InnerText) Then
                                        If (CDbl(oPrice.InnerText) < nPrice And CDbl(oPrice.InnerText) > 0) Or nPrice = 0 Then nPrice = CDbl(oPrice.InnerText)
                                    End If
                                Next
                            End If
                        End If
                    Else
                        ' Not logged on - ensure that the default price is returned, if applicable.
                        If Not (oDefaultPrice Is Nothing) Then
                            If IsNumeric(oDefaultPrice.InnerText) Then nPrice = CDbl(oDefaultPrice.InnerText)
                        End If
                    End If
                    If nPrice = 0 Then

                        Dim cCur As String = myWeb.moSession("cCurrency") 'moCartConfig("Currency")
                        If cCur = "" Then cCur = "GBP"
                        Dim oPriceElmt As XmlElement

                        For Each oPriceElmt In oProd.SelectNodes("Content/Prices/Price")
                            If oPriceElmt.GetAttribute("currency") = cCur Then
                                Dim cPrice As String = oPriceElmt.InnerText
                                If IsNumeric(cPrice) Then nPrice = cPrice
                                Exit For
                            End If
                        Next
                    End If
                    getProductPricesByXml_OLD = nPrice
                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "getProductPricesByXml", ex, "", cProcessInfo, gbDebug)
                End Try
            End Function

            Function getProductPricesByXml(ByVal cXml As String) As Double
                PerfMon.Log("Cart", "getProductPricesByXml")
                Dim cGroupXPath As String = ""
                Dim oProd As XmlNode = myWeb.moPageXml.CreateNode(XmlNodeType.Document, "", "product")
                Dim oDefaultPrice As XmlNode

                Try
                    ' Load product xml
                    oProd.InnerXml = cXml
                    ' Get the Default Price - note if it does not exist, then this is menaingless.
                    oDefaultPrice = oProd.SelectSingleNode("Content/Prices/Price[@default='true']")
                    'here we are checking if the price is the correct currency
                    'then if we are logged in we will also check if it belongs to
                    'one of the user's groups, just looking for 
                    Dim cGroups As String = mcGroups
                    If cGroups = "" Then cGroups &= "default,all,Standard,standard" Else cGroups &= ",default,all,Standard,standard"
                    cGroups = " and ( contains(@validGroup,'" & Replace(cGroups, ",", "') or contains(@validGroup,'")
                    cGroups &= "') or not(@validGroup) or @validGroup='')"

                    If myWeb.moSession("mcCurrency") = "" Then
                        myWeb.moSession("mcCurrency") = moCartConfig("currency")
                    End If
                    If myWeb.moSession("mcCurrency") = "" Then myWeb.moSession("mcCurrency") = "GBP"

                    Dim cxpath As String = "Content/Prices/Price[(@currency='" & myWeb.moSession("mcCurrency") & "') " & cGroups & " ][1]"

                    Dim oThePrice As XmlElement = oDefaultPrice

                    Dim oPNode As XmlElement
                    Dim nPrice As Double = 0.0#
                    For Each oPNode In oProd.SelectNodes(cxpath)
                        If Not oThePrice Is Nothing Then
                            If IsNumeric(oThePrice.InnerText) Then
                                If CDbl(oPNode.InnerText) < CDbl(oThePrice.InnerText) Then
                                    oThePrice = oPNode
                                    nPrice = CDbl(oThePrice.InnerText)
                                End If
                            Else
                                oThePrice = oPNode
                                nPrice = CDbl(oThePrice.InnerText)
                            End If
                        Else
                            oThePrice = oPNode
                            If IsNumeric(oThePrice.InnerText) Then
                                nPrice = CDbl(oThePrice.InnerText)
                            End If
                        End If
                    Next

                    Return nPrice
                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "getProductPricesByXml", ex, "", "", gbDebug)
                End Try

            End Function

            Private Sub applyDiscountsToPriceXml(ByVal oContentElmt As XmlElement, ByVal oDiscountElmt As XmlElement)

                Try
                    'PerfMon.Log("Cart", "applyDiscountsToPriceXml")

                    Dim cGroups As String = mcGroups
                    If cGroups = "" Then cGroups &= "default,all,Standard,standard" Else cGroups &= ",default,all,Standard,standard"
                    Dim cGroupsXp As String
                    cGroupsXp = " and ( contains(@validGroup,'" & Replace(cGroups, ",", "') or contains(@validGroup,'")
                    cGroupsXp &= "') or not(@validGroup) or @validGroup='')"

                    cGroupsXp = ""

                    Dim cxpath As String = "Prices/Price[@currency='" & mcCurrency & "' and node()!='' " & cGroupsXp & " ]"
                    ' Dim cxpath As String = "Prices/Price[@currency='" & mcCurrency & "' " & cGroupsXp & " ]"

                    Dim oPNode As XmlElement



                    For Each oPNode In oContentElmt.SelectNodes(cxpath)

                        If oPNode.GetAttribute("originalPrice").Equals("") Then
                            oPNode.SetAttribute("originalPrice", oPNode.InnerText)
                            'End If
                        End If

                        If oDiscountElmt.GetAttribute("isPercent").Equals("0") Then
                            If Not oDiscountElmt.GetAttribute("value") = "" Then
                                'Check it allows strings to be used here
                                If Not oPNode.InnerText.Equals("") Then
                                    If IsNumeric(oPNode.InnerText) Then
                                        oPNode.InnerText = Round(CDbl(oPNode.InnerText) - CDbl(oDiscountElmt.GetAttribute("value")), , , mbRoundUp)
                                        oDiscountElmt.SetAttribute("saving", Round(CDbl(oDiscountElmt.GetAttribute("value")), , , mbRoundUp))

                                    End If
                                End If
                            End If
                        ElseIf oDiscountElmt.GetAttribute("isPercent").Equals("1") Then
                            If Not oDiscountElmt.GetAttribute("value") = "" Then
                                If Not oPNode.InnerText.Equals("") Then
                                    If IsNumeric(oPNode.InnerText) Then
                                        oPNode.InnerText = Round(CDbl(oPNode.InnerText) - ((CDbl(oPNode.InnerText) / 100) * CDbl(oDiscountElmt.GetAttribute("value"))), , , mbRoundUp)
                                        oDiscountElmt.SetAttribute("saving", Round(((CDbl(oPNode.InnerText) / 100) * CDbl(oDiscountElmt.GetAttribute("value"))), , , mbRoundUp))
                                    End If
                                End If
                            End If
                        End If

                    Next

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "applyDiscountsToPriceXml", ex, "", "", gbDebug)
                End Try
            End Sub


            Function getGroupsByName() As String
                PerfMon.Log("Discount", "getGroupsByName")
                Dim cReturn As String = ""
                Dim oDs As DataSet
                Dim oDr As DataRow
                Dim cProcessInfo As String = ""
                Try
                    PerfMon.Log("Discount", "getGroupsByName-start")
                    If myWeb.mnUserId > 0 Then
                        oDs = myWeb.moDbHelper.GetDataSet("select * from tblDirectory g inner join tblDirectoryRelation r on g.nDirKey = r.nDirParentId where r.nDirChildId = " & myWeb.mnUserId, "Groups")
                        If oDs.Tables("Groups").Rows.Count > 0 Then
                            For Each oDr In oDs.Tables("Groups").Rows
                                cReturn = cReturn & "," & oDr.Item("cDirName")
                            Next
                            cReturn = Mid(cReturn, 2)
                        End If
                    End If
                    PerfMon.Log("Discount", "getGroupsByName-end")
                    Return cReturn
                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "getGroupsByName", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

#End Region


            Protected Overrides Sub Finalize()
                MyBase.Finalize()
            End Sub
        End Class
    End Class
End Class
