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
            Private Const mcModuleName As String = "Protean.Subscription.JSONActions"
            Private Const cContactType As String = "Venue"
            Private myWeb As Protean.Cms
            Private myCart As Protean.Cms.Cart

            Public Sub New()
                Dim ctest As String = "this constructor is being hit" 'for testing
                myWeb = New Protean.Cms()
                myWeb.InitializeVariables()
                myWeb.Open()
                myCart = New Protean.Cms.Cart(myWeb)

            End Sub


            Public Function SubscriptionsProcess(ByRef myApi As Protean.API, ByRef jObj As Newtonsoft.Json.Linq.JObject) As String
                Try
                    Dim cProcessInfo As String = ""

                    'Dim CartXml As XmlElement = myWeb.moCart.CreateCartElement(myWeb.moPageXml)
                    'myCart.GetCart(CartXml.FirstChild)

                    'CartXml = updateCartforJSON(CartXml)

                    'Dim jsonString As String = Newtonsoft.Json.JsonConvert.SerializeXmlNode(CartXml, Newtonsoft.Json.Formatting.Indented)
                    'jsonString = jsonString.Replace("""@", """_")
                    'jsonString = jsonString.Replace("#cdata-section", "cDataValue")

                    'Return jsonString
                    ''persist cart
                    'myCart.close()

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""))
                    Return ex.Message
                End Try
            End Function


        End Class

#End Region
    End Class

End Class

