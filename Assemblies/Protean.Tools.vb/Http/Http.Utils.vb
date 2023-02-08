Imports System
Imports System.Text
Imports System.Web
Imports System.Web.HttpUtility
Imports System.Collections.Specialized

'' ---------------------------------------------------------
'' Class       :   Protean.Tools.Http.WebRequest
'' Author      :   Ali Granger
'' Website     :   www.eonic.co.uk
'' Description :   HTTP and Web related classes and functions.
''
'' (c) Copyright 2010 Eonic Ltd.
'' ---------------------------------------------------------

Namespace Http

    Public Enum URLShorteningService

        BitLy
        Isgd
        TinyUrl

    End Enum

    ''' <summary>
    ''' Protean.Tools.Http.Utils
    ''' Collection of shared Http/Web utils
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Utils

        Public Shared Function BuildURIFromRequest(ByVal currentRequest As HttpRequest, Optional ByVal parameters As NameValueCollection = Nothing, Optional ByVal parametersToExclude As String = "") As Uri

            Dim uriBuilder As New StringBuilder()
            Dim queryString As NameValueCollection = Nothing
            Dim exclusions As String() = parametersToExclude.Split(",")
            Try

                Dim requestUri As Uri = currentRequest.Url

                ' Build the Querystring.
                queryString = ParseQueryString(currentRequest.QueryString.ToString)

                ' Add the parameters, if specified.
                ' Parameters override existing ones
                If parameters IsNot Nothing Then

                    For Each key As String In parameters.AllKeys
                        queryString.Remove(key)
                        queryString.Add(key, parameters(key))
                    Next

                End If

                ' Check the exclusions
                For Each exclusion As String In exclusions
                    If queryString(exclusion) IsNot Nothing Then
                        queryString.Remove(exclusion)
                    End If
                Next

                ' Calculate the Absolute path from the original path (not the rewritten one)
                Dim originalPath As String = currentRequest.RawUrl
                originalPath = originalPath.Substring(0, IIf(originalPath.Contains("?"), originalPath.IndexOf("?"), originalPath.Length))

                ' Build the URI
                Return BuildURI(requestUri.Host, requestUri.Scheme & "://", originalPath, queryString.ToString())


            Catch ex As Exception
                ' If an error is encountered return Nothing
                Return Nothing
            End Try

        End Function

        Public Shared Function BuildURI(ByVal host As String, Optional ByVal protocol As String = "http://", Optional ByVal path As String = "/", Optional ByVal querystring As String = "") As Uri

            Dim uriBuilder As New StringBuilder()

            Try
                ' Build the string

                If String.IsNullOrEmpty(host) Then
                    ' Empty host
                    Return Nothing
                Else

                    uriBuilder.Append(protocol.Trim())
                    uriBuilder.Append(host.Trim().Trim("/"))
                    uriBuilder.Append("/")
                    uriBuilder.Append(path.TrimStart("/"))
                    If Not String.IsNullOrEmpty(querystring) Then
                        uriBuilder.Append("?")
                        uriBuilder.Append(querystring.TrimStart("?"))
                    End If

                    If Uri.IsWellFormedUriString(uriBuilder.ToString, UriKind.Absolute) Then
                        Return New Uri(uriBuilder.ToString())
                    Else
                        Return Nothing
                    End If
                    
                End If

            Catch ex As Exception
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="urlToShorten"></param>
        ''' <param name="shorteningService"></param>
        ''' <returns></returns>
        ''' <remarks>This is derived from TwitterVB, as we wanted to add other services.</remarks>
        Public Shared Function ShortenURL(ByVal urlToShorten As String, Optional ByVal shorteningService As URLShorteningService = URLShorteningService.TinyUrl, Optional ByVal bitlyUser As String = "", Optional ByVal bitlyKey As String = "") As String

            Dim shorteningRequest As New Tools.Http.WebRequest("text/html", "GET")

            Dim shortenedURL As String = String.Empty

            Select Case shorteningService
                Case URLShorteningService.Isgd
                    shortenedURL = shorteningRequest.Send(String.Format("http://is.gd/api.php?longurl={0}", System.Web.HttpUtility.UrlEncode(urlToShorten)))

                Case URLShorteningService.TinyUrl
                    shortenedURL = shorteningRequest.Send(String.Format("http://tinyurl.com/api-create.php?url={0}", System.Web.HttpUtility.UrlEncode(urlToShorten)))

                Case URLShorteningService.BitLy
                    shortenedURL = shorteningRequest.Send(String.Format("http://api.bit.ly/v3/shorten?login={0}&apiKey={1}&longUrl={2}&format=txt", bitlyUser, bitlyKey, urlToShorten))

            End Select

            Return shortenedURL

        End Function
    End Class



End Namespace


