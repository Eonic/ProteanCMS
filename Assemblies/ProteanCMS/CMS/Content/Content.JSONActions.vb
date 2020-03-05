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
Imports System.Collections.Generic
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Partial Public Class Cms

    Partial Public Class Content

#Region "JSON Actions"

        Public Class JSONActions
            Public Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
            Private Const mcModuleName As String = "Eonic.Content.JSONActions"
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
                    Return JsonResult

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""))
                    Return ex.Message
                End Try
            End Function

            Public Function IsUnique(ByRef myApi As Protean.API, ByRef jObj As Newtonsoft.Json.Linq.JObject) As String
                Try
                    'first check the user is validated we do not want this open to non admin users.
                    Dim cContentType As String
                    Dim cTableName As String
                    Dim xPath As String
                    Dim DataField As String


                    Dim JsonResult As String = ""
                    Return JsonResult

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""))
                    Return ex.Message
                End Try
            End Function

            Public Function SearchContent(ByRef myApi As Protean.API, ByRef searchFilter As Newtonsoft.Json.Linq.JObject) As String
                Try
                    Dim nRoot As String = searchFilter("nRoot") 'Page to search
                    Dim cContentType As String = searchFilter("cContentType") 'Comma separated list of content types
                    Dim cExpression As String = searchFilter("cExpression")
                    Dim bChilds As Boolean = searchFilter("bChilds") 'Search in child pages
                    Dim nParId As String = searchFilter("nParId")
                    Dim bIgnoreParID As String = searchFilter("bIgnoreParID")
                    Dim bIncRelated As Boolean = searchFilter("bIncRelated")
                    Dim cTableName As String = searchFilter("cTableName")
                    Dim cSelectField As String = searchFilter("cSelectField")
                    Dim cFilterField As String = searchFilter("cFilterField")

                    Dim cTmp As String = String.Empty
                    If bIncRelated = True Then
                        Dim sSQL As String = "Select " & cSelectField & " From " & cTableName & " WHERE " & cFilterField & " = " & nParId
                        Dim oDre As SqlDataReader = myWeb.moDbHelper.getDataReader(sSQL)
                        Do While oDre.Read
                            cTmp &= oDre(0) & ","
                        Loop
                        oDre.Close()
                        If Not cTmp = "" Then cTmp = Left(cTmp, Len(cTmp) - 1)
                    End If

                    Dim searchResultXML As XmlElement
                    searchResultXML = myWeb.moDbHelper.RelatedContentSearch(nRoot, cContentType, bChilds, cExpression, nParId, IIf(bIgnoreParID, 0, nParId), cTmp.Split(","), bIncRelated)

                    Dim jsonString As String = Newtonsoft.Json.JsonConvert.SerializeXmlNode(searchResultXML, Newtonsoft.Json.Formatting.Indented)
                    Return jsonString.Replace("""@", """_")

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "SearchContent", ex, ""))
                    Return ex.Message
                End Try
            End Function


            Public Function SearchIndex(ByRef myApi As Protean.API, ByRef searchFilter As Newtonsoft.Json.Linq.JObject) As String
                Try
                    Dim SearchString As String = ""
                    If Not searchFilter Is Nothing Then
                        SearchString = searchFilter("query")
                    End If
                    Dim oSrch As New Protean.Cms.Search(myApi)
                    Dim oResultsXml As New XmlDocument()
                    oResultsXml.AppendChild(oResultsXml.CreateElement("Results"))
                    oSrch.moContextNode = oResultsXml.FirstChild
                    oSrch.IndexQuery(myApi, SearchString)
                    Dim jsonString As String = Newtonsoft.Json.JsonConvert.SerializeXmlNode(oResultsXml, Newtonsoft.Json.Formatting.Indented)
                    Return jsonString.Replace("""@", """_")

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "SearchContent", ex, ""))
                    Return ex.Message
                End Try
            End Function

        End Class

#End Region

    End Class

End Class

