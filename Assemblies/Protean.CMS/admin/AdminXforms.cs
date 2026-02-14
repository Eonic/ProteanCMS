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
//using Microsoft.VisualBasic.CompilerServices;

using Lucene.Net.Support;
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

                public string _moduleName = "Cms.Admin.AdminXForms";
                // Private Const gbDebug As Boolean = True
                public Cms.dbHelper moDbHelper;
                public NameValueCollection goConfig; // = WebConfigurationManager.GetWebApplicationSection("protean/web")
                public bool mbAdminMode = false;
                public System.Web.HttpRequest moRequest;
                public Tools.Security.Impersonate moImp = null;
                public string ReportExportPath = "/ewcommon/tools/export.ashx?ewCmd=CartDownload";
                List<string> sImageUrlslist = new List<string>();

                // Error Handling hasn't been formally set up for AdminXforms so this is just for method invocation found in xfrmEditContent
                public new event OnErrorEventHandler OnError;

                public new delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs err);

                public string cModuleName
                {
                    get
                    {
                        return _moduleName;
                    }
                    set
                    {
                        _moduleName = value;
                    }
                }

                private void _OnError(object sender, Tools.Errors.ErrorEventArgs err)
                {
                    stdTools.returnException(ref myWeb.msException, _moduleName, err.ProcedureName, err.Exception, "", err.AddtionalInformation, gbDebug);
                }


                public AdminXforms(ref Cms aWeb) : base(ref aWeb)
                {

                    myWeb.PerfMon.Log("AdminXforms", "New");
                    try
                    {
                        myWeb = aWeb;
                        goConfig = myWeb.moConfig;
                        moDbHelper = myWeb.moDbHelper;
                        moRequest = myWeb.moRequest;

                        base.cLanguage = myWeb.mcPageLanguage;
                        if (myWeb.bs5)
                        {
                            ReportExportPath = "/ptn/tools/export.ashx?ewCmd=CartDownload";
                        }
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "New", ex, "", "", gbDebug);
                    }

                    OnError += _OnError;
                }

                public AdminXforms(ref string sException) : base(ref sException)
                {
                    OnError += _OnError;
                }

                // Public myWeb As Protean.Cms
 
                public void open(XmlDocument oPageXml)
                {
                    string cProcessInfo = "";
                    try
                    {
                        moPageXML = oPageXml;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "Open", ex, "", cProcessInfo, gbDebug);
                    }
                }

                public XmlElement xFrmGenericObject(int nObjectId, string FormTitle, string xFormPath, Cms.dbHelper.objectTypes ptnObjectType)
                {
                    string cProcessInfo = "";
                    try
                    {
                        // This is a generic function for a framework for all protean object.
                        // This is not intended for use but rather as an example of how xforms are processed

                        // The instance of the form needs to be saved in the session to allow repeating elements to be edited prior to saving in the database.
                        object InstanceSessionName = "tempInstance_" + ptnObjectType.ToString() + "_" + nObjectId.ToString();

                        base.NewFrm(FormTitle);
                        base.bProcessRepeats = false;

                        // We load the xform from a file, it may be in local or in common folders.
                        base.load(xFormPath, myWeb.maCommonFolders);

                        // We get the instance
                        if (nObjectId > 0)
                        {
                            base.bProcessRepeats = true;
                            if (myWeb.moSession[InstanceSessionName.ToString()] is null)
                            {
                                var existingInstance = base.moXformElmt.OwnerDocument.CreateElement("instance");
                                existingInstance.InnerXml = moDbHelper.getObjectInstance(ptnObjectType, (long)nObjectId).Replace("xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "").Replace("xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "");
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
                                int nCId = Convert.ToInt16(moDbHelper.setObjectInstance(ptnObjectType, base.Instance, (long)nObjectId));
                                myWeb.moSession["tempInstance"] = (object)null;
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
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmEditUserSubscription", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }


                // Check if a specific button has been pressed
                public bool isSubmittedOther(int pgid = 0)
                {
                    int nRelId;
                    var nParId = default(int);
                    // Dim oDbh As New dbHelper(myWeb)
                    bool bResult = false;

                    try
                    {
                        XmlElement oTmpNode = (XmlElement)moXformElmt.SelectSingleNode("model/instance/tblContent/nContentKey");
                        if (oTmpNode != null)
                        {
                            if (Tools.Number.IsNumeric(oTmpNode.InnerText))
                                nParId = Convert.ToInt16(oTmpNode.InnerText);
                            var bCascade = default(bool);
                            foreach (var myItem in goRequest.Form.Keys)
                            {
                                // ok, we need to check through all the things that would require a save first, 
                                // save, then do the action
                                // ###############################-SAVE IF NEEDED-########################
                                // if it has no id then its a new piece of content
                                // we need to check and save
                                // If nParId = 0 Then

                                if (myItem.ToString().StartsWith("Relate") || myItem.ToString().StartsWith("ewSubmitClone_Relate") || myItem.ToString().StartsWith("Filter"))
                                {
                                    base.updateInstanceFromRequest();
                                    base.validate();
                                    if (base.valid)
                                    {
                                        // trim the contentName to no longer than 255 chars
                                        var contentNameNode = base.Instance.SelectSingleNode("*/cContentName");
                                        string contentName = contentNameNode.InnerXml;
                                        contentNameNode.InnerXml = contentName.Length > 255 ? contentName.Substring(0, 255) : contentName;
                                        if (base.Instance.SelectSingleNode("*/bCascade").InnerXml == "true")
                                        {
                                            bCascade = true;
                                        }
                                        if (nParId > 0)
                                        {
                                            moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Content, base.Instance);
                                        }
                                        else
                                        {
                                            nParId = Convert.ToInt16(moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Content, base.Instance));
                                            moDbHelper.setContentLocation((long)pgid, (long)nParId, true, bCascade);
                                            if (goSession["mcRelAction"]?.ToString() == "Add" || goSession["mcRelAction"]?.ToString() == "Find")
                                            {
                                                moDbHelper.insertContentRelation(Convert.ToInt16(goSession["mcRelParent"]), nParId.ToString());
                                            }
                                        }
                                    }
                                    // End If
                                    // now there should be an id if all is well
                                    if (nParId == 0)
                                        return false;

                                    // remove ewSubmitClone because it gets added by js disablebutton
                                    string[] relateCmdArr = Convert.ToString(myItem).Replace("ewSubmitClone_", "").Split('_');

                                    // ###############################-REORDER-########################
                                    goSession["mnContentRelationParent"] = (object)null;
                                    goSession["mcRelRedirectString"] = (object)null;
                                    goSession["mcRelAction"] = (object)null;
                                    goSession["mcRelParent"] = (object)null;
                                    goSession["mcRelType"] = (object)null;


                                    string pgidQueryString = string.IsNullOrEmpty(goRequest.QueryString["pgid"]) ? "" : "&pgid=" + goRequest.QueryString["pgid"];

                                    if (Convert.ToBoolean(myItem.ToString().Contains("RelateUp")))
                                    {
                                        nRelId = Convert.ToInt16(relateCmdArr[1]);
                                        myWeb.moDbHelper.ReorderContent((long)nParId, (long)nRelId, "MoveUp", true);
                                        bResult = true;
                                    }
                                    else if (Convert.ToBoolean(myItem.ToString().Contains("RelateDown")))
                                    {
                                        nRelId = Convert.ToInt16(relateCmdArr[1]);
                                        myWeb.moDbHelper.ReorderContent((long)nParId, (long)nRelId, "MoveDown", true);
                                        bResult = true;
                                    }
                                    else if (Convert.ToBoolean(myItem.ToString().Contains("RelateTop")))
                                    {
                                        nRelId = Convert.ToInt16(relateCmdArr[1]);
                                        myWeb.moDbHelper.ReorderContent((long)nParId, (long)nRelId, "MoveTop", true);
                                        bResult = true;
                                    }
                                    else if (Convert.ToBoolean(myItem.ToString().Contains("RelateBottom")))
                                    {
                                        nRelId = Convert.ToInt16(relateCmdArr[1]);
                                        myWeb.moDbHelper.ReorderContent((long)nParId, (long)nRelId, "MoveBottom", true);
                                        bResult = true;
                                    }
                                    // ###############################-ACTIONS-########################
                                    else if (Convert.ToBoolean(myItem.ToString().Contains("RelateEdit")))
                                    {
                                        nRelId = Convert.ToInt16(relateCmdArr[1]);
                                        goSession["mnContentRelationParent"] = "/" + myWeb.moConfig["ProjectPath"] + goRequest.QueryString["Path"] + "?ewCmd=EditContent&id=" + nParId +
                                            (string.IsNullOrEmpty(goRequest.QueryString["pgid"]) ? "" : "&pgid=" + goRequest.QueryString["pgid"]);
                                        goSession["mcRelRedirectString"] = "/" + myWeb.moConfig["ProjectPath"] + goRequest.QueryString["Path"] + "?ewCmd=EditContent&id=" + nRelId;
                                        bResult = true;
                                        break;
                                    }
                                    else if (Convert.ToBoolean(myItem.ToString().Contains("RelateRemove")))
                                    {
                                        nRelId = Convert.ToInt16(relateCmdArr[1]);
                                        myWeb.moDbHelper.RemoveContentRelation((long)nParId, (long)nRelId);
                                        bResult = true;

                                        break;
                                    }
                                    else if (Convert.ToBoolean(myItem.ToString().Contains("RelateAdd")))
                                    {

                                        goSession["mnContentRelationParent"] = "/" + myWeb.moConfig["ProjectPath"] + goRequest.QueryString["Path"] + "?ewCmd=EditContent&id=" + nParId + pgidQueryString;

                                        string cContentType = relateCmdArr[1];
                                        if (relateCmdArr.Length > 3)
                                        {
                                            goSession["mcRelRedirectString"] = "/" + myWeb.moConfig["ProjectPath"] + goRequest.QueryString["Path"] + "?ewCmd=AddContent&type=" + cContentType + "&name=New+" + cContentType + "&direction=" + relateCmdArr[2] + "&RelType=" + relateCmdArr[2] + "&relationType=" + relateCmdArr[3] + pgidQueryString;
                                        }
                                        else
                                        {
                                            goSession["mcRelRedirectString"] = "/" + myWeb.moConfig["ProjectPath"] + goRequest.QueryString["Path"] + "?ewCmd=AddContent&type=" + cContentType + "&name=New+" + cContentType + "&direction=" + relateCmdArr[2] + "&RelType=" + relateCmdArr[2] + pgidQueryString;
                                        }
                                        goSession["mcRelAction"] = "Add";
                                        goSession["mcRelParent"] = (object)nParId;
                                        bResult = true;
                                        break;
                                    }
                                    else if (Convert.ToBoolean(myItem.ToString().Contains("FilterEdit")))
                                    {
                                        string cContentType = relateCmdArr[1];
                                        nRelId = Convert.ToInt16(relateCmdArr[2]);
                                        goSession["mnContentRelationParent"] = "/" + myWeb.moConfig["ProjectPath"] + goRequest.QueryString["Path"] + "?ewCmd=EditContent&id=" + nParId +
                                            (string.IsNullOrEmpty(goRequest.QueryString["pgid"]) ? "" : "&pgid=" + goRequest.QueryString["pgid"]);
                                        goSession["mcRelRedirectString"] = "/" + myWeb.moConfig["ProjectPath"] + goRequest.QueryString["Path"] + "?ewCmd=EditContent&type=" + cContentType + "&id=" + nRelId + "&filter=true";
                                        bResult = true;
                                        break;
                                    }
                                    else if (Convert.ToBoolean(myItem.ToString().Contains("FilterAdd")))
                                    {

                                        goSession["mnContentRelationParent"] = "/" + myWeb.moConfig["ProjectPath"] + goRequest.QueryString["Path"] + "?ewCmd=EditContent&id=" + nParId + pgidQueryString + "&filter=true";

                                        string cContentType = relateCmdArr[1];
                                        if (relateCmdArr.Length > 3)
                                        {
                                            goSession["mcRelRedirectString"] = "/" + myWeb.moConfig["ProjectPath"] + goRequest.QueryString["Path"] + "?ewCmd=AddContent&type=" + cContentType + "&name=New+" + cContentType + "&direction=" + relateCmdArr[2] + "&RelType=" + relateCmdArr[2] + "&relationType=" + relateCmdArr[3] + pgidQueryString + "&filter=true";
                                        }
                                        else
                                        {
                                            goSession["mcRelRedirectString"] = "/" + myWeb.moConfig["ProjectPath"] + goRequest.QueryString["Path"] + "?ewCmd=AddContent&type=" + cContentType + "&name=New+" + cContentType + "&direction=" + relateCmdArr[2] + "&RelType=" + relateCmdArr[2] + pgidQueryString + "&filter=true";
                                        }
                                        goSession["mcRelAction"] = "Add";
                                        goSession["mcRelParent"] = (object)nParId;
                                        bResult = true;
                                        break;
                                    }
                                    else if (Convert.ToBoolean(myItem.ToString().Contains("FilterRemove")))
                                    {
                                        nRelId = Convert.ToInt16(relateCmdArr[2]);
                                        myWeb.moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.Content, (long)nRelId);
                                        bResult = true;
                                        break;
                                    }
                                    else if (Convert.ToBoolean(myItem.ToString().Contains("RelateFind")))
                                    {
                                        goSession["mnContentRelationParent"] = "/" + myWeb.moConfig["ProjectPath"] + goRequest.QueryString["Path"] + "?ewCmd=EditContent&id=" + nParId + pgidQueryString;
                                        string cContentType = relateCmdArr[1];
                                        if (relateCmdArr.Length > 3)
                                        {
                                            goSession["mcRelRedirectString"] = "/" + myWeb.moConfig["ProjectPath"] + goRequest.QueryString["Path"] + "?ewCmd=RelateSearch&type=" + cContentType + "&direction=" + relateCmdArr[2] + "&RelType=" + relateCmdArr[2] + "&relationType=" + relateCmdArr[3] + pgidQueryString;
                                        }
                                        else
                                        {
                                            goSession["mcRelRedirectString"] = "/" + myWeb.moConfig["ProjectPath"] + goRequest.QueryString["Path"] + "?ewCmd=RelateSearch&type=" + cContentType + "&direction=" + relateCmdArr[2] + "&RelType=" + relateCmdArr[2] + pgidQueryString;
                                        }
                                        goSession["mcRelAction"] = "Find";
                                        goSession["mcRelParent"] = (object)nParId;
                                        bResult = true;
                                        break;
                                    }
                                    // New condition for sku parent change functionality
                                    else if (Convert.ToBoolean(myItem.ToString().Contains("RelateParentChange")))
                                    {
                                        goSession["mnContentRelationParent"] = "/" + myWeb.moConfig["ProjectPath"] + goRequest.QueryString["Path"] + "?ewCmd=EditContent&id=" + nParId + pgidQueryString;
                                        string cContentType = relateCmdArr[1];

                                        goSession["mcRelRedirectString"] = "/" + myWeb.moConfig["ProjectPath"] + goRequest.QueryString["Path"] + "?ewCmd=ParentChange&type=" + cContentType + "&direction=" + relateCmdArr[4] + "&RelType=" + relateCmdArr[4] + "&childId=" + relateCmdArr[2] + "&oldParentID=" + nParId + pgidQueryString;

                                        goSession["mcRelAction"] = "Find";
                                        goSession["mcRelParent"] = (object)nParId;
                                        bResult = true;
                                        break;
                                    }
                                }
                            }
                        }

                        if (bResult)
                        {
                            // Reload Related Content
                            if (base.Instance.SelectSingleNode("ContentRelations") != null)
                            {
                                base.Instance.RemoveChild(base.Instance.SelectSingleNode("ContentRelations"));
                            }

                            XmlElement oCRNode;
                            oCRNode = moPageXML.CreateElement("ContentRelations");
                            moDbHelper.addRelatedContent(ref oCRNode, nParId, true);
                            base.Instance.AppendChild(oCRNode);
                        }

                        return bResult;
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "addInput", ex, "", "", gbDebug);
                        return default;
                    }
                }


                /// <summary>
                /// this routine now calls the Membership provider for this function as it has moved. It should only be being used by legacy overides.
                /// </summary>
                /// <param name="id"></param>
                /// <param name="cDirectorySchemaName"></param>
                /// <param name="parId"></param>
                /// <param name="cXformName"></param>
                /// <param name="FormXML"></param>
                /// <returns></returns>
                /// <remarks></remarks>

                //public virtual XmlElement xFrmEditDirectoryItem(long id = 0L, string cDirectorySchemaName = "User", long parId = 0L, string cXformName = "")
                //{
                //    XmlElement argIntanceAppend = null;
                //    return xFrmEditDirectoryItem(id, cDirectorySchemaName, parId, cXformName, "", ref argIntanceAppend);
                //}

                //public virtual XmlElement xFrmEditDirectoryItem(long id = 0L, string cDirectorySchemaName = "User", long parId = 0L, string cXformName = "", string FormXML = "")
                //{
                //    XmlElement argIntanceAppend = null;
                //    return xFrmEditDirectoryItem(id, cDirectorySchemaName, parId, cXformName, FormXML, ref argIntanceAppend);
                //}


                // Public Function xFrmPreviewNewsLetter(ByVal nPageId As Integer, ByRef oPageDetail As XmlElement) As XmlElement
                // Dim oFrmElmt As XmlElement


                // Dim cProcessInfo As String = ""
                // Try
                // MyBase.NewFrm("SendNewsLetter")

                // MyBase.submission("SendNewsLetter", "", "post", "")

                // oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Unpersonalised", "", "Send Unpersonalised")

                // Dim oElmt As XmlElement
                // oElmt = MyBase.addInput(oFrmElmt, "cEmail", True, "Email address to send to", "long")
                // MyBase.addBind("cEmail", "cEmail")
                // oElmt.AppendChild(oElmt.OwnerDocument.CreateElement("value"))
                // MyBase.addSubmit(oFrmElmt, "SendUnpersonalised", "Send Unpersonalised")

                // ' Uncomment for personalised
                // ' ''oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Personalised", "", "Send Personalised")
                // ' ''Dim cSQL As String = "SELECT tblDirectory.nDirKey, tblDirectory.cDirName" & _
                // ' ''" FROM tblDirectory INNER JOIN" & _
                // ' ''" tblDirectoryRelation ON tblDirectory.nDirKey = tblDirectoryRelation.nDirChildId INNER JOIN" & _
                // ' ''" tblDirectory Role ON tblDirectoryRelation.nDirParentId = Role.nDirKey" & _
                // ' ''" WHERE (tblDirectory.cDirSchema = N'User') AND (Role.cDirSchema = N'Role') AND (Role.cDirName = N'Administrator')" & _
                // ' ''" ORDER BY tblDirectory.cDirName"
                // ' ''Dim oDre As SqlDataReader = moDbhelper.getDataReader(cSQL)
                // ' ''Dim oSelElmt As XmlElement = MyBase.addSelect1(oFrmElmt, "cUsers", True, "Select admin user to send to", "short", ApperanceTypes.Minimal)
                // ' ''Do While oDre.Read
                // ' ''    MyBase.addOption(oSelElmt, oDre(1), oDre(0))
                // ' ''Loop
                // ' ''MyBase.addBind("cUsers", "cUsers")
                // ' ''MyBase.addSubmit(oFrmElmt, "SendPersonalised", "Send Personalised")

                // MyBase.Instance.InnerXml = "<cEmail/><cUsers/>"

                // If MyBase.isSubmitted Then
                // MyBase.updateInstanceFromRequest()
                // MyBase.validate()
                // Dim oEmailElmt As XmlElement = MyBase.Instance.SelectSingleNode("cEmail")
                // If Not is_valid_email(oEmailElmt.InnerText) Then
                // MyBase.addNote(oElmt, xForm.noteTypes.Alert, "Incorrect Email Address Supplied")
                // MyBase.valid = False
                // End If
                // If MyBase.valid Then
                // Dim moMailConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/mailinglist")
                // Dim cEmail As String = MyBase.Instance.SelectSingleNode("cEmail").InnerText
                // 'first we will only deal with unpersonalised
                // Dim oMessager As New Messaging
                // 'get the subject
                // Dim cSubject As String = ""
                // Dim oMessaging As New Protean.Messaging
                // If oMessaging.SendSingleMail_Queued(nPageId, moMailConfig("MailingXsl"), cEmail, moMailConfig("SenderEmail"), moMailConfig("SenderName"), cSubject) Then
                // 'add mssage and return to form so they can sen another

                // Dim oMsgElmt As XmlElement = oPageDetail.OwnerDocument.CreateElement("Content")

                // oMsgElmt.SetAttribute("type", "Message")
                // oMsgElmt.InnerText = "Messages Sent"
                // oPageDetail.AppendChild(oMsgElmt)
                // End If
                // End If
                // End If

                // MyBase.addValues()
                // Return MyBase.moXformElmt

                // Catch ex As Exception
                // returnException(myWeb.msException, _moduleName, "addInput", ex, "", cProcessInfo, gbDebug)
                // Return Nothing
                // End Try
                // End Function

                // Public Function xFrmSendNewsLetter(ByVal nPageId As Integer, ByVal cPageName As String, ByVal cDefaultEmail As String, ByVal cDefaultEmailName As String, ByRef oPageDetail As XmlElement) As XmlElement
                // Dim oFrmElmt As XmlElement
                // Dim oCol1 As XmlElement
                // Dim oCol2 As XmlElement

                // Dim cProcessInfo As String = ""
                // Try
                // MyBase.NewFrm("SendNewsLetter")

                // MyBase.submission("SendNewsLetter", "", "post", "")

                // oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Groups", "2col", "Please select a group(s) to send to.")

                // cDefaultEmail = Trim(cDefaultEmail)

                // oCol1 = MyBase.addGroup(oFrmElmt, "", "col1", "")
                // oCol2 = MyBase.addGroup(oFrmElmt, "", "col2", "")

                // Dim oElmt As XmlElement
                // oElmt = MyBase.addInput(oCol1, "cDefaultEmail", True, "Email address to send from", "required long")
                // MyBase.addBind("cDefaultEmail", "cDefaultEmail", "true()")
                // oElmt.AppendChild(oElmt.OwnerDocument.CreateElement("value"))

                // Dim oElmt2 As XmlElement
                // oElmt2 = MyBase.addInput(oCol1, "cDefaultEmailName", True, "Name to send from", "required long")
                // MyBase.addBind("cDefaultEmailName", "cDefaultEmailName", "true()")
                // oElmt2.AppendChild(oElmt.OwnerDocument.CreateElement("value"))

                // oElmt2 = MyBase.addInput(oCol1, "cSubject", True, "Subject", "required long")
                // MyBase.addBind("cSubject", "cSubject", "true()")
                // oElmt2.AppendChild(oElmt.OwnerDocument.CreateElement("value"))


                // Dim cSQL As String = "SELECT nDirKey, cDirName  FROM tblDirectory WHERE (cDirSchema = 'Group') ORDER BY cDirName"
                // Dim oDre As SqlDataReader = moDbHelper.getDataReader(cSQL)
                // Dim oSelElmt As XmlElement = MyBase.addSelect(oCol2, "cGroups", True, "Select Groups to send to", "required multiline", ApperanceTypes.Full)
                // Do While oDre.Read
                // MyBase.addOption(oSelElmt, oDre(1), oDre(0))
                // Loop
                // MyBase.addBind("cGroups", "cGroups", "true()")

                // oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Send", "", "")

                // MyBase.addSubmit(oFrmElmt, "SendUnpersonalised", "Send Unpersonalised")
                // ' Uncomment for personalised
                // 'MyBase.addSubmit(oFrmElmt, "SendPersonalised", "Send Personalised")

                // MyBase.Instance.InnerXml = "<cGroups/><cDefaultEmail>" & cDefaultEmail & "</cDefaultEmail><cDefaultEmailName>" & cDefaultEmailName & "</cDefaultEmailName><cSubject>" & cPageName & "</cSubject>"

                // If MyBase.isSubmitted Then
                // MyBase.updateInstanceFromRequest()
                // MyBase.validate()
                // Dim oEmailElmt As XmlElement = MyBase.Instance.SelectSingleNode("cDefaultEmail")
                // If Not Tools.Text.IsEmail(oEmailElmt.InnerText.Trim()) Then
                // MyBase.addNote(oElmt, xForm.noteTypes.Alert, "Incorrect Email Address Supplied")
                // MyBase.valid = False
                // End If
                // If MyBase.valid Then
                // Dim moMailConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/mailinglist")
                // 'get the individual elements
                // Dim oMessaging As New Protean.Messaging
                // 'First we need to get the groups we are sending to
                // Dim oGroupElmt As XmlElement = MyBase.Instance.SelectSingleNode("cGroups")
                // Dim oFromEmailElmt As XmlElement = MyBase.Instance.SelectSingleNode("cDefaultEmail")
                // Dim oFromNameElmt As XmlElement = MyBase.Instance.SelectSingleNode("cDefaultEmailName")
                // Dim oSubjectElmt As XmlElement = MyBase.Instance.SelectSingleNode("cSubject")
                // 'get the email addresses for these groups

                // Dim bResult As Boolean = oMessaging.SendMailToList_Queued(nPageId, moMailConfig("MailingXsl"), oGroupElmt.InnerText, oFromEmailElmt.InnerText, oFromNameElmt.InnerText, oSubjectElmt.InnerText)


                // ' Log the result
                // If bResult Then
                // 'moDbHelper.logActivity(dbHelper.ActivityType.Email, myWeb.mnUserId, nPageId, , oGroupElmt.InnerText)
                // moDbHelper.CommitLogToDB(dbHelper.ActivityType.NewsLetterSent, myWeb.mnUserId, myWeb.moSession.SessionID, Now, myWeb.mnPageId, 0, "", True)
                // Dim cGroupStr As String = "<Groups><Group>" & Replace(oGroupElmt.InnerText, ",", "</Group><Group>") & "</Group></Groups>"
                // 'add mssage and return to form so they can sen another
                // Dim oMsgElmt As XmlElement = oPageDetail.OwnerDocument.CreateElement("Content")
                // oMsgElmt.SetAttribute("type", "Message")
                // oMsgElmt.InnerText = "Messages Sent"
                // oPageDetail.AppendChild(oMsgElmt)
                // End If
                // End If
                // End If

                // MyBase.addValues()
                // Return MyBase.moXformElmt

                // Catch ex As Exception
                // returnException(myWeb.msException, _moduleName, "addInput", ex, "", cProcessInfo, gbDebug)
                // Return Nothing
                // End Try
                // End Function

                public XmlElement xFrmAdminOptOut()
                {
                    XmlElement oFrmElmt;


                    string cProcessInfo = "";
                    try
                    {
                        base.NewFrm("OptOut");

                        base.submission("OptOut", "", "post", "return form_check(this)");




                        oFrmElmt = base.addGroup(ref base.moXformElmt, "Existing", "", "Add Opt-Out Address");
                        XmlElement oElmt;
                        oElmt = base.addInput(ref oFrmElmt, "cEmail", true, "Add Address", "long");
                        oElmt.AppendChild(oElmt.OwnerDocument.CreateElement("value"));
                        XmlElement argoBindParent = null;
                        base.addBind("cEmail", "cEmail", oBindParent: ref argoBindParent);
                        base.addSubmit(ref oFrmElmt, "AddOptOut", "Add to List");

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "Existing", "", "Existing Opt-Out Addresses");

                        var oSelElmt = base.addSelect(ref oFrmElmt, "OptIn", true, "Addresses", "block scroll", Protean.xForm.ApperanceTypes.Full);
                        string cSQL = "SELECT EmailAddress FROM tblOptOutAddresses ORDER BY EmailAddress";
                        using (var oDre = moDbHelper.getDataReaderDisposable(cSQL))  // Done by nita on 6/7/22
                        {
                            while (oDre.Read())
                                base.addOption(ref oSelElmt, Convert.ToString(oDre[0]), Convert.ToString(oDre[0]));
                        }
                        XmlElement argoBindParent1 = null;
                        base.addBind("OptIn", "OptIn", oBindParent: ref argoBindParent1);
                        base.addSubmit(ref oFrmElmt, "RemoveOptOut", "Remove from List");

                        base.Instance.InnerXml = "<cEmail/><OptIn/>";

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            XmlElement oEmailElmt = (XmlElement)base.Instance.SelectSingleNode("cEmail");

                            if (base.valid)
                            {
                                if (!string.IsNullOrEmpty(oEmailElmt.InnerText))
                                {
                                    if (!IsEmail(oEmailElmt.InnerText))
                                    {
                                        //XmlNode argoNode = oElmt;
                                        base.addNote(ref oElmt, Protean.xForm.noteTypes.Alert, "Incorrect Email Address Supplied");
                                        //oElmt = (XmlElement)argoNode;
                                    }
                                    else if (moDbHelper.AddInvalidEmail(oEmailElmt.InnerText))
                                    {
                                        //XmlNode argoNode1 = oElmt;
                                        base.addNote(ref oElmt, Protean.xForm.noteTypes.Hint, oEmailElmt.InnerText + " Added");
                                        //oElmt = (XmlElement)argoNode1;
                                        base.addOption(ref oSelElmt, oEmailElmt.InnerText, oEmailElmt.InnerText);
                                        oEmailElmt.InnerText = "";
                                    }
                                    else
                                    {
                                        //XmlNode argoNode2 = oElmt;
                                        base.addNote(ref oElmt, Protean.xForm.noteTypes.Hint, oEmailElmt.InnerText + "Already Exists");
                                        //oElmt = (XmlElement)argoNode2;
                                        oEmailElmt.InnerText = "";
                                    }
                                }
                                XmlElement oRemoveElmt = (XmlElement)base.Instance.SelectSingleNode("OptIn");
                                if (!string.IsNullOrEmpty(oRemoveElmt.InnerText))
                                {
                                    moDbHelper.RemoveInvalidEmail(oRemoveElmt.InnerText);
                                    //XmlNode argoNode3 = (XmlNode)oSelElmt;
                                    base.addNote(ref oSelElmt, Protean.xForm.noteTypes.Hint, oRemoveElmt.InnerText + " Removed");
                                    //oSelElmt = (XmlElement)argoNode3;
                                    oRemoveElmt.InnerText = "";
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

                public XmlElement xFrmSchedulerItem(string cActionType, int nSiteId, string sSchedCon, int nID = 0)
                {
                    string cProcessInfo = "";
                    try
                    {
                        var dbh = new Cms.dbHelper( myWeb);
                        dbh.ResetConnection(sSchedCon);

                        base.NewFrm("EditScheduleItem");

                        if (goConfig["cssFramework"] == "bs5")
                        {
                            base.load("/admin/xforms/ScheduledItems/" + cActionType + ".xml", myWeb.maCommonFolders);
                        }
                        else
                        {
                            base.load("/xforms/ScheduledItems/" + cActionType + ".xml", myWeb.maCommonFolders);
                        }

                        if (nID > 0)
                        {
                            base.Instance.InnerXml = dbh.getObjectInstance(Cms.dbHelper.objectTypes.ScheduledItem, (long)nID);
                        }

                        // get menu
                        XmlElement oPageSelect = (XmlElement)base.moXformElmt.SelectSingleNode("descendant-or-self::select1[@bind='nPageId']");
                        if (oPageSelect != null)
                        {
                            MenuSelect(ref oPageSelect);
                        }
                        // get files
                        XmlElement oXSLSelect = (XmlElement)base.moXformElmt.SelectSingleNode("descendant-or-self::select1[@bind='cXSLPath']");
                        if (oXSLSelect != null)
                        {
                            if (goConfig["cssFramework"] == "bs5")
                            {

                                FileList("/feeds/", ref oXSLSelect, ".xsl");
                            }
                            else
                            {

                                FileList("/xsl/feeds/", ref oXSLSelect, ".xsl");
                            }
                        }
                        // set siteid
                        XmlElement oSiteIDElmt = (XmlElement)base.Instance.SelectSingleNode("descendant-or-self::nWebsite");
                        oSiteIDElmt.InnerText = nSiteId.ToString();

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            // check the min interval

                            NameValueCollection oSchedulerConfig = (NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/scheduler");

                            XmlElement oMainFrequency = (XmlElement)base.Instance.SelectSingleNode("tblActions/nFrequency");
                            if (oMainFrequency != null)
                            {
                                if (!(Convert.ToInt16(oMainFrequency.InnerText) >= Convert.ToInt16(oSchedulerConfig["MinimumInterval"])))
                                {
                                    base.valid = false;
                                }
                            }

                            if (base.valid)
                            {
                                // now we need to save it 
                                dbh.setObjectInstance(Cms.dbHelper.objectTypes.ScheduledItem, base.Instance);
                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmSchedulerItem", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }
                #region Temp subs for Scheduled Items
                public void MenuSelect(ref XmlElement oSelect)
                {
                    try
                    {
                        var oWeb = new Cms();
                        oWeb.Open();
                        var oMenuElmt = myWeb.GetStructureXML((long)myWeb.mnUserId, 0L, 0L, "Site", false, false, false, true, false, "MenuItem", "Menu");
                        foreach (XmlElement oMenuItem in oMenuElmt.SelectNodes("MenuItem"))
                            MenuReiterate(oMenuItem, ref oSelect, 0);
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "MenuSelect", ex, "", "", gbDebug);
                    }
                }
                public void MenuReiterate(XmlElement oMenuItem, ref XmlElement oSelect, int nDepth)
                {
                    try
                    {
                        string cNameString = "";
                        int i;
                        var loopTo = nDepth;
                        for (i = 0; i <= loopTo; i++)
                            cNameString += "-";
                        base.addOption(ref oSelect, cNameString + oMenuItem.GetAttribute("name"), oMenuItem.GetAttribute("id"));
                        foreach (XmlElement oSubelmt in oMenuItem.SelectNodes("MenuItem"))
                            MenuReiterate(oSubelmt, ref oSelect, nDepth + 1);
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "MenuReiterate", ex, "", "", gbDebug);
                    }
                }

                public void FileList(string cInitialFolder, ref XmlElement oSelect, string cFileExt)
                {
                    try
                    {

                        string oNameStr = "";

                        string cBasePath = goServer.MapPath("/" + cInitialFolder);
                        string cCommonPath = goServer.MapPath("/ewcommon" + cInitialFolder);

                        if (goConfig["cssFramework"] == "bs5")
                        {
                            cCommonPath = goServer.MapPath("/ptn" + cInitialFolder);
                        }
                        var dir = new DirectoryInfo(cBasePath);

                        if (!dir.Exists)
                        {
                            dir = new DirectoryInfo(cCommonPath);
                        }

                        FileInfo[] files = dir.GetFiles();
                        FileInfo fi;
                        foreach (var currentFi in files)
                        {
                            fi = currentFi;
                            if ((fi.Extension ?? "") == (cFileExt ?? ""))
                            {
                                if (!oNameStr.Contains(fi.Name + ","))
                                {
                                    base.addOption(ref oSelect, fi.Name.Replace(cFileExt, ""), fi.FullName);
                                    oNameStr += fi.Name + ",";
                                }
                            }
                        }

                        dir = new DirectoryInfo(cCommonPath);
                        files = dir.GetFiles();
                        foreach (var currentFi1 in files)
                        {
                            fi = currentFi1;
                            if ((fi.Extension ?? "") == (cFileExt ?? ""))
                            {
                                if (!oNameStr.Contains(fi.Name + ","))
                                {
                                    base.addOption(ref oSelect, fi.Name.Replace(cFileExt, ""), fi.FullName);
                                    oNameStr += fi.Name + ",";
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "cInitialFolder", ex, "", "", gbDebug);
                    }
                }
                #endregion

                public XmlElement xFrmFeedItem(int nContentId = 0, XmlElement oInstanceElmt = null, int nPageId = 0, string cURL = "")
                {
                    string cProcessInfo = "";
                    bool existingIsDifferent = true;
                    try
                    {


                        base.NewFrm("EditFeedItem");

                        if (myWeb.moConfig["cssFramework"] == "bs5")
                        {
                            base.load("/core/xforms/content/feeditem.xml", myWeb.maCommonFolders);
                        }
                        else
                        {
                            base.load("/xforms/content/feeditem.xml", myWeb.maCommonFolders);
                        }

                        var existingInstance = base.moXformElmt.OwnerDocument.CreateElement("instance");

                        if (nContentId > 0)
                        {
                            existingInstance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.Content, (long)nContentId);
                            base.Instance.InnerXml = existingInstance.InnerXml;
                        }
                        if (oInstanceElmt != null)
                        {
                            base.Instance.InnerXml = oInstanceElmt.InnerXml;
                        }
                        if (!string.IsNullOrEmpty(cURL))
                        {
                            XmlElement oURLElmt = (XmlElement)base.Instance.SelectSingleNode("descendant-or-self::cContentXmlBrief/Content/url");
                            if (oURLElmt != null)
                            {
                                oURLElmt.InnerText = cURL;
                            }
                        }
                        // going to override som stuff here since we will be supplying the instance
                        if (base.isSubmitted() | oInstanceElmt != null)
                        {
                            if (oInstanceElmt is null)
                                base.updateInstanceFromRequest();
                            if (oInstanceElmt is null)
                                base.validate();

                            if (nContentId > 0 && oInstanceElmt != null && !string.IsNullOrEmpty(existingInstance.InnerXml) && oInstanceElmt.SelectSingleNode("//cContentXmlBrief") != null && existingInstance.SelectSingleNode("//cContentXmlBrief") != null && oInstanceElmt.SelectSingleNode("//cContentXmlDetail") != null && existingInstance.SelectSingleNode("//cContentXmlDetail") != null && (oInstanceElmt.SelectSingleNode("//cContentXmlBrief").InnerXml ?? "") == (existingInstance.SelectSingleNode("//cContentXmlBrief").InnerXml ?? "") && (oInstanceElmt.SelectSingleNode("//cContentXmlDetail").InnerXml ?? "") == (existingInstance.SelectSingleNode("//cContentXmlDetail").InnerXml ?? ""))






                            {
                                // Do nothing - don't update it.
                                existingIsDifferent = false;
                            }

                            if (base.valid | oInstanceElmt != null & existingIsDifferent)
                            {
                                // now we need to save it 
                                int id;
                                if (nContentId > 0)
                                {
                                    id = nContentId;
                                    moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Content, base.Instance, (long)id);
                                    base.moXformElmt.SetAttribute("itemupdated", "true");
                                }
                                else
                                {
                                    id = Convert.ToInt16(moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Content, base.Instance));
                                }

                                if (!(id == 0) & !(nPageId == 0))
                                {
                                    moDbHelper.setContentLocation((long)nPageId, (long)id, true);
                                }
                            }



                        }

                        base.addValues();
                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmFeedItem", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                // Moved to edit content instead

                public XmlElement xFrmFindContentToLocate(string nNewLocationPage, string nFromPage, string bIncludeChildren, string cContentType, string cSearchTerm, ref XmlElement oDetailElement)
                {
                    if (string.IsNullOrEmpty(nFromPage))
                        nFromPage = 0.ToString();
                    if (bIncludeChildren is null)
                        bIncludeChildren = "0";
                    XmlElement oFrmElmt;
                    XmlElement oSelElmt1;
                    XmlElement oSelElmt2;
                    var oTempInstance = moPageXML.CreateElement("instance");
                    //bool bCascade = false;
                    string cProcessInfo = "";

                    try
                    {
                        base.NewFrm("FindContentToRelate");
                        // nNewLocationPage 
                        var oElement = base.Instance.OwnerDocument.CreateElement("nNewLocationPage");
                        oElement.InnerText = nNewLocationPage;
                        base.Instance.AppendChild(oElement);
                        // nFromPage
                        oElement = base.Instance.OwnerDocument.CreateElement("nFromPage");
                        oElement.InnerText = nFromPage;
                        base.Instance.AppendChild(oElement);
                        // bIncludeChildren
                        oElement = base.Instance.OwnerDocument.CreateElement("bIncludeChildren");
                        oElement.InnerText = bIncludeChildren;
                        base.Instance.AppendChild(oElement);
                        // cContentType 
                        oElement = base.Instance.OwnerDocument.CreateElement("cContentType");
                        oElement.InnerText = cContentType;
                        base.Instance.AppendChild(oElement);
                        // cSearchTerm
                        oElement = base.Instance.OwnerDocument.CreateElement("cSearchTerm");
                        oElement.InnerText = cSearchTerm;
                        base.Instance.AppendChild(oElement);

                        base.submission("AddLocation", "", "post", "form_check(this)");

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "SearchContent");
                        // oGrp1Elmt = MyBase.addGroup(oFrmElmt, "Criteria", "", "")

                        // Definitions

                        // Hidden
                        base.addInput(ref oFrmElmt, "nNewLocationPage", true, "nNewLocationPage", "hidden");
                        XmlElement argoBindParent = null;
                        base.addBind("nNewLocationPage", "nNewLocationPage", oBindParent: ref argoBindParent);

                        base.addInput(ref oFrmElmt, "type", true, "cContentType", "hidden");
                        XmlElement argoBindParent1 = null;
                        base.addBind("type", "cContentType", oBindParent: ref argoBindParent1);
                        // Textbox
                        base.addInput(ref oFrmElmt, "cSearchTerm", true, "Search Expression");
                        XmlElement argoBindParent2 = null;
                        base.addBind("cSearchTerm", "cSearchTerm", oBindParent: ref argoBindParent2, "false()");

                        // Select
                        // Pages
                        oSelElmt1 = base.addSelect1(ref oFrmElmt, "nFromPage", false, "Page", "siteTree", Protean.xForm.ApperanceTypes.Minimal);
                        XmlElement argoBindParent3 = null;
                        base.addBind("nFromPage", "nFromPage", oBindParent: ref argoBindParent3, "true()");

                        // Checkbox
                        oSelElmt2 = base.addSelect(ref oFrmElmt, "bIncludeChildren", true, "&#160;", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt2, "Search Children", 1.ToString());
                        XmlElement argoBindParent4 = null;
                        base.addBind("bIncludeChildren", "bIncludeChildren", oBindParent: ref argoBindParent4, "false()");

                        // search button
                        base.addSubmit(ref oFrmElmt, "Search", "Search", "Search");
                        base.addValues();
                        oDetailElement.AppendChild(base.moXformElmt);

                        if (base.isSubmitted() | moRequest["cSearched"] == "1")
                        {
                            oDetailElement.AppendChild(xFrmLocateContent(Convert.ToInt16(nNewLocationPage), Convert.ToInt16(nFromPage), bIncludeChildren, cContentType, cSearchTerm));
                        }

                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmFindRelated", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmImportFile(string cPath)
                {
                    XmlElement oFrmElmt;
                    string sValidResponse = "";
                    string cProcessInfo = "";
                    var oImportManifestXml = new XmlDocument();

                    try
                    {
                        try
                        {
                            oImportManifestXml.Load(goServer.MapPath(myWeb.moConfig["ProjectPath"] + "/xsl/import") + "/ImportManifest.xml");
                        }
                        catch
                        {
                            // do nothing
                        }

                        if (oImportManifestXml != null)
                        {

                            base.NewFrm("ImportFile");

                            base.submission("Inport File", "", "post", "form_check(this)");

                            oFrmElmt = base.addGroup(ref base.moXformElmt, "Import file into ProteanCMS", "", "Please select the file to import");

                            XmlElement oSelectElmt;
                            oSelectElmt = base.addSelect1(ref oFrmElmt, "importXslt", true, "Import Type");
                            string sDefaultXslt = "";
                            foreach (XmlElement oChoices in oImportManifestXml.SelectNodes("/Imports/ImportGroup/Import"))
                            {
                                // Dim oChoicesElmt As XmlElement = MyBase.addChoices(oSelectElmt, oChoices.GetAttribute("name"))
                                // For Each oItem In oChoices.SelectNodes("Import")
                                base.addOption(ref oSelectElmt, oChoices.GetAttribute("name"), oChoices.GetAttribute("xslFile"));
                                if (string.IsNullOrEmpty(sDefaultXslt))
                                    sDefaultXslt = oChoices.GetAttribute("xslFile");
                                // Next
                            }
                            base.addNote("importXslt", Protean.xForm.noteTypes.Hint, "This defines the layout and columns of the import file. Each import file must be in the pre-agreed format. To create additional import filters contact your web developer.");

                            XmlElement argoBindParent = null;
                            base.addBind("importXslt", "file/@importXslt", oBindParent: ref argoBindParent, "true()");

                            base.addInput(ref oFrmElmt, "fld", true, "Upload Path", "readonly");
                            XmlElement argoBindParent1 = null;
                            base.addBind("fld", "file/@path", oBindParent: ref argoBindParent1, "true()");

                            string argsClass = "";
                            base.addUpload(ref oFrmElmt, "uploadFile", true, "image/*", "Upload File", sClass: ref argsClass);
                            XmlElement argoBindParent2 = null;
                            base.addBind("uploadFile", "file", oBindParent: ref argoBindParent2, "true()");

                            XmlElement oSelectElmt2;
                            oSelectElmt2 = base.addSelect1(ref oFrmElmt, "opperationMode", true, "Opperation Mode", "", Protean.xForm.ApperanceTypes.Full);
                            base.addOption(ref oSelectElmt2, "Test", "test");
                            base.addOption(ref oSelectElmt2, "Full Import", "import");
                            XmlElement argoBindParent3 = null;
                            base.addBind("opperationMode", "file/@opsMode", oBindParent: ref argoBindParent3, "true()");

                            // If myWeb.moConfig("debug") = "on" Then
                            XmlElement oSelectElmt3;
                            oSelectElmt3 = base.addSelect1(ref oFrmElmt, "contentType", true, "Response Xml", "", Protean.xForm.ApperanceTypes.Full);
                            base.addOption(ref oSelectElmt3, "on", "xml");
                            base.addOption(ref oSelectElmt3, "off", "");
                            // End If
                            XmlElement argoBindParent4 = null;
                            base.addBind("xml", "file/@xml", oBindParent: ref argoBindParent4, "false()");

                            base.addSubmit(ref oFrmElmt, "", "Upload", "ewSubmit");

                            base.Instance.InnerXml = "<file path=\"" + cPath + "\" filename=\"\" mediatype=\"\" opsMode=\"test\" importXslt=\"" + sDefaultXslt + "\" xml=\"\"/>";

                            if (base.isSubmitted())
                            {

                                base.updateInstanceFromRequest();
                                base.validate();

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
                                    oFs.initialiseVariables(Protean.fsHelper.LibraryType.Documents);
                                    oFs.mcStartFolder = goServer.MapPath("/") + cPath;

                                    sValidResponse = oFs.SaveFile(ref fUpld, "");

                                    XmlElement oElmt = (XmlElement)base.Instance.FirstChild;

                                    int lastBackslashIndex = fUpld.FileName.LastIndexOf(@"\");
                                    string cFilename;
                                    if (lastBackslashIndex > -1) { 
                                        cFilename = oFs.mcStartFolder + fUpld.FileName.Substring(lastBackslashIndex);
                                    }
                                    else {
                                        cFilename = oFs.mcStartFolder + fUpld.FileName;
                                    }
                                //cFilename = cFilename.Replace(" ", "-");
                                oElmt.SetAttribute("filename", cFilename);

                                    if ((sValidResponse ?? "") == (fUpld.FileName ?? ""))
                                    {
                                        valid = true;
                                        //XmlNode argoNode = (XmlNode)this.moXformElmt;
                                        base.addNote(ref moXformElmt, Protean.xForm.noteTypes.Alert, sValidResponse + " - File Imported");
                                        //this.moXformElmt = (XmlElement)argoNode;
                                        valid = true;
                                    }
                                    else
                                    {
                                        valid = false;
                                        //XmlNode argoNode1 = (XmlNode)this.moXformElmt;
                                        base.addNote(ref moXformElmt, Protean.xForm.noteTypes.Alert, sValidResponse);
                                        //this.moXformElmt = (XmlElement)argoNode1;
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
                        else
                        {
                            base.NewFrm("ImportFile");

                            base.submission("Import File Error", "", "post", "form_check(this)");
                            oFrmElmt = base.addGroup(ref base.moXformElmt, "Import File", "", "Error");
                            //XmlNode argoNode2 = oFrmElmt;
                            base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "There are no imports configured for this site.");
                            //oFrmElmt = (XmlElement)argoNode2;
                            return base.moXformElmt;
                        }
                    }



                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "addInput", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmStartIndex()
                {
                    XmlElement oFrmElmt;
                    string cProcessInfo = "";

                    try
                    {

                        // load the xform to be edited
                        moDbHelper.moPageXml = moPageXML;
                        var idx = new Protean.IndexerAsync(ref myWeb);
                        base.NewFrm("StartIndex");

                        base.submission("DeleteFile", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "folderItem", "", "Start Index");
                        base.Instance.InnerXml = idx.GetIndexInfo();

                        //XmlNode argoNode = oFrmElmt;
                        base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "Starting off the indexing process can take up to an hour for larger sites");
                        //oFrmElmt = (XmlElement)argoNode;

                        base.addSubmit(ref oFrmElmt, "", "Start Index", sClass: "principle pleaseWait");

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                bool bResult = true;
                                idx.DoIndex(ref bResult, 0);

                                string cSubResponse = idx.cExError;
                                if (string.IsNullOrEmpty(cSubResponse))
                                {
                                    bResult = true;
                                    cSubResponse = "Completed Successfully";
                                }
                                else
                                {
                                    bResult = false;
                                }
                                cSubResponse += Environment.NewLine + "Pages: " + idx.nPagesIndexed;
                                cSubResponse += Environment.NewLine + "Documents: " + idx.nDocumentsIndexed;
                                cSubResponse += Environment.NewLine + "Contents: " + idx.nContentsIndexed;

                                //XmlNode argoNode1 = oFrmElmt;
                                base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, cSubResponse);
                                //oFrmElmt = (XmlElement)argoNode1;
                            }

                            // fire this off in its own thread.
                            // Dim t As Thread
                            // t = New Thread(AddressOf idx.DoIndex)
                            // t.Start()

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
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmStartIndex", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }


                public XmlElement xFrmLookup(int nLookupId, string Category = "", long ParentId = 0L)
                {
                    XmlElement oFrmElmt;
                    XmlElement oGrp1Elmt;
                    string cProcessInfo = "";


                    try
                    {

                        string parentOptions = "" + myWeb.moConfig["LookupParentOptions"];

                        var oDict = new Dictionary<string, string>();

                        // Append data for particular lookup id when edit, change by nita on 18Apr22
                        string cLkpKey = "";
                        string cLkpValue = "";
                        string sSqlcheck = "";
                        DataSet lookupsSingleDataset;

                        if (nLookupId > 0)
                        {
                            sSqlcheck = "select nLkpId as id, * from tblLookup " + "WHERE nLkpId = " + nLookupId;
                            lookupsSingleDataset = myWeb.moDbHelper.GetDataSet(sSqlcheck, "Lookup", "Lookups");
                            if (lookupsSingleDataset.Tables.Count > 0)
                            {

                                cLkpKey = lookupsSingleDataset.Tables[0].Rows[0]["cLkpKey"].ToString();
                                cLkpValue = lookupsSingleDataset.Tables[0].Rows[0]["cLkpValue"].ToString();

                            }
                        }

                        if (!string.IsNullOrEmpty(parentOptions))
                        {
                            foreach (var s in parentOptions.Split(';'))
                            {
                                string[] arr = s.Split(':');
                                oDict.Add(arr[0], arr[1]);
                            }
                        }

                        base.NewFrm("EditProductGroup");
                        if (nLookupId > 0)
                        {
                            base.Instance.InnerXml = "<tblLookup><nLkpID/><cLkpKey>" + cLkpKey + "</cLkpKey><cLkpValue>" + cLkpValue + "</cLkpValue><cLkpCategory>" + Category + "</cLkpCategory><nLkpParent>" + ParentId + "</nLkpParent><nAuditId/></tblLookup>";
                        }
                        else
                        {
                            base.Instance.InnerXml = "<tblLookup><nLkpID/><cLkpKey/><cLkpValue/><cLkpCategory>" + Category + "</cLkpCategory><nLkpParent>" + ParentId + "</nLkpParent><nAuditId/></tblLookup>";
                        }

                        if (nLookupId > 0)
                        {
                            // MyBase.Instance.InnerXml = moDbHelper.getObjectInstance(dbHelper.objectTypes.Lookup, nLookupId)
                            Category = base.Instance.SelectSingleNode("tblLookup/cLkpCategory").InnerText;
                        }
                        base.submission("EditLookup", "", "post", "form_check(this)");

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "Lookup");
                        base.addNote("pgheader", Protean.xForm.noteTypes.Help, (nLookupId > 0 ? "Edit " : "Add ") + "Lookup");
                        oGrp1Elmt = base.addGroup(ref oFrmElmt, "Lookup", "1col", "Details");

                        // Definitions
                        base.addInput(ref oGrp1Elmt, "nLkpID", true, "nLkpID", "hidden");
                        XmlElement argoBindParent = null;
                        base.addBind("nLkpID", "tblLookup/nLkpID", oBindParent: ref argoBindParent);

                        if (!string.IsNullOrEmpty(parentOptions))
                        {
                            if (oDict.ContainsKey(Category))
                            {
                                var SelectElmt = base.addSelect1(ref oGrp1Elmt, "nLkpParent", true, oDict[Category], ((int)Protean.xForm.ApperanceTypes.Minimal).ToString());
                                XmlElement argoBindParent1 = null;
                                base.addBind("nLkpParent", "tblLookup/nLkpParent", oBindParent: ref argoBindParent1);
                                string sSql = "select nLkpId as value, cLkpKey as name from tblLookup where cLkpCategory like '" + oDict[Category] + "'";
                                using (var oDr = moDbHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                                {
                                    base.addOptionsFromSqlDataReader( SelectElmt, oDr);
                                }
                            }
                        }

                        base.addInput(ref oGrp1Elmt, "cLkpKey", true, "Name");
                        XmlElement argoBindParent2 = null;
                        base.addBind("cLkpKey", "tblLookup/cLkpKey", oBindParent: ref argoBindParent2, "true()");

                        base.addInput(ref oGrp1Elmt, "cLkpValue", true, "Value");
                        XmlElement argoBindParent3 = null;
                        base.addBind("cLkpValue", "tblLookup/cLkpValue", oBindParent: ref argoBindParent3, "true()");

                        base.addInput(ref oGrp1Elmt, "cLkpCategory", true, "Category", "readonly");
                        XmlElement argoBindParent4 = null;
                        base.addBind("cLkpCategory", "tblLookup/cLkpCategory", oBindParent: ref argoBindParent4, "true()");

                        base.addInput(ref oGrp1Elmt, "nAuditId", true, "nAuditId", "hidden");
                        XmlElement argoBindParent5 = null;
                        base.addBind("nAuditId", "tblLookup/nAuditId", oBindParent: ref argoBindParent5);

                        // search button

                        base.addSubmit(ref oFrmElmt, "EditLookup", "Save Lookup", "SaveLookup");


                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.addValues();
                            base.validate();
                            if (base.valid)
                            {
                                moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Lookup, base.Instance, nLookupId > 0 ? (long)nLookupId : -1L);
                            }
                        }
                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmLookup", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }


                // method for indexes to create add new indexes UI
                public XmlElement xFrmIndexes(int indexId, string SchemaName = "", string ParentId = "")
                {
                    XmlElement oFrmElmt;
                    XmlElement oGrp1Elmt;
                    string cProcessInfo = "";
                    XmlElement oSelElmt;

                    try
                    {


                        var oDict = new Dictionary<string, string>();
                        // Dim oDr As SqlDataReader

                        // Append data for particular lookup id when edit, change by nita on 18Apr22
                        string nContentIndexDataType = "";
                        string cContentSchemaName = "";
                        string cDefinitionName = "";
                        string cContentValueXpath = "";
                        string bBriefNotDetail = "";
                        string bProductRefForSKU = "";
                        string cDefaultValue = string.Empty;

                        string sSqlcheck = "";
                        DataSet lookupsSingleDataset;

                        if (indexId > 0)
                        {
                            sSqlcheck = "select nContentIndexDefKey as id,nContentIndexDataType,RTRIM(LTRIM(cContentSchemaName)) AS cContentSchemaName, * from tblContentIndexDef " + "WHERE nContentIndexDefKey = " + indexId;
                            lookupsSingleDataset = myWeb.moDbHelper.GetDataSet(sSqlcheck, "indexkey", "indexkeys");
                            if (lookupsSingleDataset.Tables.Count > 0)
                            {

                                nContentIndexDataType = lookupsSingleDataset.Tables[0].Rows[0]["nContentIndexDataType"].ToString();
                                cContentSchemaName = lookupsSingleDataset.Tables[0].Rows[0]["cContentSchemaName"].ToString();
                                cDefinitionName = lookupsSingleDataset.Tables[0].Rows[0]["cDefinitionName"].ToString();
                                cContentValueXpath = lookupsSingleDataset.Tables[0].Rows[0]["cContentValueXpath"].ToString();
                                bBriefNotDetail = lookupsSingleDataset.Tables[0].Rows[0]["bBriefNotDetail"].ToString();
                                bProductRefForSKU = lookupsSingleDataset.Tables[0].Rows[0]["bProductRefForSKU"].ToString();
                                cDefaultValue = lookupsSingleDataset.Tables[0].Rows[0]["cDefaultValue"].ToString();

                            }
                        }


                        base.NewFrm("EditProductGroup");
                        if (indexId > 0)
                        {
                            base.Instance.InnerXml = "<tblContentIndexDef><nContentIndexDefKey/><nContentIndexDataType>" + nContentIndexDataType + "</nContentIndexDataType><cContentSchemaName>" + cContentSchemaName.Trim() + "</cContentSchemaName><cDefinitionName>" + cDefinitionName.Trim() + "</cDefinitionName><cContentValueXpath>" + cContentValueXpath.Trim() + "</cContentValueXpath><bBriefNotDetail>" + bBriefNotDetail + "</bBriefNotDetail><nKeywordGroupName/><nAuditId/><bProductRefForSKU>" + bProductRefForSKU + "</bProductRefForSKU><cDefaultValue>" + cDefaultValue + "</cDefaultValue></tblContentIndexDef>";
                        }
                        else
                        {
                            base.Instance.InnerXml = "<tblContentIndexDef><nContentIndexDefKey/><nContentIndexDataType/><cContentSchemaName/><cDefinitionName/><cContentValueXpath/><bBriefNotDetail>" + "0" + "</bBriefNotDetail><nKeywordGroupName/><nAuditId/><bProductRefForSKU>" + "0" + "</bProductRefForSKU><cDefaultValue></cDefaultValue> </tblContentIndexDef>";
                        }

                        if (indexId > 0)
                        {
                            // MyBase.Instance.InnerXml = moDbHelper.getObjectInstance(dbHelper.objectTypes.Lookup, nLookupId)
                            SchemaName = base.Instance.SelectSingleNode("tblContentIndexDef/cContentSchemaName").InnerText;
                        }
                        base.submission("EditIndexes", "", "post", "form_check(this)");

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "indexkey");
                        base.addNote("pgheader", Protean.xForm.noteTypes.Help, (indexId > 0 ? "Edit " : "Add ") + "indexkey");
                        oGrp1Elmt = base.addGroup(ref oFrmElmt, "indexkey", "1col", "Details");

                        // Definitions
                        base.addInput(ref oGrp1Elmt, "nContentIndexDefKey", true, "nContentIndexDefKey", "hidden");
                        XmlElement argoBindParent = null;
                        base.addBind("nContentIndexDefKey", "tblContentIndexDef/nContentIndexDefKey", oBindParent: ref argoBindParent);

                        oSelElmt = base.addSelect1(ref oGrp1Elmt, "nContentIndexDataType", true, "Data Type", ((int)Protean.xForm.ApperanceTypes.Minimal).ToString());
                        base.addOption(ref oSelElmt, "Int", "1");
                        base.addOption(ref oSelElmt, "String", "2");
                        base.addOption(ref oSelElmt, "Date", "3");
                        XmlElement argoBindParent1 = null;
                        base.addBind("nContentIndexDataType", "tblContentIndexDef/nContentIndexDataType", oBindParent: ref argoBindParent1, "true()");

                        string sSql = "select distinct cContentSchemaName from tblContent";
                        using (var oDr = moDbHelper.getDataReaderDisposable(sSql))
                        {
                            // Adding controls to the form like dropdown, radiobuttons
                            oSelElmt = base.addSelect1(ref oGrp1Elmt, "cContentSchemaName", true, "Schema Name", ((int)Protean.xForm.ApperanceTypes.Minimal).ToString());
                            base.addOptionsFromSqlDataReader( oSelElmt, oDr, "cContentSchemaName", "cContentSchemaName");
                        }
                        XmlElement argoBindParent2 = null;
                        base.addBind("cContentSchemaName", "tblContentIndexDef/cContentSchemaName", oBindParent: ref argoBindParent2, "true()");

                        base.addInput(ref oGrp1Elmt, "cDefinitionName", true, "Index Rule");
                        XmlElement argoBindParent3 = null;
                        base.addBind("cDefinitionName", "tblContentIndexDef/cDefinitionName", oBindParent: ref argoBindParent3, "true()");

                        base.addInput(ref oGrp1Elmt, "cContentValueXpath", true, "XPath");
                        XmlElement argoBindParent4 = null;
                        base.addBind("cContentValueXpath", "tblContentIndexDef/cContentValueXpath", oBindParent: ref argoBindParent4, "true()");

                        oSelElmt = base.addSelect1(ref oGrp1Elmt, "bProductRefForSKU", true, "Parent Ref", ((int)Protean.xForm.ApperanceTypes.Minimal).ToString());
                        base.addOption(ref oSelElmt, "Yes", "1");
                        base.addOption(ref oSelElmt, "No", "0");
                        XmlElement argoBindParent5 = null;
                        base.addBind("bProductRefForSKU", "tblContentIndexDef/bProductRefForSKU", oBindParent: ref argoBindParent5, "false()");




                        base.addInput(ref oGrp1Elmt, "nKeywordGroupName", true, "nKeywordGroupName", "hidden");
                        XmlElement argoBindParent6 = null;
                        base.addBind("nKeywordGroupName", "tblContentIndexDef/nKeywordGroupName", oBindParent: ref argoBindParent6);

                        base.addInput(ref oGrp1Elmt, "nAuditId", true, "nAuditId", "hidden");
                        XmlElement argoBindParent7 = null;
                        base.addBind("nAuditId", "tblContentIndexDef/nAuditId", oBindParent: ref argoBindParent7);

                        base.addInput(ref oGrp1Elmt, "cDefaultValue", true, "Default Value");
                        XmlElement argoBindParent8 = null;
                        base.addBind("cDefaultValue", "tblContentIndexDef/cDefaultValue", oBindParent: ref argoBindParent8, "false()");

                        // search button

                        base.addSubmit(ref oFrmElmt, "EditIndexes", "Save Indexes", "SaveIndexes");


                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.addValues();
                            base.validate();
                            if (base.valid)
                            {
                                moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.indexkey, base.Instance, indexId > 0 ? (long)indexId : -1L);
                            }
                        }
                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmIndexes", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }


                public XmlElement xFrmEditTemplate()
                {
                    string cProcessInfo = "";
                    string xslFilename = "";
                    XmlElement oFrmElmt;
                    try
                    {
                        base.NewFrm("EditTemplate");
                        switch (myWeb.moRequest["ewCmd2"] ?? "")
                        {
                            case "RenewalAlerts":
                                {
                                    xslFilename = "/xsl/email/subscriptionReminder.xsl";
                                    break;
                                }
                        }

                        base.Instance.InnerXml = "<Template name=\"\"><TemplateContent/></Template>";
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "SelectTemplate");

                        var XslDocument = new XmlDocument();
                        XslDocument.Load(goServer.MapPath(xslFilename));
                        base.submission("EditTemplate", "", "post", "form_check(this)");

                        XmlNamespaceManager xmlnsManager = new XmlNamespaceManager(XslDocument.NameTable);
                        xmlnsManager.AddNamespace("xsl", "http://www.w3.org/1999/XSL/Transform");

                        var SelectElmt = base.addSelect1(ref oFrmElmt, "Template", true, "Template", ((int)Protean.xForm.ApperanceTypes.Minimal).ToString());
                        short i = 1;
                        foreach (XmlElement oTmpt in XslDocument.DocumentElement.SelectNodes("xsl:template", (XmlNamespaceManager)xmlnsManager))
                        {
                            addOption(ref SelectElmt, oTmpt.GetAttribute("mode") + " - " + oTmpt.GetAttribute("match"), i.ToString());

                            if (Convert.ToInt16(myWeb.moRequest["Template"]) == (int)i)
                            {

                                addInput(ref oFrmElmt, "tplt-mode", true, "Mode");
                                XmlElement argoBindParent = null;
                                base.addBind("tplt-mode", "Template/TemplateContent/*/@mode", oBindParent: ref argoBindParent, "true()");
                                addInput(ref oFrmElmt, "tplt-match", true, "Match");
                                XmlElement argoBindParent1 = null;
                                base.addBind("tplt-match", "Template/TemplateContent/*/@match", oBindParent: ref argoBindParent1, "true()");

                                string argsClass = "xsl";
                                int argnRows = 0;
                                int argnCols = 0;
                                base.addTextArea(ref oFrmElmt, "TemplateContent", true, "Template Content", ref argsClass, nRows: ref argnRows, nCols: ref argnCols);
                                XmlElement argoBindParent2 = null;
                                base.addBind("TemplateContent", "Template/TemplateContent", oBindParent: ref argoBindParent2, "true()");
                                XmlElement oElmt = (XmlElement)base.Instance.SelectSingleNode("Template/TemplateContent");
                                oElmt.InnerXml = oTmpt.OuterXml;

                            }
                            i = (short)(i + 1);
                        }

                        var xmlElmt = base.addSelect1(ref oFrmElmt, "contentType", true, "contentType", ((int)Protean.xForm.ApperanceTypes.Minimal).ToString());
                        addOption(ref xmlElmt, "xml", "xml");

                        if (Convert.ToInt16(myWeb.moRequest["Template"]) > 0)
                        {
                            base.addSubmit(ref oFrmElmt, "EditTemplate", "Save Template", "SaveTemplate");
                        }
                        else
                        {
                            base.addSubmit(ref oFrmElmt, "EditTemplate", "Edit Template", "EditTemplate");
                        }

                        base.addValues();

                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmLookup", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmAlertEmail(string recordType, int nUserId, string xFormPath)
                {
                    string cProcessInfo = "";
                    object FormTitle = "AlertEmail User";
                    object InstanceSessionName = "tempInstance_alert" + nUserId.ToString();
                    try
                    {
                        myWeb.moSession[InstanceSessionName.ToString()] = (object)null;
                        base.NewFrm(Convert.ToString(FormTitle));
                        base.bProcessRepeats = false;

                        // We load the xform from a file, it may be in local or in common folders.
                        base.load(xFormPath, myWeb.maCommonFolders);

                        // We get the instance
                        if (nUserId > 0)
                        {
                            string sNewGroupNames = string.Empty;

                            base.bProcessRepeats = true;
                            if (myWeb.moSession[InstanceSessionName.ToString()] is null)
                            {
                                var existingInstance = base.moXformElmt.OwnerDocument.CreateElement("instance");
                                XmlElement AlertEmail = (XmlElement)existingInstance.AppendChild(base.moXformElmt.OwnerDocument.CreateElement("AlertEmail"));
                                // AlertEmail.SetAttribute("existingGroupId", "")
                                // AlertEmail.SetAttribute("existingGroupName", myWeb.moDbHelper.getNameByKey(dbHelper.objectTypes.Directory, existingGroupId))
                                AlertEmail.SetAttribute("sendEmail", "1");

                                base.Instance.SelectSingleNode("AlertEmail/Email").InnerText = myWeb.moRequest["Email"];
                                base.Instance.SelectSingleNode("AlertEmail/RecordType").InnerText = myWeb.moRequest["RecordType"];
                                base.Instance.SelectSingleNode("AlertEmail/id").InnerText = myWeb.moRequest["id"];
                                base.Instance.SelectSingleNode("AlertEmail/xFormName").InnerText = myWeb.moRequest["xFormName"];
                                base.Instance.SelectSingleNode("AlertEmail/RecipientName").InnerText = myWeb.moRequest["RecipientName"];
                                AlertEmail.AppendChild(base.Instance.SelectSingleNode("AlertEmail/Email"));
                                AlertEmail.AppendChild(base.Instance.SelectSingleNode("AlertEmail/RecordType"));
                                AlertEmail.AppendChild(base.Instance.SelectSingleNode("AlertEmail/id"));
                                AlertEmail.AppendChild(base.Instance.SelectSingleNode("AlertEmail/xFormName"));
                                AlertEmail.AppendChild(base.Instance.SelectSingleNode("AlertEmail/RecipientName"));

                                AlertEmail.AppendChild(base.Instance.SelectSingleNode("AlertEmail/emailer"));
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
                            // MyBase.updateInstanceFromRequest()
                            base.validate();
                            if (base.valid)
                            {
                                NameValueCollection moMailConfig = (NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/mailinglist");

                                // Send Email
                                var oMsg = new Protean.Messaging();
                                Cms.dbHelper argodbHelper = null;
                                oMsg.emailer((XmlElement)base.Instance.SelectSingleNode("AlertEmail"), base.Instance.SelectSingleNode("AlertEmail/emailer/xsltPath").InnerText, base.Instance.SelectSingleNode("AlertEmail/emailer/fromName").InnerText, moMailConfig["FromEmail"], base.Instance.SelectSingleNode("AlertEmail/Email").InnerText, base.Instance.SelectSingleNode("AlertEmail/emailer/SubjectLine").InnerText, odbHelper: ref argodbHelper);
                                // myWeb.msRedirectOnEnd = myWeb.moSession("lastPage")

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

                public XmlElement xFrmAlertEmail(string messageType, XmlElement PayloadData, string xFormPath, string subject, string senderName, string senderEmail, string recipientName, string recipientEmail, string ccName, string ccEmail, string bccName, string bccEmail, string emailContentXsltPath, Boolean autosend, long subjectId = 0)
                {
                    string cProcessInfo = "";
                    object FormTitle = "AlertEmail User";
                    try
                    {

                        base.NewFrm(Convert.ToString(FormTitle));
                        base.bProcessRepeats = false;

                        // We load the xform from a file, it may be in local or in common folders.
                        base.load(xFormPath, myWeb.maCommonFolders);

                        string sNewGroupNames = string.Empty;

                        base.bProcessRepeats = true;

                        XmlElement payloadXml = base.moXformElmt.OwnerDocument.CreateElement("AlertData");
                        payloadXml.InnerXml = PayloadData.OuterXml;

                        base.Instance.SetAttribute("messageType", messageType);
                        base.Instance.SetAttribute("subjectId", subjectId.ToString());

                        base.Instance.SelectSingleNode("emailer/recipientEmail").InnerText = recipientEmail;
                        base.Instance.SelectSingleNode("emailer/recipientName").InnerText = recipientName;

                        base.Instance.SelectSingleNode("emailer/oBodyXML/Items/Message").InnerText = messageType;
                        base.Instance.SelectSingleNode("emailer/fromName").InnerText = senderName;
                        base.Instance.SelectSingleNode("emailer/fromEmail").InnerText = senderEmail;
                        base.Instance.SelectSingleNode("emailer/ccRecipientName").InnerText = ccName;
                        base.Instance.SelectSingleNode("emailer/ccRecipient").InnerText = ccEmail;
                        base.Instance.SelectSingleNode("emailer/bccRecipientName").InnerText = bccName;
                        base.Instance.SelectSingleNode("emailer/bccRecipient").InnerText = bccEmail;
                        base.Instance.AppendChild(payloadXml.FirstChild);

                        // Process the XSLT for the email content
                        XmlDocument emailContent = TransformEmailContent(myWeb.goServer.MapPath(emailContentXsltPath), base.Instance);

                        // Insert the transformed content into the XML
                        base.Instance.SelectSingleNode("emailer/oBodyXML/Items/Message").InnerXml = emailContent.DocumentElement.InnerXml.Replace(" xmlns=\"http://www.w3.org/1999/xhtml\"", "");

                        if (base.Instance.SelectSingleNode("emailer/oBodyXML/Items/Message/div/@subject") != null)
                        {
                            subject = base.Instance.SelectSingleNode("emailer/oBodyXML/Items/Message/div/@subject").InnerText;
                        }
                        base.Instance.SelectSingleNode("emailer/SubjectLine").InnerText = subject;

                        if (base.isSubmitted() || autosend)
                        {
                            if (!autosend) {
                                //this was commented out so does not update this breaks why was this done?
                                //If autosend there is not request to update from
                                base.updateInstanceFromRequest();
                            }
                                                  base.validate();
                            if (base.valid)
                            {
                                NameValueCollection moMailConfig = (NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/mailinglist");

                                // Send Email
                                var oMsg = new Protean.Messaging();
                                string xsltPath = base.Instance.SelectSingleNode("emailer/xsltPath").InnerText;
                                string fromName = base.Instance.SelectSingleNode("emailer/fromName").InnerText;
                                string fromEmail = base.Instance.SelectSingleNode("emailer/fromEmail").InnerText;
                                string email = base.Instance.SelectSingleNode("emailer/recipientEmail").InnerText;
                                recipientName = base.Instance.SelectSingleNode("emailer/recipientEmail").InnerText;
                                string subjectLine = base.Instance.SelectSingleNode("emailer/SubjectLine").InnerText;
                                string ccName1 = base.Instance.SelectSingleNode("emailer/ccRecipientName").InnerText;
                                string ccEmail1 = base.Instance.SelectSingleNode("emailer/ccRecipient").InnerText;
                                string bccEmail1 = base.Instance.SelectSingleNode("emailer/bccRecipient").InnerText;
                                XmlElement BodyElmt = (XmlElement)base.Instance.SelectSingleNode("emailer/oBodyXML");

                                BodyElmt.SetAttribute("messageType", messageType);
                                BodyElmt.SetAttribute("subjectId", PayloadData.GetAttribute("id"));
                                BodyElmt.SetAttribute("subjectLine", subjectLine);

                                Cms.dbHelper argodbHelper = null;

                                object mailResponse = oMsg.emailer(BodyElmt, xsltPath, fromName, fromEmail, email, subjectLine, odbHelper: ref argodbHelper, "Message Sent", "Message Failed", recipientName, ccEmail1, bccEmail1);
                                string sResponse = mailResponse.ToString();
                                XmlNode grpNode = moXformElmt.SelectSingleNode("descendant-or-self::group[1]");
                                addNote(ref grpNode, noteTypes.Alert, sResponse, true, "alert-success");



                                if (myWeb.moSession["lastPage"] != null)
                                {
                                    if (sResponse == "Message Sent") { 
                                        myWeb.msRedirectOnEnd = myWeb.moSession["lastPage"].ToString();
                                    }
                                }
                            }
                        }

                        // we populate the values onto the form.
                        base.addValues();

                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmAlertEmail", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                private XmlDocument TransformEmailContent(string styleFile, XmlElement instance)
                {
                    TextWriter sWriter = new StringWriter();
                    Protean.XmlHelper.Transform oTransform = new Protean.XmlHelper.Transform();
                    XmlDocument xContent = new XmlDocument();
                    try
                    {

                        oTransform.XSLFile = styleFile;
                        oTransform.Compiled = false;
                        XmlDocument ourDoc = new XmlDocument();
                        ourDoc.LoadXml(instance.OuterXml);
                        sWriter = new StringWriter();
                        oTransform.Process(ourDoc, ref sWriter);


                        xContent.LoadXml(sWriter.ToString());
                        return xContent;
                    }
                    catch (Exception ex)
                    {
                        if (oTransform.transformException != null)
                        {
                            xContent.LoadXml("<body><h1>" + oTransform.transformException.Message + "</h1><p>" + oTransform.transformException.StackTrace + "</p></body>");
                            return xContent;
                        }
                        else
                        {
                            stdTools.returnException(ref myWeb.msException, _moduleName, "TransformEmailContent", ex, "", "", gbDebug);
                            return null;
                        }
                    }
                    finally
                    {
                        sWriter = null;
                        oTransform = null;

                    }

                }

                public XmlElement xFrmRequestSettlement(int nOrderId, bool bForceSend = false)
                {
                    string cProcessInfo = "";
                    object InstanceSessionName = "tempInstance_requestSettlement" + nOrderId.ToString();
                    try
                    {


                        myWeb.moSession[InstanceSessionName.ToString()] = (object)null;
                        base.NewFrm("Request Settlement");
                        base.bProcessRepeats = false;

                        // We load the xform from a file, it may be in local or in common folders.
                        base.load("/xforms/cart/requestSettlement.xml", myWeb.maCommonFolders);

                        // We get the instance
                        if (nOrderId > 0)
                        {

                            base.bProcessRepeats = true;
                            if (myWeb.moSession[InstanceSessionName.ToString()] is null)
                            {
                                var existingInstance = base.moXformElmt.OwnerDocument.CreateElement("instance");
                                Cms.Cart oCart;
                                oCart = new Cms.Cart(ref myWeb);
                                // Get Cart Xml
                                var oCartListElmt = moPageXML.CreateElement("Order");
                                oCart.GetCart(ref oCartListElmt, nOrderId);
                                existingInstance.InnerXml = oCartListElmt.OuterXml;

                                base.Instance.SelectNodes("emailer");

                                var emailerNode = base.Instance.SelectSingleNode("emailer");

                                XmlElement msgNode = (XmlElement)emailerNode.SelectSingleNode("oBodyXML/Items/Message");

                                string msgHtml = msgNode.InnerXml;


                                NameValueCollection moCartConfig = (NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/cart");


                                DateTime SettlementDate = Convert.ToDateTime(oCartListElmt.SelectSingleNode("Item[1]/productDetail/StartDate").InnerText);
                                int settlementDays = -(int)Math.Round(Convert.ToDouble(moCartConfig["SettlementDays"]));
                                SettlementDate = SettlementDate.AddDays(settlementDays);

                                msgHtml = msgHtml.Replace("{Name}", oCartListElmt.SelectSingleNode("Contact[@type='Billing Address']/GivenName").InnerText);
                                msgHtml = msgHtml.Replace("{SettlementId}", oCartListElmt.GetAttribute("settlementID"));
                                msgHtml = msgHtml.Replace("{PaymentDue}", oCartListElmt.GetAttribute("payableAmount"));
                                msgHtml = msgHtml.Replace("{PaymentDueDate}", SettlementDate.ToString("dd MMM yyyy"));
                                msgHtml = msgHtml.Replace("{CourseName}", oCartListElmt.SelectSingleNode("Item[1]/Name").InnerText);

                                msgNode.InnerXml = msgHtml;

                                existingInstance.InsertBefore(emailerNode.CloneNode(true), existingInstance.FirstChild);

                                base.LoadInstance(existingInstance);
                                myWeb.moSession[InstanceSessionName.ToString()] = base.Instance;
                            }

                            else
                            {
                                base.LoadInstance(myWeb.moSession["tempInstance"].ToString());
                            }
                        }

                        moXformElmt.SelectSingleNode("descendant-or-self::instance").InnerXml = base.Instance.InnerXml;

                        if (base.isSubmitted() | bForceSend)
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                string Name = base.Instance.SelectSingleNode("Order/Contact[@type='Billing Address']/GivenName").InnerText;
                                string EmailTo = base.Instance.SelectSingleNode("Order/Contact[@type='Billing Address']/Email").InnerText;
                                // Send Email
                                var oMsg = new Protean.Messaging();
                                Cms.dbHelper argodbHelper = null;
                                oMsg.emailer((XmlElement)base.Instance.SelectSingleNode("emailer/oBodyXML"), base.Instance.SelectSingleNode("emailer/xsltPath").InnerText, base.Instance.SelectSingleNode("emailer/fromName").InnerText, base.Instance.SelectSingleNode("emailer/fromEmail").InnerText, EmailTo, base.Instance.SelectSingleNode("emailer/SubjectLine").InnerText, odbHelper: ref argodbHelper);
                                myWeb.moSession[InstanceSessionName.ToString()] = (object)null;
                                myWeb.moDbHelper.logActivity(Cms.dbHelper.ActivityType.Email, mnUserId, 0L, 0L, (long)nOrderId, "Payment Reminder Sent - " + DateTime.Now.ToString());

                                var oFrmElmt = base.moXformElmt;
                                //XmlNode argoNode = (XmlNode)oFrmElmt;
                                base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "Message Sent.");
                                //oFrmElmt = (XmlElement)argoNode;

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

                #region IDisposable Implementation

                private bool disposedValue = false; // To detect redundant calls

                protected virtual void Dispose(bool disposing)
                {
                    if (!disposedValue)
                    {
                        if (disposing)
                        {
                            try
                            {
                                // ====================
                                // 1. UNSUBSCRIBE EVENT HANDLERS
                                // ====================
                                if (OnError != null)
                                {
                                    foreach (var handler in OnError.GetInvocationList())
                                    {
                                        OnError -= (OnErrorEventHandler)handler;
                                    }
                                }

                                // ====================
                                // 2. DISPOSE MANAGED RESOURCES
                                // ====================

                                // Impersonation object
                                if (moImp != null)
                                {
                                    try
                                    {
                                        moImp.UndoImpersonation();
                                        moImp = null;
                                    }
                                    catch (Exception ex)
                                    {
                                        System.Diagnostics.Debug.WriteLine(
                                            $"Error disposing moImp: {ex.Message}");
                                    }
                                }

                                // ====================
                                // 3. CLEAR COLLECTIONS
                                // ====================
                                if (sImageUrlslist != null)
                                {
                                    sImageUrlslist.Clear();
                                    sImageUrlslist = null;
                                }

                                // ====================
                                // 4. NULL OUT LARGE OBJECTS
                                // ====================

                                // XML Documents (inherited from xForm)
                                moPageXML = null;
                                moXformElmt = null;
                                model = null;
                                result = null;

                                // ====================
                                // 5. CLEAR REFERENCES (NOT OWNED - DO NOT DISPOSE)
                                // ====================

                                // Parent references - owned by parent Cms object
                                myWeb = null;
                                moDbHelper = null;
                                goConfig = null;
                                moRequest = null;

                                // Context references (owned by parent)
                                moCtx = null;
                                goApp = null;
                                goRequest = null;
                                goResponse = null;
                                goSession = null;
                                goServer = null;
                            }
                            catch (Exception ex)
                            {
                                // Log disposal errors but don't throw
                                System.Diagnostics.Debug.WriteLine(
                                    $"Error in AdminXforms.Dispose: {ex.Message}");
                            }
                        }

                        // Free unmanaged resources (if any)

                        disposedValue = true;
                    }
                }

                // Finalizer
                ~AdminXforms()
                {
                    Dispose(false);
                }

                // Public Dispose method
                public void Dispose()
                {
                    Dispose(true);
                    GC.SuppressFinalize(this);
                }

                // Helper method to prevent use after disposal
                protected void ThrowIfDisposed()
                {
                    if (disposedValue)
                    {
                        throw new ObjectDisposedException(GetType().Name);
                    }
                }

                #endregion

            }
        }
    }
}