Imports System.Xml
Imports System.Web.HttpUtility
Imports System.Web.Configuration
Imports System.Configuration
Imports System.Data
Imports System.Data.SqlClient
Imports VB = Microsoft.VisualBasic
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Collections.Generic
Imports System.Text
Imports SR = System.Reflection
Imports System.Web.UI.WebControls
Imports System.Security.Principal
Imports System.IO
'This is the Indexer/Search items
Imports Lucene.Net.Index
Imports Lucene.Net.Documents
Imports Lucene.Net.Analysis
Imports Lucene.Net.Analysis.Standard
Imports Lucene.Net.Search
Imports Lucene.Net.QueryParsers
'regular expressions
Imports System.Text.RegularExpressions
Imports Protean.Tools.FileHelper
Imports System
Imports System.Linq

Public Class IndexerAsync

    Shadows mcModuleName As String = "Protean.IndexerAsync"

    Dim mcIndexReadFolder As String = "" ' the folder where the index is stored (from config)
    Dim mcIndexWriteFolder As String = "" ' the folder where the index is stored (from config)
    Dim mcIndexCopyFolder As String = ""
    Dim oIndexWriter As IndexWriter 'Lucene class

    Dim oImp As Protean.Tools.Security.Impersonate = New Protean.Tools.Security.Impersonate 'impersonate for access
    Dim bNewIndex As Boolean = False 'if we need a new index or just add to one
    Dim bIsError As Boolean = False
    Dim dStartTime As Date
    Dim bDebug As Boolean = False

    Public nPagesIndexed As Integer
    Public nDocumentsIndexed As Integer
    Public nDocumentsSkipped As Integer
    Public nContentsIndexed As Integer

    Public cExError As String
    Dim moConfig As System.Collections.Specialized.NameValueCollection

    Dim myWeb As Cms

    Public Sub New(ByRef aWeb As Protean.Cms)
        PerfMon.Log("Indexer", "New")
        mcModuleName = "Eonic.Search.Indexer"
        Dim cProcessInfo As String = ""
        myWeb = aWeb
        moConfig = myWeb.moConfig
        Dim siteSearchPath As String = moConfig("SiteSearchPath")
        If LCase(moConfig("SiteSearchDebug")) = "on" Then
            bDebug = True
        End If
        Try
            If siteSearchPath = "" Then siteSearchPath = "..\Index"
            If Not siteSearchPath.EndsWith("\") Then siteSearchPath = siteSearchPath & "\"
            Dim IndexFolder As String = myWeb.goServer.MapPath("\") & siteSearchPath
            Dim dir As New DirectoryInfo(IndexFolder)

            If moConfig("SiteSearchReadPath") = "" Then
                mcIndexReadFolder = IndexFolder & "Read\"
            Else
                mcIndexReadFolder = myWeb.goServer.MapPath("\") & moConfig("SiteSearchReadPath") 'get the location to store the index
            End If
            Dim dirRead As New DirectoryInfo(mcIndexReadFolder)
            If Not dirRead.Exists Then
                dir.CreateSubdirectory("Read")
            End If

            If moConfig("SiteSearchWritePath") = "" Then
                mcIndexWriteFolder = IndexFolder & "Write\"
            Else
                mcIndexWriteFolder = myWeb.goServer.MapPath("\") & moConfig("SiteSearchWritePath") 'get the location to store the index
            End If
            Dim dirWrite As New DirectoryInfo(mcIndexReadFolder)
            If Not dirWrite.Exists Then
                dir.CreateSubdirectory("Write")
            End If

            mcIndexCopyFolder = IndexFolder & "IndexedSite\"
            Dim dirCopy As New DirectoryInfo(mcIndexCopyFolder)
            If Not dirCopy.Exists Then
                dir.CreateSubdirectory("IndexedSite")
            End If

        Catch ex As Exception
            cExError &= ex.ToString & vbCrLf
            returnException(myWeb.msException, mcModuleName, "New", ex, "", , gbDebug)
        End Try
    End Sub

    Public Function GetIndexInfo() As String
        Dim oXmlDoc As New XmlDocument
        Dim oXmlDocRead As New XmlDocument
        Dim oXmlDocWrite As New XmlDocument
        Dim oXmlElmt As XmlElement = oXmlDoc.CreateElement("IndexInfo")
        Dim oXmlReadElmt As XmlElement = oXmlDoc.CreateElement("Read")
        Dim oXmlWriteElmt As XmlElement = oXmlDoc.CreateElement("Write")
        Try
            Dim fs As New FileInfo(mcIndexReadFolder & "/indexInfo.xml")
            If fs.Exists Then
                oXmlDocRead.Load(mcIndexReadFolder & "/indexInfo.xml")
                oXmlDocRead.DocumentElement.SetAttribute("folder", "read")
                oXmlReadElmt.InnerXml = oXmlDocRead.DocumentElement.OuterXml
                oXmlElmt.AppendChild(oXmlReadElmt.FirstChild)
            End If
            fs = New FileInfo(mcIndexWriteFolder & "/indexInfo.xml")
            If fs.Exists Then
                oXmlDocWrite.Load(mcIndexWriteFolder & "/indexInfo.xml")
                oXmlDocWrite.DocumentElement.SetAttribute("folder", "write")
                oXmlWriteElmt.InnerXml = oXmlDocWrite.DocumentElement.OuterXml
                oXmlElmt.AppendChild(oXmlWriteElmt.FirstChild)
            End If

            fs = Nothing
            Return oXmlElmt.OuterXml

        Catch ex As Exception
            cExError &= ex.ToString & vbCrLf
            returnException(myWeb.msException, mcModuleName, "New", ex, "", , gbDebug)
        End Try
    End Function

    Public Function DoIndex(Optional ByVal nPage As Integer = 0, Optional ByRef bResult As Boolean = False) As String
        ' nPage = 59
        PerfMon.Log("Indexer", "DoIndex")
        Dim cProcessInfo As String = ""
        Dim cPageHtml As String = ""
        Dim cPageExtract As String = ""
        Dim cPageXsl As String = "/xsl/search/cleanPage.xsl"
        Dim cExtractXsl As String = "/xsl/search/extract.xsl"
        Dim oPageXml As New XmlDocument

        Dim cRules As String = ""

        Dim nPagesRemaining As Long = 0
        Dim nPagesSkipped As Long = 0
        Dim nContentSkipped As Long = 0
        Dim nIndexed As Integer = 0 'count of the indexed items
        Dim cIndexDetailTypes As String = "NewsArticle,Event,Product,Contact,Document,Job"
        Dim oDS As New DataSet
        Dim oDR As DataRow
        Dim cSQL As String = ""
        Dim IndexDetailTypes() As String
        Dim errElmt As XmlElement

        Dim oIndexInfo As New XmlDocument
        Dim oInfoElmt As XmlElement = oIndexInfo.CreateElement("indexInfo")
        oIndexInfo.AppendChild(oInfoElmt)
        Dim idxConcurrency As Int32 = 10
        If moConfig("indexConcurrency") <> "" Then
            idxConcurrency = CInt(moConfig("indexConcurrency"))
        End If
        Dim lcts As New LimitedConcurrencyLevelTaskScheduler(idxConcurrency)
        Dim factory As New TaskFactory(lcts)
        Dim cts As New CancellationTokenSource()

        Dim TaskArray As New List(Of Task)
        Dim ResponseMessage As String = ""
        Try

            bIsError = False
            If nPage = 0 Then bNewIndex = True 'if we are indexing everything then is a new index
            If moConfig("SiteSearchIndexDetailTypes") <> "" Then
                cIndexDetailTypes = moConfig("SiteSearchIndexDetailTypes")
            End If
            IndexDetailTypes = Split(Replace(cIndexDetailTypes, " ", ""), ",")
            dStartTime = Now

            Dim dLastRun As DateTime
            Dim oFs As New Protean.fsHelper()

            If File.Exists(mcIndexWriteFolder & "/indexInfo.xml") Then
                Dim oLastIndexInfo As New XmlDocument
                oLastIndexInfo.Load(mcIndexWriteFolder & "/indexInfo.xml")
                Dim minInterval As Int32 = 12
                'If moConfig("SiteSearchIndexMinInterval") <> "" Then
                '    minInterval = moConfig("SiteSearchIndexMinInterval")
                'End If
                If moConfig("SiteSearchIndexResultPaging") IsNot Nothing Then
                    minInterval = moConfig("SiteSearchIndexResultPaging")
                End If
                'If moConfig("SiteSearchIndexResultPaging") <> "" Then
                '    minInterval = moConfig("SiteSearchIndexResultPaging")
                'End If

                If Not oLastIndexInfo Is Nothing Then
                    Dim oLastInfoElmt As XmlElement = oLastIndexInfo.DocumentElement
                    dLastRun = oLastInfoElmt.GetAttribute("startTime")
                    If oLastInfoElmt.GetAttribute("endTime") = "" And dLastRun > Now().AddHours(minInterval * -1) Then
                        ResponseMessage = "Last Index is still running. Started:" & dLastRun
                    End If
                End If
            End If

            If ResponseMessage <> "" Then
                Return ResponseMessage

            Else

                Me.StartIndex()

                If bIsError Then Exit Function

                'Check for local xsl or go to common
                If Not IO.File.Exists(goServer.MapPath(cPageXsl)) Then
                    cPageXsl = "/ewcommon" & cPageXsl
                End If
                If Not IO.File.Exists(goServer.MapPath(cExtractXsl)) Then
                    cExtractXsl = "/ewcommon" & cExtractXsl
                End If


                oInfoElmt.SetAttribute("startTime", Tools.Xml.XmlDate(Now(), True))
                oInfoElmt.SetAttribute("cPageXsl", cPageXsl)
                oInfoElmt.SetAttribute("cExtractXsl", cExtractXsl)

                oIndexInfo.Save(mcIndexWriteFolder & "/indexInfo.xml")

                'make a new web but going to have to overide some stuff
                'and double override to be sure

                Dim styleFile As String = CType(goServer.MapPath(cPageXsl), String)
                PerfMon.Log("Web", "ReturnPageHTML - loaded Style")
                Dim oTransform As New Protean.XmlHelper.Transform()
                oTransform.XslFilePath = styleFile
                oTransform.Compiled = False
                oTransform.xsltArgs = New Xsl.XsltArgumentList
                Dim ewXsltExt As New xsltExtensions(myWeb)
                oTransform.xsltArgs.AddExtensionObject("urn:ew", ewXsltExt)

                'full pages
                cSQL = "Select nStructKey,cStructName From tblContentStructure" 'get all structure
                If nPage > 0 Then cSQL &= " WHERE nStructKey = " & nPage 'unless a specific page
                'If nPage > 0 Then cSQL &= " WHERE nStructKey = " & nPage & " Or nStructParId =" & nPage 'unless a specific page

                oDS = myWeb.moDbHelper.GetDataSet(cSQL, "Structure")

                'now we loop through the different tables and index the data
                'Do Pages
                If oDS.Tables.Contains("Structure") Then

                    Dim Tasks As New IndexPageAsync(myWeb.moCtx, oTransform, oIndexWriter, cPageXsl)
                    Tasks.bDebug = bDebug
                    Tasks.mcIndexCopyFolder = mcIndexCopyFolder
                    Tasks.mcIndexReadFolder = mcIndexReadFolder
                    Tasks.mcIndexWriteFolder = mcIndexWriteFolder
                    Tasks.cIndexDetailTypes = cIndexDetailTypes
                    Tasks.IndexDetailTypes = IndexDetailTypes
                    Tasks.oInfoElmt = oInfoElmt
                    Tasks.oIndexInfo = oIndexInfo
                    Tasks.nPagesRemaining = nPagesRemaining

                    nPagesRemaining = oDS.Tables("Structure").Rows.Count
                    Tasks.nPagesRemaining = nPagesRemaining

                    For Each oDR In oDS.Tables("Structure").Rows

                        Dim pageObj As New IndexPageAsync.oPage()
                        pageObj.pgid = oDR("nStructKey")
                        pageObj.pagename = oDR("cStructName")

                        'don't do async
                        'Tasks.IndexSinglePage(pageObj)
                        'or do aSync
                        TaskArray.Add(factory.StartNew(Sub()
                                                           Tasks.IndexSinglePage(pageObj)
                                                       End Sub,
                                    cts.Token))
                    Next

                    Task.WaitAll(TaskArray.ToArray())

                End If

                StopIndex()

                oInfoElmt.SetAttribute("endTime", Tools.Xml.XmlDate(Now(), True))

                'any non-critical errors ?
                If cExError <> "" Then
                    errElmt = oIndexInfo.CreateElement("error")
                    errElmt.InnerXml = cExError
                    oIndexInfo.FirstChild.AppendChild(errElmt)
                End If

                oIndexInfo.Save(mcIndexWriteFolder & "/indexInfo.xml")

                bResult = bIsError

                cts.Dispose()

                'send email update as to index success or failure
                cProcessInfo = "Sending Email Report"
                Dim msg As New Protean.Messaging(myWeb.msException)
                Dim serverSenderEmail As String = moConfig("ServerSenderEmail") & ""
                Dim serverSenderEmailName As String = moConfig("ServerSenderEmailName") & ""
                If Not (Tools.Text.IsEmail(serverSenderEmail)) Then
                    serverSenderEmail = "emailsender@protean.site"
                    serverSenderEmailName = "ProteanCMS Emailer"
                End If
                Dim recipientEmail As String = moConfig("SiteAdminEmail")
                If moConfig("IndexAlertEmail") <> "" Then
                    recipientEmail = moConfig("IndexAlertEmail")
                End If

                msg.emailer(oInfoElmt, "/ewcommon/xsl/Email/IndexerAlert.xsl", "ProteanCMS Indexer", serverSenderEmail, recipientEmail, myWeb.moRequest.ServerVariables("SERVER_NAME") & " Indexer Report")
                msg = Nothing

                Return "Index Complete"

            End If

        Catch ex As Exception

            cExError &= ex.InnerException.StackTrace.ToString & vbCrLf
            returnException(myWeb.msException, mcModuleName, "DoIndex", ex, "", cProcessInfo, gbDebug)
            errElmt = oIndexInfo.CreateElement("error")
            errElmt.InnerXml = cExError
            oIndexInfo.FirstChild.AppendChild(errElmt)
            oInfoElmt.SetAttribute("endTime", Tools.Xml.XmlDate(Now(), True))

            oIndexInfo.Save(mcIndexWriteFolder & "/indexInfo.xml")

            Try
                oIndexWriter.Dispose()
                oIndexWriter = Nothing
            Catch ex2 As Exception

            End Try
        Finally
            Try



            Catch ex As Exception
                returnException(myWeb.msException, mcModuleName, "DoIndex", ex, "", cProcessInfo, gbDebug)
            End Try
        End Try


    End Function

    Private Sub StartIndex()
        PerfMon.Log("Indexer", "StartIndex")
        Dim cProcessInfo As String = ""
        Try
            oImp = New Protean.Tools.Security.Impersonate 'for access
            If oImp.ImpersonateValidUser(moConfig("AdminAcct"), moConfig("AdminDomain"), moConfig("AdminPassword"), , moConfig("AdminGroup")) Then
                EmptyFolder(mcIndexWriteFolder)
                If gbDebug Then
                    EmptyFolder(mcIndexCopyFolder)
                End If

                Dim bCreate As Boolean = True 'check if file exists or if we need to create an index
                If IO.File.Exists(mcIndexWriteFolder & "segments") Then bCreate = False
                'If bNewIndex Then bCreate = True 'override creation dependant on type of index
                Dim indexDir As Lucene.Net.Store.Directory = Lucene.Net.Store.FSDirectory.Open(mcIndexWriteFolder)

                Dim maxLen As New IndexWriter.MaxFieldLength(10000)

                oIndexWriter = New IndexWriter(indexDir, New StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_CURRENT), bCreate, maxLen) 'create the index writer

                'TS added to limit memory usage
                oIndexWriter.SetRAMBufferSizeMB(500)
                oIndexWriter.SetMaxBufferedDeleteTerms(50)
                oIndexWriter.SetMaxBufferedDocs(100)

            Else
                Err.Raise(108, , "Indexer did not validate")

            End If
        Catch ex As Exception
            cExError &= ex.StackTrace.ToString & vbCrLf
            returnException(myWeb.msException, mcModuleName, "StartIndex", ex, "", cProcessInfo, gbDebug)

            bIsError = True
            Try
                oIndexWriter.Close()
                oIndexWriter = Nothing
            Catch ex2 As Exception

            End Try
        End Try
    End Sub

    Private Sub EmptyFolder(ByVal cDirectory As String)
        PerfMon.Log("Indexer", "EmptyFolder")
        Dim cProcessInfo As String = ""
        Try
            If bNewIndex Then
                'delete directories and thier children
                Do Until UBound(IO.Directory.GetDirectories(cDirectory)) <= 0
                    IO.Directory.Delete(IO.Directory.GetDirectories(cDirectory)(0), True)
                Loop
                'delete files
                Do Until UBound(IO.Directory.GetFiles(cDirectory)) <= 0
                    Try
                        IO.File.SetAttributes(IO.Directory.GetFiles(cDirectory)(0), FileAttributes.Normal)
                        IO.File.Delete(IO.Directory.GetFiles(cDirectory)(0))
                    Catch ex As Exception
                        cExError &= ex.StackTrace.ToString & vbCrLf
                        returnException(myWeb.msException, mcModuleName, "Empty Folder", ex, "", cProcessInfo, gbDebug)
                        Exit Sub

                    End Try
                Loop
                'try deleting a hidden folder
                If IO.Directory.Exists(cDirectory & IIf(Right(cDirectory, 1) = "\", "", "\") & "_vti_cnf") Then
                    Try
                        IO.Directory.Delete(cDirectory & IIf(Right(cDirectory, 1) = "\", "", "\") & "_vti_cnf", True)
                    Catch ex As Exception
                        cExError &= ex.ToString & vbCrLf
                        returnException(myWeb.msException, mcModuleName, "Empty Folder", ex, "", cProcessInfo, gbDebug)
                        Exit Sub
                    End Try
                End If
            End If
        Catch ex As Exception
            Try
                oIndexWriter.Dispose()
                oIndexWriter = Nothing
            Catch ex2 As Exception

            End Try
            cExError &= ex.ToString & vbCrLf
            returnException(myWeb.msException, mcModuleName, "Empty Folder", ex, "", cProcessInfo, gbDebug)
        End Try
    End Sub

    Private Sub StopIndex()
        PerfMon.Log("Indexer", "StopIndex")
        Dim cProcessInfo As String = ""
        Try
            oIndexWriter.Optimize()
            oIndexWriter.Commit()
            oIndexWriter.Dispose()
            EmptyFolder(mcIndexReadFolder)
            CopyFolderContents(mcIndexWriteFolder, mcIndexReadFolder)
            oImp.UndoImpersonation()
            oIndexWriter = Nothing
        Catch ex As Exception
            cExError &= ex.ToString & vbCrLf
            returnException(myWeb.msException, mcModuleName, "StopIndex", ex, "", cProcessInfo, gbDebug)
            bIsError = True
        End Try
    End Sub

    Private Sub CopyFolderContents(ByVal cLocation As String, ByVal cDestination As String)
        PerfMon.Log("Indexer", "CopyFolderContents")
        Dim cProcessInfo As String = ""
        Try
            Dim oDI As New IO.DirectoryInfo(mcIndexWriteFolder)
            Dim FileCount As Integer = oDI.GetFiles.Length
            Dim i As Integer
            For i = 0 To FileCount - 1
                Dim cFileName As String = Replace(IO.Directory.GetFiles(cLocation)(i), cLocation, cDestination)
                Debug.WriteLine(cFileName)
                IO.File.Copy(IO.Directory.GetFiles(cLocation)(i), cFileName, True)
            Next
        Catch ex As Exception
            cExError &= ex.ToString & vbCrLf
            returnException(myWeb.msException, mcModuleName, "CopyFolderContents", ex, "", cProcessInfo, gbDebug)
        End Try
    End Sub

    ' Provides a task scheduler that ensures a maximum concurrency level while 
    ' running on top of the thread pool.
    Public Class LimitedConcurrencyLevelTaskScheduler : Inherits TaskScheduler
        ' Indicates whether the current thread is processing work items.
        <ThreadStatic()> Private Shared _currentThreadIsProcessingItems As Boolean

        ' The list of tasks to be executed 
        Private ReadOnly _tasks As LinkedList(Of Task) = New LinkedList(Of Task)()

        'The maximum concurrency level allowed by this scheduler. 
        Private ReadOnly _maxDegreeOfParallelism As Integer

        ' Indicates whether the scheduler is currently processing work items. 
        Private _delegatesQueuedOrRunning As Integer = 0 ' protected by lock(_tasks)

        ' Creates a new instance with the specified degree of parallelism. 
        Public Sub New(ByVal maxDegreeOfParallelism As Integer)
            If (maxDegreeOfParallelism < 1) Then
                Throw New ArgumentOutOfRangeException("maxDegreeOfParallelism")
            End If
            _maxDegreeOfParallelism = maxDegreeOfParallelism
        End Sub

        ' Queues a task to the scheduler. 
        Protected Overrides Sub QueueTask(ByVal t As Task)
            ' Add the task to the list of tasks to be processed.  If there aren't enough 
            ' delegates currently queued or running to process tasks, schedule another. 
            SyncLock (_tasks)
                _tasks.AddLast(t)
                If (_delegatesQueuedOrRunning < _maxDegreeOfParallelism) Then
                    _delegatesQueuedOrRunning = _delegatesQueuedOrRunning + 1
                    NotifyThreadPoolOfPendingWork()
                End If
            End SyncLock
        End Sub

        ' Inform the ThreadPool that there's work to be executed for this scheduler. 
        Private Sub NotifyThreadPoolOfPendingWork()

            ThreadPool.UnsafeQueueUserWorkItem(Sub()
                                                   ' Note that the current thread is now processing work items. 
                                                   ' This is necessary to enable inlining of tasks into this thread.
                                                   _currentThreadIsProcessingItems = True
                                                   Try
                                                       ' Process all available items in the queue. 
                                                       While (True)
                                                           Dim item As Task
                                                           SyncLock (_tasks)
                                                               ' When there are no more items to be processed, 
                                                               ' note that we're done processing, and get out. 
                                                               If (_tasks.Count = 0) Then
                                                                   _delegatesQueuedOrRunning = _delegatesQueuedOrRunning - 1
                                                                   Exit While
                                                               End If

                                                               ' Get the next item from the queue
                                                               item = _tasks.First.Value
                                                               _tasks.RemoveFirst()
                                                           End SyncLock

                                                           ' Execute the task we pulled out of the queue 
                                                           MyBase.TryExecuteTask(item)
                                                       End While
                                                       ' We're done processing items on the current thread 
                                                   Finally
                                                       _currentThreadIsProcessingItems = False
                                                   End Try
                                               End Sub,
                                    Nothing)
        End Sub

        ' Attempts to execute the specified task on the current thread. 
        Protected Overrides Function TryExecuteTaskInline(ByVal t As Task,
                                                     ByVal taskWasPreviouslyQueued As Boolean) As Boolean
            ' If this thread isn't already processing a task, we don't support inlining 
            If (Not _currentThreadIsProcessingItems) Then
                Return False
            End If

            ' If the task was previously queued, remove it from the queue 
            If (taskWasPreviouslyQueued) Then
                ' Try to run the task. 
                If TryDequeue(t) Then
                    Return MyBase.TryExecuteTask(t)
                Else
                    Return False
                End If
            Else
                Return MyBase.TryExecuteTask(t)
            End If
        End Function

        ' Attempt to remove a previously scheduled task from the scheduler. 
        Protected Overrides Function TryDequeue(ByVal t As Task) As Boolean
            SyncLock (_tasks)
                Return _tasks.Remove(t)
            End SyncLock
        End Function

        ' Gets the maximum concurrency level supported by this scheduler. 
        Public Overrides ReadOnly Property MaximumConcurrencyLevel As Integer
            Get
                Return _maxDegreeOfParallelism
            End Get
        End Property

        ' Gets an enumerable of the tasks currently scheduled on this scheduler. 
        Protected Overrides Function GetScheduledTasks() As IEnumerable(Of Task)
            Dim lockTaken As Boolean = False
            Try
                System.Threading.Monitor.TryEnter(_tasks, lockTaken)
                If (lockTaken) Then
                    Return _tasks.ToArray()
                Else
                    Throw New NotSupportedException()
                End If
            Finally
                If (lockTaken) Then
                    System.Threading.Monitor.Exit(_tasks)
                End If
            End Try
        End Function

    End Class

    Public Class IndexPageAsync


        Public oIndexWriter As IndexWriter 'Lucene class
        Public mcIndexReadFolder As String = ""
        Public mcIndexWriteFolder As String = ""
        Public mcIndexCopyFolder As String = ""
        Public oIndexInfo As XmlDocument
        Public cXslPath As String
        Public bIsError As Boolean = False
        Public errElmt As XmlElement
        Public cIndexDetailTypes As String
        Public IndexDetailTypes() As String
        Public bDebug As Boolean
        Public oInfoElmt As XmlElement

        Public nPagesSkipped As Long = 0
        Public nPagesIndexed As Long = 0
        Public nDocumentsIndexed As Long = 0
        Public nDocumentsSkipped As Long = 0
        Public nContentSkipped As Long = 0
        Public nContentsIndexed As Long = 0
        Public nPagesRemaining As Long = 0
        Public doneEvent As System.Threading.ManualResetEvent
        Public nRootId As Long
        Public moTransform As Protean.XmlHelper.Transform
        Public moCtx As System.Web.HttpContext

        Dim nIndexed As Integer = 0 'count of the indexed items

        Public cExError As String
        Private mcModuleName As String = "IndexPageAsync"

#Region "Error Handling"
        Public Shadows Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)

        Private Sub _OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
            RaiseEvent OnError(sender, e)
        End Sub
#End Region

        Public Class oPage
            Public pgid As Long
            Public pagename As String
        End Class

        Public Sub New(oCtx As System.Web.HttpContext, oTransform As Protean.XmlHelper.Transform, objIndexWriter As IndexWriter, pageXSL As String)

            Try
                moCtx = oCtx
                moTransform = oTransform
                oIndexWriter = objIndexWriter
                cXslPath = pageXSL
            Catch ex As Exception
                RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""))
            End Try
        End Sub


        Public Sub IndexSinglePage(oPage As Object)
            Dim cPageHtml As String
            Dim oPageXml As New XmlDocument
            Dim cRules As String = ""
            Dim oElmtRules As XmlElement
            Dim oElmtURL As XmlElement

            Dim cProcessInfo As String
            Dim cPageExtract As String = ""
            Dim myWeb As New Protean.Cms(moCtx)

            Try
                myWeb.InitializeVariables()
                myWeb.Open()
                myWeb.mnUserId = 1
                If (myWeb.moConfig("SiteSearchIndexHiddenDetail") = "on") Then
                    myWeb.mbAdminMode = True
                Else
                    myWeb.mbAdminMode = False
                End If
                myWeb.ibIndexMode = True
                myWeb.ibIndexRelatedContent = (myWeb.moConfig("SiteSearchIndexRelatedContent") = "on")
                myWeb.moTransform = moTransform
                myWeb.moTransform.myWeb = myWeb

                'here we get a copy of the outputted html
                'as the admin user would see it
                'without bieng in admin mode
                'so we can see everything
                myWeb.mnPageId = oPage.pgid
                myWeb.moPageXml = New XmlDocument
                myWeb.mnArtId = Nothing
                myWeb.mbIgnorePath = True
                myWeb.mcEwSiteXsl = cXslPath

                cPageHtml = myWeb.ReturnPageHTML(0, True)
                cPageHtml = Replace(cPageHtml, "<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.1//EN"" ""http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd"">", "")
                cPageHtml = Replace(cPageHtml, " xmlns=""http://www.w3.org/1999/xhtml""", "")

                If cPageHtml = "" Then
                    'we have an error to handle
                    If Not myWeb.msException Is Nothing Then
                        Dim errorElmt As XmlElement = oIndexInfo.CreateElement("IndexElement")
                        errorElmt.InnerText = myWeb.msException
                        Try
                            errorElmt.SetAttribute("pgid", oPage.pgid)
                            errorElmt.SetAttribute("name", oPage.pagename)
                        Catch ex As Exception

                        End Try
                        cExError &= ControlChars.CrLf & errorElmt.OuterXml
                    End If
                    nPagesSkipped += 1
                Else
                    Try
                        oPageXml.LoadXml(cPageHtml)
                        oElmtRules = oPageXml.SelectSingleNode("/html/head/meta[@name='ROBOTS']")
                        oElmtURL = myWeb.moPageXml.SelectSingleNode("/Page/Menu/descendant-or-self::MenuItem[@id='" & myWeb.mnPageId & "']")
                        cRules = ""

                        'If xWeb.mnPageId = 156 Then
                        '   Testing what happens when we hit a specific page
                        '    cProcessInfo = "our page"
                        'End If

                        If Not oElmtRules Is Nothing Then cRules = oElmtRules.GetAttribute("content")
                        If Not InStr(cRules, "NOINDEX") > 0 And Not oElmtURL Is Nothing Then
                            If Not (oElmtURL.GetAttribute("url").StartsWith("http")) Then
                                IndexPage(oElmtURL.GetAttribute("url"), oPageXml.DocumentElement, "Page", myWeb.msException)

                                Dim oPageElmt As XmlElement = oInfoElmt.OwnerDocument.CreateElement("page")
                                oPageElmt.SetAttribute("url", oElmtURL.GetAttribute("url"))
                                oInfoElmt.AppendChild(oPageElmt)

                                nPagesIndexed += 1
                            Else
                                nPagesSkipped += 1
                            End If
                        Else
                            nPagesSkipped += 1
                        End If
                    Catch ex As Exception
                        Dim oPageErrElmt As XmlElement = oIndexInfo.CreateElement("errorInfo")
                        oPageErrElmt.SetAttribute("pgid", myWeb.mnPageId)
                        oPageErrElmt.SetAttribute("type", "Page")
                        oPageErrElmt.InnerText = ex.Message
                        oInfoElmt.AppendChild(oPageErrElmt)
                        nPagesSkipped += 1
                    End Try

                    If Not InStr(cRules, "NOFOLLOW") > 0 Then
                        'Now let index the content of the pages
                        'Only index content where this is the parent page, so we don't index for multiple locations.
                        Dim oElmt As XmlElement
                        Dim oContentElmts As XmlNodeList = myWeb.moPageXml.SelectNodes("/Page/Contents/Content[@type!='FormattedText' and @type!='PlainText' and @type!='MetaData' and @type!='Image' and @type!='xform' and @type!='report' and @type!='xformQuiz' and @type!='Module' and @parId=/Page/@id]")
                        For Each oElmt In oContentElmts
                            'If oElmt.GetAttribute("type") = "Company" Then
                            '    cProcessInfo = "Not Indexing - " & oElmt.GetAttribute("type")
                            'End If
                            ' Dim wordToFind As String = oElmt.GetAttribute("type")
                            ' Dim wordIndex As Integer = Array.IndexOf(IndexDetailTypes, oElmt.GetAttribute("type"))
                            If Not Array.IndexOf(IndexDetailTypes, oElmt.GetAttribute("type")) >= 0 Then
                                'Don't index we don't want this type.
                                cProcessInfo = "Not Indexing - " & oElmt.GetAttribute("type")

                            Else
                                cProcessInfo = "Indexing - " & oElmt.GetAttribute("type") & "id=" & oElmt.GetAttribute("id") & " name=" & oElmt.GetAttribute("name")

                                myWeb.moPageXml = New XmlDocument ' we need to get this again with our content Detail
                                myWeb.moDbHelper.moPageXml = myWeb.moPageXml
                                myWeb.mcEwSiteXsl = cXslPath
                                myWeb.mnArtId = oElmt.GetAttribute("id")
                                myWeb.moContentDetail = Nothing
                                cPageHtml = myWeb.ReturnPageHTML(0, True)
                                'remove any declarations that might affect and Xpath Search
                                cPageHtml = Replace(cPageHtml, "<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.1//EN"" ""http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd"">", "")
                                cPageHtml = Replace(cPageHtml, " xmlns=""http://www.w3.org/1999/xhtml""", "")

                                If cPageHtml = "" Then
                                    'we have an error to handle
                                    If myWeb.msException = "" Then myWeb.msException = Nothing
                                    If Not myWeb.msException Is Nothing Then
                                        Dim errorElmt As XmlElement = oIndexInfo.CreateElement("IndexElement")
                                        errorElmt.InnerXml = myWeb.msException
                                        Try
                                            errorElmt.SetAttribute("pgid", oPage.xslPath)
                                            errorElmt.SetAttribute("name", oPage.pagename)
                                        Catch ex As Exception
                                            ' Don't error if you can't set the above.
                                        End Try
                                        cExError &= ControlChars.CrLf & errorElmt.OuterXml
                                    End If
                                    nPagesSkipped += 1
                                Else
                                    Try
                                        oPageXml.LoadXml(cPageHtml)

                                        If Not oElmt.GetAttribute("type") = "Document" Then
                                            oElmtRules = oPageXml.SelectSingleNode("/html/head/meta[@name='ROBOTS']")
                                            cRules = ""
                                            Dim sPageUrl As String
                                            If Not oElmtURL Is Nothing Then
                                                sPageUrl = oElmtURL.GetAttribute("url")
                                            End If
                                            If Not oElmtRules Is Nothing Then cRules = oElmtRules.GetAttribute("content")
                                            If (Not InStr(cRules, "NOINDEX") > 0) And Not (sPageUrl.StartsWith("http")) Then

                                                'handle cannonical link tag
                                                If Not oPageXml.DocumentElement.SelectSingleNode("descendant-or-self::link[@rel='canonical']") Is Nothing Then
                                                    Dim oLinkElmt As XmlElement = oPageXml.DocumentElement.SelectSingleNode("descendant-or-self::link[@rel='canonical']")
                                                    If oLinkElmt.GetAttribute("href") <> "" Then
                                                        sPageUrl = oLinkElmt.GetAttribute("href")
                                                    End If
                                                End If

                                                IndexPage(sPageUrl, oPageXml.DocumentElement, oElmt.GetAttribute("type"), myWeb.msException)

                                                Dim oPageElmt As XmlElement = oInfoElmt.OwnerDocument.CreateElement("page")
                                                oPageElmt.SetAttribute("url", sPageUrl)
                                                oInfoElmt.AppendChild(oPageElmt)

                                                nIndexed += 1
                                                nContentsIndexed += 1
                                            Else
                                                nContentSkipped += 1
                                            End If
                                        Else
                                            Dim oDocElmt As XmlElement
                                            For Each oDocElmt In oElmt.SelectNodes("descendant-or-self::Path")
                                                If Not oDocElmt.InnerText = "" Then
                                                    If oDocElmt.InnerText.StartsWith("http") Then
                                                        'don't index
                                                        nDocumentsSkipped += 1
                                                    Else
                                                        cProcessInfo = "Indexing - " & oDocElmt.InnerText
                                                        Dim fileAsText As String = GetFileText(myWeb.goServer.MapPath(oDocElmt.InnerText), myWeb.msException)
                                                        IndexPage(myWeb.mnPageId, "<h1>" & oElmt.GetAttribute("name") & "</h1>" & fileAsText, oDocElmt.InnerText, myWeb.msException, oElmt.GetAttribute("name"), "Download", myWeb.mnArtId, cPageExtract, IIf(IsDate(oElmt.GetAttribute("publish")), CDate(oElmt.GetAttribute("publish")), Nothing), IIf(IsDate(oElmt.GetAttribute("update")), CDate(oElmt.GetAttribute("update")), Nothing))

                                                        Dim oPageElmt As XmlElement = oInfoElmt.OwnerDocument.CreateElement("page")
                                                        oPageElmt.SetAttribute("file", oDocElmt.InnerText)
                                                        oInfoElmt.AppendChild(oPageElmt)

                                                        nIndexed += 1
                                                        nDocumentsIndexed += 1
                                                    End If
                                                End If
                                            Next
                                        End If
                                    Catch ex As Exception
                                        Dim oPageErrElmt As XmlElement = oIndexInfo.CreateElement("errorInfo")
                                        oPageErrElmt.SetAttribute("pgid", myWeb.mnPageId)
                                        oPageErrElmt.SetAttribute("type", oElmt.GetAttribute("type"))
                                        oPageErrElmt.SetAttribute("artid", oElmt.GetAttribute("id"))
                                        oPageErrElmt.InnerText = ex.Message
                                        oInfoElmt.AppendChild(oPageErrElmt)
                                        nContentSkipped += 1
                                        cProcessInfo = cPageHtml
                                    End Try

                                End If

                            End If
                        Next

                    End If

                    If bIsError Then
                        errElmt = oIndexInfo.CreateElement("error")
                        errElmt.InnerXml = cExError
                        oIndexInfo.FirstChild.AppendChild(errElmt)
                        oInfoElmt.SetAttribute("endTime", Tools.Xml.XmlDate(Now(), True))
                    End If

                    Interlocked.Decrement(nPagesRemaining)

                    If oInfoElmt.GetAttribute("indexCount") Then
                        oInfoElmt.SetAttribute("indexCount", nIndexed)
                    End If
                    If oInfoElmt.GetAttribute("pagesIndexed") Then
                        oInfoElmt.SetAttribute("pagesIndexed", nPagesIndexed)
                    End If
                    If oInfoElmt.GetAttribute("pagesRemaining") IsNot Nothing Then
                        oInfoElmt.SetAttribute("pagesRemaining", nPagesRemaining)
                    End If
                    If oInfoElmt.GetAttribute("pagesSkipped") IsNot Nothing Then
                        oInfoElmt.SetAttribute("pagesSkipped", nPagesSkipped)
                    End If
                    If oInfoElmt.GetAttribute("contentCount") IsNot Nothing Then
                        oInfoElmt.SetAttribute("contentCount", nContentsIndexed)
                    End If
                    If oInfoElmt.GetAttribute("contentSkipped") IsNot Nothing Then
                        oInfoElmt.SetAttribute("contentSkipped", nContentSkipped)
                    End If
                    If oInfoElmt.GetAttribute("documentsIndexed") IsNot Nothing Then
                        oInfoElmt.SetAttribute("documentsIndexed", nDocumentsIndexed)
                    End If
                    If oInfoElmt.GetAttribute("documentsSkipped") IsNot Nothing Then
                        oInfoElmt.SetAttribute("documentsSkipped", nDocumentsSkipped)
                    End If
                    'sometimes thefile will be locked, that is OK we save it when we can.
                    If Not FileInUse(mcIndexWriteFolder & "/indexInfo.xml") Then
                        oIndexInfo.Save(mcIndexWriteFolder & "/indexInfo.xml")
                    End If

                    If bIsError Then
                        Exit Sub
                    End If

                End If

            Catch ex As Exception
                cExError = ex.Message
            Finally
                oPage = Nothing
                myWeb.Close()
                myWeb = Nothing
            End Try
        End Sub

        Public Function FileInUse(ByVal sFile As String) As Boolean
            If System.IO.File.Exists(sFile) Then
                Try
                    Dim F As Short = FreeFile()
                    FileOpen(F, sFile, OpenMode.Binary, OpenAccess.ReadWrite, OpenShare.LockReadWrite)
                    FileClose(F)
                Catch
                    Return True
                End Try
            End If
        End Function

        Private Sub IndexPage(ByVal url As String, ByVal pageXml As XmlElement, ByVal pageType As String, ByRef sException As String)

            Dim methodName As String = "IndexPage(String,XmlElement,[String])"
            PerfMon.Log("Indexer", methodName)

            Dim processInfo As String = url

            Try

                Dim indexDoc As New Document

                ' Add the basic field types
                indexDoc.Add(New Field("url", url, Field.Store.YES, Field.Index.NOT_ANALYZED))
                indexDoc.Add(New Field("type", pageType, Field.Store.YES, Field.Index.NOT_ANALYZED))


                If pageXml IsNot Nothing Then

                    ' Add the meta data to the fields
                    For Each metaContent As XmlElement In pageXml.SelectNodes("/html/head/meta")

                        indexMeta(indexDoc, metaContent, sException)

                    Next

                    ' Add the text
                    Dim body As XmlElement = pageXml.SelectSingleNode("/html/body")
                    Dim text As String = ""
                    If body Is Nothing Then
                        text = pageXml.OuterXml
                    Else
                        text = body.InnerXml
                    End If
                    indexDoc.Add(New Field("text", text, Field.Store.YES, Field.Index.ANALYZED)) 'the actual content/text 


                End If

                ' Save the page
                SavePage(url, pageXml.OuterXml)

                ' Add document to the index
                oIndexWriter.AddDocument(indexDoc)



            Catch ex As Exception
                Try
                    ' oIndexWriter.Close()
                    ' oIndexWriter = Nothing
                Catch ex2 As Exception

                End Try
                cExError &= ex.StackTrace.ToString & vbCrLf
                returnException(sException, mcModuleName, methodName, ex, "", processInfo, gbDebug)
                bIsError = True
            End Try
        End Sub


        Private Sub IndexPage(ByVal nPageId As Integer, ByVal cPageText As String, ByVal cURL As String, ByVal cPageTitle As String, ByRef sException As String, Optional ByVal cContentType As String = "Page", Optional ByVal nContentId As Long = 0, Optional ByVal cAbstract As String = "", Optional ByVal dPublish As Date = Nothing, Optional ByVal dUpdate As Date = Nothing)
            PerfMon.Log("Indexer", "IndexPage")
            Dim cProcessInfo As String = cURL

            Try
                Dim oDoc As New Document 'This is the document element
                'here we need to get the proper paths

                oDoc.Add(New Field("pgid", nPageId.ToString(), Field.Store.YES, Field.Index.ANALYZED))
                oDoc.Add(New Field("artid", nContentId.ToString, Field.Store.YES, Field.Index.NOT_ANALYZED))

                oDoc.Add(New Field("url", cURL, Field.Store.YES, Field.Index.NOT_ANALYZED)) 'url of the page (simple)
                oDoc.Add(New Field("name", cPageTitle, Field.Store.YES, Field.Index.NOT_ANALYZED)) 'the name of the page
                oDoc.Add(New Field("contenttype", cContentType, Field.Store.YES, Field.Index.NOT_ANALYZED))  'the type of the content


                If cAbstract <> "" Then
                    oDoc.Add(New Field("abstract", cAbstract, Field.Store.YES, Field.Index.NOT_ANALYZED))
                End If
                If Not dPublish = Nothing Then
                    oDoc.Add(New Field("publishDate", xmlDate(dPublish), Field.Store.YES, Field.Index.NOT_ANALYZED))
                End If
                If Not dUpdate = Nothing Then
                    oDoc.Add(New Field("updateDate", xmlDate(dUpdate), Field.Store.YES, Field.Index.NOT_ANALYZED))

                End If
                oDoc.Add(New Field("text", cPageText, Field.Store.YES, Field.Index.ANALYZED)) 'the actual content/text 

                oIndexWriter.AddDocument(oDoc) 'add it to the index

                SavePage(cURL, cPageText)

                '@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                'This is also where we can recreate the site as static html if needed
                '@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@


            Catch ex As Exception
                cExError &= ex.ToString & vbCrLf
                returnException(sException, mcModuleName, "IndexPage", ex, "", cProcessInfo, gbDebug)
                bIsError = True
            End Try
        End Sub


        Private Sub indexMeta(ByRef indexDocument As Document, ByVal metaContent As XmlElement, ByRef sException As String, Optional ByVal forSorting As Boolean = False)
            Dim processInfo As String = ""

            ' Values grabbed from each content item
            Dim indexContent As Field.Index = Field.Index.NOT_ANALYZED
            Dim storeContent As Field.Store = Field.Store.YES
            Dim metaName As String = ""
            Dim metaContentValue As String = ""
            Dim metaType As String = ""

            ' The abstract field and type specific fields
            Dim metaField As AbstractField = Nothing
            Dim metaNumericField As NumericField

            ' type specific value conversion
            Dim convertedNumber As Object = Nothing
            Dim convertedDate As Date

            Try

                ' Determine whether to tokenize this - by default, no
                indexContent = IIf(metaContent.GetAttribute("tokenize") = "true" And Not forSorting, Field.Index.ANALYZED, Field.Index.NOT_ANALYZED)

                ' Determine whether to store this - by default, YES
                storeContent = IIf(metaContent.GetAttribute("store") = "false" Or forSorting, Field.Store.NO, Field.Store.YES)

                metaName = metaContent.GetAttribute("name")
                If forSorting Then metaName = "ewsort-" & metaName

                ' Get content - this will either be an attribute, or, if not present, the innerXml of the node
                If metaContent.Attributes.GetNamedItem("content") Is Nothing Then
                    metaContentValue = metaContent.InnerXml

                Else
                    metaContentValue = metaContent.GetAttribute("content")
                End If

                metaType = metaContent.GetAttribute("type")

                ' Create the field - based on type
                Select Case metaType



                    Case "float", "number"

                        If Tools.Number.CheckAndReturnStringAsNumber(
                                                         metaContentValue,
                                                         convertedNumber,
                                                         IIf(metaType = "float", GetType(Single), GetType(Long))
                                                        ) Then

                            ' Create the numeric field
                            metaNumericField = New NumericField(
                                                        metaName,
                                                        storeContent,
                                                         True
                                                        )


                            ' Set the value
                            Select Case metaType

                                Case "float"
                                    metaNumericField.SetFloatValue(convertedNumber)

                                Case Else
                                    metaNumericField.SetLongValue(convertedNumber)

                            End Select

                            metaField = metaNumericField

                        Else

                            ' It's not a number don't add it
                            metaField = Nothing

                        End If


                    Case "date"

                        ' Check if the string is a valid date
                        If Not (String.IsNullOrEmpty(metaContentValue)) AndAlso Date.TryParse(metaContentValue, convertedDate) Then

                            metaField = New Field(
                                                    metaName,
                                                    DateTools.DateToString(metaContentValue, DateTools.Resolution.SECOND),
                                                    storeContent,
                                                    indexContent
                                                    )
                        Else

                            ' It's not a date don't add it
                            metaField = Nothing

                        End If


                    Case Else

                        metaField = New Field(
                                                    metaName,
                                                    metaContentValue,
                                                    storeContent,
                                                    indexContent
                                                    )

                End Select


                If metaField IsNot Nothing Then
                    indexDocument.Add(metaField)

                    ' Check for sorting
                    If metaContent.GetAttribute("sortable") = "true" And Not forSorting Then
                        indexMeta(indexDocument, metaContent, sException, True)
                    End If
                End If

            Catch ex As Exception
                cExError &= ex.ToString & vbCrLf
                returnException(sException, mcModuleName, "indexMeta", ex, "", processInfo, gbDebug)
                bIsError = True
            End Try

        End Sub

        Private Function GetFileText(ByVal cPath As String, ByRef sException As String, Optional ByVal cOtherText As String = "") As String
            PerfMon.Log("Indexer", "GetFileText")
            Dim cProcessInfo As String = ""
            Try
                Dim oFile As New FileDoc(cPath)
                Return cOtherText & oFile.Text
            Catch ex As Exception
                cExError &= ex.ToString & vbCrLf
                returnException(sException, mcModuleName, "DoCheck", ex, "", cProcessInfo, gbDebug)
                Return cOtherText
            End Try
        End Function

        Private Sub SavePage(ByVal cUrl As String, ByVal cBody As String)
            PerfMon.Log("Indexer", "IndexPage")
            Dim cProcessInfo As String = ""
            Dim filename As String = ""
            Dim filepath As String = ""
            Dim artId As String = ""
            Dim Ext As String = ".html"
            Try
                If bDebug Then


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
                    oFS.mcStartFolder = mcIndexCopyFolder

                    cProcessInfo = "Saving:" & mcIndexCopyFolder & filepath & "\" & filename & Ext

                    'Tidy up the filename
                    filename = ReplaceIllegalChars(filename)
                    filename = Replace(filename, "\", "-")
                    filepath = Replace(filepath, "/", "\") & ""
                    If filepath.StartsWith("\") And mcIndexCopyFolder.EndsWith("\") Then
                        filepath.Remove(0, 1)
                    End If

                    cProcessInfo = "Saving:" & mcIndexCopyFolder & filepath & "\" & filename & Ext
                    Dim FullFilePath As String = mcIndexCopyFolder & filepath & "\" & filename

                    If FullFilePath.Length > 255 Then
                        FullFilePath = Left(FullFilePath, 240) & Ext
                    Else
                        FullFilePath = FullFilePath & Ext
                    End If

                    If filepath <> "" Then
                        Dim sError As String = oFS.CreatePath(filepath)
                        If sError = "1" Then
                            System.IO.File.WriteAllText(FullFilePath, cBody, System.Text.Encoding.UTF8)
                        Else
                            cExError &= "<Error>Create Path: " & filepath & " - " & sError & "</Error>" & vbCrLf
                        End If
                    Else
                        System.IO.File.WriteAllText(mcIndexCopyFolder & filename & Ext, cBody, System.Text.Encoding.UTF8)
                    End If



                    oFS = Nothing
                End If
            Catch ex As Exception
                'if saving of a page fails we are not that bothered.
                cExError &= "<Error>" & filepath & filename & ex.Message & "</Error>" & vbCrLf
                'returnException(myWeb.msException, mcModuleName, "SavePage", ex, "", cProcessInfo, gbDebug)
                'bIsError = True
            End Try
        End Sub

    End Class


End Class


