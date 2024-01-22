// ***********************************************************************
// $Library:     eonic.admin
// $Revision:    3.1  
// $Date:        2006-03-02
// $Author:      Trevor Spink (trevor@eonic.co.uk)
// &Website:     www.eonic.co.uk
// &Licence:     All Rights Reserved.
// $Copyright:   Copyright (c) 2002 - 2006 Eonic Ltd.
// ***********************************************************************

using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Xml;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using static Protean.stdTools;
using Protean.Tools;
using static Protean.Tools.Xml;
using System.Reflection.Emit;
using Protean.Providers.Membership;
using Protean.Providers.Messaging;

namespace Protean
{

    public partial class Cms
    {
        public partial class Admin
        {

            #region Declarations


            public XmlDocument moPageXML = new XmlDocument();

            public static string mcModuleName = "Protean.Admin";
            public string mcEwCmd;
            public string mcEwCmd2;
            public string mcEwCmd3;

            public Admin.AdminXforms moAdXfm;
            // Public moXformEditor As XFormEditor

            private string mcPagePath;
            public System.Web.HttpServerUtility goServer;
            // preview mode info
            public bool mbPreviewMode; // Is Preview mode on?
            public Cms myWeb;
            public System.Collections.Specialized.NameValueCollection moConfig;
            public int nAdditionId;
            public XmlElement moDeniedAdminMenuElmt;

            private int mnAdminTopLevel;
            private string lEditContext;
            private bool bClearEditContext = true;

            public int mnAdminUserId;
            public string adminLayout = "";


            #endregion
            public Admin()
            {

            }


            public Admin(ref Cms aWeb)
            {
                myWeb = aWeb;
                moConfig = myWeb.moConfig;
                moAdXfm = (Admin.AdminXforms)myWeb.getAdminXform();
                moPageXML = myWeb.moPageXml;
                // moXformEditor = myWeb.GetXformEditor()

                if (myWeb.moSession != null)
                {
                    if (!string.IsNullOrEmpty(Conversions.ToString(Operators.ConcatenateObject(myWeb.moSession["PreviewUser"], ""))))
                    {
                        mnAdminUserId = Conversions.ToInteger(myWeb.moSession["nUserId"]);
                    }
                    else
                    {
                        mnAdminUserId = myWeb.mnUserId;
                    }
                }
                else
                {
                    mnAdminUserId = myWeb.mnUserId;
                }

                moAdXfm.open(moPageXML);
                if (myWeb.moSession != null)
                {
                    if (myWeb.moSession["EditContext"] != null)
                    {
                        EditContext = Conversions.ToString(myWeb.moSession["EditContext"]);
                        clearEditContext = true;
                    }
                }

                if (string.IsNullOrEmpty(moConfig["AdminRootPageId"]))
                {
                    mnAdminTopLevel = Conversions.ToInteger(moConfig["RootPageId"]);
                }
                else
                {
                    mnAdminTopLevel = Conversions.ToInteger(moConfig["AdminRootPageId"]);
                }

            }


            public virtual void open(XmlDocument oPageXml)
            {
                string cProcessInfo = "";
                try
                {

                    if (string.IsNullOrEmpty(mcPagePath))
                    {
                        mcPagePath = "?pgid=" + myWeb.mnPageId + "&";
                    }
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "open", ex, "", cProcessInfo, gbDebug);
                }
            }

            public void close()
            {
                string cProcessInfo = "";
                try
                {

                    moAdXfm = (Admin.AdminXforms)null;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "PersistVariables", ex, "", cProcessInfo, gbDebug);
                }
            }

            public string Command
            {
                get
                {
                    return Conversions.ToString(Interaction.IIf(mcEwCmd is null || string.IsNullOrEmpty(mcEwCmd), "", mcEwCmd));
                }
            }

            public string EditContext
            {
                // edit context is to define where we are in Content or Mailing List when we are editing content.
                // stored in the session on both the normal and advanced modes which must be visited prior to editing content.
                // principly we use this to define which to adminmenu to show and box styles which might differ between site and emails.
                get
                {
                    return lEditContext;
                }
                set
                {
                    lEditContext = value;
                    if (string.IsNullOrEmpty(value))
                    {
                        clearEditContext = true;
                    }
                    else
                    {
                        clearEditContext = false;
                    }
                }
            }


            public bool clearEditContext
            {
                // edit context is to define where we are in Content or Mailing List when we are editing content.
                // stored in the session on both the normal and advanced modes which must be visited prior to editing content.
                // principly we use this to define which to adminmenu to show and box styles which might differ between site and emails.
                get
                {
                    return bClearEditContext;
                }
                set
                {
                    bClearEditContext = value;
                }
            }

            public virtual void adminProcess(ref Cms oWeb)
            {
                string sAdminLayout;
                string sProcessInfo = "";
                var oPageDetail = moPageXML.CreateElement("ContentDetail");
                bool bAdminMode = true;
                string sPageTitle = "ProteanCMS: ";
                bool bLoadStructure = false;
                long nParId = 0L;
                bool bResetParId = false;
                // get adminxforms ready 
                bool bMailMenu = false;
                bool bSystemPagesMenu = false;
                long nContentId = 0L;
                bool clearEditContext = true;

                moAdXfm.mbAdminMode = myWeb.mbAdminMode;

                try
                {

                    oWeb.mbAdminMode = true;
                    moPageXML = oWeb.moPageXml;

                    // If myWeb.moSession("previewMode") = "true" Then
                    // mbPreviewMode = True
                    // If IsNumeric(myWeb.moSession("previewUser")) Then oWeb.mnAdminUserId = CInt(myWeb.moSession("previewUser"))
                    // If IsDate(myWeb.moSession("previewDate")) Then oWeb.mdDate = CDate(myWeb.moSession("previewDate"))
                    // Else
                    // mbPreviewMode = False
                    // End If

                    string[] EwCmd = Strings.Split(myWeb.moRequest["ewCmd"], ".");
                    mcEwCmd = EwCmd[0];
                    if (Information.UBound(EwCmd) > 0)
                        mcEwCmd2 = EwCmd[1];
                    if (Information.UBound(EwCmd) > 1)
                        mcEwCmd3 = EwCmd[2];

                    if (!string.IsNullOrEmpty(myWeb.moRequest["ewCmd2"]))
                    {
                        mcEwCmd2 = myWeb.moRequest["ewCmd2"];
                    }



                    if (!string.IsNullOrEmpty(moConfig["SecureMembershipAddress"]))
                    {
                        var oMembership = new Cms.Membership(ref myWeb);
                        oMembership.OnError += myWeb.OnComponentError;
                        oMembership.SecureMembershipProcess(mcEwCmd);
                    }


                    if (Conversions.ToBoolean(Operators.AndObject(Operators.ConditionalCompareObjectEqual(myWeb.moSession["ewCmd"], "PreviewOn", false), Strings.LCase(myWeb.moRequest["ewCmd"]) != "normal" & Strings.LCase(myWeb.moRequest["ewCmd"]) != "editcontent" & Strings.LCase(myWeb.moRequest["ewCmd"]) != "publishcontent")))
                    {
                        // case to cater for logoff in preview mode
                        mcEwCmd = "PreviewOn";
                        myWeb.mbPreview = true;
                    }
                    else if (string.IsNullOrEmpty(mcEwCmd))
                    {
                        mcEwCmd = Conversions.ToString(myWeb.moSession["ewCmd"]);
                    }
                    else if (Conversions.ToBoolean(Operators.AndObject(Operators.ConditionalCompareObjectEqual(myWeb.moSession["ewCmd"], "PreviewOn", false), Strings.LCase(mcEwCmd) == "normal" | Strings.LCase(mcEwCmd) == "editcontent" | Strings.LCase(mcEwCmd) == "publishcontent")))
                    {
                        myWeb.moSession["ewCmd"] = "";
                        mnAdminUserId = myWeb.mnUserId;
                    }


                    if (!string.IsNullOrEmpty(myWeb.moRequest["rptCmd"]))
                    {
                        mcEwCmd = "RptCourses";
                    }

                    if (mnAdminUserId > 0)
                    {
                        // lets check the current users permission level

                        adminAccessRights();

                        if (moPageXML.DocumentElement.SelectSingleNode("AdminMenu/MenuItem") is null)
                        {

                            if (!(mcEwCmd == "PreviewOn"))
                            {
                                mcEwCmd = "LogOff";
                            }

                        }
                    }

                    // If Not myWeb.moDbHelper.checkUserRole("Administrator") Then
                    // mcEwCmd = "LogOff"
                    // End If

                    else if (!((Strings.LCase(mcEwCmd) ?? "") == (Strings.LCase("LogOff") ?? "")) & !((Strings.LCase(mcEwCmd) ?? "") == (Strings.LCase("PasswordReminder") ?? "")) & !((Strings.LCase(mcEwCmd) ?? "") == (Strings.LCase("AR") ?? "")))
                    {
                        if (Strings.LCase(myWeb.moRequest["ewCmd"]) == "logoff")
                        {
                            myWeb.moSession["ewCmd"] = "";
                        }
                        myWeb.moSession["tempInstance"] = (object)null;
                        myWeb.moSession["ewAuth"] = "";
                        myWeb.mnUserId = 0;
                        mcEwCmd = "";
                    }



                    // lets remember the page we are editing
                    if (myWeb.mnPageId < 1)
                    {
                        myWeb.mnPageId = Conversions.ToInteger(myWeb.moSession["pgid"]);
                    }

                    sAdminLayout = mcEwCmd;

                    nParId = Conversions.ToLong("0" + myWeb.moRequest["parId"]);
                    if (nParId == 0L)
                    {
                        nParId = Conversions.ToLong(myWeb.moSession["nParId"]);
                    }

                    XmlElement AdminMenuNode;
                    if (!string.IsNullOrEmpty(mcEwCmd))
                    {
                        AdminMenuNode = (XmlElement)myWeb.moPageXml.SelectSingleNode("/Page/AdminMenu/descendant-or-self::MenuItem[@cmd='" + mcEwCmd + "']");
                        if (AdminMenuNode != null)
                        {
                            string classPath = AdminMenuNode.GetAttribute("action");
                            // Check for bespoke behaviour
                            if (!string.IsNullOrEmpty(classPath))
                            {

                                Type calledType;
                                string assemblyName = AdminMenuNode.GetAttribute("assembly");
                                string assemblyType = AdminMenuNode.GetAttribute("assemblyType");
                                string providerName = AdminMenuNode.GetAttribute("providerName");
                                string providerType = AdminMenuNode.GetAttribute("providerType");
                                if (string.IsNullOrEmpty(providerType))
                                    providerType = "messaging";

                                string methodName = Strings.Right(classPath, Strings.Len(classPath) - classPath.LastIndexOf(".") - 1);

                                classPath = Strings.Left(classPath, classPath.LastIndexOf("."));

                                if (!string.IsNullOrEmpty(providerName))
                                {
                                    // case for external Providers
                                    Protean.ProviderSectionHandler moPrvConfig = (Protean.ProviderSectionHandler)WebConfigurationManager.GetWebApplicationSection("protean/" + providerType + "Providers");
                                    Assembly assemblyInstance;

                                    if (moPrvConfig.Providers[providerName + "Local"] != null)
                                    {
                                        if (!string.IsNullOrEmpty(moPrvConfig.Providers[providerName + "Local"].Parameters["path"]))
                                        {
                                            assemblyInstance = Assembly.LoadFrom(myWeb.goServer.MapPath(moPrvConfig.Providers[providerName + "Local"].Parameters["path"]));
                                            calledType = assemblyInstance.GetType(classPath, true);
                                        }
                                        else
                                        {
                                            assemblyInstance = Assembly.Load(moPrvConfig.Providers[providerName + "Local"].Type);
                                            calledType = assemblyInstance.GetType(classPath, true);
                                        }
                                    }
                                    else
                                    {
                                        switch (moPrvConfig.Providers[providerName].Parameters["path"] ?? "")
                                        {
                                            case var @case when @case == "":
                                                {
                                                    assemblyInstance = Assembly.Load(moPrvConfig.Providers[providerName].Type);
                                                    calledType = assemblyInstance.GetType(classPath, true);
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
                                                    assemblyInstance = Assembly.LoadFrom(myWeb.goServer.MapPath(moPrvConfig.Providers[providerName].Parameters["path"]));
                                                    classPath = moPrvConfig.Providers[providerName].Parameters["classPrefix"] + classPath;
                                                    calledType = assemblyInstance.GetType(classPath, true);
                                                    break;
                                                }
                                        }

                                        // If moPrvConfig.Providers(providerName).Parameters("path") <> "" Then
                                        // assemblyInstance = [Assembly].LoadFrom(goServer.MapPath(moPrvConfig.Providers(providerName).Parameters("path")))
                                        // Else
                                        // assemblyInstance = [Assembly].Load(moPrvConfig.Providers(providerName).Type)
                                        // End If
                                    }
                                }

                                // calledType = assemblyInstance.GetType(classPath, True)


                                else if (!string.IsNullOrEmpty(assemblyType))
                                {
                                    // case for external DLL's
                                    var assemblyInstance = Assembly.Load(assemblyType);
                                    calledType = assemblyInstance.GetType(classPath, true);
                                }
                                else
                                {
                                    // case for methods within EonicWeb Core DLL
                                    calledType = Type.GetType(classPath, true);
                                }

                                var o = Activator.CreateInstance(calledType);

                                var args = new object[2];
                                args[0] = this;
                                args[1] = oPageDetail;

                                calledType.InvokeMember(methodName, BindingFlags.InvokeMethod, null, o, args);

                                // Error Handling ?
                                // Object Clearup ?

                                calledType = null;
                                if (!string.IsNullOrEmpty(adminLayout))
                                {
                                    sAdminLayout = adminLayout;
                                }

                                goto AfterProcessFlow;
                            }
                            else
                            {
                                goto ProcessFlow;
                            }
                        }
                        else
                        {
                            goto ProcessFlow;
                        }
                    }
                    else
                    {
                        goto ProcessFlow;
                    }

                ProcessFlow:
                    ;


                    switch (mcEwCmd ?? "")
                    {
                        case var case1 when case1 == "":
                            {
                                if (mnAdminUserId == 0)
                                {


                                    Protean.Providers.Membership.ReturnProvider RetProv = new Protean.Providers.Membership.ReturnProvider();
                                    IMembershipProvider oMembershipProv = RetProv.Get(ref myWeb, myWeb.moConfig["MembershipProvider"]);

                                    oPageDetail.AppendChild((XmlNode)oMembershipProv.AdminXforms.xFrmUserLogon("AdminLogon"));

                                    // oPageDetail.AppendChild(moAdXfm.xFrmUserLogon("AdminLogon"))
                                    if (Conversions.ToBoolean(oMembershipProv.AdminXforms.valid))
                                    {

                                        adminAccessRights();

                                        if (moPageXML.DocumentElement.SelectSingleNode("AdminMenu/MenuItem") is null)
                                        {
                                            if (Strings.LCase(mcEwCmd) == "logoff")
                                            {
                                            }
                                            else
                                            {
                                                mcEwCmd = "AdminDenied";

                                            }
                                        }

                                        if (mcEwCmd == "AdminDenied")
                                        {
                                            sAdminLayout = "AdminXForm";
                                            mnAdminUserId = 0;
                                            XmlNode argoNode = (XmlNode)moAdXfm.moXformElmt;
                                            moAdXfm.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, "You do not have administrative access to this site.");
                                            moAdXfm.moXformElmt = (XmlElement)argoNode;
                                        }
                                        else
                                        {
                                            XmlElement oAdminRoot = (XmlElement)myWeb.moPageXml.SelectSingleNode("/Page/AdminMenu/MenuItem");
                                            mcEwCmd = oAdminRoot.GetAttribute("cmd");
                                            if (string.IsNullOrEmpty(mcEwCmd))
                                                mcEwCmd = "Normal";

                                            if (mcEwCmd == "Normal")
                                            {
                                                sAdminLayout = "";
                                            }
                                            else
                                            {
                                                sAdminLayout = mcEwCmd;
                                            }

                                            oPageDetail.RemoveAll();

                                            myWeb.moSession["adminMode"] = "true";

                                            if (Cms.gbSingleLoginSessionPerUser)
                                            {
                                                myWeb.moDbHelper.logActivity(Cms.dbHelper.ActivityType.Logon, (long)mnAdminUserId, 0L);
                                            }

                                            goto ProcessFlow;

                                            // 'Get Status for Dashboard...
                                            // Dim statusElmt As XmlElement = moPageXML.CreateElement("Status")
                                            // statusElmt.InnerXml = myWeb.GetStatus().OuterXml
                                            // oPageDetail.AppendChild(statusElmt)

                                        }
                                    }
                                    else
                                    {
                                        sAdminLayout = "Logon";
                                        myWeb.moSession["tempInstance"] = (object)null;
                                    }
                                }
                                else if (myWeb.mnPageId > 0)
                                {
                                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(myWeb.moSession["ewCmd"], "", false)))
                                    {
                                        mcEwCmd = "Normal";
                                    }
                                    else
                                    {
                                        mcEwCmd = Conversions.ToString(myWeb.moSession["ewCmd"]);
                                    }
                                }

                                break;
                            }

                        case "PasswordReminder":
                            {
                                sAdminLayout = "AdminXForm";
                                Cms argmyWeb = myWeb;
                                Protean.Providers.Membership.ReturnProvider RetProv = new Protean.Providers.Membership.ReturnProvider();
                                IMembershipProvider oMembershipProv = RetProv.Get(ref argmyWeb, this.moConfig["MembershipProvider"]);

                                switch (Strings.LCase(moConfig["MembershipEncryption"]) ?? "")
                                {
                                    case "md5salt":
                                    case "md5":
                                    case "sha1":
                                    case "sha256":
                                        {
                                            oPageDetail.AppendChild((XmlNode)oMembershipProv.AdminXforms.xFrmResetAccount());
                                            break;
                                        }

                                    default:
                                        {
                                            oPageDetail.AppendChild((XmlNode)oMembershipProv.AdminXforms.xFrmPasswordReminder());
                                            break;
                                        }
                                }

                                break;
                            }
                        case "AR":
                            {
                                sAdminLayout = "AdminXForm";
                                Cms argmyWeb2 = myWeb;
                                Protean.Providers.Membership.ReturnProvider RetProv = new Protean.Providers.Membership.ReturnProvider();
                                IMembershipProvider oMembershipProv = RetProv.Get(ref argmyWeb2, this.moConfig["MembershipProvider"]);

                                oPageDetail.AppendChild((XmlNode)oMembershipProv.AdminXforms.xFrmConfirmPassword(myWeb.moRequest["AI"]));
                                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oMembershipProv.AdminXforms.valid, true, false)))
                                {

                                    adminAccessRights();

                                    XmlElement oAdminRoot = (XmlElement)myWeb.moPageXml.SelectSingleNode("/Page/AdminMenu/MenuItem");
                                    mcEwCmd = oAdminRoot.GetAttribute("cmd");
                                    if (string.IsNullOrEmpty(mcEwCmd))
                                        mcEwCmd = "Normal";

                                    if (mcEwCmd == "Normal")
                                    {
                                        sAdminLayout = "";
                                    }
                                    else
                                    {
                                        sAdminLayout = mcEwCmd;
                                    }
                                    oPageDetail.RemoveAll();

                                    myWeb.msRedirectOnEnd = "/";

                                    goto ProcessFlow;
                                }

                                break;
                            }
                        case "LogOff":
                            {
                                if (Cms.gbSingleLoginSessionPerUser)
                                {
                                    myWeb.moDbHelper.logActivity(Cms.dbHelper.ActivityType.Logoff, Convert.ToInt64(mnAdminUserId), 0L);
                                    if (myWeb.moRequest.Cookies["ewslock"] != null)
                                    {
                                        myWeb.moResponse.Cookies["ewslock"].Expires = DateTime.Now.AddDays(-1);
                                    }
                                }
                                myWeb.moSession["ewAuth"] = "";
                                myWeb.moSession["nUserId"] = 0;
                                myWeb.moSession.Abandon();
                                myWeb.mnUserId = 0;
                                myWeb.msRedirectOnEnd = Cms.gcProjectPath + "/" + mcPagePath;
                                break;
                            }

                        case "AdmHome":
                        case "Admin":
                            {
                                var statusElmt = moPageXML.CreateElement("Status");
                                statusElmt.InnerXml = myWeb.GetStatus().OuterXml;
                                oPageDetail.AppendChild(statusElmt);
                                myWeb.moSession["tempInstance"] = (object)null;
                                break;
                            }


                        case "MemberActivity":
                            {
                                MemberActivityProcess(ref oPageDetail, ref sAdminLayout);
                                break;
                            }

                        case "MemberCodes":
                            {
                                MemberCodesProcess(ref oPageDetail, ref sAdminLayout);
                                break;
                            }

                        case "Sync":
                            {

                                myWeb.oSync = new Protean.ExternalSynchronisation(ref myWeb, ref oPageDetail);
                                // AddHandler oSync.OnError, AddressOf OnError
                                sAdminLayout = myWeb.oSync.AdminProcess(myWeb.moRequest["ewCmd2"]);
                                bLoadStructure = true;
                                break;
                            }
                        case "FileImport":
                            {
                                FileImportProcess(ref oPageDetail, ref sAdminLayout);
                                break;
                            }

                        case "AwaitingApproval":
                            {
                                VersionControlProcess(ref oPageDetail, ref sAdminLayout);
                                break;
                            }

                        case "admin":
                            {
                                mcEwCmd = "Content";
                                goto ProcessFlow;
                                break;
                            }


                        case "adminDenied":
                            {

                                sAdminLayout = "adminDenied";

                                oPageDetail.AppendChild(moDeniedAdminMenuElmt);
                                break;
                            }

                        case "WebSettings":
                            {

                                if (string.IsNullOrEmpty(mcEwCmd2))
                                {
                                    oPageDetail.AppendChild(moAdXfm.xFrmWebConfig("General"));
                                }
                                else
                                {
                                    oPageDetail.AppendChild(moAdXfm.xFrmWebConfig(mcEwCmd2));
                                }
                                if (moAdXfm.valid)
                                {
                                    mcEwCmd = "Normal";
                                    myWeb.moCtx.Application["ewSettings"] = (object)null;
                                    myWeb.msRedirectOnEnd = "/?ewCmd=SettingsDash";
                                    myWeb.ClearPageCache();
                                }

                                else
                                {
                                    sAdminLayout = "AdminXForm";
                                }

                                break;
                            }
                        case "301Redirect":
                        case "302Redirect":
                        case "StaticRewrites":
                            {

                                oPageDetail.AppendChild(moAdXfm.xFrmRewriteMaps(mcEwCmd));

                                sAdminLayout = "AdminXForm";
                                break;
                            }

                        case "RewriteRules":
                            {

                                var oCfg = WebConfigurationManager.OpenWebConfiguration("/");
                                Tools.Security.Impersonate oImp = null;
                                if (Conversions.ToBoolean(myWeb.impersonationMode))
                                {
                                    oImp = new Tools.Security.Impersonate();
                                    if (oImp.ImpersonateValidUser(moConfig["AdminAcct"], moConfig["AdminDomain"], moConfig["AdminPassword"], cInGroup: moConfig["AdminGroup"]))
                                    {
                                    }
                                }

                                // code here to replace any missing nodes
                                // all of the required config settings

                                var rewriteXml = new XmlDocument();
                                rewriteXml.Load(myWeb.goServer.MapPath("/RewriteRules.config"));
                                var defaultXml = new XmlDocument();
                                defaultXml.Load(myWeb.goServer.MapPath("/ewcommon/setup/rootfiles/RewriteRules_config.xml"));
                                foreach (XmlElement oRule in rewriteXml.DocumentElement.SelectNodes("rule"))
                                {
                                    string rulename = oRule.GetAttribute("name");
                                    try
                                    {
                                        XmlElement defaultRule = (XmlElement)defaultXml.SelectSingleNode(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("descendant-or-self::rule[@name=", xPathEscapeQuote(rulename)), "]")));
                                        if (defaultRule is null)
                                        {
                                            oRule.SetAttribute("matchDefault", "create");
                                        }
                                        else if ((oRule.OuterXml ?? "") == (defaultRule.OuterXml ?? ""))
                                        {
                                            oRule.SetAttribute("matchDefault", "true");
                                        }
                                        else
                                        {
                                            oRule.SetAttribute("matchDefault", "reset");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        oRule.SetAttribute("matchDefault", ex.Message);
                                    }
                                }
                                var rulesElmt = moPageXML.CreateElement("RewriteRules");
                                rulesElmt.InnerXml = rewriteXml.DocumentElement.OuterXml;
                                oPageDetail.AppendChild(rulesElmt);

                                if (Conversions.ToBoolean(myWeb.impersonationMode))
                                {
                                    oImp.UndoImpersonation();
                                    oImp = null;
                                }

                                sAdminLayout = "RewriteRules";
                                break;
                            }

                        case "SelectTheme":
                            {

                                oPageDetail.AppendChild(moAdXfm.xFrmSelectTheme());
                                if (moAdXfm.valid)
                                {
                                    mcEwCmd = "Normal";
                                    myWeb.msRedirectOnEnd = "/?ewCmd=ThemeSettings";
                                }
                                else
                                {
                                    sAdminLayout = "AdminXForm";
                                }

                                break;
                            }

                        case "InstallTheme":
                            {

                                var SoapObj = new Protean.AdminProxy.ewAdminProxySoapClient();
                                var themesPage = SoapObj.GetThemes(Environment.MachineName, myWeb.moRequest.ServerVariables["SERVER_NAME"]);
                                var ContextThemesPage = moPageXML.CreateElement("Themes");
                                var xreader = themesPage.CreateReader();
                                xreader.MoveToContent();
                                ContextThemesPage.InnerXml = xreader.ReadInnerXml();
                                foreach (XmlElement oTheme in ContextThemesPage.SelectNodes("descendant-or-self::Content[@type='EwTheme']"))
                                    oPageDetail.AppendChild(oTheme);
                                string themeName = myWeb.moRequest["themeName"];
                                if (!string.IsNullOrEmpty(themeName))
                                {
                                    byte[] fileBytes;
                                    fileBytes = SoapObj.GetThemeZip(Environment.MachineName, myWeb.moRequest.ServerVariables["SERVER_NAME"], themeName);

                                    if (fileBytes != null)
                                    {

                                        string strdocPath;
                                        strdocPath = myWeb.goServer.MapPath(Cms.gcProjectPath + "/ewThemes/" + themeName + ".zip");
                                        var objfilestream = new FileStream(strdocPath, FileMode.Create, FileAccess.ReadWrite);
                                        objfilestream.Write(fileBytes, 0, fileBytes.Length);
                                        objfilestream.Close();

                                        // unzip the transfered file
                                        var fz = new ICSharpCode.SharpZipLib.Zip.FastZip();
                                        fz.ExtractZip(myWeb.goServer.MapPath(Cms.gcProjectPath + "/ewThemes/" + themeName + ".zip"), myWeb.goServer.MapPath(moConfig["ProjectPath"] + "/ewThemes/"), "");

                                        // delete the transfered file
                                        var oFile = new FileInfo(myWeb.goServer.MapPath(Cms.gcProjectPath + "/ewThemes/" + themeName + ".zip"));
                                        oFile.Delete();
                                        mcEwCmd = "SelectTheme";
                                        goto ProcessFlow;
                                    }
                                    else
                                    {
                                        sAdminLayout = "InstallTheme";
                                        oPageDetail.SetAttribute("errorMsg", "Theme Installation Failed");

                                    }
                                }

                                else
                                {
                                    sAdminLayout = "InstallTheme";
                                }

                                break;
                            }

                        case "ThemeSettings":
                            {

                                System.Collections.Specialized.NameValueCollection moThemeConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/theme");

                                oPageDetail.AppendChild(moAdXfm.xFrmThemeSettings(moThemeConfig["CurrentTheme"] + "/xforms/Config/ThemeSettings"));

                                if (moAdXfm.valid)
                                {

                                    var argsettingsXml = moAdXfm.Instance;
                                    this.updateLessVariables(moThemeConfig["CurrentTheme"], ref argsettingsXml);
                                    moAdXfm.Instance = argsettingsXml;
                                    var argsettingsXml1 = moAdXfm.Instance;
                                    this.updateStandardXslVariables(moThemeConfig["CurrentTheme"], ref argsettingsXml1);
                                    moAdXfm.Instance = argsettingsXml1;

                                    if (!string.IsNullOrEmpty(myWeb.moRequest["SiteXsl"]))
                                    {
                                        Protean.Config.UpdateConfigValue(ref myWeb, "protean/web", "SiteXsl", myWeb.moRequest["SiteXsl"]);
                                    }

                                    myWeb.moCtx.Application["ewSettings"] = (object)null;
                                    mcEwCmd = "Normal";
                                    myWeb.msRedirectOnEnd = "/?rebundle=true";
                                }
                                // ' When we call the rebundle when we add or edit a page (not content) we also want to clear the sitecache table.
                                // moDbHelper.clearStructureCacheAll()
                                else
                                {
                                    sAdminLayout = "AdminXForm";
                                }

                                break;
                            }

                        case "Normal":
                        case "ByPage":
                        case "Content":
                            {
                                mcEwCmd = "Normal";
                                sAdminLayout = "";
                                EditContext = "Normal";
                                myWeb.moSession["ContentEdit"] = "";

                                if (!myWeb.mbPopupMode)
                                {
                                    if (!string.IsNullOrEmpty(myWeb.moRequest["pgid"]))
                                    {
                                        // lets save the page we are editing to the session
                                        myWeb.moSession["pgid"] = myWeb.moRequest["pgid"];
                                        if (!myWeb.mbSuppressLastPageOverrides)
                                        {
                                            myWeb.moSession["lastPage"] = "/" + Cms.gcProjectPath + myWeb.mcPagePath.TrimStart('/') + "?ewCmd=Normal&pgid=" + myWeb.mnPageId; // myWeb.mcOriginalURL
                                        }
                                    }

                                    if (string.IsNullOrEmpty(myWeb.moRequest["pgid"]) & string.IsNullOrEmpty(myWeb.mcPagePath))
                                    {
                                        // this gets called only in WYSIWYG mode and I don't know why?
                                        // This Is a fudge to stop always redirect to the homepage.
                                        myWeb.mbSuppressLastPageOverrides = true;
                                    }
                                    // we want to return here after editing
                                    if (!myWeb.mbSuppressLastPageOverrides)
                                    {
                                        myWeb.moSession["lastPage"] = "/" + Cms.gcProjectPath + myWeb.mcPagePath.TrimStart('/') + "?ewCmd=Normal&pgid=" + myWeb.mnPageId; // 
                                    }

                                }

                                break;
                            }

                        case "Advanced":
                            {
                                sAdminLayout = "Advanced";
                                EditContext = "Advanced";

                                var oSiteManifest = new XmlDocument();
                                if (myWeb.moConfig["cssFramework"] == "bs5")
                                {
                                    oSiteManifest = moAdXfm.GetSiteManifest();
                                }
                                else
                                {
                                    if (File.Exists(myWeb.goServer.MapPath("/ewcommon/xsl/pagelayouts/layoutmanifest.xml")))
                                    {
                                        oSiteManifest.Load(myWeb.goServer.MapPath("/ewcommon/xsl/pagelayouts/layoutmanifest.xml"));
                                    }
                                    if (File.Exists(myWeb.goServer.MapPath(Cms.gcProjectPath + "/xsl/layoutmanifest.xml")))
                                    {
                                        var oLocalContentTypes = new XmlDocument();
                                        oLocalContentTypes.Load(myWeb.goServer.MapPath(Cms.gcProjectPath + "/xsl/layoutmanifest.xml"));
                                        XmlElement oLocals = (XmlElement)oLocalContentTypes.SelectSingleNode("/PageLayouts/ContentTypes");
                                        if (oLocals != null)
                                        {
                                            foreach (XmlElement oGrp in oLocals.SelectNodes("ContentTypeGroup"))
                                            {
                                                XmlElement oComGrp = (XmlElement)oSiteManifest.SelectSingleNode("/PageLayouts/ContentTypes/ContentTypeGroup[@name='" + oGrp.GetAttribute("name") + "']");
                                                if (oComGrp != null)
                                                {
                                                    foreach (XmlElement oTypeElmt in oGrp.SelectNodes("ContentType"))
                                                    {
                                                        if (oComGrp.SelectSingleNode("ContentType[@type='" + oTypeElmt.GetAttribute("type") + "']") != null)
                                                        {
                                                            oComGrp.SelectSingleNode("ContentType[@type='" + oTypeElmt.GetAttribute("type") + "']").InnerText = oTypeElmt.InnerText;
                                                        }
                                                        else
                                                        {
                                                            oComGrp.InnerXml += oTypeElmt.OuterXml;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    oSiteManifest.DocumentElement.SelectSingleNode("ContentTypes").InnerXml += oGrp.OuterXml;
                                                }
                                            }
                                        }
                                    }
                                }

                                // now to add it to the pagexml
                                oPageDetail.AppendChild(oPageDetail.OwnerDocument.ImportNode(oSiteManifest.SelectSingleNode("/PageLayouts/ContentTypes"), true));

                                if (!string.IsNullOrEmpty(myWeb.moRequest["pgid"]))
                                {
                                    // lets save the page we are editing to the session
                                    myWeb.moSession["pgid"] = myWeb.moRequest["pgid"];
                                }
                                // we want to return here after editing
                                myWeb.moSession["lastPage"] = myWeb.mcOriginalURL;
                                break;
                            }

                        case "ByType":
                            {
                                // list all content of a specific type

                                sAdminLayout = "ByType";
                                EditContext = "ByType." + mcEwCmd2;
                                if (!string.IsNullOrEmpty(mcEwCmd3))
                                {
                                    EditContext = EditContext + "." + mcEwCmd3;
                                }
                                myWeb.moSession["lastPage"] = "/" + Cms.gcProjectPath + myWeb.mcPagePath.TrimStart('/') + "/?ewCmd=ByType." + mcEwCmd2 + "." + mcEwCmd3;
                                string ContentType = mcEwCmd2;
                                string Filter = mcEwCmd3;
                                string FilterSQL = "";
                                string[] FilterArr;

                                if (!string.IsNullOrEmpty(Filter))
                                {
                                    FilterArr = Filter.Split(':');
                                    string FilterName = FilterArr[0];
                                    string FilterValue;
                                    if (FilterArr.Length == 1)
                                    {
                                        FilterValue = myWeb.moRequest[FilterName];
                                    }
                                    else
                                    {
                                        FilterValue = FilterArr[1];
                                    }
                                    switch (FilterArr[0] ?? "")
                                    {
                                        case "Search":
                                            {
                                                // Get a list of possible locations for this content type
                                                var oXfrm = new Cms.xForm(ref myWeb);
                                                oXfrm.NewFrm("LocationFilter");
                                                oXfrm.submission("LocationFilter", "/?ewCmd=ByType." + ContentType + ".Location", "post", "");
                                                string sSql = "select dbo.fxn_getPagePath(nStructKey) as name, nStructKey as value from tblContentStructure where nStructKey in " + "(select nStructId from tblContentLocation cl inner join tblContent c on cl.nContentID = c.nContentKey where cContentSchemaName = '" + ContentType + "' and bPrimary = 1 ) order by name";
                                                var locSelect = oXfrm.addSelect1(ref oXfrm.moXformElmt, "Location", false, "Select Location", "submit-on-select");

                                                if (!string.IsNullOrEmpty(myWeb.moRequest["Location"]))
                                                {
                                                    FilterValue = myWeb.moRequest["Location"];
                                                    myWeb.mnPageId = (int)Conversions.ToLong("0" + FilterValue);
                                                }
                                                else if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(myWeb.moSession["FilterValue"], "", false)))
                                                {
                                                    FilterValue = Conversions.ToString(myWeb.moSession["FilterValue"]);
                                                }
                                                oXfrm.addValue(ref locSelect, FilterValue);
                                                using (var oDr = myWeb.moDbHelper.getDataReaderDisposable(sSql))  // Done by sonali on 12/7/22
                                                {
                                                    var argoDr = oDr;
                                                    oXfrm.addUserOptionsFromSqlDataReader(ref locSelect, ref argoDr);
                                                }
                                                oPageDetail.AppendChild(oXfrm.moXformElmt);
                                                myWeb.ClearPageCache();


                                                Cms.Search oSrch;
                                                oSrch = new Cms.Search(ref myWeb);

                                                oSrch.moContextNode = (XmlElement)moPageXML.DocumentElement.SelectSingleNode("Contents");
                                                if (oSrch.moContextNode is null)
                                                {
                                                    oSrch.moContextNode = moPageXML.CreateElement("Contents");
                                                    moPageXML.DocumentElement.AppendChild(oSrch.moContextNode);
                                                }
                                                oSrch.FullTextQuery(myWeb.moRequest["searchString"], ContentType);
                                                bLoadStructure = true;
                                                break;
                                            }

                                        case "Location":
                                            {
                                                // Get a list of possible locations for this content type
                                                var oXfrm = new Cms.xForm(ref myWeb);
                                                oXfrm.NewFrm("LocationFilter");
                                                oXfrm.submission("LocationFilter", "/?ewCmd=ByType." + ContentType + ".Location", "post", "");
                                                // Dim sSql As String = "select dbo.fxn_getPagePath(nStructKey) as name, nStructKey as value from tblContentStructure where nStructKey in " &
                                                // "(select nStructId from tblContentLocation cl inner join tblContent c on cl.nContentID = c.nContentKey where cContentSchemaName = '" & ContentType & "' and bPrimary = 1 ) order by name"

                                                string sSql = "select dbo.fxn_getPagePath(nStructKey) as name, nStructKey as value from tblContentStructure " + "where ( select count(cl1.nContentID) from tblContentLocation cl1 inner join tblContent tc on tc.nContentKey = cl1.nContentId  where cl1.nStructId = nStructKey and tc.cContentSchemaName = '" + ContentType + "') > 0 order by name";


                                                var locSelect = oXfrm.addSelect1(ref oXfrm.moXformElmt, "Location", false, "Select Location", "submit-on-select");

                                                if (!string.IsNullOrEmpty(myWeb.moRequest["Location"]))
                                                {
                                                    FilterValue = myWeb.moRequest["Location"];
                                                    myWeb.mnPageId = (int)Conversions.ToLong("0" + FilterValue);
                                                }
                                                else if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(myWeb.moSession["FilterValue"], "", false)))
                                                {
                                                    FilterValue = Conversions.ToString(myWeb.moSession["FilterValue"]);
                                                }
                                                oXfrm.addValue(ref locSelect, FilterValue);
                                                using (var oDr = myWeb.moDbHelper.getDataReaderDisposable(sSql))  // Done by sonali on 12/7/22
                                                {
                                                    var argoDr1 = oDr;
                                                    oXfrm.addUserOptionsFromSqlDataReader(ref locSelect, ref argoDr1);
                                                }
                                                oPageDetail.AppendChild(oXfrm.moXformElmt);
                                                myWeb.ClearPageCache();

                                                string cSort = "|ASC_cl.nDisplayOrder";
                                                switch (myWeb.moRequest["sortby"] ?? "")
                                                {
                                                    case "name":
                                                        {
                                                            cSort = "|ASC_c.cContentName";
                                                            break;
                                                        }

                                                    default:
                                                        {
                                                            cSort = "|ASC_cl.nDisplayOrder";
                                                            break;
                                                        }
                                                }

                                                // get a list of pages with this content on.
                                                if (!string.IsNullOrEmpty(FilterValue))
                                                {
                                                    FilterSQL = " and CL.nStructId = '" + FilterValue + "'";
                                                    int nStart = 0;
                                                    int nRows = 500;

                                                    // Set the paging variables, if provided.
                                                    if (myWeb.moRequest["startPos"] != null && Information.IsNumeric(myWeb.moRequest["startPos"]))
                                                        nStart = Conversions.ToInteger(myWeb.moRequest["startPos"]);
                                                    if (myWeb.moRequest["rows"] != null && Information.IsNumeric(myWeb.moRequest["rows"]))
                                                        nRows = Conversions.ToInteger(myWeb.moRequest["rows"]);

                                                    var argoPageElmt = moPageXML.DocumentElement;
                                                    XmlElement argoContentModule = null;
                                                    myWeb.GetContentXMLByTypeAndOffset(ref argoPageElmt, ContentType + cSort, (long)nStart, (long)nRows, ref oPageDetail, oContentModule: ref argoContentModule, FilterSQL, "");

                                                    // myWeb.GetContentXMLByTypeAndOffset(moPageXML.DocumentElement, ContentType & cSort, FilterSQL, "", oPageDetail)
                                                    XmlElement contentsNode = (XmlElement)moPageXML.SelectSingleNode("/Page/Contents");
                                                    if (contentsNode != null)
                                                    {
                                                        int itemCount = contentsNode.SelectNodes("Content").Count;
                                                        contentsNode.SetAttribute("count", itemCount.ToString());
                                                        if (!(contentsNode == null))
                                                        {
                                                            DateTime argdUpdateDate = DateTime.Parse("0001-01-01");
                                                            myWeb.moDbHelper.addBulkRelatedContent(ref contentsNode, dUpdateDate: ref argdUpdateDate);
                                                        }
                                                    }
                                                    myWeb.moSession["FilterValue"] = FilterValue;

                                                }

                                                break;
                                            }

                                        case "User":
                                            {
                                                if (myWeb.moDbHelper.checkUserRole("Administrator"))
                                                {
                                                    var argoPageElmt1 = moPageXML.DocumentElement;
                                                    myWeb.GetContentXMLByType(ref argoPageElmt1, ContentType);
                                                }
                                                else
                                                {
                                                    FilterSQL = " and a.nInsertDirId = '" + mnAdminUserId + "'";
                                                    var argoPageElmt2 = moPageXML.DocumentElement;
                                                    myWeb.GetContentXMLByType(ref argoPageElmt2, ContentType, FilterSQL);
                                                }

                                                break;
                                            }
                                        case "Sale":
                                            {
                                                var argoPageElmt3 = moPageXML.DocumentElement;
                                                myWeb.GetContentXMLByType(ref argoPageElmt3, ContentType);
                                                break;
                                            }

                                        case "All":
                                            {
                                                var argoPageElmt4 = moPageXML.DocumentElement;
                                                myWeb.GetContentXMLByType(ref argoPageElmt4, ContentType, " order by a.dInsertDate desc");
                                                break;
                                            }

                                        case "UserRead":
                                            {

                                                string sSQL = "select c.nContentKey as id, dbo.fxn_getContentParents(c.nContentKey) as parId, c.cContentForiegnRef as ref, pc.cContentName as name, c.cContentSchemaName as type, c.cContentXmlBrief ";
                                                sSQL += "as content, a.nStatus as status, a.dInsertDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, CL.cPosition as position ";
                                                sSQL += "from tblContent c left outer join tblContentLocation CL on c.nContentKey = CL.nContentId inner join tblAudit a on c.nAuditId = a.nAuditKey ";
                                                sSQL += "inner join tblDirectory d on a.nInsertDirId = d.nDirKey left outer join tblActivityLog act on act.nArtId = c.nContentKey And act.nUserDirId = " + myWeb.mnUserId + " ";
                                                sSQL += "inner join tblContentRelation cr on cr.nContentChildId =  c.nContentKey ";
                                                sSQL += "inner join tblContent pc on pc.nContentKey = cr.nContentParentId ";
                                                sSQL += "where (c.cContentSchemaName = '" + ContentType + "') ";
                                                sSQL += "And pc.cContentSchemaName = 'Product'";
                                                sSQL += "and act.nArtId is not null ";
                                                sSQL += "And a.nInsertDirId <> " + myWeb.mnUserId + " ";
                                                sSQL += "order by a.dInsertDate desc, a.dExpireDate desc";


                                                var argoPageElmt5 = moPageXML.DocumentElement;
                                                myWeb.GetContentXMLByType(ref argoPageElmt5, ContentType, fullSQL: sSQL);
                                                break;
                                            }


                                        case "UserUnRead":
                                            {


                                                string sSQL = "select c.nContentKey as id, dbo.fxn_getContentParents(c.nContentKey) as parId, c.cContentForiegnRef as ref, pc.cContentName as name, c.cContentSchemaName as type, c.cContentXmlBrief ";
                                                sSQL += "as content, a.nStatus as status, a.dInsertDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, CL.cPosition as position ";
                                                sSQL += "from tblContent c left outer join tblContentLocation CL on c.nContentKey = CL.nContentId inner join tblAudit a on c.nAuditId = a.nAuditKey ";
                                                sSQL += "inner join tblDirectory d on a.nInsertDirId = d.nDirKey left outer join tblActivityLog act on act.nArtId = c.nContentKey And act.nUserDirId = " + myWeb.mnUserId + " ";
                                                sSQL += "inner join tblContentRelation cr on cr.nContentChildId =  c.nContentKey ";
                                                sSQL += "inner join tblContent pc on pc.nContentKey = cr.nContentParentId ";
                                                sSQL += "where (c.cContentSchemaName = '" + ContentType + "') ";
                                                sSQL += "And pc.cContentSchemaName = 'Product'";
                                                sSQL += "and act.nArtId is null ";
                                                sSQL += "And a.nInsertDirId <> " + myWeb.mnUserId + " ";
                                                sSQL += "order by a.dInsertDate desc, a.dExpireDate desc";

                                                var argoPageElmt6 = moPageXML.DocumentElement;
                                                myWeb.GetContentXMLByType(ref argoPageElmt6, ContentType, fullSQL: sSQL);
                                                break;
                                            }

                                    }
                                }
                                else
                                {

                                    var argoPageElmt7 = moPageXML.DocumentElement;
                                    myWeb.GetContentXMLByType(ref argoPageElmt7, ContentType);

                                }

                                break;
                            }



                        case "AddModule":
                            {

                                bLoadStructure = true;
                                nAdditionId = 0;

                                oPageDetail.AppendChild(moAdXfm.xFrmAddModule(Conversions.ToLong(myWeb.moRequest["pgid"]), myWeb.moRequest["position"]));
                                if (moAdXfm.valid)
                                {
                                    myWeb.ClearPageCache();

                                    if (!string.IsNullOrEmpty(myWeb.moRequest["nStatus"]))
                                    {
                                        oPageDetail.RemoveAll();
                                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(myWeb.moSession["lastPage"], "", false)))
                                        {
                                            myWeb.msRedirectOnEnd = Conversions.ToString(myWeb.moSession["lastPage"]);
                                            myWeb.moSession["lastPage"] = "";
                                        }
                                        else
                                        {
                                            mcEwCmd = "Normal";
                                            oPageDetail.RemoveAll();
                                            moAdXfm.valid = false;
                                            goto ProcessFlow;
                                        }
                                    }
                                    else
                                    {
                                        mcEwCmd = "AddContent";
                                        sAdminLayout = "AdminXForm";
                                    }
                                }

                                else
                                {
                                    sAdminLayout = "AdminXForm";
                                }

                                break;
                            }

                        case "AddContent":
                            {

                                bLoadStructure = true;
                                nAdditionId = 0;
                                nContentId = 0L;
                                bClearEditContext = false;

                                string argzcReturnSchema = "";
                                string argAlternateFormName = "";
                                oPageDetail.AppendChild(moAdXfm.xFrmEditContent(0L, myWeb.moRequest["type"], Conversions.ToLong(myWeb.moRequest["pgid"]), myWeb.moRequest["name"], false, nReturnId: ref nAdditionId, zcReturnSchema: ref argzcReturnSchema, AlternateFormName: ref argAlternateFormName));
                                if (moAdXfm.valid)
                                {
                                    sAdminLayout = "";
                                    mcEwCmd = Conversions.ToString(myWeb.moSession["ewCmd"]);
                                    myWeb.ClearPageCache();

                                    // if we have a parent releationship lets add it but not if we have a relation type becuase that happens in the xform.
                                    if (!string.IsNullOrEmpty(myWeb.moRequest["contentParId"]) && Information.IsNumeric(myWeb.moRequest["contentParId"]))
                                    {
                                        bool b2Way = Conversions.ToBoolean(Interaction.IIf(myWeb.moRequest["RelType"] == "2way" | myWeb.moRequest["direction"] == "2Way", (object)true, (object)false));
                                        string sRelType = myWeb.moRequest["relationType"];
                                        myWeb.moDbHelper.insertContentRelation(Conversions.ToInteger(myWeb.moRequest["contentParId"]), nAdditionId.ToString(), b2Way, sRelType);
                                    }

                                    oPageDetail.RemoveAll();

                                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(myWeb.moSession["lastPage"], "", false)))
                                    {
                                        myWeb.msRedirectOnEnd = Conversions.ToString(myWeb.moSession["lastPage"]);
                                        myWeb.moSession["lastPage"] = "";
                                    }
                                    else
                                    {
                                        oPageDetail.RemoveAll();
                                        moAdXfm.valid = false;
                                        goto ProcessFlow;
                                    }
                                }

                                else
                                {
                                    sAdminLayout = "AdminXForm";
                                }

                                if (mcEwCmd == "Advanced")
                                    goto ProcessFlow;
                                break;
                            }

                        case "EditContent":
                            {
                                // Get a version Id if it's passed through.
                                string cVersionKey = myWeb.moRequest["verId"] + "";
                                bClearEditContext = false;
                                bLoadStructure = true;

                                if (Information.IsNumeric(myWeb.moRequest["pgid"]))
                                {
                                    myWeb.gcLang = myWeb.moDbHelper.getPageLang(Conversions.ToLong(myWeb.moRequest["pgid"]));
                                }

                                if (!Information.IsNumeric(cVersionKey))
                                    cVersionKey = "0";
                                nContentId = 0L;
                                string zcReturnSchema = "";
                                string AlternateFormName = "";
                                XmlElement localxFrmEditContent() { int argnReturnId = (int)nContentId; var ret = moAdXfm.xFrmEditContent(Convert.ToInt64(myWeb.moRequest["id"]), "", Conversions.ToLong(myWeb.moRequest["pgid"]), "", false, nReturnId: ref argnReturnId, ref zcReturnSchema, ref AlternateFormName, nVersionId: Convert.ToInt64(cVersionKey)); nContentId = argnReturnId; return ret; }

                                oPageDetail.AppendChild(localxFrmEditContent());

                                if (moAdXfm.valid)
                                {
                                    bAdminMode = false;
                                    sAdminLayout = "";
                                    mcEwCmd = Conversions.ToString(myWeb.moSession["ewCmd"]);

                                    myWeb.ClearPageCache();

                                    // if we have a parent releationship lets add it
                                    if (!string.IsNullOrEmpty(myWeb.moRequest["contentParId"]) && Information.IsNumeric(myWeb.moRequest["contentParId"]))
                                    {
                                        myWeb.moDbHelper.insertContentRelation(Conversions.ToInteger(myWeb.moRequest["contentParId"]), nContentId.ToString());
                                    }
                                    if (!string.IsNullOrEmpty(myWeb.moRequest["EditXForm"]))
                                    {
                                        bAdminMode = true;
                                        sAdminLayout = "AdminXForm";
                                        mcEwCmd = "EditXForm";
                                        oPageDetail = oWeb.GetContentDetailXml(nArtId: Conversions.ToLong(myWeb.moRequest["id"]));
                                    }
                                    else
                                    {
                                        myWeb.mnArtId = 0;
                                        oPageDetail.RemoveAll();
                                        myWeb.moSession["ContentEdit"] = "";
                                        // Check for an optional command to redireect to
                                        if (!string.IsNullOrEmpty("" + myWeb.moRequest["ewRedirCmd"]))
                                        {
                                            myWeb.msRedirectOnEnd = Cms.gcProjectPath + "/?ewCmd=" + myWeb.moRequest["ewRedirCmd"];
                                        }
                                        else if (myWeb.msRedirectOnEnd.Contains("?ewCmd=PreviewOn"))
                                        {
                                            // skip if already defined in Xform.
                                            myWeb.moSession["lastPage"] = "";
                                        }
                                        else if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(myWeb.moSession["lastPage"], "", false)))
                                        {
                                            if (mcEwCmd == "EditPageSEO")
                                            {
                                                if (!string.IsNullOrEmpty("" + myWeb.moRequest["pgid"]))
                                                {
                                                    myWeb.msRedirectOnEnd = "/?ewCmd=" + mcEwCmd + "&pgid=" + myWeb.moRequest["pgid"];
                                                }
                                                else
                                                {
                                                    myWeb.msRedirectOnEnd = Conversions.ToString(myWeb.moSession["lastPage"]);
                                                    myWeb.moSession["lastPage"] = "";
                                                }
                                            }
                                            else
                                            {
                                                myWeb.msRedirectOnEnd = Conversions.ToString(myWeb.moSession["lastPage"]);
                                                myWeb.moSession["lastPage"] = "";
                                            }
                                        }
                                        else
                                        {
                                            oPageDetail.RemoveAll();
                                            moAdXfm.valid = false;
                                            goto ProcessFlow;
                                        }
                                    }
                                }
                                else
                                {

                                    if (!string.IsNullOrEmpty(myWeb.moRequest["id"]))
                                    {
                                        myWeb.moSession["ContentEdit"] = mcPagePath + "ewCmd=EditContent&id=" + myWeb.moRequest["id"];
                                    }

                                    sAdminLayout = "AdminXForm";

                                }

                                break;
                            }

                        case "PublishContent":
                            {

                                myWeb.moDbHelper.contentStatus(Conversions.ToLong(myWeb.moRequest["id"]), Conversions.ToLong("0" + myWeb.moRequest["verId"]), Cms.dbHelper.Status.Live);
                                myWeb.msRedirectOnEnd = "?ewCmd=PreviewOn&pgid=" + myWeb.moRequest["pgid"] + "&artid=" + myWeb.moRequest["id"];
                                break;
                            }

                        case "RollbackContent":
                            {

                                string zcReturnSchema = "";
                                string AlternateFormName = "";
                                bLoadStructure = true;
                                int nReturnId = 0;
                                oPageDetail.AppendChild(moAdXfm.xFrmEditContent(Conversions.ToLong(myWeb.moRequest["id"]), "", Conversions.ToLong(myWeb.moRequest["pgid"]), "", false, ref nReturnId, ref zcReturnSchema, ref AlternateFormName, nVersionId: Conversions.ToLong(myWeb.moRequest["verId"])));
                                if (moAdXfm.valid)
                                {
                                    bAdminMode = false;
                                    sAdminLayout = "";
                                    mcEwCmd = Conversions.ToString(myWeb.moSession["ewCmd"]);
                                    myWeb.mnArtId = 0;
                                    oPageDetail.RemoveAll();
                                    myWeb.ClearPageCache();

                                    // lest just try this redirecting to last page
                                    if (mcEwCmd == "Normal")
                                    {
                                        myWeb.msRedirectOnEnd = Conversions.ToString(myWeb.moSession["lastPage"]);
                                    }
                                }
                                else
                                {
                                    sAdminLayout = "AdminXForm";
                                }

                                break;
                            }

                        case "CopyContent":
                            {
                                bLoadStructure = true;
                                bClearEditContext = false;
                                int argnReturnId = 0;
                                string argzcReturnSchema1 = "";
                                string argAlternateFormName1 = "";
                                oPageDetail.AppendChild(moAdXfm.xFrmEditContent(Conversions.ToLong(myWeb.moRequest["id"]), "", Conversions.ToLong(myWeb.moRequest["pgid"]), "", bCopy: true, nReturnId: ref argnReturnId, zcReturnSchema: ref argzcReturnSchema1, AlternateFormName: ref argAlternateFormName1));
                                if (moAdXfm.valid)
                                {
                                    bAdminMode = false;
                                    sAdminLayout = "";
                                    mcEwCmd = Conversions.ToString(myWeb.moSession["ewCmd"]);
                                    if (myWeb.moRequest["submit"] == "Edit Questions")
                                    {
                                        bAdminMode = true;
                                        sAdminLayout = "AdminXForm";
                                        mcEwCmd = "EditXForm";
                                        oPageDetail = oWeb.GetContentDetailXml(nArtId: Conversions.ToLong(myWeb.moRequest["id"]));
                                    }
                                    else
                                    {
                                        myWeb.mnArtId = 0;
                                        oPageDetail.RemoveAll();
                                    }
                                    myWeb.ClearPageCache();

                                    goto ProcessFlow;
                                }
                                else
                                {
                                    sAdminLayout = "AdminXForm";
                                }

                                break;
                            }


                        case "UpdateContentValue":
                            {

                                string nMyContentId = myWeb.moRequest["id"];
                                string cContentXpath = myWeb.moRequest["xpath"];
                                string cContentValue = myWeb.moRequest["value"];
                                var oTempInstance = new XmlDocument();

                                if (!cContentXpath.StartsWith("/instance/tblContent/cContentXmlBrief/Content"))
                                {
                                    cContentXpath = "/instance/tblContent/cContentXmlBrief/Content" + cContentXpath;
                                }

                                oTempInstance.InnerXml = "<instance>" + myWeb.moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.Content, Conversions.ToLong(nMyContentId)) + "</instance>";
                                XmlElement ElmtToChange = (XmlElement)oTempInstance.DocumentElement.SelectSingleNode(cContentXpath);
                                if (ElmtToChange != null)
                                {
                                    ElmtToChange.InnerText = cContentValue;
                                }
                                ElmtToChange = (XmlElement)oTempInstance.DocumentElement.SelectSingleNode(cContentXpath.Replace("cContentXmlBrief", "cContentXmlDetail"));
                                if (ElmtToChange != null)
                                {
                                    ElmtToChange.InnerText = cContentValue;
                                }
                                myWeb.moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Content, oTempInstance.DocumentElement, Conversions.ToLong(nMyContentId));
                                myWeb.ClearPageCache();

                                sAdminLayout = "AjaxReturnTrue";
                                break;
                            }



                        case "RemoveContentRelation":
                            {
                                // remove content relation from item
                                myWeb.moDbHelper.RemoveContentRelation(Conversions.ToLong(myWeb.moRequest["relId"]), Conversions.ToLong(myWeb.moRequest["id"]));
                                mcEwCmd = Conversions.ToString(myWeb.moSession["ewCmd"]);
                                // lest just try this redirecting to last page
                                if (mcEwCmd == "Normal" | mcEwCmd == "NormalMail")
                                {
                                    myWeb.msRedirectOnEnd = Conversions.ToString(myWeb.moSession["lastPage"]);
                                }
                                myWeb.ClearPageCache();
                                oPageDetail.RemoveAll();
                                myWeb.mnArtId = 0;
                                if (mcEwCmd == "Advanced")
                                    goto ProcessFlow;
                                break;
                            }

                        case "RemoveContentLocation":
                            {
                                // remove content relation from item
                                myWeb.moDbHelper.RemoveContentLocation(Conversions.ToLong(myWeb.moRequest["pgid"]), Conversions.ToLong(myWeb.moRequest["id"]));
                                mcEwCmd = Conversions.ToString(myWeb.moSession["ewCmd"]);
                                // lest just try this redirecting to last page
                                if (mcEwCmd == "Normal" | mcEwCmd == "NormalMail")
                                {
                                    myWeb.msRedirectOnEnd = Conversions.ToString(myWeb.moSession["lastPage"]);
                                }
                                myWeb.ClearPageCache();
                                oPageDetail.RemoveAll();
                                myWeb.mnArtId = 0;
                                if (mcEwCmd == "Advanced")
                                    goto ProcessFlow;
                                break;
                            }
                        case "BulkContentAction":
                            {
                                switch (myWeb.moRequest["BulkAction"] ?? "")
                                {
                                    case "Move":
                                        {
                                            mcEwCmd = "MoveContent";
                                            sAdminLayout = "MoveContent";
                                            goto ProcessFlow;
                                        }
                                    case "Copy":
                                        {
                                            mcEwCmd = "CopyContent";
                                            sAdminLayout = "CopyContent";
                                            goto ProcessFlow;
                                        }
                                    case "Locate":
                                        {
                                            mcEwCmd = "LocateContent";
                                            sAdminLayout = "LocateContent";
                                            goto ProcessFlow;
                                        }
                                    case "Hide":
                                        {
                                            mcEwCmd = "HideContent";
                                            sAdminLayout = "HideContent";
                                            goto ProcessFlow;
                                        }
                                    case "Show":
                                        {
                                            mcEwCmd = "ShowContent";
                                            sAdminLayout = "ShowContent";
                                            goto ProcessFlow;
                                        }
                                    case "Delete":
                                        {
                                            mcEwCmd = "DeleteContent";
                                            sAdminLayout = "DeleteContent";
                                            goto ProcessFlow;
                                        }
                                }

                                break;
                            }
                        case "HideContent":
                            {
                                // hide content

                                if (myWeb.moRequest["id"].Contains(","))
                                {
                                    string[] ids = myWeb.moRequest["id"].Split(new char[] { ',' });
                                    foreach (var id in ids)
                                        myWeb.moDbHelper.setObjectStatus(Cms.dbHelper.objectTypes.Content, Cms.dbHelper.Status.Hidden, Conversions.ToLong(id));
                                }
                                else
                                {
                                    myWeb.moDbHelper.setObjectStatus(Cms.dbHelper.objectTypes.Content, Cms.dbHelper.Status.Hidden, Conversions.ToLong(myWeb.moRequest["id"]));
                                }

                                mcEwCmd = Conversions.ToString(myWeb.moSession["ewCmd"]);
                                // lest just try this redirecting to last page
                                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(myWeb.moSession["ContentEdit"], "", false)))
                                {
                                    myWeb.msRedirectOnEnd = Conversions.ToString(myWeb.moSession["ContentEdit"]);
                                    myWeb.moSession["ContentEdit"] = "";
                                }
                                else if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(myWeb.moSession["lastPage"], "", false)))
                                {
                                    myWeb.msRedirectOnEnd = Conversions.ToString(myWeb.moSession["lastPage"]);
                                    myWeb.moSession["lastPage"] = "";
                                }
                                else
                                {
                                    oPageDetail.RemoveAll();
                                    moAdXfm.valid = false;
                                    goto ProcessFlow;
                                }
                                myWeb.ClearPageCache();

                                oPageDetail.RemoveAll();
                                myWeb.mnArtId = 0;
                                if (mcEwCmd == "Advanced")
                                    goto ProcessFlow;
                                break;
                            }

                        case "ShowContent":
                            {
                                // hide content
                                if (myWeb.moRequest["id"].Contains(","))
                                {
                                    string[] ids = myWeb.moRequest["id"].Split(new char[] { ',' });
                                    foreach (var id in ids)
                                        myWeb.moDbHelper.setObjectStatus(Cms.dbHelper.objectTypes.Content, Cms.dbHelper.Status.Live, Conversions.ToLong(id));
                                }
                                else
                                {
                                    myWeb.moDbHelper.setObjectStatus(Cms.dbHelper.objectTypes.Content, Cms.dbHelper.Status.Live, Conversions.ToLong(myWeb.moRequest["id"]));
                                }
                                mcEwCmd = Conversions.ToString(myWeb.moSession["ewCmd"]);
                                // lest just try this redirecting to last page
                                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(myWeb.moSession["ContentEdit"], "", false)))
                                {
                                    myWeb.msRedirectOnEnd = Conversions.ToString(myWeb.moSession["ContentEdit"]);
                                    myWeb.moSession["ContentEdit"] = "";
                                }
                                else if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(myWeb.moSession["lastPage"], "", false)))
                                {
                                    myWeb.msRedirectOnEnd = Conversions.ToString(myWeb.moSession["lastPage"]);
                                    myWeb.moSession["lastPage"] = "";
                                }
                                else
                                {
                                    oPageDetail.RemoveAll();
                                    moAdXfm.valid = false;
                                    goto ProcessFlow;
                                }
                                myWeb.ClearPageCache();
                                oPageDetail.RemoveAll();
                                myWeb.mnArtId = 0;
                                if (mcEwCmd == "Advanced")
                                    goto ProcessFlow;
                                break;
                            }

                        case "DeleteContent":
                            {
                                string ids = myWeb.moRequest["id"];
                                string[] bulkIds = (string[])ids.Split(',');
                                var status = default(int);
                                int count = Conversions.ToInteger(ids.Split(',').Length);
                                int totalStatusCount = 0;
                                foreach (var id in bulkIds)
                                {
                                    status = Conversions.ToInteger(myWeb.moDbHelper.getObjectStatus(Cms.dbHelper.objectTypes.Content, id));
                                    if (status != 1)
                                    {
                                        totalStatusCount = totalStatusCount + 1;
                                    }

                                }
                                if (count == totalStatusCount)
                                {
                                    if (status != 1)  // check status here
                                    {
                                        oPageDetail.AppendChild(moAdXfm.xFrmDeleteBulkContent(bulkIds));
                                    }
                                }
                                else
                                {
                                    moAdXfm.addNote("DeleteContent", Protean.xForm.noteTypes.Alert, "Invalid product selection", false, "alert-danger");
                                }



                                if (moAdXfm.valid)
                                {
                                    bAdminMode = false;
                                    sAdminLayout = "";
                                    mcEwCmd = Conversions.ToString(myWeb.moSession["ewCmd"]);
                                    // lest just try this redirecting to last page
                                    myWeb.ClearPageCache();
                                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(myWeb.moSession["ContentEdit"], "", false)))
                                    {
                                        myWeb.msRedirectOnEnd = Conversions.ToString(myWeb.moSession["ContentEdit"]);
                                        myWeb.moSession["ContentEdit"] = "";
                                    }
                                    else if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(myWeb.moSession["lastPage"], "", false)))
                                    {
                                        myWeb.msRedirectOnEnd = Conversions.ToString(myWeb.moSession["lastPage"]);
                                        myWeb.moSession["lastPage"] = "";
                                    }
                                    else
                                    {
                                        oPageDetail.RemoveAll();
                                        moAdXfm.valid = false;
                                        goto ProcessFlow;
                                    }
                                }
                                else
                                {
                                    sAdminLayout = "AdminXForm";
                                }
                                if (mcEwCmd == "Advanced")
                                    goto ProcessFlow;
                                break;
                            }
                        case "UpdatePosition":
                            {
                                if (!string.IsNullOrEmpty(myWeb.moRequest["id"]))
                                {
                                    bool reorder = Conversions.ToBoolean(Interaction.IIf(myWeb.moRequest["reorder"] == "false", false, true));
                                    myWeb.moDbHelper.updatePagePosition(Conversions.ToLong(myWeb.moRequest["pgid"]), Conversions.ToLong(myWeb.moRequest["id"]), myWeb.moRequest["position"]);
                                    // oPageDetail.RemoveAll()
                                    // output nothing ,just called by AJAX
                                    myWeb.ClearPageCache();
                                }

                                break;
                            }
                        case "MoveContent":
                            {

                                if (!string.IsNullOrEmpty(myWeb.moRequest["parId"]))
                                {
                                    if (myWeb.moRequest["id"].Contains(","))
                                    {
                                        string[] ids = myWeb.moRequest["id"].Split(new char[] { ',' });
                                        foreach (var id in ids)
                                            myWeb.moDbHelper.moveContent(Conversions.ToLong(id), Conversions.ToLong(myWeb.moRequest["pgid"]), Conversions.ToLong(myWeb.moRequest["parId"]));
                                    }
                                    else
                                    {
                                        myWeb.moDbHelper.moveContent(Conversions.ToLong(myWeb.moRequest["id"]), Conversions.ToLong(myWeb.moRequest["pgid"]), Conversions.ToLong(myWeb.moRequest["parId"]));
                                    }

                                    // lets show the target page content
                                    myWeb.mnPageId = (int)Conversions.ToLong(myWeb.moRequest["parId"]);
                                    sAdminLayout = "";
                                    mcEwCmd = Conversions.ToString(myWeb.moSession["ewCmd"]);
                                    // lest just try this redirecting to page we moved it to
                                    if (mcEwCmd == "Normal" | mcEwCmd == "NormalMail")
                                    {
                                        myWeb.msRedirectOnEnd = "?ewCmd=" + mcEwCmd + "&pgid=" + myWeb.mnPageId; // myWeb.moSession("lastPage")
                                    }
                                    else if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(myWeb.moSession["lastPage"], "", false)))
                                    {
                                        myWeb.msRedirectOnEnd = Conversions.ToString(myWeb.moSession["lastPage"]);
                                        myWeb.moSession["lastPage"] = "";
                                    }
                                    oPageDetail.RemoveAll();
                                    myWeb.ClearPageCache();
                                }
                                bLoadStructure = true;
                                bMailMenu = true;
                                break;
                            }
                        // Xform Stuff

                        case "LocateContent":
                            {
                                if (!string.IsNullOrEmpty(myWeb.moRequest["submit"]))
                                {
                                    // updateLocations
                                    if (myWeb.moRequest["id"].Contains(","))
                                    {
                                        // Set Multiple Locations
                                        string[] ids = myWeb.moRequest["id"].Split(new char[] { ',' });
                                        foreach (var id in ids)
                                            myWeb.moDbHelper.updateLocations(Conversions.ToLong(id), myWeb.moRequest["location"], myWeb.moRequest["position"]);
                                    }
                                    else
                                    {
                                        myWeb.moDbHelper.updateLocations(Conversions.ToLong(myWeb.moRequest["id"]), myWeb.moRequest["location"], myWeb.moRequest["position"]);
                                    }
                                    sAdminLayout = "";
                                    mcEwCmd = Conversions.ToString(myWeb.moSession["ewCmd"]);
                                    // lest just try this redirecting to last page
                                    if (mcEwCmd == "Normal" | mcEwCmd == "NormalMail")
                                    {
                                        myWeb.msRedirectOnEnd = "?ewCmd=" + mcEwCmd + "&pgid=" + myWeb.mnPageId; // myWeb.moSession("lastPage")
                                    }
                                    bAdminMode = false;
                                    oPageDetail.RemoveAll();
                                    myWeb.mnArtId = 0;
                                    myWeb.ClearPageCache();
                                    if (mcEwCmd != "Normal")
                                    {
                                        goto ProcessFlow;
                                    }
                                }
                                else
                                {
                                    bLoadStructure = true;
                                    XmlElement oContentXml;
                                    if (myWeb.moRequest["id"].Contains(","))
                                    {
                                    }

                                    else
                                    {
                                        oContentXml = (XmlElement)myWeb.GetContentBriefXml(nArtId: Conversions.ToLong(myWeb.moRequest["id"])).SelectSingleNode("/ContentDetail/Content");
                                        myWeb.moContentDetail = (XmlElement)myWeb.moDbHelper.getLocationsByContentId(Conversions.ToLong(myWeb.moRequest["id"]), ref oContentXml);
                                    }
                                    bMailMenu = true;
                                    bSystemPagesMenu = true;

                                }

                                break;
                            }
                        case "LocateContentDetail":
                            {
                                oPageDetail.AppendChild(moAdXfm.xFrmContentLocationDetail(Conversions.ToInteger(myWeb.moRequest["pgid"]), Conversions.ToInteger(myWeb.moRequest["id"])));
                                sAdminLayout = "AdminXForm";
                                bMailMenu = true;
                                bSystemPagesMenu = true;
                                if (moAdXfm.valid)
                                {
                                    sAdminLayout = "";
                                    mcEwCmd = Conversions.ToString(myWeb.moSession["ewCmd"]);
                                    bAdminMode = false;
                                    oPageDetail.RemoveAll();
                                    myWeb.mnArtId = 0;
                                    myWeb.ClearPageCache();
                                }

                                break;
                            }
                        case "ContentVersions":
                            {
                                sAdminLayout = "ContentVersions";
                                XmlElement localgetContentVersions() { var tmp = myWeb.moRequest; long argnContentId = Conversions.ToLong(tmp["id"]); var ret = myWeb.moDbHelper.getContentVersions(ref argnContentId); return ret; }

                                oPageDetail.AppendChild(localgetContentVersions());
                                break;
                            }

                        case "AlertEmail":
                            {
                                sAdminLayout = "AdminXForm";
                                string RecordType = myWeb.moRequest["RecordType"]; // Content
                                long ObjectId = Conversions.ToLong(myWeb.moRequest["id"]); // ContentId
                                string AlertEmailXform = myWeb.moRequest["xFormName"];
                                AlertEmailXform = "/xforms/EmailAlert/" + AlertEmailXform + ".xml";

                                // oPageDetail.AppendChild(moAdXfm.xFrmAlertEmail(RecordType, ObjectId, AlertEmailXform))
                                oPageDetail.AppendChild(moAdXfm.xFrmAlertEmail(RecordType, (int)ObjectId, AlertEmailXform));
                                break;
                            }

                        // xFrmAlertEmail we have an xform file where the contentXml becomes the instance
                        // xform specifies the xslt for the email template
                        // when submitted sends the email
                        // We want to log this activity to the activity log.
                        // We should also show the history of emails from the activity log as part of the form so we do not accidently send twice.

                        // Menu Stuff
                        case "MoveHere":
                            {
                                bLoadStructure = true;
                                myWeb.moDbHelper.moveStructure(Conversions.ToLong(myWeb.moRequest["pgid"]), Conversions.ToLong(myWeb.moRequest["parid"]));
                                sAdminLayout = "EditStructure";
                                mcEwCmd = "MovePage";
                                myWeb.ClearPageCache();
                                break;
                            }


                        case "ContentLocations":
                        case "MovePage":
                            {
                                bLoadStructure = true;
                                break;
                            }

                        case "PageVersions":
                            {

                                sAdminLayout = "PageVersions";
                                XmlElement localgetPageVersions() { var tmp1 = myWeb.moRequest; long argPageId = Conversions.ToLong(tmp1["pgid"]); var ret = myWeb.moDbHelper.getPageVersions(ref argPageId); return ret; }

                                oPageDetail.AppendChild(localgetPageVersions());
                                break;
                            }

                        case "NewPageVersion":
                            {
                                bLoadStructure = true;
                                oPageDetail.AppendChild(moAdXfm.xFrmCopyPageVersion(Conversions.ToLong(myWeb.moRequest["pgid"]), Conversions.ToLong(myWeb.moRequest["vParId"])));
                                if (moAdXfm.valid)
                                {
                                    mcEwCmd = Conversions.ToString(myWeb.moSession["ewCmd"]);
                                    sAdminLayout = "";
                                    oPageDetail.RemoveAll();
                                    myWeb.ClearPageCache();
                                }
                                else
                                {
                                    sAdminLayout = "PageSettings";
                                }

                                break;
                            }

                        case "PageVersionMoveUp":
                        case "PageVersionMoveDown":
                            {
                                // we are sorting the menu nodes
                                string direction = "";
                                switch (mcEwCmd ?? "")
                                {
                                    case "PageVersionMoveUp":
                                        {
                                            direction = "MoveUp";
                                            break;
                                        }
                                    case "PageVersionMoveDown":
                                        {
                                            direction = "MoveDown";
                                            break;
                                        }
                                }
                                myWeb.moDbHelper.ReorderNode(Cms.dbHelper.objectTypes.PageVersion, Conversions.ToLong(myWeb.moRequest["pgid"]), direction);
                                mcEwCmd = "PageVersions";
                                myWeb.ClearPageCache();
                                goto ProcessFlow;

                            }
                        case "CopyPage":
                            {
                                bLoadStructure = true;
                                oPageDetail.AppendChild(moAdXfm.xFrmCopyPage(Conversions.ToLong(myWeb.moRequest["pgid"])));
                                if (moAdXfm.valid)
                                {
                                    mcEwCmd = Conversions.ToString(myWeb.moSession["ewCmd"]);
                                    sAdminLayout = "";
                                    oPageDetail.RemoveAll();
                                    string cUrl = Cms.gcProjectPath + "/?ewCmd=" + mcEwCmd;
                                    myWeb.msRedirectOnEnd = cUrl;
                                    myWeb.ClearPageCache();
                                }
                                else
                                {
                                    sAdminLayout = "PageSettings";
                                }

                                break;
                            }
                        case "EditPage":
                        case "AddPage":
                            {
                                bLoadStructure = true;

                                // TS thinking about how to manage redirects.
                                // We have a problem when going through edit menu then editing metatags you return to structure or normal.

                                // Dim returnCmd As String = CStr(myWeb.moRequest("returnCmd") & "")
                                // If Not myWeb.moSession("lastPage") Is Nothing Then
                                // If returnCmd <> "" And Not (myWeb.moSession("lastPage").contains(returnCmd)) Then
                                // myWeb.moSession("lastPage") = ""
                                // End If
                                // End If

                                oPageDetail.AppendChild(moAdXfm.xFrmEditPage(Conversions.ToLong(myWeb.moRequest["pgid"]), myWeb.moRequest["name"]));
                                if (moAdXfm.valid)
                                {
                                    sAdminLayout = "";
                                    // oPageDetail.RemoveAll()
                                    if (!string.IsNullOrEmpty(myWeb.moRequest["BehaviourAddPageCommand"]))
                                        myWeb.mcBehaviourAddPageCommand = myWeb.moRequest["BehaviourAddPageCommand"];
                                    if (!string.IsNullOrEmpty(myWeb.moRequest["BehaviourEditPageCommand"]))
                                        myWeb.mcBehaviourEditPageCommand = myWeb.moRequest["BehaviourEditPageCommand"];

                                    // Trev's change circa 30 Oct 2009 to set a nominal behaviour for page settings (appears to be return to Page Settings page whatever)
                                    // If either addpage or editpage behaviours have been set then we'll do something else.
                                    if (string.IsNullOrEmpty(myWeb.mcBehaviourAddPageCommand) & string.IsNullOrEmpty(myWeb.mcBehaviourEditPageCommand))
                                    {

                                        // Default behaviour
                                        if (!string.IsNullOrEmpty(myWeb.moRequest["returnCmd"]))
                                        {
                                            var returnPageId = default(int);
                                            if (mcEwCmd == "EditPage" & !string.IsNullOrEmpty(myWeb.moRequest["pgid"]))
                                            {
                                                returnPageId = Conversions.ToInteger(myWeb.moRequest["pgid"]);
                                            }
                                            else if (mcEwCmd == "AddPage" & !string.IsNullOrEmpty(myWeb.moRequest["parid"]))
                                            {
                                                returnPageId = Conversions.ToInteger(myWeb.moRequest["parid"]);
                                            }
                                            if (returnPageId > 0)
                                            {
                                                myWeb.msRedirectOnEnd = "?ewCmd=" + myWeb.moRequest["returnCmd"] + "&pgid=" + returnPageId;
                                            }
                                            else
                                            {
                                                myWeb.msRedirectOnEnd = "?ewCmd=" + myWeb.moRequest["returnCmd"];
                                            }
                                        }
                                        else if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(myWeb.moSession["lastPage"], "", false)))
                                        {
                                            myWeb.msRedirectOnEnd = Conversions.ToString(myWeb.moSession["lastPage"]);
                                            myWeb.moSession["lastPage"] = "";
                                        }
                                        else
                                        {
                                            // Edit page - just do the normal stuff
                                            mcEwCmd = Conversions.ToString(myWeb.moSession["ewCmd"]);
                                            myWeb.mnPageId = Conversions.ToInteger(myWeb.moSession["pgid"]);
                                            oPageDetail.RemoveAll();
                                        }
                                    }

                                    else
                                    {

                                        // Bespoke behaviour - is it edit or add
                                        if (Conversions.ToDouble(myWeb.moRequest["pgid"]) > 0d)
                                        {
                                            mcEwCmd = myWeb.mcBehaviourEditPageCommand;
                                        }
                                        else
                                        {
                                            mcEwCmd = myWeb.mcBehaviourAddPageCommand;
                                        }
                                        XmlNode argoParent = (XmlNode)moAdXfm.Instance;
                                        myWeb.mnPageId = Conversions.ToInteger(getNodeValueByType(ref argoParent, "//nStructKey", XmlDataType.TypeNumber, Conversions.ToLong(myWeb.moSession["pgid"])));
                                        moAdXfm.Instance = (XmlElement)argoParent;

                                        // Check if this page is a cloned page
                                        XmlNode argoParent1 = (XmlNode)moAdXfm.Instance;
                                        long nCloneId = Conversions.ToLong(getNodeValueByType(ref argoParent1, "//nCloneStructId", XmlDataType.TypeNumber, 0));
                                        moAdXfm.Instance = (XmlElement)argoParent1;

                                        // Force a redirect

                                        myWeb.moSession["ewCmd"] = mcEwCmd;
                                        myWeb.moSession["pgid"] = (object)myWeb.mnPageId;

                                        string cUrl = Cms.gcProjectPath + "/?ewCmd=" + mcEwCmd + "&pgid=" + myWeb.mnPageId;
                                        if (nCloneId > 0L)
                                            cUrl += "&context=" + myWeb.mnPageId;
                                        myWeb.msRedirectOnEnd = cUrl;

                                    }
                                    myWeb.ClearPageCache();
                                }
                                else
                                {
                                    // come back here if not going back elsewhere such as EditStucture

                                    if (Convert.ToString(myWeb.moSession["lastPage"]) == "")
                                    {
                                        myWeb.moSession["lastPage"] = myWeb.mcOriginalURL;
                                    }

                                    sAdminLayout = "PageSettings";
                                }

                                break;
                            }
                        case "EditPageLayout":
                            {
                                bLoadStructure = true;
                                oPageDetail.AppendChild(moAdXfm.xFrmEditPageLayout(Conversions.ToLong(myWeb.moRequest["pgid"])));
                                if (moAdXfm.valid)
                                {
                                    mcEwCmd = Conversions.ToString(myWeb.moSession["ewCmd"]);
                                    sAdminLayout = "";
                                    oPageDetail.RemoveAll();
                                    myWeb.ClearPageCache();
                                }
                                else
                                {
                                    sAdminLayout = "AdminXForm";
                                }

                                break;
                            }
                        case "HidePage":
                            {
                                // hide content
                                myWeb.moDbHelper.setObjectStatus(Cms.dbHelper.objectTypes.ContentStructure, Cms.dbHelper.Status.Hidden, Conversions.ToLong(myWeb.moRequest["pgid"]));
                                bLoadStructure = true;
                                mcEwCmd = Conversions.ToString(myWeb.moSession["ewCmd"]);
                                sAdminLayout = "";
                                oPageDetail.RemoveAll();
                                // Redirect to previous page
                                myWeb.mnPageId = Conversions.ToInteger(myWeb.moSession["pgid"]);
                                myWeb.ClearPageCache();
                                break;
                            }
                        case "ShowPage":
                            {
                                // Show Page
                                myWeb.moDbHelper.setObjectStatus(Cms.dbHelper.objectTypes.ContentStructure, Cms.dbHelper.Status.Live, Conversions.ToLong(myWeb.moRequest["pgid"]));
                                bLoadStructure = true;
                                mcEwCmd = Conversions.ToString(myWeb.moSession["ewCmd"]);
                                sAdminLayout = "";
                                oPageDetail.RemoveAll();
                                myWeb.ClearPageCache();
                                // Redirect to previous page
                                myWeb.mnPageId = Conversions.ToInteger(myWeb.moSession["pgid"]);
                                break;
                            }
                        case "DeletePage":
                            {
                                bLoadStructure = true;
                                oPageDetail.AppendChild(moAdXfm.xFrmDeletePage(Conversions.ToLong(myWeb.moRequest["pgid"])));
                                if (moAdXfm.valid)
                                {
                                    myWeb.msRedirectOnEnd = "?ewCmd=EditStructure&pgid=" + mnAdminTopLevel;

                                    // mcEwCmd = myWeb.moSession("ewCmd")
                                    // sAdminLayout = ""
                                    // oPageDetail.RemoveAll()
                                    // 'Redirect to previous page
                                    // myWeb.mnPageId = myWeb.moSession("pgid")
                                    myWeb.ClearPageCache();
                                }
                                else
                                {
                                    sAdminLayout = "AdminXForm";
                                }

                                break;
                            }

                        case "MoveTop":
                        case "MoveUp":
                        case "MoveDown":
                        case "MoveBottom":
                        case "SortAlphaAsc":
                        case "SortAlphaDesc":
                            {
                                bLoadStructure = true;
                                int nGroupId = 0;
                                if (!string.IsNullOrEmpty(myWeb.moRequest["groupid"]))
                                {
                                    nGroupId = Convert.ToInt32(myWeb.moRequest["groupid"]);
                                    if (myWeb.moRequest["lastPage"] != null)
                                    {
                                        if (myWeb.moRequest["lastPage"] == "ProductGroups")
                                        {
                                            myWeb.moSession["lastPage"] = "?ewCmd=ProductGroups&GrpID=" + nGroupId;
                                        }
                                    }
                                }
                                if (!string.IsNullOrEmpty(myWeb.moRequest["id"]) & string.IsNullOrEmpty(myWeb.moRequest["relId"]))
                                {
                                    // we are sorting content on a page  
                                    myWeb.moDbHelper.ReorderContent(Conversions.ToLong(myWeb.moRequest["pgid"]), Conversions.ToLong(myWeb.moRequest["id"]), myWeb.moRequest["ewCmd"], cPosition: myWeb.moRequest["position"], nGroupId: nGroupId);
                                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(myWeb.moSession["lastPage"], "", false)))
                                    {
                                        myWeb.msRedirectOnEnd = Conversions.ToString(myWeb.moSession["lastPage"]);
                                        myWeb.moSession["lastPage"] = "";
                                    }
                                    else
                                    {
                                        // Edit page - just do the normal stuff
                                        mcEwCmd = "Normal";
                                        bAdminMode = false;
                                        sAdminLayout = "";
                                        oPageDetail.RemoveAll();
                                        myWeb.mnArtId = 0;
                                    }
                                }

                                else if (!string.IsNullOrEmpty(myWeb.moRequest["relId"]))
                                {
                                    // sorting Related Content for an item
                                    myWeb.moDbHelper.ReorderContent(Conversions.ToLong(myWeb.moRequest["relId"]), Conversions.ToLong(myWeb.moRequest["id"]), myWeb.moRequest["ewCmd"], true, "", nGroupId);
                                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(myWeb.moSession["lastPage"], "", false)))
                                    {
                                        myWeb.msRedirectOnEnd = Conversions.ToString(myWeb.moSession["lastPage"]);
                                        myWeb.moSession["lastPage"] = "";
                                    }
                                    else
                                    {
                                        // Edit page - just do the normal stuff
                                        mcEwCmd = "Normal";
                                        bAdminMode = false;
                                        sAdminLayout = "";
                                        oPageDetail.RemoveAll();
                                        myWeb.mnArtId = 0;
                                    }
                                }
                                else
                                {
                                    // we are sorting the menu nodes
                                    myWeb.moDbHelper.ReorderNode(Cms.dbHelper.objectTypes.ContentStructure, Conversions.ToLong(myWeb.moRequest["pgid"]), myWeb.moRequest["ewCmd"]);
                                    mcEwCmd = Conversions.ToString(myWeb.moSession["ewCmd"]);
                                    if (mcEwCmd == "EditStructure")
                                    {
                                        sAdminLayout = "EditStructure";
                                        oPageDetail.RemoveAll();
                                    }
                                    else
                                    {
                                        mcEwCmd = "Normal";
                                        bAdminMode = false;
                                        sAdminLayout = "";
                                        oPageDetail.RemoveAll();
                                        myWeb.mnArtId = 0;
                                    }
                                    // Redirect to previous page
                                    myWeb.mnPageId = Conversions.ToInteger(myWeb.moSession["pgid"]);
                                }
                                myWeb.ClearPageCache();
                                break;
                            }
                        case "ImageLib":
                            {
                                this.LibProcess(ref oPageDetail, ref sAdminLayout, Protean.fsHelper.LibraryType.Image);
                                break;
                            }
                        case "DocsLib":
                            {
                                this.LibProcess(ref oPageDetail, ref sAdminLayout, Protean.fsHelper.LibraryType.Documents);
                                break;
                            }
                        case "MediaLib":
                            {
                                this.LibProcess(ref oPageDetail, ref sAdminLayout, Protean.fsHelper.LibraryType.Media);
                                break;
                            }
                        case "ListUsers":
                            {
                                if (Information.IsNumeric(myWeb.moRequest["parid"]))
                                {
                                    myWeb.moSession["UserParId"] = myWeb.moRequest["parid"];
                                }
                                else if (myWeb.moRequest["ewCmd"] == "ListUsers")
                                {
                                    myWeb.moSession["UserParId"] = 0;
                                }
                                int nStatus = 99;
                                if (!string.IsNullOrEmpty(myWeb.moRequest["status"]))
                                {
                                    nStatus = Conversions.ToInteger(myWeb.moRequest["status"]);
                                }
                                oPageDetail.AppendChild(myWeb.moDbHelper.listDirectory("User", (long)Conversions.ToInteger(Operators.ConcatenateObject("0", myWeb.moSession["UserParId"])), nStatus));
                                sAdminLayout = "ListDirectory"; // "ListUsers"
                                myWeb.moSession["ewCmd"] = mcEwCmd;
                                break;
                            }
                        case "Profile":
                            {
                                oPageDetail.AppendChild(myWeb.moDbHelper.GetUserXML(Conversions.ToLong(myWeb.moRequest["id"]), true));
                                var oCart = new Cms.Cart(ref myWeb);
                                moPageXML.DocumentElement.AppendChild(oPageDetail);
                                oCart.moPageXml = moPageXML;
                                XmlElement argoPageDetail = null;
                                oCart.ListOrders(0.ToString(), false, 0, oPageDetail: ref argoPageDetail, false, Conversions.ToLong(myWeb.moRequest["id"]));
                                oCart = (Cms.Cart)null;
                                break;
                            }

                        case "ListUserContacts":
                        case "ListContacts":
                        case "ListDirContacts":
                            {

                                sAdminLayout = "Profile";
                                oPageDetail.AppendChild(myWeb.moDbHelper.GetUserXML((long)Conversions.ToInteger("0" + myWeb.moRequest["parid"]), true));
                                break;
                            }

                        case "EditContact":
                            {

                                sAdminLayout = "EditUserContact";
                                oPageDetail.AppendChild(moAdXfm.xFrmEditDirectoryContact((long)Conversions.ToInteger("0" + myWeb.moRequest["id"]), Conversions.ToInteger("0" + myWeb.moRequest["parid"])));
                                if (moAdXfm.valid)
                                {
                                    oPageDetail.RemoveAll();
                                    mcEwCmd = "ListUserContacts";
                                    goto ProcessFlow;
                                }

                                break;
                            }
                        case "RefundOrder":
                            {
                                sAdminLayout = "RefundOrder";
                                string providerName = "";
                                string providerPaymentReference = "";
                                //string IsRefund = "";
                                long nStatus;
                                var oCart = new Cms.Cart(ref myWeb);
                                oCart.moPageXml = moPageXML;
                                string orderid = myWeb.moRequest["orderId"];
                                string sql = "select cpayMthdProviderName, cPayMthdProviderRef from tblCartPaymentMethod INNER JOIN tblCartOrder ON nPayMthdId = nPayMthdKey where nCartOrderkey=" + myWeb.moRequest["id"];
                                using (var oDr = myWeb.moDbHelper.getDataReaderDisposable(sql))  // Done by nita on 6/7/22
                                {
                                    while (oDr.Read())
                                    {
                                        providerName = oDr.GetString(0);
                                        providerPaymentReference = oDr.GetString(1);
                                    }
                                }
                                oPageDetail.AppendChild(moAdXfm.xFrmRefundOrder((long)Conversions.ToInteger("0" + myWeb.moRequest["id"]), providerName, providerPaymentReference));
                                if (moAdXfm.valid)
                                {
                                    string sSql = "select nCartStatus from tblCartOrder WHERE nCartOrderKey =" + myWeb.moRequest["id"];
                                    nStatus = Conversions.ToLong(myWeb.moDbHelper.ExeProcessSqlScalar(sSql));
                                    nStatus = (long)Cms.Cart.cartProcess.Refunded;
                                    if (Conversions.ToInteger(orderid) > 0)
                                    {
                                        string sSqlquery = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("update tblCartOrder set nCartStatus ='" + nStatus + "', cCartSessionId='", stdTools.SqlFmt(myWeb.moSession.SessionID)), "'  where nCartOrderKey = "), orderid));
                                        myWeb.moDbHelper.ExeProcessSql(sSqlquery);
                                    }

                                    oPageDetail.RemoveAll();
                                    mcEwCmd = "OrderDetail";
                                    myWeb.msRedirectOnEnd = "?ewCmd=Orders&ewCmd2=Display&id=" + myWeb.moRequest["id"];
                                }
                                moPageXML.DocumentElement.AppendChild(oPageDetail);
                                break;
                            }

                        // `get the payment mothod id for this order
                        // `from the paymentmethod we get the provider name And the card reference
                        // `we show a New form populating the refnd amount And showing the provider name And referance
                        // `on submitting the form we process using the provider name

                        case "EditUserContact":
                            {

                                sAdminLayout = "EditUserContact";
                                oPageDetail.AppendChild(moAdXfm.xFrmEditDirectoryContact((long)Conversions.ToInteger("0" + myWeb.moRequest["parid"]), Conversions.ToInteger("0" + myWeb.moRequest["id"])));
                                if (moAdXfm.valid)
                                {
                                    oPageDetail.RemoveAll();
                                    mcEwCmd = "ListUserContacts";
                                    myWeb.msRedirectOnEnd = "/?ewCmd=Profile&DirType=Company&id=" + myWeb.moRequest["id"];
                                    goto ProcessFlow;
                                }

                                break;
                            }
                        case "EditOrderContact":
                            {

                                sAdminLayout = "EditUserContact";
                                object sSql = "Select nContactKey from tblCartContact where cContactType = " + $"'{myWeb.moRequest["contacttype"]} Address' and nContactCartid=" + myWeb.moRequest["orderid"];
                                string sContactKey = myWeb.moDbHelper.ExeProcessSqlScalar(Conversions.ToString(sSql));

                                oPageDetail.AppendChild(moAdXfm.xFrmEditDirectoryContact((long)Conversions.ToInteger("0" + sContactKey)));
                                if (moAdXfm.valid)
                                {
                                    oPageDetail.RemoveAll();
                                    mcEwCmd = "Orders";
                                    myWeb.msRedirectOnEnd = "/?ewCmd=Orders&ewCmd2=Display&id=" + myWeb.moRequest["orderid"];
                                    goto ProcessFlow;
                                }

                                break;
                            }
                        case "AddUserContact":
                            {

                                sAdminLayout = mcEwCmd;
                                oPageDetail.AppendChild(moAdXfm.xFrmEditDirectoryContact((long)Conversions.ToInteger("0" + myWeb.moRequest["parid"]), Conversions.ToInteger("0" + myWeb.moRequest["id"])));
                                if (moAdXfm.valid)
                                {
                                    oPageDetail.RemoveAll();
                                    mcEwCmd = "ListUserContacts";
                                    myWeb.msRedirectOnEnd = "/?ewCmd=Profile&DirType=Company&id=" + myWeb.moRequest["id"];
                                    goto ProcessFlow;
                                }

                                break;
                            }

                        case "AddContact":
                        case "AddDirContact":
                            {

                                sAdminLayout = mcEwCmd;
                                oPageDetail.AppendChild(moAdXfm.xFrmEditDirectoryContact((long)Conversions.ToInteger("0" + myWeb.moRequest["id"]), Conversions.ToInteger("0" + myWeb.moRequest["parid"])));
                                if (moAdXfm.valid)
                                {
                                    oPageDetail.RemoveAll();
                                    mcEwCmd = "ListUserContacts";
                                    goto ProcessFlow;
                                }

                                break;
                            }

                        case "DeleteUserContact":
                            {

                                myWeb.moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.CartContact, (long)Conversions.ToInteger("0" + myWeb.moRequest["parid"]));
                                mcEwCmd = "ListUserContacts";
                                myWeb.msRedirectOnEnd = "/?ewCmd=Profile&DirType=Company&id=" + myWeb.moRequest["id"];
                                goto ProcessFlow;
                            }
                        case "DeleteContact":
                            {

                                myWeb.moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.CartContact, (long)Conversions.ToInteger("0" + myWeb.moRequest["id"]));
                                mcEwCmd = "ListUserContacts";
                                goto ProcessFlow;
                            }
                        case "ListCompanies":
                            {
                                oPageDetail.AppendChild(myWeb.moDbHelper.listDirectory("Company", (long)Conversions.ToInteger("0" + nParId)));

                                sAdminLayout = "ListDirectory"; // "ListCompanies"
                                myWeb.moSession["ewCmd"] = mcEwCmd;
                                myWeb.moSession["nParId"] = (object)nParId;
                                break;
                            }

                        case "ListDepartments":
                            {
                                if (Conversions.ToDouble(myWeb.moRequest["parid"]) > 0d)
                                {
                                    myWeb.moSession["DeptParId"] = myWeb.moRequest["parid"];
                                }
                                else if (myWeb.moRequest["ewCmd"] == "ListUsers")
                                {
                                    myWeb.moSession["DeptParId"] = (object)0;
                                }
                                oPageDetail.AppendChild(myWeb.moDbHelper.listDirectory("Department", (long)Conversions.ToInteger(Operators.ConcatenateObject("0", myWeb.moSession["DeptParId"]))));
                                sAdminLayout = "ListDirectory"; // "ListDepartments"
                                myWeb.moSession["ewCmd"] = mcEwCmd;
                                break;
                            }

                        case "ListGroups":
                            {
                                oPageDetail.AppendChild(myWeb.moDbHelper.listDirectory("Group", (long)Conversions.ToInteger("0" + myWeb.moRequest["parid"])));
                                sAdminLayout = "ListDirectory"; // "ListGroups"
                                myWeb.moSession["ewCmd"] = mcEwCmd;
                                break;
                            }

                        case "ListRoles":
                            {
                                oPageDetail.AppendChild(myWeb.moDbHelper.listDirectory("Role"));
                                sAdminLayout = "ListDirectory"; // "ListRoles"
                                myWeb.moSession["ewCmd"] = mcEwCmd;
                                break;
                            }

                        case "ListDirectory":
                            {
                                if (mcEwCmd2 != null)
                                {
                                    oPageDetail.AppendChild(myWeb.moDbHelper.listDirectory(mcEwCmd2, (long)Conversions.ToInteger("0" + myWeb.moRequest["parid"])));
                                }

                                sAdminLayout = "ListDirectory"; // "ListRoles"
                                myWeb.moSession["ewCmd"] = mcEwCmd;
                                myWeb.moSession["ewCmd2"] = mcEwCmd2;
                                break;
                            }

                        case "CopyGroupMembers":
                            {
                                sAdminLayout = "AdminXForm";
                                oPageDetail.AppendChild(moAdXfm.xFrmCopyGroupMembers(Convert.ToInt64(myWeb.moRequest["id"])));
                                if (moAdXfm.valid)
                                {
                                    oPageDetail.RemoveAll();
                                    mcEwCmd = "ListGroups";
                                    goto ProcessFlow;
                                }

                                break;
                            }

                        case "EditDirItem":
                            {

                                Protean.Providers.Membership.ReturnProvider RetProv = new Protean.Providers.Membership.ReturnProvider();
                                IMembershipProvider oMembershipProv = RetProv.Get(ref myWeb, myWeb.moConfig["MembershipProvider"]);

                                // oPageDetail.AppendChild(oMembershipProv.AdminXforms.xFrmUserLogon("AdminLogon"))
                                XmlElement xmlName = null;
                                oPageDetail.AppendChild((XmlNode)oMembershipProv.AdminXforms.xFrmEditDirectoryItem(ref xmlName, Convert.ToInt64(myWeb.moRequest["id"]), myWeb.moRequest["dirType"], Convert.ToInt64("0" + myWeb.moRequest["parId"])));
                                if (Conversions.ToBoolean(oMembershipProv.AdminXforms.valid))
                                {
                                    oPageDetail.RemoveAll();

                                    // clear the listDirectory cache
                                    myWeb.moDbHelper.clearDirectoryCache();

                                    // return to process flow
                                    mcEwCmd = Conversions.ToString(myWeb.moSession["ewCmd"]);
                                    mcEwCmd2 = Conversions.ToString(myWeb.moSession["ewCmd2"]);
                                    // Select Case myWeb.moRequest("dirType")
                                    // Case "User"
                                    // myWeb.msRedirectOnEnd = "/?ewCmd=ListCompanies"
                                    // Case "Group"
                                    // myWeb.msRedirectOnEnd = "/?ewCmd=ListGroups"
                                    // Case "Group"
                                    // myWeb.msRedirectOnEnd = "/?ewCmd=ListGroups"
                                    // Case Else
                                    // myWeb.msRedirectOnEnd = "/?ewCmd=ListCompanies"
                                    // End Select
                                    goto ProcessFlow;
                                }

                                else
                                {
                                    sAdminLayout = "AdminXForm";
                                }

                                break;
                            }

                        case "RegradeUser":
                            {
                                sAdminLayout = "AdminXForm";
                                string xformPath = "/xforms/directory/regradeuser.xml";
                                oPageDetail.AppendChild(moAdXfm.xFrmRegradeUser(Conversions.ToInteger(myWeb.moRequest["id"]), Conversions.ToLong(myWeb.moRequest["existingGroupId"]), myWeb.moRequest["newGroupId"], "Regrade User", xformPath, myWeb.moRequest["messageId"]));
                                if (moAdXfm.valid)
                                {
                                    oPageDetail.RemoveAll();
                                    mcEwCmd = "ListUsers";
                                    goto ProcessFlow;
                                }

                                break;
                            }

                        case "EditRole":
                            {
                                // replaces admin menu with one with full permissions
                                GetAdminMenu();
                                oPageDetail.AppendChild(moAdXfm.xFrmEditRole(Conversions.ToLong(myWeb.moRequest["id"])));
                                if (moAdXfm.valid)
                                {
                                    oPageDetail.RemoveAll();

                                    // clear the listDirectory cache
                                    myWeb.moDbHelper.clearDirectoryCache();

                                    // return to process flow
                                    mcEwCmd = Conversions.ToString(myWeb.moSession["ewCmd"]);
                                    goto ProcessFlow;
                                }

                                else
                                {
                                    sAdminLayout = "AdminXForm";
                                }

                                break;
                            }

                        case "ImpersonateUser":
                            {

                                // myWeb.moSession("ewAuth") = ""
                                // myWeb.moSession("adminMode") = ""
                                // myWeb.moSession("ewCmd") = ""

                                myWeb.moSession.RemoveAll();

                                var oMem = new Cms.Membership(ref myWeb);
                                if (!string.IsNullOrEmpty(myWeb.moConfig["SecureMembershipAddress"]))
                                {
                                    oMem.SecureMembershipProcess("logoffImpersonate");
                                    mnAdminUserId = Conversions.ToInteger(myWeb.moRequest["id"]);
                                    myWeb.msRedirectOnEnd = myWeb.moConfig["SecureMembershipAddress"] + Cms.gcProjectPath + "/";
                                }
                                else
                                {
                                    mnAdminUserId = Conversions.ToInteger(myWeb.moRequest["id"]);
                                    myWeb.msRedirectOnEnd = Cms.gcProjectPath + "/";
                                }

                                break;
                            }

                        case "ResetUserAcct":
                            {
                                Protean.Providers.Membership.ReturnProvider RetProv = new Protean.Providers.Membership.ReturnProvider();
                                IMembershipProvider oMembershipProv = RetProv.Get(ref myWeb, myWeb.moConfig["MembershipProvider"]);


                                oPageDetail.AppendChild((XmlNode)oMembershipProv.AdminXforms.xFrmResetAccount(Convert.ToInt64(myWeb.moRequest["id"])));

                                if (moAdXfm.valid)
                                {
                                    oPageDetail.RemoveAll();
                                    // clear the listDirectory cache
                                    myWeb.moDbHelper.clearDirectoryCache();
                                    // return to process flow
                                    mcEwCmd = Conversions.ToString(myWeb.moSession["ewCmd"]);
                                    goto ProcessFlow;
                                }
                                else
                                {
                                    sAdminLayout = "AdminXForm";
                                }

                                break;
                            }

                        case "ResetUserPwd":
                            {
                                Protean.Providers.Membership.ReturnProvider RetProv = new Protean.Providers.Membership.ReturnProvider();
                                IMembershipProvider oMembershipProv = RetProv.Get(ref myWeb, myWeb.moConfig["MembershipProvider"]);


                                oPageDetail.AppendChild((XmlNode)oMembershipProv.AdminXforms.xFrmResetPassword(Convert.ToInt64(myWeb.moRequest["id"])));

                                if (moAdXfm.valid)
                                {
                                    oPageDetail.RemoveAll();
                                    // clear the listDirectory cache
                                    myWeb.moDbHelper.clearDirectoryCache();
                                    // return to process flow
                                    mcEwCmd = Conversions.ToString(myWeb.moSession["ewCmd"]);
                                    goto ProcessFlow;
                                }
                                else
                                {
                                    sAdminLayout = "AdminXForm";
                                }

                                break;
                            }

                        case "UserIntegrations":
                            {
                                oPageDetail.AppendChild(moAdXfm.xFrmUserIntegrations(Conversions.ToLong(myWeb.moRequest["dirId"]), mcEwCmd2));
                                if (moAdXfm.valid)
                                {
                                    oPageDetail.RemoveAll();
                                    // clear the listDirectory cache
                                    myWeb.moDbHelper.clearDirectoryCache();
                                    // return to process flow
                                    mcEwCmd = Conversions.ToString(myWeb.moSession["ewCmd"]);
                                    goto ProcessFlow;
                                }
                                else
                                {
                                    sAdminLayout = "AdminXForm";
                                }

                                break;
                            }

                        case "HideDirItem":
                            {
                                myWeb.moDbHelper.setObjectStatus(Cms.dbHelper.objectTypes.Directory, Cms.dbHelper.Status.Hidden, Conversions.ToLong(myWeb.moRequest["id"]));
                                mcEwCmd = Conversions.ToString(myWeb.moSession["ewCmd"]);
                                goto ProcessFlow;
                            }
                        case "DeleteDirItem":
                            {
                                if (Conversions.ToDouble(myWeb.moRequest["id"]) > 2d)
                                {
                                    oPageDetail.AppendChild(moAdXfm.xFrmDeleteDirectoryItem(Conversions.ToLong(myWeb.moRequest["id"]), myWeb.moRequest["dirType"]));
                                    if (moAdXfm.valid)
                                    {
                                        oPageDetail.RemoveAll();

                                        // clear the listDirectory cache
                                        myWeb.moDbHelper.clearDirectoryCache();

                                        // return to process flow
                                        mcEwCmd = Conversions.ToString(myWeb.moSession["ewCmd"]);
                                        goto ProcessFlow;
                                    }

                                    else
                                    {
                                        sAdminLayout = "AdminXForm";
                                    }
                                }

                                break;
                            }
                        case "RemoveDuplicateDirRelations":
                            {
                                myWeb.moDbHelper.RemoveDuplicateDirRelations();
                                break;
                            }
                        case "MaintainRelations":
                            {
                                oPageDetail.AppendChild(myWeb.moDbHelper.listDirRelations(Conversions.ToLong(myWeb.moRequest["id"]), myWeb.moRequest["type"], Conversions.ToLong("0" + myWeb.moRequest["parId"])));
                                sAdminLayout = "ListDirRelations";
                                break;
                            }

                        case "SaveDirectoryRelations":
                            {
                                myWeb.moDbHelper.saveDirectoryRelations();

                                // return to process flow
                                mcEwCmd = Conversions.ToString(myWeb.moSession["ewCmd"]);
                                bResetParId = true;

                                goto ProcessFlow;

                            }

                        case "Permissions":
                            {
                                sAdminLayout = "EditStructurePermissions";
                                bLoadStructure = true;
                                break;
                            }

                        case "DirPermissions":
                            {
                                if (!string.IsNullOrEmpty(myWeb.moRequest["submit"]))
                                {
                                    myWeb.moDbHelper.saveDirectoryPermissions();
                                }
                                // impersonate User / Group
                                oPageDetail.AppendChild(oWeb.GetUserXML(Conversions.ToLong(myWeb.moRequest["parid"])));
                                oPageDetail.AppendChild(oWeb.GetStructureXML(Conversions.ToLong(myWeb.moRequest["parid"]), (long)mnAdminTopLevel, 0L, "", false, false, false, true, false, "", ""));
                                sAdminLayout = "EditDirectoryItemPermissions";
                                break;
                            }

                        case "DirMemberships":
                            {
                                // Add specified child members to a directory entity.
                                oPageDetail.AppendChild(moAdXfm.xFrmDirMemberships(myWeb.moRequest["type"], Conversions.ToLong(myWeb.moRequest["id"]), Conversions.ToLong(myWeb.moRequest["parId"]), myWeb.moRequest["childTypes"]));
                                if (moAdXfm.valid)
                                {
                                    oPageDetail.RemoveAll();
                                    // return to process flow
                                    mcEwCmd = Conversions.ToString(myWeb.moSession["ewCmd"]);
                                    goto ProcessFlow;
                                }
                                else
                                {
                                    sAdminLayout = "AdminXForm";
                                }

                                break;
                            }

                        case "EditPagePermissions":
                            {
                                bLoadStructure = true;
                                oPageDetail.AppendChild(moAdXfm.xFrmPagePermissions(Conversions.ToLong(myWeb.moRequest["pgid"])));
                                myWeb.ClearPageCache();
                                sAdminLayout = "AdminXForm";
                                break;
                            }

                        case "EditPageRights":
                            {
                                bLoadStructure = true;
                                oPageDetail.AppendChild(moAdXfm.xFrmPageRights(Conversions.ToLong(myWeb.moRequest["pgid"])));
                                sAdminLayout = "AdminXForm";
                                break;
                            }
                        // ++++++++++++++++++++++++++ ECOMMERCE ++++++++++++++++++++++++++++++++
                        case "Ecommerce":
                            {
                                break;
                            }
                        // placeholder for ecommerce dashboard
                        case "CartActivity":
                        case "CartReports":
                        case "CartActivityDrilldown":
                        case "CartActivityPeriod":
                        case "CartDownload":
                            {
                                OrderProcess(ref oPageDetail, ref sAdminLayout, "");
                                break;
                            }
                        case "Orders":
                        case "OrdersShipped":
                        case "OrdersFailed":
                        case "OrdersDeposit":
                        case "OrdersRefunded":
                        case "OrdersHistory":
                        case "OrdersAwaitingPayment":
                        case "OrdersSaved":
                        case "OrdersInProgress":
                        case "BulkCartAction":
                            {

                                OrderProcess(ref oPageDetail, ref sAdminLayout, "Order");
                                break;
                            }
                        case "EventBookings":
                            {
                                EventBookingProcess(ref oPageDetail, ref sAdminLayout);
                                break;
                            }


                        case "Quotes":
                        case "QuotesFailed":
                        case "QuotesDeposit":
                        case "QuotesHistory":
                            {
                                OrderProcess(ref oPageDetail, ref sAdminLayout, "Quote");
                                break;
                            }

                        case "ShippingLocations":
                            {
                                ShippingLocationsProcess(ref oPageDetail, ref sAdminLayout);
                                break;
                            }

                        case "DeliveryMethods":
                            {
                                DeliveryMethodProcess(ref oPageDetail, ref sAdminLayout);
                                break;
                            }

                        case "PaymentProviders":
                            {
                                PaymentProviderProcess(ref oPageDetail, ref sAdminLayout);
                                break;
                            }

                        case "Carriers":
                            {
                                CarriersProcess(ref oPageDetail, ref sAdminLayout);
                                break;
                            }

                        case "CartSettings":
                            {

                                oPageDetail.AppendChild(moAdXfm.xFrmCartSettings());
                                if (moAdXfm.valid)
                                {
                                    mcEwCmd = "Normal";
                                }
                                else
                                {
                                    sAdminLayout = "AdminXForm";
                                }

                                break;
                            }
                        case "SalesReports":
                            {
                                break;
                            }



                        case "EditStructure":
                        case "QuizReports":
                        case "ListQuizes":
                            {
                                bLoadStructure = true;
                                myWeb.moSession["lastPage"] = myWeb.mcOriginalURL;
                                break;
                            }
                        // do nothing

                        case var case2 when case2 == "QuizReports":
                        case var case3 when case3 == "ListQuizes":
                            {
                                // This should be moved to EonicLMS
                                bLoadStructure = true;
                                break;
                            }

                        case "ManagePollVotes":
                            {
                                PollsProcess(ref oPageDetail, ref sAdminLayout);
                                break;
                            }

                        case "ManageLookups":
                            {

                                ManageLookups(ref oPageDetail, ref sAdminLayout);
                                break;
                            }




                        // if the command for turning on the preview mode is sent then
                        // check if we are in admin mode, if so, turn preview on
                        case "PreviewOn":
                            {
                                sAdminLayout = "";
                                mbPreviewMode = true;
                                if (myWeb.moSession["PreviewDate"] is null)
                                {
                                    myWeb.moSession["PreviewDate"] = DateTime.Now.Date;
                                }

                                // ensure if no preview user is specified we are anonomous
                                if (string.IsNullOrEmpty(Conversions.ToString(Operators.ConcatenateObject("", myWeb.moSession["PreviewUser"]))) & Conversions.ToInteger("0" + myWeb.moRequest["PreviewUser"]) == 0)
                                {
                                    myWeb.moSession["PreviewUser"] = 0;
                                }

                                if (Strings.LCase(myWeb.moRequest["ewCmd"]) == "logoff")
                                {
                                    myWeb.moSession["PreviewUser"] = 0;
                                    myWeb.msRedirectOnEnd = "/";
                                }

                                if (Information.IsDate(myWeb.moRequest["dPreviewDate"]))
                                {
                                    myWeb.moSession["PreviewDate"] = (object)Conversions.ToDate(myWeb.moRequest["dPreviewDate"]);
                                }
                                myWeb.mdDate = Conversions.ToDate(myWeb.moSession["PreviewDate"]);

                                if (myWeb.moRequest["ewCmd2"] == "showHidden")
                                {
                                    myWeb.moSession["mbPreviewHidden"] = (object)true;
                                }
                                if (myWeb.moRequest["ewCmd2"] == "hideHidden")
                                {
                                    myWeb.moSession["mbPreviewHidden"] = (object)false;
                                }
                                myWeb.mbPreviewHidden = Conversions.ToBoolean(myWeb.moSession["mbPreviewHidden"]);

                                if (Conversions.ToInteger("0" + myWeb.moRequest["PreviewUser"]) > 0)
                                {
                                    myWeb.moSession["PreviewUser"] = (object)Conversions.ToInteger("0" + myWeb.moRequest["PreviewUser"]);
                                }
                                myWeb.mnUserId = Conversions.ToInteger(myWeb.moSession["PreviewUser"]);

                                if (Conversions.ToInteger("0" + myWeb.moRequest["CartId"]) > 0)
                                {
                                    myWeb.moSession["CartId"] = myWeb.moRequest["CartId"];

                                    // reset cart processId
                                    string sSql = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("update tblCartOrder set nCartStatus = 5, cCartSessionId='", stdTools.SqlFmt(myWeb.moSession.SessionID)), "'  where nCartOrderKey = "), myWeb.moRequest["CartId"]));
                                    myWeb.moDbHelper.ExeProcessSql(sSql);


                                }

                                break;
                            }

                        case "RelateSearch":
                            {
                                if (stdTools.ButtonSubmitted(ref myWeb.moRequest, "saveRelated"))
                                {
                                    // code for saving results of 2nd form submission
                                    myWeb.moDbHelper.saveContentRelations();
                                    // redirect to the parent content xform
                                    myWeb.moSession["ewCmd"] = "";
                                    if (myWeb.moRequest["redirect"] == "normal")
                                    {
                                        if (mcEwCmd == "Normal" | mcEwCmd == "NormalMail")
                                        {
                                            myWeb.msRedirectOnEnd = "?ewCmd=" + mcEwCmd + "&pgid=" + myWeb.mnPageId; // myWeb.moSession("lastPage")
                                        }
                                        else
                                        {
                                            myWeb.msRedirectOnEnd = Conversions.ToString(myWeb.moSession["lastPage"]);
                                        }
                                    }
                                    else
                                    {
                                        myWeb.msRedirectOnEnd = Conversions.ToString(Operators.ConcatenateObject(myWeb.moRequest.QueryString["Path"] + "?ewCmd=EditContent&id=" + myWeb.moRequest.Form.Get("id"), Interaction.IIf(string.IsNullOrEmpty(myWeb.moRequest.QueryString["pgid"]), "", "&pgid=" + myWeb.moRequest.QueryString["pgid"])));
                                    }
                                }
                                else
                                {
                                    // Process for related content
                                    long nRelParent = Conversions.ToLong("0" + myWeb.moRequest["RelParent"]);
                                    string redirect = "";
                                    if (nRelParent == 0L)
                                    {
                                        nRelParent = Conversions.ToLong(Operators.ConcatenateObject("0", myWeb.moSession["mcRelParent"]));
                                    }
                                    else
                                    {
                                        redirect = "normal";
                                    }
                                    oPageDetail.AppendChild(moAdXfm.xFrmFindRelated(nRelParent.ToString(), myWeb.moRequest.QueryString["type"], ref oPageDetail, "nParentContentId", false, "tblcontentRelation", "nContentChildId", "nContentParentId", redirect));
                                    sAdminLayout = "RelatedSearch";
                                }
                                if (moAdXfm.valid)
                                {

                                }

                                break;
                            }

                        // New case for SKU Parent Change functionality
                        case "ParentChange":
                            {
                                if (stdTools.ButtonSubmitted(ref myWeb.moRequest, "updateParent"))
                                {
                                    // code for saving results of 2nd form submission
                                    // get all id's from request
                                    long oldParentID = Conversions.ToLong(myWeb.moRequest.QueryString["oldParentID"]);
                                    long childId = Conversions.ToLong(myWeb.moRequest.QueryString["childId"]);
                                    long newParentID = Conversions.ToLong(myWeb.moRequest.QueryString["newParId"]);
                                    myWeb.moDbHelper.ChangeParentRelation(oldParentID, newParentID, childId);

                                    // redirect to the parent content xform
                                    myWeb.moSession["ewCmd"] = "";
                                    if (myWeb.moRequest["redirect"] == "normal")
                                    {
                                        if (mcEwCmd == "Normal" | mcEwCmd == "NormalMail")
                                        {
                                            myWeb.msRedirectOnEnd = "?ewCmd=" + mcEwCmd + "&pgid=" + myWeb.mnPageId; // myWeb.moSession("lastPage")
                                        }
                                        else
                                        {
                                            myWeb.msRedirectOnEnd = Conversions.ToString(myWeb.moSession["lastPage"]);
                                        }
                                    }
                                    else
                                    {
                                        myWeb.msRedirectOnEnd = Conversions.ToString(Operators.ConcatenateObject(myWeb.moRequest.QueryString["Path"] + "?ewCmd=EditContent&id=" + myWeb.moRequest.Form.Get("id"), Interaction.IIf(string.IsNullOrEmpty(myWeb.moRequest.QueryString["pgid"]), "", "&pgid=" + myWeb.moRequest.QueryString["pgid"])));
                                    }
                                }
                                else
                                {
                                    // Process for related content
                                    long nRelParent = Conversions.ToLong("0" + myWeb.moRequest["RelParent"]);
                                    string redirect = "";
                                    if (nRelParent == 0L)
                                    {
                                        nRelParent = Conversions.ToLong(Operators.ConcatenateObject("0", myWeb.moSession["mcRelParent"]));
                                    }
                                    else
                                    {
                                        redirect = "normal";
                                    }
                                    oPageDetail.AppendChild(moAdXfm.xFrmFindParent(nRelParent.ToString(), myWeb.moRequest.QueryString["childId"], myWeb.moRequest.QueryString["type"], ref oPageDetail, "nParentContentId", false, "tblcontentRelation", "nContentChildId", "nContentParentId", redirect));
                                    sAdminLayout = "ParentChange";
                                }
                                if (moAdXfm.valid)
                                {

                                }

                                break;
                            }

                        case "LocateSearch":
                            {
                                // Process for related content
                                bLoadStructure = true;
                                moAdXfm.xFrmFindContentToLocate(myWeb.moRequest["pgid"], myWeb.moRequest["nFromPage"], myWeb.moRequest["bIncludeChildren"], myWeb.moRequest["type"], myWeb.moRequest["cSearchTerm"], ref oPageDetail);
                                sAdminLayout = "LocateSearch";
                                if (moAdXfm.valid)
                                {
                                    sAdminLayout = "";
                                    oPageDetail.RemoveAll();
                                    if (mcEwCmd == "Normal" | mcEwCmd == "NormalMail")
                                    {
                                        myWeb.msRedirectOnEnd = "?ewCmd=" + mcEwCmd + "&pgid=" + myWeb.mnPageId; // myWeb.moSession("lastPage")
                                    }
                                    else
                                    {
                                        myWeb.msRedirectOnEnd = Conversions.ToString(myWeb.moSession["lastPage"]);
                                    }
                                    // myWeb.msRedirectOnEnd = "?pgid=" & myWeb.moRequest("pgid")
                                    myWeb.moSession["ewCmd"] = "";
                                }

                                break;
                            }
                        case "cleanLocation":
                            {
                                if (bAdminMode)
                                    myWeb.moDbHelper.cleanLocations();
                                break;
                            }
                        case "ProductGroups":
                            {
                                ProductGroupsProcess(ref oPageDetail, ref sAdminLayout, Conversions.ToInteger(Interaction.IIf(Information.IsNumeric(myWeb.moRequest.QueryString["GrpID"]), myWeb.moRequest.QueryString["GrpID"], (object)0)));
                                break;
                            }
                        case "AddProductGroups":
                        case "EditProductGroups":
                            {
                                bLoadStructure = true;
                                sAdminLayout = "AdminXForm";
                                oPageDetail.AppendChild(moAdXfm.xFrmProductGroup(Conversions.ToInteger(Interaction.IIf(Information.IsNumeric(myWeb.moRequest.QueryString["GroupId"]), myWeb.moRequest.QueryString["GroupId"], (object)0))));
                                if (moAdXfm.valid)
                                {
                                    mcEwCmd = "ProductGroups";
                                    goto ProcessFlow;
                                }

                                break;
                            }
                        case "DeleteProductGroups":
                            {
                                myWeb.moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.CartProductCategories, Conversions.ToLong(myWeb.moRequest.QueryString["GroupId"]));
                                mcEwCmd = "ProductGroups";
                                goto ProcessFlow;
                            }
                        case "AddProductGroupsProduct":
                            {
                                bLoadStructure = true;
                                sAdminLayout = "LocateContent";
                                if (stdTools.ButtonSubmitted(ref myWeb.moRequest, "saveRelated"))
                                {
                                    myWeb.moDbHelper.saveProductsGroupRelations();
                                    myWeb.msRedirectOnEnd = myWeb.moRequest.QueryString["Path"] + "?ewCmd=ProductGroups&GroupId=" + myWeb.moRequest.QueryString["GroupId"];
                                }
                                else
                                {
                                    string sProductTypes = "Product,SKU,Ticket";
                                    if (myWeb.Features.ContainsKey("Subscriptions"))
                                    {
                                        sProductTypes = sProductTypes + ",Subscription";
                                    }
                                    if (!string.IsNullOrEmpty(moConfig["ProductTypes"]))
                                    {
                                        sProductTypes = moConfig["ProductTypes"];
                                    }

                                    oPageDetail.AppendChild(moAdXfm.xFrmFindRelated(myWeb.moRequest.QueryString["GroupId"], sProductTypes, ref oPageDetail, myWeb.moRequest.QueryString["GroupId"], true, "tblCartCatProductRelations", "nContentId", "nCatId"));
                                    sAdminLayout = "RelatedSearch";
                                }

                                break;
                            }
                        case "RemoveProductGroupsProduct":
                            {
                                myWeb.moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.CartCatProductRelations, Conversions.ToLong(myWeb.moRequest.QueryString["RelId"]));
                                mcEwCmd = "ProductGroups";
                                goto ProcessFlow;

                            }
                        case "DiscountRules":
                        case "EditDiscountRules":
                            {
                                if (stdTools.ButtonSubmitted(ref myWeb.moRequest, "addNewDiscountRule") & Information.IsNumeric(myWeb.moRequest.Form["newDiscountType"]) | stdTools.ButtonSubmitted(ref myWeb.moRequest, "ewSubmit") | mcEwCmd == "EditDiscountRules")

                                {
                                    bLoadStructure = true;
                                    sAdminLayout = "AdminXForm";
                                    long nDiscountType = Conversions.ToLong(Interaction.IIf(Information.IsNumeric(myWeb.moRequest.Form["newDiscountType"]), myWeb.moRequest.Form["newDiscountType"], (object)0));
                                    nDiscountType = Conversions.ToLong(Interaction.IIf(Information.IsNumeric(myWeb.moRequest.Form["nDiscountCat"]), myWeb.moRequest.Form["nDiscountCat"], (object)nDiscountType));

                                    oPageDetail.AppendChild(moAdXfm.xFrmDiscountRule(Conversions.ToInteger(Interaction.IIf(Information.IsNumeric(myWeb.moRequest.QueryString["DiscId"]), myWeb.moRequest.QueryString["DiscId"], (object)0)), (int)nDiscountType));

                                    if (moAdXfm.valid)
                                    {
                                        myWeb.ClearPageCache();
                                        DiscountRulesProcess(ref oPageDetail, ref sAdminLayout);
                                    }
                                }
                                else
                                {
                                    DiscountRulesProcess(ref oPageDetail, ref sAdminLayout);
                                }

                                break;
                            }

                        case "RemoveDiscountRules":
                            {
                                myWeb.moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.CartDiscountRules, Conversions.ToLong(myWeb.moRequest.QueryString["DiscId"]));
                                mcEwCmd = "DiscountRules";
                                goto ProcessFlow;

                            }
                        case "ApplyDirDiscountRules":
                            {
                                bLoadStructure = true;
                                sAdminLayout = "AdminXForm";
                                oPageDetail.AppendChild(moAdXfm.xFrmDiscountDirRelations(Conversions.ToLong(myWeb.moRequest.QueryString["DiscId"]), ""));
                                break;
                            }
                        case "ApplyGrpDiscountRules":
                            {
                                bLoadStructure = true;
                                sAdminLayout = "AdminXForm";
                                oPageDetail.AppendChild(moAdXfm.xFrmDiscountProductRelations(Conversions.ToLong(myWeb.moRequest.QueryString["DiscId"]), ""));
                                break;
                            }
                        case "SiteIndex":
                            {
                                bLoadStructure = true;
                                sAdminLayout = "SiteIndex";
                                oPageDetail.AppendChild(moAdXfm.xFrmStartIndex());
                                break;
                            }
                        case "EditTemplate":
                            {
                                sAdminLayout = "EditTemplate";
                                oPageDetail.AppendChild(moAdXfm.xFrmEditTemplate());
                                break;
                            }

                        // -- Call all of the process for the newsletter functionaltiy
                        case "MailingList":
                        case "NormalMail":
                        case "MailPreviewOn":
                        case "AdvancedMail":
                        case "AddMailModule":
                        case "EditMailContent":
                        case "EditMail":
                        case "EditMailLayout":
                        case "NewMail":
                        case "PreviewMail":
                        case "SendMail":
                        case "SendMailPersonalised":
                        case "SendMailunPersonalised":
                        case "MailHistory":
                        case "MailOptOut":
                        case "ProcessMailbox":
                        case "DeletePageMail":
                        case "SyncMailList":
                        case "ListMailLists":
                            {
                                bMailMenu = true;

                                System.Collections.Specialized.NameValueCollection moMailConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/mailinglist");
                                string sMessagingProvider = "";
                                if (moMailConfig != null)
                                {
                                    sMessagingProvider = moMailConfig["MessagingProvider"];
                                }
                                Protean.Providers.Messaging.ReturnProvider RetProv = new Protean.Providers.Messaging.ReturnProvider();
                                IMessagingProvider moMessaging = RetProv.Get(ref myWeb, sMessagingProvider);

                                string passthroughCmd = mcEwCmd;
                                if (!string.IsNullOrEmpty(mcEwCmd2))
                                {
                                    passthroughCmd = passthroughCmd + "." + mcEwCmd2;
                                }

                                string sEditContext = EditContext;
                                string processResponse = moMessaging.AdminProcess.MailingListProcess(ref oPageDetail, ref oWeb, ref sAdminLayout, ref passthroughCmd, ref bLoadStructure, ref sEditContext, clearEditContext);
                                // If oMessaging.AdminProcess.MailingListProcess(oPageDetail, oWeb, sAdminLayout, myWeb.moRequest("ewCmd"), bLoadStructure, EditContext, clearEditContext) = "GoTo" Then GoTo ProcessFlow
                                if (processResponse == "GoTo")
                                    goto ProcessFlow;

                                mcEwCmd = passthroughCmd;

                                if (sAdminLayout == "Preview")
                                {
                                    sAdminLayout = "";
                                    mbPreviewMode = true;
                                }

                                moMessaging = null;
                                break;
                            }

                        case "ScheduledItems":
                        case "AddScheduledItem":
                        case "EditScheduledItem":
                        case "DeleteScheduledItem":
                        case "DeactivateScheduledItem":
                        case "ActivateScheduledItem":
                        case "ScheduledItemRunNow":
                            {
                                SchedulerProcess(ref mcEwCmd, ref sAdminLayout, ref oPageDetail);
                                bLoadStructure = true;
                                break;
                            }

                        case "SystemPages":
                            {
                                bSystemPagesMenu = true;
                                sAdminLayout = "SystemPages";
                                oWeb.mbAdminMode = true;
                                sAdminLayout = "SystemPages";
                                oWeb.mnPageId = (int)oWeb.mnSystemPagesId;
                                break;
                            }
                        // oWeb.mnSystemPagesId = getPageIdFromPath("System+Pages", False, False)



                        case "ViewSystemPages":
                            {
                                bSystemPagesMenu = true;
                                oWeb.mnSystemPagesId = (long)myWeb.moDbHelper.getPageIdFromPath("System+Pages", false, false);
                                oWeb.mbAdminMode = false;
                                if (!myWeb.mbSuppressLastPageOverrides)
                                    myWeb.moSession["lastPage"] = "/" + Cms.gcProjectPath + myWeb.mcPagePath.TrimStart('/') + "?ewCmd=ViewSystemPages&pgid=" + myWeb.mnPageId;
                                break;
                            }

                        case "Subscriptions":
                        case "EditUserSubscription":
                        case "AddSubscriptionGroup":
                        case "EditSubscriptionGroup":
                        case "AddSubscription":
                        case "CancelSubscription":
                        case "ResendCancellation":
                        case "EditSubscription":
                        case "MoveSubscription":
                        case "RenewSubscription":
                        case "ResendSubscription":
                        case "LocateSubscription":
                        case "UpSubscription":
                        case "DownSubscription":
                        case "ListSubscribers":
                        case "ManageUserSubscription":
                        case "UpcomingRenewals":
                        case "ExpiredSubscriptions":
                        case "CancelledSubscriptions":
                        case "RenewalAlerts":
                            {
                                SubscriptionProcess(ref mcEwCmd, ref sAdminLayout, ref oPageDetail);
                                bLoadStructure = true;
                                break;
                            }

                        case "EditXForm":
                            {
                                EditXFormProcess(ref sAdminLayout, ref oPageDetail, ref bLoadStructure);
                                break;
                            }

                        case "Reports":
                            {
                                ReportsProcess(ref oPageDetail, ref sAdminLayout);
                                break;
                            }


                        case "FilterIndex":
                            {
                                FilterIndex(ref oPageDetail, ref sAdminLayout);
                                break;
                            }

                        case "ResetWebConfig":
                            {
                                ResetWebConfig();
                                break;
                            }


                    }

                    SupplimentalProcess(ref sAdminLayout, ref oPageDetail);

                AfterProcessFlow:
                    ;


                    // Supplimental Process check for Go To Process Flow
                    if (sAdminLayout == "GoToProcessFlow")
                    {
                        goto ProcessFlow;
                    }

                    // we want to persist the cmd if we are in normal, advanced mode, so we can navigate
                    if (mcEwCmd == "Normal" | mcEwCmd == "PreviewOn" | mcEwCmd == "ViewSystemPages")
                    {
                        myWeb.moSession["ewCmd"] = mcEwCmd;
                        bAdminMode = false;
                        sAdminLayout = "";
                    }
                    // ElseIf mcEwCmd = "AddContent" Or mcEwCmd = "EditContent" Then
                    // myWeb.moSession("ewCmd") = mcEwCmd
                    // bAdminMode = False
                    // sAdminLayout = ""
                    else if (mcEwCmd == "NormalMail")
                    {
                        myWeb.moSession["ewCmd"] = mcEwCmd;
                        bAdminMode = false;
                        sAdminLayout = "";
                    }
                    else if (mcEwCmd == "EditPageSEO")
                    {
                        bAdminMode = true;
                        myWeb.moSession["ewCmd"] = mcEwCmd;
                    }
                    else if (mcEwCmd == "EditPage")
                    {
                        bAdminMode = true;
                        sAdminLayout = "PageSettings";
                        myWeb.moSession["ewCmd"] = mcEwCmd;
                    }
                    else if (mcEwCmd == "Advanced" | mcEwCmd == "EditStructure" | mcEwCmd == "EditPage")
                    {
                        bAdminMode = true;
                        sAdminLayout = mcEwCmd;
                        myWeb.moSession["ewCmd"] = mcEwCmd;
                    }
                    else if (mcEwCmd == "SystemPages")
                    {
                        myWeb.moSession["ewCmd"] = mcEwCmd;
                        bAdminMode = true;
                        bSystemPagesMenu = true;
                        // basically an exact copy of what is going on above
                        // not the best, but only way to do it
                        sAdminLayout = "SystemPages";
                        oWeb.mnSystemPagesId = (long)myWeb.moDbHelper.getPageIdFromPath("System+Pages", false, false);
                        if (oWeb.mnSystemPagesId == (long)Cms.gnTopLevel)
                        {
                            // we have no System Pages page we better create one.
                            oWeb.mnSystemPagesId = myWeb.moDbHelper.insertStructure(0L, "", "System Pages", "", "Column1");
                        }
                        oWeb.mnPageId = (int)oWeb.mnSystemPagesId;
                        sAdminLayout = "SystemPages";

                    }

                    myWeb.moSession["editContext"] = EditContext;

                    // reset the parid after go to processflow redirects
                    if (bResetParId == true)
                    {
                        myWeb.moSession["nParId"] = "0";
                    }


                    if (bMailMenu | mcEwCmd == "NormalMail")
                    {
                        if (moConfig["MailingList"] == "on")
                        {
                            System.Collections.Specialized.NameValueCollection moMailConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/mailinglist");
                            if (moMailConfig != null)
                            {
                                oWeb.mnMailMenuId = Conversions.ToLong(moMailConfig["RootPageId"]);
                                // oWeb.GetStructureXML("Newsletter", , moMailConfig("RootPageId"))
                            }
                        }
                    }
                    if (bSystemPagesMenu)
                    {
                        oWeb.mnSystemPagesId = (long)myWeb.moDbHelper.getPageIdFromPath("System+Pages", false, false);
                    }

                    if (oWeb.moPageXml.DocumentElement is null)
                    {
                        XmlElement oPageElmt;
                        oWeb.moPageXml.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                        oPageElmt = oWeb.moPageXml.CreateElement("Page");
                        oWeb.moPageXml.AppendChild(oPageElmt);
                        moPageXML = oWeb.moPageXml;
                        moPageXML.DocumentElement.SetAttribute("title", sPageTitle);
                    }

                    if (mbPreviewMode)
                    {
                        moPageXML.DocumentElement.SetAttribute("previewMode", Strings.LCase(mbPreviewMode.ToString()));
                        if (moPageXML.SelectSingleNode("AdminMenu") != null)
                        {
                            moPageXML.RemoveChild(moPageXML.SelectSingleNode("AdminMenu"));
                        }
                        if (mcEwCmd != "MailPreviewOn")
                        {
                            GetPreviewMenu();
                        }
                    }
                    else
                    {
                        moPageXML.DocumentElement.SetAttribute("adminMode", Strings.LCase(bAdminMode.ToString()));
                    }


                    // We have done all our updating lets get new pagexml
                    if (mcEwCmd == "Normal" | mcEwCmd == "Advanced" | mcEwCmd == "PreviewOn" | mcEwCmd == "MailPreviewOn" | mcEwCmd == "EditXForm" | mcEwCmd == "EditPage" | mcEwCmd == "EditPageSEO" | mcEwCmd == "NormalMail" | mcEwCmd == "AdvancedMail" | mcEwCmd == "NewMail" | mcEwCmd == "MailingList" | mcEwCmd == "SystemPages" | mcEwCmd == "ViewSystemPages" | mcEwCmd == "LocateContent" | mcEwCmd == "MoveContent" | mcEwCmd == "LocateContentDetail")

                    {
                        // TS: removed 4 jul 2014 do not need to load page content when editing
                        // Or mcEwCmd = "EditContent" Or mcEwCmd = "CopyContent" Or mcEwCmd = "AddContent" Then
                        if (mcEwCmd == "PreviewOn" | mcEwCmd == "MailPreviewOn")
                        {
                            oWeb.mbAdminMode = false;
                        }
                        else
                        {
                            oWeb.mbAdminMode = true;
                        }
                        // no need to get page xml if redirecting, speeds up editing on pages with loads of content.
                        if (string.IsNullOrEmpty(myWeb.msRedirectOnEnd))
                        {
                            oWeb.mbPreview = true;
                            oWeb.GetPageXML();
                        }
                    }
                    else
                    {
                        getAdminXML(ref oWeb, bLoadStructure);
                    }

                    if (oPageDetail != null)
                    {
                        if (moPageXML.DocumentElement.SelectSingleNode("ContentDetail") is null)
                        {
                            if (!string.IsNullOrEmpty(oPageDetail.InnerXml))
                            {
                                moPageXML.DocumentElement.AppendChild(oPageDetail);
                            }
                        }
                        else if (!string.IsNullOrEmpty(oPageDetail.InnerXml))
                        {
                            moPageXML.DocumentElement.ReplaceChild(oPageDetail, moPageXML.DocumentElement.SelectSingleNode("ContentDetail"));
                        }
                    }

                    if (!string.IsNullOrEmpty(sAdminLayout))
                    {
                        moPageXML.DocumentElement.SetAttribute("layout", sAdminLayout);
                    }

                    if (bClearEditContext)
                    {
                        myWeb.moSession["editContext"] = null;
                        EditContext = "";
                    }

                    if (EditContext != "")
                    {
                        moPageXML.DocumentElement.SetAttribute("editContext", EditContext);
                        myWeb.moSession["editContext"] = EditContext;
                    }



                    moPageXML.DocumentElement.SetAttribute("ewCmd", mcEwCmd);
                    if (!string.IsNullOrEmpty(mcEwCmd2))
                        moPageXML.DocumentElement.SetAttribute("ewCmd2", mcEwCmd2);
                    if (!string.IsNullOrEmpty(mcEwCmd3))
                        moPageXML.DocumentElement.SetAttribute("ewCmd3", mcEwCmd3);

                    if (!string.IsNullOrEmpty(myWeb.moRequest["parid"]))
                    {
                        moPageXML.DocumentElement.SetAttribute("parId", myWeb.moRequest["parid"]);
                    }
                }


                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "adminProcess", ex, "", sProcessInfo, gbDebug);
                }
                finally
                {

                }

            }

            public virtual void adminAccessRights()
            {
                string processInfo = "";
                XmlElement oUserXml;
                XmlElement oMenuElmt;
                var deleteCmds = new Hashtable();
                string pagePermLevel = string.Empty;

                try
                {

                    // Get the admin menu for the site
                    GetAdminMenu();



                    if (myWeb.mbPreview)
                    {
                        var tmp = myWeb.moSession;
                        int argnUserId = Conversions.ToInteger(tmp["nUserId"]);
                        oUserXml = myWeb.moDbHelper.getUserXMLById(ref argnUserId);
                        tmp["nUserId"] = (object)argnUserId;
                    }
                    else
                    {
                        // get the user permissions
                        myWeb.RefreshUserXML();
                        oUserXml = (XmlElement)moPageXML.SelectSingleNode("/Page/User");
                    }

                    if (oUserXml != null)
                    {
                        pagePermLevel = oUserXml.GetAttribute("pagePermission");
                    }

                    // Are you a domain user if so you are god !

                    // RJP 7 Nov 2012. Added LCase to MembershipEncryption.
                    if (Conversions.ToBoolean(!Operators.ConditionalCompareObjectEqual(myWeb.moSession["ewAuth"], Encryption.HashString(myWeb.moSession.SessionID + moConfig["AdminPassword"], Strings.LCase(myWeb.moConfig["MembershipEncryption"]), true), false)))
                    {

                        // Are you an administrator user with no AdminRights yet set then you too are god ! this to cater for existing sites
                        if (oUserXml.SelectSingleNode("Role[@name='Administrator' and @isMember='yes' and not(AdminRights)]") is null)
                        {

                            // Otherwise step throught the admin menu and remove stuff
                            foreach (XmlElement currentOMenuElmt in myWeb.moPageXml.SelectNodes("/Page/AdminMenu/descendant-or-self::*"))
                            {
                                oMenuElmt = currentOMenuElmt;

                                string ewCmd = oMenuElmt.GetAttribute("cmd");
                                if (!(string.IsNullOrEmpty(ewCmd) | ewCmd == "PreviewOn"))
                                {
                                    // do you have permissions on the current ewCmd ?
                                    if (oUserXml.SelectSingleNode("descendant-or-self::MenuItem[@cmd='" + ewCmd + "' and @adminRight='true']") is null)
                                    {

                                        // give some info about feature denied to user
                                        moDeniedAdminMenuElmt = (XmlElement)oMenuElmt.CloneNode(false);

                                        oMenuElmt.SetAttribute("adminRight", "false");

                                        if (deleteCmds[ewCmd] is null)
                                            deleteCmds.Add(ewCmd, ewCmd);

                                        if ((mcEwCmd ?? "") == (ewCmd ?? ""))
                                        {
                                            // Set the ewCmd to "adminDenied"
                                            mcEwCmd = "adminDenied";
                                            return;
                                        }
                                        else
                                        {
                                            moDeniedAdminMenuElmt = null;
                                        }
                                    }
                                    else
                                    {
                                        // Clever stuff for page level editing rights

                                        switch (pagePermLevel ?? "")
                                        {

                                            // Denied = 0 
                                            // Open = 1 
                                            // View = 2
                                            // Add = 3
                                            // AddUpdateOwn = 4
                                            // UpdateAll = 5
                                            // Approve = 6
                                            // AddUpdateOwnPublish = 7
                                            // Publish = 8
                                            // Full = 9

                                            case "Full":
                                            case "Publish":
                                            case "AddUpdateOwnPublish":
                                            case "Approve":
                                            case "AddUpdateOwn":
                                            case "UpdateAll":
                                                {
                                                    break;
                                                }
                                            // do nothing alls good
                                            case "Add":
                                                {
                                                    switch (ewCmd ?? "")
                                                    {
                                                        case "EditContent":
                                                        case "DeleteContent":
                                                        case "EditModule":
                                                        case "AddModule":
                                                        case "CopyContent":
                                                        case "LocateContent":
                                                        case "MoveContent":
                                                            {
                                                                if ((mcEwCmd ?? "") == (ewCmd ?? ""))
                                                                {
                                                                    moDeniedAdminMenuElmt = (XmlElement)oMenuElmt.CloneNode(false);
                                                                    mcEwCmd = "adminDenied";
                                                                    return;
                                                                }
                                                                if (deleteCmds[ewCmd] is null)
                                                                    deleteCmds.Add(ewCmd, ewCmd);
                                                                break;
                                                            }
                                                    }

                                                    break;
                                                }

                                            default:
                                                {
                                                    switch (ewCmd ?? "")
                                                    {
                                                        case "EditContent":
                                                        case "AddContent":
                                                        case "EditModule":
                                                        case "AddModule":
                                                        case "DeleteContent":
                                                        case "CopyContent":
                                                        case "LocateContent":
                                                        case "MoveContent":
                                                            {
                                                                if ((mcEwCmd ?? "") == (ewCmd ?? ""))
                                                                {
                                                                    moDeniedAdminMenuElmt = (XmlElement)oMenuElmt.CloneNode(false);
                                                                    mcEwCmd = "adminDenied";
                                                                    return;
                                                                }
                                                                if (deleteCmds[ewCmd] is null)
                                                                    deleteCmds.Add(ewCmd, ewCmd);
                                                                break;
                                                            }
                                                    }

                                                    break;
                                                }
                                        }
                                        // Clever stuff for page level content type editing rights (Where would this be stored ?)
                                    }
                                }

                            }
                            foreach (string key in deleteCmds.Keys)
                            {
                                processInfo = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("deleting ", deleteCmds[key]), " from admin menu"));
                                oMenuElmt = (XmlElement)myWeb.moPageXml.SelectSingleNode(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("/Page/AdminMenu/descendant-or-self::*[@cmd='", deleteCmds[key]), "']")));
                                if (oMenuElmt != null)
                                    oMenuElmt.ParentNode.RemoveChild(oMenuElmt);
                            }


                        }

                    }
                }


                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "adminAccessRights", ex, "", processInfo, gbDebug);
                }
                finally
                {

                }
            }


            public virtual void EditXFormProcess(ref string adminLayout, ref XmlElement pageDetail, ref bool loadStructure)
            {
                string processInfo = "";
                Admin.XFormEditor editor;
                long contentId = 0L;
                XmlElement xfrm;
                bool skip = false;
                bool saveInstance = false;

                string @ref = myWeb.moRequest["ref"];

                try
                {

                    if (myWeb.moRequest["artid"] != null && Information.IsNumeric(myWeb.moRequest["artid"] + ""))
                    {
                        contentId = Conversions.ToLong(myWeb.moRequest["artid"] + "");
                    }

                    editor = (Admin.XFormEditor)myWeb.GetXformEditor(contentId);

                    switch (mcEwCmd2 ?? "")
                    {
                        case var @case when @case == "":
                            {
                                adminLayout = "AdminXForm";
                                break;
                            }

                        case "EditGroup":
                            {
                                loadStructure = true;
                                xfrm = editor.xFrmEditXFormGroup(@ref);
                                if (editor.valid)
                                {
                                    mcEwCmd = "EditXForm";
                                    mcEwCmd2 = "";
                                    saveInstance = true;
                                }
                                else
                                {
                                    pageDetail.AppendChild(pageDetail.OwnerDocument.ImportNode(xfrm, true));
                                }

                                break;
                            }

                        case "AddGroup":
                            {
                                loadStructure = true;
                                xfrm = editor.xFrmEditXFormGroup("", @ref);
                                if (editor.valid)
                                {
                                    mcEwCmd = "EditXForm";
                                    mcEwCmd2 = "";
                                    saveInstance = true;
                                }
                                else
                                {
                                    pageDetail.AppendChild(pageDetail.OwnerDocument.ImportNode(xfrm, true));
                                }

                                break;
                            }

                        case "DeleteElement":
                            {

                                loadStructure = true;
                                // For Deleting xForm Elements
                                xfrm = editor.xFrmDeleteElement(myWeb.moRequest["ref"], myWeb.moRequest["pos"]);
                                if (editor.valid)
                                {
                                    mcEwCmd = "EditXForm";
                                    mcEwCmd2 = "";
                                    saveInstance = true;
                                }
                                else
                                {
                                    pageDetail.AppendChild(pageDetail.OwnerDocument.ImportNode(xfrm, true));
                                }

                                break;
                            }

                        case "MoveTop":
                        case "MoveUp":
                        case "MoveDown":
                        case "MoveBottom":
                            {
                                loadStructure = true;
                                editor.moveElement(Conversions.ToLong(myWeb.moRequest["artid"]), myWeb.moRequest["ref"], mcEwCmd2);
                                mcEwCmd = "EditXForm";
                                saveInstance = true;
                                break;
                            }

                        case "MoveItemTop":
                        case "MoveItemUp":
                        case "MoveItemDown":
                        case "MoveItemBottom":
                        case "DeleteItem":
                            {
                                loadStructure = true;
                                editor.moveElement(Conversions.ToLong(myWeb.moRequest["artid"]), myWeb.moRequest["ref"], mcEwCmd2, Conversions.ToLong(myWeb.moRequest["pos"]));
                                mcEwCmd = "EditXForm";
                                mcEwCmd2 = "";
                                saveInstance = true;
                                break;
                            }

                        case "EditInput":
                        case var case1 when case1 == "EditInput":
                            {

                                loadStructure = true;
                                xfrm = editor.xFrmEditXFormInput(myWeb.moRequest["ref"], myWeb.moRequest["parref"], myWeb.moRequest["type"]);
                                if (editor.valid)
                                {
                                    mcEwCmd = "EditXForm";
                                    mcEwCmd2 = "";
                                    saveInstance = true;
                                }
                                else
                                {
                                    pageDetail.AppendChild(pageDetail.OwnerDocument.ImportNode(xfrm, true));
                                }

                                break;
                            }


                        case "EditItem":
                        case "AddItem":
                            {
                                loadStructure = true;
                                xfrm = editor.xFrmEditXFormItem(myWeb.moRequest["ref"], Conversions.ToLong(myWeb.moRequest["pos"]));
                                if (editor.valid)
                                {
                                    mcEwCmd = "EditXForm";
                                    mcEwCmd2 = "";
                                    saveInstance = true;
                                }
                                else
                                {
                                    pageDetail.AppendChild(pageDetail.OwnerDocument.ImportNode(xfrm, true));
                                }

                                break;
                            }

                        default:
                            {
                                skip = true;
                                break;
                            }
                    }

                    if (!skip)
                    {
                        adminLayout = "AdminXForm";

                        if (saveInstance)
                        {
                            // save the xform back in the database
                            myWeb.moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Content, editor.MasterInstance);
                        }
                    }

                    xfrm = null;
                }

                // This is just a placeholder for overloading
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "EditXFormProcess", ex, "", processInfo, gbDebug);
                }
                finally
                {

                }
            }

            public virtual void SupplimentalProcess(ref string sAdminLayout, ref XmlElement oPageDetail)
            {

                // This is just a placeholder for overloading

            }

            public void FileImportProcess(ref XmlElement oPageDetail, ref string sAdminLayout)
            {
                string sProcessInfo = "";
                string sErrorMsg = "";
                try
                {

                    sAdminLayout = "FileImport";
                    string sImportLoaction = @"..\imports\";


                    oPageDetail.AppendChild(moAdXfm.xFrmImportFile(sImportLoaction));
                    if (moAdXfm.valid)
                    {

                        string cXsltPath = moAdXfm.Instance.SelectSingleNode("file/@importXslt").InnerText;
                        string cFilePath = moAdXfm.Instance.SelectSingleNode("file/@filename").InnerText;

                        // first we take our take our xls and convert to xml
                        var oImportXml = new XmlDocument();

                        if (cFilePath.EndsWith(".xls") | cFilePath.EndsWith(".xlsx") | cFilePath.EndsWith(".csv"))
                        {

                            Protean.Tools.Conversion oConvert = new Protean.Tools.Conversion(Protean.Tools.Conversion.Type.Excel, Protean.Tools.Conversion.Type.Xml, cFilePath);
                            oConvert.Convert();
                            if (oConvert.State == Tools.Conversion.Status.Succeeded)
                            {
                                XmlDocument outputXml = (XmlDocument)oConvert.Output;
                                oImportXml.LoadXml(outputXml.OuterXml);
                            }
                            else
                            {
                                moAdXfm.valid = false;
                                XmlNode argoNode = (XmlNode)moAdXfm.moXformElmt;
                                moAdXfm.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, oConvert.Message);
                                moAdXfm.moXformElmt = (XmlElement)argoNode;
                                sProcessInfo = oConvert.Message;
                            }

                          //  oImportXml.LoadXml(Conversions.ToString(oConvert.Output.OuterXml));
                        }
                        else if (cFilePath.EndsWith(".xml"))
                        {
                            // cFilePath = myWeb.goServer.MapPath(cFilePath)
                            oImportXml.Load(cFilePath);

                            var oImportRootElmt = oImportXml.DocumentElement;

                            if (oImportRootElmt.Name == "DatabaseImport")
                            {
                                string DBConn;
                                string databaseType = oImportRootElmt.GetAttribute("databaseType");
                                if (string.IsNullOrWhiteSpace(databaseType))
                                {
                                    databaseType = "mssql";
                                }

                                switch (databaseType ?? "")
                                {
                                    case "mssql":
                                        {
                                            DBConn = "Data Source=" + oImportRootElmt.GetAttribute("databaseServer") + "; " + "Initial Catalog=" + oImportRootElmt.GetAttribute("databaseName") + "; " + "user id=" + oImportRootElmt.GetAttribute("databaseUsername") + "; password=" + oImportRootElmt.GetAttribute("databasePassword");

                                            var newDb = new Cms.dbHelper(DBConn, (long)mnAdminUserId, myWeb.moCtx);
                                            if (newDb.ConnectionValid == false)
                                            {
                                                moAdXfm.valid = false;
                                                sErrorMsg = "Bad DB Connection - " + DBConn;
                                            }
                                            else
                                            {
                                                var ImportDS = new DataSet();
                                                string sSql = oImportRootElmt.GetAttribute("select");
                                                if (string.IsNullOrEmpty(sSql))
                                                {
                                                    sSql = "select * from " + oImportRootElmt.GetAttribute("tableName");
                                                }
                                                ImportDS = newDb.GetDataSet(sSql, oImportRootElmt.GetAttribute("tableName"));
                                                oImportXml.LoadXml(ImportDS.GetXml());
                                            }

                                            break;
                                        }
                                        // Case "mysql"
                                        // DBConn = "server=" & oImportRootElmt.GetAttribute("databaseServer") & "; " &
                                        // "port=" & oImportRootElmt.GetAttribute("databasePort") & "; " &
                                        // "database=" & oImportRootElmt.GetAttribute("databaseName") & "; " &
                                        // "uid=" & oImportRootElmt.GetAttribute("databaseUsername") & "; pwd=" & oImportRootElmt.GetAttribute("databasePassword")

                                        // Dim mysqlDb As New MySqlDatabase(DBConn)
                                        // If mysqlDb.ConnectionValid = False Then
                                        // moAdXfm.valid = False
                                        // sErrorMsg = "Bad DB Connection - " & DBConn
                                        // Else
                                        // Dim ImportDS As New DataSet
                                        // Dim sSql As String = oImportRootElmt.GetAttribute("select")
                                        // If sSql = "" Then
                                        // sSql = "select * from " & oImportRootElmt.GetAttribute("tableName")
                                        // End If
                                        // ImportDS = mysqlDb.GetDataSet(sSql, oImportRootElmt.GetAttribute("tableName"))
                                        // oImportXml.LoadXml(ImportDS.GetXml())
                                        // End If
                                }
                            }
                        }

                        // converted successfully to xml
                        if (string.IsNullOrEmpty(sErrorMsg))
                        {

                            if (!string.IsNullOrEmpty(oImportXml.OuterXml))
                            {

                                // lets just output our source xml
                                var oPreviewElmt = moPageXML.CreateElement("PreviewFileXml");
                                oPreviewElmt.InnerXml = oImportXml.OuterXml;
                                oPageDetail.AppendChild(oPreviewElmt);

                                // NB: Old Tools Transform----------------
                                // then we transform to our standard import XML
                                // Dim oTransform As New Protean.Tools.Xslt.Transform
                                // Dim moXSLTFunctions As New Tools.Xslt.XsltFunctions
                                // oTransform.Xml = oImportXml
                                // oTransform.XslTExtensionObject = moXSLTFunctions
                                // oTransform.XslTExtensionURN = "ew"

                                // oTransform.XslTFile = myWeb.goServer.MapPath("/xsl/import/" & cXsltPath)
                                // Dim cInstancesXml As String = oTransform.Process()
                                // NB: ----------------

                                // NB: New (Web) Transform
                                string styleFile = myWeb.goServer.MapPath("/xsl/import/" + cXsltPath);
                                var oTransform = new Protean.XmlHelper.Transform(ref myWeb, styleFile, false);
                                myWeb.PerfMon.Log("Admin", "FileImportProcess-startxsl");
                                oTransform.mbDebug = gbDebug;
                                oTransform.ProcessDocument(oImportXml);
                                myWeb.PerfMon.Log("Admin", "FileImportProcess-endxsl");
                                // We display the results
                                var oPreviewElmt2 = moPageXML.CreateElement("PreviewImport");
                                if (oTransform.HasError)
                                {
                                    oPreviewElmt2.SetAttribute("errorMsg", oTransform.currentError.Message);
                                    oPreviewElmt2.InnerText = oTransform.currentError.StackTrace;
                                }
                                else
                                {
                                    oPreviewElmt2.InnerXml = oImportXml.InnerXml;

                                }
                                oPageDetail.AppendChild(oPreviewElmt2);
                                oTransform = (Protean.XmlHelper.Transform)null;

                                // We save to database if OK
                                string cOppMode = moAdXfm.Instance.SelectSingleNode("file/@opsMode").InnerText;
                                if (cOppMode == "import")
                                {
                                    // here we go lets do the do...!! whoah!
                                    myWeb.moDbHelper.importObjects((XmlElement)oPreviewElmt2.FirstChild);

                                }
                            }

                            else
                            {
                                moAdXfm.valid = false;
                                sErrorMsg = "No Content Returned";
                                XmlNode argoNode1 = (XmlNode)moAdXfm.moXformElmt;
                                moAdXfm.addNote(ref argoNode1, Protean.xForm.noteTypes.Alert, sErrorMsg);
                                moAdXfm.moXformElmt = (XmlElement)argoNode1;
                            }
                        }
                        else
                        {

                            XmlNode argoNode2 = (XmlNode)moAdXfm.moXformElmt;
                            moAdXfm.addNote(ref argoNode2, Protean.xForm.noteTypes.Alert, sErrorMsg);
                            moAdXfm.moXformElmt = (XmlElement)argoNode2;

                        }

                    }
                }


                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "FileImportProcess", ex, "", "", gbDebug);
                }
            }

            public virtual void SupplimentalAdminMenu(ref XmlElement oAdminMenu)
            {

                // This is just a placeholder for overloading

            }

            private void getAdminXML(ref Cms oWeb, bool bLoadStructure = false)
            {

                XmlElement oPageElmt;
                string sProcessInfo = "";
                string sLayout = "default";

                try
                {

                    if (moPageXML.DocumentElement is null)
                    {
                        moPageXML.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                        oPageElmt = moPageXML.CreateElement("Page");
                        moPageXML.AppendChild(oPageElmt);
                        oWeb.GetRequestVariablesXml(ref oPageElmt);
                        oWeb.GetSettingsXml(ref oPageElmt);
                    }
                    else
                    {
                        oPageElmt = moPageXML.DocumentElement;
                    }

                    if (!(myWeb.mnUserId == 0))
                    {
                        // oPageElmt.AppendChild(oWeb.GetUserXML())
                        if (bLoadStructure)
                        {
                            // oPageElmt.AppendChild(oWeb.GetStructureXML())
                            // Ensure that the structure is always called from the root level, and not from any manipulation of the top level,
                            // such as AuthenticatedRootPageId
                            myWeb.GetStructureXML("Site", (long)-1, (long)mnAdminTopLevel);
                        }
                    }

                    oPageElmt.SetAttribute("layout", sLayout);
                    oPageElmt.SetAttribute("id", myWeb.mnPageId.ToString());
                    oPageElmt.SetAttribute("cssFramework", moConfig["cssFramework"]);
                    oPageElmt.SetAttribute("userIntegrations", Cms.gbUserIntegrations.ToString().ToLower());

                    // not sure if we need this block
                    if (!string.IsNullOrEmpty(myWeb.moRequest["artid"]))
                    {
                        myWeb.mnArtId = Conversions.ToInteger(myWeb.moRequest["artid"]);
                    }
                    else
                    {

                        long argresult = (long)myWeb.mnArtId;
                        long.TryParse(myWeb.moRequest["id"], out argresult);
                        myWeb.mnArtId = (int)argresult;

                    }

                    if (myWeb.mnArtId > 0)
                    {
                        oPageElmt.SetAttribute("artid", myWeb.mnArtId.ToString());
                    }
                }

                catch (Exception ex)
                {

                    stdTools.returnException(ref myWeb.msException, mcModuleName, "buildPageXML", ex, "", sProcessInfo, gbDebug);

                }

            }

            public virtual XmlElement GetAdminMenu()
            {

                XmlElement oMenuRoot = null;
                // Dim oMenu As XmlElement
                // Dim oMenuMod As XmlElement
                // Dim oMenuItem As XmlElement
                // Dim oOldMenuItem As XmlElement
                string filePath = "";
                XmlElement oMenuElmt;

                // old
                // Dim oElmt As XmlElement
                // Dim oElmt1 As XmlElement
                // Dim oElmt2 As XmlElement
                // Dim oElmt3 As XmlElement
                // Dim oElmt4 As XmlElement

                // Dim ewLastCmd As String
                XmlElement oPageElmt;

                string sProcessInfo = "";
                try
                {
                    // TS - Removed as seems to cause an issue with popup image selects etc.
                    // seems the wrong place for this kind of cmd.
                    // ewLastCmd = myWeb.moSession("ewCmd")
                    // If ewLastCmd = "NormalMail" Then mcEwCmd = ewLastCmd

                    if (moPageXML.DocumentElement is null)
                    {
                        moPageXML.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                        oPageElmt = moPageXML.CreateElement("Page");
                        moPageXML.AppendChild(oPageElmt);
                        myWeb.GetRequestVariablesXml(ref oPageElmt);
                        myWeb.GetSettingsXml(ref oPageElmt);
                    }
                    else
                    {
                        oPageElmt = moPageXML.DocumentElement;
                    }


                    var aFolders = new ArrayList();
                    aFolders.Add("");

                    foreach (string folder in myWeb.maCommonFolders)
                        aFolders.Add(folder);

                    // Look for a base menu - first locally, then from common alternative folders
                    foreach (string folder in new ReverseIterator(aFolders))
                    {
                        filePath = folder.TrimEnd(@"/\".ToCharArray()) + "/Admin/AdminMenu.xml";
                        if (oMenuRoot is null)
                        {
                            oMenuRoot = Xml.loadElement(myWeb.goServer.MapPath(filePath), moPageXML);
                        }
                        else
                        {
                            XmlElement oTempMenuRoot;
                            XmlElement oElmt;
                            oTempMenuRoot = Xml.loadElement(myWeb.goServer.MapPath(filePath), moPageXML);
                            string currentCmd;
                            string parentCmd;
                            // add any new nodes
                            if (oTempMenuRoot != null)
                            {
                                foreach (XmlElement currentOElmt in oTempMenuRoot.SelectNodes("descendant-or-self::MenuItem[not(ancestor-or-self::MenuItem[@replacePath])]"))
                                {
                                    oElmt = currentOElmt;
                                    currentCmd = oElmt.GetAttribute("cmd");
                                    XmlElement oParentElmt = (XmlElement)oElmt.ParentNode;
                                    parentCmd = oParentElmt.GetAttribute("cmd");
                                    if (oMenuRoot.SelectSingleNode("descendant-or-self::MenuItem[@cmd ='" + currentCmd + "' ]") is null)
                                    {
                                        // we can't find it lets add it
                                        if (!(string.IsNullOrEmpty(oParentElmt.GetAttribute("cmd")) & oMenuRoot.SelectSingleNode("descendant-or-self::MenuItem[@cmd ='" + oParentElmt.GetAttribute("cmd") + "' ]") is null))
                                        {


                                            oMenuRoot.SelectSingleNode("descendant-or-self::MenuItem[@cmd ='" + parentCmd + "' ]").AppendChild(oElmt.CloneNode(true));
                                        }
                                        else
                                        {
                                            sProcessInfo += "cannot add " + currentCmd;
                                        }
                                    }
                                    else
                                    {
                                        sProcessInfo += "cannot add " + currentCmd;
                                    }
                                }

                                string currentModule;
                                string parentModule;
                                foreach (XmlElement currentOElmt1 in oTempMenuRoot.SelectNodes("descendant-or-self::Module[@jsonURL!='']"))
                                {
                                    oElmt = currentOElmt1;
                                    currentModule = oElmt.GetAttribute("name");
                                    XmlElement oParentElmt = (XmlElement)oElmt.ParentNode;
                                    parentModule = oParentElmt.GetAttribute("name");
                                    if (oMenuRoot.SelectSingleNode("descendant-or-self::Module[@name ='" + currentModule + "' ]") is null)
                                    {
                                        // we can't find it lets add it
                                        if (!(string.IsNullOrEmpty(oParentElmt.GetAttribute("name")) & oMenuRoot.SelectSingleNode("descendant-or-self::Module[@name ='" + oParentElmt.GetAttribute("name") + "' ]") is null))
                                        {
                                            if (oMenuRoot.SelectSingleNode("descendant-or-self::MenuItem[@name ='" + parentModule + "' ]") != null)
                                            {
                                                oMenuRoot.SelectSingleNode("descendant-or-self::MenuItem[@name ='" + parentModule + "' ]").AppendChild(oElmt.CloneNode(true));
                                            }
                                            else
                                            {
                                                oMenuRoot.FirstChild.AppendChild(oElmt.CloneNode(true));
                                            }
                                        }
                                        else
                                        {
                                            sProcessInfo += "cannot add " + currentModule;
                                        }
                                    }
                                    else
                                    {
                                        sProcessInfo += "cannot add " + currentModule;
                                    }
                                }

                                foreach (XmlElement currentOElmt2 in oTempMenuRoot.SelectNodes("descendant-or-self::MenuItem[@replacePath!='']"))
                                {
                                    oElmt = currentOElmt2;

                                    XmlElement oParentElmt = (XmlElement)oElmt.ParentNode;
                                    XmlElement oRepElmt = (XmlElement)oMenuRoot.SelectSingleNode("descendant-or-self::" + oElmt.GetAttribute("replacePath"));
                                    oRepElmt.ParentNode.ReplaceChild(oElmt, oRepElmt);

                                }
                            }
                        }
                        // If oMenuRoot IsNot Nothing Then Exit For
                    }

                    // TS believe this can be fully deprecated. 25-03-2020

                    // If oMenuRoot Is Nothing Then
                    // 'build the old way 
                    // oMenuRoot = moPageXML.CreateElement("AdminMenu")
                    // oElmt1 = appendMenuItem(oMenuRoot, "Admin Home", "AdmHome")
                    // oElmt2 = appendMenuItem(oElmt1, "Content", "Content")
                    // oElmt3 = appendMenuItem(oElmt2, "By Page", "ByPage")
                    // If mcEwCmd = "ByPage" Or mcEwCmd = "Normal" Or mcEwCmd = "AddContent" Or mcEwCmd = "AddModule" Or mcEwCmd = "ContentVersions" Or mcEwCmd = "RollbackContent" Or mcEwCmd = "RelateSearch" Or mcEwCmd = "EditContent" Or mcEwCmd = "Advanced" Or mcEwCmd = "EditPage" Or mcEwCmd = "EditPageLayout" Or mcEwCmd = "EditPagePermissions" Or mcEwCmd = "EditPageRights" Or mcEwCmd = "LocateContent" Or mcEwCmd = "LocateSearch" Or mcEwCmd = "MoveContent" Or mcEwCmd = "admin" Or mcEwCmd = "AddScheduledItem" Then
                    // oElmt4 = appendMenuItem(oElmt3, "Normal Mode", "Normal", myWeb.mnPageId)
                    // appendMenuItem(oElmt4, "Edit Content", "EditContent", myWeb.mnPageId, False)
                    // appendMenuItem(oElmt4, "Add Module", "AddModule", myWeb.mnPageId, False)
                    // appendMenuItem(oElmt4, "Add Content", "AddContent", myWeb.mnPageId, False)
                    // appendMenuItem(oElmt4, "Move Content", "MoveContent", myWeb.mnPageId, False)
                    // appendMenuItem(oElmt4, "Locate Content", "LocateContent", myWeb.mnPageId, False)
                    // appendMenuItem(oElmt4, "Relate Search", "RelateSearch", myWeb.mnPageId, False)
                    // appendMenuItem(oElmt4, "Locate Search", "LocateSearch", myWeb.mnPageId, False)
                    // appendMenuItem(oElmt4, "Content Versions", "ContentVersions", myWeb.mnPageId, False)
                    // appendMenuItem(oElmt4, "Rollback Content", "RollbackContent", myWeb.mnPageId, False)
                    // appendMenuItem(oElmt3, "Advanced Mode", "Advanced", myWeb.mnPageId)
                    // appendMenuItem(oElmt3, "Page Settings", "EditPage", myWeb.mnPageId)
                    // appendMenuItem(oElmt3, "Page Layout", "EditPageLayout", myWeb.mnPageId)
                    // If moConfig("Membership") = "on" Then
                    // appendMenuItem(oElmt3, "Permissions", "EditPagePermissions", myWeb.mnPageId)
                    // appendMenuItem(oElmt3, "Rights", "EditPageRights", myWeb.mnPageId)
                    // End If
                    // appendMenuItem(oElmt3, "Preview", "PreviewOn", myWeb.mnPageId)
                    // End If
                    // oElmt3 = appendMenuItem(oElmt2, "Edit Menu", "EditStructure")
                    // appendMenuItem(oElmt3, "Move Page", "MovePage", myWeb.mnPageId, False)
                    // appendMenuItem(oElmt3, "Add Page", "AddPage", myWeb.mnPageId, False)
                    // oElmt3 = appendMenuItem(oElmt2, "Resource Library", "ImageLib")
                    // If mcEwCmd = "Library" Or mcEwCmd = "ImageLib" Or mcEwCmd = "DocsLib" Or mcEwCmd = "MediaLib" Then
                    // appendMenuItem(oElmt3, "Image Library", "ImageLib")
                    // appendMenuItem(oElmt3, "Document Library", "DocsLib")
                    // appendMenuItem(oElmt3, "Media Library", "MediaLib")
                    // End If

                    // oElmt3 = appendMenuItem(oElmt2, "Web Settings", "WebSettings")
                    // appendMenuItem(oElmt3, "Select Skin", "SelectSkin")
                    // appendMenuItem(oElmt3, "Scheduled Items", "ScheduledItems")
                    // appendMenuItem(oElmt3, "System Pages", "SystemPages")
                    // appendMenuItem(oElmt3, "System Pages", "ViewSystemPages", , False)
                    // 'ViewSystemPages
                    // If moConfig("SiteSearch") = "on" Then
                    // appendMenuItem(oElmt3, "Index Site", "SiteIndex")
                    // End If
                    // ' oElmt3 = appendMenuItem(oElmt2, "By Type", "ByType")
                    // ' If mcEwCmd = "ByType" Then
                    // '    appendMenuItem(oElmt3, "ListAll", "List All Quizzes")
                    // '    appendMenuItem(oElmt3, "AddContent", "Add New Quiz")
                    // 'End If

                    // If moConfig("VersionControl") = "on" Then
                    // oElmt3 = appendMenuItem(oElmt2, "Awaiting Approval", "AwaitingApproval")
                    // End If

                    // If moConfig("Import") = "on" Then
                    // oElmt3 = appendMenuItem(oElmt2, "Import Files", "FileImport")
                    // End If

                    // ' RJP 21 Nov 2012 Added ImpersonateUser to list
                    // If moConfig("Membership") = "on" Then
                    // oElmt2 = appendMenuItem(oElmt1, "Membership", "ListGroups")
                    // If mcEwCmd = "ListUsers" Or mcEwCmd = "EditDirItem" Or mcEwCmd = "ListCompanies" Or mcEwCmd = "ListGroups" Or mcEwCmd = "ListRoles" Or mcEwCmd = "ListMailingLists" Or mcEwCmd = "Permissions" Or mcEwCmd = "DirPermissions" _
                    // Or mcEwCmd = "Subscriptions" Or mcEwCmd = "AddSubscriptionGroup" Or mcEwCmd = "EditSubscriptionGroup" Or mcEwCmd = "AddSubscription" Or mcEwCmd = "EditSubscription" Or mcEwCmd = "MoveSubscription" Or mcEwCmd = "LocateSubscription" Or mcEwCmd = "UpSubscription" Or mcEwCmd = "DownSubscription" _
                    // Or mcEwCmd = "MemberActivity" Or mcEwCmd = "MemberCodes" Or mcEwCmd = "DeleteDirItem" Or mcEwCmd = "DirPermissions" Or mcEwCmd = "ResetUserPwd" Or mcEwCmd = "MaintainRelations" Or mcEwCmd = "ListUserContacts" Or mcEwCmd = "AddUserContact" Or mcEwCmd = "EditUserContact" Or mcEwCmd = "ImpersonateUser" Then
                    // appendMenuItem(oElmt2, "Groups", "ListGroups")
                    // appendMenuItem(oElmt2, "All Users", "ListUsers")
                    // appendMenuItem(oElmt2, "Roles", "ListRoles")
                    // appendMenuItem(oElmt2, "Edit Item", "EditDirItem", , False)
                    // ' RJP 21 Nov 2012 Added ImpersonateUser to list
                    // appendMenuItem(oElmt2, "Impersonate User", "ImpersonateUser", , False)
                    // appendMenuItem(oElmt2, "ResetPwd", "ResetUserPwd", , False)
                    // appendMenuItem(oElmt2, "DirPermissions", "DirPermissions", , False)
                    // appendMenuItem(oElmt2, "Maintain Relations", "MaintainRelations", , False)
                    // appendMenuItem(oElmt2, "ListUserContacts", "ListUserContacts", , False)
                    // appendMenuItem(oElmt2, "AddUserContact", "AddUserContact", , False)
                    // appendMenuItem(oElmt2, "EditUserContact", "EditUserContact", , False)
                    // 'appendMenuItem(oElmt2, "Companies", "ListCompanies")
                    // appendMenuItem(oElmt2, "Access Permission", "Permissions")
                    // If moConfig("Subscriptions") = "on" Then
                    // oElmt3 = appendMenuItem(oElmt2, "Subscriptions", "Subscriptions")
                    // End If
                    // If moConfig("ActivityReporting") = "on" Then
                    // appendMenuItem(oElmt2, "Activity Reporting", "MemberActivity")
                    // End If
                    // If moConfig("MemberCodes") = "on" Then
                    // appendMenuItem(oElmt2, "Member Codes", "MemberCodes")
                    // End If
                    // End If

                    // If moConfig("MailingList") = "on" Then
                    // oElmt2 = appendMenuItem(oElmt1, "Mailing List", "MailingList")
                    // If mcEwCmd = "MailingList" Or mcEwCmd = "NewMail" Or mcEwCmd = "AdvancedMail" Or mcEwCmd = "NormalMail" Or mcEwCmd = "MailHistory" Or mcEwCmd = "ProcessMailbox" Or mcEwCmd = "MailOptOut" Or mcEwCmd = "PreviewMail" Or mcEwCmd = "AddContentMail" Or mcEwCmd = "NormalMail" Or mcEwCmd = "AdvancedMail" Or mcEwCmd = "EditMail" Or mcEwCmd = "EditMailLayout" Or mcEwCmd = "PreviewMail" Or mcEwCmd = "SendMail" Or mcEwCmd = "AddContentMail" Then
                    // oElmt3 = appendMenuItem(oElmt2, "Mail Items", "MailingList")
                    // If mcEwCmd = "NormalMail" Or mcEwCmd = "AdvancedMail" Or mcEwCmd = "EditMail" Or mcEwCmd = "EditMailLayout" Or mcEwCmd = "PreviewMail" Or mcEwCmd = "SendMail" Or mcEwCmd = "AddContentMail" Then
                    // appendMenuItem(oElmt3, "Normal Mode", "NormalMail", myWeb.mnPageId)
                    // appendMenuItem(oElmt3, "Advanced Mode", "AdvancedMail", myWeb.mnPageId)
                    // appendMenuItem(oElmt3, "Mail Settings", "EditMail", myWeb.mnPageId)
                    // appendMenuItem(oElmt3, "Mail Layout", "EditMailLayout", myWeb.mnPageId)
                    // appendMenuItem(oElmt3, "Send Preview", "PreviewMail", myWeb.mnPageId)
                    // appendMenuItem(oElmt3, "Send Mail", "SendMail", myWeb.mnPageId)
                    // End If
                    // appendMenuItem(oElmt2, "History", "MailHistory")
                    // 'appendMenuItem(oElmt2, "ProcessMailbox", "ProcessMailbox")
                    // appendMenuItem(oElmt2, "Opt-Out", "MailOptOut")
                    // End If
                    // End If
                    // Else
                    // oElmt2 = appendMenuItem(oElmt1, "Membership", "ListGroups")
                    // If mcEwCmd = "ListUsers" Or mcEwCmd = "EditDirItem" Or mcEwCmd = "ListCompanies" Or mcEwCmd = "ListGroups" Or mcEwCmd = "ListRoles" Or mcEwCmd = "ListMailingLists" Or mcEwCmd = "Permissions" Or mcEwCmd = "DirPermissions" Then
                    // appendMenuItem(oElmt2, "All Users", "ListUsers")
                    // appendMenuItem(oElmt2, "Roles", "ListRoles")
                    // End If
                    // End If
                    // If moConfig("Cart") = "on" Or moConfig("Quote") = "on" Then
                    // oElmt2 = appendMenuItem(oElmt1, "Ecommerce", "Ecommerce")
                    // If moConfig("Cart") = "on" Then
                    // oElmt3 = appendMenuItem(oElmt2, "Orders", "Orders")
                    // If mcEwCmd = "Orders" Or mcEwCmd = "OrdersShipped" Or mcEwCmd = "OrdersAwaitingPayment" Or mcEwCmd = "OrdersFailed" Or mcEwCmd = "OrdersDeposit" Or mcEwCmd = "OrdersHistory" Then
                    // appendMenuItem(oElmt3, "New Sales", "Orders")
                    // appendMenuItem(oElmt3, "Awaiting Payment", "OrdersAwaitingPayment")
                    // appendMenuItem(oElmt3, "Shipped", "OrdersShipped")
                    // appendMenuItem(oElmt3, "Failed Transactions", "OrdersFailed")
                    // appendMenuItem(oElmt3, "Deposit Paid", "OrdersDeposit")
                    // appendMenuItem(oElmt3, "History", "OrdersHistory")
                    // End If
                    // End If
                    // If moConfig("Quote") = "on" Then
                    // oElmt3 = appendMenuItem(oElmt2, "Quotes", "Quotes")
                    // If mcEwCmd = "Quotes" Or mcEwCmd = "QuotesFailed" Or mcEwCmd = "QuotesDeposit" Or mcEwCmd = "QuotesHistory" Then
                    // appendMenuItem(oElmt3, "New Sales", "Quotes")
                    // appendMenuItem(oElmt3, "Failed Transactions", "QuotesFailed")
                    // appendMenuItem(oElmt3, "Deposit Paid", "QuotesDeposit")
                    // appendMenuItem(oElmt3, "History", "QuotesHistory")
                    // End If
                    // End If


                    // oElmt3 = appendMenuItem(oElmt2, "Shipping Locations", "ShippingLocations")
                    // oElmt3 = appendMenuItem(oElmt2, "Delivery Methods", "DeliveryMethods")


                    // oElmt3 = appendMenuItem(oElmt2, "Discounts", "Discounts")
                    // If mcEwCmd = "Discounts" Or mcEwCmd = "ProductGroups" Or mcEwCmd = "DiscountRules" _
                    // Or mcEwCmd = "DiscountGroupRelations" Or mcEwCmd = "EditProductGroups" Or mcEwCmd = "AddProductGroupsProduct" _
                    // Or mcEwCmd = "AddDiscountRules" Or mcEwCmd = "EditDiscountRules" Or mcEwCmd = "ApplyDirDiscountRules" _
                    // Or mcEwCmd = "ApplyGrpDiscountRules" Then
                    // appendMenuItem(oElmt3, "Product Groups", "ProductGroups")
                    // appendMenuItem(oElmt3, "Discount Rules", "DiscountRules")
                    // 'These are all hidden menu items
                    // 'the menu builder looks for cmdn to match to display the menu
                    // appendMenuItem(oElmt3, "Discounts", "Discounts", , False)
                    // appendMenuItem(oElmt3, "ProductGroups", "ProductGroups", , False)
                    // appendMenuItem(oElmt3, "DiscountRules", "DiscountRules", , False)
                    // appendMenuItem(oElmt3, "DiscountGroupRelations", "DiscountGroupRelations", , False)
                    // appendMenuItem(oElmt3, "EditProductGroups", "EditProductGroups", , False)
                    // appendMenuItem(oElmt3, "AddProductGroupsProduct", "AddProductGroupsProduct", , False)
                    // appendMenuItem(oElmt3, "AddDiscountRules", "AddDiscountRules", , False)
                    // appendMenuItem(oElmt3, "EditDiscountRules", "EditDiscountRules", , False)
                    // appendMenuItem(oElmt3, "ApplyDirDiscountRules", "ApplyDirDiscountRules", , False)
                    // appendMenuItem(oElmt3, "ApplyGrpDiscountRules", "ApplyGrpDiscountRules", , False)
                    // appendMenuItem(oElmt3, "AddProductGroups", "AddProductGroups", , False)
                    // End If

                    // 'oElmt3 = appendMenuItem(oElmt2, "Reports", "CartReports")
                    // 'oElmt3 = appendMenuItem(oElmt2, "Settings", "CartSettings")
                    // 'If mcEwCmd = "CartSettings" Or mcEwCmd = "PaymentProviders" Or mcEwCmd = "CartTandC" Or mcEwCmd = "ProductCategories" Or mcEwCmd = "CartDiscounts" Then
                    // '    appendMenuItem(oElmt3, "Payment Providers", "PaymentProviders")
                    // '    appendMenuItem(oElmt3, "Terms & Conditions", "CartTandC")
                    // '    appendMenuItem(oElmt3, "Product Categories", "ProductCategories")
                    // '    appendMenuItem(oElmt3, "Discounts", "CartDiscounts")
                    // 'End If

                    // 'Cart Settings
                    // oElmt3 = appendMenuItem(oElmt2, "Settings", "CartSettings")
                    // If mcEwCmd = "CartSettings" Or mcEwCmd = "PaymentProviders" Or mcEwCmd = "editProvider" Then
                    // appendMenuItem(oElmt3, "General Settings", "CartSettings")
                    // appendMenuItem(oElmt3, "Payment Providers", "PaymentProviders")
                    // End If

                    // If moConfig("Sync") = "on" Then
                    // oElmt3 = appendMenuItem(oElmt2, "Synchronisation", "Sync")
                    // End If
                    // End If

                    // 'Cart Reports
                    // oElmt3 = appendMenuItem(oElmt2, "Reports", "CartReportsMain")
                    // appendMenuItem(oElmt3, "Order Download", "CartDownload")
                    // appendMenuItem(oElmt3, "Sales by Product", "CartReports")
                    // appendMenuItem(oElmt3, "Sales by Page", "CartActivityDrilldown")
                    // appendMenuItem(oElmt3, "Sales by Period", "CartActivityPeriod")

                    // oElmt2 = appendMenuItem(oElmt1, "Reports", "Reports")
                    // 'oElmt3 = appendMenuItem(oElmt2, "By Company", "RptCompanies")
                    // 'oElmt3 = appendMenuItem(oElmt2, "Courses", "RptCourses")
                    // 'oElmt3 = appendMenuItem(oElmt2, "All Certificates", "RptCertificates")
                    // 'oElmt3 = appendMenuItem(oElmt2, "All Exam Activity", "RptExamActivity")
                    // 'oElmt3 = appendMenuItem(oElmt2, "All Page Activity", "RptPageActivity")
                    // 'oElmt3 = appendMenuItem(oElmt2, "Company Activity", "RptCompActivity")

                    // End If

                    // Add any options in Manifests


                    // Remove non-licenced features
                    foreach (XmlElement currentOMenuElmt in oMenuRoot.SelectNodes("descendant-or-self::MenuItem[@feature!='']"))
                    {
                        oMenuElmt = currentOMenuElmt;
                        string cFeature = oMenuElmt.GetAttribute("feature");
                        if (!myWeb.Features.ContainsKey(cFeature))
                        {
                            oMenuElmt.ParentNode.RemoveChild(oMenuElmt);
                        }
                    }

                    // Remove any options by role

                    SupplimentalAdminMenu(ref oMenuRoot);

                    System.Collections.Specialized.NameValueCollection moMailConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/mailinglist");
                    string sMessagingProvider = "";
                    if (moMailConfig != null)
                    {
                        sMessagingProvider = moMailConfig["MessagingProvider"];
                    }
                    Protean.Providers.Messaging.ReturnProvider RetProv = new Protean.Providers.Messaging.ReturnProvider();
                    IMessagingProvider moMessaging = RetProv.Get(ref myWeb, sMessagingProvider);
                    moMessaging.AdminProcess.MailingListAdminMenu(ref oMenuRoot);
                    moMessaging = null;

                    // If this is a cloned page, then remove certain options under By Page
                    if (Cms.gbClone && moPageXML.DocumentElement.SelectSingleNode("//MenuItem[@id = /Page/@id and (@clone > 0 or (@cloneparent='" + myWeb.mnCloneContextPageId + "' and @cloneparent > 0 ))]") != null)
                    {
                        if (NodeState(ref oMenuRoot, "//MenuItem[@cmd='ByPage']") != XmlNodeState.NotInstantiated)
                        {
                            XmlElement oByPage = (XmlElement)oMenuRoot.SelectSingleNode("//MenuItem[@cmd='ByPage']");
                            foreach (XmlElement currentOMenuElmt1 in oByPage.SelectNodes("MenuItem[not(@cmd='Normal' or @cmd='Advanced')]"))
                            {
                                oMenuElmt = currentOMenuElmt1;
                                if (!((oMenuElmt.GetAttribute("cmd") == "EditPage" | oMenuElmt.GetAttribute("cmd") == "EditPagePermissions") & myWeb.mbIsClonePage))

                                    oMenuElmt.SetAttribute("display", "false");
                            }
                            // <MenuItem name="Page Settings" cmd="EditPage" pgid="1" display="true"/>
                            oMenuElmt = addElement(ref oByPage, "MenuItem");
                            oMenuElmt.SetAttribute("name", "Go to master page");
                            oMenuElmt.SetAttribute("cmd", "GoToClone");
                            oMenuElmt.SetAttribute("display", "true");
                        }
                    }
                    if (oPageElmt.SelectSingleNode("AdminMenu") is null)
                    {
                        oPageElmt.AppendChild(oMenuRoot);
                    }
                    else
                    {
                        oPageElmt.ReplaceChild(oMenuRoot, oPageElmt.SelectSingleNode("AdminMenu"));
                    }

                    return oMenuRoot;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "GetAdminMenu", ex, "", sProcessInfo, gbDebug);
                    return null;
                }

            }

            public void GetPreviewMenu()
            {
                XmlElement oElmt;
                XmlElement oElmt1;
                XmlElement oElmt2;
                string sProcessInfo = "";
                try
                {
                    oElmt = moPageXML.CreateElement("PreviewMenu");

                    appendMenuItem(ref oElmt, "Return to Admin", "PreviewOff", (long)myWeb.mnPageId, true);
                    oElmt1 = appendMenuItem(ref oElmt, "Date", "", (long)myWeb.mnPageId, true);
                    oElmt2 = appendMenuItem(ref oElmt, "User", "", (long)myWeb.mnPageId, true);
                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(myWeb.moSession["PreviewUser"], 0, false)))
                    {
                        oElmt1.SetAttribute("username", "Anonymous");
                    }
                    else
                    {
                        XmlElement localgetUserXMLById() { var tmp2 = myWeb.moSession; int argnUserId = Conversions.ToInteger(tmp2["PreviewUser"]); var ret = myWeb.moDbHelper.getUserXMLById(ref argnUserId); tmp2["PreviewUser"] = (object)argnUserId; return ret; }

                        oElmt1.SetAttribute("username", localgetUserXMLById().GetAttribute("name"));
                    }
                    oElmt2.SetAttribute("date", Conversions.ToString(Interaction.IIf(Information.IsDate(myWeb.moSession["PreviewDate"]), myWeb.moSession["PreviewDate"], (object)DateTime.Now.Date)));

                    // also need to add an xform for the group and the date

                    // get the origional user details
                    XmlElement localgetUserXMLById1() { var tmp3 = myWeb.moSession; int argnUserId1 = Conversions.ToInteger(tmp3["nUserId"]); var ret = myWeb.moDbHelper.getUserXMLById(ref argnUserId1); tmp3["nUserId"] = (object)argnUserId1; return ret; }

                    oElmt.AppendChild(localgetUserXMLById1());
                    moPageXML.DocumentElement.AppendChild(oElmt);
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "GetpreviewMenu", ex, "", sProcessInfo, gbDebug);
                }

            }

            public XmlElement appendMenuItem(ref XmlElement oRootElmt, string cName, string cCmd, long pgid = 0L, bool display = true)
            {

                XmlElement oElmt;
                string sProcessInfo = "";
                try
                {

                    oElmt = moPageXML.CreateElement("MenuItem");
                    oElmt.SetAttribute("name", cName);
                    oElmt.SetAttribute("cmd", cCmd);
                    if (pgid != 0L)
                    {
                        oElmt.SetAttribute("pgid", pgid.ToString());
                    }
                    if (display)
                    {
                        oElmt.SetAttribute("display", "true");
                    }
                    else
                    {
                        oElmt.SetAttribute("display", "false");
                    }
                    oRootElmt.AppendChild(oElmt);

                    return oElmt;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "appendMenuItem", ex, "", sProcessInfo, gbDebug);
                    return null;
                }
            }


            private void LibProcess(ref XmlElement oPageDetail, ref string sAdminLayout, Protean.fsHelper.LibraryType LibType)
            {
                string sProcessInfo = "";
                try
                {

                    var oFsh = new Protean.fsHelper();
                    oFsh.initialiseVariables(LibType);
                    oFsh.moPageXML = moPageXML;

                    if (!string.IsNullOrEmpty(myWeb.moRequest["targetForm"]))
                        myWeb.moSession["targetForm"] = myWeb.moRequest["targetForm"];
                    string sTargetForm = Conversions.ToString(myWeb.moSession["targetForm"]);

                    if (!string.IsNullOrEmpty(myWeb.moRequest["targetField"]))
                        myWeb.moSession["targetField"] = myWeb.moRequest["targetField"];
                    string sTargetField = Conversions.ToString(myWeb.moSession["targetField"]);

                    if (!string.IsNullOrEmpty(myWeb.moRequest["targetClass"]))
                        myWeb.moSession["targetClass"] = myWeb.moRequest["targetClass"];
                    string sTargetClass = Conversions.ToString(myWeb.moSession["targetClass"]);

                    bool bShowTree = false;
                    string sFolder = myWeb.goServer.UrlDecode(myWeb.moRequest["fld"]);

                    if (string.IsNullOrEmpty(sFolder))
                    {
                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(myWeb.moSession[((int)LibType).ToString() + "-path"], "", false)))
                        {
                            sFolder = Conversions.ToString(myWeb.moSession[((int)LibType).ToString() + "-path"]);
                        }
                    }
                    else if (sFolder.Contains("[yyyy-mm]"))
                    {
                        sFolder = sFolder.Replace("[yyyy-mm]", DateTime.Now.Year.ToString("D4") + "-" + DateTime.Now.Month.ToString("D2"));
                        var oFs = new Protean.fsHelper(myWeb.moCtx);
                        oFs.initialiseVariables(LibType);
                        oFs.CreatePath(sFolder);
                        oFs = (Protean.fsHelper)null;
                        myWeb.moPageXml.SelectSingleNode("/Page/Request/QueryString/Item[@name='fld']").InnerText = sFolder;
                    }

                    else
                    {
                        myWeb.moSession[((int)LibType).ToString() + "-path"] = sFolder;
                    }

                    string sFile = myWeb.moRequest["file"];

                    switch (myWeb.moRequest["ewCmd2"] ?? "")
                    {
                        case "addFolder":
                            {
                                oPageDetail.AppendChild(moAdXfm.xFrmAddFolder(ref sFolder, LibType));
                                if (moAdXfm.valid == false)
                                {
                                    sAdminLayout = "AdminXForm";
                                }
                                else
                                {
                                    bShowTree = true;
                                }

                                break;
                            }
                        case "upload":
                            {
                                oPageDetail.AppendChild(moAdXfm.xFrmUpload(sFolder, LibType));
                                if (moAdXfm.valid == false)
                                {
                                    sAdminLayout = "AdminXForm";
                                }
                                else
                                {
                                    bShowTree = true;
                                }

                                break;
                            }
                        case "uploads":
                            {
                                oPageDetail.AppendChild(moAdXfm.xFrmMultiUpload(sFolder, LibType));
                                if (moAdXfm.valid == false)
                                {
                                    sAdminLayout = "AdminXForm";
                                }
                                else
                                {
                                    bShowTree = true;
                                }

                                break;
                            }
                        case "deleteFolder":
                            {
                                // Dim parentFolder  'Never Used
                                oPageDetail.AppendChild(moAdXfm.xFrmDeleteFolder(ref sFolder, LibType));
                                if (moAdXfm.valid == false)
                                {
                                    sAdminLayout = "AdminXForm";
                                }
                                else
                                {
                                    myWeb.msRedirectOnEnd = "?ewCmd=" + LibType.ToString() + @"Lib&fld=\";
                                    bShowTree = true;
                                }

                                break;
                            }
                        case "deleteFile":
                            {
                                oPageDetail.AppendChild(moAdXfm.xFrmDeleteFile(sFolder, sFile, LibType));
                                if (moAdXfm.valid == false)
                                {
                                    sAdminLayout = "AdminXForm";
                                }
                                else
                                {
                                    bShowTree = true;
                                }

                                break;
                            }
                        case "moveFile":
                            {
                                oPageDetail.AppendChild(moAdXfm.xFrmMoveFile(sFolder, sFile, LibType));
                                if (moAdXfm.valid == false)
                                {
                                    sAdminLayout = "AdminXForm";
                                }
                                else
                                {
                                    bShowTree = true;
                                }

                                break;
                            }
                        case "pickImage":
                            {
                                string imagePath = Conversions.ToString(Interaction.IIf(sFolder.Replace(@"\", "/").EndsWith("/"), sFolder.Replace(@"\", "/") + sFile, sFolder + "/" + sFile));
                                oPageDetail.AppendChild(moAdXfm.xFrmPickImage(imagePath, sTargetForm, sTargetField, sTargetClass));
                                if (moAdXfm.valid == false)
                                {
                                    sAdminLayout = "AdminXForm";
                                }
                                else
                                {
                                    // close window / js
                                }

                                break;
                            }
                        case "pickDocument":
                            {
                                // all done in js
                                if (moAdXfm.valid == false)
                                {
                                    sAdminLayout = "AdminXForm";
                                }
                                else
                                {
                                    // close window / js
                                }

                                break;
                            }
                        case "editImage":
                            {
                                oPageDetail.AppendChild(moAdXfm.xFrmEditImage(myWeb.moRequest["imgHtml"], sTargetForm, sTargetField, ""));
                                if (moAdXfm.valid == false)
                                {
                                    sAdminLayout = "AdminXForm";
                                }
                                else
                                {
                                    // close window / js
                                }

                                break;
                            }
                        case "FolderSettings":
                            {
                                break;
                            }


                        case "FileUpload":
                            {
                                var oFS = new Protean.fsHelper(myWeb.moCtx);
                                oFS.UploadRequest(myWeb.moCtx);
                                oFS = (Protean.fsHelper)null;
                                // we want to not render anything else
                                myWeb.moResponseType = Cms.pageResponseType.flush;
                                break;
                            }

                        default:
                            {
                                bShowTree = true;
                                break;
                            }
                    }

                    if (bShowTree == true)
                    {
                        myWeb.PerfMon.Log("fsHelper", "getDirectoryTreeXml-Start");

                        oPageDetail.AppendChild(oFsh.getDirectoryTreeXml(LibType, sFolder));
                        myWeb.PerfMon.Log("fsHelper", "getDirectoryTreeXml-End");
                    }

                    oFsh = (Protean.fsHelper)null;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "LibProcess", ex, "", sProcessInfo, gbDebug);
                }
            }

            private void OrderProcess(ref XmlElement oPageDetail, ref string sAdminLayout, string cSchemaName)
            {
                string sProcessInfo = "";
                System.Collections.Specialized.NameValueCollection moCartConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/cart");

                try
                {
                    int nOrderStatus1 = 0;
                    int nOrderStatus2 = 0;
                    int nOrderStatus3 = 0;

                    if (mcEwCmd.Contains("Order") | mcEwCmd == "BulkCartAction")
                    {
                        var oCart = new Cms.Cart(ref myWeb);

                        object ewCmd2 = myWeb.moRequest["ewCmd2"];

                        switch (mcEwCmd ?? "")
                        {
                            case "BulkCartAction":
                                {
                                    switch (Strings.LCase(myWeb.moRequest["BulkAction"]) ?? "")
                                    {
                                        case "print":
                                            {
                                                ewCmd2 = "Print";
                                                sAdminLayout = "Print";
                                                break;
                                            }
                                        case "setinprogress":
                                            {

                                                string[] ids = Strings.Split(myWeb.moRequest["id"], ",");
                                                foreach (var id in ids)
                                                    myWeb.moDbHelper.ExeProcessSql("update tblCartOrder set nCartStatus = 17 where nCartOrderKey = " + id);
                                                mcEwCmd = "OrdersInProgress";
                                                break;
                                            }
                                        case "setshipped":
                                            {

                                                string[] ids = Strings.Split(myWeb.moRequest["id"], ",");
                                                foreach (var id in ids)
                                                    myWeb.moDbHelper.ExeProcessSql("update tblCartOrder set nCartStatus = 9 where nCartOrderKey = " + id);
                                                mcEwCmd = "OrdersShipped";
                                                break;
                                            }


                                    }

                                    break;
                                }
                        }

                        switch (ewCmd2)
                        {
                            case "Display":
                                {
                                    long nStatus;

                                    string sSql = "select nCartStatus from tblCartOrder WHERE nCartOrderKey =" + myWeb.moRequest["id"];
                                    nStatus = Conversions.ToLong(myWeb.moDbHelper.ExeProcessSqlScalar(sSql));

                                    oPageDetail.AppendChild(moAdXfm.xFrmUpdateOrder(Conversions.ToLong(myWeb.moRequest["id"]), cSchemaName));

                                    bool forceRefresh = false;
                                    // TS removed as we do not want to refresh the cart XML as it destroys discount info and order ref etc.
                                    // If myWeb.moRequest("nStatus") = 9 Then
                                    // forceRefresh = True
                                    // End If
                                    oCart.ListOrders(myWeb.moRequest["id"], true, 0, ref oPageDetail, forceRefresh, nUserId: 0L);

                                    // :TODO Behaviour to manage resending recipts.
                                    if (moCartConfig["SendRecieptsFromAdmin"] != "off")
                                    {
                                        if (moAdXfm.isSubmitted() & moAdXfm.valid)
                                        {
                                            if ((double)nStatus != Conversions.ToDouble(myWeb.moRequest["nStatus"]) & Conversions.ToDouble(myWeb.moRequest["nStatus"]) == (double)Cms.Cart.cartProcess.Complete)
                                            {
                                                oCart.mnCartId = Conversions.ToInteger(myWeb.moRequest["id"]);
                                                XmlElement argoCartElmt = (XmlElement)oPageDetail.LastChild.FirstChild;
                                                oCart.addDateAndRef(ref argoCartElmt);
                                                XmlElement argoCartElmt1 = (XmlElement)oPageDetail.LastChild;
                                                oCart.emailReceipts(ref argoCartElmt1);
                                            }
                                        }
                                    }

                                    break;
                                }

                            case "Print":
                                {

                                    var ofs = new Protean.fsHelper();
                                    myWeb.moResponseType = Cms.pageResponseType.pdf;
                                    myWeb.mcOutputFileName = "DeliveryNote.pdf";

                                    string DeliveryNoteXslPath = @"\xsl\docs\deliverynote.xsl";
                                    if (myWeb.bs5) {
                                        DeliveryNoteXslPath = @"\features\cart\docs\delivery-note.xsl";
                                    }

                                    myWeb.mcEwSiteXsl = ofs.checkCommonFilePath(moConfig["ProjectPath"] + DeliveryNoteXslPath);

                                    oCart.ListOrders(myWeb.moRequest["id"], true, 0, ref oPageDetail, bForceRefresh: false, nUserId: 0L);
                                    break;
                                }

                            case "PrintConfirm":
                                {
                                    break;
                                }


                            case "ResendReceipt":
                                {

                                    oCart.ListOrders(myWeb.moRequest["id"], true, 0, ref oPageDetail, bForceRefresh: false, nUserId: 0L);
                                    break;
                                }

                            case "RequestSettlement":
                                {
                                    oPageDetail.AppendChild(moAdXfm.xFrmRequestSettlement(Conversions.ToInteger(myWeb.moRequest["id"])));
                                    oPageDetail.AppendChild(myWeb.moDbHelper.ActivityReport(Cms.dbHelper.ActivityType.Email, 0L, 0L, 0L, Conversions.ToLong(myWeb.moRequest["id"])));
                                    break;
                                }

                            default:
                                {
                                    switch (mcEwCmd ?? "")
                                    {
                                        case "Orders":
                                            {
                                                oCart.ListOrders(0.ToString(), true, (int)Cms.Cart.cartProcess.Complete, ref oPageDetail);
                                                break;
                                            }
                                        case "OrdersInProgress":
                                            {
                                                oCart.ListOrders(0.ToString(), true, (int)Cms.Cart.cartProcess.InProgress, ref oPageDetail);
                                                break;
                                            }
                                        case "OrdersSaved":
                                            {
                                                oCart.ListOrders(0.ToString(), true, (int)Cms.Cart.cartProcess.Confirmed, ref oPageDetail);
                                                break;
                                            }
                                        case "OrdersAwaitingPayment":
                                            {
                                                oCart.ListOrders(0.ToString(), true, (int)Cms.Cart.cartProcess.AwaitingPayment, ref oPageDetail);
                                                break;
                                            }
                                        case "OrdersShipped":
                                            {
                                                oCart.ListOrders(0.ToString(), true, (int)Cms.Cart.cartProcess.Shipped, ref oPageDetail);
                                                break;
                                            }
                                        case "OrdersRefunded":
                                            {
                                                oCart.ListOrders(0.ToString(), true, (int)Cms.Cart.cartProcess.Refunded, ref oPageDetail);
                                                break;
                                            }
                                        case "OrdersAbandoned":
                                            {
                                                oCart.ListOrders(0.ToString(), true, (int)Cms.Cart.cartProcess.Abandoned, ref oPageDetail);
                                                break;
                                            }
                                        case "OrdersFailed":
                                            {
                                                oCart.ListOrders(0.ToString(), true, (int)Cms.Cart.cartProcess.PassForPayment, ref oPageDetail);
                                                break;
                                            }
                                        case "OrdersDeposit":
                                            {
                                                oCart.ListOrders(0.ToString(), true, (int)Cms.Cart.cartProcess.DepositPaid, ref oPageDetail);
                                                break;
                                            }
                                        case "OrdersHistory":
                                            {
                                                oCart.ListOrders(0.ToString(), true, 0, ref oPageDetail, bForceRefresh: false, nUserId: 0L);
                                                break;
                                            }
                                    }

                                    break;
                                }
                        }
                        sAdminLayout = cSchemaName + "s";
                    }
                    else if (myWeb.moRequest["ewCmd"].Contains("Quote"))
                    {
                        var oQuote = new Cms.Quote(ref myWeb);

                        switch (myWeb.moRequest["ewCmd2"] ?? "")
                        {

                            case "Display":
                                {


                                    oPageDetail.AppendChild(moAdXfm.xFrmUpdateOrder(Conversions.ToLong(myWeb.moRequest["id"]), cSchemaName));
                                    oQuote.ListOrders(myWeb.moRequest["id"], true, 0, ref oPageDetail, bForceRefresh: false, nUserId: 0L);
                                    break;
                                }

                            default:
                                {
                                    switch (myWeb.moRequest["ewCmd"] ?? "")
                                    {
                                        case "Quotes":
                                            {
                                                oQuote.ListOrders(0.ToString(), true, (int)Cms.Cart.cartProcess.Complete, ref oPageDetail);
                                                break;
                                            }
                                        case "QuotesShipped":
                                            {
                                                oQuote.ListOrders(0.ToString(), true, (int)Cms.Cart.cartProcess.Shipped, ref oPageDetail);
                                                break;
                                            }
                                        case "QuotesRefunded":
                                            {
                                                oQuote.ListOrders(0.ToString(), true, (int)Cms.Cart.cartProcess.Refunded, ref oPageDetail);
                                                break;
                                            }
                                        case "QuotesAbandoned":
                                            {
                                                oQuote.ListOrders(0.ToString(), true, (int)Cms.Cart.cartProcess.Abandoned, ref oPageDetail);
                                                break;
                                            }
                                        case "QuotesFailed":
                                            {
                                                oQuote.ListOrders(0.ToString(), true, (int)Cms.Cart.cartProcess.PassForPayment, ref oPageDetail);
                                                break;
                                            }
                                        case "QuotesDeposit":
                                            {
                                                oQuote.ListOrders(0.ToString(), true, (int)Cms.Cart.cartProcess.DepositPaid, ref oPageDetail);
                                                break;
                                            }
                                        case "QuotesHistory":
                                            {
                                                oQuote.ListOrders(0.ToString(), true, 0, ref oPageDetail, bForceRefresh: false, nUserId: 0L);
                                                break;
                                            }
                                    }

                                    break;
                                }
                        }
                        sAdminLayout = cSchemaName + "s";
                    }
                    else if (myWeb.moRequest["ewCmd"] == "CartActivity" | myWeb.moRequest["ewCmd"] == "CartReports")
                    {
                        var oCart = new Cms.Cart(ref myWeb);
                        oPageDetail.AppendChild(moAdXfm.xFrmCartActivity());
                        if (moAdXfm.valid)
                        {
                            oPageDetail.AppendChild(oCart.CartReports(Conversions.ToDate(moAdXfm.Instance.FirstChild.SelectSingleNode("dBegin").InnerText), Conversions.ToDate(moAdXfm.Instance.FirstChild.SelectSingleNode("dEnd").InnerText), Conversions.ToInteger(moAdXfm.Instance.FirstChild.SelectSingleNode("bSplit").InnerText), moAdXfm.Instance.FirstChild.SelectSingleNode("cProductType").InnerText, Conversions.ToInteger(moAdXfm.Instance.FirstChild.SelectSingleNode("nProductId").InnerText), moAdXfm.Instance.FirstChild.SelectSingleNode("cCurrencySymbol").InnerText, moAdXfm.Instance.FirstChild.SelectSingleNode("nOrderStatus").InnerText, moAdXfm.Instance.FirstChild.SelectSingleNode("cOrderType").InnerText));
                        }
                        sAdminLayout = "CartActivity";
                    }
                    else if (myWeb.moRequest["ewCmd"] == "CartActivityDrilldown")
                    {
                        var oCart = new Cms.Cart(ref myWeb);
                        oPageDetail.AppendChild(moAdXfm.xFrmCartActivityDrillDown());
                        if (moAdXfm.valid)
                        {                           
                            string OrderSatus = Convert.ToString(moAdXfm.Instance.FirstChild.SelectSingleNode("nOrderStatus").InnerText);
                            if (OrderSatus.Contains(","))
                            {
                                string[] keys = OrderSatus.Split(',');
                                if (keys.Length > 0)
                                {
                                    nOrderStatus1 = Convert.ToInt32(keys[0]);
                                    nOrderStatus2 = Convert.ToInt32(keys[1]);
                                    nOrderStatus3 = Convert.ToInt32(keys[2]);
                                }
                            }
                            oPageDetail.AppendChild(oCart.CartReportsDrilldown(moAdXfm.Instance.FirstChild.SelectSingleNode("cGrouping").InnerText, Convert.ToInt32(moAdXfm.Instance.FirstChild.SelectSingleNode("nYear").InnerText), Convert.ToInt32(moAdXfm.Instance.FirstChild.SelectSingleNode("nMonth").InnerText), Convert.ToInt32(moAdXfm.Instance.FirstChild.SelectSingleNode("nDay").InnerText), moAdXfm.Instance.FirstChild.SelectSingleNode("cCurrencySymbol").InnerText, nOrderStatus1, nOrderStatus2, Convert.ToString(moAdXfm.Instance.FirstChild.SelectSingleNode("cOrderType").InnerText)));
                        }
                        sAdminLayout = "CartActivityDrilldown";
                    }
                    else if (myWeb.moRequest["ewCmd"] == "CartActivityPeriod")
                    {
                        var oCart = new Cms.Cart(ref myWeb);
                        oPageDetail.AppendChild(moAdXfm.xFrmCartActivityPeriod());
                        if (moAdXfm.valid)
                        {                           
                            oPageDetail.AppendChild(oCart.CartReportsPeriod(moAdXfm.Instance.FirstChild.SelectSingleNode("cGroup").InnerText, Convert.ToInt32(moAdXfm.Instance.FirstChild.SelectSingleNode("nYear").InnerText), Convert.ToInt32(moAdXfm.Instance.FirstChild.SelectSingleNode("nMonth").InnerText), Convert.ToInt32(moAdXfm.Instance.FirstChild.SelectSingleNode("nWeek").InnerText), moAdXfm.Instance.FirstChild.SelectSingleNode("cCurrencySymbol").InnerText, moAdXfm.Instance.FirstChild.SelectSingleNode("nOrderStatus").InnerText, Convert.ToString(moAdXfm.Instance.FirstChild.SelectSingleNode("cOrderType").InnerText)));
                        }
                        sAdminLayout = "CartActivityPeriod";
                    }
                    else if (myWeb.moRequest["ewCmd"] == "CartDownload")
                    {
                        var oCart = new Cms.Cart(ref myWeb);
                        oPageDetail.AppendChild(moAdXfm.xFrmCartOrderDownloads());
                        if (moAdXfm.valid)
                        {
                            oPageDetail.AppendChild(oCart.CartReportsDownload(Conversions.ToDate(moAdXfm.Instance.FirstChild.SelectSingleNode("dBegin").InnerText), Conversions.ToDate(moAdXfm.Instance.FirstChild.SelectSingleNode("dEnd").InnerText), moAdXfm.Instance.FirstChild.SelectSingleNode("cCurrencySymbol").InnerText, moAdXfm.Instance.FirstChild.SelectSingleNode("cOrderType").InnerText, Conversions.ToInteger(moAdXfm.Instance.FirstChild.SelectSingleNode("cOrderStage").InnerText)));
                        }
                        sAdminLayout = "CartDownload";
                    }

                    else if (myWeb.moRequest["ewCmd"] == "Ecommerce")
                    {
                        var oCart = new Cms.Cart(ref myWeb);
                        oPageDetail.AppendChild(oCart.CartOverview());
                    }
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "OrderProcess", ex, "", sProcessInfo, gbDebug);
                }
            }

            private void ShippingLocationsProcess(ref XmlElement oPageDetail, ref string sAdminLayout)
            {
                string sProcessInfo = "";
                Cms.Cart oCart;

                try
                {
                    oCart = new Cms.Cart(ref myWeb);

                    switch (myWeb.moRequest["ewCmd2"] ?? "")
                    {
                        case "edit":
                            {
                                oPageDetail.AppendChild(moAdXfm.xFrmEditShippingLocation(Conversions.ToLong(myWeb.moRequest["id"]), Conversions.ToLong(myWeb.moRequest["parid"])));
                                if (!moAdXfm.valid)
                                {
                                    sAdminLayout = "AdminXForm";
                                }
                                else
                                {
                                    oPageDetail.RemoveAll();
                                }

                                break;
                            }
                        case "movehere":
                            {
                                myWeb.moDbHelper.moveShippingLocation(Conversions.ToLong(myWeb.moRequest["id"]), Conversions.ToLong(myWeb.moRequest["parId"]));
                                break;
                            }
                        case "delete":
                            {
                                oPageDetail.AppendChild(moAdXfm.xFrmDeleteShippingLocation(Conversions.ToLong(myWeb.moRequest["id"])));
                                if (!moAdXfm.valid)
                                {
                                    sAdminLayout = "AdminXForm";
                                }
                                else
                                {
                                    oPageDetail.RemoveAll();
                                }

                                break;
                            }
                    }
                    if (string.IsNullOrEmpty(oPageDetail.InnerXml))
                    {
                        oCart.ListShippingLocations(ref oPageDetail);
                    }
                    oCart.close();
                    oCart = (Cms.Cart)null;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "ShippingLocationsProcess", ex, "", sProcessInfo, gbDebug);
                }
            }

            private void DeliveryMethodProcess(ref XmlElement oPageDetail, ref string sAdminLayout)
            {
                string sProcessInfo = "";
                Cms.Cart oCart;

                try
                {
                    oCart = new Cms.Cart(ref myWeb);

                    switch (myWeb.moRequest["ewCmd2"] ?? "")
                    {
                        case "edit":
                            {
                                oPageDetail.AppendChild(moAdXfm.xFrmEditDeliveryMethod(Conversions.ToLong(myWeb.moRequest["id"])));
                                if (!moAdXfm.valid)
                                {
                                    sAdminLayout = "AdminXForm";
                                }
                                else
                                {
                                    oPageDetail.RemoveAll();
                                }

                                break;
                            }
                        case "locations":
                            {
                                if (!string.IsNullOrEmpty(myWeb.moRequest["ewSubmit"]))
                                {
                                    myWeb.moDbHelper.updateShippingLocations(Conversions.ToLong(myWeb.moRequest["nShpOptId"]), myWeb.moRequest["aLocations"]);
                                }
                                else
                                {
                                    oCart.ListShippingLocations(ref oPageDetail, Conversions.ToLong("0" + myWeb.moRequest["id"]));
                                    sAdminLayout = "DeliveryMethodLocations";
                                }

                                break;
                            }
                        case "permissions":
                            {

                                sAdminLayout = "AdminXForm";
                                oPageDetail.AppendChild(moAdXfm.xFrmShippingDirRelations(Conversions.ToLong(myWeb.moRequest.QueryString["id"]), ""));
                                break;
                            }

                        case "ShippingGroup":
                            {
                                sAdminLayout = "AdminXForm";
                                oPageDetail.AppendChild(moAdXfm.xFrmProductShippingGroupRelations(Conversions.ToLong(myWeb.moRequest.QueryString["id"]), myWeb.moRequest.QueryString["name"]));
                                break;
                            }

                        case "delete":
                            {
                                // xFrmDeleteDeliveryMethod
                                oPageDetail.AppendChild(moAdXfm.xFrmDeleteDeliveryMethod(Conversions.ToLong(myWeb.moRequest["id"])));
                                if (!moAdXfm.valid)
                                {
                                    sAdminLayout = "AdminXForm";
                                }
                                else
                                {
                                    oPageDetail.RemoveAll();
                                    sAdminLayout = "DeliveryMethods";
                                }

                                break;
                            }
                    }
                    if (string.IsNullOrEmpty(oPageDetail.InnerXml))
                    {
                        oCart.ListDeliveryMethods(ref oPageDetail);
                    }
                    oCart.close();
                    oCart = (Cms.Cart)null;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "DeliveryMethodProcess", ex, "", sProcessInfo, gbDebug);
                }
            }

            private void CarriersProcess(ref XmlElement oPageDetail, ref string sAdminLayout)
            {
                string sProcessInfo = "";
                Cms.Cart oCart;

                try
                {
                    oCart = new Cms.Cart(ref myWeb);

                    switch (myWeb.moRequest["ewCmd2"] ?? "")
                    {
                        case "edit":
                            {
                                oPageDetail.AppendChild(moAdXfm.xFrmEditCarrier(Conversions.ToLong(myWeb.moRequest["id"])));
                                if (!moAdXfm.valid)
                                {
                                    sAdminLayout = "AdminXForm";
                                }
                                else
                                {
                                    oPageDetail.RemoveAll();
                                }

                                break;
                            }
                        case "delete":
                            {
                                // xFrmDeleteDeliveryMethod
                                oPageDetail.AppendChild(moAdXfm.xFrmDeleteCarrier(Conversions.ToLong(myWeb.moRequest["id"])));
                                if (!moAdXfm.valid)
                                {
                                    sAdminLayout = "AdminXForm";
                                }
                                else
                                {
                                    oPageDetail.RemoveAll();
                                    sAdminLayout = "Carriers";
                                }

                                break;
                            }
                    }
                    if (string.IsNullOrEmpty(oPageDetail.InnerXml))
                    {
                        oCart.ListCarriers(ref oPageDetail);
                        sAdminLayout = "Carriers";
                    }
                    oCart.close();
                    oCart = (Cms.Cart)null;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "CarriersProcess", ex, "", sProcessInfo, gbDebug);
                }
            }

            private void PaymentProviderProcess(ref XmlElement oPageDetail, ref string sAdminLayout)
            {
                string sProcessInfo = "";
                Cms.Cart oCart;

                try
                {
                    oCart = new Cms.Cart(ref myWeb);

                    switch (myWeb.moRequest["ewCmd2"] ?? "")
                    {
                        case "edit":
                        case "add":
                            {
                                oPageDetail.AppendChild(moAdXfm.xFrmPaymentProvider(myWeb.moRequest["type"]));
                                if (!moAdXfm.valid)
                                {
                                    sAdminLayout = "AdminXForm";
                                }
                                else
                                {
                                    oPageDetail.RemoveAll();
                                }

                                break;
                            }
                        case "delete":
                            {
                                // :TODO delete payment provider xform
                                oPageDetail.AppendChild(moAdXfm.xFrmDeletePaymentProvider(myWeb.moRequest["type"]));
                                if (!moAdXfm.valid)
                                {
                                    sAdminLayout = "AdminXForm";
                                }
                                else
                                {
                                    oPageDetail.RemoveAll();
                                }

                                break;
                            }
                    }
                    if (string.IsNullOrEmpty(oPageDetail.InnerXml))
                    {
                        oCart.ListPaymentProviders(ref oPageDetail);
                    }
                    oCart.close();
                    oCart = (Cms.Cart)null;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "DeliveryMethodProcess", ex, "", sProcessInfo, gbDebug);
                }
            }

            private void PollsProcess(ref XmlElement oPageDetail, ref string sAdminLayout)
            {
                string sProcessInfo = "";
                string reportName = "";
                long contentId = 0L;
                long logId = 0L;
                string pollTitle = "";
                DataSet votesDataset;
                string getVotesQuery;
                string updateVoteQuery;
                try
                {

                    contentId = (long)myWeb.GetRequestItemAsInteger("id");


                    if (contentId > 0L)
                    {

                        logId = (long)myWeb.GetRequestItemAsInteger("voteId");

                        // Vote based commands
                        if (logId > 0L)
                        {
                            switch (myWeb.moRequest["ewCmd2"] ?? "")
                            {
                                case "delete":
                                    {
                                        // :TODO delete payment provider xform

                                        if (!moAdXfm.valid)
                                        {
                                            sAdminLayout = "AdminXForm";
                                        }
                                        else
                                        {
                                            oPageDetail.RemoveAll();
                                        }

                                        break;
                                    }
                                case "hide":
                                    {
                                        updateVoteQuery = "UPDATE dbo.tblActivityLog " + "SET nActivityType=" + ((int)Cms.dbHelper.ActivityType.VoteExcluded).ToString() + " " + "WHERE nActivityKey = " + logId;

                                        myWeb.moDbHelper.ExeProcessSql(updateVoteQuery);
                                        break;
                                    }
                                case "show":
                                    {
                                        updateVoteQuery = "UPDATE dbo.tblActivityLog " + "SET nActivityType=" + ((int)Cms.dbHelper.ActivityType.SubmitVote).ToString() + " " + "WHERE nActivityKey = " + logId;

                                        myWeb.moDbHelper.ExeProcessSql(updateVoteQuery);
                                        break;
                                    }
                            }
                        }


                        // Poll report name
                        string pollXmlString = myWeb.moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.Content, contentId);
                        if (!string.IsNullOrEmpty(pollXmlString))
                        {
                            var pollXml = new XmlDocument();
                            pollXml.LoadXml(pollXmlString);
                            var argoNode = pollXml.DocumentElement;
                            Xml.NodeState(ref argoNode, "//Title", "", "", XmlNodeState.IsEmpty, null, "", pollTitle, bCheckTrimmedInnerText: false);
                        }
                        reportName = "Poll Votes: " + pollTitle;

                        // List the votes

                        getVotesQuery = "SELECT CASE WHEN nActivityType=" + ((int)Cms.dbHelper.ActivityType.SubmitVote).ToString() + " THEN 1 ELSE 0 END As Status, cContentSchemaName As contentSchema, nActivityKey As Vote_Id, cContentXmlBrief As Voted_For, dDateTime As Date_Voted, cActivityDetail As Voters_Email, cIPAddress As IP_Address " + "FROM dbo.tblActivityLog " + "LEFT JOIN dbo.tblContent ON nOtherId = nContentKey " + "WHERE nActivityType IN (" + ((int)Cms.dbHelper.ActivityType.SubmitVote).ToString() + "," + ((int)Cms.dbHelper.ActivityType.VoteExcluded).ToString() + ") AND nArtId = " + contentId;



                        votesDataset = myWeb.moDbHelper.GetDataSet(getVotesQuery, "Vote", "GenericReport");
                        myWeb.moDbHelper.ReturnNullsEmpty(ref votesDataset);

                        if (votesDataset.Tables.Count > 0)
                        {
                            votesDataset.Tables[0].Columns[1].ColumnMapping = MappingType.Attribute;
                            votesDataset.Tables[0].Columns[2].ColumnMapping = MappingType.Attribute;
                        }

                        var votesReport = new XmlDataDocument(votesDataset);
                        votesDataset.EnforceConstraints = false;

                        if (votesReport.FirstChild != null)
                        {


                            var reportElement = moPageXML.CreateElement("Content");
                            reportElement.SetAttribute("name", reportName);
                            reportElement.SetAttribute("type", "Report");

                            // Convert the content into xml
                            foreach (XmlElement voteXml in votesReport.SelectNodes("//Voted_For"))
                            {
                                string voteInnerXml = voteXml.InnerText;
                                try
                                {
                                    voteXml.InnerXml = voteInnerXml;
                                }
                                catch (Exception)
                                {
                                    voteXml.InnerText = voteInnerXml;
                                }
                            }

                            reportElement.InnerXml = votesReport.InnerXml;
                            oPageDetail.AppendChild(reportElement);

                        }

                    }

                    sAdminLayout = "ManagePollVotes";
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "PollsProcess", ex, "", sProcessInfo, gbDebug);
                }
            }

            private void ManageLookups(ref XmlElement oPageDetail, ref string sAdminLayout)
            {
                string sProcessInfo = "";
                string reportName = "Manage Lookups";
                //long contentId = 0L;
                string lookupId = null;
                string sSql;
                DataSet lookupsDataset;

                try
                {

                    if (myWeb.moRequest["lookupId"] != default)
                    {
                        lookupId = myWeb.moRequest["lookupId"];
                    }

                    switch (myWeb.moRequest["ewCmd2"] ?? "")
                    {
                        case "delete":
                            {
                                myWeb.moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.Lookup, Conversions.ToLong(lookupId));
                                if (moAdXfm.valid == false & myWeb.moRequest["ewCmd2"] == "delete")
                                {
                                    oPageDetail.InnerXml = "";
                                    lookupId = null;
                                    goto default;
                                }
                                goto default;

                            }
                        case "hide":
                            {
                                sSql = "UPDATE dbo.tblLookup " + "SET nActivityType=" + ((int)Cms.dbHelper.ActivityType.VoteExcluded).ToString() + " " + "WHERE nActivityKey = " + lookupId;

                                myWeb.moDbHelper.ExeProcessSql(sSql);
                                break;
                            }

                        case "show":
                            {
                                sSql = "UPDATE dbo.tblLookup " + "SET nActivityType=" + ((int)Cms.dbHelper.ActivityType.SubmitVote).ToString() + " " + "WHERE nActivityKey = " + lookupId;

                                myWeb.moDbHelper.ExeProcessSql(sSql);
                                break;
                            }
                        case "MoveUp":
                        case "MoveDown":
                        case "MoveTop":
                        case "MoveBottom":
                            {

                                myWeb.moDbHelper.ReorderNode(Cms.dbHelper.objectTypes.Lookup, Conversions.ToLong(lookupId), myWeb.moRequest["ewCmd2"], "cLkpCategory");
                                lookupId = null;
                                goto default;
                            }

                        default:
                            {
                                if (string.IsNullOrEmpty(lookupId))
                                {
                                    // list Lookup Lists
                                    sSql = "select nLkpId as id, * from tblLookup order by cLkpCategory, nDisplayOrder";

                                    lookupsDataset = myWeb.moDbHelper.GetDataSet(sSql, "Lookup", "Lookups");

                                    myWeb.moDbHelper.addTableToDataSet(ref lookupsDataset, "select distinct cLkpCategory as Name from tblLookup", "Category");

                                    myWeb.moDbHelper.ReturnNullsEmpty(ref lookupsDataset);

                                    if (lookupsDataset.Tables.Count > 0)
                                    {

                                        lookupsDataset.Tables[0].Columns["id"].ColumnMapping = MappingType.Attribute;
                                        lookupsDataset.Tables[1].Columns["Name"].ColumnMapping = MappingType.Attribute;

                                        // lookupsDataset.Tables(0).Columns(2).ColumnMapping = MappingType.Attribute
                                        lookupsDataset.Relations.Add("rel1", lookupsDataset.Tables[1].Columns["Name"], lookupsDataset.Tables[0].Columns["cLkpCategory"], false);
                                        lookupsDataset.Relations["rel1"].Nested = true;

                                        // lookupsDataset.Relations.Add("rel2", lookupsDataset.Tables(0).Columns("nLkpParent"), lookupsDataset.Tables(0).Columns("id"), False)
                                        // lookupsDataset.Relations("rel2").Nested = True
                                        lookupsDataset.EnforceConstraints = false;

                                    }



                                    var reportElement = moPageXML.CreateElement("Content");
                                    reportElement.SetAttribute("name", reportName);
                                    reportElement.SetAttribute("type", "Report");
                                    reportElement.InnerXml = lookupsDataset.GetXml();
                                    oPageDetail.AppendChild(reportElement);
                                }

                                else
                                {
                                    // lookupItem Xform
                                    oPageDetail.AppendChild(moAdXfm.xFrmLookup((int)Conversions.ToLong(lookupId), myWeb.moRequest["Category"], Conversions.ToLong(myWeb.moRequest["parentId"])));

                                    if (moAdXfm.valid)
                                    {
                                        oPageDetail.InnerXml = "";
                                        lookupId = null;
                                        goto default;
                                    }

                                }

                                break;
                            }

                    }


                    sAdminLayout = "ManageLookups";
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "PollsProcess", ex, "", sProcessInfo, gbDebug);
                }
            }


            private void FilterIndex(ref XmlElement oPageDetail, ref string sAdminLayout)
            {
                string sProcessInfo = "";
                string reportName = "Filter Indexes";
                //long contentId = 0L;
                string indexId = null;
                string sSql;
                string SchemaNameForUpdate;
                DataSet indexesDataset;

                try
                {
                    if (myWeb.moRequest["id"] != default)
                    {
                        indexId = myWeb.moRequest["id"];
                    }
                    switch (myWeb.moRequest["ewCmd2"] ?? "")
                    {
                        case "delete":
                            {
                                myWeb.moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.indexkey, Conversions.ToLong(indexId));
                                if (moAdXfm.valid == false & myWeb.moRequest["ewCmd2"] == "delete")
                                {
                                    oPageDetail.InnerXml = "";
                                    indexId = null;
                                    goto default;
                                }
                                goto default;

                            }
                        case "updateAllRules":
                            {
                                if (myWeb.moRequest["SchemaName"] != default)
                                {
                                    SchemaNameForUpdate = myWeb.moRequest["SchemaName"];
                                    sSql = "spScheduleToUpdateIndexTable";
                                    var arrParms = new Hashtable();
                                    arrParms.Add("SchemaName", SchemaNameForUpdate);
                                    myWeb.moDbHelper.ExeProcessSql(sSql, CommandType.StoredProcedure, arrParms);
                                    myWeb.moDbHelper.logActivity(Cms.dbHelper.ActivityType.SessionContinuation, (long)myWeb.mnUserId, 0L, 0L, 0L, "ReIndexing", true);
                                    if (moAdXfm.valid == false & myWeb.moRequest["ewCmd2"] == "update")
                                    {
                                        oPageDetail.InnerXml = "";
                                        indexId = null;
                                        goto default;
                                    }
                                }
                                goto default;

                            }
                        case "update":
                            {
                                if (myWeb.moRequest["SchemaName"] != default)
                                {
                                    SchemaNameForUpdate = myWeb.moRequest["SchemaName"];
                                    if (!string.IsNullOrEmpty(moConfig["FilterIndexITBCust"]))
                                    {
                                        sSql = "spUpdateFilterIndex";
                                        var arrParms = new Hashtable();
                                        arrParms.Add("SchemaName", SchemaNameForUpdate);
                                        arrParms.Add("IndexId", indexId);
                                        myWeb.moDbHelper.ExeProcessSql(sSql, CommandType.StoredProcedure, arrParms);
                                        myWeb.moDbHelper.logActivity(Cms.dbHelper.ActivityType.SessionContinuation, (long)myWeb.mnUserId, 0L, 0L, 0L, "ReIndexing", true);
                                        if (moAdXfm.valid == false & myWeb.moRequest["ewCmd2"] == "update")
                                        {
                                            oPageDetail.InnerXml = "";
                                            indexId = null;
                                            goto default;
                                        }
                                    }
                                    else
                                    {
                                        sSql = "spScheduleToUpdateIndexTable";
                                        var arrParms = new Hashtable();
                                        arrParms.Add("SchemaName", SchemaNameForUpdate);
                                        myWeb.moDbHelper.ExeProcessSql(sSql, CommandType.StoredProcedure, arrParms);
                                        myWeb.moDbHelper.logActivity(Cms.dbHelper.ActivityType.SessionContinuation, (long)myWeb.mnUserId, 0L, 0L, 0L, "ReIndexing", true);
                                        if (moAdXfm.valid == false & myWeb.moRequest["ewCmd2"] == "update")
                                        {
                                            oPageDetail.InnerXml = "";
                                            indexId = null;
                                            goto default;
                                        }
                                    }
                                }
                                goto default;

                            }
                        default:
                            {

                                if (string.IsNullOrEmpty(indexId))
                                {
                                    // list Lookup Lists
                                    sSql = @"select nContentIndexDefKey, CASE WHen nContentIndexDataType = 1 Then 'Int' when nContentIndexDataType=2 Then 'String' Else 'Date' End As nContentIndexDataType,
cContentSchemaName, cDefinitionName, cContentValueXpath, Case When bBriefNotDetail=0 Then 'false' Else 'True' End As bBriefNotDetail, nKeywordGroupName
from tblContentIndexDef";

                                    indexesDataset = myWeb.moDbHelper.GetDataSet(sSql, "indexkey", "indexkeys");

                                    myWeb.moDbHelper.addTableToDataSet(ref indexesDataset, "select distinct cContentSchemaName as Name from tblContentIndexDef", "SchemaName");

                                    // myWeb.moDbHelper.ReturnNullsEmpty(indexesDataset)

                                    if (indexesDataset.Tables.Count > 0)
                                    {

                                        indexesDataset.Tables[0].Columns["nContentIndexDefKey"].ColumnMapping = MappingType.Attribute;
                                        indexesDataset.Tables[1].Columns["Name"].ColumnMapping = MappingType.Attribute;

                                        // lookupsDataset.Tables(0).Columns(2).ColumnMapping = MappingType.Attribute
                                        indexesDataset.Relations.Add("rel1", indexesDataset.Tables[1].Columns["Name"], indexesDataset.Tables[0].Columns["cContentSchemaName"], false);
                                        indexesDataset.Relations["rel1"].Nested = true;

                                        // lookupsDataset.Relations.Add("rel2", lookupsDataset.Tables(0).Columns("nLkpParent"), lookupsDataset.Tables(0).Columns("id"), False)
                                        // lookupsDataset.Relations("rel2").Nested = True
                                        indexesDataset.EnforceConstraints = false;
                                    }
                                    var reportElement = moPageXML.CreateElement("Content");
                                    reportElement.SetAttribute("name", reportName);
                                    reportElement.SetAttribute("type", "Report");
                                    reportElement.InnerXml = indexesDataset.GetXml();
                                    oPageDetail.AppendChild(reportElement);
                                }

                                else
                                {
                                    // lookupItem Xform
                                    // oPageDetail.AppendChild(moAdXfm.xFrmIndexes(CLng(indexId), myWeb.moRequest("SchemaName")))
                                    oPageDetail.AppendChild(moAdXfm.xFrmIndexes((int)Conversions.ToLong(indexId), myWeb.moRequest["SchemaName"], myWeb.moRequest["parentId"]));

                                    if (moAdXfm.valid)
                                    {
                                        oPageDetail.InnerXml = "";
                                        indexId = null;
                                        goto default;
                                    }
                                }
                                break;
                            }
                    }
                    sAdminLayout = "FilterIndex";
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "PollsProcess", ex, "", sProcessInfo, gbDebug);
                }
            }



            private void ProductGroupsProcess(ref XmlElement oPageDetail, ref string sAdminLayout, int nGroupID = 0)
            {
                string sProcessInfo = "";
                sAdminLayout = "ProductGroups";
                string cSql;
                DataSet oDS;
                try
                {
                    cSql = "Select * From tblCartProductCategories";
                    oDS = myWeb.moDbHelper.GetDataSet(cSql, "ProductCategory", "ProductCategories");
                    if (oDS.Tables.Count == 1)
                    {
                        oDS.Tables["ProductCategory"].Columns.Add("Count", typeof(int));
                        oDS.Tables["ProductCategory"].Columns["Count"].ColumnMapping = MappingType.Attribute;
                        oDS.Tables["ProductCategory"].Columns["nCatKey"].ColumnMapping = MappingType.Attribute;
                        oDS.Tables["ProductCategory"].Columns["cCatSchemaName"].ColumnMapping = MappingType.Attribute;
                        oDS.Tables["ProductCategory"].Columns["cCatForeignRef"].ColumnMapping = MappingType.Attribute;
                    }
                    cSql = "SELECT c.nContentKey AS id, c.cContentForiegnRef AS ref, c.cContentName AS name, c.cContentSchemaName AS type, c.cContentXmlBrief AS content, tblCartCatProductRelations.nCatProductRelKey AS relid, tblCartCatProductRelations.nCatId AS catid FROM tblContent c INNER JOIN tblCartCatProductRelations ON c.nContentKey = tblCartCatProductRelations.nContentId " + "WHERE (tblCartCatProductRelations.nCatId Is not Null) order by nDisplayOrder";
                    myWeb.moDbHelper.addTableToDataSet(ref oDS, cSql, "Content");

                    if (oDS.Tables.Count == 2)
                    {

                        if (oDS.Tables["Content"].Columns.Contains("parID"))
                        {
                            oDS.Tables["Content"].Columns["parId"].ColumnMapping = MappingType.Attribute;
                        }
                        foreach (DataColumn oDC in oDS.Tables["Content"].Columns)
                        {
                            if (!(oDC.ColumnName == "content"))
                                oDC.ColumnMapping = MappingType.Attribute;
                        }
                        oDS.Tables["Content"].Columns["content"].ColumnMapping = MappingType.SimpleContent;

                        oDS.Relations.Add("CatCont", oDS.Tables["ProductCategory"].Columns["nCatKey"], oDS.Tables["Content"].Columns["catid"], false);

                        oDS.Relations["CatCont"].Nested = true;
                    }
                    foreach (DataRow oDr in oDS.Tables["ProductCategory"].Rows)
                    {
                        oDr["Count"] = oDr.GetChildRows("CatCont").Length;
                        if (Conversions.ToBoolean(!Operators.ConditionalCompareObjectEqual(oDr["nCatKey"], nGroupID, false)))
                        {
                            foreach (var oDr2 in oDr.GetChildRows("CatCont"))
                                oDr2.Delete();

                        }
                    }


                    var oElmt = oPageDetail.OwnerDocument.CreateElement("ProductCats");
                    oElmt.InnerXml = Strings.Replace(Strings.Replace(oDS.GetXml(), "&lt;", "<"), "&gt;", ">");

                    foreach (XmlElement contentElmt in oElmt.FirstChild.SelectNodes("ProductCategory/Content"))
                    {
                        XmlElement contentElmtL2 = (XmlElement)contentElmt.FirstChild;
                        foreach (XmlElement ChildElmts in (IEnumerable)contentElmtL2.SelectNodes("*"))
                            contentElmt.AppendChild(ChildElmts.Clone());
                        contentElmt.RemoveChild((XmlNode)contentElmtL2);
                    }
                    oPageDetail.AppendChild(oElmt.FirstChild);
                }


                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "DeliveryMethodProcess", ex, "", sProcessInfo, gbDebug);
                }
            }

            private void DiscountRulesProcess(ref XmlElement oPageDetail, ref string sAdminLayout)
            {
                string sProcessInfo = "";
                sAdminLayout = "DiscountRules";
                string cSql;
                DataSet oDS;
                try
                {
                    string status = myWeb.moRequest["isActive"];
                    if (status == "1")
                    {
                        cSql = "Select *, a.nStatus as status, a.dPublishDate as publishDate, a.dExpireDate as expireDate From tblCartDiscountRules dr inner join tblaudit a on dr.nAuditid = a.nAuditKey where (a.dExpireDate >= getdate() or a.dExpireDate is null)  and a.nStatus=1";
                    }
                    else if (status == "0")
                    {

                        cSql = "Select *, a.nStatus as status, a.dPublishDate as publishDate, a.dExpireDate as expireDate From tblCartDiscountRules dr inner join tblaudit a on dr.nAuditid = a.nAuditKey where a.dExpireDate <= getdate()  or a.nStatus=0";
                    }
                    // ElseIf status = "singleUse" Then
                    // cSql = "Select *, a.nStatus as status, a.dPublishDate as publishDate, a.dExpireDate as expireDate From tblCartDiscountRules dr inner join tblaudit a on dr.nAuditid = a.nAuditKey where dr.cDiscountCode like '%VOUCHER'"
                    else
                    {
                        cSql = "Select *, a.nStatus as status, a.dPublishDate as publishDate, a.dExpireDate as expireDate From tblCartDiscountRules dr inner join tblaudit a on dr.nAuditid = a.nAuditKey where (a.dExpireDate >= getdate() or a.dExpireDate is null)   and a.nStatus=1";
                    }


                    oDS = myWeb.moDbHelper.GetDataSet(cSql, "DiscountRule", "DiscountRules");
                    if (oDS.Tables.Count == 1)
                    {
                        oDS.Tables[0].Columns["nDiscountKey"].ColumnMapping = MappingType.Attribute;
                        oDS.Tables[0].Columns["nDiscountForeignRef"].ColumnMapping = MappingType.Element;
                        oDS.Tables[0].Columns["cDiscountName"].ColumnMapping = MappingType.Attribute;
                        oDS.Tables[0].Columns["cDiscountCode"].ColumnMapping = MappingType.Attribute;
                        oDS.Tables[0].Columns["bDiscountIsPercent"].ColumnMapping = MappingType.Element;
                        oDS.Tables[0].Columns["nDiscountCompoundBehaviour"].ColumnMapping = MappingType.Element;
                        oDS.Tables[0].Columns["nDiscountValue"].ColumnMapping = MappingType.Element;
                        oDS.Tables[0].Columns["nDiscountMinPrice"].ColumnMapping = MappingType.Element;
                        oDS.Tables[0].Columns["nDiscountMinQuantity"].ColumnMapping = MappingType.Element;
                        oDS.Tables[0].Columns["nDiscountCat"].ColumnMapping = MappingType.Element;
                        oDS.Tables[0].Columns["nAuditId"].ColumnMapping = MappingType.Attribute;
                        oDS.Tables[0].Columns["status"].ColumnMapping = MappingType.Attribute;
                        oDS.Tables[0].Columns["publishDate"].ColumnMapping = MappingType.Attribute;
                        oDS.Tables[0].Columns["expireDate"].ColumnMapping = MappingType.Attribute;
                        oDS.Tables[0].Columns["cAdditionalXML"].ColumnMapping = MappingType.Element;

                        if (myWeb.moDbHelper.checkTableColumnExists("tblCartShippingPermission", "nPermLevel"))
                        {
                            cSql = "SELECT tblDirectory.*, tblCartDiscountDirRelations.nDiscountDirRelationKey, tblCartDiscountDirRelations.nPermLevel, tblCartDiscountDirRelations.nDiscountId FROM tblCartDiscountDirRelations LEFT OUTER JOIN tblDirectory ON tblCartDiscountDirRelations.nDirId = tblDirectory.nDirKey WHERE (tblCartDiscountDirRelations.nDiscountDirRelationKey IS NOT NULL)";
                        }
                        else
                        {
                            cSql = "SELECT tblDirectory.*, tblCartDiscountDirRelations.nDiscountDirRelationKey, tblCartDiscountDirRelations.nDiscountId FROM tblCartDiscountDirRelations LEFT OUTER JOIN tblDirectory ON tblCartDiscountDirRelations.nDirId = tblDirectory.nDirKey WHERE (tblCartDiscountDirRelations.nDiscountDirRelationKey IS NOT NULL)";
                        }

                        myWeb.moDbHelper.addTableToDataSet(ref oDS, cSql, "Dir");
                        cSql = "SELECT tblCartProductCategories.*, tblCartDiscountProdCatRelations.nDiscountProdCatRelationKey, tblCartDiscountProdCatRelations.nProductCatId, tblCartDiscountProdCatRelations.nDiscountId FROM tblCartProductCategories RIGHT OUTER JOIN tblCartDiscountProdCatRelations ON tblCartProductCategories.nCatKey = tblCartDiscountProdCatRelations.nProductCatId"; // WHERE (tblCartProductCategories.cCatSchemaName = N'Discount')"
                        myWeb.moDbHelper.addTableToDataSet(ref oDS, cSql, "ProdCat");
                        if (oDS.Tables.Contains("Dir"))
                        {
                            oDS.Relations.Add("RelDiscDir", oDS.Tables["DiscountRule"].Columns["nDiscountKey"], oDS.Tables["Dir"].Columns["nDiscountId"], false);
                            oDS.Relations["RelDiscDir"].Nested = true;
                            oDS.Tables["Dir"].Columns["nDirKey"].ColumnMapping = MappingType.Attribute;
                            oDS.Tables["Dir"].Columns["cDirName"].ColumnMapping = MappingType.Attribute;
                            if (myWeb.moDbHelper.checkTableColumnExists("tblCartShippingPermission", "nPermLevel"))
                            {
                                oDS.Tables["Dir"].Columns["nPermLevel"].ColumnMapping = MappingType.Attribute;
                            }
                        }
                        if (oDS.Tables.Contains("ProdCat"))
                        {
                            oDS.Relations.Add("RelDiscProdCat", oDS.Tables["DiscountRule"].Columns["nDiscountKey"], oDS.Tables["ProdCat"].Columns["nDiscountId"], false);
                            oDS.Relations["RelDiscProdCat"].Nested = true;
                            oDS.Tables["ProdCat"].Columns["nCatKey"].ColumnMapping = MappingType.Attribute;
                            oDS.Tables["ProdCat"].Columns["cCatName"].ColumnMapping = MappingType.Attribute;
                        }
                    }
                    var oElmt = oPageDetail.OwnerDocument.CreateElement("DiscountRules");
                    oElmt.InnerXml = Strings.Replace(Strings.Replace(oDS.GetXml(), "&lt;", "<"), "&gt;", ">");
                    oPageDetail.AppendChild(oElmt.FirstChild);
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "DiscountRulesProcess", ex, "", sProcessInfo, gbDebug);
                }
            }

            private void updateLessVariables(string ThemeName, ref XmlElement settingsXml)
            {
                string cProcessInfo = "";
                string ThemeLessFile = "";
                string ThemePath = "/themes/";
                string VariablePrefix = @"\$"; // $ needs escaping.
                if (myWeb.moConfig["cssFramework"] != "bs5")
                {
                    ThemePath = "/ewThemes/";
                    VariablePrefix = "@";
                }

                try
                {
                    if (settingsXml.SelectSingleNode("theme/add[@key='variablesPath']") != null)
                    {
                        ThemeLessFile = settingsXml.SelectSingleNode("theme/add[@key='variablesPath']/@value").InnerText;
                    }
                    if (!string.IsNullOrEmpty(ThemeLessFile))
                    {
                        var oFsH = new Protean.fsHelper(myWeb.moCtx);

                        ThemeLessFile = Protean.fsHelper.checkLeadingSlash(ThemeLessFile);
                        ThemeLessFile = ThemePath + ThemeName + "/" + ThemeLessFile;
                        if (Conversions.ToBoolean(oFsH.VirtualFileExists(ThemeLessFile)))
                        {



                            Tools.Security.Impersonate oImp = null;
                            if (Conversions.ToBoolean(myWeb.impersonationMode))
                            {
                                oImp = new Tools.Security.Impersonate();
                                if (oImp.ImpersonateValidUser(myWeb.moConfig["AdminAcct"], myWeb.moConfig["AdminDomain"], myWeb.moConfig["AdminPassword"], cInGroup: myWeb.moConfig["AdminGroup"]))
                                {
                                }
                            }

                            string content;

                            // check not read only
                            var oFileInfo = new FileInfo(myWeb.goServer.MapPath(ThemeLessFile));
                            oFileInfo.IsReadOnly = false;

                            using (var reader = new StreamReader(myWeb.goServer.MapPath(ThemeLessFile)))
                            {
                                content = reader.ReadToEnd();
                                reader.Close();
                            }
                            foreach (XmlElement oElmt in settingsXml.SelectNodes("theme/add[starts-with(@key,'" + ThemeName + ".')]"))
                            {
                                string variableName = oElmt.GetAttribute("key").Replace(ThemeName + ".", "");
                                string searchText = "(?<=" + VariablePrefix + variableName + ":).*(?=;)";
                                string replaceText = oElmt.GetAttribute("value").Trim();

                                // handle image files in CSS
                                if (Strings.LCase(replaceText).EndsWith(".gif") | Strings.LCase(replaceText).EndsWith(".png") | Strings.LCase(replaceText).EndsWith(".jpg"))
                                {
                                    replaceText = " '" + replaceText + "'";
                                }
                                else
                                {
                                    replaceText = " " + replaceText;
                                }

                                content = Regex.Replace(content, searchText, replaceText);
                            }

                            using (var writer = new StreamWriter(myWeb.goServer.MapPath(ThemeLessFile)))
                            {
                                writer.Write(content);
                                writer.Close();
                            }

                            if (Conversions.ToBoolean(myWeb.impersonationMode))
                            {
                                oImp.UndoImpersonation();
                                oImp = null;
                            }
                        }

                    }
                }


                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "updateLessVariables", ex, "", cProcessInfo, gbDebug);
                }

            }


            private void updateStandardXslVariables(string ThemeName, ref XmlElement settingsXml)
            {
                string cProcessInfo = "";
                string ThemeXslFile = "/themes/" + ThemeName + "/shared.xsl";
                try
                {

                    var oFsH = new Protean.fsHelper(myWeb.moCtx);
                    if (myWeb.moConfig["cssFramework"] != "bs5")
                    {
                        ThemeXslFile = "/ewThemes/" + ThemeName + "/Standard.xsl";
                    }

                    if (Conversions.ToBoolean(oFsH.VirtualFileExists(ThemeXslFile)))
                    {

                        Tools.Security.Impersonate oImp = null;
                        if (Conversions.ToBoolean(myWeb.impersonationMode))
                        {
                            oImp = new Tools.Security.Impersonate();
                            if (oImp.ImpersonateValidUser(myWeb.moConfig["AdminAcct"], myWeb.moConfig["AdminDomain"], myWeb.moConfig["AdminPassword"], cInGroup: myWeb.moConfig["AdminGroup"]))
                            {
                            }
                        }

                        string content;

                        // check not read only
                        var oFileInfo = new FileInfo(myWeb.goServer.MapPath(ThemeXslFile));
                        oFileInfo.IsReadOnly = false;

                        using (var reader = new StreamReader(myWeb.goServer.MapPath(ThemeXslFile)))
                        {
                            content = reader.ReadToEnd();
                            reader.Close();
                        }
                        foreach (XmlElement oElmt in settingsXml.SelectNodes("theme/add[starts-with(@key,'" + ThemeName + ".')]"))
                        {
                            string variableName = oElmt.GetAttribute("key").Replace(ThemeName + ".", "");

                            string searchText = "(?<=<xsl:variable name=\"" + variableName + "\">).*(?=</xsl:variable>)";

                            string replaceText = oElmt.GetAttribute("value").Trim();

                            content = Regex.Replace(content, searchText, replaceText);
                        }

                        using (var writer = new StreamWriter(myWeb.goServer.MapPath(ThemeXslFile)))
                        {
                            writer.Write(content);
                            writer.Close();
                        }
                        if (Conversions.ToBoolean(myWeb.impersonationMode))
                        {
                            oImp.UndoImpersonation();
                            oImp = null;
                        }
                    }
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "updateStandardXslVariables", ex, "", cProcessInfo, gbDebug);
                }

            }


            public void SchedulerProcess(ref string cewCmd, ref string cLayout, ref XmlElement oContentDetail)
            {
                // Dim oScheduler As New Scheduler
                System.Collections.Specialized.NameValueCollection oSchedulerConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/scheduler");
                // bool bHasScheduledItems = false;
                string cConStr = "";

                string cProcessInfo = "";

                string cUrl = moConfig["BaseUrl"];

                // DBHelper for the Scheduler database
                var dbt = new Cms.dbHelper(ref myWeb);

                try
                {
                    if (oSchedulerConfig != null)
                    {
                        cProcessInfo = "Connecting to the Scheduler";
                        if (!string.IsNullOrEmpty(oSchedulerConfig["SiteURL"]))
                            cUrl = oSchedulerConfig["SiteURL"];
                        //bHasScheduledItems = true;
                        cConStr = "Data Source=" + oSchedulerConfig["DatabaseServer"] + "; ";
                        cConStr += "Initial Catalog=" + oSchedulerConfig["DatabaseName"] + "; ";

                        if (!string.IsNullOrEmpty(oSchedulerConfig["DatabaseAuth"]))
                        {
                            cConStr += oSchedulerConfig["DatabaseAuth"];
                        }
                        else
                        {
                            cConStr += "user id=" + oSchedulerConfig["DatabaseUsername"] + ";password=" + oSchedulerConfig["DatabasePassword"] + ";";
                        }

                        dbt.ResetConnection(cConStr);
                    }

                Process:
                    ;



                    cProcessInfo = "Process: " + cewCmd;
                    switch (cewCmd ?? "")
                    {
                        case "ScheduledItems":
                            {
                                cLayout = "ScheduledItems";
                                if (dbt.ConnectionValid == false)
                                {
                                    var oElmt = oContentDetail.OwnerDocument.CreateElement("Content");
                                    oElmt.SetAttribute("type", "ActionList");
                                    var oSubElmt = oContentDetail.OwnerDocument.CreateElement("Item");
                                    oSubElmt.InnerText = "Connection invalid please speak to server administrator - " + dbt.DatabaseConnectionString;
                                    oElmt.AppendChild(oSubElmt);
                                    oContentDetail.AppendChild(oElmt);
                                }
                                else
                                {

                                    // oScheduler.ListActions(oContentDetail)
                                    if (oSchedulerConfig is null)
                                        return;
                                    string[] oList = Strings.Split(oSchedulerConfig["AvailableActions"], ",");
                                    DataSet oDS;
                                    // create new dbtools so we can use the scheduler DB
                                    int i;
                                    var oElmt = oContentDetail.OwnerDocument.CreateElement("Content");
                                    oElmt.SetAttribute("type", "ActionList");
                                    var loopTo = Information.UBound(oList);
                                    for (i = 0; i <= loopTo; i++)
                                    {
                                        var oSubElmt = oContentDetail.OwnerDocument.CreateElement("Item");
                                        oSubElmt.InnerText = oList[i];
                                        oElmt.AppendChild(oSubElmt);
                                    }
                                    oContentDetail.AppendChild(oElmt);



                                    string cSQL = "SELECT tblActions.* FROM tblWebsites INNER JOIN tblActions ON tblWebsites.nWebsiteKey = tblActions.nWebsite WHERE (tblWebsites.cWebsiteURL = '" + cUrl + "')";
                                    oDS = dbt.GetDataSet(cSQL, "Content");
                                    cProcessInfo = "Process: " + cewCmd + " - " + cSQL + " - ";

                                    if (oDS is null)
                                    {
                                    }
                                    // Throw New Exception("Could not get Actions")
                                    else if (oDS.Tables["Content"].Rows.Count > 0)
                                    {
                                        oDS.Tables["Content"].Columns.Add(new DataColumn("Active", typeof(short)));
                                        foreach (DataColumn oDC in oDS.Tables["Content"].Columns)
                                            oDC.ColumnMapping = MappingType.Attribute;
                                        oDS.Tables["Content"].Columns["cActionXML"].ColumnMapping = MappingType.Element;
                                        foreach (DataRow oRow in oDS.Tables["Content"].Rows)
                                        {
                                            bool bActive = true;
                                            DateTime dDate;
                                            if (!(oRow["dPublishDate"] is DBNull))
                                            {
                                                dDate = Conversions.ToDate(oRow["dPublishDate"]);
                                                if (!(dDate <= DateTime.Now))
                                                    bActive = false;
                                            }

                                            if (!(oRow["dExpireDate"] is DBNull))
                                            {
                                                dDate = Conversions.ToDate(oRow["dExpireDate"]);
                                                if (!(dDate >= DateTime.Now))
                                                    bActive = false;
                                            }

                                            oRow["Active"] = Conversions.ToInteger(bActive);
                                        }
                                        var oXML = new XmlDocument();
                                        oXML.InnerXml = Strings.Replace(Strings.Replace(oDS.GetXml(), "&gt;", ">"), "&lt;", "<");
                                        oContentDetail.InnerXml += oXML.DocumentElement.InnerXml;
                                    }
                                }

                                break;
                            }

                        case "AddScheduledItem":
                        case "EditScheduledItem":
                            {
                                var oADX = new Admin.AdminXforms(ref myWeb);

                                int nID = 0;
                                string cSQL = "SELECT nWebsiteKey FROM tblWebsites WHERE (cWebsiteURL = '" + cUrl + "')";
                                cProcessInfo = "Process: " + cewCmd + " - " + cSQL;

                                dbt.ResetConnection(cConStr);
                                nID = Conversions.ToInteger(dbt.ExeProcessSqlScalar(cSQL));
                                if (!(nID > 0))
                                {
                                    cSQL = "INSERT INTO tblWebsites (cWebsiteURL) VALUES ('" + cUrl + "')";
                                    cProcessInfo = "Process: " + cewCmd + " - " + cSQL;
                                    nID = Conversions.ToInteger(dbt.GetIdInsertSql(cSQL));
                                }
                                oContentDetail.AppendChild(oContentDetail.OwnerDocument.ImportNode(oADX.xFrmSchedulerItem(myWeb.moRequest["type"], nID, cConStr, Conversions.ToInteger(myWeb.moRequest["id"])), true));
                                if (oADX.valid)
                                {
                                    cewCmd = "ScheduledItems";
                                    goto Process;
                                }
                                else
                                {
                                    cLayout = "EditScheduledItem";
                                }

                                break;
                            }
                        case "ScheduledItemRunNow":
                            {
                                // TODO
                                DataSet oDS;
                                string cSQL = "SELECT tblActions.* FROM tblWebsites INNER JOIN tblActions ON tblWebsites.nWebsiteKey = tblActions.nWebsite WHERE (tblWebsites.cWebsiteURL = '" + cUrl + "' and tblActions.nActionKey = " + myWeb.moRequest["id"] + ")";
                                oDS = dbt.GetDataSet(cSQL, "Action");
                                foreach (DataRow oRow in oDS.Tables["Action"].Rows)
                                {
                                    var oTimeStart = DateTime.Now;
                                    var oSoapClient = new Tools.SoapClient();
                                    oSoapClient.RemoveReturnSoapEnvelope = true;
                                    oSoapClient.Url = Conversions.ToString(Operators.ConcatenateObject(cUrl + "/", oRow["cSubPath"]));
                                    oSoapClient.Action = Conversions.ToString(oRow["cType"]);
                                    string ActionXml = Conversions.ToString(oRow["cActionXml"]);
                                    var oXML = new XmlDocument();
                                    oXML.LoadXml(ActionXml);
                                    if (!string.IsNullOrEmpty(oXML.DocumentElement.GetAttribute("xmlns")))
                                    {
                                        oSoapClient.Namespace = oXML.DocumentElement.GetAttribute("xmlns");
                                    }
                                    else
                                    {
                                        oSoapClient.Namespace = oXML.DocumentElement.GetAttribute("exemelnamespace");
                                    }
                                    string cResponse = oSoapClient.SendRequest(ActionXml);

                                    oContentDetail.InnerXml = cResponse;
                                }

                                break;
                            }

                        case "DeactivateScheduledItem":
                            {
                                string cSQL;
                                string dPublishDate;
                                string dExpireDate;

                                string cTime = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Interaction.IIf(DateTime.Now.Hour < 10, "0" + DateTime.Now.Hour, DateTime.Now.Hour), ":"), Interaction.IIf(DateTime.Now.Minute < 10, "0" + DateTime.Now.Minute, DateTime.Now.Minute)), ":"), Interaction.IIf(DateTime.Now.Second < 10, "0" + DateTime.Now.Second, DateTime.Now.Second)));

                                dPublishDate = "Null";
                                // dExpireDate = sqlDateTime(Now, cTime)
                                dExpireDate = Database.SqlDate(DateTime.Now, true);

                                cSQL = "UPDATE tblActions SET dPublishDate =" + dPublishDate + ", dExpireDate =" + dExpireDate + " WHERE nActionKey = " + myWeb.moRequest["id"];
                                cProcessInfo = "Process: " + cewCmd + " - " + cSQL;

                                dbt.ExeProcessSql(cSQL);

                                cewCmd = "ScheduledItems";
                                goto Process;

                            }

                        case "ActivateScheduledItem":
                            {
                                string cSQL;
                                string dPublishDate;
                                string dExpireDate;

                                // Dim cTime As String = IIf(Now.Hour < 10, "0" & Now.Hour, Now.Hour) & ":" & IIf(Now.Minute < 10, "0" & Now.Minute, Now.Minute) & ":" & IIf(Now.Second < 10, "0" & Now.Second, Now.Second)

                                // dPublishDate = sqlDateTime(Now, cTime)
                                dPublishDate = Database.SqlDate(DateTime.Now, true);
                                dExpireDate = "Null";

                                cSQL = "UPDATE tblActions SET dPublishDate =" + dPublishDate + ", dExpireDate =" + dExpireDate + " WHERE nActionKey = " + myWeb.moRequest["id"];
                                cProcessInfo = "Process: " + cewCmd + " - " + cSQL;

                                dbt.ExeProcessSql(cSQL);

                                cewCmd = "ScheduledItems";
                                goto Process;
                            }

                        case "DeleteScheduledItem":
                            {
                                string cSQL;
                                string dPublishDate;
                                string dExpireDate = string.Empty;

                                // Dim cTime As String = IIf(Now.Hour < 10, "0" & Now.Hour, Now.Hour) & ":" & IIf(Now.Minute < 10, "0" & Now.Minute, Now.Minute) & ":" & IIf(Now.Second < 10, "0" & Now.Second, Now.Second)

                                // dPublishDate = sqlDateTime(Now, cTime)
                                dPublishDate = Database.SqlDate(DateTime.Now, true);
                                dExpireDate = "Null";
                                cSQL = "DELETE tblLog WHERE nActionId = " + myWeb.moRequest["id"];
                                cProcessInfo = "Process: " + cewCmd + " - " + cSQL;
                                dbt.ExeProcessSql(cSQL);

                                cSQL = "DELETE tblActions  WHERE nActionKey = " + myWeb.moRequest["id"];
                                cProcessInfo = "Process: " + cewCmd + " - " + cSQL;
                                dbt.ExeProcessSql(cSQL);

                                cewCmd = "ScheduledItems";
                                goto Process;
                            }
                    }
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "SchedulerProcess", ex, "", cProcessInfo, gbDebug);
                }
            }

            public void SubscriptionProcess(ref string cCmd, ref string sAdminLayout, ref XmlElement oPageDetail)
            {
                var oSub = new Cms.Cart.Subscriptions(ref myWeb);
                var oADX = new Admin.AdminXforms(ref myWeb);
            SP:
                ;

                switch (cCmd ?? "")
                {
                    case "CancelSubscription":
                        {

                            oPageDetail.AppendChild(oPageDetail.OwnerDocument.ImportNode(oADX.xFrmConfirmCancelSubscription(0.ToString(), myWeb.moRequest["id"], myWeb.mnUserId, true), true));
                            if (oADX.valid)
                            {
                                cCmd = "ManageUserSubscription";
                                // oSub.CancelSubscription(myWeb.moRequest("subId"))
                                goto SP;
                            }
                            else
                            {
                                sAdminLayout = "AdminXForm";
                            }

                            break;
                        }

                    case "ExpireSubscription":
                        {

                            oPageDetail.AppendChild(oPageDetail.OwnerDocument.ImportNode(oADX.xFrmConfirmCancelSubscription(0.ToString(), myWeb.moRequest["id"], myWeb.mnUserId, true), true));
                            if (oADX.valid)
                            {
                                cCmd = "ManageUserSubscription";
                                // oSub.CancelSubscription(myWeb.moRequest("subId"))
                                goto SP;
                            }
                            else
                            {
                                sAdminLayout = "AdminXForm";
                            }

                            break;
                        }
                    case "ResendCancellation":
                        {
                            oSub.ResendCancelation(Conversions.ToInteger(myWeb.moRequest["id"]));
                            cCmd = "ManageUserSubscription";
                            goto SP;

                        }
                    case "Subscriptions":
                        {
                            oSub.ListSubscriptions(ref oPageDetail);
                            break;
                        }
                    case "ListSubscribers":
                        {
                            oSub.ListSubscribers(ref oPageDetail);
                            break;
                        }
                    case "AddSubscriptionGroup":
                        {
                            oPageDetail.AppendChild(oPageDetail.OwnerDocument.ImportNode(oADX.xFrmProductGroup(0, "Subscription"), true));
                            if (oADX.valid)
                            {
                                cCmd = "Subscriptions";
                                goto SP;
                            }
                            else
                            {
                                sAdminLayout = "AdminXForm";
                            }

                            break;
                        }
                    case "EditSubscriptionGroup":
                        {
                            oPageDetail.AppendChild(oPageDetail.OwnerDocument.ImportNode(oADX.xFrmProductGroup(Conversions.ToInteger(myWeb.moRequest["grp"]), "Subscription"), true));
                            if (oADX.valid)
                            {
                                cCmd = "Subscriptions";
                                goto SP;
                            }
                            else
                            {
                                sAdminLayout = "AdminXForm";
                            }

                            break;
                        }
                    case "AddSubscription":
                        {
                            long nSubId = 0L;
                            long pgid = 0;
                            XmlElement localxFrmEditContent() { int argnReturnId1 = (int)nSubId; string argzcReturnSchema = ""; string argAlternateFormName = ""; var ret = oADX.xFrmEditContent(Convert.ToInt64(myWeb.moRequest["id"]), "Subscription", pgid, "", true, nReturnId: ref argnReturnId1, zcReturnSchema: ref argzcReturnSchema, AlternateFormName: ref argAlternateFormName); nSubId = argnReturnId1; return ret; }

                            oPageDetail.AppendChild(oPageDetail.OwnerDocument.ImportNode(localxFrmEditContent(), true));
                            if (oADX.valid)
                            {
                                var mySub = new Cms.Cart.Subscriptions(ref myWeb);
                                mySub.SubscriptionToGroup((int)nSubId, Conversions.ToInteger(Interaction.IIf(Information.IsNumeric(myWeb.moRequest["grp"]), myWeb.moRequest["grp"], (object)0)));
                                cCmd = "Subscriptions";
                                goto SP;
                            }
                            else
                            {
                                sAdminLayout = "AdminXForm";
                            }

                            break;
                        }
                    case "EditSubscription":
                        {
                            long nSubId = 0L;
                            long pgid = 0;
                            XmlElement localxFrmEditContent1() { int argnReturnId2 = (int)nSubId; string argzcReturnSchema1 = ""; string argAlternateFormName1 = ""; var ret = oADX.xFrmEditContent(Conversions.ToLong(myWeb.moRequest["id"]), "Subscription", pgid, "", true, nReturnId: ref argnReturnId2, zcReturnSchema: ref argzcReturnSchema1, AlternateFormName: ref argAlternateFormName1); nSubId = argnReturnId2; return ret; }

                            oPageDetail.AppendChild(oPageDetail.OwnerDocument.ImportNode(localxFrmEditContent1(), true));
                            if (oADX.valid)
                            {
                                var mySub = new Cms.Cart.Subscriptions(ref myWeb);
                                mySub.SubscriptionToGroup((int)nSubId, Conversions.ToInteger(Interaction.IIf(Information.IsNumeric(myWeb.moRequest["grp"]), myWeb.moRequest["grp"], (object)0)));
                                cCmd = "Subscriptions";
                                goto SP;
                            }
                            else
                            {
                                sAdminLayout = "AdminXForm";
                            }

                            break;
                        }

                    case "RenewSubscription":
                        {
                            oPageDetail.AppendChild(oPageDetail.OwnerDocument.ImportNode(oADX.xFrmRenewSubscription(myWeb.moRequest["id"]), true));
                            if (oADX.valid)
                            {
                                // cCmd = "AdminXForm"
                                myWeb.msRedirectOnEnd = "/?ewCmd=UpcomingRenewals";
                            }
                            // GoTo SP
                            else
                            {
                                sAdminLayout = "AdminXForm";
                            }

                            break;
                        }

                    case "ResendSubscription":
                        {
                            oPageDetail.AppendChild(oPageDetail.OwnerDocument.ImportNode(oADX.xFrmResendSubscription(myWeb.moRequest["id"]), true));
                            if (oADX.valid)
                            {
                                // cCmd = "AdminXForm"
                                myWeb.msRedirectOnEnd = "/?ewCmd=OrdersHistory";
                            }
                            // GoTo SP
                            else
                            {
                                sAdminLayout = "AdminXForm";
                            }

                            break;
                        }

                    case "ManageUserSubscription":
                        {
                            myWeb.moSession["tempInstance"] = (object)null;
                            oSub.GetSubscriptionDetail(ref oPageDetail, Conversions.ToInteger(myWeb.moRequest["id"]));
                            sAdminLayout = "ManageUserSubscription";
                            break;
                        }
                    // If oADX.valid Then
                    // cCmd = "Subscriptions"
                    // GoTo SP
                    // Else
                    // sAdminLayout = "AdminXForm"
                    // End If

                    case "EditUserSubscription":
                        {
                            oPageDetail.AppendChild(oPageDetail.OwnerDocument.ImportNode(oADX.xFrmEditUserSubscription(Conversions.ToInteger(myWeb.moRequest["id"])), true));
                            if (oADX.valid)
                            {
                                cCmd = "ManageUserSubscription";
                                goto SP;
                            }
                            else
                            {
                                sAdminLayout = "AdminXForm";
                            }

                            break;
                        }

                    case "MoveSubscription":
                        {

                            sAdminLayout = "Subscriptions";
                            if (Information.IsNumeric(myWeb.moRequest["grp"]))
                            {
                                oSub.SubscriptionToGroup(Conversions.ToInteger(myWeb.moRequest["id"]), Conversions.ToInteger(myWeb.moRequest["grp"]));
                                cCmd = "Subscriptions";
                                goto SP;
                            }
                            else
                            {
                                oSub.ListSubscriptions(ref oPageDetail);
                                cCmd = "MoveSubscription";
                            }

                            break;
                        }

                    case "LocateSubscription":
                        {
                            if (!string.IsNullOrEmpty(myWeb.moRequest["submit"]))
                            {
                                // updateLocations
                                myWeb.moDbHelper.updateLocations(Conversions.ToLong(myWeb.moRequest["id"]), myWeb.moRequest["location"]);
                                sAdminLayout = "Subscriptions";
                                mcEwCmd = "Subscriptions";

                                oPageDetail.RemoveAll();
                                myWeb.mnArtId = 0;
                                goto SP;
                            }
                            else
                            {
                                XmlElement argContentNode = null;
                                oPageDetail.AppendChild(myWeb.moDbHelper.getLocationsByContentId(Conversions.ToLong(myWeb.moRequest["id"]), ContentNode: ref argContentNode));
                                sAdminLayout = "LocateContent";
                                mcEwCmd = "LocateSubscription";

                            }

                            break;
                        }
                    case "UpSubscription":
                        {
                            break;
                        }

                    case "DownSubscription":
                        {
                            break;
                        }

                    case "RecentRenewals":
                        {
                            oSub.ListRecentRenewals(ref oPageDetail);
                            break;
                        }

                    case "UpcomingRenewals":
                        {

                            oSub.ListUpcomingRenewals(ref oPageDetail);
                            break;
                        }

                    case "ExpiredSubscriptions":
                        {
                            oSub.ListExpiredSubscriptions(ref oPageDetail);
                            break;
                        }

                    case "CancelledSubscriptions":
                        {
                            oSub.ListCancelledSubscriptions(ref oPageDetail);
                            break;
                        }

                    case "RenewalAlerts":
                        {
                            oSub.ListRenewalAlerts(ref oPageDetail);
                            break;
                        }

                }


            }

            // VersionControlProcess
            public void VersionControlProcess(ref XmlElement oPageDetail, ref string sAdminLayout)
            {
                try
                {
                    sAdminLayout = "VersionControlProcess";

                    // Go get all the content that is in a state of pending

                    // Go get all the versions that are pending
                    var oContElmt = myWeb.moDbHelper.getPendingContent();

                    if (oContElmt != null)
                        oPageDetail.AppendChild(oContElmt);

                    myWeb.moSession["lastPage"] = myWeb.mcOriginalURL;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "VersionControlProcess", ex, "", "", gbDebug);
                }
            }


            public void MemberActivityProcess(ref XmlElement oPageDetail, ref string sAdminLayout)
            {
                try
                {
                    sAdminLayout = "MemberActivityReport";
                    var oDS = new DataSet();
                    string cReportName = "";
                    if (!string.IsNullOrEmpty(myWeb.moRequest["SessionID"]))
                    {
                        myWeb.moDbHelper.ConnectTimeout = 180;
                        oDS = myWeb.moDbHelper.GetDataSet("EXEC spGetUserSessionActivityDetail '" + myWeb.moRequest["SessionId"] + "'", "Item", "Report");
                        myWeb.moDbHelper.ConnectTimeout = 15;
                        cReportName = "User Session Report";
                    }
                    else if (!string.IsNullOrEmpty(myWeb.moRequest["UserId"]))
                    {
                        myWeb.moDbHelper.ConnectTimeout = 180;
                        oDS = myWeb.moDbHelper.GetDataSet("EXEC spGetUserSessionActivity " + myWeb.moRequest["UserId"], "Item", "Report");
                        myWeb.moDbHelper.ConnectTimeout = 15;
                        cReportName = "User Activity Report";
                    }
                    else
                    {
                        moAdXfm.xFrmMemberVisits();
                        if (moAdXfm.valid)
                        {
                            var dFrom = default(DateTime);
                            var dTo = default(DateTime);
                            if (Information.IsDate(moAdXfm.Instance.FirstChild.SelectSingleNode("dTo").InnerText))
                                dFrom = Conversions.ToDate(moAdXfm.Instance.FirstChild.SelectSingleNode("dFrom").InnerText);
                            if (Information.IsDate(moAdXfm.Instance.FirstChild.SelectSingleNode("dTo").InnerText))
                                dTo = Conversions.ToDate(moAdXfm.Instance.FirstChild.SelectSingleNode("dTo").InnerText);
                            string cFrom = Database.SqlDate(dFrom, false);
                            string cTo = Database.SqlDate(dTo, false);
                            string cGroups = "'" + moAdXfm.Instance.FirstChild.SelectSingleNode("cGroups").InnerText + "'";
                            string sSql = "EXEC spGetSessionActivity " + cFrom + ", " + cTo + ", " + cGroups;
                            myWeb.moDbHelper.ConnectTimeout = 180;
                            oDS = myWeb.moDbHelper.GetDataSet(sSql, "Item", "Report");
                            myWeb.moDbHelper.ConnectTimeout = 15;
                            cReportName = "Member Activity Report";
                        }
                        oPageDetail.AppendChild(moAdXfm.moXformElmt);
                    }

                    if (oDS.Tables.Count > 0)
                    {
                        var oContElmt = moPageXML.CreateElement("Content");
                        oContElmt.SetAttribute("name", cReportName);
                        oContElmt.SetAttribute("type", "Report");
                        oContElmt.InnerXml = oDS.GetXml();
                        oPageDetail.AppendChild(oContElmt);
                    }
                }


                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "MemberActivityProcess", ex, "", "", gbDebug);
                }
            }

            /// <summary>
            /// The Member code management process.
            /// </summary>
            /// <param name="oPageDetail">The page ContentDetail node</param>
            /// <param name="sAdminLayout">Referential variable for the admin layout</param>
            /// <remarks></remarks>
            public void MemberCodesProcess(ref XmlElement oPageDetail, ref string sAdminLayout)
            {

                string cProcessInfo = "";
                myWeb.PerfMon.Log("Admin", "MemberCodesProcess");

                try
                {

                    string cSQL = "";
                    DataSet oDS;
                    XmlDataDocument oXml;
                    string sContent = "";
                    int nId;

                    var oCont = moPageXML.CreateElement("Content");

                    bool bListCodesets = true;
                    string cSubCmd = "";


                    // We are either dealing with a form or not - if not just return the codesets
                    if (Information.IsNumeric(myWeb.moRequest["id"]) | !string.IsNullOrEmpty(myWeb.moRequest["subCmd"]))
                    {

                        cProcessInfo = "Process form";
                        bListCodesets = false;

                        // Get the appropriate form
                        cSubCmd = myWeb.moRequest["subCmd"];
                        if (string.IsNullOrEmpty(cSubCmd))
                            cSubCmd = "EditCodeSet";

                        // Set variables
                        nId = Conversions.ToInteger(myWeb.moRequest["id"]);

                        switch (cSubCmd ?? "")
                        {

                            case "EditCodeSet":
                            case "AddCodeSet":
                                {
                                    // This is add or edit
                                    moAdXfm.xFrmMemberCodeset(nId);
                                    if (moAdXfm.isSubmitted() && moAdXfm.valid)
                                        bListCodesets = true;
                                    break;
                                }

                            case "ManageCodes":
                                {
                                    moAdXfm.xFrmMemberCodeGenerator(nId);

                                    // Get the list of sub codes.
                                    if (nId == 0)
                                        nId = -1;
                                    cSQL = "SELECT nCodeKey As id, cCode As Code, dUseDate As Date_Used, nDirKey As User_Id, cDirName As Username, cDirXml As UserXml FROM tblCodes LEFT OUTER JOIN tblDirectory ON nUseId = nDirKey WHERE nCodeParentId = " + nId;

                                    oDS = myWeb.moDbHelper.GetDataSet(cSQL, "Code", "tblCodes");
                                    oDS.Tables["Code"].Columns["id"].ColumnMapping = MappingType.Attribute;
                                    oDS.Tables["Code"].Columns["User_Id"].ColumnMapping = MappingType.Attribute;

                                    myWeb.moDbHelper.ReturnNullsEmpty(ref oDS);
                                    oDS.EnforceConstraints = false;
                                    oXml = new XmlDataDocument(oDS);

                                    // Convert any text to xml
                                    foreach (XmlElement oElmt in oXml.SelectNodes("descendant-or-self::UserXml"))
                                    {
                                        sContent = oElmt.InnerText;
                                        if (!string.IsNullOrEmpty(sContent))
                                        {
                                            oElmt.InnerXml = sContent;
                                        }
                                    }

                                    oCont.InnerXml = oXml.InnerXml;
                                    oCont.SetAttribute("type", "SubCodeList");
                                    oCont.SetAttribute("name", "Directory Codes");
                                    oPageDetail.AppendChild(oCont);
                                    break;
                                }

                            case "ManageCodeGroups":
                                {
                                    moAdXfm.xFrmMemberCodeset(nId, "CodesGroups");
                                    if (moAdXfm.isSubmitted() && moAdXfm.valid)
                                        bListCodesets = true;
                                    break;
                                }
                        }

                        if (!bListCodesets)
                            oPageDetail.AppendChild(moAdXfm.moXformElmt);

                    }

                    if (bListCodesets)
                    {

                        cProcessInfo = "Get codeset list";

                        // Show CodeSet List
                        cSQL = "exec spGetCodes " + ((int)Cms.dbHelper.CodeType.Membership).ToString();
                        oDS = myWeb.moDbHelper.GetDataSet(cSQL, "Code", "tblCodes");

                        // Enumerate the group memberships for each
                        string cGroups = "";
                        foreach (DataRow oRow in oDS.Tables["Code"].Rows)
                        {
                            if (!string.IsNullOrEmpty(cGroups))
                                cGroups += ",";
                            cGroups = Conversions.ToString(cGroups + oRow["nCodeKey"]);
                        }
                        myWeb.moDbHelper.addTableToDataSet(ref oDS, "exec spGetCodeDirectoryGroups '" + cGroups + "'", "Groups");
                        foreach (DataTable oDT in oDS.Tables)
                        {
                            foreach (DataColumn oDC in oDT.Columns)
                                oDC.ColumnMapping = MappingType.Attribute;
                        }

                        // Add the Groups to the Dataset
                        oDS.Tables["Groups"].Columns["nCodeKey"].ColumnMapping = MappingType.Hidden;
                        oDS.Relations.Add("Rel01", oDS.Tables["Code"].Columns["nCodeKey"], oDS.Tables["Groups"].Columns["nCodeKey"], false);
                        oDS.Relations["Rel01"].Nested = true;

                        // Convert the results to XML
                        oCont.InnerXml = oDS.GetXml();
                        oCont.SetAttribute("type", "CodeSet");
                        oCont.SetAttribute("name", "Directory Codes");
                        oPageDetail.AppendChild(oCont);

                    }

                    sAdminLayout = "DirectoryCodes";
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "MemberCodesProcess", ex, "", cProcessInfo, gbDebug);
                }
            }

            private void EventBookingProcess(ref XmlElement oPageDetail, ref string sAdminLayout)
            {
                string sProcessInfo = "";

                try
                {
                    // Case "cpdReportsPage"
                    string dateQuery = " and a.dExpireDate >= " + sqlDate(DateTime.Now);
                    if (mcEwCmd2 == "pastbookings")
                    {
                        dateQuery = " and a.dExpireDate < " + sqlDate(DateTime.Now);
                    }


                    string sSql1 = "select nContentKey, cContentName, dExpireDate, cContentXmlBrief from tblContent c " + "inner join tblAudit a On c.nAuditId = a.nAuditKey " + "where cContentSchemaName = 'Event' " + dateQuery + "order by a.dExpireDate desc";

                    // get a list of events with tickets sold in the future
                    var oEvtsDs = myWeb.moDbHelper.GetDataSet(sSql1, "Event", "Events");
                    string sSql = "spTicketsSoldSummary";
                    myWeb.moDbHelper.addTableToDataSet(ref oEvtsDs, sSql, "Ticket");

                    if (oEvtsDs.Tables[0].Rows.Count > 0)
                    {

                        oEvtsDs.Tables[0].Columns[0].ColumnMapping = MappingType.Attribute;
                        oEvtsDs.Tables[0].Columns[1].ColumnMapping = MappingType.Attribute;
                        oEvtsDs.Tables[0].Columns[2].ColumnMapping = MappingType.Attribute;
                        // oEvtsDs.Tables(0).Columns("cContentXmlBrief").ColumnMapping = Data.MappingType.SimpleContent
                        oEvtsDs.Relations.Add("rel01", oEvtsDs.Tables[0].Columns["nContentKey"], oEvtsDs.Tables[1].Columns["EventKey"], false);
                        oEvtsDs.Relations["rel01"].Nested = true;


                        var oXml = new XmlDocument();
                        oXml.LoadXml(oEvtsDs.GetXml());
                        foreach (XmlElement oEvtElmt in oXml.DocumentElement.SelectNodes("Event/cContentXmlBrief"))
                            oEvtElmt.InnerXml = oEvtElmt.InnerText;

                        oPageDetail.AppendChild(moPageXML.ImportNode(oXml.DocumentElement, true));

                        if (!string.IsNullOrEmpty(myWeb.moRequest["EventId"]))
                        {

                            var oTicketDs = myWeb.moDbHelper.GetDataSet("select * from vw_TicketsSalesReport where EventKey = " + myWeb.moRequest["EventId"], "Ticket", "Tickets");

                            var oXml2 = new XmlDocument();
                            oXml2.LoadXml(oTicketDs.GetXml());
                            oXml2.DocumentElement.SetAttribute("EventId", myWeb.moRequest["EventId"]);
                            var oRpt = oPageDetail.OwnerDocument.CreateElement("Report");
                            oRpt.AppendChild(moPageXML.ImportNode(oXml2.DocumentElement, true));
                            oPageDetail.AppendChild(oRpt);

                        }
                    }
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "EventBookingProcess", ex, "", sProcessInfo, gbDebug);
                }
            }

            private void ReportsProcess(ref XmlElement oPageDetail, ref string sAdminLayout)
            {
                string sProcessInfo = "";

                try
                {
                    // Case "cpdReportsPage"

                    if (!string.IsNullOrEmpty(mcEwCmd2))
                    {
                        oPageDetail.AppendChild(moAdXfm.xFrmGetReport(mcEwCmd2));
                        if (moAdXfm.valid)
                        {
                            XmlElement argQueryXml = (XmlElement)moAdXfm.Instance.FirstChild;
                            myWeb.moDbHelper.GetReport(ref oPageDetail, ref argQueryXml);
                        }
                    }
                    if (string.IsNullOrEmpty(oPageDetail.InnerXml))
                    {
                        myWeb.moDbHelper.ListReports(ref oPageDetail);
                    }
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "DeliveryMethodProcess", ex, "", sProcessInfo, gbDebug);
                }
            }


            private void ResetWebConfig()
            {
                string sProcessInfo = "";

                try
                {
                    var myConfiguration = WebConfigurationManager.OpenWebConfiguration("~");
                    string flag = myConfiguration.AppSettings.Settings["recompile"].Value.ToString();
                    if (flag.ToLower() == "true")
                    {
                        Protean.Config.UpdateConfigValue(ref myWeb, "", "recompile", "false");
                    }
                    else
                    {
                        Protean.Config.UpdateConfigValue(ref myWeb, "", "recompile", "true");
                    }
                    myWeb.moResponse.Redirect(myWeb.mcRequestDomain);
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "ResetWebConfig", ex, "", sProcessInfo, gbDebug);
                }
            }



            private void ReIndexing(ref Cms aWeb)
            {
                myWeb = aWeb;
                string sProcessInfo = "";
                string SchemaNameForUpdate;
                string sSql;
                string IpAddress;
                var objServ = new Services();

                int mnUserId = myWeb.mnUserId;
                var moSession = myWeb.moSession;
                string cSql;
                DataSet oDS;
                DateTime lastLoginSpan;
                var CurrentDateTime = DateTime.Now;

                try
                {

                    cSql = "select top 1  dDateTime from tblActivityLog   where cActivityDetail='ReIndexing'  order by 1 desc";
                    oDS = myWeb.moDbHelper.GetDataSet(cSql, "Index", "IndexRules");
                    lastLoginSpan = Conversions.ToDate(oDS.Tables[0].Rows[0].ItemArray[0].ToString());
                    int hr = CurrentDateTime.Subtract(lastLoginSpan).Hours;
                    if (hr >= 1)
                    {
                        IpAddress = objServ.GetIpAddress(myWeb.moRequest);
                        SchemaNameForUpdate = "null";
                        sSql = "spScheduleToUpdateIndexTable";
                        var arrParms = new Hashtable();
                        arrParms.Add("SchemaName", SchemaNameForUpdate);
                        myWeb.moDbHelper.ExeProcessSql(sSql, CommandType.StoredProcedure, arrParms);

                        myWeb.moDbHelper.logActivity(Cms.dbHelper.ActivityType.SessionContinuation, (long)mnUserId, 0L, 0L, 0L, "ReIndexing", true);

                    }
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "ReIndexing", ex, "", sProcessInfo, gbDebug);
                }
            }

        }



    }
}