using System;
using System.Data;
using System.Diagnostics;

using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
// This is the Indexer/Search items
using Lucene.Net.Index;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using static Protean.stdTools;
// regular expressions
using static Protean.Tools.FileHelper;
using static Protean.Tools.Xml;

namespace Protean
{
    public class Indexer
    {
        private string mcModuleName = "Protean.Indexer";
        private string mcIndexReadFolder = ""; // the folder where the index is stored (from config)
        private string mcIndexWriteFolder = ""; // the folder where the index is stored (from config)
        private string mcIndexCopyFolder = "";
        private IndexWriter oIndexWriter; // Lucene class

        private Tools.Security.Impersonate moImp = new Tools.Security.Impersonate(); // impersonate for access
        private bool bNewIndex = false; // if we need a new index or just add to one
        private bool bIsError = false;
        private DateTime dStartTime;
        private bool bDebug = false;

        public int nPagesIndexed;
        public int nDocumentsIndexed;
        public int nDocumentsSkipped;
        public int nContentsIndexed;

        public string cExError;
        private System.Collections.Specialized.NameValueCollection moConfig;

        private Cms myWeb;


        public Indexer(ref Cms aWeb)
        {
            // PerfMon.Log("Indexer", "New")
            mcModuleName = "Eonic.Search.Indexer";
            string cProcessInfo = string.Empty;
            myWeb = aWeb;
            moConfig = myWeb.moConfig;
            string siteSearchPath = moConfig["SiteSearchPath"];
            if (Strings.LCase(moConfig["SiteSearchDebug"]) == "on")
            {
                bDebug = true;
            }
            try
            {
                if (string.IsNullOrEmpty(siteSearchPath))
                    siteSearchPath = @"..\Index";
                if (!siteSearchPath.EndsWith(@"\"))
                    siteSearchPath = siteSearchPath + @"\";
                string IndexFolder = myWeb.goServer.MapPath(@"\") + siteSearchPath;
                var dir = new DirectoryInfo(IndexFolder);

                if (string.IsNullOrEmpty(moConfig["SiteSearchReadPath"]))
                {
                    mcIndexReadFolder = IndexFolder + @"Read\";
                }
                else
                {
                    mcIndexReadFolder = myWeb.goServer.MapPath(@"\") + moConfig["SiteSearchReadPath"];
                } // get the location to store the index
                var dirRead = new DirectoryInfo(mcIndexReadFolder);
                if (!dirRead.Exists)
                {
                    dir.CreateSubdirectory("Read");
                }

                if (string.IsNullOrEmpty(moConfig["SiteSearchWritePath"]))
                {
                    mcIndexWriteFolder = IndexFolder + @"Write\";
                }
                else
                {
                    mcIndexWriteFolder = myWeb.goServer.MapPath(@"\") + moConfig["SiteSearchWritePath"];
                } // get the location to store the index
                var dirWrite = new DirectoryInfo(mcIndexReadFolder);
                if (!dirWrite.Exists)
                {
                    dir.CreateSubdirectory("Write");
                }

                mcIndexCopyFolder = IndexFolder + @"IndexedSite\";
                var dirCopy = new DirectoryInfo(mcIndexCopyFolder);
                if (!dirCopy.Exists)
                {
                    dir.CreateSubdirectory("IndexedSite");
                }
            }

            catch (Exception ex)
            {
                cExError += ex.ToString() + Constants.vbCrLf;
                stdTools.returnException(ref myWeb.msException, mcModuleName, "New", ex, "", bDebug: gbDebug);
            }
        }

        public string GetIndexInfo()
        {
            var oXmlDoc = new XmlDocument();
            var oXmlDocRead = new XmlDocument();
            var oXmlDocWrite = new XmlDocument();
            var oXmlElmt = oXmlDoc.CreateElement("IndexInfo");
            var oXmlReadElmt = oXmlDoc.CreateElement("Read");
            var oXmlWriteElmt = oXmlDoc.CreateElement("Write");
            try
            {
                var fs = new FileInfo(mcIndexReadFolder + "/indexInfo.xml");
                if (fs.Exists)
                {
                    oXmlDocRead.Load(mcIndexReadFolder + "/indexInfo.xml");
                    oXmlDocRead.DocumentElement.SetAttribute("folder", "read");
                    oXmlReadElmt.InnerXml = oXmlDocRead.DocumentElement.OuterXml;
                    oXmlElmt.AppendChild(oXmlReadElmt.FirstChild);
                }
                fs = new FileInfo(mcIndexWriteFolder + "/indexInfo.xml");
                if (fs.Exists)
                {
                    oXmlDocWrite.Load(mcIndexWriteFolder + "/indexInfo.xml");
                    oXmlDocWrite.DocumentElement.SetAttribute("folder", "write");
                    oXmlWriteElmt.InnerXml = oXmlDocWrite.DocumentElement.OuterXml;
                    oXmlElmt.AppendChild(oXmlWriteElmt.FirstChild);
                }

                fs = null;
                return oXmlElmt.OuterXml;
            }

            catch (Exception ex)
            {
                cExError += ex.ToString() + Constants.vbCrLf;
                stdTools.returnException(ref myWeb.msException, mcModuleName, "New", ex, "", bDebug: gbDebug);
                return null;
            }

        }

        public void DoIndex(ref bool bResult, int nPage = 0)
        {
            // PerfMon.Log("Indexer", "DoIndex")
            string cProcessInfo = "";
            string cPageHtml = "";
            string cPageExtract = "";
            string CommonPath = "/ewcommon";
            string cPageXsl = "/xsl/search/cleanPage.xsl";
            string cExtractXsl = "/xsl/search/extract.xsl";
            if (Cms.bs5)
            {
                CommonPath = "/ptn";
                cPageXsl = "/features/search/cleanPage.xsl";
                cExtractXsl = "/features/search/extract.xsl";
            }
            var oPageXml = new XmlDocument();
            XmlElement oElmtRules = null;
            XmlElement oElmtURL = null;
            string cRules = "";
            long nPagesSkipped = 0L;
            long nContentSkipped = 0L;
            int nIndexed = 0; // count of the indexed items
            string cIndexDetailTypes = "NewsArticle,Event,Product,Contact,Document,Job";
            var oDS = new DataSet();
            string cSQL = "";
            string[] IndexDetailTypes;
            XmlElement errElmt;

            var oIndexInfo = new XmlDocument();
            var oInfoElmt = oIndexInfo.CreateElement("indexInfo");
            oIndexInfo.AppendChild(oInfoElmt);

            try
            {

                bIsError = false;
                if (nPage == 0)
                    bNewIndex = true; // if we are indexing everything then is a new index
                if (!string.IsNullOrEmpty(moConfig["SiteSearchIndexDetailTypes"]))
                {
                    cIndexDetailTypes = moConfig["SiteSearchIndexDetailTypes"];
                }
                IndexDetailTypes = Strings.Split(Strings.Replace(cIndexDetailTypes, " ", ""), ",");
                dStartTime = DateTime.Now;

                // checking index file size start
                string TotalSize;

                var infoReader = new FileInfo(Path.GetDirectoryName(Path.GetDirectoryName(myWeb.goServer.MapPath(@"\"))) + @"\Index\Write\indexInfo.xml");
                if (infoReader.Exists)
                {
                    TotalSize = infoReader.Length.ToString();

                    if (myWeb.moConfig["indexFileSize"] != null)
                    {
                        if (Convert.ToInt32(myWeb.moConfig["indexFileSize"]) * 1048576 < Convert.ToInt32(TotalSize)) // converted config mb size to byte
                        {
                            return;
                        }
                    }
                }

                // checking index file size end
                StartIndex();

                if (bIsError)
                    return;


                // Check for local xsl or go to common
                if (!File.Exists(goServer.MapPath(cPageXsl)))
                {
                    cPageXsl = CommonPath + cPageXsl;
                }
                if (!File.Exists(goServer.MapPath(cExtractXsl)))
                {
                    cExtractXsl = CommonPath + cExtractXsl;
                }


                oInfoElmt.SetAttribute("startTime", XmlDate(DateTime.Now, true));
                oInfoElmt.SetAttribute("cPageXsl", cPageXsl);
                oInfoElmt.SetAttribute("cExtractXsl", cExtractXsl);
                oInfoElmt.SetAttribute("IndexDetailTypes", cIndexDetailTypes);

                oIndexInfo.Save(mcIndexWriteFolder + "/indexInfo.xml");

                // make a new web but going to have to overide some stuff
                // and double override to be sure
                var xWeb = new Cms();

                xWeb.InitializeVariables();
                xWeb.Open();
                xWeb.mnUserId = 1;
                if (myWeb.moConfig["SiteSearchIndexHiddenDetail"] == "on")
                {
                    myWeb.mbAdminMode = true;
                }
                else
                {
                    myWeb.mbAdminMode = false;
                }
                xWeb.ibIndexMode = true;
                xWeb.ibIndexRelatedContent = myWeb.moConfig["SiteSearchIndexRelatedContent"] == "on";

                // full pages
                cSQL = "Select nStructKey,cStructName From tblContentStructure"; // get all structure
                if (nPage > 0)
                    cSQL += " WHERE nStructKey = " + nPage; // unless a specific page
                                                            // If nPage > 0 Then cSQL &= " WHERE nStructKey = " & nPage & " Or nStructParId =" & nPage 'unless a specific page
                oDS = myWeb.moDbHelper.GetDataSet(cSQL, "Structure");

                // now we loop through the different tables and index the data
                // Do Pages
                if (oDS.Tables.Contains("Structure"))
                {
                    foreach (DataRow oDR in oDS.Tables["Structure"].Rows)
                    {

                        // checking index file size start

                        // infoReader = My.Computer.FileSystem.GetFileInfo(mcIndexWriteFolder & "indexInfo.xml")
                        infoReader = new FileInfo(Path.GetDirectoryName(Path.GetDirectoryName(myWeb.goServer.MapPath(@"\"))) + @"\Index\Write\indexInfo.xml");
                        if (infoReader.Exists)
                        {
                            TotalSize = infoReader.Length.ToString();
                            if (myWeb.moConfig["indexFileSize"] != null)
                            {
                                if (Convert.ToInt32(myWeb.moConfig["indexFileSize"]) * 1048576 < Convert.ToInt32(TotalSize)) // converted config mb size to byte
                                {
                                    StopIndex();
                                    return;
                                }
                            }
                        }

                        // checking index file size end


                        // here we get a copy of the outputted html
                        // as the admin user would see it
                        // without bieng in admin mode
                        // so we can see everything
                        xWeb.mnPageId = Convert.ToInt32(oDR["nStructKey"]);
                        xWeb.moPageXml = new XmlDocument();
                        xWeb.moContentDetail = null;
                        xWeb.mnArtId = 0;
                        xWeb.mbIgnorePath = true;
                        xWeb.mcEwSiteXsl = cPageXsl;
                        cPageHtml = xWeb.ReturnPageHTML(0, true);
                        cPageHtml = Strings.Replace(cPageHtml, "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">", "");
                        cPageHtml = Strings.Replace(cPageHtml, " xmlns=\"http://www.w3.org/1999/xhtml\"", "");

                        if (string.IsNullOrEmpty(cPageHtml))
                        {
                            // we have an error to handle
                            if (myWeb.msException != null)
                            {
                                var errorElmt = oIndexInfo.CreateElement("IndexElement");
                                errorElmt.InnerText = myWeb.msException;
                                try
                                {
                                    errorElmt.SetAttribute("pgid", Conversions.ToString(oDR["nStructKey"]));
                                    errorElmt.SetAttribute("name", Conversions.ToString(oDR["cStructName"]));
                                }
                                catch (Exception)
                                {

                                }
                                cExError += ControlChars.CrLf + errorElmt.OuterXml;
                            }
                            nPagesSkipped += 1L;
                        }
                        else
                        {
                            try
                            {
                                oPageXml.LoadXml(cPageHtml);
                                oElmtRules = (XmlElement)oPageXml.SelectSingleNode("/html/head/meta[@name='ROBOTS']");
                                oElmtURL = (XmlElement)xWeb.moPageXml.SelectSingleNode("/Page/Menu/descendant-or-self::MenuItem[@id='" + xWeb.mnPageId + "']");
                                cRules = "";

                                // If xWeb.mnPageId = 156 Then
                                // Testing what happens when we hit a specific page
                                // cProcessInfo = "our page"
                                // End If

                                if (oElmtRules != null)
                                    cRules = oElmtRules.GetAttribute("content");
                                if (!(Strings.InStr(cRules, "NOINDEX") > 0) & oElmtURL != null)
                                {
                                    if (!oElmtURL.GetAttribute("url").StartsWith("http") | Information.IsNumeric(oElmtURL.GetAttribute("url")))
                                    {

                                        string thisUrl = oElmtURL.GetAttribute("url");
                                        // strip ?pgid if index in admin mode
                                        if (thisUrl.Contains("?pgid"))
                                        {
                                            thisUrl = Strings.Left(thisUrl, Strings.InStr(thisUrl, "?") - 1);
                                        }

                                        IndexPage(thisUrl, oPageXml.DocumentElement);

                                        var oPageElmt = oInfoElmt.OwnerDocument.CreateElement("page");
                                        oPageElmt.SetAttribute("url", thisUrl);
                                        oInfoElmt.AppendChild(oPageElmt);
                                        thisUrl = null;
                                        nPagesIndexed += 1;
                                    }
                                    else
                                    {
                                        nPagesSkipped += 1L;
                                    }
                                }
                                else
                                {
                                    nPagesSkipped += 1L;
                                }
                            }
                            catch (Exception ex)
                            {
                                var oPageErrElmt = oIndexInfo.CreateElement("errorInfo");
                                oPageErrElmt.SetAttribute("pgid", xWeb.mnPageId.ToString());
                                oPageErrElmt.SetAttribute("type", "Page");
                                oPageErrElmt.InnerText = ex.Message + ex.StackTrace;
                                oInfoElmt.AppendChild(oPageErrElmt);
                                nPagesSkipped += 1L;
                            }

                            // Now let index the content of the pages
                            // Only index content where this is the parent page, so we don't index for multiple locations.
                            if (!(Strings.InStr(cRules, "NOFOLLOW") > 0))
                            {
                                XmlNodeList oContentElmts = xWeb.moPageXml.SelectNodes("/Page/Contents/Content[@type!='FormattedText' and @type!='PlainText' and @type!='MetaData' and @type!='Image' and @type!='xform' and @type!='report' and @type!='xformQuiz' and @type!='Module' and @parId=/Page/@id]");
                                foreach (XmlElement oElmt in oContentElmts)
                                {
                                    // If oElmt.GetAttribute("type") = "Company" Then
                                    // cProcessInfo = "Not Indexing - " & oElmt.GetAttribute("type")
                                    // End If
                                    // Dim wordToFind As String = oElmt.GetAttribute("type")
                                    // Dim wordIndex As Integer = Array.IndexOf(IndexDetailTypes, oElmt.GetAttribute("type"))
                                    if (!(Array.IndexOf(IndexDetailTypes, oElmt.GetAttribute("type")) >= 0))
                                    {
                                        // Don't index we don't want this type.
                                        cProcessInfo = "Not Indexing - " + oElmt.GetAttribute("type");
                                    }

                                    else
                                    {
                                        cProcessInfo = "Indexing - " + oElmt.GetAttribute("type") + "id=" + oElmt.GetAttribute("id") + " name=" + oElmt.GetAttribute("name");
                                        xWeb.moContentDetail = null;
                                        xWeb.moPageXml = new XmlDocument(); // we need to get this again with our content Detail
                                        xWeb.moDbHelper.moPageXml = xWeb.moPageXml;
                                        xWeb.mcEwSiteXsl = cPageXsl;
                                        xWeb.mnArtId = Convert.ToInt32(oElmt.GetAttribute("id"));
                                        cPageHtml = xWeb.ReturnPageHTML(0, true);
                                        // remove any declarations that might affect and Xpath Search
                                        cPageHtml = Strings.Replace(cPageHtml, "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">", "");
                                        cPageHtml = Strings.Replace(cPageHtml, " xmlns=\"http://www.w3.org/1999/xhtml\"", "");

                                        if (string.IsNullOrEmpty(cPageHtml))
                                        {
                                            // we have an error to handle
                                            if (myWeb.msException != null)
                                            {
                                                var errorElmt = oIndexInfo.CreateElement("IndexElement");
                                                errorElmt.InnerXml = myWeb.msException;
                                                try
                                                {
                                                    errorElmt.SetAttribute("pgid", Conversions.ToString(oDR["nStructKey"]));
                                                    errorElmt.SetAttribute("name", Conversions.ToString(oDR["cStructName"]));
                                                }
                                                catch (Exception)
                                                {
                                                    // Don't error if you can't set the above.
                                                }
                                                cExError += ControlChars.CrLf + errorElmt.OuterXml;
                                            }
                                            nPagesSkipped += 1L;
                                        }
                                        else
                                        {
                                            try
                                            {
                                                oPageXml.LoadXml(cPageHtml);

                                                if (!(oElmt.GetAttribute("type") == "Document"))
                                                {
                                                    oElmtRules = (XmlElement)oPageXml.SelectSingleNode("/html/head/meta[@name='ROBOTS']");
                                                    cRules = "";

                                                    string sPageUrl = string.Empty;
                                                    if (oElmtURL != null)
                                                    {
                                                        sPageUrl = oElmtURL.GetAttribute("url");
                                                    }
                                                    if (oElmtRules != null)
                                                        cRules = oElmtRules.GetAttribute("content");
                                                    if (!(Strings.InStr(cRules, "NOINDEX") > 0) & !sPageUrl.StartsWith("http"))
                                                    {

                                                        // handle cannonical link tag
                                                        if (oPageXml.DocumentElement.SelectSingleNode("descendant-or-self::link[@rel='canonical']") != null)
                                                        {
                                                            XmlElement oLinkElmt = (XmlElement)oPageXml.DocumentElement.SelectSingleNode("descendant-or-self::link[@rel='canonical']");
                                                            if (!string.IsNullOrEmpty(oLinkElmt.GetAttribute("href")))
                                                            {
                                                                sPageUrl = oLinkElmt.GetAttribute("href");
                                                            }
                                                        }

                                                        IndexPage(sPageUrl, oPageXml.DocumentElement, oElmt.GetAttribute("type"));

                                                        var oPageElmt = oInfoElmt.OwnerDocument.CreateElement("page");
                                                        oPageElmt.SetAttribute("url", sPageUrl);
                                                        oInfoElmt.AppendChild(oPageElmt);

                                                        nIndexed += 1;
                                                        nContentsIndexed += 1;
                                                    }
                                                    else
                                                    {
                                                        nContentSkipped += 1L;
                                                    }
                                                }
                                                else
                                                {
                                                    foreach (XmlElement oDocElmt in oElmt.SelectNodes("descendant-or-self::Path"))
                                                    {
                                                        if (!string.IsNullOrEmpty(oDocElmt.InnerText))
                                                        {
                                                            if (oDocElmt.InnerText.StartsWith("http"))
                                                            {
                                                                // don't index
                                                                nDocumentsSkipped += 1;
                                                            }
                                                            else
                                                            {
                                                                var xFilePath = new FileInfo(myWeb.goServer.MapPath(oDocElmt.InnerText));
                                                                if (xFilePath.Exists)
                                                                {
                                                                    cProcessInfo = "Indexing - " + oDocElmt.InnerText;
                                                                    string DocName = oElmt.GetAttribute("name");
                                                                    if (string.IsNullOrEmpty(DocName))
                                                                    {
                                                                        DocName = xFilePath.Name;
                                                                    }
                                                                    if (string.IsNullOrEmpty(DocName))
                                                                    {
                                                                        DocName = oElmt.SelectSingleNode("Title").InnerText;
                                                                    }
                                                                    if (string.IsNullOrEmpty(DocName))
                                                                    {
                                                                        DocName = "Document for Download";
                                                                    }
                                                                    string fileAsText = GetFileText(myWeb.goServer.MapPath(oDocElmt.InnerText));

                                                                    var oPageElmt = oInfoElmt.OwnerDocument.CreateElement("page");
                                                                    oPageElmt.SetAttribute("name", DocName);
                                                                    oPageElmt.SetAttribute("file", oDocElmt.InnerText);
                                                                    oPageElmt.SetAttribute("publish", oElmt.GetAttribute("publish"));
                                                                    oPageElmt.SetAttribute("updated", oElmt.GetAttribute("update"));
                                                                    oInfoElmt.AppendChild(oPageElmt);

                                                                    DateTime dPublish = Conversions.ToDate(Interaction.IIf(Information.IsDate(oElmt.GetAttribute("publish")), Conversions.ToDate(oElmt.GetAttribute("publish")), null));
                                                                    DateTime dUpdate = Conversions.ToDate(Interaction.IIf(Information.IsDate(oElmt.GetAttribute("update")), Conversions.ToDate(oElmt.GetAttribute("update")), null));

                                                                    IndexPage(xWeb.mnPageId, "<h1>" + DocName + "</h1>" + fileAsText, oDocElmt.InnerText, DocName, "Download", xWeb.mnArtId, cPageExtract, dPublish, dUpdate);



                                                                    nIndexed += 1;
                                                                    nDocumentsIndexed += 1;
                                                                }
                                                                else
                                                                {
                                                                    nDocumentsSkipped += 1;
                                                                }
                                                            }

                                                        }
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                var oPageErrElmt = oIndexInfo.CreateElement("errorInfo");
                                                oPageErrElmt.SetAttribute("pgid", xWeb.mnPageId.ToString());
                                                oPageErrElmt.SetAttribute("type", oElmt.GetAttribute("type"));
                                                oPageErrElmt.SetAttribute("artid", oElmt.GetAttribute("id"));
                                                oPageErrElmt.InnerText = ex.Message;
                                                oInfoElmt.AppendChild(oPageErrElmt);
                                                nContentSkipped += 1L;
                                                cProcessInfo = cPageHtml;
                                            }

                                        }

                                    }
                                }

                            }

                            if (bIsError)
                            {
                                bResult = bIsError;

                                errElmt = oIndexInfo.CreateElement("error");
                                errElmt.InnerXml = cExError;
                                oIndexInfo.FirstChild.AppendChild(errElmt);

                                oInfoElmt.SetAttribute("endTime", XmlDate(DateTime.Now, true));
                            }

                            // oInfoElmt.SetAttribute("indexCount", nIndexed)
                            // oInfoElmt.SetAttribute("pagesIndexed", nPagesIndexed)
                            // oInfoElmt.SetAttribute("pagesSkipped", nPagesSkipped)
                            // oInfoElmt.SetAttribute("contentCount", nContentsIndexed)
                            // oInfoElmt.SetAttribute("contentSkipped", nContentSkipped)
                            // oInfoElmt.SetAttribute("documentsIndexed", nDocumentsIndexed)
                            // oInfoElmt.SetAttribute("documentsSkipped", nDocumentsSkipped)


                            if (oInfoElmt.GetAttribute("indexCount") != null)
                            {
                                oInfoElmt.SetAttribute("indexCount", nIndexed.ToString());
                            }
                            if (oInfoElmt.GetAttribute("pagesIndexed") != null)
                            {
                                oInfoElmt.SetAttribute("pagesIndexed", nPagesIndexed.ToString());
                            }
                            if (oInfoElmt.GetAttribute("pagesSkipped") != null)
                            {
                                oInfoElmt.SetAttribute("pagesSkipped", nPagesSkipped.ToString());
                            }
                            if (oInfoElmt.GetAttribute("contentCount") != null)
                            {
                                oInfoElmt.SetAttribute("contentCount", nContentsIndexed.ToString());
                            }
                            if (oInfoElmt.GetAttribute("contentSkipped") != null)
                            {
                                oInfoElmt.SetAttribute("contentSkipped", nContentSkipped.ToString());
                            }
                            if (oInfoElmt.GetAttribute("documentsIndexed") != null)
                            {
                                oInfoElmt.SetAttribute("documentsIndexed", nDocumentsIndexed.ToString());
                            }
                            if (oInfoElmt.GetAttribute("documentsSkipped") != null)
                            {
                                oInfoElmt.SetAttribute("documentsSkipped", nDocumentsSkipped.ToString());
                            }
                            oIndexInfo.Save(mcIndexWriteFolder + "/indexInfo.xml");

                            if (bIsError)
                            {
                                return;
                            }

                        }

                    }
                }

                if (oInfoElmt.GetAttribute("endTime") != null)
                {
                    oInfoElmt.SetAttribute("endTime", XmlDate(DateTime.Now, true));
                }
                if (oInfoElmt.GetAttribute("indexCount") != null)
                {
                    oInfoElmt.SetAttribute("indexCount", nIndexed.ToString());
                }
                if (oInfoElmt.GetAttribute("pagesIndexed") != null)
                {
                    oInfoElmt.SetAttribute("pagesIndexed", nPagesIndexed.ToString());
                }
                if (oInfoElmt.GetAttribute("pagesSkipped") != null)
                {
                    oInfoElmt.SetAttribute("pagesSkipped", nPagesSkipped.ToString());
                }
                if (oInfoElmt.GetAttribute("contentCount") != null)
                {
                    oInfoElmt.SetAttribute("contentCount", nContentsIndexed.ToString());
                }
                if (oInfoElmt.GetAttribute("contentSkipped") != null)
                {
                    oInfoElmt.SetAttribute("contentSkipped", nContentSkipped.ToString());
                }
                if (oInfoElmt.GetAttribute("documentsIndexed") != null)
                {
                    oInfoElmt.SetAttribute("documentsIndexed", nDocumentsIndexed.ToString());
                }
                if (oInfoElmt.GetAttribute("documentsSkipped") != null)
                {
                    oInfoElmt.SetAttribute("documentsSkipped", nDocumentsSkipped.ToString());
                }

                // any non-critical errors ?
                if (!string.IsNullOrEmpty(cExError))
                {
                    errElmt = oIndexInfo.CreateElement("error");
                    errElmt.InnerXml = cExError;
                    oIndexInfo.FirstChild.AppendChild(errElmt);
                }


                oIndexInfo.Save(mcIndexWriteFolder + "/indexInfo.xml");

                StopIndex();
                bResult = bIsError;
                if (bIsError)
                    return;
                var oTime = dStartTime.Subtract(DateTime.Now);

                xWeb.Close();
            }

            catch (Exception ex)
            {
                cExError += ex.InnerException.StackTrace.ToString() + Constants.vbCrLf;
                stdTools.returnException(ref myWeb.msException, mcModuleName, "DoIndex", ex, "", cProcessInfo, gbDebug);
                errElmt = oIndexInfo.CreateElement("error");
                errElmt.InnerXml = cExError;
                oIndexInfo.FirstChild.AppendChild(errElmt);


                // oInfoElmt.SetAttribute("endTime", Tools.Xml.XmlDate(Now(), True))
                // oInfoElmt.SetAttribute("indexCount", nIndexed)
                // oInfoElmt.SetAttribute("pagesIndexed", nPagesIndexed)
                // oInfoElmt.SetAttribute("pagesSkipped", nPagesSkipped)
                // oInfoElmt.SetAttribute("contentCount", nContentsIndexed)
                // oInfoElmt.SetAttribute("contentSkipped", nContentSkipped)
                // oInfoElmt.SetAttribute("documentsIndexed", nDocumentsIndexed)
                // oInfoElmt.SetAttribute("documentsSkipped", nDocumentsSkipped)

                if (oInfoElmt.GetAttribute("endTime") != null)
                {
                    oInfoElmt.SetAttribute("endTime", XmlDate(DateTime.Now, true));
                }
                if (oInfoElmt.GetAttribute("indexCount") != null)
                {
                    oInfoElmt.SetAttribute("indexCount", nIndexed.ToString());
                }
                if (oInfoElmt.GetAttribute("pagesIndexed") != null)
                {
                    oInfoElmt.SetAttribute("pagesIndexed", nPagesIndexed.ToString());
                }
                if (oInfoElmt.GetAttribute("pagesSkipped") != null)
                {
                    oInfoElmt.SetAttribute("pagesSkipped", nPagesSkipped.ToString());
                }
                if (oInfoElmt.GetAttribute("contentCount") != null)
                {
                    oInfoElmt.SetAttribute("contentCount", nContentsIndexed.ToString());
                }
                if (oInfoElmt.GetAttribute("contentSkipped") != null)
                {
                    oInfoElmt.SetAttribute("contentSkipped", nContentSkipped.ToString());
                }
                if (oInfoElmt.GetAttribute("documentsIndexed") != null)
                {
                    oInfoElmt.SetAttribute("documentsIndexed", nDocumentsIndexed.ToString());
                }
                if (oInfoElmt.GetAttribute("documentsSkipped") != null)
                {
                    oInfoElmt.SetAttribute("documentsSkipped", nDocumentsSkipped.ToString());
                }

                oIndexInfo.Save(mcIndexWriteFolder + "/indexInfo.xml");

                try
                {
                    oIndexWriter.Dispose();
                    oIndexWriter = null;
                }
                catch (Exception)
                {

                }
            }
            finally
            {
                try
                {
                    // send email update as to index success or failure
                    cProcessInfo = "Sending Email Report";
                    var msg = new Protean.Messaging(ref myWeb.msException);
                    string serverSenderEmail = moConfig["ServerSenderEmail"] + "";
                    string serverSenderEmailName = moConfig["ServerSenderEmailName"] + "";
                    if (!Tools.Text.IsEmail(serverSenderEmail))
                    {
                        serverSenderEmail = "emailsender@eonichost.co.uk";
                        serverSenderEmailName = "eonicweb Email Sender";
                    }
                    string recipientEmail = moConfig["SiteAdminEmail"];
                    if (!string.IsNullOrEmpty(moConfig["IndexAlertEmail"]))
                    {
                        recipientEmail = moConfig["IndexAlertEmail"];
                    }

                    object indexerAlertXsltPath = "/ewcommon/xsl/Email/IndexerAlert.xsl";
                    if (Cms.bs5)
                    {
                        indexerAlertXsltPath = "/ptn/features/search/indexer-alert-email.xsl";
                    }


                    Protean.Cms.dbHelper argodbHelper = null;
                    msg.emailer(oInfoElmt, Convert.ToString(indexerAlertXsltPath), "EonicWebIndexer", serverSenderEmail, recipientEmail, myWeb.moRequest.ServerVariables["SERVER_NAME"] + " Indexer Report", odbHelper: ref argodbHelper);
                    msg = (Protean.Messaging)null;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "DoIndex", ex, "", cProcessInfo, gbDebug);
                }
            }


        }

        private void StartIndex()
        {
            // PerfMon.Log("Indexer", "StartIndex")
            string cProcessInfo = "";
            try
            {
                if (!string.IsNullOrEmpty(moConfig["AdminAcct"]) & moConfig["AdminGroup"] != "AzureWebApp")
                {
                    moImp = new Tools.Security.Impersonate(); // for access
                    if (!moImp.ImpersonateValidUser(moConfig["AdminAcct"], moConfig["AdminDomain"], moConfig["AdminPassword"], cInGroup: moConfig["AdminGroup"]))
                    {
                        Information.Err().Raise(108, Description: "Indexer did not authenticate with system credentials");
                    }

                }

                EmptyFolder(mcIndexWriteFolder);
                if (gbDebug)
                {
                    EmptyFolder(mcIndexCopyFolder);
                }

                bool bCreate = true; // check if file exists or if we need to create an index
                if (File.Exists(mcIndexWriteFolder + "segments"))
                    bCreate = false;
                // If bNewIndex Then bCreate = True 'override creation dependant on type of index
                Lucene.Net.Store.Directory indexDir = Lucene.Net.Store.FSDirectory.Open(mcIndexWriteFolder);

                var maxLen = new IndexWriter.MaxFieldLength(10000);

                oIndexWriter = new IndexWriter(indexDir, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_CURRENT), bCreate, maxLen); // create the index writer
            }



            catch (Exception ex)
            {
                cExError += ex.StackTrace.ToString() + Constants.vbCrLf;
                stdTools.returnException(ref myWeb.msException, mcModuleName, "StartIndex", ex, "", cProcessInfo, gbDebug);

                bIsError = true;
                try
                {
                    oIndexWriter.Dispose();
                    oIndexWriter = null;
                }
                catch (Exception)
                {

                }
            }
        }

        private void EmptyFolder(string cDirectory)
        {
            // PerfMon.Log("Indexer", "EmptyFolder")
            string cProcessInfo = "";
            try
            {
                if (bNewIndex)
                {
                    if (Directory.Exists(cDirectory))
                    {
                        // delete directories and thier children
                        while (Information.UBound(Directory.GetDirectories(cDirectory)) > 0)
                            Directory.Delete(Directory.GetDirectories(cDirectory)[0], true);
                        // delete files
                        while (Information.UBound(Directory.GetFiles(cDirectory)) > 0)
                        {
                            try
                            {
                                File.SetAttributes(Directory.GetFiles(cDirectory)[0], FileAttributes.Normal);
                                File.Delete(Directory.GetFiles(cDirectory)[0]);
                            }
                            catch (Exception ex)
                            {
                                cExError += ex.StackTrace.ToString() + Constants.vbCrLf;
                                stdTools.returnException(ref myWeb.msException, mcModuleName, "Empty Folder", ex, "", cProcessInfo, gbDebug);
                                return;

                            }
                        }
                    }

                    // try deleting a hidden folder
                    if (Directory.Exists(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(cDirectory, Interaction.IIf(Strings.Right(cDirectory, 1) == @"\", "", @"\")), "_vti_cnf"))))
                    {
                        try
                        {
                            Directory.Delete(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(cDirectory, Interaction.IIf(Strings.Right(cDirectory, 1) == @"\", "", @"\")), "_vti_cnf")), true);
                        }
                        catch (Exception ex)
                        {
                            cExError += ex.ToString() + Constants.vbCrLf;
                            stdTools.returnException(ref myWeb.msException, mcModuleName, "Empty Folder", ex, "", cProcessInfo, gbDebug);
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    oIndexWriter.Dispose();
                    oIndexWriter = null;
                }
                catch (Exception)
                {

                }
                cExError += ex.ToString() + Constants.vbCrLf;
                stdTools.returnException(ref myWeb.msException, mcModuleName, "Empty Folder", ex, "", cProcessInfo, gbDebug);
            }
        }

        private void IndexPage(string url, XmlElement pageXml, string pageType = "Page")
        {

            string methodName = "IndexPage(String,XmlElement,[String])";
            // PerfMon.Log("Indexer", methodName)

            string processInfo = url;

            try
            {

                var indexDoc = new Document();

                // Add the basic field types
                indexDoc.Add(new Field("url", url, Field.Store.YES, Field.Index.NOT_ANALYZED));
                indexDoc.Add(new Field("type", pageType, Field.Store.YES, Field.Index.NOT_ANALYZED));

                if (pageXml != null)
                {

                    // Add the meta data to the fields
                    foreach (XmlElement metaContent in pageXml.SelectNodes("/html/head/meta"))
                        indexMeta(ref indexDoc, metaContent);

                    // Add the text
                    XmlElement body = (XmlElement)pageXml.SelectSingleNode("/html/body");
                    string text = "";
                    if (body is null)
                    {
                        text = pageXml.OuterXml;
                    }
                    else
                    {
                        text = body.InnerXml;
                    }
                    indexDoc.Add(new Field("text", text, Field.Store.YES, Field.Index.ANALYZED)); // the actual content/text 

                }

                // Add document to the index
                oIndexWriter.AddDocument(indexDoc);

                // Save the page
                SavePage(url, pageXml.OuterXml);
            }

            catch (Exception ex)
            {
                try
                {
                    oIndexWriter.Dispose();
                    oIndexWriter = null;
                }
                catch (Exception)
                {
                }
                cExError += ex.StackTrace.ToString() + Constants.vbCrLf;
                stdTools.returnException(ref myWeb.msException, mcModuleName, methodName, ex, "", processInfo, gbDebug);
                bIsError = true;
            }
        }

        private void indexMeta(ref Document indexDocument, XmlElement metaContent, bool forSorting = false)
        {
            string processInfo = "";

            // Values grabbed from each content item
            var indexContent = Field.Index.NOT_ANALYZED;
            var storeContent = Field.Store.YES;
            string metaName = "";
            string metaContentValue = "";
            string metaType = "";

            // The abstract field and type specific fields
            AbstractField metaField = null;
            NumericField metaNumericField;

            // type specific value conversion
            object convertedNumber = null;
            DateTime convertedDate;

            try
            {

                // Determine whether to tokenize this - by default, no
                indexContent = (Field.Index)Conversions.ToInteger(Interaction.IIf(metaContent.GetAttribute("tokenize") == "true" & !forSorting, Field.Index.ANALYZED, Field.Index.NOT_ANALYZED));

                // Determine whether to store this - by default, YES
                storeContent = (Field.Store)Conversions.ToInteger(Interaction.IIf(metaContent.GetAttribute("store") == "false" | forSorting, Field.Store.NO, Field.Store.YES));

                metaName = metaContent.GetAttribute("name");
                if (forSorting)
                    metaName = "ewsort-" + metaName;

                // Get content - this will either be an attribute, or, if not present, the innerXml of the node
                if (metaContent.Attributes.GetNamedItem("content") is null)
                {
                    metaContentValue = metaContent.InnerXml;
                }

                else
                {
                    metaContentValue = metaContent.GetAttribute("content");
                }

                metaType = metaContent.GetAttribute("type");

                // Create the field - based on type
                switch (metaType ?? "")
                {



                    case "float":
                    case "number":
                        {

                            if (Tools.Number.CheckAndReturnStringAsNumber(metaContentValue, ref convertedNumber, (Type)Interaction.IIf(metaType == "float", typeof(float), typeof(long))))
                            {

                                // Create the numeric field
                                metaNumericField = new NumericField(metaName, storeContent, true);


                                // Set the value
                                switch (metaType ?? "")
                                {

                                    case "float":
                                        {
                                            metaNumericField.SetFloatValue(Conversions.ToSingle(convertedNumber));
                                            break;
                                        }

                                    default:
                                        {
                                            metaNumericField.SetLongValue(Conversions.ToLong(convertedNumber));
                                            break;
                                        }

                                }

                                metaField = metaNumericField;
                            }

                            else
                            {

                                // It's not a number don't add it
                                metaField = null;

                            }

                            break;
                        }


                    case "date":
                        {

                            // Check if the string is a valid date
                            if (!string.IsNullOrEmpty(metaContentValue) && DateTime.TryParse(metaContentValue, out convertedDate))
                            {

                                metaField = new Field(metaName, DateTools.DateToString(Conversions.ToDate(metaContentValue), DateTools.Resolution.SECOND), storeContent, indexContent);
                            }
                            else
                            {

                                // It's not a date don't add it
                                metaField = null;

                            }

                            break;
                        }

                    default:
                        {

                            metaField = new Field(metaName, metaContentValue, storeContent, indexContent);
                            break;
                        }

                }


                if (metaField != null)
                {
                    indexDocument.Add(metaField);

                    // Check for sorting
                    if (metaContent.GetAttribute("sortable") == "true" & !forSorting)
                    {
                        indexMeta(ref indexDocument, metaContent, true);
                    }
                }
            }

            catch (Exception ex)
            {
                cExError += ex.ToString() + Constants.vbCrLf;
                stdTools.returnException(ref myWeb.msException, mcModuleName, "indexMeta", ex, "", processInfo, gbDebug);
                bIsError = true;
            }

        }

        private void IndexPage(int nPageId, string cPageText, string cURL, string cPageTitle, string cContentType = "Page", long nContentId = 0L, string cAbstract = "", DateTime dPublish = default, DateTime dUpdate = default)
        {
            // PerfMon.Log("Indexer", "IndexPage")
            string cProcessInfo = cURL;

            try
            {
                var oDoc = new Document(); // This is the document element
                                           // here we need to get the proper paths
                if (string.IsNullOrEmpty(cPageTitle) | string.IsNullOrEmpty(cContentType))
                {

                    cExError += "No Name or Type";
                    bIsError = true;
                }

                else
                {
                    oDoc.Add(new Field("pgid", nPageId.ToString(), Field.Store.YES, Field.Index.ANALYZED));
                    oDoc.Add(new Field("artid", nContentId.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));

                    oDoc.Add(new Field("url", cURL, Field.Store.YES, Field.Index.NOT_ANALYZED)); // url of the page (simple)
                    oDoc.Add(new Field("name", cPageTitle, Field.Store.YES, Field.Index.NOT_ANALYZED)); // the name of the page
                    oDoc.Add(new Field("contenttype", cContentType, Field.Store.YES, Field.Index.NOT_ANALYZED));  // the type of the content


                    if (!string.IsNullOrEmpty(cAbstract))
                    {
                        oDoc.Add(new Field("abstract", cAbstract, Field.Store.YES, Field.Index.NOT_ANALYZED));
                    }
                    if (dPublish != default)
                    {
                        oDoc.Add(new Field("publishDate", XmlDate(dPublish), Field.Store.YES, Field.Index.NOT_ANALYZED));
                    }
                    if (dUpdate != default)
                    {
                        oDoc.Add(new Field("updateDate", XmlDate(dUpdate), Field.Store.YES, Field.Index.NOT_ANALYZED));

                    }
                    oDoc.Add(new Field("text", cPageText, Field.Store.YES, Field.Index.ANALYZED)); // the actual content/text 

                    oIndexWriter.AddDocument(oDoc); // add it to the index

                    SavePage(cURL, cPageText);
                }
            }

            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
            // This is also where we can recreate the site as static html if needed
            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@


            catch (Exception ex)
            {
                cExError += ex.ToString() + Constants.vbCrLf;
                stdTools.returnException(ref myWeb.msException, mcModuleName, "IndexPage", ex, "", cProcessInfo, gbDebug);
                bIsError = true;
            }
        }

        private void SavePage(string cUrl, string cBody)
        {
            // PerfMon.Log("Indexer", "SavePage")
            string cProcessInfo = "";
            string filename = "";
            string filepath = "";
            string artId = string.Empty;
            string Ext = ".html";
            try
            {
                if (bDebug)
                {


                    // let's clean up the url
                    if (cUrl.LastIndexOf("/?") > -1)
                    {
                        cUrl = cUrl.Substring(0, cUrl.LastIndexOf("/?"));
                    }
                    if (cUrl.LastIndexOf("?") > -1)
                    {
                        cUrl = cUrl.Substring(0, cUrl.LastIndexOf("?"));
                    }




                    if (string.IsNullOrEmpty(cUrl) | cUrl == "/")
                    {
                        filename = "home.html";
                    }
                    else
                    {
                        filename = Strings.Left(cUrl.Substring(cUrl.LastIndexOf("/") + 1), 240);
                        if (cUrl.LastIndexOf("/") > 0)
                        {
                            filepath = Strings.Left(cUrl.Substring(0, cUrl.LastIndexOf("/")), 240) + "";
                        }
                    }

                    var oFS = new fsHelper();
                    oFS.mcStartFolder = mcIndexCopyFolder;

                    cProcessInfo = "Saving:" + mcIndexCopyFolder + filepath + @"\" + filename + Ext;

                    // Tidy up the filename
                    filename = ReplaceIllegalChars(filename);
                    filename = Strings.Replace(filename, @"\", "-");
                    filepath = Strings.Replace(filepath, "/", @"\") + "";
                    if (filepath.StartsWith(@"\") & mcIndexCopyFolder.EndsWith(@"\"))
                    {
                        filepath.Remove(0, 1);
                    }

                    cProcessInfo = "Saving:" + mcIndexCopyFolder + filepath + @"\" + filename + Ext;
                    string FullFilePath = mcIndexCopyFolder + filepath + @"\" + filename;

                    if (FullFilePath.Length > 255)
                    {
                        FullFilePath = Strings.Left(FullFilePath, 240) + Ext;
                    }
                    else
                    {
                        FullFilePath = FullFilePath + Ext;
                    }

                    if (!string.IsNullOrEmpty(filepath))
                    {
                        string sError = oFS.CreatePath(filepath);
                        if (sError == "1")
                        {
                            File.WriteAllText(FullFilePath, cBody, System.Text.Encoding.UTF8);
                        }
                        else
                        {
                            cExError += "<Error>Create Path: " + filepath + " - " + sError + "</Error>" + Constants.vbCrLf;
                        }
                    }
                    else
                    {
                        File.WriteAllText(mcIndexCopyFolder + filename + Ext, cBody, System.Text.Encoding.UTF8);
                    }



                    oFS = default;
                }
            }
            catch (Exception ex)
            {
                // if saving of a page fails we are not that bothered.
                cExError += "<Error>" + filepath + filename + ex.Message + "</Error>" + Constants.vbCrLf;
                // returnException(myWeb.msException, mcModuleName, "SavePage", ex, "", cProcessInfo, gbDebug)
                // bIsError = True
            }
        }

        private void StopIndex()
        {
            // PerfMon.Log("Indexer", "StopIndex")
            string cProcessInfo = "";
            try
            {
                oIndexWriter.Optimize();
                oIndexWriter.Dispose();
                EmptyFolder(mcIndexReadFolder);
                CopyFolderContents(mcIndexWriteFolder, mcIndexReadFolder);
                if (!string.IsNullOrEmpty(moConfig["AdminAcct"]) & moConfig["AdminGroup"] != "AzureWebApp")
                {
                    moImp.UndoImpersonation();
                    moImp = null;
                }
            }
            catch (Exception ex)
            {
                cExError += ex.ToString() + Constants.vbCrLf;
                stdTools.returnException(ref myWeb.msException, mcModuleName, "StopIndex", ex, "", cProcessInfo, gbDebug);
                bIsError = true;
            }
        }

        private string GetFileText(string cPath, string cOtherText = "")
        {
            // PerfMon.Log("Indexer", "GetFileText")
            string cProcessInfo = "";
            try
            {
                var oFile = new FileDoc(cPath);
                return cOtherText + oFile.Text;
            }
            catch (Exception ex)
            {
                cExError += ex.ToString() + Constants.vbCrLf;
                stdTools.returnException(ref myWeb.msException, mcModuleName, "DoCheck", ex, "", cProcessInfo, gbDebug);
                return cOtherText;
            }
        }

        private void CopyFolderContents(string cLocation, string cDestination)
        {
            // PerfMon.Log("Indexer", "CopyFolderContents")
            string cProcessInfo = "";
            try
            {
                var oDI = new DirectoryInfo(mcIndexWriteFolder);
                int FileCount = oDI.GetFiles().Length;
                int i;
                var loopTo = FileCount - 1;
                for (i = 0; i <= loopTo; i++)
                {
                    string cFileName = Strings.Replace(Directory.GetFiles(cLocation)[i], cLocation, cDestination);
                    Debug.WriteLine(cFileName);
                    File.Copy(Directory.GetFiles(cLocation)[i], cFileName, true);
                }
            }
            catch (Exception ex)
            {
                cExError += ex.ToString() + Constants.vbCrLf;
                stdTools.returnException(ref myWeb.msException, mcModuleName, "CopyFolderContents", ex, "", cProcessInfo, gbDebug);
            }
        }

    }
}