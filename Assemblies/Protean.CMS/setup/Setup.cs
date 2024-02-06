using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Xml;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Protean.Providers.Membership;
using static Protean.stdTools;

namespace Protean
{


    public class Setup
    {

        // This is the version of the new build before the update......
        public string mnCurrentVersion = "4.1.0.45";

        // Session ProteanCMS Details
        public bool mbEwMembership;
        public bool mbEwCart;
        public bool mbEwGiftList;
        public XmlDocument moPageXml = new XmlDocument();
        public bool mbOutputXml = false;

        public System.Web.HttpContext moCtx;

        public System.Web.HttpApplicationState goApp;
        public System.Web.HttpRequest goRequest;
        public System.Web.HttpResponse goResponse;
        public System.Web.SessionState.HttpSessionState goSession;
        public System.Web.HttpServerUtility goServer;

        public System.Collections.Specialized.NameValueCollection goConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");

        public Cms myWeb;
        private Protean.Cms.dbHelper _moDbHelper;

        public virtual Protean.Cms.dbHelper moDbHelper
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _moDbHelper;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_moDbHelper != null)
                {
                    _moDbHelper.OnError -= _OnError;
                    _moDbHelper.OnError -= OnComponentError;
                }

                _moDbHelper = value;
                if (_moDbHelper != null)
                {
                    _moDbHelper.OnError += _OnError;
                    _moDbHelper.OnError += OnComponentError;
                }
            }
        }

        public int gnTopLevel;
        public int gnPageId = 0;
        public int mnArtId = 0;
        public int mnUserId = 0;
        public string mcEwCmd;

        public string mcModuleName = "Protean.Setup";
        public bool gbDebug = false;

        private string cStep = "";
        public XmlElement oResponse;
        //private XmlElement oContentElmt;
        public string cPostFlushActions = "";

        private Protean.XmlHelper.Transform oTransform = new Protean.XmlHelper.Transform();
        private string msRedirectOnEnd = "";
        private bool mbSchemaExists = false;
        private bool ConnValid = false;
        private bool mbDBExists = false;
        private bool isAlt = false;

        #region ErrorHandling

        // for anything controlling web
        public event OnErrorEventHandler OnError;

        public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);

        private void _OnError(object sender, Tools.Errors.ErrorEventArgs e)
        {
            // RaiseEvent OnError(sender, e)
            Information.Err().Raise(513, e.ProcedureName, e.AddtionalInformation + " - " + e.Exception.Message);
        }

        protected virtual void OnComponentError(object sender, Tools.Errors.ErrorEventArgs e)
        {
            // deals with the error
            stdTools.returnException(ref myWeb.msException, e.ModuleName, e.ProcedureName, e.Exception, "/ewcommon/xsl/admin/setup.xsl", e.AddtionalInformation, gbDebug);
            // close connection pooling
            if (myWeb.moDbHelper != null)
            {
                try
                {
                    myWeb.moDbHelper.CloseConnection();
                }
                catch (Exception)
                {
                }
            }

            // then raises a public event
            OnError?.Invoke(sender, e);
        }

        #endregion


        #region Class Initialize Procedures


        public Setup() : this(System.Web.HttpContext.Current)
        {

        }

        public Setup(System.Web.HttpContext Context)
        {
            string sProcessInfo = string.Empty;
            try
            {
                moCtx = Context;

                if (moCtx is null)
                {
                    moCtx = System.Web.HttpContext.Current;
                }

                goApp = moCtx.Application;
                goRequest = moCtx.Request;
                goResponse = moCtx.Response;
                goSession = moCtx.Session;
                goServer = moCtx.Server;

                myWeb = new Cms(moCtx);
                // myWeb.InitializeVariables()

                sProcessInfo = "set session variables";
                mcModuleName = "ProteanCMS.Setup";
                // msException = ""

                // Set the debug mode
                if (goConfig["Debug"] != null)
                {
                    switch (Strings.LCase(goConfig["Debug"]) ?? "")
                    {
                        case "on":
                            {
                                gbDebug = true;
                                break;
                            }
                        case "off":
                            {
                                gbDebug = false;
                                break;
                            }

                        default:
                            {
                                gbDebug = false;
                                break;
                            }
                    }
                }

                if (goSession != null)
                {
                    if (Conversions.ToBoolean(Operators.OrObject(Operators.ConditionalCompareObjectEqual(goSession["nUserId"], null, false), Operators.ConditionalCompareObjectEqual(goSession["nUserId"], 0, false))))
                    {
                    }
                    // this will get set on close
                    else
                    {
                        // lets finally set the user Id from the session
                        mnUserId = Conversions.ToInteger(goSession["nUserId"]);
                    }
                }

                // lets open our DB Helper if database is defined
                if (goConfig["DatabaseName"] != default)
                {
                    myWeb.moDbHelper = new Cms.dbHelper(goConfig["DatabaseServer"], goConfig["DatabaseName"], mnUserId, moCtx);
                    myWeb.moDbHelper.myWeb = myWeb;
                    myWeb.moDbHelper.moPageXml = moPageXml;
                    myWeb.moDbHelper.DatabaseUser = goConfig["DatabaseUsername"];
                    myWeb.moDbHelper.DatabasePassword = goConfig["DatabasePassword"];

                    ConnValid = myWeb.moDbHelper.CredentialsValid;
                    if (ConnValid)
                    {
                        if (myWeb.moDbHelper.checkDBObjectExists("tblContent", Tools.Database.objectTypes.Table))
                        {
                            mbSchemaExists = true;
                            mbDBExists = true;
                        }
                        else
                        {
                            if (myWeb.moDbHelper.checkDBObjectExists(goConfig["DatabaseName"], Tools.Database.objectTypes.Database))
                            {
                                mbDBExists = true;
                            }
                            mnUserId = 1;
                        }
                    }



                }

                oResponse = moPageXml.CreateElement("ProgressResponses");
            }

            catch (Exception ex)
            {
                AddResponseError(ex); // returnException(myWeb.msException, mcModuleName, "open", ex, "", sProcessInfo, gbDebug)
            }

        }

        public void close()
        {
            string sProcessInfo = string.Empty;
            try
            {
                // if we access base via soap the session is not available
                if (mbSchemaExists)
                {
                    if (goSession != null)
                    {
                        goSession["nUserId"] = mnUserId;
                    }
                }

                moPageXml = null;
            }

            catch (Exception ex)
            {
                AddResponseError(ex); // returnException(myWeb.msException, mcModuleName, "close", ex, "", sProcessInfo, gbDebug)
            }

        }

        public void AddResponse(string cResponse)
        {
            try
            {


                if (string.IsNullOrEmpty(cPostFlushActions))
                {
                    if (oResponse != null)
                    {
                        var oElmt = oResponse.OwnerDocument.CreateElement("ProgressResponse");
                        oElmt.InnerText = cResponse;
                        oResponse.AppendChild(oElmt);
                    }
                }
                else
                {
                    goResponse.Write("<script language=\"javascript\" type=\"text/javascript\">$('#result').append('" + Strings.Replace(cResponse, "'", @"\'") + "<br/>');$('#result').stop().animate({scrollTop: $('#result')[0].scrollHeight}, 800);</script>" + Constants.vbCrLf);
                }
            }

            catch (Exception)
            {
                goResponse.Write("<script language=\"javascript\" type=\"text/javascript\">$('#result').append('" + Strings.Replace("<p><i class=\"fa fa-check text-danger\">&#160;</i>Error in script</p>", "'", @"\'") + "<br/>');$('#result').stop().animate({scrollTop: $('#result')[0].scrollHeight}, 800);</script>" + Constants.vbCrLf);
            }
        }

        public void AddResponseComplete(string cResponse, string LinkPath)
        {
            if (string.IsNullOrEmpty(cPostFlushActions))
            {
                if (oResponse != null)
                {
                    var oElmt = oResponse.OwnerDocument.CreateElement("ProgressResponse");
                    oElmt.InnerText = cResponse;
                    oResponse.AppendChild(oElmt);
                }
            }
            else
            {
                goResponse.Write("<script language=\"javascript\" type=\"text/javascript\">$('#completeButton').attr('href','" + LinkPath + "');</script>" + Constants.vbCrLf);
                goResponse.Write("<script language=\"javascript\" type=\"text/javascript\">$('#completeButton').html('" + cResponse + "');</script>" + Constants.vbCrLf);
                goResponse.Write("<script language=\"javascript\" type=\"text/javascript\">$('#completeModal').modal('show');</script>" + Constants.vbCrLf);
            }
        }

        public void AddResponseError(Exception oEx, string cProcessInfo = "")
        {
            if (string.IsNullOrEmpty(cPostFlushActions))
            {
                AddResponse(Strings.Replace(Strings.Replace(Strings.Replace(oEx.ToString(), Conversions.ToString('\r'), "<br/>"), "&gt;", ">"), "&lt;", "<"));
            }
            else
            {
                AddResponse("ERROR:" + oEx.Message + " - " + oEx.Source + "<br/>" + Strings.Replace(Strings.Replace(oEx.StackTrace, Constants.vbCr, "<br/>"), Constants.vbLf, "<br/>" + cProcessInfo));
            }
        }

        #endregion

        #region Misc Migration Start Procedures

        public void GetPageHTML()
        {

            string sProcessInfo = "";
            try
            {

                if (goApp["JSEngineEnabled"] is null)
                {
                    // Dim msieCfg As New JavaScriptEngineSwitcher.Msie.MsieSettings()
                    // msieCfg.EngineMode = JavaScriptEngineSwitcher.Msie.JsEngineMode.ChakraIeJsRt
                    JavaScriptEngineSwitcher.Core.JsEngineSwitcher engineSwitcher = (JavaScriptEngineSwitcher.Core.JsEngineSwitcher)JavaScriptEngineSwitcher.Core.JsEngineSwitcher.Current;
                    // engineSwitcher.EngineFactories.Add(New JavaScriptEngineSwitcher.ChakraCore.ChakraCoreJsEngineFactory())
                    // engineSwitcher.EngineFactories.Add(New JavaScriptEngineSwitcher.Msie.MsieJsEngineFactory(msieCfg))
                    engineSwitcher.EngineFactories.Add(new JavaScriptEngineSwitcher.V8.V8JsEngineFactory());
                    string sJsEngine = "V8JsEngine";
                    if (!string.IsNullOrEmpty(goConfig["JSEngine"]))
                    {
                        sJsEngine = goConfig["JSEngine"];
                    }
                    engineSwitcher.DefaultEngineName = sJsEngine;
                    goApp["JSEngineEnabled"] = sJsEngine;
                }

                myWeb.msException = "";
                GetSetupXml();

                myWeb.msException = "";

                sProcessInfo = "Transform PageXML using XSLT";

                if (!string.IsNullOrEmpty(msRedirectOnEnd) & string.IsNullOrEmpty(cPostFlushActions))
                {
                    goResponse.Redirect(msRedirectOnEnd);
                    return;
                }

                // If Not msException = "" Then
                // AddResponse(msException)
                // msException = ""
                // End If

                if (mbOutputXml == true)
                {
                    goResponse.Write("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + moPageXml.OuterXml);
                }
                else
                {
                    goResponse.Buffer = false;
                    if (goConfig["cssFramework"] == "bs5")
                    {
                        oTransform.XSLFile = goServer.MapPath("/ptn/setup/setup.xsl");
                    }
                    else
                    {
                        oTransform.XSLFile = goServer.MapPath("/ewcommon/xsl/admin/setup.xsl");
                    }

                    oTransform.Compiled = false;
                    oTransform.ProcessTimed(moPageXml, ref goResponse);
                    oTransform = (Protean.XmlHelper.Transform)null;

                    if (!string.IsNullOrEmpty(cPostFlushActions))
                    {
                        goResponse.Flush();
                        PostFlushActions();
                    }

                }
                close();
            }

            catch (Exception ex)
            {

                stdTools.returnException(ref myWeb.msException, mcModuleName, "getPageHtml", ex, "", sProcessInfo, gbDebug);

            }

        }

        public string ReturnPageHTML(long nPageId = 0L, bool bReturnBlankError = false)
        {
            // PerfMon.Log("Setup", "ReturnPageHTML")
            string sProcessInfo = "Transform PageXML using XSLT";
            string cPageHTML;
            var icPageWriter = new StringWriter();
            try
            {

                myWeb.msException = "";
                GetSetupXml();

                myWeb.msException = "";

                string styleFile = goServer.MapPath("/ewcommon/xsl/admin/setup.xsl");
                myWeb.msException = "";

                var oTransform = new Protean.XmlHelper.Transform(ref myWeb, styleFile, false);
                oTransform.mbDebug = gbDebug;
                TextWriter argoWriter = icPageWriter;
                oTransform.ProcessTimed(moPageXml, ref argoWriter);
                icPageWriter = (StringWriter)argoWriter;
                oTransform = (Protean.XmlHelper.Transform)null;

                cPageHTML = Strings.Replace(icPageWriter.ToString(), "<?xml version=\"1.0\" encoding=\"utf-16\"?>", "");
                cPageHTML = Strings.Replace(cPageHTML, "<?xml version=\"1.0\" encoding=\"UTF-8\"?>", "");

                if (bReturnBlankError & !(myWeb.msException == ""))
                {
                    return "";
                }
                else
                {
                    return cPageHTML;
                }
            }

            catch (Exception ex)
            {

                stdTools.returnException(ref myWeb.msException, mcModuleName, "returnPageHtml", ex, "/ewcommon/xsl/admin/setup.xsl", sProcessInfo, gbDebug);
                if (bReturnBlankError)
                {
                    return "";
                }
                else
                {
                    return myWeb.msException;
                }
            }
            finally
            {
                icPageWriter.Dispose();
            }

        }

        public void PostFlushActions()
        {

            switch (cPostFlushActions ?? "")
            {
                case "ImportV3":
                    {
                        buildDatabase(false);
                        break;
                    }
                case "UpgradeDB":
                    {
                        UpdateDatabase();
                        AddResponseComplete("Congratulations - Go to updated website", "/");
                        break;
                    }
                case "NewDB":
                    {
                        buildDatabase(true);
                        break;
                    }
                case "RestoreZip":
                    {

                        var oDB = new Tools.Database();
                        oDB.DatabaseServer = goConfig["DatabaseServer"];
                        oDB.DatabaseUser = Tools.Text.SimpleRegexFind(goConfig["DatabaseAuth"], "user id=([^;]*)", 1, RegexOptions.IgnoreCase);
                        oDB.DatabasePassword = Tools.Text.SimpleRegexFind(goConfig["DatabaseAuth"], "password=([^;]*)", 1, RegexOptions.IgnoreCase);
                        oDB.FTPUser = goConfig["DatabaseFtpUsername"];
                        oDB.FtpPassword = goConfig["DatabaseFtpPassword"];
                        oDB.RestoreDatabase(goConfig["DatabaseName"], goRequest.Form["ewDatabaseFilename"]);
                        break;
                    }
                case "RunTests":
                    {
                        RunTests();
                        break;
                    }
            }

        }
        public void RunTests()
        {
            var oTests = new Protean.Tests();
            var testCount = default(int);
            string testResponse = "";

            testResponse = oTests.TestImpersonation();
            if (!testResponse.StartsWith("Impersonation"))
            {
                AddResponse("<p><i class=\"fa fa-times text-danger\">&#160;</i>" + testResponse + "</p>");
            }
            else
            {
                AddResponse("<p><i class=\"fa fa-check text-success\">&#160;</i>" + testResponse + "</p>");
            }
            testCount = testCount + 1;


            testResponse = oTests.TestEmailSend();
            if (testResponse != @"Message\rSent")
            {
                AddResponse("<p><i class=\"fa fa-times text-danger\">&#160;</i>Email SEND FAILED</p>");
                AddResponse(testResponse);
            }
            else
            {
                AddResponse("<p><i class=\"fa fa-check text-success\">&#160;</i>Email Sent</p>");
            }
            testCount = testCount + 1;

            testResponse = oTests.TestCreateFolder();
            if (!testResponse.StartsWith("Folder Created"))
            {
                AddResponse("<p><i class=\"fa fa-times text-danger\">&#160;</i>" + testResponse + "</p>");
            }
            else
            {
                AddResponse("<p><i class=\"fa fa-check text-success\">&#160;</i>" + testResponse + "</p>");
            }
            testCount = testCount + 1;


            testResponse = oTests.TestWriteFile();
            if (!testResponse.StartsWith("File Written"))
            {
                AddResponse("<p><i class=\"fa fa-times text-danger\">&#160;</i>" + testResponse + "</p>");
            }
            else
            {
                AddResponse("<p><i class=\"fa fa-check text-success\">&#160;</i>" + testResponse + "</p>");
            }
            testCount = testCount + 1;

            testResponse = oTests.TestWriteFileAlphaFS();
            if (!testResponse.StartsWith("File Written"))
            {
                AddResponse("<p><i class=\"fa fa-times text-danger\">&#160;</i>" + testResponse + "</p>");
            }
            else
            {
                AddResponse("<p><i class=\"fa fa-check text-success\">&#160;</i>" + testResponse + "</p>");
            }
            testCount = testCount + 1;

            testResponse = oTests.TestDeleteFile();
            if (!testResponse.StartsWith("File Deleted"))
            {
                AddResponse("<p><i class=\"fa fa-times text-danger\">&#160;</i>" + testResponse + "</p>");
            }
            else
            {
                AddResponse("<p><i class=\"fa fa-check text-success\">&#160;</i>" + testResponse + "</p>");
            }
            testCount = testCount + 1;


            testResponse = oTests.TestDeleteFolder();
            if (!testResponse.StartsWith("Folder Deleted"))
            {
                AddResponse("<p><i class=\"fa fa-times text-danger\">&#160;</i>" + testResponse + "</p>");
            }
            else
            {
                AddResponse("<p><i class=\"fa fa-check text-success\">&#160;</i>" + testResponse + "</p>");
            }
            testCount = testCount + 1;

            testResponse = oTests.TestHtmlTidy();
            if (!testResponse.StartsWith("HTML Tidy is working"))
            {
                AddResponse("<p><i class=\"fa fa-times text-danger\">&#160;</i>Tidy has failed.</p>");
            }
            else
            {
                AddResponse("<p><i class=\"fa fa-check text-success\">&#160;</i>" + testResponse + "</p>");
            }
            testCount = testCount + 1;


            // 6 test the ability to update config settins
            // 7 test the ability to write to the index folder location

            // 9 database integrety tests

            AddResponse("<h1> " + testCount.ToString() + " Tests Complete</h1>");

        }

        public void GetSetupXml()
        {
            XmlElement oPageElmt;
            string sProcessInfo = "";

            //string sLayout = "default";

            try
            {

                if (moPageXml.DocumentElement is null)
                {
                    moPageXml.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                    oPageElmt = moPageXml.CreateElement("Page");
                    moPageXml.AppendChild(oPageElmt);
                }
                else
                {
                    oPageElmt = moPageXml.DocumentElement;
                }

                // introduce the layout

                oPageElmt.SetAttribute("layout", "default");
                oPageElmt.SetAttribute("ewCmd", goRequest["ewCmd"]);
                oPageElmt.SetAttribute("ewCmd2", goRequest["ewCmd2"]);
                oPageElmt.SetAttribute("cssFramework", "bs3");
                oPageElmt.SetAttribute("adminMode", "true");
                mcEwCmd = goRequest["ewCmd"];
                setupProcessXml();
                if (mnUserId > 1)
                {
                    // lets check the current users permission level
                    if (!myWeb.moDbHelper.checkUserRole("Administrator"))
                    {
                        mcEwCmd = "LogOff";
                    }
                    else
                    {
                        oPageElmt.AppendChild(myWeb.moDbHelper.GetUserXML(mnUserId));
                        setupMenuXml();
                    }
                }
                else if (mnUserId > 0)
                {
                    // we are AD Authenticated or admin
                    // oPageElmt.AppendChild(moDbHelper.GetUserXML(mnUserId))
                    setupMenuXml();
                }
                else
                {
                    mnUserId = 0;
                    mcEwCmd = "";
                }

                // add the page content

                // setupProcessXml()
                oPageElmt.SetAttribute("Step", cStep);
            }

            catch (Exception ex)
            {
                stdTools.returnException(ref myWeb.msException, mcModuleName, "GetSetupXML", ex, "", sProcessInfo, gbDebug);
            }

        }

        private void setupProcessXml()
        {

            string cProcessInfo = "";

            try
            {
                XmlElement oRoot;
                XmlElement oElmt;
                XmlElement oPageDetail;

                oRoot = moPageXml.CreateElement("Contents");
                moPageXml.DocumentElement.AppendChild(oRoot);
                oElmt = moPageXml.CreateElement("Content");
                oRoot.AppendChild(oElmt);
                oPageDetail = moPageXml.CreateElement("ContentDetail");
                moPageXml.DocumentElement.AppendChild(oPageDetail);
                if (mbSchemaExists)
                {
                    mnUserId = Conversions.ToInteger(goSession["nUserId"]);
                }

            Recheck:
                ;

                if (string.IsNullOrEmpty(goConfig["DatabaseName"]) | ConnValid == false)
                {
                    // Step 1. Create a Web.Config and various other files.

                    var argasetup = this;
                    var oSetXfm = new SetupXforms(ref argasetup);
                    oPageDetail.AppendChild(oSetXfm.xFrmWebSettings());
                    moPageXml.DocumentElement.SetAttribute("layout", "AdminXForm");
                    if (Conversions.ToBoolean(Operators.AndObject(oSetXfm.valid, Operators.OrObject(goSession["nUserId"] is null, Operators.ConditionalCompareObjectEqual(goSession["nUserId"], 0, false)))))
                    {
                        goSession["nUserId"] = 1;
                        msRedirectOnEnd = "/ewcommon/setup/?ewCmd=NewV4&ewCmd2=Do";
                        // GoTo Recheck
                    }
                }

                else if (mbSchemaExists == false)
                {

                    var argasetup1 = this;
                    var oSetXfm = new SetupXforms(ref argasetup1);
                    oPageDetail.AppendChild(oSetXfm.xFrmNewDatabase());
                    if (oSetXfm.valid)
                    {
                        cStep = 1.ToString();
                        cPostFlushActions = "NewDB";
                    }
                    else
                    {
                        cStep = 2.ToString();
                    }
                    mcEwCmd = "NewDatabase";
                }

                // msRedirectOnEnd = "/ewcommon/setup/?ewCmd=NewV4"

                else if (mnUserId == 0)
                {
                    // Dim oAdXfm As Cms.Admin.AdminXforms = New Cms.Admin.AdminXforms(myWeb)
                    // oAdXfm.open(moPageXml)

                    ReturnProvider RetProv = new Protean.Providers.Membership.ReturnProvider();
                    IMembershipProvider oMembershipProv = RetProv.Get(ref myWeb, myWeb.moConfig["MembershipProvider"]);

                    IMembershipAdminXforms oAdXfm = oMembershipProv.AdminXforms;
                    oAdXfm.open(moPageXml);
                    oPageDetail.AppendChild((XmlNode)oAdXfm.xFrmUserLogon("AdminLogon"));
                    mnUserId = myWeb.mnUserId;
                    if (Conversions.ToBoolean(oAdXfm.valid))
                    {

                        if (!myWeb.moDbHelper.checkUserRole("Administrator") | mnUserId != 1)
                        {
                            moPageXml.DocumentElement.SetAttribute("layout", "AdminXForm");
                            mcEwCmd = "Logon";
                        }
                        else
                        {
                            mnUserId = myWeb.mnUserId;
                            oRoot.AppendChild(myWeb.moDbHelper.GetUserXML(mnUserId));
                            if (mnUserId > 0)
                            {
                                mcEwCmd = "Home";
                            }
                            else
                            {
                                moPageXml.DocumentElement.SetAttribute("layout", "AdminXForm");
                            }
                        }
                    }
                    else
                    {
                        moPageXml.DocumentElement.SetAttribute("layout", "AdminXForm");
                        mcEwCmd = "Logon";
                    }
                    // set the userId on the DBHelper
                    if (myWeb.moDbHelper != null)
                    {
                        myWeb.moDbHelper.mnUserId = mnUserId;
                    }
                }
                if (mnUserId == 0)
                    mnUserId = Conversions.ToInteger(goSession["nUserId"]);
                if (mcEwCmd == "LogOff")
                    mcEwCmd = goRequest["ewCmd"];
                if (mnUserId > 0)
                {
                    switch (mcEwCmd ?? "")
                    {
                        case "Logoff":
                            {
                                mnUserId = 0;
                                goSession["nUserId"] = 0;
                                mcEwCmd = "";
                                goto Recheck;
                                
                            }
                        case "ClearDB":
                            {
                                if (goRequest["ewCmd2"] == "Do")
                                {
                                    if (Migrate_Rollback())
                                    {
                                        cStep = 1.ToString();
                                    }
                                    else
                                    {
                                        cStep = 2.ToString();
                                    }
                                }

                                break;
                            }
                        case "NewV4":
                            {
                                if (goRequest["ewCmd2"] == "Do")
                                {
                                    if (buildDatabase(true))
                                    {
                                        cStep = 1.ToString();
                                    }
                                    else
                                    {
                                        cStep = 2.ToString();
                                    }
                                }

                                break;
                            }
                        case "ShipLoc":
                            {
                                if (goRequest["ewCmd2"] == "Do")
                                {
                                    if (LoadShippingLocations())
                                    {
                                        cStep = 1.ToString();
                                    }
                                    else
                                    {
                                        cStep = 2.ToString();
                                    }
                                }

                                break;
                            }
                        case "ImportV3":
                            {
                                if (goRequest["ewCmd2"] == "Do")
                                {
                                    cStep = 1.ToString();
                                    cPostFlushActions = "ImportV3";

                                    // If buildDatabase(False) Then
                                    // cStep = 1
                                    // Else
                                    // cStep = 2
                                    // End If

                                }

                                break;
                            }
                        case "UpgradeDB":
                            {
                                if (goRequest["ewCmd2"] == "Do")
                                {
                                    cStep = 1.ToString();
                                    cPostFlushActions = "UpgradeDB";

                                    // If UpdateDatabase2() Then
                                    // cStep = 1
                                    // Else
                                    // cStep = 2
                                    // End If
                                }

                                break;
                            }
                        case "CleanAudit":
                            {
                                if (goRequest["ewCmd2"] == "Do")
                                {
                                    AddResponse(myWeb.moDbHelper.CleanAuditOrphans());
                                    cStep = 1.ToString();
                                }

                                break;
                            }
                        case "UpgradeSchema":
                            {
                                if (goRequest["ewCmd2"] == "Do")
                                {
                                    var argoTheSetup = this;
                                    var oUS = new ContentMigration(ref argoTheSetup);
                                    if (oUS.GetPage())
                                    {
                                        cStep = 1.ToString();
                                    }
                                    else
                                    {
                                        cStep = 2.ToString();
                                    }
                                }

                                break;
                            }
                        case "OptimiseImages":
                            {
                                if (goRequest["ewCmd2"] == "Do")
                                {
                                    var oFsh = new Protean.fsHelper(myWeb.moCtx);
                                    oFsh.mcRoot = myWeb.goServer.MapPath("/");

                                    string folderPrefix = "";
                                    if (!string.IsNullOrEmpty(goRequest["resizedonly"]))
                                    {
                                        folderPrefix = "~";
                                    }
                                    string folderPath = "/Images";
                                    if (!string.IsNullOrEmpty(goRequest["folderPath"]))
                                    {
                                        folderPath = goRequest["folderPath"];
                                    }
                                    long argnFileCount = 0L;
                                    long argnSavings = 0L;
                                    this.AddResponse(oFsh.OptimiseImages(folderPath, ref argnFileCount, ref argnSavings, false, goConfig["TinifyKey"], folderPrefix));
                                    cStep = 1.ToString();
                                }

                                break;
                            }
                        case "RunTests":
                            {
                                if (goRequest["ewCmd2"] == "Do")
                                {
                                    cPostFlushActions = "RunTests";

                                    cStep = 1.ToString();
                                }

                                break;
                            }
                        case "ImportContent":
                            {
                                if (goRequest["ewCmd2"] == "Do")
                                {
                                    var oIC = new ContentImport(this);
                                    if (oIC.GetPage())
                                    {
                                        cStep = 1.ToString();
                                    }
                                    else
                                    {
                                        cStep = 2.ToString();
                                    }
                                }

                                break;
                            }
                        case "Backup":
                            {
                                if (goRequest["ewCmd2"] == "Do")
                                {
                                    var argasetup2 = this;
                                    var oSetXfm = new SetupXforms(ref argasetup2);
                                    oPageDetail.AppendChild(oSetXfm.xFrmBackupDatabase());
                                    if (oSetXfm.valid)
                                    {
                                        cStep = 1.ToString();
                                    }
                                    else
                                    {
                                        cStep = 2.ToString();
                                    }
                                }

                                break;
                            }
                        case "Restore":
                            {
                                if (goRequest["ewCmd2"] == "Do")
                                {
                                    var argasetup3 = this;
                                    var oSetXfm = new SetupXforms(ref argasetup3);
                                    oPageDetail.AppendChild(oSetXfm.xFrmRestoreDatabase());
                                    if (oSetXfm.valid)
                                    {
                                        cStep = 1.ToString();
                                    }
                                    else
                                    {
                                        cStep = 2.ToString();
                                    }
                                }

                                break;
                            }
                    }
                }
                // moPageXml.DocumentElement.SetAttribute("layout", mcEwCmd)
                moPageXml.DocumentElement.SetAttribute("ewCmd", mcEwCmd);
                moPageXml.DocumentElement.SetAttribute("User", mnUserId.ToString());
                moPageXml.DocumentElement.AppendChild(oRoot);
                oRoot.AppendChild(oResponse);
            }
            catch (Exception ex)
            {
                stdTools.returnException(ref myWeb.msException, mcModuleName, "SetupProcessXML", ex, "", cProcessInfo, gbDebug);
            }

        }


        private void setupMenuXml()
        {

            XmlElement oElmt;

            XmlElement oElmt1;
            XmlElement oElmt2;
            XmlElement oElmt3;
            XmlElement oElmt4;
            XmlElement oElmt5;
            XmlElement oElmt6;
            XmlElement oElmt7;
            XmlElement oElmt8;
            XmlElement oElmt9;
            XmlElement oElmt10;
            XmlElement oElmt11;
            XmlElement oElmt12;
            XmlElement oElmt13;

            string sProcessInfo = string.Empty;
            try
            {
                oElmt = moPageXml.CreateElement("AdminMenu");

                oElmt1 = appendMenuItem(ref oElmt, "Setup Home", "AdmHome", icon: "fa-cogs");
                if (mbSchemaExists)
                {
                    oElmt2 = appendMenuItem(ref oElmt1, "Setup and Import", "Setup", icon: "fa-cogs");
                    oElmt3 = appendMenuItem(ref oElmt2, "Delete Database", "ClearDB", icon: "fa-exclamation-circle");
                    oElmt4 = appendMenuItem(ref oElmt2, "New Database", "NewV4", icon: "fa-briefcase");
                    oElmt6 = appendMenuItem(ref oElmt2, "Add Shipping Locations", "ShipLoc", icon: "fa-globe");
                    oElmt5 = appendMenuItem(ref oElmt2, "Import V3 Data", "ImportV3", icon: "fa-level-up");

                    oElmt6 = appendMenuItem(ref oElmt1, "Maintenance", "Maintenance", icon: "fa-wrench");
                    oElmt10 = appendMenuItem(ref oElmt6, "Run Tests", "RunTests", icon: "fa-check-square-o");
                    oElmt7 = appendMenuItem(ref oElmt6, "Update Database Schema", "UpgradeDB", icon: "fa-level-up");
                    oElmt8 = appendMenuItem(ref oElmt6, "Clean Audit Table", "CleanAudit", icon: "fa-eraser");
                    oElmt9 = appendMenuItem(ref oElmt6, "Import Content", "ImportContent", icon: "fa-arrow-circle-right");
                    oElmt10 = appendMenuItem(ref oElmt6, "Upgrade Content", "UpgradeSchema", icon: "fa-angle-double-up");
                    oElmt10 = appendMenuItem(ref oElmt6, "Optimise Images", "OptimiseImages", icon: "fa-picture-o");

                    oElmt11 = appendMenuItem(ref oElmt1, "Backup/Restore", "BackupRestore", icon: "fa-save");
                    oElmt12 = appendMenuItem(ref oElmt11, "Backup", "Backup", icon: "fa-save");
                    oElmt13 = appendMenuItem(ref oElmt11, "Restore", "Restore", icon: "fa-reply");
                }
                else
                {
                    oElmt2 = appendMenuItem(ref oElmt1, "Setup", "Setup", icon: "fa-gear");
                    oElmt4 = appendMenuItem(ref oElmt2, "New Database", "NewDatabase", icon: "fa-briefcase");
                }

                moPageXml.DocumentElement.AppendChild(oElmt);
            }

            catch (Exception ex)
            {
                AddResponseError(ex);
            }

        }

        private XmlElement appendMenuItem(ref XmlElement oRootElmt, string cName, string cCmd, long pgid = 0L, bool display = true, string icon = "")
        {

            XmlElement oElmt;
            string sProcessInfo = "";
            try
            {

                oElmt = moPageXml.CreateElement("MenuItem");
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
                if (!string.IsNullOrEmpty(icon))
                {
                    oElmt.SetAttribute("icon", icon);
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

        public bool UpdateDatabase()
        {
            //bool bRes = true;
            string filePath;

            try
            {

                var commonfolders = new ArrayList();
                var oFS = new Protean.fsHelper();
                string upgradePath = "/sqlupdate/DatabaseUpgrade.xml";
                if (goConfig["cssFramework"] == "bs5")
                {
                    upgradePath = "/update/sql/databaseupgrade.xml";
                }


                myWeb.InitializeVariables();

                foreach (string AltFolder in myWeb.maCommonFolders)
                {
                    filePath = AltFolder.TrimEnd(@"/\".ToCharArray()) + upgradePath;
                    if (!string.IsNullOrEmpty(AltFolder))
                    {
                        AddResponse("Running: " + filePath);
                        if (Conversions.ToBoolean(oFS.VirtualFileExists(filePath)))
                        {
                            UpdateDatabase(filePath);
                        }
                        else
                        {
                            AddResponse("No Common Upgrades Found");
                        }
                    }
                    else
                    {
                        AddResponse("Not Running: " + filePath);
                    }
                }
                if (Conversions.ToBoolean(oFS.VirtualFileExists(upgradePath)))
                {
                    UpdateDatabase(upgradePath);
                }
                else
                {
                    AddResponse("No Bespoke Upgrades Found");
                }
                AddResponse("Database Upgrade Complete");
            }

            catch (Exception ex)
            {
                AddResponseError(ex); // returnException(myWeb.msException, mcModuleName, "updateDatabase", ex, "", cProcessInfo, gbDebug)
                                      // AddResponse(ex.Message & " - " & ex.InnerException.ToString)
                AddResponse("Update Failed");
                return false;
            }

            return default;
        }


        public bool UpdateDatabase(string filePath)
        {
            bool bRes = true;
            try
            {
                string cCurrentVersion = string.Empty;
                // main protean db
                string oDBName = goConfig["DatabaseName"];
                string oDBServerName = goConfig["DatabaseServer"];
                string oDBUserName = goConfig["DatabaseUsername"];
                string oDBPassword = goConfig["DatabasePassword"];
                var oUpgrdXML = new XmlDocument();
                string errormsg;

                System.Collections.Specialized.NameValueCollection mConfig = null;
                if (File.Exists(goServer.MapPath(filePath)))
                {

                    oUpgrdXML.Load(goServer.MapPath(filePath));

                    string rootPath = Path.GetDirectoryName(goServer.MapPath(filePath));

                    var oAlternanteDatabase = oUpgrdXML.DocumentElement.SelectSingleNode("AlternateDatabase");
                    // check alternate database
                    if (oAlternanteDatabase != null)
                    {


                        if (oAlternanteDatabase.Attributes["configName"] != null)
                        {
                            if (!string.IsNullOrEmpty(oAlternanteDatabase.Attributes["configName"].Value))
                            {
                                mConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection(oAlternanteDatabase.Attributes["configName"].Value + "/web");
                            }
                        }
                        if (mConfig != null)
                        {
                            isAlt = true;
                            string altoDBServerName = Convert.ToString(mConfig["DatabaseServer"]);
                            string altoDBName = Convert.ToString(mConfig["DatabaseName"]);
                            string altoDBUserName = Convert.ToString(mConfig["DatabaseUsername"]);
                            string altoDBPassword = Convert.ToString(mConfig["DatabasePassword"]);

                            myWeb.moDbHelper.DatabaseServer = altoDBServerName;
                            myWeb.moDbHelper.DatabaseName = altoDBName;
                            myWeb.moDbHelper.DatabaseUser = altoDBUserName;
                            myWeb.moDbHelper.DatabasePassword = altoDBPassword;

                            cCurrentVersion = getVersionNumber();
                        }
                        else
                        {
                            if (isAlt)
                            {
                                myWeb.moDbHelper.DatabaseName = oDBName;
                                myWeb.moDbHelper.DatabaseServer = oDBServerName;
                                myWeb.moDbHelper.DatabaseUser = oDBUserName;
                                myWeb.moDbHelper.DatabasePassword = oDBPassword;
                            }
                            isAlt = false;
                            cCurrentVersion = getVersionNumber();
                        }
                    }

                    else
                    {
                        if (isAlt)
                        {
                            myWeb.moDbHelper.DatabaseName = oDBName;
                            myWeb.moDbHelper.DatabaseServer = oDBServerName;
                            myWeb.moDbHelper.DatabaseUser = oDBUserName;
                            myWeb.moDbHelper.DatabasePassword = oDBPassword;
                        }
                        isAlt = false;
                        cCurrentVersion = getVersionNumber();
                    }
                    AddResponse("--------------------------------------");
                    AddResponse("Updating Database: " + oDBName + " on " + oDBServerName);
                    AddResponse("Current Version: " + cCurrentVersion);
                    string[] oCurrentVersion = Strings.Split(cCurrentVersion, ".");
                    string cLatestVersion = oUpgrdXML.DocumentElement.GetAttribute("LatestVersion");
                    if ((cLatestVersion ?? "") == (cCurrentVersion ?? ""))
                        return true;

                    // Remove all foreign keys
                    myWeb.moDbHelper.ExeProcessSqlfromFile(rootPath + "/toV4/DropAllForeignKeys.sql");
                    myWeb.msException = "";

                    AddResponse("Running updates to version: " + cLatestVersion);

                    // will loop through until we get to version above ours
                    AddResponse("Running File: " + filePath);
                    var bRunAll = default(bool);
                    foreach (XmlElement oVer in oUpgrdXML.DocumentElement.SelectNodes("Version"))
                    {
                        // in current main version or above
                        if (Operators.CompareString(oVer.GetAttribute("Number"), oCurrentVersion[0], false) >= 0)
                        {
                            foreach (XmlElement Sub1 in oVer.SelectNodes("Sub1"))
                            {
                                // in current sub version 1 or above
                                if (Conversions.ToLong(Sub1.GetAttribute("Number")) >= Conversions.ToLong(oCurrentVersion[1]))
                                {
                                    foreach (XmlElement Sub2 in Sub1.SelectNodes("Sub2"))
                                    {
                                        if (Conversions.ToLong(Sub2.GetAttribute("Number")) >= Conversions.ToLong(oCurrentVersion[2]))
                                        {

                                            foreach (XmlElement Sub3 in Sub2.SelectNodes("Sub3"))
                                            {
                                                if (Conversions.ToLong(Sub3.GetAttribute("Number")) >= Conversions.ToLong(oCurrentVersion[3]) | bRunAll)
                                                {
                                                    // now to get actions
                                                    AddResponse("Updating to:" + oVer.GetAttribute("Number") + "." + Sub1.GetAttribute("Number") + "." + Sub2.GetAttribute("Number") + "." + Sub3.GetAttribute("Number"));
                                                    string cConn = string.Empty;
                                                    foreach (XmlElement oActionElmt in Sub3.SelectNodes("Action"))
                                                    {
                                                        try
                                                        {
                                                            // consider if reference is not there then execute on main db.
                                                            if (oActionElmt.GetAttribute("dbref") != null && !string.IsNullOrEmpty(oActionElmt.GetAttribute("dbref")))
                                                            {
                                                                myWeb.moDbHelper.DatabaseName = oDBName;
                                                                myWeb.moDbHelper.DatabaseServer = oDBServerName;
                                                                myWeb.moDbHelper.DatabaseUser = oDBUserName;
                                                                myWeb.moDbHelper.DatabasePassword = oDBPassword;
                                                            }

                                                            else
                                                            {

                                                                myWeb.moDbHelper.DatabaseName = goConfig["DatabaseName"];
                                                                myWeb.moDbHelper.DatabaseServer = goConfig["DatabaseServer"];
                                                                myWeb.moDbHelper.DatabaseUser = goConfig["DatabaseUsername"];
                                                                myWeb.moDbHelper.DatabasePassword = goConfig["DatabasePassword"];
                                                            }

                                                            switch (oActionElmt.GetAttribute("Type") ?? "")
                                                            {
                                                                case "Drop":
                                                                    {
                                                                        string argsName = oActionElmt.GetAttribute("ObjectName");
                                                                        string argsObjectType = oActionElmt.GetAttribute("ObjectType");
                                                                        ifSqlObjectExistsDropIt(ref argsName, ref argsObjectType);
                                                                        AddResponse("dropped '" + oActionElmt.GetAttribute("ObjectName") + "'");
                                                                        break;
                                                                    }
                                                                case "File":
                                                                    {
                                                                        long nCount;
                                                                        AddResponse("Run File '" + oActionElmt.GetAttribute("ObjectName") + "'");
                                                                        errormsg = "";
                                                                        nCount = myWeb.moDbHelper.ExeProcessSqlfromFile(rootPath + oActionElmt.GetAttribute("ObjectName"), ref errormsg);

                                                                        if (!string.IsNullOrEmpty(errormsg))
                                                                        {
                                                                            AddResponse("<strong style=\"color:#ff0000\">WARNING: File execution generated an error</strong>");
                                                                            AddResponse("<p style=\"color:#ff0000\">" + errormsg + "</p>");
                                                                        }
                                                                        else if (nCount == -1)
                                                                        {
                                                                            AddResponse("File execution Completed...");
                                                                        }
                                                                        else
                                                                        {
                                                                            AddResponse("(" + nCount + ") Updates..");
                                                                        }

                                                                        break;
                                                                    }

                                                                default:
                                                                    {
                                                                        break;
                                                                    }
                                                                    // dont do anything
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            bRes = false;
                                                            AddResponse("Error:" + ex.ToString());
                                                            AddResponseError(ex); // returnException(myWeb.msException, mcModuleName, "updateDatabase", ex, "", cProcessInfo, gbDebug)
                                                        }
                                                        // If Not msException = "" Then
                                                        // AddResponse(msException)
                                                        // msException = ""
                                                        // End If
                                                    }
                                                    oCurrentVersion[3] = Sub3.GetAttribute("Number");
                                                }
                                                else
                                                {
                                                    // AddResponse("Error: Not yet at this version number")
                                                }
                                            }
                                            if (Conversions.ToLong(Sub2.GetAttribute("Number")) == Conversions.ToLong(oCurrentVersion[2]))
                                            {
                                                oCurrentVersion[3] = 0.ToString();
                                                oCurrentVersion[2] = (Conversions.ToDouble(oCurrentVersion[2]) + 1d).ToString();
                                            }
                                        }
                                    }
                                    if (Conversions.ToLong(Sub1.GetAttribute("Number")) == Conversions.ToLong(oCurrentVersion[1]))
                                    {
                                        oCurrentVersion[2] = 0.ToString();
                                        oCurrentVersion[1] = (Conversions.ToDouble(oCurrentVersion[1]) + 1d).ToString();
                                    }
                                }
                            }
                            oCurrentVersion[3] = (-1).ToString();
                            oCurrentVersion[2] = 0.ToString();
                            oCurrentVersion[1] = 0.ToString();
                        }
                    }
                    if (oUpgrdXML.DocumentElement.GetAttribute("update") != "false")
                    {
                        if (mConfig != null)
                        {
                            oDBServerName = Convert.ToString(mConfig["DatabaseServer"]);
                            oDBName = Convert.ToString(mConfig["DatabaseName"]);
                            oDBUserName = Convert.ToString(mConfig["DatabaseUsername"]);
                            oDBPassword = Convert.ToString(mConfig["DatabasePassword"]);

                            myWeb.moDbHelper.DatabaseName = oDBName;
                            myWeb.moDbHelper.DatabaseServer = oDBServerName;
                            myWeb.moDbHelper.DatabaseUser = oDBUserName;
                            myWeb.moDbHelper.DatabasePassword = oDBPassword;

                            saveVersionNumber(cLatestVersion);
                        }
                        else
                        {
                            saveVersionNumber(cLatestVersion);
                        }

                        mnCurrentVersion = cLatestVersion;
                        cLatestVersion = cLatestVersion + " - Updated DB version";
                    }
                    else
                    {
                        cLatestVersion = cLatestVersion + " - Not Updated DB version";
                    }

                    AddResponse("Update Completed to Version " + cLatestVersion);
                    return bRes;
                }
                else
                {
                    AddResponse("File Not Found: " + filePath);
                    return false;
                }
            }

            catch (Exception ex)
            {
                AddResponseError(ex); // returnException(myWeb.msException, mcModuleName, "updateDatabase", ex, "", cProcessInfo, gbDebug)
                AddResponse(ex.Message + " - " + ex.InnerException.ToString());
                AddResponse("Update Failed");
                return false;
            }
        }

        public bool buildDatabase(bool NewBuild = false)
        {
            //string cProcessInfo = "migrateData";

            bool bResult = false;
            try
            {

                // clear any existing tables
                Migrate_Rollback();

                goResponse.Buffer = false;
                goResponse.Flush();
                // Run The Script
                // ############################

                object dbUpdatePath = "/ewcommon/sqlupdate";
                if (goConfig["cssFramework"] == "bs5")
                {
                    dbUpdatePath = "/ptn/update/sql";
                }


                myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath(Conversions.ToString(Operators.ConcatenateObject(dbUpdatePath, "/toV4/Structure.sql"))));
                AddResponse(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("Run File ", dbUpdatePath), "/toV4/Structure.SQL'")));
                myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath(Conversions.ToString(Operators.ConcatenateObject(dbUpdatePath, "/toV4/fxn_SearchXML.sql"))));
                AddResponse(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("Run File ", dbUpdatePath), "/toV4/fxn_SearchXML.sql")));
                myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath(Conversions.ToString(Operators.ConcatenateObject(dbUpdatePath, "/toV4/fxn_addAudit.sql"))));
                AddResponse(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("Run File ", dbUpdatePath), "/toV4/fxn_addAudit.SQL'")));
                myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath(Conversions.ToString(Operators.ConcatenateObject(dbUpdatePath, "/toV4/fxn_getStatus.sql"))));
                AddResponse(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("Run File ", dbUpdatePath), "/toV4/fxn_getStatus.SQL'")));
                myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath(Conversions.ToString(Operators.ConcatenateObject(dbUpdatePath, "/toV4/fxn_checkPermission.sql"))));
                AddResponse(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("Run File ", dbUpdatePath), "/toV4/fxn_checkPermission.SQL'")));
                myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath(Conversions.ToString(Operators.ConcatenateObject(dbUpdatePath, "/toV4/fxn_getUserCompanies.sql"))));
                AddResponse(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("Run File ", dbUpdatePath), "/toV4/fxn_getUserCompanies.SQL'")));
                myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath(Conversions.ToString(Operators.ConcatenateObject(dbUpdatePath, "/toV4/fxn_shippingTotal.sql"))));
                AddResponse(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("Run File ", dbUpdatePath), "/toV4/fxn_shippingTotal.SQL'")));
                myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath(Conversions.ToString(Operators.ConcatenateObject(dbUpdatePath, "/toV4/fxn_getUserDepts.sql"))));
                AddResponse(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("Run File ", dbUpdatePath), "/toV4/fxn_getUserDepts.SQL'")));
                myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath(Conversions.ToString(Operators.ConcatenateObject(dbUpdatePath, "/toV4/fxn_getUserRoles.sql"))));
                AddResponse(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("Run File ", dbUpdatePath), "/toV4/fxn_getUserRoles.SQL'")));
                myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath(Conversions.ToString(Operators.ConcatenateObject(dbUpdatePath, "/toV4/spGetAllUsers.sql"))));
                AddResponse(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("Run File ", dbUpdatePath), "/toV4/spGetAllUsers.SQL'")));
                myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath(Conversions.ToString(Operators.ConcatenateObject(dbUpdatePath, "/toV4/spGetUsers.sql"))));
                AddResponse(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("Run File ", dbUpdatePath), "/toV4/spGetUsers.SQL'")));
                myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath(Conversions.ToString(Operators.ConcatenateObject(dbUpdatePath, "/toV4/getContentStructure.sql"))));
                AddResponse(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("Run File ", dbUpdatePath), "/toV4/getContentStructure.SQL'")));
                myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath(Conversions.ToString(Operators.ConcatenateObject(dbUpdatePath, "/toV4/spGetDirectoryItems.sql"))));
                AddResponse(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("Run File ", dbUpdatePath), "/toV4/spGetDirectoryItems.SQL'")));
                myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath(Conversions.ToString(Operators.ConcatenateObject(dbUpdatePath, "/toV4/getUsersCompanyAllParents.sql"))));
                AddResponse(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("Run File ", dbUpdatePath), "/toV4/getUsersCompanyAllParents.SQL'")));
                myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath(Conversions.ToString(Operators.ConcatenateObject(dbUpdatePath, "/toV4/spGetCompanyUsers.sql"))));
                AddResponse(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("Run File ", dbUpdatePath), "/toV4/spGetCompanyUsers.SQL'")));
                myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath(Conversions.ToString(Operators.ConcatenateObject(dbUpdatePath, "/toV4/spGetAllUsersActive.sql"))));
                AddResponse(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("Run File ", dbUpdatePath), "/toV4/spGetAllUsersActive.SQL'")));
                myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath(Conversions.ToString(Operators.ConcatenateObject(dbUpdatePath, "/toV4/spGetAllUsersInActive.sql"))));
                AddResponse(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("Run File ", dbUpdatePath), "/toV4/spGetAllUsersInActive.SQL'")));
                myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath(Conversions.ToString(Operators.ConcatenateObject(dbUpdatePath, "/toV4/spGetCompanyUsersActive.sql"))));
                AddResponse(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("Run File ", dbUpdatePath), "/toV4/spGetCompanyUsersActive.SQL'")));
                myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath(Conversions.ToString(Operators.ConcatenateObject(dbUpdatePath, "/toV4/spGetCompanyUsersInActive.sql"))));
                AddResponse(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("Run File ", dbUpdatePath), "/toV4/spGetCompanyUsersInActive.SQL'")));
                myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath(Conversions.ToString(Operators.ConcatenateObject(dbUpdatePath, "/toV4/spSearchUsers.sql"))));
                AddResponse(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("Run File ", dbUpdatePath), "/toV4/spSearchUsers.SQL'")));
                myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath(Conversions.ToString(Operators.ConcatenateObject(dbUpdatePath, "/toV4/4.0.1.40/tblOptOutAddresses.sql"))));
                AddResponse(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("Run File ", dbUpdatePath), "/toV4/4.0.1.40/tblOptOutAddresses.sql'")));
                myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath(Conversions.ToString(Operators.ConcatenateObject(dbUpdatePath, "/toV4/4.0.1.45/fxn_getContentParents.sql"))));
                AddResponse(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("Run File ", dbUpdatePath), "/toV4/4.0.1.45/fxn_getContentParents.sql'")));

                AddResponse("Completed Initial Build");
                saveVersionNumber();

                // Make some strings to carry the Migration specific errors
                if (NewBuild)
                {
                    // new build - just need the admin jobbie setup
                    bResult = true;
                    bResult = prepareDirectory();
                }
                else
                {
                    // migration from v2.6/v3.0
                    // these are the default items to migrate
                    bResult = Migrate_Directory();
                    if (bResult)
                        bResult = Migrate_Content();
                    if (bResult)
                        bResult = Migrate_Shipping();

                    // If bResult Then bResult = Migrate_Cart()

                    // will need to check the rest like cart and membership
                    // they would need to be checked if they are active
                }

                UpdateDatabase();

                AddResponseComplete("Congratulations - Go to your new Website", "/");

                return saveVersionNumber();
            }

            catch (Exception ex)
            {
                AddResponseError(ex); // returnException(myWeb.msException, mcModuleName, "InitializeVariables", ex, "", cProcessInfo, gbDebug)
                return false;
            }

        }

        public bool prepareDirectory()
        {
            long nRoleId;
            //string cProcessInfo = "prepareDirectory";
            try
            {

                string AdminPassword = goConfig["DatabasePassword"];
                if (string.IsNullOrEmpty(AdminPassword))
                    AdminPassword = "proteanpass";
                string UserEmail = "support@proteancms.com";
                myWeb.Open();

                mnUserId = Convert.ToInt32(myWeb.moDbHelper.insertDirectory("AdminV4", "User", "Admin", AdminPassword, "<User><FirstName>Website</FirstName><MiddleName/><LastName>Administrator</LastName><Position/><Email>" + UserEmail + "</Email><Notes/></User>"));

                // create system roles

                nRoleId = myWeb.moDbHelper.insertDirectory("Administrator", "Role", "Administrator", "", "<Role><Name>Administrator</Name><Notes/></Role>");
                myWeb.moDbHelper.maintainDirectoryRelation(nRoleId, mnUserId);
                nRoleId = myWeb.moDbHelper.insertDirectory("DefaultUser", "Role", "Default User", "", "<Role><Name>Administrator</Name><Notes/></Role>");
                myWeb.moDbHelper.maintainDirectoryRelation(nRoleId, mnUserId);

                string defaultPageXml = "<DisplayName title=\"\" linkType=\"internal\" exclude=\"false\" noindex=\"false\"/><Images><img class=\"icon\" /><img class=\"thumbnail\" /><img class=\"detail\" /></Images><Description/>";

                gnTopLevel = Convert.ToInt32(myWeb.moDbHelper.insertStructure(0, "", "Home", defaultPageXml, "Modules_1_column"));
                myWeb.moDbHelper.insertStructure(gnTopLevel, "", "About Us", defaultPageXml, "Modules_1_column");
                myWeb.moDbHelper.insertStructure(gnTopLevel, "", "Products", defaultPageXml, "Modules_1_column");
                myWeb.moDbHelper.insertStructure(gnTopLevel, "", "Services", defaultPageXml, "Modules_1_column");

                long infoId = myWeb.moDbHelper.insertStructure(gnTopLevel, "", "Info Menu", defaultPageXml, "Modules_1_column");
                myWeb.moDbHelper.insertStructure(infoId, "", "Contact Us", defaultPageXml, "Modules_1_column");

                return true;
            }
            catch (Exception ex)
            {
                AddResponseError(ex); // returnException(myWeb.msException, mcModuleName, "InitializeVariables", ex, "", cProcessInfo, gbDebug)
                return false;
            }

        }

        public bool saveVersionNumber(string cVersionNumber = "", string cConn = "")
        {
            try
            {
                if (string.IsNullOrEmpty(cVersionNumber))
                    cVersionNumber = mnCurrentVersion;
                object dbUpdatePath = "/ewcommon/sqlupdate";
                if (goConfig["cssFramework"] == "bs5")
                {
                    dbUpdatePath = "/ptn/update/sql";
                }

                // create the version table if not exists
                string sFilePath = Conversions.ToString(Operators.ConcatenateObject(dbUpdatePath, "/toV4/4.1.1.35/tblSchemaVersion.sql"));
                if (!myWeb.moDbHelper.checkDBObjectExists("tblSchemaVersion", Tools.Database.objectTypes.Table, cConn))
                {
                    myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath(sFilePath));
                }

                string[] aVersionNumber = Strings.Split(cVersionNumber, ".");
                string sSql;
                DataSet oDs;
                DataRow oDr;
                sSql = "select * from tblSchemaVersion where nVersionKey = 1";
                if (!string.IsNullOrEmpty(cConn))
                {
                    oDs = myWeb.moDbHelper.getDataSetForUpdate(sSql, "VersionNo", "", cConn);
                }
                else
                {
                    oDs = myWeb.moDbHelper.getDataSetForUpdate(sSql, "VersionNo");
                }


                if (oDs.Tables["VersionNo"].Rows.Count > 0)
                {
                    oDr = oDs.Tables["VersionNo"].Rows[0];
                    oDr.BeginEdit();
                    oDr["MajorVersion"] = Conversions.ToInteger(aVersionNumber[0]);
                    oDr["MinorVersion"] = Conversions.ToInteger(aVersionNumber[1]);
                    oDr["Release"] = Conversions.ToInteger(aVersionNumber[2]);
                    oDr["Build"] = Conversions.ToInteger(aVersionNumber[3]);
                    oDr.EndEdit();
                }
                else
                {
                    oDr = oDs.Tables["VersionNo"].NewRow();
                    oDr["MajorVersion"] = Conversions.ToInteger(aVersionNumber[0]);
                    oDr["MinorVersion"] = Conversions.ToInteger(aVersionNumber[1]);
                    oDr["Release"] = Conversions.ToInteger(aVersionNumber[2]);
                    oDr["Build"] = Conversions.ToInteger(aVersionNumber[3]);
                    oDs.Tables["VersionNo"].Rows.Add(oDr);
                }

                myWeb.moDbHelper.updateDataset(ref oDs, "VersionNo", false);

                // oDs.Dispose()
                oDs = null;

                // Dim oImp As Protean.Tools.Security.Impersonate = New Protean.Tools.Security.Impersonate()
                // If oImp.ImpersonateValidUser(goConfig("AdminAcct"), goConfig("AdminDomain"), goConfig("AdminPassword"), , goConfig("AdminGroup")) Then
                // IO.File.SetAttributes(goServer.MapPath(goConfig("ProjectPath") & "/Web.config"), IO.FileAttributes.Normal)
                // Dim oCfg As Configuration = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/")
                // Dim oCgfSect As System.Configuration.DefaultSection = oCfg.GetSection("eonic/web")
                // Dim oConfigXml As XmlDocument = New XmlDocument
                // oConfigXml.LoadXml(oCgfSect.SectionInformation.GetRawXml)

                // oConfigXml.SelectSingleNode("web/add[@key='VersionNumber']/@value").InnerText = cVersionNumber

                // oCgfSect.SectionInformation.RestartOnExternalChanges = False
                // oCgfSect.SectionInformation.SetRawXml(oConfigXml.OuterXml)
                // oCfg.Save()
                // AddResponse("You have been updated to Version:" & cVersionNumber & "")
                // oImp.UndoImpersonation()
                // Return True
                // Else
                // AddResponse("Failed to update version number - Auth Failed")
                // Return False
                // End If

                return true;
            }

            catch (Exception ex)
            {
                AddResponseError(ex); // returnException(myWeb.msException, mcModuleName, "updateDatabase", ex, "", cProcessInfo, gbDebug)
                AddResponse("Failed to update version number - Error Condition");
                return false;
            }
        }

        public string getVersionNumber(string cConn = "")
        {
            try
            {
                string sVersionNumber = goConfig["VersionNumber"];
                string sSql;
                DataSet oDs;


                if (myWeb.moDbHelper.checkDBObjectExists("tblSchemaVersion", Tools.Database.objectTypes.Table, cConn))
                {
                    sSql = "select * from tblSchemaVersion where nVersionKey = 1";
                    if (!string.IsNullOrEmpty(cConn))
                    {
                        oDs = myWeb.moDbHelper.GetDataSet(sSql, "VersionNo", "", false, default, CommandType.Text, 0, 0, cConn);
                    }
                    else
                    {
                        oDs = myWeb.moDbHelper.GetDataSet(sSql, "VersionNo");
                    }

                    foreach (DataRow oDr in oDs.Tables["VersionNo"].Rows)
                        sVersionNumber = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(oDr["MajorVersion"], "."), oDr["MinorVersion"]), "."), oDr["Release"]), "."), oDr["Build"]), "."));
                    oDs.Dispose();
                    oDs = null;
                }

                return sVersionNumber;
            }

            catch (Exception ex)
            {
                AddResponseError(ex); // returnException(myWeb.msException, mcModuleName, "updateDatabase", ex, "", cProcessInfo, gbDebug)
                AddResponse("Failed to update version number - Error Condition");
                return Conversions.ToString(false);
            }
        }


        #endregion

        #region V2.6/V3 to V4 Migration -BJR November 2006-

        public bool Migrate_Directory()
        {

            string sSqlStr;
            DataSet oDSDIR;
            string strSchema;
            string strforiegnRef;
            string strName;
            string strPassword;
            string strXML;

            var oDXML = new XmlDocument();
            long nDirId;

            bool bUsesDirectory;
            string cSQLP1;
            string cSQLP2;

            try
            {
                string strEwuDirectory = "tbl_ewu_Directory";
                bUsesDirectory = moDbHelper.doesTableExist(ref strEwuDirectory);
                if (!bUsesDirectory)
                {
                    AddResponse(" No Directory information to import. Creating deafults.");
                    int nRolex;
                    int nUserx;
                    nUserx = Convert.ToInt32(myWeb.moDbHelper.insertDirectory("AdminV4", "User", "Admin", "buster", "<User><FirstName/><MiddleName/><LastName/><Position/><Email/><Notes/></User>"));
                    // create system roles
                    nRolex = Convert.ToInt32(myWeb.moDbHelper.insertDirectory("Administrator", "Role", "Administrator", "", "<Role><Name>Administrator</Name><Notes/></Role>"));
                    myWeb.moDbHelper.maintainDirectoryRelation(nRolex, nUserx);
                    nRolex = Convert.ToInt32(myWeb.moDbHelper.insertDirectory("DefaultUser", "Role", "Default User", "", "<Role><Name>Administrator</Name><Notes/></Role>"));
                    myWeb.moDbHelper.maintainDirectoryRelation(nRolex, nUserx);
                    return true;
                    //return default;
                }


                AddResponse("Creating System Roles");
                long nRoleId;
                // create system roles
                mnUserId = Convert.ToInt32(myWeb.moDbHelper.insertDirectory("AdminV4", "User", "Admin", "buster", "<User><FirstName/><MiddleName/><LastName/><Position/><Email/><Notes/></User>"));
                nRoleId = myWeb.moDbHelper.insertDirectory("Administrator", "Role", "Administrator", "", "<Role><Name>Administrator</Name><Notes/></Role>");
                myWeb.moDbHelper.maintainDirectoryRelation(nRoleId, mnUserId);
                nRoleId = myWeb.moDbHelper.insertDirectory("DefaultUser", "Role", "Default User", "", "<Role><Name>Administrator</Name><Notes/></Role>");
                myWeb.moDbHelper.maintainDirectoryRelation(nRoleId, mnUserId);


                // Directory Table
                AddResponse("Migrating Directory Table");
                sSqlStr = "Select * From tbl_ewu_Directory";
                oDSDIR = myWeb.moDbHelper.GetDataSet(sSqlStr, "Directory", "dsDIR");

                myWeb.moDbHelper.addTableToDataSet(ref oDSDIR, "SELECT * FROM tbl_ewc_Contact where nContactParentType = 2", "Contacts");
                oDSDIR.Relations.Add("CartContacts", oDSDIR.Tables["Directory"].Columns["nDirId"], oDSDIR.Tables["Contacts"].Columns["nContactParentId"], false);
                oDSDIR.Relations["CartContacts"].Nested = true;

                foreach (DataTable oDT in oDSDIR.Tables)
                {
                    foreach (DataColumn oDC in oDT.Columns)
                        oDC.ColumnMapping = MappingType.Attribute;
                }

                oDXML.InnerXml = Strings.Replace(oDSDIR.GetXml(), "'", "''");

                // ok, now we need to loop through the directory, 
                // then add the contacts for the users
                // having lots of checking so just in case there is a column missing

                foreach (XmlElement oDirElmt in oDXML.SelectNodes("descendant-or-self::Directory"))
                {

                    // do the user record
                    if ((oDirElmt.GetAttribute("nDirType") ?? "") == (1.ToString() ?? "")) // Group Schema
                    {
                        strSchema = "Group";
                        strXML = "<Group><Name>" + oDirElmt.GetAttribute("cDirDN") + "</Name></Group>";
                    }
                    else
                    {
                        strSchema = "User"; // User schema
                        strXML = "<User><FirstName>" + oDirElmt.GetAttribute("cFirstName") + "</FirstName><LastName>" + oDirElmt.GetAttribute("cLastName") + "</LastName><Position/><Email>" + oDirElmt.GetAttribute("cDirEmail") + "</Email><Notes/></User>";
                    }
                    // other details
                    strforiegnRef = Convert.ToString(Convert.ToBoolean(oDirElmt.GetAttribute("nDirId"))? "": oDirElmt.GetAttribute("nDirId"));
                    strName = Convert.ToString(Convert.ToBoolean(oDirElmt.GetAttribute("cDirDN"))? "": oDirElmt.GetAttribute("cDirDN"));
                    strPassword = Convert.ToString(Convert.ToBoolean(oDirElmt.GetAttribute("cDirPassword"))? "": oDirElmt.GetAttribute("cDirPassword"));
                    nDirId = myWeb.moDbHelper.insertDirectory(strforiegnRef, strSchema, strName, strPassword, strXML);

                    foreach (XmlElement oConElmt in oDirElmt.SelectNodes("Contacts"))
                    {
                        AddResponse("Migrating Contact " + oConElmt.GetAttribute("nContactKey") + "");
                        // Firstly the Cart Order
                        cSQLP2 = "";
                        cSQLP1 = "INSERT INTO tblCartContact (nContactDirId, nContactCartId, cContactType, cContactName, " + "cContactCompany, cContactAddress, cContactCity, cContactState, cContactZip, cContactCountry, " + "cContactTel, cContactFax, cContactEmail, cContactXml, nAuditId) VALUES (";

                        cSQLP2 += nDirId + ",";
                        cSQLP2 += "0,";
                        if (string.IsNullOrEmpty(oConElmt.GetAttribute("cContactType")))
                            cSQLP2 += "Null,";
                        else
                            cSQLP2 += "'" + oConElmt.GetAttribute("cContactType") + "',";
                        if (string.IsNullOrEmpty(oConElmt.GetAttribute("cContactName")))
                            cSQLP2 += "Null,";
                        else
                            cSQLP2 += "'" + Strings.Replace(oConElmt.GetAttribute("cContactName"), "'", "''") + "',";
                        if (string.IsNullOrEmpty(oConElmt.GetAttribute("cContactCompany")))
                            cSQLP2 += "Null,";
                        else
                            cSQLP2 += "'" + Strings.Replace(oConElmt.GetAttribute("cContactCompany"), "'", "''") + "',";
                        if (string.IsNullOrEmpty(oConElmt.GetAttribute("cContactAddress")))
                            cSQLP2 += "Null,";
                        else
                            cSQLP2 += "'" + Strings.Replace(oConElmt.GetAttribute("cContactAddress"), "'", "''") + "',";
                        if (string.IsNullOrEmpty(oConElmt.GetAttribute("cContactCity")))
                            cSQLP2 += "Null,";
                        else
                            cSQLP2 += "'" + oConElmt.GetAttribute("cContactCity") + "',";
                        if (string.IsNullOrEmpty(oConElmt.GetAttribute("cContactState")))
                            cSQLP2 += "Null,";
                        else
                            cSQLP2 += "'" + oConElmt.GetAttribute("cContactState") + "',";
                        if (string.IsNullOrEmpty(oConElmt.GetAttribute("cContactZip")))
                            cSQLP2 += "Null,";
                        else
                            cSQLP2 += "'" + oConElmt.GetAttribute("cContactZip") + "',";
                        if (string.IsNullOrEmpty(oConElmt.GetAttribute("cContactCountry")))
                            cSQLP2 += "Null,";
                        else
                            cSQLP2 += "'" + oConElmt.GetAttribute("cContactCountry") + "',";
                        if (string.IsNullOrEmpty(oConElmt.GetAttribute("cContactTel")))
                            cSQLP2 += "Null,";
                        else
                            cSQLP2 += "'" + oConElmt.GetAttribute("cContactTel") + "',";
                        if (string.IsNullOrEmpty(oConElmt.GetAttribute("cContactFax")))
                            cSQLP2 += "Null,";
                        else
                            cSQLP2 += "'" + oConElmt.GetAttribute("cContactFax") + "',";
                        if (string.IsNullOrEmpty(oConElmt.GetAttribute("cContactEmail")))
                            cSQLP2 += "Null,";
                        else
                            cSQLP2 += "'" + oConElmt.GetAttribute("cContactEmail") + "',";
                        cSQLP2 += "Null,";
                        cSQLP2 += myWeb.moDbHelper.getAuditId() + ")";
                        myWeb.moDbHelper.GetIdInsertSql(cSQLP1 + cSQLP2);
                    }

                    // now we need to loop back through and and sort the relations
                    if (oDirElmt.GetAttribute("cDirMemberOfIdArr") != Convert.ToString(DBNull.Value))
                    {
                        oDirElmt.SetAttribute("cDirMemberOfIdArr", "");
                    }
                        
                    if (!string.IsNullOrEmpty(oDirElmt.GetAttribute("cDirMemberOfIdArr")))
                    {
                        string[] myArr = Strings.Split(oDirElmt.GetAttribute("cDirMemberOfIdArr"), ",");
                        int i;
                        var loopTo = Information.UBound(myArr);
                        for (i = 0; i <= loopTo; i++)
                        {
                            int myID = myWeb.moDbHelper.FindDirectoryByForiegn(Strings.Trim(myArr[i]));
                            if (myID > 0)
                                myWeb.moDbHelper.maintainDirectoryRelation(myID, nDirId);
                        }
                    }

                }


                // Done, let the world know!


                return true;
            }
            catch (Exception ex)
            {
                AddResponseError(ex); // returnException(myWeb.msException, mcModuleName, "Migrate_Directory", ex, "", "", gbDebug)


                return false;
            }
        }


        public bool Migrate_Content()
        {

            var oDS = new DataSet();
            string sContentBrief;
            string sContentDetail;
            XmlDocument oDataXML;
            XmlElement oMenuElmt;
            XmlElement oContElmt;
            bool bUsesDirectory;
            int nParID = 0;
            long nContentId;
            string sSql;

            try
            {
                string strewuDirectory = "tbl_ewu_Directory";
                bUsesDirectory = moDbHelper.doesTableExist(ref strewuDirectory);

                oDS = myWeb.moDbHelper.GetDataSet("Select * from tbl_ewm_structure", "Menu", "Structure");
                // checknest(oDS, dbT)
                myWeb.moDbHelper.addTableToDataSet(ref oDS, "SELECT tbl_ewm_content.*, tbl_ewm_contentType.cContentTypeName AS cContentTypeName FROM tbl_ewm_content INNER JOIN tbl_ewm_contentType ON tbl_ewm_content.nContentType = tbl_ewm_contentType.nContentTypeKey", "Content");


                oDS.Relations.Add("Rel0", oDS.Tables["Menu"].Columns["nID"], oDS.Tables["Menu"].Columns["nParentID"], false);



                oDS.Relations["Rel0"].Nested = true;
                oDS.Relations.Add("Rel1", oDS.Tables["Menu"].Columns["nID"], oDS.Tables["Content"].Columns["nContentParID"], false);
                oDS.Relations["Rel1"].Nested = true;

                myWeb.moDbHelper.addTableToDataSet(ref oDS, "Select * from tbl_ewm_contentLocation", "Location");


                if (bUsesDirectory)
                {
                    myWeb.moDbHelper.addTableToDataSet(ref oDS, "Select * from tbl_ewu_permissions", "Permissions");
                    oDS.Relations.Add("Rel2", oDS.Tables["Menu"].Columns["nID"], oDS.Tables["Permissions"].Columns["nPermKey"], false);
                    myWeb.moDbHelper.addTableToDataSet(ref oDS, "Select * From tblDirectory", "Directory");
                }

                oDS.EnforceConstraints = false;

                foreach (DataTable oDT in oDS.Tables)
                {
                    foreach (DataColumn oDC in oDT.Columns)
                        oDC.ColumnMapping = MappingType.Attribute;
                }

                oDataXML = new XmlDocument();
                oDataXML.InnerXml = oDS.GetXml();
                var nMenStatus = default(int);
                var nContStatus = default(int);
                foreach (XmlElement currentOMenuElmt in oDataXML.SelectNodes("descendant-or-self::Menu"))
                {
                    oMenuElmt = currentOMenuElmt;
                    if (oMenuElmt.ParentNode.SelectSingleNode("@NewID") != null)
                    {
                        nParID = Conversions.ToInteger(oMenuElmt.ParentNode.SelectSingleNode("@NewID").InnerText);
                    }
                    switch (oMenuElmt.GetAttribute("nStatus") ?? "")
                    {
                        case var @case when @case == "":
                            {
                                nMenStatus = 1;
                                break;
                            }
                        case "1":
                            {
                                nMenStatus = 1;
                                break;
                            }
                        case "2":
                            {
                                nMenStatus = 0;
                                break;
                            }
                        case "3":
                            {
                                nMenStatus = 0;
                                break;
                            }
                    }
                    nParID = Convert.ToInt32(myWeb.moDbHelper.insertStructure(nParID, oMenuElmt.GetAttribute("nId"), CleanName(oMenuElmt.GetAttribute("cName")), "<DisplayName>" + oMenuElmt.GetAttribute("cName") + "</DisplayName><Description />", oMenuElmt.GetAttribute("cTemplateName"), nMenStatus, Convert.ToDateTime(Interaction.IIf(string.IsNullOrEmpty(oMenuElmt.GetAttribute("dPublishDate")), null, oMenuElmt.GetAttribute("dPublishDate"))),Convert.ToDateTime(Interaction.IIf(string.IsNullOrEmpty(oMenuElmt.GetAttribute("dExpireDate")), null, oMenuElmt.GetAttribute("dExpireDate"))), "", Convert.ToInt64(Interaction.IIf(string.IsNullOrEmpty(oMenuElmt.GetAttribute("nDisplayOrder")), 0, oMenuElmt.GetAttribute("nDisplayOrder")))));
                    oMenuElmt.SetAttribute("NewID", nParID.ToString());
                    AddResponse("   Writing Page:" + oMenuElmt.GetAttribute("cName") + "   ");

                    foreach (XmlElement currentOContElmt in oMenuElmt.SelectNodes("Content"))
                    {
                        oContElmt = currentOContElmt;
                        switch (oContElmt.GetAttribute("nContentStatus") ?? "")
                        {
                            case var case1 when case1 == "":
                                {
                                    nContStatus = 1;
                                    break;
                                }
                            case "1":
                                {
                                    nContStatus = 1;
                                    break;
                                }
                            case "2":
                                {
                                    nContStatus = 0;
                                    break;
                                }
                            case "3":
                                {
                                    nContStatus = 0;
                                    break;
                                }
                        }
                        sContentBrief = upgradeContentSchemas(Strings.Replace(oContElmt.GetAttribute("cContentTypeName"), " ", ""), oContElmt.GetAttribute("cContentXML"), "brief");

                        sContentDetail = upgradeContentSchemas(Strings.Replace(oContElmt.GetAttribute("cContentTypeName"), " ", ""), oContElmt.GetAttribute("cContentXML"), "detail");

                        nContentId = myWeb.moDbHelper.insertContent(oContElmt.GetAttribute("nContentKey"), CleanName(oContElmt.GetAttribute("cContentPlaceName"), true), Strings.Replace(oContElmt.GetAttribute("cContentTypeName"), " ", ""), sContentBrief, sContentDetail, nParID, Interaction.IIf(string.IsNullOrEmpty(oContElmt.GetAttribute("dPublishDate")), null, oContElmt.GetAttribute("dPublishDate")), Interaction.IIf(string.IsNullOrEmpty(oContElmt.GetAttribute("dExpireDate")), null, oContElmt.GetAttribute("dExpireDate")), nContStatus);

                        bool bCascade = false;
                        if (oContElmt.GetAttribute("nContentItterateDown") == "1")
                        {
                            bCascade = true;
                        }

                        myWeb.moDbHelper.setContentLocation(nParID, nContentId, true, bCascade);

                        oContElmt.SetAttribute("NewID", nContentId.ToString());
                    }
                    if (bUsesDirectory) // Permissions
                    {
                        foreach (XmlElement oPermElmt in oMenuElmt.SelectNodes("Permissions"))
                        {
                            long nUser = 0L;
                            nUser = myWeb.moDbHelper.getObjectByRef(Cms.dbHelper.objectTypes.Directory, oPermElmt.GetAttribute("nPermDirId"));
                            myWeb.moDbHelper.maintainPermission(nParID, nUser, oPermElmt.GetAttribute("nPermLevel"));
                        }
                    }
                }
                AddResponse("Rename Subpage Images:");

                // step through all content images
                foreach (XmlElement currentOContElmt1 in oDataXML.SelectNodes("descendant::Content[@cContentTypeName='Image']"))
                {
                    oContElmt = currentOContElmt1;
                    // if the image has a menu sibling of the same name.
                    if (oContElmt.ParentNode.SelectSingleNode("Menu[@cName='" + Strings.Replace(oContElmt.GetAttribute("cContentPlaceName"), "'", "") + "']") != null)
                    {
                        oMenuElmt = (XmlElement)oContElmt.ParentNode.SelectSingleNode("Menu[@cName='" + Strings.Replace(oContElmt.GetAttribute("cContentPlaceName"), "'", "") + "']");
                        sSql = "update tblContent set cContentName = 'page_" + oMenuElmt.GetAttribute("NewID") + "_tn' where nContentKey=" + oContElmt.GetAttribute("NewID");
                        AddResponse("<p>Renaming Image: '" + oContElmt.GetAttribute("cContentPlaceName") + "' - 'page_" + oMenuElmt.GetAttribute("NewID") + "_tn'</p>");
                        myWeb.moDbHelper.ExeProcessSql(sSql);
                    }
                }


                AddResponse("Migrating Locations:");
                foreach (XmlElement oLocElmt in oDataXML.SelectNodes("descendant-or-self::Location"))
                {
                    // nParID, nContentId
                    nParID = Convert.ToInt32(myWeb.moDbHelper.getObjectByRef(Cms.dbHelper.objectTypes.ContentStructure, oLocElmt.GetAttribute("nStructureID")));
                    nContentId = myWeb.moDbHelper.getObjectByRef(Cms.dbHelper.objectTypes.Content, oLocElmt.GetAttribute("nContentID"));
                    myWeb.moDbHelper.setContentLocation(nParID, nContentId, false);
                }



                return true;
            }
            catch (Exception ex)
            {
                AddResponseError(ex); // returnException(myWeb.msException, mcModuleName, "Migrate_Content", ex, "", "", gbDebug)


                return false;
            }
        }

        public bool Migrate_Shipping()
        {
            Protean.Cms.dbHelper dbh = myWeb.moDbHelper;
            DataSet oDS;
            var oDXML = new XmlDocument();
            int nParID;
            string cSQL;
            try
            {

                AddResponse("Migrating Shipping");
                // ifSqlObjectExistsDropIt(tbl_ewc_shippingLocations, "table")
                if (!myWeb.moDbHelper.TableExists("tbl_ewc_shippingLocations"))
                    return true;
                oDS = myWeb.moDbHelper.GetDataSet("SELECT * FROM tbl_ewc_shippingLocations", "Locations", "Shipping");
                myWeb.moDbHelper.addTableToDataSet(ref oDS, "SELECT * FROM tbl_ewc_shippingOptions", "Options");
                myWeb.moDbHelper.addTableToDataSet(ref oDS, "SELECT * FROM tbl_ewc_shippingRelations", "Relations");

                // Add new column for IDs for local stuff and make all attributes
                if (oDS is null)
                    return true;
                foreach (DataTable oDT in oDS.Tables)
                {
                    oDT.Columns.Add("NewID", typeof(long));
                    foreach (DataColumn oDC in oDT.Columns)
                        oDC.ColumnMapping = MappingType.Attribute;
                }

                oDS.Relations.Add("Rel0", oDS.Tables["Locations"].Columns["nLocationKey"], oDS.Tables["Locations"].Columns["nLocationParID"], false);
                oDS.Relations["Rel0"].Nested = true;

                oDXML.InnerXml = oDS.GetXml();

                // going to do the locations first since these can potentially have n-tier relations (most complex)

                foreach (XmlElement oLocElmt in oDXML.SelectNodes("descendant-or-self::Locations"))
                {
                    // If Not oMenuElmt.ParentNode.SelectSingleNode("@NewID") Is Nothing Then
                    // nParID = oMenuElmt.ParentNode.SelectSingleNode("@NewID").InnerText
                    // End If
                    AddResponse("Migrating Location " + oLocElmt.GetAttribute("cLocationNameFull") + "");
                    // check parent
                    if (oLocElmt.ParentNode.SelectSingleNode("@NewID") != null)
                    {
                        nParID = Conversions.ToInteger(oLocElmt.ParentNode.SelectSingleNode("@NewID").InnerText);
                    }
                    cSQL = "INSERT INTO tblCartShippingLocations (nLocationType, nLocationParId, cLocationForeignRef, cLocationNameFull, " + "cLocationNameShort, cLocationISOnum, cLocationISOa2, cLocationISOa3, cLocationCode, nLocationTaxRate, " + "nAuditId) VALUES (";
                    if (string.IsNullOrEmpty(oLocElmt.GetAttribute("nLocationType")))
                        cSQL += "Null,";
                    else
                        cSQL += oLocElmt.GetAttribute("nLocationType") + ",";
                    if (string.IsNullOrEmpty(oLocElmt.GetAttribute("nLocationParId")))
                        cSQL += "Null,";
                    else
                        cSQL += oLocElmt.GetAttribute("nLocationParId") + ",";
                    if (string.IsNullOrEmpty(oLocElmt.GetAttribute("nLocationKey")))
                        cSQL += "Null,";
                    else
                        cSQL += oLocElmt.GetAttribute("nLocationKey") + ",";
                    if (string.IsNullOrEmpty(oLocElmt.GetAttribute("cLocationNameFull")))
                        cSQL += "Null,";
                    else
                        cSQL += "'" + Strings.Replace(oLocElmt.GetAttribute("cLocationNameFull"), "'", "''") + "',";
                    if (string.IsNullOrEmpty(oLocElmt.GetAttribute("cLocationNameShort")))
                        cSQL += "Null,";
                    else
                        cSQL += "'" + Strings.Replace(oLocElmt.GetAttribute("cLocationNameShort"), "'", "''") + "',";
                    if (string.IsNullOrEmpty(oLocElmt.GetAttribute("cLocationISOnum")))
                        cSQL += "Null,";
                    else
                        cSQL += "'" + oLocElmt.GetAttribute("cLocationISOnum") + "',";
                    if (string.IsNullOrEmpty(oLocElmt.GetAttribute("cLocationISOa2")))
                        cSQL += "Null,";
                    else
                        cSQL += "'" + oLocElmt.GetAttribute("cLocationISOa2") + "',";
                    if (string.IsNullOrEmpty(oLocElmt.GetAttribute("cLocationISOa3")))
                        cSQL += "Null,";
                    else
                        cSQL += "'" + oLocElmt.GetAttribute("cLocationISOa3") + "',";
                    if (string.IsNullOrEmpty(oLocElmt.GetAttribute("cLocationCode")))
                        cSQL += "Null,";
                    else
                        cSQL += "'" + oLocElmt.GetAttribute("cLocationCode") + "',";
                    if (string.IsNullOrEmpty(oLocElmt.GetAttribute("nLocationTaxRate")))
                        cSQL += "Null,";
                    else
                        cSQL += "'" + oLocElmt.GetAttribute("nLocationTaxRate") + "',";
                    cSQL += myWeb.moDbHelper.getAuditId() + ")";
                    nParID =Convert.ToInt32(myWeb.moDbHelper.GetIdInsertSql(cSQL));
                    oLocElmt.SetAttribute("NewID", nParID.ToString());
                }

                // now the methods...

                foreach (XmlElement oMetElmt in oDXML.SelectNodes("descendant-or-self::Options"))
                {
                    AddResponse("Migrating Option " + oMetElmt.GetAttribute("cShipOptName") + "");
                    cSQL = "INSERT INTO tblCartShippingMethods (nShipOptCat, cShipOptForeignRef, cShipOptName, cShipOptCarrier, cShipOptTime, " + "cShipOptTandC, nShipOptCost, nShipOptPercentage, nShipOptQuantMin, nShipOptQuantMax, nShipOptWeightMin, " + "nShipOptWeightMax, nShipOptPriceMin, nShipOptPriceMax, nShipOptHandlingPercentage, nShipOptHandlingFixedCost, " + "nShipOptTaxRate, nAuditId) VALUES (";

                    cSQL += "Null,"; // no cat
                    if (string.IsNullOrEmpty(oMetElmt.GetAttribute("nShipOptKey")))
                        cSQL += "Null,";
                    else
                        cSQL += "'" + oMetElmt.GetAttribute("nShipOptKey") + "',";
                    if (string.IsNullOrEmpty(oMetElmt.GetAttribute("cShipOptName")))
                        cSQL += "Null,";
                    else
                        cSQL += "'" + oMetElmt.GetAttribute("cShipOptName") + "',";
                    if (string.IsNullOrEmpty(oMetElmt.GetAttribute("cShipOptCarrier")))
                        cSQL += "Null,";
                    else
                        cSQL += "'" + oMetElmt.GetAttribute("cShipOptCarrier") + "',";
                    if (string.IsNullOrEmpty(oMetElmt.GetAttribute("cShipOptTime")))
                        cSQL += "Null,";
                    else
                        cSQL += "'" + oMetElmt.GetAttribute("cShipOptTime") + "',";
                    if (string.IsNullOrEmpty(oMetElmt.GetAttribute("cShipOptTandC")))
                        cSQL += "Null,";
                    else
                        cSQL += "'" + oMetElmt.GetAttribute("cShipOptTandC") + "',";
                    if (string.IsNullOrEmpty(oMetElmt.GetAttribute("nShipOptCost")))
                        cSQL += "Null,";
                    else
                        cSQL += oMetElmt.GetAttribute("nShipOptCost") + ",";
                    if (string.IsNullOrEmpty(oMetElmt.GetAttribute("nShipOptPercentage")))
                        cSQL += "Null,";
                    else
                        cSQL += oMetElmt.GetAttribute("nShipOptPercentage") + ",";
                    if (string.IsNullOrEmpty(oMetElmt.GetAttribute("nShipOptQuantMin")))
                        cSQL += "Null,";
                    else
                        cSQL += oMetElmt.GetAttribute("nShipOptQuantMin") + ",";
                    if (string.IsNullOrEmpty(oMetElmt.GetAttribute("nShipOptQuantMax")))
                        cSQL += "Null,";
                    else
                        cSQL += oMetElmt.GetAttribute("nShipOptQuantMax") + ",";
                    if (string.IsNullOrEmpty(oMetElmt.GetAttribute("nShipOptWeightMin")))
                        cSQL += "Null,";
                    else
                        cSQL += oMetElmt.GetAttribute("nShipOptWeightMin") + ",";
                    if (string.IsNullOrEmpty(oMetElmt.GetAttribute("nShipOptWeightMax")))
                        cSQL += "Null,";
                    else
                        cSQL += oMetElmt.GetAttribute("nShipOptWeightMax") + ",";
                    if (string.IsNullOrEmpty(oMetElmt.GetAttribute("nShipOptPriceMin")))
                        cSQL += "Null,";
                    else
                        cSQL += oMetElmt.GetAttribute("nShipOptPriceMin") + ",";
                    if (string.IsNullOrEmpty(oMetElmt.GetAttribute("nShipOptPriceMax")))
                        cSQL += "Null,";
                    else
                        cSQL += oMetElmt.GetAttribute("nShipOptPriceMax") + ",";
                    if (string.IsNullOrEmpty(oMetElmt.GetAttribute("nShipOptHandlingPercentage")))
                        cSQL += "Null,";
                    else
                        cSQL += oMetElmt.GetAttribute("nShipOptHandlingPercentage") + ",";
                    if (string.IsNullOrEmpty(oMetElmt.GetAttribute("nShipOptHandlingFixedCost")))
                        cSQL += "Null,";
                    else
                        cSQL += oMetElmt.GetAttribute("nShipOptHandlingFixedCost") + ",";
                    if (string.IsNullOrEmpty(oMetElmt.GetAttribute("nShipOptTaxRate")))
                        cSQL += "Null,";
                    else
                        cSQL += oMetElmt.GetAttribute("nShipOptTaxRate") + ",";
                    cSQL += myWeb.moDbHelper.getAuditId() + ")";
                    nParID =Convert.ToInt32(myWeb.moDbHelper.GetIdInsertSql(cSQL));
                    oMetElmt.SetAttribute("NewID", nParID.ToString());
                }
                AddResponse("Migrating Relations");
                // now the locations
                var nOpt = default(int);
                var nLoc = default(int);
                foreach (XmlElement oRelElmt in oDXML.SelectNodes("descendant-or-self::Relations"))
                {
                    if (!string.IsNullOrEmpty(oRelElmt.GetAttribute("nShpOptId")))
                        nOpt = Convert.ToInt32(myWeb.moDbHelper.getObjectByRef(Cms.dbHelper.objectTypes.CartShippingMethod, oRelElmt.GetAttribute("nShpOptId")));
                    if (!string.IsNullOrEmpty(oRelElmt.GetAttribute("nShpLocId")))
                        nLoc = Convert.ToInt32(myWeb.moDbHelper.getObjectByRef(Cms.dbHelper.objectTypes.CartShippingLocation, oRelElmt.GetAttribute("nShpLocId")));
                    // only add it if there is something to add on both ends of the relation
                    if (!(nOpt == 0 | nLoc == 0))
                    {
                        cSQL = "INSERT INTO tblCartShippingRelations (nShpOptID, nShpLocId, nAuditId) VALUES(";
                        cSQL += nOpt + ",";
                        cSQL += nLoc + ",";

                        cSQL += myWeb.moDbHelper.getAuditId() + ")";
                        nParID = Convert.ToInt32(myWeb.moDbHelper.GetIdInsertSql(cSQL));
                        oRelElmt.SetAttribute("NewID", nParID.ToString());
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                AddResponseError(ex); // returnException(myWeb.msException, mcModuleName, "Migrate_Shipping", ex, "", "", gbDebug)
                return false;
            }
        }

        public bool Migrate_Cart()
        {
            Protean.Cms.dbHelper dbh = myWeb.moDbHelper;
            DataSet oDS;
            var oDXML = new XmlDocument();
            int nParID;
            int nParID2;
            string cSQLP1;
            string cSQLP2;
            try
            {
                AddResponse("Migrating Orders");
                if (!dbh.TableExists("tbl_ewc_cartOrder"))
                    return true;
                oDS = dbh.GetDataSet("SELECT * FROM tbl_ewc_cartOrder", "Orders", "Cart");

                dbh.addTableToDataSet(ref oDS, "SELECT * FROM tbl_ewc_cartItem", "Items");
                dbh.addTableToDataSet(ref oDS, "SELECT * FROM tbl_ewc_Contact where nContactParentType = 1", "Contacts");

                oDS.Relations.Add("CartItems", oDS.Tables["Orders"].Columns["nCartOrderKey"], oDS.Tables["Items"].Columns["nCartItemKey"], false);
                oDS.Relations["CartItems"].Nested = true;
                oDS.Relations.Add("CartContacts", oDS.Tables["Orders"].Columns["nCartOrderKey"], oDS.Tables["Contacts"].Columns["nContactParentId"], false);
                oDS.Relations["CartContacts"].Nested = true;

                foreach (DataTable oDT in oDS.Tables)
                {
                    foreach (DataColumn oDC in oDT.Columns)
                        oDC.ColumnMapping = MappingType.Attribute;
                }

                oDXML.InnerXml = Strings.Replace(oDS.GetXml(), "'", "''");

                // ok, now we need to loop through the carts, 
                // add the items, splitting out the options as well
                // then add the contacts for the cart
                // having lots of checking so just in case there is a column missing
                var nOrdCount = default(int);
                foreach (XmlElement oOrdElmt in oDXML.SelectNodes("descendant-or-self::Orders"))
                {
                    nOrdCount += 1;
                    AddResponse("Migrating Order " + oOrdElmt.GetAttribute("nCartOrderKey") + "(" + nOrdCount + ")");
                    // Firstly the Cart Order
                    cSQLP2 = "";
                    cSQLP1 = "INSERT INTO tblCartOrder " + "(cCartForiegnRef, nCartStatus, cCartSchemaName, cCartSessionId, nCartUserDirId, " + "nPayMthdId, cPaymentRef, cCartXml, nShippingMethodId, cShippingDesc, nShippingCost, " + "cClientNotes, cSellerNotes, nTaxRate, nGiftListId, nAuditId) VALUES( ";

                    if (string.IsNullOrEmpty(oOrdElmt.GetAttribute("nCartOrderKey")))
                        cSQLP2 += "Null,";
                    else
                        cSQLP2 += oOrdElmt.GetAttribute("nCartOrderKey") + ",";

                    if (string.IsNullOrEmpty(oOrdElmt.GetAttribute("nCartStatus")))
                        cSQLP2 += "0,";
                    else
                        cSQLP2 = Conversions.ToString(cSQLP2 + Operators.ConcatenateObject(Interaction.IIf(Conversions.ToDouble(oOrdElmt.GetAttribute("nCartStatus")) == 6d, 7, oOrdElmt.GetAttribute("nCartStatus")), ","));
                    cSQLP2 += "Null,"; // Cart Schema Name
                    if (string.IsNullOrEmpty(oOrdElmt.GetAttribute("cCartSessionId")))
                        cSQLP2 += "Null,";
                    else
                        cSQLP2 += "'" + oOrdElmt.GetAttribute("cCartSessionId") + "',";
                    if (string.IsNullOrEmpty(oOrdElmt.GetAttribute("nCartUserDirId")))
                        cSQLP2 += "Null,";
                    else
                        cSQLP2 += oOrdElmt.GetAttribute("nCartUserDirId") + ",";

                    if (string.IsNullOrEmpty(oOrdElmt.GetAttribute("nPaymentMethod")))
                    {
                        cSQLP2 += "Null,";
                    }
                    else if (!Information.IsNumeric(oOrdElmt.GetAttribute("nPaymentMethod")))
                    {
                        cSQLP2 += "Null,";
                    }
                    else
                    {
                        cSQLP2 += oOrdElmt.GetAttribute("nPaymentMethod") + ",";
                    }

                    if (string.IsNullOrEmpty(oOrdElmt.GetAttribute("nPaymentRef")))
                        cSQLP2 += "Null,";
                    else
                        cSQLP2 += "'" + oOrdElmt.GetAttribute("nPaymentRef") + "',";
                    cSQLP2 += "Null,"; // Cart XML

                    if (string.IsNullOrEmpty(oOrdElmt.GetAttribute("nShippingId")))
                        cSQLP2 += "Null,";
                    else
                        cSQLP2 += moDbHelper.getObjectByRef(Cms.dbHelper.objectTypes.CartShippingMethod, oOrdElmt.GetAttribute("nShippingId")) + ",";
                    if (string.IsNullOrEmpty(oOrdElmt.GetAttribute("cShippingDesc")))
                        cSQLP2 += "Null,";
                    else
                        cSQLP2 += "'" + oOrdElmt.GetAttribute("cShippingDesc") + "',";
                    if (string.IsNullOrEmpty(oOrdElmt.GetAttribute("nShippingCost")))
                        cSQLP2 += "0,";
                    else
                        cSQLP2 += oOrdElmt.GetAttribute("nShippingCost") + ",";
                    if (string.IsNullOrEmpty(oOrdElmt.GetAttribute("cClientNotes")))
                        cSQLP2 += "Null,";
                    else
                        cSQLP2 += "'<Notes><Notes>" + oOrdElmt.GetAttribute("cClientNotes") + "</Notes></Notes>',";
                    if (string.IsNullOrEmpty(oOrdElmt.GetAttribute("cSellerNotes")))
                        cSQLP2 += "Null,";
                    else
                        cSQLP2 += "'" + oOrdElmt.GetAttribute("cSellerNotes") + "',";
                    if (string.IsNullOrEmpty(oOrdElmt.GetAttribute("nTaxRate")))
                        cSQLP2 += "0,";
                    else
                        cSQLP2 += oOrdElmt.GetAttribute("nTaxRate") + ",";
                    if (string.IsNullOrEmpty(oOrdElmt.GetAttribute("nGiftListID")))
                        cSQLP2 += "Null,";
                    else
                        cSQLP2 += "'" + oOrdElmt.GetAttribute("nGiftListID") + "',";
                    cSQLP2 += dbh.getAuditId() + ")";
                    nParID = Convert.ToInt32(dbh.GetIdInsertSql(cSQLP1 + cSQLP2));

                    // Now the cart Items
                    foreach (XmlElement oItElmt in oOrdElmt.SelectNodes("Items"))
                    {
                        AddResponse("Migrating Item " + oItElmt.GetAttribute("nCartItemKey") + "");
                        cSQLP2 = "";
                        cSQLP1 = "INSERT INTO tblCartItem (nCartOrderId, nItemId, nParentID, cItemRef, " + "cItemURL, cItemName, nItemOptGrpIdx, nItemOptIdx, nPrice, nShpCat, nDiscountCat, " + "nDiscountValue,nTaxRate, nQuantity, nWeight, nAuditId) VALUES (";

                        cSQLP2 += nParID + ","; // OrderID
                        if (string.IsNullOrEmpty(oItElmt.GetAttribute("nItemId")))
                            cSQLP2 += "Null,";
                        else
                            cSQLP2 += moDbHelper.getObjectByRef(Cms.dbHelper.objectTypes.Content, oItElmt.GetAttribute("nItemId")) + ",";
                        cSQLP2 += "Null,"; // product parentID for options
                        if (string.IsNullOrEmpty(oItElmt.GetAttribute("cItemRef")))
                            cSQLP2 += "Null,";
                        else
                            cSQLP2 += "'" + oItElmt.GetAttribute("cItemRef") + "',";
                        if (string.IsNullOrEmpty(oItElmt.GetAttribute("cItemURL")))
                            cSQLP2 += "Null,";
                        else
                            cSQLP2 += "'" + oItElmt.GetAttribute("cItemURL") + "',";
                        if (string.IsNullOrEmpty(oItElmt.GetAttribute("cItemName")))
                            cSQLP2 += "Null,";
                        else
                            cSQLP2 += "'" + CleanName(oItElmt.GetAttribute("cItemName")) + "',";
                        cSQLP2 += "Null,"; // nItemOptGrpIdx
                        cSQLP2 += "Null,"; // nItemOptIdx
                        if (string.IsNullOrEmpty(oItElmt.GetAttribute("nPrice")))
                            cSQLP2 += "Null,";
                        else
                            cSQLP2 += oItElmt.GetAttribute("nPrice") + ",";
                        if (string.IsNullOrEmpty(oItElmt.GetAttribute("nShippingLevel")))
                            cSQLP2 += "Null,";
                        else
                            cSQLP2 += oItElmt.GetAttribute("nShippingLevel") + ",";
                        cSQLP2 += "Null,"; // nDiscountCat
                        cSQLP2 += "Null,"; // nDiscountValue
                        if (string.IsNullOrEmpty(oItElmt.GetAttribute("nTaxRate")))
                            cSQLP2 += "Null,";
                        else
                            cSQLP2 += oItElmt.GetAttribute("nTaxRate") + ",";
                        if (string.IsNullOrEmpty(oItElmt.GetAttribute("nQuantity")))
                            cSQLP2 += "Null,";
                        else
                            cSQLP2 += oItElmt.GetAttribute("nQuantity") + ",";
                        // If oItElmt.GetAttribute("nDiscount") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= oItElmt.GetAttribute("nDiscount") & ","
                        if (string.IsNullOrEmpty(oItElmt.GetAttribute("nWeight")))
                            cSQLP2 += "Null,";
                        else
                            cSQLP2 += oItElmt.GetAttribute("nWeight") + ",";
                        cSQLP2 += dbh.getAuditId() + ")";
                        nParID2 = Convert.ToInt32(dbh.GetIdInsertSql(cSQLP1 + cSQLP2));

                        // Now for splitting out the options
                        // basically loops through and checks if there is anything in option 1 or 2, if there is add it(ish)
                        int nOptNo = 1;
                    DoOptions:
                        ;

                        if (nOptNo < 3) // since its only options 1 and 2 we dont want to try 3
                        {
                            if (!string.IsNullOrEmpty(oItElmt.GetAttribute("cItemOption" + nOptNo)))
                            {
                                cSQLP2 = "";
                                cSQLP1 = "INSERT INTO tblCartItem (nCartOrderId, nItemId, nParentID, cItemRef, " + "cItemURL, cItemName, nItemOptGrpIdx, nItemOptIdx, nPrice, nShpCat, nDiscountCat, " + "nDiscountValue, nTaxRate, nQuantity, nWeight, nAuditId) VALUES (";

                                cSQLP2 += nParID + ","; // OrderID
                                if (string.IsNullOrEmpty(oItElmt.GetAttribute("nItemId")))
                                    cSQLP2 += "Null,";
                                else
                                    cSQLP2 += moDbHelper.getObjectByRef(Cms.dbHelper.objectTypes.Content, oItElmt.GetAttribute("nItemId")) + ",";
                                cSQLP2 += nParID2 + ","; // product parentID for options
                                cSQLP2 += "'" + oItElmt.GetAttribute("cItemOption" + nOptNo) + "',"; // New ref as option
                                if (string.IsNullOrEmpty(oItElmt.GetAttribute("cItemURL")))
                                    cSQLP2 += "Null,";
                                else
                                    cSQLP2 += "'" + oItElmt.GetAttribute("cItemURL") + "',";
                                cSQLP2 += "'" + oItElmt.GetAttribute("cItemOption" + nOptNo) + "',"; // New Name as option
                                cSQLP2 += "0,"; // nItemOptGrpIdx
                                cSQLP2 += "'" + (nOptNo - 1) + "',"; // nItemOptIdx
                                cSQLP2 += "0,"; // price
                                if (string.IsNullOrEmpty(oItElmt.GetAttribute("nShippingLevel")))
                                    cSQLP2 += "Null,";
                                else
                                    cSQLP2 += oItElmt.GetAttribute("nShippingLevel") + ",";
                                cSQLP2 += "Null,"; // nDiscountCat
                                cSQLP2 += "Null,"; // nDiscountValue
                                if (string.IsNullOrEmpty(oItElmt.GetAttribute("nTaxRate")))
                                    cSQLP2 += "Null,";
                                else
                                    cSQLP2 += oItElmt.GetAttribute("nTaxRate") + ",";
                                cSQLP2 += "0,"; // quantity
                                                // cSQLP2 &= "0," 'Discount
                                cSQLP2 += "0,";
                                cSQLP2 += dbh.getAuditId() + ")";
                                dbh.GetIdInsertSql(cSQLP1 + cSQLP2);
                            }
                            nOptNo += 1;
                            goto DoOptions;
                        }
                    }
                    // now for the contacts
                    foreach (XmlElement oConElmt in oOrdElmt.SelectNodes("Contacts"))
                    {
                        AddResponse("Migrating Contact " + oConElmt.GetAttribute("nContactKey") + "");
                        // Firstly the Cart Order
                        cSQLP2 = "";
                        cSQLP1 = "INSERT INTO tblCartContact (nContactDirId, nContactCartId, cContactType, cContactName, " + "cContactCompany, cContactAddress, cContactCity, cContactState, cContactZip, cContactCountry, " + "cContactTel, cContactFax, cContactEmail, cContactXml, nAuditId) VALUES (";

                        cSQLP2 += "0,";
                        cSQLP2 += nParID + ",";
                        if (string.IsNullOrEmpty(oConElmt.GetAttribute("cContactType")))
                            cSQLP2 += "Null,";
                        else
                            cSQLP2 += "'" + oConElmt.GetAttribute("cContactType") + "',";
                        if (string.IsNullOrEmpty(oConElmt.GetAttribute("cContactName")))
                            cSQLP2 += "Null,";
                        else
                            cSQLP2 += "'" + Strings.Replace(oConElmt.GetAttribute("cContactName"), "'", "''") + "',";
                        if (string.IsNullOrEmpty(oConElmt.GetAttribute("cContactCompany")))
                            cSQLP2 += "Null,";
                        else
                            cSQLP2 += "'" + Strings.Replace(oConElmt.GetAttribute("cContactCompany"), "'", "''") + "',";
                        if (string.IsNullOrEmpty(oConElmt.GetAttribute("cContactAddress")))
                            cSQLP2 += "Null,";
                        else
                            cSQLP2 += "'" + Strings.Replace(oConElmt.GetAttribute("cContactAddress"), "'", "''") + "',";
                        if (string.IsNullOrEmpty(oConElmt.GetAttribute("cContactCity")))
                            cSQLP2 += "Null,";
                        else
                            cSQLP2 += "'" + oConElmt.GetAttribute("cContactCity") + "',";
                        if (string.IsNullOrEmpty(oConElmt.GetAttribute("cContactState")))
                            cSQLP2 += "Null,";
                        else
                            cSQLP2 += "'" + oConElmt.GetAttribute("cContactState") + "',";
                        if (string.IsNullOrEmpty(oConElmt.GetAttribute("cContactZip")))
                            cSQLP2 += "Null,";
                        else
                            cSQLP2 += "'" + oConElmt.GetAttribute("cContactZip") + "',";
                        if (string.IsNullOrEmpty(oConElmt.GetAttribute("cContactCountry")))
                            cSQLP2 += "Null,";
                        else
                            cSQLP2 += "'" + oConElmt.GetAttribute("cContactCountry") + "',";
                        if (string.IsNullOrEmpty(oConElmt.GetAttribute("cContactTel")))
                            cSQLP2 += "Null,";
                        else
                            cSQLP2 += "'" + oConElmt.GetAttribute("cContactTel") + "',";
                        if (string.IsNullOrEmpty(oConElmt.GetAttribute("cContactFax")))
                            cSQLP2 += "Null,";
                        else
                            cSQLP2 += "'" + oConElmt.GetAttribute("cContactFax") + "',";
                        if (string.IsNullOrEmpty(oConElmt.GetAttribute("cContactEmail")))
                            cSQLP2 += "Null,";
                        else
                            cSQLP2 += "'" + oConElmt.GetAttribute("cContactEmail") + "',";
                        cSQLP2 += "Null,";
                        cSQLP2 += dbh.getAuditId() + ")";
                        dbh.GetIdInsertSql(cSQLP1 + cSQLP2);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                AddResponseError(ex); // returnException(myWeb.msException, mcModuleName, "Migrate_Cart", ex, "", "", gbDebug)
                return false;
            }

        }

        public string upgradeContentSchemas(string cContentSchema, string cContentXml, string type)
        {

            StringWriter sWriter;
            string sResult;
            var oXmlDr = new XmlDocument();
            string cProcessInfo = "error converting type:" + cContentSchema + " data:" + cContentXml;
            var oTransform2 = new Protean.XmlHelper.Transform();
            try
            {
                // fix for any nasty entities
                cContentXml = Tools.Xml.convertEntitiesToCodes(cContentXml);

                oXmlDr.LoadXml(cContentXml);
                // add Eonic Bespoke Functions
                var xsltArgs = new System.Xml.Xsl.XsltArgumentList();

                // Run transformation
                string cPath = goServer.MapPath(goConfig["ProjectPath"] + "/migratexsl/" + cContentSchema + "-" + type + ".xsl");
                if (!File.Exists(cPath))
                {
                    cPath = goServer.MapPath("/ewcommon/migratexsl/" + cContentSchema + "-" + type + ".xsl");
                    if (!File.Exists(cPath))
                    {
                        return cContentXml;
                    }
                }
                oTransform2.XSLFile = cPath;
                oTransform2.Compiled = false;
                sWriter = new StringWriter();
                var oInputDoc = new XmlDocument();
                oInputDoc.LoadXml(cContentXml);
                TextWriter argoWriter = sWriter;
                oTransform2.Process(oInputDoc, ref argoWriter);
                sWriter = (StringWriter)argoWriter;

                sResult = sWriter.ToString();

                sWriter = null;
                oXmlDr = null;


                return sResult;
            }

            catch (Exception ex)
            {
                AddResponseError(ex, cProcessInfo); // returnException(myWeb.msException, mcModuleName, "upgradeContentSchemas", ex, "", cProcessInfo, gbDebug)
                return "";
            }

        }

        public bool Migrate_Rollback()
        {
            // deletes v4 Tables
            try
            {
                object dbUpdatePath = "/ewcommon/sqlupdate";
                if (goConfig["cssFramework"] == "bs5")
                {
                    dbUpdatePath = "/ptn/update/sql";
                }
                // AddResponse("Removing V4 Tables")
                myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath(Conversions.ToString(Operators.ConcatenateObject(dbUpdatePath, "/toV4/DropAllForeignKeys.sql"))));
                myWeb.msException = "";
                myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath(Conversions.ToString(Operators.ConcatenateObject(dbUpdatePath, "/toV4/ClearDB.SQL"))));
                AddResponse("Run File (/ewcommon/sqlupdate/toV4/ClearDB.SQL)");
                return saveVersionNumber("0.0.0.0");
            }
            catch (Exception ex)
            {
                AddResponseError(ex); // returnException(myWeb.msException, mcModuleName, "Rollback_Migration", ex, "", "", gbDebug)
                return false;
            }
        }


        public string CleanName(string cName, bool bLeaveAmp = false)
        {

            return Tools.Text.CleanName(cName, bLeaveAmp, false);

        }

        #endregion

        #region Table Dropping
        private void ifTableExistsDropIt(ref string sTableName)
        {

            string sSqlStr;
            //string cProcessInfo = "ifTableExistsDropIt";
            try
            {
                sSqlStr = "if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[" + sTableName + "]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)";
                sSqlStr = sSqlStr + "drop table [dbo].[" + sTableName + "] ";
                myWeb.moDbHelper.ExeProcessSql(sSqlStr);
            }
            catch (Exception ex)
            {
                AddResponseError(ex); // returnException(myWeb.msException, mcModuleName, "ifTableExistsDropIt", ex, "", cProcessInfo, gbDebug)
            }
        }
        private void ifSqlObjectExistsDropIt(ref string sName, ref string sObjectType)
        {

            string sSqlStr;
            //string cProcessInfo = "ifTableExistsDropIt";
            string sObjProperty;
            try
            {

                switch (sObjectType ?? "")
                {
                    case "Table":
                        {
                            sObjProperty = "OBJECTPROPERTY(id, N'IsUserTable') = 1";
                            break;
                        }
                    case "Function":
                        {
                            sObjProperty = "xtype in (N'FN', N'IF', N'TF')";
                            break;
                        }

                    default:
                        {
                            sObjProperty = "OBJECTPROPERTY(id, N'Is" + sObjectType + "') = 1";
                            break;
                        }
                }

                sSqlStr = "if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[" + sName + "]') and " + sObjProperty + ")";
                sSqlStr = sSqlStr + "drop " + Strings.LCase(sObjectType) + " [dbo].[" + sName + "] ";
                myWeb.moDbHelper.ExeProcessSql(sSqlStr);
            }

            catch (Exception ex)
            {
                AddResponseError(ex); // returnException(myWeb.msException, mcModuleName, "ifSqlObjectExistsDropIt", ex, "", cProcessInfo, gbDebug)
            }
        }

        #endregion

        #region Import / Export Routines

        public bool LoadShippingLocations()
        {

            // BJR NEW CODE BELOW


            // Dim cProcessInfo As String = "LoadShippingLocations"

            // Try
            // Dim oXml As XmlDocument = New XmlDocument
            // Dim bIsXml As Boolean = False

            // Try
            // oXml.Load(goServer.MapPath(dbUpdatePath & "/import_data/ex_shiplocs_master.xml"))
            // bIsXml = True
            // Catch ex As Exception
            // ' If Load fails then there's something invalid about what we just imported.
            // End Try

            // If bIsXml Then
            // ' Try to validate the xml
            // moDBHelper.importShippingLocations(oXml)
            // End If

            // AddResponse("Import Shipping Locations'")

            // Return True
            // Catch ex As Exception
            // AddResponseError(ex) 'returnException(myWeb.msException, mcModuleName, "ifSqlObjectExistsDropIt", ex, "", cProcessInfo, gbDebug)
            // Return False
            // End Try

            //string cProcessInfo = "LoadShippingLocations";

            try
            {
                var oXml = new XmlDocument();
                bool bIsXml = false;

                try
                {
                    string ShippingLocationsPath = "/ewcommon/sqlupdate/import_data/ex_shiplocs_master2.xml";
                    if (goConfig["cssFramework"] == "bs5")
                    {
                        ShippingLocationsPath = "/ptn/setup/data/ex_shiplocs.xml";
                    }

                    oXml.Load(goServer.MapPath(ShippingLocationsPath));
                    bIsXml = true;
                }
                catch (Exception)
                {
                    // If Load fails then there's something invalid about what we just imported.
                }

                if (bIsXml)
                {
                    // Try to validate the xml
                    myWeb.moDbHelper.importShippingLocations2(ref oXml);
                }

                AddResponse("Import Shipping Locations'");

                return true;
            }
            catch (Exception ex)
            {
                AddResponseError(ex); // returnException(myWeb.msException, mcModuleName, "ifSqlObjectExistsDropIt", ex, "", cProcessInfo, gbDebug)
                return false;
            }
        }


        public void ManageShippingLocations()
        {
            string cImport = "";
            StreamReader oImportStream;
            System.Web.HttpPostedFile fUpld;
            var oXml = new XmlDocument();
            bool bIsXml = false;
            bool bIsUpload = false;

            if (goRequest["export"] != null)
            {
                goResponse.AddHeader("Content-Type", "text/xml");
                goResponse.AddHeader("Content-Disposition", "attachment; filename=ex_shiplocs_" + Strings.Format(DateTime.Now, "yyMMddHHmmss") + ".xml");
                goResponse.Write("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + moDbHelper.exportShippingLocations());
            }
            else
            {
                if (goRequest["import"] != null)
                {
                    // lets do some hacking 
                    bIsUpload = true;
                    fUpld = goRequest.Files["importfile"];

                    if (fUpld != null)
                    {
                        // Initialize the stream.
                        oImportStream = new StreamReader(fUpld.InputStream);
                        cImport = oImportStream.ReadToEnd();
                        oImportStream.Close();

                        // Check the import stream for XML correctness
                        try
                        {
                            oXml.LoadXml(cImport);
                            bIsXml = true;
                        }
                        catch (Exception)
                        {
                            // If Load fails then there's something invalid about what we just imported.
                        }

                        if (bIsXml)
                        {
                            // Try to validate the xml
                            moDbHelper.importShippingLocations(ref oXml);
                        }
                    }

                }

                goResponse.Write("<html><head><title>Manage Shipping Locations</title></head><body>");
                goResponse.Write("<form method=post action=\"\" enctype=\"multipart/form-data\">Database Name (optional - uses web.config if none) <input type=text name=\"db\"/>  Source Version: <select name=\"version\"><option value=\"v2\">v2-v3</option><option value=\"v4\">v4</option></select><input type=submit name=export value=\"Export\"/><br/>");
                goResponse.Write("<input type=file name=\"importfile\"/><input type=submit name=import value=\"Import\"/>");
                goResponse.Write("</form>");

                if (bIsUpload)
                {

                    goResponse.Write("<div style=\"height: 200px;width:100%;overflow:scroll;\">");
                    if (bIsXml)
                    {
                        goResponse.Write(Strings.Replace(Strings.Replace(cImport, ">", "&gt;"), "<", "&lt;"));
                    }
                    else
                    {
                        goResponse.Write("The imported file was not valid xml");
                    }
                    goResponse.Write("</div>");

                }

                goResponse.Write("</body></html>");
            }

        }
        #endregion

        internal int CommitToLog(Protean.Cms.dbHelper.ActivityType nEventType, int nUserId, string cSessionId, DateTime dDateTime, int nPrimaryId = 0, int nSecondaryId = 0, string cDetail = "")
        {
            string cSQL = "INSERT INTO tblActivityLog (nUserDirId, nStructId, nArtId, dDateTime, nActivityType, cActivityDetail, cSessionId) VALUES (";
            cSQL += nUserId + ",";
            cSQL += nPrimaryId + ",";
            cSQL += nSecondaryId + ",";
            cSQL += Tools.Database.SqlDate(dDateTime, true) + ",";
            cSQL += nEventType + ",";
            cSQL += "'" + cDetail + "',";
            cSQL += "'" + cSessionId + "')";
            return Convert.ToInt32(myWeb.moDbHelper.GetIdInsertSql(cSQL));
        }

        internal void UpdateLogDetail(int nActivityKey, string cActivityDetail)
        {
            string cSQL = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("UPDATE tblActivityLog " + "SET cActivityDetail = '", SqlFmt(cActivityDetail)), "' "), "WHERE nActivityKey = "), SqlFmt(nActivityKey.ToString())));

            myWeb.moDbHelper.ExeProcessSql(cSQL);
        }


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
                    siteUrl = Strings.Replace(siteUrl, "http://", "");
                    siteUrl = Strings.Replace(siteUrl, "www.", "");
                    siteUrl = Strings.Replace(siteUrl, ".", "_");
                    siteUrl = Strings.Replace(siteUrl, "-", "_");
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
                   // oFrmElmt = (XmlElement)argoNode;

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

                    if (Conversions.ToBoolean(oFsh.VirtualFileExists("/protean.web.config")))
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
                            Cms.dbHelper oDbt = new Cms.dbHelper(ref myWebArg);
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
                                   // XmlNode argoNode1 = oFrmElmt;
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
                string DatabaseFilename = System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetYear(DateTime.Now) + "-" + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMonth(DateTime.Now) + "-" + System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetDayOfMonth(DateTime.Now) + "-" + Conversions.ToString(DateAndTime.TimeOfDay) + "-" + goConfig["DatabaseName"] + ".bak";
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

    public class FileStructureSetup
    {
        public Setup oSetup;
        public string oEwVersion = "5.0";
        public double IISVersion = 7d;
        public System.Collections.Specialized.NameValueCollection goConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");
        public string imageRootPath = "/images";
        public string docRootPath = "/docs";
        public string docMediaPath = "/media";
        public string errMsg = "";


        public bool Execute()
        {

            try
            {

                // Dim oImp As Protean.Tools.Security.Impersonate = New Protean.Tools.Security.Impersonate
                // If oImp.ImpersonateValidUser(goConfig("AdminAcct"), goConfig("AdminDomain"), goConfig("AdminPassword"), True, goConfig("AdminGroup")) Then

                if (IISVersion < 7d)
                {
                    // copy the .htaccess file for version 3 of Isapi Rewrite
                    // Dim fso As IO.File
                    if (!File.Exists(goServer.MapPath(goConfig["ProjectPath"] + "/.htaccess")))
                    {
                        File.Copy(goServer.MapPath("/ewcommon/setup/rootfiles/.htaccess"), goServer.MapPath(goConfig["ProjectPath"] + "/.htaccess"));
                    }
                    // Dim fso As IO.File
                    if (!File.Exists(goServer.MapPath(goConfig["ProjectPath"] + "/httpd.ini")))
                    {
                        File.Copy(goServer.MapPath("/ewcommon/setup/rootfiles/httpd.ini"), goServer.MapPath(goConfig["ProjectPath"] + "/httpd.ini"));
                    }
                }

                // delete the setup.ashx file
                if (File.Exists(goServer.MapPath(goConfig["ProjectPath"] + "/setup.ashx")))
                {
                    File.Delete(goServer.MapPath(goConfig["ProjectPath"] + "/setup.ashx"));
                }

                // create all standard Eonic Config files
                // disable if exisits to debug
                if (File.Exists(goServer.MapPath(goConfig["ProjectPath"] + "/web.config")))
                {
                    File.Delete(goServer.MapPath(goConfig["ProjectPath"] + "/web.config"));
                }

                // If Not IO.File.Exists(goServer.MapPath(goConfig("ProjectPath") & "/web.config")) Then
                // TS: Don't check for this we want to overright regardless !

                File.Copy(goServer.MapPath("/ewcommon/setup/rootfiles/web_config.xml"), goServer.MapPath(goConfig["ProjectPath"] + "/web.config"));
                // End If

                if (!File.Exists(goServer.MapPath(goConfig["ProjectPath"] + "/protean.web.config")))
                {
                    File.Copy(goServer.MapPath("/ewcommon/setup/rootfiles/protean_config.xml"), goServer.MapPath(goConfig["ProjectPath"] + "/protean.web.config"));
                }
                if (!File.Exists(goServer.MapPath(goConfig["ProjectPath"] + "/protean.theme.config")))
                {
                    File.Copy(goServer.MapPath("/ewcommon/setup/rootfiles/protean_theme_config.xml"), goServer.MapPath(goConfig["ProjectPath"] + "/protean.theme.config"));
                }
                if (!File.Exists(goServer.MapPath(goConfig["ProjectPath"] + "/protean.cart.config")))
                {
                    File.Copy(goServer.MapPath("/ewcommon/setup/rootfiles/protean_cart_config.xml"), goServer.MapPath(goConfig["ProjectPath"] + "/protean.cart.config"));
                }
                if (!File.Exists(goServer.MapPath(goConfig["ProjectPath"] + "/protean.payment.config")))
                {
                    File.Copy(goServer.MapPath("/ewcommon/setup/rootfiles/protean_payment_config.xml"), goServer.MapPath(goConfig["ProjectPath"] + "/protean.payment.config"));
                }
                // If Not IO.File.Exists(goServer.MapPath(goConfig("ProjectPath") & "/protean.payment.config")) Then
                // IO.File.Copy(goServer.MapPath("/ewcommon/setup/rootfiles/protean_payment_config.xml"),
                // goServer.MapPath(goConfig("ProjectPath") & "/protean.payment.config"))
                // End If
                if (!File.Exists(goServer.MapPath(goConfig["ProjectPath"] + "/protean.mailinglist.config")))
                {
                    File.Copy(goServer.MapPath("/ewcommon/setup/rootfiles/protean_mailinglist_config.xml"), goServer.MapPath(goConfig["ProjectPath"] + "/protean.mailinglist.config"));
                }

                // lets create media, images and docs directories
                if (!Directory.Exists(goServer.MapPath(imageRootPath)))
                {
                    Directory.CreateDirectory(goServer.MapPath(imageRootPath));
                }
                if (!Directory.Exists(goServer.MapPath(docRootPath)))
                {
                    Directory.CreateDirectory(goServer.MapPath(docRootPath));
                }
                if (!Directory.Exists(goServer.MapPath(docMediaPath)))
                {
                    Directory.CreateDirectory(goServer.MapPath(docMediaPath));
                }
                // If Not IO.Directory.Exists(goServer.MapPath("/ewThemes")) Then
                // IO.Directory.CreateDirectory(goServer.MapPath("/ewThemes"))
                // End If
                // 'now unzip a standard theme
                // If Not IO.File.Exists(goServer.MapPath(goConfig("ProjectPath") & "/ewThemes/mono.zip")) Then
                // IO.File.Copy(goServer.MapPath("/ewcommon/setup/rootfiles/ewThemes/mono.zip"), _
                // goServer.MapPath(goConfig("ProjectPath") & "/ewThemes/mono.zip"))
                // End If

                // If Not IO.File.Exists(goServer.MapPath(goConfig("ProjectPath") & "/ewThemes/mono/standard.xsl")) Then
                // Try
                // Dim fz As New ICSharpCode.SharpZipLib.Zip.FastZip
                // fz.ExtractZip(goServer.MapPath(goConfig("ProjectPath") & "/ewThemes/mono.zip"), goServer.MapPath(goConfig("ProjectPath") & "/ewThemes/"), "")
                // Catch ex As Exception
                // 'do nothing
                // errMsg = "Mono Theme did not extract"
                // End Try
                // End If

                // End If
                // oImp.UndoImpersonation()
                return true;
            }

            catch (Exception ex)
            {
                // oSetup.AddResponseError(ex)
                errMsg = ex.InnerException.Message;
                return false;
            }
        }


    }

    public class ContentMigration
    {

        // class for migrating one content schema to another
        public Setup oSetup;
        public Cms myWeb;


        public System.Web.HttpContext moCtx = System.Web.HttpContext.Current;

        public System.Web.HttpApplicationState goApp;
        public System.Web.HttpRequest goRequest;
        public System.Web.HttpResponse goResponse;
        public System.Web.SessionState.HttpSessionState goSession;
        public System.Web.HttpServerUtility goServer;
        public System.Collections.Specialized.NameValueCollection goConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");

        private Protean.XmlHelper.Transform oTransform = new Protean.XmlHelper.Transform();

        private string cUpdateType = "";
        private string cUpdateSchema = "";
        private string cUpdateTableName = "";
        private string cUpdateKeyColumnName = "";
        private string cUpdateSchemaColumnName = "";
        private Protean.Cms.dbHelper.objectTypes nUpdateTableType;

        private Regex rCharCheck = new Regex(@"[\u0000-\u0008\u000B\u000C\u000E-\u001F]");

        public ContentMigration(ref Setup oTheSetup)
        {
            goApp = moCtx.Application;
            goRequest = moCtx.Request;
            goResponse = moCtx.Response;
            goSession = moCtx.Session;
            goServer = moCtx.Server;

            oSetup = oTheSetup;

            try
            {

                myWeb = oSetup.myWeb;

                // Set the type
                if (!string.IsNullOrEmpty(goRequest["upgradetype"]))
                {
                    UpdateType = goRequest["upgradetype"];
                    goSession["upgradetype"] = UpdateType;
                }
                else if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(goSession["upgradetype"], "", false)))
                {
                    UpdateType = Conversions.ToString(goSession["upgradetype"]);
                }
                else
                {
                    // Default to content
                    UpdateType = "Content";
                }
            }
            catch (Exception ex)
            {
                oSetup.AddResponseError(ex);
            }

        }

        private string UpdateType
        {
            get
            {
                return cUpdateType;
            }
            set
            {

                // Set the core value, and all associated configs based around it.
                // General Settings
                if (value == "Content" | value == "Directory" | value == "ContentStructure")
                {
                    cUpdateType = value;
                    nUpdateTableType =(Cms.dbHelper.objectTypes)Enum.Parse(typeof(Cms.dbHelper.objectTypes), value);
                    cUpdateTableName = myWeb.moDbHelper.getTable(nUpdateTableType);
                    cUpdateKeyColumnName = myWeb.moDbHelper.getKey((int)nUpdateTableType);

                    // Type specific settings
                    switch (value ?? "")
                    {

                        case "Content":
                            {

                                cUpdateSchemaColumnName = "cContentSchemaName";
                                break;
                            }

                        case "Directory":
                            {

                                cUpdateSchemaColumnName = "cDirSchema";
                                break;
                            }

                        default:
                            {

                                cUpdateSchemaColumnName = "";
                                break;
                            }

                    }

                }


            }
        }

        public virtual bool GetPage()
        {
            // if a form has been submitted, then we process it
            try
            {
                GetOptions();
                if (goRequest.Form.Count > 0 & !!string.IsNullOrEmpty(goRequest["upgradetype"]))
                {
                    // get the basic variables
                    string cContentName, cMoreSQL;
                    var bSave = default(bool);
                    cContentName = goRequest.Form["ContentName"];
                    cMoreSQL = goRequest.Form["moresql"];
                    if (goRequest.Form["save"] == "1")
                        bSave = true;
                    var oFullFile = goRequest.Files["fullxsl"];
                    // now we need to read the file without saving it anywhere
                    string cFullXSLT = "";
                    int nFileLen;
                    Stream oFStream;
                    byte[] oByteFile;
                    // if there is a brief file then read it (into bytes)
                    if (oFullFile != null)
                    {
                        oSetup.AddResponse("Reading Brief File");
                        nFileLen = oFullFile.ContentLength;
                        oByteFile = new byte[nFileLen + 1];
                        var oBuffer = new byte[nFileLen + 1];
                        oFStream = oFullFile.InputStream;
                        oFStream.Read(oBuffer, 0, nFileLen);
                        int i;
                        var loopTo = nFileLen - 1;
                        for (i = 0; i <= loopTo; i++)
                            oByteFile[i] = oBuffer[i];
                        oFStream.Close();
                        // now make it a string
                        cFullXSLT = ByteToStr(oByteFile);
                    }
                    // now make the updates
                    DoChange(cContentName, cFullXSLT, bSave, cMoreSQL);
                }
                return true;
            }
            catch (Exception ex)
            {
                oSetup.AddResponseError(ex);
                // returnException(myWeb.msException, mcModuleName, "GetPage", ex, "", "", gbDebug)
                return false;
            }
        }
        public virtual bool DoChange(string cContentType, string cXSLLocation, bool bForReal, string cAdditionalWhere = "")
        {
            // input: Content type (cContentSchemaName), location for the brief xsl, location for the detail xsl,bforreal to check wether to save the changes rather than a test run, additional sql where clause to filter, stop on errors or leave the content as is
            var oDS = new DataSet();
            string cSQL = "";
            string cProcessInfo = "Upgrading content";
            int nRowCount;
            bool bCharCheck = false;
            bool bShowXml = false;
            bool bAllowLog = false;
            try
            {

                cProcessInfo = "Upgrading " + UpdateType;

                if (goRequest["illegalChars"] == "1")
                    bCharCheck = true;

                if (goRequest["showXML"] == "1")
                    bShowXml = true;

                if (goRequest["showXML"] == "1" & bForReal == false)
                {
                    // setup the sql string
                    cSQL = "SELECT TOP 50 * FROM " + cUpdateTableName + " INNER JOIN tblAudit on nauditid = nauditkey ";
                }
                else
                {
                    // setup the sql string
                    cSQL = "SELECT * FROM " + cUpdateTableName + " INNER JOIN tblAudit on nauditid = nauditkey ";
                }

                if (!string.IsNullOrEmpty(cUpdateSchemaColumnName))
                    cSQL += " WHERE (" + cUpdateSchemaColumnName + "= '" + cContentType + "')";

                if (!string.IsNullOrEmpty(Strings.Trim(cAdditionalWhere)))
                {
                    cSQL = Conversions.ToString(cSQL + Operators.ConcatenateObject(Interaction.IIf(cSQL.Contains(" WHERE "), " AND ", " WHERE "), cAdditionalWhere));
                }

                // get dataset
                oDS = myWeb.moDbHelper.GetDataSet(cSQL, UpdateType, "MigrateContent");
                nRowCount = oDS.Tables[UpdateType].Rows.Count;

                string cProgressDetail = "";
                int nProgress = 0;
                int nLogId = 0;
                string cCurrentId = "";

                // Create a log for tracking
                bAllowLog = myWeb.moDbHelper.checkDBObjectExists("tblActivityLog", Tools.Database.objectTypes.Table);
                if (bForReal & bAllowLog)
                {
                    nLogId = oSetup.CommitToLog(Cms.dbHelper.ActivityType.SetupDataUpgrade, oSetup.mnUserId, goSession.SessionID, DateTime.Now, cDetail: nProgress + "/" + nRowCount);
                }

                oSetup.AddResponse("Total Record to Process: " + nRowCount);

                // go through each row and update the content
                foreach (DataRow oDR in oDS.Tables[UpdateType].Rows)
                {
                    // dont bother if there is no xsl file for the brief
                    // Get the id
                    long nId = Conversions.ToLong(oDR[cUpdateKeyColumnName]);
                    string cResponse = myWeb.moDbHelper.getObjectInstance(nUpdateTableType, nId);
                    if (Convert.ToInt32(nUpdateTableType) == 4) // Directory
                    {
                        XmlElement oGrpElmt;
                        oGrpElmt = myWeb.moDbHelper.getGroupsInstance(nId, 0);
                        cResponse = "<instance>" + cResponse + oGrpElmt.OuterXml + "</instance>";
                    }
                    else
                    {

                        cResponse = "<instance>" + cResponse + "</instance>";
                    }


                    // DataRowToInstance(oDR, Me.cUpdateTableName)
                    if (bCharCheck)
                    {
                        var oInstance = upgradeContentSchemas(cXSLLocation, cResponse, true);
                    }
                    else
                    {
                        cCurrentId = oDR[cUpdateKeyColumnName].ToString();
                        oSetup.AddResponse("Processing ID: " + cCurrentId);

                        if (bShowXml)
                        {
                            oSetup.AddResponse("Original:");
                            oSetup.AddResponse("<code>" + cResponse.Replace("<", "&lt;").Replace(">", "&gt;") + "</code><!--" + cResponse + "-->");
                        }

                        var oInstance = upgradeContentSchemas(cXSLLocation, cResponse);

                        if (bShowXml)
                        {
                            oSetup.AddResponse("New:");
                            oSetup.AddResponse("<code>" + oInstance.OuterXml.Replace("<", "&lt;").Replace(">", "&gt;") + "</code><!--" + oInstance.OuterXml + "-->");
                        }

                        if (bForReal)
                        {
                            // diable version control for this
                            myWeb.gbVersionControl = false;
                            myWeb.moDbHelper.setObjectInstance(nUpdateTableType, oInstance, Conversions.ToLong(cCurrentId));
                            if (Conversions.ToLong(cCurrentId) > 0L)
                            {
                                myWeb.moDbHelper.processInstanceExtras(Conversions.ToLong(cCurrentId), oInstance, false, false);
                            }
                            nProgress += 1;
                            if (nProgress == nRowCount | nProgress % 20 == 0 & bAllowLog)
                            {
                                cProgressDetail = nProgress + "/" + nRowCount;
                                if (nProgress == nRowCount)
                                    cProgressDetail += ";" + Strings.Format(DateTime.Now, "yyyy-MM-dd HH:mm:ss");
                                oSetup.UpdateLogDetail(nLogId, cProgressDetail);
                            }
                        }
                    }
                    if (myWeb.msException != default)
                    {
                        oSetup.AddResponse("Error Converting Original:");
                        oSetup.AddResponse("<code>" + cResponse.Replace("<", "&lt;").Replace(">", "&gt;") + "</code><!--" + cResponse + "-->");
                        oSetup.AddResponse(myWeb.msException);
                        break;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                oSetup.AddResponseError(ex); // returnException(myWeb.msException, mcModuleName, "DoChange", ex, "", cProcessInfo, gbDebug)
                return false;
            }
        }

        public string DataRowToInstance(ref DataRow oRow, string cTableName)
        {
            var oXML = new XmlDocument();
            XmlElement oInst;
            try
            {
                oXML.AppendChild(oXML.CreateElement("instance"));
                oInst = oXML.CreateElement(cTableName);
                oXML.DocumentElement.AppendChild(oInst);
                foreach (DataColumn oDC in oRow.Table.Columns)
                {
                    var oElmt = oXML.CreateElement(oDC.ColumnName);
                    if (!(oRow[oDC.ColumnName] is DBNull))
                    {
                        oElmt.InnerXml = Conversions.ToString(oRow[oDC.ColumnName]);
                        oInst.AppendChild(oElmt);
                    }
                }
                return oXML.OuterXml;
            }
            catch (Exception ex)
            {
                oSetup.AddResponseError(ex);
                return null;
            }
        }

        public XmlElement upgradeContentSchemas(string cContentSchema, string cContentXml, bool bCharCheck = false)
        {

            StringWriter sWriter;
            string sResult;
            var oXmlDr = new XmlDocument();
            string cResponse;
            string cProcessInfo = "error converting type:" + cContentSchema + " data:" + cContentXml;
            var oXML = new XmlDocument();
            var oTransform2 = new Protean.XmlHelper.Transform(ref myWeb, "", false);
            try
            {
                // RJP 7 Nov 2012. Modified call EncodeForXML.
                // TS not here this is allready valid XML - this need to be in encrypt
                // cContentXml = Protean.Tools.Xml.EncodeForXml(cContentXml)
                // cContentXml = Protean.Tools.Xml.convertEntitiesToCodes(cContentXml)

                oXmlDr.LoadXml(cContentXml.Trim());

                // set exception to nothing
                myWeb.msException = null;

                sWriter = new StringWriter();
                oTransform2.Compiled = false;
                oTransform2.XSLFileIsPath = false;
                oTransform2.XSLFile = cContentSchema.Trim();
                TextWriter argoWriter = sWriter;
                oTransform2.Process(oXmlDr, ref argoWriter);
                sWriter = (StringWriter)argoWriter;
                sResult = sWriter.ToString();

                sWriter = null;
                oXmlDr = null;

                if (oTransform2.transformException is null)
                {
                    cResponse = Tools.Xml.convertEntitiesToCodes(sResult);

                    if (bCharCheck)
                    {
                        if (rCharCheck.Matches(cResponse).Count > 0)
                        {
                            oSetup.AddResponse("Illegal Characters found:");
                            oSetup.AddResponse(cResponse);
                        }
                    }
                    else
                    {
                        // Get rid of illegal XML chars
                        oXML.InnerXml = rCharCheck.Replace(cResponse, "");
                    }
                }
                else
                {
                    if (oTransform2.transformException.InnerException is null)
                    {
                        oXML.InnerXml = "<Error>" + Tools.Xml.convertEntitiesToCodes(oTransform2.transformException.Message) + "</Error>";
                    }
                    else
                    {
                        oXML.InnerXml = "<Error>" + Tools.Xml.convertEntitiesToCodes(oTransform2.transformException.Message) + " " + Tools.Xml.convertEntitiesToCodes(oTransform2.transformException.InnerException.Message) + "</Error>";
                    }
                    oSetup.AddResponseError(oTransform2.transformException);
                }

                return oXML.DocumentElement;
            }

            catch (XmlException exxml)
            {
                oSetup.AddResponseError(exxml); // returnException(myWeb.msException, mcModuleName, "upgradeContentSchemas", exxml, "", cProcessInfo, gbDebug)
                return oXML.DocumentElement;
            }
            catch (Exception ex)
            {
                oSetup.AddResponseError(ex); // cError = ex.ToString
                return oXML.DocumentElement;
            }

        }

        public virtual bool GetOptions()
        {

            try
            {
                XmlElement oContentDetail = (XmlElement)oSetup.moPageXml.FirstChild.SelectSingleNode("ContentDetail");
                if (oContentDetail is null)
                {
                    oContentDetail = oSetup.moPageXml.CreateElement("ContentDetail");
                    oSetup.moPageXml.FirstChild.AppendChild(oContentDetail);
                }
                var oElmt = oSetup.moPageXml.CreateElement("Content");
                oElmt.SetAttribute("upgradetype", UpdateType);
                oElmt.InnerXml = "<type>Content</type><type>Directory</type><type>ContentStructure</type>";
                oContentDetail.AppendChild(oElmt);
                if (!string.IsNullOrEmpty(cUpdateSchemaColumnName))
                {
                    string cOptions = "";
                    // Dim oDR As SqlDataReader = myWeb.moDbHelper.getDataReader("SELECT " & Me.cUpdateSchemaColumnName & " FROM " & Me.cUpdateTableName & " GROUP BY " & Me.cUpdateSchemaColumnName & " ORDER BY " & Me.cUpdateSchemaColumnName)
                    using (SqlDataReader oDr = myWeb.moDbHelper.getDataReaderDisposable("SELECT " + cUpdateSchemaColumnName + " FROM " + cUpdateTableName + " GROUP BY " + cUpdateSchemaColumnName + " ORDER BY " + cUpdateSchemaColumnName))  // Done by nita on 6/7/22
                    {
                        while (oDr.Read())
                            cOptions = Conversions.ToString(cOptions + Operators.ConcatenateObject(Operators.ConcatenateObject("<option>", oDr.GetValue(0)), "</option>"));
                        oDr.Close();
                    }
                    oElmt.InnerXml += Strings.Replace(Strings.Replace(cOptions, "&gt;", ">"), "&lt;", "<");
                }


                return true;
            }
            catch (Exception ex)
            {
                oSetup.AddResponseError(ex);
                return false;
                // returnException(myWeb.msException, mcModuleName, "BuildForm", ex, "", "", gbDebug)
            }
        }

        public string ByteToStr(byte[] oBytes)
        {
            // returns a string from bytes
            // to represent the file
            oSetup.AddResponse("Converting Bytes to String");

            char[] oChars; // array of characters

            var oDecoder = System.Text.Encoding.UTF8.GetDecoder(); // a decoder
            string cResult = ""; // the final string
            try
            {
                oChars = new char[oDecoder.GetCharCount(oBytes, 0, Information.UBound(oBytes)) + 1]; // make the character array length the same as what will be outputted
                oDecoder.GetChars(oBytes, 0, Information.UBound(oBytes), oChars, 0); // turn the bytes into chars

                cResult = string.Concat(Conversions.ToString(oChars));

                // For nI = 0 To UBound(oChars) - 1 'read the chars into a string
                // cResult &= oChars(nI)
                // Next
                return cResult;
            }
            catch (Exception ex)
            {
                oSetup.AddResponseError(ex); // returnException(myWeb.msException, mcModuleName, "ByteToStr", ex, "", "ByteToStr", gbDebug)
                return "";

            }
        }

    }

    public class ContentImport : ContentMigration
    {

        public ContentImport(Setup objSetup) : base(ref objSetup)
        {
        }

        public override bool GetOptions()
        {
            try
            {

                XmlElement oContentDetail = (XmlElement)oSetup.moPageXml.FirstChild.SelectSingleNode("ContentDetail");
                if (oContentDetail is null)
                {
                    oContentDetail = oSetup.moPageXml.CreateElement("ContentDetail");
                    oSetup.moPageXml.FirstChild.AppendChild(oContentDetail);
                }
                XmlElement oOpt;
                var oElmt = oSetup.moPageXml.CreateElement("Content");
                oContentDetail.AppendChild(oElmt);

                // Dim oDR As SqlDataReader = myWeb.moDbHelper.getDataReader("SELECT cContentSchemaName FROM tblContent GROUP BY cContentSchemaName ORDER BY cContentSchemaName")
                using (SqlDataReader oDr = myWeb.moDbHelper.getDataReaderDisposable("SELECT cContentSchemaName FROM tblContent GROUP BY cContentSchemaName ORDER BY cContentSchemaName"))  // Done by nita on 6/7/22
                {
                    while (oDr.Read())
                    {
                        oOpt = oSetup.moPageXml.CreateElement("option");
                        oOpt.SetAttribute("class", "schema");
                        oOpt.InnerText = Conversions.ToString(oDr.GetValue(0));
                        oElmt.AppendChild(oOpt);
                    }
                    oDr.Close();
                }
                // oDr = myWeb.moDbHelper.getDataReader("SELECT nStructKey, cStructName FROM tblContentStructure ORDER BY cStructName")
                using (SqlDataReader oDr = myWeb.moDbHelper.getDataReaderDisposable("SELECT nStructKey, cStructName FROM tblContentStructure ORDER BY cStructName"))  // Done by nita on 6/7/22
                {
                    while (oDr.Read())
                    {
                        oOpt = oSetup.moPageXml.CreateElement("option");
                        oOpt.SetAttribute("class", "page");
                        oOpt.SetAttribute("value", Conversions.ToString(oDr.GetValue(0)));
                        oOpt.InnerText = Conversions.ToString(oDr.GetValue(1));
                        oElmt.AppendChild(oOpt);
                    }
                    oDr.Close();
                }

                return true;
            }

            catch (Exception ex)
            {
                oSetup.AddResponseError(ex); // returnException(myWeb.msException, mcModuleName, "BuildForm", ex, "", "", gbDebug)
                return false;
            }
        }

        public override bool GetPage()
        {
            // if a form has been submitted, then we process it
            try
            {
                GetOptions();
                if (goRequest.Form.Count > 0)
                {
                    // get the basic variables
                    int nContentID;
                    string cContentType;
                    nContentID = Conversions.ToInteger(goRequest.Form["ContentId"]);
                    cContentType = goRequest.Form["Contenttype"];
                    var oInstanceXml = goRequest.Files["instancexml"];
                    // now we need to read the file without saving it anywhere
                    string cIXML = "";
                    int nFileLen;
                    Stream oFStream;
                    byte[] oByteFile;
                    // if there is a brief file then read it (into bytes)
                    if (oInstanceXml != null)
                    {
                        nFileLen = oInstanceXml.ContentLength;
                        oByteFile = new byte[nFileLen + 1];
                        var oBuffer = new byte[nFileLen + 1];
                        oFStream = oInstanceXml.InputStream;
                        oFStream.Read(oBuffer, 0, nFileLen);
                        int i;
                        var loopTo = nFileLen - 1;
                        for (i = 0; i <= loopTo; i++)
                            oByteFile[i] = oBuffer[i];
                        oFStream.Close();
                        // now make it a string
                        oSetup.AddResponse("Loaded Settings and File");
                        cIXML = ByteToStr(oByteFile);
                        oByteFile = null;

                    }

                    // now make the updates
                    return DoChange(nContentID, cContentType, cIXML);
                }
                else
                {
                    return true;
                }
            }

            catch (Exception ex)
            {
                oSetup.AddResponseError(ex); // returnException(myWeb.msException, mcModuleName, "GetPage", ex, "", "", gbDebug)
                return false;
            }
        }

        public bool DoChange(int nContentID, string cContentType, string cContent)
        {
            try
            {
                Protean.xForm oXForm;

                string cXFormPath = "/xforms/Content/" + cContentType + ".xml";
                if (!File.Exists(goRequest.MapPath(cXFormPath)))
                {
                    cXFormPath = "/ewcommon" + cXFormPath;
                }


                var oInstances = new XmlDocument();
                oInstances.PreserveWhitespace = false;

                cContent = Strings.Replace(cContent, Conversions.ToString('\r'), ""); // Remove carriage return
                cContent = Strings.Replace(cContent, Conversions.ToString('\n'), ""); // remove tab
                cContent = Strings.Replace(cContent, "  ", " "); // remove double space
                cContent = Strings.Replace(cContent, "  ", " "); // remove double space
                cContent = Strings.Replace(cContent, "  ", " "); // remove double space
                cContent = Strings.Replace(cContent, "  ", " "); // remove double space
                cContent = Strings.Replace(cContent, "  ", " "); // remove double space
                cContent = Strings.Replace(cContent, "  ", " "); // remove double space
                cContent = Strings.Replace(cContent, "> <", "><"); // clean xml
                oInstances.InnerXml = Strings.Trim(cContent);
                oInstances.PreserveWhitespace = false;

                foreach (XmlElement oElmt in oInstances.DocumentElement.SelectNodes("instance/tblContent"))
                {
                    oXForm = new Protean.xForm(ref myWeb.msException);
                    oXForm.load(cXFormPath);
                    oXForm.LoadInstance(oElmt);
                    int nNewContentId = 0;
                    nNewContentId =Convert.ToInt32(myWeb.moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Content, oXForm.Instance));
                    myWeb.moDbHelper.setContentLocation(nContentID, nNewContentId, true, false);
                    oSetup.AddResponse("Imported Content, New ID: " + nNewContentId);
                }
                oSetup.AddResponse("Complete");
                return true;
            }
            catch (Exception ex)
            {
                oSetup.AddResponseError(ex);
                return false; // returnException(myWeb.msException, mcModuleName, "GetPage", ex, "", "", gbDebug)
            }
        }

    }
}