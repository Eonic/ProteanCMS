'***********************************************************************
' $Library:     Protean.Providers.messaging.base
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
Imports Protean.Cms
Imports Protean.Tools
Imports Protean.Tools.Xml
Imports System.Net.Mail
Imports System.Reflection
Imports System.Net
Imports VB = Microsoft.VisualBasic
Imports System

Namespace Providers
    Namespace Messaging

        Public Class BaseProvider
            Private _AdminXforms As Object
            Private _AdminProcess As Object
            Private _Activities As Object
            Private Const mcModuleName As String = "Protean.Providers.Messaging"


            Public Property AdminXforms() As Object
                Set(ByVal value As Object)
                    _AdminXforms = value
                End Set
                Get
                    Return _AdminXforms
                End Get
            End Property

            Public Property AdminProcess() As Object
                Set(ByVal value As Object)
                    _AdminProcess = value
                End Set
                Get
                    Return _AdminProcess
                End Get
            End Property

            Public Property Activities() As Object
                Set(ByVal value As Object)
                    _Activities = value
                End Set
                Get
                    Return _Activities
                End Get
            End Property

            Public Sub New(ByRef myWeb As Protean.Cms, ByVal ProviderName As String)
                Dim cProgressInfo As String = ""
                Try
                    Dim calledType As Type
                    If ProviderName = "" Then
                        ProviderName = "Protean.Providers.Messaging.EonicProvider"
                        calledType = System.Type.GetType(ProviderName, True)
                    Else
                        Dim castObject As Object = WebConfigurationManager.GetWebApplicationSection("protean/messagingProviders")
                        Dim moPrvConfig As Protean.ProviderSectionHandler = castObject
                        Dim ourProvider As Object = moPrvConfig.Providers(ProviderName)
                        Dim assemblyInstance As [Assembly]
                        '= [Assembly].Load(moPrvConfig.Providers(ProviderName).Type)

                        If ourProvider.parameters("path") <> "" Then
                            cProgressInfo = goServer.MapPath(ourProvider.parameters("path"))
                            assemblyInstance = [Assembly].LoadFrom(goServer.MapPath(ourProvider.parameters("path")))
                        Else
                            assemblyInstance = [Assembly].Load(ourProvider.Type)
                        End If

                        If ourProvider.parameters("className") <> "" Then
                            ProviderName = ourProvider.parameters("className")
                        End If

                        If ourProvider.parameters("rootClass") = "" Then
                            calledType = assemblyInstance.GetType("Protean.Providers.Messaging." & ProviderName, True)
                        Else
                            'calledType = assemblyInstance.GetType(ourProvider.parameters("rootClass") & ".Providers.Messaging", True)
                            calledType = assemblyInstance.GetType(ourProvider.parameters("rootClass") & ".Providers.Messaging." & ProviderName, True)
                        End If
                    End If

                    Dim o As Object = Activator.CreateInstance(calledType)

                    Dim args(4) As Object
                    args(0) = _AdminXforms
                    args(1) = _AdminProcess
                    args(2) = _Activities
                    args(3) = Me
                    args(4) = myWeb

                    calledType.InvokeMember("Initiate", BindingFlags.InvokeMethod, Nothing, o, args)

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "New", ex,, cProgressInfo & " - " & ProviderName & " Could Not be Loaded", gbDebug)
                End Try

            End Sub
        End Class

        Public Class EonicProvider

            Public Sub New()
                'do nothing
            End Sub

            Public Sub Initiate(ByRef _AdminXforms As Object, ByRef _AdminProcess As Object, ByRef _Activities As Object, ByRef MemProvider As Object, ByRef myWeb As Protean.Cms)

                MemProvider.AdminXforms = New AdminXForms(myWeb)
                MemProvider.AdminProcess = New AdminProcess(myWeb)
                MemProvider.AdminProcess.oAdXfm = MemProvider.AdminXforms
                'MemProvider.Activities = New Activities()


            End Sub

            Public Class AdminXForms
                Inherits Cms.Admin.AdminXforms
                Private Const mcModuleName As String = "Providers.Messaging.Generic.AdminXForms"

                Sub New(ByRef aWeb As Cms)
                    MyBase.New(aWeb)
                End Sub

                Public Function xFrmPreviewNewsLetter(ByVal nPageId As Integer, ByRef oPageDetail As XmlElement, Optional ByVal cSubject As String = "") As XmlElement
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

                        ' Uncomment for personalised
                        ' ''oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Personalised", "", "Send Personalised")
                        ' ''Dim cSQL As String = "SELECT tblDirectory.nDirKey, tblDirectory.cDirName" & _
                        ' ''" FROM tblDirectory INNER JOIN" & _
                        ' ''" tblDirectoryRelation ON tblDirectory.nDirKey = tblDirectoryRelation.nDirChildId INNER JOIN" & _
                        ' ''" tblDirectory Role ON tblDirectoryRelation.nDirParentId = Role.nDirKey" & _
                        ' ''" WHERE (tblDirectory.cDirSchema = N'User') AND (Role.cDirSchema = N'Role') AND (Role.cDirName = N'Administrator')" & _
                        ' ''" ORDER BY tblDirectory.cDirName"
                        ' ''Dim oDre As SqlDataReader = moDbhelper.getDataReader(cSQL)
                        ' ''Dim oSelElmt As XmlElement = MyBase.addSelect1(oFrmElmt, "cUsers", True, "Select admin user to send to", "short", ApperanceTypes.Minimal)
                        ' ''Do While oDre.Read
                        ' ''    MyBase.addOption(oSelElmt, oDre(1), oDre(0))
                        ' ''Loop
                        ' ''MyBase.addBind("cUsers", "cUsers")
                        ' ''MyBase.addSubmit(oFrmElmt, "SendPersonalised", "Send Personalised")

                        MyBase.Instance.InnerXml = "<cEmail/><cUsers/>"

                        If MyBase.isSubmitted Then
                            MyBase.updateInstanceFromRequest()
                            MyBase.validate()
                            Dim oEmailElmt As XmlElement = MyBase.Instance.SelectSingleNode("cEmail")
                            If Not is_valid_email(oEmailElmt.InnerText) Then
                                MyBase.addNote(oElmt, xForm.noteTypes.Alert, "Incorrect Email Address Supplied")
                                MyBase.valid = False
                            End If
                            If MyBase.valid Then
                                Dim moMailConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/mailinglist")
                                Dim cEmail As String = MyBase.Instance.SelectSingleNode("cEmail").InnerText
                                'first we will only deal with unpersonalised
                                Dim oMessager As New Protean.Messaging(myWeb.msException)
                                'get the subject
                                Dim oMessaging As New Activities(myWeb.msException)

                                Dim cMailingXsl As String = moMailConfig("MailingXsl")
                                If cMailingXsl = "" Then cMailingXsl = "/xsl/mailer/mailerStandard.xsl"
                                Dim ofs As New Protean.fsHelper(myWeb.moCtx)
                                cMailingXsl = ofs.checkCommonFilePath(cMailingXsl)



                                If oMessaging.SendSingleMail_Direct(nPageId, cMailingXsl, cEmail, moMailConfig("SenderEmail"), moMailConfig("SenderName"), cSubject) Then
                                    'add mssage and return to form so they can sen another

                                    Dim oMsgElmt As XmlElement = oPageDetail.OwnerDocument.CreateElement("Content")
                                    oMsgElmt.SetAttribute("type", "Message")
                                    oMsgElmt.InnerText = "Messages Sent"
                                    oPageDetail.AppendChild(oMsgElmt)
                                Else
                                    Dim oMsgElmt As XmlElement = oPageDetail.OwnerDocument.CreateElement("Content")
                                    oMsgElmt.SetAttribute("type", "Message")
                                    oMsgElmt.InnerText = "Message Failed Check Config"
                                    oPageDetail.AppendChild(oMsgElmt)
                                End If
                            End If
                        End If

                        MyBase.addValues()
                        Return MyBase.moXformElmt

                    Catch ex As Exception
                        returnException(myWeb.msException, mcModuleName, "addInput", ex, "", cProcessInfo, gbDebug)
                        Return Nothing
                    End Try
                End Function

                Public Overloads Function xFrmSendNewsLetter(ByVal nPageId As Integer, ByVal cPageName As String, ByVal cDefaultEmail As String, ByVal cDefaultEmailName As String, ByRef oPageDetail As XmlElement) As XmlElement
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

                        oElmt2 = MyBase.addInput(oCol1, "cSubject", True, "Subject", "required long")
                        MyBase.addBind("cSubject", "cSubject", "true()")
                        oElmt2.AppendChild(oElmt.OwnerDocument.CreateElement("value"))


                        Dim cSQL As String = "SELECT nDirKey, cDirName  FROM tblDirectory WHERE (cDirSchema = 'Group') ORDER BY cDirName"
                        Dim oDre As SqlDataReader = moDbHelper.getDataReader(cSQL)
                        Dim oSelElmt As XmlElement = MyBase.addSelect(oCol2, "cGroups", True, "Select Groups to send to", "required multiline", ApperanceTypes.Full)
                        Do While oDre.Read
                            MyBase.addOption(oSelElmt, oDre(1), oDre(0))
                        Loop
                        MyBase.addBind("cGroups", "cGroups", "true()")

                        oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Send", "", "")

                        MyBase.addSubmit(oFrmElmt, "SendUnpersonalised", "Send Unpersonalised")
                        ' Uncomment for personalised
                        'MyBase.addSubmit(oFrmElmt, "SendPersonalised", "Send Personalised")

                        MyBase.Instance.InnerXml = "<cGroups/><cDefaultEmail>" & cDefaultEmail & "</cDefaultEmail><cDefaultEmailName>" & cDefaultEmailName & "</cDefaultEmailName><cSubject>" & cPageName & "</cSubject>"

                        If MyBase.isSubmitted Then
                            MyBase.updateInstanceFromRequest()
                            MyBase.validate()
                            Dim oEmailElmt As XmlElement = MyBase.Instance.SelectSingleNode("cDefaultEmail")
                            If Not Tools.Text.IsEmail(oEmailElmt.InnerText.Trim()) Then
                                MyBase.addNote(oElmt, xForm.noteTypes.Alert, "Incorrect Email Address Supplied")
                                MyBase.valid = False
                            End If
                            If MyBase.valid Then
                                Dim moMailConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/mailinglist")
                                'get the individual elements
                                Dim oMessaging As New Protean.Messaging(myWeb.msException)
                                'First we need to get the groups we are sending to
                                Dim oGroupElmt As XmlElement = MyBase.Instance.SelectSingleNode("cGroups")
                                Dim oFromEmailElmt As XmlElement = MyBase.Instance.SelectSingleNode("cDefaultEmail")
                                Dim oFromNameElmt As XmlElement = MyBase.Instance.SelectSingleNode("cDefaultEmailName")
                                Dim oSubjectElmt As XmlElement = MyBase.Instance.SelectSingleNode("cSubject")
                                'get the email addresses for these groups
                                Dim cMailingXsl As String = moMailConfig("MailingXsl")
                                If cMailingXsl = "" Then cMailingXsl = "/xsl/mailer/mailerStandard.xsl"
                                Dim ofs As New Protean.fsHelper(myWeb.moCtx)
                                cMailingXsl = ofs.checkCommonFilePath(cMailingXsl)

                                Dim bResult As Boolean = oMessaging.SendMailToList_Queued(nPageId, cMailingXsl, oGroupElmt.InnerText, oFromEmailElmt.InnerText, oFromNameElmt.InnerText, oSubjectElmt.InnerText)


                                ' Log the result
                                If bResult Then
                                    'moDbHelper.logActivity(dbHelper.ActivityType.Email, myWeb.mnUserId, nPageId, , oGroupElmt.InnerText)
                                    moDbHelper.CommitLogToDB(dbHelper.ActivityType.NewsLetterSent, myWeb.mnUserId, myWeb.moSession.SessionID, Now, myWeb.mnPageId, 0, "", True)
                                    Dim cGroupStr As String = "<Groups><Group>" & Replace(oGroupElmt.InnerText, ",", "</Group><Group>") & "</Group></Groups>"
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
                        returnException(myWeb.msException, mcModuleName, "addInput", ex, "", cProcessInfo, gbDebug)
                        Return Nothing
                    End Try
                End Function

                Public Overloads Function xFrmAddModule(ByVal pgid As Long, ByVal position As String) As XmlElement
                    Dim oFrmElmt As XmlElement
                    Dim oSelElmt As XmlElement
                    Dim sImgPath As String = ""

                    Dim cProcessInfo As String = ""
                    Dim oXformDoc As XmlDocument = New XmlDocument
                    Try

                        If moRequest("cModuleBox") <> "" Then

                            xFrmEditContent(0, "Module/" & moRequest("cModuleType"), pgid, moRequest("cPosition"))
                            Return MyBase.moXformElmt

                        Else
                            MyBase.NewFrm("EditPageLayout")
                            MyBase.submission("AddModule", "", "post", "form_check(this)")
                            MyBase.Instance.InnerXml = "<Module position=""" & position & """></Module>"
                            oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Add Module", "", "Select Module Type")
                            MyBase.addInput(oFrmElmt, "nStructParId", True, "ParId", "hidden")
                            MyBase.addInput(oFrmElmt, "cPosition", True, "Position", "hidden")
                            MyBase.addBind("cPosition", "Module/@position", "true()")
                            MyBase.addNote(oFrmElmt, xForm.noteTypes.Hint, "Click the image to select Module Type")

                            oSelElmt = MyBase.addSelect1(oFrmElmt, "cModuleType", True, "", "PickByImage", xForm.ApperanceTypes.Full)

                            EnumberateManifestOptions(oSelElmt, "/xsl/Mailer", "ModuleTypes/ModuleGroup", "Module", True)
                            EnumberateManifestOptions(oSelElmt, "/ewcommon/xsl/Mailer", "ModuleTypes/ModuleGroup", "Module", False)

                            If MyBase.isSubmitted Or goRequest.Form("ewsubmit.x") <> "" Or goRequest.Form("cModuleType") <> "" Then
                                MyBase.updateInstanceFromRequest()
                                MyBase.validate()
                                If MyBase.valid Then
                                    'Do nothing
                                    'or redirect to content form
                                    xFrmEditContent(0, "Module/" & moRequest("cModuleType"), pgid, moRequest("cPosition"))
                                End If
                            End If
                            MyBase.addValues()
                            Return MyBase.moXformElmt
                        End If

                    Catch ex As Exception
                        returnException(myWeb.msException, mcModuleName, "addInput", ex, "", cProcessInfo, gbDebug)
                        Return Nothing
                    End Try
                End Function

            End Class

            Public Class AdminProcess
                Inherits Cms.Admin

                Dim _oAdXfm As Protean.Providers.Messaging.EonicProvider.AdminXForms

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

                Public Function MailingListProcess(ByRef oPageDetail As XmlElement, ByRef oWeb As Cms, Optional ByRef sAdminLayout As String = "", Optional ByRef cCmd As String = "", Optional ByRef bLoadStructure As Boolean = False, Optional ByRef sEditContext As String = "", Optional bClearEditContext As Boolean = False) As String
                    Dim cRetVal As String = ""
                    Dim cSQL As String = ""

                    Try
                        Dim moMailConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/mailinglist")
                        If moMailConfig Is Nothing Then Return ""

                        Dim nMailMenuRoot As Long = moMailConfig("RootPageId")
                        'if we hit this we want to default to the mail root Id
                        If myWeb.mnPageId = gnTopLevel And cCmd <> "NewMail" And cCmd <> "NormalMail" And cCmd <> "AdvancedMail" Then
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

                                Dim nNewsletterRoot As Integer = CInt("0" & moMailConfig("RootPageId"))
                                If Not myWeb.moDbHelper.checkPageExist(nNewsletterRoot) Then nNewsletterRoot = 0

                                If nNewsletterRoot = 0 Then
                                    Dim defaultPageXml As String = "<DisplayName title="""" linkType=""internal"" exclude=""false"" noindex=""false""/><Images><img class=""icon"" /><img class=""thumbnail"" /><img class=""detail"" /></Images><Description/>"
                                    nNewsletterRoot = myWeb.moDbHelper.insertStructure(0, "", "Newsletters", defaultPageXml, "NewsletterRoot")
                                    Protean.Config.UpdateConfigValue(myWeb, "protean/mailinglist", "RootPageId", CStr(nNewsletterRoot))

                                End If

                                'we want to return here after editing
                                myWeb.moSession("lastPage") = myWeb.mcOriginalURL

                            Case "NormalMail"
                                sAdminLayout = ""
                                cCmd = "NormalMail"

                                myWeb.mcEwSiteXsl = cMailingXsl
                                'we want to return here after editing
                                If Not myWeb.mbPopupMode Then
                                    If Not myWeb.mbSuppressLastPageOverrides Then myWeb.moSession("lastPage") = "?ewCmd=NormalMail&pgid=" & myWeb.mnPageId 'myWeb.mcOriginalURL 'not this if being redirected after editing layout for instance.
                                End If

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
                                '    Dim oMessaging As New Protean.Messaging
                                '    If oMessaging.SendSingleMail_Queued(myWeb.mnPageId, moMailConfig("MailingXsl"), cEmail, moMailConfig("SenderEmail"), moMailConfig("SenderName"), cSubject) Then
                                '        'add mssage and return to form so they can sen another

                                '        Dim oMsgElmt As XmlElement = oPageDetail.OwnerDocument.CreateElement("Content")

                                '        oMsgElmt.SetAttribute("type", "Message")
                                '        oMsgElmt.InnerText = "Messages Sent"
                                '        oPageDetail.AppendChild(oMsgElmt)
                                '    End If
                                'End If

                            Case "SendMail"
                                sAdminLayout = "PreviewMail"
                                'get subject
                                Dim cSubject As String = ""
                                oWeb.mnMailMenuId = moMailConfig("RootPageId")
                                Dim oLocElmt As XmlElement = oWeb.GetPageXML.SelectSingleNode("descendant-or-self::MenuItem[@id=" & myWeb.mnPageId & "]")
                                If Not oLocElmt Is Nothing Then cSubject = oLocElmt.GetAttribute("name")

                                oPageDetail.AppendChild(oAdXfm.xFrmSendNewsLetter(myWeb.mnPageId, cSubject, moMailConfig("SenderEmail"), moMailConfig("SenderName"), oPageDetail))

                                'If moAdXfm.valid Then

                                '    'get the individual elements
                                '    Dim oMessaging As New Protean.Messaging
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
                        End Select

                        sEditContext = EditContext
                        bClearEditContext = clearEditContext
                        myWeb.moSession("ewCmd") = cCmd
                        mcEwCmd = cCmd
                        'Return cRetVal
                        Return ""
                    Catch ex As Exception
                        returnException(myWeb.msException, mcModuleName, "NewsLetterProcess", ex, "", "", gbDebug)
                        Return ""
                    End Try
                End Function

                Public Sub MailingListAdminMenu(ByRef oAdminMenu As XmlElement)

                    'This is just a placeholder for overloading

                End Sub

                Public Sub SyncUser(ByRef nUserId As Integer)

                    'This is just a placeholder for overloading

                End Sub



                Public Sub maintainUserInGroup(ByVal nUserId As Long, ByVal nGroupId As Long, ByVal remove As Boolean, Optional ByVal cUserEmail As String = Nothing, Optional ByVal cGroupName As String = Nothing, Optional isLast As Boolean = False)
                    PerfMon.Log("Messaging", "maintainUserInGroup")
                    Try
                        'do nothing this is a placeholder

                    Catch ex As Exception
                        returnException(myWeb.msException, mcModuleName, "maintainUserInGroup", ex, "", "", gbDebug)
                    End Try
                End Sub

            End Class

            Public Class Activities
                Inherits Protean.Messaging

                Public Sub New(ByRef sException As String)
                    MyBase.New(sException)
                End Sub

                Overloads Function SendMailToList_Queued(ByVal nPageId As Integer, ByVal cEmailXSL As String, ByVal cGroups As String, ByVal cFromEmail As String, ByVal cFromName As String, ByVal cSubject As String) As Boolean
                    PerfMon.Log("Messaging", "SendMailToList_Queued")

                    Dim cProcessInfo As String = ""

                    Try
                        Dim moMailConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/mailinglist")
                        Dim oAddressDic As UserEmailDictionary = GetGroupEmails(cGroups)
                        'max number of bcc

                        Dim oWeb As New Cms
                        oWeb.InitializeVariables()
                        oWeb.Open()
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

                        returnException(MyBase.msException, mcModuleName, "SendMailToList_Queued", ex, "", cProcessInfo, gbDebug)
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
                        returnException(msException, mcModuleName, "SendQueuedMail", ex, "", "", gbDebug)
                        Return "Error"
                    End Try
                End Function

                Public Function AddToList(ListId As String, Name As String, Email As String, values As IDictionary) As Boolean
                    PerfMon.Log("Activities", "AddToList")
                    Try
                        'do nothing this is a placeholder
                        Return Nothing
                    Catch ex As Exception
                        returnException(MyBase.msException, mcModuleName, "AddToList", ex, "", "", gbDebug)
                        Return Nothing
                    End Try
                End Function

                Public Function RemoveFromList(ListId As String, Email As String) As Boolean
                    PerfMon.Log("Activities", "RemoveFromList")
                    Try
                        'do nothing this is a placeholder
                        Return Nothing
                    Catch ex As Exception
                        returnException(MyBase.msException, mcModuleName, "RemoveFromList", ex, "", "", gbDebug)
                        Return Nothing
                    End Try
                End Function


            End Class
        End Class
    End Namespace
End Namespace


