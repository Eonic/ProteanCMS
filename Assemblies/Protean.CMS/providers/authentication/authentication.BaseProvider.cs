using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Web.Configuration;
using Newtonsoft.Json.Linq;
using System.Security.Authentication;
using Protean.Providers.Payment;
using System.Reflection;
using static Protean.stdTools;
using Protean.Tools;
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.Ajax.Utilities;
using System.Configuration.Provider;
using Microsoft.VisualBasic;
using System.Xml;
using System.Web.Security;
using System.IO.Compression;
using System.IO;
using System.Web;


namespace Protean.Providers
{
    namespace Authentication
    {
        public interface IauthenticaitonProvider
        {
            IauthenticaitonProvider Initiate(ref Cms myWeb);

            string GetAuthenticationURL();

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

                    if (moPrvConfig != null)
                    {
                        foreach (System.Configuration.ProviderSettings authProvider in moPrvConfig.Providers) { 
                        
                            ProviderClass = Convert.ToString(authProvider.Name);

                            if (string.IsNullOrEmpty(ProviderClass))
                            {
                                ProviderClass = "Protean.Providers.Authentication.DefaultProvider";
                                calledType = Type.GetType(ProviderClass, true);
                            }
                            else
                            {
                                if (authProvider.Type != "")
                                {
                                    var assemblyInstance = Assembly.Load(authProvider.Type);
                                    calledType = assemblyInstance.GetType("Protean.Providers.Authentication." + ProviderClass, true);
                                }
                                else
                                {
                                    calledType = Type.GetType("Protean.Providers.Authentication." + ProviderClass, true);
                                }
                            }

                            var o = Activator.CreateInstance(calledType);
                            var args = new object[2];
                            args[0] = myWeb;
                            args[1] = authProvider.Parameters;

                            modifiable.Add((IauthenticaitonProvider)calledType.InvokeMember("Initiate", BindingFlags.InvokeMethod, null, o, args));

                        }

                        providerList = modifiable;
                        return providerList;

                    }else
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

            public string GetAuthenticationURL() {

                return "";
            }

            public IauthenticaitonProvider Initiate(ref Cms myWeb, ref NameValueCollection config)
            {
                _myWeb = myWeb;
                _Config = config;
                return this;
            }

            IauthenticaitonProvider IauthenticaitonProvider.Initiate(ref Cms myWeb)
            {
                throw new NotImplementedException();
            }

            //check ACS URL in google account- here need to pass exactly same
            // issuer = Entity ID in google account
            public static string GetSamlLoginUrl(string idpSsoUrl, string issuer, string assertionConsumerServiceUrl)
            {
                var authRequest = GenerateSamlRequestXml(issuer, assertionConsumerServiceUrl);

                var compressedRequest = CompressAndEncode(authRequest);
                var samlRequest = HttpUtility.UrlEncode(compressedRequest);

                return $"{idpSsoUrl}&SAMLRequest={samlRequest}";
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
        }
    }
}

