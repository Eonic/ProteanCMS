Option Strict Off
Option Explicit On
Imports System.Xml
Imports System.Collections
Imports System.Web.Configuration
Imports System.Data.SqlClient
Imports System.Web.HttpUtility
Imports VB = Microsoft.VisualBasic
Imports System.IO
Imports Eonic.Tools.Xml
Imports Eonic.Tools.Xml.XmlNodeState
Imports System
Imports TweetSharp


Partial Public Class Web

    Partial Public Class Content

#Region "JSON Actions"

        Public Class JSONActions
            Public Event OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)
            Private Const mcModuleName As String = "Eonic.Cart.JSONActions"
            Private moLmsConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("eonic/lms")
            Private myWeb As Eonic.Web
            Private myCart As Eonic.Web.Cart

            Public Sub New()
                Dim ctest As String = "this constructor is being hit" 'for testing
                myWeb = New Eonic.Web()
                myWeb.InitializeVariables()
                myWeb.Open()
                myCart = New Eonic.Web.Cart(myWeb)

            End Sub

            Public Function TwitterFeed(ByRef myApi As Eonic.API, ByRef jObj As Newtonsoft.Json.Linq.JObject) As String
                Try
                    Dim cProcessInfo As String = ""
                    Dim JsonResult As String = ""
                    Dim twtKey As String
                    Dim twtSecret As String
                    Dim _accessToken As String
                    Dim _accessTokenSecret As String
                    'Dim twtSvc As New TwitterService(twtKey, twtSecret)
                    'twtSvc.AuthenticateWith(_accessToken, _accessTokenSecret)
                    'Dim uto As New ListTweetsOnUserTimelineOptions()
                    'uto.ScreenName =
                    'uto.Count = 10
                    'Dim tweets = twtSvc.ListTweetsOnUserTimeline(uto)
                    'Dim tweet As TwitterStatus
                    '' For Each tweet In tweets
                    '' tweet.TextAsHtml
                    '' Next
                    'JsonResult = tweets.ToString()
                    Return JsonResult
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""))
                    Return ex.Message
                End Try
            End Function


        End Class

#End Region
    End Class

End Class

