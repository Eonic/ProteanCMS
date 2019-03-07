Imports System.Xml
Imports System.Data.SqlClient
Imports System

Partial Public Class Cms
    Partial Class Membership
        Public Class Alerts

#Region "Declarations"
            Private WithEvents myWeb As Protean.Cms

            Private WithEvents oMember As AlertMember
            Private oAlertItems As New Hashtable 'List of Alerts
            Private oAlertUsers As New Hashtable 'List of Users
            'alert config section
            Private oAlertConfig As System.Collections.Specialized.NameValueCollection = System.Web.Configuration.WebConfigurationManager.GetWebApplicationSection("protean/alerts")
            Public Shadows Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)

            Public oNewAlert As Object

            Private Sub OnAlertError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
                RaiseEvent OnError(Me, e)
            End Sub

#End Region


#Region "Public Procedures"


            Public Sub New(ByVal aWeb As Protean.Cms)
                myWeb = aWeb
                ' Note we are adding error handling to myWeb, so that it can be picked up by the Service handler
                AddHandler myWeb.OnError, AddressOf OnAlertError
            End Sub

            Public Function CurrentAlerts(ByRef bResponse As Boolean) As XmlElement
                Return CurrentAlerts(bResponse, True)
            End Function

            Public Function CurrentAlerts(ByRef bResponse As Boolean, ByVal bReportDeep As Boolean) As XmlElement
                Dim cInfo As String = ""
                Dim oResponseXML As New XmlDocument
                oResponseXML.AppendChild(oResponseXML.CreateElement("Response"))
                Try
                    cInfo = "Date"
                    Dim dTimeNow As Date = Now 'Constant Time
                    cInfo = "Build"
                    myWeb.BuildPageXML()
                    'myWeb.moPageXml = New XmlDocument
                    'myWeb.moPageXml.AppendChild(myWeb.moPageXml.CreateElement("Page"))
                    'Gets a list of all the alerts and checks which need to run
                    Dim cSQL As String = "SELECT DISTINCT tblAlerts.nAlertKey, tblAlerts.nDirId, tblAlerts.nFrequency, ((SELECT TOP 1 dDateTime FROM tblActivityLog WHERE nOtherId = tblAlerts.nAlertKey ORDER BY dDateTime DESC)) AS dDateTime, tblDirectory.cDirSchema FROM tblAlerts LEFT OUTER JOIN tblDirectory ON tblAlerts.nDirId = tblDirectory.nDirKey LEFT OUTER JOIN tblAudit ON tblAlerts.nAuditId = tblAudit.nAuditKey WHERE (tblAudit.nStatus = 1) AND (tblAudit.dPublishDate <= " & Protean.Tools.Database.SqlDate(dTimeNow, True) & " OR tblAudit.dPublishDate IS NULL) AND (tblAudit.dExpireDate >= " & Protean.Tools.Database.SqlDate(dTimeNow, True) & " OR tblAudit.dExpireDate IS NULL) AND  (tblAlerts.nAlertParent IS NULL)"
                    cInfo = "Get oDR"
                    Dim oDR As SqlDataReader = myWeb.moDbHelper.getDataReader(cSQL)
                    Do While oDR.Read
                        cInfo = "Due Next"
                        Dim dNextDue As Date = Nothing
                        'work out the date next due
                        cInfo = "Check Date"
                        If Not IsDBNull(oDR(3)) Then dNextDue = CDate(oDR(3)).AddMinutes(oDR(2))
                        'if its due or not been run then run it
                        'add the alerts/users/groups to the lists
                        cInfo = "Check Due"
                        If dNextDue = Nothing Or dNextDue <= dTimeNow Then
                            'Add a new alert Item
                            cInfo = "New Alert"
                            'oNewAlert = New AlertItem(myWeb, oDR(0), oDR(1))
                            oNewAlert = CreateAlertItem(oDR(0), oDR(1))

                            ' Log the alert
                            oNewAlert.Log("Started")

                            cInfo = "Pop Alert"
                            oNewAlert.Populate()
                            cInfo = "Add To AlertItems"
                            oAlertItems.Add("A" & oDR(0), oNewAlert)

                            'Go through the dir item to get the user id
                            'if its a group we iterate down through the groups
                            'to get all the users underneath
                            Dim oDSUsers As New DataSet
                            cInfo = "CheckGroup"
                            If LCase(oDR(4)) = "group" Then
                                cInfo = "Is Group"
                                myWeb.moDbHelper.addTableToDataSet(oDSUsers, "EXEC sp_AllDirUsers " & oDR(1) & ", " & Protean.Tools.Database.SqlDate(dTimeNow, True), "Users")
                            Else
                                cInfo = "Is User"
                                cSQL = "SELECT tblDirectory.nDirKey, tblDirectory.cDirSchema, tblDirectory.cDirForiegnRef, tblDirectory.cDirName, tblDirectory.cDirXml "
                                cSQL &= " FROM tblDirectory INNER JOIN tblAudit ON tblDirectory.nAuditId = tblAudit.nAuditKey"
                                cSQL &= " WHERE (tblDirectory.nDirKey = " & oDR(1) & ") AND (tblAudit.dPublishDate >= " & Protean.Tools.Database.SqlDate(dTimeNow, True) & " OR"
                                cSQL &= " tblAudit.dPublishDate IS NULL) AND (tblAudit.dExpireDate <= " & Protean.Tools.Database.SqlDate(dTimeNow, True) & " OR"
                                cSQL &= " tblAudit.dExpireDate IS NULL)"
                                myWeb.moDbHelper.addTableToDataSet(oDSUsers, cSQL, "Users")
                            End If
                            For Each oUserRow As DataRow In oDSUsers.Tables("Users").Rows
                                'add this to the user list
                                cInfo = "AddMember"
                                AddMember(oUserRow("nDirKey"), oUserRow("cDirXml"), oDR(0))
                            Next
                        End If
                    Loop




                    'Loop through all the users
                    'so we can send individual emails.
                    'we already have the content required for each alert
                    'so we just need to add that for the user into one email
                    If bReportDeep Then
                        Dim oElmt As XmlElement = oResponseXML.CreateElement("Alerts")
                        oElmt.SetAttribute("Count", oAlertItems.Count)
                        oResponseXML.DocumentElement.AppendChild(oElmt)
                    End If
                    If bReportDeep Then
                        Dim oElmt As XmlElement = oResponseXML.CreateElement("Members")
                        oElmt.SetAttribute("Count", oAlertUsers.Count)
                        oResponseXML.DocumentElement.AppendChild(oElmt)
                    End If
                    cInfo = "Each User"

                    Dim nEmailCount As Integer = 0
                    Dim nEmailSuccess As Integer = 0

                    ' Note on the error handling / exception catching.
                    ' ================================================
                    ' The excessive layers of exception handling here are designed to 
                    ' carry on trying with the next users if the e-mailing goes tits up.
                    ' What we need to do is hit the point where the Alert is logged, or else  
                    ' we risk sending users repeated content.


                    Dim cStatus As String = "Alert Done"
                    Try
                        For Each cUserKey As String In oAlertUsers.Keys
                            cInfo = "GetMember"
                            Dim oMember As AlertMember = oAlertUsers(cUserKey)
                            cInfo = "Create Alert Elmt"
                            Dim oAlertElmt As XmlElement = myWeb.moPageXml.CreateElement("Alerts")
                            cInfo = "Set alertElmt " & cUserKey & " xml:" & oMember.MemberXML
                            oAlertElmt.InnerXml = oMember.MemberXML
                            Dim cAlertTitles As String = ""
                            For i As Integer = 0 To oMember.Alerts.Count - 1
                                cInfo = "Each MemberAlert"
                                Dim oAlert As AlertItem = oAlertItems("A" & oMember.Alerts(i))
                                cInfo &= "1"
                                If Not oAlert.ContentElement Is Nothing Then oAlertElmt.AppendChild(oAlert.ContentElement)
                                cInfo &= "2"
                                If Not cAlertTitles = "" Then cAlertTitles &= ", "
                                cInfo &= "3"
                                cAlertTitles &= oAlert.AlertTitle
                            Next
                            'Now to send it
                            cInfo = "Send It"
                            If oAlertElmt.SelectNodes("Contents/Content").Count > 0 Then
                                nEmailCount += 1
                                Try
                                    Dim oMailer As New Protean.Messaging
                                    Dim cResponse As String = oMailer.emailer(oAlertElmt, oAlertConfig("AlertXsl"), oAlertConfig("AlertFrom"), oAlertConfig("AlertFromEmail"), oMember.Email, cAlertTitles, , , , , , , , oAlertConfig("AlertPickupHost"), oAlertConfig("AlertPickupLocation"))
                                    msException = "" ' Clear the sodding error
                                    If bReportDeep Then
                                        Dim oElmt As XmlElement = oResponseXML.CreateElement("Email")
                                        oElmt.SetAttribute("Address", oMember.Email)
                                        oElmt.InnerText = cResponse & " (" & oMember.Email & ")"
                                        oResponseXML.DocumentElement.AppendChild(oElmt)
                                    End If
                                    If cResponse.Contains("Message Sent") Then
                                        'success
                                        nEmailSuccess += 1
                                        cInfo = "Sent Fine"
                                    Else
                                        'failed
                                        cInfo = "Not Sent"
                                    End If
                                Catch ex As Exception
                                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "CurrentAlerts", ex, cInfo & ". Problem sending email"))
                                    cStatus = "ErrorEncountered"
                                End Try
                            End If
                        Next
                    Catch ex As Exception
                        RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "CurrentAlerts", ex, cInfo & ". Problem iterating through users"))
                        cStatus = "ErrorEncountered"

                    End Try


                    If nEmailCount > 0 And nEmailCount <> nEmailSuccess Then

                        ' Emails have been sent but not all were successful
                        bResponse = False

                        If Not (cStatus.Contains("Error")) Then cStatus = "Not All Sent"

                        ' Add a response.
                        Dim cMessage As String = "Alert sending was incomplete due to problems sending e-mail (" & nEmailSuccess & " out of " & nEmailCount & " email(s) were sent successfully)."
                        Tools.Xml.addElement(oResponseXML.DocumentElement, "Message", cMessage, , True)

                        ' Raise an Error
                        RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "CurrentAlerts", New Exception(cMessage), cInfo))

                    Else
                        bResponse = True
                    End If


                    ' Log the alerts
                    For Each cKey As String In oAlertItems.Keys
                        cInfo = "Mark Done"
                        Dim oAlert As AlertItem = oAlertItems(cKey)
                        oAlert.Log(cStatus)
                    Next

                Catch ex As Exception
                    bResponse = False
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "CurrentAlerts", ex, cInfo))
                End Try
                Return oResponseXML.DocumentElement
            End Function

            Public Overridable Function CreateAlertItem(ByVal AlertId As Integer, Optional ByVal AlertDirId As Integer = 0) As Object
                Dim oAlert As AlertItem = New AlertItem(myWeb, AlertId, AlertDirId)
                AddHandler oAlert.OnError, AddressOf _OnError
                Return oAlert
            End Function

#End Region



#Region "Private Procedures"

            Private Sub AddMember(ByVal nUserId As Integer, ByVal DirXml As String, ByVal nAlertId As Integer)
                Try
                    If Not oAlertUsers.ContainsKey("u" & nUserId) Then
                        oMember = New AlertMember(nUserId, DirXml)

                        AddHandler oMember.OnError, AddressOf _OnError
                        oMember.AddAlert(nAlertId)
                        oAlertUsers.Add("u" & nUserId, oMember)
                    Else
                        oMember = oAlertUsers("u" & nUserId)
                        oMember.AddAlert(nAlertId)
                        oAlertUsers("u" & nUserId) = oMember
                    End If
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "addMember", ex, ""))
                End Try
            End Sub

            Protected Sub _OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs) Handles oMember.OnError
                RaiseEvent OnError(sender, e)
            End Sub

#End Region

            Public Class AlertItem

#Region "Declarations"

                Private myWeb As Protean.Cms
                'Private dTimeNow As Date = Now

                Protected Friend nAlertKey As Integer
                Protected Friend nAlertLogKey As Integer
                Protected Friend cAlertTitle As String
                Protected Friend nPageId As Integer
                Protected Friend cPageIds As String
                Protected Friend nFrequency As Integer
                Protected Friend cContentTypes As String
                Protected Friend cXsltFile As String
                Protected Friend bUpdatedOnly As Boolean
                Protected Friend bItterateDown As Boolean
                Protected Friend bRelatedContentUpdates As Boolean
                Protected Friend cExtraXml As String
                Protected Friend oExtraXML As XmlElement
                Protected Friend dLastDone As Date
                Protected Friend oContentElmt As XmlElement
                Protected Friend bTransformed As Boolean
                Protected Friend nAlertDirId As Integer

                Private Const mcModuleName As String = "Protean.Cms.Membership.Alerts.AlertItem"

                Public Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)

#End Region

#Region "Properties"

                Private Property AlertLogKey() As Integer
                    Get
                        Return nAlertLogKey
                    End Get
                    Set(ByVal value As Integer)
                        nAlertLogKey = value
                    End Set
                End Property

                Public ReadOnly Property AlertKey() As Integer
                    Get
                        Return nAlertKey
                    End Get
                End Property

                Public ReadOnly Property AlertTitle() As String
                    Get
                        Return cAlertTitle
                    End Get
                End Property


                Public ReadOnly Property PageId() As Integer
                    Get
                        Return nPageId
                    End Get
                End Property

                Public ReadOnly Property PageIds() As String
                    Get
                        Return cPageIds
                    End Get
                End Property

                Public ReadOnly Property FrequencyMinutes() As Integer
                    Get
                        Return nFrequency
                    End Get
                End Property

                Public ReadOnly Property LastDone() As Date
                    Get
                        Return dLastDone
                    End Get
                End Property

                Public ReadOnly Property NextDue() As Date
                    Get
                        Return dLastDone.AddMinutes(nFrequency)
                    End Get
                End Property

                Public ReadOnly Property ContentTypes() As String
                    Get
                        Return cContentTypes
                    End Get
                End Property

                Public ReadOnly Property XsltFile() As String
                    Get
                        Return cXsltFile
                    End Get
                End Property

                Public ReadOnly Property UpdatedOnly() As Boolean
                    Get
                        Return bUpdatedOnly
                    End Get
                End Property

                Public ReadOnly Property ItterateDown() As Boolean
                    Get
                        Return bItterateDown
                    End Get
                End Property

                Public ReadOnly Property RelatedContentUpdates() As Boolean
                    Get
                        Return bRelatedContentUpdates
                    End Get
                End Property

                Public ReadOnly Property ExtraXml() As XmlElement
                    Get
                        If cExtraXml = "" Then Return Nothing
                        If oExtraXML Is Nothing Then
                            Dim oXML As New XmlDocument
                            oXML.LoadXml(cExtraXml)
                            oExtraXML = oXML.DocumentElement
                        End If
                        Return oExtraXML
                    End Get
                End Property

                Public Overridable ReadOnly Property ContentElement() As XmlElement
                    Get
                        If oContentElmt.HasChildNodes Then
                            Return oContentElmt
                        Else
                            Return Nothing
                        End If
                    End Get
                End Property

                Public ReadOnly Property Transformed() As Boolean
                    Get
                        Return bTransformed
                    End Get
                End Property

                Private Property AlertDirId() As Integer
                    Get
                        Return nAlertDirId
                    End Get
                    Set(ByVal value As Integer)
                        nAlertDirId = value
                    End Set
                End Property

#End Region

#Region "Public Procedures"
                Public Sub New(ByRef aWeb As Protean.Cms, ByVal AlertId As Integer, Optional ByVal AlertDirId As Integer = 0)
                    myWeb = aWeb
                    nAlertKey = AlertId
                    Me.AlertDirId = AlertDirId
                End Sub

                Public Sub Log(Optional ByVal cStatus As String = "No Status")
                    Dim cSql As String = ""

                    Try
                        ' Test if this has been logged
                        If IsNumeric(Me.AlertLogKey) AndAlso Me.AlertLogKey > 0 Then
                            ' Alert Item has been logged, therefore update the record

                            cSql = "UPDATE tblActivityLog "
                            cSql &= "SET cActivityDetail = '" & SqlFmt(Left(cStatus, 800)) & "' "
                            cSql &= "WHERE nActivityKey = " & Me.AlertLogKey
                            myWeb.moDbHelper.ExeProcessSql(cSql)
                        Else
                            ' Alert Item has been not been logged, therefore insert the record

                            cSql = "INSERT INTO tblActivityLog ( nUserDirId, nStructId, nArtId, nOtherId, dDateTime, nActivityType, cActivityDetail, cSessionId) VALUES ("
                            cSql &= "1,"
                            cSql &= "0,"
                            cSql &= "0,"
                            cSql &= nAlertKey & ","
                            cSql &= Protean.Tools.Database.SqlDate(Now, True) & ","
                            cSql &= Protean.Cms.dbHelper.ActivityType.Alert & ","
                            cSql &= "'" & SqlFmt(Left(cStatus, 800)) & "',"
                            cSql &= "'')"

                            ' Changed this from ExeProcessSqlOrIgnore to ExeProcessSql as for user alerts
                            ' this step is essential, and should raise an error if it fails.
                            Me.AlertLogKey = myWeb.moDbHelper.GetIdInsertSql(cSql)
                        End If

                    Catch ex As Exception
                        RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Log", ex, cSql))
                    End Try
                End Sub


                Public Overridable Sub Populate()
                    Try
                        Dim cSQL As String = "SELECT * FROM tblAlerts WHERE nAlertKey = " & nAlertKey & " OR nAlertParent = " & nAlertKey
                        Dim oDS As DataSet = myWeb.moDbHelper.GetDataSet(cSQL, "tblAlerts")

                        For Each oRow As DataRow In oDS.Tables("tblAlerts").Rows
                            If Not CellValue(oRow("cAlertTitle"), "") = "" Then cAlertTitle = oRow("cAlertTitle")
                            If Not CellValue(oRow("nPageId"), 0) = 0 Then nPageId = oRow("nPageId")
                            If Not CellValue(oRow("nFrequency"), 0) = 0 Then nFrequency = oRow("nFrequency")
                            If Not CellValue(oRow("cXsltFile"), "") = "" Then cXsltFile = oRow("cXsltFile")

                            bUpdatedOnly = CellValue(oRow("bUpdatedOnly"), False)
                            bItterateDown = CellValue(oRow("bItterateDown"), False)
                            bRelatedContentUpdates = CellValue(oRow("bRelatedContentUpdates"), False)
                            If Not CellValue(oRow("cExtraXml"), "") = "" Then cAlertTitle = oRow("cExtraXml")

                            If Not CellValue(oRow("cContentType"), "") = "" Then
                                If Not cContentTypes = "" Then cContentTypes &= ","
                                cContentTypes &= "'" & oRow("cContentType") & "'"
                            End If
                            Dim cLastDone As String = myWeb.moDbHelper.GetDataValue("SELECT TOP 1 dDateTime FROM tblActivityLog WHERE (nActivityType = " & Protean.Cms.dbHelper.ActivityType.Alert & ") AND (nOtherId = " & nAlertKey & ") AND nActivityKey <> " & Me.AlertLogKey & " ORDER BY dDateTime DESC", , , "")
                            If IsDate(cLastDone) Then dLastDone = CDate(cLastDone) Else dLastDone = Now
                            ' PopulateContent(nPageId, Nothing)
                        Next
                        If Not cContentTypes = "" Then
                            PopulateContent(nPageId, Nothing)
                        End If
                    Catch ex As Exception
                        RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Populate", ex, ""))
                    End Try
                End Sub

                Public Overridable Sub PopulateContent(ByVal nCurrentPageId As Integer, ByVal oCurrElmt As XmlElement)
                    Try
                        Dim cWhere As String = " (cContentSchemaName IN (" & cContentTypes & "))"
                        'Only updatead?
                        If bUpdatedOnly Then cWhere &= " AND (dInsertDate >= " & Protean.Tools.Database.SqlDate(dLastDone, True) & "  OR dUpdateDate>= " & Protean.Tools.Database.SqlDate(dLastDone, True) & ")"
                        'Get The Root Item Content
                        cWhere &= " AND CL.nStructId = " & nCurrentPageId
                        'Add it to the Content
                        If oContentElmt Is Nothing Then
                            oContentElmt = myWeb.moPageXml.CreateElement("Contents")
                        End If
                        oContentElmt.SetAttribute("title", cAlertTitle)
                        Dim oCont As XmlElement = myWeb.moPageXml.DocumentElement.SelectSingleNode("Contents")
                        If Not oCont Is Nothing Then oCont.InnerXml = ""
                        myWeb.GetPageContentFromSelect(cWhere)
                        oCont = myWeb.moPageXml.DocumentElement.SelectSingleNode("Contents")
                        oContentElmt.InnerXml &= oCont.InnerXml
                        'Child Items
                        If bItterateDown Then
                            If oCurrElmt Is Nothing Then
                                Dim oMainElmt As XmlElement = myWeb.GetStructureXML
                                oCurrElmt = oCurrElmt.SelectSingleNode("descendant-or-self::MenuItem[@id=" & nCurrentPageId & "]")
                            End If
                            For Each oChild As XmlElement In oCurrElmt.SelectNodes("MenuItem")
                                PopulateContent(oChild.GetAttribute("id"), oChild)
                            Next
                        End If
                    Catch ex As Exception
                        RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Populatecontent", ex, ""))
                    End Try
                End Sub
#End Region

#Region "Private Procedures"


                Private Function CellValue(ByVal obj As Object, ByVal nullValue As Object) As Object
                    If IsDBNull(obj) Then Return nullValue Else Return obj
                End Function


#End Region

            End Class

            Private Class AlertMember
#Region "Declarations"
                Private nUserId As Integer
                Private cEmail As String
                Private oAlerts As New Hashtable
                Private cDirXML As String
                Private Const mcModuleName As String = "Protean.Cms.Membership.Alerts.Member"

                Public Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
#End Region

#Region "Properties"
                Public ReadOnly Property Email() As String
                    Get
                        Return cEmail
                    End Get
                End Property

                Public ReadOnly Property Alerts() As Hashtable
                    Get
                        Return oAlerts
                    End Get
                End Property

                Public ReadOnly Property MemberXML() As String
                    Get
                        Return cDirXML
                    End Get
                End Property
#End Region

                Public Sub New(ByVal UserId As Integer, ByVal DirXml As String)
                    nUserId = UserId
                    cDirXML = DirXml
                    GetEmail()
                End Sub

                Public Sub AddAlert(ByVal nAlertId As Integer)
                    Try
                        If Not oAlerts.ContainsValue(nAlertId) Then
                            oAlerts.Add(oAlerts.Count, nAlertId)
                        End If
                    Catch ex As Exception
                        RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "AddAlert", ex, ""))
                    End Try
                End Sub

                Private Sub GetEmail()
                    Try
                        If cDirXML = "" Then Exit Sub
                        Dim oXML As New XmlDocument
                        oXML.LoadXml(cDirXML)
                        Dim oEmailelmt As XmlElement = oXML.SelectSingleNode("descendant-or-self::Email")
                        If Not oEmailelmt Is Nothing Then cEmail = oEmailelmt.InnerText
                        oEmailelmt = Nothing
                        oXML = Nothing
                    Catch ex As Exception
                        RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetEmail", ex, ""))
                    End Try
                End Sub
            End Class


        End Class
    End Class
End Class


