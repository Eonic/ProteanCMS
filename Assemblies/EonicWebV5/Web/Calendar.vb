Imports System.Xml
Imports VB = Microsoft.VisualBasic
Imports System.Data
Imports System.Data.SqlClient
Imports System.Text.RegularExpressions
Imports System


Partial Public Class Web

    Public Class Calendar

#Region "   Error Handling"
        Public Shadows Event OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)

        'Private Sub _OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs) Handles MyBase.OnError
        '    RaiseEvent OnError(sender, e)
        'End Sub

#End Region

#Region "   Declarations"

        Shadows Const mcModuleName As String = "Eonic.Calendar"
        Private moCtx As System.Web.HttpContext = System.Web.HttpContext.Current
        Private myWeb As Eonic.Web
        Private moDB As Eonic.Web.dbHelper
        Private moPageXml As XmlDocument


#End Region

#Region "   Initialisation"
        ''' <summary>
        ''' Creates a new Calendar Object
        ''' </summary>
        ''' <param name="aWeb">Eonic Web Parent Object</param>
        ''' <remarks>Sets up a local DBhelper and moPageXml to use inhertited from Web</remarks>
        Public Sub New(ByRef aWeb As Eonic.Web)
            PerfMon.Log(mcModuleName, "New")
            Try
                myWeb = aWeb
                moDB = myWeb.moDbHelper
                moPageXml = myWeb.moPageXml


            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""))
            End Try
        End Sub
#End Region

#Region "   Properties"

#End Region

#Region "   Methods"


        ''' <summary>
        ''' Pull the calendar to the main content based on the inputs provided ,
        ''' Searching next content and update the relevant nodes with appropritae contents
        ''' </summary>
        ''' <remarks>Use a For loop to looking the relvant nodes within the main content</remarks>
        Public Sub apply()

            PerfMon.Log(mcModuleName, "apply")

            Dim cProcessInfo As String = ""
            Dim oCalContent As XmlElement

            Try

                ' Go through the content nodes of type Calendar
                For Each oCalContent In moPageXml.SelectNodes("/Page/Contents/Content[@type='Calendar']")


                    Dim cGetMonth As Integer = oCalContent.SelectSingleNode("DisplaySettings/Months").InnerText
                    Dim bSDateAsToday As Boolean = IIf(oCalContent.SelectSingleNode("DisplaySettings/StartDateAsToday").InnerText = "true", True, False)
                    Dim cSDateinMonths As String = oCalContent.SelectSingleNode("DisplaySettings/StartDateInMonths").InnerText
                    Dim sContentTypes As String = oCalContent.SelectSingleNode("ContentTypes").InnerText

                    add(oCalContent, cGetMonth, bSDateAsToday, cSDateinMonths, sContentTypes)

                Next

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "apply", ex, cProcessInfo))
            End Try

        End Sub

        Public Sub add(ByRef xmlContentNode As XmlElement, ByVal cMonthsToGet As Integer, ByVal bSDateAsToday As Boolean, ByVal cSDateinMonths As String, ByVal sContentTypes As String)

            PerfMon.Log(mcModuleName, "add - start")
            Dim cProcessInfo As String = ""


            Try
                'set up our variables & elements
                Dim calendarCmd As String = myWeb.moRequest.QueryString("calcmd")
                Dim cMonthDate As String = myWeb.moRequest.QueryString("monthdate") '-- I don't know what this does.

                Dim dCalendarStart As Date
                Dim dCalendarEnd As Date

                Dim xmlCalendarView As XmlElement = myWeb.moPageXml.CreateElement("CalendarView")
                Dim xmlDay As XmlElement
                Dim xmlEvent As XmlElement
                Dim xmlEventFlagInDay As XmlElement


                'get calendar start and end date
                If Not String.IsNullOrEmpty(calendarCmd) Then
                    'first look for a calendarCmd 
                    dCalendarStart = firstOfMonth(stringToDate(calendarCmd))

                ElseIf Not String.IsNullOrEmpty(cMonthDate) Then
                    'next look for a cMonthDate 
                    dCalendarStart = firstOfMonth(stringToDate(cMonthDate))

                ElseIf bSDateAsToday = True Then
                    'take start date as 1st of current month
                    dCalendarStart = firstOfMonth(Date.Now)

                Else
                    'the start date is cSDateinMonths months ahead of the current date
                    dCalendarStart = DateAdd(DateInterval.Month, CInt(cSDateinMonths), dCalendarEnd)
                End If


                'end date is always cGetMonths on from the start date
                dCalendarEnd = DateAdd(DateInterval.Month, cMonthsToGet, dCalendarStart)
                dCalendarEnd = DateAdd(DateInterval.Day, -1, dCalendarEnd)


                'get xml for calendar element within these dates
                Dim oCalendar As New Eonic.Tools.Calendar(dCalendarStart, dCalendarEnd)
                xmlCalendarView.InnerXml = oCalendar.GetCalendarXML.OuterXml


                'for each node in the calendar xml, add the events for that date
                Dim xmlDays As XmlNodeList = xmlCalendarView.SelectNodes("/Calendar/Year/Month/Week/Day")
                Dim dCurrent As Date

                For Each xmlDay In xmlDays
                    dCurrent = stringToDate(xmlDay.GetAttribute("dateid"))
                    dCurrent = dCurrent
                    Dim xmlEventsToday As XmlNodeList = moPageXml.SelectNodes(getDateXpath(dCurrent, sContentTypes))

                    'intTest = xmlEventsToday.Count
                    For Each xmlEvent In xmlEventsToday
                        xmlEventFlagInDay = addElement(xmlDay, "item")
                        'Dim xmlDayEvent As XmlElement = myWeb.moPageXml.CreateElement("item")
                        xmlEventFlagInDay.SetAttribute("contentid", xmlEvent.GetAttribute("id"))
                        xmlEventFlagInDay.SetAttribute("dateStart", xmlEvent.SelectSingleNode("StartDate").InnerText)
                        xmlEventFlagInDay.SetAttribute("dateEnd", xmlEvent.SelectSingleNode("EndDate").InnerText)
                    Next
                Next


                'add the result to the return xml
                xmlContentNode.AppendChild(xmlCalendarView)


            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "add", ex, cProcessInfo))
            End Try

        End Sub



        Private Function dateToString(dInput As Date) As String
            Dim cProcessInfo As String = ""
            Try
                Dim strResult As New Text.StringBuilder
                strResult.Append(dInput.Year.ToString)
                strResult.Append(addLeadingZero(dInput.Month.ToString))
                strResult.Append(addLeadingZero(dInput.Day.ToString))

                Return strResult.ToString

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "dateToString", ex, cProcessInfo))
            End Try

        End Function


        Private Function firstOfMonth(ByVal dInput As Date) As Date
            Dim cProcessInfo As String = ""
            Try
                Return CDate("01" & " " & MonthName(dInput.Month) & " " & dInput.Year)
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "firstOfCurrentMonth", ex, cProcessInfo))
            End Try
        End Function

        Private Function stringToDate(cInput As String) As Date
            Dim cProcessInfo As String = ""
            Try
                Dim strDate As New Text.StringBuilder

                'day
                If cInput.Length = 8 Then
                    strDate.Append(cInput.Substring(6, 2))
                Else
                    'there is no date specified, assume the first
                    strDate.Append("01")
                End If
                strDate.Append(" ")

                strDate.Append(MonthName(cInput.Substring(4, 2))) 'month
                strDate.Append(" ")
                strDate.Append(cInput.Substring(0, 4)) ' year

                Return CDate(strDate.ToString)

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "stringToDate", ex, cProcessInfo))
            End Try

        End Function


        Private Function addLeadingZero(CIn As String) As String
            Dim cProcessInfo As String = ""
            Try
                Select Case CIn.Length
                    Case 0
                        CIn = "00"
                    Case 1
                        CIn = "0" & CIn
                End Select

                Return CIn

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "addLeadingZero", ex, cProcessInfo))
            End Try

        End Function


        Private Function getDateXpath(dCurrent As Date, sContentType As String) As String

            Dim cProcessInfo As String = ""
            Try
                Dim strXpath As New Text.StringBuilder
                strXpath.Append("/Page/Contents/Content[@type='" & sContentType & "'")
                strXpath.Append(" and ")
                strXpath.Append("number(translate(StartDate, '-', '')) <= " & dateToString(dCurrent))
                strXpath.Append(" and ")
                strXpath.Append("number(translate(EndDate, '-', '')) >= " & dateToString(dCurrent))
                strXpath.Append("]")

                Return strXpath.ToString



            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getDateXpath", ex, cProcessInfo))
            End Try

        End Function



        'Public Sub add(ByRef oContentNode As XmlElement, ByVal cGetMonth As Integer, ByVal bSDateAsToday As Boolean, ByVal cSDateinMonths As String, ByVal sContentTypes As String)

        '    PerfMon.Log(mcModuleName, "add - start")

        '    Dim cProcessInfo As String = ""
        '    Dim oCalContent As XmlElement
        '    Dim dStart_date As Date
        '    Dim dEnd_date As Date

        '    Dim nCalElmtStart As Integer
        '    Dim nCalElmtEnd As Integer

        '    Dim nStartDate As Integer
        '    Dim nEndDate As Integer

        '    Dim oDay As XmlElement
        '    Dim oItem As XmlElement


        '    'Getting QueryString for validate the dates according to that.
        '    Dim calendarCmd As String = myWeb.moRequest.QueryString("calcmd")
        '    Dim cMonthDate As String = myWeb.moRequest.QueryString("monthdate")

        '    Try

        '        ' Go through the content nodes of type Calendar
        '        oCalContent = oContentNode
        '        ' Process the calendar element
        '        cProcessInfo = "Processing - Calendar ID:" & oCalContent.GetAttribute("id")

        '        ' Calculate the appropriate start and end date

        '        Dim cCurrentDate As Date = Eonic.Tools.Xml.XmlDate(Now)

        '        If cGetMonth = 0 Or cGetMonth < 0 Then
        '            cGetMonth = 1 'By default 
        '        End If

        '        'Input vaildations - Date verification
        '        If bSDateAsToday Then ' Choose to begins with Current date
        '            dStart_date = cCurrentDate 'Getting current date
        '        Else
        '            dStart_date = cCurrentDate.AddMonths(CInt(cSDateinMonths)) ' Getting date - later than current month                    End If
        '        End If

        '        ' Process calendar commands.
        '        ' At the moment calendar commands can be Next / previous month or a specific yearmonth date
        '        If calendarCmd <> "" Then

        '            ' Determine if the command is a date
        '            ' Should follow the format yyyymm
        '            Dim commandIsDate As Boolean = Regex.IsMatch(calendarCmd, "\d{6}") _
        '                AndAlso Convert.ToInt16(calendarCmd.Substring(4, 2)) > 0 _
        '                AndAlso Convert.ToInt16(calendarCmd.Substring(4, 2)) < 13

        '            Dim baseDate As String = IIf(commandIsDate, calendarCmd, cMonthDate)

        '            Dim cMonthValue As String = baseDate.Substring(4, 2)
        '            Dim cYearValue As String = baseDate.Substring(0, 4)
        '            Dim dAvailableDate As Date

        '            If IsNumeric(cYearValue) And IsNumeric(cMonthValue) _
        '                AndAlso (CInt(cMonthValue) > 0 And CInt(cMonthValue) < 13) Then

        '                Dim availableDay As Integer = Math.Min(dStart_date.Day, Date.DaysInMonth(cYearValue, cMonthValue))

        '                dAvailableDate = New Date(CInt(cYearValue), CInt(cMonthValue), availableDay)

        '                If calendarCmd = "nextMonth" Then
        '                    dStart_date = dAvailableDate.AddMonths(1)
        '                    dEnd_date = dStart_date

        '                ElseIf calendarCmd = "prevMonth" Then
        '                    dStart_date = dAvailableDate.AddMonths(-1)
        '                    dEnd_date = dStart_date

        '                ElseIf commandIsDate Then
        '                    dStart_date = dAvailableDate
        '                    dEnd_date = dStart_date
        '                End If

        '            End If

        '        End If

        '        If cGetMonth = 1 Then
        '            dEnd_date = dStart_date
        '        ElseIf cGetMonth > 1 Then ' avoiding adding extra month 
        '            dEnd_date = dStart_date.AddMonths(cGetMonth - 1)
        '        End If

        '        ' Get the Calendar XML
        '        PerfMon.Log(mcModuleName, "GetCalendarXML - start")
        '        Dim oCalendar As New Eonic.Tools.Calendar(dStart_date, dEnd_date)

        '        ' Add the node to oCalContent
        '        Dim oCalendarelmt As XmlElement = oCalContent.OwnerDocument.CreateElement("CalendarView")
        '        oCalendarelmt.InnerXml = oCalendar.GetCalendarXML.OuterXml
        '        oCalContent.AppendChild(oCalendarelmt)
        '        PerfMon.Log(mcModuleName, "GetCalendarXML - start")                ' Find the visible start and end dates as unique IDs
        '        Dim oDays As XmlNodeList = oCalendarelmt.SelectNodes("//Day")

        '        If oDays.Count > 0 Then
        '            nCalElmtStart = CInt("0" & oDays.Item(0).Attributes.GetNamedItem("dateid").Value)
        '            nCalElmtEnd = CInt("0" & oDays.Item(oDays.Count - 1).Attributes.GetNamedItem("dateid").Value)


        '            ' Now we have all the content on the Page, let's find content that's applicable
        '            ' to this calendar and add the <item contentId="xxxx"/> pointer to the relevant <day> node


        '            ' Turn the comma separated list of content types into an XPath
        '            If sContentTypes <> "" Then
        '                Dim cTypeXPath As String = "@type='" & Replace(sContentTypes, ",", "' or @type='") & "'"

        '                ' Get the content for the available types.
        '                PerfMon.Log(mcModuleName, "loop - start")
        '                For Each oContent As XmlElement In moPageXml.SelectNodes("/Page/Contents/Content[" & cTypeXPath & "]")

        '                    Dim cGetContentId As String = oContent.GetAttribute("id")
        '                    Dim sTemp As String = oContent.SelectSingleNode("StartDate").InnerText.ToString
        '                    PerfMon.Log(mcModuleName, "process Content" & cGetContentId)
        '                    If cGetContentId = 186 Then
        '                        PerfMon.Log(mcModuleName, "Our Pain")
        '                    End If

        '                    '  Get the content's date and scope
        '                    If IsDate(oContent.SelectSingleNode("StartDate").InnerText) _
        '                        AndAlso ((IsDate(oContent.SelectSingleNode("EndDate").InnerText)) _
        '                        Or (Tools.Xml.NodeState(oContent, "EndDate", , , , , , , True) <> Tools.Xml.XmlNodeState.HasContents)) Then

        '                        ' Find the content's start and end date
        '                        nStartDate = CInt(CDate(oContent.SelectSingleNode("StartDate").InnerText).ToString("yyyyMMdd"))
        '                        If Tools.Xml.NodeState(oContent, "EndDate", , , , , , , True) <> Tools.Xml.XmlNodeState.HasContents Then
        '                            nEndDate = nStartDate
        '                        Else
        '                            nEndDate = CInt(CDate(oContent.SelectSingleNode("EndDate").InnerText).ToString("yyyyMMdd"))
        '                        End If

        '                        ' Now adjust it for the visible calendar dates
        '                        nStartDate = Math.Max(nStartDate, nCalElmtStart)
        '                        nEndDate = Math.Min(nEndDate, nCalElmtEnd)

        '                        ' Run through the scope adding each day to the calendar
        '                        For nDateId As Integer = nStartDate To nEndDate

        '                            ' Find the day in the calendar
        '                            If Tools.Xml.NodeState(oCalendarelmt, "//Day[@dateid='" & nDateId & "']", , , , oDay) <> Tools.Xml.XmlNodeState.NotInstantiated Then
        '                                oItem = addElement(oDay, "item")
        '                                oItem.SetAttribute("contentid", cGetContentId)
        '                            End If

        '                        Next

        '                    End If

        '                Next
        '                PerfMon.Log(mcModuleName, "loop - end")
        '            End If

        '        End If

        '        PerfMon.Log(mcModuleName, "add - end")

        '    Catch ex As Exception
        '        RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "add", ex, cProcessInfo))
        '    End Try

        'End Sub



#End Region

        Public Class Modules

            Public Event OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)
            Private Const mcModuleName As String = "Eonic.Web.Calendar.Modules"

            Public Sub Add(ByRef myWeb As Eonic.Web, ByRef oContentNode As XmlElement)
                Dim moCalendar As Eonic.Web.Calendar
                Dim sProcessInfo As String
                Try

                    sProcessInfo = "Begin Calendar"
                    moCalendar = New Eonic.Web.Calendar(myWeb)

                    Dim cGetMonth As Integer = oContentNode.GetAttribute("months")
                    Dim bSDateAsToday As Boolean = IIf(oContentNode.GetAttribute("startDateAsToday") = "true", True, False)
                    Dim cSDateinMonths As String = oContentNode.GetAttribute("startDateInMonths")
                    Dim cContentTypes As String = oContentNode.GetAttribute("contentTypes")

                    moCalendar.add(oContentNode, cGetMonth, bSDateAsToday, cSDateinMonths, cContentTypes)

                    moCalendar = Nothing
                    sProcessInfo = "End Calendar"

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "Add", ex, ""))
                End Try
            End Sub
        End Class
    End Class

End Class
