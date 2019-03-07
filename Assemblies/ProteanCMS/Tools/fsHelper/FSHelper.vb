'***********************************************************************
' $Library:     Protean.fsHelper
' $Revision:    4.0  
' $Date:        2006-09-22
' $Author:      Trevor Spink (trevor@eonic.co.uk) et al.
' &Website:     www.eonic.co.uk
' &Licence:     All Rights Reserved.
' $Copyright:   Copyright (c) 2002 - 2011 Eonicweb Ltd.
'***********************************************************************

Option Strict Off
Option Explicit On
Imports System.Data
Imports System.Data.SqlClient
Imports System.Xml
Imports System.IO
Imports System.Configuration
Imports System.Collections
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Security.Principal
Imports System.Web.Configuration
Imports Protean.Tools.DelegateWrappers
Imports System

Partial Public Class fsHelper

#Region "Declarations"

    ' Note from AG: Where possible - please create Shared methods and props on this, 
    ' as some calling methods may not be able to provide the System.Web.HttpContext.Current object for goServer

    Shared mcModuleName As String = "Protean.fsHelper"
    Private goConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/web")
    Private goServer As System.Web.HttpServerUtility

    Public moPageXML As XmlDocument
    Public mcStartFolder As String = ""
    Public mcPopulateFilesNode As String = ""
    Public mcRoot As String = ""

    Shared _libraryTypeExtensions()() As String = { _
                                                 New String() {}, _
                                                 New String() {"png", "jpg", "gif", "jpeg", "bmp"}, _
                                                 New String() {"doc", "docx", "xls", "xlsx", "pdf"}, _
                                                 New String() {"avi", "flv", "swf", "ppt"}}

    Shared _defaultLibraryTypeContentSchemaNames() As String = {"PlainText", _
                                                                "LibraryImage", _
                                                                "Document", _
                                                                "Video,FlashMovie"}


    Enum LibraryType
        Undefined = 0
        Image = 1
        Documents = 2
        Media = 3
        Scripts = 4
        Style = 5
    End Enum

#End Region
#Region "Constructor"

    Public Sub New()
        Me.New(System.Web.HttpContext.Current)
    End Sub

    Public Sub New(ByVal Context As System.Web.HttpContext)
        goServer = Context.Server
    End Sub

    Public Shadows Sub open(ByVal oPageXml As XmlDocument)
        PerfMon.Log("fsHelper", "open")
        Dim cProcessInfo As String = ""
        Try
            moPageXML = oPageXml

        Catch ex As Exception
            returnException(mcModuleName, "PersistVariables", ex, "", cProcessInfo, gbDebug)
        End Try
    End Sub
#End Region
#Region "Initialisation"
    Public Sub initialiseVariables(ByVal nLib As LibraryType)
        PerfMon.Log("fsHelper", "initialiseVariables")
        Dim cProcessInfo As String = ""
        Dim cStartFolder As String = ""

        Try

            mcRoot = GetFileLibraryPath(nLib)
            mcRoot = Replace(mcRoot, "/", "\")

            If mcRoot.StartsWith("\") Then mcRoot = mcRoot.Substring(1)
            If Not String.IsNullOrEmpty(mcRoot) Then
                If mcRoot.StartsWith("..") Then
                    cStartFolder = goServer.MapPath("/" & goConfig("ProjectPath")) & mcRoot
                Else
                    cStartFolder = goServer.MapPath("/" & goConfig("ProjectPath") & mcRoot)
                End If
            End If

            mcStartFolder = cStartFolder

        Catch ex As Exception
            returnException(mcModuleName, "initialiseVariables", ex, "", cProcessInfo, gbDebug)
        End Try

    End Sub

    Public Sub initialiseVariables()

        PerfMon.Log("fsHelper", "initialiseVariables")
        Dim cProcessInfo As String = ""
        Dim cStartFolder As String = ""

        Try

            mcRoot = Replace(mcRoot, "/", "\")
            If mcRoot.StartsWith("\") Then mcRoot = mcRoot.Substring(1)

            If Not String.IsNullOrEmpty(mcRoot) Then
                If mcRoot.StartsWith("..") Then
                    cStartFolder = goServer.MapPath("/" & goConfig("ProjectPath") & mcRoot)
                Else
                    cStartFolder = goServer.MapPath("/" & goConfig("ProjectPath") & mcRoot)
                End If
            End If

            mcStartFolder = cStartFolder

        Catch ex As Exception
            returnException(mcModuleName, "initialiseVariables", ex, "", cProcessInfo, gbDebug)
        End Try

    End Sub
#End Region
#Region "Public Methods"
    Public Function getConfigNode(ByVal cPath As String) As XmlElement
        PerfMon.Log("fsHelper", "getConfigNode")
        Dim cProcessInfo As String = ""
        Dim oConfigXml As XmlDataDocument = New XmlDataDocument

        Dim oConfigNode As XmlElement

        Try
            oConfigXml.Load(goServer.MapPath(goConfig("ProjectPath") & "/Web.config"))
            'this now has a namespace to handle!!!
            oConfigNode = oConfigXml.SelectSingleNode("/configuration/protean/" & cPath)

            Return oConfigNode

        Catch ex As Exception
            returnException(mcModuleName, "getConfigNode", ex, "", cProcessInfo, gbDebug)
            Return Nothing
        End Try

    End Function

    Public Shared Function checkLeadingSlash(ByVal filePath As String, Optional ByVal removeSlash As Boolean = False) As String
        PerfMon.Log("fsHelper", "checkLeadingSlash")
        Try

            filePath = filePath.Replace("\", "/")
            If removeSlash Then
                filePath = filePath.TrimStart("/")
            Else
                If Not (filePath.StartsWith("/")) Then filePath = "/" & filePath
            End If

            Return filePath

        Catch ex As Exception
            returnException(mcModuleName, "checkLeadingSlash", ex, "", filePath, gbDebug)
            Return filePath
        End Try

    End Function

    Public Function checkCommonFilePath(ByVal localFilePath As String) As String
        PerfMon.Log("fsHelper", "checkCommonFilePath")
        Dim cProcessInfo As String = ""
        Try
            localFilePath = checkLeadingSlash(localFilePath)

            If IO.File.Exists(goServer.MapPath(localFilePath)) Then
                Return localFilePath
            ElseIf IO.File.Exists(goServer.MapPath("/ewcommon" & localFilePath)) Then
                Return "/ewcommon" & localFilePath
            Else
                Return ""
            End If

        Catch ex As Exception
            returnException(mcModuleName, "checkCommonFilePath", ex, "", cProcessInfo, gbDebug)
            Return ""
        End Try

    End Function
    Public Function setConfigNode(ByVal oInstance As XmlElement) As XmlElement
        PerfMon.Log("fsHelper", "setConfigNode")
        Dim cProcessInfo As String = ""
        Dim oConfigXml As XmlDataDocument = New XmlDataDocument
        Dim oConfigNode As XmlElement

        Try

            oConfigXml.Load(goServer.MapPath(goConfig("ProjectPath") & "/web.config"))
            oConfigNode = oConfigXml.SelectSingleNode("configuration/protean/" & oInstance.Name)
            oConfigNode.InnerXml = oInstance.InnerXml
            oConfigXml.Save(goServer.MapPath(goConfig("ProjectPath") & "/web.config"))
            Return oInstance
        Catch ex As Exception
            returnException(mcModuleName, "setConfigNode", ex, "", cProcessInfo, gbDebug)
            Return oInstance
        End Try

    End Function

    Public Function getImageXhtml(ByVal cPath As String) As String
        PerfMon.Log("fsHelper", "getImageXhtml")
        Dim cProcessInfo As String = ""
        Try
            cPath = Replace(cPath, "\", "/")
            If Not cPath.StartsWith("/") Then
                cPath = "/" & cPath
            End If
            Dim ImagePath As String = GetFileLibraryPath(LibraryType.Image) & cPath

            If ImagePath.EndsWith(".svg") Then
                Return "<img src=""" & ImagePath & """ alt=""""/> "
            Else
                Dim oImg As System.Drawing.Bitmap = New System.Drawing.Bitmap(goServer.MapPath("/" & Me.mcRoot & cPath))
                Return "<img src=""" & ImagePath & """ height=""" & oImg.Height & """ width=""" & oImg.Width & """ alt=""""/> "
            End If



        Catch ex As Exception
            returnException(mcModuleName, "getImageXhtml", ex, "", cProcessInfo, gbDebug)
            Return ""
        End Try

    End Function

    Public Function getDirectoryTreeXml(ByVal nLib As LibraryType, Optional ByVal populateFilesNode As String = "") As XmlElement
        PerfMon.Log("fsHelper", "getDirectoryTreeXml")
        Dim tempStartFolder As String
        Dim TreeXml As XmlElement
        Dim aVirtualImageDirectories() As String = Split(goConfig("VirtualImageDirectories"), ",")
        Dim VirtualImageDirectory As String
        Try

            If populateFilesNode <> "" Then
                mcPopulateFilesNode = populateFilesNode
            End If

            If mcStartFolder = "" Then
                initialiseVariables(nLib)
            End If
            'Create the root folder if it doesn't exist.
            tempStartFolder = mcStartFolder
            mcStartFolder = goServer.MapPath("/")
            CreatePath(mcRoot)
            mcStartFolder = tempStartFolder

            Dim nodeElem As XmlElement = XmlElement("folder", New DirectoryInfo(mcStartFolder).Name)
            nodeElem.SetAttribute("path", "")
            PerfMon.Log("fsHelper", "getDirectoryTreeXml-AddElementsStart")
            TreeXml = AddElements(nodeElem, mcStartFolder)
            PerfMon.Log("fsHelper", "getDirectoryTreeXml-AddElementsEnd")
            If nLib = LibraryType.Image Then

                For Each VirtualImageDirectory In aVirtualImageDirectories
                    If Not VirtualImageDirectory = "" Then
                        Dim virtualPath As String = goServer.MapPath("/" & mcRoot & "/" & VirtualImageDirectory)
                        nodeElem = XmlElement("folder", New DirectoryInfo(virtualPath).Name)
                        nodeElem.SetAttribute("path", "/" & VirtualImageDirectory)
                        mcStartFolder = virtualPath.Substring(0, virtualPath.Length - VirtualImageDirectory.Length - 1)
                        AddElements(nodeElem, virtualPath)
                        TreeXml.AppendChild(nodeElem)
                    End If
                Next

            End If

            Return TreeXml
        Catch ex As Exception
            Return XmlElement("error", ex.Message)
        End Try
    End Function

    Public Function CreateFolder(ByVal cFolderName As String, ByVal cFolderPath As String) As String
        PerfMon.Log("fsHelper", "CreateFolder-Start", cFolderName)
        'in order to make this work the root directory needs to have read permissions for everyone or at lease asp.net acct
        Dim dir As New DirectoryInfo(mcStartFolder & cFolderPath & "\")
        Try
            If dir.Exists Then
                dir.CreateSubdirectory(cFolderName.Replace(" ", "-"))
            Else
                Return "this root folder does not exist"
            End If

            Return "1"
            PerfMon.Log("fsHelper", "CreateFolder-End", cFolderName)
        Catch ex As Exception
            Return ex.Message
        End Try

    End Function

    ''' <summary>
    ''' This Creates the path supplied without overwriting existing folders,
    ''' It allow us to check a folder exists and create it if not.
    ''' </summary>
    ''' <param name="cFolderPath"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CreatePath(ByVal cFolderPath As String) As String
        PerfMon.Log("fsHelper", "CreatePath", cFolderPath)
        'in order to make this work the root directory needs to have read permissions for everyone or at lease asp.net acct
        cFolderPath = Replace(cFolderPath, "\", "/")
        Dim aFolderNames() As String = cFolderPath.Split("/")
        Dim i As Integer

        Try
            Dim startDir As String
            If mcRoot = "../" Then
                mcRoot = ""
                startDir = goServer.MapPath("/")
                Dim newDir As New DirectoryInfo(startDir)
                startDir = newDir.Parent.FullName
            Else
                startDir = goServer.MapPath("/" & mcRoot)
            End If

            'check startfolder exists
            Dim rootDir As New DirectoryInfo(startDir)

            If Not rootDir.Exists Then
                Dim baseDir As New DirectoryInfo(goServer.MapPath("/"))
                rootDir = baseDir.CreateSubdirectory(mcRoot.Replace(" ", "-"))
            End If

            If mcStartFolder = "" Then mcStartFolder = startDir
            Dim workingFolder As String = mcStartFolder
            Dim startFolderName As String = mcStartFolder.Replace("/", "\").Trim("\")
            startFolderName = startFolderName.Substring(startFolderName.LastIndexOf("\") + 1)
            Dim startfld As New DirectoryInfo(mcStartFolder)
            If Not startfld.Exists Then
                rootDir.CreateSubdirectory(startFolderName)
            End If

            For i = 0 To UBound(aFolderNames)
                If aFolderNames(i) <> "" Then
                    Dim dir1 As New DirectoryInfo(workingFolder)
                    If dir1.Exists Then
                        Dim dir2 As New DirectoryInfo(workingFolder.TrimEnd("\") & "\" & aFolderNames(i))
                        If Not dir2.Exists Then
                            dir1.CreateSubdirectory(CStr(aFolderNames(i)))
                        End If
                    End If
                    workingFolder = workingFolder & "\" & aFolderNames(i)
                End If
            Next

            PerfMon.Log("fsHelper", "CreatePath-End", cFolderPath)

            Return "1"

        Catch ex As Exception
            Return ex.Message & " - " & cFolderPath
        End Try

    End Function

    Public Function getUniqueFilename(ByVal cFullPath As String) As String
        Dim nCount As Int32 = 0
        Dim bFileExists As Boolean = True
        Dim cFileNameExt As String
        Dim cFilePathFull As String = cFullPath
        cFilePathFull = cFilePathFull.Replace("/", "\")
        Dim cFilePath As String
        Dim cFilePathNew As String = ""
        Dim cFileName As String = ""
        Dim cProcessInfo As String = "getUniqueFilename"

        Try
            'get file extension and path
            Dim nDotPos As Int32 = InStrRev(cFilePathFull, ".")
            Dim nSlashPos As Int32 = InStrRev(cFilePathFull, "\")
            cFilePath = cFilePathFull.Substring(0, nDotPos - 1)
            cFileName = cFilePath.Substring(nSlashPos, cFilePath.Length - nSlashPos)
            cFilePath = cFilePathFull.Substring(0, nSlashPos - 1)
            cFileNameExt = cFilePathFull.Substring(nDotPos, cFilePathFull.Length - nDotPos)

            Do
                If nCount = 0 Then
                    'first time, try the plain file name 
                    cFilePathNew = cFilePath & "\" & cFileName & "." & cFileNameExt
                Else
                    cFilePathNew = cFilePath & "\" & cFileName & "-" & nCount.ToString & "." & cFileNameExt
                End If
                bFileExists = System.IO.File.Exists(cFilePathNew)
                nCount += 1
            Loop Until System.IO.File.Exists(cFilePathNew) = False

            'remove the qualifying path and tidy up for returning
            'this line returns the filename only which we don't need
            ' cFilePathNew = cFilePathNew.Replace(cFilePath & "\", "")
            cFilePathNew = cFilePathNew.Replace("\", "/")
            Return cFilePathNew

        Catch ex As Exception
            returnException(mcModuleName, "getUniqueFilename", ex, "", cProcessInfo, gbDebug)
            Return Nothing
        End Try

    End Function



    Public Function DeleteFolder(ByVal cFolderName As String, ByVal cFolderPath As String) As String
        PerfMon.Log("fsHelper", "DeleteFolder")
        'in order to make this work the root directory needs to have read permissions for everyone or at lease asp.net acct
        Try

            Dim oImp As Protean.Tools.Security.Impersonate = New Protean.Tools.Security.Impersonate
            If oImp.ImpersonateValidUser(goConfig("AdminAcct"), goConfig("AdminDomain"), goConfig("AdminPassword"), , goConfig("AdminGroup")) Then

                Dim dir As New DirectoryInfo(mcStartFolder & cFolderPath & "\" & cFolderName)
                If dir.Exists Then

                    'FIX disable AppDomain restart when deleting subdirectory
                    'This code will turn off monitoring from the root website directory.
                    'Monitoring of Bin, App_Themes and other folders will still be operational, so updated DLLs will still auto deploy.
                    Dim p As System.Reflection.PropertyInfo = GetType(System.Web.HttpRuntime).GetProperty("FileChangesMonitor", System.Reflection.BindingFlags.NonPublic Or System.Reflection.BindingFlags.[Public] Or System.Reflection.BindingFlags.[Static])
                    Dim o As Object = p.GetValue(Nothing, Nothing)
                    Dim f As System.Reflection.FieldInfo = o.[GetType]().GetField("_dirMonSubdirs", System.Reflection.BindingFlags.Instance Or System.Reflection.BindingFlags.NonPublic Or System.Reflection.BindingFlags.IgnoreCase)
                    Dim monitor As Object = f.GetValue(o)
                    Dim m As System.Reflection.MethodInfo = monitor.[GetType]().GetMethod("StopMonitoring", System.Reflection.BindingFlags.Instance Or System.Reflection.BindingFlags.NonPublic)
                    m.Invoke(monitor, New Object() {})

                    dir.Delete(True)
                Else
                    Return "this folder does not exist"
                End If
                oImp.UndoImpersonation()
            Else
                Return "Server admin permissions are not configured"
            End If
            Return "1"
        Catch ex As Exception
            Return ex.Message
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

    Public Function VirtualFileExistsAndRecent(ByVal cVirtualPath As String, ByVal hours As Long) As Integer
        Try
            Dim cVP As String = mcStartFolder & cVirtualPath.Replace("/", "\")
            If IO.File.Exists(cVP) And IO.File.GetLastWriteTime(cVP) > DateAdd(DateInterval.Hour, (hours * -1), Now()) Then
                Return 1
            Else
                Return 0
            End If
        Catch ex As Exception
            Return 0
        End Try
    End Function

    Public Function CompareDateIsNewer(ByVal cOriginalPath As String, ByVal cCheckNewerPath As String) As Integer
        Try
            Dim cOP As String = goServer.MapPath(cOriginalPath)
            Dim cCNP As String = goServer.MapPath(cCheckNewerPath)
            If IO.File.GetCreationTime(cCNP) > IO.File.GetCreationTime(cOP) Then
                Return 1
            Else
                Return 0
            End If
        Catch ex As Exception
            Return 0
        End Try
    End Function

    Public Function DeleteFile(ByVal cFolderPath As String, ByVal cFileName As String) As String
        PerfMon.Log("fsHelper", "DeleteFile")
        Try
            Dim oImp As Protean.Tools.Security.Impersonate = New Protean.Tools.Security.Impersonate
            If oImp.ImpersonateValidUser(goConfig("AdminAcct"), goConfig("AdminDomain"), goConfig("AdminPassword"), , goConfig("AdminGroup")) Then
                Dim cFullFileName As String = mcStartFolder & cFolderPath & "\" & cFileName
                If IO.File.Exists(cFullFileName) Then
                    Dim oFileInfo As IO.FileInfo = New IO.FileInfo(cFullFileName)
                    oFileInfo.IsReadOnly = False
                    IO.File.Delete(cFullFileName)
                Else
                    Return "this file does not exist"
                End If
                oImp.UndoImpersonation()
            Else
                Return "Server admin permissions are not configured"
            End If
            Return "1"
        Catch ex As Exception
            Return ex.Message
        End Try
    End Function

    Public Function DeleteFile(ByVal cFullFilePath As String) As String
        PerfMon.Log("fsHelper", "DeleteFile")
        Try
            Dim oImp As Protean.Tools.Security.Impersonate = New Protean.Tools.Security.Impersonate
            If oImp.ImpersonateValidUser(goConfig("AdminAcct"), goConfig("AdminDomain"), goConfig("AdminPassword"), , goConfig("AdminGroup")) Then
                If IO.File.Exists(cFullFilePath) Then
                    Dim oFileInfo As IO.FileInfo = New IO.FileInfo(cFullFilePath)
                    oFileInfo.IsReadOnly = False
                    IO.File.Delete(cFullFilePath)
                Else
                    Return "this file does not exist"
                End If
                oImp.UndoImpersonation()
            Else
                Return "Server admin permissions are not configured"
            End If
            Return "1"
        Catch ex As Exception
            Return ex.Message
        End Try
    End Function

    Public Function SaveFile(ByRef FileName As String, ByVal cFolderPath As String, ByVal FileData As Byte()) As String
        PerfMon.Log("fsHelper", "SaveFile")
        Try
            'here we will fix any unsafe web charactors in the name
            FileName = Replace(FileName, " ", "-")

            cFolderPath = Replace(cFolderPath, "~", "")

            Dim dir As New DirectoryInfo(mcStartFolder & cFolderPath & "\")
            If Not dir.Exists Then
                CreatePath(cFolderPath)
            End If
            Dim myFile As FileStream = File.Create(mcStartFolder & cFolderPath & "\" & FileName)
            myFile.Write(FileData, 0, FileData.Length)
            myFile.Close()
            myFile = Nothing
            dir = Nothing

            Return cFolderPath & "/" & FileName

        Catch ex As Exception
            Return ex.Message
        End Try
    End Function

    Public Function SaveFile(ByRef postedFile As System.Web.HttpPostedFile, ByVal cFolderPath As String) As String
        PerfMon.Log("fsHelper", "SaveFile")
        Try
            Dim filename As String = Right(postedFile.FileName, postedFile.FileName.Length - postedFile.FileName.LastIndexOf("\") - 1)

            'here we will fix any unsafe web charactors in the name
            filename = Replace(filename, " ", "-")

            Dim dir As New DirectoryInfo(mcStartFolder & cFolderPath & "\")
            If dir.Exists Then
                postedFile.SaveAs(mcStartFolder & cFolderPath & "\" & filename)
                Return postedFile.FileName
            Else
                Return "this root folder does not exist:" & mcStartFolder
            End If

        Catch ex As Exception
            Return ex.Message
        End Try
    End Function

    Public Function SaveFile(ByVal httpURL As String, ByVal cFolderPath As String) As String
        PerfMon.Log("fsHelper", "SaveFile")
        Dim response As System.Net.WebResponse = Nothing
        Dim remoteStream As Stream
        Dim readStream As StreamReader
        Dim request As System.Net.WebRequest
        Dim img As System.Drawing.Image
        Try
            httpURL = httpURL.Replace("\", "/")
            Dim filename As String = Right(httpURL, httpURL.Length - httpURL.LastIndexOf("/") - 1)
            'here we will fix any unsafe web charactors in the name
            filename = Replace(filename, " ", "-")

            If IO.File.Exists(mcStartFolder & cFolderPath & "\" & filename) Then
                Return Replace(Replace(cFolderPath, "..\", "/"), "\", "/") & "/" & filename
            Else
                request = System.Net.WebRequest.Create(httpURL)
                Try
                    response = request.GetResponse()
                Catch ex As System.Net.WebException
                    Dim errResp As System.Net.HttpWebResponse = ex.Response
                    If ex.Status = System.Net.WebExceptionStatus.ProtocolError Then
                        Return errResp.StatusCode.ToString()
                    End If
                End Try

                If Not response Is Nothing Then
                    remoteStream = response.GetResponseStream
                    Try
                        img = System.Drawing.Image.FromStream(remoteStream)
                    Catch ex2 As Exception
                        Dim test As String = ex2.Message

                    End Try


                    Me.CreatePath(cFolderPath & "\")

                    Dim dir As New DirectoryInfo(mcStartFolder & cFolderPath & "\")
                    If dir.Exists Then
                        Select Case Right(httpURL, httpURL.Length - httpURL.LastIndexOf(".") - 1)
                            Case "gif"
                                img.Save(mcStartFolder & cFolderPath & "\" & filename, System.Drawing.Imaging.ImageFormat.Gif)
                                Return Replace(Replace(cFolderPath, "..\", "/"), "\", "/") & "/" & filename
                            Case "jpg"
                                img.Save(mcStartFolder & cFolderPath & "\" & filename, System.Drawing.Imaging.ImageFormat.Jpeg)
                                Return Replace(Replace(cFolderPath, "..\", "/"), "\", "/") & "/" & filename
                            Case "png"
                                img.Save(mcStartFolder & cFolderPath & "\" & filename, System.Drawing.Imaging.ImageFormat.Png)
                                Return Replace(Replace(cFolderPath, "..\", "/"), "\", "/") & "/" & filename
                            Case Else
                                Return "filetype not handled:" & filename
                        End Select

                    Else
                        Return "this root folder does not exist:" & mcStartFolder
                    End If
                    response.Close()
                    remoteStream.Close()
                    img.Dispose()
                End If
            End If


        Catch ex As Exception
            Return ex.Message
        Finally
            response = Nothing
            remoteStream = Nothing
            readStream = Nothing
            img = Nothing
        End Try
    End Function


    Public Function CopyFile(ByVal FileName As String, ByVal cFolderSource As String, ByVal cFolderDestination As String, Optional ByVal bOverwrite As Boolean = False) As Boolean
        PerfMon.Log("fsHelper", "SaveFile")
        Try

            'here we will fix any unsafe web charactors in the name
            FileName = Replace(FileName, " ", "-")

            Dim dir As New DirectoryInfo(goServer.MapPath("/") & cFolderSource)
            Dim DestDir As New DirectoryInfo(goServer.MapPath("/") & cFolderDestination)

            If dir.Exists And DestDir.Exists Then
                IO.File.Copy(dir.FullName & "/" & FileName, DestDir.FullName & "\" & FileName, bOverwrite)
                Return True
            Else
                Return False
            End If

        Catch ex As Exception
            Return ex.Message
        End Try
    End Function

    Public Function MoveFile(ByVal FileName As String, ByVal cFolderSource As String, ByVal cFolderDestination As String, Optional ByVal bOverwrite As Boolean = False) As Boolean
        PerfMon.Log("fsHelper", "SaveFile")
        Try

            Dim dir As New DirectoryInfo(goServer.MapPath("/" & mcRoot) & cFolderSource.Replace("/", "\"))
            Dim DestDir As New DirectoryInfo(goServer.MapPath("/" & mcRoot) & cFolderDestination.Replace("/", "\"))

            If dir.Exists And DestDir.Exists Then
                IO.File.Copy(dir.FullName & "\" & FileName, DestDir.FullName & "\" & FileName.Replace(" ", "-"), bOverwrite)
                IO.File.Delete(dir.FullName & "\" & FileName)
                Return True
            Else
                Return False
            End If

        Catch ex As Exception
            Return ex.Message
        End Try
    End Function

    Public Function GetFileStream(ByVal FilePath As String) As FileStream
        PerfMon.Log("GetFileStream", "SaveFile")
        Try
            Dim oImp As Protean.Tools.Security.Impersonate = New Protean.Tools.Security.Impersonate
            If oImp.ImpersonateValidUser(goConfig("AdminAcct"), goConfig("AdminDomain"), goConfig("AdminPassword"), , goConfig("AdminGroup")) Then
                Dim oFileStream As FileStream = New FileStream(FilePath, FileMode.Open, FileAccess.Read)
                Return oFileStream

                oImp.UndoImpersonation()
            Else
                Return Nothing
            End If
        Catch ex As Exception
            Return Nothing
        End Try

    End Function

    Public Function EnumerateFolders(ByVal folderLibrary As LibraryType) As Generic.List(Of String)

        Try
            Me.initialiseVariables(folderLibrary)
            Dim dir As New DirectoryInfo(mcRoot)
            Return EnumerateFolders(dir)
        Catch ex As Exception
            Return New Generic.List(Of String)
        End Try

    End Function

    Public Function EnumerateFolders(ByVal path As String) As Generic.List(Of String)

        Try
            Dim dir As New DirectoryInfo(goServer.MapPath(path))
            Return EnumerateFolders(dir)
        Catch ex As Exception
            Return New Generic.List(Of String)
        End Try

    End Function

    Public Function FindFilePathInCommonFolders(ByVal pathToCheck As String, ByVal foldersToCheck As String()) As String
        Return FindFilePathInCommonFolders(pathToCheck, New List(Of String)(foldersToCheck))
    End Function

    ''' <summary>
    ''' Given a file path and a list of other folders to check, this returns the first available file that actually exists
    ''' Precedence is inferred in the order of the list (path to check first, then folders in list + path to check)
    ''' </summary>
    ''' <param name="pathToCheck"></param>
    ''' <param name="foldersToCheck"></param>
    ''' <returns>Returns the first existing path found, or an empty string if not found.</returns>
    ''' <remarks></remarks>
    Public Function FindFilePathInCommonFolders(ByVal pathToCheck As String, ByVal foldersToCheck As List(Of String)) As String

        Try
            Dim pathsToCheck As New List(Of String)
            pathToCheck = "/" & pathToCheck.TrimStart("/\".ToCharArray())
            pathsToCheck.Add(pathToCheck)
            pathsToCheck.AddRange(foldersToCheck.ConvertAll(Function(folderToCheck As String) (folderToCheck.TrimEnd("/\".ToCharArray()) & pathToCheck)))
            Return pathsToCheck.Find(Function(path As String) (IO.File.Exists(goServer.MapPath(path))))
        Catch ex As Exception
            Return ""
        End Try

    End Function

    Public Function OptimiseImages(ByVal path As String, Optional ByRef nFileCount As Long = 0, Optional ByRef nSavings As Long = 0, ByVal Optional lossless As Boolean = True) As String
        Try
            Dim thisDir As New DirectoryInfo(goServer.MapPath(path))
            Dim ofile As FileInfo
            Dim ofolder As DirectoryInfo

            Dim nLengthBefore As Long = 0

            For Each ofolder In thisDir.GetDirectories()
                OptimiseImages(path & "/" & ofolder.Name, nFileCount, nSavings, lossless)
            Next

            For Each ofile In thisDir.GetFiles
                Dim oImgTool As New Protean.Tools.Image("")
                nSavings = nSavings + oImgTool.CompressImage(ofile, lossless)
                nFileCount = nFileCount + 1
            Next

            Return nFileCount & " Files Updated " & nSavings / 1024 & " Kb have been saved"

        Catch ex As Exception
            Return ex.Message
        End Try

    End Function

#End Region
#Region "Private Methods"
    Private Function AddElements(ByVal startNode As XmlElement, ByVal Folder As String) As XmlElement
        '  PerfMon.Log("fsHelper", "AddElements", Folder)
        Try
            Dim dir As New DirectoryInfo(Folder)
            Dim subDirs As DirectoryInfo() = dir.GetDirectories()
            Dim files As FileInfo() = dir.GetFiles()
            Dim fi As FileInfo
            Dim sVirtualPath As String

            If mcStartFolder.Contains("..") Then
                'we have a virtual path and we need to be a bit more cleverer
                sVirtualPath = mcStartFolder.Substring(mcStartFolder.LastIndexOf(".."))
                Dim aPath() As String = sVirtualPath.Split("\")
                sVirtualPath = Folder.Substring(CLng(Folder.LastIndexOf(aPath(1)) + aPath(1).Length))
            Else
                sVirtualPath = Replace(Folder, mcStartFolder, "")
            End If

            If mcPopulateFilesNode = sVirtualPath Then
                startNode.SetAttribute("active", "true")
                For Each fi In files
                    If Not (Left(fi.Name, 5) = "Icon_") And Not (fi.Name.ToLower = "thumbs.db") And Not (fi.Name.ToLower = ".ds_store") Then
                        Dim cExt As String = LCase(fi.Extension)
                        Dim fileElem As XmlElement = XmlElement("file", fi.Name)
                        fileElem.Attributes.Append(XmlAttribute("Extension", cExt))
                        PerfMon.Log("fsHelper", "AddElements-Addfile", fi.Name)
                        Select Case cExt

                            Case ".jpg", ".gif", ".jpeg", ".png", ".bmp"
                                Try
                                    Dim oWebFile As New WebFile(fi.FullName, sVirtualPath & "/" & fi.Name, True)
                                    fileElem.Attributes.Append(XmlAttribute("height", oWebFile.ExtendedProperties.Height))
                                    fileElem.Attributes.Append(XmlAttribute("width", oWebFile.ExtendedProperties.Width))
                                Catch
                                    'do nothin
                                End Try

                            Case Else

                                Dim cIcon As String = "Icon_"
                                Select Case LCase(fi.Extension)
                                    Case ".doc", ".rtf"
                                        cIcon &= "WRD"
                                    Case ".xls", ".csv"
                                        cIcon &= "XL"
                                    Case ".pdf"
                                        cIcon &= "PDF"
                                        'Case ".bmp"
                                        '    cIcon &= "IMG"
                                    Case ".mpg", ".avi"
                                        cIcon &= "MV"
                                    Case ".wmv"
                                        cIcon &= "WMV"
                                    Case Else
                                        cIcon &= "OT"
                                End Select
                                cIcon = cIcon & ".gif"

                                'If File.Exists(mcStartFolder & "\" & cIcon) Then
                                '    fileElem.SetAttribute("icon", cIcon)
                                'End If

                                If IO.File.Exists(goServer.MapPath(goConfig("ProjectPath") & "/ewcommon/images/icons/" & cIcon)) Then
                                    fileElem.SetAttribute("icon", cIcon)
                                End If
                        End Select

                        fileElem.SetAttribute("root", mcRoot)
                        startNode.AppendChild(fileElem)
                    End If
                Next fi
            End If

            Dim sd As DirectoryInfo
            For Each sd In subDirs
                If sd.Name <> "_vti_cnf" And Not (sd.Name.StartsWith("~")) Then
                    Dim folderElem As XmlElement = XmlElement("folder", Replace(sd.Name, "\", "/"))
                    Dim sPath As String

                    If mcStartFolder.Contains("..") Then
                        'we have a virtual path and we need to be a bit more cleverer
                        sPath = mcStartFolder.Substring(mcStartFolder.LastIndexOf(".."))
                        Dim aPath() As String = sPath.Split("\")
                        sPath = sd.FullName.Substring(CLng(sd.FullName.LastIndexOf(aPath(1)) + aPath(1).Length))
                    Else
                        sPath = Replace(sd.FullName, mcStartFolder, "")
                    End If

                    folderElem.Attributes.Append(XmlAttribute("path", sPath))
                    'folderElem.Attributes.Append(XmlAttribute("Hidden",(If(sd.Attributes And FileAttributes.Hidden) <> 0 Then "Y" Else "N"))) 'TODO: Unsupported feature: conditional (?) operator.
                    'folderElem.Attributes.Append(XmlAttribute("System",(If(sd.Attributes And FileAttributes.System) <> 0 Then "Y" Else "N"))) 'TODO: Unsupported feature: conditional (?) operator.
                    'folderElem.Attributes.Append(XmlAttribute("ReadOnly",(If(sd.Attributes And FileAttributes.ReadOnly) <> 0 Then "Y" Else "N"))) 'TODO: Unsupported feature: conditional (?) operator.
                    startNode.AppendChild(AddElements(folderElem, sd.FullName))
                End If
            Next sd
            Return startNode
        Catch ex As Exception
            Return XmlElement("error", ex.Message)
        End Try
    End Function 'AddElements

    Private Function XmlAttribute(ByVal attributeName As String, ByVal attributeValue As String) As XmlAttribute
        ' PerfMon.Log("fsHelper", "XmlAttribute", attributeName & " - " & attributeValue)
        Dim xmlAttrib As XmlAttribute = moPageXML.CreateAttribute(attributeName)
        xmlAttrib.Value = FilterXMLString(attributeValue)
        Return xmlAttrib
    End Function 'XmlAttribute

    Private Function XmlElement(ByVal elementName As String, ByVal elementValue As String) As XmlElement
        ' PerfMon.Log("fsHelper", "XmlElement", elementName & " - " & elementValue)
        Dim oElmt As XmlElement = moPageXML.CreateElement(elementName)
        oElmt.Attributes.Append(XmlAttribute("name", FilterXMLString(elementValue)))
        Return oElmt
    End Function

    Private Function FilterXMLString(ByVal inputString As String) As String
        'PerfMon.Log("fsHelper", "FilterXMLString")
        Dim returnString As String = inputString
        If inputString.IndexOf("&") > 0 Then
            returnString = inputString.Replace("&", "&amp;")
        End If
        If inputString.IndexOf("'") > 0 Then
            returnString = inputString.Replace("'", "&apos;")
        End If
        Return returnString
    End Function

#End Region
#Region "Shared Methods"

    Shared Function PathToURL(ByVal path As String) As String
        Try
            Return path.Replace("\", "/")
        Catch ex As Exception
            Return path
        End Try
    End Function
    Shared Function URLToPath(ByVal path As String) As String
        Try
            Return path.Replace("/", "\")
        Catch ex As Exception
            Return path
        End Try
    End Function


    Shared Function EnumerateFolders(ByVal dir As DirectoryInfo) As Generic.List(Of String)

        Dim folderList As New Collections.Generic.List(Of String)

        Try
            ' .NET4  Upgrade scope: could use LINQ to effectively filter these in one go
            If dir.Exists() Then
                folderList.Add(dir.FullName)

                For Each childDir As DirectoryInfo In dir.GetDirectories()

                    ' Exclude system and hidden folders, plus frontpage and eonicweb thumbnail
                    If ((childDir.Attributes And FileAttributes.System) = 0) _
                        And ((childDir.Attributes And FileAttributes.Hidden) = 0) _
                        And Not (childDir.Name.StartsWith("~")) _
                        And Not (childDir.Name.StartsWith("_vti")) _
                        And Not (childDir.Name.StartsWith("_private")) _
                       Then

                        folderList.AddRange(EnumerateFolders(childDir))

                    End If

                Next
            End If


        Catch ex As Exception

        End Try


        Return folderList

    End Function

    Shared Function GetFileLibraryPath(ByVal library As LibraryType) As String

        Dim path As String = ""
        Try

            Dim config As Specialized.NameValueCollection = Protean.Cms.Config()

            Select Case library
                Case LibraryType.Image
                    path = Tools.Text.Coalesce(Protean.Cms.ConfigValue("ImageRootPath"), "/images")
                Case LibraryType.Documents
                    path = Tools.Text.Coalesce(Protean.Cms.ConfigValue("DocRootPath"), "/docs")
                Case LibraryType.Media
                    path = Tools.Text.Coalesce(Protean.Cms.ConfigValue("MediaRootPath"), "/media")
                Case LibraryType.Scripts
                    path = Tools.Text.Coalesce(Protean.Cms.ConfigValue("ScriptsRootPath"), "/js")
                Case LibraryType.Style
                    path = Tools.Text.Coalesce(Protean.Cms.ConfigValue("StyleRootPath"), "/css")
            End Select

            path = Replace(path, "\", "/")

            'remove trailing slash

            If path.EndsWith("/") Then
                path = path.TrimEnd("/")
            End If

        Catch ex As Exception
            path = ""
        End Try

        Return path

    End Function

    ''' <summary>
    ''' Gets a filtered list of files in a given folder.
    ''' </summary>
    ''' <param name="physicalFolderPath">The physical location of the folder to inspect (not a relative path)</param>
    ''' <param name="libraryType">The library type to filter by</param>
    ''' <param name="includeSubfolders">Determine whether to just look at the immediate folder or inspect sub folders</param>
    ''' <returns>The List(Of String) of physical path of the files available with the filters above applied</returns>
    ''' <remarks>If includeSubfolders is on it may include thumbnail folders' contents</remarks>
    Shared Function GetFileListByTypeFromPhysicalPath(ByVal physicalFolderPath As String, ByVal libraryType As LibraryType, Optional ByVal includeSubfolders As Boolean = False) As List(Of String)

        Dim fileList As New List(Of String)
        Dim folder As New DirectoryInfo(physicalFolderPath)

        If folder.Exists() Then
            ' Filter out the files by type and return the full name
            Dim fileInfoList As New List(Of FileInfo)(folder.GetFiles("*.*", IIf(includeSubfolders, SearchOption.AllDirectories, SearchOption.TopDirectoryOnly)))
            fileInfoList = fileInfoList.FindAll(New PredicateWrapper(Of FileInfo, LibraryType)(libraryType, AddressOf FileInfoTypeFilter))
            fileList = fileInfoList.ConvertAll(New Converter(Of FileInfo, String)(AddressOf FullNameFromFileInfo))
        End If

        Return fileList
    End Function

    ''' <summary>
    ''' Gets a list of files for a library type from a path relative to Server.MapPath
    ''' </summary>
    ''' <param name="relativeFolderPath"></param>
    ''' <param name="libraryType"></param>
    ''' <param name="includeSubfolders"></param>
    ''' <returns></returns>
    ''' <remarks>Note that this may not work if you have nested virtual directories</remarks>
    Public Function GetFileListByTypeFromRelativePath(ByVal relativeFolderPath As String, ByVal libraryType As LibraryType, Optional ByVal includeSubfolders As Boolean = False) As List(Of String)
        PerfMon.Log("fsHelper", "GetFileListByTypeFromRelativePath")

        Dim fileList As New List(Of String)

        Try

            ' Work out the relative path

            ' First out find the root folder physical path
            ' Basically we want to account for virtual directories
            relativeFolderPath = relativeFolderPath.Trim("/\".ToCharArray)
            Dim rootFolderPath As String = ""
            If Not String.IsNullOrEmpty(relativeFolderPath) Then
                rootFolderPath = Protean.Tools.Text.SimpleRegexFind(relativeFolderPath, "^([^/]+?)(/.*)?$", 1)
            End If
            Dim rootFolder As New DirectoryInfo(goServer.MapPath("/" & rootFolderPath))
            Dim folderToInspect As New DirectoryInfo(goServer.MapPath("/" & relativeFolderPath))

            If rootFolder.Exists Then
                ' Get the list of files for the folder
                fileList = GetFileListByTypeFromPhysicalPath(folderToInspect.FullName, libraryType, includeSubfolders)
                fileList = fileList.ConvertAll(Function(physicalPath As String) _
                                                    ConvertPhysicalFilePathToVirtualPath(physicalPath, rootFolder.FullName, rootFolderPath))
            End If


        Catch ex As Exception

        End Try

        Return fileList
    End Function


    ''' <summary>
    ''' Converts a physical path to a virtual path.  Virtual path information is provided in the parameters.
    ''' </summary>
    ''' <param name="physicalFilePath">The physical path to inspect (e.g. c:\temp\myimages\myfolder\mypath)</param>
    ''' <param name="rootFolderPhysicalPath">The physical path of the virtual root folder (e.g. c:\temp\myimages)</param>
    ''' <param name="rootFolderVirtualPath">The virtual path of the virtual root folder (not / but /images for example)</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Shared Function ConvertPhysicalFilePathToVirtualPath(ByVal physicalFilePath As String, ByVal rootFolderPhysicalPath As String, ByVal rootFolderVirtualPath As String) As String

        Return "/" & rootFolderVirtualPath & physicalFilePath.Replace(rootFolderPhysicalPath.TrimEnd("/\".ToCharArray()), "").Replace("\", "/")

    End Function


    ''' <summary>
    ''' Determines whether a given file (FileInfo) is a valid file for the Protean.fsHelper.LibraryType.
    ''' The filter includes inspecting the file extension, the file attributes (system and hidden are ignored)
    ''' and whether the filename begins ~, which is an EonicWeb generated image so can be ignored.
    ''' </summary>
    ''' <param name="fileToInspect">The file to apply the filter against</param>
    ''' <param name="libraryTypeFilter">The Protean.fsHelper.LibraryType to filter against.</param>
    ''' <returns>True if the file is a valid file for the given library type, False otherwise.</returns>
    ''' <remarks></remarks>
    Shared Function FileInfoTypeFilter(ByVal fileToInspect As FileInfo, ByVal libraryTypeFilter As LibraryType) As Boolean

        Dim filter As New List(Of String)(_libraryTypeExtensions(libraryTypeFilter))

        Return ((fileToInspect.Attributes And FileAttributes.System) = 0) _
            And ((fileToInspect.Attributes And FileAttributes.Hidden) = 0) _
            And Not (fileToInspect.Name.StartsWith("~")) _
            And (filter.Count = 0 Or filter.Contains(fileToInspect.Extension.ToLower.TrimStart(".")))

    End Function

    ''' <summary>
    ''' Gets the full file path for a given file.
    ''' </summary>
    ''' <param name="fileToInspect"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Shared Function FullNameFromFileInfo(ByVal fileToInspect As FileInfo) As String
        Return fileToInspect.FullName
    End Function

    Shared Function GetLibraryTypeFromExtension(ByVal extension As String) As LibraryType
        extension = extension.TrimStart(".")
        Dim typeIndex As Integer = Array.FindIndex(_libraryTypeExtensions, Function(typeExtensions As String()) (Array.IndexOf(typeExtensions, extension) > -1))
        If typeIndex = -1 Then
            Return LibraryType.Undefined
        Else
            Return typeIndex
        End If
    End Function

    Shared Function GetDefaultContentSchemaNamesForLibraryType(ByVal libraryType As LibraryType) As String
        Return _defaultLibraryTypeContentSchemaNames(libraryType)
    End Function

#End Region

End Class



