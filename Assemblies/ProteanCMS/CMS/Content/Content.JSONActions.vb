Option Strict Off
Option Explicit On
Imports System.Xml
Imports System.Web.Configuration
Imports System.Data.SqlClient


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

            ' to call /ewapi/Cms.Content/SearchIndex?data={query:'driving',hitsLimit:'30'}

            Public Function SearchIndex(ByRef myApi As Protean.API, ByRef searchFilter As Newtonsoft.Json.Linq.JObject) As String
                Try
                    Dim SearchString As String = ""
                    Dim HitsLimit As Integer = 50
                    Dim fuzzySearch As String = ""
                    If Not searchFilter Is Nothing Then
                        SearchString = searchFilter("query")
                        fuzzySearch = searchFilter("fuzzysearch")
                        If searchFilter("hitslimit") <> "" Then
                            HitsLimit = CInt(searchFilter("hitslimit"))
                        End If
                    End If


                    Dim oSrch As New Protean.Cms.Search(myApi)
                    Dim oResultsXml As New XmlDocument()
                    oResultsXml.AppendChild(oResultsXml.CreateElement("Results"))
                    oSrch.moContextNode = oResultsXml.FirstChild
                    oSrch.IndexQuery(myApi, SearchString, HitsLimit, fuzzySearch)
                    Dim jsonString As String = Newtonsoft.Json.JsonConvert.SerializeXmlNode(oResultsXml, Newtonsoft.Json.Formatting.Indented)
                    jsonString = jsonString.Replace("/*?xml:namespace prefix = o ns = ""urn:schemas-microsoft-com:office:office"" /*/", "")
                    Return jsonString.Replace("""@", """_")

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "SearchContent", ex, ""))
                    Return ex.Message
                End Try
            End Function


            Public Function LogActivity(ByRef myApi As Protean.API, ByRef jObj As Newtonsoft.Json.Linq.JObject) As String
                Try
                    Dim oActivityType As dbHelper.ActivityType
                    Select Case jObj("type").ToString()
                        Case "PageViewed"
                            oActivityType = dbHelper.ActivityType.PageViewed
                    End Select
                    Dim nUserDirId As Long = CLng("0" + jObj("userId").ToString())
                    Dim nPageId As Long = CLng("0" & jObj("pageId").ToString())
                    Dim nArtId As Long = CLng("0" & jObj("artId").ToString())

                    If myApi.mnUserId > 0 Then
                        myWeb.moDbHelper.logActivity(oActivityType, nUserDirId, nPageId, nArtId)
                    End If

                    Return True

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""))
                    Return ex.Message
                End Try
            End Function

            'Charts
            Public Function GetChartData(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String

                Dim JsonResult As String = ""
                Dim chartContentKey As Integer
                If Not Integer.TryParse(inputJson("chartContentKey"), chartContentKey) Then
                    chartContentKey = 0
                End If

                Try
                    If chartContentKey > 0 Then
                        Dim dsChartData As DataSet

                        Dim sSql As String = "SELECT C.nContentKey,"
                        sSql &= " C.cContentName,"
                        sSql &= " Convert(Xml, C.cContentXmlBrief).value('(Content/cDisplayName)[1]', 'Varchar(50)') AS displayName,"
                        sSql &= " CONVERT(XML, C.cContentXmlBrief).value('(Content/@lineColor)[1]', 'Varchar(50)') AS lineColor,"
                        sSql &= " CONVERT(XML, C.cContentXmlBrief).value('(Content/@lineTension)[1]', 'Varchar(10)') AS lineTension,"
                        sSql &= " CONVERT(XML, C.cContentXmlBrief).value('(Content/@label-x)[1]', 'Varchar(10)') AS xLabelPosition,"
                        sSql &= " CONVERT(XML, C.cContentXmlBrief).value('(Content/@label-y)[1]', 'Varchar(10)') AS yLabelPosition,"
                        sSql &= " CONVERT(XML, C.cContentXmlBrief).value('(Content/@priority)[1]', 'integer') AS priority,"
                        sSql &= " '' AS url,"
                        sSql &= " P.ProductId AS productId,"
                        sSql &= " CD.D.value('(@x)[1]', 'Varchar(10)') AS xLoc,"
                        sSql &= " CD.D.value('(@y)[1]', 'Varchar(10)') AS yLoc"
                        sSql &= " FROM tblContentRelation CR"
                        sSql &= " JOIN tblContent C ON C.nContentKey = CR.nContentChildId"
                        sSql &= " JOIN tblAudit A ON A.nAuditKey = C.nAuditId"
                        sSql &= " OUTER APPLY"
                        sSql &= " ("
                        sSql &= " SELECT CR1.nContentChildId AS ProductId"
                        sSql &= " FROM tblContentRelation CR1"
                        sSql &= " JOIN tblContent C1 ON C1.nContentKey = CR1.nContentChildId"
                        sSql &= " WHERE CR1.nContentParentId = C.nContentKey AND C1.cContentSchemaName = 'Product'"
                        sSql &= " ) P"
                        sSql &= " OUTER APPLY (SELECT CAST(C.cContentXmlBrief as xml) as cContentXmlBriefxml) CB"
                        sSql &= " OUTER APPLY CB.cContentXmlBriefxml.nodes('/Content/dataset/datapoint') as CD(D) "
                        sSql &= " WHERE nContentParentId = " & chartContentKey & " AND C.cContentSchemaName = 'ChartDataSet'"
                        sSql &= " AND A.nStatus = 1 ORDER BY priority, C.nContentKey"

                        dsChartData = myWeb.moDbHelper.GetDataSet(sSql, "ChartDataSet", "Chart")

                        If Not dsChartData Is Nothing Then
                            'Update the contentUrls
                            If dsChartData.Tables.Count > 0 Then
                                For Each oRow As DataRow In dsChartData.Tables(0).Rows
                                    If Not oRow("productId") Is Nothing And Not oRow("productId") Is System.DBNull.Value Then
                                        oRow("url") = myWeb.GetContentUrl(oRow("productId"))
                                    End If
                                Next
                            End If

                            Dim chartXml As String = dsChartData.GetXml()
                            Dim xmlDoc As New XmlDocument
                            xmlDoc.LoadXml(chartXml)

                            Dim jsonString As String = Newtonsoft.Json.JsonConvert.SerializeXmlNode(xmlDoc.DocumentElement, Newtonsoft.Json.Formatting.Indented)
                            Return jsonString.Replace("""@", """_")
                        End If

                        Return String.Empty

                    End If
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

