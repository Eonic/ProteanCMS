Option Strict Off
Option Explicit On

Imports System
Imports System.IO
Imports System.Net
Imports System.Web.HttpUtility
Imports System.Text
Imports System.Text.Encoding



Namespace Http
    ''' <summary>
    ''' Simplified WebRequest class.
    ''' </summary>
    ''' <remarks>TODO: Needs a GET method</remarks>
    Public Class WebRequest

#Region "Declarations"

        Private _moduleName As String = "Protean.Tools.WebRequest"

        Private _contentType As String = "text/html"
        Private _contentLength As Long = 0
        Private _userAgent As String = ""
        Private _requestEncoding As New ASCIIEncoding
        Private _requestBody As String
        Private _requestBytes As Byte()
        Private _method As String = "POST"
        Private _includeResponse As Boolean = True

#End Region

#Region "Events"

        Public Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)

#End Region

#Region "Constructor"

        Public Sub New()

        End Sub


        Public Sub New(ByVal contentType As String)
            Me.ContentType() = contentType
        End Sub

        Public Sub New(ByVal contentType As String, ByVal method As String)
            Me.ContentType() = contentType
            Me.Method() = method
        End Sub

        Public Sub New(ByVal contentType As String, ByVal method As String, ByVal userAgent As String)
            Me.ContentType() = contentType
            Me.Method() = method
            Me.UserAgent() = userAgent
        End Sub

#End Region

#Region "Private Properties"

#End Region

#Region "Public Properties"

        Public Property ContentType() As String
            Get
                Return _contentType
            End Get
            Set(ByVal value As String)
                _contentType = value
            End Set
        End Property

        Public Property Method() As String
            Get
                Return _method
            End Get
            Set(ByVal value As String)
                _method = value
            End Set
        End Property

        Public Property UserAgent() As String
            Get
                Return _userAgent
            End Get
            Set(ByVal value As String)
                _userAgent = value
            End Set
        End Property

        Public Property IncludeResponse() As Boolean
            Get
                Return _includeResponse
            End Get
            Set(ByVal value As Boolean)
                _includeResponse = value
            End Set
        End Property


        Public Property RequestBody() As String
            Get
                Return _requestBody
            End Get
            Set(ByVal value As String)
                _requestBody = value
                _requestBytes = _requestEncoding.GetBytes(_requestBody)
                _contentLength = _requestBytes.Length
            End Set
        End Property

        Public ReadOnly Property ContentLength() As Integer
            Get
                Return _contentLength
            End Get
        End Property
#End Region

#Region "Private Members"

#End Region

#Region "Public Members"

        ''' <summary>
        ''' Instigate a synchronous request and response
        ''' </summary>
        ''' <param name="url">The URI to send the request to</param>
        ''' <param name="request">The request string to send</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Send(ByVal url As String, Optional ByVal request As String = "") As String

            Try
                If Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute) Then
                    Return Send(New Uri(url), request)
                Else
                    Return ""
                End If
            Catch ex As Exception
                RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(_moduleName, "Send(String)", ex, ""))
                Return ""
            End Try

        End Function

        ''' <summary>
        ''' Instigate a synchronous request and response
        ''' </summary>
        ''' <param name="url">The URI to send the request to</param>
        ''' <param name="request">The request string to send</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Send(ByVal url As Uri, Optional ByVal request As String = "") As String
            Try

                ' Create the response object
                Dim newRequest As HttpWebRequest = Nothing
                Dim newResponse As HttpWebResponse = Nothing
                Dim requestStream As Stream = Nothing
                Dim responseStream As StreamReader = Nothing
                Dim responseOutput As String = ""

                If Not (String.IsNullOrEmpty(request)) Then RequestBody = request

                If Not (Me.ContentLength = 0 And Me.Method = "POST") Then
                    ' Create the request object
                    newRequest = HttpWebRequest.Create(url)

                    ' Set the standard headers
                    newRequest.ContentType = Me.ContentType
                    newRequest.ContentLength = Me.ContentLength
                    newRequest.Method = Me.Method
                    'newRequest.MaximumAutomaticRedirections = 4
                    'newRequest.MaximumResponseHeadersLength = 4

                    ' Set the optional headers
                    If Not (String.IsNullOrEmpty(Me.UserAgent)) Then newRequest.UserAgent = Me.UserAgent

                    ' POST the request body
                    If Method = "POST" Then
                        requestStream = newRequest.GetRequestStream
                        requestStream.Write(Me._requestBytes, 0, Me.ContentLength)
                        requestStream.Close()
                    End If


                    If IncludeResponse() Then
                        ' Get the response
                        newResponse = newRequest.GetResponse()
                        requestStream = newResponse.GetResponseStream()
                        responseStream = New StreamReader(requestStream, System.Text.Encoding.UTF8)
                        responseOutput = responseStream.ReadToEnd

                        requestStream.Close()
                        responseStream.Close()
                        newResponse.Close()
                    End If

                End If

                Return responseOutput

            Catch ex As Exception
                RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(_moduleName, "Send(Uri)", ex, ""))
                Return ""
            End Try
        End Function

#End Region

    End Class

End Namespace

