Option Strict Off
Option Explicit On 

Imports System
Imports System.Diagnostics
Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports System.Xml
Imports System.Xml.Serialization
Imports System.Text
Imports System.io
Imports System.Net
Imports System.Text.RegularExpressions
Imports System.Collections.Generic
Imports System.Web.Configuration
Imports System.Web.Caching
Imports System.Net.Security
Imports Protean.stdTools

Public Class CssWebClient

    Public results As XmlDocument = New XmlDocument
    Public query As Object
    Public ServiceNamespace As String

    Public moCtx As System.Web.HttpContext
    Public goRequest As System.Web.HttpRequest
    Private msException As String

    Private cssSplit As List(Of String) = New List(Of String)
    Public ReadOnly Property CssSplits As List(Of String)
        Get
            Return cssSplit
        End Get
    End Property

    Private serviceUrls As List(Of String) = New List(Of String)
    Public Property ServiceUrlsList As List(Of String)
        Get
            Return serviceUrls
        End Get
        Set(value As List(Of String))
            serviceUrls = value
        End Set
    End Property

    Private fullCss As String
    Public ReadOnly Property FullCssFile As String
        Get
            Return fullCss
        End Get
    End Property

    Public Shadows mcModuleName As String = "Eonic.CssWebClient"

    Public Sub New(ByVal context As System.Web.HttpContext, ByRef sException As String)
        moCtx = context
        goRequest = context.Request
    End Sub


    Public Sub SendCssHttpHandlerRequest()
        Dim httpHandlerRequest As WebRequest
        Dim cProcessInfo As String = "sendHttpHandlerRequest"
        Dim moConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/web")

        Try
            For Each Serviceurl As String In ServiceUrlsList
                'check for fullpath
                Dim origServiceUrl As String = Serviceurl

                'PerfMon.Log("CssWebClient", "SendCssHttpHandlerRequest", "start-" & Serviceurl)

                If goRequest.ServerVariables("SERVER_NAME") = "localhost" And goRequest.ServerVariables("SERVER_PORT") <> "80" Then
                    Serviceurl = ":" & goRequest.ServerVariables("SERVER_PORT") & Serviceurl
                End If

                If Not InStr(Serviceurl, "http") = 1 Then
                    If LCase(goRequest.ServerVariables("HTTPS")) = "on" Then
                        Serviceurl = "https://" & goRequest.ServerVariables("SERVER_NAME") & Serviceurl
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12

                        ServicePointManager.ServerCertificateValidationCallback =
  Function(se As Object,
  cert As System.Security.Cryptography.X509Certificates.X509Certificate,
  chain As System.Security.Cryptography.X509Certificates.X509Chain,
  sslerror As System.Net.Security.SslPolicyErrors) True

                    Else
                        Serviceurl = "http://" & goRequest.ServerVariables("SERVER_NAME") & Serviceurl
                    End If
                End If

                cProcessInfo = Serviceurl

                httpHandlerRequest = WebRequest.Create(Serviceurl)

                Dim serviceRequest As HttpWebRequest = httpHandlerRequest

                Dim response As HttpWebResponse = CType(serviceRequest.GetResponse(), HttpWebResponse)
                ' PerfMon.Log("CssWebClient", "SendCssHttpHandlerRequest", "end-" & Serviceurl)
                Dim strResponse As String
                Using receiveStream As Stream = response.GetResponseStream()
                    Using readStream As New StreamReader(receiveStream, Encoding.UTF8)
                        strResponse = readStream.ReadToEnd()
                    End Using
                End Using
                strResponse = strResponse.Replace(vbLf, "")
                fullCss = strResponse

                ClearApplicationCache(origServiceUrl)
                ServicePointManager.ServerCertificateValidationCallback = Nothing
            Next
            Dim cssSplit As Integer = IIf(moConfig("cssSplit") = "", 2000, moConfig("cssSplit"))
            ComputeCSS(fullCss, cssSplit)
        Catch ex As Exception
            cProcessInfo = ex.Message
            'WE DO NOT !!!!! want this to return an html exception becuase it will throw a loop.
            'returnException(msException, mcModuleName, "SendHttpHandlerRequest", ex, "", cProcessInfo, gbDebug)
        End Try
    End Sub

    Public Sub ClearApplicationCache(Serviceurl As String)

        Dim keys As New List(Of String)()

        ' retrieve application Cache enumerator
        Dim enumerator As IDictionaryEnumerator = moCtx.Cache.GetEnumerator()

        ' copy all keys that currently exist in Cache
        While enumerator.MoveNext()
            keys.Add(enumerator.Key.ToString())
        End While

        ' delete every key from cache
        For i As Integer = 0 To keys.Count - 1
            If keys(i).Contains(Serviceurl.ToLower) Then
                moCtx.Cache.Remove(keys(i))
            End If
        Next
    End Sub

    ''' <summary>
    ''' This function is called to get around the IE9 CSS selector limit, max CSS selectors is 4096 so we need to split the file
    ''' </summary>
    ''' <param name="css">The aggregated css string</param>
    ''' <param name="maxRules">the max number of rules to contain within a css document</param>
    ''' <remarks>Currently should not risk setting the maxRules to the limit, as the regex may not find all selectors according to specification</remarks>
    Private Sub ComputeCSS(ByVal css As String, ByVal maxRules As Integer)
        Dim matches As MatchCollection = Regex.Matches(css, "\}\n\.|\}\.|,\.|,\n\.") 'this regex expression will search for ',.' OR '}.' OR '}[linefeed].'
        Dim cProcessInfo As String = "ComputeCSS"
        Try
            If matches.Count > maxRules Then
                If Not matches.Item(maxRules).Value = "}." Then
                    ComputeCSS(css, maxRules + 1) 'call this function recursively until we find a value that we can safely split on ("}.")
                Else
                    cssSplit.Add(css.Substring(0, matches.Item(maxRules).Index + 1))
                    'we have more matches than our maximum number of rules so call recursively with the remaining css
                    ComputeCSS(css.Substring(matches.Item(maxRules).Index + 1, css.Length - 1 - matches.Item(maxRules).Index), maxRules)
                End If

            Else
                'add the final piece of css to the List and exit
                cssSplit.Add(css.Substring(0, css.Length))
                Return
            End If
        Catch ex As Exception
            returnException(msException, mcModuleName, "ComputeCSS", ex, "", cProcessInfo, gbDebug)
        End Try
    End Sub

End Class
