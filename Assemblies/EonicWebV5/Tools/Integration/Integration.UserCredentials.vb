Option Strict Off
Option Explicit On

Imports System
Imports System.Web
Imports System.Xml

Namespace Integration

    ''' <summary>
    ''' Credentials are a simple hashtable of settings that indicate string settings intended to
    ''' represent tokens or authorisation for a user and that provider
    ''' Ideally this would be serializable but I didn't have time to make the serialization work for inheritted classes
    ''' </summary>
    ''' <remarks></remarks>
    Public Class UserCredentials

        Private _name As String = "Generic"

        Private _settings As Hashtable

        Private _permissions As XmlElement


        Public Sub New(ByVal providerName As String)
            _name = providerName
            _settings = New Hashtable
        End Sub

        Public Sub New(ByVal providerName As String, ByVal directoryInstance As XmlElement)
            _name = providerName
            _settings = New Hashtable
            Try
                If directoryInstance.SelectSingleNode("//cDirXml/Credentials[@provider='" & providerName & "']") IsNot Nothing Then
                    Deserialize(directoryInstance.SelectSingleNode("//cDirXml/Credentials[@provider='" & providerName & "']"))
                End If
            Catch ex As Exception

            End Try
        End Sub


        Public ReadOnly Property Provider() As String
            Get
                Return _name
            End Get
        End Property

        Public Sub AddSetting(ByVal key As String, ByVal value As String)
            If _settings.ContainsKey(key) Then _settings.Remove(key)
            _settings.Add(key, value)
        End Sub

        Public Function GetSetting(ByVal key As String) As String
            Return IIf(_settings.ContainsKey(key), _settings(key).ToString, "")
        End Function

        Public Function Serialize() As XmlElement
            Dim doc As New XmlDocument
            Dim element As XmlElement
            Dim root As XmlElement = doc.CreateElement("Credentials")
            root.SetAttribute("provider", Provider())
            For Each key As Object In _settings.Keys
                element = doc.CreateElement(key.ToString())
                element.InnerText = _settings(key)
                root.AppendChild(element)
            Next
            ' Add the permissions node
            If _permissions IsNot Nothing Then
                root.AppendChild(doc.ImportNode(_permissions, True))
            End If

            Return root
        End Function

        ''' <summary>
        ''' Serializes the UserCredentials to an XML node under the directory instance.
        ''' Replaces any existing node for this provider
        ''' </summary>
        ''' <param name="directoryInstance"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SerializeToDirectoryInstance(ByRef directoryInstance As XmlElement) As Boolean

            Dim serializationComplete As Boolean = False

            Dim directoryExtendedXml As XmlElement = directoryInstance.SelectSingleNode("//cDirXml")

            If directoryExtendedXml Is Nothing Then
                Throw New Exception("Could not seriliaze the credentials as there is no directory instance loaded.")
            Else
                Dim serializedCredentials As XmlElement = Serialize()
                Dim existingCredentials As XmlElement = directoryExtendedXml.SelectSingleNode("Credentials[@provider='" & Me.Provider & "']")

                If existingCredentials IsNot Nothing Then
                    directoryExtendedXml.ReplaceChild(directoryExtendedXml.OwnerDocument.ImportNode(serializedCredentials, True), existingCredentials)
                Else
                    directoryExtendedXml.AppendChild(directoryExtendedXml.OwnerDocument.ImportNode(serializedCredentials, True))
                End If

                serializationComplete = True
            End If

            Return serializationComplete

        End Function

        Public Function RemoveFromDirectoryInstance(ByRef directoryInstance As XmlElement) As Boolean

            Dim removalComplete As Boolean = False

            Dim directoryExtendedXml As XmlElement = directoryInstance.SelectSingleNode("//cDirXml")

            If directoryExtendedXml Is Nothing Then
                Throw New Exception("Could not remove the credentials as there is no directory instance loaded.")
            Else
                ' Get the credentials
                Dim existingCredentials As XmlElement = directoryExtendedXml.SelectSingleNode("Credentials[@provider='" & Me.Provider & "']")
                If existingCredentials IsNot Nothing Then
                    directoryExtendedXml.RemoveChild(existingCredentials)
                End If
                removalComplete = True
            End If

            Return removalComplete

        End Function

        Public Function Deserialize(ByVal credentialsNode As XmlElement) As Boolean
            Try
                For Each setting As XmlElement In credentialsNode.ChildNodes()

                    ' Capture everything as a hashtable
                    ' Except permissions which can be saved as a node.
                    If setting.Name = "Permissions" Then
                        _permissions = setting
                    ElseIf setting.NodeType = XmlNodeType.Element Then
                        AddSetting(setting.LocalName, setting.InnerText)
                    End If
                Next
                Return True
            Catch ex As Exception
                Return False
            End Try
        End Function

    End Class


End Namespace
