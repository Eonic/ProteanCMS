'***********************************************************************
' $Library:     eonic.providers.messaging.campaignMonitor
' $Revision:    3.1  
' $Date:        2010-03-02
' $Author:      Trevor Spink (trevor@eonic.co.uk)
' &Website:     www.eonic.co.uk
' &Licence:     All Rights Reserved.
' $Copyright:   Copyright (c) 2002 - 2040 Eonic Associates LLP
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
Imports Protean.Cms
Imports Protean.Tools
Imports Protean.Tools.Xml
Imports System.Net.Mail
Imports System.Reflection
Imports System.Net
Imports VB = Microsoft.VisualBasic
Imports CampaignMonitorAPIWrapper



Public Class CampaignMonitor

    Public Sub New()
        'do nothing
    End Sub
    'Public Shared Sub Main()
    'Console.WriteLine("Hello World")
    ' End Sub

    Public Sub Initiate(ByRef _AdminXforms As Object, ByRef _AdminProcess As Object, ByRef _Activities As Object, ByRef MemProvider As Object, ByRef myWeb As Protean.Cms)

        MemProvider.AdminXforms = New AdminXForms(myWeb)
        MemProvider.AdminProcess = New AdminProcess(myWeb)
        MemProvider.AdminProcess.oAdXfm = MemProvider.AdminXforms
        MemProvider.Activities = New Activities()

        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12

    End Sub

    Public Class AdminXForms
        Inherits Protean.Providers.Messaging.EonicProvider.AdminXForms
        Private Const mcModuleName As String = "Providers.Messaging.Generic.AdminXForms"
        Dim moMailConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/mailinglist")


        Sub New(ByRef aWeb As Cms)
            MyBase.New(aWeb)
        End Sub

        Public Overloads Function xFrmPreviewNewsLetter(ByVal nPageId As Integer, ByRef oPageDetail As XmlElement) As XmlElement
            Dim oFrmElmt As XmlElement

            Dim cProcessInfo As String = ""
            Try
                MyBase.NewFrm("SendNewsLetter")

                MyBase.submission("SendNewsLetter", "", "post", "")

                oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Unpersonalised", "", "Send Unpersonalised")

                Dim oElmt As XmlElement
                oElmt = MyBase.addInput(oFrmElmt, "cEmail", True, "Email address to send to", "long")
                MyBase.addBind("cEmail", "cEmail")
                oElmt.AppendChild(oElmt.OwnerDocument.CreateElement("value"))
                MyBase.addSubmit(oFrmElmt, "SendUnpersonalised", "Send Unpersonalised")

                MyBase.Instance.InnerXml = "<cEmail/><cUsers/>"

                If MyBase.isSubmitted Then
                    MyBase.updateInstanceFromRequest()
                    MyBase.validate()
                    Dim oEmailElmt As XmlElement = MyBase.Instance.SelectSingleNode("cEmail")
                    If Not Protean.Tools.Text.IsEmail(oEmailElmt.InnerText) Then
                        MyBase.addNote(oElmt, xForm.noteTypes.Alert, "Incorrect Email Address Supplied")
                        MyBase.valid = False
                    End If
                    If MyBase.valid Then
                        Dim moMailConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/mailinglist")
                        Dim cEmail As String = MyBase.Instance.SelectSingleNode("cEmail").InnerText
                        'first we will only deal with unpersonalised
                        Dim oMessager As New Protean.Messaging
                        'get the subject
                        Dim cSubject As String = ""
                        Dim oMessaging As New Activities
                        Dim cMailingXsl As String = moMailConfig("MailingXsl")
                        If cMailingXsl = "" Then cMailingXsl = "/xsl/mailer/mailerStandard.xsl"
                        Dim ofs As New Protean.fsHelper(myWeb.moCtx)
                        cMailingXsl = ofs.checkCommonFilePath(cMailingXsl)
                        If oMessaging.SendSingleMail_Queued(nPageId, cMailingXsl, cEmail, moMailConfig("SenderEmail"), moMailConfig("SenderName"), cSubject) Then
                            'add mssage and return to form so they can sen another

                            Dim oMsgElmt As XmlElement = oPageDetail.OwnerDocument.CreateElement("Content")

                            oMsgElmt.SetAttribute("type", "Message")
                            oMsgElmt.InnerText = "Messages Sent"
                            oPageDetail.AppendChild(oMsgElmt)
                        End If
                    End If
                End If

                MyBase.addValues()
                Return MyBase.moXformElmt

            Catch ex As Exception
                returnException(mcModuleName, "addInput", ex, "", cProcessInfo, gbDebug)
                Return Nothing
            End Try
        End Function

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

                Dim oSyncElmt As XmlElement = MyBase.addSelect1(oCol1, "SyncMode", True, "Sync List Options", " required multiline", ApperanceTypes.Full)
                MyBase.addOption(oSyncElmt, "Sync All Groups", "sync")
                MyBase.addOption(oSyncElmt, "Sync Selected Groups", "syncSelected")
                MyBase.addOption(oSyncElmt, "Send without Syncing Groups to Campaign Monitor", "noSync")
                MyBase.addBind("SyncMode", "SyncMode", "true()")


                Dim oSelElmt As XmlElement = MyBase.addSelect(oCol2, "cGroups", True, "Select Groups to send to", " required multiline", ApperanceTypes.Full)
                GetListsAsOptions(oSelElmt)

                MyBase.addBind("cGroups", "cGroups", "true()")

                oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Send", "", "")

                MyBase.addSubmit(oFrmElmt, "Send", "Send Via CampaignMonitor")

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

                        'Sync the Groups
                        Dim AdminProcess As New Protean.Providers.Messaging.CampaignMonitor.AdminProcess(myWeb)
                        Dim SubscriberListIds() As String = MyBase.Instance.SelectSingleNode("cGroups").InnerText.Split(",")

                        Dim ListIds As New List(Of String)
                        Dim SegmentIds As New List(Of String)
                        For Each id As String In SubscriberListIds
                            If id.StartsWith("SEGMENT") Then
                                SegmentIds.Add(id.Replace("SEGMENT", ""))
                            Else
                                ListIds.Add(id)
                            End If
                        Next


                        Dim listId As String
                        Select Case MyBase.Instance.SelectSingleNode("SyncMode").InnerText
                            Case "sync"
                                AdminProcess.SyncLists()
                            Case "syncSelected"
                                For Each listId In SubscriberListIds
                                    AdminProcess.SyncLists(0, listId)
                                Next
                            Case "noSync"
                                'do nothing
                        End Select


                        'Setup the Campaign
                        ' Dim _api As CampaignMonitorAPIWrapper.CampaignMonitorAPI.api = New CampaignMonitorAPI.api()

                        Dim cmAuth As New createsend_dotnet.ApiKeyAuthenticationDetails(moMailConfig("ApiKey"))

                        Dim campaignId As Object 'CampaignMonitorAPIWrapper.CampaignMonitorAPI.Campaign

                        Dim CampaignName As String = MyBase.Instance.SelectSingleNode("cCampaignName").InnerText
                        Dim CampaignSubject As String = MyBase.Instance.SelectSingleNode("cSubject").InnerText
                        Dim FromEmail As String = MyBase.Instance.SelectSingleNode("cDefaultEmail").InnerText
                        Dim FromName As String = MyBase.Instance.SelectSingleNode("cDefaultEmailName").InnerText
                        Dim ReplyEmail As String = MyBase.Instance.SelectSingleNode("cReplyEmail").InnerText
                        Dim SendHours As Integer = MyBase.Instance.SelectSingleNode("nSendInHrs").InnerText
                        Dim SendMins As Integer = MyBase.Instance.SelectSingleNode("nSendInMins").InnerText

                        Dim DirectURL As String = moMailConfig("DirectURL")
                        If DirectURL = "" Then DirectURL = myWeb.moRequest.ServerVariables("SERVER_NAME")

                        If Not DirectURL.StartsWith("http") Then
                            DirectURL = "http://" & DirectURL
                        End If

                        Dim htmlUrl As String = DirectURL & "/?contentType=email&pgid=" & myWeb.mnPageId
                        Dim textUrl As String = DirectURL & "/?contentType=email&pgid=" & myWeb.mnPageId & "&textonly=true"

                        'Dim SubscriberListIds() As String = MyBase.Instance.SelectSingleNode("cGroups").InnerText.Split(",")
                        Dim oListSegments As Object = Nothing
                        Try
                            campaignId = createsend_dotnet.Campaign.Create(cmAuth, moMailConfig("ClientID"), CampaignSubject, CampaignName, FromName, FromEmail, ReplyEmail, htmlUrl, textUrl, ListIds, SegmentIds)
                            Dim thisCampaign As New createsend_dotnet.Campaign(cmAuth, campaignId)

                            If TypeOf campaignId Is CampaignMonitorAPIWrapper.CampaignMonitorAPI.Result Then
                                MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, campaignId.Message, True)
                                MyBase.valid = False
                            Else
                                'add the campaign Id to the instance.
                                Dim oElmt4 As XmlElement = moPageXML.CreateElement("CampaignId")
                                oElmt4.InnerText = campaignId
                                MyBase.Instance.AppendChild(oElmt4)
                                Dim sendDateTime As DateTime = Now()
                                sendDateTime = sendDateTime.AddHours(SendHours)
                                sendDateTime = sendDateTime.AddMinutes(SendMins)

                                'Send the Email
                                Try
                                    thisCampaign.Send(FromEmail, sendDateTime)
                                    moDbHelper.logActivity(dbHelper.ActivityType.Email, myWeb.mnUserId, nPageId, , MyBase.Instance.OuterXml)
                                    moDbHelper.CommitLogToDB(dbHelper.ActivityType.NewsLetterSent, myWeb.mnUserId, myWeb.moSession.SessionID, Now, myWeb.mnPageId, 0, "", True)
                                    MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, "Message Sent", True)
                                Catch ex As Exception
                                    MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, ex.Message, True)
                                    MyBase.valid = False
                                End Try
                            End If
                        Catch ex As Exception
                            MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, ex.Message, True)
                            MyBase.valid = False
                        End Try
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
                'Dim _api As CampaignMonitorAPIWrapper.CampaignMonitorAPI.api = New CampaignMonitorAPI.api()
                Dim Lists As Object
                Dim Segments As Object
                Dim List As createsend_dotnet.BasicList
                Dim Segment As createsend_dotnet.BasicSegment

                Dim cmAuth As New createsend_dotnet.ApiKeyAuthenticationDetails(moMailConfig("ApiKey"))
                Dim CMclient As New createsend_dotnet.Client(cmAuth, moMailConfig("ClientID"))


                'gets the lists for the client
                Dim hLists As New Hashtable
                Lists = CMclient.Lists()
                Segments = CMclient.Segments()
                For i = 0 To UBound(Lists)
                    List = Lists(i)
                    MyBase.addOption(oSelElmt, List.Name, List.ListID)
                    For j = 0 To UBound(Segments)
                        Segment = Segments(j)
                        If Segment.ListID = List.ListID Then
                            MyBase.addOption(oSelElmt, "--- " & Segment.Title, "SEGMENT" & Segment.SegmentID)
                        End If
                    Next
                Next

                Return oSelElmt

            Catch ex As Exception
                returnException(mcModuleName, "ListCampaigns", ex, "", "", gbDebug)
                Return Nothing
            End Try

        End Function



    End Class

    Public Class AdminProcess
        Inherits Cms.Admin

        Dim IntegratorId As New String("d2d03c3dd847620c")

        Dim _oAdXfm As Protean.Providers.Messaging.CampaignMonitor.AdminXForms
        Dim moMailConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/mailinglist")

        Private CMSupressionListId As Integer
        Private CMDeletedListId As Integer
        Private CMUnSubscribedListId As Integer
        Private CMBouncedListId As Integer
        Private CMUnconfirmedListId As Integer
        Private BulkSubscribes As New List(Of ListSubscribe)

        Public Property oAdXfm() As Object
            Set(ByVal value As Object)
                _oAdXfm = value
            End Set
            Get
                Return _oAdXfm
            End Get
        End Property

        Sub New(ByRef aWeb As Cms)
            MyBase.New(aWeb)

        End Sub

        Sub GetLocalListIDs()
            Try

                CMSupressionListId = myWeb.moDbHelper.insertDirectory("CMSupressionListId", "group", "CM Supression List", "", "<Group><Name>CM Supression List</Name><Notes></Notes></Group>", 1, False)
                CMDeletedListId = myWeb.moDbHelper.insertDirectory("CMDeletedListId", "group", "CM Deleted List", "", "<Group><Name>CM Deleted List</Name><Notes></Notes></Group>", 1, False)
                CMUnSubscribedListId = myWeb.moDbHelper.insertDirectory("CMUnSubscribedListId", "group", "CM Unsubscribed List", "", "<Group><Name>CM Unsubscribed List</Name><Notes></Notes></Group>", 1, False)
                CMBouncedListId = myWeb.moDbHelper.insertDirectory("CMBouncedListId", "group", "CM Bounced List", "", "<Group><Name>CM Supression List</Name><Notes></Notes></Group>", 1, False)
                CMUnconfirmedListId = myWeb.moDbHelper.insertDirectory("CMUnconfirmedListId", "group", "CM Unconfirmed List", "", "<Group><Name>CM Unconfirmed List</Name><Notes></Notes></Group>", 1, False)

            Catch ex As Exception
                returnException(mcModuleName, "GetLocalListIDs", ex, "", "", gbDebug)
            End Try

        End Sub

        Public Function MailingListProcess(ByRef oPageDetail As XmlElement, ByRef oWeb As Cms, Optional ByRef sAdminLayout As String = "", Optional ByRef cCmd As String = "", Optional ByRef bLoadStructure As Boolean = False, Optional ByRef sEditContext As String = "", Optional bClearEditContext As Boolean = False) As String
            Dim cRetVal As String = ""
            Dim cSQL As String = ""

            Try
                Dim moMailConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/mailinglist")
                If moMailConfig Is Nothing Then Return ""



                Dim nMailMenuRoot As Long = CInt("0" & moMailConfig("RootPageId"))
                'if we hit this we want to default to the mail root Id
                If myWeb.mnPageId = gnTopLevel And cCmd <> "NewMail" And cCmd <> "NormalMail" And cCmd <> "AdvancedMail" And cCmd <> "SyncMailList" And cCmd <> "ListMailLists" Then
                    myWeb.mnPageId = nMailMenuRoot

                    If cCmd <> "" Then
                        ' skip image for submissions
                        cCmd = "MailingList"
                    End If

                End If

                Dim cMailingXsl As String = moMailConfig("MailingXsl")
                If cMailingXsl = "" Then cMailingXsl = "/xsl/mailer/mailerStandard.xsl"
                Dim ofs As New Protean.fsHelper(myWeb.moCtx)
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
                        If sEditContext = "Normal" Then
                            EditContext = "NormalMail"
                        End If
                        Dim nNewsletterRoot As Integer = CInt("0" & moMailConfig("RootPageId"))
                        If Not myWeb.moDbHelper.checkPageExist(nNewsletterRoot) Then nNewsletterRoot = 0

                        If nNewsletterRoot = 0 Then
                            Dim defaultPageXml As String = "<DisplayName title="""" linkType=""internal"" exclude=""false"" noindex=""false""/><Images><img class=""icon"" /><img class=""thumbnail"" /><img class=""detail"" /></Images><Description/>"
                            nNewsletterRoot = myWeb.moDbHelper.insertStructure(0, "", "Newsletters", defaultPageXml, "NewsletterRoot")
                            Protean.Config.UpdateConfigValue(myWeb, "protean/mailinglist", "RootPageId", CStr(nNewsletterRoot))

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
                        If Not myWeb.mbPopupMode Then

                            If Not myWeb.mbSuppressLastPageOverrides Then myWeb.moSession("lastPage") = "?ewCmd=NormalMail&pgid=" & myWeb.mnPageId 'myWeb.mcOriginalURL 'not this if being redirected after editing layout for instance.

                        End If

                        ListCampaigns(oPageDetail)
                    Case "MailPreviewOn"

                        myWeb.mcEwSiteXsl = cMailingXsl
                        sAdminLayout = "Preview"

                        myWeb.moSession("PreviewDate") = Now.Date
                        myWeb.moSession("PreviewUser") = oWeb.mnUserId

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
                        EditContext = ""
                        myWeb.moDbHelper.ViewMailHistory(oPageDetail)
                    Case "EditMailLayout"
                        EditContext = ""
                        moAdXfm.goServer = myWeb.goServer
                        oPageDetail.AppendChild(oAdXfm.xFrmEditMailLayout(myWeb.moRequest("pgid")))
                        If oAdXfm.valid Then
                            cCmd = "NormalMail"
                            sAdminLayout = "NormalMail"
                            oPageDetail.RemoveAll()
                            myWeb.msRedirectOnEnd = myWeb.moSession("lastPage")
                            ' GoTo ProcessFlow
                        Else
                            sAdminLayout = "AdminXForm"
                        End If
                    Case "AddMailModule"

                        EditContext = ""
                        bLoadStructure = True
                        nAdditionId = 0
                        oPageDetail.AppendChild(oAdXfm.xFrmAddModule(myWeb.moRequest("pgid"), myWeb.moRequest("position")))
                        If oAdXfm.valid Then
                            If myWeb.moRequest("nStatus") <> "" Then
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

                    Case "EditMailContent"

                        ' Get a version Id if it's passed through.
                        Dim cVersionKey As String = myWeb.moRequest("verId") & ""
                        bClearEditContext = False
                        bLoadStructure = True
                        If Not (IsNumeric(cVersionKey)) Then cVersionKey = "0"
                        Dim nContentId As Long
                        nContentId = 0
                        oPageDetail.AppendChild(moAdXfm.xFrmEditContent(myWeb.moRequest("id"), "", CLng(myWeb.moRequest("pgid")), , , nContentId, , , CLng(cVersionKey)))

                        If moAdXfm.valid Then
                            '  bAdminMode = False
                            sAdminLayout = ""
                            mcEwCmd = myWeb.moSession("ewCmd")

                            'if we have a parent releationship lets add it
                            If myWeb.moRequest("contentParId") <> "" AndAlso IsNumeric(myWeb.moRequest("contentParId")) Then
                                myWeb.moDbHelper.insertContentRelation(myWeb.moRequest("contentParId"), nContentId)
                            End If
                            If myWeb.moRequest("EditXForm") <> "" Then
                                '    bAdminMode = True
                                sAdminLayout = "AdminXForm"
                                mcEwCmd = "EditXForm"
                                oPageDetail = oWeb.GetContentDetailXml(, myWeb.moRequest("id"))
                            Else
                                myWeb.mnArtId = 0
                                oPageDetail.RemoveAll()

                                ' Check for an optional command to redireect to
                                If Not (String.IsNullOrEmpty("" & myWeb.moRequest("ewRedirCmd"))) Then

                                    myWeb.msRedirectOnEnd = moConfig("ProjectPath") & "/?ewCmd=" & myWeb.moRequest("ewRedirCmd")

                                ElseIf myWeb.moSession("lastPage") <> "" Then
                                    myWeb.msRedirectOnEnd = myWeb.moSession("lastPage")
                                    myWeb.moSession("lastPage") = ""
                                Else
                                    oPageDetail.RemoveAll()
                                    moAdXfm.valid = False
                                    GoTo ProcessFlow
                                End If
                            End If
                        Else
                            sAdminLayout = "AdminXForm"
                        End If
                    Case "PreviewMail"

                        Dim cSubject As String = ""
                        Dim oLocElmt As XmlElement = oWeb.GetPageXML.SelectSingleNode("descendant-or-self::MenuItem[@id=" & myWeb.mnPageId & "]")
                        If Not oLocElmt Is Nothing Then cSubject = oLocElmt.GetAttribute("name")
                        oPageDetail.AppendChild(oAdXfm.xFrmPreviewNewsLetter(myWeb.mnPageId, oPageDetail, cSubject))

                        sAdminLayout = "PreviewMail"

                        'If moAdXfm.valid Then
                        '    Dim cEmail As String = moAdXfm.instance.SelectSingleNode("cEmail").InnerText
                        '    'first we will only deal with unpersonalised
                        '    Dim oMessager As New Messaging
                        '    'get the subject
                        '    Dim cSubject As String = ""
                        '    Dim oMessaging As New Eonic.Messaging
                        '    If oMessaging.SendSingleMail_Queued(myWeb.mnPageId, moMailConfig("MailingXsl"), cEmail, moMailConfig("SenderEmail"), moMailConfig("SenderName"), cSubject) Then
                        '        'add mssage and return to form so they can sen another

                        '        Dim oMsgElmt As XmlElement = oPageDetail.OwnerDocument.CreateElement("Content")

                        '        oMsgElmt.SetAttribute("type", "Message")
                        '        oMsgElmt.InnerText = "Messages Sent"
                        '        oPageDetail.AppendChild(oMsgElmt)
                        '    End If
                        'End If

                    Case "SendMail"
                        sAdminLayout = "SendMail"
                        'get subject
                        Dim cSubject As String = ""
                        oWeb.mnMailMenuId = moMailConfig("RootPageId")
                        Dim oLocElmt As XmlElement = oWeb.GetPageXML.SelectSingleNode("descendant-or-self::MenuItem[@id=" & myWeb.mnPageId & "]")
                        If Not oLocElmt Is Nothing Then cSubject = oLocElmt.GetAttribute("name")

                        oPageDetail.AppendChild(oAdXfm.xFrmSendNewsLetter(myWeb.mnPageId, cSubject, moMailConfig("SenderEmail"), moMailConfig("SenderName"), oPageDetail, moMailConfig("ReplyEmail")))
                        oPageDetail.AppendChild(getCampaignReports(myWeb.mnPageId))
                        'If moAdXfm.valid Then

                        '    'get the individual elements
                        '    Dim oMessaging As New Eonic.Messaging
                        '    'First we need to get the groups we are sending to
                        '    Dim oGroupElmt As XmlElement = moAdXfm.instance.SelectSingleNode("cGroups")
                        '    Dim oFromEmailElmt As XmlElement = moAdXfm.instance.SelectSingleNode("cDefaultEmail")
                        '    Dim oFromNameElmt As XmlElement = moAdXfm.instance.SelectSingleNode("cDefaultEmailName")
                        '    Dim oSubjectElmt As XmlElement = moAdXfm.instance.SelectSingleNode("cSubject")
                        '    'get the email addresses for these groups

                        '    If oMessaging.SendMailToList_Queued(myWeb.mnPageId, moMailConfig("MailingXsl"), oGroupElmt.InnerText, oFromEmailElmt.InnerText, oFromNameElmt.InnerText, oSubjectElmt.InnerText) Then
                        '        Dim cGroupStr As String = "<Groups><Group>" & Replace(oGroupElmt.InnerText, ",", "</Group><Group>") & "</Group></Groups>"
                        '        myWeb.moDBHelper.logActivity(dbHelper.ActivityType.Email, myWeb.mnUserId, myWeb.mnPageId, , oGroupElmt.InnerText)
                        '        'add mssage and return to form so they can sen another
                        '        Dim oMsgElmt As XmlElement = oPageDetail.OwnerDocument.CreateElement("Content")
                        '        oMsgElmt.SetAttribute("type", "Message")
                        '        oMsgElmt.InnerText = "Messages Sent"
                        '        oPageDetail.AppendChild(oMsgElmt)
                        '    End If
                        'End If

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
                            myWeb.msRedirectOnEnd = "/?ewCmd=MailingList"

                            'cCmd = "MailingList"
                            'GoTo ProcessFlow
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

                    Case "MailingList.CMSession"
                        'Get a session URL
                        Dim AuthDetails As New createsend_dotnet.ApiKeyAuthenticationDetails(moMailConfig("ApiKey"))

                        Dim ExtSessionOptions As New createsend_dotnet.ExternalSessionOptions()

                        ExtSessionOptions.Chrome = "Tabs"
                        ExtSessionOptions.ClientID = moMailConfig("ClientID")
                        ExtSessionOptions.Email = moMailConfig("CMUsername")
                        ExtSessionOptions.IntegratorID = IntegratorId
                        ExtSessionOptions.Url = "/"

                        Dim CMGen As New createsend_dotnet.General(AuthDetails)

                        Dim sessionURL As String = CMGen.ExternalSessionUrl(ExtSessionOptions)

                        Dim oElmt As XmlElement = moPageXML.CreateElement("SessionPage")
                        oElmt.SetAttribute("CMUrl", sessionURL)

                        oPageDetail.AppendChild(oElmt)



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

        Public Sub SyncUser(ByRef nUserId As Integer)
            Try
                Dim cProcessInfo As String
                'This is just a placeholder for overloading
                Dim oUserXml As XmlElement = myWeb.moDbHelper.GetUserXML(nUserId)

                Dim Tasks As New SyncMember()
                Dim workerThreads As Int16
                Dim compPortThreads As Int16
                System.Threading.ThreadPool.GetMaxThreads(workerThreads, compPortThreads)
                System.Threading.ThreadPool.SetMaxThreads(workerThreads, compPortThreads)
                Dim finished As New CountdownEvent(1)

                For Each oGroup As XmlElement In oUserXml.SelectNodes("Group")
                    Dim unSubBehavoir As Boolean = False
                    'get unsubBehaviour
                    Dim oGroupXml As XmlElement = myWeb.moPageXml.CreateElement("instance")
                    oGroupXml.InnerXml = myWeb.moDbHelper.getObjectInstance(dbHelper.objectTypes.Directory, oGroup.GetAttribute("id"))
                    Dim groupDetail As XmlElement = oGroupXml.SelectSingleNode("tblDirectory/cDirXml/Group")
                    If groupDetail.GetAttribute("unsubscribebehaviour") = "1" Then
                        unSubBehavoir = True
                    End If

                    'GetCMListID
                    Dim stateObj As New SyncMember.MemberObj()
                    stateObj.oUserElmt = oUserXml
                    stateObj.myWeb = myWeb
                    stateObj.CmListID = getListId(oGroup.GetAttribute("name"))
                    stateObj.CMBouncedListId = CMBouncedListId
                    stateObj.CMDeletedListId = CMDeletedListId
                    stateObj.CMSupressionListId = CMSupressionListId
                    stateObj.CMUnconfirmedListId = CMUnconfirmedListId
                    stateObj.CMUnSubscribedListId = CMUnSubscribedListId
                    stateObj.EwGroupID = oGroup.GetAttribute("id")
                    stateObj.UnSubRemove = unSubBehavoir
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

                Next

                finished.Signal()
                finished.Wait(1000000) 'THIS IS A Timeout
                If finished.CurrentCount <> 0 Then
                    cProcessInfo = "Timed Out " & finished.CurrentCount & " left"
                End If
                finished.Dispose()
                finished = Nothing
            Catch ex As Exception
                returnException(mcModuleName, "ListCampaigns", ex, "", "", gbDebug)
            End Try
        End Sub

        Private Sub ListCampaigns(ByRef oPageDetail As XmlElement)
            Try
                Dim _api As CampaignMonitorAPIWrapper.CampaignMonitorAPI.api = New CampaignMonitorAPI.api()
                Dim Campaigns As Object
                Dim Campaign As CampaignMonitorAPIWrapper.CampaignMonitorAPI.Campaign
                Dim CampElmt As XmlElement
                Dim CampElmt2 As XmlElement
                Dim i As Integer
                Campaigns = _api.GetClientCampaigns(moMailConfig("ApiKey"), moMailConfig("ClientID"))
                CampElmt = MyBase.moPageXML.CreateElement("Campaigns")

                For i = 0 To UBound(Campaigns)
                    Campaign = Campaigns(i)
                    CampElmt2 = MyBase.moPageXML.CreateElement("Campaign")
                    CampElmt2.SetAttribute("id", Campaign.CampaignID)
                    CampElmt2.SetAttribute("name", Campaign.Name)
                    CampElmt2.SetAttribute("subject", Campaign.Subject)
                    CampElmt2.SetAttribute("recipients", Campaign.TotalRecipients)
                    CampElmt.AppendChild(CampElmt2)
                Next
                oPageDetail.AppendChild(CampElmt)

            Catch ex As Exception
                returnException(mcModuleName, "ListCampaigns", ex, "", "", gbDebug)
            End Try

        End Sub



        Public Sub SyncLists(Optional ListId As Integer = 0, Optional CMListId As String = "")
            Dim cProcessInfo As String = ""
            Try
                Dim _api As CampaignMonitorAPIWrapper.CampaignMonitorAPI.api = New CampaignMonitorAPI.api()
                Dim Lists As Object
                Dim List As CampaignMonitorAPIWrapper.CampaignMonitorAPI.List
                'gets the membership groups for the website.

                Dim cSQL As String = "SELECT nDirKey, cDirName  FROM tblDirectory WHERE (cDirSchema = 'Group') ORDER BY cDirName"
                Dim oDr As SqlDataReader = myWeb.moDbHelper.getDataReader(cSQL)
                Dim hDirGroups As New Hashtable
                Do While oDr.Read
                    hDirGroups.Add(oDr("cDirName"), oDr("nDirKey"))
                Loop

                'gets the lists for the client
                Dim hLists As New Hashtable
                Lists = _api.GetClientLists(moMailConfig("ApiKey"), moMailConfig("ClientID"))
                For i = 0 To UBound(Lists)
                    List = Lists(i)
                    hLists.Add(List.Name, List.ListID)
                    hDirGroups.Remove(List.Name)
                Next
                Dim key As DictionaryEntry
                For Each key In hDirGroups
                    Dim newList As Object = _api.CreateList(moMailConfig("ApiKey"), moMailConfig("ClientID"), key.Key, "", False, "")
                    cProcessInfo = newList.ToString()
                    hLists.Add(key.Key, newList)
                Next

                oDr = myWeb.moDbHelper.getDataReader(cSQL)

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
                Dim _api As CampaignMonitorAPIWrapper.CampaignMonitorAPI.api = New CampaignMonitorAPI.api()
                Dim Lists As Object
                Dim List As CampaignMonitorAPIWrapper.CampaignMonitorAPI.List
                Dim listsElmt As XmlElement = rootElmt.OwnerDocument.CreateElement("ProviderLists")

                'gets the lists for the client
                Dim hLists As New Hashtable
                Lists = _api.GetClientLists(moMailConfig("ApiKey"), moMailConfig("ClientID"))
                For i = 0 To UBound(Lists)
                    List = Lists(i)
                    Dim listElmt As XmlElement = rootElmt.OwnerDocument.CreateElement("List")
                    listElmt.SetAttribute("name", List.Name)
                    listElmt.SetAttribute("id", List.ListID)
                    listsElmt.AppendChild(listElmt)
                Next

                Dim Tasks As New AddListStats()
                System.Threading.ThreadPool.SetMaxThreads(20, 20)
                Dim count As Integer = 0
                Dim j As Integer = 0
                Dim finished As New CountdownEvent(1)
                For i = 0 To UBound(Lists)

                    List = Lists(i)
                    Dim stateObj As New AddListStats.ListObj()
                    stateObj.oMasterList = listsElmt
                    stateObj.CmListID = List.ListID
                    stateObj.API = _api

                    finished.AddCount()
                    ThreadPool.QueueUserWorkItem(Sub(state)
                                                     Try

                                                         Tasks.UpdateListStats(state)
                                                     Finally
                                                         ' Signal that the work item is complete.
                                                         If Not finished Is Nothing Then
                                                             finished.Signal()
                                                         End If

                                                     End Try
                                                 End Sub, stateObj)
                    stateObj = Nothing
                Next
                finished.Signal()
                finished.Wait(100000)
                finished.Dispose()
                finished = Nothing

                rootElmt.AppendChild(listsElmt)
                Return listsElmt

            Catch ex As Exception
                returnException(mcModuleName, "ListCampaigns", ex, "", "", gbDebug)
                Return Nothing
            End Try

        End Function

        Private Function SyncListMembers(ByVal CmListID As String, ByVal EwGroupID As Long) As Long
            Dim cProcessInfo As String = ""
            Dim i As Long = 0
            Dim unSubBehavoir As Boolean = False
            Try

                'Get existing Unsubs
                Dim _api As CampaignMonitorAPIWrapper.CampaignMonitorAPI.api = New CampaignMonitorAPI.api()
                ' Dim UnSubList As List(Of CampaignMonitorAPIWrapper.CampaignMonitorAPI.Subscriber)
                Dim UnSubList As Object
                UnSubList = _api.GetUnsubscribed(moMailConfig("ApiKey"), CmListID, Nothing)

                Dim oUsersXml As XmlElement

                GetLocalListIDs()
                'gets the users in a group
                oUsersXml = myWeb.moDbHelper.listDirectory("User", EwGroupID)

                Dim oGroupXml As XmlElement = myWeb.moPageXml.CreateElement("instance")
                oGroupXml.InnerXml = myWeb.moDbHelper.getObjectInstance(dbHelper.objectTypes.Directory, EwGroupID)
                Dim groupDetail As XmlElement = oGroupXml.SelectSingleNode("tblDirectory/cDirXml/Group")
                If groupDetail.GetAttribute("unsubscribebehaviour") = "1" Then
                    unSubBehavoir = True
                End If

                Dim totalInstances As Long = oUsersXml.SelectNodes("user").Count

                Dim Tasks As New SyncMember()
                Dim workerThreads As Int16
                Dim compPortThreads As Int16
                System.Threading.ThreadPool.GetMaxThreads(workerThreads, compPortThreads)
                System.Threading.ThreadPool.SetMaxThreads(workerThreads, compPortThreads)
                Dim finished As New CountdownEvent(1)

                Dim oElmt As XmlElement
                For Each oElmt In oUsersXml.SelectNodes("user")
                    Dim bSyncUser As Boolean = True
                    For Each unSub In UnSubList
                        If unSub.EmailAddress = oElmt.SelectSingleNode("User/Email").InnerText Then bSyncUser = False
                    Next
                    If bSyncUser Then
                        Dim stateObj As New SyncMember.MemberObj()
                        stateObj.oUserElmt = oElmt
                        stateObj.myWeb = myWeb
                        stateObj.CmListID = CmListID
                        stateObj.CMBouncedListId = CMBouncedListId
                        stateObj.CMDeletedListId = CMDeletedListId
                        stateObj.CMSupressionListId = CMSupressionListId
                        stateObj.CMUnconfirmedListId = CMUnconfirmedListId
                        stateObj.CMUnSubscribedListId = CMUnSubscribedListId
                        stateObj.EwGroupID = EwGroupID
                        stateObj.UnSubRemove = unSubBehavoir
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

            Catch ex As Exception
                returnException(mcModuleName, "ListCampaigns", ex, "", "", gbDebug)
            End Try

        End Function

        Private Function getCampaignReports(ByVal PageId As Integer) As XmlElement

            Dim cProcessInfo As String = ""
            Dim ReportElmt As XmlElement
            Dim sSql As String = "select * from  tblActivityLog where nStructId = " & PageId & " and nActivityType = 3"
            Dim oDs As DataSet
            Dim oElmt As XmlElement
            Try
                Dim _api As CampaignMonitorAPIWrapper.CampaignMonitorAPI.api = New CampaignMonitorAPI.api()

                ReportElmt = moPageXML.CreateElement("Report")
                oDs = myWeb.moDbHelper.GetDataSet(sSql, "Campaign", "Report")

                With oDs.Tables(0)
                    .Columns("nActivityKey").ColumnMapping = Data.MappingType.Attribute
                    .Columns("nUserDirId").ColumnMapping = Data.MappingType.Attribute
                    .Columns("nStructId").ColumnMapping = Data.MappingType.Attribute
                    .Columns("nArtId").ColumnMapping = Data.MappingType.Attribute
                    .Columns("dDateTime").ColumnMapping = Data.MappingType.Attribute
                    .Columns("nActivityType").ColumnMapping = Data.MappingType.Attribute
                    '  .Columns("cActivityDetail").ColumnMapping = Data.MappingType.SimpleContent
                    .Columns("cSessionId").ColumnMapping = Data.MappingType.Attribute
                    '.Columns("cIPAddress").ColumnMapping = Data.MappingType.Attribute
                    .Columns("nOtherId").ColumnMapping = Data.MappingType.Attribute
                End With

                ReportElmt.InnerXml = oDs.GetXml()
                ReportElmt = ReportElmt.FirstChild
                Dim result As Object
                Dim campaign As CampaignMonitorAPIWrapper.CampaignMonitorAPI.Campaign
                result = _api.GetClientCampaigns(moMailConfig("ApiKey"), moMailConfig("ClientId"))

                For Each oElmt In ReportElmt.SelectNodes("Campaign")
                    oElmt.InnerXml = oElmt.SelectSingleNode("cActivityDetail").InnerText
                    oElmt.InnerXml = oElmt.FirstChild.InnerXml
                    If Not oElmt.SelectSingleNode("CampaignId") Is Nothing Then
                        Dim CampaignId As String = oElmt.SelectSingleNode("CampaignId").InnerText
                        For Each campaign In result
                            If campaign.CampaignID = CampaignId Then
                                oElmt.SetAttribute("name", campaign.Name)
                                oElmt.SetAttribute("sentDate", campaign.SentDate)
                                oElmt.SetAttribute("recipients", campaign.TotalRecipients)
                            End If
                        Next
                        If oElmt.GetAttribute("name") = "" Then
                            'we can't find it lets try for a summary
                            Dim campSummary As Object
                            campSummary = _api.GetCampaignSummary(moMailConfig("ApiKey"), CampaignId)
                            If TypeOf campSummary Is CampaignMonitorAPIWrapper.CampaignMonitorAPI.CampaignSummary Then
                                oElmt.SetAttribute("bounced", campSummary.Bounced)
                            Else
                                oElmt.SetAttribute("name", campSummary.Message)
                            End If
                        End If
                    End If
                Next

                Return ReportElmt

            Catch ex As Exception
                returnException(mcModuleName, "getCampaignReports", ex, "", "", gbDebug)
                Return Nothing
            End Try

        End Function

        Private Function getListId(ByVal ListName As String) As String
            PerfMon.Log("Messaging", "getListId")

            Dim _api As CampaignMonitorAPIWrapper.CampaignMonitorAPI.api = New CampaignMonitorAPI.api()
            Dim Lists As Object
            Dim List As CampaignMonitorAPIWrapper.CampaignMonitorAPI.List
            Dim hLists As New Hashtable


            Try
                Lists = _api.GetClientLists(moMailConfig("ApiKey"), moMailConfig("ClientID"))
                For i = 0 To UBound(Lists)
                    List = Lists(i)

                    If Not hLists.ContainsKey(List.Name) Then
                        hLists.Add(List.Name, List.ListID)
                    End If

                Next

                If hLists(ListName) Is Nothing Then
                    _api.CreateList(moMailConfig("ApiKey"), moMailConfig("ClientID"), ListName, "", False, "")

                    hLists = New Hashtable
                    Lists = _api.GetClientLists(moMailConfig("ApiKey"), moMailConfig("ClientID"))
                    For i = 0 To UBound(Lists)
                        List = Lists(i)
                        If Not hLists.ContainsKey(List.Name) Then
                            hLists.Add(List.Name, List.ListID)
                        End If
                    Next
                End If

                Return hLists(ListName)

            Catch ex As Exception
                returnException(mcModuleName, "getListId", ex, "", "", gbDebug)
                Return Nothing
            End Try
        End Function

        Public Sub maintainUserInGroup(ByVal nUserId As Long, ByVal nGroupId As Long, ByVal remove As Boolean, Optional ByVal cUserEmail As String = Nothing, Optional ByVal cGroupName As String = Nothing, Optional ByVal isLast As Boolean = True)
            PerfMon.Log("Messaging", "maintainUserInGroup")

            Dim oUserXml As New XmlDocument
            Dim oGroupXml As New XmlDocument
            Dim fullName As String
            Dim email As String = Nothing

            Try

                ' Dim _api As CampaignMonitorAPIWrapper.CampaignMonitorAPI.api = New CampaignMonitorAPI.api()
                If BulkSubscribes.Count = 0 Then
                    Dim sUserXml As String = myWeb.moDbHelper.getObjectInstance(dbHelper.objectTypes.Directory, nUserId)
                    If sUserXml <> "" Then
                        oUserXml.LoadXml(sUserXml)
                        If Not oUserXml.DocumentElement.SelectSingleNode("/tblDirectory/cDirXml/User") Is Nothing Then
                            fullName = oUserXml.DocumentElement.SelectSingleNode("/tblDirectory/cDirXml/User/FirstName").InnerText & " " & oUserXml.DocumentElement.SelectSingleNode("/tblDirectory/cDirXml/User/LastName").InnerText
                            email = oUserXml.DocumentElement.SelectSingleNode("/tblDirectory/cDirXml/User/Email").InnerText
                        End If
                    End If
                Else
                    fullName = BulkSubscribes(0).FullName
                    email = BulkSubscribes(0).Email
                End If

                If Not email Is Nothing Then

                    Dim groupName As String = myWeb.moDbHelper.getNameByKey(dbHelper.objectTypes.Directory, nGroupId)
                    'test that is group

                    If Not groupName.StartsWith("CM") Then
                        'If the list doesn't exist create it
                        Dim CmListID As String = getListId(groupName)
                        If Not remove Then
                            BulkSubscribes.Add(New ListSubscribe() With {.Add = True, .CmListID = CmListID, .oUser = oUserXml.FirstChild})
                        Else
                            BulkSubscribes.Add(New ListSubscribe() With {.Add = False, .CmListID = CmListID, .oUser = oUserXml.FirstChild})
                        End If
                    End If
                End If

                If isLast Then
                    Dim _api As CampaignMonitorAPIWrapper.CampaignMonitorAPI.api = New CampaignMonitorAPI.api()
                    Dim totalInstances As Int32 = BulkSubscribes.Count

                    Dim Tasks As New SyncMember()
                    Dim workerThreads As Int16
                    Dim compPortThreads As Int16
                    System.Threading.ThreadPool.GetMaxThreads(workerThreads, compPortThreads)
                    System.Threading.ThreadPool.SetMaxThreads(workerThreads, compPortThreads)
                    Dim doneEvents(totalInstances) As System.Threading.ManualResetEvent
                    Dim i As Int32 = 0

                    For Each subscribe In BulkSubscribes
                        If subscribe.Add Then
                            Dim stateObj As New SyncMember.MemberObj()
                            stateObj.oUserElmt = subscribe.oUser
                            stateObj.myWeb = myWeb
                            stateObj.CmListID = subscribe.CmListID
                            stateObj.CMBouncedListId = CMBouncedListId
                            stateObj.CMDeletedListId = CMDeletedListId
                            stateObj.CMSupressionListId = CMSupressionListId
                            stateObj.CMUnconfirmedListId = CMUnconfirmedListId
                            stateObj.CMUnSubscribedListId = CMUnSubscribedListId
                            System.Threading.ThreadPool.QueueUserWorkItem(New System.Threading.WaitCallback(AddressOf Tasks.SyncSingleMember), stateObj)
                            stateObj = Nothing
                            i = i + 1
                        Else
                            _api.Unsubscribe(moMailConfig("ApiKey"), subscribe.CmListID, subscribe.oUser.SelectSingleNode("cDirXml/User/Email").InnerText)
                        End If
                    Next

                    BulkSubscribes.Clear()

                End If

            Catch ex As Exception
                returnException(mcModuleName, "maintainUserInGroup", ex, "", "", gbDebug)
            End Try
        End Sub

    End Class

    Public Class ListSubscribe
        Dim _add As Boolean
        Dim _CmListID As String
        Dim _email As String
        Dim _fullname As String
        Dim _userXml As XmlElement

        Public Property Add As Boolean
            Get
                Return _add
            End Get
            Set(ByVal value As Boolean)
                _add = value
            End Set
        End Property

        Public Property CmListID As String
            Get
                Return _CmListID
            End Get
            Set(ByVal value As String)
                _CmListID = value
            End Set
        End Property

        Public Property Email As String
            Get
                Return _email
            End Get
            Set(ByVal value As String)
                _email = value
            End Set
        End Property

        Public Property FullName As String
            Get
                Return _fullname
            End Get
            Set(ByVal value As String)
                _fullname = value
            End Set
        End Property

        Public Property oUser As XmlElement
            Get
                Return _userXml
            End Get
            Set(ByVal value As XmlElement)
                _userXml = value
            End Set
        End Property

    End Class

    Public Class Activities
        Inherits Protean.Messaging

        Overloads Function SendMailToList_Queued(ByVal nPageId As Integer, ByVal cEmailXSL As String, ByVal cGroups As String, ByVal cFromEmail As String, ByVal cFromName As String, ByVal cSubject As String) As Boolean
            PerfMon.Log("Messaging", "SendMailToList_Queued")

            Dim cProcessInfo As String = ""

            Try
                Dim moMailConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/mailinglist")
                Dim oAddressDic As UserEmailDictionary = GetGroupEmails(cGroups)
                'max number of bcc

                Dim oWeb As New Cms
                oWeb.InitializeVariables()
                oWeb.mnPageId = nPageId
                oWeb.mbAdminMode = False

                oWeb.mcEwSiteXsl = cEmailXSL
                'get body
                oWeb.mnMailMenuId = moMailConfig("RootPageId")
                Dim oEmailBody As String = oWeb.ReturnPageHTML(oWeb.mnPageId)

                Dim i2 As Integer

                Dim oEmail As Net.Mail.MailMessage = Nothing
                Dim cRepientMail As String

                cFromEmail = cFromEmail.Trim()

                If Protean.Tools.Text.IsEmail(cFromEmail) Then
                    For Each cRepientMail In oAddressDic.Keys
                        'create the message
                        If oEmail Is Nothing Then
                            oEmail = New Net.Mail.MailMessage
                            oEmail.IsBodyHtml = True
                            cProcessInfo = "Sending from: " & cFromEmail
                            oEmail.From = New Net.Mail.MailAddress(cFromEmail, cFromName)
                            oEmail.Body = oEmailBody
                            oEmail.Subject = cSubject
                        End If
                        'if we are not at the bcc limit then we add the addres
                        If i2 < CInt(moMailConfig("BCCLimit")) Then
                            If Protean.Tools.Text.IsEmail(cRepientMail.Trim()) Then
                                cProcessInfo = "Sending to: " & cRepientMail.Trim()
                                oEmail.Bcc.Add(New Net.Mail.MailAddress(cRepientMail.Trim()))
                                i2 += 1
                            End If
                        Else
                            'otherwise we send it
                            cProcessInfo = "Sending queued mail"
                            SendQueuedMail(oEmail, moMailConfig("PickupHost"), moMailConfig("PickupLocation"))
                            'and reset the counter
                            i2 = 0
                            oEmail = Nothing
                        End If
                    Next
                    'try a send after in case we havent reached the last send
                    If i2 < CInt(moMailConfig("BCCLimit")) Then
                        cProcessInfo = "Sending queued mail (last)"
                        SendQueuedMail(oEmail, moMailConfig("PickupHost"), moMailConfig("PickupLocation"))
                        'and reset the counter
                        i2 = 0
                        oEmail = Nothing
                    End If
                    Return True
                Else
                    Return False
                End If

            Catch ex As Exception
                returnException(mcModuleName, "SendMailToList_Queued", ex, "", cProcessInfo, gbDebug)
                Return False
            End Try
        End Function

        Public Overloads Function SendQueuedMail(ByVal oMailn As Net.Mail.MailMessage, ByVal cHost As String, ByVal cPickupLocation As String) As String
            PerfMon.Log("Messaging", "SendQueuedMail")
            Try
                If oMailn Is Nothing Then Return "No Email Supplied"
                Dim oSmtpn As New SmtpClient
                oSmtpn.Host = cHost '"127.0.0.1"
                oSmtpn.PickupDirectoryLocation = cPickupLocation '"C:\Inetpub\mailroot\Pickup"
                oSmtpn.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory
                '=#=#=#=#=#=#=#=#=#=#=#=#=#==#=#=#=
                'Exit Function
                '=#=#=#=#=#=#=#=#=#=#=#=#=#==#=#=#=
                oSmtpn.Send(oMailn)
                Return "Sent"
            Catch ex As Exception
                returnException(mcModuleName, "SendQueuedMail", ex, "", "", gbDebug)
                Return "Error"
            End Try
        End Function

        Public Function AddToList(ListId As String, name As String, email As String, values As System.Collections.Generic.Dictionary(Of String, String)) As Boolean
            PerfMon.Log("Activities", "AddToList")
            Dim cProcessInfo As String = ""
            Dim customCount As Integer = 45
            Try
                Dim moMailConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/mailinglist")

                'do nothing this is a placeholder
                'We have an Xform within this content we need to process.
                Dim apiKey As String = moMailConfig("ApiKey")
                Dim cmAuth As New createsend_dotnet.ApiKeyAuthenticationDetails(moMailConfig("ApiKey"))
                Dim subscriber As New createsend_dotnet.Subscriber(cmAuth, ListId)
                Dim customFields As New List(Of createsend_dotnet.SubscriberCustomField)()

                Dim list As New createsend_dotnet.List(cmAuth, ListId)

                For Each oElmt In values
                    If Not (oElmt.Key = "Name" Or oElmt.Key = "Email") Then
                        'scan the custom feilds to check exists
                        If customCount > 0 Then
                            Dim cfExists As Boolean = False
                            For Each cf In list.CustomFields
                                If cf.FieldName = oElmt.Key Then
                                    cfExists = True
                                End If
                            Next
                            ' if not exists create it
                            If Not cfExists Then
                                Dim opts As New Generic.List(Of String)
                                Dim dt As createsend_dotnet.CustomFieldDataType = createsend_dotnet.CustomFieldDataType.Text
                                If LCase(oElmt.Key).Contains("date") Then
                                    dt = createsend_dotnet.CustomFieldDataType.Date
                                End If
                                Try
                                    list.CreateCustomField(oElmt.Key, dt, opts, True)
                                Catch ex As Exception
                                    'do nothing we probably have too many custom feilds
                                End Try
                            End If

                            Dim customField As New createsend_dotnet.SubscriberCustomField
                            customField.Key = oElmt.Key
                            customField.Value = oElmt.Value
                            customFields.Add(customField)
                        End If
                        customCount = customCount - 1
                    End If
                Next

                Try
                    subscriber.Add(email, name, customFields, True, True)
                Catch ex As Exception
                    If ex.Message.Contains("205") Then
                        subscriber.Update(email, email, name, customFields, True, True)
                    End If
                    cProcessInfo = ex.Message
                End Try


                Return True
            Catch ex As Exception
                returnException(mcModuleName, "AddToList", ex, "", "", gbDebug)
                Return False
            End Try
        End Function

        Public Function RemoveFromList(ListId As String, Email As String) As Boolean
            PerfMon.Log("Activities", "RemoveFromList")
            Try
                Dim moMailConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/mailinglist")

                Dim apiKey As String = moMailConfig("ApiKey")
                Dim cmAuth As New createsend_dotnet.ApiKeyAuthenticationDetails(moMailConfig("ApiKey"))
                Dim subscriber As New createsend_dotnet.Subscriber(cmAuth, ListId)
                'do nothing this is a placeholder

                subscriber.Delete(Email)

                Return True
            Catch ex As Exception
                ' returnException(mcModuleName, "RemoveFromList", ex, "", "", gbDebug)
                Return False
            End Try
        End Function

    End Class

    Public Class SyncMember

        Private Const mcModuleName As String = "Providers.Messaging.Generic.AdminXForms"
        Dim moMailConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/mailinglist")

        Public Class MemberObj
            Public oUserElmt As XmlElement
            Public CmListID As String
            Public CMSupressionListId As Long
            Public CMDeletedListId As Long
            Public CMUnSubscribedListId As Long
            Public CMBouncedListId As Long
            Public CMUnconfirmedListId As Long
            Public myWeb As Protean.Cms
            Public EwGroupID As String
            Public UnSubRemove As Boolean = False
        End Class

        Public Sub SyncSingleMember(Member As MemberObj)
            Dim cTableName As String = ""
            Dim cTableKey As String = ""
            Dim cTableFRef As String = ""
            Dim cProcessInfo As String = ""
            Dim fullName As String
            Dim Email As String
            Dim nUserId As Long = 0
            Try

                Dim localDbHelper As dbHelper = New dbHelper(Member.myWeb)
                Dim memberStatus As Int16 = 1

                Dim _api As CampaignMonitorAPIWrapper.CampaignMonitorAPI.api = New CampaignMonitorAPI.api()
                If Not Member.oUserElmt Is Nothing Then
                    If Not Member.oUserElmt.SelectSingleNode("cDirXml") Is Nothing Then
                        fullName = Member.oUserElmt.SelectSingleNode("cDirXml/User/FirstName").InnerText & " " & Member.oUserElmt.SelectSingleNode("cDirXml/User/LastName").InnerText
                        nUserId = CLng(Member.oUserElmt.SelectSingleNode("nDirKey").InnerText)
                        Email = Member.oUserElmt.SelectSingleNode("cDirXml/User/Email").InnerText
                        If Member.oUserElmt.SelectSingleNode("cDirXml/User/Status") Is Nothing Then
                            memberStatus = 1
                        Else
                            memberStatus = CInt(Member.oUserElmt.SelectSingleNode("cDirXml/User/Status").InnerText)
                        End If
                    Else
                        fullName = Member.oUserElmt.SelectSingleNode("descendant-or-self::FirstName").InnerText & " " & Member.oUserElmt.SelectSingleNode("descendant-or-self::LastName").InnerText
                        nUserId = CLng(Member.oUserElmt.GetAttribute("id"))
                        Email = Member.oUserElmt.SelectSingleNode("descendant-or-self::Email").InnerText
                        If Member.oUserElmt.SelectSingleNode("descendant-or-self::Status") Is Nothing Then
                            memberStatus = 1

                        Else
                            memberStatus = CInt(Member.oUserElmt.SelectSingleNode("descendant-or-self::Status").InnerText)
                        End If
                    End If
                End If

                Dim result As CampaignMonitorAPIWrapper.CampaignMonitorAPI.Result

                Select Case memberStatus
                    Case 1, -1
                        result = _api.AddSubscriber(moMailConfig("ApiKey"), Member.CmListID, Email, fullName)
                    Case 0
                        result = _api.Unsubscribe(moMailConfig("ApiKey"), Member.CmListID, Email)
                End Select

                Select Case result.Code
                    Case "0" 'Success

                    Case "1" 'Invalid email address

                    Case "100" ' Invalid API Key

                    Case "101" 'Invalid(ListID)

                    Case "204" ' In Suppression List
                        _api.AddAndResubscribe(moMailConfig("ApiKey"), Member.CmListID, Email, fullName)
                        localDbHelper.maintainDirectoryRelation(Member.CMSupressionListId, nUserId,,, True,,, False)

                    Case "205" ' Is Deleted
                        localDbHelper.maintainDirectoryRelation(Member.CMDeletedListId, nUserId,,, True,,, False)

                    Case "206" ' Is Unsubscribed
                        If Member.UnSubRemove Then
                            localDbHelper.maintainDirectoryRelation(Member.EwGroupID, nUserId, True)
                            localDbHelper.maintainDirectoryRelation(Member.CMUnSubscribedListId, nUserId,,, True,,, False)
                        Else
                            _api.AddAndResubscribe(moMailConfig("ApiKey"), Member.CmListID, Email, fullName)
                            localDbHelper.maintainDirectoryRelation(Member.CMUnSubscribedListId, nUserId,,, True,,, False)
                        End If

                    Case "207" ' Is Bounced
                        localDbHelper.maintainDirectoryRelation(Member.CMBouncedListId, nUserId,,, True,,, False)

                    Case "208" ' Is Unconfirmed
                        localDbHelper.maintainDirectoryRelation(Member.CMUnconfirmedListId, nUserId,,, True,,, False)

                End Select
                localDbHelper.CloseConnection()
                localDbHelper = Nothing

                cProcessInfo = result.GetType().ToString


            Catch ex As Exception
                returnException(mcModuleName, "SyncSingleMember", ex, "", cProcessInfo, gbDebug)
            End Try
        End Sub

    End Class

    Public Class AddListStats

        Private Const mcModuleName As String = "Providers.Messaging.Generic.AdminXForms"
        Dim moMailConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/mailinglist")

        Public Class ListObj
            Public oMasterList As XmlElement
            Public CmListID As String
            '  Public finished As CountdownEvent
            Public API As CampaignMonitorAPIWrapper.CampaignMonitorAPI.api
        End Class

        Public Sub UpdateListStats(MyList As ListObj)
            Dim cProcessInfo As String = "Updating stats for ListID: " & MyList.CmListID
            Try

                Dim ls As CampaignMonitorAPIWrapper.CampaignMonitorAPI.ListStatistics = MyList.API.GetListStats(moMailConfig("ApiKey"), MyList.CmListID)
                Dim listElmt As XmlElement = MyList.oMasterList.SelectSingleNode("List[@id='" & MyList.CmListID & "']")
                listElmt.SetAttribute("subscribers", ls.TotalActiveSubscribers)
                listElmt.SetAttribute("unsubscribes", ls.TotalUnsubscribes)
                listElmt.SetAttribute("deleted", ls.TotalDeleted)
                listElmt.SetAttribute("bounces", ls.TotalBounces)

            Catch ex As Exception
                returnException(mcModuleName, "UpdateListStats", ex, "", cProcessInfo, gbDebug)
            Finally
                '   MyList.finished.Signal()
            End Try
        End Sub

    End Class

End Class



