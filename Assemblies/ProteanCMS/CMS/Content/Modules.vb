Option Strict Off
Option Explicit On

Imports System.Xml
Imports System
Imports System.Web.Configuration
Imports System.Data
Imports System.Data.SqlClient
Imports System.Reflection

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

        End Class

#End Region

        Protected Overrides Sub Finalize()
            MyBase.Finalize()
        End Sub
    End Class
End Class
