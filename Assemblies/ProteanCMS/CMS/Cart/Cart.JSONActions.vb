
Option Strict Off
Option Explicit On
Imports System.Xml
Imports System.Collections
Imports System.Web.Configuration
Imports System.Data.SqlClient
Imports System.Web.HttpUtility
Imports VB = Microsoft.VisualBasic
Imports System.IO
Imports Protean.Tools.Xml
Imports Protean.Tools.Xml.XmlNodeState
Imports System
Imports System.Text
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports System.Collections.Generic


Partial Public Class Cms

    Partial Public Class Cart

#Region "JSON Actions"

        Public Class JSONActions
            Public Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
            Private Const mcModuleName As String = "Eonic.Cart.JSONActions"
            Private Const cContactType As String = "Venue"
            Private moLmsConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/lms")
            Private myWeb As Protean.Cms
            Private myCart As Protean.Cms.Cart

            Public Sub New()
                Dim ctest As String = "this constructor is being hit" 'for testing
                myWeb = New Protean.Cms()
                myWeb.InitializeVariables()
                myWeb.Open()
                myCart = New Protean.Cms.Cart(myWeb)

            End Sub

            Private Function updateCartforJSON(CartXml As XmlElement) As XmlElement
                Dim newstring As String = CartXml.InnerXml.Replace("<Item ", "<CartItem ").Replace("</Item>", "</CartItem>")
                CartXml.InnerXml = newstring
                Dim cartItems As XmlElement = myWeb.moPageXml.CreateElement("CartItems")
                Dim ItemCount As Int16 = 0

                For Each oItem As XmlElement In CartXml.SelectNodes("Order/CartItem")
                    cartItems.AppendChild(oItem)
                    ItemCount = ItemCount + 1
                Next

                If ItemCount = 1 Then
                    Dim oItems As XmlElement = myWeb.moPageXml.CreateElement("CartItem")
                    oItems.SetAttribute("dummy", "true")
                    cartItems.AppendChild(oItems)
                End If
                CartXml.FirstChild.AppendChild(cartItems)

                TidyHtmltoCData(CartXml)

                Return CartXml
            End Function


            Public Function GetCart(ByRef myApi As Protean.API, ByRef jObj As Newtonsoft.Json.Linq.JObject) As String
                Try
                    Dim cProcessInfo As String = ""

                    Dim CartXml As XmlElement = myWeb.moCart.CreateCartElement(myWeb.moPageXml)
                    myCart.GetCart(CartXml.FirstChild)

                    CartXml = updateCartforJSON(CartXml)

                    Dim jsonString As String = Newtonsoft.Json.JsonConvert.SerializeXmlNode(CartXml, Newtonsoft.Json.Formatting.Indented)
                    jsonString = jsonString.Replace("""@", """_")
                    jsonString = jsonString.Replace("#cdata-section", "cDataValue")

                    Return jsonString
                    'persist cart
                    myCart.close()

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""))
                    Return ex.Message
                End Try
            End Function

            Public Function AddItems(ByRef myApi As Protean.API, ByRef jObj As Newtonsoft.Json.Linq.JObject) As String
                Try
                    Dim cProcessInfo As String = ""
                    'jsonObject("artId")
                    ' myCart.AddItem()
                    'Output the new cart
                    Dim oDoc As New XmlDocument
                    Dim CartXml As XmlElement = myWeb.moCart.CreateCartElement(myWeb.moPageXml)



                    If myCart.mnCartId < 1 Then
                        myCart.CreateNewCart(CartXml, "Order")
                        If myCart.mcItemOrderType <> "" Then
                            myCart.mmcOrderType = myCart.mcItemOrderType
                        Else
                            myCart.mmcOrderType = ""
                        End If
                        myCart.mnProcessId = 1
                    End If

                    Dim item As Newtonsoft.Json.Linq.JObject
                    If (jObj("Item") IsNot Nothing) Then
                        For Each item In jObj("Item")
                            Dim bUnique As Boolean = False
                            Dim cProductPrice As Double = 0
                            Dim sProductName As String = ""
                            Dim bPackegingRequired As Boolean = False
                            Dim sOverideURL As String = ""
                            If item.ContainsKey("UniqueProduct") Then
                                bUnique = item("UniqueProduct")
                            End If
                            If item.ContainsKey("itemPrice") Then
                                cProductPrice = item("itemPrice")
                            End If
                            If item.ContainsKey("productName") Then
                                sProductName = item("productName")
                            End If
                            If item.ContainsKey("url") Then
                                sOverideURL = item("url")
                            End If
                            myCart.AddItem(item("contentId"), item("qty"), Nothing, sProductName, cProductPrice, "", bUnique, sOverideURL)

                        Next
                    End If
                    'Output the new cart
                    myCart.GetCart(CartXml.FirstChild)
                    CartXml = updateCartforJSON(CartXml)
                    'persist cart
                    myCart.close()

                    Dim jsonString As String = Newtonsoft.Json.JsonConvert.SerializeXmlNode(CartXml, Newtonsoft.Json.Formatting.None)
                    jsonString = jsonString.Replace("""@", """_")
                    jsonString = jsonString.Replace("#cdata-section", "cDataValue")

                    Return jsonString

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""))
                    Return ex.Message
                End Try

            End Function

            Public Function RemoveItems(ByRef myApi As Protean.API, ByRef jObj As Newtonsoft.Json.Linq.JObject) As String
                Try
                    Dim cProcessInfo As String = ""
                    Dim ItemCount As Long = 1

                    Dim item As Newtonsoft.Json.Linq.JObject

                    For Each item In jObj("Item")
                        If item("contentId") Is Nothing Then
                            ItemCount = myCart.RemoveItem(item("itemId"), 0)
                        Else
                            ItemCount = myCart.RemoveItem(0, item("contentId"))
                        End If
                    Next


                    If ItemCount = 0 Then
                        myCart.QuitCart()
                        myCart.EndSession()
                    End If

                    'Output the new cart   
                    Dim CartXml As XmlElement = myWeb.moCart.CreateCartElement(myWeb.moPageXml)
                    myCart.GetCart(CartXml.FirstChild)
                    'persist cart
                    myCart.close()
                    CartXml = updateCartforJSON(CartXml)

                    Dim jsonString As String = Newtonsoft.Json.JsonConvert.SerializeXmlNode(CartXml, Newtonsoft.Json.Formatting.Indented)
                    jsonString = jsonString.Replace("""@", """_")
                    jsonString = jsonString.Replace("#cdata-section", "cDataValue")
                    Return jsonString

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""))
                    Return ex.Message
                End Try

            End Function

            Public Function UpdateItems(ByRef myApi As Protean.API, ByRef jObj As Newtonsoft.Json.Linq.JObject) As String
                Try
                    Dim cProcessInfo As String = ""
                    Dim ItemCount As Long = 1

                    Dim item As Newtonsoft.Json.Linq.JObject
                    Dim CartXml As XmlElement = myWeb.moCart.CreateCartElement(myWeb.moPageXml)

                    If myCart.mnCartId < 1 Then
                        myCart.CreateNewCart(CartXml)
                        If myCart.mcItemOrderType <> "" Then
                            myCart.mmcOrderType = myCart.mcItemOrderType
                        Else
                            myCart.mmcOrderType = ""
                        End If
                        myCart.mnProcessId = 1
                    End If

                    For Each item In jObj("Item")
                        If item("contentId") Is Nothing Then
                            If item("qty") = "0" Then
                                ItemCount = myCart.RemoveItem(item("itemId"), 0)
                            Else
                                ItemCount = myCart.UpdateItem(item("itemId"), 0, item("qty"), item("skipPackaging"))
                            End If
                        Else
                            If item("qty") = "0" Then
                                ItemCount = myCart.RemoveItem(0, item("contentId"))
                            Else
                                ItemCount = myCart.UpdateItem(0, item("contentId"), item("qty"))
                            End If
                        End If
                    Next

                    If ItemCount = 0 Then
                        myCart.QuitCart()
                        myCart.EndSession()
                    End If

                    'Output the new cart
                    myCart.GetCart(CartXml.FirstChild)
                    'persist cart
                    myCart.close()
                    CartXml = updateCartforJSON(CartXml)

                    Dim jsonString As String = Newtonsoft.Json.JsonConvert.SerializeXmlNode(CartXml, Newtonsoft.Json.Formatting.Indented)
                    jsonString = jsonString.Replace("""@", """_")
                    jsonString = jsonString.Replace("#cdata-section", "cDataValue")
                    Return jsonString

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""))
                    Return ex.Message
                End Try

            End Function



            Public Function GetShippingOptions(ByRef myApi As Protean.API, ByRef jObj As Newtonsoft.Json.Linq.JObject) As String
                Try
                    Dim cProcessInfo As String = ""
                    Dim dsShippingOption As DataSet

                    Dim cDestinationCountry As String = ""
                    ' call it from cart
                    Dim nAmount As Long
                    Dim nQuantity As Long
                    Dim nWeight As Long
                    If (jObj IsNot Nothing) Then
                        If jObj("country") <> "" Then
                            cDestinationCountry = jObj("country")
                        Else
                            cDestinationCountry = myCart.moCartConfig("DefaultDeliveryCountry")
                        End If

                        If jObj("qty") = "0" Then
                            nQuantity = jObj("qty")
                        Else
                            nQuantity = 0
                        End If

                        If jObj("amount") = "0" Then
                            nAmount = jObj("amount")
                        Else
                            nAmount = 0
                        End If

                        If jObj("Weight") = "0" Then
                            nWeight = jObj("Weight")
                        Else
                            nWeight = 0
                        End If

                    End If

                    dsShippingOption = myCart.getValidShippingOptionsDS(cDestinationCountry, nAmount, nQuantity, nWeight)

                    Dim ShippingOptionXml As String = dsShippingOption.GetXml()
                    Dim xmlDoc As New XmlDocument
                    xmlDoc.LoadXml(ShippingOptionXml)


                    Dim jsonString As String = Newtonsoft.Json.JsonConvert.SerializeXmlNode(xmlDoc.DocumentElement, Newtonsoft.Json.Formatting.Indented)
                    Return jsonString.Replace("""@", """_")


                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetShippingOptions", ex, ""))
                    Return ex.Message
                End Try
            End Function


            Public Function UpdatedCartShippingOptions(ByRef myApi As Protean.API, ByRef jObj As Newtonsoft.Json.Linq.JObject) As String
                Try

                    Dim cProcessInfo As String = ""
                    Dim ShipOptKey As String
                    Dim json As Newtonsoft.Json.Linq.JObject = jObj
                    ShipOptKey = json.SelectToken("ShipOptKey")
                    myCart.updateGCgetValidShippingOptionsDS(ShipOptKey)
                    Dim CartXml As XmlElement = myWeb.moCart.CreateCartElement(myWeb.moPageXml)
                    myCart.GetCart(CartXml.FirstChild)

                    'persist cart
                    myCart.close()
                    CartXml = updateCartforJSON(CartXml)

                    Dim jsonString As String = Newtonsoft.Json.JsonConvert.SerializeXmlNode(CartXml, Newtonsoft.Json.Formatting.Indented)
                    jsonString = jsonString.Replace("""@", """_")
                    jsonString = jsonString.Replace("#cdata-section", "cDataValue")
                    Return jsonString


                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "UpdatedCartShippingOptions", ex, ""))
                    Return ex.Message
                End Try

            End Function

            Public Function UpdateDeliveryOptionByCountry(ByRef myApi As Protean.API, ByRef jObj As Newtonsoft.Json.Linq.JObject) As String
                Dim country As String = jObj("country")

                Dim CartXml As XmlElement = myWeb.moCart.CreateCartElement(myWeb.moPageXml)
                'check config setting here so that it will take order option which is optional.
                Dim cOrderofDeliveryOption As String = myCart.moCartConfig("ShippingTotalIsNotZero")
                cOrderofDeliveryOption = myCart.updateDeliveryOptionByCountry(CartXml.FirstChild, country, cOrderofDeliveryOption)
                If (myCart.CheckPromocodeAppliedForDelivery() <> "") Then
                    RemoveDiscountCode(myApi, jObj)
                    'this will remove discount section from address page in vuemain.js
                    cOrderofDeliveryOption = cOrderofDeliveryOption & "#1"
                Else
                    cOrderofDeliveryOption = cOrderofDeliveryOption & "#0"
                End If

                Return cOrderofDeliveryOption
            End Function


            Public Function GetContacts(ByRef myApi As Protean.API, ByRef jObj As Newtonsoft.Json.Linq.JObject) As String
                Try
                    Dim JsonResult As String = ""
                    Dim dirId As String = jObj("dirId")
                    'Dim offerId As String = jObj("offerId")

                    Dim userContacts = myWeb.moDbHelper.GetUserContactsXml(dirId)
                    JsonResult = JsonConvert.SerializeObject(userContacts)
                    Return JsonResult

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetLocations", ex, ""))
                    Return ex.Message
                End Try
            End Function

            Public Function GetContactForm(ByRef myApi As Protean.API, ByRef jObj As Newtonsoft.Json.Linq.JObject) As String
                Dim nId As Integer
                Try

                    Dim JsonResult As String = ""
                    Dim oDdoc = New XmlDocument()
                    Dim contactId As Integer = jObj("contactId")
                    Dim cAddressType As String = jObj("addressType")

                    Dim oForm As xForm = myCart.contactXform(cAddressType, "", "")
                    Dim oFormXml As String = oForm.Instance.SelectSingleNode("tblCartContact").OuterXml

                    oDdoc.LoadXml(oFormXml)
                    JsonResult = JsonConvert.SerializeXmlNode(oDdoc)
                    Return JsonResult

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetLocations", ex, ""))
                    Return ex.Message
                End Try
                Return JsonConvert.ToString(nId)
            End Function

            Public Function SetContact(ByRef myApi As Protean.API, ByRef jObj As Newtonsoft.Json.Linq.JObject) As String
                Dim nId As Integer
                Try
                    Dim supplierId As Integer = jObj("supplierId")
                    Dim contact As Contact = jObj("venue").ToObject(Of Contact)()
                    contact.cContactType = cContactType
                    contact.cContactForeignRef = String.Format("SUP-{0}", supplierId)

                    nId = myWeb.moDbHelper.SetContact(contact)
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "AddContact", ex, ""))
                    Return ex.Message
                End Try
                Return JsonConvert.ToString(nId)
            End Function

            Public Function DeleteContact(ByRef myApi As Protean.API, ByRef jObj As Newtonsoft.Json.Linq.JObject) As String
                Dim isSuccess As Boolean
                Try
                    Dim cContactKey As String = jObj("nContactKey")
                    isSuccess = myWeb.moDbHelper.DeleteContact(cContactKey)
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "DeleteContact", ex, ""))
                    Return ex.Message
                End Try
                Return JsonConvert.ToString(isSuccess)
            End Function

            Public Function AddProductOption(ByRef myApi As Protean.API, ByRef jObj As Newtonsoft.Json.Linq.JObject) As String
                Try

                    Dim CartXml As XmlElement = myWeb.moCart.CreateCartElement(myWeb.moPageXml)
                    'myCart.GetCart(CartXml.FirstChild)

                    'add product option
                    myCart.AddProductOption(jObj)
                    'myCart.UpdatePackagingANdDeliveryType()
                    myCart.GetCart(CartXml.FirstChild)
                    'persist cart
                    myCart.close()

                    CartXml = updateCartforJSON(CartXml)

                    Dim jsonString As String = Newtonsoft.Json.JsonConvert.SerializeXmlNode(CartXml, Newtonsoft.Json.Formatting.Indented)
                    jsonString = jsonString.Replace("""@", """_")
                    jsonString = jsonString.Replace("#cdata-section", "cDataValue")
                    Return jsonString

                Catch ex As Exception

                End Try
            End Function

            Public Function AddDiscountCode(ByRef myApi As Protean.API, ByRef jObj As Newtonsoft.Json.Linq.JObject) As String
                Try

                    Dim CartXml As XmlElement = myWeb.moCart.CreateCartElement(myWeb.moPageXml)
                    Dim strMessage As String = String.Empty
                    Dim jsonString As String = String.Empty
                    If Not (jObj("Code") Is Nothing) Then
                        strMessage = myCart.moDiscount.AddDiscountCode(jObj("Code"))
                        If (strMessage = jObj("Code")) Then
                            myCart.GetCart(CartXml.FirstChild)
                            'persist cart
                            myCart.close()
                            CartXml = updateCartforJSON(CartXml)

                            'jsonString = Newtonsoft.Json.JsonConvert.SerializeXmlNode(CartXml, Newtonsoft.Json.Formatting.Indented)
                            'jsonString = jsonString.Replace("""@", """_")
                            'jsonString = jsonString.Replace("#cdata-section", "cDataValue")

                        End If
                        If (strMessage <> String.Empty) Then
                            Return strMessage
                        End If

                    End If
                    Return strMessage
                Catch ex As Exception

                End Try
            End Function

            Public Function RemoveDiscountCode(ByRef myApi As Protean.API, ByRef jObj As Newtonsoft.Json.Linq.JObject) As String
                Try

                    Dim CartXml As XmlElement = myWeb.moCart.CreateCartElement(myWeb.moPageXml)

                    myCart.moDiscount.RemoveDiscountCode()
                    myCart.GetCart(CartXml.FirstChild)
                    'persist cart
                    myCart.close()
                    CartXml = updateCartforJSON(CartXml)

                    Dim jsonString As String = Newtonsoft.Json.JsonConvert.SerializeXmlNode(CartXml, Newtonsoft.Json.Formatting.Indented)
                    jsonString = jsonString.Replace("""@", """_")
                    jsonString = jsonString.Replace("#cdata-section", "cDataValue")
                    Return jsonString

                Catch ex As Exception

                End Try

            End Function

            Public Function UpdateCartProductPrice(ByRef myApi As Protean.API, ByRef jObj As Newtonsoft.Json.Linq.JObject) As String
                Try
                    Dim cProcessInfo As String = ""
                    Dim cProductPrice As Double = CDbl(jObj("itemPrice"))
                    Dim cartItemId As Long = CLng(jObj("itemId"))

                    If myWeb.moDbHelper.checkUserRole(myCart.moCartConfig("AllowPriceUpdateRole"), "Role", CLng("0" & myWeb.moSession("nUserId"))) Then

                        myCart.UpdateItemPrice(cartItemId, cProductPrice)

                    End If

                    Dim CartXml As XmlElement = myWeb.moCart.CreateCartElement(myWeb.moPageXml)

                    myCart.GetCart(CartXml.FirstChild)
                    'persist cart
                    myCart.close()
                    CartXml = updateCartforJSON(CartXml)

                    Dim jsonString As String = Newtonsoft.Json.JsonConvert.SerializeXmlNode(CartXml, Newtonsoft.Json.Formatting.Indented)
                    jsonString = jsonString.Replace("""@", """_")
                    jsonString = jsonString.Replace("#cdata-section", "cDataValue")
                    Return jsonString


                Catch ex As Exception
                    Return ex.Message
                End Try
            End Function

            Public Function AddCartAddress(ByRef myApi As Protean.API, ByRef jObj As Newtonsoft.Json.Linq.JObject, ByVal contactType As String, ByVal cartId As Int32, Optional ByVal emailAddress As String = "", Optional ByVal telphone As String = "") As Int32
                Try
                    Dim contact As New Contact()
                    Dim nId As Int32
                    If (jObj IsNot Nothing) Then
                        contact.cContactEmail = emailAddress
                        contact.cContactTel = telphone
                        contact.cContactType = contactType
                        contact.nContactCartId = cartId
                        If (jObj("Forename") IsNot Nothing) Then
                            contact.cContactFirstName = jObj("Forename")
                        End If
                        If (jObj("Surname") IsNot Nothing) Then
                            contact.cContactLastName = jObj("Surname")
                        End If
                        If (jObj("Title") IsNot Nothing) Then
                            contact.cContactTitle = jObj("Title")
                        End If
                        If (jObj("cContactCompany") IsNot Nothing) Then
                            contact.cContactCompany = jObj("cContactCompany")
                        End If
                        If (jObj("CartId") IsNot Nothing) Then
                            contact.nContactCartId = cartId
                        End If
                        If (jObj("Address1") IsNot Nothing) Then
                            contact.cContactAddress = jObj("Address1")
                        End If

                        If (jObj("Address2") IsNot Nothing) Then
                            contact.cContactAddress = contact.cContactAddress + " " + jObj("Address2").ToString()
                        End If
                        If (jObj("City") IsNot Nothing) Then
                            contact.cContactCity = jObj("City")
                        End If
                        If (jObj("State") IsNot Nothing) Then
                            contact.cContactState = jObj("State")
                        End If
                        If (jObj("Country") IsNot Nothing) Then
                            contact.cContactCountry = jObj("Country")
                        End If
                        If (jObj("Postcode") IsNot Nothing) Then
                            contact.cContactZip = jObj("Postcode")
                        End If
                        If (jObj("Fax") IsNot Nothing) Then
                            contact.cContactFax = jObj("Fax")
                        End If

                        contact.cContactName = contact.cContactTitle + " " + contact.cContactFirstName + " " + contact.cContactLastName

                    End If

                    nId = myWeb.moDbHelper.SetContact(contact)
                    Return nId
                Catch ex As Exception
                    Return ex.Message
                End Try
            End Function



            Public Function CompleteOrder(ByVal sProviderName As String, ByVal nCartId As Integer, ByVal sAuthNo As String, ByVal dAmount As Double, ByVal ShippingType As String) As String
                Try
                    Dim oXml As XmlDocument = New XmlDocument
                    Dim cShippingType As String = String.Empty
                    Dim oDetailXml As XmlElement = oXml.CreateElement("Response")
                    Dim CartXml As XmlElement = myWeb.moCart.CreateCartElement(myWeb.moPageXml)
                    addNewTextNode("AuthCode", oDetailXml, sAuthNo)

                    If (ShippingType = String.Empty) Then
                        myCart.updateGCgetValidShippingOptionsDS(65)
                    Else
                        Dim shippingXml As XmlElement = myCart.makeShippingOptionsXML()

                        Dim nShipOptKey As Integer = Convert.ToInt32(shippingXml.SelectSingleNode("Method[cShipOptName='" + ShippingType + "']").SelectSingleNode("nShipOptKey").InnerText)
                        myCart.updateGCgetValidShippingOptionsDS(nShipOptKey)
                    End If

                    myWeb.moDbHelper.savePayment(nCartId, 0, sProviderName, sAuthNo, sProviderName, oDetailXml, DateTime.Now, False, dAmount)
                    myWeb.moDbHelper.SaveCartStatus(nCartId, cartProcess.Complete)

                    myCart.GetCart(CartXml.FirstChild)
                    myCart.purchaseActions(CartXml)
                    'persist cart
                    myCart.close()
                    CartXml = updateCartforJSON(CartXml)

                    Dim jsonString As String = Newtonsoft.Json.JsonConvert.SerializeXmlNode(CartXml, Newtonsoft.Json.Formatting.Indented)
                    jsonString = jsonString.Replace("""@", """_")
                    jsonString = jsonString.Replace("#cdata-section", "cDataValue")
                    Return jsonString
                Catch ex As Exception
                    Return ex.Message
                End Try
            End Function

            ''' <summary>
            ''' Refund order 
            ''' </summary>
            ''' <param name="myApi"></param>
            ''' <param name="jObj"></param>
            ''' <returns></returns>
            Public Function RefundOrder(ByRef myApi As Protean.API, ByRef jObj As Newtonsoft.Json.Linq.JObject) As String
                Try
                    Dim oCart As New Cart(myWeb)
                    oCart.moPageXml = myWeb.moPageXml

                    Dim nProviderReference = IIf(jObj("nProviderReference") IsNot Nothing, CObj(jObj("nProviderReference")), "")
                    Dim Amount = IIf(jObj("nAmount") IsNot Nothing, CDec(jObj("nAmount")), "")
                    Dim refundPaymentReceipt = ""
                    If nProviderReference <> "" Then
                        Dim oPayProv As New Providers.Payment.BaseProvider(myWeb, nProviderReference)
                        refundPaymentReceipt = oPayProv.Activities.RefundPayment(nProviderReference, Amount)

                        Dim xmlDoc As New XmlDocument
                        Dim xmlResponse As XmlElement = xmlDoc.CreateElement("Response")
                        xmlResponse.InnerXml = "<RefundPaymentReceiptId>" & refundPaymentReceipt & "</RefundPaymentReceiptId>"
                        xmlDoc.LoadXml(xmlResponse.InnerXml.ToString())
                        Dim jsonString As String = Newtonsoft.Json.JsonConvert.SerializeXmlNode(xmlDoc.DocumentElement, Newtonsoft.Json.Formatting.Indented)

                        jsonString = jsonString.Replace("""@", """_")
                        jsonString = jsonString.Replace("#cdata-section", "cDataValue")

                        Return jsonString
                    End If

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""))
                    Return ex.Message
                End Try

            End Function

            ''' <summary>
            ''' Process New payment
            ''' </summary>
            ''' <param name="myApi"></param>
            ''' <param name="jObj"></param>
            ''' <returns></returns>
            Public Function ProcessNewPayment(ByRef myApi As Protean.API, ByRef jObj As Newtonsoft.Json.Linq.JObject) As String
                Try
                    Dim oCart As New Cart(myWeb)
                    oCart.moPageXml = myWeb.moPageXml

                    Dim providerName = IIf(jObj("sProviderName") IsNot Nothing, CStr(jObj("sProviderName")), "")
                    Dim orderId = IIf(jObj("orderId") IsNot Nothing, CStr(jObj("orderId")), "")
                    Dim amount = IIf(jObj("amount") IsNot Nothing, CDec(jObj("amount")), "")
                    Dim cardNumber = IIf(jObj("cardNumber") IsNot Nothing, CStr(jObj("cardNumber")), "")
                    Dim cV2 = IIf(jObj("cV2") IsNot Nothing, CStr(jObj("cV2")), "")
                    Dim expiryDate = IIf(jObj("expiryDate") IsNot Nothing, CStr(jObj("expiryDate")), "")
                    Dim startDate = IIf(jObj("startDate") IsNot Nothing, CStr(jObj("startDate")), "")
                    Dim cardHolderName = IIf(jObj("cardHolderName") IsNot Nothing, CStr(jObj("cardHolderName")), "")
                    Dim address1 = IIf(jObj("address1") IsNot Nothing, CStr(jObj("address1")), "")
                    Dim address2 = IIf(jObj("address2") IsNot Nothing, CStr(jObj("address2")), "")
                    Dim town = IIf(jObj("town") IsNot Nothing, CStr(jObj("town")), "")
                    Dim postCode = IIf(jObj("postCode") IsNot Nothing, CStr(jObj("postCode")), "")
                    Dim paymentReceipt = ""
                    Dim jsonString As String = ""
                    If providerName <> "" Then
                        Dim oPayProv As New Providers.Payment.BaseProvider(myWeb, providerName)
                        paymentReceipt = oPayProv.Activities.ProcessNewPayment(orderId, amount, cardNumber, cV2, expiryDate, startDate, cardHolderName, address1, address2, town, postCode)
                        Dim xmlDoc As New XmlDocument
                        Dim xmlResponse As XmlElement = xmlDoc.CreateElement("Response")
                        xmlResponse.InnerXml = "<PaymentReceiptId>" & paymentReceipt & "</PaymentReceiptId>"
                        xmlDoc.LoadXml(xmlResponse.InnerXml.ToString())
                        jsonString = Newtonsoft.Json.JsonConvert.SerializeXmlNode(xmlDoc.DocumentElement, Newtonsoft.Json.Formatting.Indented)
                        jsonString = jsonString.Replace("""@", """_")
                        jsonString = jsonString.Replace("#cdata-section", "cDataValue")
                    End If
                    Return jsonString

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""))
                    Return ex.Message
                End Try

            End Function

            Public Function SavePaymentInfo(ByRef myApi As Protean.API, ByRef jObj As Newtonsoft.Json.Linq.JObject) As String
                Try
                    Dim cProcessInfo As String = ""
                    Dim josResult As String = "SUCCESS"

                    'input params
                    Dim cProductPrice As Double = CDbl(jObj("orderId"))

                    Try
                        'if we receive any response from judopay pass it from PaymentReceipt
                        'response should contain payment related all references like result, status, cardtoken, receiptId etc
                        'validate if weather success or declined in Judopay.cs and redirect accordingly

                        'Return oPayProv.Activities.PaymentReceipt()

                    Catch ex As Exception
                        josResult = "ERROR"
                    End Try


                    Return josResult

                Catch ex As Exception
                    Return ex.Message
                End Try
            End Function


        End Class

#End Region
    End Class

End Class


