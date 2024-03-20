using System;
using Microsoft.VisualBasic;

namespace Protean
{


    public class FileDoc
    {
        private string cPath = "";
        //private string mcModuleName = "FileDoc";

        public FileDoc(string thePath)
        {
            // PerfMon.Log("FileDoc", "New")
            try
            {
                cPath = thePath;
            }
            catch (Exception)
            {
                // returnException(mcModuleName, "DocWord", ex, "")
                cPath = "";
            }
        }

        public string Extension
        {
            get
            {
                // PerfMon.Log("FileDoc", "Extension")
                try
                {
                    string cTest = "";
                    string cExt = "";
                    if (cPath.Contains("."))
                    {
                        int i = 1;
                        while (cTest != ".")
                        {
                            cTest = Strings.Left(Strings.Right(cPath, i), 1);
                            if (i > 1)
                                cExt = Strings.Right(cPath, i - 1);
                            i += 1;
                        }
                    }
                    return cExt;
                }
                catch (Exception)
                {
                    // returnException(mcModuleName, "Extension", ex, "")
                    return "";
                }
            }
        }

        public string Text
        {
            get
            {
                // PerfMon.Log("FileDoc", "Text")
                try
                {
                    switch (Extension ?? "")
                    {
                        case "doc":
                            {
                                // word
                                return DocWord();
                            }
                        case "rtf":
                            {
                                // word
                                return DocWord();
                            }
                        case "txt":
                            {
                                // text
                                return DocText();
                            }
                        case "xls":
                            {
                                // excel
                                return DocExcel();
                            }
                        case "csv":
                            {
                                // text
                                return DocText();
                            }
                        case "pdf":
                            {
                                // pdf
                                return DocPDF();
                            }
                        case "zip":
                            {
                                // ignore
                                return "";
                            }

                        default:
                            {
                                return "";
                            }
                    }
                }
                catch (Exception)
                {
                    // returnException(mcModuleName, "Text", ex, "")
                    return "";
                }
            }
        }

        public override string ToString()
        {
            // PerfMon.Log("FileDoc", "ToString")
            try
            {
                return Text;
            }
            catch (Exception)
            {
                // returnException(mcModuleName, "ToString", ex, "")
                return "";
            }
        }

        #region Typed Document Handlers
        private string DocWord()
        {
            // PerfMon.Log("FileDoc", "DocWord")
            Tools.IFilter.DefaultParser oIF;
            try
            {
                oIF = new Tools.IFilter.DefaultParser();
                string cReturn = Tools.IFilter.DefaultParser.Extract(cPath);
                oIF = null;
                return cReturn;
            }
            catch (Exception)
            {
                oIF = null;
                return null;
            }
        }

        private string DocExcel()
        {
            // PerfMon.Log("FileDoc", "DocExcel")
            Tools.IFilter.DefaultParser oIF;
            try
            {
                oIF = new Tools.IFilter.DefaultParser();
                string cReturn = Tools.IFilter.DefaultParser.Extract(cPath);
                oIF = null;
                return cReturn;
            }
            catch (Exception)
            {
                oIF = null;
                return "";
            }
        }

        private string DocPDF()
        {
            // PerfMon.Log("FileDoc", "DocPDF")
            Tools.IFilter.DefaultParser oIF;
            try
            {
                oIF = new Tools.IFilter.DefaultParser();
                string cReturn = Tools.IFilter.DefaultParser.Extract(cPath);
                oIF = null;
                return cReturn;
            }
            catch (Exception)
            {

                oIF = null;
                return "";
            }
        }

        private string DocText()
        {
            Tools.IFilter.DefaultParser oIF;
            try
            {
                oIF = new Tools.IFilter.DefaultParser();
                string cReturn = Tools.IFilter.DefaultParser.Extract(cPath);
                oIF = null;
                return cReturn;
            }
            catch (Exception)
            {

                oIF = null;
                return "";
            }
        }
        #endregion
    }
}