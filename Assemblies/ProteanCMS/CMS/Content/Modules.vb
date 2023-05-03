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


            Public Sub ListHistoricEvents(ByRef myWeb As Protean.Cms, ByRef oContentNode As XmlElement)
                Dim cProcessInfo As String = "ListHistoricEvents"
                Dim PageId As String = oContentNode.GetAttribute("grabberRoot")
                Dim nItemsPerPage As Long = 0
                Dim nCurrentPage As Long = 1

                Try
                    myWeb.GetPageContentFromSelect("CL.nStructId = " & PageId & " and a.dExpireDate < GETDATE() and c.cContentSchemaName = '" & oContentNode.GetAttribute("contentType") & "' ",
                    ,,, nItemsPerPage,,,,, nCurrentPage,,, True)

                    myWeb.bAllowExpired = True

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "ListHistoricEvents", ex, "", cProcessInfo, gbDebug)
                End Try
            End Sub


            Public Sub ContentFilter(ByRef myWeb As Protean.Cms, ByRef oContentNode As XmlElement)
                Dim cProcessInfo As String = "ContentFilter"
                Try
                    'current contentfilter id
                    Dim oFilterElmt As XmlElement
                    Dim formName As String = "ContentFilter"
                    'Dim cnt As Int16
                    Dim oFrmGroup As XmlElement
                    Dim filterForm As xForm = New xForm(myWeb)
                    Dim className As String = String.Empty
                    Dim oAdditionalFilterInput As New Hashtable()
                    filterForm.NewFrm(formName)
                    filterForm.submission(formName, "", "POST", "")

                    If (myWeb.moRequest.Form("clearfilters") IsNot Nothing) Then
                        If (Convert.ToString(myWeb.moRequest.Form("clearfilters")) = "clearfilters") Then

                            myWeb.moResponse.Redirect(myWeb.moRequest.RawUrl)

                        End If
                    End If
                    oFrmGroup = filterForm.addGroup(filterForm.moXformElmt, "main-group")


                    For Each oFilterElmt In oContentNode.SelectNodes("Content[@type='Filter' and @providerName!='']")

                        Dim calledType As Type
                        className = oFilterElmt.GetAttribute("className")
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

                                    calledType = assemblyInstance.GetType(ourProvider.parameters("rootClass") & "." & className, True)
                                End If
                            End If

                            Dim methodname As String = "AddControl"

                            Dim o As Object = Activator.CreateInstance(calledType)

                            Dim args(4) As Object
                            args(0) = myWeb
                            args(1) = oFilterElmt
                            args(2) = filterForm
                            args(3) = oFrmGroup
                            args(4) = oContentNode
                            'args(5) = filterTarget
                            calledType.InvokeMember(methodname, BindingFlags.InvokeMethod, Nothing, o, args)
                        End If

                    Next
                    oContentNode.AppendChild(filterForm.moXformElmt)

                    Dim whereSQL As String = ""

                    filterForm.addSubmit(oFrmGroup, "Show Experiences", "Show Experiences", "submit", "hidden-sm hidden-md hidden-lg filter-xs-btn showexperiences")
                    'filterForm.addSubmit(oFrmGroup, "Clear Filters", "Clear Filters", "submit", "ClearFilters")
                    filterForm.addValues()

                    If (filterForm.isSubmitted) Then

                        filterForm.updateInstanceFromRequest()
                        filterForm.validate()

                        If (filterForm.valid) Then



                            For Each oFilterElmt In oContentNode.SelectNodes("Content[@type='Filter' and @providerName!='']")

                                Dim calledType As Type
                                className = oFilterElmt.GetAttribute("className")
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

                                            calledType = assemblyInstance.GetType(ourProvider.parameters("rootClass") & "." & className, True)
                                        End If
                                    End If

                                    Dim methodname As String = "ApplyFilter"

                                    Dim o As Object = Activator.CreateInstance(calledType)

                                    Dim args(4) As Object
                                    args(0) = myWeb
                                    args(1) = whereSQL
                                    args(2) = filterForm
                                    args(3) = oFrmGroup
                                    args(4) = oFilterElmt

                                    whereSQL = Convert.ToString(calledType.InvokeMember(methodname, BindingFlags.InvokeMethod, Nothing, o, args))
                                End If

                            Next

                        End If
                    End If




                    ' now we go and get the results from the filter.
                    If (whereSQL <> String.Empty) Then
                        myWeb.moSession("FilterWhereCondition") = whereSQL
                        myWeb.GetPageContentFromSelect(whereSQL,,,,,, oContentNode,,,,, "Product")
                        'oContentNode.SetAttribute("resultCount", oContentNode.SelectNodes("Content[@type='Product']").Count)

                        If (oContentNode.SelectNodes("Content[@type='Product']").Count = 0) Then
                            filterForm.addSubmit(oFrmGroup, "Clear Filters", "No results found", "clearfilters", "clear-filters",, "clearfilters")
                        End If
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
        Public Function ApplyFilters(ByRef oXform As Protean.xForm, ByRef oContentNode As XmlElement) As String
            Try

                Dim className As String
                Dim cWhereSql As String
                For Each oFilterElmt As XmlNode In oContentNode.SelectNodes("Content[@type='Filter' and @providerName!='']")
                    className = oFilterElmt.Attributes("className").Value.ToString()

                    If myWeb.moRequest.Form(className) Is Nothing Then

                        If className = "PriceFilter" AndAlso myWeb.moRequest.Form("MaxPrice") <> "" AndAlso myWeb.moRequest.Form("MaxPrice") IsNot Nothing Then
                            Dim cSelectedMinPrice As String = ""
                            Dim cSelectedMaxPrice As String = ""
                            cSelectedMinPrice = Convert.ToString(oXform.Instance.SelectSingleNode("PriceFilter/@MinPrice").InnerText)
                            cSelectedMaxPrice = Convert.ToString(oXform.Instance.SelectSingleNode("PriceFilter/@MaxPrice").InnerText)
                            cWhereSql = cWhereSql & " and  nContentKey in ( Select distinct ci.nContentId from tblContentIndex ci inner join tblContentIndexDef cid on cid.nContentIndexDefKey=ci.nContentIndexDefinitionKey "
                            cWhereSql = cWhereSql & " inner join tblAudit ca on ca.nAuditKey=cid.nAuditId and nStatus=1 where cid.cDefinitionName='Price' AND ("
                            cWhereSql = cWhereSql & "ci.nNumberValue between " & cSelectedMinPrice & " and " & cSelectedMaxPrice & "))"
                        End If

                        If className = "AgeFilter" AndAlso myWeb.moRequest.Form("MaxAge") <> "" AndAlso myWeb.moRequest.Form("MaxAge") IsNot Nothing Then
                            Dim cDefMinName As String = "Age"
                            Dim cDefMaxName As String = "Max Age"
                            Dim nAgeMin As String = Convert.ToString(myWeb.moRequest.Form("MinAge"))
                            Dim nAgeMax As String = Convert.ToString(myWeb.moRequest.Form("MaxAge"))
                            Dim cAgeMinCond As String = "(ci.nNumberValue >= " & Convert.ToString(nAgeMin) & ")"
                            Dim cAgeMaxCond As String = "(ci.nNumberValue >= " & Convert.ToString(nAgeMin) & "  and ci.nNumberValue <= " + Convert.ToString(nAgeMax) & ") "
                            cWhereSql = cWhereSql & " and nContentKey in (Select  cr.nContentParentId from tblContentIndex ci  "
                            cWhereSql = cWhereSql & " inner join tblContentIndexDef cid on cid.nContentIndexDefKey=ci.nContentIndexDefinitionKey and cid.cDefinitionName in ('" & cDefMinName & "','" & cDefMaxName & "')"
                            cWhereSql = cWhereSql & " inner join tblContent cs on ci.nContentId=cs.nContentKey and cs.cContentSchemaName='SKU' inner join tblContentRelation cr on cr.nContentChildId=cs.nContentKey   inner join tblAudit acr on acr.nAuditKey=cr.nAuditId and acr.nStatus=1 "
                            cWhereSql = cWhereSql & "  where ci.nNumberValue!=0    And  " & cAgeMinCond & " union "
                            cWhereSql = cWhereSql & "  Select  cr.nContentParentId from tblContentIndex ci  "
                            cWhereSql = cWhereSql & " inner join tblContentIndexDef cid on cid.nContentIndexDefKey=ci.nContentIndexDefinitionKey and cid.cDefinitionName in ('" & cDefMinName & "','" & cDefMaxName & "')"
                            cWhereSql = cWhereSql & " inner join tblContent cs on ci.nContentId=cs.nContentKey and cs.cContentSchemaName='SKU' inner join tblContentRelation cr on cr.nContentChildId=cs.nContentKey  inner join tblAudit acr on acr.nAuditKey=cr.nAuditId and acr.nStatus=1 "
                            cWhereSql = cWhereSql & "  where ci.nNumberValue!=0    And " & cAgeMaxCond & " )"
                        End If

                        If className = "WeightFilter" AndAlso myWeb.moRequest.Form("To") <> "" AndAlso myWeb.moRequest.Form("To") IsNot Nothing Then
                            Dim cDefinitionName As String = "weight"
                            cWhereSql = cWhereSql & " and  nContentKey in ( Select distinct ci.nContentId from tblContentIndex ci inner join tblContentIndexDef cid on cid.nContentIndexDefKey=ci.nContentIndexDefinitionKey "
                            cWhereSql = cWhereSql & " inner join tblAudit ca on ca.nAuditKey=cid.nAuditId and nStatus=1 and cid.cDefinitionName='" & cDefinitionName & "'"
                            cWhereSql = cWhereSql & " And ci.nNumberValue between " + Convert.ToString(myWeb.moRequest.Form("From")) & " and " + Convert.ToString(myWeb.moRequest.Form("To")) & ")"
                        End If
                    Else

                        If className = "PageFilter" Then
                            cWhereSql = cWhereSql & " and nStructId IN(" + myWeb.moRequest.Form(className) & ")"
                        End If

                        If className = "OccasionFilter" Then
                            cWhereSql = cWhereSql & " and  ci.nContentId in (select nContentId from tblCartCatProductRelations c inner join tblAudit a on a.nAuditKey=c.nAuditId and nStatus=1"
                            cWhereSql = cWhereSql & " where c.nCatId in (" + myWeb.moRequest.Form(className) & ")) "
                        End If

                        If className = "OfferFilter" Then
                            cWhereSql = cWhereSql & " and  c.cContentSchemaName = 'Product' "
                            cWhereSql = cWhereSql & " and nContentKey in ("
                            cWhereSql = cWhereSql & " select distinct cr.nContentParentId from tblContent cn inner join tblContentRelation cr on cr.nContentParentId = cn.nContentKey and cn.cContentSchemaName = 'Product'"
                            cWhereSql = cWhereSql & " inner join tblAudit ac on ac.nAuditKey = cn.nAuditId and ac.nStatus = 1"
                            cWhereSql = cWhereSql & " inner join tblAudit ca on ca.nAuditKey = cr.nAuditId and ca.nStatus = 1"
                            cWhereSql = cWhereSql & " where cr.nContentParentId in "
                            cWhereSql = cWhereSql & " (select nContentId from tblCartCatProductRelations c inner join tblAudit a on a.nAuditKey=c.nAuditId and nStatus=1 where c.nCatId in (" + myWeb.moRequest.Form(className) & ")) "
                            cWhereSql = cWhereSql & " union "
                            cWhereSql = cWhereSql & " select  distinct cr.nContentParentId from tblContent cn inner join tblContentRelation cr on cr.nContentChildId = cn.nContentKey  and cn.cContentSchemaName = 'SKU' "
                            cWhereSql = cWhereSql & " inner join tblAudit sa on sa.nAuditKey = cn.nAuditId and sa.nStatus = 1 "
                            cWhereSql = cWhereSql & " inner join tblAudit sca on sca.nAuditKey = cr.nAuditId and sca.nStatus = 1 "
                            cWhereSql = cWhereSql & " where cr.nContentChildId in "
                            cWhereSql = cWhereSql & " (select nContentId from tblCartCatProductRelations c inner join tblAudit a on a.nAuditKey=c.nAuditId and nStatus=1 where c.nCatId in (" + myWeb.moRequest.Form(className) & ")) "
                            cWhereSql = cWhereSql & " )"
                        End If

                        'If className = "LocationFilter" AndAlso myWeb.moRequest.Form("Location") <> "" AndAlso myWeb.moRequest.Form("Location") IsNot Nothing Then
                        '    Dim cSelectedLocation As String = String.Empty
                        '    Dim cSelectedDistance As String = String.Empty
                        '    Dim Latitude As String = ""
                        '    Dim Longitude As String = ""
                        '    cSelectedLocation = myWeb.moRequest.Form("Location")
                        '    cSelectedDistance = myWeb.moRequest.Form("Distance")

                        '    If cSelectedLocation <> "" Then

                        '        Dim commonSvc = New commans
                        '        Dim offerDistance = New OfferDistance()

                        '        If cSelectedLocation.Contains(",") Then
                        '            cSelectedLocation = cSelectedLocation.Replace(",", "")
                        '        End If

                        '        offerDistance = commonSvc.GetPostcodeDetails(cSelectedLocation.Trim())

                        '        If offerDistance.Location IsNot Nothing Then
                        '            Latitude = offerDistance.Latitude
                        '            Longitude = offerDistance.Longitude
                        '        End If
                        '    End If

                        '    cWhereSql = cWhereSql & " and  nContentKey in (  select ncontentkey from tblContent tc left  join dbo.cfn_GetAllProductsDistance('" & Latitude & "','" & Longitude & "',0) pd on pd.nProductContentKey = tc.nContentKey "
                        '    cWhereSql = cWhereSql & " where tc.cContentSchemaName = 'Product' and    (pd.distance <=" & cSelectedDistance & ")) "
                        'End If
                    End If
                Next
            Catch ex As Exception
                Return ""
            End Try

        End Function

#End Region

        Protected Overrides Sub Finalize()
            MyBase.Finalize()
        End Sub
    End Class
End Class
