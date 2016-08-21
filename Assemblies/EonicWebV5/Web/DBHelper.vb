    '***********************************************************************
    ' $Library:     eonic.dbhelper
    ' $Revision:    3.1  
    ' $Date:        2006-03-02
    ' $Author:      Trevor Spink (trevor@eonic.co.uk)
    ' &Website:     www.eonic.co.uk
    ' &Licence:     All Rights Reserved.
    ' $Copyright:   Copyright (c) 2002 - 2006 Eonic Ltd.
    '***********************************************************************

    Option Strict Off
    Option Explicit On 
    Imports System.Data
    Imports System.Data.sqlClient
    Imports System.xml
    Imports System.IO
    Imports System.web.Configuration
    Imports System.Collections
    Imports System.Collections.Generic
    Imports VB = Microsoft.VisualBasic
    Imports System.Web.Mail
    Imports System.Text.RegularExpressions
    Imports System.Net.Mail
    Imports Eonic.Tools.Dictionary
    Imports Eonic.xmlTools
Imports Eonic.Tools.Xml
Imports System

    Partial Public Class Web

        Public Class dbHelper
            'Inherits dbTools
            Inherits Eonic.Tools.Database

                        #Region "New Error Handling"
        Public Shadows Event OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)

        Private Sub _OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs) Handles MyBase.OnError
            RaiseEvent OnError(sender, e)
        End Sub

#End Region

#Region "Declarations"

        Shadows Const mcModuleName As String = "Eonic.dbHelper"

        Private moCtx As System.Web.HttpContext

        Private goApp As System.Web.HttpApplicationState

        Public goRequest As System.Web.HttpRequest
        Public goResponse As System.Web.HttpResponse
        Public goSession As System.Web.SessionState.HttpSessionState ' we need to pass this through from Web
        Public goServer As System.Web.HttpServerUtility
        Public moPageXml As XmlDocument

        Public goConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("eonic/web")

        Public mnUserId As Long

        Public gbAdminMode As Boolean = False
        Public myWeb As Eonic.Web

        Private moDataAdpt As SqlDataAdapter
        Private nCurrentPermissionLevel As PermissionLevel = PermissionLevel.Open

        Private moMessaging As Eonic.Providers.Messaging.BaseProvider

#End Region
#Region "Initialisation"


        Public Sub New(ByRef aWeb As Eonic.Web)
            MyBase.New()
            PerfMon.Log("dbHelper", "New")
            Try

                If moCtx Is Nothing Then
                    moCtx = aWeb.moCtx
                End If
                'If Not (moCtx Is Nothing) Then
                goApp = moCtx.Application
                goRequest = moCtx.Request
                goResponse = moCtx.Response
                goSession = moCtx.Session
                goServer = moCtx.Server
                'End If

                ResetConnection("Data Source=" & goConfig("DatabaseServer") & "; " & _
                    "Initial Catalog=" & goConfig("DatabaseName") & "; " & _
                    GetDBAuth())

                myWeb = aWeb
                moPageXml = myWeb.moPageXml
                mnUserId = myWeb.mnUserId

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""))
            End Try
        End Sub

        Public Function GetDBAuth() As String
            PerfMon.Log("dbHelper", "getDBAuth")
            Try
                Dim dbAuth As String
                If goConfig("DatabasePassword") <> "" Then
                    dbAuth = "user id=" & goConfig("DatabaseUsername") & "; password=" & goConfig("DatabasePassword")
                Else
                    If goConfig("DatabaseAuth") <> "" Then
                        dbAuth = goConfig("DatabaseAuth")
                    Else
                        dbAuth = "Integrated Security=SSPI;"
                    End If
                End If
                Return dbAuth
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getDBAuth", ex, ""))
                Return Nothing
            End Try
        End Function


        Public Sub New(ByVal cConnectionString As String, ByVal nUserId As Long, Optional ByVal moCtx As System.Web.HttpContext = Nothing)
            'MyBase.New(cConnectionString)
            Try

                If moCtx Is Nothing Then
                    moCtx = System.Web.HttpContext.Current
                End If

                If Not moCtx Is Nothing Then
                    goApp = moCtx.Application
                    goRequest = moCtx.Request
                    goResponse = moCtx.Response
                    goSession = moCtx.Session
                    goServer = moCtx.Server
                End If


                myWeb = Nothing
                'moPageXml = myWeb.moPageXml
                mnUserId = nUserId

                ResetConnection(cConnectionString)
                MyBase.ConnectionPooling = True
                MyBase.ConnectTimeout = 15
                MyBase.MinPoolSize = 0
                MyBase.MaxPoolSize = 100
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""))
            End Try
        End Sub

        Public Sub New(ByVal cDbServer As String, ByVal cDbName As String, ByVal nUserId As Long, Optional ByVal moCtx As System.Web.HttpContext = Nothing)
            MyBase.New()

            Try
                If moCtx Is Nothing Then
                    moCtx = System.Web.HttpContext.Current
                End If

                goApp = moCtx.Application
                goRequest = moCtx.Request
                goResponse = moCtx.Response
                goSession = moCtx.Session
                goServer = moCtx.Server

                ResetConnection("Data Source=" & cDbServer & "; " & _
                                "Initial Catalog=" & cDbName & "; " & _
                                GetDBAuth())

                myWeb = Nothing
                'moPageXml = myWeb.moPageXml
                mnUserId = nUserId

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""))
            End Try
        End Sub

        Public Sub ResetConnection(ByVal cConnectionString As String)
            Dim cPassword As String
            Dim cUsername As String
            Dim cDBAuth As String
            Try
                Dim ocon As New SqlConnection(cConnectionString)
                MyBase.DatabaseName = ocon.Database
                MyBase.DatabaseServer = ocon.DataSource

                ' Let's work out where to get the authorisation from - ideally it should be the connection string.
                If cConnectionString.ToLower.Contains("user id=") And cConnectionString.ToLower.Contains("password=") Then
                    cDBAuth = cConnectionString
                Else
                    ' No authorisation information provided in the connection string.  
                    ' We need to source it from somewhere, let's try the web.config 
                    cDBAuth = GetDBAuth()
                End If

                ' Let's find the username and password
                cUsername = Eonic.Tools.Text.SimpleRegexFind(cDBAuth, "user id=([^;]*)", 1, Text.RegularExpressions.RegexOptions.IgnoreCase)
                cPassword = Eonic.Tools.Text.SimpleRegexFind(cDBAuth, "password=([^;]*)", 1, Text.RegularExpressions.RegexOptions.IgnoreCase)

                MyBase.DatabaseUser = cUsername
                MyBase.DatabasePassword = cPassword

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "ResetConnection", ex, ""))
            End Try
        End Sub


#End Region
#Region "Enums"
        Enum objectTypes
            Content = 0
            ContentLocation = 1
            ContentRelation = 2
            ContentStructure = 3
            Directory = 4
            DirectoryRelation = 5
            Permission = 6
            QuestionaireResult = 7
            QuestionaireResultDetail = 8
            Audit = 9
            CourseResult = 10
            ActivityLog = 11
            CartOrder = 12
            CartItem = 13
            CartContact = 14
            CartShippingLocation = 15
            CartShippingMethod = 16
            CartShippingRelations = 17
            CartCatProductRelations = 18
            CartDiscountRules = 19
            CartDiscountDirRelations = 20
            CartDiscountProdCatRelations = 21
            CartProductCategories = 22
            ScheduledItem = 23
            Subscription = 24
            CartPaymentMethod = 25
            Codes = 26
            ContentVersion = 27
            CartShippingPermission = 28
            PageVersion = 29
            Lookup = 30
            CartDelivery = 31
            CartCarrier = 32

            '100-199 reserved for LMS
            CpdLog = 100
            Certificate = 101

            '200- reserved for [next thing]
        End Enum

        Enum TableNames
            tblContent = 0
            tblContentLocation = 1
            tblContentRelation = 2
            tblContentStructure = 3 'also 29
            tblDirectory = 4
            tblDirectoryRelation = 5
            tblDirectoryPermission = 6
            tblQuestionaireResult = 7
            tblQuestionaireResultDetail = 8
            tblAudit = 9
            tblCourseResult = 10
            tblActivityLog = 11
            tblCartOrder = 12
            tblCartItem = 13
            tblCartContact = 14
            tblCartShippingLocations = 15 'was tblCartShippingLocatiosn
            tblCartShippingMethods = 16 'was tblCartShippingMethod
            tblCartShippingRelations = 17
            tblCartCatProductRelations = 18
            tblCartDiscountRules = 19
            tblCartDiscountDirRelations = 20
            tblCartDiscountProdCatRelations = 21
            tblCartProductCategories = 22
            tblActions = 23 'was "tblActions" in GetTable
            tblSubscription = 24 'was tblDirectorySubscription
            tblCartPaymentMethod = 25
            tblCodes = 26
            tblContentVersions = 27
            tblCartShippingPermission = 28
            'tblContentStructure = 29 'duplicate, but leave this
            tblLookup = 30
            tblCartOrderDelivery = 31
            tblCartCarrier = 32

            '100-199 reserved for LMS
            tblCpdLog = 100
            tblCertificate = 101

            '200- reserved for [next thing]

        End Enum

        Enum PermissionLevel
            Denied = 0 ' delete
            Open = 1 ' not really used for DBe
            View = 2
            Add = 3
            AddUpdateOwn = 4
            UpdateAll = 5
            Approve = 6
            AddUpdateOwnPublish = 7
            Publish = 8
            Full = 9
        End Enum

        Enum Status
            Hidden = 0
            Live = 1
            Superceded = 2
            Pending = 3
            InProgress = 4
            Rejected = 5
            DraftSuperceded = 6
            Lead_AwaitingAuditBooking = 7
            Lead_AuditBooked = 8
            Lead_QuoteSupplied = 9
            Lead_QuoteDeclined = 10
            Lead_QuoteAccepted = 11
            Lead_LeadRejected = 12
        End Enum

        Enum ActivityType
            Undefined = 0
            'General
            Logon = 1
            PageViewed = 2
            Email = 3
            Logoff = 4
            Alert = 5
            ContentDetailViewed = 6
            SessionStart = 7
            SessionEnd = 8
            SessionContinuation = 9
            Register = 10
            DocumentDownloaded = 11
            Search = 12
            FuzzySearch = 13
            ReportDownloaded = 14
            SessionReconnectFromCookie = 15
            LogonInvalidPassword = 16
            HistoricPassword = 17

            ' Audit changes 
            StatusChangeLive = 30
            StatusChangeHidden = 31
            StatusChangeApproved = 32
            StatusChangePending = 34
            StatusChangeInProgress = 35
            StatusChangeRejected = 36
            StatusChangeSuperceded = 37

            'Admin - Starting at 40
            ContentAdded = 40
            ContentEdited = 41
            ContentHidden = 42
            ContentDeleted = 43
            ContentImport = 44

            PageAdded = 60
            PageEdited = 61
            PageHidden = 62
            PageDeleted = 63

            SetupDataUpgrade = 70

            ' Notifications
            NewsLetterSent = 80
            PendingNotificationSent = 81
            JobApplication = 82

            'Custom
            Custom1 = 98
            Custom2 = 99

            'Polls
            SubmitVote = 110
            VoteExcluded = 111

            ' Syndication
            SyndicationStarted = 200
            SyndicationInProgress = 201
            SyndicationPartialSuccess = 202
            SyndicationFailed = 203
            SyndicationCompleted = 204
            ContentSyndicated = 205

            'OpenQuote
            ValidationError = 255

            'Integrations - 900+
            IntegrationTwitterPost = 901

        End Enum

        Enum DirectoryType
            User = 0
            Department = 1
            Company = 2
            Group = 3
            Role = 4
        End Enum

        Enum CopyContentType
            None = 0
            Copy = 1
            Locate = 2
            LocateWithPrimary = 3
        End Enum

        ' Note - base 2 so they can be combined
        Enum RelationType
            Parent = 1
            Child = 2
        End Enum

        Enum CodeType
            Membership = 1
            Discount = 2
        End Enum

        Enum PageVersionType
            Personalisation = 1
            WorkingCopy = 2
            Language = 3
            SplitTest = 4
        End Enum


#End Region
#Region "Properties"
        Friend Property CurrentPermissionLevel() As PermissionLevel
            Get
                Return nCurrentPermissionLevel
            End Get
            Set(ByVal value As PermissionLevel)
                nCurrentPermissionLevel = value
            End Set
        End Property


#End Region
#Region "Shared Functions"

        Shared Function CanPublish(ByVal nPermissionLevel As PermissionLevel) As Boolean
            Try
                Return (nPermissionLevel = PermissionLevel.Publish _
                        Or nPermissionLevel = PermissionLevel.AddUpdateOwnPublish _
                        Or nPermissionLevel = PermissionLevel.Approve _
                        Or nPermissionLevel = PermissionLevel.Full)
            Catch ex As Exception
                Return False
            End Try
        End Function

        Shared Function CanAddUpdate(ByVal nPermissionLevel As PermissionLevel) As Boolean
            Try
                Return (nPermissionLevel = PermissionLevel.Add _
                        Or nPermissionLevel = PermissionLevel.AddUpdateOwnPublish _
                        Or nPermissionLevel = PermissionLevel.AddUpdateOwn _
                        Or nPermissionLevel = PermissionLevel.UpdateAll _
                        Or nPermissionLevel = PermissionLevel.Full)
            Catch ex As Exception
                Return False
            End Try
        End Function

        Shared Function CanOnlyUseOwn(ByVal nPermissionLevel As PermissionLevel) As Boolean
            Try
                Return (nPermissionLevel = PermissionLevel.AddUpdateOwnPublish _
                        Or nPermissionLevel = PermissionLevel.AddUpdateOwn)
            Catch ex As Exception
                Return False
            End Try
        End Function

#End Region
#Region "Table Definition Procedures"
        Public Function TableKey(ByVal cTableName As TableNames) As String
            Return getKey(cTableName)
        End Function

        Public Function getTable(ByVal objectType As objectTypes) As String
            PerfMon.Log("DBHelper", "getTable")
            Dim cProcessInfo As String = ""
            Dim cObjectName As String

            Try

                objectType = CInt(objectType)
                'hack for tblContentStructure = 29. It is already declared as 3.
                If objectType = 29 Then
                    cObjectName = "tblContentStructure"
                Else
                    cObjectName = [Enum].GetName(GetType(TableNames), objectType)
                End If

                Return cObjectName

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getNameByKey", ex, cProcessInfo))
                Return ""
            End Try

            'Select Case objectType
            '    Case 0
            '        Return "tblContent"
            '    Case 1
            '        Return "tblContentLocation"
            '    Case 2
            '        Return "tblContentRelation"
            '    Case 3, 29
            '        Return "tblContentStructure"
            '    Case 4
            '        Return "tblDirectory"
            '    Case 5
            '        Return "tblDirectoryRelation"
            '    Case 6
            '        Return "tblDirectoryPermission"
            '    Case 7
            '        Return "tblQuestionaireResult"
            '    Case 8
            '        Return "tblQuestionaireResultDetail"
            '    Case 9
            '        Return "tblAudit"
            '    Case 10
            '        Return "tblCourseResult"
            '    Case 11
            '        Return "tblActivityLog"
            '    Case 12
            '        Return "tblCartOrder"
            '    Case 13
            '        Return "tblCartItem"
            '    Case 14
            '        Return "tblCartContact"
            '    Case 15
            '        Return "tblCartShippingLocations"
            '    Case 16
            '        Return "tblCartShippingMethods"
            '    Case 17
            '        Return "tblCartShippingRelations"
            '    Case 18
            '        Return "tblCartCatProductRelations"
            '    Case 19
            '        Return "tblCartDiscountRules"
            '    Case 20
            '        Return "tblCartDiscountDirRelations"
            '    Case 21
            '        Return "tblCartDiscountProdCatRelations"
            '    Case 22
            '        Return "tblCartProductCategories"
            '    Case 23
            '        Return "tblActions"
            '    Case 24
            '        Return "tblDirectorySubscriptions"
            '    Case 25
            '        Return "tblCartPaymentMethod"
            '    Case 26
            '        Return "tblCodes"
            '    Case 27
            '        Return "tblContentVersions"
            '    Case 28
            '        Return "tblCartShippingPermission"
            '        'Case 29
            '        '    Return "tblContentStructure"
            '    Case Else
            '        Return ""
            'End Select
        End Function

        Protected Friend Function getKey(ByVal objectType As objectTypes) As String
            PerfMon.Log("DBHelper", "getKey")
            Dim strReturn As String = ""
            Select Case objectType
                Case 0
                    strReturn = "nContentKey"
                Case 1
                    strReturn = "nContentLocationKey" '"nContentId"
                Case 2
                    strReturn = "nContentRelationKey"
                Case 3, 29
                    strReturn = "nStructKey"
                Case 4
                    strReturn = "nDirKey"
                Case 5
                    strReturn = "nRelKey"
                Case 6
                    strReturn = "nPermKey"
                Case 7
                    strReturn = "nQResultsKey"
                Case 8
                    strReturn = "nQDetailKey"
                Case 9
                    strReturn = "nAuditKey"
                Case 10
                    strReturn = "nCourseResultKey"
                Case 11
                    strReturn = "nActivityKey"
                Case 12
                    strReturn = "nCartOrderKey"
                Case 13
                    strReturn = "nCartItemKey"
                Case 14
                    strReturn = "nContactKey"
                Case 15
                    strReturn = "nLocationKey"
                Case 16
                    strReturn = "nShipOptKey"
                Case 17
                    strReturn = "nShpRelKey"
                Case 18
                    Return "nCatProductRelKey"
                Case 19
                    Return "nDiscountKey"
                Case 20
                    Return "nDiscountDirRelationKey"
                Case 21
                    Return "nDiscountProdCatRelationKey"
                Case 22
                    Return "nCatKey"
                Case 23
                    Return "nActionKey"
                Case 24
                    Return "nSubKey"
                Case 25
                    Return "nPayMthdKey"
                Case 26
                    Return "nCodeKey"
                Case 27
                    Return "nContentVersionKey"
                Case 28
                    Return "nCartShippingPermissionKey"
                    'Case 29 'duplicate, but leave this
                    '    Return "nStructKey" 
                Case 30
                    Return "nLkpID"
                Case 31
                    Return "nDeliveryKey"
                Case 32
                    Return "nCarrierKey"
                    '100-199 reserved for LMS
                Case 100
                    Return "nCpdLogKey"
                Case 101
                    Return "nCertificateKey"

                    '200- reserved for [next thing]

            End Select
            Return strReturn
        End Function

        Private Function getParIdFname(ByVal objectType As objectTypes) As String
            PerfMon.Log("DBHelper", "getParIdFname")
            Dim strReturn As String = ""
            Select Case objectType

                Case 2
                    strReturn = "nContentParentId"
                Case 3
                    strReturn = "nStructParId"

                Case 15
                    strReturn = "nLocationParId"
                Case 29
                    strReturn = "nVersionParId"
                Case 30
                    strReturn = "nLkpParent"
            End Select
            Return strReturn
        End Function

        Private Function getOrderFname(ByVal objectType As objectTypes) As String
            PerfMon.Log("DBHelper", "getOrderFname")
            Dim strReturn As String = ""
            Select Case objectType
                Case 1
                    strReturn = "nDisplayOrder"
                Case 2
                    strReturn = "nDisplayOrder"
                Case 3
                    strReturn = "nStructOrder"
                Case 29
                    strReturn = "nStructOrder"
                Case 30
                    strReturn = "nDisplayOrder"
                    'Case 5
                    '    Return "nRelKey"
            End Select
            Return strReturn
        End Function

        Private Function getNameFname(ByVal objectType As objectTypes) As String
            PerfMon.Log("DBHelper", "getNameFname")
            Dim strReturn As String = ""
            Select Case objectType
                Case 0
                    strReturn = "cContentName"
                Case 3
                    strReturn = "cStructName"
                Case 4
                    strReturn = "cDirName"

            End Select
            Return strReturn
        End Function

        Private Function getSchemaFname(ByVal objectType As objectTypes) As String
            PerfMon.Log("DBHelper", "getSchemaFname")
            Dim strReturn As String = ""
            Select Case objectType
                Case 0
                    strReturn = "cContentSchemaName"
                Case 4
                    strReturn = "cDirSchema"
                Case 12
                    strReturn = "cCartSchemaName"
            End Select
            Return strReturn
        End Function

        Public Function getNameByKey(ByVal objectType As objectTypes, ByVal nKey As Long) As String
            PerfMon.Log("DBHelper", "getNameByKey")
            Dim sSql As String
            Dim oDr As SqlDataReader
            Dim sResult As String = ""
            Dim cProcessInfo As String = ""
            Try

                Select Case objectType

                    Case 0, 3, 4

                        sSql = "select " & getNameFname(objectType) & " from " & getTable(objectType) & " where " & getKey(objectType) & " = " & nKey
                        oDr = getDataReader(sSql)

                        While oDr.Read
                            sResult = oDr(0)
                        End While

                End Select

                Return sResult

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getNameByKey", ex, cProcessInfo))
                Return ""
            End Try

        End Function

        Public Function getFRef(ByVal objectType As objectTypes) As String
            PerfMon.Log("DBHelper", "getFRef")
            Select Case objectType
                Case 0
                    Return "cContentForiegnRef"
                Case 1
                    Return ""
                Case 2
                    Return ""
                Case 3
                    Return "cStructForiegnRef"
                Case 4
                    Return "cDirForiegnRef"
                Case 5
                    Return ""
                Case 6
                    Return ""
                Case 7
                    Return "cQResultsForiegnRef"
                Case 8
                    Return ""
                Case 9
                    Return ""
                Case 10
                    Return ""
                Case 11
                    Return ""
                Case 12
                    Return "cCartForiegnRef"
                Case 13
                    Return ""
                Case 14
                    Return "cContactForeignRef"
                Case 15
                    Return "cLocationForeignRef"
                Case 16
                    Return "cShipOptForeignRef"
                Case 26
                    Return "cCode"
                Case Else
                    Return ""
            End Select
        End Function

        ''' <summary>
        ''' Check if a database object has a bespoke equivalent in the database.
        ''' Bespoke objects are prefixed with the letter "b".
        ''' e.g. bsp_MyProcedure
        ''' </summary>
        ''' <param name="dbObjectName">The object name to check (without the b prefix)</param>
        ''' <returns>"b" + dbObjectName if it exists, dbObjectName otherwise</returns>
        ''' <remarks>Note: This doesn't check for the existence of dbObjectName</remarks>
        Public Function getDBObjectNameWithBespokeCheck(ByVal dbObjectName As String) As String
            Dim appcheckName As String = dbObjectName
            Try
                If String.IsNullOrEmpty(dbObjectName) Then
                    appcheckName = goApp("getdb-" & dbObjectName)
                    If String.IsNullOrEmpty(appcheckName) Then

                        If MyBase.checkDBObjectExists("b" & dbObjectName) Then
                            appcheckName = "b" & dbObjectName
                        Else
                            appcheckName = dbObjectName
                        End If

                        'goApp.Lock()
                        goApp("getdb-" & dbObjectName) = appcheckName
                        'goApp.UnLock()

                    End If


                End If

                Return appcheckName

            Catch ex As Exception
                Return dbObjectName
            End Try

        End Function

#End Region

        Private Function getPermissionLevel(ByVal oPermLevel As PermissionLevel) As String
            Select Case oPermLevel
                Case 0
                    Return "Denied"
                Case 1
                    Return "Open"
                Case 2
                    Return "View"
                Case 3
                    Return "Add"
                Case 4
                    Return "AddUpdateOwn"
                Case 5
                    Return "UpdateAll"
                Case 6
                    Return "Approve"
                Case 7
                    Return "AddUpdateOwnPublish"
                Case 8
                    Return "Publish"
                Case 9
                    Return "Full"
                Case Else
                    Return "Null"
            End Select
        End Function

        Public Function getKeyByNameAndSchema(ByVal objectType As objectTypes, ByVal cSchemaName As String, ByVal cName As String) As String
            PerfMon.Log("DBHelper", "getKeyByNameAndSchema")
            Dim sSql As String
            Dim oDr As SqlDataReader
            Dim sResult As String = ""
            Dim cProcessInfo As String = ""
            Try

                Select Case objectType

                    Case 0, 4, 3

                        sSql = "select " & getKey(objectType) & " from " & getTable(objectType) & " where " & getNameFname(objectType) & " LIKE '" & cName & "'"
                        oDr = getDataReader(sSql)
                        If oDr.HasRows Then
                            While oDr.Read
                                sResult = oDr(0)
                            End While
                        End If

                End Select

                Return sResult

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getNameByKey", ex, cProcessInfo))
                Return ""
            End Try

        End Function

        Public Function getObjectStatus(ByVal objecttype As objectTypes, ByVal nId As Long) As Long
            PerfMon.Log("DBHelper", "getObjectStatus")
            Dim sSql As String
            Dim nAuditId As Long
            Dim oDr As SqlDataReader
            Dim sResult As String = ""
            Dim cProcessInfo As String = ""
            Try
                sSql = "select nAuditId from " & getTable(objecttype) & " where " & getKey(objecttype) & " = " & nId
                oDr = getDataReader(sSql)

                While oDr.Read
                    nAuditId = oDr(0)
                End While

                oDr.Close()
                oDr = Nothing
                ' we could test the current status to see if the change is a valid one. I.E. live can only be hidden not moved back to approval or InProgress, but the workflow should ensure this doesn't happen.
                sSql = "select nStatus from tblAudit WHERE nAuditKey =" & nAuditId
                sResult = ExeProcessSqlScalar(sSql)

                Return CLng(sResult)

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getObjectStatus", ex, cProcessInfo))
                Return ""
            End Try

        End Function

        Public Overridable Function setObjectStatus(ByVal objecttype As objectTypes, ByVal status As Status, ByVal nId As Long) As String
            PerfMon.Log("DBHelper", "setObjectStatus")
            Dim sSql As String
            Dim nAuditId As Long
            Dim oDr As SqlDataReader
            Dim sResult As String = ""
            Dim cProcessInfo As String = ""
            Try
                sSql = "select nAuditId from " & getTable(objecttype) & " where " & getKey(objecttype) & " = " & nId
                oDr = getDataReader(sSql)

                While oDr.Read
                    nAuditId = oDr(0)
                End While

                oDr.Close()
                oDr = Nothing
                ' we could test the current status to see if the change is a valid one. I.E. live can only be hidden not moved back to approval or InProgress, but the workflow should ensure this doesn't happen.
                sSql = "UPDATE tblAudit SET nStatus = " & status & " WHERE nAuditKey =" & nAuditId
                sResult = ExeProcessSql(sSql)

                If objecttype = objectTypes.ContentStructure Then
                    clearStructureCacheAll()
                End If

                Return sResult
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "setObjectStatus", ex, cProcessInfo))
                Return ""
            End Try

        End Function

        ''' <summary>
        ''' Assess an audit id, it's previous and new status and if changed logs the appropriate change
        ''' </summary>
        ''' <param name="auditId"></param>
        ''' <param name="oldStatus"></param>
        ''' <param name="newStatus"></param>
        ''' <remarks></remarks>
        Private Sub logObjectStatusChange(ByVal auditId As Long, ByVal oldStatus As Status, ByVal newStatus As Status)
            PerfMon.Log("DBHelper", "logObjectStatusChange")
            Dim cProcessInfo As String = ""
            Dim statusChange As ActivityType = ActivityType.Undefined
            Try
                ' Check if something's changed
                If oldStatus <> newStatus Then

                    ' Process the changes, start with the absolutes
                    If newStatus = Status.Rejected Then
                        statusChange = ActivityType.StatusChangeRejected

                    ElseIf newStatus = Status.Superceded Then
                        statusChange = ActivityType.StatusChangeSuperceded

                    ElseIf newStatus = Status.Pending Then
                        statusChange = ActivityType.StatusChangePending

                    ElseIf newStatus = Status.InProgress Then
                        statusChange = ActivityType.StatusChangeInProgress

                    ElseIf newStatus = Status.Live And oldStatus = Status.Pending Then
                        statusChange = ActivityType.StatusChangeApproved

                    ElseIf newStatus = Status.Live Then
                        statusChange = ActivityType.StatusChangeLive
                    End If

                    ' If status has changed then log it
                    If statusChange <> ActivityType.Undefined Then
                        logActivity(statusChange, myWeb.mnUserId, 0, 0, Convert.ToInt16(auditId), "")
                    End If

                End If
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "logObjectStatusChange", ex, cProcessInfo))
            End Try
        End Sub


        Friend Function getPageIdFromPath(ByVal sFullPath As String, Optional ByVal bSetGlobalPageVariable As Boolean = True, Optional ByVal bCheckPermissions As Boolean = True) As Integer
            PerfMon.Log("dbHelper", "getPageIdFromPath")
            Dim aPath() As String
            Dim sPath As String

            Dim sSql As String
            Dim ods As DataSet
            Dim oRow As DataRow
            Dim nPageId As Integer

            Dim sProcessInfo As String = ""

            Try

                sPath = sFullPath
                sPath = goServer.UrlDecode(sPath)

                ' We have to assume that hyphens are spaces here
                ' Nore : if this is turned on, you will have to update any pages that have hyphens in their names
                If myWeb.moConfig("PageURLFormat") = "hyphens" Then
                    sPath = Replace(sPath, "-", " ")
                End If

                sProcessInfo = "remove first and final /"

                If Right(sPath, 1) = "/" Then
                    sPath = Left(sPath, Len(sPath) - 1)
                End If
                If InStr(1, sPath, "/") = 1 Then
                    sPath = Right(sPath, Len(sPath) - 1)
                End If

                sProcessInfo = "Strip the QueryString"

                If InStr(1, sPath, "?") > 0 Then sPath = Left(sPath, InStr(1, sPath, "?") - 1)

                aPath = Split(sPath, "/")

                If UBound(aPath) > 0 Then

                    'get the last in line
                    sPath = aPath(UBound(aPath))
                Else
                    sPath = sPath
                End If

                sSql = "select nStructKey, nStructParId from tblContentStructure where (cStructName like '" & SqlFmt(sPath) & "' or cStructName like '" & SqlFmt(Replace(sPath, " ", "")) & "' or cStructName like '" & SqlFmt(Replace(sPath, " ", "-")) & "')"

                ods = GetDataSet(sSql, "Pages")


                If ods.Tables("Pages").Rows.Count = 1 Then
                    nPageId = ods.Tables("Pages").Rows("0").Item("nStructKey")
                    ' if there is just one page validate it
                ElseIf ods.Tables("Pages").Rows.Count = 0 Then



                Else
                    For Each oRow In ods.Tables("Pages").Rows
                        Debug.WriteLine(oRow.Item("nStructKey"))
                        If recurseUpPathArray(oRow.Item("nStructParId"), aPath, UBound(aPath) - 1) = True Then
                            If bCheckPermissions Then

                                ' Check the permissions for the page - this will either return 0, the page id or a system page.
                                Dim checkPermissionPageId As Long = checkPagePermission(oRow.Item("nStructKey"))

                                If checkPermissionPageId <> 0 _
                                    And (oRow.Item("nStructKey") = checkPermissionPageId _
                                    Or IsSystemPage(checkPermissionPageId)) Then
                                    nPageId = checkPermissionPageId
                                    Exit For
                                End If
                            Else
                                nPageId = oRow.Item("nStructKey")
                                Exit For
                            End If
                        End If
                    Next
                End If


                ' Note : if sPath is empty the SQL call above WILL return pages, we don't want these, we want top level pgid
                If Not (nPageId > 1 And (sPath <> "")) Then
                    'page path cannot be found we have an error that we raise later
                    If sFullPath <> "System+Pages/Page+Not+Found" Then
                        nPageId = myWeb.gnPageNotFoundId
                    Else
                        nPageId = myWeb.RootPageId
                    End If
                End If

                'If bSetGlobalPageVariable Then gnPageId = nPageId

                PerfMon.Log("dbHelper", "getPageIdFromPath-end")
                Return nPageId
            Catch ex As Exception

                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getPageIdFromPath", ex, sProcessInfo))

            End Try
        End Function

        Public Function IsSystemPage(ByVal pageId As Long) As Boolean

            Try
                Return pageId <> 0 And (pageId = myWeb.gnPageAccessDeniedId _
                                                    Or pageId = myWeb.gnPageErrorId _
                                                    Or pageId = myWeb.gnPageLoginRequiredId _
                                                    Or pageId = myWeb.gnPageNotFoundId)
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "IsSystemPage", ex, ""))
                Return False
            End Try

        End Function

        '''' <summary>
        '''' <para>
        '''' Check the request for certain parameters which if matched will create a 302 redirect
        '''' For page redirects, ISAPI can't detect what we need, so we are going to need to check if PageURLFormat is set
        '''' For content (artid) redirects, parameters are driven by the ISAPI rewrite routines which should be updated to do the following
        '''' </para>
        '''' <list>
        ''''   <item>
        ''''    If itemNNNN is detected then set u the redirect as follows:
        ''''    <list>
        ''''       <item>pass the path through as path</item>
        ''''       <item>pass itemNNNN through as artid=NNNN</item>
        ''''       <item>set a flag called redirect=statuscode</item>
        ''''    </list>
        ''''   </item>
        '''' </list>
        '''' <para>
        '''' The redirect URL will be "redirPath/NNNN-/Product-Name
        '''' </para>
        '''' </summary>
        '''' <returns></returns>
        '''' <remarks>This may require the existing pages to be tidied up (i.e. removing hyphens and plusses)</remarks>
        'Friend Function legacyRedirection() As Boolean
        '    PerfMon.Log("DBHelper", "legacyRedirection")

        '    Dim cProcessInfo As String = ""
        '    Dim bRedirect As Boolean = False

        '    Try

        '        If myWeb.moConfig("LegacyRedirect") = "on" Then

        '            Dim cPath As String = myWeb.moRequest("path") & ""
        '            If Not (cPath.StartsWith("/")) Then cPath = "/" & cPath
        '            If Not (cPath.EndsWith("/")) Then cPath &= "/"

        '            ' The checks we need to make are as follows:
        '            ' Is RedirPath being passed through (from ISAPI Rewrite)
        '            ' Is artid being passed through

        '            ' Check if an article redirect has been called
        '            If Not (myWeb.moRequest("redirect") Is Nothing) _
        '                AndAlso myWeb.moRequest("artid") <> "" _
        '                AndAlso IsNumeric(myWeb.moRequest("artid")) Then

        '                ' Try to find the product
        '                Dim nArtId As Long = CLng(myWeb.moRequest("artid"))
        '                Dim cSql As String = "SELECT cContentName FROM tblContent WHERE nContentKey = " & SqlFmt(nArtId.ToString)
        '                Dim cName As String = GetDataValue(cSql, , , "")



        '                If Not (String.IsNullOrEmpty(cName)) Then

        '                    ' Replace any non-alphanumeric with hyphens
        '                    Dim oRe As New Text.RegularExpressions.Regex("[^A-Z0-9]", Text.RegularExpressions.RegexOptions.IgnoreCase)
        '                    cName = oRe.Replace(cName, "-").Trim("-")


        '                    ' Iron out the spaces for paths
        '                    cPath = Replace(cPath, " ", "-")

        '                    ' Construct the new path
        '                    cPath = cPath & nArtId.ToString & "-/" & cName
        '                    bRedirect = True

        '                End If

        '            ElseIf myWeb.moConfig("PageURLFormat") = "hyphens" Then
        '                ' Check the path - if hyphens is the preference then we need to analyse the path and rewrite it.
        '                If cPath.Contains("+") Or cPath.Contains(" ") Then
        '                    cPath = Replace(cPath, "+", "-")
        '                    cPath = Replace(cPath, " ", "-")
        '                    bRedirect = True
        '                End If
        '            End If

        '            ' Redirect
        '            If bRedirect And myWeb.moSession("legacyRedirect") <> "on" Then
        '                ' Stop recursive redirects
        '                myWeb.moSession("legacyRedirect") = "on"

        '                ' Assume status code is 301 unless instructed otherwise.
        '                Dim nResponseCode As Integer = 301
        '                Select Case myWeb.moRequest("redirect")
        '                    Case "301", "302", "303", "304", "307"
        '                        nResponseCode = CInt(myWeb.moRequest("redirect"))
        '                End Select
        '                HTTPRedirect(myWeb.moResponse, cPath, nResponseCode)

        '            Else
        '                myWeb.moSession("legacyRedirect") = "off"
        '            End If

        '        End If

        '        Return bRedirect

        '    Catch ex As Exception
        '        RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "legacyRedirection", ex, cProcessInfo))
        '        Return False
        '    End Try

        'End Function

        Friend Function getPageLayout(ByVal nPageId As Long) As String
            PerfMon.Log("DBHelper", "getPageLayout")

            Dim cLayout As String = ""
            Dim cSql As String = ""

            Try

                If gbClone Then
                    ' If the page is cloned then we need to look at the page that it's cloned from
                    Dim nClonePageId As Long = GetDataValue("select nCloneStructId from tblContentStructure where nStructKey = " & nPageId, , , 0)
                    If nClonePageId > 0 Then nPageId = nClonePageId
                End If

                cSql = "select cStructLayout from  tblContentStructure where nStructKey = " & nPageId
                cLayout = GetDataValue(cSql, , , "default")

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getPageLayout", ex, cSql))
            End Try

            Return cLayout

        End Function


        Friend Function getPageLang(ByVal nPageId As Long) As String
            PerfMon.Log("DBHelper", "getPageLang")

            Dim cLayout As String = ""
            Dim cSql As String = ""

            Try

                If gbClone Then
                    ' If the page is cloned then we need to look at the page that it's cloned from
                    Dim nClonePageId As Long = GetDataValue("select nCloneStructId from tblContentStructure where nStructKey = " & nPageId, , , 0)
                    If nClonePageId > 0 Then nPageId = nClonePageId
                End If

                cSql = "select cVersionLang from  tblContentStructure where nStructKey = " & nPageId
                cLayout = GetDataValue(cSql, , , "default")

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getPageLang", ex, cSql))
            End Try

            Return cLayout

        End Function



        ''' <summary>
        '''    Given a parent id and a page name (from an array of page paths) this works out if the parent matched the page name.
        '''    It is intended to give a confirmation of page ids for ambiguous page name by working up the path.
        ''' </summary>
        ''' <param name="nParentid">The parent id of the page you've come from</param>
        ''' <param name="aPath">An array of page names from a path</param>
        ''' <param name="nStep">The level of the array to inspect</param>
        ''' <returns>Returns True if the paths match the page IDs all the way up the array</returns>
        ''' <remarks></remarks>
        Private Function recurseUpPathArray(ByVal nParentid As Integer, ByRef aPath() As String, ByRef nStep As Integer) As Boolean
            PerfMon.Log("Base", "recurseUpPathArray")
            Dim oDr As SqlDataReader
            Dim sSql As String
            Dim sProcessInfo As String = ""

            recurseUpPathArray = False

            Try
                If nStep > -1 Then

                    sSql = "select nStructKey, nStructParId from tblContentStructure where nStructKey =" & nParentid & " and (cStructName like '" & SqlFmt(aPath(nStep)) & "' or cStructName like '" & SqlFmt(Replace(aPath(nStep), " ", "")) & "')"
                    oDr = getDataReader(sSql)

                    If Not oDr.HasRows Then
                        recurseUpPathArray = False
                    Else
                        oDr.Read()
                        Dim nParId As Long = oDr("nStructParId")
                        recurseUpPathArray = recurseUpPathArray(nParId, aPath, nStep - 1)
                    End If
                    oDr.Close()
                    oDr = Nothing

                ElseIf nStep = -1 And nParentid = myWeb.RootPageId Then
                    recurseUpPathArray = True
                Else
                    recurseUpPathArray = False
                End If

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getPageIdFromPath", ex, sProcessInfo))
            End Try
        End Function

        Public Function checkPagePermission(ByVal nPageId As Long) As Integer
            PerfMon.Log("DBHelper", "checkPagePermission")
            Dim sProcessInfo As String = ""
            Dim nAuthGroup As Long = gnAuthUsers

            Try
                ' if we are not in admin mode
                If Not gbAdminMode Then
                    'Check we have access to this page
                    Dim nAuthUserId As Long
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
                    End If
                    Dim oPerm As Object

                    oPerm = GetDataValue("SELECT dbo.fxn_checkPermission (" & nPageId & ", " & nAuthUserId & "," & nAuthGroup & ") AS perm")
                    If Not (oPerm Is DBNull.Value Or oPerm Is Nothing) Then
                        If InStr(oPerm, "DENIED") > 0 Then
                            If mnUserId > 0 Then
                                nPageId = myWeb.gnPageAccessDeniedId
                            Else
                                nPageId = myWeb.gnPageLoginRequiredId
                            End If
                        End If

                        If oPerm = "VIEW by Authenticated Users" And mnUserId = 0 Then
                            nPageId = myWeb.gnPageLoginRequiredId
                        End If

                    End If
                    Return nPageId
                Else
                    Return nPageId
                End If
                PerfMon.Log("DBHelper", "checkPagePermission-end")

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getPageIdFromPath", ex, sProcessInfo))
            End Try
        End Function


        Public Function checkPageExist(ByVal nPageId As Long) As Boolean
            PerfMon.Log("Base", "checkPageExist")
            Dim sProcessInfo As String = ""
            Dim nId As Object

            Try

                nId = GetDataValue("SELECT nStructKey from tblContentStructure where nStructKey = " & nPageId)
                If nId Is Nothing Then
                    Return False
                Else
                    Return True
                End If

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "checkPageExist", ex, sProcessInfo))
            End Try
        End Function


        Public Function getPagePermissionLevel(ByVal nPageId As Long) As PermissionLevel
            PerfMon.Log("DBHelper", "getPagePermissionLevel")
            Dim sProcessInfo As String = ""

            Try
                'Check if we are Domain Super Admin
                'RJP 7 Nov 2012. Amended to use Lower Case to prevent against case sensitive entries in Eonic.Web.Config, previously used the string "md5"
                If myWeb.moSession("ewAuth") = Eonic.Tools.Encryption.HashString(myWeb.moSession.SessionID & goConfig("AdminPassword"), LCase(myWeb.moConfig("MembershipEncryption")), True) Then
                    Return PermissionLevel.Full
                ElseIf checkUserRole("Administrator") Then
                    Return PermissionLevel.Full
                Else
                    'Check we have access to this page
                    Dim nAuthUserId As Long
                    If mnUserId = 0 And gnNonAuthUsers <> 0 Then
                        nAuthUserId = gnNonAuthUsers
                    Else
                        nAuthUserId = mnUserId
                    End If
                    Dim sPerm As String
                    Dim sSql As String = "SELECT dbo.fxn_checkPermission(" & nPageId & ", " & nAuthUserId & "," & gnAuthUsers & ") AS perm"
                    sPerm = GetDataValue(sSql)
                    If Not (sPerm Is DBNull.Value Or sPerm Is Nothing) Then
                        If sPerm.Contains("ADDUPDATEOWNPUBLISH") Then
                            Return PermissionLevel.AddUpdateOwnPublish
                        ElseIf sPerm.Contains("ADDUPDATEOWN") Then
                            Return PermissionLevel.AddUpdateOwn
                        ElseIf sPerm.Contains("DENIED") Then
                            Return PermissionLevel.Denied
                        ElseIf sPerm.Contains("OPEN") Then
                            Return PermissionLevel.Open
                        ElseIf sPerm.Contains("VIEW") Then
                            Return PermissionLevel.View
                        ElseIf sPerm.Contains("ADD") Then
                            Return PermissionLevel.Add
                        ElseIf sPerm.Contains("UPDATEALL") Then
                            Return PermissionLevel.UpdateAll
                        ElseIf sPerm.Contains("APPROVE") Then
                            Return PermissionLevel.Approve
                        ElseIf sPerm.Contains("PUBLISH") Then
                            Return PermissionLevel.Publish
                        ElseIf sPerm.Contains("FULL") Then
                            Return PermissionLevel.Full
                        End If
                    Else
                        Return Nothing
                    End If
                End If
                PerfMon.Log("DBHelper", "getPagePermissionLevel - END")
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getPagePermissionLevel", ex, sProcessInfo))
            End Try
        End Function


        Public Function getContentPermissionLevel(ByVal nContentId As Long, ByVal nPageId As Long) As PermissionLevel
            PerfMon.Log("DBHelper", "getContentPermissionLevel")
            Dim sProcessInfo As String = ""
            Dim oElmt As XmlElement
            Dim oElmt2 As XmlElement
            Dim oPerm As PermissionLevel = PermissionLevel.Denied
            Try

                oElmt = getLocationsByContentId(nContentId)

                For Each oElmt2 In oElmt.SelectNodes("Location")
                    If oElmt2.GetAttribute("primary") = "true" And oElmt2.GetAttribute("pgid") = nPageId Then
                        oPerm = getPagePermissionLevel(nPageId)
                    End If
                Next

                Return oPerm

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getContentPermissionLevel", ex, sProcessInfo))
            End Try
        End Function


        Public Sub DeleteXMLCache()
            PerfMon.Log("DBHelper", "DeleteXMLCache")
            'place holder until we get cashing going on V4
        End Sub

        Public Overridable Function DeleteObject(ByVal objectType As objectTypes, ByVal nId As Long, Optional ByVal bReporting As Boolean = False) As Long
            PerfMon.Log("DBHelper", "DeleteObject")
            Dim sSql As String
            Dim nAuditId As Long
            Dim oDr As SqlDataReader
            Dim bHaltDelete As Boolean = False


            Dim cProcessInfo As String = ""
            Try
                'lets use a new  because of the cascading nature of this routine

                'delete child items
                Select Case objectType
                    Case objectTypes.CartShippingLocation
                        Dim cSQL As String = "SELECT nLocationKey FROM tblCartShippingLocations WHERE nLocationParId = " & nId
                        Dim oDataReader As SqlDataReader = getDataReader(cSQL)
                        Do While oDataReader.Read
                            DeleteObject(objectTypes.CartShippingLocation, oDataReader(0))
                        Loop
                        oDataReader.Close()
                        oDataReader = Nothing
                        cSQL = "SELECT nAuditId FROM tblCartShippingLocations WHERE nLocationKey = " & nId
                        DeleteObject(objectTypes.Audit, ExeProcessSqlScalar(cSQL))
                    Case objectTypes.CartDiscountDirRelations
                        sSql = "Select  nAuditId From tblCartDiscountDirRelations Where nDiscountDirRelationKey = " & nId
                        DeleteObject(objectTypes.Audit, ExeProcessSqlScalar(sSql))

                    Case objectTypes.CartDiscountProdCatRelations
                        sSql = "Select  nAuditId From tblCartDiscountProdCatRelations Where nDiscountProdCatRelationKey = " & nId
                        DeleteObject(objectTypes.Audit, ExeProcessSqlScalar(sSql))

                    Case objectTypes.CartDiscountRules
                        sSql = "Select  nAuditId From tblCartDiscountRules Where nDiscountKey = " & nId
                        DeleteObject(objectTypes.Audit, ExeProcessSqlScalar(sSql))
                        sSql = "Select nDiscountDirRelationKey FROM tblCartDiscountDirRelations WHERE nDiscountId = " & nId
                        oDr = getDataReader(sSql)
                        Do While oDr.Read
                            DeleteObject(objectTypes.CartDiscountDirRelations, oDr(0))
                        Loop
                        oDr.Close()
                        sSql = "Select nDiscountProdCatRelationKey FROM tblCartDiscountProdCatRelations WHERE nDiscountId = " & nId
                        oDr = getDataReader(sSql)
                        Do While oDr.Read
                            DeleteObject(objectTypes.CartDiscountProdCatRelations, oDr(0))
                        Loop
                        oDr.Close()
                    Case objectTypes.CartProductCategories
                        sSql = "Select nCatProductRelKey, nAuditId From tblCartCatProductRelations Where nCatId = " & nId
                        oDr = getDataReader(sSql)
                        Do While oDr.Read
                            DeleteObject(objectTypes.CartCatProductRelations, oDr.GetValue(0))
                            DeleteObject(objectTypes.Audit, oDr.GetValue(1))
                        Loop
                        oDr.Close()
                        sSql = "Select nDiscountProdCatRelationKey FROM tblCartDiscountProdCatRelations WHERE nProductCatId = " & nId
                        oDr = getDataReader(sSql)
                        Do While oDr.Read
                            DeleteObject(objectTypes.CartDiscountProdCatRelations, oDr(0))
                        Loop
                        oDr.Close()
                    Case objectTypes.CartCatProductRelations
                        sSql = "Select nAuditId From tblCartCatProductRelations Where nCatProductRelKey = " & nId
                        oDr = getDataReader(sSql)
                        Do While oDr.Read
                            DeleteObject(objectTypes.Audit, oDr.GetValue(0))
                        Loop
                        oDr.Close()
                    Case objectTypes.Content

                        'Delete ActivityLogs for content
                        sSql = "select nActivityKey from tblActivityLog where nArtId = " & nId
                        oDr = getDataReader(sSql)
                        While oDr.Read
                            DeleteObject(objectTypes.ActivityLog, oDr(0))
                        End While
                        oDr.Close()
                        oDr = Nothing

                        'get any locations and delete
                        sSql = "select nContentLocationKey, bPrimary from tblContentLocation where nContentId = " & nId
                        oDr = getDataReader(sSql)
                        While oDr.Read
                            DeleteObject(objectTypes.ContentLocation, oDr(0))
                        End While
                        oDr.Close()
                        oDr = Nothing

                        'get any child results and delete
                        'sSql = "select nQResultsKey from tblQuestionaireResult where nContentId = " & nId
                        'oDr = getDataReader(sSql)
                        'While oDr.Read
                        '    DeleteObject(objectTypes.QuestionaireResult, oDr(0))
                        'End While
                        'oDr.Close()
                        'oDr = Nothing

                        'delete any content relations
                        sSql = "select nContentRelationKey from tblContentRelation where nContentParentId = " & nId & " or nContentChildId = " & nId
                        oDr = getDataReader(sSql)
                        While oDr.Read
                            DeleteObject(objectTypes.ContentRelation, oDr(0))
                        End While
                        oDr.Close()
                        oDr = Nothing

                    Case objectTypes.ContentRelation
                        sSql = "Select nAuditId, nContentParentID, nContentChildId From tblContentRelation Where nContentRelationKey = " & nId
                        oDr = getDataReader(sSql)
                        While oDr.Read
                            sSql = "Select nContentRelationKey From tblContentRelation WHERE nContentParentId = " & oDr(2) & " AND nContentChildId = " & oDr(1)
                            DeleteObject(objectTypes.Audit, oDr(0))
                        End While
                        oDr.Close()
                        oDr = Nothing
                        ExeProcessSql("Delete From tblContentRelation where nContentRelationKey = " & nId)
                        oDr = getDataReader(sSql)
                        Dim nOther As Integer
                        Do While oDr.Read
                            nOther = oDr(0)
                            Exit Do
                        Loop
                        oDr.Close()
                        If nOther > 0 Then DeleteObject(objectTypes.ContentRelation, nOther)
                    Case objectTypes.ContentStructure

                        'Delete ActivityLogs for page
                        sSql = "select nActivityKey from tblActivityLog where nStructId = " & nId
                        oDr = getDataReader(sSql)
                        While oDr.Read
                            DeleteObject(objectTypes.ActivityLog, oDr(0))
                        End While
                        oDr.Close()
                        oDr = Nothing

                        'delete any child pages

                        sSql = "select nStructKey from tblContentStructure where nStructParId = " & nId
                        oDr = getDataReader(sSql)
                        While oDr.Read
                            DeleteObject(objectTypes.ContentStructure, oDr(0))
                        End While
                        oDr.Close()
                        oDr = Nothing

                        'do we want to delete any content that is not also located elsewhere???? 
                        'Add Later....

                        'get any locations and delete
                        sSql = "select nContentLocationKey, bPrimary from tblContentLocation where nStructId = " & nId
                        oDr = getDataReader(sSql)
                        While oDr.Read
                            'If oDr("bPrimary") = True Then
                            DeleteObject(objectTypes.ContentLocation, oDr(0))
                            'Else
                            '     bHaltDelete = True ' there is more than one location for this content don't delete it.
                            'End If
                        End While
                        oDr.Close()
                        oDr = Nothing

                        clearStructureCacheAll()


                    Case objectTypes.QuestionaireResult
                        sSql = "select nQDetailKey from tblQuestionaireResultDetail where nQResultId = " & nId
                        oDr = getDataReader(sSql)
                        While oDr.Read
                            DeleteObject(objectTypes.QuestionaireResultDetail, oDr(0))
                        End While
                        oDr.Close()
                        oDr = Nothing

                    Case objectTypes.Directory

                        'Delete ActivityLogs
                        sSql = "select nActivityKey from tblActivityLog where nUserDirId = " & nId
                        oDr = getDataReader(sSql)
                        While oDr.Read
                            DeleteObject(objectTypes.ActivityLog, oDr(0))
                        End While
                        oDr.Close()
                        oDr = Nothing

                        'Delete Permissions
                        sSql = "select nPermKey from tblDirectoryPermission where nDirId = " & nId
                        oDr = getDataReader(sSql)
                        While oDr.Read
                            DeleteObject(objectTypes.Permission, oDr(0))
                        End While
                        oDr.Close()
                        oDr = Nothing

                        'Delete ExamResults for users
                        'sSql = "select nQResultsKey from tblQuestionaireResult where nDirId=" & nId
                        'oDr = getDataReader(sSql)
                        'While oDr.Read
                        '    DeleteObject(dbHelper.objectTypes.QuestionaireResult, oDr(0))
                        'End While
                        'oDr.Close()
                        'oDr = Nothing

                        'Delete Child Directory Objects
                        sSql = "select nDirChildId from tblDirectoryRelation where nDirParentId=" & nId
                        oDr = getDataReader(sSql)
                        While oDr.Read
                            DeleteObject(dbHelper.objectTypes.Directory, oDr(0))
                        End While
                        oDr.Close()
                        oDr = Nothing

                        'Delete Relationships
                        sSql = "select nRelKey from tblDirectoryRelation where nDirParentId = " & nId & " or nDirChildId = " & nId
                        oDr = getDataReader(sSql)
                        While oDr.Read
                            DeleteObject(objectTypes.DirectoryRelation, oDr(0))
                        End While
                        oDr.Close()
                        oDr = Nothing

                    Case objectTypes.DirectoryRelation

                        'if we are linking a user to a company we need to also delete from depts / training groups that are also in that company.

                        'get the parent dir object
                        sSql = "select d.nDirKey, d.cDirSchema, dr.nDirChildId from tblDirectoryRelation dr inner join tblDirectory d on d.nDirKey = dr.nDirParentId where nRelKey=" & nId
                        Dim nParentId As Long
                        Dim nChildId As Long
                        Dim cDirSchema As String = ""
                        oDr = getDataReader(sSql)
                        While oDr.Read
                            nParentId = oDr("nDirKey")
                            nChildId = oDr("nDirChildId")
                            cDirSchema = oDr("cDirSchema")
                        End While
                        oDr.Close()
                        oDr = Nothing
                        If cDirSchema = "Company" Then
                            'select all of the children of nParentId that have relations with our child
                            sSql = "select cr.nRelKey from tblDirectoryRelation dr " & _
                            "inner join tblDirectory pd on pd.nDirKey = dr.nDirParentId  " & _
                            "inner join tblDirectory cd on cd.nDirKey = dr.nDirChildId  " & _
                            "inner join tblDirectoryRelation cr on cd.nDirKey = cr.nDirParentId  " & _
                            "where dr.nDirParentId = " & nParentId & " " & _
                            "and cr.nDirChildId = " & nChildId
                            oDr = getDataReader(sSql)
                            'delete links between the target object and the children of the parent object.
                            While oDr.Read
                                DeleteObject(objectTypes.DirectoryRelation, oDr(0))
                            End While
                            oDr.Close()
                            oDr = Nothing
                        End If
                    Case objectTypes.CartItem
                        sSql = "Select  nAuditID from tblCartItem WHERE nCartItemKey = " & nId
                        oDr = getDataReader(sSql)
                        Dim nCrtItmAdtId As Integer
                        While oDr.Read
                            nCrtItmAdtId = oDr.GetValue(0)
                            'DeleteObject(objectTypes.Audit, oDr.GetValue(0))
                        End While
                        oDr = Nothing
                        'options
                        sSql = "Select nCartItemKey from tblCartItem WHERE nParentID = " & nId
                        oDr = getDataReader(sSql)
                        While oDr.Read
                            DeleteObject(objectTypes.CartItem, oDr.GetValue(0))
                        End While
                        oDr = Nothing

                        ExeProcessSql("Delete from tblCartItem where nCartItemKey = " & nId)
                        DeleteObject(objectTypes.Audit, nCrtItmAdtId)
                    Case objectTypes.CartOrder
                        'cart items
                        sSql = "Select nCartItemKey from tblCartItem WHERE nCartOrderID = " & nId
                        oDr = getDataReader(sSql)
                        While oDr.Read
                            DeleteObject(objectTypes.CartItem, oDr.GetValue(0))
                        End While
                        oDr = Nothing
                        'contacts
                        sSql = "Select nContactKey from tblCartContact WHERE nContactCartID = " & nId
                        oDr = getDataReader(sSql)
                        While oDr.Read
                            DeleteObject(objectTypes.CartContact, oDr.GetValue(0))
                        End While

                        oDr = Nothing
                        sSql = "Select nAuditId from tblCartOrder WHERE nCartOrderKey = " & nId
                        oDr = getDataReader(sSql)
                        Dim nCrtOrdAdtId As Integer
                        While oDr.Read
                            nCrtOrdAdtId = oDr.GetValue(0)
                            'DeleteObject(objectTypes.Audit, oDr.GetValue(0))
                        End While

                        ExeProcessSql("Delete from tblCartOrder where nCartOrderKey = " & nId)
                        DeleteObject(objectTypes.Audit, nCrtOrdAdtId)
                        oDr = Nothing
                    Case objectTypes.CartContact
                        sSql = "Select nAuditId from tblCartContact WHERE nContactKey = " & nId
                        oDr = getDataReader(sSql)
                        While oDr.Read
                            If Not IsDBNull(oDr.GetValue(0)) Then
                                DeleteObject(objectTypes.Audit, oDr.GetValue(0))
                            End If
                        End While
                        ExeProcessSql("Delete from tblCartContact where nContactKey = " & nId)
                        oDr = Nothing
                    Case objectTypes.Subscription
                        Dim oXML As New XmlDocument
                        sSql = "SELECT cSubscriptionXML, nUserId FROM tblSubscriptions WHERE nSubscriptionKey = " & nId
                        Dim nSubUserId As Integer
                        oDr = getDataReader(sSql)
                        While oDr.Read
                            oXML.InnerXml = Replace(Replace(oDr(0), "&gt;", ">"), "&lt;", "<")
                            nSubUserId = oDr(1)
                        End While

                        Dim oElmt As XmlElement = oXML.DocumentElement.SelectSingleNode("descendant-or-self::UserGroups")
                        Dim oGrpElmt As XmlElement

                        sSql = "SELECT cSubXML FROM tblSubscriptions WHERE nDirId = " & nSubUserId & " AND (NOT (nSubKey = " & nId & "))"
                        Dim oDS As DataSet = myWeb.moDbHelper.GetDataSet(sSql, "Content")
                        Dim oXML2 As New XmlDocument
                        oXML2.InnerXml = Replace(Replace(oDS.GetXml, "&gt;", ">"), "&lt;", "<")

                        For Each oGrpElmt In oElmt.SelectNodes("Group[@id!='']")
                            Dim bDelete As Boolean = True
                            Dim oTmpElmt As XmlElement
                            For Each oTmpElmt In oXML2.DocumentElement.SelectNodes("Content/cSubscriptionXML/Content/UserGroups/Group[@id=" & oGrpElmt.GetAttribute("id") & "]")
                                bDelete = False
                                Exit For
                            Next
                            If bDelete Then
                                sSql = "SELECT nRelKey FROM tblDirectoryRelation WHERE (nDirChildId = " & nSubUserId & ") AND (nDirParentId = " & oGrpElmt.GetAttribute("id") & ")"
                                oDr = getDataReader(sSql)
                                While oDr.Read
                                    DeleteObject(objectTypes.DirectoryRelation, oDr.GetValue(0))
                                End While
                            End If
                        Next

                        sSql = "Select nAuditId From tblSubscriptions where nSubKey = " & nId
                        oDr = getDataReader(sSql)
                        While oDr.Read
                            DeleteObject(objectTypes.Audit, oDr.GetValue(0))
                        End While
                        ExeProcessSql("Delete from tblSubscriptions where nSubKey = " & nId)
                        oDr.Close()
                        oDr = Nothing
                    Case objectTypes.CartShippingMethod
                        sSql = "SELECT nShpRelKey FROM tblCartShippingRelations WHERE nShpOptId = " & nId
                        oDr = getDataReader(sSql)
                        While oDr.Read
                            DeleteObject(objectTypes.CartShippingRelations, oDr.GetValue(0))
                        End While
                        ExeProcessSql("DELETE FROM tblCartShippingMethods WHERE nShipOptKey = " & nId)
                        oDr.Close()
                        oDr = Nothing
                End Select
                If bReporting Then
                    goResponse.Write("<p>Deleted from " & getTable(objectType) & " - " & nId & "</p>")
                End If

                If Not bHaltDelete Then

                    If Not (objectType = objectTypes.QuestionaireResultDetail) And Not (objectType = objectTypes.ActivityLog) And Not (objectType = objectTypes.Audit) Then
                        'delete the audit ref
                        sSql = "select nAuditId from " & getTable(objectType) & " where " & getKey(objectType) & " = " & nId
                        oDr = getDataReader(sSql)
                        While oDr.Read
                            If IsNumeric(oDr(0)) Then
                                nAuditId = oDr(0)
                            End If

                        End While
                        oDr.Close()
                        oDr = Nothing
                        If nAuditId > 0 Then
                            sSql = "delete from tblAudit where nAuditKey = " & nAuditId
                            ExeProcessSql(sSql)
                        End If

                    End If

                        'delete the main item
                        sSql = "delete from " & getTable(objectType) & " where " & getKey(objectType) & " = " & nId
                    ExeProcessSql(sSql)

                End If

                Return nId

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "DeleteObject", ex, cProcessInfo))
            End Try

        End Function

        ''' <summary>
        ''' Delete content and associated relationships by ID.
        ''' Bulk method to try and emulate DeleteObject without the bit by bit iteration
        ''' The items that will be deleted are:
        ''' the content, any locations for this content, any relations for this content, the related audit records for all three
        ''' </summary>
        ''' <param name="contentIDList">A List(Of String) of the IDs to delete</param>
        ''' <remarks></remarks>
        Protected Friend Sub BulkContentDelete(ByVal contentIDList As List(Of String))
            Dim methodName As String = "BulkContentDelete(List(Of String))"
            Dim processInfo As String = String.Empty
            Try
                BulkContentDelete(String.Join(",", contentIDList.ToArray()))
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, methodName, ex, processInfo))
            End Try
        End Sub

        ''' <summary>
        ''' Delete content and associated relationships by ID.
        ''' Bulk method to try and emulate DeleteObject without the bit by bit iteration
        ''' The items that will be deleted are:
        ''' the content, any locations for this content, any relations for this content, the related audit records for all three
        ''' </summary>
        ''' <param name="contentIDCSVList">The content IDs to delete as a comma separated list</param>
        ''' <remarks>I'm not decided on deleting orphan related content.</remarks>
        Protected Friend Sub BulkContentDelete(ByVal contentIDCSVList As String)

            Dim methodName As String = "BulkContentDelete(String)"
            Dim processInfo As String = String.Empty
            Dim deletedRecords As Integer = 0
            PerfMon.Log(mcModuleName, methodName)

            Try

                ' Validate the list as a list of numbers
                If Regex.IsMatch(contentIDCSVList, "^\d+(,\d+)*$") Then

                    ' Delete the locations for this content
                    deletedRecords = Me.ExeProcessSql("DELETE tblAudit FROM tblAudit INNER JOIN tblContentLocation ON nAuditId = nAuditKey AND nContentId IN (" & contentIDCSVList & ")")
                    If deletedRecords > 0 Then
                        Me.ExeProcessSql("DELETE FROM tblContentLocation WHERE nContentId IN (" & contentIDCSVList & ")")
                    End If

                    ' Delete the relatnios for this content
                    deletedRecords = Me.ExeProcessSql("DELETE tblAudit FROM tblAudit INNER JOIN tblContentRelation ON nAuditId = nAuditKey AND (nContentChildId IN (" & contentIDCSVList & ") OR nContentParentId IN (" & contentIDCSVList & "))")
                    If deletedRecords > 0 Then
                        Me.ExeProcessSql("DELETE  FROM tblContentRelation WHERE nContentChildId IN (" & contentIDCSVList & ") OR nContentParentId IN (" & contentIDCSVList & ")")
                    End If

                    ' Delete the actual content
                    deletedRecords = Me.ExeProcessSql("DELETE tblAudit FROM tblAudit INNER JOIN tblContent ON nAuditId = nAuditKey AND nContentKey IN (" & contentIDCSVList & ")")
                    If deletedRecords > 0 Then
                        Me.ExeProcessSql("DELETE  FROM tblContent WHERE nContentKey IN (" & contentIDCSVList & ")")
                    End If

                End If

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, methodName, ex, processInfo))
            End Try

        End Sub


        Public Sub DeleteAllObjects(ByVal objectType As objectTypes, Optional ByVal bReporting As Boolean = False)
            PerfMon.Log("DBHelper", "DeleteAllObjects")
            Dim sSql As String
            Dim oDr As SqlDataReader

            Dim cProcessInfo As String = ""
            Try


                Select Case objectType

                    Case 3 ' tblContentStructure
                        DeleteAllObjects(objectTypes.ContentLocation)
                End Select

                sSql = "select " & getKey(objectType) & " from " & getTable(objectType)

                oDr = getDataReader(sSql)
                If bReporting Then
                    goResponse.Write("Deleting: - " & getTable(objectType))
                End If

                While oDr.Read
                    DeleteObject(objectType, oDr(0), bReporting)
                End While

                oDr.Close()
                oDr = Nothing

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "DeleteAllObjects", ex, cProcessInfo))
            End Try

        End Sub

        Public Overridable Function getObjectInstance(ByVal ObjectType As objectTypes, Optional ByVal nId As Long = -1, Optional ByVal sWhere As String = "") As String
            PerfMon.Log("DBHelper", "getObjectInstance")
            Dim sSql As String
            Dim oDs As DataSet
            Dim oNode As XmlNode
            Dim oElmt As XmlElement
            '  Dim  As New 
            Dim sContent As String
            Dim sInstance As String = ""
            Dim oRow As DataRow
            Dim oColumn As DataColumn
            Dim sNewVal As String = ""

            Dim cProcessInfo As String = ""
            Try
                'Dim oXml As XmlDocument = New XmlDocument


                If sWhere = "" Then
                    sSql = "select * from " & getTable(ObjectType) & " left outer join tblAudit a on nAuditId = a.nAuditKey where " & getKey(ObjectType) & " = " & nId
                Else
                    sSql = "select TOP 1 * from " & getTable(ObjectType) & " inner join tblAudit a on nAuditId = a.nAuditKey " & sWhere
                End If
                If ObjectType = objectTypes.ScheduledItem Then
                    sSql = "select * from " & getTable(ObjectType) & " where " & getKey(ObjectType) & " = " & nId
                End If


                cProcessInfo = "running: " & sSql

                oDs = GetDataSet(sSql, getTable(ObjectType), "instance")
                ReturnNullsEmpty(oDs)

                Dim oXml As XmlDataDocument = New XmlDataDocument(oDs)
                oDs.EnforceConstraints = False

                'Convert any text to xml
                For Each oNode In oXml.SelectNodes("/instance/" & getTable(ObjectType) & "/*")

                    'ignore for passwords
                    If Not oNode.Name = "cDirPassword" Then
                        oElmt = oNode
                        sContent = oElmt.InnerText
                        Try
                            oElmt.InnerXml = sContent
                        Catch
                            'run tidy...
                            oElmt.InnerXml = tidyXhtmlFrag(sContent, True, False)
                        End Try
                        'empty empty dates
                        If oElmt.InnerXml = "0001-01-01T00:00:00+00:00" Then oElmt.InnerXml = ""
                    End If

                    If ObjectType = objectTypes.Directory Then
                        'fix for status of -1
                        If oNode.Name = "nStatus" Then
                            oElmt = oNode
                            sContent = oElmt.InnerText
                            If sContent = "-1" Then oElmt.InnerText = "1"

                        End If
                    End If

                Next
                If Not oXml.SelectSingleNode("instance") Is Nothing Then
                    sInstance = oXml.SelectSingleNode("instance").InnerXml
                Else
                    'we could be clever here and step through the dataset to build an empty instance?
                    oRow = oDs.Tables(getTable(ObjectType)).NewRow
                    oElmt = oXml.CreateElement(getTable(ObjectType))
                    For Each oColumn In oDs.Tables(getTable(ObjectType)).Columns
                        'set a default value for status.
                        If oColumn.ToString = "nStatus" Then
                            Select Case ObjectType
                                Case objectTypes.CartShippingMethod
                                    sNewVal = "1" ' active
                                Case Else
                                    sNewVal = "0" ' in-active
                            End Select

                        Else
                            If Not IsDBNull(oColumn.DefaultValue) Then
                                sNewVal = convertDtSQLtoXML(oColumn.DataType, oColumn.DefaultValue)
                            Else
                                sNewVal = ""
                            End If

                        End If
                        addNewTextNode(oColumn.ToString, oElmt, sNewVal, True, False) 'always force this
                    Next
                    oXml.AppendChild(oElmt)
                    sInstance = oXml.InnerXml
                End If

                oXml = Nothing
                ' oDs.Clear()
                oDs = Nothing

                Return sInstance

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getObjectInstance", ex, cProcessInfo))
                Return ""
            End Try

        End Function

        Public Function IsAuditedObjectLive(ByVal objectType As objectTypes, ByVal objectKey As Long) As Boolean
            Try
                Dim query As String = "SELECT " & getKey(objectType) & " FROM " & getTable(objectType) & " o INNER JOIN dbo.tblAudit a ON o.nauditId = a.nauditKey " _
                                    & "WHERE " & myWeb.GetStandardFilterSQLForContent(False)
                Return GetDataValue(query, , , 0) > 0
            Catch ex As Exception
                Return False
            End Try
        End Function

        Public Function isCascade(ByVal ContentId As Long) As Boolean
            PerfMon.Log("DBHelper", "isCascade")
            Dim nCount As Long
            Dim sSql As String
            Dim cProcessInfo As String = ""
            Try
                sSql = "select count(*) from tblContentLocation where nContentId=" & ContentId & " and bCascade = 1"

                nCount = GetDataValue(sSql)

                If nCount > 0 Then
                    Return True
                Else
                    Return False
                End If

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "isCascade", ex, cProcessInfo))
            End Try
        End Function

        Public Overridable Function setObjectInstance(ByVal ObjectType As objectTypes, Optional ByVal oInstance As XmlElement = Nothing, Optional ByVal nKey As Long = -1) As String

            PerfMon.Log("DBHelper", "setObjectInstance", ObjectType.ToString)

            'EonicWeb Specific Function for updating DB Entities, manages creation and updating of Audit table

            Dim oXml As XmlDataDocument
            Dim oElmt As XmlElement = Nothing
            Dim oElmt2 As XmlElement
            Dim nAuditId As Long
            Dim oTableNode As XmlElement

            Dim cProcessInfo As String = ""
            Try

                'if we have not been given an instance we'll create one, this makes it easy to update the audit table by just supplying an id
                If oInstance Is Nothing Then
                    oXml = New XmlDataDocument
                    oElmt = oXml.CreateElement("instance")
                    oXml.AppendChild(oElmt)
                    oTableNode = oXml.CreateElement(getTable(ObjectType))
                    oXml.FirstChild.AppendChild(oTableNode)
                    oElmt2 = oXml.CreateElement(getKey(ObjectType))
                    oElmt2.InnerText = nKey
                    oElmt.AppendChild(oElmt2)
                    oInstance = oXml.DocumentElement
                Else
                    oTableNode = oInstance.SelectSingleNode(getTable(ObjectType))
                End If
                If getTable(ObjectType) = "tblAudit" Then
                    'we get the first actual element.
                    For Each oElmt In oInstance.SelectNodes("*")
                        If oTableNode Is Nothing Then
                            If oElmt.NodeType = XmlNodeType.Element Then
                                oTableNode = oElmt
                            End If
                        End If
                    Next
                End If

                'lets get the key from the instance if not supplied
                If nKey = -1 Then
                    oElmt = oInstance.SelectSingleNode("*/" & getKey(ObjectType))
                    If Not oElmt Is Nothing Then
                        If IsNumeric(oElmt.InnerText) Then
                            nKey = CLng(oElmt.InnerText)
                        End If
                    End If
                End If

                'also lets add the key to the instance because were going to need it for saveInstanc
                If nKey > 0 Then
                    addNewTextNode(getKey(ObjectType), oInstance.SelectSingleNode("*"), nKey, True, True)
                End If

restart:
                ' Special case: contentversion (when setting audit)
                If oTableNode Is Nothing And ObjectType = objectTypes.ContentVersion Then
                    oTableNode = oInstance.SelectSingleNode(getTable(objectTypes.Content))
                End If



                ' Process for Updates or Inserts
                If nKey > 0 Then ' CASE FOR UPDATE ------- not a new record so lets update the audit.
                    'and now we update the audit table
                    cProcessInfo = "Updating Object Type: " & ObjectType & "(Table: " & getTable(ObjectType) & ", Object Id: " & nKey
                    Select Case ObjectType
                        Case objectTypes.Content

                            If gbVersionControl Then
                                'out to a subroutine for versioning
                                contentVersioning(oInstance, ObjectType, nKey)
                                If ObjectType = objectTypes.ContentVersion Then GoTo restart
                                nAuditId = tidyAuditId(oInstance, ObjectType, nKey)
                            Else
                                nAuditId = tidyAuditId(oInstance, ObjectType, nKey)
                            End If

                        Case objectTypes.ContentVersion, objectTypes.ContentStructure, objectTypes.Directory,
                        objectTypes.CourseResult, objectTypes.CartOrder, objectTypes.CartItem,
                        objectTypes.CartContact, objectTypes.CartShippingLocation,
                        objectTypes.CartShippingMethod, objectTypes.CartCatProductRelations,
                        objectTypes.CartDiscountDirRelations, objectTypes.CartDiscountProdCatRelations,
                        objectTypes.CartDiscountRules, objectTypes.CartProductCategories,
                        objectTypes.Codes, objectTypes.QuestionaireResult, objectTypes.CourseResult,
                        objectTypes.Certificate, objectTypes.CpdLog, objectTypes.QuestionaireResultDetail, objectTypes.Lookup, objectTypes.CartCarrier, objectTypes.CartDelivery
                            '
                            ' Check for Audit Id - if not found, we should be able to retrieve one from the database.
                            nAuditId = tidyAuditId(oInstance, ObjectType, nKey)

                        Case objectTypes.Audit
                            ' Check for insert fields - if they exist, then we need to get rid of them
                            '     removeChildByName(oTableNode, "dInsertDate")
                            '     removeChildByName(oTableNode, "nInsertDirId")

                            'TS not sure about this it seems the logic to set this is further on.

                            ' Update the update fields.
                            addNewTextNode("dUpdateDate", oTableNode, Eonic.Tools.Xml.XmlDate(Now(), True), True, True) 'always force this
                            addNewTextNode("nUpdateDirId", oTableNode, mnUserId, True, True) 'always force this
                        Case Else
                            ' do nowt
                    End Select

                    'Check the auditId is populated in the instnace
                    Dim auditIdElmt As XmlElement = oInstance.SelectSingleNode(getTable(ObjectType) & "/nAuditId")
                    If Not auditIdElmt Is Nothing Then
                        If auditIdElmt.InnerText = "" Then
                            auditIdElmt.InnerText = nAuditId
                        End If
                    End If

                Else  ' CASE FOR INSERT  ------------  We have a new record lets give it an auditId if it needs one.
                    cProcessInfo = "Inserting Object Type: " & ObjectType & "(Table: " & getTable(ObjectType) & ", New Object"
                    Select Case ObjectType

                        Case objectTypes.Content
                            'To be readjusted
                            If gbVersionControl Then
                                'out to a subroutine for versioning
                                contentVersioning(oInstance, ObjectType)
                                If ObjectType = objectTypes.ContentVersion Then GoTo restart
                            End If

                            'we are using getAuditId to create a new audit record.
                            nAuditId = setObjectInstance(objectTypes.Audit, oInstance)
                            addNewTextNode("nAuditId", oTableNode, nAuditId, True, True)

                        Case objectTypes.Content, objectTypes.ContentStructure,
                            objectTypes.Directory, objectTypes.CartOrder, objectTypes.CartItem, objectTypes.CartContact,
                            objectTypes.CartShippingLocation, objectTypes.CartShippingMethod, objectTypes.CartCatProductRelations,
                            objectTypes.CartDiscountDirRelations, objectTypes.CartDiscountProdCatRelations,
                            objectTypes.CartDiscountRules, objectTypes.CartProductCategories,
                            objectTypes.QuestionaireResult, objectTypes.QuestionaireResultDetail, objectTypes.CourseResult,
                            objectTypes.Codes, objectTypes.ContentVersion,
                             objectTypes.Certificate, objectTypes.CpdLog, objectTypes.Lookup, objectTypes.CartCarrier, objectTypes.CartDelivery

                            'we are using getAuditId to create a new audit record.
                            nAuditId = setObjectInstance(objectTypes.Audit, oInstance)
                            addNewTextNode("nAuditId", oTableNode, nAuditId, True, True)


                        Case objectTypes.Audit
                            'add logic for inserting default audit fields.
                            ' addNewTextNode("dPublishDate", oInstance.FirstChild, xmlDate(Now()), True, False)

                            'if this is supplied we want to keep it, mostly it will be blank
                            If oInstance.FirstChild.SelectSingleNode("dInsertDate") Is Nothing Then
                                addNewTextNode("dInsertDate", oTableNode, Eonic.Tools.Xml.XmlDate(Now(), True), True, True) 'always force this
                            ElseIf oInstance.FirstChild.SelectSingleNode("dInsertDate").InnerText = "" Then
                                oInstance.FirstChild.SelectSingleNode("dInsertDate").InnerText = Eonic.Tools.Xml.XmlDate(Now(), True)
                                'addNewTextNode("dInsertDate", oTableNode, Eonic.Tools.Xml.XmlDate(Now(), True), True, True) 'always force this
                            End If

                            If oInstance.FirstChild.SelectSingleNode("nInsertDirId") Is Nothing Then
                                addNewTextNode("nInsertDirId", oTableNode, mnUserId, True, True) 'always force this
                            ElseIf oInstance.FirstChild.SelectSingleNode("nInsertDirId").InnerText = "" Then
                                oInstance.FirstChild.SelectSingleNode("nInsertDirId").InnerText = mnUserId
                                'addNewTextNode("nInsertDirId", oTableNode, mnUserId, True, True) 'always force this
                            End If

                            addNewTextNode("dUpdateDate", oTableNode, Eonic.Tools.Xml.XmlDate(Now(), True), True, True) 'always force this
                            addNewTextNode("nUpdateDirId", oTableNode, mnUserId, True, True) 'always force this
                            If oInstance.SelectSingleNode("descendant-or-self::nStatus") Is Nothing Then
                                addNewTextNode("nStatus", oTableNode, "1", True, True)
                            Else
                                If oInstance.SelectSingleNode("descendant-or-self::nStatus").InnerText = "" Then
                                    addNewTextNode("nStatus", oTableNode, "1", True, True)
                                End If
                                addNewTextNode("nStatus", oTableNode, "1", True, False)
                            End If
                    End Select
                End If

                cProcessInfo = "Saving instance"
                PerfMon.Log("DBHelper", "setObjectInstance", "startsave")
                nKey = saveInstance(oInstance, getTable(ObjectType), getKey(ObjectType))
                PerfMon.Log("DBHelper", "setObjectInstance", "endsave")
                If ObjectType = objectTypes.ContentStructure Then
                    clearStructureCacheAll()
                End If

                Return nKey

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "setObjectInstance", ex, cProcessInfo))
                Return ""
            End Try

        End Function

        Protected Function tidyAuditId(ByRef oInstance As XmlElement, ByVal objectType As objectTypes, ByVal nKey As Long) As Long
            Dim cProcessInfo As String = ""
            Dim nAuditId As Long
            Dim oElmt As XmlElement = Nothing
            Try

                ' Check for Audit Id - if not found, we should be able to retrieve one from the database.
                nAuditId = 0

                ' First check the node (exists and is numeric)
                If Tools.Xml.NodeState(oInstance, "descendant-or-self::nAuditId", , , , oElmt) = Tools.Xml.XmlNodeState.HasContents Then
                    If IsNumeric(oElmt.InnerText()) Then
                        nAuditId = CLng(oElmt.InnerText())
                    End If
                End If

                ' If not set then try getting the value from the DB
                If Not (nAuditId > 0) Then
                    nAuditId = Me.GetDataValue("SELECT nAuditId FROM " & getTable(objectType) & " WHERE " & getKey(objectType) & "=" & nKey, , , 0)
                End If
                Dim oTableNode As XmlElement = oInstance.FirstChild

                'if this is supplied we want to keep it, mostly it will be blank
                If oTableNode.SelectSingleNode("dInsertDate") Is Nothing Then
                    addNewTextNode("dInsertDate", oTableNode, Eonic.Tools.Xml.XmlDate(Now(), True), True, True) 'always force this
                ElseIf oInstance.FirstChild.SelectSingleNode("dInsertDate").InnerText = "" Then
                    addNewTextNode("dInsertDate", oTableNode, Eonic.Tools.Xml.XmlDate(Now(), True), True, True) 'always force this
                End If

                If oTableNode.SelectSingleNode("nInsertDirId") Is Nothing Then
                    addNewTextNode("nInsertDirId", oTableNode, mnUserId, True, True) 'always force this
                ElseIf oInstance.FirstChild.SelectSingleNode("nInsertDirId").InnerText = "" Then

                    addNewTextNode("nInsertDirId", oTableNode, mnUserId, True, True) 'always force this
                End If

                ' Set the Audit instance
                If nAuditId > 0 Then setObjectInstance(objectTypes.Audit, oInstance, nAuditId)

                Return nAuditId

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "tidyAuditId", ex, cProcessInfo))
                Return ""
            End Try

        End Function

#Region "DB Methods: Version Control"
        Protected Sub contentVersioning(ByRef oInstance As XmlElement, ByRef ObjectType As objectTypes, Optional ByRef nKey As Long = 0)

            Dim cProcessInfo As String = ""
            Dim oElmt As XmlElement = Nothing
            Dim oOrigInstance As XmlElement
            Dim nCurrentVersionNumber As Long
            Dim nNewVersionNumber As Long = 0
            Dim nStatus As Status
            Dim cSql As String = ""

            Try

                'first we look at the status of the content being sumitted.

                nStatus = getNodeValueByType(oInstance, "//nStatus", dataType.TypeNumber, 1)
                nCurrentVersionNumber = getNodeValueByType(oInstance, "//nVersion", dataType.TypeNumber, 0)

                ' Get the maximum version number
                If nKey > 0 Then
                    cSql = "SELECT MAX(nVersion) As MaxV " _
                            & "FROM ( " _
                            & "       SELECT nVersion FROM dbo.tblContent WHERE nContentKey = " & nKey _
                            & "       UNION " _
                            & "       SELECT nVersion FROM dbo.tblContentVersions WHERE nContentPrimaryId = " & nKey _
                            & ") VersionList"
                    nNewVersionNumber = GetDataValue(cSql, , , 0) + 1
                End If

                oInstance.SelectSingleNode("//nVersion").InnerText = nNewVersionNumber.ToString

                ' If the status is live or hidden, then check the users page permissions
                ' At the moment assume that if we are in admin mode, we can skip this
                If Not myWeb Is Nothing Then

                    If Not (myWeb.mbAdminMode) Then
                        ' Check the permissions
                        ' Problem at the moment ascertaining the parent page for orphan content, 
                        ' This is already acheived in Web.GetAjaxXml so we pass through a value to dbHelper

                        If Not CanPublish(CurrentPermissionLevel) Then

                            ' The permission level inspected is not a publishing level, so this needs to be set as pending.
                            nStatus = Status.Pending

                            ' Check if the status node exists
                            If NodeState(oInstance, "//nStatus") = Tools.Xml.XmlNodeState.NotInstantiated Then
                                ' Status node doesn't exist - we have to assume that it's going to be added to the first child of oInstance (i.e. instance/tablename/nstatus)
                                Dim oTable As XmlElement = oInstance.FirstChild
                                oTable.AppendChild(oTable.OwnerDocument.CreateElement("nStatus"))
                            End If

                            oInstance.SelectSingleNode("//nStatus").InnerText = nStatus

                        End If
                    End If

                    Select Case nStatus
                    Case Status.Live, Status.Hidden

                        ' If this is not new then get the current version and commit it to the version table
                        If nKey > 0 Then
                            'create a copy of the origional in versions and save live
                            oOrigInstance = moPageXml.CreateElement("instance")
                            oOrigInstance.InnerXml = getObjectInstance(objectTypes.Content, nKey)

                            setNewContentVersionInstance(oOrigInstance, nKey)

                            ' If this was a pending item, supercede the copy in the version table (ie superceded anything that's pending)
                            Dim cPreviousStatus As String = ""
                            If NodeState(oInstance, "currentStatus", , , , , , cPreviousStatus) = XmlNodeState.HasContents Then
                                If cPreviousStatus = "3" Then
                                    ' Update everything with a status of Pending to be DraftSuperceded
                                    ExeProcessSql("UPDATE tblAudit SET nStatus = " & Status.Superceded & " FROM tblAudit a INNER JOIN tblContentVersions c ON c.nAuditId = a.nAuditKey AND c.nContentPrimaryId = " & nKey & " AND a.nStatus = " & Status.Pending)
                                End If
                            End If

                        End If


                    Case Status.Pending

                        ' Pending works in a number of ways
                        ' - New content is created in the content table, not the versions table
                        ' - Existing content is created in the versions table
                        '   - For existing content, if the current Content (nKey) is Live or Hidden then save Pending instance to versions
                        '   - If the current Content (nKey) is Pending or Rejected then move current to Versions and save Pending instance to Content.

                        ' First get the current live, if applicable.
                        If nKey > 0 Then

                            Dim cParentId As Long = nKey
                            ' Get the current live
                            oOrigInstance = moPageXml.CreateElement("instance")
                            oOrigInstance.InnerXml = getObjectInstance(objectTypes.Content, nKey)


                            ' Assess the status
                            Dim nLiveStatus As Status = getNodeValueByType(oOrigInstance, "//nStatus", dataType.TypeNumber)
                            Select Case nLiveStatus
                                Case Status.Live, Status.Hidden
                                    ' Leave the live content alone, set the pending content as a version
                                    ObjectType = objectTypes.ContentVersion
                                    prepareContentVersionInstance(oInstance, nKey, nStatus)
                                    nKey = 0

                                Case Else

                                    ' The LIVE content is pending, which means that this should be moved into the version 
                                    ' and the submitted content goes into the content table.
                                    setNewContentVersionInstance(oOrigInstance, nKey)

                            End Select

                            ' Update everything with a status of Pending to be DraftSuperceded
                            ExeProcessSql("UPDATE tblAudit SET nStatus = " & Status.DraftSuperceded & " FROM tblAudit a INNER JOIN tblContentVersions c ON c.nAuditId = a.nAuditKey AND c.nContentPrimaryId = " & cParentId & " AND a.nStatus = " & Status.Pending)

                        Else

                            ' This is a new item of content - do nothing, let it be added as content, with a status of Pending.


                        End If


                    Case Status.Rejected, Status.Superceded
                        'remove key
                        ObjectType = objectTypes.ContentVersion
                        'GoTo restart
                End Select

                End If
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "contentVersioning", ex, cProcessInfo))
            End Try
        End Sub

        Private Sub prepareContentVersionInstance(ByRef oInstance As XmlElement, ByVal nContentPrimaryId As Long, Optional ByVal nStatus As Status = Status.Superceded)
            PerfMon.Log("DBHelper", "prepareContentVersionInstance")
            Dim cProcessInfo As String = "ContentParId = " & nContentPrimaryId

            Try
                ' Set the reference to the parent content
                oInstance.SelectSingleNode("descendant-or-self::nContentPrimaryId").InnerText = nContentPrimaryId

                ' Empty the audit references to create a new one
                oInstance.SelectSingleNode("descendant-or-self::nAuditId").InnerText = ""

                If Not oInstance.SelectSingleNode("descendant-or-self::nAuditKey") Is Nothing Then
                    'don't need this if we don't have the related audit feilds.

                    oInstance.SelectSingleNode("descendant-or-self::nAuditKey").InnerText = ""
                    ' Change the publish date on the version to the last updated date for the content we are archiving
                    oInstance.SelectSingleNode("descendant-or-self::dInsertDate").InnerText = oInstance.SelectSingleNode("descendant-or-self::dUpdateDate").InnerText

                End If

                ' Set the status
                oInstance.SelectSingleNode("descendant-or-self::nStatus").InnerText = nStatus

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "prepareContentVersionInstance", ex, cProcessInfo))
            End Try


        End Sub

        Private Sub setNewContentVersionInstance(ByRef oInstance As XmlElement, ByVal nContentPrimaryId As Long, Optional ByVal nStatus As Status = Status.Superceded)
            PerfMon.Log("DBHelper", "setNewContentVersionInstance")
            Dim cProcessInfo As String = "ContentParId = " & nContentPrimaryId

            Try
                ' Prepare the instance
                prepareContentVersionInstance(oInstance, nContentPrimaryId, nStatus)

                ' Save the intance
                Me.setObjectInstance(objectTypes.ContentVersion, oInstance, 0)

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "setNewContentVersionInstance", ex, cProcessInfo))
            End Try

        End Sub

        Public Function getPendingContent(Optional ByVal bGetContentSinceLastLogged As Boolean = False) As XmlElement
            PerfMon.Log("DBHelper", "getPendingContent")
            Dim cProcessInfo As String = ""
            Dim pendingList As XmlElement = Nothing

            Try

                Dim cSql As String = ""
                Dim sContent As String = ""
                Dim dLastRun As String = ""
                Dim cFilterSql As String = ""

                Dim oDS As New DataSet

                ' Add the filter
                If bGetContentSinceLastLogged Then
                    dLastRun = GetDataValue("SELECT TOP 1 dDateTime FROM dbo.tblActivityLog WHERE nActivityType=" & ActivityType.PendingNotificationSent & " ORDER BY 1 DESC", , , "")
                    If Not (String.IsNullOrEmpty(dLastRun)) AndAlso IsDate(dLastRun) Then cFilterSql = " WHERE Last_Updated > " & SqlDate(dLastRun, True)
                End If

                ' Get the pending content

                oDS = myWeb.moDbHelper.GetDataSet("SELECT * FROM vw_VersionControl_GetPendingContent" & cFilterSql, "Pending", "GenericReport")

                If oDS.Tables.Count > 0 AndAlso oDS.Tables(0).Rows.Count > 0 Then

                    ' Get the Locations content
                    myWeb.moDbHelper.addTableToDataSet(oDS, "SELECT	nContentId AS id, nStructKey AS pageid, cStructName AS page FROM dbo.tblContentLocation l INNER JOIN dbo.tblContentStructure s ON l.nStructId = s.nStructKey WHERE bPrimary=1", "Location")
                    oDS.Relations.Add("PendingLocations", oDS.Tables("Pending").Columns("id"), oDS.Tables("Location").Columns("id"), False)
                    oDS.Relations("PendingLocations").Nested = True

                    ' Get the Related content
                    myWeb.moDbHelper.addTableToDataSet(oDS, "SELECT	nContentParentId AS keyid, nContentKey AS id, cContentName AS name, cContentSchemaName AS type FROM	dbo.tblContentRelation r INNER JOIN dbo.tblContent c ON c.nContentKey = r.nContentChildId", "Content")
                    oDS.Relations.Add("PendingRelations", oDS.Tables("Pending").Columns("id"), oDS.Tables("Content").Columns("keyid"), False)
                    oDS.Relations("PendingRelations").Nested = True

                    ' Map the attributes
                    With oDS.Tables("Pending")
                        .Columns("status").ColumnMapping = Data.MappingType.Attribute
                        .Columns("id").ColumnMapping = Data.MappingType.Attribute
                        .Columns("versionid").ColumnMapping = Data.MappingType.Attribute
                        .Columns("userid").ColumnMapping = Data.MappingType.Attribute
                        .Columns("username").ColumnMapping = Data.MappingType.Attribute
                        .Columns("currentLiveVersion").ColumnMapping = Data.MappingType.Attribute
                    End With

                    With oDS.Tables("Location")
                        .Columns("id").ColumnMapping = MappingType.Hidden
                        .Columns("pageid").ColumnMapping = Data.MappingType.Attribute
                        .Columns("page").ColumnMapping = Data.MappingType.Attribute
                    End With

                    With oDS.Tables("Content")
                        .Columns("keyid").ColumnMapping = MappingType.Hidden
                        .Columns("id").ColumnMapping = Data.MappingType.Attribute
                        .Columns("type").ColumnMapping = Data.MappingType.Attribute
                        .Columns("name").ColumnMapping = Data.MappingType.Attribute
                    End With


                    oDS.EnforceConstraints = False
                    myWeb.moDbHelper.ReturnNullsEmpty(oDS)

                    'convert to Xml Dom
                    Dim oXml As XmlDataDocument = New XmlDataDocument(oDS)
                    oXml.PreserveWhitespace = False

                    pendingList = moPageXml.CreateElement("Content")
                    pendingList.SetAttribute("name", "Content Awaiting Approval")
                    pendingList.SetAttribute("type", "Report")
                    If Not (String.IsNullOrEmpty(dLastRun)) AndAlso IsDate(dLastRun) Then pendingList.SetAttribute("since", xmlDateTime(dLastRun))
                    pendingList.InnerXml = oXml.InnerXml

                    ' Tidy Up - Get rid of all that orphan content from the relations
                    For Each oOrphan As XmlNode In pendingList.FirstChild.SelectNodes("*[name()!='Pending']")
                        pendingList.FirstChild.RemoveChild(oOrphan)
                    Next


                    ' Tidy Up XMl Nodes
                    For Each oElmt As XmlElement In pendingList.SelectNodes("//*[local-name()='UserXml' or local-name()='ContentXml']")

                        Tools.Xml.SetInnerXmlThenInnerText(oElmt, oElmt.InnerText)

                    Next

                    ' Tidy Up - Move all Locations and Relations into a Metadata Node
                    Dim oLocations As XmlElement = Nothing
                    Dim oRelations As XmlElement = Nothing
                    Dim Metadata As XmlElement = Nothing

                    For Each oPending As XmlElement In pendingList.SelectNodes("//Pending")

                        Metadata = oPending.OwnerDocument.CreateElement("Metadata")

                        oLocations = oPending.OwnerDocument.CreateElement("Locations")
                        Metadata.AppendChild(oLocations)
                        For Each oLocation As XmlElement In oPending.SelectNodes("Location")
                            'oLocations.AppendChild(pendingList.ImportNode(oLocation, True))
                            oPending.RemoveChild(oLocation)
                            oLocations.AppendChild(oLocation)
                        Next

                        oRelations = oPending.OwnerDocument.CreateElement("Related")
                        Metadata.AppendChild(oRelations)
                        For Each oRelation As XmlElement In oPending.SelectNodes("Content")
                            'oRelations.AppendChild(pendingList.ImportNode(oRelation, True))
                            oPending.RemoveChild(oRelation)
                            oRelations.AppendChild(oRelation)
                        Next

                        oPending.AppendChild(Metadata)

                    Next


                End If

                ' Log the activity
                If bGetContentSinceLastLogged Then
                    CommitLogToDB(ActivityType.PendingNotificationSent, myWeb.mnUserId, "", Now(), , , , True)
                End If

                Return pendingList
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getPendingContent", ex, cProcessInfo))
                Return Nothing
            End Try

        End Function
#End Region



        Public Function createFakeInstance(ByVal objectType As objectTypes, Optional ByVal oValueElmt As XmlElement = Nothing) As XmlElement
            PerfMon.Log("DBHelper", "createFakeInstance")
            Dim oElmt3 As XmlElement = moPageXml.CreateElement(getTable(objectType))
            If Not oValueElmt Is Nothing Then
                oElmt3.AppendChild(oValueElmt)
            End If
            Dim oElmt4 As XmlElement = moPageXml.CreateElement("instance")
            oElmt4.AppendChild(oElmt3)
            Return oElmt4

        End Function

        Public Overridable Function getGroupsInstance(ByVal nUserId As Long, Optional ByVal nParId As Long = -1) As XmlElement
            PerfMon.Log("DBHelper", "getGroupsInstance")
            Dim cProcessInfo As String = ""
            Try
                'Dim oXml As XmlDocument = New XmlDocument
                Dim sSql As String
                Dim oDs As DataSet
                Dim oNode As XmlNode
                Dim oGrpElmt As XmlElement
                Dim oElmt As XmlElement


                sSql = "execute getUsersCompanyAllParents @UserId=" & nUserId & ", @ParId=" & nParId
                oDs = GetDataSet(sSql, "group", "instance")

                oGrpElmt = moPageXml.CreateElement("groups")

                If oDs.Tables(0).Rows.Count > 0 Then
                    ReturnNullsEmpty(oDs)

                    oDs.Tables(0).Columns(0).ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns(1).ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns(2).ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns(3).ColumnMapping = Data.MappingType.Attribute

                    Dim oXml As XmlDataDocument = New XmlDataDocument(oDs)
                    oDs.EnforceConstraints = False
                    For Each oNode In oXml.DocumentElement.SelectNodes("group")
                        oElmt = oNode
                        If CInt(oElmt.GetAttribute("isMember")) > 0 Then
                            oElmt.SetAttribute("isMember", "true")
                        Else
                            oElmt.SetAttribute("isMember", "false")
                        End If
                    Next
                    oGrpElmt.InnerXml = oXml.SelectSingleNode("instance").InnerXml
                End If

                Return oGrpElmt

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getGroupsInstance", ex, cProcessInfo))
                Return Nothing
            End Try

        End Function

        Public Function getObjectByRef(ByVal objectType As objectTypes, ByVal cForeignRef As String, Optional ByVal cSchemaType As String = "") As Long
            Dim cProcName As String = "getObjectByRef (ObjectTypes,String,[String])"
            PerfMon.Log("DBHelper", cProcName)
            Dim nId As String = 0
            Dim cProcessInfo As String = ""
            Try

                Dim cTableName As String = getTable(objectType)
                Dim cTableKey As String = getKey(objectType)
                Dim cTableFRef As String = getFRef(objectType)

                nId = getObjectByRef(cTableName, cTableKey, cTableFRef, objectType, cForeignRef, cSchemaType)
                Return nId

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, cProcName, ex, cProcessInfo))

            End Try

        End Function

        Public Function getObjectByRef(ByVal cTableName As String, ByVal cTableKey As String, ByVal cTableFRef As String, ByVal objectType As objectTypes, ByVal cForeignRef As String, Optional ByVal cSchemaType As String = "") As Long
            Dim cProcName As String = "getObjectByRef (String,String,String,ObjectTypes,String,[String])"
            PerfMon.Log("DBHelper", cProcName)
            Dim sSql As String = ""
            Dim nId As String = 0
            Dim oDr As SqlDataReader


            ' Some failsafes
            If cTableName = "" Then
                cTableName = getTable(objectType)
            End If
            If cTableKey = "" Then
                cTableKey = getKey(objectType)
            End If
            If cTableFRef = "" Then
                cTableFRef = getFRef(objectType)
            End If



            Dim cProcessInfo As String = ""
            Try
                If cSchemaType = "" Then
                    sSql = "select " & cTableKey & " from " & cTableName & " where " & cTableFRef & " = '" & SqlFmt(cForeignRef) & "'"
                    'sSql = "select " & getKey(objectType) & " from " & getTable(objectType) & " where " & getFRef(objectType) & " = '" & SqlFmt(cForeignRef) & "'"
                Else
                    Select Case objectType
                        Case objectTypes.Content
                            sSql = "select " & cTableKey & " from " & cTableName & " where " & cTableFRef & " = '" & SqlFmt(cForeignRef) & "' and cContentSchemaName='" & cSchemaType & "'"
                            '       sSql = "select " & getKey(objectType) & " from " & getTable(objectType) & " where " & getFRef(objectType) & " = '" & SqlFmt(cForeignRef) & "' and cContentSchemaName='" & cSchemaType & "'"
                        Case objectTypes.Directory
                            sSql = "select " & cTableKey & " from " & cTableName & " where " & cTableFRef & " = '" & SqlFmt(cForeignRef) & "' and cDirSchema='" & cSchemaType & "'"
                            'sSql = "select " & getKey(objectType) & " from " & getTable(objectType) & " where " & getFRef(objectType) & " = '" & SqlFmt(cForeignRef) & "' and cDirSchema='" & cSchemaType & "'"
                    End Select
                End If

                oDr = getDataReader(sSql)
                If oDr Is Nothing Then Return 0
                While oDr.Read
                    nId = oDr(0)
                End While
                oDr.Close()
                oDr = Nothing

                Return nId

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, cProcName, ex, cProcessInfo))

            End Try

        End Function

        Public Function getObjectsByRef(ByVal objectType As objectTypes, ByVal cForiegnRef As String, Optional ByVal cSchemaType As String = "") As String()
            PerfMon.Log("DBHelper", "getObjectByRef")
            Dim sSql As String = ""
            Dim nIds As String = ""
            Dim oDr As SqlDataReader


            Dim cProcessInfo As String = ""
            Try
                If cSchemaType = "" Then
                    sSql = "select " & getKey(objectType) & " from " & getTable(objectType) & " where " & getFRef(objectType) & " = '" & SqlFmt(cForiegnRef) & "'"
                Else
                    Select Case objectType
                        Case objectTypes.Content
                            sSql = "select " & getKey(objectType) & " from " & getTable(objectType) & " where " & getFRef(objectType) & " = '" & SqlFmt(cForiegnRef) & "' and cContentSchemaName='" & cSchemaType & "'"
                        Case objectTypes.Directory
                            sSql = "select " & getKey(objectType) & " from " & getTable(objectType) & " where " & getFRef(objectType) & " = '" & SqlFmt(cForiegnRef) & "' and cDirSchema='" & cSchemaType & "'"
                    End Select
                End If

                oDr = getDataReader(sSql)
                If oDr Is Nothing Then Return Nothing
                While oDr.Read
                    If Not nIds = "" Then nIds &= ","
                    nIds &= oDr(0)
                End While
                oDr.Close()
                oDr = Nothing

                Return Split(nIds, ",")

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getObjectByRef", ex, cProcessInfo))
                Return Nothing
            End Try

        End Function

        Public Function getAuditId(Optional ByVal nStatus As Integer = 1, Optional ByVal nDirId As Long = 0, Optional ByVal cDescription As String = "", Optional ByVal dPublishDate As Object = Nothing, Optional ByVal dExpireDate As Object = Nothing, Optional ByVal dInsertDate As Object = Nothing, Optional ByVal dUpdateDate As Object = Nothing) As Integer
            PerfMon.Log("DBHelper", "getAuditId")
            Dim sSql As String
            Dim nId As Integer
            Dim nUserId As Long

            If nDirId = 0 Then
                nUserId = mnUserId
            Else
                nUserId = nDirId
            End If

            Dim cProcessInfo As String = ""
            Try
                If dInsertDate Is Nothing Then dInsertDate = Now()
                If dUpdateDate Is Nothing Then dUpdateDate = Now()

                'sSql = "insert into tblAudit (dPublishDate, dExpireDate, dInsertDate, nInsertDirId, dUpdateDate, nUpdateDirId, nStatus, cDescription)" & _
                '        " Values (" & _
                '        sqlDate(dPublishDate) & _
                '        ", " & sqlDate(dExpireDate) & _
                '        "," & sqlDateTime(dInsertDate, dInsertDate.Hour.ToString & ":" & dInsertDate.Minute.ToString & ":" & dInsertDate.Second.ToString) & ", " & _
                '        nUserId & "," & sqlDate(dUpdateDate) & ", " & nUserId & "," & nStatus & ", '" & cDescription & "')"
                sSql = "insert into tblAudit (dPublishDate, dExpireDate, dInsertDate, nInsertDirId, dUpdateDate, nUpdateDirId, nStatus, cDescription)" & _
                        " Values (" & _
                        Eonic.Tools.Database.SqlDate(dPublishDate) & _
                        ", " & Eonic.Tools.Database.SqlDate(dExpireDate) & _
                        "," & Eonic.Tools.Database.SqlDate(dInsertDate, True) & ", " & _
                        nUserId & "," & Eonic.Tools.Database.SqlDate(dUpdateDate, True) & ", " & nUserId & "," & nStatus & ", '" & cDescription & "')"
                'Eonic.Tools.Database.SqlDate
                nId = GetIdInsertSql(sSql)

                Return nId
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getAuditId", ex, cProcessInfo))

            End Try
        End Function


        Public Function checkContentLocationsInCurrentMenu(ByVal contentId As Long, Optional ByVal checkRelatedIfOrphan As Boolean = False) As Boolean

            PerfMon.Log("DBHelper", "checkContentLocationsInCurrentMenu")
            Dim processInfo As String = ""

            Try
                processInfo = "contentId=" & contentId

                Dim foundLocation As Boolean = False
                Dim locations As XmlNode = getLocationsByContentId(contentId)
                Dim menu As XmlElement = moPageXml.SelectSingleNode("/Page/Menu")

                If locations IsNot Nothing And menu IsNot Nothing Then


                    ' See if any of the locations for this content exist in the current page menu
                    For Each location As XmlElement In locations.SelectNodes("//Location")

                        If menu.SelectSingleNode("//MenuItem[@id=" & location.GetAttribute("pgid") & "]") IsNot Nothing Then
                            foundLocation = True
                            Exit For
                        End If
                    Next

                    ' If nothing was found and checkRelatedIfOrphan is flagged up, then find related (parent) content and search that as well
                    If Not foundLocation And checkRelatedIfOrphan Then

                        Dim relations As XmlElement = getRelationsByContentId(contentId, , RelationType.Parent)
                        For Each relation As XmlElement In relations.SelectNodes("//Relation")
                            foundLocation = checkContentLocationsInCurrentMenu(relation.GetAttribute("relatedContentId"))
                            If foundLocation Then Exit For
                        Next

                    End If

                End If

                Return foundLocation


            Catch ex As Exception

                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "checkContentLocationsInCurrentMenu", ex, processInfo))
                Return False

            End Try

        End Function


        Public Function getRelationsByContentId(ByVal contentId As Long, Optional ByRef contentNode As XmlElement = Nothing, Optional ByVal contentRelationType As RelationType = RelationType.Child Or RelationType.Parent) As XmlNode
            PerfMon.Log("DBHelper", "getRelationsByContentId")
            Dim sqlQuery As String = ""
            Dim sqlFilter As String = ""
            Dim ds As DataSet
            Dim returnElmt As XmlElement

            Dim processInfo As String = ""
            Try

                ' Set the relationship direction filter
                If (contentRelationType And RelationType.Parent) <> 0 Then sqlFilter &= "nContentParentId = " & contentId
                If (contentRelationType And RelationType.Parent And RelationType.Child) <> 0 Then sqlFilter &= " OR "
                If (contentRelationType And RelationType.Child) <> 0 Then sqlFilter &= "nContentChildId = " & contentId

                ' Creat the full statement - this gets rid of duplicates and indicates how many relationships exist in each example
                sqlQuery = "SELECT relatedContentId, " _
                        & "SUM(CASE WHEN relatedContentRelation='child' THEN 1 ELSE 0 END) AS childRelation, " _
                        & "SUM(CASE WHEN relatedContentRelation='parent' THEN 1 ELSE 0 END) As parentRelation	 " _
                        & "FROM ( " _
                        & "SELECT	CASE WHEN nContentChildId = " & contentId & " THEN nContentParentId ELSE nContentChildId END AS relatedContentId, " _
                        & "CASE WHEN nContentChildId = " & contentId & " THEN 'parent' ELSE 'child' END As relatedContentRelation " _
                        & "FROM tblContentRelation  " _
                        & "WHERE " & sqlFilter & ") r " _
                        & "GROUP BY relatedContentId "

                ds = GetDataSet(sqlQuery, "Relation", "Content")
                ds.Tables(0).Columns("relatedContentId").ColumnMapping = Data.MappingType.Attribute
                ds.Tables(0).Columns("childRelation").ColumnMapping = Data.MappingType.Attribute
                ds.Tables(0).Columns("parentRelation").ColumnMapping = Data.MappingType.Attribute
                ds.EnforceConstraints = False
                Dim dsXml As XmlDataDocument = New XmlDataDocument(ds)
                ds = Nothing


                If contentNode Is Nothing Then
                    returnElmt = moPageXml.CreateElement("Content")
                Else
                    returnElmt = contentNode
                End If

                For Each relation As XmlElement In dsXml.SelectNodes("Content/Relation")
                    returnElmt.AppendChild(moPageXml.ImportNode(relation, True))
                Next

                Return returnElmt

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getRelationsByContentId", ex, processInfo))

                Return Nothing
            End Try

        End Function


        Public Function getLocationsByContentId(ByVal nContentId As Long, Optional ByRef ContentNode As XmlElement = Nothing) As XmlNode
            PerfMon.Log("DBHelper", "getLocationsByContentId")
            Dim sSql As String
            Dim nId As String = 0
            Dim oDs As DataSet
            Dim oElmt As XmlElement

            Dim oLocElmt As XmlElement


            Dim cProcessInfo As String = ""
            Try
                sSql = "select nStructId as [pgid], bPrimary as [primary], cPosition as [position] from tblContentLocation where nContentId = " & nContentId

                oDs = GetDataSet(sSql, "Location", "Content")
                oDs.Tables(0).Columns("pgid").ColumnMapping = Data.MappingType.Attribute
                oDs.Tables(0).Columns("primary").ColumnMapping = Data.MappingType.Attribute
                oDs.Tables(0).Columns("position").ColumnMapping = Data.MappingType.Attribute

                oDs.EnforceConstraints = False
                Dim oXml As XmlDataDocument = New XmlDataDocument(oDs)

                oDs = Nothing
                If ContentNode Is Nothing Then
                    oElmt = moPageXml.CreateElement("Content")
                Else
                    oElmt = ContentNode
                End If
                For Each oLocElmt In oXml.SelectNodes("Content/Location")
                    oElmt.AppendChild(moPageXml.ImportNode(oLocElmt, True))
                Next

                Return oElmt

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getLocationsByContentId", ex, cProcessInfo))

                Return Nothing
            End Try

        End Function

        Public Overridable Sub updatePagePosition(ByVal nPageId As Long, ByVal nContentId As Long, ByVal sPosition As String, Optional ByVal reorder As Boolean = True)
            PerfMon.Log("DBHelper", "updateLocations")
            Dim sSql As String
            Dim cProcessInfo As String = ""
            Try

                sSql = "UPDATE tblContentLocation SET cPosition = '" & SqlFmt(sPosition) & "' WHERE (nStructId = " & nPageId & ") AND (nContentId = " & nContentId & ")"
                ExeProcessSql(sSql)

                ' sSql = "UPDATE tblContent SET cContentName = '" & SqlFmt(sPosition) & "' WHERE (nContentKey = " & nContentId & ")"
                ' ExeProcessSql(sSql)
                If reorder Then
                    ReorderContent(nPageId, nContentId, "MoveTop", , sPosition)
                End If

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "updatePagePosition", ex, cProcessInfo))
            End Try
        End Sub

        Public Overridable Sub updateLocations(ByVal nContentId As Long, ByVal sLocations As String)
            Dim cProcessInfo As String = ""
            Try
                updateLocations(nContentId, sLocations, "")

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "updateLocations", ex, cProcessInfo))
            End Try
        End Sub


        Public Overridable Sub updateLocations(ByVal nContentId As Long, ByVal sLocations As String, ByVal sPosition As String)
            PerfMon.Log("DBHelper", "updateLocations")
            Dim sSql As String
            Dim nLoc() As String
            Dim i As Long
            Dim cProcessInfo As String = ""
            Dim oDs As DataSet
            Dim oRow As DataRow
            Dim nLocations As New Hashtable
            Dim keypair As DictionaryEntry
            Try
                'delete the historic locations
                'sSql = "delete from tblContentLocation where nContentId = " & nContentId & " and bPrimary = 0"
                'ExeProcessSql(sSql)

                'If sLocations <> "" Then
                '    nLoc = Split(sLocations, ",", , CompareMethod.Binary)
                '    For i = 0 To nLoc.Length - 1
                '        Me.setContentLocation(CLng(nLoc(i)), nContentId, False, False)
                '    Next
                'End If

                sSql = "select * from tblContentLocation where nContentId = " & nContentId
                oDs = GetDataSet(sSql, "tblContentLocation")
                If sLocations <> "" Then
                    nLoc = Split(sLocations, ",", , CompareMethod.Binary)
                    For i = 0 To nLoc.Length - 1
                        nLocations.Add(CInt(nLoc(i)), nLoc(i))
                    Next
                End If

                For Each oRow In oDs.Tables(0).Rows
                    If nLocations.Contains(oRow("nStructId")) Then
                        'ignoring existing ones
                        nLocations.Remove(oRow("nStructId"))
                    Else
                        'deleting removed ones
                        If oRow("bPrimary") = False Then
                            Me.DeleteObject(objectTypes.ContentLocation, oRow("nContentLocationKey"))
                        End If
                    End If
                Next

                For Each keypair In nLocations
                    'adding new ones
                    Me.setContentLocation(keypair.Value, nContentId, False, False, False, sPosition, False)
                Next

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "updateLocations", ex, cProcessInfo))
            End Try
        End Sub

        Public Function updateLocationsDetail(ByVal nContentId As Long, ByVal nLocation As Integer, ByVal bPrimary As Boolean) As Boolean
            PerfMon.Log("DBHelper", "updateLocationsDetail")
            Dim cProcessInfo As String = ""
            Try

                Dim cSQL As String
                'first we need to  check wif we are removing the only primary id
                If Not bPrimary Then
                    cSQL = "SELECT nContentLocationKey FROM tblContentLocation WHERE (NOT nStructId = " & nLocation & ") AND (nContentId = " & nContentId & ") AND  (bPrimary = 1)"
                    cProcessInfo = cSQL
                    Dim bResult As Boolean = False
                    Dim oDRE As SqlDataReader = getDataReader(cSQL)
                    If oDRE.HasRows Then bResult = True
                    oDRE.Close()
                    If Not bResult Then Return False
                End If
                cSQL = "UPDATE tblContentLocation SET bPrimary = " & IIf(bPrimary, 1, 0) & " WHERE (nStructId = " & nLocation & ") AND (nContentId = " & nContentId & ")"
                cProcessInfo = cSQL
                ExeProcessSqlScalar(cSQL)
                Return True
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "updateLocationsDetail", ex, cProcessInfo))
                Return False
            End Try
        End Function

        Public Sub updateLocationsWithScope(ByVal nContentId As Long, ByVal sLocations As String, Optional ByVal cInscopeLocations As String = "")
            Dim sSql As String
            Dim nLoc() As String
            Dim i As Long
            Dim cProcessInfo As String = "updateLocationsWithScope"
            Dim bSetPrimary As Boolean
            Try
                '  Delete the current locations for this piece of content.
                '  We need to preserve any existing locations that are still required as they will have ordering information.
                If LCase(myWeb.moConfig("AllowContentLocationsSetPrimary")) = "on" Then
                    bSetPrimary = True
                End If
                ' First, set the general delete statement
                If bSetPrimary Then
                    sSql = "delete from tblContentLocation where nContentId = " & nContentId
                Else
                    sSql = "delete from tblContentLocation where nContentId = " & nContentId & " and bPrimary = 0"

                End If

                ' Only delete locations within a scope (e.g. if only dealing with a partial list of locations)
                If cInscopeLocations <> "" Then
                    sSql += " and nStructId IN (" & cInscopeLocations & ")"
                End If

                ' Preserve any existing and required locations
                If sLocations <> "" Then
                    sSql += " and NOT(nStructId IN (" & sLocations & "))"
                End If

                ' Delete the locations
                Me.ExeProcessSql(sSql)

                ' Update / add the new locations
                If sLocations <> "" Then
                    nLoc = Split(sLocations, ",", , CompareMethod.Binary)
                    For i = 0 To nLoc.Length - 1
                        If i = 0 And bSetPrimary Then
                            ' First location will be the primary
                            Me.setContentLocation(CLng(nLoc(i)), nContentId, True, False)
                        Else
                            Me.setContentLocation(CLng(nLoc(i)), nContentId, False, False)
                        End If
                    Next
                End If
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "updateLocationsWithScope", ex, cProcessInfo))
            End Try
        End Sub

        Public Sub updateShippingLocations(ByVal nOptId As Long, ByVal sLocations As String)
            PerfMon.Log("DBHelper", "updateShippingLocations")
            Dim sSql As String
            Dim nLoc() As String
            Dim i As Long
            Dim oDs As DataSet
            Dim cProcessInfo As String = ""
            Dim oRow As DataRow
            Try
                'delete the historic locations

                sSql = "select nShpRelKey from tblCartShippingRelations where nShpOptId = " & nOptId
                oDs = GetDataSet(sSql, "tblCartShippingRelations")
                For Each oRow In oDs.Tables(0).Rows
                    Me.DeleteObject(objectTypes.CartShippingRelations, oRow("nShpRelKey"))
                Next

                oDs = Nothing


                If sLocations <> "" Then
                    nLoc = Split(sLocations, ",", , CompareMethod.Binary)
                    For i = 0 To nLoc.Length - 1
                        Me.insertShippingLocation(nOptId, CLng(nLoc(i)), False)
                    Next
                End If
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "updateLocations", ex, cProcessInfo))

            End Try
        End Sub

        Public Function getUserQuizInstance(ByVal nQuizId As Long) As XmlNode
            PerfMon.Log("DBHelper", "getUserQuizInstance")
            Dim oDr As SqlDataReader
            Dim oQuizXml As XmlNode = Nothing
            Dim oElmt As XmlElement
            Dim dDateTaken As Object
            Dim nTimeTaken As Long
            Dim cProcessInfo As String = ""
            Try
                oDr = getDataReader("SELECT nContentId, cQResultsXml,nQResultsTimeTaken FROM tblQuestionaireResult r WHERE nQResultsKey=" & CStr(nQuizId))
                If oDr.Read Then
                    oQuizXml = moPageXml.CreateElement("container")
                    oQuizXml.InnerXml = oDr.Item("cQResultsXml")
                    oQuizXml = oQuizXml.SelectSingleNode("instance")
                    'lets add the contentId of the xFormQuiz to the results set.
                    oElmt = oQuizXml.SelectSingleNode("descendant-or-self::results")
                    oElmt.SetAttribute("contentId", oDr.Item("nContentId"))
                    ' Add the time taken
                    oElmt = oQuizXml.SelectSingleNode("//status/timeTaken")
                    If IsDBNull(oDr.Item("nQResultsTimeTaken")) Then nTimeTaken = 0 Else nTimeTaken = oDr.Item("nQResultsTimeTaken")
                    If oElmt Is Nothing Then
                        addNewTextNode("timeTaken", oQuizXml.SelectSingleNode("//status"), nTimeTaken)
                    Else
                        oElmt.InnerText = nTimeTaken
                    End If
                End If

                oDr.Close()
                oDr = Nothing

                ' let's add the date taken
                dDateTaken = GetDataValue("SELECT a.dInsertDate FROM tblAudit a INNER JOIN tblQuestionaireResult q ON a.nAuditKey = q.nAuditId AND nQResultsKey=" & CStr(nQuizId))
                If Not (dDateTaken Is DBNull.Value And dDateTaken Is Nothing) Then
                    oElmt = oQuizXml.SelectSingleNode("//status/dateTaken")
                    If oElmt Is Nothing Then
                        addNewTextNode("dateTaken", oQuizXml.SelectSingleNode("//status"), Eonic.Tools.Xml.XmlDate(dDateTaken, True))
                    Else
                        oElmt.InnerText = Eonic.Tools.Xml.XmlDate(dDateTaken, True)
                    End If
                End If

                Return oQuizXml
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getUserQuizInstance", ex, cProcessInfo))

                Return Nothing
            End Try
        End Function

        Public Function insertStructure(ByVal oInstance As XmlElement) As Long
            PerfMon.Log("DBHelper", "insertStructure (xmlElement)")
            Dim cProcessInfo As String = ""
            Try

                Return setObjectInstance(Web.dbHelper.objectTypes.ContentStructure, oInstance)

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "insertStructure(xmlElement)", ex, cProcessInfo))
                Return Nothing
            End Try

        End Function


        Public Function insertStructure(ByVal nStructParId As Long, ByVal cStructForiegnRef As String, ByVal cStructName As String, ByVal cStructDescription As String, ByVal cStructLayout As String, Optional ByVal nStatus As Long = 1, Optional ByVal dPublishDate As DateTime = #12:00:00 AM#, Optional ByVal dExpireDate As DateTime = #12:00:00 AM#, Optional ByVal cDescription As String = "", Optional ByVal nOrder As Long = 0) As Long
            PerfMon.Log("DBHelper", "insertStructure ([args])")
            Dim sSql As String
            Dim nId As String
            Dim cProcessInfo As String = ""
            Try
                sSql = "Insert Into tblContentStructure (nStructParId, cStructForiegnRef, cStructName,  cStructDescription, cStructLayout, nAuditId, nStructOrder)" & _
                "values (" & _
                nStructParId & _
                ",'" & SqlFmt(cStructForiegnRef) & "'" & _
                ",'" & SqlFmt(cStructName) & "'" & _
                ",'" & SqlFmt(cStructDescription) & "'" & _
                ",'" & SqlFmt(cStructLayout) & "'" & _
                "," & getAuditId(nStatus, , cDescription, dPublishDate, dExpireDate) & _
                "," & nOrder & ")"


                nId = GetIdInsertSql(sSql)
                clearStructureCacheAll()
                Return nId

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getAuditId", ex, cProcessInfo))

            End Try
        End Function

        Public Function insertPageVersion(ByVal nStructParId As Long, ByVal cStructForiegnRef As String, ByVal cStructName As String, ByVal cStructDescription As String, ByVal cStructLayout As String, Optional ByVal nStatus As Long = 1, Optional ByVal dPublishDate As DateTime = #12:00:00 AM#, Optional ByVal dExpireDate As DateTime = #12:00:00 AM#, Optional ByVal cDescription As String = "", Optional ByVal nOrder As Long = 0, Optional ByVal nVersionParId As Long = Nothing, Optional ByVal cVersionLang As String = "", Optional ByVal cVersionDescription As String = "", Optional ByVal nVersionType As PageVersionType = Nothing) As Long
            PerfMon.Log("DBHelper", "insertStructure ([args])")
            Dim sSql As String
            Dim nId As String
            Dim cProcessInfo As String = ""
            Try
                sSql = "Insert Into tblContentStructure (nStructParId, cStructForiegnRef, cStructName,  cStructDescription, cStructLayout, nAuditId, nStructOrder, nVersionParId, cVersionLang, cVersionDescription, nVersionType)" & _
                "values (" & _
                nStructParId & _
                ",'" & SqlFmt(cStructForiegnRef) & "'" & _
                ",'" & SqlFmt(cStructName) & "'" & _
                ",'" & SqlFmt(cStructDescription) & "'" & _
                ",'" & SqlFmt(cStructLayout) & "'" & _
                "," & getAuditId(nStatus, , cDescription, dPublishDate, dExpireDate) & _
                "," & nOrder & _
                "," & nVersionParId & _
                ",'" & cVersionLang & "'" & _
                ",'" & cVersionDescription & "'" & _
                "," & nVersionType & " )"

                nId = GetIdInsertSql(sSql)
                Return nId

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getAuditId", ex, cProcessInfo))

            End Try
        End Function

        Public Function moveShippingLocation(ByVal nLocKey As Long, ByVal nNewLocParId As Long) As Long
            PerfMon.Log("DBHelper", "moveShippingLocation")
            Dim sSql As String
            Dim nId As String
            Dim cProcessInfo As String = ""
            Try
                sSql = "UPDATE tblCartShippingLocations SET nLocationParId = " & nNewLocParId & " WHERE nLocationKey = " & nLocKey

                nId = ExeProcessSql(sSql)
                Return nId

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "moveShippingLocation", ex, cProcessInfo))

            End Try
        End Function

        Public Function moveStructure(ByVal nStructKey As Long, ByVal nNewStructParId As Long) As Long
            PerfMon.Log("DBHelper", "moveStructure")
            Dim sSql As String
            Dim nId As String
            Dim cProcessInfo As String = ""
            Try


                sSql = "UPDATE tblContentStructure SET nStructParId = " & nNewStructParId & " WHERE nStructKey = " & nStructKey


                nId = ExeProcessSql(sSql)
                clearStructureCacheAll()
                Return nId

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "moveStructure", ex, cProcessInfo))

            End Try
        End Function

        Public Overridable Function moveContent(ByVal nContentKey As Long, ByVal nStructKey As Long, ByVal nNewStructParId As Long) As Long
            PerfMon.Log("DBHelper", "moveContent")
            Dim nId As String
            Dim cProcessInfo As String = "moveContent"

            Try

                ' Don't move if destination doesn't exist.
                Dim destination As Integer = GetDataValue("SELECT TOP 1 nStructKey FROM tblContentStructure WHERE nStructKey=" & nNewStructParId, , , 0)

                If destination > 0 Then

                    ' Trying to work out what is needed for moving content to work accurately.
                    ' 1. you need to delete the location on the current page.
                    ' 2. if a location exists on the destination page then you need to remove that.
                    ' 3. you need to persist the Primary state
                    ' 4. you need to persist the Cascaded state
                    ' 5. you ought to persist the position.
                    ' 6. if in removing the destination location you remove the primary location record, you should make this the primary location record

                    ' Get the current location info
                    Dim primary As Boolean = False
                    Dim cascade As Boolean = False
                    Dim position As String = "column1"

                    Dim currentLocation As SqlDataReader = getDataReader("SELECT TOP 1 CASE WHEN bPrimary IS NULL THEN 0 ELSE bPrimary END as PrimaryLocation, CASE WHEN bCascade IS NULL THEN 0 ELSE bCascade END AS cascadeLocation, CASE WHEN cPosition IS NULL THEN '' ELSE cPosition END AS position from tblContentLocation where nStructId = " & nStructKey & " and nContentId = " & nContentKey)
                    If currentLocation.Read Then
                        primary = currentLocation(0)
                        cascade = currentLocation(1)
                        'position = currentLocation(2)
                        'TS By DEFAULT we move to column1 because it is always on every page.
                    End If
                    currentLocation.Close()

                    ' Work out if the destination is primary
                    Dim destinationPrimary As Integer = GetDataValue("SELECT TOP 1 bPrimary from tblContentLocation where nStructId = " & nNewStructParId & " and nContentId = " & nContentKey & " AND bPrimary=1", , , 0)

                    ' Work out if the destination is the only primary
                    Dim areThereOtherPrimaries As Integer = GetDataValue("SELECT TOP 1 bPrimary from tblContentLocation where nStructId <> " & nNewStructParId & " and nContentId = " & nContentKey & " AND bPrimary=1", , , 0)

                    ' If destination is the only primary then we are about to delete it so make the current location (being moved) primary
                    If destinationPrimary > 0 And areThereOtherPrimaries = 0 Then
                        primary = True
                    End If

                    ' Delete destination locations
                    ExeProcessSqlScalar("DELETE from tblContentLocation WHERE nContentId = " & nContentKey & " and nStructId = " & nNewStructParId)

                    ' Delete current location
                    ExeProcessSqlScalar("DELETE from tblContentLocation WHERE nContentId = " & nContentKey & " and nStructId = " & nStructKey)

                    ' Create new location
                    nId = setContentLocation(nNewStructParId, nContentKey, primary, cascade, True, position, True)

                Else

                    nId = nStructKey

                End If

                Return nId

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "moveContent", ex, cProcessInfo))

            End Try
        End Function

        Sub ReorderNode(ByVal objectType As objectTypes, ByVal nKey As Long, ByVal ReOrderCmd As String, Optional ByVal sSortField As String = "")
            PerfMon.Log("DBHelper", "ReorderNode")
            Dim sSql As String
            Dim oDs As DataSet
            Dim oDr As SqlDataReader
            Dim oRow As DataRow

            Dim nParentid As Long
            Dim RecCount As Long

            Dim i As Integer


            Dim sKeyField As String = getKey(objectType)

            Dim cProcessInfo As String = ""
            Try
                'Lets firsttest the object can be ordered
                If getOrderFname(objectType) <> "" Then
                    If (ReOrderCmd = "SortAlphaAsc" Or ReOrderCmd = "SortAlphaDesc") And sSortField <> "" Then
                        'Load Children
                        sSql = "Select * from " & getTable(objectType) & " where " & getParIdFname(objectType) & "=" & CStr(nKey) & " order by " & sSortField

                        If ReOrderCmd = "SortAlphaDesc" Then sSql = sSql & " DESC"
                    Else
                        'Load Principle
                        sSql = "Select * from " & getTable(objectType) & " where " & getKey(objectType) & "=" & CStr(nKey)
                        oDr = getDataReader(sSql)
                        While oDr.Read
                            nParentid = oDr.Item(getParIdFname(objectType))
                        End While
                        oDr.Close()
                        oDr = Nothing

                        'Get Siblings
                        sSql = "Select * from " & getTable(objectType) & " where " & getParIdFname(objectType) & "=" & CStr(nParentid) & " order by " & getOrderFname(objectType)
                    End If

                    oDs = getDataSetForUpdate(sSql, getTable(objectType), "results")

                    RecCount = oDs.Tables(getTable(objectType)).Rows.Count
                    i = 1
                    Dim skipnext As Boolean = False

                    Select Case ReOrderCmd
                        Case "MoveTop"
                            For Each oRow In oDs.Tables(getTable(objectType)).Rows
                                If oRow(sKeyField) = nKey Then
                                    oRow(getOrderFname(objectType)) = 1
                                Else
                                    oRow(getOrderFname(objectType)) = i + 1
                                    i = i + 1
                                End If
                            Next
                        Case "MoveBottom"
                            For Each oRow In oDs.Tables(getTable(objectType)).Rows
                                If oRow(sKeyField) = nKey Then
                                    oRow(getOrderFname(objectType)) = RecCount
                                Else
                                    oRow(getOrderFname(objectType)) = i
                                    i = i + 1
                                End If
                            Next
                        Case "MoveUp"
                            For Each oRow In oDs.Tables(getTable(objectType)).Rows
                                If oRow(sKeyField) = nKey And i <> 1 Then
                                    'swap with previous
                                    oDs.Tables(getTable(objectType)).Rows(i - 2).Item(getOrderFname(objectType)) = i
                                    oRow(getOrderFname(objectType)) = i - 1
                                Else
                                    oRow(getOrderFname(objectType)) = i
                                End If
                                i = i + 1
                            Next
                        Case "MoveDown"
                            For Each oRow In oDs.Tables(getTable(objectType)).Rows
                                If oRow(sKeyField) = nKey And i <> (RecCount) Then
                                    'swap with next
                                    oDs.Tables(getTable(objectType)).Rows(i).Item(getOrderFname(objectType)) = i
                                    oRow(getOrderFname(objectType)) = i + 1
                                    skipnext = True
                                Else
                                    If Not skipnext Then
                                        oRow(getOrderFname(objectType)) = i
                                        skipnext = False
                                    End If
                                End If
                                i = i + 1
                            Next
                        Case Else
                            For Each oRow In oDs.Tables(getTable(objectType)).Rows
                                oRow(getOrderFname(objectType)) = i
                                i = i + 1
                            Next
                    End Select

                    updateDataset(oDs, getTable(objectType))

                    clearStructureCacheAll()

                End If

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "reOrderNode", ex, cProcessInfo))

            End Try
        End Sub

        Sub ReorderContent(ByVal nPgId As Long, ByVal nContentId As Long, ByVal ReOrderCmd As String, Optional ByVal bIsRelatedContent As Boolean = False, Optional ByVal cPosition As String = "")
            PerfMon.Log("DBHelper", "ReorderContent")
            Dim sSql As String
            Dim oDs As DataSet
            Dim oDr As SqlDataReader
            Dim oRow As DataRow

            Dim cSchemaName As String = ""
            Dim RecCount As Long


            Dim i As Integer

            Dim objectType As objectTypes
            Dim sKeyField As String
            If bIsRelatedContent Then
                objectType = objectTypes.ContentRelation
                'sKeyField = "nContentRelationKey"
                sKeyField = "nContentChildId"
            Else
                objectType = objectTypes.ContentLocation
                sKeyField = "nContentId"
            End If



            Dim cProcessInfo As String = ""
            Try


                'Lets go and get the content type

                sSql = "Select cContentSchemaName from tblContent where nContentKey = " & nContentId
                oDr = getDataReader(sSql)
                While oDr.Read
                    cSchemaName = oDr.Item("cContentSchemaName")
                End While
                oDr.Close()
                oDr = Nothing

                'Get all locations of similar objects on the same page.

                sSql = "Select CL.* from tblContentLocation as CL inner join tblContent as C on C.nContentKey = CL.nContentId where CL.nStructId =" & nPgId & " and C.cContentSchemaName = '" & cSchemaName & "' order by nDisplayOrder"

                If cPosition <> "" Then
                    If cPosition.EndsWith("-") Then
                        sSql = "Select CL.* from tblContentLocation as CL inner join tblContent as C on C.nContentKey = CL.nContentId where CL.nStructId =" & nPgId & " and CL.cPosition like'" & cPosition & "%' and C.cContentSchemaName = '" & cSchemaName & "' order by nDisplayOrder"
                    Else
                        sSql = "Select CL.* from tblContentLocation as CL inner join tblContent as C on C.nContentKey = CL.nContentId where CL.nStructId =" & nPgId & " and CL.cPosition ='" & cPosition & "' and C.cContentSchemaName = '" & cSchemaName & "' order by nDisplayOrder"
                    End If
                End If

                If bIsRelatedContent Then
                    sSql = "SELECT tblContentRelation.* FROM tblContentRelation INNER JOIN" & _
                       " tblContent ON tblContentRelation.nContentChildId = tblContent.nContentKey INNER JOIN" & _
                       " tblContent tblContent_1 ON tblContent.cContentSchemaName = tblContent_1.cContentSchemaName" & _
                       " WHERE (tblContentRelation.nContentParentId = " & nPgId & ") AND (tblContent_1.nContentKey = " & nContentId & ")" & _
                       " ORDER BY tblContentRelation.nDisplayOrder"
                End If

                oDs = getDataSetForUpdate(sSql, getTable(objectType), "results")

                RecCount = oDs.Tables(getTable(objectType)).Rows.Count
                i = 1
                Dim skipnext As Boolean = False

                Select Case ReOrderCmd
                    Case "MoveTop"
                        For Each oRow In oDs.Tables(getTable(objectType)).Rows
                            If oRow(sKeyField) = nContentId Then
                                oRow(getOrderFname(objectType)) = 1
                            Else
                                oRow(getOrderFname(objectType)) = i + 1
                                i = i + 1
                            End If
                            'non-ideal alternative for updating the entire dataset
                            sSql = "update " & getTable(objectType) & " set nDisplayOrder = " & oRow(getOrderFname(objectType)) & " where nAuditId = " & oRow("nAuditId")
                            ExeProcessSql(sSql)
                        Next
                    Case "MoveBottom"
                        For Each oRow In oDs.Tables(getTable(objectType)).Rows
                            If oRow(sKeyField) = nContentId Then
                                oRow(getOrderFname(objectType)) = RecCount
                            Else
                                oRow(getOrderFname(objectType)) = i
                                i = i + 1
                            End If
                            'non-ideal alternative for updating the entire dataset
                            sSql = "update " & getTable(objectType) & " set nDisplayOrder = " & oRow(getOrderFname(objectType)) & " where nAuditId = " & oRow("nAuditId")
                            ExeProcessSql(sSql)
                        Next
                    Case "MoveUp"
                        For Each oRow In oDs.Tables(getTable(objectType)).Rows
                            If oRow(sKeyField) = nContentId And i <> 1 Then
                                'swap with previous
                                oDs.Tables(getTable(objectType)).Rows(i - 2).Item(getOrderFname(objectType)) = i
                                sSql = "update " & getTable(objectType) & " set nDisplayOrder = " & oDs.Tables(getTable(objectType)).Rows(i - 2).Item(getOrderFname(objectType)) & " where nAuditId = " & oDs.Tables(getTable(objectType)).Rows(i - 2).Item("nAuditId")
                                ExeProcessSql(sSql)

                                oRow(getOrderFname(objectType)) = i - 1
                                sSql = "update " & getTable(objectType) & " set nDisplayOrder = " & oRow(getOrderFname(objectType)) & " where nAuditId = " & oRow("nAuditId")
                                ExeProcessSql(sSql)
                            Else
                                oRow(getOrderFname(objectType)) = i
                                sSql = "update " & getTable(objectType) & " set nDisplayOrder = " & oRow(getOrderFname(objectType)) & " where nAuditId = " & oRow("nAuditId")
                                ExeProcessSql(sSql)
                            End If
                            'non-ideal alternative for updating the entire dataset
                            ' sSql = "update tblContentLocation set nDisplayOrder = " & oRow(getOrderFname(objectType)) & " where nAuditId = " & oRow("nAuditId")
                            ' exeProcessSQL(sSql)
                            i = i + 1
                        Next
                    Case "MoveDown"
                        For Each oRow In oDs.Tables(getTable(objectType)).Rows
                            If oRow(sKeyField) = nContentId And i <> (RecCount) Then
                                'swap with next
                                oDs.Tables(getTable(objectType)).Rows(i).Item(getOrderFname(objectType)) = i
                                oRow(getOrderFname(objectType)) = i + 1
                                skipnext = True
                            Else
                                If Not skipnext Then
                                    oRow(getOrderFname(objectType)) = i
                                    skipnext = False
                                End If
                            End If

                            'non-ideal alternative for updating the entire dataset
                            sSql = "update " & getTable(objectType) & " set nDisplayOrder = " & oRow(getOrderFname(objectType)) & " where nAuditId = " & oRow("nAuditId")
                            ExeProcessSql(sSql)

                            i = i + 1
                        Next
                    Case Else
                        For Each oRow In oDs.Tables(getTable(objectType)).Rows
                            oRow(getOrderFname(objectType)) = i
                            i = i + 1
                        Next
                End Select
                Dim sXml As String = oDs.GetXml
                'This won't work as we are drawing from 2 tables
                'updateDataset(oDs, getTable(objectType))

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "reOrderContent", ex, cProcessInfo))

            End Try
        End Sub

        Public Sub copyPageContent(ByVal nSourcePageId As Long, ByVal nTargetPageId As Long, ByVal bCopyDescendants As Boolean, ByVal mode As CopyContentType, Optional ByVal oMenuItem As XmlElement = Nothing)
            PerfMon.Log("DBHelper", "copyPageContent")
            Dim cProcessInfo As String = ""
            Dim sSql As String

            Try
                'First we will get all the content on the current page
                'ignoring cascaded items
                cProcessInfo = "Retreiving Original Content"

                sSql = "SELECT nContentId, bPrimary, bCascade, cPosition FROM tblContentLocation WHERE (nStructId = " & nSourcePageId & ") ORDER BY nDisplayOrder"


                Dim oDS As DataSet
                oDS = GetDataSet(sSql, "Content", "Contents")

                Dim oDr As DataRow
                'check if we are doing anything with the content
                If Not mode = CopyContentType.None Then
                    For Each oDr In oDS.Tables("Content").Rows

                        Dim nContentId As Integer = 0
                        Dim bNewItem As Boolean = False
                        'Debug.WriteLine(oDr("bPrimary"))
                        If mode = CopyContentType.Copy And oDr("bPrimary") = True Then
                            bNewItem = True
                            ''if we are copying we need an instance
                            'Dim oInstanceXML As New XmlDocument
                            'oInstanceXML.AppendChild(oInstanceXML.CreateElement("instance"))
                            'oInstanceXML.DocumentElement.InnerXml = getObjectInstance(objectTypes.Content, oDr("nContentId"))
                            'Dim oContentElmt As XmlElement
                            ''now we need to remove ids, audits etc
                            'For Each oContentElmt In oInstanceXML.DocumentElement.FirstChild.SelectNodes("nContentKey | nContentPrimaryId | nAuditId | nAuditKey")
                            '    oContentElmt.InnerText = ""
                            'Next
                            ''save it as a new piece of content and get the id
                            'nContentId = setObjectInstance(objectTypes.Content, oInstanceXML.DocumentElement)
                            ''get any items related to the origional that are not orphan (have no page) or are related to other items.
                            '' - copy relations to the new object.
                            'sSql = "select nContentChildId, nDisplayOrder, cRelationtype," & _
                            '"(select COUNT(nContentId) from tblContentLocation l where l.nContentId = r.nContentChildId) as nLocations, " & _
                            '"(select COUNT(nContentParentId) from tblContentRelation r2 where r2.nContentChildId = r.nContentChildId) as nRelations, " & _
                            '"(select COUNT(nContentParentId) from tblContentRelation r3 where r3.nContentParentId = r.nContentChildId and r3.nContentChildId = r.nContentParentId) as twoWay " & _
                            '"from tblContentRelation r where nContentParentId = " & oDr("nContentId")
                            'Dim oDS2 As DataSet
                            'Dim oDr2 As DataRow
                            'oDS2 = GetDataSet(sSql, "Relations", "Relations")
                            'For Each oDr2 In oDS.Tables("Relations").Rows
                            '    If oDr2("nLocations") = 0 And oDr2("nRelations") = 1 Then
                            '        'we copy and releate
                            '    Else
                            '        'we simply relate
                            '        insertContentRelation(nContentId, oDr2("nContentId"), oDr2("twoWay"), oDr2("cRelationType"), True)
                            '    End If
                            'Next
                            ''get orphan items that are only related to the origional 
                            '' - copy the items and related them to our object

                            nContentId = createContentCopy(oDr("nContentId"))

                        ElseIf mode = CopyContentType.Locate Then
                            'just get the id
                            nContentId = oDr("nContentId") 'just need to do a locations
                        Else
                            'locate with  new primaries
                            nContentId = oDr("nContentId")
                            If oDr("bPrimary") = True Then bNewItem = True
                        End If
                        'now set a location
                        setContentLocation(nTargetPageId, nContentId, bNewItem, IIf(IsDBNull(oDr("bCascade")), False, oDr("bCascade")), False, IIf(IsDBNull(oDr("cPosition")), "", oDr("cPosition")), True)
                        'usgin a different one since this isnt working for some reason
                        'setContentLocation2(nTargetPageId, nContentId, bNewItem, False)


                    Next
                End If

                'now we have done that page we need to look at the children

                If Not bCopyDescendants Then Exit Sub
                'do we need to create the menu for this?
                If oMenuItem Is Nothing Then
                    'get the full menu
                    sSql = "SELECT nStructKey, nStructParId  FROM tblContentStructure"
                    oDS = GetDataSet(sSql, "MenuItem", "Menu")
                    oDS.Tables("MenuItem").Columns("nStructKey").ColumnMapping = MappingType.Attribute
                    oDS.Tables("MenuItem").Columns("nStructParId").ColumnMapping = MappingType.Hidden
                    oDS.Relations.Add("Rel01", oDS.Tables("MenuItem").Columns("nStructKey"), oDS.Tables("MenuItem").Columns("nStructParId"), False)
                    oDS.Relations("Rel01").Nested = True
                    Dim oMenuXml As New XmlDocument
                    oMenuXml.InnerXml = oDS.GetXml
                    'now select the menu item we need
                    oMenuItem = oMenuXml.SelectSingleNode("descendant-or-self::MenuItem[@nStructKey=" & nSourcePageId & "]")
                    oMenuXml = Nothing
                End If
                'we need to go through its children and create items
                Dim oMenuChild As XmlElement
                For Each oMenuChild In oMenuItem.ChildNodes

                    Dim nMenuId As Integer = oMenuChild.GetAttribute("nStructKey") 'new source page id
                    Dim nNewMenuID As Integer 'new target page

                    Dim oMenuItemXML As New XmlDocument 'the xml of the item
                    oMenuItemXML.AppendChild(oMenuItemXML.CreateElement("instance"))
                    oMenuItemXML.DocumentElement.InnerXml = getObjectInstance(objectTypes.ContentStructure, nMenuId)

                    Dim oMenuElmt As XmlElement
                    For Each oMenuElmt In oMenuItemXML.DocumentElement.FirstChild.SelectNodes("nStructKey | nAuditId | nAuditKey")
                        oMenuElmt.InnerText = ""
                    Next
                    'ste the parent new id as current page
                    oMenuElmt = oMenuItemXML.DocumentElement.FirstChild.SelectSingleNode("nStructParId")
                    oMenuElmt.InnerText = nTargetPageId
                    'get the new id
                    nNewMenuID = setObjectInstance(objectTypes.ContentStructure, oMenuItemXML.DocumentElement)
                    'call the function again
                    copyPageContent(nMenuId, nNewMenuID, bCopyDescendants, mode, oMenuChild)
                Next

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "copyPageContent", ex, cProcessInfo))

            End Try
        End Sub

        Public Function createContentCopy(ByVal contentId As Long) As Long
            PerfMon.Log("DBHelper", "insertContent")
            Dim cProcessInfo As String = ""
            Dim nContentId As Long
            Dim sSql As String
            Dim oDS2 As DataSet
            Dim oDr2 As DataRow
            Try

                Dim oInstanceXML As New XmlDocument
                oInstanceXML.AppendChild(oInstanceXML.CreateElement("instance"))
                oInstanceXML.DocumentElement.InnerXml = getObjectInstance(objectTypes.Content, contentId)
                Dim oContentElmt As XmlElement
                'now we need to remove ids, audits etc
                For Each oContentElmt In oInstanceXML.DocumentElement.FirstChild.SelectNodes("nContentKey | nContentPrimaryId | nAuditId | nAuditKey")
                    oContentElmt.InnerText = ""
                Next
                'save it as a new piece of content and get the id
                nContentId = setObjectInstance(objectTypes.Content, oInstanceXML.DocumentElement)
                'get any items related to the origional that are not orphan (have no page) or are related to other items.
                ' - copy relations to the new object.
                sSql = "select nContentChildId, nDisplayOrder, cRelationtype," & _
                "(select COUNT(nContentId) from tblContentLocation l where l.nContentId = r.nContentChildId) as nLocations, " & _
                "(select COUNT(nContentParentId) from tblContentRelation r2 where r2.nContentChildId = r.nContentChildId) as nRelations, " & _
                "(select COUNT(nContentParentId) from tblContentRelation r3 where r3.nContentParentId = r.nContentChildId and r3.nContentChildId = r.nContentParentId) as twoWay " & _
                "from tblContentRelation r where nContentParentId = " & contentId

                oDS2 = GetDataSet(sSql, "Relations", "Relations")
                For Each oDr2 In oDS2.Tables("Relations").Rows
                    If oDr2("nLocations") = 0 And oDr2("nRelations") = 1 Then
                        'we copy and releate because it is orphan and only related to our item
                        Dim newRelatedContentId As String = createContentCopy(oDr2("nContentChildId"))
                        insertContentRelation(nContentId, newRelatedContentId, oDr2("twoWay"), oDr2("cRelationType"), True)
                    Else
                        'we simply relate because it is either a page or has multiple relations
                        insertContentRelation(nContentId, oDr2("nContentChildId"), oDr2("twoWay"), oDr2("cRelationType"), True)
                    End If
                Next
                oContentElmt = Nothing
                oInstanceXML = Nothing

                Return nContentId

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "createContentCopy", ex, cProcessInfo))
                Return Nothing

            End Try
        End Function


        Public Function insertContent(ByVal cContentForiegnRef As String, ByVal cContentName As String, ByVal cSchemaName As String, ByVal cContentXmlBrief As String, ByVal cContentXmlDetail As String, Optional ByVal nParentID As Integer = 0, Optional ByVal dPublishDate As Object = Nothing, Optional ByVal dExpireDate As Object = Nothing, Optional ByVal nContentStatus As Integer = 1) As Long
            PerfMon.Log("DBHelper", "insertContent")
            Dim sSql As String
            Dim nId As String
            Dim cProcessInfo As String = ""
            Try
                sSql = "Insert Into tblContent (nContentPrimaryId, nVersion, cContentForiegnRef, cContentName,  cContentSchemaName, cContentXmlBrief, cContentXmlDetail, nAuditId)" & _
                "values (" & nParentID & "," & _
                "1" & _
                ",'" & SqlFmt(cContentForiegnRef) & "'" & _
                ",'" & SqlFmt(cContentName) & "'" & _
                ",'" & SqlFmt(cSchemaName) & "'" & _
                ",'" & SqlFmt(cContentXmlBrief) & "'" & _
                ",'" & SqlFmt(cContentXmlDetail) & "'" & _
                "," & getAuditId(nContentStatus, , , dPublishDate, dExpireDate) & ")"

                nId = GetIdInsertSql(sSql)

                Return nId
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "insertContent", ex, cProcessInfo))

            End Try

        End Function

        Public Sub updateContent(ByVal nContentId As Long, ByVal cContentName As String, ByVal cContentXmlBrief As String, ByVal cContentXmlDetail As String)
            PerfMon.Log("DBHelper", "updateContent")
            Dim sSql As String
            Dim cProcessInfo As String = ""
            Try
                sSql = "update tblContent Set" & _
                " nContentPrimaryId = " & nContentId & _
                " , nVersion = nVersion + 1" & _
                " , cContentName = '" & SqlFmt(cContentName) & "'" & _
                " , cContentXmlBrief = '" & SqlFmt(cContentXmlBrief) & "'" & _
                " , cContentXmlDetail = '" & SqlFmt(cContentXmlDetail) & "'" & _
                " where nContentKey = " & nContentId

                ExeProcessSql(sSql)

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "insertContent", ex, cProcessInfo))
            End Try

        End Sub

        Public Function setContentLocation(ByVal nStructId As Long, ByVal nContentId As Long, Optional ByVal bPrimary As Boolean = False, Optional ByVal bCascade As Boolean = False, Optional ByVal bOveridePrimary As Boolean = False, Optional ByVal cPosition As String = "", Optional ByVal bUpdatePosition As Boolean = True) As Integer
            PerfMon.Log("DBHelper", "setContentLocation")
            'this is so we can save some content without trying to change any locations
            If nStructId = 0 Or nContentId = 0 Then Exit Function

            Dim sSql As String
            Dim oDs As DataSet
            Dim oRow As DataRow
            Dim nId As String
            Dim cProcessInfo As String = ""
            Dim bReorderLocations As Boolean = False
            Try
                'does this content relationship exist?
                sSql = "select * from tblContentLocation where nStructId = " & nStructId & " and nContentId=" & nContentId
                oDs = getDataSetForUpdate(sSql, "ContentLocation", "Location")
                If oDs.Tables("ContentLocation").Rows.Count = 0 Then
                    oRow = oDs.Tables("ContentLocation").NewRow()
                    oRow("nStructId") = nStructId
                    oRow("nContentId") = nContentId
                    oRow("bPrimary") = bPrimary
                    oRow("bCascade") = bCascade
                    oRow("nDisplayOrder") = 0
                    If cPosition <> "" Then
                        oRow("cPosition") = cPosition
                    End If
                    oRow("nAuditId") = getAuditId()
                    oDs.Tables("ContentLocation").Rows.Add(oRow)
                    bReorderLocations = True
                Else
                    oRow = oDs.Tables("ContentLocation").Rows(0)
                    oRow.BeginEdit()
                    'if we are allready primary then leave it.. Unless we need for force it as in External Syncronisation XSLT
                    If bOveridePrimary Then
                        oRow("bPrimary") = bPrimary
                    End If
                    If bUpdatePosition And cPosition <> "" Then
                        oRow("cPosition") = cPosition
                    End If
                    oRow("bCascade") = bCascade
                    oRow.EndEdit()
                    'update the audit table
                End If

                updateDataset(oDs, "ContentLocation", False)
                nId = ExeProcessSql(sSql)

                If bReorderLocations Then
                    If Not myWeb Is Nothing Then
                        If myWeb.mcBehaviourNewContentOrder <> "" Then
                            Me.ReorderContent(nStructId, nContentId, myWeb.mcBehaviourNewContentOrder)
                        End If
                    End If
                End If
                Return nId
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "setContentLocation", ex, cProcessInfo))
            End Try

        End Function

        Public Function setContentLocation2(ByVal nStructId As Long, ByVal nContentId As Long, Optional ByVal bPrimary As Boolean = False, Optional ByVal bCascade As Boolean = False) As Integer
            PerfMon.Log("DBHelper", "setContentLocation2")
            'this is so we can save some content without trying to change any locations
            If nStructId = 0 Or nContentId = 0 Then Exit Function

            Dim sSql As String
            Dim nId As String
            Dim cProcessInfo As String = ""
            Try
                'does this content relationship exist?
                sSql = "select * from tblContentLocation where nStructId = " & nStructId & " and nContentId=" & nContentId
                nId = ExeProcessSqlScalar(sSql)


                If nId = 0 Then
                    sSql = "INSERT INTO tblContentLocation (nStructId, nContentId, bPrimary, bCascade, nDisplayOrder, nAuditId) VALUES ("
                    sSql &= nStructId & ","
                    sSql &= nContentId & ","
                    sSql &= IIf(bPrimary, 1, 0) & ","
                    sSql &= IIf(bCascade, 1, 0) & ","
                    sSql &= "0,"
                    sSql &= getAuditId() & ");select @@identity"
                Else
                    sSql = "UPDATE tblContentLocation  SET "
                    sSql &= "nStructId =" & nStructId & ","
                    sSql &= "nContentId =" & nContentId & ","
                    'sSql &= "bPrimary =" & bPrimary & ","
                    sSql &= "bCascade =" & IIf(bCascade, 1, 0) & ","
                    sSql &= "nDisplayOrder =" & "0"
                    'sSql &= "nAuditId =" & getAuditId() & ")"
                    sSql &= " WHERE nContentLocationKey = " & nId
                End If

                nId = ExeProcessSqlScalar(sSql)
                Return nId
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "setContentLocation", ex, cProcessInfo))
            End Try

        End Function

        Public Sub insertShippingLocation(ByVal nOptId As Long, ByVal nLocId As Long, Optional ByVal bPrimary As Boolean = True)
            PerfMon.Log("DBHelper", "insertShippingLocation")
            Dim sSql As String
            Dim nId As String
            Dim cProcessInfo As String = ""
            Try
                sSql = "Insert Into tblCartShippingRelations(nShpOptId, nShpLocId, nAuditId)" & _
                "values (" & nOptId & _
                "," & nLocId & _
                "," & getAuditId() & ")"

                nId = ExeProcessSql(sSql)

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "insertShippingLocation", ex, cProcessInfo))
            End Try

        End Sub

        Public Function getLocationByRef(ByVal cForiegnRef As String) As Long
            PerfMon.Log("DBHelper", "getLocationByRef")
            Dim sSql As String
            Dim nId As String = 0
            Dim oDr As SqlDataReader


            Dim cProcessInfo As String = ""
            Try
                sSql = "select nStructKey from tblContentStructure where cStructForiegnRef = '" & cForiegnRef & "'"

                oDr = getDataReader(sSql)

                While oDr.Read
                    nId = oDr(0)
                End While
                oDr.Close()
                oDr = Nothing

                Return nId

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getLocationByRef", ex, cProcessInfo))

            End Try

        End Function

        Public Function getFRefFromPageId(ByVal nPageId As Long) As String
            PerfMon.Log("DBHelper", "getLocationByRef")
            Dim sSql As String
            Dim nId As String = ""

            Dim cProcessInfo As String = ""
            Try
                sSql = "select cStructForiegnRef from tblContentStructure where nStructKey = " & nPageId

                nId = Me.GetDataValue(sSql)

                Return nId

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getLocationByRef", ex, cProcessInfo))
            End Try

        End Function

        Public Function getPrimaryLocationByArtId(ByVal nArtId As Long) As Long
            PerfMon.Log("DBHelper", "getPrimaryLocationByArtId")
            Dim sSql As String
            Dim nId As String = 0
            Dim oDr As SqlDataReader


            Dim cProcessInfo As String = ""
            Try
                sSql = "SELECT nStructId FROM tblContentLocation WHERE nContentId = " & nArtId & " and (bPrimary = -1 or bPrimary = 1)"

                oDr = getDataReader(sSql)

                While oDr.Read
                    nId = oDr(0)
                End While
                oDr.Close()
                oDr = Nothing

                Return nId

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getLocationByRef", ex, cProcessInfo))

            End Try

        End Function

        Public Overridable Sub getContentFromModuleGrabber(ByRef oContent As XmlElement)
            PerfMon.Log("DBHelper", "getContentFromModuleGrabber")

            Dim cProcessInfo As String = ""
            Try

                Dim cWhereSql As String = ""
                Dim cOrderBy As String = ""

                ' Get the parameters SortDirection
                Dim cSchema As String = oContent.GetAttribute("contentType")
                Dim nPageId As Long = CLng("0" & oContent.GetAttribute("grabberRoot"))
                Dim nTop As Long = CLng("0" & oContent.GetAttribute("grabberItems"))
                Dim cSort As String = oContent.GetAttribute("sortBy")
                Dim cSortDirection As String = oContent.GetAttribute("order")
                Dim cIncludeChildPages As String = oContent.GetAttribute("grabberItterate")

                ' Validate and Build the SQL conditions that we are going to need

                If cSchema <> "" _
                    AndAlso nTop > 0 Then

                    cWhereSql = "cContentSchemaName = " & SqlString(cSchema) & " "

                    If nPageId > 0 Then
                        If cIncludeChildPages.ToLower = "true" Then
                            ' Get the page from the structure and enumerate its children
                            Dim cPageIds As String = "" & nPageId.ToString
                            For Each oPage As XmlElement In myWeb.moPageXml.SelectNodes("/Page/Menu//MenuItem[@id=" & nPageId.ToString & "]//MenuItem")
                                cPageIds &= "," & oPage.GetAttribute("id")
                            Next
                            cWhereSql &= " AND CL.nStructId IN (" & cPageIds & ") "
                        Else
                            cWhereSql &= " AND CL.nStructId=" & SqlFmt(nPageId) & " "
                        End If
                    End If


                    Select Case cSort
                        Case "CreationDate"
                            cOrderBy = "dInsertDate"
                        Case "Name"
                            cOrderBy = "cContentName"
                        Case "PublishDate", "publish"
                            cOrderBy = "dPublishDate"
                        Case "ExpireDate", "expire"
                            cOrderBy = "dExpireDate"
                        Case "StartDate" ' to catch historic error
                            cOrderBy = "dExpireDate"
                        Case "EndDate" ' to catch historic error
                            cOrderBy = "dExpireDate"
                        Case Else
                            cOrderBy = cSort
                    End Select

                    If cOrderBy <> "" AndAlso LCase(cSortDirection) = "descending" Then cOrderBy &= " DESC"

                    myWeb.GetPageContentFromSelect(cWhereSql, , , myWeb.mbAdminMode, nTop, cOrderBy, oContent)

                End If


            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getContentFromContentGrabber", ex, cProcessInfo))
            End Try
        End Sub

        Public Overridable Sub getContentFromContentGrabber(ByRef oGrabber As XmlElement)
            PerfMon.Log("DBHelper", "getContentFromContentGrabber")

            Dim cProcessInfo As String = ""
            Try

                Dim cWhereSql As String = ""
                Dim cOrderBy As String = ""

                ' Get the parameters SortDirection
                Dim cSchema As String = getNodeValueByType(oGrabber, "Type")
                Dim nPageId As Long = getNodeValueByType(oGrabber, "Page", dataType.TypeNumber)
                Dim nTop As Long = getNodeValueByType(oGrabber, "NumberOfItems", dataType.TypeNumber)
                Dim cSort As String = getNodeValueByType(oGrabber, "Sort")
                Dim cSortDirection As String = getNodeValueByType(oGrabber, "SortDirection")
                Dim cIncludeChildPages As String = getNodeValueByType(oGrabber, "IncludeChildPages")
                Dim joinSQL As String = Nothing

                ' Validate and Build the SQL conditions that we are going to need

                If cSchema <> "" _
                    AndAlso nTop > 0 Then

                    cWhereSql = "cContentSchemaName = " & SqlString(cSchema) & " "

                    If nPageId > 0 Then
                        If cIncludeChildPages.ToLower = "true" Then
                            ' Get the page from the structure and enumerate its children
                            Dim cPageIds As String = "(" & nPageId.ToString & ")"
                            For Each oPage As XmlElement In myWeb.moPageXml.SelectNodes("/Page/Menu//MenuItem[@id=" & nPageId.ToString & "]//MenuItem")
                                cPageIds &= ", (" & oPage.GetAttribute("id") & ")"
                            Next
                            joinSQL = " Join(values " & cPageIds & ") V(pageRef) on V.pageRef = CL.nStructId "
                        Else
                            cWhereSql &= " AND CL.nStructId=" & SqlFmt(nPageId) & " "
                        End If
                    End If


                    Select Case cSort
                        Case "CreationDate"
                            cOrderBy = "dInsertDate"
                        Case "Name"
                            cOrderBy = "cContentName"
                        Case "PublishDate"
                            cOrderBy = "dPublishDate"
                        Case "ExpireDate"
                            cOrderBy = "dExpireDate"
                        Case "StartDate" ' to catch historic error
                            cOrderBy = "dExpireDate"
                        Case "EndDate" ' to catch historic error
                            cOrderBy = "dExpireDate"
                        Case Else
                            cOrderBy = cSort
                    End Select

                    If cOrderBy <> "" AndAlso cSortDirection = "Descending" Then cOrderBy &= " DESC"
                    PerfMon.Log("DBHelper", "getContentFromContentGrabber-Start")
                    myWeb.GetPageContentFromSelect(cWhereSql, , , , nTop, cOrderBy,, joinSQL)
                    PerfMon.Log("DBHelper", "getContentFromContentGrabber-End")

                End If


            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getContentFromContentGrabber", ex, cProcessInfo))
            End Try
        End Sub

        Public Function getContentByRef(ByVal cForiegnRef As String) As Long
            PerfMon.Log("DBHelper", "getContentByRef")
            Dim sSql As String
            Dim nId As String = 0
            Dim oDr As SqlDataReader


            Dim cProcessInfo As String = ""
            Try
                sSql = "select nContentKey from tblContent where cContentForiegnRef = '" & cForiegnRef & "'"

                oDr = getDataReader(sSql)

                While oDr.Read
                    nId = oDr(0)
                End While
                oDr.Close()
                oDr = Nothing

                Return nId

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getContentByRef", ex, cProcessInfo))

            End Try

        End Function

        Public Function getContentBrief(ByVal nId As Integer) As String
            PerfMon.Log("DBHelper", "getContentBrief")
            Dim sSql As String
            Dim oDr As SqlDataReader
            Dim cContent As String = ""

            Dim cProcessInfo As String = ""
            Try
                sSql = "select cContentXmlBrief from tblContent where nContentKey = " & nId

                oDr = getDataReader(sSql)

                While oDr.Read
                    cContent = oDr(0)
                End While
                oDr.Close()
                oDr = Nothing

                Return cContent

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getContentByRef", ex, cProcessInfo))

                Return ""
            End Try
        End Function

        Public Function getContentFilePath(ByVal oRow As DataRow, Optional ByVal URLXpath As String = "/Content/Path") As String
            PerfMon.Log("DBHelper", "getContentFilePath")
            Dim cProcessInfo As String = ""
            Dim oXml As XmlDocument = New XmlDocument
            Dim oPathElmt As XmlElement
            Dim sPath As String = ""
            Try
                If URLXpath = "" Then URLXpath = "/Content/Path"

                oXml.InnerXml = oRow("cContentXmlBrief")
                oPathElmt = oXml.SelectSingleNode(URLXpath)
                If Not oPathElmt Is Nothing Then
                    sPath = oPathElmt.InnerText
                End If
                If oPathElmt Is Nothing Or sPath = "" Then
                    cProcessInfo = "No 'Path' in Brief"
                    oXml.InnerXml = oRow("cContentXmlDetail")
                    oPathElmt = oXml.SelectSingleNode(URLXpath)
                    If oPathElmt Is Nothing Then
                        cProcessInfo = "No 'Path' in Detail"
                    Else
                        sPath = oPathElmt.InnerText
                    End If
                End If
                'if no path found return nothing
                If sPath = "" Then
                    Return ""
                Else
                    If sPath.StartsWith("..") Then
                        Return goServer.MapPath("/") & goServer.UrlDecode(Eonic.Tools.Xml.convertEntitiesToString(Eonic.Tools.Xml.convertEntitiesToString(sPath)))
                    Else
                        Return goServer.MapPath("/" & goServer.UrlDecode(Eonic.Tools.Xml.convertEntitiesToString(Eonic.Tools.Xml.convertEntitiesToString(sPath))))
                    End If
                End If

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getContentFilePath", ex, cProcessInfo))
                Return ""
            End Try
        End Function

        Public Function getContentType(ByVal nId As Integer) As String

            Dim sSql As String
            Dim sContentSchemaName As String = ""
            Dim oDr As SqlDataReader


            Dim cProcessInfo As String = ""
            Try
                sSql = "select cContentSchemaName from tblContent where nContentKey = " & nId

                oDr = getDataReader(sSql)

                While oDr.Read
                    sContentSchemaName = oDr(0)
                End While
                oDr.Close()
                oDr = Nothing

                Return sContentSchemaName

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getContentType", ex, cProcessInfo))

                Return ""
            End Try

        End Function

        Public Function getContentVersions(ByRef nContentId As Long) As XmlElement
            PerfMon.Log("DBHelper", "getContentVersions")
            Dim oRoot As XmlElement
            Dim sSqlContent As String
            Dim sSqlVersions As String

            Dim oDs As DataSet
            Dim oDs2 As DataSet

            Dim cProcessInfo As String = "getContentVersions"
            Try

                oRoot = moPageXml.CreateElement("ContentDetail")

                sSqlContent = "" & _
                "select c.nContentKey as id,  c.nContentPrimaryId as primaryId,  c.nVersion as version,  cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, a.nStatus as status, a.dInsertDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], dins.cDirName as owner, dins.cDirXml as ownerDetail, dupd.cDirName as updater, dupd.cDirXml as updaterDetail " & _
                "from tblContent c inner join tblAudit a on c.nAuditId = a.nAuditKey inner join tblDirectory dins on dins.nDirKey = a.nInsertDirId inner join tblDirectory dupd on dupd.nDirKey = a.nUpdateDirId  where nContentKey = " & nContentId

                oDs = GetDataSet(sSqlContent, "Version", "ContentVersions")
                With oDs.Tables("Version")
                    .Columns("id").ColumnMapping = Data.MappingType.Attribute
                    .Columns("primaryId").ColumnMapping = Data.MappingType.Attribute
                    .Columns("version").ColumnMapping = Data.MappingType.Attribute
                    .Columns("ref").ColumnMapping = Data.MappingType.Attribute
                    .Columns("name").ColumnMapping = Data.MappingType.Attribute
                    .Columns("type").ColumnMapping = Data.MappingType.Attribute
                    .Columns("status").ColumnMapping = Data.MappingType.Attribute
                    .Columns("publish").ColumnMapping = Data.MappingType.Attribute
                    .Columns("expire").ColumnMapping = Data.MappingType.Attribute
                    .Columns("update").ColumnMapping = Data.MappingType.Attribute
                    .Columns("owner").ColumnMapping = Data.MappingType.Attribute
                    .Columns("updater").ColumnMapping = Data.MappingType.Attribute
                End With

                sSqlVersions = "" & _
                              "select c.nContentVersionKey as id,  c.nContentPrimaryId as primaryId,  c.nVersion as version,  cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, a.nStatus as status, a.dInsertDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], dins.cDirName as owner, dins.cDirXml as ownerDetail, dupd.cDirName as updater, dupd.cDirXml as updaterDetail " & _
                              "from tblContentVersions c inner join tblAudit a on c.nAuditId = a.nAuditKey inner join tblDirectory dins on dins.nDirKey = a.nInsertDirId inner join tblDirectory dupd on dupd.nDirKey = a.nUpdateDirId where nContentPrimaryId = " & nContentId & " ORDER BY c.nVersion DESC"

                oDs2 = GetDataSet(sSqlVersions, "Version", "ContentVersions")
                With oDs2.Tables("Version")
                    .Columns("id").ColumnMapping = Data.MappingType.Attribute
                    .Columns("primaryId").ColumnMapping = Data.MappingType.Attribute
                    .Columns("version").ColumnMapping = Data.MappingType.Attribute
                    .Columns("ref").ColumnMapping = Data.MappingType.Attribute
                    .Columns("name").ColumnMapping = Data.MappingType.Attribute
                    .Columns("type").ColumnMapping = Data.MappingType.Attribute
                    .Columns("status").ColumnMapping = Data.MappingType.Attribute
                    .Columns("publish").ColumnMapping = Data.MappingType.Attribute
                    .Columns("expire").ColumnMapping = Data.MappingType.Attribute
                    .Columns("update").ColumnMapping = Data.MappingType.Attribute
                    .Columns("owner").ColumnMapping = Data.MappingType.Attribute
                    .Columns("updater").ColumnMapping = Data.MappingType.Attribute
                End With

                oDs.Merge(oDs2)
                oRoot.InnerXml = oDs.GetXml

                'tidy the user details
                Dim oElmt As XmlElement

                For Each oElmt In oRoot.SelectNodes("ContentVersions/Version/ownerDetail | ContentVersions/Version/updaterDetail ")
                    oElmt.InnerXml = oElmt.InnerText
                Next

                Return oRoot.FirstChild

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getContentVersions", ex, cProcessInfo))
                Return Nothing
            End Try
        End Function

        Public Function getPageVersions(ByRef PageId As Long) As XmlElement
            PerfMon.Log("DBHelper", "getPageVersions")
            Dim oRoot As XmlElement
            Dim sSqlContent As String
            Dim sSqlVersions As String
            Dim oDs As DataSet
            Dim oDs2 As DataSet
            Dim ParPageId As Object

            Dim cProcessInfo As String = "getPageVersions"
            Try
                sSqlContent = "select nVersionParId from tblContentStructure where nStructKey = " & PageId
                ParPageId = GetDataValue(sSqlContent)
                If IsDBNull(ParPageId) Then
                    ParPageId = PageId
                ElseIf ParPageId = 0 Then
                    ParPageId = PageId
                End If

                oRoot = moPageXml.CreateElement("ContentDetail")

                sSqlContent = "" & _
                "select p.nStructKey as id,  p.nStructKey as primaryId,  p.cVersionDescription as description,  p.cStructForiegnRef as ref, cStructName as name, nVersionType as type, cVersionLang as lang, a.nStatus as status, a.dInsertDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], dins.cDirName as owner, dins.cDirXml as ownerDetail, dupd.cDirName as updater, dupd.cDirXml as updaterDetail,  dbo.fxn_getPageGroups(p.nStructKey) as Groups " & _
                "from tblContentStructure p inner join tblAudit a on p.nAuditId = a.nAuditKey LEFT OUTER JOIN  tblDirectory dins on dins.nDirKey = a.nInsertDirId LEFT OUTER JOIN tblDirectory dupd on dupd.nDirKey = a.nUpdateDirId  where nStructKey = " & ParPageId & " order by nVersionType, nStructOrder"

                oDs = GetDataSet(sSqlContent, "Version", "PageVersions")
                With oDs.Tables("Version")
                    .Columns("id").ColumnMapping = Data.MappingType.Attribute
                    .Columns("primaryId").ColumnMapping = Data.MappingType.Attribute
                    .Columns("description").ColumnMapping = Data.MappingType.Attribute
                    .Columns("ref").ColumnMapping = Data.MappingType.Attribute
                    .Columns("name").ColumnMapping = Data.MappingType.Attribute
                    .Columns("type").ColumnMapping = Data.MappingType.Attribute
                    .Columns("lang").ColumnMapping = Data.MappingType.Attribute
                    .Columns("status").ColumnMapping = Data.MappingType.Attribute
                    .Columns("publish").ColumnMapping = Data.MappingType.Attribute
                    .Columns("expire").ColumnMapping = Data.MappingType.Attribute
                    .Columns("update").ColumnMapping = Data.MappingType.Attribute
                    .Columns("owner").ColumnMapping = Data.MappingType.Attribute
                    .Columns("updater").ColumnMapping = Data.MappingType.Attribute
                    .Columns("groups").ColumnMapping = Data.MappingType.Attribute
                End With

                sSqlVersions = "" & _
                "select p.nStructKey as id,  p.nVersionParId as primaryId,  p.cVersionDescription as description,  p.cStructForiegnRef as ref, cStructName as name, nVersionType as type, cVersionLang as lang, a.nStatus as status, a.dInsertDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], dins.cDirName as owner, dins.cDirXml as ownerDetail, dupd.cDirName as updater, dupd.cDirXml as updaterDetail, dbo.fxn_getPageGroups(p.nStructKey) as Groups " & _
                "from tblContentStructure p inner join tblAudit a on p.nAuditId = a.nAuditKey inner join tblDirectory dins on dins.nDirKey = a.nInsertDirId inner join tblDirectory dupd on dupd.nDirKey = a.nUpdateDirId  where p.nVersionParId = " & ParPageId & " order by nVersionType, nStructOrder"
                oDs2 = GetDataSet(sSqlVersions, "Version", "PageVersions")
                With oDs2.Tables("Version")
                    .Columns("id").ColumnMapping = Data.MappingType.Attribute
                    .Columns("primaryId").ColumnMapping = Data.MappingType.Attribute
                    .Columns("description").ColumnMapping = Data.MappingType.Attribute
                    .Columns("ref").ColumnMapping = Data.MappingType.Attribute
                    .Columns("name").ColumnMapping = Data.MappingType.Attribute
                    .Columns("type").ColumnMapping = Data.MappingType.Attribute
                    .Columns("lang").ColumnMapping = Data.MappingType.Attribute
                    .Columns("status").ColumnMapping = Data.MappingType.Attribute
                    .Columns("publish").ColumnMapping = Data.MappingType.Attribute
                    .Columns("expire").ColumnMapping = Data.MappingType.Attribute
                    .Columns("update").ColumnMapping = Data.MappingType.Attribute
                    .Columns("owner").ColumnMapping = Data.MappingType.Attribute
                    .Columns("updater").ColumnMapping = Data.MappingType.Attribute
                    .Columns("groups").ColumnMapping = Data.MappingType.Attribute
                End With

                oDs2.Merge(oDs)
                oRoot.InnerXml = oDs2.GetXml

                'tidy the user details
                Dim oElmt As XmlElement

                For Each oElmt In oRoot.SelectNodes("PageVersions/Version/ownerDetail | PageVersions/Version/updaterDetail ")
                    oElmt.InnerXml = oElmt.InnerText
                Next

                For Each oElmt In oRoot.SelectNodes("PageVersions/Version")
                    If Not myWeb.goLangConfig Is Nothing Then
                        If oElmt.GetAttribute("lang") = myWeb.goLangConfig.GetAttribute("code") Or oElmt.GetAttribute("lang") = "" Then
                            oElmt.SetAttribute("langSystemName", myWeb.goLangConfig.GetAttribute("default"))
                        Else
                            Dim langElmt As XmlElement = myWeb.goLangConfig.SelectSingleNode("Language[@code='" & oElmt.GetAttribute("lang") & "']")
                            If Not langElmt Is Nothing Then
                                oElmt.SetAttribute("langSystemName", langElmt.GetAttribute("systemName"))
                            End If
                        End If
                    End If
                Next

                Return oRoot.FirstChild

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getContentVersions", ex, cProcessInfo))
                Return Nothing
            End Try
        End Function

        Public Function insertDirectory(ByVal cDirForiegnRef As String, ByVal cDirSchema As String, ByVal cDirName As String, ByVal cDirPassword As String, ByVal cDirXml As String, Optional ByVal nStatus As Integer = 1, Optional ByVal bOverwrite As Boolean = False) As Long
            PerfMon.Log("DBHelper", "insertDirectory")
            Dim sSql As String
            Dim nId As Long
            Dim cProcessInfo As String = ""
            'Dim oDr As SqlDataReader

            Try
                If cDirForiegnRef <> "" Then
                    nId = getObjectByRef(objectTypes.Directory, cDirForiegnRef, cDirSchema)
                End If

                'We should consider setting up encrypted passwords by default.

                cDirPassword = Eonic.Tools.Encryption.HashString(cDirPassword, goConfig("MembershipEncryption"), True)

                If nId < 1 Then
                    sSql = "Insert Into tblDirectory (cDirSchema, cDirForiegnRef, cDirName, cDirPassword, cDirXml, nAuditId)" & _
                    " values (" & _
                    "'" & SqlFmt(cDirSchema) & "'" & _
                    ",'" & SqlFmt(cDirForiegnRef) & "'" & _
                    ",'" & SqlFmt(cDirName) & "'" & _
                    ",'" & SqlFmt(cDirPassword) & "'" & _
                    ",'" & SqlFmt(cDirXml) & "'" & _
                    "," & getAuditId(nStatus) & ")"
                    nId = GetIdInsertSql(sSql)
                Else
                    If bOverwrite Then

                        sSql = "update tblDirectory set " & _
                        "cDirSchema = '" & SqlFmt(cDirSchema) & "'," & _
                        "cDirName = '" & SqlFmt(cDirName) & "'," & _
                        "cDirPassword = '" & SqlFmt(cDirPassword) & "'," & _
                        "cDirXML = '" & SqlFmt(cDirXml) & "'" & _
                        " where nDirKey = " & nId
                        ExeProcessSql(sSql)
                        ' insert code to update the audit table here
                    End If
                End If

                Return nId

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "insertUser", ex, cProcessInfo))

            End Try
        End Function

        Public Sub maintainDirectoryRelation(ByVal nParId As Long, ByVal nChildId As Long, Optional ByVal bRemove As Boolean = False, Optional ByVal dExpireDate As Object = Nothing, Optional ByVal bIfExistsDontUpdate As Boolean = False, Optional ByVal cEmail As String = Nothing, Optional ByVal cGroup As String = Nothing, Optional ByVal isLast As Boolean = True)
            Dim sSql As String
            Dim oDr As SqlDataReader
            Dim cProcessInfo As String = ""
            Dim nDelRelationId As Long = 0
            Dim oXml As XmlDocument
            Dim bHasChanged As Boolean = False
            PerfMon.Log(mcModuleName, "maintainMembershipsFromXForm", "start")
            Try
                If Not (nParId = 0 Or nChildId = 0) Then
                    'Does relationship exist?
                    sSql = "select * from tblDirectoryRelation where nDirParentId = " & nParId & " and nDirChildId  = " & nChildId
                    oDr = getDataReader(sSql)
                    If oDr.HasRows Then
                        'if so check bRemove and remove it if nessesary
                        If Not bRemove Then
                            ' If the permission exists, then we can update it - or we can opt to ignore the option of updating it
                            ' This is a db performance savign for some bulk routines.
                            If Not (bIfExistsDontUpdate) Then
                                While oDr.Read
                                    'update audit
                                    oXml = New XmlDocument
                                    If IsDate(dExpireDate) Then
                                        oXml.LoadXml("<instance><tblAudit><dExpireDate>" & Eonic.Tools.Xml.XmlDate(dExpireDate) & "</dExpireDate></tblAudit></instance>")
                                    Else
                                        'this should update the update date and user
                                        oXml.LoadXml("<instance><tblAudit/></instance>")
                                    End If
                                    setObjectInstance(objectTypes.Audit, oXml.DocumentElement, oDr("nAuditId"))
                                    oXml = Nothing
                                End While
                            End If
                        Else
                            While oDr.Read
                                DeleteObject(objectTypes.DirectoryRelation, oDr("nRelKey"))
                                bHasChanged = True
                            End While
                        End If
                        oDr.Close()
                        oDr = Nothing
                    Else
                        'if not create it
                        oDr.Close()
                        oDr = Nothing
                        If Not bRemove Then
                            'Dim nAuditId As String = ""
                            If IsDate(dExpireDate) Then
                                sSql = "insert into tblDirectoryRelation(nDirParentId, nDirChildId, nAuditId) values( " & nParId & ", " & nChildId & ", " & Me.getAuditId(, , , , dExpireDate) & ")"
                            Else
                                sSql = "insert into tblDirectoryRelation(nDirParentId, nDirChildId, nAuditId) values( " & nParId & ", " & nChildId & ", " & Me.getAuditId() & ")"
                            End If
                            ExeProcessSql(sSql)
                            bHasChanged = True
                        End If
                    End If

                    If bHasChanged Then
                        'Keep Mailing List In Sync.
                        ' If Not cEmail Is Nothing Then
                        Dim moMailConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("eonic/mailinglist")
                        Dim sMessagingProvider As String = ""
                        If Not moMailConfig Is Nothing Then
                            sMessagingProvider = moMailConfig("MessagingProvider")
                        End If
                        If moMessaging Is Nothing Then
                            moMessaging = New Eonic.Providers.Messaging.BaseProvider(myWeb, sMessagingProvider)
                        End If
                        If moMessaging IsNot Nothing AndAlso moMessaging.AdminProcess IsNot Nothing Then
                            Try
                                moMessaging.AdminProcess.maintainUserInGroup(nChildId, nParId, bRemove, cEmail, cGroup, isLast)
                            Catch ex As Exception
                                cProcessInfo = ex.StackTrace
                            End Try
                        End If
                        ' End If
                    End If

                End If

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "maintainDirectoryRelation", ex, cProcessInfo))
                'Close()
            End Try
        End Sub

        Public Sub maintainPermission(ByVal nPageId As Long, ByVal nDirId As Long, Optional ByVal nLevel As String = "1")
            PerfMon.Log("DBHelper", "maintainPermission")
            Dim sSql As String
            Dim oDr As SqlDataReader
            Dim cProcessInfo As String = ""

            Try

                'Does relationship exist?
                sSql = "select * from tblDirectoryPermission where nDirId = " & nDirId & " and nStructId  = " & nPageId
                oDr = getDataReader(sSql)
                If oDr.HasRows Then
                    'if so check bRemove and remove it if nessesary
                    While oDr.Read
                        'update audit
                        ' the permission level has changed... update it
                        If nLevel <> oDr("nAccessLevel") Then
                            sSql = "update tblDirectoryPermission set nAccessLevel = " & nLevel & " where nPermKey=" & oDr("nPermKey")
                            ExeProcessSql(sSql)
                        End If
                    End While
                    oDr.Close()
                    oDr = Nothing
                Else
                    'if not create it
                    oDr.Close()
                    oDr = Nothing

                    sSql = "insert into tblDirectoryPermission(nDirId, nStructId,nAccessLevel, nAuditId) values( " & nDirId & ", " & nPageId & "," & nLevel & " ," & Me.getAuditId & ")"
                    ExeProcessSql(sSql)
                End If

                'If nLevel = 0 Then
                '    sSql = "delete tblDirectoryPermission where nDirId = " & nDirId & " and nStructId  = " & nPageId
                '    exeProcessSQL(sSql)
                'End If

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "maintainDirectoryRelation", ex, cProcessInfo))

            End Try
        End Sub

        Public Overridable Function listDirectory(ByVal cSchemaName As String, Optional ByVal nParId As Long = 0, Optional ByVal nStatus As Integer = 99) As XmlElement
            PerfMon.Log("DBHelper", "listDirectory")
            Dim sSql As String
            Dim oDs As DataSet
            Dim oDr As SqlDataReader
            Dim oElmt As XmlElement
            Dim oElmt2 As XmlElement
            Dim oXml As XmlDataDocument

            Dim sContent As String

            Dim cProcessInfo As String = ""
            Try

                If goSession("oDirList") Is Nothing Or goSession("cDirListType") <> cSchemaName Then
                    Select Case cSchemaName
                        Case "User"
                            sSql = "execute spGetUsers"
                            If nParId <> 0 Then
                                sSql = sSql & " @nParDirId= " & nParId
                                If nStatus <> 99 Then
                                    sSql = sSql & ", @nStatus= " & nStatus
                                End If
                            Else
                                If nStatus <> 99 Then
                                    sSql = sSql & " @nStatus= " & nStatus
                                End If
                            End If

                            If goRequest("search") <> "" Then
                                sSql = "execute spSearchUsers @cSearch='" & goRequest("search") & "'"
                            End If
                        Case Else
                            sSql = "execute spGetDirectoryItems @cSchemaName = '" & cSchemaName & "'"
                            If nParId <> 0 Then
                                sSql = sSql & ", @nParDirId= " & nParId
                            End If
                    End Select

                    'DataSet Method
                    oDs = GetDataSet(sSql, LCase(cSchemaName), "directory")
                    ReturnNullsEmpty(oDs)

                    oDs.Tables(0).Columns(0).ColumnMapping = Data.MappingType.Attribute

                    oXml = New XmlDataDocument(oDs)
                    oDs.EnforceConstraints = False

                    'Convert any text to xml
                    Select Case cSchemaName
                        Case "User"
                            For Each oElmt2 In oXml.SelectNodes("descendant-or-self::UserXml")
                                sContent = oElmt2.InnerText
                                If sContent <> "" Then
                                    oElmt2.InnerXml = sContent
                                End If
                                oElmt2.ParentNode.ReplaceChild(oElmt2.FirstChild, oElmt2)
                            Next
                        Case Else
                            For Each oElmt2 In oXml.SelectNodes("descendant-or-self::Details")
                                sContent = oElmt2.InnerText
                                If sContent <> "" Then
                                    oElmt2.InnerXml = sContent
                                End If
                                oElmt2.ParentNode.ReplaceChild(oElmt2.FirstChild, oElmt2)
                            Next
                    End Select

                    goSession("sDirListType") = cSchemaName
                    goSession("oDirList") = oXml
                Else
                    oXml = goSession("sDirListType")
                End If
                oElmt = moPageXml.CreateElement("directory")

                If Not oXml.FirstChild Is Nothing Then
                    oElmt.InnerXml = oXml.FirstChild.InnerXml
                End If

                'let get the details of the parent object
                If nParId <> 0 Then
                    sSql = "select * from tblDirectory where nDirKey = " & nParId
                    oDr = getDataReader(sSql)
                    While oDr.Read
                        oElmt.SetAttribute("parId", nParId)
                        oElmt.SetAttribute("parType", oDr("cDirSchema"))
                        oElmt.SetAttribute("parName", oDr("cDirName"))
                    End While
                    oDr.Close()
                    oDr = Nothing
                Else
                    oElmt.SetAttribute("parType", cSchemaName)
                    oElmt.SetAttribute("parName", "All")
                End If
                oElmt.SetAttribute("itemType", cSchemaName)
                Select Case cSchemaName
                    Case "User"
                        oElmt.SetAttribute("displayName", "Users")
                    Case "Role"
                        oElmt.SetAttribute("displayName", "Roles")
                    Case "Group"
                        oElmt.SetAttribute("displayName", "Groups")
                    Case "Company"
                        oElmt.SetAttribute("displayName", "Companies")
                    Case "Department"
                        oElmt.SetAttribute("displayName", "Departments")
                    Case Else
                        oElmt.SetAttribute("displayName", cSchemaName & "s")
                End Select

                Return oElmt

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getUsers", ex, cProcessInfo))
                Return Nothing
            End Try

        End Function

        Public Function GetUserXML(ByVal nUserId As Long, Optional ByVal bIncludeContacts As Boolean = True) As XmlElement
            PerfMon.Log("DBHelper", "GetUserXML")

            Dim odr As SqlDataReader
            Dim root As XmlElement = Nothing
            Dim oElmt As XmlElement
            Dim sSql As String = ""
            Dim sProcessInfo As String = ""
            Dim PermLevel As PermissionLevel = PermissionLevel.Open
            Dim cOverrideUserGroups As String
            Dim cJoinType As String
            Try
                cOverrideUserGroups = goConfig("SearchAllUserGroups")
                If nUserId <> 0 Then
                    '

                    'If nPermLevel > 2 Then
                    '    myWeb.moPageXml.DocumentElement.SetAttribute("adminMode", getPermissionLevel(nPermLevel))
                    'End If
                    odr = getDataReader("SELECT * FROM tblDirectory where nDirKey = " & nUserId)
                    While odr.Read
                        root = moPageXml.CreateElement(odr("cDirSchema"))
                        root.SetAttribute("id", nUserId)
                        root.SetAttribute("name", odr("cDirName"))
                        root.SetAttribute("fRef", odr("cDirForiegnRef"))
                        'root.SetAttribute("permission", getPermissionLevel(nPermLevel))
                        If odr("cDirXml") <> "" Then
                            root.InnerXml = odr("cDirXml")
                            root.InnerXml = root.SelectSingleNode("*").InnerXml
                        End If
                        ' Ignore if myWeb is nothing
                        If Not (myWeb Is Nothing) Then
                            PermLevel = getPagePermissionLevel(myWeb.mnPageId)
                            root.SetAttribute("pagePermission", PermLevel.ToString)
                        End If
                    End While
                    odr.Close()
                    odr = Nothing

                    'get group memberships


                    If cOverrideUserGroups = "on" Then
                        cJoinType = "LEFT"
                    Else
                        cJoinType = "INNER"
                    End If

                    sSql = "SELECT	d.*," & _
                            " r.nRelKey As Member" & _
                            " FROM	tblDirectory d" & _
                            " " & cJoinType & " JOIN tblDirectoryRelation r" & _
                            " ON r.nDirParentid = d.nDirKey " & _
                            " AND r.nDirChildId = " & nUserId & _
                            " WHERE   d.cDirSchema <> 'User'" & _
                            " ORDER BY d.cDirName"

                    odr = getDataReader(sSql)
                    While odr.Read
                        oElmt = moPageXml.CreateElement(odr("cDirSchema"))
                        oElmt.SetAttribute("id", odr("nDirKey"))
                        oElmt.SetAttribute("name", odr("cDirName"))
                        If Not IsDBNull(odr("Member")) Then
                            oElmt.SetAttribute("isMember", "yes")
                        End If
                        oElmt.InnerXml = odr("cDirXml")
                        oElmt.InnerXml = oElmt.FirstChild.InnerXml
                        If Not cOverrideUserGroups = "on" Then
                            If Not IsDBNull(odr("Member")) Then
                                root.AppendChild(oElmt)
                            End If
                        Else
                            root.AppendChild(oElmt)
                        End If
                    End While
                    odr.Close()
                    odr = Nothing
                    If root Is Nothing Then
                        root = moPageXml.CreateElement("User")
                        root.SetAttribute("id", nUserId)
                        root.SetAttribute("old", "true()")
                    End If

                    If bIncludeContacts Then root.AppendChild(GetUserContactsXml(nUserId))
                    If goConfig("Subscriptions") = "on" And Not myWeb Is Nothing Then
                        Dim mySub As New Eonic.Web.Cart.Subscriptions(myWeb)
                        mySub.AddSubscriptionToUserXML(root, nUserId)
                    End If
                    'now we want to get the admin permissions for this page


                End If

                Return root

            Catch ex As Exception
                If sProcessInfo = "" Then sProcessInfo = sSql
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "GetUserXML", ex, sProcessInfo))
                Return Nothing
            End Try

        End Function


        Function GetUserContactsXml(ByVal nUserId As Integer) As XmlElement
            PerfMon.Log("DBHelper", "GetUserContactsXMl")
            Try
                Dim oContacts As XmlElement = moPageXml.CreateElement("Contacts")
                Dim cSQL As String = "SELECT * FROM tblCartContact where nContactCartId = 0 and nContactDirId = " & nUserId
                Dim oDS As New DataSet
                oDS = GetDataSet(cSQL, "Contact")
                Dim oDRow As DataRow
                Dim oDC As DataColumn
                For Each oDRow In oDS.Tables(0).Rows
                    Dim oContact As XmlElement = moPageXml.CreateElement("Contact")
                    For Each oDC In oDS.Tables(0).Columns
                        Dim oIElmt As XmlElement = moPageXml.CreateElement(oDC.ColumnName)

                        If Not IsDBNull(oDRow(oDC.ColumnName)) Then
                            Dim cStrContent As String = oDRow(oDC.ColumnName)
                            cStrContent = Replace(Replace(cStrContent, "&gt;", ">"), "&lt;", "<")
                            If Not cStrContent Is Nothing And Not cStrContent = "" Then oIElmt.InnerText = cStrContent

                        End If
                        oContact.AppendChild(oIElmt)
                    Next
                    oContacts.AppendChild(oContact)
                Next
                Return oContacts
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "GetUserContactsXMl", ex, ""))
                Return Nothing
            End Try
        End Function

        Function getDirParentId(ByVal nChildId As Long) As Long
            PerfMon.Log("DBHelper", "getDirParentId")
            'only to be used on departments because they can only have 1 company
            Dim sSql As String
            Dim cProcessInfo As String = ""
            Try
                sSql = "select d.nDirKey as id " & _
                "FROM tblDirectory d " & _
                "INNER JOIN tblDirectoryRelation dept2company " & _
                "ON d.nDirKey = dept2company.nDirParentId " & _
                "WHERE dept2company.nDirChildId=" & nChildId

                Return GetDataValue(sSql)

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getDirParentId", ex, cProcessInfo))
            End Try
        End Function

        Public Overridable Function listDirRelations(ByVal nChildId As Long, ByVal cSchemaName As String, Optional ByVal nParId As Long = 0) As XmlElement
            PerfMon.Log("DBHelper", "listDirRelations")
            Dim sSql As String
            Dim oDs As DataSet
            Dim oDr As SqlDataReader
            Dim oElmt As XmlElement
            Dim oElmt2 As XmlElement
            Dim cChildSchema As String = ""
            Dim oXml As XmlDataDocument
            Dim sSqlCompanyCol As String = ""
            Dim sSqlCompanyOrder As String = ""

            Dim sContent As String

            Dim cProcessInfo As String = ""
            Try

                'If goSession("oDirList") Is Nothing Or goSession("cDirListType") <> cSchemaName Then

                oElmt = moPageXml.CreateElement("directory")

                'let get the details of the child object
                If nChildId <> 0 Then
                    sSql = "select * from tblDirectory where nDirKey = " & nChildId
                    oDr = getDataReader(sSql)
                    If oDr.HasRows Then
                        While oDr.Read
                            oElmt.SetAttribute("childId", nChildId)
                            oElmt.SetAttribute("childType", oDr("cDirSchema"))
                            cChildSchema = oDr("cDirSchema")
                            oElmt.SetAttribute("childName", oDr("cDirName"))
                        End While
                    End If
                    oDr.Close()
                    oDr = Nothing
                Else
                    oElmt.SetAttribute("parType", cSchemaName)
                    oElmt.SetAttribute("parName", "All")
                End If

                oElmt.SetAttribute("itemType", cSchemaName)
                Select Case cSchemaName
                    Case "User"
                        oElmt.SetAttribute("displayName", "Users")
                    Case "Role"
                        oElmt.SetAttribute("displayName", "Roles")
                    Case "Group"
                        oElmt.SetAttribute("displayName", "Groups")
                    Case "Company"
                        oElmt.SetAttribute("displayName", "Companies")
                    Case "Department"
                        oElmt.SetAttribute("displayName", "Departments")
                    Case Else
                End Select

                If cSchemaName = "Department" And cChildSchema = "User" Then
                    'get only the departments which are in the users companies
                    'this is not required if the parent is not a user

                    sSql = "select d.nDirKey as id, d.cDirName as name, d.cDirXml as details, a.nStatus as status, dr.nDirChildId as related " & _
                    "FROM (((tblDirectory d " & _
                         "inner join tblAudit a on nAuditId = a.nAuditKey " & _
                         "INNER JOIN tblDirectoryRelation dept2company " & _
                              "ON d.nDirKey = dept2company.nDirChildId) " & _
                         "INNER JOIN tblDirectory company " & _
                              "ON company.nDirKey = dept2company.nDirParentId) " & _
                         "INNER JOIN tblDirectoryRelation user2company " & _
                              "ON company.nDirKey = user2company.nDirParentId) " & _
                         "INNER JOIN tblDirectory users " & _
                              "ON users.nDirKey = user2company.nDirChildId " & _
                         " left outer join tblDirectoryRelation dr on d.nDirKey = dr.nDirParentId and dr.nDirChildId = " & nChildId & _
                        "WHERE d.cDirSchema = 'Department' AND company.cDirSchema = 'Company' AND users.cDirSchema = 'User' " & _
                         "AND users.nDirKey=" & nChildId & " order by d.cDirName"
                Else
                    'if we are dealing with departments then lets list the companies they are in and sort by companyies first
                    If cSchemaName = "Department" Then
                        sSqlCompanyCol = "dbo.fxn_getUserCompanies(d.nDirKey) as Company,"
                        sSqlCompanyOrder = "Company, "
                    End If
                    If goRequest("relateAs") = "children" Then
                        sSql = "select d.nDirKey as id, " & sSqlCompanyCol & " d.cDirName as name, d.cDirXml as details, a.nStatus as status, dr.nDirChildId as related" _
                                & " from tblDirectory d" _
                                & " inner join tblAudit a on nAuditId = a.nAuditKey " _
                                & " left outer join tblDirectoryRelation dr on nDirKey = dr.nDirChildId and dr.nDirParentId = " & nChildId _
                                & " where cDirSchema = '" & cSchemaName & "' " _
                                & " and a.nStatus <> 0 order by " & sSqlCompanyOrder & " d.cDirName"

                    Else
                        sSql = "select d.nDirKey as id, d.cDirName as name, d.cDirXml as details, a.nStatus as status, dr.nDirChildId as related from tblDirectory d" _
                                & " inner join tblAudit a on nAuditId = a.nAuditKey " _
                                & " left outer join tblDirectoryRelation dr on nDirKey = dr.nDirParentId and dr.nDirChildId = " & nChildId _
                                & " where cDirSchema = '" & cSchemaName & "' " _
                                & " and a.nStatus <> 0 order by d.cDirName"
                    End If

                End If

                'DataSet Method

                oDs = GetDataSet(sSql, LCase(cSchemaName), "directory")
                ReturnNullsEmpty(oDs)
                If sSqlCompanyCol = "" Then
                    oDs.Tables(0).Columns(0).ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns(1).ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns(3).ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns(4).ColumnMapping = Data.MappingType.Attribute
                Else
                    oDs.Tables(0).Columns(0).ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns(1).ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns(2).ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns(4).ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns(5).ColumnMapping = Data.MappingType.Attribute
                End If


                oXml = New XmlDataDocument(oDs)
                oDs.EnforceConstraints = False

                'Convert any text to xml
                For Each oElmt2 In oXml.SelectNodes("descendant-or-self::details")
                    sContent = oElmt2.InnerText
                    If sContent <> "" Then
                        oElmt2.ParentNode.InnerXml = sContent
                    End If
                Next

                'goSession("sDirListType") = cSchemaName
                'goSession("oDirList") = oXml
                'Else
                '    oXml = goSession("sDirListType")
                'End If

                If Not oXml.FirstChild Is Nothing Then
                    oElmt.InnerXml = oXml.FirstChild.InnerXml
                End If



                Return oElmt

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getUsers", ex, cProcessInfo))
                Return Nothing
            End Try

        End Function

        Sub saveDirectoryRelations(Optional ByVal nChildId As Long = 0, Optional ByVal sParIds As String = "", Optional ByVal bRemove As Boolean = False, Optional ByVal relateAs As RelationType = RelationType.Parent, Optional ByVal bIfExistsDontUpdate As Boolean = False)

            Dim aParId() As String = Nothing
            Dim nParId As Long
            Dim i As Long
            PerfMon.Log("DBHelper", "saveDirectoryRelations", "Start")
            Dim cProcessInfo As String = ""
            Try
                If nChildId = 0 Then
                    nChildId = CLng(myWeb.moRequest("childId"))
                End If


                If sParIds = "" Then
                    If Not (myWeb.moRequest("parentList")) Is Nothing Then
                        aParId = CStr(myWeb.moRequest("parentList")).Split(",")
                    End If
                Else
                    aParId = CStr(sParIds).Split(",")
                End If

                If Not (aParId Is Nothing) Then
                    For i = 0 To UBound(aParId)
                        If IsNumeric(aParId(i)) Then
                            nParId = CLng(aParId(i))
                            If myWeb.moRequest("relateAs") = "children" Or relateAs = RelationType.Child Then
                                'OK we are reversing the way this works and relating as children rather than parents.
                                'this is done for group memberships where companyies, departments etc. are children of groups.
                                If sParIds = "" Then
                                    'handle the checkbox form
                                    If myWeb.moRequest("rel_" & nParId) <> "" Then
                                        Me.maintainDirectoryRelation(nChildId, nParId, , , bIfExistsDontUpdate)
                                    Else
                                        Me.maintainDirectoryRelation(nChildId, nParId, True, , bIfExistsDontUpdate)
                                    End If
                                Else
                                    If bRemove = False Then
                                        Me.maintainDirectoryRelation(nChildId, nParId, , , bIfExistsDontUpdate)
                                    Else
                                        Me.maintainDirectoryRelation(nChildId, nParId, True, , bIfExistsDontUpdate)
                                    End If
                                End If
                            Else
                                If sParIds = "" Then
                                    'handle the checkbox form
                                    If myWeb.moRequest("rel_" & nParId) <> "" Then
                                        Me.maintainDirectoryRelation(nParId, nChildId, , , bIfExistsDontUpdate)
                                    Else
                                        Me.maintainDirectoryRelation(nParId, nChildId, True, , bIfExistsDontUpdate)
                                    End If
                                Else
                                    If bRemove = False Then
                                        Me.maintainDirectoryRelation(nParId, nChildId, , , bIfExistsDontUpdate)
                                    Else
                                        Me.maintainDirectoryRelation(nParId, nChildId, True, , bIfExistsDontUpdate)
                                    End If
                                End If
                            End If
                        End If
                    Next
                End If

                PerfMon.Log("DBHelper", "saveDirectoryRelations", "End" & i)

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "saveDirectoryRelations", ex, cProcessInfo))
            End Try

        End Sub


        Sub moveDirectoryRelations(ByVal nSourceDirId As Long, ByVal nTargetDirId As Long, Optional ByVal bDeleteSource As Boolean = False)

            Dim sSql As String = ""
            Dim oDr As SqlDataReader
            Dim cProcessInfo As String = ""
            Try

                'Transfer all the relations to another department
                sSql = "select nRelKey, nDirChildId from tblDirectoryRelation where nDirParentId = " & nSourceDirId
                oDr = getDataReader(sSql)
                While oDr.Read
                    'first we remove
                    DeleteObject(dbHelper.objectTypes.DirectoryRelation, oDr("nRelKey"))
                    'then we insert new
                    saveDirectoryRelations(oDr("nDirChildId"), nTargetDirId, False, , True)
                End While
                oDr.Close()
                oDr = Nothing

                'Delete Department Directory Relations, Department Permissions and Department

                If bDeleteSource Then
                    DeleteObject(dbHelper.objectTypes.Directory, nSourceDirId)
                End If

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "moveDirectoryRelations", ex, cProcessInfo))
            End Try

        End Sub

        Sub deleteChildDirectoryRelations(ByVal nDirId As Long)

            Dim sSql As String = ""
            Dim oDr As SqlDataReader
            Dim cProcessInfo As String = ""
            Try

                'remove all child relations so child objects don't get deleted
                sSql = "select nRelKey from tblDirectoryRelation where nDirParentId = " & nDirId
                oDr = getDataReader(sSql)
                While oDr.Read
                    DeleteObject(dbHelper.objectTypes.DirectoryRelation, oDr(0))
                End While
                oDr.Close()
                oDr = Nothing

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "moveDirectoryRelations", ex, cProcessInfo))
            End Try

        End Sub


        Sub saveDirectoryPermissions()
            PerfMon.Log("DBHelper", "saveDirectoryPermissions")
            Dim nParId As Long
            Dim nPageId As String



            Dim cProcessInfo As String = ""
            Try

                nParId = CLng(goRequest("parId"))
                Dim item As Object

                For Each item In goRequest.Form
                    If InStr(item, "page_") > 0 Then
                        nPageId = CLng(Replace(item, "page_", ""))
                        Select Case goRequest(item)
                            Case "permit"
                                savePermissions(nPageId, goRequest("parId"), PermissionLevel.View)
                            Case "deny"
                                savePermissions(nPageId, goRequest("parId"), PermissionLevel.Denied)
                            Case "remove"
                                savePermissions(nPageId, goRequest("parId"), PermissionLevel.Open)
                        End Select
                    End If
                Next


            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "saveDirectoryPermissions", ex, cProcessInfo))
            End Try

        End Sub

        Sub savePermissions(ByVal nPageId As Long, ByVal csDirId As String, Optional ByVal nLevel As PermissionLevel = PermissionLevel.View)
            PerfMon.Log("DBHelper", "savePermissions")
            Dim aDirId() As String
            Dim nDirId As Long
            Dim i As Long
            Dim sSql As String
            Dim oDr As SqlDataReader

            Dim cProcessInfo As String = ""
            Try

                If csDirId <> "" Then

                    aDirId = Split(csDirId, ",")

                    For i = 0 To UBound(aDirId)
                        If aDirId(i) <> "" Then
                            nDirId = CLng(aDirId(i))
                            If nLevel <> PermissionLevel.Open Then
                                Me.maintainPermission(nPageId, nDirId, nLevel)
                            Else
                                sSql = "select * from tblDirectoryPermission where nStructId = " & nPageId & " and nDirId=" & nDirId
                                oDr = getDataReader(sSql)
                                While oDr.Read
                                    DeleteObject(objectTypes.Permission, oDr("nPermKey"))
                                End While
                                oDr.Close()
                                oDr = Nothing
                            End If

                        End If
                    Next
                Else
                    'reset all permissions for page
                    Select Case nLevel
                        Case PermissionLevel.Open
                            sSql = "select * from tblDirectoryPermission where nStructId = " & nPageId
                            oDr = getDataReader(sSql)
                            While oDr.Read
                                DeleteObject(objectTypes.Permission, oDr("nPermKey"))
                            End While
                            oDr.Close()
                            oDr = Nothing

                        Case Else
                            'do nothing why would you want to reset all permissions to the same level?
                    End Select

                End If

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "saveDirectoryRelations", ex, cProcessInfo))
            End Try

        End Sub

        Sub clearDirectoryCache()
            PerfMon.Log("DBHelper", "clearDirectoryCache")
            Dim cProcessInfo As String = ""
            Try

                goSession("sDirListType") = Nothing
                goSession("oDirList") = Nothing

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "clearDirectoryCache", ex, cProcessInfo))
            End Try
        End Sub

        Public Overridable Sub clearStructureCacheUser()

            Dim sSql As String = ""
            Try
                If checkDBObjectExists("tblXmlCache", Tools.Database.objectTypes.Table) Then
                    If myWeb.mnUserId > 0 Then
                        ' user id exists
                        sSql = "DELETE FROM dbo.tblXmlCache " _
                            & " WHERE nCacheDirId = " & Eonic.SqlFmt(myWeb.mnUserId)
                    Else
                        ' No user id - delete the cache based on session id.
                        sSql = "DELETE FROM dbo.tblXmlCache " _
                            & " WHERE cCacheSessionID = '" & SqlFmt(myWeb.moSession.SessionID.ToString) & "'"
                    End If
                    MyBase.ExeProcessSql(sSql)
                End If
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "clearStructureCache", ex, sSql))
            End Try

        End Sub

        Public Overridable Sub clearStructureCacheAll()

            Dim sSql As String = ""
            Try
                If checkDBObjectExists("tblXmlCache", Tools.Database.objectTypes.Table) Then
                    ' user id exists
                    sSql = "DELETE FROM dbo.tblXmlCache "
                    'clear from app level too
                    myWeb.goApp("AdminStructureCache") = Nothing


                    MyBase.ExeProcessSql(sSql)
                End If
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "clearStructureCache", ex, sSql))
            End Try

        End Sub

        Public Sub addStructureCache(ByVal bAuth As Boolean, ByVal nUserId As Long, ByRef cCacheType As String, ByRef StructureXml As XmlElement)
            Dim sSql As String = ""


            Try
                If checkDBObjectExists("tblXmlCache", Tools.Database.objectTypes.Table) Then

                    'OPTION 1 - Insert using dataset update because insert statement vSlow.
                    'Dim oDs As DataSet
                    'Dim oRow As DataRow
                    'sSql = "select * from tblXmlCache where nCacheKey = 0"
                    'oDs = getDataSetForUpdate(sSql, "tblXmlCache", "Cache")
                    'If oDs.Tables("tblXmlCache").Rows.Count = 0 Then
                    '    oRow = oDs.Tables("tblXmlCache").NewRow()
                    '    oRow("cCacheType") = cCacheType
                    '    oRow("cCacheSessionId") = IIf(bAuth, Eonic.SqlFmt(goSession.SessionID), "")
                    '    oRow("nCacheDirId") = mnUserId
                    '    oRow("cCacheStructure") = StructureXml.OuterXml 'New XmlNodeReader(StructureXml)
                    '    oDs.Tables("tblXmlCache").Rows.Add(oRow)
                    'End If
                    'updateDataset(oDs, "tblXmlCache", False)

                    'OPTION 2 - Insert using parameter Also slow
                    Dim nUpdateCount As String
                    sSql = "INSERT INTO dbo.tblXmlCache (cCacheSessionID,nCacheDirId,cCacheStructure,cCacheType) " _
                                               & "VALUES (" _
                                               & "'" & IIf(bAuth, Eonic.SqlFmt(goSession.SessionID), "") & "'," _
                                               & Eonic.SqlFmt(nUserId) & "," _
                                               & " @XmlValue," _
                                               & "'" & cCacheType & "'" _
                                               & ")"

                    Dim oCmd As New SqlCommand(sSql, oConn)

                    If oConn.State = ConnectionState.Closed Then oConn.Open()

                    Dim param As New SqlParameter("@XmlValue", SqlDbType.Xml)
                    param.Direction = ParameterDirection.Input
                    param.Value = New XmlNodeReader(StructureXml) 'StructureXml.OuterXml '
                    param.Size = StructureXml.OuterXml.Length
                    oCmd.Parameters.Add(param)

                    nUpdateCount = oCmd.ExecuteNonQuery

                    'OPTION 3 User SQLBULKCOPY because supposed to be fast

                    'Dim dt As New DataTable()
                    'dt.Columns.Add("nCacheKey", Type.GetType("System.Int64"))
                    'dt.Columns.Add("cCacheType", Type.GetType("System.String"))
                    'dt.Columns.Add("cCacheSessionID", Type.GetType("System.String"))
                    'dt.Columns.Add("nCacheDirId", Type.GetType("System.Int64"))
                    'dt.Columns.Add("cCacheDate", Type.GetType("System.DateTime"))
                    ''dt.Columns.Add("cCacheStructure", Type.GetType("System.Byte[]"))
                    'dt.Columns.Add("cCacheStructure", Type.GetType("System.String"))
                    'Dim oRow As DataRow = dt.NewRow
                    'oRow("cCacheSessionID") = IIf(bAuth, Eonic.SqlFmt(goSession.SessionID), "")
                    'oRow("nCacheDirId") = nUserId
                    ''oRow("cCacheStructure") = Text.Encoding.Default.GetBytes(StructureXml.OuterXml)
                    'oRow("cCacheStructure") = StructureXml.OuterXml
                    'oRow("cCacheType") = cCacheType
                    'dt.Rows.Add(oRow)

                    'Dim oCopy As New SqlBulkCopy(oConn)
                    'If oConn.State = ConnectionState.Closed Then oConn.Open()
                    'oCopy.DestinationTableName = "dbo.tblXmlCache"
                    'oCopy.WriteToServer(dt)

                    '  ExeProcessSql(sSql)

                End If
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "addStructureCache", ex, sSql))
            End Try
        End Sub
        Public Overridable Function validateUser(ByVal nUserId As Long, ByVal cPassword As String) As String
            Return validateUser(nUserId, "", cPassword)
        End Function

        Public Overridable Function validateUser(ByVal cUsername As String, ByVal cPassword As String) As String
            Return validateUser(0, cUsername, cPassword)
        End Function


        Private Function validateUser(ByVal nUserId As Long, ByVal cUsername As String, ByVal cPasswordForm As String) As String

            ' Username can be an email address if specified in the web config setting.

            PerfMon.Log("DBHelper", "validateUser")
            Dim sSql As String = ""
            'Dim oDr As SqlDataReader
            Dim dsUsers As DataSet
            Dim oUserDetails As DataRow
            Dim cPasswordDatabase As String = ""
            Dim sReturn As String = ""
            Dim cProcessInfo As String = ""
            Dim ADPassword As String = cPasswordForm
            Dim bValidPassword As Boolean = False
            Dim oImp As Eonic.Tools.Security.Impersonate
            Dim isValidEmailAddress As Boolean
            Dim areEmailAddressesAllowed As Boolean
            Dim nNumberOfUsers As Integer

            Try

                'Does the configuration setting indicate that email addresses are allowed.
                If LCase(myWeb.moConfig("EmailUsernames")) = "on" Then
                    areEmailAddressesAllowed = True
                Else
                    areEmailAddressesAllowed = False
                End If

                'Returns true if cUsername is a valid email address.
                isValidEmailAddress = Tools.Xml.EmailAddressCheck(cUsername)

                'first we validate against active directory
                If checkDBObjectExists("tblDirectory", Tools.Database.objectTypes.Table) Then

                    'If validating the user based on their User Id
                    If nUserId > 0 Then
                        sSql = "select d.*, a.* from tblDirectory d inner join tblAudit a on a.nAuditkey = nAuditId where " & _
                    "cDirSchema = 'User' and nDirKey = " & nUserId
                        sReturn = "<span class=""msg-1023"">The user Id is does not exist. Please contact your administrator.</span>"
                    Else
                        'If the config setting indicates that email addresses are allowed and it is a valid email address.
                        If areEmailAddressesAllowed = True And isValidEmailAddress = True Then
                            'Log in using an email address.
                            'If validating the user by their username which can be an email address.
                            sSql = "select d.*, a.* from tblDirectory d inner join tblAudit a on a.nAuditkey = nAuditId where " & _
                        "cDirSchema = 'User' and cDirEmail = '" & SqlFmt(cUsername) & "'"
                            sReturn = "<span class=""msg-1024"">The email address was not found. Please contact your administrator.</span>"
                        ElseIf areEmailAddressesAllowed = True And isValidEmailAddress = False Then
                            'It is not an email address so try to log in using a user name.
                            'If validating the user by their username which can be an email address.
                            sSql = "select d.*, a.* from tblDirectory d inner join tblAudit a on a.nAuditkey = nAuditId where " & _
                        "cDirSchema = 'User' and cDirName = '" & SqlFmt(cUsername) & "'"
                            sReturn = "<span class=""msg-1015"">These credentials do not match a valid account</span>"
                        Else
                            'Log in using a user name.
                            'If validating the user by their username which can be an email address.
                            sSql = "select d.*, a.* from tblDirectory d inner join tblAudit a on a.nAuditkey = nAuditId where " & _
                        "cDirSchema = 'User' and cDirName = '" & SqlFmt(cUsername) & "'"
                            sReturn = "<span class=""msg-1015"">These credentials do not match a valid account</span>"
                        End If
                    End If

                    dsUsers = GetDataSet(sSql, "tblTemp")
                    nNumberOfUsers = dsUsers.Tables(0).Rows.Count

                    If nNumberOfUsers = 0 Then
                        sReturn = sReturn ' "<span class=""msg-1015"">The username was not found</span>"
                        'Return sReturn
                    ElseIf nNumberOfUsers > 1 Then
                        sReturn = "<span class=""msg-1025"">Sorry that email address is ambiguous. Please use your username instead.</span>"
                        'Return sReturn
                    Else
                        oUserDetails = dsUsers.Tables(0).Rows(0)
                        cPasswordDatabase = oUserDetails("cDirPassword")
                        nUserId = oUserDetails("nDirKey")

                        If Not LCase(myWeb.moConfig("MembershipEncryption")) = "plain" And Not myWeb.moConfig("MembershipEncryption") = "" Then
                            cPasswordForm = Eonic.Tools.Encryption.HashString(cPasswordForm, LCase(myWeb.moConfig("MembershipEncryption")), True) 'plain - md5 - sha1

                            If LCase(myWeb.moConfig("MembershipEncryption")) = "md5salt" Then
                                ' we need password from the database, as this has the salt in format: hashedpassword:salt
                                Dim arrPasswordFromDatabase As String() = Split(cPasswordDatabase, ":")
                                If arrPasswordFromDatabase.Length = 2 Then
                                    'RJP 7 Nov 2012. Note leave the md5 hard coded in the line below.
                                    If arrPasswordFromDatabase(0) = Eonic.Tools.Encryption.HashString(arrPasswordFromDatabase(1) + ADPassword, "md5", True) Then
                                        bValidPassword = True
                                    End If
                                End If

                            Else
                                Dim oConvDoc As New XmlDocument
                                Dim oConvElmt As XmlElement = oConvDoc.CreateElement("PW")
                                oConvElmt.InnerText = cPasswordForm
                                cPasswordForm = oConvElmt.InnerXml
                                cPasswordForm = Replace(cPasswordForm, "&gt;", ">")
                                cPasswordForm = Replace(cPasswordForm, "&lt;", "<")
                                If cPasswordDatabase = cPasswordForm Then bValidPassword = True

                            End If
                        Else
                            If cPasswordDatabase = cPasswordForm Then bValidPassword = True
                        End If

                        If bValidPassword = True Then

                            Select Case dsUsers.Tables(0).Rows.Count

                                Case 0
                                    sReturn = "<span class=""msg-1015"">These credentials do not match a valid account</span>"

                                Case 1
                                    sReturn = oUserDetails("nDirKey")

                                    'Check user dates
                                    If IsDate(oUserDetails("dExpireDate")) Then
                                        If oUserDetails("dExpireDate") < Now() Then
                                            sReturn = "<span class=""msg-1016"">User account has expired</span>"
                                        End If
                                    End If
                                    If IsDate(oUserDetails("dPublishDate")) Then
                                        If oUserDetails("dPublishDate") >= Now() Then
                                            sReturn = "<span class=""msg-1012"">User account is not active</span>"
                                        End If
                                    End If
                                    'Check user status
                                    If Not (oUserDetails("nStatus") = 1 Or oUserDetails("nStatus") = -1) Then
                                        sReturn = "<span class=""msg-1013"">User account has been disabled</span>"
                                    End If

                                Case Else
                                    'RJP 7 Nov 2012. Modified the message below to reflect the problem encountered.
                                    sReturn = "<span class=""msg-1033"">Could not identify the account from the details supplied. Please contact the System Administrator.</span>"
                            End Select

                        Else
                            Dim moPolicy As XmlElement = WebConfigurationManager.GetWebApplicationSection("eonic/PasswordPolicy")
                            Dim retryMsg As String = ""
                            If Not moPolicy Is Nothing Then
                                Dim nRetrys As Integer = CInt("0" & moPolicy.FirstChild.SelectSingleNode("retrys").InnerText)
                                If nRetrys > 0 Then
                                    myWeb.moDbHelper.logActivity(dbHelper.ActivityType.LogonInvalidPassword, nUserId, 0, , cPasswordForm)

                                    Dim sSql2 As String = "select count(nActivityKey) from tblActivityLog where nActivityType=" & dbHelper.ActivityType.LogonInvalidPassword & " and nUserDirId = " & nUserId
                                    Dim earlierTries As Integer = MyBase.ExeProcessSqlScalar(sSql2)
                                    If earlierTries >= nRetrys Then
                                        Dim oMembershipProv As New Providers.Membership.BaseProvider(myWeb, myWeb.moConfig("MembershipProvider"))

                                        oMembershipProv.Activities.ResetUserAcct(myWeb, nUserId)

                                        sReturn = "<span class=""msg-1032"">Your account is blocked, an email has been sent to you to reset your password.</span>"

                                    Else
                                        Dim trysLeft As Integer = nRetrys - earlierTries
                                        retryMsg = "<br/><span class=""msg-1031"">You have <span class=""retryCount"">" & trysLeft & "</span> attempts before you account is blocked</span>"
                                        sReturn = "<span class=""msg-1014"">The Password is not valid</span>" & retryMsg
                                    End If
                                Else
                                    sReturn = "<span class=""msg-1015"">These credentials do not match a valid account</span>"
                                End If
                            Else
                                sReturn = "<span class=""msg-1014"">The Password is not valid</span>" & retryMsg
                            End If


                        End If


                    End If

                    ' If we get to here and have passed all the validation than sReturn will have been set to a userId and therefore numeric.

                    'check AD Login
                    If Not IsNumeric(sReturn) And cUsername <> "" Then
                        oImp = New Eonic.Tools.Security.Impersonate
                        If oImp.ImpersonateValidUser(cUsername, goConfig("AdminDomain"), ADPassword, True, goConfig("AdminGroup")) Then
                            sReturn = 1
                            'RJP 7 Nov 2012. Amended to use Lower Case to prevent against case sensitive entries in Eonic.Web.Config.
                            myWeb.moSession("ewAuth") = Eonic.Tools.Encryption.HashString(myWeb.moSession.SessionID & goConfig("AdminPassword"), LCase(myWeb.moConfig("MembershipEncryption")), True)

                        End If
                    End If

                    'check Single User Login
                    If gbSingleLoginSessionPerUser AndAlso IsNumeric(sReturn) AndAlso cUsername <> "" Then

                        ' Find the latest activity for this user within a timeout period - if it isn't logoff then flag up an error
                        Dim lastSeenActivityQuery As String = "" _
                        & "SELECT TOP 1 nACtivityType FROM tblActivityLog l " _
                        & "WHERE nUserDirId = " & sReturn.ToString & " " _
                        & "AND DATEDIFF(s,l.dDateTime,GETDATE()) < " & gnSingleLoginSessionTimeout.ToString & " " _
                        & "ORDER BY dDateTime DESC "

                        Dim lastSeenActivity As Integer = GetDataValue(lastSeenActivityQuery, , , ActivityType.Logoff)
                        If lastSeenActivity <> ActivityType.Logoff Then
                            sReturn = "<span class=""msg-9017"">This username is currently logged on.  Please wait for them to log off or try another username.</span>"
                        End If

                    End If

                    If IsNumeric(sReturn) Then
                        'delete failed logon attempts record
                        Dim sSql2 As String = "delete from tblActivityLog where nActivityType = " & Web.dbHelper.ActivityType.LogonInvalidPassword & " and nUserDirId=" & sReturn
                        myWeb.moDbHelper.ExeProcessSql(sSql2)
                    End If

                    Return sReturn
                Else
                    'table doesn't exist
                    If cUsername <> "" Then
                        oImp = New Eonic.Tools.Security.Impersonate
                        If oImp.ImpersonateValidUser(cUsername, goConfig("AdminDomain"), ADPassword, True, goConfig("AdminGroup")) Then
                            Return 1
                            'RJP 7 Nov 2012. Amended to use Lower Case to prevent against case sensitive entries in Eonic.Web.Config.
                            myWeb.moSession("ewAuth") = Eonic.Tools.Encryption.HashString(myWeb.moSession.SessionID & goConfig("AdminPassword"), LCase(myWeb.moConfig("MembershipEncryption")), True)
                        Else
                            Return Nothing
                        End If
                    End If
                End If

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "validateUser", ex, cProcessInfo))
                Return Nothing
            End Try
        End Function

        Public Overridable Function GetUserIDFromEmail(ByVal cEmail As String, Optional ByVal bIncludeInactive As Boolean = False) As Integer

            PerfMon.Log("DbHelper", "GetUserIDFromEmail")

            Dim cSql As String = ""
            Dim nReturnId As Integer = -1

            Try
                cEmail = Trim(cEmail)
                If Eonic.Tools.Text.IsEmail(cEmail) Then
                    ' This assumes that e-mail addresses are unique, but in case they're not we'll select
                    ' the first active user that has been most recently updated
                    cSql = "SELECT TOP 1 nDirKey " _
                            & "FROM tblDirectory  " _
                            & "INNER JOIN tblAudit ON nauditid = nauditkey  " _
                            & "WHERE cDirXml LIKE ('%<Email>' + @email + '</Email>%') " _
                            & "AND cDirSchema='User'  " _
                            & IIf(bIncludeInactive, " ", "AND nStatus<>0 ") _
                            & "ORDER BY ABS(nStatus) DESC, dUpdateDate DESC "
                    nReturnId = GetDataValue(cSql, , Eonic.Tools.Dictionary.getSimpleHashTable("email:" & SqlFmt(cEmail)), -1)
                End If

                Return nReturnId

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "GetUserIDFromEmail", ex, cSql))
                Return nReturnId
            End Try
        End Function


        Public Function IsValidUser(ByVal nUserId As Integer) As Boolean
            PerfMon.Log("DbHelper", "IsValidUser")

            Dim cSql As String = ""
            Dim cIsValidUser As Boolean = False

            Try

                cSql = "SELECT nDirKey " _
                        & "FROM tblDirectory  " _
                        & "INNER JOIN tblAudit ON nauditid = nauditkey  " _
                        & "WHERE nDirKey = " & SqlFmt(nUserId.ToString) & " " _
                        & "AND cDirSchema='User'  " _
                        & "AND nStatus <> 0 " _
                        & "AND (dPublishDate is null or dPublishDate = 0 or dPublishDate <= " & Eonic.Tools.Database.SqlDate(Now) & " ) " _
                        & "AND (dExpireDate is null or dExpireDate = 0 or dExpireDate >= " & Eonic.Tools.Database.SqlDate(Now) & " ) "

                cIsValidUser = (Me.GetDataValue(cSql, , , -1) <> -1)
                Return cIsValidUser

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "IsValidUser", ex, cSql))
                Return cIsValidUser
            End Try
        End Function


        Public Function DirParentByType(ByVal nChildId As Integer, ByVal nParentType As String) As Integer
            Try
                Dim cReturn As Integer
                Dim cSQL As String = "SELECT top 1 tblDirectory.nDirKey FROM tblDirectory RIGHT OUTER JOIN tblDirectoryRelation ON tblDirectory.nDirKey = tblDirectoryRelation.nDirParentId"
                cSQL &= " WHERE tblDirectory.cDirSchema = '" & nParentType & "' AND tblDirectoryRelation.nDirChildId = " & nChildId
                cReturn = ExeProcessSqlScalar(cSQL)
                If IsNumeric(cReturn) Then Return cReturn Else Return 0
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "DirParentByType", ex, ""))
            End Try
        End Function

        Public Function getDirectoryParentsByType(ByVal nId As Long, ByVal oParentType As DirectoryType) As XmlElement
            PerfMon.Log("DBHelper", "getDirectoryParentsByType")
            Dim oDs As DataSet
            Dim oXml As XmlDocument
            Dim cSql As String
            Dim sContent As String

            Dim oElmt As XmlElement = moPageXml.CreateElement("directory")
            Dim oElmt2 As XmlElement
            Dim cDirectoryType() As String = New String() {"User", "Department", "Company", "Group", "Role"}

            cSql = "exec dbo.spGetDirParentByType @nDirId = " & nId & ", @cParentType = '" & cDirectoryType(oParentType) & "'"

            oDs = GetDataSet(cSql, "item", "directory")
            ReturnNullsEmpty(oDs)

            oXml = New XmlDataDocument(oDs)
            oDs.EnforceConstraints = False


            For Each oElmt2 In oXml.SelectNodes("descendant-or-self::cDirXml")
                sContent = oElmt2.InnerText
                If sContent <> "" Then
                    oElmt2.InnerXml = sContent
                End If
            Next

            If Not oXml.FirstChild Is Nothing Then
                oElmt.InnerXml = oXml.FirstChild.InnerXml
            End If

            Return oElmt

        End Function

        Public Function passwordReminder(ByVal cEmail As String) As String

            PerfMon.Log("DBHelper", "passwordReminder")
            Dim sSql As String
            Dim oDr As SqlDataReader
            Dim sReturn As String = ""
            Dim cProcessInfo As String = ""
            Dim bValid As Boolean

            Dim sProjectPath As String = goConfig("ProjectPath") & ""
            Dim sSenderName As String = goConfig("SiteAdminName")
            Dim sSenderEmail As String = goConfig("SiteAdminEmail")


            Try
                sSql = "select * from tblDirectory where cDirSchema = 'User' and cDirXml like '%<Email>" & LCase(cEmail) & "</Email>%'"

                oDr = getDataReader(sSql)
                If oDr.HasRows Then
                    While oDr.Read
                        Dim oXmlDetails As XmlDataDocument = New XmlDataDocument
                        oXmlDetails.LoadXml(Me.GetUserXML(oDr("nDirKey"), False).OuterXml)

                        'lets add the saved password to the xml
                        Dim oElmtPwd As XmlElement = oXmlDetails.CreateElement("Password")
                        oElmtPwd.InnerText = oDr("cDirPassword")
                        oXmlDetails.SelectSingleNode("User").AppendChild(oElmtPwd)






                        'now lets send the email
                        Dim oMsg As Messaging = New Messaging



                        ' Set the language
                        If moPageXml IsNot Nothing _
                            AndAlso moPageXml.DocumentElement IsNot Nothing _
                            AndAlso moPageXml.DocumentElement.HasAttribute("translang") Then
                            oMsg.Language = moPageXml.DocumentElement.GetAttribute("translang")
                        End If




                        Try
                            Dim fsHelper As New Eonic.fsHelper
                            Dim filePath As String = fsHelper.checkCommonFilePath("/xsl/email/passwordReminder.xsl")

                            sReturn = oMsg.emailer(oXmlDetails.DocumentElement, goConfig("ProjectPath") & filePath _
                                                , sSenderName _
                                                , sSenderEmail _
                                                , cEmail _
                                                , "Password Reminder" _
                                                , "Your password has been emailed to you")
                            bValid = True
                        Catch ex As Exception
                            sReturn = "Your email failed to send from password reminder"
                            bValid = False
                        End Try
                        oXmlDetails = Nothing

                    End While
                Else
                    sReturn = "This user was not found"
                End If

                oDr.Close()
                oDr = Nothing




                Return sReturn

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "passwordReminder", ex, cProcessInfo))

                Return Nothing
            End Try
        End Function

        Public Function checkUserUnique(ByVal cUsername As String, Optional ByVal nCurrId As Long = 0) As Boolean
            PerfMon.Log("DBHelper", "checkUserUnique")
            Dim sSql As String
            Dim oDr As SqlDataReader
            Dim cProcessInfo As String = ""

            Try
                If nCurrId > 0 Then
                    sSql = "select * from tblDirectory where cDirSchema = 'User' and cDirName = '" & SqlFmt(cUsername) & "' and nDirKey <> " & nCurrId
                Else
                    sSql = "select * from tblDirectory where cDirSchema = 'User' and cDirName = '" & SqlFmt(cUsername) & "'"
                End If

                oDr = getDataReader(sSql)
                If oDr.HasRows Then
                    oDr.Close()
                    oDr = Nothing
                    Return False
                Else
                    oDr.Close()
                    oDr = Nothing
                    Return True
                End If

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "checkUserExists", ex, cProcessInfo))

            End Try
        End Function

        Public Function checkEmailUnique(ByVal cEmail As String, Optional ByVal nCurrId As Long = 0) As Boolean
            PerfMon.Log("DBHelper", "checkEmailUnique")
            Dim sSql As String
            Dim oDr As SqlDataReader
            Dim cProcessInfo As String = ""

            Try

                If LCase(myWeb.moConfig("EmailUsernames")) = "on" Then
                    If nCurrId > 0 Then
                        sSql = "select * from tblDirectory where cDirSchema = 'User' and cDirEmail like '" & SqlFmt(cEmail) & "' and nDirKey <> " & nCurrId
                    Else
                        sSql = "select * from tblDirectory where cDirSchema = 'User' and cDirEmail like '" & SqlFmt(cEmail) & "'"
                    End If
                Else
                    If nCurrId > 0 Then
                        sSql = "select * from tblDirectory where cDirSchema = 'User' and cDirXml like '%<Email>" & SqlFmt(cEmail) & "</Email>%' and nDirKey <> " & nCurrId
                    Else
                        sSql = "select * from tblDirectory where cDirSchema = 'User' and cDirXml like '%<Email>" & SqlFmt(cEmail) & "</Email>%'"
                    End If
                End If

                oDr = getDataReader(sSql)
                If oDr.HasRows Then
                    oDr.Close()
                    oDr = Nothing
                    If nCurrId > 0 Then
                        'fix for duplicate emails allready on the system
                        Return True
                    Else
                        Return False
                    End If
                Else
                    oDr.Close()
                    oDr = Nothing
                    Return True
                End If
                PerfMon.Log("DBHelper", "checkEmailUnique", sSql)

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "checkUserExists", ex, cProcessInfo))

            End Try
        End Function

        Public Function checkUserRole(ByVal cRoleName As String, Optional ByVal cSchemaName As String = "Role") As Boolean
            PerfMon.Log("DBHelper", "checkUserRole")
            Dim sSql As String
            Dim oDr As SqlDataReader
            Dim cProcessInfo As String = ""
            Dim bValid As Boolean
            Try
                'get group memberships
                sSql = "SELECT d.* FROM tblDirectory d " & _
                "inner join tblDirectoryRelation r on r.nDirParentId = d.nDirKey " & _
                "where r.nDirChildId = " & mnUserId & " and d.cDirSchema='" & cSchemaName & "'"

                oDr = getDataReader(sSql)
                While oDr.Read
                    If oDr("cDirName") = cRoleName Then
                        bValid = True
                    End If

                End While
                oDr.Close()
                oDr = Nothing

                Return bValid

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "checkUserRole", ex, cProcessInfo))

            End Try
        End Function

        Public Function getUserXMLById(ByRef nUserId As Integer) As XmlElement
            PerfMon.Log("DBHelper", "getUserXMLById")
            'Dim oDs As Data.DataSet
            Dim odr As SqlDataReader
            Dim root As XmlElement
            Dim oElmt As XmlElement
            Dim sSql As String

            Dim sProcessInfo As String = ""
            Dim cOverrideUserGroups As String
            Dim cJoinType As String
            Try
                cOverrideUserGroups = goConfig("SearchAllUserGroups")

                root = moPageXml.CreateElement("User")
                root.SetAttribute("id", nUserId)
                If nUserId <> 0 Then
                    odr = getDataReader("SELECT * FROM tblDirectory where nDirKey = " & nUserId)
                    While odr.Read
                        root.SetAttribute("name", odr("cDirName"))
                        If odr("cDirXml") <> "" Then
                            root.InnerXml = odr("cDirXml")
                            root.InnerXml = root.SelectSingleNode("*").InnerXml
                        End If
                    End While
                    odr.Close()
                    odr = Nothing

                    'get parent directory item memberships
                    If cOverrideUserGroups = "on" Then
                        cJoinType = "LEFT"
                    Else
                        cJoinType = "INNER"
                    End If

                    sSql = "SELECT	d.*," & _
                            " r.nRelKey As Member" & _
                            " FROM	tblDirectory d" & _
                            " " & cJoinType & " JOIN tblDirectoryRelation r" & _
                            " ON r.nDirParentid = d.nDirKey " & _
                            " AND r.nDirChildId = " & nUserId & _
                            " WHERE   d.cDirSchema <> 'User'" & _
                            " ORDER BY d.cDirName"

                    odr = getDataReader(sSql)
                    While odr.Read
                        oElmt = moPageXml.CreateElement(odr("cDirSchema"))
                        oElmt.SetAttribute("id", odr("nDirKey"))
                        oElmt.SetAttribute("name", odr("cDirName"))
                        If Not IsDBNull(odr("Member")) Then
                            oElmt.SetAttribute("isMember", "yes")
                        End If

                        If Not cOverrideUserGroups = "on" Then
                            If Not IsDBNull(odr("Member")) Then
                                root.AppendChild(oElmt)
                            End If
                        Else
                            root.AppendChild(oElmt)
                        End If
                    End While
                    odr.Close()
                    odr = Nothing
                End If

                Return root

            Catch ex As Exception

                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getUserXMLById", ex, sProcessInfo))
                Return Nothing
            End Try


        End Function

        Public Function logActivity(ByVal nActivityType As ActivityType, ByVal nUserDirId As Long, ByVal nStructId As Long, Optional ByVal nArtId As Long = 0, Optional ByVal cActivityDetail As String = "", Optional ByVal cForiegnRef As String = "") As Long
            Dim cSubName As String = "logActivity(ActivityType,Int,Int,[Int],[String])"
            PerfMon.Log("DBHelper", cSubName)

            Try
                Return logActivity(nActivityType, nUserDirId, nStructId, nArtId, 0, cActivityDetail, False, cForiegnRef)
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, cSubName, ex, ""))
            End Try

        End Function

        Public Function logActivity(ByVal nActivityType As ActivityType, ByVal nUserDirId As Long, ByVal nStructId As Long, ByVal nArtId As Long, ByVal nOtherId As Long, ByVal cActivityDetail As String) As Long
            Return logActivity(nActivityType, nUserDirId, nStructId, nArtId, nOtherId, cActivityDetail, False)
        End Function

        Public Function logActivity(ByVal loggedActivityType As ActivityType, ByVal userDirId As Long, ByVal structId As Long, ByVal artId As Long, ByVal otherId As Long, ByVal activityDetail As String, ByVal removePreviousActivitiesFromCurrentSession As Boolean, Optional cForiegnRef As String = Nothing) As Long

            Dim cSubName As String = "logActivity(ActivityType,Int,Int,Int,Int,String,Boolean)"

            PerfMon.Log("DBHelper", cSubName)
            Dim sSql As String
            Dim cProcessInfo As String = ""
            Dim sessionId As String = ""
            Try

                ' Find the Session ID
                If goSession IsNot Nothing Then
                    sessionId = goSession.SessionID
                End If

                If removePreviousActivitiesFromCurrentSession And Not String.IsNullOrEmpty(sessionId) Then
                    sSql = "DELETE FROM tblActivityLog WHERE cSessionId=" & SqlString(sessionId) & " AND nActivityType=" & SqlFmt(loggedActivityType)
                    ExeProcessSql(sSql)
                End If


                If gbIPLogging Then
                    checkForIpAddressCol()
                End If


                ' Generate the SQL statement
                ' First add the values

                Dim valuesList As New List(Of String)
                valuesList.Add(userDirId.ToString)
                valuesList.Add(structId.ToString)
                valuesList.Add(artId.ToString)
                valuesList.Add(Eonic.Tools.Database.SqlDate(Now, True))
                valuesList.Add(loggedActivityType)
                valuesList.Add(SqlString(activityDetail))
                valuesList.Add(SqlString(IIf(String.IsNullOrEmpty(sessionId), "Service_" & Now.ToString(), sessionId)))
                If otherId > 0 Then valuesList.Add(otherId.ToString)
                If gbIPLogging Then valuesList.Add(SqlString(Left(myWeb.moRequest.ServerVariables("REMOTE_ADDR"), 15)))

                ' Now build the SQL
                sSql = "Insert Into tblActivityLog (nUserDirId, nStructId, nArtId, dDateTime, nActivityType, cActivityDetail, cSessionId"

                ' Handle optional columns
                If otherId > 0 Then sSql &= ",nOtherId"
                If gbIPLogging Then sSql &= ",cIPAddress"

                If checkTableColumnExists("tblActivityLog", "cForeignRef") Then
                    sSql &= ",cForeignRef"
                    valuesList.Add(SqlString(cForiegnRef))
                End If

                sSql &= ") values (" & String.Join(",", valuesList.ToArray()) & ")"

                Return GetIdInsertSql(sSql)


            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, cSubName, ex, cProcessInfo))

            End Try

        End Function

        Public Sub updateActivity(ByVal activityID As Long, ByVal cActivityDetail As String)
            Try

                Dim sSql As String

                sSql = "Update tblActivityLog set cActivityDetail = '" & cActivityDetail & "' where nActivityKey = " & activityID

                ExeProcessSql(ssql)

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "updateActivity", ex, cActivityDetail))
            End Try
        End Sub


        Public Sub checkForIpAddressCol()

            'check for gbIPLogging column
            Dim strSqlCheckForIpCol As New Text.StringBuilder
            strSqlCheckForIpCol.Append("if NOT Exists(select * from sys.columns where Name = 'cIPAddress' and Object_ID = Object_ID('tblActivityLog')) ")
            strSqlCheckForIpCol.Append("begin ")
            strSqlCheckForIpCol.Append("alter table tblActivityLog add cIPAddress nvarchar(15) NULL  ")
            strSqlCheckForIpCol.Append("end ")

            ExeProcessSql(strSqlCheckForIpCol.ToString)


        End Sub



        Public Function AllowMigration() As Boolean
            PerfMon.Log("DBHelper", "AllowMigration")
            Dim oMigration As Object
            Try
                oMigration = GetDataValue("SELECT cLkpValue from tblLookup where cLkpKey = 'MigrationDone'")
                If oMigration = "1" Then
                    Return False
                Else
                    ExeProcessSql("INSERT INTO tblLookup (cLkpValue,cLkpKey,cLkpCategory) VALUES (1,'MigrationDone','MigrationDone')")
                    Return True
                End If
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "AllowMigration", ex, "AllowMigration"))

            End Try
        End Function

        Public Function FindDirectoryByForiegn(ByVal ForiegnRef As String) As Integer 'returns the id of the Dir Entry with the Foreign Ref
            PerfMon.Log("DBHelper", "FindDirectoryByForiegn")
            Try

                Dim strSQL As String = "Select nDirKey FROM tblDirectory WHERE cDirForiegnRef = '" & ForiegnRef & "'"
                Dim iID As Integer
                iID = CInt(ExeProcessSqlScalar(strSQL))
                Return iID
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "FindDirectoryByForiegn", ex, "AllowMigration"))
                Return -1

            End Try
        End Function

        Public Sub ListOrders(ByRef oContentsXML As XmlElement, ByVal ProcessId As Cart.cartProcess, ByVal cSchemaName As String)
            PerfMon.Log("DBHelper", "ListOrders")
            Dim oRoot As XmlElement
            Dim oElmt As XmlElement
            Dim sSql As String
            Dim oDs As DataSet
            Dim cProcessInfo As String = ""
            Try

                oRoot = moPageXml.CreateElement("Content")
                oRoot.SetAttribute("type", "listTree")
                oRoot.SetAttribute("template", "default")
                oRoot.SetAttribute("name", cSchemaName & "s - " & ProcessId.GetType.ToString)
                If ProcessId = 0 Then
                    sSql = "SELECT nCartOrderKey as id, nCartStatus as status, c.cContactName as name, c.cContactEmail as email, a.dUpdateDate from tblCartOrder inner join tblAudit a on nAuditId = a.nAuditKey left outer join tblCartContact c on (nCartOrderKey = c.nContactCartId and cContactType = 'Billing Address') where (cCartSchemaName= '" & cSchemaName & "') order by nCartOrderKey desc"
                Else
                    sSql = "SELECT nCartOrderKey as id, nCartStatus as status, c.cContactName as name, c.cContactEmail as email, a.dUpdateDate from tblCartOrder inner join tblAudit a on nAuditId = a.nAuditKey left outer join tblCartContact c on (nCartOrderKey = c.nContactCartId and cContactType = 'Billing Address') where nCartStatus = " & ProcessId & " and (cCartSchemaName= '" & cSchemaName & "') order by nCartOrderKey desc"
                End If


                oDs = GetDataSet(sSql, cSchemaName, "List")

                If oDs.Tables(0).Rows.Count > 0 Then
                    oDs.Tables(0).Columns(0).ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns(1).ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns(2).ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns(3).ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns(4).ColumnMapping = Data.MappingType.Attribute

                    'load existing data into the instance
                    oElmt = moPageXml.CreateElement("List")
                    oElmt.InnerXml = oDs.GetXml

                    oContentsXML.AppendChild(oElmt)
                End If

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "ListShippingLocations", ex, cProcessInfo))
            End Try
        End Sub

        Public Function DisplayCart(ByVal nCartId As Long, ByVal cCartSchema As String) As XmlElement
            PerfMon.Log("DBHelper", "DisplayCart")
            '   Content for the XML that will display all the information stored for the Cart
            '   This is a list of cart items (and quantity, price ...), totals,
            '   billing & delivery addressi and delivery method.

            Dim oDs As DataSet

            Dim sSql As String
            Dim oRow As DataRow

            Dim oElmt As XmlElement
            Dim oXml As XmlDataDocument
            Dim oCartElmt As XmlElement
            Dim oOrderElmt As XmlElement

            Dim quant As Integer
            Dim weight As Double
            Dim total As Double
            Dim vatAmt As Double
            Dim shipCost As Double


            Dim cProcessInfo As String = ""
            Try

                Dim oCartConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("eonic/cart")

                Dim cSiteURL As String = oCartConfig("SiteURL")
                Dim cCartURL As String = oCartConfig("SecureURL")
                Dim nTaxRate As String = oCartConfig("TaxRate")
                Dim cOrderNoPrefix As String = oCartConfig("OrderNoPrefix")
                Dim cCurrency As String = oCartConfig("CurrencySymbol")

                oCartElmt = moPageXml.CreateElement("Cart")
                oCartElmt.SetAttribute("type", LCase(cCartSchema)) '"order")
                oCartElmt.SetAttribute("currency", cCurrency)

                oOrderElmt = moPageXml.CreateElement(cCartSchema)
                oCartElmt.AppendChild(oOrderElmt)

                If Not (nCartId > 0) Then '   no shopping
                    oOrderElmt.SetAttribute("status", "Empty") '   set CartXML attributes
                    oOrderElmt.SetAttribute("itemCount", "0") '       to nothing
                    oOrderElmt.SetAttribute("total", "0.00") '       for nothing
                Else

                    'Add Totals
                    quant = 0 '   get number of items & sum of collective prices (ie. cart total) from db
                    total = 0.0#
                    weight = 0.0#

                    sSql = "select i.nCartItemKey as id, i.cItemRef as ref, i.cItemURL as url, i.cItemName as Name, i.nPrice as price, i.nTaxRate as taxRate, i.nQuantity as quantity, i.nShpCat as shippingLevel, i.nDiscountValue as discount,i.nWeight as weight, p.cContentXmlDetail as productDetail, i.nItemOptGrpIdx, i.nItemOptIdx, i.nParentId  from tblCartItem i left join tblContent p on i.nItemId = p.nContentKey where nCartOrderId=" & nCartId

                    oDs = getDataSetForUpdate(sSql, "Item", "Cart")
                    '@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                    oDs.Relations.Add("Rel1", oDs.Tables("Item").Columns("id"), oDs.Tables("Item").Columns("nParentId"), False)
                    oDs.Relations("Rel1").Nested = True
                    '@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                    For Each oRow In oDs.Tables("Item").Rows

                        If CInt("0" & oRow("nParentId")) = 0 Then

                            '@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                            Dim oOpRow As DataRow
                            Dim nOpPrices As Decimal = 0
                            For Each oOpRow In oRow.GetChildRows("Rel1")
                                'need an option check price bit here

                                nOpPrices += oOpRow("price")

                            Next

                            weight = weight + (oRow("weight") * oRow("quantity"))
                            quant = quant + oRow("quantity")
                            total = total + (oRow("quantity") * ((oRow("price") + nOpPrices) - CInt("0" & oRow("discount"))))
                        End If

                    Next
                    updateDataset(oDs, "Item", True)

                    '   add to Cart XML
                    oCartElmt.SetAttribute("status", "Active")
                    oCartElmt.SetAttribute("itemCount", quant)
                    oCartElmt.SetAttribute("weight", weight)
                    oCartElmt.SetAttribute("orderType", "")

                    'Add the addresses to the dataset
                    If nCartId > 0 Then
                        sSql = "select cContactType as type, cContactName as GivenName, cContactCompany as Company, cContactAddress as Street, cContactCity as City, cContactState as State, cContactZip as PostalCode, cContactCountry as Country, cContactTel as Telephone, cContactFax as Fax, cContactEmail as Email, cContactXml as Details from tblCartContact where nContactCartId=" & nCartId
                        addTableToDataSet(oDs, sSql, "Contact")
                    End If
                    'Add Items - note - do this AFTER we've updated the prices! 

                    If oDs.Tables("Item").Rows.Count > 0 Then
                        'cart items
                        oDs.Tables(0).Columns(0).ColumnMapping = Data.MappingType.Attribute
                        oDs.Tables(0).Columns(1).ColumnMapping = Data.MappingType.Attribute
                        oDs.Tables(0).Columns(2).ColumnMapping = Data.MappingType.Attribute
                        oDs.Tables(0).Columns(4).ColumnMapping = Data.MappingType.Attribute
                        oDs.Tables(0).Columns(5).ColumnMapping = Data.MappingType.Attribute
                        oDs.Tables(0).Columns(6).ColumnMapping = Data.MappingType.Attribute
                        oDs.Tables(0).Columns(7).ColumnMapping = Data.MappingType.Attribute
                        oDs.Tables(0).Columns(8).ColumnMapping = Data.MappingType.Attribute
                        'cart contacts
                        oDs.Tables("Contact").Columns(0).ColumnMapping = Data.MappingType.Attribute

                        oXml = New XmlDataDocument(oDs)
                        oDs.EnforceConstraints = False
                        'Convert the detail to xml
                        For Each oElmt In oXml.SelectNodes("/Cart/Item/productDetail | /Cart/Contact/Detail")
                            oElmt.InnerXml = oElmt.InnerText
                            If Not oElmt.SelectSingleNode("Content") Is Nothing Then
                                oElmt.InnerXml = oElmt.SelectSingleNode("Content").InnerXml
                            End If
                        Next
                        oOrderElmt.InnerXml = oXml.FirstChild.InnerXml
                    End If
                    oXml = Nothing
                    oDs = Nothing

                    sSql = "select o.*, dUpdateDate from tblCartOrder o join tblAudit a on nAuditId = a.nAuditKey where nCartOrderKey=" & nCartId
                    oDs = GetDataSet(sSql, "Order", "Cart")
                    For Each oRow In oDs.Tables("Order").Rows
                        oOrderElmt.SetAttribute("ref", cOrderNoPrefix & oRow("nCartOrderKey"))
                        oOrderElmt.SetAttribute("date", oRow("dUpdateDate"))
                        oOrderElmt.SetAttribute("status", oRow("nCartStatus"))

                        shipCost = CDbl("0" & oRow("nShippingCost"))
                        oOrderElmt.SetAttribute("shippingType", oRow("nShippingMethodId") & "")
                        oOrderElmt.SetAttribute("shippingCost", shipCost & "")

                        If nTaxRate > 0 Then

                            vatAmt = (total + shipCost) * (nTaxRate / 100)

                            oOrderElmt.SetAttribute("totalNet", FormatNumber(total + shipCost, 2, TriState.True, TriState.False, TriState.False))
                            oOrderElmt.SetAttribute("vatRate", nTaxRate)
                            oOrderElmt.SetAttribute("shippingType", oRow("nShippingMethodId") & "")
                            oOrderElmt.SetAttribute("shippingCost", FormatNumber(shipCost, 2, TriState.True, TriState.False, TriState.False))
                            oOrderElmt.SetAttribute("vatAmt", FormatNumber(vatAmt, 2, TriState.True, TriState.False, TriState.False))
                            oOrderElmt.SetAttribute("total", FormatNumber(total + shipCost + vatAmt, 2, TriState.True, TriState.False, TriState.False))
                        Else
                            oOrderElmt.SetAttribute("totalNet", FormatNumber(total + shipCost, 2, TriState.True, TriState.False, TriState.False))
                            oOrderElmt.SetAttribute("vatRate", 0.0#)
                            oOrderElmt.SetAttribute("shippingType", oRow("nShippingMethodId") & "")
                            oOrderElmt.SetAttribute("shippingCost", FormatNumber(shipCost, 2, TriState.True, TriState.False, TriState.False))
                            oOrderElmt.SetAttribute("vatAmt", 0.0#)
                            oOrderElmt.SetAttribute("total", FormatNumber(total + shipCost, 2, TriState.True, TriState.False, TriState.False))
                        End If



                        If Not (IsDBNull(oRow("cClientNotes")) Or oRow("cClientNotes") & "" = "") Then
                            oElmt = moPageXml.CreateElement("ClientNotes")
                            oElmt.InnerXml = oRow("cClientNotes")
                            oOrderElmt.AppendChild(oElmt.FirstChild())
                        End If
                        If Not (oRow("cSellerNotes") & "" = "") Then
                            oElmt = moPageXml.CreateElement("SellerNotes")
                            oElmt.InnerText = oRow("cSellerNotes")
                            oOrderElmt.AppendChild(oElmt)
                        End If
                    Next

                End If

                Return oCartElmt

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, cProcessInfo))
                Return Nothing
            End Try

        End Function

        Public Sub ListUserOrders(ByRef oContentsXML As XmlElement, ByVal cOrderType As String)
            PerfMon.Log("DBHelper", "ListUserOrders")
            Dim oRoot As XmlElement
            Dim oElmt As XmlElement
            Dim oElmtOrder As XmlElement
            Dim sSql As String
            Dim oDs As DataSet
            Dim oDr As DataRow

            Dim cProcessInfo As String = ""
            Dim cExtraWhere As String = ""
            Dim UserId As Long = CLng("0" & goSession("nEwUserId"))
            Try

                If UserId = 0 Then UserId = mnUserId

                'Basic List
                oRoot = moPageXml.CreateElement("Content")
                oRoot.SetAttribute("template", "default")
                Select Case cOrderType
                    Case "Order"
                        oRoot.SetAttribute("name", "Orders")
                        cExtraWhere = "(tblCartOrder.cCartSchemaName = 'Order' or tblCartOrder.cCartSchemaName = '' or tblCartOrder.cCartSchemaName is null )" ' AND (tblCartOrder.nCartStatus = 7)"
                    Case "Quote"
                        oRoot.SetAttribute("name", "Quotes")
                        cExtraWhere = "(tblCartOrder.cCartSchemaName = 'Quote')" ' AND (tblCartOrder.nCartStatus = 7)"
                    Case "GiftList"
                        oRoot.SetAttribute("name", "Gift Lists")
                        cExtraWhere = "(tblCartOrder.cCartSchemaName = 'GiftList')" ' AND (tblCartOrder.nCartStatus = 7)"
                    Case Else
                End Select

                sSql = "SELECT tblCartOrder.nCartOrderKey, tblCartOrder.cCartXml FROM tblCartOrder INNER JOIN tblAudit ON tblCartOrder.nAuditId = tblAudit.nAuditKey WHERE tblCartOrder.nCartUserDirId = " & UserId & " AND " & cExtraWhere
                oDs = GetDataSet(sSql, cOrderType, "OrderList")
                For Each oDr In oDs.Tables(0).Rows
                    oElmt = moPageXml.CreateElement(cOrderType)
                    oElmt.InnerXml = oDr("cCartXml")
                    oElmtOrder = oElmt.FirstChild
                    oElmtOrder.SetAttribute("id", oDr("nCartOrderKey"))
                    oContentsXML.AppendChild(oElmt.FirstChild)
                Next

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "ListUserOrders", ex, cProcessInfo))
            End Try
        End Sub


        Public Sub ListUserVouchers(ByRef oContentsXML As XmlElement)
            PerfMon.Log("DBHelper", "ListUserOrders")
            Dim oRoot As XmlElement
            Dim oElmt As XmlElement
            Dim oElmtOrder As XmlElement
            Dim sSql As String
            Dim oDs As DataSet
            Dim oDr As DataRow

            Dim cProcessInfo As String = ""
            Dim cExtraWhere As String = ""
            Dim UserId As Long = CLng("0" & goSession("nEwUserId"))
            Dim oXml As XmlDocument
            Try

                If UserId = 0 Then UserId = mnUserId

                'Basic List
                oRoot = moPageXml.CreateElement("Content")
                oRoot.SetAttribute("template", "default")

                sSql = "SELECT nCodeKey as id, cCode as code, dIssuedDate as issueDate, dUseDate as usedDate, xUsageData as UsageData FROM tblCodes INNER JOIN tblAudit ON tblCodes.nAuditId = tblAudit.nAuditKey WHERE tblCodes.nIssuedDirId = " & UserId & ""
                oDs = GetDataSet(sSql, "Voucher", "VoucherList")


                If oDs.Tables("Voucher").Rows.Count > 0 Then
                    'cart items
                    oDs.Tables(0).Columns("id").ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns("issueDate").ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns("usedDate").ColumnMapping = Data.MappingType.Attribute

                    oXml = New XmlDataDocument(oDs)
                    oDs.EnforceConstraints = False

                    oContentsXML.InnerXml = oXml.FirstChild.InnerXml
                End If
                oXml = Nothing
                oDs = Nothing

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "ListUserOrders", ex, cProcessInfo))
            End Try
        End Sub


        Public Function exportShippingLocations() As String
            PerfMon.Log("DBHelper", "exportShippingLocations")
            Dim cSql As String
            Dim oXml As XmlDataDocument
            Dim oDs As DataSet
            Dim cXml As String

            ' Get all the data from the table
            cSql = "select * from "
            If Not (goRequest("db") Is Nothing) Then
                If goRequest("db") <> "" Then
                    cSql = cSql & "[" & goRequest("db") & "]."
                End If
            End If
            Select Case goRequest("version")
                Case "v2"
                    cSql = cSql & "[dbo].[tbl_ewc_shippingLocations]"
                Case "v4"
                    cSql = cSql & "[dbo].[tblCartShippingLocations]"
            End Select
            oDs = GetDataSet(cSql, "tblCartShippingLocations", "Export")
            ReturnNullsEmpty(oDs)

            ' Put the data into an xml document
            If oDs Is Nothing Then
                cXml = "<Export/>"
            Else
                oXml = New XmlDataDocument(oDs)
                cXml = oXml.InnerXml
            End If


            ' Things to ponder - do we need to convert any xml blobs within a field (as their entities will have been converted to &entity;)
            Return cXml

        End Function

        Public Function importObjects(ByVal ObjectsXml As XmlElement, Optional FeedRef As String = "") As String
            PerfMon.Log("DBHelper", "importObjects")
            Dim cProcessInfo As String = ""
            Dim cContentLocationTable As String = ""

            Dim cTableName As String = ""
            Dim cTableKey As String = ""
            Dim cTableFRef As String = ""

            Dim cPreviousTableName As String = ""

            Dim bDeleteNonEntries As Boolean = False
            Dim cDeleteTempTableName As String = ""
            Dim cDefiningField As String = ""
            Dim cDefiningFieldValue As String = ""
            Dim cDeleteTempType As String = "Content"
            Dim cDefiningWhereStmt As String = ""
            Dim bSkipExisting As Boolean = False
            Dim bResetLocations As Boolean = True

            Try

                'Do we allready have a feed running that is not complete ?

                Dim FeedCheck As String = ""
                If FeedRef <> "" Then

                    Dim sSQL As String = "select cActivityDetail from tblActivityLog where cActivityDetail like '" & FeedRef & "#' and not(cActivityDetail like '#Complete' )"
                    FeedCheck = Me.ExeProcessSqlScalar(sSQL)

                End If

                If FeedCheck <> "" Then
                    Return FeedCheck
                    Exit Function
                End If

                Dim oInstance As XmlElement
                If Not ObjectsXml Is Nothing Then

                    cContentLocationTable = getTable(objectTypes.ContentLocation)

                    'NB NEW STUFF ------------
                    'Check that we want to delete missing objects from the spreadsheet (For Content)
                    If Not ObjectsXml.SelectSingleNode("DeleteNonEntries[@enabled='true']") Is Nothing Then
                        'Now look for the defining field, this allows say the content to only work with a distinct type of object, as defined by the defining field name

                        If Not ObjectsXml.SelectSingleNode("DeleteNonEntries/@sqlWhere") Is Nothing Then
                            cDefiningWhereStmt = ObjectsXml.SelectSingleNode("DeleteNonEntries/@sqlWhere").InnerText
                        End If

                        If Not ObjectsXml.SelectSingleNode("DeleteNonEntries[@enabled='true']/cDefiningField") Is Nothing Then
                            cDefiningField = ObjectsXml.SelectSingleNode("DeleteNonEntries[@enabled='true']/cDefiningField").InnerText.ToString
                            cDefiningFieldValue = ObjectsXml.SelectSingleNode("DeleteNonEntries[@enabled='true']/cDefiningField/@value").InnerText.ToString
                            bDeleteNonEntries = True
                            If Not ObjectsXml.SelectSingleNode("DeleteNonEntries/@type") Is Nothing Then
                                cDeleteTempType = ObjectsXml.SelectSingleNode("DeleteNonEntries/@type").InnerText.ToString
                            End If
                            cDeleteTempTableName = "_temp_" & Date.Now.ToString
                            cDeleteTempTableName = cDeleteTempTableName.Replace("/", "_").Replace(":", "_").Replace(" ", "_")
                            'Remember to import the SP into the database to be used
                            'The next line is currently not used, it was incase of having to use a Store Procedure, however that did not overcome the collation error
                            'Dim cSQL As String = "exec [spCreateImportTable] '" & cDeleteTempTableName & "'"
                            Dim cSQL As String = "CREATE TABLE dbo." & cDeleteTempTableName & " (cImportID nvarchar(800), cTableName nvarchar(50))"
                            Me.ExeProcessSql(cSQL)
                        End If
                    End If

                    'To delete existing Directory Relations (excluding Admin ones)
                    If Not ObjectsXml.SelectSingleNode("DeleteDirRelations[@enabled='true']") Is Nothing Then
                        Dim cSql_Relation_Audits As String = "DELETE tblAudit from tblAudit a " & _
                        "Inner Join tblDirectoryRelation r " & _
                        "On r.nAuditId = a.nAuditKey " & _
                        "Where r.nDirChildId IN ( " & _
                        "Select nDirKey " & _
                        "From tblDirectory " & _
                        "WHERE nDirKey NOT IN (" & _
                        "Select d.nDirKey " & _
                        "From tblDirectoryRelation r " & _
                        "Inner Join tblDirectory d " & _
                        "On r.nDirChildId = d.nDirKey " & _
                        "WHERE r.nDirParentId = " & _
                        "(SELECT nDirKey From tblDirectory Where cDirForiegnRef = 'Administrator')))"

                        myWeb.moDbHelper.ExeProcessSqlorIgnore(cSql_Relation_Audits)

                        Dim cSql_Relations As String = "DELETE " & _
                            "From tblDirectoryRelation " & _
                            "Where nDirChildId IN ( " & _
                            "Select nDirKey " & _
                            "From tblDirectory " & _
                            "WHERE nDirKey NOT IN (" & _
                            "Select d.nDirKey " & _
                            "From tblDirectoryRelation r " & _
                            "Inner Join tblDirectory d " & _
                            "On r.nDirChildId = d.nDirKey " & _
                            "WHERE r.nDirParentId = " & _
                            "(SELECT nDirKey From tblDirectory Where cDirForiegnRef = 'Administrator')))"

                        myWeb.moDbHelper.ExeProcessSqlorIgnore(cSql_Relations)
                    End If
                    'NB NEW STUFF ------------

                    If Not ObjectsXml.SelectSingleNode("SkipExisting[@enabled='true']") Is Nothing Then
                        bSkipExisting = True
                    End If

                    Dim bOrphan As Boolean = ObjectsXml.SelectSingleNode("NoLocations[@enabled='true']") IsNot Nothing

                    If Not ObjectsXml.SelectSingleNode("ResetLocations[@enabled='false']") Is Nothing Then
                        bResetLocations = False
                    End If
                    Dim totalInstances As Long = ObjectsXml.SelectNodes("Instance | instance").Count

                    Dim ReturnMessage As String = FeedRef & " Importing " & totalInstances & " Objects"

                    Dim logId As Long = Me.logActivity(ActivityType.ContentImport, mnUserId, 0, 0, ReturnMessage & " Started")

                    Dim completeCount As Long = 0


                    Dim Tasks As New dbImport(Me.oConn.ConnectionString, mnUserId)

                    System.Threading.ThreadPool.SetMaxThreads(10, 10)

                    'Dim InstanceCol As IEnumerable(Of XmlNode) = ObjectsXml.SelectNodes("Instance | instance")


                    'System.Threading.Tasks.Parallel.ForEach(InstanceCol, Sub(myInstance As XmlNode)

                    '                                                         completeCount = completeCount + 1

                    '                                                         Dim stateObj As New dbImport.ImportStateObj()
                    '                                                         stateObj.oInstance = myInstance
                    '                                                         stateObj.LogId = logId
                    '                                                         stateObj.CompleteCount = completeCount
                    '                                                         stateObj.totalInstances = totalInstances
                    '                                                         stateObj.bSkipExisting = bSkipExisting
                    '                                                         stateObj.bResetLocations = bResetLocations
                    '                                                         stateObj.bOrphan = bOrphan
                    '                                                         stateObj.bDeleteNonEntries = bDeleteNonEntries
                    '                                                         stateObj.cDeleteTempTableName = cDeleteTempTableName

                    '                                                         Tasks.ImportSingleObject(stateObj)

                    '                                                     End Sub)

                    Dim doneEvents(totalInstances) As System.Threading.ManualResetEvent

                    For Each oInstance In ObjectsXml.SelectNodes("Instance | instance")

                        completeCount = completeCount + 1

                        Dim stateObj As New dbImport.ImportStateObj()
                        stateObj.oInstance = oInstance
                        stateObj.LogId = logId
                        stateObj.FeedRef = FeedRef
                        stateObj.CompleteCount = completeCount
                        stateObj.totalInstances = totalInstances
                        stateObj.bSkipExisting = bSkipExisting
                        stateObj.bResetLocations = bResetLocations
                        stateObj.bOrphan = bOrphan
                        stateObj.bDeleteNonEntries = bDeleteNonEntries
                        stateObj.cDeleteTempTableName = cDeleteTempTableName

                        If oInstance.NextSibling Is Nothing Then
                            cProcessInfo = "Is Last"
                        End If

                        System.Threading.ThreadPool.QueueUserWorkItem(New System.Threading.WaitCallback(AddressOf Tasks.ImportSingleObject), stateObj)

                        stateObj = Nothing

                    Next

                    Return ReturnMessage

                    System.Threading.WaitHandle.WaitAll(doneEvents)

                    'Me.updateActivity(logId, "Importing " & totalInstances & "Objects, " & completeCount & " Complete")

                    ''lets get the object type from the table name.
                    'cTableName = oInstance.FirstChild.Name

                    ''return the object type from the table name
                    'Dim oTblName As TableNames
                    'For Each oTblName In [Enum].GetValues(GetType(objectTypes))
                    '    If oTblName.ToString = cTableName Then Exit For
                    'Next
                    'Dim oObjType As New objectTypes

                    ''^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                    ''Disabled on 16/09/2008 the following, due to the incompatible assignment of Value to the Object Types 
                    ''oTblName = oObjType
                    'oObjType = oTblName
                    ''^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

                    '' The purpose of this is to try to reduce the amount of table name/key/fref calls
                    '' so to optimise this for bulk use.
                    'If cTableName <> cPreviousTableName Then
                    '    cTableKey = getKey(oObjType)
                    '    cTableFRef = getFRef(oObjType)
                    'End If



                    'Dim fRefNode As XmlElement = oInstance.SelectSingleNode(cTableName & "/" & cTableFRef)
                    'Dim fRef As String = fRefNode.InnerText

                    ''We absolutly do not do anything if no fRef
                    'If Not fRef = "" Then
                    '    Dim nId As Long
                    '    'lets get an id if we are updating a record with a foriegn Ref

                    '    nId = getObjectByRef(cTableName, cTableKey, cTableFRef, oObjType, fRef)

                    '    'nId = myWeb.moDbHelper.getObjectByRef(cTableName, cTableKey, cTableFRef, oObjType, fRef)

                    '    'if we want to replace the fRef
                    '    If Not fRefNode.GetAttribute("replaceWith") = "" Then
                    '        fRefNode.InnerText = fRefNode.GetAttribute("replaceWith")
                    '    End If

                    '    oInstance.SelectSingleNode(cTableName & "/" & cTableFRef)

                    '    If Not (bSkipExisting And nId <> 0) Then
                    '        nId = setObjectInstance(oObjType, oInstance, nId)
                    '    End If

                    '    ' PerfMon.Log("DBHelper", "importObjects", "objectId=" & nId)

                    '    processInstanceExtras(nId, oInstance, bResetLocations, bOrphan)

                    '    'NB NEW STUFF ------------
                    '    If bDeleteNonEntries Then

                    '        Dim cSQL As String = "INSERT INTO dbo." & cDeleteTempTableName & " (cImportID , cTableName) VALUES ('" & SqlFmt(fRef) & "','" & SqlFmt(cTableName) & "')"
                    '        Me.ExeProcessSql(cSQL)

                    '    End If
                    '    'NB NEW STUFF ------------


                    'End If

                    If bDeleteNonEntries Then

                        Dim cSQL As String = ""

                        'The following check ensures if the temp table is empty, nothing is deleted
                        'This is incase nothing is imported, maybe due to wrong import XSL
                        Dim nSizeCheck As String = ""
                        cSQL = "SELECT * FROM " & cDeleteTempTableName
                        nSizeCheck = "" + Me.ExeProcessSqlScalar(cSQL)

                        If Not nSizeCheck.Equals("") Then

                            'Remove anything that's not from tblContent (future upgrade to support further tables maybe?)
                            'cSQL = "DELETE FROM " & cDeleteTempTableName & " WHERE cTableName != 'tblContent'"
                            'Me.ExeProcessSql(cSQL)
                            Select Case cDeleteTempType
                                Case "Content"
                                    'Delete Content Items
                                    cSQL = "Select nContentKey FROM tblContent " _
                                        & "WHERE nContentKey IN (SELECT nContentKey FROM tblContent c " _
                                        & " LEFT OUTER JOIN " & cDeleteTempTableName & " t " _
                                        & " ON c.cContentForiegnRef = t.cImportID "
                                    If cDefiningWhereStmt = "" Then
                                        cSQL += " WHERE t.cImportID is null AND c." & cDefiningField & " = '" & SqlFmt(cDefiningFieldValue) & "'"
                                    Else
                                        cSQL += " WHERE t.cImportID is null AND c." & cDefiningField & " = '" & SqlFmt(cDefiningFieldValue) & "' AND " & cDefiningWhereStmt & ""
                                    End If
                                    cSQL += ")"

                                    Dim oDR As SqlClient.SqlDataReader = myWeb.moDbHelper.getDataReader(cSQL)

                                    Do While oDR.Read
                                        myWeb.moDbHelper.DeleteObject(Web.dbHelper.objectTypes.Content, oDR(0))
                                    Loop
                                Case "Directory"
                                    'Delete Directory Items
                                    cSQL = "Select nDirKey FROM tblDirectory " _
                                                                  & "WHERE nDirKey IN (SELECT nDirKey FROM tblDirectory d " _
                                                                  & " LEFT OUTER JOIN " & cDeleteTempTableName & " t " _
                                                                  & " ON d.cDirForiegnRef = t.cImportID "
                                    If cDefiningWhereStmt = "" Then
                                        cSQL += " WHERE t.cImportID is null AND d." & cDefiningField & " = '" & SqlFmt(cDefiningFieldValue) & "'"
                                    Else
                                        cSQL += " WHERE t.cImportID is null AND d." & cDefiningField & " = '" & SqlFmt(cDefiningFieldValue) & "' AND " & cDefiningWhereStmt & ""
                                    End If
                                    cSQL += ")"

                                    Dim oDR As SqlClient.SqlDataReader = myWeb.moDbHelper.getDataReader(cSQL)

                                    Do While oDR.Read
                                        If oDR(0) <> 1 Then
                                            'dont delete admin logon
                                            myWeb.moDbHelper.DeleteObject(Web.dbHelper.objectTypes.Directory, oDR(0))
                                        End If
                                    Loop
                            End Select



                        End If
                        cSQL = "DROP TABLE " & cDeleteTempTableName
                        Me.ExeProcessSql(cSQL)
                    End If


                    updateActivity(logId, ReturnMessage & " Complete")

                End If

                Return ""

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "ImportObjects", ex, cProcessInfo))
                Return ""
            End Try
        End Function

     


        Public Function processInstanceExtras(ByVal savedId As Long, ByVal oInstance As XmlElement, ByVal bResetLocations As Boolean, ByVal bOrphan As Boolean) As Integer
            PerfMon.Log("DBHelper", "processInstanceExtras", "")
            Dim cProcessInfo As String = ""
            Dim i As Integer = 0
            Try
                Dim cContentLocationTable As String = getTable(objectTypes.ContentLocation)
                'lets get the object type from the table name.
                If oInstance Is Nothing Then
                    Return 0
                Else

                    Dim cTableName As String = oInstance.FirstChild.Name

                    'return the object type from the table name
                    Dim oTblName As TableNames
                    For Each oTblName In [Enum].GetValues(GetType(objectTypes))
                        If oTblName.ToString = cTableName Then Exit For
                    Next
                    Dim oObjType As New objectTypes
                    oObjType = oTblName

                    ' Type specific additional processes.
                    Select Case oObjType
                        Case objectTypes.Content
                            'now lets sort out the locations
                            'lets delete previous locations for that content
                            If bResetLocations Then
                                RemoveContentLocations(savedId, cContentLocationTable)
                            End If

                            'now lets add those specificed
                            ' Process locations
                            If Not bOrphan Then
                                Dim oLocation As XmlElement
                                Dim oPrmLoc As XmlElement = oInstance.SelectSingleNode("Location[@primary='true']")
                                If oPrmLoc Is Nothing Then
                                    oPrmLoc = oInstance.SelectSingleNode("Location")
                                End If
                                For Each oLocation In oInstance.SelectNodes("Location")
                                    Dim sPrimary As Long = 0
                                    If oLocation Is oPrmLoc Then sPrimary = 1
                                    If oLocation.GetAttribute("foriegnRef") <> "" Then
                                        Dim cleanFref As String = oLocation.GetAttribute("foriegnRef")
                                        If InStr(cleanFref, "&") Then
                                            cleanFref = cleanFref.Replace("&amp;", "&")
                                        End If
                                        setContentLocationByRef(cleanFref, savedId, sPrimary, 0, oLocation.GetAttribute("position"))
                                    ElseIf oLocation.GetAttribute("id") <> "" Then
                                        setContentLocation(oLocation.GetAttribute("id"), savedId, sPrimary, False, False, oLocation.GetAttribute("position"))
                                    End If
                                Next
                            End If


                            'lets look for content relationships
                            Dim oRelation As XmlElement
                            For Each oRelation In oInstance.SelectNodes("Relation")
                                ' Relate this content to an item by either that item's parent ID or the foreign ref
                                If Not String.IsNullOrEmpty(oRelation.GetAttribute("foriegnRef")) Then
                                    setContentRelationByRef(savedId, oRelation.GetAttribute("foriegnRef"), True, oRelation.GetAttribute("type"), True)
                                ElseIf Not String.IsNullOrEmpty(oRelation.GetAttribute("relatedContentId")) _
                                        AndAlso Tools.Number.IsReallyNumeric(oRelation.GetAttribute("relatedContentId")) _
                                        AndAlso Convert.ToInt32(oRelation.GetAttribute("relatedContentId")) > 0 _
                                        AndAlso oRelation.GetAttribute("direction") = "" Then
                                    myWeb.moDbHelper.insertContentRelation(savedId, Convert.ToInt32(oRelation.GetAttribute("relatedContentId")), True, oRelation.GetAttribute("type"), True)

                                ElseIf oRelation.GetAttribute("relatedContentId").Contains(",") Or oRelation.GetAttribute("direction") <> "" Then
                                    Dim relContId As String
                                    'remove existing content relations of type
                                    myWeb.moDbHelper.RemoveContentRelationByType(savedId, oRelation.GetAttribute("type"), oRelation.GetAttribute("direction"))

                                    For Each relContId In oRelation.GetAttribute("relatedContentId").Split(",")
                                        If IsNumeric(relContId) Then
                                            If LCase(oRelation.GetAttribute("direction")) = "child" Then
                                                myWeb.moDbHelper.insertContentRelation(Convert.ToInt32(relContId), savedId, True, oRelation.GetAttribute("type"), True)
                                            Else
                                                myWeb.moDbHelper.insertContentRelation(savedId, Convert.ToInt32(relContId), True, oRelation.GetAttribute("type"), True)
                                            End If
                                        End If
                                    Next
                                End If
                            Next
                            For Each oRelation In oInstance.SelectNodes("ProductGroups")
                                myWeb.moDbHelper.insertProductGroupRelation(savedId, oRelation.GetAttribute("ids"))
                            Next
                        Case objectTypes.Directory

                            Dim oRelation As XmlElement
                            For Each oRelation In oInstance.SelectNodes("Relation")
                                Dim nloc As Long
                                If oRelation.GetAttribute("foriegnRef") <> "" Then
                                    nloc = getObjectByRef(objectTypes.Directory, oRelation.GetAttribute("foriegnRef"), oRelation.GetAttribute("type"))
                                Else
                                    nloc = CInt("0" & oRelation.GetAttribute("relatedDirId"))
                                End If

                                If nloc > 0 Then
                                    maintainDirectoryRelation(nloc, savedId)
                                End If
                            Next

                        Case objectTypes.ContentStructure

                            Dim oContentInstance As XmlElement
                            For Each oContentInstance In oInstance.SelectNodes("Contents/instance")
                                'lets get an id if we are updating a record with a foriegn Ref
                                'return the object type from the table name
                                Dim cTableName2 As String = oContentInstance.FirstChild.Name
                                Dim oTblName2 As TableNames
                                For Each oTblName2 In [Enum].GetValues(GetType(objectTypes))
                                    If oTblName2.ToString = cTableName2 Then Exit For
                                Next
                                Dim oObjType2 As New objectTypes
                                oObjType2 = oTblName2

                                Dim nContentId As Long = 0
                                Dim fRefElmt As XmlElement = oContentInstance.SelectSingleNode(getTable(oObjType2) & "/" & getFRef(oObjType2))
                                Dim fRef As String
                                If Not fRefElmt Is Nothing Then
                                    fRef = fRefElmt.InnerText
                                    If fRef <> "" Then
                                        nContentId = myWeb.moDbHelper.getObjectByRef(getTable(oObjType2), getKey(oObjType2), getFRef(oObjType2), oObjType2, fRef)
                                    End If
                                End If

                                nContentId = setObjectInstance(oObjType2, oContentInstance, nContentId)
                                processInstanceExtras(nContentId, oContentInstance, bResetLocations, bOrphan)

                            Next

                    End Select

                End If
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "processInstanceExtras", ex, cProcessInfo))
                Return 0
            End Try
        End Function

        ''' <summary>
        '''   <para>This deletes all content locations for an item of content.</para>
        ''' </summary>
        ''' <param name="nContentId">The content ID</param>
        ''' <param name="cTable">Optional - table name for content locations - useful to pass thru to save extra DB calls</param>
        ''' <returns>The number of items deleted.</returns>
        ''' <remarks>Note - we changed this from using DeleteObject as it's not conducive for bulk deletes.</remarks>
        Public Function RemoveContentLocations(ByVal nContentId As Integer, Optional ByVal cTable As String = "") As Integer
            PerfMon.Log("DBHelper", "RemoveContentLocations", "nContentId=" & nContentId)
            Dim i As Integer = 0
            Try

                ' Normally the table name will be passed thru for optimisation, but just in case...
                If String.IsNullOrEmpty(cTable) Then cTable = getTable(objectTypes.ContentLocation)

                Dim cSQL As String = "DELETE FROM " & cTable & " WHERE nContentId = " & nContentId
                i = Me.ExeProcessSql(cSQL)
                Return i
            Catch ex As Exception
                Return i
            End Try
        End Function

        'Public Function RemoveContentLocations(ByVal nContentId As Integer) As Integer
        '    PerfMon.Log("DBHelper", "RemoveContentLocations", "nContentId=" & nContentId)
        '    Dim i As Integer = 0
        '    Try

        '        Dim cSQL As String = "SELECT tblContentLocation.nContentLocationKey FROM tblContentLocation INNER JOIN tblContentStructure ON tblContentLocation.nStructId = tblContentStructure.nStructKey"
        '        cSQL &= " WHERE nContentId = " & nContentId
        '        Dim oDR As SqlClient.SqlDataReader = myWeb.moDbHelper.getDataReader(cSQL)
        '        Do While oDR.Read
        '            myWeb.moDbHelper.DeleteObject(Web.dbHelper.objectTypes.ContentLocation, oDR(0))
        '            i += 1
        '        Loop
        '        oDR.Close()
        '        Return i
        '    Catch ex As Exception
        '        Return i
        '    End Try
        'End Function

        Public Function setContentLocationByRef(ByVal cStructFRef As String, ByVal nContentId As Integer, ByVal bPrimary As Integer, ByVal bCascade As Integer) As Integer

            PerfMon.Log("DBHelper", "setContentLocationByRef", "ref=" & cStructFRef & " nContentId=" & nContentId)
            Dim cProcessInfo As String = ""

            Try
                Dim nID As String = "" 'myWeb.moDbHelper.getKeyByNameAndSchema(Web.dbHelper.objectTypes.ContentStructure, "", cStructName)
                nID = myWeb.moDbHelper.getObjectByRef(Web.dbHelper.objectTypes.ContentStructure, cStructFRef)
                If nID = "" Then nID = 0
                If nID > 0 Then

                    Return myWeb.moDbHelper.setContentLocation(nID, nContentId, IIf(bPrimary = 1, True, False), bCascade)
                Else
                    Return 0
                End If
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "setContentLocationByRef", ex, cProcessInfo))
                Return 0
            End Try
        End Function


        Public Function setContentLocationByRef(ByVal cStructFRef As String, ByVal nContentId As Integer, ByVal bPrimary As Integer, ByVal bCascade As Integer, ByVal cPosition As String) As Integer

            PerfMon.Log("DBHelper", "setContentLocationByRef", "ref=" & cStructFRef & " nContentId=" & nContentId)
            Dim cProcessInfo As String = ""

            Try
                Dim nID As String = "" 'myWeb.moDbHelper.getKeyByNameAndSchema(Web.dbHelper.objectTypes.ContentStructure, "", cStructName)
                nID = getObjectByRef(Web.dbHelper.objectTypes.ContentStructure, cStructFRef)
                If nID = "" Then nID = 0
                If nID > 0 Then

                    Return setContentLocation(nID, nContentId, IIf(bPrimary = 1, True, False), bCascade, False, cPosition, True)
                Else
                    Return 0
                End If
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "setContentLocationByRef", ex, cProcessInfo))
                Return 0
            End Try
        End Function


        Public Function setContentRelationByRef(ByVal nContentId As Integer, ByVal cContentFRef As String, Optional ByVal b2Way As Boolean = False, Optional ByVal rType As String = "", Optional ByVal bHaltRecursion As Boolean = False) As String
            Try

                Dim nRefId As Integer = GetDataValue("SELECT nContentKey, cContentForiegnRef FROM tblContent WHERE (cContentForiegnRef = '" & cContentFRef & "')", , , 0)

                If Not nRefId = 0 Then
                    Return insertContentRelation(nContentId, nRefId, b2Way, rType, bHaltRecursion)
                Else
                    Return "Not Found"
                End If
            Catch ex As Exception
                Return " Relation Error " & ex.Message
            End Try


        End Function



        Public Function importShippingLocations(ByRef oXml As XmlDocument) As String
            PerfMon.Log("DBHelper", "importShippingLocations")
            Dim cSql As String
            Dim oShipLoc As XmlElement
            Dim oShipLocClone As XmlElement
            Dim oInstance As XmlElement
            Dim oElmt As XmlElement
            Dim cKey As String
            Dim cParKey As String
            Dim cProcessInfo As String = ""
            Try

                ' Clear out the current shipping locations and relations
                cSql = "DELETE tblAudit FROM tblAudit a INNER JOIN tblCartShippingLocations c ON a.nAuditKey = c.nAuditId;" _
                     & "DELETE FROM tblCartShippingLocations;" _
                     & "DELETE tblAudit FROM tblAudit a INNER JOIN tblCartShippingRelations c ON a.nAuditKey = c.nAuditId;" _
                     & "DELETE FROM tblCartShippingRelations;" _
                     & "INSERT INTO tblLookup (cLkpKey,cLkpValue,cLkpCategory) VALUES ('ShippingLocations',1,'ImportLock');"
                ExeProcessSql(cSql)

                ' Run through each record
                For Each oShipLoc In oXml.SelectNodes("//tblCartShippingLocations")
                    oInstance = oXml.CreateElement("instance")
                    oShipLocClone = oShipLoc.CloneNode(True)

                    ' Remove the Unique ID
                    oElmt = oShipLocClone.SelectSingleNode("nLocationKey")
                    If Not (oElmt Is Nothing) Then oShipLocClone.RemoveChild(oElmt)

                    ' Remove the Audit Id
                    oElmt = oShipLocClone.SelectSingleNode("nAuditId")
                    If Not (oElmt Is Nothing) Then oShipLocClone.RemoveChild(oElmt)

                    oInstance.AppendChild(oShipLocClone)

                    ' Save the instance
                    cKey = setObjectInstance(objectTypes.CartShippingLocation, oInstance)
                    addNewTextNode("newKey", oShipLoc, cKey)
                Next

                ' Now go through and reconcile the old parent keys
                For Each oShipLoc In oXml.SelectNodes("//tblCartShippingLocations")
                    ' Get the parent key
                    cKey = oShipLoc.SelectSingleNode("newKey").InnerText
                    cParKey = oShipLoc.SelectSingleNode("nLocationParId").InnerText

                    ' Get the new parentkey
                    oElmt = oXml.SelectSingleNode("//tblCartShippingLocations[nLocationKey=" & cParKey & "]")
                    If Not (oElmt Is Nothing) Then
                        oElmt = oElmt.SelectSingleNode("newKey")
                        If Not (oElmt Is Nothing) Then
                            cSql = "UPDATE tblCartShippingLocations SET nLocationParId=" & oElmt.InnerText & " WHERE nLocationKey=" & cKey
                            ExeProcessSql(cSql)
                        End If
                    End If
                Next

                Return ""
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "importShippingLocations", ex, cProcessInfo))
                Return ""
            End Try
        End Function

        Public Function importShippingLocations2(ByRef oXml As XmlDocument) As String
            PerfMon.Log("DBHelper", "importShippingLocations")
            Dim cSql As String

            Dim cProcessInfo As String = ""
            Try

                ' Clear out the current shipping locations and relations
                cSql = "DELETE tblAudit FROM tblAudit a INNER JOIN tblCartShippingLocations c ON a.nAuditKey = c.nAuditId;" _
                     & "DELETE FROM tblCartShippingLocations;" _
                     & "DELETE tblAudit FROM tblAudit a INNER JOIN tblCartShippingRelations c ON a.nAuditKey = c.nAuditId;" _
                     & "DELETE FROM tblCartShippingRelations;" _
                     & "INSERT INTO tblLookup (cLkpKey,cLkpValue,cLkpCategory) VALUES ('ShippingLocations',1,'ImportLock');"
                ExeProcessSql(cSql)
                Dim nCount As Integer = importShippingLocationDrillDown(oXml.DocumentElement.FirstChild, 0)

                Return nCount


            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "importShippingLocations2", ex, cProcessInfo))
                Return ""
            End Try
        End Function

        Private Function importShippingLocationDrillDown(ByVal oElmt As XmlElement, ByVal nParId As Integer) As Integer
            Dim nCount As Integer
            Try
                Dim oCopy As XmlElement = oElmt.CloneNode(True)
                Dim oChild As XmlElement
                For Each oChild In oCopy.SelectNodes("instance")
                    oChild.ParentNode.RemoveChild(oChild)
                Next
                oCopy.SelectSingleNode("tblCartShippingLocations/cLocationNameFull").InnerText = Replace(oCopy.SelectSingleNode("tblCartShippingLocations/cLocationNameFull").InnerText, "&amp;", "and")
                oCopy.SelectSingleNode("tblCartShippingLocations/cLocationNameShort").InnerText = Replace(oCopy.SelectSingleNode("tblCartShippingLocations/cLocationNameShort").InnerText, "&amp;", "and")
                oCopy.SelectSingleNode("tblCartShippingLocations/nLocationParId").InnerText = nParId
                Dim nId As Integer = setObjectInstance(objectTypes.CartShippingLocation, oCopy)

                If nId > 0 Then
                    nCount += 1

                    For Each oChild In oElmt.SelectNodes("instance")

                        nCount += importShippingLocationDrillDown(oChild, nId)
                    Next
                End If

                Return nCount

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "importShippingLocationDrillDown", ex, ""))
                Return nCount
            End Try
        End Function

        Public Function GetContentDetailXml(Optional ByVal nArtId As Long = 0) As XmlElement
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

            Try

                ' If requested, we need to make sure that the content we are looking for doesn't belong to a page
                ' that the user is not allowed to access.
                ' We can review the current menu structure xml instead of calling the super slow permissions functions.

                If nArtId > 0 Then
                    sProcessInfo = "loading content" & nArtId

                    sFilterSql &= GetStandardFilterSQLForContent()

                    oRoot = moPageXml.CreateElement("ContentDetail")
                    sSql = "select c.nContentKey as id, cContentForiegnRef as ref, dbo.fxn_getContentParents(c.nContentKey) as parId, cContentName as name, cContentSchemaName as type, cContentXmlDetail as content, a.dpublishDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, a.nStatus as status from tblContent c "
                    sSql &= "inner join tblAudit a on c.nAuditId = a.nAuditKey  "
                    'sSql &= "inner join tblContentLocation CL on c.nContentKey = CL.nContentId "
                    sSql &= "where c.nContentKey = " & nArtId & sFilterSql & " "
                    'sSql &= "and CL.nStructId = " & mnPageId

                    oDs = GetDataSet(sSql, "Content", "ContentDetail")

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

                    'Need to check the content is found on the current page.


                    If oDs.Tables(0).Rows.Count > 0 Then

                        oRoot.InnerXml = Replace(oDs.GetXml, "xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""", "")
                        For Each oNode In oRoot.SelectNodes("/ContentDetail/Content")
                            oElmt = oNode
                            sContent = oElmt.InnerText

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

                                ' addRelatedContent(oNode, nArtId, False)

                            End If

                        Next

                        Return oElmt

                    Else
                        'Just a page no detail requested
                        Return Nothing
                    End If

                End If

            Catch ex As Exception
                'returnException(mcModuleName, "getContentDetailXml", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "GetContentDetailXml", ex, sProcessInfo))
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
        Public Function GetStandardFilterSQLForContent(Optional ByVal bPrecedingAND As Boolean = True, Optional ByVal bAdminMode As Boolean = False, Optional ByVal PagePerm As PermissionLevel = PermissionLevel.Open) As String

            PerfMon.Log("Web", "GetStandardFilterSQLForContent")

            Dim sFilterSQL As String = ""

            Try

                ' Only check for permissions if not in Admin Mode
                If Not bAdminMode Then

                    ' Set the default filter
                    sFilterSQL = "a.nStatus = 1 "

                    If gbVersionControl _
                        AndAlso Me.mnUserId > 0 Then
                        ' Version control is on
                        ' Check the page permission
                        If dbHelper.CanAddUpdate(PagePerm) Then

                            ' User has update permissions - now can they only have control over their own items
                            If dbHelper.CanOnlyUseOwn(PagePerm) Then

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
                    sFilterSQL &= " and (a.dPublishDate is null or a.dPublishDate = 0 or a.dPublishDate <= " & Eonic.Tools.Database.SqlDate(Now) & " )"
                    sFilterSQL &= " and (a.dExpireDate is null or a.dExpireDate = 0 or a.dExpireDate >= " & Eonic.Tools.Database.SqlDate(Now) & " )"
                End If

                Return sFilterSQL

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "GetStandardFilterSQLForContent", ex, sFilterSQL))
                Return ""
            End Try
        End Function

        Public Sub AddDataSetToContent(ByRef oDs As DataSet, ByRef oContent As XmlElement, Optional ByVal nCurrentPageId As Long = 0, Optional ByVal bIgnoreDuplicates As Boolean = False, Optional ByVal cAddSourceAttribute As String = "", Optional ByRef dExpireDate As DateTime = Nothing, Optional ByRef dUpdateDate As DateTime = Nothing, Optional ByVal bAllowRecursion As Boolean = True)
            Try

                ' Calculate the maxdepth - it can be overrided by ShowRelatedBriefDepth
                Dim cShowRelatedBriefDepth As String = goConfig("ShowRelatedBriefDepth") & ""
                Dim nMaxDepth As Integer = 1
                If Not (String.IsNullOrEmpty(cShowRelatedBriefDepth)) _
                    AndAlso IsNumeric(cShowRelatedBriefDepth) Then
                    nMaxDepth = CInt(cShowRelatedBriefDepth)
                End If

                AddDataSetToContent(oDs, oContent, nCurrentPageId, bIgnoreDuplicates, cAddSourceAttribute, dExpireDate, dUpdateDate, bAllowRecursion, nMaxDepth)

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "AddDataSetToContent", ex, ""))
            End Try
        End Sub


        Public Sub AddDataSetToContent(ByRef oDs As DataSet, ByRef oContent As XmlElement, ByVal nCurrentPageId As Long, ByVal bIgnoreDuplicates As Boolean, ByVal cAddSourceAttribute As String, ByRef dExpireDate As DateTime, ByRef dUpdateDate As DateTime, ByVal bAllowRecursion As Boolean, ByVal nMaxDepth As Integer)
            PerfMon.Log("DBHelper", "AddDataSetToContent - Start")
            Dim sProcessInfo As String = ""

            Dim oNode As XmlNode
            Dim oElmt2 As XmlElement
            Dim oElmt3 As XmlElement

            Dim sNodeName As String = ""
            Dim sContentText As String = ""

            Try

                Dim aNodeTypes() As String
                Dim cShowRelatedBriefContentTypes As String = goConfig("ShowRelatedBriefContentTypes") & ""
                Dim cShowSpecificContentTypes As String = ""
                Dim oXml As XmlDocument = ContentDataSetToXml(oDs, dUpdateDate)
                Dim oXml2 As XmlNode = oContent.OwnerDocument.ImportNode(oXml.DocumentElement, True)

                Dim n As Long

                For Each oNode In oXml2.SelectNodes("Content")
                    n = n + 1
                    oElmt2 = SimpleTidyContentNode(oNode, cAddSourceAttribute, dExpireDate, dUpdateDate)

                    sNodeName = oElmt2.GetAttribute("name")
                    Dim nNodeId As Integer = oElmt2.GetAttribute("id")
                    Dim cNodeType As String = oElmt2.GetAttribute("type")
                    Dim cRelationType As String = oElmt2.GetAttribute("rtype")
                    Dim cPosition As String = oElmt2.GetAttribute("position")

                    'ensure this items is not allready in this location....
                    If oContent.SelectSingleNode("*[@id='" & nNodeId & "']") Is Nothing Then

                        'Allow related content to be added to the specified types always (Module is default)
                        If oElmt2.HasAttribute("showRelated") Then
                            cShowSpecificContentTypes += "," & oElmt2.GetAttribute("showRelated")
                        End If

                        aNodeTypes = Split(IIf(String.IsNullOrEmpty(cShowRelatedBriefContentTypes), "module", cShowRelatedBriefContentTypes.ToString.ToLower), ",")

                        If bAllowRecursion And Not (myWeb.ibIndexMode And Not myWeb.ibIndexRelatedContent) _
                            And (cNodeType = "Module" _
                            OrElse Array.IndexOf(aNodeTypes, cNodeType.ToLower) >= aNodeTypes.GetLowerBound(0) _
                            OrElse oContent.SelectNodes("ancestor::ContentDetail").Count <> 0) _
                            OrElse cShowSpecificContentTypes <> "" Then

                            ' Related Content in Brief
                            '   Avoiding recursion.
                            '      We can take two approaches:
                            '           1.  Maintain a list of IDs currently visited down this branch of the node tree. (Prevents repetition)
                            '           2.  Implement depth limiter (Prevents branching).
                            '
                            '   Depth limitation is acheived by reading the number of items in the List

                            Dim cAvoidRecursionList As String = oContent.GetAttribute("avoidRecursionList")
                            Dim nDepth As Integer = IIf(String.IsNullOrEmpty(cAvoidRecursionList), 0, cAvoidRecursionList.Split(",").GetLength(0))

                            If nDepth < nMaxDepth Then
                                If nDepth > 0 Then
                                    ' We need to pass on a curent list of content ids down the related content tree to avoid recursion
                                    ' Because this stage happens before oElmt2 is added to the content we can't simply look at what has been added
                                    ' So we use this temporary attribute and remove it later
                                    oElmt2.SetAttribute("avoidRecursionList", cAvoidRecursionList)
                                End If
                                ' Trevors Bulk Content Relations Experiment
                                oElmt2.SetAttribute("processForRelatedContent", "true")
                                'addRelatedContent(oElmt2, nNodeId, myWeb.mbAdminMode, cShowSpecificContentTypes)
                            End If
                            oElmt2.RemoveAttribute("avoidRecursionList")
                        End If

                        'Content[@name='" & sNodeName & "']"
                        'If Not (oContent.SelectSingleNode("Content[@name='" & sNodeName & "' and @type='" & cNodeType & "' and @id='" & nNodeId & "']") Is Nothing) And Not bIgnoreDuplicates Then
                        If Not (oContent.SelectSingleNode("Content[@name=" & Eonic.Tools.Xml.GenerateConcatForXPath(sNodeName) & " and @position='" & cPosition & "']") Is Nothing) And Not bIgnoreDuplicates And Not sNodeName = "" Then
                            'replace node.
                            oElmt3 = oContent.SelectSingleNode("Content[@name=" & Eonic.Tools.Xml.GenerateConcatForXPath(sNodeName) & " and @position='" & cPosition & "']")
                            'we will only replace more "sytem" items
                            'If _
                            'LCase(oElmt3.GetAttribute("type")) = "plaintext" Or _
                            'LCase(oElmt3.GetAttribute("type")) = "formattedtext" Or _
                            'LCase(oElmt3.GetAttribute("type")) = "image" Or _
                            'LCase(oElmt3.GetAttribute("type")) = "flashmovie" Then

                            If CLng("0" & oElmt3.GetAttribute("parId")) > 0 Then
                                sProcessInfo = oElmt2.OuterXml
                                Dim primaryParId As Long

                                'fix for comma separated parId's
                                If InStr(oElmt2.GetAttribute("parId"), ",") > 0 Then
                                    primaryParId = Left(oElmt2.GetAttribute("parId"), InStr(oElmt2.GetAttribute("parId"), ",") - 1)
                                Else
                                    primaryParId = CLng("0" & oElmt2.GetAttribute("parId"))
                                End If

                                Dim primaryParId3 As Long
                                If InStr(oElmt3.GetAttribute("parId"), ",") > 0 Then
                                    primaryParId3 = Left(oElmt3.GetAttribute("parId"), InStr(oElmt3.GetAttribute("parId"), ",") - 1)
                                Else
                                    primaryParId3 = CLng("0" & oElmt3.GetAttribute("parId"))
                                End If

                                If CheckIfAncestorPage(oElmt3.OwnerDocument, primaryParId, primaryParId3) Then
                                    oContent.ReplaceChild(oElmt2, oElmt3)
                                Else
                                    oContent.AppendChild(oElmt2)
                                End If
                            Else
                                oContent.AppendChild(oElmt2)
                            End If
                            'End If
                        Else
                            oContent.AppendChild(oElmt2)
                        End If

                    End If

                Next

                PerfMon.Log("DBHelper", "AddDataSetToContent " & n & " items of content - End")

                ' Trevors Bulk Content Relations Experiment
                If goConfig("FinalAddBulk") <> "on" Then
                    addBulkRelatedContent(oContent, dUpdateDate, nMaxDepth)
                End If

            Catch ex As Exception

                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "AddDataSetToContent", ex, sProcessInfo))

            End Try
        End Sub

        Public Function ContentDataSetToXml(ByRef oDs As DataSet, Optional ByRef dUpdateDate As DateTime = Nothing) As XmlDocument
            PerfMon.Log("DBHelper", "ContentDataSetToXml - Start")
            Dim sProcessInfo As String = ""

            Dim sNodeName As String = ""
            Dim sContentText As String = ""

            Try
                'If nCurrentPageId = 0 Then nCurrentPageId = gnPageId

                'map the feilds to columns
                oDs.Tables(0).Columns("id").ColumnMapping = Data.MappingType.Attribute

                If oDs.Tables(0).Columns.Contains("parID") Then
                    oDs.Tables(0).Columns("parId").ColumnMapping = Data.MappingType.Attribute
                End If

                If oDs.Tables(0).Columns.Contains("locId") Then
                    oDs.Tables(0).Columns("locId").ColumnMapping = Data.MappingType.Attribute
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

                    If Not oDs.Tables(0).Columns("position") Is Nothing Then
                        oDs.Tables(0).Columns("position").ColumnMapping = Data.MappingType.Attribute
                    End If

                    If Not oDs.Tables(0).Columns("update") Is Nothing Then
                        If Not dUpdateDate = Nothing Then
                            .Columns("update").ColumnMapping = Data.MappingType.Attribute
                        End If
                    End If
                    .Columns("content").ColumnMapping = Data.MappingType.SimpleContent
                End With

                oDs.EnforceConstraints = False
                'convert to Xml Dom
                Dim oXml As XmlDocument = New XmlDocument
                oXml.LoadXml(oDs.GetXml)
                oXml.PreserveWhitespace = False

                PerfMon.Log("DBHelper", "ContentDataSetToXml - End")

                Return oXml

            Catch ex As Exception

                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "ContentDataSetToXml", ex, sProcessInfo))
                Return Nothing

            End Try
        End Function

        Protected Friend Function SimpleTidyContentNode(ByRef oContent As XmlElement, Optional ByVal cAddSourceAttribute As String = "", Optional ByRef dExpireDate As DateTime = Nothing, Optional ByRef dUpdateDate As DateTime = Nothing) As XmlElement
            Dim sProcessInfo As String = ""
            Dim oElmt As XmlElement
            Dim oAttr As XmlAttribute
            Dim sNodeName As String = ""
            Dim sContentText As String = ""

            Try
                'PerfMon.Log("DBHelper", "SimpleTidyContentNode")

                'oElmt = oContent.OwnerDocument.ImportNode(oContent, True)
                oElmt = oContent
                sNodeName = oElmt.GetAttribute("name")
                'perfMon.Log("DBHelper", "SimpleTidyContentNode - Import")

                'make sure the page expiredate is fed back on the contents expiry
                If (Not dExpireDate = Nothing) And oElmt.GetAttribute("expire") <> "" Then
                    If dExpireDate > CDate(oElmt.GetAttribute("expire")) Then
                        dExpireDate = CDate(oElmt.GetAttribute("expire"))
                    End If
                End If

                If (Not dUpdateDate = Nothing) And oElmt.GetAttribute("update") <> "" Then
                    If dUpdateDate < CDate(oElmt.GetAttribute("update")) Then
                        dUpdateDate = CDate(oElmt.GetAttribute("update"))
                    End If
                End If

                If cAddSourceAttribute <> "" Then oElmt.SetAttribute("source", cAddSourceAttribute)

                'change the xhtml string to xml
                'PerfMon.Log("DBHelper", "SimpleTidyContentNode - ConvertText")
                sContentText = oElmt.InnerText
                Try
                    oElmt.InnerXml = sContentText
                Catch
                    'try removing the declaration
                    Try
                        oElmt.InnerXml = Replace(sContentText, "<?xml version=""1.0"" encoding=""UTF-8""?>", "")
                    Catch
                        oElmt.InnerText = sContentText
                    End Try

                End Try
                'PerfMon.Log("DBHelper", "SimpleTidyContentNode - EndConvertText")
                ' Draw the content node back to the main node.
                Dim oContentElmt As XmlElement = oElmt.SelectSingleNode("Content")
                If Not oContentElmt Is Nothing Then
                    If oContentElmt.GetAttribute("subType") <> "" Then
                        oElmt.SetAttribute("subType", oContentElmt.GetAttribute("subType"))
                    End If
                    'set any attributes on the content node
                    For Each oAttr In oElmt.SelectNodes("Content/@*")
                        If oElmt.GetAttribute(oAttr.Name) = "" Then
                            oElmt.SetAttribute(oAttr.Name, oAttr.InnerText)
                        End If
                    Next
                    oElmt.InnerXml = oContentElmt.InnerXml
                End If
                'PerfMon.Log("DBHelper", "SimpleTidyContentNode - Done")
                Return oElmt

            Catch ex As Exception

                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "SimpleTidyContentNode", ex, sProcessInfo))
                Return Nothing

            End Try

        End Function

        Public Sub addRelatedContent(ByRef oContentElmt As XmlElement, ByVal nParentId As Integer, ByVal bAdminMode As Boolean, Optional ByVal specificTypes As String = "")
            'TS - This is no longer used replaced by addBulkRelatedContent

            PerfMon.Log("DBHelper", "addRelatedContent - Start")
            'adds related content to passed xmlNode.
            'Page/ContentDetail/Content
            If oContentElmt Is Nothing Or (myWeb.ibIndexMode And Not myWeb.ibIndexRelatedContent) Then Exit Sub

            Dim sSql As String
            Dim sFilterSql As String = ""
            Dim sSql2 As String = ""
            Dim sNodeName As String = ""
            Dim sContent As String = ""
            'Dim rTypePresent As Boolean = False
            Dim sSepcificTypeSQL As String = ""
            ''specificTypes = "SKU"

            Dim sProcessInfo As String = "building the related Content XML"

            Try

                ' Recursive loop avoidance
                ' Maintain a list of content ids in related content to stop it recursing for two way relationships when sending back to AddContentToDataset and vice versa.
                ' Start by adding the current id
                Dim cRelatedContentList As String = oContentElmt.GetAttribute("avoidRecursionList")

                ' Add the id if it is not null or zero
                If Not (String.IsNullOrEmpty(oContentElmt.GetAttribute("id"))) _
                    AndAlso IsNumeric(oContentElmt.GetAttribute("id")) _
                    AndAlso CLng(oContentElmt.GetAttribute("id")) > 0 Then
                    If Not (String.IsNullOrEmpty(cRelatedContentList)) Then cRelatedContentList &= ","
                    cRelatedContentList &= oContentElmt.GetAttribute("id")
                End If

                ' If there is a list, add a filter.
                If Not (String.IsNullOrEmpty(cRelatedContentList)) Then
                    oContentElmt.SetAttribute("avoidRecursionList", cRelatedContentList)
                    sFilterSql &= " and NOT(c.nContentKey IN (" & cRelatedContentList & "))"
                End If

                If specificTypes <> "" Then
                    Dim aSpecificTypes() As String = Split(specificTypes, ",")
                    Dim i As Integer
                    For i = 0 To UBound(aSpecificTypes)
                        If i > 0 Then sSepcificTypeSQL &= ","
                        sSepcificTypeSQL = "'" & aSpecificTypes(i) & "'"
                    Next
                    sFilterSql &= " and c.cContentSchemaName IN (" & sSepcificTypeSQL & ")"
                End If

                ' Build the filter SQL
                sFilterSql &= myWeb.GetStandardFilterSQLForContent()


                'sSql = "select c.nContentKey as id, cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire, x.nDisplayOrder as displayorder, (select TOP 1 CL2.nStructId from tblContentLocation CL2 where CL2.nContentId=c.nContentKey and CL2.bPrimary = 1) as parId " & _

                Dim cSQL As String = "SELECT * FROM tblContentRelation"
                Dim oDs_2 As New DataSet
                oDs_2 = GetDataSet(cSQL, "Content")
                If (oDs_2.Tables("Content").Columns.Contains("cRelationType")) Then
                    sSql = "select c.nContentKey as id, cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cRelationType as rtype, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire, a.nInsertDirId as owner, x.nDisplayOrder as displayorder, dbo.fxn_getContentParents(c.nContentKey) as parId " & _
                            " FROM tblContent c INNER JOIN" & _
                            " tblAudit a ON c.nAuditId = a.nAuditKey INNER JOIN" & _
                            " tblContentRelation x ON c.nContentKey = x.nContentChildId" & _
                            " WHERE (x.nContentParentId = " & nParentId & ")"
                Else
                    sSql = "select c.nContentKey as id, cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire, a.nInsertDirId as owner, x.nDisplayOrder as displayorder, dbo.fxn_getContentParents(c.nContentKey) as parId " & _
                            " FROM tblContent c INNER JOIN" & _
                            " tblAudit a ON c.nAuditId = a.nAuditKey INNER JOIN" & _
                            " tblContentRelation x ON c.nContentKey = x.nContentChildId" & _
                            " WHERE (x.nContentParentId = " & nParentId & ")"
                End If


                sSql = sSql & sFilterSql & " order by type, x.nDisplayOrder"

                Dim oDs As DataSet = New DataSet
                oDs = GetDataSet(sSql, "Content", "Contents")

                oDs.Tables(0).Columns("displayorder").ColumnMapping = Data.MappingType.Attribute

                PerfMon.Log("DBHelper", "AddDataSetToContent - AddRelated - " & sSql)


                If Not oContentElmt.ParentNode Is Nothing Then
                    If oContentElmt.ParentNode.Name = "ContentDetail" Then
                        ' Calculate the maxdepth for contentDetial
                        Dim cShowRelatedBriefDepth As String = goConfig("ShowRelatedDetailedDepth") & ""
                        If CInt("0" & oContentElmt.GetAttribute("relatedDepth")) > 0 Then
                            cShowRelatedBriefDepth = oContentElmt.GetAttribute("relatedDepth")
                        End If
                        Dim nMaxDepth As Integer = 1
                        If Not (String.IsNullOrEmpty(cShowRelatedBriefDepth)) _
                            AndAlso IsNumeric(cShowRelatedBriefDepth) Then
                            nMaxDepth = CInt(cShowRelatedBriefDepth)
                        End If
                        AddDataSetToContent(oDs, oContentElmt, nParentId, 0, "", #12:00:00 AM#, #12:00:00 AM#, True, nMaxDepth)
                    Else
                        AddDataSetToContent(oDs, oContentElmt, nParentId)
                    End If
                Else
                    AddDataSetToContent(oDs, oContentElmt, nParentId)
                End If

                PerfMon.Log("DBHelper", "addRelatedContent - END")

                '!!! we really should not call this recursively !!!!!
                'TS. Now we don't this is not being called.

                'Nath: Warning this caused errors with Paramo's displaying of related items!
                'SKUs were getting deleted because they no longer had a discount node!
                'Dim oDiscounts As New Eonic.Web.Cart.Discount(myWeb)
                'oDiscounts.getAvailableDiscounts(oContentElmt)

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "addRelatedContent", ex, sProcessInfo))
            End Try

        End Sub
        ''' <summary>
        ''' This adds related content to child content nodes, given a parent node.
        ''' The scope of which child nodes are processed is pre-determined by the presence of
        ''' the @processForRelatedContent=true attribute in the child node.
        ''' Child nodes are then recursively processed down their related content to a 
        ''' depth determined by nMaxDepth.
        ''' Related content nodes may be filtered by type if their parent node has an
        ''' attribute of showRelated.
        ''' </summary>
        ''' <param name="oContentParent">The initial starting node, whose child nodes will be considered for adding related content to</param>
        ''' <param name="dUpdateDate">The date to get content by. E.g. find related content on such and such a day</param>
        ''' <param name="nMaxDepth">The levels of related content to try to retrieve.</param>
        ''' <remarks></remarks>
        Public Sub addBulkRelatedContent(ByRef oContentParent As XmlElement, Optional ByRef dUpdateDate As DateTime = Nothing, Optional ByVal nMaxDepth As Integer = 1)

            PerfMon.Log("DBHelper", "addBulkRelatedContent")
            'adds related content to passed xmlNode.
            'Page/ContentDetail/Content

            Dim sSql As String
            Dim sFilterSql As String = ""
            Dim sSql2 As String = ""
            Dim sNodeName As String = ""
            Dim sContent As String = ""
            Dim specificTypeSQL As String = ""
            Dim sProcessInfo As String = "building the related Content XML"
            Dim oElmt As XmlElement
            Dim sContentLevelxPath As String = ""
            ' Dim specificTypes As String = goConfig("ShowRelatedBriefContentTypes") & ""
            Dim i As Integer

            ' This is not cleared with each depth iteration as 
            ' a Content node for a given @id should have the same @showRelated 
            ' value regardless of how often or at what depth that content appears.
            Dim contentTypeFilters As New Dictionary(Of String, String)

            Try

                ' The iteration in inferred by an xPath of ParentNode/Content[@processForRelatedContent='true']/Content/Content/Content etc.
                ' where the iteration depth is reflected by the Content nodes.
                ' Note that the base child content nodes will have an attribute of @processForRelatedContent=true which sets the scope.
                Do While nMaxDepth > 0

                    ' First we get a list of the relations for the content that is in scope
                    ' Note: we ensure that the child node is not a Module (as some two way relationships may historically exist)
                    Dim relationTypeColumn As String = IIf(checkTableColumnExists("tblContentRelation", "cRelationType"), ",cRelationType AS rtype ", "")

                    sSql = "SELECT r.nContentParentId as parId, r.nContentChildId as id, r.nDisplayOrder as displayorder " & relationTypeColumn & ", parentContent.cContentSchemaName as parType, childContent.cContentSchemaName as childType " & _
                    " FROM tblContentRelation r " & _
                    " inner join tblContent parentContent on (r.nContentParentId = parentContent.nContentKey)" & _
                    " inner join tblContent childContent on (r.nContentChildId = childContent.nContentKey)" & _
                    " where childContent.cContentSchemaName <> 'Module' AND r.nContentParentId IN ("
                    Dim cRelatedIds As String = ""
                    For Each oElmt In oContentParent.SelectNodes("Content[@processForRelatedContent='true']" & sContentLevelxPath)

                        ' Add to the related IDs
                        cRelatedIds &= oElmt.GetAttribute("id") & ","

                        ' Build a content type filter for this content's child nodes
                        ' if the content has a filter, indicated by the repsence of a showRelated attribute
                        ' Note: we only build these content filters if the content is Brief content - this
                        ' can be determined by looking to see if it has an ancestor of ContentDetail
                        If oContentParent.SelectSingleNode("ancestor::ContentDetail") Is Nothing _
                               AndAlso Not (String.IsNullOrEmpty(oElmt.GetAttribute("showRelated").Trim)) _
                               AndAlso Not contentTypeFilters.ContainsKey(oElmt.GetAttribute("id")) Then
                            contentTypeFilters.Add(oElmt.GetAttribute("id"), oElmt.GetAttribute("showRelated"))
                        End If


                        ' Cleanup the recursion attribute
                        'oElmt.RemoveAttribute("processForRelatedContent")

                    Next
                    cRelatedIds = cRelatedIds.Trim(",")
                    sSql += cRelatedIds & ") order by r.nContentParentId, r.nDisplayOrder"

                    If Not String.IsNullOrEmpty(cRelatedIds) Then


                        ' Get the relations data and transform it into XML 
                        Dim oDs1 As New DataSet
                        PerfMon.Log("DBHelper", "addBulkRelatedContent - GetData")
                        oDs1 = GetDataSet(sSql, "Relation")
                        PerfMon.Log("DBHelper", "addBulkRelatedContent - GetDataEND - " & sSql)
                        With oDs1.Tables(0)
                            .Columns("parId").ColumnMapping = Data.MappingType.Attribute
                            .Columns("id").ColumnMapping = Data.MappingType.Attribute
                            .Columns("displayorder").ColumnMapping = Data.MappingType.Attribute
                            If relationTypeColumn <> "" Then
                                .Columns("rtype").ColumnMapping = Data.MappingType.Attribute
                            End If
                            .Columns("parType").ColumnMapping = Data.MappingType.Attribute
                            .Columns("childType").ColumnMapping = Data.MappingType.Attribute
                        End With

                        Dim oRelationsXml As XmlDocument = New XmlDocument
                        oRelationsXml.LoadXml(oDs1.GetXml)
                        oRelationsXml.PreserveWhitespace = False



                        ' ==========================================================
                        '  FILTER THE RELATIONS BY RESTRICTED CHILD CONTENT TYPE
                        ' ==========================================================
                        For Each contentTypeFilterKey As String In contentTypeFilters.Keys

                            Dim typeFilter As String = ""
                            Dim filterXPath As String = ""

                            ' Build the Xpath filter
                            Dim filteredTypes As String() = contentTypeFilters(contentTypeFilterKey).Split(",")
                            For index As Integer = 0 To filteredTypes.Length - 1
                                filteredTypes(index) = String.Format("@childType='{0}'", filteredTypes(index))
                            Next
                            typeFilter = String.Join(" or ", filteredTypes)

                            filterXPath = "@parId=" & contentTypeFilterKey
                            If Not String.IsNullOrEmpty(typeFilter) Then filterXPath &= " and not(" & typeFilter & ")"

                            ' Go and get the nodes to remove
                            For Each removableChild As XmlElement In oRelationsXml.SelectNodes("NewDataSet/Relation[" & filterXPath & "]")
                                removableChild.ParentNode.RemoveChild(removableChild)
                            Next

                        Next

                        ' Create a content list - it doesn't matter about type 
                        Dim contentList As New List(Of String)
                        For Each relation As XmlElement In oRelationsXml.SelectNodes("NewDataSet/Relation")
                            If Not contentList.Contains(relation.GetAttribute("id")) Then
                                contentList.Add(relation.GetAttribute("id"))
                            End If
                        Next


                        ' Only proceed if we have some items to investigate
                        If contentList.Count > 0 Then

                            sSql = "select c.nContentKey as id, cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire, a.nInsertDirId as owner, dbo.fxn_getContentParents(c.nContentKey) as parId " & _
                                " FROM tblContent c INNER JOIN" & _
                                " tblAudit a ON c.nAuditId = a.nAuditKey " & _
                                " WHERE "

                            sSql &= " c.nContentKey IN (" & String.Join(",", contentList.ToArray()) & ") "

                            ' Build the filter SQL
                            sFilterSql = myWeb.GetStandardFilterSQLForContent()

                            sSql &= sFilterSql & " order by type"

                            Dim oDs As DataSet = New DataSet
                            PerfMon.Log("DBHelper", "addBulkRelatedContent - get relations", sSql)
                            oDs = GetDataSet(sSql, "Content", "Contents")
                            PerfMon.Log("DBHelper", "addBulkRelatedContent - get relations end", sSql)
                            Dim contents As XmlDocument = ContentDataSetToXml(oDs, dUpdateDate)
                            Dim contents2 As XmlNode = oContentParent.OwnerDocument.ImportNode(contents.DocumentElement, True)
                            Dim contentChild As XmlElement

                            Dim oContent As XmlElement
                            For Each oContent In contents2.SelectNodes("Content")
                                oContent = SimpleTidyContentNode(oContent, "", Nothing, dUpdateDate)
                            Next

                            'now lets take our xml's and do the magic
                            PerfMon.Log("DBHelper", "addBulkRelatedContent - start place contents")
                            Dim nRelationCount = 0
                            ' Run through each Relation
                            For Each relation As XmlElement In oRelationsXml.SelectNodes("NewDataSet/Relation")

                                nRelationCount = nRelationCount + 1

                                ' For each relation, find content nodes at the current level that match the parent id
                                ' This implements the following protections:
                                '   RECURSION - Make sure that the content nodes don't have an ancestor of the relationship child id.
                                '   DUPLICATE CONTENT - Make sure that the content nodes don't already have this child
                                ' Note that sContentLevelxPath has the / at the front - we need it at the end for this
                                Dim xPathToParents As String = ""
                                If Not (String.IsNullOrEmpty(sContentLevelxPath)) Then xPathToParents &= sContentLevelxPath.Substring(1) & "/"

                                xPathToParents &= "Content[@id='" & relation.GetAttribute("parId") & "'" _
                                    & " and not(ancestor-or-self::Content[@id='" & relation.GetAttribute("id") & "'])" _
                                    & " and not(Content[@id='" & relation.GetAttribute("id") & "'])]"
                                'Dim j As Integer = 0
                                'Dim k As Integer = 0


                                For Each contentParent As XmlElement In oContentParent.SelectNodes(xPathToParents)

                                    contentChild = contents2.SelectSingleNode("Content[@id='" & relation.GetAttribute("id") & "']")

                                    If contentChild IsNot Nothing Then

                                        If relation.HasAttribute("rtype") Then contentChild.SetAttribute("rtype", relation.GetAttribute("rtype"))
                                        'contentChild = SimpleTidyContentNode(contentChild, "", Nothing, dUpdateDate)

                                        contentParent.AppendChild(contentChild.CloneNode(True))
                                        'k = k + 1
                                    End If
                                    'j = j + 1
                                Next
                                'PerfMon.Log("DBHelper", "addrelation[" & relation.GetAttribute("id") & "]count[" & j & "]added[" & k & "]")

                            Next
                            PerfMon.Log("DBHelper", "addBulkRelatedContent - end place contents (" & nRelationCount & " relations)")
                        End If
                    End If
                    sContentLevelxPath += "/Content"
                    nMaxDepth = nMaxDepth - 1
                Loop

                ' Tidy up attributes
                For Each contentNode As XmlElement In oContentParent.SelectNodes("Content[@processForRelatedContent]")
                    contentNode.RemoveAttribute("processForRelatedContent")
                Next

                PerfMon.Log("DBHelper", "addBulkRelatedContent - END")

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "addBulkRelatedContent", ex, sProcessInfo))

            End Try

        End Sub

        Public Sub saveContentRelations()
            PerfMon.Log("DBHelper", "saveContentRelations")
            Dim sProcessInfo As String = ""
            Try

                Dim oFrmItem As Object
                For Each oFrmItem In goRequest.Form
                    Dim cOName As String = oFrmItem
                    Dim cOValue As String = goRequest.Form.Get(cOName)
                    Dim rType As String = myWeb.moRequest("relationType")
                    If cOName.Contains("list") Then
                        If rType = "" Then
                            insertContentRelation(goRequest.Form.Get("id"), cOValue, IIf(myWeb.moRequest("RelType") = "1way", False, True))
                        Else
                            insertContentRelation(goRequest.Form.Get("id"), cOValue, IIf(myWeb.moRequest("RelType") = "1way", False, True), rType)
                        End If
                    End If
                    'If cOName.Contains("reciprocate_") Then
                    '    insertContentRelation(cOValue, goRequest.Form.Get("id"))
                    'End If
                Next

            Catch ex As Exception

                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "saveContentRelations", ex, sProcessInfo))

            End Try

        End Sub

        Public Function insertContentRelation(ByVal nParentID As Integer, ByVal nChildIDs As String, Optional ByVal b2Way As Boolean = False, Optional ByVal rType As String = "", Optional ByVal bHaltRecursion As Boolean = False) As String
            PerfMon.Log("DBHelper", "insertContentRelation")
            Try
                Dim nChilds() As String = Split(nChildIDs, ",")
                Dim nIDs As String = ""
                Dim nIx As Integer
                For nIx = 0 To UBound(nChilds)


                    Dim cSQl As String
                    If rType <> "" Then
                        cSQl = "Select nContentRelationKey from tblContentRelation where nContentParentId = " & nParentID & " and nContentChildId = " & nChilds(nIx) & "and cRelationType = '" & rType & "'"
                    Else
                        cSQl = "Select nContentRelationKey from tblContentRelation where nContentParentId = " & nParentID & " and nContentChildId = " & nChilds(nIx)
                    End If

                    Dim nID As Integer = ExeProcessSqlScalar(cSQl)
                    If nID > 0 Then
                        nIDs &= nID & ","
                    Else

                        Dim oDs As New DataSet
                        oDs = GetDataSet("SELECT * FROM tblContentRelation", "Content")

                        cSQl = "SELECT Count(tblContentRelation.nContentRelationKey) FROM tblContent INNER JOIN tblContent tblContent_1 ON tblContent.cContentSchemaName = tblContent_1.cContentSchemaName INNER JOIN tblContentRelation ON tblContent_1.nContentKey = tblContentRelation.nContentChildId" & _
                        " WHERE (tblContent.nContentKey = " & nChilds(nIx) & ") AND (tblContentRelation.nContentParentId = " & nParentID & ")"

                        Dim nCount As Integer = ExeProcessSqlScalar(cSQl) + 1

                        If (oDs.Tables("Content").Columns.Contains("cRelationType")) Then
                            cSQl = "INSERT INTO tblContentRelation (nContentParentId, nContentChildId, nDisplayOrder, nAuditId, cRelationType) VALUES (" & _
                            nParentID & "," & _
                            nChilds(nIx) & "," & _
                            nCount & "," & _
                            getAuditId(, , "ContentRelation") & "," & _
                            "'" & SqlFmt(rType) & "'" & ")"
                        Else
                            'getAuditId(nContentStatus, , , dPublishDate, dExpireDate) 
                            cSQl = "INSERT INTO tblContentRelation (nContentParentId, nContentChildId, nDisplayOrder, nAuditId) VALUES (" & _
                            nParentID & "," & _
                            nChilds(nIx) & "," & _
                            nCount & "," & _
                            getAuditId(, , "ContentRelation") & ")"
                        End If

                        ' AG Note: Not sure what this is (BR wrote this)
                        ' It appears to force a 2way relationship if this is an orphan content
                        '  Problem is if the parent is also an orphan, then this will recurse infinitely
                        '  hence we pass back the bHaltRecursion parameter.
                        Dim nIsOrphan As Long = GetDataValue("SELECT nStructId  FROM tblContentLocation WHERE nContentId = " & nChilds(nIx), , , 0)
                        If nIsOrphan = 0 And Not b2Way And Not bHaltRecursion Then
                            insertContentRelation(nChilds(nIx), nParentID, , , True)
                        End If


                        nIDs &= GetIdInsertSql(cSQl) & ","
                    End If
                    If b2Way Then
                        If Not rType = "" Then
                            insertContentRelation(nChilds(nIx), nParentID, False, rType)
                        Else
                            insertContentRelation(nChilds(nIx), nParentID, False)
                        End If
                    End If
                Next
                Return Left(nIDs, nIDs.Length - 1)
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "addContentRelation", ex, ""))
                Return ""
            End Try

        End Function


        Public Sub RemoveContentRelation(ByVal nRelatedParentId As Long, ByVal nContentId As Long)
            PerfMon.Log("DBHelper", "RemoveContentRelation")
            Try

                Dim cSQL As String = "Select nContentRelationKey from tblContentRelation where nContentParentID = " & nRelatedParentId & " AND nContentChildId = " & nContentId
                DeleteObject(dbHelper.objectTypes.ContentRelation, ExeProcessSqlScalar(cSQL))

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "RemoveContentRelation", ex, ""))

            End Try

        End Sub

        Public Sub RemoveContentRelationByType(ByVal nRelatedParentId As Long, ByVal cRelationType As String, ByVal direction As String)
            PerfMon.Log("DBHelper", "RemoveContentRelation")
            Try

                Dim cSQL As String
                If LCase(direction) = "child" Then
                    cSQL = "Select nContentRelationKey from tblContentRelation where nContentChildId = " & nRelatedParentId & " AND cRelationType = '" & cRelationType & "'"
                Else
                    cSQL = "Select nContentRelationKey from tblContentRelation where nContentParentId = " & nRelatedParentId & " AND cRelationType = '" & cRelationType & "'"
                End If

                Dim oRead As SqlDataReader = getDataReader(cSQL)
                While oRead.Read()
                    DeleteObject(dbHelper.objectTypes.ContentRelation, oRead.GetInt32(0))
                End While

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "RemoveContentRelation", ex, ""))

            End Try

        End Sub

        Public Sub RemoveContentLocation(ByVal nPageId As Long, ByVal nContentId As Long)
            PerfMon.Log("DBHelper", "RemoveContentRelation")
            Try

                Dim cSQL As String = "Select nContentLocationKey from tblContentLocation where nStructId = " & nPageId & " AND nContentId = " & nContentId
                DeleteObject(dbHelper.objectTypes.ContentLocation, ExeProcessSqlScalar(cSQL))

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "RemoveContentLocation", ex, ""))

            End Try

        End Sub

        Public Function RelatedContentSearch(ByVal nRootNode As Integer, ByVal cSchemaName As String, ByVal bChildren As Boolean, ByVal cSearchExpression As String, ByVal nParentId As Integer, Optional ByVal nIgnoreID As Integer = 0, Optional ByVal oRelated() As String = Nothing) As XmlElement
            PerfMon.Log("DBHelper", "RelatedContentSearch")
            Try
                Dim sSearch As String = cSearchExpression
                'remove reserved words
                sSearch = Replace(" " & sSearch & " ", " the ", "")
                sSearch = Replace(" " & sSearch & " ", " and ", "")
                sSearch = Replace(" " & sSearch & " ", " if ", "")
                sSearch = Replace(" " & sSearch & " ", " then ", "")
                sSearch = Replace(" " & sSearch & " ", " or ", "")
                sSearch = Trim(sSearch)


                Dim cSQL As String = ""
                Dim oDs As New DataSet
                Dim oFullData As XmlDataDocument
                Dim oResults As XmlElement = moPageXml.CreateElement("RelatedResults")
                Dim cXPath As String = ""
                Dim cXPath2 As String = ""

                'if schemaName is a comma separated string
                Dim aSchema() As String = cSchemaName.Split(",")
                Dim i As Integer
                Dim sWhere As String = ""
                For i = 0 To UBound(aSchema)
                    sWhere = sWhere & "cContentSchemaName = '" & Trim(aSchema(i)) & "' or "
                Next
                'lets whip of the last and
                sWhere = Left(sWhere, Len(sWhere) - 3)


                If nRootNode = 0 Then
                    'All
                    cSQL = "SELECT nContentKey as id, cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content,  " _
                            & "	a.dPublishDate AS publishDate " _
                            & " FROM tblContent c " _
                            & "	INNER JOIN tblAudit a ON a.nAuditKey = c.nAuditId " _
                            & " WHERE " & sWhere & " ORDER BY cContentName"

                ElseIf nRootNode = -1 Then
                    'Orphans only
                    cSQL = "SELECT c.nContentKey as id, c.cContentForiegnRef as ref, c.cContentName as name, c.cContentSchemaName as type, c.cContentXmlBrief as content " _
                            & "	a.dPublishDate AS publishDate " _
                            & " FROM tblContent c" _
                            & "	INNER JOIN tblAudit a ON a.nAuditKey = c.nAuditId " _
                            & " LEFT OUTER JOIN tblContentLocation cl ON tblContent.nContentKey = cl.nContentId WHERE ( " & sWhere & " ) AND (cl.nContentLocationKey IS NULL) ORDER BY c.cContentName"
                Else
                    'by a page

                    ' Optimisation is needed to get only relevant content for processing

                    ' Firstly we get a structure XML and identify the nodes in question.
                    cSQL = "SELECT * FROM tblContentStructure"
                    oDs = GetDataSet(cSQL, "Structure", "SearchRelateable")
                    oDs.Relations.Add("StructStruct", oDs.Tables("Structure").Columns("nStructKey"), oDs.Tables("Structure").Columns("nStructParId"), False)
                    oDs.Relations("StructStruct").Nested = True
                    oDs.Tables("Structure").Columns("nStructKey").ColumnMapping = MappingType.Attribute

                    ' DS to XML
                    Dim oStructure As XmlDataDocument = New XmlDataDocument()
                    oStructure.PreserveWhitespace = False
                    oStructure.InnerXml = oDs.GetXml.ToString.Replace("&lt;", "<").Replace("&gt;", ">")

                    ' Find the root node
                    Dim oRootNode As XmlElement = oStructure.SelectSingleNode("//Structure[@nStructKey='" & nRootNode & "']")

                    ' Get a list of the page ids - we're going to use this to filter content
                    Dim cLocations As String = nRootNode.ToString
                    If bChildren Then
                        For Each oChild As XmlElement In oRootNode.SelectNodes("descendant::Structure")
                            cLocations &= "," & oChild.GetAttribute("nStructKey")
                        Next
                    End If

                    ' Destroy things
                    oRootNode = Nothing
                    oStructure = Nothing

                    ' Now get content (filtered by locations)

                    ' First get a subquery that gets distinct items according to the criteria
                    Dim cSubquerySQL As String =
                            "SELECT DISTINCT c.nContentKey AS id " _
                            & "FROM tblContent c " _
                            & "	INNER JOIN tblContentLocation l " _
                            & "		ON c.nContentKey = l.nContentId " _
                            & "WHERE (" & sWhere & ") " _
                            & "	AND NOT(c.nContentKey IN (0," & nIgnoreID & ")) " _
                            & "	AND l.nStructId IN (" & cLocations & ") "

                    ' No get more information
                    cSQL = "SELECT " _
                            & "	c.nContentKey AS id,  " _
                            & "	c.cContentForiegnRef AS ref,  " _
                            & "	c.cContentName AS name,  " _
                            & "	c.cContentSchemaName AS type,  " _
                            & "	c.cContentXmlBrief AS content,  " _
                            & "	a.dPublishDate AS publishDate " _
                            & "FROM tblContent c " _
                            & "	INNER JOIN tblAudit a ON a.nAuditKey = c.nAuditId " _
                            & "	INNER JOIN (" & cSubquerySQL & ") distinctlist " _
                            & "		ON c.nContentKey = distinctlist.id " _
                            & "ORDER BY c.cContentName"

                End If


                oDs = GetDataSet(cSQL, "Content", "SearchRelateable")
                oDs.EnforceConstraints = False
                oDs.Tables("Content").Columns("id").ColumnMapping = Data.MappingType.Attribute
                oDs.Tables("Content").Columns("ref").ColumnMapping = Data.MappingType.Attribute
                oDs.Tables("Content").Columns("name").ColumnMapping = Data.MappingType.Attribute
                oDs.Tables("Content").Columns("type").ColumnMapping = Data.MappingType.Attribute
                oDs.Tables("Content").Columns("publishDate").ColumnMapping = Data.MappingType.Attribute
                oDs.Tables("Content").Columns("content").ColumnMapping = Data.MappingType.SimpleContent

                oFullData = New XmlDataDocument()
                oFullData.PreserveWhitespace = False
                oFullData.InnerXml = oDs.GetXml.ToString
                Dim oNode As XmlNode
                For Each oNode In oFullData.SelectNodes("SearchRelateable/Content")

                    oNode = SimpleTidyContentNode(oNode)

                Next

                Dim SearchArr() As String = Split(cSearchExpression, " ")

                Dim bFound As Boolean = False


                ' Get each content node and check it against the Search Array
                For Each oTempNode As XmlElement In oFullData.SelectNodes("SearchRelateable/Content")

                    For i = SearchArr.GetLowerBound(0) To SearchArr.GetUpperBound(0)
                        If oTempNode.InnerText.ToUpper.Contains(SearchArr(i).ToUpper) Then
                            oResults.AppendChild(moPageXml.ImportNode(oTempNode, True))
                            bFound = True
                        End If
                    Next

                Next

                oResults.SetAttribute("nParentID", nParentId)
                oResults.SetAttribute("cSchemaName", cSchemaName)

                Dim nI As Integer
                If Not oRelated Is Nothing Then
                    For nI = 0 To UBound(oRelated)
                        If Not oRelated(nI) = "" Then

                            Dim oTmp As XmlElement = oResults.SelectSingleNode("Content[@id=" & oRelated(nI) & "]")
                            If Not oTmp Is Nothing Then
                                oTmp.SetAttribute("related", 1)

                                cSQL = "SELECT * FROM tblContentRelation"
                                Dim oDs_2 As New DataSet
                                oDs_2 = GetDataSet(cSQL, "Content")
                                If (oDs_2.Tables("Content").Columns.Contains("cRelationType")) Then

                                    cSQL = "SELECT cRelationType FROM tblContentRelation WHERE nContentParentId = '" + nParentId.ToString + "' AND nContentChildId = '" + oRelated(nI) + "'"

                                    Dim oDre As SqlDataReader = getDataReader(cSQL)
                                    Dim cSqlResults As String = ""

                                    Do While oDre.Read
                                        cSqlResults &= oDre(0) & ","
                                    Loop
                                    oDre.Close()
                                    If Not cSqlResults = "" Then cSqlResults = Left(cSqlResults, Len(cSqlResults) - 1)

                                    If Not cSqlResults = "" Then
                                        oTmp.SetAttribute("sType", cSqlResults)
                                    Else
                                        oTmp.SetAttribute("sType", "Not Specified")
                                    End If
                                Else
                                    oTmp.SetAttribute("sType", "Not Specified")
                                End If

                            End If
                        End If
                    Next
                End If

                Return oResults
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "RelatedContentSearch", ex, ""))
                Return Nothing
            End Try
        End Function

        Public Sub saveProductsGroupRelations()
            PerfMon.Log("DBHelper", "saveProductsGroupRelations")
            Try
                Dim oFrmItem As Object
                For Each oFrmItem In goRequest.Form
                    Dim cOName As String = oFrmItem
                    Dim cOValue As String = goRequest.Form.Get(cOName)
                    If cOName.Contains("list") Then
                        insertGroupProductRelation(goRequest.QueryString("GroupId"), cOValue)
                    End If
                Next
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "saveProductsGroupRelations", ex, ""))

            End Try
        End Sub


        Public Function insertGroupProductRelation(ByVal nGroupId As Integer, ByVal nContent As String) As String
            PerfMon.Log("DBHelper", "insertProductGroupRelation")
            Try
                Dim oContentArr() As String = Split(nContent, ",")
                Dim cCount As Integer
                Dim strReturn As New Text.StringBuilder


                For cCount = 0 To UBound(oContentArr)

                    Dim nCatProductRelKey As Integer

                    Dim cSQl As String = "Select nCatProductRelKey from tblCartCatProductRelations where nCatId = " & nGroupId & " and nContentId = " & oContentArr(cCount)
                    nCatProductRelKey = ExeProcessSqlScalar(cSQl)

                    'if there's no existing relation, make a new one
                    If nCatProductRelKey = 0 Then
                        cSQl = "INSERT INTO tblCartCatProductRelations (nContentId, nCatId, nAuditId) VALUES (" &
                        oContentArr(cCount) & "," &
                        nGroupId & "," &
                        getAuditId(, , "ContentRelation") & ")"

                        nCatProductRelKey = GetIdInsertSql(cSQl)
                    End If

                    strReturn.Append(nCatProductRelKey)

                    'delimit by comma, except on last pass
                    If cCount <> UBound(oContentArr) Then
                        strReturn.Append(",")
                    End If

                Next
                Dim s As String = strReturn.ToString


                Return strReturn.ToString


            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "insertProductGroupRelation", ex, ""))
                Return "0"
            End Try


        End Function




        Public Function insertProductGroupRelation(ByVal nProductId As Integer, ByVal sGroupIds As String) As String
            PerfMon.Log("DBHelper", "insertProductGroupRelation")
            Try
                Dim oGroupArr() As String = Split(sGroupIds, ",")
                Dim cCount As Integer
                Dim strReturn As New Text.StringBuilder

                If sGroupIds <> "" Then
                    For cCount = 0 To UBound(oGroupArr)

                        Dim nCatProductRelKey As Integer
                        Dim savedId As Integer

                        Dim cSQl As String = "Select nCatProductRelKey from tblCartCatProductRelations where nContentId = " & nProductId & " and nCatId = " & oGroupArr(cCount)
                        nCatProductRelKey = ExeProcessSqlScalar(cSQl)

                        'if there's no existing relation, make a new one
                        If nCatProductRelKey = 0 Then
                            cSQl = "INSERT INTO tblCartCatProductRelations (nContentId, nCatId, nAuditId) VALUES (" &
                            nProductId & "," &
                            oGroupArr(cCount) & "," &
                            getAuditId(, , "ContentRelation") & ")"
                            savedId = GetIdInsertSql(cSQl)
                        End If

                        strReturn.Append(savedId)
                        strReturn.Append(",")

                    Next

                    Dim s As String = strReturn.ToString
                    s = s.TrimEnd(",")

                    'delete any new ones
                    Dim delSql As String = "Select nCatProductRelKey from tblCartCatProductRelations where nContentId = " & nProductId & " and nCatProductRelKey not in (" & s & ")"
                    Dim oDr As SqlDataReader = getDataReader(delSql)
                    Do While oDr.Read
                        Me.DeleteObject(objectTypes.CartCatProductRelations, oDr(0))
                    Loop
                    Return s
                Else
                    'if GroupIds is empty then delete all
                    Dim delSql As String = "Select nCatProductRelKey from tblCartCatProductRelations where nContentId = " & nProductId
                    Dim oDr As SqlDataReader = getDataReader(delSql)
                    Do While oDr.Read
                        Me.DeleteObject(objectTypes.CartCatProductRelations, oDr(0))
                    Loop
                    Return ""
                End If

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "insertProductGroupRelation", ex, ""))
                Return "0"
            End Try


        End Function




        Public Function saveDiscountDirRelation(ByVal nDiscountId As Integer, ByVal nDirIds As String, Optional ByVal bInsert As Boolean = True, Optional ByVal Permlevel As PermissionLevel = PermissionLevel.Open) As String
            PerfMon.Log("DBHelper", "saveDiscountDirRelation")
            Try
                Dim cGroups() As String = Split(nDirIds, ",")
                Dim nI As Integer
                Dim nDirId As Integer
                Dim cNewIds As String = ""
                Dim nPermLevel As Integer = "1"
                Dim bDeny As Boolean = False
                If checkTableColumnExists("tblCartDiscountDirRelations", "nPermLevel") Then
                    bDeny = True
                    Select Case Permlevel
                        Case PermissionLevel.Denied
                            nPermLevel = 0
                        Case Else
                            nPermLevel = 1
                    End Select
                End If

                For nI = 0 To UBound(cGroups)
                    nDirId = CInt(cGroups(nI))
                    If bInsert Then


                        'if exists then  return the id
                        Dim cSQL As String = "Select nDiscountDirRelationKey From tblCartDiscountDirRelations Where nDiscountId = " & nDiscountId & " And nDirId = " & nDirId
                        Dim nId As Integer = ExeProcessSqlScalar(cSQL)
                        If nId > 0 Then Return nId

                        'Logic to rmove relations if an "all" entry exists, or other way round
                        If nDirId > 0 Then
                            'remove any "all" record
                            cSQL = "Select nDiscountDirRelationKey From tblCartDiscountDirRelations Where nDiscountId = " & nDiscountId & " And nDirId = 0 And nPermLevel = " & nPermLevel
                            Me.DeleteObject(objectTypes.CartDiscountDirRelations, ExeProcessSqlScalar(cSQL))
                        Else
                            'remove any specific record
                            cSQL = "Select nDiscountDirRelationKey From tblCartDiscountDirRelations Where nDiscountId = " & nDiscountId & " And nDirId > 0 And nPermLevel = " & nPermLevel
                            Dim oDre As SqlDataReader = getDataReader(cSQL)
                            Do While oDre.Read
                                Me.DeleteObject(objectTypes.CartDiscountDirRelations, oDre(0))
                            Loop
                            oDre.Close()
                        End If
                        If bDeny Then
                            cSQL = "INSERT INTO tblCartDiscountDirRelations (nDiscountId, nDirId, nAuditId, nPermLevel) Values(" & _
                            nDiscountId & "," & _
                            nDirId & "," & _
                            Me.getAuditId(1, , "DiscountDirRelation") & ", " & nPermLevel & ")"
                        Else
                            cSQL = "INSERT INTO tblCartDiscountDirRelations (nDiscountId, nDirId, nAuditId) Values(" & _
                            nDiscountId & "," & _
                            nDirId & "," & _
                            Me.getAuditId(1, , "DiscountDirRelation") & ")"
                        End If

                        cNewIds &= GetIdInsertSql(cSQL) & ","
                    Else
                        cNewIds &= DeleteObject(objectTypes.CartDiscountDirRelations, nDirId) & ","
                    End If
                Next
                Return Left(cNewIds, cNewIds.Length - 1)
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "insertDiscountDirRelation", ex, ""))
                Return 0
            End Try
        End Function





        Public Function saveShippingDirRelation(ByVal nShippingMethodId As Integer, ByVal nDirIds As String, Optional ByVal bInsert As Boolean = True, Optional ByVal Permlevel As PermissionLevel = PermissionLevel.Open) As String
            PerfMon.Log("DBHelper", "saveShippingDirRelation")
            Try
                Dim cGroups() As String = Split(nDirIds, ",")
                Dim nI As Integer
                Dim nDirId As Integer
                Dim cNewIds As String = ""
                Dim bDeny As Boolean = False
                Dim nPermLevel As Integer = "1"
                If checkTableColumnExists("tblCartShippingPermission", "nPermLevel") Then
                    bDeny = True
                    Select Case Permlevel
                        Case PermissionLevel.Denied
                            nPermLevel = 0
                        Case Else
                            nPermLevel = 1
                    End Select
                End If

                For nI = 0 To UBound(cGroups)
                    nDirId = CInt(cGroups(nI))
                    If bInsert Then
                        'if exists then  return the id
                        Dim cSQL As String = "Select nCartShippingPermissionKey From tblCartShippingPermission Where nShippingMethodId = " & nShippingMethodId & " And nDirId = " & nDirId
                        Dim nId As Integer = ExeProcessSqlScalar(cSQL)
                        If nId > 0 Then Return nId

                        'Logic to rmove relations if an "all" entry exists, or other way round
                        If nDirId > 0 Then
                            'remove any "all" record
                            cSQL = "Select nCartShippingPermissionKey From tblCartShippingPermission Where nShippingMethodId = " & nShippingMethodId & " And nDirId = 0"
                            Me.DeleteObject(objectTypes.CartShippingPermission, ExeProcessSqlScalar(cSQL))
                        Else
                            'remove any specific record
                            cSQL = "Select nCartShippingPermissionKey From tblCartShippingPermission Where nShippingMethodId = " & nShippingMethodId & " And nDirId > 0"
                            Dim oDre As SqlDataReader = getDataReader(cSQL)
                            Do While oDre.Read
                                Me.DeleteObject(objectTypes.CartShippingPermission, oDre(0))
                            Loop
                            oDre.Close()
                        End If
                        If bDeny Then
                            cSQL = "INSERT INTO tblCartShippingPermission (nShippingMethodId, nDirId, nPermLevel, nAuditId) Values(" & _
                                nShippingMethodId & "," & _
                                nDirId & "," & _
                                nPermLevel & "," & _
                                Me.getAuditId(1, , "CartShippingPermission") & ")"

                        Else
                            cSQL = "INSERT INTO tblCartShippingPermission (nShippingMethodId, nDirId, nAuditId) Values(" & _
                                                   nShippingMethodId & "," & _
                                                   nDirId & "," & _
                                                   Me.getAuditId(1, , "CartShippingPermission") & ")"
                        End If

                        cNewIds &= GetIdInsertSql(cSQL) & ","
                    Else
                        cNewIds &= DeleteObject(objectTypes.CartShippingPermission, nDirId) & ","
                    End If
                Next
                Return Left(cNewIds, cNewIds.Length - 1)
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "saveShippingDirRelation", ex, ""))
                Return 0
            End Try
        End Function


        Public Function saveDiscountProdGroupRelation(ByVal nDiscountId As Integer, ByVal cProductgroups As String, Optional ByVal bInsert As Boolean = True) As String
            PerfMon.Log("DBHelper", "saveDiscountProdGroupRelation")
            Try
                Dim cGroups() As String = Split(cProductgroups, ",")
                Dim nI As Integer
                Dim nProdGroupId As Integer
                Dim cNewIds As String = ""
                For nI = 0 To UBound(cGroups)
                    nProdGroupId = CInt(cGroups(nI))
                    If bInsert Then
                        'if exists then  return the id
                        Dim cSQL As String = "Select nDiscountProdCatRelationKey From tblCartDiscountProdCatRelations Where nDiscountId = " & nDiscountId & " And nProductCatId = " & nProdGroupId
                        Dim nId As Integer = ExeProcessSqlScalar(cSQL)
                        If nId > 0 Then Return nId

                        'Logic to rmove relations if an "all" entry exists, or other way round
                        If nProdGroupId > 0 Then
                            'remove any "all" record
                            cSQL = "Select nDiscountProdCatRelationKey From tblCartDiscountProdCatRelations Where nDiscountId = " & nDiscountId & " And nProductCatId = 0"
                            Me.DeleteObject(objectTypes.CartDiscountProdCatRelations, ExeProcessSqlScalar(cSQL))
                        Else
                            'remove any specific record
                            cSQL = "Select nDiscountProdCatRelationKey From tblCartDiscountProdCatRelations Where nDiscountId = " & nDiscountId & " And nProductCatId > 0"
                            Dim oDre As SqlDataReader = getDataReader(cSQL)
                            Do While oDre.Read
                                Me.DeleteObject(objectTypes.CartDiscountProdCatRelations, oDre(0))
                            Loop
                            oDre.Close()
                        End If

                        cSQL = "INSERT INTO tblCartDiscountProdCatRelations (nDiscountId, nProductCatId, nAuditId) Values(" & _
                        nDiscountId & "," & _
                        nProdGroupId & "," & _
                        Me.getAuditId(1, , "DiscountProdGroupRelation") & ")"
                        cNewIds &= GetIdInsertSql(cSQL) & ","
                    Else
                        cNewIds &= DeleteObject(objectTypes.CartDiscountProdCatRelations, nProdGroupId) & ","
                    End If
                Next
                Return Left(cNewIds, cNewIds.Length - 1)
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "insertDiscountProdGroupRelation", ex, ""))
                Return 0
            End Try
        End Function

        Public Function PageIsLive(ByVal nPageId As Integer) As Boolean
            PerfMon.Log("DBHelper", "PageIsLive")
            Try
                Dim sSQL As String = "Select tblAudit.dPublishDate, tblAudit.dExpireDate, tblAudit.nStatus, tblContentStructure.nStructKey FROM tblAudit INNER JOIN tblContentStructure On tblAudit.nAuditKey = tblContentStructure.nAuditId WHERE tblContentStructure.nStructKey = " & nPageId
                Dim oDRe As SqlDataReader = getDataReader(sSQL)
                Dim bPublish As Boolean = False
                Dim bExpire As Boolean = False
                Dim bStatus As Boolean = False

                Do While oDRe.Read
                    'Publish
                    If Not oDRe.IsDBNull(0) Then
                        If IsNumeric(oDRe(0)) Then
                            If oDRe(0) = 0 Then bPublish = True
                        Else
                            If CDate(oDRe(0)) <= Now Then bPublish = True
                        End If
                    Else
                        bPublish = True
                    End If
                    'Expire
                    If Not oDRe.IsDBNull(1) Then
                        If IsNumeric(oDRe(1)) Then
                            If oDRe(1) = 0 Then bExpire = True
                        Else
                            If CDate(oDRe(1)) <= Now Then bExpire = True
                        End If
                    Else
                        bExpire = True
                    End If
                    'Status
                    If Not oDRe.IsDBNull(2) Then
                        If Not oDRe(2) = 0 Then bStatus = True
                    Else
                        bStatus = True
                    End If
                Loop
                oDRe.Close()

                If bPublish And bExpire And bStatus Then
                    Return True
                Else
                    Return False
                End If

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "PageIsLive", ex, ""))
            End Try
        End Function

        Function AddInvalidEmail(ByVal cEmailAddress As String) As Boolean
            PerfMon.Log("DBHelper", "AddInvalidEmail")
            Try
                If cEmailAddress = "" Then Return False
                Dim cSQL As String = "Select EmailAddress FROM tblOptOutAddresses WHERE (EmailAddress = '" & cEmailAddress & "')"
                If Not ExeProcessSqlScalar(cSQL) = cEmailAddress Then
                    cSQL = "INSERT INTO tblOptOutAddresses (EmailAddress) VALUES ('" & cEmailAddress & "')"
                    ExeProcessSql(cSQL)
                    Return True
                Else
                    Return False
                End If
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "AddInvalidEmail", ex, ""))
            End Try
        End Function

        Sub RemoveInvalidEmail(ByVal cEmailAddressesCSV As String)
            PerfMon.Log("DBHelper", "RemoveInvalidEmail")
            Try
                If cEmailAddressesCSV = "" Then Exit Sub
                cEmailAddressesCSV = "'" & cEmailAddressesCSV & "'"
                cEmailAddressesCSV = Replace(cEmailAddressesCSV, ",", "','")
                Dim cSQL As String = "DELETE FROM tblOptOutAddresses  WHERE (EmailAddress IN (" & cEmailAddressesCSV & "))"

                ExeProcessSql(cSQL)

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "AddInvalidEmail", ex, ""))
            End Try
        End Sub

        Public Sub ViewMailHistory(ByRef oPageDetailElmt As XmlElement)
            PerfMon.Log("DBHelper", "ViewMailHistory")
            Dim cSQL As String = ""
            Try

                cSQL = "SELECT tblActivityLog.nActivityKey, tblDirectory.cDirName, tblContentStructure.cStructName, tblActivityLog.dDateTime, tblActivityLog.cActivityDetail" & _
                " FROM tblActivityLog INNER JOIN" & _
                " tblDirectory ON tblActivityLog.nUserDirId = tblDirectory.nDirKey INNER JOIN" & _
                " tblContentStructure ON tblActivityLog.nStructId = tblContentStructure.nStructKey" & _
                " WHERE(tblActivityLog.nActivityType = " & Me.ActivityType.NewsLetterSent & ")" & _
                " ORDER BY tblActivityLog.dDateTime DESC"
                Dim oDs As DataSet = GetDataSet(cSQL, "Activity", "ActivityLog")
                cSQL = "SELECT cDirName, nDirKey FROM tblDirectory tDir WHERE (cDirSchema = N'Group')"
                addTableToDataSet(oDs, cSQL, "Group")
                oDs.Tables("Activity").Columns("nActivityKey").ColumnMapping = MappingType.Attribute
                oDs.Tables("Activity").Columns("cDirName").ColumnMapping = MappingType.Attribute
                oDs.Tables("Activity").Columns("cStructName").ColumnMapping = MappingType.Attribute
                oDs.Tables("Activity").Columns("dDateTime").ColumnMapping = MappingType.Attribute
                oDs.Tables("Activity").Columns("cActivityDetail").ColumnMapping = MappingType.Element
                oDs.Tables("Group").Columns("cDirName").ColumnMapping = MappingType.Attribute
                oDs.Tables("Group").Columns("nDirKey").ColumnMapping = MappingType.Attribute

                Dim oActivityElement As XmlElement = oPageDetailElmt.OwnerDocument.CreateElement("ActivityLog")
                oActivityElement.InnerXml = Replace(Replace(oDs.GetXml, "&gt;", ">"), "&lt;", "<")
                Dim odtElement As XmlElement
                'xmlDateTime
                For Each odtElement In oActivityElement.SelectNodes("descendant-or-self::Activity")
                    odtElement.SetAttribute("dDateTime", Eonic.Tools.Xml.XmlDate(odtElement.GetAttribute("dDateTime"), True))
                Next
                oPageDetailElmt.AppendChild(oActivityElement.FirstChild)
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "ViewMailHistory", ex, cSQL))
            End Try
        End Sub

        Public Function CheckOptOut(ByVal nCheckAddress As String) As Boolean
            PerfMon.Log("DBHelper", "CheckOptOut")
            Try
                Dim cSQL As String
                If Not nCheckAddress = "" Then
                    Dim bReturn As Boolean
                    cSQL = "SELECT EmailAddress FROM tblOptOutAddresses WHERE EmailAddress = '" & nCheckAddress & "'"
                    Dim oDRE As SqlDataReader = getDataReader(cSQL)
                    bReturn = oDRE.HasRows
                    oDRE.Close()
                    Return bReturn
                Else
                    Return False
                End If
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs("CheckOptOut", "", ex, ""))
                Return False
            End Try
        End Function

        Public Function ListOptOuts() As XmlElement
            PerfMon.Log("DBHelper", "ListOptOuts")
            Try
                Dim oElmt As XmlElement = moPageXml.CreateElement("Content")
                oElmt.SetAttribute("type", "OptOut")
                Dim cSQL As String = "SELECT EmailAddress FROM tblOptOutAddresses ORDER BY EmailAddress"
                Dim oDS As DataSet = GetDataSet(cSQL, "Email", "Addresses")
                Dim oXML As New XmlDocument
                oXML.InnerXml = oDS.GetXml
                oElmt.SetAttribute("count", oDS.Tables("Addresses").Rows.Count)
                oElmt.InnerText = oXML.DocumentElement.InnerXml
                Return oElmt
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs("ListOptOuts", "", ex, ""))
                Return Nothing
            End Try
        End Function

        Private Function CheckIfAncestorPage(ByVal oPage As XmlDocument, ByVal nChildPageId As Long, ByVal nCheckPageId As Long) As Boolean
            ' If Not PerfMon Is Nothing Then PerfMon.Log("stdTools", "CheckIsParentPage")
            Try
                'check if the pages have the same parent then return false
                If nChildPageId = 0 Or nCheckPageId = 0 Or nChildPageId = nCheckPageId Then Return False

                'check if page exists on menu if return false
                Dim oParentElmt As XmlElement
                oParentElmt = oPage.SelectSingleNode("descendant-or-self::MenuItem[@id='" & nCheckPageId & "']")
                If oParentElmt Is Nothing Then Return False

                'check if parent is ancestor of child then return true
                Dim oChildElmt As XmlElement
                oChildElmt = oParentElmt.SelectSingleNode("descendant-or-self::MenuItem[@id='" & nChildPageId & "']")
                If oChildElmt Is Nothing Then Return False Else Return True

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "CheckIsParentPage", ex, ""))
            End Try
        End Function

        Public Function CleanAuditOrphans() As String
            PerfMon.Log("dbTools", "CleanAuditOrphans")
            Try
                Dim oDs As New DataSet
                Dim cSQL As String
                Dim oDr As DataRow
                Dim bHasChild As Boolean = False
                Dim oDRel As DataRelation
                Dim nNoDel As Integer = 0

                cSQL = "Select * from tblAudit"
                oDs = Me.getDataSetForUpdate(cSQL, "Audit", "CleanOrphans")
                cSQL = "Select name from sysobjects where xtype='U' and type='U' and userstat=1"
                Me.addTableToDataSet(oDs, cSQL, "Tables")

                'fill the dataset with all the used keys
                For Each oDr In oDs.Tables("Tables").Rows
                    If Not oDr("name") = "tblAudit" Then
                        cSQL = "Select nAuditId from " & oDr("name")
                        Try
                            Me.addTableToDataSet(oDs, cSQL, oDr("name"))
                            oDs.Relations.Add("Audit" & oDr("name"), oDs.Tables("Audit").Columns("nAuditKey"), oDs.Tables(oDr("name")).Columns("nAuditId"), False)
                            oDs.Relations("Audit" & oDr("name")).Nested = True
                        Catch ex As Exception
                            'clean the exception text as it is more than likely
                            'the error appears as the table does not bother with 
                            'auditing at all
                            'msException = ""
                        End Try
                    End If
                Next

                For Each oDr In oDs.Tables("Audit").Rows
                    bHasChild = False
                    For Each oDRel In oDs.Relations
                        If UBound(oDr.GetChildRows(oDRel.RelationName)) = 0 Then
                            bHasChild = True
                            Exit For
                        End If
                    Next
                    If Not bHasChild Then
                        oDr.Delete()
                        nNoDel += 1
                    End If
                Next

                If nNoDel > 0 Then Me.updateDataset(oDs, "Audit", False)
                Dim sResponse As String = "Audit table cleaned, " & nNoDel & " records removed.</br>"

                'Clean Orphan RelatedContent.
                cSQL = "Delete tblContentRelation  from tblContentRelation r left join tblContent c on r.nContentChildId = c.nContentKey where c.nContentKey is null"
                sResponse += "Deleted Orphaned Child Content Relations - " & Me.ExeProcessSql(cSQL) & "</br>"

                cSQL = "Delete tblContentRelation  from tblContentRelation r left join tblContent c on r.nContentParentId = c.nContentKey where c.nContentKey is null"
                sResponse += "Deleted Orphaned Parent Content Relations - " & Me.ExeProcessSql(cSQL) & "</br>"

                cSQL = "Delete tblContentLocation  from tblContentLocation l left join tblContent c on l.nContentId = c.nContentKey where c.nContentKey is null"
                sResponse += "Deleted Orphaned Content Locations - " & Me.ExeProcessSql(cSQL) & "</br>"


                Return sResponse



            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "Clean Audit Orphans", ex, ""))
                Return "Error cleaning Audit table"
            End Try
        End Function

        Public Function cleanLocations() As String
            PerfMon.Log("dbTools", "cleanLocations")
            Dim cSQL1 As String
            Dim oDs As DataSet
            Dim oDV As DataView
            Dim oDr1 As DataRow
            Dim oDr2 As DataRow
            Dim nContentId As Integer
            Try
                cSQL1 = "Select * from tblContentLocation"
                oDs = getDataSetForUpdate(cSQL1, "tblcontentLocation")
                For Each oDr1 In oDs.Tables("tblContentLocation").Rows
                    nContentId = oDr1("nContentId")
                    oDV = New DataView(oDs.Tables("tblContentLocation"))
                    oDV.RowFilter = "nContentId = " & nContentId
                    oDV.Sort = "bPrimary DESC, nContentLocationKey"
                    Dim nI As Integer
                    For nI = 0 To oDV.Count - 1
                        oDr2 = oDV.Item(nI).Row
                        If nI = 0 Then
                            oDr2("bPrimary") = 1
                        Else
                            oDr2("bPrimary") = 0
                        End If
                    Next
                Next
                updateDataset(oDs, "tblContentLocation")
                Return "Finished"
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "exeProcessSQLfromFile", ex, ""))
                Return "Error"
            End Try
        End Function

        Public Function LocateContentSearch(ByVal nRootNode As Integer, ByVal cSchemaName As String, ByVal bChildren As String, ByVal cSearchExpression As String, ByVal nCurrentPage As Integer) As XmlNodeList
            Dim oDS As DataSet
            Dim oDT As DataTable
            Dim oDC As DataColumn
            Dim oXML As New XmlDocument
            Dim oResults As XmlNodeList
            Dim cSQL As String
            Dim cWhere As String = ""
            Dim oPages As XmlNodeList
            Dim oElmt As XmlElement
            PerfMon.Log("DBHelper", "RelatedContentSearch")
            Try
                Dim sSearch As String = cSearchExpression
                'remove reserved words
                sSearch = Replace(" " & sSearch & " ", " the ", "")
                sSearch = Replace(" " & sSearch & " ", " and ", "")
                sSearch = Replace(" " & sSearch & " ", " if ", "")
                sSearch = Replace(" " & sSearch & " ", " then ", "")
                sSearch = Replace(" " & sSearch & " ", " or ", "")
                sSearch = Trim(sSearch)
                Dim i As Integer = 0

                Dim oMenuElmt As XmlElement = myWeb.GetStructureXML()

                cSQL = "SELECT nContentKey as id, cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, tblContent.cContentXmlDetail as detail  FROM tblContent "

                If nRootNode > 1 Then
                    cSQL &= "INNER JOIN tblContentLocation ON tblContent.nContentKey = tblContentLocation.nContentId"
                    If bChildren Then
                        oPages = oMenuElmt.SelectNodes("descendant-or-self::MenuItem[@id='" & nRootNode & "']/descendant-or-self::MenuItem")
                        cWhere &= " tblContentLocation.nStructId IN("
                        Dim nPages As Integer = oPages.Count
                        For Each oElmt In oPages
                            cWhere &= oElmt.GetAttribute("id")
                            nPages = nPages - 1
                            If nPages <> 0 Then
                                cWhere &= ","
                            End If
                        Next
                        cWhere &= ") "
                    Else
                        cWhere &= " tblContentLocation.nStructId=" & nRootNode
                    End If
                End If

                'get only one type
                If Not cWhere = "" Then cWhere &= " AND "
                If Not cSchemaName = "" Then cWhere &= " (tblContent.cContentSchemaName = '" & cSchemaName & "')"
                If Not cWhere = "" Then cWhere &= " AND "
                'now the search expression
                cWhere &= " ((cast(tblContent.cContentXmlBrief as varchar(max)) LIKE '%" & sSearch & "%') OR (cast(tblContent.cContentXmlDetail as varchar(max))LIKE '%" & sSearch & "%'))"
                If Not cWhere = "" Then cWhere = " WHERE " & cWhere

                cSQL &= cWhere & " ORDER BY cContentName"
                oDS = GetDataSet(cSQL, "Content", "Contents")

                For Each oDT In oDS.Tables
                    For Each oDC In oDT.Columns
                        oDC.ColumnMapping = MappingType.Attribute
                    Next
                Next
                oDS.Tables("Content").Columns("content").ColumnMapping = MappingType.Element
                oDS.Tables("Content").Columns("detail").ColumnMapping = MappingType.Element

                oXML.InnerXml = Replace(Replace(oDS.GetXml, "&gt;", ">"), "&lt;", "<")
                oResults = oXML.DocumentElement.SelectNodes("Content")
                Return oResults


            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "LocateContentSearch", ex, ""))
                Return Nothing
            End Try
        End Function


        Public Function emailActivity(ByVal nUserDirId As Int16, Optional ByVal cActivityFullDetail As String = "", Optional ByVal cEmailRecipient As String = "", Optional ByVal cEmailSender As String = "", Optional ByVal cActivityXml As String = "") As Long

            Dim sSql As String
            Try
                If checkTableColumnExists("tblEmailActivityLog", "cActivityXml") Then
                    sSql = "Insert Into tblEmailActivityLog (nUserDirId, dDateTime,  cEmailRecipient, cEmailSender, cActivityDetail, cActivityXml) " & _
                                    "values (" & _
                                    nUserDirId & ", " & _
                                    Eonic.Tools.Database.SqlDate(Now, True) & ", " & _
                                    "'" & SqlFmt(Left(cEmailRecipient, 255)) & "', " & _
                                    "'" & SqlFmt(Left(cEmailSender, 255)) & "', " & _
                                    "'" & SqlFmt(cActivityFullDetail) & "', " & _
                                    " '" & SqlFmt(cActivityXml) & "')"
                Else
                    sSql = "Insert Into tblEmailActivityLog (nUserDirId, dDateTime,  cEmailRecipient, cEmailSender, cActivityDetail) " & _
                                    "values (" & _
                                    nUserDirId & ", " & _
                                    Eonic.Tools.Database.SqlDate(Now, True) & ", " & _
                                    "'" & SqlFmt(Left(cEmailRecipient, 255)) & "', " & _
                                    "'" & SqlFmt(Left(cEmailSender, 255)) & "', " & _
                                    "'" & SqlFmt(cActivityFullDetail) & "')"
                End If

                Return MyBase.GetIdInsertSql(sSql)

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "emailActivity", ex, ""))
                'Close()
            End Try

        End Function

        Public Sub RemoveDuplicateDirRelations()
            Try
                Dim sSQL As String = "SELECT nRelKey"
                sSQL &= " FROM tblDirectoryRelation"
                sSQL &= " WHERE (((SELECT COUNT(nRelKey) AS [COUNT]"
                sSQL &= " FROM tblDirectoryRelation Rel"
                sSQL &= " WHERE (nDirChildId = tblDirectoryRelation.nDirChildId) AND (nDirParentId = tblDirectoryRelation.ndirparentId) AND (nRelKey < tblDirectoryRelation.nrelkey))) > 0)"
                Dim oDr As SqlDataReader = myWeb.moDbHelper.getDataReader(sSQL)
                Do While oDr.Read
                    DeleteObject(dbHelper.objectTypes.DirectoryRelation, oDr(0))
                Loop
                oDr.Close()
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "RemoveDuplicateDirRelations", ex, ""))
            End Try
        End Sub

        Public Function CommitLogToDB(ByVal nEventType As Eonic.Web.dbHelper.ActivityType, ByVal nUserId As Integer, ByVal cSessionId As String, ByVal dDateTime As Date, Optional ByVal nPrimaryId As Integer = 0, Optional ByVal nSecondaryId As Integer = 0, Optional ByVal cDetail As String = "", Optional ByVal bOverrideLoggingChecks As Boolean = False) As Integer

            If Not myWeb Is Nothing And Not (bOverrideLoggingChecks) Then
                If Not myWeb.Features.ContainsKey("ActivityReporting") Then Exit Function
            End If

            Try

                'TS 04/12/11 this logall feature has been added in by someone but seems to break functionaility 
                ' unless logAll is set, no reference to setting logAll is found anywhere else.
                ' can only assume this was added to make some kind of overload work, but breaks standard activityLogging/Reporting
                ' any logic as to wether this should be run prior to this.

                'If Not goSession Is Nothing And Not (bOverrideLoggingChecks) Then
                '    If Not goSession("LogAll") = 1 And Not goSession("LogAll") = "true" And Not goSession("LogAll") = "on" Then
                '        'not logging everything if no user and logall not turned on
                '        Exit Function
                '    End If
                'End If

                'TS 04/12/11 however have assumed an overload sets negative values too so have reversed the logic.
                If Not goSession Is Nothing And Not (bOverrideLoggingChecks) Then
                    If goSession("LogAll") = "0" Or goSession("LogAll") = "false" Or goSession("LogAll") = "off" Then
                        'not logging everything if no user and logall not turned on
                        Exit Function
                    End If
                End If

                If myWeb Is Nothing Then
                    gbIPLogging = False
                End If

                Dim cSQL As String = "INSERT INTO tblActivityLog (nUserDirId, nStructId, nArtId, dDateTime, nActivityType, cActivityDetail, cSessionId"
                If gbIPLogging Then cSQL &= ",cIPAddress"
                cSQL &= ") VALUES ("
                cSQL &= nUserId & ","
                cSQL &= nPrimaryId & ","
                cSQL &= nSecondaryId & ","
                cSQL &= Eonic.Tools.Database.SqlDate(dDateTime, True) & ","
                cSQL &= nEventType & ","
                cSQL &= "'" & cDetail & "',"
                cSQL &= "'" & cSessionId & "'"
                If gbIPLogging Then cSQL &= ",'" & SqlFmt(Left(myWeb.moRequest.ServerVariables("REMOTE_ADDR"), 15)) & "'"
                cSQL &= ")"

                Return MyBase.GetIdInsertSql(cSQL)

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "CommitLogToDB", ex, ""))
                Return 0
            End Try
        End Function

        Public Function CheckCode(ByVal cCode As String, Optional ByVal cCodeSet As String = "") As Boolean

            Dim cSql As String = ""
            Try
                Dim cCurDate As String = Eonic.Tools.Database.SqlDate(Now, True)
                cSql = " SELECT tblCodes.nCodeKey" & _
                    " FROM tblCodes INNER JOIN tblAudit ON tblCodes.nAuditId = tblAudit.nAuditKey" & _
                    " WHERE (tblCodes.cCode = '" & cCode & "')" & _
                    " AND (tblCodes.nUseId IS NULL OR tblCodes.nUseId = 0)" & _
                    " AND (tblAudit.dPublishDate <= " & cCurDate & " OR tblAudit.dPublishDate IS NULL)" & _
                    " AND (tblAudit.dExpireDate >= " & cCurDate & " OR tblAudit.dExpireDate IS NULL)" & _
                    " AND (tblAudit.nStatus = 1 OR tblAudit.nStatus = - 1 OR tblAudit.nStatus IS NULL)"
                If cCodeSet <> "" Then
                    cSql += " AND nCodeParentId IN (" & cCodeSet & ")"
                End If
                If MyBase.GetDataValue(cSql, , , 0) > 0 Then
                    Return True
                Else
                    Return False
                End If
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "CheckCode", ex, ""))
                Return False
            End Try
        End Function

        Public Function IssueCode(ByVal cCodeSet As String, ByVal nUseId As String) As String

            Dim cSql As String = ""
            Dim returnCode As String = ""

            Try
                Dim cCurDate As String = Eonic.Tools.Database.SqlDate(Now, True)
                cSql = " SELECT top 1 tblCodes.cCode" & _
                    " FROM tblCodes INNER JOIN tblAudit ON tblCodes.nAuditId = tblAudit.nAuditKey" & _
                    " INNER JOIN tblCodes partlb ON tblCodes.nCodeParentId = partlb.nCodeKey" & _
                    " WHERE (tblCodes.nUseId IS NULL OR tblCodes.nUseId = 0)" & _
                    " AND (tblAudit.dPublishDate <= " & cCurDate & " OR tblAudit.dPublishDate IS NULL)" & _
                    " AND (tblAudit.dExpireDate >= " & cCurDate & " OR tblAudit.dExpireDate IS NULL)" & _
                    " AND (tblAudit.nStatus = 1 OR tblAudit.nStatus = - 1 OR tblAudit.nStatus IS NULL)"
                cSql += " AND partlb.cCodeName like '" & cCodeSet & "'"

                returnCode = MyBase.GetDataValue(cSql, , , 0)

                If returnCode <> "" Then
                    UseCode(returnCode, nUseId)
                    Return returnCode
                Else
                    Return ""
                End If
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "CheckCode", ex, ""))
                Return False
            End Try
        End Function

        Public Function IssueCode(ByVal nCodeSet As Integer, ByVal nUseId As Integer, Optional ByVal UseNow As Boolean = True, Optional CodeXml As XmlElement = Nothing) As String

            Dim cSql As String = ""
            Dim returnCode As String = ""

            Try
                Dim cCurDate As String = Eonic.Tools.Database.SqlDate(Now, True)
                cSql = " SELECT top 1 tblCodes.cCode" & _
                    " FROM tblCodes INNER JOIN tblAudit ON tblCodes.nAuditId = tblAudit.nAuditKey" & _
                    " INNER JOIN tblCodes partlb ON tblCodes.nCodeParentId = partlb.nCodeKey" & _
                    " WHERE (tblCodes.nUseId IS NULL AND tblCodes.nIssuedDirId is NULL)" & _
                    " AND (tblAudit.dPublishDate <= " & cCurDate & " OR tblAudit.dPublishDate IS NULL)" & _
                    " AND (tblAudit.dExpireDate >= " & cCurDate & " OR tblAudit.dExpireDate IS NULL)" & _
                    " AND (tblAudit.nStatus = 1 OR tblAudit.nStatus = - 1 OR tblAudit.nStatus IS NULL)"
                cSql += " AND partlb.nCodeKey = '" & nCodeSet & "'"

                returnCode = MyBase.GetDataValue(cSql, , , 0)
                If UseNow = False Then
                    If myWeb.mnUserId > 0 Then
                        Dim nKey As Integer = MyBase.GetDataValue( _
                                           " SELECT tblCodes.nCodeKey" & _
                                           " FROM tblCodes INNER JOIN tblAudit ON tblCodes.nAuditId = tblAudit.nAuditKey" & _
                                           " WHERE (tblCodes.cCode = '" & returnCode & "')" & _
                                           " AND (tblCodes.nUseId IS NULL OR tblCodes.nUseId = 0)" & _
                                           " AND (tblAudit.dPublishDate <= " & cCurDate & " OR tblAudit.dPublishDate IS NULL)" & _
                                           " AND (tblAudit.dExpireDate >= " & cCurDate & " OR tblAudit.dExpireDate IS NULL)" & _
                                           " AND (tblAudit.nStatus = 1 OR tblAudit.nStatus = - 1 OR tblAudit.nStatus IS NULL)", , , 0)

                        MyBase.ExeProcessSql("UPDATE tblCodes SET nIssuedDirId = " & myWeb.mnUserId & ", dIssuedDate = " & cCurDate & " WHERE nCodeKey = " & nKey)

                        If Not CodeXml Is Nothing Then
                            MyBase.ExeProcessSql("UPDATE tblCodes SET xUsageData = '" & SqlFmt(CodeXml.OuterXml) & "' WHERE nCodeKey = " & nKey)
                        End If

                        Return returnCode
                    Else
                        Return ""
                    End If
                Else
                    If returnCode <> "" Then
                        UseCode(returnCode, nUseId)
                        Return returnCode
                    Else
                        Return ""
                    End If

                End If



            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "CheckCode", ex, ""))
                Return False
            End Try
        End Function

        Public Function UseCode(ByVal cCode As String, ByVal nUseID As Integer) As Boolean
            Try
                'recheck the code
                Dim cCurDate As String = Eonic.Tools.Database.SqlDate(Now, True)
                Dim nKey As Integer = MyBase.GetDataValue( _
                                    " SELECT tblCodes.nCodeKey" & _
                                    " FROM tblCodes INNER JOIN tblAudit ON tblCodes.nAuditId = tblAudit.nAuditKey" & _
                                    " WHERE (tblCodes.cCode = '" & cCode & "')" & _
                                    " AND (tblCodes.nUseId IS NULL OR tblCodes.nUseId = 0)" & _
                                    " AND (tblAudit.dPublishDate <= " & cCurDate & " OR tblAudit.dPublishDate IS NULL)" & _
                                    " AND (tblAudit.dExpireDate >= " & cCurDate & " OR tblAudit.dExpireDate IS NULL)" & _
                                    " AND (tblAudit.nStatus = 1 OR tblAudit.nStatus = - 1 OR tblAudit.nStatus IS NULL)", , , 0)
                If nKey > 0 Then
                    MyBase.ExeProcessSql("UPDATE tblCodes SET nUseID = " & nUseID & ", dUseDate = " & cCurDate & " WHERE nCodeKey = " & nKey)
                    Return True
                Else
                    Return False
                End If
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "UseCode", ex, ""))
                Return False
            End Try
        End Function

        Public Function UseCode(ByVal nCodeId As Integer, ByVal nUseID As Integer, ByVal nOrderId As Long) As Boolean
            Try
                'recheck the code
                Dim cCurDate As String = Eonic.Tools.Database.SqlDate(Now, True)
                Dim nKey As Integer = MyBase.GetDataValue( _
                                    " SELECT tblCodes.nCodeKey" & _
                                    " FROM tblCodes INNER JOIN tblAudit ON tblCodes.nAuditId = tblAudit.nAuditKey" & _
                                    " WHERE (tblCodes.nCodeKey = " & nCodeId & ")" & _
                                    " AND (tblCodes.nUseId IS NULL OR tblCodes.nUseId = 0)" & _
                                    " AND (tblAudit.dPublishDate <= " & cCurDate & " OR tblAudit.dPublishDate IS NULL)" & _
                                    " AND (tblAudit.dExpireDate >= " & cCurDate & " OR tblAudit.dExpireDate IS NULL)" & _
                                    " AND (tblAudit.nStatus = 1 OR tblAudit.nStatus = - 1 OR tblAudit.nStatus IS NULL)", , , 0)
                If nKey > 0 Then
                    MyBase.ExeProcessSql("UPDATE tblCodes SET nUseID = " & nUseID & ", nOrderId = " & nOrderId & ", dUseDate = " & cCurDate & " WHERE nCodeKey = " & nKey)
                    Return True
                Else
                    Return False
                End If
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "UseCode", ex, ""))
                Return False
            End Try
        End Function

        Public Function DBN2Str(ByVal Frm As Object, Optional ByVal useMarks As Boolean = False, Optional ByVal NullText As Boolean = False) As String
            'PerfMon.Log("dbTools", "DBN2Str")
            Dim strNull As String
            strNull = ""

            If Frm Is Nothing Then
                strNull = "Null"
                GoTo ReturnMe
            End If
            If NullText Then strNull = "Null"
            If useMarks Then strNull = "''"
ReturnMe:
            If IsDBNull(Frm) Then Return strNull Else Return IIf(useMarks, "'" & Replace(CStr(Frm), "'", "''") & "'", CStr(Frm))
        End Function
        Public Function DBN2int(ByVal Frm As Object, Optional ByVal NullText As Boolean = False) As Object
            'PerfMon.Log("dbTools", "DBN2int")
            If IsDBNull(Frm) Then Return IIf(NullText, "Null", 0) Else Return CInt(Frm)
        End Function
        Public Function DBN2dte(ByVal Frm As Object, Optional ByVal NullText As Boolean = False) As Object
            'PerfMon.Log("dbTools", "DBN2dte")
            If IsDBNull(Frm) Then Return IIf(NullText, "Null", #12:00:00 AM#) Else Return CDate(Frm)
        End Function




        Public Function getDataSetForUpdate(ByVal sSql As String, ByVal tableName As String, Optional ByVal datasetName As String = "") As DataSet
            'PerfMon.Log("dbTools", "getDataSetForUpdate")
            Dim oDs As DataSet
            Dim cProcessInfo As String = "Running SQL:  " & sSql
            Try

                moDataAdpt = New SqlDataAdapter
                If datasetName = "" Then
                    oDs = New DataSet
                Else
                    oDs = New DataSet(datasetName)
                End If

                If oConn.State = ConnectionState.Closed Then oConn.Open()

                Dim oSqlCmd As SqlCommand = New SqlCommand(sSql, oConn)
                moDataAdpt.SelectCommand = oSqlCmd
                Dim cb As SqlCommandBuilder = New SqlCommandBuilder(moDataAdpt)
                moDataAdpt.TableMappings.Add(tableName, tableName)

                moDataAdpt.Fill(oDs, tableName)
                moDataAdpt.Update(oDs, tableName)
                getDataSetForUpdate = oDs

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getDataSetForUpdate", ex, cProcessInfo))
                Return Nothing
            End Try

        End Function

        Public Function updateDataset(ByRef oDs As DataSet, ByVal sTableName As String, Optional ByVal bReUse As Boolean = False) As Boolean
            'PerfMon.Log("dbTools", "updateDataset")
            Dim cProcessInfo As String = "returnDataSet"

            Try
                Dim oxx As New SqlClient.SqlCommandBuilder(moDataAdpt)
                moDataAdpt.DeleteCommand = oxx.GetDeleteCommand()
                moDataAdpt.InsertCommand = oxx.GetInsertCommand()
                moDataAdpt.UpdateCommand = oxx.GetUpdateCommand()
                moDataAdpt.Update(oDs, sTableName)

                If Not bReUse Then
                    'lets tidy up
                    moDataAdpt = Nothing
                    oDs.Clear()
                    oDs = Nothing
                End If


            Catch ex As Exception
                Dim oXml As XmlDocument = New XmlDataDocument(oDs)
                cProcessInfo = oXml.OuterXml
                'Return False
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getDataSet", ex, cProcessInfo))
            End Try

        End Function

        Public Function saveInstance(ByRef instanceElmt As XmlElement, ByVal targetTable As String, ByVal keyField As String, Optional ByVal whereStmt As String = "") As Integer
            PerfMon.Log("dbTools", "saveInstance")

            'Generic function to save xml to a database, picking only the relevent fields out of the XML

            Dim keyValue As String = Nothing
            Dim sSql As String
            Dim cProcessInfo As String = "AddRStoXMLnode"
            Dim nUpdateCount As Long
            Dim oRow As DataRow
            Dim column As DataColumn
            Dim oDs As New DataSet

            Try
                If oConn.State = ConnectionState.Closed Then oConn.Open()

                'Identify the keyValue and build the initial SQL Statement.
                If whereStmt = "" Then
                    If Not instanceElmt.SelectSingleNode("descendant-or-self::" & keyField) Is Nothing Then
                        keyValue = instanceElmt.SelectSingleNode("descendant-or-self::" & keyField).InnerText
                        If keyValue = "" Then keyValue = "-1"
                    Else
                        keyValue = "-1"
                    End If
                    sSql = "select * from " & targetTable & " where " & keyField & " = " & keyValue
                Else
                    sSql = "select * from " & targetTable & " where " & whereStmt
                End If

                cProcessInfo = "error running SQL: " & sSql

                Dim oDataAdpt As New SqlDataAdapter(sSql, oConn)
                'autogenerate commands
                Dim cmdBuilder As SqlCommandBuilder = New SqlCommandBuilder(oDataAdpt)
                oDataAdpt.Fill(oDs, targetTable)

                If oDs.Tables(targetTable).Rows.Count > 0 Then ' CASE FOR UPDATE
                    oRow = oDs.Tables(targetTable).Rows(0)
                    oRow.BeginEdit()
                    For Each column In oDs.Tables(targetTable).Columns
                        cProcessInfo = targetTable & " Update: "
                        If Not instanceElmt.SelectSingleNode("*/" & column.ToString) Is Nothing Then
                            cProcessInfo += column.ToString & " - " & instanceElmt.SelectSingleNode("*/" & column.ToString).InnerXml
                            oRow(column) = convertDtXMLtoSQL(column.DataType, instanceElmt.SelectSingleNode("*/" & column.ToString).InnerXml, IIf(InStr(column.ToString, "Xml") > 0, True, False))
                        End If
                    Next
                    oRow.EndEdit()

                    'run the update
                    nUpdateCount = oDataAdpt.Update(oDs, targetTable)

                Else ' CASE FOR INSERT
                    oRow = oDs.Tables(targetTable).NewRow
                    For Each column In oDs.Tables(targetTable).Columns

                        If Not (instanceElmt.SelectSingleNode("*/" & column.ToString) Is Nothing) Then
                            'don't want to set the value on the key feild on insert
                            cProcessInfo = targetTable & " Insert: "
                            If Not (column.ToString = keyField) Then
                                cProcessInfo += column.ToString & " - " & instanceElmt.SelectSingleNode("*/" & column.ToString).InnerXml
                                oRow(column) = convertDtXMLtoSQL(column.DataType, instanceElmt.SelectSingleNode("*/" & column.ToString).InnerXml, IIf(InStr(column.ToString, "Xml") > 0, True, False))
                            End If
                        End If
                    Next
                    cProcessInfo = targetTable & " Add Rows"
                    oDs.Tables(targetTable).Rows.Add(oRow)

                    'Run the insert and get back the new id
                    cProcessInfo = targetTable & " Get ID"
                    Dim ExecuteQuery As String = "SELECT @@IDENTITY"
                    Dim getid As New SqlCommand(ExecuteQuery, oConn)
                    'cProcessInfo = targetTable & " Get ID: nUpdateCount : " & oDs.GetXml
                    nUpdateCount = oDataAdpt.Update(oDs, targetTable)
                    'cProcessInfo = targetTable & " Get ID: ExecuteScalar"
                    keyValue = getid.ExecuteScalar()
                    cProcessInfo = targetTable & " ID Retrieved: " & CStr(keyValue)
                End If

                If nUpdateCount = 0 Then
                    Err.Raise(1000, mcModuleName, "No Update")
                End If

                oDs.Dispose()
                oDs = Nothing

                oDataAdpt.Dispose()
                oDataAdpt = Nothing
                PerfMon.Log("dbTools", "saveInstance-End", cProcessInfo)
                Return keyValue

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "saveInstance", ex, cProcessInfo))
            End Try

        End Function
        Public Function updateInstanceField(ByVal targetTable As Eonic.Web.dbHelper.objectTypes, ByVal keyValue As Integer, ByVal fieldName As String, ByVal value As String, Optional ByVal whereStmt As String = "") As Integer
            Dim cProcessInfo As String = ""
            Dim oInstance As XmlElement = myWeb.moPageXml.CreateElement("Instance")

            Try

                oInstance.InnerXml = Me.getObjectInstance(targetTable, keyValue, whereStmt)
                oInstance.SelectSingleNode("*/" & fieldName).InnerText = value
                Me.setObjectInstance(targetTable, oInstance, keyValue)

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "updateInstanceField", ex, cProcessInfo))
            End Try

        End Function

        Public Overloads Function getHashTable(ByVal sSql As String, ByVal sNameField As String, ByRef sValueField As String) As Hashtable
            'PerfMon.Log("dbTools", "getHashTable")
            Dim cProcessInfo As String = ""
            Try

                Return MyBase.getHashTable(sSql, sNameField, sValueField)

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getDataSet", ex, cProcessInfo))
                Return Nothing
            End Try

        End Function

        Public Function getDatasetAddRows(ByVal sSQL As Array, ByVal cTableName As String, Optional ByVal cDatasetName As String = "") As DataSet
            'PerfMon.Log("dbTools", "getDatasetAddRows")
            'Creates a Dataset and a datatable
            'with specified names
            'runs multiple sql fills to add rows
            'to the table
            If sSQL Is Nothing Then Return Nothing
            Dim cProcessInfo As String = ""
            Try
                Dim cSQL As String
                Dim nI As Integer
                Dim oDs As New DataSet


                Dim oDA As SqlDataAdapter

                For nI = 0 To UBound(sSQL)
                    cSQL = sSQL(nI)
                    cProcessInfo = "Running SQL:  " & cSQL
                    oDA = New SqlDataAdapter(cSQL, oConn)
                    If Not cDatasetName = "" Then oDs.DataSetName = cDatasetName
                    oDA.Fill(oDs, cTableName)
                Next

                Return oDs

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "GetDatasetAddRows", ex, cProcessInfo))
                Return Nothing
            End Try
        End Function

        Public Function convertDtXMLtoSQL(ByVal datatype As System.Type, ByVal value As Object, Optional ByVal bKeepXml As Boolean = True) As Object
            'PerfMon.Log("dbTools", "convertDtXMLtoSQL")
            Dim cProcessInfo As String = "Converting Datatype:  " & datatype.Name
            Try
                Select Case datatype.Name
                    Case "Boolean"
                        If value = "true" Then
                            Return True
                        Else
                            Return False
                        End If
                    Case "DateTime"
                        If IsDate(value) And Not value.ToString.StartsWith("0001-01-01") Then
                            Return CDate(value)
                        Else
                            Return System.DBNull.Value
                        End If
                    Case "Double", "Int32", "Int16", "Decimal"
                        If IsNumeric(value) Then
                            Return CDbl(value)
                        Else
                            Return System.DBNull.Value
                        End If
                    Case "String"

                        If Left(Trim(value.ToString), 1) = "<" And Right(Trim(value.ToString), 1) = ">" Then
                            'we can assume this is XML
                            bKeepXml = True
                        End If

                        If bKeepXml Then
                            Return value
                        Else
                            Return convertEntitiesToString(value)
                        End If
                    Case Else
                        Return value
                End Select
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "convertDtXMLtoSQL", ex, cProcessInfo))
                Return Nothing
            End Try
        End Function

        Public Function convertDtSQLtoXML(ByVal datatype As System.Type, ByVal value As Object) As String
            'PerfMon.Log("dbTools", "convertDtSQLtoXML")
            Dim cProcessInfo As String = "Converting Datatype:  " & datatype.Name
            Try
                If IsDBNull(value) Then
                    Return ""
                Else
                    Select Case datatype.Name
                        Case "Boolean"
                            If value = True Then
                                Return "true"
                            Else
                                Return "false"
                            End If
                        Case "DateTime"
                            If IsDate(value) And Not IsDBNull(value) Then
                                Return Eonic.Tools.Xml.XmlDate(value)
                            Else
                                Return ""
                            End If
                        Case "Double", "Int32", "Int16"
                            If IsNumeric(value) Then
                                Return CStr(value)
                            Else
                                Return ""
                            End If
                        Case "DBNull"
                            Return ""
                        Case Else
                            Return CStr(value)
                    End Select
                End If

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "convertDtSQLtoXML", ex, cProcessInfo))
                Return Nothing
            End Try
        End Function

        Public Function createDB(ByVal DatabaseName As String) As Boolean
            Dim cProcessInfo As String = "createDB"
            Try
                Dim oConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("eonic/web")
                Dim myConn As SqlConnection = New SqlConnection("Data Source=" & oConfig("DatabaseServer") & "; Initial Catalog=master;" & oConfig("DatabaseAuth"))
                Dim sSql As String
                oConn = myConn

                sSql = "select db_id('" & DatabaseName & "')"
                If ExeProcessSqlScalar(sSql) Is Nothing Then
                    ExeProcessSql("CREATE DATABASE " & DatabaseName)
                Else
                    oConn = Nothing
                    Return False
                End If
                oConn = Nothing
                Return True

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "createDB", ex, ""))
                Return Nothing
            End Try
        End Function


        Public Function isClonedPage(ByVal nPageId As Integer) As Boolean

            PerfMon.Log("DBHelper", "isClonedPage")

            Dim cProcessInfo As String = ""
            Try
                cProcessInfo = "Page ID: " & nPageId
                Return (getClonePageID(nPageId) > 0)

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "isClonedPage", ex, cProcessInfo))
                Return False
            End Try
        End Function

        Public Function getClonePageID(ByVal nPageId As Integer) As Integer

            PerfMon.Log("DBHelper", "getClonePageID")

            Dim cProcessInfo As String = ""
            Try
                cProcessInfo = "Page ID: " & nPageId

                Return GetDataValue("SELECT nCloneStructId FROM tblContentStructure WHERE nStructKey = " & nPageId, , , 0)

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getClonePageID", ex, cProcessInfo))
                Return 0
            End Try
        End Function

        Public Function getUnorphanedAncestor(ByVal nParentContentId As Integer, Optional ByVal nLevel As Long = 99) As Hashtable

            PerfMon.Log("DBHelper", "getUnorphanedAncestor")

            Dim cProcessInfo As String = ""
            Dim hReturn As Hashtable = Nothing
            Dim nPageId As Long
            Dim cSql As String = ""
            Try
                cProcessInfo = "Parent Content ID: " & nParentContentId

                ' Arbitrarily stop recursion
                If nLevel < 15 Then

                    ' Get the primary location of the parent content
                    ' Assumes that there will only be one parent id.
                    cSql = "SELECT	TOP 1 nStructId " _
                            & "FROM tblContentLocation " _
                            & "WHERE	(nContentId = " & SqlFmt(nParentContentId) & ") AND (bPrimary = 1) "
                    nPageId = GetDataValue(cSql, , , 0)

                    ' If the page retrieved is null (0) then this is orphan content
                    ' Let's see if there's any other parents to this content
                    ' Assume that the content has 0 or 1 related parents
                    If nPageId = 0 Then

                        cSql = "SELECT TOP 1 nContentParentId " _
                                & "FROM dbo.tblContentRelation " _
                                & "WHERE nContentChildId = " & SqlFmt(nParentContentId)
                        nParentContentId = GetDataValue(cSql, , , 0)

                        ' If there is a parent related to the content then try to see if that's got a location
                        If nParentContentId > 0 Then
                            hReturn = getUnorphanedAncestor(nParentContentId, nLevel + 1)
                        End If

                    Else

                        ' This is not orphaned content, so this is what we want to find
                        ' Return the request type of info
                        hReturn = New Hashtable
                        hReturn.Add("id", nParentContentId)
                        hReturn.Add("page", nPageId)

                    End If

                End If

                Return hReturn

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getUnorphanedAncestor", ex, cProcessInfo))
                Return hReturn
            End Try


        End Function

        Public Function GetMostPopularSearches(ByVal numberToReturn As Integer, ByVal filterStartString As String) As XmlElement
            PerfMon.Log("dbHelper", "GetMostPopularSearches")

            Dim result As XmlElement = Nothing

            Try
                Dim popularSearches As XmlElement = myWeb.moPageXml.CreateElement("popularSearches")

                Dim filter As String = ""

                If Not String.IsNullOrEmpty(filterStartString) Then
                    ' Create the predictive filter, with wildcards taken out
                    filter = " AND cActivityDetail LIKE '" & Text.RegularExpressions.Regex.Replace(SqlFmt(filterStartString), "[%\*]", "") & "%'"
                End If

                If Not numberToReturn > 0 Then numberToReturn = 5

                Dim popularSearchesQuery As String = "SELECT TOP " & numberToReturn & " cActivityDetail " _
                                                    & "FROM dbo.tblActivityLog " _
                                                    & "WHERE nActivityType=" & ActivityType.Search & " " _
                                                    & filter & " " _
                                                    & "GROUP BY cActivityDetail " _
                                                    & "HAVING SUM(nOtherId) > 0 " _
                                                    & "ORDER BY COUNT(*) DESC, SUM(nOtherId) DESC "

                Dim query As SqlDataReader = getDataReader(popularSearchesQuery)

                While query.Read
                    result = myWeb.moPageXml.CreateElement("search")
                    result.InnerText = query(0).ToString
                    popularSearches.AppendChild(result)
                End While

                query.Close()

                Return popularSearches

            Catch ex As Exception
                returnException(mcModuleName, "GetMostPopularSearches", ex, "", "", gbDebug)
                Return Nothing
            End Try
        End Function

        Public Function savePayment(ByVal CartId As Integer, ByVal nUserId As Long, ByVal cProviderName As String, ByVal cProviderRef As String, ByVal cMethodName As String, ByVal oDetailXML As XmlElement, ByVal dExpire As Date, ByVal bUserSaved As Boolean, ByVal nAmountPaid As Double) As Integer
            Dim cSQL As String = ""
            Dim cRes As String = ""

            Dim nPaymentMethodKey As Long = -1
            oDetailXML.SetAttribute("AmountPaid", nAmountPaid)
            Try
                If bUserSaved Then
                    cSQL = "SELECT tblCartPaymentMethod.nPayMthdKey FROM tblCartPaymentMethod INNER JOIN tblAudit ON tblCartPaymentMethod.nAuditId = tblAudit.nAuditKey" & _
                    " WHERE (tblAudit.dExpireDate = " & Eonic.Tools.Database.SqlDate(dExpire) & ") and (tblCartPaymentMethod.cPayMthdAcctName = '" & cMethodName & "') and (cPayMthdProviderName = '" & cProviderName & "') and (tblCartPaymentMethod.nPayMthdUserId = " & nUserId & ")"
                    cRes = ExeProcessSqlScalar(cSQL)
                    If IsNumeric(cRes) Then
                        If CInt(cRes) > 0 Then
                            dExpire = Now
                        End If
                    End If
                End If

                'check if we allready have a payment method for this order if so we overwrite

                cSQL = "Select nPayMthdId from tblCartOrder WHERE nCartOrderKey = " & CartId
                cRes = ExeProcessSqlScalar(cSQL)

                If IsNumeric(cRes) Then
                    nPaymentMethodKey = CLng(cRes)
                End If

                'mask the credit card number
                Dim oCcNum As XmlElement = oDetailXML.SelectSingleNode("number")
                If Not oCcNum Is Nothing Then
                    oCcNum.InnerText = MaskString(oCcNum.InnerText, "*", False, 4)
                End If

                'mask CV2 digits
                Dim oCV2 As XmlElement = oDetailXML.SelectSingleNode("CV2")
                If Not oCV2 Is Nothing Then
                    oCV2.InnerText = ""
                End If

                Dim oXml As XmlDocument = New XmlDocument
                Dim oInstance As XmlElement = oXml.CreateElement("Instance")
                Dim oElmt As XmlElement = oXml.CreateElement("tblCartPaymentMethod")
                addNewTextNode("nPayMthdUserId", oElmt, nUserId)
                addNewTextNode("cPayMthdProviderName", oElmt, cProviderName)
                addNewTextNode("cPayMthdProviderRef", oElmt, cProviderRef)
                addNewTextNode("cPayMthdAcctName", oElmt, cMethodName)
                Dim oElmt2 As XmlElement = addNewTextNode("cPayMthdDetailXml", oElmt, )
                oElmt2.InnerXml = oDetailXML.OuterXml

                'addNewTextNode("dPayMthdExpire", oElmt, xmlDate(dExpire))

                Dim nPaymentId As Integer
                oInstance.AppendChild(oElmt)
                If Not nPaymentMethodKey > 0 Then
                    Dim nAudit As Integer = getAuditId(0, myWeb.mnUserId, "Payment", Now, dExpire, Now, Now)

                    addNewTextNode("nAuditId", oElmt, nAudit)
                    nPaymentId = setObjectInstance(dbHelper.objectTypes.CartPaymentMethod, oInstance, nPaymentMethodKey)
                Else
                    nPaymentId = setObjectInstance(dbHelper.objectTypes.CartPaymentMethod, oInstance, nPaymentMethodKey)
                    cSQL = "Select nAuditId from tblCartPaymentMethod where nPayMthdKey = " & nPaymentId
                    Dim nAuditId As Integer = ExeProcessSqlScalar(cSQL)
                    oInstance.RemoveAll()
                    oElmt = oXml.CreateElement("tblAudit")
                    addNewTextNode("nAuditKey", oElmt, nAuditId)
                    addNewTextNode("dExpireDate", oElmt, Eonic.Tools.Xml.XmlDate(dExpire))
                    addNewTextNode("dUpdateDate", oElmt, Eonic.Tools.Xml.XmlDate(Now))
                    addNewTextNode("nUpdateDirId", oElmt, myWeb.mnUserId)
                    addNewTextNode("nInsertDirId", oElmt, myWeb.mnUserId) '
                    addNewTextNode("nStatus", oElmt, 1)
                    oInstance.AppendChild(oElmt)
                    nPaymentId = setObjectInstance(dbHelper.objectTypes.CartPaymentMethod, oInstance, nPaymentMethodKey)
                End If

                CartPaymentMethod(CartId, nPaymentId)

                Return nPaymentId

            Catch ex As Exception
                returnException(mcModuleName, "savePayment", ex, "", "", gbDebug)
                Return 0
            End Try
        End Function

        Public Sub CartPaymentMethod(ByVal CartId As Integer, ByVal PaymentId As Integer)
            Try
                Dim cSQL As String = "UPDATE tblCartOrder SET nPayMthdId = " & PaymentId & " WHERE nCartOrderKey = " & CartId
                ExeProcessSql(cSQL)
            Catch ex As Exception
                returnException(mcModuleName, "CartPaymentMethod", ex, "", "", gbDebug)
            End Try
        End Sub

        Public Sub UpdateSellerNotes(ByVal CartId As Long, ByVal TransactionDetails As String)
            Dim sSql As String = ""
            Dim oDs As DataSet
            Dim oRow As DataRow
            Dim cProcessInfo As String
            Try

                'Update Seller Notes:
                sSql = "select * from tblCartOrder where nCartOrderKey = " & CartId
                oDs = getDataSetForUpdate(sSql, "Order", "Cart")
                For Each oRow In oDs.Tables("Order").Rows
                    oRow("cSellerNotes") = oRow("cSellerNotes") & vbLf & Today _
                    & " " & TimeOfDay & ": changed to: (Order Placed)" & vbLf _
                    & vbLf & TransactionDetails
                Next

                updateDataset(oDs, "Order")

            Catch ex As Exception
                returnException(mcModuleName, "UpdateSellerNotes", ex, "", cProcessInfo, gbDebug)
            End Try

        End Sub

        Public Sub ListReports(ByRef oContentsXML As XmlElement)
            PerfMon.Log("Cart", "ListReports")

            Dim listElement As XmlElement
            Dim reportElement As XmlElement
            Dim reportName As String = ""
            Dim dir As DirectoryInfo
            Dim files As FileInfo()
            Dim foldersToCheck As New List(Of String)
            Try

                foldersToCheck.Add("/")
                foldersToCheck.AddRange(myWeb.maCommonFolders)

                listElement = moPageXml.CreateElement("List")


                For Each folder As String In foldersToCheck

                    dir = New DirectoryInfo(myWeb.moCtx.Server.MapPath(folder) & "/xforms/Reports")
                    If dir.Exists Then
                        files = dir.GetFiles("*.xml")

                        For Each reportFile As FileInfo In files

                            reportName = reportFile.Name.Substring(0, reportFile.Name.LastIndexOf("."))

                            ' Check if the report has already been added (which suggest that local one has been found)
                            If listElement.SelectSingleNode("Report[@type='" & reportName & "']") Is Nothing Then

                                ' Add the report
                                reportElement = addNewTextNode("Report", listElement, reportName.Replace("-", " "))
                                reportElement.SetAttribute("type", reportName)

                            End If

                        Next
                    End If

                Next

                oContentsXML.AppendChild(listElement)

            Catch ex As Exception
                returnException(mcModuleName, "ListReports", ex, "", "", gbDebug)
            End Try
        End Sub



        Public Sub GetReport(ByRef oContentsXML As XmlElement, ByRef QueryXml As XmlElement)
            PerfMon.Log("Cart", "ListReports")
            Dim processInfo As String = ""
            Dim storedProcedure As String
            Dim ds As DataSet
            Dim reportName As String
            Dim reportElmt As XmlElement = oContentsXML.OwnerDocument.CreateElement("Report")

            Dim params As New Hashtable
            Dim paramValue As String = ""
            Dim paramName As String = ""

            Dim logDetail As String = ""
            Dim outputFormat As String = ""

            Try
                ' Gather variables
                storedProcedure = QueryXml.GetAttribute("storedProcedure")
                reportName = QueryXml.GetAttribute("name")
                outputFormat = QueryXml.GetAttribute("output")
                logDetail = QueryXml.GetAttribute("logActivityDetail")
                ' If nothing has been specific for the detail field, use the SP name
                If String.IsNullOrEmpty(logDetail) Then logDetail = storedProcedure

                ' Build the query parameters
                processInfo = "Building Parameters"
                For Each param As XmlElement In QueryXml.SelectNodes("param[@name!='']")
                    paramValue = param.GetAttribute("value")
                    paramName = param.GetAttribute("name")

                    ' Assume empty values are NULL values - SQl can handle this.
                    If Not String.IsNullOrEmpty(paramValue) And Not params.ContainsKey(paramName) Then
                        If param.GetAttribute("type") = "datetime" Then
                            paramValue = Replace(SqlDate(paramValue, True), "'", "")
                        End If
                        params.Add(paramName, paramValue)
                    Else
                        Select Case param.GetAttribute("type")
                            Case "number"
                                params.Add(paramName, 0)
                            Case "string"
                                params.Add(paramName, "")
                        End Select

                    End If
                Next

                ' Run the data
                processInfo = "Populated Parameter Count = " & params.Count.ToString
                Dim spWithBespokeCheck As String = getDBObjectNameWithBespokeCheck(storedProcedure)
                processInfo &= "; Running SP - " & spWithBespokeCheck
                Me.ConnectTimeout = 180
                ds = Me.GetDataSet(spWithBespokeCheck, "Item", "Report", , params, CommandType.StoredProcedure)


                ' Convert the dataset to Xml
                Dim reportXml As XmlDocument = New XmlDocument
                myWeb.moDbHelper.ReturnNullsEmpty(ds)
                reportXml.LoadXml(ds.GetXml)


                ' Process the data for type-specific columns
                processInfo &= "; Updating Column data"
                For Each column As DataColumn In ds.Tables("Item").Columns

                    ' Pick up whether this is XML - by name and not by type (which will have already converted)
                    If column.ColumnName.ToLower.Contains("xml") Then
                        For Each dataItem As XmlElement In reportXml.SelectNodes("Report/Item/" & column.ColumnName)
                            Tools.Xml.SetInnerXmlThenInnerText(dataItem, dataItem.InnerText)
                        Next
                    End If

                    ' Add metadata and format data based on type.
                    Select Case column.DataType.ToString
                        Case "System.DateTime"
                            Dim dateNodes As XmlNodeList = reportXml.SelectNodes("Report/Item/" & column.ColumnName)

                            If dateNodes.Count > 0 Then
                                ' Add a datatype info to the first item
                                Dim firstDate As XmlElement = dateNodes.Item(0)
                                firstDate.SetAttribute("datatype", "date")

                                ' Convert other items to xml date format
                                For Each dataItem As XmlElement In dateNodes
                                    dataItem.InnerText = Eonic.Tools.Xml.XmlDate(dataItem.InnerText.ToString, True)
                                Next
                            End If
                    End Select
                Next


                ' Organise data for contents node
                reportXml.PreserveWhitespace = False
                reportElmt.SetAttribute("name", reportName)
                reportElmt.SetAttribute("format", outputFormat)

                For Each param As XmlElement In QueryXml.SelectNodes("param[@name!='']")
                    reportElmt.SetAttribute("p-" & param.GetAttribute("name"), param.GetAttribute("value"))
                Next

                reportElmt.InnerXml = reportXml.DocumentElement.InnerXml()
                reportElmt.AppendChild(QueryXml.CloneNode(True))
                oContentsXML.AppendChild(reportElmt)


                ' Update the filename
                If Not String.IsNullOrEmpty(outputFormat) AndAlso outputFormat <> "rawxml" Then
                    processInfo &= "; Updating filename"
                    Dim filename As New List(Of String)
                    If Not String.IsNullOrEmpty(QueryXml.GetAttribute("filePrefix")) Then filename.Add(QueryXml.GetAttribute("filePrefix"))

                    If QueryXml.GetAttribute("fileExcludeReportName").ToLower <> "true" Then
                        filename.Add(reportName)
                    End If

                    Select Case QueryXml.GetAttribute("fileUID").ToLower
                        Case "log"
                            ' This is a sequential number based on the download activity log for this report.
                            Dim logCount As Integer = GetDataValue("SELECT COUNT(*) FROM dbo.tblActivityLog WHERE cActivityDetail=" & SqlString(logDetail) & " AND nActivityType = " & ActivityType.ReportDownloaded, , , 0)
                            logCount += 1
                            filename.Add(logCount.ToString("00000"))
                        Case "random", "guid"
                            filename.Add(Guid.NewGuid.ToString())
                        Case Else ' "timestamp"
                            filename.Add(Now.ToString("yyyyMMddhhmmss"))
                    End Select

                    myWeb.mcContentDisposition = "attachment;filename=" & String.Join("-", filename.ToArray()) & "." & outputFormat
                End If


                ' Finally, log activity if it has been requested.
                If QueryXml.GetAttribute("logActivity") = "true" Then
                    processInfo &= "; Logging activity"


                    Me.logActivity(ActivityType.ReportDownloaded, myWeb.mnUserId, 0, 0, logDetail)
                End If

            Catch ex As Exception
                returnException(mcModuleName, "GetReport", ex, "", processInfo, gbDebug)
            End Try
        End Sub

        Public Overloads Function checkTableColumnExists(ByVal tableName As String, ByVal columnName As String) As Boolean

            Dim columnExists As Boolean = False
            Dim colState As String = ""
            Try
                'this saves the result into the application object, this result will never change for an active website.

                ' Check for empty strings.
                If Not (String.IsNullOrEmpty(tableName) Or String.IsNullOrEmpty(columnName)) Then
                    If myWeb Is Nothing Then
                        'case is web is not instatiated and we have not context, such as when running from a webservice.
                        columnExists = MyBase.checkTableColumnExists(tableName, columnName)
                    Else
                        Dim oApp As System.Web.HttpApplicationState = myWeb.moCtx.Application
                        colState = oApp(tableName & "-" & columnName)
                        Select Case colState
                            Case "0"
                                columnExists = False
                            Case "1"
                                columnExists = True
                            Case ""
                                If MyBase.checkTableColumnExists(tableName, columnName) Then
                                    oApp(tableName & "-" & columnName) = 1
                                    columnExists = True
                                Else
                                    oApp(tableName & "-" & columnName) = 0
                                    columnExists = False
                                End If
                        End Select
                    End If

                End If

                Return columnExists

            Catch ex As Exception
                Return False
            End Try
        End Function

#Region "Deprecated Functions"
        Public Function doesTableExist(ByRef sTableName As String) As Boolean
            Try

                Return Me.checkDBObjectExists(sTableName, Tools.Database.objectTypes.Table)

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "doesTableExist", ex, "Checking: " & sTableName))
                Return False
            End Try

        End Function
        Public Function TableExists(ByVal cTableName As String) As Boolean
            Dim cProcessInfo As String = "TableExists: " & cTableName
            Try
                Return Me.checkDBObjectExists(cTableName, Tools.Database.objectTypes.Table)
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "doesTableExist", ex, cProcessInfo))
                Return False
            End Try
        End Function
#End Region


    End Class

    Public Class dbImport

        Inherits Eonic.Tools.Database

#Region "New Error Handling"
        Public Shadows Event OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)

        Private Sub _OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs) Handles MyBase.OnError
            RaiseEvent OnError(sender, e)
        End Sub

#End Region

        Public oConnString As String
        Public mnUserId As Long
        Public moCtx As System.Web.HttpContext
        Dim mcModuleName As String = "dbImport"

        Public Sub New(ByVal cConnectionString As String, ByVal nUserId As Long, Optional ByVal oCtx As System.Web.HttpContext = Nothing)
            'MyBase.New(cConnectionString)
            Try
                oConnString = cConnectionString
                mnUserId = nUserId
                moCtx = oCtx


            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""))
            End Try
        End Sub


        Public Class ImportStateObj
            Public oInstance As XmlElement
            Public LogId As Long
            Public FeedRef As String
            Public CompleteCount As Long
            Public totalInstances As Long
            Public bSkipExisting As Boolean
            Public bResetLocations As Boolean
            Public bOrphan As Boolean
            Public bDeleteNonEntries As Boolean
            Public cDeleteTempTableName As String
            Public modbhelper As dbHelper
        End Class


        Public Sub ImportSingleObject(ImportStateObj As Object)
            Dim cTableName As String = ""
            Dim cTableKey As String = ""
            Dim cTableFRef As String = ""
            Try


                Dim modbhelper As dbHelper = New dbHelper(oConnString, mnUserId, moCtx)

                moDbHelper.ResetConnection(oConnString)
                modbhelper.updateActivity(ImportStateObj.LogId, ImportStateObj.FeedRef & " Importing " & ImportStateObj.totalInstances & " Objects, " & ImportStateObj.CompleteCount & " Processed")

                'lets get the object type from the table name.
                cTableName = ImportStateObj.oInstance.FirstChild.Name

                'return the object type from the table name
                Dim oTblName As dbHelper.TableNames
                For Each oTblName In [Enum].GetValues(GetType(dbHelper.objectTypes))
                    If oTblName.ToString = cTableName Then Exit For
                Next
                Dim oObjType As New dbHelper.objectTypes

                '^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                'Disabled on 16/09/2008 the following, due to the incompatible assignment of Value to the Object Types 
                'oTblName = oObjType
                oObjType = oTblName
                '^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

                ' The purpose of this is to try to reduce the amount of table name/key/fref calls
                ' so to optimise this for bulk use.
                'If cTableName <> cPreviousTableName Then
                cTableKey = moDbHelper.getKey(oObjType)
                cTableFRef = moDbHelper.getFRef(oObjType)
                'End If

                Dim fRefNode As XmlElement = ImportStateObj.oInstance.SelectSingleNode(cTableName & "/" & cTableFRef)
                Dim fRef As String = fRefNode.InnerText

                'We absolutly do not do anything if no fRef
                If Not fRef = "" Then
                    Dim nId As Long
                    'lets get an id if we are updating a record with a foriegn Ref

                    nId = moDbHelper.getObjectByRef(cTableName, cTableKey, cTableFRef, oObjType, fRef)

                    'nId = myWeb.moDbHelper.getObjectByRef(cTableName, cTableKey, cTableFRef, oObjType, fRef)

                    'if we want to replace the fRef
                    If Not fRefNode.GetAttribute("replaceWith") = "" Then
                        fRefNode.InnerText = fRefNode.GetAttribute("replaceWith")
                    End If

                    ImportStateObj.oInstance.SelectSingleNode(cTableName & "/" & cTableFRef)

                    If Not (ImportStateObj.bSkipExisting And nId <> 0) Then
                        moDbHelper.ResetConnection(oConnString)
                        nId = moDbHelper.setObjectInstance(oObjType, ImportStateObj.oInstance, nId)
                    End If

                    moDbHelper.processInstanceExtras(nId, ImportStateObj.oInstance, ImportStateObj.bResetLocations, ImportStateObj.bOrphan)

                    If ImportStateObj.bDeleteNonEntries Then

                        Dim cSQL As String = "INSERT INTO dbo." & ImportStateObj.cDeleteTempTableName & " (cImportID , cTableName) VALUES ('" & SqlFmt(fRef) & "','" & SqlFmt(cTableName) & "')"
                        moDbHelper.ResetConnection(oConnString)
                        moDbHelper.ExeProcessSql(cSQL)

                    End If



                End If
                moDbHelper = Nothing

            Catch ex As Exception

                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "ImportSingleObject", ex, ""))
            End Try
        End Sub
    End Class

    End Class
