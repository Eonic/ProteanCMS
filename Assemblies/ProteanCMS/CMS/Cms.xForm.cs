using System;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

using static Protean.stdTools;

namespace Protean
{

    public partial class Cms
    {
        public class xForm : Protean.xForm
        {

            #region New Error Handling

            public event OnErrorEventHandler OnError;

            public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);

            private void _OnError(object sender, Tools.Errors.ErrorEventArgs e)
            {
                stdTools.returnException(ref Protean.xForm.msException, e.ModuleName, e.ProcedureName, e.Exception, "", e.AddtionalInformation, gbDebug);
            }

            #endregion

            #region Declarations
            public Cms myWeb;
            private string mcModuleName = "Web.xForm";
            #endregion
            public xForm(ref string sException) : base(ref sException)
            {
                OnError += _OnError;
            }

            public xForm(ref Cms aWeb) : base(ref aWeb.msException)
            {
                aWeb.PerfMon.Log(mcModuleName, "New");
                try
                {

                    myWeb = aWeb;
                    this.moPageXML = myWeb.moPageXml;
                    this.mnUserId = myWeb.mnUserId;
                    this.moCtx = myWeb.moCtx;

                    if (this.moCtx is not null)
                    {
                        this.goApp = this.moCtx.Application;
                        this.goRequest = this.moCtx.Request;
                        this.goResponse = this.moCtx.Response;
                        this.goSession = this.moCtx.Session;
                        this.goServer = this.moCtx.Server;
                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""));
                }

                OnError += _OnError;
            }


            public override string evaluateByType(string sValue, string sType, string cExtensions = "", bool isRequired = false)
            {
                string cProcessInfo = "";
                string cReturn = ""; // Set this as a clear return string

                try
                {
                    cReturn = base.evaluateByType(sValue, sType, cExtensions, isRequired);

                    // Only evaulate if there is data to evaluate against!

                    if (myWeb is not null & !string.IsNullOrEmpty(sValue))
                    {
                        switch (sType ?? "")
                        {
                            case "reValidateUser":
                                {
                                    cReturn = myWeb.moDbHelper.validateUser(this.mnUserId, sValue);
                                    if (Information.IsNumeric(cReturn))
                                        cReturn = "";
                                    break;
                                }
                        }
                    }

                    return cReturn;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "evaluteByType", ex, ""));
                    return "";
                }
            }


            public override bool isUnique(string sValue, string sPath)
            {
                string cProcessInfo = "";
                // Placeholder for overide
                try
                {

                    // Confirm the contenttype and the field
                    string[] unqiueVal = sPath.Split('|');
                    string tableName = unqiueVal[0];
                    string schemaName = unqiueVal[1];
                    string fieldName = unqiueVal[2];
                    string xPath = unqiueVal[3];
                    // Generate the xpath if value is in XML within the field


                    // Query the database to confirm if this value is unique.

                    return myWeb.moDbHelper.isUnique(tableName, schemaName, fieldName, xPath, sValue);
                }


                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "isUnique", ex, "", cProcessInfo, gbDebug);
                    return Conversions.ToBoolean("");
                }
            }

        }
    }
}