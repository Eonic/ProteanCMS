using Microsoft.Ajax.Utilities;
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
using System.Text.RegularExpressions;
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
                    Protean.ProviderSectionHandler moPrvConfig = (Protean.ProviderSectionHandler)WebConfigurationManager.GetWebApplicationSection("protean/authentication");

                    ICollection<IauthenticaitonProvider> providerList = new IauthenticaitonProvider[0];
                    var modifiable = providerList.ToList();

                   

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

            /// <summary>
            /// Gets the authentication type from config (SAML or OAuth2)
            /// </summary>
            private string AuthenticationType
            {
                get
                {
                    string authType = config["method"];
                    if (string.IsNullOrEmpty(authType))
                    {
                        authType = "SAML"; // Default to SAML for backward compatibility
                    }
                    return authType.ToUpper();
                }
            }

            public IauthenticaitonProvider Initiate(Cms myWeb, NameValueCollection config)
            {
                _myWeb = myWeb;
                _Config = config;
                return this;
            }

            /// <summary>
            /// Gets authentication URL based on configured type (SAML or OAuth2)
            /// </summary>
            public string GetAuthenticationURL(string ProviderName)
            {
                if (AuthenticationType == "OAUTH2")
                {
                    return GetOAuth2AuthenticationURL(ProviderName);
                }
                else
                {
                    return GetSamlAuthenticationURL(ProviderName);
                }
            }

            /// <summary>
            /// Gets SAML authentication URL
            /// </summary>
            private string GetSamlAuthenticationURL(string ProviderName)
            {
                string gcEwBaseUrl = "https://" + _myWeb.moRequest.ServerVariables["HTTP_HOST"];
                // NOTE: This value must match the exact Application Identifier (Entity ID) configured in the Microsoft/Google SAML app.
                string appId = "ProteanCMS";
                if (Convert.ToString(config["AppID"]) != "" && config["AppID"] != null && !string.IsNullOrWhiteSpace(config["AppID"].ToString()))
                {
                    appId = Convert.ToString(config["AppID"]);
                }
                //Note: For Google or Microsoft, ACS URL is the URL where SAML response is posted- here we are using the same URL as the original request URL
                // like :https://local.intotheblue.co.uk/?ewCmd=admin
                string keyUrl = string.Empty;
                if (!string.IsNullOrEmpty(_myWeb.moRequest.QueryString["userkey"]))
                {
                    keyUrl = "|" + _myWeb.moRequest.QueryString["userkey"];
                    _myWeb.mcOriginalURL = Regex.Replace(_myWeb.mcOriginalURL, @"(&|\?)(userkey|LogOff)=[^&]*", "");
                }
                return GetSamlLoginUrl(config["ssoUrl"].ToString(), appId, gcEwBaseUrl + _myWeb.mcOriginalURL, ProviderName, keyUrl);
            }

            /// <summary>
            /// Gets OAuth2 authentication URL
            /// </summary>
            private string GetOAuth2AuthenticationURL(string ProviderName)
            {
                string authorizationEndpoint = config["authorizationEndpoint"];
                string clientId = config["clientId"];
                string scope = config["scope"] ?? "openid profile email";
                string responseType = config["responseType"] ?? "code";

                string gcEwBaseUrl = "https://" + _myWeb.moRequest.ServerVariables["HTTP_HOST"];
                string redirectUri = gcEwBaseUrl + (_myWeb.moConfig["ProjectPath"] ?? "") + "/oauth2callback";

                if (!string.IsNullOrEmpty(config["redirectUri"]))
                {
                    redirectUri = config["redirectUri"];
                }

                // Generate state parameter for CSRF protection
                string state = GenerateState(ProviderName);

                // Store state in session for validation
                if (_myWeb.moSession != null)
                {
                    _myWeb.moSession["OAuth2_State"] = state;
                    _myWeb.moSession["OAuth2_Provider"] = ProviderName;
                }

                var parameters = new List<string>
                {
                    $"client_id={HttpUtility.UrlEncode(clientId)}",
                    $"redirect_uri={HttpUtility.UrlEncode(redirectUri)}",
                    $"response_type={responseType}",
                    $"scope={HttpUtility.UrlEncode(scope)}",
                    $"state={HttpUtility.UrlEncode(state)}"
                };

                // Add optional parameters
                if (!string.IsNullOrEmpty(config["prompt"]))
                {
                    parameters.Add($"prompt={HttpUtility.UrlEncode(config["prompt"])}");
                }

                if (!string.IsNullOrEmpty(config["accessType"]))
                {
                    parameters.Add($"access_type={HttpUtility.UrlEncode(config["accessType"])}");
                }

                string queryString = string.Join("&", parameters);
                return $"{authorizationEndpoint}?{queryString}";
            }

            //check ACS URL in google account- here need to pass exactly same
            // issuer = Entity ID in google account
            public static string GetSamlLoginUrl(string idpSsoUrl, string issuer, string assertionConsumerServiceUrl, string ProviderName, string keyUrl)
            {
                var authRequest = GenerateSamlRequestXml(issuer, assertionConsumerServiceUrl);

                var compressedRequest = CompressAndEncode(authRequest);
                var samlRequest = HttpUtility.UrlEncode(compressedRequest);
                var returnUrl = string.Empty;
                if (ProviderName.ToLower() == "google")
                {
                    returnUrl = $"{idpSsoUrl}&SAMLRequest={samlRequest}&RelayState={ProviderName + keyUrl}";
                }
                else
                {
                    returnUrl = $"{idpSsoUrl}?SAMLRequest={samlRequest}&RelayState={ProviderName + keyUrl}";
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
                // For SAML authentication
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

            /// <summary>
            /// Extracts email from OAuth2 JSON response
            /// </summary>
            public string ExtractEmailFromJson(JObject userInfo)
            {
                // Try common email claim names
                var emailClaims = new[] { "email", "mail", "emailAddress", "upn", "preferred_username" };

                foreach (var claim in emailClaims)
                {
                    if (userInfo[claim] != null)
                    {
                        return userInfo[claim].ToString();
                    }
                }

                return string.Empty;
            }

            /// <summary>
            /// Extracts issuer from OAuth2 JSON response
            /// </summary>
            public string ExtractIssuerFromJson(JObject userInfo)
            {
                if (userInfo["iss"] != null)
                {
                    return userInfo["iss"].ToString();
                }

                // Fallback to provider name from config
                return config["name"] ?? "OAuth2";
            }

            /// <summary>
            /// Exchanges authorization code for access token (OAuth2)
            /// </summary>
            public async Task<OAuth2TokenResponse> ExchangeCodeForTokenAsync(string code, string state)
            {
                // Validate state
                if (_myWeb.moSession != null)
                {
                    string storedState = _myWeb.moSession["OAuth2_State"] as string;
                    if (storedState != state)
                    {
                        throw new AuthenticationException("Invalid state parameter - possible CSRF attack");
                    }
                }

                string tokenEndpoint = config["tokenEndpoint"];
                string clientId = config["clientId"];
                string clientSecret = config["clientSecret"];

                string gcEwBaseUrl = "https://" + _myWeb.moRequest.ServerVariables["HTTP_HOST"];
                string redirectUri = gcEwBaseUrl + (_myWeb.moConfig["ProjectPath"] ?? "") + "/oauth2callback";

                if (!string.IsNullOrEmpty(config["redirectUri"]))
                {
                    redirectUri = config["redirectUri"];
                }

                using (var httpClient = new HttpClient())
                {
                    var parameters = new Dictionary<string, string>
                    {
                        { "grant_type", "authorization_code" },
                        { "code", code },
                        { "redirect_uri", redirectUri },
                        { "client_id", clientId },
                        { "client_secret", clientSecret }
                    };

                    var content = new FormUrlEncodedContent(parameters);
                    var response = await httpClient.PostAsync(tokenEndpoint, content);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new AuthenticationException($"Token exchange failed: {responseContent}");
                    }

                    var tokenResponse = JsonConvert.DeserializeObject<OAuth2TokenResponse>(responseContent);
                    return tokenResponse;
                }
            }

            /// <summary>
            /// Gets user info from OAuth2 provider
            /// </summary>
            public async Task<JObject> GetUserInfoAsync(string accessToken)
            {
                string userInfoEndpoint = config["userInfoEndpoint"];

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = 
                        new AuthenticationHeaderValue("Bearer", accessToken);

                    var response = await httpClient.GetAsync(userInfoEndpoint);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new AuthenticationException($"UserInfo request failed: {responseContent}");
                    }

                    return JObject.Parse(responseContent);
                }
            }

            /// <summary>
            /// Generates a secure state parameter for OAuth2
            /// </summary>
            private string GenerateState(string providerName)
            {
                var random = new byte[32];
                using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
                {
                    rng.GetBytes(random);
                }
                return $"{providerName}_{Convert.ToBase64String(random)}";
            }

            /// <summary>
            /// Decodes JWT token (without validation - for debugging/info only)
            /// </summary>
            public JObject DecodeJwt(string token)
            {
                var parts = token.Split('.');
                if (parts.Length != 3)
                {
                    throw new ArgumentException("Invalid JWT token format");
                }

                var payload = parts[1];
                // Add padding if needed
                payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');

                var payloadBytes = Convert.FromBase64String(payload);
                var payloadJson = Encoding.UTF8.GetString(payloadBytes);

                return JObject.Parse(payloadJson);
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
            //            userid = Convert.ToInt64(oUserDetails["nDirKey"]);                
            //        }
            //    }
            //    return userid;
            //}

        }

        /// <summary>
        /// OAuth2 token response model
        /// </summary>
        public class OAuth2TokenResponse
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; }

            [JsonProperty("token_type")]
            public string TokenType { get; set; }

            [JsonProperty("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonProperty("refresh_token")]
            public string RefreshToken { get; set; }

            [JsonProperty("id_token")]
            public string IdToken { get; set; }

            [JsonProperty("scope")]
            public string Scope { get; set; }
        }
    }
}

