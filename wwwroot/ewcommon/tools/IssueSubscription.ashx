<%@ WebHandler Language="VB" Class="IssueSubscription" %>

Imports Microsoft.VisualBasic
Imports System
Imports System.IO
Imports System.Web
Imports System.Xml

' script to create a subscription for an order if for any reason this fails.

Public Class IssueSubscription : Implements IHttpHandler, IRequiresSessionState

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

        Dim OrderId = context.Request("orderid")
        Dim UserId = context.Request("userid")

        Dim myCms As Protean.Cms = New Protean.Cms
        myCms.InitializeVariables()
        myCms.Open()

        Dim oCart As New Protean.Cms.Cart(myCms)
        oCart.InitializeVariables()
        oCart.mnCartId = OrderId
        oCart.mnEwUserId = UserId
        'Dim oCartListElmt As XmlElement = myCms.moPageXml.CreateElement("Order")
        oCart.moCartXml = myCms.moPageXml.CreateElement("Order")

        oCart.GetCart(oCart.moCartXml, OrderId)

        oCart.mnPaymentId = myCms.moDbHelper.ExeProcessSqlScalar("select nPayMthdId from tblCartOrder where nCartOrderKey = " & OrderId)

        Dim itemElmt As XmlElement

        For Each itemElmt In oCart.moCartXml.SelectNodes("descendant-or-self::Order/Item/productDetail")

            Dim moSubscription As Protean.Cms.Cart.Subscriptions
            moSubscription = New Protean.Cms.Cart.Subscriptions(myCms)
            moSubscription.AddUserSubscriptions(oCart.mnCartId, oCart.mnEwUserId, oCart.mnPaymentId, itemElmt)

        Next

        context.Response.Redirect("/?ewCmd=Orders&ewCmd2=Display&id=" & OrderId)

    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class