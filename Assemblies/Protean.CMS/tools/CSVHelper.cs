using System;
using System.Data;
using System.Xml;
using Microsoft.VisualBasic;

namespace Protean
{

    public class CSVHelper
    {

        private DataTable oTable;
        private Exception oError = null;

        public CSVHelper(string cTableName, string cFileString, string cDelimiter, string cQualifier, bool bColumnNames)
        {
            try
            {

                oTable = new DataTable();
                oTable.TableName = cTableName;
                Convert(cFileString, cDelimiter, cQualifier, bColumnNames);
            }
            catch (Exception ex)
            {
                if (oError is null)
                    oError = ex;
            }
        }

        private void Convert(string cFileString, string cDelimiter, string cQualifier, bool bColumnNames)
        {
            if (oError != null)
                return;
            try
            {
                string[] cRows = GetRows(cFileString);
                int i;
                var loopTo = Information.UBound(cRows);
                for (i = 0; i <= loopTo; i++)
                {
                    if (i == 0 & bColumnNames)
                    {
                        SetColNames(GetColumns(cRows[i], cDelimiter, cQualifier));
                    }
                    else
                    {
                        AddRow(GetColumns(cRows[i], cDelimiter, cQualifier));
                    }
                }
            }
            catch (Exception ex)
            {
                if (oError is null)
                    oError = ex;
            }
        }

        private string[] GetRows(string cFileString)
        {
            if (oError != null)
                return null;
            try
            {
                return Strings.Split(cFileString, Constants.vbCrLf);
            }
            catch (Exception ex)
            {
                if (oError is null)
                    oError = ex;
                return null;
            }
        }

        private string[] GetColumns(string cRow, string cDelimiter, string cQualifier)
        {
            if (oError != null)
                return null;
            try
            {
                string[] cCols = Strings.Split(cRow, cDelimiter);
                int i;
                var loopTo = Information.UBound(cCols);
                for (i = 0; i <= loopTo; i++)
                {
                    if (!((cCols[i] ?? "") == (cQualifier ?? "")) & cCols[i].Length > 0)
                    {
                        if ((cCols[i].Substring(0, 1) ?? "") == (cQualifier ?? ""))
                        {
                            cCols[i] = Strings.Right(cCols[i], cCols[i].Length - 1);
                        }
                    }
                    else
                    {
                        cCols[i] = "";
                    }
                    if (!((cCols[i] ?? "") == (cQualifier ?? "")) & cCols[i].Length > 0)
                    {
                        if ((cCols[i].Substring(cCols[i].Length - 1, 1) ?? "") == (cQualifier ?? ""))
                        {
                            cCols[i] = Strings.Left(cCols[i], cCols[i].Length - 1);
                        }
                    }
                    else
                    {
                        cCols[i] = "";
                    }
                }
                return cCols;
            }
            catch (Exception ex)
            {
                if (oError is null)
                    oError = ex;
                return null;
            }
        }

        private void SetColNames(string[] cCols)
        {
            if (oError != null)
                return;
            try
            {
                int i;
                var loopTo = Information.UBound(cCols);
                for (i = 0; i <= loopTo; i++)
                    oTable.Columns.Add(new DataColumn(cCols[i]));
            }
            catch (Exception ex)
            {
                if (oError is null)
                    oError = ex;
            }
        }

        private void AddRow(string[] cCols)
        {
            if (oError != null)
                return;
            try
            {
                int i;
                var loopTo = Information.UBound(cCols);
                for (i = 0; i <= loopTo; i++)
                {
                    if (i >= oTable.Columns.Count)
                    {
                        oTable.Columns.Add(new DataColumn("Column_" + i));
                    }
                }
                oTable.Rows.Add(cCols);
            }
            catch (Exception ex)
            {
                if (oError is null)
                    oError = ex;
            }
        }

        public DataTable Table
        {
            get
            {
                return oTable;
            }
        }

        public Exception Exception
        {
            get
            {
                return oError;
            }
        }

        public XmlDocument XML
        {
            get
            {
                try
                {
                    var oDS = new DataSet("CSV");
                    oDS.Tables.Add(oTable);
                    var oXML = new XmlDocument();
                    oXML.InnerXml = oDS.GetXml();
                    return oXML;
                }
                catch (Exception ex)
                {
                    if (oError is null)
                        oError = ex;
                    return null;
                }
            }
        }
    }
}