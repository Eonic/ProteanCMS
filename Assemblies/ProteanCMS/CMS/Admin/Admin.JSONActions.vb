Option Strict Off
Option Explicit On
Imports System.Xml
Imports System.Collections
Imports System.Web.Configuration
Imports System.Data.SqlClient
Imports System.Web.HttpUtility
Imports VB = Microsoft.VisualBasic
Imports System.IO
Imports Protean.Tools.Xml
Imports Protean.Tools.Xml.XmlNodeState
Imports System
Imports TweetSharp
Imports System.Collections.Generic
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Partial Public Class Cms

    Partial Public Class Admin


#Region "JSON Actions"

        Public Class JSONActions
            Public Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
            Private Const mcModuleName As String = "Cms.Admin.JSONActions"
            Private moLmsConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/lms")
            Private myWeb As Protean.Cms
            Private myCart As Protean.Cms.Cart
            Public moAdXfm As Protean.Cms.xForm
            Public moAdminXfm As Protean.Cms.Admin.AdminXforms



            Public Sub New()
                Dim ctest As String = "this constructor is being hit" 'for testing
                myWeb = New Protean.Cms()
                myWeb.InitializeVariables()
                myWeb.Open()
                myCart = New Protean.Cms.Cart(myWeb)
                moAdXfm = New Protean.Cms.xForm(myWeb)
                moAdminXfm = myWeb.getAdminXform()
            End Sub

            Public Shadows Sub open(ByVal oPageXml As XmlDocument)
                Dim cProcessInfo As String = ""
                Try
                    moAdXfm.moPageXML = oPageXml

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "Open", ex, "", cProcessInfo, gbDebug)
                End Try
            End Sub

            Public Function ManageRedirects(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String
                Try

                    Dim PageId As Long = inputJson("pageId").ToObject(Of Long)
                    Dim RedirectType As String = inputJson("redirectType").ToObject(Of String)
                    Dim OldURL As String = inputJson("oldUrl").ToObject(Of String)
                    Dim NewURL As String = inputJson("newUrl").ToObject(Of String)

                    Dim ArticleId As Long
                    Dim NewPageName As String

                    'Validate that the user is an administrator with session user ID
                    If myWeb.mbAdminMode And myWeb.moSession("nUserId") > 0 And RedirectType <> "404" Then
                        Dim rewriteXml As New XmlDocument
                        rewriteXml.Load(myWeb.goServer.MapPath("/rewriteMaps.config"))

                        ''Check we do not have a redirect for the OLD URL allready. Remove if exists
                        Dim existingRedirects As XmlNodeList = rewriteXml.SelectNodes("rewriteMaps/rewriteMap[@name='" & RedirectType & "Redirect']/add[@key='" & OldURL & "']")
                        If Not existingRedirects Is Nothing Then
                            For Each existingNode As XmlNode In existingRedirects
                                existingNode.RemoveAll()
                                rewriteXml.Save(myWeb.goServer.MapPath("/rewriteMaps.config"))
                            Next
                        End If

                        'Add redirect
                        Dim oCgfSectPath As String = "rewriteMaps/rewriteMap[@name='" & RedirectType & "Redirect']"
                        Dim redirectSectionXmlNode As XmlNode = rewriteXml.SelectSingleNode(oCgfSectPath)
                        If Not redirectSectionXmlNode Is Nothing Then
                            Dim replacingElement As XmlElement = rewriteXml.CreateElement("RedirectInfo")
                            replacingElement.InnerXml = $"<add key='{OldURL}' value='{NewURL}'/>"

                            ' rewriteXml.SelectSingleNode(oCgfSectPath).FirstChild.AppendChild(replacingElement.FirstChild)
                            rewriteXml.SelectSingleNode(oCgfSectPath).AppendChild(replacingElement.FirstChild)
                            rewriteXml.Save(myWeb.goServer.MapPath("/rewriteMaps.config"))
                        End If

                        'Determine all the paths that need to be redirected
                        If RedirectType = "301" Then
                            'step through and create rules to deal with paths
                            Dim folderRules As New ArrayList
                            Dim rulesXml As New XmlDocument
                            rulesXml.Load(myWeb.goServer.MapPath("/RewriteRules.config"))
                            Dim insertAfterElment As XmlElement = rulesXml.SelectSingleNode("descendant-or-self::rule[@name='EW: 301 Redirects']")
                            Dim oRule As XmlElement

                            'For Each oRule In replacerNode.SelectNodes("add")
                            Dim CurrentRule As XmlElement = rulesXml.SelectSingleNode("descendant-or-self::rule[@name='Folder: " & OldURL & "']")
                            Dim newRule As XmlElement = rulesXml.CreateElement("newRule")
                            Dim matchString As String = OldURL
                            If matchString.StartsWith("/") Then
                                matchString = matchString.TrimStart("/")
                            End If
                            folderRules.Add("Folder: " & OldURL)
                            newRule.InnerXml = "<rule name=""Folder: " & OldURL & """><match url=""^" & matchString & "(.*)""/><action type=""Redirect"" url=""" & NewURL & "{R:1}"" /></rule>"
                            If CurrentRule Is Nothing Then
                                insertAfterElment.ParentNode.InsertAfter(newRule.FirstChild, insertAfterElment)
                            Else
                                CurrentRule.ParentNode.ReplaceChild(newRule.FirstChild, CurrentRule)
                            End If
                            'Next

                            For Each oRule In rulesXml.SelectNodes("descendant-or-self::rule[starts-with(@name,'Folder: ')]")
                                If Not folderRules.Contains(oRule.GetAttribute("name")) Then
                                    oRule.ParentNode.RemoveChild(oRule)
                                End If
                            Next

                            rulesXml.Save(myWeb.goServer.MapPath("/RewriteRules.config"))
                            myWeb.bRestartApp = True
                        End If
                    End If


                    Dim JsonResult As String = ""
                    Return JsonResult
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""))
                    Return ex.Message
                End Try
            End Function

            Public Function redirectPagination(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String

                Dim ConfigType As String = inputJson("redirectType").ToObject(Of String)
                Dim pageloadCount As Integer = inputJson("loadCount").ToObject(Of Integer)
                Dim oFrmElmt As XmlElement
                Dim cProcessInfo As String = ""
                Dim oFsh As fsHelper
                Dim xFormPath As String = "/xforms/config/" & ConfigType & ".xml"
                If Not moAdXfm.load(xFormPath, myWeb.maCommonFolders) Then

                    oFrmElmt = moAdXfm.addGroup(moAdXfm.moXformElmt, "Config", "", "ConfigSettings")
                    moAdXfm.addNote(oFrmElmt, xForm.noteTypes.Alert, xFormPath & " could not be found. - ")

                Else
                    Try
                        oFsh = New fsHelper
                        oFsh.open(moAdXfm.moPageXML)

                        moAdXfm.NewFrm("WebSettings")
                        moAdXfm.bProcessRepeats = False

                        Dim JsonResult As String = ""
                        Dim rewriteXml As New XmlDocument

                        rewriteXml.Load(myWeb.goServer.MapPath("/rewriteMaps.config"))

                        Dim oTemplateInstance As XmlElement = moAdXfm.moPageXML.CreateElement("Instance")
                        oTemplateInstance.InnerXml = moAdXfm.Instance.InnerXml

                        Dim oCgfSectName As String = "system.webServer"
                        Dim oCgfSectPath As String = "rewriteMaps/rewriteMap[@name='" & ConfigType & "']"
                        ' Dim oCgfSect As System.Configuration.DefaultSection = oCfg.GetSection(oCgfSectName)
                        cProcessInfo = "Getting Section Name:" & oCgfSectPath
                        Dim sectionMissing As Boolean = False

                        If Not rewriteXml.SelectSingleNode(oCgfSectPath) Is Nothing Then
                            moAdXfm.bProcessRepeats = True

                            Dim PerPageCount As Integer = 50
                            Dim TotalCount As Integer = 0

                            Dim skipRecords As Integer = 0
                            If (myWeb.moSession("loadCount") Is Nothing) Then

                                myWeb.moSession("loadCount") = PerPageCount


                            Else
                                If (pageloadCount = 0) Then
                                    myWeb.moSession("loadCount") = PerPageCount
                                    moAdXfm.goSession("oTempInstance") = Nothing
                                Else
                                    skipRecords = Convert.ToInt32(myWeb.moSession("loadCount"))
                                    myWeb.moSession("loadCount") = Convert.ToInt32(myWeb.moSession("loadCount")) + PerPageCount

                                End If

                            End If

                            Dim takeRecord As Integer = PerPageCount

                            Dim url As String = System.Web.HttpContext.Current.Request.Url.AbsoluteUri

                            Dim props As XmlNode = rewriteXml.SelectSingleNode(oCgfSectPath)
                            TotalCount = props.ChildNodes.Count

                            If props.ChildNodes.Count >= PerPageCount Then
                                Dim xmlstring As String = "<rewriteMap name='" & ConfigType & "'>"
                                Dim xmlWholestring As String = "<rewriteMap name='" & ConfigType & "'>"
                                Dim xmlstringend As String = "</rewriteMap>"

                                Dim count As Integer = 0

                                For i As Integer = skipRecords To props.ChildNodes.Count - 1
                                    If i > (skipRecords + takeRecord) - 1 Then
                                        Exit For
                                    Else
                                        xmlstring = xmlstring & props.ChildNodes(i).OuterXml
                                    End If

                                Next

                                For i As Integer = 0 To (skipRecords + takeRecord) - 1

                                    xmlWholestring = xmlWholestring & props.ChildNodes(i).OuterXml
                                Next
                                moAdXfm.goSession("totalCountTobeLoad") = skipRecords + takeRecord
                                moAdXfm.LoadInstanceFromInnerXml(xmlWholestring & xmlstringend)
                                moAdXfm.goSession("urlStringOfLoded") = xmlWholestring
                                JsonResult = xmlstring & xmlstringend

                            Else
                                JsonResult = rewriteXml.SelectSingleNode(oCgfSectPath).OuterXml
                            End If
                        Else
                            Dim oTempInstance As XmlElement = moAdXfm.moPageXML.CreateElement("instance")
                            oTempInstance = moAdXfm.goSession("oTempInstance")
                            moAdXfm.updateInstance(oTempInstance)
                        End If

                        moAdXfm.addValues()

                        Return JsonResult
                    Catch ex As Exception
                        RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""))
                        Return ex.Message
                    End Try
                End If
            End Function

            Public Function AddNewUrl(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String
                Dim JsonResult As String = ""
                Dim ConfigType As String = inputJson("redirectType").ToObject(Of String)
                Dim oldUrl As String = inputJson("oldUrl").ToObject(Of String)
                Dim newUrl As String = inputJson("newUrl").ToObject(Of String)
                Dim renderedCount As Integer = Convert.ToInt32(moAdXfm.goSession("totalCountTobeLoad"))
                Dim urlStringOfLoded As String = ""
                Dim xmlWholestring As String = "<rewriteMap name='" & ConfigType & "'>"
                Dim xmlstringend As String = "</rewriteMap>"
                Dim xFormPath As String = "/xforms/config/" & ConfigType & ".xml"
                Dim oFrmElmt As XmlElement
                Dim oFsh As fsHelper
                oFsh = New fsHelper
                oFsh.open(moAdXfm.moPageXML)


                moAdXfm.NewFrm("WebSettings")
                moAdXfm.bProcessRepeats = False
                If Not moAdXfm.load(xFormPath, myWeb.maCommonFolders) Then

                    oFrmElmt = moAdXfm.addGroup(moAdXfm.moXformElmt, "Config", "", "ConfigSettings")
                    moAdXfm.addNote(oFrmElmt, xForm.noteTypes.Alert, xFormPath & " could not be found. - ")

                Else
                    Try
                        Dim oTemplateInstance As XmlElement = moAdXfm.moPageXML.CreateElement("Instance")
                        oTemplateInstance.InnerXml = moAdXfm.Instance.InnerXml
                        urlStringOfLoded = moAdXfm.goSession("urlStringOfLoded")

                        Dim stringExtraNode As String = "<add key=""" & oldUrl & """ value=""" & newUrl & " "" />"

                        moAdXfm.LoadInstanceFromInnerXml(xmlWholestring & stringExtraNode & xmlstringend)
                        ' moAdXfm.goSession("urlStringOfLoded") = urlStringOfLoded & stringExtraNode
                        moAdXfm.goSession("totalCountTobeLoad") = 1
                        If moAdXfm.goSession("oTempInstance") Is Nothing Then
                            moAdXfm.updateInstanceFromRequest()
                            moAdXfm.goSession("oTempInstance") = moAdXfm.Instance
                        End If

                        Return JsonResult
                    Catch ex As Exception
                        RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""))
                        Return ex.Message
                    End Try
                End If
                Return JsonResult
            End Function

            Public Function searchUrl(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String
                Dim ConfigType As String = inputJson("redirectType").ToObject(Of String)

                Dim oFrmElmt As XmlElement
                Dim cProcessInfo As String = ""
                Dim oFsh As fsHelper
                Dim xFormPath As String = "/xforms/config/" & ConfigType & ".xml"
                If Not moAdXfm.load(xFormPath, myWeb.maCommonFolders) Then

                    oFrmElmt = moAdXfm.addGroup(moAdXfm.moXformElmt, "Config", "", "ConfigSettings")
                    moAdXfm.addNote(oFrmElmt, xForm.noteTypes.Alert, xFormPath & " could not be found. - ")

                Else
                    Try
                        oFsh = New fsHelper
                        oFsh.open(moAdXfm.moPageXML)

                        moAdXfm.NewFrm("WebSettings")
                        moAdXfm.bProcessRepeats = False

                        Dim JsonResult As String = ""
                        Dim rewriteXml As New XmlDocument

                        rewriteXml.Load(myWeb.goServer.MapPath("/rewriteMaps.config"))

                        Dim oTemplateInstance As XmlElement = moAdXfm.moPageXML.CreateElement("Instance")
                        oTemplateInstance.InnerXml = moAdXfm.Instance.InnerXml
                        Dim oCgfSectName As String = "system.webServer"
                        Dim oCgfSectPath As String = "rewriteMaps/rewriteMap[@name='" & ConfigType & "']"
                        ' Dim oCgfSect As System.Configuration.DefaultSection = oCfg.GetSection(oCgfSectName)
                        cProcessInfo = "Getting Section Name:" & oCgfSectPath
                        Dim sectionMissing As Boolean = False

                        If Not rewriteXml.SelectSingleNode(oCgfSectPath) Is Nothing Then

                            Dim props As XmlNode = rewriteXml.SelectSingleNode(oCgfSectPath)
                            JsonResult = rewriteXml.SelectSingleNode(oCgfSectPath).OuterXml
                        End If

                        Return JsonResult
                    Catch ex As Exception
                        RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""))
                        Return ex.Message
                    End Try
                End If
            End Function

        End Class


#End Region

    End Class

End Class

