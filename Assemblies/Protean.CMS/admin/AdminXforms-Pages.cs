// ***********************************************************************
// $Library:     protean.cms.adminXforms
// $Revision:    3.1  
// $Date:        2006-03-02
// $Author:      Trevor Spink (trevor@eonic.com)
// &Website:     eonic.com
// &Licence:     Apache-2.0 license
// $Copyright:   Copyright (c) 2002 - 2022 Eonic Digital LLP.
// ***********************************************************************

using Protean.Tools;
using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Xml;
using static Protean.stdTools;
using static Protean.Tools.Xml;

namespace Protean
{

    public partial class Cms
    {
        public partial class Admin
        {
            public partial class AdminXforms : xForm, IDisposable
            {

                public XmlElement xFrmEditPage(long pgid = 0L, string cName = "", string cFormName = "Page", string cParId = "")
                {
                    string cXmlFilePath;
                    XmlElement oFrmElmt = null;
                    XmlElement oSelElmt;
                    string cProcessInfo = "";

                    var oObjType = new Cms.dbHelper.objectTypes();
                    var oTblName = default(Cms.dbHelper.TableNames);
                    int nRContentId = 0;

                    try
                    {

                        cXmlFilePath = "/xforms/page/" + cFormName + ".xml";
                        if (goConfig["cssFramework"] == "bs5")
                        {
                            cXmlFilePath = "/admin" + cXmlFilePath;
                        }
                        if (!base.load(cXmlFilePath, myWeb.maCommonFolders))
                        {
                            // If not a custom page is loaded, pull in the standard elements
                            base.NewFrm("EditPage");
                            // MyBase.submission("EditEage", "admin.aspx?ewCmd=EditPage&pgid=46&xml=x", "post")

                            base.submission("EditEage", "", "post", "form_check(this)");

                            oFrmElmt = base.addGroup(ref base.moXformElmt, "EditPage", "", "Edit Page");
                            base.addInput(ref oFrmElmt, "nStructParId", true, "ParId", "hidden");
                            XmlElement argoBindParent = null;
                            base.addBind("nStructParId", "tblContentStructure/nStructParId", oBindParent: ref argoBindParent, "true()");

                            base.addInput(ref oFrmElmt, "cStructName", true, "Page Name", string.IsNullOrEmpty(cName) ? "" : "readonly");
                            XmlElement argoBindParent1 = null;
                            base.addBind("cStructName", "tblContentStructure/cStructName", oBindParent: ref argoBindParent1, "true()");

                            base.addInput(ref oFrmElmt, "cDisplayName", true, "Display Name");
                            XmlElement argoBindParent2 = null;
                            base.addBind("cDisplayName", "tblContentStructure/cStructDescription/DisplayName", oBindParent: ref argoBindParent2, "false()");

                            string argsClass = "xhtml";
                            int argnRows = 10;
                            int argnCols = 0;
                            base.addTextArea(ref oFrmElmt, "cStructDescription", true, "Description", ref argsClass, ref argnRows, nCols: ref argnCols);
                            XmlElement argoBindParent3 = null;
                            base.addBind("cStructDescription", "tblContentStructure/cStructDescription/Description", oBindParent: ref argoBindParent3, "false()");

                            if (myWeb.moConfig["ShowStructForiegnRef"].ToLower() == "yes" | myWeb.moConfig["ShowStructForiegnRef"].ToLower() == "on")
                            {
                                base.addInput(ref oFrmElmt, "cStructForiegnRef", true, "Foriegn Reference");
                                XmlElement argoBindParent4 = null;
                                base.addBind("cStructForiegnRef", "tblContentStructure/cStructForiegnRef", oBindParent: ref argoBindParent4, "false()");
                            }

                            if (myWeb.goLangConfig != null)
                            {
                                oSelElmt = base.addSelect1(ref oFrmElmt, "cLang", true, "Language", "", Protean.xForm.ApperanceTypes.Full);
                                base.addOption(ref oSelElmt, myWeb.goLangConfig.GetAttribute("default"), myWeb.goLangConfig.GetAttribute("code"));
                                foreach (XmlElement langNode in myWeb.goLangConfig.SelectNodes("Language"))
                                    base.addOption(ref oSelElmt, langNode.GetAttribute("systemName"), langNode.GetAttribute("code"));
                                XmlElement argoBindParent5 = null;
                                base.addBind("cLang", "tblContentStructure/cVersionLang", oBindParent: ref argoBindParent5, "tblContentStructure/nVersionType='3'");
                            }

                            base.addInput(ref oFrmElmt, "thumbnail", true, "Thumbnail Image", "short pickImage");
                            XmlElement argoBindParent6 = null;
                            base.addBind("thumbnail", "tblContentStructure/cStructDescription/Images/img[@class='thumbnail']", oBindParent: ref argoBindParent6, "false()", "xml-replace");

                            base.addInput(ref oFrmElmt, "cUrl", true, "URL");
                            XmlElement argoBindParent7 = null;
                            base.addBind("cUrl", "tblContentStructure/cUrl", oBindParent: ref argoBindParent7, "false()");

                            base.addInput(ref oFrmElmt, "dPublishDate", true, "Publish Date", "calendar");
                            XmlElement argoBindParent8 = null;
                            base.addBind("dPublishDate", "tblContentStructure/dPublishDate", oBindParent: ref argoBindParent8, "false()");

                            base.addInput(ref oFrmElmt, "dExpireDate", true, "Expire Date", "calendar");
                            XmlElement argoBindParent9 = null;
                            base.addBind("dExpireDate", "tblContentStructure/dExpireDate", oBindParent: ref argoBindParent9, "false()");

                            oSelElmt = base.addSelect1(ref oFrmElmt, "nStatus", true, "Status", "", Protean.xForm.ApperanceTypes.Full);
                            base.addOption(ref oSelElmt, "Live", 1.ToString());
                            base.addOption(ref oSelElmt, "Hidden", 0.ToString());
                            XmlElement argoBindParent10 = null;
                            base.addBind("nStatus", "tblContentStructure/nStatus", oBindParent: ref argoBindParent10, "true()");

                            base.addInput(ref oFrmElmt, "cDescription", true, "Change Notes");
                            XmlElement argoBindParent11 = null;
                            base.addBind("cDescription", "tblContentStructure/cDescription", oBindParent: ref argoBindParent11, "false()");

                            base.addSubmit(ref oFrmElmt, "", "Save Page");

                        }

                        // As this is only needed for Related Content Pages
                        XmlNodeState localNodeState() { var argoNode = base.Instance; var ret = Xml.NodeState(ref argoNode, "tblContentStructure/RelatedContent"); base.Instance = argoNode; return ret; }

                        if (localNodeState() == XmlNodeState.HasContents)
                        {
                            foreach (Cms.dbHelper.TableNames currentOTblName in Enum.GetValues(typeof(Cms.dbHelper.objectTypes)))
                            {
                                oTblName = currentOTblName;
                                if (oTblName.ToString() == "tblContent")
                                    break;
                            }
                            oObjType = (Cms.dbHelper.objectTypes)oTblName;
                        }

                        if (pgid > 0L)
                        {
                            XmlNodeState localNodeState1() { var argoNode1 = base.Instance; var ret = Xml.NodeState(ref argoNode1, "tblContentStructure/RelatedContent"); base.Instance = argoNode1; return ret; }

                            if (localNodeState1() == XmlNodeState.HasContents)
                            {
                                XmlNode oContent;
                                // Dim oDr As SqlDataReader

                                // For Each oContent In MyBase.Instance.SelectNodes("tblContentStructure/RelatedContent/tblContent")
                                string sSql = "Select nContentKey from tblContent c Inner Join tblContentLocation cl on c.nContentKey = cl.nContentId Where cl.nStructId = '" + pgid + "' AND c.cContentName = '" + cFormName + "_RelatedContent'";
                                using (var oDr = moDbHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                                {

                                    // nRContentId = 0
                                    while (oDr.Read())
                                        nRContentId = Convert.ToInt16(oDr[0]);
                                }

                                if (nRContentId > 0)
                                {
                                    oContent = base.Instance.SelectSingleNode("tblContentStructure/RelatedContent");
                                    oContent.InnerXml = moDbHelper.getObjectInstance(oObjType, (long)nRContentId);
                                }
                            }
                        }

                        else
                        {
                            // Set the default language

                        }


                        if (pgid > 0L)
                        {

                            base.Instance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.ContentStructure, pgid);

                            // Set the default language if empty
                            if (myWeb.goLangConfig != null)
                            {
                                if (base.Instance.SelectSingleNode("tblContentStructure/cVersionLang") != null)
                                {
                                    if (string.IsNullOrEmpty(base.Instance.SelectSingleNode("tblContentStructure/cVersionLang").InnerText))
                                    {
                                        base.Instance.SelectSingleNode("tblContentStructure/cVersionLang").InnerText = myWeb.goLangConfig.GetAttribute("code");
                                    }
                                }
                            }
                        }

                        else if (string.IsNullOrEmpty(base.Instance.InnerXml))
                        {
                            base.Instance.InnerXml = "<tblContentStructure><nStructKey/><nStructParId/><cStructForiegnRef/><cStructName/><cStructDescription><DisplayName/><Images><img class=\"thumbnail\"/></Images><Description/></cStructDescription><cUrl/><nStructOrder/><cStructLayout>1_Column</cStructLayout><cVersionLang/><nAuditId/>" + "<nAuditKey/><dPublishDate/><dExpireDate/><dInsertDate/><nInsertDirId/><dUpdateDate/><nUpdateDirId/><nStatus>0</nStatus><cDescription></cDescription></tblContentStructure>";
                        }
                        else if (myWeb.goLangConfig != null)
                        {
                            base.Instance.SelectSingleNode("tblContentStructure/cVersionLang").InnerText = myWeb.goLangConfig.GetAttribute("code");
                        }



                        // Add the page name if passed through
                        if (!string.IsNullOrEmpty(cName))
                        {
                            if (myWeb.moConfig["PageURLFormat"] == "hyphens")
                            {
                                cName = cName.Replace("-", " ");
                            }
                            cProcessInfo = base.Instance.InnerXml;
                            base.Instance.SelectSingleNode("tblContentStructure/cStructName").InnerText = cName;
                        }

                        // disable the status if we are editing the home page

                        if (pgid == Convert.ToInt64(myWeb.moConfig["RootPageId"]))
                        {
                            XmlElement oStatusElmt;
                            oStatusElmt = (XmlElement)base.moXformElmt.SelectSingleNode("descendant-or-self::*[@bind='nStatus' or @ref='nStatus']");
                            if (oStatusElmt != null)
                            {
                                oStatusElmt.SetAttribute("class", oStatusElmt.GetAttribute("class") + " readonly");
                            }
                            XmlElement oStatusBindElmt = (XmlElement)base.moXformElmt.SelectSingleNode("descendant-or-self::bind[@id='nStatus']");
                            if (oStatusBindElmt != null)
                            {
                                oStatusBindElmt.SetAttribute("required", "false()");
                            }
                        }

                        // If the Par Id is empty then populate it
                        if (string.IsNullOrEmpty(cParId))
                        {
                            XmlElement xmlBase = base.Instance;
                            Xml.NodeState(ref xmlBase, "tblContentStructure/nStructParId", (base.goRequest["parId"] == null ? "" : Convert.ToString(base.goRequest["parId"])));
                        }
                        else
                        {
                            var argoNode = base.Instance;
                            Xml.NodeState(ref argoNode, "tblContentStructure/nStructParId", cParId);
                            base.Instance = argoNode;
                        }


                        // Account for the clone node ' TS removed because added in to page xform on a per site basis if required see wanner.
                        // If gbClone Then

                        // ' Check for the instance of Clone
                        // If Tools.Xml.NodeState(MyBase.Instance, "tblContentStructure/nCloneStructId") = Tools.Xml.XmlNodeState.NotInstantiated Then
                        // addElement(MyBase.Instance.SelectSingleNode("tblContentStructure"), "nCloneStructId")
                        // End If

                        // ' Check for the binding of clone
                        // If Tools.Xml.NodeState(MyBase.model, "//bind[contains(@nodeset,'nCloneStructId'])") = Tools.Xml.XmlNodeState.NotInstantiated Then
                        // Dim oGroup As XmlElement = MyBase.moXformElmt.SelectSingleNode("group")
                        // MyBase.addInput(oGroup, "nCloneStructId", True, "Clone Page", "clonepage")
                        // MyBase.addBind("nCloneStructId", "tblContentStructure/nCloneStructId", "false()")
                        // End If
                        // End If

                        cName = base.Instance.SelectSingleNode("tblContentStructure/cStructName").InnerText;
                        if (myWeb.moConfig["PageURLFormat"] == "hyphens")
                        {
                            cName = cName.Replace("-", " ");
                            base.Instance.SelectSingleNode("tblContentStructure/cStructName").InnerText = cName;
                        }


                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                PageValidation();
                            }
                            if (base.valid)
                            {

                                // TS reset system application page values
                                goApp["PageNotFoundId"] = (object)null;
                                goApp["PageAccessDeniedId"] = (object)null;
                                goApp["PageLoginRequiredId"] = (object)null;
                                goApp["PageLoginRequiredId"] = (object)null;

                                // NB Notes: Extract RelatedContent Nodes here - is this old now?

                                if (pgid > 0L)
                                {
                                    moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.ContentStructure, base.Instance);
                                }
                                else
                                {

                                    pgid = moDbHelper.insertStructure(base.Instance);
                                    moDbHelper.ReorderNode(Cms.dbHelper.objectTypes.ContentStructure, pgid, "MoveBottom");

                                    // If the site wants to, by default, restrict new pages to a given group or directory item, then
                                    // read this in from the config and set the permission.
                                    if (Tools.Number.IsNumeric(goConfig["DefaultPagePermissionGroupId"]) & Convert.ToDouble(goConfig["DefaultPagePermissionGroupId"]) > 0d)
                                    {
                                        long nDefaultPagePermDirId = Convert.ToInt64(goConfig["DefaultPagePermissionGroupId"]);
                                        moDbHelper.maintainPermission(pgid, nDefaultPagePermDirId, ((int)Cms.dbHelper.PermissionLevel.View).ToString());
                                    }

                                    // We need to return the page id somehow, so we could update the instance
                                    var argoNode1 = base.Instance;
                                    Xml.NodeState(ref argoNode1, "//nStructKey", pgid.ToString(), "", XmlNodeState.IsEmpty, returnElement: null, returnAsXml: "", returnAsText: "", bCheckTrimmedInnerText: false);
                                    base.Instance = argoNode1;

                                }


                                // Clear the cache
                                if (Cms.gbSiteCacheMode)
                                {
                                    moDbHelper.ExeProcessSqlScalar("DELETE FROM dbo.tblXmlCache");
                                }


                                // NB Notes: Get PgId above then process Related Content
                                XmlNodeState localNodeState2() { var argoNode2 = base.Instance; var ret = Xml.NodeState(ref argoNode2, "tblContentStructure/RelatedContent"); base.Instance = argoNode2; return ret; }

                                if (localNodeState2() == XmlNodeState.HasContents)
                                {
                                    if (pgid > 0L)
                                    {
                                        XmlNode oContent;
                                        // Dim oDr As SqlDataReader

                                        oContent = base.Instance.SelectSingleNode("tblContentStructure/RelatedContent/tblContent");
                                        string sSql = "Select nContentKey from tblContent c Inner Join tblContentLocation cl on c.nContentKey = cl.nContentId Where cl.nStructId = '" + pgid + "' AND c.cContentName = '" + cFormName + "_RelatedContent'";
                                        using (var oDr = moDbHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                                        {


                                            var oInstance = new XmlDocument();
                                            oInstance.AppendChild(oInstance.CreateElement("Instance"));
                                            oInstance.FirstChild.AppendChild(oInstance.ImportNode(oContent, true));

                                            nRContentId = 0;
                                            while (oDr.Read())
                                                nRContentId = Convert.ToInt16(oDr[0]);

                                            if (nRContentId > 0)
                                            {
                                                nRContentId = Convert.ToInt16(moDbHelper.setObjectInstance(oObjType, (XmlElement)oInstance.FirstChild, (long)nRContentId));
                                                moDbHelper.CommitLogToDB(Cms.dbHelper.ActivityType.ContentEdited, myWeb.mnUserId, myWeb.moSession.SessionID, DateTime.Now, nRContentId, (int)pgid, "");
                                                moDbHelper.setContentLocation(pgid, (long)nRContentId);
                                            }
                                            else
                                            {
                                                nRContentId = Convert.ToInt16(moDbHelper.setObjectInstance(oObjType, (XmlElement)oInstance.FirstChild));
                                                moDbHelper.CommitLogToDB(Cms.dbHelper.ActivityType.ContentAdded, myWeb.mnUserId, myWeb.moSession.SessionID, DateTime.Now, nRContentId, (int)pgid, "");
                                                moDbHelper.setContentLocation(pgid, (long)nRContentId);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmEditPage", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                /// <summary>
                /// Page xform validation:
                /// <list>
                ///   <item>Checks for reserved words</item>
                ///   <item>Checks for illegal characters</item>
                /// </list>
                /// </summary>
                /// <remarks>This can be overridden, but should be called by the overriding method.</remarks>
                protected virtual void PageValidation()
                {
                    string cProcessInfo = "";
                    try
                    {

                        cProcessInfo = "Check for reserved words";
                        string[] aReservedDirs = "ewcommon,images,docs,media,css,bin,js,xforms,xsl".Split(',');
                        int i;
                        var loopTo = aReservedDirs.Length - 1;
                        for (i = 0; i <= loopTo; i++)
                        {
                            if ((goRequest["cStructName"] ?? "") == (aReservedDirs[i] ?? ""))
                            {
                                base.valid = false;
                                XmlNode argoNode = (XmlNode)base.RootGroup;
                                base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, "<strong>" + aReservedDirs[i] + "</strong> is a reserved directory name, please use another.");
                            }
                        }

                        // check for illegal charactors within the page name.
                        // update 21-May-09 : taken out - and +  as these can confuse our URLs
                        cProcessInfo = "Check for illegal characters";
                        var oUrlExp = new Regex(@"^[\w\u0020]+$");

                        if (!oUrlExp.IsMatch(goRequest["cStructName"]))
                        {
                            base.valid = false;
                            base.addNote("cStructName", Protean.xForm.noteTypes.Alert, "Page names are used for the URL and only contain alphanumberic characters, underscores and spaces.");
                        }
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "PageValidation", ex, "", cProcessInfo, gbDebug);
                    }
                }

                public XmlElement xFrmCopyPage(long pgid)
                {
                    XmlElement oFrmElmt;
                    XmlElement oSelElmt;
                    string cProcessInfo = "";
                    long nNewPgid;
                    XmlElement oElmt;

                    try
                    {
                        base.NewFrm("CopyPage");
                        // MyBase.submission("EditEage", "admin.aspx?ewCmd=EditPage&pgid=46&xml=x", "post")

                        base.submission("EditEage", "", "post", "form_check(this)");

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "CopyPage", "", "Copy Page");
                        base.addInput(ref oFrmElmt, "nStructParId", true, "ParId", "hidden");
                        XmlElement argoBindParent = null;
                        base.addBind("nStructParId", "tblContentStructure/nStructParId", oBindParent: ref argoBindParent, "true()");

                        oSelElmt = base.addSelect1(ref oFrmElmt, "nCopyType", true, "Copy", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "This page and all its decendants", 1.ToString());
                        base.addOption(ref oSelElmt, "This page only", 0.ToString());
                        XmlElement argoBindParent1 = null;
                        base.addBind("nCopyType", "tblContentStructure/@nCopyType", oBindParent: ref argoBindParent1, "true()");

                        oSelElmt = base.addSelect1(ref oFrmElmt, "nCopyContent", true, "Page Content", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "Create empty pages", 0.ToString());
                        base.addOption(ref oSelElmt, "Same content with multiple locations", 2.ToString());
                        base.addOption(ref oSelElmt, "Same content with multiple primary locations", 3.ToString());
                        base.addOption(ref oSelElmt, "Create copies of the content", 1.ToString());
                        base.addOption(ref oSelElmt, "Force copies of the content", 4.ToString());
                        XmlElement argoBindParent2 = null;
                        base.addBind("nCopyContent", "tblContentStructure/@nCopyContent", oBindParent: ref argoBindParent2, "true()");

                        base.addInput(ref oFrmElmt, "cStructName", true, "Page Name");
                        XmlElement argoBindParent3 = null;
                        base.addBind("cStructName", "tblContentStructure/cStructName", oBindParent: ref argoBindParent3, "true()");

                        base.addInput(ref oFrmElmt, "cDisplayName", true, "Display Name");
                        XmlElement argoBindParent4 = null;
                        base.addBind("cDisplayName", "tblContentStructure/cStructDescription/DisplayName", oBindParent: ref argoBindParent4, "false()");

                        string argsClass = "xhtml";
                        int argnRows = 10;
                        int argnCols = 0;
                        base.addTextArea(ref oFrmElmt, "cStructDescription", true, "Description", ref argsClass, ref argnRows, nCols: ref argnCols);
                        XmlElement argoBindParent5 = null;
                        base.addBind("cStructDescription", "tblContentStructure/cStructDescription/Description", oBindParent: ref argoBindParent5, "false()");

                        base.addInput(ref oFrmElmt, "cUrl", true, "URL");
                        XmlElement argoBindParent6 = null;
                        base.addBind("cUrl", "tblContentStructure/cUrl", oBindParent: ref argoBindParent6, "false()");

                        base.addInput(ref oFrmElmt, "dPublishDate", true, "Publish Date", "calendar");
                        XmlElement argoBindParent7 = null;
                        base.addBind("dPublishDate", "tblContentStructure/dPublishDate", oBindParent: ref argoBindParent7, "false()");

                        base.addInput(ref oFrmElmt, "dExpireDate", true, "Expire Date", "calendar");
                        XmlElement argoBindParent8 = null;
                        base.addBind("dExpireDate", "tblContentStructure/dExpireDate", oBindParent: ref argoBindParent8, "false()");

                        oSelElmt = base.addSelect1(ref oFrmElmt, "nStatus", true, "Status", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "Live", 1.ToString());
                        base.addOption(ref oSelElmt, "Hidden", 0.ToString());
                        XmlElement argoBindParent9 = null;
                        base.addBind("nStatus", "tblContentStructure/nStatus", oBindParent: ref argoBindParent9, "true()");

                        base.addInput(ref oFrmElmt, "cDescription", true, "Change Notes");
                        XmlElement argoBindParent10 = null;
                        base.addBind("cDescription", "tblContentStructure/cDescription", oBindParent: ref argoBindParent10, "false()");

                        //XmlNode argoNode = oFrmElmt;
                        base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Hint, "This page will be copied without any permissions and will inherit the permissions from the new locations ancestors");
                        //oFrmElmt = (XmlElement)argoNode;
                        base.addSubmit(ref oFrmElmt, "", "Save Page");


                        base.Instance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.ContentStructure, pgid);

                        // delete old page id so new page is added not updated
                        base.Instance.SelectSingleNode("tblContentStructure/nStructKey").InnerText = "";
                        base.Instance.SelectSingleNode("tblContentStructure/cStructName").InnerText = "Copy of " + base.Instance.SelectSingleNode("tblContentStructure/cStructName").InnerText;

                        oElmt = (XmlElement)base.Instance.SelectSingleNode("tblContentStructure");
                        oElmt.SetAttribute("copyPgid", pgid.ToString());
                        oElmt.SetAttribute("nCopyType", "0");
                        oElmt.SetAttribute("nCopyContent", "0");

                        if (base.Instance.SelectSingleNode("tblContentStructure/cStructDescription/DisplayName") is null)
                        {
                            string sDescText = base.Instance.SelectSingleNode("tblContentStructure/cStructDescription").InnerXml;
                            // make sure the description contains our xml  

                            oElmt = moPageXML.CreateElement("DisplayName");
                            base.Instance.SelectSingleNode("tblContentStructure/cStructDescription").AppendChild(oElmt);

                            var oElmt2 = moPageXML.CreateElement("Description");
                            oElmt2.InnerXml = sDescText;
                            base.Instance.SelectSingleNode("tblContentStructure/cStructDescription").AppendChild(oElmt2);

                        }

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                string[] aReservedDirs = "ewcommon,images,docs,media,css,bin,js,xforms,xsl".Split(',');
                                int i;
                                var loopTo = aReservedDirs.Length - 1;
                                for (i = 0; i <= loopTo; i++)
                                {
                                    if ((goRequest["cStructName"] ?? "") == (aReservedDirs[i] ?? ""))
                                    {
                                        base.valid = false;
                                        //XmlNode argoNode1 = oFrmElmt;
                                        base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "<strong>" + aReservedDirs[i] + "</strong> is a reserved directory name, please use another.");
                                        // oFrmElmt = (XmlElement)argoNode1;
                                    }
                                }

                                // check for illegal charactors within the page name.
                                var oUrlExp = new Regex(@"^[\w\-\u0020\+]+$");

                                if (!oUrlExp.IsMatch(goRequest["cStructName"]))
                                {
                                    base.valid = false;
                                    base.addNote("cStructName", Protean.xForm.noteTypes.Alert, "Page names are used for the URL and only contain Alphanumberic, underscores, hyphens and spaces.");
                                }
                            }

                            if (base.valid)
                            {
                                // add new
                                var dPublishDate = DateTime.Parse("0001-01-01");
                                if (!string.IsNullOrEmpty(base.goRequest["dPublishDate"]))
                                    dPublishDate = Convert.ToDateTime(base.goRequest["dPublishDate"]);
                                var dExpireDate = DateTime.Parse("0001-01-01");
                                if (!string.IsNullOrEmpty(base.goRequest["dExpireDate"]))
                                    dExpireDate = Convert.ToDateTime(base.goRequest["dExpireDate"]);

                                nNewPgid = moDbHelper.insertStructure(Convert.ToInt64(base.goRequest["nStructParId"]), base.goRequest["nStructForeignRef"], base.goRequest["cStructName"], base.Instance.SelectSingleNode("tblContentStructure/cStructDescription").InnerXml, base.Instance.SelectSingleNode("tblContentStructure/cStructLayout").InnerXml, Convert.ToInt64(base.goRequest["nStatus"]), dPublishDate, dExpireDate, base.goRequest["cDescription"]);

                                moDbHelper.ReorderNode(Cms.dbHelper.objectTypes.ContentStructure, nNewPgid, "MoveBottom");

                                // Copy content and children
                                // Dim nCopyType As Boolean = False
                                // If Not IsNumeric(goRequest("nCopyType")) Then
                                // nCopyType = False
                                // ElseIf goRequest("nCopyType") = 1 Then
                                // nCopyType = True
                                // Else
                                // nCopyType = False
                                // End If
                                moDbHelper.copyPageContent(pgid, nNewPgid, Convert.ToBoolean(goRequest["nCopyType"]), (Cms.dbHelper.CopyContentType)Convert.ToInt16(goRequest["nCopyContent"]));
                            }

                        }

                        base.addValues();
                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmEditPage", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmCopyPageVersion(long contentPageId, long parentPageId)
                {
                    XmlElement oFrmElmt;
                    XmlElement oSelElmt;
                    string cProcessInfo = "";
                    long nNewPgid;
                    XmlElement oElmt;

                    try
                    {
                        base.NewFrm("NewPageVersion");

                        base.submission("EditEage", "", "post", "form_check(this)");

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "PageVersion", "2col", "New Page Version");
                        // MyBase.addInput(oFrmElmt, "nStructParId", True, "ParId", "hidden")
                        // MyBase.addBind("nStructParId", "tblContentStructure/nStructParId", "true()")

                        var oGrp1 = base.addGroup(ref oFrmElmt, "Group1", "", "");
                        var oGrp2 = base.addGroup(ref oGrp1, "PageSettings", "", "Page Settings");

                        base.addInput(ref oGrp2, "dPublishDate", true, "Publish Date", "calendar");
                        XmlElement argoBindParent = null;
                        base.addBind("dPublishDate", "tblContentStructure/dPublishDate", oBindParent: ref argoBindParent, "false()");

                        base.addInput(ref oGrp2, "dExpireDate", true, "Expire Date", "calendar");
                        XmlElement argoBindParent1 = null;
                        base.addBind("dExpireDate", "tblContentStructure/dExpireDate", oBindParent: ref argoBindParent1, "false()");

                        oSelElmt = base.addSelect1(ref oGrp2, "nStatus", true, "Status", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "Live", 1.ToString());
                        base.addOption(ref oSelElmt, "Hidden", 0.ToString());
                        XmlElement argoBindParent2 = null;
                        base.addBind("nStatus", "tblContentStructure/nStatus", oBindParent: ref argoBindParent2, "true()");

                        base.addInput(ref oGrp2, "cDescription", true, "Change Notes");
                        XmlElement argoBindParent3 = null;
                        base.addBind("cDescription", "tblContentStructure/cDescription", oBindParent: ref argoBindParent3, "false()");

                        var oGrp3 = base.addGroup(ref oFrmElmt, "Group3", "", "");
                        var oGrp4 = base.addGroup(ref oGrp3, "CopySettings", "", "Copy Settings");

                        base.addInput(ref oGrp4, "cVersionDescription", true, "Version Description", "long");
                        XmlElement argoBindParent4 = null;
                        base.addBind("cVersionDescription", "tblContentStructure/cVersionDescription", oBindParent: ref argoBindParent4, "true()");

                        oSelElmt = base.addSelect1(ref oGrp4, "nVersionType", true, "Type", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "Personalisation", 1.ToString());
                        // MyBase.addOption(oSelElmt, "Working Copy", 2)
                        if (myWeb.goLangConfig != null)
                        {
                            base.addOption(ref oSelElmt, "Language Version", 3.ToString());
                        }
                        // MyBase.addOption(oSelElmt, "Split Test", 4)
                        XmlElement argoBindParent5 = null;
                        base.addBind("nVersionType", "tblContentStructure/nVersionType", oBindParent: ref argoBindParent5, "true()");

                        if (myWeb.goLangConfig != null)
                        {
                            oSelElmt = base.addSelect1(ref oGrp4, "cVersionLang", true, "Language", "", Protean.xForm.ApperanceTypes.Full);
                            base.addOption(ref oSelElmt, myWeb.goLangConfig.GetAttribute("default"), myWeb.goLangConfig.GetAttribute("code"));
                            foreach (XmlElement langNode in myWeb.goLangConfig.SelectNodes("Language"))
                                base.addOption(ref oSelElmt, langNode.GetAttribute("systemName"), langNode.GetAttribute("code"));
                            XmlElement argoBindParent6 = null;
                            base.addBind("cVersionLang", "tblContentStructure/cVersionLang", oBindParent: ref argoBindParent6, "tblContentStructure/nVersionType='3'");
                        }

                        oSelElmt = base.addSelect1(ref oGrp4, "nCopyContent", true, "Page Content", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "Create empty pages", 0.ToString());
                        base.addOption(ref oSelElmt, "Same content with multiple locations", 2.ToString());
                        // MyBase.addOption(oSelElmt, "Same content with multiple primary locations", 3)
                        base.addOption(ref oSelElmt, "Create copies of the content", 1.ToString());
                        XmlElement argoBindParent7 = null;
                        base.addBind("nCopyContent", "tblContentStructure/@nCopyContent", oBindParent: ref argoBindParent7, "true()");

                        var oGrp6 = base.addGroup(ref oGrp3, "PageDetails", "", "Page Details");

                        base.addInput(ref oGrp6, "cStructName", true, "Page Name", "long");
                        XmlElement argoBindParent8 = null;
                        base.addBind("cStructName", "tblContentStructure/cStructName", oBindParent: ref argoBindParent8, "true()");

                        base.addInput(ref oGrp6, "cDisplayName", true, "Display Name", "long");
                        XmlElement argoBindParent9 = null;
                        base.addBind("cDisplayName", "tblContentStructure/cStructDescription/DisplayName", oBindParent: ref argoBindParent9, "false()");

                        string argsClass = "xhtml";
                        int argnRows = 10;
                        int argnCols = 0;
                        base.addTextArea(ref oGrp6, "cStructDescription", true, "Description", ref argsClass, ref argnRows, nCols: ref argnCols);
                        XmlElement argoBindParent10 = null;
                        base.addBind("cStructDescription", "tblContentStructure/cStructDescription/Description", oBindParent: ref argoBindParent10, "false()");

                        base.addInput(ref oGrp6, "cUrl", true, "URL");
                        XmlElement argoBindParent11 = null;
                        base.addBind("cUrl", "tblContentStructure/cUrl", oBindParent: ref argoBindParent11, "false()");

                        var oGrp5 = base.addGroup(ref base.moXformElmt, "", "", "");

                        //XmlNode argoNode = (XmlNode)oGrp5;
                        base.addNote(ref oGrp5, Protean.xForm.noteTypes.Hint, "This page will be copied without any permissions and will inherit the permissions from the new locations ancestors");
                        //oGrp5 = (XmlElement)argoNode;
                        base.addSubmit(ref oGrp5, "", "Save Page");


                        base.Instance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.ContentStructure, contentPageId);

                        // delete old page id so new page is added not updated
                        base.Instance.SelectSingleNode("tblContentStructure/nStructKey").InnerText = "";
                        base.Instance.SelectSingleNode("tblContentStructure/cVersionDescription").InnerText = base.Instance.SelectSingleNode("tblContentStructure/cStructName").InnerText + " New Version";

                        // set a default for version type
                        base.Instance.SelectSingleNode("tblContentStructure/nVersionType").InnerText = "1";

                        // set a default for lang type
                        if (myWeb.goLangConfig != null)
                        {
                            base.Instance.SelectSingleNode("tblContentStructure/cVersionLang").InnerText = myWeb.goLangConfig.GetAttribute("code");
                        }

                        oElmt = (XmlElement)base.Instance.SelectSingleNode("tblContentStructure");
                        oElmt.SetAttribute("copyPgid", contentPageId.ToString());
                        oElmt.SetAttribute("nCopyType", "0");
                        oElmt.SetAttribute("nCopyContent", "0");

                        if (base.Instance.SelectSingleNode("tblContentStructure/cStructDescription/DisplayName") is null)
                        {
                            string sDescText = base.Instance.SelectSingleNode("tblContentStructure/cStructDescription").InnerXml;
                            // make sure the description contains our xml  

                            oElmt = moPageXML.CreateElement("DisplayName");
                            base.Instance.SelectSingleNode("tblContentStructure/cStructDescription").AppendChild(oElmt);

                            var oElmt2 = moPageXML.CreateElement("Description");
                            oElmt2.InnerXml = sDescText;
                            base.Instance.SelectSingleNode("tblContentStructure/cStructDescription").AppendChild(oElmt2);

                        }

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                string[] aReservedDirs = "ewcommon,images,docs,media,css,bin,js,xforms,xsl".Split(',');
                                int i;
                                var loopTo = aReservedDirs.Length - 1;
                                for (i = 0; i <= loopTo; i++)
                                {
                                    if ((goRequest["cStructName"] ?? "") == (aReservedDirs[i] ?? ""))
                                    {
                                        base.valid = false;
                                        //XmlNode argoNode1 = oFrmElmt;
                                        base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "<strong>" + aReservedDirs[i] + "</strong> is a reserved directory name, please use another.");
                                        //oFrmElmt = (XmlElement)argoNode1;
                                    }
                                }

                                // check for illegal charactors within the page name.
                                var oUrlExp = new Regex(@"^[\w\-\u0020\+]+$");

                                if (!oUrlExp.IsMatch(goRequest["cStructName"]))
                                {
                                    base.valid = false;
                                    base.addNote("cStructName", Protean.xForm.noteTypes.Alert, "Page names are used for the URL and only contain Alphanumberic, underscores, hyphens and spaces.");
                                }
                            }

                            if (base.valid)
                            {
                                // add new
                                var dPublishDate = DateTime.Parse("0001-01-01");
                                if (!string.IsNullOrEmpty(base.goRequest["dPublishDate"]))
                                    dPublishDate = Convert.ToDateTime(base.goRequest["dPublishDate"]);
                                var dExpireDate = DateTime.Parse("0001-01-01");
                                if (!string.IsNullOrEmpty(base.goRequest["dExpireDate"]))
                                    dExpireDate = Convert.ToDateTime(base.goRequest["dExpireDate"]);

                                nNewPgid = moDbHelper.insertPageVersion(Convert.ToInt64(base.goRequest["nStructParId"]), base.goRequest["nStructForeignRef"], base.goRequest["cStructName"], base.Instance.SelectSingleNode("tblContentStructure/cStructDescription").InnerXml, base.Instance.SelectSingleNode("tblContentStructure/cStructLayout").InnerXml, Convert.ToInt64(base.goRequest["nStatus"]), dPublishDate, dExpireDate, base.goRequest["cDescription"], nVersionParId: Convert.ToInt64(base.goRequest["vParId"]), cVersionLang: base.goRequest["cVersionLang"], cVersionDescription: base.goRequest["cVersionDescription"], nVersionType: (Cms.dbHelper.PageVersionType)Convert.ToInt16(base.goRequest["nVersionType"]));

                                moDbHelper.ReorderNode(Cms.dbHelper.objectTypes.ContentStructure, nNewPgid, "MoveBottom");

                                // Copy content and children
                                // Dim nCopyType As Boolean = False
                                // If Not IsNumeric(goRequest("nCopyType")) Then
                                // nCopyType = False
                                // ElseIf goRequest("nCopyType") = 1 Then
                                // nCopyType = True
                                // Else
                                // nCopyType = False
                                // End If
                                moDbHelper.copyPageContent(contentPageId, nNewPgid, Convert.ToBoolean(0), (Cms.dbHelper.CopyContentType)Convert.ToInt16(goRequest["nCopyContent"]));
                            }

                        }

                        base.addValues();
                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmCopyPageVersion", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmEditPageLayout(long pgid = 0L)
                {
                    XmlElement oFrmElmt;
                    XmlElement oSelElmt;
                    XmlElement oChoices;
                    // Dim oChoicesElmt As XmlElement
                    XmlElement oItem;
                    XmlElement oOptElmt;
                    XmlElement oDescElmt;
                    string sImgPath = "";

                    string cProcessInfo = "";
                    var oXformDoc = new XmlDocument();
                    try
                    {

                        base.NewFrm("EditPageLayout");
                        base.submission("EditEage", "", "post", "form_check(this)");

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "EditPage", "", "Select Page Layout");
                        base.addInput(ref oFrmElmt, "nStructParId", true, "ParId", "hidden");
                        XmlElement argoBindParent = null;
                        base.addBind("nStructParId", "tblContentStructure/nStructParId", oBindParent: ref argoBindParent, "true()");

                        // MyBase.addNote(oFrmElmt, xForm.noteTypes.Hint, "Click the image to select page layout")

                        oSelElmt = base.addSelect1(ref oFrmElmt, "cStructLayout", true, "", "PickByImage", Protean.xForm.ApperanceTypes.Full);
                        XmlElement argoBindParent1 = null;
                        base.addBind("cStructLayout", "tblContentStructure/cStructLayout", oBindParent: ref argoBindParent1, "true()");
                        if (goConfig["cssFramework"] != "bs5")
                        {
                            try
                            {
                                // if this file exists then add the bespoke templates
                                oXformDoc.Load(goServer.MapPath(myWeb.moConfig["ProjectPath"] + "/xsl") + "/LayoutManifest.xml");
                                sImgPath = oXformDoc.DocumentElement.GetAttribute("imgPath");

                                foreach (XmlElement currentOChoices in oXformDoc.SelectNodes("/PageLayouts/LayoutGroup"))
                                {
                                    oChoices = currentOChoices;
                                    var oChoicesElmt = base.addChoices(ref oSelElmt, oChoices.GetAttribute("name"));
                                    foreach (XmlElement currentOItem in oChoices.SelectNodes("Layout"))
                                    {
                                        oItem = currentOItem;
                                        oOptElmt = base.addOption(ref oChoicesElmt, oItem.GetAttribute("name").Replace("_", " "), oItem.GetAttribute("name"));
                                        // lets add an image tag
                                        oDescElmt = moPageXML.CreateElement("img");
                                        oDescElmt.SetAttribute("src", sImgPath + "/" + oItem.GetAttribute("name") + ".gif");
                                        oOptElmt.AppendChild(oDescElmt);

                                        // lets insert a description html tag
                                        if (!string.IsNullOrEmpty(oItem.InnerXml))
                                        {
                                            oDescElmt = moPageXML.CreateElement("div");
                                            oDescElmt.SetAttribute("class", "description");
                                            oDescElmt.InnerXml = oItem.InnerXml;
                                            oOptElmt.AppendChild(oDescElmt);
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                // do nothing
                            }
                        }

                        // Lets load in the available common templates from XML file
                        try
                        {
                            if (goConfig["cssFramework"] == "bs5")
                            {
                                oXformDoc = GetSiteManifest();
                            }
                            else
                            {
                                oXformDoc.Load(goServer.MapPath("/" + Cms.gcProjectPath + "ewcommon/xsl/pageLayouts") + "/LayoutManifest.xml");
                            }
                            sImgPath = oXformDoc.DocumentElement.GetAttribute("imgPath");
                        }
                        catch (Exception ex)
                        {
                            //XmlNode argoNode = oFrmElmt;
                            base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "/" + Cms.gcProjectPath + "ewcommon/xsl/pageLayouts/LayoutManifest.xml could not be found. - " + ex.Message);
                            //oFrmElmt = (XmlElement)argoNode;
                        }


                        foreach (XmlElement currentOChoices1 in oXformDoc.SelectNodes("/PageLayouts/LayoutGroup"))
                        {
                            oChoices = currentOChoices1;
                            var oChoicesElmt = base.addChoices(ref oSelElmt, oChoices.GetAttribute("name"));
                            foreach (XmlElement currentOItem1 in oChoices.SelectNodes("Layout"))
                            {
                                oItem = currentOItem1;
                                oOptElmt = base.addOption(ref oChoicesElmt, oItem.GetAttribute("name").Replace("_", " "), oItem.GetAttribute("name"));

                                // lets add an image tag
                                oDescElmt = moPageXML.CreateElement("img");
                                oDescElmt.SetAttribute("src", sImgPath + "/" + oItem.GetAttribute("name") + ".gif");
                                oOptElmt.AppendChild(oDescElmt);

                                // lets insert a description html tag
                                if (!string.IsNullOrEmpty(oItem.InnerXml))
                                {
                                    oDescElmt = moPageXML.CreateElement("div");
                                    oDescElmt.SetAttribute("class", "description");
                                    oDescElmt.InnerXml = oItem.InnerXml;
                                    oOptElmt.AppendChild(oDescElmt);
                                }
                            }
                        }

                        NameValueCollection oConfig = (NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");
                        if (oConfig["cart"] == "on" & goConfig["cssFramework"] != "bs5")
                        {
                            try
                            {
                                oXformDoc.Load(goServer.MapPath("/" + Cms.gcProjectPath + "ewcommon/xsl/cart") + "/LayoutManifest.xml");
                                sImgPath = oXformDoc.DocumentElement.GetAttribute("imgPath");
                            }
                            catch (Exception ex)
                            {
                                //XmlNode argoNode1 = oFrmElmt;
                                base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "/" + Cms.gcProjectPath + "ewcommon/xsl/cart/LayoutManifest.xml could not be found. - " + ex.Message);
                                //oFrmElmt = (XmlElement)argoNode1;
                            }

                            foreach (XmlElement currentOChoices2 in oXformDoc.SelectNodes("/PageLayouts/LayoutGroup"))
                            {
                                oChoices = currentOChoices2;
                                var oChoicesElmt = base.addChoices(ref oSelElmt, oChoices.GetAttribute("name"));
                                foreach (XmlElement currentOItem2 in oChoices.SelectNodes("Layout"))
                                {
                                    oItem = currentOItem2;
                                    oOptElmt = base.addOption(ref oChoicesElmt, oItem.GetAttribute("name").Replace("_", " "), oItem.GetAttribute("name"));

                                    // lets add an image tag
                                    oDescElmt = moPageXML.CreateElement("img");
                                    oDescElmt.SetAttribute("src", sImgPath + "/" + oItem.GetAttribute("name") + ".gif");
                                    oOptElmt.AppendChild(oDescElmt);

                                    // lets insert a description html tag
                                    if (!string.IsNullOrEmpty(oItem.InnerXml))
                                    {
                                        oDescElmt = moPageXML.CreateElement("div");
                                        oDescElmt.SetAttribute("class", "description");
                                        oDescElmt.InnerXml = oItem.InnerXml;
                                        oOptElmt.AppendChild(oDescElmt);
                                    }
                                }
                            }
                        }

                        if (oConfig["membership"] == "on" & goConfig["cssFramework"] != "bs5")
                        {
                            try
                            {
                                oXformDoc.Load(goServer.MapPath("/" + Cms.gcProjectPath + "ewcommon/xsl/membership") + "/LayoutManifest.xml");
                                sImgPath = oXformDoc.DocumentElement.GetAttribute("imgPath");
                            }
                            catch (Exception ex)
                            {
                                //XmlNode argoNode2 = oFrmElmt;
                                base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "/" + Cms.gcProjectPath + "ewcommon/xsl/membership/LayoutManifest.xml could not be found. - " + ex.Message);
                                //oFrmElmt = (XmlElement)argoNode2;
                            }

                            foreach (XmlElement currentOChoices3 in oXformDoc.SelectNodes("/PageLayouts/LayoutGroup"))
                            {
                                oChoices = currentOChoices3;
                                var oChoicesElmt = base.addChoices(ref oSelElmt, oChoices.GetAttribute("name"));
                                foreach (XmlElement currentOItem3 in oChoices.SelectNodes("Layout"))
                                {
                                    oItem = currentOItem3;
                                    oOptElmt = base.addOption(ref oChoicesElmt, oItem.GetAttribute("name").Replace("_", " "), oItem.GetAttribute("name"));

                                    // lets add an image tag
                                    oDescElmt = moPageXML.CreateElement("img");
                                    oDescElmt.SetAttribute("src", sImgPath + "/" + oItem.GetAttribute("name") + ".gif");
                                    oOptElmt.AppendChild(oDescElmt);

                                    // lets insert a description html tag
                                    if (!string.IsNullOrEmpty(oItem.InnerXml))
                                    {
                                        oDescElmt = moPageXML.CreateElement("div");
                                        oDescElmt.SetAttribute("class", "description");
                                        oDescElmt.InnerXml = oItem.InnerXml;
                                        oOptElmt.AppendChild(oDescElmt);
                                    }
                                }
                            }
                        }

                        if (pgid > 0L)
                        {
                            base.Instance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.ContentStructure, pgid);
                        }
                        else
                        {
                            //XmlNode argoNode3 = oFrmElmt;
                            base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "No page identified");
                            //oFrmElmt = (XmlElement)argoNode3;
                        }

                        if (base.isSubmitted() | !string.IsNullOrEmpty(goRequest.Form["ewsubmit.x"]) | !string.IsNullOrEmpty(goRequest.Form["cStructLayout"]))
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                if (pgid > 0L)
                                {
                                    moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.ContentStructure, base.Instance);
                                }
                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "addInput", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmEditMailLayout(long pgid = 0L)
                {
                    XmlElement oFrmElmt;
                    XmlElement oSelElmt;
                    XmlElement oChoices;
                    // Dim oChoicesElmt As XmlElement
                    XmlElement oItem;
                    XmlElement oOptElmt;
                    XmlElement oDescElmt;
                    string sImgPath = "";

                    string cProcessInfo = "";
                    var oXformDoc = new XmlDocument();
                    try
                    {

                        base.NewFrm("EditPageLayout");
                        base.submission("EditEage", "", "post", "form_check(this)");

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "EditPage", "", "Select Page Layout");
                        base.addInput(ref oFrmElmt, "nStructParId", true, "ParId", "hidden");
                        XmlElement argoBindParent = null;
                        base.addBind("nStructParId", "tblContentStructure/nStructParId", oBindParent: ref argoBindParent, "true()");

                        // MyBase.addNote(oFrmElmt, xForm.noteTypes.Hint, "Click the image to select page layout")

                        oSelElmt = base.addSelect1(ref oFrmElmt, "cStructLayout", true, "", "PickByImage", Protean.xForm.ApperanceTypes.Full);
                        XmlElement argoBindParent1 = null;
                        base.addBind("cStructLayout", "tblContentStructure/cStructLayout", oBindParent: ref argoBindParent1, "true()");

                        try
                        {
                            // if this file exists then add the bespoke templates
                            oXformDoc.Load(goServer.MapPath(goConfig["ProjectPath"] + "/xsl/Mailer/") + "/LayoutManifest.xml");
                            sImgPath = oXformDoc.DocumentElement.GetAttribute("imgPath");

                            foreach (XmlElement currentOChoices in oXformDoc.SelectNodes("/PageLayouts/LayoutGroup"))
                            {
                                oChoices = currentOChoices;
                                var oChoicesElmt = base.addChoices(ref oSelElmt, oChoices.GetAttribute("name"));
                                foreach (XmlElement currentOItem in oChoices.SelectNodes("Layout"))
                                {
                                    oItem = currentOItem;
                                    oOptElmt = base.addOption(ref oChoicesElmt, oItem.GetAttribute("name").Replace("_", " "), oItem.GetAttribute("name"));
                                    // lets add an image tag
                                    oDescElmt = moPageXML.CreateElement("img");
                                    oDescElmt.SetAttribute("src", sImgPath + "/" + oItem.GetAttribute("name") + ".gif");
                                    oOptElmt.AppendChild(oDescElmt);

                                    // lets insert a description html tag
                                    if (!string.IsNullOrEmpty(oItem.InnerXml))
                                    {
                                        oDescElmt = moPageXML.CreateElement("div");
                                        oDescElmt.SetAttribute("class", "description");
                                        oDescElmt.InnerXml = oItem.InnerXml;
                                        oOptElmt.AppendChild(oDescElmt);
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // do nothing
                        }



                        // Lets load in the available common templates from XML file
                        try
                        {
                            oXformDoc.Load(goServer.MapPath("/" + Cms.gcProjectPath + "ewcommon/xsl/mailer") + "/LayoutManifest.xml");
                            sImgPath = oXformDoc.DocumentElement.GetAttribute("imgPath");
                        }
                        catch (Exception ex)
                        {
                            //XmlNode argoNode = oFrmElmt;
                            base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "/" + Cms.gcProjectPath + "ewcommon/xsl/mailer/LayoutManifest.xml could not be found. - " + ex.Message);
                            //oFrmElmt = (XmlElement)argoNode;
                        }

                        foreach (XmlElement currentOChoices1 in oXformDoc.SelectNodes("/PageLayouts/LayoutGroup"))
                        {
                            oChoices = currentOChoices1;
                            var oChoicesElmt = base.addChoices(ref oSelElmt, oChoices.GetAttribute("name"));
                            foreach (XmlElement currentOItem1 in oChoices.SelectNodes("Layout"))
                            {
                                oItem = currentOItem1;
                                oOptElmt = base.addOption(ref oChoicesElmt, oItem.GetAttribute("name").Replace("_", " "), oItem.GetAttribute("name"));

                                // lets add an image tag
                                oDescElmt = moPageXML.CreateElement("img");
                                oDescElmt.SetAttribute("src", sImgPath + "/" + oItem.GetAttribute("name") + ".gif");
                                oOptElmt.AppendChild(oDescElmt);

                                // lets insert a description html tag
                                if (!string.IsNullOrEmpty(oItem.InnerXml))
                                {
                                    oDescElmt = moPageXML.CreateElement("div");
                                    oDescElmt.SetAttribute("class", "description");
                                    oDescElmt.InnerXml = oItem.InnerXml;
                                    oOptElmt.AppendChild(oDescElmt);
                                }
                            }
                        }


                        // MyBase.addSubmit(oFrmElmt, "", "Save Page")

                        if (pgid > 0L)
                        {
                            base.Instance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.ContentStructure, pgid);
                        }
                        else
                        {
                            //XmlNode argoNode1 = oFrmElmt;
                            base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "No page identified");
                            //oFrmElmt = (XmlElement)argoNode1;
                        }

                        if (base.isSubmitted() | !string.IsNullOrEmpty(goRequest.Form["ewsubmit.x"]) | !string.IsNullOrEmpty(goRequest.Form["cStructLayout"]))
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                if (pgid > 0L)
                                {
                                    moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.ContentStructure, base.Instance);
                                }
                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "addInput", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }


                public XmlElement xFrmDeletePage(long pgid)
                {

                    XmlElement oFrmElmt;
                    string sContentName;
                    string sContentSchemaName = "";

                    string cProcessInfo = "";

                    try
                    {
                        // load the xform to be edited
                        moDbHelper.moPageXml = moPageXML;

                        sContentName = moDbHelper.getNameByKey(Cms.dbHelper.objectTypes.ContentStructure, pgid);

                        base.NewFrm("DeletePage");

                        base.submission("DeleteContent", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "DeletePg", "", "Delete Page");

                        //XmlNode argoNode = oFrmElmt;
                        base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "<h4>Are you sure you want to delete this page - \"" + encodeAllHTML(sContentName) + "\"</h4><br/><br/>By deleting this page you will also delete <strong>ALL</strong> the child pages beneath <strong>ARE YOU SURE</strong> !", false, "alert-danger");
                        //oFrmElmt = (XmlElement)argoNode;

                        base.addSubmit(ref oFrmElmt, "", "Delete Page" + sContentSchemaName, sClass: "btn-danger principle", sIcon: "fa-trash");

                        base.Instance.InnerXml = "<delete/>";

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {

                                // remove the relevent content information
                                moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.ContentStructure, pgid);
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

            }
        }
    }
}