Imports System.Xml
Imports System.Collections
Imports System.Web.Configuration

Partial Public Class Cms
    Partial Public Class Admin
        Public Class Redirects
            Public Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
            Private Const mcModuleName As String = "Cms.Admin.Redirects"
            Private myWeb As Protean.Cms
            Private myCart As Protean.Cms.Cart
            Public moAdXfm As Protean.Cms.xForm

            Enum RedirectType
                Redirect301 = 301
                Redirect302 = 302
                Redirect404 = 404
            End Enum

            Public Sub New()
                Dim ctest As String = "this constructor is being hit" 'for testing
                myWeb = New Protean.Cms()
                myWeb.InitializeVariables()
                myWeb.Open()
                myCart = New Protean.Cms.Cart(myWeb)
                moAdXfm = New Protean.Cms.xForm(myWeb)
            End Sub

            Public Function CreateRedirect(ByRef redirectType As RedirectType, ByRef OldUrl As String, ByRef NewUrl As String) As String
                Try

                    ' Dim ArticleId As Long
                    ' Dim NewPageName As String

                    Dim rewriteXml As New XmlDocument
                    rewriteXml.Load(myWeb.goServer.MapPath("/rewriteMaps.config"))

                    ''Check we do not have a redirect for the OLD URL allready. Remove if exists
                    Dim existingRedirects As XmlNodeList = rewriteXml.SelectNodes("rewriteMaps/rewriteMap[@name='" & redirectType & "Redirect']/add[@key='" & OldUrl & "']")
                    If Not existingRedirects Is Nothing Then
                        For Each existingNode As XmlNode In existingRedirects
                            existingNode.RemoveAll()
                            rewriteXml.Save(myWeb.goServer.MapPath("/rewriteMaps.config"))
                        Next
                    End If

                    'Add redirect
                    Dim oCgfSectPath As String = "rewriteMaps/rewriteMap[@name='" & redirectType & "Redirect']"
                    Dim redirectSectionXmlNode As XmlNode = rewriteXml.SelectSingleNode(oCgfSectPath)
                    If Not redirectSectionXmlNode Is Nothing Then
                        Dim replacingElement As XmlElement = rewriteXml.CreateElement("RedirectInfo")
                        replacingElement.InnerXml = $"<add key='{OldUrl}' value='{NewUrl}'/>"

                        ' rewriteXml.SelectSingleNode(oCgfSectPath).FirstChild.AppendChild(replacingElement.FirstChild)
                        rewriteXml.SelectSingleNode(oCgfSectPath).AppendChild(replacingElement.FirstChild)
                        rewriteXml.Save(myWeb.goServer.MapPath("/rewriteMaps.config"))
                    End If

                    'Determine all the paths that need to be redirected
                    If redirectType = 301 Then
                        'step through and create rules to deal with paths
                        Dim folderRules As New ArrayList
                        Dim rulesXml As New XmlDocument
                        rulesXml.Load(myWeb.goServer.MapPath("/RewriteRules.config"))
                        Dim insertAfterElment As XmlElement = rulesXml.SelectSingleNode("descendant-or-self::rule[@name='EW: 301 Redirects']")
                        Dim oRule As XmlElement

                        'For Each oRule In replacerNode.SelectNodes("add")
                        Dim CurrentRule As XmlElement = rulesXml.SelectSingleNode("descendant-or-self::rule[@name='Folder: " & OldUrl & "']")
                        Dim newRule As XmlElement = rulesXml.CreateElement("newRule")
                        Dim matchString As String = OldUrl
                        If matchString.StartsWith("/") Then
                            matchString = matchString.TrimStart("/")
                        End If
                        folderRules.Add("Folder: " & OldUrl)
                        newRule.InnerXml = "<rule name=""Folder: " & OldUrl & """><match url=""^" & matchString & "(.*)""/><action type=""Redirect"" url=""" & NewUrl & "{R:1}"" /></rule>"
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

                    Dim Result As String = ""
                    Return Result

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "CreateRedirect", ex, ""))
                    Return ex.Message
                End Try

            End Function

        End Class
    End Class
End Class
