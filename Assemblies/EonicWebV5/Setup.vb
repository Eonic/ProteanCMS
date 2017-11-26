Option Strict Off
Option Explicit On
Imports System.Xml
Imports System.Web.HttpUtility
Imports System.Web.Configuration
Imports System.Configuration
Imports System.Data
Imports System.Data.SqlClient
Imports VB = Microsoft.VisualBasic
Imports System.Threading
Imports System.Text
Imports System.Text.RegularExpressions
Imports SR = System.Reflection
Imports System.Web.UI.WebControls
Imports System.Security.Principal
Imports System.IO
Imports System.Xml.XPath
Imports System

Public Class Setup

    'This is the version of the new build before the update......
    Public mnCurrentVersion As String = "4.1.0.45"

    'Session EonicWeb Details
    Public mbEwMembership As Boolean
    Public mbEwCart As Boolean
    Public mbEwGiftList As Boolean
    Public moPageXml As XmlDocument = New XmlDocument
    Public mbOutputXml As Boolean = False

    Public moCtx As System.Web.HttpContext

    Public goApp As System.Web.HttpApplicationState
    Public goRequest As System.Web.HttpRequest
    Public goResponse As System.Web.HttpResponse
    Public goSession As System.Web.SessionState.HttpSessionState
    Public goServer As System.Web.HttpServerUtility

    Public goConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("eonic/web")

    Public myWeb As Web
    Public WithEvents moDbHelper As Eonic.Web.dbHelper

    Public gnTopLevel As Integer
    Public gnPageId As Integer = 0
    Public mnArtId As Integer = 0
    Public mnUserId As Integer = 0
    Public mcEwCmd As String

    Public mcModuleName As String = "Eonic.Setup"
    Public gbDebug As Boolean = False

    Dim cStep As String = ""
    Public oResponse As XmlElement
    Dim oContentElmt As XmlElement
    Public cPostFlushActions As String = ""

    Dim oTransform As New Eonic.XmlHelper.Transform()
    Dim msRedirectOnEnd As String = ""
    Dim mbSchemaExists As Boolean = False
    Dim mbDBExists As Boolean = False

#Region "ErrorHandling"

    'for anything controlling web
    Public Event OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)

    Private Sub _OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs) Handles moDbHelper.OnError
        'RaiseEvent OnError(sender, e)
        Err.Raise(513, e.ProcedureName, e.AddtionalInformation & " - " & e.Exception.Message, , )
    End Sub

    Protected Overridable Sub OnComponentError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs) Handles moDbHelper.OnError
        'deals with the error
        returnException(e.ModuleName, e.ProcedureName, e.Exception, "/ewcommon/xsl/admin/setup.xsl", e.AddtionalInformation, gbDebug)
        'close connection pooling
        If Not myWeb.moDbHelper Is Nothing Then
            Try
                myWeb.moDbHelper.CloseConnection()
            Catch ex As Exception

            End Try
        End If



        'then raises a public event
        RaiseEvent OnError(sender, e)
    End Sub

#End Region


#Region "Class Initialize Procedures"

    Public Sub New()

        Me.New(System.Web.HttpContext.Current)

    End Sub

    Public Sub New(ByVal Context As System.Web.HttpContext)

        Dim sProcessInfo As String = ""
        Try
            moCtx = Context

            If moCtx Is Nothing Then
                moCtx = System.Web.HttpContext.Current
            End If

            goApp = moCtx.Application
            goRequest = moCtx.Request
            goResponse = moCtx.Response
            goSession = moCtx.Session
            goServer = moCtx.Server

            myWeb = New Web(moCtx)
            ' myWeb.InitializeVariables()

            sProcessInfo = "set session variables"
            mcModuleName = "EonicWeb.Setup"
            ' msException = ""

            ' Set the debug mode
            If Not (goConfig("Debug") Is Nothing) Then
                Select Case LCase(goConfig("Debug"))
                    Case "on" : gbDebug = True
                    Case "off" : gbDebug = False
                    Case Else : gbDebug = False
                End Select
            End If

            If Not goSession Is Nothing Then
                If goSession("nUserId") = Nothing Or goSession("nUserId") = 0 Then
                    'this will get set on close
                Else
                    'lets finally set the user Id from the session
                    mnUserId = goSession("nUserId")
                End If
            End If

            'lets open our DB Helper if database is defined
            If Not goConfig("DatabaseName") = Nothing Then
                myWeb.moDbHelper = New Web.dbHelper(goConfig("DatabaseServer"), goConfig("DatabaseName"), mnUserId, moCtx)
                myWeb.moDbHelper.myWeb = myWeb
                myWeb.moDbHelper.moPageXml = moPageXml

                If myWeb.moDbHelper.checkDBObjectExists("tblContent", Tools.Database.objectTypes.Table) Then
                    mbSchemaExists = True
                    mbDBExists = True
                Else
                    If myWeb.moDbHelper.checkDBObjectExists(goConfig("DatabaseName"), Tools.Database.objectTypes.Database) Then
                        mbDBExists = True
                    End If
                    mnUserId = 1
                End If
            End If

            oResponse = moPageXml.CreateElement("ProgressResponses")

        Catch ex As Exception
            AddResponseError(ex) 'returnException(mcModuleName, "open", ex, "", sProcessInfo, gbDebug)
        End Try

    End Sub

    Public Sub close()

        Dim sProcessInfo As String = ""
        Try

            'if we access base via soap the session is not available
            If mbSchemaExists Then
                If Not goSession Is Nothing Then
                    goSession("nUserId") = mnUserId
                End If
            End If

            moPageXml = Nothing

        Catch ex As Exception
            AddResponseError(ex) 'returnException(mcModuleName, "close", ex, "", sProcessInfo, gbDebug)
        End Try

    End Sub

    Sub AddResponse(ByVal cResponse As String)
        If cPostFlushActions = "" Then
            If Not oResponse Is Nothing Then
                Dim oElmt As XmlElement = oResponse.OwnerDocument.CreateElement("ProgressResponse")
                oElmt.InnerText = cResponse
                oResponse.AppendChild(oElmt)
            End If
        Else
            goResponse.Write("<script language=""javascript"" type=""text/javascript"">$('#result').append('" & Replace(cResponse, "'", "\'") & "<br/>');</script>" & vbCrLf)
        End If
    End Sub

    Sub AddResponseComplete(ByVal cResponse As String, ByVal LinkPath As String)
        If cPostFlushActions = "" Then
            If Not oResponse Is Nothing Then
                Dim oElmt As XmlElement = oResponse.OwnerDocument.CreateElement("ProgressResponse")
                oElmt.InnerText = cResponse
                oResponse.AppendChild(oElmt)
            End If
        Else
            goResponse.Write("<script language=""javascript"" type=""text/javascript"">$('#completeButton').attr('href','" & LinkPath & "');</script>" & vbCrLf)
            goResponse.Write("<script language=""javascript"" type=""text/javascript"">$('#completeButton').html('" & cResponse & "');</script>" & vbCrLf)
            goResponse.Write("<script language=""javascript"" type=""text/javascript"">$('#completeModal').modal('show');</script>" & vbCrLf)
        End If
    End Sub

    Sub AddResponseError(ByVal oEx As Exception, Optional ByVal cProcessInfo As String = "")
        If cPostFlushActions = "" Then
            AddResponse(Replace(Replace(Replace(oEx.ToString, Chr(13), "<br/>"), "&gt;", ">"), "&lt;", "<"))
        Else
            AddResponse("ERROR:" & oEx.Message & " - " & oEx.Source & "<br/>" & Replace(Replace(oEx.StackTrace, vbCr, "<br/>"), vbLf, "<br/>" & cProcessInfo))
        End If
    End Sub

#End Region

#Region "Misc Migration Start Procedures"

    Public Sub GetPageHTML()

        Dim sProcessInfo As String = ""
        Try

            msException = ""
            GetSetupXml()

            msException = ""

            sProcessInfo = "Transform PageXML using XSLT"

            If msRedirectOnEnd <> "" And cPostFlushActions = "" Then
                goResponse.Redirect(msRedirectOnEnd)
                Exit Sub
            End If

            'If Not msException = "" Then
            '    AddResponse(msException)
            '    msException = ""
            'End If

            If mbOutputXml = True Then
                goResponse.Write("<?xml version=""1.0"" encoding=""UTF-8""?>" & moPageXml.OuterXml)
            Else
                goResponse.Buffer = False

                oTransform.XSLFile = CType(goServer.MapPath("/ewcommon/xsl/admin/setup.xsl"), String)
                oTransform.Compiled = False
                oTransform.ProcessTimed(moPageXml, goResponse)
                oTransform = Nothing

                If Not cPostFlushActions = "" Then
                    goResponse.Flush()
                    PostFlushActions()
                End If

            End If
            close()

        Catch ex As Exception

            returnException(mcModuleName, "getPageHtml", ex, "", sProcessInfo, gbDebug)

        End Try

    End Sub

    Public Function ReturnPageHTML(Optional ByVal nPageId As Long = 0, Optional ByVal bReturnBlankError As Boolean = False) As String
        PerfMon.Log("Setup", "ReturnPageHTML")
        Dim sProcessInfo As String = "Transform PageXML using XSLT"
        Dim cPageHTML As String
        Dim icPageWriter As StringWriter = New IO.StringWriter
        Try

            msException = ""
            GetSetupXml()

            msException = ""

            Dim styleFile As String = CType(goServer.MapPath("/ewcommon/xsl/admin/setup.xsl"), String)
            msException = ""

            Dim oTransform As New Eonic.XmlHelper.Transform(myWeb, styleFile, False)
            oTransform.mbDebug = gbDebug
            oTransform.ProcessTimed(moPageXml, icPageWriter)
            oTransform = Nothing

            cPageHTML = Replace(icPageWriter.ToString, "<?xml version=""1.0"" encoding=""utf-16""?>", "")
            cPageHTML = Replace(cPageHTML, "<?xml version=""1.0"" encoding=""UTF-8""?>", "")

            If bReturnBlankError And Not msException = "" Then
                Return ""
            Else
                Return cPageHTML
            End If

        Catch ex As Exception

            returnException(mcModuleName, "returnPageHtml", ex, "/ewcommon/xsl/admin/setup.xsl", sProcessInfo, gbDebug)
            If bReturnBlankError Then
                Return ""
            Else
                Return msException
            End If
        Finally
            icPageWriter.Dispose()
        End Try

    End Function

    Public Sub PostFlushActions()

        Select Case cPostFlushActions
            Case "ImportV3"
                buildDatabase(False)
            Case "UpgradeDB"
                UpdateDatabase()
            Case "NewDB"
                buildDatabase(True)
            Case "RestoreZip"
                Dim oImp As Eonic.Tools.Security.Impersonate = New Eonic.Tools.Security.Impersonate
                Dim oDB As New Eonic.Tools.Database

                oDB.DatabaseServer = goConfig("DatabaseServer")
                oDB.DatabaseUser = Eonic.Tools.Text.SimpleRegexFind(goConfig("DatabaseAuth"), "user id=([^;]*)", 1, Text.RegularExpressions.RegexOptions.IgnoreCase)
                oDB.DatabasePassword = Eonic.Tools.Text.SimpleRegexFind(goConfig("DatabaseAuth"), "password=([^;]*)", 1, Text.RegularExpressions.RegexOptions.IgnoreCase)
                oDB.FTPUser = goConfig("DatabaseFtpUsername")
                oDB.FtpPassword = goConfig("DatabaseFtpPassword")
                oDB.RestoreDatabase(goConfig("DatabaseName"), goRequest.Form("ewDatabaseFilename"))

        End Select

    End Sub


    Public Sub GetSetupXml()
        Dim oPageElmt As XmlElement
        Dim sProcessInfo As String = ""

        Dim sLayout As String = "default"

        Try

            If moPageXml.DocumentElement Is Nothing Then
                moPageXml.CreateXmlDeclaration("1.0", "UTF-8", "yes")
                oPageElmt = moPageXml.CreateElement("Page")
                moPageXml.AppendChild(oPageElmt)
            Else
                oPageElmt = moPageXml.DocumentElement
            End If

            'introduce the layout

            oPageElmt.SetAttribute("layout", "default")
            oPageElmt.SetAttribute("ewCmd", goRequest("ewCmd"))
            oPageElmt.SetAttribute("ewCmd2", goRequest("ewCmd2"))
            oPageElmt.SetAttribute("cssFramework", "bs3")
            oPageElmt.SetAttribute("adminMode", "true")
            mcEwCmd = goRequest("ewCmd")
            setupProcessXml()
            If mnUserId > 1 Then
                'lets check the current users permission level
                If Not myWeb.moDbHelper.checkUserRole("Administrator") Then
                    mcEwCmd = "LogOff"
                Else
                    oPageElmt.AppendChild(myWeb.moDbHelper.GetUserXML(mnUserId))
                    setupMenuXml()
                End If
            ElseIf mnUserId > 0 Then
                'we are AD Authenticated or admin
                'oPageElmt.AppendChild(moDbHelper.GetUserXML(mnUserId))
                setupMenuXml()
            Else
                mnUserId = 0
                mcEwCmd = ""
            End If

            'add the page content

            '   setupProcessXml()
            oPageElmt.SetAttribute("Step", cStep)

        Catch ex As Exception
            returnException(mcModuleName, "GetSetupXML", ex, "", sProcessInfo, gbDebug)
        End Try

    End Sub

    Private Sub setupProcessXml()

        Dim cProcessInfo As String = ""

        Try
            Dim oRoot As XmlElement
            Dim oElmt As XmlElement
            Dim oPageDetail As XmlElement

            oRoot = moPageXml.CreateElement("Contents")
            moPageXml.DocumentElement.AppendChild(oRoot)
            oElmt = moPageXml.CreateElement("Content")
            oRoot.AppendChild(oElmt)
            oPageDetail = moPageXml.CreateElement("ContentDetail")
            moPageXml.DocumentElement.AppendChild(oPageDetail)
            If mbSchemaExists Then
                mnUserId = CInt(goSession("nUserId"))
            End If
Recheck:
            If goConfig("DatabaseName") = Nothing Then
                'Step 1. Create a Web.Config and various other files.

                Dim oSetXfm As SetupXforms = New SetupXforms(Me)
                oPageDetail.AppendChild(oSetXfm.xFrmWebSettings())
                moPageXml.DocumentElement.SetAttribute("layout", "AdminXForm")
                If oSetXfm.valid And (goSession("nUserId") Is Nothing Or goSession("nUserId") = 0) Then
                    goSession("nUserId") = 1
                    msRedirectOnEnd = "/ewcommon/setup/?ewCmd=NewV4&ewCmd2=Do"
                    ' GoTo Recheck
                End If

            ElseIf mbSchemaExists = False Then

                Dim oSetXfm As SetupXforms = New SetupXforms(Me)
                oPageDetail.AppendChild(oSetXfm.xFrmNewDatabase())
                If oSetXfm.valid Then
                    cStep = 1
                    cPostFlushActions = "NewDB"
                Else
                    cStep = 2
                End If
                mcEwCmd = "NewDatabase"

                'msRedirectOnEnd = "/ewcommon/setup/?ewCmd=NewV4"

            Else
                If mnUserId = 0 Then
                    'Dim oAdXfm As Web.Admin.AdminXforms = New Web.Admin.AdminXforms(myWeb)
                    'oAdXfm.open(moPageXml)

                    Dim oMembershipProv As New Providers.Membership.BaseProvider(myWeb, myWeb.moConfig("MembershipProvider"))
                    Dim oAdXfm As Object = oMembershipProv.AdminXforms
                    oAdXfm.open(moPageXml)
                    oPageDetail.AppendChild(oAdXfm.xFrmUserLogon("AdminLogon"))

                    If oAdXfm.isSubmitted Then
                        oAdXfm.validate()
                    End If
                    mnUserId = myWeb.mnUserId
                    If oAdXfm.valid Then

                        If Not myWeb.moDbHelper.checkUserRole("Administrator") Or mnUserId <> 1 Then
                            moPageXml.DocumentElement.SetAttribute("layout", "AdminXForm")
                            mcEwCmd = "Logon"
                        Else
                            mnUserId = myWeb.mnUserId
                            oRoot.AppendChild(myWeb.moDbHelper.GetUserXML(mnUserId))
                            If mnUserId > 0 Then
                                mcEwCmd = "Home"
                            Else
                                moPageXml.DocumentElement.SetAttribute("layout", "AdminXForm")
                            End If
                        End If
                    Else
                        moPageXml.DocumentElement.SetAttribute("layout", "AdminXForm")
                        mcEwCmd = "Logon"
                    End If
                    'set the userId on the DBHelper
                    If Not myWeb.moDbHelper Is Nothing Then
                        myWeb.moDbHelper.mnUserId = mnUserId
                    End If
                End If
            End If
            If mnUserId = 0 Then mnUserId = CInt(goSession("nUserId"))
            If mcEwCmd = "LogOff" Then mcEwCmd = goRequest("ewCmd")
            If mnUserId > 0 Then
                Select Case mcEwCmd
                    Case "Logoff"
                        mnUserId = 0
                        goSession("nUserId") = 0
                        mcEwCmd = ""
                        GoTo Recheck
                    Case "ClearDB"
                        If goRequest("ewCmd2") = "Do" Then
                            If Migrate_Rollback() Then
                                cStep = 1
                            Else
                                cStep = 2
                            End If
                        End If
                    Case "NewV4"
                        If goRequest("ewCmd2") = "Do" Then
                            If buildDatabase(True) Then
                                cStep = 1
                            Else
                                cStep = 2
                            End If
                        End If
                    Case "ShipLoc"
                        If goRequest("ewCmd2") = "Do" Then
                            If LoadShippingLocations() Then
                                cStep = 1
                            Else
                                cStep = 2
                            End If
                        End If
                    Case "ImportV3"
                        If goRequest("ewCmd2") = "Do" Then
                            cStep = 1
                            cPostFlushActions = "ImportV3"

                            'If buildDatabase(False) Then
                            '    cStep = 1
                            'Else
                            '    cStep = 2
                            'End If

                        End If
                    Case "UpgradeDB"
                        If goRequest("ewCmd2") = "Do" Then
                            cStep = 1
                            cPostFlushActions = "UpgradeDB"

                            'If UpdateDatabase2() Then
                            '    cStep = 1
                            'Else
                            '    cStep = 2
                            'End If
                        End If
                    Case "CleanAudit"
                        If goRequest("ewCmd2") = "Do" Then
                            AddResponse(myWeb.moDbHelper.CleanAuditOrphans())
                            cStep = 1
                        End If
                    Case "UpgradeSchema"
                        If goRequest("ewCmd2") = "Do" Then
                            Dim oUS As New ContentMigration(Me)
                            If oUS.GetPage() Then
                                cStep = 1
                            Else
                                cStep = 2
                            End If
                        End If
                    Case "OptimiseImages"
                        If goRequest("ewCmd2") = "Do" Then
                            Dim oFsh As New fsHelper(myWeb.moCtx)
                            oFsh.mcRoot = myWeb.goServer.MapPath("/")
                            AddResponse(oFsh.OptimiseImages("/Images", 0, 0, False))
                            cStep = 1
                        End If

                    Case "ImportContent"
                        If goRequest("ewCmd2") = "Do" Then
                            Dim oIC As New ContentImport(Me)
                            If oIC.GetPage() Then
                                cStep = 1
                            Else
                                cStep = 2
                            End If
                        End If
                    Case "Backup"
                        If goRequest("ewCmd2") = "Do" Then
                            Dim oSetXfm As SetupXforms = New SetupXforms(Me)
                            oPageDetail.AppendChild(oSetXfm.xFrmBackupDatabase())
                            If oSetXfm.valid Then
                                cStep = 1
                            Else
                                cStep = 2
                            End If
                        End If
                    Case "Restore"
                        If goRequest("ewCmd2") = "Do" Then
                            Dim oSetXfm As SetupXforms = New SetupXforms(Me)
                            oPageDetail.AppendChild(oSetXfm.xFrmRestoreDatabase())
                            If oSetXfm.valid Then
                                cStep = 1
                            Else
                                cStep = 2
                            End If
                        End If
                End Select
            End If
            '  moPageXml.DocumentElement.SetAttribute("layout", mcEwCmd)
            moPageXml.DocumentElement.SetAttribute("ewCmd", mcEwCmd)
            moPageXml.DocumentElement.SetAttribute("User", mnUserId)
            moPageXml.DocumentElement.AppendChild(oRoot)
            oRoot.AppendChild(oResponse)
        Catch ex As Exception
            returnException(mcModuleName, "SetupProcessXML", ex, "", cProcessInfo, gbDebug)
        End Try

    End Sub


    Private Sub setupMenuXml()

        Dim oElmt As XmlElement

        Dim oElmt1 As XmlElement
        Dim oElmt2 As XmlElement
        Dim oElmt3 As XmlElement
        Dim oElmt4 As XmlElement
        Dim oElmt5 As XmlElement
        Dim oElmt6 As XmlElement
        Dim oElmt7 As XmlElement
        Dim oElmt8 As XmlElement
        Dim oElmt9 As XmlElement
        Dim oElmt10 As XmlElement
        Dim oElmt11 As XmlElement
        Dim oElmt12 As XmlElement
        Dim oElmt13 As XmlElement

        Dim sProcessInfo As String = ""
        Try
            oElmt = moPageXml.CreateElement("AdminMenu")

            oElmt1 = appendMenuItem(oElmt, "Setup Home", "AdmHome", , , "fa-gears")
            If mbSchemaExists Then
                oElmt2 = appendMenuItem(oElmt1, "Setup and Import", "Setup", , , "fa-gear")
                oElmt3 = appendMenuItem(oElmt2, "Delete Database", "ClearDB", , , "fa-warning")
                oElmt4 = appendMenuItem(oElmt2, "New Database", "NewV4", , , "fa-briefcase")
                oElmt6 = appendMenuItem(oElmt2, "Add Shipping Locations", "ShipLoc", , , "fa-globe")
                oElmt5 = appendMenuItem(oElmt2, "Import V3 Data", "ImportV3", , , "fa-level-up")

                oElmt6 = appendMenuItem(oElmt1, "Maintenance", "Maintenance", , , "fa-wrench")
                oElmt7 = appendMenuItem(oElmt6, "Update Database Schema", "UpgradeDB", , , "fa-level-up")
                oElmt8 = appendMenuItem(oElmt6, "Clean Audit Table", "CleanAudit", , , "fa-eraser")
                oElmt9 = appendMenuItem(oElmt6, "Import Content", "ImportContent", , , "fa-arrow-circle-right")
                oElmt10 = appendMenuItem(oElmt6, "Upgrade Content", "UpgradeSchema", , , "fa-angle-double-up")
                oElmt10 = appendMenuItem(oElmt6, "Optimise Images", "OptimiseImages", , , "fa-picture-o")

                oElmt11 = appendMenuItem(oElmt1, "Backup/Restore", "BackupRestore", , , "fa-save")
                oElmt12 = appendMenuItem(oElmt11, "Backup", "Backup", , , "fa-save")
                oElmt13 = appendMenuItem(oElmt11, "Restore", "Restore", , , "fa-reply")
            Else
                oElmt2 = appendMenuItem(oElmt1, "Setup", "Setup", , , "fa-gear")
                oElmt4 = appendMenuItem(oElmt2, "New Database", "NewDatabase", , , "fa-briefcase")
            End If

            moPageXml.DocumentElement.AppendChild(oElmt)

        Catch ex As Exception
            AddResponseError(ex)
        End Try

    End Sub

    Private Function appendMenuItem(ByRef oRootElmt As XmlElement, ByVal cName As String, ByVal cCmd As String, Optional ByVal pgid As Long = 0, Optional ByVal display As Boolean = True, Optional ByVal icon As String = "") As XmlElement

        Dim oElmt As XmlElement
        Dim sProcessInfo As String = ""
        Try

            oElmt = moPageXml.CreateElement("MenuItem")
            oElmt.SetAttribute("name", cName)
            oElmt.SetAttribute("cmd", cCmd)
            If pgid <> 0 Then
                oElmt.SetAttribute("pgid", pgid)
            End If
            If display Then
                oElmt.SetAttribute("display", "true")
            Else
                oElmt.SetAttribute("display", "false")
            End If
            If icon <> "" Then
                oElmt.SetAttribute("icon", icon)
            End If
            oRootElmt.AppendChild(oElmt)

            Return oElmt

        Catch ex As Exception
            returnException(mcModuleName, "appendMenuItem", ex, "", sProcessInfo, gbDebug)

            Return Nothing
        End Try
    End Function

    Public Function UpdateDatabase() As Boolean
        Dim bRes As Boolean = True
        Dim AltFolder As String
        Dim filePath As String

        Try

            Dim commonfolders As New ArrayList
            Dim oFS As New fsHelper

            myWeb.InitializeVariables()

            For Each AltFolder In myWeb.maCommonFolders
                filePath = AltFolder.TrimEnd("/\".ToCharArray) & "/sqlupdate/DatabaseUpgrade.xml"
                If Not String.IsNullOrEmpty(AltFolder) Then
                    AddResponse("Running: " & filePath)
                    If oFS.VirtualFileExists(filePath) Then
                        UpdateDatabase(filePath)
                    Else
                        AddResponse("No Common Upgrades Found")
                    End If
                Else
                        AddResponse("Not Running: " & filePath)
                End If
            Next
            If oFS.VirtualFileExists("/sqlupdate/DatabaseUpgrade.xml") Then
                UpdateDatabase("/sqlupdate/DatabaseUpgrade.xml")
            Else
                AddResponse("No Bespoke Upgrades Found")
            End If
            AddResponse("Database Upgrade Complete")

        Catch ex As Exception
            AddResponseError(ex) 'returnException(mcModuleName, "updateDatabase", ex, "", cProcessInfo, gbDebug)
            'AddResponse(ex.Message & " - " & ex.InnerException.ToString)
            AddResponse("Update Failed")
            Return False
        End Try
    End Function


    Public Function UpdateDatabase(ByVal filePath As String) As Boolean
        Dim bRes As Boolean = True
        Try
            Dim cCurrentVersion As String = getVersionNumber()
            AddResponse("Current Version: " & cCurrentVersion)
            Dim oCurrentVersion() As String = Split(cCurrentVersion, ".")
            Dim oUpgrdXML As New XmlDocument

            If IO.File.Exists(goServer.MapPath(filePath)) Then

                oUpgrdXML.Load(goServer.MapPath(filePath))

                Dim cLatestVersion As String = oUpgrdXML.DocumentElement.GetAttribute("LatestVersion")
                If cLatestVersion = cCurrentVersion Then Return True

                'Remove all foreign keys
                myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath("/ewcommon/sqlupdate/toV4/DropAllForeignKeys.sql"))
                msException = ""
                'If Not msException = "" Then
                '    AddResponse(msException)
                '    msException = ""
                'End If

                AddResponse("Running updates to version: " & cLatestVersion)
                AddResponse("Running File: " & filePath)

                'will loop through until we get to version above ours
                Dim oVer As XmlElement
                For Each oVer In oUpgrdXML.DocumentElement.SelectNodes("Version")
                    If oVer.GetAttribute("Number") >= oCurrentVersion(0) Then
                        'in current main version or above
                        Dim Sub1 As XmlElement
                        For Each Sub1 In oVer.SelectNodes("Sub1")
                            If CLng(Sub1.GetAttribute("Number")) >= CLng(oCurrentVersion(1)) Then
                                'in current sub version 1 or above
                                Dim Sub2 As XmlElement
                                For Each Sub2 In Sub1.SelectNodes("Sub2")
                                    If CLng(Sub2.GetAttribute("Number")) >= CLng(oCurrentVersion(2)) Then
                                        Dim Sub3 As XmlElement
                                        Dim bRunAll As Boolean

                                        For Each Sub3 In Sub2.SelectNodes("Sub3")
                                            If CLng(Sub3.GetAttribute("Number")) >= CLng(oCurrentVersion(3)) Or bRunAll Then
                                                'now to get actions
                                                AddResponse("Updating to:" & oVer.GetAttribute("Number") & "." & Sub1.GetAttribute("Number") & "." & Sub2.GetAttribute("Number") & "." & Sub3.GetAttribute("Number"))
                                                Dim oActionElmt As XmlElement
                                                For Each oActionElmt In Sub3.SelectNodes("Action")
                                                    Try
                                                        Select Case oActionElmt.GetAttribute("Type")
                                                            Case "Drop"
                                                                ifSqlObjectExistsDropIt(oActionElmt.GetAttribute("ObjectName"), oActionElmt.GetAttribute("ObjectType"))
                                                                AddResponse("dropped '" & oActionElmt.GetAttribute("ObjectName") & "'")
                                                            Case "File"
                                                                Dim nCount As Long
                                                                AddResponse("Run File '" & oActionElmt.GetAttribute("ObjectName") & "'")
                                                                nCount = myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath(oActionElmt.GetAttribute("ObjectName")))
                                                                If nCount = -1 Then
                                                                    AddResponse("File execution Completed...")
                                                                Else
                                                                    AddResponse("(" & nCount & ") Updates..")
                                                                End If

                                                            Case Else
                                                                'dont do anything
                                                        End Select
                                                    Catch ex As Exception
                                                        bRes = False
                                                        AddResponse("Error:" & ex.ToString)
                                                        AddResponseError(ex) 'returnException(mcModuleName, "updateDatabase", ex, "", cProcessInfo, gbDebug)
                                                    End Try
                                                    'If Not msException = "" Then
                                                    '    AddResponse(msException)
                                                    '    msException = ""
                                                    'End If
                                                Next
                                                oCurrentVersion(3) = Sub3.GetAttribute("Number")
                                            Else
                                                ' AddResponse("Error: Not yet at this version number")
                                            End If
                                        Next
                                        If CLng(Sub2.GetAttribute("Number")) = CLng(oCurrentVersion(2)) Then
                                            oCurrentVersion(3) = 0
                                            oCurrentVersion(2) = oCurrentVersion(2) + 1
                                        End If
                                    End If
                                Next
                                If CLng(Sub1.GetAttribute("Number")) = CLng(oCurrentVersion(1)) Then
                                    oCurrentVersion(2) = 0
                                    oCurrentVersion(1) = oCurrentVersion(1) + 1
                                End If
                            End If
                        Next
                        oCurrentVersion(3) = -1
                        oCurrentVersion(2) = 0
                        oCurrentVersion(1) = 0
                    End If
                Next
                If oUpgrdXML.DocumentElement.GetAttribute("update") <> "false" Then
                    saveVersionNumber(cLatestVersion)
                    mnCurrentVersion = cLatestVersion
                    cLatestVersion = cLatestVersion & " - Updated DB version"
                Else
                    cLatestVersion = cLatestVersion & " - Not Updated DB version"
                End If

                AddResponse("Update Completed to Version " & cLatestVersion)

                Return bRes
            Else
                AddResponse("File Not Found: " & filePath)
                Return False
            End If

        Catch ex As Exception
            AddResponseError(ex) 'returnException(mcModuleName, "updateDatabase", ex, "", cProcessInfo, gbDebug)
            AddResponse(ex.Message & " - " & ex.InnerException.ToString)
            AddResponse("Update Failed")
            Return False
        End Try
    End Function

    Public Function buildDatabase(Optional ByVal NewBuild As Boolean = False) As Boolean
        Dim cProcessInfo As String = "migrateData"

        Dim bResult As Boolean = False
        Try

            'clear any existing tables
            Migrate_Rollback()

            goResponse.Buffer = False
            goResponse.Flush()
            'Run The Script
            '############################
            myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath("/ewcommon/sqlupdate/toV4/Structure.sql"))
            AddResponse("Run File '/ewcommon/sqlupdate/toV4/Structure.SQL'")
            myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath("/ewcommon/sqlupdate/toV4/fxn_SearchXML.sql"))
            AddResponse("Run File '/ewcommon/sqlupdate/toV4/fxn_SearchXML.SQL'")
            myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath("/ewcommon/sqlupdate/toV4/fxn_addAudit.sql"))
            AddResponse("Run File '/ewcommon/sqlupdate/toV4/fxn_addAudit.SQL'")
            myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath("/ewcommon/sqlupdate/toV4/fxn_getStatus.sql"))
            AddResponse("Run File '/ewcommon/sqlupdate/toV4/fxn_getStatus.SQL'")
            myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath("/ewcommon/sqlupdate/toV4/fxn_checkPermission.sql"))
            AddResponse("Run File '/ewcommon/sqlupdate/toV4/fxn_checkPermission.SQL'")
            myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath("/ewcommon/sqlupdate/toV4/fxn_getUserCompanies.sql"))
            AddResponse("Run File '/ewcommon/sqlupdate/toV4/fxn_getUserCompanies.SQL'")
            myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath("/ewcommon/sqlupdate/toV4/fxn_shippingTotal.sql"))
            AddResponse("Run File '/ewcommon/sqlupdate/toV4/fxn_shippingTotal.SQL'")
            myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath("/ewcommon/sqlupdate/toV4/fxn_getUserDepts.sql"))
            AddResponse("Run File '/ewcommon/sqlupdate/toV4/fxn_getUserDepts.SQL'")
            myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath("/ewcommon/sqlupdate/toV4/fxn_getUserRoles.sql"))
            AddResponse("Run File '/ewcommon/sqlupdate/toV4/fxn_getUserRoles.SQL'")
            myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath("/ewcommon/sqlupdate/toV4/spGetAllUsers.sql"))
            AddResponse("Run File '/ewcommon/sqlupdate/toV4/spGetAllUsers.SQL'")
            myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath("/ewcommon/sqlupdate/toV4/spGetUsers.sql"))
            AddResponse("Run File '/ewcommon/sqlupdate/toV4/spGetUsers.SQL'")
            myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath("/ewcommon/sqlupdate/toV4/getContentStructure.sql"))
            AddResponse("Run File '/ewcommon/sqlupdate/toV4/getContentStructure.SQL'")
            myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath("/ewcommon/sqlupdate/toV4/spGetDirectoryItems.sql"))
            AddResponse("Run File '/ewcommon/sqlupdate/toV4/spGetDirectoryItems.SQL'")
            myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath("/ewcommon/sqlupdate/toV4/getUsersCompanyAllParents.sql"))
            AddResponse("Run File '/ewcommon/sqlupdate/toV4/getUsersCompanyAllParents.SQL'")
            myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath("/ewcommon/sqlupdate/toV4/spGetCompanyUsers.sql"))
            AddResponse("Run File '/ewcommon/sqlupdate/toV4/spGetCompanyUsers.SQL'")
            myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath("/ewcommon/sqlupdate/toV4/spGetAllUsersActive.sql"))
            AddResponse("Run File '/ewcommon/sqlupdate/toV4/spGetAllUsersActive.SQL'")
            myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath("/ewcommon/sqlupdate/toV4/spGetAllUsersInActive.sql"))
            AddResponse("Run File '/ewcommon/sqlupdate/toV4/spGetAllUsersInActive.SQL'")
            myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath("/ewcommon/sqlupdate/toV4/spGetCompanyUsersActive.sql"))
            AddResponse("Run File '/ewcommon/sqlupdate/toV4/spGetCompanyUsersActive.SQL'")
            myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath("/ewcommon/sqlupdate/toV4/spGetCompanyUsersInActive.sql"))
            AddResponse("Run File '/ewcommon/sqlupdate/toV4/spGetCompanyUsersInActive.SQL'")
            myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath("/ewcommon/sqlupdate/toV4/spSearchUsers.sql"))
            AddResponse("Run File '/ewcommon/sqlupdate/toV4/spSearchUsers.SQL'")
            myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath("/ewcommon/sqlupdate/toV4/4.0.1.40/tblOptOutAddresses.sql"))
            AddResponse("Run File '/ewcommon/sqlupdate/toV4/4.0.1.40/tblOptOutAddresses.sql'")
            myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath("/ewcommon/sqlupdate/toV4/4.0.1.45/fxn_getContentParents.sql"))
            AddResponse("Run File '/ewcommon/sqlupdate/toV4/4.0.1.45/fxn_getContentParents.sql'")

            AddResponse("Completed Initial Build")
            saveVersionNumber()

            'Make some strings to carry the Migration specific errors
            If NewBuild Then
                'new build - just need the admin jobbie setup
                bResult = True
                bResult = prepareDirectory()
            Else
                'migration from v2.6/v3.0
                'these are the default items to migrate
                bResult = Migrate_Directory()
                If bResult Then bResult = Migrate_Content()
                If bResult Then bResult = Migrate_Shipping()

                ' If bResult Then bResult = Migrate_Cart()

                'will need to check the rest like cart and membership
                'they would need to be checked if they are active
            End If

            UpdateDatabase()

            AddResponseComplete("Congratulations - Go to your new Website", "/")

            Return saveVersionNumber()

        Catch ex As Exception
            AddResponseError(ex) 'returnException(mcModuleName, "InitializeVariables", ex, "", cProcessInfo, gbDebug)
            Return False
        End Try

    End Function

    Public Function prepareDirectory() As Boolean
        Dim nRoleId As Long
        Dim cProcessInfo As String = "prepareDirectory"
        Try

            Dim AdminPassword As String = goConfig("DatabasePassword")
            If AdminPassword = "" Then AdminPassword = "buster"
            Dim UserEmail As String = "support@eonic.co.uk"
            myWeb.Open()

            mnUserId = myWeb.moDbHelper.insertDirectory("AdminV4", "User", "Admin", AdminPassword, "<User><FirstName>Website</FirstName><MiddleName/><LastName>Administrator</LastName><Position/><Email>" & UserEmail & "</Email><Notes/></User>")

            'create system roles

            nRoleId = myWeb.moDbHelper.insertDirectory("Administrator", "Role", "Administrator", "", "<Role><Name>Administrator</Name><Notes/></Role>")
            myWeb.moDbHelper.maintainDirectoryRelation(nRoleId, mnUserId)
            nRoleId = myWeb.moDbHelper.insertDirectory("DefaultUser", "Role", "Default User", "", "<Role><Name>Administrator</Name><Notes/></Role>")
            myWeb.moDbHelper.maintainDirectoryRelation(nRoleId, mnUserId)

            Dim defaultPageXml As String = "<DisplayName title="""" linkType=""internal"" exclude=""false"" noindex=""false""/><Images><img class=""icon"" /><img class=""thumbnail"" /><img class=""detail"" /></Images><Description/>"

            gnTopLevel = myWeb.moDbHelper.insertStructure("0", "", "Home", defaultPageXml, "Modules_1_column")
            myWeb.moDbHelper.insertStructure(gnTopLevel, "", "About Us", defaultPageXml, "Modules_1_column")
            myWeb.moDbHelper.insertStructure(gnTopLevel, "", "Products", defaultPageXml, "Modules_1_column")
            myWeb.moDbHelper.insertStructure(gnTopLevel, "", "Services", defaultPageXml, "Modules_1_column")

            Dim infoId As Long = myWeb.moDbHelper.insertStructure(gnTopLevel, "", "Information", defaultPageXml, "Modules_1_column")
            myWeb.moDbHelper.insertStructure(infoId, "", "Contact Us", defaultPageXml, "Modules_1_column")

            Return True
        Catch ex As Exception
            AddResponseError(ex) 'returnException(mcModuleName, "InitializeVariables", ex, "", cProcessInfo, gbDebug)
            Return False
        End Try

    End Function

    Function saveVersionNumber(Optional ByVal cVersionNumber As String = "") As Boolean
        Try
            If cVersionNumber = "" Then cVersionNumber = mnCurrentVersion

            'create the version table if not exists
            Dim sFilePath As String = "/ewcommon/sqlupdate/toV4/4.1.1.35/tblSchemaVersion.sql"
            If Not myWeb.moDbHelper.checkDBObjectExists("tblSchemaVersion", Tools.Database.objectTypes.Table) Then
                myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath(sFilePath))
            End If

            Dim aVersionNumber() As String = Split(cVersionNumber, ".")
            Dim sSql As String
            Dim oDs As DataSet
            Dim oDr As DataRow
            sSql = "select * from tblSchemaVersion where nVersionKey = 1"

            oDs = myWeb.moDbHelper.getDataSetForUpdate(sSql, "VersionNo")

            If oDs.Tables("VersionNo").Rows.Count > 0 Then
                oDr = oDs.Tables("VersionNo").Rows(0)
                oDr.BeginEdit()
                oDr("MajorVersion") = CInt(aVersionNumber(0))
                oDr("MinorVersion") = CInt(aVersionNumber(1))
                oDr("Release") = CInt(aVersionNumber(2))
                oDr("Build") = CInt(aVersionNumber(3))
                oDr.EndEdit()
            Else
                oDr = oDs.Tables("VersionNo").NewRow
                oDr("MajorVersion") = CInt(aVersionNumber(0))
                oDr("MinorVersion") = CInt(aVersionNumber(1))
                oDr("Release") = CInt(aVersionNumber(2))
                oDr("Build") = CInt(aVersionNumber(3))
                oDs.Tables("VersionNo").Rows.Add(oDr)
            End If

            myWeb.moDbHelper.updateDataset(oDs, "VersionNo", False)

            ' oDs.Dispose()
            oDs = Nothing

            'Dim oImp As Eonic.Tools.Security.Impersonate = New Eonic.Tools.Security.Impersonate()
            'If oImp.ImpersonateValidUser(goConfig("AdminAcct"), goConfig("AdminDomain"), goConfig("AdminPassword"), , goConfig("AdminGroup")) Then
            '    IO.File.SetAttributes(goServer.MapPath(goConfig("ProjectPath") & "/Web.config"), IO.FileAttributes.Normal)
            '    Dim oCfg As Configuration = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/")
            '    Dim oCgfSect As System.Configuration.DefaultSection = oCfg.GetSection("eonic/web")
            '    Dim oConfigXml As XmlDocument = New XmlDocument
            '    oConfigXml.LoadXml(oCgfSect.SectionInformation.GetRawXml)

            '    oConfigXml.SelectSingleNode("web/add[@key='VersionNumber']/@value").InnerText = cVersionNumber

            '    oCgfSect.SectionInformation.RestartOnExternalChanges = False
            '    oCgfSect.SectionInformation.SetRawXml(oConfigXml.OuterXml)
            '    oCfg.Save()
            '    AddResponse("You have been updated to Version:" & cVersionNumber & "")
            '    oImp.UndoImpersonation()
            '    Return True
            'Else
            '    AddResponse("Failed to update version number - Auth Failed")
            '    Return False
            'End If

            Return True

        Catch ex As Exception
            AddResponseError(ex) 'returnException(mcModuleName, "updateDatabase", ex, "", cProcessInfo, gbDebug)
            AddResponse("Failed to update version number - Error Condition")
            Return False
        End Try
    End Function

    Function getVersionNumber() As String
        Try
            Dim sVersionNumber As String = goConfig("VersionNumber")
            Dim sSql As String
            Dim oDs As DataSet
            Dim oDr As DataRow

            If myWeb.moDbHelper.checkDBObjectExists("tblSchemaVersion", Tools.Database.objectTypes.Table) Then
                sSql = "select * from tblSchemaVersion where nVersionKey = 1"
                oDs = myWeb.moDbHelper.GetDataSet(sSql, "VersionNo")
                For Each oDr In oDs.Tables("VersionNo").Rows
                    sVersionNumber = oDr("MajorVersion") & "." & oDr("MinorVersion") & "." & oDr("Release") & "." & oDr("Build") & "."
                Next
                oDs.Dispose()
                oDs = Nothing
            End If

            Return sVersionNumber

        Catch ex As Exception
            AddResponseError(ex) 'returnException(mcModuleName, "updateDatabase", ex, "", cProcessInfo, gbDebug)
            AddResponse("Failed to update version number - Error Condition")
            Return False
        End Try
    End Function


#End Region

#Region "V2.6/V3 to V4 Migration -BJR November 2006-"

    Public Function Migrate_Directory() As Boolean

        Dim sSqlStr As String
        Dim oDSDIR As DataSet
        Dim strSchema As String
        Dim strforiegnRef As String
        Dim strName As String
        Dim strPassword As String
        Dim strXML As String
        Dim oDT As DataTable
        Dim oDC As DataColumn

        Dim oDXML As New XmlDocument
        Dim oDirElmt As XmlElement
        Dim oConElmt As XmlElement
        Dim nDirId As Long

        Dim bUsesDirectory As Boolean
        Dim cSQLP1 As String
        Dim cSQLP2 As String

        Try

            bUsesDirectory = moDbHelper.doesTableExist("tbl_ewu_Directory")
            If Not bUsesDirectory Then
                AddResponse(" No Directory information to import. Creating deafults.")
                Dim nRolex As Integer
                Dim nUserx As Integer
                nUserx = myWeb.moDbHelper.insertDirectory("AdminV4", "User", "Admin", "buster", "<User><FirstName/><MiddleName/><LastName/><Position/><Email/><Notes/></User>")
                'create system roles
                nRolex = myWeb.moDbHelper.insertDirectory("Administrator", "Role", "Administrator", "", "<Role><Name>Administrator</Name><Notes/></Role>")
                myWeb.moDbHelper.maintainDirectoryRelation(nRolex, nUserx)
                nRolex = myWeb.moDbHelper.insertDirectory("DefaultUser", "Role", "Default User", "", "<Role><Name>Administrator</Name><Notes/></Role>")
                myWeb.moDbHelper.maintainDirectoryRelation(nRolex, nUserx)
                Return True
                Exit Function
            End If


            AddResponse("Creating System Roles")
            Dim nRoleId As Long
            'create system roles
            mnUserId = myWeb.moDbHelper.insertDirectory("AdminV4", "User", "Admin", "buster", "<User><FirstName/><MiddleName/><LastName/><Position/><Email/><Notes/></User>")
            nRoleId = myWeb.moDbHelper.insertDirectory("Administrator", "Role", "Administrator", "", "<Role><Name>Administrator</Name><Notes/></Role>")
            myWeb.moDbHelper.maintainDirectoryRelation(nRoleId, mnUserId)
            nRoleId = myWeb.moDbHelper.insertDirectory("DefaultUser", "Role", "Default User", "", "<Role><Name>Administrator</Name><Notes/></Role>")
            myWeb.moDbHelper.maintainDirectoryRelation(nRoleId, mnUserId)


            'Directory Table
            AddResponse("Migrating Directory Table")
            sSqlStr = "Select * From tbl_ewu_Directory"
            oDSDIR = myWeb.moDbHelper.GetDataSet(sSqlStr, "Directory", "dsDIR")

            myWeb.moDbHelper.addTableToDataSet(oDSDIR, "SELECT * FROM tbl_ewc_Contact where nContactParentType = 2", "Contacts")
            oDSDIR.Relations.Add("CartContacts", oDSDIR.Tables("Directory").Columns("nDirId"), oDSDIR.Tables("Contacts").Columns("nContactParentId"), False)
            oDSDIR.Relations("CartContacts").Nested = True

            For Each oDT In oDSDIR.Tables
                For Each oDC In oDT.Columns
                    oDC.ColumnMapping = MappingType.Attribute
                Next
            Next

            oDXML.InnerXml = Replace(oDSDIR.GetXml, "'", "''")

            'ok, now we need to loop through the directory, 
            'then add the contacts for the users
            'having lots of checking so just in case there is a column missing

            For Each oDirElmt In oDXML.SelectNodes("descendant-or-self::Directory")

                'do the user record
                If oDirElmt.GetAttribute("nDirType") = CStr(1) Then 'Group Schema
                    strSchema = "Group"
                    strXML = "<Group><Name>" & oDirElmt.GetAttribute("cDirDN") & "</Name></Group>"
                Else
                    strSchema = "User" 'User schema
                    strXML = "<User><FirstName>" & oDirElmt.GetAttribute("cFirstName") & "</FirstName><LastName>" & _
                    oDirElmt.GetAttribute("cLastName") & "</LastName><Position/><Email>" & oDirElmt.GetAttribute("cDirEmail") & _
                    "</Email><Notes/></User>"
                End If
                'other details
                strforiegnRef = IIf(IsDBNull(oDirElmt.GetAttribute("nDirId")), "", oDirElmt.GetAttribute("nDirId"))
                strName = IIf(IsDBNull(oDirElmt.GetAttribute("cDirDN")), "", oDirElmt.GetAttribute("cDirDN"))
                strPassword = IIf(IsDBNull(oDirElmt.GetAttribute("cDirPassword")), "", oDirElmt.GetAttribute("cDirPassword"))
                nDirId = myWeb.moDbHelper.insertDirectory(strforiegnRef, strSchema, strName, strPassword, strXML)

                For Each oConElmt In oDirElmt.SelectNodes("Contacts")
                    AddResponse("Migrating Contact " & oConElmt.GetAttribute("nContactKey") & "")
                    'Firstly the Cart Order
                    cSQLP2 = ""
                    cSQLP1 = "INSERT INTO tblCartContact (nContactDirId, nContactCartId, cContactType, cContactName, " & _
                            "cContactCompany, cContactAddress, cContactCity, cContactState, cContactZip, cContactCountry, " & _
                            "cContactTel, cContactFax, cContactEmail, cContactXml, nAuditId) VALUES ("

                    cSQLP2 &= nDirId & ","
                    cSQLP2 &= "0,"
                    If oConElmt.GetAttribute("cContactType") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= "'" & oConElmt.GetAttribute("cContactType") & "',"
                    If oConElmt.GetAttribute("cContactName") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= "'" & Replace(oConElmt.GetAttribute("cContactName"), "'", "''") & "',"
                    If oConElmt.GetAttribute("cContactCompany") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= "'" & Replace(oConElmt.GetAttribute("cContactCompany"), "'", "''") & "',"
                    If oConElmt.GetAttribute("cContactAddress") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= "'" & Replace(oConElmt.GetAttribute("cContactAddress"), "'", "''") & "',"
                    If oConElmt.GetAttribute("cContactCity") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= "'" & oConElmt.GetAttribute("cContactCity") & "',"
                    If oConElmt.GetAttribute("cContactState") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= "'" & oConElmt.GetAttribute("cContactState") & "',"
                    If oConElmt.GetAttribute("cContactZip") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= "'" & oConElmt.GetAttribute("cContactZip") & "',"
                    If oConElmt.GetAttribute("cContactCountry") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= "'" & oConElmt.GetAttribute("cContactCountry") & "',"
                    If oConElmt.GetAttribute("cContactTel") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= "'" & oConElmt.GetAttribute("cContactTel") & "',"
                    If oConElmt.GetAttribute("cContactFax") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= "'" & oConElmt.GetAttribute("cContactFax") & "',"
                    If oConElmt.GetAttribute("cContactEmail") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= "'" & oConElmt.GetAttribute("cContactEmail") & "',"
                    cSQLP2 &= "Null,"
                    cSQLP2 &= myWeb.moDbHelper.getAuditId() & ")"
                    myWeb.moDbHelper.GetIdInsertSql(cSQLP1 & cSQLP2)
                Next

                'now we need to loop back through and and sort the relations
                If IsDBNull(oDirElmt.GetAttribute("cDirMemberOfIdArr")) Then oDirElmt.SetAttribute("cDirMemberOfIdArr", "")
                If Not oDirElmt.GetAttribute("cDirMemberOfIdArr") = "" Then
                    Dim myArr() As String = Split(oDirElmt.GetAttribute("cDirMemberOfIdArr"), ",")
                    Dim i As Integer
                    For i = 0 To UBound(myArr)
                        Dim myID As Integer = myWeb.moDbHelper.FindDirectoryByForiegn(CInt(Trim(myArr(i))))
                        If myID > 0 Then myWeb.moDbHelper.maintainDirectoryRelation(myID, nDirId)
                    Next
                End If

            Next


            'Done, let the world know!


            Return True
        Catch ex As Exception
            AddResponseError(ex) 'returnException(mcModuleName, "Migrate_Directory", ex, "", "", gbDebug)


            Return False
        End Try
    End Function


    Function Migrate_Content() As Boolean

        Dim oDS As New DataSet
        Dim sContentBrief As String
        Dim sContentDetail As String
        Dim oDataXML As XmlDocument
        Dim oMenuElmt As XmlElement
        Dim oContElmt As XmlElement
        Dim oPermElmt As XmlElement
        Dim oLocElmt As XmlElement
        Dim bUsesDirectory As Boolean
        Dim nParID As Integer = 0
        Dim nContentId As Long
        Dim oDT As DataTable
        Dim oDC As DataColumn
        Dim sSql As String

        Try

            bUsesDirectory = moDbHelper.doesTableExist("tbl_ewu_Directory")

            oDS = myWeb.moDbHelper.GetDataSet("Select * from tbl_ewm_structure", "Menu", "Structure")
            'checknest(oDS, dbT)
            myWeb.moDbHelper.addTableToDataSet(oDS, "SELECT tbl_ewm_content.*, tbl_ewm_contentType.cContentTypeName AS cContentTypeName FROM tbl_ewm_content INNER JOIN tbl_ewm_contentType ON tbl_ewm_content.nContentType = tbl_ewm_contentType.nContentTypeKey", "Content")


            oDS.Relations.Add("Rel0", oDS.Tables("Menu").Columns("nID"), oDS.Tables("Menu").Columns("nParentID"), False)



            oDS.Relations("Rel0").Nested = True
            oDS.Relations.Add("Rel1", oDS.Tables("Menu").Columns("nID"), oDS.Tables("Content").Columns("nContentParID"), False)
            oDS.Relations("Rel1").Nested = True

            myWeb.moDbHelper.addTableToDataSet(oDS, "Select * from tbl_ewm_contentLocation", "Location")


            If bUsesDirectory Then
                myWeb.moDbHelper.addTableToDataSet(oDS, "Select * from tbl_ewu_permissions", "Permissions")
                oDS.Relations.Add("Rel2", oDS.Tables("Menu").Columns("nID"), oDS.Tables("Permissions").Columns("nPermKey"), False)
                myWeb.moDbHelper.addTableToDataSet(oDS, "Select * From tblDirectory", "Directory")
            End If

            oDS.EnforceConstraints = False

            For Each oDT In oDS.Tables
                For Each oDC In oDT.Columns
                    oDC.ColumnMapping = Data.MappingType.Attribute
                Next
            Next

            oDataXML = New XmlDocument
            oDataXML.InnerXml = oDS.GetXml
            For Each oMenuElmt In oDataXML.SelectNodes("descendant-or-self::Menu")
                If Not oMenuElmt.ParentNode.SelectSingleNode("@NewID") Is Nothing Then
                    nParID = oMenuElmt.ParentNode.SelectSingleNode("@NewID").InnerText
                End If
                Dim nMenStatus As Integer
                Select Case oMenuElmt.GetAttribute("nStatus")
                    Case ""
                        nMenStatus = 1
                    Case 1
                        nMenStatus = 1
                    Case 2
                        nMenStatus = 0
                    Case 3
                        nMenStatus = 0
                End Select
                nParID = myWeb.moDbHelper.insertStructure(
                            nParID,
                            oMenuElmt.GetAttribute("nId"),
                             CleanName(oMenuElmt.GetAttribute("cName")),
                             "<DisplayName>" & oMenuElmt.GetAttribute("cName") & "</DisplayName><Description />",
                            oMenuElmt.GetAttribute("cTemplateName"),
                            nMenStatus,
                            IIf(oMenuElmt.GetAttribute("dPublishDate") = "", Nothing, oMenuElmt.GetAttribute("dPublishDate")),
                             IIf(oMenuElmt.GetAttribute("dExpireDate") = "", Nothing, oMenuElmt.GetAttribute("dExpireDate")),
                            "",
                            IIf(oMenuElmt.GetAttribute("nDisplayOrder") = "", 0, oMenuElmt.GetAttribute("nDisplayOrder")))
                oMenuElmt.SetAttribute("NewID", nParID)
                AddResponse("   Writing Page:" & oMenuElmt.GetAttribute("cName") & "   ")

                For Each oContElmt In oMenuElmt.SelectNodes("Content")
                    Dim nContStatus As Integer
                    Select Case oContElmt.GetAttribute("nContentStatus")
                        Case ""
                            nContStatus = 1
                        Case 1
                            nContStatus = 1
                        Case 2
                            nContStatus = 0
                        Case 3
                            nContStatus = 0
                    End Select
                    sContentBrief = upgradeContentSchemas( _
                    Replace(oContElmt.GetAttribute("cContentTypeName"), " ", ""), _
                    oContElmt.GetAttribute("cContentXML"), "brief")

                    sContentDetail = upgradeContentSchemas( _
                    Replace(oContElmt.GetAttribute("cContentTypeName"), " ", ""), _
                    oContElmt.GetAttribute("cContentXML"), "detail")

                    nContentId = myWeb.moDbHelper.insertContent(
                    oContElmt.GetAttribute("nContentKey"),
                     CleanName(oContElmt.GetAttribute("cContentPlaceName"), True),
                     Replace(oContElmt.GetAttribute("cContentTypeName"), " ", ""),
                    sContentBrief,
                    sContentDetail,
                    nParID,
                    IIf(oContElmt.GetAttribute("dPublishDate") = "", Nothing, oContElmt.GetAttribute("dPublishDate")),
                    IIf(oContElmt.GetAttribute("dExpireDate") = "", Nothing, oContElmt.GetAttribute("dExpireDate")),
                    nContStatus)

                    Dim bCascade As Boolean = False
                    If oContElmt.GetAttribute("nContentItterateDown") = "1" Then
                        bCascade = True
                    End If

                    myWeb.moDbHelper.setContentLocation(nParID, nContentId, True, bCascade)

                    oContElmt.SetAttribute("NewID", nContentId)
                Next
                If bUsesDirectory Then ' Permissions
                    For Each oPermElmt In oMenuElmt.SelectNodes("Permissions")
                        Dim nUser As Long = 0
                        nUser = myWeb.moDbHelper.getObjectByRef(Web.dbHelper.objectTypes.Directory, oPermElmt.GetAttribute("nPermDirId"))
                        myWeb.moDbHelper.maintainPermission(nParID, nUser, oPermElmt.GetAttribute("nPermLevel"))
                    Next
                End If
            Next
            AddResponse("Rename Subpage Images:")

            'step through all content images
            For Each oContElmt In oDataXML.SelectNodes("descendant::Content[@cContentTypeName='Image']")
                'if the image has a menu sibling of the same name.
                If Not oContElmt.ParentNode.SelectSingleNode("Menu[@cName='" & Replace(oContElmt.GetAttribute("cContentPlaceName"), "'", "") & "']") Is Nothing Then
                    oMenuElmt = oContElmt.ParentNode.SelectSingleNode("Menu[@cName='" & Replace(oContElmt.GetAttribute("cContentPlaceName"), "'", "") & "']")
                    sSql = "update tblContent set cContentName = 'page_" & oMenuElmt.GetAttribute("NewID") & "_tn' where nContentKey=" & oContElmt.GetAttribute("NewID")
                    AddResponse("<p>Renaming Image: '" & oContElmt.GetAttribute("cContentPlaceName") & "' - 'page_" & oMenuElmt.GetAttribute("NewID") & "_tn'</p>")
                    myWeb.moDbHelper.ExeProcessSql(sSql)
                End If
            Next


            AddResponse("Migrating Locations:")
            For Each oLocElmt In oDataXML.SelectNodes("descendant-or-self::Location")
                'nParID, nContentId
                nParID = myWeb.moDbHelper.getObjectByRef(Web.dbHelper.objectTypes.ContentStructure, oLocElmt.GetAttribute("nStructureID"))
                nContentId = myWeb.moDbHelper.getObjectByRef(Web.dbHelper.objectTypes.Content, oLocElmt.GetAttribute("nContentID"))
                myWeb.moDbHelper.setContentLocation(nParID, nContentId, False)
            Next



            Return True
        Catch ex As Exception
            AddResponseError(ex) 'returnException(mcModuleName, "Migrate_Content", ex, "", "", gbDebug)


            Return False
        End Try
    End Function

    Public Function Migrate_Shipping() As Boolean
        Dim dbh As Web.dbHelper = myWeb.moDbHelper
        Dim oDS As DataSet
        Dim oDXML As New XmlDocument
        Dim oLocElmt As XmlElement
        Dim oMetElmt As XmlElement
        Dim oRelElmt As XmlElement
        Dim oDT As DataTable
        Dim oDC As DataColumn
        Dim nParID As Integer
        Dim cSQL As String
        Try

            AddResponse("Migrating Shipping")
            'ifSqlObjectExistsDropIt(tbl_ewc_shippingLocations, "table")
            If Not myWeb.moDbHelper.TableExists("tbl_ewc_shippingLocations") Then Return True
            oDS = myWeb.moDbHelper.GetDataSet("SELECT * FROM tbl_ewc_shippingLocations", "Locations", "Shipping")
            myWeb.moDbHelper.addTableToDataSet(oDS, "SELECT * FROM tbl_ewc_shippingOptions", "Options")
            myWeb.moDbHelper.addTableToDataSet(oDS, "SELECT * FROM tbl_ewc_shippingRelations", "Relations")

            'Add new column for IDs for local stuff and make all attributes
            If oDS Is Nothing Then Return True
            For Each oDT In oDS.Tables
                oDT.Columns.Add("NewID", GetType(System.Int64))
                For Each oDC In oDT.Columns
                    oDC.ColumnMapping = Data.MappingType.Attribute
                Next
            Next

            oDS.Relations.Add("Rel0", oDS.Tables("Locations").Columns("nLocationKey"), oDS.Tables("Locations").Columns("nLocationParID"), False)
            oDS.Relations("Rel0").Nested = True

            oDXML.InnerXml = oDS.GetXml

            'going to do the locations first since these can potentially have n-tier relations (most complex)

            For Each oLocElmt In oDXML.SelectNodes("descendant-or-self::Locations")
                '    If Not oMenuElmt.ParentNode.SelectSingleNode("@NewID") Is Nothing Then
                '        nParID = oMenuElmt.ParentNode.SelectSingleNode("@NewID").InnerText
                '    End If
                AddResponse("Migrating Location " & oLocElmt.GetAttribute("cLocationNameFull") & "")
                'check parent
                If Not oLocElmt.ParentNode.SelectSingleNode("@NewID") Is Nothing Then
                    nParID = oLocElmt.ParentNode.SelectSingleNode("@NewID").InnerText
                End If
                cSQL = "INSERT INTO tblCartShippingLocations (nLocationType, nLocationParId, cLocationForeignRef, cLocationNameFull, " & _
                "cLocationNameShort, cLocationISOnum, cLocationISOa2, cLocationISOa3, cLocationCode, nLocationTaxRate, " & _
                "nAuditId) VALUES ("
                If oLocElmt.GetAttribute("nLocationType") = "" Then cSQL &= "Null," Else cSQL &= oLocElmt.GetAttribute("nLocationType") & ","
                If oLocElmt.GetAttribute("nLocationParId") = "" Then cSQL &= "Null," Else cSQL &= oLocElmt.GetAttribute("nLocationParId") & ","
                If oLocElmt.GetAttribute("nLocationKey") = "" Then cSQL &= "Null," Else cSQL &= oLocElmt.GetAttribute("nLocationKey") & ","
                If oLocElmt.GetAttribute("cLocationNameFull") = "" Then cSQL &= "Null," Else cSQL &= "'" & Replace(oLocElmt.GetAttribute("cLocationNameFull"), "'", "''") & "',"
                If oLocElmt.GetAttribute("cLocationNameShort") = "" Then cSQL &= "Null," Else cSQL &= "'" & Replace(oLocElmt.GetAttribute("cLocationNameShort"), "'", "''") & "',"
                If oLocElmt.GetAttribute("cLocationISOnum") = "" Then cSQL &= "Null," Else cSQL &= "'" & oLocElmt.GetAttribute("cLocationISOnum") & "',"
                If oLocElmt.GetAttribute("cLocationISOa2") = "" Then cSQL &= "Null," Else cSQL &= "'" & oLocElmt.GetAttribute("cLocationISOa2") & "',"
                If oLocElmt.GetAttribute("cLocationISOa3") = "" Then cSQL &= "Null," Else cSQL &= "'" & oLocElmt.GetAttribute("cLocationISOa3") & "',"
                If oLocElmt.GetAttribute("cLocationCode") = "" Then cSQL &= "Null," Else cSQL &= "'" & oLocElmt.GetAttribute("cLocationCode") & "',"
                If oLocElmt.GetAttribute("nLocationTaxRate") = "" Then cSQL &= "Null," Else cSQL &= "'" & oLocElmt.GetAttribute("nLocationTaxRate") & "',"
                cSQL &= myWeb.moDbHelper.getAuditId() & ")"
                nParID = myWeb.moDbHelper.GetIdInsertSql(cSQL)
                oLocElmt.SetAttribute("NewID", nParID)
            Next

            'now the methods...

            For Each oMetElmt In oDXML.SelectNodes("descendant-or-self::Options")
                AddResponse("Migrating Option " & oMetElmt.GetAttribute("cShipOptName") & "")
                cSQL = "INSERT INTO tblCartShippingMethods (nShipOptCat, cShipOptForeignRef, cShipOptName, cShipOptCarrier, cShipOptTime, " & _
                "cShipOptTandC, nShipOptCost, nShipOptPercentage, nShipOptQuantMin, nShipOptQuantMax, nShipOptWeightMin, " & _
                "nShipOptWeightMax, nShipOptPriceMin, nShipOptPriceMax, nShipOptHandlingPercentage, nShipOptHandlingFixedCost, " & _
                "nShipOptTaxRate, nAuditId) VALUES ("

                cSQL &= "Null," 'no cat
                If oMetElmt.GetAttribute("nShipOptKey") = "" Then cSQL &= "Null," Else cSQL &= "'" & oMetElmt.GetAttribute("nShipOptKey") & "',"
                If oMetElmt.GetAttribute("cShipOptName") = "" Then cSQL &= "Null," Else cSQL &= "'" & oMetElmt.GetAttribute("cShipOptName") & "',"
                If oMetElmt.GetAttribute("cShipOptCarrier") = "" Then cSQL &= "Null," Else cSQL &= "'" & oMetElmt.GetAttribute("cShipOptCarrier") & "',"
                If oMetElmt.GetAttribute("cShipOptTime") = "" Then cSQL &= "Null," Else cSQL &= "'" & oMetElmt.GetAttribute("cShipOptTime") & "',"
                If oMetElmt.GetAttribute("cShipOptTandC") = "" Then cSQL &= "Null," Else cSQL &= "'" & oMetElmt.GetAttribute("cShipOptTandC") & "',"
                If oMetElmt.GetAttribute("nShipOptCost") = "" Then cSQL &= "Null," Else cSQL &= oMetElmt.GetAttribute("nShipOptCost") & ","
                If oMetElmt.GetAttribute("nShipOptPercentage") = "" Then cSQL &= "Null," Else cSQL &= oMetElmt.GetAttribute("nShipOptPercentage") & ","
                If oMetElmt.GetAttribute("nShipOptQuantMin") = "" Then cSQL &= "Null," Else cSQL &= oMetElmt.GetAttribute("nShipOptQuantMin") & ","
                If oMetElmt.GetAttribute("nShipOptQuantMax") = "" Then cSQL &= "Null," Else cSQL &= oMetElmt.GetAttribute("nShipOptQuantMax") & ","
                If oMetElmt.GetAttribute("nShipOptWeightMin") = "" Then cSQL &= "Null," Else cSQL &= oMetElmt.GetAttribute("nShipOptWeightMin") & ","
                If oMetElmt.GetAttribute("nShipOptWeightMax") = "" Then cSQL &= "Null," Else cSQL &= oMetElmt.GetAttribute("nShipOptWeightMax") & ","
                If oMetElmt.GetAttribute("nShipOptPriceMin") = "" Then cSQL &= "Null," Else cSQL &= oMetElmt.GetAttribute("nShipOptPriceMin") & ","
                If oMetElmt.GetAttribute("nShipOptPriceMax") = "" Then cSQL &= "Null," Else cSQL &= oMetElmt.GetAttribute("nShipOptPriceMax") & ","
                If oMetElmt.GetAttribute("nShipOptHandlingPercentage") = "" Then cSQL &= "Null," Else cSQL &= oMetElmt.GetAttribute("nShipOptHandlingPercentage") & ","
                If oMetElmt.GetAttribute("nShipOptHandlingFixedCost") = "" Then cSQL &= "Null," Else cSQL &= oMetElmt.GetAttribute("nShipOptHandlingFixedCost") & ","
                If oMetElmt.GetAttribute("nShipOptTaxRate") = "" Then cSQL &= "Null," Else cSQL &= oMetElmt.GetAttribute("nShipOptTaxRate") & ","
                cSQL &= myWeb.moDbHelper.getAuditId() & ")"
                nParID = myWeb.moDbHelper.GetIdInsertSql(cSQL)
                oMetElmt.SetAttribute("NewID", nParID)
            Next
            AddResponse("Migrating Relations")
            'now the locations
            For Each oRelElmt In oDXML.SelectNodes("descendant-or-self::Relations")
                Dim nOpt As Integer
                Dim nLoc As Integer
                If Not oRelElmt.GetAttribute("nShpOptId") = "" Then nOpt = myWeb.moDbHelper.getObjectByRef(Web.dbHelper.objectTypes.CartShippingMethod, oRelElmt.GetAttribute("nShpOptId"))
                If Not oRelElmt.GetAttribute("nShpLocId") = "" Then nLoc = myWeb.moDbHelper.getObjectByRef(Web.dbHelper.objectTypes.CartShippingLocation, oRelElmt.GetAttribute("nShpLocId"))
                'only add it if there is something to add on both ends of the relation
                If Not (nOpt = 0 Or nLoc = 0) Then
                    cSQL = "INSERT INTO tblCartShippingRelations (nShpOptID, nShpLocId, nAuditId) VALUES("
                    cSQL &= nOpt & ","
                    cSQL &= nLoc & ","

                    cSQL &= myWeb.moDbHelper.getAuditId() & ")"
                    nParID = myWeb.moDbHelper.GetIdInsertSql(cSQL)
                    oRelElmt.SetAttribute("NewID", nParID)
                End If
            Next
            Return True
        Catch ex As Exception
            AddResponseError(ex) 'returnException(mcModuleName, "Migrate_Shipping", ex, "", "", gbDebug)
            Return False
        End Try
    End Function

    Public Function Migrate_Cart() As Boolean
        Dim dbh As Web.dbHelper = myWeb.moDbHelper
        Dim oDS As DataSet
        Dim oDXML As New XmlDocument
        Dim oOrdElmt As XmlElement
        Dim oItElmt As XmlElement
        Dim oConElmt As XmlElement
        Dim nParID As Integer
        Dim nParID2 As Integer
        Dim oDT As DataTable
        Dim oDC As DataColumn
        Dim cSQLP1 As String
        Dim cSQLP2 As String
        Try
            AddResponse("Migrating Orders")
            If Not dbh.TableExists("tbl_ewc_cartOrder") Then Return True
            oDS = dbh.GetDataSet("SELECT * FROM tbl_ewc_cartOrder", "Orders", "Cart")

            dbh.addTableToDataSet(oDS, "SELECT * FROM tbl_ewc_cartItem", "Items")
            dbh.addTableToDataSet(oDS, "SELECT * FROM tbl_ewc_Contact where nContactParentType = 1", "Contacts")

            oDS.Relations.Add("CartItems", oDS.Tables("Orders").Columns("nCartOrderKey"), oDS.Tables("Items").Columns("nCartItemKey"), False)
            oDS.Relations("CartItems").Nested = True
            oDS.Relations.Add("CartContacts", oDS.Tables("Orders").Columns("nCartOrderKey"), oDS.Tables("Contacts").Columns("nContactParentId"), False)
            oDS.Relations("CartContacts").Nested = True

            For Each oDT In oDS.Tables
                For Each oDC In oDT.Columns
                    oDC.ColumnMapping = MappingType.Attribute
                Next
            Next

            oDXML.InnerXml = Replace(oDS.GetXml, "'", "''")

            'ok, now we need to loop through the carts, 
            'add the items, splitting out the options as well
            'then add the contacts for the cart
            'having lots of checking so just in case there is a column missing
            Dim nOrdCount As Integer
            For Each oOrdElmt In oDXML.SelectNodes("descendant-or-self::Orders")
                nOrdCount += 1
                AddResponse("Migrating Order " & oOrdElmt.GetAttribute("nCartOrderKey") & "(" & nOrdCount & ")")
                'Firstly the Cart Order
                cSQLP2 = ""
                cSQLP1 = "INSERT INTO tblCartOrder " & _
                "(cCartForiegnRef, nCartStatus, cCartSchemaName, cCartSessionId, nCartUserDirId, " & _
                "nPayMthdId, cPaymentRef, cCartXml, nShippingMethodId, cShippingDesc, nShippingCost, " & _
                "cClientNotes, cSellerNotes, nTaxRate, nGiftListId, nAuditId) VALUES( "

                If oOrdElmt.GetAttribute("nCartOrderKey") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= oOrdElmt.GetAttribute("nCartOrderKey") & ","

                If oOrdElmt.GetAttribute("nCartStatus") = "" Then cSQLP2 &= "0," Else cSQLP2 &= IIf(oOrdElmt.GetAttribute("nCartStatus") = 6, 7, oOrdElmt.GetAttribute("nCartStatus")) & ","
                cSQLP2 &= "Null," 'Cart Schema Name
                If oOrdElmt.GetAttribute("cCartSessionId") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= "'" & oOrdElmt.GetAttribute("cCartSessionId") & "',"
                If oOrdElmt.GetAttribute("nCartUserDirId") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= oOrdElmt.GetAttribute("nCartUserDirId") & ","

                If oOrdElmt.GetAttribute("nPaymentMethod") = "" Then
                    cSQLP2 &= "Null,"
                ElseIf Not IsNumeric(oOrdElmt.GetAttribute("nPaymentMethod")) Then
                    cSQLP2 &= "Null,"
                Else
                    cSQLP2 &= oOrdElmt.GetAttribute("nPaymentMethod") & ","
                End If

                If oOrdElmt.GetAttribute("nPaymentRef") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= "'" & oOrdElmt.GetAttribute("nPaymentRef") & "',"
                cSQLP2 &= "Null," 'Cart XML

                If oOrdElmt.GetAttribute("nShippingId") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= moDbHelper.getObjectByRef(Web.dbHelper.objectTypes.CartShippingMethod, oOrdElmt.GetAttribute("nShippingId")) & ","
                If oOrdElmt.GetAttribute("cShippingDesc") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= "'" & oOrdElmt.GetAttribute("cShippingDesc") & "',"
                If oOrdElmt.GetAttribute("nShippingCost") = "" Then cSQLP2 &= "0," Else cSQLP2 &= oOrdElmt.GetAttribute("nShippingCost") & ","
                If oOrdElmt.GetAttribute("cClientNotes") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= "'<Notes><Notes>" & oOrdElmt.GetAttribute("cClientNotes") & "</Notes></Notes>',"
                If oOrdElmt.GetAttribute("cSellerNotes") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= "'" & oOrdElmt.GetAttribute("cSellerNotes") & "',"
                If oOrdElmt.GetAttribute("nTaxRate") = "" Then cSQLP2 &= "0," Else cSQLP2 &= oOrdElmt.GetAttribute("nTaxRate") & ","
                If oOrdElmt.GetAttribute("nGiftListID") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= "'" & oOrdElmt.GetAttribute("nGiftListID") & "',"
                cSQLP2 &= dbh.getAuditId() & ")"
                nParID = dbh.GetIdInsertSql(cSQLP1 & cSQLP2)

                'Now the cart Items
                For Each oItElmt In oOrdElmt.SelectNodes("Items")
                    AddResponse("Migrating Item " & oItElmt.GetAttribute("nCartItemKey") & "")
                    cSQLP2 = ""
                    cSQLP1 = "INSERT INTO tblCartItem (nCartOrderId, nItemId, nParentID, cItemRef, " & _
                    "cItemURL, cItemName, nItemOptGrpIdx, nItemOptIdx, nPrice, nShpCat, nDiscountCat, " & _
                    "nDiscountValue,nTaxRate, nQuantity, nWeight, nAuditId) VALUES ("

                    cSQLP2 &= nParID & "," 'OrderID
                    If oItElmt.GetAttribute("nItemId") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= moDbHelper.getObjectByRef(Web.dbHelper.objectTypes.Content, oItElmt.GetAttribute("nItemId")) & ","
                    cSQLP2 &= "Null," 'product parentID for options
                    If oItElmt.GetAttribute("cItemRef") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= "'" & oItElmt.GetAttribute("cItemRef") & "',"
                    If oItElmt.GetAttribute("cItemURL") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= "'" & oItElmt.GetAttribute("cItemURL") & "',"
                    If oItElmt.GetAttribute("cItemName") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= "'" & CleanName(oItElmt.GetAttribute("cItemName")) & "',"
                    cSQLP2 &= "Null," 'nItemOptGrpIdx
                    cSQLP2 &= "Null," 'nItemOptIdx
                    If oItElmt.GetAttribute("nPrice") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= oItElmt.GetAttribute("nPrice") & ","
                    If oItElmt.GetAttribute("nShippingLevel") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= oItElmt.GetAttribute("nShippingLevel") & ","
                    cSQLP2 &= "Null," 'nDiscountCat
                    cSQLP2 &= "Null," 'nDiscountValue
                    If oItElmt.GetAttribute("nTaxRate") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= oItElmt.GetAttribute("nTaxRate") & ","
                    If oItElmt.GetAttribute("nQuantity") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= oItElmt.GetAttribute("nQuantity") & ","
                    'If oItElmt.GetAttribute("nDiscount") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= oItElmt.GetAttribute("nDiscount") & ","
                    If oItElmt.GetAttribute("nWeight") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= oItElmt.GetAttribute("nWeight") & ","
                    cSQLP2 &= dbh.getAuditId() & ")"
                    nParID2 = dbh.GetIdInsertSql(cSQLP1 & cSQLP2)

                    'Now for splitting out the options
                    'basically loops through and checks if there is anything in option 1 or 2, if there is add it(ish)
                    Dim nOptNo As Integer = 1
DoOptions:
                    If nOptNo < 3 Then 'since its only options 1 and 2 we dont want to try 3
                        If Not oItElmt.GetAttribute("cItemOption" & nOptNo) = "" Then
                            cSQLP2 = ""
                            cSQLP1 = "INSERT INTO tblCartItem (nCartOrderId, nItemId, nParentID, cItemRef, " & _
                            "cItemURL, cItemName, nItemOptGrpIdx, nItemOptIdx, nPrice, nShpCat, nDiscountCat, " & _
                            "nDiscountValue, nTaxRate, nQuantity, nWeight, nAuditId) VALUES ("

                            cSQLP2 &= nParID & "," 'OrderID
                            If oItElmt.GetAttribute("nItemId") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= moDbHelper.getObjectByRef(Web.dbHelper.objectTypes.Content, oItElmt.GetAttribute("nItemId")) & ","
                            cSQLP2 &= nParID2 & "," 'product parentID for options
                            cSQLP2 &= "'" & oItElmt.GetAttribute("cItemOption" & nOptNo) & "'," 'New ref as option
                            If oItElmt.GetAttribute("cItemURL") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= "'" & oItElmt.GetAttribute("cItemURL") & "',"
                            cSQLP2 &= "'" & oItElmt.GetAttribute("cItemOption" & nOptNo) & "'," 'New Name as option
                            cSQLP2 &= "0," 'nItemOptGrpIdx
                            cSQLP2 &= "'" & (nOptNo - 1) & "'," 'nItemOptIdx
                            cSQLP2 &= "0," 'price
                            If oItElmt.GetAttribute("nShippingLevel") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= oItElmt.GetAttribute("nShippingLevel") & ","
                            cSQLP2 &= "Null," 'nDiscountCat
                            cSQLP2 &= "Null," 'nDiscountValue
                            If oItElmt.GetAttribute("nTaxRate") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= oItElmt.GetAttribute("nTaxRate") & ","
                            cSQLP2 &= "0," 'quantity
                            'cSQLP2 &= "0," 'Discount
                            cSQLP2 &= "0,"
                            cSQLP2 &= dbh.getAuditId() & ")"
                            dbh.GetIdInsertSql(cSQLP1 & cSQLP2)
                        End If
                        nOptNo += 1
                        GoTo DoOptions
                    End If
                Next
                'now for the contacts
                For Each oConElmt In oOrdElmt.SelectNodes("Contacts")
                    AddResponse("Migrating Contact " & oConElmt.GetAttribute("nContactKey") & "")
                    'Firstly the Cart Order
                    cSQLP2 = ""
                    cSQLP1 = "INSERT INTO tblCartContact (nContactDirId, nContactCartId, cContactType, cContactName, " & _
                            "cContactCompany, cContactAddress, cContactCity, cContactState, cContactZip, cContactCountry, " & _
                            "cContactTel, cContactFax, cContactEmail, cContactXml, nAuditId) VALUES ("

                    cSQLP2 &= "0,"
                    cSQLP2 &= nParID & ","
                    If oConElmt.GetAttribute("cContactType") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= "'" & oConElmt.GetAttribute("cContactType") & "',"
                    If oConElmt.GetAttribute("cContactName") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= "'" & Replace(oConElmt.GetAttribute("cContactName"), "'", "''") & "',"
                    If oConElmt.GetAttribute("cContactCompany") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= "'" & Replace(oConElmt.GetAttribute("cContactCompany"), "'", "''") & "',"
                    If oConElmt.GetAttribute("cContactAddress") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= "'" & Replace(oConElmt.GetAttribute("cContactAddress"), "'", "''") & "',"
                    If oConElmt.GetAttribute("cContactCity") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= "'" & oConElmt.GetAttribute("cContactCity") & "',"
                    If oConElmt.GetAttribute("cContactState") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= "'" & oConElmt.GetAttribute("cContactState") & "',"
                    If oConElmt.GetAttribute("cContactZip") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= "'" & oConElmt.GetAttribute("cContactZip") & "',"
                    If oConElmt.GetAttribute("cContactCountry") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= "'" & oConElmt.GetAttribute("cContactCountry") & "',"
                    If oConElmt.GetAttribute("cContactTel") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= "'" & oConElmt.GetAttribute("cContactTel") & "',"
                    If oConElmt.GetAttribute("cContactFax") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= "'" & oConElmt.GetAttribute("cContactFax") & "',"
                    If oConElmt.GetAttribute("cContactEmail") = "" Then cSQLP2 &= "Null," Else cSQLP2 &= "'" & oConElmt.GetAttribute("cContactEmail") & "',"
                    cSQLP2 &= "Null,"
                    cSQLP2 &= dbh.getAuditId() & ")"
                    dbh.GetIdInsertSql(cSQLP1 & cSQLP2)
                Next
            Next
            Return True
        Catch ex As Exception
            AddResponseError(ex) 'returnException(mcModuleName, "Migrate_Cart", ex, "", "", gbDebug)
            Return False
        End Try

    End Function

    Public Function upgradeContentSchemas(ByVal cContentSchema As String, ByVal cContentXml As String, ByVal type As String) As String

        Dim sWriter As IO.StringWriter
        Dim sResult As String
        Dim oXmlDr As New XmlDocument
        Dim cProcessInfo As String = "error converting type:" & cContentSchema & " data:" & cContentXml
        Dim oTransform2 As New Eonic.XmlHelper.Transform()
        Try
            'fix for any nasty entities
            cContentXml = Eonic.Tools.Xml.convertEntitiesToCodes(cContentXml)

            oXmlDr.LoadXml(cContentXml)
            'add Eonic Bespoke Functions
            Dim xsltArgs As Xsl.XsltArgumentList = New Xsl.XsltArgumentList

            'Run transformation
            Dim cPath As String = CStr(goServer.MapPath(goConfig("ProjectPath") & "/migratexsl/" & cContentSchema & "-" & type & ".xsl"))
            If Not IO.File.Exists(cPath) Then
                cPath = CStr(goServer.MapPath("/ewcommon/migratexsl/" & cContentSchema & "-" & type & ".xsl"))
                If Not IO.File.Exists(cPath) Then
                    Return cContentXml
                End If
            End If
            oTransform2.XSLFile = cPath
            oTransform2.Compiled = False
            sWriter = New IO.StringWriter
            Dim oInputDoc As New XmlDocument
            oInputDoc.LoadXml(cContentXml)
            oTransform2.Process(oInputDoc, sWriter)

            sResult = sWriter.ToString()

            sWriter = Nothing
            oXmlDr = Nothing


            Return sResult

        Catch ex As Exception
            AddResponseError(ex, cProcessInfo) 'returnException(mcModuleName, "upgradeContentSchemas", ex, "", cProcessInfo, gbDebug)
            Return ""
        End Try

    End Function

    Public Function Migrate_Rollback() As Boolean
        'deletes v4 Tables
        Try
            'AddResponse("Removing V4 Tables")
            myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath("/ewcommon/sqlupdate/toV4/DropAllForeignKeys.sql"))
            msException = ""
            myWeb.moDbHelper.ExeProcessSqlfromFile(goServer.MapPath("/ewcommon/sqlupdate/toV4/ClearDB.SQL"))
            AddResponse("Run File (/ewcommon/sqlupdate/toV4/ClearDB.SQL)")
            Return saveVersionNumber("0.0.0.0")
        Catch ex As Exception
            AddResponseError(ex) 'returnException(mcModuleName, "Rollback_Migration", ex, "", "", gbDebug)
            Return False
        End Try
    End Function


    Public Function CleanName(ByVal cName As String, Optional ByVal bLeaveAmp As Boolean = False) As String

        Eonic.Tools.CleanName(cName, bLeaveAmp, False)

    End Function

#End Region

#Region "Table Dropping"
    Private Sub ifTableExistsDropIt(ByRef sTableName As String)

        Dim sSqlStr As String
        Dim cProcessInfo As String = "ifTableExistsDropIt"
        Try
            sSqlStr = "if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[" & sTableName & "]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)"
            sSqlStr = sSqlStr & "drop table [dbo].[" & sTableName & "] "
            myWeb.moDbHelper.ExeProcessSql(sSqlStr)
        Catch ex As Exception
            AddResponseError(ex) 'returnException(mcModuleName, "ifTableExistsDropIt", ex, "", cProcessInfo, gbDebug)
        End Try
    End Sub
    Private Sub ifSqlObjectExistsDropIt(ByRef sName As String, ByRef sObjectType As String)

        Dim sSqlStr As String
        Dim cProcessInfo As String = "ifTableExistsDropIt"
        Dim sObjProperty As String
        Try

            Select Case sObjectType
                Case "Table"
                    sObjProperty = "OBJECTPROPERTY(id, N'IsUserTable') = 1"
                Case "Function"
                    sObjProperty = "xtype in (N'FN', N'IF', N'TF')"
                Case Else
                    sObjProperty = "OBJECTPROPERTY(id, N'Is" & sObjectType & "') = 1"
            End Select

            sSqlStr = "if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[" & sName & "]') and " & sObjProperty & ")"
            sSqlStr = sSqlStr & "drop " & LCase(sObjectType) & " [dbo].[" & sName & "] "
            myWeb.moDbHelper.ExeProcessSql(sSqlStr)

        Catch ex As Exception
            AddResponseError(ex) 'returnException(mcModuleName, "ifSqlObjectExistsDropIt", ex, "", cProcessInfo, gbDebug)
        End Try
    End Sub

#End Region

#Region "Import / Export Routines"

    Public Function LoadShippingLocations() As Boolean

        'BJR NEW CODE BELOW


        'Dim cProcessInfo As String = "LoadShippingLocations"

        'Try
        '    Dim oXml As XmlDocument = New XmlDocument
        '    Dim bIsXml As Boolean = False

        '    Try
        '        oXml.Load(goServer.MapPath("/ewcommon/sqlupdate/import_data/ex_shiplocs_master.xml"))
        '        bIsXml = True
        '    Catch ex As Exception
        '        ' If Load fails then there's something invalid about what we just imported.
        '    End Try

        '    If bIsXml Then
        '        ' Try to validate the xml
        '        moDBHelper.importShippingLocations(oXml)
        '    End If

        '    AddResponse("Import Shipping Locations'")

        '    Return True
        'Catch ex As Exception
        '    AddResponseError(ex) 'returnException(mcModuleName, "ifSqlObjectExistsDropIt", ex, "", cProcessInfo, gbDebug)
        '    Return False
        'End Try
        Dim cProcessInfo As String = "LoadShippingLocations"

        Try
            Dim oXml As XmlDocument = New XmlDocument
            Dim bIsXml As Boolean = False

            Try
                oXml.Load(goServer.MapPath("/ewcommon/sqlupdate/import_data/ex_shiplocs_master2.xml"))
                bIsXml = True
            Catch ex As Exception
                ' If Load fails then there's something invalid about what we just imported.
            End Try

            If bIsXml Then
                ' Try to validate the xml
                myWeb.moDbHelper.importShippingLocations2(oXml)
            End If

            AddResponse("Import Shipping Locations'")

            Return True
        Catch ex As Exception
            AddResponseError(ex) 'returnException(mcModuleName, "ifSqlObjectExistsDropIt", ex, "", cProcessInfo, gbDebug)
            Return False
        End Try
    End Function


    Public Sub ManageShippingLocations()
        Dim cImport As String = ""
        Dim oImportStream As System.IO.StreamReader
        Dim fUpld As System.Web.HttpPostedFile
        Dim oXml As XmlDocument = New XmlDocument
        Dim bIsXml As Boolean = False
        Dim bIsUpload As Boolean = False

        If Not (goRequest("export") Is Nothing) Then
            goResponse.AddHeader("Content-Type", "text/xml")
            goResponse.AddHeader("Content-Disposition", "attachment; filename=ex_shiplocs_" & Format(Now(), "yyMMddHHmmss") & ".xml")
            goResponse.Write("<?xml version=""1.0"" encoding=""UTF-8""?>" & moDbHelper.exportShippingLocations())
        Else
            If Not (goRequest("import") Is Nothing) Then
                'lets do some hacking 
                bIsUpload = True
                fUpld = goRequest.Files("importfile")

                If Not fUpld Is Nothing Then
                    ' Initialize the stream.
                    oImportStream = New System.IO.StreamReader(fUpld.InputStream)
                    cImport = oImportStream.ReadToEnd
                    oImportStream.Close()

                    ' Check the import stream for XML correctness
                    Try
                        oXml.LoadXml(cImport)
                        bIsXml = True
                    Catch ex As Exception
                        ' If Load fails then there's something invalid about what we just imported.
                    End Try

                    If bIsXml Then
                        ' Try to validate the xml
                        moDbHelper.importShippingLocations(oXml)
                    End If
                End If

            End If

            goResponse.Write("<html><head><title>Manage Shipping Locations</title></head><body>")
            goResponse.Write("<form method=post action="""" enctype=""multipart/form-data"">Database Name (optional - uses web.config if none) <input type=text name=""db""/>  Source Version: <select name=""version""><option value=""v2"">v2-v3</option><option value=""v4"">v4</option></select><input type=submit name=export value=""Export""/><br/>")
            goResponse.Write("<input type=file name=""importfile""/><input type=submit name=import value=""Import""/>")
            goResponse.Write("</form>")

            If bIsUpload Then

                goResponse.Write("<div style=""height: 200px;width:100%;overflow:scroll;"">")
                If bIsXml Then
                    goResponse.Write(Replace(Replace(cImport, ">", "&gt;"), "<", "&lt;"))
                Else
                    goResponse.Write("The imported file was not valid xml")
                End If
                goResponse.Write("</div>")

            End If

            goResponse.Write("</body></html>")
        End If

    End Sub
#End Region

    Friend Function CommitToLog(ByVal nEventType As Eonic.Web.dbHelper.ActivityType, ByVal nUserId As Integer, ByVal cSessionId As String, ByVal dDateTime As Date, Optional ByVal nPrimaryId As Integer = 0, Optional ByVal nSecondaryId As Integer = 0, Optional ByVal cDetail As String = "") As Integer
        Dim cSQL As String = "INSERT INTO tblActivityLog (nUserDirId, nStructId, nArtId, dDateTime, nActivityType, cActivityDetail, cSessionId) VALUES ("
        cSQL &= nUserId & ","
        cSQL &= nPrimaryId & ","
        cSQL &= nSecondaryId & ","
        cSQL &= Eonic.Tools.Database.SqlDate(dDateTime, True) & ","
        cSQL &= nEventType & ","
        cSQL &= "'" & cDetail & "',"
        cSQL &= "'" & cSessionId & "')"
        Return myWeb.moDbHelper.GetIdInsertSql(cSQL)
    End Function

    Friend Sub UpdateLogDetail(ByVal nActivityKey As Integer, ByVal cActivityDetail As String)
        Dim cSQL As String = "UPDATE tblActivityLog " _
                                & "SET cActivityDetail = '" & SqlFmt(cActivityDetail) & "' " _
                                & "WHERE nActivityKey = " & SqlFmt(nActivityKey)
        myWeb.moDbHelper.ExeProcessSql(cSQL)
    End Sub


    Public Class SetupXforms
        Inherits xForm
        Private Const mcModuleName As String = "Setup.SetupXForms"
        Private mySetup As Eonic.Setup

        Public goConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("eonic/web")

        Public Sub New(ByRef asetup As Eonic.Setup)
            PerfMon.Log("Discount", "New")
            Try
                mySetup = asetup
                goConfig = mySetup.goConfig
                MyBase.moPageXML = mySetup.moPageXml
            Catch ex As Exception
                returnException(mcModuleName, "New", ex, "", "", True)
            End Try
        End Sub


        Public Function GuessDBName() As String
            PerfMon.Log("Setup", "GuessDBName")
            Try
                Dim siteUrl As String = goRequest.ServerVariables("SERVER_NAME")
                Dim PrePropURL As String = goConfig("PrePropUrl") & ""
                If PrePropURL <> "" Then siteUrl = siteUrl.Replace(PrePropURL, "")
                siteUrl = Replace(siteUrl, "http://", "")
                siteUrl = Replace(siteUrl, "www.", "")
                siteUrl = Replace(siteUrl, ".", "_")
                siteUrl = Replace(siteUrl, "-", "_")
                Return "ew_" & siteUrl
            Catch ex As Exception
                returnException(mcModuleName, "GuessDBName", ex, "", "", True)
                Return ""
            End Try
        End Function

        Public Function xFrmWebSettings() As XmlElement
            Dim oFrmElmt As XmlElement
            Dim cProcessInfo As String = ""
            Dim oFsh As fsHelper

            Try
                oFsh = New fsHelper
                oFsh.open(moPageXML)

                MyBase.NewFrm("WebSettings")

                MyBase.submission("WebSettings", "", "post", "form_check(this)")

                oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "WebSettings", "", "Enter your MS SQL connection details")

                MyBase.addNote(oFrmElmt, noteTypes.Hint, "Please enter your database connection details.")

                MyBase.addInput(oFrmElmt, "ewDatabaseServer", True, "DB Server Hostname")
                MyBase.addBind("ewDatabaseServer", "web/add[@key='DatabaseServer']/@value", "true()")

                MyBase.addInput(oFrmElmt, "ewDatabaseName", True, "DB Name")
                MyBase.addBind("ewDatabaseName", "web/add[@key='DatabaseName']/@value", "true()")

                MyBase.addInput(oFrmElmt, "ewDatabaseUsername", True, "DB Username")
                MyBase.addBind("ewDatabaseUsername", "web/add[@key='DatabaseUsername']/@value", "false()")

                MyBase.addInput(oFrmElmt, "ewDatabasePassword", True, "DB Password / CMS Admin Password")
                MyBase.addBind("ewDatabasePassword", "web/add[@key='DatabasePassword']/@value", "false()")

                MyBase.addInput(oFrmElmt, "ewSiteAdminEmail", True, "Webmaster Email")
                MyBase.addBind("ewSiteAdminEmail", "web/add[@key='SiteAdminEmail']/@value", "true()")

                MyBase.addSubmit(oFrmElmt, "", "Save Settings")

                'Dim oCfg As Configuration = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/")
                'Dim oCgfSect As System.Configuration.DefaultSection = oCfg.GetSection("eonic/web")

                '  Dim oImp As Eonic.Tools.Security.Impersonate = New Eonic.Tools.Security.Impersonate
                '  If oImp.ImpersonateValidUser(goConfig("AdminAcct"), goConfig("AdminDomain"), goConfig("AdminPassword"), True, goConfig("AdminGroup")) Then

                'MyBase.instance.InnerXml = oCgfSect.SectionInformation.GetRawXml
                Dim oDefaultCfgXml As New XmlDocument

                If oFsh.VirtualFileExists("/eonic.web.config") Then
                    oDefaultCfgXml.Load(goServer.MapPath("/eonic.web.config"))
                Else
                    oDefaultCfgXml.Load(goServer.MapPath("/ewcommon/setup/rootfiles/eonic_web_config.xml"))
                End If


                MyBase.Instance.InnerXml = oDefaultCfgXml.SelectSingleNode("web").OuterXml

                    'code here to replace any missing nodes
                    'all of the required config settings

                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                    If MyBase.valid Then

                        'lets insure all the essential files are in place
                        ' Dim CreateDirs As New FileStructureSetup
                        'If Not CreateDirs.Execute() Then

                        'MyBase.valid = False
                        'MyBase.addNote(oFrmElmt, noteTypes.Alert, CreateDirs.errMsg)

                        'Else

                        Dim oCfg As Configuration = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/")

                        If Not oCfg Is Nothing Then
                            Dim oCgfSect As System.Configuration.DefaultSection = oCfg.GetSection("eonic/web")
                            Dim oRwSect As System.Configuration.IgnoreSection = oCfg.GetSection("system.webServer")
                            If Not oCgfSect Is Nothing Then
                                oCgfSect.SectionInformation.RestartOnExternalChanges = False
                                oCgfSect.SectionInformation.SetRawXml(MyBase.Instance.InnerXml)
                                'oRwSect.SectionInformation.SetRawXml(oDefaultCfgXml.SelectSingleNode("/configuration/system.webServer").OuterXml)
                                oCfg.Save()
                            Else
                                'update config based on form submission
                                oDefaultCfgXml.SelectSingleNode("/configuration/eonic").InnerXml = MyBase.Instance.InnerXml
                                'save as web.config in the root
                                oDefaultCfgXml.Save(goServer.MapPath("eonic.web.config"))
                            End If
                        Else
                            'update config based on form submission
                            oDefaultCfgXml.SelectSingleNode("/configuration/eonic").InnerXml = MyBase.Instance.InnerXml
                            'save as web.config in the root
                            oDefaultCfgXml.Save(goServer.MapPath("web.config"))
                        End If

                        'Now lets create the database
                        Dim oDbt As New Eonic.Web.dbHelper(Nothing)
                        Dim sDbName As String = Instance.SelectSingleNode("web/add[@key='DatabaseName']/@value").InnerText
                        If oDbt.createDB(sDbName) Then
                            'success
                            MyBase.valid = True
                        Else
                            MyBase.valid = False
                            MyBase.addNote(oFrmElmt, noteTypes.Hint, "These database connection details could not connect.")
                        End If
                        oDbt = Nothing
                    End If

                    'CreateDirs = Nothing

                End If
                ' End If
                '  oImp.UndoImpersonation()
                'lets take a guess at the DB Name
                If Instance.SelectSingleNode("web/add[@key='DatabaseName']/@value").InnerText = "" Then
                    Instance.SelectSingleNode("web/add[@key='DatabaseName']/@value").InnerText = GuessDBName()
                End If

                ' If Instance.SelectSingleNode("web/add[@key='VersionNumber']/@value").InnerText = "" Then
                '   Instance.SelectSingleNode("web/add[@key='VersionNumber']/@value").InnerText = "4.1.0.0"
                ' End If

                ' Else
                '  MyBase.addNote(oFrmElmt, noteTypes.Alert, "Admin credentials need to be configured correctly in the web.config", True)
                ' End If

                MyBase.addValues()
                Return MyBase.moXformElmt

            Catch ex As Exception
                returnException(mcModuleName, "xFrmWebSettings", ex, "", cProcessInfo, True)
                Return Nothing
            End Try
        End Function


        Public Function xFrmBackupDatabase() As XmlElement
            Dim oFrmElmt As XmlElement
            Dim cProcessInfo As String = ""
            Dim oFsh As fsHelper

            Dim DatabaseName As String = goConfig("DatabaseName")
            Dim DatabaseFilename As String = Year(DateTime.Now()) & "-" & Month(DateTime.Now()) & "-" & Day(DateTime.Now()) & "-" & TimeOfDay() & "-" & goConfig("DatabaseName") & ".bak"
            Dim DatabaseFilepath As String = goServer.MapPath("/") & "..\data"

            Try
                oFsh = New fsHelper
                oFsh.open(moPageXML)

                MyBase.NewFrm("BackupDatabase")

                MyBase.submission("BackupDatabase", "", "post", "form_check(this)")

                oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "BackupDatabase", "", "Backup Database")

                MyBase.addNote(oFrmElmt, noteTypes.Hint, "Please enter your database connection details.")

                MyBase.addInput(oFrmElmt, "ewDatabaseName", True, "Database Name")
                MyBase.addBind("ewDatabaseName", "backup/@name", "true()")

                MyBase.addInput(oFrmElmt, "ewDatabaseFilename", True, "Backup Filename")
                MyBase.addBind("ewDatabaseFilename", "backup/@filename", "false()")

                MyBase.addInput(oFrmElmt, "ewDatabaseFilepath", True, "Backup Filepath")
                MyBase.addBind("ewDatabaseFilepath", "backup/@filepath", "false()")

                MyBase.addSubmit(oFrmElmt, "", "Backup Database")

                Dim oImp As Eonic.Tools.Security.Impersonate = New Eonic.Tools.Security.Impersonate
                If oImp.ImpersonateValidUser(goConfig("AdminAcct"), goConfig("AdminDomain"), goConfig("AdminPassword"), True, goConfig("AdminGroup")) Then

                    MyBase.Instance.InnerXml = "<backup name=""" & DatabaseName & """ filename=""" & DatabaseFilename & """ filepath=""" & DatabaseFilepath & """/>"

                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                        If MyBase.valid Then

                            Dim oDB As New Eonic.Tools.Database

                            oDB.DatabaseServer = goConfig("DatabaseServer")
                            oDB.DatabaseUser = Eonic.Tools.Text.SimpleRegexFind(goConfig("DatabaseAuth"), "user id=([^;]*)", 1, Text.RegularExpressions.RegexOptions.IgnoreCase)
                            oDB.DatabasePassword = Eonic.Tools.Text.SimpleRegexFind(goConfig("DatabaseAuth"), "password=([^;]*)", 1, Text.RegularExpressions.RegexOptions.IgnoreCase)
                            oDB.ConnectTimeout = 60
                            'oDB.ConnectionPooling = True
                            oDB.BackupDatabase(DatabaseName, DatabaseFilepath)

                        End If
                    End If
                    oImp.UndoImpersonation()

                Else
                    MyBase.addNote(oFrmElmt, noteTypes.Alert, "Admin credentials need to be configured correctly in the web.config", True)
                End If

                MyBase.addValues()
                Return MyBase.moXformElmt

            Catch ex As Exception
                returnException(mcModuleName, "xFrmBackupDatabase", ex, "", cProcessInfo, True)
                Return Nothing
            End Try
        End Function

        Public Function xFrmRestoreDatabase() As XmlElement
            Dim oFrmElmt As XmlElement
            Dim cProcessInfo As String = ""
            Dim oFsh As fsHelper

            Dim DatabaseName As String = goConfig("DatabaseName")
            Dim DatabaseFilename As String
            Dim DatabaseFilepath As String = goServer.MapPath("/") & "..\data"

            Try
                oFsh = New fsHelper
                oFsh.open(moPageXML)

                MyBase.NewFrm("RestoreDatabase")

                MyBase.submission("RestoreDatabase", "", "post", "form_check(this)")

                oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "RestoreDatabase", "", "Restore Database")

                MyBase.addNote(oFrmElmt, noteTypes.Hint, "Please select the database to restore.")

                MyBase.addInput(oFrmElmt, "ewDatabaseName", True, "Database Name to be Overwritten")
                MyBase.addBind("ewDatabaseName", "restore/@name", "true()")

                Dim sel1 As XmlElement = MyBase.addSelect1(oFrmElmt, "ewDatabaseFilename", True, "Select a backup allready on the server")
                MyBase.addOptionsFilesFromDirectory(sel1, DatabaseFilepath)
                MyBase.addBind("ewDatabaseFilename", "restore/@filename", "false()")

                MyBase.addUpload(oFrmElmt, "ewDatabaseUpload", True, "bak,zip", "Or upload here....")
                MyBase.addBind("ewDatabaseUpload", "restore/@upload", "false()")

                MyBase.addSubmit(oFrmElmt, "", "Restore Database")

                Dim oImp As Eonic.Tools.Security.Impersonate = New Eonic.Tools.Security.Impersonate
                If oImp.ImpersonateValidUser(goConfig("AdminAcct"), goConfig("AdminDomain"), goConfig("AdminPassword"), True, goConfig("AdminGroup")) Then

                    MyBase.Instance.InnerXml = "<restore name=""" & DatabaseName & """ filename=""" & DatabaseFilename & """ filepath=""" & DatabaseFilepath & """ upload=""""/>"

                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                        'lets do some hacking 
                        Dim fUpld As System.Web.HttpPostedFile
                        fUpld = goRequest.Files("ewDatabaseUpload")

                        If fUpld.ContentLength = 0 And goRequest("ewDatabaseFilename") = "" Then
                            MyBase.valid = False
                            MyBase.addNote(oFrmElmt, noteTypes.Alert, "Please Specify a file to restore")
                        End If

                        If MyBase.valid Then

                            If fUpld.ContentLength = 0 Then
                                DatabaseFilename = goRequest("ewDatabaseFilename")
                            Else
                                Dim oFs As fsHelper = New fsHelper
                                'oFs.initialiseVariables(nType)
                                Dim sValidResponse As String
                                sValidResponse = oFs.SaveFile(fUpld, DatabaseFilepath)
                                DatabaseFilename = DatabaseFilepath & "\" & fUpld.FileName
                            End If

                            Dim oDB As New Eonic.Tools.Database

                            oDB.DatabaseServer = goConfig("DatabaseServer")
                            oDB.DatabaseUser = Eonic.Tools.Text.SimpleRegexFind(goConfig("DatabaseAuth"), "user id=([^;]*)", 1, Text.RegularExpressions.RegexOptions.IgnoreCase)
                            oDB.DatabasePassword = Eonic.Tools.Text.SimpleRegexFind(goConfig("DatabaseAuth"), "password=([^;]*)", 1, Text.RegularExpressions.RegexOptions.IgnoreCase)
                            oDB.FTPUser = goConfig("DatabaseFtpUsername")
                            oDB.FtpPassword = goConfig("DatabaseFtpPassword")
                            oDB.RestoreDatabase(DatabaseName, DatabaseFilename)

                        End If
                    End If
                    oImp.UndoImpersonation()

                Else
                    MyBase.addNote(oFrmElmt, noteTypes.Alert, "Admin credentials need to be configured correctly in the web.config", True)
                End If

                MyBase.addValues()
                Return MyBase.moXformElmt

            Catch ex As Exception
                returnException(mcModuleName, "xFrmRestoreDatabase", ex, "", cProcessInfo, True)
                Return Nothing
            End Try
        End Function

        Public Function xFrmNewDatabase() As XmlElement
            Dim oFrmElmt As XmlElement
            Dim cProcessInfo As String = ""
            Dim oFsh As fsHelper

            Dim DatabaseName As String = goConfig("DatabaseName")
            Dim DatabaseFilename As String = "NewV4"
            Dim DatabaseFilepath As String = goServer.MapPath("/ewcommon/setup/db")

            Try
                oFsh = New fsHelper
                oFsh.open(moPageXML)

                MyBase.NewFrm("NewDatabase")

                MyBase.submission("NewDatabase", "", "post", "form_check(this)")

                oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "NewDatabase", "", "New Database")

                MyBase.addNote(oFrmElmt, noteTypes.Hint, "Create the eonicweb5 Database Tables")

                MyBase.addInput(oFrmElmt, "ewDatabaseName", True, "Database Name")
                MyBase.addBind("ewDatabaseName", "restore/@name", "true()")

                Dim sel1 As XmlElement = MyBase.addSelect1(oFrmElmt, "ewDatabaseFilename", True, "Install Empty DB or one containing sample data which would be better for initial evaluation of the platform", "", Eonic.xForm.ApperanceTypes.Full)
                MyBase.addOption(sel1, "Empty Database", "NewV4")
                MyBase.addOptionsFilesFromDirectory(sel1, DatabaseFilepath, "zip")
                MyBase.addBind("ewDatabaseFilename", "restore/@filename", "false()")


                MyBase.addSubmit(oFrmElmt, "", "Create Database")

                MyBase.Instance.InnerXml = "<restore name=""" & DatabaseName & """ filename=""" & DatabaseFilename & """ filepath=""" & DatabaseFilepath & """/>"

                If MyBase.isSubmitted Then
                    MyBase.updateInstanceFromRequest()
                    MyBase.validate()

                    If MyBase.valid Then
                        DatabaseFilename = goRequest("ewDatabaseFilename")
                        If DatabaseFilename = "NewV4" Then
                            '  mySetup.buildDatabase(True)
                            mySetup.cPostFlushActions = "NewDB"
                        Else
                            mySetup.cPostFlushActions = "RestoreZip"



                        End If
                    End If
                End If

                MyBase.addValues()
                Return MyBase.moXformElmt

            Catch ex As Exception
                returnException(mcModuleName, "xFrmRestoreDatabase", ex, "", cProcessInfo, True)
                Return Nothing
            End Try
        End Function


    End Class

End Class

Public Class FileStructureSetup
    Public oSetup As Setup
    Public oEwVersion As String = "5.0"
    Public IISVersion As Double = 7
    Public goConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("eonic/web")
    Public imageRootPath As String = "/images"
    Public docRootPath As String = "/docs"
    Public docMediaPath As String = "/media"
    Public errMsg As String = ""


    Function Execute() As Boolean

        Try

            'Dim oImp As Eonic.Tools.Security.Impersonate = New Eonic.Tools.Security.Impersonate
            'If oImp.ImpersonateValidUser(goConfig("AdminAcct"), goConfig("AdminDomain"), goConfig("AdminPassword"), True, goConfig("AdminGroup")) Then

            If IISVersion < 7 Then
                'copy the .htaccess file for version 3 of Isapi Rewrite
                'Dim fso As IO.File
                If Not IO.File.Exists(goServer.MapPath(goConfig("ProjectPath") & "/.htaccess")) Then
                    IO.File.Copy(goServer.MapPath("/ewcommon/setup/rootfiles/.htaccess"), goServer.MapPath(goConfig("ProjectPath") & "/.htaccess"))
                End If
                'Dim fso As IO.File
                If Not IO.File.Exists(goServer.MapPath(goConfig("ProjectPath") & "/httpd.ini")) Then
                    IO.File.Copy(goServer.MapPath("/ewcommon/setup/rootfiles/httpd.ini"), goServer.MapPath(goConfig("ProjectPath") & "/httpd.ini"))
                End If
            End If

            'delete the setup.ashx file
            If IO.File.Exists(goServer.MapPath(goConfig("ProjectPath") & "/setup.ashx")) Then
                IO.File.Delete(goServer.MapPath(goConfig("ProjectPath") & "/setup.ashx"))
            End If

            'create all standard Eonic Config files
            'disable if exisits to debug
            If IO.File.Exists(goServer.MapPath(goConfig("ProjectPath") & "/web.config")) Then
                IO.File.Delete(goServer.MapPath(goConfig("ProjectPath") & "/web.config"))
            End If

            ' If Not IO.File.Exists(goServer.MapPath(goConfig("ProjectPath") & "/web.config")) Then
            ' TS: Don't check for this we want to overright regardless !

            IO.File.Copy(goServer.MapPath("/ewcommon/setup/rootfiles/web_config.xml"), goServer.MapPath(goConfig("ProjectPath") & "/web.config"))
            ' End If

            If Not IO.File.Exists(goServer.MapPath(goConfig("ProjectPath") & "/eonic.web.config")) Then
                IO.File.Copy(goServer.MapPath("/ewcommon/setup/rootfiles/eonic_web_config.xml"), _
                 goServer.MapPath(goConfig("ProjectPath") & "/eonic.web.config"))
            End If
            If Not IO.File.Exists(goServer.MapPath(goConfig("ProjectPath") & "/eonic.theme.config")) Then
                IO.File.Copy(goServer.MapPath("/ewcommon/setup/rootfiles/eonic_theme_config.xml"), _
                 goServer.MapPath(goConfig("ProjectPath") & "/eonic.theme.config"))
            End If
            If Not IO.File.Exists(goServer.MapPath(goConfig("ProjectPath") & "/eonic.cart.config")) Then
                IO.File.Copy(goServer.MapPath("/ewcommon/setup/rootfiles/eonic_cart_config.xml"), _
                 goServer.MapPath(goConfig("ProjectPath") & "/eonic.cart.config"))
            End If
            If Not IO.File.Exists(goServer.MapPath(goConfig("ProjectPath") & "/eonic.payment.config")) Then
                IO.File.Copy(goServer.MapPath("/ewcommon/setup/rootfiles/eonic_payment_config.xml"), _
                 goServer.MapPath(goConfig("ProjectPath") & "/eonic.payment.config"))
            End If
            If Not IO.File.Exists(goServer.MapPath(goConfig("ProjectPath") & "/eonic.payment.config")) Then
                IO.File.Copy(goServer.MapPath("/ewcommon/setup/rootfiles/eonic_payment_config.xml"), _
                 goServer.MapPath(goConfig("ProjectPath") & "/eonic.payment.config"))
            End If
            If Not IO.File.Exists(goServer.MapPath(goConfig("ProjectPath") & "/eonic.mailinglist.config")) Then
                IO.File.Copy(goServer.MapPath("/ewcommon/setup/rootfiles/eonic_mailinglist_config.xml"), _
                 goServer.MapPath(goConfig("ProjectPath") & "/eonic.mailinglist.config"))
            End If

            'lets create media, images and docs directories
            If Not IO.Directory.Exists(goServer.MapPath(imageRootPath)) Then
                IO.Directory.CreateDirectory(goServer.MapPath(imageRootPath))
            End If
            If Not IO.Directory.Exists(goServer.MapPath(docRootPath)) Then
                IO.Directory.CreateDirectory(goServer.MapPath(docRootPath))
            End If
            If Not IO.Directory.Exists(goServer.MapPath(docMediaPath)) Then
                IO.Directory.CreateDirectory(goServer.MapPath(docMediaPath))
            End If
            'If Not IO.Directory.Exists(goServer.MapPath("/ewThemes")) Then
            '    IO.Directory.CreateDirectory(goServer.MapPath("/ewThemes"))
            'End If
            ''now unzip a standard theme
            'If Not IO.File.Exists(goServer.MapPath(goConfig("ProjectPath") & "/ewThemes/mono.zip")) Then
            '    IO.File.Copy(goServer.MapPath("/ewcommon/setup/rootfiles/ewThemes/mono.zip"), _
            '     goServer.MapPath(goConfig("ProjectPath") & "/ewThemes/mono.zip"))
            'End If

            'If Not IO.File.Exists(goServer.MapPath(goConfig("ProjectPath") & "/ewThemes/mono/standard.xsl")) Then
            '    Try
            '        Dim fz As New ICSharpCode.SharpZipLib.Zip.FastZip
            '        fz.ExtractZip(goServer.MapPath(goConfig("ProjectPath") & "/ewThemes/mono.zip"), goServer.MapPath(goConfig("ProjectPath") & "/ewThemes/"), "")
            '    Catch ex As Exception
            '        'do nothing
            '        errMsg = "Mono Theme did not extract"
            '    End Try
            'End If

            'End If
            'oImp.UndoImpersonation()
            Return True

        Catch ex As Exception
            ' oSetup.AddResponseError(ex)
            errMsg = ex.InnerException.Message
            Return False
        End Try
    End Function


End Class

Public Class ContentMigration

    'class for migrating one content schema to another
    Public oSetup As Setup
    Public myWeb As Eonic.Web


    Public moCtx As System.Web.HttpContext = System.Web.HttpContext.Current

    Public goApp As System.Web.HttpApplicationState = moCtx.Application
    Public goRequest As System.Web.HttpRequest = moCtx.Request
    Public goResponse As System.Web.HttpResponse = moCtx.Response
    Public goSession As System.Web.SessionState.HttpSessionState = moCtx.Session
    Public goServer As System.Web.HttpServerUtility = moCtx.Server
    Public goConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("eonic/web")

    Dim oTransform As New Eonic.XmlHelper.Transform()

    Private cUpdateType As String = ""
    Private cUpdateSchema As String = ""
    Private cUpdateTableName As String = ""
    Private cUpdateKeyColumnName As String = ""
    Private cUpdateSchemaColumnName As String = ""
    Private nUpdateTableType As Web.dbHelper.objectTypes

    Private rCharCheck As Regex = New Regex("[\u0000-\u0008\u000B\u000C\u000E-\u001F]")

    Public Sub New(ByRef oTheSetup As Setup)

        oSetup = oTheSetup

        Try

            myWeb = oSetup.myWeb

            ' Set the type
            If goRequest("upgradetype") <> "" Then
                UpdateType = goRequest("upgradetype")
                goSession("upgradetype") = UpdateType
            ElseIf goSession("upgradetype") <> "" Then
                UpdateType = goSession("upgradetype")
            Else
                ' Default to content
                UpdateType = "Content"
            End If
        Catch ex As Exception
            oSetup.AddResponseError(ex)
        End Try

    End Sub

    Private Property UpdateType() As String
        Get
            Return cUpdateType
        End Get
        Set(ByVal value As String)

            ' Set the core value, and all associated configs based around it.
            ' General Settings
            If value = "Content" Or value = "Directory" Or value = "ContentStructure" Then
                cUpdateType = value
                nUpdateTableType = System.Enum.Parse(GetType(Web.dbHelper.objectTypes), value)
                cUpdateTableName = myWeb.moDbHelper.getTable(nUpdateTableType)
                cUpdateKeyColumnName = myWeb.moDbHelper.getKey(nUpdateTableType)

                ' Type specific settings
                Select Case value

                    Case "Content"

                        cUpdateSchemaColumnName = "cContentSchemaName"

                    Case "Directory"

                        cUpdateSchemaColumnName = "cDirSchema"

                    Case Else

                        cUpdateSchemaColumnName = ""

                End Select

            End If


        End Set
    End Property

    Public Overridable Function GetPage() As Boolean
        'if a form has been submitted, then we process it
        Try
            GetOptions()
            If goRequest.Form.Count > 0 And Not goRequest("upgradetype") <> "" Then
                'get the basic variables
                Dim cContentName, cMoreSQL As String
                Dim bSave As Boolean
                cContentName = goRequest.Form("ContentName")
                cMoreSQL = goRequest.Form("moresql")
                If goRequest.Form("save") = "1" Then bSave = True
                Dim oFullFile As System.Web.HttpPostedFile = goRequest.Files("fullxsl")
                'now we need to read the file without saving it anywhere
                Dim cFullXSLT As String = ""
                Dim nFileLen As Integer
                Dim oFStream As System.IO.Stream
                Dim oByteFile() As Byte
                'if there is a brief file then read it (into bytes)
                If Not oFullFile Is Nothing Then
                    oSetup.AddResponse("Reading Brief File")
                    nFileLen = oFullFile.ContentLength
                    ReDim oByteFile(nFileLen)
                    Dim oBuffer(nFileLen) As Byte
                    oFStream = oFullFile.InputStream
                    oFStream.Read(oBuffer, 0, nFileLen)
                    Dim i As Integer
                    For i = 0 To nFileLen - 1
                        oByteFile(i) = oBuffer(i)
                    Next
                    oFStream.Close()
                    'now make it a string
                    cFullXSLT = ByteToStr(oByteFile)
                End If
                'now make the updates
                DoChange(cContentName, cFullXSLT, bSave, cMoreSQL)
            End If
            Return True
        Catch ex As Exception
            oSetup.AddResponseError(ex)
            'returnException(mcModuleName, "GetPage", ex, "", "", gbDebug)
            Return False
        End Try
    End Function
    Public Overridable Function DoChange(ByVal cContentType As String, ByVal cXSLLocation As String, ByVal bForReal As Boolean, Optional ByVal cAdditionalWhere As String = "") As Boolean
        'input: Content type (cContentSchemaName), location for the brief xsl, location for the detail xsl,bforreal to check wether to save the changes rather than a test run, additional sql where clause to filter, stop on errors or leave the content as is
        Dim oDS As New DataSet
        Dim cSQL As String = ""
        Dim oDR As DataRow
        Dim cProcessInfo As String = "Upgrading content"
        Dim nRowCount As Integer
        Dim bCharCheck As Boolean = False
        Dim bShowXml As Boolean = False
        Dim bAllowLog As Boolean = False
        Try

            cProcessInfo = "Upgrading " & UpdateType

            If goRequest("illegalChars") = "1" Then bCharCheck = True

            If goRequest("showXML") = "1" Then bShowXml = True

            If goRequest("showXML") = "1" And bForReal = False Then
                'setup the sql string
                cSQL = "SELECT TOP 50 * FROM " & Me.cUpdateTableName & " INNER JOIN tblAudit on nauditid = nauditkey "
            Else
                'setup the sql string
                cSQL = "SELECT * FROM " & Me.cUpdateTableName & " INNER JOIN tblAudit on nauditid = nauditkey "
            End If

            If Not (String.IsNullOrEmpty(Me.cUpdateSchemaColumnName)) Then cSQL &= " WHERE (" & Me.cUpdateSchemaColumnName & "= '" & cContentType & "')"

            If Not Trim(cAdditionalWhere) = "" Then
                cSQL &= IIf(cSQL.Contains(" WHERE "), " AND ", " WHERE ") & cAdditionalWhere
            End If

            'get dataset
            oDS = myWeb.moDbHelper.GetDataSet(cSQL, UpdateType, "MigrateContent")
            nRowCount = oDS.Tables(UpdateType).Rows.Count

            Dim cProgressDetail As String = ""
            Dim nProgress As Integer = 0
            Dim nLogId As Integer = 0
            Dim cCurrentId As String = ""

            ' Create a log for tracking
            bAllowLog = myWeb.moDbHelper.checkDBObjectExists("tblActivityLog", Tools.Database.objectTypes.Table)
            If bForReal And bAllowLog Then
                nLogId = oSetup.CommitToLog(Web.dbHelper.ActivityType.SetupDataUpgrade, oSetup.mnUserId, goSession.SessionID, Now(), , , nProgress & "/" & nRowCount)
            End If

            oSetup.AddResponse("Total Record to Process: " & nRowCount)

            'go through each row and update the content
            For Each oDR In oDS.Tables(UpdateType).Rows
                'dont bother if there is no xsl file for the brief
                ' Get the id
                Dim nId As Long = oDR.Item(Me.cUpdateKeyColumnName)
                Dim cResponse As String = myWeb.moDbHelper.getObjectInstance(Me.nUpdateTableType, nId)
                If Me.nUpdateTableType = 4 Then 'Directory
                    Dim oGrpElmt As XmlElement
                    oGrpElmt = myWeb.moDbHelper.getGroupsInstance(nId, 0)
                    cResponse = "<instance>" & cResponse & oGrpElmt.OuterXml & "</instance>"
                Else

                    cResponse = "<instance>" & cResponse & "</instance>"
                End If


                ' DataRowToInstance(oDR, Me.cUpdateTableName)
                If bCharCheck Then
                    Dim oInstance As XmlElement = upgradeContentSchemas(cXSLLocation, cResponse, True)
                Else
                    cCurrentId = oDR(Me.cUpdateKeyColumnName).ToString
                    oSetup.AddResponse("Processing ID: " & cCurrentId)

                    If bShowXml Then
                        oSetup.AddResponse("Original:")
                        oSetup.AddResponse("<code>" & cResponse.Replace("<", "&lt;").Replace(">", "&gt;") & "</code><!--" & cResponse & "-->")
                    End If

                    Dim oInstance As XmlElement = upgradeContentSchemas(cXSLLocation, cResponse)

                    If bShowXml Then
                        oSetup.AddResponse("New:")
                        oSetup.AddResponse("<code>" & oInstance.OuterXml.Replace("<", "&lt;").Replace(">", "&gt;") & "</code><!--" & oInstance.OuterXml & "-->")
                    End If

                    If bForReal Then
                        myWeb.moDbHelper.setObjectInstance(Me.nUpdateTableType, oInstance, CLng(cCurrentId))
                        If CLng(cCurrentId) > 0 Then
                            myWeb.moDbHelper.processInstanceExtras(CLng(cCurrentId), oInstance, False, False)
                        End If
                        nProgress += 1
                        If nProgress = nRowCount Or nProgress Mod 20 = 0 And bAllowLog Then
                            cProgressDetail = nProgress & "/" & nRowCount
                            If nProgress = nRowCount Then cProgressDetail &= ";" & Format(Now(), "yyyy-MM-dd HH:mm:ss")
                            oSetup.UpdateLogDetail(nLogId, cProgressDetail)
                        End If
                    End If
                End If
                If msException <> Nothing Then
                    oSetup.AddResponse("Error Converting Original:")
                    oSetup.AddResponse("<code>" & cResponse.Replace("<", "&lt;").Replace(">", "&gt;") & "</code><!--" & cResponse & "-->")
                    oSetup.AddResponse(msException)
                    Exit For
                End If
            Next
            Return True
        Catch ex As Exception
            oSetup.AddResponseError(ex) ' returnException(mcModuleName, "DoChange", ex, "", cProcessInfo, gbDebug)
            Return False
        End Try
    End Function

    Public Function DataRowToInstance(ByRef oRow As DataRow, ByVal cTableName As String) As String
        Dim oXML As New XmlDocument
        Dim oDC As DataColumn
        Dim oInst As XmlElement
        Try
            oXML.AppendChild(oXML.CreateElement("instance"))
            oInst = oXML.CreateElement(cTableName)
            oXML.DocumentElement.AppendChild(oInst)
            For Each oDC In oRow.Table.Columns
                Dim oElmt As XmlElement = oXML.CreateElement(oDC.ColumnName)
                If Not IsDBNull(oRow(oDC.ColumnName)) Then
                    oElmt.InnerXml = oRow(oDC.ColumnName)
                    oInst.AppendChild(oElmt)
                End If
            Next
            Return oXML.OuterXml
        Catch ex As Exception
            oSetup.AddResponseError(ex)
            Return Nothing
        End Try
    End Function

    Public Function upgradeContentSchemas(ByVal cContentSchema As String, ByVal cContentXml As String, Optional ByVal bCharCheck As Boolean = False) As XmlElement

        Dim sWriter As IO.StringWriter
        Dim sResult As String
        Dim oXmlDr As New XmlDocument
        Dim cResponse As String
        Dim cProcessInfo As String = "error converting type:" & cContentSchema & " data:" & cContentXml
        Dim oXML As New XmlDocument
        Dim oTransform2 As New Eonic.XmlHelper.Transform(myWeb, "", False)
        Try
            'RJP 7 Nov 2012. Modified call EncodeForXML.
            'TS not here this is allready valid XML - this need to be in encrypt
            'cContentXml = Eonic.Tools.Xml.EncodeForXml(cContentXml)
            'cContentXml = Eonic.Tools.Xml.convertEntitiesToCodes(cContentXml)

            oXmlDr.LoadXml(cContentXml.Trim)

            'set exception to nothing
            msException = Nothing

            sWriter = New IO.StringWriter
            oTransform2.Compiled = False
            oTransform2.XSLFileIsPath = False
            oTransform2.XSLFile = cContentSchema.Trim
            oTransform2.Process(oXmlDr, sWriter)
            sResult = sWriter.ToString()

            sWriter = Nothing
            oXmlDr = Nothing

            If oTransform2.transformException Is Nothing Then
                cResponse = Eonic.Tools.Xml.convertEntitiesToCodes(sResult)

                If bCharCheck Then
                    If rCharCheck.Matches(cResponse).Count > 0 Then
                        oSetup.AddResponse("Illegal Characters found:")
                        oSetup.AddResponse(cResponse)
                    End If
                Else
                    ' Get rid of illegal XML chars
                    oXML.InnerXml = rCharCheck.Replace(cResponse, "")
                End If
            Else
                If oTransform2.transformException.InnerException Is Nothing Then
                    oXML.InnerXml = "<Error>" & Eonic.Tools.Xml.convertEntitiesToCodes(oTransform2.transformException.Message) & "</Error>"
                Else
                    oXML.InnerXml = "<Error>" & Eonic.Tools.Xml.convertEntitiesToCodes(oTransform2.transformException.Message) & " " & Eonic.Tools.Xml.convertEntitiesToCodes(oTransform2.transformException.InnerException.Message) & "</Error>"
                End If
                oSetup.AddResponseError(oTransform2.transformException)
            End If

            Return oXML.DocumentElement

        Catch exxml As XmlException
            oSetup.AddResponseError(exxml) ' returnException(mcModuleName, "upgradeContentSchemas", exxml, "", cProcessInfo, gbDebug)
            Return oXML.DocumentElement
        Catch ex As Exception
            oSetup.AddResponseError(ex) 'cError = ex.ToString
            Return oXML.DocumentElement
        End Try

    End Function

    Public Overridable Function GetOptions() As Boolean

        Try
            Dim oContentDetail As XmlElement = oSetup.moPageXml.FirstChild.SelectSingleNode("ContentDetail")
            If oContentDetail Is Nothing Then
                oContentDetail = oSetup.moPageXml.CreateElement("ContentDetail")
                oSetup.moPageXml.FirstChild.AppendChild(oContentDetail)
            End If
            Dim oElmt As XmlElement = oSetup.moPageXml.CreateElement("Content")
            oElmt.SetAttribute("upgradetype", Me.UpdateType)
            oElmt.InnerXml = "<type>Content</type><type>Directory</type><type>ContentStructure</type>"
            oContentDetail.AppendChild(oElmt)
            If Not (String.IsNullOrEmpty(Me.cUpdateSchemaColumnName)) Then
                Dim cOptions As String = ""
                Dim oDR As SqlDataReader = myWeb.moDbHelper.getDataReader("SELECT " & Me.cUpdateSchemaColumnName & " FROM " & Me.cUpdateTableName & " GROUP BY " & Me.cUpdateSchemaColumnName & " ORDER BY " & Me.cUpdateSchemaColumnName)
                Do While oDR.Read
                    cOptions &= "<option>" & oDR.GetValue(0) & "</option>"
                Loop
                oDR.Close()
                oElmt.InnerXml &= Replace(Replace(cOptions, "&gt;", ">"), "&lt;", "<")
            End If


            Return True
        Catch ex As Exception
            oSetup.AddResponseError(ex)
            Return False
            'returnException(mcModuleName, "BuildForm", ex, "", "", gbDebug)
        End Try
    End Function

    Public Function ByteToStr(ByVal oBytes() As Byte) As String
        'returns a string from bytes
        'to represent the file
        oSetup.AddResponse("Converting Bytes to String")

        Dim oChars() As Char 'array of characters

        Dim oDecoder As System.Text.Decoder = System.Text.Encoding.UTF8.GetDecoder() 'a decoder
        Dim cResult As String = "" 'the final string
        Try
            ReDim oChars(oDecoder.GetCharCount(oBytes, 0, UBound(oBytes))) 'make the character array length the same as what will be outputted
            oDecoder.GetChars(oBytes, 0, UBound(oBytes), oChars, 0) 'turn the bytes into chars

            cResult = String.Concat(oChars)

            'For nI = 0 To UBound(oChars) - 1 'read the chars into a string
            '    cResult &= oChars(nI)
            'Next
            Return cResult
        Catch ex As Exception
            oSetup.AddResponseError(ex) 'returnException(mcModuleName, "ByteToStr", ex, "", "ByteToStr", gbDebug)
            Return ""

        End Try
    End Function

End Class

Public Class ContentImport
    Inherits ContentMigration

    Public Sub New(ByVal objSetup As Setup)
        MyBase.New(objSetup)
    End Sub

    Public Overrides Function GetOptions() As Boolean
        Try

            Dim oContentDetail As XmlElement = oSetup.moPageXml.FirstChild.SelectSingleNode("ContentDetail")
            If oContentDetail Is Nothing Then
                oContentDetail = oSetup.moPageXml.CreateElement("ContentDetail")
                oSetup.moPageXml.FirstChild.AppendChild(oContentDetail)
            End If
            Dim oOpt As XmlElement
            Dim oElmt As XmlElement = oSetup.moPageXml.CreateElement("Content")
            oContentDetail.AppendChild(oElmt)

            Dim oDR As SqlDataReader = myWeb.moDbHelper.getDataReader("SELECT cContentSchemaName FROM tblContent GROUP BY cContentSchemaName ORDER BY cContentSchemaName")
            Do While oDR.Read
                oOpt = oSetup.moPageXml.CreateElement("option")
                oOpt.SetAttribute("class", "schema")
                oOpt.InnerText = oDR.GetValue(0)
                oElmt.AppendChild(oOpt)
            Loop
            oDR.Close()
            oDR = myWeb.moDbHelper.getDataReader("SELECT nStructKey, cStructName FROM tblContentStructure ORDER BY cStructName")
            Do While oDR.Read
                oOpt = oSetup.moPageXml.CreateElement("option")
                oOpt.SetAttribute("class", "page")
                oOpt.SetAttribute("value", oDR.GetValue(0))
                oOpt.InnerText = oDR.GetValue(1)
                oElmt.AppendChild(oOpt)
            Loop
            oDR.Close()


            Return True

        Catch ex As Exception
            oSetup.AddResponseError(ex) 'returnException(mcModuleName, "BuildForm", ex, "", "", gbDebug)
            Return False
        End Try
    End Function

    Public Overrides Function GetPage() As Boolean
        'if a form has been submitted, then we process it
        Try
            GetOptions()
            If goRequest.Form.Count > 0 Then
                'get the basic variables
                Dim nContentID As Integer
                Dim cContentType As String
                nContentID = goRequest.Form("ContentId")
                cContentType = goRequest.Form("Contenttype")
                Dim oInstanceXml As System.Web.HttpPostedFile = goRequest.Files("instancexml")
                'now we need to read the file without saving it anywhere
                Dim cIXML As String = ""
                Dim nFileLen As Integer
                Dim oFStream As System.IO.Stream
                Dim oByteFile() As Byte
                'if there is a brief file then read it (into bytes)
                If Not oInstanceXml Is Nothing Then
                    nFileLen = oInstanceXml.ContentLength
                    ReDim oByteFile(nFileLen)
                    Dim oBuffer(nFileLen) As Byte
                    oFStream = oInstanceXml.InputStream
                    oFStream.Read(oBuffer, 0, nFileLen)
                    Dim i As Integer
                    For i = 0 To nFileLen - 1
                        oByteFile(i) = oBuffer(i)
                    Next
                    oFStream.Close()
                    'now make it a string
                    oSetup.AddResponse("Loaded Settings and File")
                    cIXML = ByteToStr(oByteFile)
                    oByteFile = Nothing

                End If

                'now make the updates
                Return DoChange(nContentID, cContentType, cIXML)
            Else
                Return True
            End If

        Catch ex As Exception
            oSetup.AddResponseError(ex) '  returnException(mcModuleName, "GetPage", ex, "", "", gbDebug)
            Return False
        End Try
    End Function

    Public Shadows Function DoChange(ByVal nContentID As Integer, ByVal cContentType As String, ByVal cContent As String) As Boolean
        Try
            Dim oXForm As xForm

            Dim cXFormPath As String = "/xforms/Content/" & cContentType & ".xml"
            If Not IO.File.Exists(goRequest.MapPath(cXFormPath)) Then
                cXFormPath = "/ewcommon" & cXFormPath
            End If


            Dim oInstances As New XmlDocument
            oInstances.PreserveWhitespace = False

            cContent = Replace(cContent, Chr(13), "") 'Remove carriage return
            cContent = Replace(cContent, Chr(10), "") 'remove tab
            cContent = Replace(cContent, "  ", " ") 'remove double space
            cContent = Replace(cContent, "  ", " ") 'remove double space
            cContent = Replace(cContent, "  ", " ") 'remove double space
            cContent = Replace(cContent, "  ", " ") 'remove double space
            cContent = Replace(cContent, "  ", " ") 'remove double space
            cContent = Replace(cContent, "  ", " ") 'remove double space
            cContent = Replace(cContent, "> <", "><") 'clean xml
            oInstances.InnerXml = Trim(cContent)
            oInstances.PreserveWhitespace = False
            Dim oElmt As XmlElement

            For Each oElmt In oInstances.DocumentElement.SelectNodes("instance/tblContent")
                oXForm = New xForm
                oXForm.load(cXFormPath)
                oXForm.LoadInstance(oElmt)
                Dim nNewContentId As Integer = 0
                nNewContentId = myWeb.moDbHelper.setObjectInstance(Web.dbHelper.objectTypes.Content, oXForm.Instance)
                myWeb.moDbHelper.setContentLocation(nContentID, nNewContentId, True, False)
                oSetup.AddResponse("Imported Content, New ID: " & nNewContentId)
            Next
            oSetup.AddResponse("Complete")
            Return True
        Catch ex As Exception
            oSetup.AddResponseError(ex)
            Return False ' returnException(mcModuleName, "GetPage", ex, "", "", gbDebug)
        End Try
    End Function

End Class

