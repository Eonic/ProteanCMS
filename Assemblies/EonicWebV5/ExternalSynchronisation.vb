Imports System.Xml
Imports System.Web.Configuration
Imports System

Public Class ExternalSynchronisation
    Inherits Eonic.Tools.SoapClient

#Region "Declarations"


    Private moPageDetail As XmlElement
    Private moConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("eonic/web")
    Private moSyncConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("eonic/synchronisation")
    Private Const mcModuleName As String = "Web.ExternalSynchronisation"
    Private myWeb As Eonic.Web
    Private WithEvents moDBT As Eonic.Tools.Database
    Private WithEvents moTransform As Eonic.Tools.Xslt.Transform
    Private WithEvents moDBH As Eonic.Web.dbHelper
    Private WithEvents moXSLTFunctions As Xslt
    Public Shadows Event OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)

    Public cLastError As String = ""


    Private Sub _OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs) Handles moDBT.OnError, MyBase.OnError, moTransform.OnError
        RaiseEvent OnError(sender, e)
        LogError(e.ModuleName, e.ProcedureName, e.Exception, e.AddtionalInformation)
        AddExceptionToEventLog(e.Exception, e.ProcedureName)
    End Sub


    Private oLowErrors As XmlDocument

#End Region

#Region "Initialisation"
    Public Sub New(ByRef aWeb As Eonic.Web, Optional ByRef oPageDetail As XmlElement = Nothing)
        myWeb = aWeb
        moPageDetail = oPageDetail
        InitialiseVariables()
    End Sub

    Public Sub New(Optional ByRef oPageDetail As XmlElement = Nothing)
        myWeb = New Eonic.Web
        moPageDetail = oPageDetail
        InitialiseVariables()
    End Sub

    Private Sub InitialiseVariables()

        Dim cPassword As String
        Dim cUsername As String
        Dim cDBAuth As String

        MyBase.RemoveReturnSoapEnvelope = True
        MyBase.Url = moSyncConfig("URL")
        MyBase.Namespace = moSyncConfig("Namespace")

        moDBT = New Eonic.Tools.Database

        moDBT.DatabaseServer = moConfig("DatabaseServer")
        moDBT.DatabaseName = moConfig("DatabaseName")

        If Not moConfig("DatabaseUsername") Is Nothing Then
            moDBT.DatabaseUser = moConfig("DatabaseUsername")
            moDBT.DatabasePassword = moConfig("DatabasePassword")
        Else
            ' No authorisation information provided in the connection string.  
            ' We need to source it from somewhere, let's try the web.config 
            cDBAuth = GetDBAuth()

            ' Let's find the username and password
            cUsername = Eonic.Tools.Text.SimpleRegexFind(cDBAuth, "user id=([^;]*)", 1, Text.RegularExpressions.RegexOptions.IgnoreCase)
            cPassword = Eonic.Tools.Text.SimpleRegexFind(cDBAuth, "password=([^;]*)", 1, Text.RegularExpressions.RegexOptions.IgnoreCase)
            moDBT.DatabaseUser = cUsername
            moDBT.DatabasePassword = cPassword
        End If

        moTransform = New Eonic.Tools.Xslt.Transform
        moXSLTFunctions = New Xslt(myWeb)
        moTransform.XslTExtensionObject = moXSLTFunctions
        moTransform.XslTExtensionURN = "ew"

        If myWeb Is Nothing Then
            moDBH = New Eonic.Web.dbHelper(moDBT.DatabaseConnectionString, 1)
        Else
            moDBH = New Eonic.Web.dbHelper(myWeb)
        End If

    End Sub

    Public Function GetDBAuth() As String
        PerfMon.Log("dbHelper", "getDBAuth")
        Try
            Dim dbAuth As String
            If moConfig("DatabasePassword") <> "" Then
                dbAuth = "user id=" & moConfig("DatabaseUsername") & "; password=" & moConfig("DatabasePassword")
            Else
                If moConfig("DatabaseAuth") <> "" Then
                    dbAuth = moConfig("DatabaseAuth")
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
#End Region

#Region "Import"
    Public Function ImportItems(ByVal oSoapBody As String, ByVal cAction As String) As String
        Dim nNew As Integer = 0
        Dim nUpdated As Integer = 0
        Dim nSkipped As Integer = 0
        Dim cResult As String = ""
        Dim cInfo As String = "Beginning"
        Dim oFinalResultsXML As New XmlDocument
        Dim nItemCount As Integer = 0

        oFinalResultsXML.AppendChild(oFinalResultsXML.CreateElement("Results"))
        Try
            MyBase.Action = cAction

            'Get a List of items
            Dim oListXML As New XmlDocument 'List of Items
            cInfo = "Getting 1st Soap"
            oListXML.LoadXml(Eonic.Tools.Xml.convertEntitiesToCodes(MyBase.SendRequest(oSoapBody)))
            If MyBase.ErrorReturned Or Not oListXML.SelectSingleNode("descendant-or-self::Errors") Is Nothing Then Return ExternalError(oListXML.OuterXml)

            Dim oStkList As XmlElement = oListXML.SelectSingleNode("GetStockListResponse/GetStockListResult/*")

            If oStkList.GetAttribute("ReturnedRecordCount") > 0 Then
                cInfo = "1st Transform"
                moTransform.XslTFile = goServer.MapPath(moSyncConfig("ImportXSL"))
                moTransform.Xml = oListXML




                'STAGE 1
                'transform the list into something recognisable
                Dim oDBResults As New XmlDocument
                oDBResults.LoadXml(moTransform.Process)

                'This provides Table names and fields we need to search on and the ref of the foreign items
                Dim oRDBesElmt As XmlElement
                Dim oDS As DataSet = Nothing

                'Loop through each one

                For Each oRDBesElmt In oDBResults.DocumentElement
                    Dim bNew As Boolean = True 'counter

                    Dim oTableNameEnum As Eonic.Web.dbHelper.TableNames = System.Enum.Parse(GetType(Eonic.Web.dbHelper.TableNames), oRDBesElmt.Name)
                    Dim cKeyField As String = moDBH.TableKey(oTableNameEnum)
                    Dim cSQL As String = "SELECT " & cKeyField & " FROM " & oRDBesElmt.Name
                    Dim cWhere As String = ""
                    Dim oWhereElmts As XmlElement
                    For Each oWhereElmts In oRDBesElmt.ChildNodes
                        If Not cWhere = "" Then cWhere &= " AND "
                        cWhere &= " (" & oWhereElmts.Name & " = "

                        'took this out because datatype wrong
                        'If IsNumeric(oWhereElmts.InnerText) Then
                        '    cWhere &= oWhereElmts.InnerText & ")"
                        'ElseIf IsDate(oWhereElmts.InnerText) Then
                        'cWhere &= Eonic.Tools.Database.SqlDate(oWhereElmts.InnerText) & ")"
                        'Else
                        'cWhere &= "'" & Eonic.Tools.Database.SqlFmt(oWhereElmts.InnerText) & "')"
                        'End If

                        'replaced with this which decided datatype based on fieldname
                        Select Case Left(oWhereElmts.Name, 1)
                            Case "c"
                                cWhere &= "'" & Eonic.Tools.Database.SqlFmt(oWhereElmts.InnerText) & "')"
                            Case "n"
                                cWhere &= oWhereElmts.InnerText & ")"
                            Case "d"
                                cWhere &= Eonic.Tools.Database.SqlDate(oWhereElmts.InnerText) & ")"
                            Case Else
                                cWhere &= "'" & Eonic.Tools.Database.SqlFmt(oWhereElmts.InnerText) & "')"
                        End Select


                    Next
                    If Not cWhere = "" Then cWhere = " WHERE " & cWhere
                    cSQL &= cWhere

                    'get the id
                    Dim oInstanceXML As New XmlDocument
                    moDBH.moPageXml = oInstanceXML

                    Dim nId As Integer = moDBT.GetDataValue(cSQL, CommandType.Text, , 0)

                    Dim oInstance As XmlElement = oInstanceXML.CreateElement("OriginalInstance")
                    If nId = 0 Then
                        Select Case oTableNameEnum
                            Case Web.dbHelper.TableNames.tblContent 'content item
                                'we need to know what type of item it is
                                Dim cSchemaName As String = ""
                                If Not oRDBesElmt.SelectSingleNode("cContentSchemaName") Is Nothing Then
                                    cSchemaName = oRDBesElmt.SelectSingleNode("cContentSchemaName").InnerText
                                End If
                                If Not cSchemaName = "" Then
                                    Dim oXForm As New Eonic.xForm
                                    If Not oXForm.load("/xforms/content/" & cSchemaName & ".xml", myWeb.maCommonFolders) Then
                                        'cant load it
                                        GoTo SkipIt

                                    End If
                                    oInstance.InnerXml = oXForm.Instance.InnerXml
                                Else
                                    'we have no idea what type of content it is
                                    GoTo SkipIt
                                End If
                            Case Else
                                oInstance.InnerXml = moDBH.getObjectInstance(oTableNameEnum, nId)
                        End Select
                    Else
                        bNew = False
                        oInstance.InnerXml = moDBH.getObjectInstance(oTableNameEnum, nId)
                        If oInstance.InnerXml = "" Then
                            cInfo = nId & "returned no instance data"
                        End If
                    End If



                    'now to get the information from the foreign database
                    'first need to create the soap body
                    'STAGE 2
                    Dim oRefXML As New XmlDocument

                    Dim oRefElmt As XmlElement = oRefXML.CreateElement("Ref")
                    oRefElmt.InnerText = oRDBesElmt.GetAttribute("Ref")
                    moTransform.Xml = oRefElmt
                    cInfo = "2nd Transform"
                    oRefXML.LoadXml(moTransform.Process)
                    Dim cItemAction As String = oRefXML.DocumentElement.GetAttribute("Action")
                    oRefXML.DocumentElement.RemoveAttribute("Action")
                    Me.Action = cItemAction


                    Dim oForeignItem As XmlElement = oRefXML.CreateElement("ForeignInstance")
                    cInfo = "2nd Request"
                    oForeignItem.InnerXml = MyBase.SendRequest(oRefXML.OuterXml)
                    If MyBase.ErrorReturned Or Not oListXML.SelectSingleNode("descendant-or-self::Errors") Is Nothing Then Return ExternalError(oListXML.OuterXml)


                    oInstanceXML.AppendChild(oInstanceXML.CreateElement("Instances"))
                    oInstanceXML.DocumentElement.AppendChild(oInstance)
                    oInstanceXML.DocumentElement.AppendChild(oInstanceXML.ImportNode(oForeignItem, True))

                    'now to get the new instance
                    'STAGE 3
                    cInfo = "3rd Transform - id SQL" & cSQL
                    Dim oFinalInstance As New XmlDocument
                    moTransform.Xml = oInstanceXML
                    Dim cFinalInstance As String = moTransform.Process
                    'check we actually have an instance to save
                    If cFinalInstance = "" Then GoTo SkipIt
                    'else carry on
                    oFinalInstance.LoadXml(cFinalInstance)

                    'check we actually have an instance to save
                    Dim nReturnId As Integer

                    If oFinalInstance.DocumentElement Is Nothing Or oFinalInstance.DocumentElement.InnerXml = "" Then

                        GoTo SkipIt

                    Else
                        nReturnId = moDBH.setObjectInstance(oTableNameEnum, oFinalInstance.DocumentElement, nId)
                    End If


                    'now we have an id we can see if there are any other things its wants to do
                    Dim oFinishElmt As XmlElement = oFinalInstance.CreateElement("Finish")
                    oFinishElmt.SetAttribute("ID", nReturnId)
                    'lets add some instances in case they are needed
                    oFinishElmt.AppendChild(oFinalInstance.ImportNode(oForeignItem, True))
                    Dim oNewInstance As XmlElement = oFinalInstance.CreateElement("NewInstance")
                    oNewInstance.InnerXml = myWeb.moDbHelper.getObjectInstance(oTableNameEnum, nReturnId)
                    oFinishElmt.AppendChild(oNewInstance)


                    moTransform.Xml = oFinishElmt

                    'now we have finished yay
                    'STAGE 4
                    cInfo = "4th Transform"
                    Dim oResultXML As New XmlDocument
                    oResultXML.LoadXml(moTransform.Process())
                    oFinalResultsXML.DocumentElement.AppendChild(oFinalResultsXML.ImportNode(oResultXML.DocumentElement, True))
                    If bNew Then nNew += 1 Else nUpdated += 1
                    GoTo EndIt
SkipIt:
                    nSkipped += 1
EndIt:
                    oFinalResultsXML.DocumentElement.SetAttribute("New", nNew)
                    oFinalResultsXML.DocumentElement.SetAttribute("Updated", nUpdated)
                    oFinalResultsXML.DocumentElement.SetAttribute("Skipped", nSkipped)
                    nItemCount += 1

                Next
            Else
                'No Results returned
                oFinalResultsXML.DocumentElement.AppendChild(oFinalResultsXML.ImportNode(oListXML.DocumentElement.FirstChild, True))
            End If
            ReportLowErrors(oFinalResultsXML)
            Return oFinalResultsXML.OuterXml
        Catch ex As Exception
            RaiseEvent OnError(mcModuleName, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "ImportContentItems", ex, cInfo))
            'cResult = " An Error occured " & cInfo & ": " & ex.ToString & "<br/>" & cLastError
            'oFinalResultsXML.DocumentElement.InnerText = cResult
            Throw New ArgumentException("Exception")
            Return ""

        End Try


    End Function

    '    Public Function ImportItems(ByRef oInstancesXml As XmlDocument, ByVal cXsltPat As String) As String
    '        Dim nNew As Integer = 0
    '        Dim nUpdated As Integer = 0
    '        Dim nSkipped As Integer = 0
    '        Dim cResult As String = ""
    '        Dim cInfo As String = "Beginning"
    '        Dim oFinalResultsXML As New XmlDocument
    '        Dim nItemCount As Integer = 0

    '        oFinalResultsXML.AppendChild(oFinalResultsXML.CreateElement("Results"))
    '        Try

    '            For Each oInstance In oInstancesXml.DocumentElement
    '                Dim bNew As Boolean = True 'counter

    '                Dim oTableNameEnum As Eonic.Web.dbHelper.TableNames = System.Enum.Parse(GetType(Eonic.Web.dbHelper.TableNames), oRDBesElmt.Name)
    '                Dim cKeyField As String = moDBH.TableKey(oTableNameEnum)
    '                Dim cSQL As String = "SELECT " & cKeyField & " FROM " & oRDBesElmt.Name
    '                Dim cWhere As String = ""
    '                Dim oWhereElmts As XmlElement
    '                For Each oWhereElmts In oRDBesElmt.ChildNodes
    '                    If Not cWhere = "" Then cWhere &= " AND "
    '                    cWhere &= " (" & oWhereElmts.Name & " = "
    '                    If IsNumeric(oWhereElmts.InnerText) Then
    '                        cWhere &= oWhereElmts.InnerText & ")"
    '                    ElseIf IsDate(oWhereElmts.InnerText) Then
    '                        cWhere &= Eonic.Tools.Database.SqlDate(oWhereElmts.InnerText) & ")"
    '                    Else
    '                        cWhere &= "'" & Eonic.Tools.Database.SqlFmt(oWhereElmts.InnerText) & "')"
    '                    End If
    '                Next
    '                If Not cWhere = "" Then cWhere = " WHERE " & cWhere
    '                cSQL &= cWhere

    '                'get the id
    '                Dim oInstanceXML As New XmlDocument
    '                moDBH.moPageXml = oInstanceXML

    '                Dim nId As Integer = moDBT.GetDataValue(cSQL, CommandType.Text, , 0)

    '                Dim oInstance As XmlElement = oInstanceXML.CreateElement("OriginalInstance")
    '                If nId = 0 Then
    '                    Select Case oTableNameEnum
    '                        Case Web.dbHelper.TableNames.tblContent 'content item
    '                            'we need to know what type of item it is
    '                            Dim cSchemaName As String = ""
    '                            If Not oRDBesElmt.SelectSingleNode("cContentSchemaName") Is Nothing Then
    '                                cSchemaName = oRDBesElmt.SelectSingleNode("cContentSchemaName").InnerText
    '                            End If
    '                            If Not cSchemaName = "" Then
    '                                Dim oXForm As New Eonic.xForm
    '                                If Not oXForm.load("/xforms/content/" & cSchemaName & ".xml") Then
    '                                    If Not oXForm.load("/ewcommon/xforms/content/" & cSchemaName & ".xml") Then
    '                                        'cant load it
    '                                        GoTo SkipIt
    '                                    End If
    '                                End If
    '                                oInstance.InnerXml = oXForm.instance.InnerXml
    '                            Else
    '                                'we have no idea what type of content it is
    '                                GoTo SkipIt
    '                            End If
    '                        Case Else
    '                            oInstance.InnerXml = moDBH.getObjectInstance(oTableNameEnum, nId)
    '                    End Select
    '                Else
    '                    bNew = False
    '                    oInstance.InnerXml = moDBH.getObjectInstance(oTableNameEnum, nId)
    '                End If

    '                'now to get the information from the foreign database
    '                'first need to create the soap body
    '                'STAGE 2
    '                Dim oRefXML As New XmlDocument

    '                Dim oRefElmt As XmlElement = oRefXML.CreateElement("Ref")
    '                oRefElmt.InnerText = oRDBesElmt.GetAttribute("Ref")
    '                moTransform.Xml = oRefElmt
    '                cInfo = "2nd Transform"
    '                oRefXML.LoadXml(moTransform.Process)
    '                Dim cItemAction As String = oRefXML.DocumentElement.GetAttribute("Action")
    '                oRefXML.DocumentElement.RemoveAttribute("Action")
    '                Me.Action = cItemAction


    '                Dim oForeignItem As XmlElement = oRefXML.CreateElement("ForeignInstance")
    '                cInfo = "2nd Request"
    '                oForeignItem.InnerXml = MyBase.SendRequest(oRefXML.OuterXml)
    '                If MyBase.ErrorReturned Or Not oListXML.SelectSingleNode("descendant-or-self::Errors") Is Nothing Then Return ExternalError(oListXML.OuterXml)


    '                oInstanceXML.AppendChild(oInstanceXML.CreateElement("Instances"))
    '                oInstanceXML.DocumentElement.AppendChild(oInstance)
    '                oInstanceXML.DocumentElement.AppendChild(oInstanceXML.ImportNode(oForeignItem, True))

    '                'now to get the new instance
    '                'STAGE 3
    '                cInfo = "3rd Transform"
    '                Dim oFinalInstance As New XmlDocument
    '                moTransform.Xml = oInstanceXML
    '                Dim cFinalInstance As String = moTransform.Process
    '                'check we actually have an instance to save
    '                If cFinalInstance = "" Then GoTo SkipIt
    '                'else carry on
    '                oFinalInstance.LoadXml(cFinalInstance)

    '                'check we actually have an instance to save
    '                'If oFinalInstance.DocumentElement Is Nothing Then GoTo SkipIt


    '                Dim nReturnId As Integer = moDBH.setObjectInstance(oTableNameEnum, oFinalInstance.DocumentElement, nId)
    '                'now we have an id we can see if there are any other things its wants to do
    '                Dim oFinishElmt As XmlElement = oFinalInstance.CreateElement("Finish")
    '                oFinishElmt.SetAttribute("ID", nReturnId)
    '                'lets add some instances in case they are needed
    '                oFinishElmt.AppendChild(oFinalInstance.ImportNode(oForeignItem, True))
    '                Dim oNewInstance As XmlElement = oFinalInstance.CreateElement("NewInstance")
    '                oNewInstance.InnerXml = myWeb.moDbHelper.getObjectInstance(oTableNameEnum, nReturnId)
    '                oFinishElmt.AppendChild(oNewInstance)


    '                moTransform.Xml = oFinishElmt

    '                'now we have finished yay
    '                'STAGE 4
    '                cInfo = "4th Transform"
    '                Dim oResultXML As New XmlDocument
    '                oResultXML.LoadXml(moTransform.Process())
    '                oFinalResultsXML.DocumentElement.AppendChild(oFinalResultsXML.ImportNode(oResultXML.DocumentElement, True))
    '                If bNew Then nNew += 1 Else nUpdated += 1
    '                GoTo EndIt
    'SkipIt:
    '                nSkipped += 1
    'EndIt:
    '                oFinalResultsXML.DocumentElement.SetAttribute("New", nNew)
    '                oFinalResultsXML.DocumentElement.SetAttribute("Updated", nUpdated)
    '                oFinalResultsXML.DocumentElement.SetAttribute("Skipped", nSkipped)
    '                nItemCount += 1



    '            Next


    '        Catch ex As Exception
    '            RaiseEvent OnError(mcModuleName, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "ImportContentItems", ex, cInfo))
    '            cResult = " An Error occured " & cInfo & ": " & ex.ToString & "<br/>" & cLastError
    '        End Try
    '        ReportLowErrors(oFinalResultsXML)
    '        Return oFinalResultsXML.OuterXml

    '    End Function

#End Region

#Region "Export"
    Public Function ExportItems(ByVal oSoapBody As String, ByVal cAction As String) As String
        Dim cProcessInfo As String = ""
        Try
            Dim oSoapXML As New XmlDocument
            oSoapXML.LoadXml(oSoapBody)
            moTransform.XslTFile = goServer.MapPath(moSyncConfig("ExportXSL"))
            moTransform.Xml = oSoapXML
            Dim oQueryXML As New XmlDocument
            oQueryXML.LoadXml(moTransform.Process)
            Dim oDS As New DataSet(oQueryXML.DocumentElement.GetAttribute("name"))
            Dim oDT As DataTable
            Dim oDC As DataColumn
            Dim oSQLElmt As XmlElement
            'Get Tables
            For Each oSQLElmt In oQueryXML.DocumentElement.SelectNodes("Table")
                moDBT.addTableToDataSet(oDS, oSQLElmt.InnerText, oSQLElmt.GetAttribute("name"))
            Next
            'Get Relations
            For Each oSQLElmt In oQueryXML.DocumentElement.SelectNodes("Relation")
                cProcessInfo = "error linking relation " & oSQLElmt.GetAttribute("name") & " to " & oSQLElmt.GetAttribute("ParentTable") & " on " & oSQLElmt.GetAttribute("Nested") & " with " & oSQLElmt.GetAttribute("ParentColumn")
                Dim parentColumns As DataColumn() = moDBT.GetColumnArray(oDS, oSQLElmt.GetAttribute("ParentTable"), Split(oSQLElmt.GetAttribute("ParentColumn"), ","))
                Dim childColumns As DataColumn() = moDBT.GetColumnArray(oDS, oSQLElmt.GetAttribute("ChildTable"), Split(oSQLElmt.GetAttribute("ChildColumn"), ","))
                oDS.Relations.Add(oSQLElmt.GetAttribute("name"), parentColumns, childColumns, False)
                oDS.Relations(oSQLElmt.GetAttribute("name")).Nested = oSQLElmt.GetAttribute("Nested")
            Next
            cProcessInfo = ""
            'Map all columns as attributes first
            For Each oDT In oDS.Tables
                For Each oDC In oDT.Columns
                    oDC.ColumnMapping = MappingType.Attribute
                Next
            Next
            'Get Mappings
            For Each oSQLElmt In oQueryXML.DocumentElement.SelectNodes("Mapping")
                Dim oType As System.Data.MappingType = System.Enum.Parse(GetType(System.Data.MappingType), oSQLElmt.GetAttribute("Type"))
                oDS.Tables(oSQLElmt.GetAttribute("Table")).Columns(oSQLElmt.GetAttribute("Column")).ColumnMapping = oType
            Next
            'Now we have the dataset we can produce an Export XML
            'and then send it using the original saop instance
            Dim xmltemp As New XmlDocument
            xmltemp.InnerXml = oDS.GetXml
            oSoapXML.DocumentElement.FirstChild.InnerXml = Replace(Replace(oDS.GetXml, "&gt;", ">"), "&lt;", "<")
            MyBase.Action = cAction
            Dim oFinishXML As New XmlDocument
            oFinishXML.AppendChild(oFinishXML.CreateElement("Finish"))
            oFinishXML.DocumentElement.InnerXml = MyBase.SendRequest(oSoapXML.OuterXml)

            If MyBase.ErrorReturned Or Not oFinishXML.SelectSingleNode("descendant-or-self::Errors") Is Nothing Then Return ExternalError(oFinishXML.OuterXml)
            'do any tidying
            moTransform.Xml = oFinishXML
            Dim cReturn As String = Replace(moTransform.Process(), "xmlns:", "exemelnamespace")
            Return cReturn
        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "ExportItems", ex, cProcessInfo))
            Return ""
        End Try
    End Function
#End Region


#Region "Delete"
    Public Function DeleteItems(ByVal sSoapBody As String, ByVal cAction As String) As String
        Dim cContentType As String
        Dim cDeleteMode As String
        Dim oSoapBody As New XmlDocument
        Dim oDbt As New Eonic.Web.dbHelper(myWeb)

        Try
            'lets get the content type
            oSoapBody.LoadXml(sSoapBody)
            cContentType = oSoapBody.SelectSingleNode("descendant-or-self::cContentType").InnerText
            cDeleteMode = oSoapBody.SelectSingleNode("descendant-or-self::cDeleteMode").InnerText

            Dim sSql As String = "select nContentKey, cContentForiegnRef from tblContent where cContentSchemaName = " & Eonic.Tools.Database.SqlString(cContentType)

            Dim oDs As DataSet = oDbt.GetDataSet(sSql, "tblContent")
            Dim oRow As DataRow
            Dim nCount As Long = 0
            Dim nCountHidden As Long = 0
            Dim nCountNoRef As Long = 0
            Dim oRequest As New XmlDocument
            Dim oElmt As XmlElement
            oRequest.LoadXml("<CheckDeleted xmlns=""http://www.eonic.co.uk/ewcommon/Services""><StockItems><ItemList/></StockItems></CheckDeleted>")

            Dim nsmgr As XmlNamespaceManager = New XmlNamespaceManager(oRequest.NameTable)
            nsmgr.AddNamespace("ews", "http://www.eonic.co.uk/ewcommon/Services")

            If oDs.Tables("tblContent").Rows.Count = 0 Then
                Return "Nothing to Delete"
            Else
                For Each oRow In oDs.Tables("tblContent").Rows
                    Dim cFRef As String = oRow("cContentForiegnRef")
                    'CheckItemExists
                    Select Case cDeleteMode
                        Case "Delete Items Not Found"
                            If Not cFRef = "" And Not cFRef Is DBNull.Value Then
                                oElmt = oRequest.CreateElement("cStockCode")
                                oElmt.InnerText = cFRef
                                oRequest.SelectSingleNode("/ews:CheckDeleted/ews:StockItems/ews:ItemList", nsmgr).AppendChild(oElmt)
                            Else
                                ' Deleting objects with no FRef
                                oDbt.DeleteObject(Web.dbHelper.objectTypes.Content, oRow("nContentKey"))
                                'oDbt.setObjectStatus(Web.dbHelper.objectTypes.Content, Web.dbHelper.Status.Hidden, oRow("nContentKey"))
                                nCountNoRef = nCountNoRef + 1
                            End If
                        Case "Delete All"
                            oDbt.DeleteObject(Web.dbHelper.objectTypes.Content, oRow("nContentKey"))
                            nCount = nCount + 1
                    End Select
                Next
                If cDeleteMode = "Delete Items Not Found" Then
                    Dim oRecord As New XmlDocument
                    MyBase.Action = "CheckDeleted"
                    oRecord.AppendChild(oRecord.CreateElement("Result"))
                    oRecord.DocumentElement.InnerXml = MyBase.SendRequest(Replace(oRequest.OuterXml, "xmlns=""""", ""))
                    Dim nsmgr2 As XmlNamespaceManager = New XmlNamespaceManager(oRecord.NameTable)
                    nsmgr2.AddNamespace("ews", "http://www.eonic.co.uk/ewcommon/Services")
                    For Each oElmt In oRecord.SelectNodes("descendant-or-self::Delete", nsmgr2)
                        Dim nContentId As Long = oDbt.getObjectByRef(Web.dbHelper.objectTypes.Content, oElmt.InnerText)
                        oDbt.DeleteObject(Web.dbHelper.objectTypes.Content, nContentId)
                        'oDbt.setObjectStatus(Web.dbHelper.objectTypes.Content, Web.dbHelper.Status.Hidden, nContentId)
                        nCount = nCount + 1
                    Next
                    For Each oElmt In oRecord.SelectNodes("descendant-or-self::Hide", nsmgr2)
                        Dim nContentId As Long = oDbt.getObjectByRef(Web.dbHelper.objectTypes.Content, oElmt.InnerText)
                        'oDbt.DeleteObject(Web.dbHelper.objectTypes.Content, nContentId)
                        oDbt.setObjectStatus(Web.dbHelper.objectTypes.Content, Web.dbHelper.Status.Hidden, nContentId)
                        nCountHidden = nCountHidden + 1
                    Next
                    oRecord = Nothing

                    Return "<result>Deleted [" & nCount & "] Hidden [" & nCountHidden & "] foriegn items and [" & nCountNoRef & "] items with no fRef out of [" & oDs.Tables("tblContent").Rows.Count & "] Items</result>"

                Else

                    Return "<result>Deleted " & nCount & " items out of " & oDs.Tables("tblContent").Rows.Count & " Items</result>"

                End If
            End If

        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "DeleteItems", ex, ""))
            Return ""
        End Try
    End Function


#End Region


#Region "Admin"
    Public Function AdminProcess(ByVal cEwCmd2 As String) As String
        Dim oSyncItems() As String = Split(moSyncConfig("Actions"), ",")
        Dim i As Integer = 0
        Dim oXForm As New xForm


        Try
            Select Case cEwCmd2
                Case "", Nothing
                    For i = 0 To oSyncItems.Length - 1
                        Eonic.Tools.Xml.addElement(moPageDetail, "SyncAction", oSyncItems(i))
                    Next
                Case Else
                    For i = 0 To oSyncItems.Length - 1
                        If oSyncItems(i) = cEwCmd2 Then
                            oXForm.moPageXML = moPageDetail.OwnerDocument
                            oXForm.load("/xforms/synchronisation/" & cEwCmd2 & ".xml")
                            If oXForm.isSubmitted Then
                                oXForm.updateInstanceFromRequest()
                                oXForm.validate()
                                If oXForm.valid Then
                                    Dim oSoapElmt As XmlElement = oXForm.Instance.SelectSingleNode("descendant-or-self::*[@exemelnamespace and @type]")
                                    Dim cInOut As String = oSoapElmt.GetAttribute("type")
                                    oSoapElmt.RemoveAttribute("type")
                                    Select Case cInOut
                                        Case "Input"
                                            Dim oElmt As XmlElement = moPageDetail.OwnerDocument.CreateElement("Result")
                                            oElmt.SetAttribute("Action", cEwCmd2)
                                            oElmt.InnerXml = ImportItems(oSoapElmt.OuterXml, oSoapElmt.Name)
                                            moPageDetail.AppendChild(oElmt)
                                            Return "SynchronisationResults"
                                        Case "Output"
                                            Dim oElmt As XmlElement = moPageDetail.OwnerDocument.CreateElement("Result")
                                            oElmt.SetAttribute("Action", cEwCmd2)
                                            oElmt.InnerXml = ExportItems(oSoapElmt.OuterXml, oSoapElmt.Name)
                                            moPageDetail.AppendChild(oElmt)
                                            Return "SynchronisationResults"
                                        Case "Delete"
                                            Dim oElmt As XmlElement = moPageDetail.OwnerDocument.CreateElement("Result")
                                            oElmt.SetAttribute("Action", cEwCmd2)
                                            oElmt.InnerXml = DeleteItems(oSoapElmt.OuterXml, oSoapElmt.Name)
                                            moPageDetail.AppendChild(oElmt)
                                            Return "SynchronisationResults"
                                    End Select
                                Else
                                    oXForm.addValues()
                                    moPageDetail.AppendChild(oXForm.moXformElmt)
                                    Return "SynchronisationXForm"
                                End If
                            Else
                                oXForm.addValues()
                                moPageDetail.AppendChild(oXForm.moXformElmt)
                                Return "SynchronisationXForm"
                            End If
                        End If
                    Next
            End Select
            Return "Synchronisation"
        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "AdminProcess", ex, ""))
            Return ""
        End Try
    End Function


#End Region


#Region "Private Procedures"

    Private Function ExternalError(ByVal cErrorXML As String) As String
        Try
            Dim oErrorXML As New XmlDocument
            oErrorXML.LoadXml(cErrorXML)
            Dim cReturn As String = ""
            Dim oElmt As XmlElement
            For Each oElmt In oErrorXML.DocumentElement.FirstChild.FirstChild.ChildNodes
                cReturn &= "<h3>Error:</h3><br/>"
                cReturn &= "<ul>"
                Dim oChild As XmlElement
                For Each oChild In oElmt.ChildNodes
                    cReturn &= "<li><strong>" & oChild.Name & "</strong>: "
                    Dim oGChild As XmlElement
                    Dim i As Integer = 0
                    For Each oGChild In oChild.SelectNodes("*")
                        cReturn &= "<ul><li><strong>" & oGChild.Name & "</strong>: " & oGChild.InnerXml & "</li></ul>"
                        i += 1
                    Next
                    If i = 0 Then cReturn &= oChild.InnerText
                    cReturn &= "</li>"
                Next
                cReturn &= "</ul><br/>"
            Next
            cReturn = Replace(cReturn, "&", "&amp;")
            cReturn = Replace(cReturn, "&amp;amp;", "&amp;")
            Return cReturn
        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "ExportItems", ex, ""))
            Return ex.ToString
        End Try
    End Function

    Private Sub LogError(ByVal cModuleName As String, ByVal cRoutineName As String, ByVal oException As Exception, ByVal cFurtherInfo As String)
        Dim cInfo As String = ""
        Try
            cInfo = "Check Path"
            If moSyncConfig("ErrorLog") = "" Then Exit Sub
            cInfo = "Build File Name"
            Dim cFileName As String = Eonic.Tools.Text.IntegerToString(Now.Year, 2)
            cFileName &= Eonic.Tools.Text.IntegerToString(Now.Month, 2)
            cFileName &= Eonic.Tools.Text.IntegerToString(Now.Day, 2)
            cFileName &= ".xml"
            cFileName = moSyncConfig("ErrorLog") & cFileName

            Dim oErrorXML As New XmlDocument
            If IO.File.Exists(cFileName) Then
                cInfo = "File Exists"
                oErrorXML.Load(cFileName)
            Else
                cInfo = "New"
                oErrorXML.AppendChild(oErrorXML.CreateElement("Log"))
            End If
            cInfo = "Building Log"
            Dim oLogElmt As XmlElement = oErrorXML.CreateElement("Error")
            oLogElmt.SetAttribute("Time", Now)
            oLogElmt.SetAttribute("Module", cModuleName)
            oLogElmt.SetAttribute("Routine", cRoutineName)
            oLogElmt.SetAttribute("Information", cFurtherInfo)

            Dim oExceptionElmt As XmlElement = oErrorXML.CreateElement("Exception")
            oExceptionElmt.AppendChild(oErrorXML.CreateElement("Message")).InnerText = oException.Message
            oExceptionElmt.AppendChild(oErrorXML.CreateElement("Source")).InnerText = oException.Source
            oExceptionElmt.AppendChild(oErrorXML.CreateElement("StackTrace")).InnerText = oException.StackTrace
            oLogElmt.AppendChild(oExceptionElmt)
            oErrorXML.DocumentElement.AppendChild(oLogElmt)
            cInfo = "Saving Log"
            oErrorXML.Save(cFileName)

        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "LogError", ex, cInfo))
        End Try
    End Sub




    Private Sub AddLowError(ByVal cInformation As String) Handles moXSLTFunctions.XSLTError
        Try
            If oLowErrors Is Nothing Then oLowErrors = New XmlDocument
            If oLowErrors.DocumentElement Is Nothing Then oLowErrors.AppendChild(oLowErrors.CreateElement("LowErrors"))
            Dim oErr As XmlElement = oLowErrors.CreateElement("LowError")
            oErr.InnerText = cInformation
            oLowErrors.DocumentElement.AppendChild(oErr)
        Catch ex As Exception

        End Try
    End Sub

    Private Sub ReportLowErrors(ByRef ReportDoc As XmlDocument)
        Try
            ReportDoc.DocumentElement.AppendChild(ReportDoc.ImportNode(oLowErrors.DocumentElement, True))
        Catch ex As Exception

        End Try
    End Sub

#End Region

#Region "Sub Classes"

    Private Class Xslt
        Inherits Eonic.Tools.Xslt.XsltFunctions

#Region "Declarations"

        Private myWeb As Web
        Public Event OnError(ByVal cModuleName As String, ByVal cRoutineName As String, ByVal oException As Exception, ByVal cFurtherInfo As String)
        Private Const mcModuleName As String = "Web.ExternalSynchronisation.Xslt"
        Private moSyncConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("eonic/synchronisation")
        Private moConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("eonic/web")
        Public Event XSLTError(ByVal cInformation As String)
#End Region

#Region "Initialisation/Private"
        Public Sub New(ByVal aWeb As Web)
            myWeb = aWeb
        End Sub
#End Region

#Region "General Text/Number/File"

        Private Function tidyXhtml(ByVal shtml As String, Optional ByVal bReturnNumbericEntities As Boolean = False) As String
            Dim sTidyXhtml As String
            PerfMon.Log("Web", "tidyXhtmlFrag")


            sTidyXhtml = tidyXhtmlFrag(shtml, bReturnNumbericEntities)


            Return sTidyXhtml

        End Function


        Public Function CleanHTML(ByVal cHTML As String) As Object

            Dim oXML As New XmlDocument

            If cHTML Is Nothing Then
                cHTML = ""
            End If

            cHTML = Replace(cHTML, Chr(13), "<br/>")
            cHTML = Eonic.Tools.Xml.convertEntitiesToCodes(cHTML)
            cHTML = Replace(Replace(cHTML, "&gt;", ">"), "&lt;", "<")
            cHTML = "<p>" & cHTML & "</p>"
            Try
                cHTML = tidyXhtml(cHTML)
            Catch ex As Exception
                RaiseEvent XSLTError(ex.ToString)
            End Try
            cHTML = Replace(cHTML, "&#x0;", "")
            cHTML = Replace(cHTML, " &#0;", "")
            Try
                oXML.LoadXml(cHTML)
                Return oXML.DocumentElement
            Catch ex As Exception
                'Lets try option 2 first before we raise an error
                ' RaiseEvent XSLTError(ex.ToString)
                Try
                    oXML = New XmlDocument
                    oXML.AppendChild(oXML.CreateElement("div"))
                    oXML.DocumentElement.InnerXml = cHTML
                    Return oXML.DocumentElement
                Catch ex2 As Exception
                    RaiseEvent XSLTError(ex2.ToString)
                    Return cHTML
                End Try
            End Try
        End Function

        Public Function SQLDateTime(ByVal sDate As String) As String
            Try
                If IsDate(sDate) Then
                    Return Eonic.Tools.Database.SqlDate(CDate(sDate))
                Else
                    Return "'" & sDate & "'"
                End If
            Catch ex As Exception
                Return "'" & sDate & "'"
            End Try
        End Function

        Public Function VirtualFileExists(ByVal cVirtualPath As String) As Integer
            Try
                Dim cVP As String = goServer.MapPath(cVirtualPath)
                If IO.File.Exists(cVP) Then
                    Return 1
                Else
                    Return 0
                End If
            Catch ex As Exception
                Return 0
            End Try
        End Function

        Public Function ImageWidth(ByVal cVirtualPath As String) As String
            Try
                If VirtualFileExists(cVirtualPath) > 0 Then
                    Dim oImage As New Eonic.Tools.Image(goServer.MapPath(cVirtualPath))
                    Dim nVar As Integer = oImage.Width
                    oImage.Close()
                    Return nVar
                Else
                    Return ""
                End If
            Catch ex As Exception
                Return ""
            End Try
        End Function

        Public Function ImageHeight(ByVal cVirtualPath As String) As String
            Try
                If VirtualFileExists(cVirtualPath) > 0 Then
                    Dim oImage As New Eonic.Tools.Image(goServer.MapPath(cVirtualPath))
                    Dim nVar As Integer = oImage.Height
                    oImage.Close()
                    Return nVar
                Else
                    Return ""
                End If
            Catch ex As Exception
                Return ""
            End Try
        End Function


        Public Function ResizeImage(ByVal cVirtualPath As String, ByVal maxWidth As Long, ByVal maxHeight As Long, ByVal sSuffix As String) As String
            Dim newFilepath As String = ""
            Try
                If VirtualFileExists(cVirtualPath) > 0 Then
                    Dim oImage As New Eonic.Tools.Image(goServer.MapPath(cVirtualPath))
                    'calculate the new filename
                    newFilepath = Replace(cVirtualPath, ".jpg", sSuffix & ".jpg")
                    If Not (VirtualFileExists(newFilepath) > 0) Then
                        oImage.KeepXYRelation = True
                        oImage.SetMaxSize(maxWidth, maxHeight)
                        oImage.Save(goServer.MapPath(newFilepath), 25)
                        Return newFilepath
                    Else
                        Return newFilepath
                    End If
                Else
                    Return "Source Image Not Found"
                End If


            Catch ex As Exception
                Return ""
            End Try
        End Function

        Public Function ResizeImage(ByVal cVirtualPath As String, ByVal maxWidth As Long, ByVal maxHeight As Long, ByVal sSuffix As String, ByVal nCompression As Integer) As String
            Dim newFilepath As String = ""
            Try
                If VirtualFileExists(cVirtualPath) > 0 Then
                    Dim oImage As New Eonic.Tools.Image(goServer.MapPath(cVirtualPath))
                    'calculate the new filename
                    newFilepath = Replace(cVirtualPath, ".jpg", sSuffix & ".jpg")
                    If Not (VirtualFileExists(newFilepath) > 0) Then
                        oImage.KeepXYRelation = True
                        oImage.SetMaxSize(maxWidth, maxHeight)
                        oImage.Save(goServer.MapPath(newFilepath), nCompression)
                        Return newFilepath
                    Else
                        Return newFilepath
                    End If
                Else
                    Return "Source Image Not Found"
                End If


            Catch ex As Exception
                Return ""
            End Try
        End Function



        Public Overloads Function replacestring(ByVal text As String, ByVal replace As String, ByVal replaceWith As String) As String
            Try
                Return MyBase.replacestring(text, replace, replaceWith)
            Catch ex As Exception
                Return text
            End Try
        End Function

        Public Overloads Function replacestrings(ByVal text As String, ByVal replaceCSV As String, ByVal replaceWithCSV As String) As String
            Try
                Dim cReplace() As String = Split(replaceCSV, ",")
                Dim cReplaceWith() As String = Split(replaceWithCSV, ",")
                For i As Integer = 0 To cReplace.Length - 1
                    Dim replaceWith As String = ""
                    If cReplaceWith.Length >= i Then replaceWith = cReplaceWith(i)
                    MyBase.replacestring(text, cReplace(i), replaceWith)
                Next
                Return text
            Catch ex As Exception
                Return text
            End Try
        End Function

        Public Function LeftBefore(ByVal Text As String, ByVal Character As String) As String
            Try
                If Not Text.Contains(Character) Then Return Text
                Return Text.Substring(0, Text.IndexOf(Character))
            Catch ex As Exception
                Return ""
            End Try
        End Function

        Public Function LeftBeforeChr(ByVal Text As String, ByVal Chr As Integer) As String
            Return LeftBefore(Text, Microsoft.VisualBasic.Chr(Chr))
        End Function

        Public Function RightAfter(ByVal Text As String, ByVal Character As String) As String
            Try
                If Not Text.Contains(Character) Then Return Text
                Dim nPos As Integer = Text.IndexOf(Character) + 1
                Return Text.Substring(nPos, Text.Length - nPos)
            Catch ex As Exception
                Return ""
            End Try
        End Function

        Public Function RightAfterChr(ByVal Text As String, ByVal Chr As Integer) As String
            Return RightAfter(Text, Microsoft.VisualBasic.Chr(Chr))
        End Function

#End Region

#Region "Eonicweb Specific"

        Public Function setContentLocationByRef(ByVal cStructName As String, ByVal nContentId As Integer, ByVal bPrimary As Integer, ByVal bCascade As Integer) As Integer
            Try
                Dim nID As String = "" 'myWeb.moDbHelper.getKeyByNameAndSchema(Web.dbHelper.objectTypes.ContentStructure, "", cStructName)
                nID = myWeb.moDbHelper.getObjectByRef(Web.dbHelper.objectTypes.ContentStructure, cStructName)
                If nID = "" Then nID = 0
                If nID > 0 Then

                    Return myWeb.moDbHelper.setContentLocation(nID, nContentId, IIf(bPrimary = 1, True, False), bCascade, True)
                Else
                    Return 0
                End If
            Catch ex As Exception
                RaiseEvent OnError(mcModuleName, "setContentLocation", ex, "")
                Return 0
            End Try
        End Function

        Public Function setContentLocationsByRef(ByVal cStructName As String, ByVal nContentId As Integer, ByVal bPrimary As Integer, ByVal bCascade As Integer) As String
            Try
                Dim nIDs() As String = Nothing 'myWeb.moDbHelper.getKeyByNameAndSchema(Web.dbHelper.objectTypes.ContentStructure, "", cStructName)
                Dim i As Integer
                nIDs = myWeb.moDbHelper.getObjectsByRef(Web.dbHelper.objectTypes.ContentStructure, cStructName)

                If Not nIDs Is Nothing Then
                    For i = 0 To nIDs.Length - 1
                        myWeb.moDbHelper.setContentLocation(nIDs(i), nContentId, IIf(bPrimary = 1, True, False), bCascade, True)
                    Next
                    Return String.Concat(nIDs)
                Else
                    Return 0
                End If
            Catch ex As Exception
                RaiseEvent OnError(mcModuleName, "setContentLocation", ex, "")
                Return 0
            End Try
        End Function

        Public Function GetSetting(ByVal SettingName As String) As String
            Try
                Dim cReturn As String = moSyncConfig(SettingName)
                If cReturn = "" Or cReturn = Nothing Then cReturn = moConfig(SettingName)
                Return cReturn
            Catch ex As Exception
                Return ""
            End Try
        End Function

        Public Function SetRef(ByVal objecttype As Eonic.Web.dbHelper.objectTypes, ByVal id As Integer, ByVal value As String) As String
            Try
                Dim cTableName As String = myWeb.moDbHelper.getTable(objecttype)
                Dim cRefField As String = myWeb.moDbHelper.getFRef(objecttype)
                Dim cWhereField As String = myWeb.moDbHelper.TableKey(objecttype)
                Dim cSQL As String = "UPDATE " & cTableName & " SET " & cRefField & " = '" & value & "' WHERE " & cWhereField & " = " & id
                myWeb.moDbHelper.ExeProcessSql(cSQL)
                Return value
            Catch ex As Exception
                Return ""
            End Try
        End Function


        ' this was stupidly named
        Public Function RemoveLocationsByRef(ByVal cValidStructNames As String, ByVal nContentId As Long) As Integer
            Dim i As Integer = 0
            Try
                Return RemoveLocationsRefNotIncluded(cValidStructNames, nContentId)
            Catch ex As Exception
                Return i
            End Try
        End Function

        Public Function RemoveLocationsRefNotIncluded(ByVal cValidStructNames As String, ByVal nContentId As Long) As Integer
            Dim i As Integer = 0
            Try
                cValidStructNames = Replace(cValidStructNames, "'',", "")
                cValidStructNames = Replace(cValidStructNames, ",''", "")
                'don't remove from pages with no foriegn ref's
                cValidStructNames = cValidStructNames & ",''"
                If cValidStructNames = "" Then Return 0
                Dim cSQL As String = "SELECT tblContentLocation.nContentLocationKey FROM tblContentLocation INNER JOIN tblContentStructure ON tblContentLocation.nStructId = tblContentStructure.nStructKey"
                cSQL &= " WHERE (NOT (tblContentStructure.cStructForiegnRef IN (" & cValidStructNames & "))) AND nContentId = " & nContentId
                Dim oDR As SqlClient.SqlDataReader = myWeb.moDbHelper.getDataReader(cSQL)
                Do While oDR.Read
                    myWeb.moDbHelper.DeleteObject(Web.dbHelper.objectTypes.ContentLocation, oDR(0))
                    i += 1
                Loop
                oDR.Close()
                Return i
            Catch ex As Exception
                Return i
            End Try
        End Function

        Public Function RemoveLocationsWithRef(ByVal cRemoveRefs As String, ByVal nContentId As Long) As Integer
            Dim i As Integer = 0
            Try
                cRemoveRefs = Replace(cRemoveRefs, "'',", "")
                cRemoveRefs = Replace(cRemoveRefs, ",''", "")
                If cRemoveRefs = "" Then Return 0
                Dim cSQL As String = "SELECT tblContentLocation.nContentLocationKey FROM tblContentLocation INNER JOIN tblContentStructure ON tblContentLocation.nStructId = tblContentStructure.nStructKey"
                cSQL &= " WHERE (tblContentStructure.cStructForiegnRef IN (" & cRemoveRefs & ")) AND nContentId = " & nContentId
                Dim oDR As SqlClient.SqlDataReader = myWeb.moDbHelper.getDataReader(cSQL)
                Do While oDR.Read
                    myWeb.moDbHelper.DeleteObject(Web.dbHelper.objectTypes.ContentLocation, oDR(0))
                    i += 1
                Loop
                oDR.Close()
                Return i
            Catch ex As Exception
                Return i
            End Try
        End Function

        ''' <summary>
        ''' Function to be called from an import XSLT to allow the deletion of content not contained in ValidContentNames.
        ''' ONLY REMOVES CONTENT RELEATION FOR CONTENTS THAT ARE THE SAME TYPE AS SPECIFICALLY FOR PRODUCT IMPORTS
        ''' </summary>
        ''' <param name="cValidContentNames">List of Frefs for content for which we don't want to remove the relationship for</param>
        ''' <param name="nContentId">The Content Id of the item we are handing relations for.</param>
        ''' <param name="cContentTypeToRemove">The content Type of the Content Type we are removing from.</param>
        ''' <returns>A count of the number of content items removed</returns>
        ''' <remarks></remarks>
        ''' 
        Public Function RemoveRelatedContentByRef(ByVal cValidContentNames As String, ByVal nContentId As Integer, ByVal cContentTypeToRemove As String) As Integer
            Dim i As Integer = 0
            Try
                cValidContentNames = Replace(cValidContentNames, "'',", "")
                cValidContentNames = Replace(cValidContentNames, ",''", "")
                If cValidContentNames = "" Then Return 0
                Dim cSQL As String = "SELECT Rel.nContentRelationKey, Rel.nContentParentId, Rel.nContentChildId, Childs.cContentForiegnRef" & _
                " FROM tblContent Childs INNER JOIN" & _
                " tblContentRelation Rel ON Childs.nContentKey = Rel.nContentChildId INNER JOIN" & _
                " tblContent Parents ON Rel.nContentParentId = Parents.nContentKey WHERE" & _
                " (((Rel.nContentParentId = " & nContentId & ") AND (NOT (Childs.cContentForiegnRef IN (" & cValidContentNames & ")))) OR" & _
                " ((Rel.nContentChildId  = " & nContentId & ") AND (NOT (Childs.cContentForiegnRef IN (" & cValidContentNames & "))))) AND " & _
                " (Childs.cContentSchemaName = '" & cContentTypeToRemove & "' AND  Parents.cContentSchemaName = '" & cContentTypeToRemove & "')"

                Dim oDR As SqlClient.SqlDataReader = myWeb.moDbHelper.getDataReader(cSQL)
                Do While oDR.Read
                    myWeb.moDbHelper.DeleteObject(Web.dbHelper.objectTypes.ContentRelation, oDR(0))
                    i += 1
                Loop
                oDR.Close()
                Return i
            Catch ex As Exception
                Return i
            End Try
        End Function

        Public Function ResetRelatedByType(ByVal ContentId As Integer, ByVal RelatedRefArr As String, ByVal RelationType As String, ByVal RelatedContentType As String, ByVal TwoWayRelations As Boolean) As Integer
            Dim i As Integer = 0
            Dim cSQL As String
            Dim oRef() As String = Split(RelatedRefArr, ",")
            Dim DelFlag As Boolean
            Dim AddFlag As Boolean
            Try

                'Select all valid existing content relationships for the contentId returning Foriegn Keys.
                cSQL = "SELECT Rel.nContentRelationKey, Rel.nContentParentId, Rel.nContentChildId, Childs.cContentForiegnRef" & _
                " FROM tblContent Childs INNER JOIN" & _
                " tblContentRelation Rel ON Childs.nContentKey = Rel.nContentChildId " & _
                " WHERE" & _
                " Rel.nContentParentId = " & ContentId & " " & _
                " AND Childs.cContentSchemaName = '" & RelatedContentType & "'"
                If RelationType = "" Then
                    cSQL += " AND Rel.cRelationType is null"
                Else
                    cSQL += " AND Rel.cRelationType = '" & RelationType & "'"
                End If

                If TwoWayRelations Then
                    cSQL = cSQL & " UNION "
                    cSQL = cSQL & " SELECT Rel.nContentRelationKey, Rel.nContentParentId, Rel.nContentChildId, Childs.cContentForiegnRef" & _
                    " FROM tblContent Childs INNER JOIN" & _
                    " tblContentRelation Rel ON Childs.nContentKey = Rel.nContentParentId " & _
                    " WHERE" & _
                    " Rel.nContentChildId = " & ContentId & " " & _
                    " AND Childs.cContentSchemaName = '" & RelatedContentType & "'"
                    If RelationType = "" Then
                        cSQL += " AND Rel.cRelationType is null"
                    Else
                        cSQL += " AND Rel.cRelationType = '" & RelationType & "'"
                    End If
                End If

                'Step through oRs and delete those not in the Ref Array.
                Dim oDR As SqlClient.SqlDataReader = myWeb.moDbHelper.getDataReader(cSQL)
                Do While oDR.Read
                    DelFlag = True
                    For i = 0 To UBound(oRef)
                        If oRef(i).Trim <> "" Then
                            If oRef(i) = oDR("cContentForiegnRef") Then
                                DelFlag = False
                            End If
                        Else
                            DelFlag = False
                        End If
                    Next
                    If DelFlag Then
                        myWeb.moDbHelper.DeleteObject(Web.dbHelper.objectTypes.ContentRelation, oDR(0))
                    End If
                Loop

                'Step through ref array and add those not found in the oRs
                For i = 0 To UBound(oRef)
                    If oRef(i).Trim <> "" Then
                        AddFlag = True
                        Do While oDR.Read
                            If oRef(i).Trim = oDR("cContentForiegnRef") Then
                                AddFlag = False
                            End If
                        Loop
                        If AddFlag Then
                            AddContentRelationByRef(ContentId, oRef(i).Trim, TwoWayRelations, RelationType)
                        End If
                    End If
                Next

                oDR.Close()
                Return i
            Catch ex As Exception
                Return i
            End Try
        End Function


        Public Function AddContentRelationByRef(ByVal nContentId As Integer, ByVal cContentRef As String) As String
            Try

                Return AddContentRelationByRef(nContentId, cContentRef, True, "")

            Catch ex As Exception
                Return " Relation Error"
            End Try
        End Function

        Public Function AddContentRelationByRef(ByVal nContentId As Integer, ByVal cContentRef As String, ByVal TwoWay As Boolean, ByVal Type As String) As String
            Try
                If nContentId = 0 Then Return "No ContentId"
                If cContentRef = "" Then Return "No ContentRef"
                Dim nRefId As Integer = myWeb.moDbHelper.GetDataValue("SELECT nContentKey, cContentForiegnRef FROM tblContent WHERE (cContentForiegnRef = '" & cContentRef & "')", , , 0)
                If Not nRefId = 0 Then
                    myWeb.moDbHelper.insertContentRelation(nContentId, nRefId, TwoWay, Type)
                    Return nRefId
                Else
                    Return "Not Related"
                End If
            Catch ex As Exception
                Return " Relation Error"
            End Try
        End Function


#End Region

        Private Sub Xslt_XSLTError(ByVal cInformation As String) Handles Me.XSLTError

        End Sub
    End Class

#End Region






End Class
