using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Xml;

namespace Protean
{
    public partial class Setup
    {

        public class SetupXforms : Protean.xForm
        {
            private const string mcModuleName = "Setup.SetupXForms";
            private Setup mySetup;

            public System.Collections.Specialized.NameValueCollection goConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");

            public SetupXforms(ref Setup asetup) : base(ref asetup.myWeb.msException)
            {
                // PerfMon.Log("Discount", "New")
                try
                {
                    mySetup = asetup;
                    goConfig = mySetup.goConfig;
                    base.moPageXML = mySetup.moPageXml;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref Protean.xForm.msException, mcModuleName, "New", ex, "", "", true);
                }
            }


            public string GuessDBName()
            {
                // PerfMon.Log("Setup", "GuessDBName")
                try
                {
                    string siteUrl = this.goRequest.ServerVariables["SERVER_NAME"];
                    string PrePropURL = goConfig["PrePropUrl"] + "";
                    if (!string.IsNullOrEmpty(PrePropURL))
                        siteUrl = siteUrl.Replace(PrePropURL, "");
                    siteUrl = siteUrl.Replace("http://", "");
                    siteUrl = siteUrl.Replace("www.", "");
                    siteUrl = siteUrl.Replace(".", "_");
                    siteUrl = siteUrl.Replace("-", "_");
                    return "ew_" + siteUrl;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref Protean.xForm.msException, mcModuleName, "GuessDBName", ex, "", "", true);
                    return "";
                }
            }

            public XmlElement xFrmWebSettings()
            {
                XmlElement oFrmElmt;
                string cProcessInfo = "";
                Protean.fsHelper oFsh;

                try
                {
                    oFsh = new Protean.fsHelper();
                    oFsh.open(this.moPageXML);

                    base.NewFrm("WebSettings");

                    base.submission("WebSettings", "", "post", "form_check(this)");

                    oFrmElmt = base.addGroup(ref base.moXformElmt, "WebSettings", "", "Enter your MS SQL connection details");

                    //XmlNode argoNode = oFrmElmt;
                    base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Hint, "Please enter your database connection details.");
                    //oFrmElmt = (XmlElement)argoNode;

                    // If goConfig("DatabaseServer") = "" Then

                    base.addInput(ref oFrmElmt, "ewDatabaseServer", true, "DB Server Hostname");
                    XmlElement argoBindParent = null;
                    base.addBind("ewDatabaseServer", "web/add[@key='DatabaseServer']/@value", oBindParent: ref argoBindParent, "true()");

                    // End If

                    base.addInput(ref oFrmElmt, "ewDatabaseName", true, "DB Name");
                    XmlElement argoBindParent1 = null;
                    base.addBind("ewDatabaseName", "web/add[@key='DatabaseName']/@value", oBindParent: ref argoBindParent1, "true()");

                    base.addInput(ref oFrmElmt, "ewDatabaseUsername", true, "DB Username");
                    XmlElement argoBindParent2 = null;
                    base.addBind("ewDatabaseUsername", "web/add[@key='DatabaseUsername']/@value", oBindParent: ref argoBindParent2, "false()");

                    base.addInput(ref oFrmElmt, "ewDatabasePassword", true, "DB Password / CMS Admin Password");
                    XmlElement argoBindParent3 = null;
                    base.addBind("ewDatabasePassword", "web/add[@key='DatabasePassword']/@value", oBindParent: ref argoBindParent3, "false()");

                    base.addInput(ref oFrmElmt, "ewSiteAdminEmail", true, "Webmaster Email");
                    XmlElement argoBindParent4 = null;
                    base.addBind("ewSiteAdminEmail", "web/add[@key='SiteAdminEmail']/@value", oBindParent: ref argoBindParent4, "true()");

                    base.addSubmit(ref oFrmElmt, "", "Save Settings");

                    // Dim oCfg As Configuration = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/")
                    // Dim oCgfSect As System.Configuration.DefaultSection = oCfg.GetSection("eonic/web")

                    // Dim oImp As Protean.Tools.Security.Impersonate = New Protean.Tools.Security.Impersonate
                    // If oImp.ImpersonateValidUser(goConfig("AdminAcct"), goConfig("AdminDomain"), goConfig("AdminPassword"), True, goConfig("AdminGroup")) Then

                    // MyBase.instance.InnerXml = oCgfSect.SectionInformation.GetRawXml
                    var oDefaultCfgXml = new XmlDocument();

                    if (Convert.ToBoolean(oFsh.VirtualFileExists("/protean.web.config")))
                    {
                        oDefaultCfgXml.Load(this.goServer.MapPath("/protean.web.config"));
                    }
                    else
                    {
                        oDefaultCfgXml.Load(this.goServer.MapPath("/ewcommon/setup/rootfiles/protean_config.xml"));
                    }


                    base.Instance.InnerXml = oDefaultCfgXml.SelectSingleNode("web").OuterXml;

                    // code here to replace any missing nodes
                    // all of the required config settings

                    if (base.isSubmitted())
                    {
                        base.updateInstanceFromRequest();
                        base.validate();
                        if (base.valid)
                        {

                            // lets insure all the essential files are in place
                            // Dim CreateDirs As New FileStructureSetup
                            // If Not CreateDirs.Execute() Then

                            // MyBase.valid = False
                            // MyBase.addNote(oFrmElmt, noteTypes.Alert, CreateDirs.errMsg)

                            // Else

                            var oCfg = WebConfigurationManager.OpenWebConfiguration("/");

                            // Now lets create the database
                            Cms myWebArg = new Cms(moCtx);
                            Cms.dbHelper oDbt = new Cms.dbHelper(myWebArg);
                            string sDbName = this.Instance.SelectSingleNode("web/add[@key='DatabaseName']/@value").InnerText;
                            string cDbServer = this.Instance.SelectSingleNode("web/add[@key='DatabaseServer']/@value").InnerText;
                            string cDbUsername = this.Instance.SelectSingleNode("web/add[@key='DatabaseUsername']/@value").InnerText;
                            string cDbPassword = this.Instance.SelectSingleNode("web/add[@key='DatabasePassword']/@value").InnerText;
                            if (oDbt.createDB(sDbName, cDbServer, cDbUsername, cDbPassword))
                            {
                                // success
                                oDbt.ResetConnection("Data Source=" + cDbServer + "; " + "Initial Catalog=" + sDbName + "; " + "user id=" + cDbUsername + "; password=" + cDbPassword);
                                if (oDbt.ConnectionValid)
                                {
                                    base.valid = true;
                                }
                                else
                                {
                                    base.valid = false;
                                    //XmlNode argoNode1 = oFrmElmt;
                                    base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "These database connection details could not connect.");
                                    //oFrmElmt = (XmlElement)argoNode1;
                                }
                            }

                            else
                            {
                                base.valid = false;
                                //XmlNode argoNode2 = oFrmElmt;
                                base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "These database connection details could not connect.");
                                //oFrmElmt = (XmlElement)argoNode2;
                            }

                            if (base.valid)
                            {
                                if (oCfg != null)
                                {
                                    DefaultSection oCgfSect = (DefaultSection)oCfg.GetSection("protean/web");
                                    IgnoreSection oRwSect = (IgnoreSection)oCfg.GetSection("system.webServer");
                                    if (oCgfSect != null)
                                    {
                                        oCgfSect.SectionInformation.RestartOnExternalChanges = false;
                                        oCgfSect.SectionInformation.SetRawXml(base.Instance.InnerXml);
                                        // oRwSect.SectionInformation.SetRawXml(oDefaultCfgXml.SelectSingleNode("/configuration/system.webServer").OuterXml)
                                        oCfg.Save();
                                    }
                                    else
                                    {
                                        // update config based on form submission
                                        oDefaultCfgXml.SelectSingleNode("/configuration/protean").InnerXml = base.Instance.InnerXml;
                                        // save as web.config in the root
                                        oDefaultCfgXml.Save(this.goServer.MapPath("protean.web.config"));
                                    }
                                }
                                else
                                {
                                    // update config based on form submission
                                    oDefaultCfgXml.SelectSingleNode("/configuration/protean").InnerXml = base.Instance.InnerXml;
                                    // save as web.config in the root
                                    oDefaultCfgXml.Save(this.goServer.MapPath("web.config"));
                                }
                            }
                            oDbt = default;
                        }

                        // CreateDirs = Nothing

                    }
                    // End If
                    // oImp.UndoImpersonation()
                    // lets take a guess at the DB Name
                    if (string.IsNullOrEmpty(this.Instance.SelectSingleNode("web/add[@key='DatabaseName']/@value").InnerText))
                    {
                        this.Instance.SelectSingleNode("web/add[@key='DatabaseName']/@value").InnerText = GuessDBName();
                    }

                    // If Instance.SelectSingleNode("web/add[@key='VersionNumber']/@value").InnerText = "" Then
                    // Instance.SelectSingleNode("web/add[@key='VersionNumber']/@value").InnerText = "4.1.0.0"
                    // End If

                    // Else
                    // MyBase.addNote(oFrmElmt, noteTypes.Alert, "Admin credentials need to be configured correctly in the web.config", True)
                    // End If

                    base.addValues();
                    return base.moXformElmt;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref Protean.xForm.msException, mcModuleName, "xFrmWebSettings", ex, "", cProcessInfo, true);
                    return null;
                }
            }


            public XmlElement xFrmBackupDatabase()
            {
                XmlElement oFrmElmt;
                string cProcessInfo = "";
                Protean.fsHelper oFsh;

                string DatabaseName = goConfig["DatabaseName"];
                string DatabaseFilename = $"{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}-{DateTime.Now:HH.mm.ss}-{goConfig["DatabaseName"]}.bak";
                string DatabaseFilepath = this.goServer.MapPath("/") + @"..\data";

                try
                {
                    oFsh = new Protean.fsHelper();
                    oFsh.open(this.moPageXML);

                    base.NewFrm("BackupDatabase");

                    base.submission("BackupDatabase", "", "post", "form_check(this)");

                    oFrmElmt = base.addGroup(ref base.moXformElmt, "BackupDatabase", "", "Backup Database");

                    //XmlNode argoNode = oFrmElmt;
                    base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Hint, "Please enter your database connection details.");
                    //oFrmElmt = (XmlElement)argoNode;

                    base.addInput(ref oFrmElmt, "ewDatabaseName", true, "Database Name");
                    XmlElement argoBindParent = null;
                    base.addBind("ewDatabaseName", "backup/@name", oBindParent: ref argoBindParent, "true()");

                    base.addInput(ref oFrmElmt, "ewDatabaseFilename", true, "Backup Filename");
                    XmlElement argoBindParent1 = null;
                    base.addBind("ewDatabaseFilename", "backup/@filename", oBindParent: ref argoBindParent1, "false()");

                    base.addInput(ref oFrmElmt, "ewDatabaseFilepath", true, "Backup Filepath");
                    XmlElement argoBindParent2 = null;
                    base.addBind("ewDatabaseFilepath", "backup/@filepath", oBindParent: ref argoBindParent2, "false()");

                    base.addSubmit(ref oFrmElmt, "", "Backup Database");

                    Tools.Security.Impersonate oImp = null;
                    if (!string.IsNullOrEmpty(goConfig["AdminAcct"]))
                    {
                        oImp = new Tools.Security.Impersonate();
                        if (oImp.ImpersonateValidUser(goConfig["AdminAcct"], goConfig["AdminDomain"], goConfig["AdminPassword"], true, goConfig["AdminGroup"]))
                        {
                        }
                        else
                        {
                            //XmlNode argoNode1 = oFrmElmt;
                            base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "Admin credentials need to be configured correctly in the web.config", true);
                            //oFrmElmt = (XmlElement)argoNode1;
                        }
                    }

                    base.Instance.InnerXml = "<backup name=\"" + DatabaseName + "\" filename=\"" + DatabaseFilename + "\" filepath=\"" + DatabaseFilepath + "\"/>";

                    if (base.isSubmitted())
                    {
                        base.updateInstanceFromRequest();
                        base.validate();
                        if (base.valid)
                        {

                            var oDB = new Tools.Database();

                            oDB.DatabaseServer = goConfig["DatabaseServer"];
                            oDB.DatabaseUser = Tools.Text.SimpleRegexFind(goConfig["DatabaseAuth"], "user id=([^;]*)", 1, RegexOptions.IgnoreCase);
                            oDB.DatabasePassword = Tools.Text.SimpleRegexFind(goConfig["DatabaseAuth"], "password=([^;]*)", 1, RegexOptions.IgnoreCase);
                            oDB.ConnectTimeout = 60;
                            // oDB.ConnectionPooling = True
                            oDB.BackupDatabase(DatabaseName, DatabaseFilepath);
                        }
                    }
                    // End If


                    if (!string.IsNullOrEmpty(goConfig["AdminAcct"]))
                    {
                        oImp.UndoImpersonation();
                        oImp = null;
                    }

                    base.addValues();
                    return base.moXformElmt;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref Protean.xForm.msException, mcModuleName, "xFrmBackupDatabase", ex, "", cProcessInfo, true);
                    return null;
                }
            }

            public XmlElement xFrmRestoreDatabase()
            {
                XmlElement oFrmElmt;
                string cProcessInfo = "";
                Protean.fsHelper oFsh;

                string DatabaseName = goConfig["DatabaseName"];
                string DatabaseFilename = string.Empty;
                string DatabaseFilepath = this.goServer.MapPath("/") + @"..\data";

                try
                {
                    oFsh = new Protean.fsHelper();
                    oFsh.open(this.moPageXML);

                    base.NewFrm("RestoreDatabase");

                    base.submission("RestoreDatabase", "", "post", "form_check(this)");

                    oFrmElmt = base.addGroup(ref base.moXformElmt, "RestoreDatabase", "", "Restore Database");

                    //XmlNode argoNode = oFrmElmt;
                    base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Hint, "Please select the database to restore.");
                    //oFrmElmt = (XmlElement)argoNode;

                    base.addInput(ref oFrmElmt, "ewDatabaseName", true, "Database Name to be Overwritten");
                    XmlElement argoBindParent = null;
                    base.addBind("ewDatabaseName", "restore/@name", oBindParent: ref argoBindParent, "true()");

                    var sel1 = base.addSelect1(ref oFrmElmt, "ewDatabaseFilename", true, "Select a backup allready on the server");
                    base.addOptionsFilesFromDirectory(ref sel1, DatabaseFilepath);
                    XmlElement argoBindParent1 = null;
                    base.addBind("ewDatabaseFilename", "restore/@filename", oBindParent: ref argoBindParent1, "false()");

                    string argsClass = "";
                    base.addUpload(ref oFrmElmt, "ewDatabaseUpload", true, "bak,zip", "Or upload here....", sClass: ref argsClass);
                    XmlElement argoBindParent2 = null;
                    base.addBind("ewDatabaseUpload", "restore/@upload", oBindParent: ref argoBindParent2, "false()");

                    base.addSubmit(ref oFrmElmt, "", "Restore Database");

                    var oImp = new Tools.Security.Impersonate();
                    if (oImp.ImpersonateValidUser(goConfig["AdminAcct"], goConfig["AdminDomain"], goConfig["AdminPassword"], true, goConfig["AdminGroup"]))
                    {

                        base.Instance.InnerXml = "<restore name=\"" + DatabaseName + "\" filename=\"" + DatabaseFilename + "\" filepath=\"" + DatabaseFilepath + "\" upload=\"\"/>";

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            // lets do some hacking 
                            System.Web.HttpPostedFile fUpld;
                            fUpld = this.goRequest.Files["ewDatabaseUpload"];

                            if (fUpld.ContentLength == 0 & string.IsNullOrEmpty(this.goRequest["ewDatabaseFilename"]))
                            {
                                base.valid = false;
                                //XmlNode argoNode1 = oFrmElmt;
                                base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "Please Specify a file to restore");
                                //oFrmElmt = (XmlElement)argoNode1;
                            }

                            if (base.valid)
                            {

                                if (fUpld.ContentLength == 0)
                                {
                                    DatabaseFilename = this.goRequest["ewDatabaseFilename"];
                                }
                                else
                                {
                                    var oFs = new Protean.fsHelper();
                                    // oFs.initialiseVariables(nType)
                                    string sValidResponse;
                                    sValidResponse = oFs.SaveFile(ref fUpld, DatabaseFilepath);
                                    DatabaseFilename = DatabaseFilepath + @"\" + fUpld.FileName;
                                }

                                var oDB = new Tools.Database();

                                oDB.DatabaseServer = goConfig["DatabaseServer"];
                                oDB.DatabaseUser = Tools.Text.SimpleRegexFind(goConfig["DatabaseAuth"], "user id=([^;]*)", 1, RegexOptions.IgnoreCase);
                                oDB.DatabasePassword = Tools.Text.SimpleRegexFind(goConfig["DatabaseAuth"], "password=([^;]*)", 1, RegexOptions.IgnoreCase);
                                oDB.FTPUser = goConfig["DatabaseFtpUsername"];
                                oDB.FtpPassword = goConfig["DatabaseFtpPassword"];
                                oDB.RestoreDatabase(DatabaseName, DatabaseFilename);

                            }
                        }
                        oImp.UndoImpersonation();
                    }

                    else
                    {
                        //XmlNode argoNode2 = oFrmElmt;
                        base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "Admin credentials need to be configured correctly in the web.config", true);
                        //oFrmElmt = (XmlElement)argoNode2;
                    }

                    base.addValues();
                    return base.moXformElmt;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref Protean.xForm.msException, mcModuleName, "xFrmRestoreDatabase", ex, "", cProcessInfo, true);
                    return null;
                }
            }

            public XmlElement xFrmNewDatabase()
            {
                XmlElement oFrmElmt;
                string cProcessInfo = "";
                Protean.fsHelper oFsh;

                string DatabaseName = goConfig["DatabaseName"];
                string DatabaseFilename = "NewV4";
                string DatabaseFilepath = this.goServer.MapPath("/ewcommon/setup/db");

                try
                {
                    oFsh = new Protean.fsHelper();
                    oFsh.open(this.moPageXML);

                    base.NewFrm("NewDatabase");

                    base.submission("NewDatabase", "", "post", "form_check(this)");

                    oFrmElmt = base.addGroup(ref base.moXformElmt, "NewDatabase", "", "New Database");

                    //XmlNode argoNode = oFrmElmt;
                    base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Hint, "Create the ProteanCMS Database Tables");
                    //oFrmElmt = (XmlElement)argoNode;

                    base.addInput(ref oFrmElmt, "ewDatabaseName", true, "Database Name");
                    XmlElement argoBindParent = null;
                    base.addBind("ewDatabaseName", "restore/@name", oBindParent: ref argoBindParent, "true()");

                    var sel1 = base.addSelect1(ref oFrmElmt, "ewDatabaseFilename", true, "Install Empty DB or one containing sample data which would be better for initial evaluation of the platform", "", Protean.xForm.ApperanceTypes.Full);
                    base.addOption(ref sel1, "Empty Database", "NewV4");
                    base.addOptionsFilesFromDirectory(ref sel1, DatabaseFilepath, "zip");
                    XmlElement argoBindParent1 = null;
                    base.addBind("ewDatabaseFilename", "restore/@filename", oBindParent: ref argoBindParent1, "false()");


                    base.addSubmit(ref oFrmElmt, "", "Create Database");

                    base.Instance.InnerXml = "<restore name=\"" + DatabaseName + "\" filename=\"" + DatabaseFilename + "\" filepath=\"" + DatabaseFilepath + "\"/>";

                    if (base.isSubmitted())
                    {
                        base.updateInstanceFromRequest();
                        base.validate();

                        if (base.valid)
                        {
                            DatabaseFilename = this.goRequest["ewDatabaseFilename"];
                            if (DatabaseFilename == "NewV4")
                            {
                                // mySetup.buildDatabase(True)
                                mySetup.cPostFlushActions = "NewDB";
                            }
                            else
                            {
                                mySetup.cPostFlushActions = "RestoreZip";



                            }
                        }
                    }

                    base.addValues();
                    return base.moXformElmt;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref Protean.xForm.msException, mcModuleName, "xFrmRestoreDatabase", ex, "", cProcessInfo, true);
                    return null;
                }
            }


        }

    }
}
