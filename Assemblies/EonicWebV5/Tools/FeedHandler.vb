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
Imports Eonic.Tools
Imports System


Public Class FeedHandler

    Public cFeedURL As String 'placeholder values so we dont have to keep parsing them to all the subs
    Public cXSLTransformPath As String
    Public nHostPageID As Integer
    Public nSave As SaveMode

    Public bResult As Boolean = True
    Public oResultElmt As XmlElement
    Public TotalsElmt As XmlElement

    Public oDBH As Web.dbHelper
    Public oTransform As Eonic.XmlHelper.Transform
    Public oAdmXFrm As New Web.Admin.AdminXforms()

    Dim oConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("eonic/web")

    Private _countertypes As String() = {"add", "update", "delete", "archive", "total", "notupdated"}
    Private _counters As CounterCollection
    Private _updateExistingItems As Boolean


    Public Enum SaveMode
        Delete = 0
        Archive = 1
        Manual = 2
    End Enum


    Public Sub New(ByVal cURL As String, ByVal cXSLPath As String, ByVal nPageId As Long, ByVal nSaveMode As Integer, Optional ByRef oResultRecorderElmt As XmlElement = Nothing)
        PerfMon.Log("FeedHandler", "New")
        Try
            oDBH = New Web.dbHelper("Data Source=" & oConfig("DatabaseServer") & "; " &
            "Initial Catalog=" & oConfig("DatabaseName") & "; " &
            oConfig("DatabaseAuth"), 1)
            oDBH.myWeb = New Eonic.Web(System.Web.HttpContext.Current)
            oAdmXFrm.goConfig = oConfig
            oAdmXFrm.moDbHelper = oDBH
            oAdmXFrm.myWeb = oDBH.myWeb
            oAdmXFrm.myWeb.InitializeVariables()
            'set the main values
            cFeedURL = Replace(cURL, "&amp;", "&") 'when saving a url it can replace ampersands
            cXSLTransformPath = cXSLPath
            nHostPageID = nPageId
            nSave = nSaveMode
            oResultElmt = oResultRecorderElmt
            TotalsElmt = Xml.addElement(oResultElmt, "Totals")
            _updateExistingItems = True
            _counters = New CounterCollection()
            InitialiseCounters()
            oTransform = New Eonic.XmlHelper.Transform(oDBH.myWeb, cXSLTransformPath, False)
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
            Dim oInstanceXML As XmlDocument = GetFeedItems()
            If Not oInstanceXML Is Nothing Then
                'sort the guids so we have something to compare
                'UpdateGuids(oInstanceXML)
                'now we need to compare them to existing feed items on the page
                'and depending on the save mode, ignore/delete
                'we wont overwrite details in case the admin has edited some text

                If LCase(oConfig("Debug")) = "on" Then
                    oInstanceXML.Save(goServer.MapPath("/parsedFeed.xml"))
                End If

                If LCase(oConfig("FeedMode")) = "import" Then
                    Me.AddExternalMessage(oDBH.importObjects(oInstanceXML.DocumentElement, cFeedURL, cXSLTransformPath))
                Else
                    CompareFeedItems(oInstanceXML)
                End If

            Else
                Me.AddExternalMessage("No Feed Items")
            End If
            If Not msException = "" Then
                bResult = False
                AddExternalMessage(msException)
            End If
            Return bResult
        Catch ex As Exception
            AddExternalError(ex)
        End Try
    End Function

    Function GetFeedItems() As XmlDocument
        Dim oFeedXML As String = ""
        Try
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

            oResponse = DirectCast(oRequest.GetResponse(), HttpWebResponse)
            oReader = New StreamReader(oResponse.GetResponseStream())
            oFeedXML = oReader.ReadToEnd

            ' The problem with masking namespaces is that you have to deal with any node that calls that namespace.
            'oFeedXML = Replace(oFeedXML, "xmlns:", "exemelnamespace")
            'oFeedXML = Replace(oFeedXML, "xmlns", "exemelnamespace")
            oFeedXML = Regex.Replace(oFeedXML, "&gt;", ">")
            oFeedXML = Regex.Replace(oFeedXML, "&lt;", "<")

            oFeedXML = xmlTools.convertEntitiesToCodes(oFeedXML)

            Dim oResXML As New XmlDocument
            oResXML.InnerXml = oFeedXML
            If LCase(oConfig("Debug")) = "on" Then
                File.WriteAllText(goServer.MapPath("/recivedFeedRaw.xml"), oResXML.OuterXml)
            End If
            'now get the feed into out format
            Dim cFeedItemXML As String
            Dim oTW As IO.TextWriter = New StringWriter()
            Dim oTR As IO.TextReader

            oTransform.XSLFile = cXSLTransformPath
            oTransform.Compiled = False
            oTransform.Process(oResXML, oTW)
            oTR = New StringReader(oTW.ToString())
            cFeedItemXML = oTR.ReadToEnd

            Dim oInstanceXML As New XmlDocument

            ' Strip out the xmlns
            cFeedItemXML = Regex.Replace(cFeedItemXML, "xmlns(\:\w*)?=""[^""]*""", "")

            ' Fix Number Entities
            cFeedItemXML = cFeedItemXML.Replace("&amp;#", "&#")

            cFeedItemXML = cFeedItemXML.Replace("&amp;", "&")
            cFeedItemXML = cFeedItemXML.Replace("&", "&amp;")
            'Fix any missing &amp;
            ' cFeedItemXML = Regex.Replace(cFeedItemXML, "/&(?!amp;)/", "&amp;")


            oInstanceXML.InnerXml = cFeedItemXML

            ' Populate empty url nodes
            For Each oUrlNode As XmlElement In oInstanceXML.SelectNodes("//url[.='']")
                oUrlNode.InnerText = cFeedURL
            Next

            ' If the Body has been cast as CData then the html will not have been converted
            Dim sContent As String
            For Each oBodyItem As XmlElement In oInstanceXML.SelectNodes("//*[(local-name()='Body' and not(@htmlTransform='off')) or @htmlTransform='on']")
                sContent = oBodyItem.InnerText
                If sContent <> "" Then
                    Try
                        oBodyItem.InnerXml = sContent
                        oBodyItem.SetAttribute("htmlTransform", "innertext-innerxml")
                    Catch
                        oBodyItem.InnerXml = Eonic.tidyXhtmlFrag(sContent)
                        oBodyItem.SetAttribute("htmlTransform", "tidyXhtml")
                    End Try
                End If
            Next

            Return oInstanceXML

        Catch ex As Exception
            AddExternalError(ex)
            If oFeedXML <> "" Then AddExternalMessage(oFeedXML)
            Return Nothing
        End Try
    End Function


    Public Sub CompareFeedItems(ByRef oInstanceXML As XmlDocument)
        Try
            ' Dim oAdmXFrm As New Eonic.Web.Admin.AdminXforms
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
                oInstanceElmt.SelectSingleNode("tblContent/cContentName").InnerText = CleanName(oInstanceElmt.SelectSingleNode("tblContent/cContentName").InnerText, True)
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
                                    oDBH.DeleteObject(Web.dbHelper.objectTypes.Content, oDR("nContentKey"))
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
            Dim cSQL As String = "UPDATE a SET a.dExpireDate =" & Eonic.Tools.Database.SqlDate(Now, True) & ", a.nStatus=0"
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
        AddExternalMessage(ex.ToString)
        bResult = False
    End Sub





End Class
