

// *
// * This file is part of the TwitterVB software
// * Copyright (c) 2009, Duane Roelands <duane@getTwitterVB.com>
// * All rights reserved.
// *
// * TwitterVB is a port of the Twitterizer library <http://code.google.com/p/twitterizer/>
// * Copyright (c) 2008, Patrick "Ricky" Smith <ricky@digitally-born.com>
// * All rights reserved. 
// *
// * Redistribution and use in source and binary forms, with or without modification, are 
// * permitted provided that the following conditions are met:
// *
// * - Redistributions of source code must retain the above copyright notice, this list 
// *   of conditions and the following disclaimer.
// * - Redistributions in binary form must reproduce the above copyright notice, this list 
// *   of conditions and the following disclaimer in the documentation and/or other 
// *   materials provided with the distribution.
// * - Neither the name of TwitterVB nor the names of its contributors may be 
// *   used to endorse or promote products derived from this software without specific 
// *   prior written permission.
// *
// * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
// * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
// * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
// * IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
// * INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT 
// * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR 
// * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
// * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
// * POSSIBILITY OF SUCH DAMAGE.
// *
using System;
using System.IO;
using System.Net;
using System.Web;

namespace Protean.Tools.Integration.Twitter.TwitterVB2
{
    /// <summary>
    /// A class for implementing OAuth authentication via Twitter.
    /// </summary>
    /// <remarks></remarks>
    /// <exclude/>
    public partial class TwitterOAuth : Protean.Tools.Integration.Authentication.OAuth
    {

        public const string REQUEST_TOKEN = "https://api.twitter.com/oauth/request_token";
        public const string AUTHORIZE = "https://api.twitter.com/oauth/authorize";
        public const string ACCESS_TOKEN = "https://api.twitter.com/oauth/access_token";
        public const string AUTHENTICATE = "https://api.twitter.com/oauth/authenticate";
        public const string XACCESS_TOKEN = "https://api.twitter.com/oauth/access_token";

        public string ConsumerKey = string.Empty;
        public string ConsumerSecret = string.Empty;
        public string Token = string.Empty;
        public string TokenSecret = string.Empty;
        public string Verifier = string.Empty;
        public string CallbackUrl = string.Empty;

        public TwitterOAuth()
        {
        }

        public TwitterOAuth(string ConsumerKey, string ConsumerKeySecret)
        {
            this.ConsumerKey = ConsumerKey;
            ConsumerSecret = ConsumerKeySecret;
        }

        public TwitterOAuth(string ConsumerKey, string ConsumerKeySecret, string Token, string TokenSecret)
        {
            {
                ref var withBlock = ref this;
                withBlock.Token = Token;
                withBlock.TokenSecret = TokenSecret;
                withBlock.ConsumerKey = ConsumerKey;
                withBlock.ConsumerSecret = ConsumerKeySecret;
            }
        }

        public TwitterOAuth(string ConsumerKey, string ConsumerKeySecret, string CallbackUrl)
        {
            this.ConsumerKey = ConsumerKey;
            ConsumerSecret = ConsumerKeySecret;
            this.CallbackUrl = CallbackUrl;
        }

        public string GetAuthorizationLink()
        {
            string ReturnValue = null;
            string Response = OAuthWebRequest(Method.GET, REQUEST_TOKEN, string.Empty);
            if (Response.Length > 0)
            {
                var qs = HttpUtility.ParseQueryString(Response);
                if (qs["oauth_token"] != null)
                {
                    Token = qs["oauth_token"];
                    ReturnValue = string.Concat(AUTHORIZE, "?oauth_token=", qs["oauth_token"]);
                }
            }
            return ReturnValue;
        }

        public string GetAuthenticationLink()
        {
            string ReturnValue = null;
            string Response = OAuthWebRequest(Method.GET, REQUEST_TOKEN, string.Empty);
            if (Response.Length > 0)
            {
                var qs = HttpUtility.ParseQueryString(Response);
                if (qs["oauth_token"] != null)
                {
                    Token = qs["oauth_token"];
                    ReturnValue = string.Concat(AUTHENTICATE, "?oauth_token=", qs["oauth_token"]);
                }
            }
            return ReturnValue;
        }

        public bool ValidatePIN(string PIN)
        {

            bool ReturnValue;

            try
            {
                string Response = OAuthWebRequest(Method.GET, string.Format("{0}?oauth_verifier={1}", ACCESS_TOKEN, PIN), string.Empty);
                if (Response.Length > 0)
                {
                    var qs = HttpUtility.ParseQueryString(Response);
                    if (qs["oauth_token"] != null)
                    {
                        Token = qs["oauth_token"];
                    }
                    if (qs["oauth_token_secret"] != null)
                    {
                        TokenSecret = qs["oauth_token_secret"];
                    }
                    ReturnValue = true;
                }
                else
                {
                    ReturnValue = false;
                }
            }

            catch (Exception ex)
            {
                ReturnValue = false;
            }

            return ReturnValue;
        }
        public void GetXAccess(string p_strUserName, string p_strPassword)
        {

            string strInformation;
            strInformation = "?x_auth_username=" + p_strUserName + "&x_auth_password=" + p_strPassword + "&x_auth_mode=client_auth";
            string response = OAuthWebRequest(Method.POST, XACCESS_TOKEN, strInformation);
            if (response.Length > 0)
            {
                var qs = HttpUtility.ParseQueryString(response);
                if (qs["oauth_token"] != null)
                {
                    Token = qs["oauth_token"];
                }
                if (qs["oauth_token_secret"] != null)
                {
                    TokenSecret = qs["oauth_token_secret"];
                }
            }
        }
        public void GetAccessToken(string AuthToken, string AuthVerifier)
        {
            Token = AuthToken;
            Verifier = AuthVerifier;
            string Response = OAuthWebRequest(Method.GET, ACCESS_TOKEN, string.Empty);
            if (Response.Length > 0)
            {
                var qs = HttpUtility.ParseQueryString(Response);
                if (qs["oauth_token"] != null)
                {
                    Token = qs["oauth_token"];
                }
                if (qs["oauth_token_secret"] != null)
                {
                    TokenSecret = qs["oauth_token_secret"];
                }
            }
        }

        public string OAuthWebRequest(Method RequestMethod, string url, string PostData)
        {
            string OutURL = string.Empty;
            string QueryString = string.Empty;
            string ReturnValue = string.Empty;
            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(ValidateCertificate);
            if (RequestMethod == Method.POST)
            {
                if (PostData.Length > 0)
                {
                    var qs = HttpUtility.ParseQueryString(PostData);
                    PostData = string.Empty;
                    foreach (string Key in qs.AllKeys)
                    {
                        if (PostData.Length > 0)
                        {
                            PostData += "&";
                        }
                        qs[Key] = HttpUtility.UrlDecode(qs[Key]);
                        qs[Key] = OAuthUrlEncode(qs[Key]);
                        PostData += Key + "=" + qs[Key];
                    }
                    if (url.IndexOf("?") > 0)
                    {
                        url += "&";
                    }
                    else
                    {
                        url += "?";
                    }
                    url += PostData;
                }
            }

            var RequestUri = new Uri(url);
            string Nonce = GenerateNonce();
            string TimeStamp = GenerateTimeStamp();
            string Sig = GenerateSignature(RequestUri, ConsumerKey, ConsumerSecret, Token, TokenSecret, RequestMethod.ToString(), TimeStamp, Nonce, ref OutURL, ref QueryString, CallbackUrl, Verifier);
            QueryString += "&oauth_signature=" + OAuthUrlEncode(Sig);
            if (RequestMethod == Protean.Tools.Integration.Authentication.OAuth.Method.POST)
            {
                PostData = QueryString;
                QueryString = string.Empty;
            }

            if (QueryString.Length > 0)
            {
                OutURL += "?";
            }

            ReturnValue = WebRequest(RequestMethod, OutURL + QueryString, PostData);

            return ReturnValue;
        }

        public string WebRequest(Method RequestMethod, string Url, string PostData)
        {

            try
            {
                HttpWebRequest Request = System.Net.WebRequest.Create(Url) as HttpWebRequest;
                System.Net.WebProxy p = default;
                Request.Method = RequestMethod.ToString();
                Request.ServicePoint.Expect100Continue = false;
                if (Globals.Proxy_Username != string.Empty & Globals.Proxy_Password != string.Empty)
                {
                    p = new System.Net.WebProxy();
                    p.Credentials = new NetworkCredential(Globals.Proxy_Username, Globals.Proxy_Password);
                    Request.Proxy = p;
                }

                if (RequestMethod == Method.POST)
                {
                    Request.ContentType = "application/x-www-form-urlencoded";
                    using (var RequestWriter = new StreamWriter(Request.GetRequestStream()))
                    {
                        RequestWriter.Write(PostData);
                    }
                }

                System.Net.WebResponse wr = Request.GetResponse;
                Globals.API_HourlyLimit = wr.Headers("X-RateLimit-Limit");
                Globals.API_RemainingHits = wr.Headers("X-RateLimit-Remaining");
                Globals.API_Reset = wr.Headers("X-RateLimit-Reset");

                using (var ResponseReader = new StreamReader(wr.GetResponseStream()))
                {
                    return ResponseReader.ReadToEnd();
                }
            }

            catch (System.Net.WebException wex)
            {
                var tax = new TwitterAPIException(wex);
                tax.Url = Url;
                tax.Method = RequestMethod.ToString;
                tax.AuthType = "OAUTH";
                tax.Status = wex.Status;
                tax.Response = wex.Response;
                throw tax;
            }

            catch (Exception ex)
            {
                var tax = new TwitterAPIException(ex);
                tax.Url = Url;
                tax.Method = RequestMethod.ToString;
                tax.AuthType = "OAUTH";
                tax.Status = null;
                tax.Response = null;
                throw tax;
            }

        }
        private bool ValidateCertificate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }

}
