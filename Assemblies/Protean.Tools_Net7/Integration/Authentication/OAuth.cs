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
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Protean.Tools.Integration.Authentication
{
    public partial class OAuth
    {
        public enum Method
        {
            GET,
            POST
        }

        protected const string OAuthVersion = "1.0";
        protected const string OAuthParameterPrefix = "oauth_";
        protected const string OAuthConsumerKeyKey = "oauth_consumer_key";
        protected const string OAuthCallbackKey = "oauth_callback";
        protected const string OAuthVersionKey = "oauth_version";
        protected const string OAuthSignatureMethodKey = "oauth_signature_method";
        protected const string OAuthSignatureKey = "oauth_signature";
        protected const string OAuthTimestampKey = "oauth_timestamp";
        protected const string OAuthNonceKey = "oauth_nonce";
        protected const string OAuthTokenKey = "oauth_token";
        protected const string OAuthTokenSecretKey = "oauth_token_secret";
        protected const string OAuthVerifier = "oauth_verifier";
        protected const string HMACSHA1SignatureType = "HMAC-SHA1";
        protected const string PlainTextSignatureType = "PLAINTEXT";
        protected const string RSASHA1SignatureType = "RSA-SHA1";

        protected Random _Random = new Random();

        public enum SignatureTypes
        {
            HMACSHA1,
            PLAINTEXT,
            RSASHA1
        }

        protected partial class QueryParameter
        {
            private string _name = null;
            private string _value = null;

            public QueryParameter(string Name, string value)
            {
                _name = Name;
                _value = value;
            }

            public string Name
            {
                get
                {
                    return _name;
                }
                set
                {
                    _name = value;
                }
            }

            public string Value
            {
                get
                {
                    return _value;
                }
                set
                {
                    _value = value;
                }
            }
        }

        protected partial class QueryParameterComparer : IComparer<QueryParameter>
        {

            public int Compare(QueryParameter x, QueryParameter y)
            {
                if ((x.Name ?? "") == (y.Name ?? ""))
                {
                    return string.Compare(x.Value, y.Value);
                }
                else
                {
                    return string.Compare(x.Name, y.Name);
                }
            }
        }

        private string ComputeHash(HashAlgorithm Algorithm, string Data)
        {
            if (Algorithm != null)
            {
                if (string.IsNullOrEmpty(Data) == false)
                {
                    byte[] DataBuffer = Encoding.ASCII.GetBytes(Data);
                    byte[] HashBytes = Algorithm.ComputeHash(DataBuffer);
                    return Convert.ToBase64String(HashBytes);
                }
                else
                {
                    throw new ArgumentNullException("Data");
                }
            }
            else
            {
                throw new ArgumentNullException("Algorithm");
            }


        }

        private List<QueryParameter> GetQueryParameters(string Parameters)
        {
            if (Parameters.StartsWith("?"))
            {
                Parameters = Parameters.Remove(0, 1);
            }

            var Result = new List<QueryParameter>();

            if (string.IsNullOrEmpty(Parameters) == false)
            {
                string[] p = Parameters.Split(Convert.ToChar("&"));
                foreach (string s in p)
                {
                    if (string.IsNullOrEmpty(s) == false) // And s.StartsWith(OAuthParameterPrefix) = False Then
                    {
                        if (s.IndexOf("=") > -1)
                        {
                            string[] Temp = s.Split(Convert.ToChar("="));
                            Result.Add(new QueryParameter(Temp[0], Temp[1]));
                        }
                        else
                        {
                            Result.Add(new QueryParameter(s, string.Empty));
                        }
                    }
                }
            }
            return Result;
        }

        // OAuthUrlEncode() method by "Spleen"
        // Thank you.
        public static string OAuthUrlEncode(string value)
        {
            var result = new StringBuilder();
            string unreservedchars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";
            foreach (char symbol in value)
            {
                if (unreservedchars.IndexOf(symbol) != -1)
                {
                    result.Append(symbol);
                }
                else
                {
                    byte[] charBytes;
                    charBytes = Encoding.UTF8.GetBytes(symbol.ToString());
                    foreach (byte charByte in charBytes)
                        result.AppendFormat("%{0:X2}", charByte);
                }
            }

            return result.ToString();
        }

        protected string NormalizeRequestParameters(IList<QueryParameter> Parameters)
        {
            var sb = new StringBuilder();
            QueryParameter p = null;
            for (int i = 0, loopTo = Parameters.Count - 1; i <= loopTo; i++)
            {
                p = Parameters[i];
                sb.AppendFormat("{0}={1}", p.Name, p.Value);
                if (i < Parameters.Count - 1)
                {
                    sb.Append("&");
                }
            }
            return sb.ToString();
        }

        public string GenerateSignatureBase(Uri URL, string ConsumerKey, string Token, string TokenSecret, string HTTPMethod, string TimeStamp, string Nonce, string SignatureType, ref string NormalizedURL, ref string NormalizedRequestParameters, string CallbackUrl, string Verifier)
        {
            if (Token is null)
            {
                Token = string.Empty;
            }

            if (TokenSecret is null)
            {
                TokenSecret = string.Empty;
            }

            if (string.IsNullOrEmpty(ConsumerKey))
            {
                throw new ArgumentNullException("ConsumerKey");
            }

            if (string.IsNullOrEmpty(HTTPMethod))
            {
                throw new ArgumentNullException("HTTPMethod");
            }

            if (string.IsNullOrEmpty(SignatureType))
            {
                throw new ArgumentNullException("SignatureType");
            }

            NormalizedURL = null;
            NormalizedRequestParameters = null;

            var Parameters = GetQueryParameters(URL.Query);
            Parameters.Add(new QueryParameter(OAuthVersionKey, OAuthVersion));
            Parameters.Add(new QueryParameter(OAuthNonceKey, Nonce));
            Parameters.Add(new QueryParameter(OAuthTimestampKey, TimeStamp));
            Parameters.Add(new QueryParameter(OAuthSignatureMethodKey, SignatureType));
            Parameters.Add(new QueryParameter(OAuthConsumerKeyKey, ConsumerKey));

            if (string.IsNullOrEmpty(Token) == false)
            {
                Parameters.Add(new QueryParameter(OAuthTokenKey, Token));
            }

            if (string.IsNullOrEmpty(CallbackUrl) == false)
            {
                Parameters.Add(new QueryParameter(OAuthCallbackKey, OAuthUrlEncode(CallbackUrl)));
            }

            if (string.IsNullOrEmpty(Verifier) == false)
            {
                Parameters.Add(new QueryParameter(OAuthVerifier, Verifier));
            }

            Parameters.Sort(new QueryParameterComparer());

            NormalizedURL = string.Format("{0}://{1}", URL.Scheme, URL.Host);
            if (!(URL.Scheme == "http" & URL.Port == 80 | URL.Scheme == "https" & URL.Port == 443))
            {
                NormalizedURL += ":" + URL.Port.ToString();
            }

            NormalizedURL += URL.AbsolutePath;
            NormalizedRequestParameters = NormalizeRequestParameters(Parameters);
            var SignatureBase = new StringBuilder();
            SignatureBase.AppendFormat("{0}&", HTTPMethod.ToUpper());
            SignatureBase.AppendFormat("{0}&", OAuthUrlEncode(NormalizedURL));
            SignatureBase.AppendFormat("{0}", OAuthUrlEncode(NormalizedRequestParameters));

            return SignatureBase.ToString();
        }

        public string GenerateSignatureUsingHash(string SignatureBase, HashAlgorithm Hash)
        {
            return ComputeHash(Hash, SignatureBase);
        }

        public string GenerateSignature(Uri URL, string ConsumerKey, string ConsumerSecret, string Token, string TokenSecret, string HTTPMethod, string TimeStamp, string Nonce, ref string NormalizedUrl, ref string NormalizedRequestParameters, string CallbackUrl, string Verifier)
        {
            return GenerateSignature(URL, ConsumerKey, ConsumerSecret, Token, TokenSecret, HTTPMethod, TimeStamp, Nonce, SignatureTypes.HMACSHA1, ref NormalizedUrl, ref NormalizedRequestParameters, CallbackUrl, Verifier);
        }

        public string GenerateSignature(Uri url, string ConsumerKey, string ConsumerSecret, string Token, string TokenSecret, string HTTPMethod, string TimeStamp, string Nonce, SignatureTypes SignatureType, ref string NormalizedUrl, ref string NormalizedRequestParameters, string CallbackUrl, string Verifier)
        {
            NormalizedUrl = null;
            NormalizedRequestParameters = null;

            switch (SignatureType)
            {
                case SignatureTypes.PLAINTEXT:
                    {
                        return HttpUtility.UrlEncode(string.Format("{0}&{1}", ConsumerSecret, TokenSecret));
                    }

                case SignatureTypes.HMACSHA1:
                    {
                        string SignatureBase = GenerateSignatureBase(url, ConsumerKey, Token, TokenSecret, HTTPMethod, TimeStamp, Nonce, HMACSHA1SignatureType, ref NormalizedUrl, ref NormalizedRequestParameters, CallbackUrl, Verifier);

                        var hmacsha1 = new HMACSHA1();
                        string ts = string.Empty;
                        if (string.IsNullOrEmpty(TokenSecret))
                        {
                            ts = string.Empty;
                        }
                        else
                        {
                            ts = OAuthUrlEncode(TokenSecret);
                        }
                        hmacsha1.Key = Encoding.ASCII.GetBytes(string.Format("{0}&{1}", OAuthUrlEncode(ConsumerSecret), ts));
                        return GenerateSignatureUsingHash(SignatureBase, hmacsha1);
                    }

                case SignatureTypes.RSASHA1:
                    {
                        throw new NotImplementedException();
                    }

                default:
                    {
                        throw new ArgumentException("Unknown signature type", "signatureType");
                    }
            }
        }

        public virtual string GenerateTimeStamp()
        {
            // Default implementation of UNIX time of the current UTC time
            var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

        public virtual string GenerateNonce()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }
    }
}
