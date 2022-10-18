Option Strict Off
Option Explicit On

Imports System.Xml
Imports System
Imports System.Web.Configuration
Imports System.Data
Imports System.Data.SqlClient
Imports System.Reflection
Imports System.Collections.Generic

Partial Public Class Cms
    Public Class Content

#Region "Declarations"

        Dim myWeb As Protean.Cms
        Public Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
        Public Event OnErrorWithWeb(ByRef myweb As Protean.Cms, ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
        Private Const mcModuleName As String = "Protean.Cms.Content"

#End Region


#Region "Module Behaviour"

        Public Class Modules

            Public Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
            Private Const mcModuleName As String = "Protean.Cms.Content.Modules"

            Public Sub New()

                'do nowt

            End Sub

            Public Sub Filters(ByRef myWeb As Protean.Cms, ByRef oContentNode As XmlElement)
                Try

                Catch ex As Exception

                End Try
            End Sub

            Public Sub NewsByDate(ByRef myWeb As Protean.Cms, ByRef oContentNode As XmlElement)
                Try
                    Dim datestring As String = ""
                    Dim startDate As Date
                    Dim endDate As Nullable(Of Date) = Nothing
                    Dim dateQuery As String = ""
                    Dim cOrigUrl As String = myWeb.mcOriginalURL
                    Dim cOrigQS As String = ""
                    Dim cPageURL As String = myWeb.mcPagePath.TrimEnd("/")

                    Dim PageDate As DateTime = myWeb.mdDate

                    'Overide for testing
                    'PageDate = New Date(2016, 3, 15)

                    'handle querystrings
                    If myWeb.mcOriginalURL.Contains("?") Then
                        cOrigUrl = myWeb.mcOriginalURL.Split("?")(0)
                        cOrigQS = "?" & myWeb.mcOriginalURL.Split("?")(1)
                    End If

                    If InStr(cOrigUrl, "-/") > 0 Then
                        dateQuery = cOrigUrl.Substring(InStr(cOrigUrl, "-/") + 1)
                    End If

                    Dim thisDateQuery As String
                    'Week start date...
                    'Get ranges for articles on this page

                    Dim sFilterSql As String = myWeb.GetStandardFilterSQLForContent()
                    Dim sSql As String = "select  a.dpublishDate as publish from tblContent c" &
                    " inner join tblContentLocation CL on c.nContentKey = CL.nContentId" &
                    " inner join tblAudit a on c.nAuditId = a.nAuditKey" &
                    " where( CL.nStructId = " & myWeb.mnPageId
                    sSql = sSql & sFilterSql & " and c.cContentSchemaName = '" & oContentNode.GetAttribute("contentType") & "') order by a.dpublishDate desc"

                    Dim DateSet As DataSet = myWeb.moDbHelper.GetDataSet(sSql, "ArticleDates")
                    Dim dr As DataRow
                    Dim dEarliestDate As Date = PageDate
                    'Latest Articles
                    Dim nFirstPageCount As Integer = CInt("0" + oContentNode.GetAttribute("firstPageCount"))
                    Dim FirstPageLastDate As Date = Nothing
                    Dim counter As Long = nFirstPageCount

                    For Each dr In DateSet.Tables(0).Rows
                        If IsDate(dr("publish")) Then
                            If dr("publish") < dEarliestDate Then
                                dEarliestDate = dr("publish")
                            End If
                            counter = counter - 1
                            If counter = 0 Then
                                FirstPageLastDate = dr("publish")
                            End If
                        End If
                    Next

                    Dim mondayDate As Date = PageDate
                    While (mondayDate.DayOfWeek <> DayOfWeek.Monday)
                        mondayDate = mondayDate.AddDays(-1)
                    End While

                    Dim oContent As New ewXmlElement(oContentNode)
                    Dim NewMenu As ewXmlElement = oContent.AddElement("Menu")
                    Dim contentCount As Integer = 0
                    NewMenu.XmlElement.SetAttribute("id", "newsByDate")

                    If nFirstPageCount > 0 Then

                        thisDateQuery = "latest"
                        NewMenu.AddMenuItem("Latest Articles", thisDateQuery, cPageURL & "/" & oContentNode.GetAttribute("id") & "-/" & thisDateQuery & cOrigQS,,, contentCount)
                        If dateQuery = thisDateQuery Or dateQuery = "" Then
                            startDate = FirstPageLastDate
                            endDate = PageDate
                            dateQuery = thisDateQuery
                        End If
                    End If


                    'This Week
                    contentCount = getArticleCount(DateSet, mondayDate, PageDate)
                    If contentCount > 0 Then
                        thisDateQuery = "thisweek"
                        NewMenu.AddMenuItem("This Week", thisDateQuery, cPageURL & "/" & oContentNode.GetAttribute("id") & "-/" & thisDateQuery & cOrigQS,,, contentCount)
                        If dateQuery = thisDateQuery Or dateQuery = "" Then
                            startDate = mondayDate
                            endDate = PageDate
                            dateQuery = thisDateQuery
                        End If
                    End If

                    'Last Week
                    contentCount = getArticleCount(DateSet, mondayDate.AddDays(-8), mondayDate.AddDays(-1))
                    If contentCount > 0 Then
                        thisDateQuery = "lastweek"
                        NewMenu.AddMenuItem("Last Week", thisDateQuery, cPageURL & "/" & oContentNode.GetAttribute("id") & "-/" & thisDateQuery & cOrigQS,,, contentCount)
                        If dateQuery = thisDateQuery Or dateQuery = "" Then
                            startDate = mondayDate.AddDays(-8)
                            endDate = mondayDate.AddDays(-1)
                            dateQuery = thisDateQuery
                        End If
                    End If

                    'This Month
                    Dim firstDayMonth = New DateTime(PageDate.Year, PageDate.Month, 1)
                    contentCount = getArticleCount(DateSet, firstDayMonth, PageDate)
                    If contentCount > 0 Then
                        thisDateQuery = "thismonth"
                        NewMenu.AddMenuItem("This Month", thisDateQuery, cPageURL & "/" & oContentNode.GetAttribute("id") & "-/" & thisDateQuery & cOrigQS,,, contentCount)
                        If dateQuery = thisDateQuery Or dateQuery = "" Then
                            startDate = firstDayMonth
                            endDate = PageDate
                            dateQuery = thisDateQuery
                        End If
                    End If

                    'Step through this years months
                    Dim nPrevMonths As Integer = CInt("0" + oContentNode.GetAttribute("previousMonthsListed"))
                    If nPrevMonths = 0 Then nPrevMonths = 12
                    Dim thisCount As Integer = 1
                    Dim nThisMonth As Int16 = PageDate.Month - 1
                    Dim nThisYear As Int16 = PageDate.Year
                    Do While nThisMonth <> 0 And thisCount <= nPrevMonths
                        Dim firstDayloopMonth = New DateTime(PageDate.Year, nThisMonth, 1)
                        contentCount = getArticleCount(DateSet, firstDayloopMonth, dhLastDayInMonth(firstDayloopMonth))
                        If contentCount > 0 Then
                            thisDateQuery = nThisYear & "-" & nThisMonth
                            NewMenu.AddMenuItem(MonthName(nThisMonth) & " " & nThisYear, thisDateQuery, cPageURL & "/" & oContentNode.GetAttribute("id") & "-/" & thisDateQuery & cOrigQS,,, contentCount)
                            If dateQuery = thisDateQuery Or dateQuery = "" Then
                                startDate = firstDayloopMonth
                                endDate = dhLastDayInMonth(firstDayloopMonth)
                                dateQuery = thisDateQuery
                            End If
                        End If
                        nThisMonth = nThisMonth - 1
                        thisCount = thisCount + 1
                    Loop
                    nThisYear = nThisYear - 1

                    If nPrevMonths < 12 And nThisMonth > 0 Then
                        Dim lastMonthDate As New DateTime(PageDate.Year, nThisMonth, 1)
                        lastMonthDate = dhLastDayInMonth(lastMonthDate)
                        Dim firstDayYear = New DateTime(PageDate.Year, 1, 1)
                        contentCount = getArticleCount(DateSet, firstDayYear, lastMonthDate)
                        If contentCount > 0 Then
                            thisDateQuery = "restofyear"
                            NewMenu.AddMenuItem("Rest of " & PageDate.Year, thisDateQuery, cPageURL & "/" & oContentNode.GetAttribute("id") & "-/" & thisDateQuery & cOrigQS,,, contentCount)
                            If dateQuery = thisDateQuery Or dateQuery = "" Then
                                startDate = firstDayYear
                                endDate = lastMonthDate
                                dateQuery = thisDateQuery
                            End If
                        End If
                    End If


                    'Step through previous years 
                    Dim nYearOfOldestArticle As Int16 = dEarliestDate.Year
                    Do While nThisYear >= nYearOfOldestArticle
                        Dim firstDayloopYear = New DateTime(nThisYear, 1, 1)
                        Dim lastDayloopYear = New DateTime(nThisYear, 12, 31)
                        contentCount = getArticleCount(DateSet, firstDayloopYear, lastDayloopYear)
                        If contentCount > 0 Then
                            thisDateQuery = nThisYear
                            NewMenu.AddMenuItem(nThisYear, thisDateQuery, cPageURL & "/" & oContent.XmlElement.GetAttribute("id") & "-/" & thisDateQuery & cOrigQS,,, contentCount)
                            If dateQuery = thisDateQuery Or dateQuery = "" Then
                                startDate = firstDayloopYear
                                endDate = lastDayloopYear
                                dateQuery = thisDateQuery
                            End If
                        End If

                        nThisYear = nThisYear - 1
                    Loop

                    If myWeb.mbAdminMode And endDate = Date.Today Then
                        'Get content by date range and future posts
                        If startDate = DateTime.MinValue Then
                            myWeb.GetPageContentFromSelect("CL.nStructId = " & myWeb.mnPageId & " And c.cContentSchemaName = '" & oContentNode.GetAttribute("contentType") & "'")
                        Else
                            myWeb.GetPageContentFromSelect("CL.nStructId = " & myWeb.mnPageId & " And c.cContentSchemaName = '" & oContentNode.GetAttribute("contentType") & "' and a.dpublishDate >= " & sqlDate(startDate))
                        End If
                    Else
                        Dim endstr As String
                        If endDate Is Nothing Then
                            endstr = ""
                        Else
                            endstr = " and a.dpublishDate <= " & sqlDate(endDate)
                        End If

                        'Get content by date range
                        If startDate = DateTime.MinValue Then
                            myWeb.GetPageContentFromSelect("CL.nStructId = " & myWeb.mnPageId & " And c.cContentSchemaName = '" & oContentNode.GetAttribute("contentType") & "'" & endstr)
                        Else
                            myWeb.GetPageContentFromSelect("CL.nStructId = " & myWeb.mnPageId & " And c.cContentSchemaName = '" & oContentNode.GetAttribute("contentType") & "' and a.dpublishDate >= " & sqlDate(startDate) & endstr)
                        End If
                    End If

                    'remove content detail
                    If Not myWeb.moContentDetail Is Nothing Then
                        myWeb.moPageXml.DocumentElement.RemoveChild(myWeb.moPageXml.DocumentElement.SelectSingleNode("ContentDetail"))
                        myWeb.moContentDetail = Nothing
                        myWeb.moPageXml.DocumentElement.RemoveAttribute("artid")
                        myWeb.mnArtId = Nothing
                    End If
                    oContent.XmlElement.SetAttribute("dateQuery", dateQuery)
                    oContentNode = oContent.XmlElement

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Logon", ex, ""))
                End Try
            End Sub

            Function getArticleCount(ods As DataSet, startDate As Date, endDate As Date) As Integer
                Dim ReturnCount As Integer = 0
                Dim dr As DataRow
                For Each dr In ods.Tables(0).Rows
                    If IsDate(dr("publish")) Then
                        If dr("publish") >= startDate And dr("publish") <= endDate Then
                            ReturnCount = ReturnCount + 1
                        End If
                    End If
                Next
                Return ReturnCount
            End Function

            Function dhLastDayInMonth(dtmDate As Date) As Date
                Return DateSerial(Year(dtmDate), Month(dtmDate) + 1, 0)
            End Function

            Public Sub ProductStepper(ByRef myWeb As Protean.Cms, ByRef oContentNode As XmlElement)
                Try
                    Dim cOrigUrl As String = myWeb.mcOriginalURL
                    Dim cOrigQS As String = ""
                    Dim cPageURL As String = ""
                    If Not myWeb.mcPagePath Is Nothing Then
                        cPageURL = myWeb.mcPagePath.TrimEnd("/")
                    End If
                    Dim nItemsPerPage As Long = CLng(oContentNode.GetAttribute("stepCount"))
                    Dim nCurrentPage As Long = 1
                    Dim itemCount As Long = myWeb.moDbHelper.GetDataValue("select count(nContentKey) from tblContent c inner join tblContentLocation cl on c.nContentKey =  cl.nContentId
where cl.nStructId = " & myWeb.mnPageId)
                    oContentNode.SetAttribute("itemCount", itemCount)
                    If myWeb.moRequest("startPos" & oContentNode.GetAttribute("id")) <> "" Then
                        nCurrentPage = (CLng(myWeb.moRequest("startPos" & oContentNode.GetAttribute("id"))) / nItemsPerPage) + 1
                    End If
                    'handle querystrings
                    If myWeb.mcOriginalURL.Contains("?") Then
                        cOrigUrl = myWeb.mcOriginalURL.Split("?")(0)
                        cOrigQS = "?" & myWeb.mcOriginalURL.Split("?")(1)
                    End If

                    'Get content by date range
                    myWeb.GetPageContentFromSelect("CL.nStructId = " & myWeb.mnPageId & " And c.cContentSchemaName = '" & oContentNode.GetAttribute("contentType") & "' ",
                    ,,, nItemsPerPage,,,,, nCurrentPage)
                    'remove content detail
                    If Not myWeb.moContentDetail Is Nothing Then
                        myWeb.moPageXml.DocumentElement.RemoveChild(myWeb.moPageXml.DocumentElement.SelectSingleNode("ContentDetail"))
                        myWeb.moContentDetail = Nothing
                        myWeb.moPageXml.DocumentElement.RemoveAttribute("artid")
                        myWeb.mnArtId = Nothing
                    End If


                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Logon", ex, ""))
                End Try
            End Sub

            'need to remove
            'Public Sub ProductFilter(ByRef myWeb As Protean.Cms, ByRef oContentNode As XmlElement)

            '    Dim inputs() As String = {"PageFilter", "PriceFilter"}
            '    'Dim inputs() As String = {"ProductFilter"}
            '    Dim lstOfFilters As List(Of String) = New List(Of String)(inputs)

            '    Dim Filter As String = ""

            '    For Each Filter In lstOfFilters
            '        Dim formName As String = Filter 'oContentNode.GetAttribute("name")
            '        Dim oFrmGroup As XmlElement
            '        Dim filters As Object
            '        Try

            '            Dim filterForm As xForm = New xForm(myWeb)
            '            Dim oFrmInstance As XmlElement
            '            'Dim assemblyInstance As [Assembly] = [Assembly].Load(moPrvConfig.Providers(providerName).Type.ToString())
            '            'Dim calledType As Type
            '            'Dim classPath As String = moPrvConfig.Providers(providerName).Parameters("rootClass")
            '            'Dim methodName As String = "ProcessOrder"
            '            If formName = "PageFilter" Then
            '                filters = New Protean.Providers.Filter.PageFilter()
            '            ElseIf formName = "AgeFilter" Then
            '                filters = New Protean.Providers.Filter.AgeFilter()
            '            ElseIf formName = "PriceFilter" Then
            '                filters = New Protean.Providers.Filter.PriceFilter()
            '            ElseIf formName = "GroupSizeFilter" Then
            '                filters = New Protean.Providers.Filter.GroupSizeFilter()
            '            ElseIf formName = "ProductFilter" Then
            '                filters = New Protean.Providers.Filter.PageFilter()
            '            ElseIf formName = "OccasionFilter" Then
            '                filters = New Protean.Providers.Filter.OccasionFilter()
            '            End If

            '            filterForm.NewFrm(formName)

            '            filterForm.submission(formName, "", "POST", "return form_check(this);")

            '            oFrmGroup = filterForm.addGroup(filterForm.moXformElmt, formName + "Group", formName + "Group", "")
            '            If formName = "PageFilter" Then
            '                filterForm.addBind("PageFilter", "PageFilter")
            '            ElseIf formName = "AgeFilter" Then
            '                filterForm.addBind("AgeFilter", "AgeFilter")
            '            ElseIf formName = "PriceFilter" Then
            '                filterForm.addBind("PriceFilter", "PriceFilter")
            '            ElseIf formName = "GroupSizeFilter" Then
            '                filterForm.addBind("GroupSizeFilter", "GroupSizeFilter")
            '            ElseIf formName = "ProductFilter" Then
            '                filterForm.addBind("PageFilter", "PageFilter", "true()")
            '            ElseIf formName = "OccasionFilter" Then
            '                filterForm.addBind("PageFilter", "PageFilter")
            '            End If
            '            filters.AddControl(myWeb, myWeb.mnPageId, filterForm, oFrmGroup)
            '            oFrmGroup = filterForm.addGroup(filterForm.moXformElmt, "submit", "contentSubmit", "")
            '            oContentNode.AppendChild(filterForm.moXformElmt)
            '            If (myWeb.moRequest.Form("Submit") IsNot Nothing) Then
            '                If (myWeb.moRequest.Form("Submit").ToLower() <> "search") Then
            '                    filters.RemovePageFromFilter(myWeb, myWeb.moRequest.Form("Submit"))
            '                End If
            '            End If
            '            oFrmInstance = filterForm.Instance
            '            If (myWeb.moSession("PageIds") IsNot Nothing) Then
            '                Protean.Tools.Xml.addElement(oFrmInstance, formName, Convert.ToString(myWeb.moSession("PageIds")))
            '            Else
            '                Protean.Tools.Xml.addElement(oFrmInstance, formName)
            '            End If

            '            filterForm.Instance = oFrmInstance

            '            filterForm.addSubmit(oFrmGroup, "Search", "Search")
            '            filterForm.addValues()
            '            If (filterForm.isSubmitted) Then
            '                'If (filterForm.valid) Then
            '                filterForm.updateInstanceFromRequest()
            '                If formName = "ContentFilter" Then
            '                    filters.ApplyFilter(myWeb, myWeb.mnPageId, filterForm, oFrmGroup)
            '                End If


            '            End If

            '        Catch ex As Exception
            '              RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Logon", ex, ""))
            '        End Try
            '    Next
            'End Sub
            Public Sub ListHistoricEvents(ByRef myWeb As Protean.Cms, ByRef oContentNode As XmlElement)
                Dim cProcessInfo As String = "ListHistoricEvents"
                Dim PageId As String = oContentNode.GetAttribute("grabberRoot")
                Dim nItemsPerPage As Long = 0
                Dim nCurrentPage As Long = 1

                Try
                    myWeb.GetPageContentFromSelect("CL.nStructId = " & PageId & " and a.dExpireDate < GETDATE() and c.cContentSchemaName = '" & oContentNode.GetAttribute("contentType") & "' ",
                    ,,, nItemsPerPage,,,,, nCurrentPage,,, True)


                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "ListHistoricEvents", ex, "", cProcessInfo, gbDebug)
                End Try
            End Sub


            Public Sub ContentFilter(ByRef myWeb As Protean.Cms, ByRef oContentNode As XmlElement)
                Dim cProcessInfo As String = "ContentFilter"
                Try
                    'current contentfilter id

                    If (oContentNode.Attributes("resultCount") IsNot Nothing) Then

                    End If

                    'Dim nContentFilterId = myWeb.moPageXml.SelectSingleNode("Page/Contents/Content[@moduleType='ContentFilter']").Attributes(0).Value
                    Dim oFilterElmt As XmlElement
                    Dim formName As String = "ContentFilter"
                    Dim cnt As Int16
                    Dim oFrmGroup As XmlElement
                    Dim filterForm As xForm = New xForm(myWeb)

                    filterForm.NewFrm(formName)
                    filterForm.submission(formName, "", "POST", "return form_check(this);")

                    oFrmGroup = filterForm.addGroup(filterForm.moXformElmt, "main-group")


                    For Each oFilterElmt In oContentNode.SelectNodes("Content[@type='Filter' and @providerName!='']")

                        Dim calledType As Type
                        Dim className As String = oFilterElmt.GetAttribute("className")
                        Dim providerName As String = oFilterElmt.GetAttribute("providerName")

                        If className <> "" Then

                            If providerName = "" Or LCase(providerName) = "default" Then
                                providerName = "Protean.Providers.Filters." & className
                                calledType = System.Type.GetType(providerName, True)
                            Else
                                Dim castObject As Object = WebConfigurationManager.GetWebApplicationSection("protean/filterProviders")
                                Dim moPrvConfig As Protean.ProviderSectionHandler = castObject
                                Dim ourProvider As Object = moPrvConfig.Providers(providerName)
                                Dim assemblyInstance As [Assembly]

                                If ourProvider.parameters("path") <> "" Then
                                    assemblyInstance = [Assembly].LoadFrom(myWeb.goServer.MapPath(ourProvider.parameters("path")))
                                Else
                                    assemblyInstance = [Assembly].Load(ourProvider.Type)
                                End If
                                If ourProvider.parameters("rootClass") = "" Then
                                    calledType = assemblyInstance.GetType("Protean.Providers.Filters." & providerName, True)
                                Else
                                    'calledType = assemblyInstance.GetType(ourProvider.parameters("rootClass") & ".Providers.Messaging", True)
                                    calledType = assemblyInstance.GetType(ourProvider.parameters("rootClass") & "." & className, True)
                                End If
                            End If

                            Dim methodname As String = "AddControl"

                            Dim o As Object = Activator.CreateInstance(calledType)

                            Dim args(3) As Object
                            args(0) = myWeb
                            args(1) = oFilterElmt
                            args(2) = filterForm
                            args(3) = oFrmGroup

                            calledType.InvokeMember(methodname, BindingFlags.InvokeMethod, Nothing, o, args)
                        End If

                    Next
                    oContentNode.AppendChild(filterForm.moXformElmt)

                    Dim whereSQL As String = ""
                    filterForm.addSubmit(oFrmGroup, "Clear Filters", "Clear Filters", "submit", "ClearFilter")
                    filterForm.addSubmit(oFrmGroup, "Show Experiences", "Show Experiences", "submit", "ShowExperiences")

                    filterForm.addValues()

                    If (filterForm.isSubmitted) Then
                        If (myWeb.moSession("FilterApplied") IsNot Nothing) Then
                            If (myWeb.moSession("FilterApplied") = "true") Then
                                For Each oFilterElmt In oContentNode.SelectNodes("Content[@type='Filter' and @providerName!='']")
                                    Dim className As String = oFilterElmt.GetAttribute("className")

                                    If (myWeb.moSession(className) IsNot Nothing) Then
                                            Dim filterValue As String = Convert.ToString(myWeb.moSession(className))


                                            filterForm.Instance.SelectSingleNode(className).InnerText = Convert.ToString(myWeb.moSession(className))


                                            Dim cFilterIds As String = myWeb.moSession(className)
                                            Dim aFilterId() As String = cFilterIds.Split(",")
                                            For cnt = 0 To aFilterId.Length - 1 Step 1
                                                If (aFilterId(cnt) <> String.Empty) Then
                                                    If aFilterId(cnt) <> "" Then
                                                        filterForm.addSubmit(oFrmGroup, "Remove - " + className + "-" + aFilterId(cnt), aFilterId(cnt))
                                                    End If
                                                End If
                                            Next

                                    End If
                                Next
                            End If
                        End If
                        filterForm.updateInstanceFromRequest()
                        filterForm.validate()
                        If (filterForm.valid) Then


                            If (myWeb.moRequest.Form("Submit") IsNot Nothing) Then
                                If (Convert.ToString(myWeb.moRequest.Form("Submit")).Contains("Remove -")) Then
                                    Dim aFilter() As String = Convert.ToString(myWeb.moRequest.Form("Submit")).Split("-")
                                    If (aFilter.Length > 0) Then
                                        Dim sessionValue As String = myWeb.moSession(aFilter(1).Trim())
                                        sessionValue.Replace(aFilter(2).Trim(), "")
                                        myWeb.moSession(aFilter(1).Trim()) = sessionValue
                                    End If
                                End If
                            End If

                            For Each oFilterElmt In oContentNode.SelectNodes("Content[@type='Filter' and @providerName!='']")

                                Dim calledType As Type
                                Dim className As String = oFilterElmt.GetAttribute("className")
                                Dim providerName As String = oFilterElmt.GetAttribute("providerName")

                                If className <> "" Then
                                    If (myWeb.moRequest.Form("Submit") = "Remove Filter") Then
                                        myWeb.moSession.Remove(className)
                                    End If
                                    If providerName = "" Or LCase(providerName) = "default" Then
                                        providerName = "Protean.Providers.Filters." & className
                                        calledType = System.Type.GetType(providerName, True)
                                    Else
                                        Dim castObject As Object = WebConfigurationManager.GetWebApplicationSection("protean/filterProviders")
                                        Dim moPrvConfig As Protean.ProviderSectionHandler = castObject
                                        Dim ourProvider As Object = moPrvConfig.Providers(providerName)
                                        Dim assemblyInstance As [Assembly]

                                        If ourProvider.parameters("path") <> "" Then
                                            assemblyInstance = [Assembly].LoadFrom(myWeb.goServer.MapPath(ourProvider.parameters("path")))
                                        Else
                                            assemblyInstance = [Assembly].Load(ourProvider.Type)
                                        End If
                                        If ourProvider.parameters("rootClass") = "" Then
                                            calledType = assemblyInstance.GetType("Protean.Providers.Filters." & providerName, True)
                                        Else

                                            calledType = assemblyInstance.GetType(ourProvider.parameters("rootClass") & "." & className, True)
                                        End If
                                    End If

                                    Dim methodname As String = "ApplyFilter"

                                    Dim o As Object = Activator.CreateInstance(calledType)

                                    Dim args(3) As Object
                                    args(0) = myWeb
                                    args(1) = whereSQL
                                    args(2) = filterForm
                                    args(3) = oFrmGroup

                                    whereSQL = Convert.ToString(calledType.InvokeMember(methodname, BindingFlags.InvokeMethod, Nothing, o, args))
                                End If

                            Next

                        End If
                    End If




                    ' now we go and get the results from the filter.
                    If (whereSQL <> String.Empty) Then
                        myWeb.moSession("FilterApplied") = "true"
                        myWeb.GetPageContentFromSelect(whereSQL,,,,,, oContentNode,,,,, "Product")
                        oContentNode.SetAttribute("resultCount", oContentNode.SelectNodes("Content[@type='Product']").Count)

                    End If

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "ContentFilter", ex, "", cProcessInfo, gbDebug)
                End Try
            End Sub



            Public Sub Conditional(ByRef myWeb As Protean.Cms, ByRef oContentNode As XmlElement)
                Try
                    If Not myWeb.mbAdminMode Then
                        If Not myWeb.moRequest.QueryString.ToString().Contains(oContentNode.GetAttribute("querystringcontains")) Then
                            oContentNode.ParentNode.RemoveChild(oContentNode)
                        End If
                    End If


                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Logon", ex, ""))
                End Try
            End Sub

        End Class

#End Region

        Protected Overrides Sub Finalize()
            MyBase.Finalize()
        End Sub
    End Class
End Class
