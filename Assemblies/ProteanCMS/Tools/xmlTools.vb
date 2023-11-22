
Imports System.Xml
Imports System.Web
Imports System.IO
Imports System.Xml.XPath
Imports System.Text.RegularExpressions
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports Protean.stdTools

Partial Public Module xmlTools

    Public Function getXpathFromQueryXml(ByVal oInstance As XmlElement, Optional ByVal sXsltPath As String = "") As String


        Dim oTransform As New Protean.XmlHelper.Transform()

        Dim sWriter As IO.TextWriter = New IO.StringWriter

        If sXsltPath <> "" Then
            oTransform.Compiled = False
            oTransform.XSLFile = goServer.MapPath(sXsltPath)
            Dim oXML As New XmlDocument
            oXML.InnerXml = oInstance.OuterXml
            oTransform.Process(oXML, sWriter)
            'Run transformation


            oInstance.InnerXml = sWriter.ToString()
            sWriter.Close()
            sWriter = Nothing

        End If

        'build an xpath query based on the xform instance
        'Example:

        '<instance>
        '<Query ewCmd="xpathSearch" xPathMask="//*[VARating>$RequiredVA and Uptime>$Uptime and Format=$Format]">
        '<RequiredVA>100</RequiredVA>
        '<Redundancy>Yes</Redundancy>
        '<Uptime>20</Uptime>
        '<Format>Rack</Format>
        '</Query>
        '</instance>

        Dim sXpath As String

        ':TODO [TS] handle the instance tranform if xsl provided

        sXpath = oInstance.SelectSingleNode("Query/@xPathMask").InnerText
        Dim oElmt As XmlElement
        For Each oElmt In oInstance.SelectNodes("Query/*")
            'step through each of the child nodes and replace values in the xPathMask
            sXpath = Replace(sXpath, "$" & oElmt.Name, """" & oElmt.InnerText & """")
        Next

        Return sXpath

    End Function

End Module


