Imports System.Xml
Imports System.Web.HttpUtility
Imports System.Web.Configuration
Imports System.Configuration
Imports System.Data
Imports System.Data.SqlClient
Imports VB = Microsoft.VisualBasic
Imports System.Threading
Imports System.Text
Imports SR = System.Reflection
Imports System.Web.UI.WebControls
Imports System.Security.Principal
Imports System.IO
'This is the Indexer/Search items
Imports Lucene.Net.Index
Imports Lucene.Net.Documents
Imports Lucene.Net.Analysis
Imports Lucene.Net.Analysis.Standard
Imports Lucene.Net.Search
Imports Lucene.Net.QueryParsers
Imports Lucene.Net.Store
'regular expressions
Imports System.Text.RegularExpressions

Imports Protean.Tools.Number
Imports Protean.Tools.Xml
Imports System
Imports System.Collections.Generic

Partial Public Class Cms

    Public Class Search

        Protected moPageXml As XmlDocument 'the actual page, given from the web object
        Dim mcIndexFolder As String = ""
        Shadows mcModuleName As String = "Eonic.Search.Search" 'module name
        Public moContextNode As XmlElement
        Public myWeb As Cms
        Public moConfig As System.Collections.Specialized.NameValueCollection

        ' Settings - some of these are specific to search type
        ' Ideally I would like to extract search types into separate classes

        Private _includeFuzzySearch As Boolean = False
        Private _runPreFuzzySearch As Boolean = False
        Private _includePrefixNameSearch As Boolean = False
        Private _overrideQueryBuilder As Boolean = False
        Private _pagingDefaultSize As Integer = 25
        Private _indexReadFolder As String = ""
        Private _indexPath As String = "../Index"
        Private _logSearches As Boolean = False

        Public Class Modules

            Public Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
            Private Const mcModuleName As String = "Protean.Cms.Search.Modules"

            Public Sub New()
                'do nowt
            End Sub

            Public Sub GetResults(ByRef myWeb As Protean.Cms, ByRef oContentNode As XmlElement)
                PerfMon.Log("Search", "New")
                Dim oSrch As Protean.Cms.Search
                Try
                    oSrch = New Protean.Cms.Search(myWeb)
                    oSrch.moContextNode = oContentNode

                    Dim searchMode As String = CStr(myWeb.moRequest("searchMode"))
                    If searchMode = "" Then oContentNode.GetAttribute("searchMode")

                    Dim fuzzySearch As String = CStr(myWeb.moRequest("fuzzysearch"))

                    Dim contentType As String = CStr(myWeb.moRequest("contentType"))
                    If contentType = "" Then oContentNode.GetAttribute("contentType")

                    Select Case UCase(searchMode)
                        Case "REGEX", ""
                            'RegEx
                            oSrch.RegExpQuery(myWeb.moRequest("searchString"), contentType)
                        Case "XPATH"
                            'xPath
                            oSrch.XPathQuery(myWeb.moRequest("searchString"), contentType)
                        Case "INDEX"
                            'Searches an index of the entire site.
                            oSrch.IndexQuery(myWeb.moRequest("searchString"), fuzzySearch)
                        Case "USER"
                            oSrch.RegExpQuery(myWeb.moRequest("searchString"), myWeb.moRequest("groupId"), True)
                        Case "LATESTCONTENT"
                            oSrch.ContentByDateQuery(CStr(contentType),
                                                CStr(myWeb.moRequest("searchDateUnit")),
                                                CStr(myWeb.moRequest("searchDateUnitTotal")),
                                                CStr(myWeb.moRequest("searchStartDate")),
                                                CStr(myWeb.moRequest("searchEndDate")),
                                                CStr(myWeb.moRequest("searchColumn")))
                        Case "BESPOKE"
                            oSrch.BespokeQuery()
                        Case Else
                            'Do nothing
                    End Select

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "GetResults", ex, "", , gbDebug)
                End Try
            End Sub

        End Class

        Public Sub New(ByRef aWeb As Protean.Cms)
            PerfMon.Log("Search", "New")
            Try

                ' Set the global variables
                myWeb = aWeb
                moPageXml = myWeb.moPageXml
                moConfig = myWeb.moConfig

                ' Read settings in from the config

                ' General settings
                _logSearches = (moConfig("SearchLogging") = "on")
                _includeFuzzySearch = (moConfig("SearchFuzzy") = "on")
                _runPreFuzzySearch = (moConfig("SearchGetFuzzyCount") = "on")

                Dim pageDefaultSize As String = moConfig("SearchDefaultPageSize") & ""
                _pagingDefaultSize = Tools.Number.ConvertStringToIntegerWithFallback(pageDefaultSize, 25)

                ' Site Search specifics
                If moConfig("SiteSearch") = "on" Then
                    ' Search path with default
                    If Not String.IsNullOrEmpty(moConfig("SiteSearchPath") & "") Then _indexPath = moConfig("SiteSearchPath")
                    _indexPath = _indexPath.TrimEnd("/\".ToCharArray) & "/"
                    _indexReadFolder = myWeb.goServer.MapPath("/") & _indexPath

                    ' Search read path
                    If moConfig("SiteSearchReadPath") = "" Then
                        _indexReadFolder &= "Read/"
                    Else
                        _indexReadFolder = myWeb.goServer.MapPath("/") & moConfig("SiteSearchReadPath")
                        _indexReadFolder = _indexReadFolder.TrimEnd("/\".ToCharArray) & "/"
                    End If

                    mcIndexFolder = _indexReadFolder


                End If

            Catch ex As Exception
                returnException(myWeb.msException, mcModuleName, "New", ex, "", , gbDebug)
            End Try
        End Sub

        Public Sub New(ByRef myAPi As Protean.API)
            PerfMon.Log("Search", "New")
            Try

                ' Set the global variables

                moConfig = myAPi.moConfig

                ' Read settings in from the config

                ' General settings
                _logSearches = (moConfig("SearchLogging") = "on")
                _includeFuzzySearch = (moConfig("SearchFuzzy") = "on")
                _runPreFuzzySearch = (moConfig("SearchGetFuzzyCount") = "on")

                Dim pageDefaultSize As String = moConfig("SearchDefaultPageSize") & ""
                _pagingDefaultSize = Tools.Number.ConvertStringToIntegerWithFallback(pageDefaultSize, 25)

                ' Site Search specifics
                If moConfig("SiteSearch") = "on" Then
                    ' Search path with default
                    If Not String.IsNullOrEmpty(moConfig("SiteSearchPath") & "") Then _indexPath = moConfig("SiteSearchPath")
                    _indexPath = _indexPath.TrimEnd("/\".ToCharArray) & "/"
                    _indexReadFolder = myAPi.goServer.MapPath("/") & _indexPath

                    ' Search read path
                    If moConfig("SiteSearchReadPath") = "" Then
                        _indexReadFolder &= "Read/"
                    Else
                        _indexReadFolder = myWeb.goServer.MapPath("/") & moConfig("SiteSearchReadPath")
                        _indexReadFolder = _indexReadFolder.TrimEnd("/\".ToCharArray) & "/"
                    End If

                    mcIndexFolder = _indexReadFolder


                End If

            Catch ex As Exception
                returnException(myWeb.msException, mcModuleName, "New", ex, "", , gbDebug)
            End Try
        End Sub


        Public Overridable Sub apply()
            PerfMon.Log("Search", "apply")
            Dim sXpath As String = ""
            Dim sSearch As String = myWeb.moRequest("searchString")
            Dim aNewSearchWords() As String = Nothing
            Dim sError As String = ""
            Dim cSearchTerm As String = ""
            Dim bFirst As Boolean = True
            Dim bSearchSubmitted As Boolean = False

            Dim sProcessInfo As String = "Search Routing"

            Try
                Dim dStartTime As DateTime = Now()

                ' Get a context element
                ' This is the last element added tothe Contents Node, if applicable - otherwise create the Contents Node
                ' This gives us a marker when constructing a list of what was added to the Contents node by the search
                moContextNode = moPageXml.SelectSingleNode("/Page/Contents")
                If moContextNode Is Nothing Then
                    moContextNode = moPageXml.CreateElement("Contents")
                    moPageXml.DocumentElement.AppendChild(moContextNode)
                End If

                'If Not (oContextElmt.LastChild Is Nothing) Then oContextElmt = oContextElmt.LastChild

                ' There are five types of search:
                '
                ' CACHED ::         This is a search that has already been submitted and the user is simply navigating to 
                '                   another page of these results.  These use content IDs stored in a session variable to 
                '                   retrieve content.
                '
                ' XPATH ::          This search uses XPath in the SQL statement to filter out the search terms.  It is 
                '                   slow, but can be used to query specific parts of a content node, rather than all the nodes.
                ' 
                ' REGEX ::          This search uses a combination of approximate SQL filtering followed by refined Regular
                '                   Expression comparison.  It is very fast, but can only be executed on all the nodes in the 
                '                   content node, which is usually what most people want.
                '                   This is turned on by adding the config item ewSearchMode = "RegEx"
                ' 
                ' MSINDEX ::        This uses Microsoft Indexing Service to do a compeltely different, much more page content
                '                   focussed search.  Trevor knows more about this. (DEPRICATED)
                '
                ' INDEX ::          This uses the dot.Lucene Library from apache ported to C# an index is created from firing 
                '                   off the indexing routine from the admin area

                ' BESPOKE ::        Allows the search to be overridden



                Select Case CStr(myWeb.moRequest("searchMode"))
                    Case "REGEX", ""
                        'RegEx
                        RegExpQuery(myWeb.moRequest("searchString"), myWeb.moRequest("contentType"))
                    Case "XPATH"
                        'xPath
                        XPathQuery(myWeb.moRequest("searchString"), myWeb.moRequest("contentType"))
                    Case "INDEX"
                        'Searches an index of the entire site.
                        IndexQuery(myWeb.moRequest("searchString"))
                    Case "USER"
                        RegExpQuery(myWeb.moRequest("searchString"), myWeb.moRequest("groupId"), True)
                    Case "LATESTCONTENT"
                        ContentByDateQuery(CStr(myWeb.moRequest("contentType")),
                                            CStr(myWeb.moRequest("searchDateUnit")),
                                            CStr(myWeb.moRequest("searchDateUnitTotal")),
                                            CStr(myWeb.moRequest("searchStartDate")),
                                            CStr(myWeb.moRequest("searchEndDate")),
                                            CStr(myWeb.moRequest("searchColumn")))
                    Case "BESPOKE"
                        BespokeQuery()

                    Case Else
                        'Do nothing
                End Select

            Catch ex As Exception
                returnException(myWeb.msException, mcModuleName, "ExecuteSearch", ex, "", sProcessInfo, gbDebug)
            End Try
        End Sub


#Region "Search Methods"

        Protected Function BuildFiltersFromRequest(ByRef XmlDoc As XmlDocument, ByRef currentRequest As System.Web.HttpRequest) As XmlElement
            PerfMon.Log("Search", "BuildFiltersFromRequest")
            Dim processInfo As String = ""

            Dim filters As XmlElement
            Dim filter As XmlElement

            Dim requestItemsLength As Integer

            Try

                ' Create filter metadata for search criteria based on the request
                ' The request should be in the form <reserved_parameter>_ordinal
                ' reserved parameters are:
                ' sf-name - the field name
                ' sf-value - the value to search for - if this equals searchstring, then use the main searchstring
                ' sf-min - for range queries, the min
                ' sf-max - for range queries, the max
                ' sf-type - the data type of the field "string","number" or "float"

                filters = XmlDoc.CreateElement("Filters")

                requestItemsLength = currentRequest.QueryString.AllKeys.Length + currentRequest.Form.AllKeys.Length

                If requestItemsLength > 0 Then

                    Dim formsAndQueryStringKeys(requestItemsLength - 1) As String
                    Array.Copy(currentRequest.QueryString.AllKeys, 0, formsAndQueryStringKeys, 0, currentRequest.QueryString.AllKeys.Length)
                    Array.Copy(currentRequest.Form.AllKeys, 0, formsAndQueryStringKeys, currentRequest.QueryString.AllKeys.Length, currentRequest.Form.AllKeys.Length)

                    Dim requestOrdinal As String = ""

                    Dim addFilter As Boolean

                    Dim fieldName As String = ""
                    Dim fieldValue As String = ""
                    Dim fieldMin As String = ""
                    Dim fieldMax As String = ""
                    Dim fieldType As String = ""
                    Dim fieldTextTerms As String() = Nothing


                    For Each requestKey As String In formsAndQueryStringKeys

                        If requestKey IsNot Nothing Then
                            If requestKey.StartsWith("sf-name-") And currentRequest.Item(requestKey) <> "" Then

                                addFilter = False

                                ' Get the field data
                                requestOrdinal = requestKey.Substring(8)
                                fieldName = currentRequest.Item(requestKey)
                                fieldValue = currentRequest.Item("sf-value-" & requestOrdinal)
                                fieldMin = currentRequest.Item("sf-min-" & requestOrdinal)
                                fieldMax = currentRequest.Item("sf-max-" & requestOrdinal)
                                fieldType = currentRequest.Item("sf-type-" & requestOrdinal)

                                filter = filters.OwnerDocument.CreateElement("Filter")
                                filter.SetAttribute("ordinal", requestOrdinal.ToString)
                                filter.SetAttribute("field", currentRequest.Item(requestKey))
                                filter.SetAttribute("fieldType", fieldType)

                                ' Determine the type of query
                                ' If min or max are populated then it's a range query
                                ' If value is populated then it's a term query
                                If Not (String.IsNullOrEmpty(fieldMin)) Or Not (String.IsNullOrEmpty(fieldMax)) Then
                                    ' Range query
                                    filter.SetAttribute("type", "range")
                                    filter.SetAttribute("min", fieldMin)
                                    filter.SetAttribute("max", fieldMax)
                                    addFilter = True
                                ElseIf Not (String.IsNullOrEmpty(fieldValue)) Then
                                    ' term query
                                    filter.SetAttribute("type", "term")
                                    filter.SetAttribute("value", fieldValue)
                                    addFilter = True
                                End If

                                If addFilter Then
                                    filters.AppendChild(filter)
                                End If

                            End If
                        End If

                    Next

                End If



                Return filters

            Catch ex As Exception
                returnException(myWeb.msException, mcModuleName, "BuildFiltersFromRequest", ex, "", processInfo, gbDebug)
                Return Nothing
            End Try
        End Function

        Sub IndexQuery(ByVal cQuery As String, Optional fuzzySearch As String = "on")
            PerfMon.Log("Search", "IndexQuery")
            Dim processInfo As String = "Looking for : " & cQuery
            Try

                Dim resultsCount As Integer = 0
                Dim resultsXML As XmlElement = moContextNode.OwnerDocument.CreateElement("Content")
                Dim HitsLimit As Integer
                If (myWeb.moRequest("hitlimit") > 0) Then
                    HitsLimit = myWeb.moRequest("hitlimit")
                Else
                    HitsLimit = 300
                End If
                Dim PerPageCount As Integer
                If (myWeb.moRequest("PerPageCount") > 0) Then
                    PerPageCount = myWeb.moRequest("PerPageCount")
                Else
                    PerPageCount = 12
                End If
                Dim command As Integer
                If (myWeb.moRequest("command") > 0) Then
                    command = myWeb.moRequest("command")
                Else
                    command = 0
                End If


                If Not cQuery.Equals("") Then

                    'do the actual search
                    Dim dateStart As Date = Now
                    Dim dateFinish As Date
                    Dim oIndexDir As New System.IO.DirectoryInfo(mcIndexFolder)
                    Dim fsDir As Lucene.Net.Store.FSDirectory = FSDirectory.Open(oIndexDir)
                    Dim searcher As New IndexSearcher(fsDir, True)

                    ' Check for settings
                    If (fuzzySearch <> "off") Then
                        If LCase(myWeb.moConfig("SiteSearchFuzzy")) = "on" Then _includeFuzzySearch = True
                    End If

                    If myWeb.moRequest("fuzzy") = "on" Then _includeFuzzySearch = True
                    If myWeb.moRequest("fuzzy") = "off" Then _includeFuzzySearch = False

                    _overrideQueryBuilder = myWeb.moRequest("overrideQueryBuilder") = "true"
                    _includePrefixNameSearch = myWeb.moRequest("prefixNameSearch") = "true"

                    resultsXML.SetAttribute("fuzzy", IIf(_includeFuzzySearch, "on", "off"))
                    resultsXML.SetAttribute("prefixNameSearch", IIf(_includePrefixNameSearch, "true", "false"))

                    ' Generate the live page filter
                    Dim livePages As Filter = LivePageLuceneFilter()

                    ' Generate the query criteria
                    Dim searchFilters As XmlElement = BuildFiltersFromRequest(resultsXML.OwnerDocument, myWeb.moRequest)
                    If searchFilters IsNot Nothing Then resultsXML.AppendChild(searchFilters)

                    ' Generate the search query
                    Dim searchQuery As Lucene.Net.Search.Query = BuildLuceneQuery(cQuery, searchFilters)

                    ' Add a sort from the request
                    Dim queryOrder As Sort = SetSortFieldFromRequest(myWeb.moRequest)

                    ' Perform the search
                    Dim results As Lucene.Net.Search.TopDocs

                    If LCase(myWeb.moConfig("SiteSearchDebug")) = "on" Then
                        addElement(resultsXML, "LuceneQuery", searchQuery.ToString)
                        addElement(resultsXML, "LuceneLivePageFilter", livePages.ToString)
                    End If



                    If livePages Is Nothing Then
                        results = searcher.Search(searchQuery, HitsLimit)
                    Else
                        results = searcher.Search(searchQuery, livePages, HitsLimit, queryOrder)
                    End If

                    ' Log the search
                    If _logSearches Then
                        myWeb.moDbHelper.logActivity(IIf(_includeFuzzySearch, dbHelper.ActivityType.FuzzySearch, dbHelper.ActivityType.Search), myWeb.mnUserId, 0, 0, results.TotalHits, cQuery)
                    End If


                    ' Optional fuzzysearch for figures
                    ' This is simply designed to get a count that can be fed back to users
                    ' This could be a performance no-no, although most searches are quick
                    If _runPreFuzzySearch And Not _includeFuzzySearch Then
                        _includeFuzzySearch = True
                        searchQuery = BuildLuceneQuery(cQuery, searchFilters)
                        resultsXML.SetAttribute("fuzzyCount", searcher.Search(searchQuery, livePages, HitsLimit, queryOrder).TotalHits.ToString)
                    End If


                    Dim resultDoc As Document = Nothing
                    Dim result As XmlElement = Nothing
                    Dim pageIdField As Field = Nothing
                    Dim pageId As Long = 0
                    Dim url As String = ""
                    Dim menuItem As XmlElement = Nothing
                    Dim reservedFieldNames() As String = {"type", "text", "abstract"}
                    Dim pageStart As Integer
                    Dim pageCount As Integer
                    Dim pageEnd As Integer
                    ' Paging settings
                    ' Hits is so lightweight that we don't have to filter it beforehand
                    ' See: http://wiki.apache.org/lucene-java/LuceneFAQ#How_do_I_implement_paging.2C_i.e._showing_result_from_1-10.2C_11-20_etc.3F
                    Dim totalResults As Long = results.TotalHits

                    pageCount = myWeb.GetRequestItemAsInteger("pageCount", _pagingDefaultSize)
                    If pageCount <= 0 Then
                        pageCount = results.TotalHits
                    End If

                    If totalResults = 0 Then
                        pageStart = totalResults
                        pageEnd = totalResults
                    Else
                        pageStart = Math.Max(myWeb.GetRequestItemAsInteger("pageStart", 1), 1)
                        pageEnd = Math.Min(totalResults, pageStart + pageCount - 1)
                    End If

                    If pageEnd < pageStart Then pageEnd = pageStart

                    If totalResults > 0 And pageCount > 0 Then
                        Dim pageNumber As Integer = totalResults Mod pageCount
                    End If
                    Dim PerPageCount As Integer
                    If (myWeb.moRequest("PerPageCount") > 0) Then
                        PerPageCount = myWeb.moRequest("PerPageCount")
                    Else
                        PerPageCount = 12
                    End If














































































                    resultsXML.SetAttribute("TotalResult", totalResults)
                    resultsXML.SetAttribute("pageStart", pageStart)
                    resultsXML.SetAttribute("pageCount", pageCount)
                    resultsXML.SetAttribute("pageEnd", pageEnd)

                    resultsXML.SetAttribute("sortCol", myWeb.moRequest("sortCol"))
                    resultsXML.SetAttribute("sortColType", myWeb.moRequest("sortColType"))
                    resultsXML.SetAttribute("sortDir", myWeb.moRequest("sortDir"))
                    resultsXML.SetAttribute("Hits", HitsLimit)
                    resultsXML.SetAttribute("satrtCount", HitsLimit - PerPageCount)

                    Dim artIdResults As New List(Of Long)
                    Dim skipRecords As Integer = (myWeb.moRequest("page")) * PerPageCount
                    Dim takeRecord As Integer = PerPageCount
                    'Dim luceneDocuments As IList(Of Document) = New List(Of Document)()
                    Dim scoreDocs As ScoreDoc() = results.ScoreDocs
                    For i As Integer = skipRecords To results.TotalHits - 1

                        If i > (skipRecords + takeRecord) - 1 Then
                        Exit For
                    End If
                    'searcher.Doc(scoreDocs(i).Doc)

                    'Next

                    '' Process the results
                    'If totalResults > 0 Then
                    '        Dim sDoc As ScoreDoc
                    '    For Each sDoc In  ' results.ScoreDocs()

                    '        resultDoc = searcher.Doc(sDoc.Doc)
                    resultDoc = searcher.Doc(scoreDocs(i).Doc)
                    pageIdField = resultDoc.GetField("pgid")
                    If pageIdField IsNot Nothing AndAlso IsStringNumeric(pageIdField.StringValue) Then
                        pageId = Convert.ToInt32(pageIdField.StringValue)
                    Else
                        pageId = 0
                    End If

                    url = "" ' this is the link for the page

                    ' Get the menuitem element from the xml
                    If NodeState(moPageXml.DocumentElement, "/Page/Menu/descendant-or-self::MenuItem[@id=" & pageId & "]", , , , menuItem) <> XmlNodeState.NotInstantiated Then

                        ' Only process this result if it's in the paging zone
                        ' pageStart - 1 To pageEnd - 1
                        'don't add artId more than twice to results.
                        Dim thisArtId As Long
                        If Not resultDoc.GetField("artid") Is Nothing Then
                            thisArtId = CInt(resultDoc.GetField("artid").StringValue)
                        End If

                        If thisArtId = Nothing Or Not artIdResults.Exists(Function(x) x = thisArtId) Then
                            If Not thisArtId = Nothing Then artIdResults.Add(thisArtId)

                            url = resultDoc.GetField("url").StringValue & ""

                            ' Build the URL
                            If url = "" Then
                                url = menuItem.GetAttribute("url")
                                ' Add the artId, if exists
                                If Not resultDoc.GetField("artid") Is Nothing Then


                                    If resultDoc.GetField("contenttype") IsNot Nothing _
                                            AndAlso resultDoc.GetField("contenttype").StringValue = "Download" Then
                                        url = resultDoc.GetField("url").StringValue
                                    Else
                                        If moConfig("LegacyRedirect") = "on" Then
                                            url &= IIf(url = "/", "", "/") & resultDoc.GetField("artid").StringValue & "-/"

                                            Dim artName As String = ""
                                            If resultDoc.GetField("name") IsNot Nothing Then
                                                artName = resultDoc.GetField("name").StringValue
                                                Dim oRe As New Text.RegularExpressions.Regex("[^A-Z0-9]", Text.RegularExpressions.RegexOptions.IgnoreCase)
                                                artName = oRe.Replace(artName, "-").Trim("-")
                                                url &= artName
                                            End If
                                        Else
                                            url &= IIf(url = "/", "", "/") & "item" & resultDoc.GetField("artid").StringValue
                                        End If
                                    End If


                                End If
                            End If



                            result = moPageXml.CreateElement("Content")
                            result.SetAttribute("type", "SearchResult")
                            'result.SetAttribute("indexId", )
                            result.SetAttribute("indexRank", scoreDocs(i).Score)

                            For Each docField As Field In resultDoc.GetFields()

                                ' Don't add info to certain fields
                                If Array.IndexOf(reservedFieldNames, docField.Name) = -1 Then
                                    result.SetAttribute(docField.Name, docField.StringValue)
                                End If

                                If docField.Name = "abstract" Then

                                    ' Try to output this as Xml
                                    Dim innerString As String = docField.StringValue & ""
                                    processInfo = innerString
                                    Try
                                        result.InnerXml = innerString.Trim
                                    Catch ex As Exception
                                        innerString = innerString.Replace("&", "&amp;").Replace("&amp;amp;", "&amp;").Trim()
                                        processInfo = innerString
                                        result.InnerText = innerString
                                    End Try

                                End If
                            Next
                            result.SetAttribute("url", url)

                            moContextNode.AppendChild(result)
                            resultsCount = resultsCount + 1
                        End If

                    Else
                        ' Couldn't find the menuitme in the xml - which is odd given the livepagefilter
                        processInfo = "not found in live page filter"
                    End If

                    Next
                    ' End If

                    dateFinish = Now
                    resultsXML.SetAttribute("Time", dateFinish.Subtract(dateStart).TotalMilliseconds)
                    'resultsCount = results.TotalHits()
                Else
                    resultsXML.SetAttribute("Time", "0")
                End If

                'Dim oResXML As XmlElement = moPageXml.CreateElement("Content")

                'Dim prop As XmlNode = moContextNode.SelectSingleNode("Content[@type='SearchResult']")
                ' myWeb.moSession("lastResultCount") = Nothing
                Dim lodedResultCount As String = moContextNode.ChildNodes.Count.ToString()
                If (myWeb.moRequest("page") = 0) Then
                    myWeb.moSession("lastResultCount") = Nothing
                End If

                If myWeb.moSession("lastResultCount") Is Nothing Then

                    myWeb.moSession("lastResultCount") = lodedResultCount.ToString()
                    resultsXML.SetAttribute("satrtCount", HitsLimit - PerPageCount)
                    resultsXML.SetAttribute("loadedResult", myWeb.moSession("lastResultCount").ToString())
                Else
                    If (PerPageCount > myWeb.moSession("lastResultCount") And command = 0) Then
                        resultsXML.SetAttribute("satrtCount", HitsLimit - Convert.ToInt32(myWeb.moSession("lastResultCount") + 1))
                    Else
                        resultsXML.SetAttribute("satrtCount", HitsLimit - PerPageCount)
                    End If
                    resultsXML.SetAttribute("loadedResult", myWeb.moSession("lastResultCount").ToString())
                    myWeb.moSession("lastResultCount") = lodedResultCount.ToString()
                End If


                resultsXML.SetAttribute("SearchString", cQuery)
                resultsXML.SetAttribute("searchType", "INDEX")
                resultsXML.SetAttribute("type", "SearchHeader")


                moContextNode.AppendChild(resultsXML)


            Catch ex As Exception
                returnException(myWeb.msException, mcModuleName, "Search", ex, "", processInfo, gbDebug)
            End Try
        End Sub



        Sub IndexQuery(ByRef myAPI As Protean.API, ByVal cQuery As String, Optional HitsLimit As Integer = 300, Optional fuzzySearch As String = "on")
            PerfMon.Log("Search", "IndexQuery")
            Dim processInfo As String = "Looking for : " & cQuery
            Try

                Dim resultsCount As Integer = 0
                Dim resultsXML As XmlElement = moContextNode.OwnerDocument.CreateElement("Content")

                If Not cQuery.Equals("") Then

                    'do the actual search
                    Dim dateStart As Date = Now
                    Dim dateFinish As Date
                    Dim oIndexDir As New System.IO.DirectoryInfo(mcIndexFolder)
                    Dim fsDir As Lucene.Net.Store.FSDirectory = FSDirectory.Open(oIndexDir)
                    Dim searcher As New IndexSearcher(fsDir, True)

                    ' Check for settings
                    If myAPI.moRequest("fuzzy") = "on" Then _includeFuzzySearch = True
                    If (fuzzySearch <> "off") Then
                        If LCase(myAPI.moConfig("SiteSearchFuzzy")) = "on" Then _includeFuzzySearch = True
                    End If

                    _overrideQueryBuilder = myAPI.moRequest("overrideQueryBuilder") = "true"
                    _includePrefixNameSearch = myAPI.moRequest("prefixNameSearch") = "true"

                    resultsXML.SetAttribute("fuzzy", IIf(_includeFuzzySearch, "on", "off"))
                    resultsXML.SetAttribute("prefixNameSearch", IIf(_includePrefixNameSearch, "true", "false"))

                    ' Generate the live page filter
                    Dim livePages As Filter = LivePageLuceneFilter(myAPI)

                    ' Generate the query criteria
                    Dim searchFilters As XmlElement = BuildFiltersFromRequest(resultsXML.OwnerDocument, myAPI.moRequest)
                    If searchFilters IsNot Nothing Then resultsXML.AppendChild(searchFilters)

                    ' Generate the search query
                    Dim searchQuery As Lucene.Net.Search.Query = BuildLuceneQuery(cQuery, searchFilters)

                    ' Add a sort from the request
                    Dim queryOrder As Sort = SetSortFieldFromRequest(myAPI.moRequest)

                    ' Perform the search
                    Dim results As Lucene.Net.Search.TopDocs

                    If LCase(myAPI.moConfig("SiteSearchDebug")) = "on" Then
                        addElement(resultsXML, "LuceneQuery", searchQuery.ToString)
                        If Not livePages Is Nothing Then
                            addElement(resultsXML, "LuceneLivePageFilter", livePages.ToString)
                        End If
                    End If

                    '  Dim HitsLimit As Integer = 300

                    If livePages Is Nothing Then
                        results = searcher.Search(searchQuery, HitsLimit)
                    Else
                        results = searcher.Search(searchQuery, livePages, HitsLimit, queryOrder)
                    End If

                    ' Log the search
                    If _logSearches Then
                        myAPI.moDbHelper.logActivity(IIf(_includeFuzzySearch, dbHelper.ActivityType.FuzzySearch, dbHelper.ActivityType.Search), myAPI.mnUserId, 0, 0, results.TotalHits, cQuery)
                    End If

                    ' Optional fuzzysearch for figures
                    ' This is simply designed to get a count that can be fed back to users
                    ' This could be a performance no-no, although most searches are quick
                    If _runPreFuzzySearch And Not _includeFuzzySearch Then
                        _includeFuzzySearch = True
                        searchQuery = BuildLuceneQuery(cQuery, searchFilters)
                        resultsXML.SetAttribute("fuzzyCount", searcher.Search(searchQuery, livePages, HitsLimit, queryOrder).TotalHits.ToString)
                    End If

                    Dim resultDoc As Document = Nothing
                    Dim result As XmlElement = Nothing
                    Dim pageIdField As Field = Nothing
                    Dim pageId As Long = 0
                    Dim url As String = ""
                    Dim menuItem As XmlElement = Nothing
                    Dim reservedFieldNames() As String = {"type", "text", "abstract"}
                    Dim pageStart As Integer
                    Dim pageCount As Integer
                    Dim pageEnd As Integer
                    ' Paging settings
                    ' Hits is so lightweight that we don't have to filter it beforehand
                    ' See: http://wiki.apache.org/lucene-java/LuceneFAQ#How_do_I_implement_paging.2C_i.e._showing_result_from_1-10.2C_11-20_etc.3F
                    Dim totalResults As Long = results.TotalHits

                    pageCount = 0 'myAPI.GetRequestItemAsInteger("pageCount", _pagingDefaultSize)
                    If pageCount <= 0 Then
                        pageCount = results.TotalHits
                    End If

                    If totalResults = 0 Then
                        pageStart = totalResults
                        pageEnd = totalResults
                    Else
                        ' pageStart = Math.Max(myAPI.GetRequestItemAsInteger("pageStart", 1), 1)

                        pageStart = 1
                        pageEnd = Math.Min(totalResults, pageStart + pageCount - 1)
                    End If

                    If pageEnd < pageStart Then pageEnd = pageStart

                    If totalResults > 0 And pageCount > 0 Then
                        Dim pageNumber As Integer = totalResults Mod pageCount
                    End If


                    resultsXML.SetAttribute("pageStart", pageStart)
                    resultsXML.SetAttribute("pageCount", pageCount)
                    resultsXML.SetAttribute("pageEnd", pageEnd)

                    resultsXML.SetAttribute("sortCol", myAPI.moRequest("sortCol"))
                    resultsXML.SetAttribute("sortColType", myAPI.moRequest("sortColType"))
                    resultsXML.SetAttribute("sortDir", myAPI.moRequest("sortDir"))

                    Dim artIdResults As New List(Of Long)


                    ' Process the results
                    If totalResults > 0 Then
                        Dim sDoc As ScoreDoc
                        For Each sDoc In results.ScoreDocs()

                            resultDoc = searcher.Doc(sDoc.Doc)

                            pageIdField = resultDoc.GetField("pgid")
                            If pageIdField IsNot Nothing AndAlso IsStringNumeric(pageIdField.StringValue) Then
                                pageId = Convert.ToInt32(pageIdField.StringValue)
                            Else
                                pageId = 0
                            End If

                            url = "" ' this is the link for the page

                            ' Get the menuitem element from the xml
                            '     If NodeState(moContextNode.OwnerDocument.DocumentElement, "/Page/Menu/descendant-or-self::MenuItem[@id=" & pageId & "]", , , , menuItem) <> XmlNodeState.NotInstantiated Then

                            ' Only process this result if it's in the paging zone
                            ' pageStart - 1 To pageEnd - 1
                            'don't add artId more than twice to results.
                            Dim thisArtId As Long
                            If Not resultDoc.GetField("artid") Is Nothing Then
                                thisArtId = CInt(resultDoc.GetField("artid").StringValue)
                            End If

                            If thisArtId = Nothing Or Not artIdResults.Exists(Function(x) x = thisArtId) Then
                                If Not thisArtId = Nothing Then artIdResults.Add(thisArtId)

                                url = resultDoc.GetField("url").StringValue & ""

                                ' Build the URL
                                If url = "" Then
                                    url = menuItem.GetAttribute("url")
                                    ' Add the artId, if exists
                                    If Not resultDoc.GetField("artid") Is Nothing Then


                                        If resultDoc.GetField("contenttype") IsNot Nothing _
                                                AndAlso resultDoc.GetField("contenttype").StringValue = "Download" Then
                                            url = resultDoc.GetField("url").StringValue
                                        Else
                                            If moConfig("LegacyRedirect") = "on" Then
                                                url &= IIf(url = "/", "", "/") & resultDoc.GetField("artid").StringValue & "-/"

                                                Dim artName As String = ""
                                                If resultDoc.GetField("name") IsNot Nothing Then
                                                    artName = resultDoc.GetField("name").StringValue
                                                    Dim oRe As New Text.RegularExpressions.Regex("[^A-Z0-9]", Text.RegularExpressions.RegexOptions.IgnoreCase)
                                                    artName = oRe.Replace(artName, "-").Trim("-")
                                                    url &= artName
                                                End If
                                            Else
                                                url &= IIf(url = "/", "", "/") & "item" & resultDoc.GetField("artid").StringValue
                                            End If
                                        End If


                                    End If
                                End If



                                result = moContextNode.OwnerDocument.CreateElement("Content")
                                result.SetAttribute("type", "SearchResult")
                                'result.SetAttribute("indexId", )
                                result.SetAttribute("indexRank", sDoc.Score)

                                For Each docField As Field In resultDoc.GetFields()

                                    ' Don't add info to certain fields
                                    If Array.IndexOf(reservedFieldNames, docField.Name) = -1 Then
                                        result.SetAttribute(docField.Name, docField.StringValue)
                                    End If

                                    If docField.Name = "abstract" Then

                                        ' Try to output this as Xml
                                        Dim innerString As String = docField.StringValue & ""
                                        processInfo = innerString
                                        Try
                                            result.InnerXml = innerString.Trim
                                        Catch ex As Exception
                                            innerString = innerString.Replace("&", "&amp;").Replace("&amp;amp;", "&amp;").Trim()
                                            processInfo = innerString
                                            result.InnerText = innerString
                                        End Try

                                    End If
                                Next
                                result.SetAttribute("url", url)

                                moContextNode.AppendChild(result)
                                resultsCount = resultsCount + 1
                            End If

                            '  Else
                            ' Couldn't find the menuitme in the xml - which is odd given the livepagefilter
                            ' processInfo = "not found in live page filter"
                            'End If

                        Next
                    End If

                    dateFinish = Now
                    resultsXML.SetAttribute("Time", dateFinish.Subtract(dateStart).TotalMilliseconds)
                    'resultsCount = results.TotalHits()
                Else
                    resultsXML.SetAttribute("Time", "0")
                End If

                'Dim oResXML As XmlElement = moPageXml.CreateElement("Content")

                resultsXML.SetAttribute("SearchString", cQuery)
                resultsXML.SetAttribute("searchType", "INDEX")
                resultsXML.SetAttribute("type", "SearchHeader")
                resultsXML.SetAttribute("Hits", resultsCount)

                moContextNode.AppendChild(resultsXML)


            Catch ex As Exception
                returnException(myWeb.msException, mcModuleName, "Search", ex, "", processInfo, gbDebug)
            End Try
        End Sub


        Sub XPathQuery(ByVal sSearch As String, ByVal cContentType As String)
            PerfMon.Log("Search", "XPathQuery")
            Dim sXpath As String = ""
            Dim sXpathRoot As String = ""
            Dim aSearchWords() As String
            Dim aNewSearchWords() As String = Nothing
            Dim i As Integer
            Dim sError As String = ""
            Dim cSearchTerm As String = ""
            Dim bFirst As Boolean = True
            Dim bSearchSubmitted As Boolean = False
            Dim dtStart As Date = Now
            Try

                'going to see if there is a a form
                Dim ofrmElmt As XmlElement = moContextNode.SelectSingleNode("Content[@name='search' and @type='xform']")
                If Not ofrmElmt Is Nothing Then
                    Dim ofrm As New xForm(myWeb)
                    ofrm.load(ofrmElmt)
                    ofrm.updateInstanceFromRequest()
                    If ofrm.Instance Is Nothing Then ofrmElmt = ofrm.Instance
                    sXpath = getXpathFromQueryXml(ofrm.Instance)
                    ofrm.addValues()
                End If

                ' XPATH Search
                bSearchSubmitted = True

                If moConfig("ewSearchXpath") = "" Then
                    sXpathRoot = "//*"
                    'sXpathRoot = "//"
                Else
                    sXpathRoot = moConfig("ewSearchXpath")
                End If


                ' Clean the search term and put it into an array of words
                aSearchWords = Split(CleanSearchString(sSearch), " ")

                ' Construct the Xpath
                ' Note that the logic for searching is (Word 1 OR a variant of it) AND (Word 2 OR a variant of it)
                For i = 0 To UBound(aSearchWords)
                    cSearchTerm = aSearchWords(i)
                    If Trim(cSearchTerm) <> "" Then

                        If bFirst Then
                            bFirst = Not (bFirst)
                        Else
                            sXpath = sXpath & " and "
                        End If

                        ' Add the word
                        sXpath = sXpath & "[contains(upper-case(.),''" & SqlFmt(UCase(cSearchTerm)) & "'')"

                        ' Get the variants of the word
                        If cSearchTerm.EndsWith("s") Then
                            sXpath = sXpath & " or contains(upper-case(.),''" & SqlFmt(UCase(Left(cSearchTerm, Len(cSearchTerm) - 1))) & "'')"
                        End If
                        If aSearchWords(i).EndsWith("ies") Then
                            sXpath = sXpath & " or contains(upper-case(.),''" & SqlFmt(UCase(Left(cSearchTerm, Len(cSearchTerm) - 3) & "y")) & "'')"
                        End If
                        If aSearchWords(i).EndsWith("y") Then
                            sXpath = sXpath & " or contains(upper-case(.),''" & SqlFmt(UCase(Left(cSearchTerm, Len(cSearchTerm) - 1) & "ies")) & "'')"
                        End If

                        sXpath = sXpath & "]"
                    End If
                Next

                If sXpath <> "" Then
                    'sXpath = sXpathRoot & "[" & sXpath & "]"
                    sXpath = sXpathRoot & sXpath
                    If myWeb.moRequest("contentType") = "" Then
                        ' GetPageContentFromSelect(" where dbo.fxn_SearchXML(cContentXML,'" & sXpath & "') = 1")
                    Else
                        'GetPageContentFromSelect(" where typ.cContentTypeName='" & myWeb.moRequest("contentType") & "' and dbo.fxn_SearchXML(cContentXML,'" & sXpath & "') = 1")
                    End If
                    Dim cSQL As String = "SET ARITHABORT ON SELECT nContentKey, cContentXmlBrief,  cContentXmlDetail, nContentPrimaryId, cContentName, cContentSchemaName " &
                        " FROM tblContent WHERE" &
                        "  (CAST(cContentXmlBrief as xml).exist('" & sXpath & "') = 1 or CAST(cContentXmlDetail as xml).exist('" & sXpath & "') = 1)" &
                        IIf(cContentType = "", "", " AND (cContentSchemaName = '" & SqlFmt(cContentType) & "')")
                    Dim oDr As SqlDataReader
                    oDr = myWeb.moDbHelper.getDataReader(cSQL)
                    Dim nResultCount As Integer
                    Dim cResultIDsCSV As String = ""
                    While oDr.Read
                        'If checkPagePermission(oDr("nContentPrimaryId")) = oDr("nContentPrimaryId") And ContentPagesLive(oDr("nContentKey")) Then


                        cResultIDsCSV &= oDr("nContentKey") & ","


                    End While
                    oDr.Close()
                    If cResultIDsCSV = "" Then cResultIDsCSV = "0"
                    nResultCount = GetContentXml(moContextNode, cResultIDsCSV)

                    ' Log the search
                    If _logSearches Then
                        myWeb.moDbHelper.logActivity(dbHelper.ActivityType.Search, myWeb.mnUserId, 0, 0, nResultCount, sSearch)
                    End If

                    Dim oResXML As XmlElement = moPageXml.CreateElement("Content")

                    oResXML.SetAttribute("SearchString", sSearch)
                    oResXML.SetAttribute("type", "SearchHeader")
                    oResXML.SetAttribute("searchType", "XPATH")
                    oResXML.SetAttribute("Hits", nResultCount)
                    oResXML.SetAttribute("Time", Now.Subtract(dtStart).TotalMilliseconds)
                    oResXML.SetAttribute("DebugSQL", cSQL)

                    moContextNode.AppendChild(oResXML)




                End If
            Catch ex As Exception
                returnException(myWeb.msException, mcModuleName, "Search", ex, "", , gbDebug)
            End Try
        End Sub

        Sub RegExpQuery(ByVal sSearch As String, ByVal cContentType As String, Optional ByVal bUserQuery As Boolean = False)
            PerfMon.Log("Search", "RegExpQuery")
            Dim sXpath As String = ""
            Dim aSearchWords() As String
            Dim aNewSearchWords() As String = Nothing
            Dim i As Integer
            Dim sError As String = ""
            Dim cSearchTerm As String = ""
            Dim bFirst As Boolean = True
            Dim bSearchSubmitted As Boolean = False
            Dim oRegEx As Regex
            Dim sProcessInfo As String = "Search Routing"
            Dim nResultCount As Integer = 0
            Dim dtStart As Date = Now
            ' REGEXP Search

            ' Optional parameters (from the Web.config):
            '   ewSearchMatchWholeWords  - when set to "on", this will only match entered words, as opposed to substring-match (e.g. matching web in eonicweb)
            '   ewSearchRegExIncludeTags - experimental.  This tries to replicate simple Xpath by only looking at what's in certain tags.
            '                               This should be a pipe separated list (i.e. h1|h2|h3)

            bSearchSubmitted = True

            Dim cIDs As String = ""
            Dim cSearchWhereCONTENT As String = ""
            Dim cSearchWhereUSER As String = ""
            Dim cRegEx As String = ""
            Dim cRegExPattern As String = ""
            Dim reMasterCheck As Regex
            Dim cSql As String = ""
            Dim aSearchTerms As Collection = New Collection
            Dim bFullMatch As Boolean = True
            Dim cSearchVariant As String


            Dim cRegEx_TagStartPattern As String = ""
            Dim cRegEx_TagEndPattern As String = ""
            Try
                ' If ewSearchRegExIncludeTags is initiated then, we need to create a regex to match 
                ' the opening tag and closing tag (e.g. Match <a> and </a>).  These will be added to the 
                ' each of the search terms, making a very large regex.

                If moConfig("ewSearchRegExIncludeTags") <> "" Then
                    cRegEx_TagStartPattern = "(<(" & moConfig("ewSearchRegExIncludeTags") & ")[^>]*>).*?"
                    cRegEx_TagEndPattern = ".*?(<\/(" & moConfig("ewSearchRegExIncludeTags") & ")>)"
                End If

                'remove any single quotes to prevent injection attacks
                cContentType = Replace(cContentType, "'", "")
                ' Change contentType from CSV to CSV with quotes!
                cContentType = Replace(cContentType, ",", "','")
                cContentType = "'" & cContentType & "'"


                ' Note that the logic for searching is (Word 1 OR a variant of it) AND (Word 2 OR a variant of it)

                ' Two things are being constructed here:
                ' An array of a word and its variants, which is then added to aSearchTerms
                ' A SQL statement, which will allow us to get anything that is likely any of the search words or their variants.
                aSearchWords = Split(CleanSearchString(sSearch), " ")
                For i = 0 To UBound(aSearchWords)
                    cSearchTerm = aSearchWords(i)
                    If Trim(cSearchTerm) <> "" Then

                        If bFirst Then
                            bFirst = Not (bFirst)
                        Else
                            cSearchWhereCONTENT = cSearchWhereCONTENT & " OR "
                            cSearchWhereUSER = cSearchWhereUSER & " OR "
                        End If



                        cRegEx = cSearchTerm
                        ' Note :: in the SQL statement below, the inclusion of [^<] is a token gesture to make sure we don't match to tags that begin with a search term, e.g. <Content and </Content etc...
                        cSearchWhereCONTENT &= " (cContentXMLBrief LIKE '%[^<]" & SqlFmt(cSearchTerm) & "%' AND cContentXMLBrief LIKE '%[^<][^/]" & SqlFmt(cSearchTerm) & "%') "
                        cSearchWhereCONTENT &= " or "
                        cSearchWhereCONTENT &= " (cContentXMLDetail LIKE '%[^<]" & SqlFmt(cSearchTerm) & "%' AND cContentXMLDetail LIKE '%[^<][^/]" & SqlFmt(cSearchTerm) & "%') "

                        cSearchWhereUSER &= " (cDirXml LIKE '%[^<]" & SqlFmt(cSearchTerm) & "%'  )"

                        If cSearchTerm.ToLower.EndsWith("s") Then
                            cSearchVariant = Left(cSearchTerm, Len(cSearchTerm) - 1)
                            cRegEx &= "|" & cSearchVariant
                            cSearchWhereCONTENT &= " OR (cContentXMLBrief LIKE '%[^<]" & SqlFmt(cSearchVariant) & "%' AND cContentXMLBrief LIKE '%[^<][^/]" & SqlFmt(cSearchVariant) & "%') "
                            cSearchWhereCONTENT &= " OR (cContentXMLDetail LIKE '%[^<]" & SqlFmt(cSearchVariant) & "%' AND cContentXMLDetail LIKE '%[^<][^/]" & SqlFmt(cSearchVariant) & "%') "

                            cSearchWhereUSER &= " OR  (cDirXml LIKE '%[^<]" & SqlFmt(cSearchVariant) & "%'  )"
                        End If
                        If aSearchWords(i).ToLower.EndsWith("ies") Then
                            cSearchVariant = Left(cSearchTerm, Len(cSearchTerm) - 3) & "y"
                            cRegEx &= "|" & cSearchVariant
                            cSearchWhereCONTENT &= " OR (cContentXMLBrief LIKE '%[^<]" & SqlFmt(cSearchVariant) & "%' AND cContentXMLBrief LIKE '%[^<][^/]" & SqlFmt(cSearchVariant) & "%') "
                            cSearchWhereCONTENT &= " OR (cContentXMLDetail LIKE '%[^<]" & SqlFmt(cSearchVariant) & "%' AND cContentXMLDetail LIKE '%[^<][^/]" & SqlFmt(cSearchVariant) & "%') "

                            cSearchWhereUSER &= " OR  (cDirXml LIKE '%[^<]" & SqlFmt(cSearchVariant) & "%'  )"
                        End If
                        If aSearchWords(i).ToLower.EndsWith("y") Then
                            cSearchVariant = Left(cSearchTerm, Len(cSearchTerm) - 1) & "ies"
                            cRegEx &= "|" & cSearchVariant
                            cSearchWhereCONTENT &= " OR (cContentXMLBrief LIKE '%[^<]" & SqlFmt(cSearchVariant) & "%' AND cContentXMLBrief LIKE '%[^<][^/]" & SqlFmt(cSearchVariant) & "%') "
                            cSearchWhereCONTENT &= " OR (cContentXMLDetail LIKE '%[^<]" & SqlFmt(cSearchVariant) & "%' AND cContentXMLDetail LIKE '%[^<][^/]" & SqlFmt(cSearchVariant) & "%') "


                            cSearchWhereUSER &= " OR (cDirXml LIKE '%[^<]" & SqlFmt(cSearchVariant) & "%'  )"
                        End If

                        ' Each word and its variants are put into a distinct array which is then added to a collection of search terms  
                        'aSearchTerms.Add(LCase(cRegEx).Split("|"))
                        If moConfig("ewSearchMatchWholeWords") = "on" Then
                            cRegEx = "\b(" & cRegEx & ")\b"
                        Else
                            cRegEx = "(" & cRegEx & ")"
                            'cRegEx = "(?>" & cRegEx & ")"
                        End If

                        cRegExPattern &= "(?=.*?" & cRegEx_TagStartPattern & cRegEx & cRegEx_TagEndPattern & ")"
                    End If
                Next


                If cRegExPattern <> "" Or bUserQuery Then
                    Dim oDr As SqlClient.SqlDataReader

                    ' Create a Reg Exp to strip out any tags
                    ' Pattern: "<", optionally followed by "/", followed by one or more occurences of any character that isn't ">", followed by ">"
                    oRegEx = New Regex("<\/?[^>]+>")

                    ' Critical for performance: this search matches the beginning (^) and end ($) of the string,
                    ' meaning that this phrase will only attempt the match once, rather than on every character
                    ' of the search string
                    cRegExPattern = "^" & cRegExPattern & ".*?$"

                    reMasterCheck = New Regex(cRegExPattern, RegexOptions.IgnoreCase Or RegexOptions.Compiled)

                    ' Get the SQL that will look for any of the search words or their variants

                    cSql = "SELECT nContentKey, cContentXmlBrief,  cContentXmlDetail, nContentPrimaryId, cContentName, cContentSchemaName " _
                        & " FROM tblContent " _
                        & " WHERE (" & cSearchWhereCONTENT & ")" & IIf(cContentType = "", "", " AND (cContentSchemaName IN (" & cContentType & "))")

                    Dim cResultIDsCSV As String = ""
                    If Not bUserQuery Then
                        oDr = myWeb.moDbHelper.getDataReader(cSql)

                        Dim ppppp As Integer = 0
                        While oDr.Read
                            Debug.WriteLine("Search loop " & ppppp)
                            ppppp += 1
                            'If moDbHelper.checkPagePermission(oDr("nContentPrimaryId")) = oDr("nContentPrimaryId") And ContentPagesLive(oDr("nContentKey")) Then

                            Dim cNewLineLessBrief As String = oDr("cContentXmlBrief")
                            Dim cNewLineLessDetail As String = oDr("cContentXmlDetail")
                            If cNewLineLessBrief <> Nothing Then cNewLineLessBrief = Replace(cNewLineLessBrief, Chr(10), "")
                            If cNewLineLessBrief <> Nothing Then cNewLineLessBrief = Replace(cNewLineLessBrief, Chr(13), "")
                            If cNewLineLessDetail <> Nothing Then cNewLineLessDetail = Replace(cNewLineLessDetail, Chr(10), "")
                            If cNewLineLessDetail <> Nothing Then cNewLineLessDetail = Replace(cNewLineLessDetail, Chr(13), "")

                            If (reMasterCheck.IsMatch(cNewLineLessBrief) Or reMasterCheck.IsMatch(cNewLineLessDetail)) Then
                                'If (reMasterCheck.IsMatch(oDr("cContentXmlBrief")) Or reMasterCheck.IsMatch(oDr("cContentXmlDetail"))) Then
                                'If reMasterCheck.IsMatch(oDr("cContentXmlDetail")) Then
                                cResultIDsCSV &= oDr("nContentKey") & ","
                            End If

                        End While
                        oDr.Close()
                        If cResultIDsCSV = "" Then cResultIDsCSV = "0"
                        nResultCount = GetContentXml(moContextNode, cResultIDsCSV)
                    Else
                        If myWeb.mnUserId = 0 Then Exit Sub

                        'users in all groups
                        cSql = "SELECT nDirKey, cDirSchema, cDirForiegnRef, cDirName, cDirXml" &
                        " FROM tblDirectory Users"

                        Dim cOverrideUserGroups As String
                        cOverrideUserGroups = moConfig("SearchAllUserGroups")
                        If Not cOverrideUserGroups Is Nothing And Not cOverrideUserGroups = "" Then
                            If Not cOverrideUserGroups = "on" Then
                                If cContentType = "" Then
                                    cSql &= "WHERE (cDirSchema = N'User') AND" &
                                    " (((SELECT TOP 1 DirRel1.nDirChildId AS OtherUser" &
                                    " FROM tblDirectory Dir INNER JOIN" &
                                    " tblDirectoryRelation DirRel ON Dir.nDirKey = DirRel.nDirParentId INNER JOIN" &
                                    " tblDirectoryRelation DirRel1 ON Dir.nDirKey = DirRel1.nDirParentId" &
                                    " WHERE (DirRel.nDirChildId = " & myWeb.mnUserId & ") AND (DirRel1.nDirChildId = Users.nDirKey))) IS NOT NULL)"
                                    cSql &= " And (cDirSchema = 'User')"
                                Else
                                    cSql &= " WHERE (cDirSchema = N'User') AND" &
                                    " (((SELECT TOP 1 DirRel1.nDirChildId AS OtherUser" &
                                    " FROM tblDirectory Dir INNER JOIN" &
                                    " tblDirectoryRelation DirRel ON Dir.nDirKey = DirRel.nDirParentId INNER JOIN" &
                                    " tblDirectoryRelation DirRel1 ON Dir.nDirKey = DirRel1.nDirParentId" &
                                    " WHERE (DirRel.nDirChildId = " & myWeb.mnUserId & ") AND (DirRel1.nDirChildId = Users.nDirKey) AND (Dir.cDirName = '" & cContentType & "'))) IS NOT NULL)"
                                    cSql &= " And (cDirSchema = 'User')"
                                End If
                            Else
                                If Not cContentType = "" And Not cContentType = "''" Then
                                    cSql &= " WHERE (cDirSchema = N'User') AND" &
                                    " (((SELECT nDirParentId" &
                                    " FROM tblDirectoryRelation " &
                                    " WHERE (nDirParentId = " & cContentType & ") AND (nDirChildId = Users.nDirKey))) IS NOT NULL)"
                                    cSql &= " And (cDirSchema = 'User')"
                                Else
                                    cSql &= " WHERE (cDirSchema = 'User')"
                                End If
                            End If
                        End If

                        If Not sSearch = "" Then
                            If Not cSql.Contains(" WHERE ") Then cSql &= " WHERE " Else cSql &= " AND "
                            cSql &= "(" & cSearchWhereUSER & " )"
                        End If


                        cSql &= " ORDER BY cDirName"
                        oDr = myWeb.moDbHelper.getDataReader(cSql)
                        While oDr.Read

                            Dim oContent As XmlElement = moContextNode.OwnerDocument.CreateElement("Content")
                            oContent.SetAttribute("id", oDr("nDirKey"))
                            oContent.SetAttribute("type", "User")
                            oContent.SetAttribute("name", oDr("cDirName"))
                            oContent.InnerXml = oDr("cDirXml")
                            If reMasterCheck.IsMatch(oContent.OuterXml) Then
                                nResultCount += 1
                                oContent.FirstChild.AppendChild(oContent.OwnerDocument.ImportNode(myWeb.moDbHelper.GetUserContactsXml(oDr("nDirKey")).CloneNode(True), True))
                                moContextNode.AppendChild(oContent)
                            End If
                        End While
                        oDr.Close()
                    End If

                    ' Log the search
                    If _logSearches Then
                        myWeb.moDbHelper.logActivity(dbHelper.ActivityType.Search, myWeb.mnUserId, 0, 0, nResultCount, sSearch)
                    End If

                    Dim oResXML As XmlElement = moPageXml.CreateElement("Content")
                    oResXML.SetAttribute("searchType", IIf(bUserQuery, "USER", "REGEX"))
                    oResXML.SetAttribute("SearchString", sSearch)
                    oResXML.SetAttribute("type", "SearchHeader")
                    oResXML.SetAttribute("Hits", nResultCount)
                    oResXML.SetAttribute("Time", Now.Subtract(dtStart).TotalMilliseconds)
                    oResXML.SetAttribute("resultIds", cResultIDsCSV)
                    moContextNode.AppendChild(oResXML)



                End If
            Catch ex As Exception
                returnException(myWeb.msException, mcModuleName, "RegExpQuery", ex, "", sProcessInfo, gbDebug)
            End Try
        End Sub

        Sub FullTextQuery(ByVal sSearch As String, ByVal cContentType As String, Optional ByVal bUserQuery As Boolean = False)
            PerfMon.Log("Search", "FullTextQuery")
            Dim sXpath As String = ""
            Dim aSearchWords() As String
            Dim aNewSearchWords() As String = Nothing
            Dim i As Integer
            Dim sError As String = ""
            Dim cSearchTerm As String = ""
            Dim bFirst As Boolean = True
            Dim bSearchSubmitted As Boolean = False
            Dim sProcessInfo As String = "Search Routing"
            Dim nResultCount As Integer = 0
            Dim dtStart As Date = Now

            bSearchSubmitted = True

            Dim cIDs As String = ""
            Dim cSearchWhereCONTENT As String = ""
            Dim cSearchWhereCONTENTforCode As String = ""
            Dim cSearchWhereUSER As String = ""
            Dim cRegEx As String = ""
            Dim cRegExPattern As String = ""
            Dim reMasterCheck As Regex
            Dim cSql As String = ""
            Dim aSearchTerms As Collection = New Collection
            Dim bFullMatch As Boolean = True

            Try

                'remove any single quotes to prevent injection attacks
                cContentType = Replace(cContentType, "'", "")
                ' Change contentType from CSV to CSV with quotes!
                cContentType = Replace(cContentType, ",", "','")
                cContentType = "'" & cContentType & "'"


                aSearchWords = Split(CleanSearchString(sSearch), " ")
                For i = 0 To UBound(aSearchWords)
                    cSearchTerm = aSearchWords(i)
                    If Trim(cSearchTerm) <> "" Then

                        If bFirst Then
                            bFirst = Not (bFirst)
                        Else
                            cSearchWhereCONTENT = cSearchWhereCONTENT & " AND "
                            cSearchWhereUSER = cSearchWhereUSER & " AND "
                        End If
                        cRegEx = cSearchTerm
                        cSearchWhereCONTENT = cSearchWhereCONTENT & " (CONTAINS(parentContent.cContentXmlDetail, '" & cSearchTerm & "') OR CONTAINS(parentContent.cContentXmlBrief, '" & cSearchTerm & "'))"
                    End If
                Next

                If cSearchWhereCONTENT <> "" Then
                    cSearchWhereCONTENT = " ( " & cSearchWhereCONTENT & " ) "
                    bFirst = True
                End If

                For i = 0 To UBound(aSearchWords)
                    cSearchTerm = aSearchWords(i)
                    If Trim(cSearchTerm) <> "" Then

                        If bFirst Then
                            bFirst = Not (bFirst)
                        Else
                            cSearchWhereCONTENTforCode = cSearchWhereCONTENTforCode & " AND "
                            cSearchWhereUSER = cSearchWhereUSER & " AND "
                        End If
                        cRegEx = cSearchTerm
                        cSearchWhereCONTENTforCode = cSearchWhereCONTENTforCode & " (CONTAINS(childContent.cContentXmlDetail, '" & cSearchTerm & "') OR CONTAINS(childContent.cContentXmlBrief, '" & cSearchTerm & "'))"
                    End If
                Next

                If cSearchWhereCONTENTforCode <> "" Then
                    cSearchWhereCONTENT = cSearchWhereCONTENT & " OR ( " & cSearchWhereCONTENTforCode & " ) "
                End If

                If cSearchWhereCONTENT <> "" Or bUserQuery Then
                    Dim oDr As SqlClient.SqlDataReader

                    cSql = "SELECT distinct  parentContent.nContentKey, Cast(parentContent.cContentXmlBrief as NVarchar(Max)) as cContentXmlBrief,  Cast(parentContent.cContentXmlDetail as NVarchar(Max)) as cContentXmlDetail, parentContent.nContentPrimaryId, parentContent.cContentName, parentContent.cContentSchemaName " _
                        & " FROM tblContentRelation r  
inner join tblContent parentContent on (r.nContentParentId = parentContent.nContentKey) " _
                        & "inner join tblContent childContent on (r.nContentChildId = childContent.nContentKey) " _
                        & " WHERE (" & cSearchWhereCONTENT & ")" & IIf(cContentType = "", "", " AND (parentContent.cContentSchemaName IN (" & cContentType & "))")

                    Dim cResultIDsCSV As String = ""
                    If Not bUserQuery Then
                        oDr = myWeb.moDbHelper.getDataReader(cSql)

                        Dim ppppp As Integer = 0
                        While oDr.Read
                            'Debug.WriteLine("Search loop " & ppppp)
                            'ppppp += 1

                            Dim cNewLineLessBrief As String = oDr("cContentXmlBrief")
                            Dim cNewLineLessDetail As String = oDr("cContentXmlDetail")

                            cResultIDsCSV &= oDr("nContentKey") & ","

                        End While
                        oDr.Close()
                        If cResultIDsCSV = "" Then cResultIDsCSV = "0"
                        nResultCount = GetContentXml(moContextNode, cResultIDsCSV)
                    Else
                        If myWeb.mnUserId = 0 Then Exit Sub

                        'users in all groups
                        cSql = "SELECT nDirKey, cDirSchema, cDirForiegnRef, cDirName, cDirXml" &
                        " FROM tblDirectory Users"

                        Dim cOverrideUserGroups As String
                        cOverrideUserGroups = moConfig("SearchAllUserGroups")
                        If Not cOverrideUserGroups Is Nothing And Not cOverrideUserGroups = "" Then
                            If Not cOverrideUserGroups = "on" Then
                                If cContentType = "" Then
                                    cSql &= "WHERE (cDirSchema = N'User') AND" &
                                    " (((SELECT TOP 1 DirRel1.nDirChildId AS OtherUser" &
                                    " FROM tblDirectory Dir INNER JOIN" &
                                    " tblDirectoryRelation DirRel ON Dir.nDirKey = DirRel.nDirParentId INNER JOIN" &
                                    " tblDirectoryRelation DirRel1 ON Dir.nDirKey = DirRel1.nDirParentId" &
                                    " WHERE (DirRel.nDirChildId = " & myWeb.mnUserId & ") AND (DirRel1.nDirChildId = Users.nDirKey))) IS NOT NULL)"
                                    cSql &= " And (cDirSchema = 'User')"
                                Else
                                    cSql &= " WHERE (cDirSchema = N'User') AND" &
                                    " (((SELECT TOP 1 DirRel1.nDirChildId AS OtherUser" &
                                    " FROM tblDirectory Dir INNER JOIN" &
                                    " tblDirectoryRelation DirRel ON Dir.nDirKey = DirRel.nDirParentId INNER JOIN" &
                                    " tblDirectoryRelation DirRel1 ON Dir.nDirKey = DirRel1.nDirParentId" &
                                    " WHERE (DirRel.nDirChildId = " & myWeb.mnUserId & ") AND (DirRel1.nDirChildId = Users.nDirKey) AND (Dir.cDirName = '" & cContentType & "'))) IS NOT NULL)"
                                    cSql &= " And (cDirSchema = 'User')"
                                End If
                            Else
                                If Not cContentType = "" And Not cContentType = "''" Then
                                    cSql &= " WHERE (cDirSchema = N'User') AND" &
                                    " (((SELECT nDirParentId" &
                                    " FROM tblDirectoryRelation " &
                                    " WHERE (nDirParentId = " & cContentType & ") AND (nDirChildId = Users.nDirKey))) IS NOT NULL)"
                                    cSql &= " And (cDirSchema = 'User')"
                                Else
                                    cSql &= " WHERE (cDirSchema = 'User')"
                                End If
                            End If
                        End If

                        If Not sSearch = "" Then
                            If Not cSql.Contains(" WHERE ") Then cSql &= " WHERE " Else cSql &= " AND "
                            cSql &= "(" & cSearchWhereUSER & " )"
                        End If


                        cSql &= " ORDER BY cDirName"
                        oDr = myWeb.moDbHelper.getDataReader(cSql)
                        While oDr.Read

                            Dim oContent As XmlElement = moContextNode.OwnerDocument.CreateElement("Content")
                            oContent.SetAttribute("id", oDr("nDirKey"))
                            oContent.SetAttribute("type", "User")
                            oContent.SetAttribute("name", oDr("cDirName"))
                            oContent.InnerXml = oDr("cDirXml")
                            If reMasterCheck.IsMatch(oContent.OuterXml) Then
                                nResultCount += 1
                                oContent.FirstChild.AppendChild(oContent.OwnerDocument.ImportNode(myWeb.moDbHelper.GetUserContactsXml(oDr("nDirKey")).CloneNode(True), True))
                                moContextNode.AppendChild(oContent)
                            End If
                        End While
                        oDr.Close()
                    End If

                    ' Log the search
                    If _logSearches Then
                        myWeb.moDbHelper.logActivity(dbHelper.ActivityType.Search, myWeb.mnUserId, 0, 0, nResultCount, sSearch)
                    End If

                    Dim oResXML As XmlElement = moPageXml.CreateElement("Content")
                    oResXML.SetAttribute("searchType", IIf(bUserQuery, "USER", "REGEX"))
                    oResXML.SetAttribute("SearchString", sSearch)
                    oResXML.SetAttribute("type", "SearchHeader")
                    oResXML.SetAttribute("Hits", nResultCount)
                    oResXML.SetAttribute("Time", Now.Subtract(dtStart).TotalMilliseconds)
                    oResXML.SetAttribute("resultIds", cResultIDsCSV)
                    moContextNode.AppendChild(oResXML)



                End If
            Catch ex As Exception
                returnException(myWeb.msException, mcModuleName, "RegExpQuery", ex, "", sProcessInfo, gbDebug)
            End Try
        End Sub


        ''' <summary>
        '''    This returns all valid content within a site for a daterange.
        '''    Parameters can be optionally called by unit (cUnit, cValue) or a specififed date range (cStartDate, cEndDate).
        ''' </summary>
        ''' <param name="cContentTypes">CSV list of content types to search for.</param>
        ''' <param name="cUnit">Date period to search by : values are "Year","Month","Week","Day"</param>
        ''' <param name="cValue">Number of units to search for</param>
        ''' <param name="cStartDate">If searching for specific dates, then set this as the start of the range.</param>
        ''' <param name="cEndDate">If searching for specific dates, then set this as the end of the range.</param>
        ''' <param name="cSqlColumnToCheck">Specifies the preference on whether to check the Publish, Insert or Update column (values: PUBLISH,INSERT,UPDATE)</param>
        ''' <remarks></remarks>
        Protected Sub ContentByDateQuery(ByVal cContentTypes As String, Optional ByVal cUnit As String = "", Optional ByVal cValue As String = "", Optional ByVal cStartDate As String = "", Optional ByVal cEndDate As String = "", Optional ByVal cSqlColumnToCheck As String = "PUBLISH")

            PerfMon.Log("Search", "LatestContentQuery")

            Dim dStart As Date
            Dim dEnd As Date

            Dim dtStart As Date = DateTime.Now()

            Dim cRangeDate_Start As Date = DateTime.Today()
            Dim cRangeDate_End As Date

            Dim cSql As String = ""
            Dim cSqlDateRange As String = ""
            Dim cSqlColumn As String = ""

            ' Retuns the number of results found.
            Dim nResultCount As Long = 0

            Try

                ' Test for a value
                If IsNumeric(cValue) Then

                    If cValue <> 0 Then

                        ' Valid value - now check the unit
                        Select Case cUnit
                            Case "Day"
                                cRangeDate_End = Today.AddDays(cValue)
                            Case "Week"
                                cRangeDate_End = Today.AddDays(cValue * 7)
                            Case "Month"
                                cRangeDate_End = Today.AddMonths(cValue)
                            Case "Year"
                                cRangeDate_End = Today.AddYears(cValue)

                        End Select

                        If cRangeDate_End <> DateTime.MinValue Then

                            If DateTime.Compare(cRangeDate_Start, cRangeDate_End) > 0 Then
                                dStart = cRangeDate_End
                                dEnd = cRangeDate_Start
                            Else
                                dStart = cRangeDate_Start
                                dEnd = cRangeDate_End
                            End If

                        End If

                    End If

                End If

                ' See if the previous code has returned any values
                If dStart = DateTime.MinValue Then
                    ' No values, let's see if any explicit dates have been requested.
                    If (IsDate(cStartDate) Or cStartDate.ToLower = "now") And (IsDate(cEndDate) Or cEndDate.ToLower = "now") Then
                        If cStartDate.ToLower = "now" Then dStart = DateTime.Today Else dStart = CDate(cStartDate)
                        If cEndDate.ToLower = "now" Then dEnd = DateTime.Today Else dEnd = CDate(cEndDate)
                    End If
                End If

                ' We should now have a date range to run the search against
                If dStart <> DateTime.MinValue And dEnd <> DateTime.MinValue Then

                    Dim cResultIDsCSV As String = ""
                    Dim oDr As Data.SqlClient.SqlDataReader

                    ' Set the column to search by
                    Select Case UCase(cSqlColumnToCheck)
                        Case "INSERT"
                            cSqlColumn = "dInsertDate"
                        Case "UPDATE"
                            cSqlColumn = "dUpdateDate"
                        Case Else
                            cSqlColumn = "dPublishDate"
                    End Select

                    ' Start search
                    dEnd = CDate(Format(dEnd, "dd-MMM-yyyy") & " 23:59:59")
                    cSqlDateRange = " sa." & cSqlColumn & " >=  " & Protean.Tools.Database.SqlDate(dStart)
                    cSqlDateRange &= " AND sa." & cSqlColumn & " <=  " & Protean.Tools.Database.SqlDate(dEnd, True)


                    ' Change contentType from CSV to CSV with quotes!
                    cContentTypes = Replace(cContentTypes, ",", "','")
                    cContentTypes = "'" & cContentTypes & "'"


                    ' Ensure we get Distinct values
                    cSql = "SELECT DISTINCT sc.nContentKey" _
                        & " FROM tblContent sc INNER JOIN tblAudit sa ON sc.nAuditId = sa.nAuditKey" _
                        & " WHERE (" & cSqlDateRange & ")" & IIf(cContentTypes = "", "", " AND (sc.cContentSchemaName IN (" & cContentTypes & "))")

                    oDr = myWeb.moDbHelper.getDataReader(cSql)
                    While oDr.Read
                        cResultIDsCSV &= oDr("nContentKey") & ","
                    End While
                    oDr.Close()


                    If cResultIDsCSV <> "" Then
                        ' Add the content
                        nResultCount = GetContentXml(moContextNode, cResultIDsCSV, "a." & cSqlColumn & " DESC")
                    End If
                End If


                Dim oResXML As XmlElement = moPageXml.CreateElement("Content")
                oResXML.SetAttribute("searchType", "LATESTCONTENT")
                oResXML.SetAttribute("contentType", cContentTypes)
                oResXML.SetAttribute("searchDateUnit", cUnit)
                oResXML.SetAttribute("searchDateUnitTotal", cValue)
                oResXML.SetAttribute("searchStartDate", cStartDate)
                oResXML.SetAttribute("searchEndDate", cEndDate)
                oResXML.SetAttribute("searchedRangeStart", IIf(dStart = DateTime.MinValue, "Could not evaluate", Tools.Xml.XmlDate(dStart)))
                oResXML.SetAttribute("searchedRangeEnd", IIf(dEnd = DateTime.MinValue, "Could not evaluate", Tools.Xml.XmlDate(dEnd)))
                oResXML.SetAttribute("type", "SearchHeader")
                oResXML.SetAttribute("Hits", nResultCount)
                oResXML.SetAttribute("Time", Now.Subtract(dtStart).TotalMilliseconds)
                moContextNode.AppendChild(oResXML)

            Catch ex As Exception
                returnException(myWeb.msException, "Search", "SearchLatestContent", ex, "", , gbDebug)
            End Try

        End Sub

        Public Overridable Sub BespokeQuery()
            'placeholder for overloads

        End Sub
#End Region

#Region "General Search Functions"
        ''' <summary>
        ''' This is not needed at the moment.
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub SetSearchTrackingCookie()
            PerfMon.Log("Search", "SetSearchTrackingCookie")
            Try

                ' Only need a cookie if not logged in
                If myWeb.moSession IsNot Nothing Then
                    ' Search tracking cookie
                    ' As the cookie is client readable, we should encrypt it in some way
                    ' therefore let's store The Activity Key + a hash of the date and time and session id

                    ' Get the last search for this session
                    Dim lastSearch As SqlDataReader = myWeb.moDbHelper.getDataReader("SELECT TOP 1 nActivityKey,dDatetime,cSessionId FROM tblActivityLog WHERE cSessionId = " & Tools.Database.SqlString(myWeb.moSession.SessionID) & " ORDER BY 1 DESC")
                    If lastSearch.Read Then

                        Dim cookieValue As String
                        cookieValue = lastSearch(0).ToString & "|"
                        cookieValue &= Tools.Text.AscString(Tools.Encryption.HashString(Format(lastSearch(1).ToString, "s") & lastSearch(2).ToString, Tools.Hash.Provider.Md5, False), "|")

                        Dim trackingCookie As New System.Web.HttpCookie("search", cookieValue)
                        trackingCookie.Expires = Now.AddDays(2)

                        myWeb.moResponse.Cookies.Remove("search")
                        myWeb.moResponse.Cookies.Add(trackingCookie)

                    End If
                    lastSearch.Close()
                    lastSearch = Nothing
                End If


            Catch ex As Exception
                returnException(myWeb.msException, mcModuleName, "SetSearchTrackingCookie", ex, "", "", gbDebug)
            End Try
        End Sub

        ''' <summary>
        ''' This takes a string and produces an array of the keywords and phrases, where keywords are individual words,
        ''' and phrases are contained within quotemarks.   Phrases take precedence over keywords.
        ''' </summary>
        ''' <param name="searchString">The search string to parse</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function ParseKeywordsAndPhrases(ByVal searchString As String) As String()
            PerfMon.Log("Search", "ParseKeywordsAndPhrases")
            Dim processInfo As String = "Processing: " & searchString

            Try

                Dim terms As New ArrayList()
                Dim term As New StringBuilder
                Dim foundOpeningQuoteMark As Boolean = False
                Dim whiteSpaceAtBeginning As Boolean = True

                For Each character As Char In searchString

                    If character = " " And whiteSpaceAtBeginning Then
                        ' Ignore whitespace at the start of the search substring

                    ElseIf character = """" Then

                        ' If a quotemark, then add it and work out whether to add it as a term
                        term.Append(character)

                        If foundOpeningQuoteMark Then

                            ' This is the second quote - we have found a phrase
                            terms.Add(term.ToString)

                            foundOpeningQuoteMark = False
                            whiteSpaceAtBeginning = True
                            term = New StringBuilder

                        Else

                            ' This is the first quote - flag it up
                            foundOpeningQuoteMark = True
                            whiteSpaceAtBeginning = False
                        End If

                    ElseIf character = " " And Not foundOpeningQuoteMark Then

                        ' For whitespace in normal circumstances, add the term
                        terms.Add(term.ToString)
                        whiteSpaceAtBeginning = True
                        term = New StringBuilder

                        ' add something for the last term

                    Else

                        ' Add the character
                        term.Append(character)
                        whiteSpaceAtBeginning = False
                    End If

                Next

                ' Add the last term
                If Not String.IsNullOrEmpty(term.ToString.Trim) Then
                    terms.Add(term.ToString)
                End If

                ' Add the terms as a strign array
                Return CType(terms.ToArray(GetType(String)), String())

            Catch ex As Exception

                returnException(myWeb.msException, mcModuleName, "ParseKeywordsAndPhrases", ex, "", processInfo, gbDebug)

                Dim defaultString(1) As String
                defaultString(0) = searchString
                Return defaultString
            End Try

        End Function

        Public Function CleanSearchString(ByVal cSearchString As String, Optional ByVal cReservedWords As String = "a|an|for|with|the|and|if|then|or|from|where", Optional ByVal cPunctuation As String = ",-.&+:;""") As String
            PerfMon.Log("Search", "CleanSearchString")
            Try
                ' General RegEx Object
                Dim reGeneral As Regex = New Regex(RegexOptions.IgnoreCase)
                ' RegEx to search for special characters in the punctuation list variable
                Dim reSpecialChars As Regex = New Regex("([\[\\\^\$\.\|\?\*\+\(\)])")
                ' Remove Reserved Words - replace with a space
                cSearchString = Replace(cSearchString, "\b(" & cReservedWords & ")\b", " ")
                ' Remove certain punctuation
                cSearchString = Replace(cSearchString, "[" & reSpecialChars.Replace(cPunctuation, "\$1") & "]", " ")
                ' Remove any repeated whitespace
                cSearchString = Replace(cSearchString, "\s+", " ")
                ' Remove any doublequotes
                cSearchString = Replace(cSearchString, """", "")
                ' Finally Trim the WhiteSpace
                Return Trim(cSearchString)
            Catch
                Return ""
            End Try
        End Function

        Public Function GetContentXml(ByRef oContentElmt As XmlElement, ByVal nContentIds As String, Optional ByVal cSqlOrderClause As String = "type, cl.nDisplayOrder") As Integer
            PerfMon.Log("Search", "GetContentXml")
            Dim sSql As String
            Dim sProcessInfo As String = "building the Content XML"
            Dim nResultCount As Integer = 0
            If VB.Left(nContentIds, 1) = "," Then nContentIds = VB.Right(nContentIds, nContentIds.Length - 1)
            If VB.Right(nContentIds, 1) = "," Then nContentIds = VB.Left(nContentIds, nContentIds.Length - 1)

            Try
                'sSql = "select c.nContentKey as id, (select TOP 1 CL2.nStructId from tblContentLocation CL2 where CL2.nContentId=c.nContentKey and CL2.bPrimary = 1) as parId, cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire from tblContent c inner join tblContentLocation CL on c.nContentKey = CL.nContentId inner join tblAudit a on c.nAuditId = a.nAuditKey"
                '              sSql = "select c.nContentKey as id, 'Search' as source, dbo.fxn_getContentParents(c.nContentKey) as parId, c.cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire from tblContent c inner join tblContentLocation CL on c.nContentKey = CL.nContentId inner join tblAudit a on c.nAuditId = a.nAuditKey"
                If myWeb.mbAdminMode Then
                    sSql = "select c.nContentKey as id,  dbo.fxn_getContentParents(c.nContentKey) as parId, c.cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire, a.nInsertDirId as owner from tblContent c inner join tblContentLocation CL on c.nContentKey = CL.nContentId inner join tblAudit a on c.nAuditId = a.nAuditKey"
                    sSql &= " where( C.nContentKey IN (" & nContentIds & ")"
                    sSql &= ") "
                    sSql &= "AND (( SELECT tblContentStructure.nStructKey FROM tblContentStructure INNER JOIN tblAudit ON tblContentStructure.nAuditId = tblAudit.nAuditKey"
                    sSql &= " WHERE (tblContentStructure.nStructKey = CL.nStructId)"
                    sSql &= ")) is not null"
                    sSql &= " order by " & cSqlOrderClause
                Else
                    sSql = "select c.nContentKey as id,  dbo.fxn_getContentParents(c.nContentKey) as parId, c.cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire, a.nInsertDirId as owner from tblContent c inner join tblContentLocation CL on c.nContentKey = CL.nContentId inner join tblAudit a on c.nAuditId = a.nAuditKey"
                    sSql &= " where( C.nContentKey IN (" & nContentIds & ")"
                    sSql &= " and nStatus = 1 "
                    sSql &= " and (dPublishDate is null or dPublishDate = 0 or dPublishDate <= " & Protean.Tools.Database.SqlDate(Now) & " )"
                    sSql &= " and (dExpireDate is null or dExpireDate = 0 or dExpireDate >= " & Protean.Tools.Database.SqlDate(Now) & " )"
                    sSql &= ") "
                    sSql &= "AND (( SELECT tblContentStructure.nStructKey FROM tblContentStructure INNER JOIN tblAudit ON tblContentStructure.nAuditId = tblAudit.nAuditKey"
                    sSql &= " WHERE (tblAudit.nStatus = 1)"
                    sSql &= " and (tblAudit.dPublishDate is null or tblAudit.dPublishDate = 0 or tblAudit.dPublishDate <= " & Protean.Tools.Database.SqlDate(Now) & " )"
                    sSql &= " and (tblAudit.dExpireDate is null or tblAudit.dExpireDate = 0 or tblAudit.dExpireDate >= " & Protean.Tools.Database.SqlDate(Now) & " )"
                    sSql &= " and (tblContentStructure.nStructKey = CL.nStructId)"
                    sSql &= ")) is not null"
                    sSql &= " order by " & cSqlOrderClause
                End If

                Dim oDs As DataSet = New DataSet
                oDs = myWeb.moDbHelper.GetDataSet(sSql, "Content", "Contents")

                nResultCount = oDs.Tables("Content").Rows.Count
                myWeb.moDbHelper.AddDataSetToContent(oDs, oContentElmt, myWeb.mnPageId, , "search")

                'need to remove duplicate items

                Dim oSplit() As String = Split(nContentIds, ",")
                Dim i As Integer
                Dim nRemoved As Integer = 0
                For i = 0 To UBound(oSplit)
                    Dim oElmts As XmlNodeList = oContentElmt.SelectNodes("Content[@id=" & oSplit(i) & "]")
                    Dim nCount As Integer = 0
                    Dim oElmt As XmlElement
                    For Each oElmt In oElmts
                        If nCount > 0 Then
                            oElmt.ParentNode.RemoveChild(oElmt)
                            nRemoved += 1
                        End If
                        nCount += 1
                    Next
                Next
                nResultCount -= nRemoved

                Return nResultCount
            Catch ex As Exception
                returnException(myWeb.msException, mcModuleName, "getContentXml", ex, , sProcessInfo, gbDebug)
                Return nResultCount
            End Try

        End Function

#End Region

#Region "Index Query Functions"
        ''' <summary>
        ''' <para>Builds a Lucene Query Parser based on the keyword string and predefined values in the request object</para>
        ''' <list>
        ''' <listheader>The keyword search does the following queries (in order of priority)</listheader>
        ''' <item>The ss in quotemarks (i.e. as a phrase)</item>
        ''' <item>The ss without quotemarks (i.e. as it comes)</item>
        ''' <item>Each term in ss as a fuzzy search</item>
        ''' </list>
        ''' 
        ''' </summary>
        ''' <param name="keywordsToSearch"></param>
        ''' <param name="filters"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function BuildLuceneQuery(ByVal keywordsToSearch As String, ByVal filters As XmlElement) As Lucene.Net.Search.Query

            PerfMon.Log("Search", "BuildLuceneQuery")
            Dim processInfo As String = "Looking for : " & keywordsToSearch

            Dim fieldValue As String = ""
            Dim fieldName As String = ""
            Dim fieldTextTerms As String() = Nothing
            Dim fieldMin As String = ""
            Dim fieldMax As String = ""

            Try

                Dim analyzer As New StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29)
                Dim parser As New QueryParser(Lucene.Net.Util.Version.LUCENE_29, "text", analyzer)
                Dim queryBuilder As New BooleanQuery()
                Dim queryToBeParsed As New StringBuilder
                Dim queryTerms As String() = Nothing
                Dim keywords As String = ""

                ' Tidy up the searchString
                keywords = CleanUpLuceneSearchString(keywordsToSearch, Not _overrideQueryBuilder)

                ' Choose if a manual override has been set
                If _overrideQueryBuilder Then

                    queryToBeParsed.Append(keywords)

                Else

                    ' Build a prioritised query
                    ' We could build this through Lucene objects, or we could build this as a string
                    ' and let the QueryParser do the honours.

                    queryTerms = ParseKeywordsAndPhrases(keywords)

                    ' Prioritise the name field
                    BuildLuceneKeywordQuery(queryToBeParsed, queryTerms, "name", 3, _includeFuzzySearch)

                    ' Default field search
                    queryToBeParsed.Append(" OR ")
                    BuildLuceneKeywordQuery(queryToBeParsed, queryTerms, "", 1, _includeFuzzySearch)

                End If

                ' Prefix name search is an optional hardcoded search that prefix searches the qsname field
                ' Which represents an untokenized version of the name field.
                ' In other words if you include this option, you will need to update your index to 
                ' index the name field under qsname lowercased untokenized.
                ' e.g. <meta name="qsname" content="{$displayNameToLower}" tokenize="false" />
                Dim keywordQuery As Query
                If moConfig("SiteWildCardSearch") = "on" Then
                    parser.AllowLeadingWildcard = True
                    keywordQuery = parser.Parse($"*{keywordsToSearch}*")
                Else
                    keywordQuery = parser.Parse(queryToBeParsed.ToString())
                End If


                If _includePrefixNameSearch And Not _overrideQueryBuilder Then
                    Dim booleanQ As New BooleanQuery()
                    booleanQ.Add(keywordQuery, Occur.SHOULD)
                    booleanQ.Add(New PrefixQuery(New Term("qsname", keywords)), Occur.SHOULD)
                    queryBuilder.Add(booleanQ, Occur.MUST)
                Else
                    queryBuilder.Add(keywordQuery, Occur.MUST)
                End If


                ' Process any additional search criteria
                If filters IsNot Nothing Then
                    For Each searchFilter As XmlElement In filters

                        fieldName = searchFilter.GetAttribute("field")

                        Select Case searchFilter.GetAttribute("type")

                            Case "term"

                                ' Clean up the search terms
                                fieldValue = CleanUpLuceneSearchString(searchFilter.GetAttribute("value"))
                                fieldTextTerms = ParseKeywordsAndPhrases(fieldValue)

                                ' Field keyword search
                                queryToBeParsed = New StringBuilder()
                                BuildLuceneKeywordQuery(queryToBeParsed, fieldTextTerms, fieldName, 3, _includeFuzzySearch)
                                queryBuilder.Add(parser.Parse(queryToBeParsed.ToString()), Occur.MUST)


                            Case "range"

                                fieldMin = searchFilter.GetAttribute("min")
                                fieldMax = searchFilter.GetAttribute("max")

                                Select Case searchFilter.GetAttribute("fieldType")

                                    Case "number", "integer"
                                        Dim minNumber As Integer
                                        Dim maxNumber As Integer

                                        If Not String.IsNullOrEmpty(fieldMin) AndAlso IsNumeric(fieldMin) Then
                                            minNumber = Convert.ToInt16(fieldMin)
                                        Else
                                            minNumber = Integer.MinValue
                                        End If

                                        If Not String.IsNullOrEmpty(fieldMax) AndAlso IsNumeric(fieldMax) Then
                                            maxNumber = Convert.ToInt16(fieldMax)
                                        Else
                                            maxNumber = Integer.MaxValue
                                        End If
                                        Dim numericQuery As Query = NumericRangeQuery.NewIntRange(fieldName, minNumber, maxNumber, True, True)
                                        queryBuilder.Add(numericQuery, Occur.MUST)

                                    Case "float"
                                        Dim minNumber As Single
                                        Dim maxNumber As Single

                                        If Not String.IsNullOrEmpty(fieldMin) AndAlso IsNumeric(fieldMin) Then
                                            minNumber = Convert.ToSingle(fieldMin)
                                        Else
                                            minNumber = Single.MinValue
                                        End If

                                        If Not String.IsNullOrEmpty(fieldMax) AndAlso IsNumeric(fieldMax) Then
                                            maxNumber = Convert.ToSingle(fieldMax)
                                        Else
                                            maxNumber = Single.MaxValue
                                        End If
                                        Dim numericQuery As Query = NumericRangeQuery.NewFloatRange(fieldName, minNumber, maxNumber, True, True)
                                        queryBuilder.Add(numericQuery, Occur.MUST)

                                    Case Else

                                        Dim termQuery As New TermRangeQuery(fieldName, fieldMin, fieldMax, True, True)
                                        queryBuilder.Add(termQuery, Occur.MUST)


                                End Select
                        End Select

                    Next
                End If



                Return queryBuilder

            Catch ex As Exception
                returnException(myWeb.msException, mcModuleName, "BuildLuceneQuery", ex, "", processInfo, gbDebug)
                Return Nothing
            End Try

        End Function

        ''' <summary>
        ''' Returns a Lucene Filter of the live pages
        ''' 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' Justification for this is that Lucene may become out of date if a page is hidden. 
        ''' If we return a large result set, then we would have to trawl through all the results
        ''' checking if they on a live page = Fail.
        ''' It's quicker to create a live page filter here and filter that into lucene.
        ''' </remarks>
        Private Function LivePageLuceneFilter() As Filter

            Try

                ' Live page check - to avoid checking each result from EonicWeb, we factor the current live pages into the search.
                ' This means we can leverage a true search result set from Lucene and paging becomes a helluvalot easier

                Dim livePagesQuery As New BooleanQuery()
                Dim livePages As XmlNodeList = myWeb.moPageXml.SelectNodes("/Page/Menu/descendant-or-self::MenuItem")
                BooleanQuery.MaxClauseCount = Math.Max(BooleanQuery.MaxClauseCount, livePages.Count)
                For Each livePage As XmlElement In livePages
                    livePagesQuery.Add(New BooleanClause(New TermQuery(New Term("pgid", livePage.GetAttribute("id"))), Occur.SHOULD))
                Next

                Return New QueryWrapperFilter(livePagesQuery)

            Catch ex As Exception
                Return Nothing
            End Try

        End Function

        Private Function LivePageLuceneFilter(ByRef myApi As Protean.API) As Filter

            Try

                ' Live page check - to avoid checking each result from EonicWeb, we factor the current live pages into the search.
                ' This means we can leverage a true search result set from Lucene and paging becomes a helluvalot easier

                Dim livePagesQuery As New BooleanQuery()
                Dim myWeb As New Protean.Cms(myApi.moCtx)
                myWeb.Open()

                Dim siteStructure As XmlElement = myWeb.GetStructureXML(myWeb.mnUserId)
                Dim livePages As XmlNodeList = siteStructure.SelectNodes("*/descendant-or-self::MenuItem")
                BooleanQuery.MaxClauseCount = Math.Max(BooleanQuery.MaxClauseCount, livePages.Count)
                For Each livePage As XmlElement In livePages
                    livePagesQuery.Add(New BooleanClause(New TermQuery(New Term("pgid", livePage.GetAttribute("id"))), Occur.SHOULD))
                Next

                Return New QueryWrapperFilter(livePagesQuery)

            Catch ex As Exception
                Return Nothing
            End Try

        End Function


        Private Sub BuildLuceneKeywordQuery(ByRef queryBuilder As StringBuilder, ByVal keywords As String(), Optional ByVal fieldName As String = "", Optional ByVal boostBase As Integer = 1, Optional ByVal includeFuzzySearch As Boolean = False)

            PerfMon.Log("Search", "BuildLuceneKeywordQuery")
            Dim processInfo As String = ""
            Dim firstItem As Boolean = False

            Try
                ' Text query 1:
                ' Remove all quotes and put the whole thing in quotes
                If Not String.IsNullOrEmpty(fieldName) Then queryBuilder.Append(fieldName).Append(":")
                queryBuilder.Append("""")
                For Each keyword As String In keywords
                    queryBuilder.Append(keyword.Replace("""", ""))
                    queryBuilder.Append(" ")
                Next
                queryBuilder.Append("""^").Append((boostBase + 2).ToString)


                ' Text query 2 - as it comes:
                If keywords.Length > 1 Then
                    queryBuilder.Append(" OR (")
                    firstItem = True
                    For Each keyword As String In keywords
                        If Not firstItem Then
                            queryBuilder.Append(" AND ")
                        Else
                            firstItem = False
                        End If
                        If Not String.IsNullOrEmpty(fieldName) Then queryBuilder.Append(fieldName).Append(":")
                        queryBuilder.Append(keyword)
                        queryBuilder.Append("^").Append((boostBase + 1).ToString)
                    Next
                    queryBuilder.Append(") ")
                End If

                ' Text query 3 - fuzzy search:
                If keywords.Length > 0 And includeFuzzySearch Then
                    queryBuilder.Append(" OR (")
                    firstItem = True
                    For Each keyword As String In keywords
                        If Not firstItem Then
                            queryBuilder.Append(" AND ")
                        Else
                            firstItem = False
                        End If
                        If Not String.IsNullOrEmpty(fieldName) Then queryBuilder.Append(fieldName).Append(":")
                        queryBuilder.Append(keyword)
                        queryBuilder.Append("~^").Append((boostBase).ToString)
                    Next
                    queryBuilder.Append(") ")
                End If


            Catch ex As Exception
                returnException(myWeb.msException, mcModuleName, "BuildLuceneKeywordQuery", ex, "", processInfo, gbDebug)
            End Try

        End Sub


        Private Function SetSortFieldFromRequest(ByVal currentRequest As System.Web.HttpRequest) As Sort
            PerfMon.Log("Search", "SetSortFieldFromRequest")
            Dim processInfo As String = ""

            Try

                Dim sortFieldName As String = "" & currentRequest("sortCol")
                Dim sortDir As String = "" & currentRequest("sortDir")
                Dim sortFieldType As String = "" & currentRequest("sortColType")
                Dim sortOverridePrefixes As Boolean = (currentRequest("sortOverrideEWPrefixes") = "true")
                Dim returnedSort As New Sort

                If Not String.IsNullOrEmpty(sortFieldName) Then

                    If Not sortOverridePrefixes Then sortFieldName = "ewsort-" & sortFieldName

                    Dim fieldToSortType As Integer
                    Select Case sortFieldType.ToLower
                        Case "number", "float"
                            fieldToSortType = SortField.FLOAT
                        Case Else
                            fieldToSortType = SortField.STRING
                    End Select


                    returnedSort.SetSort(New SortField(sortFieldName, fieldToSortType, (sortDir.ToLower = "desc")))

                Else

                    ' Set the default sort order - relevance
                    returnedSort.SetSort(SortField.FIELD_SCORE)

                End If

                Return returnedSort

            Catch ex As Exception
                returnException(myWeb.msException, mcModuleName, "SetSortFieldFromRequest", ex, "", processInfo, gbDebug)
                Return Nothing
            End Try
        End Function

        Public Shared Function CleanUpLuceneSearchString(ByVal keywords As String, Optional ByVal escapeLuceneChars As Boolean = True) As String
            Try
                ' Tidy up 1: make sure we have an even number of quotes
                ' If not, remove the last quote
                ' e.g. from "My test" with this"
                '      to   "My test" with this
                If IsOdd(Regex.Matches(keywords, """").Count) Then
                    ' remove the last quote
                    keywords = Tools.Text.ReplaceLastCharacter(keywords, """"c, " ")
                End If

                ' Tidy up 2 : collapse the whitespace
                '             e.g. from "foo     foo" to "foo foo"
                keywords = Regex.Replace(keywords, "\s\s+", " ")

                ' Tidy up 3: escape special characters
                If escapeLuceneChars Then keywords = Regex.Replace(keywords, "[\\\+\-\!\(\)\{\}\[\]\^\~\*\?\:]", "\$0")

                Return keywords
            Catch ex As Exception

                Return ""
            End Try

        End Function

#End Region

    End Class

End Class
