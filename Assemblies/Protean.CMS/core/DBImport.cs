// ***********************************************************************
// $Library:     protean.cms.dbimport
// $Revision:    3.1  
// $Date:        2006-03-02
// $Author:      Trevor Spink (trevor@eonic.co.uk)
// &Website:     www.eonic.digital
// &Licence:     All Rights Reserved.
// $Copyright:   Copyright (c) 2002 - 2024 Trevor Spink Consultants Ltd.
// ***********************************************************************

using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Protean
{
    public partial class Cms
    {


        public class dbImport : Tools.Database
        {

            #region New Error Handling
            public new event OnErrorEventHandler OnError;

            public new delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);

            private void _OnError(object sender, Tools.Errors.ErrorEventArgs e)
            {
                OnError?.Invoke(sender, e);
            }

            #endregion

            public string oConnString;
            public long mnUserId;
            public System.Web.HttpContext moCtx;
            private string mcModuleName = "dbImport";

            public dbImport(string cConnectionString, long nUserId, System.Web.HttpContext oCtx = null)
            {
                // MyBase.New(cConnectionString)
                try
                {
                    oConnString = cConnectionString;
                    mnUserId = nUserId;
                    moCtx = oCtx;
                }


                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""));
                }

                base.OnError += _OnError;
            }


            public class ImportStateObj
            {
                public XmlElement oInstance;
                public long LogId;
                public string FeedRef;
                public long CompleteCount;
                public long totalInstances;
                public bool bSkipExisting;
                public bool bResetLocations;
                public long nResetLocationIfHere;
                public bool bOrphan;
                public bool bDeleteNonEntries;
                public string cDeleteTempTableName;
                public string cDeleteTempType;
                public dbHelper modbhelper;
                public Protean.XmlHelper.Transform moTransform;
                public ManualResetEvent oResetEvt;
                public bool LastItem;
                public string cDefiningWhereStmt;
                public string cDefiningField;
                public string cDefiningFieldValue;
            }


            public void ImportSingleObject(object importStateObjObj)
            {
                ImportStateObj importStateObj = (ImportStateObj)importStateObjObj;
                string cTableName = "";
                string cTableKey = "";
                string cTableFRef = "";
                string ErrorMsg = "";
                var ErrorId = default(long);
                string cProcessInfo;
                string fRef = "";
                var modbhelper = new dbHelper(oConnString, mnUserId, moCtx);
                string logMessage;
                try
                {
                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(importStateObj.totalInstances, 0, false)))
                    {
                        logMessage = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(importStateObj.cDeleteTempTableName, " Streaming Objects, "), importStateObj.CompleteCount), " Processed"));
                    }
                    else
                    {
                        logMessage = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(importStateObj.cDeleteTempTableName, " Importing "), importStateObj.totalInstances), " Objects, "), importStateObj.CompleteCount), " Processed"));
                    }
                    modbhelper.ResetConnection(oConnString);
                    if (importStateObj.CompleteCount.ToString().EndsWith("0"))
                    {
                        modbhelper.updateActivity(Conversions.ToLong(importStateObj.LogId), logMessage);
                    }

                    // lets get the object type from the table name.
                    cTableName = Conversions.ToString(importStateObj.oInstance.FirstChild.Name);

                    // return the object type from the table name
                    var oTblName = default(dbHelper.TableNames);
                    foreach (dbHelper.TableNames currentOTblName in Enum.GetValues(typeof(dbHelper.objectTypes)))
                    {
                        oTblName = currentOTblName;
                        if ((oTblName.ToString() ?? "") == (cTableName ?? ""))
                            break;
                    }
                    var oObjType = new dbHelper.objectTypes();

                    // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                    // Disabled on 16/09/2008 the following, due to the incompatible assignment of Value to the Object Types 
                    // oTblName = oObjType
                    oObjType = (dbHelper.objectTypes)oTblName;
                    // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

                    // The purpose of this is to try to reduce the amount of table name/key/fref calls
                    // so to optimise this for bulk use.
                    // If cTableName <> cPreviousTableName Then
                    cTableKey = modbhelper.getKey((int)oObjType);
                    cTableFRef = modbhelper.getFRef(oObjType);
                    // End If

                    XmlElement fRefNode = (XmlElement)importStateObj.oInstance.SelectSingleNode(cTableName + "/" + cTableFRef);
                    fRef = fRefNode.InnerText;

                    
                    long nId;
                    string fRefOld = fRefNode.GetAttribute("oldValue");
                    if (!string.IsNullOrEmpty(fRef))
                    {
                        nId = modbhelper.getObjectByRef(cTableName, cTableKey, cTableFRef, oObjType, fRef);
                    }
                    else {
                        XmlElement idNode = (XmlElement)importStateObj.oInstance.SelectSingleNode(cTableName + "/" + cTableKey);
                        nId = Convert.ToInt64("0" + idNode.InnerText);
                    }

                    // We absolutly do not do anything if no fRef
                    if (!string.IsNullOrEmpty(fRef) | nId > 0)
                    {
                        
                        // lets get an id if we are updating a record with a foriegn Ref
                        if (!string.IsNullOrEmpty(fRefOld))
                        {
                            if (nId == 0L)
                            {
                                // we don't have an new one so we need to rename the old
                                nId = modbhelper.getObjectByRef(cTableName, cTableKey, cTableFRef, oObjType, fRefOld);
                            }
                            else
                            {
                                // we have a new one and that is the one we need to update so we simply delete the old
                                long nOldId = modbhelper.getObjectByRef(cTableName, cTableKey, cTableFRef, oObjType, fRefOld);
                                if (nOldId > 0L)
                                {
                                    modbhelper.DeleteObject(oObjType, nOldId);
                                }

                            }
                        }

                        // if we want to replace the fRef
                        if (!string.IsNullOrEmpty(fRefNode.GetAttribute("replaceWith")))
                        {
                            fRefNode.InnerText = fRefNode.GetAttribute("replaceWith");
                        }

                        importStateObj.oInstance.SelectSingleNode(cTableName + "/" + cTableFRef);

                        modbhelper.ResetConnection(oConnString);

                        if (Conversions.ToBoolean(Operators.AndObject(nId > 0L, importStateObj.oInstance.GetAttribute("delete").Contains("true"))))
                        {
                            modbhelper.DeleteObject(oObjType, nId);
                        }
                        else if (Conversions.ToBoolean(Operators.AndObject(nId > 0L, importStateObj.oInstance.GetAttribute("update").Contains("surgical"))))
                        {
                            // Get origional instance
                            var origInstance = new XmlDocument();

                            // Setupthrough nodes with @surgicalUpdate & update the origional instance
                            origInstance.LoadXml("<instance>" + modbhelper.getObjectInstance(oObjType, nId) + "</instance>");
                            foreach (XmlElement oUpdElmt in (IEnumerable)importStateObj.oInstance.SelectNodes("descendant-or-self::*[@updateSurgical!='']"))
                            {
                                string updXpath = oUpdElmt.GetAttribute("updateSurgical");
                                XmlElement nodeToUpdate = (XmlElement)origInstance.SelectSingleNode("/instance/" + updXpath);
                                if (nodeToUpdate != null)
                                {
                                    if (oUpdElmt.InnerText.Trim() != "surgicalIgnore")
                                    {
                                        nodeToUpdate.InnerText = oUpdElmt.InnerText;
                                    }
                                    foreach (XmlAttribute att in oUpdElmt.Attributes)
                                        nodeToUpdate.SetAttribute(att.Name, att.Value);
                                }
                                else
                                {
                                    ErrorMsg = ErrorMsg + updXpath + " not found";
                                }


                                // clean up sugical update - just in case this failed on insert / can be deleted
                            }
                            foreach (XmlElement oRemoveElmt in origInstance.SelectNodes("descendant-or-self::*[@updateSurgical!='']"))
                                oRemoveElmt.RemoveAttribute("updateSurgical");

                            // save the origional instance
                            nId = Conversions.ToLong(modbhelper.setObjectInstance(oObjType, origInstance.DocumentElement, nId));
                            // run instance extras on update like relate and locate etc.
                            if (Conversions.ToBoolean(importStateObj.oInstance.GetAttribute("update").Contains("locate")))
                            {
                                bool bResetLocations = Conversions.ToBoolean(importStateObj.bResetLocations);
                                if (Conversions.ToBoolean(importStateObj.oInstance.GetAttribute("update").Contains("relocate")))
                                {
                                    bResetLocations = true;
                                }
                                else
                                {
                                    bResetLocations = false;
                                }

                                var xmlDoc = new XmlDocument();
                                modbhelper.moPageXml = xmlDoc;
                                modbhelper.ResetConnection(oConnString);
                                long PrimaryLocation = Conversions.ToLong(Operators.ConcatenateObject("0", modbhelper.GetDataValue("select nStructId from tblContentLocation where bPrimary=1 and nContentId = " + nId)));

                                if (PrimaryLocation == 0L)
                                {
                                    bResetLocations = true;
                                }
                                else
                                {
                                    long resetIfHere = Conversions.ToLong(Operators.ConcatenateObject("0", importStateObj.oInstance.GetAttribute("resetifhere")));
                                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectGreater(importStateObj.nResetLocationIfHere, 0, false)))
                                    {
                                        resetIfHere = Conversions.ToLong(importStateObj.nResetLocationIfHere);
                                    }
                                    if (resetIfHere > 0L)
                                    {
                                        if (PrimaryLocation == resetIfHere)
                                        {
                                            bResetLocations = true;
                                        }
                                    }

                                }
                                modbhelper.processInstanceExtras(nId, (XmlElement)importStateObj.oInstance, bResetLocations, Conversions.ToBoolean(importStateObj.bOrphan));
                            }
                        }
                        else
                        {

                            if (Conversions.ToBoolean(importStateObj.oInstance.GetAttribute("delete").Contains("true")))
                            {
                                importStateObj.bSkipExisting = true;
                                // clean up sugical update as we are doing inserts or straight replacements.
                            }
                            foreach (XmlElement oRemoveElmt in (IEnumerable)importStateObj.oInstance.SelectNodes("descendant-or-self::*[@updateSurgical!='']"))
                            {
                                oRemoveElmt.RemoveAttribute("updateSurgical");
                                if (oRemoveElmt.InnerText.Trim() == "surgicalIgnore")
                                {
                                    oRemoveElmt.InnerText = "";
                                }
                            }

                            XmlElement updateInstance = (XmlElement)importStateObj.oInstance;

                            if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(importStateObj.oInstance.GetAttribute("insert"), "reparse", false)))
                            {
                                // run XSL again on instance....
                                TextWriter oTW = new StringWriter();
                                XmlWriter sWriterOTW = XmlWriter.Create(oTW);
                                TextReader oTR;
                                string cFeedItemXML;
                                XmlDocument oInstanceDoc = new XmlDocument();
                                oInstanceDoc.LoadXml(Conversions.ToString(importStateObj.oInstance.OuterXml));
                                XmlReader oXMLReaderInstance = new XmlNodeReader(oInstanceDoc);
                                importStateObj.moTransform.Process(oXMLReaderInstance, ref sWriterOTW);
                                oTR = new StringReader(oTW.ToString());
                                cFeedItemXML = oTR.ReadToEnd();
                                // remove whitespace
                                var myRegex = new Regex(@">\s*<");
                                cFeedItemXML = myRegex.Replace(cFeedItemXML, "><");
                                // move up a node
                                importStateObj.oInstance.InnerXml = cFeedItemXML;
                                updateInstance = (XmlElement)importStateObj.oInstance.FirstChild;
                            }

                            bool bRelocate = false;

                            object bCommitUpdate = true;

                            if (nId > 0L)
                            {
                                // case for updates
                                if (Conversions.ToBoolean(importStateObj.oInstance.GetAttribute("update").Contains("none")))
                                {
                                    importStateObj.bSkipExisting = true;
                                    bCommitUpdate = false;
                                }
                                if (Conversions.ToBoolean(importStateObj.oInstance.GetAttribute("update").Contains("relocate")))
                                {
                                    bRelocate = true;
                                }
                            }
                            else
                            {
                                bRelocate = true;
                                // case for inserts
                                if (Conversions.ToBoolean(importStateObj.oInstance.GetAttribute("insert").Contains("none")))
                                {
                                    importStateObj.bSkipExisting = true;
                                    bCommitUpdate = false;
                                }
                            }

                            if (Conversions.ToBoolean(bCommitUpdate))
                            {
                                nId = Conversions.ToLong(modbhelper.setObjectInstance(oObjType, updateInstance, nId));
                                if (bRelocate)
                                {
                                    modbhelper.processInstanceExtras(nId, updateInstance, Conversions.ToBoolean(importStateObj.bResetLocations), Conversions.ToBoolean(importStateObj.bOrphan));
                                }
                                cProcessInfo = nId + " Saved";
                            }
                            else
                            {
                                cProcessInfo = nId + "Not Saved";
                            }

                            updateInstance = null;


                        }

                        if (Conversions.ToBoolean(importStateObj.bDeleteNonEntries))
                        {

                            string cSQL = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("INSERT INTO dbo.", importStateObj.cDeleteTempTableName), " (cImportID , cTableName) VALUES ('"), SqlFmt(fRef)), "','"), SqlFmt(cTableName)), "')"));
                            modbhelper.ResetConnection(oConnString);
                            modbhelper.ExeProcessSql(cSQL);

                        }
                        ErrorId = nId;

                    }

                    // update every 10 records
                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(importStateObj.totalInstances, importStateObj.CompleteCount, false)))
                    {
                        modbhelper.updateActivity(Conversions.ToLong(importStateObj.LogId), Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(importStateObj.cDeleteTempTableName, " Imported "), importStateObj.totalInstances), " Objects, "), importStateObj.CompleteCount), " Completed")));
                    }

                    fRefNode = null;

                    if (Conversions.ToBoolean(Operators.AndObject(importStateObj.bDeleteNonEntries, importStateObj.LastItem)))
                    {

                        string cSQL = "";

                        // The following check ensures if the temp table is empty, nothing is deleted
                        // This is incase nothing is imported, maybe due to wrong import XSL
                        string nSizeCheck = "";
                        cSQL = Conversions.ToString(Operators.ConcatenateObject("SELECT * FROM ", importStateObj.cDeleteTempTableName));
                        nSizeCheck = "" + modbhelper.ExeProcessSqlScalar(cSQL);

                        if (!nSizeCheck.Equals(""))
                        {

                            // Remove anything that's not from tblContent (future upgrade to support further tables maybe?)
                            // cSQL = "DELETE FROM " & cDeleteTempTableName & " WHERE cTableName != 'tblContent'"
                            // Me.ExeProcessSql(cSQL)
                            switch (importStateObj.cDeleteTempType)
                            {
                                case "Content":
                                    {
                                        // Delete Content Items
                                        cSQL = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("Select nContentKey FROM tblContent " + "WHERE nContentKey IN (SELECT nContentKey FROM tblContent c " + " LEFT OUTER JOIN ", importStateObj.cDeleteTempTableName), " t "), " ON c.cContentForiegnRef = t.cImportID "));


                                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(importStateObj.cDefiningWhereStmt, "", false)))
                                        {
                                            cSQL = Conversions.ToString(cSQL + Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(" WHERE t.cImportID is null AND c.", importStateObj.cDefiningField), " = '"), SqlFmt(Conversions.ToString(importStateObj.cDefiningFieldValue))), "'"));
                                        }
                                        else
                                        {
                                            cSQL = Conversions.ToString(cSQL + Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(" WHERE t.cImportID is null AND c.", importStateObj.cDefiningField), " = '"), SqlFmt(Conversions.ToString(importStateObj.cDefiningFieldValue))), "' AND "), importStateObj.cDefiningWhereStmt), ""));
                                        }
                                        cSQL += ")";

                                        // Dim oDR As SqlClient.SqlDataReader = myWeb.moDbHelper.getDataReader(cSQL)
                                        using (var oDR = modbhelper.getDataReaderDisposable(cSQL))  // Done by nita on 6/7/22
                                        {
                                            while (oDR.Read())
                                                modbhelper.DeleteObject(dbHelper.objectTypes.Content, Conversions.ToLong(oDR[0]));
                                        }

                                        break;
                                    }
                                case "Directory":
                                    {
                                        // Delete Directory Items
                                        cSQL = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("Select nDirKey FROM tblDirectory " + "WHERE nDirKey IN (SELECT nDirKey FROM tblDirectory d " + " LEFT OUTER JOIN ", importStateObj.cDeleteTempTableName), " t "), " ON d.cDirForiegnRef = t.cImportID "));


                                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(importStateObj.cDefiningWhereStmt, "", false)))
                                        {
                                            cSQL = Conversions.ToString(cSQL + Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(" WHERE t.cImportID is null AND d.", importStateObj.cDefiningField), " = '"), SqlFmt(Conversions.ToString(importStateObj.cDefiningFieldValue))), "'"));
                                        }
                                        else
                                        {
                                            cSQL = Conversions.ToString(cSQL + Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(" WHERE t.cImportID is null AND d.", importStateObj.cDefiningField), " = '"), SqlFmt(Conversions.ToString(importStateObj.cDefiningFieldValue))), "' AND "), importStateObj.cDefiningWhereStmt), ""));
                                        }
                                        cSQL += ")";

                                        using (var oDr = modbhelper.getDataReaderDisposable(cSQL))  // Done by nita on 6/7/22
                                        {

                                            while (oDr.Read())
                                            {
                                                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(oDr[0], 1, false)))
                                                {
                                                    // dont delete admin logon
                                                    modbhelper.DeleteObject(dbHelper.objectTypes.Directory, Conversions.ToLong(oDr[0]));
                                                }
                                            }
                                        }

                                        break;
                                    }
                            }



                        }
                        cSQL = Conversions.ToString(Operators.ConcatenateObject("DROP TABLE ", importStateObj.cDeleteTempTableName));
                        modbhelper.ExeProcessSql(cSQL);
                    }
                }

                catch (Exception ex)
                {
                    modbhelper.logActivity(dbHelper.ActivityType.ValidationError, 0L, 0L, ErrorId, Strings.Right(ex.Message + " - " + ex.StackTrace, 700), fRef);
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ImportSingleObject", ex, ""));

                }
                finally
                {
                    if (importStateObj.oResetEvt != null)
                    {
                        importStateObj.oResetEvt.Set();
                    }
                    modbhelper.CloseConnection();
                    modbhelper = null;
                }
            }
        }


    }
}
