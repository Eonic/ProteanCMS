' ================================================================================================
'   Eonic.Syndication
'   Desc:       Retrieves site content and syndicates it to distributors
'   Author:     Ali Granger
'   Updated:    17-Aug-09
'   Docs:       http://intra.office.eonic.co.uk/intranet/Documents/Development/Eonic.Syndication.docx  
'
'   To create a new distrubtor, you need to add the following (search Technorati for the various update points)
'   - Add name to Shared Constant in Syndication.distributorTypesList 
'   - Add the method into the creation list in Private Sub Syndication.CreateDistributors
'   - Create the distributor as a class under Syndication.Distributor
'       - The class inherits Syndication.Distributor
'       - It should override New (where specific config settings can be defined) and RunSyndicate
'   Note: At the moment the only distributors are ping servers, which use the same XMLRPC interface
'       and to this end Weblogs, Technorati and PingOMatic inherit Syndication.Distributor.GenericXmlRpc
'
' ================================================================================================


Option Strict Off
Option Explicit On

Imports System
Imports System.Net
Imports System.Xml
Imports System.IO
Imports Eonic.Tools
Imports Eonic.Tools.Database
Imports Eonic.Tools.Xml

''' <summary>
'''    Eonic.Syndication retrieves site content and syndicates it to distributors
''' </summary>
''' <remarks></remarks>
Public Class Syndication

#Region "Declarations"

    ' Eonic objects
    Private _myWeb As Eonic.Web
    Private _moduleName As String = "Eonic.Syndication"

    ' Core collections
    Private _distributorTypes As String()
    Private _contentTypes As String()
    Private _distributors As Distributor()

    ' Core variables
    Private _distributorTypesList As String = ""
    Private _structureNode As XmlElement = Nothing
    Private _sourcePage As Long
    Private _iterate As Boolean
    Private _extendedConfig As XmlElement

    ' Flag variables
    Private _hasDistributors As Boolean = False

    ' State variables
    Private _activityLog As Integer = 0
    Private _diagnostics As String = ""
    Private _isCompleted As Boolean = False
    Private _hasFailures As Boolean = False
    Private _totalCompleted As Integer = 0
    Private _contentSyndicated As Integer = 0
    Private _lastRun As Date
    Private _hasBeenRunBefore As Boolean = False

#End Region

#Region "Enums and Constants"

    ' Pseudo-constant array
    ' Use this instead of Enum, as the process of parsing values from Enum is inaccurate and processor heavy.
    Shared distributorTypesList As String() = {"Weblogs", "Technorati", "GenericXmlRpc", "PingOMatic"}

#End Region

#Region "Events"

    Public Shadows Event OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)

    Private Sub _OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)
        RaiseEvent OnError(sender, e)
    End Sub

#End Region

#Region "Constructor"

    Public Sub New( _
        ByRef aWeb As Eonic.Web, _
        ByVal distributorTypes As String, _
        ByVal contentTypes As String, _
        ByVal pageId As Long, _
        ByVal iterate As Boolean)

        Try
            _myWeb = aWeb
            _distributorTypesList = distributorTypes
            Me.DistributorTypeCollection = GetCSVArray(distributorTypes)
            Me.ContentTypes = GetCSVArray(contentTypes)
            Me.SourcePage = pageId
            Me.Iterate = iterate
        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(_moduleName, "New(Web,String,String,Long,Boolean)", ex, ""))
        End Try


    End Sub

    Public Sub New( _
        ByRef aWeb As Eonic.Web, _
        ByVal distributorTypes As String, _
        ByVal contentTypes As String, _
        ByVal pageId As Long, _
        ByVal iterate As Boolean, _
        ByVal extendedConfig As XmlElement)
        Try
            _myWeb = aWeb
            _distributorTypesList = distributorTypes
            Me.DistributorTypeCollection = GetCSVArray(distributorTypes)
            Me.ContentTypes = GetCSVArray(contentTypes)
            Me.SourcePage = pageId
            Me.Iterate = iterate
            Me.ExtendedConfig = extendedConfig
        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(_moduleName, "New(Web,String,String,Long,Boolean,XmlElement)", ex, ""))
        End Try
    End Sub

#End Region

#Region "Private Properties"

    Private Property ContentsNode() As XmlElement
        Get
            Try
                Return _myWeb.moPageXml.DocumentElement.SelectSingleNode("Contents")
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(_moduleName, "ContentsNode(Get)", ex, ""))
                Return Nothing
            End Try
        End Get
        Set(ByVal value As XmlElement)
            Try
                If value.Name = "Contents" Then
                    _myWeb.moPageXml.DocumentElement.SelectSingleNode("Contents").InnerXml = value.InnerXml
                End If
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(_moduleName, "ContentsNode(Set)", ex, ""))
            End Try
        End Set
    End Property

    Private ReadOnly Property IsStructureLoaded() As Boolean
        Get
            Return Not (_structureNode Is Nothing)
        End Get
    End Property

#End Region

#Region "Public Properties"

    Public Property ContentTypes() As String()
        Get
            Return Me._contentTypes
        End Get
        Set(ByVal value As String())
            Me._contentTypes = value
        End Set
    End Property

    Public Property DistributorTypeCollection() As String()
        Get
            Return Me._distributorTypes
        End Get
        Set(ByVal value As String())

            Try
                Dim tempDistributorTypes As New ArrayList()
                Dim hasValidDistributors As Boolean = False
                For Each distributorType As String In value

                    ' For each value we check whether or not it's valid
                    If IsDistributor(distributorType) Then
                        tempDistributorTypes.Add(distributorType)
                        hasValidDistributors = True
                    End If

                Next

                ' Set the private class variables
                _hasDistributors = hasValidDistributors
                _distributorTypes = CType(tempDistributorTypes.ToArray(GetType(String)), String())

                ' Now try to set the distributors
                CreateDistributors()

            Catch ex As Exception

            End Try

        End Set
    End Property

    Public Property ExtendedConfig() As XmlElement
        Get
            Return _extendedConfig
        End Get
        Set(ByVal value As XmlElement)
            _extendedConfig = value
        End Set
    End Property

    Public Property Iterate() As Boolean
        Get
            Return _iterate
        End Get
        Set(ByVal value As Boolean)
            _iterate = value
        End Set
    End Property

    Public Property SourcePage() As Long
        Get
            Return _sourcePage
        End Get
        Set(ByVal value As Long)
            _sourcePage = value
        End Set
    End Property

    Public ReadOnly Property ActivityKey() As Integer
        Get
            Return _activityLog
        End Get
    End Property

    Public ReadOnly Property Diagnostics() As String
        Get
            Return _diagnostics
        End Get
    End Property

    Public ReadOnly Property ContentCount() As Integer
        Get
            Return Me._contentSyndicated
        End Get
    End Property

    Private ReadOnly Property HasBeenRunBefore() As Boolean
        Get
            Dim sqlQuery As String = "SELECT MAX(dDateTime) As LastRun FROM tblActivityLog WHERE nActivityType IN (200,201,202,203,204) AND nActivityKey <> " & Me._activityLog & " "

            If Me.HasSourcePage() Then
                sqlQuery &= " AND nStructId=" & Me.SourcePage
            End If

            Dim objDate As Object = _myWeb.moDbHelper.GetDataValue(sqlQuery, , , Nothing)

            If Not (objDate Is Nothing) AndAlso IsDate(objDate) Then
                Me._lastRun = objDate
                Me._hasBeenRunBefore = True
            Else
                Me._hasBeenRunBefore = False
            End If

            Return Me._hasBeenRunBefore
        End Get
    End Property

    Public ReadOnly Property HasDistributors() As Boolean
        Get
            Return _hasDistributors
        End Get
    End Property

    Public ReadOnly Property HasSourcePage() As Boolean
        Get
            Return IsNumeric(Me.SourcePage()) AndAlso Me.SourcePage() > 0
        End Get
    End Property

    Public ReadOnly Property IsCompleted() As Boolean
        Get
            Return _isCompleted
        End Get
    End Property

    Public ReadOnly Property IsContentsNodePopulated() As Boolean
        Get
            Return ContentsNode().SelectNodes("Content").Count > 0
        End Get
    End Property

    Public ReadOnly Property IsReady() As Boolean
        Get
            Return Me.HasDistributors() And Me.ContentTypes.GetLength(0) > 0
        End Get
    End Property


#End Region

#Region "Private Members"

    ''' <summary>
    ''' Runs through the content looking for specific data types, retrieving metadata (e.g. BlogControl) for them
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub AddContentMetadata()
        Try
            If Me.IsContentsNodePopulated() Then

                ' Because each content type has specific circumstances, rather than run through each content node,
                ' we'll specifically search for content types.  Some content types will only require one call for all content

                ' ==============================================
                '   METADATA ADD:  BLOG ARTICLES
                ' ==============================================
                Dim blogArticles As XmlNodeList = ContentsNode().SelectNodes("Content[@type='BlogArticle']")
                If blogArticles.Count > 0 Then

                    Dim blogArticlePageIds As New Hashtable

                    '  Go get all the pages to search for 
                    For Each blogArticle As XmlElement In blogArticles
                        If Not (blogArticlePageIds.ContainsKey(blogArticle.GetAttribute("parId"))) Then
                            blogArticlePageIds.Add(blogArticle.GetAttribute("parId"), "")
                        End If
                    Next

                    ' Now get the BlogSettings
                    Dim blogControls As XmlElement = ContentsNode().OwnerDocument.CreateElement("Contents")

                    Dim sqlCriteria As String = ""
                    sqlCriteria &= " cContentSchemaName ='BlogSettings'"
                    sqlCriteria &= " AND CL.nStructId IN (" & Dictionary.hashtableToCSV(blogArticlePageIds, Dimension.Key) & ")"
                    _myWeb.GetPageContentFromSelect(sqlCriteria, , , True, , , blogControls)

                    Dim blogControl As XmlElement = Nothing
                    If blogControls.HasChildNodes() Then
                        ' Go throgh all the blogArticles and add the metadata
                        For Each blogArticle As XmlElement In blogArticles
                            If NodeState(blogControls, "Content[@parId = " & blogArticle.GetAttribute("parId") & "]", , , , blogControl) <> XmlNodeState.NotInstantiated Then
                                blogArticle.AppendChild(blogArticle.OwnerDocument.ImportNode(blogControl, True))
                            End If
                        Next
                    End If


                End If

            End If
        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(_moduleName, "AddContentMetadata", ex, ""))
        End Try
    End Sub

    ''' <summary>
    ''' Check the submitted distributors against actual distributors, and create the distributors if valid.
    ''' This also loads extended distributor settings if provided/required.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub CreateDistributors()
        Try
            Dim newDistributor As Distributor = Nothing

            ' We use ArrayLists because they can be easily dynamically extended using the Add method.
            Dim tempDistributors As New ArrayList()
            For Each distributorType As String In DistributorTypeCollection

                newDistributor = Nothing

                ' Create the distributor specific object
                Select Case distributorType
                    Case "Weblogs"
                        newDistributor = New Distributor.Weblogs(_myWeb)
                    Case "Technorati"
                        newDistributor = New Distributor.Technorati(_myWeb)
                    Case "GenericXmlRpc"
                        newDistributor = New Distributor.GenericXmlRpc(_myWeb)
                    Case "PingOMatic"
                        newDistributor = New Distributor.PingOMatic(_myWeb)
                End Select

                If Not (newDistributor Is Nothing) Then
                    ' Set common variables
                    AddHandler newDistributor.OnError, AddressOf _OnError

                    If newDistributor.UsesExtendedConfig Then
                        ' Try to locate the config
                        newDistributor.LoadConfig(Me.ExtendedConfig)
                    End If

                    ' Add the distributor to the array
                    tempDistributors.Add(newDistributor)
                End If


            Next

            ' Convert the ArrayList back to an array
            _distributors = CType(tempDistributors.ToArray(GetType(Distributor)), Distributor())

        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(_moduleName, "CreateDistributors", ex, ""))
        End Try
    End Sub

    ''' <summary>
    ''' Converts a CSV list into a String Array
    ''' </summary>
    ''' <param name="csvList">The list to convert</param>
    ''' <param name="separator">The seperator in the list</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function GetCSVArray(ByVal csvList As String, Optional ByVal separator As String = ",") As String()
        Try
            If String.IsNullOrEmpty(csvList) Then
                Return Nothing
            Else
                Return csvList.Split(separator)
            End If
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Handles Syndication logging.  Creates a new log if needed and sets the log key, or updates the current log.
    ''' Note that logging is ties to SourcePage if it is specified.  This should allow multiple Syndications to be run against a website if needed.
    ''' </summary>
    ''' <param name="activityType">The activity type to log</param>
    ''' <param name="logDetail">Optional. Additional detail to log</param>
    ''' <remarks></remarks>
    Public Sub Log(ByVal activityType As Eonic.Web.dbHelper.ActivityType, Optional ByVal logDetail As String = "")
        Dim sqlQuery As String = ""

        Try
            ' Test if this has been logged
            If IsNumeric(Me.ActivityKey) AndAlso Me.ActivityKey > 0 Then
                ' Alert Item has been logged, therefore update the record

                sqlQuery = "UPDATE tblActivityLog "
                sqlQuery &= "SET nActivityType = " & activityType & " "
                If Not (String.IsNullOrEmpty(logDetail)) Then sqlQuery &= "   ,cActivityDetail = '" & SqlFmt(Left(logDetail, 800)) & "' "
                sqlQuery &= "WHERE nActivityKey = " & Me.ActivityKey
                _myWeb.moDbHelper.ExeProcessSql(sqlQuery)
            Else
                ' Alert Item has been not been logged, therefore insert the record

                sqlQuery = "INSERT INTO tblActivityLog ( nUserDirId, nStructId, nArtId, nOtherId, dDateTime, nActivityType, cActivityDetail, cSessionId) VALUES ("
                sqlQuery &= "1,"
                sqlQuery &= Me.SourcePage & ","
                sqlQuery &= "0,"
                sqlQuery &= "0,"
                sqlQuery &= Eonic.Tools.Database.SqlDate(Now, True) & ","
                sqlQuery &= activityType & ","
                sqlQuery &= "'" & SqlFmt(Left(logDetail, 800)) & "',"
                sqlQuery &= "'')"

                _activityLog = _myWeb.moDbHelper.GetIdInsertSql(sqlQuery)
            End If

        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(_moduleName, "Log", ex, sqlQuery))
        End Try
    End Sub

    ''' <summary>
    ''' Runs through anything in the contents node and adds it to the activity log so that it doesn't get picked up again.
    ''' Note that logging is ties to SourcePage if it is specified.  This should allow multiple Syndications to be run against a website if needed.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub LogContent()
        Dim sqlQuery As String = ""
        Try
            If IsContentsNodePopulated() Then
                ' Go through the content and flag it up on the database.
                For Each content As XmlElement In ContentsNode().SelectNodes("Content")
                    _contentSyndicated += 1
                    sqlQuery = "INSERT INTO tblActivityLog ( nUserDirId, nStructId, nArtId, nOtherId, dDateTime, nActivityType, cActivityDetail, cSessionId) VALUES ("
                    sqlQuery &= "1,"
                    sqlQuery &= "0,"
                    sqlQuery &= content.GetAttribute("id") & ","
                    sqlQuery &= Me._activityLog & ","
                    sqlQuery &= Eonic.Tools.Database.SqlDate(Now, True) & ","
                    sqlQuery &= Web.dbHelper.ActivityType.ContentSyndicated & ","
                    sqlQuery &= "'',"
                    sqlQuery &= "'')"

                    _myWeb.moDbHelper.ExeProcessSql(sqlQuery)
                Next
            End If
        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(_moduleName, "LogContent", ex, sqlQuery))
        End Try
    End Sub

    ''' <summary>
    ''' Retrieve website content and add to the contentnode.
    ''' Criteria for retrieval are content types, optional page id and iteration.
    ''' There are also checks that content has not been synidcated already.
    ''' </summary>
    ''' <param name="contentTypeSqlList">Sql formatted, comma-separated list of content types to retireve</param>
    ''' <param name="pageId">Optional, page to retrieve content from</param>
    ''' <remarks></remarks>
    Private Sub PopulateContent(ByVal contentTypeSqlList As String, Optional ByVal pageId As Long = 0)
        Try

            Dim sqlCriteria As String = ""
            Dim sqlAdditionalTables As String = ""

            sqlCriteria &= " (cContentSchemaName IN (" & contentTypeSqlList & "))"

            ' Page Id
            If pageId > 0 Then
                sqlCriteria &= " AND CL.nStructId = " & pageId
            End If

            ' Reset myWeb content
            Dim contents As XmlElement = Nothing
            NodeState(_myWeb.moPageXml.DocumentElement, "Contents", " ", , XmlNodeState.HasContents, contents)

            ' Add some criteria to stop retrieving items that have been syndicated.
            sqlAdditionalTables = " LEFT JOIN tblActivityLog activity ON c.nContentKey = activity.nArtId And activity.nActivityType = 205 "
            If Me.HasSourcePage() Then sqlAdditionalTables &= " AND activity.nStructId = " & Me.SourcePage & " "
            sqlCriteria &= " AND activity.nActivityKey Is NULL "

            ' Only get content since the last time run.
            If Me.HasBeenRunBefore() Then
                sqlCriteria &= " AND a.dUpdateDate >= " & Eonic.Tools.Database.SqlDate(Me._lastRun, True) & " "
            End If

            ' Go get the content
            _myWeb.GetPageContentFromSelect(sqlCriteria, , , , , , , sqlAdditionalTables)


            ' Add the contents to content node
            ContentsNode().InnerXml &= Trim(contents.InnerXml)


            ' Address ChildItems
            If Iterate() And IsStructureLoaded() And pageId > 0 Then

                For Each childPage As XmlElement In _structureNode.SelectNodes("//MenuItem[@id=" & pageId & "]/MenuItem")

                    PopulateContent(contentTypeSqlList, childPage.GetAttribute("id"))

                Next

            End If

        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(_moduleName, "Populatecontent", ex, ""))
        End Try
    End Sub

    ''' <summary>
    ''' Runs through the contents found and removes any duplicate content nodes.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub RemoveDuplicateContent()
        Try
            If Me.IsContentsNodePopulated() Then
                Dim contentIds As New Hashtable
                For Each contentNode As XmlElement In ContentsNode().SelectNodes("Content")
                    If contentIds.ContainsKey(contentNode.GetAttribute("id")) Then
                        contentNode.ParentNode.RemoveChild(contentNode)
                    Else
                        contentIds.Add(contentNode.GetAttribute("id"), "")
                    End If
                Next
            End If
        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(_moduleName, "RemoveDuplicateContent", ex, ""))
        End Try
    End Sub

    ''' <summary>
    ''' Clears out the page's Contents node.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub ResetContentsNode()
        Try
            Dim pageContentsNode As XmlElement = Nothing
            If NodeState(_myWeb.moPageXml.DocumentElement, "Contents", , , , pageContentsNode) Then
                pageContentsNode.InnerXml = ""
            End If

        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(_moduleName, "ResetContentsNode", ex, ""))
        End Try
    End Sub

#End Region

#Region "Public Members"

    ''' <summary>
    ''' Gathers the content togethers and tidies it up where necessary.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Populate()
        Try

            If IsReady() Then

                ' Build the page XML
                _myWeb.InitializeVariables()
                _myWeb.Open()
                _myWeb.BuildPageXML()

                ' Initialise the Contents Node
                ResetContentsNode()

                ' Get the ContentTypes as a list
                Dim contentTypeSqlList As String = ""
                For Each contentType As String In ContentTypes

                    If Not (String.IsNullOrEmpty(contentTypeSqlList)) Then contentTypeSqlList &= ","
                    contentTypeSqlList &= SqlString(contentType)

                Next

                ' If pages are being referenced, then we need to get the structure
                If HasSourcePage() Then Me._structureNode = _myWeb.GetStructureXML()

                ' Go and get the content
                PopulateContent(contentTypeSqlList, IIf(HasSourcePage(), SourcePage(), 0))

                ' Clean up duplicates
                RemoveDuplicateContent()

                ' Add Content Metadata
                AddContentMetadata()

                ' Mark the content as syndicated
                LogContent()

            End If

        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(_moduleName, "Populate", ex, ""))
        End Try
    End Sub

    ''' <summary>
    ''' Runs the content synidcation
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Syndicate()
        Try

            '  Kick off the logging.
            Log(Web.dbHelper.ActivityType.SyndicationStarted, _distributorTypesList)

            ' Get the content
            Populate()

            ' Syndicate the content
            If IsContentsNodePopulated() Then
                ' Log: begun
                Log(Web.dbHelper.ActivityType.SyndicationInProgress)

                ' Run through each distributor
                For Each distributorInstance As Distributor In _distributors

                    distributorInstance.ContentsNode() = ContentsNode()
                    distributorInstance.Syndicate()

                    ' Update the completion totals.
                    If distributorInstance.IsCompleted Then
                        _totalCompleted += 1
                    Else
                        _hasFailures = True
                    End If

                    _diagnostics &= distributorInstance.Diagnostics & Microsoft.VisualBasic.vbCrLf

                Next

                ' At the end indicate partial or full completion or total failure
                ' Add the diagnostics to the log
                If _hasFailures And _totalCompleted > 0 Then
                    Log(Web.dbHelper.ActivityType.SyndicationPartialSuccess, _diagnostics)
                    _diagnostics = "Partial Completion (Failure detail):" & Microsoft.VisualBasic.vbCrLf & _diagnostics
                ElseIf _hasFailures Then
                    Log(Web.dbHelper.ActivityType.SyndicationFailed, _diagnostics)
                    _diagnostics = "Syndication Failed (Detail):" & Microsoft.VisualBasic.vbCrLf & _diagnostics
                Else
                    Log(Web.dbHelper.ActivityType.SyndicationCompleted, _diagnostics)
                    Me._isCompleted = True

                End If

            Else
                Me._isCompleted = True
            End If

        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(_moduleName, "Syndicate", ex, ""))
        End Try
    End Sub

#End Region

#Region "Shared Members"

    ''' <summary>
    ''' Checks whether a submitted string is a valid Distributor
    ''' </summary>
    ''' <param name="possibleDistributor">The distributor name to check.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Shared Function IsDistributor(ByVal possibleDistributor As String) As Boolean
        Try
            Return Array.IndexOf(distributorTypesList, possibleDistributor) >= distributorTypesList.GetLowerBound(0)
        Catch ex As Exception
            Return False
        End Try
    End Function

#End Region


    ''' <summary>
    '''    Distributor is an abstract (base) class for all distributors.
    '''    It contains common properties.
    ''' </summary>
    ''' <remarks></remarks>
    Public MustInherit Class Distributor

#Region "Declarations"

        Private _myWeb As Eonic.Web
        Private _moduleName As String = "Eonic.Syndication.Distributor"

        ' Core variables
        Private _contentsNode As XmlElement = Nothing
        Private _transformedData As String = ""
        Private _config As Config = Nothing
        Private _diagnostics As String = ""


        ' Flags
        Private _usesXslTransform As Boolean = False
        Private _transformExists As Boolean = False
        Private _transformCompleted As Boolean = False
        Private _usesExtendedConfig As Boolean = False
        Private _completed As Boolean = False

#End Region

#Region "Events"
        Public Shadows Event OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)

        Private Sub _OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)
            _diagnostics &= Microsoft.VisualBasic.vbCrLf & "Error:" & e.ToString()
            RaiseEvent OnError(sender, e)
        End Sub
#End Region

#Region "Constructor"
        Public Sub New(ByRef aWeb As Eonic.Web)
            Try
                _myWeb = aWeb
                _moduleName &= "." & Me.Name()
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(_moduleName, "New", ex, ""))
            End Try

        End Sub
#End Region

#Region "Private Properties"

        ''' <summary>
        '''  The name of the distributor
        ''' </summary>
        ''' <returns>String</returns>
        ''' <remarks></remarks>
        Private ReadOnly Property Name() As String
            Get
                Return Me.GetType().Name
            End Get
        End Property

        ''' <summary>
        ''' If xsl is being used, this stores the transformation output
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Property TransformedData() As String
            Get
                Return _transformedData
            End Get
            Set(ByVal value As String)
                _transformedData = value
            End Set
        End Property

        Private Property TransformCompleted() As Boolean
            Get
                Return _transformCompleted
            End Get
            Set(ByVal value As Boolean)
                _transformCompleted = value
            End Set
        End Property

        Private ReadOnly Property UsesXslTransform() As Boolean
            Get
                Return Me._usesXslTransform
            End Get
        End Property

#End Region

#Region "Public Properties"

        Public Property ContentsNode() As XmlElement
            Get
                Return _contentsNode
            End Get
            Set(ByVal value As XmlElement)
                _contentsNode = value
            End Set
        End Property

        Public ReadOnly Property Diagnostics() As String
            Get
                Return Name & ":" & _diagnostics
            End Get
        End Property

        Public ReadOnly Property ExtendedConfig() As Config
            Get
                If _config Is Nothing Or Not (Me.UsesExtendedConfig) Then
                    Return Nothing
                Else
                    Return _config
                End If
            End Get
        End Property

        Public ReadOnly Property IsCompleted() As Boolean
            Get
                Return _completed
            End Get
        End Property

        Public ReadOnly Property UsesExtendedConfig() As Boolean
            Get
                Return _usesExtendedConfig
            End Get

        End Property

#End Region

#Region "Private Members"
        Private ReadOnly Property HasContent() As Boolean
            Get
                Return Not (ContentsNode() Is Nothing OrElse ContentsNode().SelectNodes("Content").Count = 0)
            End Get
        End Property

        Private Property TransformExists() As Boolean
            Get
                Return _transformExists
            End Get
            Set(ByVal value As Boolean)
                _transformExists = value
            End Set
        End Property
#End Region

#Region "Public Members"

        Protected MustOverride Sub RunSyndicate()

        ''' <summary>
        ''' If the Distributor syndication is acheived partially through transforming the Contents by XSL
        ''' then this function will look for a local and then common distributor-specific xsl
        ''' </summary>
        ''' <remarks></remarks>
        Public Overridable Sub Transform()
            Try

                If Me.UsesXslTransform Then
                    Dim testTransformExists As Boolean = True

                    Dim xslPath As String = "/xsl/Syndication/" & Me.Name & ".xsl"

                    ' Does a local xsl exist?
                    If Not (File.Exists(_myWeb.goServer.MapPath(xslPath))) Then
                        xslPath = "/ewcommon" & xslPath
                        If Not (File.Exists(_myWeb.goServer.MapPath(xslPath))) Then
                            testTransformExists = False
                        End If
                    End If

                    Me.TransformExists = testTransformExists

                    ' If it doesn't exist then we can't proceed.
                    If Me.TransformExists Then

                        Dim xslTransform As New Eonic.XmlHelper.Transform()
                        Dim output As IO.TextWriter = New IO.StringWriter

                        xslTransform.XSLFile = _myWeb.goServer.MapPath(xslPath)
                        xslTransform.Compiled = False
                        xslTransform.Process(ContentsNode.OwnerDocument, output)
                        If xslTransform.HasError Then Throw New Exception("There was an error transforming the distributor")

                        'Run transformation
                        TransformedData = output.ToString()
                        output.Close()
                        output = Nothing

                        TransformCompleted = True
                    Else
                        ' Flag up that it hasn't run
                        TransformCompleted = False
                    End If
                End If

            Catch ex As Exception
                TransformCompleted = False
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(_moduleName, "Transform", ex, ""))
            End Try
        End Sub

        ''' <summary>
        '''  Runs the Syndication
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Syndicate()
            Try
                _completed = False
                If Not (Me.UsesExtendedConfig) OrElse (Me.UsesExtendedConfig AndAlso Me.ExtendedConfig.IsLoaded()) Then
                    If Me.HasContent() Then
                        Transform()
                        RunSyndicate()
                    Else
                    End If
                Else
                End If
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(_moduleName, "Syndicate", ex, ""))
            End Try


        End Sub

        ''' <summary>
        ''' This routine is for loading configs - it tries to find a Distributor node with the Distributor name
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub LoadConfig(ByVal value As XmlElement)
            Try
                If Me.UsesExtendedConfig Then
                    ' Try to find the current provider
                    Dim distributorSettings As XmlElement = Nothing
                    If NodeState(value, "Distributor[@name='" & Me.Name() & "']", , , , distributorSettings) <> XmlNodeState.NotInstantiated Then
                        _config = New Config(distributorSettings)
                    End If
                End If
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(_moduleName, "LoadConfig", ex, ""))
            End Try

        End Sub
#End Region

        ' Example config for GenericXmlRpc
        ' <Distributor name="GenericXmlRpc"><add key="methodName" value="weblogUpdates.ping" /><add key="endpoint" value="http://myrpcexample/rpc" /><add key="contentType" value="text/xml"></add></Distributor>

        ''' <summary>
        ''' GenericXmlRpc is a generic ping distributor, that is inherited by other xmlrpc ping distributors
        ''' It can work in its own right if config settings are passed through for it.
        ''' See the example in the code
        ''' </summary>
        ''' <remarks></remarks>
        Public Class GenericXmlRpc
            Inherits Distributor

            Protected _endpoint As String = ""
            Protected _contentType As String = "text/xml"
            Protected _methodName As String = "weblogUpdates.ping"

            Public Sub New(ByRef aWeb As Eonic.Web)
                MyBase.New(aWeb)
                Try
                    Me._usesExtendedConfig = True
                    Me._usesXslTransform = True
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(_moduleName, "New", ex, ""))
                End Try
            End Sub

            Public Overrides Sub Transform()
                Try
                    If _usesExtendedConfig Then
                        If ExtendedConfig.GetValue("methodName") <> "" Then _methodName = ExtendedConfig.GetValue("methodName")
                        If ExtendedConfig.GetValue("endpoint") <> "" Then _endpoint = ExtendedConfig.GetValue("endpoint")
                        If ExtendedConfig.GetValue("contentType") <> "" Then _contentType = ExtendedConfig.GetValue("contentType")
                    End If

                    ContentsNode.OwnerDocument.DocumentElement.SetAttribute("xmlRpcMethodName", _methodName)
                    MyBase.Transform()
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(_moduleName, "Transform", ex, ""))
                End Try

            End Sub

            ''' <summary>
            '''    Syndicate Content for Generic XmlRpc interfaces
            ''' </summary>
            ''' <remarks></remarks>
            Protected Overrides Sub RunSyndicate()
                Try
                    If Not (String.IsNullOrEmpty(_methodName)) _
                        And Not (String.IsNullOrEmpty(_endpoint)) _
                        And Not (String.IsNullOrEmpty(_contentType)) _
                    Then
                        ' Define the request
                        Dim syndicateRequest As New Eonic.Tools.Http.WebRequest(_contentType, "POST", "EonicWeb")

                        ' Send the request and get the response
                        _diagnostics = syndicateRequest.Send(_endpoint, Me.TransformedData)

                        ' Weblogs indicated success with boolean value of 0.
                        ' Rather than parse this, we can simply search the string for an expected repsonse.
                        If _diagnostics.Contains("<boolean>0</boolean>") Then
                            _completed = True
                        End If
                    End If
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(_moduleName, "RunSyndicate", ex, ""))
                End Try

            End Sub
        End Class

        Public Class Weblogs
            Inherits GenericXmlRpc

            Public Sub New(ByRef aWeb As Eonic.Web)
                MyBase.New(aWeb)
                Try
                    _usesXslTransform = True
                    _usesExtendedConfig = False
                    _endpoint = "http://rpc.weblogs.com/RPC2"
                    _methodName = "weblogUpdates.extendedPing"
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(_moduleName, "New", ex, ""))
                End Try
            End Sub

        End Class

        Public Class Technorati
            Inherits GenericXmlRpc

            Public Sub New(ByRef aWeb As Eonic.Web)
                MyBase.New(aWeb)
                Try
                    _usesXslTransform = True
                    _usesExtendedConfig = False
                    _endpoint = "http://rpc.technorati.com/rpc/ping"
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(_moduleName, "New", ex, ""))
                End Try
            End Sub

        End Class

        Public Class PingOMatic
            Inherits GenericXmlRpc

            Public Sub New(ByRef aWeb As Eonic.Web)
                MyBase.New(aWeb)
                Try
                    _usesXslTransform = True
                    _usesExtendedConfig = False
                    _endpoint = "http://rpc.pingomatic.com/"
                    _methodName = "weblogUpdates.extendedPing"
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(_moduleName, "New", ex, ""))
                End Try
            End Sub

        End Class

        ' Class Eonic.Syndication.Distributor.Config

        '  Lightweight xml config loader that picks up xml in web.config form
        '  e.g. <mySettings>
        '           <add key="myKey" value="myValue"/>
        '       </mySettings>
        ''' <summary>
        ''' Class Eonic.Syndication.Distributor.Config
        ''' Lightweight xml config loader that picks up xml in web.config form
        ''' </summary>
        ''' <remarks></remarks>
        Public Class Config

            Private _moduleName As String = "Eonic.Syndication.Distributor.Config"
            Private _settings As XmlElement = Nothing
            Private _isLoaded As Boolean = False

#Region "Events"
            Public Shadows Event OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)

            Private Sub _OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)
                RaiseEvent OnError(sender, e)
            End Sub
#End Region

            Public Sub New()
                _isLoaded = False
            End Sub

            Public Sub New(ByVal newSettings As XmlElement)
                Settings = newSettings
            End Sub

            Public ReadOnly Property IsLoaded() As Boolean
                Get
                    Return _isLoaded
                End Get
            End Property

            Public Property Settings() As XmlElement
                Get
                    Return _settings
                End Get
                Set(ByVal value As XmlElement)
                    Try

                        _settings = value
                        _isLoaded = True

                    Catch ex As Exception
                        _settings = Nothing
                        _isLoaded = False
                    End Try
                End Set
            End Property

            Public Function GetValue(ByVal key As String) As String
                Try
                    Dim setting As XmlElement = Nothing
                    If NodeState(Settings(), "add[@key='" & key & "']", , , , setting) <> XmlNodeState.NotInstantiated Then
                        Return setting.GetAttribute("value")
                    Else
                        Return ""
                    End If
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(_moduleName, "New", ex, ""))
                    Return ""
                End Try
            End Function
        End Class
    End Class

End Class
