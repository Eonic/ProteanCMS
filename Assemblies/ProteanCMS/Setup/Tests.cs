using System;
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
                emailerMsg = oMsg.emailer(bodyXml, emailXSL, "ProteanCMS Test", goConfig["SiteAdminEmail"], goConfig["SiteAdminEmail"], "This is a TEST", odbHelper: ref argodbHelper).ToString;

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

                var oEw = new Cms();
                oEw.InitializeVariables();
                string filepath = "/ewCache/";
                string filename = "FS-TEST.html";
                Tools.Security.Impersonate oImp = null;
                if (oEw.moConfig("AdminAcct") != "")
                {
                    oImp = new Tools.Security.Impersonate();
                    if (oImp.ImpersonateValidUser(oEw.moConfig("AdminAcct"), oEw.moConfig("AdminDomain"), oEw.moConfig("AdminPassword"), cInGroup: oEw.moConfig("AdminGroup")))
                    {
                    }
                    else
                    {
                        return "Impersonation Failed";
                    }
                }

                oEw.moFSHelper.SaveFile(filename, oEw.goServer.MapPath("/" + oEw.gcProjectPath) + filepath, System.Text.Encoding.Unicode.GetBytes(htmltotest));

                return "File Written Using SaveFile :" + filepath + filename;

                if (oEw.moConfig("AdminAcct") != "")
                {
                    oImp.UndoImpersonation();
                    oImp = null;
                }
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
                if (oEw.moConfig("AdminAcct") != "")
                {
                    oImp = new Tools.Security.Impersonate();
                    if (oImp.ImpersonateValidUser(oEw.moConfig("AdminAcct"), oEw.moConfig("AdminDomain"), oEw.moConfig("AdminPassword"), cInGroup: oEw.moConfig("AdminGroup")))
                    {
                    }
                    else
                    {
                        return "Impersonation Failed";
                    }
                }
                string response;

                response = oEw.moFSHelper.DeleteFile(oEw.goServer.MapPath("/" + oEw.gcProjectPath) + filepath, filename);

                oEw.moFSHelper.DeleteFile(oEw.goServer.MapPath("/" + oEw.gcProjectPath) + filepath, "FS-Alpha-TEST.html");

                if (oEw.moConfig("AdminAcct") != "")
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
            try
            {

                var oEw = new Cms();
                oEw.InitializeVariables();

                Tools.Security.Impersonate oImp = null;
                if (oEw.moConfig("AdminAcct") != "")
                {
                    oImp = new Tools.Security.Impersonate();
                    if (oImp.ImpersonateValidUser(oEw.moConfig("AdminAcct"), oEw.moConfig("AdminDomain"), oEw.moConfig("AdminPassword"), cInGroup: oEw.moConfig("AdminGroup")))
                    {
                        return "Impersonation Success";
                    }
                    else
                    {
                        return "Error Impersonation Failed";
                    }
                }
                else
                {
                    return "Impersonation Disabled";
                }

                if (oEw.moConfig("AdminAcct") != "")
                {
                    oImp.UndoImpersonation();
                    oImp = null;
                }
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
                if (oEw.moConfig("AdminAcct") != "")
                {
                    oImp = new Tools.Security.Impersonate();
                    if (oImp.ImpersonateValidUser(oEw.moConfig("AdminAcct"), oEw.moConfig("AdminDomain"), oEw.moConfig("AdminPassword"), cInGroup: oEw.moConfig("AdminGroup")))
                    {
                    }
                    else
                    {
                        return "Impersonation Failed";
                    }
                }

                Alphaleonis.Win32.Filesystem.File.WriteAllText(@"\\?\" + oEw.goServer.MapPath("/" + oEw.gcProjectPath) + filepath, htmltotest, System.Text.Encoding.UTF8);

                return "File Written Using AlphaFS :" + filepath;

                if (oEw.moConfig("AdminAcct") != "")
                {
                    oImp.UndoImpersonation();
                    oImp = null;
                }
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

                string sResponse = Tools.Text.tidyXhtmlFrag(htmltotest, true, true, "");

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

    }
}