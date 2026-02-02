// ***********************************************************************
// $Library:     eonic.dbhelper
// $Revision:    3.1  
// $Date:        2006-03-02
// $Author:      Trevor Spink (trevor@eonic.co.uk)
// &Website:     www.eonic.co.uk
// &Licence:     All Rights Reserved.
// $Copyright:   Copyright (c) 2002 - 2024 Trevor Spink Consultants Ltd.
// ***********************************************************************

using AngleSharp.Dom;
using AngleSharp.Io;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Protean.Providers.Authentication;
using Protean.Providers.Membership;
using Protean.Providers.Messaging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.FtpClient.Extensions;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Configuration;
using System.Xml;
using static Protean.Cms;
using static Protean.Cms.dbHelper;
using static Protean.Cms.dbImport;
using static Protean.stdTools;
using static Protean.Tools.Xml;

namespace Protean
{



    public partial class Cms
    {

        // Inherits dbTools
        public partial class dbHelper : Tools.Database
        {


            public string importObjects(XmlElement ObjectsXml, string FeedRef = "", string ReParseXsl = "")
            {
                PerfMonLog("DBHelper", "importObjects");
                string cProcessInfo = "";
                string cContentLocationTable = "";

                string cTableName = string.Empty;
                string cTableKey = string.Empty;
                string cTableFRef = string.Empty;

                string cPreviousTableName = string.Empty;

                bool bDeleteNonEntries = false;
                string cDeleteTempTableName = "";
                string cDefiningField = "";
                string cDefiningFieldValue = "";
                string cDeleteTempType = "Content";
                string cDefiningWhereStmt = "";
                bool bSkipExisting = false;
                bool bResetLocations = true;
                long nResetLocationIfHere = 0L;
                //long ProcessedQty = 0L;
                long completeCount = 0L;
                long startNo = 0L;

                try
                {

                    // Do we allready have a feed running that is not complete ?

                    string FeedCheck = "";
                    if (!string.IsNullOrEmpty(FeedRef))
                    {
                        string sSQL = "select TOP 1 cActivityDetail from tblActivityLog where nActivityType = 44 and cActivityDetail like '" + FeedRef + "%' and not(cActivityDetail like '%Complete') and dDateTime > " + sqlDateTime(DateAndTime.DateAdd(DateInterval.Minute, -60, DateTime.Now)) + " order by dDateTime DESC";
                        FeedCheck = ExeProcessSqlScalar(sSQL);
                    }

                    if (!string.IsNullOrEmpty(FeedCheck))
                    {
                        logActivity(ActivityType.Custom1, mnUserId, 0L, 0L, "Previous Feed Still Processing:" + FeedCheck);
                        return "Previous Feed Still Processing:" + FeedCheck;
                        //return default;
                    }
                    else
                    {
                        // Get the last time the feed run and check it completed
                        string sSQL = "select TOP 1 cActivityDetail from tblActivityLog where nActivityType = 44 and cActivityDetail like '" + FeedRef + "%' order by dDateTime DESC ";
                        FeedCheck = ExeProcessSqlScalar(sSQL) + "";
                        if (FeedCheck.EndsWith(" Processed"))
                        {
                            string sProcessesQty = Strings.Mid(FeedCheck, FeedCheck.IndexOf("Objects, ") + 10, FeedCheck.IndexOf(" Processed") - FeedCheck.IndexOf("Objects, ") - 9);
                            if (Information.IsNumeric(sProcessesQty))
                            {
                                startNo = Conversions.ToLong(sProcessesQty);
                                logActivity(ActivityType.Custom1, mnUserId, 0L, 0L, "Previous Feed Restarted:" + startNo);
                            }
                            else
                            {
                                return "StartNo not found:" + FeedCheck;
                                // return default;
                            }
                        }
                    }
                    if (ObjectsXml != null)
                    {

                        cContentLocationTable = getTable(objectTypes.ContentLocation);

                        // NB NEW STUFF ------------
                        // Check that we want to delete missing objects from the spreadsheet (For Content)
                        if (ObjectsXml.SelectSingleNode("DeleteNonEntries[@enabled='true']") != null)
                        {
                            // Now look for the defining field, this allows say the content to only work with a distinct type of object, as defined by the defining field name

                            if (ObjectsXml.SelectSingleNode("DeleteNonEntries/@sqlWhere") != null)
                            {
                                cDefiningWhereStmt = ObjectsXml.SelectSingleNode("DeleteNonEntries/@sqlWhere").InnerText;
                            }

                            if (ObjectsXml.SelectSingleNode("DeleteNonEntries[@enabled='true']/cDefiningField") != null)
                            {
                                cDefiningField = ObjectsXml.SelectSingleNode("DeleteNonEntries[@enabled='true']/cDefiningField").InnerText.ToString();
                                cDefiningFieldValue = ObjectsXml.SelectSingleNode("DeleteNonEntries[@enabled='true']/cDefiningField/@value").InnerText.ToString();
                                bDeleteNonEntries = true;
                                if (ObjectsXml.SelectSingleNode("DeleteNonEntries/@type") != null)
                                {
                                    cDeleteTempType = ObjectsXml.SelectSingleNode("DeleteNonEntries/@type").InnerText.ToString();
                                }
                                cDeleteTempTableName = "temp_" + DateTime.Now.ToString();
                                cDeleteTempTableName = cDeleteTempTableName.Replace("/", "_").Replace(":", "_").Replace(" ", "_");
                                // Remember to import the SP into the database to be used
                                // The next line is currently not used, it was incase of having to use a Store Procedure, however that did not overcome the collation error
                                // Dim cSQL As String = "exec [spCreateImportTable] '" & cDeleteTempTableName & "'"
                                string cSQL = "CREATE TABLE dbo." + cDeleteTempTableName + " (cImportID nvarchar(800), cTableName nvarchar(50))";
                                ExeProcessSql(cSQL);
                            }
                        }

                        // To delete existing Directory Relations (excluding Admin ones)
                        if (ObjectsXml.SelectSingleNode("DeleteDirRelations[@enabled='true']") != null)
                        {
                            string cSql_Relation_Audits = "DELETE tblAudit from tblAudit a " + "Inner Join tblDirectoryRelation r " + "On r.nAuditId = a.nAuditKey " + "Where r.nDirChildId IN ( " + "Select nDirKey " + "From tblDirectory " + "WHERE nDirKey NOT IN (" + "Select d.nDirKey " + "From tblDirectoryRelation r " + "Inner Join tblDirectory d " + "On r.nDirChildId = d.nDirKey " + "WHERE r.nDirParentId = " + "(SELECT nDirKey From tblDirectory Where cDirForiegnRef = 'Administrator')))";

                            myWeb.moDbHelper.ExeProcessSqlorIgnore(cSql_Relation_Audits);

                            string cSql_Relations = "DELETE " + "From tblDirectoryRelation " + "Where nDirChildId IN ( " + "Select nDirKey " + "From tblDirectory " + "WHERE nDirKey NOT IN (" + "Select d.nDirKey " + "From tblDirectoryRelation r " + "Inner Join tblDirectory d " + "On r.nDirChildId = d.nDirKey " + "WHERE r.nDirParentId = " + "(SELECT nDirKey From tblDirectory Where cDirForiegnRef = 'Administrator')))";

                            myWeb.moDbHelper.ExeProcessSqlorIgnore(cSql_Relations);
                        }
                        // NB NEW STUFF ------------

                        if (ObjectsXml.SelectSingleNode("SkipExisting[@enabled='true']") != null)
                        {
                            bSkipExisting = true;
                        }

                        bool bOrphan = ObjectsXml.SelectSingleNode("NoLocations[@enabled='true']") != null;

                        if (ObjectsXml.SelectSingleNode("ResetLocations[@enabled='false']") != null)
                        {
                            bResetLocations = false;
                        }
                        else
                        {
                            bResetLocations = true;
                            XmlElement resetNode = (XmlElement)ObjectsXml.SelectSingleNode("ResetLocations");
                            if (resetNode != null)
                            {
                                if (Information.IsNumeric(resetNode.GetAttribute("enabled")))
                                {
                                    nResetLocationIfHere = Conversions.ToLong(resetNode.GetAttribute("enabled"));
                                }
                            }
                        }

                        long totalInstances = ObjectsXml.SelectNodes("Instance | instance").Count;

                        string ReturnMessage = FeedRef + " Importing " + totalInstances + " Objects";

                        long logId = logActivity(ActivityType.ContentImport, mnUserId, 0L, 0L, ReturnMessage + " Started");

                        var oTransform = new Protean.XmlHelper.Transform(ref myWeb, ReParseXsl, false);
                        // oTransform.XSLFile = ReParseXsl
                        // oTransform.Compiled = False

                        var Tasks = new dbImport(oConn.ConnectionString, mnUserId);

                        short nThreads = (short)Conversions.ToInteger("0" + myWeb.moConfig["ImportThreads"]);
                        if (nThreads == 0)
                            nThreads = 10;
                        ThreadPool.SetMaxThreads(nThreads, nThreads);


                        List<ManualResetEvent> doneEvents = new List<ManualResetEvent>();
                        // Dim eventsDoneEvt As New System.Threading.ManualResetEvent(False)

                        foreach (XmlElement oInstance in ObjectsXml.SelectNodes("Instance | instance"))
                        {
                            completeCount = completeCount + 1L;
                            if (completeCount > startNo)
                            {

                                ImportStateObj stateObj = new dbImport.ImportStateObj();
                                stateObj.oInstance = oInstance;
                                stateObj.LogId = logId;
                                stateObj.FeedRef = FeedRef;
                                stateObj.CompleteCount = completeCount;
                                stateObj.totalInstances = totalInstances;
                                stateObj.bSkipExisting = bSkipExisting;
                                stateObj.bResetLocations = bResetLocations;
                                stateObj.nResetLocationIfHere = nResetLocationIfHere;
                                stateObj.bOrphan = bOrphan;
                                stateObj.bDeleteNonEntries = bDeleteNonEntries;
                                stateObj.cDeleteTempTableName = cDeleteTempTableName;
                                stateObj.cDeleteTempType = cDeleteTempType;
                                stateObj.moTransform = oTransform;
                                stateObj.oResetEvt = new ManualResetEvent(false);
                                if (oInstance.NextSibling is null)
                                {
                                    stateObj.LastItem = true;
                                }
                                else
                                {
                                    stateObj.LastItem = false;
                                }

                                stateObj.cDefiningWhereStmt = cDefiningWhereStmt;
                                stateObj.cDefiningField = cDefiningField;
                                stateObj.cDefiningFieldValue = cDefiningFieldValue;

                                doneEvents.Add(stateObj.oResetEvt);
                                ThreadPool.QueueUserWorkItem(new WaitCallback(Tasks.ImportSingleObject), stateObj);

                                stateObj = null;
                            }
                        }

                        // ' eventsDoneEvt.WaitOne()
                        // '    If System.Threading.WaitHandle.WaitAll(doneEvents, New TimeSpan(0, 0, 5), False) Then

                        updateActivity(logId, ReturnMessage + " Complete");

                        // Clear Page Cache

                        myWeb.ClearPageCache();
                        return ReturnMessage;
                    }
                    // '       End If

                    // Me.updateActivity(logId, "Importing " & totalInstances & "Objects, " & completeCount & " Complete")

                    // 'lets get the object type from the table name.
                    // cTableName = oInstance.FirstChild.Name

                    // 'return the object type from the table name
                    // Dim oTblName As TableNames
                    // For Each oTblName In [Enum].GetValues(GetType(objectTypes))
                    // If oTblName.ToString = cTableName Then Exit For
                    // Next
                    // Dim oObjType As New objectTypes

                    // '^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                    // 'Disabled on 16/09/2008 the following, due to the incompatible assignment of Value to the Object Types 
                    // 'oTblName = oObjType
                    // oObjType = oTblName
                    // '^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

                    // ' The purpose of this is to try to reduce the amount of table name/key/fref calls
                    // ' so to optimise this for bulk use.
                    // If cTableName <> cPreviousTableName Then
                    // cTableKey = getKey(oObjType)
                    // cTableFRef = getFRef(oObjType)
                    // End If



                    // Dim fRefNode As XmlElement = oInstance.SelectSingleNode(cTableName & "/" & cTableFRef)
                    // Dim fRef As String = fRefNode.InnerText

                    // 'We absolutly do not do anything if no fRef
                    // If Not fRef = "" Then
                    // Dim nId As Long
                    // 'lets get an id if we are updating a record with a foriegn Ref

                    // nId = getObjectByRef(cTableName, cTableKey, cTableFRef, oObjType, fRef)

                    // 'nId = myWeb.moDbHelper.getObjectByRef(cTableName, cTableKey, cTableFRef, oObjType, fRef)

                    // 'if we want to replace the fRef
                    // If Not fRefNode.GetAttribute("replaceWith") = "" Then
                    // fRefNode.InnerText = fRefNode.GetAttribute("replaceWith")
                    // End If

                    // oInstance.SelectSingleNode(cTableName & "/" & cTableFRef)

                    // If Not (bSkipExisting And nId <> 0) Then
                    // nId = setObjectInstance(oObjType, oInstance, nId)
                    // End If

                    // ' PerfMonLog("DBHelper", "importObjects", "objectId=" & nId)

                    // processInstanceExtras(nId, oInstance, bResetLocations, bOrphan)

                    // 'NB NEW STUFF ------------
                    // If bDeleteNonEntries Then

                    // Dim cSQL As String = "INSERT INTO dbo." & cDeleteTempTableName & " (cImportID , cTableName) VALUES ('" & SqlFmt(fRef) & "','" & SqlFmt(cTableName) & "')"
                    // Me.ExeProcessSql(cSQL)

                    // End If
                    // 'NB NEW STUFF ------------


                    // End If



                    else
                    {
                        return "";
                    }
                }


                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ImportObjects", ex, cProcessInfo));
                    return "";
                }
            }




            public int processInstanceExtras(long savedId, XmlElement oInstance, bool bResetLocations, bool bOrphan)
            {

                PerfMonLog("DBHelper", "processInstanceExtras", "");

                string cProcessInfo = "";
                int i = 0;
                try
                {
                    string cContentLocationTable = getTable(objectTypes.ContentLocation);
                    // lets get the object type from the table name.
                    if (oInstance is null)
                    {
                        return 0;
                    }
                    else
                    {

                        string cTableName = oInstance.FirstChild.Name;

                        // return the object type from the table name
                        var oTblName = default(TableNames);
                        foreach (TableNames currentOTblName in Enum.GetValues(typeof(objectTypes)))
                        {
                            oTblName = currentOTblName;
                            if ((oTblName.ToString() ?? "") == (cTableName ?? ""))
                                break;
                        }
                        var oObjType = new objectTypes();
                        oObjType = (objectTypes)oTblName;

                        // Type specific additional processes.
                        switch (oObjType)
                        {
                            case objectTypes.Content:
                                {
                                    // now lets sort out the locations
                                    // lets delete previous locations for that content
                                    if (bResetLocations)
                                    {
                                        RemoveContentLocations((int)savedId, cContentLocationTable);
                                    }


                                    // now lets add those specificed
                                    // Process locations
                                    if (!bOrphan)
                                    {
                                        XmlElement oPrmLoc = (XmlElement)oInstance.SelectSingleNode("Location[@primary='true']");
                                        if (oPrmLoc is null)
                                        {
                                            oPrmLoc = (XmlElement)oInstance.SelectSingleNode("Location");
                                        }
                                        foreach (XmlElement oLocation in oInstance.SelectNodes("Location"))
                                        {
                                            long sPrimary = 0L;
                                            long displayOrder = Conversions.ToInteger("0" + oLocation.GetAttribute("displayOrder"));
                                            if (ReferenceEquals(oLocation, oPrmLoc))
                                                sPrimary = 1L;
                                            if (!string.IsNullOrEmpty(oLocation.GetAttribute("foriegnRef")))
                                            {
                                                string cleanFref = oLocation.GetAttribute("foriegnRef");
                                                if (Conversions.ToBoolean(Strings.InStr(cleanFref, "&")))
                                                {
                                                    cleanFref = cleanFref.Replace("&amp;", "&");
                                                }
                                                bool updateLocation = true;
                                                if (sPrimary == 1L)
                                                {
                                                    // does the item have a primary location that does not match the fRef ?
                                                    // if so we want to remove the location associated with the fRef because the client has moved the product manually to a more appropreate page/
                                                    string sSQL = "select count(*)  FROM tblContentLocation cl inner join tblContentStructure cs on cl.nStructId = cs.nStructKey where bPrimary = 1 and nContentId = " + savedId + " and cStructForiegnRef != '" + SqlFmt(cleanFref) + "'";
                                                    if (Conversions.ToDouble(ExeProcessSqlScalar(sSQL)) > 0d)
                                                    {
                                                        // this item has an alternate primary location, then make sure we don't add it 
                                                        updateLocation = false;
                                                        string[] pageids = getObjectsByRef(objectTypes.ContentStructure, cleanFref);
                                                        var loopTo = pageids.Length - 1;
                                                        for (i = 0; i <= loopTo; i++)
                                                            // and delete the existing location for that fRef
                                                            RemoveContentLocation(Conversions.ToLong(pageids[i]), savedId);


                                                    }
                                                }
                                                if (updateLocation)
                                                {
                                                    setContentLocationByRef(cleanFref, (int)savedId, (int)sPrimary, 0, oLocation.GetAttribute("position"), displayOrder);
                                                }
                                            }

                                            else if (!string.IsNullOrEmpty(oLocation.GetAttribute("id")))
                                            {
                                                setContentLocation(Conversions.ToLong(oLocation.GetAttribute("id")), savedId, Conversions.ToBoolean(sPrimary), false, false, oLocation.GetAttribute("position"), true, displayOrder);
                                            }
                                        }
                                    }


                                    // lets look for content relationships
                                    XmlElement oRelation;
                                    foreach (XmlElement currentORelation in oInstance.SelectNodes("Relation"))
                                    {
                                        oRelation = currentORelation;
                                        // Relate this content to an item by either that item's parent ID or the foreign ref
                                        if (!string.IsNullOrEmpty(oRelation.GetAttribute("foriegnRef")))
                                        {
                                            setContentRelationByRef((int)savedId, oRelation.GetAttribute("foriegnRef"), true, oRelation.GetAttribute("type"), true);
                                        }
                                        else if (!string.IsNullOrEmpty(oRelation.GetAttribute("relatedContentId")) && Tools.Number.IsReallyNumeric(oRelation.GetAttribute("relatedContentId")) && Convert.ToInt32(oRelation.GetAttribute("relatedContentId")) > 0 && string.IsNullOrEmpty(oRelation.GetAttribute("direction")))


                                        {
                                            insertContentRelation((int)savedId, Convert.ToInt32(oRelation.GetAttribute("relatedContentId")).ToString(), true, oRelation.GetAttribute("type"), true);
                                        }

                                        else if (oRelation.GetAttribute("relatedContentId").Contains(",") | !string.IsNullOrEmpty(oRelation.GetAttribute("direction")))
                                        {
                                            // remove existing content relations of type
                                            RemoveContentRelationByType(savedId, oRelation.GetAttribute("type"), oRelation.GetAttribute("direction"));

                                            foreach (var relContId in oRelation.GetAttribute("relatedContentId").Split(','))
                                            {
                                                if (Information.IsNumeric(relContId))
                                                {
                                                    if (Strings.LCase(oRelation.GetAttribute("direction")) == "child")
                                                    {
                                                        insertContentRelation(Convert.ToInt32(relContId), savedId.ToString(), false, oRelation.GetAttribute("type"), true);
                                                    }
                                                    else
                                                    {
                                                        insertContentRelation((int)savedId, Convert.ToInt32(relContId).ToString(), true, oRelation.GetAttribute("type"), true);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    foreach (XmlElement currentORelation1 in oInstance.SelectNodes("ProductGroups"))
                                    {
                                        oRelation = currentORelation1;
                                        insertProductGroupRelation((int)savedId, oRelation.GetAttribute("ids"));
                                    }
                                    foreach (XmlElement oRelatedLibraryImages in oInstance.SelectNodes("RelatedLibraryImages"))
                                    {
                                        // createGalleryImages(savedId, oRelatedGalleryImages.InnerText,oRelatedGalleryImages.attribute("skipFirst"))
                                        // function to step through each image in the array, check it exists, get width and height, open LibraryImage Xform,
                                        // Get the instance, set /images/img[@class=display] with src and width and height
                                        // then use setObjectInstance, get the new id and related to the savedId setcontentrelation(newid,savedid).
                                        if (!string.IsNullOrEmpty(oRelatedLibraryImages.GetAttribute("skipFirst")) & !string.IsNullOrEmpty(oRelatedLibraryImages.GetAttribute("type")))
                                        {
                                            CreateLibraryImages((int)savedId, oRelatedLibraryImages.InnerText, oRelatedLibraryImages.GetAttribute("skipFirst"), oRelatedLibraryImages.GetAttribute("type"));
                                        }

                                    }

                                    break;
                                }
                            case objectTypes.Directory:
                                {
                                    foreach (XmlElement oRelation in oInstance.SelectNodes("Relation"))
                                    {
                                        long nloc;

                                        if (!string.IsNullOrEmpty(oRelation.GetAttribute("foriegnRef")))
                                        {
                                            nloc = getObjectByRef(objectTypes.Directory, oRelation.GetAttribute("foriegnRef"), oRelation.GetAttribute("type"));
                                        }
                                        else
                                        {
                                            nloc = Conversions.ToInteger("0" + oRelation.GetAttribute("relatedDirId"));
                                        }

                                        if (nloc > 0L)
                                        {
                                            bool bRemove = false;
                                            if (oRelation.GetAttribute("remove") == "true")
                                            {
                                                bRemove = true;
                                            }
                                            maintainDirectoryRelation(nloc, savedId, bRemove);
                                        }

                                    }

                                    break;
                                }

                            case objectTypes.ContentStructure:
                                {
                                    var oTblName2 = default(TableNames);
                                    foreach (XmlElement oContentInstance in oInstance.SelectNodes("Contents/instance"))
                                    {
                                        // lets get an id if we are updating a record with a foriegn Ref
                                        // return the object type from the table name
                                        string cTableName2 = oContentInstance.FirstChild.Name;
                                        foreach (TableNames currentOTblName2 in Enum.GetValues(typeof(objectTypes)))
                                        {
                                            oTblName2 = currentOTblName2;
                                            if ((oTblName2.ToString() ?? "") == (cTableName2 ?? ""))
                                                break;
                                        }
                                        var oObjType2 = new objectTypes();
                                        oObjType2 = (objectTypes)oTblName2;

                                        long nContentId = 0L;
                                        XmlElement fRefElmt = (XmlElement)oContentInstance.SelectSingleNode(getTable(oObjType2) + "/" + getFRef(oObjType2));
                                        string fRef;
                                        if (fRefElmt != null)
                                        {
                                            fRef = fRefElmt.InnerText;
                                            if (!string.IsNullOrEmpty(fRef))
                                            {
                                                nContentId = myWeb.moDbHelper.getObjectByRef(getTable(oObjType2), getKey((int)oObjType2), getFRef(oObjType2), oObjType2, fRef);
                                            }
                                        }

                                        nContentId = Conversions.ToLong(setObjectInstance(oObjType2, oContentInstance, nContentId));
                                        processInstanceExtras(nContentId, oContentInstance, bResetLocations, bOrphan);

                                    }

                                    break;
                                }

                        }

                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "processInstanceExtras", ex, cProcessInfo));
                    return 0;
                }

                return default;
            }
        }
    }
}