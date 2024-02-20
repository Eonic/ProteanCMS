using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using static System.Web.HttpUtility;

using System.Xml;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using static Protean.stdTools;
using static Protean.Tools.Xml;
using Protean.Providers.Membership;

namespace Protean
{

    public partial class Cms : Protean.Base, IDisposable
    {

        #region ErrorHandling

        // for anything controlling web
        public new event OnErrorEventHandler OnError;

        public new delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);

        protected override void OnComponentError(object sender, Tools.Errors.ErrorEventArgs e)
        {
            // deals with the error
            returnException(ref msException, e.ModuleName, e.ProcedureName, e.Exception, mcEwSiteXsl, e.AddtionalInformation, gbDebug);
            // close connection poolinguseralerts
            if (this.moDbHelper != null)
            {
                try
                {
                    this.moDbHelper.CloseConnection();
                }
                catch (Exception)
                {
                }
            }
            // then raises a public event
            OnError?.Invoke(sender, e);
        }

        #endregion

        #region Declarations
        private Protean.Base.licenceMode moLicenceMode = Protean.Base.licenceMode.Live;

        private bool mbSystemPage = false;
        private Cms.dbHelper.PermissionLevel mnUserPagePermission = Cms.dbHelper.PermissionLevel.Open;


        public bool mbPopupMode = false;

        public XmlDocument moPageXml = new XmlDocument();
        //private XmlDocument moXmlAddedContent;
        public DateTime? mdPageExpireDate = DateAndTime.DateAdd(DateInterval.Year, 1d, DateTime.Now);
        public DateTime? mdPageUpdateDate = DateAndTime.DateAdd(DateInterval.Year, -1, DateTime.Now);
        private int mnPageCacheMode;
        public XmlElement moContentDetail;
        public string mcContentType = System.Net.Mime.MediaTypeNames.Text.Html;
        public string mcContentDisposition = "";
        public long mnProteanCMSError = 0L;


        public string msException = "";

        // Clone Page Info
        public int mnClonePageId = 0;
        public int mnClonePageVersionId = 0;
        public bool mbIsClonePage = false;
        public int mnCloneContextPageId = 0;

        public string mcPageLanguage = "";
        public string mcPreferredLanguage = "";
        public string mcPageLanguageUrlPrefix = "";
        public string mcPageDefaultDomain = "";

        // Quizmaker Time Info.
        public DateTime mdProcessStartTime = DateTime.Now;
        public DateTime mdDate = DateTime.Now.Date; // the generic date for selecting content by publish/expiry date

        public long mnMailMenuId = 0L;
        public long mnSystemPagesId = 0L;
        // BJR For Indexing
        public bool ibIndexMode = false;
        public bool ibIndexRelatedContent = false;
        public StringWriter icPageWriter;

        private string gcEwSiteXsl;

        public static string gcMenuContentCountTypes;
        public static string gcMenuContentBriefTypes;
        public static string gcEwBaseUrl;
        public static string gcBlockContentType = "";
        public static bool gbMembership = false;
        public static bool gbCart = false;
        public static bool gbQuote = false;
        public static bool gbReport = false;
        public static int gnTopLevel = 0;
        public static int gnNonAuthUsers = 0;
        public static int gnAuthUsers = 0;
        public bool bs3 = false;
        public bool bs5 = false;

        public static bool gbClone = false;
        public static bool gbMemberCodes = false;
        public bool gbVersionControl = false;
        public static bool gbIPLogging = false;
        public static string gcGenerator = "";
        public static string gcCodebase = "";
        public static string gcReferencedAssemblies = "";
        public static bool gbUseLanguageStylesheets = false;
        public static string gcProjectPath = "";
        public static bool gbUserIntegrations = false;
        public static bool gbSingleLoginSessionPerUser = false;
        public static short gnSingleLoginSessionTimeout = 900;
        // Site cache
        public static bool gbSiteCacheMode = false;

        public int sessionRootPageId = 0;

        public static bool gbCompiledTransform = false;
        private static System.Xml.Xsl.XslCompiledTransform moCompliedStyle;
        public long gnPageNotFoundId = 0L;
        private long gnPageAccessDeniedId = 0L;
        private long gnPageLoginRequiredId = 0L;
        private long gnPageErrorId = 0L;
        private long gnResponseCode = 200L;
        public string msRedirectOnEnd = "";
        public string mbRedirectPerm = "false";
        public bool bRedirectStarted = false;
        public string mcOriginalURL = "";
        public string mcPageURL = "";
        public bool mbSuppressLastPageOverrides = false;
        public bool mbIgnorePath = false;
        public string gcLang = "en-gb";
        public string mcRequestDomain = "";
        public bool bAllowExpired = false;

        private string mcLOCALEwSiteXsl = "";

        public string mcBehaviourNewContentOrder = "";
        public string mcBehaviourAddPageCommand = "";
        public string mcBehaviourEditPageCommand = "";


        public string mcClientCommonFolder = "";
        public string mcEWCommonFolder = "/ewcommon";
        public string[] maCommonFolders = Array.Empty<string>();

        public new string mcModuleName = "Protean.Cms";

        public IMembershipProvider moMemProv;

        public Cms.Cart moCart;
        public Cms.Cart.Discount moDiscount;
        public bool mbIsUsingHTTPS = false;
        public Protean.XmlHelper.Transform moTransform;

        public Cms.Admin moAdmin;
        protected internal Cms.Cart oEc; // Used for BuildFeedXML for some reason, perhpas this could use moCart needs testing
        protected Cms.Search oSrch;
        protected internal Protean.fsHelper moFSHelper;

        private XmlElement _responses;

        private Protean.ExternalSynchronisation _oSync;

        public virtual Protean.ExternalSynchronisation oSync
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _oSync;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_oSync != null)
                {
                    _oSync.OnError -= OnComponentError;
                }

                _oSync = value;
                if (_oSync != null)
                {
                    _oSync.OnError += OnComponentError;
                }
            }
        }
        private Cms.Calendar _moCalendar;

        public virtual Cms.Calendar moCalendar
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _moCalendar;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_moCalendar != null)
                {
                    _moCalendar.OnError -= OnComponentError;
                }

                _moCalendar = value;
                if (_moCalendar != null)
                {
                    _moCalendar.OnError += OnComponentError;
                }
            }
        }

        public Protean.xForm oXform;
        public int gnShowRelatedBriefDepth = 2;
        public bool bRestartApp = false;
        public pageResponseType moResponseType;
        public bool bPageCache = false;
        public bool mbSetNoBrowserCache = false;
        public string mcPageCacheFolder = @"\ewCache";
        private string mcSessionReferrer = null;


        private PerformanceCounter _workingSetPrivateMemoryCounter;
        public string mcOutputFileName = "FileName.pdf";

        private const string NotFoundPagePath = "/System-Pages/Page-Not-Found";
        private const string AccessDeniedPagePath = "/System-Pages/Access-Denied";
        private const string LoginRequiredPagePath = "/System-Pages/Login-Required";
        private const string ProteanErrorPagePath = "/System-Pages/Protean-Error";

        public object impersonationMode = false;

        #endregion
        #region Enums
        public enum pageResponseType
        {
            Page = 0,
            xml = 1,
            json = 2,
            popup = 3,
            ajaxadmin = 4,
            mail = 5,
            iframe = 6,
            pdf = 7,
            flush = 8,
            qrcode = 9
        }

        #endregion

        #region Constructors

        public Cms() : this(System.Web.HttpContext.Current)
        {

        }

        public Cms(System.Web.HttpContext Context) : base(Context)
        {
            string sProcessInfo = "";
            try
            {


                moFSHelper = new Protean.fsHelper(this.moCtx);
                InitialiseGlobal();
                if (this.moCtx != null)
                {
                    this.PerfMon.Log("Web", "New");
                }

                if (this.moDbHelper is null)
                {
                    this.moDbHelper = (Cms.dbHelper)GetDbHelper();
                }
            }
            // Open()
            // 
            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "New", ex, "", sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, sProcessInfo));
            }
        }

        #endregion



        public string mcEwSiteXsl
        {
            set
            {
                mcLOCALEwSiteXsl = value;
            }
            get
            {
                if (string.IsNullOrEmpty(mcLOCALEwSiteXsl))
                {
                    return gcEwSiteXsl;
                }
                else
                {
                    return mcLOCALEwSiteXsl;
                }
            }
        }

        /// <summary>
        /// Sets or gets the root page id.
        /// This contains a stop-gap measure to transition the root page from
        /// an application context (incorrect) to a session context.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public int RootPageId
        {
            get
            {
                return sessionRootPageId;
            }
            set
            {
                sessionRootPageId = value;
                gnTopLevel = value;
            }
        }

        public virtual object GetDbHelper()
        {

            var argaWeb = this;
            return new Cms.dbHelper(ref argaWeb);

        }

        private bool CheckLicence(Protean.Base.licenceMode mode)
        {
            string sProcessInfo = "";
            try
            {

                return true;
            }

            // Dim EncyrptionKey As String = "1ad54c70-5fa7-44c5-8059-ab68e136cc63"
            // Dim Hostname As String
            // Dim Enc As New Eonic.Encryption
            // Dim ServerIP As String

            // If moRequest Is Nothing Then
            // Hostname = "127.0.0.1"
            // ServerIP = "127.0.0.1"
            // Else
            // Hostname = moRequest.ServerVariables("SERVER_NAME")
            // ServerIP = moRequest.ServerVariables("LOCAL_ADDR")
            // End If

            // Dim Hash As String = Enc.GenerateMD5Hash(EncyrptionKey & Hostname)

            // If goApp Is Nothing Then
            // Return True
            // Else
            // If goApp(Hash) Is Nothing Then

            // Dim Valid As Boolean = False

            // Select Case mode
            // Case licenceMode.Demo, licenceMode.Development
            // 'Runs on local host
            // If ServerIP = "::1" Or ServerIP = "127.0.0.1" Then
            // Valid = True
            // End If
            // If Hostname.EndsWith("eonicweb.dev") Or Hostname.EndsWith("ds01.eonic.co.uk") Then
            // Valid = True
            // End If

            // Case licenceMode.Live
            // 'This is where we put the clever licencing stuff later on.
            // Valid = True

            // End Select

            // If Valid = True Then
            // goApp(Hash) = True
            // Return True
            // Else
            // goApp(Hash) = Nothing
            // Return False
            // End If

            // Else
            // Return True
            // End If
            // End If

            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "CheckLicence", ex, sProcessInfo));
            }

            return default;

        }

        public XmlDocument GetStatus()
        {
            var oDb = new Tools.Database();
            System.Collections.Specialized.NameValueCollection oVConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/versioncontrol");
            var oRXML = new XmlDocument();
            var oResponseElmt = oRXML.CreateElement("Status");
            var bResult = default(bool);
            try
            {
                oRXML.AppendChild(oResponseElmt);
                string sSql;
                var oElmt = oRXML.CreateElement("Debug");
                oElmt.InnerText = this.moConfig["debug"];
                oResponseElmt.AppendChild(oElmt);


                var compileElmt = oRXML.CreateElement("CompiledTransform");
                compileElmt.InnerText = this.moConfig["CompiledTransform"];
                oResponseElmt.AppendChild(compileElmt);

                var pageCacheElmt = oRXML.CreateElement("PageCache");
                pageCacheElmt.InnerText = this.moConfig["PageCache"];
                oResponseElmt.AppendChild(pageCacheElmt);

                // 'Start URI
                // oElmt = oRXML.CreateElement("AbsoluteURI")
                // oElmt.InnerText = System.Web.HttpContext.Current.Request.Url.AbsoluteUri
                // oResponseElmt.AppendChild(oElmt)
                // 'End URI
                // Dim oDr As System.Data.SqlClient.SqlDataReader
                oElmt = oRXML.CreateElement("DBVersion");
                try
                {

                    string sResult = "";
                    sSql = "select * from tblSchemaVersion";
                    using (var oDr = this.moDbHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {
                        if (oDr.HasRows)
                        {
                            while (oDr.Read())
                                sResult = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(oDr[1], "."), oDr[2]), "."), oDr[3]), "."), oDr[4]));
                        }
                        else
                        {
                            sResult = "nodata";
                        }
                        oElmt.InnerText = sResult;
                    }
                }
                catch (Exception)
                {
                    if (string.IsNullOrEmpty(this.moConfig["VersionNumber"]))
                    {
                        oElmt.InnerText = "Pre V4 or no ProteanCMS";
                    }
                    else
                    {
                        oElmt.InnerText = this.moConfig["VersionNumber"];
                    }
                }

                oResponseElmt.AppendChild(oElmt);

                // Get Page Count

                sSql = "select count(cs.nStructKey) from tblContentStructure cs inner join tblAudit a on a.nAuditKey = cs.nAuditId where nStatus = 1 and (cUrl = null or cUrl = '')";
                using (var oDr = this.moDbHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                {
                    if (oDr.HasRows)
                    {
                        while (oDr.Read())
                            oResponseElmt.SetAttribute("activePageCount", Conversions.ToString(oDr[0]));
                    }
                }
                sSql = "select count(nStructKey) from tblContentStructure where cUrl = null or cUrl = ''";
                using (var oDr = this.moDbHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                {
                    if (oDr.HasRows)
                    {
                        while (oDr.Read())
                            oResponseElmt.SetAttribute("totalPageCount", Conversions.ToString(oDr[0]));
                    }
                }
                sSql = "select count(nStructKey) from tblContentStructure";
                using (var oDr = this.moDbHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                {
                    if (oDr.HasRows)
                    {
                        while (oDr.Read())
                            oResponseElmt.SetAttribute("totalPageRedirects", Conversions.ToString(Operators.SubtractObject(oDr[0], Conversions.ToInteger("0" + oResponseElmt.GetAttribute("totalPageCount")))));
                    }
                }

                // Get Content Count
                sSql = "select count(nContentKey) from tblContent";
                using (var oDr = this.moDbHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                {
                    if (oDr.HasRows)
                    {
                        while (oDr.Read())
                            oResponseElmt.SetAttribute("contentCount", Conversions.ToString(oDr[0]));
                    }
                }

                // Start DotNetversion
                oElmt = oRXML.CreateElement("DotNetVersion");
                oElmt.InnerText = Environment.Version.ToString();
                oResponseElmt.AppendChild(oElmt);
                // End DotNetversion

                // Start LocalOrCommonBin
                oElmt = oRXML.CreateElement("LocalOrCommonBin");
                if (!string.IsNullOrEmpty(FileSystem.Dir(this.goServer.MapPath("/default.ashx"))))
                {
                    oElmt.InnerText = "Local";
                }
                else if (!string.IsNullOrEmpty(FileSystem.Dir(this.goServer.MapPath("/ewcommon/default.ashx"))))
                {
                    oElmt.InnerText = "Common";
                }
                else
                {
                    oElmt.InnerText = "Error";
                }
                oResponseElmt.AppendChild(oElmt);
                // End LocalOrCommonBin


                // Start DLL Version

                oElmt = oRXML.CreateElement("DLLVersion");

                // Dim sCodeBase As String = myWeb.moRequest.ServerVariables("GENERATOR")
                // oElmt.InnerText = sCodeBase

                // oElmt.InnerText = myWeb.Generator().FullName()

                oElmt.InnerText = gcGenerator.ToString();

                // Dim CodeGenerator As Assembly = Me.Generator()
                // gcGenerator = CodeGenerator.FullName()

                oResponseElmt.AppendChild(oElmt);

                // End DLL Version


                // Start LatestDBVersion
                oElmt = oRXML.CreateElement("LatestDBVersion");
                XmlTextReader LatestDBVersion;
                string dbUpgradeFile = "/ewcommon/sqlUpdate/DatabaseUpgrade.xml";
                if (bs5)
                    dbUpgradeFile = "/ptn/update/sql/databaseupgrade.xml";
                LatestDBVersion = new XmlTextReader(this.goServer.MapPath(dbUpgradeFile));
                LatestDBVersion.WhitespaceHandling = WhitespaceHandling.None;
                // Disable whitespace so that it doesn't have to read over whitespaces
                LatestDBVersion.Read();
                LatestDBVersion.Read();
                string LatestDBversionAttribute = LatestDBVersion.GetAttribute("LatestVersion");
                oElmt.InnerText = LatestDBversionAttribute;
                oResponseElmt.AppendChild(oElmt);
                // End LatestDBVersion


                // Start EnabledFeatures

                oElmt = oRXML.CreateElement("Membership");
                oElmt.InnerText = this.moConfig["Membership"];
                oResponseElmt.AppendChild(oElmt);

                oElmt = oRXML.CreateElement("Cart");
                oElmt.InnerText = this.moConfig["Cart"];
                oResponseElmt.AppendChild(oElmt);

                oElmt = oRXML.CreateElement("Quote");
                oElmt.InnerText = this.moConfig["Quote"];
                oResponseElmt.AppendChild(oElmt);

                oElmt = oRXML.CreateElement("Report");
                oElmt.InnerText = this.moConfig["Report"];
                oResponseElmt.AppendChild(oElmt);

                oElmt = oRXML.CreateElement("Subscriptions");
                oElmt.InnerText = this.moConfig["Subscriptions"];
                oResponseElmt.AppendChild(oElmt);

                oElmt = oRXML.CreateElement("MailingList");
                oElmt.InnerText = this.moConfig["MailingList"];
                oResponseElmt.AppendChild(oElmt);

                oElmt = oRXML.CreateElement("Search");
                oElmt.InnerText = this.moConfig["Search"];
                oResponseElmt.AppendChild(oElmt);
            }

            // End Enabledfeatures

            catch (Exception ex)
            {
                bResult = false;
                AddResponse(ex.ToString());
                returnException(ref msException, mcModuleName, "GetPendingContent", ex, bDebug: gbDebug);
            }
            finally
            {
                oResponseElmt.SetAttribute("bResult", Conversions.ToString(bResult));
            }

            return oRXML;

        }


        // Protected Overrides Sub Finalize()ActivityLogging
        // 'PerfMon.Log("Web", "Finalize")
        // close()
        // MyBase.Finalize()
        // End Sub

        public void Open()
        {
            this.PerfMon.Log("Web", "Open");
            string sProcessInfo = "";
            string cCloneContext = "";
            string rootPageIdFromConfig = "";
            try
            {
                // Open DB Connections
                if (this.moDbHelper is null)
                {
                    this.moDbHelper = (Cms.dbHelper)GetDbHelper();
                }
                if (string.IsNullOrEmpty(this.moDbHelper.DatabaseName))
                {
                    // redirect to setup
                    if (bs5)
                    {
                        msRedirectOnEnd = "ptn/setup/default.ashx";
                    }
                    else
                    {
                        msRedirectOnEnd = "ewcommon/setup/default.ashx";
                    }
                }
                else
                {

                    if (this.moSession != null)
                    {
                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(this.moSession["adminMode"], "true", false)))
                        {
                            this.mbAdminMode = true;
                            this.moDbHelper.gbAdminMode = this.mbAdminMode;
                        }
                    }

                    if (!string.IsNullOrEmpty(this.moConfig["AdminAcct"]) & this.moConfig["AdminGroup"] != "AzureWebApp")
                    {
                        impersonationMode = true;
                    }

                    // Get system page ID's for application level
                    if (this.moCtx.Application["PageNotFoundId"] is null)
                    {
                        this.moCtx.Application["PageNotFoundId"] = (object)this.moDbHelper.getPageIdFromPath(NotFoundPagePath, false, false);
                    }
                    if (this.moCtx.Application["PageAccessDeniedId"] is null)
                    {
                        this.moCtx.Application["PageAccessDeniedId"] = (object)this.moDbHelper.getPageIdFromPath(AccessDeniedPagePath, false, false);
                    }
                    if (this.moCtx.Application["PageLoginRequiredId"] is null)
                    {
                        this.moCtx.Application["PageLoginRequiredId"] = (object)this.moDbHelper.getPageIdFromPath(LoginRequiredPagePath, false, false);
                    }
                    if (this.moCtx.Application["PageErrorId"] is null)
                    {
                        this.moCtx.Application["PageErrorId"] = (object)this.moDbHelper.getPageIdFromPath(ProteanErrorPagePath, false, false);
                    }

                    gnPageNotFoundId = Conversions.ToLong(this.moCtx.Application["PageNotFoundId"]);
                    gnPageAccessDeniedId = Conversions.ToLong(this.moCtx.Application["PageAccessDeniedId"]);
                    gnPageLoginRequiredId = Conversions.ToLong(this.moCtx.Application["PageLoginRequiredId"]);
                    gnPageErrorId = Conversions.ToLong(this.moCtx.Application["PageErrorId"]);

                    this.mcPagePath = this.moRequest["path"] + "";
                    this.mcPagePath = this.mcPagePath.Replace("//", "/");

                    JSStart.InitialiseJSEngine();

                    // Get the User ID
                    // if we access base via soap the session is not available
                    if (this.moSession != null)
                    {
                        Cms argmyWeb = this;
                        if (moMemProv == null)
                        {
                            ReturnProvider RetProv = new Protean.Providers.Membership.ReturnProvider();
                            moMemProv = RetProv.Get(ref argmyWeb, this.moConfig["MembershipProvider"]);
                            RetProv = null;
                        }
                        this.mnUserId = Conversions.ToInteger(moMemProv.Activities.GetUserId(ref argmyWeb));       
                    }
                    // We need the userId placed into dbhelper.
                    this.moDbHelper.mnUserId = (long)this.mnUserId;

                    // Initialise the cart
                    if (gbCart | gbQuote)
                    {
                        InitialiseCart();
                        // moCart = New Cart(Me)
                        if (moCart != null)
                        {
                            var argaWeb = this;
                            moDiscount = new Cms.Cart.Discount(ref argaWeb);
                            moDiscount.mcCurrency = moCart.mcCurrency;
                        }
                    }


                    // Facility to allow the generation bespoke errors.
                    if (this.moRequest["ewerror"] != null)
                    {
                        // If moRequest("ewerror") <> "nodebug" Then gbDebug = True Else gbDebug = False
                        throw new Exception(Strings.LCase("errortest:" + this.moRequest["ewerror"]));
                    }


                    // Logon Redirect Facility
                    // once you are logged on this becomes the root 
                    // If in Admin, always defer to the AdminRootPageId
                    // unless you are logging off, then you are going to the user site.
                    // If not Admin, then check if we're logged in
                    if (this.mbAdminMode)
                    {
                        // Admin mode
                        string ewCmd;
                        if (Conversions.ToBoolean(Operators.AndObject(Operators.ConditionalCompareObjectEqual(this.moSession["ewCmd"], "PreviewOn", false), Strings.LCase(this.moRequest["ewCmd"]) == "logoff")))
                        {
                            // case to cater for logoff in preview mode
                            ewCmd = "PreviewOn";
                        }
                        else
                        {
                            ewCmd = Conversions.ToString(Interaction.IIf(string.IsNullOrEmpty(this.moRequest["ewCmd"]), this.moSession["ewCmd"], this.moRequest["ewCmd"]));
                        }


                        if (Conversions.ToLong("0" + this.moConfig["AdminRootPageId"]) > 0L & Strings.LCase(ewCmd) != "logoff")
                        {
                            rootPageIdFromConfig = this.moConfig["AdminRootPageId"];
                        }

                        else if (Conversions.ToLong("0" + this.moConfig["AuthenticatedRootPageId"]) > 0L && this.mnUserId > 0 && !this.moDbHelper.checkUserRole("Administrator"))
                        {
                            // This is to accomodate users in admin who have admin rights revoked and therefore must 
                            // be sent back to the user site, but also are logged in, thus they need to go to the authenticatedpageroot if it exists.
                            rootPageIdFromConfig = this.moConfig["AuthenticatedRootPageId"];
                        }
                    }
                    // Not admin mode
                    else if (this.mnUserId > 0 & Conversions.ToLong("0" + this.moConfig["AuthenticatedRootPageId"]) > 0L)
                    {
                        rootPageIdFromConfig = this.moConfig["AuthenticatedRootPageId"];
                    }

                    // Convert the root ID
                    if (Tools.Number.IsStringNumeric(rootPageIdFromConfig))
                        RootPageId = Convert.ToInt32(rootPageIdFromConfig);


                    GetRequestLanguage();

                    long newPageId = 0L;

                    if (this.mnPageId < 1)
                    {
                        if (!string.IsNullOrEmpty(this.moRequest["pgid"]))
                        {

                            // And we still need to check permissions
                            // strip any non numberic charactors
                            object sPageId = Regex.Replace("0" + this.moRequest["pgid"], @"[^\d]", "");
                            // check not too large for an int
                            int argresult = 0;
                            if (int.TryParse(Conversions.ToString(sPageId), out argresult))
                            {
                                this.mnPageId = Conversions.ToInteger(sPageId);
                            }

                            // specified pgid takes priority
                            if (!this.mbAdminMode)
                            {
                                newPageId = (long)this.moDbHelper.checkPagePermission((long)this.mnPageId);
                            }
                            else if (!this.moDbHelper.checkPageExist((long)this.mnPageId))
                            {
                                // And we still need to check it exists
                                newPageId = gnPageNotFoundId;
                                gnResponseCode = 404L;
                            }
                        }
                        else
                        {
                            // Check for a redirect path
                            if (legacyRedirection())
                                return;

                            if (!(string.IsNullOrEmpty(this.moRequest["path"]) | this.mcPagePath == "/"))
                            {
                                // then pathname
                                long argnPageId = (long)this.mnPageId;
                                long argnArtId = (long)this.mnArtId;
                                this.moDbHelper.getPageAndArticleIdFromPath(ref argnPageId, ref argnArtId, this.mcPagePath);
                                this.mnPageId = (int)argnPageId;
                                this.mnArtId = (int)argnArtId;
                                // mnPageId = moDbHelper.getPageIdFromPath(mcPagePath).ToString
                                if (!this.mbAdminMode)
                                {
                                    newPageId = (long)this.moDbHelper.checkPagePermission((long)this.mnPageId);
                                }
                            }
                            else
                            {
                                // then root
                                this.mnPageId = RootPageId;
                            }
                        }
                    }

                    // don't return anything but error in admi mode
                    if (!string.IsNullOrEmpty(this.mcPagePath) & this.mnPageId == RootPageId & this.mbAdminMode)
                    {
                        // TS Removed because not quite sure what is going on... 
                        // was causing a problem with editing locations.
                        // gnResponseCode = 404
                    }

                    if (newPageId > 0L & newPageId == gnPageLoginRequiredId & this.mnUserId == 0)
                    {
                        // we should set an pageId to overide the logon redirect
                        this.moSession["LogonRedirectId"] = (object)this.mnPageId;

                    }


                    if (newPageId > 0L)
                        this.mnPageId = (int)newPageId;

                    // Check for cloned pages, and clone contexts
                    if (gbClone)
                    {
                        mnClonePageId = this.moDbHelper.getClonePageID(this.mnPageId);
                        if (mnClonePageId > 0)
                            mbIsClonePage = true;
                        cCloneContext = this.moRequest["context"];
                        if (Information.IsNumeric(cCloneContext) && Convert.ToInt32(cCloneContext) > 0)
                        {
                            mnCloneContextPageId = Convert.ToInt32(cCloneContext);
                        }
                    }

                    if (!this.mbAdminMode)
                    {

                        if ((long)this.mnPageId == gnPageNotFoundId | (long)this.mnPageId == gnPageAccessDeniedId | (long)this.mnPageId == gnPageLoginRequiredId | (long)this.mnPageId == gnPageErrorId)


                        {
                            if (RootPageId != this.mnPageId)
                            {
                                mbSystemPage = true;

                                if ((long)this.mnPageId == gnPageAccessDeniedId | (long)this.mnPageId == gnPageLoginRequiredId)
                                {
                                    // moResponse.StatusCode = 401
                                }
                                if ((long)this.mnPageId == gnPageNotFoundId)
                                {
                                    gnResponseCode = 404L;
                                }
                                if ((long)this.mnPageId == gnPageErrorId)
                                {
                                    gnResponseCode = 500L;
                                }

                            }
                        }
                    }

                    if (this.mnArtId < 1)
                    {
                        if (!string.IsNullOrEmpty(this.moRequest["artid"]))
                        {

                            this.mnArtId = GetRequestItemAsInteger("artid", 0);
                        }
                    }

                    if (ibIndexMode)
                    {
                        this.mbAdminMode = false;
                        this.mnUserId = 1;
                    }

                    // Version Control: Set a permission state for this page.
                    // If the permissions are great enough, this will allow content other htan just LIVE to be brought in to the page

                    if (this.moSession != null)
                    {
                        if (gbVersionControl)
                        {
                            mnUserPagePermission = this.moDbHelper.getPagePermissionLevel((long)this.mnPageId);
                        }
                    }
                }
            }

            catch (Exception ex)
            {

                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "Open", ex, sProcessInfo));

            }

        }

        public void Close()
        {
            if (this.moCtx != null)
            {
                this.PerfMon.Log("Base", "Close");
            }
            string sProcessInfo = "";
            try
            {

                // if we access base via soap the session != available
                if (this.moSession != null)
                {

                    Cms argmyWeb = this;
                    moMemProv.Activities.SetUserId(ref argmyWeb);
                    moMemProv.Dispose();
                    moMemProv = null;

                }


                // goApp = Nothing
                this.moRequest = (System.Web.HttpRequest)null;
                // we need this for redirect
                // moResponse = Nothing
                // moSession = Nothing
                this.goServer = (System.Web.HttpServerUtility)null;
                this.moConfig = (System.Collections.Specialized.NameValueCollection)null;

                if (moTransform != null & ibIndexMode == false)
                {
                    moTransform.Close();
                    moTransform = (Protean.XmlHelper.Transform)null;
                }
                if (gbCart & moCart != null)
                {
                    moCart.close();
                    moCart = (Cms.Cart)null;
                }
                // moCart.close() requires db connection
                if (this.moDbHelper != null)
                {
                    // moDbHelper.Close()
                    // TS added in to ensure connections are closed
                    this.moDbHelper.CloseConnection(true);
                    this.moDbHelper = (Cms.dbHelper)null;
                }
            }

            // Dim nMemDif As Integer = Process.GetCurrentProcess.PrivateMemorySize64
            // Dim nProcDif As Integer = Process.GetCurrentProcess.PrivilegedProcessorTime.Milliseconds


            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "Close", ex, sProcessInfo));
            }

        }

        public void InitialiseGlobal()
        {

            string cProcessInfo = string.Empty;
            msException = "";
            string sProcessInfo = string.Empty;
            try
            {
                // _workingSetPrivateMemoryCounter = New PerformanceCounter("Process", "Working Set - Private", Process.GetCurrentProcess.ProcessName)

                // sProcessInfo = Process.GetCurrentProcess.ProcessName

                // Set the debug mode
                //this.moConfig = this.moConfig;
                switch (Strings.LCase(this.moConfig["CssFramework"]) ?? "") 
                {
                    case "bs5":
                        { bs5 = true; break; }
                    case "bs3": 
                        { bs3 = true; break; }
                    default:
                        { break; }
                }

                if (this.moConfig["Debug"] != null)
                {
                    switch (Strings.LCase(this.moConfig["Debug"]) ?? "")
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
                bool bSessionLogging = false;
                if (this.moSession != null)
                {
                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(this.moSession["Logging"], "On", false)))
                    {
                        bSessionLogging = true;
                    }
                }

                if (!string.IsNullOrEmpty(this.moRequest["perfmon"]) | bSessionLogging)
                {
                    if (this.PerfMon is null)
                        this.PerfMon = new PerfLog(this.moConfig["DatabaseName"]);
                    if (this.moSession != null)
                    {
                        // only bother if we are not doing a scheduler thingie
                        if (this.moRequest["perfmon"] == "on")
                        {
                            this.PerfMon.Start();
                        }
                        else if (this.moRequest["perfmon"] == "off")
                        {
                            this.PerfMon.Stop();
                        }
                        else if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(this.moSession["Logging"], "On", false)))
                        {
                            this.PerfMon.Start();
                        }
                    }
                }


                if (this.moRequest["ewCmd"] == "admin")
                {
                    if (this.moSession != null)
                        this.moSession["adminMode"] = "true";
                }


                RootPageId = GetConfigItemAsInteger("RootPageId", 0);
                gnAuthUsers = GetConfigItemAsInteger("AuthenticatedUsersGroupId", 0);
                gnNonAuthUsers = GetConfigItemAsInteger("NonAuthenticatedUsersGroupId", 0);

                if (!string.IsNullOrEmpty(this.moConfig["BehaviourNewContentOrder"]))
                    mcBehaviourNewContentOrder = this.moConfig["BehaviourNewContentOrder"];
                if (!string.IsNullOrEmpty(this.moConfig["BehaviourAddPageCommand"]))
                    mcBehaviourAddPageCommand = this.moConfig["BehaviourAddPageCommand"];
                if (!string.IsNullOrEmpty(this.moConfig["BehaviourEditPageCommand"]))
                    mcBehaviourEditPageCommand = this.moConfig["BehaviourEditPageCommand"];

                if (!string.IsNullOrEmpty(this.moConfig["Language"]))
                {
                    gcLang = this.moConfig["Language"];
                }

                if (this.moConfig["CompliedTransform"] == "on" | this.moConfig["CompiledTransform"] == "on")
                {
                    gbCompiledTransform = true;
                }
                else
                {
                    gbCompiledTransform = false;
                }

                if (Strings.LCase(this.moConfig["Membership"]) == "on")
                {
                    gbMembership = true;
                }

                if (Strings.LCase(this.moConfig["Cart"]) == "on")
                {
                    gbCart = true;
                }

                if (Strings.LCase(this.moConfig["Quote"]) == "on")
                {
                    gbQuote = true;
                }

                if (Strings.LCase(this.moConfig["Report"]) == "on")
                {
                    gbReport = true;
                }

                if (Strings.LCase(this.moConfig["SiteCache"]) == "on")
                {
                    gbSiteCacheMode = true;
                }

                if (Strings.LCase(this.moConfig["Clone"]) == "on")
                {
                    gbClone = true;
                }

                gbMemberCodes = Strings.LCase(this.moConfig["MemberCodes"]) == "on";
                gbVersionControl = Strings.LCase(this.moConfig["VersionControl"]) == "on";
                gbIPLogging = Strings.LCase(this.moConfig["IPLogging"]) == "on";
                gbUseLanguageStylesheets = Strings.LCase(this.moConfig["LanguageStylesheets"]) == "on";
                gcProjectPath = Conversions.ToString(Interaction.IIf(this.moConfig["ProjectPath"] is null, "", this.moConfig["ProjectPath"] + ""));
                gbUserIntegrations = Strings.LCase(this.moConfig["UserIntegrations"]) == "on";
                gbSingleLoginSessionPerUser = Strings.LCase(this.moConfig["SingleLoginSessionPerUser"]) == "on";
                if (!string.IsNullOrEmpty(this.moConfig["SingleLoginSessionTimeout"]))
                {
                    gnSingleLoginSessionTimeout = (short)Tools.Number.ConvertStringToIntegerWithFallback("0" + this.moConfig["SingleLoginSessionTimeout"], (int)gnSingleLoginSessionTimeout);
                }
                gcMenuContentCountTypes = this.moConfig["MenuContentCountTypes"];
                gcMenuContentBriefTypes = this.moConfig["MenuContentBriefTypes"];

                // Get referenced assembly info
                // Given that assemblies are loaded at an application level, we can store the info we find in an application object
                if (string.IsNullOrEmpty(Conversions.ToString(this.goCache["GENERATOR"])))
                {
                    var CodeGenerator = Generator;
                    gcGenerator = CodeGenerator.FullName;
                    gcCodebase = CodeGenerator.CodeBase;
                    foreach (AssemblyName ReferencedAssembly in ReferencedAssemblies)
                        gcReferencedAssemblies += ReferencedAssembly.Name + " (" + ReferencedAssembly.Version.ToString() + "); ";
                    this.goCache["GENERATOR"] = gcGenerator;
                    this.goCache["CODEBASE"] = gcCodebase;
                    this.goCache["REFERENCED_ASSEMBLIES"] = gcReferencedAssemblies;
                }
                else
                {
                    gcGenerator = Conversions.ToString(this.goCache["GENERATOR"]);
                    gcCodebase = Conversions.ToString(this.goCache["CODEBASE"]);
                    gcReferencedAssemblies = Conversions.ToString(this.goCache["REFERENCED_ASSEMBLIES"]);
                }
                if (!string.IsNullOrEmpty(this.moConfig["ShowRelatedBriefDepth"]))
                {
                    gnShowRelatedBriefDepth = Conversions.ToInteger(this.moConfig["ShowRelatedBriefDepth"]);
                }
                if (this.moConfig["cssFramework"] == "bs5")
                {
                    bs5 = true;
                    mcEWCommonFolder = "/ptn";
                }
            }

            catch (Exception ex)
            {
                // returnException("GlobalObjects", "open", ex, "", sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "InitialiseGlobal", ex, sProcessInfo));
            }

        }

        public virtual void InitializeVariables()
        {
            this.PerfMon.Log("Web", "InitializeVariables");
            // Author:        Trevor Spink
            // Copyright:     Eonic Ltd 2005
            // Date:          2005-03-09

            mcModuleName = "Protean.Cms";
            string cProcessInfo = string.Empty;
            msException = "";

            try
            {
                cProcessInfo = "set session variables";

                // Set the rewrite URL
                // Check both IIS7 URLRewrite ad ISAPI Rewrite variants
                if (!string.IsNullOrEmpty("" + this.moRequest.ServerVariables["HTTP_X_ORIGINAL_URL"]))
                {
                    mcOriginalURL = this.moRequest.ServerVariables["HTTP_X_ORIGINAL_URL"];
                }
                else if (!string.IsNullOrEmpty("" + this.moRequest.ServerVariables["HTTP_X_REWRITE_URL"]))
                {
                    mcOriginalURL = this.moRequest.ServerVariables["HTTP_X_REWRITE_URL"];
                }

                System.Collections.Specialized.NameValueCollection moCartConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/cart");

                if (!string.IsNullOrEmpty(this.moRequest.ContentType))
                {
                    mcContentType = this.moRequest.ContentType;
                }

                string ContentType = Strings.LCase(this.moRequest["contentType"]);

                if (mcOriginalURL.StartsWith("/ewapi/"))
                {
                    ContentType = "json";
                }
                if (mcOriginalURL.StartsWith("/QRcode/"))
                {
                    ContentType = "qrcode";
                }

                switch (ContentType ?? "")
                {
                    case "qrcode":
                        {
                            moResponseType = pageResponseType.qrcode;
                            mcContentType = "image/png";
                            break;
                        }
                    case "xml":
                        {
                            mcContentType = "application/xml";
                            this.mbOutputXml = true;
                            moResponseType = pageResponseType.xml;

                            if (!string.IsNullOrEmpty(this.moConfig["XmlAllowedIPList"]))
                            {
                                if (!Tools.Text.IsIPAddressInList(this.moRequest.ServerVariables["REMOTE_ADDR"], this.moConfig["XmlAllowedIPList"]))
                                {
                                    mcContentType = "text/html";
                                    gcEwSiteXsl = this.moConfig["SiteXsl"];
                                    this.mbOutputXml = false;
                                    moResponseType = pageResponseType.Page;
                                }
                            }

                            break;
                        }
                    case "json":
                        {
                            mcContentType = "application/json";
                            this.mbOutputXml = true;
                            moResponseType = pageResponseType.json;
                            break;
                        }
                    case "popup":
                        {
                            if (File.Exists(this.goServer.MapPath("/xsl/admin/AdminPopups.xsl")))
                            {
                                mcEwSiteXsl = "/xsl/admin/AdminPopups.xsl";
                            }
                            else
                            {
                                mcEwSiteXsl = "/ewcommon/xsl/admin/AdminPopups.xsl";
                            }
                            if (bs5)
                                mcEwSiteXsl = "/ptn/admin/admin-popups.xsl";
                            mbPopupMode = true;
                            this.mbAdminMode = true;
                            moResponseType = pageResponseType.popup;
                            break;
                        }
                    case "ajaxadmin":
                        {
                            mcEwSiteXsl = "/ewcommon/xsl/admin/ajaxAdmin.xsl";
                            if (bs5)
                                mcEwSiteXsl = "/ptn/admin/admin-ajax.xsl";
                            moResponseType = pageResponseType.ajaxadmin;
                            break;
                        }
                    // oEw.GetAjaxHTML("MenuNode")
                    case "mail":
                    case "email":
                        {
                            System.Collections.Specialized.NameValueCollection moMailConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/mailinglist");
                            if (!string.IsNullOrEmpty(moMailConfig["MailingXsl"]))
                            {
                                gcEwSiteXsl = moMailConfig["MailingXsl"];
                            }
                            else
                            {
                                gcEwSiteXsl = "/ewcommon/xsl/Mailer/mailerStandard.xsl";
                                if (bs5)
                                    mcEwSiteXsl = "/ptn/features/mailer/mailer-core.xsl";
                            }
                            mnMailMenuId = Conversions.ToLong(moMailConfig["RootPageId"]);
                            mcContentType = "text/html";
                            moResponseType = pageResponseType.mail;
                            break;
                        }

                    default:
                        {
                            mcContentType = "text/html";
                            gcEwSiteXsl = this.moConfig["SiteXsl"];
                            moResponseType = pageResponseType.Page;
                            // can we get a cached page
                            if (this.moRequest.ServerVariables["HTTP_X_ORIGINAL_URL"] != null)
                            {
                                if (gnResponseCode == 200L & this.moRequest.Form.Count == 0 & this.mnUserId == 0 & !this.moRequest.ServerVariables["HTTP_X_ORIGINAL_URL"].Contains("?"))
                                {
                                    bPageCache = Conversions.ToBoolean(Interaction.IIf(Strings.LCase(this.moConfig["PageCache"]) == "on", true, false));
                                }

                                if (this.moRequest["perfmon"] == "on" & this.moRequest.QueryString.Count == 1)
                                {
                                    bPageCache = Conversions.ToBoolean(Interaction.IIf(Strings.LCase(this.moConfig["PageCache"]) == "on", true, false));
                                }

                                if (this.moRequest["reBundle"] != null)
                                {
                                    bPageCache = true;
                                }
                            }

                            break;
                        }
                }


                if (this.moSession != null)
                {
                    // this simply gets the userId earlier if it is in the session.
                    // behaviour to check single session or transfer from the cart is still called from Open()
                    Cms argmyWeb = this;
                    if (moMemProv == null)
                    { 
                        ReturnProvider RetProv = new Protean.Providers.Membership.ReturnProvider();
                        moMemProv = RetProv.Get(ref argmyWeb, this.moConfig["MembershipProvider"]);
                        RetProv = null;
                    }
                    this.mnUserId = Conversions.ToInteger(moMemProv.Activities.GetUserSessionId(ref argmyWeb));

                    if (this.mnUserId > 0)
                    {
                        bPageCache = false;
                    }
                }


                //this.mnArtId = this.mnArtId;
                //this.mnPageId = this.mnPageId;
                //this.mnUserId = this.mnUserId;

                mbIsUsingHTTPS = this.moRequest.ServerVariables["HTTPS"] == "on";

                // If the site refers to itself for HTTPS and the domain is different then 
                // BaseURL needs to be the secure site
                if (gbCart && mbIsUsingHTTPS && this.moConfig["OverrideBaseUrlWithSecureSiteForHTTPS"] == "on" && !string.IsNullOrEmpty(moCartConfig["SecureURL"]))


                {
                    gcEwBaseUrl = moCartConfig["SecureURL"];
                }
                else
                {
                    gcEwBaseUrl = this.moConfig["BaseUrl"];
                }

                mcRequestDomain = Conversions.ToString(Operators.ConcatenateObject(Interaction.IIf(mbIsUsingHTTPS, "https://", "http://"), this.moRequest.ServerVariables["SERVER_NAME"]));

                // Language sites.
                // If LanguageStyleSheets is on, then we look for the xsl with the language as a suffix
                // If it exists, we load that in as the stylesheet.
                if (gbUseLanguageStylesheets)
                {
                    try
                    {
                        string langStyleFile = mcEwSiteXsl.Replace(".xsl", "-" + gcLang.Replace(':', '-') + ".xsl");
                        if (File.Exists(this.goServer.MapPath(langStyleFile)))
                        {
                            mcEwSiteXsl = langStyleFile;
                        }
                    }
                    catch (Exception)
                    {
                        // Do nothing, but don't fall over
                    }
                }



                var commonfolders = new ArrayList();

                mcClientCommonFolder = Conversions.ToString(Interaction.IIf(this.moConfig["ClientCommonFolder"] is null, "", this.moConfig["ClientCommonFolder"] + ""));

                if (!string.IsNullOrEmpty(gcProjectPath))
                    commonfolders.Add(gcProjectPath);
                if (!string.IsNullOrEmpty(mcClientCommonFolder))
                    commonfolders.Add(mcClientCommonFolder);
                if (!string.IsNullOrEmpty(mcEWCommonFolder))
                    commonfolders.Add(mcEWCommonFolder);

                if (bs5)
                {
                    commonfolders.Add("/ptn/features/");
                    commonfolders.Add("/ptn/core/");
                }



                maCommonFolders = (string[])commonfolders.ToArray(typeof(string));
            }

            // force an error for testing error function

            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "InitializeVariables", ex, gcEwSiteXsl, cProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "InitializeVariables", ex, ""));
            }

        }


        public void CheckPagePath()
        {
            string pageUrl = this.moRequest.RawUrl.ToString();
            bool bLowerCaseUrl = false;
            bool bTrailingSlash = false;
            if (this.moSession != null)
            {
                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(this.moSession["adminMode"], "true", false)))
                {
                    this.mbAdminMode = true;
                }
            }
            if (!this.mbAdminMode & this.moConfig["CheckPageURL"] == "on" & !ibIndexMode & this.mbPreview == false & this.moRequest.QueryString["cartCmd"] is null & !mcOriginalURL.Contains("?"))
            {
                if (this.moConfig["LowerCaseUrl"] == "on")
                {
                    if (Regex.IsMatch(pageUrl, "[A-Z]"))
                    {
                        bLowerCaseUrl = true;
                        pageUrl = pageUrl.ToLower();
                    }
                }
                if (!string.IsNullOrEmpty(this.moConfig["DetailPathType"]))
                {
                    if (this.moConfig["TrailingSlash"] == "on")
                    {
                        if (pageUrl.Length != 0 & Strings.Right(pageUrl, 1) != "/")
                        {
                            if (pageUrl.Length != 1)
                            {
                                pageUrl = pageUrl + "/";
                                bTrailingSlash = true;
                            }
                        }
                    }
                }
                if ((bTrailingSlash | bLowerCaseUrl) & pageUrl.Length != 0)
                {
                    mbRedirectPerm = Conversions.ToString(true);
                    msRedirectOnEnd = this.moRequest.Url.Scheme.ToString().ToLower() + "://" + this.moRequest.Url.Host.ToString().ToLower() + pageUrl;
                }
            }
        }

        public bool RestoreRedirectSession(string sSessionId, int nStandardDuration, bool isAdmin = false)
        {
            // we check the activity log for recompile with same session id, check the datetime is within 5 seconds.
            try
            {
                int nDuration = 0;
                int nUserId = 0;
                object sSql = "select top 1 nUserDirId, datediff(SS,getdate(),dDateTime) as Duration from tblActivityLog where cSessionId='" + sSessionId + "' order by dDateTime desc";
                using (var oDr = this.moDbHelper.getDataReaderDisposable(Conversions.ToString(sSql)))
                {
                    if (oDr != null)
                    {
                        while (oDr.Read())
                        {
                            nDuration = Convert.ToInt32(oDr["Duration"]);
                            nUserId = Conversions.ToInteger(oDr["nUserDirId"]);
                        }

                    }
                }
                if (nDuration <= nStandardDuration & nUserId != 0)
                {

                    this.mnUserId = nUserId;
                    if (isAdmin)
                    {
                        this.moSession["adminMode"] = "true";
                        this.mbAdminMode = true;
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }



        public virtual void GetPageHTML()
        {
            this.PerfMon.Log("Web", "GetPageHTML - start");
            string sProcessInfo = "";
            string sCachePath = "";
            string sServeFile = "";
            try
            {
                switch (moResponseType)
                {
                    case pageResponseType.ajaxadmin:
                        {
                            // TS 21-06-2017 Moved from New() as not required for cached pages I think.
                            Open();
                            GetAjaxHTML("MenuNode");
                            break;
                        }

                    case pageResponseType.json:
                        {

                            var moApi = new Protean.rest();

                            moApi.InitialiseVariables();
                            moApi.JSONRequest();
                            break;
                        }

                    case pageResponseType.qrcode:
                        {

                            // This feature returns a QRcode of the website path.
                            // calling http://proteancms.com/QRcode/anyurlgoeshere
                            // returns qr code for http://proteancms.com/anyurlgoeshere

                            string QRPath = this.moRequest.RawUrl.ToString();

                            QRPath = QRPath.Replace("/QRcode", "");
                            object isSecure = "https://";
                            if (this.moRequest.ServerVariables["SERVER_PORT_SECURE"] == "0")
                            {
                                isSecure = "http://";
                            }
                            QRPath = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(isSecure, this.moRequest.ServerVariables["SERVER_NAME"]), QRPath));
                            var qrGenerator = new QRCoder.QRCodeGenerator();
                            var qrCodeData = qrGenerator.CreateQrCode(QRPath, QRCoder.QRCodeGenerator.ECCLevel.Q);
                            var qrCode = new QRCoder.QRCode(qrCodeData);
                            var qrCodeImage = qrCode.GetGraphic(20);
                            this.moResponse.ContentType = mcContentType;
                            qrCodeImage.Save(this.moResponse.OutputStream, System.Drawing.Imaging.ImageFormat.Png);
                            this.moResponse.End();
                            break;
                        }

                    default:
                        {
                            CheckPagePath();

                            if (gbCart | gbQuote)
                            {
                                if (Conversions.ToInteger(Operators.AddObject("0", this.moSession["CartId"])) > 0)
                                {
                                    bPageCache = false;
                                }
                            }


                            if (this.moRequest["reBundle"] != null)
                            {

                                if (this.moRequest["SessionId"] != null)
                                {
                                    this.RestoreRedirectSession(this.moRequest["SessionId"], 5, true);
                                }

                                if (this.mbAdminMode)
                                {
                                    ClearPageCache();
                                    ClearBundleCache("js");
                                    ClearBundleCache("css");
                                }

                            }
                            if (bPageCache & !ibIndexMode & !(gnResponseCode == 404L))
                            {

                                sCachePath = this.goServer.UrlDecode(mcOriginalURL);
                                if (sCachePath.Contains("?"))
                                {
                                    sCachePath = sCachePath.Substring(0, sCachePath.IndexOf("?"));
                                }
                                sCachePath = sCachePath + ".html";
                                if (!string.IsNullOrEmpty(gcProjectPath))
                                {
                                    sCachePath = sCachePath.Replace(gcProjectPath, "");
                                }


                                if (sCachePath == "/.html" | sCachePath == ".html")
                                {
                                    sCachePath = "/home.html";
                                }

                                long nCacheTimeout = 24L;
                                if (Information.IsNumeric(this.moConfig["PageCacheTimeout"]))
                                {
                                    nCacheTimeout = Conversions.ToLong(this.moConfig["PageCacheTimeout"]);
                                }
                                var oFS = new Protean.fsHelper(this.moCtx);
                                oFS.mcRoot = gcProjectPath;
                                oFS.mcStartFolder = this.goServer.MapPath(@"\" + gcProjectPath).TrimEnd('\\') + mcPageCacheFolder;
                                if (Conversions.ToBoolean(oFS.VirtualFileExistsAndRecent(sCachePath, nCacheTimeout)))
                                {
                                    sServeFile = mcPageCacheFolder + sCachePath;
                                }
                            }

                            this.moResponse.HeaderEncoding = System.Text.Encoding.UTF8;
                            this.moResponse.ContentEncoding = System.Text.Encoding.UTF8;
                            this.moResponse.Expires = 0;
                            this.moResponse.AppendHeader("Generator", gcGenerator);

                            if (string.IsNullOrEmpty(sServeFile))
                            {

                                // TS 21-06-2017 Moved from New() as not required for cached pages I think.
                                Open();

                                if (this.mbAdminMode & !ibIndexMode & !(gnResponseCode == 404L))
                                {
                                    bPageCache = false;
                                }

                                sProcessInfo = "Transform PageXML Using XSLT";
                                if (this.mbAdminMode & !ibIndexMode & !(gnResponseCode == 404L))
                                {
                                    sProcessInfo = "In Admin Mode";
                                    if (moAdmin is null)
                                    {
                                        var argaWeb = this;
                                        moAdmin = new Cms.Admin(ref argaWeb);
                                    }
                                    // Dim oAdmin As Admin = New Admin(Me)
                                    // Dim oAdmin As Protean.Cms.Admin = New Protean.Cms.Admin(Me)
                                    moAdmin.open(moPageXml);
                                    var argoWeb = this;
                                    moAdmin.adminProcess(ref argoWeb);
                                    moAdmin.close();
                                    moAdmin = (Cms.Admin)null;
                                }
                                else if (string.IsNullOrEmpty(moPageXml.OuterXml))
                                {
                                    sProcessInfo = "Getting Page XML";
                                    GetPageXML();


                                }

                                if (moResponseType == pageResponseType.flush)
                                {

                                    this.moResponse.Flush();
                                    this.moResponse.End();
                                    Close();
                                }
                                else
                                {


                                    // we assume this has allready been set and we have allready done a response.write
                                    // used for admin file uploader, not quite happy with this TS. 9-9-2020

                                    if (!string.IsNullOrEmpty(msException))
                                    {
                                        // If there is an error we can add our own header.
                                        // this means external programs can check that there is an error
                                        this.moResponse.AddHeader("X-ProteanCMSError", "An Error has occured");
                                        gnResponseCode = 500L;
                                        this.moResponse.ContentType = "text/html";
                                        bPageCache = false;
                                    }
                                    else
                                    {
                                        // Set the Content Type
                                        if (mcContentType == "application/x-www-form-urlencoded")
                                        {
                                            this.moResponse.ContentType = "text/html";
                                        }
                                        else
                                        {
                                            this.moResponse.ContentType = mcContentType;
                                        }
                                        // Set the Content Disposition
                                        if (!string.IsNullOrEmpty(mcContentDisposition))
                                        {
                                            this.moResponse.AddHeader("Content-Disposition", mcContentDisposition);
                                        }

                                        // ONLY CACHE html PAGES
                                        if (mcContentType != "text/html" & !string.IsNullOrEmpty(mcContentDisposition))
                                        {
                                            bPageCache = false;
                                        }

                                    }

                                    if (bPageCache == false)
                                    {
                                        sServeFile = "";
                                    }

                                    if (!string.IsNullOrEmpty(msRedirectOnEnd))
                                    {
                                        moPageXml = null;
                                        Close();
                                    }
                                    else
                                    {
                                        // Check if the XML Output has an optional IP restriction placed against it.
                                        if (this.mbOutputXml)
                                        {
                                            if (!string.IsNullOrEmpty(this.moConfig["XmlAllowedIPList"]))
                                            {
                                                if (!Tools.Text.IsIPAddressInList(this.moRequest.ServerVariables["REMOTE_ADDR"], this.moConfig["XmlAllowedIPList"]))
                                                    this.mbOutputXml = false;
                                            }
                                        }
                                        if (this.mbOutputXml == true)
                                        {
                                            switch (Strings.LCase(mcContentType) ?? "")
                                            {
                                                case "application/xml":
                                                    {
                                                        this.moResponse.Write("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + moPageXml.OuterXml);
                                                        break;
                                                    }
                                                case "application/json":
                                                    {
                                                        this.moResponse.Write(Newtonsoft.Json.JsonConvert.SerializeXmlNode(moPageXml.DocumentElement, Newtonsoft.Json.Formatting.None));
                                                        break;
                                                    }
                                            }
                                        }



                                        else
                                        {

                                            this.PerfMon.Log("Web", "GetPageHTML-loadxsl");
                                            string styleFile;
                                            if (this.mbAdminMode == true)
                                            {
                                                if (Strings.LCase(moPageXml.DocumentElement.GetAttribute("adminMode")) == "false" | mbPopupMode == true | mcContentType != "text/html")
                                                {
                                                    styleFile = this.goServer.MapPath(mcEwSiteXsl);
                                                }
                                                else if (Strings.LCase(this.moConfig["AdminXsl"]) == "common")
                                                {
                                                    // uses the default admin xsl
                                                    styleFile = this.goServer.MapPath("/ewcommon/xsl/admin/page.xsl");
                                                }
                                                else if (!string.IsNullOrEmpty(this.moConfig["AdminXsl"]))
                                                {
                                                    // uses a specified admin XSL
                                                    styleFile = this.goServer.MapPath(this.moConfig["AdminXsl"]);
                                                }
                                                else
                                                {
                                                    // uses the sites main XSL
                                                    styleFile = this.goServer.MapPath(mcEwSiteXsl);
                                                }
                                                if (moResponseType == pageResponseType.pdf)
                                                {

                                                    styleFile = this.goServer.MapPath(mcEwSiteXsl);
                                                }
                                            }

                                            else
                                            {
                                                if (moResponseType == pageResponseType.Page)
                                                {
                                                    if (!string.IsNullOrEmpty(this.moConfig["xframeoptions"]))
                                                    {
                                                        this.moResponse.AddHeader("X-Frame-Options", this.moConfig["xframeoptions"]);
                                                    }
                                                    else
                                                    {
                                                        this.moResponse.AddHeader("X-Frame-Options", "DENY");
                                                    }
                                                }
                                                if (mbSetNoBrowserCache)
                                                {
                                                    this.moResponse.Cache.SetNoStore();
                                                    this.moResponse.Cache.AppendCacheExtension("no-cache");
                                                    this.moResponse.Expires = 0;
                                                }
                                                styleFile = this.goServer.MapPath(mcEwSiteXsl);
                                            }

                                            bool brecompile = false;

                                            if (!string.IsNullOrEmpty(this.moRequest["recompile"]))
                                            {

                                                if (this.moRequest["recompile"] == "del")
                                                {

                                                    if (this.RestoreRedirectSession(this.moRequest["SessionId"], 10, true) == true)
                                                    {

                                                        // Protean.Config.UpdateConfigValue(Me, "", "recompile", "false")

                                                        var oFS = new Protean.fsHelper(this.moCtx);
                                                        oFS.mcRoot = gcProjectPath;
                                                        oFS.mcStartFolder = this.goServer.MapPath(@"\" + gcProjectPath) + "xsltc";

                                                        oFS.DeleteFolderContents("", "");
                                                        var argmyWeb = this;
                                                        Protean.Config.UpdateConfigValue(ref argmyWeb, "protean/web", "CompiledTransform", "on");
                                                        var argmyWeb1 = this;
                                                        Protean.Config.UpdateConfigValue(ref argmyWeb1, "", "recompile", "false");
                                                        msRedirectOnEnd = "/?rebundle=true&SessionId=" + SessionID;
                                                    }
                                                }

                                                else if (this.mbAdminMode)
                                                {
                                                    var argmyWeb2 = this;
                                                    Protean.Config.UpdateConfigValue(ref argmyWeb2, "protean/web", "CompiledTransform", "off");
                                                    // just sent value as it might be true when user did ResetConfig
                                                    // to avoid skipping update functionality, we are just set it differently
                                                    var argmyWeb3 = this;
                                                    Protean.Config.UpdateConfigValue(ref argmyWeb3, "", "recompile", "recompiling");
                                                    this.moDbHelper.logActivity(Cms.dbHelper.ActivityType.Recompile, (long)this.mnUserId, 0L);
                                                    // we log to the activity log this action
                                                    msRedirectOnEnd = "/?recompile=del&SessionId=" + SessionID;
                                                }
                                            }

                                            var argaWeb1 = this;
                                            var oTransform = new Protean.XmlHelper.Transform(ref argaWeb1, styleFile, gbCompiledTransform, 15000L, brecompile);
                                            if (!string.IsNullOrEmpty(this.moConfig["XslTimeout"]))
                                            {
                                                oTransform.TimeOut = Conversions.ToLong(this.moConfig["XslTimeout"]);
                                            }
                                            oTransform.mbDebug = gbDebug;

                                            if (bPageCache)
                                            {

                                                var textWriter = new StringWriterWithEncoding(System.Text.Encoding.UTF8);
                                                this.PerfMon.Log("Web", "GetPageHTML-startxsl");
                                                TextWriter argoWriter = textWriter;
                                                oTransform.ProcessTimed(moPageXml, ref argoWriter);
                                                textWriter = (StringWriterWithEncoding)argoWriter;
                                                this.PerfMon.Log("Web", "GetPageHTML-endxsl");

                                                // save the page
                                                if (!oTransform.bError)
                                                {
                                                    if (bPageCache)
                                                    {
                                                        string pagestring = textWriter.ToString();

                                                        if (gnResponseCode != 200L)
                                                        {
                                                            this.moResponse.TrySkipIisCustomErrors = true;
                                                            this.moResponse.StatusCode = (int)gnResponseCode;
                                                        }

                                                        this.moResponse.Write(pagestring);
                                                        this.moResponse.Flush();
                                                        gnResponseCode = 200L; // we don't want this to happen again on line 1930
                                                        SavePage(sCachePath, pagestring);
                                                    }
                                                    // sServeFile = mcPageCacheFolder & sCachePath
                                                    else
                                                    {
                                                        this.moResponse.Write(textWriter.ToString());
                                                    }
                                                }
                                                else
                                                {
                                                    this.moResponse.AddHeader("X-ProteanCMSError", "An Error has occured");
                                                    gnResponseCode = 500L;
                                                    this.moResponse.Write(textWriter.ToString());
                                                }
                                            }

                                            else if (moResponseType == pageResponseType.pdf)
                                            {
                                                mcContentType = "application/pdf";
                                                // Next we transform using into FO.Net Xml


                                                string styleFile2 = this.goServer.MapPath(mcEwSiteXsl);
                                                this.PerfMon.Log("Web", "ReturnPageHTML - loaded Style");
                                                var argaWeb2 = this;
                                                oTransform = new Protean.XmlHelper.Transform(ref argaWeb2, styleFile2, false);
                                                oTransform.mbDebug = gbDebug;

                                                msException = "";
                                                icPageWriter = new StringWriter();

                                                TextWriter argoWriter1 = icPageWriter;
                                                oTransform.ProcessTimed(moPageXml, ref argoWriter1);
                                                icPageWriter = (StringWriter)argoWriter1;

                                                string foNetXml = icPageWriter.ToString();


                                                if (foNetXml.StartsWith("<html"))
                                                {
                                                    this.moResponse.Write(foNetXml);
                                                }
                                                else
                                                {
                                                    // now we use FO.Net to generate our PDF

                                                    string strFileName = mcOutputFileName;

                                                    var oFoNet = new Fonet.FonetDriver();
                                                    var ofileStream = new MemoryStream();
                                                    var oTxtReader = new StringReader(foNetXml);
                                                    oFoNet.CloseOnExit = false;

                                                    var rendererOpts = new Fonet.Render.Pdf.PdfRendererOptions();

                                                    rendererOpts.Author = "ProteanCMS";
                                                    rendererOpts.EnablePrinting = true;
                                                    rendererOpts.FontType = Fonet.Render.Pdf.FontType.Embed;
                                                    // rendererOpts.Kerning = True
                                                    // rendererOpts.EnableCopy = True

                                                    string fontPath = this.goServer.MapPath("") + "/fonts";
                                                    if (bs5)
                                                    {
                                                        fontPath = this.goServer.MapPath("") + "/fonts";
                                                    }

                                                    var dir = new DirectoryInfo(fontPath);

                                                    if (dir.Exists)
                                                    {
                                                        DirectoryInfo[] subDirs = dir.GetDirectories();
                                                        FileInfo[] files = dir.GetFiles();

                                                        foreach (var fi in files)
                                                        {
                                                            string cExt = Strings.LCase(fi.Extension);
                                                            switch (cExt ?? "")
                                                            {
                                                                case ".otf":
                                                                    {
                                                                        rendererOpts.AddPrivateFont(fi);
                                                                        break;
                                                                    }
                                                            }
                                                        }
                                                    }

                                                    fontPath = this.goServer.MapPath("/ewcommon") + "/fonts";
                                                    if (bs5) {
                                                        fontPath = this.goServer.MapPath("/ptn") + "/fonts";
                                                    } 

                                                    dir = new DirectoryInfo(fontPath);

                                                    if (dir.Exists)
                                                    {
                                                        DirectoryInfo[] subDirs = dir.GetDirectories();
                                                        FileInfo[] files = dir.GetFiles();

                                                        foreach (var fi in files)
                                                        {
                                                            string cExt = Strings.LCase(fi.Extension);
                                                            switch (cExt ?? "")
                                                            {
                                                                case ".otf":
                                                                    {
                                                                        rendererOpts.AddPrivateFont(fi);
                                                                        break;
                                                                    }
                                                            }
                                                        }
                                                    }

                                                    oFoNet.Options = rendererOpts;
                                                    oFoNet.Render(oTxtReader, ofileStream);

                                                    this.moResponse.Buffer = true;
                                                    this.moResponse.Expires = 0;
                                                    this.goServer.ScriptTimeout = 10000;

                                                    string strFileSize = ofileStream.Length.ToString();
                                                    byte[] Buffer = ofileStream.ToArray();

                                                    this.moCtx.Response.Clear();
                                                    // Const adTypeBinary = 1
                                                    this.moCtx.Response.AddHeader("Connection", "keep-alive");
                                                    if (this.moCtx.Request.QueryString["mode"] == "open")
                                                    {
                                                        this.moCtx.Response.AddHeader("Content-Disposition", "filename=" + Strings.Replace(strFileName, ",", ""));
                                                    }
                                                    else
                                                    {
                                                        this.moCtx.Response.AddHeader("Content-Disposition", "attachment; filename=" + Strings.Replace(strFileName, ",", ""));
                                                    }
                                                    this.moCtx.Response.AddHeader("Content-Length", strFileSize);
                                                    // ctx.Response.Charset = "UTF-8"
                                                    this.moCtx.Response.ContentType = Tools.FileHelper.GetMIMEType("PDF");
                                                    this.moCtx.Response.BinaryWrite(Buffer);
                                                    this.moCtx.Response.Flush();

                                                    // objStream = Nothing
                                                    oFoNet = null;
                                                    oTxtReader = null;
                                                    ofileStream = null;
                                                }
                                            }
                                            else
                                            {
                                                DateTime UpdatedTime = mdPageUpdateDate ?? DateTime.Now;
                                                this.moResponse.AddHeader("Last-Modified", Tools.Text.HtmlHeaderDateTime(UpdatedTime) + ",");
                                                this.PerfMon.Log("Web", "GetPageHTML-startxsl");

                                                oTransform.ProcessTimed(moPageXml, ref this.moResponse);
                                                this.PerfMon.Log("Web", "GetPageHTML-endxsl");
                                            }
                                            this.PerfMon.Log("Web", "GetPageHTML-endxsl");
                                            oTransform.Close();
                                            oTransform = (Protean.XmlHelper.Transform)null;
                                        }

                                        // moResponse.SuppressContent = False
                                        if (gnResponseCode != 200L)
                                        {
                                            // TODO: This is IIS7 specific, needs addressing for IIS6

                                            this.moResponse.TrySkipIisCustomErrors = true;
                                            this.moResponse.StatusCode = (int)gnResponseCode;
                                        }

                                        // we don't need this anymore.
                                        if (!ibIndexMode)
                                        {
                                            if (string.IsNullOrEmpty(msRedirectOnEnd))
                                            {
                                                moPageXml = null;
                                                if (string.IsNullOrEmpty(sServeFile))
                                                {
                                                    Close();
                                                }
                                            }
                                            else
                                            {
                                                moPageXml = null;
                                                if (string.IsNullOrEmpty(sServeFile))
                                                {
                                                    Close();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            moPageXml = new XmlDocument();
                                        }
                                    }
                                }
                            }

                            if (this.moSession != null)
                            {
                                this.moSession["previousPage"] = mcOriginalURL;
                            }

                            if (!string.IsNullOrEmpty(sServeFile))
                            {
                                if (!string.IsNullOrEmpty(this.moConfig["xframeoptions"]))
                                {
                                    this.moResponse.AddHeader("X-Frame-Options", this.moConfig["xframeoptions"]);
                                }
                                else
                                {
                                    this.moResponse.AddHeader("X-Frame-Options", "DENY");
                                }
                                short filelen = (short)(this.goServer.MapPath("/" + gcProjectPath).Length + sServeFile.Length);
                                DateTime UpdatedTime = mdPageUpdateDate ?? DateTime.Now;
                                this.moResponse.AddHeader("Last-Modified", Tools.Text.HtmlHeaderDateTime(UpdatedTime));
                                this.PerfMon.Log("Web", "GetPageHTML - serve cached file");
                                if (filelen > 260)
                                {
                                    this.moResponse.Write(Alphaleonis.Win32.Filesystem.File.ReadAllText(this.goServer.MapPath("/" + gcProjectPath) + sServeFile));
                                }
                                else
                                {
                                    this.moResponse.WriteFile(this.goServer.MapPath("/" + gcProjectPath) + sServeFile);
                                }
                                Close();
                            }

                            break;
                        }


                }

                if (this.moSession != null)
                {
                    // clears the recaptcha flag for the session
                    this.moSession["recaptcha"] = (object)null;
                }
            }

            catch (Exception ex)
            {
                if ((mcEwSiteXsl ?? "") != (this.moConfig["SiteXsl"] ?? ""))
                    mcEwSiteXsl = this.moConfig["SiteXsl"];
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetPageHTML", ex, sProcessInfo));
                // returnException(msException, mcModuleName, "getPageHtml", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                this.moResponse.Write(msException);
                //Finalize();
            }
            finally
            {
                // '  moDbHelper.CloseConnection(True)
                if (!string.IsNullOrEmpty(msRedirectOnEnd))
                {
                    if (!msRedirectOnEnd.StartsWith("http"))
                    {
                        msRedirectOnEnd = msRedirectOnEnd.Replace("//", "/");
                    }
                    if (Conversions.ToBoolean(mbRedirectPerm))
                    {
                        this.moResponse.RedirectPermanent(msRedirectOnEnd, false);
                    }
                    else
                    {

                        this.moResponse.Redirect(msRedirectOnEnd, false);
                        if (bRestartApp)
                        {
                            RestartAppPool();
                        }
                    }
                }
                // moCtx.ApplicationInstance.CompleteRequest()
                else if (bRestartApp)
                {
                    this.moResponse.Redirect("/", false);
                    RestartAppPool();
                }
                this.PerfMon.Log("Web", "GetPageHTML-Final");
                this.PerfMon.Write();
                // moResponse.End()
            }

        }

        public virtual XmlDocument GetPageXML()
        {
            XmlDocument GetPageXMLRet = default;
            string sProcessInfo = "PerfMon";
            this.PerfMon.Log("Web", "GetPageXML");

            try
            {
                if (!ibIndexMode)
                {
                    AddCurrency();
                }

                sProcessInfo = "BuildPageXML";
                BuildPageXML();

                if (string.IsNullOrEmpty(msException))
                {


                    if (!this.mbAdminMode & this.moConfig["CheckPageURL"] == "on" & !ibIndexMode)
                    {
                        string url;
                        string pagePath;
                        if (!string.IsNullOrEmpty(this.moConfig["DetailPathType"]) & this.mnArtId == 0) // case to check for detail path setting and are we on a detail page. 
                        {
                            XmlElement oMenuNode = (XmlElement)moPageXml.SelectSingleNode("/Page/Menu/MenuItem/descendant-or-self::MenuItem[@id='" + this.mnPageId + "']");
                            if (oMenuNode != null)
                            {
                                if (!oMenuNode.GetAttribute("url").StartsWith("http"))
                                {
                                    url = oMenuNode.GetAttribute("url");
                                    pagePath = this.mcPagePath;
                                    if (this.moConfig["TrailingSlash"] == "on")
                                    {
                                        if (url.Length != 0 & Strings.Right(url, 1) == "/")
                                        {
                                            url = Strings.Left(url, url.Length - 1);
                                        }
                                        if (pagePath.Length != 0 & Strings.Right(pagePath, 1) == "/")
                                        {
                                            pagePath = Strings.Left(pagePath, pagePath.Length - 1);
                                        }
                                    }

                                    if ((url.ToLower() ?? "") != (pagePath.ToLower() ?? ""))
                                    {
                                        // msRedirectOnEnd = "/System+Pages/Page+Not+Found"

                                        this.mnPageId = (int)gnPageNotFoundId;
                                        moPageXml = new XmlDocument();
                                        BuildPageXML();
                                        // moResponse.StatusCode = 404
                                        gnResponseCode = 404L;

                                    }
                                }
                            }
                            else if (this.moConfig["PageNotFoundId"] != null)
                            {
                                if ((this.mnPageId.ToString() ?? "") != (this.moConfig["PageNotFoundId"] ?? "") & string.IsNullOrEmpty(msException))
                                {

                                    // msRedirectOnEnd = "/System+Pages/Page+Not+Found"
                                    this.mnPageId = (int)gnPageNotFoundId;
                                    moPageXml = new XmlDocument();
                                    BuildPageXML();
                                    // moResponse.StatusCode = 404
                                    gnResponseCode = 404L;
                                }

                            }
                        }

                    }

                    string layoutCmd = "";
                    if (this.moSession != null & !ibIndexMode)
                    {
                        if (((double)this.mnUserId != Conversions.ToDouble("0") | Strings.LCase(this.moConfig["LogAll"]) == "On") & this.mbAdminMode == false & this.Features.ContainsKey("ActivityReporting"))
                        {
                            if (this.moRequest["noFrames"] != "True") // Fix for frameset double counting                   
                            {
                                // moDbHelper.logActivity(dbHelper.ActivityType.PageAccess, mnUserId, mnPageId, mnArtId)
                                this.moDbHelper.CommitLogToDB(Cms.dbHelper.ActivityType.PageViewed, this.mnUserId, this.moSession.SessionID, DateTime.Now, this.mnPageId, this.mnArtId, this.moRequest.ServerVariables["REMOTE_ADDR"] + " " + this.moRequest.ServerVariables["HTTP_USER_AGENT"]);
                            }
                        }
                    }

                    sProcessInfo = "Check Index Mode";
                    if (!ibIndexMode)
                    {

                        sProcessInfo = "Check Membership";
                        if (gbMembership)
                        {
                            MembershipProcess();
                        }
                        else if (this.mbAdminMode & this.mnUserId > 0)
                        {
                            RefreshUserXML();
                        }

                        // TS-Moved to after add bulk related content to enable filters to be processed.
                        // not sure if this is before for a reason. I cannot seem to think of one.
                        // required for related images on contentgrabber for practitioner so added config setting
                        if (Strings.LCase(this.moConfig["ActionsBeforeAddBulk"]) == "on")
                        {
                            ContentActions();
                        }


                        if (Strings.LCase(this.moConfig["FinalAddBulk"]) == "on")
                        {

                            string cShowRelatedBriefDepth = this.moConfig["ShowRelatedBriefDepth"] + "";
                            int nMaxDepth = 1;
                            if (!string.IsNullOrEmpty(cShowRelatedBriefDepth) && Information.IsNumeric(cShowRelatedBriefDepth))
                            {
                                nMaxDepth = Conversions.ToInteger(cShowRelatedBriefDepth);
                            }
                            XmlElement argoContentParent = (XmlElement)moPageXml.DocumentElement.SelectSingleNode("Contents");
                            DateTime UpdatedTime = mdPageUpdateDate ?? DateTime.Now;
                            this.moDbHelper.addBulkRelatedContent(ref argoContentParent, ref UpdatedTime, nMaxDepth);

                        }

                        sProcessInfo = "Check Admin Mode";

                        if (this.moConfig["ActionsBeforeAddBulk"] != "on")
                        {
                            ContentActions();
                        }

                        CommonActions();

                        // TS commented out so Century can perform searches in admin mode
                        // If Not (mbAdminMode) Then
                        layoutCmd = LayoutActions();
                        // End If

                        AddCart();

                        if (gbQuote)
                        {
                            sProcessInfo = "Begin Quote";
                            var argaWeb = this;
                            var oEq = new Cms.Quote(ref argaWeb);
                            oEq.apply();
                            oEq.close();
                            oEq = (Cms.Quote)null;
                            sProcessInfo = "End Quote";
                        }

                        if (Strings.LCase(this.moConfig["Search"]) == "On")
                        {

                            var oSearchNode = moPageXml.CreateElement("Search");
                            oSearchNode.SetAttribute("mode", this.moConfig["SearchMode"]);
                            oSearchNode.SetAttribute("contentTypes", this.moConfig["SearchContentTypes"]);
                            moPageXml.DocumentElement.AppendChild(oSearchNode);

                        }

                        if (this.mbAdminMode)
                        {
                            try
                            {
                                if (string.IsNullOrEmpty(this.moRequest["ewCmd"]))
                                {
                                    ProcessReports();
                                }
                            }
                            catch
                            {
                                // do nothing
                            }
                        }

                        else
                        {
                            ProcessReports();
                        }

                        // Process the Calendars
                        ProcessCalendar();

                        if (gbVersionControl)
                            CheckContentVersions();

                    }
                    sProcessInfo = "CheckMultiParents";
                    var argoPage = moPageXml.DocumentElement;
                    this.CheckMultiParents(ref argoPage, this.mnPageId);

                    // ProcessContentForLanguage
                    ProcessPageXMLForLanguage();

                    // Add the responses
                    CommitResponsesToPage();

                    if (this.moSession != null)
                    {
                        if (Conversions.ToBoolean(Operators.AndObject(Operators.ConditionalCompareObjectNotEqual(this.moSession["RedirectReason"], "", false), !bRedirectStarted))) // bRegistrationSuccessful is a local variable and is only set before the redirection occurs - hence looking for it being False.
                        {
                            // Add a flag - the XSL can pick this up
                            moPageXml.DocumentElement.SetAttribute("RedirectReason", Conversions.ToString(this.moSession["RedirectReason"]));
                            // Remove the Registration flag.
                            this.moSession.Remove("RedirectReason");
                        }
                    }

                    GetPageXMLRet = moPageXml;

                }
            }

            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "getPageXML", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetPageXML", ex, sProcessInfo));
                return null;
            }

            return GetPageXMLRet;

        }

        public XmlDocument BuildPageXML()
        {
            this.PerfMon.Log("Web", "BuildPageXML");
            XmlElement oPageElmt;
            string sProcessInfo = "";
            string sLayout = "Default";

            try
            {

                if (moPageXml.DocumentElement is null)
                {
                    moPageXml.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                    oPageElmt = moPageXml.CreateElement("Page");
                    moPageXml.AppendChild(oPageElmt);
                    GetRequestVariablesXml(ref oPageElmt);

                    GetSettingsXml(ref oPageElmt);
                }
                else
                {
                    oPageElmt = moPageXml.DocumentElement;
                }

                if (CheckLicence(moLicenceMode))
                {

                    SetPageLanguage();

                    long specialPageId = 0L;
                    if ((long)this.mnPageId == gnPageNotFoundId | (long)this.mnPageId == gnPageAccessDeniedId | (long)this.mnPageId == gnPageLoginRequiredId | (long)this.mnPageId == gnPageErrorId)
                    {
                        specialPageId = (long)this.mnPageId;
                    }

                    // add the page content
                    // TS moved this above setting page attributes as it now sets page id on page versions.
                    GetStructureXML("Site");

                    // if specialPageId then reset.
                    if (specialPageId > 0L)
                    {
                        this.mnPageId = (int)specialPageId;
                    }

                    if (string.IsNullOrEmpty(msException))
                    {

                        if (mnMailMenuId > 0L)
                        {
                            GetStructureXML("Newsletter", 0L, mnMailMenuId, true, nCloneContextId: 0L);
                        }
                        if (mnSystemPagesId > 0L & !(mnSystemPagesId == RootPageId))
                        {
                            GetStructureXML(0L, mnSystemPagesId, 0L, "SystemPages", true, true, false, true, false, "", "");
                        }

                        // 
                        if (!string.IsNullOrEmpty(gcMenuContentCountTypes))
                        {
                            foreach (var contentType in Strings.Split(gcMenuContentCountTypes, ","))
                                AddContentCount((XmlElement)moPageXml.SelectSingleNode("/Page/Menu"), Strings.Trim(contentType));
                        }

                        if (!string.IsNullOrEmpty(gcMenuContentBriefTypes))
                        {
                            foreach (var contentType in Strings.Split(gcMenuContentBriefTypes, ","))
                                AddContentBrief((XmlElement)moPageXml.SelectSingleNode("/Page/Menu"), Strings.Trim(contentType));
                        }

                        // establish the artid
                        if (!(this.moRequest.QueryString.Count == 0))
                        {
                            if (!string.IsNullOrEmpty(this.moRequest["artid"]))
                            {

                                object sArtId = Regex.Replace("0" + this.moRequest["artid"], @"[^\d]", "");
                                // check not too large for an int
                                int argresult = 0;
                                if (int.TryParse(Conversions.ToString(sArtId), out argresult))
                                {
                                    this.mnArtId = Conversions.ToInteger(sArtId);
                                }

                            }
                        }

                        // set the page attributes
                        if (this.mnArtId > 0)
                        {
                            oPageElmt.SetAttribute("artid", this.mnArtId.ToString());
                        }

                        oPageElmt.SetAttribute("id", this.mnPageId.ToString());
                        if (this.moSession != null)
                        {
                            if (Conversions.ToBoolean(Operators.AndObject(Conversions.ToInteger(Operators.ConcatenateObject("0", this.moSession["LogonRedirectId"])) > 0, !Operators.ConditionalCompareObjectEqual(this.moSession["LogonRedirectId"], this.mnPageId, false))))
                            {
                                oPageElmt.SetAttribute("requestedId", Conversions.ToString(this.moSession["LogonRedirectId"]));
                            }
                        }

                        oPageElmt.SetAttribute("cacheMode", mnPageCacheMode.ToString());

                        if (!string.IsNullOrEmpty(gcEwBaseUrl))
                        {
                            oPageElmt.SetAttribute("baseUrl", gcEwBaseUrl);
                        }

                        // introduce the layout
                        if (sLayout == "Default")
                        {
                            sLayout = this.moDbHelper.getPageLayout((long)this.mnPageId);
                        }
                        oPageElmt.SetAttribute("layout", sLayout);
                        oPageElmt.SetAttribute("pageExt", this.moConfig["pageExt"]);
                        oPageElmt.SetAttribute("cssFramework", this.moConfig["cssFramework"]);

                        if (this.mnPageId > 0)
                        {
                            GetContentXml(ref oPageElmt);
                            // only get the detail if we are not on a system page and not at root
                            if (RootPageId == this.mnPageId | !((long)this.mnPageId == gnPageNotFoundId | (long)this.mnPageId == gnPageAccessDeniedId | (long)this.mnPageId == gnPageLoginRequiredId | (long)this.mnPageId == gnPageErrorId))
                            {


                                long validatedVersion = 0L;

                                if (this.mbPreview & !string.IsNullOrEmpty(this.moRequest["verId"]))
                                {
                                    validatedVersion = Conversions.ToLong(this.moRequest["verId"]);
                                }

                                if (this.mbPreview == false & !string.IsNullOrEmpty(this.moRequest["verId"]))
                                {
                                    if ((Tools.Encryption.RC4.Decrypt(this.moRequest["previewKey"], this.moConfig["SharedKey"]) ?? "") == (this.moRequest["verId"] ?? ""))
                                    {

                                        validatedVersion = Conversions.ToLong(this.moRequest["verId"]);

                                    }
                                }


                                if (Conversions.ToBoolean(validatedVersion))
                                {
                                    moContentDetail = GetContentDetailXml(oPageElmt, bCheckAccessToContentLocation: true, nVersionId: Conversions.ToLong(this.moRequest["verId"]));
                                }
                                else if (Strings.LCase(this.moConfig["AllowContentDetailAccess"]) == "On")
                                {
                                    moContentDetail = GetContentDetailXml(oPageElmt);
                                }
                                else
                                {
                                    moContentDetail = GetContentDetailXml(oPageElmt, bCheckAccessToContentLocation: true);
                                }
                            }

                            if (Strings.LCase(this.moConfig["CheckDetailPath"]) == "on" & this.mbAdminMode == false & this.mnArtId > 0 & (mcOriginalURL.Contains("-/") | mcOriginalURL.Contains("/Item")))
                            {
                                if (oPageElmt.SelectSingleNode("ContentDetail/Content/@name") != null)
                                {
                                    string cContentDetailName = oPageElmt.SelectSingleNode("ContentDetail/Content/@name").InnerText;
                                    cContentDetailName = Tools.Text.CleanName(cContentDetailName, false, true);
                                    string RequestedContentName = "";
                                    string myQueryString = "";

                                    if (mcOriginalURL.Contains("?"))
                                    {
                                        myQueryString = mcOriginalURL.Substring(mcOriginalURL.LastIndexOf("?"));
                                        mcOriginalURL = mcOriginalURL.Substring(0, mcOriginalURL.LastIndexOf("?"));
                                    }

                                    mcOriginalURL = mcOriginalURL.TrimEnd('?');
                                    if (mcOriginalURL.Contains("-/"))
                                    {
                                        RequestedContentName = Strings.Right(mcOriginalURL, mcOriginalURL.Length - Strings.InStr(mcOriginalURL, "-/") - 1);
                                    }

                                    if ((RequestedContentName ?? "") != (cContentDetailName ?? ""))
                                    {
                                        // Change to redirect to correct URL, automatic redirects for content name changes
                                        if (string.IsNullOrEmpty(RequestedContentName))
                                        {
                                            if (mcOriginalURL.EndsWith("-/"))
                                            {
                                                mbRedirectPerm = Conversions.ToString(true);
                                                msRedirectOnEnd = mcOriginalURL + cContentDetailName;
                                            }
                                            else
                                            {
                                                string PathBefore = mcOriginalURL.Substring(0, mcOriginalURL.LastIndexOf("/Item"));
                                                mbRedirectPerm = Conversions.ToString(true);
                                                msRedirectOnEnd = PathBefore + "/" + this.mnArtId + "-/" + cContentDetailName;
                                            }
                                        }

                                        else
                                        {
                                            string PathBefore = mcOriginalURL.Substring(0, mcOriginalURL.Length - RequestedContentName.Length);
                                            mbRedirectPerm = Conversions.ToString(true);
                                            msRedirectOnEnd = PathBefore + cContentDetailName;
                                        }
                                        if (!string.IsNullOrEmpty(myQueryString))
                                        {
                                            msRedirectOnEnd = msRedirectOnEnd + myQueryString;
                                        }
                                    }
                                }
                            }


                            this.CheckMultiParents(ref oPageElmt, this.mnPageId);
                        }
                        else
                        {
                            mnProteanCMSError = 1005L;
                        }
                        if (mnProteanCMSError > 0L)
                        {
                            GetErrorXml(ref oPageElmt);
                        }

                        SetPageLanguage(); // TS not sure why this is being called twice ???

                        oPageElmt.SetAttribute("expireDate", Tools.Xml.XmlDate(mdPageExpireDate));
                        oPageElmt.SetAttribute("updateDate", Tools.Xml.XmlDate(mdPageUpdateDate));
                        oPageElmt.SetAttribute("userIntegrations", gbUserIntegrations.ToString().ToLower());
                        oPageElmt.SetAttribute("pageViewDate", Tools.Xml.XmlDate(mdDate));
                        oPageElmt.SetAttribute("previewHidden", Conversions.ToString(Interaction.IIf(this.mbPreviewHidden, "on", "off")));

                        // Assess if this page is a cloned page.
                        // Is it a direct clone (in which case the page id will have a @clone node in the Menu Item
                        // Or is it a child of a cloned page (in which case the page id MenuItem will have a @cloneparent node that matches the requested context, stored in mnCloneContextPageId)
                        if (gbClone && oPageElmt.SelectSingleNode("//MenuItem[@id = /Page/@id and (@clone > 0 or (@cloneparent='" + mnCloneContextPageId + "' and @cloneparent > 0 ))]") != null)
                        {
                            // If the current page is a cloned page
                            oPageElmt.SetAttribute("clone", "true");
                        }
                    }
                }
                else
                {
                    // Invalid Licence
                    mnProteanCMSError = 1008L;
                    if (mnProteanCMSError > 0L)
                    {
                        GetErrorXml(ref oPageElmt);
                    }

                }


                return moPageXml;
            }

            catch (Exception ex)
            {

                // returnException(msException, mcModuleName, "buildPageXML", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "BuildPageXML", ex, sProcessInfo));
                return null;
            }

        }

        /// <summary>
        ///   Generates feeds for page content.
        /// </summary>
        /// <param name="contentSchema">The content schema to search for</param>
        /// <param name="showRelated">Add related content to the content retrieved</param>
        /// <param name="pageId">The page ID to search for. If 0, then search all available pages.</param>
        /// <param name="includeChildPages">If a page ID is specified, then this determines whether or not to go down the site tree as well</param>
        /// <param name="mimeType">Optional: The mimeType to output in the HTTP response ContentType Header (default is "text/xml")</param>
        /// <param name="feedName">Optional: A name to determine the folder to look in for the feed XSL - default is "generic"</param>
        /// <remarks>
        /// <list>
        /// <listheader>The XSL file name is predetermined by the options that are passed through and have the following precedence:</listheader>
        /// <item>/xsl/feeds/&lt;feedname&gt;/&lt;contentSchema&gt;-&lt;mimeType&gt;.xsl  (then check common folders)</item>
        /// <item>/xsl/feeds/generic/&lt;contentSchema&gt;-&lt;mimeType&gt;.xsl  (then check common folders)</item>
        /// <item>/xsl/feeds/&lt;feedname&gt;/&lt;contentSchema&gt;.xsl  (then check common folders)</item>
        /// <item>/xsl/feeds/generic/&lt;contentSchema&gt;.xsl  (then check common folders)</item>
        /// </list>
        /// </remarks>
        public virtual void GetFeedXML(string contentSchema, bool showRelated, int pageId, bool includeChildPages, string mimeType = System.Net.Mime.MediaTypeNames.Text.Xml, string feedName = "generic", bool bContentDetail = false, string cRelatedSchemasToShow = "", int nGroupId = 0)
        {
            string methodName = "GetFeedXML(string,boolean,integer,boolean,string,string)";
            string processInfo = "";
            this.PerfMon.Log("Web", methodName);

            try
            {
                // TS 21-06-2017 Moved from New() as not required for cached pages I think.
                Open();
                // Determine and set the mimeType
                // AJG - application/xhtml+xml is interpretly correctly by many browsers, especially IE
                // Default Content type returned to be XHTML (to clear warning on XHTML validator)
                processInfo = "Determine the mimeType";
                mimeType = Conversions.ToString(Interaction.IIf(ValidatedOutputXml | string.IsNullOrEmpty(mimeType), System.Net.Mime.MediaTypeNames.Text.Xml, Strings.Replace(mimeType, " ", "+")));
                mcContentType = mimeType;
                this.moResponse.ContentType = mcContentType;

                // Generate the XML
                if (string.IsNullOrEmpty(moPageXml.OuterXml))
                {
                    processInfo = "Generate the XML";


                    // If page is not 0 and the contentSchema is all then we need to generate the page XML as normal.
                    if (pageId > 0 & contentSchema.ToLower() == "all")
                    {
                        this.mnPageId = pageId;
                        BuildPageXML();
                    }
                    else
                    {
                        BuildFeedXML(contentSchema, showRelated, pageId, includeChildPages, bContentDetail, cRelatedSchemasToShow, nGroupId);
                    }
                }

                // Add the responses
                CommitResponsesToPage();

                // Set the page encoding
                this.moResponse.HeaderEncoding = System.Text.Encoding.UTF8;
                this.moResponse.ContentEncoding = System.Text.Encoding.UTF8;

                // Set error header if errors were encountered building the XML
                if (!string.IsNullOrEmpty(msException))
                {
                    // If there is an error we can add our own header.
                    // this means external programs can check that there is an error
                    this.moResponse.AddHeader("X-ProteanCMSError", "An Error has occured");
                }

                // AJG - Commenting out this, as building feed xml shouldn't include any content processing, 
                // which is how redirects are normally generated.
                // If msRedirectOnEnd <> "" Then
                // moResponse.Redirect(msRedirectOnEnd)
                // Exit Sub
                // End If


                if (ValidatedOutputXml)
                {

                    // Output as XML
                    this.moResponse.Write("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + moPageXml.OuterXml);
                }
                else
                {

                    // Load the XSL file

                    // The filename has the following checks and preferences:

                    // /xsl/feeds/<feedname>/<contentSchema>-<mimeType>.xsl  (then check common folders)
                    // /xsl/feeds/generic/<contentSchema>-<mimeType>.xsl  (then check common folders)
                    // /xsl/feeds/<feedname>/<contentSchema>.xsl  (then check common folders)
                    // /xsl/feeds/generic/<contentSchema>.xsl  (then check common folders)
                    processInfo = "Load the XSL file";


                    string pathprefix = "/xsl/feeds/";
                    var rootPaths = new ArrayList();
                    var paths = new ArrayList();
                    string mimeTypeFlattened = new Regex("[^A-Z0-9]", RegexOptions.IgnoreCase).Replace(mimeType, "-");

                    // Determine a default feedName, just in case it's blank
                    feedName = Conversions.ToString(Interaction.IIf(string.IsNullOrEmpty(feedName), "generic", feedName));

                    // Add the paths to an array
                    rootPaths.Add(pathprefix + feedName + "/" + contentSchema + "-" + mimeTypeFlattened + ".xsl");
                    if (feedName != "generic")
                        rootPaths.Add(pathprefix + "generic/" + contentSchema + "-" + mimeTypeFlattened + ".xsl");
                    rootPaths.Add(pathprefix + feedName + "/" + contentSchema + ".xsl");
                    if (feedName != "generic")
                        rootPaths.Add(pathprefix + "generic/" + contentSchema + ".xsl");
                    rootPaths.Add(pathprefix + feedName + "/generic.xsl");
                    if (feedName != "generic")
                        rootPaths.Add(pathprefix + "generic/generic.xsl");

                    // Add the common folders to the paths
                    foreach (string path in rootPaths)
                    {
                        paths.Add(path);
                        foreach (string commonFolders in maCommonFolders)
                            paths.Add("/" + commonFolders.Trim(Conversions.ToChar(@"/\")) + path);
                    }
                    rootPaths = null;

                    // Now check all the files
                    string xslFile = "";
                    foreach (string path in paths)
                    {
                        // Security feature: Don't check any paths that have ".."
                        if (!path.Contains(".."))
                        {
                            if (File.Exists(this.goServer.MapPath(path)))
                            {
                                xslFile = path;
                                break;
                            }
                        }
                    }


                    // Only transform the feed if we have a file
                    if (!string.IsNullOrEmpty(xslFile))
                    {

                        mcEwSiteXsl = xslFile;


                        // Transform PageXML using XSLT
                        processInfo = "Transform PageXML using XSLT";
                        string styleFile = this.goServer.MapPath(mcEwSiteXsl);
                        var argaWeb = this;
                        var oTransform = new Protean.XmlHelper.Transform(ref argaWeb, styleFile, gbCompiledTransform, 120000L);
                        this.PerfMon.Log("Web", "GetFeedXML-startxsl");
                        oTransform.mbDebug = gbDebug;
                        oTransform.ProcessTimed(moPageXml, ref this.moResponse);
                        this.PerfMon.Log("Web", "GetFeedXML-endxsl");
                        oTransform.Close();
                        oTransform = (Protean.XmlHelper.Transform)null;
                    }

                    else
                    {
                        throw new IOException("Could not find a valid feed file");
                    }


                }

                moPageXml = null;
                Close();
            }

            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, methodName, ex, processInfo));
                this.moResponse.Write(msException);
                // Finalize();
            }
            this.PerfMon.Write();
        }

        /// <summary>
        /// [Deprecated] This gets the feed XML for all available content of a certain schema, 
        /// using the google folder of feed xsls to transform the data. 
        /// </summary>
        /// <param name="contentSchema">The content schema to search for</param>
        /// <param name="showRelated">Add related content to the content retrieved</param>
        /// <remarks></remarks>
        public virtual void GetFeedXML(string contentSchema, bool showRelated, bool blnContentDetail = false, string cRelatedSchemasToShow = "", int nGroupId = 0)
        {
            string methodName = "GetFeedXML(string,boolean)";
            string processInfo = "";
            this.PerfMon.Log("Web", methodName);
            try
            {
                GetFeedXML(contentSchema, showRelated, 0, true, "text/xml", "google", blnContentDetail, cRelatedSchemasToShow, nGroupId);
            }
            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, methodName, ex, processInfo));
            }
        }

        /// <summary>
        /// Gets XML optimised for Feeds
        /// </summary>
        /// <param name="contentSchema">The content schema to search for</param>
        /// <param name="showRelated">Add related content to the content retrieved</param>
        /// <param name="pageId">The page ID to search for. If 0, then search all available pages.</param>
        /// <param name="includeChildPages">If a page ID is specified, then this determines whether or not to go down the site tree as well</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public XmlDocument BuildFeedXML(string contentSchema, bool showRelated, int pageId, bool includeChildPages, bool blnContentDetail = false, string cRelatedSchemasToShow = "", int nGroupId = 0)
        {
            string methodName = "BuildFeedXML(string,boolean,integer,boolean)";
            this.PerfMon.Log("Web", methodName);

            XmlElement oPageElmt;
            string processInfo = "";
            //string sLayout = "default";
            XmlElement oElmt;

            try
            {

                if (moPageXml.DocumentElement is null)
                {
                    moPageXml.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                    oPageElmt = moPageXml.CreateElement("Page");
                    moPageXml.AppendChild(oPageElmt);
                    GetRequestVariablesXml(ref oPageElmt);

                    GetSettingsXml(ref oPageElmt);
                }
                else
                {
                    oPageElmt = moPageXml.DocumentElement;
                }

                oPageElmt.SetAttribute("id", this.mnPageId.ToString());
                oPageElmt.SetAttribute("cacheMode", mnPageCacheMode.ToString());

                if (!string.IsNullOrEmpty(gcEwBaseUrl))
                {
                    oPageElmt.SetAttribute("baseUrl", gcEwBaseUrl);
                }

                // Get the site structure, this will determine what can and can't be seen 
                GetStructureXML("Site");

                // Get the page scope.
                // Let's set a few rules
                // 1. If page id = 0 then we assume all pages
                // 2. If page > 0 then get that page, and its children if includechildpages is set
                // We can build an xpath on this logic
                string pageFinderXPath = "descendant-or-self::MenuItem";
                if (pageId > 0)
                    pageFinderXPath += "[@id=" + pageId + "]";
                if (pageId > 0 & includeChildPages)
                    pageFinderXPath += "/descendant-or-self::MenuItem";

                // Build the page list
                string pageIds = "";
                foreach (XmlElement currentOElmt in moPageXml.SelectNodes(pageFinderXPath))
                {
                    oElmt = currentOElmt;
                    pageIds += "'" + oElmt.GetAttribute("id") + "',";
                }
                pageIds = pageIds.TrimEnd(',');



                // Only process the XML if we have page ids to work with
                if (!string.IsNullOrEmpty(pageIds))
                {

                    // Note, for optimisation, we can call this ***without*** permissions checks,
                    // because we have already determined permissions through the sitestructure call
                    // and we are limiting the results by page id.
                    // Note: This brings back any FeedOutput content that happens to be placed on any of the pages.
                    // This can be used for configuring RSS feeds (see Wellards)
                    int pageSize = default;
                    int pageNumber = default;

                    if (!string.IsNullOrEmpty(this.moConfig["LimitFeed"]))
                    {
                        pageSize = Conversions.ToInteger(this.moConfig["LimitFeed"]);
                        pageNumber = 1;
                    }

                    if (Conversions.ToDouble(this.moRequest["pageSize"]) > 0d)
                    {
                        pageSize = Conversions.ToInteger(this.moRequest["pageSize"]);
                        pageNumber = Conversions.ToInteger(this.moRequest["pageNumber"]);
                        if (pageNumber == 0)
                            pageNumber = 1;
                    }

                    if (!string.IsNullOrEmpty(this.moConfig["feedRelatedBriefDepth"]))
                    {
                        gnShowRelatedBriefDepth = Conversions.ToInteger(this.moConfig["feedRelatedBriefDepth"]);
                    }

                    string productGrpSql = "";
                    if (nGroupId > 0)
                    {
                        productGrpSql = " and nContentKey IN (Select nContentId from tblCartCatProductRelations where nCatId = " + nGroupId + ")";
                    }

                    XmlElement dummyPageDetail = null;
                    XmlElement dummyContentNode = null;
                    int dummycount = 0;
                    this.GetPageContentFromSelect("cContentSchemaName IN (" + Tools.Database.SqlString(contentSchema) + ",'FeedOutput') and CL.nStructId IN(" + pageIds + ")" + productGrpSql, ref dummycount, ref dummyContentNode, ref dummyPageDetail, true, bIgnorePermissionsCheck: true, nReturnRows: pageSize, cOrderBy: "dInsertDate DESC", bContentDetail: blnContentDetail, pageNumber: (long)pageNumber, cShowSpecificContentTypes: cRelatedSchemasToShow);

                    string ProductTypes = this.moConfig["ProductTypes"];
                    if (string.IsNullOrEmpty(ProductTypes))
                        ProductTypes = "Product,SKU";

                    if (this.moConfig["Cart"] == "on")
                    {
                        var argaWeb = this;
                        oEc = new Cms.Cart(ref argaWeb);
                    }

                    // NB put the parId code here?
                    foreach (XmlElement currentOElmt1 in oPageElmt.SelectNodes("/Page/Contents/Content"))
                    {
                        oElmt = currentOElmt1;
                        processInfo = "Cleaning parId for: " + oElmt.OuterXml;
                        string parId = oElmt.GetAttribute("parId");
                        if (Conversions.ToBoolean(parId.Contains(",")))
                        {
                            parId = oElmt.GetAttribute("parId").Split(',')[1];
                        }
                        if (Conversions.ToLong(Operators.ConcatenateObject("0", parId)) > 0L)
                        {
                            // processInfo = "Cleaning parId for: " & oElmt.OuterXml
                            long primaryParId = 0L;

                            if (Strings.InStr(oElmt.GetAttribute("parId"), ",") > 0)
                            {

                                string[] aParIds = oElmt.GetAttribute("parId").ToString().Split(',');

                                foreach (string cParId in aParIds)
                                {
                                    if (oPageElmt.SelectSingleNode("descendant-or-self::MenuItem[@id='" + cParId + "']") != null)
                                    {
                                        primaryParId = Conversions.ToLong(cParId);
                                    }
                                }

                                oElmt.SetAttribute("parId", primaryParId.ToString());

                            }

                        }

                        if (this.moConfig["Cart"] == "on")
                        {
                            // Get the shipping costs
                            string feedShowShippingOptions = this.moConfig["feedShowShippingOptions"];
                            if (ProductTypes.Contains(oElmt.GetAttribute("type")) & feedShowShippingOptions == "on")
                            {

                                string strfeedWeightXpath = this.moConfig["feedWeightXpath"];
                                string feedPriceXpath = this.moConfig["feedPriceXpath"];

                                var xmlNodeWeight = oElmt.SelectSingleNode(this.moConfig["feedWeightXpath"]);
                                var xmlNodePrice = oElmt.SelectSingleNode(this.moConfig["feedPriceXpath"]);

                                if (xmlNodeWeight != null & xmlNodeWeight != null)
                                {
                                    string nWeight = xmlNodeWeight.InnerText;
                                    string nPrice = xmlNodePrice.InnerText;
                                    if (string.IsNullOrEmpty(nWeight))
                                        nWeight = "0";
                                    if (Information.IsNumeric(nWeight) & Information.IsNumeric(nPrice))
                                    {
                                        oEc.AddShippingCosts(ref oElmt, Conversions.ToDouble(nPrice).ToString(), Conversions.ToDouble(nWeight).ToString());
                                    }
                                }

                            }
                        }


                    }

                    if (this.moConfig["Cart"] == "on")
                    {
                        oEc.close();
                        oEc = (Cms.Cart)null;
                    }
                    // If showRelated Then
                    // DbHelper.addBulkRelatedContent(oPageElmt.SelectSingleNode("/Page/Contents"))

                    // For Each oElmt In oPageElmt.SelectNodes("/Page/Contents/Content")
                    // moDbHelper.addRelatedContent(oElmt, oElmt.GetAttribute("id"), False, cRelatedSchemasToShow)
                    // Next
                    // End If

                }

                SetPageLanguage();
                oPageElmt.SetAttribute("expireDate", Tools.Xml.XmlDate(mdPageExpireDate));
                oPageElmt.SetAttribute("updateDate", Tools.Xml.XmlDate(mdPageUpdateDate));

                return moPageXml;
            }

            catch (Exception ex)
            {

                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, methodName, ex, processInfo));
                return null;
            }

        }

        /// <summary>
        /// Gets XML optimised for feeds, retrieving all content of a certain schema, plus optionally its related content.
        /// </summary>
        /// <param name="contentSchema">The content schema to search for</param>
        /// <param name="showRelated">Add related content to the content retrieved</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public XmlDocument BuildFeedXML(string contentSchema, bool showRelated)
        {
            string methodName = "BuildFeedXML(string,boolean)";
            this.PerfMon.Log("Web", methodName);

            try
            {

                return BuildFeedXML(contentSchema, showRelated, 0, true);
            }

            catch (Exception ex)
            {

                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, methodName, ex, ""));
                return null;
            }

        }

        public void GetAjaxHTML(string sAjaxCmd = "")
        {

            this.PerfMon.Log("Web", "GetAjaxHTML");
            string sProcessInfo = "";
            try
            {
                if (!string.IsNullOrEmpty(this.moRequest["AjaxCmd"]))
                {
                    sAjaxCmd = this.moRequest["AjaxCmd"];
                }

                GetAjaxXML(sAjaxCmd);


                var argaWeb = this;
                var moAdmin = new Cms.Admin(ref argaWeb);
                // Dim oAdmin As Admin = New Admin(Me)
                // Dim oAdmin As Protean.Cms.Admin = New Protean.Cms.Admin(Me)
                moAdmin.open(moPageXml);
                if (Conversions.ToInteger(Operators.ConcatenateObject("0", this.moSession["nUserId"])) > 0)
                {
                    moAdmin.GetPreviewMenu();
                }

                moAdmin.close();
                moAdmin = (Cms.Admin)null;


                sProcessInfo = "Transform PageXML using XSLT";

                // make sure each requests gets a new one.
                this.moResponse.Expires = 0;

                this.moResponse.HeaderEncoding = System.Text.Encoding.UTF8;
                this.moResponse.ContentEncoding = System.Text.Encoding.UTF8;
                // Default Content type returned to be XHTML (to clear warning on XHTML validator)
                this.moResponse.ContentType = "text/html"; // "application/xhtml+xml"

                if (!string.IsNullOrEmpty(msException))
                {
                    // If there is an error we can add our own header.
                    // this means external programs can check that there is an error
                    this.moResponse.AddHeader("X-ProteanCMSError", "An Error has occured");
                }

                if (this.mbOutputXml == true)
                {
                    this.moResponse.ContentType = "text/xml";
                    this.moResponse.Write("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + moPageXml.OuterXml);
                }
                else
                {

                    string styleFile = this.goServer.MapPath(mcEwSiteXsl);

                    if (!string.IsNullOrEmpty(this.moRequest["recompile"]))
                    {
                        this.moCtx.Application[styleFile] = (object)null;
                    }

                    var argaWeb1 = this;
                    var oTransform = new Protean.XmlHelper.Transform(ref argaWeb1, styleFile, gbCompiledTransform);

                    this.PerfMon.Log("Web", "GetPageHTML-startxsl");
                    oTransform.mbDebug = gbDebug;
                    oTransform.ProcessTimed(moPageXml, ref this.moResponse);
                    this.PerfMon.Log("Web", "GetPageHTML-endxsl");
                    oTransform.Close();
                    oTransform = (Protean.XmlHelper.Transform)null;

                }

                moPageXml = null;
                Close();
            }

            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetAjaxHTML", ex, sProcessInfo));
                this.moResponse.Write(msException);
                //Finalize();
            }

            this.PerfMon.Write();

        }

        public virtual XmlDocument GetAjaxXML(string AjaxCmd)
        {
            this.PerfMon.Log("Web", "GetAjaxXML");
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
                    GetRequestVariablesXml(ref oPageElmt);

                    GetSettingsXml(ref oPageElmt);
                    var userXml = GetUserXML();
                    if (userXml != null)
                    {
                        moPageXml.DocumentElement.AppendChild(GetUserXML());
                    }
                }

                else
                {
                    oPageElmt = moPageXml.DocumentElement;
                }

                // establish the artid
                if (!string.IsNullOrEmpty(this.moRequest["artid"]))
                {
                    this.mnArtId = Conversions.ToInteger(this.moRequest["artid"]);
                }
                // set the page attributes
                if (this.mnArtId > 0)
                {
                    oPageElmt.SetAttribute("artid", this.mnArtId.ToString());
                }
                long nPageId = 0L; // CLng("0" & moRequest("pgid"))
                string NodeId = this.moRequest["pgid"];
                if (Information.IsNumeric(NodeId))
                {
                    nPageId = Conversions.ToLong("0" + this.moRequest["pgid"]);
                }

                long nContentParId = Conversions.ToLong("0" + this.moRequest["contentParId"]);
                if (nPageId > 0L)
                {
                    this.mnPageId = (int)nPageId;
                    if (nContentParId > 0L)
                    {
                        nPageId = 0L;
                    }
                }

                oPageElmt.SetAttribute("id", this.mnPageId.ToString());
                oPageElmt.SetAttribute("contentParId", nContentParId.ToString());
                oPageElmt.SetAttribute("cacheMode", mnPageCacheMode.ToString());
                oPageElmt.SetAttribute("cssFramework", this.moConfig["cssFramework"]);

                if (!string.IsNullOrEmpty(gcEwBaseUrl))
                {
                    oPageElmt.SetAttribute("baseUrl", gcEwBaseUrl);
                }

                switch (AjaxCmd ?? "")
                {
                    case "BespokeProvider":
                        {
                            // Dim assemblyInstance As [Assembly]
                            Type calledType;
                            Protean.ProviderSectionHandler moPrvConfig = (Protean.ProviderSectionHandler)WebConfigurationManager.GetWebApplicationSection("protean/bespokeProviders");
                            string providerName = this.moRequest["provider"];
                            var assemblyInstance = Assembly.Load(moPrvConfig.Providers[providerName].Type.ToString());

                            string classPath = this.moRequest["method"];

                            string methodName = Strings.Right(classPath, Strings.Len(classPath) - classPath.LastIndexOf(".") - 1);
                            classPath = Strings.Left(classPath, classPath.LastIndexOf("."));

                            calledType = assemblyInstance.GetType(classPath, true);
                            var o = Activator.CreateInstance(calledType);

                            var args = new object[1];
                            args[0] = this;

                            calledType.InvokeMember(methodName, BindingFlags.InvokeMethod, null, o, args);
                            break;
                        }

                    case "Edit":
                    case "Delete":
                        {

                            // TODO: We Need to confirm Permissions and Right before we allow this !!!!

                            Cms.Admin.AdminXforms moAdXfm = (Cms.Admin.AdminXforms)getAdminXform();
                            moAdXfm.open(moPageXml);

                            long nContentId = Conversions.ToLong("0" + this.moRequest["id"]);
                            bool bUserValid = false;
                            long nPagePermissionCheck = nPageId;
                            long nContentPermissionCheck = nContentId;
                            bool bResetUser = false;

                            if (this.mbPreview & this.moConfig["inlineContentPermissions"] == "AdminUser")
                            {
                                // commented out because it was breaking PSMG edit jobs
                                this.mnUserId = Conversions.ToInteger(this.moSession["nUserId"]);
                            }

                            if (this.mnUserId == 0)
                            {
                                this.mnUserId = Conversions.ToInteger(this.moConfig["NonAuthUserID"]);
                                this.moDbHelper.mnUserId = (long)this.mnUserId;
                                bResetUser = true;
                            }

                            moAdXfm.mnUserId = (long)this.mnUserId;
                            this.moDbHelper.mnUserId = (long)this.mnUserId;

                            // Let's check permissions - but first let's accomodate orphan content
                            // Orhpan content has a page id of 0, but is related to another piece of content.
                            // We need to track down the ultimate related content primary page id.
                            if (nPagePermissionCheck == 0L & nContentParId > 0L)
                            {
                                var hUnorphanedAncestor = this.moDbHelper.getUnorphanedAncestor((int)nContentParId, 1L);
                                if (hUnorphanedAncestor != null)
                                {
                                    nPagePermissionCheck = Conversions.ToLong(hUnorphanedAncestor["page"]);
                                    nContentPermissionCheck = Conversions.ToLong(hUnorphanedAncestor["id"]);
                                }
                            }

                            Cms.dbHelper.PermissionLevel nContentPermLevel;
                            if (nContentId > 0L & nContentParId != 0L)
                            {
                                nContentPermLevel = this.moDbHelper.getContentPermissionLevel(nContentPermissionCheck, nPagePermissionCheck);
                            }
                            else
                            {
                                nContentPermLevel = this.moDbHelper.getPagePermissionLevel(nPagePermissionCheck);
                            }

                            // Check if the permissions are valid
                            bUserValid = Cms.dbHelper.CanAddUpdate(nContentPermLevel) & this.mnUserId > 0;

                            if (this.moRequest["type"] != null)
                            {
                                if (this.moRequest["type"].ToLower() == "review")
                                {
                                    bUserValid = true; // set true for submitting review functionality
                                }
                            }

                            // We need to set this for version control
                            this.moDbHelper.CurrentPermissionLevel = nContentPermLevel;

                            if (bUserValid)
                            {

                                switch (AjaxCmd ?? "")
                                {
                                    case "Edit":
                                        {
                                            XmlElement xFrmContent;
                                            int argnReturnId = (int)nContentId;
                                            var tmp = this.moRequest;
                                            string argAlternateFormName = tmp["formName"];
                                            string zcReturnSchema = null;
                                            xFrmContent = moAdXfm.xFrmEditContent(nContentId, this.moRequest["type"], nPageId, this.moRequest["name"], false, nReturnId: ref argnReturnId, ref zcReturnSchema, AlternateFormName: ref argAlternateFormName, nVersionId: Conversions.ToLong("0" + this.moRequest["verId"]));
                                            nContentId = argnReturnId;
                                            if (moAdXfm.valid)
                                            {
                                                // if we have a parent releationship lets add it
                                                if (!string.IsNullOrEmpty(this.moRequest["contentParId"]))
                                                {
                                                    this.moDbHelper.insertContentRelation(Conversions.ToInteger(this.moRequest["contentParId"]), nContentId.ToString(), Conversions.ToBoolean(Interaction.IIf(this.moRequest["2way"] == "true", true, false)));
                                                }
                                                // simply output the content detail XML
                                                // As this is content that we must've been able to get,
                                                // we should be able to see it.
                                                // However if it's related content the page id may not be coming through (it's orphaned)
                                                // sooo... we may need to bodge the permissions if version control is on
                                                if (gbVersionControl)
                                                    mnUserPagePermission = Cms.dbHelper.PermissionLevel.AddUpdateOwn;

                                                if (!string.IsNullOrEmpty(this.moRequest["showParent"]))
                                                {
                                                    GetContentDetailXml(oPageElmt, Conversions.ToLong(this.moRequest["contentParId"]));
                                                }
                                                else
                                                {
                                                    GetContentDetailXml(oPageElmt, nContentId);
                                                }

                                                ClearPageCache();
                                            }

                                            else
                                            {
                                                // lets add the form to Content Detail
                                                var oPageDetail = moPageXml.CreateElement("ContentDetail");
                                                oPageElmt.AppendChild(oPageDetail);
                                                oPageDetail.AppendChild(xFrmContent);

                                                // lets add the page content
                                                if (this.mnPageId > 0)
                                                {
                                                    GetContentXml(ref oPageElmt);
                                                }
                                            }

                                            break;
                                        }

                                    case "Delete":
                                        {

                                            // remove the relevent content information
                                            XmlElement xFrmContent;
                                            xFrmContent = moAdXfm.xFrmDeleteContent(nContentId);
                                            if (moAdXfm.valid)
                                            {


                                                var oPageDetail = moPageXml.CreateElement("ContentDetail");
                                                oPageElmt.AppendChild(oPageDetail);
                                                oPageDetail.InnerXml = "<Content type=\" message\"><div>Content deleted successfully.</div></Content>";
                                                ClearPageCache();
                                            }

                                            else
                                            {
                                                // lets add the form to Content Detail
                                                var oPageDetail = moPageXml.CreateElement("ContentDetail");
                                                oPageElmt.AppendChild(oPageDetail);
                                                oPageDetail.AppendChild(xFrmContent);

                                                // lets add the page content
                                                if (this.mnPageId > 0)
                                                {
                                                    GetContentXml(ref oPageElmt);
                                                }
                                            }

                                            break;
                                        }

                                }
                            }
                            else
                            {
                                // raise error or do nothing
                                var oPageDetail = moPageXml.CreateElement("ContentDetail");
                                oPageElmt.AppendChild(oPageDetail);
                                oPageDetail.InnerXml = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("<Content type=\" error\"><div>Sorry you do not have permission to ", Interaction.IIf(AjaxCmd == "Delete", "delete", "update")), " this item, please contact the site administrator.</div></Content>"));

                            }

                            if (bResetUser)
                            {
                                this.mnUserId = 0;
                                this.moDbHelper.mnUserId = 0L;
                            }
                            // Production request: Add a menu to everything
                            oPageElmt.AppendChild(GetStructureXML((long)this.mnUserId));
                            break;
                        }


                    case "MenuNode":
                    case "GetStructureNode":
                    case "GetMoveNode":
                    case "GetMoveContent":
                    case "GetLocateNode":
                    case "GetAdvNode":
                    case "editStructurePermissions":
                        {

                            // Make sure admin mode is true and we don't need to check for permissions

                            // TS Force to Admin Mode because you might not be calling from the same application
                            // mbAdminMode = True

                            long expId = Conversions.ToLong(this.moRequest["pgid"]);
                            long nContextId = 0L;
                            if (!string.IsNullOrEmpty(this.moRequest["expid"]))
                            {
                                expId = Conversions.ToLong(this.moRequest["expid"]);
                            }

                            // Check for a context node
                            if (!string.IsNullOrEmpty(this.moRequest["context"]) && Information.IsNumeric(this.moRequest["context"]) && Conversions.ToLong(this.moRequest["context"]) > 0L)
                            {
                                nContextId = Conversions.ToLong(this.moRequest["context"]);
                            }

                            // make sure we don't check for permissions.
                            this.mbAdminMode = true;

                            // Note need to fix for newsletters.
                            var FullMenuXml = GetStructureXML(-1, RootPageId, nContextId);
                            long getLevel = 0L;

                            // Move the requested ID to the top.
                            if (FullMenuXml.SelectSingleNode("descendant-or-self::MenuItem[@id = " + expId + "]") is null)
                            {
                            }
                            else
                            {
                                getLevel = FullMenuXml.SelectNodes("descendant-or-self::MenuItem[@id = " + expId + "]/ancestor-or-self::MenuItem").Count;
                                FullMenuXml.ReplaceChild(FullMenuXml.SelectSingleNode("descendant-or-self::MenuItem[@id = " + expId + "]"), FullMenuXml.FirstChild);

                                FullMenuXml.SetAttribute("level", getLevel.ToString());
                            }

                            oPageElmt.AppendChild(FullMenuXml);
                            break;
                        }
                    case "GetFolderNode":
                        {
                            var oPageDetail = moPageXml.CreateElement("ContentDetail");
                            oPageElmt.AppendChild(oPageDetail);
                            var oFsh = new Protean.fsHelper(this.moCtx);
                            var libType = Protean.fsHelper.LibraryType.Image;
                            object thisEwCmd = "ImageLib";
                            switch (this.moRequest["LibType"] ?? "")
                            {
                                case "Media":
                                    {
                                        thisEwCmd = "MediaLib";
                                        libType = Protean.fsHelper.LibraryType.Media;
                                        break;
                                    }
                                case "Docs":
                                    {
                                        thisEwCmd = "DocsLib";
                                        libType = Protean.fsHelper.LibraryType.Documents;
                                        break;
                                    }
                            }

                            oFsh.initialiseVariables(libType);
                            oFsh.moPageXML = moPageXml;
                            oFsh.mcStartFolder = oFsh.mcStartFolder + this.moRequest["pgid"].Replace("~", @"\");
                            oPageDetail.AppendChild(oFsh.getDirectoryTreeXml(libType, "+++", this.moRequest["pgid"].Replace("~", @"\")));
                            moPageXml.DocumentElement.SetAttribute("ewCmd", Conversions.ToString(thisEwCmd));
                            break;
                        }

                    case "Search.MostPopular":
                        {

                            var popularSearches = this.moDbHelper.GetMostPopularSearches(5, this.moRequest["filter"] + "");

                            if (popularSearches != null)
                            {
                                var oPageDetail = moPageXml.CreateElement("ContentDetail");
                                oPageElmt.AppendChild(oPageDetail);
                                oPageDetail.AppendChild(popularSearches);
                            }

                            break;
                        }

                }


                // Process common actions
                CommonActions();


                // Check content versions
                if (gbVersionControl)
                    CheckContentVersions();

                SetPageLanguage();

                // Process language
                ProcessPageXMLForLanguage();

                oPageElmt.SetAttribute("layout", AjaxCmd);
                oPageElmt.SetAttribute("pageExt", this.moConfig["pageExt"]);

                // Add the responses
                CommitResponsesToPage();

                return moPageXml;
            }

            catch (Exception ex)
            {

                // returnException(msException, mcModuleName, "buildPageXML", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetAjaxXML", ex, sProcessInfo));
                return null;
            }

        }

        public string ReturnPageHTML(long nPageId = 0L, bool bReturnBlankError = false)
        {
            this.PerfMon.Log("Web", "ReturnPageHTML");
            string sProcessInfo = "Transform PageXML using XSLT";
            string cPageHTML;
            try
            {

                if (nPageId != 0L)
                    this.mnPageId = (int)nPageId;

                if (string.IsNullOrEmpty(moPageXml.OuterXml))
                {
                    GetPageXML();
                }

                if (moTransform is null)
                {
                    string styleFile = this.goServer.MapPath(mcEwSiteXsl);
                    this.PerfMon.Log("Web", "ReturnPageHTML - loaded Style");
                    var argaWeb = this;
                    moTransform = new Protean.XmlHelper.Transform(ref argaWeb, styleFile, false);
                }

                msException = "";

                moTransform.mbDebug = gbDebug;
                icPageWriter = new StringWriter();
                TextWriter argoWriter = icPageWriter;
                moTransform.ProcessTimed(moPageXml, ref argoWriter);
                icPageWriter = (StringWriter)argoWriter;

                cPageHTML = Strings.Replace(icPageWriter.ToString(), "<?xml version=\"1.0\" encoding=\"utf-16\"?>", "");
                cPageHTML = Strings.Replace(cPageHTML, "<?xml version=\"1.0\" encoding=\"UTF-8\"?>", "");

                if (bReturnBlankError & !string.IsNullOrEmpty(msException))
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

                returnException(ref msException, mcModuleName, "returnPageHtml", ex, gcEwSiteXsl, sProcessInfo, gbDebug);
                if (bReturnBlankError)
                {
                    return "";
                }
                else
                {
                    return msException;
                }
            }
            finally
            {
                icPageWriter.Dispose();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void AddCurrency()
        {
            string sProcessInfo = "PerfMon";
            this.PerfMon.Log("Web", "AddCurrency");
            // Isolated function to provide the facility to overload the cart when called from an overloaded .web
            try
            {
                if (gbCart)
                {

                    sProcessInfo = "Begin AddCurrency";
                    if (moCart is null)
                    {
                        var argaWeb = this;
                        moCart = new Cms.Cart(ref argaWeb);
                    }
                    moCart.SelectCurrency();
                    sProcessInfo = "End AddCurrency";

                }
            }
            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "AddCurrency", ex, sProcessInfo));
            }
        }

        public virtual void InitialiseCart()
        {
            string sProcessInfo = "PerfMon";
            this.PerfMon.Log("Web", "addCart");
            // Isolated function to provide the facility to overload the cart when called from an overloaded .web
            try
            {
                if (gbCart)
                {
                    if (this.moSession != null)
                    {
                        if (moCart is null)
                        {
                            // we should not hit this because addCurrency should establish it.
                            var argaWeb = this;
                            moCart = new Cms.Cart(ref argaWeb);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "addCart", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "AddCart", ex, sProcessInfo));
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual void AddCart()
        {
            string sProcessInfo = "PerfMon";
            this.PerfMon.Log("Web", "addCart");
            // Isolated function to provide the facility to overload the cart when called from an overloaded .web
            try
            {
                if (gbCart)
                {

                    sProcessInfo = "Begin Cart";
                    if (moCart is null)
                    {
                        // we should not hit this because addCurrency should establish it.
                        var argaWeb = this;
                        moCart = new Cms.Cart(ref argaWeb);
                    }
                    // reinitialize variables because we might've changed some
                    moCart.InitializeVariables();
                    moCart.apply();
                    // get any discount information for this page
                    var argoRootElmt = moPageXml.DocumentElement;
                    moDiscount.getAvailableDiscounts(ref argoRootElmt);
                    sProcessInfo = "End Cart";
                }
            }
            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "addCart", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "AddCart", ex, sProcessInfo));
            }
        }

        public virtual void ProcessReports()
        {
            string sProcessInfo = "PerfMon";
            this.PerfMon.Log("Web", "ProcessReports");
            // Isolated function to provide the facility to overload the cart when called from an overloaded .web
            try
            {
                if (gbReport)
                {
                    sProcessInfo = "Begin Report";
                    var argaWeb = this;
                    var oEr = new Cms.Report(ref argaWeb);
                    oEr.apply();
                    oEr.close();
                    oEr = (Cms.Report)null;
                    sProcessInfo = "End Report";
                }
            }
            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "ProcessReports", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ProcessReports", ex, sProcessInfo));

            }
        }

        public virtual void ProcessCalendar()
        {
            string sProcessInfo = "PerfMon";
            this.PerfMon.Log("Web", "ProcessCalendar");
            try
            {
                sProcessInfo = "Begin Calendar";
                var argaWeb = this;
                moCalendar = new Cms.Calendar(ref argaWeb);
                moCalendar.apply();
                moCalendar = (Cms.Calendar)null;
                sProcessInfo = "End Calendar";
            }
            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ProcessCalendar", ex, sProcessInfo));
            }
        }


        private void ProcessPolls()
        {
            this.PerfMon.Log("Web", "ProcessPolls");

            string sProcessInfo = "PerfMon";
            string sPollField = "";
            string sPollItemField = "";
            string sPollId = "";
            string sTable = "";
            string sSql = "";
            string sVoteFrequency = "";
            var dCurrentVotesExpiryDate = default(DateTime);
            var dPreviousVotesCreationDate = default(DateTime);
            bool bVoteOnce = false;
            string sVoteIdentifiers = "";

            string sEmail = "";
            string cValidationError = "";

            // Dim oDr As SqlDataReader

            bool bUseUserId = false;
            bool bUseCookies = false;
            bool bUseEmail = false;
            bool bUseIPAddress = false;

            bool bCanVote = true;
            var nVoteBlockReason = PollBlockReason.None;
            string cCookieName = "";
            string cLastVotedSql = string.Empty;

            var openDate = DateTime.MinValue;
            var closeDate = DateTime.MaxValue;


            bool bHasVoted = false;

            try
            {
                sProcessInfo = "Begin Poll Processing";

                foreach (XmlElement ocNode in moPageXml.SelectNodes("/Page/Contents/Content[@type='Poll']"))
                {

                    // Reset variables - this is why I would like this to be a class instead.
                    bCanVote = true;
                    nVoteBlockReason = PollBlockReason.None;
                    cValidationError = "";
                    cCookieName = "";
                    cLastVotedSql = "";
                    bVoteOnce = false;
                    sVoteIdentifiers = "";
                    bUseUserId = false;
                    bUseCookies = false;
                    bUseEmail = false;
                    bUseIPAddress = false;
                    openDate = DateTime.MinValue;
                    closeDate = DateTime.MaxValue;

                    // Look for required nodes
                    if (ocNode.SelectSingleNode("Restrictions/Frequency") is null | ocNode.SelectSingleNode("Restrictions/RegisteredVotersOnly") is null | ocNode.SelectSingleNode("Restrictions/Identifiers") is null)

                    {
                    }
                    // Required nodes are missing
                    else
                    {
                        // Set the config options for this poll
                        sVoteFrequency = ocNode.SelectSingleNode("Restrictions/Frequency").InnerText;
                        bUseUserId = ocNode.SelectSingleNode("Restrictions/RegisteredVotersOnly").InnerText == "true";
                        sVoteIdentifiers = ocNode.SelectSingleNode("Restrictions/Identifiers").InnerText;

                        // Check open and close dates
                        if (ocNode.SelectSingleNode("dOpenDate") != null && Information.IsDate(ocNode.SelectSingleNode("dOpenDate").InnerText))
                        {
                            openDate = Conversions.ToDate(ocNode.SelectSingleNode("dOpenDate").InnerText);
                        }
                        if (ocNode.SelectSingleNode("dCloseDate") != null && Information.IsDate(ocNode.SelectSingleNode("dCloseDate").InnerText))
                        {
                            closeDate = Conversions.ToDate(ocNode.SelectSingleNode("dCloseDate").InnerText);
                        }
                        if (openDate > DateTime.Now | closeDate < DateTime.Now)
                        {
                            bCanVote = false;
                            nVoteBlockReason = PollBlockReason.PollNotAvailable;
                        }


                        // Sort out the vote frequency
                        switch (sVoteFrequency ?? "")
                        {
                            case "once":
                                {
                                    bVoteOnce = true;
                                    break;
                                }
                            case "daily":
                                {
                                    dCurrentVotesExpiryDate = DateAndTime.DateAdd(DateInterval.Hour, 24d, DateTime.Now);
                                    dPreviousVotesCreationDate = DateAndTime.DateAdd(DateInterval.Hour, -24, DateTime.Now);
                                    break;
                                }
                            case "weekly":
                                {
                                    dCurrentVotesExpiryDate = DateAndTime.DateAdd(DateInterval.Day, 7d, DateTime.Now);
                                    dPreviousVotesCreationDate = DateAndTime.DateAdd(DateInterval.Day, -7, DateTime.Now);
                                    break;
                                }
                            case "monthly":
                                {
                                    dCurrentVotesExpiryDate = DateAndTime.DateAdd(DateInterval.Month, 1d, DateTime.Now);
                                    dPreviousVotesCreationDate = DateAndTime.DateAdd(DateInterval.Month, -1, DateTime.Now);
                                    break;
                                }
                        }


                        // If we're not using a registered user then set the other identifiers
                        if (!bUseUserId)
                        {
                            bUseCookies = sVoteIdentifiers.Contains("cookies");
                            bUseEmail = sVoteIdentifiers.Contains("email");
                            bUseIPAddress = sVoteIdentifiers.Contains("ipaddress");
                            if (bUseIPAddress)
                            {
                                gbIPLogging = true;
                                gbIPLogging = true;
                            }
                        }


                        // Set the metadata
                        sPollField = "nArtId";
                        sPollItemField = "nOtherId";
                        sTable = "tblActivityLog";
                        sPollId = ocNode.GetAttribute("id");
                        cCookieName = "pvote-" + sPollId;

                        // Get the poll items
                        var oCRNode = moPageXml.CreateElement("PollItems");
                        this.moDbHelper.addRelatedContent(ref oCRNode, Convert.ToInt32(sPollId), true);
                        ocNode.AppendChild(oCRNode);

                        // ===================================================================
                        // Check the restrictions
                        // ===================================================================
                        // This should tell us if the user can vote, and has voted.

                        // First - is this restricted to logged on users only
                        if (bUseUserId & !(this.mnUserId > 0))
                        {
                            bCanVote = false;
                            nVoteBlockReason = PollBlockReason.RegisteredUsersOnly;
                        }

                        if (bCanVote)
                        {

                            // There are two places that votes can be identified
                            // tblActivityLog - if User Id, IP address or email are being used
                            // Cookie - if cookies are being used.

                            // We need to check for both and then assess whether they can vote.
                            sSql = "";

                            if (bUseUserId)
                            {
                                sSql += " AND nUserDirId=" + this.mnUserId;
                            }
                            else
                            {
                                // First check if a cookie is being used - if it exists then we block voting.
                                if (bUseCookies && this.moRequest.Cookies[cCookieName] != null)
                                {
                                    bCanVote = false;
                                    nVoteBlockReason = PollBlockReason.CookieFound;
                                }

                                // If no cookie formulate the sql for the other restrictions.
                                if (bCanVote)
                                {

                                    if (bUseIPAddress)
                                    {
                                        this.moDbHelper.checkForIpAddressCol();
                                        sSql += " AND cIPAddress=" + Tools.Database.SqlString(Strings.Left(this.moRequest.ServerVariables["REMOTE_ADDR"], 15));
                                    }

                                    if (this.moRequest["poll-email"] != null)
                                    {
                                        sEmail = this.moRequest["poll-email"].ToString();
                                        if (bUseEmail)
                                            sSql += " AND cActivityDetail=" + Tools.Database.SqlString(sEmail);
                                    }
                                }
                            }

                            // Finally, check the tblActivityLog for votes
                            if (bCanVote)
                            {


                                // Get the last tblActivityVote
                                if (!string.IsNullOrEmpty(sSql))
                                {


                                    // Check for blocking
                                    // First get the scope of blocking, either Global or Poll
                                    string blockingScope = this.moConfig["PollBlockingScope"];
                                    string blockingScopeQuery = "";
                                    if (string.IsNullOrEmpty(blockingScope))
                                        blockingScope = "Global";
                                    if (blockingScope.ToLower() == "poll")
                                    {
                                        blockingScopeQuery = "AND nArtId=" + sPollId + " ";
                                    }

                                    // Check for blocks / exclusions
                                    blockingScopeQuery = "SELECT TOP 1 nActivityKey " + "FROM	dbo.tblActivityLog " + "WHERE   nActivityType=" + ((int)Cms.dbHelper.ActivityType.VoteExcluded).ToString() + " " + blockingScopeQuery + sSql;



                                    string blocked = Conversions.ToString(this.moDbHelper.GetDataValue(blockingScopeQuery, CommandType.Text, null, ""));
                                    if (!string.IsNullOrEmpty(blocked))
                                    {
                                        // Block has been found
                                        bCanVote = false;
                                        nVoteBlockReason = PollBlockReason.Excluded;
                                    }
                                    else
                                    {
                                        // Get the last tblActivityVote
                                        sSql = "SELECT  TOP 1 dDateTime As LastVoted " + "FROM	dbo.tblActivityLog " + "WHERE   nActivityType=" + ((int)Cms.dbHelper.ActivityType.SubmitVote).ToString() + " " + "AND nArtId=" + sPollId + " " + sSql;



                                        string cLastVoted = Conversions.ToString(this.moDbHelper.GetDataValue(sSql, CommandType.Text, null, ""));
                                        if (!string.IsNullOrEmpty(cLastVoted) && Information.IsDate(cLastVoted))
                                        {
                                            // We found a vote, check the date
                                            if (bVoteOnce || Conversions.ToDate(cLastVoted) > dPreviousVotesCreationDate)
                                            {
                                                // Block the vote
                                                bCanVote = false;
                                                nVoteBlockReason = PollBlockReason.LogFound;
                                            }
                                        }

                                    }


                                }
                            }
                        }




                        // ===================================================================
                        // Look for votes being submitted
                        // ===================================================================

                        if (bCanVote && this.moRequest["pollsubmit-" + sPollId] != null && !string.IsNullOrEmpty(this.moRequest["pollsubmit-" + sPollId].ToString()) && this.moRequest["polloption-" + sPollId] != null)


                        {

                            string sResult = this.moRequest["polloption-" + sPollId].ToString();

                            sEmail = "";
                            if (this.moRequest["poll-email"] != null)
                            {
                                sEmail = this.moRequest["poll-email"].ToString();
                            }

                            bHasVoted = true;

                            if (!bUseEmail & string.IsNullOrEmpty(sEmail) | Tools.Text.IsEmail(sEmail))
                            {
                                this.moDbHelper.logActivity(Cms.dbHelper.ActivityType.SubmitVote, (long)this.mnUserId, (long)this.mnPageId, (long)Convert.ToInt16(sPollId), (long)Convert.ToInt16(sResult), sEmail);

                                if (bUseCookies)
                                {
                                    var oCookie = new System.Web.HttpCookie(cCookieName, "voted");
                                    oCookie.Expires = dCurrentVotesExpiryDate;
                                    this.moResponse.Cookies.Add(oCookie);
                                }


                                bCanVote = false;
                                nVoteBlockReason = PollBlockReason.JustVoted;
                            }
                            else if (bUseEmail & string.IsNullOrEmpty(sEmail))
                            {
                                cValidationError = "Email address required";
                            }
                            else
                            {
                                cValidationError = "Invalid email address";
                            }

                        }

                        // Add the status node

                        var oCStatusNode = moPageXml.CreateElement("Status");
                        oCStatusNode.SetAttribute("canVote", Conversions.ToString(Interaction.IIf(bCanVote, "true", "false")));
                        if (bHasVoted)
                            oCStatusNode.SetAttribute("justVoted", "true");
                        if (nVoteBlockReason != PollBlockReason.None)
                            oCStatusNode.SetAttribute("blockReason", nVoteBlockReason.ToString());
                        if (!string.IsNullOrEmpty(cValidationError))
                            oCStatusNode.SetAttribute("validationError", cValidationError);
                        ocNode.AppendChild(oCStatusNode);


                        // Calculate the results
                        var oCResNode = moPageXml.CreateElement("Results");
                        // Do we need to check the table exists?
                        sSql = "SELECT DISTINCT " + sPollItemField + " AS PollOption, Count(" + sPollItemField + ") AS ResultsCount FROM " + sTable + " WHERE nActivityType=" + ((int)Cms.dbHelper.ActivityType.SubmitVote).ToString() + " AND " + sPollField + " = '" + sPollId + "' GROUP BY " + sPollItemField;
                        using (var oDr = this.moDbHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                        {

                            while (oDr.Read())
                            {
                                var oResNode = moPageXml.CreateElement("PollResult");
                                oResNode.SetAttribute("entryId", Conversions.ToString(oDr[0]));
                                oResNode.SetAttribute("votes", Conversions.ToString(oDr[1]));
                                oCResNode.AppendChild(oResNode);
                            }
                            oDr.Close();
                        }

                        ocNode.AppendChild(oCResNode);
                    }


                }

                sProcessInfo = "End  Poll Processing";
            }
            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ProcessPolls", ex, sProcessInfo));
            }
        }

        public virtual void AddSearch(ref Cms aWeb)
        {
            var argaWeb = this;
            oSrch = new Cms.Search(ref argaWeb);
            oSrch.apply();
        }

        /// <summary>
        /// Executes processing on specific content types.
        /// </summary>
        /// <remarks>In most cases, plugins (e.g. Search, Reports) and LayoutActions do this, so this is for content types that require a non-layout, non-function specific approach.</remarks>
        private void ContentActions()
        {

            this.PerfMon.Log("Web", "ContentActions");
            string sProcessInfo = "";
            XmlElement ocNode;

            try
            {

                // Load in any specified content files

                foreach (XmlElement currentOcNode in moPageXml.SelectNodes("/Page/Contents/Content[@contentFile!=''] | /Page/ContentDetail/descendant-or-self::Content[@contentFile!='']"))
                {
                    ocNode = currentOcNode;

                    if (File.Exists(this.goServer.MapPath("/" + gcProjectPath + ocNode.GetAttribute("contentFile"))))
                    {
                        var newXml = new XmlDocument();
                        newXml.PreserveWhitespace = true;
                        // copy related nodes
                        newXml.Load(this.goServer.MapPath("/" + gcProjectPath + ocNode.GetAttribute("contentFile")));
                        foreach (XmlElement relElem in ocNode.SelectNodes("Content"))
                        {
                            var argnodeToAddTo = newXml.DocumentElement;
                            Tools.Xml.AddExistingNode(ref argnodeToAddTo, relElem);

                        }
                        ocNode.InnerXml = newXml.DocumentElement.InnerXml;
                        // ocNode.AppendChild(moPageXml.ImportNode(Protean.Tools.Xml.firstElement(newXml.DocumentElement), True))
                    }

                }

                foreach (XmlElement currentOcNode1 in moPageXml.SelectNodes("/Page/*/Content[@appendFile!='']"))
                {
                    ocNode = currentOcNode1;

                    if (File.Exists(this.goServer.MapPath("/" + gcProjectPath + ocNode.GetAttribute("appendFile"))))
                    {
                        var newXml = new XmlDocument();
                        newXml.PreserveWhitespace = true;
                        newXml.Load(this.goServer.MapPath("/" + gcProjectPath + ocNode.GetAttribute("appendFile")));

                        ocNode.AppendChild(moPageXml.ImportNode(newXml.DocumentElement, true));
                    }

                }

                string ContentActionXpath = "";
                if (this.mnArtId > 0 & Strings.LCase(this.moConfig["ActionOnDetail"]) != "true")
                {
                    ContentActionXpath = "/Page/Contents/Content[@action!='' and @actionOnDetail='true'] | /Page/ContentDetail/Content[@action!=''] | /Page/ContentDetail/Content/Content[@action!=''] | /Page/Contents/Content[@action!='' and @id='" + this.mnArtId + "']";
                }
                else
                {
                    ContentActionXpath = "/Page/Contents/Content[@action!='']";
                }


                foreach (XmlElement currentOcNode2 in moPageXml.SelectNodes(ContentActionXpath))
                {
                    ocNode = currentOcNode2;
                    string classPath = ocNode.GetAttribute("action");
                    string assemblyName = ocNode.GetAttribute("assembly");
                    string assemblyType = ocNode.GetAttribute("assemblyType");
                    string providerName = ocNode.GetAttribute("providerName");
                    string providerType = ocNode.GetAttribute("providerType");
                
                    if (providerType == "")
                    { 
                        providerType = "messaging"; 
                    }
                    string methodName = Strings.Right(classPath, Strings.Len(classPath) - classPath.LastIndexOf(".") - 1);

                    classPath = Strings.Left(classPath, classPath.LastIndexOf("."));

                    if (!string.IsNullOrEmpty(classPath))
                    {
                        try
                        {
                            Type calledType = null;

                            if (!string.IsNullOrEmpty(assemblyName))
                            {
                                classPath = classPath + ", " + assemblyName;
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
                                        assemblyInstance = Assembly.LoadFrom(this.goServer.MapPath(moPrvConfig.Providers[providerName + "Local"].Parameters["path"]));
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
                                    if (moPrvConfig.Providers[providerName] != null)
                                    {
      switch (moPrvConfig.Providers[providerName].Parameters["path"])
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
                                                assemblyInstance = Assembly.LoadFrom(this.goServer.MapPath(moPrvConfig.Providers[providerName].Parameters["path"]));
                                                classPath = moPrvConfig.Providers[providerName].Parameters["classPrefix"] + classPath;
                                                calledType = assemblyInstance.GetType(classPath, true);
                                                break;
                                            }
                                    }
                                    }

                              
                                }
                            }
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
                            if (calledType != null) { 

                            var o = Activator.CreateInstance(calledType);

                            var args = new object[2];
                            args[0] = this;
                            args[1] = ocNode;

                            calledType.InvokeMember(methodName, BindingFlags.InvokeMethod, null, o, args);

                                // Error Handling ?
                                // Object Clearup ?
                            }
                            calledType = null;
                        }

                        catch (Exception ex)
                        {
                            // OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ContentActions", ex, sProcessInfo))
                            sProcessInfo = classPath + "." + methodName + " not found";
                            ocNode.InnerXml = "<Content type=\"error\"><div>" + HtmlEncode(sProcessInfo + ex.Message + ex.StackTrace) + "</div></Content>";
                        }
                    }

                }

                // Content Type : ContentGrabber
                foreach (XmlElement currentOcNode3 in moPageXml.SelectNodes("/Page/Contents/Content[@display='grabber']"))
                {
                    ocNode = currentOcNode3;
                    this.moDbHelper.getContentFromModuleGrabber(ref ocNode);
                }

                foreach (XmlElement currentOcNode4 in moPageXml.SelectNodes("/Page/Contents/Content[@display='group']"))
                {
                    ocNode = currentOcNode4;
                    this.moDbHelper.getContentFromProductGroup(ref ocNode);
                }

                // Content Type : ContentGrabber
                foreach (XmlElement currentOcNode5 in moPageXml.SelectNodes("/Page/Contents/Content[@type='ContentGrabber']"))
                {
                    ocNode = currentOcNode5;
                    this.moDbHelper.getContentFromContentGrabber(ref ocNode);
                }

                // Content Type : Poll
                if (moPageXml.SelectNodes("/Page/Contents/Content[@type='Poll']").Count > 0)
                {
                    ProcessPolls();
                }



                // Count Relations
                string cContentIdsForRelatedCount = "";
                foreach (XmlElement currentOcNode6 in moPageXml.SelectNodes("/Page/Contents/descendant-or-self::Content[@type='Tag' or @relatedCount='true']"))
                {
                    ocNode = currentOcNode6;
                    cContentIdsForRelatedCount += ocNode.GetAttribute("id") + ",";
                }
                if (!string.IsNullOrEmpty(cContentIdsForRelatedCount))
                {
                    cContentIdsForRelatedCount = cContentIdsForRelatedCount.Remove(cContentIdsForRelatedCount.Length - 1);
                    string sSql = "select Distinct COUNT(nContentParentid) as count, nContentChildid from tblContentRelation where nContentChildId in (" + cContentIdsForRelatedCount + ")  group by nContentChildid";
                    // Dim oDr As SqlDataReader = moDbHelper.getDataReader(sSql)
                    using (var oDr = this.moDbHelper.getDataReaderDisposable(sSql)) // Done by sonali on 13/7/2022
                    {

                        while (oDr.Read())
                        {
                            foreach (XmlElement currentOcNode7 in moPageXml.SelectNodes(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("/Page/Contents/descendant-or-self::Content[@id='", oDr["nContentChildId"]), "']"))))
                            {
                                ocNode = currentOcNode7;
                                ocNode.SetAttribute("relatedCount", Conversions.ToString(oDr["count"]));
                            }
                        }
                    }
                }


                // Content Type : xForm with ContentAction
                if (!this.mbAdminMode)
                {
                    string formXpath = "/Page/Contents/Content[(@type='xform' and model/submission/@SOAPAction) or (@process='xform') or (@moduleType='xForm' and model/submission/@SOAPAction)]";
                    if (this.mnArtId > 0)
                    {
                        // if the current contentDetail has child xform then that is all we process.
                        if (moPageXml.SelectNodes("/Page/ContentDetail/descendant-or-self::Content[(@type='xform' and model/submission/@SOAPAction) or (@process='xform')]").Count > 0)
                        {
                            formXpath = "/Page/ContentDetail/descendant-or-self::Content[(@type='xform' and model/submission/@SOAPAction) or (@process='xform')]";
                        }
                    }

                    foreach (XmlElement currentOcNode8 in moPageXml.SelectNodes(formXpath))
                    {
                        ocNode = currentOcNode8;
                        // If oXform Is Nothing Then oXform = New xForm
                        if (oXform is null)
                            oXform = (Protean.xForm)getXform();

                        oXform.moPageXML = moPageXml;
                        oXform.load(ref ocNode, true);
                        if (oXform.isSubmitted())
                        {
                            oXform.submit();
                        }
                        oXform.addValues();
                        oXform = (Protean.xForm)null;
                    }
                    // just want to add submitted values but not hanlde submit
                    formXpath = "/Page/Contents/Content[(@process='addValues')]";
                    if (this.mnArtId > 0)
                        formXpath = "/Page/ContentDetail/descendant-or-self::Content[(@process='addValues')]";
                    foreach (XmlElement currentOcNode9 in moPageXml.SelectNodes(formXpath))
                    {
                        ocNode = currentOcNode9;
                        if (oXform is null)
                            oXform = (Protean.xForm)getXform();
                        oXform.moPageXML = moPageXml;
                        oXform.load(ref ocNode, true);
                        if (oXform.isSubmitted())
                        {
                            oXform.updateInstanceFromRequest();
                        }
                        oXform.addValues();
                        oXform = (Protean.xForm)null;
                    }
                }

                BespokeActions();
            }



            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ContentActions", ex, sProcessInfo));
            }
        }


        /// <summary>
        /// Use of this is for discussion, but it is my intention that this could contain a repository of actions
        /// that are not necearrily triggered by Content, but rather by command; and yet could be accessible to different 
        /// faculties of EonicWeb, such as Admin calls, Ajax calls and Content building calls.
        /// </summary>
        /// <remarks></remarks>
        protected virtual void CommonActions()
        {
            try
            {
                // Integration calls - trying to use a similar methodology as above
                // Commented out by adding False.
                string integrationCommand = "";

                if (this.moRequest.QueryString.Count > 0)
                {
                    integrationCommand = this.moRequest["integration"];
                }

                if (!string.IsNullOrEmpty(integrationCommand))
                {


                    // Directory integrations take a directory ID
                    string requestedDirectoryId = this.moRequest["dirId"];
                    long directoryId = (long)this.mnUserId;
                    if (!string.IsNullOrEmpty(requestedDirectoryId) && Information.IsNumeric(requestedDirectoryId) && Conversions.ToInteger(requestedDirectoryId) > 0)

                    {
                        directoryId = Conversions.ToLong(requestedDirectoryId);
                    }

                    if (directoryId > 0L)
                    {
                        // Invoke the integration method.
                        object[] constructorArguments = new object[] { this, Convert.ToInt64(directoryId) };
                        Invoke.InvokeObjectMethod("Protean.Integration.Directory." + integrationCommand, constructorArguments, null, this, "OnComponentError", "OnError");
                    }

                }
            }
            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "CommonActions", ex, ""));

            }
        }


        /// <summary>
        /// To allow for bespoke content actions to be overloaded
        /// </summary>
        /// <remarks></remarks>
        public virtual void BespokeActions()
        {

        }

        /// <summary>
        /// Executes Actions based on Specific Page Layouts.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual string LayoutActions()
        {
            this.PerfMon.Log("Web", "LayoutActions");
            string sProcessInfo = "";
            bool bRunSearches = false;


            try
            {

                switch (moPageXml.SelectSingleNode("/Page/@layout").Value ?? "")
                {
                    case "Search_Results":
                    case "Search_Results_Products_Index":
                    case "Search":
                    case "Quick_Find":
                        {
                            if (!ibIndexMode)
                            {
                                bRunSearches = true;
                            }

                            break;
                        }
                    case "List_Quotes":
                        {
                            if (this.mnUserId > 0)
                            {
                                Cms.Quote oQuote;
                                var argaWeb = this;
                                oQuote = new Cms.Quote(ref argaWeb);
                                XmlElement argoPageDetail = null;
                                oQuote.ListOrders(("0" + this.moRequest["OrderId"]).ToString(), true, 0, oPageDetail: ref argoPageDetail);
                                oQuote = (Cms.Quote)null;
                            }

                            break;
                        }
                    case "List_Orders":
                        {
                            if (this.mnUserId > 0)
                            {
                                Cms.Cart oCart;
                                var argaWeb1 = this;
                                oCart = new Cms.Cart(ref argaWeb1);
                                XmlElement argoPageDetail1 = null;
                                oCart.ListOrders(("0" + this.moRequest["OrderId"]).ToString(), true, 0, oPageDetail: ref argoPageDetail1);
                                oCart = (Cms.Cart)null;
                            }

                            break;
                        }
                }

                if ((gbCart | gbQuote) & moPageXml.SelectSingleNode("/Page/@layout").Value.Contains("Discounts_"))
                {
                    var argPageElmt = moPageXml.DocumentElement;
                    moDiscount.getDiscountXML(ref argPageElmt);
                }

                // extra bit for searches
                if (moPageXml.SelectSingleNode("/Page/@layout").Value.Contains("Search_Template"))
                {
                    bRunSearches = true;
                }
                // commented out by TS because this case is hit in v5 when using a module and this means it runs twice.
                if (moPageXml.SelectSingleNode("/Page/Contents/Content[@action='Protean.Cms+Search+Modules.GetResults']") is null)
                {
                    if (this.moRequest.QueryString.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(this.moRequest["searchMode"]))
                        {
                            bRunSearches = true;
                        }
                    }
                }

                if (bRunSearches)
                {
                    var argaWeb2 = this;
                    AddSearch(ref argaWeb2);
                }

                // extra bit for user control panels
                if (moPageXml.SelectSingleNode("/Page/@layout").Value.Contains("User_Control_Panel") | moPageXml.SelectSingleNode("/Page/@layout").Value.Contains("Internal_Feed"))
                {
                    string cContentTypes = this.moConfig["ControlPanelTypes"];
                    if (cContentTypes != null & !string.IsNullOrEmpty(cContentTypes))
                    {
                        string[] oTypes = Strings.Split(cContentTypes, ",");
                        int i;
                        var loopTo = Information.UBound(oTypes);
                        for (i = 0; i <= loopTo; i++)
                        {
                            // add all of those types of content to the pagecontent to the page
                            var argoPageElmt = moPageXml.DocumentElement;
                            GetContentXMLByType(ref argoPageElmt, oTypes[i]);
                        }
                    }
                }

                return "";
            }

            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "LayoutActions", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "LayoutActions", ex, sProcessInfo));
                return "";
            }

        }


        public void GetPageContentFromStoredProcedure(string SpName, Hashtable parameters, ref int nCount)
        {

            this.PerfMon.Log("Web", "GetPageContentFromStoredProcedure");

            XmlElement oRoot;
            string sProcessInfo = "";
            DataSet oDs;

            try
            {
                oRoot = (XmlElement)moPageXml.SelectSingleNode("//Contents");

                if (oRoot is null)
                {
                    oRoot = moPageXml.CreateElement("Contents");
                    moPageXml.AppendChild(oRoot);
                }

                oDs = this.moDbHelper.GetDataSet(SpName, "Content", "Contents", parameters: parameters, querytype: CommandType.StoredProcedure);

                if (oDs.Tables.Count > 0)
                {
                    nCount = oDs.Tables["Content"].Rows.Count;
                    this.moDbHelper.AddDataSetToContent(ref oDs, ref oRoot, ref mdPageExpireDate, ref mdPageUpdateDate, (long)this.mnPageId, false, "");
                    // AddGroupsToContent(oRoot)
                }
            }


            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetPageContentFromStoredProcedure", ex, sProcessInfo));
            }
        }


        public void RemoveDuplicateContent([Optional, DefaultParameterValue(0L)] ref long nRemoved)
        {
            string sProcessInfo = "";
            long nContentId;
            var hContentIds = new Hashtable();
            try
            {

                // this will delete all but the first found.
                foreach (XmlElement oContentElmt in moPageXml.SelectNodes("/Page/Contents/Content"))
                {
                    nContentId = Conversions.ToLong(oContentElmt.GetAttribute("id"));
                    if (hContentIds[nContentId] is null)
                    {
                        hContentIds.Add(nContentId, nContentId);
                    }
                    else
                    {
                        oContentElmt.ParentNode.RemoveChild(oContentElmt);
                        nRemoved = nRemoved - 1L;
                    }
                }
            }


            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "RemoveDuplicateContent", ex, sProcessInfo));
            }
            finally
            {
                hContentIds = null;
            }

        }

        /// <summary>
        /// This will be passed the where portion of a SQL statement and will add content to the page based on the where statement. This funciton contains all the logic to respect page permissions, statuses and wheather you are logged into the admin system. It also calls moDbHelper.AddDataSetToContent which formats the dataset result and adds as the correct XML to the site page.
        /// No content really should be added to the page without calling .AddDataSetToContent otherwise the result will not be consistent.
        /// </summary>
        /// <param name="sWhereSql"></param>
        /// <param name="bPrimaryOnly"></param>
        /// <param name="nCount"></param>
        /// <param name="bIgnorePermissionsCheck"></param>
        /// <param name="nReturnRows"></param>
        /// <param name="cOrderBy"></param>
        /// <param name="oContentsNode"></param>
        /// <param name="cAdditionalJoins"></param>
        /// <param name="bContentDetail"></param>
        /// <param name="pageNumber"></param>
        /// <param name="distinct"></param>
        /// <param name="cShowSpecificContentTypes"></param>
        /// 


        public void GetPageContentFromSelect(string sWhereSql, ref int nCount, ref XmlElement oContentsNode, ref XmlElement oPageDetail, bool bPrimaryOnly = false, bool bIgnorePermissionsCheck = false, int nReturnRows = 0, string cOrderBy = "type, cl.nDisplayOrder", string cAdditionalJoins = "", bool bContentDetail = false, long pageNumber = 0L, bool distinct = false, string cShowSpecificContentTypes = "", bool ignoreActiveAndDate = false, long nStartPos = 0L, long nItemCount = 0L, bool bShowContentDetails = true)
        {
            this.PerfMon.Log("Web", "GetPageContentFromSelect");
            XmlElement oRoot;
            string sSql;
            string sPrimarySql = "";
            string sTopSql = "";
            string sMembershipSql = "";
            string sFilterSql = "";
            string sProcessInfo = "";
            DataSet oDs;
            long nAuthUserId;
            long nAuthGroup;
            string cContentField = "";
            string cFilterTarget = string.Empty;


            try
            {



                // Apply the possiblity of getting contents into a node other than the page contents node
                if (oContentsNode is null)
                {
                    oRoot = (XmlElement)moPageXml.DocumentElement.SelectSingleNode("Contents");
                    if (oRoot is null)
                    {
                        oRoot = moPageXml.CreateElement("Contents");
                        moPageXml.DocumentElement.AppendChild(oRoot);
                    }
                }
                else
                {
                    oRoot = oContentsNode;
                    nItemCount = Conversions.ToInteger("0" + oContentsNode.GetAttribute("stepCount"));
                }



                if (bContentDetail == false)
                {
                    cContentField = "cContentXmlBrief";
                }
                else
                {
                    cContentField = "cContentXmlDetail";
                }



                if (nReturnRows > 0 & pageNumber == 0L)
                {
                    sTopSql = "TOP " + nReturnRows + " ";
                }



                sSql = "SET ARITHABORT ON ";
                sSql = Conversions.ToString(sSql + Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("SELECT ", Interaction.IIf(distinct, "DISTINCT ", "")), sTopSql), " c.nContentKey as id, dbo.fxn_getContentParents(c.nContentKey) as parId, cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, CAST("), cContentField), " AS varchar(max)) as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, CL.cPosition as position "));
                sSql += "FROM tblContent AS c INNER JOIN ";
                sSql += "tblAudit AS a ON c.nAuditId = a.nAuditKey LEFT OUTER JOIN ";
                sSql += "tblContentLocation AS CL ON c.nContentKey = CL.nContentId ";
                // sSql &= "INNER Join tblCartCatProductRelations On c.nContentKey = tblCartCatProductRelations.nContentId "   'uncomment by nita because resolving table not found error



                // GCF - sql replaced by the above - 24/06/2011
                // replaced JOIN to tblContentLocation with  LEFT OUTER JOIN
                // as we were getting nothing back when content had no related 
                // content location data



                // sSql = "SET ARITHABORT ON SELECT " & sTopSql & " c.nContentKey as id, dbo.fxn_getContentParents(c.nContentKey) as parId, cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, CL.cPosition as position from tblContent c inner join tblContentLocation CL on c.nContentKey = CL.nContentId inner join tblAudit a on c.nAuditId = a.nAuditKey"



                // ' Add the extra joins if specified.
                if (!string.IsNullOrEmpty(cAdditionalJoins))
                    sSql += " " + cAdditionalJoins + " ";



                // we only want to return results that occur on pages beneath the current root id.
                // create a new funtion that passes in the StructId and the RootId to return yes or no.



                if (bPrimaryOnly)
                {
                    sPrimarySql = " CL.bPrimary = 1 ";
                }

                if (oContentsNode != null)
                {
                    if (oContentsNode.Attributes["contentType"] != null)
                    {
                        cFilterTarget = oContentsNode.Attributes["contentType"].Value;
                    }
                    if (oContentsNode.Attributes["filterTarget"] != null)
                    {
                        cFilterTarget = oContentsNode.Attributes["filterTarget"].Value;
                    }
                }
                object sFilterTargetSql = "";
                if (!string.IsNullOrEmpty(cFilterTarget))
                {
                    cFilterTarget = " and c.cContentSchemaName ='" + cFilterTarget + "' ";
                }

                if (gbMembership == true & bIgnorePermissionsCheck == false)
                {



                    if (this.mnUserId == 0 & gnNonAuthUsers != 0)
                    {



                        // Note : if we are checking permissions for a page, and we're not logged in, then we shouldn't check with the gnAuthUsers group
                        // Ratehr, we should use the gnNonAuthUsers user group if it exists.



                        nAuthUserId = gnNonAuthUsers;
                        nAuthGroup = gnNonAuthUsers;
                    }



                    else if (this.mnUserId == 0)
                    {



                        // If no gnNonAuthUsers user group exists, then remove the auth group
                        nAuthUserId = (long)this.mnUserId;
                        nAuthGroup = -1;
                    }



                    else
                    {
                        nAuthUserId = (long)this.mnUserId;
                        nAuthGroup = gnAuthUsers;
                    }

                   

                    // Check the page is not denied
                    sMembershipSql = "NOT(dbo.fxn_checkPermission(CL.nStructId," + nAuthUserId + "," + nAuthGroup + ") LIKE '%DENIED%')";



                    // Commenting out the folowing as it wouldn't return items that were Inherited view etc.
                    // sMembershipSql = " (dbo.fxn_checkPermission(CL.nStructId," & mnUserId & "," & gnAuthUsers & ") = 'OPEN' or dbo.fxn_checkPermission(CL.nStructId," & mnUserId & "," & gnAuthUsers & ") = 'VIEW')"
                    // add "and" if clause before
                    if (!string.IsNullOrEmpty(sPrimarySql))
                        sMembershipSql = " and " + sMembershipSql;
                }



                if (ignoreActiveAndDate == false)
                {
                    // show only live content that is within date, unless we are in admin mode.
                    sFilterSql = GetStandardFilterSQLForContent(!string.IsNullOrEmpty(sPrimarySql) | !string.IsNullOrEmpty(sMembershipSql));
                }



                // add "and" if clause before
                if (!string.IsNullOrEmpty(sPrimarySql) | !string.IsNullOrEmpty(sMembershipSql) | !string.IsNullOrEmpty(sFilterSql))
                    sWhereSql = " and " + sWhereSql;



                string combinedWhereSQL = sPrimarySql + sMembershipSql + sFilterSql + sWhereSql;

                if (!string.IsNullOrEmpty(cFilterTarget))
                {
                    combinedWhereSQL = combinedWhereSQL + cFilterTarget;
                }

                if (Strings.Trim(combinedWhereSQL).StartsWith("and"))
                {
                    combinedWhereSQL = combinedWhereSQL.Substring(4);
                }



                sSql = sSql + " where (" + combinedWhereSQL + ")";
                if (oContentsNode != null)
                {
                    // Quick call to get the total number of records
                    string cSQL = "SET ARITHABORT ON ";
                    cSQL += "Select COUNT(distinct c.nContentKey) FROM tblContent AS c INNER JOIN ";
                    cSQL += "tblAudit AS a ON c.nAuditId = a.nAuditKey LEFT OUTER JOIN ";
                    cSQL += "tblContentLocation AS CL ON c.nContentKey = CL.nContentId ";
                    // ' Add the extra joins if specified.
                    if (!string.IsNullOrEmpty(cAdditionalJoins))
                        cSQL += " " + cAdditionalJoins + " ";
                    cSQL = cSQL + " where (" + combinedWhereSQL + ")";

                    long nTotal = Conversions.ToLong(this.moDbHelper.GetDataValue(cSQL, CommandType.Text, null, (object)0));

                    oContentsNode.SetAttribute("resultCount", nTotal.ToString());
                }


                if (!string.IsNullOrEmpty(cOrderBy))
                {
                    sSql += " ORDER BY " + cOrderBy;
                }
                else
                {
                    sSql += " ORDER BY(SELECT NULL)";
                }
                if (nItemCount > 0L)
                {

                    sSql += " OFFSET " + nStartPos + " ROWS FETCH NEXT " + nItemCount + " ROWS ONLY";

                }


                sSql = Strings.Replace(sSql, "&lt;", "<");

                this.PerfMon.Log("Web", "GetPageContentFromSelect", "GetPageContentFromSelect:" + sSql);

                if (pageNumber > 0L)
                {
                    oDs = this.moDbHelper.GetDataSet(sSql, "Content", "Contents", pageSize: nReturnRows, pageNumber: (int)pageNumber);
                }
                else
                {
                    oDs = this.moDbHelper.GetDataSet(sSql, "Content", "Contents");
                }
                if (oDs != null)
                {
                    nCount = oDs.Tables["Content"].Rows.Count;
                    this.PerfMon.Log("Web", "GetPageContentFromSelect", "GetPageContentFromSelect: " + nCount + " returned");
                    DateTime ExpireDate = mdPageExpireDate ?? DateTime.Now;
                    DateTime UpdateDate = mdPageUpdateDate ?? DateTime.Now;
                    this.moDbHelper.AddDataSetToContent(ref oDs, ref oRoot, (long)this.mnPageId, false, "", ref ExpireDate, ref UpdateDate, true, gnShowRelatedBriefDepth, cShowSpecificContentTypes);
                }
            }

            // If gbCart Or gbQuote Then
            // moDiscount.getAvailableDiscounts(oRoot)
            // End If
            // AddGroupsToContent(oRoot)
            catch (Exception ex)
            {



                // returnException(msException, mcModuleName, "GetPageContentFromSelect", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetPageContentFromSelect", ex, sProcessInfo));
            }
        }



        public void GetPageContentFromSelectFilterPagination(ref int nCount, ref XmlElement oContentsNode, ref XmlElement oPageDetail, string sWhereSql, bool bPrimaryOnly = false, bool bIgnorePermissionsCheck = false, int nReturnRows = 0, string cOrderBy = "type, cl.nDisplayOrder", string cAdditionalJoins = "", bool bContentDetail = false, long pageNumber = 0L, bool distinct = false, string cShowSpecificContentTypes = "", bool ignoreActiveAndDate = false, long nStartPos = 0L, long nItemCount = 0L, bool bShowContentDetails = true)
        {
            this.PerfMon.Log("Web", "GetPageContentFromSelect");
            XmlElement oRoot;
            string sSql;

            string sPrimarySql = "";
            string sTopSql = "";
            string sMembershipSql = "";
            string sFilterSql = "";
            string sProcessInfo = "";
            DataSet oDs;
            long nAuthUserId;
            long nAuthGroup;
            string cContentField = "";

            try
            {
                if (bContentDetail == false)
                {
                    cContentField = "cContentXmlBrief";
                }
                else
                {
                    cContentField = "cContentXmlDetail";
                }

                if (nReturnRows > 0 & pageNumber == 0L)
                {
                    sTopSql = "TOP " + nReturnRows + " ";
                }

                sSql = "SET ARITHABORT ON ";
                sSql = Conversions.ToString(sSql + Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("SELECT ", Interaction.IIf(distinct, "DISTINCT ", "")), sTopSql), " c.nContentKey as id, dbo.fxn_getContentParents(c.nContentKey) as parId, cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, CAST("), cContentField), " AS varchar(max)) as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, CL.cPosition as position "));
                sSql += "FROM tblContent AS c INNER JOIN ";
                sSql += "tblAudit AS a ON c.nAuditId = a.nAuditKey LEFT OUTER JOIN ";
                sSql += "tblContentLocation AS CL ON c.nContentKey = CL.nContentId ";
                // sSql &= "INNER Join tblCartCatProductRelations On c.nContentKey = tblCartCatProductRelations.nContentId "   'uncomment by nita because resolving table not found error

                // GCF - sql replaced by the above - 24/06/2011
                // replaced JOIN to tblContentLocation with  LEFT OUTER JOIN
                // as we were getting nothing back when content had no related 
                // content location data

                // sSql = "SET ARITHABORT ON SELECT " & sTopSql & " c.nContentKey as id, dbo.fxn_getContentParents(c.nContentKey) as parId, cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, CL.cPosition as position from tblContent c inner join tblContentLocation CL on c.nContentKey = CL.nContentId inner join tblAudit a on c.nAuditId = a.nAuditKey"

                // ' Add the extra joins if specified.
                if (!string.IsNullOrEmpty(cAdditionalJoins))
                    sSql += " " + cAdditionalJoins + " ";

                // we only want to return results that occur on pages beneath the current root id.
                // create a new funtion that passes in the StructId and the RootId to return yes or no.

                if (bPrimaryOnly)
                {
                    sPrimarySql = " CL.bPrimary = 1 ";
                }

                if (gbMembership == true & bIgnorePermissionsCheck == false)
                {

                    if (this.mnUserId == 0 & gnNonAuthUsers != 0)
                    {

                        // Note : if we are checking permissions for a page, and we're not logged in, then we shouldn't check with the gnAuthUsers group
                        // Ratehr, we should use the gnNonAuthUsers user group if it exists.

                        nAuthUserId = gnNonAuthUsers;
                        nAuthGroup = gnNonAuthUsers;
                    }

                    else if (this.mnUserId == 0)
                    {

                        // If no gnNonAuthUsers user group exists, then remove the auth group
                        nAuthUserId = (long)this.mnUserId;
                        nAuthGroup = -1;
                    }

                    else
                    {
                        nAuthUserId = (long)this.mnUserId;
                        nAuthGroup = gnAuthUsers;
                    }

                    // Check the page is not denied
                    if (!string.IsNullOrEmpty(cShowSpecificContentTypes))
                    {
                        sMembershipSql = " c.cContentSchemaName ='" + cShowSpecificContentTypes + "' and  NOT(dbo.fxn_checkPermission(CL.nStructId," + nAuthUserId + "," + nAuthGroup + ") LIKE '%DENIED%')";
                    }
                    else
                    {
                        sMembershipSql = "NOT(dbo.fxn_checkPermission(CL.nStructId," + nAuthUserId + "," + nAuthGroup + ") LIKE '%DENIED%')";
                    }


                    // Commenting out the folowing as it wouldn't return items that were Inherited view etc.
                    // sMembershipSql = " (dbo.fxn_checkPermission(CL.nStructId," & mnUserId & "," & gnAuthUsers & ") = 'OPEN' or dbo.fxn_checkPermission(CL.nStructId," & mnUserId & "," & gnAuthUsers & ") = 'VIEW')"
                    // add "and" if clause before
                    if (!string.IsNullOrEmpty(sPrimarySql))
                        sMembershipSql = " and " + sMembershipSql;
                }

                if (ignoreActiveAndDate == false)
                {
                    // show only live content that is within date, unless we are in admin mode.
                    sFilterSql = GetStandardFilterSQLForContent(!string.IsNullOrEmpty(sPrimarySql) | !string.IsNullOrEmpty(sMembershipSql));
                }

                // add "and" if clause before
                if (!string.IsNullOrEmpty(sPrimarySql) | !string.IsNullOrEmpty(sMembershipSql) | !string.IsNullOrEmpty(sFilterSql))
                    sWhereSql = " and " + sWhereSql;

                string combinedWhereSQL = sPrimarySql + sMembershipSql + sFilterSql + sWhereSql;
                if (Strings.Trim(combinedWhereSQL).StartsWith("and"))
                {
                    combinedWhereSQL = combinedWhereSQL.Substring(4);
                }

                sSql = sSql + " where (" + combinedWhereSQL + ")";
                if (!string.IsNullOrEmpty(cOrderBy))
                    sSql += " ORDER BY " + cOrderBy;


                sSql += " offset " + nStartPos + " rows fetch next " + nItemCount + " rows only";


                sSql = Strings.Replace(sSql, "&lt;", "<");

                this.PerfMon.Log("Web", "GetPageContentFromSelect", "GetPageContentFromSelect:" + sSql);

                if (pageNumber > 0L)
                {
                    oDs = this.moDbHelper.GetDataSet(sSql, "Content", "Contents", pageSize: nReturnRows, pageNumber: (int)pageNumber);
                }
                else
                {
                    oDs = this.moDbHelper.GetDataSet(sSql, "Content", "Contents");
                }
                nCount = oDs.Tables["Content"].Rows.Count;
                this.PerfMon.Log("Web", "GetPageContentFromSelect", "GetPageContentFromSelect: " + nCount + " returned");

                oRoot = (XmlElement)moPageXml.DocumentElement.SelectSingleNode("Contents");
                if (oRoot is null)
                {
                    oRoot = moPageXml.CreateElement("Contents");
                    moPageXml.DocumentElement.AppendChild(oRoot);
                }

                this.moDbHelper.AddDataSetToContent(ref oDs, ref oRoot, ref mdPageExpireDate, ref mdPageUpdateDate, (long)this.mnPageId, false, "");
                if (bShowContentDetails)
                {
                    // Get the content Detail element
                    XmlElement oContentDetails;
                    if (oPageDetail is null)
                    {
                        oContentDetails = (XmlElement)moPageXml.SelectSingleNode("Page/ContentDetail");
                        if (oContentDetails is null)
                        {
                            oContentDetails = moPageXml.CreateElement("ContentDetail");
                            if (!string.IsNullOrEmpty(moPageXml.InnerXml))
                            {
                                moPageXml.FirstChild.AppendChild(oContentDetails);
                            }
                            else
                            {
                                oPageDetail.AppendChild(oContentDetails);
                            }

                        }
                    }
                    else
                    {
                        oContentDetails = oPageDetail;
                    }

                }
            }

            // If gbCart Or gbQuote Then
            // moDiscount.getAvailableDiscounts(oRoot)
            // End If
            // AddGroupsToContent(oRoot)
            catch (Exception ex)
            {

                // returnException(msException, mcModuleName, "GetPageContentFromSelect", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetPageContentFromSelect", ex, sProcessInfo));
            }
        }

        public void GetMenuContentFromSelect(string sWhereSql, ref int nCount, ref XmlElement oContentsNode, bool bPrimaryOnly = false, bool bIgnorePermissionsCheck = false, int nReturnRows = 0, string cOrderBy = "type, cl.nDisplayOrder", string cAdditionalJoins = "", bool bContentDetail = false, long pageNumber = 0L, bool distinct = false)
        {
            this.PerfMon.Log("Web", "GetPageContentFromSelect");
            XmlElement oRoot;
            string sSql;

            string sPrimarySql = "";
            string sTopSql = "";
            string sMembershipSql = "";
            string sFilterSql = "";
            string sProcessInfo = "";
            DataSet oDs;
            long nAuthUserId;
            long nAuthGroup;
            string cContentField = "";

            try
            {

                // Apply the possiblity of getting contents into a node other than the page contents node
                if (oContentsNode is null)
                {
                    oRoot = (XmlElement)moPageXml.SelectSingleNode("//Contents");

                    if (oRoot is null)
                    {
                        oRoot = moPageXml.CreateElement("Contents");
                        moPageXml.DocumentElement.AppendChild(oRoot);
                    }
                }
                else
                {
                    oRoot = oContentsNode;
                }

                if (bContentDetail == false)
                {
                    cContentField = "cContentXmlBrief";
                }
                else
                {
                    cContentField = "cContentXmlDetail";
                }

                if (nReturnRows > 0 & pageNumber == 0L)
                {
                    sTopSql = "TOP " + nReturnRows + " ";
                }

                sSql = "SET ARITHABORT ON ";
                sSql = Conversions.ToString(sSql + Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("SELECT ", Interaction.IIf(distinct, "DISTINCT ", "")), sTopSql), " c.nContentKey as id, cl.nStructId as locId,"));
                if (bPrimaryOnly)
                {
                    sSql += " CL.nStructId as parId,";
                }
                else
                {
                    sSql += " dbo.fxn_getContentParents(c.nContentKey) as parId,";
                }
                sSql += " cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, CAST(" + cContentField + " AS varchar(max)) as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, CL.cPosition as position ";
                sSql += "FROM tblContent AS c INNER JOIN ";
                sSql += "tblAudit AS a ON c.nAuditId = a.nAuditKey LEFT OUTER JOIN ";
                sSql += "tblContentLocation AS CL ON c.nContentKey = CL.nContentId ";

                // GCF - sql replaced by the above - 24/06/2011
                // replaced JOIN to tblContentLocation with  LEFT OUTER JOIN
                // as we were getting nothing back when content had no related 
                // content location data

                // sSql = "SET ARITHABORT ON SELECT " & sTopSql & " c.nContentKey as id, dbo.fxn_getContentParents(c.nContentKey) as parId, cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, CL.cPosition as position from tblContent c inner join tblContentLocation CL on c.nContentKey = CL.nContentId inner join tblAudit a on c.nAuditId = a.nAuditKey"

                // ' Add the extra joins if specified.
                if (!string.IsNullOrEmpty(cAdditionalJoins))
                    sSql += " " + cAdditionalJoins + " ";

                // we only want to return results that occur on pages beneath the current root id.
                // create a new funtion that passes in the StructId and the RootId to return yes or no.

                if (bPrimaryOnly)
                {
                    sPrimarySql = " CL.bPrimary = 1 ";
                }

                if (gbMembership == true & bIgnorePermissionsCheck == false)
                {

                    if (this.mnUserId == 0 & gnNonAuthUsers != 0)
                    {

                        // Note : if we are checking permissions for a page, and we're not logged in, then we shouldn't check with the gnAuthUsers group
                        // Ratehr, we should use the gnNonAuthUsers user group if it exists.

                        nAuthUserId = gnNonAuthUsers;
                        nAuthGroup = gnNonAuthUsers;
                    }

                    else if (this.mnUserId == 0)
                    {

                        // If no gnNonAuthUsers user group exists, then remove the auth group
                        nAuthUserId = (long)this.mnUserId;
                        nAuthGroup = -1;
                    }

                    else
                    {
                        nAuthUserId = (long)this.mnUserId;
                        nAuthGroup = gnAuthUsers;
                    }

                    // Check the page is not denied
                    sMembershipSql = " NOT(dbo.fxn_checkPermission(CL.nStructId," + nAuthUserId + "," + nAuthGroup + ") LIKE '%DENIED%')";

                    // Commenting out the folowing as it wouldn't return items that were Inherited view etc.
                    // sMembershipSql = " (dbo.fxn_checkPermission(CL.nStructId," & mnUserId & "," & gnAuthUsers & ") = 'OPEN' or dbo.fxn_checkPermission(CL.nStructId," & mnUserId & "," & gnAuthUsers & ") = 'VIEW')"
                    // add "and" if clause before
                    if (!string.IsNullOrEmpty(sPrimarySql))
                        sMembershipSql = " and " + sMembershipSql;
                }

                // show only live content that is within date, unless we are in admin mode.
                sFilterSql = GetStandardFilterSQLForContent(!string.IsNullOrEmpty(sPrimarySql) | !string.IsNullOrEmpty(sMembershipSql));


                // add "and" if clause before
                if (!string.IsNullOrEmpty(sPrimarySql) | !string.IsNullOrEmpty(sMembershipSql) | !string.IsNullOrEmpty(sFilterSql))
                    sWhereSql = " and " + sWhereSql;

                sSql = sSql + " where (" + sPrimarySql + sMembershipSql + sFilterSql + sWhereSql + ")";
                if (!string.IsNullOrEmpty(cOrderBy))
                    sSql += " ORDER BY " + cOrderBy;
                sSql = Strings.Replace(sSql, "&lt;", "<");

                if (pageNumber > 0L)
                {
                    oDs = this.moDbHelper.GetDataSet(sSql, "Content", "Contents", pageSize: nReturnRows, pageNumber: (int)pageNumber);
                }
                else
                {
                    oDs = this.moDbHelper.GetDataSet(sSql, "Content", "Contents");
                }
                nCount = oDs.Tables["Content"].Rows.Count;
                DateTime UpdatedTime = mdPageUpdateDate ?? DateTime.Now;
                DateTime ExpireTime = mdPageExpireDate ?? DateTime.Now;
                var oXml = this.moDbHelper.ContentDataSetToXml(ref oDs, ref UpdatedTime);
                var oXml2 = oRoot.OwnerDocument.ImportNode(oXml.DocumentElement, true);

                foreach (XmlElement oNode in oXml2.SelectNodes("Content"))
                {
                    XmlElement xmloContent = (XmlElement)oNode;
                    oRoot.AppendChild(this.moDbHelper.SimpleTidyContentNode(ref xmloContent, ref ExpireTime, ref UpdatedTime, ""));
                }

            }

            // moDbHelper.AddDataSetToContent(oDs, oRoot, mnPageId, False, "", mdPageExpireDate, mdPageUpdateDate, True, gnShowRelatedBriefDepth)

            // If gbCart Or gbQuote Then
            // moDiscount.getAvailableDiscounts(oRoot)
            // End If
            // AddGroupsToContent(oRoot)
            catch (Exception ex)
            {

                // returnException(msException, mcModuleName, "GetPageContentFromSelect", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetPageContentFromSelect", ex, sProcessInfo));
            }
        }


        public virtual string MembershipProcess()
        {
            this.PerfMon.Log("Web", "MembershipProcess");
            string sProcessInfo = "";
            string sReturnValue = string.Empty;
            string cLogonCmd = string.Empty;
            try
            {

                Cms argmyWeb = this;
                return Conversions.ToString(moMemProv.Activities.MembershipProcess(ref argmyWeb));

            }

            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "MembershipLogon", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "MembershipProcess", ex, sProcessInfo));
                return null;
            }
        }



        public virtual bool AlternativeAuthentication()
        {



            this.PerfMon.Log("Web", "AlternativeAuthentication");


            string cProcessInfo = "";
            // Dim bCheck As Boolean = False
            // Dim cToken As String = ""
            // Dim cKey As String = ""
            // Dim cDecrypted As String = ""
            // Dim nReturnId As Integer

            try
            {
                Cms argmyWeb = this;
                return Conversions.ToBoolean(moMemProv.Activities.AlternativeAuthentication(ref argmyWeb));
            }

            // ' Look for the RC4 token
            // If moRequest("token") <> "" And moConfig("AlternativeAuthenticationKey") <> "" Then

            // cProcessInfo = "IP Address Checking"

            // Dim cIPList As String = CStr(moConfig("AlternativeAuthenticationIPList"))

            // If cIPList = "" OrElse Tools.Text.IsIPAddressInList(moRequest.UserHostAddress, cIPList) Then

            // cProcessInfo = "Decrypting token"
            // Dim oEnc As New Protean.Tools.Encryption.RC4()

            // cToken = moRequest("token")
            // cKey = moConfig("AlternativeAuthenticationKey")

            // ' There are two accepted formats to receive:
            // '  1. Email address
            // '  2. User ID

            // cDecrypted = Trim(Tools.Encryption.RC4.Decrypt(cToken, cKey))

            // If Tools.Text.IsEmail(cDecrypted) Then

            // ' Authentication is by way of e-mail address
            // cProcessInfo = "Email authenctication: Retrieving user for email: " & cDecrypted
            // ' Get the user id based on the e-mail address
            // nReturnId = moDbHelper.GetUserIDFromEmail(cDecrypted)

            // If nReturnId > 0 Then
            // bCheck = True
            // Me.mnUserId = nReturnId
            // End If

            // ElseIf IsNumeric(cDecrypted) AndAlso CInt(cDecrypted) > 0 Then

            // ' Authentication is by way of user ID
            // cProcessInfo = "User ID Authentication: " & cDecrypted
            // ' Get the user id based on the e-mail address
            // bCheck = moDbHelper.IsValidUser(CInt(cDecrypted))
            // If bCheck Then Me.mnUserId = CInt(cDecrypted)

            // End If

            // End If

            // End If

            // Return bCheck

            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "AlternativeAuthentication", ex, cProcessInfo));
                return false;
            }

        }

        /// <summary>
        /// <para>This provides the facility to redirect a logged on user from PATH A to PATH B if they belong to a certain group/directory item.</para>
        /// <para>PATH A and PATH B are assumed to be sites that share the same database.  These could be different sites or just different application paths.</para>
        /// <para>PATH A and PATH B cannot be child folders of each other - i.e. if PATH A is /patha, path b could not be /patha/pathb</para>
        /// <para>PATH A and PATH B must be of the same type - i.e. both URIs or both Application paths.</para>
        /// <example>e.g. "/patha/" and "/pathb/", "http://domain1/" and "http://domain2/", "http://domain1/" and "http://domain2/pathb"</example>
        /// <example>But NOT "/patha/" and "http://domain2/pathb"</example>
        /// </summary>
        /// <remarks></remarks>
        public virtual void SiteRedirection()
        {

            this.PerfMon.Log("Web", "SiteRedirection");
            string cProcessInfo = "";
            try
            {

                // Parse the config string
                string[] cSiteConfig;
                string cToken = "";
                string cUrl = "";
                string[] aSites = this.moConfig["SiteGroupRedirection"].ToString().Split(';');


                foreach (string cSite in aSites)
                {
                    cSiteConfig = cSite.Split(',');

                    // Now check for the following (using the bitwise operator, and in this order)
                    // 1 - We're not already arriving from a redirection (indicated by token being passed through in the Request) - this stops recurring redirection!
                    // 2 - Does the config contain 3 items
                    // 3 - Is the first config item a number
                    // 4 - Does the path in the config NOT match the start of the current page (check URI and Path) - in other words - don't run this check if we're actually on the site in question!
                    // 5 - Is the user a member of the directory item ID listed in the config.
                    if (!!string.IsNullOrEmpty(this.moRequest["token"]) && cSiteConfig.Length == 3 && Information.IsNumeric(cSiteConfig[0]) && !(this.moRequest.Url.AbsoluteUri.StartsWith(cSiteConfig[1].ToString(), StringComparison.CurrentCultureIgnoreCase) || this.moRequest.Url.AbsolutePath.StartsWith(cSiteConfig[1].ToString(), StringComparison.CurrentCultureIgnoreCase)) && moPageXml.DocumentElement.SelectSingleNode("/Page/User/*[@id='" + cSiteConfig[0] + "']") != null)




                    {

                        // Success - try to rewrite the URL, and produce a token
                        cUrl = "" + cSiteConfig[1];
                        cToken = "" + cSiteConfig[2];
                        if (!(string.IsNullOrEmpty(cUrl) | string.IsNullOrEmpty(cToken)))
                        {

                            // Create the Redirection Token
                            cToken = Tools.Encryption.RC4.Encrypt(this.mnUserId.ToString(), cToken);

                            // Log the user off this site.
                            LogOffProcess();

                            // Redirect the user.
                            this.moResponse.Redirect(cUrl + "?token=" + cToken, false);
                            this.moCtx.ApplicationInstance.CompleteRequest();
                        }

                    }

                }
            }

            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "SiteRedirection", ex, cProcessInfo));
            }

        }

        public virtual void LogOffProcess()
        {
            this.PerfMon.Log("Web", "LogOffProcess");
            string cProcessInfo = "";

            try
            {

                cProcessInfo = "Commit to Log";
                if (gbSingleLoginSessionPerUser)
                {
                    this.moDbHelper.logActivity(Cms.dbHelper.ActivityType.Logoff, (long)this.mnUserId, 0L);
                    if (this.moRequest.Cookies["ewslock"] != null)
                    {
                        this.moResponse.Cookies["ewslock"].Expires = DateTime.Now.AddDays(-1);
                    }
                }
                else
                {
                    this.moDbHelper.CommitLogToDB(Cms.dbHelper.ActivityType.Logoff, this.mnUserId, this.moSession.SessionID, DateTime.Now, 0, 0, "");
                }


                // Call this BEFORE clearing the user ID.
                cProcessInfo = "Clear Site Stucture";
                this.moDbHelper.clearStructureCacheUser();

                // Clear the user ID.
                this.mnUserId = 0;

                // Clear the cart
                if (this.moSession != null && gbCart)
                {
                    string cSql = "update tblCartOrder set cCartSessionId = 'OLD_' + cCartSessionId where (cCartSessionId = '" + this.moSession.SessionID + "' and cCartSessionId <> '')";
                    this.moDbHelper.ExeProcessSql(cSql);
                }

                if (this.moSession != null)
                {
                    cProcessInfo = "Abandon Session";
                    // AJG : Question - why does this not clear the Session ID?
                    this.moSession.Abandon();
                }

                if (this.moConfig["RememberMeMode"] != "KeepCookieAfterLogoff")
                {
                    cProcessInfo = "Clear Cookies";
                    this.moResponse.Cookies["RememberMeUserName"].Expires = DateTime.Now.AddDays(-1);
                    this.moResponse.Cookies["RememberMeUserId"].Expires = DateTime.Now.AddDays(-1);
                }
            }

            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "LogOffProcess", ex, cProcessInfo));
            }

        }

        public virtual string UserEditProcess()
        {
            this.PerfMon.Log("Web", "UserEditProcess");
            string sProcessInfo = "";
            string sReturnValue = string.Empty;
            try
            {


                return null;
            }

            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "UserEditProcess", ex, sProcessInfo));
                return null;
            }
        }

        public virtual void logonRedirect(string cLogonCmd)
        {
            string sProcessInfo = "";

            try
            {

                string sRedirectPath;

                // don't redirect if in cart process
                if (this.moRequest.QueryString["cartCmd"] != "Logon")
                {

                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(this.moSession["LogonRedirect"], "", false)))
                    {
                        sRedirectPath = Conversions.ToString(this.moSession["LogonRedirect"]);
                        this.moSession["LogonRedirect"] = "";
                    }
                    else
                    {

                        if (!string.IsNullOrEmpty(this.moConfig["LogonRedirectPath"]))
                        {
                            // sRedirectPath = mcOriginalURL & moConfig("LogonRedirectPath") & cLogonCmd
                            // TS changed this remvoing mcOriginalURL because it breaks logon redirect "/" to go to root/
                            sRedirectPath = this.moConfig["LogonRedirectPath"] + cLogonCmd;
                        }
                        else
                        {
                            sRedirectPath = mcOriginalURL + cLogonCmd;
                        }

                        if (this.goLangConfig != null)
                        {
                            sRedirectPath = mcPageLanguageUrlPrefix + sRedirectPath;
                        }

                    }
                    if (sRedirectPath.Contains("token="))
                    {

                        sRedirectPath = Regex.Replace(sRedirectPath, "(token=.*?)&", "");
                    }


                    msRedirectOnEnd = sRedirectPath;
                }
            }

            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "logonRedirect", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "logonRedirect", ex, sProcessInfo));
            }
        }

        /// <summary>
        /// <para>
        /// Check the request for certain parameters which if matched will create a 302 redirect
        /// For page redirects, ISAPI can't detect what we need, so we are going to need to check if PageURLFormat is set
        /// For content (artid) redirects, parameters are driven by the ISAPI rewrite routines which should be updated to do the following
        /// </para>
        /// <list>
        ///   <item>
        ///    If itemNNNN is detected then set u the redirect as follows:
        ///    <list>
        ///       <item>pass the path through as path</item>
        ///       <item>pass itemNNNN through as artid=NNNN</item>
        ///       <item>set a flag called redirect=statuscode</item>
        ///    </list>
        ///   </item>
        /// </list>
        /// <para>
        /// The redirect URL will be "redirPath/NNNN-/Product-Name
        /// </para>
        /// </summary>
        /// <returns></returns>
        /// <remarks>This may require the existing pages to be tidied up (i.e. removing hyphens and plusses)</remarks>
        private bool legacyRedirection()
        {
            this.PerfMon.Log("Web", "legacyRedirection");

            string cProcessInfo = "";
            bool bRedirect = false;

            try
            {
                // We can only do this if session exists
                if (this.moSession != null)
                {
                    if (this.moConfig["LegacyRedirect"] == "on")
                    {

                        string cPath = this.moRequest["path"] + "";
                        if (!cPath.StartsWith("/"))
                            cPath = "/" + cPath;
                        if (!cPath.EndsWith("/"))
                            cPath += "/";

                        // The checks we need to make are as follows:
                        // Is RedirPath being passed through (from ISAPI Rewrite)
                        // Is artid being passed through

                        // Check if an article redirect has been called
                        if (this.moRequest["redirect"] != null && !string.IsNullOrEmpty(this.moRequest["artid"]) && Information.IsNumeric(this.moRequest["artid"]))

                        {

                            // Try to find the product
                            long nArtId = Conversions.ToLong(this.moRequest["artid"]);
                            string cSql = Conversions.ToString(Operators.ConcatenateObject("SELECT cContentName FROM tblContent WHERE nContentKey = ", SqlFmt(nArtId.ToString())));
                            string cName = Conversions.ToString(this.moDbHelper.GetDataValue(cSql, CommandType.Text, null, ""));



                            if (!string.IsNullOrEmpty(cName))
                            {

                                // Replace any non-alphanumeric with hyphens
                                var oRe = new Regex("[^A-Z0-9]", RegexOptions.IgnoreCase);
                                cName = oRe.Replace(cName, "-").Trim('-');


                                // Iron out the spaces for paths
                                cPath = Strings.Replace(cPath, " ", "-");

                                // Construct the new path
                                cPath = cPath + nArtId.ToString() + "-/" + cName;
                                bRedirect = true;

                            }
                        }

                        else if (this.moConfig["PageURLFormat"] == "hyphens")
                        {
                            // Check the path - if hyphens is the preference then we need to analyse the path and rewrite it.
                            if (cPath.Contains("+") | cPath.Contains(" "))
                            {
                                cPath = Strings.Replace(cPath, "+", "-");
                                cPath = Strings.Replace(cPath, " ", "-");
                                bRedirect = true;
                            }
                        }

                        // Redirect
                        if (Conversions.ToBoolean(Operators.AndObject(bRedirect, Operators.ConditionalCompareObjectNotEqual(this.moSession["legacyRedirect"], "on", false))))
                        {
                            // Stop recursive redirects
                            this.moSession["legacyRedirect"] = "on";

                            // Assume status code is 301 unless instructed otherwise.
                            int nResponseCode = 301;
                            switch (this.moRequest["redirect"] ?? "")
                            {
                                case "301":
                                case "302":
                                case "303":
                                case "304":
                                case "307":
                                    {
                                        nResponseCode = Conversions.ToInteger(this.moRequest["redirect"]);
                                        break;
                                    }
                            }
                            stdTools.HTTPRedirect(ref this.moCtx, cPath, ref nResponseCode);
                        }

                        else
                        {
                            this.moSession["legacyRedirect"] = "off";
                        }
                    }

                }
                this.PerfMon.Log("Web", "legacyRedirection - end");
                return bRedirect;
            }

            catch (Exception ex)
            {
                OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "legacyRedirection", ex, cProcessInfo));
                return false;
            }

        }


        public virtual Cms.Admin.AdminXforms getAdminXform()
        {
            // this is to allow us to overide adminXforms lower down
            Cms.Admin.AdminXforms oAdXfm;

            var argaWeb = this;
            oAdXfm = new Cms.Admin.AdminXforms(ref argaWeb);

            return oAdXfm;

        }

        public virtual object getXform()
        {
            // this is to allow us to overide Xforms lower down

            var argaWeb = this;
            var oXfm = new Cms.xForm(ref argaWeb);

            return oXfm;

        }

        public virtual object GetXformEditor(long contentId = 0L)
        {

            string sProcessInfo = "";
            var oInstance = new XmlDocument();
            string providerName;
            string classPath;
            string methodName = string.Empty;
            object xFrmEditor;
            try
            {

                oInstance.LoadXml(this.moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.Content, contentId));

                if (oInstance.SelectSingleNode("/tblContent/cContentXmlDetail/Content/@providerName") != null)
                {
                    providerName = oInstance.SelectSingleNode("/tblContent/cContentXmlDetail/Content/@providerName").Value;
                }
                else
                {
                    providerName = "";

                }

                if (oInstance.SelectSingleNode("/tblContent/cContentXmlDetail/Content/@xformeditor") != null)
                {
                    classPath = oInstance.SelectSingleNode("/tblContent/cContentXmlDetail/Content/@xformeditor").Value;
                }
                else
                {
                    classPath = "";
                }

                methodName = "New";

                if (!string.IsNullOrEmpty(providerName))
                {
                    Protean.ProviderSectionHandler moPrvConfig = (Protean.ProviderSectionHandler)WebConfigurationManager.GetWebApplicationSection("protean/messagingProviders");
                    var assemblyInstance = Assembly.Load(moPrvConfig.Providers[providerName].Type);
                    var calledType = assemblyInstance.GetType(classPath, true);

                    var args = new object[2];
                    args[0] = this;
                    args[1] = contentId;

                    xFrmEditor = Activator.CreateInstance(calledType, args);

                    return xFrmEditor;
                }
                else
                {
                    var argaWeb = this;
                    xFrmEditor = new Cms.Admin.XFormEditor(ref argaWeb, contentId);
                }

                return xFrmEditor;
            }

            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetXformEditor", ex, sProcessInfo));
                return null;
            }

        }

        public void GetRequestVariablesXml(ref XmlElement oPageElmt)
        {
            this.PerfMon.Log("Web", "GetRequestVariablesXml");
            XmlElement root;
            object item;
            XmlElement newElem;
            XmlElement newElem2;

            string sProcessInfo = "";
            // If mDebugMode <> "Debug" Then On Error GoTo ErrorHandler
            try
            {

                root = moPageXml.CreateElement("Request");

                // Bring in the ServerVariables
                newElem = moPageXml.CreateElement("ServerVariables");
                foreach (var currentItem in this.moRequest.ServerVariables)
                {
                    item = currentItem;
                    try
                    {
                        if (!(Conversions.ToString(item) == "ALL_HTTP" | Conversions.ToString(item) == "ALL_RAW" | string.IsNullOrEmpty(this.moRequest.ServerVariables[Conversions.ToString(item)])))
                        {
                            newElem2 = moPageXml.CreateElement("Item");
                            newElem2.SetAttribute("name", Conversions.ToString(item));
                            newElem2.InnerText = this.moRequest.ServerVariables[Conversions.ToString(item)];
                            newElem.AppendChild(newElem2);
                        }
                    }
                    catch
                    {
                        newElem2 = moPageXml.CreateElement("Item");
                        newElem2.SetAttribute("name", Conversions.ToString(item));
                        newElem.AppendChild(newElem2);
                    }


                }
                newElem2 = moPageXml.CreateElement("Item");
                newElem2.SetAttribute("name", "Date");
                newElem2.InnerText = Tools.Xml.XmlDate(DateTime.Now);
                newElem.AppendChild(newElem2);

                // Meta-Generator
                newElem2 = moPageXml.CreateElement("Item");
                newElem2.SetAttribute("name", "GENERATOR");
                newElem2.InnerText = gcGenerator;
                newElem.AppendChild(newElem2);

                newElem2 = moPageXml.CreateElement("Item");
                newElem2.SetAttribute("name", "CODEBASE");
                newElem2.InnerText = gcCodebase;
                newElem.AppendChild(newElem2);

                newElem2 = moPageXml.CreateElement("Item");
                newElem2.SetAttribute("name", "REFERENCED_ASSEMBLIES");
                newElem2.InnerText = gcReferencedAssemblies;
                newElem.AppendChild(newElem2);

                newElem2 = moPageXml.CreateElement("Item");
                newElem2.SetAttribute("name", "PREVIOUS_PAGE");
                if (this.moSession != null)
                    newElem2.InnerText = Conversions.ToString(this.moSession["previousPage"]);
                newElem.AppendChild(newElem2);

                newElem2 = moPageXml.CreateElement("Item");
                newElem2.SetAttribute("name", "SESSION_REFERRER");
                if (this.moSession != null)
                    newElem2.InnerText = Referrer;
                newElem.AppendChild(newElem2);

                if (!string.IsNullOrEmpty(this.moConfig["ProjectPath"]))
                {
                    newElem2 = moPageXml.CreateElement("Item");
                    newElem2.SetAttribute("name", "APPLICATION_ROOT");
                    if (this.moConfig["ProjectPath"].StartsWith("/"))
                    {
                        newElem2.InnerText = this.moConfig["ProjectPath"];
                    }
                    else
                    {
                        newElem2.InnerText = "/" + this.moConfig["ProjectPath"];
                    }
                    newElem.AppendChild(newElem2);
                }

                root.AppendChild(newElem);

                // Bring in the QueryString
                newElem = moPageXml.CreateElement("QueryString");
                foreach (var currentItem1 in this.moRequest.QueryString)
                {
                    item = currentItem1;
                    newElem2 = moPageXml.CreateElement("Item");
                    newElem2.SetAttribute("name", Conversions.ToString(item));
                    newElem2.InnerText = this.moRequest.QueryString[Conversions.ToString(item)];
                    newElem.AppendChild(newElem2);
                }
                root.AppendChild(newElem);

                // Bring in the Form
                newElem = moPageXml.CreateElement("Form");
                foreach (var currentItem2 in this.moRequest.Form)
                {
                    item = currentItem2;
                    newElem2 = moPageXml.CreateElement("Item");
                    newElem2.SetAttribute("name", Conversions.ToString(item));
                    newElem2.InnerText = this.moRequest.Form[Conversions.ToString(item)];
                    newElem.AppendChild(newElem2);
                }
                root.AppendChild(newElem);


                // Code to store and retain google campaign codes with the session and output in XML.

                newElem = moPageXml.CreateElement("GoogleCampaign");
                string sVariables = "source,medium,term,content,campaign";
                string[] aVariables = sVariables.Split(',');
                int i;
                bool bAppend = false;

                var loopTo = Information.UBound(aVariables);
                for (i = 0; i <= loopTo; i++)
                {
                    if (this.moRequest.QueryString.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(this.moRequest["utm_" + aVariables[i]]))
                        {
                            var oCookie = new System.Web.HttpCookie("utm_" + aVariables[i], this.moRequest["utm_" + aVariables[i]]);
                            oCookie.Expires = DateAndTime.DateAdd(DateInterval.Day, 14d, DateTime.Now);
                            this.moResponse.AppendCookie(oCookie);
                            // moSession("utm_" & aVariables(i)) = moRequest("utm_" & aVariables(i))
                        }
                        if (!(this.moRequest.Cookies.Get("utm_" + aVariables[i]) is null & string.IsNullOrEmpty(this.moRequest["utm_" + aVariables[i]])))
                        {
                            newElem2 = moPageXml.CreateElement("Item");
                            newElem2.SetAttribute("name", "utm_" + aVariables[i]);
                            if (string.IsNullOrEmpty(this.moRequest["utm_" + aVariables[i]]))
                            {
                                newElem2.InnerText = this.moRequest["utm_" + aVariables[i]];
                            }
                            else
                            {
                                newElem2.InnerText = this.moRequest.Cookies.Get("utm_" + aVariables[i]).Value;
                            }
                            // newElem2.InnerText = moSession("utm_" & aVariables(i))
                            newElem.AppendChild(newElem2);
                            bAppend = true;
                        }
                    }
                }
                if (bAppend)
                    root.AppendChild(newElem);

                if (oPageElmt.SelectSingleNode("/Page/Request") is null)
                {
                    oPageElmt.AppendChild(oPageElmt.OwnerDocument.ImportNode(root, true));
                }
                else
                {
                    oPageElmt.ReplaceChild(oPageElmt.OwnerDocument.ImportNode(root, true), oPageElmt.SelectSingleNode("/Page/Request"));
                }
            }

            catch (Exception ex)
            {

                // returnException(msException, mcModuleName, "buildPageXML", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetRequestVariablesXml", ex, sProcessInfo));
            }


        }

        public void GetSettingsXml(ref XmlElement oPageElmt)
        {
            this.PerfMon.Log("Web", "GetSettingsXml");
            XmlElement root;
            string sProcessInfo = "";
            try
            {
                if (this.moRequest["reBundle"] != null)
                {
                    this.moCtx.Application["ewSettings"] = (object)null;
                }
                if (this.moCtx.Application["ewSettings"] is null)
                {
                    root = moPageXml.CreateElement("Settings");

                    // Please never add any setting here you do not want to be publicly accessible.
                    object s = "web.DescriptiveContentURLs;web.BaseUrl;web.SiteName;web.SiteLogo;web.GoogleAnalyticsUniversalID;web.GoogleGA4MeasurementID;web.GoogleTagManagerID;web.GoogleAPIKey;web.PayPalTagManagerID;web.ScriptAtBottom;web.debug;cart.SiteURL;web.ImageRootPath;web.DocRootPath;web.MediaRootPath;web.menuNoReload;web.RootPageId;web.MenuTreeDepth;";
                    s = Operators.AddObject(s, "web.eonicwebProductName;web.eonicwebCMSName;web.eonicwebAdminSystemName;web.eonicwebCopyright;web.eonicwebSupportTelephone;web.eonicwebWebsite;web.eonicwebSupportEmail;web.eonicwebLogo;web.websitecreditURL;web.websitecreditText;web.websitecreditLogo;web.GoogleTagManagerID;web.GoogleOptimizeID;web.FeedOptimiseID;web.FacebookPixelId;web.BingTrackingID;web.ReCaptchaKey;web.EnableWebP;web.EnableRetina;");
                    s = Operators.AddObject(s, "theme.BespokeBoxStyles;theme.BespokeBackgrounds;theme.BespokeTextClasses;");
                    s = Operators.ConcatenateObject(Operators.AddObject(s, this.moConfig["XmlSettings"]), ";");

                    var match = Regex.Match(Conversions.ToString(s), @"(?<Name>[^\.]*)\.(?<Value>[^;]*);?");

                    System.Collections.Specialized.NameValueCollection moCartConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/cart");
                    System.Collections.Specialized.NameValueCollection oThemeConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/theme");
                    string sCurrentTheme = null;
                    if (oThemeConfig != null)
                    {
                        sCurrentTheme = oThemeConfig["CurrentTheme"];
                        XmlElement themesetting = moPageXml.CreateElement("add");
                        themesetting.SetAttribute("key", "theme.CurrentTheme");
                        themesetting.SetAttribute("value", sCurrentTheme);
                        root.AppendChild((XmlNode)themesetting);
                    }
                    while (match.Success)
                    {

                        XmlElement setting = moPageXml.CreateElement("add");
                        setting.SetAttribute("key", match.Groups["Name"].Value + "." + match.Groups["Value"].Value);
                        switch (match.Groups["Name"].Value ?? "")
                        {
                            case "web":
                                {
                                    setting.SetAttribute("value", this.moConfig[match.Groups["Value"].Value]);
                                    break;
                                }
                            case "cart":
                                {
                                    if (moCartConfig != null)
                                    {
                                        setting.SetAttribute("value", moCartConfig[match.Groups["Value"].Value]);
                                    }

                                    break;
                                }
                            case "theme":
                                {
                                    if (sCurrentTheme != null & match.Groups["Value"].Value != "CurrentTheme")
                                    {
                                        setting.SetAttribute("value", oThemeConfig[sCurrentTheme + "." + match.Groups["Value"].Value]);
                                    }

                                    break;
                                }
                        }
                        root.AppendChild((XmlNode)setting);
                        match = match.NextMatch();
                    }

                    // create a random bundle version number
                    XmlElement rnsetting = moPageXml.CreateElement("add");
                    rnsetting.SetAttribute("key", "bundleVersion");
                    var rn = new Random();
                    rnsetting.SetAttribute("value", rn.Next(10000, 99999).ToString());
                    root.AppendChild((XmlNode)rnsetting);

                    this.moCtx.Application["ewSettings"] = root.InnerXml;
                }
                else
                {
                    root = moPageXml.CreateElement("Settings");
                    root.InnerXml = Conversions.ToString(this.moCtx.Application["ewSettings"]);
                }
                oPageElmt.AppendChild(root);
            }

            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetSettingsXml", ex, sProcessInfo));
            }


        }

        public XmlElement GetUserXML(long nUserId = 0L)
        {
            this.PerfMon.Log("Web", "GetUserXML");
            string sProcessInfo = "";
            try
            {
                Cms argmyWeb = this;
                return moMemProv.Activities.GetUserXML(ref argmyWeb, nUserId);
            }

            catch (Exception ex)
            {

                // returnException(msException, mcModuleName, "getUserXml", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetUserXml", ex, sProcessInfo));
                return null;
            }

        }

        public void RefreshUserXML()
        {
            this.PerfMon.Log("Web", "GetUserXML");
            string sProcessInfo = "";
            XmlElement oUserXml;
            try
            {
                if (this.mnUserId != 0)
                {
                    oUserXml = (XmlElement)moPageXml.SelectSingleNode("/Page/User");
                    if (oUserXml is null)
                    {

                        moPageXml.DocumentElement.AppendChild(moPageXml.ImportNode(GetUserXML((long)this.mnUserId), true));
                    }
                    // moPageXml.DocumentElement.AppendChild(GetUserXML(mnUserId))
                    else
                    {
                        moPageXml.DocumentElement.ReplaceChild(GetUserXML((long)this.mnUserId), oUserXml);
                    }
                }
            }
            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "RefreshUserXML", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "RefreshUserXML", ex, sProcessInfo));
            }

        }

        /// <summary>
        ///    Gets the Structure XML, but doesn't add it to the page XML
        /// </summary>
        /// <param name="nUserId">User ID that you want to check for permissions on - if in Admin Mode, then if this is -1, no permissions will be checked, otherwise the permissions will be checked AND enumerated</param>
        /// <param name="nRootId">The root ID of the Structure that you want to get</param>
        /// <param name="nCloneContextId">If the root ID occurs within a cloned part of the structure, then give the cloned part's root node id</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual XmlElement GetStructureXML(long nUserId = 0L, long nRootId = 0L, long nCloneContextId = 0L)
        {

            string cFunctionDef = "GetStructureXML([Long], [Long], [Long])";
            this.PerfMon.Log("Web", cFunctionDef);

            try
            {

                return GetStructureXML(nUserId, nRootId, nCloneContextId, "", false, false, true, false, false, "MenuItem", "Menu");
            }

            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, cFunctionDef, ex, ""));
                return null;
            }

        }

        /// <summary>
        ///    Gets the Structure XML, and adds it to the page XML
        /// </summary>
        /// <param name="cMenuId">Adds an id attribute to the root node</param>
        /// <param name="nUserId">User ID that you want to check for permissions on - if in Admin Mode, then if this is -1, no permissions will be checked, otherwise the permissions will be checked AND enumerated</param>
        /// <param name="nRootId">The root ID of the Structure that you want to get</param>
        /// <param name="bLockRoot">Adds a loced attribute to the root node</param>
        /// <param name="nCloneContextId">If the root ID occurs within a cloned part of the structure, then give the cloned part's root node id</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual XmlElement GetStructureXML(string cMenuId, long nUserId = 0L, long nRootId = 0L, bool bLockRoot = false, long nCloneContextId = 0L)
        {

            string cFunctionDef = "GetStructureXML(String, [Long], [Long], [Boolean], [Long])";
            this.PerfMon.Log("Web", cFunctionDef);

            try
            {

                return GetStructureXML(nUserId, nRootId, nCloneContextId, cMenuId, true, bLockRoot, true, false, false, "MenuItem", "Menu");
            }

            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, cFunctionDef, ex, ""));
                return null;
            }

        }

        public virtual XmlElement GetStructurePageXML(long nPageId, string cMenuItemNodeName, string cRootNodeName)
        {

            string cFunctionDef = "GetStructureXML(String, [Long], [Long], [Boolean], [Long])";
            this.PerfMon.Log("Web", cFunctionDef);

            string sSql;
            DataSet oDs;
            string sProcessInfo;
            XmlElement oElmt = null;

            try
            {

                sSql = "SELECT		" + " s.nStructKey as id, " + " s.nStructParId as parId, " + " s.cStructName as name, " + " s.cUrl as url, " + " s.cStructDescription as Description, " + " a.dPublishDate as publish, " + " a.dExpireDate as expire, " + " a.nStatus as status, " + " s.cStructForiegnRef as ref, " + " 'ADMIN' as access,	" + " s.cStructLayout as layout," + " s.nCloneStructId as clone," + " '' As accessSource," + " 0 As accessSourceId, " + " s.nVersionParId as vParId" + " FROM	tblContentStructure s" + " INNER JOIN  tblAudit a " + " ON s.nAuditId = a.nAuditKey" + " where(s.nVersionParId Is null Or s.nVersionParId = 0)" + " and s.nStructKey = " + nPageId;


                // Get the dataset
                oDs = this.moDbHelper.GetDataSet(sSql, cMenuItemNodeName, cRootNodeName);

                // Add nestings
                oDs.Relations.Add("rel01", oDs.Tables[0].Columns["id"], oDs.Tables[0].Columns["parId"], false);
                oDs.Relations["rel01"].Nested = true;

                if (oDs.Tables[0].Rows.Count > 0)
                {

                    // COLUMN MAPPING - STANDARD
                    // =========================
                    oDs.Tables[0].Columns[0].ColumnMapping = MappingType.Attribute;
                    oDs.Tables[0].Columns[1].ColumnMapping = MappingType.Attribute;
                    oDs.Tables[0].Columns[2].ColumnMapping = MappingType.Attribute;
                    oDs.Tables[0].Columns[3].ColumnMapping = MappingType.Attribute;
                    oDs.Tables[0].Columns["url"].ColumnMapping = MappingType.Attribute;
                    if (this.mbAdminMode)
                    {
                        oDs.Tables[0].Columns["status"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["publish"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["expire"].ColumnMapping = MappingType.Attribute;
                    }

                    // COLUMN MAPPING - ACCESS
                    // =========================
                    oDs.Tables[0].Columns["access"].ColumnMapping = MappingType.Attribute;
                    if (oDs.Tables[0].Columns.Contains("accessSource"))
                    {
                        oDs.Tables[0].Columns["accessSource"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["accessSourceId"].ColumnMapping = MappingType.Attribute;
                    }

                    if (oDs.Tables[0].Columns.Contains("vParId"))
                    {
                        oDs.Tables[0].Columns["vParId"].ColumnMapping = MappingType.Attribute;
                    }

                    if (oDs.Tables[0].Columns.Contains("ref"))
                    {
                        oDs.Tables[0].Columns["ref"].ColumnMapping = MappingType.Attribute;
                    }

                    // COLUMN MAPPING - CLONES
                    // =========================
                    if (oDs.Tables[0].Columns.Contains("clone"))
                        oDs.Tables[0].Columns["clone"].ColumnMapping = MappingType.Attribute;


                    // COVERT DATASET TO XML
                    // =====================
                    oElmt = moPageXml.CreateElement(cRootNodeName);


                    sProcessInfo = "GetStructureXML-dsToXml";
                    this.PerfMon.Log("Web", sProcessInfo);

                    // TS added lines to avoid whitespace issues
                    var oXml = new XmlDocument();
                    oXml.LoadXml(oDs.GetXml());
                    oXml.PreserveWhitespace = false;

                    oElmt.InnerXml = oXml.DocumentElement.OuterXml;
                    string sContent;
                    // Convert any text to xml
                    foreach (XmlElement oElmt2 in oElmt.SelectNodes("descendant-or-self::" + cMenuItemNodeName + "/Description"))
                    {
                        sContent = oElmt2.InnerText;
                        if (!string.IsNullOrEmpty(sContent))
                        {
                            try
                            {
                                oElmt2.InnerXml = sContent;
                            }
                            catch
                            {
                                oElmt2.InnerXml = Tools.Text.tidyXhtmlFrag(sContent);
                            }
                        }
                    }

                }

                return oElmt;
            }

            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, cFunctionDef, ex, ""));
                return null;
            }

        }

        /// <summary>
        /// Gets the Structure XML
        /// </summary>
        /// <param name="nUserId">User ID that you want to check for permissions on - if in Admin Mode, then if this is -1, no permissions will be checked, otherwise the permissions will be checked AND enumerated</param>
        /// <param name="nRootId">The root ID of the Structure that you want to get</param>
        /// <param name="nCloneContextId">If the root ID occurs within a cloned part of the structure, then give the cloned part's root node id</param>
        /// <param name="cMenuId">Adds an id attribute to the root node</param>
        /// <param name="bAddMenuToPageXML">If True, this will add the elmt to the page XML</param>
        /// <param name="bLockRoot">Adds a loced attribute to the root node</param>
        /// <param name="bUseCache">Can be used to force use of the Cache</param>
        /// <param name="bIncludeExpiredAndHidden">Include pages that are expired, yet to be published or hidden.</param>
        /// <param name="bPruneEvenIfInAdminMode">By default in admin mode all pages even DENIED ones are returned.  If this value is True then regardless of the admin mode status DENIED pages will be removed.</param>
        /// <param name="cMenuItemNodeName">Specifies the node name of each Page node.  Default should be "MenuItem".  Note - if not default, then caching will not be employed.</param>
        /// <param name="cRootNodeName">Specifies the root node name.  Default should be "Menu".  Note - if not default, then caching will not be employed.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual XmlElement GetStructureXML(long nUserId, long nRootId, long nCloneContextId, string cMenuId, bool bAddMenuToPageXML, bool bLockRoot, bool bUseCache, bool bIncludeExpiredAndHidden, bool bPruneEvenIfInAdminMode, string cMenuItemNodeName, string cRootNodeName)
        {
            string cFunctionDef = "GetStructureXML(Long,Long,Long,String,Boolean,Boolean,Boolean,Boolean,Boolean,String,String)";
            this.PerfMon.Log("Web", cFunctionDef);

            DataSet oDs;
            XmlElement oElmt = null;
            XmlElement oClone = null;
            int nCloneId = 0;
            int nCloneParentId = 0;

            int nTempRootId = 0;
            XmlElement oMenuItem;

            long nAuthUsers = gnAuthUsers;

            string sContent;
            string sSql;
            string cCacheMode = "off";
            bool bCacheXml = false;
            string cFilePathModifier = "";
            bool bAuth = true;

            string sProcessInfo = Conversions.ToString(string.IsNullOrEmpty("GetStructureXML"));
            string cCacheType;

            try
            {

                // INITIALISE VARIABLES
                // =====================

                // Node names
                if (string.IsNullOrEmpty(cMenuItemNodeName))
                    cMenuItemNodeName = "MenuItem";
                if (string.IsNullOrEmpty(cRootNodeName))
                    cRootNodeName = "Menu";
                cCacheType = cRootNodeName + "/" + cMenuItemNodeName;

                // Override the cache if we're not getting menu items
                // If cMenuItemNodeName <> "MenuItem" And cRootNodeName <> "Menu" Then bUseCache = False

                // Project Path modifier
                if (this.moConfig["ProjectPath"] != null)
                {
                    cFilePathModifier = this.moConfig["ProjectPath"].TrimEnd('/').TrimStart('/');
                    if (!string.IsNullOrEmpty(cFilePathModifier))
                        cFilePathModifier = "/" + cFilePathModifier;
                }

                // Root Id
                if (nRootId == 0L)
                    nRootId = RootPageId;

                // User Id
                // If AdminMode and no user then set user to be -1 otherwise apply the userid
                if (nUserId == 0L)
                    nUserId = Conversions.ToLong(Interaction.IIf(this.mbAdminMode, (object)-1, (object)this.mnUserId));
                if (nUserId == -1)
                    bAuth = false;

                // Set Non-Authenticated user permissions
                // Don't forget to clear authenticated users if the user is not logged on.
                if (gbMembership)
                {
                    if (nUserId == 0L | nUserId == gnNonAuthUsers)
                    {
                        nAuthUsers = 0L;
                        if (nUserId == 0L & gnNonAuthUsers != 0)
                        {
                            nUserId = gnNonAuthUsers;
                            bAuth = false;
                        }
                    }
                }
                else
                {
                    bAuth = false;
                }

                // CACHE MODE
                // ===========
                // Work out whether or not to use site structure caching
                // Only check caching if user is logged on and is not admin mode, and caching has been turned on

                // If we are indexing from SOAP we have no session object therefore we don't use cache
                if (this.moSession is null)
                    bUseCache = false;
                // If we are in admin mode we don't use cache
                // If mbAdminMode Then bUseCache = False

                // Site caching can be turned on through a site request - this will only work for sites with membership
                // OR if site caching is turned on and membership is off, then save a single site structure for all users (i.e. don;t use session)
                if (bUseCache)
                {
                    sProcessInfo = "GetStructureXML-CheckCaching";
                    this.PerfMon.Log("Web", sProcessInfo);

                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(this.moSession["cacheMode"], "", false)))
                    {
                        cCacheMode = Conversions.ToString(this.moSession["cacheMode"]);
                    }
                    else if (!string.IsNullOrEmpty(this.moConfig["SiteCache"]))
                    {
                        cCacheMode = Conversions.ToString(Interaction.IIf(gbSiteCacheMode, "on", "off"));
                    }

                    // Check out of a caching override has been passed through
                    if (!string.IsNullOrEmpty(this.moRequest["cacheMode"]))
                    {
                        cCacheMode = Strings.LCase(this.moRequest["cacheMode"]);
                    }

                    // Store the cache to the session
                    this.moSession["cacheMode"] = cCacheMode;

                    // If Cache is on then check for a cached strcture
                    if (cCacheMode == "on")
                    {

                        var oCache = moPageXml.CreateElement(cRootNodeName);

                        if (this.mbAdminMode & cCacheType == "Menu/MenuItem")
                        {
                            oCache.InnerXml = Conversions.ToString(this.moCtx.Application["AdminStructureCache"]);
                        }
                        else
                        {


                            string cacheSearchCriteria = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(" WHERE nCacheDirId = ", SqlFmt(nUserId.ToString())), " AND cCacheType='"), cCacheType), "'"));
                            if (bAuth)
                            {
                                cacheSearchCriteria += " AND cCacheSessionID = '" + this.moSession.SessionID + "' AND DATEDIFF(hh,dCacheDate,GETDATE()) > 12";
                            }
                            sProcessInfo = "GetStructureXML-SelectFromCache";
                            this.PerfMon.Log("Web", sProcessInfo);
                            // Get the cached structure - returns empty string if no structure found.
                            sSql = "SELECT TOP 1 cCacheStructure FROM dbo.tblXmlCache " + cacheSearchCriteria;

                            sProcessInfo = "GetStructureXML-getCachefromDB-Start";
                            this.PerfMon.Log("Web", sProcessInfo);

                            this.moDbHelper.AddXMLValueToNode(sSql, ref oCache);

                            sProcessInfo = "GetStructureXML-getCachefromDB-End";
                            this.PerfMon.Log("Web", sProcessInfo);


                        }

                        if (cRootNodeName != "Menu")
                        {
                            foreach (XmlElement currentOElmt in oCache.SelectNodes("descendant-or-self::Menu"))
                            {
                                oElmt = currentOElmt;
                                Tools.Xml.renameNode(ref oElmt, cRootNodeName);
                            }
                        }

                        if (cMenuItemNodeName != "MenuItem")
                        {
                            foreach (XmlElement currentOElmt1 in oCache.SelectNodes("descendant-or-self::MenuItem"))
                            {
                                oElmt = currentOElmt1;
                                Tools.Xml.renameNode(ref oElmt, cMenuItemNodeName);
                            }
                        }



                        if (oCache.FirstChild != null)
                        {
                            bCacheXml = true;
                            oElmt = oCache;
                        }


                    }

                }

                // If we don't have a cache to check then get a new structure
                if (bCacheXml == false | cCacheMode == "off")
                {

                    // DATA CALL : GET STRUCTURE
                    // =========================
                    // 
                    // This will return structure nodes, that will optionally have the following:
                    // - checks for page level permissions (indicated by nUserId not being -1) - if these are returned they will need to cleaned up.
                    // - enumerate who teh permissions have come from (indicated by nUserId not being -1 and badminMode being 1)
                    // - exclude expired, not yet published and hidden pages if not in adminmode.

                    // If preview mode is set to show hidden
                    bool spoofAdminMode = this.mbAdminMode;
                    if (this.mbPreviewHidden)
                    {
                        bIncludeExpiredAndHidden = true;
                    }

                    sSql = "EXEC getContentStructure_v2 @userId=" + nUserId + ", @bAdminMode=" + Conversions.ToInteger(this.mbAdminMode) + ", @dateNow=" + sqlDate(mdDate) + ", @authUsersGrp = " + nAuthUsers + ", @bReturnDenied=1";

                    sProcessInfo = "GetStructureXML-getContentStrcuture";
                    this.PerfMon.Log("Web", sProcessInfo);

                    if (bIncludeExpiredAndHidden)
                        sSql += ",@bShowAll=1";

                    // Get the dataset
                    oDs = this.moDbHelper.GetDataSet(sSql, cMenuItemNodeName, cRootNodeName);

                    // Add Page Version Info
                    if (this.Features.ContainsKey("PageVersions"))
                    {
                        if (this.mbAdminMode & nUserId == -1)
                        {
                            // check we are returning strucutre for current user and not for another user such as in bespoke report (PDP for LMS system).
                            sSql = "EXEC getAllPageVersions";
                        }
                        else
                        {
                            sSql = "EXEC getUserPageVersions @userId=" + nUserId + ", @dateNow=" + sqlDate(mdDate) + ", @authUsersGrp = " + nAuthUsers + ", @bReturnDenied=0, @bShowAll=0";
                        }

                        sProcessInfo = "GetStructureXML-getPageVersions";
                        this.PerfMon.Log("Web", sProcessInfo);

                        this.moDbHelper.addTableToDataSet(ref oDs, sSql, "PageVersion");
                        oDs.Relations.Add("rel02", oDs.Tables[0].Columns["id"], oDs.Tables[1].Columns["vParId"], false);
                        oDs.Relations["rel02"].Nested = true;

                        if (oDs.Tables[1].Rows.Count > 0)
                        {
                            oDs.Tables[1].Columns["id"].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[1].Columns["parId"].ColumnMapping = MappingType.Hidden;
                            oDs.Tables[1].Columns["name"].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[1].Columns["url"].ColumnMapping = MappingType.Attribute;
                            // oDs.Tables(1).Columns("Description").ColumnMapping = Data.MappingType.SimpleContent
                            oDs.Tables[1].Columns["publish"].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[1].Columns["expire"].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[1].Columns["status"].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[1].Columns["access"].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[1].Columns["layout"].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[1].Columns["clone"].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[1].Columns["vParId"].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[1].Columns["lang"].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[1].Columns["desc"].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[1].Columns["verType"].ColumnMapping = MappingType.Attribute;
                        }

                    }

                    // Add nestings
                    oDs.Relations.Add("rel01", oDs.Tables[0].Columns["id"], oDs.Tables[0].Columns["parId"], false);
                    oDs.Relations["rel01"].Nested = true;

                    if (oDs.Tables[0].Rows.Count > 0)
                    {

                        // COLUMN MAPPING - STANDARD
                        // =========================
                        oDs.Tables[0].Columns[0].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[1].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[2].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[3].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["url"].ColumnMapping = MappingType.Attribute;
                        if (this.mbAdminMode)
                        {
                            oDs.Tables[0].Columns["status"].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[0].Columns["publish"].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[0].Columns["expire"].ColumnMapping = MappingType.Attribute;
                        }

                        // COLUMN MAPPING - ACCESS
                        // =========================
                        oDs.Tables[0].Columns["access"].ColumnMapping = MappingType.Attribute;
                        if (oDs.Tables[0].Columns.Contains("accessSource"))
                        {
                            oDs.Tables[0].Columns["accessSource"].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[0].Columns["accessSourceId"].ColumnMapping = MappingType.Attribute;
                        }

                        if (oDs.Tables[0].Columns.Contains("vParId"))
                        {
                            oDs.Tables[0].Columns["vParId"].ColumnMapping = MappingType.Attribute;
                        }

                        if (oDs.Tables[0].Columns.Contains("ref"))
                        {
                            oDs.Tables[0].Columns["ref"].ColumnMapping = MappingType.Attribute;
                        }

                        // COLUMN MAPPING - CLONES
                        // =========================
                        if (oDs.Tables[0].Columns.Contains("clone"))
                            oDs.Tables[0].Columns["clone"].ColumnMapping = MappingType.Attribute;


                        // COVERT DATASET TO XML
                        // =====================
                        oElmt = moPageXml.CreateElement(cRootNodeName);

                        sProcessInfo = "GetStructureXML-dsToXml";
                        this.PerfMon.Log("Web", sProcessInfo);

                        // TS added lines to avoid whitespace issues
                        var oXml = new XmlDocument();
                        oXml.LoadXml(oDs.GetXml());
                        oXml.PreserveWhitespace = false;

                        oElmt.InnerXml = oXml.DocumentElement.OuterXml;



                        // MENU TIDY UP
                        // ==================
                        // 'Rename the VersionMenuNodes
                        // If mbAdminMode And Features.ContainsKey("PageVersions") Then
                        // sProcessInfo = "GetStructureXML-RenameVersions"
                        // PerfMon.Log("Web", sProcessInfo)
                        // Dim oVersionMenuItems As XmlNodeList = oElmt.SelectNodes(cRootNodeName & "/" & cMenuItemNodeName & "[@vParId!='']")
                        // For Each oVerMenuItem As XmlElement In oVersionMenuItems
                        // Tools.Xml.renameNode(oVerMenuItem, "PageVersion")
                        // Next
                        // End If

                        // REMOVE THE ORPHANS
                        // ==================
                        // Because the SQL may not return DENIED pages, we need to check for orphaned items
                        // Orphaned records are ones that have no parent menu node (because it wasn't returned by the SQL)
                        // By default these will be returned to the root node, which will also include the root page node - which we don't want to delete.
                        // The genuine root node will be node with a parId of 0 (possibly also including System Pages)

                        sProcessInfo = "GetStructureXML-CleanOrphans";
                        this.PerfMon.Log("Web", sProcessInfo);

                        var oRootMenuItems = oElmt.SelectNodes(cRootNodeName + "/" + cMenuItemNodeName + "[@parId!=0]");
                        foreach (XmlNode oRootMenuItem in oRootMenuItems)
                            oRootMenuItem.ParentNode.RemoveChild(oRootMenuItem);



                        // DETERMINE THE ROOT NODE ID
                        // ==========================
                        // We need to determine the root id
                        // This allows us to not do unnecessary cloning
                        // If a clonecontextnode has been passed, then we need to get that for now

                        sProcessInfo = "GetStructureXML-GetIndicativeRootId";
                        this.PerfMon.Log("Web", sProcessInfo);

                        if (nCloneContextId > 0L)
                        {
                            nTempRootId = (int)nCloneContextId;
                        }
                        else if (nRootId > 0L)
                        {
                            nTempRootId = (int)nRootId;
                        }
                        else
                        {
                            nTempRootId = Conversions.ToInteger(oElmt.FirstChild.FirstChild.Attributes["id"].Value);
                        }



                        // GET CLONED NODES
                        // ==================
                        // Note - we only want to find cloned nodes under the root id in question
                        if (gbClone)
                        {

                            this.PerfMon.Log("Web", "GetStructureXML-cloneNodes");
                            var cNodeSnapshot = new Hashtable();

                            // GET CLONE SNAPSHOT
                            // Why snapshot?  It stops cloned pages that also may have parent nodes cloned later changing through this process.
                            foreach (XmlElement currentOMenuItem in oElmt.SelectNodes("descendant-or-self::" + cMenuItemNodeName + "[@id='" + nTempRootId + "']/descendant-or-self::" + cMenuItemNodeName + "[@clone and not(@clone=0)]"))
                            {
                                oMenuItem = currentOMenuItem;
                                // Go and get the cloned node
                                nCloneId = Conversions.ToInteger(oMenuItem.GetAttribute("clone"));
                                if (!(Tools.Xml.NodeState(ref oElmt, "descendant-or-self::" + cMenuItemNodeName + "[@id='" + nCloneId + "']", ref oClone) == Tools.Xml.XmlNodeState.NotInstantiated))
                                {
                                    if (oClone != null)
                                    {
                                        if (!cNodeSnapshot.ContainsKey(nCloneId))
                                            cNodeSnapshot.Add(nCloneId, oClone.InnerXml);
                                    }

                                }
                            }

                            // ADD CLONE SNAPSHOTS TO MENU
                            // Run through the nodes again, this time processing them 
                            foreach (XmlElement currentOMenuItem1 in oElmt.SelectNodes("descendant-or-self::" + cMenuItemNodeName + "[@id='" + nTempRootId + "']/descendant-or-self::" + cMenuItemNodeName + "[@clone and not(@clone=0)]"))
                            {
                                oMenuItem = currentOMenuItem1;

                                // Go and get the cloned node
                                nCloneId = Conversions.ToInteger(oMenuItem.GetAttribute("clone"));
                                nCloneParentId = Conversions.ToInteger(oMenuItem.GetAttribute("id"));
                                if (!(Tools.Xml.NodeState(ref oElmt, "descendant-or-self::" + cMenuItemNodeName + "[@id='" + nCloneId + "']", "", "", XmlNodeState.IsEmpty, oClone, returnAsXml: "", returnAsText: "", bCheckTrimmedInnerText: false) == Tools.Xml.XmlNodeState.NotInstantiated))
                                {

                                    oMenuItem.InnerXml = Conversions.ToString(cNodeSnapshot[nCloneId]);

                                    // Add the cloned flag to nodes and children
                                    foreach (XmlElement oChild in oMenuItem.SelectNodes("descendant-or-self::" + cMenuItemNodeName))
                                        oChild.SetAttribute("cloneparent", nCloneParentId.ToString());
                                }
                            }

                        }


                        // PROPOGATE THE PERMISSIONS AND PRUNE
                        // ===================================
                        sProcessInfo = "GetStructureXML-TidyMenunode";
                        this.PerfMon.Log("Web", sProcessInfo);
                        XmlElement argoMenuItem = (XmlElement)oElmt.FirstChild;
                        this.TidyMenunode(ref argoMenuItem, "OPEN", "", bPruneEvenIfInAdminMode | !this.mbAdminMode, nUserId, cMenuItemNodeName, cRootNodeName);

                        foreach (XmlElement currentOMenuItem2 in oElmt.SelectNodes("descendant-or-self::" + cMenuItemNodeName + " | descendant-or-self::PageVersion"))
                        {
                            oMenuItem = currentOMenuItem2;
                            // For Each oMenuItem In oElmt.SelectNodes("descendant-or-self::*")
                            XmlElement oDesc = (XmlElement)oMenuItem.SelectSingleNode("Description");
                            if (oDesc != null)
                            {

                                // First try to convert the node into Xml
                                sContent = oDesc.InnerText + "";
                                try
                                {
                                    oDesc.InnerXml = sContent;
                                }
                                catch
                                {
                                    oDesc.InnerText = sContent;
                                }

                                // we have a historic error where some sites have 2 description nodes in some database
                                // This should be data cleansed but for now we hack

                                // Work through all the child nodes of Description and move them to the MenuItem
                                foreach (XmlElement oDescChild in oDesc.SelectNodes("*[name()!='Description' or (name()='Description' and not(preceding-sibling::Description))]"))
                                    // Tidy the node
                                    // sContent = oDescChild.InnerText
                                    // If sContent <> "" Then
                                    // Try
                                    // oDescChild.InnerXml = Protean.Tools.Xml.convertEntitiesToCodes(sContent)
                                    // Catch
                                    // PerfMon.Log("Web", "GetStructureXML-tidy")
                                    // oDescChild.InnerXml = tidyXhtmlFrag(sContent)
                                    // End Try
                                    // End If

                                    // Move the node
                                    oMenuItem.InsertBefore(oDescChild.CloneNode(true), oDesc);

                                // Remove the original oDesc
                                oMenuItem.RemoveChild(oDesc);

                            }

                            // Remove the parId attribute
                            oMenuItem.RemoveAttribute("parId");

                        }

                        if (bLockRoot)
                        {
                            // LOCK THE ROOT
                            // ===============
                            sProcessInfo = "GetStructureXML-lockRoot";
                            this.PerfMon.Log("Web", sProcessInfo);
                            XmlElement oMenuFirstChild = (XmlElement)oElmt.FirstChild;
                            oMenuFirstChild.SetAttribute("Locked", Conversions.ToString(true));
                        }
                    }

                    else
                    {

                        // NO ROWS WERE FOUND - CREATE AN EMPTY Menu NODE
                        // ===============================================
                        oElmt = moPageXml.CreateElement(cRootNodeName);

                    }

                    // CACHING - ADD THE STRUCTURE TO THE CACHE
                    // ========================================
                    // Only if caching is on, user is logged on, and not in AdminMode.
                    if (bUseCache & cCacheMode == "on")
                    {
                        sProcessInfo = "GetStructureXML-addCacheToStructure";
                        this.PerfMon.Log("Web", sProcessInfo);
                        // ts this was commented out I have restored 04/11/2022 please leave not to say why commented next time
                        if (this.moRequest["reBundle"] != null)
                        {
                            this.moDbHelper.clearStructureCacheAll();
                        }
                        // only cache if MenuItem / Menu
                        if (cMenuItemNodeName == "MenuItem" & cRootNodeName == "Menu")
                        {
                            if (this.mbAdminMode)
                            {
                                this.moCtx.Application["AdminStructureCache"] = oElmt.InnerXml;
                            }
                            else
                            {
                                XmlElement argStructureXml = (XmlElement)oElmt.FirstChild;
                                this.moDbHelper.addStructureCache(bAuth, nUserId, ref cCacheType, ref argStructureXml);
                            }
                        }
                        else
                        {
                            XmlElement argStructureXml1 = (XmlElement)oElmt.FirstChild;
                            this.moDbHelper.addStructureCache(bAuth, nUserId, ref cCacheType, ref argStructureXml1);

                        }


                        // sSql = "INSERT INTO dbo.tblXmlCache (cCacheSessionID,nCacheDirId,cCacheStructure,cCacheType) " _
                        // & "VALUES (" _
                        // & "'" & IIf(bAuth, Eonic.SqlFmt(moSession.SessionID), "") & "'," _
                        // & Eonic.SqlFmt(nUserId) & "," _
                        // & "'" & Eonic.SqlFmt(oElmt.InnerXml) & "'," _
                        // & "'" & cCacheType & "'" _
                        // & ")"
                        // moDbHelper.ExeProcessSql(sSql)

                    }
                }

                // Now we need to do some page dependant processing

                // MENU TIDY: XML TIDY
                // ===================================
                sProcessInfo = "GetStructureXML-txt2xml";
                this.PerfMon.Log("Web", sProcessInfo);
                string sUrl;
                string cPageName;
                string cCloneParent;
                var oRe = new Regex("[^A-Z0-9]", RegexOptions.IgnoreCase);
                XmlElement oPageVerElmts;

                string DomainURL = mcRequestDomain;
                string ExcludeFoldersFromPaths = Strings.LCase("" + this.moConfig["ExcludeFoldersFromPaths"]);
                string[] foldersExcludedFromPaths = ExcludeFoldersFromPaths.Split(',');

                foreach (XmlElement currentOMenuItem3 in oElmt.SelectNodes("descendant-or-self::" + cMenuItemNodeName))
                {
                    oMenuItem = currentOMenuItem3;
                    string urlPrefix = "";
                    XmlElement verNodeLoop = null;
                    XmlElement verNode = null;

                    // Determine Page Version
                    if (this.Features.ContainsKey("PageVersions"))
                    {
                        if (!this.mbAdminMode | this.moRequest["ewCmd"] == "Normal" | this.moRequest["ewCmd"] == "EditContent")
                        {

                            // check for language version
                            foreach (XmlElement currentVerNodeLoop in oMenuItem.SelectNodes("PageVersion[@lang='" + gcLang + "']"))
                            {
                                verNodeLoop = currentVerNodeLoop;
                                verNode = verNodeLoop;
                                urlPrefix = mcPageLanguageUrlPrefix;
                            }

                            // check for permission version
                            foreach (XmlElement currentVerNodeLoop1 in oMenuItem.SelectNodes("PageVersion[@verType='1']"))
                            {
                                verNodeLoop = currentVerNodeLoop1;
                                if (verNode is null)
                                    verNode = verNodeLoop;
                            }

                            // update our pageId
                            if (verNode != null)
                            {
                                // Case for if our version is also the root page
                                if (nRootId == Conversions.ToDouble(oMenuItem.GetAttribute("id")))
                                {
                                    nRootId = Conversions.ToLong(verNode.GetAttribute("id"));
                                }

                                // Case if we are on the current page then we reset the mnPageId so we pull in the right content
                                if ((double)this.mnPageId == Conversions.ToDouble(oMenuItem.GetAttribute("id")))
                                {
                                    // If (verNode.GetAttribute("lang") = gcLang Or gcLang = "" Or verNode.GetAttribute("lang") = "") And verNode.GetAttribute("verType") <> 1 Then
                                    // If (verNode.GetAttribute("lang") = gcLang Or gcLang = "" Or verNode.GetAttribute("lang") = "") Then
                                    switch (Conversions.ToInteger(verNode.GetAttribute("verType")))
                                    {
                                        case 1: // case for permission version
                                            {
                                                // Dim permLevel As dbHelper.PermissionLevel = moDbHelper.getPagePermissionLevel(verNode.GetAttribute("id"))
                                                if (!this.mbAdminMode)
                                                {
                                                    this.mnPageId = Conversions.ToInteger(verNode.GetAttribute("id"));
                                                }

                                                break;
                                            }
                                        case 3: // case for language version
                                            {
                                                if ((verNode.GetAttribute("lang") ?? "") == (gcLang ?? ""))
                                                {
                                                    this.mnPageId = Conversions.ToInteger(verNode.GetAttribute("id"));
                                                }

                                                break;
                                            }

                                        default:
                                            {
                                                this.mnPageId = Conversions.ToInteger(verNode.GetAttribute("id"));
                                                break;
                                            }
                                    }
                                }

                                // create a version for the default we are replacing
                                var newVerNode = moPageXml.CreateElement("PageVersion");
                                newVerNode.SetAttribute("id", oMenuItem.GetAttribute("id"));
                                newVerNode.SetAttribute("name", oMenuItem.GetAttribute("name"));
                                newVerNode.SetAttribute("url", oMenuItem.GetAttribute("url"));
                                newVerNode.SetAttribute("publish", oMenuItem.GetAttribute("publish"));
                                newVerNode.SetAttribute("expire", oMenuItem.GetAttribute("expire"));
                                newVerNode.SetAttribute("status", oMenuItem.GetAttribute("status"));
                                newVerNode.SetAttribute("access", oMenuItem.GetAttribute("access"));
                                newVerNode.SetAttribute("layout", oMenuItem.GetAttribute("layout"));
                                string sInnerXml = string.Empty;
                                foreach (XmlElement infoElmt in oMenuItem.SelectNodes("*[name()!='PageVersion' and name()!='MenuItem']"))
                                    sInnerXml = sInnerXml + infoElmt.OuterXml;
                                newVerNode.InnerXml = sInnerXml;
                                sInnerXml = "";
                                if (this.goLangConfig != null)
                                {
                                    newVerNode.SetAttribute("lang", this.goLangConfig.GetAttribute("code"));
                                }

                                newVerNode.SetAttribute("verType", "0");
                                oMenuItem.AppendChild(newVerNode);

                                // now replace the menuitem with the node we are on
                                oMenuItem.SetAttribute("id", verNode.GetAttribute("id"));
                                oMenuItem.SetAttribute("name", verNode.GetAttribute("name"));
                                oMenuItem.SetAttribute("url", verNode.GetAttribute("url"));
                                oMenuItem.SetAttribute("publish", verNode.GetAttribute("publish"));
                                oMenuItem.SetAttribute("expire", verNode.GetAttribute("expire"));
                                oMenuItem.SetAttribute("status", verNode.GetAttribute("status"));
                                oMenuItem.SetAttribute("access", verNode.GetAttribute("access"));
                                oMenuItem.SetAttribute("layout", verNode.GetAttribute("layout"));
                                oMenuItem.SetAttribute("clone", verNode.GetAttribute("clone"));
                                oMenuItem.SetAttribute("lang", verNode.GetAttribute("lang"));
                                oMenuItem.SetAttribute("verDesc", verNode.GetAttribute("verDesc"));
                                oMenuItem.SetAttribute("verType", verNode.GetAttribute("verType"));




                                foreach (XmlElement currentOPageVerElmts in verNode.SelectNodes("*"))
                                {
                                    oPageVerElmts = currentOPageVerElmts;
                                    string nodeName = oPageVerElmts.Name;
                                    if (oMenuItem.SelectSingleNode(nodeName) != null)
                                    {
                                        oMenuItem.SelectSingleNode(nodeName).InnerXml = verNode.SelectSingleNode(nodeName).InnerXml;
                                    }
                                    else
                                    {
                                        oMenuItem.AppendChild(moPageXml.CreateElement(nodeName));
                                        oMenuItem.SelectSingleNode(nodeName).InnerXml = verNode.SelectSingleNode(nodeName).InnerXml;
                                    }
                                }
                                // If Not oMenuItem.SelectSingleNode("Description") Is Nothing Then
                                // oMenuItem.SelectSingleNode("Description").InnerText = verNode.SelectSingleNode("Description").InnerText
                                // Else
                                // oMenuItem.AppendChild(moPageXml.CreateElement("Description"))
                                // oMenuItem.SelectSingleNode("Description").InnerText = verNode.SelectSingleNode("Description").InnerText
                                // End If
                            }
                        }
                        else if (this.moRequest["ewCmd"] != "LocateContent" & this.moRequest["ewCmd"] != "MoveContent" & this.moRequest["ewCmd"] != "EditStructure")
                        {
                            foreach (XmlElement currentVerNode in oMenuItem.SelectNodes("PageVersion[@id=" + this.mnPageId + "]"))
                            {
                                verNode = currentVerNode;
                                if (Conversions.ToDouble(oMenuItem.GetAttribute("id")) == nRootId)
                                {
                                    // case for replacing homepage in admin
                                    nRootId = Conversions.ToLong(verNode.GetAttribute("id"));
                                }
                                // update menu item with current page
                                oMenuItem.SetAttribute("id", verNode.GetAttribute("id"));
                                oMenuItem.SetAttribute("name", verNode.GetAttribute("name"));
                                oMenuItem.SetAttribute("url", verNode.GetAttribute("url"));
                                oMenuItem.SetAttribute("publish", verNode.GetAttribute("publish"));
                                oMenuItem.SetAttribute("expire", verNode.GetAttribute("expire"));
                                oMenuItem.SetAttribute("status", verNode.GetAttribute("status"));
                                oMenuItem.SetAttribute("access", verNode.GetAttribute("access"));
                                oMenuItem.SetAttribute("layout", verNode.GetAttribute("layout"));
                                oMenuItem.SetAttribute("clone", Conversions.ToString(Interaction.IIf(string.IsNullOrEmpty(verNode.GetAttribute("clone")), "0", verNode.GetAttribute("clone"))));
                                oMenuItem.SetAttribute("lang", verNode.GetAttribute("lang"));
                                oMenuItem.SetAttribute("verDesc", verNode.GetAttribute("desc"));
                                oMenuItem.SetAttribute("verType", verNode.GetAttribute("verType"));

                                foreach (XmlElement currentOPageVerElmts1 in verNode.SelectNodes("*"))
                                {
                                    oPageVerElmts = currentOPageVerElmts1;
                                    string nodeName = oPageVerElmts.Name;
                                    if (oMenuItem.SelectSingleNode(nodeName) != null)
                                    {
                                        oMenuItem.SelectSingleNode(nodeName).InnerXml = verNode.SelectSingleNode(nodeName).InnerXml;
                                    }
                                    else
                                    {
                                        oMenuItem.AppendChild(moPageXml.CreateElement(nodeName));
                                        oMenuItem.SelectSingleNode(nodeName).InnerXml = verNode.SelectSingleNode(nodeName).InnerXml;
                                    }
                                }
                                // If Not oMenuItem.SelectSingleNode("Description") Is Nothing Then
                                // oMenuItem.SelectSingleNode("Description").InnerText = verNode.SelectSingleNode("Description").InnerText
                                // Else
                                // oMenuItem.AppendChild(moPageXml.CreateElement("Description"))
                                // oMenuItem.SelectSingleNode("Description").InnerText = verNode.SelectSingleNode("Description").InnerText
                                // End If
                            }
                            verNode = null;

                        }
                    }

                    if (this.goLangConfig != null)
                    {

                        if (verNode is null & this.goLangConfig.GetAttribute("localDefaults") != "off")
                        {
                            urlPrefix = mcPageLanguageUrlPrefix; // & cFilePathModifier
                        }

                    }



                    // Only generate URLs for MneuItems that do not already have a url explicitly defined
                    if (string.IsNullOrEmpty(oMenuItem.GetAttribute("url")))
                    {

                        // Start with the base path
                        sUrl = this.moConfig["BasePath"] + urlPrefix + cFilePathModifier;

                        if (this.moConfig["UsePageIdsForURLs"] == "on")
                        {
                            // Use the page ID instead of a Pretty URL
                            sUrl = sUrl + "/?pgid=" + oMenuItem.GetAttribute("id");
                        }
                        else
                        {
                            // Get all the descendant menuitems and append the names onto the Url string
                            foreach (XmlElement oDescendant in oMenuItem.SelectNodes("ancestor-or-self::" + cMenuItemNodeName + "[ancestor::MenuItem[@id=" + nRootId + "]]"))
                            {
                                if (!(oDescendant.ParentNode.Name == "Menu"))
                                {
                                    if (this.moConfig["PageURLFormat"] == "hyphens")
                                    {
                                        cPageName = oRe.Replace(oDescendant.GetAttribute("name"), "-");
                                    }
                                    else
                                    {
                                        cPageName = this.goServer.UrlEncode(oDescendant.GetAttribute("name"));
                                    }

                                    sUrl = sUrl + "/" + cPageName;

                                }
                            }
                        }

                        if (this.moConfig["TrailingSlash"] == "on")
                        {
                            sUrl = "/" + sUrl.Trim('/') + "/";
                        }

                        // Account for a root url
                        if (string.IsNullOrEmpty(sUrl))
                        {
                            sUrl = "/";
                        }

                        if (sUrl == "//")
                        {
                            sUrl = "/";
                        }

                        if (sUrl == "/")
                        {
                            sUrl = DomainURL;
                            if (this.moRequest.ServerVariables["SERVER_PORT"] != "80" & this.moRequest.ServerVariables["SERVER_PORT"] != "443")
                            {
                                sUrl = sUrl + ":" + this.moRequest.ServerVariables["SERVER_PORT"];
                            }
                        }
                        if (this.moConfig["LowerCaseUrl"] == "on")
                        {
                            sUrl = sUrl.ToLower();
                        }
                        // for admin mode we tag the pgid on the end to be safe for duplicate pagenames with different permissions.
                        if (this.mbAdminMode & string.IsNullOrEmpty(this.moConfig["pageExt"]) & this.moConfig["UsePageIdsForURLs"] != "on")


                            sUrl = sUrl + "?pgid=" + oMenuItem.GetAttribute("id");



                        if (this.moConfig["LowerCaseUrl"] == "on")
                        {
                            sUrl = sUrl.ToLower();
                        }

                        oMenuItem.SetAttribute("url", sUrl);

                        // If oMenuItem.GetAttribute("id") = "609" Then
                        // mbIgnorePath = mbIgnorePath
                        // If

                        if (!mbIgnorePath)
                        {
                            if (this.moRequest.QueryString.Count > 0)
                            {
                                if (this.moRequest["path"] != null)
                                {
                                    // If this matches the path requested then change the pageId
                                    if (!string.IsNullOrEmpty(sUrl))
                                    {
                                        string PathToMatch = Strings.Replace(sUrl, DomainURL, "").ToLower();
                                        string PathToMatch2 = "/" + gcLang + PathToMatch;
                                        string PathToTest = this.moRequest["path"].ToLower().TrimEnd('/');
                                        if ((PathToMatch ?? "") == (PathToTest ?? "") | (PathToMatch2 ?? "") == (PathToTest ?? ""))
                                        {
                                            if (oMenuItem.SelectSingleNode("ancestor-or-self::MenuItem[@id=" + nRootId + "]") != null)
                                            {
                                                // case for if newsletter has same page name as menu item
                                                if (this.Features.ContainsKey("PageVersions"))
                                                {
                                                    // catch for page version
                                                    if (oMenuItem.SelectSingleNode("PageVersion[@id='" + this.mnPageId + "']") is null)
                                                    {
                                                        this.mnPageId = Conversions.ToInteger(oMenuItem.GetAttribute("id"));
                                                    }
                                                }
                                                else
                                                {
                                                    this.mnPageId = Conversions.ToInteger(oMenuItem.GetAttribute("id"));
                                                }

                                                if (this.mnUserId != 0 | this.mbAdminMode != true)
                                                {
                                                    // case for personalisation and admin TS 14/02/2021
                                                    this.mnPageId = Conversions.ToInteger(oMenuItem.GetAttribute("id"));
                                                }
                                                // If oMenuItem.GetAttribute("verType") = "3" Then
                                                mnClonePageVersionId = this.mnPageId;
                                                // this is used in clone mode to determine the page content in GetPageContent.
                                                // End If
                                                oMenuItem.SetAttribute("requestedPage", "1");
                                            }

                                        }
                                    }
                                }
                            }
                        }
                        // set the URL for each language pageversion so we can link between

                        // Address the context of the page
                        if (gbClone)
                        {
                            cCloneParent = oMenuItem.GetAttribute("cloneparent");
                            if (Information.IsNumeric(cCloneParent) && Conversions.ToInteger(cCloneParent) > 0)
                            {
                                sUrl = Conversions.ToString(sUrl + Interaction.IIf(sUrl.Contains("?"), "&", "?"));
                                sUrl += "context=" + cCloneParent;
                            }
                        }

                        foreach (XmlElement pvElmt in oMenuItem.SelectNodes("PageVersion"))
                        {
                            string pageLang = pvElmt.GetAttribute("lang");
                            if (!string.IsNullOrEmpty(pageLang))
                            {
                                sUrl = "";
                                foreach (XmlElement oDescendant in oMenuItem.SelectNodes("ancestor-or-self::" + cMenuItemNodeName))
                                {
                                    if (!(oDescendant.ParentNode.Name == "Menu"))
                                    {
                                        cPageName = null;
                                        foreach (XmlElement parPvElmt in oDescendant.SelectNodes("PageVersion[@lang='" + pageLang + "']"))
                                        {
                                            if (this.moConfig["PageURLFormat"] == "hyphens")
                                            {
                                                cPageName = oRe.Replace(parPvElmt.GetAttribute("name"), "-");
                                            }
                                            else
                                            {
                                                cPageName = this.goServer.UrlEncode(parPvElmt.GetAttribute("name"));
                                            }
                                            // I know this means we get the last one but we should only have one anyway.
                                        }
                                        if (cPageName is null)
                                        {
                                            if (this.moConfig["PageURLFormat"] == "hyphens")
                                            {
                                                cPageName = oRe.Replace(oDescendant.GetAttribute("name"), "-");
                                            }
                                            else
                                            {
                                                cPageName = this.goServer.UrlEncode(oDescendant.GetAttribute("name"));
                                            }
                                        }
                                        sUrl = sUrl + "/" + cPageName;
                                        if (this.moConfig["LowerCaseUrl"] == "on")
                                        {
                                            sUrl = sUrl.ToLower();
                                        }
                                    }
                                }

                                string pvUrlPrefix = "";
                                // Check Language by Domain
                                if (this.goLangConfig != null)
                                {
                                    string httpStart;
                                    if (this.moRequest.ServerVariables["SERVER_PORT_SECURE"] == "1")
                                    {
                                        httpStart = "https://";
                                    }
                                    else
                                    {
                                        httpStart = "http://";
                                    }

                                    if (this.goLangConfig.SelectSingleNode("Language[@code='" + pageLang + "']") != null)
                                    {
                                        foreach (XmlElement oLangElmt in this.goLangConfig.SelectNodes("Language[@code='" + pageLang + "']"))
                                        {
                                            switch (Strings.LCase(oLangElmt.GetAttribute("identMethod")) ?? "")
                                            {
                                                case "domain":
                                                    {
                                                        pvUrlPrefix = httpStart + oLangElmt.GetAttribute("identifier");
                                                        break;
                                                    }
                                                case "path":
                                                    {
                                                        pvUrlPrefix = httpStart + this.goLangConfig.GetAttribute("defaultDomain") + "/" + oLangElmt.GetAttribute("identifier");
                                                        break;
                                                    }
                                            }
                                        }
                                    }

                                    if (string.IsNullOrEmpty(pvUrlPrefix))
                                        pvUrlPrefix = httpStart + this.goLangConfig.GetAttribute("defaultDomain");
                                    pvElmt.SetAttribute("url", pvUrlPrefix + sUrl);

                                }


                            }


                        }

                        if ((double)this.mnPageId == Conversions.ToDouble(oMenuItem.GetAttribute("id")))
                        {
                            mcPageURL = sUrl;
                        }
                    }
                }



                // GET THE ROOT NODE
                // ==================
                // get the Menu from the site root.
                this.PerfMon.Log("Web", "GetStructureXML-rootnode");
                if (nRootId > 0L)
                {
                    string cCloneModifier = "";
                    if (nCloneContextId > 0L)
                    {
                        cCloneModifier = " and @cloneparent='" + nCloneContextId + "'";
                    }

                    XmlElement oRoot = (XmlElement)oElmt.SelectSingleNode("descendant-or-self::" + cMenuItemNodeName + "[@id='" + nRootId + "'" + cCloneModifier + "]");
                    if (oRoot is null)
                    {
                    }
                    // Return error for when root page id does not exist.
                    // Dim cErrorMsg As String = "EonicWeb Config Error: The root page id does not exist, it may be hidden."
                    // cErrorMsg += " Root Id: " & nRootId & ";"
                    // cErrorMsg += " Top Level Id: " & RootPageId & ";"
                    // cErrorMsg += " User Id: " & nUserId & ";"
                    // cErrorMsg += " Clone Context Id: " & nCloneContextId & ";"
                    // cErrorMsg += " Menu Name: " & cMenuId & ";"
                    // Err.Raise(1001, "GetStructure", cErrorMsg)
                    else
                    {
                        oElmt.InnerXml = oRoot.OuterXml;
                    }
                }
                else
                {
                    oElmt.InnerXml = oElmt.FirstChild.FirstChild.OuterXml;
                }

                // MENU - ADD ID ATTRIBUTE
                // ========================
                if (!string.IsNullOrEmpty(cMenuId))
                {
                    oElmt.SetAttribute("id", cMenuId);
                }

                // MENU - ADD THE MENU TO THE PAGE XML
                // ===================================
                if (bAddMenuToPageXML && moPageXml.DocumentElement != null)
                {
                    sProcessInfo = "GetStructureXML-addMenuToPageXML";
                    this.PerfMon.Log("Web", sProcessInfo);
                    // Check if there's already a menu node.
                    if (moPageXml.SelectSingleNode("/Page/" + cRootNodeName) is null)
                    {
                        // No menu node - add it to the pagexml
                        moPageXml.DocumentElement.AppendChild(oElmt);
                    }
                    else
                    {
                        // Menu node found - add it after the last Menu node
                        moPageXml.DocumentElement.InsertAfter(oElmt, moPageXml.SelectSingleNode("/Page/" + cRootNodeName + "[position()=last()]"));
                    }
                }

                sProcessInfo = "GetStructureXML-End";
                this.PerfMon.Log("Web", sProcessInfo);

                return oElmt;
            }

            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, cFunctionDef, ex, sProcessInfo));
                return null;
            }

        }


        public virtual void AddContentCount(XmlElement oMenu, string SchemaType)
        {
            XmlElement oElmt;
            string cSQL;
            try
            {
                cSQL = "select COUNT(nContentKey) as count,nStructId from tblContentLocation cl" + " inner join tblContent c on c.nContentKey = cl.nContentId" + " inner join tblAudit a on c.nAuditId = a.nAuditKey" + " where c.cContentSchemaName = '" + Strings.Trim(SchemaType) + "' " + this.moConfig["MenuContentCountWhere"] + GetStandardFilterSQLForContent(true) + " group by nStructId";


                using (var oDR = this.moDbHelper.getDataReaderDisposable(cSQL))  // Done by nita on 6/7/22
                {
                    while (oDR.Read())
                    {
                        oElmt = (XmlElement)oMenu.SelectSingleNode(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("descendant::MenuItem[@id='", oDR["nStructId"]), "']")));
                        if (oElmt != null)
                        {
                            var newElmt = moPageXml.CreateElement("ContentCount");
                            newElmt.SetAttribute("type", Strings.Trim(SchemaType));
                            newElmt.SetAttribute("count", Conversions.ToString(oDR["count"]));
                            oElmt.AppendChild(newElmt);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "AddContentCount", ex, ""));
            }

        }

        public virtual void AddContentBrief(XmlElement oMenu, string SchemaType)
        {
            XmlElement oRoot;
            string parId;
            XmlElement ParentMenuElmt;
            try
            {

                oRoot = moPageXml.CreateElement("Contents");

                int argnCount = 0;
                GetMenuContentFromSelect("cContentSchemaName = '" + SchemaType + "'", ref argnCount, oContentsNode: ref oRoot, false, false, 0);

                foreach (XmlElement oElmt in oRoot.SelectNodes("*"))
                {
                    parId = oElmt.GetAttribute("locId");
                    ParentMenuElmt = (XmlElement)oMenu.SelectSingleNode("descendant-or-self::MenuItem[@id='" + parId + "']");
                    if (ParentMenuElmt != null)
                    {
                        ParentMenuElmt.AppendChild(oElmt);
                    }
                }
            }

            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "AddContentCount", ex, ""));
            }

        }

        /// <summary>
        /// Takes a menu node, and removes the denied nodes
        /// 
        /// A note on how permissions are inherited.
        /// If a page is OPEN, then it will inherit the permission.
        /// If a page has a parent permission of DENIED then it will be denied.
        /// </summary>
        /// <param name="oMenuItem"></param>
        /// <param name="cPerm"></param>
        /// <param name="cPermSource"></param>
        /// <param name="bPruneDenied"></param>
        /// <remarks></remarks>
        private void TidyMenunode(ref XmlElement oMenuItem, string cPerm, string cPermSource, bool bPruneDenied, long nUserId, string cMenuItemNodeName, string cRootNodeName)
        {
            string cCurrentPerm;
            string cCurrentPermSource;
            try
            {
                foreach (XmlElement oChild in oMenuItem.SelectNodes(cRootNodeName + " | " + cMenuItemNodeName))
                {
                    if (bPruneDenied && oChild.GetAttribute("access").Contains("DENIED"))
                    {
                        oMenuItem.RemoveChild(oChild);
                    }
                    else
                    {
                        // Work out the child permissions
                        cCurrentPerm = cPerm;
                        cCurrentPermSource = cPermSource;

                        if (cCurrentPerm.Contains("DENIED") | oChild.GetAttribute("access").Contains("OPEN") & !cCurrentPerm.Contains("OPEN"))


                        {

                            // If parent is DENIED,
                            // OR the child page is OPEN but the parent is not
                            // THEN set the child page permission to be the INHERITED parent perm and the source.

                            if (!cCurrentPerm.Contains("INHERITED"))
                                cCurrentPerm = "INHERITED " + cCurrentPerm;
                        }


                        else
                        {
                            // ELSE set perm and source variables to the current ones
                            cCurrentPerm = oChild.GetAttribute("access");

                            if ((oChild.GetAttribute("accessSourceId") ?? "") == (nUserId.ToString() ?? ""))
                            {
                                cCurrentPermSource = "";
                            }

                            else
                            {
                                if (cCurrentPerm == "DENIED")
                                    cCurrentPerm = "IMPLIED " + cCurrentPerm;
                                cCurrentPermSource = oChild.GetAttribute("accessSource");
                                if (!string.IsNullOrEmpty(cCurrentPermSource) & !cCurrentPerm.Contains("DENIED"))
                                    cCurrentPerm += " by " + cCurrentPermSource;
                            }

                        }

                        oChild.SetAttribute("access", cCurrentPerm);

                        oChild.RemoveAttribute("accessSource");
                        oChild.RemoveAttribute("accessSourceId");
                        XmlElement xmloChild = oChild;
                        TidyMenunode(ref xmloChild, cCurrentPerm, cCurrentPermSource, bPruneDenied, nUserId, cMenuItemNodeName, cRootNodeName);
                    }
                }
            }

            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "TidyMenunode", ex, ""));
            }
        }

        public void addPageDetailLinksToStructure(string cContentTypes)
        {
            string cProcessInfo = "addPageDetailLinksToStructure";
            try
            {
                XmlElement oMenuElmt = (XmlElement)moPageXml.DocumentElement.SelectSingleNode("Menu");
                if (oMenuElmt is null)
                    return;

                string[] cContentTypesArray = cContentTypes.Split(',');
                cContentTypes = "";
                foreach (string cContentType in cContentTypesArray)
                    cContentTypes += Tools.Database.SqlString(cContentType.Trim()) + ",";

                cContentTypes = cContentTypes.TrimEnd(',');
                var pageDict = new SortedDictionary<long, string>();
                foreach (XmlElement MenuItem in oMenuElmt.SelectNodes("descendant-or-self::MenuItem"))
                    pageDict.Add(Conversions.ToLong(MenuItem.GetAttribute("id")), MenuItem.GetAttribute("url"));
                // Dim keys As List(Of Long) = pageDict.KeyCollection
                // keys.Sort()

                // AG 19-Jan-2010 - Replaced with above code to add protection against SQL Injections
                // cContentTypes = cContentTypes.Replace(",", "','")
                // cContentTypes = "'" & cContentTypes & "'"
                // cContentTypes = cContentTypes.Replace("''", "'")


                //string sProcessInfo = "addPageDetailLinksToStructure";
                string cSQL = "SELECT tblContent.nContentKey, tblContent.cContentName, tblContentLocation.nStructId, tblAudit.dPublishDate, tblAudit.dUpdateDate, tblContent.cContentSchemaName" + " FROM tblContent INNER JOIN" + " tblAudit ON tblContent.nAuditId = tblAudit.nAuditKey INNER JOIN" + " tblContentLocation ON tblContent.nContentKey = tblContentLocation.nContentId" + " WHERE (tblContentLocation.bPrimary = 1) AND (tblAudit.nStatus = 1) AND (tblAudit.dPublishDate <= " + Tools.Database.SqlDate(mdDate) + " or tblAudit.dPublishDate is null) AND " + " (tblAudit.dExpireDate >= " + Tools.Database.SqlDate(mdDate) + " or tblAudit.dExpireDate is null) AND (tblContent.cContentSchemaName IN (" + cContentTypes + ")) ";


                using (var oDR = this.moDbHelper.getDataReaderDisposable(cSQL))  // Done by nita on 6/7/22
                {

                    var oRe = new Regex("[^A-Z0-9]", RegexOptions.IgnoreCase);

                    while (oDR.Read())
                    {
                        string cURL = "";
                        var oContElmt = moPageXml.CreateElement("MenuItem");

                        switch (this.moConfig["DetailPathType"] ?? "")
                        {
                            case "ContentType/ContentName":
                                {
                                    string[] prefixs = this.moConfig["DetailPrefix"].Split(',');
                                    string thisPrefix = "";
                                    string thisContentType = "";
                                    int i;
                                    var loopTo = prefixs.Length - 1;
                                    for (i = 0; i <= loopTo; i++)
                                    {
                                        thisPrefix = prefixs[i].Substring(0, prefixs[i].IndexOf("/"));
                                        thisContentType = prefixs[i].Substring(prefixs[i].IndexOf("/") + 1, prefixs[i].Length - prefixs[i].IndexOf("/") - 1);
                                        if ((thisContentType ?? "") == (oDR[5].ToString() ?? ""))
                                        {
                                            cURL = "/" + thisPrefix + "/" + oRe.Replace(oDR[1].ToString(), "-").Trim('-');
                                            if (this.moConfig["DetailPathTrailingSlash"] == "on")
                                            {
                                                cURL = cURL + "/";
                                            }
                                            if (this.moConfig["LowerCaseUrl"] == "on")
                                            {
                                                cURL = cURL.ToLower();
                                            }
                                        }
                                    }

                                    break;
                                }

                            default:
                                {
                                    if (pageDict.ContainsKey(Conversions.ToLong(oDR[2])))
                                    {
                                        cURL = pageDict[Conversions.ToLong(oDR[2])];
                                        // If moConfig("LegacyRedirect") = "on" Then
                                        cURL += "/" + oDR[0].ToString() + "-/" + Tools.Text.CleanName(oDR[1].ToString(), false, true);
                                    }
                                    // Else
                                    // cURL &= "/Item" & oDR(0).ToString
                                    // End If
                                    else
                                    {
                                        cProcessInfo = "orphan Content";
                                    }

                                    break;
                                }
                        }
                        if (this.moConfig["LowerCaseUrl"] == "on")
                        {
                            cURL = cURL.ToLower();
                        }
                        if (!string.IsNullOrEmpty(cURL))
                        {
                            oContElmt.SetAttribute("url", cURL);
                            oContElmt.SetAttribute("name", oDR[1].ToString());
                            oContElmt.SetAttribute("publish", Tools.Xml.XmlDate(oDR[3].ToString(), false));
                            oContElmt.SetAttribute("update", Tools.Xml.XmlDate(oDR[4].ToString(), false));
                            oMenuElmt.AppendChild(oContElmt);
                        }

                    }
                    oDR.Close();
                }
            }

            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "addPageDetailLinksToStructure", ex, gcEwSiteXsl, "", gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "addPageDetailLinksToStructure", ex, cProcessInfo));
            }
        }


        public virtual void AddContentXml(ref XmlElement oContentElmt)
        {
            this.PerfMon.Log("Web", "AddContentXml");
            XmlElement oContents;
            string sProcessInfo = "Add Content XML";

            try
            {
                if (oContentElmt is null)
                    return;
                oContents = (XmlElement)moPageXml.DocumentElement.SelectSingleNode("Contents");
                if (oContents is null)
                {
                    oContents = moPageXml.CreateElement("Contents");
                    moPageXml.DocumentElement.AppendChild(oContents);
                }

                oContents.AppendChild(oContentElmt);
            }

            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "AddContentXml", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "AddContentXml", ex, sProcessInfo));
            }

        }

        public void GetContentXml(ref XmlElement oPageElmt)
        {
            this.PerfMon.Log("Web", "GetContentXml");

            string sNodeName = string.Empty;
            string cXPathModifier = "";
            string sContent = string.Empty;
            string IsInTree = Conversions.ToString(false);
            string sProcessInfo = "building the Content XML";

            try
            {


                if (!mbSystemPage)
                {
                    // Clone page adjustment
                    if (gbClone)
                    {

                        if (mbIsClonePage)
                        {
                            // Is this a clone page that directly references its cloned page.
                            cXPathModifier = " and @clone='" + mnClonePageId.ToString() + "'";
                        }
                        else if (mnCloneContextPageId > 0)
                        {
                            // Is this a child of a clone page
                            cXPathModifier = " and @cloneparent='" + mnCloneContextPageId.ToString() + "'";
                        }

                        else if (mnClonePageVersionId > 0)
                        {

                            cXPathModifier = " and @requestedPage='1'";
                        }
                        // Page Version of a cloned page.

                        else
                        {
                            // this is not a cloned page, make sure we don't accidentally look for the cloned pages.

                            // this version works on clone language variations but wrong menu
                            cXPathModifier = " and (((not(@cloneparent) or @cloneparent=0) and (not(@clone) or @clone='' or @clone=0)) or (starts-with(@url,'" + mcPageLanguageUrlPrefix + this.mcPagePath + "') or starts-with(@url,'" + mcRequestDomain + mcPageLanguageUrlPrefix.Replace(mcRequestDomain, "") + this.mcPagePath + "')))";

                            // cXPathModifier = " and (((not(@cloneparent) or @cloneparent=0) and (not(@clone) or @clone='' or @clone=0)) and contains(@url,'" & mcPageLanguageUrlPrefix & mcPagePath & "'))"
                        }



                    }

                    // Check for blocked content


                    gcBlockContentType = this.moDbHelper.GetPageBlockedContent((long)this.mnPageId);
                    string parentXpath = "/Page/Menu/descendant-or-self::MenuItem[descendant-or-self::MenuItem[@id='" + this.mnPageId + "'" + cXPathModifier + "]]";

                    oPageElmt.SetAttribute("blockedContent", Regex.Replace(gcBlockContentType, @"^[|\d]+", ""));

                    // this is for load more steppers - we do not want any other content other than the one on the list
                    // the page url looks like
                    // /ourpage/?singleContentType=Product&startPos=10&rows=10

                    if (!string.IsNullOrEmpty(this.moRequest["singleContentType"]))
                    {
                        // sql for content on page and permissions etc
                        string sFilterSql = GetStandardFilterSQLForContent();
                        sFilterSql = sFilterSql + " and nstructid=" + this.mnPageId;
                        string cSort = "|ASC_cl.nDisplayOrder";
                        switch (this.moRequest["sortby"] ?? "")
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
                        // Paging variables
                        int nStart = 0;
                        int nRows = 500;

                        // Set the paging variables, if provided.
                        if (this.moRequest["startPos"] != null && Information.IsNumeric(this.moRequest["startPos"]))
                            nStart = Conversions.ToInteger(this.moRequest["startPos"]);
                        if (this.moRequest["rows"] != null && Information.IsNumeric(this.moRequest["rows"]))
                            nRows = Conversions.ToInteger(this.moRequest["rows"]);
                        // In admin mode want active and hidden products separatly
                        if (this.mbAdminMode)
                        {
                            if (this.moRequest["status"] != null && Information.IsNumeric(this.moRequest["status"]))
                            {
                                int nstatus = Conversions.ToInteger(this.moRequest["status"]);
                                if (nstatus == 0)
                                {
                                    sFilterSql = sFilterSql + " and nstructid=" + this.mnPageId + " and a.nStatus!=1";
                                    nStart = 0;
                                    nRows = Conversions.ToInteger(this.moRequest["TotalCount"]);  // getting all hidden products in list
                                }
                                else
                                {
                                    sFilterSql = sFilterSql + " and nstructid=" + this.mnPageId + " and a.nStatus=" + nstatus;
                                }
                            }
                        }
                        else
                        {
                            sFilterSql = sFilterSql + " and nstructid=" + this.mnPageId;
                        }
                        if (this.moSession["FilterWhereCondition"] != null && Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(this.moSession["FilterWhereCondition"], string.Empty, false)))
                        {
                            string whereSQL = Conversions.ToString(this.moSession["FilterWhereCondition"]);
                            XmlElement argoPageDetail = null;
                            int nCount = 0;
                            this.GetPageContentFromSelectFilterPagination(ref nCount, oContentsNode: ref oPageElmt, oPageDetail: ref argoPageDetail, whereSQL, cShowSpecificContentTypes: this.moRequest["singleContentType"], ignoreActiveAndDate: false, nStartPos: (long)nStart, nItemCount: (long)nRows);
                        }
                        else
                        {
                            var argoPageElmt = moPageXml.DocumentElement;
                            XmlElement argoPageDetail1 = null;
                            XmlElement argoContentModule = null;
                            this.GetContentXMLByTypeAndOffset(ref argoPageElmt, this.moRequest["singleContentType"] + cSort, (long)nStart, (long)nRows, oPageDetail: ref argoPageDetail1, oContentModule: ref argoContentModule, sFilterSql);

                        }
                    }

                    else
                    {

                        // Set nothing to Filter Pagination session
                        if (this.moSession != null)
                        {
                            if (this.moSession["FilterWhereCondition"] != null)
                            {
                                this.moSession["FilterWhereCondition"] = (object)null;
                                // moSession.Remove("FilterWhereCondition")
                            }
                        }


                        // step through the tree from home to our current page
                        foreach (XmlElement oElmt in oPageElmt.SelectNodes(parentXpath))
                        {
                            oElmt.SetAttribute("active", "1");
                            long nPageId = Conversions.ToLong(oElmt.GetAttribute("id"));
                            GetPageContentXml(nPageId);
                            nPageId = default;
                            IsInTree = Conversions.ToString(true);
                        }

                        if (this.mbPreview & Conversions.ToBoolean(IsInTree) == false)
                        {
                            GetPageContentXml((long)this.mnPageId);
                        }

                        if (this.Features.ContainsKey("PageVersions"))
                        {
                            if (Conversions.ToBoolean(IsInTree) == false & this.mbAdminMode == true)
                            {
                                GetPageContentXml((long)this.mnPageId);
                            }
                        }
                        if ((long)this.mnPageId == gnPageNotFoundId)
                        {
                            GetPageContentXml((long)this.mnPageId);
                        }

                        // get the first records in the case of a load more stepper.
                        if (!string.IsNullOrEmpty(gcBlockContentType))
                        {

                            string[] cContentTypes = Strings.Split(gcBlockContentType, ",");
                            int i;
                            var loopTo = cContentTypes.Length - 1;
                            for (i = 0; i <= loopTo; i++)
                            {
                                string[] cContentType = Strings.Split(cContentTypes[i], "|");

                                string SingleContentType = cContentType[0];
                                string ModuleId = cContentType[1];
                                XmlElement ContentModule = (XmlElement)moPageXml.SelectSingleNode("/Page/Contents/Content[@id='" + ModuleId + "']");
                                if (ContentModule != null)
                                {
                                    string cSort = "|ASC_cl.nDisplayOrder";
                                    switch (ContentModule.GetAttribute("sortBy") ?? "")
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
                                    // Paging variables
                                    int nStart = 0;
                                    int nRows = 500;
                                    nRows = Conversions.ToInteger("0" + ContentModule.GetAttribute("stepCount"));

                                    string sFilterSql = GetStandardFilterSQLForContent();
                                    sFilterSql = sFilterSql + " and nstructid=" + this.mnPageId;
                                    if (ContentModule.HasAttribute("TotalCount") == false)
                                    {
                                        ContentModule.SetAttribute("TotalCount", 0.ToString());
                                    }
                                    var argoPageElmt1 = moPageXml.DocumentElement;
                                    XmlElement oPagedetail = null;
                                    this.GetContentXMLByTypeAndOffset(ref argoPageElmt1, SingleContentType + cSort, (long)nStart, (long)nRows, ref oPagedetail, oContentModule: ref ContentModule, sFilterSql, bShowContentDetails: false);

                                }

                            }
                        }

                    }
                }


                else
                {
                    // if we are on a system page we only want the content on that page not parents.
                    GetPageContentXml((long)this.mnPageId);
                    oPageElmt.SetAttribute("systempage", "true");

                }

                // Check content versions
                // If gbVersionControl Then CheckContentVersions()
                // moved to GetPageXml because content may be added from module overloads or grabbers etc.


                // Always ensure we have a content node
                XmlElement oRoot = (XmlElement)moPageXml.DocumentElement.SelectSingleNode("Contents");
                if (oRoot is null)
                {
                    oRoot = moPageXml.CreateElement("Contents");
                    moPageXml.DocumentElement.AppendChild(oRoot);
                }
            }



            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "getContentXml", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetContentXml", ex, sProcessInfo));
            }

        }

        public virtual void GetPageContentXml(long nPageId)
        {
            this.PerfMon.Log("Web", "GetPageContentXml");
            string sProcessInfo = "Getting the content from page " + nPageId;
            string sSql = "";
            string sFilterSql = "";
            string sWhereSql = string.Empty;
            string sSql2 = string.Empty;
            XmlElement oRoot;
            try
            {

                // Create the live filter
                sFilterSql = GetStandardFilterSQLForContent();

                oRoot = (XmlElement)moPageXml.DocumentElement.SelectSingleNode("Contents");
                if (oRoot is null)
                {
                    oRoot = moPageXml.CreateElement("Contents");
                    moPageXml.DocumentElement.AppendChild(oRoot);
                }

                string nCurrentPageId = nPageId.ToString();

                // Adjust the page id if it's a cloned page.
                if (Conversions.ToDouble(nCurrentPageId) != (double)this.mnPageId)
                {
                    // we are only pulling in cascaded items
                    sFilterSql += " and CL.bCascade = 1 and CL.bPrimary = 1 ";
                }
                else
                {
                    // we are pulling in located and native items but not cascaded
                }

                // Check if the page is a cloned page
                // If it is, then we need to switch the page id.
                if (gbClone)
                {
                    int nClonePage = this.moDbHelper.getClonePageID((int)nPageId);
                    if (nClonePage > 0)
                        nPageId = nClonePage;
                }

                // sSql = "select c.nContentKey as id, (select TOP 1 CL2.nStructId from tblContentLocation CL2 where CL2.nContentId=c.nContentKey and CL2.bPrimary = 1) as parId, cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire from tblContent c inner join tblContentLocation CL on c.nContentKey = CL.nContentId inner join tblAudit a on c.nAuditId = a.nAuditKey" & _

                if (!string.IsNullOrEmpty(gcBlockContentType))
                {

                    string gcBlockContentTypeRemovedIds = Regex.Replace(gcBlockContentType, @"[|\d]+", "");

                    sFilterSql = sFilterSql + " and c.cContentSchemaName NOT IN ('" + gcBlockContentTypeRemovedIds.Replace(",", "','") + "') ";
                }
                string cContentLimit = "";
                if (!string.IsNullOrEmpty(this.moConfig["ContentLimit"]) & Information.IsNumeric(this.moConfig["ContentLimit"]))
                {
                    cContentLimit = " TOP " + this.moConfig["ContentLimit"] + " ";
                }

                sSql = "select " + cContentLimit + "c.nContentKey as id, dbo.fxn_getContentParents(c.nContentKey) as parId ,cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, CL.cPosition as position from tblContent c" + " inner join tblContentLocation CL on c.nContentKey = CL.nContentId" + " inner join tblAudit a on c.nAuditId = a.nAuditKey" + " where( CL.nStructId = " + nPageId;

                sSql = sSql + sFilterSql + ") order by type, cl.nDisplayOrder";

                var oDs = new DataSet();
                oDs = this.moDbHelper.GetDataSet(sSql, "Content", "Contents");
                this.PerfMon.Log("Web", "AddDataSetToContent - For Page ", sSql);
                this.moDbHelper.AddDataSetToContent(ref oDs, ref oRoot, ref mdPageExpireDate, ref mdPageUpdateDate, Conversions.ToLong(nCurrentPageId), false, "");
            }

            // If gbCart Or gbQuote Then
            // moDiscount.getAvailableDiscounts(oRoot)
            // End If


            // AddGroupsToContent(oRoot)



            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "getPageContentXml", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetPagecontentXml", ex, sProcessInfo));
            }

        }

        public void AddGroupsToContent(ref XmlElement oContentElmt)
        {
            // Adds ProductGroups to content nodes
            this.PerfMon.Log("Web", "AddGroupsToContent-start");
            try
            {

                // TS This is hammering performance on sites with lots of discount rules and doesn't seem to be used anywhere ?
                // if this is requred we need only pull back categories for (products or other content names sold) that are on the current page seriously reducing the time required.

                // build In statement

                XmlElement oContElmt;
                string InStatement = " tblCartCatProductRelations.nContentId IN(";
                foreach (XmlElement currentOContElmt in oContentElmt.SelectNodes("Content"))
                {
                    oContElmt = currentOContElmt;
                    InStatement = InStatement + oContElmt.GetAttribute("id") + ",";
                }
                InStatement = InStatement.Trim(',') + ")";

                string cSQL = "SELECT tblCartProductCategories.nCatKey AS id, tblCartProductCategories.cCatSchemaName As type, tblCartProductCategories.nCatParentId As parent, " + " tblCartProductCategories.cCatName As name, tblCartProductCategories.cCatDescription As description, tblCartCatProductRelations.nContentId" + " FROM tblCartProductCategories INNER JOIN" + " tblAudit On tblCartProductCategories.nAuditId = tblAudit.nAuditKey INNER JOIN" + " tblCartCatProductRelations On tblCartProductCategories.nCatKey = tblCartCatProductRelations.nCatId";
                if (!this.mbAdminMode)
                {
                    cSQL += " WHERE tblAudit.nStatus = 1 " + " And (tblAudit.dPublishDate Is null Or tblAudit.dPublishDate = 0 Or tblAudit.dPublishDate <= " + Tools.Database.SqlDate(DateTime.Now) + " )" + " And (tblAudit.dExpireDate Is null Or tblAudit.dExpireDate = 0 Or tblAudit.dExpireDate >= " + Tools.Database.SqlDate(DateTime.Now) + " )" + " And " + InStatement;
                }
                else
                {
                    cSQL += " where " + InStatement;
                }
                var oDS = new DataSet();
                oDS = this.moDbHelper.GetDataSet(cSQL, "ContentGroup", "ContentGroups");

                this.PerfMon.Log("Web", "AddGroupsToContent-startloop-" + oDS.Tables["ContentGroup"].Rows.Count);

                foreach (DataRow oDR in oDS.Tables["ContentGroup"].Rows)
                {

                    foreach (XmlElement currentOContElmt1 in oContentElmt.SelectNodes(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("Content[@id='", oDR["nContentId"]), "']"))))
                    {
                        oContElmt = currentOContElmt1;
                        var oGroupElmt = oContentElmt.OwnerDocument.CreateElement("ContentGroup");
                        oGroupElmt.SetAttribute("id", Conversions.ToString(Operators.ConcatenateObject(oDR["id"], "")));
                        oGroupElmt.SetAttribute("type", Conversions.ToString(Operators.ConcatenateObject(oDR["type"], "")));
                        oGroupElmt.SetAttribute("name", Conversions.ToString(Operators.ConcatenateObject(oDR["name"], "")));
                        oGroupElmt.SetAttribute("parent", Conversions.ToString(Operators.ConcatenateObject(oDR["parent"], "")));
                        oGroupElmt.InnerText = Conversions.ToString(Operators.ConcatenateObject(oDR["description"], ""));
                        oContElmt.AppendChild(oGroupElmt);
                    }
                }
                this.PerfMon.Log("Web", "AddGroupsToContent-end");
            }

            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "AddGroupsToConent", ex, gcEwSiteXsl, "", gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "AddGroupsToContent", ex, ""));
            }
        }



        public string GetContentUrl(long nContentId)
        {
            this.PerfMon.Log("Web", "GetContentUrl");
            string sSql;
            string sFilterSql;
            string ContentName = "";
            string[] ParentPages;
            var PrimaryPageId = default(long);
            string ContentURL = "/" + nContentId.ToString() + "-/";
            string sProcessInfo = "Getting path for content id " + nContentId;
            try
            {
                sFilterSql = GetStandardFilterSQLForContent();

                sSql = "select c.nContentKey as id, dbo.fxn_getContentParents(c.nContentKey) as parId ,cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, CL.cPosition as position from tblContent c" + " left outer join tblContentLocation CL on c.nContentKey = CL.nContentId and CL.bPrimary = 1" + " inner join tblAudit a on c.nAuditId = a.nAuditKey" + " where(c.nContentKey = " + nContentId + sFilterSql + ") order by type, cl.nDisplayOrder";

                var oDs = new DataSet();
                oDs = this.moDbHelper.GetDataSet(sSql, "Content", "Contents");

                if (oDs.Tables.Count > 0 && oDs.Tables[0].Rows.Count > 0)
                {

                    foreach (DataRow oRow in oDs.Tables[0].Rows)
                    {

                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oRow["type"], "SKU", false)))
                        {
                            // get the id of the parent product

                            sSql = "select c.nContentKey as id, dbo.fxn_getContentParents(c.nContentKey) as parId ,cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, CL.cPosition as position from tblContent c" + " inner join tblContentLocation CL on c.nContentKey = CL.nContentId" + " inner join tblAudit a on c.nAuditId = a.nAuditKey" + " inner join tblContentRelation cr on c.nContentKey = cr.nContentParentId" + " where( cr.nContentChildId = " + nContentId;
                            sSql = sSql + sFilterSql + " and CL.bPrimary = 1) order by type, cl.nDisplayOrder";
                            var oDs2 = new DataSet();
                            oDs2 = this.moDbHelper.GetDataSet(sSql, "Content", "Contents");
                            foreach (DataRow oRow2 in oDs2.Tables[0].Rows)
                            {
                                ContentName = Conversions.ToString(oRow2["name"]);
                                if (oRow2["parId"].ToString().Contains(","))
                                {
                                    ParentPages = oRow2["parId"].ToString().Split(',');
                                    if (Conversions.ToDouble(ParentPages[0]) > 0d)
                                    {
                                        PrimaryPageId = Conversions.ToLong(ParentPages[0]);
                                    }
                                }
                                else if (Information.IsNumeric(oRow2["parId"]))
                                {
                                    PrimaryPageId = Conversions.ToLong(oRow2["parId"]);
                                }
                                ContentURL = "/" + Conversions.ToString(oRow2["id"]) + "-/";
                            }
                        }

                        else
                        {

                            ContentName = Conversions.ToString(oRow["name"]);
                            if (oRow["parId"].ToString().Contains(","))
                            {
                                ParentPages = oRow["parId"].ToString().Split(',');
                                if (Conversions.ToDouble(ParentPages[0]) > 0d)
                                {
                                    PrimaryPageId = Conversions.ToLong(ParentPages[0]);
                                }
                            }
                            else if (Information.IsNumeric(oRow["parId"]))
                            {
                                PrimaryPageId = Conversions.ToLong(oRow["parId"]);
                            }
                        }

                    }
                }
                if (PrimaryPageId > 0L)
                {
                    XmlElement pageMenuElmt = (XmlElement)moPageXml.SelectSingleNode("/Page/Menu/descendant-or-self::MenuItem[@id='" + PrimaryPageId + "']");
                    if (pageMenuElmt != null)
                    {
                        ContentURL = pageMenuElmt.GetAttribute("url") + ContentURL;
                    }
                }
                if (!string.IsNullOrEmpty(ContentName))
                {
                    ContentName = ContentName.Replace(" ", "-");
                    ContentName = this.goServer.UrlEncode(ContentName);
                    ContentURL = ContentURL + ContentName;
                }


                if (this.moConfig["LowerCaseUrl"] == "on")
                {
                    ContentURL = ContentURL.ToLower();
                }
                return ContentURL;
            }

            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "getContentXml", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetContentXml", ex, sProcessInfo));
                return "";
            }

        }
        /// <summary>
        /// The purpose of this function is to check if the user should be seeing a Pending version of the content
        /// as opposed to the live one.  If the user is an admin or has AddUpdate permissions then this should be the case
        /// </summary>
        /// <remarks></remarks>
        protected void CheckContentVersions()
        {
            this.PerfMon.Log("Web", "CheckContentVersions");

            string cProcessInfo = "";

            DataSet oDs;
            string cSql = "";
            string cOwnerFilter = "";

            XmlNodeList childNodes = null;
            XmlElement oReplacement;

            try
            {

                // We need to check two things
                // - for content that hasn't been retrieved for this page, but is Pending (this will be in the content table)
                // - for content that is on this page, but has a version on the Versions table called Pending.


                var nPagePermission = this.moDbHelper.getPagePermissionLevel((long)this.mnPageId);

                if (Cms.dbHelper.CanAddUpdate(nPagePermission) | this.mbAdminMode)
                {

                    var oTempNode = moPageXml.CreateElement("temp");

                    // Now check if it's everything we're getting, or just what the user created.
                    if (!this.mbAdminMode & nPagePermission == Cms.dbHelper.PermissionLevel.AddUpdateOwn | nPagePermission == Cms.dbHelper.PermissionLevel.AddUpdateOwnPublish)
                    {
                        // Put a filter in to limit checks to content we've added
                        cOwnerFilter = " and @owner=" + this.mnUserId;
                    }

                    string filter = "[@id[number(.)=number(.)]" + cOwnerFilter + "]";

                    string cCheckContentList = "";

                    foreach (XmlElement oContent in moPageXml.SelectNodes("//Content" + filter))
                    {

                        // Add the id if it is not null or zero
                        if (!string.IsNullOrEmpty(oContent.GetAttribute("id")) && Information.IsNumeric(oContent.GetAttribute("id")) && Conversions.ToLong(oContent.GetAttribute("id")) > 0L)

                        {
                            if (!string.IsNullOrEmpty(cCheckContentList))
                                cCheckContentList += ",";
                            cCheckContentList += oContent.GetAttribute("id");
                        }

                    }

                    if (!string.IsNullOrEmpty(cCheckContentList))
                    {
                        // Get any content on the page that has a Status Pending Version
                        cSql = "select c.nContentPrimaryId as id, " + "   dbo.fxn_getContentParents(c.nContentPrimaryId) as parId, " + "   cContentForiegnRef as ref, cContentName as name, " + "   cContentSchemaName as type, " + "   cContentXmlBrief as content, " + "   a.nStatus as status, " + "   a.dpublishDate as publish, " + "   a.dExpireDate as expire, " + "   a.dUpdateDate as [update], " + "   c.nVersion as version, " + "   c.nContentVersionKey as versionid, " + "   a.nInsertDirId as owner " + " from tblContentVersions c " + "   inner join tblAudit a " + "   on c.nAuditId = a.nAuditKey " + " WHERE c.nContentPrimaryId IN (" + cCheckContentList + ")" + "   AND a.nStatus = " + ((int)Cms.dbHelper.Status.Pending).ToString() + " AND c.nVersion = (Select TOP 1 nVersion from tblContentVersions c2 where c.nContentPrimaryId = c2.nContentPrimaryId AND a.nStatus = " + ((int)Cms.dbHelper.Status.Pending).ToString() + " ORDER BY nVersion DESC)";

                        oDs = this.moDbHelper.GetDataSet(cSql, "Content", "Contents");

                        if (oDs.Tables.Count > 0 && oDs.Tables[0].Rows.Count > 0)
                        {
                            // If contentversion exists, replace the current node with this.

                            oDs.Tables[0].Columns["id"].ColumnMapping = MappingType.Attribute;

                            if (oDs.Tables[0].Columns.Contains("parID"))
                            {
                                oDs.Tables[0].Columns["parId"].ColumnMapping = MappingType.Attribute;
                            }

                            // Added to handle the new relationship type
                            if (oDs.Tables[0].Columns.Contains("rtype"))
                            {
                                oDs.Tables[0].Columns["rtype"].ColumnMapping = MappingType.Attribute;
                            }

                            {
                                var withBlock = oDs.Tables[0];
                                withBlock.Columns["ref"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["name"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["type"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["status"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["publish"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["expire"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["owner"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["update"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["version"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["versionid"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["content"].ColumnMapping = MappingType.SimpleContent;
                            }

                            oDs.EnforceConstraints = false;
                            // convert to Xml Dom
                            var oXml = new XmlDataDocument(oDs);
                            oXml.PreserveWhitespace = false;

                            foreach (XmlElement oReplaceContent in oXml.SelectNodes("/Contents/Content"))
                            {

                                XmlElement oContent = (XmlElement)moPageXml.SelectSingleNode("//Content[@id='" + oReplaceContent.GetAttribute("id") + "']");

                                // Before we replace the node, move any child Content nodes (i.e. related content) out from it.
                                childNodes = oContent.SelectNodes("Content");
                                if (childNodes.Count > 0)
                                {
                                    foreach (XmlElement child in childNodes)
                                    {
                                        oContent.RemoveChild(child);
                                        oTempNode.AppendChild(child);
                                    }
                                }

                                // Replace the contentNode
                                oReplacement = (XmlElement)moPageXml.ImportNode(oReplaceContent, true);
                                DateTime argdExpireDate = DateTime.Parse("0001-01-01");
                                DateTime argdUpdateDate = DateTime.Parse("0001-01-01");
                                oReplacement = this.moDbHelper.SimpleTidyContentNode(ref oReplacement, dExpireDate: ref argdExpireDate, dUpdateDate: ref argdUpdateDate);
                                oContent.ParentNode.ReplaceChild(oReplacement, oContent);

                                // Move the related content back
                                if (childNodes.Count > 0)
                                {
                                    foreach (XmlElement child in childNodes)
                                    {
                                        oTempNode.RemoveChild(child);
                                        oReplacement.AppendChild(child);
                                    }
                                }

                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "getContentXml", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "CheckContentVersions", ex, cProcessInfo));
            }

        }

        protected void CheckContentVersions_Replaced()
        {
            this.PerfMon.Log("Web", "CheckContentVersions");

            string cProcessInfo = "";

            DataSet oDs;
            string cSql = "";
            string cOwnerFilter = "";

            XmlNodeList childNodes = null;
            XmlElement oReplacement;

            try
            {

                // We need to check two things
                // - for content that hasn't been retrieved for this page, but is Pending (this will be in the content table)
                // - for content that is on this page, but has a version on the Versions table called Pending.


                var nPagePermission = this.moDbHelper.getPagePermissionLevel((long)this.mnPageId);

                if (Cms.dbHelper.CanAddUpdate(nPagePermission) | this.mbAdminMode)
                {

                    var oTempNode = moPageXml.CreateElement("temp");

                    // Now check if it's everything we're getting, or just what the user created.
                    if (!this.mbAdminMode & nPagePermission == Cms.dbHelper.PermissionLevel.AddUpdateOwn | nPagePermission == Cms.dbHelper.PermissionLevel.AddUpdateOwnPublish)
                    {

                        // Put a filter in to limit checks to content we've added
                        cOwnerFilter = " and @owner=" + this.mnUserId;

                    }

                    string filter = "[@id[number(.)=number(.)]" + cOwnerFilter + "]";



                    // only search content that has an id which is numeric
                    foreach (XmlElement oContent in moPageXml.SelectNodes("//Content" + filter))
                    {

                        // Search for pending versions of the content

                        cSql = "select TOP 1 c.nContentPrimaryId as id, " + "   dbo.fxn_getContentParents(c.nContentPrimaryId) as parId, " + "   cContentForiegnRef as ref, cContentName as name, " + "   cContentSchemaName as type, " + "   cContentXmlBrief as content, " + "   a.nStatus as status, " + "   a.dpublishDate as publish, " + "   a.dExpireDate as expire, " + "   a.dUpdateDate as [update], " + "   c.nVersion as version, " + "   c.nContentVersionKey as versionid, " + "   a.nInsertDirId as owner " + " from tblContentVersions c " + "   inner join tblAudit a " + "     on c.nAuditId = a.nAuditKey " + " WHERE c.nContentPrimaryId = " + oContent.GetAttribute("id") + "   AND a.nStatus = " + ((int)Cms.dbHelper.Status.Pending).ToString() + " ORDER BY c.nVersion DESC ";

                        oDs = this.moDbHelper.GetDataSet(cSql, "Content", "Contents");

                        if (oDs.Tables.Count > 0 && oDs.Tables[0].Rows.Count > 0)
                        {
                            // If contentversion exists, replace the current node with this.


                            oDs.Tables[0].Columns["id"].ColumnMapping = MappingType.Attribute;

                            if (oDs.Tables[0].Columns.Contains("parID"))
                            {
                                oDs.Tables[0].Columns["parId"].ColumnMapping = MappingType.Attribute;
                            }

                            // Added to handle the new relationship type
                            if (oDs.Tables[0].Columns.Contains("rtype"))
                            {
                                oDs.Tables[0].Columns["rtype"].ColumnMapping = MappingType.Attribute;
                            }

                            {
                                var withBlock = oDs.Tables[0];
                                withBlock.Columns["ref"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["name"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["type"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["status"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["publish"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["expire"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["owner"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["update"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["version"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["versionid"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["content"].ColumnMapping = MappingType.SimpleContent;
                            }

                            oDs.EnforceConstraints = false;
                            // convert to Xml Dom
                            var oXml = new XmlDataDocument(oDs);
                            oXml.PreserveWhitespace = false;


                            // Before we replace the node, move any child Content nodes (i.e. related content) out from it.
                            childNodes = oContent.SelectNodes("Content");
                            if (childNodes.Count > 0)
                            {
                                foreach (XmlElement child in childNodes)
                                {
                                    oContent.RemoveChild(child);
                                    oTempNode.AppendChild(child);
                                }
                            }

                            // Replace the contentNode
                            oReplacement = (XmlElement)moPageXml.ImportNode(oXml.SelectSingleNode("/Contents/Content"), true);
                            DateTime argdExpireDate = DateTime.Parse("0001-01-01");
                            DateTime argdUpdateDate = DateTime.Parse("0001-01-01");
                            oReplacement = this.moDbHelper.SimpleTidyContentNode(ref oReplacement, dExpireDate: ref argdExpireDate, dUpdateDate: ref argdUpdateDate);
                            oContent.ParentNode.ReplaceChild(oReplacement, oContent);

                            // Move the related content back
                            if (childNodes.Count > 0)
                            {
                                foreach (XmlElement child in childNodes)
                                {
                                    oTempNode.RemoveChild(child);
                                    oReplacement.AppendChild(child);
                                }
                            }
                        }


                    }


                }
            }


            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "getContentXml", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "CheckContentVersions", ex, cProcessInfo));
            }

        }


        public void GetErrorXml(ref XmlElement oPageElmt)
        {
            this.PerfMon.Log("Web", "GetErrorXml");
            XmlElement oRoot;
            XmlElement oElmt;
            string sFilterSql = string.Empty;
            string sSql2 = string.Empty;
            string sNodeName = string.Empty;
            string sContent = string.Empty;
            string sErrorModule = "";
            string sErrorHTML = string.Empty;
            string strMessageHtml = "";
            string strMessageText = "";

            string sProcessInfo = "building the Content XML";

            try
            {

                oRoot = (XmlElement)moPageXml.DocumentElement.SelectSingleNode("Contents");
                if (oRoot is null)
                {
                    oRoot = moPageXml.CreateElement("Contents");
                    moPageXml.DocumentElement.AppendChild(oRoot);
                }

                oPageElmt.SetAttribute("systempage", "true");

                oElmt = moPageXml.CreateElement("Content");
                oElmt.SetAttribute("type", "Error");
                oElmt.SetAttribute("name", mnProteanCMSError.ToString());
                switch (mnProteanCMSError)
                {
                    case 1005L:
                        {
                            strMessageText = "Page Not Found";
                            strMessageHtml = "<div><h2>Page Not Found</h2>" + "</div>";
                            gnResponseCode = 404L;
                            sErrorModule = "BuildPageXml";
                            bPageCache = false;
                            break;
                        }
                    case 1006L:
                        {
                            strMessageText = "Access Denied";
                            strMessageHtml = "<div><h2>Access Denied</h2>" + "</div>";
                            // gnResponseCode = 401
                            sErrorModule = "BuildPageXml";

                            bPageCache = false;
                            break;
                        }
                    case 1007L:
                        {
                            strMessageText = "File Not Found";
                            strMessageHtml = "<div><h2>File Not Found</h2>" + "</div>";
                            // gnResponseCode = 404
                            sErrorModule = "Get Document";

                            bPageCache = false;
                            break;
                        }

                    case 1008L:
                        {
                            strMessageText = "Invalid Licence";
                            strMessageHtml = "<div><h2>Please get a valid ProteanCMS Licence</h2>" + "</div>";
                            sErrorModule = "BuildPageXml";

                            bPageCache = false;
                            break;
                        }
                }

                if (gbDebug)
                {
                    try
                    {
                        Information.Err().Raise((int)mnProteanCMSError, sErrorModule, strMessageText);
                    }
                    catch (Exception)
                    {
                        oPageElmt.SetAttribute("layout", "Error");
                        oElmt.InnerXml = strMessageHtml;
                    }
                }
                else
                {
                    oPageElmt.SetAttribute("layout", "Error");
                    oElmt.InnerXml = strMessageHtml;
                }

                oRoot.AppendChild(oElmt);
                oPageElmt.AppendChild(oRoot);
            }



            catch (Exception ex)
            {

                // returnException(msException, mcModuleName, "GetErrorXml", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetErrorXml", ex, sProcessInfo));
            }

        }

        public void GetContentXMLByType(ref XmlElement oPageElmt, string cContentType, string sqlFilter = "", string fullSQL = "")
        {
            this.PerfMon.Log("Web", "GetContentXMLByType");
            // <add key="ControlPanelTypes" value="Event,Document|Top_10|DESC_Publish"/>
            try
            {
                string[] oTypeCriteria = Strings.Split(cContentType, "|");
                string cTop = "";
                string cOrderDirection = "";
                string oOrderField = "";
                string strContentType = "";

                int i;
                var loopTo = Information.UBound(oTypeCriteria);
                for (i = 0; i <= loopTo; i++)
                {
                    if (oTypeCriteria[i].Contains("Top_"))
                    {
                        cTop = Strings.Split(oTypeCriteria[i], "_")[1];
                        if (!Information.IsNumeric(cTop))
                            cTop = "";
                    }
                    else if (oTypeCriteria[i].Contains("ASC_"))
                    {
                        cOrderDirection = "";
                        oOrderField = Strings.Split(oTypeCriteria[i], "_")[1];
                    }
                    else if (oTypeCriteria[i].Contains("DESC_"))
                    {
                        cOrderDirection = "DESC";
                        oOrderField = Strings.Split(oTypeCriteria[i], "_")[1];
                    }
                    else
                    {
                        // its the field name
                        strContentType = oTypeCriteria[i];
                    }
                }

                string cSQL = "select c.nContentKey as id, dbo.fxn_getContentParents(c.nContentKey) as parId, cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, CL.cPosition as position from tblContent c left outer join tblContentLocation CL on c.nContentKey = CL.nContentId inner join tblAudit a on c.nAuditId = a.nAuditKey" + " where (cContentSchemaName = '" + strContentType + "') ";
                if (!string.IsNullOrEmpty(sqlFilter))
                {
                    cSQL += sqlFilter;
                }
                cSQL += GetStandardFilterSQLForContent();

                if (!string.IsNullOrEmpty(oOrderField))
                {
                    cSQL += " ORDER BY " + oOrderField + " " + cOrderDirection;
                }

                if (!string.IsNullOrEmpty(fullSQL))
                {
                    cSQL = fullSQL;
                }

                var oDS = this.moDbHelper.GetDataSet(cSQL, "Content1", "Contents");
                var oDT = new DataTable();
                oDT = oDS.Tables["Content1"].Copy();
                oDT.Rows.Clear();
                oDT.TableName = "Content";
                oDS.Tables.Add(oDT);
                int nMax = 0;
                string cDoneIds = ",";
                string ochkStr = "";
                if (Information.IsNumeric(cTop))
                    nMax = Conversions.ToInteger(cTop);
                foreach (DataRow oDR in oDS.Tables["Content1"].Rows)
                {
                    if (oDS.Tables["Content"].Rows.Count < nMax | nMax == 0)
                    {
                        if (Conversions.ToBoolean(Operators.AndObject(Information.IsNumeric(oDR["parId"]), !oDR["parId"].ToString().Contains(","))))
                        {
                            ochkStr = this.moDbHelper.checkPagePermission(Conversions.ToLong(oDR["parId"])).ToString();
                            if (Information.IsNumeric(ochkStr))
                            {
                                if (Conversions.ToBoolean(Operators.AndObject(Operators.ConditionalCompareObjectEqual(Conversions.ToInteger(ochkStr), oDR["parId"], false), !cDoneIds.Contains(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(",", oDR["id"]), ","))))))
                                {
                                    oDS.Tables["Content"].ImportRow(oDR);
                                    cDoneIds = Conversions.ToString(cDoneIds + Operators.ConcatenateObject(oDR["id"], ","));
                                }
                            }
                        }
                        else if (this.mbAdminMode) // if in adminmode get everything regardless
                        {
                            oDS.Tables["Content"].ImportRow(oDR);
                            cDoneIds = Conversions.ToString(cDoneIds + Operators.ConcatenateObject(oDR["id"], ","));
                        }

                    }
                }
                oDS.Tables.Remove(oDS.Tables["Content1"]);
                XmlElement oRoot;
                oRoot = (XmlElement)moPageXml.DocumentElement.SelectSingleNode("Contents");
                if (oRoot is null)
                {
                    oRoot = moPageXml.CreateElement("Contents");
                    moPageXml.DocumentElement.AppendChild(oRoot);
                }
                this.moDbHelper.AddDataSetToContent(ref oDS, ref oRoot, ref mdPageExpireDate, ref mdPageUpdateDate, (long)this.mnPageId, false, "");
            }

            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "getContentXml", ex, gcEwSiteXsl, "", gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetContentXMLByType", ex, ""));
            }
        }

        public void GetContentXMLByTypeAndOffset(ref XmlElement oPageElmt, string cContentType, long nStartPos, long nItemCount, ref XmlElement oPageDetail, ref XmlElement oContentModule, string sqlFilter = "", string fullSQL = "", bool bShowContentDetails = true)
        {
            this.PerfMon.Log("Web", "GetContentXMLByTypeAndOffset");
            // <add key="ControlPanelTypes" value="Event,Document|Top_10|DESC_Publish"/>
            try
            {
                string[] oTypeCriteria = Strings.Split(cContentType, "|");
                string cTop = "";
                string cOrderDirection = "";
                string oOrderField = "";
                string strContentType = "";

                int i;
                var loopTo = Information.UBound(oTypeCriteria);
                for (i = 0; i <= loopTo; i++)
                {
                    if (oTypeCriteria[i].Contains("Top_"))
                    {
                        cTop = Strings.Split(oTypeCriteria[i], "_")[1];
                        if (!Information.IsNumeric(cTop))
                            cTop = "";
                    }
                    else if (oTypeCriteria[i].Contains("ASC_"))
                    {
                        cOrderDirection = "";
                        oOrderField = Strings.Split(oTypeCriteria[i], "_")[1];
                    }
                    else if (oTypeCriteria[i].Contains("DESC_"))
                    {
                        cOrderDirection = "DESC";
                        oOrderField = Strings.Split(oTypeCriteria[i], "_")[1];
                    }
                    else
                    {
                        // its the field name
                        strContentType = oTypeCriteria[i];
                    }
                }


                if (nStartPos < 0L)
                    nStartPos = 0L;
                if (nItemCount < 1L)
                    nItemCount = 1000L;


                // Quick call to get the total number of records
                string cSQL = "select count(*) from tblContent c left outer join tblContentLocation CL on c.nContentKey = CL.nContentId inner join tblAudit a on c.nAuditId = a.nAuditKey" + " where (cContentSchemaName = '" + strContentType + "') ";
                if (!string.IsNullOrEmpty(sqlFilter))
                {
                    cSQL += sqlFilter;
                }
                long nTotal = Conversions.ToLong(this.moDbHelper.GetDataValue(cSQL, CommandType.Text, null, (object)0));

                if (nTotal > 0L)
                {
                    cSQL = "select c.nContentKey as id, dbo.fxn_getContentParents(c.nContentKey) as parId, cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, CL.cPosition as position from tblContent c left outer join tblContentLocation CL on c.nContentKey = CL.nContentId inner join tblAudit a on c.nAuditId = a.nAuditKey" + " where (cContentSchemaName = '" + strContentType + "') ";

                    if (!string.IsNullOrEmpty(sqlFilter))
                    {
                        cSQL += sqlFilter;
                    }
                    cSQL += GetStandardFilterSQLForContent();

                    if (!string.IsNullOrEmpty(oOrderField))
                    {
                        cSQL += " ORDER BY " + oOrderField + " " + cOrderDirection;
                    }

                    if (!string.IsNullOrEmpty(fullSQL))
                    {
                        cSQL = fullSQL;
                    }

                    cSQL += " offset " + nStartPos + " rows fetch next " + nItemCount + " rows only";

                    var oDS = this.moDbHelper.GetDataSet(cSQL, "Content1", "Contents");
                    var oDT = new DataTable();
                    oDT = oDS.Tables["Content1"].Copy();
                    oDT.Rows.Clear();
                    oDT.TableName = "Content";
                    oDS.Tables.Add(oDT);
                    int nMax = 0;
                    string cDoneIds = ",";
                    string ochkStr = "";
                    if (Information.IsNumeric(cTop))
                        nMax = Conversions.ToInteger(cTop);
                    foreach (DataRow oDR in oDS.Tables["Content1"].Rows)
                    {
                        if (oDS.Tables["Content"].Rows.Count < nMax | nMax == 0)
                        {
                            if (Conversions.ToBoolean(Operators.AndObject(Information.IsNumeric(oDR["parId"]), !oDR["parId"].ToString().Contains(","))))
                            {
                                ochkStr = this.moDbHelper.checkPagePermission(Conversions.ToLong(oDR["parId"])).ToString();
                                if (Information.IsNumeric(ochkStr))
                                {
                                    if (Conversions.ToBoolean(Operators.AndObject(Operators.ConditionalCompareObjectEqual(Conversions.ToInteger(ochkStr), oDR["parId"], false), !cDoneIds.Contains(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(",", oDR["id"]), ","))))))
                                    {
                                        oDS.Tables["Content"].ImportRow(oDR);
                                        cDoneIds = Conversions.ToString(cDoneIds + Operators.ConcatenateObject(oDR["id"], ","));
                                    }
                                }
                            }
                            else if (this.mbAdminMode) // if in adminmode get everything regardless
                            {
                                oDS.Tables["Content"].ImportRow(oDR);
                                cDoneIds = Conversions.ToString(cDoneIds + Operators.ConcatenateObject(oDR["id"], ","));
                            }

                        }
                    }
                    oDS.Tables.Remove(oDS.Tables["Content1"]);
                    XmlElement oRoot;
                    oRoot = (XmlElement)moPageXml.DocumentElement.SelectSingleNode("Contents");
                    if (oRoot is null)
                    {
                        oRoot = moPageXml.CreateElement("Contents");
                        moPageXml.DocumentElement.AppendChild(oRoot);
                    }
                    this.moDbHelper.AddDataSetToContent(ref oDS, ref oRoot, ref mdPageExpireDate, ref mdPageUpdateDate, (long)this.mnPageId, false, "");
                    if (bShowContentDetails)
                    {
                        // Get the content Detail element
                        XmlElement oContentDetails;
                        if (oPageDetail is null)
                        {
                            oContentDetails = (XmlElement)moPageXml.SelectSingleNode("Page/ContentDetail");
                            if (oContentDetails is null)
                            {
                                oContentDetails = moPageXml.CreateElement("ContentDetail");
                                if (!string.IsNullOrEmpty(moPageXml.InnerXml))
                                {
                                    moPageXml.FirstChild.AppendChild(oContentDetails);
                                }
                                else
                                {
                                    oPageDetail.AppendChild(oContentDetails);
                                }

                            }
                        }
                        else
                        {
                            oContentDetails = oPageDetail;
                        }

                        oContentDetails.SetAttribute("start", nStartPos.ToString());
                        oContentDetails.SetAttribute("total", nTotal.ToString());
                        oContentDetails.SetAttribute("rows", nItemCount.ToString());
                    }

                    if (oContentModule != null)
                    {
                        if (oContentModule.HasAttribute("TotalCount"))
                        {
                            oContentModule.SetAttribute("TotalCount", nTotal.ToString());
                        }
                    }

                }
            }

            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "getContentXml", ex, gcEwSiteXsl, "", gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetContentXMLByTypeAndOffset", ex, ""));
            }
        }

        public XmlElement GetContentDetailXml(XmlElement oPageElmt = null, long nArtId = 0L, bool disableRedirect = false, bool bCheckAccessToContentLocation = false, long nVersionId = 0L, bool bIgnoreContentStatus = false)
        {
            this.PerfMon.Log("Web", "GetContentDetailXml");
            XmlElement oRoot;
            XmlElement oElmt;
            XmlElement retElmt = null;
            string sContent;
            string sSql;
            string sProcessInfo = "GetContentDetailXml";
            var oDs = new DataSet();
            string sFilterSql = "";
            bool bLoadAsXml;
            XmlComment oComment;
            if (nArtId > 0L)
            {
                this.mnArtId = (int)nArtId;
            }
            try
            {
                if (moContentDetail is null)
                {

                    // If requested, we need to make sure that the content we are looking for doesn't belong to a page
                    // that the user is not allowed to access.
                    // We can review the current menu structure xml instead of calling the super slow permissions functions.



                    if (bCheckAccessToContentLocation & this.mnArtId > 0)
                    {
                        if (!this.moDbHelper.checkContentLocationsInCurrentMenu((long)this.mnArtId, true))
                        {
                            this.mnArtId = 0;
                        }
                    }
                    if (this.mnArtId > 0)
                    {
                        sProcessInfo = "loading content" + this.mnArtId;

                        // I don't like this but need a quick solution
                        if (moPageXml.SelectSingleNode("Page/Contents/Content[@action='Protean.Cms+Content+Modules.ListHistoricEvents']") != null)
                        {
                            bAllowExpired = true;
                        }


                        sFilterSql += GetStandardFilterSQLForContent();

                        oRoot = moPageXml.CreateElement("ContentDetail");

                        // check if new function exists in DB, this logic can be later deprecated when all db are inline.
                        bool bContLoc = this.moDbHelper.checkDBObjectExists("fxn_getContentLocations", Tools.Database.objectTypes.UserFunction);
                        if (bContLoc)
                        {
                            sSql = "select c.nContentKey as id, cContentForiegnRef as ref, dbo.fxn_getContentParents(c.nContentKey) as parId, dbo.fxn_getContentLocations(c.nContentKey) as locations, cContentName as name, cContentSchemaName as type, cContentXmlDetail as content, a.dpublishDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, a.nStatus as status from tblContent c ";
                        }
                        else
                        {
                            sSql = "select c.nContentKey as id, cContentForiegnRef as ref, dbo.fxn_getContentParents(c.nContentKey) as parId, cContentName as name, cContentSchemaName as type, cContentXmlDetail as content, a.dpublishDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, a.nStatus as status from tblContent c ";
                        }
                        sSql += "inner join tblAudit a on c.nAuditId = a.nAuditKey  ";
                        // sSql &= "inner join tblContentLocation CL on c.nContentKey = CL.nContentId "

                        if (bIgnoreContentStatus)
                        {
                            sSql += "where c.nContentKey = " + this.mnArtId;
                        }
                        else
                        {
                            sSql += "where c.nContentKey = " + this.mnArtId + sFilterSql + " ";
                        }

                        // sSql &= "and CL.nStructId = " & mnPageId

                        if (nVersionId > 0L)
                        {
                            sSql = "select c.nContentPrimaryId as id, nContentVersionKey as verid, nVersion as verno, cContentForiegnRef as ref, dbo.fxn_getContentParents(c.nContentPrimaryId) as parId, dbo.fxn_getContentLocations(c.nContentPrimaryId) as locations, cContentName as name, cContentSchemaName as type, cContentXmlDetail as content, a.dpublishDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, a.nStatus as status from tblContentVersions c ";
                            sSql += "inner join tblAudit a on c.nAuditId = a.nAuditKey  ";
                            sSql += "where c.nContentPrimaryId = " + this.mnArtId + " and nContentVersionKey=" + nVersionId + " ";
                        }
                        oDs = this.moDbHelper.GetDataSet(sSql, "Content", "ContentDetail");
                        oDs.Tables[0].Columns["id"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["ref"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["name"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["type"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["publish"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["expire"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["update"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["parId"].ColumnMapping = MappingType.Attribute;
                        if (nVersionId > 0L)
                        {
                            oDs.Tables[0].Columns["verid"].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[0].Columns["verno"].ColumnMapping = MappingType.Attribute;
                        }
                        if (bContLoc)
                        {
                            oDs.Tables[0].Columns["locations"].ColumnMapping = MappingType.Attribute;
                        }
                        oDs.Tables[0].Columns["owner"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["status"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["content"].ColumnMapping = MappingType.SimpleContent;

                        // Need to check the content is found on the current page.


                        if (oDs.Tables[0].Rows.Count > 0)
                        {
                            oRoot.InnerXml = Strings.Replace(oDs.GetXml(), "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "");
                            foreach (XmlNode oNode in oRoot.SelectNodes("/ContentDetail/Content"))
                            {
                                oElmt = (XmlElement)oNode;
                                sContent = oElmt.InnerText;

                                if (Information.IsDate(oElmt.GetAttribute("update")))
                                    mdPageUpdateDate = Conversions.ToDate(oElmt.GetAttribute("update"));


                                // Try to convert the InnerText to InnerXml
                                // Also if the innerxml has Content as a first node, then get the innerxml of the content node.
                                try
                                {
                                    oElmt.InnerXml = sContent;
                                    bLoadAsXml = true;
                                }

                                catch (Exception)
                                {
                                    // If the load failed, then flag it in the Content node and return the InnerText as a Comment
                                    oComment = oRoot.OwnerDocument.CreateComment(oElmt.InnerText);
                                    oElmt.SetAttribute("xmlerror", "getContentBriefXml");
                                    oElmt.InnerXml = "";
                                    oElmt.AppendChild(oComment);
                                    oComment = null;
                                    bLoadAsXml = false;
                                }
                                if (bLoadAsXml)
                                {

                                    // Successfully converted to XML.
                                    // Now check if the node imported is a Content node - if so get rid of the Content node
                                    var oFirst = Tools.Xml.firstElement(ref oElmt);
                                    // NB 19-02-2010 Added to stop unsupported types falling over
                                    if (oFirst != null)
                                    {
                                        if (oFirst.LocalName == "Content")
                                        {
                                            foreach (XmlAttribute oAttr in oElmt.SelectNodes("Content/@*"))
                                            {
                                                if (string.IsNullOrEmpty(oElmt.GetAttribute(oAttr.Name)))
                                                {
                                                    oElmt.SetAttribute(oAttr.Name, oAttr.InnerText);
                                                }
                                            }
                                            oElmt.InnerXml = oFirst.InnerXml;
                                        }
                                    }

                                    XmlElement argoContentElmt = (XmlElement)oNode;
                                    this.moDbHelper.addRelatedContent(ref argoContentElmt, this.mnArtId, this.mbAdminMode);
                                    //oNode = argoContentElmt;
                                    if (!string.IsNullOrEmpty(this.moConfig["ShowOwnerOnDetail"]))
                                    {
                                        string cContentType = oElmt.GetAttribute("type");
                                        if (this.moConfig["ShowOwnerOnDetail"].Contains(cContentType))
                                        {
                                            long nOwner = Conversions.ToLong("0" + oElmt.GetAttribute("owner"));
                                            if (nOwner > 0L)
                                            {
                                                oElmt.AppendChild(GetUserXML(nOwner));
                                            }
                                        }
                                    }
                                }
                            }

                            // If gbCart Or gbQuote Then
                            // moDiscount.getAvailableDiscounts(oRoot)
                            // End If

                            XmlElement contentElmt = (XmlElement)oRoot.SelectSingleNode("/ContentDetail/Content");
                            if (nVersionId > 0L)
                            {
                                contentElmt.SetAttribute("previewKey", Tools.Encryption.RC4.Encrypt(nVersionId.ToString(), this.moConfig["SharedKey"]));
                            }
                            XmlElement argoContentElmt1 = (XmlElement)oRoot.SelectSingleNode("/ContentDetail");
                            AddGroupsToContent(ref argoContentElmt1);
                            if (oPageElmt != null)
                            {
                                var oContentDetail = contentElmt;
                                if (oContentDetail != null && !string.IsNullOrEmpty(oContentDetail.InnerXml.Trim()))
                                {
                                    // If we can find a content detail Content node, 
                                    // AND it contains some InnerXml, then YAY.
                                    oPageElmt.AppendChild(oRoot.FirstChild);
                                }
                                else
                                {

                                    // OTHERWISE if there is nothing in the detail we get the brief instead.
                                    GetContentBriefXml(oPageElmt, nArtId);
                                }
                            }
                            retElmt = (XmlElement)oRoot.FirstChild;

                            if (this.mbAdminMode == false & Strings.LCase(this.moConfig["RedirectToDescriptiveContentURLs"]) == "true")
                            {

                                string SafeURLName = Tools.Text.CleanName(contentElmt.GetAttribute("name"), false, true);
                                string myOrigURL;
                                string myQueryString = "";
                                if (mcOriginalURL.Contains("?"))
                                {
                                    myOrigURL = mcOriginalURL.Substring(0, mcOriginalURL.IndexOf("?"));
                                    myQueryString = mcOriginalURL.Substring(mcOriginalURL.LastIndexOf("?"));
                                }
                                else
                                {
                                    myOrigURL = mcOriginalURL;
                                }

                                if ((myOrigURL ?? "") != (mcPageURL + "/" + this.mnArtId + "-/" + SafeURLName ?? ""))
                                {
                                    // we redirect perminently
                                    mbRedirectPerm = Conversions.ToString(true);
                                    msRedirectOnEnd = mcPageURL + "/" + this.mnArtId + "-/" + SafeURLName + myQueryString;
                                }

                            }
                            moContentDetail = (XmlElement)oRoot.FirstChild;

                            // Add single item shipping costs for JSON-LD
                            string ProductTypes = this.moConfig["ProductTypes"];
                            if (string.IsNullOrEmpty(ProductTypes))
                                ProductTypes = "Product,SKU";
                            if (ProductTypes.Contains(contentElmt.GetAttribute("type")) & moCart != null)
                            {
                                try
                                {
                                    var oShippingElmt = moPageXml.CreateElement("ShippingCosts");
                                    string cDestinationCountry = moCart.moCartConfig["DefaultDeliveryCountry"];
                                    double nPrice = 0d;
                                    if (contentElmt.SelectSingleNode("Prices/Price[@type='sale']") != null)
                                    {
                                        nPrice = Conversions.ToDouble("0" + contentElmt.SelectSingleNode("Prices/Price[@type='sale']").InnerText);
                                    }

                                    if (nPrice == 0d)
                                    {
                                        if (contentElmt.SelectSingleNode("Prices/Price[@type='rrp']") != null)
                                        {
                                            nPrice = Conversions.ToDouble("0" + contentElmt.SelectSingleNode("Prices/Price[@type='rrp']").InnerText);
                                        }
                                    }
                                    double nWeight = 0d;
                                    if (contentElmt.SelectSingleNode("ShippingWeight") != null)
                                    {
                                        nWeight = Conversions.ToDouble("0" + contentElmt.SelectSingleNode("ShippingWeight").InnerText);
                                    }
                                    // Dim nWeight As Double = CDbl("0" & contentElmt.SelectSingleNode("ShippingWeight").InnerText)
                                    var dsShippingOption = moCart.getValidShippingOptionsDS(cDestinationCountry, nPrice, 1L, nWeight, this.mnArtId);
                                    if (dsShippingOption != null)
                                    {
                                        oShippingElmt.InnerXml = Strings.Replace(dsShippingOption.GetXml(), "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "");
                                    }
                                    contentElmt.AppendChild(oShippingElmt);
                                }
                                catch (Exception)
                                {

                                }
                            }

                            return moContentDetail;
                        }
                        else
                        {
                            sProcessInfo = "no content to add - we redirect";
                            // this content is not found either page not found or re-direct home.
                            if (!disableRedirect)
                            {
                                // put this in to prevent a redirect if we are calling this from somewhere strange.
                                if (gnPageNotFoundId > 1L)
                                {
                                    // msRedirectOnEnd = "/System+Pages/Page+Not+Found"
                                    this.mnPageId = (int)gnPageNotFoundId;
                                    this.mnArtId = 0;
                                    moPageXml = new XmlDocument();
                                    BuildPageXML();
                                    this.moResponse.StatusCode = 404;
                                }
                                else
                                {
                                    msRedirectOnEnd = this.moConfig["BaseUrl"];
                                    this.moResponse.StatusCode = 404;
                                }
                                // End If
                                // Return Nothing
                            }
                            // Else
                            // Just a page no detail requested
                            return null;
                        }
                    }
                    else
                    {
                        // Just a page no detail requested
                        return null;
                    }
                }

                else
                {
                    sProcessInfo = "content exists adding content";
                    oRoot = moContentDetail.OwnerDocument.CreateElement("ContentDetail");
                    oRoot.AppendChild(moContentDetail);
                    if (oPageElmt != null)
                    {
                        oPageElmt.AppendChild(oRoot);
                    }
                    AddGroupsToContent(ref oRoot);
                    retElmt = moContentDetail;
                    this.moDbHelper.CommitLogToDB(Cms.dbHelper.ActivityType.ContentDetailViewed, this.mnUserId, SessionID, DateTime.Now, this.mnArtId, 0, "");
                    return moContentDetail;
                }
            }
            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "getContentDetailXml", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetContentDetailXml", ex, sProcessInfo));
                return null;
            }
            //sSql = null;
        }


        public XmlElement GetContentBriefXml(XmlElement oPageElmt = null, long nArtId = 0L)
        {
            this.PerfMon.Log("Web", "GetContentBriefXml");
            XmlElement oRoot;
            XmlElement oElmt;
            XmlElement retElmt = null;
            string sContent;
            string sSql;
            string sProcessInfo = "GetContentBriefXml";
            var oDs = new DataSet();
            string sFilterSql = "";
            bool bLoadAsXml;
            XmlComment oComment;
            if (nArtId > 0L)
            {
                this.mnArtId = (int)nArtId;
            }
            try
            {
                if (moContentDetail is null)
                {
                    if (this.mnArtId > 0)
                    {
                        sProcessInfo = "loading content" + this.mnArtId;
                        sFilterSql += GetStandardFilterSQLForContent();
                        oRoot = moPageXml.CreateElement("ContentDetail");
                        sSql = "select c.nContentKey as id, cContentForiegnRef as ref, dbo.fxn_getContentParents(c.nContentKey) as parId, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.dpublishDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, a.nStatus as status " + "from tblContent c inner join tblAudit a on c.nAuditId = a.nAuditKey  where c.nContentKey = " + this.mnArtId + sFilterSql;
                        oDs = this.moDbHelper.GetDataSet(sSql, "Content", "ContentDetail");
                        oDs.Tables[0].Columns["id"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["ref"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["name"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["type"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["publish"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["expire"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["update"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["parId"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["owner"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["status"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["content"].ColumnMapping = MappingType.SimpleContent;

                        oRoot.InnerXml = Strings.Replace(oDs.GetXml(), "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "");
                        foreach (XmlNode oNode in oRoot.SelectNodes("/ContentDetail/Content"))
                        {
                            oElmt = (XmlElement)oNode;
                            sContent = oElmt.InnerText;
                            if (Information.IsDate(oElmt.GetAttribute("update")))
                                mdPageUpdateDate = Conversions.ToDate(oElmt.GetAttribute("update"));

                            // Try to convert the InnerText to InnerXml
                            // Also if the innerxml has Content as a first node, then get the innerxml of the content node.
                            try
                            {
                                oElmt.InnerXml = sContent;
                                bLoadAsXml = true;
                            }

                            catch (Exception)
                            {
                                // If the load failed, then flag it in the Content node and return the InnerText as a Comment
                                oComment = oRoot.OwnerDocument.CreateComment(oElmt.InnerText);
                                oElmt.SetAttribute("xmlerror", "getContentBriefXml");
                                oElmt.InnerXml = "";
                                oElmt.AppendChild(oComment);
                                oComment = null;
                                bLoadAsXml = false;
                            }

                            if (bLoadAsXml)
                            {

                                // Successfully converted to XML.
                                // Now check if the node imported is a Content node - if so get rid of the Content node
                                var oFirst = Tools.Xml.firstElement(ref oElmt);
                                if (oFirst.LocalName == "Content")
                                {
                                    oElmt.InnerXml = oFirst.InnerXml;
                                }

                                XmlElement argoContentElmt = (XmlElement)oNode;
                                this.moDbHelper.addRelatedContent(ref argoContentElmt, this.mnArtId, this.mbAdminMode);
                                //oNode = argoContentElmt;

                            }

                        }

                        // If gbCart Or gbQuote Then
                        // moDiscount.getAvailableDiscounts(oRoot)
                        // End If

                        if (oPageElmt != null)
                        {
                            oPageElmt.AppendChild(oRoot.FirstChild);
                        }
                        // AddGroupsToContent(oRoot)
                        retElmt = (XmlElement)oRoot.FirstChild;
                        return (XmlElement)oRoot.FirstChild;
                    }
                    else
                    {
                        sProcessInfo = "no content to add";
                    }
                }
                else
                {
                    sProcessInfo = "content exists adding content";
                    oRoot = moPageXml.CreateElement("ContentDetail");
                    oRoot.AppendChild(moContentDetail);
                    if (oPageElmt != null)
                    {
                        oPageElmt.AppendChild(oRoot);
                    }
                    // AddGroupsToContent(oRoot)
                    retElmt = moContentDetail;
                    this.moDbHelper.CommitLogToDB(Cms.dbHelper.ActivityType.ContentDetailViewed, this.mnUserId, SessionID, DateTime.Now, this.mnArtId, 0, "");
                    return moContentDetail;
                }

                return retElmt;
            }
            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "getContentDetailXml", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetContentBriefXml", ex, sProcessInfo));
                return null;
            }

        }

        /// <summary>
        /// This attempts to construct the standard SQL filter for getting LIVE content
        /// If version control is on it will also assess the page permissions, 
        /// and if appropriate, it will get content that is not LIVe but, say, PENDING
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public string GetStandardFilterSQLForContent(bool bPrecedingAND = true)
        {

            this.PerfMon.Log("Web", "GetStandardFilterSQLForContent");

            string sFilterSQL = "";

            try
            {

                // Only check for permissions if not in Admin Mode
                if (!this.mbAdminMode)
                {

                    // Set the default filter
                    if (!(this.mbPreviewHidden == true))
                    {
                        sFilterSQL = "a.nStatus = 1 ";
                    }

                    if (gbVersionControl && this.mnUserId > 0)
                    {
                        // Version control is on
                        // Check the page permission
                        if (Cms.dbHelper.CanAddUpdate(mnUserPagePermission))
                        {

                            // User has update permissions - now can they only have control over their own items
                            if (Cms.dbHelper.CanOnlyUseOwn(mnUserPagePermission))
                            {

                                // Return everything with a status of live and anything that was created by
                                // the user and has a status that isn't hidden
                                sFilterSQL = "(a.nStatus = 1 OR (a.nStatus >= 1 AND a.nInsertDirId=" + this.mnUserId.ToString() + ")) ";
                            }

                            else
                            {

                                // Return anything with a status that isn't hidden
                                sFilterSQL = "a.nStatus >= 1 ";

                            }

                        }
                    }

                    if (bPrecedingAND & !string.IsNullOrEmpty(sFilterSQL))
                    {
                        sFilterSQL = " AND " + sFilterSQL;
                    }


                    string ExpireLogic = ">= ";
                    if (Strings.LCase(this.moConfig["ExpireAtEndOfDay"]) == "on")
                    {
                        ExpireLogic = "> ";
                    }

                    sFilterSQL += " and (a.dPublishDate is null or a.dPublishDate = 0 or a.dPublishDate <= " + Tools.Database.SqlDate(mdDate) + " )";

                    if (!bAllowExpired)
                    {
                        sFilterSQL += " and (a.dExpireDate is null or a.dExpireDate = 0 or a.dExpireDate " + ExpireLogic + Tools.Database.SqlDate(mdDate) + " )";
                    }

                    // sFilterSQL &= " and (a.dPublishDate is null or a.dPublishDate <= " & Protean.Tools.Database.SqlDate(mdDate) & " )"
                    // sFilterSQL &= " and (a.dExpireDate is null or a.dExpireDate " & ExpireLogic & Protean.Tools.Database.SqlDate(mdDate) & " )"
                }
                this.PerfMon.Log("Web", "GetStandardFilterSQLForContent-END");
                return sFilterSQL;
            }

            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetStandardFilterSQLForContent", ex, sFilterSQL));
                return "";
            }
        }


        public void AddBreadCrumb(string cPath, string cDisplayName)
        {
            this.PerfMon.Log("Web", "AddBreadCrumb");
            XmlNode oNode;
            XmlElement oElmt;
            XmlElement oElmt2;

            string sProcessInfo = Conversions.ToString(string.IsNullOrEmpty("adding Link to Breadcrumb"));

            try
            {
                oNode = moPageXml.DocumentElement.SelectSingleNode("Breadcrumb");
                if (oNode is null)
                {
                    oElmt = moPageXml.CreateElement("Breadcrumb");
                    moPageXml.DocumentElement.AppendChild(oElmt);
                }
                else
                {
                    oElmt = (XmlElement)oNode;
                }
                oElmt2 = moPageXml.CreateElement("Link");
                oElmt2.SetAttribute("name", cDisplayName);
                oElmt2.SetAttribute("url", cPath);
                oElmt.AppendChild(oElmt2);
            }

            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "AddBreadCrumb", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "AddBreadCrumb", ex, sProcessInfo));
            }
        }

        public void returnDocumentFromItem(ref System.Web.HttpContext ctx)
        {
            this.PerfMon.Log("Web", "returnDocumentFromItem");
            var oXML = new XmlDocument();
            string sSql = "";
            string strFilePath = "";
            string strFileName = "";
            string objStream = string.Empty;
            string strFileSize, strFileType, FileExt;
            // Dim strPageInfo, strReferrer
            // Dim sWriter = New IO.StringWriter
            var oDS = new DataSet();
            string sProcessInfo = "returnDocumentFromItem";
            string sPath = string.Empty;
            var oErrorXml = new XmlDocument();
            long nDocId;
            string downloadFileName = "download.zip";
            try
            {
                this.moResponse.Buffer = true;
                this.moResponse.Expires = 0;
                this.goServer.ScriptTimeout = 10000;


                if (Regex.IsMatch(this.moRequest["docId"] + "", "^[0-9,]*$") & this.moRequest["docId"] != null)
                {

                    string[] aDocId = Strings.Split(this.moRequest["docId"], ",");
                    if (Information.UBound(aDocId) != 0)
                    {


                        // Firstly, work out if any / all of the requested files IDs exist
                        // If some of the documents exist, zip them up, and report the missing files to the site admin
                        // If none of the files exist, then report top site admin and return an error.

                        var documentPaths = new ArrayList();
                        var documentsNotFound = new ArrayList();

                        foreach (string docId in aDocId)
                        {
                            if (Information.IsNumeric(docId) && !string.IsNullOrEmpty(docId))
                            {
                                sSql += Tools.Database.SqlString(docId) + ",";
                            }
                        }
                        sSql = sSql.TrimEnd(',');

                        // Get the file paths from the parent content
                        if (!string.IsNullOrEmpty(sSql))
                        {
                            sSql = "select * from tblContent where nContentKey in (" + sSql + ")";
                            oDS = this.moDbHelper.GetDataSet(sSql, "Item");
                            foreach (DataRow dsRow in oDS.Tables["Item"].Rows)
                            {
                                strFilePath = this.moDbHelper.getContentFilePath(dsRow, this.moRequest["xPath"]);
                                if (string.IsNullOrEmpty(strFilePath))
                                {
                                    documentsNotFound.Add("Content does not contain filepath");
                                }
                                else if (File.Exists(strFilePath))
                                {
                                    documentPaths.Add(strFilePath);
                                }
                                else
                                {
                                    documentsNotFound.Add(strFilePath);

                                }
                            }
                            oDS = null;
                        }

                        // Send the zipped up files
                        if (documentPaths.Count > 0)
                        {

                            if (!string.IsNullOrEmpty(this.moRequest["filename"]))
                                downloadFileName = this.moRequest["filename"];

                            var oZip = new ICSharpCode.SharpZipLib.Zip.ZipOutputStream(ctx.Response.OutputStream);
                            oZip.SetLevel(3);
                            ctx.Response.AddHeader("Connection", "keep-alive");
                            ctx.Response.AddHeader("Content-Disposition", "filename=" + downloadFileName);

                            ctx.Response.Charset = "UTF-8";
                            ctx.Response.ContentType = Tools.FileHelper.GetMIMEType("zip");


                            foreach (string documentPath in documentPaths)
                            {

                                var fsHelper = new Protean.fsHelper();
                                var oFileStream = fsHelper.GetFileStream(documentPath);
                                strFileName = Tools.Text.filenameFromPath(documentPath);

                                var oZipEntry = new ICSharpCode.SharpZipLib.Zip.ZipEntry(strFileName);
                                oZipEntry.DateTime = DateTime.Now;
                                oZipEntry.Size = oFileStream.Length;
                                // oZipEntry.Offset = -1

                                var abyBuffer = new byte[(int)(oFileStream.Length - 1L) + 1];
                                oFileStream.Read(abyBuffer, 0, abyBuffer.Length);

                                var objCrc32 = new ICSharpCode.SharpZipLib.Checksum.Crc32();
                                objCrc32.Reset();
                                objCrc32.Update(abyBuffer);
                                oZipEntry.Crc = objCrc32.Value;

                                oZip.PutNextEntry(oZipEntry);
                                oZip.Write(abyBuffer, 0, abyBuffer.Length);
                            }

                            oZip.Close();
                            ctx.Response.Flush();
                            ctx.Response.End();

                        }


                        // Tidy up - send errors if needs be
                        if (documentsNotFound.Count > 0 | documentPaths.Count == 0)
                        {


                            string filesNotFound = "";
                            foreach (string documentNotFound in documentsNotFound)
                                filesNotFound += documentNotFound + ",";
                            filesNotFound = filesNotFound.TrimEnd(',');

                            // Report the unfound documents
                            if (documentsNotFound.Count > 0)
                            {
                                oErrorXml.LoadXml("<Content type=\"error\"><filename>" + filesNotFound + "</filename></Content>");
                                siteAdminErrorNotification(oErrorXml.DocumentElement);
                            }

                            // If nothing is found then display an error
                            if (documentPaths.Count == 0)
                            {
                                Redirect404(NotFoundPagePath);
                            }

                        }
                    }

                    else
                    {
                        if (!Information.IsNumeric(aDocId[0]))
                            throw new Exception("Incorrect Document Id Format");
                        nDocId = Conversions.ToLong(aDocId[0]);
                        sSql = "select * from tblContent where nContentKey = " + aDocId[0];
                        oDS = this.moDbHelper.GetDataSet(sSql, "Item");

                        if (oDS == null | oDS.Tables.Count == 0)
                        {
                            Redirect404(NotFoundPagePath);
                        }
                        else
                        {

                            bool allowAccess = true;

                            if (Strings.LCase(this.moConfig["SecureDownloads"]) == "on")
                            {
                                XmlElement oPageElmt;
                                if (moPageXml.DocumentElement is null)
                                {
                                    moPageXml.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                                    oPageElmt = moPageXml.CreateElement("Page");
                                    moPageXml.AppendChild(oPageElmt);
                                    GetRequestVariablesXml(ref oPageElmt);
                                    GetSettingsXml(ref oPageElmt);
                                }
                                else
                                {
                                    oPageElmt = moPageXml.DocumentElement;
                                }

                                GetStructureXML("Site");

                                allowAccess = this.moDbHelper.checkContentLocationsInCurrentMenu(Conversions.ToLong(aDocId[0]), true);


                            }

                            if (allowAccess)
                            {

                                if (oDS.Tables["Item"]?.Rows?.Count > 0)
                                {
                                    strFilePath = this.moDbHelper.getContentFilePath(oDS.Tables["Item"].Rows[0], this.moRequest["xPath"]);
                                }

                                if (!string.IsNullOrEmpty(strFilePath))
                                {
                                    // lets clear up the file name
                                    // check for paths both local and virtual
                                    strFileName = Tools.Text.filenameFromPath(strFilePath);
                                    FileExt = Strings.Mid(strFileName, Strings.InStrRev(strFileName, ".") + 1);
                                    strFileType = Tools.FileHelper.GetMIMEType(FileExt);

                                    var fso = new FileInfo(strFilePath);

                                    if (fso.Exists)
                                    {
                                        Tools.Security.Impersonate oImp = null;
                                        if (Conversions.ToBoolean(impersonationMode))
                                        {
                                            oImp = new Tools.Security.Impersonate();
                                            oImp.ImpersonateValidUser(this.moConfig["AdminAcct"], this.moConfig["AdminDomain"], this.moConfig["AdminPassword"], cInGroup: this.moConfig["AdminGroup"]);

                                        }

                                        var oFileStream = new FileStream(strFilePath, FileMode.Open);
                                        strFileSize = oFileStream.Length.ToString();

                                        var Buffer = new byte[Conversions.ToInteger(strFileSize) + 1];
                                        oFileStream.Read(Buffer, 0, Conversions.ToInteger(strFileSize));
                                        oFileStream.Close();

                                        ctx.Response.Clear();
                                        // Const adTypeBinary = 1
                                        ctx.Response.AddHeader("Connection", "keep-alive");
                                        if (ctx.Request.QueryString["mode"] == "open")
                                        {
                                            ctx.Response.AddHeader("Content-Disposition", "filename=" + strFileName);
                                        }
                                        else
                                        {
                                            ctx.Response.AddHeader("Content-Disposition", "attachment; filename=" + strFileName);
                                        }
                                        ctx.Response.AddHeader("Content-Length", (Conversions.ToDouble(strFileSize) + 1d).ToString());
                                        ctx.Response.Charset = "UTF-8";
                                        ctx.Response.ContentType = Tools.FileHelper.GetMIMEType(FileExt);
                                        ctx.Response.BinaryWrite(Buffer);
                                        ctx.Response.Flush();

                                        objStream = null;

                                        // Activity Log
                                        if ((double)this.mnUserId != Conversions.ToDouble("0") & this.mbAdminMode == false & this.Features.ContainsKey("ActivityReporting"))
                                        {
                                            // NB: 30-03-2010 New check to add in the ArtId (original line is the 2nd, with ArtId hardcoded as 0?)
                                            if (this.moRequest["docId"] != null)
                                            {
                                                this.moDbHelper.CommitLogToDB(Cms.dbHelper.ActivityType.DocumentDownloaded, this.mnUserId, this.moSession.SessionID, DateTime.Now, this.mnPageId, Conversions.ToInteger(this.moRequest["docId"]), strFileName);
                                            }
                                            else
                                            {
                                                this.moDbHelper.CommitLogToDB(Cms.dbHelper.ActivityType.DocumentDownloaded, this.mnUserId, this.moSession.SessionID, DateTime.Now, this.mnPageId, 0, strFileName);
                                            }
                                        }

                                        if (Conversions.ToBoolean(impersonationMode))
                                        {
                                            oImp.UndoImpersonation();
                                            oImp = null;
                                        }
                                    }


                                    else
                                    {
                                        // ------------------------------------------ 26-08-2008
                                        // here we want to email the site admin to tell them the file is missing
                                        oErrorXml.LoadXml("<Content type=\"error\"><filename>" + strFilePath + "</filename></Content>");
                                        siteAdminErrorNotification(oErrorXml.DocumentElement);
                                        // -----------------------------------------------------

                                        // ---------------------------------------------Original

                                        // -----------------------------------------------------
                                        if ((double)this.mnUserId != Conversions.ToDouble("0") & this.mbAdminMode == false & this.Features.ContainsKey("ActivityReporting"))
                                        {
                                            this.moDbHelper.CommitLogToDB(Cms.dbHelper.ActivityType.DocumentDownloaded, this.mnUserId, this.moSession.SessionID, DateTime.Now, this.mnPageId, 0, "ERROR NOT FOUND:" + strFileName);
                                        }

                                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(this.moCtx.Application["PageNotFoundId"], RootPageId, false)))
                                        {
                                            this.moCtx.ApplicationInstance.CompleteRequest();
                                            Redirect404(NotFoundPagePath);
                                        }
                                        else
                                        {
                                            // Kept for follow up window, however does this send original mail out?
                                            sProcessInfo = "File Not Found:" + strFilePath;
                                            Information.Err().Raise(1007, Description: "ProteanCMS Error: " + sProcessInfo);
                                        }


                                    }
                                }
                                // Content ID exists but is does not contain an xPath so is not a valid document
                                else if (gnPageNotFoundId > 1L)
                                {
                                    Redirect404(NotFoundPagePath);
                                }
                                else
                                {
                                    this.Redirect404(this.moConfig["BaseUrl"]);
                                }
                            }


                            else
                            {
                                this.moSession["LogonRedirect"] = this.moRequest.ServerVariables["PATH_INFO"] + "?" + this.moRequest.ServerVariables["QUERY_STRING"];
                                msRedirectOnEnd = AccessDeniedPagePath;

                                this.moResponse.Redirect(msRedirectOnEnd, false);
                                this.moCtx.ApplicationInstance.CompleteRequest();
                                return;

                            }
                        }

                    }
                }

                // put this in to prevent a redirect if we are calling this from somewhere strange.
                else if (gnPageNotFoundId > 1L)
                {
                    Redirect404(NotFoundPagePath);
                }
                else
                {
                    this.Redirect404(this.moConfig["BaseUrl"]);

                    // ctx.Response.StatusCode = 404
                    // ctx.Response.Flush()

                }
            }

            catch (Exception ex)
            {

                // returnException(msException, mcModuleName, "returnDocumentFromItem", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "returnDocumentFromItem", ex, sProcessInfo));
                this.moResponse.Write(msException);
            }

        }

        public void Redirect404(string PagePath)
        {
            msRedirectOnEnd = PagePath;
            this.moResponse.StatusCode = 404;
            this.moResponse.Redirect(msRedirectOnEnd, false);
        }

        public void returnPageAsPDF(ref System.Web.HttpContext ctx)
        {
            this.PerfMon.Log("Web", "returnDocumentFromItem");
            var oXML = new XmlDocument();
            string sSql = string.Empty;
            string strFilePath = string.Empty;
            string strFileName = this.moRequest["filename"] + ".pdf";
            string objStream = string.Empty;
            string strFileSize;
            // Dim strPageInfo, strReferrer
            // Dim sWriter = New IO.StringWriter
            var oDS = new DataSet();
            string sProcessInfo = "returnDocumentFromItem";
            string sPath = string.Empty;
            var oErrorXml = new XmlDocument();
            long nArtId = Conversions.ToInteger(this.moRequest["artId"]);
            long nPageId = Conversions.ToInteger(this.moRequest["pgid"]);

            try
            {

                // First we get the page XML
                string cEwSitePDFXsl = this.moConfig["PagePDFXslPath"];

                if (this.mbAdminMode & !ibIndexMode & !(gnResponseCode == 404L))
                {
                    bPageCache = false;
                }

                sProcessInfo = "Transform PageXML Using XSLT";
                if (this.mbAdminMode & !ibIndexMode & !(gnResponseCode == 404L))
                {
                    sProcessInfo = "In Admin Mode";
                    if (moAdmin is null)
                    {
                        var argaWeb = this;
                        moAdmin = new Cms.Admin(ref argaWeb);
                    }
                    // Dim oAdmin As Admin = New Admin(Me)
                    // Dim oAdmin As Protean.Cms.Admin = New Protean.Cms.Admin(Me)
                    moAdmin.open(moPageXml);
                    var argoWeb = this;
                    moAdmin.adminProcess(ref argoWeb);
                    moAdmin.close();
                    moAdmin = (Cms.Admin)null;
                }
                else if (string.IsNullOrEmpty(moPageXml.OuterXml))
                {
                    sProcessInfo = "Getting Page XML";
                    GetPageXML();
                }

                // Next we transform using into FO.Net Xml

                if (moTransform is null)
                {
                    string styleFile = this.goServer.MapPath(cEwSitePDFXsl);
                    this.PerfMon.Log("Web", "ReturnPageHTML - loaded Style");
                    var argaWeb1 = this;
                    moTransform = new Protean.XmlHelper.Transform(ref argaWeb1, styleFile, false);
                }

                msException = "";

                moTransform.mbDebug = gbDebug;
                icPageWriter = new StringWriter();
                TextWriter argoWriter = icPageWriter;
                moTransform.ProcessTimed(moPageXml, ref argoWriter);
                icPageWriter = (StringWriter)argoWriter;



                string foNetXml = icPageWriter.ToString();

                if (foNetXml.StartsWith("<html"))
                {
                    this.moResponse.Write(foNetXml);
                }
                else
                {
                    // now we use FO.Net to generate our PDF

                    var oFoNet = new Fonet.FonetDriver();
                    var ofileStream = new MemoryStream();
                    var oTxtReader = new StringReader(foNetXml);
                    oFoNet.CloseOnExit = false;

                    var rendererOpts = new Fonet.Render.Pdf.PdfRendererOptions();

                    rendererOpts.Author = "ProteanCMS";
                    rendererOpts.EnablePrinting = true;
                    rendererOpts.FontType = Fonet.Render.Pdf.FontType.Embed;

                    var dir = new DirectoryInfo(this.goServer.MapPath("/") + "/fonts");
                    if (dir.Exists) { 
                    DirectoryInfo[] subDirs = dir.GetDirectories();
                    FileInfo[] files = dir.GetFiles();

                    foreach (var fi in files)
                    {
                        string cExt = Strings.LCase(fi.Extension);
                        switch (cExt ?? "")
                        {
                            case ".otf":
                                {
                                    rendererOpts.AddPrivateFont(fi);
                                    break;
                                }
                        }
                    }
                    }
                    oFoNet.Options = rendererOpts;
                    oFoNet.Render(oTxtReader, ofileStream);

                    // And then we stram out to the browser

                    this.moResponse.Buffer = true;
                    this.moResponse.Expires = 0;
                    this.goServer.ScriptTimeout = 10000;

                    strFileSize = ofileStream.Length.ToString();

                    byte[] Buffer = ofileStream.ToArray();

                    ctx.Response.Clear();
                    // Const adTypeBinary = 1
                    ctx.Response.AddHeader("Connection", "keep-alive");
                    if (ctx.Request.QueryString["mode"] == "open")
                    {
                        ctx.Response.AddHeader("Content-Disposition", "filename=" + Strings.Replace(strFileName, ",", ""));
                    }
                    else
                    {
                        ctx.Response.AddHeader("Content-Disposition", "attachment; filename=" + Strings.Replace(strFileName, ",", ""));
                    }
                    ctx.Response.AddHeader("Content-Length", strFileSize);
                    // ctx.Response.Charset = "UTF-8"
                    ctx.Response.ContentType = Tools.FileHelper.GetMIMEType("PDF");
                    ctx.Response.BinaryWrite(Buffer);
                    ctx.Response.Flush();

                    objStream = null;
                    oFoNet = null;
                    oTxtReader = null;
                    ofileStream = null;
                }
            }


            catch (Exception ex)
            {

                // returnException(msException, mcModuleName, "returnDocumentFromItem", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "returnPageAsPDF", ex, sProcessInfo));
                this.moResponse.Write(msException);
            }

        }


        private void siteAdminErrorNotification(XmlElement oXmlE)
        {
            this.PerfMon.Log("Web", "siteAdminErrorNotification");
            // Create the Email for File not Found error

            XmlElement oPageElmt;

            try
            {
                // Create the Page Xml
                if (moPageXml.DocumentElement is null)
                {
                    moPageXml.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                    oPageElmt = moPageXml.CreateElement("Page");
                    moPageXml.AppendChild(oPageElmt);
                    GetRequestVariablesXml(ref oPageElmt);

                    GetSettingsXml(ref oPageElmt);
                }
                else
                {
                    oPageElmt = moPageXml.DocumentElement;
                }

                // GetUserXML() - Returns XML Element of User Details
                var oUserXml = GetUserXML();
                // Require Error Handling
                if (oUserXml != null)
                {
                    moPageXml.DocumentElement.AppendChild(moPageXml.ImportNode(oUserXml.CloneNode(true), true));
                    // ^^ This line had some basic testing, seems to work, might need extended?
                }

                // Add remaining details pulled in
                moPageXml.DocumentElement.AppendChild(moPageXml.ImportNode(oXmlE, true));

                var oMsg = new Protean.Messaging(ref msException);
                Cms.dbHelper argodbHelper = null;
                oMsg.emailer(oPageElmt, "/ewcommon/xsl/email/siteAdminError_FileNotFound.xsl", "ProteanCMS Error", "error@proteancms.com", this.moConfig["siteAdminEmail"], "File not found", ccRecipient: "error@proteancms.com", odbHelper: ref argodbHelper);
            }

            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "siteAdminErrorNotification", ex, ""));
            }

        }

        /// <summary>
        /// Multiple ParentId Checking
        /// each content item with mulitple parents replace with current page as parId
        /// each content item with no parent replace with current page as parId
        /// </summary>
        /// <param name="oPage">The Page XML Element</param>
        /// <param name="nCurrentPage">The current Page ID</param>
        /// <remarks></remarks>
        public void CheckMultiParents(ref XmlElement oPage, int nCurrentPage)
        {
            this.PerfMon.Log("Web", "CheckMultiParents-Start");
            try
            {
                XmlElement oElmt;

                // TS optimise don't need these in GetParId for each node
                XmlElement oMenu = (XmlElement)moPageXml.SelectSingleNode("/Page/Menu[@id='Site']");
                if (oMenu is null)
                {
                    oMenu = GetStructureXML((long)this.mnUserId);
                }
                XmlElement oCurPage = (XmlElement)oMenu.SelectSingleNode("descendant-or-self::MenuItem[@id='" + nCurrentPage + "']");

                foreach (XmlElement currentOElmt in oPage.SelectNodes("descendant-or-self::Content[contains(@parId,',')]"))
                {
                    oElmt = currentOElmt;
                    oElmt.SetAttribute("parId", GetParId(oElmt.GetAttribute("parId"), nCurrentPage, ref oMenu, ref oCurPage).ToString());
                }
                foreach (XmlElement currentOElmt1 in oPage.SelectNodes("descendant-or-self::Content[@parId='']"))
                {
                    oElmt = currentOElmt1;
                    oElmt.SetAttribute("parId", nCurrentPage.ToString());
                }
                this.PerfMon.Log("Web", "CheckMultiParents-End");
            }
            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "CheckMultiParents", ex, gcEwSiteXsl, , gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "CheckMultiParents", ex, ""));
            }
        }


        // Multiple ParentId Checking FOR CART ITEMS
        public void CheckMultiParents(ref XmlElement oCartElmt)
        {
            this.PerfMon.Log("Web", "CheckMultiParents");
            try
            {

                foreach (XmlElement oElmt in oCartElmt.SelectNodes("descendant-or-self::Item[contains(@parId,',')]"))
                    oElmt.SetAttribute("parId", this.GetParId(oElmt.GetAttribute("parId"), this.mnPageId).ToString());
            }
            // For Each oElmt In oPage.SelectNodes("descendant-or-self::Item[@parId='']")
            // oElmt.SetAttribute("parId", nCurrentPage)
            // Next
            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "CheckMultiParents", ex, gcEwSiteXsl, , gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "CheckMultiParents", ex, ""));
            }
        }

        // TS - now overloaded to preseve existing public function
        public int GetParId(string nParids, int nCurrentPage)
        {
            this.PerfMon.Log("Web", "GetParId");
            string cProcessInfo = string.Empty;
            string[] oParents = Strings.Split(nParids, ",");
            try
            {

                XmlElement oMenu = (XmlElement)moPageXml.SelectSingleNode("/Page/Menu[@id='Site']");
                if (oMenu is null)
                {
                    oMenu = GetStructureXML((long)this.mnUserId);
                }

                XmlElement oCurPage = (XmlElement)oMenu.SelectSingleNode("descendant-or-self::MenuItem[@id='" + nCurrentPage + "']");

                return GetParId(nParids, nCurrentPage, ref oMenu, ref oCurPage);
            }

            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetParId", ex, ""));
                return Conversions.ToInteger(oParents[0]);
            }
        }

        public int GetParId(string nParids, int nCurrentPage, ref XmlElement oMenu, ref XmlElement oCurPage)
        {
            // PerfMon.Log("Web", "GetParId")
            string cProcessInfo = string.Empty;
            string[] oParents = Strings.Split(nParids, ",");
            string sFilteredParents = "";
            string cXPath = "";
            int i;
            try
            {

                var loopTo = Information.UBound(oParents);
                for (i = 0; i <= loopTo; i++)
                {
                    // We are only interested parIds in the current site tree
                    if (oMenu.SelectSingleNode("descendant-or-self::MenuItem[@id='" + oParents[i] + "']") != null)
                    {
                        if (!string.IsNullOrEmpty(cXPath))
                            cXPath += " or ";
                        cXPath += "@id='" + oParents[i] + "'";
                        if (Conversions.ToDouble(oParents[i]) > nCurrentPage)
                        {
                            sFilteredParents = oParents[i] + ",";
                        }
                    }
                }

                // if no parId on this site return current page
                if (string.IsNullOrEmpty(sFilteredParents))
                {
                    return nCurrentPage;
                }
                else
                {
                    string[] oFilteredParents;
                    oFilteredParents = Strings.Split(sFilteredParents.Remove(sFilteredParents.LastIndexOf(","), 1), ",");

                    if (oFilteredParents.Length == 1)
                    {
                        return Conversions.ToInteger(oFilteredParents[0]);
                    }
                    else
                    {
                        cXPath = "MenuItem[" + cXPath + "]";
                        int[] oIDs = new int[1], oSteps = new int[1];
                        ClosestDescendants(oCurPage, 0, cXPath, ref oIDs, ref oSteps);
                        ClosestAncestor(oCurPage, 0, cXPath, oFilteredParents, ref oIDs, ref oSteps);

                        int nLowestStep = 99999;
                        var nNewPage = default(int);
                        var loopTo1 = Information.UBound(oSteps);
                        for (i = 0; i <= loopTo1; i++)
                        {
                            if (oSteps[i] < nLowestStep)
                            {
                                nNewPage = oIDs[i];
                            }
                        }

                        if (nNewPage == 0)
                        {
                            return Conversions.ToInteger(oFilteredParents[0]);
                        }
                        else
                        {
                            return nNewPage;
                        }
                    }
                }
            }




            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "GetParId", ex, gcEwSiteXsl, , gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetParId", ex, ""));
                return Conversions.ToInteger(oParents[0]);
            }
        }

        public void ClosestDescendants(XmlElement oElmt, int nStep, string cXpath, ref int[] oIDs, ref int[] oSteps)
        {
            XmlElement oChild;
            try
            {
                if (nStep == 0)
                    this.PerfMon.Log("Web", "ClosestDescendants-Start");
                nStep += 1;
                foreach (XmlElement currentOChild in oElmt.SelectNodes(cXpath))
                {
                    oChild = currentOChild;
                    Array.Resize(ref oIDs, Information.UBound(oIDs) + 1);
                    Array.Resize(ref oSteps, Information.UBound(oSteps) + 1);
                    oIDs[Information.UBound(oIDs)] = Conversions.ToInteger(oChild.GetAttribute("id"));
                    oSteps[Information.UBound(oSteps)] = nStep;
                }
                foreach (XmlElement currentOChild1 in oElmt.SelectNodes("MenuItem"))
                {
                    oChild = currentOChild1;
                    ClosestDescendants(oChild, nStep, cXpath, ref oIDs, ref oSteps);
                }
                if (nStep == 0)
                    this.PerfMon.Log("Web", "ClosestDescendants-End");
            }
            catch (Exception)
            {
                // dont do a thing, might not be children
            }
        }

        public void ClosestAncestor(XmlElement oElmt, int nStep, string cXpath, string[] oParents, ref int[] oIDs, ref int[] oSteps)
        {
            this.PerfMon.Log("Web", "ClosestAncestor");
            try
            {
                XmlElement oParentElmt = (XmlElement)oElmt.ParentNode;

                nStep += 1;
                // check parent
                int i;
                var loopTo = Information.UBound(oParents);
                for (i = 0; i <= loopTo; i++)
                {
                    if ((oParentElmt.GetAttribute("id") ?? "") == (oParents[i] ?? ""))
                    {
                        oIDs[Information.UBound(oIDs)] = Conversions.ToInteger(oParentElmt.GetAttribute("id"));
                        oSteps[Information.UBound(oSteps)] = nStep;
                    }
                }
                // now check its grandparents children?
                ClosestDescendants((XmlElement)oParentElmt.ParentNode, nStep, cXpath, ref oIDs, ref oSteps);
                // keep going up
                ClosestAncestor((XmlElement)oParentElmt.ParentNode, nStep, cXpath, oParents, ref oIDs, ref oSteps);
            }

            catch (Exception)
            {
                // dont do anything, there might not be ancestors
            }
        }

        public string Referrer
        {
            get
            {
                try
                {

                    if (this.moSession != null)
                    {
                        mcSessionReferrer = Conversions.ToString(this.moSession["Referrer"]);
                    }
                    else
                    {
                        mcSessionReferrer = this.moRequest.UrlReferrer.AbsoluteUri;
                    }

                    if (mcSessionReferrer is null)
                    {
                        if (this.moRequest.UrlReferrer != null)
                        {
                            if ((this.moRequest.UrlReferrer.Host ?? "") != (this.moRequest.ServerVariables["HTTP_HOST"] ?? ""))
                            {
                                this.moSession.Add("Referrer", this.moRequest.UrlReferrer.AbsoluteUri);
                                mcSessionReferrer = Conversions.ToString(this.moSession["Referrer"]);
                            }
                        }
                    }

                    return mcSessionReferrer;
                }

                catch (Exception)
                {
                    return "";
                }
            }
        }

        protected virtual Assembly Generator
        {
            get
            {
                return Assembly.GetExecutingAssembly();
            }
        }


        protected virtual AssemblyName[] ReferencedAssemblies
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetReferencedAssemblies();
            }
        }

        /// <summary>
        /// If a request has been made for the page xml to be output, 
        /// this checks the IP address against locked down addresses
        /// if the IP address lockdown config setting has been created.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        protected bool ValidatedOutputXml
        {
            get
            {
                try
                {
                    string xmlAllowedIPList = this.moConfig["XmlAllowedIPList"];
                    return Conversions.ToBoolean(Interaction.IIf(string.IsNullOrEmpty(xmlAllowedIPList), (object)this.mbOutputXml, IsCurrentIPAddressInList(xmlAllowedIPList)));
                }
                catch (Exception ex)
                {
                    OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ValidatedOutputXml", ex, ""));
                    return false;
                }
            }
        }



        public virtual bool HasSession
        {
            get
            {
                return this.moSession != null;
            }
        }

        // ''' <summary>
        // ''' If in Single Session per Session mode this will log an activity indicating that the user is still in session.
        // ''' </summary>
        // ''' <remarks>It is called in EonicWeb but has been extracted so that it may be called by lightweight EonicWeb calls (e.g. ajax calls)</remarks>
        public void LogSingleUserSession()
        {
            try
            {

                Cms argmyWeb = this;
                moMemProv.Activities.LogSingleUserSession(ref argmyWeb);

            }

            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "LogSingleUserSession", ex, ""));
            }
        }

        private void GetRequestLanguage()
        {
            this.PerfMon.Log("Web", "GetRequestLanguage");

            string cProcessInfo = string.Empty;
            string sCurrency = "";
            string httpPrefix = "http://";


            try
            {


                if (this.goLangConfig != null)
                {

                    if (this.moRequest.ServerVariables["SERVER_PORT_SECURE"] == "1")
                    {
                        httpPrefix = "https://";
                    }

                    foreach (XmlElement oElmt in this.goLangConfig.SelectNodes("Language"))
                    {
                        switch (Strings.LCase(oElmt.GetAttribute("identMethod")) ?? "")
                        {
                            case "domain":
                                {
                                    if ((oElmt.GetAttribute("identifier") ?? "") == (this.moRequest.ServerVariables["HTTP_HOST"] ?? ""))
                                    {
                                        mcPageLanguage = oElmt.GetAttribute("code");
                                        mcPageLanguageUrlPrefix = httpPrefix + oElmt.GetAttribute("identifier");
                                        sCurrency = oElmt.GetAttribute("currency");
                                    }

                                    break;
                                }
                            case "path":
                                {
                                    if (this.moRequest.ServerVariables["HTTP_X_ORIGINAL_URL"] != null)
                                    {
                                        if (this.moRequest.ServerVariables["HTTP_X_ORIGINAL_URL"].StartsWith("/" + oElmt.GetAttribute("identifier") + "/") | (this.moRequest.ServerVariables["HTTP_X_ORIGINAL_URL"] ?? "") == ("/" + oElmt.GetAttribute("identifier") ?? "") | this.moRequest.ServerVariables["HTTP_X_ORIGINAL_URL"].StartsWith("/" + oElmt.GetAttribute("identifier") + "?"))

                                        {
                                            mcPageLanguage = oElmt.GetAttribute("code");
                                            mcPageLanguageUrlPrefix = httpPrefix + this.goLangConfig.GetAttribute("defaultDomain") + "/" + oElmt.GetAttribute("identifier");
                                            sCurrency = oElmt.GetAttribute("currency");

                                            // remove lang from path
                                            this.mcPagePath = this.mcPagePath.Replace("/" + oElmt.GetAttribute("identifier"), "/");
                                            this.mcPagePath = this.mcPagePath.Replace("//", "/");
                                        }
                                    }

                                    break;
                                }
                            case "page":
                                {
                                    mcPageLanguage = this.moDbHelper.getPageLang((long)this.mnPageId);
                                    break;
                                }
                        }
                    }

                    if (string.IsNullOrEmpty(mcPageLanguage))
                    {
                        // set Default Language
                        mcPageLanguage = this.goLangConfig.GetAttribute("code");
                        sCurrency = this.goLangConfig.GetAttribute("currency");
                        mcPreferredLanguage = mcPageLanguage;
                        mcPageDefaultDomain = httpPrefix + this.goLangConfig.GetAttribute("defaultDomain");
                    }

                    if (this.moSession != null)
                    {
                        if (!string.IsNullOrEmpty(sCurrency))
                        {
                            this.moSession["bCurrencySelected"] = (object)true;
                            this.moSession["cCurrency"] = sCurrency;
                            this.moSession["cCurrencyRef"] = sCurrency;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetRequestLanguage", ex, ""));
            }

        }
        /// <summary>
        /// Check the language setting against cookies and the like
        /// </summary>
        /// <remarks></remarks>
        private void SetPageLanguage()
        {
            this.PerfMon.Log("Web", "SetPageLanguage");

            string cProcessInfo = "";
            string sCurrency = string.Empty;

            try
            {
                if (this.goLangConfig != null)
                {

                    if (string.IsNullOrEmpty(mcPageLanguage))
                    {
                        GetRequestLanguage();
                    }

                    // if the page requested is a version in another language then set the page language.
                    if (this.mbAdminMode)
                    {
                        string cPageLang = this.moDbHelper.getPageLang((long)this.mnPageId);
                        if ((cPageLang ?? "") != (mcPageLanguage ?? "") & (cPageLang ?? "") != (this.goLangConfig.GetAttribute("code") ?? ""))
                        {
                            mcPageLanguage = this.moDbHelper.getPageLang((long)this.mnPageId);
                        }
                    }

                    // add the language info to the pageXml
                    if (moPageXml.DocumentElement.SelectSingleNode("languages") is null)
                    {
                        var oLangElmt = moPageXml.CreateElement("Lang");
                        oLangElmt.InnerXml = this.goLangConfig.OuterXml;
                        moPageXml.DocumentElement.AppendChild(oLangElmt.FirstChild);
                    }
                }
                // Legacy code
                else if (moPageXml.DocumentElement.SelectSingleNode("Contents") != null)
                {
                    // First find the page language, which should be called after content has been loaded in
                    string cLang = "";
                    var argoNode = moPageXml.DocumentElement;
                    if (Tools.Xml.NodeState(ref argoNode, "//Content[@name='XmlLang']", "", "", XmlNodeState.IsEmpty, null, "", cLang, bCheckTrimmedInnerText: false) == Tools.Xml.XmlNodeState.HasContents)
                    {
                        mcPageLanguage = cLang;
                    }
                    else
                    {
                        mcPageLanguage = gcLang;
                    }

                    // Next check for a user language preference
                    if (this.moRequest.Cookies["language"] != null)
                    {
                        mcPreferredLanguage = this.moRequest.Cookies["language"].Value.ToString();
                    }

                    moPageXml.DocumentElement.SetAttribute("translang", Conversions.ToString(Interaction.IIf(string.IsNullOrEmpty(mcPreferredLanguage), mcPageLanguage, mcPreferredLanguage)));
                }

                if (!string.IsNullOrEmpty(mcPageLanguage))
                {
                    // Set the page attribute
                    moPageXml.DocumentElement.SetAttribute("lang", mcPageLanguage);
                    gcLang = mcPageLanguage;
                }

                if (!string.IsNullOrEmpty(mcPreferredLanguage))
                {
                    // Set the page attribute
                    moPageXml.DocumentElement.SetAttribute("userlang", mcPreferredLanguage);
                }
                else
                {
                    moPageXml.DocumentElement.SetAttribute("userlang", mcPageLanguage);
                }
            }

            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "SetPageLanguage", ex, cProcessInfo));
            }
        }

        protected void ProcessPageXMLForLanguage()
        {

            this.PerfMon.Log("Web", "ProcessContentForLanguage - Start");

            try
            {
                // Only strip out the content if we're not editting a form.
                bool isEditForm = false;
                string keepLanguageVariants = "";
                if (this.moRequest.QueryString.Count > 0)
                {
                    keepLanguageVariants = this.moRequest["keeplanguagevariants"];
                }

                if (moAdmin != null)
                {
                    isEditForm = moAdmin.Command.Contains("Add") | moAdmin.Command.Contains("Edit");
                }

                if (!isEditForm | !string.IsNullOrEmpty(keepLanguageVariants))
                {

                    if (!string.IsNullOrEmpty(mcPageLanguage))
                    {
                        var nsMgr = new XmlNamespaceManager(moPageXml.NameTable);
                        nsMgr.AddNamespace("xml", "http://www.w3.org/XML/1998/namespace");

                        // Find all the parents of nodes with xml:lang attributes
                        // And only those with more than one matching node (just to begin to cut the scope down)
                        var children = new ArrayList();
                        foreach (XmlElement parent in moPageXml.SelectNodes("//*[count(*[@xml:lang]) > 1]", nsMgr))
                        {

                            foreach (XmlElement langChild in parent.SelectNodes("*[@xml:lang]", nsMgr))
                            {

                                if (!children.Contains(langChild.Name.ToString()))
                                {

                                    // This is a new node name
                                    children.Add(langChild.Name.ToString());

                                    // Note the checks
                                    // Check one says if there's only 1 node of this name, then don't delete it.
                                    // Check two looks at the user preference and tries to identify if there's an item in this language, which it'll keep and dump the rest.
                                    // Check three does the same as the above, except with the page language
                                    // Check two and three only work if there's some innertext to the language nodes being looked at.
                                    // If none if the checks are matched then the first node is kept.

                                    // Check one: Check for more than one node of this name
                                    var groupedNodes = parent.SelectNodes(langChild.Name.ToString(), nsMgr);
                                    if (groupedNodes.Count > 1)
                                    {

                                        bool groupNodesAreAllEmpty = true;
                                        // First check if all groupNodes are empty
                                        for (int nodeIndex = 1, loopTo = groupedNodes.Count - 1; nodeIndex <= loopTo; nodeIndex++)
                                        {
                                            if (!string.IsNullOrEmpty(groupedNodes.Item(nodeIndex).InnerText.Trim()))
                                            {
                                                groupNodesAreAllEmpty = false;
                                                break;
                                            }
                                        }

                                        // More than one node of this type.
                                        if (parent.SelectSingleNode(langChild.Name.ToString() + "[@xml:lang='" + mcPreferredLanguage + "']", nsMgr) != null && parent.SelectSingleNode(langChild.Name.ToString() + "[@xml:lang='" + mcPreferredLanguage + "']", nsMgr).InnerText.Trim().Length > 0 | groupNodesAreAllEmpty)
                                        {
                                            // Check two: Check for the user preferred language
                                            foreach (XmlElement childToRemove in parent.SelectNodes(langChild.Name.ToString() + "[not(@xml:lang) or @xml:lang!='" + mcPreferredLanguage + "']", nsMgr))
                                                childToRemove.SetAttribute("removeUnwantedLanguageNode", "");
                                        }
                                        else if (parent.SelectSingleNode(langChild.Name.ToString() + "[@xml:lang='" + mcPageLanguage + "']", nsMgr) != null && parent.SelectSingleNode(langChild.Name.ToString() + "[@xml:lang='" + mcPageLanguage + "']", nsMgr).InnerText.Trim().Length > 0 | groupNodesAreAllEmpty)
                                        {
                                            // Check three: Check for the user page language
                                            foreach (XmlElement childToRemove in parent.SelectNodes(langChild.Name.ToString() + "[not(@xml:lang) or @xml:lang!='" + mcPageLanguage + "']", nsMgr))
                                                childToRemove.SetAttribute("removeUnwantedLanguageNode", "");
                                        }
                                        else
                                        {


                                            // Assume the first is the default, get rid of the rest.
                                            for (int nodeIndex = 1, loopTo1 = groupedNodes.Count - 1; nodeIndex <= loopTo1; nodeIndex++)
                                                ((XmlElement)groupedNodes.Item(nodeIndex)).SetAttribute("removeUnwantedLanguageNode", "");
                                        }


                                    }

                                }

                            }

                            // Remove the unwanted nodes
                            foreach (XmlElement childToRemove in parent.SelectNodes("*[@removeUnwantedLanguageNode]", nsMgr))
                                parent.RemoveChild(childToRemove);

                            // Clear the node name list
                            children.Clear();

                        }
                    }


                }
                this.PerfMon.Log("Web", "ProcessContentForLanguage - End");
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "ProcessContentForLanguage", ex, "", "", gbDebug);
            }
        }

        /// <summary>
        /// Tests if the current incoming IP address can be found in a CSV list of IP addresses
        /// </summary>
        /// <param name="ipList">CSV list of IP addresses</param>
        /// <returns></returns>
        /// <remarks>Uses the incoming request to determine the IP address to check.  Use Protean.Tools.Text.IsIPAddressInList to check a specific IP address</remarks>
        public bool IsCurrentIPAddressInList(string ipList)
        {
            try
            {
                return Tools.Text.IsIPAddressInList(this.moRequest.ServerVariables["REMOTE_ADDR"], ipList);
            }

            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "IsCurrentIPAddressInList", ex, ""));
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemKey"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public int GetRequestItemAsInteger(string itemKey, int defaultValue = 0)
        {
            try
            {
                string item = this.moRequest[itemKey] + "";
                return Tools.Number.ConvertStringToIntegerWithFallback(item, defaultValue);
            }

            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetRequestItemAsInteger", ex, ""));
                return defaultValue;
            }
        }

        public int GetConfigItemAsInteger(string itemKey, int defaultValue = 0)
        {
            try
            {
                string item = this.moConfig[itemKey] + "";
                return Tools.Number.ConvertStringToIntegerWithFallback(item, defaultValue);
            }

            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetConfigItemAsInteger", ex, ""));
                return defaultValue;
            }
        }

        /// <summary>
        /// Adds a Response node to the Repsonses node of the page XML
        /// Response is a simple text node to convey response information.
        /// </summary>
        /// <param name="responseName">The identifier for this response</param>
        /// <param name="responseText">Optional the text that goes inside the Response node</param>
        /// <param name="overwriteDuplicates">Optional if True this will overwrite any Response node with the same Name</param>
        /// <remarks>If the Responses node has not been created then it will be.</remarks>
        public void AddResponse(string responseName, string responseText = "", bool overwriteDuplicates = true, ResponseType typeAttribute = ResponseType.Undefined)
        {
            try
            {

                if (_responses is null)
                {
                    var defaultDocument = new XmlDocument();
                    _responses = defaultDocument.CreateElement("Responses");
                }

                var response = _responses.OwnerDocument.CreateElement("Response");
                response.SetAttribute("name", responseName);
                response.InnerText = responseText;

                if (typeAttribute != ResponseType.Undefined)
                {
                    response.SetAttribute("type", typeAttribute.ToString());
                }

                if (overwriteDuplicates && _responses.SelectSingleNode("Response[@name='" + responseName + "']") != null)
                {
                    _responses.ReplaceChild(response, _responses.SelectSingleNode("Response[@name='" + responseName + "']"));
                }
                else
                {
                    _responses.AppendChild(response);
                }
            }
            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "AddResponse", ex, ""));
            }
        }

        private void CommitResponsesToPage()
        {
            try
            {
                if (_responses != null && moPageXml != null && moPageXml.SelectSingleNode("Page") != null)
                {
                    moPageXml.SelectSingleNode("Page").AppendChild(moPageXml.ImportNode(_responses, true));
                }
            }
            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "CommitResponsesToPage", ex, ""));

            }
        }

        public XmlNodeList PageXMLResponses
        {
            get
            {
                if (_responses is null)
                {
                    var emptyList = new XmlDocument();
                    return emptyList.ChildNodes;
                }
                else
                {
                    return _responses.SelectNodes("Response");
                }
            }
        }

        public string SessionID
        {
            get
            {
                if (this.moSession is null)
                {
                    return "";
                }
                else
                {
                    return this.moSession.SessionID;
                }
            }
        }


        /// <summary>
        /// Protean.Cms specific config retrieval.
        /// Shared function
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string ConfigValue(string key)
        {
            return Protean.Config.Value(key, "protean/web");
        }

        /// <summary>
        /// Protean.Cms specific config retrieval.
        /// Shared function
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public static System.Collections.Specialized.NameValueCollection Config()
        {
            return Protean.Config.ConfigSection("protean/web");
        }


        private bool disposedValue = false;        // To detect redundant calls

        // IDisposable
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // free managed resources when explicitly called
                    if (icPageWriter != null)
                    {
                        icPageWriter.Dispose();
                    }
                }

                // free shared unmanaged resources
            }
            disposedValue = true;
        }
        public virtual string UserFolder()
        {
            // NB : Empty to hold a place for Brokerage's bespoky-er-ness
            // Returns a string, that can be inserted as an Attribute
            return "";
        }

        /// <summary>
        /// Proxy for logging performance stats
        /// This is used for classes that Shadow Protean.Cms becuase PerfLog is found inside
        /// a module (stdTools) which means it is not accessible by shadowable classes.
        /// This should be a shared class really.
        /// </summary>
        /// <param name="cModuleName"></param>
        /// <param name="cProcessName"></param>
        /// <param name="cDescription"></param>
        /// <remarks></remarks>
        public void PerfMonLog(string cModuleName, string cProcessName, string cDescription = "")
        {
            this.PerfMon.Log(cModuleName, cProcessName, cDescription);
        }

        public bool RestartAppPool()
        {

            // First try killing your worker process
            // Try
            // Dim proc As Process = Process.GetCurrentProcess()
            // proc.Kill()
            // Return True
            // Catch ex As Exception
            // Return False
            // End Try

            // Try unloading appdomain
            try
            {
                // Dim oImp As Protean.Tools.Security.Impersonate = New Protean.Tools.Security.Impersonate
                // If oImp.ImpersonateValidUser(moConfig("AdminAcct"), moConfig("AdminDomain"), moConfig("AdminPassword"), , moConfig("AdminGroup")) Then
                // AppDomain.Unload(AppDomain.CurrentDomain)
                // don't forget to undo and set oImp to nothing
                return true;
            }
            // System.Web.HttpRuntime.UnloadAppDomain()
            // Else
            // Return False
            // End If
            catch (Exception)
            {
                return false;
            }

            // update web.config
            // Try
            // '  Protean.Config.UpdateConfigValue(Me, "eonic/web", "CompliedTransform", "rebuild")
            // File.SetLastWriteTimeUtc(goServer.MapPath("/web.config"), DateTime.UtcNow)
            // Catch ex As Exception
            // Return False
            // End Try

        }

        public void SavePage(string cUrl, string cBody)
        {
            string cProcessInfo = "";
            string filename = "";
            string filepath = "";
            string artId = string.Empty;
            //string Ext = ".html";
            try
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

                var oFS = new Protean.fsHelper(this.moCtx);
                oFS.mcRoot = gcProjectPath;
                oFS.mcStartFolder = this.goServer.MapPath(@"\" + gcProjectPath) + mcPageCacheFolder;

                cProcessInfo = "Saving:" + mcPageCacheFolder + filepath + @"\" + filename;

                // Tidy up the filename
                filename = Tools.FileHelper.ReplaceIllegalChars(filename);
                filename = Strings.Replace(filename, @"\", "-");
                filepath = Strings.Replace(filepath, "/", @"\") + "";
                if (filepath.StartsWith(@"\") & mcPageCacheFolder.EndsWith(@"\"))
                {
                    filepath.Remove(0, 1);
                }

                cProcessInfo = "Saving:" + mcPageCacheFolder + filepath + @"\" + filename;
                this.PerfMon.Log(mcModuleName, "SavePage", cProcessInfo);
                string cleanfilename = this.goServer.UrlDecode(filename);

                // Limit the file length to 255
                string Extension = Strings.Right(cleanfilename, cleanfilename.Length - Strings.InStr(cleanfilename, "."));
                cleanfilename = Strings.Left(cleanfilename, Strings.InStr(cleanfilename, ".") - 1);
                short FilenameLength = (short)(255 - Extension.Length);
                if (cleanfilename.Length > FilenameLength)
                {
                    cleanfilename = Strings.Left(cleanfilename, FilenameLength);
                }

                string FullFilePath = mcPageCacheFolder + filepath + @"\" + this.goServer.UrlDecode(cleanfilename + "." + Extension);

                // If FullFilePath.Length > 255 Then
                // FullFilePath = Left(FullFilePath, 240) & Ext
                // Else
                // FullFilePath = FullFilePath & Ext
                // End If

                if (string.IsNullOrEmpty(filepath))
                    filepath = "/";

                this.PerfMon.Log(mcModuleName, "Create Path - Start");
                string sError = oFS.CreatePath(filepath);
                oFS = (Protean.fsHelper)null;
                this.PerfMon.Log(mcModuleName, "Create Path - End");

                if (sError == "1")
                {
                    Tools.Security.Impersonate oImp = null;
                    if (Conversions.ToBoolean(impersonationMode))
                    {
                        this.PerfMon.Log(mcModuleName, "Impersonation - Start");
                        oImp = new Tools.Security.Impersonate();
                        oImp.ImpersonateValidUser(this.moConfig["AdminAcct"], this.moConfig["AdminDomain"], this.moConfig["AdminPassword"], cInGroup: this.moConfig["AdminGroup"]);
                        this.PerfMon.Log(mcModuleName, "Impersonation - End");
                    }

                    if (Alphaleonis.Win32.Filesystem.Directory.Exists(@"\\?\" + this.goServer.MapPath("/" + gcProjectPath) + mcPageCacheFolder + filepath))
                    {
                        if (!Alphaleonis.Win32.Filesystem.File.Exists(@"\\?\" + this.goServer.MapPath("/" + gcProjectPath) + FullFilePath))
                        {
                            this.PerfMon.Log(mcModuleName, "SavePage - start file write");
                            Alphaleonis.Win32.Filesystem.File.WriteAllText(@"\\?\" + this.goServer.MapPath("/" + gcProjectPath) + FullFilePath, cBody, System.Text.Encoding.UTF8);
                            this.PerfMon.Log(mcModuleName, "SavePage - end file write");
                        }
                        else
                        {
                            cProcessInfo += "<Error>File Locked: " + filepath + " - " + sError + "</Error>" + Constants.vbCrLf;
                            sError = cProcessInfo;
                        }
                    }
                    else
                    {
                        cProcessInfo += "<Error>Directory Not Exists: " + filepath + " - " + sError + "</Error>" + Constants.vbCrLf;
                        sError = cProcessInfo;
                    }

                    if (Conversions.ToBoolean(impersonationMode))
                    {
                        oImp.UndoImpersonation();
                        oImp = null;
                    }
                }

                else
                {
                    cProcessInfo += "<Error>Create Path: " + filepath + " - " + sError + "</Error>" + Constants.vbCrLf;
                    sError = cProcessInfo;
                }
                if (sError != "1")
                {
                    throw new Exception("An Error writing the page.");
                }
                this.PerfMon.Log("Web", "SavePage - End");
            }
            catch (Exception ex)
            {
                // if saving of a page fails we are not that bothered.
                // cExError &= "<Error>" & filepath & filename & ex.Message & "</Error>" & vbCrLf
                AddExceptionToEventLog(ex, cProcessInfo);
                // returnException(msException, mcModuleName, "SavePage", ex, "", cProcessInfo, gbDebug)
                // bIsError = True
                // PerfMon.Log("Web", "SavePage - error")
            }
        }

        public void ClearPageCache()
        {
            string cProcessInfo = "";
            try
            {

                moFSHelper.DeleteFolder(mcPageCacheFolder, this.goServer.MapPath("/" + gcProjectPath));
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "ClearPageCache", ex, "", cProcessInfo, gbDebug);
            }
        }
        /// <summary>
        /// get active productslist
        /// </summary>
        /// <param name="nArtId"></param>
        /// <returns></returns>
        public string CheckProductStatus(string nArtId)
        {
            try
            {
                // Dim oDr As System.Data.SqlClient.SqlDataReader
                string sSQL = "DECLARE @List VARCHAR(8000) SELECT @List = COALESCE(@List + ',', '') + CAST(C.nContentKey AS VARCHAR) from tblcontent C inner join tblAudit A on C.nAuditId = A.nAuditKey where A.nstatus=1 and c.ncontentkey in ( " + nArtId.ToString() + " )";
                sSQL = sSQL + " SELECT @List ";

                // "select  C.nContentKey from tblcontent C inner join tblAudit A on C.nAuditId = A.nAuditKey "
                // sSQL = sSQL & " where A.nstatus=1 and  c.ncontentkey in ( " & nArtId.ToString() & " )"
                if (this.moDbHelper is null)
                {
                    this.moDbHelper = (Cms.dbHelper)GetDbHelper();
                }
                using (var oDr = this.moDbHelper.getDataReaderDisposable(sSQL))  // Done by nita on 6/7/22
                {
                    if (oDr != null)
                    {
                        if (oDr.HasRows)
                        {
                            while (oDr.Read())
                                return oDr[0].ToString();
                        }
                        else
                        {
                            return "";
                        }
                    }
                    else
                    {
                        return "";
                    }
                }
            }
            catch (Exception)
            {
                return "";
            }

            return default;

        }

        public void ClearBundleCache(string bundlePath)
        {
            string cProcessInfo = "";
            try
            {


                var rootfolder = new DirectoryInfo(this.goServer.MapPath("/" + this.moConfig["ProjectPath"] + bundlePath + "/bundles"));
                if (rootfolder.Exists)
                {

                    // Delete all child Directories
                    foreach (DirectoryInfo dir in rootfolder.GetDirectories())
                    {
                        foreach (FileInfo filepath in dir.GetFiles())
                            filepath.Delete();
                        // "~/js/bundles/X"
                        string AppVarName = dir.FullName;
                        AppVarName = AppVarName.Substring(this.goServer.MapPath("/" + this.moConfig["ProjectPath"] + bundlePath).Length);
                        AppVarName = AppVarName.Replace(@"\", "/");
                        AppVarName = Strings.LCase(bundlePath + "/" + AppVarName.Trim('/'));

                        // check exists before we remove
                        if (this.moCtx.Application.Get(AppVarName) != null)
                        {
                            this.moCtx.Application.Remove(AppVarName);
                        }
                    }
                }
            }


            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "ClearPageCache", ex, "", cProcessInfo, gbDebug);
            }
        }

        #region  IDisposable Support 
        // This code added by Visual Basic to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        ~Cms()
        {
        }
    }
}