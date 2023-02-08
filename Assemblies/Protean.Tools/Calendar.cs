using System;
using System.Xml;
using System.Globalization;
namespace Protean.Tools
{
    public class Calendar
    {
        public static event ErrorEventHandler OnError;
        public delegate void ErrorEventHandler(object sender, Protean.Tools.Errors.ErrorEventArgs e);
        private const string mcModuleName = "Protean.Tools.Calendar";

        private DateTimeFormatInfo DateConfig;
        private DateTime dRangeStart;
        private DateTime dRangeEnd;

        public enum DatePeriod
        {
            Year = 1,
            Month = 2,
            Week = 3,
            Day = 4
        }

        public Calendar(DateTime dStart_date, DateTime dEnd_date, DayOfWeek nFirstDayOfWeek = System.DayOfWeek.Sunday)
        {
            DateConfig = new System.Globalization.DateTimeFormatInfo();
            dRangeStart = dStart_date;
            dRangeEnd = dEnd_date;
            FirstDayOfWeek = nFirstDayOfWeek;
        }

        public DayOfWeek FirstDayOfWeek
        {
            get
            {
                return DateConfig.FirstDayOfWeek;
            }
            set
            {
                DateConfig.FirstDayOfWeek = value;
            }
        }

        public XmlElement GetCalendarXML()
        {
            try
            {
                XmlDocument oXml = new XmlDocument();
                XmlElement oCalendar = null;
                XmlElement oYear = null;
                XmlElement oMonth = null;
                XmlElement oWeek = null;
                XmlElement oDay = null;

                DateTime dMonthCurrentDate;
                DateTime dMonthLastDay;

                DateTime dCurrentDate = GetFirstDayOfMonth(this.dRangeStart);

                long nDay;


                // Set up the calendar XML node
                oCalendar = oXml.CreateElement("Calendar");

                // Work out the actual range (i.e. the first weekday of the first week of the month,
                // or the last weekday of the last week of the month)
                // dActualStartRange = GetFirstDayOfWeek(GetFirstDayOfMonth(dRangeStart))
                // dActualEndRange = 

                // Iterate through the months
                while (dCurrentDate <= this.dRangeEnd)
                {

                    // Find or set up the year node
                    if (oYear == null)
                        oYear = CreateDatePeriodElement(ref oCalendar, dCurrentDate, DatePeriod.Year);
                    else if (!oYear.GetAttribute("index").Equals(dCurrentDate.Year))
                        oYear = CreateDatePeriodElement(ref oCalendar, dCurrentDate, DatePeriod.Year);

                    // Add the month node
                    oMonth = CreateDatePeriodElement(ref oYear, dCurrentDate, DatePeriod.Month);

                    // Get the month start date
                    dMonthCurrentDate = GetFirstDayOfWeek(GetFirstDayOfMonth(dCurrentDate));
                    dMonthLastDay = GetLastDayOfMonth(dCurrentDate);

                    // Iterate through the weeks
                    while (dMonthCurrentDate <= dMonthLastDay)
                    {
                        oWeek = CreateDatePeriodElement(ref oMonth, dMonthCurrentDate, DatePeriod.Week);
                        for (nDay = 0; nDay <= 6; nDay++)
                        {
                            oDay = CreateDatePeriodElement(ref oWeek, dMonthCurrentDate.AddDays(nDay), DatePeriod.Day);
                            oDay.SetAttribute("day", DateConfig.DayNames[nDay]);
                            oDay.SetAttribute("month", DateConfig.GetMonthName(dMonthCurrentDate.AddDays(nDay).Month));
                        }

                        dMonthCurrentDate = dMonthCurrentDate.AddDays(7);
                    }

                    dCurrentDate = dCurrentDate.AddMonths(1);
                }

                return oCalendar;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetCalendarXML", ex, ""));
                return null;
            }
        }

        private XmlElement CreateDatePeriodElement(ref XmlElement oParent, DateTime dDate, Calendar.DatePeriod nDatePeriod)
        {
            XmlElement oElmt;
            oElmt = oParent.OwnerDocument.CreateElement(nDatePeriod.ToString());

            // Set the attributes
            switch (nDatePeriod)
            {
                case DatePeriod.Day:
                    {
                        oElmt.SetAttribute("dateid", dDate.ToString("yyyyMMdd"));
                        oElmt.SetAttribute("index", dDate.Day.ToString());
                        break;
                    }

                case DatePeriod.Week:
                    {
                        int weekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dDate, CalendarWeekRule.FirstDay, DayOfWeek.Monday);


                        // Get the Week by sepearting the Format by "DatePart" facility
                        oElmt.SetAttribute("dateid", dDate.ToString("yyyy") + "" + weekNumber.ToString());
                        oElmt.SetAttribute("index", weekNumber.ToString());
                        break;
                    }

                case DatePeriod.Month:
                    {
                        oElmt.SetAttribute("dateid", dDate.ToString("yyyyMM"));
                        oElmt.SetAttribute("index", DateConfig.GetMonthName(dDate.Month));
                        // Added by requirement on 25/09/2008
                        oElmt.SetAttribute("prevMonth", dDate.AddMonths(-1).Month.ToString("00"));
                        oElmt.SetAttribute("nextMonth", dDate.AddMonths(+1).Month.ToString("00"));
                        oElmt.SetAttribute("prev", DateConfig.GetMonthName(dDate.AddMonths(-1).Month));
                        oElmt.SetAttribute("next", DateConfig.GetMonthName(dDate.AddMonths(+1).Month));
                        // Added by requirement on 27/09/2008
                        oElmt.SetAttribute("prevMonthYear", dDate.AddMonths(-1).Year.ToString());
                        oElmt.SetAttribute("nextMonthYear", dDate.AddMonths(+1).Year.ToString());
                        break;
                    }

                case DatePeriod.Year:
                    {
                        oElmt.SetAttribute("dateid", dDate.ToString("yyyy"));
                        oElmt.SetAttribute("index", dDate.Year.ToString());
                        break;
                    }
            }

            oParent.AppendChild(oElmt);

            return oElmt;
        }

        public DateTime GetFirstDayOfMonth(DateTime dDate)
        {
            return dDate.AddDays(-1 * (dDate.Day - 1));
        }

        public DateTime GetLastDayOfMonth(DateTime dDate)
        {
            return GetFirstDayOfMonth(dDate).AddMonths(1).AddDays(-1);
        }

        public DateTime GetFirstDayOfWeek(DateTime dDate)
        {
            return dDate.AddDays(-1 * ((dDate.DayOfWeek + 7 - DateConfig.FirstDayOfWeek) % 7));
        }

        //  public DateTime GetNextMonth(DateTime dDate)
        //  {

        //  }

        //  public DateTime GetPrevMonth(DateTime dDate)
        //  {
        //  }
    }

}
