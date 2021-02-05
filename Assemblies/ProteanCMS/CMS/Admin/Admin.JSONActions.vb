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
            Public moAdminRedirect As Protean.Cms.Admin.Redirects




            Public Sub New()
                Dim ctest As String = "this constructor is being hit" 'for testing
                myWeb = New Protean.Cms()
                myWeb.InitializeVariables()
                myWeb.Open()
                myCart = New Protean.Cms.Cart(myWeb)
                moAdXfm = New Protean.Cms.xForm(myWeb)
                moAdminRedirect = New Protean.Cms.Admin.Redirects()
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


            Public Function loadUrlsForPagination(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String

                Dim redirectType As String = inputJson("redirectType").ToObject(Of String)
                Dim pageloadCount As Integer = inputJson("loadCount").ToObject(Of Integer)

                Try

                    Dim JsonResult As String = ""
                    Dim rewriteXml As New XmlDocument
                    rewriteXml.Load(myWeb.goServer.MapPath("/rewriteMaps.config"))

                    Dim oCgfSectName As String = "system.webServer"
                    Dim oCgfSectPath As String = "rewriteMaps/rewriteMap[@name='" & redirectType & "Redirect']"

                    If Not rewriteXml.SelectSingleNode(oCgfSectPath) Is Nothing Then

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
                            JsonResult = xmlstring & xmlstringend

                        Else
                            JsonResult = rewriteXml.SelectSingleNode(oCgfSectPath).OuterXml
                        End If

                    End If

                    Return JsonResult
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""))
                    Return ex.Message
                End Try

            End Function

            Public Function AddNewUrl(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String
                Dim JsonResult As String = ""
                Dim redirectType As String = inputJson("redirectType").ToObject(Of Integer)
                Dim oldUrl As String = inputJson("oldUrl").ToObject(Of String)
                Dim newUrl As String = inputJson("newUrl").ToObject(Of String)
                Try
                    JsonResult = moAdminRedirect.CreateRedirect(redirectType, oldUrl, newUrl)
                    Return JsonResult
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""))
                    Return ex.Message
                End Try

            End Function

            Public Function searchUrl(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String
                Dim redirectType As String = inputJson("redirectType").ToObject(Of String)
                Dim searchObj As String = inputJson("searchObj").ToObject(Of String)
                Try

                    Dim JsonResult As String = ""
                    Dim rewriteXml As New XmlDocument

                    rewriteXml.Load(myWeb.goServer.MapPath("/rewriteMaps.config"))

                    Dim oCgfSectName As String = "system.webServer"
                    Dim oCgfSectPath As String = "rewriteMaps/rewriteMap[@name='" & redirectType & "Redirect']"
                    Dim props As XmlNode
                    If Not rewriteXml.SelectSingleNode(oCgfSectPath) Is Nothing Then

                        props = rewriteXml.SelectSingleNode(oCgfSectPath)
                        Dim searchString As String = "<rewriteMap name='" & redirectType & "Redirect'>"
                        Dim xmlstringend As String = "</rewriteMap>"
                        For i As Integer = 0 To props.ChildNodes.Count - 1
                            If (props.ChildNodes(i).OuterXml).IndexOf(searchObj, 0, StringComparison.CurrentCultureIgnoreCase) > -1 Then
                                searchString = searchString & props.ChildNodes(i).OuterXml
                            End If
                        Next
                        JsonResult = searchString & xmlstringend
                    End If

                    Return JsonResult

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""))
                    Return ex.Message
                End Try

            End Function



            Public Function saveUrls(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String
                Dim redirectType As String = inputJson("redirectType").ToObject(Of String)
                Dim oldUrl As String = inputJson("oldUrl").ToObject(Of String)
                Dim newUrl As String = inputJson("NewUrl").ToObject(Of String)
                Dim hiddenOldUrl As String = inputJson("hiddenOldUrl").ToObject(Of String)
                Dim JsonResult As String = ""

                Try
                    JsonResult = moAdminRedirect.CreateRedirect(redirectType, oldUrl, newUrl, hiddenOldUrl)

                    Return JsonResult
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""))
                    Return ex.Message
                End Try

                Return JsonResult
            End Function

            Public Function deleteUrls(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String
                Dim redirectType As String = inputJson("redirectType").ToObject(Of String)
                Dim oldUrl As String = inputJson("oldUrl").ToObject(Of String)
                Dim newUrl As String = inputJson("NewUrl").ToObject(Of String)
                Dim JsonResult As String = ""
                Dim rewriteXml As New XmlDocument
                rewriteXml.Load(myWeb.goServer.MapPath("/rewriteMaps.config"))
                Try

                    Dim existingRedirects As XmlNodeList = rewriteXml.SelectNodes("rewriteMaps/rewriteMap[@name='" & redirectType & "Redirect']/add[@key='" & oldUrl & "']")
                    If existingRedirects.Count > 0 Then

                        For Each existingNode As XmlNode In existingRedirects
                            existingNode.ParentNode.RemoveChild(existingNode)
                            rewriteXml.Save(myWeb.goServer.MapPath("/rewriteMaps.config"))
                        Next
                    End If
                    JsonResult = "success"
                    Return JsonResult
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""))
                    Return ex.Message
                End Try
            End Function

            Public Function IsUrlPresent(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String
                Dim JsonResult As String = ""
                Dim redirectType As String = inputJson("redirectType").ToObject(Of Integer)
                Dim oldUrl As String = inputJson("oldUrl").ToObject(Of String)
                Dim rewriteXml As New XmlDocument
                rewriteXml.Load(myWeb.goServer.MapPath("/rewriteMaps.config"))

                ''Check we do not have a redirect for the OLD URL allready. Remove if exists
                Dim existingRedirects As XmlNodeList = rewriteXml.SelectNodes("rewriteMaps/rewriteMap[@name='" & redirectType & "Redirect']/add[@key='" & oldUrl & "']")
                If (existingRedirects.Count > 0) Then
                    JsonResult = "True"
                Else
                    JsonResult = "false"
                End If
                Return JsonResult
            End Function
        End Class


#End Region

    End Class

End Class

