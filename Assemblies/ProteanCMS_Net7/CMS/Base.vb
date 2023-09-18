Option Strict Off
Option Explicit On

Imports System.Xml
Imports System.Web.HttpUtility
Imports System.Configuration
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
Imports Microsoft.Extensions.Configuration
Imports Microsoft.Extensions.Caching.Memory
Imports Microsoft.AspNetCore.Hosting

Public Class Base


#Region "ErrorHandling"

    'for anything controlling web
    Public Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)

    Protected Overridable Sub OnComponentError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
        'deals with the error
        'returnException(e.ModuleName, e.ProcedureName, e.Exception, mcEwSiteXsl, e.AddtionalInformation, gbDebug)
        'close connection pooling
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

#Region "Enums"
    Enum licenceMode
        Demo = 0
        Development = 1
        Live = 2
    End Enum

#End Region

#Region "Declarations"
    Private moLicenceMode As licenceMode = licenceMode.Live

    Public moCtx As Microsoft.AspNetCore.Http.HttpContext

    'Session / Request Level Properties
    Public moRequest As System.Web.HttpRequest
    Public moResponse As System.Web.HttpResponse
    Public moSession As System.Web.SessionState.HttpSessionState
    Public moHost As IWebHostEnvironment

    Public mcPagePath As String
    Public mcPageLayout As String
    Public mnPageId As Integer = 0
    Public mnArtId As Integer = 0
    Public mnUserId As Integer = 0

    Public mbAdminMode As Boolean = False

    Private mbSystemPage As Boolean = False
    Private mnUserPagePermission As Cms.dbHelper.PermissionLevel = Cms.dbHelper.PermissionLevel.Open

    Public mbOutputXml As Boolean = False

    Public WithEvents moDbHelper As Cms.dbHelper

    'Application Level Properties   
    ' Public moConfigNew As ConfigurationSection
    Public moConfig As NameValueCollection
    '  Public goApp As System.Web.HttpApplicationState
    Public goAppCache As IMemoryCache
    Public goCache As System.Web.Caching.Cache
    Public goLangConfig As XmlElement ' = ConfigurationManager.GetSection("protean/languages")

    Public mcModuleName As String = "Protean.Base"

    Public Features As New System.Collections.Generic.Dictionary(Of String, String)

    Public mbPreview As Boolean = False
    Public mbPreviewHidden As Boolean = False

    Public PerfMon As PerfLog

#End Region

#Region "Constructors"
    Public Sub New()

        'Me.New(Microsoft.AspNetCore.Http.HttpContext)

    End Sub


    Public Sub New(ByVal Context As Microsoft.AspNetCore.Http.HttpContext, _host As IWebHostEnvironment, ByRef MemCache As IMemoryCache)


        Dim sProcessInfo As String = ""
        Try

            'Dim nMemUse As Integer = Process.GetCurrentProcess.WorkingSet64

            If moCtx Is Nothing Then
                moCtx = Context
            End If

            ' goApp = moCtx.Application
            goAppCache = MemCache
            moRequest = moCtx.Request
            moResponse = moCtx.Response
            moSession = moCtx.Session
            moHost = _host
            'goServer = moCtx.Server
            ' goCache = moCtx.Cache
            ' stdTools.goApp = moCtx.Application
            'stdTools.goServer = moCtx.Server

            PerfMon = New PerfLog("")
            PerfMon.Log("Base", "New")

            ' Dim PtnCfg As IConfiguration = New ConfigurationBuilder().AddJsonFile("appsettings.json").Build()

            '  Dim MyConfig As Configuration = New ConfigurationBuilder().AddJsonFile("appsettings.json").Build()
            ' Dim moConfig As IConfigurationSection = PtnCfg.GetSection("protean:web")
            moConfig = GetConfigSection("web")

            EnumberateFeatures()

        Catch ex As Exception
            'returnException(mcModuleName, "New", ex, "", sProcessInfo, gbDebug)
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, sProcessInfo))
        End Try
    End Sub

    Public Sub New(ByVal Context As Microsoft.AspNetCore.Http.HttpContext, ByRef ptnConfig As Configuration)

        Dim sProcessInfo As String = ""
        Try

            'Dim nMemUse As Integer = Process.GetCurrentProcess.WorkingSet64

            If moCtx Is Nothing Then
                moCtx = Context
            End If

            ' goApp = moCtx.Application
            moRequest = moCtx.Request
            moResponse = moCtx.Response
            moSession = moCtx.Session
            'goServer = moCtx.Server
            ' goCache = moCtx.Cache
            ' stdTools.goApp = moCtx.Application
            'stdTools.goServer = moCtx.Server

            PerfMon = New PerfLog("")
            PerfMon.Log("Base", "New")

            moConfig = GetConfigSection("web")



            '  moConfigNew = Config.GetSection("protean:web")
            EnumberateFeatures()

        Catch ex As Exception
            'returnException(mcModuleName, "New", ex, "", sProcessInfo, gbDebug)
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, sProcessInfo))
        End Try
    End Sub

    Public Function GetConfigSection(ByVal sectionName As String) As NameValueCollection
        Try
            Dim returnNVC As New NameValueCollection
            Dim MyConfig As ConfigurationRoot = New ConfigurationBuilder().AddJsonFile("appsettings.json").Build()
            Dim dicConfig As Dictionary(Of String, String)
            dicConfig = MyConfig.GetSection("Protean:" & sectionName).[Get](Of Dictionary(Of String, String))()
            For Each kvp In dicConfig
                returnNVC.Add(kvp.Key.ToString(), kvp.Value.ToString())
            Next

            Return returnNVC

        Catch ex As Exception
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""))
        End Try
    End Function
#End Region

    Public Sub EnumberateFeatures()
        Features.Add("Lite", "Lite")
        Features.Add("Pro", "Pro")
        If LCase(moConfig("Cart")) = "on" Then
            Features.Add("Cart", "Cart")
        End If
        If LCase(moConfig("Quote")) = "on" Then
            Features.Add("Quote", "Quote")
        End If
        If LCase(moConfig("Membership")) = "on" Then
            Features.Add("Membership", "Membership")
        End If
        If LCase(moConfig("MailingList")) = "on" Then
            Features.Add("MailingList", "MailingList")
        End If
        If LCase(moConfig("Search")) = "on" Or LCase(moConfig("SiteSearch")) = "on" Then
            Features.Add("Search", "Search")
        End If
        If LCase(moConfig("VersionControl")) = "on" Then
            Features.Add("VersionControl", "VersionControl")
        End If
        If LCase(moConfig("Import")) = "on" Then
            Features.Add("Import", "Import")
        End If
        If LCase(moConfig("Sync")) = "on" Then
            Features.Add("Sync", "Sync")
        End If
        If LCase(moConfig("MemberCodes")) = "on" Then
            Features.Add("MemberCodes", "MemberCodes")
        End If
        If LCase(moConfig("Subscriptions")) = "on" Then
            Features.Add("Subscriptions", "Subscriptions")
        End If
        If LCase(moConfig("Scheduler")) = "on" Then
            Features.Add("Scheduler", "Scheduler")
        End If
        If LCase(moConfig("ActivityLogging")) = "on" Or LCase(moConfig("ActivityReporting")) = "on" Then
            Features.Add("ActivityLogging", "ActivityLogging")
            Features.Add("ActivityReporting", "ActivityReporting")
        End If
        If LCase(moConfig("PageVersions")) = "on" Then
            Features.Add("PageVersions", "PageVersions")
        End If
        If Not goLangConfig Is Nothing Then
            Features.Add("MultiLanguage", "MultiLanguage")
        End If
        'If File.Exists(CStr(goServer.MapPath("/")) & "eonic.theme.config") Then
        'Features.Add("Themes", "Themes")
        'End If

    End Sub

    Public Function MapPath(path As String) As String
        Return moHost.WebRootPath & path
    End Function

    Private disposedValue As Boolean = False        ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)

        Me.disposedValue = True
    End Sub

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class
