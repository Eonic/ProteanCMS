﻿using Imazen.WebP;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO;
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
                string newFilepath = string.Empty;
                var oEw = new Cms();
                oEw.InitializeVariables();
                try
                {
                    oEw.moFSHelper.DeleteFile(webpFileName);
                }
                catch
                {
                };
                short WebPQuality = 60;
                using (var bitMap = new Bitmap(oEw.goServer.MapPath(cVirtualPath)))
                {
                    using (var saveImageStream = File.Open(oEw.goServer.MapPath(webpFileName), FileMode.Create))
                    {
                        var encoder = new SimpleEncoder();
                        encoder.Encode(bitMap, saveImageStream, WebPQuality);
                        encoder = null;
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


    }
}