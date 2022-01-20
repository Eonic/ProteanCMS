using System;
using System.Collections;

namespace Protean.Tools.Errors
{
    public class ErrorEventArgs : System.EventArgs
    {
        private string cModuleName;
        private string cProcedureName;
        private string cInfo;
        private Hashtable oOtherSettings;
        private Exception oException;
        private int nImportance;
        public string ModuleName
        {
            get
            {
                return cModuleName;
            }
        }
        public string ProcedureName
        {
            get
            {
                return cProcedureName;
            }
        }
        public string AddtionalInformation
        {
            get
            {
                return cInfo;
            }
        }
        public Hashtable OtherSettings
        {
            get
            {
                return oOtherSettings;
            }
        }
        public Exception Exception
        {
            get
            {
                return oException;
            }
        }
        public int importance
        {
            get
            {
                return nImportance;
            }
        }
        public ErrorEventArgs(string Module, string Procedure, Exception ex, string Info, int importance = 0, Hashtable Settings = null/* TODO Change to default(_) if this is not a reference type */)
        {
            cModuleName = Module;
            cProcedureName = Procedure;
            cInfo = Info;
            oOtherSettings = Settings;
            oException = ex;
        }

        public override string ToString()
        {
            string cMessage = "";
            cMessage += "Module: " + cModuleName;
            cMessage += "Procedure: " + cProcedureName + System.Environment.NewLine;;
            cMessage += "Exception: " + oException.ToString() + System.Environment.NewLine;
            cMessage += "Additional Info: " + cInfo;
            if (!(oOtherSettings == null) && oOtherSettings.Count > 0)
                cMessage += System.Environment.NewLine + "Other Settings: " + oOtherSettings.ToString();
            return cMessage;
        }
    }
}
