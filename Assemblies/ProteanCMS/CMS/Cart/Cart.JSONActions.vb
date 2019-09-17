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
                        myCart.CreateNewCart(CartXml)
                        If myCart.mcItemOrderType <> "" Then
                            myCart.mmcOrderType = myCart.mcItemOrderType
                        Else
                            myCart.mmcOrderType = ""
                        End If
                        myCart.mnProcessId = 1
                    End If

                    Dim item As Newtonsoft.Json.Linq.JObject

                    For Each item In jObj("Item")
                        Dim bUnique As Boolean = False
                        Dim cProductPrice As Double = 0
                        Dim sProductName As String = ""
                        If item.ContainsKey("UniqueProduct") Then
                            bUnique = item("UniqueProduct")
                        End If
                        If item.ContainsKey("itemPrice") Then
                            cProductPrice = item("itemPrice")
                        End If
                        If item.ContainsKey("productName") Then
                            sProductName = item("productName")
                        End If
                        myCart.AddItem(item("contentId"), item("qty"), Nothing, sProductName, cProductPrice, "", bUnique)
                    Next

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
                                ItemCount = myCart.UpdateItem(item("itemId"), 0, item("qty"), item("SkipPackaging"))
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

                    Dim cDestinationCountry As String = myCart.moCartConfig("DefaultDeliveryCountry")
                    ' call it from cart
                    Dim nAmount As Long
                    Dim nQuantity As Long
                    Dim nWeight As Long

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

                    'GetCart(myApi, jObj)


                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "UpdatedCartShippingOptions", ex, ""))
                    Return ex.Message
                End Try



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
                    myCart.GetCart(CartXml.FirstChild)

                    'add product option
                    myCart.AddProductOption(jObj)

                Catch ex As Exception

                End Try
            End Function

            Public Function AddDiscountCode(ByRef myApi As Protean.API, ByRef jObj As Newtonsoft.Json.Linq.JObject) As String
                Try

                    Dim CartXml As XmlElement = myWeb.moCart.CreateCartElement(myWeb.moPageXml)
                    ' myCart.GetCart(CartXml.FirstChild)
                    'add discount Code option
                    Return myCart.moDiscount.AddDiscountCode(jObj("Code"))

                    'Output the new cart
                    myCart.GetCart(CartXml.FirstChild)
                    'persist cart
                    myCart.close()
                    CartXml = updateCartforJSON(CartXml)

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




        End Class

#End Region
    End Class

End Class

