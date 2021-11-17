
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

        Public Class PurchaseAction
            Public Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
            Private Const mcModuleName As String = "Protean.Cms.Cart.PurchaseActions"
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



            Public Function IssueTickets(ByRef myWeb As Protean.Cms, ByRef oCartItemProductDetailXml As XmlElement) As Boolean
                Try
                    Dim cartItem As XmlElement = oCartItemProductDetailXml.ParentNode

                    Dim CodeSetId As Int16 = CInt("0" & oCartItemProductDetailXml.GetAttribute("codeBank"))
                    Dim Quantity As Int16 = CInt(cartItem.GetAttribute("quantity"))
                    Dim thisQty As Int16
                    Dim CartItemId As Int16 = CInt(cartItem.GetAttribute("id"))
                    If CodeSetId > 0 Then
                        For thisQty = Quantity To 1 Step -1
                            Dim Code As String = myWeb.moDbHelper.IssueCode(CodeSetId, CartItemId, True, Nothing)
                            Dim TicketElement As XmlElement = oCartItemProductDetailXml.OwnerDocument.CreateElement("Ticket")
                            TicketElement.SetAttribute("code", Code)
                            oCartItemProductDetailXml.AppendChild(TicketElement)
                        Next
                    End If
                    Dim copyNode As String = oCartItemProductDetailXml.OuterXml

                    copyNode = copyNode.Replace("<productDetail", "<Content")
                    copyNode = copyNode.Replace("</productDetail>", "</Content>")

                    myWeb.moDbHelper.updateInstanceField(dbHelper.objectTypes.CartItem, CartItemId, "xItemXml", copyNode)

                    myWeb.moCart.SaveCartXML(cartItem.ParentNode)

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "IssueTickets", ex, ""))
                    Return ex.Message
                End Try
            End Function


        End Class

#End Region
    End Class

End Class


