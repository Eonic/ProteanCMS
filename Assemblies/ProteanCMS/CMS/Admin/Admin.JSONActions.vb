
Option Strict Off
Option Explicit On
Imports System.Xml
Imports System.Collections
Imports System.Web.Configuration
Imports System.Configuration


Partial Public Class Cms

    Partial Public Class Admin


#Region "JSON Actions"

        Public Class JSONActions
            Inherits API.JsonActions

            Public Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
            Private Const mcModuleName As String = "Cms.Admin.JSONActions"
            Private moLmsConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/lms")
            Private myWeb As Protean.Cms
            Private myCart As Protean.Cms.Cart
            Public moAdXfm As Protean.Cms.xForm
            Public moAdminXfm As Protean.Cms.Admin.AdminXforms
            Public moAdminRedirect As Protean.Cms.Admin.Redirects
            Public goConfig As System.Collections.Specialized.NameValueCollection





            Public Sub New()
                Dim ctest As String = "this constructor is being hit" 'for testing
                myWeb = New Protean.Cms()
                myWeb.InitializeVariables()
                myWeb.Open()
                myCart = New Protean.Cms.Cart(myWeb)
                moAdXfm = New Protean.Cms.xForm(myWeb)
                moAdminRedirect = New Protean.Cms.Admin.Redirects()
                moAdminXfm = myWeb.getAdminXform()
                goConfig = myWeb.moConfig
            End Sub

            Public Shadows Sub Open(ByVal oPageXml As XmlDocument)
                Dim cProcessInfo As String = ""
                Try
                    moAdXfm.moPageXML = oPageXml

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "Open", ex, "", cProcessInfo, gbDebug)
                End Try
            End Sub

            Public Function QueryValue(ByRef myApi As Protean.API, ByRef jObj As Newtonsoft.Json.Linq.JObject) As String
                Try

                    Dim bIsAuthorized As Boolean = False
                    bIsAuthorized = ValidateAPICall(myWeb, "Administrator")
                    If bIsAuthorized Then
                        Dim count As String = "0"
                        Dim sSql = myApi.moConfig(myApi.moRequest("query"))

                        Dim result = myWeb.moDbHelper.GetDataValue(sSql, System.Data.CommandType.StoredProcedure)
                        count = If(result Is Nothing, "", Convert.ToString(result))

                        Return "[{""Key"":""" & myApi.moRequest("query") & """,""Value"":""" & count & """}]"
                    End If


                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Query", ex, ""))
                    Return ex.Message
                End Try
            End Function

            Public Function LoadUrlsForPagination(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String

                Dim redirectType As String = ""
                If inputJson("redirectType") IsNot Nothing Then
                    redirectType = inputJson("redirectType").ToObject(Of String)()
                End If
                Dim pageloadCount As Integer = inputJson("loadCount").ToObject(Of Integer)
                Dim JsonResult As String = ""
                Try
                    If myApi.mbAdminMode Then
                        JsonResult = moAdminRedirect.LoadUrlsForPegination(redirectType, pageloadCount)
                    End If
                    Return JsonResult
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "LoadUrlsForPagination", ex, ""))
                    Return ex.Message
                End Try

            End Function

            Public Function AddNewUrl(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String
                Dim JsonResult As String = ""
                Dim redirectType As String = ""
                Dim oldUrl As String = ""
                Dim newUrl As String = ""
                If inputJson("redirectType") IsNot Nothing Then
                    redirectType = inputJson("redirectType").ToObject(Of String)()
                End If

                If inputJson("oldUrl") IsNot Nothing Then
                    oldUrl = inputJson("oldUrl").ToObject(Of String)()
                End If

                If inputJson("newUrl") IsNot Nothing Then
                    newUrl = inputJson("newUrl").ToObject(Of String)()
                End If
                Try
                    If myApi.mbAdminMode Then
                        JsonResult = moAdminRedirect.CreateRedirect(redirectType, oldUrl, newUrl)
                    End If

                    Return JsonResult
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "AddNewUrl", ex, ""))
                    Return ex.Message
                End Try

            End Function

            Public Function SearchUrl(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String
                Dim redirectType As String = ""
                Dim searchObj As String = ""
                Dim pageloadCount As Integer = 0

                If inputJson("redirectType") IsNot Nothing Then
                    redirectType = inputJson("redirectType").ToObject(Of String)()
                End If
                If inputJson("searchObj") IsNot Nothing Then
                    searchObj = inputJson("searchObj").ToObject(Of String)()
                End If
                If inputJson("loadCount") IsNot Nothing Then
                    pageloadCount = inputJson("loadCount").ToObject(Of Integer)
                End If

                Try
                    Dim JsonResult As String = ""
                    If myApi.mbAdminMode Then
                        JsonResult = moAdminRedirect.SearchUrl(redirectType, searchObj, pageloadCount)
                    End If
                    Return JsonResult

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "SearchUrl", ex, ""))
                    Return ex.Message
                End Try

            End Function



            Public Function SaveUrls(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String
                Dim redirectType As String = ""
                Dim oldUrl As String = ""
                Dim newUrl As String = ""
                Dim hiddenOldUrl As String = ""
                Dim JsonResult As String = ""
                If inputJson("redirectType") IsNot Nothing Then
                    redirectType = inputJson("redirectType").ToObject(Of String)()
                End If

                If inputJson("oldUrl") IsNot Nothing Then
                    oldUrl = inputJson("oldUrl").ToObject(Of String)()
                End If

                If inputJson("NewUrl") IsNot Nothing Then
                    newUrl = inputJson("NewUrl").ToObject(Of String)()
                End If
                If inputJson("hiddenOldUrl") IsNot Nothing Then
                    hiddenOldUrl = inputJson("hiddenOldUrl").ToObject(Of String)
                End If

                Try
                    If myApi.mbAdminMode Then
                        JsonResult = moAdminRedirect.CreateRedirect(redirectType, oldUrl, newUrl, hiddenOldUrl)
                    End If
                    Return JsonResult
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "SaveUrls", ex, ""))
                    Return ex.Message
                End Try

                Return JsonResult
            End Function

            Public Function DeleteUrls(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String
                Dim redirectType As String = ""
                Dim oldUrl As String = ""
                Dim newUrl As String = ""
                Dim JsonResult As String = ""
                If inputJson("redirectType") IsNot Nothing Then
                    redirectType = inputJson("redirectType").ToObject(Of String)()
                End If
                If inputJson("oldUrl") IsNot Nothing Then
                    oldUrl = inputJson("oldUrl").ToObject(Of String)()
                End If

                If inputJson("NewUrl") IsNot Nothing Then
                    newUrl = inputJson("NewUrl").ToObject(Of String)()
                End If
                Try
                    If myApi.mbAdminMode Then
                        JsonResult = moAdminRedirect.DeleteUrls(redirectType, oldUrl, newUrl)
                    End If

                    Return JsonResult
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "DeleteUrls", ex, ""))
                    Return ex.Message
                End Try
            End Function

            Public Function IsUrlPresent(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String
                Dim JsonResult As String = ""
                Dim redirectType As String = ""
                Dim oldUrl As String = ""
                If inputJson("redirectType") IsNot Nothing Then
                    redirectType = inputJson("redirectType").ToObject(Of String)()
                End If
                If inputJson("oldUrl") IsNot Nothing Then
                    oldUrl = inputJson("oldUrl").ToObject(Of String)()
                End If
                Try
                    If myApi.mbAdminMode Then
                        JsonResult = moAdminRedirect.IsUrlPresent(redirectType, oldUrl)
                    End If
                    Return JsonResult
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "IsUrlPresent", ex, ""))
                    Return ex.Message
                End Try
            End Function

            Public Function GetTotalNumberOfUrls(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String
                Dim JsonResult As String = ""
                Dim redirectType As String = ""
                If inputJson("redirectType") IsNot Nothing Then
                    redirectType = inputJson("redirectType").ToObject(Of String)()
                End If
                Try
                    If myApi.mbAdminMode Then
                        JsonResult = moAdminRedirect.GetTotalNumberOfUrls(redirectType)
                    End If
                    Return JsonResult
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetTotalNumberOfUrls", ex, ""))
                    Return ex.Message
                End Try
            End Function
            Public Function GetTotalNumberOfSearchUrls(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String
                Dim JsonResult As String = ""
                Dim redirectType As String = ""
                Dim SearchObj As String = ""
                If inputJson("redirectType") IsNot Nothing Then
                    redirectType = inputJson("redirectType").ToObject(Of String)()
                End If
                If inputJson("searchObj") IsNot Nothing Then
                    SearchObj = inputJson("searchObj").ToObject(Of String)()
                End If

                Try
                    If myApi.mbAdminMode Then
                        JsonResult = moAdminRedirect.GetTotalNumberOfSearchUrls(redirectType, SearchObj)
                    End If
                    Return JsonResult
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetTotalNumberOfSearchUrls", ex, ""))
                    Return ex.Message
                End Try
            End Function

            Public Function LoadAllUrls(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String

                Dim redirectType As String = ""
                Dim pageloadCount As Integer = 0
                Dim actionFlag As String = ""
                If inputJson("redirectType") IsNot Nothing Then
                    redirectType = inputJson("redirectType").ToObject(Of String)()
                End If
                If inputJson("loadCount") IsNot Nothing Then
                    pageloadCount = inputJson("loadCount").ToObject(Of Integer)
                End If
                If inputJson("flag") IsNot Nothing Then
                    actionFlag = inputJson("flag").ToObject(Of String)
                End If

                Dim JsonResult As String = ""
                Try
                    If myApi.mbAdminMode Then

                        JsonResult = moAdminRedirect.LoadAllurls(redirectType, pageloadCount, actionFlag)
                    End If
                    Return JsonResult
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "LoadAllUrls", ex, ""))
                    Return ex.Message
                End Try

            End Function



            Public Function IsParentPage(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String
                Dim JsonResult As String = ""
                Dim pageId As String = 0
                If inputJson("pageId") IsNot Nothing Then
                    pageId = inputJson("pageId").ToObject(Of Integer)
                End If
                Try
                    If myApi.mbAdminMode Then
                        JsonResult = moAdminRedirect.IsParentPage(pageId)
                    End If
                    Return JsonResult
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "IsParentPage", ex, ""))
                    Return ex.Message
                End Try
            End Function

            Public Function RedirectPage(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String
                Dim JsonResult As String = ""
                Dim redirectType As String = ""
                Dim oldUrl As String = ""
                Dim newUrl As String = ""
                Dim hiddenOldUrl As String = ""
                Dim pageId As String = ""
                Dim isParentPage As String = ""
                Dim sType As String = ""

                If inputJson("redirectType") IsNot Nothing Then
                    redirectType = inputJson("redirectType").ToObject(Of String)()
                End If

                If inputJson("oldUrl") IsNot Nothing Then
                    oldUrl = inputJson("oldUrl").ToObject(Of String)()
                End If

                If inputJson("newUrl") IsNot Nothing Then
                    newUrl = inputJson("newUrl").ToObject(Of String)()
                End If

                If inputJson("pageId") IsNot Nothing Then
                    pageId = inputJson("pageId").ToObject(Of String)()
                End If
                If inputJson("isParent") IsNot Nothing Then
                    isParentPage = inputJson("isParent").ToObject(Of String)()
                End If

                If inputJson("pageType") IsNot Nothing Then
                    sType = inputJson("pageType").ToObject(Of String)()
                End If
                Try
                    If myApi.mbAdminMode Then
                        JsonResult = moAdminRedirect.RedirectPage(redirectType, oldUrl, newUrl, hiddenOldUrl, isParentPage, sType, pageId)
                    End If

                    Return JsonResult
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "RedirectPage", ex, ""))
                    Return ex.Message
                End Try
            End Function
        End Class
#End Region

    End Class

End Class

