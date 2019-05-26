'***********************************************************************
' $Library:     eonic.providers.messaging.base
' $Revision:    3.1  
' $Date:        2010-03-02
' $Author:      Trevor Spink (trevor@eonic.co.uk)
' &Website:     www.eonic.co.uk
' &Licence:     All Rights Reserved.
' $Copyright:   Copyright (c) 2002 - 2010 Eonic Ltd.
'***********************************************************************

Option Strict Off
Option Explicit On

Imports System
Imports System.Xml
Imports System.Web.HttpUtility
Imports System.Web.Configuration
Imports System.Configuration
Imports System.IO
Imports System.Collections
Imports System.Data
Imports System.Data.SqlClient
Imports System.Text.RegularExpressions
Imports System.Threading
Imports Protean.Cms
Imports Protean.Tools
Imports Protean.Tools.Xml
Imports Protean.CMS.Cart
Imports System.Net.Mail
Imports System.Reflection
Imports System.Net
Imports VB = Microsoft.VisualBasic
Imports Microsoft.WindowsAzure.MediaServices.Client
Imports System.Linq

Namespace Providers
    Namespace Streaming

        Public Class AzureStreaming



            Public Sub New()
                'do nothing
            End Sub

            Public Sub Initiate(ByRef _AdminXforms As Object, ByRef _AdminProcess As Object, ByRef _Activities As Object, ByRef MemProvider As Object, ByRef myWeb As Protean.CMS)

                MemProvider.AdminXforms = New AdminXForms(myWeb)
                MemProvider.AdminProcess = New AdminProcess(myWeb)
                MemProvider.AdminProcess.oAdXfm = MemProvider.AdminXforms
                MemProvider.Activities = New Activities()

            End Sub

            Public Class AdminXForms
                Inherits Cms.Admin.AdminXforms
                Private Const mcModuleName As String = "Eonic.Providers.Streaming.AdminXForms"

                Sub New(ByRef aWeb As Cms)
                    MyBase.New(aWeb)
                End Sub

            End Class

            Public Class AdminProcess
                Inherits Cms.Admin

                Dim _oAdXfm As Protean.Providers.Streaming.AzureStreaming.AdminXForms

                Public Property oAdXfm() As Object
                    Set(ByVal value As Object)
                        _oAdXfm = value
                    End Set
                    Get
                        Return _oAdXfm
                    End Get
                End Property

                Sub New(ByRef aWeb As Cms)
                    MyBase.New(aWeb)
                End Sub
            End Class


            Public Class Activities
                Private Const mcModuleName As String = "Eonic.Providers.Streaming.AzureStreaming.Activities"



            End Class

            Public Class Modules

                Public Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
                Private Const mcModuleName As String = "Eonic.Providers.Streaming.AzureStreaming.Modules"

                Dim moStreamingConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("eonic/streaming")


                Public Sub New()

                    'do nowt

                End Sub

                Public Sub GetSteamURL(ByRef myWeb As Protean.CMS, ByRef oContentNode As XmlElement)
                    Try
                        Dim html5Elmt As XmlElement
                        Dim assetId As String
                        html5Elmt = oContentNode.SelectSingleNode("HTML5")
                        assetId = html5Elmt.GetAttribute("azureId")
                        Dim dStreamExpires As Date
                        If html5Elmt.GetAttribute("streamExpires") = "" Then
                            dStreamExpires = Now()
                        Else
                            dStreamExpires = CDate(html5Elmt.GetAttribute("streamExpires"))
                        End If

                        If dStreamExpires < Now() Or LCase(myWeb.moRequest("refresh")) = "true" Then
                            Dim streamingUrl As String = RefreshStreamURL(myWeb, oContentNode)

                            If streamingUrl <> "" Then

                                'now update the database
                                Dim oInstance As XmlElement = myWeb.moPageXml.CreateElement("Instance")
                                oInstance.InnerXml = myWeb.moDbHelper.getObjectInstance(dbHelper.objectTypes.Content, CLng(oContentNode.GetAttribute("id")))
                                '   oContentNode.RemoveAttribute("id")
                                '  oContentNode.RemoveAttribute("parId")
                                '  oContentNode.RemoveAttribute("name")
                                '  oContentNode.RemoveAttribute("type")
                                '  oContentNode.RemoveAttribute("publish")
                                '  oContentNode.RemoveAttribute("update")
                                '  oContentNode.RemoveAttribute("owner")
                                '  oContentNode.RemoveAttribute("status")
                                '  oInstance.SelectSingleNode("*/cContentXmlDetail").InnerXml = oContentNode.OuterXml

                                ' replace the html5 node in the Brief
                                Dim briefHTML5 As XmlElement = oContentNode.SelectSingleNode("HTML5")

                                Dim html5Elmt2 As XmlElement
                                For Each html5Elmt2 In oInstance.SelectNodes("tblContent/*/Content/HTML5")
                                    html5Elmt2.ParentNode.ReplaceChild(briefHTML5.CloneNode(True), html5Elmt2)
                                Next
                                'persist updated values
                                myWeb.moDbHelper.setObjectInstance(dbHelper.objectTypes.Content, oInstance, CLng(oContentNode.GetAttribute("id")))

                            End If

                        End If

                    Catch ex As Exception
                        RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetSteamURL", ex, ""))
                    End Try
                End Sub

                Private Function GetMediaContext() As CloudMediaContext
                    Try

                        Dim tokenCredentials As New AzureAdTokenCredentials(moStreamingConfig("ADDDomain"),
                        New AzureAdClientSymmetricKey(moStreamingConfig("clientId"), moStreamingConfig("clientKey")),
                        AzureEnvironments.AzureCloudEnvironment)

                        Dim tokenProvider As New AzureAdTokenProvider(tokenCredentials)

                        Dim streamingServer As New Uri(moStreamingConfig("RESTAPIuri"))
                        Dim context As CloudMediaContext = New CloudMediaContext(streamingServer, tokenProvider)

                        Return context

                    Catch ex As Exception
                        RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetMediaContext", ex, ""))
                    End Try

                End Function


                Public Sub uploadAndEncode(ByRef myWeb As Protean.Cms, ByRef oContentNode As XmlElement, ByVal nReturnId As Long, ByVal editResult As Protean.Cms.dbHelper.ActivityType)
                    Try
                        Dim html5Elmt As XmlElement
                        Dim sPath As String
                        Dim cProcessInfo As String

                        html5Elmt = oContentNode.SelectSingleNode("HTML5")
                        sPath = html5Elmt.GetAttribute("sourcePath")


                        If html5Elmt.GetAttribute("azureId") = "" Or myWeb.moRequest("reencode") <> "" Then
                            Dim uploadFilePath As String
                            If sPath.StartsWith("/media") Then
                                sPath = sPath.Replace("/media/", "/")
                                uploadFilePath = goServer.MapPath("/media") & goServer.UrlDecode(Protean.Tools.Xml.convertEntitiesToString(Protean.Tools.Xml.convertEntitiesToString(sPath)))
                            Else
                                uploadFilePath = goServer.MapPath("/") & goServer.UrlDecode(Protean.Tools.Xml.convertEntitiesToString(Protean.Tools.Xml.convertEntitiesToString(sPath)))
                            End If
                            uploadFilePath = uploadFilePath.Replace("/", "\")
                            uploadFilePath = uploadFilePath.Replace("\\", "\")

                            Dim context As CloudMediaContext = GetMediaContext()

                            'delete files with same name
                            Dim streamingAssetloop As IAsset
                            For Each streamingAssetloop In context.Assets
                                If Path.GetFileNameWithoutExtension(uploadFilePath) = streamingAssetloop.Name Then
                                    streamingAssetloop.Delete()
                                End If
                            Next

                            Dim uploadAsset As IAsset = context.Assets.Create(Path.GetFileNameWithoutExtension(uploadFilePath), AssetCreationOptions.None)
                            Dim assetFile As IAssetFile = uploadAsset.AssetFiles.Create(Path.GetFileName(uploadFilePath))

                            html5Elmt.SetAttribute("azureId", assetFile.Id)

                            assetFile.Upload(uploadFilePath)

                            ' Preset reference documentation: http://msdn.microsoft.com/en-us/library/windowsazure/jj129582.aspx
                            Dim encodingPreset As String = moStreamingConfig("encodingPreset")
                            If encodingPreset = "" Then encodingPreset = "H264 Broadband 720p"
                            Dim encodingDesc As String = encodingPreset

                            If encodingPreset.StartsWith("/") Then
                                encodingDesc = encodingPreset.Replace("/", "").Replace(".xml", "")
                                encodingPreset = File.ReadAllText(goServer.MapPath("/") & encodingPreset)
                            End If

                            If Not LCase(encodingPreset) = "none" Then

                                Dim encodeAssetId As String = assetFile.Id

                                Dim assetToEncode As IAsset = uploadAsset ' context.Assets.Where(Function(a) a.Id = encodeAssetId).FirstOrDefault()

                                If assetToEncode Is Nothing Then
                                    Throw New ArgumentException("Could not find assetId: " + encodeAssetId)
                                End If

                                Dim job As IJob = context.Jobs.Create("Encoding " & assetToEncode.Name & " to " & encodingDesc)

                                ' For Each p In context.MediaProcessors
                                ' cProcessInfo = p.Name
                                ' Next

                                Dim latestWameMediaProcessor As IMediaProcessor = (From p In context.MediaProcessors Where p.Name = moStreamingConfig("MediaEncoder")).ToList().OrderBy(Function(wame) New Version(wame.Version)).LastOrDefault()

                                Dim encodeTask As ITask = job.Tasks.AddNew("Encoding", latestWameMediaProcessor, encodingPreset, TaskOptions.None)
                                encodeTask.InputAssets.Add(assetToEncode)

                                Dim EncodedName As String = assetToEncode.Name + "-" + encodingDesc

                                'delete files with same name
                                For Each streamingAssetloop In context.Assets
                                    If EncodedName = streamingAssetloop.Name Then
                                        streamingAssetloop.Delete()
                                    End If
                                Next

                                Dim encodedAsset As IAsset = encodeTask.OutputAssets.AddNew(EncodedName, AssetCreationOptions.None)

                                '   job.StateChanged += New EventHandler(Of JobStateChangedEventArgs)(Function(sender, jsc) Console.WriteLine(String.Format("{0}" & vbLf & "  State: {1}" & vbLf & "  Time: {2}" & vbLf & vbLf, DirectCast(sender, IJob).Name, jsc.CurrentState, DateTime.UtcNow.ToString("yyyy_M_d_hhmmss"))))
                                job.Submit()
                                job.GetExecutionProgressTask(CancellationToken.None).Wait()

                                For Each streamingAssetloop In context.Assets
                                    If EncodedName = streamingAssetloop.Name Then
                                        encodedAsset = streamingAssetloop
                                    End If
                                Next

                                html5Elmt.SetAttribute("azureId", encodedAsset.Id)

                                ' perhaps we delete the uploaded file now ?
                                assetFile.Delete()
                                uploadAsset.Delete()

                            End If

                            RefreshStreamURL(myWeb, oContentNode)

                            ' replace the html5 node in the Brief
                            Dim oInstance As XmlElement = oContentNode.SelectSingleNode("ancestor::tblContent")
                            Dim briefHTML5 As XmlElement = oInstance.SelectSingleNode("cContentXmlBrief/Content/HTML5")
                            briefHTML5.ParentNode.ReplaceChild(oContentNode.SelectSingleNode("HTML5").Clone, briefHTML5)

                        End If

                    Catch ex As Exception
                        RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "uploadAndEncode", ex, ""))
                    End Try
                End Sub


                Private Function RefreshStreamURL(ByRef myWeb As Protean.Cms, ByRef oContentNode As XmlElement) As String

                    Dim accessPolicy As IAccessPolicy = Nothing
                    Dim processInfo As String = ""
                    Try

                        Dim html5Elmt As XmlElement
                        Dim assetId As String
                        html5Elmt = oContentNode.SelectSingleNode("HTML5")
                        assetId = html5Elmt.GetAttribute("azureId")

                        Dim context As CloudMediaContext = GetMediaContext()
                        Dim streamingAssetId As String = assetId

                        If Not streamingAssetId Is Nothing Then
                            ' streamingAssetId = streamingAssetId.Replace("nb:cid:", "")
                            ' "YOUR ASSET ID";
                            Dim daysStreamValid As Integer = CInt("0" & moStreamingConfig("daysStreamValid"))
                            If daysStreamValid = 0 Then daysStreamValid = 365
                            Dim daysForWhichStreamingUrlIsActive As Integer = daysStreamValid

                            Dim streamingAsset As IAsset = context.Assets.Where(Function(a) a.Id = streamingAssetId).FirstOrDefault()
                            Dim streamingAssetloop As IAsset
                            For Each streamingAssetloop In context.Assets
                                If streamingAssetId = streamingAssetloop.Id Then
                                    streamingAsset = streamingAssetloop
                                End If
                            Next

                            Dim ourLocator As ILocator = Nothing
                            Dim ap As IAccessPolicy
                            If Not streamingAsset Is Nothing Then
                                Dim looplocator As ILocator
                                For Each looplocator In streamingAsset.Locators

                                    If looplocator.Type = LocatorType.OnDemandOrigin Then
                                        If Not ourLocator Is Nothing Then
                                            'delete alternative locators
                                            looplocator.Delete()
                                        End If
                                        'lets take the first locator and update if nessesary
                                        If looplocator.ExpirationDateTime < DateAdd(DateInterval.Hour, -1, Now) Then
                                            looplocator.Update(DateAdd(DateInterval.Day, daysForWhichStreamingUrlIsActive, Now()))
                                        End If
                                        ourLocator = looplocator
                                    End If
                                Next

                                For Each ap In context.AccessPolicies
                                    'Find access policy matching content
                                    If streamingAsset.Name = ap.Name Then
                                        accessPolicy = ap
                                        Exit For
                                    End If
                                Next

                                If accessPolicy Is Nothing Then
                                    accessPolicy = context.AccessPolicies.Create(streamingAsset.Name, TimeSpan.FromDays(daysForWhichStreamingUrlIsActive), AccessPermissions.Read)
                                End If

                                Dim streamingUrl As String = String.Empty
                                Dim assetFiles As System.Collections.Generic.List(Of IAssetFile) = streamingAsset.AssetFiles.ToList()
                                Dim streamingAssetFile As IAssetFile = assetFiles.Where(Function(f) f.Name.ToLower().EndsWith("m3u8-aapl.ism")).FirstOrDefault()
                                If streamingAssetFile IsNot Nothing Then
                                    If ourLocator Is Nothing Then
                                        ourLocator = context.Locators.CreateLocator(LocatorType.OnDemandOrigin, streamingAsset, accessPolicy)
                                    End If

                                    Dim hlsUri As New Uri(ourLocator.Path + streamingAssetFile.Name + "/manifest(format=m3u8-aapl)")
                                    streamingUrl = hlsUri.ToString()
                                    html5Elmt.SetAttribute("hlsUri", streamingUrl)
                                End If
                                streamingAssetFile = assetFiles.Where(Function(f) f.Name.ToLower().EndsWith(".ism")).FirstOrDefault()
                                If streamingAssetFile IsNot Nothing Then
                                    If ourLocator Is Nothing Then
                                        ourLocator = context.Locators.CreateLocator(LocatorType.OnDemandOrigin, streamingAsset, accessPolicy)
                                    End If
                                    Dim smoothUri As New Uri(ourLocator.Path + streamingAssetFile.Name + "/manifest")
                                    streamingUrl = smoothUri.ToString()
                                    html5Elmt.SetAttribute("smoothUri", streamingUrl)
                                End If
                                streamingAssetFile = assetFiles.Where(Function(f) f.Name.ToLower().EndsWith(".mp4")).FirstOrDefault()
                                If streamingAssetFile IsNot Nothing Then
                                    If ourLocator Is Nothing Then
                                        ourLocator = context.Locators.CreateLocator(LocatorType.Sas, streamingAsset, accessPolicy)
                                    End If

                                    Dim mp4Uri As System.UriBuilder = New UriBuilder(ourLocator.Path)
                                    mp4Uri.Path += "/" + streamingAssetFile.Name
                                    streamingUrl = mp4Uri.ToString()
                                    html5Elmt.SetAttribute("mp4Uri", streamingUrl)
                                End If

                                If streamingUrl <> "" Then
                                    html5Elmt.SetAttribute("streamExpires", xmlDate(DateAdd(DateInterval.Day, daysStreamValid, Now())))
                                End If

                                Return streamingUrl
                            Else
                                Return ""
                            End If

                        Else
                            Return Nothing
                        End If

                    Catch ex As Exception
                        RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetSteamURL", ex, ""))
                        Return Nothing
                    Finally
                        If Not accessPolicy Is Nothing Then
                            '  accessPolicy.Delete()
                        End If

                    End Try

                End Function

            End Class

        End Class

    End Namespace

End Namespace
