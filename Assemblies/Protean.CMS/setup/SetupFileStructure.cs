
using System;
using System.IO;
using System.Web.Configuration;



namespace Protean
{
    public partial class Setup : IDisposable
    {
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

            public System.Web.HttpServerUtility goServer;

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
    }
}