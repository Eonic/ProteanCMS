using DocumentFormat.OpenXml.Wordprocessing;
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

            // mode: "signin" (default) or "link" when connecting an account to an existing user
            XmlElement GetLogonBtn(ref XmlElement oContextNode, string mode = "signin");

            string ExtractEmail(XmlDocument xmlDoc);
            string ExtractIssuer(XmlDocument xmlDoc);

            // Returns validated UserId (numeric string), empty if no response for this provider, otherwise an error message
            string ProcessAuthenticationResponse(string formName, out string keyUrl);

            // Links a response from this provider to an already signed-in user. Returns empty on success or no response, otherwise an error message.
            string LinkAuthenticationResponse(string formName, long userId, out bool linked);

            // Removes the link between this provider and the user
            void Unlink(long userId);

            // True if the user has a linked identity for this provider
            bool IsLinked(long userId);
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
                _Name = config["name"] ?? "Default";
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

            public XmlElement GetLogonBtn(ref XmlElement oContextNode, string mode = "signin")
            {
                bool linkMode = string.Equals(mode, "link", StringComparison.OrdinalIgnoreCase);

                string sClass = "principle";
                string sIcon = "";
                string sValue = _Name.ToLower();
                string sRef = "AuthProvider";
                string sSubmission = "AuthProvider";
                string sLabel = (linkMode ? "Connect " : "Sign in with ") + _Name;

                XmlElement addSubmitRet = default;

                XmlElement oIptElmt;
                XmlElement oLabelElmt;
               // string cProcessInfo = "";
                try
                {
                    oIptElmt = oContextNode.OwnerDocument.CreateElement("submit");
                    oIptElmt.SetAttribute("submission", sSubmission);
                    oIptElmt.SetAttribute("ref", sRef);
                    oIptElmt.SetAttribute("data-mode", linkMode ? "link" : "signin");
                    oIptElmt.SetAttribute("class", sClass);
                    if (!string.IsNullOrEmpty(sIcon))
                    {
                        oIptElmt.SetAttribute("icon", sIcon);
                    }
                    if (!string.IsNullOrEmpty(sValue))
                    {
                        oIptElmt.SetAttribute("value", sValue);
                    }

                    if (!string.IsNullOrEmpty(sLabel))
                    {
                        oLabelElmt = oContextNode.OwnerDocument.CreateElement("label");
                        if (sClass.Contains("term-"))
                        {
                            XmlElement oSpanElmt = oContextNode.OwnerDocument.CreateElement("span");
                            oSpanElmt.SetAttribute("class", "term" + Regex.Match(sClass, @"term-(\d+)").Groups[1].Value);
                            oSpanElmt.InnerText = sLabel;
                            oLabelElmt.AppendChild(oSpanElmt);
                        }
                        else
                        {
                            oLabelElmt.InnerText = sLabel;
                        }
                        oIptElmt.AppendChild(oLabelElmt);
                    }

                    oContextNode.AppendChild(oIptElmt);

                    addSubmitRet = oIptElmt;
                }
                catch (Exception ex)
                {
                    //returnException(ref msException, mcModuleName, "addSubmit", ex, "", cProcessInfo, gbDebug);
                    return null;
                }

                return addSubmitRet;
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

            /// <summary>
            /// Checks the current request for a SAML or OAuth2 response for this provider and validates the user.
            /// Returns the validated UserId as a string, empty if there is no response for this provider, otherwise an error message.
            /// </summary>
            public string ProcessAuthenticationResponse(string formName, out string keyUrl)
            {
                keyUrl = string.Empty;
                try
                {
                    ExternalIdentity identity;
                    if (AuthenticationType == "OAUTH2")
                    {
                        identity = GetOAuth2Identity(formName);
                    }
                    else
                    {
                        identity = GetSamlIdentity(formName, out keyUrl);
                    }

                    if (identity == null)
                    {
                        return string.Empty;
                    }
                    if (string.IsNullOrEmpty(identity.Issuer) || string.IsNullOrEmpty(identity.Subject))
                    {
                        throw new AuthenticationException("Response did not contain an issuer and subject");
                    }

                    long userId = FindLinkedUser(identity);
                    if (userId == 0)
                    {
                        if (string.IsNullOrEmpty(identity.Email))
                        {
                            throw new AuthenticationException("No email address was returned by the provider");
                        }
                        if (!identity.EmailVerified)
                        {
                            throw new AuthenticationException("The email address has not been verified by the provider");
                        }
                        if (!IsAllowedDomain(identity.Email))
                        {
                            throw new AuthenticationException("Email domain is not permitted");
                        }

                        userId = FindUserByEmail(identity.Email);
                        if (userId != 0)
                        {
                            if ((config["autoLink"] ?? "true").ToLower() == "false")
                            {
                                throw new AuthenticationException("An account with this email already exists and cannot be linked automatically");
                            }
                        }
                        else if (formName != "AdminLogon" && (config["autoCreate"] ?? "false").ToLower() == "true")
                        {
                            userId = CreateUser(identity);
                        }

                        if (userId == 0)
                        {
                            keyUrl = string.Empty;
                            return "No account exists for " + identity.Email;
                        }
                        LinkUser(userId, identity);
                    }

                    string inactiveReason = GetInactiveReason(userId);
                    if (!string.IsNullOrEmpty(inactiveReason))
                    {
                        keyUrl = string.Empty;
                        return inactiveReason;
                    }

                    _myWeb.moDbHelper.ExeProcessSql("update tblDirectoryExternalLogin set dLastLogin = GETDATE(), cEmail = '" + SqlFmt(identity.Email ?? string.Empty) + "' where cIssuer = '" + SqlFmt(identity.Issuer) + "' and cSubject = '" + SqlFmt(identity.Subject) + "'");
                    return userId.ToString();
                }
                catch (Exception ex)
                {
                    keyUrl = string.Empty;
                    return "Authentication failed: " + ex.Message;
                }
            }

            public string LinkAuthenticationResponse(string formName, long userId, out bool linked)
            {
                linked = false;
                try
                {
                    string keyUrl;
                    ExternalIdentity identity = AuthenticationType == "OAUTH2" ? GetOAuth2Identity(formName) : GetSamlIdentity(formName, out keyUrl);
                    if (identity == null)
                    {
                        return string.Empty;
                    }
                    if (string.IsNullOrEmpty(identity.Issuer) || string.IsNullOrEmpty(identity.Subject))
                    {
                        throw new AuthenticationException("Response did not contain an issuer and subject");
                    }

                    long existing = FindLinkedUser(identity);
                    if (existing == userId)
                    {
                        linked = true;
                        return string.Empty;
                    }
                    if (existing != 0)
                    {
                        return "This " + _Name + " account is already linked to another user";
                    }
                    if (!string.IsNullOrEmpty(identity.Email) && !IsAllowedDomain(identity.Email))
                    {
                        return "Email domain is not permitted";
                    }
                    LinkUser(userId, identity);
                    linked = true;
                    return string.Empty;
                }
                catch (Exception ex)
                {
                    return "Could not connect " + _Name + ": " + ex.Message;
                }
            }

            public void Unlink(long userId)
            {
                _myWeb.moDbHelper.ExeProcessSql("delete from tblDirectoryExternalLogin where nDirKey = " + userId + " and cProviderName = '" + SqlFmt(_Name) + "'");
            }

            public bool IsLinked(long userId)
            {
                object result = _myWeb.moDbHelper.ExeProcessSqlScalar("select count(*) from tblDirectoryExternalLogin where nDirKey = " + userId + " and cProviderName = '" + SqlFmt(_Name) + "'");
                return result != null && !(result is DBNull) && Convert.ToInt32(result) > 0;
            }

            private class ExternalIdentity
            {
                public string Issuer;
                public string Subject;
                public string Email;
                public string FirstName;
                public string LastName;
                public bool EmailVerified;
            }

            private const string SamlAssertionNs = "urn:oasis:names:tc:SAML:2.0:assertion";

            /// <summary>
            /// Validates the XML signature of a SAML response against the IdP certificate (config "x509Certificate",
            /// base64 or PEM), guards against signature wrapping and checks the assertion time conditions.
            /// Throws AuthenticationException if the response cannot be trusted.
            /// </summary>
            public void ValidateSamlSignature(XmlDocument xmlDoc)
            {
                string certText = config["x509Certificate"];
                if (string.IsNullOrWhiteSpace(certText))
                {
                    throw new AuthenticationException("x509Certificate is not configured for provider");
                }
                var cert = new System.Security.Cryptography.X509Certificates.X509Certificate2(
                    Convert.FromBase64String(Regex.Replace(certText, @"-----[^-]+-----|\s", "")));

                XmlNodeList assertions = xmlDoc.SelectNodes("//*[local-name()='Assertion' and namespace-uri()='" + SamlAssertionNs + "']");
                if (assertions.Count != 1)
                {
                    throw new AuthenticationException("SAML response must contain exactly one assertion");
                }
                XmlElement assertion = (XmlElement)assertions[0];

                bool signatureValid = false;
                foreach (XmlElement target in new[] { xmlDoc.DocumentElement, assertion })
                {
                    string id = target.GetAttribute("ID");
                    if (string.IsNullOrEmpty(id))
                    {
                        continue;
                    }
                    foreach (XmlNode child in target.ChildNodes)
                    {
                        if (child.LocalName != "Signature" || child.NamespaceURI != System.Security.Cryptography.Xml.SignedXml.XmlDsigNamespaceUrl)
                        {
                            continue;
                        }
                        var signedXml = new System.Security.Cryptography.Xml.SignedXml(target);
                        signedXml.LoadXml((XmlElement)child);

                        // the signature must cover exactly the element it sits in
                        if (signedXml.SignedInfo.References.Count != 1)
                        {
                            continue;
                        }
                        var reference = (System.Security.Cryptography.Xml.Reference)signedXml.SignedInfo.References[0];
                        if (reference.Uri != "#" + id)
                        {
                            continue;
                        }
                        if (signedXml.CheckSignature(cert, true))
                        {
                            signatureValid = true;
                            break;
                        }
                    }
                    if (signatureValid)
                    {
                        break;
                    }
                }
                if (!signatureValid)
                {
                    throw new AuthenticationException("Invalid SAML signature");
                }

                XmlElement conditions = (XmlElement)assertion.SelectSingleNode("*[local-name()='Conditions']");
                if (conditions != null)
                {
                    var skew = TimeSpan.FromMinutes(5);
                    string notBefore = conditions.GetAttribute("NotBefore");
                    string notOnOrAfter = conditions.GetAttribute("NotOnOrAfter");
                    if (!string.IsNullOrEmpty(notBefore) && DateTime.UtcNow < XmlConvert.ToDateTime(notBefore, XmlDateTimeSerializationMode.Utc) - skew)
                    {
                        throw new AuthenticationException("SAML assertion is not yet valid");
                    }
                    if (!string.IsNullOrEmpty(notOnOrAfter) && DateTime.UtcNow >= XmlConvert.ToDateTime(notOnOrAfter, XmlDateTimeSerializationMode.Utc) + skew)
                    {
                        throw new AuthenticationException("SAML assertion has expired");
                    }
                }
            }

            public string ExtractSubject(XmlDocument xmlDoc)
            {
                XmlNode nameId = xmlDoc.SelectSingleNode("//*[local-name()='Subject']/*[local-name()='NameID']");
                return nameId?.InnerText.Trim() ?? string.Empty;
            }

            private long FindLinkedUser(ExternalIdentity identity)
            {
                object result = _myWeb.moDbHelper.ExeProcessSqlScalar("select nDirKey from tblDirectoryExternalLogin where cIssuer = '" + SqlFmt(identity.Issuer) + "' and cSubject = '" + SqlFmt(identity.Subject) + "'");
                return (result == null || result is DBNull) ? 0 : Convert.ToInt64(result);
            }

            private long FindUserByEmail(string email)
            {
                string safeEmail = Convert.ToString(SqlFmt(email));
                object result = _myWeb.moDbHelper.ExeProcessSqlScalar("select top 1 nDirKey from tblDirectory where cDirSchema = 'User' and (cDirName = '" + safeEmail + "' or (cDirEmail != '' and cDirEmail = '" + safeEmail + "'))");
                return (result == null || result is DBNull) ? 0 : Convert.ToInt64(result);
            }

            private void LinkUser(long userId, ExternalIdentity identity)
            {
                _myWeb.moDbHelper.ExeProcessSql("insert into tblDirectoryExternalLogin (nDirKey, cProviderName, cIssuer, cSubject, cEmail) values (" + userId + ", '" + SqlFmt(_Name) + "', '" + SqlFmt(identity.Issuer) + "', '" + SqlFmt(identity.Subject) + "', '" + SqlFmt(identity.Email ?? string.Empty) + "')");
            }

            private long CreateUser(ExternalIdentity identity)
            {
                XmlDocument userXml = new XmlDocument();
                XmlElement user = userXml.CreateElement("User");
                userXml.AppendChild(user);
                foreach (var field in new[] { Tuple.Create("FirstName", identity.FirstName), Tuple.Create("LastName", identity.LastName), Tuple.Create("Email", identity.Email) })
                {
                    XmlElement elmt = userXml.CreateElement(field.Item1);
                    elmt.InnerText = field.Item2 ?? string.Empty;
                    user.AppendChild(elmt);
                }

                // Random password that nobody knows - the user signs in through the provider
                string unusablePassword = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
                long userId = _myWeb.moDbHelper.insertDirectory(string.Empty, "User", identity.Email, unusablePassword, userXml.OuterXml, 1, false, identity.Email);

                long groupId;
                if (userId > 0 && long.TryParse(config["defaultGroupId"], out groupId) && groupId > 0)
                {
                    _myWeb.moDbHelper.maintainDirectoryRelation(groupId, userId);
                }
                return userId;
            }

            private bool IsAllowedDomain(string email)
            {
                string allowed = config["allowedDomains"];
                if (string.IsNullOrWhiteSpace(allowed))
                {
                    return true;
                }
                string domain = email.Substring(email.LastIndexOf('@') + 1);
                return allowed.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Any(d => d.Trim().Equals(domain, StringComparison.OrdinalIgnoreCase));
            }

            /// <summary>
            /// Returns an empty string if the user is active, otherwise the reason they cannot log on.
            /// </summary>
            private string GetInactiveReason(long userId)
            {
                DataSet ds = _myWeb.moDbHelper.GetDataSet("select a.nStatus, a.dPublishDate, a.dExpireDate from tblDirectory d inner join tblAudit a on a.nAuditKey = d.nAuditId where d.cDirSchema = 'User' and d.nDirKey = " + userId, "tblTemp");
                if (ds == null || ds.Tables[0].Rows.Count == 0)
                {
                    return "Could not identify the account. Please contact the System Administrator.";
                }
                DataRow row = ds.Tables[0].Rows[0];
                int status = Convert.ToInt32(row["nStatus"]);
                if (status != 1 && status != -1)
                {
                    return "User account has been disabled";
                }
                if (!(row["dPublishDate"] is DBNull) && Convert.ToDateTime(row["dPublishDate"]) > DateTime.Now)
                {
                    return "User account is not active";
                }
                if (!(row["dExpireDate"] is DBNull) && Convert.ToDateTime(row["dExpireDate"]) < DateTime.Now.Date)
                {
                    return "User account has expired";
                }
                return string.Empty;
            }

            private ExternalIdentity GetSamlIdentity(string formName, out string keyUrl)
            {
                keyUrl = string.Empty;
                string samlResponse = _myWeb.moRequest["SAMLResponse"];
                string relayState = _myWeb.moRequest["RelayState"];
                if (string.IsNullOrEmpty(samlResponse) || relayState == null)
                {
                    return null;
                }
                if (relayState.Contains("|"))
                {
                    var parts = relayState.Split('|');
                    relayState = parts[0];
                    keyUrl = parts[1];
                }

                if (formName != "AdminLogon" || config["scope"] != "admin")
                {
                    return null;
                }
                if (!_Name.Equals(relayState, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.XmlResolver = null;
                xmlDoc.PreserveWhitespace = true;
                xmlDoc.LoadXml(Encoding.UTF8.GetString(Convert.FromBase64String(samlResponse)));

                string issuer = ExtractIssuer(xmlDoc);
                if (config["entityId"] == null || !config["entityId"].Equals(issuer, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                ValidateSamlSignature(xmlDoc);

                string email = ExtractEmail(xmlDoc) ?? string.Empty;
                string subject = ExtractSubject(xmlDoc);
                return new ExternalIdentity
                {
                    Issuer = issuer,
                    Subject = string.IsNullOrEmpty(subject) ? email : subject,
                    Email = email,
                    EmailVerified = true // asserted by the configured IdP
                };
            }

            private ExternalIdentity GetOAuth2Identity(string formName)
            {
                string code = _myWeb.moRequest["code"];
                string state = _myWeb.moRequest["state"];
                string idToken = _myWeb.moRequest["id_token"];
                string postedProvider = _myWeb.moRequest["auth_provider"];
                bool clientSideJwt = !string.IsNullOrEmpty(idToken) && string.IsNullOrEmpty(state);

                if (string.IsNullOrEmpty(code) && string.IsNullOrEmpty(idToken))
                {
                    return null;
                }
                if (clientSideJwt)
                {
                    // JWT posted by client script (e.g. Google Identity Services button)
                    if (!_Name.Equals(postedProvider, StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }
                }
                else if (string.IsNullOrEmpty(state) || !state.StartsWith(_Name + "_", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                string loginScope = config["loginScope"];
                if (!string.IsNullOrEmpty(loginScope) && (loginScope == "admin") != (formName == "AdminLogon"))
                {
                    return null;
                }

                if (!clientSideJwt)
                {
                    string storedState = _myWeb.moSession?["OAuth2_State"] as string;
                    if (storedState == null || storedState != state)
                    {
                        throw new AuthenticationException("Invalid state parameter - possible CSRF attack");
                    }
                }

                if (string.IsNullOrEmpty(idToken))
                {
                    var tokens = Task.Run(() => ExchangeCodeForTokenAsync(code, state)).GetAwaiter().GetResult();
                    idToken = tokens.IdToken;
                }
                if (!clientSideJwt && _myWeb.moSession != null)
                {
                    _myWeb.moSession["OAuth2_State"] = null;
                }
                if (string.IsNullOrEmpty(idToken))
                {
                    throw new AuthenticationException("No JWT returned by provider");
                }

                JObject claims = ValidateJwt(idToken);

                bool emailVerified;
                JToken verifiedClaim = claims["email_verified"];
                if (verifiedClaim != null)
                {
                    emailVerified = verifiedClaim.Type == JTokenType.Boolean
                        ? (bool)verifiedClaim
                        : string.Equals((string)verifiedClaim, "true", StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    // Some providers (e.g. Microsoft) do not send email_verified
                    emailVerified = IsEmailVerifiedFallback(claims);
                }

                return new ExternalIdentity
                {
                    Issuer = (string)claims["iss"],
                    Subject = (string)claims["sub"],
                    Email = ExtractEmailFromJson(claims),
                    FirstName = (string)claims["given_name"] ?? _myWeb.moRequest["first_name"],
                    LastName = (string)claims["family_name"] ?? _myWeb.moRequest["last_name"],
                    EmailVerified = emailVerified
                };
            }

            /// <summary>
            /// Validates JWT signature (RS256 via jwksUri), issuer, audience and expiry. Returns the claims.
            /// </summary>
            public JObject ValidateJwt(string token)
            {
                var parts = token.Split('.');
                if (parts.Length != 3)
                {
                    throw new AuthenticationException("Invalid JWT token format");
                }

                JObject header = JObject.Parse(Encoding.UTF8.GetString(Base64UrlDecode(parts[0])));
                if ((string)header["alg"] != "RS256")
                {
                    throw new AuthenticationException("Unsupported JWT algorithm");
                }

                string jwksUri = config["jwksUri"];
                if (string.IsNullOrEmpty(jwksUri))
                {
                    jwksUri = DefaultJwksUri;
                }
                if (string.IsNullOrEmpty(jwksUri))
                {
                    throw new AuthenticationException("jwksUri is not configured for provider");
                }

                string jwksJson;
                using (var httpClient = new HttpClient())
                {
                    jwksJson = Task.Run(() => httpClient.GetStringAsync(jwksUri)).GetAwaiter().GetResult();
                }

                string kid = (string)header["kid"];
                JToken key = JObject.Parse(jwksJson)["keys"]?
                    .FirstOrDefault(k => (string)k["kty"] == "RSA" && (kid == null || (string)k["kid"] == kid));
                if (key == null)
                {
                    throw new AuthenticationException("Signing key not found");
                }

                var rsaParams = new System.Security.Cryptography.RSAParameters
                {
                    Modulus = Base64UrlDecode((string)key["n"]),
                    Exponent = Base64UrlDecode((string)key["e"])
                };
                using (var rsa = new System.Security.Cryptography.RSACryptoServiceProvider())
                {
                    rsa.ImportParameters(rsaParams);
                    byte[] data = Encoding.ASCII.GetBytes(parts[0] + "." + parts[1]);
                    if (!rsa.VerifyData(data, System.Security.Cryptography.CryptoConfig.MapNameToOID("SHA256"), Base64UrlDecode(parts[2])))
                    {
                        throw new AuthenticationException("Invalid JWT signature");
                    }
                }

                JObject claims = DecodeJwt(token);

                string expectedIssuer = config["issuer"];
                if (!string.IsNullOrEmpty(expectedIssuer) && !expectedIssuer.Equals((string)claims["iss"], StringComparison.OrdinalIgnoreCase))
                {
                    throw new AuthenticationException("Invalid JWT issuer");
                }
                if (string.IsNullOrEmpty(expectedIssuer) && !IsValidDefaultIssuer((string)claims["iss"]))
                {
                    throw new AuthenticationException("Invalid JWT issuer");
                }

                string clientId = config["clientId"];
                JToken aud = claims["aud"];
                bool audOk = aud != null && (aud.Type == JTokenType.Array
                    ? aud.Any(a => (string)a == clientId)
                    : (string)aud == clientId);
                if (!audOk)
                {
                    throw new AuthenticationException("Invalid JWT audience");
                }

                long exp = claims["exp"] != null ? (long)claims["exp"] : 0;
                if (exp == 0 || DateTimeOffset.UtcNow.ToUnixTimeSeconds() > exp + 60)
                {
                    throw new AuthenticationException("JWT has expired");
                }

                return claims;
            }

            /// <summary>
            /// Provider specific default JWKS endpoint, used when jwksUri is not configured. Override in derived providers.
            /// </summary>
            protected virtual string DefaultJwksUri
            {
                get { return null; }
            }

            /// <summary>
            /// Provider specific issuer check, used when issuer is not configured. Override in derived providers.
            /// </summary>
            protected virtual bool IsValidDefaultIssuer(string issuer)
            {
                return true;
            }

            /// <summary>
            /// Used when the JWT has no email_verified claim. Defaults to the trustUnverifiedEmail config setting. Override in derived providers.
            /// </summary>
            protected virtual bool IsEmailVerifiedFallback(JObject claims)
            {
                return string.Equals(config["trustUnverifiedEmail"], "true", StringComparison.OrdinalIgnoreCase);
            }

            private static byte[] Base64UrlDecode(string input)
            {
                string s = input.Replace('-', '+').Replace('_', '/');
                s = s.PadRight(s.Length + (4 - s.Length % 4) % 4, '=');
                return Convert.FromBase64String(s);
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

