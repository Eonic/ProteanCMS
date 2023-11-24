using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Xml;
using static Protean.stdTools;
using Protean.Tools;

namespace Protean
{

    public partial class Cms
    {


        /// <summary>
    /// Class for sync methods that are specific to Protean.Cms
    /// e.g. syncing fle lists as content to modules
    /// </summary>
    /// <remarks></remarks>
        public class Synchronisation
        {

            public event OnErrorEventHandler OnError;

            public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);
            private const string mcModuleName = "Protean.Cms.Synchronisation";

            private void _OnError(object sender, Tools.Errors.ErrorEventArgs e)
            {
                OnError?.Invoke(sender, e);
            }

            private Cms _myWeb;

            public Synchronisation(ref Cms aWeb)
            {
                _myWeb = aWeb;
            }

            public void SyncModuleContentFromFolder(int id)
            {

                string cProcessInfo = "SyncModuleContentFromFolder";

                try
                {

                    // List
                    var currentRelatedContent = new Dictionary<string, string>();
                    var currentUnrelatedContent = new Dictionary<string, string>();
                    List<string> currentFileList;
                    var filesToAdd = new List<string>();
                    var contentToRelate = new List<string>();
                    var contentIDsToRemoveRelation = new List<string>();

                    string folderPath = "";

                    fsHelper.LibraryType fileType = fsHelper.LibraryType.Image; // Default Value

                    // =====================================================================
                    // Load the module XML
                    // =====================================================================
                    XmlElement moduleContentInstance = _myWeb.moPageXml.CreateElement("ContentInstance");
                    moduleContentInstance.InnerXml = _myWeb.moDbHelper.getObjectInstance(dbHelper.objectTypes.Content, id);

                    // =====================================================================
                    // Get the folder path to work on
                    // Only proceed if there is a sync folder to work on.
                    // =====================================================================
                    XmlElement moduleContentBrief = (XmlElement)moduleContentInstance.SelectSingleNode("tblContent/cContentXmlBrief/Content[@syncLocation]");
                    if (moduleContentBrief is not null && !string.IsNullOrEmpty(moduleContentBrief.GetAttribute("syncLocation")))
                    {



                        if (!EnumUtility.TryParse(typeof(fsHelper.LibraryType), moduleContentBrief.GetAttribute("syncLibraryType"), true, ref fileType))
                        {
                            fileType = fsHelper.LibraryType.Image;
                        }

                        // =====================================================================
                        // Build a list of files currently in the actual folder
                        // =====================================================================
                        folderPath = moduleContentBrief.GetAttribute("syncLocation");
                        currentFileList = _myWeb.moFSHelper.GetFileListByTypeFromRelativePath(folderPath, fileType);



                        // =====================================================================
                        // From existing content, find all content with a foreign reference,
                        // and work out if it's already related to the content
                        // =====================================================================
                        string frefContentQuery = "SELECT DISTINCT c.nContentKey, c.cContentForiegnRef , r.nContentRelationKey " + "FROM	tblContent c " + "INNER JOIN tblAudit a " + "ON c.nAuditId = a.nAuditKey AND c.cContentForiegnRef <> '' And c.cContentForiegnRef IS NOT NULL " + "LEFT JOIN  tblContentRelation r " + "ON r.nContentChildId = c.nContentKey AND nContentParentId = " + id + " ";




                        // & "WHERE " & _myWeb.GetStandardFilterSQLForContent(False)


                        // Dim fRefContentData As SqlDataReader = _myWeb.moDbHelper.getDataReader(frefContentQuery)
                        using (SqlDataReader fRefContentData = _myWeb.moDbHelper.getDataReaderDisposable(frefContentQuery))  // Done by nita on 6/7/22
                        {
                            while (fRefContentData.Read())
                            {
                                // Foreign Ref follows the format <FILE PATH> e.g. /images/myimage.jpg

                                string pathFromFref = fRefContentData[1].ToString();


                                // Now work out if it's related content or if it's not related
                                // Not this assumes that Foreign Refs are unique, which they should be!
                                if (fRefContentData[2] is DBNull)
                                {
                                    cProcessInfo = "problem adding:" + pathFromFref;
                                    if (!currentUnrelatedContent.ContainsKey(pathFromFref))
                                    {
                                        currentUnrelatedContent.Add(pathFromFref, fRefContentData[0].ToString());
                                    }
                                }
                                else if (!currentRelatedContent.ContainsKey(pathFromFref))
                                {
                                    currentRelatedContent.Add(pathFromFref, fRefContentData[0].ToString());
                                }

                            }

                            cProcessInfo = "";
                            fRefContentData.Close();
                        }
                        // =====================================================================
                        // Build a list of files to add
                        // i.e. those that are in the folder but have no content in this scope
                        // =====================================================================
                        filesToAdd = currentFileList.FindAll((filePath) => !(currentRelatedContent.ContainsKey(filePath) | currentUnrelatedContent.ContainsKey(filePath)));

                        // =====================================================================
                        // Build a list of content to relate
                        // i.e. those that are in the folder and have unrelated content
                        // =====================================================================
                        contentToRelate = currentFileList.FindAll((filePath) => currentUnrelatedContent.ContainsKey(filePath));
                        contentToRelate = contentToRelate.ConvertAll((filePath) => currentUnrelatedContent[filePath].ToString());

                        // =====================================================================
                        // Build a list of content to delete the relation to
                        // i.e. those that are not in the folder but appear as related content
                        // in this scope
                        // =====================================================================
                        foreach (KeyValuePair<string, string> relatedContentkvp in currentRelatedContent)
                        {
                            if (!currentFileList.Contains(relatedContentkvp.Key))
                            {
                                contentIDsToRemoveRelation.Add(relatedContentkvp.Value);
                            }
                        }

                    }


                    // =====================================================================
                    // Add files
                    // =====================================================================
                    if (filesToAdd.Count > 0)
                    {
                        // Process the lists (add and update)
                        var webFilesToAdd = new List<fsHelper.WebFile>();
                        webFilesToAdd = filesToAdd.ConvertAll((virtualpath) => convertVirtualPathToWebFile(_myWeb, virtualpath));

                        // Convert the webFiles to XML
                        XmlElement serializedWebFiles = (XmlElement)Xml.SerializeToXml<List<fsHelper.WebFile>>(webFilesToAdd);

                        // Build XML to be converted
                        var importXmlDocument = new XmlDocument();
                        importXmlDocument.LoadXml("<Imports><Source/><Schemas/><Settings><ParentContentID>" + id.ToString() + "</ParentContentID></Settings></Imports>");

                        // Add the Web files to the XML
                        XmlElement importWebFiles = (XmlElement)importXmlDocument.SelectSingleNode("Imports/Source");
                        importWebFiles.InnerXml = serializedWebFiles.InnerXml;

                        // Add the XML schema for specified content schemas
                        XmlElement importSchemas = (XmlElement)importXmlDocument.SelectSingleNode("Imports/Schemas");
                        string contentTypes = moduleContentBrief.GetAttribute("syncContentType");
                        if (string.IsNullOrEmpty(contentTypes))
                            contentTypes = fsHelper.GetDefaultContentSchemaNamesForLibraryType(fileType);
                        var contentTypesList = new List<string>(contentTypes.Split(','));
                        if (contentTypesList.Count > 0)
                        {
                            var blankSchema = new Protean.xForm(ref _myWeb.msException);
                            foreach (string schemaName in contentTypesList)
                            {
                                if (blankSchema.load("/xforms/content/" + schemaName + ".xml", _myWeb.maCommonFolders))
                                {
                                    Xml.AddExistingNode(ref importSchemas, blankSchema.Instance);
                                }
                            }
                        }

                        // syncXSLPath
                        string xslPath = moduleContentBrief.GetAttribute("syncXSLPath");
                        if (string.IsNullOrEmpty(xslPath))
                        {
                            xslPath = "/xsl/import/fileTo" + Enum.GetName(typeof(fsHelper.LibraryType), fileType) + ".xsl";
                        }
                        xslPath = _myWeb.moFSHelper.FindFilePathInCommonFolders(xslPath, _myWeb.maCommonFolders);

                        if (!string.IsNullOrEmpty(xslPath))
                        {

                            var transformer = new Protean.XmlHelper.Transform(ref _myWeb, _myWeb.goServer.MapPath(xslPath), false);
                            _myWeb.PerfMon.Log("Admin", "FileSyncTransform-startxsl");
                            transformer.mbDebug = gbDebug;
                            transformer.ProcessDocument(importXmlDocument);
                            _myWeb.PerfMon.Log("Admin", "FileSyncTransform-endxsl");
                            transformer = (Protean.XmlHelper.Transform)null;

                            if (importXmlDocument.SelectNodes("/Instances/Instance").Count > 0)
                            {
                                _myWeb.moDbHelper.importObjects(importXmlDocument.DocumentElement);
                            }

                        }


                    }

                    // =====================================================================
                    // Add relations to existing content
                    // =====================================================================
                    if (contentToRelate.Count > 0)
                    {
                        _myWeb.moDbHelper.insertContentRelation(id, string.Join(",", contentToRelate.ToArray()));
                    }


                    // =====================================================================
                    // Remove relations to existing content
                    // =====================================================================
                    if (contentIDsToRemoveRelation.Count > 0)
                    {
                        foreach (string contentId in contentIDsToRemoveRelation)
                            _myWeb.moDbHelper.RemoveContentRelation(id, Convert.ToInt64(contentId));
                    }
                }

                // Process the delete list
                // If contentIDsToDelete.Count > 0 Then _myWeb.moDbHelper.BulkContentDelete(contentIDsToDelete)

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, System.Reflection.MethodBase.GetCurrentMethod().Name, ex, cProcessInfo));
                }

            }

            public fsHelper.WebFile convertVirtualPathToWebFile(Cms myWeb, string virtualPath)
            {

                return new fsHelper.WebFile(myWeb.goServer.MapPath(virtualPath), virtualPath, true);

            }


            #region Module Behaviour

            public class Modules
            {

                public event OnErrorEventHandler OnError;

                public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);
                private const string mcModuleName = "Protean.Cms.Synchronisation.Modules";

                private void _OnError(object sender, Tools.Errors.ErrorEventArgs e)
                {
                    OnError?.Invoke(sender, e);
                }

                public Modules()
                {

                }

                public void SyncModuleContentFromFolder(ref Cms myWeb, XmlElement contentNode, int contentId, Global.Protean.Cms.dbHelper.ActivityType editAction)
                {

                    try
                    {
                        var mySync = new Synchronisation(ref myWeb);
                        mySync.SyncModuleContentFromFolder(contentId);
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, System.Reflection.MethodBase.GetCurrentMethod().Name, ex, ""));
                    }

                }

            }

            #endregion


        }




    }
}