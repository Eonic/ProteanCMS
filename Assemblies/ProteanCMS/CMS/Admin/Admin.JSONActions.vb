
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
                Dim JsonResult As String = ""
                Try
                    If myApi.mbAdminMode Then
                        JsonResult = moAdminRedirect.urlsForPegination(redirectType, pageloadCount)
                    End If
                    Return JsonResult
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""))
                    Return ex.Message
                End Try

            End Function

            Public Function AddNewUrl(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String
                Dim JsonResult As String = ""
                Dim redirectType As String = inputJson("redirectType").ToObject(Of String)
                Dim oldUrl As String = inputJson("oldUrl").ToObject(Of String)
                Dim newUrl As String = inputJson("newUrl").ToObject(Of String)
                Try
                    If myApi.mbAdminMode Then
                        JsonResult = moAdminRedirect.CreateRedirect(redirectType, oldUrl, newUrl)
                    End If

                    Return JsonResult
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""))
                    Return ex.Message
                End Try

            End Function

            Public Function searchUrl(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String
                Dim redirectType As String = inputJson("redirectType").ToObject(Of String)
                Dim searchObj As String = inputJson("searchObj").ToObject(Of String)
                Dim pageloadCount As Integer = inputJson("loadCount").ToObject(Of Integer)
                Try
                    Dim JsonResult As String = ""
                    If myApi.mbAdminMode Then
                        JsonResult = moAdminRedirect.searchUrl(redirectType, searchObj, pageloadCount)
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
                    If myApi.mbAdminMode Then
                        JsonResult = moAdminRedirect.CreateRedirect(redirectType, oldUrl, newUrl, hiddenOldUrl)
                    End If
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

                Try
                    If myApi.mbAdminMode Then
                        JsonResult = moAdminRedirect.deleteUrls(redirectType, oldUrl, newUrl)
                    End If

                    Return JsonResult
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""))
                    Return ex.Message
                End Try
            End Function

            Public Function IsUrlPresent(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String
                Dim JsonResult As String = ""
                Dim redirectType As String = inputJson("redirectType").ToObject(Of String)
                Dim oldUrl As String = inputJson("oldUrl").ToObject(Of String)
                If myApi.mbAdminMode Then
                    JsonResult = moAdminRedirect.IsUrlPresent(redirectType, oldUrl)
                End If
                Return JsonResult
            End Function

            Public Function getTotalNumberOfUrls(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String
                Dim JsonResult As String = ""
                Dim redirectType As String = inputJson("redirectType").ToObject(Of String)

                If myApi.mbAdminMode Then
                    JsonResult = moAdminRedirect.getTotalNumberOfUrls(redirectType)
                End If
                Return JsonResult
            End Function
            Public Function getTotalNumberOfSearchUrls(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String
                Dim JsonResult As String = ""
                Dim redirectType As String = inputJson("redirectType").ToObject(Of String)
                Dim SearchObj As String = inputJson("searchObj").ToObject(Of String)

                If myApi.mbAdminMode Then
                    JsonResult = moAdminRedirect.getTotalNumberOfSearchUrls(redirectType, SearchObj)
                End If
                Return JsonResult
            End Function

            Public Function loadAllUrls(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String

                Dim redirectType As String = inputJson("redirectType").ToObject(Of String)
                Dim pageloadCount As Integer = inputJson("loadCount").ToObject(Of Integer)
                Dim actionFlag As String = inputJson("flag").ToObject(Of String)

                Dim JsonResult As String = ""
                Try
                    If myApi.mbAdminMode Then

                        JsonResult = moAdminRedirect.LoadAllurls(redirectType, pageloadCount, actionFlag)
                    End If
                    Return JsonResult
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""))
                    Return ex.Message
                End Try

            End Function

            Public Function IsParentPage(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String
                Dim JsonResult As String = ""
                Dim pageId As String = inputJson("pageId").ToObject(Of Integer)

                If myApi.mbAdminMode Then
                    JsonResult = moAdminRedirect.isParentPage(pageId)
                End If
                Return JsonResult
            End Function
        End Class
#End Region

    End Class

End Class

