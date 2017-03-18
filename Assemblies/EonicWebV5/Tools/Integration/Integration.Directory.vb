Option Strict Off
Option Explicit On

Imports System.Xml
Imports System.Reflection
Imports Eonic.Web.dbHelper
Imports System

Namespace Integration.Directory

    ''' <summary>
    ''' Abstract class for Directory based integrations
    '''
    ''' </summary>
    ''' <remarks>
    ''' Because the actual integration may differ in how it does what it does this is implemented
    ''' as an abstract class rather than an interface
    ''' The common default functionality is likely to be found in the user data retrieval and checks, but the 
    ''' actual integration processes - e.g. authentication are likely to be bespoke.
    ''' </remarks>
    Public MustInherit Class BaseProvider
        Implements IPostable

        ' Core class variables
        Protected _moduleName As String = "Eonic.Integration.Directory"
        Protected _myWeb As Eonic.Web
        Private _diagnostics As String = ""
        Protected _providerName As String


        ' Core class default behaviour variables
        Private _directoryId As Long = 0
        Private _directoryInstance As XmlElement

        ' Flags
        Private _isDirectoryItemLoaded As Boolean = False
        Private _isProviderCredentialLoaded As Boolean = False

        Protected _credentials As UserCredentials

        Public Shadows Event OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)

        Private Sub _OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)
            _diagnostics &= Microsoft.VisualBasic.vbCrLf & "Error:" & e.ToString()
            RaiseEvent OnError(sender, e)
        End Sub

        Public Sub New(ByRef aWeb As Eonic.Web)
            Try
                If aWeb Is Nothing Then Throw New ArgumentNullException("Eonic.Web is not initialised")
                _myWeb = aWeb
                _directoryId = _myWeb.mnUserId
                _moduleName &= "." & Me.Name()
                If IsAuthorisedUser Then LoadCredentials()
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(_moduleName, "New(Web)", ex, ""))
            End Try
        End Sub

        Public Sub New(ByRef aWeb As Eonic.Web, ByRef directoryId As Long)
            Try
                If aWeb Is Nothing Then Throw New ArgumentNullException("Eonic.Web is not initialised")
                _myWeb = aWeb
                _directoryId = directoryId
                _moduleName &= "." & Me.Name()
                If IsAuthorisedUser Then LoadCredentials()
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(_moduleName, "New(Web,Long)", ex, ""))
            End Try
        End Sub

        ''' <summary>
        '''  The name of the Integration
        ''' </summary>
        ''' <returns>String</returns>
        ''' <remarks></remarks>
        Private ReadOnly Property Name() As String
            Get
                Return Me.GetType().Name
            End Get
        End Property

        ''' <summary>
        ''' The id of the directory item that the integration is using
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property DirectoryId() As Long
            Get
                Return _directoryId
            End Get
            Set(ByVal value As Long)
                _directoryId = IIf(value < 0, 0, value)
            End Set
        End Property

        ''' <summary>
        ''' Checks if the Eonic.Web object has the right context to make integration calls.
        ''' At the moment it should the logged in user, or someone logged in as an admin
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property IsAuthorisedUser() As Boolean
            Get
                Return _myWeb.mbAdminMode Or (_myWeb.mnUserId = _directoryId)
            End Get
        End Property

        Public ReadOnly Property IsDirectoryItemLoaded() As Boolean
            Get
                Return _isDirectoryItemLoaded
            End Get
        End Property

        Public ReadOnly Property IsProviderCredentialLoaded() As Boolean
            Get
                Return _isProviderCredentialLoaded
            End Get
        End Property

        Function ValidateExternalAuth(ByVal ExternalId As String) As Long
            Try
                Dim oDr As SqlClient.SqlDataReader = _myWeb.moDbHelper.getDataReader("select nDirectoryId from tblDirectoryExternalAuth where cProviderName = '" & _providerName & "' and cProviderId='" & ExternalId & "'")
                Dim oRow As DataRow

                If oDr.HasRows Then
                    For Each oRow In oDr
                        Return oRow("nDirectoryId")
                    Next
                Else
                    Return 0
                End If

            Catch ex As Exception

            End Try
        End Function

        Function GetUserSchemaXml() As XmlElement

            Dim oXform As New Eonic.xForm
            oXform.load("/xforms/directory/User.xml", _myWeb.maCommonFolders)
            Return oXform.Instance.SelectSingleNode("tblDirectory/cDirXml")

        End Function
        ''' <summary>
        ''' Gets the user instance and loads the provider credentials if they exist.
        ''' If they don't exist then a default credentials object is created.
        ''' </summary>
        ''' <remarks></remarks>
        Protected Sub LoadCredentials()

            _isProviderCredentialLoaded = False

            ' Load the user
            If LoadDirectoryInstance() Then
                _credentials = New UserCredentials(Me.Name, _directoryInstance)
                _isProviderCredentialLoaded = True
            Else
                Throw New Exception("Unable to load credentials as directory instance was not loaded.")
            End If

        End Sub

        Private Function LoadDirectoryInstance() As Boolean
            Dim directoryInstance As String

            Try

                _isDirectoryItemLoaded = False

                ' Load the directory item
                directoryInstance = _myWeb.moDbHelper.getObjectInstance(objectTypes.Directory, _directoryId)

                ' Validate it
                If Not String.IsNullOrEmpty(directoryInstance) Then
                    _directoryInstance = _myWeb.moPageXml.CreateElement("instance")
                    _directoryInstance.InnerXml = directoryInstance
                    _isDirectoryItemLoaded = True
                End If

                Return _isDirectoryItemLoaded

            Catch ex As Exception
                Return False
            End Try
        End Function

        Protected Sub SaveCredentials()
            If Not _isProviderCredentialLoaded Then
                Throw New Exception("Unable to save credentials as no credentials were instantiated.")

            ElseIf Not LoadDirectoryInstance() Then
                Throw New Exception("Unable to save credentials as directory instance was not loaded.")

            Else
                If _credentials.SerializeToDirectoryInstance(_directoryInstance) Then
                    ' Save the instance.
                    _myWeb.moDbHelper.setObjectInstance(objectTypes.Directory, _directoryInstance, _directoryId)
                End If
            End If

        End Sub

        Public Sub DeleteCredentials()
            If Not _isProviderCredentialLoaded Then
                Throw New Exception("Unable to delete credentials as no credentials were instantiated.")
            ElseIf Not LoadDirectoryInstance() Then
                Throw New Exception("Unable to save credentials as directory instance was not loaded.")

            Else
                If _credentials.RemoveFromDirectoryInstance(_directoryInstance) Then
                    ' Save the instance.
                    Dim userid As Long = 0
                    Dim updateStatus As String = _myWeb.moDbHelper.setObjectInstance(objectTypes.Directory, _directoryInstance, _directoryId)
                    If Tools.CheckAndReturnStringAsNumber(updateStatus, userid, GetType(Long)) Then
                        _myWeb.AddResponse(Me.Name & ".DeleteCredentials.Success", "User account has been successfully unlinked from " & Me.Name, , ResponseType.Hint)
                    Else
                        Throw New Exception("Unable to save credentials.")
                    End If
                End If
            End If
        End Sub

        Public Overridable Sub Post(ByVal content As String, Optional ByVal trackbackUri As Uri = Nothing, Optional ByVal contentId As Long = 0) Implements IPostable.Post
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(_moduleName, "Post", New NotSupportedException("Post method is not available for this provider:" & Me.Name), ""))
        End Sub

    End Class

    ''' <summary>
    ''' Helper class for integrations
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Helper

        ' Core class variables
        Protected _moduleName As String = "Eonic.Integration.Directory.Helper"
        Protected _myWeb As Eonic.Web
        Private _diagnostics As String = ""

        Private _integrationsEnabled As Boolean = False

        Public Shadows Event OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)

        Private Sub _OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)
            _diagnostics &= Microsoft.VisualBasic.vbCrLf & "Error:" & e.ToString()
            RaiseEvent OnError(sender, e)
        End Sub

        Public Sub New(ByRef aWeb As Eonic.Web)
            Try
                If aWeb Is Nothing Then Throw New ArgumentNullException("Eonic.Web is not initialised")
                _myWeb = aWeb
                _integrationsEnabled = _myWeb.moConfig("UserIntegrations") = "on"
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(_moduleName, "New(Web)", ex, ""))
            End Try
        End Sub

        Public ReadOnly Property Enabled() As Boolean
            Get
                Return _integrationsEnabled
            End Get
        End Property

        Public ReadOnly Property ContentCheckboxEnabled() As Boolean
            Get
                Return Not (_myWeb.moConfig("UserIntegrationsContentCheckbox") = "off")
            End Get
        End Property

        Private ReadOnly Property ContentCheckboxContentTypes() As String
            Get
                Dim types As String = _myWeb.moConfig("UserIntegrationsContentCheckboxTypes") & ""
                Return types.Trim
            End Get
        End Property


        ''' <summary>
        ''' Takes a content ID, and works out if it can post this, based on the current user id
        ''' </summary>
        ''' <param name="contentId">The content ID to post</param>
        ''' <param name="isUpdatedContent">Indicates if the content already exists and is being updated</param>
        ''' <remarks></remarks>
        Public Sub PostContent(ByVal contentId As Long, Optional ByVal isUpdatedContent As Boolean = True)

            Dim postableContentDocument As New XmlDocument

            Try
                If Enabled() Then

                    ' Check the user
                    If _myWeb.mnUserId > 0 Then

                        Dim userXml As XmlElement = postableContentDocument.CreateElement("User")
                        Dim userInnerXml As String = _myWeb.moDbHelper.getObjectInstance(objectTypes.Directory, _myWeb.mnUserId)
                        userXml.InnerXml = userInnerXml

                        ' Get the content brief
                        Dim content As XmlElement = _myWeb.GetContentBriefXml(, contentId)
                        Dim contentSchema As String = ""
                        Tools.Xml.NodeState(content, "//cContentSchemaName", , , , , , contentSchema)

                        ' Work out whether we have permission to do this.
                        ' There are two places to look for permissions
                        '    Credentials/Permissions/Permission[@contentType @add @edit]
                        '      where ContentType matches and add or edit is true for the isUpdatedContent
                        '    Or if Request posts "overrideAutomatedIntegrationPostings" then we look at 
                        '   the Request item "postToIntegrations" to see which integrations to select.
                        Dim integrationsSelector As String = ""
                        If _myWeb.moRequest("overrideAutomatedIntegrationPostings") = "true" Then

                            ' Override the settings - search for postToIntegrations
                            For Each post As String In (_myWeb.moRequest("postToIntegrations") & "").Split(",")
                                If Not String.IsNullOrEmpty(post) Then integrationsSelector &= "@provider='" & post & "' or "
                            Next
                            If Not String.IsNullOrEmpty(integrationsSelector) Then
                                integrationsSelector = "[" & integrationsSelector.Substring(0, integrationsSelector.Length - 4) & "]"
                            End If

                        Else

                            ' Match up the permissions for this scenario
                            integrationsSelector = "[Permissions/Permission[@type='postContent' and @contentType='" & contentSchema & "' and @" & IIf(isUpdatedContent, "edit", "add") & "='true']]"
                        End If

                        ' Check for the presence of Credentials
                        Dim credentials As XmlNodeList = userXml.SelectNodes("//Credentials" & integrationsSelector)
                        If credentials.Count > 0 And Not String.IsNullOrEmpty(integrationsSelector) Then

                            ' Check if the content is on a page that can be viewed by non-authenticated users.
                            Dim tempAdminMode As Boolean = _myWeb.mbAdminMode
                            _myWeb.mbAdminMode = False

                            ' Check if the content is live
                            If _myWeb.moDbHelper.IsAuditedObjectLive(objectTypes.Directory, contentId) Then

                                ' Get the primary location
                                Dim primaryLocation As Long = _myWeb.moDbHelper.GetDataValue("SELECT TOP 1 nStructId FROM tblContentLocation WHERE bPrimary = 1 AND nContentId=" & contentId, , , 0)
                                If primaryLocation > 0 Then

                                    Dim siteStructureForBeginners As XmlElement = _myWeb.GetStructureXML(0)

                                    ' Check that primary location is a live page
                                    If Tools.Xml.NodeState(siteStructureForBeginners, "//MenuItem[@id=" & primaryLocation & "]") <> Tools.Xml.XmlNodeState.NotInstantiated Then

                                        ' All checks passed
                                        ' Construct a basic XML to transform
                                        ' It contains the content brief and the site structure
                                        Dim pageElement As XmlElement
                                        Dim contentsElement As XmlElement
                                        postableContentDocument.CreateXmlDeclaration("1.0", "UTF-8", "yes")
                                        pageElement = postableContentDocument.CreateElement("Page")
                                        postableContentDocument.AppendChild(pageElement)
                                        pageElement.AppendChild(postableContentDocument.ImportNode(siteStructureForBeginners, True))
                                        contentsElement = postableContentDocument.CreateElement("Contents")
                                        pageElement.AppendChild(contentsElement)
                                        If Not String.IsNullOrEmpty(Eonic.Web.gcEwBaseUrl) Then pageElement.SetAttribute("baseUrl", Eonic.Web.gcEwBaseUrl)
                                        _myWeb.GetRequestVariablesXml(pageElement)

                                        contentsElement.AppendChild(postableContentDocument.ImportNode(content, True))

                                        ' Set the style file
                                        ' TODO: AG Set a common integration
                                        Dim styleFile As String = ""
                                        If IO.File.Exists(goServer.MapPath("/xsl/integrations/post.xsl")) Then
                                            styleFile = goServer.MapPath("/xsl/integrations/post.xsl")
                                        Else
                                            styleFile = goServer.MapPath("/ewcommon/xsl/integrations/post.xsl")

                                        End If

                                        ' Add all the credentials to the Page
                                        For Each credential As XmlElement In credentials
                                            pageElement.AppendChild(postableContentDocument.ImportNode(credential, True))
                                        Next

                                        ' Transform the xml into postable content via xsl, which returns xml
                                        ' The xsl should pick up every credential (provider) and produce an  
                                        ' appropriately formatted response for that provider.
                                        ' The format is as follows:
                                        ' <post provider="provider" url="url to content">content to post</post>
                                        Dim textWriter As New System.IO.StringWriter
                                        Dim oTransform As New Eonic.XmlHelper.Transform(_myWeb, styleFile, False)
                                        oTransform.mbDebug = gbDebug
                                        oTransform.ProcessTimed(postableContentDocument, textWriter)
                                        oTransform.Close()
                                        oTransform = Nothing

                                        ' Load the response.
                                        Dim response As New XmlDocument
                                        response.LoadXml(textWriter.ToString)

                                        ' iterate through the providers
                                        ' only post those with content.
                                        Dim integrationBase As String = "Eonic.Integration.Directory."

                                        For Each post As XmlElement In response.SelectNodes("/posts/post")

                                            ' Check the post content and URI validity
                                            If post.InnerXml.Trim.Length > 0 _
                                                AndAlso Uri.IsWellFormedUriString(post.GetAttribute("url"), UriKind.Absolute) _
                                                AndAlso Not String.IsNullOrEmpty(post.GetAttribute("provider")) _
                                                Then

                                                ' Everything's good, let's try to post it.
                                                Dim constructorArguments() As Object = {_myWeb, Convert.ToInt64(_myWeb.mnUserId)}
                                                Dim methodArguments() As Object = {post.InnerXml.Trim, New Uri(post.GetAttribute("url"), UriKind.Absolute), Convert.ToInt64(contentId)}
                                                Invoke.InvokeObjectMethod( _
                                                    integrationBase & post.GetAttribute("provider") & ".Post", _
                                                    constructorArguments, _
                                                    methodArguments, _
                                                    Me, _
                                                    "_OnError", _
                                                    "OnError" _
                                                    )

                                            End If

                                        Next

                                    End If

                                End If


                            End If

                            ' Important - revert back to the previous setting
                            _myWeb.mbAdminMode = tempAdminMode

                        End If

                    End If

                End If

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(_moduleName, "PostContent", ex, ""))
            End Try
        End Sub

        ''' <summary>
        ''' Takes a content xForm and works out if it needs to add the checkboxes or alerts for the current user 
        ''' </summary>
        ''' <param name="form"></param>
        ''' <remarks></remarks>
        Public Sub PostContentCheckboxes(ByRef form As Eonic.Web.Admin.AdminXforms, ByVal contentSchema As String, ByVal isContentBeingUpdated As Boolean)

            Dim message As String = ""

            Try

                If Enabled AndAlso Me.ContentCheckboxEnabled Then

                    ' Get the user XML
                    Dim userXml As XmlElement = form.moXformElmt.OwnerDocument.CreateElement("User")
                    userXml.InnerXml = _myWeb.moDbHelper.getObjectInstance(objectTypes.Directory, _myWeb.mnUserId)

                    ' Only continue if there are credentials
                    Dim credentials As XmlNodeList = userXml.SelectNodes("//Credentials")
                    If credentials.Count > 0 Then

                        ' Create a group to add all this to
                        Dim group As XmlElement = form.moXformElmt.OwnerDocument.CreateElement("group")
                        group.SetAttribute("class", "postContentToIntegrations")
                        addElement(group, "label", "Share Content")

                        ' First search for automatic content postings for the content type
                        Dim automaticPostingsXPath As String = "//Credentials[Permissions/Permission[@contentType='" & contentSchema & "' and @" & IIf(isContentBeingUpdated, "edit", "add") & "='true']]"
                        Dim automaticPostingProviders As XmlNodeList = userXml.SelectNodes(automaticPostingsXPath)
                        Dim postingProviders As String = ""
                        If automaticPostingProviders.Count > 0 Then

                            Dim isFirst As Boolean = True
                            For Each credential As XmlElement In automaticPostingProviders
                                If isFirst Then
                                    isFirst = False
                                Else
                                    postingProviders &= ","
                                End If
                                postingProviders &= credential.GetAttribute("provider")
                            Next

                            message = "<p>According to your preferences, this content will be automatically posted to the following: <span id=""autopost"">" & postingProviders & "</span>. </p>"

                        End If

                        ' Check if this content schema is postable
                        ' By default look if the permission exists (not if it's set up for automated posting),
                        ' but also check for an overrided list from the config
                        If Array.IndexOf(Me.ContentCheckboxContentTypes.Trim.ToLower.Split(","), contentSchema.ToLower) > 0 _
                            OrElse (String.IsNullOrEmpty(Me.ContentCheckboxContentTypes.Trim) And userXml.SelectNodes("//Credentials[Permissions/Permission[@contentType='" & contentSchema & "']]").Count > 0) Then

                            Dim providersSelect As XmlElement = form.addSelect(group, "postToIntegrations", False, "", "checkboxes", Eonic.xForm.ApperanceTypes.Full)
                            addElement(providersSelect, "value", postingProviders)
                            For Each provider As XmlElement In userXml.SelectNodes("//Credentials")

                                form.addOption(providersSelect, "Share to " & provider.GetAttribute("provider"), provider.GetAttribute("provider"))

                            Next

                            ' Set the message
                            message = "<div msg-id=""2311""><p>You can share this content. </p>" & message & "<p>To share (or opt not to share) this content, please tick/untick the appropriate boxes below.</p></div>"
                            form.addNote(group, xForm.noteTypes.Hint, message, True)

                            Dim override As XmlElement = form.addInput(group, "overrideAutomatedIntegrationPostings", False, "", "hidden")
                            addElement(override, "value", "true")


                            Dim submission As XmlElement = form.moXformElmt.SelectSingleNode("//submit[not(following::submit)]")
                            submission.ParentNode.InsertBefore(group, submission)


                        End If




                    End If

                End If

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(_moduleName, "PostContentCheckboxes", ex, ""))
            End Try
        End Sub
    End Class



    Public Interface IPostable
        Sub Post(ByVal content As String, Optional ByVal trackbackUrl As Uri = Nothing, Optional ByVal contentId As Long = 0)
    End Interface
End Namespace

