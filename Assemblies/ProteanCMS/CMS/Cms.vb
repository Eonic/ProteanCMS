Option Strict Off
Option Explicit On

Imports System.Xml
Imports System.Web.HttpUtility
Imports System.Web.Configuration
Imports System.IO
Imports System.Collections
Imports System.Threading
Imports System.Data
Imports System.Data.SqlClient
Imports System.Reflection
Imports System.Net
Imports System.Text.RegularExpressions
Imports System.Collections.Specialized
Imports VB = Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic



Public Class Cms
    Inherits Base
    Implements IDisposable

#Region "ErrorHandling"

    'for anything controlling web
    Public Shadows Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)

    Protected Overrides Sub OnComponentError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs) Handles moDbHelper.OnError, oSync.OnError, moCalendar.OnError
        'deals with the error
        returnException(e.ModuleName, e.ProcedureName, e.Exception, mcEwSiteXsl, e.AddtionalInformation, gbDebug)
        'close connection poolinguseralerts
        If Not moDbHelper Is Nothing Then
            Try
                moDbHelper.CloseConnection()
            Catch ex As Exception

            End Try
        End If
        'then raises a public event
        RaiseEvent OnError(sender, e)
    End Sub

#End Region

#Region "Declarations"
    Private moLicenceMode As licenceMode = licenceMode.Live

    Private mbSystemPage As Boolean = False
    Private mnUserPagePermission As dbHelper.PermissionLevel = dbHelper.PermissionLevel.Open

    Public mbAdminMode As Boolean = False
    Public mbPopupMode As Boolean = False

    Public moPageXml As New XmlDocument
    Private moXmlAddedContent As Xml.XmlDocument
    Public mdPageExpireDate As Date = DateAdd(DateInterval.Year, 1, Now)
    Public mdPageUpdateDate As Date = DateAdd(DateInterval.Year, -1, Now)
    Private mnPageCacheMode As Integer
    Public moContentDetail As Xml.XmlElement
    Public mcContentType As String = Mime.MediaTypeNames.Text.Html
    Public mcContentDisposition As String = ""
    Public mnProteanCMSError As Long = 0
    ' Public msException As String = ""

    ' Clone Page Info
    Public mnClonePageId As Integer = 0
    Public mbIsClonePage As Boolean = False
    Public mnCloneContextPageId As Integer = 0

    Public mcPageLanguage As String = ""
    Public mcPreferredLanguage As String = ""
    Public mcPageLanguageUrlPrefix As String = ""
    Public mcPageDefaultDomain As String = ""

    ' Quizmaker Time Info.
    Public mdProcessStartTime As Date = Now()
    Public mdDate As Date = Now.Date 'the generic date for selecting content by publish/expiry date

    Public mnMailMenuId As Long = 0
    Public mnSystemPagesId As Long = 0
    'BJR For Indexing
    Public ibIndexMode As Boolean = False
    Public ibIndexRelatedContent As Boolean = False
    Public icPageWriter As StringWriter

    Private gcEwSiteXsl As String

    Public Shared gcMenuContentCountTypes As String
    Public Shared gcMenuContentBriefTypes As String
    Public Shared gcEwBaseUrl As String
    Public Shared gcBlockContentType As String = ""
    Public Shared gbMembership As Boolean = False
    Public Shared gbCart As Boolean = False
    Public Shared gbQuote As Boolean = False
    Public Shared gbReport As Boolean = False
    Public Shared gnTopLevel As Integer = 0
    Public Shared gnNonAuthUsers As Integer = 0
    Public Shared gnAuthUsers As Integer = 0

    Public Shared gbClone As Boolean = False
    Public Shared gbMemberCodes As Boolean = False
    Public gbVersionControl As Boolean = False
    Public Shared gbIPLogging As Boolean = False
    Public Shared gcGenerator As String = ""
    Public Shared gcCodebase As String = ""
    Public Shared gcReferencedAssemblies As String = ""
    Public Shared gbUseLanguageStylesheets As Boolean = False
    Public Shared gcProjectPath As String = ""
    Public Shared gbUserIntegrations As Boolean = False
    Public Shared gbSingleLoginSessionPerUser As Boolean = False
    Public Shared gnSingleLoginSessionTimeout As Int16 = 900
    ' Site cache
    Public Shared gbSiteCacheMode As Boolean = False

    Public sessionRootPageId As Integer = 0

    Public Shared gbCompiledTransform As Boolean = False
    Private Shared moCompliedStyle As Xsl.XslCompiledTransform
    Public gnPageNotFoundId As Long = 0
    Private gnPageAccessDeniedId As Long = 0
    Private gnPageLoginRequiredId As Long = 0
    Private gnPageErrorId As Long = 0
    Private gnResponseCode As Long = 200
    Public msRedirectOnEnd As String = ""
    Public mbRedirectPerm As String = "false"
    Public bRedirectStarted As Boolean = False
    Public mcOriginalURL As String = ""
    Public mcPageURL As String = ""
    Public mbSuppressLastPageOverrides As Boolean = False
    Public mbIgnorePath As Boolean = False
    Public gcLang As String = "en-gb"

    Private mcLOCALEwSiteXsl As String = ""

    Public mcBehaviourNewContentOrder As String = ""
    Public mcBehaviourAddPageCommand As String = ""
    Public mcBehaviourEditPageCommand As String = ""


    Public mcClientCommonFolder As String = ""
    Public mcEWCommonFolder As String = "/ewcommon"
    Public maCommonFolders As String() = {}

    Public Shadows mcModuleName As String = "Protean.Cms"
    Public moCart As Cms.Cart
    Public moDiscount As Cms.Cart.Discount
    Public mbIsUsingHTTPS As Boolean = False
    Public moTransform As Protean.XmlHelper.Transform

    Public moAdmin As Admin
    Protected Friend oEc As Cart ' Used for BuildFeedXML for some reason, perhpas this could use moCart needs testing
    Protected oSrch As Protean.Cms.Search
    Protected Friend moFSHelper As fsHelper

    Private _responses As XmlElement

    Public WithEvents oSync As ExternalSynchronisation
    Public WithEvents moCalendar As Calendar

    Public oXform As Protean.xForm
    Public gnShowRelatedBriefDepth As Integer = 2
    Public bRestartApp As Boolean = False
    Public moResponseType As pageResponseType
    Public bPageCache As Boolean = False
    Public mbSetNoBrowserCache As Boolean = False
    Public mcPageCacheFolder As String = "\ewCache"
    Private mcSessionReferrer As String = Nothing


    Private _workingSetPrivateMemoryCounter As PerformanceCounter

#End Region
#Region "Enums"
    Enum pageResponseType
        Page = 0
        xml = 1
        json = 2
        popup = 3
        ajaxadmin = 4
        mail = 5
        iframe = 6
    End Enum

#End Region

#Region "Constructors"
    Public Sub New()

        Me.New(System.Web.HttpContext.Current)

    End Sub

    Public Sub New(ByVal Context As System.Web.HttpContext)
        MyBase.New(Context)
        Dim sProcessInfo As String = ""
        Try


            moFSHelper = New fsHelper(moCtx)
            InitialiseGlobal()
            If Not moCtx Is Nothing Then
                PerfMon.Log("Web", "New")
            End If

            ' If moDbHelper Is Nothing Then
            ' moDbHelper = GetDbHelper()
            '  End If
            ' Open()
            '
        Catch ex As Exception
            'returnException(mcModuleName, "New", ex, "", sProcessInfo, gbDebug)
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, sProcessInfo))
        End Try
    End Sub

#End Region



    Public Property mcEwSiteXsl() As String
        Set(ByVal mcEwSiteXsl As String)
            mcLOCALEwSiteXsl = mcEwSiteXsl
        End Set
        Get
            If mcLOCALEwSiteXsl = "" Then
                Return gcEwSiteXsl
            Else
                Return mcLOCALEwSiteXsl
            End If
        End Get
    End Property

    ''' <summary>
    ''' Sets or gets the root page id.
    ''' This contains a stop-gap measure to transition the root page from
    ''' an application context (incorrect) to a session context.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property RootPageId() As Integer
        Get
            Return sessionRootPageId
        End Get
        Set(ByVal value As Integer)
            sessionRootPageId = value
            gnTopLevel = value
        End Set
    End Property

    Public Overridable Function GetDbHelper() As Object

        Return New dbHelper(Me)

    End Function

    Private Function CheckLicence(ByVal mode As licenceMode) As Boolean
        Dim sProcessInfo As String = ""
        Try

            Return True

            'Dim EncyrptionKey As String = "1ad54c70-5fa7-44c5-8059-ab68e136cc63"
            'Dim Hostname As String
            'Dim Enc As New Eonic.Encryption
            'Dim ServerIP As String

            'If moRequest Is Nothing Then
            '    Hostname = "127.0.0.1"
            '    ServerIP = "127.0.0.1"
            'Else
            '    Hostname = moRequest.ServerVariables("SERVER_NAME")
            '    ServerIP = moRequest.ServerVariables("LOCAL_ADDR")
            'End If

            'Dim Hash As String = Enc.GenerateMD5Hash(EncyrptionKey & Hostname)

            'If goApp Is Nothing Then
            '    Return True
            'Else
            '    If goApp(Hash) Is Nothing Then

            '        Dim Valid As Boolean = False

            '        Select Case mode
            '            Case licenceMode.Demo, licenceMode.Development
            '                'Runs on local host
            '                If ServerIP = "::1" Or ServerIP = "127.0.0.1" Then
            '                    Valid = True
            '                End If
            '                If Hostname.EndsWith("eonicweb.dev") Or Hostname.EndsWith("ds01.eonic.co.uk") Then
            '                    Valid = True
            '                End If

            '            Case licenceMode.Live
            '                'This is where we put the clever licencing stuff later on.
            '                Valid = True

            '        End Select

            '        If Valid = True Then
            '            goApp(Hash) = True
            '            Return True
            '        Else
            '            goApp(Hash) = Nothing
            '            Return False
            '        End If

            '    Else
            '        Return True
            '    End If
            'End If

        Catch ex As Exception
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "CheckLicence", ex, sProcessInfo))
        End Try

    End Function

    Public Function GetStatus() As XmlDocument
        Dim oDb As New Protean.Tools.Database
        Dim oVConfig As System.Collections.Specialized.NameValueCollection = System.Web.Configuration.WebConfigurationManager.GetWebApplicationSection("protean/versioncontrol")
        Dim oRXML As New XmlDocument
        Dim oResponseElmt As XmlElement = oRXML.CreateElement("Status")
        Dim bResult As Boolean
        Try
            oRXML.AppendChild(oResponseElmt)
            Dim sSql As String
            Dim oElmt As XmlElement = oRXML.CreateElement("Debug")
            oElmt.InnerText = moConfig("debug")
            oResponseElmt.AppendChild(oElmt)

            ''Start URI
            'oElmt = oRXML.CreateElement("AbsoluteURI")
            'oElmt.InnerText = System.Web.HttpContext.Current.Request.Url.AbsoluteUri
            'oResponseElmt.AppendChild(oElmt)
            ''End URI
            Dim oDr As System.Data.SqlClient.SqlDataReader
            oElmt = oRXML.CreateElement("DBVersion")
            Try

                Dim sResult As String = ""
                sSql = "select * from tblSchemaVersion"
                oDr = moDbHelper.getDataReader(sSql)
                If oDr.HasRows Then
                    While oDr.Read
                        sResult = oDr(1) & "." & oDr(2) & "." & oDr(3) & "." & oDr(4)
                    End While
                Else
                    sResult = "nodata"
                End If
                oElmt.InnerText = sResult
            Catch ex As Exception
                If moConfig("VersionNumber") = "" Then
                    oElmt.InnerText = "Pre V4 or no ProteanCMS"
                Else
                    oElmt.InnerText = moConfig("VersionNumber")
                End If
            End Try

            oResponseElmt.AppendChild(oElmt)

            'Get Page Count

            sSql = "select count(cs.nStructKey) from tblContentStructure cs inner join tblAudit a on a.nAuditKey = cs.nAuditId where nStatus = 1 and (cUrl = null or cUrl = '')"
            oDr = moDbHelper.getDataReader(sSql)
            If oDr.HasRows Then
                While oDr.Read
                    oResponseElmt.SetAttribute("activePageCount", oDr(0))
                End While
            End If

            sSql = "select count(nStructKey) from tblContentStructure where cUrl = null or cUrl = ''"
            oDr = moDbHelper.getDataReader(sSql)
            If oDr.HasRows Then
                While oDr.Read
                    oResponseElmt.SetAttribute("totalPageCount", oDr(0))
                End While
            End If

            sSql = "select count(nStructKey) from tblContentStructure"
            oDr = moDbHelper.getDataReader(sSql)
            If oDr.HasRows Then
                While oDr.Read
                    oResponseElmt.SetAttribute("totalPageRedirects", oDr(0) - CInt("0" & oResponseElmt.GetAttribute("totalPageCount")))
                End While
            End If


            'Get Content Count
            sSql = "select count(nContentKey) from tblContent"
            oDr = moDbHelper.getDataReader(sSql)
            If oDr.HasRows Then
                While oDr.Read
                    oResponseElmt.SetAttribute("contentCount", oDr(0))
                End While
            End If
            oDr = Nothing

            'Start DotNetversion
            oElmt = oRXML.CreateElement("DotNetVersion")
            oElmt.InnerText = Environment.Version.ToString()
            oResponseElmt.AppendChild(oElmt)
            'End DotNetversion

            'Start LocalOrCommonBin
            oElmt = oRXML.CreateElement("LocalOrCommonBin")
            If Dir(goServer.MapPath("/default.ashx")) <> "" Then
                oElmt.InnerText = "Local"
            Else
                If Dir(goServer.MapPath("/ewcommon/default.ashx")) <> "" Then
                    oElmt.InnerText = "Common"
                Else
                    oElmt.InnerText = "Error"
                End If
            End If
            oResponseElmt.AppendChild(oElmt)
            'End LocalOrCommonBin


            'Start DLL Version

            oElmt = oRXML.CreateElement("DLLVersion")

            'Dim sCodeBase As String = myWeb.moRequest.ServerVariables("GENERATOR")
            'oElmt.InnerText = sCodeBase

            'oElmt.InnerText = myWeb.Generator().FullName()

            oElmt.InnerText = Protean.Cms.gcGenerator.ToString()

            'Dim CodeGenerator As Assembly = Me.Generator()
            'gcGenerator = CodeGenerator.FullName()

            oResponseElmt.AppendChild(oElmt)

            'End DLL Version


            'Start LatestDBVersion
            oElmt = oRXML.CreateElement("LatestDBVersion")
            Dim LatestDBVersion As XmlTextReader
            LatestDBVersion = New XmlTextReader(goServer.MapPath("/ewcommon/sqlUpdate/DatabaseUpgrade.xml"))
            LatestDBVersion.WhitespaceHandling = WhitespaceHandling.None
            'Disable whitespace so that it doesn't have to read over whitespaces
            LatestDBVersion.Read()
            LatestDBVersion.Read()
            Dim LatestDBversionAttribute As String = LatestDBVersion.GetAttribute("LatestVersion")
            oElmt.InnerText = LatestDBversionAttribute
            oResponseElmt.AppendChild(oElmt)
            'End LatestDBVersion


            'Start EnabledFeatures

            oElmt = oRXML.CreateElement("Membership")
            oElmt.InnerText = moConfig("Membership")
            oResponseElmt.AppendChild(oElmt)

            oElmt = oRXML.CreateElement("Cart")
            oElmt.InnerText = moConfig("Cart")
            oResponseElmt.AppendChild(oElmt)

            oElmt = oRXML.CreateElement("Quote")
            oElmt.InnerText = moConfig("Quote")
            oResponseElmt.AppendChild(oElmt)

            oElmt = oRXML.CreateElement("Report")
            oElmt.InnerText = moConfig("Report")
            oResponseElmt.AppendChild(oElmt)

            oElmt = oRXML.CreateElement("Subscriptions")
            oElmt.InnerText = moConfig("Subscriptions")
            oResponseElmt.AppendChild(oElmt)

            oElmt = oRXML.CreateElement("MailingList")
            oElmt.InnerText = moConfig("MailingList")
            oResponseElmt.AppendChild(oElmt)

            oElmt = oRXML.CreateElement("Search")
            oElmt.InnerText = moConfig("Search")
            oResponseElmt.AppendChild(oElmt)

            'End Enabledfeatures

        Catch ex As System.Exception
            bResult = False
            AddResponse(ex.ToString)
            returnException(mcModuleName, "GetPendingContent", ex, , , gbDebug)
        Finally
            oResponseElmt.SetAttribute("bResult", bResult)
        End Try

        Return oRXML

    End Function


    'Protected Overrides Sub Finalize()ActivityLogging
    '    'PerfMon.Log("Web", "Finalize")
    '    close()
    '    MyBase.Finalize()
    'End Sub

    Public Sub Open()
        PerfMon.Log("Web", "Open")
        Dim sProcessInfo As String = ""
        Dim cCloneContext As String = ""
        Dim rootPageIdFromConfig As String = ""
        Try
            'Open DB Connections
            If moDbHelper Is Nothing Then
                moDbHelper = GetDbHelper()
            End If
            If moDbHelper.DatabaseName = "" Then
                'redirect to setup
                msRedirectOnEnd = "ewcommon/setup/default.ashx"

            Else

                If Not moSession Is Nothing Then
                    If moSession("adminMode") = "true" Then
                        mbAdminMode = True
                        moDbHelper.gbAdminMode = mbAdminMode
                    End If
                End If
                'Get system page ID's for application level
                If goApp("PageNotFoundId") Is Nothing Then
                    goApp("PageNotFoundId") = moDbHelper.getPageIdFromPath("System+Pages/Page+Not+Found", False, False)
                End If
                If goApp("PageAccessDeniedId") Is Nothing Then
                    goApp("PageAccessDeniedId") = moDbHelper.getPageIdFromPath("System+Pages/Access+Denied", False, False)
                End If
                If goApp("PageLoginRequiredId") Is Nothing Then
                    goApp("PageLoginRequiredId") = moDbHelper.getPageIdFromPath("System+Pages/Login+Required", False, False)
                End If
                If goApp("PageErrorId") Is Nothing Then
                    goApp("PageErrorId") = moDbHelper.getPageIdFromPath("System+Pages/Protean+Error", False, False)
                End If

                gnPageNotFoundId = goApp("PageNotFoundId")
                gnPageAccessDeniedId = goApp("PageAccessDeniedId")
                gnPageLoginRequiredId = goApp("PageLoginRequiredId")
                gnPageErrorId = goApp("PageErrorId")

                mcPagePath = CStr(moRequest("path") & "")

                InitialiseJSEngine()

                'Get the User ID
                'if we access base via soap the session is not available
                If Not moSession Is Nothing Then
                    Dim oMembershipProv As New Providers.Membership.BaseProvider(Me, moConfig("MembershipProvider"))
                    mnUserId = oMembershipProv.Activities.GetUserId(Me)
                End If
                'We need the userId placed into dbhelper.
                moDbHelper.mnUserId = mnUserId

                'Initialise the cart
                If gbCart Or gbQuote Then
                    InitialiseCart()
                    'moCart = New Cart(Me)
                    If Not moCart Is Nothing Then
                        moDiscount = New Cart.Discount(Me)
                        moDiscount.mcCurrency = moCart.mcCurrency
                    End If
                End If


                ' Facility to allow the generation bespoke errors.
                If Not (moRequest("ewerror") Is Nothing) Then
                    'If moRequest("ewerror") <> "nodebug" Then gbDebug = True Else gbDebug = False
                    Throw New Exception(LCase("errortest:" & moRequest("ewerror")))
                End If


                'Logon Redirect Facility
                'once you are logged on this becomes the root 
                ' If in Admin, always defer to the AdminRootPageId
                '    unless you are logging off, then you are going to the user site.
                ' If not Admin, then check if we're logged in
                If mbAdminMode Then
                    ' Admin mode
                    Dim ewCmd As String
                    If moSession("ewCmd") = "PreviewOn" And LCase(moRequest("ewCmd")) = "logoff" Then
                        'case to cater for logoff in preview mode
                        ewCmd = "PreviewOn"
                    Else
                        ewCmd = IIf(moRequest("ewCmd") = "", moSession("ewCmd"), moRequest("ewCmd"))
                    End If


                    If CLng("0" & moConfig("AdminRootPageId")) > 0 And LCase(ewCmd) <> "logoff" Then
                            rootPageIdFromConfig = moConfig("AdminRootPageId")

                        ElseIf CLng("0" & moConfig("AuthenticatedRootPageId")) > 0 AndAlso mnUserId > 0 AndAlso Not moDbHelper.checkUserRole("Administrator") Then
                            ' This is to accomodate users in admin who have admin rights revoked and therefore must 
                            ' be sent back to the user site, but also are logged in, thus they need to go to the authenticatedpageroot if it exists.
                            rootPageIdFromConfig = moConfig("AuthenticatedRootPageId")
                        End If
                    Else
                        ' Not admin mode
                        If mnUserId > 0 And CLng("0" & moConfig("AuthenticatedRootPageId")) > 0 Then
                        rootPageIdFromConfig = moConfig("AuthenticatedRootPageId")
                    End If
                End If

                ' Convert the root ID
                If Tools.Number.IsStringNumeric(rootPageIdFromConfig) Then RootPageId = Convert.ToInt32(rootPageIdFromConfig)


                Me.GetRequestLanguage()

                Dim newPageId As Long = 0

                If mnPageId < 1 Then
                    If Not moRequest("pgid") = "" Then

                        'And we still need to check permissions
                        mnPageId = CLng(moRequest("pgid"))
                        'specified pgid takes priority
                        If Not mbAdminMode Then
                            newPageId = moDbHelper.checkPagePermission(mnPageId)
                        Else
                            If Not moDbHelper.checkPageExist(mnPageId) Then
                                'And we still need to check it exists
                                newPageId = gnPageNotFoundId
                                gnResponseCode = 404
                            End If
                        End If
                    Else
                        ' Check for a redirect path
                        If legacyRedirection() Then Exit Sub

                        If Not (moRequest("path") = "" Or mcPagePath = "/") Then
                            'then pathname
                            moDbHelper.getPageAndArticleIdFromPath(mnPageId, mnArtId, mcPagePath)
                            ' mnPageId = moDbHelper.getPageIdFromPath(mcPagePath).ToString
                            If Not mbAdminMode Then
                                newPageId = moDbHelper.checkPagePermission(mnPageId)
                            End If
                        Else
                            'then root
                            mnPageId = RootPageId
                        End If
                    End If
                End If

                'don't return anything but error in admi mode
                If mcPagePath <> "" And mnPageId = RootPageId And mbAdminMode Then
                    'TS Removed because not quite sure what is going on... 
                    ' was causing a problem with editing locations.
                    '    gnResponseCode = 404
                End If

                If newPageId > 0 And newPageId = gnPageLoginRequiredId And mnUserId = 0 Then
                    'we should set an pageId to overide the logon redirect
                    moSession("LogonRedirectId") = mnPageId

                End If


                If newPageId > 0 Then mnPageId = newPageId

                ' Check for cloned pages, and clone contexts
                If gbClone Then
                    mnClonePageId = moDbHelper.getClonePageID(Me.mnPageId)
                    If mnClonePageId > 0 Then mbIsClonePage = True
                    cCloneContext = moRequest("context")
                    If IsNumeric(cCloneContext) AndAlso Convert.ToInt32(cCloneContext) > 0 Then
                        Me.mnCloneContextPageId = Convert.ToInt32(cCloneContext)
                    End If
                End If

                If Not mbAdminMode Then

                    If mnPageId = gnPageNotFoundId _
                        Or mnPageId = gnPageAccessDeniedId _
                        Or mnPageId = gnPageLoginRequiredId _
                        Or mnPageId = gnPageErrorId Then
                        If RootPageId <> mnPageId Then
                            mbSystemPage = True

                            If mnPageId = gnPageAccessDeniedId Or mnPageId = gnPageLoginRequiredId Then
                                'moResponse.StatusCode = 401
                            End If
                            If mnPageId = gnPageNotFoundId Then
                                gnResponseCode = 404
                            End If
                            If mnPageId = gnPageErrorId Then
                                gnResponseCode = 500
                            End If

                        End If
                    End If
                End If

                If mnArtId < 1 Then
                    If Not moRequest("artid") = "" Then
                        mnArtId = Me.GetRequestItemAsInteger("artid", 0)
                    End If
                End If

                If ibIndexMode Then
                    mbAdminMode = False
                    mnUserId = 1
                End If

                ' Version Control: Set a permission state for this page.
                ' If the permissions are great enough, this will allow content other htan just LIVE to be brought in to the page

                If Not moSession Is Nothing Then
                    If gbVersionControl Then
                        mnUserPagePermission = moDbHelper.getPagePermissionLevel(mnPageId)
                    End If
                End If
            End If

        Catch ex As Exception

            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Open", ex, sProcessInfo))

        End Try

    End Sub

    Public Sub Close()
        PerfMon.Log("Base", "Close")
        Dim sProcessInfo As String = ""
        Try

            'if we access base via soap the session is not available
            If Not moSession Is Nothing Then

                Dim oMembershipProv As New Providers.Membership.BaseProvider(Me, moConfig("MembershipProvider"))
                oMembershipProv.Activities.SetUserId(Me)
            End If

            If Not moDbHelper Is Nothing Then
                'moDbHelper.Close()
                moDbHelper = Nothing
            End If

            goApp = Nothing
            moRequest = Nothing
            'we need this for redirect
            'moResponse = Nothing
            'moSession = Nothing
            goServer = Nothing
            moConfig = Nothing

            If Not moTransform Is Nothing And ibIndexMode = False Then
                moTransform.Close()
                moTransform = Nothing
            End If
            If gbCart And Not moCart Is Nothing Then
                moCart.close()
                moCart = Nothing
            End If

            'Dim nMemDif As Integer = Process.GetCurrentProcess.PrivateMemorySize64
            'Dim nProcDif As Integer = Process.GetCurrentProcess.PrivilegedProcessorTime.Milliseconds


        Catch ex As Exception
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Close", ex, sProcessInfo))
        End Try

    End Sub

    Public Sub InitialiseGlobal()

        Dim cProcessInfo As String = ""

        msException = ""

        Dim sProcessInfo As String = ""
        Try
            ' _workingSetPrivateMemoryCounter = New PerformanceCounter("Process", "Working Set - Private", Process.GetCurrentProcess.ProcessName)

            'sProcessInfo = Process.GetCurrentProcess.ProcessName

            ' Set the debug mode
            moConfig = moConfig
            If Not (moConfig("Debug") Is Nothing) Then
                Select Case LCase(moConfig("Debug"))
                    Case "on" : gbDebug = True
                    Case "off" : gbDebug = False
                    Case Else : gbDebug = False
                End Select
            End If
            Dim bSessionLogging As Boolean = False
            If Not moSession Is Nothing Then
                If moSession("Logging") = "On" Then
                    bSessionLogging = True
                End If
            End If

            If moRequest("perfmon") <> "" Or bSessionLogging Then
                If PerfMon Is Nothing Then PerfMon = New PerfLog(moConfig("DatabaseName"))
                If Not moSession Is Nothing Then
                    'only bother if we are not doing a scheduler thingie
                    If moRequest("perfmon") = "on" Then
                        PerfMon.Start()
                    ElseIf moRequest("perfmon") = "off" Then
                        PerfMon.Stop()
                    ElseIf moSession("Logging") = "On" Then
                        PerfMon.Start()
                    End If
                End If
            End If


            If moRequest("ewCmd") = "admin" Then
                If Not moSession Is Nothing Then moSession("adminMode") = "true"
            End If


            RootPageId = GetConfigItemAsInteger("RootPageId", 0)
            gnAuthUsers = GetConfigItemAsInteger("AuthenticatedUsersGroupId", 0)
            gnNonAuthUsers = GetConfigItemAsInteger("NonAuthenticatedUsersGroupId", 0)

            If CStr(moConfig("BehaviourNewContentOrder")) <> "" Then mcBehaviourNewContentOrder = moConfig("BehaviourNewContentOrder")
            If CStr(moConfig("BehaviourAddPageCommand")) <> "" Then mcBehaviourAddPageCommand = moConfig("BehaviourAddPageCommand")
            If CStr(moConfig("BehaviourEditPageCommand")) <> "" Then mcBehaviourEditPageCommand = moConfig("BehaviourEditPageCommand")

            If moConfig("Language") <> "" Then
                gcLang = moConfig("Language")
            End If

            If moConfig("CompliedTransform") = "on" Or moConfig("CompiledTransform") = "on" Then
                gbCompiledTransform = True
            Else
                gbCompiledTransform = False
            End If

            If LCase(moConfig("Membership")) = "on" Then
                gbMembership = True
            End If

            If LCase(moConfig("Cart")) = "on" Then
                gbCart = True
            End If

            If LCase(moConfig("Quote")) = "on" Then
                gbQuote = True
            End If

            If LCase(moConfig("Report")) = "on" Then
                gbReport = True
            End If

            If LCase(moConfig("SiteCache")) = "on" Then
                gbSiteCacheMode = True
            End If

            If LCase(moConfig("Clone")) = "on" Then
                gbClone = True
            End If

            gbMemberCodes = LCase(moConfig("MemberCodes")) = "on"
            gbVersionControl = LCase(moConfig("VersionControl")) = "on"
            gbIPLogging = LCase(moConfig("IPLogging")) = "on"
            gbUseLanguageStylesheets = LCase(moConfig("LanguageStylesheets")) = "on"
            gcProjectPath = IIf(moConfig("ProjectPath") Is Nothing, "", CStr(moConfig("ProjectPath") & ""))
            gbUserIntegrations = LCase(moConfig("UserIntegrations")) = "on"
            gbSingleLoginSessionPerUser = LCase(moConfig("SingleLoginSessionPerUser")) = "on"
            If moConfig("SingleLoginSessionTimeout") <> "" Then
                gnSingleLoginSessionTimeout = Tools.Number.ConvertStringToIntegerWithFallback("0" & moConfig("SingleLoginSessionTimeout"), gnSingleLoginSessionTimeout)
            End If
            gcMenuContentCountTypes = moConfig("MenuContentCountTypes")
            gcMenuContentBriefTypes = moConfig("MenuContentBriefTypes")



            ' Get referenced assembly info
            ' Given that assemblies are loaded at an application level, we can store the info we find in an application object
            If String.IsNullOrEmpty(CStr(goCache("GENERATOR"))) Then
                Dim CodeGenerator As Assembly = Me.Generator()
                gcGenerator = CodeGenerator.FullName()
                gcCodebase = CodeGenerator.CodeBase()

                For Each ReferencedAssembly As AssemblyName In Me.ReferencedAssemblies()
                    gcReferencedAssemblies &= ReferencedAssembly.Name & " (" & ReferencedAssembly.Version.ToString & "); "
                Next

                goCache("GENERATOR") = gcGenerator
                goCache("CODEBASE") = gcCodebase
                goCache("REFERENCED_ASSEMBLIES") = gcReferencedAssemblies

            Else
                gcGenerator = goCache("GENERATOR")
                gcCodebase = goCache("CODEBASE")
                gcReferencedAssemblies = goCache("REFERENCED_ASSEMBLIES")
            End If
            If moConfig("ShowRelatedBriefDepth") <> "" Then
                gnShowRelatedBriefDepth = moConfig("ShowRelatedBriefDepth")
            End If


        Catch ex As Exception
            'returnException("GlobalObjects", "open", ex, "", sProcessInfo, gbDebug)
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "InitialiseGlobal", ex, sProcessInfo))
        End Try

    End Sub

    Public Overridable Sub InitializeVariables()
        PerfMon.Log("Web", "InitializeVariables")
        'Author:        Trevor Spink
        'Copyright:     Eonic Ltd 2005
        'Date:          2005-03-09

        mcModuleName = "Protean.Cms"

        Dim cProcessInfo As String = ""

        msException = ""

        Try
            cProcessInfo = "set session variables"

            ' Set the rewrite URL
            ' Check both IIS7 URLRewrite ad ISAPI Rewrite variants
            If Not (String.IsNullOrEmpty(CStr("" & moRequest.ServerVariables("HTTP_X_ORIGINAL_URL")))) Then
                mcOriginalURL = moRequest.ServerVariables("HTTP_X_ORIGINAL_URL")
            ElseIf Not (String.IsNullOrEmpty(CStr("" & moRequest.ServerVariables("HTTP_X_REWRITE_URL")))) Then
                mcOriginalURL = moRequest.ServerVariables("HTTP_X_REWRITE_URL")
            End If

            Dim moCartConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/cart")

            If moRequest.ContentType <> "" Then
                mcContentType = moRequest.ContentType
            End If

            Dim ContentType As String = LCase(moRequest("contentType"))

            If mcOriginalURL.StartsWith("/ewapi/") Then
                ContentType = "json"
            End If

            Select Case ContentType
                Case "xml"
                    mcContentType = "application/xml"
                    mbOutputXml = True
                    moResponseType = pageResponseType.xml

                    If moConfig("XmlAllowedIPList") <> "" Then
                        If Not (Tools.Text.IsIPAddressInList(moRequest.ServerVariables("REMOTE_ADDR"), moConfig("XmlAllowedIPList"))) Then
                            mcContentType = "text/html"
                            gcEwSiteXsl = moConfig("SiteXsl")
                            mbOutputXml = False
                            moResponseType = pageResponseType.Page
                        End If
                    End If
                Case "json"
                    mcContentType = "application/json"
                    mbOutputXml = True
                    moResponseType = pageResponseType.json
                Case "popup"
                    If IO.File.Exists(goServer.MapPath("/xsl/admin/AdminPopups.xsl")) Then
                        mcEwSiteXsl = "/xsl/admin/AdminPopups.xsl"
                    Else
                        mcEwSiteXsl = "/ewcommon/xsl/admin/AdminPopups.xsl"
                    End If
                    mbPopupMode = True
                    mbAdminMode = True
                    moResponseType = pageResponseType.popup
                Case "ajaxadmin"
                    mcEwSiteXsl = "/ewcommon/xsl/admin/ajaxAdmin.xsl"
                    moResponseType = pageResponseType.ajaxadmin
                   ' oEw.GetAjaxHTML("MenuNode")
                Case "mail", "email"
                    Dim moMailConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/mailinglist")
                    If moMailConfig("MailingXsl") <> "" Then
                        gcEwSiteXsl = moMailConfig("MailingXsl")
                    Else
                        gcEwSiteXsl = "/ewcommon/xsl/Mailer/mailerStandard.xsl"
                    End If
                    mnMailMenuId = moMailConfig("RootPageId")
                    mcContentType = "text/html"
                    moResponseType = pageResponseType.mail
                Case Else
                    mcContentType = "text/html"
                    gcEwSiteXsl = moConfig("SiteXsl")
                    moResponseType = pageResponseType.Page
                    'can we get a cached page
                    If Not moRequest.ServerVariables("HTTP_X_ORIGINAL_URL") Is Nothing Then
                        If gnResponseCode = 200 And moRequest.Form.Count = 0 And mnUserId = 0 And Not (moRequest.ServerVariables("HTTP_X_ORIGINAL_URL").Contains("?")) Then
                            bPageCache = IIf(LCase(moConfig("PageCache")) = "on", True, False)
                        End If
                        If Not moRequest("reBundle") Is Nothing Then
                            bPageCache = True
                        End If
                    End If
            End Select


            If Not moSession Is Nothing Then
                'this simply gets the userId earlier if it is in the session.
                'behaviour to check single session or transfer from the cart is still called from Open()
                Dim oMembershipProv As New Providers.Membership.BaseProvider(Me, moConfig("MembershipProvider"))
                mnUserId = oMembershipProv.Activities.GetUserSessionId(Me)
                If mnUserId > 0 Then
                    bPageCache = False
                End If
            End If


            mnArtId = mnArtId
            mnPageId = mnPageId
            mnUserId = mnUserId

            mbIsUsingHTTPS = (moRequest.ServerVariables("HTTPS") = "on")

            ' If the site refers to itself for HTTPS and the domain is different then 
            ' BaseURL needs to be the secure site
            If gbCart _
                AndAlso mbIsUsingHTTPS _
                AndAlso moConfig("OverrideBaseUrlWithSecureSiteForHTTPS") = "on" _
                AndAlso moCartConfig("SecureURL") <> "" Then
                gcEwBaseUrl = moCartConfig("SecureURL")
            Else
                gcEwBaseUrl = moConfig("BaseUrl")
            End If

            ' Language sites.
            ' If LanguageStyleSheets is on, then we look for the xsl with the language as a suffix
            ' If it exists, we load that in as the stylesheet.
            If gbUseLanguageStylesheets Then
                Try
                    Dim langStyleFile As String = mcEwSiteXsl.Replace(".xsl", "-" & gcLang.Replace(":"c, "-"c) & ".xsl")
                    If File.Exists(CStr(goServer.MapPath(langStyleFile))) Then
                        mcEwSiteXsl = langStyleFile
                    End If
                Catch ex As Exception
                    ' Do nothing, but don't fall over
                End Try
            End If



            Dim commonfolders As New ArrayList

            mcClientCommonFolder = IIf(moConfig("ClientCommonFolder") Is Nothing, "", CStr(moConfig("ClientCommonFolder") & ""))

            If Not String.IsNullOrEmpty(gcProjectPath) Then commonfolders.Add(gcProjectPath)
            If Not String.IsNullOrEmpty(mcClientCommonFolder) Then commonfolders.Add(mcClientCommonFolder)
            If Not String.IsNullOrEmpty(mcEWCommonFolder) Then commonfolders.Add(mcEWCommonFolder)

            maCommonFolders = commonfolders.ToArray(GetType(String))

            ' force an error for testing error function

        Catch ex As Exception
            'returnException(mcModuleName, "InitializeVariables", ex, gcEwSiteXsl, cProcessInfo, gbDebug)
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "InitializeVariables", ex, ""))
        End Try

    End Sub

    Public Overridable Sub GetPageHTML()
        PerfMon.Log("Web", "GetPageHTML")
        Dim sProcessInfo As String = ""
        Dim sCachePath As String = ""
        Dim sServeFile As String = ""
        Try
            Select Case moResponseType
                Case pageResponseType.ajaxadmin
                    'TS 21-06-2017 Moved from New() as not required for cached pages I think.
                    Open()
                    GetAjaxHTML("MenuNode")

                Case pageResponseType.json

                    Dim moApi As New Protean.API()
                    moApi.InitialiseVariables()
                    moApi.JSONRequest()

                Case Else


                    If gbCart Or gbQuote Then
                        If CInt("0" + moSession("CartId")) > 0 Then
                            bPageCache = False
                        End If
                    End If

                    If bPageCache And Not ibIndexMode And Not gnResponseCode = 404 Then

                        If Not moRequest("reBundle") Is Nothing Then
                            ClearPageCache()
                        End If
                        sCachePath = goServer.UrlDecode(mcOriginalURL)
                        If sCachePath.Contains("?") Then
                            sCachePath = sCachePath.Substring(0, sCachePath.IndexOf("?"))
                        End If
                        sCachePath = sCachePath & ".html"
                        If gcProjectPath <> "" Then
                            sCachePath = sCachePath.Replace(gcProjectPath, "")
                        End If


                        If sCachePath = "/.html" Or sCachePath = ".html" Then
                            sCachePath = "/home.html"
                        End If

                        Dim nCacheTimeout As Long = 24
                        If IsNumeric(moConfig("PageCacheTimeout")) Then
                            nCacheTimeout = moConfig("PageCacheTimeout")
                        End If
                        Dim oFS As New Protean.fsHelper(moCtx)
                        oFS.mcRoot = gcProjectPath
                        oFS.mcStartFolder = goServer.MapPath("\" & gcProjectPath).TrimEnd("\") & mcPageCacheFolder
                        If oFS.VirtualFileExistsAndRecent(sCachePath, nCacheTimeout) Then
                            sServeFile = mcPageCacheFolder & sCachePath
                        End If
                    End If

                    moResponse.HeaderEncoding = System.Text.Encoding.UTF8
                    moResponse.ContentEncoding = System.Text.Encoding.UTF8
                    moResponse.Expires = 0
                    moResponse.AppendHeader("Generator", gcGenerator)

                    If sServeFile = "" Then

                        'TS 21-06-2017 Moved from New() as not required for cached pages I think.
                        Open()

                        If mbAdminMode And Not ibIndexMode And Not gnResponseCode = 404 Then
                            bPageCache = False
                        End If

                        sProcessInfo = "Transform PageXML Using XSLT"
                        If mbAdminMode And Not ibIndexMode And Not gnResponseCode = 404 Then
                            sProcessInfo = "In Admin Mode"
                            If moAdmin Is Nothing Then moAdmin = New Admin(Me)
                            'Dim oAdmin As Admin = New Admin(Me)
                            'Dim oAdmin As Protean.Cms.Admin = New Protean.Cms.Admin(Me)
                            moAdmin.open(moPageXml)
                            moAdmin.adminProcess(Me)
                            moAdmin.close()
                            moAdmin = Nothing
                        Else
                            If moPageXml.OuterXml = "" Then
                                sProcessInfo = "Getting Page XML"
                                GetPageXML()


                            End If
                        End If

                        If Not msException = "" Then
                            'If there is an error we can add our own header.
                            'this means external programs can check that there is an error
                            moResponse.AddHeader("X-ProteanCMSError", "An Error has occured")
                            gnResponseCode = 500
                            moResponse.ContentType = "text/html"
                            bPageCache = False
                        Else
                            ' Set the Content Type
                            moResponse.ContentType = mcContentType
                            ' Set the Content Disposition
                            If Not String.IsNullOrEmpty(mcContentDisposition) Then
                                moResponse.AddHeader("Content-Disposition", mcContentDisposition)
                            End If

                            'ONLY CACHE html PAGES
                            If mcContentType <> "text/html" And Not String.IsNullOrEmpty(mcContentDisposition) Then
                                bPageCache = False
                            End If

                        End If

                        If bPageCache = False Then
                            sServeFile = ""
                        End If

                        If msRedirectOnEnd <> "" Then
                            moPageXml = Nothing
                            Close()
                        Else
                            ' Check if the XML Output has an optional IP restriction placed against it.
                            If mbOutputXml Then
                                If moConfig("XmlAllowedIPList") <> "" Then
                                    If Not (Tools.Text.IsIPAddressInList(moRequest.ServerVariables("REMOTE_ADDR"), moConfig("XmlAllowedIPList"))) Then mbOutputXml = False
                                End If
                            End If
                            If mbOutputXml = True Then
                                Select Case LCase(mcContentType)
                                    Case "application/xml"
                                        moResponse.Write("<?xml version=""1.0"" encoding=""UTF-8""?>" & moPageXml.OuterXml)
                                    Case "application/json"
                                        moResponse.Write(Newtonsoft.Json.JsonConvert.SerializeXmlNode(moPageXml.DocumentElement, Newtonsoft.Json.Formatting.None))
                                End Select
                            Else

                                PerfMon.Log("Web", "GetPageHTML-loadxsl")
                                Dim styleFile As String
                                If Me.mbAdminMode = True Then
                                    If LCase(moPageXml.DocumentElement.GetAttribute("adminMode")) = "false" Or mbPopupMode = True Or mcContentType <> "text/html" Then
                                        styleFile = CStr(goServer.MapPath(mcEwSiteXsl))
                                    Else
                                        If LCase(moConfig("AdminXsl")) = "common" Then
                                            'uses the default admin xsl
                                            styleFile = CStr(goServer.MapPath("/ewcommon/xsl/admin/page.xsl"))
                                        ElseIf moConfig("AdminXsl") <> "" Then
                                            'uses a specified admin XSL
                                            styleFile = CStr(goServer.MapPath(moConfig("AdminXsl")))
                                        Else
                                            'uses the sites main XSL
                                            styleFile = CStr(goServer.MapPath(mcEwSiteXsl))
                                        End If
                                    End If
                                Else
                                    If moResponseType = pageResponseType.Page Then
                                        If moConfig("xframeoptions") <> "" Then
                                            moResponse.AddHeader("X-Frame-Options", moConfig("xframeoptions"))
                                        Else
                                            moResponse.AddHeader("X-Frame-Options", "DENY")
                                        End If
                                    End If
                                    If mbSetNoBrowserCache Then
                                        moResponse.Cache.SetNoStore()
                                        moResponse.Cache.AppendCacheExtension("no-cache")
                                        moResponse.Expires = 0
                                    End If
                                    styleFile = CStr(goServer.MapPath(mcEwSiteXsl))
                                End If

                                Dim brecompile As Boolean = False

                                If moRequest("recompile") <> "" Then
                                    'add delete xsltc flag to web.config
                                    If moRequest("recompile") = "del" Then
                                        brecompile = True
                                        msRedirectOnEnd = Nothing
                                    Else
                                        msRedirectOnEnd = "/?recompile=del"
                                        bRestartApp = True
                                        Protean.Config.UpdateConfigValue(Me, "protean/web", "CompliedTransform", "rebuild")
                                    End If

                                End If

                                Dim oTransform As New Protean.XmlHelper.Transform(Me, styleFile, gbCompiledTransform, , brecompile)

                                PerfMon.Log("Web", "GetPageHTML-startxsl")
                                If moConfig("XslTimeout") <> "" Then
                                    oTransform.TimeOut = moConfig("XslTimeout")
                                End If
                                oTransform.mbDebug = gbDebug

                                If bPageCache Then

                                    Dim textWriter As New StringWriterWithEncoding(System.Text.Encoding.UTF8)

                                    oTransform.ProcessTimed(moPageXml, textWriter)
                                    'save the page
                                    If Not oTransform.bError Then
                                        '  If bPageCache Then

                                        SavePage(sCachePath, textWriter.ToString())
                                        sServeFile = mcPageCacheFolder & sCachePath

                                        '  Else
                                        '  moResponse.Write(textWriter.ToString())
                                        'End If
                                    Else
                                        moResponse.AddHeader("X-ProteanCMSError", "An Error has occured")
                                        gnResponseCode = 500
                                        moResponse.Write(textWriter.ToString())
                                    End If
                                Else
                                    moResponse.AddHeader("Last-Modified", Protean.Tools.Text.HtmlHeaderDateTime(mdPageUpdateDate) & ",")
                                    oTransform.ProcessTimed(moPageXml, moResponse)
                                End If

                                'moResponse.SuppressContent = False
                                If gnResponseCode <> 200 Then
                                    ' TODO: This is IIS7 specific, needs addressing for IIS6
                                    moResponse.TrySkipIisCustomErrors = True
                                    moResponse.StatusCode = gnResponseCode
                                End If

                                PerfMon.Log("Web", "GetPageHTML-endxsl")
                                oTransform.Close()
                                oTransform = Nothing

                                'we don't need this anymore.
                                If Not ibIndexMode Then
                                    If msRedirectOnEnd = "" Then
                                        PerfMon.Write()
                                        moPageXml = Nothing
                                        If sServeFile = "" Then
                                            Close()
                                        End If
                                    Else
                                        moPageXml = Nothing
                                        If sServeFile = "" Then
                                            Close()
                                        End If
                                    End If
                                Else
                                    moPageXml = New XmlDocument
                                End If
                            End If
                        End If
                    End If

                    If Not moSession Is Nothing Then
                        moSession("previousPage") = mcOriginalURL
                    End If

                    If sServeFile <> "" Then
                        If moConfig("xframeoptions") <> "" Then
                            moResponse.AddHeader("X-Frame-Options", moConfig("xframeoptions"))
                        Else
                            moResponse.AddHeader("X-Frame-Options", "DENY")
                        End If
                        Dim filelen As Int16 = goServer.MapPath("/" & gcProjectPath).Length + sServeFile.Length
                        moResponse.AddHeader("Last-Modified", Protean.Tools.Text.HtmlHeaderDateTime(mdPageUpdateDate))
                        If filelen > 260 Then
                            moResponse.Write(Alphaleonis.Win32.Filesystem.File.ReadAllText(goServer.MapPath("/" & gcProjectPath) & sServeFile))
                        Else
                            moResponse.WriteFile(goServer.MapPath("/" & gcProjectPath) & sServeFile)
                        End If
                        Close()
                    End If

            End Select


        Catch ex As Exception
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetPageHTML", ex, sProcessInfo))
            'returnException(mcModuleName, "getPageHtml", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
            moResponse.Write(msException)
            Me.Finalize()
        Finally
            If msRedirectOnEnd <> "" Then
                If Not msRedirectOnEnd.StartsWith("http") Then
                    msRedirectOnEnd = msRedirectOnEnd.Replace("//", "/")
                End If
                If mbRedirectPerm Then
                    moResponse.RedirectPermanent(msRedirectOnEnd, False)
                Else

                    moResponse.Redirect(msRedirectOnEnd, False)
                    If bRestartApp Then
                        RestartAppPool()
                    End If
                End If
                '  moCtx.ApplicationInstance.CompleteRequest()
            Else
                If bRestartApp Then
                    moResponse.Redirect("/", False)
                    RestartAppPool()
                End If
            End If
        End Try

    End Sub

    Public Overridable Function GetPageXML() As XmlDocument
        Dim sProcessInfo As String = "PerfMon"
        PerfMon.Log("Web", "GetPageXML")

        Try
            If Not ibIndexMode Then
                AddCurrency()
            End If

            sProcessInfo = "BuildPageXML"
            BuildPageXML()

            Dim layoutCmd As String = ""
            If Not moSession Is Nothing And Not ibIndexMode Then
                If (mnUserId <> "0" Or LCase(moConfig("LogAll")) = "On") And mbAdminMode = False And Features.ContainsKey("ActivityReporting") Then
                    If moRequest("noFrames") <> "True" Then ' Fix for frameset double counting                   
                        'moDbHelper.logActivity(dbHelper.ActivityType.PageAccess, mnUserId, mnPageId, mnArtId)
                        moDbHelper.CommitLogToDB(dbHelper.ActivityType.PageViewed, mnUserId, moSession.SessionID, Now, mnPageId, mnArtId, moRequest.ServerVariables("REMOTE_ADDR") & " " & moRequest.ServerVariables("HTTP_USER_AGENT"))
                    End If
                End If
            End If

            sProcessInfo = "Check Index Mode"
            If Not ibIndexMode Then

                sProcessInfo = "Check Membership"
                If gbMembership Then
                    MembershipProcess()
                ElseIf mbAdminMode And mnUserId > 0 Then
                    RefreshUserXML()
                End If

                sProcessInfo = "Check Admin Mode"
                ContentActions()

                If LCase(moConfig("FinalAddBulk")) = "on" Then

                    Dim cShowRelatedBriefDepth As String = moConfig("ShowRelatedBriefDepth") & ""
                    Dim nMaxDepth As Integer = 1
                    If Not (String.IsNullOrEmpty(cShowRelatedBriefDepth)) _
                    AndAlso IsNumeric(cShowRelatedBriefDepth) Then
                        nMaxDepth = CInt(cShowRelatedBriefDepth)
                    End If
                    moDbHelper.addBulkRelatedContent(moPageXml.DocumentElement.SelectSingleNode("Contents"), mdPageUpdateDate, nMaxDepth)

                End If

                CommonActions()


                'TS commented out so Century can perform searches in admin mode
                '  If Not (mbAdminMode) Then
                layoutCmd = LayoutActions()
                '  End If

                AddCart()

                If gbQuote Then
                    sProcessInfo = "Begin Quote"
                    Dim oEq As Protean.Cms.Quote = New Protean.Cms.Quote(Me)
                    oEq.apply()
                    oEq.close()
                    oEq = Nothing
                    sProcessInfo = "End Quote"
                End If

                If LCase(moConfig("Search")) = "On" Then

                    Dim oSearchNode As XmlElement = moPageXml.CreateElement("Search")
                    oSearchNode.SetAttribute("mode", moConfig("SearchMode"))
                    oSearchNode.SetAttribute("contentTypes", moConfig("SearchContentTypes"))
                    moPageXml.DocumentElement.AppendChild(oSearchNode)

                End If

                If mbAdminMode Then
                    Try
                        If moRequest("ewCmd") = "" Then
                            ProcessReports()
                        End If
                    Catch
                        'do nothing
                    End Try

                Else
                    ProcessReports()
                End If

                ' Process the Calendars
                ProcessCalendar()

                If gbVersionControl Then CheckContentVersions()

            End If
            sProcessInfo = "CheckMultiParents"
            Me.CheckMultiParents(moPageXml.DocumentElement, mnPageId)

            ' ProcessContentForLanguage
            ProcessPageXMLForLanguage()

            ' Add the responses
            CommitResponsesToPage()

            If Not moSession Is Nothing Then
                If moSession("RedirectReason") <> "" And Not (bRedirectStarted) Then ' bRegistrationSuccessful is a local variable and is only set before the redirection occurs - hence looking for it being False.
                    ' Add a flag - the XSL can pick this up
                    Me.moPageXml.DocumentElement.SetAttribute("RedirectReason", moSession("RedirectReason"))
                    ' Remove the Registration flag.
                    moSession.Remove("RedirectReason")
                End If
            End If

            GetPageXML = moPageXml

        Catch ex As Exception
            'returnException(mcModuleName, "getPageXML", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetPageXML", ex, sProcessInfo))
            Return Nothing
        End Try

    End Function

    Public Function BuildPageXML() As XmlDocument
        PerfMon.Log("Web", "BuildPageXML")
        Dim oPageElmt As XmlElement
        Dim sProcessInfo As String = ""
        Dim sLayout As String = "Default"

        Try

            If moPageXml.DocumentElement Is Nothing Then
                moPageXml.CreateXmlDeclaration("1.0", "UTF-8", "yes")
                oPageElmt = moPageXml.CreateElement("Page")
                moPageXml.AppendChild(oPageElmt)
                GetRequestVariablesXml(oPageElmt)

                GetSettingsXml(oPageElmt)
            Else
                oPageElmt = moPageXml.DocumentElement
            End If

            If CheckLicence(moLicenceMode) Then

                Me.SetPageLanguage()

                'add the page content
                'TS moved this above setting page attributes as it now sets page id on page versions.
                GetStructureXML("Site")

                If mnMailMenuId > 0 Then
                    GetStructureXML("Newsletter", , mnMailMenuId, True)
                End If
                If mnSystemPagesId > 0 And Not mnSystemPagesId = RootPageId Then
                    GetStructureXML(0, mnSystemPagesId, 0, "SystemPages", True, True, False, True, False, "", "")
                End If

                '
                If gcMenuContentCountTypes <> "" Then
                    Dim contentType As String
                    For Each contentType In Split(gcMenuContentCountTypes, ",")
                        AddContentCount(moPageXml.SelectSingleNode("/Page/Menu"), Trim(contentType))
                    Next
                End If

                If gcMenuContentBriefTypes <> "" Then
                    Dim contentType As String
                    For Each contentType In Split(gcMenuContentBriefTypes, ",")
                        AddContentBrief(moPageXml.SelectSingleNode("/Page/Menu"), Trim(contentType))
                    Next
                End If

                'establish the artid
                If Not moRequest.QueryString.Count = 0 Then
                    If moRequest("artid") <> "" Then
                        mnArtId = moRequest("artid")
                    End If
                End If

                'set the page attributes
                If mnArtId > 0 Then
                    oPageElmt.SetAttribute("artid", mnArtId)
                End If

                oPageElmt.SetAttribute("id", mnPageId)
                If Not moSession Is Nothing Then
                    If CInt("0" & moSession("LogonRedirectId")) > 0 And Not (moSession("LogonRedirectId") = mnPageId) Then
                        oPageElmt.SetAttribute("requestedId", moSession("LogonRedirectId"))
                    End If
                End If

                oPageElmt.SetAttribute("cacheMode", mnPageCacheMode)

                If gcEwBaseUrl <> "" Then
                    oPageElmt.SetAttribute("baseUrl", gcEwBaseUrl)
                End If

                'introduce the layout
                If sLayout = "Default" Then
                    sLayout = moDbHelper.getPageLayout(mnPageId)
                End If
                oPageElmt.SetAttribute("layout", sLayout)
                oPageElmt.SetAttribute("pageExt", moConfig("pageExt"))
                oPageElmt.SetAttribute("cssFramework", moConfig("cssFramework"))

                If mnPageId > 0 Then
                    GetContentXml(oPageElmt)
                    'only get the detail if we are not on a system page and not at root
                    If RootPageId = mnPageId Or Not (mnPageId = gnPageNotFoundId Or
                    mnPageId = gnPageAccessDeniedId Or
                    mnPageId = gnPageLoginRequiredId Or
                    mnPageId = gnPageErrorId) Then

                        If mbPreview And moRequest("verId") <> "" Then
                            moContentDetail = GetContentDetailXml(oPageElmt, , , True, moRequest("verId"))
                        Else
                            If LCase(moConfig("AllowContentDetailAccess")) = "On" Then
                                moContentDetail = GetContentDetailXml(oPageElmt)
                            Else
                                moContentDetail = GetContentDetailXml(oPageElmt, , , True)
                            End If
                        End If



                    End If

                    If LCase(moConfig("CheckDetailPath")) = "on" And mbAdminMode = False And mnArtId > 0 And mcOriginalURL.Contains("-/") Then
                        If Not oPageElmt.SelectSingleNode("ContentDetail/Content/@name") Is Nothing Then
                            Dim cContentDetailName As String = oPageElmt.SelectSingleNode("ContentDetail/Content/@name").InnerText
                            cContentDetailName = Protean.Tools.Text.CleanName(cContentDetailName, False, True)
                            Dim RequestedContentName = Right(mcOriginalURL, mcOriginalURL.Length - InStr(mcOriginalURL, "-/") - 1)
                            If RequestedContentName.contains("?") Then
                                RequestedContentName = RequestedContentName.Substring(0, RequestedContentName.IndexOf("?"))
                                'myQueryString = RequestedContentName.Substring(mcOriginalURL.LastIndexOf("?"))
                            End If

                            If RequestedContentName <> cContentDetailName Then
                                mnPageId = gnPageNotFoundId
                                oPageElmt.RemoveChild(oPageElmt.SelectSingleNode("ContentDetail"))
                                mnProteanCMSError = 1005
                            End If
                        End If
                    End If

                    Me.CheckMultiParents(oPageElmt, mnPageId)
                Else
                    mnProteanCMSError = 1005
                End If
                If mnProteanCMSError > 0 Then
                    GetErrorXml(oPageElmt)
                End If

                '  Me.SetPageLanguage() 'TS not sure why this is being called twice ???

                oPageElmt.SetAttribute("expireDate", Protean.Tools.Xml.XmlDate(mdPageExpireDate))
                oPageElmt.SetAttribute("updateDate", Protean.Tools.Xml.XmlDate(mdPageUpdateDate))
                oPageElmt.SetAttribute("userIntegrations", gbUserIntegrations.ToString.ToLower)
                oPageElmt.SetAttribute("pageViewDate", Protean.Tools.Xml.XmlDate(mdDate))

                ' Assess if this page is a cloned page.
                ' Is it a direct clone (in which case the page id will have a @clone node in the Menu Item
                ' Or is it a child of a cloned page (in which case the page id MenuItem will have a @cloneparent node that matches the requested context, stored in mnCloneContextPageId)
                If gbClone _
                    AndAlso Not (oPageElmt.SelectSingleNode("//MenuItem[@id = /Page/@id And (@clone > 0 Or (@cloneparent='" & Me.mnCloneContextPageId & "' and @cloneparent > 0 ))]") Is Nothing) Then
                        ' If the current page is a cloned page
                        oPageElmt.SetAttribute("clone", "true")
                End If
            Else
                'Invalid Licence
                mnProteanCMSError = 1008
                If mnProteanCMSError > 0 Then
                    GetErrorXml(oPageElmt)
                End If

            End If

            Return moPageXml

        Catch ex As Exception

            'returnException(mcModuleName, "buildPageXML", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "BuildPageXML", ex, sProcessInfo))
            Return Nothing
        End Try

    End Function

    ''' <summary>
    '''   Generates feeds for page content.
    ''' </summary>
    ''' <param name="contentSchema">The content schema to search for</param>
    ''' <param name="showRelated">Add related content to the content retrieved</param>
    ''' <param name="pageId">The page ID to search for. If 0, then search all available pages.</param>
    ''' <param name="includeChildPages">If a page ID is specified, then this determines whether or not to go down the site tree as well</param>
    ''' <param name="mimeType">Optional: The mimeType to output in the HTTP response ContentType Header (default is "text/xml")</param>
    ''' <param name="feedName">Optional: A name to determine the folder to look in for the feed XSL - default is "generic"</param>
    ''' <remarks>
    ''' <list>
    ''' <listheader>The XSL file name is predetermined by the options that are passed through and have the following precedence:</listheader>
    ''' <item>/xsl/feeds/&lt;feedname&gt;/&lt;contentSchema&gt;-&lt;mimeType&gt;.xsl  (then check common folders)</item>
    ''' <item>/xsl/feeds/generic/&lt;contentSchema&gt;-&lt;mimeType&gt;.xsl  (then check common folders)</item>
    ''' <item>/xsl/feeds/&lt;feedname&gt;/&lt;contentSchema&gt;.xsl  (then check common folders)</item>
    ''' <item>/xsl/feeds/generic/&lt;contentSchema&gt;.xsl  (then check common folders)</item>
    ''' </list>
    ''' </remarks>
    Public Overridable Sub GetFeedXML(ByVal contentSchema As String, ByVal showRelated As Boolean, ByVal pageId As Integer, ByVal includeChildPages As Boolean, Optional ByVal mimeType As String = Mime.MediaTypeNames.Text.Xml, Optional ByVal feedName As String = "generic", Optional ByVal bContentDetail As Boolean = False, Optional ByVal cRelatedSchemasToShow As String = "", Optional ByVal nGroupId As Int32 = 0)
        Dim methodName As String = "GetFeedXML(string,boolean,integer,boolean,string,string)"
        Dim processInfo As String = ""
        PerfMon.Log("Web", methodName)

        Try
            'TS 21-06-2017 Moved from New() as not required for cached pages I think.
            Open()
            ' Determine and set the mimeType
            ' AJG - application/xhtml+xml is interpretly correctly by many browsers, especially IE
            ' Default Content type returned to be XHTML (to clear warning on XHTML validator)
            processInfo = "Determine the mimeType"
            mimeType = IIf(ValidatedOutputXml() Or String.IsNullOrEmpty(mimeType), Mime.MediaTypeNames.Text.Xml, Replace(mimeType, " ", "+"))
            mcContentType = mimeType
            moResponse.ContentType = mcContentType

            ' Generate the XML
            If moPageXml.OuterXml = "" Then
                processInfo = "Generate the XML"


                ' If page is not 0 and the contentSchema is all then we need to generate the page XML as normal.
                If pageId > 0 And contentSchema.ToLower() = "all" Then
                    mnPageId = pageId
                    BuildPageXML()
                Else
                    BuildFeedXML(contentSchema, showRelated, pageId, includeChildPages, bContentDetail, cRelatedSchemasToShow, nGroupId)
                End If
            End If

            ' Add the responses
            CommitResponsesToPage()

            ' Set the page encoding
            moResponse.HeaderEncoding = System.Text.Encoding.UTF8
            moResponse.ContentEncoding = System.Text.Encoding.UTF8

            ' Set error header if errors were encountered building the XML
            If Not msException = "" Then
                'If there is an error we can add our own header.
                'this means external programs can check that there is an error
                moResponse.AddHeader("X-ProteanCMSError", "An Error has occured")
            End If

            ' AJG - Commenting out this, as building feed xml shouldn't include any content processing, 
            ' which is how redirects are normally generated.
            'If msRedirectOnEnd <> "" Then
            '    moResponse.Redirect(msRedirectOnEnd)
            '    Exit Sub
            'End If


            If ValidatedOutputXml() Then

                ' Output as XML
                moResponse.Write("<?xml version=""1.0"" encoding=""UTF-8""?>" & moPageXml.OuterXml)
            Else

                ' Load the XSL file

                ' The filename has the following checks and preferences:

                '/xsl/feeds/<feedname>/<contentSchema>-<mimeType>.xsl  (then check common folders)
                '/xsl/feeds/generic/<contentSchema>-<mimeType>.xsl  (then check common folders)
                '/xsl/feeds/<feedname>/<contentSchema>.xsl  (then check common folders)
                '/xsl/feeds/generic/<contentSchema>.xsl  (then check common folders)
                processInfo = "Load the XSL file"


                Dim pathprefix As String = "/xsl/feeds/"
                Dim rootPaths As New ArrayList
                Dim paths As New ArrayList
                Dim mimeTypeFlattened As String = New Regex("[^A-Z0-9]", Text.RegularExpressions.RegexOptions.IgnoreCase).Replace(mimeType, "-")

                ' Determine a default feedName, just in case it's blank
                feedName = IIf(String.IsNullOrEmpty(feedName), "generic", feedName)

                ' Add the paths to an array
                rootPaths.Add(pathprefix & feedName & "/" & contentSchema & "-" & mimeTypeFlattened & ".xsl")
                If feedName <> "generic" Then rootPaths.Add(pathprefix & "generic/" & contentSchema & "-" & mimeTypeFlattened & ".xsl")
                rootPaths.Add(pathprefix & feedName & "/" & contentSchema & ".xsl")
                If feedName <> "generic" Then rootPaths.Add(pathprefix & "generic/" & contentSchema & ".xsl")
                rootPaths.Add(pathprefix & feedName & "/generic.xsl")
                If feedName <> "generic" Then rootPaths.Add(pathprefix & "generic/generic.xsl")

                ' Add the common folders to the paths
                For Each path As String In rootPaths
                    paths.Add(path)
                    For Each commonFolders As String In maCommonFolders
                        paths.Add("/" & commonFolders.Trim("/\") & path)
                    Next
                Next
                rootPaths = Nothing

                ' Now check all the files
                Dim xslFile As String = ""
                For Each path As String In paths
                    ' Security feature: Don't check any paths that have ".."
                    If Not path.Contains("..") Then
                        If File.Exists(goServer.MapPath(path)) Then
                            xslFile = path
                            Exit For
                        End If
                    End If
                Next


                ' Only transform the feed if we have a file
                If Not String.IsNullOrEmpty(xslFile) Then

                    mcEwSiteXsl = xslFile


                    ' Transform PageXML using XSLT
                    processInfo = "Transform PageXML using XSLT"
                    Dim styleFile As String = CStr(goServer.MapPath(mcEwSiteXsl))
                    Dim oTransform As New Protean.XmlHelper.Transform(Me, styleFile, gbCompiledTransform, 120000)
                    PerfMon.Log("Web", "GetFeedXML-startxsl")
                    oTransform.mbDebug = gbDebug
                    oTransform.ProcessTimed(moPageXml, moResponse)
                    PerfMon.Log("Web", "GetFeedXML-endxsl")
                    oTransform.Close()
                    oTransform = Nothing

                Else
                    Throw New System.IO.IOException("Could not find a valid feed file")
                End If


            End If

            moPageXml = Nothing
            Close()

        Catch ex As Exception
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, methodName, ex, processInfo))
            moResponse.Write(msException)
            Me.Finalize()
        End Try
        PerfMon.Write()
    End Sub

    ''' <summary>
    ''' [Deprecated] This gets the feed XML for all available content of a certain schema, 
    ''' using the google folder of feed xsls to transform the data. 
    ''' </summary>
    ''' <param name="contentSchema">The content schema to search for</param>
    ''' <param name="showRelated">Add related content to the content retrieved</param>
    ''' <remarks></remarks>
    Public Overridable Sub GetFeedXML(ByVal contentSchema As String, ByVal showRelated As Boolean, Optional ByVal blnContentDetail As Boolean = False, Optional ByVal cRelatedSchemasToShow As String = "", Optional ByVal nGroupId As Int32 = 0)
        Dim methodName As String = "GetFeedXML(string,boolean)"
        Dim processInfo As String = ""
        PerfMon.Log("Web", methodName)
        Try
            GetFeedXML(contentSchema, showRelated, 0, True, , "google", blnContentDetail, cRelatedSchemasToShow, nGroupId)
        Catch ex As Exception
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, methodName, ex, processInfo))
        End Try
    End Sub

    ''' <summary>
    ''' Gets XML optimised for Feeds
    ''' </summary>
    ''' <param name="contentSchema">The content schema to search for</param>
    ''' <param name="showRelated">Add related content to the content retrieved</param>
    ''' <param name="pageId">The page ID to search for. If 0, then search all available pages.</param>
    ''' <param name="includeChildPages">If a page ID is specified, then this determines whether or not to go down the site tree as well</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function BuildFeedXML(ByVal contentSchema As String, ByVal showRelated As Boolean, ByVal pageId As Integer, ByVal includeChildPages As Boolean, Optional ByVal blnContentDetail As Boolean = False, Optional ByVal cRelatedSchemasToShow As String = "", Optional ByVal nGroupId As Int32 = 0) As XmlDocument
        Dim methodName As String = "BuildFeedXML(string,boolean,integer,boolean)"
        PerfMon.Log("Web", methodName)

        Dim oPageElmt As XmlElement
        Dim processInfo As String = ""
        Dim sLayout As String = "default"
        Dim oElmt As XmlElement

        Try

            If moPageXml.DocumentElement Is Nothing Then
                moPageXml.CreateXmlDeclaration("1.0", "UTF-8", "yes")
                oPageElmt = moPageXml.CreateElement("Page")
                moPageXml.AppendChild(oPageElmt)
                GetRequestVariablesXml(oPageElmt)

                GetSettingsXml(oPageElmt)
            Else
                oPageElmt = moPageXml.DocumentElement
            End If

            oPageElmt.SetAttribute("id", mnPageId)
            oPageElmt.SetAttribute("cacheMode", mnPageCacheMode)

            If gcEwBaseUrl <> "" Then
                oPageElmt.SetAttribute("baseUrl", gcEwBaseUrl)
            End If

            ' Get the site structure, this will determine what can and can't be seen 
            GetStructureXML("Site")

            ' Get the page scope.
            ' Let's set a few rules
            ' 1. If page id = 0 then we assume all pages
            ' 2. If page > 0 then get that page, and its children if includechildpages is set
            ' We can build an xpath on this logic
            Dim pageFinderXPath As String = "descendant-or-self::MenuItem"
            If pageId > 0 Then pageFinderXPath &= "[@id=" & pageId & "]"
            If pageId > 0 And includeChildPages Then pageFinderXPath &= "/descendant-or-self::MenuItem"

            ' Build the page list
            Dim pageIds As String = ""
            For Each oElmt In moPageXml.SelectNodes(pageFinderXPath)
                pageIds &= "'" & oElmt.GetAttribute("id") & "',"
            Next
            pageIds = pageIds.TrimEnd(",")



            ' Only process the XML if we have page ids to work with
            If Not String.IsNullOrEmpty(pageIds) Then

                ' Note, for optimisation, we can call this ***without*** permissions checks,
                ' because we have already determined permissions through the sitestructure call
                ' and we are limiting the results by page id.
                ' Note: This brings back any FeedOutput content that happens to be placed on any of the pages.
                ' This can be used for configuring RSS feeds (see Wellards)
                Dim pageSize As Integer = Nothing
                Dim pageNumber As Integer = Nothing

                If moConfig("LimitFeed") <> "" Then
                    pageSize = moConfig("LimitFeed")
                End If

                If moRequest("pageSize") > 0 Then
                    pageSize = moRequest("pageSize")
                    pageNumber = moRequest("pageNumber")
                    If pageNumber = 0 Then pageNumber = 1
                End If

                If moConfig("feedRelatedBriefDepth") <> "" Then
                    gnShowRelatedBriefDepth = moConfig("feedRelatedBriefDepth")
                End If

                Dim productGrpSql As String = ""
                If nGroupId > 0 Then
                    productGrpSql = " and nContentKey IN (Select nContentId from tblCartCatProductRelations where nCatId = " & nGroupId & ")"
                End If

                Me.GetPageContentFromSelect("cContentSchemaName IN (" & dbHelper.SqlString(contentSchema) & ",'FeedOutput') and CL.nStructId IN(" & pageIds & ")" & productGrpSql, True, , True, pageSize, "dInsertDate DESC", , , blnContentDetail, pageNumber,, cRelatedSchemasToShow)

                Dim ProductTypes As String = moConfig("ProductTypes")
                If ProductTypes = "" Then ProductTypes = "Product,SKU"

                If moConfig("Cart") = "on" Then
                    oEc = New Cart(Me)
                End If

                'NB put the parId code here?
                For Each oElmt In oPageElmt.SelectNodes("/Page/Contents/Content")

                    If CLng("0" & oElmt.GetAttribute("parId")) > 0 Then
                        processInfo = "Cleaning parId for: " & oElmt.OuterXml
                        Dim primaryParId As Long = 0

                        If InStr(oElmt.GetAttribute("parId"), ",") > 0 Then

                            Dim aParIds() As String = oElmt.GetAttribute("parId").ToString.Split(",")

                            For Each cParId As String In aParIds
                                If Not oPageElmt.SelectSingleNode("descendant-or-self::MenuItem[@id='" & cParId & "']") Is Nothing Then
                                    primaryParId = cParId
                                End If
                            Next

                            oElmt.SetAttribute("parId", primaryParId)

                        End If

                    End If

                    If moConfig("Cart") = "on" Then
                        'Get the shipping costs
                        Dim feedShowShippingOptions As String = moConfig("feedShowShippingOptions")
                        If ProductTypes.Contains(oElmt.GetAttribute("type")) And feedShowShippingOptions = "on" Then

                            Dim strfeedWeightXpath As String = moConfig("feedWeightXpath")
                            Dim feedPriceXpath As String = moConfig("feedPriceXpath")

                            Dim xmlNodeWeight As XmlNode = oElmt.SelectSingleNode(moConfig("feedWeightXpath"))
                            Dim xmlNodePrice As XmlNode = oElmt.SelectSingleNode(moConfig("feedPriceXpath"))

                            If (Not xmlNodeWeight Is Nothing) And (Not xmlNodeWeight Is Nothing) Then
                                Dim nWeight As String = xmlNodeWeight.InnerText
                                Dim nPrice As String = xmlNodePrice.InnerText
                                If nWeight = "" Then nWeight = "0"
                                If IsNumeric(nWeight) And IsNumeric(nPrice) Then
                                    oEc.AddShippingCosts(oElmt, CDbl(nPrice), CDbl(nWeight))
                                End If
                            End If

                        End If
                    End If


                Next

                If moConfig("Cart") = "on" Then
                    oEc.close()
                    oEc = Nothing
                End If
                ' If showRelated Then
                'DbHelper.addBulkRelatedContent(oPageElmt.SelectSingleNode("/Page/Contents"))

                '    For Each oElmt In oPageElmt.SelectNodes("/Page/Contents/Content")
                '      moDbHelper.addRelatedContent(oElmt, oElmt.GetAttribute("id"), False, cRelatedSchemasToShow)
                'Next
                'End If

            End If

            Me.SetPageLanguage()
            oPageElmt.SetAttribute("expireDate", Protean.Tools.Xml.XmlDate(mdPageExpireDate))
            oPageElmt.SetAttribute("updateDate", Protean.Tools.Xml.XmlDate(mdPageUpdateDate))

            Return moPageXml

        Catch ex As Exception

            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, methodName, ex, processInfo))
            Return Nothing
        End Try

    End Function

    ''' <summary>
    ''' Gets XML optimised for feeds, retrieving all content of a certain schema, plus optionally its related content.
    ''' </summary>
    ''' <param name="contentSchema">The content schema to search for</param>
    ''' <param name="showRelated">Add related content to the content retrieved</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function BuildFeedXML(ByVal contentSchema As String, ByVal showRelated As Boolean) As XmlDocument
        Dim methodName As String = "BuildFeedXML(string,boolean)"
        PerfMon.Log("Web", methodName)

        Try

            Return BuildFeedXML(contentSchema, showRelated, 0, True)

        Catch ex As Exception

            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, methodName, ex, ""))
            Return Nothing
        End Try

    End Function

    Public Sub GetAjaxHTML(Optional ByVal sAjaxCmd As String = "")

        PerfMon.Log("Web", "GetAjaxHTML")
        Dim sProcessInfo As String = ""
        Try
            If sAjaxCmd = "" Then
                sAjaxCmd = moRequest("AjaxCmd")
            End If

            GetAjaxXML(sAjaxCmd)


            Dim moAdmin As Protean.Cms.Admin = New Admin(Me)
            'Dim oAdmin As Admin = New Admin(Me)
            'Dim oAdmin As Protean.Cms.Admin = New Protean.Cms.Admin(Me)
            moAdmin.open(moPageXml)
            If CInt("0" & moSession("nUserId")) > 0 Then
                moAdmin.GetPreviewMenu()
            End If

            moAdmin.close()
            moAdmin = Nothing


            sProcessInfo = "Transform PageXML using XSLT"

            'make sure each requests gets a new one.
            moResponse.Expires = 0

            moResponse.HeaderEncoding = System.Text.Encoding.UTF8
            moResponse.ContentEncoding = System.Text.Encoding.UTF8
            ' Default Content type returned to be XHTML (to clear warning on XHTML validator)
            moResponse.ContentType = "text/html" '"application/xhtml+xml"

            If Not msException = "" Then
                'If there is an error we can add our own header.
                'this means external programs can check that there is an error
                moResponse.AddHeader("X-ProteanCMSError", "An Error has occured")
            End If

            If mbOutputXml = True Then
                moResponse.ContentType = "text/xml"
                moResponse.Write("<?xml version=""1.0"" encoding=""UTF-8""?>" & moPageXml.OuterXml)
            Else

                Dim styleFile As String = CStr(goServer.MapPath(mcEwSiteXsl))

                If moRequest("recompile") <> "" Then
                    goApp.Item(styleFile) = Nothing
                End If

                Dim oTransform As New Protean.XmlHelper.Transform(Me, styleFile, gbCompiledTransform)

                PerfMon.Log("Web", "GetPageHTML-startxsl")
                oTransform.mbDebug = gbDebug
                oTransform.ProcessTimed(moPageXml, moResponse)
                PerfMon.Log("Web", "GetPageHTML-endxsl")
                oTransform.Close()
                oTransform = Nothing

            End If

            moPageXml = Nothing
            Close()

        Catch ex As Exception
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetAjaxHTML", ex, sProcessInfo))
            moResponse.Write(msException)
            Me.Finalize()
        End Try

        PerfMon.Write()

    End Sub

    Public Overridable Function GetAjaxXML(ByVal AjaxCmd As String) As XmlDocument
        PerfMon.Log("Web", "GetAjaxXML")
        Dim oPageElmt As XmlElement
        Dim sProcessInfo As String = ""
        Dim sLayout As String = "default"

        Try

            If moPageXml.DocumentElement Is Nothing Then
                moPageXml.CreateXmlDeclaration("1.0", "UTF-8", "yes")
                oPageElmt = moPageXml.CreateElement("Page")
                moPageXml.AppendChild(oPageElmt)
                GetRequestVariablesXml(oPageElmt)

                GetSettingsXml(oPageElmt)
                Dim userXml As XmlElement = GetUserXML()
                If Not userXml Is Nothing Then
                    moPageXml.DocumentElement.AppendChild(GetUserXML())
                End If

            Else
                oPageElmt = moPageXml.DocumentElement
            End If

            'establish the artid
            If moRequest("artid") <> "" Then
                mnArtId = moRequest("artid")
            End If
            'set the page attributes
            If mnArtId > 0 Then
                oPageElmt.SetAttribute("artid", mnArtId)
            End If
            Dim nPageId As Long = CLng("0" & moRequest("pgid"))
            Dim nContentParId As Long = CLng("0" & moRequest("contentParId"))
            If nPageId > 0 Then
                mnPageId = nPageId
                If nContentParId > 0 Then
                    nPageId = 0
                End If
            End If

            oPageElmt.SetAttribute("id", mnPageId)
            oPageElmt.SetAttribute("contentParId", nContentParId)
            oPageElmt.SetAttribute("cacheMode", mnPageCacheMode)
            oPageElmt.SetAttribute("cssFramework", moConfig("cssFramework"))

            If gcEwBaseUrl <> "" Then
                oPageElmt.SetAttribute("baseUrl", gcEwBaseUrl)
            End If

            Select Case AjaxCmd
                Case "BespokeProvider"
                    'Dim assemblyInstance As [Assembly]
                    Dim calledType As Type
                    Dim moPrvConfig As Protean.ProviderSectionHandler = WebConfigurationManager.GetWebApplicationSection("protean/bespokeProviders")
                    Dim providerName = moRequest("provider")
                    Dim assemblyInstance As [Assembly] = [Assembly].Load(moPrvConfig.Providers(providerName).Type.ToString())

                    Dim classPath As String = moRequest("method")

                    Dim methodName As String = Right(classPath, Len(classPath) - classPath.LastIndexOf(".") - 1)
                    classPath = Left(classPath, classPath.LastIndexOf("."))

                    calledType = assemblyInstance.GetType(classPath, True)
                    Dim o As Object = Activator.CreateInstance(calledType)

                    Dim args(0) As Object
                    args(0) = Me

                    calledType.InvokeMember(methodName, BindingFlags.InvokeMethod, Nothing, o, args)

                Case "Edit", "Delete"

                    'TODO: We Need to confirm Permissions and Right before we allow this !!!!

                    Dim moAdXfm As Admin.AdminXforms = getAdminXform()
                    moAdXfm.open(moPageXml)

                    Dim nContentId As Long = CLng("0" & moRequest("id"))
                    Dim bUserValid As Boolean = False
                    Dim nPagePermissionCheck As Long = nPageId
                    Dim nContentPermissionCheck As Long = nContentId
                    Dim bResetUser As Boolean = False

                    If mbPreview Then
                        mnUserId = moSession("nUserId")
                    End If

                    If mnUserId = 0 Then
                        mnUserId = moConfig("NonAuthUserID")
                        moDbHelper.mnUserId = mnUserId
                        bResetUser = True
                    End If

                    moAdXfm.mnUserId = mnUserId
                    moDbHelper.mnUserId = mnUserId

                    ' Let's check permissions - but first let's accomodate orphan content
                    ' Orhpan content has a page id of 0, but is related to another piece of content.
                    ' We need to track down the ultimate related content primary page id.
                    If nPagePermissionCheck = 0 And nContentParId > 0 Then
                        Dim hUnorphanedAncestor As Hashtable = moDbHelper.getUnorphanedAncestor(nContentParId, 1)
                        If Not (hUnorphanedAncestor Is Nothing) Then
                            nPagePermissionCheck = hUnorphanedAncestor("page")
                            nContentPermissionCheck = hUnorphanedAncestor("id")
                        End If
                    End If

                    Dim nContentPermLevel As dbHelper.PermissionLevel
                    If nContentId > 0 And nContentParId <> 0 Then
                        nContentPermLevel = moDbHelper.getContentPermissionLevel(nContentPermissionCheck, nPagePermissionCheck)
                    Else
                        nContentPermLevel = moDbHelper.getPagePermissionLevel(nPagePermissionCheck)
                    End If

                    ' Check if the permissions are valid
                    bUserValid = dbHelper.CanAddUpdate(nContentPermLevel) And mnUserId > 0

                    ' We need to set this for version control
                    moDbHelper.CurrentPermissionLevel = nContentPermLevel

                    If bUserValid Then

                        Select Case AjaxCmd
                            Case "Edit"
                                Dim xFrmContent As XmlElement
                                xFrmContent = moAdXfm.xFrmEditContent(nContentId, moRequest("type"), nPageId, moRequest("name"), , nContentId, , moRequest("formName"), "0" & moRequest("verId"))
                                If moAdXfm.valid Then
                                    'if we have a parent releationship lets add it
                                    If moRequest("contentParId") <> "" Then
                                        moDbHelper.insertContentRelation(moRequest("contentParId"), nContentId, IIf(moRequest("2way") = "true", True, False))
                                    End If
                                    'simply output the content detail XML
                                    '  As this is content that we must've been able to get,
                                    '  we should be able to see it.
                                    '  However if it's related content the page id may not be coming through (it's orphaned)
                                    '  sooo... we may need to bodge the permissions if version control is on
                                    If gbVersionControl Then Me.mnUserPagePermission = dbHelper.PermissionLevel.AddUpdateOwn

                                    If moRequest("showParent") <> "" Then
                                        GetContentDetailXml(oPageElmt, moRequest("contentParId"))
                                    Else
                                        GetContentDetailXml(oPageElmt, nContentId)
                                    End If

                                    ClearPageCache()

                                    Else
                                        'lets add the form to Content Detail
                                        Dim oPageDetail As XmlElement = moPageXml.CreateElement("ContentDetail")
                                    oPageElmt.AppendChild(oPageDetail)
                                    oPageDetail.AppendChild(xFrmContent)

                                    'lets add the page content
                                    If mnPageId > 0 Then
                                        GetContentXml(oPageElmt)
                                    End If
                                End If

                            Case "Delete"

                                'remove the relevent content information
                                Dim xFrmContent As XmlElement
                                xFrmContent = moAdXfm.xFrmDeleteContent(nContentId)
                                If moAdXfm.valid Then


                                    Dim oPageDetail As XmlElement = moPageXml.CreateElement("ContentDetail")
                                    oPageElmt.AppendChild(oPageDetail)
                                    oPageDetail.InnerXml = "<Content type=""message""><div>Content deleted successfully.</div></Content>"
                                    ClearPageCache()

                                Else
                                    'lets add the form to Content Detail
                                    Dim oPageDetail As XmlElement = moPageXml.CreateElement("ContentDetail")
                                    oPageElmt.AppendChild(oPageDetail)
                                    oPageDetail.AppendChild(xFrmContent)

                                    'lets add the page content
                                    If mnPageId > 0 Then
                                        GetContentXml(oPageElmt)
                                    End If
                                End If

                        End Select



                    Else
                        'raise error or do nothing
                        Dim oPageDetail As XmlElement = moPageXml.CreateElement("ContentDetail")
                        oPageElmt.AppendChild(oPageDetail)
                        oPageDetail.InnerXml = "<Content type=""error""><div>Sorry you do not have permission to " & IIf(AjaxCmd = "Delete", "delete", "update") & " this item, please contact the site administrator.</div></Content>"

                    End If

                    If bResetUser Then
                        mnUserId = 0
                        moDbHelper.mnUserId = 0
                    End If
                    ' Production request: Add a menu to everything
                    oPageElmt.AppendChild(GetStructureXML(mnUserId))


                Case "MenuNode"

                    'Make sure admin mode is true and we don't need to check for permissions

                    'TS Force to Admin Mode because you might not be calling from the same application
                    '  mbAdminMode = True

                    Dim expId As Long = CLng(moRequest("pgid"))
                    Dim nContextId As Long = 0
                    If moRequest("expid") <> "" Then
                        expId = CLng(moRequest("expid"))
                    End If

                    'Check for a context node
                    If moRequest("context") <> "" AndAlso IsNumeric(moRequest("context")) AndAlso CLng(moRequest("context")) > 0 Then
                        nContextId = CLng(moRequest("context"))
                    End If

                    'make sure we don't check for permissions.
                    mbAdminMode = True

                    'Note need to fix for newsletters.
                    Dim FullMenuXml As XmlElement = GetStructureXML(-1, RootPageId, nContextId)

                    Dim getLevel As Long = 0

                    'Move the requested ID to the top.
                    If FullMenuXml.SelectSingleNode("descendant-or-self::MenuItem[@id = " & expId & "]") Is Nothing Then

                    Else
                        getLevel = FullMenuXml.SelectNodes("descendant-or-self::MenuItem[@id = " & expId & "]/ancestor-or-self::MenuItem").Count()
                        FullMenuXml.ReplaceChild(FullMenuXml.SelectSingleNode("descendant-or-self::MenuItem[@id = " & expId & "]"), FullMenuXml.FirstChild)

                        FullMenuXml.SetAttribute("level", getLevel.ToString)
                    End If

                    oPageElmt.AppendChild(FullMenuXml)

                Case "Search.MostPopular"

                    Dim popularSearches As XmlElement = moDbHelper.GetMostPopularSearches(5, moRequest("filter") & "")

                    If popularSearches IsNot Nothing Then
                        Dim oPageDetail As XmlElement = moPageXml.CreateElement("ContentDetail")
                        oPageElmt.AppendChild(oPageDetail)
                        oPageDetail.AppendChild(popularSearches)
                    End If

            End Select

            ' Process common actions
            CommonActions()


            ' Check content versions
            If gbVersionControl Then CheckContentVersions()

            Me.SetPageLanguage()

            ' Process language
            Me.ProcessPageXMLForLanguage()

            oPageElmt.SetAttribute("layout", AjaxCmd)
            oPageElmt.SetAttribute("pageExt", moConfig("pageExt"))

            ' Add the responses
            CommitResponsesToPage()

            Return moPageXml

        Catch ex As Exception

            'returnException(mcModuleName, "buildPageXML", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetAjaxXML", ex, sProcessInfo))
            Return Nothing
        End Try

    End Function

    Public Function ReturnPageHTML(Optional ByVal nPageId As Long = 0, Optional ByVal bReturnBlankError As Boolean = False) As String
        PerfMon.Log("Web", "ReturnPageHTML")
        Dim sProcessInfo As String = "Transform PageXML using XSLT"
        Dim cPageHTML As String
        Try

            If nPageId <> 0 Then mnPageId = nPageId

            If moPageXml.OuterXml = "" Then
                GetPageXML()
            End If

            If moTransform Is Nothing Then
                Dim styleFile As String = CType(goServer.MapPath(mcEwSiteXsl), String)
                PerfMon.Log("Web", "ReturnPageHTML - loaded Style")
                moTransform = New Protean.XmlHelper.Transform(Me, styleFile, False)
            End If

            msException = ""

            moTransform.mbDebug = gbDebug
            icPageWriter = New IO.StringWriter
            moTransform.ProcessTimed(moPageXml, icPageWriter)

            cPageHTML = Replace(icPageWriter.ToString, "<?xml version=""1.0"" encoding=""utf-16""?>", "")
            cPageHTML = Replace(cPageHTML, "<?xml version=""1.0"" encoding=""UTF-8""?>", "")

            If bReturnBlankError And Not msException = "" Then
                Return ""
            Else
                Return cPageHTML
            End If

        Catch ex As Exception

            returnException(mcModuleName, "returnPageHtml", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
            If bReturnBlankError Then
                Return ""
            Else
                Return msException
            End If
        Finally
            icPageWriter.Dispose()
        End Try

    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    Public Overridable Sub AddCurrency()
        Dim sProcessInfo As String = "PerfMon"
        PerfMon.Log("Web", "AddCurrency")
        'Isolated function to provide the facility to overload the cart when called from an overloaded .web
        Try
            If gbCart Then

                sProcessInfo = "Begin AddCurrency"
                If moCart Is Nothing Then
                    moCart = New Cart(Me)
                End If
                moCart.SelectCurrency()
                sProcessInfo = "End AddCurrency"

            End If
        Catch ex As Exception
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "AddCurrency", ex, sProcessInfo))
        End Try
    End Sub

    Public Overridable Sub InitialiseCart()
        Dim sProcessInfo As String = "PerfMon"
        PerfMon.Log("Web", "addCart")
        'Isolated function to provide the facility to overload the cart when called from an overloaded .web
        Try
            If gbCart Then
                If Not moSession Is Nothing Then
                    If moCart Is Nothing Then
                        'we should not hit this because addCurrency should establish it.
                        moCart = New Cart(Me)
                    End If
                End If
            End If
        Catch ex As Exception
            'returnException(mcModuleName, "addCart", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "AddCart", ex, sProcessInfo))
        End Try
    End Sub
    ''' <summary>
    ''' 
    ''' </summary>
    Public Overridable Sub AddCart()
        Dim sProcessInfo As String = "PerfMon"
        PerfMon.Log("Web", "addCart")
        'Isolated function to provide the facility to overload the cart when called from an overloaded .web
        Try
            If gbCart Then

                sProcessInfo = "Begin Cart"
                If moCart Is Nothing Then
                    'we should not hit this because addCurrency should establish it.
                    moCart = New Cart(Me)
                End If
                'reinitialize variables because we might've changed some
                moCart.InitializeVariables()
                moCart.apply()
                'get any discount information for this page
                moDiscount.getAvailableDiscounts(moPageXml.DocumentElement)
                sProcessInfo = "End Cart"
            End If
        Catch ex As Exception
            'returnException(mcModuleName, "addCart", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "AddCart", ex, sProcessInfo))
        End Try
    End Sub

    Public Overridable Sub ProcessReports()
        Dim sProcessInfo As String = "PerfMon"
        PerfMon.Log("Web", "ProcessReports")
        'Isolated function to provide the facility to overload the cart when called from an overloaded .web
        Try
            If gbReport Then
                sProcessInfo = "Begin Report"
                Dim oEr As Protean.Cms.Report = New Protean.Cms.Report(Me)
                oEr.apply()
                oEr.close()
                oEr = Nothing
                sProcessInfo = "End Report"
            End If
        Catch ex As Exception
            'returnException(mcModuleName, "ProcessReports", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ProcessReports", ex, sProcessInfo))

        End Try
    End Sub

    Public Overridable Sub ProcessCalendar()
        Dim sProcessInfo As String = "PerfMon"
        PerfMon.Log("Web", "ProcessCalendar")
        Try
            sProcessInfo = "Begin Calendar"
            Me.moCalendar = New Protean.Cms.Calendar(Me)
            moCalendar.apply()
            moCalendar = Nothing
            sProcessInfo = "End Calendar"
        Catch ex As Exception
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ProcessCalendar", ex, sProcessInfo))
        End Try
    End Sub


    Private Sub ProcessPolls()
        PerfMon.Log("Web", "ProcessPolls")

        Dim sProcessInfo As String = "PerfMon"
        Dim sPollField As String = ""
        Dim sPollItemField As String = ""
        Dim sPollId As String = ""
        Dim sTable As String = ""
        Dim sSql As String = ""
        Dim sVoteFrequency As String = ""
        Dim dCurrentVotesExpiryDate As Date
        Dim dPreviousVotesCreationDate As Date
        Dim bVoteOnce As Boolean = False
        Dim sVoteIdentifiers As String = ""

        Dim sEmail As String = ""
        Dim cValidationError As String = ""

        Dim oDr As SqlDataReader

        Dim bUseUserId As Boolean = False
        Dim bUseCookies As Boolean = False
        Dim bUseEmail As Boolean = False
        Dim bUseIPAddress As Boolean = False

        Dim bCanVote As Boolean = True
        Dim nVoteBlockReason As PollBlockReason = PollBlockReason.None
        Dim cCookieName As String = ""
        Dim cLastVotedSql As String = ""

        Dim openDate As Date = Date.MinValue
        Dim closeDate As Date = Date.MaxValue


        Dim bHasVoted As Boolean = False

        Try
            sProcessInfo = "Begin Poll Processing"

            For Each ocNode As XmlElement In moPageXml.SelectNodes("/Page/Contents/Content[@type='Poll']")

                ' Reset variables - this is why I would like this to be a class instead.
                bCanVote = True
                nVoteBlockReason = PollBlockReason.None
                cValidationError = ""
                cCookieName = ""
                cLastVotedSql = ""
                bVoteOnce = False
                sVoteIdentifiers = ""
                bUseUserId = False
                bUseCookies = False
                bUseEmail = False
                bUseIPAddress = False
                openDate = Date.MinValue
                closeDate = Date.MaxValue

                ' Look for required nodes
                If ocNode.SelectSingleNode("Restrictions/Frequency") Is Nothing _
                   Or ocNode.SelectSingleNode("Restrictions/RegisteredVotersOnly") Is Nothing _
                   Or ocNode.SelectSingleNode("Restrictions/Identifiers") Is Nothing Then
                    ' Required nodes are missing
                Else
                    ' Set the config options for this poll
                    sVoteFrequency = ocNode.SelectSingleNode("Restrictions/Frequency").InnerText
                    bUseUserId = (ocNode.SelectSingleNode("Restrictions/RegisteredVotersOnly").InnerText = "true")
                    sVoteIdentifiers = ocNode.SelectSingleNode("Restrictions/Identifiers").InnerText

                    ' Check open and close dates
                    If ocNode.SelectSingleNode("dOpenDate") IsNot Nothing _
                        AndAlso IsDate(ocNode.SelectSingleNode("dOpenDate").InnerText) Then
                        openDate = CDate(ocNode.SelectSingleNode("dOpenDate").InnerText)
                    End If
                    If ocNode.SelectSingleNode("dCloseDate") IsNot Nothing _
                        AndAlso IsDate(ocNode.SelectSingleNode("dCloseDate").InnerText) Then
                        closeDate = CDate(ocNode.SelectSingleNode("dCloseDate").InnerText)
                    End If
                    If openDate > Date.Now Or closeDate < Date.Now Then
                        bCanVote = False
                        nVoteBlockReason = PollBlockReason.PollNotAvailable
                    End If

                    ' Sort out the vote frequency
                    Select Case sVoteFrequency
                        Case "once"
                            bVoteOnce = True
                        Case "daily"
                            dCurrentVotesExpiryDate = DateAdd(DateInterval.Hour, 24, Now())
                            dPreviousVotesCreationDate = DateAdd(DateInterval.Hour, -24, Now())
                        Case "weekly"
                            dCurrentVotesExpiryDate = DateAdd(DateInterval.Day, 7, Now())
                            dPreviousVotesCreationDate = DateAdd(DateInterval.Day, -7, Now())
                        Case "monthly"
                            dCurrentVotesExpiryDate = DateAdd(DateInterval.Month, 1, Now())
                            dPreviousVotesCreationDate = DateAdd(DateInterval.Month, -1, Now())
                    End Select


                    ' If we're not using a registered user then set the other identifiers
                    If Not bUseUserId Then
                        bUseCookies = sVoteIdentifiers.Contains("cookies")
                        bUseEmail = sVoteIdentifiers.Contains("email")
                        bUseIPAddress = sVoteIdentifiers.Contains("ipaddress")
                        If bUseIPAddress Then
                            gbIPLogging = True
                            Cms.gbIPLogging = True
                        End If
                    End If


                    ' Set the metadata
                    sPollField = "nArtId"
                    sPollItemField = "nOtherId"
                    sTable = "tblActivityLog"
                    sPollId = ocNode.GetAttribute("id")
                    cCookieName = "pvote-" & sPollId

                    ' Get the poll items
                    Dim oCRNode As XmlElement = moPageXml.CreateElement("PollItems")
                    moDbHelper.addRelatedContent(oCRNode, Convert.ToInt32(sPollId), True)
                    ocNode.AppendChild(oCRNode)

                    ' ===================================================================
                    ' Check the restrictions
                    ' ===================================================================
                    ' This should tell us if the user can vote, and has voted.

                    ' First - is this restricted to logged on users only
                    If bUseUserId And Not (mnUserId > 0) Then
                        bCanVote = False
                        nVoteBlockReason = PollBlockReason.RegisteredUsersOnly
                    End If

                    If bCanVote Then

                        ' There are two places that votes can be identified
                        ' tblActivityLog - if User Id, IP address or email are being used
                        ' Cookie - if cookies are being used.

                        ' We need to check for both and then assess whether they can vote.
                        sSql = ""

                        If bUseUserId Then
                            sSql &= " AND nUserDirId=" & mnUserId
                        Else
                            ' First check if a cookie is being used - if it exists then we block voting.
                            If bUseCookies AndAlso Not (moRequest.Cookies(cCookieName)) Is Nothing Then
                                bCanVote = False
                                nVoteBlockReason = PollBlockReason.CookieFound
                            End If

                            ' If no cookie formulate the sql for the other restrictions.
                            If bCanVote Then

                                If bUseIPAddress Then
                                    moDbHelper.checkForIpAddressCol()
                                    sSql &= " AND cIPAddress=" & Protean.Tools.Database.SqlString(Left(moRequest.ServerVariables("REMOTE_ADDR"), 15))
                                End If

                                If Not moRequest("poll-email") Is Nothing Then
                                    sEmail = moRequest("poll-email").ToString
                                    If bUseEmail Then sSql &= " AND cActivityDetail=" & Protean.Tools.Database.SqlString(sEmail)
                                End If
                            End If
                        End If

                        ' Finally, check the tblActivityLog for votes
                        If bCanVote Then


                            ' Get the last tblActivityVote
                            If Not String.IsNullOrEmpty(sSql) Then


                                ' Check for blocking
                                ' First get the scope of blocking, either Global or Poll
                                Dim blockingScope As String = moConfig("PollBlockingScope")
                                Dim blockingScopeQuery As String = ""
                                If String.IsNullOrEmpty(blockingScope) Then blockingScope = "Global"
                                If blockingScope.ToLower = "poll" Then
                                    blockingScopeQuery = "AND nArtId=" & sPollId & " "
                                End If

                                ' Check for blocks / exclusions
                                blockingScopeQuery = "SELECT TOP 1 nActivityKey " _
                                     & "FROM	dbo.tblActivityLog " _
                                     & "WHERE   nActivityType=" & dbHelper.ActivityType.VoteExcluded & " " _
                                     & blockingScopeQuery _
                                     & sSql
                                Dim blocked As String = moDbHelper.GetDataValue(blockingScopeQuery, , , "")
                                If Not String.IsNullOrEmpty(blocked) Then
                                    ' Block has been found
                                    bCanVote = False
                                    nVoteBlockReason = PollBlockReason.Excluded
                                Else
                                    ' Get the last tblActivityVote
                                    sSql = "SELECT  TOP 1 dDateTime As LastVoted " _
                                         & "FROM	dbo.tblActivityLog " _
                                         & "WHERE   nActivityType=" & dbHelper.ActivityType.SubmitVote & " " _
                                         & "AND nArtId=" & sPollId & " " _
                                         & sSql
                                    Dim cLastVoted As String = moDbHelper.GetDataValue(sSql, , , "")
                                    If Not (String.IsNullOrEmpty(cLastVoted)) AndAlso IsDate(cLastVoted) Then
                                        ' We found a vote, check the date
                                        If bVoteOnce OrElse cLastVoted > dPreviousVotesCreationDate Then
                                            ' Block the vote
                                            bCanVote = False
                                            nVoteBlockReason = PollBlockReason.LogFound
                                        End If
                                    End If

                                End If


                            End If
                        End If
                    End If




                    ' ===================================================================
                    ' Look for votes being submitted
                    ' ===================================================================

                    If bCanVote _
                        AndAlso Not moRequest("pollsubmit-" + sPollId) Is Nothing _
                        AndAlso Not String.IsNullOrEmpty(moRequest("pollsubmit-" + sPollId).ToString) _
                        AndAlso Not moRequest("polloption-" + sPollId) Is Nothing Then

                        Dim sResult As String = moRequest("polloption-" + sPollId).ToString

                        sEmail = ""
                        If Not moRequest("poll-email") Is Nothing Then
                            sEmail = moRequest("poll-email").ToString
                        End If

                        bHasVoted = True

                        If (Not (bUseEmail) And String.IsNullOrEmpty(sEmail)) Or Tools.Text.IsEmail(sEmail) Then
                            moDbHelper.logActivity(dbHelper.ActivityType.SubmitVote, mnUserId, mnPageId, Convert.ToInt16(sPollId), Convert.ToInt16(sResult), sEmail)

                            If bUseCookies Then
                                Dim oCookie As New System.Web.HttpCookie(cCookieName, "voted")
                                oCookie.Expires = dCurrentVotesExpiryDate
                                moResponse.Cookies.Add(oCookie)
                            End If


                            bCanVote = False
                            nVoteBlockReason = PollBlockReason.JustVoted
                        ElseIf bUseEmail And String.IsNullOrEmpty(sEmail) Then
                            cValidationError = "Email address required"
                        Else
                            cValidationError = "Invalid email address"
                        End If

                    End If

                    ' Add the status node

                    Dim oCStatusNode As XmlElement = moPageXml.CreateElement("Status")
                    oCStatusNode.SetAttribute("canVote", IIf(bCanVote, "true", "false"))
                    If bHasVoted Then oCStatusNode.SetAttribute("justVoted", "true")
                    If nVoteBlockReason <> PollBlockReason.None Then oCStatusNode.SetAttribute("blockReason", nVoteBlockReason.ToString)
                    If Not (String.IsNullOrEmpty(cValidationError)) Then oCStatusNode.SetAttribute("validationError", cValidationError)
                    ocNode.AppendChild(oCStatusNode)


                    ' Calculate the results
                    Dim oCResNode As XmlElement = moPageXml.CreateElement("Results")
                    'Do we need to check the table exists?
                    sSql = "SELECT DISTINCT " & sPollItemField & " AS PollOption, Count(" & sPollItemField & ") AS ResultsCount FROM " & sTable & " WHERE nActivityType=" & dbHelper.ActivityType.SubmitVote & " AND " & sPollField & " = '" & sPollId & "' GROUP BY " & sPollItemField
                    oDr = moDbHelper.getDataReader(sSql)

                    While oDr.Read
                        Dim oResNode As XmlElement = moPageXml.CreateElement("PollResult")
                        oResNode.SetAttribute("entryId", oDr(0))
                        oResNode.SetAttribute("votes", oDr(1))
                        oCResNode.AppendChild(oResNode)
                    End While
                    oDr.Close()
                    oDr = Nothing

                    ocNode.AppendChild(oCResNode)
                End If


            Next

            sProcessInfo = "End  Poll Processing"
        Catch ex As Exception
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ProcessPolls", ex, sProcessInfo))
        End Try
    End Sub

    Public Overridable Sub AddSearch(ByRef aWeb As Protean.Cms)
        oSrch = New Protean.Cms.Search(Me)
        oSrch.apply()
    End Sub

    ''' <summary>
    ''' Executes processing on specific content types.
    ''' </summary>
    ''' <remarks>In most cases, plugins (e.g. Search, Reports) and LayoutActions do this, so this is for content types that require a non-layout, non-function specific approach.</remarks>
    Private Sub ContentActions()

        PerfMon.Log("Web", "ContentActions")
        Dim sProcessInfo As String = ""
        Dim ocNode As XmlElement

        Try

            'Load in any specified content files

            For Each ocNode In moPageXml.SelectNodes("/Page/Contents/Content[@contentFile!=''] | /Page/ContentDetail/descendant-or-self::Content[@contentFile!='']")

                If IO.File.Exists(goServer.MapPath("/" & gcProjectPath & ocNode.GetAttribute("contentFile"))) Then
                    Dim newXml As New XmlDocument
                    newXml.PreserveWhitespace = True
                    newXml.Load(goServer.MapPath("/" & gcProjectPath & ocNode.GetAttribute("contentFile")))
                    'copy related nodes
                    Dim relElem As XmlElement
                    For Each relElem In ocNode.SelectNodes("Content")
                        Protean.Tools.Xml.AddExistingNode(newXml.DocumentElement, relElem)

                    Next
                    ocNode.InnerXml = newXml.DocumentElement.InnerXml
                    'ocNode.AppendChild(moPageXml.ImportNode(Protean.Tools.Xml.firstElement(newXml.DocumentElement), True))
                End If

            Next

            For Each ocNode In moPageXml.SelectNodes("/Page/*/Content[@appendFile!='']")

                If IO.File.Exists(goServer.MapPath("/" & gcProjectPath & ocNode.GetAttribute("appendFile"))) Then
                    Dim newXml As New XmlDocument
                    newXml.PreserveWhitespace = True
                    newXml.Load(goServer.MapPath("/" & gcProjectPath & ocNode.GetAttribute("appendFile")))

                    ocNode.AppendChild(moPageXml.ImportNode(newXml.DocumentElement, True))
                End If

            Next

            Dim ContentActionXpath As String = ""
            If mnArtId > 0 And LCase(moConfig("ActionOnDetail")) <> "true" Then
                ContentActionXpath = "/Page/Contents/Content[@action!='' and @actionOnDetail='true'] | /Page/ContentDetail/Content[@action!=''] | /Page/ContentDetail/Content/Content[@action!=''] | /Page/Contents/Content[@action!='' and @id='" & mnArtId & "']"
            Else
                ContentActionXpath = "/Page/Contents/Content[@action!='']"
            End If


            For Each ocNode In moPageXml.SelectNodes(ContentActionXpath)
                Dim classPath As String = ocNode.GetAttribute("action")
                Dim assemblyName As String = ocNode.GetAttribute("assembly")
                Dim assemblyType As String = ocNode.GetAttribute("assemblyType")
                Dim providerName As String = ocNode.GetAttribute("providerName")
                Dim providerType As String = ocNode.GetAttribute("providerType")
                If providerType = "" Then providerType = "messaging"

                Dim methodName As String = Right(classPath, Len(classPath) - classPath.LastIndexOf(".") - 1)

                classPath = Left(classPath, classPath.LastIndexOf("."))

                If classPath <> "" Then
                    Try
                        Dim calledType As Type

                        If assemblyName <> "" Then
                            classPath = classPath & ", " & assemblyName
                        End If
                        'Dim oModules As New Protean.Cms.Membership.Modules




                        If providerName <> "" Then
                            'case for external Providers
                            Dim moPrvConfig As Protean.ProviderSectionHandler = WebConfigurationManager.GetWebApplicationSection("protean/" & providerType & "Providers")
                            Dim assemblyInstance As [Assembly]

                            If Not moPrvConfig.Providers(providerName & "Local") Is Nothing Then
                                If moPrvConfig.Providers(providerName & "Local").Parameters("path") <> "" Then
                                    assemblyInstance = [Assembly].LoadFrom(goServer.MapPath(moPrvConfig.Providers(providerName & "Local").Parameters("path")))
                                    calledType = assemblyInstance.GetType(classPath, True)
                                Else
                                    assemblyInstance = [Assembly].Load(moPrvConfig.Providers(providerName & "Local").Type)
                                    calledType = assemblyInstance.GetType(classPath, True)
                                End If
                            Else
                                Select Case moPrvConfig.Providers(providerName).Parameters("path")
                                    Case ""
                                        assemblyInstance = [Assembly].Load(moPrvConfig.Providers(providerName).Type)
                                        calledType = assemblyInstance.GetType(classPath, True)
                                    Case "builtin"
                                        Dim prepProviderName As String ' = Replace(moPrvConfig.Providers(providerName).Type, ".", "+")
                                        'prepProviderName = (New Regex("\+")).Replace(prepProviderName, ".", 1)
                                        prepProviderName = moPrvConfig.Providers(providerName).Type
                                        calledType = System.Type.GetType(prepProviderName & "+" & classPath, True)
                                    Case Else
                                        assemblyInstance = [Assembly].LoadFrom(goServer.MapPath(moPrvConfig.Providers(providerName).Parameters("path")))
                                        classPath = moPrvConfig.Providers(providerName).Parameters("classPrefix") & classPath
                                        calledType = assemblyInstance.GetType(classPath, True)
                                End Select

                                'If moPrvConfig.Providers(providerName).Parameters("path") <> "" Then
                                '    assemblyInstance = [Assembly].LoadFrom(goServer.MapPath(moPrvConfig.Providers(providerName).Parameters("path")))
                                'Else
                                '    assemblyInstance = [Assembly].Load(moPrvConfig.Providers(providerName).Type)
                                'End If
                            End If

                            '  calledType = assemblyInstance.GetType(classPath, True)

                        ElseIf assemblyType <> "" Then
                            'case for external DLL's
                            Dim assemblyInstance As [Assembly] = [Assembly].Load(assemblyType)
                            calledType = assemblyInstance.GetType(classPath, True)
                        Else
                            'case for methods within EonicWeb Core DLL
                            calledType = System.Type.GetType(classPath, True)
                        End If

                        Dim o As Object = Activator.CreateInstance(calledType)

                        Dim args(1) As Object
                        args(0) = Me
                        args(1) = ocNode

                        calledType.InvokeMember(methodName, BindingFlags.InvokeMethod, Nothing, o, args)

                        'Error Handling ?
                        'Object Clearup ?

                        calledType = Nothing

                    Catch ex As Exception
                        '  OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ContentActions", ex, sProcessInfo))
                        sProcessInfo = classPath & "." & methodName & " not found"
                        ocNode.InnerXml = "<Content type=""error""><div>" & sProcessInfo & "</div></Content>"
                    End Try
                End If

            Next

            ' Content Type : ContentGrabber
            For Each ocNode In moPageXml.SelectNodes("/Page/Contents/Content[@display='grabber']")
                moDbHelper.getContentFromModuleGrabber(ocNode)
            Next

            For Each ocNode In moPageXml.SelectNodes("/Page/Contents/Content[@display='group']")
                moDbHelper.getContentFromProductGroup(ocNode)
            Next

            ' Content Type : ContentGrabber
            For Each ocNode In moPageXml.SelectNodes("/Page/Contents/Content[@type='ContentGrabber']")
                moDbHelper.getContentFromContentGrabber(ocNode)
            Next

            ' Content Type : Poll
            If moPageXml.SelectNodes("/Page/Contents/Content[@type='Poll']").Count > 0 Then
                ProcessPolls()
            End If




            ' Count Relations
            Dim cContentIdsForRelatedCount As String = ""
            For Each ocNode In moPageXml.SelectNodes("/Page/Contents/descendant-or-self::Content[@type='Tag' or @relatedCount='true']")
                cContentIdsForRelatedCount += ocNode.GetAttribute("id") & ","
            Next
            If cContentIdsForRelatedCount <> "" Then
                cContentIdsForRelatedCount = cContentIdsForRelatedCount.Remove(cContentIdsForRelatedCount.Length - 1)
                Dim sSql As String = "select Distinct COUNT(nContentParentid) as count, nContentChildid from tblContentRelation where nContentChildId in (" & cContentIdsForRelatedCount & ")  group by nContentChildid"
                Dim oDr As SqlDataReader = moDbHelper.getDataReader(sSql)
                Do While oDr.Read
                    For Each ocNode In moPageXml.SelectNodes("/Page/Contents/descendant-or-self::Content[@id='" & oDr("nContentChildId") & "']")
                        ocNode.SetAttribute("relatedCount", oDr("count"))
                    Next
                Loop
            End If


            ' Content Type : xForm with ContentAction
            If Not mbAdminMode Then
                Dim formXpath As String = "/Page/Contents/Content[(@type='xform' and model/submission/@SOAPAction) or (@process='xform') or (@moduleType='xForm' and model/submission/@SOAPAction)]"
                If mnArtId > 0 Then
                    'if the current contentDetail has child xform then that is all we process.
                    If moPageXml.SelectNodes("/Page/ContentDetail/descendant-or-self::Content[(@type='xform' and model/submission/@SOAPAction) or (@process='xform')]").Count > 0 Then
                        formXpath = "/Page/ContentDetail/descendant-or-self::Content[(@type='xform' and model/submission/@SOAPAction) or (@process='xform')]"
                    End If
                End If

                For Each ocNode In moPageXml.SelectNodes(formXpath)
                    'If oXform Is Nothing Then oXform = New xForm
                    If oXform Is Nothing Then oXform = getXform()

                    oXform.moPageXML = moPageXml
                    oXform.load(ocNode, True)
                    If oXform.isSubmitted Then
                        oXform.submit()
                    End If
                    oXform.addValues()
                    oXform = Nothing
                Next
                'just want to add submitted values but not hanlde submit
                formXpath = "/Page/Contents/Content[(@process='addValues')]"
                If mnArtId > 0 Then formXpath = "/Page/ContentDetail/descendant-or-self::Content[(@process='addValues')]"
                For Each ocNode In moPageXml.SelectNodes(formXpath)
                    If oXform Is Nothing Then oXform = getXform()
                    oXform.moPageXML = moPageXml
                    oXform.load(ocNode, True)
                    If oXform.isSubmitted Then
                        oXform.updateInstanceFromRequest()
                    End If
                    oXform.addValues()
                    oXform = Nothing
                Next
            End If

            BespokeActions()



        Catch ex As Exception
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ContentActions", ex, sProcessInfo))
        End Try
    End Sub


    ''' <summary>
    ''' Use of this is for discussion, but it is my intention that this could contain a repository of actions
    ''' that are not necearrily triggered by Content, but rather by command; and yet could be accessible to different 
    ''' faculties of EonicWeb, such as Admin calls, Ajax calls and Content building calls.
    ''' </summary>
    ''' <remarks></remarks>
    Protected Overridable Sub CommonActions()
        Try
            ' Integration calls - trying to use a similar methodology as above
            ' Commented out by adding False.
            Dim integrationCommand As String = ""

            If moRequest.QueryString.Count > 0 Then
                integrationCommand = moRequest("integration")
            End If

            If Not String.IsNullOrEmpty(integrationCommand) Then


                ' Directory integrations take a directory ID
                Dim requestedDirectoryId As String = moRequest("dirId")
                Dim directoryId As Long = Me.mnUserId
                If Not (String.IsNullOrEmpty(requestedDirectoryId)) _
                    AndAlso IsNumeric(requestedDirectoryId) _
                    AndAlso CInt(requestedDirectoryId) > 0 Then
                    directoryId = CLng(requestedDirectoryId)
                End If

                If directoryId > 0 Then
                    ' Invoke the integration method.
                    Dim constructorArguments() As Object = {Me, Convert.ToInt64(directoryId)}
                    Invoke.InvokeObjectMethod(
                        "Protean.Integration.Directory." & integrationCommand,
                        constructorArguments,
                        Nothing,
                        Me,
                        "OnComponentError",
                        "OnError"
                        )
                End If

            End If
        Catch ex As Exception
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "CommonActions", ex, ""))

        End Try
    End Sub


    ''' <summary>
    ''' To allow for bespoke content actions to be overloaded
    ''' </summary>
    ''' <remarks></remarks>
    Public Overridable Sub BespokeActions()

    End Sub

    ''' <summary>
    ''' Executes Actions based on Specific Page Layouts.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable Function LayoutActions() As String
        PerfMon.Log("Web", "LayoutActions")
        Dim sProcessInfo As String = ""
        Dim bRunSearches As Boolean = False


        Try

            Select Case moPageXml.SelectSingleNode("/Page/@layout").Value
                Case "Search_Results", "Search_Results_Products_Index", "Search", "Quick_Find"
                    If Not ibIndexMode Then
                        bRunSearches = True
                    End If
                Case "List_Quotes"
                    If mnUserId > 0 Then
                        Dim oQuote As Quote
                        oQuote = New Quote(Me)
                        oQuote.ListOrders(CInt("0" & moRequest("OrderId")))
                        oQuote = Nothing
                    End If
                Case "List_Orders"
                    If mnUserId > 0 Then
                        Dim oCart As Cart
                        oCart = New Cart(Me)
                        oCart.ListOrders(CInt("0" & moRequest("OrderId")))
                        oCart = Nothing
                    End If
            End Select

            If (gbCart Or gbQuote) And moPageXml.SelectSingleNode("/Page/@layout").Value.Contains("Discounts_") Then
                moDiscount.getDiscountXML(moPageXml.DocumentElement)
            End If

            'extra bit for searches
            If moPageXml.SelectSingleNode("/Page/@layout").Value.Contains("Search_Template") Then
                bRunSearches = True
            End If
            'commented out by TS because this case is hit in v5 when using a module and this means it runs twice.
            If moPageXml.SelectSingleNode("/Page/Contents/Content[@action='Protean.Cms+Search+Modules.GetResults']") Is Nothing Then
                If moRequest.QueryString.Count > 0 Then
                    If moRequest("searchMode") <> "" Then
                        bRunSearches = True
                    End If
                End If
            End If

            If bRunSearches Then
                AddSearch(Me)
            End If

            'extra bit for user control panels
            If moPageXml.SelectSingleNode("/Page/@layout").Value.Contains("User_Control_Panel") Or
            moPageXml.SelectSingleNode("/Page/@layout").Value.Contains("Internal_Feed") Then
                Dim cContentTypes As String = moConfig("ControlPanelTypes")
                If Not cContentTypes Is Nothing And Not cContentTypes = "" Then
                    Dim oTypes() As String = Split(cContentTypes, ",")
                    Dim i As Integer
                    For i = 0 To UBound(oTypes)
                        'add all of those types of content to the pagecontent to the page
                        GetContentXMLByType(moPageXml.DocumentElement, oTypes(i))
                    Next
                End If
            End If

            Return ""

        Catch ex As Exception
            'returnException(mcModuleName, "LayoutActions", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "LayoutActions", ex, sProcessInfo))
            Return ""
        End Try

    End Function


    Public Sub GetPageContentFromStoredProcedure(
                                                    ByVal SpName As String,
                                                    Optional ByVal parameters As Hashtable = Nothing,
                                                    Optional ByRef nCount As Integer = 0
                                                )

        PerfMon.Log("Web", "GetPageContentFromStoredProcedure")

        Dim oRoot As XmlElement
        Dim sProcessInfo As String = ""
        Dim oDs As DataSet

        Try
            oRoot = moPageXml.SelectSingleNode("//Contents")

            If oRoot Is Nothing Then
                oRoot = moPageXml.CreateElement("Contents")
                moPageXml.AppendChild(oRoot)
            End If

            oDs = moDbHelper.GetDataSet(SpName, "Content", "Contents", , parameters, CommandType.StoredProcedure)

            If oDs.Tables.Count > 0 Then
                nCount = oDs.Tables("Content").Rows.Count
                moDbHelper.AddDataSetToContent(oDs, oRoot, mnPageId, False, "", mdPageExpireDate, mdPageUpdateDate)
                '   AddGroupsToContent(oRoot)
            End If


        Catch ex As Exception
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetPageContentFromStoredProcedure", ex, sProcessInfo))
        End Try
    End Sub


    Public Sub RemoveDuplicateContent(Optional ByRef nRemoved As Long = 0)
        Dim sProcessInfo As String = ""
        Dim oContentElmt As XmlElement
        Dim nContentId As Long
        Dim hContentIds As New Hashtable
        Try

            'this will delete all but the first found.
            For Each oContentElmt In moPageXml.SelectNodes("/Page/Contents/Content")
                nContentId = oContentElmt.GetAttribute("id")
                If hContentIds(nContentId) Is Nothing Then
                    hContentIds.Add(nContentId, nContentId)
                Else
                    oContentElmt.ParentNode.RemoveChild(oContentElmt)
                    nRemoved = nRemoved - 1
                End If
            Next


        Catch ex As Exception
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "RemoveDuplicateContent", ex, sProcessInfo))
        Finally
            hContentIds = Nothing
        End Try

    End Sub


    'Public Sub GetPageContentFromSelect(ByVal sWhereSql As String, Optional ByVal bPrimaryOnly As Boolean = False, Optional ByRef nCount As Integer = 0, Optional ByVal bIgnorePermissionsCheck As Boolean = False, Optional ByVal nReturnRows As Integer = 0, Optional ByVal cOrderBy As String = "type, cl.nDisplayOrder", Optional ByRef oContentsNode As XmlElement = Nothing, Optional ByVal cAdditionalJoins As String = "", Optional bContentDetail As Boolean = False)
    '    PerfMon.Log("Web", "GetPageContentFromSelect")
    '    Dim oRoot As XmlElement
    '    Dim sSql As String

    '    Dim sPrimarySql As String = ""
    '    Dim sTopSql As String = ""
    '    Dim sMembershipSql As String = ""
    '    Dim sFilterSql As String = ""
    '    Dim sProcessInfo As String = ""
    '    Dim oDs As DataSet
    '    Dim nAuthUserId As Long
    '    Dim nAuthGroup As Long
    '    Dim cContentField As String = ""

    '    Try

    '        ' Apply the possiblity of getting contents into a node other than the page contents node
    '        If oContentsNode Is Nothing Then
    '            oRoot = moPageXml.SelectSingleNode("//Contents")

    '            If oRoot Is Nothing Then
    '                oRoot = moPageXml.CreateElement("Contents")
    '                moPageXml.DocumentElement.AppendChild(oRoot)
    '            End If
    '        Else
    '            oRoot = oContentsNode
    '        End If

    '        If bContentDetail = False Then
    '            cContentField = "cContentXmlBrief"
    '        Else
    '            cContentField = "cContentXmlDetail"
    '        End If

    '        If nReturnRows > 0 Then sTopSql = "TOP " & nReturnRows & " "

    '        sSql = "SET ARITHABORT ON "
    '        sSql &= "SELECT " & sTopSql & " c.nContentKey as id, dbo.fxn_getContentParents(c.nContentKey) as parId, cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, " & cContentField & " as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, CL.cPosition as position "
    '        sSql &= "FROM tblContent AS c INNER JOIN "
    '        sSql &= "tblAudit AS a ON c.nAuditId = a.nAuditKey LEFT OUTER JOIN "
    '        sSql &= "tblContentLocation AS CL ON c.nContentKey = CL.nContentId "

    '        ' GCF - sql replaced by the above - 24/06/2011
    '        ' replaced JOIN to tblContentLocation with  LEFT OUTER JOIN
    '        ' as we were getting nothing back when content had no related 
    '        ' content location data

    '        'sSql = "SET ARITHABORT ON SELECT " & sTopSql & " c.nContentKey as id, dbo.fxn_getContentParents(c.nContentKey) as parId, cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, CL.cPosition as position from tblContent c inner join tblContentLocation CL on c.nContentKey = CL.nContentId inner join tblAudit a on c.nAuditId = a.nAuditKey"


    '        '' Add the extra joins if specified.
    '        If Not (String.IsNullOrEmpty(cAdditionalJoins)) Then sSql &= " " & cAdditionalJoins & " "

    '        ' we only want to return results that occur on pages beneath the current root id.
    '        ' create a new funtion that passes in the StructId and the RootId to return yes or no.

    '        If bPrimaryOnly Then
    '            sPrimarySql = " CL.bPrimary = 1 "
    '        End If

    '        If (gbMembership = True And bIgnorePermissionsCheck = False) Then

    '            If mnUserId = 0 And gnNonAuthUsers <> 0 Then

    '                ' Note : if we are checking permissions for a page, and we're not logged in, then we shouldn't check with the gnAuthUsers group
    '                '         Ratehr, we should use the gnNonAuthUsers user group if it exists.

    '                nAuthUserId = gnNonAuthUsers
    '                nAuthGroup = gnNonAuthUsers

    '            ElseIf mnUserId = 0 Then

    '                ' If no gnNonAuthUsers user group exists, then remove the auth group
    '                nAuthUserId = mnUserId
    '                nAuthGroup = -1

    '            Else
    '                nAuthUserId = mnUserId
    '                nAuthGroup = gnAuthUsers
    '            End If

    '            ' Check the page is not denied
    '            sMembershipSql = " NOT(dbo.fxn_checkPermission(CL.nStructId," & nAuthUserId & "," & nAuthGroup & ") LIKE '%DENIED%')"

    '            ' Commenting out the folowing as it wouldn't return items that were Inherited view etc.
    '            ' sMembershipSql = " (dbo.fxn_checkPermission(CL.nStructId," & mnUserId & "," & gnAuthUsers & ") = 'OPEN' or dbo.fxn_checkPermission(CL.nStructId," & mnUserId & "," & gnAuthUsers & ") = 'VIEW')"
    '            ' add "and" if clause before
    '            If sPrimarySql <> "" Then sMembershipSql = " and " & sMembershipSql
    '        End If

    '        'show only live content that is within date, unless we are in admin mode.
    '        sFilterSql = GetStandardFilterSQLForContent((sPrimarySql <> "" Or sMembershipSql <> ""))


    '        ' add "and" if clause before
    '        If sPrimarySql <> "" Or sMembershipSql <> "" Or sFilterSql <> "" Then sWhereSql = " and " & sWhereSql

    '        sSql = sSql & " where (" & sPrimarySql & sMembershipSql & sFilterSql & sWhereSql & ")"
    '        If cOrderBy <> "" Then sSql &= " ORDER BY " & cOrderBy
    '        sSql = Replace(sSql, "&lt;", "<")
    '        oDs = moDbHelper.GetDataSet(sSql, "Content", "Contents")
    '        nCount = oDs.Tables("Content").Rows.Count
    '        moDbHelper.AddDataSetToContent(oDs, oRoot, mnPageId, False, "", mdPageExpireDate, mdPageUpdateDate)
    '        'If gbCart Or gbQuote Then
    '        '    moDiscount.getAvailableDiscounts(oRoot)
    '        'End If
    '        AddGroupsToContent(oRoot)
    '    Catch ex As Exception

    '        ' returnException(mcModuleName, "GetPageContentFromSelect", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
    '        OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetPageContentFromSelect", ex, sProcessInfo))
    '    End Try
    'End Sub


    Public Sub GetPageContentFromSelect(ByVal sWhereSql As String, Optional ByVal bPrimaryOnly As Boolean = False, Optional ByRef nCount As Integer = 0, Optional ByVal bIgnorePermissionsCheck As Boolean = False, Optional ByVal nReturnRows As Integer = 0, Optional ByVal cOrderBy As String = "type, cl.nDisplayOrder", Optional ByRef oContentsNode As XmlElement = Nothing, Optional ByVal cAdditionalJoins As String = "", Optional bContentDetail As Boolean = False, Optional pageNumber As Long = 0, Optional distinct As Boolean = False, Optional cShowSpecificContentTypes As String = "")
        PerfMon.Log("Web", "GetPageContentFromSelect")
        Dim oRoot As XmlElement
        Dim sSql As String

        Dim sPrimarySql As String = ""
        Dim sTopSql As String = ""
        Dim sMembershipSql As String = ""
        Dim sFilterSql As String = ""
        Dim sProcessInfo As String = ""
        Dim oDs As DataSet
        Dim nAuthUserId As Long
        Dim nAuthGroup As Long
        Dim cContentField As String = ""

        Try

            ' Apply the possiblity of getting contents into a node other than the page contents node
            If oContentsNode Is Nothing Then
                oRoot = moPageXml.SelectSingleNode("//Contents")

                If oRoot Is Nothing Then
                    oRoot = moPageXml.CreateElement("Contents")
                    moPageXml.DocumentElement.AppendChild(oRoot)
                End If
            Else
                oRoot = oContentsNode
            End If

            If bContentDetail = False Then
                cContentField = "cContentXmlBrief"
            Else
                cContentField = "cContentXmlDetail"
            End If

            If nReturnRows > 0 And pageNumber = 0 Then
                sTopSql = "TOP " & nReturnRows & " "
            End If

            sSql = "SET ARITHABORT ON "
            sSql &= "SELECT " & IIf(distinct, "DISTINCT ", "") & sTopSql & " c.nContentKey as id, dbo.fxn_getContentParents(c.nContentKey) as parId, cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, CAST(" & cContentField & " AS varchar(max)) as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, CL.cPosition as position "
            sSql &= "FROM tblContent AS c INNER JOIN "
            sSql &= "tblAudit AS a ON c.nAuditId = a.nAuditKey LEFT OUTER JOIN "
            sSql &= "tblContentLocation AS CL ON c.nContentKey = CL.nContentId "

            ' GCF - sql replaced by the above - 24/06/2011
            ' replaced JOIN to tblContentLocation with  LEFT OUTER JOIN
            ' as we were getting nothing back when content had no related 
            ' content location data

            'sSql = "SET ARITHABORT ON SELECT " & sTopSql & " c.nContentKey as id, dbo.fxn_getContentParents(c.nContentKey) as parId, cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, CL.cPosition as position from tblContent c inner join tblContentLocation CL on c.nContentKey = CL.nContentId inner join tblAudit a on c.nAuditId = a.nAuditKey"

            '' Add the extra joins if specified.
            If Not (String.IsNullOrEmpty(cAdditionalJoins)) Then sSql &= " " & cAdditionalJoins & " "

            ' we only want to return results that occur on pages beneath the current root id.
            ' create a new funtion that passes in the StructId and the RootId to return yes or no.

            If bPrimaryOnly Then
                sPrimarySql = " CL.bPrimary = 1 "
            End If

            If (gbMembership = True And bIgnorePermissionsCheck = False) Then

                If mnUserId = 0 And gnNonAuthUsers <> 0 Then

                    ' Note : if we are checking permissions for a page, and we're not logged in, then we shouldn't check with the gnAuthUsers group
                    '         Ratehr, we should use the gnNonAuthUsers user group if it exists.

                    nAuthUserId = gnNonAuthUsers
                    nAuthGroup = gnNonAuthUsers

                ElseIf mnUserId = 0 Then

                    ' If no gnNonAuthUsers user group exists, then remove the auth group
                    nAuthUserId = mnUserId
                    nAuthGroup = -1

                Else
                    nAuthUserId = mnUserId
                    nAuthGroup = gnAuthUsers
                End If

                ' Check the page is not denied
                sMembershipSql = " NOT(dbo.fxn_checkPermission(CL.nStructId," & nAuthUserId & "," & nAuthGroup & ") LIKE '%DENIED%')"

                ' Commenting out the folowing as it wouldn't return items that were Inherited view etc.
                ' sMembershipSql = " (dbo.fxn_checkPermission(CL.nStructId," & mnUserId & "," & gnAuthUsers & ") = 'OPEN' or dbo.fxn_checkPermission(CL.nStructId," & mnUserId & "," & gnAuthUsers & ") = 'VIEW')"
                ' add "and" if clause before
                If sPrimarySql <> "" Then sMembershipSql = " and " & sMembershipSql
            End If

            'show only live content that is within date, unless we are in admin mode.
            sFilterSql = GetStandardFilterSQLForContent((sPrimarySql <> "" Or sMembershipSql <> ""))


            ' add "and" if clause before
            If sPrimarySql <> "" Or sMembershipSql <> "" Or sFilterSql <> "" Then sWhereSql = " and " & sWhereSql

            sSql = sSql & " where (" & sPrimarySql & sMembershipSql & sFilterSql & sWhereSql & ")"
            If cOrderBy <> "" Then sSql &= " ORDER BY " & cOrderBy
            sSql = Replace(sSql, "&lt;", "<")

            PerfMon.Log("Web", "GetPageContentFromSelect", "GetPageContentFromSelect:" & sSql)

            If pageNumber > 0 Then
                oDs = moDbHelper.GetDataSet(sSql, "Content", "Contents", , , , nReturnRows, pageNumber)
            Else
                oDs = moDbHelper.GetDataSet(sSql, "Content", "Contents")
            End If
            nCount = oDs.Tables("Content").Rows.Count
            PerfMon.Log("Web", "GetPageContentFromSelect", "GetPageContentFromSelect: " & nCount & " returned")

            moDbHelper.AddDataSetToContent(oDs, oRoot, mnPageId, False, "", mdPageExpireDate, mdPageUpdateDate, True, gnShowRelatedBriefDepth, cShowSpecificContentTypes)

            'If gbCart Or gbQuote Then
            '    moDiscount.getAvailableDiscounts(oRoot)
            'End If
            ' AddGroupsToContent(oRoot)
        Catch ex As Exception

            ' returnException(mcModuleName, "GetPageContentFromSelect", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetPageContentFromSelect", ex, sProcessInfo))
        End Try
    End Sub

    Public Sub GetMenuContentFromSelect(ByVal sWhereSql As String, Optional ByVal bPrimaryOnly As Boolean = False, Optional ByRef nCount As Integer = 0, Optional ByVal bIgnorePermissionsCheck As Boolean = False, Optional ByVal nReturnRows As Integer = 0, Optional ByVal cOrderBy As String = "type, cl.nDisplayOrder", Optional ByRef oContentsNode As XmlElement = Nothing, Optional ByVal cAdditionalJoins As String = "", Optional bContentDetail As Boolean = False, Optional pageNumber As Long = 0, Optional distinct As Boolean = False)
        PerfMon.Log("Web", "GetPageContentFromSelect")
        Dim oRoot As XmlElement
        Dim sSql As String

        Dim sPrimarySql As String = ""
        Dim sTopSql As String = ""
        Dim sMembershipSql As String = ""
        Dim sFilterSql As String = ""
        Dim sProcessInfo As String = ""
        Dim oDs As DataSet
        Dim nAuthUserId As Long
        Dim nAuthGroup As Long
        Dim cContentField As String = ""

        Try

            ' Apply the possiblity of getting contents into a node other than the page contents node
            If oContentsNode Is Nothing Then
                oRoot = moPageXml.SelectSingleNode("//Contents")

                If oRoot Is Nothing Then
                    oRoot = moPageXml.CreateElement("Contents")
                    moPageXml.DocumentElement.AppendChild(oRoot)
                End If
            Else
                oRoot = oContentsNode
            End If

            If bContentDetail = False Then
                cContentField = "cContentXmlBrief"
            Else
                cContentField = "cContentXmlDetail"
            End If

            If nReturnRows > 0 And pageNumber = 0 Then
                sTopSql = "TOP " & nReturnRows & " "
            End If

            sSql = "SET ARITHABORT ON "
            sSql &= "SELECT " & IIf(distinct, "DISTINCT ", "") & sTopSql & " c.nContentKey as id, cl.nStructId as locId,"
            If bPrimaryOnly Then
                sSql &= " CL.nStructId as parId,"
            Else
                sSql &= " dbo.fxn_getContentParents(c.nContentKey) as parId,"
            End If
            sSql &= " cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, CAST(" & cContentField & " AS varchar(max)) as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, CL.cPosition as position "
            sSql &= "FROM tblContent AS c INNER JOIN "
            sSql &= "tblAudit AS a ON c.nAuditId = a.nAuditKey LEFT OUTER JOIN "
            sSql &= "tblContentLocation AS CL ON c.nContentKey = CL.nContentId "

            ' GCF - sql replaced by the above - 24/06/2011
            ' replaced JOIN to tblContentLocation with  LEFT OUTER JOIN
            ' as we were getting nothing back when content had no related 
            ' content location data

            'sSql = "SET ARITHABORT ON SELECT " & sTopSql & " c.nContentKey as id, dbo.fxn_getContentParents(c.nContentKey) as parId, cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, CL.cPosition as position from tblContent c inner join tblContentLocation CL on c.nContentKey = CL.nContentId inner join tblAudit a on c.nAuditId = a.nAuditKey"

            '' Add the extra joins if specified.
            If Not (String.IsNullOrEmpty(cAdditionalJoins)) Then sSql &= " " & cAdditionalJoins & " "

            ' we only want to return results that occur on pages beneath the current root id.
            ' create a new funtion that passes in the StructId and the RootId to return yes or no.

            If bPrimaryOnly Then
                sPrimarySql = " CL.bPrimary = 1 "
            End If

            If (gbMembership = True And bIgnorePermissionsCheck = False) Then

                If mnUserId = 0 And gnNonAuthUsers <> 0 Then

                    ' Note : if we are checking permissions for a page, and we're not logged in, then we shouldn't check with the gnAuthUsers group
                    '         Ratehr, we should use the gnNonAuthUsers user group if it exists.

                    nAuthUserId = gnNonAuthUsers
                    nAuthGroup = gnNonAuthUsers

                ElseIf mnUserId = 0 Then

                    ' If no gnNonAuthUsers user group exists, then remove the auth group
                    nAuthUserId = mnUserId
                    nAuthGroup = -1

                Else
                    nAuthUserId = mnUserId
                    nAuthGroup = gnAuthUsers
                End If

                ' Check the page is not denied
                sMembershipSql = " NOT(dbo.fxn_checkPermission(CL.nStructId," & nAuthUserId & "," & nAuthGroup & ") LIKE '%DENIED%')"

                ' Commenting out the folowing as it wouldn't return items that were Inherited view etc.
                ' sMembershipSql = " (dbo.fxn_checkPermission(CL.nStructId," & mnUserId & "," & gnAuthUsers & ") = 'OPEN' or dbo.fxn_checkPermission(CL.nStructId," & mnUserId & "," & gnAuthUsers & ") = 'VIEW')"
                ' add "and" if clause before
                If sPrimarySql <> "" Then sMembershipSql = " and " & sMembershipSql
            End If

            'show only live content that is within date, unless we are in admin mode.
            sFilterSql = GetStandardFilterSQLForContent((sPrimarySql <> "" Or sMembershipSql <> ""))


            ' add "and" if clause before
            If sPrimarySql <> "" Or sMembershipSql <> "" Or sFilterSql <> "" Then sWhereSql = " and " & sWhereSql

            sSql = sSql & " where (" & sPrimarySql & sMembershipSql & sFilterSql & sWhereSql & ")"
            If cOrderBy <> "" Then sSql &= " ORDER BY " & cOrderBy
            sSql = Replace(sSql, "&lt;", "<")

            If pageNumber > 0 Then
                oDs = moDbHelper.GetDataSet(sSql, "Content", "Contents", , , , nReturnRows, pageNumber)
            Else
                oDs = moDbHelper.GetDataSet(sSql, "Content", "Contents")
            End If
            nCount = oDs.Tables("Content").Rows.Count

            Dim oXml As XmlDocument = moDbHelper.ContentDataSetToXml(oDs, mdPageUpdateDate)
            Dim oXml2 As XmlNode = oRoot.OwnerDocument.ImportNode(oXml.DocumentElement, True)


            Dim oNode As XmlElement

            For Each oNode In oXml2.SelectNodes("Content")
                oRoot.AppendChild(moDbHelper.SimpleTidyContentNode(oNode, "", mdPageExpireDate, mdPageUpdateDate))
            Next

            ' moDbHelper.AddDataSetToContent(oDs, oRoot, mnPageId, False, "", mdPageExpireDate, mdPageUpdateDate, True, gnShowRelatedBriefDepth)

            'If gbCart Or gbQuote Then
            '    moDiscount.getAvailableDiscounts(oRoot)
            'End If
            '   AddGroupsToContent(oRoot)
        Catch ex As Exception

            ' returnException(mcModuleName, "GetPageContentFromSelect", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetPageContentFromSelect", ex, sProcessInfo))
        End Try
    End Sub


    Public Overridable Function MembershipProcess() As String
        PerfMon.Log("Web", "MembershipProcess")
        Dim sProcessInfo As String = ""
        Dim sReturnValue As String = Nothing
        Dim cLogonCmd As String = ""
        Try

            Dim oMembershipProv As New Providers.Membership.BaseProvider(Me, moConfig("MembershipProvider"))

            Return oMembershipProv.Activities.MembershipProcess(Me)

        Catch ex As Exception
            'returnException(mcModuleName, "MembershipLogon", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "MembershipProcess", ex, sProcessInfo))
            Return Nothing
        End Try
    End Function



    Public Overridable Function AlternativeAuthentication() As Boolean



        PerfMon.Log("Web", "AlternativeAuthentication")


        Dim cProcessInfo As String = ""
        'Dim bCheck As Boolean = False
        'Dim cToken As String = ""
        'Dim cKey As String = ""
        'Dim cDecrypted As String = ""
        'Dim nReturnId As Integer

        Try

            Dim oMembershipProv As New Providers.Membership.BaseProvider(Me, moConfig("MembershipProvider"))

            Return oMembershipProv.Activities.AlternativeAuthentication(Me)

            '' Look for the RC4 token
            'If moRequest("token") <> "" And moConfig("AlternativeAuthenticationKey") <> "" Then

            '    cProcessInfo = "IP Address Checking"

            '    Dim cIPList As String = CStr(moConfig("AlternativeAuthenticationIPList"))

            '    If cIPList = "" OrElse Tools.Text.IsIPAddressInList(moRequest.UserHostAddress, cIPList) Then

            '        cProcessInfo = "Decrypting token"
            '        Dim oEnc As New Protean.Tools.Encryption.RC4()

            '        cToken = moRequest("token")
            '        cKey = moConfig("AlternativeAuthenticationKey")

            '        ' There are two accepted formats to receive:
            '        '  1. Email address
            '        '  2. User ID

            '        cDecrypted = Trim(Tools.Encryption.RC4.Decrypt(cToken, cKey))

            '        If Tools.Text.IsEmail(cDecrypted) Then

            '            ' Authentication is by way of e-mail address
            '            cProcessInfo = "Email authenctication: Retrieving user for email: " & cDecrypted
            '            ' Get the user id based on the e-mail address
            '            nReturnId = moDbHelper.GetUserIDFromEmail(cDecrypted)

            '            If nReturnId > 0 Then
            '                bCheck = True
            '                Me.mnUserId = nReturnId
            '            End If

            '        ElseIf IsNumeric(cDecrypted) AndAlso CInt(cDecrypted) > 0 Then

            '            ' Authentication is by way of user ID
            '            cProcessInfo = "User ID Authentication: " & cDecrypted
            '            ' Get the user id based on the e-mail address
            '            bCheck = moDbHelper.IsValidUser(CInt(cDecrypted))
            '            If bCheck Then Me.mnUserId = CInt(cDecrypted)

            '        End If

            '    End If

            'End If

            'Return bCheck

        Catch ex As Exception
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "AlternativeAuthentication", ex, cProcessInfo))
            Return False
        End Try

    End Function

    ''' <summary>
    ''' <para>This provides the facility to redirect a logged on user from PATH A to PATH B if they belong to a certain group/directory item.</para>
    ''' <para>PATH A and PATH B are assumed to be sites that share the same database.  These could be different sites or just different application paths.</para>
    ''' <para>PATH A and PATH B cannot be child folders of each other - i.e. if PATH A is /patha, path b could not be /patha/pathb</para>
    ''' <para>PATH A and PATH B must be of the same type - i.e. both URIs or both Application paths.</para>
    ''' <example>e.g. "/patha/" and "/pathb/", "http://domain1/" and "http://domain2/", "http://domain1/" and "http://domain2/pathb"</example>
    ''' <example>But NOT "/patha/" and "http://domain2/pathb"</example>
    ''' </summary>
    ''' <remarks></remarks>
    Public Overridable Sub SiteRedirection()

        PerfMon.Log("Web", "SiteRedirection")
        Dim cProcessInfo As String = ""
        Try

            ' Parse the config string
            Dim cSiteConfig() As String
            Dim cToken As String = ""
            Dim cUrl As String = ""
            Dim aSites() As String = moConfig("SiteGroupRedirection").ToString.Split(";")


            For Each cSite As String In aSites
                cSiteConfig = cSite.Split(",")

                ' Now check for the following (using the bitwise operator, and in this order)
                ' 1 - We're not already arriving from a redirection (indicated by token being passed through in the Request) - this stops recurring redirection!
                ' 2 - Does the config contain 3 items
                ' 3 - Is the first config item a number
                ' 4 - Does the path in the config NOT match the start of the current page (check URI and Path) - in other words - don't run this check if we're actually on the site in question!
                ' 5 - Is the user a member of the directory item ID listed in the config.
                If Not (moRequest("token") <> "") _
                   AndAlso cSiteConfig.Length = 3 _
                   AndAlso IsNumeric(cSiteConfig(0)) _
                   AndAlso Not (
                                moRequest.Url.AbsoluteUri.StartsWith(cSiteConfig(1).ToString, StringComparison.CurrentCultureIgnoreCase) _
                                OrElse moRequest.Url.AbsolutePath.StartsWith(cSiteConfig(1).ToString, StringComparison.CurrentCultureIgnoreCase)
                            ) _
                   AndAlso Not (moPageXml.DocumentElement.SelectSingleNode("/Page/User/*[@id='" & cSiteConfig(0) & "']") Is Nothing) Then

                    ' Success - try to rewrite the URL, and produce a token
                    cUrl = "" & CStr(cSiteConfig(1))
                    cToken = "" & CStr(cSiteConfig(2))
                    If Not (String.IsNullOrEmpty(cUrl) Or String.IsNullOrEmpty(cToken)) Then

                        ' Create the Redirection Token
                        cToken = Tools.Encryption.RC4.Encrypt(Me.mnUserId.ToString, cToken)

                        ' Log the user off this site.
                        Me.LogOffProcess()

                        ' Redirect the user.
                        moResponse.Redirect(cUrl & "?token=" & cToken, False)
                        moCtx.ApplicationInstance.CompleteRequest()
                    End If

                End If

            Next

        Catch ex As Exception
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "SiteRedirection", ex, cProcessInfo))
        End Try

    End Sub

    Public Overridable Sub LogOffProcess()
        PerfMon.Log("Web", "LogOffProcess")
        Dim cProcessInfo As String = ""

        Try

            cProcessInfo = "Commit to Log"
            If gbSingleLoginSessionPerUser Then
                moDbHelper.logActivity(dbHelper.ActivityType.Logoff, mnUserId, 0)
                If moRequest.Cookies("ewslock") IsNot Nothing Then
                    moResponse.Cookies("ewslock").Expires = DateTime.Now.AddDays(-1)
                End If
            Else
                moDbHelper.CommitLogToDB(dbHelper.ActivityType.Logoff, mnUserId, moSession.SessionID, Now, 0, 0, "")
            End If


            ' Call this BEFORE clearing the user ID.
            cProcessInfo = "Clear Site Stucture"
            moDbHelper.clearStructureCacheUser()

            ' Clear the user ID.
            mnUserId = 0

            ' Clear the cart
            If Not moSession Is Nothing AndAlso gbCart Then
                Dim cSql As String = "update tblCartOrder set cCartSessionId = 'OLD_' + cCartSessionId where (cCartSessionId = '" & moSession.SessionID & "' and cCartSessionId <> '')"
                moDbHelper.ExeProcessSql(cSql)
            End If

            If Not moSession Is Nothing Then
                cProcessInfo = "Abandon Session"
                ' AJG : Question - why does this not clear the Session ID?
                moSession.Abandon()
            End If

            If moConfig("RememberMeMode") <> "KeepCookieAfterLogoff" Then
                cProcessInfo = "Clear Cookies"
                moResponse.Cookies("RememberMeUserName").Expires = DateTime.Now.AddDays(-1)
                moResponse.Cookies("RememberMeUserId").Expires = DateTime.Now.AddDays(-1)
            End If

        Catch ex As Exception
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "LogOffProcess", ex, cProcessInfo))
        End Try

    End Sub

    Public Overridable Function UserEditProcess() As String
        PerfMon.Log("Web", "UserEditProcess")
        Dim sProcessInfo As String = ""
        Dim sReturnValue As String = Nothing
        Try


            Return Nothing

        Catch ex As Exception
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "UserEditProcess", ex, sProcessInfo))
            Return Nothing
        End Try
    End Function

    Public Overridable Sub logonRedirect(ByVal cLogonCmd As String)
        Dim sProcessInfo As String = ""

        Try

            Dim sRedirectPath As String

            'don't redirect if in cart process
            If moRequest.QueryString("cartCmd") <> "Logon" Then

                If moSession("LogonRedirect") <> "" Then
                    sRedirectPath = moSession("LogonRedirect")
                    moSession("LogonRedirect") = ""
                Else

                    If moConfig("LogonRedirectPath") <> "" Then
                        'sRedirectPath = mcOriginalURL & moConfig("LogonRedirectPath") & cLogonCmd
                        'TS changed this remvoing mcOriginalURL because it breaks logon redirect "/" to go to root/
                        sRedirectPath = moConfig("LogonRedirectPath") & cLogonCmd
                    Else
                        sRedirectPath = mcOriginalURL & cLogonCmd
                    End If

                    If Not goLangConfig Is Nothing Then
                        sRedirectPath = mcPageLanguageUrlPrefix & sRedirectPath
                    End If

                End If
                If sRedirectPath.Contains("token=") Then

                    sRedirectPath = Regex.Replace(sRedirectPath, "(token=.*?)&", "")
                End If


                msRedirectOnEnd = sRedirectPath
            End If

        Catch ex As Exception
            'returnException(mcModuleName, "logonRedirect", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "logonRedirect", ex, sProcessInfo))
        End Try
    End Sub

    ''' <summary>
    ''' <para>
    ''' Check the request for certain parameters which if matched will create a 302 redirect
    ''' For page redirects, ISAPI can't detect what we need, so we are going to need to check if PageURLFormat is set
    ''' For content (artid) redirects, parameters are driven by the ISAPI rewrite routines which should be updated to do the following
    ''' </para>
    ''' <list>
    '''   <item>
    '''    If itemNNNN is detected then set u the redirect as follows:
    '''    <list>
    '''       <item>pass the path through as path</item>
    '''       <item>pass itemNNNN through as artid=NNNN</item>
    '''       <item>set a flag called redirect=statuscode</item>
    '''    </list>
    '''   </item>
    ''' </list>
    ''' <para>
    ''' The redirect URL will be "redirPath/NNNN-/Product-Name
    ''' </para>
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>This may require the existing pages to be tidied up (i.e. removing hyphens and plusses)</remarks>
    Private Function legacyRedirection() As Boolean
        PerfMon.Log("Web", "legacyRedirection")

        Dim cProcessInfo As String = ""
        Dim bRedirect As Boolean = False

        Try
            ' We can only do this if session exists
            If Not moSession Is Nothing Then
                If Me.moConfig("LegacyRedirect") = "on" Then

                    Dim cPath As String = Me.moRequest("path") & ""
                    If Not (cPath.StartsWith("/")) Then cPath = "/" & cPath
                    If Not (cPath.EndsWith("/")) Then cPath &= "/"

                    ' The checks we need to make are as follows:
                    ' Is RedirPath being passed through (from ISAPI Rewrite)
                    ' Is artid being passed through

                    ' Check if an article redirect has been called
                    If Not (Me.moRequest("redirect") Is Nothing) _
                        AndAlso Me.moRequest("artid") <> "" _
                        AndAlso IsNumeric(Me.moRequest("artid")) Then

                        ' Try to find the product
                        Dim nArtId As Long = CLng(Me.moRequest("artid"))
                        Dim cSql As String = "SELECT cContentName FROM tblContent WHERE nContentKey = " & SqlFmt(nArtId.ToString)
                        Dim cName As String = moDbHelper.GetDataValue(cSql, , , "")



                        If Not (String.IsNullOrEmpty(cName)) Then

                            ' Replace any non-alphanumeric with hyphens
                            Dim oRe As New Text.RegularExpressions.Regex("[^A-Z0-9]", Text.RegularExpressions.RegexOptions.IgnoreCase)
                            cName = oRe.Replace(cName, "-").Trim("-")


                            ' Iron out the spaces for paths
                            cPath = Replace(cPath, " ", "-")

                            ' Construct the new path
                            cPath = cPath & nArtId.ToString & "-/" & cName
                            bRedirect = True

                        End If

                    ElseIf Me.moConfig("PageURLFormat") = "hyphens" Then
                        ' Check the path - if hyphens is the preference then we need to analyse the path and rewrite it.
                        If cPath.Contains("+") Or cPath.Contains(" ") Then
                            cPath = Replace(cPath, "+", "-")
                            cPath = Replace(cPath, " ", "-")
                            bRedirect = True
                        End If
                    End If

                    ' Redirect
                    If bRedirect And Me.moSession("legacyRedirect") <> "on" Then
                        ' Stop recursive redirects
                        Me.moSession("legacyRedirect") = "on"

                        ' Assume status code is 301 unless instructed otherwise.
                        Dim nResponseCode As Integer = 301
                        Select Case Me.moRequest("redirect")
                            Case "301", "302", "303", "304", "307"
                                nResponseCode = CInt(Me.moRequest("redirect"))
                        End Select
                        HTTPRedirect(Me.moCtx, cPath, nResponseCode)

                    Else
                        Me.moSession("legacyRedirect") = "off"
                    End If
                End If

            End If
            PerfMon.Log("Web", "legacyRedirection - end")
            Return bRedirect

        Catch ex As Exception
            RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "legacyRedirection", ex, cProcessInfo))
            Return False
        End Try

    End Function


    Public Overridable Function getAdminXform() As Object
        'this is to allow us to overide adminXforms lower down
        Dim oAdXfm As Object

        oAdXfm = New Protean.Cms.Admin.AdminXforms(Me)

        Return oAdXfm

    End Function

    Public Overridable Function getXform() As Object
        'this is to allow us to overide Xforms lower down

        Dim oXfm As xForm = New xForm(Me)

        Return oXfm

    End Function

    Public Overridable Function GetXformEditor(Optional ByVal contentId As Long = 0) As Object

        Dim sProcessInfo As String = ""
        Dim oInstance As New XmlDocument
        Dim providerName As String
        Dim classPath As String
        Dim methodName As String
        Dim xFrmEditor As Object
        Try

            oInstance.LoadXml(moDbHelper.getObjectInstance(dbHelper.objectTypes.Content, contentId))

            If Not oInstance.SelectSingleNode("/tblContent/cContentXmlDetail/Content/@providerName") Is Nothing Then
                providerName = oInstance.SelectSingleNode("/tblContent/cContentXmlDetail/Content/@providerName").Value
            Else
                providerName = ""

            End If

            If Not oInstance.SelectSingleNode("/tblContent/cContentXmlDetail/Content/@xformeditor") Is Nothing Then
                classPath = oInstance.SelectSingleNode("/tblContent/cContentXmlDetail/Content/@xformeditor").Value
            Else
                classPath = ""
            End If

            methodName = "New"

            If providerName <> "" Then
                Dim moPrvConfig As Protean.ProviderSectionHandler = WebConfigurationManager.GetWebApplicationSection("protean/messagingProviders")
                Dim assemblyInstance As [Assembly] = [Assembly].Load(moPrvConfig.Providers(providerName).Type)
                Dim calledType As Type = assemblyInstance.GetType(classPath, True)

                Dim args(1) As Object
                args(0) = Me
                args(1) = contentId

                xFrmEditor = Activator.CreateInstance(calledType, args)

                Return xFrmEditor
            Else
                xFrmEditor = New Admin.XFormEditor(Me, contentId)
            End If

            Return xFrmEditor

        Catch ex As Exception
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetXformEditor", ex, sProcessInfo))
            Return Nothing
        End Try

    End Function

    Public Sub GetRequestVariablesXml(ByRef oPageElmt As XmlElement)
        PerfMon.Log("Web", "GetRequestVariablesXml")
        Dim root As XmlElement
        Dim item As Object
        Dim newElem As XmlElement
        Dim newElem2 As XmlElement

        Dim sProcessInfo As String = ""
        'If mDebugMode <> "Debug" Then On Error GoTo ErrorHandler
        Try

            root = moPageXml.CreateElement("Request")

            'Bring in the ServerVariables
            newElem = moPageXml.CreateElement("ServerVariables")
            For Each item In moRequest.ServerVariables
                Try
                    If Not (CStr(item) = "ALL_HTTP" Or CStr(item) = "ALL_RAW" Or moRequest.ServerVariables(CStr(item)) = "") Then
                        newElem2 = moPageXml.CreateElement("Item")
                        newElem2.SetAttribute("name", CStr(item))
                        newElem2.InnerText = moRequest.ServerVariables(CStr(item))
                        newElem.AppendChild(newElem2)
                    End If
                Catch
                    newElem2 = moPageXml.CreateElement("Item")
                    newElem2.SetAttribute("name", CStr(item))
                    newElem.AppendChild(newElem2)
                End Try


            Next item
            newElem2 = moPageXml.CreateElement("Item")
            newElem2.SetAttribute("name", "Date")
            newElem2.InnerText = Protean.Tools.Xml.XmlDate(Now)
            newElem.AppendChild(newElem2)

            'Meta-Generator
            newElem2 = moPageXml.CreateElement("Item")
            newElem2.SetAttribute("name", "GENERATOR")
            newElem2.InnerText = gcGenerator
            newElem.AppendChild(newElem2)

            newElem2 = moPageXml.CreateElement("Item")
            newElem2.SetAttribute("name", "CODEBASE")
            newElem2.InnerText = gcCodebase
            newElem.AppendChild(newElem2)

            newElem2 = moPageXml.CreateElement("Item")
            newElem2.SetAttribute("name", "REFERENCED_ASSEMBLIES")
            newElem2.InnerText = gcReferencedAssemblies
            newElem.AppendChild(newElem2)

            newElem2 = moPageXml.CreateElement("Item")
            newElem2.SetAttribute("name", "PREVIOUS_PAGE")
            If Not moSession Is Nothing Then newElem2.InnerText = moSession("previousPage")
            newElem.AppendChild(newElem2)

            newElem2 = moPageXml.CreateElement("Item")
            newElem2.SetAttribute("name", "SESSION_REFERRER")
            If Not moSession Is Nothing Then newElem2.InnerText = Referrer
            newElem.AppendChild(newElem2)

            If moConfig("ProjectPath") <> "" Then
                newElem2 = moPageXml.CreateElement("Item")
                newElem2.SetAttribute("name", "APPLICATION_ROOT")
                If moConfig("ProjectPath").StartsWith("/") Then
                    newElem2.InnerText = moConfig("ProjectPath")
                Else
                    newElem2.InnerText = "/" & moConfig("ProjectPath")
                End If
                newElem.AppendChild(newElem2)
            End If

            root.AppendChild(newElem)

            'Bring in the QueryString
            newElem = moPageXml.CreateElement("QueryString")
            For Each item In moRequest.QueryString
                newElem2 = moPageXml.CreateElement("Item")
                newElem2.SetAttribute("name", CStr(item))
                newElem2.InnerText = moRequest.QueryString(CStr(item))
                newElem.AppendChild(newElem2)
            Next item
            root.AppendChild(newElem)

            'Bring in the Form
            newElem = moPageXml.CreateElement("Form")
            For Each item In moRequest.Form
                newElem2 = moPageXml.CreateElement("Item")
                newElem2.SetAttribute("name", CStr(item))
                newElem2.InnerText = moRequest.Form(CStr(item))
                newElem.AppendChild(newElem2)
            Next item
            root.AppendChild(newElem)


            ' Code to store and retain google campaign codes with the session and output in XML.

            newElem = moPageXml.CreateElement("GoogleCampaign")
            Dim sVariables As String = "source,medium,term,content,campaign"
            Dim aVariables() As String = sVariables.Split(",")
            Dim i As Integer
            Dim bAppend As Boolean = False

            For i = 0 To UBound(aVariables)
                If moRequest.QueryString.Count > 0 Then
                    If moRequest("utm_" & aVariables(i)) <> "" Then
                        Dim oCookie As New System.Web.HttpCookie("utm_" & aVariables(i), moRequest("utm_" & aVariables(i)))
                        oCookie.Expires = DateAdd(DateInterval.Day, 14, Now())
                        moResponse.AppendCookie(oCookie)
                        'moSession("utm_" & aVariables(i)) = moRequest("utm_" & aVariables(i))
                    End If
                    If Not (moRequest.Cookies.Get("utm_" & aVariables(i)) Is Nothing And moRequest("utm_" & aVariables(i)) = "") Then
                        newElem2 = moPageXml.CreateElement("Item")
                        newElem2.SetAttribute("name", "utm_" & aVariables(i))
                        If moRequest("utm_" & aVariables(i)) = "" Then
                            newElem2.InnerText = moRequest("utm_" & aVariables(i))
                        Else
                            newElem2.InnerText = moRequest.Cookies.Get("utm_" & aVariables(i)).Value
                        End If
                        'newElem2.InnerText = moSession("utm_" & aVariables(i))
                        newElem.AppendChild(newElem2)
                        bAppend = True
                    End If
                End If
            Next
            If bAppend Then root.AppendChild(newElem)

            If oPageElmt.SelectSingleNode("/Page/Request") Is Nothing Then
                oPageElmt.AppendChild(oPageElmt.OwnerDocument.ImportNode(root, True))
            Else
                oPageElmt.ReplaceChild(oPageElmt.OwnerDocument.ImportNode(root, True), oPageElmt.SelectSingleNode("/Page/Request"))
            End If

        Catch ex As Exception

            'returnException(mcModuleName, "buildPageXML", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetRequestVariablesXml", ex, sProcessInfo))
        End Try


    End Sub

    Public Sub GetSettingsXml(ByRef oPageElmt As XmlElement)
        PerfMon.Log("Web", "GetSettingsXml")
        Dim root As XmlElement
        Dim sProcessInfo As String = ""
        Try
            If Not moRequest("reBundle") Is Nothing Then
                moCtx.Application("ewSettings") = Nothing
            End If
            If moCtx.Application("ewSettings") Is Nothing Then
                root = moPageXml.CreateElement("Settings")

                'Please never add any setting here you do not want to be publicly accessible.
                Dim s = "web.DescriptiveContentURLs;web.BaseUrl;web.SiteName;web.SiteLogo;web.GoogleAnalyticsUniversalID;web.GoogleTagManagerID;web.GoogleAPIKey;web.ScriptAtBottom;web.debug;cart.SiteURL;web.ImageRootPath;web.DocRootPath;web.MediaRootPath;web.menuNoReload;web.RootPageId;web.MenuTreeDepth;"
                s = s + "web.eonicwebProductName;web.eonicwebCMSName;web.eonicwebAdminSystemName;web.eonicwebCopyright;web.eonicwebSupportTelephone;web.eonicwebWebsite;web.eonicwebSupportEmail;web.eonicwebLogo;web.websitecreditURL;web.websitecreditText;web.websitecreditLogo;web.GoogleTagManagerID;web.ReCaptchaKey;"
                s = s + "theme.BespokeBoxStyles;theme.BespokeBackgrounds;theme.BespokeTextClasses;"
                s = s + moConfig("XmlSettings") & ";"

                Dim match As Match = Regex.Match(s, "(?<Name>[^\.]*)\.(?<Value>[^;]*);?")

                Dim moCartConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/cart")
                Dim oThemeConfig As System.Collections.Specialized.NameValueCollection = System.Web.Configuration.WebConfigurationManager.GetWebApplicationSection("protean/theme")
                Dim sCurrentTheme As String = Nothing
                If Not oThemeConfig Is Nothing Then
                    sCurrentTheme = oThemeConfig("CurrentTheme")
                    Dim themesetting = moPageXml.CreateElement("add")
                    themesetting.setAttribute("key", "theme.CurrentTheme")
                    themesetting.setAttribute("value", sCurrentTheme)
                    root.AppendChild(themesetting)
                End If
                While (match.Success)

                    Dim setting = moPageXml.CreateElement("add")
                    setting.setAttribute("key", match.Groups("Name").Value + "." + match.Groups("Value").Value)
                    Select Case match.Groups("Name").Value
                        Case "web"
                            setting.setAttribute("value", moConfig(match.Groups("Value").Value))
                        Case "cart"
                            If Not moCartConfig Is Nothing Then
                                setting.setAttribute("value", moCartConfig(match.Groups("Value").Value))
                            End If
                        Case "theme"
                            If Not sCurrentTheme Is Nothing And match.Groups("Value").Value <> "CurrentTheme" Then
                                setting.setAttribute("value", oThemeConfig(sCurrentTheme & "." & match.Groups("Value").Value))
                            End If
                    End Select
                    root.AppendChild(setting)
                    match = match.NextMatch()
                End While

                'create a random bundle version number
                Dim rnsetting = moPageXml.CreateElement("add")
                rnsetting.setAttribute("key", "bundleVersion")
                Dim rn As New Random
                rnsetting.setAttribute("value", rn.Next(10000, 99999))
                root.AppendChild(rnsetting)

                moCtx.Application("ewSettings") = root.InnerXml
            Else
                root = moPageXml.CreateElement("Settings")
                root.InnerXml = moCtx.Application("ewSettings")
            End If
            oPageElmt.AppendChild(root)

        Catch ex As Exception
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetSettingsXml", ex, sProcessInfo))
        End Try


    End Sub

    Public Function GetUserXML(Optional ByVal nUserId As Long = 0) As XmlElement
        PerfMon.Log("Web", "GetUserXML")
        Dim sProcessInfo As String = ""
        Try

            Dim oMembershipProv As New Providers.Membership.BaseProvider(Me, moConfig("MembershipProvider"))
            Return oMembershipProv.Activities.GetUserXml(Me, nUserId)

        Catch ex As Exception

            'returnException(mcModuleName, "getUserXml", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetUserXml", ex, sProcessInfo))
            Return Nothing
        End Try

    End Function

    Public Sub RefreshUserXML()
        PerfMon.Log("Web", "GetUserXML")
        Dim sProcessInfo As String = ""
        Dim oUserXml As XmlElement
        Try
            If mnUserId <> 0 Then
                oUserXml = moPageXml.SelectSingleNode("/Page/User")
                If oUserXml Is Nothing Then
                    moPageXml.DocumentElement.AppendChild(Me.GetUserXML(mnUserId))
                Else
                    moPageXml.DocumentElement.ReplaceChild(Me.GetUserXML(mnUserId), oUserXml)
                End If
            End If
        Catch ex As Exception
            'returnException(mcModuleName, "RefreshUserXML", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "RefreshUserXML", ex, sProcessInfo))
        End Try

    End Sub

    ''' <summary>
    '''    Gets the Structure XML, but doesn't add it to the page XML
    ''' </summary>
    ''' <param name="nUserId">User ID that you want to check for permissions on - if in Admin Mode, then if this is -1, no permissions will be checked, otherwise the permissions will be checked AND enumerated</param>
    ''' <param name="nRootId">The root ID of the Structure that you want to get</param>
    ''' <param name="nCloneContextId">If the root ID occurs within a cloned part of the structure, then give the cloned part's root node id</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable Function GetStructureXML(Optional ByVal nUserId As Long = 0, Optional ByVal nRootId As Long = 0, Optional ByVal nCloneContextId As Long = 0) As XmlElement

        Dim cFunctionDef As String = "GetStructureXML([Long], [Long], [Long])"
        Protean.PerfMon.Log("Web", cFunctionDef)

        Try

            Return Me.GetStructureXML(nUserId, nRootId, nCloneContextId, "", False, False, True, False, False, "MenuItem", "Menu")

        Catch ex As Exception
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, cFunctionDef, ex, ""))
            Return Nothing
        End Try

    End Function

    ''' <summary>
    '''    Gets the Structure XML, and adds it to the page XML
    ''' </summary>
    ''' <param name="cMenuId">Adds an id attribute to the root node</param>
    ''' <param name="nUserId">User ID that you want to check for permissions on - if in Admin Mode, then if this is -1, no permissions will be checked, otherwise the permissions will be checked AND enumerated</param>
    ''' <param name="nRootId">The root ID of the Structure that you want to get</param>
    ''' <param name="bLockRoot">Adds a loced attribute to the root node</param>
    ''' <param name="nCloneContextId">If the root ID occurs within a cloned part of the structure, then give the cloned part's root node id</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable Function GetStructureXML(ByVal cMenuId As String, Optional ByVal nUserId As Long = 0, Optional ByVal nRootId As Long = 0, Optional ByVal bLockRoot As Boolean = False, Optional ByVal nCloneContextId As Long = 0) As XmlElement

        Dim cFunctionDef As String = "GetStructureXML(String, [Long], [Long], [Boolean], [Long])"
        Protean.PerfMon.Log("Web", cFunctionDef)

        Try

            Return Me.GetStructureXML(nUserId, nRootId, nCloneContextId, cMenuId, True, bLockRoot, True, False, False, "MenuItem", "Menu")

        Catch ex As Exception
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, cFunctionDef, ex, ""))
            Return Nothing
        End Try

    End Function

    Public Overridable Function GetStructurePageXML(ByVal nPageId As Long,
                                           ByVal cMenuItemNodeName As String,
                                           ByVal cRootNodeName As String
                                           ) As XmlElement

        Dim cFunctionDef As String = "GetStructureXML(String, [Long], [Long], [Boolean], [Long])"
        Protean.PerfMon.Log("Web", cFunctionDef)

        Dim sSql As String
        Dim oDs As DataSet
        Dim sProcessInfo As String
        Dim oElmt As XmlElement = Nothing

        Try

            sSql = "SELECT		" &
                    " s.nStructKey as id, " &
                      " s.nStructParId as parId, " &
                      " s.cStructName as name, " &
                      " s.cUrl as url, " &
                      " s.cStructDescription as Description, " &
                      " a.dPublishDate as publish, " &
                      " a.dExpireDate as expire, " &
                      " a.nStatus as status, " &
                      " s.cStructForiegnRef as ref, " &
                      " 'ADMIN' as access,	" &
                      " s.cStructLayout as layout," &
                      " s.nCloneStructId as clone," &
                      " '' As accessSource," &
                      " 0 As accessSourceId, " &
                      " s.nVersionParId as vParId" &
                      " FROM	tblContentStructure s" &
                      " INNER JOIN  tblAudit a " &
                      " ON s.nAuditId = a.nAuditKey" &
                      " where(s.nVersionParId Is null Or s.nVersionParId = 0)" &
                      " and s.nStructKey = " & nPageId


            ' Get the dataset
            oDs = moDbHelper.GetDataSet(sSql, cMenuItemNodeName, cRootNodeName)

            ' Add nestings
            oDs.Relations.Add("rel01", oDs.Tables(0).Columns("id"), oDs.Tables(0).Columns("parId"), False)
            oDs.Relations("rel01").Nested = True

            If oDs.Tables(0).Rows.Count > 0 Then

                ' COLUMN MAPPING - STANDARD
                ' =========================
                oDs.Tables(0).Columns(0).ColumnMapping = Data.MappingType.Attribute
                oDs.Tables(0).Columns(1).ColumnMapping = Data.MappingType.Attribute
                oDs.Tables(0).Columns(2).ColumnMapping = Data.MappingType.Attribute
                oDs.Tables(0).Columns(3).ColumnMapping = Data.MappingType.Attribute
                oDs.Tables(0).Columns("url").ColumnMapping = Data.MappingType.Attribute
                If Me.mbAdminMode Then
                    oDs.Tables(0).Columns("status").ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns("publish").ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns("expire").ColumnMapping = Data.MappingType.Attribute
                End If

                ' COLUMN MAPPING - ACCESS
                ' =========================
                oDs.Tables(0).Columns("access").ColumnMapping = Data.MappingType.Attribute
                If oDs.Tables(0).Columns.Contains("accessSource") Then
                    oDs.Tables(0).Columns("accessSource").ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns("accessSourceId").ColumnMapping = Data.MappingType.Attribute
                End If

                If oDs.Tables(0).Columns.Contains("vParId") Then
                    oDs.Tables(0).Columns("vParId").ColumnMapping = Data.MappingType.Attribute
                End If

                If oDs.Tables(0).Columns.Contains("ref") Then
                    oDs.Tables(0).Columns("ref").ColumnMapping = Data.MappingType.Attribute
                End If

                ' COLUMN MAPPING - CLONES
                ' =========================
                If oDs.Tables(0).Columns.Contains("clone") Then oDs.Tables(0).Columns("clone").ColumnMapping = Data.MappingType.Attribute


                ' COVERT DATASET TO XML
                ' =====================
                oElmt = moPageXml.CreateElement(cRootNodeName)


                sProcessInfo = "GetStructureXML-dsToXml"
                Protean.PerfMon.Log("Web", sProcessInfo)

                'TS added lines to avoid whitespace issues
                Dim oXml As New XmlDocument
                oXml.LoadXml(oDs.GetXml)
                oXml.PreserveWhitespace = False

                oElmt.InnerXml = oXml.DocumentElement.OuterXml

                Dim oElmt2 As XmlElement
                Dim sContent As String
                'Convert any text to xml
                For Each oElmt2 In oElmt.SelectNodes("descendant-or-self::" & cMenuItemNodeName & "/Description")
                    sContent = oElmt2.InnerText
                    If sContent <> "" Then
                        Try
                            oElmt2.InnerXml = sContent
                        Catch
                            oElmt2.InnerXml = Protean.tidyXhtmlFrag(sContent)
                        End Try
                    End If
                Next

            End If

            Return oElmt

        Catch ex As Exception
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, cFunctionDef, ex, ""))
            Return Nothing
        End Try

    End Function

    ''' <summary>
    ''' Gets the Structure XML
    ''' </summary>
    ''' <param name="nUserId">User ID that you want to check for permissions on - if in Admin Mode, then if this is -1, no permissions will be checked, otherwise the permissions will be checked AND enumerated</param>
    ''' <param name="nRootId">The root ID of the Structure that you want to get</param>
    ''' <param name="nCloneContextId">If the root ID occurs within a cloned part of the structure, then give the cloned part's root node id</param>
    ''' <param name="cMenuId">Adds an id attribute to the root node</param>
    ''' <param name="bAddMenuToPageXML">If True, this will add the elmt to the page XML</param>
    ''' <param name="bLockRoot">Adds a loced attribute to the root node</param>
    ''' <param name="bUseCache">Can be used to force use of the Cache</param>
    ''' <param name="bIncludeExpiredAndHidden">Include pages that are expired, yet to be published or hidden.</param>
    ''' <param name="bPruneEvenIfInAdminMode">By default in admin mode all pages even DENIED ones are returned.  If this value is True then regardless of the admin mode status DENIED pages will be removed.</param>
    ''' <param name="cMenuItemNodeName">Specifies the node name of each Page node.  Default should be "MenuItem".  Note - if not default, then caching will not be employed.</param>
    ''' <param name="cRootNodeName">Specifies the root node name.  Default should be "Menu".  Note - if not default, then caching will not be employed.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable Function GetStructureXML(
                                            ByVal nUserId As Long,
                                            ByVal nRootId As Long,
                                            ByVal nCloneContextId As Long,
                                            ByVal cMenuId As String,
                                            ByVal bAddMenuToPageXML As Boolean,
                                            ByVal bLockRoot As Boolean,
                                            ByVal bUseCache As Boolean,
                                            ByVal bIncludeExpiredAndHidden As Boolean,
                                            ByVal bPruneEvenIfInAdminMode As Boolean,
                                            ByVal cMenuItemNodeName As String,
                                            ByVal cRootNodeName As String
                                            ) As XmlElement
        Dim cFunctionDef As String = "GetStructureXML(Long,Long,Long,String,Boolean,Boolean,Boolean,Boolean,Boolean,String,String)"
        Protean.PerfMon.Log("Web", cFunctionDef)

        Dim oDs As Data.DataSet
        Dim oElmt As XmlElement
        Dim oClone As XmlElement = Nothing
        Dim nCloneId As Integer = 0
        Dim nCloneParentId As Integer = 0

        Dim nTempRootId As Integer = 0
        Dim oMenuItem As XmlElement
        Dim oChild As XmlElement

        Dim nAuthUsers As Long = gnAuthUsers

        Dim sContent As String
        Dim sSql As String
        Dim cCacheMode As String = "off"
        Dim bCacheXml As Boolean = False
        Dim cFilePathModifier As String = ""
        Dim pvElmt As XmlElement
        Dim bAuth As Boolean = True

        Dim sProcessInfo As String = "" = "GetStructureXML"
        Dim cCacheType As String

        Try

            ' INITIALISE VARIABLES
            ' =====================

            ' Node names
            If cMenuItemNodeName = "" Then cMenuItemNodeName = "MenuItem"
            If cRootNodeName = "" Then cRootNodeName = "Menu"
            cCacheType = cRootNodeName & "/" & cMenuItemNodeName

            ' Override the cache if we're not getting menu items
            'If cMenuItemNodeName <> "MenuItem" And cRootNodeName <> "Menu" Then bUseCache = False

            ' Project Path modifier
            If Not (moConfig("ProjectPath") Is Nothing) Then
                cFilePathModifier = moConfig("ProjectPath").TrimEnd("/").TrimStart("/")
                If cFilePathModifier <> "" Then cFilePathModifier = "/" & cFilePathModifier
            End If

            ' Root Id
            If nRootId = 0 Then nRootId = RootPageId

            ' User Id
            ' If AdminMode and no user then set user to be -1 otherwise apply the userid
            If nUserId = 0 Then nUserId = IIf(mbAdminMode, -1, mnUserId)
            If nUserId = -1 Then bAuth = False

            ' Set Non-Authenticated user permissions
            ' Don't forget to clear authenticated users if the user is not logged on.
            If gbMembership Then
                If nUserId = 0 Or nUserId = gnNonAuthUsers Then
                    nAuthUsers = 0
                    If nUserId = 0 And gnNonAuthUsers <> 0 Then
                        nUserId = gnNonAuthUsers
                        bAuth = False
                    End If
                End If
            Else
                bAuth = False
            End If

            ' CACHE MODE
            ' ===========
            ' Work out whether or not to use site structure caching
            ' Only check caching if user is logged on and is not admin mode, and caching has been turned on

            'If we are indexing from SOAP we have no session object therefore we don't use cache
            If moSession Is Nothing Then bUseCache = False
            ' If we are in admin mode we don't use cache
            ' If mbAdminMode Then bUseCache = False

            ' Site caching can be turned on through a site request - this will only work for sites with membership
            ' OR if site caching is turned on and membership is off, then save a single site structure for all users (i.e. don;t use session)
            If bUseCache Then
                sProcessInfo = "GetStructureXML-CheckCaching"
                Protean.PerfMon.Log("Web", sProcessInfo)

                If moSession("cacheMode") <> "" Then
                    cCacheMode = moSession("cacheMode")
                ElseIf moConfig("SiteCache") <> "" Then
                    cCacheMode = IIf(gbSiteCacheMode, "on", "off")
                End If

                ' Check out of a caching override has been passed through
                If moRequest("cacheMode") <> "" Then
                    cCacheMode = LCase(CStr(moRequest("cacheMode")))
                End If

                ' Store the cache to the session
                moSession("cacheMode") = cCacheMode

                ' If Cache is on then check for a cached strcture
                If cCacheMode = "on" Then

                    Dim oCache As XmlElement = moPageXml.CreateElement(cRootNodeName)

                    If mbAdminMode And cCacheType = "Menu/MenuItem" Then
                        oCache.InnerXml = goApp("AdminStructureCache")
                    Else


                        Dim cacheSearchCriteria As String = " WHERE nCacheDirId = " & Protean.SqlFmt(nUserId) & " AND cCacheType='" & cCacheType & "'"
                        If bAuth Then
                            cacheSearchCriteria &= " AND cCacheSessionID = '" & moSession.SessionID & "' AND DATEDIFF(hh,dCacheDate,GETDATE()) > 12"
                        End If
                        sProcessInfo = "GetStructureXML-SelectFromCache"
                        Protean.PerfMon.Log("Web", sProcessInfo)
                        ' Get the cached structure - returns empty string if no structure found.
                        sSql = "SELECT TOP 1 cCacheStructure FROM dbo.tblXmlCache " _
                                    & cacheSearchCriteria

                        sProcessInfo = "GetStructureXML-getCachefromDB-Start"
                        Protean.PerfMon.Log("Web", sProcessInfo)

                        moDbHelper.AddXMLValueToNode(sSql, oCache)

                        sProcessInfo = "GetStructureXML-getCachefromDB-End"
                        Protean.PerfMon.Log("Web", sProcessInfo)


                    End If

                    If cRootNodeName <> "Menu" Then
                        For Each oElmt In oCache.SelectNodes("descendant-or-self::Menu")
                            Tools.Xml.renameNode(oElmt, cRootNodeName)
                        Next
                    End If

                    If cMenuItemNodeName <> "MenuItem" Then
                        For Each oElmt In oCache.SelectNodes("descendant-or-self::MenuItem")
                            Tools.Xml.renameNode(oElmt, cMenuItemNodeName)
                        Next
                    End If



                    If Not oCache.FirstChild Is Nothing Then
                        bCacheXml = True
                        oElmt = oCache
                    End If


                End If

            End If

            ' If we don't have a cache to check then get a new structure
            If bCacheXml = False Or cCacheMode = "off" Then

                ' DATA CALL : GET STRUCTURE
                ' =========================
                '
                ' This will return structure nodes, that will optionally have the following:
                ' - checks for page level permissions (indicated by nUserId not being -1) - if these are returned they will need to cleaned up.
                ' - enumerate who teh permissions have come from (indicated by nUserId not being -1 and badminMode being 1)
                ' - exclude expired, not yet published and hidden pages if not in adminmode.



                sSql = "EXEC getContentStructure_v2 @userId=" & nUserId & ", @bAdminMode=" & CInt(mbAdminMode) & ", @dateNow=" & Protean.sqlDate(mdDate) & ", @authUsersGrp = " & nAuthUsers & ", @bReturnDenied=1"

                sProcessInfo = "GetStructureXML-getContentStrcuture"
                Protean.PerfMon.Log("Web", sProcessInfo)

                If bIncludeExpiredAndHidden Then sSql += ",@bShowAll=1"

                ' Get the dataset
                oDs = moDbHelper.GetDataSet(sSql, cMenuItemNodeName, cRootNodeName)

                ' Add Page Version Info
                If Features.ContainsKey("PageVersions") Then
                    If mbAdminMode And (nUserId = -1) Then
                        ' check we are returning strucutre for current user and not for another user such as in bespoke report (PDP for LMS system).
                        sSql = "EXEC getAllPageVersions"
                    Else
                        sSql = "EXEC getUserPageVersions @userId=" & nUserId & ", @dateNow=" & Protean.sqlDate(mdDate) & ", @authUsersGrp = " & nAuthUsers & ", @bReturnDenied=0, @bShowAll=0"
                    End If

                    sProcessInfo = "GetStructureXML-getPageVersions"
                    Protean.PerfMon.Log("Web", sProcessInfo)

                    moDbHelper.addTableToDataSet(oDs, sSql, "PageVersion")
                    oDs.Relations.Add("rel02", oDs.Tables(0).Columns("id"), oDs.Tables(1).Columns("vParId"), False)
                    oDs.Relations("rel02").Nested = True

                    If oDs.Tables(1).Rows.Count > 0 Then
                        oDs.Tables(1).Columns("id").ColumnMapping = Data.MappingType.Attribute
                        oDs.Tables(1).Columns("parId").ColumnMapping = Data.MappingType.Hidden
                        oDs.Tables(1).Columns("name").ColumnMapping = Data.MappingType.Attribute
                        oDs.Tables(1).Columns("url").ColumnMapping = Data.MappingType.Attribute
                        ' oDs.Tables(1).Columns("Description").ColumnMapping = Data.MappingType.SimpleContent
                        oDs.Tables(1).Columns("publish").ColumnMapping = Data.MappingType.Attribute
                        oDs.Tables(1).Columns("expire").ColumnMapping = Data.MappingType.Attribute
                        oDs.Tables(1).Columns("status").ColumnMapping = Data.MappingType.Attribute
                        oDs.Tables(1).Columns("access").ColumnMapping = Data.MappingType.Attribute
                        oDs.Tables(1).Columns("layout").ColumnMapping = Data.MappingType.Attribute
                        oDs.Tables(1).Columns("clone").ColumnMapping = Data.MappingType.Attribute
                        oDs.Tables(1).Columns("vParId").ColumnMapping = Data.MappingType.Attribute
                        oDs.Tables(1).Columns("lang").ColumnMapping = Data.MappingType.Attribute
                        oDs.Tables(1).Columns("desc").ColumnMapping = Data.MappingType.Attribute
                        oDs.Tables(1).Columns("verType").ColumnMapping = Data.MappingType.Attribute
                    End If

                End If

                ' Add nestings
                oDs.Relations.Add("rel01", oDs.Tables(0).Columns("id"), oDs.Tables(0).Columns("parId"), False)
                oDs.Relations("rel01").Nested = True

                If oDs.Tables(0).Rows.Count > 0 Then

                    ' COLUMN MAPPING - STANDARD
                    ' =========================
                    oDs.Tables(0).Columns(0).ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns(1).ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns(2).ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns(3).ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns("url").ColumnMapping = Data.MappingType.Attribute
                    If Me.mbAdminMode Then
                        oDs.Tables(0).Columns("status").ColumnMapping = Data.MappingType.Attribute
                        oDs.Tables(0).Columns("publish").ColumnMapping = Data.MappingType.Attribute
                        oDs.Tables(0).Columns("expire").ColumnMapping = Data.MappingType.Attribute
                    End If

                    ' COLUMN MAPPING - ACCESS
                    ' =========================
                    oDs.Tables(0).Columns("access").ColumnMapping = Data.MappingType.Attribute
                    If oDs.Tables(0).Columns.Contains("accessSource") Then
                        oDs.Tables(0).Columns("accessSource").ColumnMapping = Data.MappingType.Attribute
                        oDs.Tables(0).Columns("accessSourceId").ColumnMapping = Data.MappingType.Attribute
                    End If

                    If oDs.Tables(0).Columns.Contains("vParId") Then
                        oDs.Tables(0).Columns("vParId").ColumnMapping = Data.MappingType.Attribute
                    End If

                    If oDs.Tables(0).Columns.Contains("ref") Then
                        oDs.Tables(0).Columns("ref").ColumnMapping = Data.MappingType.Attribute
                    End If

                    ' COLUMN MAPPING - CLONES
                    ' =========================
                    If oDs.Tables(0).Columns.Contains("clone") Then oDs.Tables(0).Columns("clone").ColumnMapping = Data.MappingType.Attribute


                    ' COVERT DATASET TO XML
                    ' =====================
                    oElmt = moPageXml.CreateElement(cRootNodeName)

                    sProcessInfo = "GetStructureXML-dsToXml"
                    Protean.PerfMon.Log("Web", sProcessInfo)

                    'TS added lines to avoid whitespace issues
                    Dim oXml As New XmlDocument
                    oXml.LoadXml(oDs.GetXml)
                    oXml.PreserveWhitespace = False

                    oElmt.InnerXml = oXml.DocumentElement.OuterXml



                    ' MENU TIDY UP
                    ' ==================
                    ''Rename the VersionMenuNodes
                    'If mbAdminMode And Features.ContainsKey("PageVersions") Then
                    '    sProcessInfo = "GetStructureXML-RenameVersions"
                    '    Protean.PerfMon.Log("Web", sProcessInfo)
                    '    Dim oVersionMenuItems As XmlNodeList = oElmt.SelectNodes(cRootNodeName & "/" & cMenuItemNodeName & "[@vParId!='']")
                    '    For Each oVerMenuItem As XmlElement In oVersionMenuItems
                    '        Tools.Xml.renameNode(oVerMenuItem, "PageVersion")
                    '    Next
                    'End If

                    ' REMOVE THE ORPHANS
                    ' ==================
                    ' Because the SQL may not return DENIED pages, we need to check for orphaned items
                    ' Orphaned records are ones that have no parent menu node (because it wasn't returned by the SQL)
                    ' By default these will be returned to the root node, which will also include the root page node - which we don't want to delete.
                    ' The genuine root node will be node with a parId of 0 (possibly also including System Pages)

                    sProcessInfo = "GetStructureXML-CleanOrphans"
                    Protean.PerfMon.Log("Web", sProcessInfo)

                    Dim oRootMenuItems As XmlNodeList = oElmt.SelectNodes(cRootNodeName & "/" & cMenuItemNodeName & "[@parId!=0]")
                    For Each oRootMenuItem As XmlNode In oRootMenuItems
                        oRootMenuItem.ParentNode.RemoveChild(oRootMenuItem)
                    Next



                    ' DETERMINE THE ROOT NODE ID
                    ' ==========================
                    ' We need to determine the root id
                    ' This allows us to not do unnecessary cloning
                    ' If a clonecontextnode has been passed, then we need to get that for now

                    sProcessInfo = "GetStructureXML-GetIndicativeRootId"
                    Protean.PerfMon.Log("Web", sProcessInfo)

                    If nCloneContextId > 0 Then
                        nTempRootId = nCloneContextId
                    ElseIf nRootId > 0 Then
                        nTempRootId = nRootId
                    Else
                        nTempRootId = oElmt.FirstChild.FirstChild.Attributes("id").Value
                    End If



                    ' GET CLONED NODES
                    ' ==================
                    ' Note - we only want to find cloned nodes under the root id in question
                    If gbClone Then

                        Protean.PerfMon.Log("Web", "GetStructureXML-cloneNodes")
                        Dim cNodeSnapshot As New Hashtable()

                        ' GET CLONE SNAPSHOT
                        ' Why snapshot?  It stops cloned pages that also may have parent nodes cloned later changing through this process.
                        For Each oMenuItem In oElmt.SelectNodes("descendant-or-self::" & cMenuItemNodeName & "[@id='" & nTempRootId & "']/descendant-or-self::" & cMenuItemNodeName & "[@clone and not(@clone=0)]")
                            ' Go and get the cloned node
                            nCloneId = oMenuItem.GetAttribute("clone")
                            If Not (Tools.Xml.NodeState(oElmt, "descendant-or-self::" & cMenuItemNodeName & "[@id='" & nCloneId & "']", , , , oClone) = Tools.Xml.XmlNodeState.NotInstantiated) Then

                                If Not (cNodeSnapshot.ContainsKey(nCloneId)) Then cNodeSnapshot.Add(nCloneId, oClone.InnerXml)
                            End If
                        Next

                        ' ADD CLONE SNAPSHOTS TO MENU
                        ' Run through the nodes again, this time processing them 
                        For Each oMenuItem In oElmt.SelectNodes("descendant-or-self::" & cMenuItemNodeName & "[@id='" & nTempRootId & "']/descendant-or-self::" & cMenuItemNodeName & "[@clone and not(@clone=0)]")

                            ' Go and get the cloned node
                            nCloneId = oMenuItem.GetAttribute("clone")
                            nCloneParentId = oMenuItem.GetAttribute("id")
                            If Not (Tools.Xml.NodeState(oElmt, "descendant-or-self::" & cMenuItemNodeName & "[@id='" & nCloneId & "']", , , , oClone) = Tools.Xml.XmlNodeState.NotInstantiated) Then

                                oMenuItem.InnerXml = cNodeSnapshot(nCloneId)

                                ' Add the cloned flag to nodes and children
                                For Each oChild In oMenuItem.SelectNodes("descendant-or-self::" & cMenuItemNodeName)
                                    oChild.SetAttribute("cloneparent", nCloneParentId)
                                Next
                            End If
                        Next

                    End If


                    ' PROPOGATE THE PERMISSIONS AND PRUNE
                    ' ===================================
                    sProcessInfo = "GetStructureXML-TidyMenunode"
                    Protean.PerfMon.Log("Web", sProcessInfo)
                    Me.TidyMenunode(oElmt.FirstChild, "OPEN", "", bPruneEvenIfInAdminMode Or Not (mbAdminMode), nUserId, cMenuItemNodeName, cRootNodeName)

                    For Each oMenuItem In oElmt.SelectNodes("descendant-or-self::" & cMenuItemNodeName & " | descendant-or-self::PageVersion")
                        'For Each oMenuItem In oElmt.SelectNodes("descendant-or-self::*")
                        Dim oDesc As XmlElement = oMenuItem.SelectSingleNode("Description")
                        If Not oDesc Is Nothing Then

                            ' First try to convert the node into Xml
                            sContent = oDesc.InnerText & ""
                            Try
                                oDesc.InnerXml = sContent
                            Catch
                                oDesc.InnerText = sContent
                            End Try

                            'we have a historic error where some sites have 2 description nodes in some database
                            'This should be data cleansed but for now we hack

                            ' Work through all the child nodes of Description and move them to the MenuItem
                            For Each oDescChild As XmlElement In oDesc.SelectNodes("*[name()!='Description' or (name()='Description' and not(preceding-sibling::Description))]")
                                ' Tidy the node
                                'sContent = oDescChild.InnerText
                                'If sContent <> "" Then
                                '    Try
                                '        oDescChild.InnerXml = Protean.Tools.Xml.convertEntitiesToCodes(sContent)
                                '    Catch
                                '        PerfMon.Log("Web", "GetStructureXML-tidy")
                                '        oDescChild.InnerXml = tidyXhtmlFrag(sContent)
                                '    End Try
                                'End If

                                ' Move the node
                                oMenuItem.InsertBefore(oDescChild.CloneNode(True), oDesc)
                            Next

                            ' Remove the original oDesc
                            oMenuItem.RemoveChild(oDesc)

                        End If

                        ' Remove the parId attribute
                        oMenuItem.RemoveAttribute("parId")

                    Next

                    If bLockRoot Then
                        ' LOCK THE ROOT
                        '===============
                        sProcessInfo = "GetStructureXML-lockRoot"
                        Protean.PerfMon.Log("Web", sProcessInfo)
                        Dim oMenuFirstChild As XmlElement = oElmt.FirstChild
                        oMenuFirstChild.SetAttribute("Locked", True)
                    End If

                Else

                    ' NO ROWS WERE FOUND - CREATE AN EMPTY Menu NODE
                    '===============================================
                    oElmt = moPageXml.CreateElement(cRootNodeName)

                End If

                ' CACHING - ADD THE STRUCTURE TO THE CACHE
                ' ========================================
                ' Only if caching is on, user is logged on, and not in AdminMode.
                If bUseCache And cCacheMode = "on" Then
                    sProcessInfo = "GetStructureXML-addCacheToStructure"
                    Protean.PerfMon.Log("Web", sProcessInfo)

                    'only cache if MenuItem / Menu
                    If cMenuItemNodeName = "MenuItem" And cRootNodeName = "Menu" Then
                        If mbAdminMode Then
                            goApp("AdminStructureCache") = oElmt.InnerXml
                        Else
                            moDbHelper.addStructureCache(bAuth, nUserId, cCacheType, oElmt.FirstChild)
                        End If
                    Else
                        moDbHelper.addStructureCache(bAuth, nUserId, cCacheType, oElmt.FirstChild)

                    End If



                    'sSql = "INSERT INTO dbo.tblXmlCache (cCacheSessionID,nCacheDirId,cCacheStructure,cCacheType) " _
                    '        & "VALUES (" _
                    '        & "'" & IIf(bAuth, Eonic.SqlFmt(moSession.SessionID), "") & "'," _
                    '        & Eonic.SqlFmt(nUserId) & "," _
                    '        & "'" & Eonic.SqlFmt(oElmt.InnerXml) & "'," _
                    '        & "'" & cCacheType & "'" _
                    '        & ")"
                    'moDbHelper.ExeProcessSql(sSql)

                End If

            End If

            'Now we need to do some page dependant processing

            ' MENU TIDY: XML TIDY
            ' ===================================
            sProcessInfo = "GetStructureXML-txt2xml"
            Protean.PerfMon.Log("Web", sProcessInfo)

            Dim sUrl As String
            Dim cPageName As String
            Dim cCloneParent As String
            Dim oRe As New Text.RegularExpressions.Regex("[^A-Z0-9]", Text.RegularExpressions.RegexOptions.IgnoreCase)
            Dim oPageVerElmts As XmlElement

            Dim DomainURL As String = moRequest.ServerVariables("HTTP_HOST")
            If moRequest.ServerVariables("SERVER_PORT_SECURE") = "1" Then
                DomainURL = "https://" & DomainURL
            Else
                DomainURL = "http://" & DomainURL
            End If

            For Each oMenuItem In oElmt.SelectNodes("descendant-or-self::" & cMenuItemNodeName)
                Dim urlPrefix As String = ""
                Dim verNodeLoop As XmlElement = Nothing
                Dim verNode As XmlElement = Nothing

                'Determine Page Version
                If Features.ContainsKey("PageVersions") Then
                    If Not (mbAdminMode) Then

                        'check for language version
                        For Each verNodeLoop In oMenuItem.SelectNodes("PageVersion[@lang='" & gcLang & "']")
                            verNode = verNodeLoop
                            urlPrefix = mcPageLanguageUrlPrefix
                        Next

                        'check for permission version
                        For Each verNodeLoop In oMenuItem.SelectNodes("PageVersion[@verType='1']")
                            If verNode Is Nothing Then verNode = verNodeLoop
                        Next

                        ' update our pageId
                        If Not verNode Is Nothing Then
                            'Case for if our version is also the root page
                            If nRootId = oMenuItem.GetAttribute("id") Then
                                nRootId = verNode.GetAttribute("id")
                            End If

                            'Case if we are on the current page then we reset the mnPageId so we pull in the right content
                            If mnPageId = oMenuItem.GetAttribute("id") Then
                                If verNode.GetAttribute("lang") = gcLang Or gcLang = "" Or verNode.GetAttribute("lang") = "" Then

                                    mnPageId = verNode.GetAttribute("id")
                                End If
                            End If

                            'create a version for the default we are replacing
                            Dim newVerNode As XmlElement = moPageXml.CreateElement("PageVersion")
                            newVerNode.SetAttribute("id", oMenuItem.GetAttribute("id"))
                            newVerNode.SetAttribute("name", oMenuItem.GetAttribute("name"))
                            newVerNode.SetAttribute("url", oMenuItem.GetAttribute("url"))
                            newVerNode.SetAttribute("publish", oMenuItem.GetAttribute("publish"))
                            newVerNode.SetAttribute("expire", oMenuItem.GetAttribute("expire"))
                            newVerNode.SetAttribute("status", oMenuItem.GetAttribute("status"))
                            newVerNode.SetAttribute("access", oMenuItem.GetAttribute("access"))
                            newVerNode.SetAttribute("layout", oMenuItem.GetAttribute("layout"))
                            If Not goLangConfig Is Nothing Then
                                newVerNode.SetAttribute("lang", goLangConfig.GetAttribute("code"))
                            End If

                            newVerNode.SetAttribute("verType", "0")
                            oMenuItem.AppendChild(newVerNode)

                            'now replace the menuitem with the node we are on
                            oMenuItem.SetAttribute("id", verNode.GetAttribute("id"))
                            oMenuItem.SetAttribute("name", verNode.GetAttribute("name"))
                            oMenuItem.SetAttribute("url", verNode.GetAttribute("url"))
                            oMenuItem.SetAttribute("publish", verNode.GetAttribute("publish"))
                            oMenuItem.SetAttribute("expire", verNode.GetAttribute("expire"))
                            oMenuItem.SetAttribute("status", verNode.GetAttribute("status"))
                            oMenuItem.SetAttribute("access", verNode.GetAttribute("access"))
                            oMenuItem.SetAttribute("layout", verNode.GetAttribute("layout"))
                            oMenuItem.SetAttribute("clone", verNode.GetAttribute("clone"))
                            oMenuItem.SetAttribute("lang", verNode.GetAttribute("lang"))
                            oMenuItem.SetAttribute("verDesc", verNode.GetAttribute("verDesc"))
                            oMenuItem.SetAttribute("verType", verNode.GetAttribute("verType"))
                            For Each oPageVerElmts In verNode.SelectNodes("*")
                                Dim nodeName As String = oPageVerElmts.Name
                                If Not oMenuItem.SelectSingleNode(nodeName) Is Nothing Then
                                    oMenuItem.SelectSingleNode(nodeName).InnerXml = verNode.SelectSingleNode(nodeName).InnerXml
                                Else
                                    oMenuItem.AppendChild(moPageXml.CreateElement(nodeName))
                                    oMenuItem.SelectSingleNode(nodeName).InnerXml = verNode.SelectSingleNode(nodeName).InnerXml
                                End If
                            Next
                            'If Not oMenuItem.SelectSingleNode("Description") Is Nothing Then
                            '    oMenuItem.SelectSingleNode("Description").InnerText = verNode.SelectSingleNode("Description").InnerText
                            'Else
                            '    oMenuItem.AppendChild(moPageXml.CreateElement("Description"))
                            '    oMenuItem.SelectSingleNode("Description").InnerText = verNode.SelectSingleNode("Description").InnerText
                            'End If
                        End If
                    Else
                        If moRequest("ewCmd") <> "LocateContent" And moRequest("ewCmd") <> "MoveContent" And moRequest("ewCmd") <> "EditStructure" Then
                            For Each verNode In oMenuItem.SelectNodes("PageVersion[@id=" & mnPageId & "]")
                                If oMenuItem.GetAttribute("id") = nRootId Then
                                    'case for replacing homepage in admin
                                    nRootId = verNode.GetAttribute("id")
                                End If
                                ' update menu item with current page
                                oMenuItem.SetAttribute("id", verNode.GetAttribute("id"))
                                oMenuItem.SetAttribute("name", verNode.GetAttribute("name"))
                                oMenuItem.SetAttribute("url", verNode.GetAttribute("url"))
                                oMenuItem.SetAttribute("publish", verNode.GetAttribute("publish"))
                                oMenuItem.SetAttribute("expire", verNode.GetAttribute("expire"))
                                oMenuItem.SetAttribute("status", verNode.GetAttribute("status"))
                                oMenuItem.SetAttribute("access", verNode.GetAttribute("access"))
                                oMenuItem.SetAttribute("layout", verNode.GetAttribute("layout"))
                                oMenuItem.SetAttribute("clone", IIf(verNode.GetAttribute("clone") = "", "0", verNode.GetAttribute("clone")))
                                oMenuItem.SetAttribute("lang", verNode.GetAttribute("lang"))
                                oMenuItem.SetAttribute("verDesc", verNode.GetAttribute("desc"))
                                oMenuItem.SetAttribute("verType", verNode.GetAttribute("verType"))
                                For Each oPageVerElmts In verNode.SelectNodes("*")
                                    Dim nodeName As String = oPageVerElmts.Name
                                    If Not oMenuItem.SelectSingleNode(nodeName) Is Nothing Then
                                        oMenuItem.SelectSingleNode(nodeName).InnerXml = verNode.SelectSingleNode(nodeName).InnerXml
                                    Else
                                        oMenuItem.AppendChild(moPageXml.CreateElement(nodeName))
                                        oMenuItem.SelectSingleNode(nodeName).InnerXml = verNode.SelectSingleNode(nodeName).InnerXml
                                    End If
                                Next
                                'If Not oMenuItem.SelectSingleNode("Description") Is Nothing Then
                                '    oMenuItem.SelectSingleNode("Description").InnerText = verNode.SelectSingleNode("Description").InnerText
                                'Else
                                '    oMenuItem.AppendChild(moPageXml.CreateElement("Description"))
                                '    oMenuItem.SelectSingleNode("Description").InnerText = verNode.SelectSingleNode("Description").InnerText
                                'End If
                            Next
                            verNode = Nothing
                        End If

                    End If
                End If

                If verNode Is Nothing And Not goLangConfig Is Nothing Then
                    urlPrefix = mcPageLanguageUrlPrefix '& cFilePathModifier
                End If



                ' Only generate URLs for MneuItems that do not already have a url explicitly defined
                If oMenuItem.GetAttribute("url") = "" Then

                    ' Start with the base path
                    sUrl = moConfig("BasePath") & urlPrefix & cFilePathModifier

                    If moConfig("UsePageIdsForURLs") = "on" Then
                        ' Use the page ID instead of a Pretty URL
                        sUrl = sUrl & "/?pgid=" & oMenuItem.GetAttribute("id")
                    Else
                        ' Get all the descendant menuitems and append the names onto the Url string
                        For Each oDescendant As XmlElement In oMenuItem.SelectNodes("ancestor-or-self::" & cMenuItemNodeName & "[ancestor::MenuItem[@id=" & nRootId & "]]")
                            If Not oDescendant.ParentNode.Name = "Menu" Then
                                If moConfig("PageURLFormat") = "hyphens" Then
                                    cPageName = oRe.Replace(oDescendant.GetAttribute("name"), "-")
                                Else
                                    cPageName = goServer.UrlEncode(oDescendant.GetAttribute("name"))
                                End If
                                sUrl = sUrl & "/" & cPageName
                            End If
                        Next
                    End If

                    If moConfig("TrailingSlash") = "on" Then
                        sUrl = "/" & sUrl.Trim("/") & "/"
                    End If

                    ' Account for a root url
                    If sUrl = "" Then
                        sUrl = "/"
                    End If

                    If sUrl = "//" Then
                        sUrl = "/"
                    End If

                    If sUrl = "/" Then
                        sUrl = DomainURL
                    End If

                    'for admin mode we tag the pgid on the end to be safe for duplicate pagenames with different permissions.
                    If mbAdminMode _
                        And moConfig("pageExt") = "" _
                        And moConfig("UsePageIdsForURLs") <> "on" _
                        Then sUrl = sUrl & "?pgid=" & oMenuItem.GetAttribute("id")

                    ' Address the context of the page
                    If gbClone Then
                        cCloneParent = oMenuItem.GetAttribute("cloneparent")
                        If IsNumeric(cCloneParent) AndAlso CInt(cCloneParent) > 0 Then
                            sUrl += IIf(sUrl.Contains("?"), "&", "?")
                            sUrl += "context=" + cCloneParent
                        End If
                    End If

                    oMenuItem.SetAttribute("url", sUrl)

                    'If this matches the path requested then change the pageId
                    If Not mbIgnorePath Then
                        If moRequest.QueryString.Count > 0 Then
                            If Not moRequest("path") Is Nothing Then
                                If Replace(sUrl, DomainURL, "") = moRequest("path") Or Replace(sUrl, DomainURL, "") & "/" = moRequest("path") Then
                                    If Not oMenuItem.SelectSingleNode("ancestor-or-self::MenuItem[@id=" & nRootId & "]") Is Nothing Then
                                        'case for if newsletter has same page name as menu item
                                        mnPageId = oMenuItem.GetAttribute("id")
                                    End If

                                End If
                            End If
                        End If
                    End If
                    'set the URL for each language pageversion so we can link between
                    Dim parPvElmt As XmlElement

                    For Each pvElmt In oMenuItem.SelectNodes("PageVersion")
                        Dim pageLang As String = pvElmt.GetAttribute("lang")
                        If pageLang <> "" Then
                            sUrl = ""
                            For Each oDescendant As XmlElement In oMenuItem.SelectNodes("ancestor-or-self::" & cMenuItemNodeName)
                                If Not oDescendant.ParentNode.Name = "Menu" Then
                                    cPageName = Nothing
                                    For Each parPvElmt In oDescendant.SelectNodes("PageVersion[@lang='" & pageLang & "']")
                                        If moConfig("PageURLFormat") = "hyphens" Then
                                            cPageName = oRe.Replace(parPvElmt.GetAttribute("name"), "-")
                                        Else
                                            cPageName = goServer.UrlEncode(parPvElmt.GetAttribute("name"))
                                        End If
                                        ' I know this means we get the last one but we should only have one anyway.
                                    Next
                                    If cPageName Is Nothing Then
                                        If moConfig("PageURLFormat") = "hyphens" Then
                                            cPageName = oRe.Replace(oDescendant.GetAttribute("name"), "-")
                                        Else
                                            cPageName = goServer.UrlEncode(oDescendant.GetAttribute("name"))
                                        End If
                                    End If
                                    sUrl = sUrl & "/" & cPageName
                                End If
                            Next

                            Dim pvUrlPrefix As String = ""
                            If Not goLangConfig Is Nothing Then
                                'Check Language by Domain
                                Dim oLangElmt As XmlElement
                                Dim httpStart As String
                                If moRequest.ServerVariables("SERVER_PORT_SECURE") = "1" Then
                                    httpStart = "https://"
                                Else
                                    httpStart = "http://"
                                End If

                                If Not goLangConfig.SelectSingleNode("Language[@code='" & pageLang & "']") Is Nothing Then
                                    For Each oLangElmt In goLangConfig.SelectNodes("Language[@code='" & pageLang & "']")
                                        Select Case LCase(oLangElmt.GetAttribute("identMethod"))
                                            Case "domain"
                                                pvUrlPrefix = httpStart & oLangElmt.GetAttribute("identifier")
                                            Case "path"
                                                pvUrlPrefix = httpStart & goLangConfig.GetAttribute("defaultDomain") & "/" & oLangElmt.GetAttribute("identifier")
                                        End Select
                                    Next
                                End If

                                If pvUrlPrefix = "" Then pvUrlPrefix = httpStart & goLangConfig.GetAttribute("defaultDomain")
                                pvElmt.SetAttribute("url", pvUrlPrefix & sUrl)

                            End If


                        End If


                    Next

                    If mnPageId = oMenuItem.GetAttribute("id") Then
                        mcPageURL = sUrl
                    End If

                End If

            Next



            ' GET THE ROOT NODE
            ' ==================
            ' get the Menu from the site root.
            Protean.PerfMon.Log("Web", "GetStructureXML-rootnode")
            If nRootId > 0 Then
                Dim cCloneModifier As String = ""
                If nCloneContextId > 0 Then
                    cCloneModifier = " and @cloneparent='" & nCloneContextId & "'"
                End If

                Dim oRoot As XmlElement = oElmt.SelectSingleNode("descendant-or-self::" & cMenuItemNodeName & "[@id='" & nRootId & "'" & cCloneModifier & "]")
                If oRoot Is Nothing Then
                    'Return error for when root page id does not exist.
                    'Dim cErrorMsg As String = "EonicWeb Config Error: The root page id does not exist, it may be hidden."
                    'cErrorMsg += " Root Id: " & nRootId & ";"
                    'cErrorMsg += " Top Level Id: " & RootPageId & ";"
                    'cErrorMsg += " User Id: " & nUserId & ";"
                    'cErrorMsg += " Clone Context Id: " & nCloneContextId & ";"
                    'cErrorMsg += " Menu Name: " & cMenuId & ";"
                    'Err.Raise(1001, "GetStructure", cErrorMsg)
                Else
                    oElmt.InnerXml = oRoot.OuterXml
                End If
            Else
                oElmt.InnerXml = oElmt.FirstChild.FirstChild.OuterXml
            End If

            ' MENU - ADD ID ATTRIBUTE
            ' ========================
            If cMenuId <> "" Then
                oElmt.SetAttribute("id", cMenuId)
            End If

            ' MENU - ADD THE MENU TO THE PAGE XML
            ' ===================================
            If bAddMenuToPageXML AndAlso Not moPageXml.DocumentElement Is Nothing Then
                sProcessInfo = "GetStructureXML-addMenuToPageXML"
                Protean.PerfMon.Log("Web", sProcessInfo)
                ' Check if there's already a menu node.
                If moPageXml.SelectSingleNode("/Page/" & cRootNodeName) Is Nothing Then
                    ' No menu node - add it to the pagexml
                    moPageXml.DocumentElement.AppendChild(oElmt)
                Else
                    ' Menu node found - add it after the last Menu node
                    moPageXml.DocumentElement.InsertAfter(oElmt, moPageXml.SelectSingleNode("/Page/" & cRootNodeName & "[position()=last()]"))
                End If
            End If

            sProcessInfo = "GetStructureXML-End"
            Protean.PerfMon.Log("Web", sProcessInfo)

            Return oElmt

        Catch ex As Exception
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, cFunctionDef, ex, sProcessInfo))
            Return Nothing
        End Try

    End Function


    Public Overridable Sub AddContentCount(ByVal oMenu As XmlElement, ByVal SchemaType As String)
        Dim oElmt As XmlElement
        Dim cSQL As String
        Try
            cSQL = "select COUNT(nContentKey) as count,nStructId from tblContentLocation cl" &
                " inner join tblContent c on c.nContentKey = cl.nContentId" &
                " inner join tblAudit a on c.nAuditId = a.nAuditKey" &
                " where c.cContentSchemaName = '" & Trim(SchemaType) & "' " &
                moConfig("MenuContentCountWhere") &
                  GetStandardFilterSQLForContent(True) &
                " group by nStructId"


            Dim oDR As Data.SqlClient.SqlDataReader = moDbHelper.getDataReader(cSQL)
            Do While oDR.Read
                oElmt = oMenu.SelectSingleNode("descendant::MenuItem[@id='" & oDR("nStructId") & "']")
                If Not oElmt Is Nothing Then
                    Dim newElmt As XmlElement = moPageXml.CreateElement("ContentCount")
                    newElmt.SetAttribute("type", Trim(SchemaType))
                    newElmt.SetAttribute("count", oDR("count"))
                    oElmt.AppendChild(newElmt)
                End If

            Loop

        Catch ex As Exception
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "AddContentCount", ex, ""))
        End Try

    End Sub

    Public Overridable Sub AddContentBrief(ByVal oMenu As XmlElement, ByVal SchemaType As String)
        Dim oElmt As XmlElement
        Dim oRoot As XmlElement
        Dim parId As String
        Dim ParentMenuElmt As XmlElement
        Try

            oRoot = moPageXml.CreateElement("Contents")

            GetMenuContentFromSelect("cContentSchemaName = '" & SchemaType & "'", False, 0, False, 0, , oRoot)

            For Each oElmt In oRoot.SelectNodes("*")
                parId = oElmt.GetAttribute("locId")
                ParentMenuElmt = oMenu.SelectSingleNode("descendant-or-self::MenuItem[@id='" & parId & "']")
                If Not ParentMenuElmt Is Nothing Then
                    ParentMenuElmt.AppendChild(oElmt)
                End If
            Next

        Catch ex As Exception
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "AddContentCount", ex, ""))
        End Try

    End Sub

    ''' <summary>
    ''' Takes a menu node, and removes the denied nodes
    ''' 
    ''' A note on how permissions are inherited.
    ''' If a page is OPEN, then it will inherit the permission.
    ''' If a page has a parent permission of DENIED then it will be denied.
    ''' </summary>
    ''' <param name="oMenuItem"></param>
    ''' <param name="cPerm"></param>
    ''' <param name="cPermSource"></param>
    ''' <param name="bPruneDenied"></param>
    ''' <remarks></remarks>
    Private Sub TidyMenunode(ByRef oMenuItem As XmlElement, ByVal cPerm As String, ByVal cPermSource As String, ByVal bPruneDenied As Boolean, ByVal nUserId As Long, ByVal cMenuItemNodeName As String, ByVal cRootNodeName As String)
        Dim cCurrentPerm As String
        Dim cCurrentPermSource As String
        Try
            For Each oChild As XmlElement In oMenuItem.SelectNodes(cRootNodeName & " | " & cMenuItemNodeName)
                If bPruneDenied AndAlso oChild.GetAttribute("access").Contains("DENIED") Then
                    oMenuItem.RemoveChild(oChild)
                Else
                    ' Work out the child permissions
                    cCurrentPerm = cPerm
                    cCurrentPermSource = cPermSource

                    If _
                            cCurrentPerm.Contains("DENIED") _
                        Or
                            (
                                oChild.GetAttribute("access").Contains("OPEN") _
                                And Not (cCurrentPerm.Contains("OPEN"))
                            ) Then

                        ' If parent is DENIED,
                        ' OR the child page is OPEN but the parent is not
                        ' THEN set the child page permission to be the INHERITED parent perm and the source.

                        If Not cCurrentPerm.Contains("INHERITED") Then cCurrentPerm = "INHERITED " & cCurrentPerm


                    Else
                        ' ELSE set perm and source variables to the current ones
                        cCurrentPerm = oChild.GetAttribute("access")

                        If oChild.GetAttribute("accessSourceId") = nUserId.ToString Then
                            cCurrentPermSource = ""

                        Else
                            If cCurrentPerm = "DENIED" Then cCurrentPerm = "IMPLIED " & cCurrentPerm
                            cCurrentPermSource = oChild.GetAttribute("accessSource")
                            If cCurrentPermSource <> "" And Not cCurrentPerm.Contains("DENIED") Then cCurrentPerm += " by " & cCurrentPermSource
                        End If

                    End If

                    oChild.SetAttribute("access", cCurrentPerm)

                    oChild.RemoveAttribute("accessSource")
                    oChild.RemoveAttribute("accessSourceId")

                    TidyMenunode(oChild, cCurrentPerm, cCurrentPermSource, bPruneDenied, nUserId, cMenuItemNodeName, cRootNodeName)
                End If
            Next

        Catch ex As Exception
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "TidyMenunode", ex, ""))
        End Try
    End Sub

    Public Sub addPageDetailLinksToStructure(ByVal cContentTypes As String)
        Dim cProcessInfo As String
        Try
            Dim oMenuElmt As XmlElement = moPageXml.DocumentElement.SelectSingleNode("Menu")
            If oMenuElmt Is Nothing Then Exit Sub

            Dim cContentTypesArray() As String = cContentTypes.Split(",")
            cContentTypes = ""
            For Each cContentType As String In cContentTypesArray
                cContentTypes &= Protean.Tools.Database.SqlString(cContentType.Trim()) & ","
            Next

            cContentTypes = cContentTypes.TrimEnd(",")

            Dim MenuItem As XmlElement
            Dim pageDict As New SortedDictionary(Of Long, String)
            For Each MenuItem In oMenuElmt.SelectNodes("descendant-or-self::MenuItem")
                pageDict.Add(MenuItem.GetAttribute("id"), MenuItem.GetAttribute("url"))
            Next
            'Dim keys As List(Of Long) = pageDict.KeyCollection
            ' keys.Sort()

            'AG 19-Jan-2010 - Replaced with above code to add protection against SQL Injections
            'cContentTypes = cContentTypes.Replace(",", "','")
            'cContentTypes = "'" & cContentTypes & "'"
            'cContentTypes = cContentTypes.Replace("''", "'")


            Dim sProcessInfo As String = "addPageDetailLinksToStructure"
            Dim cSQL As String = "SELECT tblContent.nContentKey, tblContent.cContentName, tblContentLocation.nStructId, tblAudit.dPublishDate, tblAudit.dUpdateDate, tblContent.cContentSchemaName" &
            " FROM tblContent INNER JOIN" &
            " tblAudit ON tblContent.nAuditId = tblAudit.nAuditKey INNER JOIN" &
            " tblContentLocation ON tblContent.nContentKey = tblContentLocation.nContentId" &
            " WHERE (tblContentLocation.bPrimary = 1) AND (tblAudit.nStatus = 1) AND (tblAudit.dPublishDate <= " & Protean.Tools.Database.SqlDate(mdDate) & " or tblAudit.dPublishDate is null) AND " &
            " (tblAudit.dExpireDate >= " & Protean.Tools.Database.SqlDate(mdDate) & " or tblAudit.dExpireDate is null) AND (tblContent.cContentSchemaName IN (" & cContentTypes & ")) "


            Dim oDR As Data.SqlClient.SqlDataReader = moDbHelper.getDataReader(cSQL)

            Dim oRe As New Text.RegularExpressions.Regex("[^A-Z0-9]", Text.RegularExpressions.RegexOptions.IgnoreCase)

            Do While oDR.Read
                Dim cURL As String = ""
                Dim oContElmt As XmlElement = moPageXml.CreateElement("MenuItem")

                Select Case moConfig("DetailPathType")
                    Case "ContentType/ContentName"
                        Dim prefixs() As String = moConfig("DetailPrefix").Split(",")
                        Dim thisPrefix As String = ""
                        Dim thisContentType As String = ""
                        Dim i As Integer
                        For i = 0 To prefixs.Length - 1
                            thisPrefix = prefixs(i).Substring(0, prefixs(i).IndexOf("/"))
                            thisContentType = prefixs(i).Substring(prefixs(i).IndexOf("/") + 1, prefixs(i).Length - prefixs(i).IndexOf("/") - 1)
                            If thisContentType = oDR(5).ToString() Then
                                cURL = "/" & thisPrefix & "/" & oRe.Replace(oDR(1).ToString, "-").Trim("-")
                                If moConfig("DetailPathTrailingSlash") = "on" Then
                                    cURL = cURL + "/"
                                End If

                            End If
                        Next
                    Case Else
                        If pageDict.ContainsKey(oDR(2)) Then
                            cURL = pageDict.Item(oDR(2))
                            'If moConfig("LegacyRedirect") = "on" Then
                            cURL &= "/" & oDR(0).ToString & "-/" & oRe.Replace(oDR(1).ToString, "-").Trim("-")
                            ' Else
                            '     cURL &= "/Item" & oDR(0).ToString
                            ' End If
                        Else
                            cProcessInfo = "orphan Content"
                        End If
                End Select
                If cURL <> "" Then
                    oContElmt.SetAttribute("url", cURL)
                    oContElmt.SetAttribute("name", oDR(1).ToString)
                    oContElmt.SetAttribute("publish", Protean.Tools.Xml.XmlDate(oDR(3).ToString, False))
                    oContElmt.SetAttribute("update", Protean.Tools.Xml.XmlDate(oDR(4).ToString, False))
                    oMenuElmt.AppendChild(oContElmt)
                End If

            Loop
            oDR.Close()
            oDR = Nothing

        Catch ex As Exception
            'returnException(mcModuleName, "addPageDetailLinksToStructure", ex, gcEwSiteXsl, "", gbDebug)
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "addPageDetailLinksToStructure", ex, cProcessInfo))
        End Try
    End Sub


    Public Overridable Sub AddContentXml(ByRef oContentElmt As XmlElement)
        PerfMon.Log("Web", "AddContentXml")
        Dim oContents As XmlElement
        Dim sProcessInfo As String = "Add Content XML"

        Try
            If oContentElmt Is Nothing Then Exit Sub
            oContents = moPageXml.DocumentElement.SelectSingleNode("Contents")
            If oContents Is Nothing Then
                oContents = moPageXml.CreateElement("Contents")
                moPageXml.DocumentElement.AppendChild(oContents)
            End If

            oContents.AppendChild(oContentElmt)

        Catch ex As Exception
            'returnException(mcModuleName, "AddContentXml", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "AddContentXml", ex, sProcessInfo))
        End Try

    End Sub

    Public Sub GetContentXml(ByRef oPageElmt As XmlElement)
        PerfMon.Log("Web", "GetContentXml")

        Dim oElmt As XmlElement

        Dim sNodeName As String = ""
        Dim cXPathModifier As String = ""
        Dim sContent As String = ""
        Dim IsInTree As String = False
        Dim sProcessInfo As String = "building the Content XML"

        Try


            If Not mbSystemPage Then
                ' Clone page adjustment
                If gbClone Then

                    If Me.mbIsClonePage Then
                        ' Is this a clone page that directly references its cloned page.
                        cXPathModifier = " and @clone='" & Me.mnClonePageId.ToString & "'"
                    ElseIf mnCloneContextPageId > 0 Then
                        ' Is this a child of a clone page
                        cXPathModifier = " and @cloneparent='" & Me.mnCloneContextPageId.ToString & "'"
                    Else
                        ' this is not a cloned page, make sure we don't accidentally look for the cloned pages.
                        cXPathModifier = " and ((not(@cloneparent) or @cloneparent=0) and (not(@clone) or @clone='' or @clone=0))"
                    End If
                End If

                ' Check for blocked content


                gcBlockContentType = moDbHelper.GetPageBlockedContent(mnPageId)

                oPageElmt.SetAttribute("blockedContent", gcBlockContentType)
                'step through the tree from home to our current page
                For Each oElmt In oPageElmt.SelectNodes("/Page/Menu/descendant-or-self::MenuItem[descendant-or-self::MenuItem[@id='" & mnPageId & "'" & cXPathModifier & "]]")
                    GetPageContentXml(oElmt.GetAttribute("id"))
                    IsInTree = True
                Next

                If mbPreview And IsInTree = False Then
                    GetPageContentXml(mnPageId)
                End If

                If Features.ContainsKey("PageVersions") Then
                    If IsInTree = False And mbAdminMode = True Then
                        GetPageContentXml(mnPageId)
                    End If
                End If
            Else
                'if we are on a system page we only want the content on that page not parents.
                GetPageContentXml(mnPageId)
                oPageElmt.SetAttribute("systempage", "true")

            End If

            ' Check content versions
            ' If gbVersionControl Then CheckContentVersions()
            ' moved to GetPageXml because content may be added from module overloads or grabbers etc.


            'Always ensure we have a content node
            Dim oRoot As XmlElement = moPageXml.DocumentElement.SelectSingleNode("Contents")
            If oRoot Is Nothing Then
                oRoot = moPageXml.CreateElement("Contents")
                moPageXml.DocumentElement.AppendChild(oRoot)
            End If



        Catch ex As Exception
            'returnException(mcModuleName, "getContentXml", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetContentXml", ex, sProcessInfo))
        End Try

    End Sub

    Public Overridable Sub GetPageContentXml(ByVal nPageId As Long)
        PerfMon.Log("Web", "GetPageContentXml")
        Dim sProcessInfo As String = "Getting the content from page " & nPageId
        Dim sSql As String = ""
        Dim sFilterSql As String = ""
        Dim sSql2 As String = ""
        Dim oRoot As XmlElement
        Try

            ' Create the live filter
            sFilterSql = GetStandardFilterSQLForContent()

            oRoot = moPageXml.DocumentElement.SelectSingleNode("Contents")
            If oRoot Is Nothing Then
                oRoot = moPageXml.CreateElement("Contents")
                moPageXml.DocumentElement.AppendChild(oRoot)
            End If

            Dim nCurrentPageId As String = nPageId

            ' Adjust the page id if it's a cloned page.
            If nCurrentPageId <> mnPageId Then
                'we are only pulling in cascaded items
                sFilterSql &= " and CL.bCascade = 1 and CL.bPrimary = 1 "
            Else
                'we are pulling in located and native items but not cascaded
            End If

            ' Check if the page is a cloned page
            ' If it is, then we need to switch the page id.
            If gbClone Then
                Dim nClonePage As Integer = moDbHelper.getClonePageID(nPageId)
                If nClonePage > 0 Then nPageId = nClonePage
            End If

            'sSql = "select c.nContentKey as id, (select TOP 1 CL2.nStructId from tblContentLocation CL2 where CL2.nContentId=c.nContentKey and CL2.bPrimary = 1) as parId, cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire from tblContent c inner join tblContentLocation CL on c.nContentKey = CL.nContentId inner join tblAudit a on c.nAuditId = a.nAuditKey" & _

            If gcBlockContentType <> "" Then
                sFilterSql = sFilterSql & " and c.cContentSchemaName NOT IN ('" & gcBlockContentType.Replace(",", "','") & "') "
            End If
            Dim cContentLimit As String = ""
            If moConfig("ContentLimit") <> "" And IsNumeric(moConfig("ContentLimit")) Then
                cContentLimit = " TOP " & moConfig("ContentLimit") & " "
            End If

            sSql = "select " & cContentLimit & "c.nContentKey as id, dbo.fxn_getContentParents(c.nContentKey) as parId ,cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, CL.cPosition as position from tblContent c" &
                    " inner join tblContentLocation CL on c.nContentKey = CL.nContentId" &
                    " inner join tblAudit a on c.nAuditId = a.nAuditKey" &
                    " where( CL.nStructId = " & nPageId

            sSql = sSql & sFilterSql & ") order by type, cl.nDisplayOrder"

            Dim oDs As DataSet = New DataSet
            oDs = moDbHelper.GetDataSet(sSql, "Content", "Contents")
            PerfMon.Log("Web", "AddDataSetToContent - For Page ", sSql)
            moDbHelper.AddDataSetToContent(oDs, oRoot, nCurrentPageId, False, "", mdPageExpireDate, mdPageUpdateDate)

            'If gbCart Or gbQuote Then
            '    moDiscount.getAvailableDiscounts(oRoot)
            'End If


            '  AddGroupsToContent(oRoot)



        Catch ex As Exception
            'returnException(mcModuleName, "getPageContentXml", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetPagecontentXml", ex, sProcessInfo))
        End Try

    End Sub

    Public Sub AddGroupsToContent(ByRef oContentElmt As XmlElement)
        ' Adds ProductGroups to content nodes
        PerfMon.Log("Web", "AddGroupsToContent-start")
        Try

            ' TS This is hammering performance on sites with lots of discount rules and doesn't seem to be used anywhere ?
            ' if this is requred we need only pull back categories for (products or other content names sold) that are on the current page seriously reducing the time required.

            'build In statement

            Dim oContElmt As XmlElement
            Dim InStatement As String = " tblCartCatProductRelations.nContentId IN("
            For Each oContElmt In oContentElmt.SelectNodes("Content")
                InStatement = InStatement + oContElmt.GetAttribute("id") + ","
            Next
            InStatement = InStatement.Trim(",") & ")"

            Dim cSQL As String = "SELECT tblCartProductCategories.nCatKey AS id, tblCartProductCategories.cCatSchemaName As type, tblCartProductCategories.nCatParentId As parent, " &
            " tblCartProductCategories.cCatName As name, tblCartProductCategories.cCatDescription As description, tblCartCatProductRelations.nContentId" &
            " FROM tblCartProductCategories INNER JOIN" &
            " tblAudit On tblCartProductCategories.nAuditId = tblAudit.nAuditKey INNER JOIN" &
            " tblCartCatProductRelations On tblCartProductCategories.nCatKey = tblCartCatProductRelations.nCatId"
            If Not Me.mbAdminMode Then
                cSQL &= " WHERE tblAudit.nStatus = 1 " &
                " And (tblAudit.dPublishDate Is null Or tblAudit.dPublishDate = 0 Or tblAudit.dPublishDate <= " & Protean.Tools.Database.SqlDate(Now) & " )" &
                " And (tblAudit.dExpireDate Is null Or tblAudit.dExpireDate = 0 Or tblAudit.dExpireDate >= " & Protean.Tools.Database.SqlDate(Now) & " )" &
                " And " + InStatement
            Else
                cSQL &= " where " + InStatement
            End If
            Dim oDS As New DataSet
            oDS = moDbHelper.GetDataSet(cSQL, "ContentGroup", "ContentGroups")
            Dim oDR As DataRow

            PerfMon.Log("Web", "AddGroupsToContent-startloop-" & oDS.Tables("ContentGroup").Rows.Count)

            For Each oDR In oDS.Tables("ContentGroup").Rows

                For Each oContElmt In oContentElmt.SelectNodes("Content[@id='" & oDR("nContentId") & "']")
                    Dim oGroupElmt As XmlElement = oContentElmt.OwnerDocument.CreateElement("ContentGroup")
                    oGroupElmt.SetAttribute("id", oDR("id") & "")
                    oGroupElmt.SetAttribute("type", oDR("type") & "")
                    oGroupElmt.SetAttribute("name", oDR("name") & "")
                    oGroupElmt.SetAttribute("parent", oDR("parent") & "")
                    oGroupElmt.InnerText = oDR("description") & ""
                    oContElmt.AppendChild(oGroupElmt)
                Next
            Next
            PerfMon.Log("Web", "AddGroupsToContent-end")

        Catch ex As Exception
            'returnException(mcModuleName, "AddGroupsToConent", ex, gcEwSiteXsl, "", gbDebug)
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "AddGroupsToContent", ex, ""))
        End Try
    End Sub



    Public Function GetContentUrl(ByVal nContentId As Long) As String
        PerfMon.Log("Web", "GetContentUrl")
        Dim sSql As String
        Dim sFilterSql As String
        Dim oRow As DataRow
        Dim ContentName As String = ""
        Dim ParentPages() As String
        Dim PrimaryPageId As Long
        Dim ContentURL As String = "/" & CStr(nContentId) & "-/"
        Dim sProcessInfo As String = "Getting path for content id " & nContentId
        Try
            sFilterSql = GetStandardFilterSQLForContent()

            sSql = "select c.nContentKey as id, dbo.fxn_getContentParents(c.nContentKey) as parId ,cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, CL.cPosition as position from tblContent c" &
                    " left outer join tblContentLocation CL on c.nContentKey = CL.nContentId and CL.bPrimary = 1" &
                    " inner join tblAudit a on c.nAuditId = a.nAuditKey" &
                    " where(c.nContentKey = " & nContentId & sFilterSql & ") order by type, cl.nDisplayOrder"

            Dim oDs As DataSet = New DataSet
            oDs = moDbHelper.GetDataSet(sSql, "Content", "Contents")

            If oDs.Tables.Count > 0 AndAlso oDs.Tables(0).Rows.Count > 0 Then

                For Each oRow In oDs.Tables(0).Rows

                    If oRow("type") = "SKU" Then
                        'get the id of the parent product

                        sSql = "select c.nContentKey as id, dbo.fxn_getContentParents(c.nContentKey) as parId ,cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, CL.cPosition as position from tblContent c" &
                                " inner join tblContentLocation CL on c.nContentKey = CL.nContentId" &
                                " inner join tblAudit a on c.nAuditId = a.nAuditKey" &
                                " inner join tblContentRelation cr on c.nContentKey = cr.nContentParentId" &
                                " where( cr.nContentChildId = " & nContentId
                        sSql = sSql & sFilterSql & " and CL.bPrimary = 1) order by type, cl.nDisplayOrder"
                        Dim oDs2 As DataSet = New DataSet

                        Dim oRow2 As DataRow
                        oDs2 = moDbHelper.GetDataSet(sSql, "Content", "Contents")
                        For Each oRow2 In oDs2.Tables(0).Rows
                            ContentName = oRow2("name")
                            If oRow2("parId").ToString.Contains(",") Then
                                ParentPages = oRow2("parId").ToString.Split(",")
                                If ParentPages(0) > 0 Then
                                    PrimaryPageId = ParentPages(0)
                                End If
                            Else
                                If IsNumeric(oRow2("parId")) Then
                                    PrimaryPageId = oRow2("parId")
                                End If
                            End If
                            ContentURL = "/" & CStr(oRow2("id")) & "-/"
                        Next

                    Else

                        ContentName = oRow("name")
                        If oRow("parId").ToString.Contains(",") Then
                            ParentPages = oRow("parId").ToString.Split(",")
                            If ParentPages(0) > 0 Then
                                PrimaryPageId = ParentPages(0)
                            End If
                        Else
                            If IsNumeric(oRow("parId")) Then
                                PrimaryPageId = oRow("parId")
                            End If
                        End If
                    End If

                Next
            End If
                If PrimaryPageId > 0 Then
                Dim pageMenuElmt As XmlElement = moPageXml.SelectSingleNode("/Page/Menu/descendant-or-self::MenuItem[@id='" & PrimaryPageId & "']")
                If Not pageMenuElmt Is Nothing Then
                    ContentURL = pageMenuElmt.GetAttribute("url") & ContentURL
                End If
            End If
            If ContentName <> "" Then
                ContentName = ContentName.Replace(" ", "-")
                ContentName = goServer.UrlEncode(ContentName)
                ContentURL = ContentURL & ContentName
            End If

            Return ContentURL

        Catch ex As Exception
            'returnException(mcModuleName, "getContentXml", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetContentXml", ex, sProcessInfo))
            Return ""
        End Try

    End Function
    ''' <summary>
    ''' The purpose of this function is to check if the user should be seeing a Pending version of the content
    ''' as opposed to the live one.  If the user is an admin or has AddUpdate permissions then this should be the case
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub CheckContentVersions()
        PerfMon.Log("Web", "CheckContentVersions")

        Dim cProcessInfo As String = ""

        Dim oDs As DataSet
        Dim cSql As String = ""
        Dim cOwnerFilter As String = ""

        Dim childNodes As XmlNodeList = Nothing
        Dim oReplacement As XmlElement

        Try

            ' We need to check two things
            ' - for content that hasn't been retrieved for this page, but is Pending (this will be in the content table)
            ' - for content that is on this page, but has a version on the Versions table called Pending.


            Dim nPagePermission As dbHelper.PermissionLevel = moDbHelper.getPagePermissionLevel(mnPageId)

            If dbHelper.CanAddUpdate(nPagePermission) Or Me.mbAdminMode Then

                Dim oTempNode As XmlElement = moPageXml.CreateElement("temp")

                ' Now check if it's everything we're getting, or just what the user created.
                If Not (Me.mbAdminMode) And nPagePermission = dbHelper.PermissionLevel.AddUpdateOwn Or nPagePermission = dbHelper.PermissionLevel.AddUpdateOwnPublish Then

                    ' Put a filter in to limit checks to content we've added
                    cOwnerFilter = " and @owner=" & mnUserId

                End If

                Dim filter As String = "[@id[number(.)=number(.)]" & cOwnerFilter & "]"

                Dim cCheckContentList As String = ""

                For Each oContent As XmlElement In moPageXml.SelectNodes("//Content" & filter)

                    ' Add the id if it is not null or zero
                    If Not (String.IsNullOrEmpty(oContent.GetAttribute("id"))) _
                        AndAlso IsNumeric(oContent.GetAttribute("id")) _
                        AndAlso CLng(oContent.GetAttribute("id")) > 0 Then
                        If Not (String.IsNullOrEmpty(cCheckContentList)) Then cCheckContentList &= ","
                        cCheckContentList &= oContent.GetAttribute("id")
                    End If

                Next

                If cCheckContentList <> "" Then

                    'Get any content on the page that has a Status Pending Version

                    cSql = "select c.nContentPrimaryId as id, " _
                         & "   dbo.fxn_getContentParents(c.nContentPrimaryId) as parId, " _
                         & "   cContentForiegnRef as ref, cContentName as name, " _
                         & "   cContentSchemaName as type, " _
                         & "   cContentXmlBrief as content, " _
                         & "   a.nStatus as status, " _
                         & "   a.dpublishDate as publish, " _
                         & "   a.dExpireDate as expire, " _
                         & "   a.dUpdateDate as [update], " _
                         & "   c.nVersion as version, " _
                         & "   c.nContentVersionKey as versionid, " _
                         & "   a.nInsertDirId as owner " _
                         & " from tblContentVersions c " _
                         & "   inner join tblAudit a " _
                         & "   on c.nAuditId = a.nAuditKey " _
                         & " WHERE c.nContentPrimaryId IN (" & cCheckContentList & ")" _
                         & "   AND a.nStatus = " & dbHelper.Status.Pending _
                         & " AND c.nVersion = (Select TOP 1 nVersion from tblContentVersions c2 where c.nContentPrimaryId = c2.nContentPrimaryId AND a.nStatus = " & dbHelper.Status.Pending & " ORDER BY nVersion DESC)"


                    oDs = moDbHelper.GetDataSet(cSql, "Content", "Contents")

                    If oDs.Tables.Count > 0 AndAlso oDs.Tables(0).Rows.Count > 0 Then

                        ' If contentversion exists, replace the current node with this.

                        oDs.Tables(0).Columns("id").ColumnMapping = Data.MappingType.Attribute

                        If oDs.Tables(0).Columns.Contains("parID") Then
                            oDs.Tables(0).Columns("parId").ColumnMapping = Data.MappingType.Attribute
                        End If

                        'Added to handle the new relationship type
                        If oDs.Tables(0).Columns.Contains("rtype") Then
                            oDs.Tables(0).Columns("rtype").ColumnMapping = Data.MappingType.Attribute
                        End If

                        With oDs.Tables(0)
                            .Columns("ref").ColumnMapping = Data.MappingType.Attribute
                            .Columns("name").ColumnMapping = Data.MappingType.Attribute
                            .Columns("type").ColumnMapping = Data.MappingType.Attribute
                            .Columns("status").ColumnMapping = Data.MappingType.Attribute
                            .Columns("publish").ColumnMapping = Data.MappingType.Attribute
                            .Columns("expire").ColumnMapping = Data.MappingType.Attribute
                            .Columns("owner").ColumnMapping = Data.MappingType.Attribute
                            .Columns("update").ColumnMapping = Data.MappingType.Attribute
                            .Columns("version").ColumnMapping = Data.MappingType.Attribute
                            .Columns("versionid").ColumnMapping = Data.MappingType.Attribute
                            .Columns("content").ColumnMapping = Data.MappingType.SimpleContent
                        End With

                        oDs.EnforceConstraints = False
                        'convert to Xml Dom
                        Dim oXml As XmlDataDocument = New XmlDataDocument(oDs)
                        oXml.PreserveWhitespace = False

                        Dim oReplaceContent As XmlElement

                        For Each oReplaceContent In oXml.SelectNodes("/Contents/Content")

                            Dim oContent As XmlElement = moPageXml.SelectSingleNode("//Content[@id='" & oReplaceContent.GetAttribute("id") & "']")

                            ' Before we replace the node, move any child Content nodes (i.e. related content) out from it.
                            childNodes = oContent.SelectNodes("Content")
                            If childNodes.Count > 0 Then
                                For Each child As XmlElement In childNodes
                                    oContent.RemoveChild(child)
                                    oTempNode.AppendChild(child)
                                Next
                            End If

                            ' Replace the contentNode
                            oReplacement = moPageXml.ImportNode(oReplaceContent, True)
                            oReplacement = moDbHelper.SimpleTidyContentNode(oReplacement)
                            oContent.ParentNode.ReplaceChild(oReplacement, oContent)

                            ' Move the related content back
                            If childNodes.Count > 0 Then
                                For Each child As XmlElement In childNodes
                                    oTempNode.RemoveChild(child)
                                    oReplacement.AppendChild(child)
                                Next
                            End If

                        Next
                    End If
                End If
            End If

        Catch ex As Exception
            'returnException(mcModuleName, "getContentXml", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "CheckContentVersions", ex, cProcessInfo))
        End Try

    End Sub

    Protected Sub CheckContentVersions_Replaced()
        PerfMon.Log("Web", "CheckContentVersions")

        Dim cProcessInfo As String = ""

        Dim oDs As DataSet
        Dim cSql As String = ""
        Dim cOwnerFilter As String = ""

        Dim childNodes As XmlNodeList = Nothing
        Dim oReplacement As XmlElement

        Try

            ' We need to check two things
            ' - for content that hasn't been retrieved for this page, but is Pending (this will be in the content table)
            ' - for content that is on this page, but has a version on the Versions table called Pending.


            Dim nPagePermission As dbHelper.PermissionLevel = moDbHelper.getPagePermissionLevel(mnPageId)

            If dbHelper.CanAddUpdate(nPagePermission) Or Me.mbAdminMode Then

                Dim oTempNode As XmlElement = moPageXml.CreateElement("temp")

                ' Now check if it's everything we're getting, or just what the user created.
                If Not (Me.mbAdminMode) And nPagePermission = dbHelper.PermissionLevel.AddUpdateOwn Or nPagePermission = dbHelper.PermissionLevel.AddUpdateOwnPublish Then

                    ' Put a filter in to limit checks to content we've added
                    cOwnerFilter = " and @owner=" & mnUserId

                End If

                Dim filter As String = "[@id[number(.)=number(.)]" & cOwnerFilter & "]"



                ' only search content that has an id which is numeric
                For Each oContent As XmlElement In moPageXml.SelectNodes("//Content" & filter)

                    ' Search for pending versions of the content

                    cSql = "select TOP 1 c.nContentPrimaryId as id, " _
                         & "   dbo.fxn_getContentParents(c.nContentPrimaryId) as parId, " _
                         & "   cContentForiegnRef as ref, cContentName as name, " _
                         & "   cContentSchemaName as type, " _
                         & "   cContentXmlBrief as content, " _
                         & "   a.nStatus as status, " _
                         & "   a.dpublishDate as publish, " _
                         & "   a.dExpireDate as expire, " _
                         & "   a.dUpdateDate as [update], " _
                         & "   c.nVersion as version, " _
                         & "   c.nContentVersionKey as versionid, " _
                         & "   a.nInsertDirId as owner " _
                         & " from tblContentVersions c " _
                         & "   inner join tblAudit a " _
                         & "     on c.nAuditId = a.nAuditKey " _
                         & " WHERE c.nContentPrimaryId = " & oContent.GetAttribute("id") _
                         & "   AND a.nStatus = " & dbHelper.Status.Pending _
                         & " ORDER BY c.nVersion DESC "

                    oDs = moDbHelper.GetDataSet(cSql, "Content", "Contents")

                    If oDs.Tables.Count > 0 AndAlso oDs.Tables(0).Rows.Count > 0 Then
                        ' If contentversion exists, replace the current node with this.


                        oDs.Tables(0).Columns("id").ColumnMapping = Data.MappingType.Attribute

                        If oDs.Tables(0).Columns.Contains("parID") Then
                            oDs.Tables(0).Columns("parId").ColumnMapping = Data.MappingType.Attribute
                        End If

                        'Added to handle the new relationship type
                        If oDs.Tables(0).Columns.Contains("rtype") Then
                            oDs.Tables(0).Columns("rtype").ColumnMapping = Data.MappingType.Attribute
                        End If

                        With oDs.Tables(0)
                            .Columns("ref").ColumnMapping = Data.MappingType.Attribute
                            .Columns("name").ColumnMapping = Data.MappingType.Attribute
                            .Columns("type").ColumnMapping = Data.MappingType.Attribute
                            .Columns("status").ColumnMapping = Data.MappingType.Attribute
                            .Columns("publish").ColumnMapping = Data.MappingType.Attribute
                            .Columns("expire").ColumnMapping = Data.MappingType.Attribute
                            .Columns("owner").ColumnMapping = Data.MappingType.Attribute
                            .Columns("update").ColumnMapping = Data.MappingType.Attribute
                            .Columns("version").ColumnMapping = Data.MappingType.Attribute
                            .Columns("versionid").ColumnMapping = Data.MappingType.Attribute
                            .Columns("content").ColumnMapping = Data.MappingType.SimpleContent
                        End With

                        oDs.EnforceConstraints = False
                        'convert to Xml Dom
                        Dim oXml As XmlDataDocument = New XmlDataDocument(oDs)
                        oXml.PreserveWhitespace = False


                        ' Before we replace the node, move any child Content nodes (i.e. related content) out from it.
                        childNodes = oContent.SelectNodes("Content")
                        If childNodes.Count > 0 Then
                            For Each child As XmlElement In childNodes
                                oContent.RemoveChild(child)
                                oTempNode.AppendChild(child)
                            Next
                        End If

                        ' Replace the contentNode
                        oReplacement = moPageXml.ImportNode(oXml.SelectSingleNode("/Contents/Content"), True)
                        oReplacement = moDbHelper.SimpleTidyContentNode(oReplacement)
                        oContent.ParentNode.ReplaceChild(oReplacement, oContent)

                        ' Move the related content back
                        If childNodes.Count > 0 Then
                            For Each child As XmlElement In childNodes
                                oTempNode.RemoveChild(child)
                                oReplacement.AppendChild(child)
                            Next
                        End If
                    End If


                Next


            End If


        Catch ex As Exception
            'returnException(mcModuleName, "getContentXml", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "CheckContentVersions", ex, cProcessInfo))
        End Try

    End Sub


    Public Sub GetErrorXml(ByRef oPageElmt As XmlElement)
        PerfMon.Log("Web", "GetErrorXml")
        Dim oRoot As XmlElement
        Dim oElmt As XmlElement
        Dim sFilterSql As String = ""
        Dim sSql2 As String = ""
        Dim sNodeName As String = ""
        Dim sContent As String = ""
        Dim sErrorModule As String = ""
        Dim sErrorHTML As String = ""
        Dim strMessageHtml As String = ""
        Dim strMessageText As String = ""

        Dim sProcessInfo As String = "building the Content XML"

        Try

            oRoot = moPageXml.DocumentElement.SelectSingleNode("Contents")
            If oRoot Is Nothing Then
                oRoot = moPageXml.CreateElement("Contents")
                moPageXml.DocumentElement.AppendChild(oRoot)
            End If

            oPageElmt.SetAttribute("systempage", "true")

            oElmt = moPageXml.CreateElement("Content")
            oElmt.SetAttribute("type", "Error")
            oElmt.SetAttribute("name", mnProteanCMSError)
            Select Case Me.mnProteanCMSError
                Case 1005
                    strMessageText = "Page Not Found"
                    strMessageHtml = "<div><h2>Page Not Found</h2>" &
                                    "</div>"
                    gnResponseCode = 404
                    sErrorModule = "BuildPageXml"
                    bPageCache = False
                Case 1006
                    strMessageText = "Access Denied"
                    strMessageHtml = "<div><h2>Access Denied</h2>" &
                                    "</div>"
                    'gnResponseCode = 401
                    sErrorModule = "BuildPageXml"

                    bPageCache = False
                Case 1007
                    strMessageText = "File Not Found"
                    strMessageHtml = "<div><h2>File Not Found</h2>" &
                                    "</div>"
                    'gnResponseCode = 404
                    sErrorModule = "Get Document"

                    bPageCache = False

                Case 1008
                    strMessageText = "Invalid Licence"
                    strMessageHtml = "<div><h2>Please get a valid ProteanCMS Licence</h2>" &
                                    "</div>"
                    sErrorModule = "BuildPageXml"

                    bPageCache = False
            End Select

            If gbDebug Then
                Try
                    Err.Raise(mnProteanCMSError, sErrorModule, strMessageText)
                Catch ex As Exception
                    oPageElmt.SetAttribute("layout", "Error")
                    oElmt.InnerXml = strMessageHtml
                End Try
            Else
                oPageElmt.SetAttribute("layout", "Error")
                oElmt.InnerXml = strMessageHtml
            End If

            oRoot.AppendChild(oElmt)
            oPageElmt.AppendChild(oRoot)



        Catch ex As Exception

            'returnException(mcModuleName, "GetErrorXml", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetErrorXml", ex, sProcessInfo))
        End Try

    End Sub

    Public Sub GetContentXMLByType(ByRef oPageElmt As XmlElement, ByVal cContentType As String, Optional sqlFilter As String = "", Optional fullSQL As String = "")
        PerfMon.Log("Web", "GetContentXMLByType")
        '<add key="ControlPanelTypes" value="Event,Document|Top_10|DESC_Publish"/>
        Try
            Dim oTypeCriteria() As String = Split(cContentType, "|")
            Dim cTop As String = ""
            Dim cOrderDirection As String = ""
            Dim oOrderField As String = ""
            Dim strContentType As String = ""

            Dim i As Integer
            For i = 0 To UBound(oTypeCriteria)
                If oTypeCriteria(i).Contains("Top_") Then
                    cTop = Split(oTypeCriteria(i), "_")(1)
                    If Not IsNumeric(cTop) Then cTop = ""
                ElseIf oTypeCriteria(i).Contains("ASC_") Then
                    cOrderDirection = ""
                    oOrderField = Split(oTypeCriteria(i), "_")(1)
                ElseIf oTypeCriteria(i).Contains("DESC_") Then
                    cOrderDirection = "DESC"
                    oOrderField = Split(oTypeCriteria(i), "_")(1)
                Else
                    'its the field name
                    strContentType = oTypeCriteria(i)
                End If
            Next

            Dim cSQL As String = "select c.nContentKey as id, dbo.fxn_getContentParents(c.nContentKey) as parId, cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, CL.cPosition as position from tblContent c left outer join tblContentLocation CL on c.nContentKey = CL.nContentId inner join tblAudit a on c.nAuditId = a.nAuditKey" &
            " where (cContentSchemaName = '" & strContentType & "') "
            If sqlFilter <> "" Then
                cSQL &= sqlFilter
            End If
            cSQL &= GetStandardFilterSQLForContent()

            If Not oOrderField = "" Then
                cSQL &= " ORDER BY " & oOrderField & " " & cOrderDirection
            End If

            If fullSQL <> "" Then
                cSQL = fullSQL
            End If

            Dim oDS As DataSet = moDbHelper.GetDataSet(cSQL, "Content1", "Contents")
            Dim oDT As New DataTable
            oDT = oDS.Tables("Content1").Copy()
            oDT.Rows.Clear()
            oDT.TableName = "Content"
            oDS.Tables.Add(oDT)
            Dim oDR As DataRow
            Dim nMax As Integer = 0
            Dim cDoneIds As String = ","
            Dim ochkStr As String = ""
            If IsNumeric(cTop) Then nMax = CInt(cTop)
            For Each oDR In oDS.Tables("Content1").Rows
                If oDS.Tables("Content").Rows.Count < nMax Or nMax = 0 Then
                    If IsNumeric(oDR("parId")) And Not oDR("parId").Contains(",") Then
                        ochkStr = moDbHelper.checkPagePermission(oDR("parId"))
                        If IsNumeric(ochkStr) Then
                            If CInt(ochkStr) = oDR("parId") And Not cDoneIds.Contains("," & oDR("id") & ",") Then
                                oDS.Tables("Content").ImportRow(oDR)
                                cDoneIds &= oDR("id") & ","
                            End If
                        End If
                    Else
                        If mbAdminMode Then 'if in adminmode get everything regardless
                            oDS.Tables("Content").ImportRow(oDR)
                            cDoneIds &= oDR("id") & ","
                        End If
                    End If

                End If
            Next
            oDS.Tables.Remove(oDS.Tables("Content1"))
            Dim oRoot As XmlElement
            oRoot = moPageXml.DocumentElement.SelectSingleNode("Contents")
            If oRoot Is Nothing Then
                oRoot = moPageXml.CreateElement("Contents")
                moPageXml.DocumentElement.AppendChild(oRoot)
            End If
            moDbHelper.AddDataSetToContent(oDS, oRoot, mnPageId, False, "", mdPageExpireDate, mdPageUpdateDate)

        Catch ex As Exception
            'returnException(mcModuleName, "getContentXml", ex, gcEwSiteXsl, "", gbDebug)
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetContentXMLByType", ex, ""))
        End Try
    End Sub

    Public Function GetContentDetailXml(Optional ByVal oPageElmt As XmlElement = Nothing, Optional ByVal nArtId As Long = 0, Optional ByVal disableRedirect As Boolean = False, Optional ByVal bCheckAccessToContentLocation As Boolean = False, Optional ByVal nVersionId As Long = 0) As XmlElement
        PerfMon.Log("Web", "GetContentDetailXml")
        Dim oRoot As XmlElement
        Dim oNode As XmlNode
        Dim oElmt As XmlElement
        Dim retElmt As XmlElement = Nothing
        Dim sContent As String
        Dim sSql As String
        Dim sProcessInfo As String = "GetContentDetailXml"
        Dim oDs As DataSet = New DataSet
        Dim sFilterSql As String = ""
        Dim bLoadAsXml As Boolean
        Dim oComment As XmlComment
        If nArtId > 0 Then
            mnArtId = nArtId
        End If
        Try
            If moContentDetail Is Nothing Then

                ' If requested, we need to make sure that the content we are looking for doesn't belong to a page
                ' that the user is not allowed to access.
                ' We can review the current menu structure xml instead of calling the super slow permissions functions.



                If bCheckAccessToContentLocation And mnArtId > 0 Then
                    If Not moDbHelper.checkContentLocationsInCurrentMenu(mnArtId, True) Then
                        mnArtId = 0
                    End If
                End If
                If mnArtId > 0 Then
                    sProcessInfo = "loading content" & mnArtId

                    sFilterSql &= GetStandardFilterSQLForContent()

                    oRoot = moPageXml.CreateElement("ContentDetail")

                    'check if new function exists in DB, this logic can be later deprecated when all db are inline.
                    Dim bContLoc As Boolean = moDbHelper.checkDBObjectExists("fxn_getContentLocations", Tools.Database.objectTypes.UserFunction)
                    If bContLoc Then
                        sSql = "select c.nContentKey as id, cContentForiegnRef as ref, dbo.fxn_getContentParents(c.nContentKey) as parId, dbo.fxn_getContentLocations(c.nContentKey) as locations, cContentName as name, cContentSchemaName as type, cContentXmlDetail as content, a.dpublishDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, a.nStatus as status from tblContent c "
                    Else
                        sSql = "select c.nContentKey as id, cContentForiegnRef as ref, dbo.fxn_getContentParents(c.nContentKey) as parId, cContentName as name, cContentSchemaName as type, cContentXmlDetail as content, a.dpublishDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, a.nStatus as status from tblContent c "
                    End If
                    sSql &= "inner join tblAudit a on c.nAuditId = a.nAuditKey  "
                    'sSql &= "inner join tblContentLocation CL on c.nContentKey = CL.nContentId "
                    sSql &= "where c.nContentKey = " & mnArtId & sFilterSql & " "
                    'sSql &= "and CL.nStructId = " & mnPageId

                    If nVersionId > 0 Then
                        sSql = "select c.nContentPrimaryId as id, nContentVersionKey as verid, nVersion as verno, cContentForiegnRef as ref, dbo.fxn_getContentParents(c.nContentPrimaryId) as parId, dbo.fxn_getContentLocations(c.nContentPrimaryId) as locations, cContentName as name, cContentSchemaName as type, cContentXmlDetail as content, a.dpublishDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, a.nStatus as status from tblContentVersions c "
                        sSql &= "inner join tblAudit a on c.nAuditId = a.nAuditKey  "
                        sSql &= "where c.nContentPrimaryId = " & mnArtId & " and nContentVersionKey=" & nVersionId & " "
                    End If

                    oDs = moDbHelper.GetDataSet(sSql, "Content", "ContentDetail")

                    oDs.Tables(0).Columns("id").ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns("ref").ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns("name").ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns("type").ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns("publish").ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns("expire").ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns("update").ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns("parId").ColumnMapping = Data.MappingType.Attribute

                    If nVersionId > 0 Then
                        oDs.Tables(0).Columns("verid").ColumnMapping = Data.MappingType.Attribute
                        oDs.Tables(0).Columns("verno").ColumnMapping = Data.MappingType.Attribute
                    End If
                    If bContLoc Then
                            oDs.Tables(0).Columns("locations").ColumnMapping = Data.MappingType.Attribute
                        End If
                        oDs.Tables(0).Columns("owner").ColumnMapping = Data.MappingType.Attribute
                        oDs.Tables(0).Columns("status").ColumnMapping = Data.MappingType.Attribute
                        oDs.Tables(0).Columns("content").ColumnMapping = Data.MappingType.SimpleContent

                        'Need to check the content is found on the current page.


                        If oDs.Tables(0).Rows.Count > 0 Then

                            oRoot.InnerXml = Replace(oDs.GetXml, "xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""", "")
                            For Each oNode In oRoot.SelectNodes("/ContentDetail/Content")
                                oElmt = oNode
                                sContent = oElmt.InnerText

                                If IsDate(oElmt.GetAttribute("update")) Then mdPageUpdateDate = CDate(oElmt.GetAttribute("update"))


                                '  Try to convert the InnerText to InnerXml
                                '  Also if the innerxml has Content as a first node, then get the innerxml of the content node.
                                Try
                                    oElmt.InnerXml = sContent
                                    bLoadAsXml = True

                                Catch ex As Exception
                                    ' If the load failed, then flag it in the Content node and return the InnerText as a Comment
                                    oComment = oRoot.OwnerDocument.CreateComment(oElmt.InnerText)
                                    oElmt.SetAttribute("xmlerror", "getContentBriefXml")
                                    oElmt.InnerXml = ""
                                    oElmt.AppendChild(oComment)
                                    oComment = Nothing
                                    bLoadAsXml = False
                                End Try

                                If bLoadAsXml Then

                                    ' Successfully converted to XML.
                                    ' Now check if the node imported is a Content node - if so get rid of the Content node
                                    Dim oFirst As XmlElement = Tools.Xml.firstElement(oElmt)
                                    'NB 19-02-2010 Added to stop unsupported types falling over
                                    If Not oFirst Is Nothing Then
                                        If oFirst.LocalName = "Content" Then
                                            Dim oAttr As XmlAttribute
                                            For Each oAttr In oElmt.SelectNodes("Content/@*")
                                                If oElmt.GetAttribute(oAttr.Name) = "" Then
                                                    oElmt.SetAttribute(oAttr.Name, oAttr.InnerText)
                                                End If
                                            Next

                                            oElmt.InnerXml = oFirst.InnerXml
                                        End If
                                    End If



                                    moDbHelper.addRelatedContent(oNode, mnArtId, mbAdminMode)
                                    If moConfig("ShowOwnerOnDetail") <> "" Then
                                        Dim cContentType As String = oElmt.GetAttribute("type")
                                        If moConfig("ShowOwnerOnDetail").Contains(cContentType) Then
                                            Dim nOwner As Long = CLng("0" & oElmt.GetAttribute("owner"))
                                            If nOwner > 0 Then
                                                oElmt.AppendChild(GetUserXML(nOwner))
                                            End If
                                        End If
                                    End If
                                End If

                            Next

                            'If gbCart Or gbQuote Then
                            '    moDiscount.getAvailableDiscounts(oRoot)
                            'End If

                            Dim contentElmt As XmlElement = oRoot.SelectSingleNode("/ContentDetail/Content")
                            Dim getSafeURLName As String = contentElmt.GetAttribute("name")

                            AddGroupsToContent(oRoot.SelectSingleNode("/ContentDetail"))

                            If Not oPageElmt Is Nothing Then
                                Dim oContentDetail As XmlElement = contentElmt
                                If Not (oContentDetail Is Nothing) _
                                AndAlso oContentDetail.InnerXml.Trim() <> "" Then

                                    ' If we can find a content detail Content node, 
                                    ' AND it contains some InnerXml, then YAY.
                                    oPageElmt.AppendChild(oRoot.FirstChild)
                                Else

                                    'OTHERWISE if there is nothing in the detail we get the brief instead.
                                    GetContentBriefXml(oPageElmt, nArtId)
                                End If
                            End If
                            retElmt = oRoot.FirstChild

                            If mbAdminMode = False And LCase(moConfig("RedirectToDescriptiveContentURLs")) = "true" Then

                                'get SAFE URL NAME
                                'getSafeURLName
                                ' <xsl:variable name="illegalString">
                                '  <xsl:text> /\.:£%&#34;&#147;&#148;&#39;&#8220;&#8221;&#8216;&#8217;</xsl:text>
                                '   </xsl:variable>
                                '<xsl:value-of select="translate(@name,$illegalString,'----')"/>

                                getSafeURLName = getSafeURLName.Replace(" ", "-")
                                getSafeURLName = getSafeURLName.Replace("/", "-")
                                getSafeURLName = getSafeURLName.Replace("\", "-")
                                getSafeURLName = getSafeURLName.Replace(".", "-")

                                getSafeURLName = getSafeURLName.Replace("+", "-")
                                getSafeURLName = getSafeURLName.Replace("""", "")
                                getSafeURLName = getSafeURLName.Replace("'", "")

                                Dim myOrigURL As String
                                Dim myQueryString As String = ""

                                If mcOriginalURL.Contains("?") Then
                                    myOrigURL = mcOriginalURL.Substring(0, mcOriginalURL.IndexOf("?"))
                                    myQueryString = mcOriginalURL.Substring(mcOriginalURL.LastIndexOf("?"))
                                Else
                                    myOrigURL = mcOriginalURL
                                End If

                                If myOrigURL <> mcPageURL & "/" & mnArtId & "-/" & getSafeURLName Then
                                    'we redirect perminently
                                    mbRedirectPerm = True
                                    msRedirectOnEnd = mcPageURL & "/" & mnArtId & "-/" & getSafeURLName & myQueryString
                                End If
                            End If
                            moContentDetail = oRoot.FirstChild
                            Return moContentDetail
                        Else
                            sProcessInfo = "no content to add - we redirect"
                            'this content is not found either page not found or re-direct home.
                            If Not disableRedirect Then
                                'put this in to prevent a redirect if we are calling this from somewhere strange.
                                If gnPageNotFoundId > 1 Then
                                    msRedirectOnEnd = "/System+Pages/Page+Not+Found"
                                    moResponse.StatusCode = 404
                                Else
                                    msRedirectOnEnd = moConfig("BaseUrl")
                                    moResponse.StatusCode = 404
                                End If
                            End If
                            Return Nothing
                        End If
                    Else
                        'Just a page no detail requested
                        Return Nothing
                End If
            Else
                sProcessInfo = "content exists adding content"
                oRoot = moContentDetail.OwnerDocument.CreateElement("ContentDetail")
                oRoot.AppendChild(moContentDetail)
                If Not oPageElmt Is Nothing Then
                    oPageElmt.AppendChild(oRoot)
                End If
                AddGroupsToContent(oRoot)
                retElmt = moContentDetail
                moDbHelper.CommitLogToDB(Cms.dbHelper.ActivityType.ContentDetailViewed, mnUserId, Me.SessionID, Now, mnArtId, 0, "")
                Return moContentDetail
            End If

            sSql = Nothing

        Catch ex As Exception
            'returnException(mcModuleName, "getContentDetailXml", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetContentDetailXml", ex, sProcessInfo))
            Return Nothing
        End Try

    End Function


    Public Function GetContentBriefXml(Optional ByVal oPageElmt As XmlElement = Nothing, Optional ByVal nArtId As Long = 0) As XmlElement
        PerfMon.Log("Web", "GetContentBriefXml")
        Dim oRoot As XmlElement
        Dim oNode As XmlNode
        Dim oElmt As XmlElement
        Dim retElmt As XmlElement = Nothing
        Dim sContent As String
        Dim sSql As String
        Dim sProcessInfo As String = "GetContentBriefXml"
        Dim oDs As DataSet = New DataSet
        Dim sFilterSql As String = ""
        Dim bLoadAsXml As Boolean
        Dim oComment As XmlComment
        If nArtId > 0 Then
            mnArtId = nArtId
        End If
        Try
            If moContentDetail Is Nothing Then
                If mnArtId > 0 Then
                    sProcessInfo = "loading content" & mnArtId

                    sFilterSql &= GetStandardFilterSQLForContent()


                    oRoot = moPageXml.CreateElement("ContentDetail")
                    sSql = "select c.nContentKey as id, cContentForiegnRef as ref, dbo.fxn_getContentParents(c.nContentKey) as parId, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.dpublishDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, a.nStatus as status " &
                    "from tblContent c inner join tblAudit a on c.nAuditId = a.nAuditKey  where c.nContentKey = " & mnArtId & sFilterSql



                    oDs = moDbHelper.GetDataSet(sSql, "Content", "ContentDetail")
                    oDs.Tables(0).Columns("id").ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns("ref").ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns("name").ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns("type").ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns("publish").ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns("expire").ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns("update").ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns("parId").ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns("owner").ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns("status").ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns("content").ColumnMapping = Data.MappingType.SimpleContent

                    oRoot.InnerXml = Replace(oDs.GetXml, "xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""", "")
                    For Each oNode In oRoot.SelectNodes("/ContentDetail/Content")
                        oElmt = oNode
                        sContent = oElmt.InnerText

                        If IsDate(oElmt.GetAttribute("update")) Then mdPageUpdateDate = CDate(oElmt.GetAttribute("update"))

                        '  Try to convert the InnerText to InnerXml
                        '  Also if the innerxml has Content as a first node, then get the innerxml of the content node.
                        Try
                            oElmt.InnerXml = sContent
                            bLoadAsXml = True

                        Catch ex As Exception
                            ' If the load failed, then flag it in the Content node and return the InnerText as a Comment
                            oComment = oRoot.OwnerDocument.CreateComment(oElmt.InnerText)
                            oElmt.SetAttribute("xmlerror", "getContentBriefXml")
                            oElmt.InnerXml = ""
                            oElmt.AppendChild(oComment)
                            oComment = Nothing
                            bLoadAsXml = False
                        End Try

                        If bLoadAsXml Then

                            ' Successfully converted to XML.
                            ' Now check if the node imported is a Content node - if so get rid of the Content node
                            Dim oFirst As XmlElement = Tools.Xml.firstElement(oElmt)
                            If oFirst.LocalName = "Content" Then
                                oElmt.InnerXml = oFirst.InnerXml
                            End If

                            moDbHelper.addRelatedContent(oNode, mnArtId, mbAdminMode)

                        End If

                    Next

                    'If gbCart Or gbQuote Then
                    '    moDiscount.getAvailableDiscounts(oRoot)
                    'End If

                    If Not oPageElmt Is Nothing Then
                        oPageElmt.AppendChild(oRoot.FirstChild)
                    End If
                    ' AddGroupsToContent(oRoot)
                    retElmt = oRoot.FirstChild
                    Return oRoot.FirstChild
                Else
                    sProcessInfo = "no content to add"
                End If
            Else
                sProcessInfo = "content exists adding content"
                oRoot = moPageXml.CreateElement("ContentDetail")
                oRoot.AppendChild(moContentDetail)
                If Not oPageElmt Is Nothing Then
                    oPageElmt.AppendChild(oRoot)
                End If
                ' AddGroupsToContent(oRoot)
                retElmt = moContentDetail
                moDbHelper.CommitLogToDB(Cms.dbHelper.ActivityType.ContentDetailViewed, mnUserId, Me.SessionID(), Now, mnArtId, 0, "")
                Return moContentDetail
            End If

            Return retElmt
        Catch ex As Exception
            'returnException(mcModuleName, "getContentDetailXml", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetContentBriefXml", ex, sProcessInfo))
            Return Nothing
        End Try

    End Function

    ''' <summary>
    ''' This attempts to construct the standard SQL filter for getting LIVE content
    ''' If version control is on it will also assess the page permissions, 
    ''' and if appropriate, it will get content that is not LIVe but, say, PENDING
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetStandardFilterSQLForContent(Optional ByVal bPrecedingAND As Boolean = True) As String

        PerfMon.Log("Web", "GetStandardFilterSQLForContent")

        Dim sFilterSQL As String = ""

        Try

            ' Only check for permissions if not in Admin Mode
            If Not Me.mbAdminMode Then

                ' Set the default filter
                sFilterSQL = "a.nStatus = 1 "

                If gbVersionControl _
                    AndAlso Me.mnUserId > 0 Then
                    ' Version control is on
                    ' Check the page permission
                    If dbHelper.CanAddUpdate(Me.mnUserPagePermission) Then

                        ' User has update permissions - now can they only have control over their own items
                        If dbHelper.CanOnlyUseOwn(Me.mnUserPagePermission) Then

                            ' Return everything with a status of live and anything that was created by
                            ' the user and has a status that isn't hidden
                            sFilterSQL = "(a.nStatus = 1 OR (a.nStatus >= 1 AND a.nInsertDirId=" & Me.mnUserId.ToString & ")) "

                        Else

                            ' Return anything with a status that isn't hidden
                            sFilterSQL = "a.nStatus >= 1 "

                        End If

                    End If
                End If

                If bPrecedingAND Then sFilterSQL = " AND " & sFilterSQL

                Dim ExpireLogic As String = ">= "
                If LCase(moConfig("ExpireAtEndOfDay")) = "on" Then
                    ExpireLogic = "> "
                End If

                sFilterSQL &= " and (a.dPublishDate is null or a.dPublishDate = 0 or a.dPublishDate <= " & Protean.Tools.Database.SqlDate(mdDate) & " )"
                sFilterSQL &= " and (a.dExpireDate is null or a.dExpireDate = 0 or a.dExpireDate " & ExpireLogic & Protean.Tools.Database.SqlDate(mdDate) & " )"


                ' sFilterSQL &= " and (a.dPublishDate is null or a.dPublishDate <= " & Protean.Tools.Database.SqlDate(mdDate) & " )"
                ' sFilterSQL &= " and (a.dExpireDate is null or a.dExpireDate " & ExpireLogic & Protean.Tools.Database.SqlDate(mdDate) & " )"
            End If
            PerfMon.Log("Web", "GetStandardFilterSQLForContent-END")
            Return sFilterSQL

        Catch ex As Exception
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetStandardFilterSQLForContent", ex, sFilterSQL))
            Return ""
        End Try
    End Function


    Public Sub AddBreadCrumb(ByVal cPath As String, ByVal cDisplayName As String)
        PerfMon.Log("Web", "AddBreadCrumb")
        Dim oNode As XmlNode
        Dim oElmt As XmlElement
        Dim oElmt2 As XmlElement

        Dim sProcessInfo As String = "" = "adding Link to Breadcrumb"

        Try
            oNode = moPageXml.DocumentElement.SelectSingleNode("Breadcrumb")
            If oNode Is Nothing Then
                oElmt = moPageXml.CreateElement("Breadcrumb")
                moPageXml.DocumentElement.AppendChild(oElmt)
            Else
                oElmt = oNode
            End If
            oElmt2 = moPageXml.CreateElement("Link")
            oElmt2.SetAttribute("name", cDisplayName)
            oElmt2.SetAttribute("url", cPath)
            oElmt.AppendChild(oElmt2)

        Catch ex As Exception
            'returnException(mcModuleName, "AddBreadCrumb", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "AddBreadCrumb", ex, sProcessInfo))
        End Try
    End Sub

    Public Sub returnDocumentFromItem(ByRef ctx As System.Web.HttpContext)
        PerfMon.Log("Web", "returnDocumentFromItem")
        Dim oXML As XmlDocument = New XmlDocument
        Dim sSql As String = ""
        Dim strFilePath As String = ""
        Dim strFileName As String = ""
        Dim objStream, strFileSize, strFileType, FileExt As String
        'Dim strPageInfo, strReferrer
        'Dim sWriter = New IO.StringWriter
        Dim oDS As DataSet = New DataSet
        Dim sProcessInfo As String = "returnDocumentFromItem"
        Dim sPath As String = ""
        Dim oErrorXml As XmlDocument = New XmlDocument
        Dim nDocId As Long
        Dim dsRow As DataRow
        Dim downloadFileName As String = "download.zip"
        Try
            moResponse.Buffer = True
            moResponse.Expires = 0
            goServer.ScriptTimeout = 10000


            If Regex.IsMatch(CStr(moRequest("docId") & ""), "^[0-9,]*$") And Not (moRequest("docId") Is Nothing) Then

                Dim aDocId() As String = Split(moRequest("docId"), ",")
                If UBound(aDocId) <> 0 Then


                    ' Firstly, work out if any / all of the requested files IDs exist
                    ' If some of the documents exist, zip them up, and report the missing files to the site admin
                    ' If none of the files exist, then report top site admin and return an error.

                    Dim documentPaths As New ArrayList
                    Dim documentsNotFound As New ArrayList

                    For Each docId As String In aDocId
                        If IsNumeric(docId) AndAlso Not (String.IsNullOrEmpty(docId)) Then
                            sSql += dbHelper.SqlString(docId) + ","
                        End If
                    Next
                    sSql = sSql.TrimEnd(",")

                    ' Get the file paths from the parent content
                    If Not (String.IsNullOrEmpty(sSql)) Then
                        sSql = "select * from tblContent where nContentKey in (" & sSql & ")"
                        oDS = moDbHelper.GetDataSet(sSql, "Item")
                        For Each dsRow In oDS.Tables("Item").Rows
                            strFilePath = moDbHelper.getContentFilePath(dsRow, moRequest("xPath"))
                            If strFilePath = "" Then
                                documentsNotFound.Add("Content does not contain filepath")
                            Else
                                If File.Exists(strFilePath) Then
                                    documentPaths.Add(strFilePath)
                                Else
                                    documentsNotFound.Add(strFilePath)
                                End If

                            End If
                        Next
                        oDS = Nothing
                    End If

                    ' Send the zipped up files
                    If documentPaths.Count > 0 Then

                        If moRequest("filename") <> "" Then downloadFileName = moRequest("filename")

                        Dim oZip As ICSharpCode.SharpZipLib.Zip.ZipOutputStream = New ICSharpCode.SharpZipLib.Zip.ZipOutputStream(ctx.Response.OutputStream)
                        oZip.SetLevel(3)
                        ctx.Response.AddHeader("Connection", "keep-alive")
                        ctx.Response.AddHeader("Content-Disposition", "filename=" & downloadFileName)

                        ctx.Response.Charset = "UTF-8"
                        ctx.Response.ContentType = Tools.FileHelper.GetMIMEType("zip")


                        For Each documentPath As String In documentPaths

                            Dim fsHelper As New Protean.fsHelper()
                            Dim oFileStream As FileStream = fsHelper.GetFileStream(documentPath)
                            strFileName = Tools.Text.filenameFromPath(documentPath)

                            Dim oZipEntry As ICSharpCode.SharpZipLib.Zip.ZipEntry = New ICSharpCode.SharpZipLib.Zip.ZipEntry(strFileName)
                            oZipEntry.DateTime = DateTime.Now
                            oZipEntry.Size = oFileStream.Length
                            'oZipEntry.Offset = -1

                            Dim abyBuffer(CInt(oFileStream.Length - 1)) As Byte
                            oFileStream.Read(abyBuffer, 0, abyBuffer.Length)

                            Dim objCrc32 As New ICSharpCode.SharpZipLib.Checksum.Crc32()
                            objCrc32.Reset()
                            objCrc32.Update(abyBuffer)
                            oZipEntry.Crc = objCrc32.Value

                            oZip.PutNextEntry(oZipEntry)
                            oZip.Write(abyBuffer, 0, abyBuffer.Length)
                        Next

                        oZip.Close()
                        ctx.Response.Flush()
                        ctx.Response.End()

                    End If


                    ' Tidy up - send errors if needs be
                    If documentsNotFound.Count > 0 Or documentPaths.Count = 0 Then


                        Dim filesNotFound As String = ""
                        For Each documentNotFound As String In documentsNotFound
                            filesNotFound &= documentNotFound & ","
                        Next
                        filesNotFound = filesNotFound.TrimEnd(",")

                        ' Report the unfound documents
                        If documentsNotFound.Count > 0 Then
                            oErrorXml.LoadXml("<Content type=""error""><filename>" & filesNotFound & "</filename></Content>")
                            siteAdminErrorNotification(oErrorXml.DocumentElement)
                        End If

                        ' If nothing is found then display an error
                        If documentPaths.Count = 0 Then

                            sProcessInfo = "File(s) Not Found: " & filesNotFound & "; Content Id(s) requested:" & moRequest("docId")
                            Err.Raise(1007)

                        End If

                    End If

                Else
                    If Not IsNumeric(aDocId(0)) Then Throw New Exception("Incorrect Document Id Format")
                    nDocId = aDocId(0)
                    sSql = "select * from tblContent where nContentKey = " & aDocId(0)
                    oDS = moDbHelper.GetDataSet(sSql, "Item")

                    If oDS.Tables.Count = 0 Then
                        'ErrorDoc("Document was not found in database", moRequest("docId"), "")
                    Else

                        Dim allowAccess As Boolean = True

                        If LCase(moConfig("SecureDownloads")) = "on" Then
                            Dim oPageElmt As XmlElement
                            If moPageXml.DocumentElement Is Nothing Then
                                moPageXml.CreateXmlDeclaration("1.0", "UTF-8", "yes")
                                oPageElmt = moPageXml.CreateElement("Page")
                                moPageXml.AppendChild(oPageElmt)
                                GetRequestVariablesXml(oPageElmt)
                                GetSettingsXml(oPageElmt)
                            Else
                                oPageElmt = moPageXml.DocumentElement
                            End If

                            GetStructureXML("Site")

                            allowAccess = moDbHelper.checkContentLocationsInCurrentMenu(aDocId(0), True)


                        End If

                        If allowAccess Then

                            strFilePath = moDbHelper.getContentFilePath(oDS.Tables("Item").Rows(0), moRequest("xPath"))

                            If strFilePath <> "" Then
                                'lets clear up the file name
                                'check for paths both local and virtual
                                strFileName = Tools.Text.filenameFromPath(strFilePath)
                                FileExt = Mid(strFileName, InStrRev(strFileName, ".") + 1)
                                strFileType = Protean.Tools.FileHelper.GetMIMEType(FileExt)

                                Dim fso As FileInfo = New FileInfo(strFilePath)

                                If fso.Exists Then
                                    Dim oImp As Protean.Tools.Security.Impersonate = New Protean.Tools.Security.Impersonate
                                    If oImp.ImpersonateValidUser(moConfig("AdminAcct"), moConfig("AdminDomain"), moConfig("AdminPassword"), , moConfig("AdminGroup")) Then



                                        Dim oFileStream As FileStream = New FileStream(strFilePath, FileMode.Open)
                                        strFileSize = oFileStream.Length

                                        Dim Buffer(CInt(strFileSize)) As Byte
                                        oFileStream.Read(Buffer, 0, CInt(strFileSize))
                                        oFileStream.Close()

                                        ctx.Response.Clear()
                                        'Const adTypeBinary = 1
                                        ctx.Response.AddHeader("Connection", "keep-alive")
                                        If ctx.Request.QueryString("mode") = "open" Then
                                            ctx.Response.AddHeader("Content-Disposition", "filename=" & strFileName)
                                        Else
                                            ctx.Response.AddHeader("Content-Disposition", "attachment; filename=" & strFileName)
                                        End If
                                        ctx.Response.AddHeader("Content-Length", strFileSize + 1)
                                        ctx.Response.Charset = "UTF-8"
                                        ctx.Response.ContentType = Protean.Tools.FileHelper.GetMIMEType(FileExt)
                                        ctx.Response.BinaryWrite(Buffer)
                                        ctx.Response.Flush()

                                        objStream = Nothing

                                        oImp.UndoImpersonation()

                                        'Activity Log
                                        If mnUserId <> "0" And mbAdminMode = False And Features.ContainsKey("ActivityReporting") Then
                                            'NB: 30-03-2010 New check to add in the ArtId (original line is the 2nd, with ArtId hardcoded as 0?)
                                            If Not moRequest("docId") Is Nothing Then
                                                moDbHelper.CommitLogToDB(dbHelper.ActivityType.DocumentDownloaded, mnUserId, moSession.SessionID, Now, mnPageId, moRequest("docId"), strFileName)
                                            Else
                                                moDbHelper.CommitLogToDB(dbHelper.ActivityType.DocumentDownloaded, mnUserId, moSession.SessionID, Now, mnPageId, 0, strFileName)
                                            End If
                                        End If

                                    End If

                                Else
                                    '------------------------------------------ 26-08-2008
                                    ' here we want to email the site admin to tell them the file is missing
                                    oErrorXml.LoadXml("<Content type=""error""><filename>" & strFilePath & "</filename></Content>")
                                    siteAdminErrorNotification(oErrorXml.DocumentElement)
                                    '-----------------------------------------------------

                                    '---------------------------------------------Original

                                    '-----------------------------------------------------
                                    If mnUserId <> "0" And mbAdminMode = False And Features.ContainsKey("ActivityReporting") Then
                                        moDbHelper.CommitLogToDB(dbHelper.ActivityType.DocumentDownloaded, mnUserId, moSession.SessionID, Now, mnPageId, 0, "ERROR NOT FOUND:" & strFileName)
                                    End If

                                    If goApp("PageNotFoundId") <> RootPageId Then
                                        Me.msRedirectOnEnd = "/System+Pages/Page+Not+Found"
                                        moResponse.Redirect(msRedirectOnEnd, False)
                                        moCtx.ApplicationInstance.CompleteRequest()
                                    Else
                                        ' Kept for follow up window, however does this send original mail out?
                                        sProcessInfo = "File Not Found:" & strFilePath
                                        Err.Raise(1007, , "ProteanCMS Error: " & sProcessInfo)
                                    End If


                                End If
                            Else
                                ' Content ID exists but is does not contain an xPath so is not a valid document
                                If gnPageNotFoundId > 1 Then
                                    msRedirectOnEnd = "/System+Pages/Page+Not+Found"
                                    moResponse.StatusCode = 404
                                Else
                                    msRedirectOnEnd = moConfig("BaseUrl")
                                    moResponse.StatusCode = 404
                                End If
                            End If


                        Else
                            moSession("LogonRedirect") = moRequest.ServerVariables("PATH_INFO") & "?" & moRequest.ServerVariables("QUERY_STRING")
                            Me.msRedirectOnEnd = "/System+Pages/Access+Denied"

                            moResponse.Redirect(msRedirectOnEnd, False)
                            moCtx.ApplicationInstance.CompleteRequest()
                            Exit Sub

                        End If
                    End If

                End If
            Else

                'put this in to prevent a redirect if we are calling this from somewhere strange.
                If gnPageNotFoundId > 1 Then
                    msRedirectOnEnd = "/System+Pages/Page+Not+Found"
                    moResponse.StatusCode = 404
                Else
                    msRedirectOnEnd = moConfig("BaseUrl")
                    moResponse.StatusCode = 404
                End If

                '  ctx.Response.StatusCode = 404
                '  ctx.Response.Flush()

            End If

        Catch ex As Exception

            'returnException(mcModuleName, "returnDocumentFromItem", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "returnDocumentFromItem", ex, sProcessInfo))
            moResponse.Write(msException)
        End Try

    End Sub

    Public Sub returnPageAsPDF(ByRef ctx As System.Web.HttpContext)
        PerfMon.Log("Web", "returnDocumentFromItem")
        Dim oXML As XmlDocument = New XmlDocument
        Dim sSql As String = ""
        Dim strFilePath As String = ""
        Dim strFileName As String = CStr(moRequest("filename")) & ".pdf"
        Dim objStream, strFileSize As String
        'Dim strPageInfo, strReferrer
        'Dim sWriter = New IO.StringWriter
        Dim oDS As DataSet = New DataSet
        Dim sProcessInfo As String = "returnDocumentFromItem"
        Dim sPath As String = ""
        Dim oErrorXml As XmlDocument = New XmlDocument
        Dim nArtId As Long = CInt(moRequest("artId"))
        Dim nPageId As Long = CInt(moRequest("pgid"))

        Try

            'First we get the page XML
            Dim cEwSitePDFXsl As String = moConfig("PagePDFXslPath")

            If mbAdminMode And Not ibIndexMode And Not gnResponseCode = 404 Then
                bPageCache = False
            End If

            sProcessInfo = "Transform PageXML Using XSLT"
            If mbAdminMode And Not ibIndexMode And Not gnResponseCode = 404 Then
                sProcessInfo = "In Admin Mode"
                If moAdmin Is Nothing Then moAdmin = New Admin(Me)
                'Dim oAdmin As Admin = New Admin(Me)
                'Dim oAdmin As Protean.Cms.Admin = New Protean.Cms.Admin(Me)
                moAdmin.open(moPageXml)
                moAdmin.adminProcess(Me)
                moAdmin.close()
                moAdmin = Nothing
            Else
                If moPageXml.OuterXml = "" Then
                    sProcessInfo = "Getting Page XML"
                    GetPageXML()
                End If
            End If

            'Next we transform using into FO.Net Xml

            If moTransform Is Nothing Then
                Dim styleFile As String = CType(goServer.MapPath(cEwSitePDFXsl), String)
                PerfMon.Log("Web", "ReturnPageHTML - loaded Style")
                moTransform = New Protean.XmlHelper.Transform(Me, styleFile, False)
            End If

            msException = ""

            moTransform.mbDebug = gbDebug
            icPageWriter = New IO.StringWriter
            moTransform.ProcessTimed(moPageXml, icPageWriter)



            Dim foNetXml As String = icPageWriter.ToString

            If foNetXml.StartsWith("<html") Then
                moResponse.Write(foNetXml)
            Else
                'now we use FO.Net to generate our PDF

                Dim oFoNet As New Fonet.FonetDriver()
                Dim ofileStream As New System.IO.MemoryStream()
                Dim oTxtReader As New System.IO.StringReader(foNetXml)
                oFoNet.CloseOnExit = False

                Dim rendererOpts As New Fonet.Render.Pdf.PdfRendererOptions()

                rendererOpts.Author = "ProteanCMS"
                rendererOpts.EnablePrinting = True
                rendererOpts.FontType = Fonet.Render.Pdf.FontType.Embed
                ' rendererOpts.Kerning = True
                ' rendererOpts.EnableCopy = True

                'Dim oImp As Protean.Tools.Security.Impersonate = New Protean.Tools.Security.Impersonate
                'If oImp.ImpersonateValidUser(moConfig("AdminAcct"), moConfig("AdminDomain"), moConfig("AdminPassword"), , moConfig("AdminGroup")) Then

                Dim dir As New DirectoryInfo(goServer.MapPath("/") & "/fonts")
                Dim subDirs As DirectoryInfo() = dir.GetDirectories()
                Dim files As FileInfo() = dir.GetFiles()
                Dim fi As FileInfo

                For Each fi In files
                    Dim cExt As String = LCase(fi.Extension)
                    Select Case cExt
                        Case ".otf"
                            rendererOpts.AddPrivateFont(fi)
                    End Select
                Next fi

                oFoNet.Options = rendererOpts
                oFoNet.Render(oTxtReader, ofileStream)

                'oImp.UndoImpersonation()

                'End If


                'And then we stram out to the browser

                moResponse.Buffer = True
                moResponse.Expires = 0
                goServer.ScriptTimeout = 10000

                strFileSize = ofileStream.Length

                Dim Buffer() As Byte = ofileStream.ToArray
                'oFileStream.Read(Buffer, 0, CInt(strFileSize))
                'oFileStream.Close()

                'downloadBytes = ofileStream.ToArray

                ctx.Response.Clear()
                'Const adTypeBinary = 1
                ctx.Response.AddHeader("Connection", "keep-alive")
                If ctx.Request.QueryString("mode") = "open" Then
                    ctx.Response.AddHeader("Content-Disposition", "filename=" & Replace(strFileName, ",", ""))
                Else
                    ctx.Response.AddHeader("Content-Disposition", "attachment; filename=" & Replace(strFileName, ",", ""))
                End If
                ctx.Response.AddHeader("Content-Length", strFileSize)
                'ctx.Response.Charset = "UTF-8"
                ctx.Response.ContentType = Protean.Tools.FileHelper.GetMIMEType("PDF")
                ctx.Response.BinaryWrite(Buffer)
                ctx.Response.Flush()

                objStream = Nothing
                oFoNet = Nothing
                oTxtReader = Nothing
                ofileStream = Nothing
            End If


        Catch ex As Exception

            'returnException(mcModuleName, "returnDocumentFromItem", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "returnDocumentFromItem", ex, sProcessInfo))
            moResponse.Write(msException)
        End Try

    End Sub


    Private Sub siteAdminErrorNotification(ByVal oXmlE As XmlElement)
        PerfMon.Log("Web", "siteAdminErrorNotification")
        'Create the Email for File not Found error

        Dim oPageElmt As XmlElement

        Try
            'Create the Page Xml
            If moPageXml.DocumentElement Is Nothing Then
                moPageXml.CreateXmlDeclaration("1.0", "UTF-8", "yes")
                oPageElmt = moPageXml.CreateElement("Page")
                moPageXml.AppendChild(oPageElmt)
                GetRequestVariablesXml(oPageElmt)

                GetSettingsXml(oPageElmt)
            Else
                oPageElmt = moPageXml.DocumentElement
            End If

            'GetUserXML() - Returns XML Element of User Details
            Dim oUserXml As XmlElement = GetUserXML()
            'Require Error Handling
            If Not (oUserXml Is Nothing) Then
                moPageXml.DocumentElement.AppendChild(moPageXml.ImportNode(oUserXml.CloneNode(True), True))
                ' ^^ This line had some basic testing, seems to work, might need extended?
            End If

            'Add remaining details pulled in
            moPageXml.DocumentElement.AppendChild(moPageXml.ImportNode(oXmlE, True))

            Dim oMsg As Protean.Messaging = New Protean.Messaging
            oMsg.emailer(oPageElmt, "/ewcommon/xsl/email/siteAdminError_FileNotFound.xsl", "ProteanCMS Error", "error@proteancms.com", moConfig("siteAdminEmail"), "File not found", , , , "error@proteancms.com")

        Catch ex As Exception
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "siteAdminErrorNotification", ex, ""))
        End Try

    End Sub

    ''' <summary>
    ''' Multiple ParentId Checking
    ''' each content item with mulitple parents replace with current page as parId
    ''' each content item with no parent replace with current page as parId
    ''' </summary>
    ''' <param name="oPage">The Page XML Element</param>
    ''' <param name="nCurrentPage">The current Page ID</param>
    ''' <remarks></remarks>
    Public Sub CheckMultiParents(ByRef oPage As XmlElement, ByVal nCurrentPage As Integer)
        PerfMon.Log("Web", "CheckMultiParents-Start")
        Try
            Dim oElmt As XmlElement

            'TS optimise don't need these in GetParId for each node
            Dim oMenu As XmlElement = moPageXml.SelectSingleNode("/Page/Menu[@id='Site']")
            If oMenu Is Nothing Then
                oMenu = Me.GetStructureXML(mnUserId)
            End If
            Dim oCurPage As XmlElement = oMenu.SelectSingleNode("descendant-or-self::MenuItem[@id='" & nCurrentPage & "']")

            For Each oElmt In oPage.SelectNodes("descendant-or-self::Content[contains(@parId,',')]")
                oElmt.SetAttribute("parId", GetParId(oElmt.GetAttribute("parId"), nCurrentPage, oMenu, oCurPage))
            Next
            For Each oElmt In oPage.SelectNodes("descendant-or-self::Content[@parId='']")
                oElmt.SetAttribute("parId", nCurrentPage)
            Next
            PerfMon.Log("Web", "CheckMultiParents-End")
        Catch ex As Exception
            'returnException(mcModuleName, "CheckMultiParents", ex, gcEwSiteXsl, , gbDebug)
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "CheckMultiParents", ex, ""))
        End Try
    End Sub


    'Multiple ParentId Checking FOR CART ITEMS
    Public Sub CheckMultiParents(ByRef oCartElmt As XmlElement)
        PerfMon.Log("Web", "CheckMultiParents")
        Try
            Dim oElmt As XmlElement

            For Each oElmt In oCartElmt.SelectNodes("descendant-or-self::Item[contains(@parId,',')]")
                oElmt.SetAttribute("parId", GetParId(oElmt.GetAttribute("parId"), mnPageId))
            Next
            'For Each oElmt In oPage.SelectNodes("descendant-or-self::Item[@parId='']")
            '    oElmt.SetAttribute("parId", nCurrentPage)
            'Next
        Catch ex As Exception
            'returnException(mcModuleName, "CheckMultiParents", ex, gcEwSiteXsl, , gbDebug)
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "CheckMultiParents", ex, ""))
        End Try
    End Sub

    'TS - now overloaded to preseve existing public function
    Public Function GetParId(ByVal nParids As String, ByVal nCurrentPage As Integer) As Integer
        PerfMon.Log("Web", "GetParId")
        Dim cProcessInfo As String = ""
        Dim oParents() As String = Split(nParids, ",")
        Try

            Dim oMenu As XmlElement = moPageXml.SelectSingleNode("/Page/Menu[@id='Site']")
            If oMenu Is Nothing Then
                oMenu = Me.GetStructureXML(mnUserId)
            End If

            Dim oCurPage As XmlElement = oMenu.SelectSingleNode("descendant-or-self::MenuItem[@id='" & nCurrentPage & "']")

            Return GetParId(nParids, nCurrentPage, oMenu, oCurPage)

        Catch ex As Exception
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetParId", ex, ""))
            Return oParents(0)
        End Try
    End Function

    Public Function GetParId(ByVal nParids As String, ByVal nCurrentPage As Integer, ByRef oMenu As XmlElement, ByRef oCurPage As XmlElement) As Integer
        'PerfMon.Log("Web", "GetParId")
        Dim cProcessInfo As String = ""
        Dim oParents() As String = Split(nParids, ",")
        Dim sFilteredParents As String = ""
        Dim cXPath As String = ""
        Dim i As Integer
        Try

            For i = 0 To UBound(oParents)
                'We are only interested parIds in the current site tree
                If Not oMenu.SelectSingleNode("descendant-or-self::MenuItem[@id='" & oParents(i) & "']") Is Nothing Then
                    If Not cXPath = "" Then cXPath &= " or "
                    cXPath &= "@id='" & oParents(i) & "'"
                    If oParents(i) > nCurrentPage Then
                        sFilteredParents = oParents(i) & ","
                    End If
                End If
            Next

            'if no parId on this site return current page
            If sFilteredParents = "" Then
                Return nCurrentPage
            Else
                Dim oFilteredParents() As String
                oFilteredParents = Split(sFilteredParents.Remove(sFilteredParents.LastIndexOf(","), 1), ",")

                If oFilteredParents.Length = 1 Then
                    Return oFilteredParents(0)
                Else
                    cXPath = "MenuItem[" & cXPath & "]"
                    Dim oIDs(0), oSteps(0) As Integer
                    ClosestDescendants(oCurPage, 0, cXPath, oIDs, oSteps)
                    ClosestAncestor(oCurPage, 0, cXPath, oFilteredParents, oIDs, oSteps)

                    Dim nLowestStep As Integer = 99999
                    Dim nNewPage As Integer
                    For i = 0 To UBound(oSteps)
                        If oSteps(i) < nLowestStep Then
                            nNewPage = oIDs(i)
                        End If
                    Next

                    If nNewPage = 0 Then
                        Return oFilteredParents(0)
                    Else
                        Return nNewPage
                    End If
                End If
            End If




        Catch ex As Exception
            'returnException(mcModuleName, "GetParId", ex, gcEwSiteXsl, , gbDebug)
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetParId", ex, ""))
            Return oParents(0)
        End Try
    End Function

    Public Sub ClosestDescendants(ByVal oElmt As XmlElement, ByVal nStep As Integer, ByVal cXpath As String, ByRef oIDs() As Integer, ByRef oSteps() As Integer)
        Dim oChild As XmlElement
        Try
            If nStep = 0 Then PerfMon.Log("Web", "ClosestDescendants-Start")
            nStep += 1
            For Each oChild In oElmt.SelectNodes(cXpath)
                ReDim Preserve oIDs(UBound(oIDs))
                ReDim Preserve oSteps(UBound(oSteps))
                oIDs(UBound(oIDs)) = oChild.GetAttribute("id")
                oSteps(UBound(oSteps)) = nStep
            Next
            For Each oChild In oElmt.SelectNodes("MenuItem")
                ClosestDescendants(oChild, nStep, cXpath, oIDs, oSteps)
            Next
            If nStep = 0 Then PerfMon.Log("Web", "ClosestDescendants-End")
        Catch ex As Exception
            'dont do a thing, might not be children
        End Try
    End Sub

    Public Sub ClosestAncestor(ByVal oElmt As XmlElement, ByVal nStep As Integer, ByVal cXpath As String, ByVal oParents() As String, ByRef oIDs() As Integer, ByRef oSteps() As Integer)
        PerfMon.Log("Web", "ClosestAncestor")
        Try
            Dim oParentElmt As XmlElement = oElmt.ParentNode

            nStep += 1
            'check parent
            Dim i As Integer
            For i = 0 To UBound(oParents)
                If oParentElmt.GetAttribute("id") = oParents(i) Then
                    oIDs(UBound(oIDs)) = oParentElmt.GetAttribute("id")
                    oSteps(UBound(oSteps)) = nStep
                End If
            Next
            'now check its grandparents children?
            ClosestDescendants(oParentElmt.ParentNode, nStep, cXpath, oIDs, oSteps)
            'keep going up
            ClosestAncestor(oParentElmt.ParentNode, nStep, cXpath, oParents, oIDs, oSteps)

        Catch ex As Exception
            'dont do anything, there might not be ancestors
        End Try
    End Sub

    Public ReadOnly Property Referrer() As String
        Get
            Try

                If Not moSession Is Nothing Then
                    mcSessionReferrer = moSession("Referrer")
                Else
                    mcSessionReferrer = moRequest.UrlReferrer.AbsoluteUri
                End If

                If mcSessionReferrer Is Nothing Then
                    If Not moRequest.UrlReferrer Is Nothing Then
                        If moRequest.UrlReferrer.Host <> moRequest.ServerVariables("HTTP_HOST") Then
                            moSession.Add("Referrer", moRequest.UrlReferrer.AbsoluteUri)
                            mcSessionReferrer = moSession("Referrer")
                        End If
                    End If
                End If

                Return mcSessionReferrer

            Catch ex As Exception
                Return ""
            End Try
        End Get
    End Property

    Protected Overridable ReadOnly Property Generator() As Assembly
        Get
            Return Assembly.GetExecutingAssembly()
        End Get
    End Property


    Protected Overridable ReadOnly Property ReferencedAssemblies() As AssemblyName()
        Get
            Return Assembly.GetExecutingAssembly().GetReferencedAssemblies()
        End Get
    End Property

    ''' <summary>
    ''' If a request has been made for the page xml to be output, 
    ''' this checks the IP address against locked down addresses
    ''' if the IP address lockdown config setting has been created.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Protected ReadOnly Property ValidatedOutputXml() As Boolean
        Get
            Try
                Dim xmlAllowedIPList As String = moConfig("XmlAllowedIPList")
                Return IIf(String.IsNullOrEmpty(xmlAllowedIPList), mbOutputXml, IsCurrentIPAddressInList(xmlAllowedIPList))
            Catch ex As Exception
                OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ValidatedOutputXml", ex, ""))
                Return False
            End Try
        End Get
    End Property



    Public Overridable ReadOnly Property HasSession() As Boolean
        Get
            Return moSession IsNot Nothing
        End Get
    End Property

    ' ''' <summary>
    ' ''' If in Single Session per Session mode this will log an activity indicating that the user is still in session.
    ' ''' </summary>
    ' ''' <remarks>It is called in EonicWeb but has been extracted so that it may be called by lightweight EonicWeb calls (e.g. ajax calls)</remarks>
    Public Sub LogSingleUserSession()
        Try

            Dim oMembershipProv As New Providers.Membership.BaseProvider(Me, moConfig("MembershipProvider"))
            oMembershipProv.Activities.LogSingleUserSession(Me)

        Catch ex As Exception
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "LogSingleUserSession", ex, ""))
        End Try
    End Sub

    Private Sub GetRequestLanguage()
        PerfMon.Log("Web", "GetRequestLanguage")

        Dim cProcessInfo As String = ""
        Dim oElmt As XmlElement
        Dim sCurrency As String = ""
        Dim httpPrefix As String = "http://"


        Try


            If Not goLangConfig Is Nothing Then

                If moRequest.ServerVariables("SERVER_PORT_SECURE") = "1" Then
                    httpPrefix = "https://"
                End If

                For Each oElmt In goLangConfig.ChildNodes
                    Select Case LCase(oElmt.GetAttribute("identMethod"))
                        Case "domain"
                            If oElmt.GetAttribute("identifier") = moRequest.ServerVariables("HTTP_HOST") Then
                                mcPageLanguage = oElmt.GetAttribute("code")
                                mcPageLanguageUrlPrefix = httpPrefix & oElmt.GetAttribute("identifier")
                                sCurrency = oElmt.GetAttribute("currency")
                            End If
                        Case "path"
                            If Not moRequest.ServerVariables("HTTP_X_ORIGINAL_URL") Is Nothing Then
                                If moRequest.ServerVariables("HTTP_X_ORIGINAL_URL").StartsWith("/" & oElmt.GetAttribute("identifier") & "/") _
                                Or moRequest.ServerVariables("HTTP_X_ORIGINAL_URL") = "/" & oElmt.GetAttribute("identifier") _
                                Or moRequest.ServerVariables("HTTP_X_ORIGINAL_URL").StartsWith("/" & oElmt.GetAttribute("identifier") & "?") Then
                                    mcPageLanguage = oElmt.GetAttribute("code")
                                    mcPageLanguageUrlPrefix = httpPrefix & goLangConfig.GetAttribute("defaultDomain") & "/" & oElmt.GetAttribute("identifier")
                                    sCurrency = oElmt.GetAttribute("currency")
                                End If
                            End If
                        Case "page"
                            mcPageLanguage = moDbHelper.getPageLang(mnPageId)
                    End Select
                Next

                If mcPageLanguage = "" Then
                    'set Default Language
                    mcPageLanguage = goLangConfig.GetAttribute("code")
                    sCurrency = goLangConfig.GetAttribute("currency")
                    Me.mcPreferredLanguage = mcPageLanguage
                    mcPageDefaultDomain = httpPrefix & goLangConfig.GetAttribute("defaultDomain")
                End If

                If Not moSession Is Nothing Then
                    If sCurrency <> "" Then
                        moSession("bCurrencySelected") = True
                        moSession("cCurrency") = sCurrency
                        moSession("cCurrencyRef") = sCurrency
                    End If
                End If

            End If
        Catch ex As Exception

        End Try

    End Sub
    ''' <summary>
    ''' Check the language setting against cookies and the like
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub SetPageLanguage()
        PerfMon.Log("Web", "SetPageLanguage")

        Dim cProcessInfo As String = ""
        Dim sCurrency As String = ""

        Try
            If Not goLangConfig Is Nothing Then

                If mcPageLanguage = "" Then
                    GetRequestLanguage()
                End If



                ' if the page requested is a version in another language then set the page language.
                If mbAdminMode Then
                    Dim cPageLang As String = moDbHelper.getPageLang(mnPageId)
                    If cPageLang <> mcPageLanguage _
                    And cPageLang <> goLangConfig.GetAttribute("code") Then
                        mcPageLanguage = moDbHelper.getPageLang(mnPageId)
                    End If
                End If

                'add the language info to the pageXml
                If moPageXml.DocumentElement.SelectSingleNode("languages") Is Nothing Then
                    Dim oLangElmt As XmlElement = moPageXml.CreateElement("Lang")
                    oLangElmt.InnerXml = goLangConfig.OuterXml
                    moPageXml.DocumentElement.AppendChild(oLangElmt.FirstChild)
                End If
            Else
                'Legacy code
                If Not moPageXml.DocumentElement.SelectSingleNode("Contents") Is Nothing Then
                    ' First find the page language, which should be called after content has been loaded in
                    Dim cLang As String = ""
                    If Tools.Xml.NodeState(moPageXml.DocumentElement, "//Content[@name='XmlLang']", , , , , , cLang) = Tools.Xml.XmlNodeState.HasContents Then
                        Me.mcPageLanguage = cLang
                    Else
                        Me.mcPageLanguage = gcLang
                    End If

                    ' Next check for a user language preference
                    If moRequest.Cookies("language") IsNot Nothing Then
                        Me.mcPreferredLanguage = moRequest.Cookies("language").Value.ToString
                    End If

                    Me.moPageXml.DocumentElement.SetAttribute("translang", IIf(String.IsNullOrEmpty(Me.mcPreferredLanguage), Me.mcPageLanguage, Me.mcPreferredLanguage))
                End If
            End If

            If mcPageLanguage <> "" Then
                ' Set the page attribute
                Me.moPageXml.DocumentElement.SetAttribute("lang", Me.mcPageLanguage)
                gcLang = Me.mcPageLanguage
            End If

            If Me.mcPreferredLanguage <> "" Then
                ' Set the page attribute
                Me.moPageXml.DocumentElement.SetAttribute("userlang", Me.mcPreferredLanguage)
            Else
                Me.moPageXml.DocumentElement.SetAttribute("userlang", Me.mcPageLanguage)
            End If

        Catch ex As Exception
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "SetPageLanguage", ex, cProcessInfo))
        End Try
    End Sub

    Protected Sub ProcessPageXMLForLanguage()

        PerfMon.Log("Web", "ProcessContentForLanguage - Start")

        Try
            ' Only strip out the content if we're not editting a form.
            Dim isEditForm As Boolean = False
            Dim keepLanguageVariants As String = ""
            If moRequest.QueryString.Count > 0 Then
                keepLanguageVariants = Me.moRequest("keeplanguagevariants")
            End If

            If moAdmin IsNot Nothing Then
                isEditForm = moAdmin.Command.Contains("Add") Or moAdmin.Command.Contains("Edit")
            End If

            If Not isEditForm Or Not String.IsNullOrEmpty(keepLanguageVariants) Then

                If Not String.IsNullOrEmpty(Me.mcPageLanguage) Then
                    Dim nsMgr As XmlNamespaceManager = New XmlNamespaceManager(moPageXml.NameTable)
                    nsMgr.AddNamespace("xml", "http://www.w3.org/XML/1998/namespace")

                    ' Find all the parents of nodes with xml:lang attributes
                    ' And only those with more than one matching node (just to begin to cut the scope down)
                    Dim children As New ArrayList
                    For Each parent As XmlElement In moPageXml.SelectNodes("//*[count(*[@xml:lang]) > 1]", nsMgr)

                        For Each langChild As XmlElement In parent.SelectNodes("*[@xml:lang]", nsMgr)

                            If Not children.Contains(langChild.Name.ToString) Then

                                ' This is a new node name
                                children.Add(langChild.Name.ToString)

                                ' Note the checks
                                ' Check one says if there's only 1 node of this name, then don't delete it.
                                ' Check two looks at the user preference and tries to identify if there's an item in this language, which it'll keep and dump the rest.
                                ' Check three does the same as the above, except with the page language
                                ' Check two and three only work if there's some innertext to the language nodes being looked at.
                                ' If none if the checks are matched then the first node is kept.

                                ' Check one: Check for more than one node of this name
                                Dim groupedNodes As XmlNodeList = parent.SelectNodes(langChild.Name.ToString, nsMgr)
                                If groupedNodes.Count > 1 Then

                                    Dim groupNodesAreAllEmpty As Boolean = True
                                    ' First check if all groupNodes are empty
                                    For nodeIndex As Integer = 1 To groupedNodes.Count - 1
                                        If Not String.IsNullOrEmpty(groupedNodes.Item(nodeIndex).InnerText.Trim) Then
                                            groupNodesAreAllEmpty = False
                                            Exit For
                                        End If
                                    Next

                                    ' More than one node of this type.
                                    If parent.SelectSingleNode(langChild.Name.ToString & "[@xml:lang='" & Me.mcPreferredLanguage & "']", nsMgr) IsNot Nothing _
                                       AndAlso (parent.SelectSingleNode(langChild.Name.ToString & "[@xml:lang='" & Me.mcPreferredLanguage & "']", nsMgr).InnerText.Trim.Length > 0 Or groupNodesAreAllEmpty) Then
                                        ' Check two: Check for the user preferred language
                                        For Each childToRemove As XmlElement In parent.SelectNodes(langChild.Name.ToString & "[not(@xml:lang) or @xml:lang!='" & Me.mcPreferredLanguage & "']", nsMgr)
                                            childToRemove.SetAttribute("removeUnwantedLanguageNode", "")
                                        Next
                                    ElseIf parent.SelectSingleNode(langChild.Name.ToString & "[@xml:lang='" & Me.mcPageLanguage & "']", nsMgr) IsNot Nothing _
                                       AndAlso (parent.SelectSingleNode(langChild.Name.ToString & "[@xml:lang='" & Me.mcPageLanguage & "']", nsMgr).InnerText.Trim.Length > 0 Or groupNodesAreAllEmpty) Then
                                        ' Check three: Check for the user page language
                                        For Each childToRemove As XmlElement In parent.SelectNodes(langChild.Name.ToString & "[not(@xml:lang) or @xml:lang!='" & Me.mcPageLanguage & "']", nsMgr)
                                            childToRemove.SetAttribute("removeUnwantedLanguageNode", "")
                                        Next
                                    Else


                                        ' Assume the first is the default, get rid of the rest.
                                        For nodeIndex As Integer = 1 To groupedNodes.Count - 1
                                            DirectCast(groupedNodes.Item(nodeIndex), XmlElement).SetAttribute("removeUnwantedLanguageNode", "")
                                        Next
                                    End If


                                End If

                            End If

                        Next

                        'Remove the unwanted nodes
                        For Each childToRemove As XmlElement In parent.SelectNodes("*[@removeUnwantedLanguageNode]", nsMgr)
                            parent.RemoveChild(childToRemove)
                        Next

                        ' Clear the node name list
                        children.Clear()

                    Next
                End If


            End If
            PerfMon.Log("Web", "ProcessContentForLanguage - End")

        Catch ex As Exception
            returnException(mcModuleName, "ProcessContentForLanguage", ex, "", "", gbDebug)
        End Try
    End Sub

    ''' <summary>
    ''' Tests if the current incoming IP address can be found in a CSV list of IP addresses
    ''' </summary>
    ''' <param name="ipList">CSV list of IP addresses</param>
    ''' <returns></returns>
    ''' <remarks>Uses the incoming request to determine the IP address to check.  Use Protean.Tools.Text.IsIPAddressInList to check a specific IP address</remarks>
    Public Function IsCurrentIPAddressInList(ByVal ipList As String) As Boolean
        Try
            Return Tools.Text.IsIPAddressInList(moRequest.ServerVariables("REMOTE_ADDR"), ipList)

        Catch ex As Exception
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "IsCurrentIPAddressInList", ex, ""))
            Return False
        End Try
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="itemKey"></param>
    ''' <param name="defaultValue"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetRequestItemAsInteger(ByVal itemKey As String, Optional ByVal defaultValue As Integer = 0) As Integer
        Try
            Dim item As String = moRequest(itemKey) & ""
            Return Tools.Number.ConvertStringToIntegerWithFallback(item, defaultValue)

        Catch ex As Exception
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetRequestItemAsInteger", ex, ""))
            Return defaultValue
        End Try
    End Function

    Public Function GetConfigItemAsInteger(ByVal itemKey As String, Optional ByVal defaultValue As Integer = 0) As Integer
        Try
            Dim item As String = moConfig(itemKey) & ""
            Return Tools.Number.ConvertStringToIntegerWithFallback(item, defaultValue)

        Catch ex As Exception
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetConfigItemAsInteger", ex, ""))
            Return defaultValue
        End Try
    End Function

    ''' <summary>
    ''' Adds a Response node to the Repsonses node of the page XML
    ''' Response is a simple text node to convey response information.
    ''' </summary>
    ''' <param name="responseName">The identifier for this response</param>
    ''' <param name="responseText">Optional the text that goes inside the Response node</param>
    ''' <param name="overwriteDuplicates">Optional if True this will overwrite any Response node with the same Name</param>
    ''' <remarks>If the Responses node has not been created then it will be.</remarks>
    Public Sub AddResponse(ByVal responseName As String, Optional ByVal responseText As String = "", Optional ByVal overwriteDuplicates As Boolean = True, Optional ByVal typeAttribute As Protean.ResponseType = ResponseType.Undefined)
        Try

            If _responses Is Nothing Then
                Dim defaultDocument As New XmlDocument
                _responses = defaultDocument.CreateElement("Responses")
            End If

            Dim response As XmlElement = _responses.OwnerDocument.CreateElement("Response")
            response.SetAttribute("name", responseName)
            response.InnerText = responseText

            If typeAttribute <> ResponseType.Undefined Then
                response.SetAttribute("type", typeAttribute.ToString)
            End If

            If overwriteDuplicates AndAlso _responses.SelectSingleNode("Response[@name='" & responseName & "']") IsNot Nothing Then
                _responses.ReplaceChild(response, _responses.SelectSingleNode("Response[@name='" & responseName & "']"))
            Else
                _responses.AppendChild(response)
            End If
        Catch ex As Exception
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "AddResponse", ex, ""))
        End Try
    End Sub

    Private Sub CommitResponsesToPage()
        Try
            If _responses IsNot Nothing AndAlso moPageXml IsNot Nothing AndAlso moPageXml.SelectSingleNode("Page") IsNot Nothing Then
                moPageXml.SelectSingleNode("Page").AppendChild(moPageXml.ImportNode(_responses, True))
            End If
        Catch ex As Exception
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "CommitResponsesToPage", ex, ""))

        End Try
    End Sub

    Public ReadOnly Property PageXMLResponses() As XmlNodeList
        Get
            If _responses Is Nothing Then
                Dim emptyList As New XmlDocument
                Return emptyList.ChildNodes
            Else
                Return _responses.SelectNodes("Response")
            End If
        End Get
    End Property

    Public ReadOnly Property SessionID() As String
        Get
            If moSession Is Nothing Then
                Return ""
            Else
                Return moSession.SessionID
            End If
        End Get
    End Property


    ''' <summary>
    ''' Protean.Cms specific config retrieval.
    ''' Shared function
    ''' </summary>
    ''' <param name="key"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Shared Function ConfigValue(ByVal key As String) As String
        Return Protean.Config.Value(key, "protean/web")
    End Function

    ''' <summary>
    ''' Protean.Cms specific config retrieval.
    ''' Shared function
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Shared Function Config() As System.Collections.Specialized.NameValueCollection
        Return Protean.Config.ConfigSection("protean/web")
    End Function


    Private disposedValue As Boolean = False        ' To detect redundant calls

    ' IDisposable
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' free managed resources when explicitly called
                If Not icPageWriter Is Nothing Then
                    icPageWriter.Dispose()
                End If
            End If

            ' free shared unmanaged resources
        End If
        Me.disposedValue = True
    End Sub
    Public Overridable Function UserFolder() As String
        ' NB : Empty to hold a place for Brokerage's bespoky-er-ness
        ' Returns a string, that can be inserted as an Attribute
        Return ""
    End Function

    ''' <summary>
    ''' Proxy for logging performance stats
    ''' This is used for classes that Shadow Protean.Cms becuase PerfLog is found inside
    ''' a module (stdTools) which means it is not accessible by shadowable classes.
    ''' This should be a shared class really.
    ''' </summary>
    ''' <param name="cModuleName"></param>
    ''' <param name="cProcessName"></param>
    ''' <param name="cDescription"></param>
    ''' <remarks></remarks>
    Public Sub PerfMonLog(ByVal cModuleName As String, ByVal cProcessName As String, Optional ByVal cDescription As String = "")
        PerfMon.Log(cModuleName, cProcessName, cDescription)
    End Sub

    Public Function RestartAppPool() As Boolean

        'First try killing your worker process
        'Try
        '    Dim proc As Process = Process.GetCurrentProcess()
        '    proc.Kill()
        '    Return True
        'Catch ex As Exception
        '    Return False
        'End Try

        '  Try unloading appdomain
        Try
            Dim oImp As Protean.Tools.Security.Impersonate = New Protean.Tools.Security.Impersonate
            ' If oImp.ImpersonateValidUser(moConfig("AdminAcct"), moConfig("AdminDomain"), moConfig("AdminPassword"), , moConfig("AdminGroup")) Then

            ' AppDomain.Unload(AppDomain.CurrentDomain)
            Return True
            'System.Web.HttpRuntime.UnloadAppDomain()
            ' Else
            'Return False
            ' End If
        Catch ex As Exception
            Return False
        End Try

        'update web.config
        '    Try
        '        '  Protean.Config.UpdateConfigValue(Me, "eonic/web", "CompliedTransform", "rebuild")
        '        File.SetLastWriteTimeUtc(goServer.MapPath("/web.config"), DateTime.UtcNow)
        '    Catch ex As Exception
        '        Return False
        'End Try

    End Function

    Public Sub SavePage(ByVal cUrl As String, ByVal cBody As String)
        PerfMon.Log("Indexer", "IndexPage")
        Dim cProcessInfo As String = ""
        Dim filename As String = ""
        Dim filepath As String = ""
        Dim artId As String = ""
        Dim Ext As String = ".html"
        Try

            'let's clean up the url
            If cUrl.LastIndexOf("/?") > -1 Then
                cUrl = cUrl.Substring(0, cUrl.LastIndexOf("/?"))
            End If
            If cUrl.LastIndexOf("?") > -1 Then
                cUrl = cUrl.Substring(0, cUrl.LastIndexOf("?"))
            End If

            If cUrl = "" Or cUrl = "/" Then
                filename = "home.html"
            Else
                filename = Left(cUrl.Substring(cUrl.LastIndexOf("/") + 1), 240)
                If cUrl.LastIndexOf("/") > 0 Then
                    filepath = Left(cUrl.Substring(0, cUrl.LastIndexOf("/")), 240) & ""
                End If
            End If

            Dim oFS As New Protean.fsHelper(moCtx)
            oFS.mcRoot = gcProjectPath
            oFS.mcStartFolder = goServer.MapPath("\" & gcProjectPath) & mcPageCacheFolder

            cProcessInfo = "Saving:" & mcPageCacheFolder & filepath & "\" & filename

            'Tidy up the filename
            filename = Protean.Tools.FileHelper.ReplaceIllegalChars(filename)
            filename = Replace(filename, "\", "-")
            filepath = Replace(filepath, "/", "\") & ""
            If filepath.StartsWith("\") And mcPageCacheFolder.EndsWith("\") Then
                filepath.Remove(0, 1)
            End If

            cProcessInfo = "Saving:" & mcPageCacheFolder & filepath & "\" & filename

            Dim cleanfilename As String = goServer.UrlDecode(filename)

            If cleanfilename.Length > 260 Then
                cleanfilename = Left(cleanfilename, 260)
            End If

            Dim FullFilePath As String = mcPageCacheFolder & filepath & "\" & goServer.UrlDecode(cleanfilename)

            ' If FullFilePath.Length > 255 Then
            ' FullFilePath = Left(FullFilePath, 240) & Ext
            ' Else
            ' FullFilePath = FullFilePath & Ext
            ' End If

            If filepath = "" Then filepath = "/"
            Dim sError As String = oFS.CreatePath(filepath)

            If sError = "1" Then

                Alphaleonis.Win32.Filesystem.File.WriteAllText("\\?\" & goServer.MapPath("/" & gcProjectPath) & FullFilePath, cBody, System.Text.Encoding.UTF8)

                '   If oFS.VirtualFileExistsAndRecent(FullFilePath, 10) Then

                'Else
                '   cProcessInfo &= "<Error>Create Path: " & filepath & " - " & sError & "</Error>" & vbCrLf
                '  Err.Raise(1001, "File not saved", cProcessInfo)
                '   End If

            Else
                cProcessInfo &= "<Error>Create Path: " & filepath & " - " & sError & "</Error>" & vbCrLf
            End If

            oFS = Nothing

        Catch ex As Exception
            'if saving of a page fails we are not that bothered.
            'cExError &= "<Error>" & filepath & filename & ex.Message & "</Error>" & vbCrLf
            returnException(mcModuleName, "SavePage", ex, "", cProcessInfo, gbDebug)
            'bIsError = True
        End Try
    End Sub

    Public Sub ClearPageCache()
        Dim cProcessInfo As String = ""
        Try

            moFSHelper.DeleteFolder(mcPageCacheFolder, goServer.MapPath("/" & gcProjectPath))

        Catch ex As Exception
            returnException(mcModuleName, "ClearPageCache", ex, "", cProcessInfo, gbDebug)
        End Try
    End Sub

#Region " IDisposable Support "
    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Overloads Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class
