Imports System.Configuration
Imports System.Web.Configuration

''' <summary>
''' Eonic.Web.TypeExtensions extends Eonic.Tools.TypeExtensions
''' allows handling of various scenarios that return a type.
''' </summary>
''' <remarks></remarks>
Partial Public Class Web
    Public Class TypeExtensions
        Inherits Eonic.Tools.TypeExtensions



        Shared Function TypeFromProviderName(ByVal typeFromSpecifiedAssembly As String, ByVal providerName As String, ByVal providerSection As String) As System.Type

            Dim providerConfig As Eonic.ProviderSectionHandler = WebConfigurationManager.GetWebApplicationSection(providerSection)
            If providerConfig IsNot Nothing Then
                Dim provider As ProviderSettings = providerConfig.Providers(providerName)
                If provider IsNot Nothing Then
                    Return Eonic.Tools.TypeExtensions.Type(typeFromSpecifiedAssembly, provider.Type)
                Else
                    Return Nothing
                End If
            Else
                Return Nothing
            End If

        End Function

        Overloads Shared Function Type(ByVal typeName As String, ByVal assemblyName As String, ByVal assemblyType As String, ByVal providerName As String, ByVal providerSection As String) As System.Type

            If Not String.IsNullOrEmpty(providerName) And Not String.IsNullOrEmpty(providerSection) Then
                Return TypeFromProviderName(typeName, providerName, providerSection)
            ElseIf Not String.IsNullOrEmpty(assemblyName) Then
                Return Eonic.Tools.TypeExtensions.Type(typeName, assemblyName, assemblyType)
            Else
                ' Need to avoid going back to Eonic.Tools, so just repliacte here
                Return System.Type.GetType(typeName, True, True)
            End If

        End Function

    End Class
End Class

