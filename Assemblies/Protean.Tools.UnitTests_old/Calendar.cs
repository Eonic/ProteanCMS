using Microsoft.VisualStudio.TestTools.UnitTesting;
using Protean.Tools;
using System;
using System.Globalization;
using System.Xml;

namespace Protean.Tools.UnitTests
{
    [TestClass]
    public class CalendarTest
    {
        [TestMethod]
        public void GetCalendarXML_ReturnsXml_True()
        {
            DateTime dateNow = DateTime.Now;
            Calendar myCal = new Calendar(dateNow, dateNow.AddYears(1));
            XmlElement xCal =  myCal.GetCalendarXML();
            Assert.IsInstanceOfType(xCal, typeof(XmlElement));
        }
        [TestMethod]
        public void GetFirstDayOfMonth_Returns_Date()
        {
            DateTime dateNow = DateTime.Now;
            Calendar myCal = new Calendar(dateNow, dateNow.AddYears(1));
            DateTime firstDay = myCal.GetFirstDayOfMonth(dateNow);
            Assert.IsInstanceOfType(firstDay, typeof(DateTime));
        }
    }
};
