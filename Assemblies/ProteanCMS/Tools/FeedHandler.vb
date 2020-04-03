Imports System.Xml
Imports System.Web.HttpUtility
Imports System.Web.Configuration
Imports System.IO
Imports System.Collections
Imports System.Data
Imports System.Data.sqlClient
Imports VB = Microsoft.VisualBasic
Imports System.Net
Imports System.Text.RegularExpressions
Imports Protean.Tools
Imports System.Xml.Linq
Imports System


Public Class FeedHandler

    Public cFeedURL As String 'placeholder values so we dont have to keep parsing them to all the subs
    Public cXSLTransformPath As String
    Public nHostPageID As Integer
    Public nSave As SaveMode

    Public bResult As Boolean = True
    Public oResultElmt As XmlElement
    Public TotalsElmt As XmlElement
    Public FeedItemNode As String
    Public FeedXml As XmlDocument

    Public oDBH As Cms.dbHelper
    Public oTransform As Protean.XmlHelper.Transform
    Public oAdmXFrm As New Cms.Admin.AdminXforms()

    Dim oConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/web")

    Private _countertypes As String() = {"add", "update", "delete", "archive", "total", "notupdated"}
    Private _counters As CounterCollection
    Private _updateExistingItems As Boolean


    Public Enum SaveMode
        Delete = 0
        Archive = 1
        Manual = 2
    End Enum


    Public Sub New(ByVal cURL As String, ByVal cXSLPath As String, ByVal nPageId As Long, ByVal nSaveMode As Integer, Optional ByRef oResultRecorderElmt As XmlElement = Nothing, Optional ByVal cItemNodeName As String = "")
        PerfMon.Log("FeedHandler", "New")
        Try
            oDBH = New Cms.dbHelper("Data Source=" & oConfig("DatabaseServer") & "; " &
            "Initial Catalog=" & oConfig("DatabaseName") & "; " &
            "user id=" & oConfig("DatabaseUsername") & "; password=" & oConfig("DatabasePassword"), 1)
            oDBH.ResetConnection("Data Source=" & oConfig("DatabaseServer") & "; " &
                    "Initial Catalog=" & oConfig("DatabaseName") & "; " &
                    oDBH.GetDBAuth())
            oDBH.myWeb = New Protean.Cms(System.Web.HttpContext.Current)
            oDBH.myWeb.InitializeVariables()
            'oDBH.myWeb.Open()
            oAdmXFrm.goConfig = oConfig
            oAdmXFrm.moDbHelper = oDBH
            oAdmXFrm.myWeb = oDBH.myWeb

            'set the main values
            cFeedURL = Replace(cURL, "&amp;", "&") 'when saving a url it can replace ampersands
            cXSLTransformPath = cXSLPath
            nHostPageID = nPageId
            nSave = nSaveMode
            FeedItemNode = cItemNodeName
            oResultElmt = oResultRecorderElmt
            TotalsElmt = Xml.addElement(oResultElmt, "Totals")
            _updateExistingItems = True
            _counters = New CounterCollection()
            InitialiseCounters()
            oTransform = New Protean.XmlHelper.Transform(oDBH.myWeb, cXSLTransformPath, False)
        Catch ex As Exception
            AddExternalError(ex)
        End Try
    End Sub

    Public Property UpdateExistingItems() As Boolean
        Get
            Return _updateExistingItems
        End Get
        Set(ByVal value As Boolean)
            _updateExistingItems = value
        End Set
    End Property


    Private Sub _OnCounterChange(ByVal sender As Counter, ByVal e As EventArgs)

        If Me.TotalsElmt IsNot Nothing Then
            TotalsElmt.SetAttribute(sender.Name, sender.ToInt)
        End If

    End Sub

    Private Sub InitialiseCounters()
        Dim ctr As Counter
        _counters.Clear()
        For Each countertype As String In _countertypes
            ctr = _counters.Add(countertype)
            AddHandler ctr.OnChange, AddressOf _OnCounterChange
        Next
    End Sub



    Public Function ProcessFeeds() As Boolean

        Try
            'get the feed instances

            'sort the guids so we have something to compare
            'UpdateGuids(oInstanceXML)
            'now we need to compare them to existing feed items on the page
            'and depending on the save mode, ignore/delete
            'we wont overwrite details in case the admin has edited some text



            Select Case LCase(oConfig("FeedMode"))
                Case "import"
                    Dim oInstanceXML As XmlDocument = GetFeedItems()
                    If Not oInstanceXML Is Nothing Then
                        If LCase(oConfig("Debug")) = "on" Then
                            oInstanceXML.Save(goServer.MapPath("/parsedFeed.xml"))
                        End If
                        Me.AddExternalMessage(oDBH.importObjects(oInstanceXML.DocumentElement, cFeedURL, cXSLTransformPath))
                    Else
                        Me.AddExternalMessage("No Feed Items")
                    End If
                Case "stream"
                    Me.AddExternalMessage(ImportStream())
                Case Else
                    Dim oInstanceXML As XmlDocument = GetFeedItems()
                    If Not oInstanceXML Is Nothing Then
                        CompareFeedItems(oInstanceXML)
                    Else
                        Me.AddExternalMessage("No Feed Items")
                    End If

            End Select


            If Not msException = "" Then
                bResult = False
                AddExternalMessage(msException)
            End If
            Return bResult
        Catch ex As Exception
            AddExternalError(ex)
        End Try
    End Function

    Public Function ImportStream() As String

        Dim instanceNodeName As String = FeedItemNode
        Dim origInstance As XElement
        Dim ProcessedQty As Long = 0
        Dim completeCount As Long = 0
        Dim startNo As Long = 0
        Dim processInfo As String
        Dim logId As Long = 0
        Try

            oTransform.Compiled = True
            oTransform.XslFilePath = cXSLTransformPath

            Dim ReturnMessage As String = "Streaming Feed "
            logId = oDBH.logActivity(Protean.Cms.dbHelper.ActivityType.ContentImport, 0, 0, 0, ReturnMessage & " Started using " & cXSLTransformPath)

            Dim cDeleteTempTableName As String = "tmp-" & cFeedURL.Substring(cFeedURL.LastIndexOf("/") + 1).Replace(".xml", "").Replace(".ashx", "")
            Dim eventsDoneEvt As New System.Threading.ManualResetEvent(False)
            Dim Tasks As New Protean.Cms.dbImport(oDBH.oConn.ConnectionString, 0)
            Dim workerThreads As Integer = 0
            Dim portThreads As Integer = 0
            System.Threading.ThreadPool.GetMaxThreads(workerThreads, portThreads)
            System.Threading.ThreadPool.SetMaxThreads(workerThreads / 2, portThreads / 2)
            Dim doneEvents(0) As System.Threading.ManualResetEvent

            Dim settings As XmlWriterSettings = New XmlWriterSettings()
            settings.Indent = True
            settings.OmitXmlDeclaration = True
            settings.NewLineOnAttributes = False
            settings.ConformanceLevel = ConformanceLevel.Document
            settings.CheckCharacters = True
            Dim debugFolder As String = ""

            If LCase(oConfig("Debug")) = "on" Then
                Dim ofs As New Protean.fsHelper
                ofs.mcRoot = "../"
                ofs.CreatePath("/importtest")
                ofs = Nothing
                Dim newDir As New DirectoryInfo(System.Web.HttpContext.Current.Request.MapPath("/"))
                debugFolder = newDir.Parent.FullName & "\importtest\"
            End If

            oDBH.updateActivity(logId, cDeleteTempTableName & " Streaming Start x Objects")
            'is the feed XML

            Dim wrequest As WebRequest = WebRequest.Create(cFeedURL)
            wrequest.Timeout = -1
            Using response As WebResponse = wrequest.GetResponse()
                Using reader As XmlReader = XmlReader.Create(response.GetResponseStream())
                    Dim name As XElement = Nothing
                    Dim item As XElement = Nothing
                    Dim sDoc As String = ""
                    reader.MoveToContent()
                    While reader.Read()
                        If reader.NodeType = XmlNodeType.Element AndAlso reader.Name = instanceNodeName Then

                            origInstance = TryCast(XElement.ReadFrom(reader), XElement)

                            Dim oWriter As TextWriter = New StringWriter
                            Dim xWriter As XmlWriter = XmlWriter.Create(oWriter, settings)
                            Try

                                Dim xreader As XmlReader = origInstance.CreateReader()
                                xreader.MoveToContent()
                                oTransform.Process(xreader, xWriter)

                                sDoc = oWriter.ToString()
                                ' sDoc = Regex.Replace(sDoc, "&gt;", ">")
                                ' sDoc = Regex.Replace(sDoc, "&lt;", "<")
                                sDoc = Protean.Tools.Xml.convertEntitiesToCodesFast(sDoc)
                                Dim filename As String
                                Dim xDoc As New XmlDocument
                                xDoc.LoadXml(sDoc)
                                Dim oInstance As XmlElement
                                For Each oInstance In xDoc.DocumentElement.SelectNodes("descendant-or-self::instance")
                                    Dim stateObj As New Protean.Cms.dbImport.ImportStateObj()
                                    stateObj.oInstance = oInstance
                                    stateObj.LogId = logId
                                    stateObj.FeedRef = cFeedURL
                                    stateObj.CompleteCount = completeCount
                                    stateObj.totalInstances = 0
                                    stateObj.bSkipExisting = False
                                    stateObj.bResetLocations = True
                                    stateObj.nResetLocationIfHere = 0
                                    stateObj.bOrphan = False
                                    stateObj.bDeleteNonEntries = False
                                    stateObj.cDeleteTempTableName = cDeleteTempTableName
                                    stateObj.moTransform = oTransform

                                    ' If oInstance.NextSibling Is Nothing Then
                                    '     cProcessInfo = "Is Last"
                                    '      eventsDoneEvt.Set()
                                    ' End If

                                    System.Threading.ThreadPool.QueueUserWorkItem(New System.Threading.WaitCallback(AddressOf Tasks.ImportSingleObject), stateObj)
                                    stateObj = Nothing
                                    completeCount = completeCount + 1
                                Next

                                If LCase(oConfig("Debug")) = "on" Then
                                    If xDoc.DocumentElement.SelectSingleNode("descendant-or-self::cContentForiegnRef[1]") Is Nothing Then
                                        filename = "ImportStreamFile"
                                    Else
                                        filename = xDoc.DocumentElement.SelectSingleNode("descendant-or-self::cContentForiegnRef[1]").InnerText.Replace("/", "-")
                                    End If
                                    xDoc.Save(debugFolder & filename & ".xml")
                                End If

                                xDoc = Nothing
                                origInstance = Nothing
                                oWriter = Nothing
                                xWriter = Nothing

                            Catch ex2 As Exception
                                processInfo = sDoc

                                AddExternalMessage(ex2.ToString & ex2.StackTrace.ToString & " DOC {" & sDoc & "} EndDoc")
                                bResult = False
                                ' AddExternalError(ex2)
                            End Try


                        End If
                    End While
                    eventsDoneEvt.Set()

                End Using

            End Using
            ReturnMessage = cDeleteTempTableName & " " & completeCount & " Items Queued For Import"
            oDBH.logActivity(Protean.Cms.dbHelper.ActivityType.ContentImport, 0, 0, 0, ReturnMessage & " Queued")

            oDBH.myWeb.ClearPageCache()

            Return completeCount & " Items Processed"

        Catch ex As Exception
            If logId > 0 Then
                oDBH.updateActivity(logId, cFeedURL & "Error" & ex.Message)
            End If
            AddExternalError(ex)
        End Try
    End Function

    Function GetFeedItems() As XmlDocument
        Dim oFeedXML As String = ""
        Try
            Dim oResXML As New XmlDocument
            If FeedXml Is Nothing Then
                'Get the feed xml
                Dim oRequest As HttpWebRequest
                Dim oResponse As HttpWebResponse = Nothing
                Dim oReader As StreamReader

                'request the page
                oRequest = DirectCast(System.Net.WebRequest.Create(cFeedURL), HttpWebRequest)
                ' Force a user agent for rubbish feed providers.
                oRequest.UserAgent = "Mozilla/5.0 (compatible; eonicweb v5.1)"
                ' Set a 10 min timeout
                If oConfig("FeedTimeout") <> "" Then
                    oRequest.Timeout = oConfig("FeedTimeout")
                End If

                oDBH.logActivity(Cms.dbHelper.ActivityType.Custom1, 0, 0, 0, 0, "getting url: " & cFeedURL)

                oResponse = DirectCast(oRequest.GetResponse(), HttpWebResponse)
                oReader = New StreamReader(oResponse.GetResponseStream())
                oFeedXML = oReader.ReadToEnd
                '  oDBH.logActivity(Cms.dbHelper.ActivityType.Custom1, 0, 0, 0, 0, "received url: " & cFeedURL)
                ' The problem with masking namespaces is that you have to deal with any node that calls that namespace.
                'oFeedXML = Replace(oFeedXML, "xmlns:", "exemelnamespace")
                'oFeedXML = Replace(oFeedXML, "xmlns", "exemelnamespace")

                'TS commented out for LogicRc Feed
                'oFeedXML = Regex.Replace(oFeedXML, "&gt;", ">")
                'oFeedXML = Regex.Replace(oFeedXML, "&lt;", "<")

                'oFeedXML = xmlTools.convertEntitiesToCodes(oFeedXML)
                oResXML.InnerXml = oFeedXML
            Else
                oResXML = FeedXml
            End If



            oResXML.InnerXml = oFeedXML
            If LCase(oConfig("Debug")) = "on" Then
                File.WriteAllText(goServer.MapPath("/recivedFeedRaw.xml"), oResXML.OuterXml)
            End If
            'now get the feed into out format
            Dim cFeedItemXML As String
            Dim oTW As IO.TextWriter = New StringWriter()
            Dim oTR As IO.TextReader
            oDBH.logActivity(Cms.dbHelper.ActivityType.Custom1, 0, 0, 0, 0, "Start transform url: " & cFeedURL)
            oTransform.XSLFile = cXSLTransformPath
            oTransform.Compiled = False
            oTransform.Process(oResXML, oTW)
            oTR = New StringReader(oTW.ToString())
            cFeedItemXML = oTR.ReadToEnd
            oDBH.logActivity(Cms.dbHelper.ActivityType.Custom1, 0, 0, 0, 0, "End transform url: " & cFeedURL)


            ' Strip out the xmlns
            cFeedItemXML = Regex.Replace(cFeedItemXML, "xmlns(\:\w*)?=""[^""]*""", "")

            ' Fix Number Entities
            cFeedItemXML = cFeedItemXML.Replace("&amp;#", "&#")

            cFeedItemXML = cFeedItemXML.Replace("&amp;", "&")
            cFeedItemXML = cFeedItemXML.Replace("&", "&amp;")
            'Fix any missing &amp;
            ' cFeedItemXML = Regex.Replace(cFeedItemXML, "/&(?!amp;)/", "&amp;")

            File.WriteAllText(goServer.MapPath("/recivedFeedTransformed.xml"), cFeedItemXML)

            Dim oInstanceXML As New XmlDocument
            oInstanceXML.LoadXml(stripNonValidXMLCharacters(cFeedItemXML))
            ' oInstanceXML.InnerXml = cFeedItemXML

            ' Populate empty url nodes
            For Each oUrlNode As XmlElement In oInstanceXML.SelectNodes("//url[.='']")
                oUrlNode.InnerText = cFeedURL
            Next
            oDBH.logActivity(Cms.dbHelper.ActivityType.Custom1, 0, 0, 0, 0, "Start Tidy")
            ' If the Body has been cast as CData then the html will not have been converted
            Dim sContent As String
            For Each oBodyItem As XmlElement In oInstanceXML.SelectNodes("//*[(local-name()='Body' and not(@htmlTransform='off')) or @htmlTransform='on']")
                sContent = oBodyItem.InnerText
                If sContent <> "" Then
                    Try
                        oBodyItem.InnerXml = sContent
                        oBodyItem.SetAttribute("htmlTransform", "innertext-innerxml")
                    Catch
                        oBodyItem.InnerXml = Protean.tidyXhtmlFrag(sContent)
                        oBodyItem.SetAttribute("htmlTransform", "tidyXhtml")
                    End Try
                End If
            Next
            oDBH.logActivity(Cms.dbHelper.ActivityType.Custom1, 0, 0, 0, 0, "End Tidy")
            Return oInstanceXML

        Catch ex As Exception
            oDBH.logActivity(Cms.dbHelper.ActivityType.Custom1, 0, 0, 0, 0, "error getting url: " & ex.Message & ex.StackTrace)
            AddExternalError(ex)
            If oFeedXML <> "" Then AddExternalMessage(oFeedXML)
            Return Nothing
        End Try
    End Function

    Public Function stripNonValidXMLCharacters(ByVal textIn As String) As [String]
        Dim textOut As New System.Text.StringBuilder()
        ' Used to hold the output.
        Dim current As Integer
        ' Used to reference the current character.
        If textIn Is Nothing OrElse textIn = String.Empty Then
            Return String.Empty
        End If
        ' vacancy test.
        For i As Integer = 0 To textIn.Length - 1
            current = AscW(textIn(i))


            If (current = &H9 OrElse current = &HA OrElse current = &HD) OrElse ((current >= &H20) AndAlso (current <= &HD7FF)) OrElse ((current >= &HE000) AndAlso (current <= &HFFFD)) OrElse ((current >= &H10000) AndAlso (current <= &H10FFFF)) Then
                textOut.Append(ChrW(current))
            End If
        Next
        Return textOut.ToString()
    End Function

    Public Sub CompareFeedItems(ByRef oInstanceXML As XmlDocument)
        Try
            ' Dim oAdmXFrm As New Protean.Cms.Admin.AdminXforms
            ' oAdmXFrm.open(New XmlDocument)
            Dim cContentType As String = oInstanceXML.DocumentElement.SelectSingleNode("instance[1]/tblContent/cContentSchemaName").InnerText

            Dim cSQL As String

            If cContentType = "FeedItem" Then
                cSQL = "SELECT tblContent.* ,0 AS InFeed FROM tblContent INNER JOIN tblContentLocation ON tblContent.nContentKey = tblContentLocation.nContentId WHERE (cContentXmlBrief LIKE '%<url>" & cFeedURL & "</url>%') AND (cContentSchemaName = 'FeedItem')  AND (tblContentLocation.nStructId = " & nHostPageID & ")"
            Else
                cSQL = "SELECT tblContent.* ,0 AS InFeed FROM tblContent WHERE (cContentXmlBrief LIKE '%<url>" & cFeedURL & "</url>%') AND (cContentForiegnRef <> '') AND (cContentSchemaName = '" & cContentType & "')"
            End If

            Dim oDS As DataSet = oDBH.GetDataSet(cSQL, "Items")
            Dim oInstanceElmt As XmlElement
            Dim oDR As DataRow
            Dim nContentKey As Integer

            For Each oInstanceElmt In oInstanceXML.DocumentElement.SelectNodes("instance")
                Dim cId As String = oInstanceElmt.SelectSingleNode("tblContent/cContentForiegnRef").InnerText
                oInstanceElmt.SelectSingleNode("tblContent/cContentName").InnerText = Text.CleanName(oInstanceElmt.SelectSingleNode("tblContent/cContentName").InnerText, True)
                ' If there's no foreign ref then let's use the contentname
                If cId = "" Then
                    cId = oInstanceElmt.SelectSingleNode("tblContent/cContentName").InnerText
                    cId = cId.Substring(0, Math.Min(50, cId.Length))
                    oInstanceElmt.SelectSingleNode("tblContent/cContentForiegnRef").InnerText = cId
                End If
                nContentKey = 0
                ' nContentKey = oDBH.getContentByRef(cId)

                ' Try to find the item in the existing items

                If oDS.Tables.Count > 0 Then
                    For Each oDR In oDS.Tables("Items").Rows
                        Debug.WriteLine("'" & oDR("cContentForiegnRef") & "' = '" & cId & "' = (" & IIf(oDR("cContentForiegnRef") = cId, "True", "False") & ")")
                        If oDR("cContentForiegnRef") = cId Then
                            oDR("InFeed") = 1
                            nContentKey = oDR("nContentKey")
                            Exit For
                        End If
                    Next
                End If


                ' If the item was not found, then add it.
                If nContentKey = 0 Then

                    Me.AddExternalMessage("Adding Item", cId)
                    _counters("add").Add()
                    oAdmXFrm.xFrmFeedItem(, oInstanceElmt, nHostPageID, cFeedURL)

                ElseIf nContentKey > 0 And UpdateExistingItems Then
                    ' If found, and update is flagged, then update it.


                    Dim oNewElmt As XmlElement = oInstanceXML.CreateElement("instance")
                    oNewElmt.InnerXml = oInstanceElmt.InnerXml

                    If oNewElmt.SelectSingleNode("//nContentKey") IsNot Nothing Then oNewElmt.SelectSingleNode("//nContentKey").InnerText = nContentKey
                    If oNewElmt.SelectSingleNode("//nContentPrimaryId") IsNot Nothing Then oNewElmt.SelectSingleNode("//nContentPrimaryId").InnerText = "0"
                    If oNewElmt.SelectSingleNode("//nAuditId") IsNot Nothing Then oNewElmt.SelectSingleNode("//nAuditId").ParentNode.RemoveChild(oNewElmt.SelectSingleNode("//nAuditId"))

                    Dim oXfrm As XmlElement = oAdmXFrm.xFrmFeedItem(nContentKey, oNewElmt, 0, cFeedURL)

                    If oXfrm.GetAttribute("itemupdated") = "true" Then
                        Me.AddExternalMessage("Updating Item", cId)
                        _counters("update").Add()
                    Else
                        Me.AddExternalMessage("Not Updated", cId)
                        _counters("notupdated").Add()
                    End If

                End If
                _counters("total").Add()
            Next

            'now we need to look at the old ones
            'if they want them manually removed then we leave it
            If nSave = SaveMode.Archive Or nSave = SaveMode.Delete Then
                If oDS.Tables.Count > 0 Then
                    For Each oDR In oDS.Tables("Items").Rows
                        If oDR("InFeed") = 0 Then
                            Select Case nSave
                                Case SaveMode.Delete
                                    Me.AddExternalMessage("Deleteing Item", oDR("cContentForiegnRef").ToString)
                                    _counters("delete").Add()
                                    oDBH.DeleteObject(Cms.dbHelper.objectTypes.Content, oDR("nContentKey"))
                                Case SaveMode.Archive
                                    Me.AddExternalMessage("Archiving Item", oDR("cContentForiegnRef").ToString)
                                    _counters("archive").Add()
                                    ArchiveFeed(oDR("nContentKey"))
                            End Select
                        End If
                    Next
                End If
            End If
        Catch ex As Exception
            AddExternalError(ex)
        End Try
    End Sub

    Sub ArchiveFeed(ByVal nFeedId As Integer)
        Try
            Dim cSQL As String = "UPDATE a SET a.dExpireDate =" & Protean.Tools.Database.SqlDate(Now, True) & ", a.nStatus=0"
            cSQL &= " from tblAudit as a"
            cSQL &= " INNER JOIN tblContent as c ON a.nAuditKey = c.nAuditId"
            cSQL &= " WHERE c.nContentKey = " & nFeedId & ""

            oDBH.ExeProcessSql(cSQL)

        Catch ex As Exception
            AddExternalError(ex)
        End Try
    End Sub

    Sub AddExternalMessage(ByVal cMessage As String, Optional ByVal id As String = "")
        If Not oResultElmt Is Nothing Then
            Dim oElmt As XmlElement = oResultElmt.OwnerDocument.CreateElement("FeedMessage")
            Try
                oElmt.InnerXml = cMessage
            Catch ex As Exception
                oElmt.InnerText = Replace(Replace(cMessage, "&gt;", ">"), "&lt;", "<")
            End Try
            If Not String.IsNullOrEmpty(id) Then oElmt.SetAttribute("id", id)
            oResultElmt.AppendChild(oElmt)
        End If
    End Sub

    Sub AddExternalError(ByVal ex As Exception)
        AddExternalMessage(ex.ToString & ex.StackTrace.ToString)
        bResult = False
    End Sub





End Class
