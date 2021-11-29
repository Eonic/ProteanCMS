using System;
using System.Data;
using System.Xml;
using Microsoft.VisualBasic;

namespace Protean.Tools
{
    public class Csv : IDisposable
    {

        #region Declarations
        private DataTable oTable;
        // Dim oError As Exception = Nothing
        public event OnErrorEventHandler OnError;

        public delegate void OnErrorEventHandler(object sender, Errors.ErrorEventArgs e);

        private const string mcModuleName = "Protean.Tools.Csv";
        #endregion
        #region Properties
        public DataTable Table
        {
            get
            {
                return oTable;
            }
        }

        public System.Xml.XPath.IXPathNavigable Xml
        {
            get
            {
                try
                {
                    var oDS = new DataSet("CSV");
                    oDS.Tables.Add(oTable);
                    var oXml = new XmlDocument();
                    oXml.InnerXml = oDS.GetXml();
                    return oXml;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Errors.ErrorEventArgs(mcModuleName, "Xml", ex, ""));
                    return null;
                }
            }
        }
        #endregion
        #region Public Procedures
        public Csv(string Tablename, string Filebody, string Delimiter, string Qualifier, bool Columnnames)
        {
            try
            {
                oTable = new DataTable();
                oTable.TableName = Tablename;
                Convert(Filebody, Delimiter, Qualifier, Columnnames);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Errors.ErrorEventArgs(mcModuleName, "New", ex, ""));
            }
        }
        #endregion
        #region Private Procedures
        private void Convert(string Filebody, string Delimiter, string Qualifier, bool Columnnames)
        {
            try
            {
                var cRows = GetRows(Filebody);
                int i;
                var loopTo = Information.UBound(cRows);
                for (i = 0; i <= loopTo; i++)
                {
                    if (i == 0 & Columnnames)
                    {
                        SetColNames(GetColumns(cRows[i], Delimiter, Qualifier));
                    }
                    else
                    {
                        AddRow(GetColumns(cRows[i], Delimiter, Qualifier));
                    }
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Errors.ErrorEventArgs(mcModuleName, "Convert", ex, ""));
            }
        }

        private string[] GetRows(string Filebody)
        {
            try
            {
                return Strings.Split(Filebody, Constants.vbCrLf);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Errors.ErrorEventArgs(mcModuleName, "GetRows", ex, ""));
                return null;
            }
        }

        private string[] GetColumns(string Rowcolumns, string Delimiter, string Qualifier)
        {
            try
            {
                var cCols = Strings.Split(Rowcolumns, Delimiter);
                int i;
                var loopTo = Information.UBound(cCols);
                for (i = 0; i <= loopTo; i++)
                {
                    if (!((cCols[i] ?? "") == (Qualifier ?? "")) & cCols[i].Length > 0)
                    {
                        if ((cCols[i].Substring(0, 1) ?? "") == (Qualifier ?? ""))
                        {
                            cCols[i] = Strings.Right(cCols[i], cCols[i].Length - 1);
                        }
                    }
                    else
                    {
                        cCols[i] = "";
                    }

                    if (!((cCols[i] ?? "") == (Qualifier ?? "")) & cCols[i].Length > 0)
                    {
                        if ((cCols[i].Substring(cCols[i].Length - 1, 1) ?? "") == (Qualifier ?? ""))
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
                OnError?.Invoke(this, new Errors.ErrorEventArgs(mcModuleName, "GetColumns", ex, ""));
                return null;
            }
        }

        private void SetColNames(string[] Cols)
        {
            try
            {
                int i;
                var loopTo = Information.UBound(Cols);
                for (i = 0; i <= loopTo; i++)
                    oTable.Columns.Add(new DataColumn(Cols[i]));
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Errors.ErrorEventArgs(mcModuleName, "SetColNames", ex, ""));
            }
        }

        private void AddRow(string[] Cols)
        {
            try
            {
                int i;
                var loopTo = Information.UBound(Cols);
                for (i = 0; i <= loopTo; i++)
                {
                    if (i >= oTable.Columns.Count)
                    {
                        oTable.Columns.Add(new DataColumn("Column_" + i));
                    }
                }

                oTable.Rows.Add(Cols);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Errors.ErrorEventArgs(mcModuleName, "AddRow", ex, ""));
            }
        }

        #endregion

        private bool disposedValue = false;        // To detect redundant calls

        // IDisposable
        protected virtual void Dispose(bool Disposing)
        {
            if (!disposedValue)
            {
                if (Disposing)
                {
                    if (oTable is object)
                    {
                        oTable.Dispose();
                    }
                }
            }

            disposedValue = true;
        }

        #region  IDisposable Support 
        // This code added by Visual Basic to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}