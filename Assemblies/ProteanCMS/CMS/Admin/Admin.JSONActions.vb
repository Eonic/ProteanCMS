
Option Strict Off
Option Explicit On
Imports System.Xml
Imports System.Collections
Imports System.Web.Configuration
Imports System.Configuration
Imports Alphaleonis.Win32.Filesystem
Imports System.Drawing.Imaging
Imports System.Data.SqlClient

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
            Public moCtx As System.Web.HttpContext




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
                moCtx = myWeb.moCtx

            End Sub

            Public Shadows Sub Open(ByVal oPageXml As XmlDocument)
                Dim cProcessInfo As String = ""
                Try
                    moAdXfm.moPageXML = oPageXml

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "Open", ex, "", cProcessInfo, gbDebug)
                End Try
            End Sub



            Public Function DeleteObject(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String
                Dim ObjType As String = ""  'Just declare at top
                Dim result As Long
                Dim ObjId As String = ""
                Try

                    If ValidateAPICall(myWeb, "Administrator") Then

                        If inputJson("objType") IsNot Nothing Then
                            ObjType = inputJson("ObjType").ToObject(Of String)()
                        End If

                        If inputJson("objId") IsNot Nothing Then
                            ObjId = inputJson("objId").ToObject(Of String)()
                        End If
                        result = myWeb.moDbHelper.DeleteObject(ObjType, ObjId, False)

                    End If
                    Return "[{""Key"":""" & ObjId & """,""Value"":""" & result & """}]"
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Query", ex, ""))
                    Return ex.Message
                End Try
            End Function

            Public Function QueryValue(ByRef myApi As Protean.API, ByRef jObj As Newtonsoft.Json.Linq.JObject) As String
                Try
                    Dim count As String = "0"
                    Dim bIsAuthorized As Boolean = False
                    bIsAuthorized = ValidateAPICall(myWeb, "Administrator")
                    If bIsAuthorized Then

                        Dim sSql = myApi.moConfig(myApi.moRequest("query"))

                        Dim result = myWeb.moDbHelper.GetDataValue(sSql, System.Data.CommandType.StoredProcedure)
                        count = If(result Is Nothing, "", Convert.ToString(result))

                    End If
                    Return "[{""Key"":""" & myApi.moRequest("query") & """,""Value"":""" & count & """}]"

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

                If inputJson("pageurl") IsNot Nothing Then
                    hiddenOldUrl = inputJson("pageurl").ToObject(Of String)()
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

            Public Function ReIndexingAPI(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String
                Dim sString As String
                Try
                    Dim objservices As Services = New Services()

                    If objservices.CheckUserIP() Then
                        Dim bIsAuthorized As Boolean = False
                        bIsAuthorized = ValidateAPICall(myWeb, "Administrator")
                        If bIsAuthorized Then
                            Dim objAdmin As Admin = New Admin()
                            objAdmin.ReIndexing(myWeb)
                            sString = "success"
                        Else
                            sString = "Invalid authentication"
                        End If

                    Else
                        sString = "No access to this IPAddress"
                    End If
                    Return sString
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "IsParentPage", ex, ""))
                    Return ex.Message
                End Try

            End Function

            Public Function CleanfileName(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String
                Dim JsonResult As String = ""
                Dim Filename As String = String.Empty
                Dim fsHelper As New fsHelper()

                Try
                    If myApi.mbAdminMode Then

                        If myApi.moRequest.QueryString("Filename") IsNot Nothing Then
                            Filename = myApi.moRequest.QueryString("Filename")
                        End If
                        If Filename <> String.Empty Then
                            JsonResult = fsHelper.CleanfileName(Filename)
                        End If

                    End If
                    Return JsonResult
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ReplaceRegularExpression", ex, ""))
                    Return ex.Message
                End Try
            End Function
            Public Function GetExistsFileName(ByRef myApi As Protean.API, ByRef jObj As Newtonsoft.Json.Linq.JObject) As String
                Try
                    Return moCtx.Session("ExistsFileName")
                Catch
                Finally
                    moCtx.Session("ExistsFileName") = Nothing
                End Try
            End Function
            Public Function CompressImage(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String
                Dim JsonResult As String = "0"
                Dim Filename As String = String.Empty
                Dim fsHelper As New fsHelper()
                Dim TinyAPIKey As String = goConfig("TinifyKey")
                Try
                    If myApi.mbAdminMode Then

                        If myApi.moRequest.QueryString("Filename") IsNot Nothing Then
                            Dim oImgTool As New Protean.Tools.Image("")
                            oImgTool.TinifyKey = TinyAPIKey
                            Dim oFile As IO.FileInfo = New IO.FileInfo(myApi.goServer.MapPath(myApi.moRequest.QueryString("Filename")))
                            JsonResult = "reduction:'" & (oImgTool.CompressImage(oFile, True) / 1000) & "'"
                            oFile.Refresh()
                            JsonResult = JsonResult & ",new_size:'" & (oFile.Length / 1000) & "'"
                        End If

                    End If
                    Return JsonResult
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ReplaceRegularExpression", ex, ""))
                    Return ex.Message
                End Try
            End Function
            Public Function SendReviewCompleteEmail(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String
                Dim JsonResult As String = ""
                Dim ReviewId As String = String.Empty
                Dim fsHelper As New fsHelper()

                Try
                    If Not inputJson Is Nothing Then
                        If inputJson("ReviewId").ToString() IsNot Nothing Then
                            ReviewId = inputJson("ReviewId").ToString()
                        End If
                    Else
                        ReviewId = myWeb.moRequest("id")
                    End If
                    'Send Email
                    Dim oMsg As New Protean.Messaging()
                    Dim doc = New XmlDocument()
                    Dim strUrl As String = "https://www.intotheblue.co.uk"
                    Dim CustomerName As String = String.Empty
                    Dim strEmail As String = ""
                    Dim moMailConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/mailinglist")
                    Dim xsltPath As String = moMailConfig("ReviewCompleteEmailTemplatePath")

                    Dim dtReviewerEmail As DataTable = New DataTable()
                    Dim arrParms = New System.Collections.Hashtable()
                    arrParms.Add("ReviewId", ReviewId)
                    Using oDr As SqlDataReader = myWeb.moDbHelper.getDataReaderDisposable("spSendEmailAfterSubmitReview", CommandType.StoredProcedure, arrParms)
                        If oDr IsNot Nothing Then
                            While oDr.Read()
                                Dim ReviewContentXML As XmlNode = doc.CreateElement("ReviewContentXML")
                                Dim cContentXmlDetail As String = Convert.ToString(oDr("cContentXmlDetail"))
                                If cContentXmlDetail <> "" Then
                                    ReviewContentXML.InnerXml = cContentXmlDetail
                                    CustomerName = Convert.ToString(ReviewContentXML.SelectSingleNode("Content/Reviewer/node()").Value)

                                    If ReviewContentXML.SelectSingleNode("Content/ReviewerEmail/node()") IsNot Nothing Then
                                        strEmail = Convert.ToString(ReviewContentXML.SelectSingleNode("Content/ReviewerEmail/node()").Value)
                                        Dim xmlDetails As XmlElement = doc.CreateElement("Order")
                                        xmlDetails.InnerXml = "<strUrl>" & strUrl & "</strUrl>"
                                        xmlDetails.InnerXml = xmlDetails.InnerXml & "<CustomerName>" & CustomerName & "</CustomerName>"
                                        'Send Email
                                        oMsg.emailer(xmlDetails, xsltPath, "INTOTHEBLUE experience completion", moMailConfig("FromEmail"), strEmail, "Thank you")
                                    End If
                                End If
                            End While
                        End If
                    End Using

                    Return JsonResult
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ReplaceRegularExpression", ex, ""))
                    Return ex.Message
                End Try
            End Function

            Public Function SaveMultipleLibraryImages(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String
                Dim JsonResult As String = ""
                Dim nContentId As String = ""
                Dim cRelatedLibraryImages As String = ""
                Dim cSkipAttribute As String = "false"
                Dim count As Integer = moCtx.Request.Files.Count

                If moCtx.Request("contentId") IsNot Nothing Then
                    nContentId = moCtx.Request("contentId")
                End If

                If moCtx.Request("cRelatedLibraryImages") IsNot Nothing Then
                    cRelatedLibraryImages = moCtx.Request("cRelatedLibraryImages")
                End If

                Try
                    If myApi.mbAdminMode Then
                        JsonResult = myWeb.moDbHelper.CreateLibraryImages(nContentId, cRelatedLibraryImages, cSkipAttribute, "LibraryImage")
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

