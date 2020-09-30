Imports VB = Microsoft.VisualBasic
Imports System.Xml
Imports System.Web
Imports System.IO
Imports System.Xml.XPath
Imports System.Text.RegularExpressions
Imports System.Reflection
Imports System.Web.Configuration
Imports System.Globalization
Imports System.Globalization.CultureInfo
Imports System.Windows.Media.Imaging
Imports System
Imports System.Runtime.InteropServices

Imports BundleTransformer.Core.Builders
Imports BundleTransformer.Core.Orderers
Imports BundleTransformer.Core.Resolvers
Imports BundleTransformer.Core.Transformers

Imports System.Linq
Imports System.Collections.Generic
Partial Public Module xmlTools
    Public Class ewXmlElement

        Private Const mcModuleName As String = "xmlTools.ewXmlElement"
        Private BaseXmlElement As XmlElement

        Public Sub New(xmlElement As XmlElement)
            Try
                BaseXmlElement = xmlElement
            Catch ex As Exception
                ' returnException(mcModuleName, "AddElement", ex, "", "", gbDebug)
            End Try
        End Sub

        Property XmlElement() As XmlElement
            Get
                Return BaseXmlElement
            End Get
            Set(ByVal oldElmt As XmlElement)
                BaseXmlElement = oldElmt
            End Set
        End Property

        Public Function AddElement(localName As String, Optional innerText As String = Nothing) As ewXmlElement
            Try
                Dim newElmt As New ewXmlElement(BaseXmlElement.OwnerDocument.CreateElement(localName))
                If Not innerText Is Nothing Then
                    newElmt.XmlElement.InnerText = innerText
                End If
                BaseXmlElement.AppendChild(newElmt.XmlElement)
                Return newElmt
            Catch ex As Exception
                '  returnException(mcModuleName, "AddElement", ex, "", "", gbDebug)
                Return Nothing
            End Try
        End Function

        Public Function AddMenuItem(name As String, id As String, url As String, Optional dislayName As String = Nothing, Optional description As String = Nothing, Optional contentCount As Integer = Nothing) As ewXmlElement
            Try
                Dim newElmt As ewXmlElement = Me.AddElement("MenuItem")
                newElmt.XmlElement.SetAttribute("id", id)
                newElmt.XmlElement.SetAttribute("name", name)
                newElmt.XmlElement.SetAttribute("url", url)
                If Not contentCount = Nothing Then
                    newElmt.XmlElement.SetAttribute("contentCount", contentCount)
                End If
                newElmt.AddElement("DisplayName", dislayName)
                newElmt.AddElement("Description", description)
                Return newElmt
            Catch ex As Exception
                '  returnException(mcModuleName, "AddElement", ex, "", "", gbDebug)
                Return Nothing
            End Try
        End Function

    End Class

End Module
