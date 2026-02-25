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


                public virtual XmlElement GetProviderXFrmUserLogon(string FormName = "UserLogon", string cmdPrefix = "")
                {
                    string cProcessInfo = "";

                    try
                    {

                        IMembershipAdminXforms oAdXfm = myWeb.moMemProv.AdminXforms;

                        oAdXfm.xFrmUserLogon(FormName, cmdPrefix);
                        valid = Convert.ToBoolean(oAdXfm.valid);
                        return oAdXfm.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmUserLogon", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }



                public XmlElement xFrmUserIntegrations(long userId, string secondaryCommand)
                {

                    string processInfo = "";
                    string provider = "";
                    string formName = "UserIntegrations";
                    XmlElement userInstance = null;
                    XmlElement credentials = null;
                    XmlElement permissions = null;
                    try
                    {

                        // Handle the direct integration commands
                        myWeb.CommonActions();
                        provider = myWeb.moRequest["provider"];

                        // Create the form
                        if (!string.IsNullOrEmpty(secondaryCommand))
                            formName += "." + secondaryCommand;
                        base.NewFrm(formName);

                        // Add form parameters
                        if (!string.IsNullOrEmpty(provider))
                        {
                            string[] integrationsFormParameters = new string[] { provider };
                            FormParameters = integrationsFormParameters;
                        }

                        // Load the form
                        if (base.load("/xforms/directory/" + formName + ".xml", myWeb.maCommonFolders))
                        {



                            if (userId > 0L)
                            {

                                // Load the user instance
                                userInstance = base.moXformElmt.OwnerDocument.CreateElement("instance");
                                userInstance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.Directory, userId);

                                // If this is permissions then we need to extract the permissions first, or create new ones.
                                if (secondaryCommand == "Permissions")
                                {
                                    credentials = (XmlElement)userInstance.SelectSingleNode("//Credentials[@provider='" + provider + "']");
                                    permissions = (XmlElement)credentials.SelectSingleNode("Permissions");
                                    if (permissions != null)
                                        base.Instance.InnerXml = permissions.OuterXml;
                                }
                                else
                                {
                                    // By default add the user instance
                                    base.Instance.InnerXml = userInstance.InnerXml;
                                }


                                // Handle the submits
                                if (base.isSubmitted())
                                {
                                    base.updateInstanceFromRequest();
                                    base.validate();
                                    if (base.valid)
                                    {

                                        // Permissions
                                        if (secondaryCommand == "Permissions")
                                        {

                                            // Need to put the permissions node back into the user instance
                                            if (credentials != null)
                                            {

                                                if (permissions != null)
                                                {
                                                    XmlElement localfirstElement() { var argoElement = base.Instance; var ret = Xml.firstElement(ref argoElement); base.Instance = argoElement; return ret; }

                                                    credentials.ReplaceChild(credentials.OwnerDocument.ImportNode(localfirstElement(), true), permissions);
                                                }
                                                else
                                                {
                                                    XmlElement localfirstElement1() { var argoElement1 = base.Instance; var ret = Xml.firstElement(ref argoElement1); base.Instance = argoElement1; return ret; }

                                                    credentials.AppendChild(credentials.OwnerDocument.ImportNode(localfirstElement1(), true));
                                                }

                                                moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Directory, userInstance, userId);
                                            }

                                            else
                                            {
                                                // No credentials to save something against
                                            }

                                        }



                                        // Go back to UserIntegrations
                                        var newCmd = new NameValueCollection(1);
                                        newCmd.Add("ewCmd", "UserIntegrations");
                                        myWeb.msRedirectOnEnd = Tools.Http.Utils.BuildURIFromRequest(myWeb.moRequest, newCmd, "integration,provider").ToString();
                                    }
                                }
                            }

                            else
                            {
                                // No user found - bad times.

                            }

                            // Because we have handled the integrations, we need to follow up any responses
                            foreach (XmlElement response in myWeb.PageXMLResponses)
                            {

                                switch (response.GetAttribute("type") ?? "")
                                {

                                    case "Redirect":
                                        {
                                            myWeb.msRedirectOnEnd = response.InnerText;
                                            break;
                                        }
                                    case "Alert":
                                        {
                                            var argoNode = base.moXformElmt.SelectSingleNode("group");
                                            base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, response.InnerText);
                                            break;
                                        }
                                    case "Hint":
                                        {
                                            var argoNode1 = base.moXformElmt.SelectSingleNode("group");
                                            base.addNote(ref argoNode1, Protean.xForm.noteTypes.Hint, response.InnerText);
                                            break;
                                        }
                                    case "Help":
                                        {
                                            var argoNode2 = base.moXformElmt.SelectSingleNode("group");
                                            base.addNote(ref argoNode2, Protean.xForm.noteTypes.Help, response.InnerText);
                                            break;
                                        }
                                }

                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmUserIntegrations", ex, "", processInfo, gbDebug);
                        return null;
                    }

                }


                public virtual XmlElement xFrmCopyGroupMembers(long dirId)
                {
                    XmlElement oFrmElmt;
                    XmlElement oElmt;
                    XmlElement oElmt2;
                    XmlElement oElmt3;
                    string sSql;
                    // Dim oDr As SqlDataReader
                    string cProcessInfo = "";
                    string sType = "Group";

                    try
                    {
                        // load the directory item to be deleted
                        moDbHelper.moPageXml = moPageXML;
                        base.NewFrm("CopyGroupMembers");

                        // Lets get the object
                        oElmt = moPageXML.CreateElement("sType");
                        oElmt.SetAttribute("id", dirId.ToString());
                        if (dirId != 0L)
                        {
                            // oDr = moDbHelper.getDataReader("SELECT * FROM tblDirectory where nDirKey = " & dirId)
                            using (var oDr = moDbHelper.getDataReaderDisposable("SELECT * FROM tblDirectory where nDirKey = " + dirId))  // Done by nita on 6/7/22
                            {
                                while (oDr.Read())
                                {
                                    oElmt.SetAttribute("name", Convert.ToString(oDr["cDirName"]));
                                    if (!string.IsNullOrEmpty(Convert.ToString(oDr["cDirXml"])))
                                    {
                                        oElmt.InnerXml = Convert.ToString(oDr["cDirXml"]);
                                    }
                                    else
                                    {
                                        oElmt.InnerXml = Instance.SelectSingleNode("*").InnerXml;
                                    }
                                }
                            }

                            // get item parents
                            sSql = "SELECT d.* FROM tblDirectory d " + "inner join tblDirectoryRelation r on r.nDirParentId = d.nDirKey " + "where r.nDirChildId = " + dirId;

                            using (var oDr = moDbHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                            {
                                while (oDr.Read())
                                {
                                    oElmt2 = moPageXML.CreateElement(Convert.ToString(oDr["cDirSchema"]));
                                    oElmt2.SetAttribute("id", Convert.ToString(oDr["nDirKey"]));
                                    oElmt2.SetAttribute("name", Convert.ToString(oDr["cDirName"]));
                                    oElmt2.SetAttribute("relType", "child");
                                    base.Instance.AppendChild(oElmt2);
                                }
                            }

                            // get item Children
                            sSql = "SELECT d.* FROM tblDirectory d " + "inner join tblDirectoryRelation r on r.nDirChildId = d.nDirKey " + "where r.nDirParentId = " + dirId;

                            using (var oDr = moDbHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                            {
                                while (oDr.Read())
                                {
                                    oElmt3 = moPageXML.CreateElement(Convert.ToString(oDr["cDirSchema"]));
                                    oElmt3.SetAttribute("id", Convert.ToString(oDr["nDirKey"]));
                                    oElmt3.SetAttribute("name", Convert.ToString(oDr["cDirName"]));
                                    oElmt3.SetAttribute("relType", "parent");
                                    base.Instance.AppendChild(oElmt3);
                                }
                            }

                        }

                        base.Instance.AppendChild(oElmt);

                        base.submission("EditInput", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "MoveDirMembers", "", "Copy " + sType + " Members");

                        //XmlNode argoNode = oFrmElmt;
                        base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "Are you sure you want to copy this " + sType + " Members " + encodeAllHTML(oElmt.GetAttribute("name")), false, "alert-danger");
                        //oFrmElmt = (XmlElement)argoNode;

                        // Lets get all other groups
                        oElmt3 = base.addSelect1(ref oFrmElmt, sType + "CopyTo", false, "Copy " + sType + " Members To", "scroll_10", Protean.xForm.ApperanceTypes.Minimal);
                        sSql = "SELECT d.nDirKey as value, d.cDirName as name from tblDirectory d where d.cDirSchema='" + sType + "' and d.nDirKey<>" + dirId + " order by cDirName";
                        using (var oDr = moDbHelper.getDataReaderDisposable(sSql)) // done by sonali at 12/7/22
                        {
                            var argoDr = oDr;
                            base.addOptionsFromSqlDataReader(oElmt3, argoDr, "name", "value");
                        }
                        base.addSubmit(ref oFrmElmt, "", "Copy " + sType);

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {

                                // select all child relations so child objects don't get deleted
                                sSql = "select nRelKey, nDirChildId from tblDirectoryRelation where nDirParentId = " + dirId;
                                using (var oDr = moDbHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                                {
                                    // Loop through 1 behind so we can trigger sync on last one.
                                    long previousId = 0L;
                                    while (oDr.Read())
                                    {
                                        if (!(previousId == 0L))
                                        {
                                            moDbHelper.maintainDirectoryRelation(Convert.ToInt64(moRequest["GroupCopyTo"]), previousId, false, bIfExistsDontUpdate: true, isLast: false);
                                        }
                                        previousId = Convert.ToInt64(oDr[1]);
                                    }
                                    moDbHelper.maintainDirectoryRelation(Convert.ToInt64(moRequest["GroupCopyTo"]), previousId, false, bIfExistsDontUpdate: true, isLast: true);

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
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmCopyGroupMembers", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                public virtual XmlElement xFrmEditRole(long id)
                {


                    string cProcessInfo = "";
                    //string cCurrentPassword = "";
                    //string cCodeUsed = "";
                    // bool addNewitemToParId = false;
                    string cDirectorySchemaName = "role";
                    string cXformName = "";
                    XmlElement oElmt;

                    try
                    {

                        if (id > 0L)
                        {
                            if (string.IsNullOrEmpty(cXformName))
                                cXformName = cDirectorySchemaName;

                            // ok lets load in an xform from the file location.
                            string formPath = "/xforms/directory/" + cXformName + ".xml";
                            if (myWeb.moConfig["cssFramework"] == "bs5")
                            {
                                formPath = "/features/membership/" + cXformName + ".xml";
                            }
                            if (!base.load(formPath, myWeb.maCommonFolders))
                            {
                                // load a default content xform if no alternative.
                            }

                            base.Instance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.Directory, id);

                            XmlElement oRoleRights = (XmlElement)base.Instance.SelectSingleNode("tblDirectory/cDirXml/Role/AdminRights");

                            XmlElement siteRights = (XmlElement)moPageXML.SelectSingleNode("/Page/AdminMenu");

                            if (oRoleRights is null)
                            {
                                oRoleRights = moPageXML.CreateElement("AdminRights");
                                oRoleRights.InnerXml = siteRights.InnerXml;
                                foreach (XmlElement currentOElmt in oRoleRights.SelectNodes("descendant-or-self::MenuItem"))
                                {
                                    oElmt = currentOElmt;
                                    oElmt.SetAttribute("adminRight", "");
                                }
                                base.Instance.SelectSingleNode("tblDirectory/cDirXml/Role").AppendChild(oRoleRights);
                            }
                            else
                            {
                                XmlElement oRoleRights2;
                                oRoleRights2 = moPageXML.CreateElement("AdminRights");
                                oRoleRights2.InnerXml = siteRights.InnerXml;
                                foreach (XmlElement currentOElmt1 in oRoleRights2.SelectNodes("descendant-or-self::MenuItem"))
                                {
                                    oElmt = currentOElmt1;
                                    if (oRoleRights.SelectSingleNode("descendant-or-self::MenuItem[@cmd='" + oElmt.GetAttribute("cmd") + "' and @adminRight='true']") != null)
                                    {
                                        oElmt.SetAttribute("adminRight", "true");
                                    }
                                    else
                                    {
                                        oElmt.SetAttribute("adminRight", "");
                                    }

                                }
                                // remove the old admin rights
                                foreach (XmlElement currentOElmt2 in Instance.SelectNodes("tblDirectory/cDirXml/Role/AdminRights"))
                                {
                                    oElmt = currentOElmt2;
                                    oElmt.ParentNode.RemoveChild(oElmt);
                                }
                                // paste in new and updated
                                base.Instance.SelectSingleNode("tblDirectory/cDirXml/Role").AppendChild(oRoleRights2);
                            }

                            oRoleRights = (XmlElement)base.Instance.SelectSingleNode("tblDirectory/cDirXml/Role/AdminRights");
                            XmlElement oSelElmt;
                            XmlElement oGrpRoot = (XmlElement)moXformElmt.SelectSingleNode("group[@class='2Col']/group[2]");
                            if (myWeb.moConfig["cssFramework"] == "bs5")
                            {
                                oGrpRoot = (XmlElement)moXformElmt.SelectSingleNode("descendant-or-self::group[@class='admin-rights-block']");
                            }
                            if (oGrpRoot is null)
                                oGrpRoot = moXformElmt;

                            var oGrp = base.addGroup(ref oGrpRoot, "adminRights", "adminRights", "Admin Rights");

                            XmlElement oGrp2;
                            XmlElement oGrp3;
                            XmlElement oGrp4;
                            XmlElement oGrp5;
                            long nCount = 1L;
                            string sRef;
                            XmlElement oBind;
                            XmlElement oBind1;
                            XmlElement oBind2;
                            XmlElement oBind3;
                            XmlElement oBind4;

                            foreach (XmlElement currentOElmt3 in oRoleRights.SelectNodes("MenuItem"))
                            {
                                oElmt = currentOElmt3;
                                sRef = "adminRight" + nCount;
                                oSelElmt = base.addSelect(ref oGrp, sRef, true, "", nAppearance: Protean.xForm.ApperanceTypes.Full);
                                base.addOption(ref oSelElmt, oElmt.GetAttribute("name"), "true");
                                XmlElement argoBindParent = null;
                                oBind = base.addBind("", "tblDirectory/cDirXml/Role/AdminRights/MenuItem[@cmd='" + oElmt.GetAttribute("cmd") + "']", oBindParent: ref argoBindParent);
                                base.addBind(sRef, "@adminRight", oBindParent: ref oBind);
                                nCount = nCount + 1L;
                                if (oElmt.SelectNodes("MenuItem").Count > 0)
                                {
                                    oGrp2 = base.addGroup(ref oGrp, oElmt.GetAttribute("name"), "grp" + nCount);
                                    foreach (XmlElement oElmt2 in oElmt.SelectNodes("MenuItem"))
                                    {
                                        sRef = "adminRight" + nCount;
                                        oSelElmt = base.addSelect(ref oGrp2, sRef, true, "", nAppearance: Protean.xForm.ApperanceTypes.Full);
                                        base.addOption(ref oSelElmt, oElmt2.GetAttribute("name"), "true");
                                        oBind1 = base.addBind("", "MenuItem[@cmd='" + oElmt2.GetAttribute("cmd") + "']", oBindParent: ref oBind);
                                        base.addBind(sRef, "@adminRight", oBindParent: ref oBind1);
                                        nCount = nCount + 1L;
                                        if (oElmt2.SelectNodes("MenuItem").Count > 0)
                                        {
                                            oGrp3 = base.addGroup(ref oGrp2, oElmt2.GetAttribute("name"), "grp" + nCount);
                                            foreach (XmlElement oElmt3 in oElmt2.SelectNodes("MenuItem"))
                                            {
                                                sRef = "adminRight" + nCount;
                                                oSelElmt = base.addSelect(ref oGrp3, sRef, true, "", nAppearance: Protean.xForm.ApperanceTypes.Full);
                                                base.addOption(ref oSelElmt, oElmt3.GetAttribute("name"), "true");
                                                oBind2 = base.addBind("", "MenuItem[@cmd='" + oElmt3.GetAttribute("cmd") + "']", oBindParent: ref oBind1);
                                                base.addBind(sRef, "@adminRight", oBindParent: ref oBind2);
                                                nCount = nCount + 1L;
                                                if (oElmt3.SelectNodes("MenuItem").Count > 0)
                                                {
                                                    oGrp4 = base.addGroup(ref oGrp3, oElmt3.GetAttribute("name"), "grp" + nCount);
                                                    foreach (XmlElement oElmt4 in oElmt3.SelectNodes("MenuItem"))
                                                    {
                                                        sRef = "adminRight" + nCount;
                                                        oSelElmt = base.addSelect(ref oGrp4, sRef, true, "", nAppearance: Protean.xForm.ApperanceTypes.Full);
                                                        base.addOption(ref oSelElmt, oElmt4.GetAttribute("name"), "true");
                                                        oBind3 = base.addBind("", "MenuItem[@cmd='" + oElmt4.GetAttribute("cmd") + "']", oBindParent: ref oBind2);
                                                        base.addBind(sRef, "@adminRight", oBindParent: ref oBind3);
                                                        nCount = nCount + 1L;
                                                        if (oElmt4.SelectNodes("MenuItem").Count > 0)
                                                        {
                                                            oGrp5 = base.addGroup(ref oGrp4, oElmt4.GetAttribute("name"), "grp" + nCount);
                                                            foreach (XmlElement oElmt5 in oElmt4.SelectNodes("MenuItem"))
                                                            {
                                                                sRef = "adminRight" + nCount;
                                                                oSelElmt = base.addSelect(ref oGrp5, sRef, true, "", nAppearance: Protean.xForm.ApperanceTypes.Full);
                                                                base.addOption(ref oSelElmt, oElmt5.GetAttribute("name"), "true");
                                                                oBind4 = base.addBind("", "MenuItem[@cmd='" + oElmt5.GetAttribute("cmd") + "']", oBindParent: ref oBind3);
                                                                base.addBind(sRef, "@adminRight", oBindParent: ref oBind4);
                                                                nCount = nCount + 1L;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            cDirectorySchemaName = base.Instance.SelectSingleNode("tblDirectory/cDirSchema").InnerText;

                            if (base.isSubmitted())
                            {
                                base.updateInstanceFromRequest();
                                base.validate();
                                // any additonal validation goes here

                                if (base.valid)
                                {

                                    moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Directory, base.Instance, id);

                                    base.addNote("EditContent", Protean.xForm.noteTypes.Alert, "<span class=\"msg-1010\">Your details have been updated.</span>", true);

                                }
                            }

                            base.addValues();
                            return base.moXformElmt;
                        }
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmEditRole", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }



                public virtual XmlElement xFrmDeleteDirectoryItem(long dirId, string sType)
                {

                    XmlElement oFrmElmt;
                    XmlElement oElmt;
                    XmlElement oElmt2;
                    XmlElement oElmt3;
                    string sSql;
                    // Dim oDr As SqlDataReader
                    string cProcessInfo = "";


                    try
                    {
                        // load the directory item to be deleted
                        moDbHelper.moPageXml = moPageXML;

                        base.NewFrm("DeleteDirectory");

                        // Lets get the object
                        oElmt = moPageXML.CreateElement("sType");
                        oElmt.SetAttribute("id", dirId.ToString());
                        if (dirId != 0L)
                        {
                            // oDr = moDbHelper.getDataReader("SELECT * FROM tblDirectory where nDirKey = " & dirId)
                            using (var oDr = moDbHelper.getDataReaderDisposable("SELECT * FROM tblDirectory where nDirKey = " + dirId))  // Done by nita on 6/7/22
                            {
                                while (oDr.Read())
                                {
                                    oElmt.SetAttribute("name", Convert.ToString(oDr["cDirName"]));
                                    if (!string.IsNullOrEmpty(Convert.ToString(oDr["cDirXml"])))
                                    {
                                        oElmt.InnerXml = Convert.ToString(oDr["cDirXml"]);
                                    }
                                    else
                                    {
                                        oElmt.InnerXml = Instance.SelectSingleNode("*").InnerXml;
                                    }
                                }
                            }

                            // get item parents
                            sSql = "SELECT d.* FROM tblDirectory d " + "inner join tblDirectoryRelation r on r.nDirParentId = d.nDirKey " + "where r.nDirChildId = " + dirId;

                            using (var oDr = moDbHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                            {
                                while (oDr.Read())
                                {
                                    oElmt2 = moPageXML.CreateElement(Convert.ToString(oDr["cDirSchema"]));
                                    oElmt2.SetAttribute("id", Convert.ToString(oDr["nDirKey"]));
                                    oElmt2.SetAttribute("name", Convert.ToString(oDr["cDirName"]));
                                    oElmt2.SetAttribute("relType", "child");
                                    base.Instance.AppendChild(oElmt2);
                                }
                            }

                            // get item Children
                            sSql = "SELECT d.* FROM tblDirectory d " + "inner join tblDirectoryRelation r on r.nDirChildId = d.nDirKey " + "where r.nDirParentId = " + dirId;

                            using (var oDr = moDbHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                            {
                                while (oDr.Read())
                                {
                                    oElmt3 = moPageXML.CreateElement(Convert.ToString(oDr["cDirSchema"]));
                                    oElmt3.SetAttribute("id", Convert.ToString(oDr["nDirKey"]));
                                    oElmt3.SetAttribute("name", Convert.ToString(oDr["cDirName"]));
                                    oElmt3.SetAttribute("relType", "parent");
                                    base.Instance.AppendChild(oElmt3);
                                }
                            }

                        }

                        base.Instance.AppendChild(oElmt);




                        base.submission("delete-form", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "", "delete-form", "Delete " + sType);

                        //XmlNode argoNode = oFrmElmt;
                        base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "Are you sure you want to delete this " + sType + " " + encodeAllHTML(oElmt.GetAttribute("name")), false, "alert-danger");
                        //oFrmElmt = (XmlElement)argoNode;

                        switch (sType ?? "")
                        {
                            case "User":
                                {
                                    break;
                                }
                            // MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, "What do you want to do with this " & sType & "s exam results?")

                            // oElmt2 = MyBase.addSelect1(oFrmElmt, "Options", False, "Exam Results", "", xForm.ApperanceTypes.Full)
                            // MyBase.addOption(oElmt2, "Delete All", "Delete")
                            // MyBase.addOption(oElmt2, "Transfer to another user in the same company", "Transfer")

                            // oElmt3 = MyBase.addSelect1(oFrmElmt, "TransUserId", False, "Select User", "scroll_10", xForm.ApperanceTypes.Minimal)

                            // 'Lets get all the users in the same company

                            // sSql = "select d.nDirKey as value, d.cDirName as name " & _
                            // "FROM (((tblDirectory d " & _
                            // "inner join tblAudit a on nAuditId = a.nAuditKey " & _
                            // "INNER JOIN tblDirectoryRelation user2company1 " & _
                            // "ON d.nDirKey = user2company1.nDirChildId) " & _
                            // "INNER JOIN tblDirectory company " & _
                            // "ON company.nDirKey = user2company1.nDirParentId) " & _
                            // "INNER JOIN tblDirectoryRelation user2company2 " & _
                            // "ON company.nDirKey = user2company2.nDirParentId ) " & _
                            // "INNER JOIN tblDirectory users " & _
                            // "ON users.nDirKey = user2company2.nDirChildId " & _
                            // "left outer join tblDirectoryRelation dr on d.nDirKey = dr.nDirParentId and dr.nDirChildId = " & dirId & _
                            // "WHERE d.cDirSchema = 'User' AND company.cDirSchema = 'Company' AND users.cDirSchema = 'User' " & _
                            // "AND users.nDirKey = " & dirId & " and d.nDirKey<>" & dirId & " order by d.cDirName"

                            // MyBase.addOptionsFromSqlDataReader(oElmt3, oDt.getDataReader(sSql), "name", "value")

                            case "Department":
                                {
                                    //XmlNode argoNode1 = oFrmElmt;
                                    base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "What do you want to do with this " + sType + "s users?", false, "alert-danger");
                                    //oFrmElmt = (XmlElement)argoNode1;

                                    oElmt2 = base.addSelect1(ref oFrmElmt, "Options", false, "Users", "", Protean.xForm.ApperanceTypes.Full);
                                    base.addOption(ref oElmt2, "Just remove the Department relationship from the Users", "Remove");
                                    base.addOption(ref oElmt2, "Transfer Users to another Department in the same Company", "Transfer");

                                    oElmt3 = base.addSelect1(ref oFrmElmt, "TransDeptId", false, "Select User", "scroll_10", Protean.xForm.ApperanceTypes.Minimal);

                                    // Lets get all the departments in the same company

                                    sSql = "select d.nDirKey as value, d.cDirName as name " + "FROM (((tblDirectory d " + "inner join tblAudit a on nAuditId = a.nAuditKey " + "INNER JOIN tblDirectoryRelation dept2company1 " + "ON d.nDirKey = dept2company1.nDirChildId) " + "INNER JOIN tblDirectory company " + "ON company.nDirKey = dept2company1.nDirParentId) " + "INNER JOIN tblDirectoryRelation user2company2 " + "ON company.nDirKey = user2company2.nDirParentId ) " + "INNER JOIN tblDirectory dept " + "ON dept.nDirKey = user2company2.nDirChildId " + "left outer join tblDirectoryRelation dr on d.nDirKey = dr.nDirParentId and dr.nDirChildId = " + dirId + "WHERE d.cDirSchema = 'Department' AND company.cDirSchema = 'Company' AND dept.cDirSchema = 'Department' " + "AND dept.nDirKey = " + dirId + " and d.nDirKey<>" + dirId + " order by d.cDirName";
                                    using (var oDr = moDbHelper.getDataReaderDisposable(sSql)) // done by sonali at 12/7/22
                                    {
                                        var argoDr = oDr;
                                        base.addOptionsFromSqlDataReader(oElmt3, argoDr, "name", "value");
                                    }

                                    break;
                                }

                            case "Company":
                                {

                                    //XmlNode argoNode2 = oFrmElmt;
                                    base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "What do you want to do with this " + sType + "s users?", false, "alert-danger");
                                    //oFrmElmt = (XmlElement)argoNode2;

                                    oElmt2 = base.addSelect1(ref oFrmElmt, "Options", false, "Users / Departments", "", Protean.xForm.ApperanceTypes.Full);
                                    base.addOption(ref oElmt2, "Delete All Users/Departments", "Delete");
                                    base.addOption(ref oElmt2, "Just remove the Company relationship from the Users and delete Departments", "RemoveDept");
                                    base.addOption(ref oElmt2, "Transfer Users/Departments to another Company", "Transfer");

                                    // Lets get all other companies
                                    oElmt3 = base.addSelect1(ref oFrmElmt, "TransCompanyId", false, "Select Departments", "scroll_10", Protean.xForm.ApperanceTypes.Minimal);
                                    sSql = "SELECT d.nDirKey as value, d.cDirName as name from tblDirectory d where d.cDirSchema='Company' and d.nDirKey<>" + dirId + " order by cDirName";

                                    using (var oDr = moDbHelper.getDataReaderDisposable(sSql)) // done by sonali at 12/7/22
                                    {
                                        base.addOptionsFromSqlDataReader(oElmt3, oDr, "name", "value");
                                    } // "Group", "Role"

                                    break;
                                }

                            default:
                                {

                                    oElmt2 = base.addSelect1(ref oFrmElmt, "Options", false, "What do you want to do with this " + sType + "s members?", "", Protean.xForm.ApperanceTypes.Full);
                                    base.addOption(ref oElmt2, "Just remove the " + sType + " relationship from the members", "Remove");
                                    base.addOption(ref oElmt2, "Transfer members to another " + sType, "Transfer");

                                    // Lets get all other groups
                                    oElmt3 = base.addSelect1(ref oFrmElmt, sType + "s", false, "Select Alternative " + sType, "scroll_10", Protean.xForm.ApperanceTypes.Minimal);
                                    sSql = "SELECT d.nDirKey as value, d.cDirName as name from tblDirectory d where d.cDirSchema='" + sType + "' and d.nDirKey<>" + dirId + " order by cDirName";
                                    using (var oDr = moDbHelper.getDataReaderDisposable(sSql)) // done by sonali at 12/7/22
                                    {

                                        base.addOptionsFromSqlDataReader(oElmt3, oDr, "name", "value");
                                    }

                                    break;
                                }
                                // Case "Role"

                                // oElmt2 = MyBase.addSelect1(oFrmElmt, "Options", False, "What do you want to do with this " & sType & "s users?", "", xForm.ApperanceTypes.Full)
                                // MyBase.addOption(oElmt2, "Just remove the Role relationship from the Users", "Remove")
                                // MyBase.addOption(oElmt2, "Transfer Users to another Role", "Transfer")

                                // 'Lets get all other roles
                                // oElmt3 = MyBase.addSelect1(oFrmElmt, "Roles", False, "Select Roles", "scroll_10", xForm.ApperanceTypes.Minimal)
                                // sSql = "SELECT d.nDirKey as value, d.cDirName as name from tblDirectory d where d.cDirSchema='Role' and d.nDirKey<>" & dirId & " order by cDirName"
                                // MyBase.addOptionsFromSqlDataReader(oElmt3, moDbHelper.getDataReader(sSql), "name", "value")

                        }

                        base.addSubmit(ref oFrmElmt, "delete-form", "Delete " + sType);

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {

                                // remove the relevent answer information
                                switch (sType ?? "")
                                {
                                    case "User":
                                        {
                                            // Select Case goRequest("Options")
                                            // Case "Transfer"
                                            // sSql = "select nQResultsKey from tblQuestionaireResult where nDirId=" & dirId
                                            // oDr = oDt.getDataReader(sSql)
                                            // While oDr.Read
                                            // sSql = "update tblQuestionaireResult set nDirId = " & goRequest("TransUserId") & " where nDirId=" & dirId
                                            // oDt.exeProcessSQL(sSql)
                                            // End While
                                            // oDr.Close()
                                            // oDr = Nothing
                                            // moDbhelper.DeleteObject(dbHelper.objectTypes.Directory, dirId)
                                            // Case "Delete"
                                            moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.Directory, dirId);
                                            break;
                                        }
                                    // Case Else
                                    // 'do nothing
                                    // End Select

                                    case "Company":
                                        {
                                            // Delete Departments 
                                            // Delete Departments Directory Relations
                                            // Delete Company Permissions
                                            // Delete Company Users / or move to another Company / or leave oprhaned?
                                            // Move to another department / or leave oprhaned?
                                            switch (goRequest["Options"] ?? "")
                                            {
                                                case "RemoveDept":
                                                    {
                                                        sSql = "select r.nRelKey from tblDirectoryRelation r where r.nDirParentId = " + dirId + " inner join tblDirectory d on r.nDirChildId = d.nDirKey " + " where d.cDirSchema = 'User' ";
                                                        using (var oDr = moDbHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                                                        {
                                                            while (oDr.Read())
                                                                moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.DirectoryRelation, Convert.ToInt64(oDr[0]));
                                                            oDr.Close();
                                                        }

                                                        // Delete Company Directory Relations, Company Permissions and Company
                                                        moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.Directory, dirId);
                                                        break;
                                                    }

                                                case "Transfer":
                                                    {

                                                        sSql = "update tblDirectoryRelation set nDirParentId = " + goRequest["TransCompanyId"] + " where nDirParentId=" + dirId;
                                                        moDbHelper.ExeProcessSql(sSql);

                                                        // Delete Company Directory Relations, Company Permissions and Company
                                                        moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.Directory, dirId);
                                                        break;
                                                    }

                                                case "Delete":
                                                    {
                                                        // Delete Company Directory Relations, Company Permissions and Company
                                                        moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.Directory, dirId);
                                                        break;
                                                    }

                                            }

                                            break;
                                        }

                                    case "Department":
                                        {

                                            // Move to another department / or leave oprhaned?
                                            switch (goRequest["Options"] ?? "")
                                            {
                                                case "Transfer":
                                                    {

                                                        sSql = "update tblDirectoryRelation set nDirParentId = " + goRequest["TransDeptId"] + " where nDirParentId=" + dirId;
                                                        moDbHelper.ExeProcessSql(sSql);

                                                        // Delete Department Directory Relations, Department Permissions and Department
                                                        moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.Directory, dirId);
                                                        break;
                                                    }
                                                case "Remove":
                                                    {
                                                        // remove all child relations so child objects don't get deleted
                                                        sSql = "select nRelKey from tblDirectoryRelation where nDirParentId = " + dirId;
                                                        using (var oDr = moDbHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                                                        {
                                                            while (oDr.Read())
                                                                moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.DirectoryRelation, Convert.ToInt64(oDr[0]));
                                                        }

                                                        // Delete Department Directory Relations, Department Permissions and Department
                                                        moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.Directory, dirId);
                                                        break;
                                                    }

                                                default:
                                                    {
                                                        break;
                                                    }
                                                    // do nothing
                                            }

                                            break;
                                        }


                                    case "Group":
                                        {
                                            switch (goRequest["Options"] ?? "")
                                            {
                                                case "Transfer":
                                                    {
                                                        // remove all child relations so child objects don't get deleted
                                                        sSql = "select nRelKey, nDirChildId from tblDirectoryRelation where nDirParentId = " + dirId;
                                                        using (var oDr = moDbHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                                                        {
                                                            while (oDr.Read())
                                                            {
                                                                moDbHelper.saveDirectoryRelations(Convert.ToInt64(oDr[1]), goRequest["Groups"]);
                                                                moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.DirectoryRelation, Convert.ToInt64(oDr[0]));
                                                            }
                                                        }

                                                        moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.Directory, dirId);
                                                        break;
                                                    }

                                                case "Remove":
                                                    {
                                                        // remove all child relations so child objects don't get deleted
                                                        sSql = "select nRelKey from tblDirectoryRelation where nDirParentId = " + dirId;
                                                        using (var oDr = moDbHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                                                        {
                                                            while (oDr.Read())
                                                                moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.DirectoryRelation, Convert.ToInt64(oDr[0]));
                                                        }

                                                        moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.Directory, dirId);
                                                        break;
                                                    }

                                                default:
                                                    {
                                                        break;
                                                    }
                                                    // do nothing
                                            }

                                            break;
                                        }
                                    case "Role":
                                        {
                                            switch (goRequest["Options"] ?? "")
                                            {
                                                case "Transfer":
                                                    {

                                                        // remove all child relations so child objects don't get deleted
                                                        sSql = "select nRelKey, nDirChildId from tblDirectoryRelation where nDirParentId = " + dirId;
                                                        using (var oDr = moDbHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                                                        {
                                                            while (oDr.Read())
                                                            {
                                                                moDbHelper.saveDirectoryRelations(Convert.ToInt64(oDr[1]), goRequest["Roles"]);
                                                                moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.DirectoryRelation, Convert.ToInt64(oDr[0]));
                                                            }
                                                        }

                                                        moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.Directory, dirId);
                                                        break;
                                                    }

                                                case "Remove":
                                                    {
                                                        // remove all child relations so child objects don't get deleted
                                                        sSql = "select nRelKey from tblDirectoryRelation where nDirParentId = " + dirId;
                                                        using (var oDr = moDbHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                                                        {
                                                            while (oDr.Read())
                                                                moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.DirectoryRelation, Convert.ToInt64(oDr[0]));
                                                        }

                                                        moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.Directory, dirId);
                                                        break;
                                                    }

                                                default:
                                                    {
                                                        break;
                                                    }
                                                    // do nothing
                                            }

                                            break;
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

                public virtual XmlElement xFrmPagePermissions(long id)
                {

                    string cDirectorySchemas = "Group,Role";
                    string[] aDirectorySchemas;

                    XmlElement oFrmElmt;
                    XmlElement oFrmGrp1;
                    XmlElement oFrmGrp2;
                    XmlElement oFrmGrp3;


                    XmlElement oElmt2;

                    XmlElement oElmt4;

                    string sSql;



                    string cProcessInfo = "";

                    try
                    {

                        // Check for schema overloads
                        if (!string.IsNullOrEmpty("" + goConfig["AdminPagePermissionSchemas"]))
                        {

                            cDirectorySchemas = goConfig["AdminPagePermissionSchemas"];

                        }

                        // Split the schema into an array
                        cProcessInfo = "Schemas: " + cDirectorySchemas;
                        aDirectorySchemas = cDirectorySchemas.Split(',');

                        // load the xform to be edited
                        base.NewFrm("EditPagePermissions");

                        base.submission("EditInputPagePermissions", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "EditPermissions", "3col", "Permissions for Page - " + moDbHelper.getNameByKey(Cms.dbHelper.objectTypes.ContentStructure, id));

                        oFrmGrp1 = base.addGroup(ref oFrmElmt, "AllObjects", "", "System User Groups &amp; Roles");

                        // add the buttons so we can test for submission
                        oFrmGrp2 = base.addGroup(ref oFrmElmt, "EditPermissions", "PermissionButtons", "Set Selected Group Permissions");
                        // MyBase.addSubmit(oFrmGrp2, "AddAll", "Add All >", "", "PermissionButtons")
                        base.addSubmit(ref oFrmGrp2, "AllowSelected", "Allow Selected", "", "PermissionButton btn-allow", "fa-arrow-right");
                        base.addSubmit(ref oFrmGrp2, "DenySelected", "Deny Selected", "", "PermissionButton btn-deny", "fa-arrow-right");
                        base.addSubmit(ref oFrmGrp2, "RemoveSelected", "Remove Selected", "", "PermissionButton btn-remove", "fa-arrow-left");
                        base.addSubmit(ref oFrmGrp2, "RemoveAll", "Clear All Permissions - Open", "", "PermissionButton btn-clear-all", "fa-times");

                        //XmlNode argoNode = oFrmGrp2;
                        base.addNote(ref oFrmGrp2, Protean.xForm.noteTypes.Hint, "Allowing one group impicitly denies all others, only use deny permissions to further filter members of allowed groups");
                        //oFrmGrp2 = (XmlElement)argoNode;

                        // Process any submissions
                        switch (base.getSubmitted() ?? "")
                        {

                            case "AddAll":
                                {
                                    break;
                                }

                            case "AllowSelected":
                                {
                                    foreach (string cSchema in aDirectorySchemas)
                                        moDbHelper.savePermissions(Convert.ToInt64(goRequest["pgid"]), goRequest[cSchema], Cms.dbHelper.PermissionLevel.View);
                                    break;
                                }


                            case "DenySelected":
                                {
                                    foreach (string cSchema in aDirectorySchemas)
                                        moDbHelper.savePermissions(Convert.ToInt64(goRequest["pgid"]), goRequest[cSchema], Cms.dbHelper.PermissionLevel.Denied);
                                    break;
                                }

                            case "RemoveSelected":
                                {
                                    moDbHelper.savePermissions(Convert.ToInt64(goRequest["pgid"]), goRequest["Items"], Cms.dbHelper.PermissionLevel.Open);
                                    break;
                                }

                            case "RemoveAll":
                                {
                                    moDbHelper.savePermissions(Convert.ToInt64(goRequest["pgid"]), "", Cms.dbHelper.PermissionLevel.Open);
                                    break;
                                }

                        }


                        // Populate the left hand selects, grouped by Directory schema.
                        foreach (string cSchema in aDirectorySchemas)
                        {
                            oElmt2 = base.addSelect(ref oFrmGrp1, cSchema, false, cSchema, (cSchema == "User" ? "scroll_30" : "scroll_10"), Protean.xForm.ApperanceTypes.Minimal);
                            sSql = "SELECT d.nDirKey as value, d.cDirName as name from tblDirectory d " + "left outer join tblDirectoryPermission p on p.nDirId = d.nDirKey and p.nStructId = " + id + " " + "where d.cDirSchema='" + SqlFmt(cSchema) + "' and p.nPermKey is null order by d.cDirName";
                            using (var oDr = moDbHelper.getDataReaderDisposable(sSql)) // done by sonali at 12/7/22
                            {
                                base.addOptionsFromSqlDataReader(oElmt2, oDr, "name", "value");
                            }
                        }


                        // XmlNode argoNode1 = oFrmGrp1;
                        base.addNote(ref oFrmGrp1, Protean.xForm.noteTypes.Hint, "You can select multiple items by holding down CRTL whilse clicking the names");
                        // oFrmGrp1 = (XmlElement)argoNode1;


                        // Populate the allow and denied boxes.
                        oFrmGrp3 = base.addGroup(ref oFrmElmt, "PermittedObjects", "", "Assigned Permissions");

                        oElmt4 = base.addSelect(ref oFrmGrp3, "Items", false, "Allowed", "scroll_10", Protean.xForm.ApperanceTypes.Minimal);
                        sSql = "SELECT p.nDirId as value, '['+ d.cDirSchema + '] ' + d.cDirName as name from tblDirectoryPermission p " + "inner join tblDirectory d on d.nDirKey = p.nDirId " + "where p.nStructId=" + id + " and p.nAccessLevel = 2" + " order by d.cDirSchema";
                        using (var oDr = moDbHelper.getDataReaderDisposable(sSql)) // done by sonali at 12/7/22
                        {
                            base.addOptionsFromSqlDataReader(oElmt4, oDr, "name", "value");
                        }


                        oElmt4 = base.addSelect(ref oFrmGrp3, "Items", false, "Denied", "scroll_10", Protean.xForm.ApperanceTypes.Minimal);
                        sSql = "SELECT p.nDirId as value, '['+ d.cDirSchema + '] ' + d.cDirName as name from tblDirectoryPermission p " + "inner join tblDirectory d on d.nDirKey = p.nDirId " + "where p.nStructId=" + id + " and p.nAccessLevel = 0" + " order by d.cDirSchema";

                        //XmlNode argoNode2 = oFrmGrp3;
                        base.addNote(ref oFrmGrp3, Protean.xForm.noteTypes.Hint, "Please note: Permissions can also be inherited from pages above");
                        //oFrmGrp3 = (XmlElement)argoNode2;
                        using (var oDr = moDbHelper.getDataReaderDisposable(sSql)) // done by sonali at 12/7/22
                        {
                            base.addOptionsFromSqlDataReader(oElmt4, oDr, "name", "value");
                        }

                        base.Instance.InnerXml = "<permissions/>";

                        // Rights Alert - to give a user an idea that Rights exists on this page, we'll highlight
                        // this on the Rights page in an alert
                        //If moDbHelper.GetDataValue("SELECT COUNT(*) As pCount FROM tblDirectoryPermission WHERE nAccessLevel > 2 AND nStructId=" & id, , , 0) > 0 Then
                        if (Convert.ToInt32(moDbHelper.GetDataValue("SELECT COUNT(*) As pCount FROM tblDirectoryPermission WHERE nAccessLevel > 2 AND nStructId=" + id)) > 0)
                        {

                            //XmlNode argoNode3 = oFrmElmt;
                            base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "Note: there are also Rights being applied to this page.  You can view these by clicking the Rights button above.");
                            //oFrmElmt = (XmlElement)argoNode3;

                        }

                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmEditXFormGroup", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmPageRights(long id)
                {

                    string cDirectorySchemas = "Group,Role,User";
                    string[] aDirectorySchemas;


                    XmlElement oFrmElmt;
                    XmlElement oFrmGrp1;
                    XmlElement oFrmGrp2;
                    XmlElement oFrmGrp3;

                    XmlElement oElmt;
                    XmlElement oElmt2;
                    XmlElement oElmt4;
                    string sSql;
                    bool bRightsByUser = true;

                    string cProcessInfo = "";

                    try
                    {

                        // Check for schema overloads
                        if (!string.IsNullOrEmpty("" + goConfig["AdminPageRightSchemas"]))
                        {
                            cDirectorySchemas = goConfig["AdminPageRightSchemas"];
                        }

                        // Split the schema into an array
                        cProcessInfo = "Schemas: " + cDirectorySchemas;
                        aDirectorySchemas = cDirectorySchemas.Split(',');


                        // Load the xform to be edited
                        base.NewFrm("EditPagePermissions");
                        base.submission("EditInputPageRights", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "EditPermissions", "3col", "Rights for Page - " + moDbHelper.getNameByKey(Cms.dbHelper.objectTypes.ContentStructure, id));
                        oFrmGrp1 = base.addGroup(ref oFrmElmt, "AllObjects", "", "Select the items you want to have access to this page");
                        //XmlNode argoNode = oFrmGrp1;
                        base.addNote(ref oFrmGrp1, Protean.xForm.noteTypes.Hint, "You can select multiple items by holding down CTRL while clicking the names");
                        // oFrmGrp1 = (XmlElement)argoNode;


                        // Add the buttons and radios
                        oFrmGrp2 = base.addGroup(ref oFrmElmt, "EditPermissions", "PermissionButtons", "Buttons");

                        oElmt2 = base.addSelect1(ref oFrmGrp2, "Level", false, "Access Level", "multiline", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oElmt2, "View [2]", "2");
                        base.addOption(ref oElmt2, "Add [3]", "3");
                        base.addOption(ref oElmt2, "Add and Update Own [4]", "4");
                        base.addOption(ref oElmt2, "Update All  [5]", "5");
                        base.addOption(ref oElmt2, "Approve All  [6]", "6");
                        base.addOption(ref oElmt2, "Add, Update and Publish Own [7]", "7");
                        base.addOption(ref oElmt2, "Publish All [8]", "8");
                        base.addOption(ref oElmt2, "Full [9]", "9");

                        // Add All does nothing, probably is not good for this screen either.
                        // MyBase.addSubmit(oFrmGrp2, "AddAll", "Add All", "", "PermissionButtons btn-all")
                        base.addSubmit(ref oFrmGrp2, "AddSelected", "Add Selected", "", "PermissionButton btn-add");
                        base.addSubmit(ref oFrmGrp2, "RemoveSelected", "Remove Selected", "", "PermissionButton btn-remove");
                        base.addSubmit(ref oFrmGrp2, "RemoveAll", "Clear All", "", "PermissionButton btn-clear-all");


                        // Save the permissions on submission
                        switch (base.getSubmitted() ?? "")
                        {

                            // Case "AddAll"

                            case "AddSelected":
                                {
                                    foreach (string cSchema in aDirectorySchemas)
                                    {
                                        if ((cSchema != "User" | bRightsByUser) & !string.IsNullOrEmpty("" + goRequest[cSchema]))
                                            moDbHelper.savePermissions(Convert.ToInt64(goRequest["pgid"]), goRequest[cSchema], (Cms.dbHelper.PermissionLevel)Convert.ToInt16(goRequest["Level"]));
                                    }

                                    break;
                                }

                            case "RemoveSelected":
                                {
                                    foreach (string cSchema in aDirectorySchemas)
                                    {
                                        if ((cSchema != "User" | bRightsByUser) & !string.IsNullOrEmpty("" + goRequest["Items" + cSchema]))
                                            moDbHelper.savePermissions(Convert.ToInt64(goRequest["pgid"]), goRequest["Items" + cSchema], Cms.dbHelper.PermissionLevel.Open);
                                    }

                                    break;
                                }

                            case "RemoveAll":
                                {
                                    moDbHelper.savePermissions(Convert.ToInt64(goRequest["pgid"]), "", Cms.dbHelper.PermissionLevel.Open);
                                    break;
                                }

                        }


                        // Add the left hand selection boxes.
                        foreach (string cSchema in aDirectorySchemas)
                        {
                            // Run this for all schemas except users, unless bRightsByUser is True
                            if (cSchema != "User" | bRightsByUser)
                            {
                                oElmt = base.addSelect(ref oFrmGrp1, cSchema, false, cSchema, (cSchema == "User" ? "scroll_30" : "scroll_10"), Protean.xForm.ApperanceTypes.Minimal);
                                sSql = "SELECT d.nDirKey as value, d.cDirName as name from tblDirectory d " + "left outer join tblDirectoryPermission p on p.nDirId = d.nDirKey and p.nStructId = " + id + " " + "where d.cDirSchema='" + SqlFmt(cSchema) + "' and p.nPermKey is null order by d.cDirName";
                                using (var oDr = moDbHelper.getDataReaderDisposable(sSql)) // done by sonali at 12/7/22
                                {
                                    base.addOptionsFromSqlDataReader(oElmt, oDr, "name", "value");
                                }
                            }
                        }


                        // Add the right hand selected lists.
                        oFrmGrp3 = base.addGroup(ref oFrmElmt, "PermittedObjects", "", "All items with permissions to access page");
                        //XmlNode argoNode1 = oFrmGrp3;
                        base.addNote(ref oFrmGrp3, Protean.xForm.noteTypes.Hint, "Please note: Permissions can also be inherited from pages above");
                        //oFrmGrp3 = (XmlElement)argoNode1;


                        foreach (string cSchema in aDirectorySchemas)
                        {
                            // Run this for all schemas except users, unless bRightsByUser is True
                            if (cSchema != "User" | bRightsByUser)
                            {

                                oElmt4 = base.addSelect(ref oFrmGrp3, "Items" + cSchema, false, "Allowed " + cSchema, (cSchema == "User" ? "scroll_30" : "scroll_10"), Protean.xForm.ApperanceTypes.Minimal);
                                sSql = "SELECT p.nDirId as value, '['+ str(p.nAccessLevel) + '] ' + d.cDirName as name from tblDirectoryPermission p " + "inner join tblDirectory d on d.nDirKey = p.nDirId " + "where p.nStructId=" + id + " and d.cDirSchema = '" + SqlFmt(cSchema) + "' " + "order by d.cDirSchema";
                                using (var oDr = moDbHelper.getDataReaderDisposable(sSql)) // done by sonali at 12/7/22
                                {
                                    base.addOptionsFromSqlDataReader(oElmt4, oDr, "name", "value");
                                }
                            }
                        }

                        base.Instance.InnerXml = "<permissions/>";

                        // Permissions Alert - to give a user an idea that permissions exists on this page, we'll highlight
                        // this on the rights page in an alert
                        if (Convert.ToInt32(moDbHelper.GetDataValue("SELECT COUNT(*) As pCount FROM tblDirectoryPermission WHERE nAccessLevel IN (0,2) AND nStructId=" + id, CommandType.Text, null, (object)0)) > 0)
                        {

                            // XmlNode argoNode2 = oFrmElmt;
                            base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "Note: there are also Permissions being applied to this page.  You can view these by clicking the Permissions button above.");
                            //oFrmElmt = (XmlElement)argoNode2;

                        }

                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmPageRights", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }


                public XmlElement xFrmUserMemberships(long UserId)
                {

                    XmlElement oFrmElmt;
                    XmlElement oFrmGrp1;
                    XmlElement oFrmGrp2;
                    XmlElement oFrmGrp3;

                    XmlElement oElmt1;
                    XmlElement oElmt2;

                    XmlElement oElmt4;
                    string sSql;

                    string cProcessInfo = "";

                    try
                    {
                        // load the xform to be edited

                        base.NewFrm("EditUserMemberships");

                        base.submission("EditUserMemberships", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "EditUserMemberships", "3col", "Memberships for User - ");

                        oFrmGrp1 = base.addGroup(ref oFrmElmt, "AllObjects", "", "Select the groups you want this user to belong too");
                        // XmlNode argoNode = oFrmGrp1;
                        base.addNote(ref oFrmGrp1, Protean.xForm.noteTypes.Hint, "You can select multiple items by holding down CRTL whilse clicking the names");
                        // oFrmGrp1 = (XmlElement)argoNode;

                        // add the buttons so we can test for submission
                        oFrmGrp2 = base.addGroup(ref oFrmElmt, "EditPermissions", "PermissionButtons", "Buttons");

                        base.addSubmit(ref oFrmGrp2, "AddSelected", "Add Selected >", "", "PermissionButton btn-add");
                        base.addSubmit(ref oFrmGrp2, "RemoveSelected", "< Remove Selected", "", "PermissionButton btn-remove");
                        base.addSubmit(ref oFrmGrp2, "Finish", "Finish Editing", "", "PermissionButton btmn-finish");

                        switch (base.getSubmitted() ?? "")
                        {

                            case "AddSelected":
                                {
                                    moDbHelper.saveDirectoryRelations(Convert.ToInt64(goRequest["id"]), goRequest["Departments"]);
                                    moDbHelper.saveDirectoryRelations(Convert.ToInt64(goRequest["id"]), goRequest["Groups"]);
                                    break;
                                }

                            case "RemoveSelected":
                                {
                                    moDbHelper.saveDirectoryRelations(Convert.ToInt64(goRequest["id"]), goRequest["Items"], true);
                                    break;
                                }
                            case "Finish":
                                {
                                    base.valid = true;
                                    break;
                                }

                        }

                        oElmt1 = base.addSelect(ref oFrmGrp1, "Departments", false, "Departments", "scroll_10", Protean.xForm.ApperanceTypes.Minimal);

                        // sSql = "SELECT d.nDirKey as value, d.cDirName as name from tblDirectory d " & _
                        // "left outer join tblDirectoryRelation dr on dr.nDirParentId = d.nDirKey and dr.nDirChildId = " & UserId & " " & _
                        // "where d.cDirSchema='Department' and dr.nRelKey is null order by d.cDirName"

                        sSql = "execute getUsersCompanyDepartments @userId=" + UserId + ", @adminUserId=" + myWeb.mnUserId;

                        using (var oDr = moDbHelper.getDataReaderDisposable(sSql)) // done by sonali at 12/7/22
                        {

                            base.addOptionsFromSqlDataReader(oElmt1, oDr, "name", "value");
                        }


                        oElmt2 = base.addSelect(ref oFrmGrp1, "Groups", false, "Groups", "scroll_10", Protean.xForm.ApperanceTypes.Minimal);

                        sSql = "execute getUsersCompanyGroups @userId=" + UserId + ", @adminUserId=" + myWeb.mnUserId;

                        using (var oDr = moDbHelper.getDataReaderDisposable(sSql)) // done by sonali at 12/7/22
                        {
                            base.addOptionsFromSqlDataReader(oElmt2, oDr, "name", "value");
                        }


                        oFrmGrp3 = base.addGroup(ref oFrmElmt, "PermittedObjects", "", "User is Member of...");
                        //XmlNode argoNode1 = oFrmGrp3;
                        base.addNote(ref oFrmGrp3, Protean.xForm.noteTypes.Hint, "Please note: Permissions can also be inherited from pages above");
                        //oFrmGrp3 = (XmlElement)argoNode1;

                        oElmt4 = base.addSelect(ref oFrmGrp3, "Items", false, "Allowed", "scroll_30", Protean.xForm.ApperanceTypes.Minimal);

                        sSql = "SELECT d.nDirKey as value, '['+ d.cDirSchema + '] ' + d.cDirName as name from tblDirectory d " + "inner join tblDirectoryRelation dr on dr.nDirParentId = d.nDirKey and dr.nDirChildId = " + UserId + " " + "where d.cDirSchema='Group' or d.cDirSchema='Department'  order by d.cDirName";
                        using (var oDr = moDbHelper.getDataReaderDisposable(sSql)) // done by sonali at 12/7/22
                        {
                            base.addOptionsFromSqlDataReader(oElmt4, oDr, "name", "value");
                        }


                        base.Instance.InnerXml = "<memberships/>";

                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmUserMemberships", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }


                public virtual XmlElement xFrmDirMemberships(string rootType, long DirId, long DirParId, string ChildTypes)
                {
                    XmlElement oFrmElmt;

                    XmlElement oFrmAll;
                    XmlElement oFrmChosen;

                    XmlElement oFrmButtons;



                    XmlElement oElmt1;

                    XmlElement oElmt4;

                    string sSql;
                    string[] aChildTypes = ChildTypes.Split(',');
                    string aChildDesc = "";
                    int i;

                    string cProcessInfo = "";

                    try
                    {
                        // Enumerate the childtypes
                        var loopTo = aChildTypes.Length - 1;
                        for (i = 0; i <= loopTo; i++)
                        {
                            if (i == aChildTypes.Length - 1)
                            {
                                aChildDesc = aChildDesc + " and " + aChildTypes[i] + "s";
                            }
                            else
                            {
                                aChildDesc = aChildDesc + ", " + aChildTypes[i] + "s";
                            }
                        }

                        // Get the description
                        if (aChildTypes.Length - 1 == 0)
                        {
                            aChildDesc = aChildDesc.Substring(aChildDesc.Length - (aChildDesc.Length - 5)).ToLower();
                        }
                        else
                        {
                            aChildDesc = aChildDesc.Substring(aChildDesc.Length - (aChildDesc.Length - 2)).ToLower();
                        }

                        // Create the form
                        base.NewFrm("EditMemberships");
                        base.submission("EditMemberships", "", "post");


                        // Create the groups
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "EditUserMemberships", "3col", "Memberships for " + rootType + " - " + moDbHelper.getNameByKey(Cms.dbHelper.objectTypes.Directory, DirId));
                        //XmlNode argoNode = oFrmElmt;
                        base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Hint, "You can select multiple items by holding down CTRL whilst clicking the names");
                        //oFrmElmt = (XmlElement)argoNode;

                        // Create a group for all directory items
                        oFrmAll = base.addGroup(ref oFrmElmt, "AllObjects", "", "Add " + aChildDesc + " to " + rootType);
                        //XmlNode argoNode1 = oFrmAll;
                        base.addNote(ref oFrmAll, Protean.xForm.noteTypes.Hint, "Select the " + aChildDesc + " you would like to belong to this " + rootType.ToLower());
                        //oFrmAll = (XmlElement)argoNode1;

                        // Create a middle column
                        oFrmButtons = base.addGroup(ref oFrmElmt, "SubmissionButtons", "PermissionButtons", "Actions");

                        // Create a group for chosen directory items
                        oFrmChosen = base.addGroup(ref oFrmElmt, "PermittedObjects", "", rootType + " Contains");
                        // MyBase.addNote(oFrmGrp2, xForm.noteTypes.Hint, "Please note: Permissions can also be inherited from pages above")


                        // Add submit buttons (group specified by issue 1362)
                        base.addSubmit(ref oFrmButtons, "AddSelected", "Add Selected", "", "PermissionButton btn-add");
                        base.addSubmit(ref oFrmButtons, "RemoveSelected", "Remove Selected", "", "PermissionButton btn-remove");
                        base.addSubmit(ref oFrmButtons, "Finish", "Finish Editing", "", "principle PermissionButton btn-finish");

                        // lets add / remove before we populate
                        switch (base.getSubmitted() ?? "")
                        {
                            case "AddSelected":
                                {
                                    var loopTo1 = aChildTypes.Length - 1;
                                    for (i = 0; i <= loopTo1; i++)
                                        moDbHelper.saveDirectoryRelations(Convert.ToInt64(goRequest["id"]), goRequest[aChildTypes[i] + "s"], false, Cms.dbHelper.RelationType.Child);
                                    break;
                                }
                            case "RemoveSelected":
                                {
                                    moDbHelper.saveDirectoryRelations(Convert.ToInt64(goRequest["id"]), goRequest["Items"], true, Cms.dbHelper.RelationType.Child);
                                    break;
                                }
                        }

                        // populate the add boxes

                        var loopTo2 = aChildTypes.Length - 1;
                        for (i = 0; i <= loopTo2; i++)
                        {
                            switch (aChildTypes[i] ?? "")
                            {
                                case "User":
                                    {
                                        oElmt1 = base.addSelect(ref oFrmAll, aChildTypes[i] + "s", false, aChildTypes[i] + "s", "scroll_15 alphasort", Protean.xForm.ApperanceTypes.Minimal);
                                        break;
                                    }

                                default:
                                    {
                                        oElmt1 = base.addSelect(ref oFrmAll, aChildTypes[i] + "s", false, aChildTypes[i] + "s", "scroll_10 alphasort", Protean.xForm.ApperanceTypes.Minimal);
                                        break;
                                    }
                            }

                            sSql = "";

                            if (DirParId != 0L)
                            {
                                if (Tools.Number.IsNumeric(DirParId) && Tools.Number.IsNumeric(DirId))
                                {
                                    sSql = "SELECT d.nDirKey as value, d.cDirName as name, d.cDirXml as detail " + "FROM tblDirectory d " + "     INNER JOIN tblAudit a on nAuditId = a.nAuditKey " + "     INNER JOIN tblDirectoryRelation dr on d.nDirKey = dr.nDirChildId " + "     LEFT JOIN tblDirectoryRelation dr2 on dr2.nDirChildId = d.nDirKey and dr2.nDirParentId =  " + SqlFmt(DirId.ToString()) + " " + "WHERE d.cDirSchema = " + Database.SqlString(aChildTypes[i].Trim()) + "  " + "     AND dr.nDirParentId =  " + SqlFmt(DirParId.ToString()) + " " + "     AND dr2.nRelKey is null " + "     AND (a.nStatus =1 or a.nStatus = -1) " + "ORDER BY d.cDirName ";








                                }
                            }
                            else if (Tools.Number.IsNumeric(DirId))
                            {
                                sSql = "SELECT d.nDirKey as value, d.cDirName as name, d.cDirXml as detail " + "FROM tblDirectory d " + "     INNER JOIN tblAudit a on nAuditId = a.nAuditKey " + "     LEFT JOIN tblDirectoryRelation dr2 on dr2.nDirChildId = d.nDirKey and dr2.nDirParentId =  " + SqlFmt(DirId.ToString()) + " " + "WHERE d.cDirSchema = " + Database.SqlString(aChildTypes[i].Trim()) + "  " + "     AND dr2.nRelKey is null " + "     AND (a.nStatus =1 or a.nStatus = -1) " + "ORDER BY d.cDirName ";






                            }
                            if (!string.IsNullOrEmpty(sSql))
                            {
                                using (var oDr = moDbHelper.getDataReaderDisposable(sSql)) // done by sonali at 12/7/22
                                {
                                    if (aChildTypes[i] == "User")
                                    {
                                        var argoDr = oDr;
                                        addUserOptionsFromSqlDataReader(ref oElmt1, ref argoDr, "name", "value");
                                    }
                                    else
                                    {
                                        addOptionsFromSqlDataReader(oElmt1, oDr, "name", "value");
                                    }
                                }
                            }

                        }

                        // populate the allowed boxes
                        oElmt4 = base.addSelect(ref oFrmChosen, "Items", false, "Allowed", "scroll_30 alphasort", Protean.xForm.ApperanceTypes.Minimal);

                        sSql = "SELECT d.nDirKey as value, '['+ d.cDirSchema + '] ' + d.cDirName as name, d.cDirXml as detail from tblDirectory d " + "inner join tblAudit a on nAuditId = a.nAuditKey inner join tblDirectoryRelation dr on dr.nDirChildId = d.nDirKey and dr.nDirParentId = " + DirId + " " + "where ";
                        var loopTo3 = aChildTypes.Length - 1;
                        for (i = 0; i <= loopTo3; i++)
                        {
                            if (i == 0)
                            {
                                sSql = sSql + "d.cDirSchema='" + aChildTypes[i].Trim() + "'";
                            }
                            else
                            {
                                sSql = sSql + " or d.cDirSchema='" + aChildTypes[i].Trim() + "'";
                            }
                        }
                        sSql = sSql + "and (a.nStatus =1 or a.nStatus = -1) order by d.cDirName";
                        using (var oDr = moDbHelper.getDataReaderDisposable(sSql)) // done by sonali at 12/7/22
                        {
                            var argoDr2 = oDr;
                            addUserOptionsFromSqlDataReader(ref oElmt4, ref argoDr2, "name", "value");
                        }



                        if (base.getSubmitted() == "Finish")
                            base.valid = true;

                        base.Instance.InnerXml = "<memberships/>";

                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmDirMemberships", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }


                public XmlElement xFrmEditDirectoryContact(long id = 0L, long nUID = 0, string xFormPath = "/xforms/directory/UserContact.xml")
                {
                    string cProcessInfo = "";
                    try
                    {
                        if (xFormPath == "/xforms/directory/UserContact.xml" && myWeb.bs5)
                        {
                            xFormPath = "/features/membership/UserContact.xml";
                        }

                        base.NewFrm("EditContact");
                        base.load(xFormPath, myWeb.maCommonFolders);

                        if (id > 0L)
                        {
                            base.Instance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.CartContact, id);

                        }

                        // Add the countries list to the form
                        if (base.moXformElmt.SelectSingleNode("//select1[@bind='cContactCountry']") != null)
                        {
                            var oEc = new Cms.Cart(ref myWeb);
                            Cms.xForm argoXform = (Cms.xForm)this;
                            XmlElement argoCountriesDropDown = (XmlElement)base.moXformElmt.SelectSingleNode("//select1[@bind='cContactCountry']");
                            oEc.populateCountriesDropDown(ref argoXform, ref argoCountriesDropDown, "Billing Address");
                            oEc.close();
                            oEc = (Cms.Cart)null;
                        }

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            // MyBase.instance.SelectSingleNode("tblCartContact/nContactDirId").InnerText = myweb.mnUserId
                            base.addValues();
                            if (nUID > 0)
                            {
                                // if not supplied we do not want to overwrite it.
                                base.Instance.SelectSingleNode("tblCartContact/nContactDirId").InnerText = nUID.ToString();
                            }

                            base.validate();
                            if (base.valid)
                            {
                                moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartContact, base.Instance, id > 0L ? id : -1);
                                try
                                {
                                    var argoNode = base.moXformElmt.SelectSingleNode("group/group(1)");
                                    base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, "Successfully Updated", true);
                                }
                                catch (Exception)
                                {
                                    var argoNode1 = base.moXformElmt.SelectSingleNode("group");
                                    base.addNote(ref argoNode1, Protean.xForm.noteTypes.Alert, "Successfully Updated", true);
                                }
                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmEditDirectoryContact", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }


                public XmlElement xFrmRegradeUser(int nUserId, long existingGroupId, string newGroupId, string FormTitle, string xFormPath, string messageId)
                {
                    string cProcessInfo = "";
                    object InstanceSessionName = "tempInstance_regrade" + nUserId.ToString();
                    try
                    {
                        // This is a generic function for a framework for all protean object.
                        // This is not intended for use but rather as an example of how xforms are processed

                        // The instance of the form needs to be saved in the session to allow repeating elements to be edited prior to saving in the database.

                        myWeb.moSession[InstanceSessionName.ToString()] = (object)null;
                        base.NewFrm(FormTitle);
                        base.bProcessRepeats = false;

                        // We load the xform from a file, it may be in local or in common folders.
                        base.load(xFormPath, myWeb.maCommonFolders);

                        // We get the instance
                        if (nUserId > 0)
                        {
                            string sNewGroupNames = "";
                            if (newGroupId.Contains(","))
                            {
                                foreach (var i in newGroupId.Split(','))
                                    sNewGroupNames = sNewGroupNames + myWeb.moDbHelper.getNameByKey(Cms.dbHelper.objectTypes.Directory, Convert.ToInt64(i.ToString())) + ", ";
                                sNewGroupNames.TrimEnd();
                                sNewGroupNames.TrimEnd(',');
                            }
                            else
                            {
                                sNewGroupNames = myWeb.moDbHelper.getNameByKey(Cms.dbHelper.objectTypes.Directory, Convert.ToInt64(newGroupId));
                            }

                            base.bProcessRepeats = true;
                            if (myWeb.moSession[InstanceSessionName.ToString()] is null)
                            {
                                var existingInstance = base.moXformElmt.OwnerDocument.CreateElement("instance");
                                XmlElement regradeUser = (XmlElement)existingInstance.AppendChild(base.moXformElmt.OwnerDocument.CreateElement("RegradeUser"));
                                regradeUser.SetAttribute("existingGroupId", existingGroupId.ToString());
                                regradeUser.SetAttribute("existingGroupName", myWeb.moDbHelper.getNameByKey(Cms.dbHelper.objectTypes.Directory, existingGroupId));
                                regradeUser.SetAttribute("newGroupId", newGroupId.ToString());
                                regradeUser.SetAttribute("newGroupName", sNewGroupNames);
                                regradeUser.SetAttribute("sendEmail", "1");

                                // Remove Messages that don't match the messageId
                                regradeUser.InnerXml = myWeb.GetUserXML((long)nUserId).OuterXml;
                                foreach (XmlElement msgNode in base.Instance.SelectNodes("RegradeUser/emailer/oBodyXML/Items/Message"))
                                {
                                    if ((msgNode.GetAttribute("id") ?? "") == (messageId ?? ""))
                                    {
                                    }
                                    // do nothing we want to keep
                                    else
                                    {
                                        msgNode.ParentNode.RemoveChild(msgNode);
                                    }
                                }

                                regradeUser.AppendChild(base.Instance.SelectSingleNode("RegradeUser/emailer"));
                                base.LoadInstance(existingInstance);
                                myWeb.moSession[InstanceSessionName.ToString()] = base.Instance;
                            }
                            else
                            {
                                base.LoadInstance(myWeb.moSession["tempInstance"].ToString());
                            }
                        }

                        moXformElmt.SelectSingleNode("descendant-or-self::instance").InnerXml = base.Instance.InnerXml;

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {

                                // Change User Group
                                moDbHelper.saveDirectoryRelations((long)nUserId, existingGroupId.ToString(), true);
                                if (newGroupId.Contains(","))
                                {
                                    foreach (var i in newGroupId.Split(','))
                                        moDbHelper.saveDirectoryRelations((long)nUserId, Convert.ToInt64(i).ToString(), false);
                                }
                                else
                                {
                                    moDbHelper.saveDirectoryRelations((long)nUserId, newGroupId, false);
                                }

                                // Send Email
                                var oMsg = new Protean.Messaging();
                                Cms.dbHelper argodbHelper = null;
                                oMsg.emailer((XmlElement)base.Instance.SelectSingleNode("RegradeUser"), base.Instance.SelectSingleNode("RegradeUser/emailer/xsltPath").InnerText, base.Instance.SelectSingleNode("RegradeUser/emailer/fromName").InnerText, base.Instance.SelectSingleNode("RegradeUser/emailer/fromEmail").InnerText, base.Instance.SelectSingleNode("RegradeUser/User/Email").InnerText, base.Instance.SelectSingleNode("RegradeUser/emailer/SubjectLine").InnerText, odbHelper: ref argodbHelper);

                                myWeb.moSession[InstanceSessionName.ToString()] = (object)null;

                            }
                        }
                        else if (base.isTriggered)
                        {
                            // we have clicked a trigger so we must update the instance
                            base.updateInstanceFromRequest();
                            // lets save the instance
                            goSession[InstanceSessionName.ToString()] = base.Instance;
                        }
                        else
                        {
                            goSession[InstanceSessionName.ToString()] = base.Instance;
                        }

                        // we populate the values onto the form.
                        base.addValues();

                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        myWeb.moSession[InstanceSessionName.ToString()] = (object)null;
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmEditUserSubscription", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }


            }
        }
    }
}