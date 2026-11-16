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
using Microsoft.Ajax.Utilities;
using System.Configuration.Provider;
using System.Xml;
using System.Web.Security;
using System.Web.SessionState;
using System.Web;
using System.Data;


namespace Protean.Providers
{
    namespace Authentication
    {
     
        public class Microsoft : Authentication.Default, IauthenticaitonProvider
        {            
            public Microsoft()
            {
                // do nothing
            }


            public IauthenticaitonProvider Initiate(ref Cms myWeb)
            {
                return this;
            }

            // Keys are shared by all tenants
            protected override string DefaultJwksUri
            {
                get { return "https://login.microsoftonline.com/common/discovery/v2.0/keys"; }
            }

            // Tenant specific issuer: https://login.microsoftonline.com/{tenantId}/v2.0
            protected override bool IsValidDefaultIssuer(string issuer)
            {
                return !string.IsNullOrEmpty(issuer) &&
                    System.Text.RegularExpressions.Regex.IsMatch(issuer, @"^https://login\.microsoftonline\.com/[0-9a-fA-F-]{36}/v2\.0$");
            }

            // Tenant id Microsoft uses for all personal (consumer) Microsoft accounts
            private const string ConsumerTenantId = "9188040d-6c67-4c5b-b112-36a304b66dad";

            /// <summary>
            /// Microsoft ID tokens have no email_verified claim, so trust is decided as follows:
            /// 1. xms_edov = true (optional claim, Microsoft verified the domain owner) - always trusted.
            /// 2. Personal Microsoft accounts (tid = consumer tenant) - Microsoft verifies the address, trusted unless trustConsumerEmail="false".
            /// 3. Single named tenant (config tenant is a tenant id/domain) - trustUnverifiedEmail.
            /// 4. Any other work/school account (multi-tenant) - only if trustWorkEmail="true". A tenant admin can set any email
            ///    on their own users, so enable this only if xms_edov cannot be configured.
            /// </summary>
            protected override bool IsEmailVerifiedFallback(JObject claims)
            {
                JToken edov = claims["xms_edov"];
                if (edov != null && (edov.Type == JTokenType.Boolean ? (bool)edov : string.Equals((string)edov, "true", StringComparison.OrdinalIgnoreCase) || (string)edov == "1"))
                {
                    return true;
                }

                string tid = (string)claims["tid"];
                if (string.Equals(tid, ConsumerTenantId, StringComparison.OrdinalIgnoreCase))
                {
                    return !string.Equals(base.config["trustConsumerEmail"], "false", StringComparison.OrdinalIgnoreCase);
                }

                string tenant = (base.config["tenant"] ?? "common").ToLower();
                bool multiTenant = tenant == "common" || tenant == "organizations" || tenant == "consumers";
                if (!multiTenant)
                {
                    return base.IsEmailVerifiedFallback(claims);
                }

                return string.Equals(base.config["trustWorkEmail"], "true", StringComparison.OrdinalIgnoreCase);
            }

            public XmlElement GetLogonBtn(ref XmlElement oContextNode, string mode = "signin")
            {
                XmlElement thisButton = base.GetLogonBtn(ref oContextNode, mode);

                thisButton.SetAttribute("data-clientid", base.config["clientId"]);
                thisButton.SetAttribute("data-authority", "https://login.microsoftonline.com/" + (base.config["tenant"] ?? "common"));

                // Must exactly match a SPA redirect URI registered in the Entra app
                string redirectUri = base.config["redirectUri"];
                if (string.IsNullOrEmpty(redirectUri))
                {
                    redirectUri = "https://" + _myWeb.moRequest.ServerVariables["HTTP_HOST"] + "/";
                }
                thisButton.SetAttribute("data-redirecturi", redirectUri);

                return thisButton;
            }
            //public new string GetAuthenticationURL(string ProviderName)
            //{
            //    string gcEwBaseUrl = moCartConfig["SecureURL"].TrimEnd('/');
            //    return GetSamlLoginUrl(base.config["ssoUrl"].ToString(), "ProteanCMS", gcEwBaseUrl + _myWeb.mcOriginalURL, ProviderName);
            //}

            //public new long CheckAuthenticationResponse(HttpRequest request, HttpSessionState session, HttpResponse response)
            //{
            //    XmlDocument xmlDoc = ParseSaml(request["SAMLResponse"]);
            //    string issuer = ExtractIssuer(xmlDoc);
            //    if (issuer.Contains("sts.windows.net"))
            //    {
            //        string email = ExtractEmail(xmlDoc);
            //        return ValidateUser(email);
            //    }
            //    else
            //    {
            //        return 0;
            //    }  
            //}
            
        }
    }
}
