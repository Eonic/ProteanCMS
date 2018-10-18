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


Public Class SoapClient

    Public results As XmlDocument = New XmlDocument
    Public query As Object
    Public ServiceUrl As String
    Public ServiceNamespace As String

    Public moCtx As System.Web.HttpContext = System.Web.HttpContext.Current
    Public goRequest As System.Web.HttpRequest = moCtx.Request

    Public Shadows mcModuleName As String = "Eonic.SoapClient"

    Public Sub SendSoapRequest(ByVal actionName As String, ByVal soapBody As String)

        Dim soapRequest As WebRequest
        Dim cProcessInfo As String = "sendSoapRequest"

        Try
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
            'check for fullpath
            If Not InStr(ServiceUrl, "http") = 1 Then
                If LCase(goRequest.ServerVariables("HTTPS")) = "on" Then
                    ServiceUrl = "https://" & goRequest.ServerVariables("SERVER_NAME") & ServiceUrl
                Else
                    ServiceUrl = "http://" & goRequest.ServerVariables("SERVER_NAME") & ServiceUrl
                End If
            End If

            soapRequest = WebRequest.Create(ServiceUrl)

            soapRequest.Headers.Add("SOAPAction", "http://www.eonic.co.uk/ewcommon/Services/" & actionName)
            soapRequest.ContentType = "text/xml; charset=UTF-8"

            ' soapRequest.Method = "POST"

            Dim serviceRequest As HttpWebRequest = soapRequest

            soapBody = Replace(soapBody, "xmlns=""""", "")
            serviceRequest.ContentType = "text/xml; charset=UTF-8"
            serviceRequest.Accept = "text/xml"
            serviceRequest.Method = "POST"
            serviceRequest.Headers("SessionReferer") = moCtx.Session("Referrer")
            addSoap(serviceRequest, soapBody)

            results.LoadXml(ReturnSoapResponse(serviceRequest))

        Catch ex As Exception
            returnException(mcModuleName, "sendSoapRequest", ex, "", cProcessInfo, gbDebug)
        End Try
    End Sub

    Private Sub addSoap(ByVal serviceRequest As HttpWebRequest, ByVal soapBody As String)
        Dim cProcessInfo As String = "addSoap"

        Try
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
            Dim encoding As UTF8Encoding = New UTF8Encoding
            Dim bodybytes As Byte() = encoding.GetBytes(soapBody)
            serviceRequest.ContentLength = bodybytes.Length
            Dim bodyStream As Stream = serviceRequest.GetRequestStream
            bodyStream.Write(bodybytes, 0, bodybytes.Length)
            bodyStream.Close()

        Catch ex As Exception
            returnException(mcModuleName, "addSoap", ex, "", cProcessInfo, gbDebug)
        End Try

    End Sub

    Private Function ReturnSoapResponse(ByVal serviceRequest As HttpWebRequest) As String

        Dim serviceResponseStream As StreamReader = Nothing
        Dim serviceResponseBody As String
        Dim cProcessInfo As String '= "ReturnSoapResponse: " & serviceRequest.Address.ToString & " - " & serviceRequest.Method & " https:" & goRequest.ServerVariables("HTTPS")
        Try
            Dim servResponse As WebResponse
            servResponse = serviceRequest.GetResponse
            Dim serviceResponse As HttpWebResponse = servResponse
            'serviceResponseStream = New StreamReader(serviceResponse.GetResponseStream, System.Text.Encoding.ASCII)
            serviceResponseStream = New StreamReader(serviceResponse.GetResponseStream, System.Text.Encoding.UTF8)

        Catch ex As WebException

            ' Dim serviceResponse As HttpWebResponse = ex.Response
            ' serviceResponseStream = New StreamReader(serviceResponse.GetResponseStream, System.Text.Encoding.ASCII)
            returnException(mcModuleName, "addSoap", ex, "", cProcessInfo, gbDebug)

        Catch ex As Exception

            'Return ex.Message.tostring
            returnException(mcModuleName, "addSoap", ex, "", cProcessInfo, gbDebug)

        End Try
        serviceResponseBody = serviceResponseStream.ReadToEnd
        serviceResponseStream.Close()
        Return serviceResponseBody

    End Function

    Function getSoapEnvelope(ByVal oBodyElmt As XmlElement) As XmlDocument

        Dim oXml As XmlDocument
        Dim sEnvelope As String
        Dim cProcessInfo As String = "getSoapEnvelope"

        Try

            sEnvelope = "<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><soap:Body/></soap:Envelope>"

            oXml = New XmlDocument
            oXml.CreateXmlDeclaration("1.0", "UTF-8", "yes")
            oXml.LoadXml(sEnvelope)
            oXml.SelectSingleNode("soap:Envelope/soap:Body").AppendChild(oBodyElmt)

            Return oXml

        Catch ex As Exception
            returnException(mcModuleName, "getSoapEnvelope", ex, "", cProcessInfo, gbDebug)
            Return Nothing
        End Try
    End Function

    Function getSoapEnvelope(ByVal sBodyElmt As String) As XmlDocument

        Dim oXml As XmlDocument
        Dim sEnvelope As String
        Dim cProcessInfo As String = "getSoapEnvelope"

        Try
            sEnvelope = "<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><soap:Body/></soap:Envelope>"

            oXml = New XmlDocument
            oXml.CreateXmlDeclaration("1.0", "UTF-8", "yes")
            oXml.LoadXml(sEnvelope)

            'add the soap namespace to the nametable of the xmlDocument to allow xpath to query the namespace
            Dim nsmgr As XmlNamespaceManager = New XmlNamespaceManager(oXml.NameTable)
            nsmgr.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/")

            oXml.SelectSingleNode("/soap:Envelope/soap:Body", nsmgr).InnerXml = sBodyElmt

            Return oXml

        Catch ex As Exception
            returnException(mcModuleName, "getSoapEnvelope", ex, "", cProcessInfo, gbDebug)
            Return Nothing
        End Try
    End Function



End Class
