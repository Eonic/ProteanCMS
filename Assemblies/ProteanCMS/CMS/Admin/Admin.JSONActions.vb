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

            Public Sub New()
                Dim ctest As String = "this constructor is being hit" 'for testing
                myWeb = New Protean.Cms()
                myWeb.InitializeVariables()
                myWeb.Open()
                myCart = New Protean.Cms.Cart(myWeb)
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

                        'Check we do not have a redirect for the OLD URL allready. Remove if exists
                        Dim existingRedirects As XmlNodeList = rewriteXml.SelectNodes("rewriteMaps/rewriteMap/add/@key='" & OldURL & "'")
                        If Not existingRedirects Is Nothing Then
                            For Each existingNode As XmlNode In existingRedirects
                                rewriteXml.RemoveChild(existingNode)
                            Next
                        End If

                        'Add redirect
                        Dim oCgfSectPath As String = "rewriteMaps/rewriteMap[@name='" & RedirectType & "Redirect']"
                        Dim redirectSectionXmlNode As XmlNode = rewriteXml.SelectSingleNode(oCgfSectPath)
                        If Not redirectSectionXmlNode Is Nothing Then
                            Dim replacingElement As XmlElement = rewriteXml.CreateElement("RedirectInfo")
                            replacingElement.InnerXml = $"<add key='{OldURL}' value='{NewURL}'/>"

                            rewriteXml.SelectSingleNode(oCgfSectPath).FirstChild.AppendChild(replacingElement.FirstChild)
                            rewriteXml.Save(myWeb.goServer.MapPath("/rewriteMaps.config"))
                        End If

                        'Determine all the paths that need to be redirected
                        If RedirectType = "301Redirect" Then
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


        End Class

#End Region

    End Class

End Class

