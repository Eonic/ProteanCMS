using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
    /// <summary>
///   <para>   Protean.Tools.Conversion is designed to covert from one data source to another (e.g. Excel to Xml)</para>
///   <example>
///     <para>Exmaple usage to convert an Excel file to XML</para>
///     <code>Dim c As New Conversion(Conversion.Type.Excel, Conversion.Type.Xml, "c:\text.xls")</code>
///     <code>c.Convert()</code>
///     <code>If c.State = Status.Succeeded Then Response.Write c.Output.OuterXml Else Response.Write c.Message</code>
///   </example>
/// </summary>
/// <remarks></remarks>
namespace Protean.Tools
{
    public class Conversion
    {

        #region     Declarations
        public static event OnErrorEventHandler OnError;
        public delegate void OnErrorEventHandler(object sender, Errors.ErrorEventArgs e);
        private const string mcModuleName = "Protean.Tools.Conversion";

        public enum Type
        {
            Undefined = 0,
            CSV = 1,
            Excel = 2,
            Xml = 3
        }

        public enum Status
        {
            Failed = 1,
            NotReady = 2,
            Ready = 100,
            Succeeded = 101
        }

        public enum StatusReason
        {
            Undefined = 0,
            InputAndOutputTypeCannotBeSame = 1,
            InputOrOutputTypeNotSet = 2,
            ConversionNotAvailable = 3,
            InputSourceNotValidorEmpty = 4
        }

        private Type nInputType = Type.Undefined;
        private Type nOutputType = Type.Undefined;
        private object oInput = null;
        private XmlElement oOutputXml = null;
        private object oOutput = null;
        private Status nStatus = Status.NotReady;
        private StatusReason nStatusReason = StatusReason.Undefined;
        public string writePath;
        public char CSVseparator = ',';
        // Public CSVfirstLineTitles As Boolean = True
        public string CSVfirstLineTitles;
        public string CSVLineName = "DataRow";

        #endregion

        #region     Properties

        public Status State
        {
            get
            {
                return nStatus;
            }
        }

        public StatusReason StateReason
        {
            get
            {
                return nStatusReason;
            }
        }

        public Type InputType
        {
            get
            {
                return nInputType;
            }

            set
            {
                nInputType = value;
                UpdateStatus();
            }
        }

        public Type OutputType
        {
            get
            {
                return nOutputType;
            }

            set
            {
                nOutputType = value;
                UpdateStatus();
            }
        }

        public object Input
        {
            get
            {
                return oInput;
            }

            set
            {
                oInput = value;
                UpdateStatus();
            }
        }

        public object Output
        {
            get
            {
                return oOutput;
            }
        }

        public string Message
        {
            get
            {
                string cMessage = "";
                switch (nStatus)
                {
                    case Status.Failed:
                        {
                            cMessage += "The conversion failed.";
                            break;
                        }

                    case Status.NotReady:
                        {
                            cMessage += "The conversion is not ready to be processed.";
                            break;
                        }

                    case Status.Ready:
                        {
                            cMessage += "All parameters are valid, and the conversion can proceed.";
                            break;
                        }

                    case var @case when @case == Status.Failed:
                        {
                            cMessage += "The conversion suceeded.";
                            break;
                        }
                }

                switch (nStatusReason)
                {
                    case StatusReason.ConversionNotAvailable:
                        {
                            cMessage += " The conversion for the types requested is not available.";
                            break;
                        }

                    case StatusReason.InputAndOutputTypeCannotBeSame:
                        {
                            cMessage += " The input type and output type are the same.";
                            break;
                        }

                    case StatusReason.InputOrOutputTypeNotSet:
                        {
                            cMessage += " Either the input type or output type has not been specified.";
                            break;
                        }

                    case StatusReason.InputSourceNotValidorEmpty:
                        {
                            cMessage += " The input is not valid.  It may not match the input type, may be empty, or if it is a file, it may not be available.";
                            break;
                        }
                }

                return cMessage;
            }
        }
        #endregion

        #region     Initialisation

        public Conversion(Type inputType, Type outputType)
        {
            nInputType = inputType;
            nOutputType = outputType;
            UpdateStatus();
        }

        public Conversion(Type inputType, Type outputType, object input)
        {
            nInputType = inputType;
            nOutputType = outputType;
            oInput = input;
            UpdateStatus();
        }

        public void Dispose()
        {
            nInputType = default;
            nOutputType = default;
            oInput = null;
            oOutputXml = null;
        }

        #endregion

        #region     Private Procedures

        private void UpdateStatus()
        {
            if (nInputType == Type.Undefined | nOutputType == Type.Undefined)
            {
                // Input or Output are undefined
                SetStatus(Status.NotReady, StatusReason.InputOrOutputTypeNotSet);
            }
            else if (nInputType == nOutputType)
            {
                // Input and Output are the same
                SetStatus(Status.NotReady, StatusReason.InputOrOutputTypeNotSet);
            }
            else if (!IsConversionAvailableConversion())
            {
                // Conversion method is not available
                SetStatus(Status.NotReady, StatusReason.ConversionNotAvailable);
            }
            else if (!IsResourceValid(oInput, nInputType))
            {
                // Input is not valid
                SetStatus(Status.NotReady, StatusReason.InputSourceNotValidorEmpty);
            }
            else
            {
                // Everything is okay
                SetStatus(Status.Ready, StatusReason.Undefined);
            }
        }

        private void SetStatus(Status status, StatusReason statusReason)
        {
            nStatus = status;
            nStatusReason = statusReason;
        }

        private void ConvertExcelToXml()
        {
            OleDbConnection oExcelConn = null;
            var oExcelAdapter = new OleDbDataAdapter();
            DataSet oExcelDataset;
            DataTable oWorksheets;
            string cWorksheetname = "";
            XmlDataDocument oXml;
            var oOutputXml = new XmlDocument();
            XmlElement oOutputRoot;
            XmlElement oOutputWorksheet;
            string cSql = "";
            try
            {

                // Establish the connection
                // oExcelConn = New OleDbConnection( _
                // "Provider=Microsoft.Jet.OLEDB.4.0;" & _
                // "Data Source=" & Me.oInput & ";" & _
                // "Extended Properties=""Excel 8.0;HDR=Yes;IMEX=1;""")

                if (Conversions.ToBoolean(oInput.endswith(".xlsx")))
                {
                    oOutputXml.LoadXml(GetXML(Conversions.ToString(oInput)));
                    this.oOutputXml = oOutputXml.DocumentElement;
                }
                else if (Conversions.ToBoolean(oInput.endswith(".csv")))
                {
                    oOutputXml.LoadXml(GetXML(Conversions.ToString(oInput)));
                    this.oOutputXml = oOutputXml.DocumentElement;
                }
                else
                {
                    oExcelConn = new OleDbConnection(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=", oInput), ";"), "Extended Properties=Excel 12.0")));
                    oExcelConn.Open();

                    // Get the worksheet names
                    oWorksheets = oExcelConn.GetSchema("Tables");

                    // Set up the Xml
                    oOutputRoot = oOutputXml.CreateElement("Workbook");
                    if (oWorksheets.Rows.Count == 0)
                    {
                        SetStatus(Status.Failed, StatusReason.InputSourceNotValidorEmpty);
                    }
                    else
                    {
                        foreach (DataRow oWorksheet in oWorksheets.Rows)
                        {
                            // .Trim("'") added to deal with worksheet names encompassed with 's
                            cWorksheetname = oWorksheet["TABLE_NAME"].ToString().Trim('\'');
                            if (cWorksheetname.EndsWith("$"))
                            {

                                // Get the data
                                cSql = "SELECT * FROM [" + cWorksheetname + "]";
                                oExcelAdapter.SelectCommand = new OleDbCommand(cSql, oExcelConn);
                                oExcelDataset = new DataSet("Sheet");
                                oExcelAdapter.Fill(oExcelDataset, "Row");

                                // Make the Column Names XML Friendly
                                foreach (DataColumn oExcelColumn in oExcelDataset.Tables[0].Columns)
                                    oExcelColumn.ColumnName = Strings.Replace(Strings.Replace(oExcelColumn.ColumnName, " ", "_"), ":", "_");

                                // Turn the DS into XML

                                oExcelDataset.EnforceConstraints = false;
                                oXml = new XmlDataDocument(oExcelDataset);
                                oOutputWorksheet = (XmlElement)oXml.FirstChild;

                                // Add the sheet
                                if (oOutputWorksheet is object)
                                {
                                    oOutputWorksheet.SetAttribute("name", Strings.Replace(cWorksheetname, "$", ""));
                                }

                                oOutputRoot.AppendChild(oOutputXml.ImportNode(oOutputWorksheet, true));
                            }
                        }

                        this.oOutputXml = oOutputRoot;
                        SetStatus(Status.Succeeded, StatusReason.Undefined);
                    }
                }
            }
            catch (Exception ex)
            {
                SetStatus(Status.Failed, StatusReason.Undefined);
            }
            finally
            {
                if (oExcelConn is object)
                {
                    if (oExcelConn.State != ConnectionState.Closed)
                    {
                        oExcelConn.Close();
                    }
                }
            }
        }

        private void ConvertCSVToXml()
        {
            bool firstRow = true;
            char _separator = CSVseparator;
            string _fieldnames = null;
            long succeeded = 0L;
            long failed = 0L;
            var ResponseXml = new XmlDocument();
            string cProcessInfo;
            var sr = default(StreamReader);
            // Dim writeFilePath As String = Me.writePath & "debug.txt"
            // Dim sw As New StreamWriter(File.Open(writeFilePath, FileMode.OpenOrCreate))
            try
            {
                if (ReferenceEquals(oInput.GetType(), typeof(StreamReader)))
                {
                    sr = (StreamReader)oInput;
                }
                else
                {
                    sr = new StreamReader(oInput.ToString());
                    // writeFilePath = Me.oInput.ToString().Replace(".csv", ".txt")
                }

                ResponseXml.AppendChild(ResponseXml.CreateElement("Feed"));
                var rowTemplate = ResponseXml.CreateElement(CSVLineName);
                string sLine;
                short nSepCount = 0;
                string[] afieldsTitles;
                if (CSVfirstLineTitles is object & firstRow)
                {
                    afieldsTitles = Strings.Split(CSVfirstLineTitles, Conversions.ToString(_separator));
                    for (int ii = 0, loopTo = afieldsTitles.Count() - 1; ii <= loopTo; ii++)
                    {
                        string _fName = "";
                        _fName = afieldsTitles[ii].Replace(Conversions.ToString(_separator), "");
                        _fName = _fName.Replace("\"", "");
                        _fName = _fName.Replace("?", "");
                        _fName = string.Concat(_fName.Where(c => !char.IsWhiteSpace(c)));
                        rowTemplate.AppendChild(ResponseXml.CreateElement(_fName));
                    }

                    firstRow = false;
                }

                // Do While (succeeded < 10000) And Not (sr.EndOfStream)

                while (!sr.EndOfStream)
                {
                    sLine = sr.ReadLine();
                    sLine = sLine.Replace(Constants.vbCrLf, "");

                    // get the number of seps in line 1
                    if (nSepCount == 0)
                    {
                        nSepCount = (short)(sLine.Split(_separator).Length - 1);
                    }
                    // keep reading till no of seps are hit.
                    while (sLine.Split(_separator).Length - 1 < nSepCount | ProcessNextLine(sLine, _separator, nSepCount))
                    {
                        object nextLine = sr.ReadLine();
                        // exit the loop if reached end of the file.
                        if (Information.IsNothing(nextLine))
                        {
                            break;
                        }

                        sLine = Conversions.ToString(Operators.AddObject(sLine, nextLine));
                        sLine = sLine.Replace(Constants.vbCrLf, "");
                    }

                    if (sLine is null)
                    {
                        break;
                    }

                    var fields = SplitWhilePreservingQuotedValues(sLine, _separator).ToArray();
                    if (firstRow)
                    {
                        for (int ii = 0, loopTo1 = fields.Count() - 1; ii <= loopTo1; ii++)
                        {
                            string _fName = "";
                            _fName = fields[ii];
                            _fName = _fName.Replace("\"", "");
                            _fName = _fName.Replace("?", "");
                            _fName = string.Concat(_fName.Where(c => !char.IsWhiteSpace(c)));
                            rowTemplate.AppendChild(ResponseXml.CreateElement(_fName));
                        }

                        firstRow = false;
                    }
                    else
                    {
                        try
                        {
                            bool bHasHtml = false;
                            XmlElement rowElmt = (XmlElement)rowTemplate.CloneNode(true);
                            for (int ii = 0, loopTo2 = fields.Count() - 1; ii <= loopTo2; ii++)
                            {
                                object _fValue = fields[ii];
                                if (!string.IsNullOrEmpty(Strings.Trim(Conversions.ToString(_fValue))))
                                {
                                    if (rowElmt.SelectSingleNode("*[position() = " + (long)(ii + 1) + "]") is null)
                                    {
                                        XmlElement wtf1 = (XmlElement)rowElmt.SelectSingleNode("*[position() = " + (long)(ii + 1) + "]");
                                    }
                                    else if (Conversions.ToBoolean(_fValue.startswith("<")))
                                    {
                                        rowElmt.SelectSingleNode("*[position() = " + (long)(ii + 1) + "]").InnerXml = Text.tidyXhtmlFrag(Conversions.ToString(_fValue), true, true);
                                        bHasHtml = true;
                                    }
                                    else
                                    {
                                        rowElmt.SelectSingleNode("*[position() = " + (long)(ii + 1) + "]").InnerText = Strings.Trim(Conversions.ToString(_fValue)).Replace(Constants.vbTab, "");
                                    }
                                }
                            }

                            if (bHasHtml == false)
                            {
                                cProcessInfo = "stop";
                            }

                            succeeded = succeeded + 1L;
                            rowElmt.SetAttribute("count", succeeded.ToString());
                            ResponseXml.DocumentElement.AppendChild(rowElmt);
                        }
                        // ResponseXml.Save(writeFilePath.Replace(".txt", ".xml"))

                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.ToString());
                            failed = failed + 1L;
                        }
                    }
                }

                XmlElement rootElmt = (XmlElement)ResponseXml.FirstChild;
                rootElmt.SetAttribute("succeeded", succeeded.ToString());
                rootElmt.SetAttribute("failed", failed.ToString());
                var oOutputXml = new XmlDocument();
                oOutputXml.LoadXml(ResponseXml.OuterXml);
                this.oOutputXml = oOutputXml.DocumentElement;
            }
            catch (Exception ex)
            {
                SetStatus(Status.Failed, StatusReason.Undefined);
            }
            finally
            {
                // sw.Close()
                // sw.Dispose()
                sr.Dispose();
                ResponseXml = null;
            }
        }

        #endregion

        #region     Private Functions

        private bool ProcessNextLine(string sLine, char _separator, int nSepCount)
        {
            // check for comma as separator
            if (_separator == ',')
            {
                sLine = sLine.Replace(@"\""", string.Empty);
                int countDoubleQuotes = Strings.Split(sLine, "\"").Length - 1;
                if (countDoubleQuotes > 0)
                {
                    object result = countDoubleQuotes % 2;
                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectGreater(result, 0, false))) // Odd number of quotes ..consider next line
                    {
                        return true;
                    }
                    else if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(result, 0, false))) // Even number of quotes but number of fields are less than expected..consider next line
                    {
                        var csvPreservingQuotedStrings = new Regex(string.Format(@"(?:^|{0})(\""(?:[^\""]+|\""\"")*\""|[^{0}]*)", _separator));
                        if (csvPreservingQuotedStrings.Matches(sLine).Count - 1 < nSepCount)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private IEnumerable<string> SplitWhilePreservingQuotedValues(string value, char delimiter)
        {
            try
            {


                // Dim csvPreservingQuotedStrings As New Regex(String.Format("(""[^""]*""|[^{0}])+", delimiter))
                var csvPreservingQuotedStrings = new Regex(string.Format(@"(?:^|{0})(\""(?:[^\""]+|\""\"")*\""|[^{0}]*)", delimiter));
                var values = new string[1];
                foreach (Match match in csvPreservingQuotedStrings.Matches(value))
                {
                    if (string.IsNullOrEmpty(values[0]))
                    {
                        values[0] = match.Value.TrimStart(delimiter);
                    }
                    else
                    {
                        Array.Resize(ref values, values.Length + 1);
                        values[values.Length - 1] = TrimQuotes(match.Value.TrimStart(delimiter).Replace("\"\"", "\""));
                    }
                }
                // Dim values() As String = csvPreservingQuotedStrings.Matches(value).Cast(Of Match)().[Select](Function(m) m.Value).ToArray()

                return values;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private string TrimQuotes(string myString)
        {
            try
            {
                if (myString.EndsWith("\""))
                {
                    myString = myString.Substring(0, myString.Length - 1);
                }

                if (myString.StartsWith("\""))
                {
                    myString = myString.Substring(1);
                }

                return myString;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private bool IsResourceValid(object oResource, Type oResourceType)
        {
            bool bCheck = true;
            try
            {

                // Check against type, and for populated values
                switch (oResourceType)
                {
                    case Type.CSV:
                    case Type.Excel:
                        {
                            if (oResource is string)
                            {
                                // Expected input is a filepath in the form of a string
                                if (Conversions.ToBoolean(!Operators.AndObject(Operators.ConditionalCompareObjectNotEqual(oResource, "", false), File.Exists(Conversions.ToString(oResource)))))
                                    bCheck = false;
                            }
                            else if (oResource is StreamReader)
                            {
                                bCheck = true;
                            }
                            else
                            {
                                bCheck = false;
                            }

                            break;
                        }

                    case Type.Xml:
                        {
                            if (!(oResource is XmlElement | oResource is XmlNode) & oResource is object)
                                bCheck = false;
                            break;
                        }

                    case Type.Undefined:
                        {
                            bCheck = false;
                            break;
                        }
                }

                return bCheck;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private bool IsConversionAvailableConversion()
        {
            // Explicitly determine if we can offer the conversion
            if (nInputType == Type.Excel & nOutputType == Type.Xml | nInputType == Type.CSV & nOutputType == Type.Xml)


            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region     Public Functions

        #endregion

        #region     Public Procedures

        public void Convert()
        {

            // Convert works by converting to Xml and then converting to the output type, if different from Xml
            try
            {
                if (nStatus == Status.Ready)
                {
                    // Convert the input type to Xml
                    switch (nInputType)
                    {
                        case Type.Excel:
                            {
                                ConvertExcelToXml();
                                if (oOutputXml is object)
                                {
                                    nStatus = Status.Succeeded;
                                }
                                else
                                {
                                    nStatus = Status.Failed;
                                }

                                break;
                            }

                        case Type.CSV:
                            {
                                ConvertCSVToXml();
                                if (oOutputXml is object)
                                {
                                    nStatus = Status.Succeeded;
                                }
                                else
                                {
                                    nStatus = Status.Failed;
                                }

                                break;
                            }

                        case Type.Xml:
                            {
                                oOutputXml = (XmlElement)oInput;
                                nStatus = Status.Succeeded;
                                break;
                            }
                    }

                    if (nStatus == Status.Succeeded)
                    {

                        // Convert the XML to the output type
                        switch (nOutputType)
                        {

                            // Case Type.CSV
                            // ConvertXMLToCSV()

                            case Type.Xml:
                                {
                                    oOutput = oOutputXml;
                                    break;
                                }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null, new Errors.ErrorEventArgs(mcModuleName, "Convert", ex, ""));
            }
        }


        #endregion

        public string GetXML(string filename)
        {
            try
            {
                var ds = ReadExcelFile(filename);
                return ds.GetXml();
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null, new Errors.ErrorEventArgs(mcModuleName, "GetXML", ex, ""));
            }

            return default;
        }

        private DataSet ReadExcelFile(string filename)
        {
            string cProcessInfo;
            var ds = new DataSet();
            try
            {
                var spreadsheetDocument = SpreadsheetDocument.Open(filename, false);
                var workbookPart = spreadsheetDocument.WorkbookPart;
                var sheetcollection = spreadsheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();
                int sheetCount = 0;
                foreach (var myWorksheet in sheetcollection)
                {
                    var dt = new DataTable();
                    string relationshipId = myWorksheet.Id.Value;
                    WorksheetPart worksheetPart = (WorksheetPart)spreadsheetDocument.WorkbookPart.GetPartById(relationshipId);
                    var sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();
                    var rowcollection = sheetData.Descendants<Row>();
                    if (!(rowcollection.Count() == 0))
                    {
                        // TS - First row on second worksheet is empty so stepping on one
                        // For Each cell As Cell In rowcollection.ElementAt(0)
                        long colCount = 1L;
                        // create rows using first line
                        if (sheetCount > 0)
                        {
                            dt.Columns.Add("column" + colCount);
                        }

                        foreach (Cell cell in rowcollection.ElementAt(0)) // rowcollection.ElementAt(sheetCount)
                        {
                            string rowName = GetValueOfCell(spreadsheetDocument, cell);
                            if ((rowName ?? "") == (string.Empty ?? ""))
                            {
                                dt.Columns.Add("column" + colCount);
                            }
                            else
                            {
                                dt.Columns.Add(rowName);
                            }

                            colCount = colCount + 1L;
                        }
                        // add some spare columns incase not all are titled.
                        dt.Columns.Add("spare" + (colCount + 1L).ToString());
                        dt.Columns.Add("spare" + (colCount + 2L).ToString());
                        dt.Columns.Add("spare" + (colCount + 3L).ToString());
                        dt.Columns.Add("spare" + (colCount + 4L).ToString());
                        dt.Columns.Add("spare" + (colCount + 5L).ToString());
                        foreach (Row row in rowcollection)
                        {
                            var temprow = dt.NewRow();
                            int columnIndex = 0;
                            foreach (Cell cell in row.Descendants<Cell>())
                            {
                                int cellColumnIndex = GetColumnIndex(GetColumnName(cell.CellReference));
                                if (columnIndex < cellColumnIndex)
                                {
                                    do
                                    {
                                        try
                                        {
                                            temprow[columnIndex] = string.Empty;
                                        }
                                        catch (Exception ex)
                                        {
                                            cProcessInfo = "Not found " + columnIndex;
                                        }

                                        columnIndex += 1;
                                    }
                                    while (columnIndex < cellColumnIndex);
                                }

                                string cellVal = GetValueOfCell(spreadsheetDocument, cell);
                                try
                                {
                                    temprow[columnIndex] = cellVal;
                                }
                                catch (Exception ex)
                                {
                                    cProcessInfo = "Not found " + columnIndex;
                                }

                                columnIndex += 1;
                            }

                            dt.Rows.Add(temprow);
                        }
                        // Here remove header row
                        dt.Rows.RemoveAt(0);
                        ds.Tables.Add(dt);
                    }

                    sheetCount = sheetCount + 1;
                }
                // End Using
                spreadsheetDocument.Close();
                spreadsheetDocument = null;
                return ds;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null, new Errors.ErrorEventArgs(mcModuleName, "GetXML", ex, ""));
            }

            return default;
        }

        private static string GetValueOfCell(SpreadsheetDocument spreadsheetdocument, Cell cell)
        {
            var sharedString = spreadsheetdocument.WorkbookPart.SharedStringTablePart;
            if (cell.CellValue is null)
            {
                return string.Empty;
            }

            string cellValue = cell.CellValue.InnerText;
            if (cell.DataType is object && cell.DataType.Value == CellValues.SharedString)
            {
                return sharedString.SharedStringTable.ChildElements[int.Parse(cellValue)].InnerText;
            }
            else
            {
                return cellValue;
            }
        }

        private int GetColumnIndex(string columnName)
        {
            int columnIndex = 0;
            int factor = 1;

            // From right to left
            for (int position = columnName.Length - 1; position >= 0; position -= 1)
            {
                // For letters
                if (char.IsLetter(columnName[position]))
                {
                    columnIndex += factor * (Strings.AscW(columnName[position]) - Strings.AscW('A') + 1) - 1;
                    factor *= 26;
                }
            }

            return columnIndex;
        }

        private string GetColumnName(string cellReference)
        {
            // Create a regular expression to match the column name of cell
            var regex = new Regex("[A-Za-z]+");
            var match = regex.Match(cellReference);
            return match.Value;
        }
    }

    public class ReverseIterator : IEnumerable
    {

        // a low-overhead ArrayList to store references
        private ArrayList items = new ArrayList();
        // load all the items in the ArrayList, but in reverse order
        public ReverseIterator(IEnumerable collection)
        {
            foreach (var o in collection)
                items.Insert(0, o);
        }

        public IEnumerator GetEnumerator()
        {
            // return the enumerator of the inner ArrayList
            return items.GetEnumerator();
        }
    }

    public class classFileLineReader : IDisposable
    {
        // Provides a File Reader that improves upon the .ReadLine method
        // in order to read through single CR and LF and return a full
        // line based on CrLf
        private StreamReader m_objInputFile;
        private FileStream m_objTempFile;
        private BinaryReader m_objTempReader;
        private long m_lngFilePointer = 0L;

        // Clean up our resources

        public void Dispose()
        {
            ;
#error Cannot convert OnErrorResumeNextStatementSyntax - see comment for details
            /* Cannot convert OnErrorResumeNextStatementSyntax, CONVERSION ERROR: Conversion for OnErrorResumeNextStatement not implemented, please report this issue in 'On Error Resume Next' at character 35198


            Input:

                    'Clean up our resources

                    On Error Resume Next

             */
            m_lngFilePointer = 0L;
            if (Information.IsNothing(m_objTempReader) == false)
            {
                m_objTempReader.Close();
                m_objTempReader = null;
            }

            if (Information.IsNothing(m_objTempFile) == false)
            {
                m_objTempFile.Close();
                m_objTempFile.Dispose();
                m_objTempFile = null;
            }

            if (Information.IsNothing(m_objInputFile) == false)
            {
                m_objInputFile.Close();
                m_objInputFile.Dispose();
                m_objInputFile = null;
            }
        }

        public classFileLineReader(string pFilePath)
        {
            // When the Class is created, we need to know what file we'll be working with

            try
            {
                m_objInputFile = new StreamReader(pFilePath, System.Text.Encoding.ASCII);
                m_objTempFile = File.OpenRead(pFilePath);
                m_objTempReader = new BinaryReader(m_objTempFile);
            }
            catch (Exception ex)
            {
                throw new Exception("Error: [" + Information.Err().Number + "] " + Information.Err().Description, ex);
            }
        }

        // Clean up our resources
        ~classFileLineReader()
        {
            ;
#error Cannot convert OnErrorResumeNextStatementSyntax - see comment for details
            /* Cannot convert OnErrorResumeNextStatementSyntax, CONVERSION ERROR: Conversion for OnErrorResumeNextStatement not implemented, please report this issue in 'On Error Resume Next' at character 36731


            Input:

                    'Clean up our resources
                    On Error Resume Next

             */
            Dispose();
        }

        public bool EOF
        {

            // Replaces the End Of File test for a normal stream

            get
            {
                ;
#error Cannot convert OnErrorResumeNextStatementSyntax - see comment for details
                /* Cannot convert OnErrorResumeNextStatementSyntax, CONVERSION ERROR: Conversion for OnErrorResumeNextStatement not implemented, please report this issue in 'On Error Resume Next' at character 36955


                Input:
                            On Error Resume Next

                 */
                return m_objInputFile.EndOfStream;
            }
        }

        public string ReadLine()
        {
            // Returns a Text line from the file up to the next CrLf
            // while skipping any single Cr or Lf's (which the normal ReadLine does not do)

            string strLine = "";
            string strNextLine = "";
            var bytPeekChars = new byte[3];
            const byte cCR = 13;
            const byte cLf = 10;
            string test = "";
            try
            {
                if (m_objInputFile.EndOfStream == false)
                {
                    strLine = m_objInputFile.ReadLine(); // Read in the Next Line Like Normal
                                                         // Now we use our 2nd Reader to to PEEK at the character
                                                         // that caused the ReadLine to End PLUS the next one to Determine
                                                         // If we are really at the end of line
                    m_lngFilePointer += strLine.Length;
                    do
                    {
                        strNextLine = "[Lf]"; // By Defaulting to Text, it prevents CrCr or LfLf from stopping the loop early
                        m_objTempReader.BaseStream.Seek(m_lngFilePointer, SeekOrigin.Begin);
                        bytPeekChars = m_objTempReader.ReadBytes(2);
                        // See if what Caused the EOL was the End of the File, a Single Cr, or a Single Lf
                        // If (bytPeekChars.Length > 0) AndAlso (bytPeekChars(0) <> cCR Or bytPeekChars(1) <> cLf) Then
                        if (bytPeekChars.Length > 0 && bytPeekChars[1] != cLf)
                        {
                            strNextLine = m_objInputFile.ReadLine(); // Read Lines until we reach the end or get a real CrLf
                            m_lngFilePointer += strNextLine.Length + 1; // We only had a +1 becaue it was a Single Cr or Lf
                            if (!string.IsNullOrEmpty(strNextLine))
                            {
                                strLine += " " + strNextLine; // Append it to the Real Line
                            }
                        }
                        else
                        {
                            test = "";
                        }
                    }
                    while (strNextLine != "[Lf]");
                    m_lngFilePointer += 1L; // We Add +1 to our position to account for the Final Lf
                }
            }
            catch (Exception ex)
            {
                strLine = "";
                throw new Exception("Error: [" + Information.Err().Number + "] " + Information.Err().Description, ex);
            }

            return strLine;
        }

        void IDisposable.Dispose()
        {
            Dispose();
        }

        private void IDisposable_Dispose() => ((IDisposable)this).Dispose();
    }
}