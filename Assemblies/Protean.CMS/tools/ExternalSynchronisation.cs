using System;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Web.Configuration;
using System.Xml;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Protean.Tools.Integration.Twitter;
using static Protean.stdTools;

namespace Protean
{
    public class ExternalSynchronisation : Tools.SoapClient
    {

        #region Declarations


        private XmlElement moPageDetail;
        private System.Collections.Specialized.NameValueCollection moConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");
        private System.Collections.Specialized.NameValueCollection moSyncConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/synchronisation");
        private const string mcModuleName = "Web.ExternalSynchronisation";
        private Cms myWeb;
        private Tools.Database _moDBT;

        private Tools.Database moDBT
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _moDBT;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_moDBT != null)
                {
                    _moDBT.OnError -= _OnError;
                }

                _moDBT = value;
                if (_moDBT != null)
                {
                    _moDBT.OnError += _OnError;
                }
            }
        }
        private Tools.Xslt.Transform _moTransform;

        private Tools.Xslt.Transform moTransform
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _moTransform;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_moTransform != null)
                {
                    _moTransform.OnError -= _OnError;
                }

                _moTransform = value;
                if (_moTransform != null)
                {
                    _moTransform.OnError += _OnError;
                }
            }
        }
        private Protean.Cms.dbHelper _moDBH;

        private Protean.Cms.dbHelper moDBH
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _moDBH;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                _moDBH = value;
            }
        }
        private Xslt _moXSLTFunctions;

        private Xslt moXSLTFunctions
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _moXSLTFunctions;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_moXSLTFunctions != null)
                {
                    _moXSLTFunctions.XSLTError -= AddLowError;
                }

                _moXSLTFunctions = value;
                if (_moXSLTFunctions != null)
                {
                    _moXSLTFunctions.XSLTError += AddLowError;
                }
            }
        }
        public new event OnErrorEventHandler OnError;

        public new delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);

        public string cLastError = "";


        private void _OnError(object sender, Tools.Errors.ErrorEventArgs e)
        {
            OnError?.Invoke(sender, e);
            LogError(e.ModuleName, e.ProcedureName, e.Exception, e.AddtionalInformation);
            AddExceptionToEventLog(e.Exception, e.ProcedureName);
        }


        private XmlDocument oLowErrors;

        #endregion

        #region Initialisation
        public ExternalSynchronisation(ref Cms aWeb, [Optional, DefaultParameterValue(null)] ref XmlElement oPageDetail)
        {
            myWeb = aWeb;
            moPageDetail = oPageDetail;
            InitialiseVariables();
            base.OnError += _OnError;
        }

        public ExternalSynchronisation([Optional, DefaultParameterValue(null)] ref XmlElement oPageDetail)
        {
            myWeb = new Cms();
            moPageDetail = oPageDetail;
            InitialiseVariables();
            base.OnError += _OnError;
        }

        private void InitialiseVariables()
        {

            string cPassword;
            string cUsername;
            string cDBAuth;

            RemoveReturnSoapEnvelope = true;
            Url = moSyncConfig["URL"];
            Namespace = moSyncConfig["Namespace"];

            moDBT = new Tools.Database();

            moDBT.DatabaseServer = moConfig["DatabaseServer"];
            moDBT.DatabaseName = moConfig["DatabaseName"];

            if (moConfig["DatabaseUsername"] != null)
            {
                moDBT.DatabaseUser = moConfig["DatabaseUsername"];
                moDBT.DatabasePassword = moConfig["DatabasePassword"];
            }
            else
            {
                // No authorisation information provided in the connection string.  
                // We need to source it from somewhere, let's try the web.config 
                cDBAuth = GetDBAuth();

                // Let's find the username and password
                cUsername = Tools.Text.SimpleRegexFind(cDBAuth, "user id=([^;]*)", 1, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                cPassword = Tools.Text.SimpleRegexFind(cDBAuth, "password=([^;]*)", 1, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                moDBT.DatabaseUser = cUsername;
                moDBT.DatabasePassword = cPassword;
            }

            moTransform = new Tools.Xslt.Transform();
            moXSLTFunctions = new Xslt(myWeb);
            moTransform.XslTExtensionObject = moXSLTFunctions;
            moTransform.XslTExtensionURN = "ew";

            if (myWeb is null)
            {
                moDBH = new Cms.dbHelper(moDBT.DatabaseConnectionString, 1);
            }
            else
            {
                moDBH = new Cms.dbHelper(ref myWeb);
            }

        }

        public string GetDBAuth()
        {
            // PerfMon.Log("dbHelper", "getDBAuth")
            try
            {
                string dbAuth;
                if (!string.IsNullOrEmpty(moConfig["DatabasePassword"]))
                {
                    dbAuth = "user id=" + moConfig["DatabaseUsername"] + "; password=" + moConfig["DatabasePassword"];
                }
                else if (!string.IsNullOrEmpty(moConfig["DatabaseAuth"]))
                {
                    dbAuth = moConfig["DatabaseAuth"];
                }
                else
                {
                    dbAuth = "Integrated Security=SSPI;";
                }
                return dbAuth;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "getDBAuth", ex, ""));
                return null;
            }
        }
        #endregion

        #region Import
        public string ImportItems(string oSoapBody, string cAction)
        {
            int nNew = 0;
            int nUpdated = 0;
            int nSkipped = 0;
            string cResult = "";
            string cInfo = "Beginning";
            var oFinalResultsXML = new XmlDocument();
            int nItemCount = 0;

            oFinalResultsXML.AppendChild(oFinalResultsXML.CreateElement("Results"));
            try
            {
                Action = cAction;

                // Get a List of items
                var oListXML = new XmlDocument(); // List of Items
                cInfo = "Getting 1st Soap";
                oListXML.LoadXml(Tools.Xml.convertEntitiesToCodes(SendRequest(oSoapBody)));
                if (ErrorReturned | oListXML.SelectSingleNode("descendant-or-self::Errors") != null)
                    return ExternalError(oListXML.OuterXml);

                XmlElement oStkList = (XmlElement)oListXML.SelectSingleNode("GetStockListResponse/GetStockListResult/*");

                if (Conversions.ToDouble(oStkList.GetAttribute("ReturnedRecordCount")) > 0d)
                {
                    cInfo = "1st Transform";
                    moTransform.XslTFile = goServer.MapPath(moSyncConfig["ImportXSL"]);
                    moTransform.Xml = oListXML;




                    // STAGE 1
                    // transform the list into something recognisable
                    var oDBResults = new XmlDocument();

                    // This provides Table names and fields we need to search on and the ref of the foreign items
                    oDBResults.LoadXml(moTransform.Process());
                    DataSet oDS = null;

                    // Loop through each one

                    foreach (XmlElement oRDBesElmt in oDBResults.DocumentElement)
                    {
                        bool bNew = true; // counter
                        Protean.Cms.dbHelper.TableNames oTableNameEnum = (Cms.dbHelper.TableNames)System.Enum.Parse(typeof(Cms.dbHelper.TableNames), oRDBesElmt.Name);                        
                        string cKeyField = moDBH.TableKey(oTableNameEnum);
                        string cSQL = "SELECT " + cKeyField + " FROM " + oRDBesElmt.Name;
                        string cWhere = "";
                        foreach (XmlElement oWhereElmts in oRDBesElmt.ChildNodes)
                        {
                            if (!string.IsNullOrEmpty(cWhere))
                                cWhere += " AND ";
                            cWhere += " (" + oWhereElmts.Name + " = ";

                            // took this out because datatype wrong
                            // If IsNumeric(oWhereElmts.InnerText) Then
                            // cWhere &= oWhereElmts.InnerText & ")"
                            // ElseIf IsDate(oWhereElmts.InnerText) Then
                            // cWhere &= Protean.Tools.Database.SqlDate(oWhereElmts.InnerText) & ")"
                            // Else
                            // cWhere &= "'" & Protean.Tools.Database.SqlFmt(oWhereElmts.InnerText) & "')"
                            // End If

                            // replaced with this which decided datatype based on fieldname
                            switch (Strings.Left(oWhereElmts.Name, 1) ?? "")
                            {
                                case "c":
                                    {
                                        cWhere += "'" + Tools.Database.SqlFmt(oWhereElmts.InnerText) + "')";
                                        break;
                                    }
                                case "n":
                                    {
                                        cWhere += oWhereElmts.InnerText + ")";
                                        break;
                                    }
                                case "d":
                                    {
                                        cWhere += Tools.Database.SqlDate(oWhereElmts.InnerText) + ")";
                                        break;
                                    }

                                default:
                                    {
                                        cWhere += "'" + Tools.Database.SqlFmt(oWhereElmts.InnerText) + "')";
                                        break;
                                    }
                            }


                        }
                        if (!string.IsNullOrEmpty(cWhere))
                            cWhere = " WHERE " + cWhere;
                        cSQL += cWhere;

                        // get the id
                        var oInstanceXML = new XmlDocument();
                        moDBH.moPageXml = oInstanceXML;

                        int nId = Conversions.ToInteger(moDBT.GetDataValue(cSQL, CommandType.Text, null, 0));

                        var oInstance = oInstanceXML.CreateElement("OriginalInstance");
                        if (nId == 0)
                        {
                            switch (oTableNameEnum)
                            {
                                case var @case when @case == Cms.dbHelper.TableNames.tblContent: // content item
                                    {
                                        // we need to know what type of item it is
                                        string cSchemaName = "";
                                        if (oRDBesElmt.SelectSingleNode("cContentSchemaName") != null)
                                        {
                                            cSchemaName = oRDBesElmt.SelectSingleNode("cContentSchemaName").InnerText;
                                        }
                                        if (!string.IsNullOrEmpty(cSchemaName))
                                        {
                                            var oXForm = new Protean.xForm(ref myWeb.msException);
                                            if (!oXForm.load("/xforms/content/" + cSchemaName + ".xml", myWeb.maCommonFolders))
                                            {
                                                // cant load it
                                                goto SkipIt;

                                            }
                                            oInstance.InnerXml = oXForm.Instance.InnerXml;
                                        }
                                        else
                                        {
                                            // we have no idea what type of content it is
                                            goto SkipIt;
                                        }

                                        break;
                                    }

                                default:
                                    {
                                        oInstance.InnerXml = moDBH.getObjectInstance((Cms.dbHelper.objectTypes)oTableNameEnum, nId);
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            bNew = false;
                            oInstance.InnerXml = moDBH.getObjectInstance((Cms.dbHelper.objectTypes)oTableNameEnum, nId);
                            if (string.IsNullOrEmpty(oInstance.InnerXml))
                            {
                                cInfo = nId + "returned no instance data";
                            }
                        }



                        // now to get the information from the foreign database
                        // first need to create the soap body
                        // STAGE 2
                        var oRefXML = new XmlDocument();

                        var oRefElmt = oRefXML.CreateElement("Ref");
                        oRefElmt.InnerText = oRDBesElmt.GetAttribute("Ref");
                        moTransform.Xml = oRefElmt;
                        cInfo = "2nd Transform";
                        oRefXML.LoadXml(moTransform.Process());
                        string cItemAction = oRefXML.DocumentElement.GetAttribute("Action");
                        oRefXML.DocumentElement.RemoveAttribute("Action");
                        Action = cItemAction;


                        var oForeignItem = oRefXML.CreateElement("ForeignInstance");
                        cInfo = "2nd Request";
                        oForeignItem.InnerXml = SendRequest(oRefXML.OuterXml);
                        if (ErrorReturned | oListXML.SelectSingleNode("descendant-or-self::Errors") != null)
                            return ExternalError(oListXML.OuterXml);


                        oInstanceXML.AppendChild(oInstanceXML.CreateElement("Instances"));
                        oInstanceXML.DocumentElement.AppendChild(oInstance);
                        oInstanceXML.DocumentElement.AppendChild(oInstanceXML.ImportNode(oForeignItem, true));

                        // now to get the new instance
                        // STAGE 3
                        cInfo = "3rd Transform - id SQL" + cSQL;
                        var oFinalInstance = new XmlDocument();
                        moTransform.Xml = oInstanceXML;
                        string cFinalInstance = moTransform.Process();
                        // check we actually have an instance to save
                        if (string.IsNullOrEmpty(cFinalInstance))
                            goto SkipIt;
                        // else carry on
                        oFinalInstance.LoadXml(cFinalInstance);

                        // check we actually have an instance to save
                        int nReturnId;

                        if (oFinalInstance.DocumentElement is null | string.IsNullOrEmpty(oFinalInstance.DocumentElement.InnerXml))
                        {

                            goto SkipIt;
                        }

                        else
                        {
                            nReturnId =Convert.ToInt32(moDBH.setObjectInstance((Cms.dbHelper.objectTypes)oTableNameEnum, oFinalInstance.DocumentElement, nId));
                        }


                        // now we have an id we can see if there are any other things its wants to do
                        var oFinishElmt = oFinalInstance.CreateElement("Finish");
                        oFinishElmt.SetAttribute("ID", nReturnId.ToString());
                        // lets add some instances in case they are needed
                        oFinishElmt.AppendChild(oFinalInstance.ImportNode(oForeignItem, true));
                        var oNewInstance = oFinalInstance.CreateElement("NewInstance");
                        oNewInstance.InnerXml = myWeb.moDbHelper.getObjectInstance((Cms.dbHelper.objectTypes)oTableNameEnum, nReturnId);
                        oFinishElmt.AppendChild(oNewInstance);


                        moTransform.Xml = oFinishElmt;

                        // now we have finished yay
                        // STAGE 4
                        cInfo = "4th Transform";
                        var oResultXML = new XmlDocument();
                        oResultXML.LoadXml(moTransform.Process());
                        oFinalResultsXML.DocumentElement.AppendChild(oFinalResultsXML.ImportNode(oResultXML.DocumentElement, true));
                        if (bNew)
                            nNew += 1;
                        else
                            nUpdated += 1;
                        goto EndIt;
                    SkipIt:
                        ;

                        nSkipped += 1;
                    EndIt:
                        ;

                        oFinalResultsXML.DocumentElement.SetAttribute("New", nNew.ToString());
                        oFinalResultsXML.DocumentElement.SetAttribute("Updated", nUpdated.ToString());
                        oFinalResultsXML.DocumentElement.SetAttribute("Skipped", nSkipped.ToString());
                        nItemCount += 1;

                    }
                }
                else
                {
                    // No Results returned
                    oFinalResultsXML.DocumentElement.AppendChild(oFinalResultsXML.ImportNode(oListXML.DocumentElement.FirstChild, true));
                }
                ReportLowErrors(ref oFinalResultsXML);
                return oFinalResultsXML.OuterXml;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(mcModuleName, new Tools.Errors.ErrorEventArgs(mcModuleName, "ImportContentItems", ex, cInfo));
                // cResult = " An Error occured " & cInfo & ": " & ex.ToString & "<br/>" & cLastError
                // oFinalResultsXML.DocumentElement.InnerText = cResult
                throw new ArgumentException("Exception");
                return "";

            }


        }

        // Public Function ImportItems(ByRef oInstancesXml As XmlDocument, ByVal cXsltPat As String) As String
        // Dim nNew As Integer = 0
        // Dim nUpdated As Integer = 0
        // Dim nSkipped As Integer = 0
        // Dim cResult As String = ""
        // Dim cInfo As String = "Beginning"
        // Dim oFinalResultsXML As New XmlDocument
        // Dim nItemCount As Integer = 0

        // oFinalResultsXML.AppendChild(oFinalResultsXML.CreateElement("Results"))
        // Try

        // For Each oInstance In oInstancesXml.DocumentElement
        // Dim bNew As Boolean = True 'counter

        // Dim oTableNameEnum As Protean.Cms.dbHelper.TableNames = System.Enum.Parse(GetType(Protean.Cms.dbHelper.TableNames), oRDBesElmt.Name)
        // Dim cKeyField As String = moDBH.TableKey(oTableNameEnum)
        // Dim cSQL As String = "SELECT " & cKeyField & " FROM " & oRDBesElmt.Name
        // Dim cWhere As String = ""
        // Dim oWhereElmts As XmlElement
        // For Each oWhereElmts In oRDBesElmt.ChildNodes
        // If Not cWhere = "" Then cWhere &= " AND "
        // cWhere &= " (" & oWhereElmts.Name & " = "
        // If IsNumeric(oWhereElmts.InnerText) Then
        // cWhere &= oWhereElmts.InnerText & ")"
        // ElseIf IsDate(oWhereElmts.InnerText) Then
        // cWhere &= Protean.Tools.Database.SqlDate(oWhereElmts.InnerText) & ")"
        // Else
        // cWhere &= "'" & Protean.Tools.Database.SqlFmt(oWhereElmts.InnerText) & "')"
        // End If
        // Next
        // If Not cWhere = "" Then cWhere = " WHERE " & cWhere
        // cSQL &= cWhere

        // 'get the id
        // Dim oInstanceXML As New XmlDocument
        // moDBH.moPageXml = oInstanceXML

        // Dim nId As Integer = moDBT.GetDataValue(cSQL, CommandType.Text, , 0)

        // Dim oInstance As XmlElement = oInstanceXML.CreateElement("OriginalInstance")
        // If nId = 0 Then
        // Select Case oTableNameEnum
        // Case Cms.dbHelper.TableNames.tblContent 'content item
        // 'we need to know what type of item it is
        // Dim cSchemaName As String = ""
        // If Not oRDBesElmt.SelectSingleNode("cContentSchemaName") Is Nothing Then
        // cSchemaName = oRDBesElmt.SelectSingleNode("cContentSchemaName").InnerText
        // End If
        // If Not cSchemaName = "" Then
        // Dim oXForm As New Protean.xForm
        // If Not oXForm.load("/xforms/content/" & cSchemaName & ".xml") Then
        // If Not oXForm.load("/ewcommon/xforms/content/" & cSchemaName & ".xml") Then
        // 'cant load it
        // GoTo SkipIt
        // End If
        // End If
        // oInstance.InnerXml = oXForm.instance.InnerXml
        // Else
        // 'we have no idea what type of content it is
        // GoTo SkipIt
        // End If
        // Case Else
        // oInstance.InnerXml = moDBH.getObjectInstance(oTableNameEnum, nId)
        // End Select
        // Else
        // bNew = False
        // oInstance.InnerXml = moDBH.getObjectInstance(oTableNameEnum, nId)
        // End If

        // 'now to get the information from the foreign database
        // 'first need to create the soap body
        // 'STAGE 2
        // Dim oRefXML As New XmlDocument

        // Dim oRefElmt As XmlElement = oRefXML.CreateElement("Ref")
        // oRefElmt.InnerText = oRDBesElmt.GetAttribute("Ref")
        // moTransform.Xml = oRefElmt
        // cInfo = "2nd Transform"
        // oRefXML.LoadXml(moTransform.Process)
        // Dim cItemAction As String = oRefXML.DocumentElement.GetAttribute("Action")
        // oRefXML.DocumentElement.RemoveAttribute("Action")
        // Me.Action = cItemAction


        // Dim oForeignItem As XmlElement = oRefXML.CreateElement("ForeignInstance")
        // cInfo = "2nd Request"
        // oForeignItem.InnerXml = MyBase.SendRequest(oRefXML.OuterXml)
        // If MyBase.ErrorReturned Or Not oListXML.SelectSingleNode("descendant-or-self::Errors") Is Nothing Then Return ExternalError(oListXML.OuterXml)


        // oInstanceXML.AppendChild(oInstanceXML.CreateElement("Instances"))
        // oInstanceXML.DocumentElement.AppendChild(oInstance)
        // oInstanceXML.DocumentElement.AppendChild(oInstanceXML.ImportNode(oForeignItem, True))

        // 'now to get the new instance
        // 'STAGE 3
        // cInfo = "3rd Transform"
        // Dim oFinalInstance As New XmlDocument
        // moTransform.Xml = oInstanceXML
        // Dim cFinalInstance As String = moTransform.Process
        // 'check we actually have an instance to save
        // If cFinalInstance = "" Then GoTo SkipIt
        // 'else carry on
        // oFinalInstance.LoadXml(cFinalInstance)

        // 'check we actually have an instance to save
        // 'If oFinalInstance.DocumentElement Is Nothing Then GoTo SkipIt


        // Dim nReturnId As Integer = moDBH.setObjectInstance(oTableNameEnum, oFinalInstance.DocumentElement, nId)
        // 'now we have an id we can see if there are any other things its wants to do
        // Dim oFinishElmt As XmlElement = oFinalInstance.CreateElement("Finish")
        // oFinishElmt.SetAttribute("ID", nReturnId)
        // 'lets add some instances in case they are needed
        // oFinishElmt.AppendChild(oFinalInstance.ImportNode(oForeignItem, True))
        // Dim oNewInstance As XmlElement = oFinalInstance.CreateElement("NewInstance")
        // oNewInstance.InnerXml = myWeb.moDbHelper.getObjectInstance(oTableNameEnum, nReturnId)
        // oFinishElmt.AppendChild(oNewInstance)


        // moTransform.Xml = oFinishElmt

        // 'now we have finished yay
        // 'STAGE 4
        // cInfo = "4th Transform"
        // Dim oResultXML As New XmlDocument
        // oResultXML.LoadXml(moTransform.Process())
        // oFinalResultsXML.DocumentElement.AppendChild(oFinalResultsXML.ImportNode(oResultXML.DocumentElement, True))
        // If bNew Then nNew += 1 Else nUpdated += 1
        // GoTo EndIt
        // SkipIt:
        // nSkipped += 1
        // EndIt:
        // oFinalResultsXML.DocumentElement.SetAttribute("New", nNew)
        // oFinalResultsXML.DocumentElement.SetAttribute("Updated", nUpdated)
        // oFinalResultsXML.DocumentElement.SetAttribute("Skipped", nSkipped)
        // nItemCount += 1



        // Next


        // Catch ex As Exception
        // RaiseEvent OnError(mcModuleName, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ImportContentItems", ex, cInfo))
        // cResult = " An Error occured " & cInfo & ": " & ex.ToString & "<br/>" & cLastError
        // End Try
        // ReportLowErrors(oFinalResultsXML)
        // Return oFinalResultsXML.OuterXml

        // End Function

        #endregion

        #region Export
        public string ExportItems(string oSoapBody, string cAction)
        {
            string cProcessInfo = "";
            try
            {
                var oSoapXML = new XmlDocument();
                oSoapXML.LoadXml(oSoapBody);
                moTransform.XslTFile = goServer.MapPath(moSyncConfig["ExportXSL"]);
                moTransform.Xml = oSoapXML;
                var oQueryXML = new XmlDocument();
                oQueryXML.LoadXml(moTransform.Process());
                var oDS = new DataSet(oQueryXML.DocumentElement.GetAttribute("name"));
                XmlElement oSQLElmt;
                // Get Tables
                foreach (XmlElement currentOSQLElmt in oQueryXML.DocumentElement.SelectNodes("Table"))
                {
                    oSQLElmt = currentOSQLElmt;
                    moDBT.addTableToDataSet(ref oDS, oSQLElmt.InnerText, oSQLElmt.GetAttribute("name"));
                }
                // Get Relations
                foreach (XmlElement currentOSQLElmt1 in oQueryXML.DocumentElement.SelectNodes("Relation"))
                {
                    oSQLElmt = currentOSQLElmt1;
                    cProcessInfo = "error linking relation " + oSQLElmt.GetAttribute("name") + " to " + oSQLElmt.GetAttribute("ParentTable") + " on " + oSQLElmt.GetAttribute("Nested") + " with " + oSQLElmt.GetAttribute("ParentColumn");
                    DataColumn[] parentColumns = moDBT.GetColumnArray(ref oDS, oSQLElmt.GetAttribute("ParentTable"), Strings.Split(oSQLElmt.GetAttribute("ParentColumn"), ","));
                    DataColumn[] childColumns = moDBT.GetColumnArray(ref oDS, oSQLElmt.GetAttribute("ChildTable"), Strings.Split(oSQLElmt.GetAttribute("ChildColumn"), ","));
                    oDS.Relations.Add(oSQLElmt.GetAttribute("name"), parentColumns, childColumns, false);
                    oDS.Relations[oSQLElmt.GetAttribute("name")].Nested = Conversions.ToBoolean(oSQLElmt.GetAttribute("Nested"));
                }
                cProcessInfo = "";
                // Map all columns as attributes first
                foreach (DataTable oDT in oDS.Tables)
                {
                    foreach (DataColumn oDC in oDT.Columns)
                        oDC.ColumnMapping = MappingType.Attribute;
                }
                // Get Mappings
                foreach (XmlElement currentOSQLElmt2 in oQueryXML.DocumentElement.SelectNodes("Mapping"))
                {
                    oSQLElmt = currentOSQLElmt2;
                    MappingType oType = (MappingType)Conversions.ToInteger(Enum.Parse(typeof(MappingType), oSQLElmt.GetAttribute("Type")));
                    oDS.Tables[oSQLElmt.GetAttribute("Table")].Columns[oSQLElmt.GetAttribute("Column")].ColumnMapping = oType;
                }
                // Now we have the dataset we can produce an Export XML
                // and then send it using the original saop instance
                var xmltemp = new XmlDocument();
                xmltemp.InnerXml = oDS.GetXml();
                oSoapXML.DocumentElement.FirstChild.InnerXml = Strings.Replace(Strings.Replace(oDS.GetXml(), "&gt;", ">"), "&lt;", "<");
                Action = cAction;
                var oFinishXML = new XmlDocument();
                oFinishXML.AppendChild(oFinishXML.CreateElement("Finish"));
                oFinishXML.DocumentElement.InnerXml = SendRequest(oSoapXML.OuterXml);

                if (ErrorReturned | oFinishXML.SelectSingleNode("descendant-or-self::Errors") != null)
                    return ExternalError(oFinishXML.OuterXml);
                // do any tidying
                moTransform.Xml = oFinishXML;
                string cReturn = Strings.Replace(moTransform.Process(), "xmlns:", "exemelnamespace");
                return cReturn;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ExportItems", ex, cProcessInfo));
                return "";
            }
        }
        #endregion


        #region Delete
        public string DeleteItems(string sSoapBody, string cAction)
        {
            string cContentType;
            string cDeleteMode;
            var oSoapBody = new XmlDocument();
            var oDbt = new Cms.dbHelper(ref myWeb);

            try
            {
                // lets get the content type
                oSoapBody.LoadXml(sSoapBody);
                cContentType = oSoapBody.SelectSingleNode("descendant-or-self::cContentType").InnerText;
                cDeleteMode = oSoapBody.SelectSingleNode("descendant-or-self::cDeleteMode").InnerText;

                string sSql = "select nContentKey, cContentForiegnRef from tblContent where cContentSchemaName = " + Tools.Database.SqlString(cContentType);

                DataSet oDs = oDbt.GetDataSet(sSql, "tblContent");
                long nCount = 0L;
                long nCountHidden = 0L;
                long nCountNoRef = 0L;
                var oRequest = new XmlDocument();
                XmlElement oElmt;
                oRequest.LoadXml("<CheckDeleted xmlns=\"http://www.eonic.co.uk/ewcommon/Services\"><StockItems><ItemList/></StockItems></CheckDeleted>");

                var nsmgr = new XmlNamespaceManager(oRequest.NameTable);
                nsmgr.AddNamespace("ews", "http://www.eonic.co.uk/ewcommon/Services");

                if (oDs.Tables["tblContent"].Rows.Count == 0)
                {
                    return "Nothing to Delete";
                }
                else
                {
                    foreach (DataRow oRow in oDs.Tables["tblContent"].Rows)
                    {
                        string cFRef = Conversions.ToString(oRow["cContentForiegnRef"]);
                        // CheckItemExists
                        switch (cDeleteMode ?? "")
                        {
                            case "Delete Items Not Found":
                                {
                                    if (!string.IsNullOrEmpty(cFRef) & !ReferenceEquals(cFRef, DBNull.Value))
                                    {
                                        oElmt = oRequest.CreateElement("cStockCode");
                                        oElmt.InnerText = cFRef;
                                        oRequest.SelectSingleNode("/ews:CheckDeleted/ews:StockItems/ews:ItemList", nsmgr).AppendChild(oElmt);
                                    }
                                    else
                                    {
                                        // Deleting objects with no FRef
                                        oDbt.DeleteObject(Cms.dbHelper.objectTypes.Content, Convert.ToInt64(oRow["nContentKey"]));
                                        // oDbt.setObjectStatus(Cms.dbHelper.objectTypes.Content, Cms.dbHelper.Status.Hidden, oRow("nContentKey"))
                                        nCountNoRef = nCountNoRef + 1L;
                                    }

                                    break;
                                }
                            case "Delete All":
                                {
                                    oDbt.DeleteObject(Cms.dbHelper.objectTypes.Content, Convert.ToInt64(oRow["nContentKey"]));
                                    nCount = nCount + 1L;
                                    break;
                                }
                        }
                    }
                    if (cDeleteMode == "Delete Items Not Found")
                    {
                        var oRecord = new XmlDocument();
                        Action = "CheckDeleted";
                        oRecord.AppendChild(oRecord.CreateElement("Result"));
                        oRecord.DocumentElement.InnerXml = SendRequest(Strings.Replace(oRequest.OuterXml, "xmlns=\"\"", ""));
                        var nsmgr2 = new XmlNamespaceManager(oRecord.NameTable);
                        nsmgr2.AddNamespace("ews", "http://www.eonic.co.uk/ewcommon/Services");
                        foreach (XmlElement currentOElmt in oRecord.SelectNodes("descendant-or-self::Delete", nsmgr2))
                        {
                            oElmt = currentOElmt;
                            long nContentId = oDbt.getObjectByRef(Cms.dbHelper.objectTypes.Content, oElmt.InnerText);
                            oDbt.DeleteObject(Cms.dbHelper.objectTypes.Content, nContentId);
                            // oDbt.setObjectStatus(Cms.dbHelper.objectTypes.Content, Cms.dbHelper.Status.Hidden, nContentId)
                            nCount = nCount + 1L;
                        }
                        foreach (XmlElement currentOElmt1 in oRecord.SelectNodes("descendant-or-self::Hide", nsmgr2))
                        {
                            oElmt = currentOElmt1;
                            long nContentId = oDbt.getObjectByRef(Cms.dbHelper.objectTypes.Content, oElmt.InnerText);
                            // oDbt.DeleteObject(Cms.dbHelper.objectTypes.Content, nContentId)
                            oDbt.setObjectStatus(Cms.dbHelper.objectTypes.Content, Cms.dbHelper.Status.Hidden, nContentId);
                            nCountHidden = nCountHidden + 1L;
                        }
                        oRecord = null;

                        return "<result>Deleted [" + nCount + "] Hidden [" + nCountHidden + "] foriegn items and [" + nCountNoRef + "] items with no fRef out of [" + oDs.Tables["tblContent"].Rows.Count + "] Items</result>";
                    }

                    else
                    {

                        return "<result>Deleted " + nCount + " items out of " + oDs.Tables["tblContent"].Rows.Count + " Items</result>";

                    }
                }
            }

            catch (Exception ex)
            {
                OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "DeleteItems", ex, ""));
                return "";
            }
        }


        #endregion


        #region Admin
        public string AdminProcess(string cEwCmd2)
        {
            string[] oSyncItems = Strings.Split(moSyncConfig["Actions"], ",");
            int i = 0;
            var oXForm = new Protean.xForm(ref myWeb.msException);


            try
            {
                switch (cEwCmd2 ?? "")
                {
                    case var @case when @case == "":
                    case var case1 when case1 == "":
                        {
                            var loopTo = oSyncItems.Length - 1;
                            for (i = 0; i <= loopTo; i++)
                                Tools.Xml.addElement(ref moPageDetail, "SyncAction", oSyncItems[i]);
                            break;
                        }

                    default:
                        {
                            var loopTo1 = oSyncItems.Length - 1;
                            for (i = 0; i <= loopTo1; i++)
                            {
                                if ((oSyncItems[i] ?? "") == (cEwCmd2 ?? ""))
                                {
                                    oXForm.moPageXML = moPageDetail.OwnerDocument;
                                    oXForm.load("/xforms/synchronisation/" + cEwCmd2 + ".xml");
                                    if (oXForm.isSubmitted())
                                    {
                                        oXForm.updateInstanceFromRequest();
                                        oXForm.validate();
                                        if (oXForm.valid)
                                        {
                                            XmlElement oSoapElmt = (XmlElement)oXForm.Instance.SelectSingleNode("descendant-or-self::*[@exemelnamespace and @type]");
                                            string cInOut = oSoapElmt.GetAttribute("type");
                                            oSoapElmt.RemoveAttribute("type");
                                            switch (cInOut ?? "")
                                            {
                                                case "Input":
                                                    {
                                                        var oElmt = moPageDetail.OwnerDocument.CreateElement("Result");
                                                        oElmt.SetAttribute("Action", cEwCmd2);
                                                        oElmt.InnerXml = ImportItems(oSoapElmt.OuterXml, oSoapElmt.Name);
                                                        moPageDetail.AppendChild(oElmt);
                                                        return "SynchronisationResults";
                                                    }
                                                case "Output":
                                                    {
                                                        var oElmt = moPageDetail.OwnerDocument.CreateElement("Result");
                                                        oElmt.SetAttribute("Action", cEwCmd2);
                                                        oElmt.InnerXml = ExportItems(oSoapElmt.OuterXml, oSoapElmt.Name);
                                                        moPageDetail.AppendChild(oElmt);
                                                        return "SynchronisationResults";
                                                    }
                                                case "Delete":
                                                    {
                                                        var oElmt = moPageDetail.OwnerDocument.CreateElement("Result");
                                                        oElmt.SetAttribute("Action", cEwCmd2);
                                                        oElmt.InnerXml = DeleteItems(oSoapElmt.OuterXml, oSoapElmt.Name);
                                                        moPageDetail.AppendChild(oElmt);
                                                        return "SynchronisationResults";
                                                    }
                                            }
                                        }
                                        else
                                        {
                                            oXForm.addValues();
                                            moPageDetail.AppendChild(oXForm.moXformElmt);
                                            return "SynchronisationXForm";
                                        }
                                    }
                                    else
                                    {
                                        oXForm.addValues();
                                        moPageDetail.AppendChild(oXForm.moXformElmt);
                                        return "SynchronisationXForm";
                                    }
                                }
                            }

                            break;
                        }
                }
                return "Synchronisation";
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "AdminProcess", ex, ""));
                return "";
            }
        }


        #endregion


        #region Private Procedures

        private string ExternalError(string cErrorXML)
        {
            try
            {
                var oErrorXML = new XmlDocument();
                oErrorXML.LoadXml(cErrorXML);
                string cReturn = "";
                foreach (XmlElement oElmt in oErrorXML.DocumentElement.FirstChild.FirstChild.ChildNodes)
                {
                    cReturn += "<h3>Error:</h3><br/>";
                    cReturn += "<ul>";
                    foreach (XmlElement oChild in oElmt.ChildNodes)
                    {
                        cReturn += "<li><strong>" + oChild.Name + "</strong>: ";
                        int i = 0;
                        foreach (XmlElement oGChild in oChild.SelectNodes("*"))
                        {
                            cReturn += "<ul><li><strong>" + oGChild.Name + "</strong>: " + oGChild.InnerXml + "</li></ul>";
                            i += 1;
                        }
                        if (i == 0)
                            cReturn += oChild.InnerText;
                        cReturn += "</li>";
                    }
                    cReturn += "</ul><br/>";
                }
                cReturn = Strings.Replace(cReturn, "&", "&amp;");
                cReturn = Strings.Replace(cReturn, "&amp;amp;", "&amp;");
                return cReturn;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ExportItems", ex, ""));
                return ex.ToString();
            }
        }

        private void LogError(string cModuleName, string cRoutineName, Exception oException, string cFurtherInfo)
        {
            string cInfo = "";
            try
            {
                cInfo = "Check Path";
                if (string.IsNullOrEmpty(moSyncConfig["ErrorLog"]))
                    return;
                cInfo = "Build File Name";
                string cFileName = Tools.Text.IntegerToString(DateTime.Now.Year, 2);
                cFileName += Tools.Text.IntegerToString(DateTime.Now.Month, 2);
                cFileName += Tools.Text.IntegerToString(DateTime.Now.Day, 2);
                cFileName += ".xml";
                cFileName = moSyncConfig["ErrorLog"] + cFileName;

                var oErrorXML = new XmlDocument();
                if (System.IO.File.Exists(cFileName))
                {
                    cInfo = "File Exists";
                    oErrorXML.Load(cFileName);
                }
                else
                {
                    cInfo = "New";
                    oErrorXML.AppendChild(oErrorXML.CreateElement("Log"));
                }
                cInfo = "Building Log";
                var oLogElmt = oErrorXML.CreateElement("Error");
                oLogElmt.SetAttribute("Time", Conversions.ToString(DateTime.Now));
                oLogElmt.SetAttribute("Module", cModuleName);
                oLogElmt.SetAttribute("Routine", cRoutineName);
                oLogElmt.SetAttribute("Information", cFurtherInfo);

                var oExceptionElmt = oErrorXML.CreateElement("Exception");
                oExceptionElmt.AppendChild(oErrorXML.CreateElement("Message")).InnerText = oException.Message;
                oExceptionElmt.AppendChild(oErrorXML.CreateElement("Source")).InnerText = oException.Source;
                oExceptionElmt.AppendChild(oErrorXML.CreateElement("StackTrace")).InnerText = oException.StackTrace;
                oLogElmt.AppendChild(oExceptionElmt);
                oErrorXML.DocumentElement.AppendChild(oLogElmt);
                cInfo = "Saving Log";
                oErrorXML.Save(cFileName);
            }

            catch (Exception ex)
            {
                OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "LogError", ex, cInfo));
            }
        }




        private void AddLowError(string cInformation)
        {
            try
            {
                if (oLowErrors is null)
                    oLowErrors = new XmlDocument();
                if (oLowErrors.DocumentElement is null)
                    oLowErrors.AppendChild(oLowErrors.CreateElement("LowErrors"));
                var oErr = oLowErrors.CreateElement("LowError");
                oErr.InnerText = cInformation;
                oLowErrors.DocumentElement.AppendChild(oErr);
            }
            catch (Exception ex)
            {

            }
        }

        private void ReportLowErrors(ref XmlDocument ReportDoc)
        {
            try
            {
                ReportDoc.DocumentElement.AppendChild(ReportDoc.ImportNode(oLowErrors.DocumentElement, true));
            }
            catch (Exception ex)
            {

            }
        }

        #endregion

        #region Sub Classes

        private class Xslt : Tools.Xslt.XsltFunctions
        {

            #region Declarations

            private Cms myWeb;
            public event OnErrorEventHandler OnError;

            public delegate void OnErrorEventHandler(string cModuleName, string cRoutineName, Exception oException, string cFurtherInfo);
            private const string mcModuleName = "Web.ExternalSynchronisation.Xslt";
            private System.Collections.Specialized.NameValueCollection moSyncConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/synchronisation");
            private System.Collections.Specialized.NameValueCollection moConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");
            public event XSLTErrorEventHandler XSLTError;

            public delegate void XSLTErrorEventHandler(string cInformation);
            #endregion

            #region Initialisation/Private
            public Xslt(Cms aWeb)
            {
                myWeb = aWeb;
                XSLTError += Xslt_XSLTError;
            }
            #endregion

            #region General Text/Number/File

            private string tidyXhtml(string shtml, bool bReturnNumbericEntities = false)
            {
                string sTidyXhtml;
                // PerfMon.Log("Web", "tidyXhtmlFrag")

                sTidyXhtml = Tools.Text.tidyXhtmlFrag(shtml, bReturnNumbericEntities);

                return sTidyXhtml;

            }


            public object CleanHTML(string cHTML)
            {

                var oXML = new XmlDocument();

                if (cHTML is null)
                {
                    cHTML = "";
                }

                cHTML = Strings.Replace(cHTML, Conversions.ToString('\r'), "<br/>");
                cHTML = Tools.Xml.convertEntitiesToCodes(cHTML);
                cHTML = Strings.Replace(Strings.Replace(cHTML, "&gt;", ">"), "&lt;", "<");
                cHTML = "<p>" + cHTML + "</p>";
                try
                {
                    cHTML = tidyXhtml(cHTML);
                }
                catch (Exception ex)
                {
                    XSLTError?.Invoke(ex.ToString());
                }
                cHTML = Strings.Replace(cHTML, "&#x0;", "");
                cHTML = Strings.Replace(cHTML, " &#0;", "");
                try
                {
                    oXML.LoadXml(cHTML);
                    return oXML.DocumentElement;
                }
                catch (Exception ex)
                {
                    // Lets try option 2 first before we raise an error
                    // RaiseEvent XSLTError(ex.ToString)
                    try
                    {
                        oXML = new XmlDocument();
                        oXML.AppendChild(oXML.CreateElement("div"));
                        oXML.DocumentElement.InnerXml = cHTML;
                        return oXML.DocumentElement;
                    }
                    catch (Exception ex2)
                    {
                        XSLTError?.Invoke(ex2.ToString());
                        return cHTML;
                    }
                }
            }

            public string SQLDateTime(string sDate)
            {
                try
                {
                    if (Information.IsDate(sDate))
                    {
                        return Tools.Database.SqlDate(Conversions.ToDate(sDate));
                    }
                    else
                    {
                        return "'" + sDate + "'";
                    }
                }
                catch (Exception ex)
                {
                    return "'" + sDate + "'";
                }
            }

            public int VirtualFileExists(string cVirtualPath)
            {
                try
                {
                    string cVP = goServer.MapPath(cVirtualPath);
                    if (System.IO.File.Exists(cVP))
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
                catch (Exception ex)
                {
                    return 0;
                }
            }

            public string ImageWidth(string cVirtualPath)
            {
                try
                {
                    if (VirtualFileExists(cVirtualPath) > 0)
                    {
                        var oImage = new Tools.Image(goServer.MapPath(cVirtualPath));
                        int nVar = oImage.Width;
                        oImage.Close();
                        return nVar.ToString();
                    }
                    else
                    {
                        return "";
                    }
                }
                catch (Exception ex)
                {
                    return "";
                }
            }

            public string ImageHeight(string cVirtualPath)
            {
                try
                {
                    if (VirtualFileExists(cVirtualPath) > 0)
                    {
                        var oImage = new Tools.Image(goServer.MapPath(cVirtualPath));
                        int nVar = oImage.Height;
                        oImage.Close();
                        return nVar.ToString();
                    }
                    else
                    {
                        return "";
                    }
                }
                catch (Exception ex)
                {
                    return "";
                }
            }


            public string ResizeImage(string cVirtualPath, long maxWidth, long maxHeight, string sSuffix)
            {
                string newFilepath = "";
                try
                {
                    if (VirtualFileExists(cVirtualPath) > 0)
                    {
                        var oImage = new Tools.Image(goServer.MapPath(cVirtualPath));
                        // calculate the new filename
                        newFilepath = Strings.Replace(cVirtualPath, ".jpg", sSuffix + ".jpg");
                        if (!(VirtualFileExists(newFilepath) > 0))
                        {
                            oImage.KeepXYRelation = true;
                            oImage.SetMaxSize((int)maxWidth, (int)maxHeight);
                            oImage.Save(goServer.MapPath(newFilepath), 25);
                            return newFilepath;
                        }
                        else
                        {
                            return newFilepath;
                        }
                    }
                    else
                    {
                        return "Source Image Not Found";
                    }
                }


                catch (Exception ex)
                {
                    return "";
                }
            }

            public string ResizeImage(string cVirtualPath, long maxWidth, long maxHeight, string sSuffix, int nCompression)
            {
                string newFilepath = "";
                try
                {
                    if (VirtualFileExists(cVirtualPath) > 0)
                    {
                        var oImage = new Tools.Image(goServer.MapPath(cVirtualPath));
                        // calculate the new filename
                        newFilepath = Strings.Replace(cVirtualPath, ".jpg", sSuffix + ".jpg");
                        if (!(VirtualFileExists(newFilepath) > 0))
                        {
                            oImage.KeepXYRelation = true;
                            oImage.SetMaxSize((int)maxWidth, (int)maxHeight);
                            oImage.Save(goServer.MapPath(newFilepath), nCompression);
                            return newFilepath;
                        }
                        else
                        {
                            return newFilepath;
                        }
                    }
                    else
                    {
                        return "Source Image Not Found";
                    }
                }


                catch (Exception ex)
                {
                    return "";
                }
            }



            public new string replacestring(string text, string replace, string replaceWith)
            {
                try
                {
                    return base.replacestring(text, replace, replaceWith);
                }
                catch (Exception ex)
                {
                    return text;
                }
            }

            public string replacestrings(string text, string replaceCSV, string replaceWithCSV)
            {
                try
                {
                    string[] cReplace = Strings.Split(replaceCSV, ",");
                    string[] cReplaceWith = Strings.Split(replaceWithCSV, ",");
                    for (int i = 0, loopTo = cReplace.Length - 1; i <= loopTo; i++)
                    {
                        string replaceWith = "";
                        if (cReplaceWith.Length >= i)
                            replaceWith = cReplaceWith[i];
                        base.replacestring(text, cReplace[i], replaceWith);
                    }
                    return text;
                }
                catch (Exception ex)
                {
                    return text;
                }
            }

            public string LeftBefore(string Text, string Character)
            {
                try
                {
                    if (!Text.Contains(Character))
                        return Text;
                    return Text.Substring(0, Text.IndexOf(Character));
                }
                catch (Exception ex)
                {
                    return "";
                }
            }

            public string LeftBeforeChr(string Text, int Chr)
            {
                return LeftBefore(Text, Conversions.ToString(Strings.Chr(Chr)));
            }

            public string RightAfter(string Text, string Character)
            {
                try
                {
                    if (!Text.Contains(Character))
                        return Text;
                    int nPos = Text.IndexOf(Character) + 1;
                    return Text.Substring(nPos, Text.Length - nPos);
                }
                catch (Exception ex)
                {
                    return "";
                }
            }

            public string RightAfterChr(string Text, int Chr)
            {
                return RightAfter(Text, Conversions.ToString(Strings.Chr(Chr)));
            }

            #endregion

            #region Eonicweb Specific

            public int setContentLocationByRef(string cStructName, int nContentId, int bPrimary, int bCascade)
            {
                try
                {
                    string nID = ""; // myWeb.moDbHelper.getKeyByNameAndSchema(Cms.dbHelper.objectTypes.ContentStructure, "", cStructName)
                    nID = Convert.ToString(myWeb.moDbHelper.getObjectByRef(Cms.dbHelper.objectTypes.ContentStructure, cStructName));
                    if (string.IsNullOrEmpty(nID))
                        nID = 0.ToString();
                    if (Conversions.ToDouble(nID) > 0d)
                    {

                        return myWeb.moDbHelper.setContentLocation(Convert.ToInt64(nID), nContentId, Convert.ToBoolean(bPrimary)? true: false, Convert.ToBoolean(bCascade), true);
                    }
                    else
                    {
                        return 0;
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(mcModuleName, "setContentLocation", ex, "");
                    return 0;
                }
            }

            public string setContentLocationsByRef(string cStructName, int nContentId, int bPrimary, int bCascade)
            {
                try
                {
                    string[] nIDs = null; // myWeb.moDbHelper.getKeyByNameAndSchema(Cms.dbHelper.objectTypes.ContentStructure, "", cStructName)
                    int i;
                    nIDs = myWeb.moDbHelper.getObjectsByRef(Cms.dbHelper.objectTypes.ContentStructure, cStructName);

                    if (nIDs != null)
                    {
                        var loopTo = nIDs.Length - 1;
                        for (i = 0; i <= loopTo; i++)
                            myWeb.moDbHelper.setContentLocation(Convert.ToInt64(nIDs[i]), nContentId, Convert.ToBoolean(bPrimary)? true:false, Convert.ToBoolean(bCascade), true);
                        return string.Concat(nIDs);
                    }
                    else
                    {
                        return 0.ToString();
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(mcModuleName, "setContentLocation", ex, "");
                    return 0.ToString();
                }
            }

            public string GetSetting(string SettingName)
            {
                try
                {
                    string cReturn = moSyncConfig[SettingName];
                    if (string.IsNullOrEmpty(cReturn) | string.IsNullOrEmpty(cReturn))
                        cReturn = moConfig[SettingName];
                    return cReturn;
                }
                catch (Exception ex)
                {
                    return "";
                }
            }

            public string SetRef(Protean.Cms.dbHelper.objectTypes objecttype, int id, string value)
            {
                try
                {
                    string cTableName = myWeb.moDbHelper.getTable(objecttype);
                    string cRefField = myWeb.moDbHelper.getFRef(objecttype);
                    string cWhereField = myWeb.moDbHelper.TableKey((Cms.dbHelper.TableNames)objecttype);
                    string cSQL = "UPDATE " + cTableName + " SET " + cRefField + " = '" + value + "' WHERE " + cWhereField + " = " + id;
                    myWeb.moDbHelper.ExeProcessSql(cSQL);
                    return value;
                }
                catch (Exception ex)
                {
                    return "";
                }
            }


            // this was stupidly named
            public int RemoveLocationsByRef(string cValidStructNames, long nContentId)
            {
                int i = 0;
                try
                {
                    return RemoveLocationsRefNotIncluded(cValidStructNames, nContentId);
                }
                catch (Exception ex)
                {
                    return i;
                }
            }

            public int RemoveLocationsRefNotIncluded(string cValidStructNames, long nContentId)
            {
                int i = 0;
                try
                {
                    cValidStructNames = Strings.Replace(cValidStructNames, "'',", "");
                    cValidStructNames = Strings.Replace(cValidStructNames, ",''", "");
                    // don't remove from pages with no foriegn ref's
                    cValidStructNames = cValidStructNames + ",''";
                    if (string.IsNullOrEmpty(cValidStructNames))
                        return 0;
                    string cSQL = "SELECT tblContentLocation.nContentLocationKey FROM tblContentLocation INNER JOIN tblContentStructure ON tblContentLocation.nStructId = tblContentStructure.nStructKey";
                    cSQL += " WHERE (NOT (tblContentStructure.cStructForiegnRef IN (" + cValidStructNames + "))) AND nContentId = " + nContentId;
                    using (SqlDataReader oDR = myWeb.moDbHelper.getDataReaderDisposable(cSQL))  // Done by nita on 6/7/22
                    {
                        while (oDR.Read())
                        {
                            myWeb.moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.ContentLocation, Convert.ToInt64(oDR[0]));
                            i += 1;
                        }
                        oDR.Close();
                    }
                    return i;
                }
                catch (Exception ex)
                {
                    return i;
                }
            }

            public int RemoveLocationsWithRef(string cRemoveRefs, long nContentId)
            {
                int i = 0;
                try
                {
                    cRemoveRefs = Strings.Replace(cRemoveRefs, "'',", "");
                    cRemoveRefs = Strings.Replace(cRemoveRefs, ",''", "");
                    if (string.IsNullOrEmpty(cRemoveRefs))
                        return 0;
                    string cSQL = "SELECT tblContentLocation.nContentLocationKey FROM tblContentLocation INNER JOIN tblContentStructure ON tblContentLocation.nStructId = tblContentStructure.nStructKey";
                    cSQL += " WHERE (tblContentStructure.cStructForiegnRef IN (" + cRemoveRefs + ")) AND nContentId = " + nContentId;
                    using (SqlDataReader oDr = myWeb.moDbHelper.getDataReaderDisposable(cSQL))  // Done by nita on 6/7/22
                    {
                        while (oDr.Read())
                        {
                            myWeb.moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.ContentLocation, Convert.ToInt64(oDr[0]));
                            i += 1;
                        }
                        oDr.Close();
                    }
                    return i;
                }
                catch (Exception ex)
                {
                    return i;
                }
            }

            /// <summary>
            /// Function to be called from an import XSLT to allow the deletion of content not contained in ValidContentNames.
            /// ONLY REMOVES CONTENT RELEATION FOR CONTENTS THAT ARE THE SAME TYPE AS SPECIFICALLY FOR PRODUCT IMPORTS
            /// </summary>
            /// <param name="cValidContentNames">List of Frefs for content for which we don't want to remove the relationship for</param>
            /// <param name="nContentId">The Content Id of the item we are handing relations for.</param>
            /// <param name="cContentTypeToRemove">The content Type of the Content Type we are removing from.</param>
            /// <returns>A count of the number of content items removed</returns>
            /// <remarks></remarks>
            public int RemoveRelatedContentByRef(string cValidContentNames, int nContentId, string cContentTypeToRemove)
            {
                int i = 0;
                try
                {
                    cValidContentNames = Strings.Replace(cValidContentNames, "'',", "");
                    cValidContentNames = Strings.Replace(cValidContentNames, ",''", "");
                    if (string.IsNullOrEmpty(cValidContentNames))
                        return 0;
                    string cSQL = "SELECT Rel.nContentRelationKey, Rel.nContentParentId, Rel.nContentChildId, Childs.cContentForiegnRef" + " FROM tblContent Childs INNER JOIN" + " tblContentRelation Rel ON Childs.nContentKey = Rel.nContentChildId INNER JOIN" + " tblContent Parents ON Rel.nContentParentId = Parents.nContentKey WHERE" + " (((Rel.nContentParentId = " + nContentId + ") AND (NOT (Childs.cContentForiegnRef IN (" + cValidContentNames + ")))) OR" + " ((Rel.nContentChildId  = " + nContentId + ") AND (NOT (Childs.cContentForiegnRef IN (" + cValidContentNames + "))))) AND " + " (Childs.cContentSchemaName = '" + cContentTypeToRemove + "' AND  Parents.cContentSchemaName = '" + cContentTypeToRemove + "')";

                    using (SqlDataReader oDr = myWeb.moDbHelper.getDataReaderDisposable(cSQL))  // Done by nita on 6/7/22
                    {
                        while (oDr.Read())
                        {
                            myWeb.moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.ContentRelation, Convert.ToInt64(oDr[0]));
                            i += 1;
                        }
                        oDr.Close();
                    }
                    return i;
                }
                catch (Exception ex)
                {
                    return i;
                }
            }

            public int ResetRelatedByType(int ContentId, string RelatedRefArr, string RelationType, string RelatedContentType, bool TwoWayRelations)
            {
                int i = 0;
                string cSQL;
                string[] oRef = Strings.Split(RelatedRefArr, ",");
                bool DelFlag;
                bool AddFlag;
                try
                {

                    // Select all valid existing content relationships for the contentId returning Foriegn Keys.
                    cSQL = "SELECT Rel.nContentRelationKey, Rel.nContentParentId, Rel.nContentChildId, Childs.cContentForiegnRef" + " FROM tblContent Childs INNER JOIN" + " tblContentRelation Rel ON Childs.nContentKey = Rel.nContentChildId " + " WHERE" + " Rel.nContentParentId = " + ContentId + " " + " AND Childs.cContentSchemaName = '" + RelatedContentType + "'";
                    if (string.IsNullOrEmpty(RelationType))
                    {
                        cSQL += " AND Rel.cRelationType is null";
                    }
                    else
                    {
                        cSQL += " AND Rel.cRelationType = '" + RelationType + "'";
                    }

                    if (TwoWayRelations)
                    {
                        cSQL = cSQL + " UNION ";
                        cSQL = cSQL + " SELECT Rel.nContentRelationKey, Rel.nContentParentId, Rel.nContentChildId, Childs.cContentForiegnRef" + " FROM tblContent Childs INNER JOIN" + " tblContentRelation Rel ON Childs.nContentKey = Rel.nContentParentId " + " WHERE" + " Rel.nContentChildId = " + ContentId + " " + " AND Childs.cContentSchemaName = '" + RelatedContentType + "'";
                        if (string.IsNullOrEmpty(RelationType))
                        {
                            cSQL += " AND Rel.cRelationType is null";
                        }
                        else
                        {
                            cSQL += " AND Rel.cRelationType = '" + RelationType + "'";
                        }
                    }

                    // Step through oRs and delete those not in the Ref Array.
                    using (SqlDataReader oDR = myWeb.moDbHelper.getDataReaderDisposable(cSQL))  // Done by nita on 6/7/22
                    {
                        while (oDR.Read())
                        {
                            DelFlag = true;
                            var loopTo = Information.UBound(oRef);
                            for (i = 0; i <= loopTo; i++)
                            {
                                if (!string.IsNullOrEmpty(oRef[i].Trim()))
                                {
                                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oRef[i], oDR["cContentForiegnRef"], false)))
                                    {
                                        DelFlag = false;
                                    }
                                }
                                else
                                {
                                    DelFlag = false;
                                }
                            }
                            if (DelFlag)
                            {
                                myWeb.moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.ContentRelation, Convert.ToInt64(oDR[0]));
                            }
                        }

                        // Step through ref array and add those not found in the oRs
                        var loopTo1 = Information.UBound(oRef);
                        for (i = 0; i <= loopTo1; i++)
                        {
                            if (!string.IsNullOrEmpty(oRef[i].Trim()))
                            {
                                AddFlag = true;
                                while (oDR.Read())
                                {
                                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oRef[i].Trim(), oDR["cContentForiegnRef"], false)))
                                    {
                                        AddFlag = false;
                                    }
                                }
                                if (AddFlag)
                                {
                                    AddContentRelationByRef(ContentId, oRef[i].Trim(), TwoWayRelations, RelationType);
                                }
                            }
                        }

                        oDR.Close();
                    }
                    return i;
                }
                catch (Exception ex)
                {
                    return i;
                }
            }


            public string AddContentRelationByRef(int nContentId, string cContentRef)
            {
                try
                {

                    return AddContentRelationByRef(nContentId, cContentRef, true, "");
                }

                catch (Exception ex)
                {
                    return " Relation Error";
                }
            }

            public string AddContentRelationByRef(int nContentId, string cContentRef, bool TwoWay, string Type)
            {
                try
                {
                    if (nContentId == 0)
                        return "No ContentId";
                    if (string.IsNullOrEmpty(cContentRef))
                        return "No ContentRef";
                    int nRefId =Convert.ToInt32(myWeb.moDbHelper.GetDataValue("SELECT nContentKey, cContentForiegnRef FROM tblContent WHERE (cContentForiegnRef = '" + cContentRef + "')", default, default, 0));
                    if (!(nRefId == 0))
                    {
                        myWeb.moDbHelper.insertContentRelation(nContentId, nRefId.ToString(), TwoWay, Type);
                        return nRefId.ToString();
                    }
                    else
                    {
                        return "Not Related";
                    }
                }
                catch (Exception ex)
                {
                    return " Relation Error";
                }
            }


            #endregion

            private void Xslt_XSLTError(string cInformation)
            {

            }
        }

        #endregion






    }
}