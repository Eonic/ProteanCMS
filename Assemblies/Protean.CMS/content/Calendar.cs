using System;
using System.Xml;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using static Protean.Tools.Xml;

namespace Protean
{


    public partial class Cms
    {

        public class Calendar
        {

            #region    Error Handling
            public event OnErrorEventHandler OnError;

            public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs err);

            // Private Sub _OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs) Handles MyBase.OnError
            // RaiseEvent OnError(sender, e)
            // End Sub

            #endregion

            #region    Declarations

            private const string mcModuleName = "Eonic.Calendar";
            private System.Web.HttpContext moCtx = System.Web.HttpContext.Current;
            private Cms myWeb;
            private Cms.dbHelper moDB;
            private XmlDocument moPageXml;


            #endregion

            #region    Initialisation
            /// <summary>
            /// Creates a new Calendar Object
            /// </summary>
            /// <param name="aWeb">Eonic Web Parent Object</param>
            /// <remarks>Sets up a local DBhelper and moPageXml to use inhertited from Web</remarks>
            public Calendar(ref Cms aWeb)
            {
                aWeb.PerfMon.Log(mcModuleName, "New");
                try
                {
                    myWeb = aWeb;
                    moDB = myWeb.moDbHelper;
                    moPageXml = myWeb.moPageXml;
                }


                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""));
                }
            }
            #endregion

            #region    Properties

            #endregion

            #region    Methods


            /// <summary>
            /// Pull the calendar to the main content based on the inputs provided ,
            /// Searching next content and update the relevant nodes with appropritae contents
            /// </summary>
            /// <remarks>Use a For loop to looking the relvant nodes within the main content</remarks>
            public void apply()
            {

                myWeb.PerfMon.Log(mcModuleName, "apply");

                string cProcessInfo = "";

                try
                {

                    // Go through the content nodes of type Calendar
                    foreach (XmlElement oCalContent in moPageXml.SelectNodes("/Page/Contents/Content[@type='Calendar']"))
                    {


                        int cGetMonth = Conversions.ToInteger(oCalContent.SelectSingleNode("DisplaySettings/Months").InnerText);
                        bool bSDateAsToday = Conversions.ToBoolean(Interaction.IIf(oCalContent.SelectSingleNode("DisplaySettings/StartDateAsToday").InnerText == "true", true, false));
                        string cSDateinMonths = oCalContent.SelectSingleNode("DisplaySettings/StartDateInMonths").InnerText;
                        string sContentTypes = oCalContent.SelectSingleNode("ContentTypes").InnerText;
                        XmlElement xmloCalContent = oCalContent;
                        add(ref xmloCalContent, cGetMonth, bSDateAsToday, cSDateinMonths, sContentTypes);

                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "apply", ex, cProcessInfo));
                }

            }

            public void add(ref XmlElement xmlContentNode, int cMonthsToGet, bool bSDateAsToday, string cSDateinMonths, string sContentTypes)
            {

                myWeb.PerfMon.Log(mcModuleName, "add - start");
                string cProcessInfo = "";


                try
                {
                    // set up our variables & elements
                    string calendarCmd = myWeb.moRequest.QueryString["calcmd"];
                    string cMonthDate = myWeb.moRequest.QueryString["monthdate"]; // -- I don't know what this does.

                    DateTime dCalendarStart;
                    var dCalendarEnd = default(DateTime);

                    var xmlCalendarView = myWeb.moPageXml.CreateElement("CalendarView");
                    XmlElement xmlEventFlagInDay;


                    // get calendar start and end date
                    if (!string.IsNullOrEmpty(calendarCmd))
                    {
                        // first look for a calendarCmd 
                        dCalendarStart = firstOfMonth(stringToDate(calendarCmd));
                    }

                    else if (!string.IsNullOrEmpty(cMonthDate))
                    {
                        // next look for a cMonthDate 
                        dCalendarStart = firstOfMonth(stringToDate(cMonthDate));
                    }

                    else if (bSDateAsToday == true)
                    {
                        // take start date as 1st of current month
                        dCalendarStart = firstOfMonth(DateTime.Now);
                    }

                    else
                    {
                        // the start date is cSDateinMonths months ahead of the current date
                        dCalendarStart = DateAndTime.DateAdd(DateInterval.Month, Conversions.ToInteger(cSDateinMonths), dCalendarEnd);
                    }


                    // end date is always cGetMonths on from the start date
                    dCalendarEnd = DateAndTime.DateAdd(DateInterval.Month, cMonthsToGet, dCalendarStart);
                    dCalendarEnd = DateAndTime.DateAdd(DateInterval.Day, -1, dCalendarEnd);


                    // get xml for calendar element within these dates
                    var oCalendar = new Tools.Calendar(dCalendarStart, dCalendarEnd);
                    xmlCalendarView.InnerXml = oCalendar.GetCalendarXML().OuterXml;


                    // for each node in the calendar xml, add the events for that date
                    var xmlDays = xmlCalendarView.SelectNodes("/Calendar/Year/Month/Week/Day");
                    DateTime dCurrent;

                    foreach (XmlElement xmlDay in xmlDays)
                    {
                        dCurrent = stringToDate(xmlDay.GetAttribute("dateid"));
                        //dCurrent = dCurrent;
                        var xmlEventsToday = moPageXml.SelectNodes(getDateXpath(dCurrent, sContentTypes));

                        // intTest = xmlEventsToday.Count
                        foreach (XmlElement xmlEvent in xmlEventsToday)
                        {
                            var argoParent = xmlDay;
                            xmlEventFlagInDay = addElement(ref argoParent, "item");
                            // Dim xmlDayEvent As XmlElement = myWeb.moPageXml.CreateElement("item")
                            xmlEventFlagInDay.SetAttribute("contentid", xmlEvent.GetAttribute("id"));
                            xmlEventFlagInDay.SetAttribute("dateStart", xmlEvent.SelectSingleNode("StartDate").InnerText);
                            xmlEventFlagInDay.SetAttribute("dateEnd", xmlEvent.SelectSingleNode("EndDate").InnerText);
                        }
                    }


                    // add the result to the return xml
                    xmlContentNode.AppendChild(xmlCalendarView);
                }


                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "add", ex, cProcessInfo));
                }

            }



            private string dateToString(DateTime dInput)
            {
                string cProcessInfo = "";
                var strResult = new System.Text.StringBuilder();
                try
                {

                    strResult.Append(dInput.Year.ToString());
                    strResult.Append(addLeadingZero(dInput.Month.ToString()));
                    strResult.Append(addLeadingZero(dInput.Day.ToString()));

                    return strResult.ToString();
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "dateToString", ex, cProcessInfo));
                    return null;
                }

            }


            private DateTime firstOfMonth(DateTime dInput)
            {
                string cProcessInfo = "";
                try
                {
                    return Conversions.ToDate("01" + " " + DateAndTime.MonthName(dInput.Month) + " " + dInput.Year);
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "firstOfCurrentMonth", ex, cProcessInfo));
                }

                return default;
            }

            private DateTime stringToDate(string cInput)
            {
                string cProcessInfo = "";
                try
                {
                    var strDate = new System.Text.StringBuilder();

                    // day
                    if (cInput.Length == 8)
                    {
                        strDate.Append(cInput.Substring(6, 2));
                    }
                    else
                    {
                        // there is no date specified, assume the first
                        strDate.Append("01");
                    }
                    strDate.Append(" ");

                    strDate.Append(DateAndTime.MonthName(Conversions.ToInteger(cInput.Substring(4, 2)))); // month
                    strDate.Append(" ");
                    strDate.Append(cInput.Substring(0, 4)); // year

                    return Conversions.ToDate(strDate.ToString());
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "stringToDate", ex, cProcessInfo));
                }

                return default;

            }


            private string addLeadingZero(string CIn)
            {
                string cProcessInfo = "";
                try
                {
                    switch (CIn.Length)
                    {
                        case 0:
                            {
                                CIn = "00";
                                break;
                            }
                        case 1:
                            {
                                CIn = "0" + CIn;
                                break;
                            }
                    }

                    return CIn;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "addLeadingZero", ex, cProcessInfo));
                    return null;
                }

            }


            private string getDateXpath(DateTime dCurrent, string sContentType)
            {
                var strXpath = new System.Text.StringBuilder();
                string cProcessInfo = "getDateXpath";
                try
                {

                    strXpath.Append("/Page/Contents/Content[@type='" + sContentType + "'");
                    strXpath.Append(" and ");
                    strXpath.Append("number(translate(StartDate, '-', '')) <= " + dateToString(dCurrent));
                    strXpath.Append(" and ");
                    strXpath.Append("number(translate(EndDate, '-', '')) >= " + dateToString(dCurrent));
                    strXpath.Append("]");

                    return strXpath.ToString();
                }



                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getDateXpath", ex, cProcessInfo));
                    return null;
                }

            }



            // Public Sub add(ByRef oContentNode As XmlElement, ByVal cGetMonth As Integer, ByVal bSDateAsToday As Boolean, ByVal cSDateinMonths As String, ByVal sContentTypes As String)

            // myWeb.PerfMon.Log(mcModuleName, "add - start")

            // Dim cProcessInfo As String = ""
            // Dim oCalContent As XmlElement
            // Dim dStart_date As Date
            // Dim dEnd_date As Date

            // Dim nCalElmtStart As Integer
            // Dim nCalElmtEnd As Integer

            // Dim nStartDate As Integer
            // Dim nEndDate As Integer

            // Dim oDay As XmlElement
            // Dim oItem As XmlElement


            // 'Getting QueryString for validate the dates according to that.
            // Dim calendarCmd As String = myWeb.moRequest.QueryString("calcmd")
            // Dim cMonthDate As String = myWeb.moRequest.QueryString("monthdate")

            // Try

            // ' Go through the content nodes of type Calendar
            // oCalContent = oContentNode
            // ' Process the calendar element
            // cProcessInfo = "Processing - Calendar ID:" & oCalContent.GetAttribute("id")

            // ' Calculate the appropriate start and end date

            // Dim cCurrentDate As Date = Protean.Tools.Xml.XmlDate(Now)

            // If cGetMonth = 0 Or cGetMonth < 0 Then
            // cGetMonth = 1 'By default 
            // End If

            // 'Input vaildations - Date verification
            // If bSDateAsToday Then ' Choose to begins with Current date
            // dStart_date = cCurrentDate 'Getting current date
            // Else
            // dStart_date = cCurrentDate.AddMonths(CInt(cSDateinMonths)) ' Getting date - later than current month                    End If
            // End If

            // ' Process calendar commands.
            // ' At the moment calendar commands can be Next / previous month or a specific yearmonth date
            // If calendarCmd <> "" Then

            // ' Determine if the command is a date
            // ' Should follow the format yyyymm
            // Dim commandIsDate As Boolean = Regex.IsMatch(calendarCmd, "\d{6}") _
            // AndAlso Convert.ToInt16(calendarCmd.Substring(4, 2)) > 0 _
            // AndAlso Convert.ToInt16(calendarCmd.Substring(4, 2)) < 13

            // Dim baseDate As String = IIf(commandIsDate, calendarCmd, cMonthDate)

            // Dim cMonthValue As String = baseDate.Substring(4, 2)
            // Dim cYearValue As String = baseDate.Substring(0, 4)
            // Dim dAvailableDate As Date

            // If IsNumeric(cYearValue) And IsNumeric(cMonthValue) _
            // AndAlso (CInt(cMonthValue) > 0 And CInt(cMonthValue) < 13) Then

            // Dim availableDay As Integer = Math.Min(dStart_date.Day, Date.DaysInMonth(cYearValue, cMonthValue))

            // dAvailableDate = New Date(CInt(cYearValue), CInt(cMonthValue), availableDay)

            // If calendarCmd = "nextMonth" Then
            // dStart_date = dAvailableDate.AddMonths(1)
            // dEnd_date = dStart_date

            // ElseIf calendarCmd = "prevMonth" Then
            // dStart_date = dAvailableDate.AddMonths(-1)
            // dEnd_date = dStart_date

            // ElseIf commandIsDate Then
            // dStart_date = dAvailableDate
            // dEnd_date = dStart_date
            // End If

            // End If

            // End If

            // If cGetMonth = 1 Then
            // dEnd_date = dStart_date
            // ElseIf cGetMonth > 1 Then ' avoiding adding extra month 
            // dEnd_date = dStart_date.AddMonths(cGetMonth - 1)
            // End If

            // ' Get the Calendar XML
            // myWeb.PerfMon.Log(mcModuleName, "GetCalendarXML - start")
            // Dim oCalendar As New Protean.Tools.Calendar(dStart_date, dEnd_date)

            // ' Add the node to oCalContent
            // Dim oCalendarelmt As XmlElement = oCalContent.OwnerDocument.CreateElement("CalendarView")
            // oCalendarelmt.InnerXml = oCalendar.GetCalendarXML.OuterXml
            // oCalContent.AppendChild(oCalendarelmt)
            // myWeb.PerfMon.Log(mcModuleName, "GetCalendarXML - start")                ' Find the visible start and end dates as unique IDs
            // Dim oDays As XmlNodeList = oCalendarelmt.SelectNodes("//Day")

            // If oDays.Count > 0 Then
            // nCalElmtStart = CInt("0" & oDays.Item(0).Attributes.GetNamedItem("dateid").Value)
            // nCalElmtEnd = CInt("0" & oDays.Item(oDays.Count - 1).Attributes.GetNamedItem("dateid").Value)


            // ' Now we have all the content on the Page, let's find content that's applicable
            // ' to this calendar and add the <item contentId="xxxx"/> pointer to the relevant <day> node


            // ' Turn the comma separated list of content types into an XPath
            // If sContentTypes <> "" Then
            // Dim cTypeXPath As String = "@type='" & Replace(sContentTypes, ",", "' or @type='") & "'"

            // ' Get the content for the available types.
            // myWeb.PerfMon.Log(mcModuleName, "loop - start")
            // For Each oContent As XmlElement In moPageXml.SelectNodes("/Page/Contents/Content[" & cTypeXPath & "]")

            // Dim cGetContentId As String = oContent.GetAttribute("id")
            // Dim sTemp As String = oContent.SelectSingleNode("StartDate").InnerText.ToString
            // myWeb.PerfMon.Log(mcModuleName, "process Content" & cGetContentId)
            // If cGetContentId = 186 Then
            // myWeb.PerfMon.Log(mcModuleName, "Our Pain")
            // End If

            // '  Get the content's date and scope
            // If IsDate(oContent.SelectSingleNode("StartDate").InnerText) _
            // AndAlso ((IsDate(oContent.SelectSingleNode("EndDate").InnerText)) _
            // Or (Tools.Xml.NodeState(oContent, "EndDate", , , , , , , True) <> Tools.Xml.XmlNodeState.HasContents)) Then

            // ' Find the content's start and end date
            // nStartDate = CInt(CDate(oContent.SelectSingleNode("StartDate").InnerText).ToString("yyyyMMdd"))
            // If Tools.Xml.NodeState(oContent, "EndDate", , , , , , , True) <> Tools.Xml.XmlNodeState.HasContents Then
            // nEndDate = nStartDate
            // Else
            // nEndDate = CInt(CDate(oContent.SelectSingleNode("EndDate").InnerText).ToString("yyyyMMdd"))
            // End If

            // ' Now adjust it for the visible calendar dates
            // nStartDate = Math.Max(nStartDate, nCalElmtStart)
            // nEndDate = Math.Min(nEndDate, nCalElmtEnd)

            // ' Run through the scope adding each day to the calendar
            // For nDateId As Integer = nStartDate To nEndDate

            // ' Find the day in the calendar
            // If Tools.Xml.NodeState(oCalendarelmt, "//Day[@dateid='" & nDateId & "']", , , , oDay) <> Tools.Xml.XmlNodeState.NotInstantiated Then
            // oItem = addElement(oDay, "item")
            // oItem.SetAttribute("contentid", cGetContentId)
            // End If

            // Next

            // End If

            // Next
            // myWeb.PerfMon.Log(mcModuleName, "loop - end")
            // End If

            // End If

            // myWeb.PerfMon.Log(mcModuleName, "add - end")

            // Catch ex As Exception
            // RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "add", ex, cProcessInfo))
            // End Try

            // End Sub



            #endregion

            public class Modules
            {

                public event OnErrorEventHandler OnError;

                public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs err);
                private const string mcModuleName = "Protean.Cms.Calendar.Modules";

                public void Add(ref Cms myWeb, ref XmlElement oContentNode)
                {
                    Calendar moCalendar;
                    //string sProcessInfo;
                    try
                    {

                       // string sProcessInfo = "Begin Calendar";
                        moCalendar = new Calendar(ref myWeb);

                        int cGetMonth = Conversions.ToInteger(oContentNode.GetAttribute("months"));
                        bool bSDateAsToday = Conversions.ToBoolean(Interaction.IIf(oContentNode.GetAttribute("startDateAsToday") == "true", true, false));
                        string cSDateinMonths = oContentNode.GetAttribute("startDateInMonths");
                        string cContentTypes = oContentNode.GetAttribute("contentTypes");

                        moCalendar.add(ref oContentNode, cGetMonth, bSDateAsToday, cSDateinMonths, cContentTypes);

                        moCalendar = null;
                        //sProcessInfo = "End Calendar";
                    }

                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "Add", ex, ""));
                    }
                }
            }
        }

    }
}