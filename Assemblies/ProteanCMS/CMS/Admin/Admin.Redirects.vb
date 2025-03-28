﻿Imports System.Xml
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
            Public moDbHelper As dbHelper

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
                moDbHelper = myWeb.moDbHelper
            End Sub

            Public Function CreateRedirect(ByRef redirectType As String, ByRef OldUrl As String, ByRef NewUrl As String, Optional ByVal hiddenOldUrl As String = "", Optional ByVal pageId As Integer = 0, Optional ByVal isParentPage As String = "false") As String

                Try

                    Dim rewriteXml As New XmlDocument
                    Dim Result As String = "success"
                    rewriteXml.Load(myWeb.goServer.MapPath("/rewriteMaps.config"))
                    Dim oCgfSectPath As String = "rewriteMaps/rewriteMap[@name='" & redirectType & "']"
                    Dim redirectSectionXmlNode As XmlNode = rewriteXml.SelectSingleNode(oCgfSectPath)
                    ''Check we do not have a redirect for the OLD URL allready. Remove if exists
                    Dim existingRedirectsForOldUrlAsKey As XmlNodeList
                    Dim existingRedirectsForOldUrlAsValue As XmlNodeList
                    Dim existingRedirectsForNewUrlAsKey As XmlNodeList
                    Dim existingRedirectsForNewUrlAsValue As XmlNodeList
                    If (hiddenOldUrl = "") Then
                        existingRedirectsForOldUrlAsKey = rewriteXml.SelectNodes("rewriteMaps/rewriteMap[@name='" & redirectType & "']/add[@key='" & OldUrl & "']")
                        existingRedirectsForOldUrlAsValue = rewriteXml.SelectNodes("rewriteMaps/rewriteMap[@name='" & redirectType & "']/add[@value='" & OldUrl & "']")

                        existingRedirectsForNewUrlAsKey = rewriteXml.SelectNodes("rewriteMaps/rewriteMap[@name='" & redirectType & "']/add[@key='" & NewUrl & "']")
                        ' existingRedirectsForOldUrlAsValue = rewriteXml.SelectNodes("rewriteMaps/rewriteMap[@name='" & redirectType & "']/add[@value='" & OldUrl & "']")

                    Else
                        existingRedirectsForOldUrlAsKey = rewriteXml.SelectNodes("rewriteMaps/rewriteMap[@name='" & redirectType & "']/add[@key='" & hiddenOldUrl & "']")
                        existingRedirectsForOldUrlAsValue = rewriteXml.SelectNodes("rewriteMaps/rewriteMap[@name='" & redirectType & "']/add[@value='" & hiddenOldUrl & "']")
                        existingRedirectsForNewUrlAsKey = rewriteXml.SelectNodes("rewriteMaps/rewriteMap[@name='" & redirectType & "']/add[@key='" & NewUrl & "']")

                    End If

                    If existingRedirectsForOldUrlAsKey.Count > 0 Or existingRedirectsForOldUrlAsValue.Count > 0 Then
                        If existingRedirectsForOldUrlAsKey.Count > 0 Then
                            For Each existingNode As XmlNode In existingRedirectsForOldUrlAsKey
                                Dim newNode As XmlNode = existingNode
                                newNode.Attributes.Item(0).InnerXml = OldUrl
                                newNode.Attributes.Item(1).InnerXml = NewUrl
                                rewriteXml.Save(myWeb.goServer.MapPath("/rewriteMaps.config"))
                            Next
                        End If
                        If existingRedirectsForNewUrlAsKey.Count > 0 Then
                            For Each existingNewUrlNode As XmlNode In existingRedirectsForNewUrlAsKey

                                Dim newUrlNode As XmlNode = existingNewUrlNode
                                Dim existingNewUrl As String = newUrlNode.Attributes.Item(1).InnerXml
                                'newNode.Attributes.Item(1).InnerXml = 

                                For Each existingNode As XmlNode In existingRedirectsForOldUrlAsKey

                                    Dim newNode As XmlNode = existingNode
                                    newNode.Attributes.Item(0).InnerXml = OldUrl
                                    newNode.Attributes.Item(1).InnerXml = existingNewUrl
                                    rewriteXml.Save(myWeb.goServer.MapPath("/rewriteMaps.config"))
                                Next
                            Next
                        End If
                        If existingRedirectsForOldUrlAsValue.Count > 0 Then

                            For Each existingNode As XmlNode In existingRedirectsForOldUrlAsValue
                                Dim newNode As XmlNode = existingNode
                                'newNode.Attributes.Item(0).InnerXml = OldUrl
                                newNode.Attributes.Item(1).InnerXml = NewUrl
                                rewriteXml.Save(myWeb.goServer.MapPath("/rewriteMaps.config"))

                            Next
                            If existingRedirectsForOldUrlAsKey.Count = 0 Then
                                If Not redirectSectionXmlNode Is Nothing Then
                                    Dim replacingElement As XmlElement = rewriteXml.CreateElement("RedirectInfo")
                                    replacingElement.InnerXml = $"<add key='{OldUrl}' value='{NewUrl}'/>"
                                    rewriteXml.SelectSingleNode(oCgfSectPath).AppendChild(replacingElement.FirstChild)
                                    rewriteXml.Save(myWeb.goServer.MapPath("/rewriteMaps.config"))

                                End If
                            End If
                        End If
                    Else

                        'Add redirect
                        If isParentPage.ToLower() = "false" Then

                            If Not redirectSectionXmlNode Is Nothing Then
                                Dim replacingElement As XmlElement = rewriteXml.CreateElement("RedirectInfo")
                                replacingElement.InnerXml = $"<add key='{OldUrl}' value='{NewUrl}'/>"

                                ' rewriteXml.SelectSingleNode(oCgfSectPath).FirstChild.AppendChild(replacingElement.FirstChild)
                                rewriteXml.SelectSingleNode(oCgfSectPath).AppendChild(replacingElement.FirstChild)
                                rewriteXml.Save(myWeb.goServer.MapPath("/rewriteMaps.config"))


                            End If
                        End If
                    End If
                    'Determine all the paths that need to be redirected
                    ' If redirectType = "301Redirect" Then
                    If pageId > 0 Then

                        If isParentPage = "True" Then
                            Select Case redirectType
                                Case "301Redirect"

                                    redirectType = "301 Redirects"

                                Case "302Redirect"
                                    redirectType = "302 Redirects"

                                Case "Redirect404"
                                    redirectType = "404 Redirects"
                                Case Else

                            End Select

                            'step through and create rules to deal with paths
                            Dim folderRules As New ArrayList
                            Dim rulesXml As New XmlDocument
                            rulesXml.Load(myWeb.goServer.MapPath("/RewriteRules.config"))
                            Dim insertAfterElment As XmlElement = rulesXml.SelectSingleNode("descendant-or-self::rule[@name='EW: " & redirectType & "']")
                            'Dim oRule As XmlElement

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

                            'For Each oRule In rulesXml.SelectNodes("descendant-or-self::rule[starts-with(@name,'Folder: ')]")
                            '    If Not folderRules.Contains(oRule.GetAttribute("name")) Then
                            '        oRule.ParentNode.RemoveChild(oRule)
                            '    End If
                            'Next

                            rulesXml.Save(myWeb.goServer.MapPath("/RewriteRules.config"))
                            myWeb.bRestartApp = True
                        End If
                    End If

                    Return Result

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "CreateRedirect", ex, ""))
                    Return ex.Message
                End Try

            End Function


            Public Function LoadUrlsForPegination(ByRef redirectType As String, ByRef pageloadCount As Integer) As String
                Try

                    Dim Result As String = ""
                    Dim rewriteXml As New XmlDocument
                    rewriteXml.Load(myWeb.goServer.MapPath("/rewriteMaps.config"))

                    Dim oCgfSectName As String = "system.webServer"
                    Dim oCgfSectPath As String = "rewriteMaps/rewriteMap[@name='" & redirectType & "']"
                    Dim props As XmlNode = rewriteXml.SelectSingleNode(oCgfSectPath)

                    If Not rewriteXml.SelectSingleNode(oCgfSectPath) Is Nothing Then

                        Dim PerPageCount As Integer = 50
                        Dim TotalCount As Integer = 0
                        Dim skipRecords As Integer = 0
                        If (myWeb.moSession("loadCount") Is Nothing) Then
                            If (PerPageCount >= props.ChildNodes.Count) Then
                                myWeb.moSession("loadCount") = props.ChildNodes.Count

                            End If
                            'Add Value To Session first time When load 50 records. changed by nita on 29march22
                            myWeb.moSession("loadCount") = Convert.ToInt32(myWeb.moSession("loadCount")) + PerPageCount
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

                        TotalCount = props.ChildNodes.Count

                        If props.ChildNodes.Count >= PerPageCount Then
                            Dim xmlstring As String = "<rewriteMap name='" & redirectType & "'>"
                            Dim xmlstringend As String = "</rewriteMap>"

                            Dim count As Integer = 0

                            For i As Integer = skipRecords To props.ChildNodes.Count - 1
                                If i > (skipRecords + takeRecord) - 1 Then
                                    Exit For
                                Else
                                    If props.ChildNodes(i).Name = "add" Then
                                        xmlstring = xmlstring & props.ChildNodes(i).OuterXml
                                    End If
                                End If
                            Next
                            Result = xmlstring & xmlstringend

                        Else
                            If (pageloadCount = 0) Then
                                Result = rewriteXml.SelectSingleNode(oCgfSectPath).OuterXml
                            Else
                                Result = ""
                            End If

                        End If

                    End If

                    Return Result
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""))
                    Return ex.Message
                End Try

            End Function

            Public Function SearchUrl(ByRef redirectType As String, ByRef searchObj As String, ByRef pageloadCount As Integer) As String

                Try

                    Dim Result As String = ""
                    Dim rewriteXml As New XmlDocument

                    rewriteXml.Load(myWeb.goServer.MapPath("/rewriteMaps.config"))

                    Dim oCgfSectName As String = "system.webServer"
                    Dim oCgfSectPath As String = "rewriteMaps/rewriteMap[@name='" & redirectType & "']"

                    If Not rewriteXml.SelectSingleNode(oCgfSectPath) Is Nothing Then

                        Dim PerPageCount As Integer = 50
                        Dim TotalCount As Integer = 0
                        Dim skipRecords As Integer = 0
                        If (myWeb.moSession("searchLoadCount") Is Nothing) Then
                            myWeb.moSession("searchLoadCount") = PerPageCount
                        Else
                            If (pageloadCount = 0) Then
                                myWeb.moSession("searchLoadCount") = PerPageCount
                                moAdXfm.goSession("oTempInstance") = Nothing
                            Else
                                skipRecords = Convert.ToInt32(myWeb.moSession("searchLoadCount"))
                                myWeb.moSession("searchLoadCount") = Convert.ToInt32(myWeb.moSession("searchLoadCount")) + PerPageCount
                            End If
                        End If

                        Dim takeRecord As Integer = PerPageCount
                        Dim props As XmlNode = rewriteXml.SelectSingleNode(oCgfSectPath)
                        TotalCount = props.ChildNodes.Count

                        Dim xmlstring As String = "<rewriteMap name='" & redirectType & "'>"
                        Dim xmlstringend As String = "</rewriteMap>"
                        Dim searchString As String = "<rewriteMap name='" & redirectType & "'>"

                        Dim searchProps As New XmlDocument
                        Dim count As Integer = 0

                        For i As Integer = 0 To props.ChildNodes.Count - 1
                            If (props.ChildNodes(i).OuterXml).IndexOf(searchObj, 0, StringComparison.CurrentCultureIgnoreCase) > -1 Then

                                xmlstring = xmlstring & props.ChildNodes(i).OuterXml

                            End If
                        Next
                        searchProps.LoadXml(xmlstring & xmlstringend)


                        For i As Integer = skipRecords To searchProps.ChildNodes(0).ChildNodes.Count - 1

                            If (searchProps.ChildNodes(0).ChildNodes(i).OuterXml).IndexOf(searchObj, 0, StringComparison.CurrentCultureIgnoreCase) > -1 Then
                                If i > (skipRecords + takeRecord) - 1 Then
                                    Exit For
                                Else
                                    searchString = searchString & searchProps.ChildNodes(0).ChildNodes(i).OuterXml
                                End If
                            End If
                        Next


                        Result = searchString & xmlstringend
                    End If

                    Return Result

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""))
                    Return ex.Message
                End Try

            End Function

            Public Function DeleteUrls(ByRef redirectType As String, ByRef oldUrl As String, ByRef newUrl As String) As String

                Dim Result As String = ""
                Dim rewriteXml As New XmlDocument
                rewriteXml.Load(myWeb.goServer.MapPath("/rewriteMaps.config"))
                Try

                    Dim existingRedirects As XmlNodeList = rewriteXml.SelectNodes("rewriteMaps/rewriteMap[@name='" & redirectType & "']/add[@key='" & oldUrl & "']")
                    If existingRedirects.Count > 0 Then

                        For Each existingNode As XmlNode In existingRedirects
                            existingNode.ParentNode.RemoveChild(existingNode)
                            rewriteXml.Save(myWeb.goServer.MapPath("/rewriteMaps.config"))
                        Next
                    End If
                    Result = "success"
                    Return Result
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""))
                    Return ex.Message
                End Try
            End Function

            Public Function IsUrlPresent(ByRef redirectType As String, ByRef oldUrl As String) As String
                Dim Result As String = ""

                Dim rewriteXml As New XmlDocument
                rewriteXml.Load(myWeb.goServer.MapPath("/rewriteMaps.config"))

                ''Check we do not have a redirect for the OLD URL allready. Remove if exists
                Dim existingRedirectsForOldUrl As XmlNodeList = rewriteXml.SelectNodes("rewriteMaps/rewriteMap[@name='" & redirectType & "']/add[@key='" & oldUrl & "']")
                Dim existingRedirectsForNewUrl As XmlNodeList = rewriteXml.SelectNodes("rewriteMaps/rewriteMap[@name='" & redirectType & "']/add[@value='" & oldUrl & "']")

                If (existingRedirectsForOldUrl.Count > 0 Or existingRedirectsForNewUrl.Count > 0) Then
                    Result = "True"
                Else
                    Result = "false"
                End If
                Return Result
            End Function
            Public Function GetTotalNumberOfUrls(ByRef redirectType As String) As String

                Dim Result As String = ""
                Dim rewriteXml As New XmlDocument
                rewriteXml.Load(myWeb.goServer.MapPath("/rewriteMaps.config"))

                Dim oCgfSectName As String = "system.webServer"
                Dim oCgfSectPath As String = "rewriteMaps/rewriteMap[@name='" & redirectType & "']"

                Dim props As XmlNode = rewriteXml.SelectSingleNode(oCgfSectPath)
                Dim TotalCount As Integer = props.ChildNodes.Count

                Result = TotalCount.ToString()

                Return Result
            End Function

            Public Function LoadAllurls(ByRef redirectType As String, ByRef pageloadCount As Integer, ByRef flag As String) As String
                Try

                    Dim Result As String = ""
                    Dim rewriteXml As New XmlDocument
                    rewriteXml.Load(myWeb.goServer.MapPath("/rewriteMaps.config"))

                    Dim oCgfSectName As String = "system.webServer"
                    Dim oCgfSectPath As String = "rewriteMaps/rewriteMap[@name='" & redirectType & "']"

                    If Not rewriteXml.SelectSingleNode(oCgfSectPath) Is Nothing Then

                        Dim PerPageCount As Integer = 50
                        Dim TotalCount As Integer = 0
                        Dim skipRecords As Integer = 0



                        Dim takeRecord As Integer = pageloadCount
                        Dim props As XmlNode = rewriteXml.SelectSingleNode(oCgfSectPath)
                        TotalCount = props.ChildNodes.Count

                        If props.ChildNodes.Count >= PerPageCount Then
                            Dim xmlstring As String = "<rewriteMap name='" & redirectType & "Redirect'>"
                            Dim xmlstringend As String = "</rewriteMap>"

                            Dim count As Integer = 0

                            For i As Integer = skipRecords To props.ChildNodes.Count - 1
                                If i > (skipRecords + takeRecord) - 1 Then
                                    Exit For
                                Else
                                    xmlstring = xmlstring & props.ChildNodes(i).OuterXml
                                End If

                            Next
                            Result = xmlstring & xmlstringend

                        Else
                            Result = rewriteXml.SelectSingleNode(oCgfSectPath).OuterXml
                        End If

                    End If

                    Return Result
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""))
                    Return ex.Message
                End Try

            End Function
            Public Function GetTotalNumberOfSearchUrls(ByRef redirectType As String, ByRef searchObj As String) As String

                Dim Result As String = ""
                Dim rewriteXml As New XmlDocument
                rewriteXml.Load(myWeb.goServer.MapPath("/rewriteMaps.config"))

                Dim oCgfSectName As String = "system.webServer"
                Dim oCgfSectPath As String = "rewriteMaps/rewriteMap[@name='" & redirectType & "']"

                Dim props As XmlNode = rewriteXml.SelectSingleNode(oCgfSectPath)

                Dim xmlstringend As String = "</rewriteMap>"
                Dim searchString As String = "<rewriteMap name='" & redirectType & "'>"

                Dim searchProps As New XmlDocument

                For i As Integer = 0 To props.ChildNodes.Count - 1
                    If (props.ChildNodes(i).OuterXml).IndexOf(searchObj, 0, StringComparison.CurrentCultureIgnoreCase) > -1 Then

                        searchString = searchString & props.ChildNodes(i).OuterXml

                    End If
                Next
                searchProps.LoadXml(searchString & xmlstringend)

                Dim TotalCount As Integer = searchProps.ChildNodes(0).ChildNodes.Count

                Result = TotalCount.ToString()

                Return Result
            End Function

            Public Function IsParentPage(ByRef pageId As Integer) As Boolean

                Dim Result As String = ""
                If pageId > 0 Then
                    Result = moDbHelper.isParent(pageId)
                End If
                Return Result
            End Function
            Public Function RedirectPage(ByRef sRedirectType As String, ByRef sOldUrl As String, ByRef sNewUrl As String, ByRef sPageUrl As String, Optional ByVal bRedirectChildPage As Boolean = False, Optional ByVal sType As String = "", Optional ByVal nPageId As Integer = 0) As String

                Dim result As String = "success"
                If sRedirectType IsNot Nothing And sRedirectType <> String.Empty Then

                    Dim sUrl As String = ""
                    If myWeb.moConfig("PageURLFormat") = "hyphens" Then
                        sNewUrl = sNewUrl.TrimEnd()
                        sOldUrl = sOldUrl.Replace(" ", "-")
                        sNewUrl = sNewUrl.Replace(" ", "-")
                    End If
                    If sPageUrl IsNot Nothing And sPageUrl <> String.Empty Then
                        sUrl = sPageUrl
                        Dim arr() As String
                        arr = sUrl.Split("?"c)
                        sUrl = arr(0)
                        ' sUrl = sUrl.Substring(0, sUrl.LastIndexOf("/"))
                    End If

                    Select Case sType
                        Case "Page"
                            If (sUrl <> String.Empty) Then
                                sNewUrl = sUrl.Replace(sOldUrl, sNewUrl)
                                sOldUrl = sUrl
                            End If
                        Case Else

                            ' If (sType = "Product") Then
                            If myWeb.moConfig("DetailPrefix") IsNot Nothing And (myWeb.moConfig("DetailPrefix") <> "") Then
                                Dim prefixs() As String = myWeb.moConfig("DetailPrefix").Split(",")
                                Dim thisPrefix As String = ""
                                Dim thisContentType As String = ""

                                Dim i As Integer
                                For i = 0 To prefixs.Length - 1
                                    thisPrefix = prefixs(i).Substring(0, prefixs(i).IndexOf("/"))
                                    thisContentType = prefixs(i).Substring(prefixs(i).IndexOf("/") + 1, prefixs(i).Length - prefixs(i).IndexOf("/") - 1)
                                    If thisContentType = sType Then
                                        sNewUrl = "/" & thisPrefix & "/" & sNewUrl
                                        sOldUrl = "/" & thisPrefix & "/" & sOldUrl
                                    End If
                                Next

                            Else

                                Dim url As String = myWeb.GetContentUrl(nPageId)
                                sOldUrl = sUrl & url & "/" & sOldUrl
                                sNewUrl = sUrl & url & "/" & sNewUrl

                            End If
                            If myWeb.moConfig("TrailingSlash") IsNot Nothing And (myWeb.moConfig("TrailingSlash") = "on") Then
                                sOldUrl = sOldUrl & "/"
                                sNewUrl = sNewUrl & "/"
                            End If
                            'End If
                    End Select

                    Select Case sRedirectType
                        Case "301Redirect"

                            CreateRedirect(sRedirectType, sOldUrl, sNewUrl, "", nPageId, bRedirectChildPage)

                        Case "302Redirect"
                            CreateRedirect(sRedirectType, sOldUrl, sNewUrl, "", nPageId, bRedirectChildPage)

                        Case Else
                            'do nothing
                    End Select
                End If
                Return result
            End Function
        End Class
    End Class
End Class
