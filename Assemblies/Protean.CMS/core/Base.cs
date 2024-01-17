using System;
using System.Collections.Specialized;
using System.IO;
using System.Runtime.CompilerServices;
using System.Web.Configuration;
using System.Xml;
using Microsoft.VisualBasic;

namespace Protean
{

    public class Base
    {


        #region ErrorHandling

        // for anything controlling web
        public event OnErrorEventHandler OnError;

        public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);

        protected virtual void OnComponentError(object sender, Tools.Errors.ErrorEventArgs e)
        {
            // deals with the error
            // returnException(e.ModuleName, e.ProcedureName, e.Exception, mcEwSiteXsl, e.AddtionalInformation, gbDebug)
            // close connection pooling
            if (moDbHelper != null)
            {
                try
                {
                    moDbHelper.CloseConnection();
                }
                catch (Exception)
                {

                }
            }
            // then raises a public event
            OnError?.Invoke(sender, e);
        }

        #endregion

        #region Enums
        public enum licenceMode
        {
            Demo = 0,
            Development = 1,
            Live = 2
        }

        #endregion

        #region Declarations
        //private licenceMode moLicenceMode = licenceMode.Live;

        public System.Web.HttpContext moCtx;

        // Session / Request Level Properties
        public System.Web.HttpRequest moRequest;
        public System.Web.HttpResponse moResponse;
        public System.Web.SessionState.HttpSessionState moSession;

        public string mcPagePath;
        public string mcPageLayout;
        public int mnPageId = 0;
        public int mnArtId = 0;
        public int mnUserId = 0;

        public bool mbAdminMode = false;

        private bool mbSystemPage = false;
       // private Cms.dbHelper.PermissionLevel mnUserPagePermission = Cms.dbHelper.PermissionLevel.Open;

        public bool mbOutputXml = false;

        private Cms.dbHelper _moDbHelper;

        public virtual Cms.dbHelper moDbHelper
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _moDbHelper;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                _moDbHelper = value;
            }
        }

        // Application Level Properties   
        public NameValueCollection moConfig = (NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");
        // Public goApp As System.Web.HttpApplicationState
        public System.Web.Caching.Cache goCache;
        public System.Web.HttpServerUtility goServer;
        public XmlElement goLangConfig = (XmlElement)WebConfigurationManager.GetWebApplicationSection("protean/languages");

        public string mcModuleName = "Protean.Base";

        public System.Collections.Generic.Dictionary<string, string> Features = new System.Collections.Generic.Dictionary<string, string>();

        public bool mbPreview = false;
        public bool mbPreviewHidden = false;

        public PerfLog PerfMon;

        #endregion

        #region Constructors

        public Base() : this(System.Web.HttpContext.Current)
        {

        }

        public Base(System.Web.HttpContext Context)
        {

            string sProcessInfo = "";
            try
            {

                // Dim nMemUse As Integer = Process.GetCurrentProcess.WorkingSet64

                if (moCtx is null)
                {
                    moCtx = Context;
                }

                // goApp = moCtx.Application
                moRequest = moCtx.Request;
                moResponse = moCtx.Response;
                moSession = moCtx.Session;
                goServer = moCtx.Server;
                goCache = moCtx.Cache;

                PerfMon = new PerfLog("");
                PerfMon.Log("Base", "New");

                EnumberateFeatures();
            }

            catch (Exception ex)
            {
                // returnException(mcModuleName, "New", ex, "", sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, sProcessInfo));
            }
        }

        #endregion

        public void EnumberateFeatures()
        {
            Features.Add("Lite", "Lite");
            Features.Add("Pro", "Pro");
            if (Strings.LCase(moConfig["Cart"]) == "on")
            {
                Features.Add("Cart", "Cart");
            }
            if (Strings.LCase(moConfig["Quote"]) == "on")
            {
                Features.Add("Quote", "Quote");
            }
            if (Strings.LCase(moConfig["Membership"]) == "on")
            {
                Features.Add("Membership", "Membership");
            }
            if (Strings.LCase(moConfig["MailingList"]) == "on")
            {
                Features.Add("MailingList", "MailingList");
            }
            if (Strings.LCase(moConfig["Search"]) == "on" | Strings.LCase(moConfig["SiteSearch"]) == "on")
            {
                Features.Add("Search", "Search");
            }
            if (Strings.LCase(moConfig["VersionControl"]) == "on")
            {
                Features.Add("VersionControl", "VersionControl");
            }
            if (Strings.LCase(moConfig["Import"]) == "on")
            {
                Features.Add("Import", "Import");
            }
            if (Strings.LCase(moConfig["Sync"]) == "on")
            {
                Features.Add("Sync", "Sync");
            }
            if (Strings.LCase(moConfig["MemberCodes"]) == "on")
            {
                Features.Add("MemberCodes", "MemberCodes");
            }
            if (Strings.LCase(moConfig["Subscriptions"]) == "on")
            {
                Features.Add("Subscriptions", "Subscriptions");
            }
            if (Strings.LCase(moConfig["Scheduler"]) == "on")
            {
                Features.Add("Scheduler", "Scheduler");
            }
            if (Strings.LCase(moConfig["ActivityLogging"]) == "on" | Strings.LCase(moConfig["ActivityReporting"]) == "on")
            {
                Features.Add("ActivityLogging", "ActivityLogging");
                Features.Add("ActivityReporting", "ActivityReporting");
            }
            if (Strings.LCase(moConfig["PageVersions"]) == "on")
            {
                Features.Add("PageVersions", "PageVersions");
            }
            if (goLangConfig != null)
            {
                Features.Add("MultiLanguage", "MultiLanguage");
            }
            if (File.Exists(goServer.MapPath("/") + "eonic.theme.config"))
            {
                Features.Add("Themes", "Themes");
            }

        }

        private bool disposedValue = false;        // To detect redundant calls

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {

            disposedValue = true;
        }

        ~Base()
        {
        }
    }
}