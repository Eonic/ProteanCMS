using Imazen.WebP;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web.Configuration;
using System.Xml;

namespace Protean
{

    public class Tests
    {

        public System.Web.HttpApplicationState goApp;
        public System.Web.HttpRequest goRequest;
        public System.Web.HttpResponse goResponse;
        public System.Web.SessionState.HttpSessionState goSession;
        public System.Web.HttpServerUtility goServer;

        public System.Collections.Specialized.NameValueCollection goConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");
        public System.Web.HttpContext moCtx;


        public Tests() : this(System.Web.HttpContext.Current)
        {

        }
        public Tests(System.Web.HttpContext oCtx)
        {

            moCtx = oCtx;

        }

        public string TestEmailSend()
        {
            try
            {
                string errMsg = "";

                var oMsg = new Protean.Messaging(ref errMsg);

                var oBodyXml = new XmlDocument();
                oBodyXml.LoadXml("<Items><Name>ProteanCMS Test</Name><Telephone /><Email>" + goConfig["SiteAdminEmail"] + "</Email><Message>This is a test</Message></Items>");
                string emailerMsg;
                XmlElement bodyXml = (XmlElement)oBodyXml.FirstChild;
                string emailXSL = "/ptn/email/mailform.xsl";
                if (goConfig["cssFramework"] != "bs5")
                {
                    emailXSL = "/ewcommon/xsl/email/mailform.xsl";
                }

                Protean.Cms.dbHelper argodbHelper = null;
                emailerMsg = oMsg.emailer(bodyXml, emailXSL, "ProteanCMS Test", goConfig["SiteAdminEmail"], goConfig["SiteAdminEmail"], "This is a TEST", odbHelper: ref argodbHelper).ToString();

                if (!string.IsNullOrEmpty(emailerMsg))
                {
                    return Tools.Text.EscapeJS(emailerMsg);
                }
                else
                {
                    return "1";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        public string TestWriteFile()
        {
            try
            {
                string htmltotest = "<h1>Test Write</h1>";

                Cms oEw = new Cms();
                oEw.InitializeVariables();
                string filepath = "/ewCache/";
                string filename = "FS-TEST.html";
                Tools.Security.Impersonate oImp = null;
                if (oEw.moConfig["AdminAcct"] != "")
                {
                    oImp = new Tools.Security.Impersonate();
                    if (oImp.ImpersonateValidUser(oEw.moConfig["AdminAcct"], oEw.moConfig["AdminDomain"], oEw.moConfig["AdminPassword"], cInGroup: oEw.moConfig["AdminGroup"]))
                    {
                    }
                    else
                    {
                        return "Impersonation Failed";
                    }
                }

                oEw.moFSHelper.SaveFile(ref filename, oEw.goServer.MapPath("/" + Cms.gcProjectPath) + filepath, System.Text.Encoding.Unicode.GetBytes(htmltotest));

                if (oEw.moConfig["AdminAcct"] != "")
                {
                    oImp.UndoImpersonation();
                    oImp = null;
                }
                return "File Written Using SaveFile :" + filepath + filename;
            }

            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string TestDeleteFile()
        {
            try
            {

                var oEw = new Cms();
                oEw.InitializeVariables();
                string filepath = @"\ewCache\";
                string filename = "FS-TEST.html";
                Tools.Security.Impersonate oImp = null;
                if (oEw.moConfig["AdminAcct"] != "")
                {
                    oImp = new Tools.Security.Impersonate();
                    if (oImp.ImpersonateValidUser(oEw.moConfig["AdminAcct"], oEw.moConfig["AdminDomain"], oEw.moConfig["AdminPassword"], cInGroup: oEw.moConfig["AdminGroup"]))
                    {
                    }
                    else
                    {
                        return "Impersonation Failed";
                    }
                }
                string response;

                response = oEw.moFSHelper.DeleteFile(oEw.goServer.MapPath("/" + Cms.gcProjectPath) + filepath, filename);

                oEw.moFSHelper.DeleteFile(oEw.goServer.MapPath("/" + Cms.gcProjectPath) + filepath, "FS-Alpha-TEST.html");

                if (oEw.moConfig["AdminAcct"] != "")
                {
                    oImp.UndoImpersonation();
                    oImp = null;
                }

                if (response == "1")
                {
                    return "File Deleted:" + filepath + filename;
                }
                else
                {
                    return response;
                }
            }

            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string TestImpersonation()
        {
            var oEw = new Cms();
            oEw.InitializeVariables();
            try
            {
                string returnValue = "";
                Tools.Security.Impersonate oImp = null;
                if (oEw.moConfig["AdminAcct"] != "")
                {
                    oImp = new Tools.Security.Impersonate();
                    if (oImp.ImpersonateValidUser(oEw.moConfig["AdminAcct"], oEw.moConfig["AdminDomain"], oEw.moConfig["AdminPassword"], cInGroup: oEw.moConfig["AdminGroup"]))
                    {
                        returnValue = "Impersonation Success";
                    }
                    else
                    {
                        returnValue = "Error Impersonation Failed";
                    }
                }
                else
                {
                    returnValue = "Impersonation Disabled";
                }
                if (oEw.moConfig["AdminAcct"] != "")
                {
                    oImp.UndoImpersonation();
                    oImp = null;
                }
                return returnValue;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string TestWriteFileAlphaFS()
        {
            try
            {
                string htmltotest = "<h1>Test Alpha FS Write</h1>";

                var oEw = new Cms();
                oEw.InitializeVariables();
                string filepath = "/ewCache/FS-Alpha-TEST.html";
                Tools.Security.Impersonate oImp = null;
                if (oEw.moConfig["AdminAcct"] != "")
                {
                    oImp = new Tools.Security.Impersonate();
                    if (oImp.ImpersonateValidUser(oEw.moConfig["AdminAcct"], oEw.moConfig["AdminDomain"], oEw.moConfig["AdminPassword"], cInGroup: oEw.moConfig["AdminGroup"]))
                    {
                    }
                    else
                    {
                        return "Impersonation Failed";
                    }
                }

                Alphaleonis.Win32.Filesystem.File.WriteAllText(@"\\?\" + oEw.goServer.MapPath("/" + Cms.gcProjectPath) + filepath, htmltotest, System.Text.Encoding.UTF8);

                if (oEw.moConfig["AdminAcct"] != "")
                {
                    oImp.UndoImpersonation();
                    oImp = null;
                }
                return "File Written Using AlphaFS :" + filepath;
            }

            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        public string TestCreateFolder()
        {
            try
            {

                var oEw = new Cms();
                oEw.InitializeVariables();
                string filepath = "/ewCache/Testfolder/test2";

                string response = oEw.moFSHelper.CreatePath(filepath);
                if (response == "1")
                {
                    return "Folder Created:" + filepath;
                }
                else
                {
                    return response;
                }
            }

            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        public string TestDeleteFolder()
        {
            try
            {

                var oEw = new Cms();
                oEw.InitializeVariables();
                string filepath = "/ewCache/Testfolder/test2";

                string response = oEw.moFSHelper.DeleteFolder("Testfolder", oEw.goServer.MapPath("/ewCache/"));
                if (response == "1")
                {
                    return "Folder Deleted:" + filepath;
                }
                else
                {
                    return response;
                }
            }

            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string TestHtmlTidy()
        {
            try
            {


                string htmltotest = "<h1>HTMLTidy is Tidying</H1>";

                string sResponse = stdTools.tidyXhtmlFrag(htmltotest, true, true, "");

                if (sResponse.StartsWith("<h1>HTMLTidy is Tidying</h1>"))
                {
                    sResponse = "HTML Tidy is working";
                }

                return sResponse;
            }



            catch (Exception ex)
            {
                return ex.Message + @"<br/>If server is 64 bit and failed to load tidy.dll reference and broke functionality. Get dll from below link with the version you want http://binaries.html-tidy.org/ Please fllow steps- Put tidyX86.dll, tidyX64.dll and tidy.dll in C:\Windows\System32 and it will start working.";
            }
        }

        public string TestWebP()
        {
            try
            {
                string cVirtualPath = string.Empty;
                if (goConfig["cssFramework"] != null)
                {
                    if (goConfig["cssFramework"].ToLower() == "bs3")
                    {
                        cVirtualPath = "/ewcommon/images/logon-bg.png";
                    }
                    else
                    {
                        cVirtualPath = "/ptn/admin/skin/images/logosquare.png";
                    }
                }

                string webpFileName = Strings.Replace(cVirtualPath, ".png", ".webp");
                var oEw = new Cms();
                oEw.InitializeVariables();

                try
                {
                    oEw.moFSHelper.DeleteFile(webpFileName);
                }
                catch
                {
                    // Ignore if file doesn't exist
                }

                short WebPQuality = 60;

                // ✅ Use SkiaSharp for both decoding AND encoding
                using (var bitmap = SKBitmap.Decode(oEw.goServer.MapPath(cVirtualPath)))
                {
                    if (bitmap == null)
                    {
                        return "Error: Could not decode image at " + cVirtualPath;
                    }

                    using (var image = SKImage.FromBitmap(bitmap))
                    using (var data = image.Encode(SKEncodedImageFormat.Webp, WebPQuality))
                    using (var saveImageStream = File.OpenWrite(oEw.goServer.MapPath(webpFileName)))
                    {
                        data.SaveTo(saveImageStream);
                    }
                }

                return "Protean Logo converted to WebP <img src='" + webpFileName + "'/>";
            }
            catch (Exception ex)
            {
                return ex.Message + "<br/>" + ex.StackTrace;
            }
        }

        public string TestCSRedist2010()
        {
            string displayName = "Microsoft Visual C++ 2010";
            try
            {

                string uninstallKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

                using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(uninstallKey))
                {
                    foreach (string skName in rk.GetSubKeyNames())
                    {
                        using (RegistryKey sk = rk.OpenSubKey(skName))
                        {
                            if (sk.GetValue("DisplayName") != null)
                            {
                                string dispname = sk.GetValue("DisplayName").ToString();
                                if (dispname.Contains(displayName))
                                {
                                    return dispname + " Installed";
                                }
                            }
                        }
                    }
                }
                return displayName + " Error Installation Required";

            }
            catch (Exception ex)
            {
                return ex.Message + "<br/>" + ex.StackTrace;
            }
        }

        public string TestReadPDF()
        {
            try
            {
                Cms oEw = new Cms();
                oEw.InitializeVariables();
                string filepath = oEw.goServer.MapPath("/ewcommon/setup/test.pdf");
                string response = "";

                response = Protean.Tools.FileHelper.GetPDFText(filepath);

                return response;
            }

            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        public string SkiaSharpDiagnostics()
        {
            var html = new System.Text.StringBuilder();

            try
            {
                html.AppendLine("<div class='skiasharp-diagnostics'>");
                html.AppendLine("<h3>SkiaSharp Diagnostics</h3>");

                // Get the executing assembly location (where your app is running from)
                string assemblyDir = AppDomain.CurrentDomain.BaseDirectory;
                html.AppendLine($"<p><strong>Application Base Directory:</strong> {assemblyDir}</p>");

                // Check if running in IIS
                bool isIIS = System.Web.HttpContext.Current != null;
                html.AppendLine($"<p><strong>Running in IIS:</strong> {isIIS}</p>");

                if (isIIS)
                {
                    html.AppendLine($"<p><strong>Physical Application Path:</strong> {System.Web.HttpContext.Current.Server.MapPath("~")}</p>");
                }

                // Check architecture
                bool is64Bit = Environment.Is64BitProcess;
                html.AppendLine($"<p><strong>Process Architecture:</strong> {(is64Bit ? "x64" : "x86")}</p>");
                html.AppendLine($"<p><strong>OS Architecture:</strong> {(Environment.Is64BitOperatingSystem ? "x64" : "x86")}</p>");

                // Define search paths for libSkiaSharp.dll
                var searchPaths = new List<string>
        {
            // Bin directory
            System.IO.Path.Combine(assemblyDir, "bin", "libSkiaSharp.dll"),
            System.IO.Path.Combine(assemblyDir, "libSkiaSharp.dll"),
            
            // Runtime-specific paths
            System.IO.Path.Combine(assemblyDir, "bin", "runtimes", is64Bit ? "win-x64" : "win-x86", "native", "libSkiaSharp.dll"),
            System.IO.Path.Combine(assemblyDir, "runtimes", is64Bit ? "win-x64" : "win-x86", "native", "libSkiaSharp.dll"),
            
            // Architecture-specific paths
            System.IO.Path.Combine(assemblyDir, "bin", is64Bit ? "x64" : "x86", "libSkiaSharp.dll"),
            System.IO.Path.Combine(assemblyDir, is64Bit ? "x64" : "x86", "libSkiaSharp.dll")
        };

                html.AppendLine("<h4>Searching for libSkiaSharp.dll:</h4>");
                html.AppendLine("<ul>");

                bool found = false;
                string foundPath = null;

                foreach (var path in searchPaths)
                {
                    bool exists = System.IO.File.Exists(path);
                    string icon = exists ? "<i class='fa fa-check text-success'></i>" : "<i class='fa fa-times text-danger'></i>";
                    html.AppendLine($"<li>{icon} {path}</li>");

                    if (exists && !found)
                    {
                        found = true;
                        foundPath = path;
                    }
                }

                html.AppendLine("</ul>");

                if (found)
                {
                    html.AppendLine($"<div class='alert alert-success'>");
                    html.AppendLine($"<strong>✓ Native library found at:</strong><br/>{foundPath}");

                    // Try to get file version info
                    try
                    {
                        var fileInfo = new System.IO.FileInfo(foundPath);
                        html.AppendLine($"<br/><strong>File Size:</strong> {fileInfo.Length:N0} bytes");
                        html.AppendLine($"<br/><strong>Last Modified:</strong> {fileInfo.LastWriteTime}");

                        var versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(foundPath);
                        if (!string.IsNullOrEmpty(versionInfo.FileVersion))
                        {
                            html.AppendLine($"<br/><strong>File Version:</strong> {versionInfo.FileVersion}");
                        }
                    }
                    catch (Exception vex)
                    {
                        html.AppendLine($"<br/><em>Could not read file details: {vex.Message}</em>");
                    }

                    html.AppendLine("</div>");

                    // Try to actually use SkiaSharp
                    try
                    {
                        html.AppendLine("<h4>Testing SkiaSharp Functionality:</h4>");

                        using (var surface = SkiaSharp.SKSurface.Create(new SkiaSharp.SKImageInfo(100, 100)))
                        {
                            if (surface != null)
                            {
                                var canvas = surface.Canvas;
                                canvas.Clear(SkiaSharp.SKColors.White);
                                canvas.DrawRect(10, 10, 80, 80, new SkiaSharp.SKPaint { Color = SkiaSharp.SKColors.Blue });

                                html.AppendLine("<div class='alert alert-success'>");
                                html.AppendLine("<i class='fa fa-check'></i> <strong>SkiaSharp is working correctly!</strong> Successfully created a surface and rendered graphics.");
                                html.AppendLine("</div>");
                            }
                        }
                    }
                    catch (Exception skex)
                    {
                        html.AppendLine("<div class='alert alert-danger'>");
                        html.AppendLine($"<i class='fa fa-exclamation-triangle'></i> <strong>SkiaSharp Error:</strong><br/>");
                        html.AppendLine($"{skex.Message}<br/>");
                        if (skex.InnerException != null)
                        {
                            html.AppendLine($"<strong>Inner Exception:</strong> {skex.InnerException.Message}");
                        }
                        html.AppendLine("</div>");
                    }
                }
                else
                {
                    html.AppendLine("<div class='alert alert-danger'>");
                    html.AppendLine("<i class='fa fa-exclamation-triangle'></i> <strong>libSkiaSharp.dll NOT FOUND</strong>");
                    html.AppendLine("<h5>Recommended Actions:</h5>");
                    html.AppendLine("<ol>");
                    html.AppendLine("<li>Ensure <code>SkiaSharp.NativeAssets.Win32</code> NuGet package is installed</li>");
                    html.AppendLine("<li>Rebuild your solution</li>");
                    html.AppendLine($"<li>Check that native assets are being copied to: <code>{System.IO.Path.Combine(assemblyDir, "bin")}</code></li>");
                    html.AppendLine("<li>Verify the correct platform target (x64/x86) is set</li>");
                    html.AppendLine("</ol>");
                    html.AppendLine("</div>");
                }

                // Check PATH environment variable
                html.AppendLine("<h4>Checking System PATH:</h4>");
                var pathDirs = Environment.GetEnvironmentVariable("PATH")?.Split(';');
                bool foundInPath = false;

                if (pathDirs != null)
                {
                    foreach (var dir in pathDirs)
                    {
                        if (!string.IsNullOrWhiteSpace(dir))
                        {
                            var dllPath = System.IO.Path.Combine(dir.Trim(), "libSkiaSharp.dll");
                            if (System.IO.File.Exists(dllPath))
                            {
                                html.AppendLine($"<p><i class='fa fa-check text-success'></i> Found in PATH: {dllPath}</p>");
                                foundInPath = true;
                            }
                        }
                    }
                }

                if (!foundInPath)
                {
                    html.AppendLine("<p><em>libSkiaSharp.dll not found in system PATH (this is normal)</em></p>");
                }

                // Check loaded assemblies
                html.AppendLine("<h4>Loaded SkiaSharp Assemblies:</h4>");
                html.AppendLine("<ul>");
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.FullName.Contains("SkiaSharp"))
                    {
                        html.AppendLine($"<li><strong>{assembly.GetName().Name}</strong> v{assembly.GetName().Version} <br/><small>{assembly.Location}</small></li>");
                    }
                }
                html.AppendLine("</ul>");

                html.AppendLine("</div>");

                return html.ToString();
            }
            catch (Exception ex)
            {
                html.Clear();
                html.AppendLine("<div class='alert alert-danger'>");
                html.AppendLine("<h3>SkiaSharp Diagnostics Error</h3>");
                html.AppendLine($"<p><strong>Error:</strong> {ex.Message}</p>");
                html.AppendLine($"<pre>{ex.StackTrace}</pre>");
                html.AppendLine("</div>");
                return html.ToString();
            }
        }

    }
}