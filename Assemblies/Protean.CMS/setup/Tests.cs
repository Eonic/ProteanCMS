using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using System.Xml;
using System.Web.Configuration;
using System.Configuration;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
//using Microsoft.ClearScript.Util;
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

            Protean.Messaging oMsg = new Protean.Messaging(errMsg);

            XmlDocument oBodyXml = new XmlDocument();
            oBodyXml.LoadXml("<Items><Name>ProteanCMS Test</Name><Telephone /><Email>" + goConfig["SiteAdminEmail"] + "</Email><Message>This is a test</Message></Items>");
            string emailerMsg;
            XmlElement bodyXml = (XmlElement)oBodyXml.FirstChild;
            string emailXSL = "/ptn/email/mailform.xsl";
            if (goConfig["cssFramework"] != "bs5")
                emailXSL = "/ewcommon/xsl/email/mailform.xsl";

            emailerMsg = System.Convert.ToString(oMsg.emailer(bodyXml, emailXSL, "ProteanCMS Test", goConfig["SiteAdminEmail"], goConfig["SiteAdminEmail"], "This is a TEST").ToString);

            if (emailerMsg != "")
                return Protean.Tools.Text.EscapeJS(emailerMsg);
            else
                return "1";
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
            Protean.Tools.Security.Impersonate oImp = null/* TODO Change to default(_) if this is not a reference type */;
            if (oEw.moConfig("AdminAcct") != "")
            {
                oImp = new Protean.Tools.Security.Impersonate();
                if (oImp.ImpersonateValidUser(oEw.moConfig("AdminAcct"), oEw.moConfig("AdminDomain"), oEw.moConfig("AdminPassword"), null/* Conversion error: Set to default value for this argument */, oEw.moConfig("AdminGroup")))
                {
                }
                else
                    return "Impersonation Failed";
            }

            oEw.moFSHelper.SaveFile(filename, oEw.goServer.MapPath("/" + oEw.gcProjectPath) + filepath, System.Text.Encoding.Unicode.GetBytes(htmltotest));

            return "File Written Using SaveFile :" + filepath + filename;

            if (oEw.moConfig("AdminAcct") != "")
            {
                oImp.UndoImpersonation();
                oImp = null/* TODO Change to default(_) if this is not a reference type */;
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
            Protean.Cms oEw = new Protean.Cms();
            oEw.InitializeVariables();
            string filepath = @"\ewCache\";
            string filename = "FS-TEST.html";
            Protean.Tools.Security.Impersonate oImp = null/* TODO Change to default(_) if this is not a reference type */;
            if (oEw.moConfig("AdminAcct") != "")
            {
                oImp = new Protean.Tools.Security.Impersonate();
                if (oImp.ImpersonateValidUser(oEw.moConfig("AdminAcct"), oEw.moConfig("AdminDomain"), oEw.moConfig("AdminPassword"), null/* Conversion error: Set to default value for this argument */, oEw.moConfig("AdminGroup")))
                {
                }
                else
                    return "Impersonation Failed";
            }
            string response;

            response = oEw.moFSHelper.DeleteFile(oEw.goServer.MapPath("/" + oEw.gcProjectPath) + filepath, filename);

            oEw.moFSHelper.DeleteFile(oEw.goServer.MapPath("/" + oEw.gcProjectPath) + filepath, "FS-Alpha-TEST.html");

            if (oEw.moConfig("AdminAcct") != "")
            {
                oImp.UndoImpersonation();
                oImp = null/* TODO Change to default(_) if this is not a reference type */;
            }

            if (response == "1")
                return "File Deleted:" + filepath + filename;
            else
                return response;
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
            Protean.Cms oEw = new Protean.Cms();
            oEw.InitializeVariables();

            Protean.Tools.Security.Impersonate oImp = null/* TODO Change to default(_) if this is not a reference type */;
            if (oEw.moConfig("AdminAcct") != "")
            {
                oImp = new Protean.Tools.Security.Impersonate();
                if (oImp.ImpersonateValidUser(oEw.moConfig("AdminAcct"), oEw.moConfig("AdminDomain"), oEw.moConfig("AdminPassword"), null/* Conversion error: Set to default value for this argument */, oEw.moConfig("AdminGroup")))
                    return "Impersonation Success";
                else
                    return "Error Impersonation Failed";
            }
            else
                return "Impersonation Disabled";

            if (oEw.moConfig("AdminAcct") != "")
            {
                oImp.UndoImpersonation();
                oImp = null/* TODO Change to default(_) if this is not a reference type */;
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

            Protean.Cms oEw = new Protean.Cms();
            oEw.InitializeVariables();
            string filepath = "/ewCache/FS-Alpha-TEST.html";
            Protean.Tools.Security.Impersonate oImp = null/* TODO Change to default(_) if this is not a reference type */;
            if (oEw.moConfig("AdminAcct") != "")
            {
                oImp = new Protean.Tools.Security.Impersonate();
                if (oImp.ImpersonateValidUser(oEw.moConfig("AdminAcct"), oEw.moConfig("AdminDomain"), oEw.moConfig("AdminPassword"), null/* Conversion error: Set to default value for this argument */, oEw.moConfig("AdminGroup")))
                {
                }
                else
                    return "Impersonation Failed";
            }

            Alphaleonis.Win32.Filesystem.File.WriteAllText(@"\\?\" + oEw.goServer.MapPath("/" + oEw.gcProjectPath) + filepath, htmltotest, System.Text.Encoding.UTF8);

            return "File Written Using AlphaFS :" + filepath;

            if (oEw.moConfig("AdminAcct") != "")
            {
                oImp.UndoImpersonation();
                oImp = null/* TODO Change to default(_) if this is not a reference type */;
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
            Protean.Cms oEw = new Protean.Cms();
            oEw.InitializeVariables();
            string filepath = "/ewCache/Testfolder/test2";

            string response = oEw.moFSHelper.CreatePath(filepath);
            if (response == "1")
                return "Folder Created:" + filepath;
            else
                return response;
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
            Protean.Cms oEw = new Protean.Cms();
            oEw.InitializeVariables();
            string filepath = "/ewCache/Testfolder/test2";

            string response = oEw.moFSHelper.DeleteFolder("Testfolder", oEw.goServer.MapPath("/ewCache/"));
            if (response == "1")
                return "Folder Deleted:" + filepath;
            else
                return response;
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

            string sResponse = Protean.Tools.Text.tidyXhtmlFrag(htmltotest, true, true, "");

            if (sResponse.StartsWith("<h1>HTMLTidy is Tidying</h1>"))
                sResponse = "HTML Tidy is working";

            return sResponse;
        }


        catch (Exception ex)
        {
            return ex.Message + @"<br/>If server is 64 bit and failed to load tidy.dll reference and broke functionality. Get dll from below link with the version you want http://binaries.html-tidy.org/ Please fllow steps- Put tidyX86.dll, tidyX64.dll and tidy.dll in C:\Windows\System32 and it will start working.";
        }
    }
}


