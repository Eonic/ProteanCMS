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

                private bool startImp()
                {
                    try
                    {
                        if (Convert.ToBoolean(myWeb.impersonationMode))
                        {
                            moImp = new Tools.Security.Impersonate();
                            return moImp.ImpersonateValidUser(goConfig["AdminAcct"], goConfig["AdminDomain"], goConfig["AdminPassword"], cInGroup: goConfig["AdminGroup"]);
                        }
                        else
                        {
                            return default;
                        }
                    }

                    catch (Exception)
                    {
                        return false;
                    }
                }

                private void endImp()
                {
                    try
                    {
                        if (Convert.ToBoolean(myWeb.impersonationMode))
                        {
                            moImp.UndoImpersonation();
                            moImp = null;
                        }
                    }

                    catch (Exception)
                    {

                    }
                }

                public XmlElement xFrmWebSettings()
                {
                    XmlElement oFrmElmt;
                    XmlElement oSelElmt;
                    string cProcessInfo = "";
                    Protean.fsHelper oFsh;

                    try
                    {
                        oFsh = new Protean.fsHelper();
                        oFsh.open(moPageXML);

                        base.NewFrm("WebSettings");

                        base.submission("WebSettings", "", "post", "form_check(this)");

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "WebSettings", "", "Web Settings");

                        // oSelElmt = MyBase.addSelect1(oFrmElmt, "ewDatabaseType", True, "DB Type", "", ApperanceTypes.Full)
                        // MyBase.addOption(oSelElmt, "MS SQL", "SQL")
                        // MyBase.addOption(oSelElmt, "MS Access", "Access")
                        // MyBase.addBind("ewDatabaseType", "web/add[@key='DatabaseType']/@value", "true()")

                        // MyBase.addInput(oFrmElmt, "ewDatabaseServer", True, "DB Server")
                        // MyBase.addBind("ewDatabaseServer", "web/add[@key='DatabaseServer']/@value", "true()")

                        //XmlNode argoNode = oFrmElmt;
                        base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "Any Changes you make to this form risk making this site completely non-functional. Please be sure you know what you are doing before making any changes, or call your web developer for support");
                        //oFrmElmt = (XmlElement)argoNode;

                        base.addInput(ref oFrmElmt, "ewDatabaseName", true, "DB Name");
                        XmlElement argoBindParent = null;
                        base.addBind("ewDatabaseName", "web/add[@key='DatabaseName']/@value", oBindParent: ref argoBindParent, "true()");

                        // MyBase.addInput(oFrmElmt, "ewDatabaseAuth", True, "DB Auth")
                        // MyBase.addBind("ewDatabaseAuth", "web/add[@key='DatabaseAuth']/@value", "false()")

                        base.addInput(ref oFrmElmt, "ewDatabaseUsername", true, "DB Username");
                        XmlElement argoBindParent1 = null;
                        base.addBind("ewDatabaseUsername", "web/add[@key='DatabaseUsername']/@value", oBindParent: ref argoBindParent1, "false()");

                        base.addInput(ref oFrmElmt, "ewDatabasePassword", true, "DB Passowrd");
                        XmlElement argoBindParent2 = null;
                        base.addBind("ewDatabasePassword", "web/add[@key='DatabasePassword']/@value", oBindParent: ref argoBindParent2, "false()");

                        // oSelElmt = MyBase.addSelect1(oFrmElmt, "ewSiteXsl", True, "Site Scheme", "", ApperanceTypes.Minimal)
                        // ' MyBase.addOptionsFilesFromDirectory(oSelElmt, "/ewcommon/xsl/scheme", ".xsl")
                        // MyBase.addOption(oSelElmt, "standard.xsl [bespoke]", "/xsl/standard.xsl")
                        // MyBase.addBind("ewSiteXsl", "web/add[@key='SiteXsl']/@value", "false()")

                        base.addInput(ref oFrmElmt, "ewRootPageId", true, "Root Page Id");
                        XmlElement argoBindParent3 = null;
                        base.addBind("ewRootPageId", "web/add[@key='RootPageId']/@value", oBindParent: ref argoBindParent3, "true()");

                        base.addInput(ref oFrmElmt, "ewBaseUrl", true, "Base URL");
                        XmlElement argoBindParent4 = null;
                        base.addBind("ewBaseUrl", "web/add[@key='BaseUrl']/@value", oBindParent: ref argoBindParent4, "false()");

                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewDebug", true, "Debug", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "On", "on");
                        base.addOption(ref oSelElmt, "Off", "off");
                        XmlElement argoBindParent5 = null;
                        base.addBind("ewDebug", "web/add[@key='Debug']/@value", oBindParent: ref argoBindParent5, "true()");

                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewCompiledTransform", true, "Compiled Transform", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "On", "on");
                        base.addOption(ref oSelElmt, "Off", "off");
                        XmlElement argoBindParent6 = null;
                        base.addBind("ewCompiledTransform", "web/add[@key='CompiledTransform']/@value", oBindParent: ref argoBindParent6, "true()");

                        base.addInput(ref oFrmElmt, "ewImageRootPath", true, "Images Directory");
                        XmlElement argoBindParent7 = null;
                        base.addBind("ewImageRootPath", "web/add[@key='ImageRootPath']/@value", oBindParent: ref argoBindParent7, "true()");

                        base.addInput(ref oFrmElmt, "ewDocRootPath", true, "Docs Directory");
                        XmlElement argoBindParent8 = null;
                        base.addBind("ewDocRootPath", "web/add[@key='DocRootPath']/@value", oBindParent: ref argoBindParent8, "true()");

                        base.addInput(ref oFrmElmt, "ewMediaRootPath", true, "Media Directory");
                        XmlElement argoBindParent9 = null;
                        base.addBind("ewMediaRootPath", "web/add[@key='MediaRootPath']/@value", oBindParent: ref argoBindParent9, "true()");

                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewCart", true, "Shopping Cart", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "On", "on");
                        base.addOption(ref oSelElmt, "Off", "off");
                        XmlElement argoBindParent10 = null;
                        base.addBind("ewCart", "web/add[@key='Cart']/@value", oBindParent: ref argoBindParent10, "true()");

                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewMembership", true, "Membership", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "On", "on");
                        base.addOption(ref oSelElmt, "Off", "off");
                        XmlElement argoBindParent11 = null;
                        base.addBind("ewMembership", "web/add[@key='Membership']/@value", oBindParent: ref argoBindParent11, "true()");

                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewMailingList", true, "Mailing List", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "On", "on");
                        base.addOption(ref oSelElmt, "Off", "off");
                        XmlElement argoBindParent12 = null;
                        base.addBind("ewMailingList", "web/add[@key='MailingList']/@value", oBindParent: ref argoBindParent12, "true()");

                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewSubscriptions", true, "Subscriptions", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "On", "on");
                        base.addOption(ref oSelElmt, "Off", "off");
                        XmlElement argoBindParent13 = null;
                        base.addBind("ewSubscriptions", "web/add[@key='Subscriptions']/@value", oBindParent: ref argoBindParent13, "true()");

                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewActivityLogging", true, "Activity Logging", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "On", "on");
                        base.addOption(ref oSelElmt, "Off", "off");
                        XmlElement argoBindParent14 = null;
                        base.addBind("ewActivityLogging", "web/add[@key='ActivityLogging']/@value", oBindParent: ref argoBindParent14, "true()");

                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewIPLogging", true, "IP Tracking", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "On", "on");
                        base.addOption(ref oSelElmt, "Off", "off");
                        XmlElement argoBindParent15 = null;
                        base.addBind("ewIPLogging", "web/add[@key='IPLogging']/@value", oBindParent: ref argoBindParent15, "true()");

                        base.addInput(ref oFrmElmt, "ewMailserver", true, "Mailserver");
                        XmlElement argoBindParent16 = null;
                        base.addBind("ewMailserver", "web/add[@key='MailServer']/@value", oBindParent: ref argoBindParent16, "false()");

                        base.addInput(ref oFrmElmt, "ewSiteAdminEmail", true, "Webmaster Email");
                        XmlElement argoBindParent17 = null;
                        base.addBind("ewSiteAdminEmail", "web/add[@key='SiteAdminEmail']/@value", oBindParent: ref argoBindParent17, "true()");

                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewSearch", true, "Content Search", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "On", "on");
                        base.addOption(ref oSelElmt, "Off", "off");
                        XmlElement argoBindParent18 = null;
                        base.addBind("ewSearch", "web/add[@key='ContentSearch']/@value", oBindParent: ref argoBindParent18, "true()");

                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewSiteSearch", true, "Index Search", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "On", "on");
                        base.addOption(ref oSelElmt, "Off", "off");
                        XmlElement argoBindParent19 = null;
                        base.addBind("ewSiteSearch", "web/add[@key='SiteSearch']/@value", oBindParent: ref argoBindParent19, "false()");

                        base.addInput(ref oFrmElmt, "ewSiteSearchPath", true, "Index Path");
                        XmlElement argoBindParent20 = null;
                        base.addBind("ewSiteSearchPath", "web/add[@key='SiteSearchPath']/@value", oBindParent: ref argoBindParent20, "false()");

                        base.addInput(ref oFrmElmt, "ewGoogleContentTypes", true, "Google Sitemap Content Types");
                        XmlElement argoBindParent21 = null;
                        base.addBind("ewGoogleContentTypes", "web/add[@key='GoogleContentTypes']/@value", oBindParent: ref argoBindParent21, "false()");

                        base.addInput(ref oFrmElmt, "ewShowRelatedBriefContentTypes", true, "Include Related Content for these Content Types");
                        XmlElement argoBindParent22 = null;
                        base.addBind("ewShowRelatedBriefContentTypes", "web/add[@key='ShowRelatedBriefContentTypes']/@value", oBindParent: ref argoBindParent22, "false()");

                        base.addInput(ref oFrmElmt, "ewShowRelatedBriefDepth", true, "Depth to get related content for brief content.");
                        XmlElement argoBindParent23 = null;
                        base.addBind("ewShowRelatedBriefDepth", "web/add[@key='ShowRelatedBriefDepth']/@value", oBindParent: ref argoBindParent23, "false()", "number");

                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewVersionControl", true, "Version Control", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "On", "on");
                        base.addOption(ref oSelElmt, "Off", "off");
                        XmlElement argoBindParent24 = null;
                        base.addBind("ewVersionControl", "web/add[@key='VersionControl']/@value", oBindParent: ref argoBindParent24, "false()");

                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewLegacyRedirect", true, "Legacy URL Forwarding", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "On", "on");
                        base.addOption(ref oSelElmt, "Off", "off");
                        XmlElement argoBindParent25 = null;
                        base.addBind("ewLegacyRedirect", "web/add[@key='LegacyRedirect']/@value", oBindParent: ref argoBindParent25, "true()");

                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewPageURLFormat", true, "Page URL Format for Spaces", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "Use Hyphens", "hyphens");
                        base.addOption(ref oSelElmt, "No preference", "off");
                        XmlElement argoBindParent26 = null;
                        base.addBind("ewPageURLFormat", "web/add[@key='PageURLFormat']/@value", oBindParent: ref argoBindParent26, "true()");

                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewAllowContentDetailAccess", true, "Always allow access to content detail regardless of location and permissions", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "On", "on");
                        base.addOption(ref oSelElmt, "Off", "off");
                        XmlElement argoBindParent27 = null;
                        base.addBind("ewAllowContentDetailAccess", "web/add[@key='AllowContentDetailAccess']/@value", oBindParent: ref argoBindParent27, "true()");


                        base.addSubmit(ref oFrmElmt, "", "Save Settings");

                        var oCfg = WebConfigurationManager.OpenWebConfiguration("/" + myWeb.moConfig["ProjectPath"]);
                        DefaultSection oCgfSect = (DefaultSection)oCfg.GetSection("protean/web");

                        startImp();

                        base.Instance.InnerXml = oCgfSect.SectionInformation.GetRawXml();

                        // code here to replace any missing nodes
                        // all of the required config settings
                        string[] aSettingValues = "DatabaseType,DatabaseName,DatabaseAuth,DatabaseUsername,DatabasePassword,MailServer,RootPageId,BaseUrl,SiteXsl,ImageRootPath,DocRootPath,MediaRootPath,Membership,MailingList,NonAuthenticatedUsersGroupId,AuthenticatedUsersGroupId,RegisterBehaviour,RegisterRedirectPageId,Cart,Quote,Debug,CompiledTransform,SiteAdminName,SiteAdminEmail,ContentSearch,SiteSearch,SiteSearchPath,Subscriptions,ActivityLogging,IPLogging,GoogleContentTypes,ShowRelatedBriefContentTypes,ShowRelatedBriefDepth,VersionControl,LegacyRedirect,PageURLFormat,AllowContentDetailAccess".Split(',');

                        long i;
                        XmlElement oElmt;
                        XmlElement oElmtAft = null;

                        var loopTo = (long)(aSettingValues.Length - 1);
                        for (i = 0L; i <= loopTo; i++)
                        {
                            oElmt = (XmlElement)base.Instance.SelectSingleNode("web/add[@key='" + aSettingValues[(int)i] + "']");
                            if (oElmt is null)
                            {
                                oElmt = moPageXML.CreateElement("add");
                                oElmt.SetAttribute("key", aSettingValues[(int)i]);
                                oElmt.SetAttribute("value", "");
                                if (oElmtAft is null)
                                {
                                    base.Instance.FirstChild.InsertBefore(oElmt, base.Instance.FirstChild.FirstChild);
                                }
                                else
                                {
                                    base.Instance.FirstChild.InsertAfter(oElmt, oElmtAft);
                                }
                            }
                            oElmtAft = oElmt;
                        }

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                oCgfSect.SectionInformation.RestartOnExternalChanges = false;
                                oCgfSect.SectionInformation.SetRawXml(base.Instance.InnerXml);
                                oCfg.Save();
                            }
                        }

                        endImp();

                        base.addValues();
                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmWebSettings", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmWebConfig(string ConfigType)
                {
                    XmlElement oFrmElmt;
                    string cProcessInfo = "";
                    Protean.fsHelper oFsh;
                    string xFormPath = "/xforms/config/" + ConfigType + ".xml";
                    try
                    {
                        if (myWeb.mcEWCommonFolder == "/ptn")
                        {
                            xFormPath = "/admin/xforms/config/" + ConfigType + ".xml";
                        }

                        oFsh = new Protean.fsHelper();
                        oFsh.open(moPageXML);

                        base.NewFrm("WebSettings");

                        if (!base.load(xFormPath, myWeb.maCommonFolders))
                        {

                            oFrmElmt = base.addGroup(ref base.moXformElmt, "Config", "", "ConfigSettings");
                            //XmlNode argoNode = oFrmElmt;
                            base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, xFormPath + " could not be found. - ");
                            //oFrmElmt = (XmlElement)argoNode;
                        }

                        else
                        {

                            var oCfg = WebConfigurationManager.OpenWebConfiguration("/" + myWeb.moConfig["ProjectPath"]);

                            startImp();
                            // code here to replace any missing nodes
                            // all of the required config settings

                            var oTemplateInstance = moPageXML.CreateElement("Instance");
                            oTemplateInstance.InnerXml = base.Instance.InnerXml;
                            string oCgfSectName = oTemplateInstance.FirstChild.Name;
                            string oCgfSectPath = "protean/" + oCgfSectName;
                            DefaultSection oCgfSect = (DefaultSection)oCfg.GetSection(oCgfSectPath);
                            cProcessInfo = "Getting Section Name:" + oCgfSectPath;
                            bool sectionMissing = false;

                            // Get the current settings
                            if (!string.IsNullOrEmpty(oCgfSect.SectionInformation.GetRawXml()))
                            {
                                base.Instance.InnerXml = oCgfSect.SectionInformation.GetRawXml();
                            }
                            else
                            {
                                // no current settings create them
                                sectionMissing = true;
                                oFrmElmt = base.moXformElmt;
                                //XmlNode argoNode1 = oFrmElmt;
                                base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "This config section has not yet been setup, saving will implement these settings for the first time and then log you off the admin system.");
                                //oFrmElmt = (XmlElement)argoNode1;
                            }
                            XmlElement oElmt;
                            string Key;
                            string ConfigSectionName;
                            NameValueCollection ConfigSection;

                            foreach (XmlElement oTemplateElmt in oTemplateInstance.SelectNodes("*/add"))
                            {
                                ConfigSectionName = oTemplateElmt.ParentNode.Name;
                                ConfigSection = (NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/" + ConfigSectionName);
                                Key = oTemplateElmt.GetAttribute("key");

                                oElmt = (XmlElement)base.Instance.SelectSingleNode(ConfigSectionName + "/add[@key='" + Key + "']");
                                // lets not write an empty value if inherited from machine level web.config
                                if (!(!string.IsNullOrEmpty(ConfigSection[Key]) & string.IsNullOrEmpty(oTemplateElmt.GetAttribute("value"))))
                                {
                                    if (oElmt is null)
                                    {
                                        oElmt = moPageXML.CreateElement("add");
                                        oElmt.SetAttribute("key", Key);
                                        oElmt.SetAttribute("value", oTemplateElmt.GetAttribute("value"));
                                        base.Instance.SelectSingleNode(ConfigSectionName).AppendChild(oElmt);
                                    }
                                }
                            }

                            if (base.isSubmitted())
                            {

                                base.updateInstanceFromRequest();
                                base.validate();
                                if (base.valid)
                                {
                                    if (sectionMissing)
                                    {
                                        // update config based on form submission
                                        var oNewCfgXml = new XmlDocument();
                                        oNewCfgXml.LoadXml(base.Instance.InnerXml);
                                        // save as web.config in the root
                                        oNewCfgXml.Save(goServer.MapPath(@"\" + oCgfSectPath.Replace("/", ".") + ".config"));
                                        var oMainCfgXml = new XmlDocument();
                                        // update the the web.config to include new file
                                        cProcessInfo = "loading file:" + goServer.MapPath("/web.config");
                                        oMainCfgXml.Load(goServer.MapPath("/web.config"));
                                        XmlElement oElmtEonic = (XmlElement)oMainCfgXml.SelectSingleNode("configuration/protean");
                                        var oNewElmt = oMainCfgXml.CreateElement(oCgfSectName);
                                        oNewElmt.SetAttribute("configSource", oCgfSectPath.Replace("/", ".") + ".config");
                                        oElmtEonic.AppendChild(oNewElmt);
                                        oMainCfgXml.Save(goServer.MapPath("/web.config"));
                                        myWeb.msRedirectOnEnd = "/";
                                    }
                                    else
                                    {
                                        // check not read only
                                        var oFileInfo = new FileInfo(goServer.MapPath(@"\" + oCgfSectPath.Replace("/", ".") + ".config"));
                                        oFileInfo.IsReadOnly = false;

                                        oCgfSect.SectionInformation.RestartOnExternalChanges = false;
                                        oCgfSect.SectionInformation.SetRawXml(base.Instance.InnerXml);
                                        oCfg.Save();
                                        //XmlNode argoNode2 = (XmlNode)this.moXformElmt;
                                        base.addNote(ref moXformElmt, Protean.xForm.noteTypes.Alert, "Settings Saved");
                                        //this.moXformElmt = (XmlElement)argoNode2;
                                    }

                                }
                            }

                            endImp();
                            base.addValues();
                        }

                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmWebConfig", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmRewriteMaps(string ConfigType)
                {
                    XmlElement oFrmElmt;
                    string cProcessInfo = "";
                    Protean.fsHelper oFsh;
                    string xFormPath = "/xforms/config/" + ConfigType + ".xml";
                    try
                    {
                        if (myWeb.mcEWCommonFolder == "/ptn")
                        {
                            xFormPath = "/admin/xforms/config/" + ConfigType + ".xml";
                        }
                        oFsh = new Protean.fsHelper();
                        oFsh.open(moPageXML);

                        base.NewFrm("WebSettings");
                        base.bProcessRepeats = false;

                        if (!base.load(xFormPath, myWeb.maCommonFolders))
                        {

                            oFrmElmt = base.addGroup(ref base.moXformElmt, "Config", "", "ConfigSettings");
                            base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, xFormPath + " could not be found. - ");

                        }

                        else
                        {

                            var oCfg = WebConfigurationManager.OpenWebConfiguration("/");
                            startImp();

                            // code here to replace any missing nodes
                            // all of the required config settings

                            var rewriteXml = new XmlDocument();

                            rewriteXml.Load(goServer.MapPath("/rewriteMaps.config"));


                            var oTemplateInstance = moPageXML.CreateElement("Instance");
                            oTemplateInstance.InnerXml = base.Instance.InnerXml;
                            //string oCgfSectName = "system.webServer";
                            string oCgfSectPath = "rewriteMaps/rewriteMap[@name='" + ConfigType + "']";
                            // Dim oCgfSect As System.Configuration.DefaultSection = oCfg.GetSection(oCgfSectName)
                            cProcessInfo = "Getting Section Name:" + oCgfSectPath;
                            // bool sectionMissing = false;

                            // Get the current settings
                            if (rewriteXml.SelectSingleNode(oCgfSectPath) != null)
                            {
                                base.bProcessRepeats = true;
                                if (goSession["oTempInstance"] is null)
                                {


                                    int PerPageCount = 50;
                                    if (goSession["totalCountTobeLoad"] != null)
                                    {
                                        PerPageCount = Convert.ToInt16(goSession["totalCountTobeLoad"]);
                                    }
                                    var props = rewriteXml.SelectSingleNode(oCgfSectPath);
                                    int TotalCount = props.ChildNodes.Count;

                                    if (props.ChildNodes.Count >= PerPageCount)
                                    {
                                        string xmlstring = "<rewriteMap name='" + ConfigType + "'>";
                                        string xmlstringend = "</rewriteMap>";
                                        // int count = 0;

                                        for (int i = 0, loopTo = PerPageCount - 1; i <= loopTo; i++)
                                            xmlstring = xmlstring + props.ChildNodes[i].OuterXml;

                                        base.LoadInstanceFromInnerXml(xmlstring + xmlstringend);
                                    }
                                    else
                                    {
                                        base.LoadInstanceFromInnerXml(rewriteXml.SelectSingleNode(oCgfSectPath).OuterXml);
                                    }

                                    bProcessRepeats = false;
                                }
                                else
                                {
                                    var oTempInstance = moPageXML.CreateElement("instance");
                                    oTempInstance = (XmlElement)goSession["oTempInstance"];
                                    base.updateInstance(oTempInstance);
                                }
                            }
                            else
                            {
                                // no current settings create them
                                // sectionMissing = true;
                                oFrmElmt = base.moXformElmt;
                                //XmlNode argoNode1 = oFrmElmt;
                                base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "This config section has not yet been setup, saving will implement these settings for the first time and then log you off the admin system.");
                                //oFrmElmt = (XmlElement)argoNode1;
                            }


                            XmlElement oElmt;


                            if (base.isSubmitted())
                            {

                                base.updateInstanceFromRequest();
                                base.validate();
                                // Check for loop
                                foreach (XmlElement currentOElmt in base.Instance.FirstChild.SelectNodes("descendant-or-self::add"))
                                {
                                    oElmt = currentOElmt;
                                    string newURL = oElmt.GetAttribute("value");
                                    if (base.Instance.FirstChild.SelectSingleNode("descendant-or-self::add[@key='" + newURL + "']") != null)
                                    {
                                        base.valid = false;
                                        XmlElement argoContextNode = (XmlElement)moXformElmt.SelectSingleNode("group[1]");
                                        var alertGrp = base.addGroup(ref argoContextNode, "alert", oInsertBeforeNode: (XmlElement)moXformElmt.SelectSingleNode("group[1]/group[1]"));
                                        //XmlNode argoNode2 = (XmlNode)alertGrp;
                                        base.addNote(ref alertGrp, Protean.xForm.noteTypes.Alert, "<strong>" + newURL + "</strong> cannot match an old URL");
                                        //alertGrp = (XmlElement)argoNode2;
                                    }
                                }


                                if (base.valid)
                                {
                                    XmlElement replacerNode = (XmlElement)rewriteXml.ImportNode(base.Instance.FirstChild, true);
                                    var folderRules = new ArrayList();
                                    if (ConfigType == "301Redirect")
                                    {
                                        // step through and create rules to deal with paths
                                        var rulesXml = new XmlDocument();
                                        rulesXml.Load(myWeb.goServer.MapPath("/RewriteRules.config"));
                                        XmlElement insertAfterElment = (XmlElement)rulesXml.SelectSingleNode("descendant-or-self::rule[@name='EW: 301 Redirects']");
                                        XmlElement oRule;
                                        foreach (XmlElement currentORule in replacerNode.SelectNodes("add"))
                                        {
                                            oRule = currentORule;
                                            XmlElement CurrentRule = (XmlElement)rulesXml.SelectSingleNode("descendant-or-self::rule[@name='Folder: " + oRule.GetAttribute("key") + "']");
                                            var newRule = rulesXml.CreateElement("newRule");
                                            string matchString = oRule.GetAttribute("key");
                                            if (matchString.StartsWith("/"))
                                            {
                                                matchString = matchString.TrimStart('/');
                                            }
                                            folderRules.Add("Folder: " + oRule.GetAttribute("key"));
                                            newRule.InnerXml = "<rule name=\"Folder: " + oRule.GetAttribute("key") + "\"><match url=\"^" + matchString + "(.*)\"/><action type=\"Redirect\" url=\"" + oRule.GetAttribute("value") + "{R:1}\" /></rule>";
                                            if (CurrentRule is null)
                                            {
                                                insertAfterElment.ParentNode.InsertAfter(newRule.FirstChild, insertAfterElment);
                                            }
                                            else
                                            {
                                                CurrentRule.ParentNode.ReplaceChild(newRule.FirstChild, CurrentRule);
                                            }
                                        }

                                        foreach (XmlElement currentORule1 in rulesXml.SelectNodes("descendant-or-self::rule[starts-with(@name,'Folder: ')]"))
                                        {
                                            oRule = currentORule1;
                                            if (!folderRules.Contains(oRule.GetAttribute("name")))
                                            {
                                                oRule.ParentNode.RemoveChild(oRule);
                                            }
                                        }

                                        rulesXml.Save(goServer.MapPath("/RewriteRules.config"));
                                        myWeb.bRestartApp = true;

                                    }

                                    // Dim replacingNode As XmlElement = rewriteXml.SelectSingleNode(oCgfSectPath)
                                    // If replacingNode Is Nothing Then
                                    // rewriteXml.FirstChild.AppendChild(replacerNode)
                                    // Else
                                    // rewriteXml.FirstChild.ReplaceChild(replacerNode, replacingNode)
                                    // End If
                                    // rewriteXml.Save(goServer.MapPath("/rewriteMaps.config"))

                                    // 'Check we do not have a redirect for the OLD URL allready. Remove if exists
                                    // Dim addValue As XmlElement


                                    foreach (XmlElement currentOElmt1 in base.Instance.FirstChild.SelectNodes("descendant-or-self::add"))
                                    {
                                        oElmt = currentOElmt1;
                                        string oldUrl = oElmt.GetAttribute("key");

                                        // If Not MyBase.Instance.FirstChild.SelectSingleNode("descendant-or-self::add[@key='" & newURL & "']") Is Nothing Then
                                        var existingRedirects = rewriteXml.SelectNodes("rewriteMaps/rewriteMap[@name='" + ConfigType + "']/add[@key='" + oldUrl + "']");
                                        if (existingRedirects != null)
                                        {

                                            foreach (XmlNode existingNode in existingRedirects)
                                            {
                                                existingNode.ParentNode.RemoveChild(existingNode);
                                                // existingNode.RemoveAll()
                                                rewriteXml.Save(myWeb.goServer.MapPath("/rewriteMaps.config"));
                                            }
                                        }
                                    }

                                    // Add redirect
                                    string oCgfSectPathobj = "rewriteMaps/rewriteMap[@name='" + ConfigType + "']";
                                    var redirectSectionXmlNode = rewriteXml.SelectSingleNode(oCgfSectPathobj);
                                    if (redirectSectionXmlNode != null)
                                    {
                                        foreach (XmlElement currentOElmt2 in base.Instance.FirstChild.SelectNodes("descendant-or-self::add"))
                                        {
                                            oElmt = currentOElmt2;
                                            var replacingElement = rewriteXml.CreateElement("RedirectInfo");
                                            replacingElement.InnerXml = oElmt.OuterXml;

                                            // rewriteXml.SelectSingleNode(oCgfSectPath).FirstChild.AppendChild(replacingElement.FirstChild)
                                            rewriteXml.SelectSingleNode(oCgfSectPathobj).AppendChild(replacingElement.FirstChild);

                                            rewriteXml.Save(myWeb.goServer.MapPath("/rewriteMaps.config"));
                                        }
                                    }






                                    XmlElement argoContextNode1 = (XmlElement)moXformElmt.SelectSingleNode("group[1]");
                                    var alertGrp = base.addGroup(ref argoContextNode1, "alert", oInsertBeforeNode: (XmlElement)moXformElmt.SelectSingleNode("group[1]/group[1]"));
                                    //XmlNode argoNode3 = (XmlNode)alertGrp;
                                    base.addNote(ref alertGrp, Protean.xForm.noteTypes.Alert, "Settings Saved");
                                    //alertGrp = (XmlElement)argoNode3;
                                    goSession["oTempInstance"] = (object)null;
                                }
                            }
                            else if (base.isTriggered)
                            {
                                // we have clicked a trigger so we must update the instance
                                base.updateInstanceFromRequest();
                                // lets save the instance
                                goSession["oTempInstance"] = base.Instance;
                            }
                            else
                            {
                                // clear this if we are loading the first form
                                goSession["oTempInstance"] = (object)null;
                            }
                            endImp();
                            base.addValues();
                        }

                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmWebConfig", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmThemeSettings(string ConfigPath)
                {
                    XmlElement oFrmElmt;
                    string cProcessInfo = "";
                    Protean.fsHelper oFsh;
                    string themePath = "/themes/";
                    if (goConfig["cssFramework"] != "bs5")
                    {
                        themePath = "/ewThemes/";
                    }
                    string xFormPath = themePath + ConfigPath + ".xml";
                    try
                    {
                        oFsh = new Protean.fsHelper();
                        oFsh.open(moPageXML);


                        NameValueCollection moThemeConfig = (NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/theme");
                        string currentTheme = moThemeConfig["CurrentTheme"];

                        base.NewFrm("WebSettings");

                        if (!base.load(xFormPath, myWeb.maCommonFolders))
                        {

                            oFrmElmt = base.addGroup(ref base.moXformElmt, "Config", "", "ConfigSettings");
                            //XmlNode argoNode = oFrmElmt;
                            base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, xFormPath + " could not be found. - ");
                            //oFrmElmt = (XmlElement)argoNode;
                        }

                        else
                        {

                            var oCfg = WebConfigurationManager.OpenWebConfiguration("/");

                            startImp();

                            // code here to replace any missing nodes
                            // all of the required config settings

                            var oTemplateInstance = moPageXML.CreateElement("Instance");
                            oTemplateInstance.InnerXml = base.Instance.InnerXml;
                            string oCgfSectName = "protean/" + oTemplateInstance.FirstChild.Name;
                            DefaultSection oCgfSect = (DefaultSection)oCfg.GetSection(oCgfSectName);
                            cProcessInfo = "Getting Section Name:" + oCgfSectName;
                            // Get the current settings
                            base.Instance.InnerXml = oCgfSect.SectionInformation.GetRawXml();



                            string currentPresetName = "";
                            XmlElement presetSetting = (XmlElement)base.Instance.SelectSingleNode("theme/add[@key='" + currentTheme + ".ThemePreset']");
                            if (presetSetting != null)
                            {
                                currentPresetName = presetSetting.GetAttribute("value");
                            }

                            if ((myWeb.moRequest["ThemePreset"] ?? "") != (currentPresetName ?? "") | string.IsNullOrEmpty(currentPresetName))
                            {
                                // replace Instance Elements WITH VALUES IN NAMED THEME PRESET FILE.

                                if (File.Exists(goServer.MapPath(themePath + "/" + currentTheme + "/themeManifest.xml")))
                                {



                                    var newXml = new XmlDocument();
                                    newXml.PreserveWhitespace = true;
                                    newXml.Load(goServer.MapPath(themePath + "/" + currentTheme + "/themeManifest.xml"));
                                    foreach (XmlElement oElmt2 in newXml.SelectNodes("/Theme/Presets/Preset[@name='" + myWeb.moRequest["ThemePreset"] + "']/add"))
                                    {
                                        // <add key="Bootswatch.Layout" value="TopNavSideSub"/>
                                        XmlElement changeElmt = (XmlElement)base.Instance.SelectSingleNode("descendant-or-self::add[@key='" + oElmt2.GetAttribute("key") + "']");
                                        if (changeElmt != null)
                                        {
                                            changeElmt.SetAttribute("value", oElmt2.GetAttribute("value"));
                                        }
                                    }
                                }
                            }
                            XmlElement oElmt;
                            string Key;
                            string ConfigSectionName;
                            NameValueCollection ConfigSection;

                            foreach (XmlElement oTemplateElmt in oTemplateInstance.SelectNodes("*/add"))
                            {
                                ConfigSectionName = oTemplateElmt.ParentNode.Name;
                                ConfigSection = (NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/" + ConfigSectionName);
                                Key = oTemplateElmt.GetAttribute("key");

                                oElmt = (XmlElement)base.Instance.SelectSingleNode(ConfigSectionName + "/add[@key='" + Key + "']");
                                // lets not write an empty value if inherited from machine level web.config
                                if (!(!string.IsNullOrEmpty(ConfigSection[Key]) & string.IsNullOrEmpty(oTemplateElmt.GetAttribute("value"))))
                                {
                                    if (oElmt is null)
                                    {
                                        oElmt = moPageXML.CreateElement("add");
                                        oElmt.SetAttribute("key", Key);
                                        oElmt.SetAttribute("value", oTemplateElmt.GetAttribute("value"));
                                        base.Instance.SelectSingleNode(ConfigSectionName).AppendChild(oElmt);
                                    }
                                }
                            }

                            if (base.isSubmitted())
                            {

                                // If myWeb.moRequest("ThemePreset") <> currentPresetName And Not (myWeb.moSession("presetView") = "true") Then
                                // MyBase.valid = False
                                // myWeb.moSession("presetView") = "true"
                                // Else
                                // myWeb.moSession("presetView") = Nothing
                                // MyBase.updateInstanceFromRequest()
                                // MyBase.validate()
                                // End If

                                base.updateInstanceFromRequest();
                                base.validate();


                                if (base.valid)
                                {

                                    // check not read only
                                    var oFileInfo = new FileInfo(goServer.MapPath("/protean.theme.config"));
                                    oFileInfo.IsReadOnly = false;

                                    oCgfSect.SectionInformation.RestartOnExternalChanges = false;
                                    oCgfSect.SectionInformation.SetRawXml(base.Instance.InnerXml);
                                    oCfg.Save();

                                    if (!string.IsNullOrEmpty(myWeb.moRequest["newPresetName"]))
                                    {

                                        if (File.Exists(goServer.MapPath(themePath + currentTheme + "/themeManifest.xml")))
                                        {
                                            var newXml = new XmlDocument();
                                            newXml.PreserveWhitespace = true;
                                            newXml.Load(goServer.MapPath(themePath + currentTheme + "/themeManifest.xml"));
                                            bool addNew = true;
                                            // update existing
                                            foreach (XmlElement oElmt2 in newXml.SelectNodes("/Theme/Presets/Preset[@name='" + myWeb.moRequest["newPresetName"] + "']"))
                                            {
                                                oElmt2.InnerXml = Instance.InnerXml;
                                                addNew = false;
                                            }
                                            if (addNew)
                                            {
                                                XmlElement PresetsNode = (XmlElement)newXml.SelectSingleNode("/Theme/Presets");
                                                var NewPreset = PresetsNode.OwnerDocument.CreateElement("Preset");
                                                NewPreset.SetAttribute("name", myWeb.moRequest["newPresetName"]);
                                                foreach (XmlElement matchingElmt in Instance.SelectNodes("descendant-or-self::add[starts-with(@key,'" + currentTheme + ".')]"))
                                                {
                                                    if ((matchingElmt.GetAttribute("key") ?? "") == (currentTheme + ".ThemePreset" ?? ""))
                                                    {
                                                        matchingElmt.SetAttribute("value", myWeb.moRequest["newPresetName"]);
                                                    }
                                                    AddExistingNode(ref NewPreset, matchingElmt);
                                                }
                                                PresetsNode.AppendChild(NewPreset);
                                            }
                                            // check not read only
                                            var oFileInfo2 = new FileInfo(goServer.MapPath(themePath + currentTheme + "/themeManifest.xml"));
                                            oFileInfo2.IsReadOnly = false;
                                            newXml.Save(goServer.MapPath(themePath + currentTheme + "/themeManifest.xml"));
                                        }

                                        // XmlNode argoNode1 = (XmlNode)this.moXformElmt;
                                        base.addNote(ref moXformElmt, Protean.xForm.noteTypes.Alert, "New Preset Saved");
                                        // this.moXformElmt = (XmlElement)argoNode1;
                                    }
                                    else
                                    {
                                        //XmlNode argoNode2 = (XmlNode)this.moXformElmt;
                                        base.addNote(ref moXformElmt, Protean.xForm.noteTypes.Alert, "Settings Saved");
                                        //this.moXformElmt = (XmlElement)argoNode2;
                                        base.valid = true;
                                    }
                                }
                                else
                                {
                                    //XmlNode argoNode3 = (XmlNode)this.moXformElmt;
                                    base.addNote(ref moXformElmt, Protean.xForm.noteTypes.Alert, "Form Invalid:" + base.validationError);
                                    //this.moXformElmt = (XmlElement)argoNode3;

                                }
                            }

                            endImp();
                            base.addValues();
                        }

                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmWebConfig", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmSelectTheme()
                {
                    XmlElement oFrmElmt;
                    XmlElement oSelElmt;
                    string cProcessInfo = "";
                    Protean.fsHelper oFsh;

                    try
                    {
                        oFsh = new Protean.fsHelper();
                        oFsh.open(moPageXML);

                        base.NewFrm("WebSettings");
                        // MyBase.Instance.InnerXml = "<web><add key=""SiteXsl"" value="""" /></web><theme><add key=""CurrentTheme"" value="""" /></theme>"
                        base.submission("WebSettings", "", "post", "form_check(this)");

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "WebSettings", "", "Select Theme");

                        var rootdir = new DirectoryInfo(goServer.MapPath("/ewThemes"));
                        if (!rootdir.Exists)
                        {
                            //XmlNode argoNode = oFrmElmt;
                            base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "This site is not configured to allow new themes to be selected.");
                            //oFrmElmt = (XmlElement)argoNode;
                        }
                        else
                        {
                            //XmlNode argoNode1 = oFrmElmt;
                            base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "Any Changes you make to this form risk making this site non-functional. Please be sure you know what you are doing before making any changes.");
                            //oFrmElmt = (XmlElement)argoNode1;

                            oSelElmt = base.addSelect1(ref oFrmElmt, "ewSiteTheme", true, "Site Theme", "PickByImage", Protean.xForm.ApperanceTypes.Full);
                            EnumberateThemeOptions(ref oSelElmt, "/ewThemes", ".xsl", "", true);
                            // MyBase.addOption(oSelElmt, "standard.xsl [bespoke]", "/xsl/standard.xsl")
                            XmlElement argoBindParent = null;
                            base.addBind("ewSiteTheme", "theme/add[@key='CurrentTheme']/@value", oBindParent: ref argoBindParent, "false()");

                            // MyBase.addSubmit(oFrmElmt, "", "Save Settings")

                            var oCfg = WebConfigurationManager.OpenWebConfiguration("/");

                            DefaultSection oWebCgfSect = (DefaultSection)oCfg.GetSection("protean/web");
                            DefaultSection oThemeCgfSect = (DefaultSection)oCfg.GetSection("protean/theme");

                            startImp();
                            base.Instance.InnerXml = oWebCgfSect.SectionInformation.GetRawXml() + oThemeCgfSect.SectionInformation.GetRawXml();

                            var oTemplateInstance = moPageXML.CreateElement("Instance");
                            oTemplateInstance.InnerXml = base.Instance.InnerXml;

                            if (base.isSubmitted() | !string.IsNullOrEmpty(goRequest.Form["ewsubmit.x"]) | !string.IsNullOrEmpty(goRequest.Form["ewSiteTheme"]))
                            {
                                XmlElement oElmt;
                                string Key;
                                string ConfigSectionName;
                                NameValueCollection ConfigSection;

                                foreach (XmlElement oTemplateElmt in oTemplateInstance.SelectNodes("*/add"))
                                {
                                    ConfigSectionName = oTemplateElmt.ParentNode.Name;
                                    ConfigSection = (NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/" + ConfigSectionName);
                                    Key = oTemplateElmt.GetAttribute("key");

                                    oElmt = (XmlElement)base.Instance.SelectSingleNode(ConfigSectionName + "/add[@key='" + Key + "']");
                                    // lets not write an empty value if inherited from machine level web.config
                                    if (!(!string.IsNullOrEmpty(ConfigSection[Key]) & string.IsNullOrEmpty(oTemplateElmt.GetAttribute("value"))))
                                    {
                                        if (oElmt is null)
                                        {
                                            oElmt = moPageXML.CreateElement("add");
                                            oElmt.SetAttribute("key", Key);
                                            oElmt.SetAttribute("value", oTemplateElmt.GetAttribute("value"));
                                            base.Instance.SelectSingleNode(ConfigSectionName).AppendChild(oElmt);
                                        }
                                    }
                                }

                                base.updateInstanceFromRequest();
                                base.validate();
                                if (base.valid)
                                {

                                    XmlElement updElmt = (XmlElement)base.Instance.SelectSingleNode("web/add[@key='SiteXsl']");
                                    updElmt.SetAttribute("value", "/ewthemes/" + moRequest["ewSiteTheme"] + "/standard.xsl");
                                    string cssFramework = "";
                                    if (oSelElmt.SelectSingleNode("descendant-or-self::Theme[@name='" + moRequest["ewSiteTheme"] + "' and @cssFramework!='']") != null)
                                    {
                                        XmlElement themeElmt = (XmlElement)oSelElmt.SelectSingleNode("descendant-or-self::Theme[@name='" + moRequest["ewSiteTheme"] + "' and @cssFramework!='']");
                                        cssFramework = themeElmt.GetAttribute("cssFramework");
                                    }
                                    XmlElement cssElmt = (XmlElement)base.Instance.SelectSingleNode("web/add[@key='cssFramework']");
                                    if (cssElmt is null)
                                    {
                                        oElmt = moPageXML.CreateElement("add");
                                        oElmt.SetAttribute("key", "cssFramework");
                                        oElmt.SetAttribute("value", cssFramework);
                                        base.Instance.SelectSingleNode("web").AppendChild(oElmt);
                                    }
                                    else
                                    {
                                        cssElmt.SetAttribute("value", cssFramework);
                                    }
                                    oWebCgfSect.SectionInformation.RestartOnExternalChanges = false;
                                    oWebCgfSect.SectionInformation.SetRawXml(base.Instance.SelectSingleNode("web").OuterXml);
                                    oThemeCgfSect.SectionInformation.RestartOnExternalChanges = false;
                                    oThemeCgfSect.SectionInformation.SetRawXml(base.Instance.SelectSingleNode("theme").OuterXml);

                                    oCfg.Save();
                                    //XmlNode argoNode2 = (XmlNode)this.moXformElmt;
                                    base.addNote(ref moXformElmt, Protean.xForm.noteTypes.Alert, "Settings Saved");
                                    //this.moXformElmt = (XmlElement)argoNode2;
                                }
                            }

                            endImp();
                        }


                        base.addValues();
                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmSelectTheme", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                private void EnumberateThemeOptions(ref XmlElement oSelectElmt, string filepath, string groupName, string optionName, bool bIgnoreIfNotFound)
                {

                    var oXformDoc = new XmlDocument();
                    string cProcessInfo = "";
                    //string sImgPath = "";
                    XmlElement oOptElmt;

                    try
                    {
                        if (string.IsNullOrEmpty(filepath))
                            filepath = "/";

                        // for each folder found in ewskins

                        var rootdir = new DirectoryInfo(goServer.MapPath(filepath));
                        DirectoryInfo[] dir = rootdir.GetDirectories();

                        FileInfo[] files;
                        var oChoicesElmt = base.addChoices(ref oSelectElmt, "Installed Themes");

                        foreach (var di in dir)
                        {
                            files = di.GetFiles("themeManifest.xml");
                            foreach (var fi in files)
                            {
                                cProcessInfo = "loading File:" + goServer.MapPath(filepath) + @"\" + di.Name + @"\" + fi.Name;
                                oXformDoc.Load(goServer.MapPath(filepath) + @"\" + di.Name + @"\" + fi.Name);
                                foreach (XmlElement oChoices in oXformDoc.SelectNodes("/Theme"))
                                {
                                    XmlElement RootXsltElmt = (XmlElement)oChoices.SelectSingleNode("/Theme/RootXslt");
                                    if (RootXsltElmt != null)
                                    {
                                        oOptElmt = base.addOption(ref oChoicesElmt, oChoices.GetAttribute("name"), RootXsltElmt.GetAttribute("src"));
                                        oOptElmt.FirstChild.InnerXml = oChoices.OuterXml;
                                    }
                                }
                            }
                        }
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "EnumberateThemeOptions", ex, "", cProcessInfo, gbDebug);
                    }

                }


                public XmlElement xFrmCartSettings()
                {
                    XmlElement oFrmElmt;
                    XmlElement oSelElmt;
                    string cProcessInfo = "";
                    Protean.fsHelper oFsh;

                    try
                    {
                        oFsh = new Protean.fsHelper();
                        oFsh.open(moPageXML);

                        base.NewFrm("WebSettings");

                        base.submission("WebSettings", "", "post", "form_check(this)");

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "CartSettings", "", "Cart Settings");

                        //XmlNode argoNode = oFrmElmt;
                        base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "Any Changes you make to this form risk making this site completely non-functional. Please be sure you know what you are doing before making any changes, or call your web developer for support.");
                        //oFrmElmt = (XmlElement)argoNode;

                        base.addInput(ref oFrmElmt, "ewSiteURL", true, "Site URL");
                        XmlElement argoBindParent = null;
                        base.addBind("ewSiteURL", "cart/add[@key='SiteURL']/@value", oBindParent: ref argoBindParent, "true()");

                        base.addInput(ref oFrmElmt, "ewSecureURL", true, "Secure URL");
                        XmlElement argoBindParent1 = null;
                        base.addBind("ewSecureURL", "cart/add[@key='SecureURL']/@value", oBindParent: ref argoBindParent1, "true()");

                        base.addInput(ref oFrmElmt, "ewTaxRate", true, "Tax Rate");
                        XmlElement argoBindParent2 = null;
                        base.addBind("ewTaxRate", "cart/add[@key='TaxRate']/@value", oBindParent: ref argoBindParent2, "true()");

                        base.addInput(ref oFrmElmt, "ewMerchantName", true, "Merchant Name");
                        XmlElement argoBindParent3 = null;
                        base.addBind("ewMerchantName", "cart/add[@key='MerchantName']/@value", oBindParent: ref argoBindParent3, "true()");

                        base.addInput(ref oFrmElmt, "ewMerchantEmail", true, "Merchant Email");
                        XmlElement argoBindParent4 = null;
                        base.addBind("ewMerchantEmail", "cart/add[@key='MerchantEmail']/@value", oBindParent: ref argoBindParent4, "true()");

                        base.addInput(ref oFrmElmt, "ewOrderEmailSubject", true, "Order Email Subject");
                        XmlElement argoBindParent5 = null;
                        base.addBind("ewOrderEmailSubject", "cart/add[@key='OrderEmailSubject']/@value", oBindParent: ref argoBindParent5, "true()");

                        base.addInput(ref oFrmElmt, "ewOrderNoPrefix", true, "Order No. Prefix");
                        XmlElement argoBindParent6 = null;
                        base.addBind("ewOrderNoPrefix", "cart/add[@key='OrderNoPrefix']/@value", oBindParent: ref argoBindParent6, "true()");

                        base.addInput(ref oFrmElmt, "ewCurrencySymbol", true, "Currency Symbol");
                        XmlElement argoBindParent7 = null;
                        base.addBind("ewCurrencySymbol", "cart/add[@key='CurrencySymbol']/@value", oBindParent: ref argoBindParent7, "false()");

                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewStockControl", true, "Stock Control", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "On", "on");
                        base.addOption(ref oSelElmt, "Off", "off");
                        XmlElement argoBindParent8 = null;
                        base.addBind("ewStockControl", "cart/add[@key='StockControl']/@value", oBindParent: ref argoBindParent8, "true()");

                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewDeposit", true, "Deposit", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "On", "1");
                        base.addOption(ref oSelElmt, "Off", "0");
                        XmlElement argoBindParent9 = null;
                        base.addBind("ewDeposit", "cart/add[@key='Deposit']/@value", oBindParent: ref argoBindParent9, "true()");

                        base.addInput(ref oFrmElmt, "ewDepositAmount", true, "DepositAmount");
                        XmlElement argoBindParent10 = null;
                        base.addBind("ewDepositAmount", "cart/add[@key='DepositAmount']/@value", oBindParent: ref argoBindParent10, "false()");

                        base.addInput(ref oFrmElmt, "ewNotesXForm", true, "Notes Xform");
                        XmlElement argoBindParent11 = null;
                        base.addBind("ewNotesXForm", "cart/add[@key='NotesXForm']/@value", oBindParent: ref argoBindParent11, "false()");

                        base.addInput(ref oFrmElmt, "ewBillingAddressXForm", true, "Billing Address Xform");
                        XmlElement argoBindParent12 = null;
                        base.addBind("ewBillingAddressXForm", "cart/add[@key='BillingAddressXForm']/@value", oBindParent: ref argoBindParent12, "false()");

                        base.addInput(ref oFrmElmt, "ewDeliveryAddressXForm", true, "DeliveryAddress Xform");
                        XmlElement argoBindParent13 = null;
                        base.addBind("ewDeliveryAddressXForm", "cart/add[@key='DeliveryAddressXForm']/@value", oBindParent: ref argoBindParent13, "false()");

                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewNoDeliveryAddress", true, "Disable Delivery Address", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "Yes", "on");
                        base.addOption(ref oSelElmt, "No", "off");
                        XmlElement argoBindParent14 = null;
                        base.addBind("ewNoDeliveryAddress", "cart/add[@key='NoDeliveryAddress']/@value", oBindParent: ref argoBindParent14, "true()");

                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewEmailReceipts", true, "Email Receipts", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "Yes", "on");
                        base.addOption(ref oSelElmt, "No", "off");
                        XmlElement argoBindParent15 = null;
                        base.addBind("ewEmailReceipts", "cart/add[@key='EmailReceipts']/@value", oBindParent: ref argoBindParent15, "true()");

                        base.addInput(ref oFrmElmt, "ewMerchantEmailTemplatePath", true, "Merchant Email Template Path");
                        XmlElement argoBindParent16 = null;
                        base.addBind("ewMerchantEmailTemplatePath", "cart/add[@key='MerchantEmailTemplatePath']/@value", oBindParent: ref argoBindParent16, "false()");

                        base.addInput(ref oFrmElmt, "ewCustomerEmailTemplatePath", true, "Customer Email Template Path");
                        XmlElement argoBindParent17 = null;
                        base.addBind("ewCustomerEmailTemplatePath", "cart/add[@key='CustomerEmailTemplatePath']/@value", oBindParent: ref argoBindParent17, "false()");


                        base.addInput(ref oFrmElmt, "ewPriorityCountries", true, "Priority Countries");
                        XmlElement argoBindParent18 = null;
                        base.addBind("ewPriorityCountries", "cart/add[@key='PriorityCountries']/@value", oBindParent: ref argoBindParent18, "true()");

                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewSavePayments", true, "SavePayments", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "On", "on");
                        base.addOption(ref oSelElmt, "Off", "off");
                        XmlElement argoBindParent19 = null;
                        base.addBind("ewSavePayments", "cart/add[@key='SavePayments']/@value", oBindParent: ref argoBindParent19, "true()");

                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewVatAtUnit", true, "Vat At Unit", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "Yes", "yes");
                        base.addOption(ref oSelElmt, "No", "no");
                        XmlElement argoBindParent20 = null;
                        base.addBind("ewVatAtUnit", "cart/add[@key='VatAtUnit']/@value", oBindParent: ref argoBindParent20, "true()");

                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewDiscounts", true, "Discounts", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "On", "on");
                        base.addOption(ref oSelElmt, "Off", "off");
                        XmlElement argoBindParent21 = null;
                        base.addBind("ewDiscounts", "cart/add[@key='Discounts']/@value", oBindParent: ref argoBindParent21, "true()");

                        base.addInput(ref oFrmElmt, "ewPriceModOrder", true, "Price Mod Order");
                        XmlElement argoBindParent22 = null;
                        base.addBind("ewPriceModOrder", "cart/add[@key='PriceModOrder']/@value", oBindParent: ref argoBindParent22, "false()");

                        base.addSubmit(ref oFrmElmt, "", "Save Settings");

                        var oCfg = WebConfigurationManager.OpenWebConfiguration("/");
                        DefaultSection oCgfSect = (DefaultSection)oCfg.GetSection("protean/cart");

                        startImp();
                        base.Instance.InnerXml = oCgfSect.SectionInformation.GetRawXml();

                        // code here to replace any missing nodes
                        // all of the required config settings
                        string[] aSettingValues = "SiteURL,SecureURL,TaxRate,MerchantName,MerchantEmail,OrderEmailSubject,OrderNoPrefix,CurrencySymbol,StockControl,Deposit,DepositAmount,NotesXForm,BillingAddressXForm,DeliveryAddressXForm,NoDeliveryAddress,MerchantEmailTemplatePath,PriorityCountries,SavePayments,VatAtUnit,Discounts,PriceModOrder".Split(',');

                        long i;
                        XmlElement oElmt;
                        XmlElement oElmtAft = null;

                        var loopTo = (long)(aSettingValues.Length - 1);
                        for (i = 0L; i <= loopTo; i++)
                        {
                            oElmt = (XmlElement)base.Instance.SelectSingleNode("cart/add[@key='" + aSettingValues[(int)i] + "']");
                            if (oElmt is null)
                            {
                                oElmt = moPageXML.CreateElement("add");
                                oElmt.SetAttribute("key", aSettingValues[(int)i]);
                                oElmt.SetAttribute("value", "");
                                if (oElmtAft is null)
                                {
                                    base.Instance.FirstChild.InsertBefore(oElmt, base.Instance.FirstChild.FirstChild);
                                }
                                else
                                {
                                    base.Instance.FirstChild.InsertAfter(oElmt, oElmtAft);
                                }
                            }
                            oElmtAft = oElmt;
                        }

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                oCgfSect.SectionInformation.RestartOnExternalChanges = false;
                                oCgfSect.SectionInformation.SetRawXml(base.Instance.InnerXml);
                                oCfg.Save();
                            }
                        }

                        endImp();

                        base.addValues();
                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmWebSettings", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }


            }
        }
    }
}