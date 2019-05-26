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
Imports Eonic.Web
Imports Eonic.Tools
Imports Eonic.Tools.Xml
Imports System.Net.Mail
Imports System.Reflection
Imports System.Net
Imports VB = Microsoft.VisualBasic


Namespace Providers.Messaging
    Public Class iContactProvider

        Public Sub New()
            'do nothing
        End Sub

        Public Sub Initiate(ByRef _AdminXforms As Object, ByRef _AdminProcess As Object, ByRef _Activities As Object, ByRef MemProvider As Object, ByRef myWeb As Eonic.Web)

            MemProvider.AdminXforms = New AdminXForms(myWeb)
            MemProvider.AdminProcess = New AdminProcess(myWeb)
            MemProvider.AdminProcess.oAdXfm = MemProvider.AdminXforms
            'MemProvider.Activities = New Activities()

        End Sub

        Public Class AdminXForms
            Inherits Eonic.Providers.Messaging.EonicProvider.AdminXForms
            Private Const mcModuleName As String = "Providers.Messaging.Generic.AdminXForms"
            Dim moMailConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("eonic/mailinglist")


            Sub New(ByRef aWeb As Web)
                MyBase.New(aWeb)
            End Sub

            Public Overloads Function xFrmSendNewsLetter(ByVal nPageId As Integer, ByVal cPageName As String, ByVal cDefaultEmail As String, ByVal cDefaultEmailName As String, ByRef oPageDetail As XmlElement, ByVal cReplyEmail As String) As XmlElement
                Dim oFrmElmt As XmlElement
                Dim oCol1 As XmlElement
                Dim oCol2 As XmlElement

                Dim cProcessInfo As String = ""
                Try
                    MyBase.NewFrm("SendNewsLetter")

                    MyBase.submission("SendNewsLetter", "", "post", "")

                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Groups", "2col", "Please select a group(s) to send to.")

                    cDefaultEmail = Trim(cDefaultEmail)

                    oCol1 = MyBase.addGroup(oFrmElmt, "", "col1", "")
                    oCol2 = MyBase.addGroup(oFrmElmt, "", "col2", "")

                    Dim oElmt As XmlElement
                    oElmt = MyBase.addInput(oCol1, "cDefaultEmail", True, "Email address to send from", "required long")
                    MyBase.addBind("cDefaultEmail", "cDefaultEmail", "true()")
                    oElmt.AppendChild(oElmt.OwnerDocument.CreateElement("value"))

                    Dim oElmt2 As XmlElement
                    oElmt2 = MyBase.addInput(oCol1, "cDefaultEmailName", True, "Name to send from", "required long")
                    MyBase.addBind("cDefaultEmailName", "cDefaultEmailName", "true()")
                    oElmt2.AppendChild(oElmt.OwnerDocument.CreateElement("value"))

                    oElmt2 = MyBase.addInput(oCol1, "cReplyEmail", True, "Email to Reply to", "required long")
                    MyBase.addBind("cReplyEmail", "cReplyEmail", "true()")
                    oElmt2.AppendChild(oElmt.OwnerDocument.CreateElement("value"))


                    oElmt2 = MyBase.addInput(oCol1, "cCampaignName", True, "Campaign Name", "required long")
                    MyBase.addBind("cCampaignName", "cCampaignName", "true()")
                    oElmt2.AppendChild(oElmt.OwnerDocument.CreateElement("value"))

                    oElmt2 = MyBase.addInput(oCol1, "cSubject", True, "Subject", "required long")
                    MyBase.addBind("cSubject", "cSubject", "true()")
                    oElmt2.AppendChild(oElmt.OwnerDocument.CreateElement("value"))

                    oElmt2 = MyBase.addRange(oCol1, "nSendInHrs", True, "Send in x Hrs", "0", "24", "1")
                    MyBase.addBind("nSendInHrs", "nSendInHrs", "true()")
                    oElmt2.AppendChild(oElmt.OwnerDocument.CreateElement("value"))

                    oElmt2 = MyBase.addRange(oCol1, "nSendInMins", True, "Send in x Mins", "5", "60", "1")
                    MyBase.addBind("nSendInMins", "nSendInMins", "true()")
                    oElmt2.AppendChild(oElmt.OwnerDocument.CreateElement("value"))

                    'Dim oSyncElmt As XmlElement = MyBase.addSelect1(oCol1, "SyncMode", True, "Sync List Options", " required multiline", ApperanceTypes.Full)
                    'MyBase.addOption(oSyncElmt, "Sync All Groups", "sync")
                    'MyBase.addOption(oSyncElmt, "Sync Selected Groups", "syncSelected")
                    'MyBase.addOption(oSyncElmt, "Send without Syncing Groups to Campaign Monitor", "noSync")
                    'MyBase.addBind("SyncMode", "SyncMode", "true()")

                    Dim oSelElmt As XmlElement = MyBase.addSelect(oCol2, "cGroups", True, "Select Groups to send to", " required multiline", ApperanceTypes.Full)
                    GetListsAsOptions(oSelElmt)

                    MyBase.addBind("cGroups", "cGroups", "true()")

                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Send", "", "")

                    MyBase.addSubmit(oFrmElmt, "Send", "Send Via iContact")

                    MyBase.Instance.InnerXml = "<cGroups/><cDefaultEmail>" & cDefaultEmail & "</cDefaultEmail><cDefaultEmailName>" & cDefaultEmailName & "</cDefaultEmailName><cReplyEmail>" & cDefaultEmail & "</cReplyEmail><cCampaignName>" & cPageName & "</cCampaignName><cSubject>" & cPageName & "</cSubject><nSendInHrs>0</nSendInHrs><nSendInMins>5</nSendInMins><SyncMode>syncSelected</SyncMode>"

                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                        Dim oEmailElmt As XmlElement = MyBase.Instance.SelectSingleNode("cDefaultEmail")

                        If Not Tools.Text.IsEmail(oEmailElmt.InnerText.Trim()) Then
                            MyBase.addNote(oElmt, xForm.noteTypes.Alert, "Incorrect Email Address Supplied")
                            MyBase.valid = False
                        End If

                        If MyBase.valid Then
                            Dim CampaignName As String = MyBase.Instance.SelectSingleNode("cCampaignName").InnerText
                            Dim CampaignSubject As String = MyBase.Instance.SelectSingleNode("cSubject").InnerText
                            Dim FromEmail As String = MyBase.Instance.SelectSingleNode("cDefaultEmail").InnerText
                            Dim FromName As String = MyBase.Instance.SelectSingleNode("cDefaultEmailName").InnerText
                            Dim ReplyEmail As String = MyBase.Instance.SelectSingleNode("cReplyEmail").InnerText
                            Dim SendHours As Integer = MyBase.Instance.SelectSingleNode("nSendInHrs").InnerText
                            Dim SendMins As Integer = MyBase.Instance.SelectSingleNode("nSendInMins").InnerText
                            Dim sendDateTime As DateTime = Now()
                            sendDateTime = sendDateTime.AddHours(SendHours)
                            sendDateTime = sendDateTime.AddMinutes(SendMins)

                            'get the body html
                            Dim oWeb As New Web
                            oWeb.InitializeVariables()
                            oWeb.mnPageId = nPageId
                            oWeb.mbAdminMode = False

                            Dim cMailingXsl As String = moMailConfig("MailingXsl")
                            If cMailingXsl = "" Then cMailingXsl = "/xsl/mailer/mailerStandard.xsl"
                            Dim ofs As New Eonic.fsHelper(myWeb.moCtx)
                            cMailingXsl = ofs.checkCommonFilePath(cMailingXsl)

                            myWeb.mcEwSiteXsl = cMailingXsl
                            'get body
                            oWeb.mnMailMenuId = moMailConfig("RootPageId")
                            Dim oEmailBody As String = oWeb.ReturnPageHTML(oWeb.mnPageId)

                            Dim SubscriberListIds As String = MyBase.Instance.SelectSingleNode("cGroups").InnerText

                            'iContactSend
                            Dim iContactApi As New iContactAPI(moMailConfig("ApiID"), moMailConfig("AccountName"), moMailConfig("ApplicationPassword"), moMailConfig("ClientID"), moMailConfig("FolderID"), False, goServer.MapPath("/xsl/iContact/Transforms.xsl"))
                            'create the email
                            Dim CampaignId As Integer
                            CampaignId = iContactApi.CreateCampaign(CampaignName, FromName, FromEmail, "")
                            Dim MailId As Integer
                            MailId = iContactApi.CreateEmail(CampaignId, "Normal", CampaignSubject, oEmailBody, "", CampaignName)
                            'Send
                            Dim sendCount As Integer
                            sendCount = iContactApi.SendEmail(MailId, SubscriberListIds, sendDateTime)

                            If sendCount > 0 Then
                                moDbHelper.logActivity(dbHelper.ActivityType.Email, myWeb.mnUserId, nPageId, , MyBase.Instance.OuterXml)
                                moDbHelper.CommitLogToDB(dbHelper.ActivityType.NewsLetterSent, myWeb.mnUserId, myWeb.moSession.SessionID, Now, myWeb.mnPageId, 0, "", True)
                                MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, "Message Sending to " & sendCount & " Recipients via iContact", True)
                            Else
                                MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, "Failed", True)
                                MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, iContactApi.errorStr, True)
                                MyBase.valid = False
                            End If
            
                        End If
                    End If

                    MyBase.addValues()
                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(mcModuleName, "xFrmSendNewsLetter", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function GetListsAsOptions(ByRef oSelElmt As XmlElement) As XmlElement

                Try

                    Dim iContactApi As New iContactAPI(moMailConfig("ApiID"), moMailConfig("AccountName"), moMailConfig("ApplicationPassword"), moMailConfig("ClientID"), moMailConfig("FolderID"), False, goServer.MapPath("/xsl/iContact/Transforms.xsl"))

                    Dim Lists As XmlElement = iContactApi.GetLists()

                    For Each oElmt In Lists.SelectNodes("list")

                        MyBase.addOption(oSelElmt, oElmt.selectsinglenode("name").innertext, oElmt.selectsinglenode("listId").innertext)
                    Next

                    Return oSelElmt

                Catch ex As Exception
                    returnException(mcModuleName, "ListCampaigns", ex, "", "", gbDebug)
                    Return Nothing
                End Try

            End Function


            Public Function GetFoldersAsOptions(ByRef oSelElmt As XmlElement) As XmlElement

                Try

                    Dim iContactApi As New iContactAPI(moMailConfig("ApiID"), moMailConfig("AccountName"), moMailConfig("ApplicationPassword"), moMailConfig("ClientID"), moMailConfig("FolderID"), False, goServer.MapPath("/xsl/iContact/Transforms.xsl"))

                    Dim Lists As XmlElement = iContactApi.GetClientFolders()

                    For Each oElmt In Lists.SelectNodes("clientfolder")
                        MyBase.addOption(oSelElmt, oElmt.selectsinglenode("name").innertext, oElmt.selectsinglenode("clientFolderId").innertext)
                    Next

                    Return oSelElmt

                Catch ex As Exception
                    returnException(mcModuleName, "ListCampaigns", ex, "", "", gbDebug)
                    Return Nothing
                End Try

            End Function


            Public Function GetMessagesAsOptions(ByRef oSelElmt As XmlElement) As XmlElement

                Try

                    Dim iContactApi As New iContactAPI(moMailConfig("ApiID"), moMailConfig("AccountName"), moMailConfig("ApplicationPassword"), moMailConfig("ClientID"), moMailConfig("FolderID"), False, goServer.MapPath("/xsl/iContact/Transforms.xsl"))

                    Dim Lists As XmlElement = iContactApi.GetMessages()

                    For Each oElmt In Lists.SelectNodes("message")

                        MyBase.addOption(oSelElmt, oElmt.selectsinglenode("subject").innertext, oElmt.selectsinglenode("messageId").innertext)
                    Next

                    Return oSelElmt

                Catch ex As Exception
                    returnException(mcModuleName, "ListCampaigns", ex, "", "", gbDebug)
                    Return Nothing
                End Try

            End Function


        End Class

        Public Class AdminProcess
            Inherits Web.Admin

            Public Event OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)
            Public Event OnErrorWithWeb(ByRef myweb As Eonic.Web, ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)

            Dim _oAdXfm As Eonic.Providers.Messaging.iContactProvider.AdminXForms
            Dim moMailConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("eonic/mailinglist")
            Dim moIContactApi As iContactAPI

            Private CMSupressionListId As Integer
            Private CMDeletedListId As Integer
            Private CMUnSubscribedListId As Integer
            Private CMBouncedListId As Integer
            Private CMUnconfirmedListId As Integer

            Public Property oAdXfm() As Object
                Set(ByVal value As Object)
                    _oAdXfm = value
                End Set
                Get
                    Return _oAdXfm
                End Get
            End Property

            Sub New(ByRef aWeb As Web)
                MyBase.New(aWeb)
            End Sub

            '        Sub GetLocalListIDs()
            '            Try

            '                CMSupressionListId = myWeb.moDbHelper.insertDirectory("CMSupressionListId", "group", "CM Supression List", "", "<Group><Name>CM Supression List</Name><Notes></Notes></Group>", 1, False)
            '                CMDeletedListId = myWeb.moDbHelper.insertDirectory("CMDeletedListId", "group", "CM Deleted List", "", "<Group><Name>CM Deleted List</Name><Notes></Notes></Group>", 1, False)
            '                CMUnSubscribedListId = myWeb.moDbHelper.insertDirectory("CMUnSubscribedListId", "group", "CM Unsubscribed List", "", "<Group><Name>CM Unsubscribed List</Name><Notes></Notes></Group>", 1, False)
            '                CMBouncedListId = myWeb.moDbHelper.insertDirectory("CMBouncedListId", "group", "CM Bounced List", "", "<Group><Name>CM Supression List</Name><Notes></Notes></Group>", 1, False)
            '                CMUnconfirmedListId = myWeb.moDbHelper.insertDirectory("CMUnconfirmedListId", "group", "CM Unconfirmed List", "", "<Group><Name>CM Unconfirmed List</Name><Notes></Notes></Group>", 1, False)

            '            Catch ex As Exception
            '                returnException(mcModuleName, "GetLocalListIDs", ex, "", "", gbDebug)
            '            End Try

            '        End Sub

            Public Function MailingListProcess(ByRef oPageDetail As XmlElement, ByRef oWeb As Web, Optional ByRef sAdminLayout As String = "", Optional ByRef cCmd As String = "", Optional ByRef bLoadStructure As Boolean = False, Optional ByRef sEditContext As String = "", Optional bClearEditContext As Boolean = False) As String
                Dim cRetVal As String = ""
                Dim cSQL As String = ""

                Try
                    Dim moMailConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("eonic/mailinglist")
                    If moMailConfig Is Nothing Then Return ""

                    Dim nMailMenuRoot As Long = moMailConfig("RootPageId")
                    'if we hit this we want to default to the mail root Id
                    If myWeb.mnPageId = gnTopLevel And cCmd <> "NewMail" And cCmd <> "NormalMail" And cCmd <> "AdvancedMail" And cCmd <> "SyncMailList" And cCmd <> "ListMailLists" Then
                        myWeb.mnPageId = nMailMenuRoot
                        cCmd = "MailingList"
                    End If

                    Dim cMailingXsl As String = moMailConfig("MailingXsl")
                    If cMailingXsl = "" Then cMailingXsl = "/xsl/mailer/mailerStandard.xsl"
                    Dim ofs As New Eonic.fsHelper(myWeb.moCtx)
                    cMailingXsl = ofs.checkCommonFilePath(cMailingXsl)

                    'If cCmd = "IsMailingList" Then
                    '    If myWeb.mnPageId = 0 Or Not myweb.moConfig("MailingList") = "on" Then Return False
                    '    If myWeb.mnPageId = moMailConfig("RootPageId") Then
                    '        Return True
                    '    Else
                    '        cSQL = "SELECT nStructParId FROM tblContentStructure WHERE nStructKey = " & myWeb.mnPageId
                    '        If myWeb.moDBHelper.exeProcessSQLScalar(cSQL) = moMailConfig("RootPageId") Then
                    '            Return True
                    '        Else
                    '            Return False
                    '        End If
                    '    End If
                    'End If

                    If cCmd = "OverrideAdminMode" Then
                        If oPageDetail.OwnerDocument.DocumentElement.GetAttribute("adminMode") = "true" Then
                            oPageDetail.OwnerDocument.DocumentElement.SetAttribute("adminMode", "false")
                        End If
                    End If
ProcessFlow:
                    Select Case cCmd
                        Case "MailingList"

                            sAdminLayout = "MailingList"
                            myWeb.mcEwSiteXsl = cMailingXsl
                            bLoadStructure = False

                            Dim nNewsletterRoot As Integer = CInt("0" & moMailConfig("RootPageId"))
                            If Not myWeb.moDbHelper.checkPageExist(nNewsletterRoot) Then nNewsletterRoot = 0

                            If nNewsletterRoot = 0 Then
                                Dim defaultPageXml As String = "<DisplayName title="""" linkType=""internal"" exclude=""false"" noindex=""false""/><Images><img class=""icon"" /><img class=""thumbnail"" /><img class=""detail"" /></Images><Description/>"
                                nNewsletterRoot = myWeb.moDbHelper.insertStructure(0, "", "Newsletters", defaultPageXml, "NewsletterRoot")
                                Eonic.Config.UpdateConfigValue(myWeb, "eonic/mailinglist", "RootPageId", CStr(nNewsletterRoot))

                            End If

                            'we want to return here after editing
                            myWeb.moSession("lastPage") = myWeb.mcOriginalURL

                            'sAdminLayout = "MailingList"
                            'Web.gcEwSiteXsl = moMailConfig("MailingXsl")
                            'bLoadStructure = False
                            'we want to return here after editing
                            If Not myWeb.mbSuppressLastPageOverrides Then myWeb.moSession("lastPage") = myWeb.mcOriginalURL

                        Case "NormalMail"
                            sAdminLayout = ""
                            cCmd = "NormalMail"
                            EditContext = "NormalMail"

                            myWeb.mcEwSiteXsl = cMailingXsl
                            'we want to return here after editing
                            If Not myWeb.mbSuppressLastPageOverrides Then myWeb.moSession("lastPage") = "?ewCmd=NormalMail&pgid=" & myWeb.mnPageId 'myWeb.mcOriginalURL 'not this if being redirected after editing layout for instance.
                            ' ListCampaigns(oPageDetail)

                        Case "NewMail", "EditMail"
                            sAdminLayout = "NewMail"
                            Dim nPage As Integer
                            If myWeb.moRequest("pgid") = "" Then
                                nPage = 0
                            Else
                                nPage = myWeb.moRequest("pgid")
                            End If
                            oPageDetail.AppendChild(oAdXfm.xFrmEditPage(nPage, myWeb.moRequest("name"), "Mail"))
                            If oAdXfm.valid Then
                                If cCmd = "NewMail" Then
                                    cCmd = "MailingList"
                                Else
                                    cCmd = "NormalMail"
                                End If
                                oPageDetail.RemoveAll()
                                GoTo ProcessFlow
                            End If

                        Case "AdvancedMail"
                            sAdminLayout = "Advanced"
                            EditContext = "AdvancedMail"

                            Dim oCommonContentTypes As New XmlDocument
                            If IO.File.Exists(myWeb.goServer.MapPath("/ewcommon/xsl/mailer/layoutmanifest.xml")) Then oCommonContentTypes.Load(myWeb.goServer.MapPath("/ewcommon/xsl/pagelayouts/layoutmanifest.xml"))
                            If IO.File.Exists(myWeb.goServer.MapPath(moConfig("ProjectPath") & "/xsl/mailer/layoutmanifest.xml")) Then
                                Dim oLocalContentTypes As New XmlDocument
                                oLocalContentTypes.Load(myWeb.goServer.MapPath(moConfig("ProjectPath") & "/xsl/mailer/layoutmanifest.xml"))
                                Dim oLocals As XmlElement = oLocalContentTypes.SelectSingleNode("/PageLayouts/ContentTypes")
                                If Not oLocals Is Nothing Then
                                    Dim oGrp As XmlElement
                                    For Each oGrp In oLocals.SelectNodes("ContentTypeGroup")
                                        Dim oComGrp As XmlElement = oCommonContentTypes.SelectSingleNode("/PageLayouts/ContentTypes/ContentTypeGroup[@name='" & oGrp.GetAttribute("name") & "']")
                                        If Not oComGrp Is Nothing Then
                                            Dim oTypeElmt As XmlElement
                                            For Each oTypeElmt In oGrp.SelectNodes("ContentType")
                                                If Not oComGrp.SelectSingleNode("ContentType[@type='" & oTypeElmt.GetAttribute("type") & "']") Is Nothing Then
                                                    oComGrp.SelectSingleNode("ContentType[@type='" & oTypeElmt.GetAttribute("type") & "']").InnerText = oTypeElmt.InnerText
                                                Else
                                                    oComGrp.InnerXml &= oTypeElmt.OuterXml
                                                End If
                                            Next
                                        Else
                                            oCommonContentTypes.DocumentElement.SelectSingleNode("ContentTypes").InnerXml &= oGrp.OuterXml
                                        End If
                                    Next
                                End If
                            End If
                            'now to add it to the pagexml
                            oPageDetail.AppendChild(moPageXML.ImportNode(oCommonContentTypes.SelectSingleNode("/PageLayouts/ContentTypes"), True))


                            If myWeb.moRequest("pgid") <> "" Then
                                'lets save the page we are editing to the session
                                myWeb.moSession("pgid") = myWeb.moRequest("pgid")
                            End If
                            'we want to return here after editing
                            myWeb.moSession("lastPage") = myWeb.mcOriginalURL

                        Case "MailHistory"
                            myWeb.moDbHelper.ViewMailHistory(oPageDetail)
                        Case "EditMailLayout"
                            moAdXfm.goServer = myWeb.goServer
                            oPageDetail.AppendChild(oAdXfm.xFrmEditMailLayout(myWeb.moRequest("pgid")))
                            If oAdXfm.valid Then
                                cCmd = "NormalMail"
                                sAdminLayout = "NormalMail"
                                oPageDetail.RemoveAll()
                                GoTo ProcessFlow
                            Else
                                sAdminLayout = "AdminXForm"
                            End If
                        Case "AddMailModule"
                            bLoadStructure = True
                            nAdditionId = 0
                            oPageDetail.AppendChild(oAdXfm.xFrmAddModule(myWeb.moRequest("pgid"), myWeb.moRequest("position")))
                            If oAdXfm.valid Then
                                If myWeb.moRequest("cModuleBox") <> "" Then
                                    oPageDetail.RemoveAll()
                                    If myWeb.moSession("lastPage") <> "" Then
                                        myWeb.msRedirectOnEnd = myWeb.moSession("lastPage")
                                        myWeb.moSession("lastPage") = ""
                                    Else
                                        cCmd = "NormalMail"
                                        oPageDetail.RemoveAll()
                                        moAdXfm.valid = False
                                        GoTo ProcessFlow
                                    End If
                                Else
                                    cCmd = "AddMailModule"
                                    sAdminLayout = "AdminXForm"
                                End If

                            Else
                                sAdminLayout = "AdminXForm"
                            End If

                        Case "PreviewMail"

                            oPageDetail.AppendChild(oAdXfm.xFrmPreviewNewsLetter(myWeb.mnPageId, oPageDetail))

                            sAdminLayout = "PreviewMail"

                        Case "SendMail"
                            sAdminLayout = "SendMail"
                            'get subject
                            Dim cSubject As String = ""
                            oWeb.mnMailMenuId = moMailConfig("RootPageId")
                            Dim oLocElmt As XmlElement = oWeb.GetPageXML.SelectSingleNode("descendant-or-self::MenuItem[@id=" & myWeb.mnPageId & "]")
                            If Not oLocElmt Is Nothing Then cSubject = oLocElmt.GetAttribute("name")

                            oPageDetail.AppendChild(oAdXfm.xFrmSendNewsLetter(myWeb.mnPageId, cSubject, moMailConfig("SenderEmail"), moMailConfig("SenderName"), oPageDetail, moMailConfig("ReplyEmail")))
                            'oPageDetail.AppendChild(getCampaignReports(myWeb.mnPageId))
                            

                        Case "MailOptOut"
                            sAdminLayout = "OptOut"
                            oPageDetail.AppendChild(oAdXfm.xFrmAdminOptOut)
                        Case "ProcessMailbox"
                            'Dim oPop3 As New POP3
                            'oPop3.ReadMail(moMailConfig("Pop3Server"), moMailConfig("Pop3Acct"), moMailConfig("Pop3Pwd"))
                        Case "DeletePageMail"
                            bLoadStructure = True
                            oPageDetail.AppendChild(oAdXfm.xFrmDeletePage(myWeb.moRequest("pgid")))
                            If oAdXfm.valid Then
                                cCmd = "MailingList"
                                GoTo ProcessFlow
                            Else
                                sAdminLayout = "AdminXForm"
                            End If
                        Case "SearchMailContent"
                            'for searching for content to add


                        Case "ListMailLists"
                            Dim oGrp As XmlElement

                            oPageDetail.AppendChild(myWeb.moDbHelper.listDirectory("Group", CInt("0")))

                            'remove the global groups
                            Dim GroupsXpath As String = "directory/group"
                            If myWeb.moConfig("NonAuthenticatedUsersGroupId") <> "" Then
                                GroupsXpath = GroupsXpath + "[@id = '" & myWeb.moConfig("NonAuthenticatedUsersGroupId") & "' and @id = '" & myWeb.moConfig("AuthenticatedUsersGroupId") & "']"
                            End If

                            For Each oGrp In oPageDetail.SelectNodes(GroupsXpath)
                                oGrp.ParentNode.RemoveChild(oGrp)
                            Next

                            sAdminLayout = "ListMailLists" '"ListGroups"
                            'myWeb.moSession("ewCmd") = mcEwCmd

                            GetListsAsXml(oPageDetail)

                        'SyncLists()
                        'cCmd = "SendMail"
                        'GoTo ProcessFlow
                        Case "SyncMailList"
                            SyncLists(CInt(myWeb.moRequest("parId")))
                            cCmd = "ListMailLists"
                            GoTo ProcessFlow

                    End Select

                    sEditContext = EditContext
                    bClearEditContext = clearEditContext

                    myWeb.moSession("ewCmd") = cCmd
                    mcEwCmd = cCmd
                    'Return cRetVal
                    Return ""
                Catch ex As Exception
                    returnException(mcModuleName, "NewsLetterProcess", ex, "", "", gbDebug)
                    Return ""
                End Try
            End Function


            Public Sub MailingListAdminMenu(ByRef oAdminMenu As XmlElement)

                'This is just a placeholder for overloading

            End Sub



            Public Sub SyncLists(Optional ListId As Integer = 0, Optional CMListId As String = "")
                Dim cProcessInfo As String = ""
                Try
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
                    Dim iContactApi As New iContactAPI(moMailConfig("ApiID"), moMailConfig("AccountName"), moMailConfig("ApplicationPassword"), moMailConfig("ClientID"), moMailConfig("FolderID"), False, goServer.MapPath("/xsl/iContact/Transforms.xsl"))

                    Dim Lists As XmlElement = iContactApi.GetLists()
                    Dim grpEmt As XmlElement = Nothing

                    'gets the membership groups for the website.

                    'gets the lists for the client
                    Dim hLists As New Hashtable

                    For Each oList As XmlElement In Lists.SelectNodes("list")
                        hLists.Add(oList.SelectSingleNode("name").InnerText, oList.SelectSingleNode("listId").InnerText)
                    Next

                    Dim cSQL As String = "SELECT nDirKey, cDirName  FROM tblDirectory WHERE (cDirSchema = 'Group') ORDER BY cDirName"
                    Dim oDr As SqlDataReader = myWeb.moDbHelper.getDataReader(cSQL)

                    Do While oDr.Read
                        If oDr("nDirKey") <> myWeb.moConfig("NonAuthenticatedUsersGroupId") And oDr("nDirKey") <> myWeb.moConfig("AuthenticatedUsersGroupId") Then
                            If ListId = 0 Or ListId = oDr("nDirKey") Then
                                If CMListId = "" Or CMListId = hLists(oDr("cDirName")) Then
                                    If hLists(oDr("cDirName")) Is Nothing Then
                                        cProcessInfo += oDr("cDirName") & "[" & SyncListMembers(hLists(oDr("cDirName")), oDr("nDirKey")) & "]"
                                    Else
                                        cProcessInfo += oDr("cDirName") & "[" & SyncListMembers(hLists(oDr("cDirName")), oDr("nDirKey")) & "]"
                                    End If
                                End If
                            End If
                        End If
                    Loop

                    'If a list doesn't exist for a group create it.

                    'Save the List ID in the foriegn Ref.
                    cProcessInfo = cProcessInfo & ""
                    'Returns the lists as XML format

                Catch ex As Exception
                    returnException(mcModuleName, "ListCampaigns", ex, "", cProcessInfo, gbDebug)
                End Try

            End Sub

            Public Function GetListsAsXml(ByRef rootElmt As XmlElement) As XmlElement
                Dim cProcessInfo As String = ""
                Try
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
                    Dim iContactApi As New iContactAPI(moMailConfig("ApiID"), moMailConfig("AccountName"), moMailConfig("ApplicationPassword"), moMailConfig("ClientID"), moMailConfig("FolderID"), False, goServer.MapPath("/xsl/iContact/Transforms.xsl"))

                    Dim Lists As XmlElement = iContactApi.GetLists()
                    Dim grpEmt As XmlElement = Nothing
                    For Each grpElmt In rootElmt.SelectNodes("directory/group")

                        If Lists.SelectSingleNode("list/name[node()='" & grpElmt.selectsingleNode("Name").innertext & "']") Is Nothing Then
                            'create the list
                            iContactApi.SyncList(grpElmt)
                        Else
                            'Add Stats
                        End If

                    Next
                    '   rootElmt.AppendChild(Lists)
                    Return rootElmt

                Catch ex As Exception
                    returnException(mcModuleName, "ListCampaigns", ex, "", "", gbDebug)
                    Return Nothing
                End Try

            End Function

            Private Function SyncListMembers(ByVal iCListID As String, ByVal EwGroupID As Long) As Long
                Dim cProcessInfo As String = ""
                Dim i As Long = 0
                Try

                    Dim oUsersXml As XmlElement

                    'gets the users in a group
                    oUsersXml = myWeb.moDbHelper.listDirectory("User", EwGroupID, 1)
                    Dim oElmt As XmlElement

                    'For Each oElmt In oUsersXml.SelectNodes("user")

                    '    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
                    '    Dim iContactApi As New iContactAPI(moMailConfig("ApiID"), moMailConfig("AccountName"), moMailConfig("ApplicationPassword"), moMailConfig("ClientID"), moMailConfig("FolderID"), False, goServer.MapPath("/xsl/iContact/Transforms.xsl"))

                    '    iContactApi.SyncContact(oElmt, iCListID)
                    '    cProcessInfo = ""

                    '    i = i + 1
                    'Next

                    Dim iContactApi As New iContactAPI(moMailConfig("ApiID"), moMailConfig("AccountName"), moMailConfig("ApplicationPassword"), moMailConfig("ClientID"), moMailConfig("FolderID"), False, goServer.MapPath("/xsl/iContact/Transforms.xsl"))
                    iContactApi.SyncContacts(oUsersXml, iCListID)

                    Return i

                Catch ex As Exception
                    returnException(mcModuleName, "ListCampaigns", ex, "", "", gbDebug)
                End Try

            End Function

            '        Private Function getCampaignReports(ByVal PageId As Integer) As XmlElement

            '            Dim cProcessInfo As String = ""
            '            Dim ReportElmt As XmlElement
            '            Dim sSql As String = "select * from  tblActivityLog where nStructId = " & PageId & " and nActivityType = 3"
            '            Dim oDs As DataSet
            '            Dim oElmt As XmlElement
            '            Try
            '                Dim _api As CampaignMonitorAPIWrapper.CampaignMonitorAPI.api = New CampaignMonitorAPI.api()

            '                ReportElmt = moPageXML.CreateElement("Report")
            '                oDs = myWeb.moDbHelper.GetDataSet(sSql, "Campaign", "Report")

            '                With oDs.Tables(0)
            '                    .Columns("nActivityKey").ColumnMapping = Data.MappingType.Attribute
            '                    .Columns("nUserDirId").ColumnMapping = Data.MappingType.Attribute
            '                    .Columns("nStructId").ColumnMapping = Data.MappingType.Attribute
            '                    .Columns("nArtId").ColumnMapping = Data.MappingType.Attribute
            '                    .Columns("dDateTime").ColumnMapping = Data.MappingType.Attribute
            '                    .Columns("nActivityType").ColumnMapping = Data.MappingType.Attribute
            '                    '  .Columns("cActivityDetail").ColumnMapping = Data.MappingType.SimpleContent
            '                    .Columns("cSessionId").ColumnMapping = Data.MappingType.Attribute
            '                    '.Columns("cIPAddress").ColumnMapping = Data.MappingType.Attribute
            '                    .Columns("nOtherId").ColumnMapping = Data.MappingType.Attribute
            '                End With

            '                ReportElmt.InnerXml = oDs.GetXml()
            '                ReportElmt = ReportElmt.FirstChild
            '                Dim result As Object
            '                Dim campaign As CampaignMonitorAPIWrapper.CampaignMonitorAPI.Campaign
            '                result = _api.GetClientCampaigns(moMailConfig("ApiKey"), moMailConfig("ClientId"))

            '                For Each oElmt In ReportElmt.SelectNodes("Campaign")
            '                    oElmt.InnerXml = oElmt.SelectSingleNode("cActivityDetail").InnerText
            '                    oElmt.InnerXml = oElmt.FirstChild.InnerXml
            '                    Dim CampaignId As String = oElmt.SelectSingleNode("CampaignId").InnerText
            '                    For Each campaign In result
            '                        If campaign.CampaignID = CampaignId Then
            '                            oElmt.SetAttribute("name", campaign.Name)
            '                            oElmt.SetAttribute("sentDate", campaign.SentDate)
            '                            oElmt.SetAttribute("recipients", campaign.TotalRecipients)
            '                        End If
            '                    Next
            '                    If oElmt.GetAttribute("name") = "" Then
            '                        'we can't find it lets try for a summary
            '                        Dim campSummary As Object
            '                        campSummary = _api.GetCampaignSummary(moMailConfig("ApiKey"), CampaignId)
            '                        If TypeOf campSummary Is CampaignMonitorAPIWrapper.CampaignMonitorAPI.CampaignSummary Then
            '                            oElmt.SetAttribute("bounced", campSummary.Bounced)
            '                        Else
            '                            oElmt.SetAttribute("name", campSummary.Message)
            '                        End If
            '                    End If

            '                Next

            '                Return ReportElmt

            '            Catch ex As Exception
            '                returnException(mcModuleName, "getCampaignReports", ex, "", "", gbDebug)
            '                Return Nothing
            '            End Try

            '        End Function

            '        Private Function getListId(ByVal ListName As String) As String
            '            PerfMon.Log("Messaging", "getListId")

            '            Dim _api As CampaignMonitorAPIWrapper.CampaignMonitorAPI.api = New CampaignMonitorAPI.api()
            '            Dim Lists As Object
            '            Dim List As CampaignMonitorAPIWrapper.CampaignMonitorAPI.List
            '            Dim hLists As New Hashtable


            '            Try
            '                Lists = _api.GetClientLists(moMailConfig("ApiKey"), moMailConfig("ClientID"))
            '                For i = 0 To UBound(Lists)
            '                    List = Lists(i)
            '                    hLists.Add(List.Name, List.ListID)
            '                Next

            '                If hLists(ListName) Is Nothing Then
            '                    _api.CreateList(moMailConfig("ApiKey"), moMailConfig("ClientID"), ListName, "", False, "")

            '                    hLists = New Hashtable
            '                    Lists = _api.GetClientLists(moMailConfig("ApiKey"), moMailConfig("ClientID"))
            '                    For i = 0 To UBound(Lists)
            '                        List = Lists(i)
            '                        hLists.Add(List.Name, List.ListID)
            '                    Next
            '                End If

            '                Return hLists(ListName)

            '            Catch ex As Exception
            '                returnException(mcModuleName, "getListId", ex, "", "", gbDebug)
            '                Return Nothing
            '            End Try
            '        End Function

            Public Sub maintainUserInGroup(ByVal nUserId As Long, ByVal nGroupId As Long, ByVal remove As Boolean, Optional ByVal cUserEmail As String = Nothing, Optional ByVal cGroupName As String = Nothing, Optional isLast As Boolean = False)
                PerfMon.Log("Messaging", "maintainUserInGroup")
                Try
                    'do nothing this is a placeholder
                    If moIContactApi Is Nothing Then
                        moIContactApi = New iContactAPI(moMailConfig("ApiID"), moMailConfig("AccountName"), moMailConfig("ApplicationPassword"), moMailConfig("ClientID"), moMailConfig("FolderID"), False, goServer.MapPath("/xsl/iContact/Transforms.xsl"))
                    End If

                    If cUserEmail Is Nothing Then cUserEmail = MyBase.myWeb.moDbHelper.getNameByKey(dbHelper.objectTypes.Directory, nUserId)
                    If cGroupName Is Nothing Then cGroupName = MyBase.myWeb.moDbHelper.getNameByKey(dbHelper.objectTypes.Directory, nGroupId)

                    Dim iContactUserId As Integer = moIContactApi.GetContactId(cUserEmail)

                    If iContactUserId = 0 Then
                        Dim UserElmt As XmlElement = myWeb.moPageXml.CreateElement("User")
                        UserElmt.AppendChild(myWeb.moDbHelper.GetUserXML(nUserId))
                        Dim iContactGroupId = moIContactApi.GetListId(cGroupName)
                        moIContactApi.SyncContact(UserElmt.FirstChild, iContactGroupId)
                    Else
                        moIContactApi.AddContactToList(iContactUserId, cGroupName, remove)
                    End If

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "maintainUserInGroup", ex, ""))
                End Try
            End Sub


        End Class

        Public Class Activities
            Inherits Eonic.Messaging

            'Overloads Function SendMailToList_Queued(ByVal nPageId As Integer, ByVal cEmailXSL As String, ByVal cGroups As String, ByVal cFromEmail As String, ByVal cFromName As String, ByVal cSubject As String) As Boolean
            '    PerfMon.Log("Messaging", "SendMailToList_Queued")

            '    Dim cProcessInfo As String = ""

            '    Try
            '        Dim moMailConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("eonic/mailinglist")
            '        Dim oAddressDic As UserEmailDictionary = GetGroupEmails(cGroups)
            '        'max number of bcc

            '        Dim oWeb As New Web
            '        oWeb.InitializeVariables()
            '        oWeb.mnPageId = nPageId
            '        oWeb.mbAdminMode = False

            '        Web.gcEwSiteXsl = cEmailXSL
            '        'get body
            '        oWeb.mnMailMenuId = moMailConfig("RootPageId")
            '        Dim oEmailBody As String = oWeb.ReturnPageHTML(oWeb.mnPageId)

            '        Dim i2 As Integer

            '        Dim oEmail As Net.Mail.MailMessage = Nothing
            '        Dim cRepientMail As String

            '        cFromEmail = cFromEmail.Trim()

            '        If Eonic.Tools.Text.IsEmail(cFromEmail) Then
            '            For Each cRepientMail In oAddressDic.Keys
            '                'create the message
            '                If oEmail Is Nothing Then
            '                    oEmail = New Net.Mail.MailMessage
            '                    oEmail.IsBodyHtml = True
            '                    cProcessInfo = "Sending from: " & cFromEmail
            '                    oEmail.From = New Net.Mail.MailAddress(cFromEmail, cFromName)
            '                    oEmail.Body = oEmailBody
            '                    oEmail.Subject = cSubject
            '                End If
            '                'if we are not at the bcc limit then we add the addres
            '                If i2 < CInt(moMailConfig("BCCLimit")) Then
            '                    If Eonic.Tools.Text.IsEmail(cRepientMail.Trim()) Then
            '                        cProcessInfo = "Sending to: " & cRepientMail.Trim()
            '                        oEmail.Bcc.Add(New Net.Mail.MailAddress(cRepientMail.Trim()))
            '                        i2 += 1
            '                    End If
            '                Else
            '                    'otherwise we send it
            '                    cProcessInfo = "Sending queued mail"
            '                    SendQueuedMail(oEmail, moMailConfig("PickupHost"), moMailConfig("PickupLocation"))
            '                    'and reset the counter
            '                    i2 = 0
            '                    oEmail = Nothing
            '                End If
            '            Next
            '            'try a send after in case we havent reached the last send
            '            If i2 < CInt(moMailConfig("BCCLimit")) Then
            '                cProcessInfo = "Sending queued mail (last)"
            '                SendQueuedMail(oEmail, moMailConfig("PickupHost"), moMailConfig("PickupLocation"))
            '                'and reset the counter
            '                i2 = 0
            '                oEmail = Nothing
            '            End If
            '            Return True
            '        Else
            '            Return False
            '        End If

            '    Catch ex As Exception
            '        returnException(mcModuleName, "SendMailToList_Queued", ex, "", cProcessInfo, gbDebug)
            '        Return False
            '    End Try
            'End Function

            'Public Overloads Function SendQueuedMail(ByVal oMailn As Net.Mail.MailMessage, ByVal cHost As String, ByVal cPickupLocation As String) As String
            '    PerfMon.Log("Messaging", "SendQueuedMail")
            '    Try
            '        If oMailn Is Nothing Then Return "No Email Supplied"
            '        Dim oSmtpn As New SmtpClient
            '        oSmtpn.Host = cHost '"127.0.0.1"
            '        oSmtpn.PickupDirectoryLocation = cPickupLocation '"C:\Inetpub\mailroot\Pickup"
            '        oSmtpn.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory
            '        '=#=#=#=#=#=#=#=#=#=#=#=#=#==#=#=#=
            '        'Exit Function
            '        '=#=#=#=#=#=#=#=#=#=#=#=#=#==#=#=#=
            '        oSmtpn.Send(oMailn)
            '        Return "Sent"
            '    Catch ex As Exception
            '        returnException(mcModuleName, "SendQueuedMail", ex, "", "", gbDebug)
            '        Return "Error"
            '    End Try
            'End Function

        End Class
    End Class



End Namespace