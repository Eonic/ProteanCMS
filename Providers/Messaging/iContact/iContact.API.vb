'***********************************************************************
' $Library:     eonic.messaging.campaignMonitorProvider
' $Revision:    3.1  
' $Date:        2010-03-02
' $Author:      Trevor Spink (trevor@eonic.co.uk)
' &Website:     www.eonic.co.uk
' &Licence:     All Rights Reserved.
' $Copyright:   Copyright (c) 2002 - 2010 Eonic Ltd.
'***********************************************************************

Option Strict Off
Option Explicit On

Imports System.Xml
Imports System.Web.HttpUtility
Imports System.Web.Configuration
Imports System.Configuration
Imports System.IO
Imports System.Collections
Imports System.Data
Imports System.Data.SqlClient
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Net.Mail
Imports System.Reflection
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Web
Imports VB = Microsoft.VisualBasic

Public Class iContactAPI

    Private APIid As String
    Private APIusername As String = "EonicWeb5"
    Private APIpassword As String
    Private AccountId As String
    Private FolderId As String
    Private BaseURL As String
    Private Sandbox As Boolean
    Private XslPath As String
    Shadows Const mcModuleName As String = "Eonic.iContact.API"

    Public errorStr As String
#Region "New Error Handling"
    Public Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)

    Private Sub _OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
        RaiseEvent OnError(sender, e)
    End Sub
#End Region

#Region "Enums"
    Enum requestMethod
        _GET = 0
        _POST = 1
        _DELETE = 2
        _PUT = 3

        '200- reserved for [next thing]
    End Enum

    Dim moMailConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("eonic/mailinglist")

#End Region

    Public Sub New(ByVal cAPIid As String, ByVal cAPIusername As String, ByVal cAPIpassword As String, ByVal cAccountId As String, ByVal cFolderId As String, ByVal bSandbox As Boolean, ByVal sXslPath As String)
        PerfMon.Log("dbHelper", "New")
        Try
            APIid = cAPIid
            APIusername = cAPIusername
            APIpassword = cAPIpassword
            AccountId = cAccountId
            FolderId = cFolderId
            Sandbox = bSandbox
            XslPath = sXslPath
            If Sandbox Then
                BaseURL = "https://app.sandbox.icontact.com/icp/a/" & AccountId & "/c/" & FolderId & "/"
            Else
                BaseURL = "https://app.icontact.com/icp/a/" & AccountId & "/c/" & FolderId & "/"
            End If

        Catch ex As Exception
            RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""))
        End Try
    End Sub

    Public Function CreateCampaign(ByVal Name As String, ByVal FromName As String, ByVal FromEmail As String, ByVal Description As String) As Integer
        Try
            Dim forwardToFriend As String = "3"
            Dim clickTrackMode As String = "1"
            Dim SubscriptionManagement As String = "0"
            Dim useAccountAddress As String = "1"
            Dim archiveByDefault As String = "1"

            Dim campaginXml As String = "<campaigns><campaign><name>" & Protean.Tools.Xml.EncodeForXml(Name) & "</name><description>" & Protean.Tools.Xml.EncodeForXml(Description) & "</description><fromName>" & Protean.Tools.Xml.EncodeForXml(FromName) & "</fromName><fromEmail>" & Protean.Tools.Xml.EncodeForXml(FromEmail) & "</fromEmail>" &
    "<forwardToFriend>" & Protean.Tools.Xml.EncodeForXml(forwardToFriend) & "</forwardToFriend>" &
    "<clickTrackMode>" & Protean.Tools.Xml.EncodeForXml(clickTrackMode) & "</clickTrackMode>" &
    "<subscriptionManagement>" & Protean.Tools.Xml.EncodeForXml(SubscriptionManagement) & "</subscriptionManagement>" &
    "<useAccountAddress>" & Protean.Tools.Xml.EncodeForXml(useAccountAddress) & "</useAccountAddress>" &
    "<archiveByDefault>" & Protean.Tools.Xml.EncodeForXml(archiveByDefault) & "</archiveByDefault></campaign></campaigns>"
            Dim xmlDoc As New XmlDocument
            xmlDoc.LoadXml(campaginXml)
            Dim oResponse As New XmlDocument
            oResponse.LoadXml(MakeRequest("campaigns", requestMethod._POST, xmlDoc.DocumentElement))
            Return CLng(oResponse.SelectSingleNode("response/campaigns/campaign/campaignId").InnerText)

        Catch ex As Exception
            RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "SendEmail", ex, ""))
            Return Nothing
        End Try
    End Function


    Public Function CreateEmail(ByVal CampaignId As Integer, ByVal MessageType As String, ByVal Subject As String, ByVal HtmlBody As String, ByVal TextBody As String, ByVal MessageName As String) As Integer
        Try

            Dim emailXml As String = "<messages><message><campaignId>" & Protean.Tools.Xml.EncodeForXml(CampaignId) & "</campaignId>" &
    "<subject>" & Protean.Tools.Xml.EncodeForXml(Subject) & "</subject>" &
    "<messageType>normal</messageType>" &
    "<messageName>" & Protean.Tools.Xml.EncodeForXml(MessageName) & "</messageName>" &
    "<htmlBody>" & Protean.Tools.Xml.EncodeForXml(HtmlBody) & "</htmlBody>" &
    "<textBody>" & Protean.Tools.Xml.EncodeForXml(TextBody) & "</textBody>" &
    "</message></messages>"
            Dim xmlDoc As New XmlDocument
            xmlDoc.LoadXml(emailXml)
            Dim oResponse As New XmlDocument
            oResponse.LoadXml(MakeRequest("messages", requestMethod._POST, xmlDoc.DocumentElement))
            Return CLng(oResponse.SelectSingleNode("response/messages/message/messageId").InnerText)

        Catch ex As Exception
            RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "SendEmail", ex, ""))
            Return Nothing
        End Try
    End Function


    Public Function SendEmail(ByVal EmailId As Integer, ByVal RecipientGroupsCSV As String, ByVal sendTime As DateTime) As Integer
        Try

            Dim sendXml As String = "<sends><send><messageId>" & Protean.Tools.Xml.EncodeForXml(EmailId) & "</messageId>" &
           "<includeListIds>" & Protean.Tools.Xml.EncodeForXml(RecipientGroupsCSV) & "</includeListIds>" &
           "<scheduledTime>" & Protean.Tools.Xml.XmlDate(sendTime, True) & "</scheduledTime>" &
            "</send></sends>"
            Dim xmlDoc As New XmlDocument
            xmlDoc.LoadXml(sendXml)
            Dim oResponse As New XmlDocument
            oResponse.LoadXml(MakeRequest("sends", requestMethod._POST, xmlDoc.DocumentElement))

            If oResponse.SelectSingleNode("response/warnings/warning") Is Nothing Then
                Return CLng(oResponse.SelectSingleNode("response/sends/send/recipientCount").InnerText)
            Else
                errorStr = oResponse.SelectSingleNode("response/warnings/warning").InnerText
                Return 0
            End If
            Return CLng(oResponse.SelectSingleNode("response/sends/send/recipientCount").InnerText)

        Catch ex As Exception
            RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "SendEmail", ex, ""))
            Return Nothing
        End Try
    End Function


    Public Function SyncContact(ByRef UserXml As XmlElement, Optional listId As Integer = 0) As String

        Dim newUserXml As XmlDocument
        Dim userEmail As String
        Try
            'Transform the UserXml
            newUserXml = TransformIncomingXml(UserXml)

            'Should end up like this

            '<contact>
            '   <email>joewilliams@icontact.com</email>
            '   <prefix>Mr.</prefix>
            '   <firstName>Joe</firstName>
            '   <lastName>Williams</lastName>
            '   <suffix>Jr.</suffix>
            '   <street>2365 Meridian Parkway</street>
            '   <city>Durham</city>
            '   <state>NC</state>
            '   <postalCode>27713</postalCode>
            '   <phone>8668039462</phone>
            '   <business>iContact</business>
            '   <status>normal</status>
            '</contact>

            userEmail = newUserXml.DocumentElement.SelectSingleNode("descendant-or-self::contact/email").InnerText

            Dim ContactId As String = GetContactId(userEmail)

            'If users exists insert Contact ID
            If ContactId > 0 Then
                'Update Existing User Record
                MakeRequest("contacts/" & ContactId, requestMethod._POST, newUserXml.SelectSingleNode("descendant-or-self::contact"))
            Else
                'Add New Record
                MakeRequest("contacts/", requestMethod._POST, newUserXml.SelectSingleNode("descendant-or-self::contacts"))
                ContactId = GetContactId(userEmail)
            End If

            If listId > 0 Then
                AddContactToList(CInt(ContactId), listId, False)
            Else
                'Check and Add to Groups
                Dim listElmt As XmlElement

                For Each listElmt In newUserXml.SelectNodes("descendant-or-self::lists/list")
                    Dim groupName As String = listElmt.SelectSingleNode("name").InnerText
                    Dim bRemove As Boolean = IIf(listElmt.GetAttribute("isMember"), "false", "true")
                    AddContactToList(CInt(ContactId), groupName, bRemove)
                Next
            End If


            ' If listId > 0 Then
            '   AddContactToList(CInt(ContactId), listId, False)
            ' End If

            Return ""

        Catch ex As Exception
            RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""))
            Return Nothing
        End Try
    End Function

    Public Function SyncContacts(ByRef UsersXml As XmlElement, Optional listId As Integer = 0) As String

        Dim newUserXml As XmlDocument
        Dim userEmail As String
        Dim sResponse As String = ""
        Dim i As Integer
        Dim cProcessInfo As String = ""
        Try
            'Transform the UserXml
            newUserXml = TransformIncomingXml(UsersXml)

            Dim totalInstances As Long = newUserXml.SelectNodes("iContact/contacts/contact").Count

            Dim Tasks As New SyncMember()
            Dim workerThreads As Int16 = 20
            Dim compPortThreads As Int16 = 20
            ' throttle to 20 at a time
            ' System.Threading.ThreadPool.GetMaxThreads(workerThreads, compPortThreads)
            System.Threading.ThreadPool.SetMaxThreads(workerThreads, compPortThreads)
            Dim finished As New CountdownEvent(1)

            Dim oElmt As XmlElement
            For Each oElmt In newUserXml.SelectNodes("iContact/contacts/contact")
                Dim bSyncUser As Boolean = True

                If bSyncUser Then
                    Dim stateObj As New SyncMember.MemberObj()
                    stateObj.oUserElmt = oElmt
                    stateObj.iContactAPI = Me
                    stateObj.iCListID = listId
                    ' stateObj.EwGroupID = EwGroupID
                    'stateObj.UnSubRemove = unSubBehavoir

                    ' System.Threading.ThreadPool.QueueUserWorkItem(New System.Threading.WaitCallback(AddressOf Tasks.SyncSingleMember), stateObj)
                    finished.AddCount()
                    ThreadPool.QueueUserWorkItem(Sub(state)
                                                     Try

                                                         Tasks.SyncSingleMember(state)
                                                     Finally
                                                         ' Signal that the work item is complete.
                                                         finished.Signal()
                                                     End Try
                                                 End Sub, stateObj)
                    stateObj = Nothing
                End If
                i = i + 1
            Next

            finished.Signal()
            finished.Wait(1000000) 'THIS IS A Timeout
            If finished.CurrentCount <> 0 Then
                cProcessInfo = "Timed Out " & finished.CurrentCount & " left"
            End If
            finished.Dispose()
            finished = Nothing

            Return i

            'sResponse = MakeRequest("contacts/", requestMethod._POST, newUserXml.DocumentElement.FirstChild)

            ''now add the users to the list
            'If listId > 0 Then
            '    Dim respXml As New XmlDocument
            '    respXml.LoadXml(sResponse)

            '    Dim AddSubscriptionXml As New XmlDocument
            '    AddSubscriptionXml.LoadXml("<subscriptions/>")
            '    Dim respElmt As XmlElement
            '    For Each respElmt In respXml.DocumentElement.SelectNodes("contacts/contact")
            '        Dim subElmt As XmlElement = AddSubscriptionXml.CreateElement("subscription")
            '        Dim elmt1 As XmlElement = AddSubscriptionXml.CreateElement("contactId")
            '        elmt1.InnerText = respElmt.SelectSingleNode("contactId").InnerText
            '        subElmt.AppendChild(elmt1)
            '        Dim elmt2 As XmlElement = AddSubscriptionXml.CreateElement("listId")
            '        elmt2.InnerText = listId
            '        subElmt.AppendChild(elmt2)
            '        Dim elmt3 As XmlElement = AddSubscriptionXml.CreateElement("status")
            '        elmt3.InnerText = "normal"
            '        subElmt.AppendChild(elmt3)
            '        AddSubscriptionXml.DocumentElement.AppendChild(subElmt)
            '    Next

            '    Dim sResponse2 As String = MakeRequest("subscriptions/", requestMethod._POST, AddSubscriptionXml.DocumentElement)

            '    Dim TestMsg As String = sResponse2

            'End If

            Return ""

        Catch ex As Exception

            RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, sResponse))
            Return Nothing
        End Try
    End Function

    Public Function GetContactId(ByVal Email As String) As Long

        Dim newGroupXml As New XmlDocument
        Try

            Dim existingUser As String = MakeRequest("contacts?status=total&email=" & goServer.UrlEncode(Email), requestMethod._GET)
            Dim oExistingUser As XmlElement = newGroupXml.CreateElement("ExistingUser")
            oExistingUser.InnerXml = existingUser

            If Not oExistingUser.SelectSingleNode("response/contacts/contact/contactId") Is Nothing Then
                Return CLng(oExistingUser.SelectSingleNode("response/contacts/contact/contactId").InnerText)
            Else
                Return 0
            End If

        Catch ex As Exception
            RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""))
            Return Nothing
        End Try
    End Function

    Public Function SyncList(ByRef GroupXml As XmlElement) As String

        Dim newGroupXml As XmlDocument
        Dim groupName As String
        Try
            'Transform the UserXml
            newGroupXml = TransformIncomingXml(GroupXml)

            'Should end up like this

            '<contact>
            '   <email>joewilliams@icontact.com</email>
            '   <prefix>Mr.</prefix>
            '   <firstName>Joe</firstName>
            '   <lastName>Williams</lastName>
            '   <suffix>Jr.</suffix>
            '   <street>2365 Meridian Parkway</street>
            '   <city>Durham</city>
            '   <state>NC</state>
            '   <postalCode>27713</postalCode>
            '   <phone>8668039462</phone>
            '   <business>iContact</business>
            '   <status>normal</status>
            '</contact>

            groupName = newGroupXml.DocumentElement.SelectSingleNode("list/name").InnerText
            Dim listId As Long = GetListId(groupName)
            'If users exists insert Contact ID
            If listId > 0 Then
                'Update Existing User Record
                MakeRequest("lists/" & listId, requestMethod._POST, newGroupXml.SelectSingleNode("descendant-or-self::list"))
            Else
                'Add New Record
                MakeRequest("lists", requestMethod._POST, newGroupXml.SelectSingleNode("descendant-or-self::lists"))
            End If
            Return ""

        Catch ex As Exception
            RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""))
            Return Nothing
        End Try
    End Function

    Public Function GetListId(ByVal ListName As String) As Integer

        Dim newGroupXml As New XmlDocument
        Try

            Dim existingGroup As String = MakeRequest("lists?status=total&name=" & goServer.UrlEncode(ListName), requestMethod._GET)
            Dim oExistingGroup As XmlElement = newGroupXml.CreateElement("ExistingUser")
            oExistingGroup.InnerXml = existingGroup

            If Not oExistingGroup.SelectSingleNode("response/lists/list/listId") Is Nothing Then
                Return CLng(oExistingGroup.SelectSingleNode("response/lists/list/listId").InnerText)
            Else
                'create list
                Try
                    Dim WelcomeMessageId As String = moMailConfig("WelcomeMessageID")
                    Dim listXml As String = "<lists><list><name>" & Protean.Tools.Xml.EncodeForXml(ListName) & "</name><description/><emailOwnerOnChange>0</emailOwnerOnChange><welcomeOnManualAdd>0</welcomeOnManualAdd><welcomeOnSignupAdd>0</welcomeOnSignupAdd></list></lists>"
                    Dim xmlDoc As New XmlDocument
                    xmlDoc.LoadXml(listXml)
                    Dim oResponse As New XmlDocument
                    oResponse.LoadXml(MakeRequest("lists", requestMethod._POST, xmlDoc.DocumentElement))
                    Return CLng(oResponse.SelectSingleNode("response/lists/list/listId").InnerText)

                Catch ex As Exception
                    Return 0
                End Try
            End If
        Catch ex As Exception
            RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""))
            Return Nothing
        End Try
    End Function


    Public Function GetLists() As XmlElement

        Dim newGroupXml As New XmlDocument
        Try

            Dim existingGroup As String = MakeRequest("lists?limit=200", requestMethod._GET)
            Dim oExistingGroup As XmlElement = newGroupXml.CreateElement("ExistingUser")
            oExistingGroup.InnerXml = existingGroup

            If Not oExistingGroup.SelectSingleNode("response/lists") Is Nothing Then
                Return oExistingGroup.SelectSingleNode("response/lists")
            Else
                Return Nothing
            End If

        Catch ex As Exception
            RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""))
            Return Nothing
        End Try
    End Function


    Public Function GetClientFolders() As XmlElement

        Dim newFolderXml As New XmlDocument
        Try

            newFolderXml.LoadXml(MakeRequest("switchfolder", requestMethod._GET))

            '  Dim oExistingGroup As XmlElement = newGroupXml.CreateElement("clientfolders")
            ' oExistingGroup.InnerXml = existingGroup

            If Not newFolderXml.SelectSingleNode("response/clientfolders") Is Nothing Then
                Return newFolderXml.SelectSingleNode("response/clientfolders")
            Else
                Return Nothing
            End If

        Catch ex As Exception
            RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""))
            Return Nothing
        End Try
    End Function

    Public Function GetMessages() As XmlElement

        Dim newGroupXml As New XmlDocument
        Try

            Dim existingGroup As String = MakeRequest("messages", requestMethod._GET)
            Dim oExistingGroup As XmlElement = newGroupXml.CreateElement("Response")
            oExistingGroup.InnerXml = existingGroup

            If Not oExistingGroup.SelectSingleNode("response/messages") Is Nothing Then
                Return oExistingGroup.SelectSingleNode("response/messages")
            Else
                Return Nothing
            End If

        Catch ex As Exception
            RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""))
            Return Nothing
        End Try
    End Function

    Public Function AddContactToList(ByVal ContactEmail As String, ByVal ListName As String, ByVal remove As Boolean) As Boolean
        Dim newSubXml As New XmlDocument
        Dim existingSub As String = Nothing
        Try

            Dim ContactId As String = GetContactId(ContactEmail)
            Dim ListId As String = GetListId(ListName)
            If ContactId > 0 Then
                AddContactToList(ContactId, ListId, remove)
            Else
                'contact does not exist so we create


            End If

        Catch ex As Exception
            RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""))
            Return Nothing
        End Try
    End Function

    Public Function AddContactToList(ByVal ContactId As Integer, ByVal ListName As String, ByVal remove As Boolean) As Boolean
        Dim newSubXml As New XmlDocument
        Dim existingSub As String = Nothing
        Try

            Dim ListId As Integer = GetListId(ListName)
            If ListId > 0 Then
                AddContactToList(ContactId, ListId, remove)
            End If


        Catch ex As Exception
            RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""))
            Return Nothing
        End Try
    End Function

    Public Function AddContactToList(ByVal ContactId As Integer, ByVal ListId As Integer, ByVal remove As Boolean) As Boolean
        Dim newSubXml As New XmlDocument
        Dim SubId As String
        Dim existingSub As String = Nothing
        Try

            'we won't bother checking first as too many 404 might block us from making requests.

            '  If ListId > 0 And ContactId > 0 Then
            '  SubId = ListId & "_" & ContactId
            '  existingSub = MakeRequest("subscriptions/" & SubId, requestMethod._GET)
            '  End If


            'Dim oExistingSub As XmlElement = newSubXml.CreateElement("ExistingSub")
            'oExistingSub.InnerXml = existingSub
            Dim sResponse As String
            If remove Then
                If Not existingSub Is Nothing Then
                    'Add remove
                    Dim addXml As String = "<subscriptions><subscription><subscriptionId>" & SubId & "</contactId><status>unsubscribed</status></subscription></subscriptions>"
                    Dim oNewSub As XmlElement = newSubXml.CreateElement("NewSub")
                    oNewSub.InnerXml = addXml
                    sResponse = MakeRequest("subscriptions", requestMethod._POST, oNewSub.SelectSingleNode("descendant-or-self::subscriptions"))
                End If
            Else
                If existingSub Is Nothing And Not (SubId Is Nothing) Then
                    'Add subscription
                    Dim addXml As String = "<subscriptions><subscription><contactId>" & ContactId & "</contactId><listId>" & ListId & "</listId><status>normal</status></subscription></subscriptions>"
                    Dim oNewSub As XmlElement = newSubXml.CreateElement("NewSub")
                    oNewSub.InnerXml = addXml
                    sResponse = MakeRequest("subscriptions", requestMethod._POST, oNewSub.SelectSingleNode("descendant-or-self::subscriptions"))
                Else
                    'do nothing
                End If
            End If

        Catch ex As Exception
            RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""))
            Return Nothing
        End Try
    End Function


    Private Function TransformIncomingXml(ByVal sourceXml As XmlElement) As XmlDocument
        Dim oTransform As New Protean.XmlHelper.Transform()
        Dim resultXmlString As String = ""
        Dim messagePlainText As String = ""
        Dim oXml As XmlDocument = New XmlDocument
        Dim resultXml As XmlDocument = New XmlDocument
        Dim XmlString As String = sourceXml.OuterXml
        Dim sWriter As IO.TextWriter = New IO.StringWriter
        Try

            oXml.LoadXml(XmlString)

            oTransform.XSLFile = XslPath
            oTransform.Compiled = False

            ' Transform for HTML
            '  oXml.DocumentElement.SetAttribute("output", "xml")
            oTransform.Process(oXml, sWriter)
            ' If oTransform.HasError Then Throw New Exception("There was an error transforming the email (Output: HTML).")
            resultXmlString = sWriter.ToString()
            sWriter.Close()

            resultXml.LoadXml(resultXmlString)

            Return resultXml

        Catch ex As Exception
            RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "TransformIncomingXml", ex, ""))
            Return Nothing
        End Try

    End Function

    Private Function MakeRequest(ByVal UrlExtension As String, ByVal Method As requestMethod, Optional ByRef xmlRequest As XmlElement = Nothing) As String

        Dim oRequest As HttpWebRequest
        Dim oResponse As HttpWebResponse
        Dim oStream As System.IO.Stream
        Dim oStreamReader As StreamReader
        Dim oEncoding As New ASCIIEncoding
        Dim byRequest As Byte() = oEncoding.GetBytes("")
        Dim cResponse As String

        Try

            If UrlExtension = "switchfolder" Then
                oRequest = HttpWebRequest.Create(BaseURL.Replace(FolderId & "/", ""))
            Else
                oRequest = HttpWebRequest.Create(BaseURL & UrlExtension)
            End If


            Select Case Method
                Case requestMethod._GET
                    oRequest.Method = "GET"
                Case requestMethod._DELETE
                    oRequest.Method = "DELETE"
                Case requestMethod._POST
                    oRequest.Method = "POST"
                Case requestMethod._PUT
                    oRequest.Method = "PUT"
            End Select
            oRequest.Accept = "text/xml"
            oRequest.ContentType = "text/xml"
            oRequest.Headers.Add("API-Version", "2.2")
            oRequest.Headers.Add("API-AppId", APIid)
            oRequest.Headers.Add("API-Username", APIusername)
            oRequest.Headers.Add("API-Password", APIpassword)

            If Not xmlRequest Is Nothing Then
                byRequest = oEncoding.GetBytes(xmlRequest.OuterXml)
                oRequest.ContentLength = byRequest.Length
                oStream = oRequest.GetRequestStream
                oStream.Write(byRequest, 0, byRequest.Length)
                oStream.Close()
            End If

            '5min timeout 300000 miliseconds
            oRequest.Timeout = 300000
            Try
                oResponse = oRequest.GetResponse()
                oStream = oResponse.GetResponseStream()
                oStreamReader = New StreamReader(oStream, System.Text.Encoding.UTF8)
                cResponse = oStreamReader.ReadToEnd

                oStreamReader.Close()
                oResponse.Close()
                Return cResponse

            Catch ex As WebException
                Dim resp As HttpWebResponse = ex.Response
                If resp.StatusCode = HttpStatusCode.NotFound Then
                    Return Nothing
                Else
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "TransformIncomingXml", ex, ""))
                    Return "<error>" & ex.Message & "</error>"
                End If
            End Try


            '  Return cResponse ' oXmlResponse.FirstChild

        Catch ex As Exception
            RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "TransformIncomingXml", ex, ""))
            Return Nothing
        End Try

    End Function


    Public Class SyncMember

        Private Const mcModuleName As String = "Providers.Messaging.iContactProvider.AdminXForms"
        Dim moMailConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("eonic/mailinglist")

        Public Class MemberObj
            Public iContactAPI As iContactAPI
            Public oUserElmt As XmlElement
            Public iCListID As Integer
            Public EwGroupID As String
            Public UnSubRemove As Boolean = False
        End Class

        Public Sub SyncSingleMember(Member As MemberObj)
            Dim cTableName As String = ""
            Dim cTableKey As String = ""
            Dim cTableFRef As String = ""
            Dim cProcessInfo As String = ""
            Dim UserXml As XmlElement
            Dim nUserId As Long = 0
            Dim userEmail As String
                Try

                userEmail = Member.oUserElmt.SelectSingleNode("descendant-or-self::contact/email").InnerText

                Dim ContactId As String = Member.iContactAPI.GetContactId(userEmail)

                'If users exists insert Contact ID
                If ContactId > 0 Then
                    'Update Existing User Record
                    Member.iContactAPI.MakeRequest("contacts/" & ContactId, requestMethod._POST, Member.oUserElmt.SelectSingleNode("descendant-or-self::contact"))
                Else
                    Dim oContacts As XmlElement = Member.oUserElmt.OwnerDocument.CreateElement("contacts")
                    oContacts.AppendChild(Member.oUserElmt)
                    'Add New Record
                    Member.iContactAPI.MakeRequest("contacts", requestMethod._POST, oContacts)
                    ContactId = Member.iContactAPI.GetContactId(userEmail)
                    End If

                    'Check and Add to Groups
                    Dim listElmt As XmlElement

                '   For Each listElmt In Member.oUserElmt.SelectNodes("descendant-or-self::lists/list")
                'Dim groupName As String = listElmt.SelectSingleNode("name").InnerText
                'Dim bRemove As Boolean = IIf(listElmt.GetAttribute("isMember"), "false", "true")
                'Member.iContactAPI.AddContactToList(CInt(ContactId), groupName, bRemove)
                'Next

                If Member.iCListID > 0 Then
                        Member.iContactAPI.AddContactToList(CInt(ContactId), Member.iCListID, False)
                    End If

                cProcessInfo = ""

            Catch ex As Exception
                returnException(mcModuleName, "SyncSingleMember", ex, "", cProcessInfo, gbDebug)
            End Try
        End Sub

    End Class

End Class
