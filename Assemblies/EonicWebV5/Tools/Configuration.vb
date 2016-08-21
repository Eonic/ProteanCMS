Imports System.Xml
Imports System.Xml.Xpath
Imports System.Xml.Serialization
Imports System.Configuration
Imports System.Web.Configuration
Imports System


''' <summary>
''' Generic web configuration section retrieval handler.
''' Methods are all shared.
''' The idea is that Eonic.Config can be called by individual Eonic classes that offer shared config retrieval.
''' Config doens't have to be part of instantiated classes, meaning that it can be used without the other usual dependencies
''' that might not be there - e.g. HttpContext.Current
''' </summary>
''' <remarks></remarks>
Public Class Config

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="key"></param>
    ''' <param name="sectionName"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Shared Function Value(ByVal key As String, ByVal sectionName As String) As String

        Dim returnValue As String = ""
        Try
            Dim section As System.Collections.Specialized.NameValueCollection = ConfigSection(sectionName)
            If section IsNot Nothing Then
                returnValue = CStr(section(key))
            End If
        Catch ex As Exception
            returnValue = ""
        End Try
        Return returnValue
    End Function

    Shared Function ConfigSection(ByVal section As String) As System.Collections.Specialized.NameValueCollection
        Return WebConfigurationManager.GetWebApplicationSection(section)
    End Function

    Shared Function UpdateConfigValue(ByRef myWeb As Eonic.Web, ByVal configPath As String, ByVal name As String, ByVal value As String) As Boolean

        Try


            'update config values
            Dim oCfg As System.Configuration.Configuration = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/")
            Dim oCgfSect As System.Configuration.DefaultSection = oCfg.GetSection(configPath)
            Dim oImp As Eonic.Tools.Security.Impersonate = New Eonic.Tools.Security.Impersonate
            If oImp.ImpersonateValidUser(myWeb.moConfig("AdminAcct"), myWeb.moConfig("AdminDomain"), myWeb.moConfig("AdminPassword"), , myWeb.moConfig("AdminGroup")) Then
                Dim oConfigDoc As New XmlDocument
                oConfigDoc.LoadXml(oCgfSect.SectionInformation.GetRawXml)
                Dim oelmt As XmlElement
                oelmt = oConfigDoc.DocumentElement.SelectSingleNode("add[@key='" & name & "']")
                If Not oelmt Is Nothing Then
                    oelmt.SetAttribute("value", value)
                Else
                    oelmt = oConfigDoc.CreateElement("add")
                    oelmt.SetAttribute("key", name)
                    oelmt.SetAttribute("value", value)
                    oConfigDoc.DocumentElement.AppendChild(oelmt)
                End If
                oCgfSect.SectionInformation.RestartOnExternalChanges = False
                oCgfSect.SectionInformation.SetRawXml(oConfigDoc.DocumentElement.OuterXml)
                oCfg.Save()
            End If
            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function




End Class


Public Class XmlSectionHandler
    Implements IConfigurationSectionHandler


    Public Function Create(ByVal parent As Object, ByVal configContext As Object, ByVal section As System.Xml.XmlNode) As Object _
            Implements IConfigurationSectionHandler.Create
        PerfMon.Log("XmlSectionHandler", "Create")
        Return section

    End Function

End Class

Public Class ProviderSectionHandler
    Inherits ConfigurationSection
    <ConfigurationProperty("providers")> _
    Public ReadOnly Property Providers() As ProviderSettingsCollection
        Get
            Return DirectCast(MyBase.Item("providers"), ProviderSettingsCollection)
        End Get
    End Property

    <ConfigurationProperty("default", DefaultValue:="EonicProvider")> _
    Public Property [Default]() As String
        Get
            Return DirectCast(MyBase.Item("default"), String)
        End Get
        Set(ByVal value As String)
            MyBase.Item("default") = value
        End Set
    End Property
End Class



''' <summary>
''' Configuration section handler that deserializes configuration settings to an object.
''' </summary>
''' <remarks>The root node must have a type attribute defining the type to deserialize to.</remarks>
''' 

Public Class XmlSerializerSectionHandler
    Implements IConfigurationSectionHandler

    Public Function Create(ByVal parent As Object, ByVal configContext As Object, ByVal section As System.Xml.XmlNode) As Object _
        Implements IConfigurationSectionHandler.Create
        PerfMon.Log("XmlSerializerSectionHandler", "Create")
        '-- get the name of the type from the type= attribute on the root node
        Dim xpn As XPathNavigator = section.CreateNavigator
        Dim TypeName As String = xpn.Evaluate("string(@type)").ToString
        If TypeName = "" Then
            Throw New ConfigurationErrorsException( _
                "The type attribute is not present on the root node of " & _
                "the <" & section.Name & "> configuration section ", _
                section)
        End If

        Dim rootClass As String = xpn.Evaluate("string(@rootClass)").ToString

        Dim path As String = xpn.Evaluate("string(@path)").ToString

        '-- make sure this string evaluates to a valid type
        Dim t As Type = Type.GetType(TypeName)
        If t Is Nothing Then
            Throw New ConfigurationErrorsException( _
                "The type attribute '" & TypeName & "' specified in the root node of the " & _
                "the <" & section.Name & "> configuration section " & _
                "is not a valid type.", section)
        End If
        Dim xs As XmlSerializer = New XmlSerializer(t)

        '-- attempt to deserialize an object of this type from the provided XML section
        Dim xnr As New XmlNodeReader(section)
        Try
            Return xs.Deserialize(xnr)
        Catch ex As Exception
            Dim s As String = ex.Message
            Dim innerException As Exception = ex.InnerException
            Do While Not innerException Is Nothing
                s &= " " & innerException.Message
                innerException = innerException.InnerException
            Loop
            Throw New ConfigurationErrorsException( _
                "Unable to deserialize an object of type '" & TypeName & "' from " & _
                "the <" & section.Name & "> configuration section: " & s, _
                ex, section)
        End Try
    End Function

End Class
