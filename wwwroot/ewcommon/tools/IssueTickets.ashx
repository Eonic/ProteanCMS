<%@ WebHandler Language="VB" Class="IssueTickets" %>

Imports Microsoft.VisualBasic
Imports System
Imports System.IO
Imports System.Web
Imports System.Xml

Public Class IssueTickets : Implements IHttpHandler, IRequiresSessionState

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

        Dim OrderId = context.Request("orderid")


        Dim myCms As Protean.Cms = New Protean.Cms
        myCms.InitializeVariables()
        myCms.Open()

        Dim oCart As New Protean.Cms.Cart(myCms)
        oCart.InitializeVariables()
        oCart.mnCartId = OrderId
        'Dim oCartListElmt As XmlElement = myCms.moPageXml.CreateElement("Order")
        oCart.moCartXml = myCms.moPageXml.CreateElement("Order")
        oCart.mnTaxRate = 8

        oCart.GetCart(oCart.moCartXml, OrderId)
        Dim itemElmt As XmlElement

        If oCart.moCartXml.SelectNodes("descendant-or-self::Ticket").Count < 1 Then

            For Each itemElmt In oCart.moCartXml.SelectNodes("descendant-or-self::Order/Item/productDetail")
                If itemElmt.GetAttribute("purchaseAction") = "" Then
                    itemElmt.SetAttribute("purchaseAction", "Protean.Cms+Cart+PurchaseAction.IssueTickets")
                End If
            Next

            oCart.purchaseActions(oCart.moCartXml)

        End If

        context.Response.Redirect("/?ewCmd=Orders&ewCmd2=Display&id=" & OrderId)

    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class