Imports System.IO
Imports System.Collections.Generic
Imports System.Xml
Imports System.Data.SqlClient
Imports Protean.Tools
Imports System

Partial Public Class Cms


    ''' <summary>
    ''' Class for sync methods that are specific to Protean.Cms
    ''' e.g. syncing fle lists as content to modules
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Synchronisation

        Public Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
        Private Const mcModuleName As String = "Protean.Cms.Synchronisation"

        Private Sub _OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
            RaiseEvent OnError(sender, e)
        End Sub

        Private _myWeb As Protean.Cms

        Public Sub New(ByRef aWeb As Protean.Cms)
            _myWeb = aWeb
        End Sub

        Public Sub SyncModuleContentFromFolder(ByVal id As Integer)

            Dim cProcessInfo As String = "SyncModuleContentFromFolder"

            Try

                ' List
                Dim currentRelatedContent As New Dictionary(Of String, String)
                Dim currentUnrelatedContent As New Dictionary(Of String, String)
                Dim currentFileList As List(Of String)
                Dim filesToAdd As New List(Of String)
                Dim contentToRelate As New List(Of String)
                Dim contentIDsToRemoveRelation As New List(Of String)

                Dim folderPath As String = ""

                Dim fileType As fsHelper.LibraryType = fsHelper.LibraryType.Image ' Default Value

                ' =====================================================================
                '   Load the module XML
                ' =====================================================================
                Dim moduleContentInstance As XmlElement = _myWeb.moPageXml.CreateElement("ContentInstance")
                moduleContentInstance.InnerXml = _myWeb.moDbHelper.getObjectInstance(dbHelper.objectTypes.Content, id)

                ' =====================================================================
                '   Get the folder path to work on
                '   Only proceed if there is a sync folder to work on.
                ' =====================================================================
                Dim moduleContentBrief As XmlElement = moduleContentInstance.SelectSingleNode("tblContent/cContentXmlBrief/Content[@syncLocation]")
                If moduleContentBrief IsNot Nothing _
                    AndAlso Not String.IsNullOrEmpty(moduleContentBrief.GetAttribute("syncLocation")) Then



                    If Not (Tools.EnumUtility.TryParse(GetType(fsHelper.LibraryType), moduleContentBrief.GetAttribute("syncLibraryType"), True, fileType)) Then
                        fileType = fsHelper.LibraryType.Image
                    End If

                    ' =====================================================================
                    '   Build a list of files currently in the actual folder
                    ' =====================================================================
                    folderPath = moduleContentBrief.GetAttribute("syncLocation")
                    currentFileList = _myWeb.moFSHelper.GetFileListByTypeFromRelativePath(folderPath, fileType)



                    ' =====================================================================
                    '   From existing content, find all content with a foreign reference,
                    '   and work out if it's already related to the content
                    ' =====================================================================
                    Dim frefContentQuery As String = "SELECT DISTINCT c.nContentKey, c.cContentForiegnRef , r.nContentRelationKey " _
                        & "FROM	tblContent c " _
                        & "INNER JOIN tblAudit a " _
                        & "ON c.nAuditId = a.nAuditKey AND c.cContentForiegnRef <> '' And c.cContentForiegnRef IS NOT NULL " _
                        & "LEFT JOIN  tblContentRelation r " _
                        & "ON r.nContentChildId = c.nContentKey AND nContentParentId = " & id & " "
                    ' & "WHERE " & _myWeb.GetStandardFilterSQLForContent(False)


                    Dim fRefContentData As SqlDataReader = _myWeb.moDbHelper.getDataReader(frefContentQuery)
                    Do While fRefContentData.Read
                        ' Foreign Ref follows the format <FILE PATH> e.g. /images/myimage.jpg

                        Dim pathFromFref As String = fRefContentData(1).ToString


                        ' Now work out if it's related content or if it's not related
                        ' Not this assumes that Foreign Refs are unique, which they should be!
                        If IsDBNull(fRefContentData(2)) Then
                            cProcessInfo = "problem adding:" & pathFromFref
                            If Not currentUnrelatedContent.ContainsKey(pathFromFref) Then
                                currentUnrelatedContent.Add(pathFromFref, fRefContentData(0).ToString)
                            End If
                        Else
                            If Not currentRelatedContent.ContainsKey(pathFromFref) Then
                                currentRelatedContent.Add(pathFromFref, fRefContentData(0).ToString)
                            End If
                        End If

                    Loop

                    cProcessInfo = ""
                    fRefContentData.Close()

                    ' =====================================================================
                    '   Build a list of files to add
                    '   i.e. those that are in the folder but have no content in this scope
                    ' =====================================================================
                    filesToAdd = currentFileList.FindAll(Function(filePath As String) Not (currentRelatedContent.ContainsKey(filePath) Or currentUnrelatedContent.ContainsKey(filePath)))

                    ' =====================================================================
                    '   Build a list of content to relate
                    '   i.e. those that are in the folder and have unrelated content
                    ' =====================================================================
                    contentToRelate = currentFileList.FindAll(Function(filePath As String) currentUnrelatedContent.ContainsKey(filePath))
                    contentToRelate = contentToRelate.ConvertAll(Function(filePath As String) currentUnrelatedContent(filePath).ToString)

                    ' =====================================================================
                    '   Build a list of content to delete the relation to
                    '   i.e. those that are not in the folder but appear as related content
                    '   in this scope
                    ' =====================================================================
                    For Each relatedContentkvp As KeyValuePair(Of String, String) In currentRelatedContent
                        If Not currentFileList.Contains(relatedContentkvp.Key) Then
                            contentIDsToRemoveRelation.Add(relatedContentkvp.Value)
                        End If
                    Next

                End If


                ' =====================================================================
                '   Add files
                ' =====================================================================
                If filesToAdd.Count > 0 Then
                    ' Process the lists (add and update)
                    Dim webFilesToAdd As New List(Of fsHelper.WebFile)
                    webFilesToAdd = filesToAdd.ConvertAll(Function(virtualpath As String) convertVirtualPathToWebFile(_myWeb, virtualpath))

                    ' Convert the webFiles to XML
                    Dim serializedWebFiles As XmlElement = Tools.Xml.SerializeToXml(Of List(Of fsHelper.WebFile))(webFilesToAdd)

                    ' Build XML to be converted
                    Dim importXmlDocument As New XmlDocument
                    importXmlDocument.LoadXml("<Imports><Source/><Schemas/><Settings><ParentContentID>" & id.ToString & "</ParentContentID></Settings></Imports>")

                    ' Add the Web files to the XML
                    Dim importWebFiles As XmlElement = importXmlDocument.SelectSingleNode("Imports/Source")
                    importWebFiles.InnerXml = serializedWebFiles.InnerXml

                    ' Add the XML schema for specified content schemas
                    Dim importSchemas As XmlElement = importXmlDocument.SelectSingleNode("Imports/Schemas")
                    Dim contentTypes As String = moduleContentBrief.GetAttribute("syncContentType")
                    If String.IsNullOrEmpty(contentTypes) Then contentTypes = fsHelper.GetDefaultContentSchemaNamesForLibraryType(fileType)
                    Dim contentTypesList As New List(Of String)(contentTypes.Split(","))
                    If contentTypesList.Count > 0 Then
                        Dim blankSchema As New xForm(_myWeb.msException)
                        For Each schemaName As String In contentTypesList
                            If blankSchema.load("/xforms/content/" & schemaName & ".xml", _myWeb.maCommonFolders) Then
                                Tools.Xml.AddExistingNode(importSchemas, blankSchema.Instance)
                            End If
                        Next
                    End If

                    ' syncXSLPath
                    Dim xslPath As String = moduleContentBrief.GetAttribute("syncXSLPath")
                    If String.IsNullOrEmpty(xslPath) Then
                        xslPath = "/xsl/import/fileTo" & [Enum].GetName(GetType(fsHelper.LibraryType), fileType) & ".xsl"
                    End If
                    xslPath = _myWeb.moFSHelper.FindFilePathInCommonFolders(xslPath, _myWeb.maCommonFolders)

                    If Not String.IsNullOrEmpty(xslPath) Then

                        Dim transformer As New Protean.XmlHelper.Transform(_myWeb, _myWeb.goServer.MapPath(xslPath), False)
                        PerfMon.Log("Admin", "FileSyncTransform-startxsl")
                        transformer.mbDebug = gbDebug
                        transformer.ProcessDocument(importXmlDocument)
                        PerfMon.Log("Admin", "FileSyncTransform-endxsl")
                        transformer = Nothing

                        If importXmlDocument.SelectNodes("/Instances/Instance").Count > 0 Then
                            _myWeb.moDbHelper.importObjects(importXmlDocument.DocumentElement)
                        End If

                    End If


                End If

                ' =====================================================================
                '   Add relations to existing content
                ' =====================================================================
                If contentToRelate.Count > 0 Then
                    _myWeb.moDbHelper.insertContentRelation(id, String.Join(",", contentToRelate.ToArray()))
                End If


                ' =====================================================================
                '   Remove relations to existing content
                ' =====================================================================
                If contentIDsToRemoveRelation.Count > 0 Then
                    For Each contentId As String In contentIDsToRemoveRelation
                        _myWeb.moDbHelper.RemoveContentRelation(id, Convert.ToInt64(contentId))
                    Next
                End If

                ' Process the delete list
                'If contentIDsToDelete.Count > 0 Then _myWeb.moDbHelper.BulkContentDelete(contentIDsToDelete)

            Catch ex As Exception
                RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, System.Reflection.MethodBase.GetCurrentMethod().Name, ex, cProcessInfo))
            End Try

        End Sub

        Public Function convertVirtualPathToWebFile(ByVal myWeb As Protean.Cms, ByVal virtualPath As String) As fsHelper.WebFile

            Return New fsHelper.WebFile(myWeb.goServer.MapPath(virtualPath), virtualPath, True)

        End Function


#Region "Module Behaviour"

        Public Class Modules

            Public Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
            Private Const mcModuleName As String = "Protean.Cms.Synchronisation.Modules"

            Private Sub _OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
                RaiseEvent OnError(sender, e)
            End Sub

            Public Sub New()

            End Sub

            Public Sub SyncModuleContentFromFolder(ByRef myWeb As Protean.Cms, ByVal contentNode As XmlElement, ByVal contentId As Integer, ByVal editAction As Cms.dbHelper.ActivityType)

                Try
                    Dim mySync As New Synchronisation(myWeb)
                    mySync.SyncModuleContentFromFolder(contentId)
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, System.Reflection.MethodBase.GetCurrentMethod().Name, ex, ""))
                End Try

            End Sub

        End Class

#End Region


    End Class




End Class

