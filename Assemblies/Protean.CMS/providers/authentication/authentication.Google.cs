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
using System.Text.RegularExpressions;
using System.Web.SessionState;
using System.Web;
using static Protean.Cms;
using System.Net.PeerToPeer;
using static QRCoder.PayloadGenerator;
using System.Data;


namespace Protean.Providers
{
    namespace Authentication
    {
     
        public class Google : Authentication.Default, IauthenticaitonProvider
        {
             
            System.Collections.Specialized.NameValueCollection moCartConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/cart");
            public Google()
            {
                // do nothing
            }


            public IauthenticaitonProvider Initiate(ref Cms myWeb)
            {               
                return this;
            }

            public new string GetAuthenticationURL(string ProviderName)
            {
                string gcEwBaseUrl = moCartConfig["SecureURL"].TrimEnd('/');
                return GetSamlLoginUrl(base.config["ssoUrl"].ToString(), "ProteanCMS", gcEwBaseUrl + _myWeb.mcOriginalURL, ProviderName);
            }

            public new long CheckAuthenticationResponse(HttpRequest request, HttpSessionState session, HttpResponse response)
            {
                XmlDocument xmlDoc = ParseSaml(request["SAMLResponse"]);
                string issuer = ExtractIssuer(xmlDoc);
                if (issuer.Contains("accounts.google.com"))
                {
                    string samlUserEmail = ExtractEmail(xmlDoc);
                    return ValidateUser(samlUserEmail);
                }
                else {
                    return 0;
                }  
                
            }

            
        }
    }
}
