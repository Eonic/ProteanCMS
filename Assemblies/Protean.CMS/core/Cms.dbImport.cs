// ***********************************************************************
// $Library:     protean.cms.dbimport
// $Revision:    3.1  
// $Date:        2006-03-02
// $Author:      Trevor Spink (trevor@eonic.digital)
// &Website:     www.eonic.digital
// &Licence:     Apache-2.0 license
// $Copyright:   Copyright (c) 2002 - 2026 Eonic Digital Group Ltd.
// ***********************************************************************

using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

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
                    if (importStateObj.totalInstances == 0)
                    {
                        logMessage = importStateObj.cDeleteTempTableName + " Streaming Objects, " + importStateObj.CompleteCount + " Processed";
                    }
                    else
                    {
                        logMessage = importStateObj.cDeleteTempTableName + " Importing " + importStateObj.totalInstances +
                                     " Objects, " + importStateObj.CompleteCount + " Processed";
                    }

                    modbhelper.ResetConnection(oConnString);
                    if (importStateObj.CompleteCount.ToString().EndsWith("0"))
                    {
                        modbhelper.updateActivity(Convert.ToInt64(importStateObj.LogId), logMessage);
                    }

                    // lets get the object type from the table name.
                    cTableName = Convert.ToString(importStateObj.oInstance.FirstChild.Name);

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
                    string fRefOld = "";
                    if (fRefNode != null) {                     
                        fRef = fRefNode.InnerText;
                        fRefOld = fRefNode.GetAttribute("oldValue");
                    }                    
                    long nId;
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

                        if (nId > 0 && importStateObj.oInstance.GetAttribute("delete")?.Contains("true") == true)
                        {
                            modbhelper.DeleteObject(oObjType, nId);
                        }
                        else if (nId > 0 && importStateObj.oInstance.GetAttribute("update")?.Contains("surgical") == true)
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
                            nId = Convert.ToInt64(modbhelper.setObjectInstance(oObjType, origInstance.DocumentElement, nId));
                            // run instance extras on update like relate and locate etc.
                            if (Convert.ToBoolean(importStateObj.oInstance.GetAttribute("update").Contains("locate")))
                            {
                                bool bResetLocations = Convert.ToBoolean(importStateObj.bResetLocations);
                                if (Convert.ToBoolean(importStateObj.oInstance.GetAttribute("update").Contains("relocate")))
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
                                string dataValue = Convert.ToString(modbhelper.GetDataValue("select nStructId from tblContentLocation where bPrimary=1 and nContentId = " + nId));
                                long PrimaryLocation = Convert.ToInt64("0" + (dataValue ?? "0"));

                                if (PrimaryLocation == 0L)
                                {
                                    bResetLocations = true;
                                }
                                else
                                {
                                    long resetIfHere = Convert.ToInt64("0" + (importStateObj.oInstance.GetAttribute("resetifhere") ?? "0"));
                                    if (importStateObj.nResetLocationIfHere > 0)
                                    {
                                        resetIfHere = Convert.ToInt64(importStateObj.nResetLocationIfHere);
                                    }
                                    if (resetIfHere > 0L)
                                    {
                                        if (PrimaryLocation == resetIfHere)
                                        {
                                            bResetLocations = true;
                                        }
                                    }

                                }
                                modbhelper.processInstanceExtras(nId, (XmlElement)importStateObj.oInstance, bResetLocations, Convert.ToBoolean(importStateObj.bOrphan));
                            }
                        }
                        else
                        {

                            if (Convert.ToBoolean(importStateObj.oInstance.GetAttribute("delete").Contains("true")))
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

                            if ((importStateObj.oInstance.GetAttribute("insert") ?? "") == "reparse")
                            {
                                // run XSL again on instance....
                                TextWriter oTW = new StringWriter();
                                XmlWriter sWriterOTW = XmlWriter.Create(oTW);
                                TextReader oTR;
                                string cFeedItemXML;
                                XmlDocument oInstanceDoc = new XmlDocument();
                                oInstanceDoc.LoadXml(Convert.ToString(importStateObj.oInstance.OuterXml));
                                XmlReader oXMLReaderInstance = new XmlNodeReader(oInstanceDoc);
                                importStateObj.moTransform.Process(oXMLReaderInstance, ref sWriterOTW);
                                oTR = new StringReader(oTW.ToString());
                                cFeedItemXML = oTR.ReadToEnd();
                                // remove whitespace
                                var myRegex = new Regex(@">\s*<");
                                cFeedItemXML = myRegex.Replace(cFeedItemXML, "><");
                                // move up a node
                                importStateObj.oInstance.InnerXml = cFeedItemXML;
                                updateInstance = (XmlElement)importStateObj.oInstance.SelectSingleNode("*");
                            }

                            bool bRelocate = false;

                            object bCommitUpdate = true;

                            if (nId > 0L)
                            {
                                // case for updates
                                if (Convert.ToBoolean(importStateObj.oInstance.GetAttribute("update").Contains("none")))
                                {
                                    importStateObj.bSkipExisting = true;
                                    bCommitUpdate = false;
                                }
                                if (Convert.ToBoolean(importStateObj.oInstance.GetAttribute("update").Contains("relocate")))
                                {
                                    bRelocate = true;
                                }
                            }
                            else
                            {
                                bRelocate = true;
                                // case for inserts
                                if (Convert.ToBoolean(importStateObj.oInstance.GetAttribute("insert").Contains("none")))
                                {
                                    importStateObj.bSkipExisting = true;
                                    bCommitUpdate = false;
                                }
                            }

                            if (Convert.ToBoolean(bCommitUpdate))
                            {
                                nId = Convert.ToInt64(modbhelper.setObjectInstance(oObjType, updateInstance, nId));
                                if (bRelocate)
                                {
                                    modbhelper.processInstanceExtras(nId, updateInstance, Convert.ToBoolean(importStateObj.bResetLocations), Convert.ToBoolean(importStateObj.bOrphan));
                                }
                                cProcessInfo = nId + " Saved";
                            }
                            else
                            {
                                cProcessInfo = nId + "Not Saved";
                            }

                            updateInstance = null;


                        }

                        if (Convert.ToBoolean(importStateObj.bDeleteNonEntries))
                        {

                            string cSQL = "INSERT INTO dbo." + importStateObj.cDeleteTempTableName + " (cImportID , cTableName) VALUES ('" + SqlFmt(fRef) + "','" + SqlFmt(cTableName) + "')";
                            modbhelper.ResetConnection(oConnString);
                            modbhelper.ExeProcessSql(cSQL);

                        }
                        ErrorId = nId;

                    }

                    // update every 10 records
                    if (importStateObj.totalInstances == importStateObj.CompleteCount)
                    {
                        string message = importStateObj.cDeleteTempTableName + " Imported " + importStateObj.totalInstances + " Objects, " +  importStateObj.CompleteCount + " Completed";
                        modbhelper.updateActivity(Convert.ToInt64(importStateObj.LogId), message);
                    }


                    fRefNode = null;

                    if (importStateObj.bDeleteNonEntries && importStateObj.LastItem)
                    {

                        string cSQL = "";

                        // The following check ensures if the temp table is empty, nothing is deleted
                        // This is incase nothing is imported, maybe due to wrong import XSL
                        string nSizeCheck = "";
                        cSQL = "SELECT * FROM " + importStateObj.cDeleteTempTableName;
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
                                        cSQL = "Select nContentKey FROM tblContent " + "WHERE nContentKey IN (SELECT nContentKey FROM tblContent c " + " LEFT OUTER JOIN " + importStateObj.cDeleteTempTableName + " t " + " ON c.cContentForiegnRef = t.cImportID ";

                                        if (string.IsNullOrEmpty(importStateObj.cDefiningWhereStmt))
                                        {
                                            cSQL += " WHERE t.cImportID is null AND c." + importStateObj.cDefiningField + " = '" + SqlFmt(Convert.ToString(importStateObj.cDefiningFieldValue)) + "'";
                                        }
                                        else
                                        {
                                            cSQL += " WHERE t.cImportID is null AND c." + importStateObj.cDefiningField + " = '" + SqlFmt(Convert.ToString(importStateObj.cDefiningFieldValue)) + "' AND " + importStateObj.cDefiningWhereStmt;
                                        }
                                        cSQL += ")";

                                        // Dim oDR As SqlClient.SqlDataReader = myWeb.moDbHelper.getDataReader(cSQL)
                                        using (var oDR = modbhelper.getDataReaderDisposable(cSQL))  // Done by nita on 6/7/22
                                        {
                                            while (oDR.Read())
                                                modbhelper.DeleteObject(dbHelper.objectTypes.Content, Convert.ToInt64(oDR[0]));
                                        }

                                        break;
                                    }
                                case "Directory":
                                    {
                                        // Delete Directory Items
                                        cSQL = "Select nDirKey FROM tblDirectory " + "WHERE nDirKey IN (SELECT nDirKey FROM tblDirectory d " + " LEFT OUTER JOIN " + importStateObj.cDeleteTempTableName + " t " + " ON d.cDirForiegnRef = t.cImportID ";

                                        if (string.IsNullOrEmpty(importStateObj.cDefiningWhereStmt))
                                        {
                                            cSQL += " WHERE t.cImportID is null AND d." + importStateObj.cDefiningField + " = '" + SqlFmt(Convert.ToString(importStateObj.cDefiningFieldValue)) + "'";
                                        }
                                        else
                                        {
                                            cSQL += " WHERE t.cImportID is null AND d." + importStateObj.cDefiningField + " = '" + SqlFmt(Convert.ToString(importStateObj.cDefiningFieldValue)) + "' AND " + importStateObj.cDefiningWhereStmt;
                                        }

                                        cSQL += ")";


                                        using (var oDr = modbhelper.getDataReaderDisposable(cSQL))  // Done by nita on 6/7/22
                                        {

                                            while (oDr.Read())
                                            {
                                                if (!oDr[0].Equals(1))
                                                {
                                                    // dont delete admin logon
                                                    modbhelper.DeleteObject(dbHelper.objectTypes.Directory, Convert.ToInt64(oDr[0]));
                                                }
                                            }
                                        }

                                        break;
                                    }
                            }



                        }
                        cSQL = "DROP TABLE " + importStateObj.cDeleteTempTableName;
                        modbhelper.ExeProcessSql(cSQL);
                    }
                }

                catch (Exception ex)
                {
                    modbhelper.logActivity(dbHelper.ActivityType.ValidationError, 0L, 0L, ErrorId, (ex.Message + " - " + ex.StackTrace).Length > 700 ? (ex.Message + " - " + ex.StackTrace).Substring((ex.Message + " - " + ex.StackTrace).Length - 700) : (ex.Message + " - " + ex.StackTrace), fRef);
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
