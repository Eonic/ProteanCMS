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
Imports TweetSharp


Partial Public Class Cms

    Partial Public Class Content

#Region "JSON Actions"

        Public Class JSONActions
            Public Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
            Private Const mcModuleName As String = "Eonic.Cart.JSONActions"
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

            Public Function TwitterFeed(ByRef myApi As Protean.API, ByRef jObj As Newtonsoft.Json.Linq.JObject) As String
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
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""))
                    Return ex.Message
                End Try
            End Function

            Public Function UpdateContentValue(ByRef myApi As Protean.API, ByRef jObj As Newtonsoft.Json.Linq.JObject) As String
                Try

                    'first check the user privages
                    'myApi.mnUserId

                    Dim JsonResult As String = ""
                    Dim contentId As String = ""
                    Dim xpath As String = ""
                    Dim value As String 'JSON convert to XML and save ensure the xml schemas match.

                    Return JsonResult
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""))
                    Return ex.Message
                End Try
            End Function


            Public Function GetContent(ByRef myApi As Protean.API, ByRef jObj As Newtonsoft.Json.Linq.JObject) As String
                Try

                    Dim JsonResult As String = ""
                    Dim contentId As String = ""

                    Return JsonResult

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""))
                    Return ex.Message
                End Try
            End Function

        End Class

#End Region
    End Class

End Class

