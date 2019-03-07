Imports System
Imports System.Xml
Imports System.Xml.Serialization
Imports System.Text
Imports System.io
Imports System.Net


Public Class SoapClient
#Region "Declarations"
    Public oResults As SoapResponse = New SoapResponse
    Private cServiceUrl As String
    Private cServiceNamespace As String '"http://www.eonic.co.uk/ewcommon/Services/"
    Private cActionName As String
    Private bRemoveReturnEnvelope As Boolean = False
    Public Event OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)
    Private Const mcModuleName As String = "Eonic.Tools.SoapClient"
    Private bErrorReturned As Boolean = False
    Public moSession As System.Web.SessionState.HttpSessionState
    Public Headers As New Dictionary(Of String, String)


#End Region

#Region "Properties"
    Public Property Url() As String
        Get
            Return cServiceUrl
        End Get
        Set(ByVal value As String)
            cServiceUrl = value
        End Set
    End Property

    Public Property [Namespace]() As String
        Get
            Return cServiceNamespace
        End Get
        Set(ByVal value As String)
            If Not value.Substring(value.Length - 1, 1) = "/" Then value &= "/"
            cServiceNamespace = value
        End Set
    End Property

    Public Property Action() As String
        Get
            Return cActionName
        End Get
        Set(ByVal value As String)
            cActionName = value
        End Set
    End Property

    Public Property RemoveReturnSoapEnvelope() As Boolean
        Get
            Return bRemoveReturnEnvelope
        End Get
        Set(ByVal value As Boolean)
            bRemoveReturnEnvelope = value
        End Set
    End Property

    Public ReadOnly Property ErrorReturned() As Boolean
        Get
            Return bErrorReturned
        End Get
    End Property

#End Region

#Region "Private Procedures"


    Private Sub SendSoapRequest(ByVal soapBody As String)

        Dim soapRequest As System.Net.WebRequest
        Dim cProcessInfo As String = "sendSoapRequest"

        Try
            'reset the results so we don't return the same in a loop.
            oResults = Nothing
            oResults = New SoapResponse

            ' If cServiceUrl.EndsWith(cActionName) Then
            soapRequest = System.Net.WebRequest.Create(cServiceUrl)
            ' Else
            '   soapRequest = WebRequest.Create(cServiceUrl & "/" & cActionName)
            ' End If

            soapRequest.Timeout = System.Threading.Timeout.Infinite

            Dim namespacestring As String = [Namespace] & cActionName

            soapRequest.Headers.Add("SOAPAction", namespacestring)

            'Moved into OpenQuote not needed here
            'If Not moSession Is Nothing Then
            '    If moSession("JSESSIONID") <> "" Then
            '        soapRequest.Headers.Add("Cookie", "JSESSIONID=" & moSession("JSESSIONID"))
            '    End If
            'End If

            Dim serviceRequest As HttpWebRequest = soapRequest

            serviceRequest.ContentType = "text/Xml"

            Dim pair As KeyValuePair(Of String, String)
            For Each pair In Me.Headers
                serviceRequest.Headers.Add(pair.Key, pair.Value)
            Next

            addSoap(serviceRequest, soapBody)

            ProduceReturn(ReturnSoapResponse(serviceRequest))

        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "SendSoapRequest", ex, cProcessInfo))
        End Try
    End Sub

    Private Sub addSoap(ByVal serviceRequest As HttpWebRequest, ByVal soapBody As String)
        Dim cProcessInfo As String = "addSoap"

        Try

            serviceRequest.Method = "POST"
            Dim encoding As UTF8Encoding = New UTF8Encoding
            Dim bodybytes As Byte() = encoding.GetBytes(soapBody)
            serviceRequest.ContentLength = bodybytes.Length
            Dim bodyStream As Stream = serviceRequest.GetRequestStream
            bodyStream.Write(bodybytes, 0, bodybytes.Length)
            bodyStream.Close()

        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "addSoap", ex, cProcessInfo))
        End Try

    End Sub

    Private Function ReturnSoapResponse(ByVal serviceRequest As HttpWebRequest) As String

        Dim serviceResponseStream As StreamReader = Nothing
        Dim serviceResponseBody As String
        Dim cProcessInfo As String = "ReturnSoapResponse"
        Try
            Dim servResponse As WebResponse
            serviceRequest.Timeout = System.Threading.Timeout.Infinite

            servResponse = serviceRequest.GetResponse


            Dim serviceResponse As HttpWebResponse = servResponse
            serviceResponseStream = New StreamReader(serviceResponse.GetResponseStream, System.Text.Encoding.UTF8)

            If Not moSession Is Nothing Then
                GetCookieHeaders(serviceResponse)
            End If

            GetErrorHeaders(serviceResponse)

        Catch ex As Exception

            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "ReturnSoapRequest", ex, cProcessInfo))

        End Try

        If serviceResponseStream Is Nothing Then
            serviceResponseBody = Nothing '"<error>Response Stream Error</error>" 'Nothing is required for Openquote please don't change
        Else
            serviceResponseBody = serviceResponseStream.ReadToEnd
            serviceResponseStream.Close()
        End If
        Return serviceResponseBody

    End Function

    Private Sub GetErrorHeaders(ByVal serviceResponse As HttpWebResponse)
        Try
            Dim i As Integer
            bErrorReturned = False
            For i = 0 To serviceResponse.Headers.Count - 1
                Select Case serviceResponse.Headers.Keys(i)
                    Case "Error"
                        bErrorReturned = True
                        Exit Sub
                End Select
            Next
        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "GetErrorHeaders", ex, ""))
        End Try
    End Sub

    Private Sub GetCookieHeaders(ByVal serviceResponse As HttpWebResponse)
        Try
            Dim i As Integer
            bErrorReturned = False
            For i = 0 To serviceResponse.Headers.Count - 1
                Select Case serviceResponse.Headers.Keys(i)
                    Case "Set-Cookie"
                        Dim aCookies As String() = serviceResponse.Headers.GetValues("Set-Cookie")
                        Dim j As Long
                        Dim k As Long

                        For j = 0 To UBound(aCookies)
                            Dim aCookies2 As String() = Split(aCookies(j), ";")
                            For k = 0 To UBound(aCookies2)
                                Dim aCookies3 As String() = Split(aCookies2(k), "=")
                                moSession(aCookies3(0)) = aCookies3(1)
                            Next
                        Next
                        Exit Sub
                End Select
            Next
        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "GetErrorHeaders", ex, ""))
        End Try
    End Sub

    Public Function getSoapEnvelope(ByVal sBodyElmt As String) As XmlDocument

        Dim oXml As XmlDocument
        Dim sEnvelope As String
        Dim cProcessInfo As String = "getSoapEnvelope"

        Try
            sEnvelope = "<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsd=""http://www.w3.org/2001/XmlSchema"" xmlns:xsi=""http://www.w3.org/2001/XmlSchema-instance""><soap:Body/></soap:Envelope>"

            oXml = New XmlDocument
            oXml.CreateXmlDeclaration("1.0", "UTF-8", "yes")
            oXml.LoadXml(sEnvelope)

            'add the soap namespace to the nametable of the XmlDocument to allow xpath to query the namespace
            Dim nsmgr As XmlNamespaceManager = New XmlNamespaceManager(oXml.NameTable)
            nsmgr.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/")

            oXml.SelectSingleNode("/soap:Envelope/soap:Body", nsmgr).InnerXml = sBodyElmt
            oXml.InnerXml = "<?xml version=""1.0"" encoding=""utf-8""?>" & oXml.InnerXml
            Return oXml

        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getSoapEnvelope", ex, cProcessInfo))
            Return Nothing
        End Try
    End Function

    Private Sub ProduceReturn(ByVal cReturn As String)
        Try
            If Me.RemoveReturnSoapEnvelope Then
                cReturn = Replace(cReturn, "soap:Envelope", "soapEnvelope")
                cReturn = Replace(cReturn, "soap:Body", "soapBody")

                'cReturn = Replace(cReturn, "xmlns", "exemelnamespace")
                'cReturn = Replace(cReturn, ":soap", "")
                Dim oNewXml As New XmlDocument
                oNewXml.LoadXml(cReturn)
                oResults.LoadXml(oNewXml.SelectSingleNode("soapEnvelope/soapBody").InnerXml)
                oResults.InnerXml = Replace(oResults.InnerXml, "xmlns", "exemelnamespace")
            Else
                'cReturn = Replace(cReturn, "env:", "")
                'cReturn = Replace(cReturn, "ns1:", "")
                oResults.LoadXml(cReturn)
            End If
        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "SendRequest", ex, ""))
        End Try
    End Sub



#End Region

    Public Function SendRequest(ByVal oSoapBody As String) As String
        Try
            oSoapBody = Replace(oSoapBody, "exemelnamespace", "xmlns")
            oSoapBody = Me.getSoapEnvelope(oSoapBody).OuterXml
            Me.SendSoapRequest(oSoapBody)
            Return Me.oResults.OuterXml
        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "SendRequest", ex, ""))
            Return ""
        End Try
    End Function

    Public Async Function SendRequestAsync(ByVal oSoapBody As String) As Threading.Tasks.Task(Of String)
        Try
            oSoapBody = Replace(oSoapBody, "exemelnamespace", "xmlns")
            oSoapBody = Me.getSoapEnvelope(oSoapBody).OuterXml
            Me.SendSoapRequest(oSoapBody)
            Return Me.oResults.OuterXml
        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "SendRequest", ex, ""))
            Return ""
        End Try
    End Function


    Public Class SoapResponse
        Inherits XmlDocument

        Public ReadOnly Property getBody() As XmlElement
            Get
                Return Me.SelectSingleNode("Body")
            End Get
        End Property


    End Class

    'Public Class headers
    '    Inherits dic
    'End Class

End Class
