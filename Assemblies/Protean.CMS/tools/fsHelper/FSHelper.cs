// ***********************************************************************
// $Library:     Protean.fsHelper
// $Revision:    4.0  
// $Date:        2006-09-22
// $Author:      Trevor Spink (trevor@eonic.co.uk) et al.
// &Website:     www.eonic.co.uk
// &Licence:     All Rights Reserved.
// $Copyright:   Copyright (c) 2002 - 2011 Eonicweb Ltd.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Xml;
using DelegateWrappers;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using static Protean.stdTools;

namespace Protean
{

    public partial class fsHelper
    {

        #region Declarations

        // Note from AG: Where possible - please create Shared methods and props on this, 
        // as some calling methods may not be able to provide the System.Web.HttpContext.Current object for goServer

        private static string mcModuleName = "Protean.fsHelper";
        private System.Collections.Specialized.NameValueCollection goConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");
        private HttpServerUtility goServer;

        public XmlDocument moPageXML;
        public string mcStartFolder = "";
        public string mcPopulateFilesNode = "";
        public string mcRoot = "";
        private static string msException;
        public Protean.Cms.Admin.JSONActions AdminJsonAPI;

        public bool ImpersonationMode = false;

        private string _thumbnailPath = "/~ptn";
        // Public cleanUploadedPaths As New List(Of String)()

        public string uploadReviewImageRootPath = "";
        public string cleanUploadedPaths;
        private static string[][] _libraryTypeExtensions = new string[][] { new string[] { }, new string[] { "png", "jpg", "gif", "jpeg", "bmp" }, new string[] { "doc", "docx", "xls", "xlsx", "pdf" }, new string[] { "avi", "flv", "swf", "ppt" } };

        private static string[] _defaultLibraryTypeContentSchemaNames = new string[] { "PlainText", "LibraryImage", "Document", "Video,FlashMovie" };


        public enum LibraryType
        {
            Undefined = 0,
            Image = 1,
            Documents = 2,
            Media = 3,
            Scripts = 4,
            Style = 5
        }

        #endregion
        #region Constructor

        public fsHelper() : this(HttpContext.Current)
        {
            if (!string.IsNullOrEmpty(goConfig["AdminAcct"]) & goConfig["AdminGroup"] != "AzureWebApp")
            {
                ImpersonationMode = true;
            }
        }

        public fsHelper(HttpContext Context)
        {
            goServer = Context.Server;
            if (!string.IsNullOrEmpty(goConfig["AdminAcct"]) & goConfig["AdminGroup"] != "AzureWebApp")
            {
                ImpersonationMode = true;
            }
        }

        public new void open(XmlDocument oPageXml)
        {
            // PerfMon.Log("fsHelper", "open")
            string cProcessInfo = "";
            try
            {
                moPageXML = oPageXml;
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "PersistVariables", ex, "", cProcessInfo, gbDebug);
            }
        }
        #endregion
        #region Initialisation
        public void initialiseVariables(LibraryType nLib)
        {
            // PerfMon.Log("fsHelper", "initialiseVariables")
            string cProcessInfo = "";
            string cStartFolder = "";

            try
            {

                mcRoot = GetFileLibraryPath(nLib);
                mcRoot = Strings.Replace(mcRoot, "/", @"\");

                if (mcRoot.StartsWith(@"\"))
                    mcRoot = mcRoot.Substring(1);
                if (!string.IsNullOrEmpty(mcRoot))
                {
                    if (mcRoot.StartsWith(".."))
                    {
                        cStartFolder = goServer.MapPath("/" + goConfig["ProjectPath"]) + mcRoot;
                    }
                    else
                    {
                        cStartFolder = goServer.MapPath("/" + goConfig["ProjectPath"] + mcRoot);
                    }
                }

                mcStartFolder = cStartFolder;
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "initialiseVariables", ex, "", cProcessInfo, gbDebug);
            }

        }

        public void initialiseVariables()
        {

            // PerfMon.Log("fsHelper", "initialiseVariables")
            string cProcessInfo = "";
            string cStartFolder = "";

            try
            {

                mcRoot = Strings.Replace(mcRoot, "/", @"\");
                if (mcRoot.StartsWith(@"\"))
                    mcRoot = mcRoot.Substring(1);

                if (!string.IsNullOrEmpty(mcRoot))
                {
                    if (mcRoot.StartsWith(".."))
                    {
                        cStartFolder = goServer.MapPath("/" + goConfig["ProjectPath"] + mcRoot);
                    }
                    else
                    {
                        cStartFolder = goServer.MapPath("/" + goConfig["ProjectPath"] + mcRoot);
                    }
                }

                mcStartFolder = cStartFolder;
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "initialiseVariables", ex, "", cProcessInfo, gbDebug);
            }

        }
        #endregion
        #region Public Methods
        public XmlElement getConfigNode(string cPath)
        {
            // PerfMon.Log("fsHelper", "getConfigNode")
            string cProcessInfo = "";
            var oConfigXml = new XmlDocument();  // Change XmlDataDocument to XmlDocument

            XmlElement oConfigNode;

            try
            {
                oConfigXml.Load(goServer.MapPath(goConfig["ProjectPath"] + "/Web.config"));
                // this now has a namespace to handle!!!
                oConfigNode = (XmlElement)oConfigXml.SelectSingleNode("/configuration/protean/" + cPath);

                return oConfigNode;
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "getConfigNode", ex, "", cProcessInfo, gbDebug);
                return null;
            }

        }

        public static string checkLeadingSlash(string filePath, bool removeSlash = false)
        {
            // PerfMon.Log("fsHelper", "checkLeadingSlash")
            try
            {

                filePath = filePath.Replace(@"\", "/");
                if (removeSlash)
                {
                    filePath = filePath.TrimStart('/');
                }
                else if (!filePath.StartsWith("/"))
                    filePath = "/" + filePath;

                return filePath;
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "checkLeadingSlash", ex, "", filePath, gbDebug);
                return filePath;
            }

        }

        public string checkCommonFilePath(string localFilePath)
        {
            // PerfMon.Log("fsHelper", "checkCommonFilePath")
            string cProcessInfo = "";
            try
            {
                localFilePath = checkLeadingSlash(localFilePath);

                if (File.Exists(goServer.MapPath(localFilePath)))
                {
                    return localFilePath;
                }
                else if (File.Exists(goServer.MapPath("/ptn" + localFilePath)))
                {
                    return "/ptn" + localFilePath;
                }
                else if (File.Exists(goServer.MapPath("/ewcommon" + localFilePath)))
                {
                    return "/ewcommon" + localFilePath;
                }
                else
                {
                    return "";
                }
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "checkCommonFilePath", ex, "", cProcessInfo, gbDebug);
                return "";
            }

        }
        public XmlElement setConfigNode(XmlElement oInstance)
        {
            // PerfMon.Log("fsHelper", "setConfigNode")
            string cProcessInfo = "";
            var oConfigXml = new XmlDocument();  // Change XmlDataDocument to XmlDocument
            XmlElement oConfigNode;

            try
            {

                oConfigXml.Load(goServer.MapPath(goConfig["ProjectPath"] + "/web.config"));
                oConfigNode = (XmlElement)oConfigXml.SelectSingleNode("configuration/protean/" + oInstance.Name);
                oConfigNode.InnerXml = oInstance.InnerXml;
                oConfigXml.Save(goServer.MapPath(goConfig["ProjectPath"] + "/web.config"));
                return oInstance;
            }
            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "setConfigNode", ex, "", cProcessInfo, gbDebug);
                return oInstance;
            }

        }

        public string getImageXhtml(string cPath)
        {
            // PerfMon.Log("fsHelper", "getImageXhtml")
            string cProcessInfo = "";
            try
            {
                cPath = Strings.Replace(cPath, @"\", "/");
                if (!cPath.StartsWith("/"))
                {
                    cPath = "/" + cPath;
                }
                string ImagePath = GetFileLibraryPath(LibraryType.Image) + cPath;

                if (ImagePath.EndsWith(".svg"))
                {
                    return "<img src=\"" + ImagePath + "\" alt=\"\"/> ";
                }
                else
                {
                    var oImg = new System.Drawing.Bitmap(goServer.MapPath("/" + mcRoot + cPath));
                    return "<img src=\"" + ImagePath + "\" height=\"" + oImg.Height + "\" width=\"" + oImg.Width + "\" alt=\"\"/> ";
                }
            }



            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "getImageXhtml", ex, "", cProcessInfo, gbDebug);
                return "";
            }

        }

        public XmlElement getDirectoryTreeXml(LibraryType nLib, string populateFilesNode = "", string pathPrefix = "")
        {
            // PerfMon.Log("fsHelper", "getDirectoryTreeXml")
            string tempStartFolder;
            XmlElement TreeXml;
            string[] aVirtualImageDirectories = Strings.Split(goConfig["VirtualImageDirectories"], ",");
            try
            {

                if (!string.IsNullOrEmpty(populateFilesNode))
                {
                    mcPopulateFilesNode = populateFilesNode;
                }

                if (string.IsNullOrEmpty(mcStartFolder))
                {
                    initialiseVariables(nLib);
                }
                // Create the root folder if it doesn't exist.
                tempStartFolder = mcStartFolder;
                mcStartFolder = goServer.MapPath("/");
                CreatePath(mcRoot);
                mcStartFolder = tempStartFolder;

                var nodeElem = XmlElement("folder", new DirectoryInfo(mcStartFolder).Name);
                object rootPath = @"\";
                if (!string.IsNullOrEmpty(pathPrefix))
                    rootPath = pathPrefix;
                nodeElem.SetAttribute("path", Conversions.ToString(rootPath));

                nodeElem.SetAttribute("startLevel", (pathPrefix.Split('\\').Length - 1).ToString());
                // PerfMon.Log("fsHelper", "getDirectoryTreeXml-AddElementsStart")
                TreeXml = AddElements(nodeElem, mcStartFolder, pathPrefix);
                // PerfMon.Log("fsHelper", "getDirectoryTreeXml-AddElementsEnd")
                if (nLib == LibraryType.Image)
                {

                    foreach (var VirtualImageDirectory in aVirtualImageDirectories)
                    {
                        if (!string.IsNullOrEmpty(VirtualImageDirectory))
                        {
                            string virtualPath = goServer.MapPath("/" + mcRoot + "/" + VirtualImageDirectory);
                            nodeElem = XmlElement("folder", new DirectoryInfo(virtualPath).Name);
                            nodeElem.SetAttribute("path", "/" + VirtualImageDirectory);
                            mcStartFolder = virtualPath.Substring(0, virtualPath.Length - VirtualImageDirectory.Length - 1);
                            AddElements(nodeElem, virtualPath);
                            TreeXml.AppendChild(nodeElem);
                        }
                    }

                }

                return TreeXml;
            }
            catch (Exception ex)
            {
                return XmlElement("error", ex.Message);
            }
        }

        public string CreateFolder(string cFolderName, string cFolderPath)
        {
            // PerfMon.Log("fsHelper", "CreateFolder-Start", cFolderName)
            // in order to make this work the root directory needs to have read permissions for everyone or at lease asp.net acct
            var dir = new DirectoryInfo(mcStartFolder + cFolderPath + @"\");
            try
            {
                if (dir.Exists)
                {
                    dir.CreateSubdirectory(cFolderName.Replace(" ", "-"));
                }
                else
                {
                    return "this root folder does not exist";
                }

                return "1";
            }
            // PerfMon.Log("fsHelper", "CreateFolder-End", cFolderName)
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        /// <summary>
        /// This Creates the path supplied without overwriting existing folders,
        /// It allow us to check a folder exists and create it if not.
        /// </summary>
        /// <param name="cFolderPath"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public string CreatePath(string cFolderPath)
        {
            // PerfMon.Log("fsHelper", "CreatePath", cFolderPath)
            // in order to make this work the root directory needs to have read permissions for everyone or at lease asp.net acct
            cFolderPath = Strings.Replace(cFolderPath, @"\", "/");
            string[] aFolderNames = cFolderPath.Split('/');
            int i;
            string tempFolder = "";
            try
            {

                Tools.Security.Impersonate oImp = null;

                if (ImpersonationMode)
                {
                    oImp = new Tools.Security.Impersonate();
                    oImp.ImpersonateValidUser(goConfig["AdminAcct"], goConfig["AdminDomain"], goConfig["AdminPassword"], cInGroup: goConfig["AdminGroup"]);
                }

                string startDir;
                if (mcRoot == "../")
                {
                    mcRoot = "";
                    startDir = goServer.MapPath("/");
                    var newDir = new DirectoryInfo(startDir);
                    startDir = newDir.Parent.FullName;
                }
                else
                {
                    startDir = goServer.MapPath("/" + mcRoot);
                }

                // check startfolder exists
                var rootDir = new DirectoryInfo(startDir);

                if (!rootDir.Exists)
                {
                    var baseDir = new DirectoryInfo(goServer.MapPath("/"));
                    rootDir = baseDir.CreateSubdirectory(mcRoot.Replace(" ", "-"));
                }

                if (string.IsNullOrEmpty(mcStartFolder))
                    mcStartFolder = startDir;
                string workingFolder = mcStartFolder;
                string startFolderName = mcStartFolder.Replace("/", @"\").Trim('\\');
                startFolderName = startFolderName.Substring(startFolderName.LastIndexOf(@"\") + 1);
                var startfld = new DirectoryInfo(mcStartFolder);
                if (!startfld.Exists)
                {
                    rootDir.CreateSubdirectory(startFolderName);
                }

                var loopTo = Information.UBound(aFolderNames);
                for (i = 0; i <= loopTo; i++)
                {
                    if (!string.IsNullOrEmpty(aFolderNames[i]))
                    {
                        var dir1 = new DirectoryInfo(workingFolder);
                        if (dir1.Exists)
                        {
                            tempFolder = workingFolder.TrimEnd('\\') + @"\" + aFolderNames[i];
                            var dir2 = new DirectoryInfo(tempFolder);
                            if (!dir2.Exists)
                            {
                                dir1.CreateSubdirectory(aFolderNames[i]);
                            }
                        }
                        workingFolder = workingFolder + @"\" + aFolderNames[i];
                    }
                }

                // PerfMon.Log("fsHelper", "CreatePath-End", cFolderPath)
                if (ImpersonationMode)
                {
                    oImp.UndoImpersonation();
                    oImp = null;
                }

                return "1";
            }

            // PerfMon.Log("fsHelper", "CreatePath - end")

            catch (Exception ex)
            {
                return ex.Message + " - " + tempFolder + "<br/>" + ex.StackTrace;
            }

        }

        public string getUniqueFilename(string cFullPath)
        {
            int nCount = 0;
            bool bFileExists = true;
            string cFileNameExt;
            string cFilePathFull = cFullPath;
            cFilePathFull = cFilePathFull.Replace("/", @"\");
            string cFilePath;
            string cFilePathNew = "";
            string cFileName = "";
            string cProcessInfo = "getUniqueFilename";

            try
            {
                // get file extension and path
                int nDotPos = Strings.InStrRev(cFilePathFull, ".");
                int nSlashPos = Strings.InStrRev(cFilePathFull, @"\");
                cFilePath = cFilePathFull.Substring(0, nDotPos - 1);
                cFileName = cFilePath.Substring(nSlashPos, cFilePath.Length - nSlashPos);
                cFilePath = cFilePathFull.Substring(0, nSlashPos - 1);
                cFileNameExt = cFilePathFull.Substring(nDotPos, cFilePathFull.Length - nDotPos);

                do
                {
                    if (nCount == 0)
                    {
                        // first time, try the plain file name 
                        cFilePathNew = cFilePath + @"\" + cFileName + "." + cFileNameExt;
                    }
                    else
                    {
                        cFilePathNew = cFilePath + @"\" + cFileName + "-" + nCount.ToString() + "." + cFileNameExt;
                    }
                    bFileExists = File.Exists(cFilePathNew);
                    nCount += 1;
                }
                while (File.Exists(cFilePathNew) != false);

                // remove the qualifying path and tidy up for returning
                // this line returns the filename only which we don't need
                // cFilePathNew = cFilePathNew.Replace(cFilePath & "\", "")
                cFilePathNew = cFilePathNew.Replace(@"\", "/");
                return cFilePathNew;
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "getUniqueFilename", ex, "", cProcessInfo, gbDebug);
                return null;
            }

        }



        public string DeleteFolder(string cFolderName, string cFolderPath)
        {
            // PerfMon.Log("fsHelper", "DeleteFolder")
            // in order to make this work the root directory needs to have read permissions for everyone or at lease asp.net acct
            do
            {
                try
                {

                    Tools.Security.Impersonate oImp = null;
                    if (ImpersonationMode)
                    {
                        oImp = new Tools.Security.Impersonate();
                        if (oImp.ImpersonateValidUser(goConfig["AdminAcct"], goConfig["AdminDomain"], goConfig["AdminPassword"], cInGroup: goConfig["AdminGroup"]) == false)
                        {
                            return "Server admin permissions are not configured";
                            break;
                        }
                    }

                    var dir = new DirectoryInfo(mcStartFolder + cFolderPath + @"\" + cFolderName);
                    if (dir.Exists)
                    {
                        // FIX disable AppDomain restart when deleting subdirectory
                        // This code will turn off monitoring from the root website directory.
                        // Monitoring of Bin, App_Themes and other folders will still be operational, so updated DLLs will still auto deploy.
                        var p = typeof(HttpRuntime).GetProperty("FileChangesMonitor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                        var o = p.GetValue(null, null);
                        var f = o.GetType().GetField("_dirMonSubdirs", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.IgnoreCase);
                        var monitor = f.GetValue(o);
                        var m = monitor.GetType().GetMethod("StopMonitoring", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                        m.Invoke(monitor, new object[] { });

                        dir.Delete(true);
                    }

                    else
                    {
                        return "this folder does not exist - " + cFolderPath + cFolderName;
                    }

                    if (ImpersonationMode)
                    {
                        oImp.UndoImpersonation();
                        oImp = null;
                    }

                    return "1";
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
            while (false);

        }


        public string DeleteFolderContents(string cFolderName, string cFolderPath)
        {
            // PerfMon.Log("fsHelper", "DeleteFolder")
            // in order to make this work the root directory needs to have read permissions for everyone or at lease asp.net acct
            do
            {
                try
                {

                    Tools.Security.Impersonate oImp = null;
                    if (ImpersonationMode)
                    {
                        oImp = new Tools.Security.Impersonate();
                        if (oImp.ImpersonateValidUser(goConfig["AdminAcct"], goConfig["AdminDomain"], goConfig["AdminPassword"], cInGroup: goConfig["AdminGroup"]) == false)
                        {
                            return "Server admin permissions are not configured";
                            break;
                        }
                    }

                    string FolderName = mcStartFolder + cFolderPath + @"\" + cFolderName;
                    var dir = new DirectoryInfo(FolderName);

                    if (dir.Exists)
                    {
                        dir.Attributes = FileAttributes.Normal;
                        foreach (var f in dir.GetFiles())
                        {
                            f.Attributes = FileAttributes.Normal;
                            f.Delete();
                        }
                        foreach (var d in dir.GetDirectories())
                            d.Delete(true);
                    }
                    else
                    {
                        return "this folder does not exist";
                    }


                    if (ImpersonationMode)
                    {
                        oImp.UndoImpersonation();
                        oImp = null;
                    }
                    return "1";
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
            while (false);

        }


        public int VirtualFileExists(string cVirtualPath)
        {
            try
            {
                string cVP = goServer.MapPath(cVirtualPath);
                if (File.Exists(cVP))
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

        public int VirtualFileExistsAndRecent(string cVirtualPath, long hours)
        {
            try
            {
                string cVP = mcStartFolder + cVirtualPath.Replace("/", @"\");
                if (File.Exists(cVP) & File.GetLastWriteTime(cVP) > DateAndTime.DateAdd(DateInterval.Hour, hours * -1, DateTime.Now))
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

        public int CompareDateIsNewer(string cOriginalPath, string cCheckNewerPath)
        {
            try
            {
                string cOP = goServer.MapPath(cOriginalPath);
                string cCNP = goServer.MapPath(cCheckNewerPath);
                if (File.GetCreationTime(cCNP) > File.GetCreationTime(cOP))
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

        public string DeleteFile(string cFolderPath, string cFileName)
        {
            // PerfMon.Log("fsHelper", "DeleteFile")
            do
            {
                try
                {

                    Tools.Security.Impersonate oImp = null;
                    if (ImpersonationMode)
                    {
                        oImp = new Tools.Security.Impersonate();
                        if (oImp.ImpersonateValidUser(goConfig["AdminAcct"], goConfig["AdminDomain"], goConfig["AdminPassword"], cInGroup: goConfig["AdminGroup"]) == false)
                        {
                            return "Server admin permissions are not configured";
                            break;
                        }
                    }

                    string cFullFileName = mcStartFolder + cFolderPath + @"\" + cFileName;
                    if (File.Exists(cFullFileName))
                    {
                        var oFileInfo = new FileInfo(cFullFileName);
                        oFileInfo.IsReadOnly = false;
                        File.Delete(cFullFileName);
                    }
                    else
                    {
                        return "this file does not exist";
                    }

                    if (ImpersonationMode)
                    {
                        oImp.UndoImpersonation();
                        oImp = null;
                    }

                    return "1";
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
            while (false);
        }

        public string DeleteFile(string cFullFilePath)
        {
            // PerfMon.Log("fsHelper", "DeleteFile")
            do
            {
                try
                {
                    Tools.Security.Impersonate oImp = null;
                    if (ImpersonationMode)
                    {
                        oImp = new Tools.Security.Impersonate();
                        if (oImp.ImpersonateValidUser(goConfig["AdminAcct"], goConfig["AdminDomain"], goConfig["AdminPassword"], cInGroup: goConfig["AdminGroup"]) == false)
                        {
                            return "Server admin permissions are not configured";
                            break;
                        }
                    }

                    if (File.Exists(cFullFilePath))
                    {
                        var oFileInfo = new FileInfo(cFullFilePath);
                        oFileInfo.IsReadOnly = false;
                        File.Delete(cFullFilePath);
                    }
                    // scan for thumbnails to delete. subfolders starting with ~ look for files that end in the filename regardless of extension.
                    else
                    {
                        return "this file does not exist";
                    }

                    if (ImpersonationMode)
                    {
                        oImp.UndoImpersonation();
                        oImp = null;
                    }

                    return "1";
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
            while (false);
        }

        public string SaveFile(ref string FileName, string cFolderPath, byte[] FileData)
        {
            // PerfMon.Log("fsHelper", "SaveFile")
            try
            {
                // here we will fix any unsafe web charactors in the name
                FileName = Strings.Replace(FileName, " ", "-");

                cFolderPath = Strings.Replace(cFolderPath, "~", "");

                var dir = new DirectoryInfo(mcStartFolder + cFolderPath + @"\");
                if (!dir.Exists)
                {
                    CreatePath(cFolderPath);
                }
                // this saves the entire file to memory and kills the server occasionaly.
                // File.WriteAllBytes(mcStartFolder & cFolderPath & "\" & FileName, FileData)

                var myFile = File.Create(mcStartFolder + cFolderPath + @"\" + FileName);
                try
                {
                    myFile.Write(FileData, 0, FileData.Length);
                    myFile.Close();
                    myFile = null;
                    dir = null;
                    return cFolderPath + "/" + FileName;
                }

                catch (Exception ex)
                {
                    // We assume if failing to write then the file is being written allready by another process therefore we just return the filename as usual.
                    return cFolderPath + "/" + FileName;
                }
            }

            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }
        }

        public string SaveFile(ref HttpPostedFile postedFile, string cFolderPath)
        {
            // PerfMon.Log("fsHelper", "SaveFile")
            try
            {
                string filename = Strings.Right(postedFile.FileName, postedFile.FileName.Length - postedFile.FileName.LastIndexOf(@"\") - 1);

                // here we will fix any unsafe web charactors in the name
                filename = Strings.Replace(filename, " ", "-");

                var dir = new DirectoryInfo(mcStartFolder + cFolderPath + @"\");
                if (dir.Exists)
                {
                    postedFile.SaveAs(mcStartFolder + cFolderPath + @"\" + filename);
                    return postedFile.FileName;
                }
                else
                {
                    return "this root folder does not exist:" + mcStartFolder;
                }
            }

            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string SaveFile(string httpURL, string cFolderPath)
        {
            // PerfMon.Log("fsHelper", "SaveFile")
            WebResponse response = null;
            Stream remoteStream;
            StreamReader readStream;
            WebRequest request;
            System.Drawing.Image img = null;
            try
            {
                httpURL = httpURL.Replace(@"\", "/");
                string filename = Strings.Right(httpURL, httpURL.Length - httpURL.LastIndexOf("/") - 1);
                if (filename.IndexOf("?") > -1)
                {
                    filename = Strings.Right(filename, filename.Length - filename.LastIndexOf("=") - 1);
                }
                // here we will fix any unsafe web charactors in the name
                filename = Strings.Replace(filename, " ", "-");
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                if (File.Exists(mcStartFolder + cFolderPath + @"\" + filename))
                {
                    return Strings.Replace(Strings.Replace(cFolderPath, @"..\", "/"), @"\", "/") + "/" + filename;
                }
                else
                {
                    request = WebRequest.Create(httpURL);
                    try
                    {
                        response = request.GetResponse();
                    }
                    catch (WebException ex)
                    {
                        HttpWebResponse errResp = (HttpWebResponse)ex.Response;
                        if (ex.Status == WebExceptionStatus.ProtocolError)
                        {
                            return errResp.StatusCode.ToString();
                        }
                    }

                    if (response != null)
                    {
                        remoteStream = response.GetResponseStream();
                        try
                        {
                            img = System.Drawing.Image.FromStream(remoteStream);
                        }
                        catch (Exception ex2)
                        {
                            string test = "Error downloading " + httpURL + " - " + ex2.Message;

                        }


                        CreatePath(cFolderPath + @"\");

                        var dir = new DirectoryInfo(mcStartFolder + cFolderPath + @"\");
                        if (dir.Exists)
                        {
                            switch (Strings.Right(httpURL, httpURL.Length - httpURL.LastIndexOf(".") - 1) ?? "")
                            {
                                case "gif":
                                    {
                                        img.Save(mcStartFolder + cFolderPath + @"\" + filename, System.Drawing.Imaging.ImageFormat.Gif);
                                        return Strings.Replace(Strings.Replace(cFolderPath, @"..\", "/"), @"\", "/") + "/" + filename;
                                    }
                                case "jpg":
                                    {
                                        img.Save(mcStartFolder + cFolderPath + @"\" + filename, System.Drawing.Imaging.ImageFormat.Jpeg);
                                        return Strings.Replace(Strings.Replace(cFolderPath, @"..\", "/"), @"\", "/") + "/" + filename;
                                    }
                                case "png":
                                    {
                                        img.Save(mcStartFolder + cFolderPath + @"\" + filename, System.Drawing.Imaging.ImageFormat.Png);
                                        return Strings.Replace(Strings.Replace(cFolderPath, @"..\", "/"), @"\", "/") + "/" + filename;
                                    }

                                default:
                                    {
                                        return "filetype not handled:" + filename;
                                    }
                            }
                        }

                        else
                        {
                            return "this root folder does not exist:" + mcStartFolder;
                        }
                        response.Close();
                        remoteStream.Close();
                        img.Dispose();
                    }



                }
            }


            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                response = null;
                remoteStream = null;
                readStream = null;
                img = null;
            }

            return default;
        }


        public bool CopyFile(string FileName, string cFolderSource, string cFolderDestination, bool bOverwrite = false)
        {
            // PerfMon.Log("fsHelper", "CopyFile")
            try
            {

                // here we will fix any unsafe web charactors in the name
                FileName = Strings.Replace(FileName, " ", "-");

                var dir = new DirectoryInfo(goServer.MapPath("/") + cFolderSource);
                var DestDir = new DirectoryInfo(goServer.MapPath("/") + cFolderDestination);

                if (dir.Exists & DestDir.Exists)
                {
                    File.Copy(dir.FullName + "/" + FileName, DestDir.FullName + @"\" + FileName, bOverwrite);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            catch (Exception ex)
            {
                return Conversions.ToBoolean(ex.Message);
            }
        }

        public bool MoveFile(string FileName, string cFolderSource, string cFolderDestination, bool bOverwrite = false)
        {
            // PerfMon.Log("fsHelper", "MoveFile")

            string cProcessInfo = "Moving " + FileName + " to " + cFolderDestination;
            try
            {
                var dir = new DirectoryInfo(goServer.MapPath("/" + mcRoot) + cFolderSource.Replace("/", @"\"));
                var DestDir = new DirectoryInfo(goServer.MapPath("/" + mcRoot) + cFolderDestination.Replace("/", @"\"));

                if (dir.Exists & DestDir.Exists)
                {
                    if (!File.Exists(DestDir.FullName + @"\" + FileName.Replace(" ", "-")))
                    {
                        File.Copy(dir.FullName + @"\" + FileName, DestDir.FullName + @"\" + FileName.Replace(" ", "-"), bOverwrite);
                    }

                    File.SetAttributes(dir.FullName + @"\" + FileName, FileAttributes.Normal);

                    Thread.Sleep(50);

                    File.Delete(dir.FullName + @"\" + FileName);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            catch (Exception ex)
            {
                // Return ex.Message
                returnException(ref msException, mcModuleName, "MoveFile", ex, "", cProcessInfo, gbDebug);
                return false;

            }
        }

        public FileStream GetFileStream(string FilePath)
        {
            // PerfMon.Log("GetFileStream", "SaveFile")
            do
            {
                try
                {

                    Tools.Security.Impersonate oImp = null;
                    if (ImpersonationMode)
                    {
                        oImp = new Tools.Security.Impersonate();
                        if (oImp.ImpersonateValidUser(goConfig["AdminAcct"], goConfig["AdminDomain"], goConfig["AdminPassword"], cInGroup: goConfig["AdminGroup"]) == false)
                        {
                            return null;
                            break;
                        }
                    }

                    var oFileStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
                    return oFileStream;

                    if (ImpersonationMode)
                    {
                        oImp.UndoImpersonation();
                        oImp = null;
                    }
                }

                catch (Exception ex)
                {
                    return null;
                }
            }
            while (false);

        }

        public List<string> EnumerateFolders(LibraryType folderLibrary)
        {

            try
            {
                initialiseVariables(folderLibrary);
                var dir = new DirectoryInfo(mcRoot);
                return EnumerateFolders(dir);
            }
            catch (Exception ex)
            {
                return new List<string>();
            }

        }

        public List<string> EnumerateFolders(string path)
        {

            try
            {
                var dir = new DirectoryInfo(goServer.MapPath(path));
                return EnumerateFolders(dir);
            }
            catch (Exception ex)
            {
                return new List<string>();
            }

        }

        public string FindFilePathInCommonFolders(string pathToCheck, string[] foldersToCheck)
        {
            return FindFilePathInCommonFolders(pathToCheck, new List<string>(foldersToCheck));
        }

        /// <summary>
        /// Given a file path and a list of other folders to check, this returns the first available file that actually exists
        /// Precedence is inferred in the order of the list (path to check first, then folders in list + path to check)
        /// </summary>
        /// <param name="pathToCheck"></param>
        /// <param name="foldersToCheck"></param>
        /// <returns>Returns the first existing path found, or an empty string if not found.</returns>
        /// <remarks></remarks>
        public string FindFilePathInCommonFolders(string pathToCheck, List<string> foldersToCheck)
        {

            try
            {
                var pathsToCheck = new List<string>();
                pathToCheck = "/" + pathToCheck.TrimStart(@"/\".ToCharArray());
                pathsToCheck.Add(pathToCheck);
                pathsToCheck.AddRange(foldersToCheck.ConvertAll((folderToCheck) => folderToCheck.TrimEnd(@"/\".ToCharArray()) + pathToCheck));
                return pathsToCheck.Find((path) => File.Exists(goServer.MapPath(path)));
            }
            catch (Exception ex)
            {
                return "";
            }

        }

        public string OptimiseImages(string path, [Optional, DefaultParameterValue(0L)] ref long nFileCount, [Optional, DefaultParameterValue(0L)] ref long nSavings, bool lossless = true, string tinyAPIKey = "", string FolderPrefix = "")
        {
            try
            {
                var thisDir = new DirectoryInfo(goServer.MapPath(path));
                long newSavings = 0L;

                long nLengthBefore = 0L;

                path = HttpUtility.UrlDecode(path);

                foreach (var ofolder in thisDir.GetDirectories())
                    OptimiseImages(path + "/" + ofolder.Name, ref nFileCount, ref nSavings, lossless, tinyAPIKey);

                if (thisDir.Name.StartsWith(FolderPrefix) | string.IsNullOrEmpty(FolderPrefix))
                {
                    var LogFile = new FileInfo(goServer.MapPath(path) + "/optimiselog.txt");
                    if (LogFile.Exists == false | DateAndTime.DateDiff(DateInterval.Hour, LogFile.LastWriteTimeUtc, DateTime.Now) > 24L)
                    {
                        object FileCountBefore = nFileCount;

                        foreach (var ofile in thisDir.GetFiles())
                        {
                            var oImgTool = new Tools.Image("");
                            oImgTool.TinifyKey = tinyAPIKey;
                            newSavings = newSavings + oImgTool.CompressImage(ofile, lossless);
                            nFileCount = nFileCount + 1L;
                        }

                        long FilesProcessedCount = Conversions.ToLong(Operators.SubtractObject(nFileCount, FileCountBefore));
                        string LogText = "Last Optimised:" + DateTime.Now.ToLongDateString() + " Savings:" + newSavings + " FileCount:" + FilesProcessedCount + Constants.vbCrLf;
                        if (LogFile.Exists)
                        {
                            using (var fs = File.AppendText(goServer.MapPath(path) + "/optimiselog.txt"))
                            {
                                fs.WriteLine(LogText);
                                fs.Close();
                            }
                        }
                        else
                        {
                            using (var fs = File.Create(goServer.MapPath(path) + "/optimiselog.txt"))
                            {
                                byte[] info = new System.Text.UTF8Encoding(true).GetBytes(LogText + Constants.vbCrLf);
                                fs.Write(info, 0, info.Length);
                                fs.Close();
                            }
                        }

                    }
                }
                nSavings = nSavings + newSavings;
                return nFileCount + " Files Updated " + nSavings / 1024d + " Kb have been saved";
            }

            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        public bool ContainsSpecialChars(string sfilename)
        {
            return sfilename.IndexOfAny("[~`!@#$%^&*()-+=|{}':;,<>/?]".ToCharArray()) != -1;
        }
        public string UploadRequest(HttpContext context, string UploadDirPath = "")
        {
            try
            {


                context.Response.AddHeader("Pragma", "no-cache");
                context.Response.AddHeader("Cache-Control", "Private, no - cache");

                if (UploadDirPath != null & !string.IsNullOrEmpty(UploadDirPath))
                {

                    mcStartFolder = mcStartFolder + UploadDirPath.Replace("/", @"\");
                    if (!Directory.Exists(mcStartFolder))
                    {
                        Directory.CreateDirectory(mcStartFolder);
                    }
                    mcRoot = context.Server.MapPath("/");
                    HandleUploads(context);
                    return cleanUploadedPaths;
                }
                else
                {
                    mcStartFolder = context.Server.MapPath(context.Request["storageRoot"].Replace(@"\", "/").Replace("\"", ""));
                    mcRoot = context.Server.MapPath("/");
                    HandleUploads(context);
                    if (context.Request["IsmultiImage"] == "yes")
                    {
                        return cleanUploadedPaths;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }

            catch (Exception ex)
            {
                // catch errorr
                return ex.Message;
            }
        }

        private void HandleUploads(HttpContext context)
        {

            // Check user has permissions


            switch (context.Request.HttpMethod ?? "")
            {
                case "POST":
                case "PUT":
                    {
                        UploadFile(context);
                        break;
                    }

                case "OPTIONS":
                    {
                        ReturnOptions(context);
                        break;
                    }

                default:
                    {

                        context.Response.ClearHeaders();
                        context.Response.StatusCode = 405;
                        break;
                    }
            }
        }

        private void ReturnOptions(HttpContext context)
        {
            context.Response.AddHeader("Allow", "POST, PUT, OPTIONS");
            context.Response.StatusCode = 200;
        }

        // Upload file to the server
        private void UploadFile(HttpContext context)
        {
            object statuses = new List<FilesStatus>();
            System.Collections.Specialized.NameValueCollection headers = context.Request.Headers;
            string isOverwrite = context.Request["isOverwrite"];
            string cOldFile = context.Request["oldfile"];

            for (int i = 0, loopTo = context.Request.Files.Count - 1; i <= loopTo; i++)
            {
                System.Web.HttpPostedFile file = context.Request.Files[i];
                string cfileName = CleanfileName(Conversions.ToString(file.FileName));
                string scleanFileName = cfileName;
                string isExists = "true";
                string NewFileName = CleanFileExists(cfileName, context);
                if ((NewFileName ?? "") == (cfileName ?? ""))
                {
                    isExists = "false";
                }
                else
                {
                    cfileName = NewFileName;
                }
                if (Conversions.ToBoolean(isExists) && string.IsNullOrEmpty(isOverwrite))
                {
                    context.Session["ExistsFileName"] = cfileName + "," + scleanFileName + "," + isExists;
                }
                else
                {
                    // check if overwrite true then delete old one and upload new one
                    if (isOverwrite == "true" && !string.IsNullOrEmpty(cOldFile))
                    {
                        string cOldFullFileName = goServer.MapPath(context.Request["storageRoot"].Replace(@"\", "/").Replace("\"", "") + "/" + cOldFile.Replace("\"", ""));
                        if (File.Exists(cOldFullFileName))
                        {
                            var oFileInfo = new FileInfo(cOldFullFileName);
                            oFileInfo.IsReadOnly = false;
                            File.Delete(cOldFullFileName);
                        }
                    }
                    if (string.IsNullOrEmpty(Convert.ToString(headers["X-File-Name"])))
                    {
                        UploadWholeFile(context, (List<FilesStatus>)statuses);
                    }
                    else
                    {
                        UploadPartialFile(Conversions.ToString(headers["X-File-Name"]), context, (List<FilesStatus>)statuses);
                    }

                    WriteJsonIframeSafe(context, (List<FilesStatus>)statuses);
                }
            }
        }

        // Upload partial file
        private void UploadPartialFile(string fileName, HttpContext context, List<FilesStatus> statuses)
        {
            if (context.Request.Files.Count != 1)
            {
                throw new HttpRequestValidationException("Attempt To upload chunked file containing more than one fragment per request");
            }
            Stream inputStream = context.Request.Files[0].InputStream;
            string fullName = mcStartFolder + Path.GetFileName(fileName);

            using (FileStream fs = new FileStream(Conversions.ToString(fullName), FileMode.Append, FileAccess.Write))
            {
                byte[] buffer = new byte[1024];

                var l = inputStream.Read(buffer, 0, 1024);
                while (Operators.ConditionalCompareObjectGreater(l, 0, false))
                {
                    fs.Write(buffer, 0, l);
                    l = inputStream.Read(buffer, 0, 1024);
                }
                fs.Flush();
                fs.Close();
            }
            statuses.Add(new FilesStatus(new FileInfo(Conversions.ToString(fullName))));
        }

        public string CleanfileName(string cFilename)
        {

            try
            {
                string combineFile;
                string cCleanfileName = Regex.Replace(cFilename, @"\s+", "-");
                cCleanfileName = Regex.Replace(cCleanfileName, @"(\s+|\$|\,|\'|\£|\:|\*|&|\?|\/)", "");
                cCleanfileName = Regex.Replace(cCleanfileName, "-{2,}", "-", RegexOptions.None);
                bool FileContainsnonAlphaChar = ContainsSpecialChars(cCleanfileName);
                if (FileContainsnonAlphaChar)
                {
                    cCleanfileName = Regex.Replace(cCleanfileName, "[~`!@#$%^&*()-+=|{}':;,<>/?]", "");
                    // combineFile = cFilename + "," + cCleanfileName
                    return cCleanfileName;
                }
                else
                {
                    return cCleanfileName;
                }
            }

            catch (Exception ex)
            {
                // RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ReplaceRegularExpression", ex, ""))
                return ex.Message;
            }
        }

        public string CleanFileExists(string cFilename, HttpContext context)
        {

            bool fileExists = false;
            string cFilePath = context.Request["storageRoot"].Replace(@"\", "/").Replace("\"", "");
            if (!cFilePath.EndsWith(@"\"))
                cFilePath = cFilePath + @"\";
            try
            {
                if (cFilePath != null)
                {
                    fileExists = File.Exists(goServer.MapPath(cFilePath + cFilename));
                }
                if (fileExists)
                {
                    for (int i = 0; i <= 1000; i++)
                    {
                        // save Regex to replace filename-{digit}.jpg with filename-{newdigit}.jpg 
                        string cExtension = Path.GetExtension(cFilename);
                        if (Regex.IsMatch(cFilename, @"(-\d+).([a-z]{3,4})$"))    // this regex checks hyphen and number present only before dot.
                        {
                            cFilename = Regex.Replace(cFilename, @"(-\d+).([a-z]{3,4})$", "-" + i) + cExtension;
                        }
                        else
                        {
                            cFilename = cFilename.Replace(".", "-" + i + ".");
                        }

                        if (!File.Exists(goServer.MapPath(cFilePath + cFilename)))
                        {
                            break;
                        }
                    }
                    return cFilename;
                }
                else
                {
                    return cFilename;
                }
            }

            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        // Upload entire file
        private void UploadWholeFile(HttpContext context, List<FilesStatus> statuses)
        {
            for (int i = 0, loopTo = context.Request.Files.Count - 1; i <= loopTo; i++)
            {
                System.Web.HttpPostedFile file = context.Request.Files[i];

                try
                {
                    if (!mcStartFolder.EndsWith(@"\"))
                        mcStartFolder = mcStartFolder + @"\";
                    string cfileName = CleanfileName(Conversions.ToString(file.FileName));
                    context.Session["ExistsFileName"] = cfileName;
                    file.SaveAs(mcStartFolder + cfileName);

                    if (Strings.LCase(mcStartFolder + cfileName).EndsWith(".jpg") | Strings.LCase(mcStartFolder + cfileName).EndsWith(".jpeg") | Strings.LCase(mcStartFolder + cfileName).EndsWith(".png"))
                    {
                        var eImg = new Tools.Image(mcStartFolder + cfileName);
                        System.Collections.Specialized.NameValueCollection moWebCfg = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");
                        eImg.UploadProcessing(Conversions.ToString(moWebCfg["WatermarkText"]), Conversions.ToString(Operators.ConcatenateObject(mcRoot, moWebCfg["WatermarkImage"])));
                    }

                    string fullName = Path.GetFileName(Conversions.ToString(file.FileName)).Replace("'", "");
                    statuses.Add(new FilesStatus(fullName.Replace(" ", "-"), Conversions.ToInteger(file.ContentLength)));
                    context.Server.MapPath("/");
                    // We will add one node in ReviewFeedback.xml form and use it instead of config key = context.Request.Form("reviewimagepath")
                    if (!string.IsNullOrEmpty(context.Request.Form["cImageBasePath"]) && !string.IsNullOrEmpty(context.Request.Form["cImageBasePath"]))
                    {
                        cleanUploadedPaths = "/" + mcStartFolder.Replace(context.Request.Form["cImageBasePath"], "").Replace(@"\", "/") + cfileName;
                    }
                    else
                    {
                        cleanUploadedPaths = "/" + mcStartFolder.Replace(context.Server.MapPath("/"), "").Replace(@"\", "/") + cfileName;
                    }
                }

                catch (Exception ex)
                {
                    statuses.Add(new FilesStatus("failed", 0));
                }

            }
        }

        private readonly System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();

        private void WriteJsonIframeSafe(HttpContext context, List<FilesStatus> statuses)
        {
            context.Response.AddHeader("Vary", "Accept");
            try
            {
                if (context.Request["HTTP_ACCEPT"].Contains("application/json"))
                {
                    context.Response.ContentType = "application/json";
                }
                else
                {
                    context.Response.ContentType = "text/plain";
                }
            }
            catch
            {
                context.Response.ContentType = "text/plain";
            }

            object jsonObj = js.Serialize(statuses.ToArray());
            context.Response.Write(jsonObj);

        }

        private bool GivenFilename(HttpContext context)
        {
            return !string.IsNullOrEmpty(context.Request["f"]);
        }
        #endregion
        #region Private Methods
        private XmlElement AddElements(XmlElement startNode, string Folder, string pathPrefix = "")
        {
            // 'PerfMon.Log("fsHelper", "AddElements", Folder)
            try
            {
                var dir = new DirectoryInfo(Folder);
                DirectoryInfo[] subDirs = dir.GetDirectories();
                FileInfo[] files = dir.GetFiles();
                string sVirtualPath;
                FileInfo[] tnfiles = null;


                if (mcStartFolder.Contains(".."))
                {
                    // we have a virtual path and we need to be a bit more cleverer
                    sVirtualPath = mcStartFolder.Substring(mcStartFolder.LastIndexOf(".."));
                    string[] aPath = sVirtualPath.Split('\\');
                    sVirtualPath = Folder.Substring((int)(long)(Folder.LastIndexOf(aPath[1]) + aPath[1].Length));
                }
                else
                {
                    sVirtualPath = Strings.Replace(Folder, mcStartFolder, "");
                }

                mcPopulateFilesNode = mcPopulateFilesNode.Replace("/", @"\");

                if ((mcPopulateFilesNode ?? "") == (sVirtualPath ?? "") | mcPopulateFilesNode == @"\" & string.IsNullOrEmpty(sVirtualPath))
                {
                    var tndir = new DirectoryInfo(Folder + _thumbnailPath);
                    if (tndir.Exists)
                    {
                        tnfiles = tndir.GetFiles();
                    }
                    startNode.SetAttribute("active", "true");
                    short fileCount = 1;
                    foreach (var fi in files)
                    {
                        if (!(Strings.Left(fi.Name, 5) == "Icon_") & !(fi.Name.ToLower() == "thumbs.db") & !(fi.Name.ToLower() == ".ds_store"))
                        {
                            string cExt = Strings.LCase(fi.Extension);
                            var fileElem = XmlElement("file", fi.Name);
                            fileElem.Attributes.Append(XmlAttribute("Extension", cExt));
                            fileElem.Attributes.Append(XmlAttribute("length", (fi.Length / 1000d).ToString()));
                            // PerfMon.Log("fsHelper", "AddElements-Addfile", fi.Name)
                            switch (cExt ?? "")
                            {

                                case ".jpg":
                                case ".gif":
                                case ".jpeg":
                                case ".png":
                                case ".bmp":
                                    {
                                        try
                                        {
                                            // this is slow so we only want to do it for the first 100 files
                                            if (fileCount < 100)
                                            {
                                                var oWebFile = new fsHelper.WebFile(fi.FullName, sVirtualPath + "/" + fi.Name, true);
                                                fileElem.Attributes.Append(this.XmlAttribute("height", oWebFile.ExtendedProperties.Height.ToString()));
                                                fileElem.Attributes.Append(this.XmlAttribute("width", oWebFile.ExtendedProperties.Width.ToString()));
                                            }

                                            // check and return the thumbnail path
                                            bool createTn = true;
                                            if (tnfiles != null)
                                            {
                                                foreach (FileInfo tnfi in tnfiles)
                                                {
                                                    if ((fi.Name ?? "") == (tnfi.Name ?? ""))
                                                    {
                                                        createTn = false;
                                                    }
                                                }
                                            }

                                            if (createTn)
                                            {
                                                var oImage = new Tools.Image(mcStartFolder + sVirtualPath + @"\" + fi.Name);
                                                fileElem.Attributes.Append(XmlAttribute("thumbnail", oImage.CreateThumbnail(sVirtualPath, _thumbnailPath)));
                                                oImage = null;
                                            }
                                            else
                                            {
                                                fileElem.Attributes.Append(XmlAttribute("thumbnail", _thumbnailPath + "/" + fi.Name));
                                            }
                                        }

                                        catch (Exception ex)
                                        {
                                            return XmlElement("error", ex.Message);
                                        }

                                        break;
                                    }

                                default:
                                    {

                                        string cIcon = "Icon_";
                                        switch (Strings.LCase(fi.Extension) ?? "")
                                        {
                                            case ".doc":
                                            case ".rtf":
                                                {
                                                    cIcon += "WRD";
                                                    break;
                                                }
                                            case ".xls":
                                            case ".csv":
                                                {
                                                    cIcon += "XL";
                                                    break;
                                                }
                                            case ".pdf":
                                                {
                                                    cIcon += "PDF";
                                                    break;
                                                }
                                            // Case ".bmp"
                                            // cIcon &= "IMG"
                                            case ".mpg":
                                            case ".avi":
                                                {
                                                    cIcon += "MV";
                                                    break;
                                                }
                                            case ".wmv":
                                                {
                                                    cIcon += "WMV";
                                                    break;
                                                }

                                            default:
                                                {
                                                    cIcon += "OT";
                                                    break;
                                                }
                                        }
                                        cIcon = cIcon + ".gif";

                                        // If File.Exists(mcStartFolder & "\" & cIcon) Then
                                        // fileElem.SetAttribute("icon", cIcon)
                                        // End If

                                        if (File.Exists(goServer.MapPath(goConfig["ProjectPath"] + "/ewcommon/images/icons/" + cIcon)))
                                        {
                                            fileElem.SetAttribute("icon", cIcon);
                                        }

                                        break;
                                    }
                            }

                            fileElem.SetAttribute("root", mcRoot);
                            startNode.AppendChild(fileElem);
                        }
                        fileCount = (short)(fileCount + 1);
                    }
                }
                foreach (var sd in subDirs)
                {
                    if (sd.Name != "_vti_cnf" & !sd.Name.StartsWith("~"))
                    {
                        var folderElem = XmlElement("folder", Strings.Replace(sd.Name, @"\", "/"));
                        string sPath;

                        if (mcStartFolder.Contains(".."))
                        {
                            // we have a virtual path and we need to be a bit more cleverer
                            sPath = mcStartFolder.Substring(mcStartFolder.LastIndexOf(".."));
                            string[] aPath = sPath.Split('\\');
                            sPath = sd.FullName.Substring((int)(long)(sd.FullName.LastIndexOf(aPath[1]) + aPath[1].Length));
                        }
                        else
                        {
                            sPath = Strings.Replace(sd.FullName, mcStartFolder, "");
                        }

                        folderElem.Attributes.Append(XmlAttribute("path", pathPrefix + sPath));
                        // folderElem.Attributes.Append(XmlAttribute("Hidden",(If(sd.Attributes And FileAttributes.Hidden) <> 0 Then "Y" Else "N"))) 'TODO: Unsupported feature: conditional (?) operator.
                        // folderElem.Attributes.Append(XmlAttribute("System",(If(sd.Attributes And FileAttributes.System) <> 0 Then "Y" Else "N"))) 'TODO: Unsupported feature: conditional (?) operator.
                        // folderElem.Attributes.Append(XmlAttribute("ReadOnly",(If(sd.Attributes And FileAttributes.ReadOnly) <> 0 Then "Y" Else "N"))) 'TODO: Unsupported feature: conditional (?) operator.
                        startNode.AppendChild(AddElements(folderElem, sd.FullName, pathPrefix));
                    }
                }
                return startNode;
            }
            catch (Exception ex)
            {
                return XmlElement("error", ex.Message);
            }
        } // AddElements

        private XmlAttribute XmlAttribute(string attributeName, string attributeValue)
        {
            // 'PerfMon.Log("fsHelper", "XmlAttribute", attributeName & " - " & attributeValue)
            var xmlAttrib = moPageXML.CreateAttribute(attributeName);
            xmlAttrib.Value = FilterXMLString(attributeValue);
            return xmlAttrib;
        } // XmlAttribute

        private XmlElement XmlElement(string elementName, string elementValue)
        {
            // 'PerfMon.Log("fsHelper", "XmlElement", elementName & " - " & elementValue)
            var oElmt = moPageXML.CreateElement(elementName);
            oElmt.Attributes.Append(XmlAttribute("name", FilterXMLString(elementValue)));
            return oElmt;
        }

        private string FilterXMLString(string inputString)
        {
            // 'PerfMon.Log("fsHelper", "FilterXMLString")
            string returnString = inputString;
            if (inputString.IndexOf("&") > 0)
            {
                returnString = inputString.Replace("&", "&amp;");
            }
            if (inputString.IndexOf("'") > 0)
            {
                returnString = inputString.Replace("'", "&apos;");
            }
            return returnString;
        }

        #endregion
        #region Shared Methods

        public static string PathToURL(string path)
        {
            try
            {
                return path.Replace(@"\", "/");
            }
            catch (Exception ex)
            {
                return path;
            }
        }
        public static string URLToPath(string path)
        {
            try
            {
                return path.Replace("/", @"\");
            }
            catch (Exception ex)
            {
                return path;
            }
        }


        public static List<string> EnumerateFolders(DirectoryInfo dir)
        {

            var folderList = new List<string>();

            try
            {
                // .NET4  Upgrade scope: could use LINQ to effectively filter these in one go
                if (dir.Exists)
                {
                    folderList.Add(dir.FullName);

                    foreach (DirectoryInfo childDir in dir.GetDirectories())
                    {

                        // Exclude system and hidden folders, plus frontpage and eonicweb thumbnail
                        if ((childDir.Attributes & FileAttributes.System) == 0 & (childDir.Attributes & FileAttributes.Hidden) == 0 & !childDir.Name.StartsWith("~") & !childDir.Name.StartsWith("_vti") & !childDir.Name.StartsWith("_private"))




                        {

                            folderList.AddRange(EnumerateFolders(childDir));

                        }

                    }
                }
            }


            catch (Exception ex)
            {

            }


            return folderList;

        }

        public static string GetFileLibraryPath(LibraryType library)
        {

            string path = "";
            try
            {

                var config = Protean.Cms.Config();

                switch (library)
                {
                    case LibraryType.Image:
                        {
                            path = Tools.Text.Coalesce(Protean.Cms.ConfigValue("ImageRootPath"), "/images");
                            break;
                        }
                    case LibraryType.Documents:
                        {
                            path = Tools.Text.Coalesce(Protean.Cms.ConfigValue("DocRootPath"), "/docs");
                            break;
                        }
                    case LibraryType.Media:
                        {
                            path = Tools.Text.Coalesce(Protean.Cms.ConfigValue("MediaRootPath"), "/media");
                            break;
                        }
                    case LibraryType.Scripts:
                        {
                            path = Tools.Text.Coalesce(Protean.Cms.ConfigValue("ScriptsRootPath"), "/js");
                            break;
                        }
                    case LibraryType.Style:
                        {
                            path = Tools.Text.Coalesce(Protean.Cms.ConfigValue("StyleRootPath"), "/css");
                            break;
                        }
                }

                path = Strings.Replace(path, @"\", "/");

                // remove trailing slash

                if (path.EndsWith("/"))
                {
                    path = path.TrimEnd('/');
                }
            }

            catch (Exception ex)
            {
                path = "";
            }

            return path;

        }

        /// <summary>
        /// Gets a filtered list of files in a given folder.
        /// </summary>
        /// <param name="physicalFolderPath">The physical location of the folder to inspect (not a relative path)</param>
        /// <param name="libraryType">The library type to filter by</param>
        /// <param name="includeSubfolders">Determine whether to just look at the immediate folder or inspect sub folders</param>
        /// <returns>The List(Of String) of physical path of the files available with the filters above applied</returns>
        /// <remarks>If includeSubfolders is on it may include thumbnail folders' contents</remarks>
        public static List<string> GetFileListByTypeFromPhysicalPath(string physicalFolderPath, LibraryType libraryType, bool includeSubfolders = false)
        {

            var fileList = new List<string>();
            var folder = new DirectoryInfo(physicalFolderPath);

            if (folder.Exists)
            {
                // Filter out the files by type and return the full name
                var fileInfoList = new List<FileInfo>(folder.GetFiles("*.*", (SearchOption)Conversions.ToInteger(Interaction.IIf(includeSubfolders, SearchOption.AllDirectories, SearchOption.TopDirectoryOnly))));
                fileInfoList = fileInfoList.FindAll(new PredicateWrapper<FileInfo, LibraryType>(libraryType, FileInfoTypeFilter));
                fileList = fileInfoList.ConvertAll(new Converter<FileInfo, string>(FullNameFromFileInfo));
            }

            return fileList;
        }

        /// <summary>
        /// Gets a list of files for a library type from a path relative to Server.MapPath
        /// </summary>
        /// <param name="relativeFolderPath"></param>
        /// <param name="libraryType"></param>
        /// <param name="includeSubfolders"></param>
        /// <returns></returns>
        /// <remarks>Note that this may not work if you have nested virtual directories</remarks>
        public List<string> GetFileListByTypeFromRelativePath(string relativeFolderPath, LibraryType libraryType, bool includeSubfolders = false)
        {
            // PerfMon.Log("fsHelper", "GetFileListByTypeFromRelativePath")

            var fileList = new List<string>();

            try
            {

                // Work out the relative path

                // First out find the root folder physical path
                // Basically we want to account for virtual directories
                relativeFolderPath = relativeFolderPath.Trim(@"/\".ToCharArray());
                string rootFolderPath = "";
                if (!string.IsNullOrEmpty(relativeFolderPath))
                {
                    rootFolderPath = Tools.Text.SimpleRegexFind(relativeFolderPath, "^([^/]+?)(/.*)?$", 1);
                }
                var rootFolder = new DirectoryInfo(goServer.MapPath("/" + rootFolderPath));
                var folderToInspect = new DirectoryInfo(goServer.MapPath("/" + relativeFolderPath));

                if (rootFolder.Exists)
                {
                    // Get the list of files for the folder
                    fileList = GetFileListByTypeFromPhysicalPath(folderToInspect.FullName, libraryType, includeSubfolders);
                    fileList = fileList.ConvertAll((physicalPath) => ConvertPhysicalFilePathToVirtualPath(physicalPath, rootFolder.FullName, rootFolderPath));
                }
            }


            catch (Exception ex)
            {

            }

            return fileList;
        }


        /// <summary>
        /// Converts a physical path to a virtual path.  Virtual path information is provided in the parameters.
        /// </summary>
        /// <param name="physicalFilePath">The physical path to inspect (e.g. c:\temp\myimages\myfolder\mypath)</param>
        /// <param name="rootFolderPhysicalPath">The physical path of the virtual root folder (e.g. c:\temp\myimages)</param>
        /// <param name="rootFolderVirtualPath">The virtual path of the virtual root folder (not / but /images for example)</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string ConvertPhysicalFilePathToVirtualPath(string physicalFilePath, string rootFolderPhysicalPath, string rootFolderVirtualPath)
        {

            return "/" + rootFolderVirtualPath + physicalFilePath.Replace(rootFolderPhysicalPath.TrimEnd(@"/\".ToCharArray()), "").Replace(@"\", "/");

        }


        /// <summary>
        /// Determines whether a given file (FileInfo) is a valid file for the Protean.fsHelper.LibraryType.
        /// The filter includes inspecting the file extension, the file attributes (system and hidden are ignored)
        /// and whether the filename begins ~, which is an EonicWeb generated image so can be ignored.
        /// </summary>
        /// <param name="fileToInspect">The file to apply the filter against</param>
        /// <param name="libraryTypeFilter">The Protean.fsHelper.LibraryType to filter against.</param>
        /// <returns>True if the file is a valid file for the given library type, False otherwise.</returns>
        /// <remarks></remarks>
        public static bool FileInfoTypeFilter(FileInfo fileToInspect, LibraryType libraryTypeFilter)
        {

            var filter = new List<string>(_libraryTypeExtensions[(int)libraryTypeFilter]);

            return (fileToInspect.Attributes & FileAttributes.System) == 0 & (fileToInspect.Attributes & FileAttributes.Hidden) == 0 & !fileToInspect.Name.StartsWith("~") & (filter.Count == 0 | filter.Contains(fileToInspect.Extension.ToLower().TrimStart('.')));



        }

        /// <summary>
        /// Gets the full file path for a given file.
        /// </summary>
        /// <param name="fileToInspect"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string FullNameFromFileInfo(FileInfo fileToInspect)
        {
            return fileToInspect.FullName;
        }

        public static LibraryType GetLibraryTypeFromExtension(string extension)
        {
            extension = extension.TrimStart('.');
            int typeIndex = Array.FindIndex(_libraryTypeExtensions, (typeExtensions) => Array.IndexOf(typeExtensions, extension) > -1);
            if (typeIndex == -1)
            {
                return LibraryType.Undefined;
            }
            else
            {
                return (LibraryType)typeIndex;
            }
        }

        public static string GetDefaultContentSchemaNamesForLibraryType(LibraryType libraryType)
        {
            return _defaultLibraryTypeContentSchemaNames[(int)libraryType];
        }

        #endregion

    }

    public class FilesStatus
    {
        public const string HandlerPath = "/";

        public string group
        {
            get
            {
                return m_group;
            }
            set
            {
                m_group = value;
            }
        }
        private string m_group;
        public string name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
            }
        }
        private string m_name;
        public string type
        {
            get
            {
                return m_type;
            }
            set
            {
                m_type = value;
            }
        }
        private string m_type;
        public int size
        {
            get
            {
                return m_size;
            }
            set
            {
                m_size = value;
            }
        }
        private int m_size;
        public string progress
        {
            get
            {
                return m_progress;
            }
            set
            {
                m_progress = value;
            }
        }
        private string m_progress;
        public string url
        {
            get
            {
                return m_url;
            }
            set
            {
                m_url = value;
            }
        }
        private string m_url;
        public string thumbnail_url
        {
            get
            {
                return m_thumbnail_url;
            }
            set
            {
                m_thumbnail_url = value;
            }
        }
        private string m_thumbnail_url;
        public string delete_url
        {
            get
            {
                return m_delete_url;
            }
            set
            {
                m_delete_url = value;
            }
        }
        private string m_delete_url;
        public string delete_type
        {
            get
            {
                return m_delete_type;
            }
            set
            {
                m_delete_type = value;
            }
        }
        private string m_delete_type;
        public string error
        {
            get
            {
                return m_error;
            }
            set
            {
                m_error = value;
            }
        }
        private string m_error;

        public FilesStatus()
        {
        }

        public FilesStatus(FileInfo fileInfo)
        {
            SetValues(fileInfo.Name, (int)fileInfo.Length);
        }

        public FilesStatus(string fileName, int fileLength)
        {
            SetValues(fileName, fileLength);
        }

        private void SetValues(string fileName, int fileLength)
        {
            name = fileName;
            type = "image/png";
            size = fileLength;
            progress = "1.0";
            url = HandlerPath + "FileTransferHandler.ashx?f=" + fileName;
            delete_url = HandlerPath + "FileTransferHandler.ashx?f=" + fileName;
            delete_type = "DELETE";
        }
    }
}