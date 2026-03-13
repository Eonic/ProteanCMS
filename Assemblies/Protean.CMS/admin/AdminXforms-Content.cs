// ***********************************************************************
// $Library:     protean.cms.adminXforms
// $Revision:    3.1  
// $Date:        2006-03-02
// $Author:      Trevor Spink (trevor@eonic.com)
// &Website:     eonic.com
// &Licence:     Apache-2.0 license
// $Copyright:   Copyright (c) 2002 - 2022 Eonic Digital LLP.
// ***********************************************************************


//using Microsoft.VisualBasic;
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

                public XmlElement xFrmAddModule(long pgid, string position)
                {
                    XmlElement oFrmElmt;
                    XmlElement oSelElmt;
                    //string sImgPath = "";

                    string cProcessInfo = "";
                    var oXformDoc = new XmlDocument();
                    try
                    {

                        if (!string.IsNullOrEmpty(moRequest["cModuleBox"]) | !string.IsNullOrEmpty(moRequest["cModuleType"]))
                        {
                            // case for when the content form is being submitted
                            if (goConfig["cssFramework"] == "bs5")
                            {
                                string ModulePath = GetModuleFormPath(moRequest["cModuleType"]);
                                long argnReturnId = 0;
                                string argzcReturnSchema = "";
                                string argAlternateFormName = "";
                                xFrmEditContent(0L, ModulePath, pgid, moRequest["cPosition"], false, nReturnId: argnReturnId, zcReturnSchema:  argzcReturnSchema, AlternateFormName:  argAlternateFormName);
                            }
                            else
                            {
                                long argnReturnId1 = 0;
                                string argzcReturnSchema1 = "";
                                string argAlternateFormName1 = "";
                                xFrmEditContent(0L, "Module/" + moRequest["cModuleType"], pgid, moRequest["cPosition"], false, nReturnId: argnReturnId1, zcReturnSchema:  argzcReturnSchema1, AlternateFormName:  argAlternateFormName1);
                            }


                            return base.moXformElmt;
                        }
                        else
                        {
                            base.NewFrm("EditPageLayout");
                            base.submission("AddModule", "", "post", "form_check(this)");
                            base.Instance.InnerXml = "<Module position=\"" + position + "\"></Module>";

                            oFrmElmt = base.addGroup(ref base.moXformElmt, "Add Module", "", "Select Module Type");
                            base.addInput(ref oFrmElmt, "nStructParId", true, "ParId", "hidden");
                            base.addInput(ref oFrmElmt, "cPosition", true, "Position", "hidden");
                            XmlElement argoBindParent = null;
                            base.addBind("cPosition", "Module/@position", oBindParent: ref argoBindParent, "true()");

                            oSelElmt = base.addSelect1(ref oFrmElmt, "cModuleType", true, "", "PickByImage", Protean.xForm.ApperanceTypes.Full);

                            GetModuleOptions(ref oSelElmt);

                            var submitted = base.isSubmitted();
                            var ewsubmit = !string.IsNullOrEmpty(goRequest.Form["ewsubmit.x"]);
                            var cModuletype = !string.IsNullOrEmpty(goRequest.Form["cModuleType"]);

                            if (base.isSubmitted() | !string.IsNullOrEmpty(goRequest.Form["ewsubmit.x"]) | !string.IsNullOrEmpty(goRequest.Form["cModuleType"]))
                            {
                                base.updateInstanceFromRequest();
                                base.validate();
                                if (base.valid)
                                {
                                    // Do nothing
                                    // or redirect to content form
                                    // 
                                    if (goConfig["cssFramework"] == "bs5")
                                    {
                                        string ModulePath = GetModuleFormPath(moRequest["cModuleType"]);
                                        long argnReturnId2 = 0;
                                        string argzcReturnSchema2 = "";
                                        string argAlternateFormName2 = "";
                                        xFrmEditContent(0L, ModulePath, pgid, moRequest["cPosition"], false, nReturnId: argnReturnId2, zcReturnSchema:   argzcReturnSchema2, AlternateFormName: argAlternateFormName2);
                                    }

                                    else
                                    {
                                        long argnReturnId3 = 0;
                                        string argzcReturnSchema3 = "";
                                        string argAlternateFormName3 = "";
                                        xFrmEditContent(0L, "Module/" + moRequest["cModuleType"], pgid, moRequest["cPosition"], false, nReturnId: argnReturnId3, zcReturnSchema: argzcReturnSchema3, AlternateFormName:  argAlternateFormName3);
                                    }
                                }
                            }

                            base.addValues();
                            return base.moXformElmt;
                        }
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "addInput", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                public string GetContentFormPath(string SchemaName)
                {
                    string cProcessInfo = "";
                    try
                    {
                        string cssFramework = string.Empty;
                        if (goConfig["cssFramework"] != null)
                        {
                            cssFramework = goConfig["cssFramework"];
                        }
                        var oManifest = GetSiteManifest(cssFramework);
                        XmlElement thisModule = (XmlElement)oManifest.SelectSingleNode("descendant-or-self::ContentType[@type='" + SchemaName + "']");
                        if (thisModule is null)
                        {
                            return SchemaName;
                        }
                        else
                        {
                            return thisModule.GetAttribute("formPath");
                        }
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "GetContentFormPath", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                protected string GetFilterFormPath(string SchemaName)
                {
                    string cProcessInfo = "";
                    try
                    {

                        var oManifest = GetSiteManifest(goConfig["cssFramework"]);
                        XmlElement thisModule = (XmlElement)oManifest.SelectSingleNode("descendant-or-self::Filter[@type='" + SchemaName + "']");
                        if (thisModule is null)
                        {
                            return SchemaName;
                        }
                        else
                        {
                            return thisModule.GetAttribute("formPath");
                        }
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "GetContentFormPath", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                protected string GetModuleFormPath(string SchemaName)
                {
                    string cProcessInfo = "";
                    try
                    {

                        var oManifest = GetSiteManifest();
                        XmlElement thisModule = (XmlElement)oManifest.SelectSingleNode("descendant-or-self::Module[@type='" + SchemaName + "']");
                        if (thisModule is null)
                        {
                            return SchemaName;
                        }
                        else
                        {
                            return thisModule.GetAttribute("formPath");
                        }
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "GetContentFormPath", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }


                public XmlDocument GetSiteManifest(string sFramework = "bs5")
                {
                    string cProcessInfo = "";

                    XmlDocument ManifestDoc = null;
                    try
                    {
                        switch (sFramework ?? "")
                        {
                            case "bs5":
                                {

                                    string PathPrefix = @"ptn\";

                                    EnumberateManifest(ref ManifestDoc, "/" + Cms.gcProjectPath + PathPrefix + @"core\modules", "manifest.xml");
                                    var rootFolder = new DirectoryInfo(myWeb.goServer.MapPath("/" + Cms.gcProjectPath + PathPrefix + "modules"));
                                    DirectoryInfo fld;
                                    foreach (var currentFld in rootFolder.GetDirectories())
                                    {
                                        fld = currentFld;
                                        EnumberateManifest(ref ManifestDoc, "/" + Cms.gcProjectPath + PathPrefix + @"modules\" + fld.Name, "manifest.xml");
                                        // EnumberateManifest(ManifestDoc, "/" & gcProjectPath & "modules\" & fld.Name, "manifest.xml")

                                    }
                                    if (!string.IsNullOrEmpty(myWeb.moConfig["ClientCommonFolder"]))
                                    {
                                        EnumberateManifest(ref ManifestDoc, myWeb.moConfig["ClientCommonFolder"] + @"\xsl", "manifest.xml");
                                    }

                                    // new local modules
                                    rootFolder = new DirectoryInfo(myWeb.goServer.MapPath("/" + Cms.gcProjectPath + "/modules"));
                                    if (rootFolder.Exists)
                                    {
                                        foreach (var currentFld1 in rootFolder.GetDirectories())
                                        {
                                            fld = currentFld1;
                                            EnumberateManifest(ref ManifestDoc, "/" + Cms.gcProjectPath + @"\modules\" + fld.Name, "manifest.xml");
                                        }
                                    }

                                    EnumberateManifest(ref ManifestDoc, "/xsl", "manifest.xml");

                                    if (myWeb.moConfig["Search"] == "on")
                                    {
                                        EnumberateManifest(ref ManifestDoc, "/" + Cms.gcProjectPath + PathPrefix + @"features\search", "manifest.xml");
                                    }
                                    if (myWeb.moConfig["Membership"] == "on")
                                    {
                                        EnumberateManifest(ref ManifestDoc, "/" + Cms.gcProjectPath + PathPrefix + @"features\membership", "manifest.xml");
                                    }
                                    if (myWeb.moConfig["Cart"] == "on")
                                    {
                                        EnumberateManifest(ref ManifestDoc, "/" + Cms.gcProjectPath + PathPrefix + @"features\cart", "manifest.xml");
                                    }
                                    if (myWeb.moConfig["Quote"] == "on")
                                    {
                                        EnumberateManifest(ref ManifestDoc, "/" + Cms.gcProjectPath + PathPrefix + @"features\quote", "manifest.xml");
                                    }
                                    if (myWeb.moConfig["MailingList"] == "on")
                                    {
                                        EnumberateManifest(ref ManifestDoc, "/" + Cms.gcProjectPath + PathPrefix + @"features\mailer", "manifest.xml");
                                    }
                                    if (myWeb.moConfig["Subscriptions"] == "on")
                                    {
                                        EnumberateManifest(ref ManifestDoc, "/" + Cms.gcProjectPath + PathPrefix + @"features\subscriptions", "manifest.xml");
                                    }

                                    break;
                                }
                            default:
                                {
                                    string PathPrefix = @"ewcommon\";
                                    EnumberateManifest(ref ManifestDoc, "/" + Cms.gcProjectPath + PathPrefix + "xsl/PageLayouts", "LayoutManifest.xml");
                                    if (!string.IsNullOrEmpty(myWeb.moConfig["ClientCommonFolder"]))
                                    {
                                        EnumberateManifest(ref ManifestDoc, myWeb.moConfig["ClientCommonFolder"] + @"\xsl", "layoutManifest.xml");
                                    }
                                    EnumberateManifest(ref ManifestDoc, "/xsl", "layoutManifest.xml");
                                    break;
                                }
                        }
                        return ManifestDoc;
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "addInput", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }


                public string GetMailModuleFormPath(string SchemaName)
                {
                    string cProcessInfo = "";
                    try
                    {

                        var oManifest = GetMailManifest();
                        XmlElement thisModule = (XmlElement)oManifest.SelectSingleNode("descendant-or-self::Module[@type='" + SchemaName + "']");
                        if (thisModule is null)
                        {
                            return SchemaName;
                        }
                        else
                        {
                            return thisModule.GetAttribute("formPath");
                        }
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "GetContentFormPath", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }


                public XmlDocument GetMailManifest(string sFramework = "bs5")
                {
                    string cProcessInfo = "";

                    XmlDocument ManifestDoc = null;
                    try
                    {
                        switch (sFramework ?? "")
                        {
                            case "bs5":
                                {

                                    string PathPrefix = @"ptn\";

                                    EnumberateManifest(ref ManifestDoc, "/" + Cms.gcProjectPath + PathPrefix + @"features/mailer/modules", "manifest.xml");

                                    var rootFolder = new DirectoryInfo(myWeb.goServer.MapPath("/" + Cms.gcProjectPath + PathPrefix + @"features/mailer/modules"));
                                    DirectoryInfo fld;
                                    foreach (var currentFld in rootFolder.GetDirectories())
                                    {
                                        fld = currentFld;
                                        EnumberateManifest(ref ManifestDoc, "/" + Cms.gcProjectPath + PathPrefix + @"modules\" + fld.Name, "manifest.xml");

                                    }
                                    if (!string.IsNullOrEmpty(myWeb.moConfig["ClientCommonFolder"]))
                                    {
                                        EnumberateManifest(ref ManifestDoc, myWeb.moConfig["ClientCommonFolder"] + @"\xsl", "manifest.xml");
                                    }

                                    // new local modules
                                    rootFolder = new DirectoryInfo(myWeb.goServer.MapPath("/" + Cms.gcProjectPath + "/modules"));
                                    if (rootFolder.Exists)
                                    {
                                        foreach (var currentFld1 in rootFolder.GetDirectories())
                                        {
                                            fld = currentFld1;
                                            EnumberateManifest(ref ManifestDoc, "/" + Cms.gcProjectPath + @"\modules\" + fld.Name, "manifest.xml");
                                        }
                                    }

                                    break;
                                }
                            default:
                                {
                                    string PathPrefix = @"ewcommon\";
                                    EnumberateManifest(ref ManifestDoc, "/" + Cms.gcProjectPath + PathPrefix + "xsl/PageLayouts", "LayoutManifest.xml");
                                    if (!string.IsNullOrEmpty(myWeb.moConfig["ClientCommonFolder"]))
                                    {
                                        EnumberateManifest(ref ManifestDoc, myWeb.moConfig["ClientCommonFolder"] + @"\xsl", "layoutManifest.xml");
                                    }
                                    EnumberateManifest(ref ManifestDoc, "/xsl", "layoutManifest.xml");
                                    break;
                                }
                        }
                        return ManifestDoc;
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "addInput", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }


                public void EnumberateManifest(ref XmlDocument ManifestDoc, string filepath, string manifestFilename = "LayoutManifest.xml")
                {

                    string cProcessInfo = "";
                    //string sImgPath = "";
                    XmlElement oContentType;
                    XmlElement oModuleType;
                    // Dim oItem As XmlElement
                    // Dim oOptElmt As XmlElement   'never used
                    // Dim oDescElmt As XmlElement

                    try
                    {
                        if (string.IsNullOrEmpty(filepath))
                            filepath = "/";
                        filepath = filepath.Replace(@"\", "/");


                        if (File.Exists(myWeb.goServer.MapPath(filepath) + "/" + manifestFilename))
                        {
                            // if this file exists then add the bespoke templates

                            if (ManifestDoc is null)
                            {
                                ManifestDoc = new XmlDocument();
                                ManifestDoc.Load(myWeb.goServer.MapPath(filepath) + "/" + manifestFilename);
                                foreach (XmlElement currentOContentType in ManifestDoc.SelectNodes("/PageLayouts/ContentTypes/ContentTypeGroup/ContentType"))
                                {
                                    oContentType = currentOContentType;
                                    string formPath = oContentType.GetAttribute("formPath");
                                    // If formPath.contains("/") Then
                                    // formPath = formPath.Split("/")(1)
                                    // End If
                                    if (!string.IsNullOrEmpty(formPath))
                                    {
                                        // do nothing, just checked here if formPath attribute is present and not blank
                                    }
                                    else
                                    {
                                        oContentType.SetAttribute("formPath", filepath.Replace("/ptn", "") + "/" + formPath);
                                    }
                                }
                                foreach (XmlElement currentOModuleType in ManifestDoc.SelectNodes("/PageLayouts/ModuleTypes/ModuleGroup/Module"))
                                {
                                    oModuleType = currentOModuleType;
                                    string formPath = oModuleType.GetAttribute("formPath");
                                    // If formPath.contains("/") Then
                                    // formPath = formPath.Split("/")(1)
                                    // End If
                                    oModuleType.SetAttribute("formPath", filepath.Replace(@"\", "/") + "/" + formPath);
                                }
                            }

                            else
                            {
                                var ManifestTemp = new XmlDocument();
                                ManifestTemp.Load(myWeb.goServer.MapPath(filepath) + "/" + manifestFilename);

                                // step through contentTypes to add to ManifestDoc

                                foreach (XmlElement currentOContentType1 in ManifestTemp.SelectNodes("/PageLayouts/ContentTypes/ContentTypeGroup/ContentType"))
                                {
                                    oContentType = currentOContentType1;
                                    // build the xformPath
                                    string formPath = oContentType.GetAttribute("formPath");
                                    // If formPath.contains("/") Then
                                    // formPath = formPath.Split("/")(1)
                                    // End If
                                    oContentType.SetAttribute("formPath", filepath.Replace("/ptn", "") + "/" + formPath);

                                    string contentTypeGroupName = oContentType.SelectSingleNode("parent::ContentTypeGroup/@name").InnerText;
                                    XmlElement contentTypeGroup = (XmlElement)ManifestDoc.SelectSingleNode("/PageLayouts/ContentTypes/ContentTypeGroup[@name='" + contentTypeGroupName + "']");
                                    if (contentTypeGroup is null)
                                    {
                                        XmlElement oContentTypes = (XmlElement)ManifestDoc.SelectSingleNode("/PageLayouts/ContentTypes");
                                        oContentTypes.AppendChild(oContentTypes.OwnerDocument.ImportNode(oContentType.SelectSingleNode("parent::ContentTypeGroup"), true));
                                    }
                                    else
                                    {
                                        string ContentTypeName = oContentType.GetAttribute("name");
                                        if (contentTypeGroup.SelectSingleNode("ContentType[@name='" + ContentTypeName + "']") is null)
                                        {
                                            contentTypeGroup.AppendChild(contentTypeGroup.OwnerDocument.ImportNode(oContentType, true));
                                        }
                                        else
                                        {
                                            contentTypeGroup.ReplaceChild(contentTypeGroup.OwnerDocument.ImportNode(oContentType, true), contentTypeGroup.SelectSingleNode("ContentType[@name='" + ContentTypeName + "']"));
                                        }
                                    }
                                }

                                // step through moduleTypes to add to ManifestDoc
                                foreach (XmlElement currentOModuleType1 in ManifestTemp.SelectNodes("/PageLayouts/ModuleTypes/ModuleGroup/Module"))
                                {
                                    oModuleType = currentOModuleType1;
                                    string formPath = oModuleType.GetAttribute("formPath");
                                    // If formPath.contains("/") Then
                                    // formPath = formPath.Split("/")(1)
                                    // End If
                                    oModuleType.SetAttribute("formPath", filepath + "/" + formPath);
                                    string moduleGroupName = oModuleType.SelectSingleNode("parent::ModuleGroup/@name").InnerText;
                                    XmlElement moduleGroup = (XmlElement)ManifestDoc.SelectSingleNode("/PageLayouts/ModuleTypes/ModuleGroup[@name='" + moduleGroupName + "']");
                                    if (moduleGroup is null)
                                    {
                                        XmlElement oModuleTypes = (XmlElement)ManifestDoc.SelectSingleNode("/PageLayouts/ModuleTypes");
                                        oModuleTypes.AppendChild(oModuleTypes.OwnerDocument.ImportNode(oModuleType.SelectSingleNode("parent::ModuleGroup"), true));
                                    }
                                    else
                                    {
                                        string ModuleTypeName = oModuleType.GetAttribute("name");
                                        if (moduleGroup.SelectSingleNode("Module[@name='" + ModuleTypeName + "']") is null)
                                        {
                                            moduleGroup.AppendChild(moduleGroup.OwnerDocument.ImportNode(oModuleType.CloneNode(true), true));
                                        }
                                        else
                                        {
                                            moduleGroup.ReplaceChild(moduleGroup.OwnerDocument.ImportNode(oModuleType, true), moduleGroup.SelectSingleNode("Module[@name='" + ModuleTypeName + "']"));

                                        }
                                    }
                                }

                                // step through filterTypes to add to ManifestDoc
                                foreach (XmlElement currentOModuleType2 in ManifestTemp.SelectNodes("/PageLayouts/FilterTypes/Filter"))
                                {
                                    oModuleType = currentOModuleType2;
                                    string formPath = oModuleType.GetAttribute("formPath");
                                    string FilterTypeName = oModuleType.GetAttribute("type");
                                    // oModuleType.SetAttribute("formPath", filepath & "/" & formPath)

                                    XmlElement filterTypes = (XmlElement)ManifestDoc.SelectSingleNode("/PageLayouts/FilterTypes");
                                    if (filterTypes == null)
                                    {
                                        filterTypes = ManifestDoc.CreateElement("FilterTypes");
                                        ManifestDoc.DocumentElement.AppendChild(filterTypes);
                                    }
                                    if (filterTypes != null)
                                    {
                                        if (filterTypes.SelectSingleNode("Module[@name='" + FilterTypeName + "']") is null)
                                        {
                                            filterTypes.AppendChild(filterTypes.OwnerDocument.ImportNode(oModuleType.CloneNode(true), true));
                                        }
                                        else
                                        {
                                            filterTypes.ReplaceChild(filterTypes.OwnerDocument.ImportNode(oModuleType, true), filterTypes.SelectSingleNode("Filter[@name='" + FilterTypeName + "']"));
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            // do nothing
                        }
                    }


                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "EnumberateManifestOptions", ex, "", cProcessInfo, gbDebug);
                    }

                }
                protected void GetContentOptions(ref XmlElement oSelElmt)
                {
                    string cProcessInfo = "";
                    try
                    {
                        string PathPrefix = "ewcommon/xsl/";
                        if (goConfig["cssFramework"] == "bs5")
                        {
                            PathPrefix = @"ptn\";
                            EnumberateManifestOptions(ref oSelElmt, "/" + Cms.gcProjectPath + PathPrefix + @"core\modules", "ContentTypes/ContentTypeGroup", "ContentType", false, "manifest.xml");
                            var rootFolder = new DirectoryInfo(goServer.MapPath("/" + Cms.gcProjectPath + PathPrefix + "modules"));
                            DirectoryInfo fld;
                            foreach (var currentFld in rootFolder.GetDirectories())
                            {
                                fld = currentFld;
                                EnumberateManifestOptions(ref oSelElmt, "/" + Cms.gcProjectPath + PathPrefix + @"modules\" + fld.Name, "ContentTypes/ContentTypeGroup", "ContentType", false, "manifest.xml");

                            }
                            if (!string.IsNullOrEmpty(myWeb.moConfig["ClientCommonFolder"]))
                            {
                                EnumberateManifestOptions(ref oSelElmt, myWeb.moConfig["ClientCommonFolder"] + @"\xsl", "ContentTypes/ContentTypeGroup", "ContentType", false, "manifest.xml");
                            }

                            // new local modules
                            rootFolder = new DirectoryInfo(goServer.MapPath("/" + Cms.gcProjectPath + "/modules"));
                            if (rootFolder.Exists)
                            {
                                foreach (var currentFld1 in rootFolder.GetDirectories())
                                {
                                    fld = currentFld1;
                                    EnumberateManifestOptions(ref oSelElmt, "/" + Cms.gcProjectPath + @"\modules\" + fld.Name, "ContentTypes/ContentTypeGroup", "ContentType", true, "manifest.xml");
                                }
                            }

                            EnumberateManifestOptions(ref oSelElmt, "/xsl", "ContentTypes/ContentTypeGroup", "ContentType", false);

                            if (myWeb.moConfig["Search"] == "on")
                            {
                                EnumberateManifestOptions(ref oSelElmt, "/" + Cms.gcProjectPath + PathPrefix + @"features\search", "ContentTypes/ContentTypeGroup", "ContentType", false, "manifest.xml");
                            }
                            if (myWeb.moConfig["Membership"] == "on")
                            {
                                EnumberateManifestOptions(ref oSelElmt, "/" + Cms.gcProjectPath + PathPrefix + @"features\membership", "ContentTypes/ContentTypeGroup", "ContentType", false, "manifest.xml");
                            }
                            if (myWeb.moConfig["Cart"] == "on")
                            {
                                EnumberateManifestOptions(ref oSelElmt, "/" + Cms.gcProjectPath + PathPrefix + @"features\cart", "ContentTypes/ContentTypeGroup", "ContentType", false, "manifest.xml");
                            }
                            if (myWeb.moConfig["Quote"] == "on")
                            {
                                EnumberateManifestOptions(ref oSelElmt, "/" + Cms.gcProjectPath + PathPrefix + @"features\quote", "ContentTypes/ContentTypeGroup", "ContentType", false, "manifest.xml");
                            }
                            if (myWeb.moConfig["MailingList"] == "on")
                            {
                                EnumberateManifestOptions(ref oSelElmt, "/" + Cms.gcProjectPath + PathPrefix + @"features\mailer", "ContentTypes/ContentTypeGroup", "ContentType", false, "manifest.xml");
                            }
                            if (myWeb.moConfig["Subscriptions"] == "on")
                            {
                                EnumberateManifestOptions(ref oSelElmt, "/" + Cms.gcProjectPath + PathPrefix + @"features\subscriptions", "ContentTypes/ContentTypeGroup", "ContentType", false, "manifest.xml");
                            }
                        }
                        else
                        {
                            // EnumberateManifestOptions(oSelElmt, "/" & gcProjectPath & PathPrefix & "PageLayouts", "ModuleTypes/ModuleGroup", "Module", False)

                            // MyBase.addNote(oFrmElmt, xForm.noteTypes.Hint, "Click the image to select Module Type")

                            // EnumberateManifestOptions(oSelElmt, "/" & gcProjectPath & PathPrefix & "PageLayouts", "ModuleTypes/ModuleGroup", "Module", False)

                            // If myWeb.moConfig("ClientCommonFolder") <> "" Then
                            // EnumberateManifestOptions(oSelElmt, myWeb.moConfig("ClientCommonFolder") & "/xsl", "ModuleTypes/ModuleGroup", "Module", False)
                            // End If
                            // EnumberateManifestOptions(oSelElmt, "/xsl", "ModuleTypes/ModuleGroup", "Module", True)
                            // If myWeb.moConfig("Search") = "on" Then
                            // EnumberateManifestOptions(oSelElmt, "/" & gcProjectPath & PathPrefix & "Search", "ModuleTypes/ModuleGroup", "Module", False)
                            // End If
                            // If myWeb.moConfig("Membership") = "on" Then
                            // EnumberateManifestOptions(oSelElmt, "/" & gcProjectPath & PathPrefix & "Membership", "ModuleTypes/ModuleGroup", "Module", False)
                            // End If
                            // If myWeb.moConfig("Cart") = "on" Then
                            // EnumberateManifestOptions(oSelElmt, "/" & gcProjectPath & PathPrefix & "Cart", "ModuleTypes/ModuleGroup", "Module", False)
                            // End If
                            // If myWeb.moConfig("Quote") = "on" Then
                            // EnumberateManifestOptions(oSelElmt, "/" & gcProjectPath & PathPrefix & "Quote", "ModuleTypes/ModuleGroup", "Module", False)
                            // End If
                            // If myWeb.moConfig("MailingList") = "on" Then
                            // EnumberateManifestOptions(oSelElmt, "/" & gcProjectPath & PathPrefix & "Mailer", "ModuleTypes/ModuleGroup", "Module", False)
                            // End If
                            // If myWeb.moConfig("Subscriptions") = "on" Then
                            // EnumberateManifestOptions(oSelElmt, "/" & gcProjectPath & PathPrefix & "Subscriptions", "ModuleTypes/ModuleGroup", "Module", False)
                            // End If
                        }
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "addInput", ex, "", cProcessInfo, gbDebug);
                    }
                }

                protected void GetModuleOptions(ref XmlElement oSelElmt)
                {
                    string cProcessInfo = "";
                    try
                    {
                        string PathPrefix = "ewcommon/xsl/";
                        if (goConfig["cssFramework"] == "bs5")
                        {
                            PathPrefix = @"ptn\";
                            EnumberateManifestOptions(ref oSelElmt, "/" + Cms.gcProjectPath + PathPrefix + @"core\modules", "ModuleTypes/ModuleGroup", "Module", false, "manifest.xml");
                            var rootFolder = new DirectoryInfo(goServer.MapPath("/" + Cms.gcProjectPath + PathPrefix + "modules"));
                            DirectoryInfo fld;
                            foreach (var currentFld in rootFolder.GetDirectories())
                            {
                                fld = currentFld;
                                EnumberateManifestOptions(ref oSelElmt, "/" + Cms.gcProjectPath + PathPrefix + @"modules\" + fld.Name, "ModuleTypes/ModuleGroup", "Module", true, "manifest.xml");
                            }
                            if (!string.IsNullOrEmpty(myWeb.moConfig["ClientCommonFolder"]))
                            {
                                EnumberateManifestOptions(ref oSelElmt, myWeb.moConfig["ClientCommonFolder"] + @"\xsl", "ModuleTypes/ModuleGroup", "Module", false, "manifest.xml");
                            }

                            // new local modules
                            rootFolder = new DirectoryInfo(goServer.MapPath("/" + Cms.gcProjectPath + "/modules"));
                            if (rootFolder.Exists)
                            {
                                foreach (var currentFld1 in rootFolder.GetDirectories())
                                {
                                    fld = currentFld1;
                                    EnumberateManifestOptions(ref oSelElmt, "/" + Cms.gcProjectPath + @"\modules\" + fld.Name, "ModuleTypes/ModuleGroup", "Module", true, "manifest.xml");
                                }
                            }

                            // legacy local modules
                            EnumberateManifestOptions(ref oSelElmt, "/xsl", "ModuleTypes/ModuleGroup", "Module", true);

                            if (myWeb.moConfig["Search"] == "on")
                            {
                                EnumberateManifestOptions(ref oSelElmt, "/" + Cms.gcProjectPath + PathPrefix + @"features\search", "ModuleTypes/ModuleGroup", "Module", false, "manifest.xml");
                            }
                            if (myWeb.moConfig["Membership"] == "on")
                            {
                                EnumberateManifestOptions(ref oSelElmt, "/" + Cms.gcProjectPath + PathPrefix + @"features\membership", "ModuleTypes/ModuleGroup", "Module", false, "manifest.xml");
                            }
                            if (myWeb.moConfig["Cart"] == "on")
                            {
                                EnumberateManifestOptions(ref oSelElmt, "/" + Cms.gcProjectPath + PathPrefix + @"features\cart", "ModuleTypes/ModuleGroup", "Module", false, "manifest.xml");
                            }
                            if (myWeb.moConfig["Quote"] == "on")
                            {
                                EnumberateManifestOptions(ref oSelElmt, "/" + Cms.gcProjectPath + PathPrefix + @"features\quote", "ModuleTypes/ModuleGroup", "Module", false, "manifest.xml");
                            }
                            if (myWeb.moConfig["MailingList"] == "on")
                            {
                                EnumberateManifestOptions(ref oSelElmt, "/" + Cms.gcProjectPath + PathPrefix + @"features\mailer", "ModuleTypes/ModuleGroup", "Module", false, "manifest.xml");
                            }
                            if (myWeb.moConfig["Subscriptions"] == "on")
                            {
                                EnumberateManifestOptions(ref oSelElmt, "/" + Cms.gcProjectPath + PathPrefix + @"features\subscriptions", "ModuleTypes/ModuleGroup", "Module", false, "manifest.xml");
                            }
                        }
                        else
                        {
                            EnumberateManifestOptions(ref oSelElmt, "/" + Cms.gcProjectPath + PathPrefix + "PageLayouts", "ModuleTypes/ModuleGroup", "Module", false);

                            if (!string.IsNullOrEmpty(myWeb.moConfig["ClientCommonFolder"]))
                            {
                                EnumberateManifestOptions(ref oSelElmt, myWeb.moConfig["ClientCommonFolder"] + "/xsl", "ModuleTypes/ModuleGroup", "Module", false);
                            }
                            EnumberateManifestOptions(ref oSelElmt, "/xsl", "ModuleTypes/ModuleGroup", "Module", true);
                            if (myWeb.moConfig["Search"] == "on")
                            {
                                EnumberateManifestOptions(ref oSelElmt, "/" + Cms.gcProjectPath + PathPrefix + "Search", "ModuleTypes/ModuleGroup", "Module", false);
                            }
                            if (myWeb.moConfig["Membership"] == "on")
                            {
                                EnumberateManifestOptions(ref oSelElmt, "/" + Cms.gcProjectPath + PathPrefix + "Membership", "ModuleTypes/ModuleGroup", "Module", false);
                            }
                            if (myWeb.moConfig["Cart"] == "on")
                            {
                                EnumberateManifestOptions(ref oSelElmt, "/" + Cms.gcProjectPath + PathPrefix + "Cart", "ModuleTypes/ModuleGroup", "Module", false);
                            }
                            if (myWeb.moConfig["Quote"] == "on")
                            {
                                EnumberateManifestOptions(ref oSelElmt, "/" + Cms.gcProjectPath + PathPrefix + "Quote", "ModuleTypes/ModuleGroup", "Module", false);
                            }
                            if (myWeb.moConfig["MailingList"] == "on")
                            {
                                EnumberateManifestOptions(ref oSelElmt, "/" + Cms.gcProjectPath + PathPrefix + "Mailer", "ModuleTypes/ModuleGroup", "Module", false);
                            }
                            if (myWeb.moConfig["Subscriptions"] == "on")
                            {
                                EnumberateManifestOptions(ref oSelElmt, "/" + Cms.gcProjectPath + PathPrefix + "Subscriptions", "ModuleTypes/ModuleGroup", "Module", false);
                            }
                        }
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "addInput", ex, "", cProcessInfo, gbDebug);
                    }
                }


                protected void EnumberateManifestOptions(ref XmlElement oSelectElmt, string filepath, string groupName, string optionName, bool bIgnoreIfNotFound, string manifestFilename = "LayoutManifest.xml")
                {

                    var oXformDoc = new XmlDocument();
                    string cProcessInfo = "";
                    string sImgPath = "";
                    XmlElement oOptElmt;
                    XmlElement oDescElmt;

                    try
                    {
                        if (string.IsNullOrEmpty(filepath))
                            filepath = "/";



                        if (File.Exists(goServer.MapPath(filepath) + "/" + manifestFilename))
                        {
                            // if this file exists then add the bespoke templates
                            oXformDoc.Load(goServer.MapPath(filepath) + "/" + manifestFilename);
                            sImgPath = oXformDoc.DocumentElement.GetAttribute("imgPath");
                            foreach (XmlElement oChoices in oXformDoc.SelectNodes("/PageLayouts/" + groupName))
                            {
                                if (string.IsNullOrEmpty(oChoices.GetAttribute("targetCssFramework")) | myWeb.moConfig["cssFramework"] != null & oChoices.GetAttribute("targetCssFramework").Contains("" + myWeb.moConfig["cssFramework"]))
                                {
                                    // do we have a choices element?
                                    XmlElement oChoicesElmt = (XmlElement)oSelectElmt.SelectSingleNode("choices[label/node()='" + oChoices.GetAttribute("name") + "']");
                                    if (oChoicesElmt is null)
                                    {
                                        oChoicesElmt = base.addChoices(ref oSelectElmt, oChoices.GetAttribute("name"));
                                    }

                                    if (!string.IsNullOrEmpty(oChoices.GetAttribute("icon")))
                                    {
                                        XmlElement labelElmt = (XmlElement)oChoicesElmt.SelectSingleNode("label");
                                        labelElmt.SetAttribute("icon", oChoices.GetAttribute("icon"));
                                    }
                                    foreach (XmlElement oItem in oChoices.SelectNodes(optionName))
                                    {
                                        if (string.IsNullOrEmpty(oItem.GetAttribute("targetCssFramework")) | myWeb.moConfig["cssFramework"] != null & oItem.GetAttribute("targetCssFramework").Contains("" + myWeb.moConfig["cssFramework"]))
                                        {
                                            string FormPath = oItem.GetAttribute("type");
                                            oOptElmt = base.addOption(ref oChoicesElmt, oItem.GetAttribute("name").Replace("_", " "), FormPath);
                                            // lets add an image tag
                                            // If oItem.GetAttribute("type") = "LibraryImage" Then
                                            // oOptElmt.SetAttribute("type", oItem.GetAttribute("type"))
                                            // End If

                                            oOptElmt.SetAttribute("type", oItem.GetAttribute("type"));
                                            if (!string.IsNullOrEmpty(oItem.GetAttribute("formPath")))
                                            {
                                                oOptElmt.SetAttribute("formPath", oItem.GetAttribute("formPath"));
                                            }
                                            oDescElmt = moPageXML.CreateElement("img");
                                            oDescElmt.SetAttribute("src", sImgPath + "/" + oItem.GetAttribute("name") + ".gif");
                                            if (!string.IsNullOrEmpty(oItem.GetAttribute("icon")))
                                            {
                                                oDescElmt.SetAttribute("icon", oItem.GetAttribute("icon"));
                                            }
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

                            }
                        }
                        else if (!bIgnoreIfNotFound)
                        {
                            var argoNode = oSelectElmt.ParentNode;
                            base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, filepath + " could not be found.");
                        }
                    }


                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "EnumberateManifestOptions", ex, "", cProcessInfo, gbDebug);
                    }

                }
                // Public Overridable Function xFrmEditContent(Optional ByVal id As Long = 0, Optional ByVal cContentSchemaName As String = "", Optional ByVal pgid As Long = 0, Optional ByVal cContentName As String = "", Optional ByVal bCopy As Boolean = False, Optional ByRef nReturnId As Integer = 0, Optional ByVal nVersionId As Long = 0) As XmlElement
                // xFrmEditContent(id, cContentSchemaName, pgid, cContentName, bCopy, nReturnId, "", "", Optional ByVal nVersionId As Long = 0)
                // End Function
                public virtual XmlElement xFrmEditContent(long id = 0L, string cContentSchemaName = "", long pgid = 0L, string cContentName = "", bool bCopy = false)
                {
                    long unusedReturnId = 0;
                    string unusedReturnSchema = "";
                    string unusedAlternateFormName = "";
                    return xFrmEditContent(id, cContentSchemaName, pgid, cContentName, bCopy,  unusedReturnId,  unusedReturnSchema,  unusedAlternateFormName);
                }

                public virtual XmlElement xFrmEditContent(long id, string cContentSchemaName, long pgid, string cContentName, bool bCopy,  long nReturnId,  string zcReturnSchema,  string AlternateFormName, long nVersionId = 0L)
                {
                    XmlElement oFrmElmt;
                    // Dim oGrp1Elmt As XmlElement
                    // Dim oGrp2Elmt As XmlElement   'Never used
                    // Dim oSelElmt As XmlElement
                    var oTempInstance = moPageXML.CreateElement("instance");
                    bool bCascade = false;
                    string cProcessInfo = "";
                    XmlElement oCRNode;
                    string cModuleType = "";
                    string cFilterType = "";
                    // Location specific scopes
                    //XmlNodeList oLocationSelects = null;
                    //XmlNodeList oMenuItemsFromSelect = null;

                    xFormContentLocations oContentLocations;


                    try
                    {

                        var integrationHelper = new Integration.Directory.Helper(ref myWeb);
                        // if product
                        string sProductTypes = "Product,SKU,Ticket";
                        if (myWeb.Features.ContainsKey("Subscriptions"))
                        {
                            sProductTypes = sProductTypes + ",Subscription";
                        }
                        if (!string.IsNullOrEmpty(myWeb.moConfig["ProductTypes"]))
                        {
                            sProductTypes = myWeb.moConfig["ProductTypes"];
                        }
                        sProductTypes = sProductTypes.Trim().TrimEnd(',') + ",";


                        if (id > 0L)
                        {
                            // we may be halfway through a trigger so lets rescue the instance from the session
                            if (goSession["oContentInstance"] is null)
                            {
                                if (nVersionId > 0L)
                                {

                                    oTempInstance = moDbHelper.GetVersionInstance(id, nVersionId);
                                    // Only Update the status if the cmd is ewcmd is RollbackContent
                                    if (myWeb.moRequest["ewCmd"] == "RollbackContent")
                                    {
                                        oTempInstance.SelectSingleNode("tblContent/nStatus").InnerText = ((int)Cms.dbHelper.Status.Live).ToString();
                                    }
                                }

                                else
                                {
                                    oTempInstance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.Content, id);
                                }

                                // turn off process repeats when loading from file
                                bProcessRepeats = false;
                            }
                            else
                            {
                                oTempInstance = (XmlElement)goSession["oContentInstance"];
                            }

                            if (string.IsNullOrEmpty(cContentSchemaName))
                            {
                                cContentSchemaName = oTempInstance.SelectSingleNode("tblContent/cContentSchemaName").InnerText;
                                if (cContentSchemaName == "Module")
                                {
                                    if (oTempInstance.SelectSingleNode("tblContent/cContentXmlBrief/Content/@moduleType") != null)
                                    {
                                        cModuleType = oTempInstance.SelectSingleNode("tblContent/cContentXmlBrief/Content/@moduleType").Value;
                                    }
                                }
                                if (cContentSchemaName == "Filter")
                                {
                                    if (oTempInstance.SelectSingleNode("tblContent/cContentXmlBrief/Content/@filterType") != null)
                                    {
                                        cFilterType = oTempInstance.SelectSingleNode("tblContent/cContentXmlBrief/Content/@filterType").Value;
                                    }
                                }

                                if (!string.IsNullOrEmpty(moRequest["type"]))
                                    cContentSchemaName = moRequest["type"];
                            }

                            XmlElement argContentNode = (XmlElement)oTempInstance.FirstChild;
                            moDbHelper.getLocationsByContentId(id, ref argContentNode);

                            // Add ProductCategories
                            if (sProductTypes.Contains(cContentSchemaName + ",") & id > 0L)
                            {
                                if (moDbHelper.checkDBObjectExists("sp_GetProductGroups"))
                                {
                                    var prodCatElmt = oTempInstance.OwnerDocument.CreateElement("ProductGroups");
                                    string sSQL = "execute sp_GetProductGroups " + id;
                                    string Ids = Convert.ToString(moDbHelper.GetDataValue(sSQL));
                                    Ids.TrimEnd(',');
                                    prodCatElmt.SetAttribute("ids", Ids);
                                    oTempInstance.AppendChild(prodCatElmt);
                                }
                                AddPageSpecs(ref myWeb.mnPageId, ref oTempInstance);
                            }
                        }
                        else
                        {
                            cModuleType = moRequest["cModuleType"];
                        }

                        if (goSession["oContentInstance"] != null)
                        {
                            // turn off process repeats when loading from file if we are going to load the instance later.
                            bProcessRepeats = false;
                        }

                        // Set the return parameter
                        zcReturnSchema = cContentSchemaName;

                        // ok lets load in an xform from the file location.
                        // If cContentSchemaName = "Subscription" Then
                        // Return xFrmEditSubscription(id, pgid)
                        // End If

                        // '''''' if contentSchemeaName starts with "filter|" then modify the path...

                        string cXformPath = cContentSchemaName;

                        if (!string.IsNullOrEmpty(AlternateFormName))
                            cXformPath = AlternateFormName;

                        // Quick fix for V4 sites
                        if (cModuleType == "BasicContentTypes")
                        {
                            cModuleType = "";
                        }

                        if (!string.IsNullOrEmpty(cModuleType))
                        {
                            if (goConfig["cssFramework"] == "bs5")
                            {
                                if (_moduleName.Contains("Providers.Messaging"))
                                {

                                    cXformPath = GetMailModuleFormPath(cModuleType);

                                }
                                else
                                {

                                    cXformPath = GetModuleFormPath(cModuleType);

                                }


                            }
                            else if (!cXformPath.EndsWith("/" + cModuleType))
                            {
                                cXformPath = cXformPath + "/" + cModuleType;

                            }
                        }
                        else if (goConfig["cssFramework"] == "bs5")
                        {
                            cXformPath = GetContentFormPath(cContentSchemaName);
                            if (!string.IsNullOrEmpty(AlternateFormName))
                                cXformPath = cXformPath.Remove(cXformPath.Length - cContentSchemaName.Length) + AlternateFormName;
                        }
                        if (moRequest["filter"] == "true")
                        {
                            cXformPath = GetFilterFormPath(cContentSchemaName);
                        }

                        if (goConfig["cssFramework"] == "bs5")
                        {
                            if (cXformPath.StartsWith("/"))
                            {
                                //cXformPath = cXformPath;
                            }
                            else
                            {
                                cXformPath = "/modules/" + cXformPath;
                            }
                        }
                        else
                        {
                            cXformPath = "/xforms/content/" + cXformPath;
                        }

                        // TS we want to do this later after we have loaded specs etc.
                        base.bProcessRepeats = false;

                        if (!base.load(cXformPath + ".xml", myWeb.maCommonFolders))
                        {
                            // load a default content xform if no alternative.
                            cProcessInfo = cXformPath + ".xml - Not Found";

                            base.NewFrm("EditContent");
                            base.submission("EditContent", "", "post", "form_check(this)");

                            oFrmElmt = base.addGroup(ref base.moXformElmt, "EditContent", "2Col", "Edit Content");
                            base.addNote("EditContent", Protean.xForm.noteTypes.Alert, "We do not have an XForm for this type of content: " + cXformPath);

                        }

                        if (id > 0L)
                        {
                            // here it would be really useful to merge nodes!
                            base.bProcessRepeats = true;
                            foreach (XmlElement NonTableInstanceElements in base.Instance)
                            {
                                if (NonTableInstanceElements.Name != "tblContent")
                                {
                                    // <Relation type="" direction="child" relatedContentId=""/>
                                    if (NonTableInstanceElements.Name == "Relation")
                                    {
                                        string sSql;
                                        if (NonTableInstanceElements.GetAttribute("direction").ToLower() == "child")
                                        {
                                            sSql = "Select nContentParentId from tblContentRelation where nContentChildId = " + id + " And cRelationType = '" + NonTableInstanceElements.GetAttribute("type") + "'";
                                        }
                                        else
                                        {
                                            sSql = "Select nContentChildId from tblContentRelation where nContentParentId = " + id + " AND cRelationType = '" + NonTableInstanceElements.GetAttribute("type") + "'";
                                        }
                                        using (var oRead = moDbHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                                        {
                                            string CSV = "";
                                            while (oRead.Read())
                                            {
                                                if (!string.IsNullOrEmpty(CSV))
                                                {
                                                    CSV = CSV + ",";
                                                }
                                                CSV = CSV + oRead.GetInt32(0).ToString();
                                            }
                                            NonTableInstanceElements.SetAttribute("relatedContentId", CSV);
                                        }
                                    }
                                    var newNode = oTempInstance.OwnerDocument.ImportNode(NonTableInstanceElements, true);
                                    oTempInstance.AppendChild(newNode);
                                }
                            }

                            base.updateInstance(oTempInstance);

                            // Add related content to the instance
                            oCRNode = moPageXML.CreateElement("ContentRelations");
                            moDbHelper.addRelatedContent(ref oCRNode, (int)id, true);
                            base.Instance.AppendChild(oCRNode);

                            if (bCopy)
                            {
                                oCRNode.SetAttribute("copyRelations", "true");

                                // select all nodes where content = cContentName
                                cContentName = base.Instance.SelectSingleNode("tblContent/cContentName").InnerText;
                                foreach (XmlElement oElmt in base.Instance.SelectNodes("descendant::*[.='" + cContentName + "']"))
                                {
                                    if ((oElmt.InnerText ?? "") == (cContentName ?? "") & oElmt.Name == "cContentName")
                                    {
                                        oElmt.InnerText = "Copy of " + cContentName;
                                    }
                                }
                                // set the id to zero
                                id = 0L;
                                base.Instance.SelectSingleNode("tblContent/nContentKey").InnerText = 0.ToString();
                                // remove any audit info, but keep the publish start and expire dates.
                                base.Instance.SelectSingleNode("tblContent/nAuditId").InnerText = "";
                                base.Instance.SelectSingleNode("tblContent/nAuditKey").InnerText = "";
                                base.Instance.SelectSingleNode("tblContent/dInsertDate").InnerText = "";
                                base.Instance.SelectSingleNode("tblContent/nInsertDirId").InnerText = "";
                                base.Instance.SelectSingleNode("tblContent/dUpdateDate").InnerText = "";
                                base.Instance.SelectSingleNode("tblContent/nUpdateDirId").InnerText = "";
                            }



                            var argoNode = base.Instance.SelectSingleNode("tblContent");
                            Xml.addNewTextNode("bCascade", ref argoNode, moDbHelper.isCascade(id).ToString().ToLower(), true, false);
                            if (pgid == 0L)
                            {
                                // lets go get a parId to set the pgid so we can update the cascade position
                                pgid = moDbHelper.getPrimaryLocationByArtId(id);
                            }
                        }
                        else
                        {

                            XmlElement myInstance = base.Instance;

                            if (sProductTypes.Contains(cContentSchemaName + ","))
                            {
                                AddPageSpecs(ref myWeb.mnPageId, ref myInstance);
                            }
                            bProcessRepeats = true;
                            LoadInstance(myInstance);

                            if (goSession["oContentInstance"] != null)
                            {
                                bProcessRepeats = true;
                                base.Instance = (XmlElement)goSession["oContentInstance"];
                            }

                            if (!string.IsNullOrEmpty(cContentName) & base.Instance.FirstChild != null)
                            {
                                if (base.Instance.SelectSingleNode("tblContent/cContentSchemaName") != null)
                                {
                                    if (base.Instance.SelectSingleNode("tblContent/cContentSchemaName").InnerText.ToLower() != "filter")
                                    {
                                        base.Instance.SelectSingleNode("tblContent/cContentName").InnerText = cContentName;
                                    }
                                }

                                base.Instance.SelectSingleNode("tblContent/dPublishDate").InnerText = XmlDate(DateTime.Now);
                            }

                            // we are adding orphan content so cannot cascade
                            // remove any cascade radio on form
                            if (pgid == 0L)
                            {
                                foreach (XmlElement oElmt in base.moXformElmt.SelectNodes("descendant-or-self::*[@bind='bCascade']"))
                                    oElmt.ParentNode.RemoveChild(oElmt);
                            }
                            else if (base.Instance.FirstChild != null)
                            {
                                var argoNode1 = base.Instance.SelectSingleNode("tblContent");
                                Xml.addNewTextNode("bCascade", ref argoNode1, "", true, false);
                            }



                        }

                        // Check for adding the integration checkbox
                        var argform = this;
                        integrationHelper.PostContentCheckboxes(ref argform, cContentSchemaName, id > 0L);

                        // Process any location selects
                        Cms.xForm argForm = (Cms.xForm)this;
                        oContentLocations = new xFormContentLocations(id, ref argForm);
                        oContentLocations.ProcessSelects();

                        // Version Control: if on, copy the status node for use after submission
                        if (myWeb.gbVersionControl)
                        {
                            string nCurrentStatus = "";
                            XmlNodeState localNodeState() { var argoNode3 = base.Instance; var ret = Xml.NodeState(ref argoNode3, "//nStatus", "", "", XmlNodeState.IsEmpty, null, "", nCurrentStatus, bCheckTrimmedInnerText: false); base.Instance = argoNode3; return ret; }

                            if (localNodeState() == XmlNodeState.HasContents)
                            {
                                XmlNode argoNode2 = (XmlNode)base.Instance;
                                addNewTextNode("currentStatus", ref argoNode2, nCurrentStatus);
                                base.Instance = (XmlElement)argoNode2;
                            }
                        }

                        // Additional Processing : Post Build
                        xFrmEditContentPostBuildProcessing(cContentSchemaName);

                        if (base.isSubmitted())
                        {

                            // Additional Processing : Pre Submission 
                            xFrmEditContentSubmissionPreProcessing();

                            base.updateInstanceFromRequest();
                            base.validate();

                            if (base.valid)
                            {

                                bool bPreviewRedirect = false;

                                if (!string.IsNullOrEmpty(goRequest["ptn-preview"]))
                                {
                                    if (myWeb.gbVersionControl)
                                    {
                                        // Leave the current version unchanged and live

                                        // create a new version of the content as pending
                                        base.Instance.SelectSingleNode("tblContent/nStatus").InnerText = ((int)Cms.dbHelper.Status.InProgress).ToString();

                                        // redirect to preview version in preview mode
                                        bPreviewRedirect = true;
                                    }
                                }

                                Cms.dbHelper.ActivityType editResult = (Cms.dbHelper.ActivityType)default;

                                // we don't need this now.
                                goSession["oContentInstance"] = (object)null;

                                // trim the contentName to no longer than 255 chars
                                string contentName = base.Instance.SelectSingleNode("*/cContentName").InnerXml;
                                if (contentName.Length > 255)
                                {
                                    contentName = contentName.Substring(0, 255);
                                }
                                base.Instance.SelectSingleNode("*/cContentName").InnerXml = contentName;

                                // remove any invalid charactors from the contentName
                                var oUrlExp = new Regex(@"[^\w\-\u0020\+]");
                                base.Instance.SelectSingleNode("*/cContentName").InnerXml = oUrlExp.Replace(base.Instance.SelectSingleNode("*/cContentName").InnerXml, "");
                                oUrlExp = null;
                                if (base.Instance.SelectSingleNode("*/bCascade") != null)
                                {
                                    if (base.Instance.SelectSingleNode("*/bCascade").InnerXml == "true")
                                    {
                                        bCascade = true;
                                    }
                                }

                                sProductTypes = sProductTypes.Trim().TrimEnd(',') + ",";
                                if (sProductTypes.Contains(cContentSchemaName + ","))
                                {
                                    XmlElement thisInstance = base.Instance;
                                    DelEmptySpecs(ref thisInstance);
                                    base.Instance = thisInstance;
                                }

                                if (id > 0L)
                                {

                                    object updatedVersionId = moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Content, base.Instance);

                                    moDbHelper.CommitLogToDB(Cms.dbHelper.ActivityType.ContentEdited, myWeb.mnUserId, myWeb.moSession.SessionID, DateTime.Now, (int)id, (int)pgid, "");
                                    // Redirection 
                                    string redirectType = "";
                                    string newUrl = "";
                                    string strOldurl = "";
                                    if (moRequest["redirectType"] != null)
                                    {
                                        redirectType = moRequest["redirectType"].ToString();
                                    }

                                    if (moRequest["productNewUrl"] != null)
                                    {
                                        newUrl = moRequest["productNewUrl"].ToString();
                                    }
                                    if (moRequest["productOldUrl"] != null)
                                    {
                                        strOldurl = moRequest["productOldUrl"].ToString();
                                    }

                                    // Individual content location set
                                    // Don't set a location if a contentparid has been passed (still process content locations as tickboexs on the form, if they've been set)
                                    if (!(myWeb.moRequest["contentParId"] != null & !string.IsNullOrEmpty(myWeb.moRequest["contentParId"])))
                                    {

                                        // TS 28-11-2017 we only want to update the cascade information if the content is on this page.
                                        // If not on this page i.e. being edited via search results or related content on a page we should ignore this.
                                        if (Convert.ToDouble(moDbHelper.ExeProcessSqlScalar("select count(nContentLocationKey) from tblContentLocation where nContentId=" + id + " and nStructId = " + pgid)) > 0d)
                                        {
                                            moDbHelper.setContentLocation(pgid, id, bCascade: bCascade, cPosition: "");
                                        }
                                    }

                                    // TS 10-01-2014 fix for cascade on saved items... To Be tested
                                    if (bCascade & pgid > 0L)
                                    {
                                        moDbHelper.setContentLocation(pgid, id, true, bCascade);
                                    }

                                    editResult = Cms.dbHelper.ActivityType.ContentEdited;

                                    if (updatedVersionId != null && !updatedVersionId.Equals(id))
                                    {
                                        nReturnId = Convert.ToInt64(updatedVersionId);
                                    }
                                    else
                                    {
                                        nReturnId = (int)id;
                                    }
                                }

                                else
                                {
                                    long nContentId;
                                    nContentId = Convert.ToInt64(moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Content, base.Instance));
                                    moDbHelper.CommitLogToDB(Cms.dbHelper.ActivityType.ContentAdded, myWeb.mnUserId, myWeb.moSession.SessionID, DateTime.Now, (int)nContentId, (int)pgid, "");


                                    // If we have an action here we need to relate the item
                                    if (goSession["mcRelAction"] != null && (goSession["mcRelAction"].ToString() == "Add" || goSession["mcRelAction"].ToString() == "Find" || goSession["mcRelAction"].ToString() == "Edit"))
                                    {
                                        bool b2Way = (moRequest["RelType"] == "2way" | moRequest["direction"] == "2Way") ? true : false;
                                        string sRelType = moRequest["relationType"];
                                        moDbHelper.insertContentRelation(Convert.ToInt32(goSession["mcRelParent"]), nContentId.ToString(), b2Way, sRelType);
                                    }
                                    // TS - Change 26/04/2016 We do not want added to the page if it is related.

                                    // Individual content location set
                                    // Don't set a location if a contentparid has been passed (still process content locations as tickboexs on the form, if they've been set)
                                    else if (!(myWeb.moRequest["contentParId"] != null & !string.IsNullOrEmpty(myWeb.moRequest["contentParId"])))
                                    {
                                        moDbHelper.setContentLocation(pgid, nContentId, true, bCascade, cPosition: moRequest["cPosition"]);
                                    }


                                    editResult = Cms.dbHelper.ActivityType.ContentAdded;

                                    // If this is a new element then we need to add the related content to the instance to be handled in processInstanceExtras
                                    nReturnId = (int)nContentId;
                                    foreach (var item in moRequest.Form)
                                    {
                                        if (Convert.ToString(item).StartsWith("Relate_"))
                                        {
                                            string[] arr = Convert.ToString(item).Split('_');
                                            var relateElmt = moPageXML.CreateElement("Relation");
                                            relateElmt.SetAttribute("relatedContentId", moRequest.Form[Convert.ToString(item)]);
                                            relateElmt.SetAttribute("type", arr[1]);
                                            relateElmt.SetAttribute("direction", arr[2]);
                                            base.Instance.AppendChild(relateElmt);
                                        }
                                    }

                                }

                                // TS Added 24-11-2014 to allow content forms to add related content from dropdowns.
                                moDbHelper.processInstanceExtras((long)nReturnId, base.Instance, false, false);


                                // Check for related content redirection
                                string mcRelRedirectString = Convert.ToString(goSession["mcRelRedirectString"]);
                                if (base.valid && !string.IsNullOrEmpty(mcRelRedirectString))
                                {

                                    string cQueryString = goRequest.QueryString.ToString();
                                    if (cQueryString.IndexOf("ewCmd=") > 0)
                                    {
                                        // here we fail because the ? is an & >=[
                                        if (cQueryString.IndexOf("&") > 0)
                                        {
                                            // So we have the index of the first &, how to replace?!
                                        }
                                    }
                                    else
                                    {
                                        cQueryString = "?" + goRequest.QueryString.ToString().Replace("path=", "");
                                    }
                                    if (cQueryString.IndexOf("ewCmd=") != -1)
                                    {
                                        cQueryString = cQueryString.Substring(cQueryString.IndexOf("ewCmd="));
                                    }
                                    if (cQueryString.IndexOf("ajaxCmd=") != -1)
                                    {
                                        cQueryString = cQueryString.Substring(cQueryString.IndexOf("ajaxCmd="));
                                    }
                                    mcRelRedirectString = mcRelRedirectString.Substring(mcRelRedirectString.IndexOf("ewCmd="));

                                    if ((mcRelRedirectString.ToLower() ?? "") == (cQueryString.ToLower() ?? ""))
                                    {
                                        // Suppress last page being reset anywhere else
                                        myWeb.mbSuppressLastPageOverrides = true;
                                        myWeb.moSession.Remove("lastPage");
                                        myWeb.msRedirectOnEnd = Convert.ToString(goSession["mnContentRelationParent"]);
                                    }

                                }

                                goSession["mnContentRelationParent"] = (object)null;
                                goSession["mcRelRedirectString"] = (object)null;
                                goSession["mcRelAction"] = (object)null;
                                goSession["mcRelParent"] = (object)null;

                                if (goSession["EwCmd"] == null || goSession["EwCmd"].ToString() == "")
                                {
                                    goSession["EwCmd"] = "Normal";
                                }

                                // Submitted and valid - should have a content id let's process the relationships
                                oContentLocations.ProcessRequest(nReturnId);

                                // User Integrations check
                                if (integrationHelper.Enabled)
                                {
                                    integrationHelper.PostContent((long)nReturnId);


                                    // Module content edit action handler
                                }

                                foreach (XmlElement contentEditActionHandler in base.Instance.SelectNodes("//Content[@editAction]"))
                                {
                                    string contentEditAction = contentEditActionHandler.GetAttribute("editAction");
                                    if (!string.IsNullOrEmpty(contentEditAction))
                                    {

                                        string assemblyName = contentEditActionHandler.GetAttribute("assembly");
                                        string assemblyType = contentEditActionHandler.GetAttribute("assemblyType");
                                        string providerName = contentEditActionHandler.GetAttribute("providerName");
                                        string providerType = contentEditActionHandler.GetAttribute("providerType");
                                        if (string.IsNullOrEmpty(providerType))
                                            providerType = "messaging";


                                        string methodName = contentEditAction;
                                        string classPath = "";

                                        if (methodName.Contains("."))
                                        {
                                            int lastDotIndex = contentEditAction.LastIndexOf(".");
                                            methodName = contentEditAction.Substring(lastDotIndex + 1);
                                            classPath = contentEditAction.Substring(0, lastDotIndex);
                                        }

                                        // Dim providerSection As String = Coalesce(contentEditActionHandler.GetAttribute("providerSection"), "eonic/" & providerType & "Providers")

                                        // Edit Action method constructor follows the following format:
                                        // 1 - Protean.Cms
                                        // 2 - Content XML
                                        // 3 - Content ID being editted
                                        // 4 - Content action 

                                        try
                                        {
                                            Type calledType;

                                            if (!string.IsNullOrEmpty(assemblyName))
                                            {
                                                contentEditAction = contentEditAction + ", " + assemblyName;
                                            }
                                            // Dim oModules As New Protean.Cms.Membership.Modules

                                            if (!string.IsNullOrEmpty(providerName))
                                            {
                                                // case for external Providers
                                                Protean.ProviderSectionHandler moPrvConfig = (Protean.ProviderSectionHandler)WebConfigurationManager.GetWebApplicationSection("protean/" + providerType + "Providers");
                                                Assembly assemblyInstance;

                                                if (moPrvConfig.Providers[providerName + "Local"] != null)
                                                {
                                                    if (!string.IsNullOrEmpty(moPrvConfig.Providers[providerName + "Local"].Parameters["path"]))
                                                    {
                                                        assemblyInstance = Assembly.LoadFrom(goServer.MapPath(moPrvConfig.Providers[providerName + "Local"].Parameters["path"]));
                                                        calledType = assemblyInstance.GetType(contentEditAction, true);
                                                    }
                                                    else
                                                    {
                                                        assemblyInstance = Assembly.Load(moPrvConfig.Providers[providerName + "Local"].Type);
                                                        calledType = assemblyInstance.GetType(contentEditAction, true);
                                                    }
                                                }
                                                else
                                                {
                                                    switch (moPrvConfig.Providers[providerName].Parameters["path"] ?? "")
                                                    {
                                                        case var @case when @case == "":
                                                            {
                                                                assemblyInstance = Assembly.Load(moPrvConfig.Providers[providerName].Type);
                                                                calledType = assemblyInstance.GetType(contentEditAction, true);
                                                                break;
                                                            }
                                                        case "builtin":
                                                            {
                                                                string prepProviderName; // = Replace(moPrvConfig.Providers(providerName).Type, ".", "+")
                                                                                         // prepProviderName = (New Regex("\+")).Replace(prepProviderName, ".", 1)
                                                                prepProviderName = moPrvConfig.Providers[providerName].Type;
                                                                calledType = Type.GetType(prepProviderName + "+" + classPath, true);
                                                                break;
                                                            }

                                                        default:
                                                            {
                                                                assemblyInstance = Assembly.LoadFrom(goServer.MapPath(moPrvConfig.Providers[providerName].Parameters["path"]));

                                                                classPath = moPrvConfig.Providers[providerName].Parameters["classPrefix"] + classPath;
                                                                calledType = assemblyInstance.GetType(classPath, true);
                                                                break;
                                                            }
                                                    }

                                                }
                                            }
                                            else if (!string.IsNullOrEmpty(assemblyType))
                                            {
                                                // case for external DLL's
                                                var assemblyInstance = Assembly.Load(assemblyType);
                                                calledType = assemblyInstance.GetType(contentEditAction, true);
                                            }
                                            else
                                            {
                                                // case for methods within EonicWeb Core DLL
                                                calledType = Type.GetType(contentEditAction, true);
                                            }

                                            var o = Activator.CreateInstance(calledType);

                                            var args = new object[4];
                                            args[0] = myWeb;
                                            args[1] = contentEditActionHandler;
                                            args[2] = nReturnId;
                                            args[3] = editResult;

                                            calledType.InvokeMember(methodName, BindingFlags.InvokeMethod, null, o, args);

                                            // Error Handling ?
                                            // Object Clearup ?

                                            calledType = null;

                                            // Update again ?
                                            base.Instance.SelectSingleNode("*/nContentPrimaryId").InnerText = 0.ToString();
                                            moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Content, base.Instance, (long)nReturnId);
                                        }

                                        catch (Exception)
                                        {
                                            // OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(_moduleName, "ContentActions", ex, sProcessInfo))
                                            cProcessInfo = assemblyName + "." + contentEditAction + " not found";

                                        }
                                    }
                                }

                                if (bPreviewRedirect)
                                {
                                    //long VerId = 0L;
                                    myWeb.msRedirectOnEnd = "/?ewCmd=PreviewOn&pgid=" + pgid + "&artid=" + id + "&verId=" + nReturnId;
                                }

                            }
                        }
                        else if (isSubmittedOther((int)pgid)) // has another specific submit button been pressed?
                        {
                            // This should really be taken over using  xForms Triggers
                            base.updateInstanceFromRequest();
                            if (goSession["mcRelRedirectString"] != null && goSession["mcRelRedirectString"].ToString() != "")
                            {
                                base.validate();
                                if (base.valid)
                                {
                                    myWeb.msRedirectOnEnd = Convert.ToString(goSession["mcRelRedirectString"]);
                                    base.valid = false;
                                }
                            }
                            else
                            {
                                // we are re-ordering so we don't want a valid form
                                base.valid = false;
                            }
                            goSession["oContentInstance"] = (object)null;
                        }

                        else if (base.isTriggered)
                        {
                            // we have clicked a trigger so we must update the instance
                            base.updateInstanceFromRequest();
                            // lets save the instance
                            goSession["oContentInstance"] = base.Instance;
                        }
                        else
                        {
                            // clear this if we are loading the first form
                            goSession["oContentInstance"] = (object)null;
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmEditContent", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                public virtual void xFrmEditContentPostBuildProcessing(string cContentSchemaName)
                {
                    // Holding function for adding proecssing after building the form.
                }

                public virtual void xFrmEditContentSubmissionPreProcessing()
                {
                    // Holding function for adding pre proecssing on submitting this form.
                }


                public void AddPageSpecs(ref long nPgId, ref XmlElement Instance)
                {

                    if (Instance.SelectSingleNode("descendant-or-self::Specs") != null)
                    {

                        Protean.Cms myCMS = new Protean.Cms(myWeb.moCtx);
                        myCMS.InitializeVariables();
                        myCMS.mnPageId = nPgId;
                        myCMS.ibIndexMode = true;
                        myCMS.mbAdminMode = false;
                        XmlDocument myPageXml = myCMS.GetPageXML();

                        XmlElement SpecsElmt = myPageXml.CreateElement("Specs");
                        foreach (XmlElement SpecElmt in myPageXml.SelectNodes("descendant-or-self::Spec"))
                        {
                            string name = SpecElmt.GetAttribute("name");
                            if (name != "")
                            {
                                if (SpecsElmt.SelectSingleNode($"Spec[@name='{name}']") == null)
                                {
                                    SpecElmt.InnerText = "";
                                    XmlElement existingSpec = (XmlElement)Instance.SelectSingleNode($"descendant-or-self::Spec[@name='{name}']");
                                    if (existingSpec != null && existingSpec.InnerText != "")
                                    {
                                        SpecElmt.InnerText = existingSpec.InnerText;
                                    }
                                    else
                                    {
                                        SpecElmt.SetAttribute("noDel", "true");
                                    }
                                    SpecsElmt.AppendChild(SpecElmt);
                                }
                            }
                        }

                        foreach (XmlNode InstanceSpecs in Instance.SelectNodes("descendant-or-self::Specs"))
                        {
                            InstanceSpecs.InnerXml = SpecsElmt.InnerXml;
                        }
                    }
                }

                public void DelEmptySpecs(ref XmlElement Instance)
                {
                    // removes empty specs from the instance
                    if (Instance.SelectSingleNode("descendant-or-self::Specs") != null)
                    {
                        foreach (XmlNode InstanceSpecs in Instance.SelectNodes("descendant-or-self::Specs"))
                        {
                            if (InstanceSpecs.InnerXml == "")
                            {
                                InstanceSpecs.ParentNode.RemoveChild(InstanceSpecs);
                            }
                        }
                    }
                }


                public XmlElement xFrmDeleteContent(long artid)
                {

                    XmlElement oFrmElmt;
                    string sContentName;
                    string sContentSchemaName;

                    string cProcessInfo = "";

                    try
                    {
                        // load the xform to be edited
                        moDbHelper.moPageXml = moPageXML;

                        sContentName = moDbHelper.getNameByKey(Cms.dbHelper.objectTypes.Content, artid);
                        sContentSchemaName = moDbHelper.getContentType((int)artid);

                        base.NewFrm("DeleteContent");

                        base.submission("DeleteContent", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "DeleteItem", "", "Delete Content");

                        //XmlNode argoNode = oFrmElmt;
                        base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "Are you sure you want to delete this item - \"" + encodeAllHTML(sContentName) + "\"", false, "alert-danger");
                        //oFrmElmt = (XmlElement)argoNode;
                        if (sContentSchemaName == "xFormQuiz")
                        {
                            //XmlNode argoNode1 = oFrmElmt;
                            base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "By deleting the Exam you will also delete all the user results from the database \"ARE YOU SURE\" !", false, "alert-danger");
                            //oFrmElmt = (XmlElement)argoNode1;

                        }
                        base.addSubmit(ref oFrmElmt, "", "Delete " + sContentSchemaName, sClass: "principle btn-danger", sIcon: "fa-trash-o");

                        base.Instance.InnerXml = "<delete/>";

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {

                                // remove the relevent content information
                                moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.Content, artid);
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


                public XmlElement xFrmContentLocationDetail(int nPageId, int nContentId)
                {
                    XmlElement oFrmElmt;


                    string cProcessInfo = "";
                    try
                    {
                        base.NewFrm("EditContent");

                        base.submission("EditContent", "", "post", "");

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "RelationshipType", "", "Please select a relationship type.");
                        var oSelElmt = base.addSelect1(ref oFrmElmt, "nRelType", true, "Type", "required", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "Primary", 1.ToString());
                        base.addOption(ref oSelElmt, "Link", 0.ToString());
                        XmlElement argoBindParent = null;
                        base.addBind("nRelType", "nRelType", oBindParent: ref argoBindParent, "true()", "number");

                        base.addSubmit(ref oFrmElmt, "Save", "Save Changes");
                        int nPrimary = 0;
                        string cRes = moDbHelper.ExeProcessSqlScalar("Select bPrimary from tblContentLocation where nStructId = " + nPageId + " and nContentId = " + nContentId).ToLower();
                        if (cRes == "true")
                        {
                            nPrimary = 1;
                        }
                        base.Instance.InnerXml = "<nRelType>" + nPrimary + "</nRelType>";

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                bool bResult = (Convert.ToDouble(base.Instance.SelectSingleNode("nRelType").InnerText) == 1d) ? true : false;
                                bResult = moDbHelper.updateLocationsDetail((long)nContentId, nPageId, bResult);
                                valid = true;
                                if (!bResult)
                                {
                                    //XmlNode argoNode = oFrmElmt;
                                    base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Hint, "Cannot remove the only Primary Relationship", true);
                                    //oFrmElmt = (XmlElement)argoNode;
                                    valid = false;
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
                        stdTools.returnException(ref myWeb.msException, _moduleName, "addInput", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }


                public XmlElement xFrmLocateContent(int nNewLocationPage, int nFromPage, string bIncludeChildren, string cContentType, string cSearchTerm)
                {
                    // if nNewLocationPage =0 or nFromPage=0 or cContentType="" then 
                    XmlElement oFrmElmt;
                    XmlElement oGrp1Elmt;
                    XmlElement oSelElmt2;
                    var oTempInstance = moPageXML.CreateElement("instance");
                    //bool bCascade = false;
                    string cProcessInfo = "";

                    try
                    {
                        base.NewFrm("SelectContentToLocate");
                        base.submission("SelectLocation", "", "post", "");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "Content");
                        var oGrp0Elmt = base.addGroup(ref oFrmElmt, "ResultsHeader", sLabel: "&#160;");

                        base.addSubmit(ref oFrmElmt, "Locate", "Add To Page", "Locate");

                        if (base.isSubmitted())
                        {

                            string[] oItems = myWeb.moRequest["Results"].Split(',');
                            string cPosition = myWeb.moRequest["Position"];
                            int i = 0;
                            var loopTo = oItems.Length - 1;
                            for (i = 0; i <= loopTo; i++)
                                myWeb.moDbHelper.setContentLocation((long)nNewLocationPage, Convert.ToInt64(oItems[i]), false, false, false);
                            base.valid = true;
                        }
                        else
                        {


                            var oResults = moDbHelper.LocateContentSearch(nFromPage, cContentType, bIncludeChildren, cSearchTerm, nNewLocationPage);
                            int nCount = 0;
                            if (oResults != null)
                                nCount = oResults.Count;

                            oGrp0Elmt.SelectSingleNode("label").InnerText = "Results (" + nCount + ")";

                            oGrp1Elmt = base.addGroup(ref oGrp0Elmt, "Results", "horizontal ", "");
                            // nNewLocation
                            var oElement = base.Instance.OwnerDocument.CreateElement("nNewLocationPage");
                            oElement.InnerText = nNewLocationPage.ToString();
                            base.Instance.AppendChild(oElement);

                            // nFromPage
                            oElement = base.Instance.OwnerDocument.CreateElement("nFromPage");
                            oElement.InnerText = nFromPage.ToString();
                            base.Instance.AppendChild(oElement);
                            base.addInput(ref oGrp1Elmt, "nNewLocationPage", true, "nNewLocationPage", "hidden").SetAttribute("value", nNewLocationPage.ToString());
                            XmlElement argoBindParent = null;
                            base.addBind("nNewLocationPage", "nNewLocationPage", oBindParent: ref argoBindParent);

                            // bIncludeChildren
                            oElement = base.Instance.OwnerDocument.CreateElement("bIncludeChildren");
                            oElement.InnerText = bIncludeChildren;
                            base.Instance.AppendChild(oElement);
                            base.addInput(ref oGrp1Elmt, "type", true, "type", "hidden").SetAttribute("value", cContentType);
                            XmlElement argoBindParent1 = null;
                            base.addBind("type", "type", oBindParent: ref argoBindParent1);

                            // cContentType 
                            oElement = base.Instance.OwnerDocument.CreateElement("cContentType");
                            oElement.InnerText = cContentType;
                            base.Instance.AppendChild(oElement);
                            base.addInput(ref oGrp1Elmt, "nFromPage", true, "nFromPage", "hidden").SetAttribute("value", nFromPage.ToString());
                            XmlElement argoBindParent2 = null;
                            base.addBind("nFromPage", "nFromPage", oBindParent: ref argoBindParent2);
                            base.addInput(ref oGrp1Elmt, "bIncludeChildren", true, "bIncludeChildren", "hidden").SetAttribute("value", bIncludeChildren);
                            XmlElement argoBindParent3 = null;
                            base.addBind("bIncludeChildren", "bIncludeChildren", oBindParent: ref argoBindParent3);

                            // cSearchTerm
                            oElement = base.Instance.OwnerDocument.CreateElement("cSearchTerm");
                            oElement.InnerText = cSearchTerm;
                            base.Instance.AppendChild(oElement);
                            base.addInput(ref oGrp1Elmt, "cSearchTerm", true, "cSearchTerm", "hidden").SetAttribute("value", cSearchTerm);
                            XmlElement argoBindParent4 = null;
                            base.addBind("cSearchTerm", "cSearchTerm", oBindParent: ref argoBindParent4);

                            // cSearched
                            oElement = base.Instance.OwnerDocument.CreateElement("cSearched");
                            oElement.InnerText = "1";
                            base.Instance.AppendChild(oElement);
                            base.addInput(ref oGrp1Elmt, "cSearched", true, "cSearched", "hidden").SetAttribute("value", "1");
                            XmlElement argoBindParent5 = null;
                            base.addBind("cSearched", "cSearched", oBindParent: ref argoBindParent5);

                            base.addSubmit(ref oGrp1Elmt, "Locate", "Add To Page", "Locate");
                            oSelElmt2 = base.addSelect(ref oGrp1Elmt, "Results", true, "", "radiocheckbox multiline selectAll content", Protean.xForm.ApperanceTypes.Full);
                            if (oResults != null)
                            {
                                foreach (XmlElement oResult in oResults)
                                {
                                    if (oSelElmt2.SelectSingleNode("item[value/node()='" + oResult.GetAttribute("id") + "']") is null)
                                    {
                                        base.addOption(ref oSelElmt2, oResult.OuterXml, oResult.GetAttribute("id"), true);
                                    }
                                }
                            }

                            base.addValues();
                        }
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmLocateContent", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }


                public XmlElement xFrmDeleteBulkContent(params string[] artid)
                {

                    XmlElement oFrmElmt;
                    string sContentName;
                    string sContentSchemaName;

                    string cProcessInfo = "";
                    string bulkContentName = "";
                    string bulkContentSchemaName = "";

                    try
                    {
                        // load the xform to be edited
                        moDbHelper.moPageXml = moPageXML;

                        base.NewFrm("DeleteContent");

                        base.submission("DeleteContent", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "DeleteItem", "", "Delete Content");
                        //XmlNode argoNode = oFrmElmt;
                        base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "Are you sure you want to delete below items ?", false, "alert-danger");
                        //oFrmElmt = (XmlElement)argoNode;
                        for (int i = 0, loopTo = artid.Length - 1; i <= loopTo; i++)
                        {
                            sContentName = moDbHelper.getNameByKey(Cms.dbHelper.objectTypes.Content, Convert.ToInt64(artid[i]));
                            sContentSchemaName = moDbHelper.getContentType(Convert.ToInt16(artid[i]));
                            bulkContentName = encodeAllHTML(sContentName);
                            //XmlNode argoNode1 = oFrmElmt;
                            base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, bulkContentName, false, "item-deleted");
                            // oFrmElmt = (XmlElement)argoNode1;
                            oFrmElmt.LastChild.InnerXml = moDbHelper.getContentBrief(Convert.ToInt16(artid[i]));
                            if (sContentSchemaName == "xFormQuiz")
                            {
                                //XmlNode argoNode2 = oFrmElmt;
                                base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "By deleting the Exam you will also delete all the user results from the database \"ARE YOU SURE\" !", false, "alert-danger");
                                //oFrmElmt = (XmlElement)argoNode2;
                            }
                            bulkContentSchemaName = encodeAllHTML(sContentSchemaName) + " , ";
                        }
                        bulkContentSchemaName = bulkContentSchemaName.Trim(' ').Trim(',').Trim(' ');
                        base.addSubmit(ref oFrmElmt, "", "Delete", sClass: "principle btn-danger", sIcon: "fa-trash-o");

                        base.Instance.InnerXml = "<delete/>";

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {

                                // remove the relevent content information
                                for (int i = 0, loopTo1 = artid.Length - 1; i <= loopTo1; i++)
                                {
                                    sContentName = moDbHelper.getNameByKey(Cms.dbHelper.objectTypes.Content, Convert.ToInt64(artid[i]));
                                    sContentSchemaName = moDbHelper.getContentType(Convert.ToInt16(artid[i]));
                                    moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.Content, Convert.ToInt64(artid[i]));
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
            }
        }
    }
}