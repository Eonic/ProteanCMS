using Microsoft.Ajax.Utilities;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Protean.Providers.Payment;
using Protean.Tools;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Policy;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.SessionState;
using System.Xml;
using static Protean.Cms;
using static Protean.stdTools;
using static Protean.Tools.Xml;


namespace Protean.Providers
{
    namespace Authentication
    {
        public interface IauthenticaitonProvider
        {
            IauthenticaitonProvider Initiate(Cms myWeb, NameValueCollection config);
            string GetAuthenticationURL(string ProviderName);
            string ExtractEmail(XmlDocument xmlDoc);
            string ExtractIssuer(XmlDocument xmlDoc);
            //long CheckAuthenticationResponse(HttpRequest request, HttpSessionState session, HttpResponse response); // returns userid

            System.Collections.Specialized.NameValueCollection config
            { get; }

            string name { get; }

        }

        public class ReturnProvider
        {
            private const string mcModuleName = "Protean.Providers.Authentication.GetProvider";
          
            public IEnumerable<IauthenticaitonProvider> Get(ref Cms myWeb)
            {
                try
                {

                                      
                    Type calledType;
                    string ProviderClass = "";
                    Protean.ProviderSectionHandler moPrvConfig = (Protean.ProviderSectionHandler)WebConfigurationManager.GetWebApplicationSection("protean/authenticationProviders");

                    ICollection<IauthenticaitonProvider> providerList = new IauthenticaitonProvider[0];
                    var modifiable = providerList.ToList();

                    //if (moPrvConfig != null)
                    //{
                    //    foreach (System.Configuration.ProviderSettings authProvider in moPrvConfig.Providers) { 

                    //        ProviderClass = Convert.ToString(authProvider.Name);

                    //        if (string.IsNullOrEmpty(ProviderClass))
                    //        {
                    //            ProviderClass = "Protean.Providers.Authentication.DefaultProvider";
                    //            calledType = Type.GetType(ProviderClass, true);
                    //        }
                    //        else
                    //        {
                    //            if (authProvider.Type != "")
                    //            {
                    //                var assemblyInstance = Assembly.Load(authProvider.Type);
                    //                calledType = assemblyInstance.GetType("Protean.Providers.Authentication." + ProviderClass, true);
                    //            }
                    //            else
                    //            {
                    //                calledType = Type.GetType("Protean.Providers.Authentication." + ProviderClass, true);
                    //            }
                    //        }

                    //        var o = Activator.CreateInstance(calledType);
                    //        var args = new object[2];
                    //        args[0] = myWeb;
                    //        args[1] = authProvider.Parameters;

                    //        modifiable.Add((IauthenticaitonProvider)calledType.InvokeMember("Initiate", BindingFlags.InvokeMethod, null, o, args));

                    //    }

                    //    providerList = modifiable;
                    //    return providerList;

                    //}else
                    //{
                    //    return null;
                    //}


                    if (moPrvConfig != null)
                    {
                        foreach (ProviderSettings authProvider in moPrvConfig.Providers)
                        {
                            string providerTypeName = authProvider.Type;
                            if (string.IsNullOrEmpty(providerTypeName))
                            {                                
                                providerTypeName = "Protean.Providers.Authentication.DefaultProvider, ProteanCMS";
                            }

                            try
                            {
                                // Load the full type (with namespace + assembly)
                                calledType = Type.GetType(providerTypeName, throwOnError: true);

                                object instance = Activator.CreateInstance(calledType);

                                var configParams = new NameValueCollection(authProvider.Parameters);
                                configParams["name"] = authProvider.Name;

                                var args = new object[2];
                                args[0] = myWeb;
                                args[1] = configParams;

                                var provider = (IauthenticaitonProvider)calledType.InvokeMember("Initiate", BindingFlags.InvokeMethod, null, instance, args);

                                modifiable.Add(provider);
                            }
                            catch (Exception ex)
                            {
                                // Log each individual provider error, skip to next
                                Console.WriteLine($"[!] Failed to load provider '{authProvider.Name}': {ex.Message}");
                            }
                        }
                        return modifiable;
                    }
                    else
                    {
                        return null;
                    }

                }
                catch (Exception ex)
                {
                    // TS commented this out as if we have an old payment provider that has been retired we do not want errors.
                    //stdTools.returnException(ref myWeb.msException, mcModuleName, "New", ex, "", ProviderName + " Could Not be Loaded", gbDebug);
                    return null;
                }
            }

        }
        public class Default : IauthenticaitonProvider
        {
            private string _Name = "Default";
            public Protean.Cms _myWeb;
            System.Collections.Specialized.NameValueCollection moCartConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/cart");

            private NameValueCollection _Config;
            public Default()
            {
                // do nothing
            }

            string IauthenticaitonProvider.name
            {
                get
                {
                    return GetType().Name; ;
                }
            }

            public NameValueCollection config
            {
                get
                {
                    return _Config; 
                }
            }
          
            public IauthenticaitonProvider Initiate(Cms myWeb, NameValueCollection config)
            {
                _myWeb = myWeb;
                _Config = config;
                return this;
            }           

            public string GetAuthenticationURL(string ProviderName)
            {
                string gcEwBaseUrl = moCartConfig["SecureURL"].TrimEnd('/');
                return GetSamlLoginUrl(config["ssoUrl"].ToString(), "ProteanCMS", gcEwBaseUrl + _myWeb.mcOriginalURL, ProviderName);
            }
            //check ACS URL in google account- here need to pass exactly same
            // issuer = Entity ID in google account
            public static string GetSamlLoginUrl(string idpSsoUrl, string issuer, string assertionConsumerServiceUrl, string ProviderName)
            {
                var authRequest = GenerateSamlRequestXml(issuer, assertionConsumerServiceUrl);

                var compressedRequest = CompressAndEncode(authRequest);
                var samlRequest = HttpUtility.UrlEncode(compressedRequest);
                var returnUrl = string.Empty;
                if(ProviderName.ToLower() == "google")
                {
                    returnUrl = $"{idpSsoUrl}&SAMLRequest={samlRequest}&RelayState={ProviderName}";
                }
                else
                {
                    returnUrl = $"{idpSsoUrl}?SAMLRequest={samlRequest}&RelayState={ProviderName}";
                }
                return returnUrl;
            }

            private static string GenerateSamlRequestXml(string issuer, string assertionConsumerServiceUrl)
            {
                var id = "_" + Guid.NewGuid().ToString("N");
                var issueInstant = DateTime.UtcNow.ToString("o");

                return $@"
<samlp:AuthnRequest xmlns:samlp='urn:oasis:names:tc:SAML:2.0:protocol' 
    ID='{id}' Version='2.0' IssueInstant='{issueInstant}' 
    AssertionConsumerServiceURL='{assertionConsumerServiceUrl}' 
    ProtocolBinding='urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST'>
    <saml:Issuer xmlns:saml='urn:oasis:names:tc:SAML:2.0:assertion'>{issuer}</saml:Issuer>
</samlp:AuthnRequest>";
            }

            private static string CompressAndEncode(string input)
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                using (var output = new MemoryStream())
                {
                    using (var compress = new DeflateStream(output, CompressionMode.Compress, true))
                    {
                        compress.Write(bytes, 0, bytes.Length);
                    }
                    return Convert.ToBase64String(output.ToArray());
                }
            }

            protected XmlDocument ParseSaml(string base64Saml)
            {
                byte[] bytes = Convert.FromBase64String(base64Saml);
                string xml = Encoding.UTF8.GetString(bytes);

                XmlDocument doc = new XmlDocument();
                doc.PreserveWhitespace = true;
                doc.LoadXml(xml);

                return doc;
            }

            public string ExtractIssuer(XmlDocument xmlDoc)
            {
                XmlNode issuerNode = xmlDoc.SelectSingleNode(
                    "//*[local-name()='Issuer' and namespace-uri()='urn:oasis:names:tc:SAML:2.0:assertion']");

                return issuerNode?.InnerText ?? string.Empty;
            }

            public string ExtractEmail(XmlDocument xmlDoc)
            {
                XmlNodeList attributes = xmlDoc.SelectNodes(
                    "//*[local-name()='Attribute' and namespace-uri()='urn:oasis:names:tc:SAML:2.0:assertion']");

                foreach (XmlNode attr in attributes)
                {
                    string name = attr.Attributes["Name"]?.Value;
                    if (!string.IsNullOrEmpty(name) && name.ToLower().Contains("email"))
                    {
                        XmlNode val = attr.SelectSingleNode("*[local-name()='AttributeValue']");
                        return val?.InnerText.Trim();
                    }
                }
                return string.Empty;
            }

            //public long ValidateUser(string samlUserEmail)
            //{
            //    long userid = 0;
            //    if (!string.IsNullOrEmpty(samlUserEmail))
            //    {
            //        string sSql = "select d.*, a.* from tblDirectory d inner join tblAudit a on a.nAuditkey = nAuditId where " + "cDirSchema = 'User' and cDirName = '" + SqlFmt(samlUserEmail) + "'";
            //        DataSet dsUsers = _myWeb.moDbHelper.GetDataSet(sSql, "tblTemp");
            //        int nNumberOfUsers = dsUsers.Tables[0].Rows.Count;

            //        if (nNumberOfUsers == 0)
            //        {
            //            userid = 0;
            //        }
            //        else
            //        {
            //            DataRow oUserDetails = dsUsers.Tables[0].Rows[0];
            //            userid = Conversions.ToLong(oUserDetails["nDirKey"]);                
            //        }
            //    }
            //    return userid;
            //}

        }
    }
}

