using System;
using System.Collections.Specialized;
using System.IO;
using System.Runtime.CompilerServices;
using System.Web.Configuration;
using System.Xml;
using Microsoft.VisualBasic;

namespace Protean
{

    public class Base : IDisposable
    {


        #region ErrorHandling

        // for anything controlling web
        public event OnErrorEventHandler OnError;

        public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);

        protected virtual void OnComponentError(object sender, Tools.Errors.ErrorEventArgs e)
        {
            if (disposedValue)
            {
                return; // Don't process errors if disposed
            }

            if (moDbHelper != null)
            {
                try
                {
                    moDbHelper.CloseConnection();
                }
                catch (Exception) { }
            }

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

        //private bool mbSystemPage = false;
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
        public System.Web.HttpApplicationState goApp;
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

                goApp = moCtx.Application
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
               // OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, sProcessInfo));
                Dispose();
                throw;
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

        private bool disposedValue = false;

        // ✅ Add public Dispose() method
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // ✅ Complete the protected Dispose(bool) pattern
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)  // ✅ Now checks the flag
            {
                if (disposing)
                {
                    try
                    {
                        // 1. Unsubscribe event handlers FIRST
                        if (OnError != null)
                        {
                            foreach (var handler in OnError.GetInvocationList())
                            {
                                OnError -= (OnErrorEventHandler)handler;
                            }
                        }

                        // 2. Dispose database helper (CRITICAL)
                        if (_moDbHelper != null)
                        {
                            try
                            {
                                _moDbHelper.CloseConnection(true);
                                if (_moDbHelper is IDisposable disposableHelper)
                                {
                                    disposableHelper.Dispose();
                                }
                            }
                            catch (Exception ex)
                            {
                                // Log but don't throw in Dispose
                                System.Diagnostics.Debug.WriteLine(
                                    $"Error disposing moDbHelper: {ex.Message}");
                            }
                            finally
                            {
                                _moDbHelper = null;
                            }
                        }

                        // 3. Dispose PerfMon if it implements IDisposable
                        if (PerfMon is IDisposable disposablePerfMon)
                        {
                            disposablePerfMon.Dispose();
                        }
                        PerfMon = null;

                        // 4. Clear large collections
                        Features?.Clear();
                        Features = null;

                        // 5. Null out context references
                        moCtx = null;
                        moRequest = null;
                        moResponse = null;
                        moSession = null;
                        goServer = null;
                        goCache = null;
                        moConfig = null;
                        goLangConfig = null;
                    }
                    catch (Exception ex)
                    {
                        // Log disposal errors but don't throw
                        System.Diagnostics.Debug.WriteLine(
                            $"Error in Base.Dispose: {ex.Message}");
                    }
                }

                // Free unmanaged resources here if any exist

                disposedValue = true;
            }
        }

        // ✅ Remove empty finalizer OR implement properly if unmanaged resources exist
        // If no unmanaged resources, delete this:
        // ~Base() { }

        // ✅ OR if keeping finalizer, implement properly:
        ~Base()
        {
            Dispose(false);
        }

        // ✅ Add helper method to prevent use after disposal
        protected void ThrowIfDisposed()
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}