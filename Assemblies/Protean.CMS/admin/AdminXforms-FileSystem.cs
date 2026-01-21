// ***********************************************************************
// $Library:     protean.cms.adminXforms
// $Revision:    3.1  
// $Date:        2006-03-02
// $Author:      Trevor Spink (trevor@eonic.com)
// &Website:     eonic.com
// &Licence:     Apache-2.0 license
// $Copyright:   Copyright (c) 2002 - 2022 Eonic Digital LLP.
// ***********************************************************************



using Protean.Providers.CDN;
using Protean.Providers.Membership;
using Protean.Providers.Payment;
using Protean.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
//using System.Text.Json.Nodes;
using System.Web;
using System.Web.Configuration;
using System.Xml;
using static Lucene.Net.QueryParsers.QueryParser;
using static Protean.Cms;
using static Protean.stdTools;
using static Protean.Tools.Text;
using static Protean.Tools.Xml;
using static System.Web.HttpUtility;

namespace Protean
{

    public partial class Cms
    {
        public partial class Admin
        {
            public partial class AdminXforms : xForm, IDisposable
            {

                public XmlElement xFrmDeleteFolder(ref string cPath, Protean.fsHelper.LibraryType nType)
                {

                    XmlElement oFrmElmt;
                    string sValidResponse;
                    string cProcessInfo = "";
                    XmlElement oinputElmt;
                    try
                    {
                        // load the xform to be edited
                        moDbHelper.moPageXml = moPageXML;


                        base.NewFrm("DeleteFolder");

                        base.submission("DeleteFolder", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "folderItem", "", "Delete Content");
                        oinputElmt = base.addInput(ref oFrmElmt, "cFolderName", false, "FolderName", "hidden");
                        XmlNode argoNode = oinputElmt;
                        addNewTextNode("value", ref argoNode, cPath);
                        oinputElmt = (XmlElement)argoNode;
                        if (string.IsNullOrEmpty(cPath) | cPath == @"\" | cPath == "/")
                        {
                            //XmlNode argoNode1 = oFrmElmt;
                            base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "You cannot delete the root folder", false, "alert-danger");
                            //oFrmElmt = (XmlElement)argoNode1;
                        }
                        else
                        {
                            //XmlNode argoNode2 = oFrmElmt;
                            base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "Are you sure you want to delete this folder? - \"" + cPath + "\"", false, "alert-danger");
                            //oFrmElmt = (XmlElement)argoNode2;
                            base.addSubmit(ref oFrmElmt, "", "Delete folder");
                        }

                        base.Instance.InnerXml = "<delete/>";

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (string.IsNullOrEmpty(goRequest["cFolderName"]) | goRequest["cFolderName"] == @"\" | goRequest["cFolderName"] == "/")
                            {
                                base.valid = false;
                            }
                            if (base.valid)
                            {

                                var oFs = new Protean.fsHelper();
                                oFs.initialiseVariables(nType);
                                sValidResponse = oFs.DeleteFolder("", cPath);

                                // fsh.DeleteFolder()
                                // cPath = Left(cPath, InStrRev(cPath, "\") - 1)
                                if (sValidResponse != "1")
                                {
                                    base.valid = false;
                                    //XmlNode argoNode3 = oFrmElmt;
                                    base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, sValidResponse);
                                    //oFrmElmt = (XmlElement)argoNode3;
                                    base.addValues();
                                }
                            }
                            else
                            {
                                base.addValues();
                            }
                        }
                        else
                        {
                            base.addValues();
                        }

                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmDeleteFolder", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }
                public XmlElement xFrmDeleteFile(string cPath, string cName, Protean.fsHelper.LibraryType nType)
                {
                    XmlElement oFrmElmt;
                    string sValidResponse;
                    string cProcessInfo = "";

                    try
                    {
                        // load the xform to be edited
                        moDbHelper.moPageXml = moPageXML;


                        base.NewFrm("DeleteFile");

                        base.submission("DeleteFile", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "folderItem", "", "Delete File");

                        //XmlNode argoNode = oFrmElmt;
                        base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Hint, "This file is used in these content Items");
                        //oFrmElmt = (XmlElement)argoNode;
                        // search for file in content and pages
                        var oFsh = new Protean.fsHelper();
                        oFsh.initialiseVariables(nType);
                        string fileToFind = "/" + oFsh.mcRoot + cPath.Replace(@"\", "/") + "/" + Protean.Tools.Database.EscapeFullTextSearch(cName);

                        SqlDataReader oDr;
                        if (myWeb.moDbHelper.checkDBObjectExists("spCheckFileInUse", Database.objectTypes.StoredProcedure))
                        {
                            string sSQL = "spCheckFileInUse";
                            System.Collections.Hashtable arrParms = new System.Collections.Hashtable();
                            arrParms.Add("filePath", fileToFind);
                            oDr = moDbHelper.getDataReaderDisposable(sSQL, CommandType.StoredProcedure, arrParms);
                        }
                        else
                        {
                            string sSQL = "select * from tblContent where cContentXmlBrief like '%" + fileToFind + "%' or cContentXmlDetail like '%" + fileToFind + "%'";
                            oDr = moDbHelper.getDataReaderDisposable(sSQL);
                        }



                        if (oDr.HasRows)
                        {
                            string contentFound = "<p>This file is used in these content Items</p><ul>";
                            while (oDr.Read())
                                contentFound = contentFound + "<li><a href=\"?artid=" + oDr["nContentKey"] + "\" target=\"_new\">" + oDr["cContentSchemaName"] + " - " + oDr["cContentName"] + "</a></li>";
                            //XmlNode argoNode1 = oFrmElmt;
                            base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Hint, contentFound + "</ul>");
                            //oFrmElmt = (XmlElement)argoNode1;
                        }

                        else
                        {
                            //XmlNode argoNode2 = oFrmElmt;
                            base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Hint, "This cannot be found referenced in any content but it may be used in a template or stylesheet");
                            //oFrmElmt = (XmlElement)argoNode2;
                        }
                        oDr.Close();
                        oDr = null;

                        //XmlNode argoNode3 = oFrmElmt;
                        base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "Are you sure you want to delete this file? - \"" + cPath + @"\" + cName + "\"", false, "alert-danger");
                        //oFrmElmt = (XmlElement)argoNode3;

                        base.addSubmit(ref oFrmElmt, "", "Delete file");

                        base.Instance.InnerXml = "<delete/>";

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {

                                var oFs = new Protean.fsHelper();
                                oFs.initialiseVariables(nType);

                                sValidResponse = oFs.DeleteFile(cPath, cName);
                                if (sValidResponse != "1")
                                {
                                    base.valid = false;
                                    //XmlNode argoNode4 = oFrmElmt;
                                    base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, sValidResponse);
                                    //oFrmElmt = (XmlElement)argoNode4;
                                    base.addValues();
                                }
                                else
                                {
                                    DeleteAllInstancesOfOrigianlFile(cPath, cName, oFs);
                                    //Add method for deleteing images from cache
                                    if (sImageUrlslist.Count != 0)
                                    {
                                        string result = "Cached";
                                        string[] myString = sImageUrlslist.ToArray();
                                        Protean.Providers.CDN.ReturnProvider oCdnProv = new Protean.Providers.CDN.ReturnProvider();
                                        ICDNProvider oCDNProvider = oCdnProv.Get(ref myWeb);
                                        if (oCDNProvider != null)
                                        {
                                            result = Convert.ToString(oCDNProvider.AdminXforms.PurgeImageCacheAsync(myString, ref myWeb));
                                        }

                                        //DeleteFileFromCache(myString);
                                    }
                                }
                            }

                            else
                            {
                                base.addValues();
                            }
                        }
                        else
                        {
                            base.addValues();
                        }

                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmEditXFormGroup", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                private void DeleteAllInstancesOfOrigianlFile(string filePath, string fileName, Protean.fsHelper oFs)
                {
                    filePath = filePath.Contains(oFs.mcStartFolder) ? filePath : oFs.mcStartFolder + filePath;
                    var subFolders = System.IO.Directory.GetDirectories(filePath, "~*");
                    try
                    {
                        if (subFolders.Length > 0)
                        {
                            foreach (var sFolder in subFolders)
                            {
                                DeleteAllInstancesOfOrigianlFile(sFolder, fileName, oFs);
                            }
                        }
                        DeleteFiles(filePath, fileName, oFs);
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "TryDeleteAllInstancesOfOrigianlFile", ex, "", "TryDeleteAllInstancesOfOrigianlFile", gbDebug);
                    }
                }

                private void DeleteFiles(string directoryPath, string fileName, Protean.fsHelper oFs)
                {
                    NameValueCollection moCartConfig = (NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/cart");
                    string originalFileNameFull = System.IO.Path.Combine(directoryPath, fileName);
                    var filesToDelete = System.IO.Directory.GetFiles(directoryPath, "*" + fileName);
                    string FilesToDeleteFromCache = string.Empty;
                    bool bFileExists = true;
                    if (moCartConfig["SiteURL"] != null && myWeb.moConfig["ImageRootPath"] != null && myWeb.moConfig["EnableWebP"] != null)
                    {
                        string WebPath = moCartConfig["SiteURL"]; // used it from web.config and cart.config
                        for (int i = 0; i < filesToDelete.Length; i++)
                        {
                            oFs.DeleteFile(filesToDelete[i]);
                            bFileExists = File.Exists(filesToDelete[i]);
                            if (bFileExists)
                            {
                                FilesToDeleteFromCache = filesToDelete[i].Replace(oFs.mcStartFolder, "").Replace(@"\", "/");
                                FilesToDeleteFromCache = WebPath + myWeb.moConfig["ImageRootPath"].Replace("/", "") + FilesToDeleteFromCache;
                                sImageUrlslist.Add(FilesToDeleteFromCache);
                            }

                            if (myWeb.moConfig["EnableWebP"].ToLower() == "on")
                            {
                                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filesToDelete[i]);
                                string newFilePath = Path.Combine(directoryPath, fileNameWithoutExtension + ".webp");
                                bFileExists = File.Exists(newFilePath);
                                if (bFileExists)
                                {
                                    oFs.DeleteFile(newFilePath);
                                    newFilePath = newFilePath.Replace(oFs.mcStartFolder, "").Replace(@"\", "/");
                                    newFilePath = WebPath + myWeb.moConfig["ImageRootPath"].Replace("/", "") + newFilePath;
                                    sImageUrlslist.Add(newFilePath);
                                }
                            }
                        }
                    }
                    oFs.DeleteFile(originalFileNameFull);
                }

                public XmlElement xFrmMoveFile(string cPath, string cName, Protean.fsHelper.LibraryType nType)
                {
                    XmlElement oFrmElmt;
                    string sValidResponse = string.Empty;
                    string cProcessInfo = "xFrmMoveFile";
                    try
                    {
                        // load the xform to be edited
                        moDbHelper.moPageXml = moPageXML;
                        base.NewFrm("MoveFile");
                        base.submission("MoveFile", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "folderItem", "", "Move File");

                        // search for file in content and pages
                        var oFsh = new Protean.fsHelper();
                        oFsh.initialiseVariables(nType);

                        string fileToFind = "/" + oFsh.mcRoot + cPath.Replace(@"\", "/").Replace("//", "/");

                        if (fileToFind.EndsWith("/"))
                        {
                            fileToFind = fileToFind + cName;
                        }
                        else
                        {
                            fileToFind = fileToFind + "/" + cName;
                        }

                        string sSQL = "select * from tblContent where cContentXmlBrief like '%" + fileToFind + "%' or cContentXmlDetail like '%" + fileToFind + "%'";
                        using (var oDr = moDbHelper.getDataReaderDisposable(sSQL))  // Done by nita on 6/7/22
                        {
                            if (oDr is null)
                            {
                                //XmlNode argoNode = oFrmElmt;
                                base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Hint, "This cannot be found referenced in any content but it may be used in a template or stylesheet");
                                //oFrmElmt = (XmlElement)argoNode;
                            }

                            else if (oDr.HasRows)
                            {
                                string contentFound = "<p>This file is used in these content Items</p><ul>";
                                string artIds = "";
                                while (oDr.Read())
                                {
                                    contentFound = contentFound + "<li><a href=\"?artid=" + oDr["nContentKey"] + "\" target=\"_new\">" + oDr["cContentSchemaName"] + " - " + oDr["cContentName"] + "</a></li>";
                                    artIds = oDr["nContentKey"] + ",";
                                }
                                //XmlNode argoNode1 = oFrmElmt;
                                base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Hint, contentFound + "</ul>");
                                //oFrmElmt = (XmlElement)argoNode1;

                                var oSelUpd = base.addSelect1(ref oFrmElmt, "UpdatePaths", false, "Update Paths", "", Protean.xForm.ApperanceTypes.Full);
                                base.addOption(ref oSelUpd, "Yes", artIds.TrimEnd(','));
                                base.addOption(ref oSelUpd, "No", "0");
                            }
                            else
                            {
                                //XmlNode argoNode2 = oFrmElmt;
                                base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Hint, "This cannot be found referenced in any content but it may be used in a template or stylesheet");
                                //oFrmElmt = (XmlElement)argoNode2;
                            }
                        }

                        var oSelElmt = base.addSelect1(ref oFrmElmt, "destPath", false, "Move To");
                        base.addOptionsFoldersFromDirectory(ref oSelElmt, "/" + oFsh.mcRoot);

                        //XmlNode argoNode3 = oFrmElmt;
                        base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "Are you sure you want to move this file? - \"" + cPath + @"\" + cName + "\"", false, "alert-danger");
                        //oFrmElmt = (XmlElement)argoNode3;



                        base.addSubmit(ref oFrmElmt, "", "Move file");

                        base.Instance.InnerXml = "<delete/>";

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {

                                var oFs = new Protean.fsHelper();
                                oFs.initialiseVariables(nType);
                                string cDestPath = myWeb.moRequest["destPath"].Replace(oFs.mcRoot, "").Replace("//", "/");

                                if (oFs.MoveFile(cName, cPath, cDestPath))
                                {

                                    if (myWeb.moRequest["UpdatePaths"] != "0" & !string.IsNullOrEmpty(myWeb.moRequest["UpdatePaths"]))
                                    {
                                        string fileToReplace = "/" + oFs.mcRoot + cDestPath.Replace(@"\", "/") + "/" + cName.Replace(" ", "-");
                                        string sSQLUpd = "Update tblContent set cContentXmlBrief = REPLACE(CAST(cContentXmlBrief AS NVARCHAR(MAX)),'" + fileToFind + "','" + fileToReplace + "'), cContentXmlDetail = REPLACE(CAST(cContentXmlDetail AS NVARCHAR(MAX)),'" + fileToFind + "','" + fileToReplace + "') where nContentKey IN (" + myWeb.moRequest["UpdatePaths"] + ")";
                                        moDbHelper.ExeProcessSql(sSQLUpd);
                                    }
                                }

                                else
                                {
                                    base.valid = false;
                                    //XmlNode argoNode4 = oFrmElmt;
                                    base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "File move error");
                                    //oFrmElmt = (XmlElement)argoNode4;
                                    base.addValues();

                                }
                            }


                            else
                            {
                                base.addValues();
                            }
                        }
                        else
                        {
                            base.addValues();
                        }

                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmMoveFile", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }


                public XmlElement xFrmAddFolder(ref string cPath, Protean.fsHelper.LibraryType nType)
                {
                    XmlElement oFrmElmt;
                    string sValidResponse = "";
                    string cProcessInfo = "";
                    string SubmitPath = "?ewcmd=";
                    try
                    {

                        SubmitPath = SubmitPath + myWeb.moRequest["ewcmd"] + "&ewCmd2=" + myWeb.moRequest["ewCmd2"] + "&pathonly=" + myWeb.moRequest["pathonly"] + "&targetForm=" + myWeb.moRequest["targetForm"] + "&targetField=" + myWeb.moRequest["targetField"];

                        base.NewFrm("AddFolder");

                        base.submission("AddFolder", SubmitPath, "post", "");

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "New Folder", "ptn-admin-form", "Please enter the folder name");
                        base.addInput(ref oFrmElmt, "fld", true, "Path", "readonly");
                        XmlElement argoBindParent = null;
                        base.addBind("fld", "folder/@path", oBindParent: ref argoBindParent, "false()");

                        base.addInput(ref oFrmElmt, "cFolderName", true, "Folder Name");
                        XmlElement argoBindParent1 = null;
                        base.addBind("cFolderName", "folder/@name", oBindParent: ref argoBindParent1, "true()");

                        base.addSubmit(ref oFrmElmt, "AddFolder", "Create Folder", "ewSubmit");

                        base.Instance.InnerXml = "<folder path=\"" + cPath + "\" name=\"\"/>";

                        if (base.isSubmitted() | !string.IsNullOrEmpty(moRequest["cFolderName"]))
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {

                                string FolderName = goRequest["cFolderName"];

                                var oFs = new Protean.fsHelper();
                                oFs.initialiseVariables(nType);
                                sValidResponse = oFs.CreateFolder(HtmlDecode(FolderName), cPath);

                                if (Tools.Number.IsNumeric(sValidResponse))
                                {
                                    valid = true;
                                    cPath += @"\" + FolderName.Replace(" ", "-");
                                    cPath = cPath.Replace(@"\\", @"\");
                                }
                                else
                                {
                                    valid = false;
                                    //XmlNode argoNode = (XmlNode)this.moXformElmt;
                                    base.addNote(ref moXformElmt, Protean.xForm.noteTypes.Alert, sValidResponse);
                                    //this.moXformElmt = (XmlElement)argoNode;
                                }
                            }
                            else
                            {
                                valid = false;
                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmAddFolder", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmUpload(string cPath, Protean.fsHelper.LibraryType nType)
                {
                    XmlElement oFrmElmt;
                    string sValidResponse = "";
                    string cProcessInfo = "";
                    try
                    {
                        base.NewFrm("UploadFile");

                        base.submission("Upload File", "", "post", "form_check(this)");

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "New File", "", "Please select the file to upload");
                        base.addInput(ref oFrmElmt, "fld", true, "Upload Path", "readonly");
                        XmlElement argoBindParent = null;
                        base.addBind("fld", "file/@path", oBindParent: ref argoBindParent, "true()");

                        string argsClass = "";
                        base.addUpload(ref oFrmElmt, "uploadFile", true, "image/*", "Pick File", sClass: ref argsClass);
                        XmlElement argoBindParent1 = null;
                        base.addBind("uploadFile", "file", oBindParent: ref argoBindParent1, "true()");

                        base.addSubmit(ref oFrmElmt, "", "Upload", "ewSubmit");

                        base.Instance.InnerXml = "<file path=\"" + cPath + "\" filename=\"\" mediatype=\"\"/>";

                        if (base.isSubmitted())
                        {

                            base.updateInstanceFromRequest();

                            // lets do some hacking 
                            System.Web.HttpPostedFile fUpld;
                            fUpld = goRequest.Files["uploadFile"];

                            if (fUpld != null)
                            {
                                base.valid = true;
                            }

                            if (base.valid)
                            {

                                var oFs = new Protean.fsHelper();
                                oFs.initialiseVariables(nType);
                                sValidResponse = oFs.SaveFile(ref fUpld, cPath);

                                if ((sValidResponse ?? "") == (fUpld.FileName ?? ""))
                                {
                                    valid = true;
                                }
                                // MyBase.addNote(moXformElmt, xForm.noteTypes.Alert, sValidResponse & " - File Saved")
                                else
                                {
                                    valid = false;
                                    //XmlNode argoNode = (XmlNode)this.moXformElmt;
                                    base.addNote(ref moXformElmt, Protean.xForm.noteTypes.Alert, sValidResponse);
                                    // this.moXformElmt = (XmlElement)argoNode;
                                }
                            }
                            else
                            {
                                valid = false;
                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmUpload", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmMultiUpload(string cPath, Protean.fsHelper.LibraryType nType)
                {
                    XmlElement oFrmElmt;
                    //string sValidResponse = "";
                    string cProcessInfo = "";
                    string rootDir = @"\";
                    try
                    {

                        switch (nType)
                        {
                            case Protean.fsHelper.LibraryType.Image:
                                {
                                    rootDir = myWeb.moConfig["ImageRootPath"];
                                    break;
                                }
                            case Protean.fsHelper.LibraryType.Documents:
                                {
                                    rootDir = myWeb.moConfig["DocRootPath"];
                                    break;
                                }
                            case Protean.fsHelper.LibraryType.Media:
                                {
                                    rootDir = myWeb.moConfig["MediaRootPath"];
                                    break;
                                }
                        }

                        myWeb.moSession["allowUpload"] = "True";

                        base.NewFrm("UploadFile");

                        base.submission("Upload File", "", "post", "");

                        cPath = cPath.Replace(@"\", "/");

                        if (cPath.StartsWith("/"))
                        {
                            cPath = cPath.Substring(1);
                        }

                        string SavePath = rootDir + cPath;

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "New File", "", "Please select files to upload to " + SavePath);
                        base.addInput(ref oFrmElmt, "fld", true, "Upload Path", "hidden");
                        XmlElement argoBindParent = null;
                        base.addBind("fld", "file/@path", oBindParent: ref argoBindParent, "true()");

                        string argsClass = "MultiPowUpload";
                        base.addUpload(ref oFrmElmt, "uploadFile", true, "image/*", "", ref argsClass);
                        XmlElement argoBindParent1 = null;
                        base.addBind("uploadFile", "file", oBindParent: ref argoBindParent1, "true()");

                        base.addSubmit(ref oFrmElmt, "", "Finish", "ewSubmit");

                        base.Instance.InnerXml = "<file path=\"" + SavePath + "\" filename=\"\" mediatype=\"\"/>";

                        if (base.isSubmitted())
                        {

                            // do nothing
                            valid = true;
                            myWeb.moSession["allowUpload"] = (object)null;

                        }

                        base.addValues();
                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmMultiUpload", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmPickImage(string cPathName, string cTargetForm, string cTargetFeild, string cClassName = "")
                {
                    XmlElement oFrmElmt;
                    XmlElement oElmt;
                    string sValidResponse = "0";
                    string cProcessInfo = "";
                    try
                    {
                        if (string.IsNullOrEmpty(cTargetForm))
                            cTargetForm = "ContentForm";

                        base.NewFrm("AddFolder");

                        base.submission("imageDetailsForm", "", "post", "form_check(this);passImgToForm('" + cTargetForm + "','" + cTargetFeild + "');return(false);");

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "Image Details", "", "Please enter image description");

                        base.addInput(ref oFrmElmt, "cName", true, "Class", "readonly");
                        XmlElement argoBindParent = null;
                        base.addBind("cName", "img/@class", oBindParent: ref argoBindParent, "true()");

                        base.addInput(ref oFrmElmt, "cPathName", true, "Path Name");
                        XmlElement argoBindParent1 = null;
                        base.addBind("cPathName", "img/@src", oBindParent: ref argoBindParent1, "true()");

                        base.addInput(ref oFrmElmt, "nWidth", true, "Width");
                        XmlElement argoBindParent2 = null;
                        base.addBind("nWidth", "img/@width", oBindParent: ref argoBindParent2, "true()");

                        base.addInput(ref oFrmElmt, "nHeight", true, "Height");
                        XmlElement argoBindParent3 = null;
                        base.addBind("nHeight", "img/@height", oBindParent: ref argoBindParent3, "true()");

                        base.addInput(ref oFrmElmt, "cDesc", true, "Alt Description");
                        XmlElement argoBindParent4 = null;
                        base.addBind("cDesc", "img/@alt", oBindParent: ref argoBindParent4, "false()");


                        base.addSubmit(ref oFrmElmt, "", "Add Image", "ewSubmit");

                        var oFs = new Protean.fsHelper();
                        oFs.initialiseVariables(Protean.fsHelper.LibraryType.Image);
                        base.Instance.InnerXml = oFs.getImageXhtml(cPathName);


                        if (!string.IsNullOrEmpty(cClassName))
                        {
                            oElmt = (XmlElement)base.Instance.FirstChild;
                            if (oElmt != null)
                            {
                                oElmt.SetAttribute("class", cClassName);
                            }
                        }

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {

                                if (Tools.Number.IsNumeric(sValidResponse))
                                {
                                    valid = true;
                                }
                                // MyBase.jsOnLoad()
                                else
                                {
                                    valid = false;
                                    //XmlNode argoNode = (XmlNode)this.moXformElmt;
                                    base.addNote(ref moXformElmt, Protean.xForm.noteTypes.Alert, sValidResponse);
                                    //this.moXformElmt = (XmlElement)argoNode;
                                }
                            }
                            else
                            {
                                valid = false;
                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmPickImage", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                // Public Function xFrmPickDocument(ByVal cPathName As String, ByVal cTargetFeild As String, Optional ByVal cClassName As String = "") As XmlElement
                // Dim oFrmElmt As XmlElement
                // Dim oElmt As XmlElement
                // Dim sValidResponse As String = ""
                // Dim cProcessInfo As String = ""
                // Try
                // MyBase.NewFrm("AddFolder")

                // MyBase.submission("documentDetailsForm", "", "post", "form_check(this);passDocToForm('" & cTargetFeild & "');return(false);")

                // oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Document Details", "", "Please enter image description")

                // MyBase.addInput(oFrmElmt, "cPathName", True, "Path Name")
                // MyBase.addBind("cPathName", "a/@href", "true()")

                // MyBase.addSubmit(oFrmElmt, "", "Add Document ", "ewSubmit", "ewSubmit")


                // MyBase.instance.InnerXml = "<a href=""" & Replace(cPathName, "\", "/") & """/>"


                // If cClassName <> "" Then
                // oElmt = MyBase.instance.FirstChild()
                // oElmt.SetAttribute("class", cClassName)
                // End If

                // 'auto submit-
                // MyBase.submit()


                // '------------

                // If MyBase.isSubmitted Then
                // MyBase.updateInstanceFromRequest()
                // MyBase.validate()
                // If MyBase.valid Then

                // If IsNumeric(sValidResponse) Then
                // valid = True
                // 'MyBase.jsOnLoad()
                // Else
                // valid = False
                // MyBase.addNote(moXformElmt, xForm.noteTypes.Alert, sValidResponse)
                // End If
                // Else
                // valid = False
                // End If
                // End If

                // MyBase.addValues()
                // Return MyBase.moXformElmt

                // Catch ex As Exception
                // returnException(myWeb.msException, _moduleName, "addInput", ex, "", cProcessInfo, gbDebug)
                // Return Nothing
                // End Try
                // End Function

                public XmlElement xFrmEditImage(string cImgHtml, string cTargetForm, string cTargetFeild, string cClassName = "")
                {
                    XmlElement oFrmElmt;
                    XmlElement oFrmElmt1;
                    XmlElement oElmt;
                    string sValidResponse = "";
                    string cProcessInfo = "";
                    try
                    {
                        base.NewFrm("EditImage");
                        if (cImgHtml.Contains("</img>"))
                        {
                            // if image tag is closed properly for XHTML
                            base.Instance.InnerXml = cImgHtml.Replace("&", "&amp;");
                        }
                        else
                        {
                            base.Instance.InnerXml = cImgHtml.Replace("\">", "\"/>").Replace("&", "&amp;");
                        }

                        if (string.IsNullOrEmpty(cClassName))
                        {
                            oElmt = (XmlElement)base.Instance.FirstChild;
                            cClassName = oElmt.GetAttribute("class");
                        }

                        base.submission("imageDetailsForm", "", "post", "form_check(this);passImgToForm('" + cTargetForm + "','" + cTargetFeild + "');return(false);");

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "Image Details", "", "Please enter image description");
                        oFrmElmt1 = base.addGroup(ref oFrmElmt, "", "", "");
                        base.addInput(ref oFrmElmt1, "cName", true, "Class", "readonly");
                        XmlElement argoBindParent = null;
                        base.addBind("cName", "img/@class", oBindParent: ref argoBindParent, "true()");

                        base.addInput(ref oFrmElmt1, "cPathName", true, "Path Name");
                        XmlElement argoBindParent1 = null;
                        base.addBind("cPathName", "img/@src", oBindParent: ref argoBindParent1, "true()");

                        base.addInput(ref oFrmElmt1, "nWidth", true, "Width");
                        XmlElement argoBindParent2 = null;
                        base.addBind("nWidth", "img/@width", oBindParent: ref argoBindParent2, "true()");

                        base.addInput(ref oFrmElmt1, "nHeight", true, "Height");
                        XmlElement argoBindParent3 = null;
                        base.addBind("nHeight", "img/@height", oBindParent: ref argoBindParent3, "true()");

                        base.addInput(ref oFrmElmt1, "cDesc", true, "Alt Description");
                        XmlElement argoBindParent4 = null;
                        base.addBind("cDesc", "img/@alt", oBindParent: ref argoBindParent4, "false()");

                        base.addDiv(ref oFrmElmt, "<div class=\"form-group pick-new-image\"><a href=\"?contentType=popup&amp;ewCmd=ImageLib&amp;targetField=" + cTargetFeild + "&amp;targetClass=" + cClassName + "\" class=\"btn btn-primary pull-right\" data-toggle=\"modal\"><i class=\"fa fa-picture-o\"> </i> Pick New Image</a></div>", "");
                        base.addSubmit(ref oFrmElmt, "", "Update Image", "ewSubmit", "ewSubmit");

                        if (!string.IsNullOrEmpty(cClassName))
                        {
                            oElmt = (XmlElement)base.Instance.FirstChild;
                            oElmt.SetAttribute("class", cClassName);
                        }

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {

                                if (Tools.Number.IsNumeric(sValidResponse))
                                {
                                    valid = true;
                                }
                                // MyBase.jsOnLoad()
                                else
                                {
                                    valid = false;
                                    //XmlNode argoNode = (XmlNode)this.moXformElmt;
                                    base.addNote(ref moXformElmt, Protean.xForm.noteTypes.Alert, sValidResponse);
                                    // this.moXformElmt = (XmlElement)argoNode;
                                }
                            }
                            else
                            {
                                valid = false;
                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmEditImage", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }


            }
        }
    }
}