'*
'* This file is part of the TwitterVB software
'* Copyright (c) 2009, Duane Roelands <duane@getTwitterVB.com>
'* All rights reserved.
'*
'* TwitterVB is a port of the Twitterizer library <http://code.google.com/p/twitterizer/>
'* Copyright (c) 2008, Patrick "Ricky" Smith <ricky@digitally-born.com>
'* All rights reserved. 
'*
'* Redistribution and use in source and binary forms, with or without modification, are 
'* permitted provided that the following conditions are met:
'*
'* - Redistributions of source code must retain the above copyright notice, this list 
'*   of conditions and the following disclaimer.
'* - Redistributions in binary form must reproduce the above copyright notice, this list 
'*   of conditions and the following disclaimer in the documentation and/or other 
'*   materials provided with the distribution.
'* - Neither the name of TwitterVB nor the names of its contributors may be 
'*   used to endorse or promote products derived from this software without specific 
'*   prior written permission.
'*
'* THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
'* ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
'* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
'* IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
'* INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT 
'* NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR 
'* PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
'* WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
'* ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
'* POSSIBILITY OF SUCH DAMAGE.
'*
Imports System
Imports System.Security.Cryptography
Imports System.Collections.Specialized
Imports System.IO
Imports System.Text
Imports System.Web
Imports System.Net
Namespace Integration.Authentication
    Public Class OAuth

        Public Enum Method
            [GET]
            POST
        End Enum

        Protected Const OAuthVersion As String = "1.0"
        Protected Const OAuthParameterPrefix As String = "oauth_"
        Protected Const OAuthConsumerKeyKey As String = "oauth_consumer_key"
        Protected Const OAuthCallbackKey As String = "oauth_callback"
        Protected Const OAuthVersionKey As String = "oauth_version"
        Protected Const OAuthSignatureMethodKey As String = "oauth_signature_method"
        Protected Const OAuthSignatureKey As String = "oauth_signature"
        Protected Const OAuthTimestampKey As String = "oauth_timestamp"
        Protected Const OAuthNonceKey As String = "oauth_nonce"
        Protected Const OAuthTokenKey As String = "oauth_token"
        Protected Const OAuthTokenSecretKey As String = "oauth_token_secret"
        Protected Const OAuthVerifier As String = "oauth_verifier"
        Protected Const HMACSHA1SignatureType As String = "HMAC-SHA1"
        Protected Const PlainTextSignatureType As String = "PLAINTEXT"
        Protected Const RSASHA1SignatureType As String = "RSA-SHA1"

        Protected _Random As Random = New Random()

        Public Enum SignatureTypes
            HMACSHA1
            PLAINTEXT
            RSASHA1
        End Enum

        Protected Class QueryParameter
            Dim _name As String = Nothing
            Dim _value As String = Nothing

            Public Sub New(ByVal Name As String, ByVal value As String)
                _name = Name
                _value = value
            End Sub

            Public Property Name() As String
                Get
                    Return _name
                End Get
                Set(ByVal value As String)
                    _name = value
                End Set
            End Property

            Public Property Value() As String
                Get
                    Return _value
                End Get
                Set(ByVal value As String)
                    _value = value
                End Set
            End Property
        End Class

        Protected Class QueryParameterComparer
            Implements IComparer(Of QueryParameter)

            Public Function Compare(ByVal x As QueryParameter, ByVal y As QueryParameter) As Integer Implements System.Collections.Generic.IComparer(Of QueryParameter).Compare
                If x.Name = y.Name Then
                    Return String.Compare(x.Value, y.Value)
                Else
                    Return String.Compare(x.Name, y.Name)
                End If
            End Function
        End Class

        Private Function ComputeHash(ByVal Algorithm As HashAlgorithm, ByVal Data As String) As String
            If Algorithm IsNot Nothing Then
                If String.IsNullOrEmpty(Data) = False Then
                    Dim DataBuffer() As Byte = System.Text.Encoding.ASCII.GetBytes(Data)
                    Dim HashBytes() As Byte = Algorithm.ComputeHash(DataBuffer)
                    Return Convert.ToBase64String(HashBytes)
                Else
                    Throw New ArgumentNullException("Data")
                End If
            Else
                Throw New ArgumentNullException("Algorithm")
            End If


        End Function

        Private Function GetQueryParameters(ByVal Parameters As String) As List(Of QueryParameter)
            If Parameters.StartsWith("?") Then
                Parameters = Parameters.Remove(0, 1)
            End If

            Dim Result As List(Of QueryParameter) = New List(Of QueryParameter)

            If String.IsNullOrEmpty(Parameters) = False Then
                Dim p() As String = Parameters.Split(Convert.ToChar("&"))
                For Each s As String In p
                    If String.IsNullOrEmpty(s) = False Then 'And s.StartsWith(OAuthParameterPrefix) = False Then
                        If s.IndexOf("=") > -1 Then
                            Dim Temp() As String = s.Split(Convert.ToChar("="))
                            Result.Add(New QueryParameter(Temp(0), Temp(1)))
                        Else
                            Result.Add(New QueryParameter(s, String.Empty))
                        End If
                    End If
                Next
            End If
            Return Result
        End Function

        ' OAuthUrlEncode() method by "Spleen"
        ' Thank you.
        Public Shared Function OAuthUrlEncode(ByVal value As String) As String
            Dim result As New StringBuilder()
            Dim unreservedchars As String = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~"
            For Each symbol As Char In value
                If unreservedchars.IndexOf(symbol) <> -1 Then
                    result.Append(symbol)
                Else
                    Dim charBytes As Byte()
                    charBytes = Encoding.UTF8.GetBytes(symbol.ToString())
                    For Each charByte As Byte In charBytes
                        result.AppendFormat("%{0:X2}", charByte)
                    Next
                End If
            Next

            Return result.ToString()
        End Function

        Protected Function NormalizeRequestParameters(ByVal Parameters As IList(Of QueryParameter)) As String
            Dim sb As New StringBuilder
            Dim p As QueryParameter = Nothing
            For i As Integer = 0 To Parameters.Count - 1
                p = Parameters(i)
                sb.AppendFormat("{0}={1}", p.Name, p.Value)
                If i < Parameters.Count - 1 Then
                    sb.Append("&")
                End If
            Next
            Return sb.ToString
        End Function

        Public Function GenerateSignatureBase(ByVal URL As Uri, ByVal ConsumerKey As String, ByVal Token As String, ByVal TokenSecret As String, ByVal HTTPMethod As String, ByVal TimeStamp As String, ByVal Nonce As String, ByVal SignatureType As String, ByRef NormalizedURL As String, ByRef NormalizedRequestParameters As String, ByVal CallbackUrl As String, ByVal Verifier As String) As String
            If Token Is Nothing Then
                Token = String.Empty
            End If

            If TokenSecret Is Nothing Then
                TokenSecret = String.Empty
            End If

            If String.IsNullOrEmpty(ConsumerKey) Then
                Throw New ArgumentNullException("ConsumerKey")
            End If

            If String.IsNullOrEmpty(HTTPMethod) Then
                Throw New ArgumentNullException("HTTPMethod")
            End If

            If String.IsNullOrEmpty(SignatureType) Then
                Throw New ArgumentNullException("SignatureType")
            End If

            NormalizedURL = Nothing
            NormalizedRequestParameters = Nothing

            Dim Parameters As List(Of QueryParameter) = GetQueryParameters(URL.Query)
            With Parameters
                Parameters.Add(New QueryParameter(OAuthVersionKey, OAuthVersion))
                Parameters.Add(New QueryParameter(OAuthNonceKey, Nonce))
                Parameters.Add(New QueryParameter(OAuthTimestampKey, TimeStamp))
                Parameters.Add(New QueryParameter(OAuthSignatureMethodKey, SignatureType))
                Parameters.Add(New QueryParameter(OAuthConsumerKeyKey, ConsumerKey))
            End With

            If String.IsNullOrEmpty(Token) = False Then
                Parameters.Add(New QueryParameter(OAuthTokenKey, Token))
            End If

            If String.IsNullOrEmpty(CallbackUrl) = False Then
                Parameters.Add(New QueryParameter(OAuthCallbackKey, OAuthUrlEncode(CallbackUrl)))
            End If

            If String.IsNullOrEmpty(Verifier) = False Then
                Parameters.Add(New QueryParameter(OAuthVerifier, Verifier))
            End If

            Parameters.Sort(New QueryParameterComparer)

            NormalizedURL = String.Format("{0}://{1}", URL.Scheme, URL.Host)
            If Not ((URL.Scheme = "http" And URL.Port = 80) Or (URL.Scheme = "https" And URL.Port = 443)) Then
                NormalizedURL &= ":" + URL.Port.ToString
            End If

            NormalizedURL &= URL.AbsolutePath
            NormalizedRequestParameters = NormalizeRequestParameters(Parameters)
            Dim SignatureBase As New StringBuilder()
            With SignatureBase
                .AppendFormat("{0}&", HTTPMethod.ToUpper())
                .AppendFormat("{0}&", OAuthUrlEncode(NormalizedURL))
                .AppendFormat("{0}", OAuthUrlEncode(NormalizedRequestParameters))
            End With

            Return SignatureBase.ToString
        End Function

        Public Function GenerateSignatureUsingHash(ByVal SignatureBase As String, ByVal Hash As HashAlgorithm) As String
            Return ComputeHash(Hash, SignatureBase)
        End Function

        Public Function GenerateSignature(ByVal URL As Uri, ByVal ConsumerKey As String, ByVal ConsumerSecret As String, ByVal Token As String, ByVal TokenSecret As String, ByVal HTTPMethod As String, ByVal TimeStamp As String, ByVal Nonce As String, ByRef NormalizedUrl As String, ByRef NormalizedRequestParameters As String, ByVal CallbackUrl As String, ByVal Verifier As String) As String
            Return GenerateSignature(URL, ConsumerKey, ConsumerSecret, Token, TokenSecret, HTTPMethod, TimeStamp, Nonce, SignatureTypes.HMACSHA1, NormalizedUrl, NormalizedRequestParameters, CallbackUrl, Verifier)
        End Function

        Public Function GenerateSignature(ByVal url As Uri, ByVal ConsumerKey As String, ByVal ConsumerSecret As String, ByVal Token As String, ByVal TokenSecret As String, ByVal HTTPMethod As String, ByVal TimeStamp As String, ByVal Nonce As String, ByVal SignatureType As SignatureTypes, ByRef NormalizedUrl As String, ByRef NormalizedRequestParameters As String, ByVal CallbackUrl As String, ByVal Verifier As String) As String
            NormalizedUrl = Nothing
            NormalizedRequestParameters = Nothing

            Select Case SignatureType
                Case SignatureTypes.PLAINTEXT
                    Return HttpUtility.UrlEncode(String.Format("{0}&{1}", ConsumerSecret, TokenSecret))

                Case SignatureTypes.HMACSHA1
                    Dim SignatureBase As String = GenerateSignatureBase(url, ConsumerKey, Token, TokenSecret, HTTPMethod, TimeStamp, Nonce, HMACSHA1SignatureType, NormalizedUrl, NormalizedRequestParameters, CallbackUrl, Verifier)

                    Dim hmacsha1 As New HMACSHA1()
                    Dim ts As String = String.Empty
                    If String.IsNullOrEmpty(TokenSecret) Then
                        ts = String.Empty
                    Else
                        ts = OAuthUrlEncode(TokenSecret)
                    End If
                    hmacsha1.Key = Encoding.ASCII.GetBytes(String.Format("{0}&{1}", OAuthUrlEncode(ConsumerSecret), ts))
                    Return GenerateSignatureUsingHash(SignatureBase, hmacsha1)

                Case SignatureTypes.RSASHA1
                    Throw New NotImplementedException()

                Case Else
                    Throw New ArgumentException("Unknown signature type", "signatureType")
            End Select
        End Function

        Public Overridable Function GenerateTimeStamp() As String
            ' Default implementation of UNIX time of the current UTC time
            Dim ts As TimeSpan = DateTime.UtcNow - New DateTime(1970, 1, 1, 0, 0, 0, 0)
            Return Convert.ToInt64(ts.TotalSeconds).ToString()
        End Function

        Public Overridable Function GenerateNonce() As String
            Return Guid.NewGuid().ToString.Replace("-", "")
        End Function
    End Class
End Namespace







