using System;
using System.Web.Configuration;

namespace Protean
{

    public static class Startup
    {

        public static void Go()
        {

            JSStart.InitialiseJSEngine();

            ClearXSLCache();

        }

        public static void ClearXSLCache()
        {

            string compiledFolder = @"\xsltc\";
            string sProcessInfo = "";
            var myServer = System.Web.HttpContext.Current.Server;
            System.Collections.Specialized.NameValueCollection myConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");

            var oImp = new Tools.Security.Impersonate();
            if (oImp.ImpersonateValidUser(myConfig["AdminAcct"], myConfig["AdminDomain"], myConfig["AdminPassword"], true, myConfig["AdminGroup"]))
            {

                string cWorkingDirectory = myServer.MapPath(compiledFolder);
                sProcessInfo = "clearing " + cWorkingDirectory;
                var di = new System.IO.DirectoryInfo(cWorkingDirectory);

                foreach (var fi in di.EnumerateFiles())
                {
                    try
                    {

                        var oFileInfo = new System.IO.FileInfo(fi.FullName);
                        oFileInfo.IsReadOnly = false;
                        System.IO.File.Delete(fi.FullName);
                    }
                    catch (Exception)
                    {
                        // returnException("Protean.XmlHelper.Transform", "ClearXSLTassemblyCache", ex2, msXslFile, sProcessInfo)
                    }
                }

                oImp.UndoImpersonation();

            }

        }

    }

    public static class JSStart
    {

        public static void InitialiseJSEngine()
        {
            string sProcessInfo = "V8JsEngine";
            System.Web.HttpApplicationState moApp = null;
            if (System.Web.HttpContext.Current != null)
            {
                moApp = System.Web.HttpContext.Current.Application;
            }

            try
            {
                if (moApp != null)
                {
                    // ensures we have configured JSEngine for Bundle Transformer
                    if (moApp["JSEngineEnabled"] is null)
                    {
                        // Dim msieCfg As New JavaScriptEngineSwitcher.Msie.MsieSettings()
                        // msieCfg.EngineMode = JavaScriptEngineSwitcher.Msie.JsEngineMode.ChakraIeJsRt
                      
                        JavaScriptEngineSwitcher.Core.JsEngineSwitcher engineSwitcher = (JavaScriptEngineSwitcher.Core.JsEngineSwitcher)JavaScriptEngineSwitcher.Core.JsEngineSwitcher.Current;
                        // engineSwitcher.EngineFactories.Add(New JavaScriptEngineSwitcher.ChakraCore.ChakraCoreJsEngineFactory())
                        // engineSwitcher.EngineFactories.Add(New JavaScriptEngineSwitcher.Msie.MsieJsEngineFactory(msieCfg))
                        engineSwitcher.EngineFactories.Add(new JavaScriptEngineSwitcher.V8.V8JsEngineFactory());
                        string sJsEngine = "V8JsEngine";
                        // Dim sJsEngine As String = "MsieJsEngine"

                        // Dim sJsEngine As String = "ChakraCoreJsEngine"
                        // If moConfig("JSEngine") <> "" Then
                        // sJsEngine = moConfig("JSEngine")
                        // End If
                        engineSwitcher.DefaultEngineName = sJsEngine;
                        moApp["JSEngineEnabled"] = sJsEngine;
                    }
                }
            }
            catch (Exception ex)
            {
                sProcessInfo = ex.Message;
                // OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "InitialiseJSEngine", ex, sProcessInfo))
            }

        }

    }
}