Imports System.IO
Imports System.Xml
Imports System.Web
Imports System.DateTime
Public Class Calendar

#Region "Declarations"

    Public Shared Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
    Private Const mcModuleName As String = "Protean.Tools.Calendar"

    Private DateConfig As System.Globalization.DateTimeFormatInfo

    Private dRangeStart As Date
    Private dRangeEnd As Date




    Public Enum DatePeriod
        Year = 1
        Month = 2
        Week = 3
        Day = 4
    End Enum

#End Region

#Region "    Initialisation"

    Public Sub New(ByVal dStart_date As Date, ByVal dEnd_date As Date, Optional ByVal nFirstDayOfWeek As DayOfWeek = System.DayOfWeek.Sunday)
        DateConfig = New System.Globalization.DateTimeFormatInfo
        dRangeStart = dStart_date
        dRangeEnd = dEnd_date
        FirstDayOfWeek = nFirstDayOfWeek
    End Sub

#End Region

#Region "    Properties"

    Public Property FirstDayOfWeek() As DayOfWeek
        Get
            Return DateConfig.FirstDayOfWeek
        End Get
        Set(ByVal value As DayOfWeek)
            DateConfig.FirstDayOfWeek = value
        End Set
    End Property
#End Region

#Region "    Members"

    Public Function GetCalendarXML() As XmlElement

        Try

            Dim oXml As New XmlDocument
            Dim oCalendar As XmlElement = Nothing
            Dim oYear As XmlElement = Nothing
            Dim oMonth As XmlElement = Nothing
            Dim oWeek As XmlElement = Nothing
            Dim oDay As XmlElement = Nothing

            Dim dMonthCurrentDate As Date
            Dim dMonthLastDay As Date

            Dim dCurrentDate As Date = GetFirstDayOfMonth(Me.dRangeStart)

            Dim nDay As Long


            ' Set up the calendar XML node
            oCalendar = oXml.CreateElement("Calendar")

            ' Work out the actual range (i.e. the first weekday of the first week of the month,
            ' or the last weekday of the last week of the month)
            'dActualStartRange = GetFirstDayOfWeek(GetFirstDayOfMonth(dRangeStart))
            'dActualEndRange = 

            ' Iterate through the months
            Do While dCurrentDate <= Me.dRangeEnd

                ' Find or set up the year node
                If oYear Is Nothing Then
                    oYear = CreateDatePeriodElement(oCalendar, dCurrentDate, DatePeriod.Year)
                ElseIf oYear.GetAttribute("index") <> dCurrentDate.Year Then
                    oYear = CreateDatePeriodElement(oCalendar, dCurrentDate, DatePeriod.Year)
                End If

                ' Add the month node
                oMonth = CreateDatePeriodElement(oYear, dCurrentDate, DatePeriod.Month)

                ' Get the month start date
                dMonthCurrentDate = GetFirstDayOfWeek(GetFirstDayOfMonth(dCurrentDate))
                dMonthLastDay = GetLastDayOfMonth(dCurrentDate)

                ' Iterate through the weeks
                Do While dMonthCurrentDate <= dMonthLastDay
                    oWeek = CreateDatePeriodElement(oMonth, dMonthCurrentDate, DatePeriod.Week)
                    For nDay = 0 To 6
                        oDay = CreateDatePeriodElement(oWeek, dMonthCurrentDate.AddDays(nDay), DatePeriod.Day)
                        oDay.SetAttribute("day", DateConfig.DayNames(nDay))
                        oDay.SetAttribute("month", DateConfig.GetMonthName(dMonthCurrentDate.AddDays(nDay).Month))
                    Next

                    dMonthCurrentDate = dMonthCurrentDate.AddDays(7)
                Loop

                dCurrentDate = dCurrentDate.AddMonths(1)
            Loop

            Return oCalendar

        Catch ex As Exception
            RaiseEvent OnError(Nothing, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetCalendarXML", ex, ""))
            Return Nothing
        End Try

    End Function

    Private Function CreateDatePeriodElement(ByRef oParent As XmlElement, ByVal dDate As Date, ByVal nDatePeriod As Calendar.DatePeriod) As XmlElement

        Dim oElmt As XmlElement
        oElmt = oParent.OwnerDocument.CreateElement(nDatePeriod.ToString)

        ' Set the attributes
        Select Case nDatePeriod
            Case DatePeriod.Day
                oElmt.SetAttribute("dateid", dDate.ToString("yyyyMMdd"))
                oElmt.SetAttribute("index", dDate.Day)
            Case DatePeriod.Week
                'Get the Week by sepearting the Format by "DatePart" facility
                oElmt.SetAttribute("dateid", dDate.ToString("yyyy") & "" & DatePart("ww", dDate))
                oElmt.SetAttribute("index", DatePart("ww", dDate))
            Case DatePeriod.Month
                oElmt.SetAttribute("dateid", dDate.ToString("yyyyMM"))
                oElmt.SetAttribute("index", DateConfig.GetMonthName(dDate.Month))
                ' Added by requirement on 25/09/2008
                oElmt.SetAttribute("prevMonth", dDate.AddMonths(-1).Month.ToString("00"))
                oElmt.SetAttribute("nextMonth", dDate.AddMonths(+1).Month.ToString("00"))
                oElmt.SetAttribute("prev", DateConfig.GetMonthName(dDate.AddMonths(-1).Month))
                oElmt.SetAttribute("next", DateConfig.GetMonthName(dDate.AddMonths(+1).Month))
                ' Added by requirement on 27/09/2008
                oElmt.SetAttribute("prevMonthYear", dDate.AddMonths(-1).Year)
                oElmt.SetAttribute("nextMonthYear", dDate.AddMonths(+1).Year)

            Case DatePeriod.Year
                oElmt.SetAttribute("dateid", dDate.ToString("yyyy"))
                oElmt.SetAttribute("index", dDate.Year)
        End Select

        oParent.AppendChild(oElmt)

        Return oElmt

    End Function

    Public Function GetFirstDayOfMonth(ByVal dDate As Date) As Date
        Return dDate.AddDays(-1 * (dDate.Day - 1))
    End Function

    Public Function GetLastDayOfMonth(ByVal dDate As Date) As Date
        Return GetFirstDayOfMonth(dDate).AddMonths(1).AddDays(-1)
    End Function

    Public Function GetFirstDayOfWeek(ByVal dDate As Date) As Date
        Return dDate.AddDays(-1 * ((dDate.DayOfWeek + 7 - DateConfig.FirstDayOfWeek) Mod 7))
    End Function

    Public Function GetNextMonth(ByVal dDate As Date) As Date

    End Function

    Public Function GetPrevMonth(ByVal dDate As Date) As Date

    End Function

#End Region

End Class

