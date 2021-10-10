
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

            Public Shadows Sub Open(ByVal oPageXml As XmlDocument)
                Dim cProcessInfo As String = ""
                Try
                    moAdXfm.moPageXML = oPageXml

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "Open", ex, "", cProcessInfo, gbDebug)
                End Try
            End Sub


            Public Function LoadUrlsForPagination(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String

                Dim redirectType As String = inputJson("redirectType").ToObject(Of String)
                Dim pageloadCount As Integer = inputJson("loadCount").ToObject(Of Integer)
                Dim JsonResult As String = ""
                Try
                    If myApi.mbAdminMode Then
                        JsonResult = moAdminRedirect.LoadUrlsForPegination(redirectType, pageloadCount)
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

            Public Function SearchUrl(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String
                Dim redirectType As String = inputJson("redirectType").ToObject(Of String)
                Dim searchObj As String = inputJson("searchObj").ToObject(Of String)
                Dim pageloadCount As Integer = inputJson("loadCount").ToObject(Of Integer)
                Try
                    Dim JsonResult As String = ""
                    If myApi.mbAdminMode Then
                        JsonResult = moAdminRedirect.SearchUrl(redirectType, searchObj, pageloadCount)
                    End If
                    Return JsonResult

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""))
                    Return ex.Message
                End Try

            End Function



            Public Function SaveUrls(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String
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

            Public Function DeleteUrls(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String
                Dim redirectType As String = inputJson("redirectType").ToObject(Of String)
                Dim oldUrl As String = inputJson("oldUrl").ToObject(Of String)
                Dim newUrl As String = inputJson("NewUrl").ToObject(Of String)
                Dim JsonResult As String = ""

                Try
                    If myApi.mbAdminMode Then
                        JsonResult = moAdminRedirect.DeleteUrls(redirectType, oldUrl, newUrl)
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

            Public Function GetTotalNumberOfUrls(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String
                Dim JsonResult As String = ""
                Dim redirectType As String = inputJson("redirectType").ToObject(Of String)

                If myApi.mbAdminMode Then
                    JsonResult = moAdminRedirect.getTotalNumberOfUrls(redirectType)
                End If
                Return JsonResult
            End Function
            Public Function GetTotalNumberOfSearchUrls(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String
                Dim JsonResult As String = ""
                Dim redirectType As String = inputJson("redirectType").ToObject(Of String)
                Dim SearchObj As String = inputJson("searchObj").ToObject(Of String)

                If myApi.mbAdminMode Then
                    JsonResult = moAdminRedirect.GetTotalNumberOfSearchUrls(redirectType, SearchObj)
                End If
                Return JsonResult
            End Function

            Public Function LoadAllUrls(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String

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

            'Charts
            Public Function GetChartData(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String

                Dim JsonResult As String = ""
                Dim chartContentKey As Integer
                If Not Integer.TryParse(inputJson("chartContentKey"), chartContentKey) Then
                    chartContentKey = 0
                End If

                Try
                    If chartContentKey > 0 Then
                        Dim dsChartData As DataSet

                        Dim sSql As String = "SELECT C.nContentKey,"
                        sSql &= " C.cContentName,"
                        sSql &= " Convert(Xml, C.cContentXmlBrief).value('(Content/cDisplayName)[1]', 'Varchar(50)') AS displayName,"
                        sSql &= " CONVERT(XML, C.cContentXmlBrief).value('(Content/@lineColor)[1]', 'Varchar(50)') AS lineColor,"
                        sSql &= " CONVERT(XML, C.cContentXmlBrief).value('(Content/@lineTension)[1]', 'Varchar(10)') AS lineTension,"
                        sSql &= " CONVERT(XML, C.cContentXmlBrief).value('(Content/@label-x)[1]', 'Varchar(10)') AS xLabelPosition,"
                        sSql &= " CONVERT(XML, C.cContentXmlBrief).value('(Content/@label-y)[1]', 'Varchar(10)') AS yLabelPosition,"
                        sSql &= " '' AS url,"
                        sSql &= " P.ProductId AS productId,"
                        sSql &= " CD.D.value('(@x)[1]', 'Varchar(10)') AS xLoc,"
                        sSql &= " CD.D.value('(@y)[1]', 'Varchar(10)') AS yLoc"
                        sSql &= " FROM tblContentRelation CR"
                        sSql &= " JOIN tblContent C ON C.nContentKey = CR.nContentChildId"
                        sSql &= " OUTER APPLY"
                        sSql &= " ("
                        sSql &= " SELECT CR1.nContentChildId AS ProductId"
                        sSql &= " FROM tblContentRelation CR1"
                        sSql &= " JOIN tblContent C1 ON C1.nContentKey = CR1.nContentChildId"
                        sSql &= " WHERE CR1.nContentParentId = C.nContentKey AND C1.cContentSchemaName = 'Product'"
                        sSql &= " ) P"
                        sSql &= " OUTER APPLY (SELECT CAST(C.cContentXmlBrief as xml) as cContentXmlBriefxml) CB"
                        sSql &= " OUTER APPLY CB.cContentXmlBriefxml.nodes('/Content/dataset/datapoint') as CD(D) "
                        sSql &= " WHERE nContentParentId = " & chartContentKey & " AND C.cContentSchemaName = 'ChartDataSet'"

                        dsChartData = myWeb.moDbHelper.GetDataSet(sSql, "ChartDataSet", "Chart")

                        If Not dsChartData Is Nothing Then
                            'Update the contentUrls
                            If dsChartData.Tables.Count > 0 Then
                                For Each oRow As DataRow In dsChartData.Tables(0).Rows
                                    If Not oRow("productId") Is Nothing And Not oRow("productId") Is System.DBNull.Value Then
                                        oRow("url") = myWeb.GetContentUrl(oRow("productId"))
                                    End If
                                Next
                            End If

                            Dim chartXml As String = dsChartData.GetXml()
                            Dim xmlDoc As New XmlDocument
                            xmlDoc.LoadXml(chartXml)

                            Dim jsonString As String = Newtonsoft.Json.JsonConvert.SerializeXmlNode(xmlDoc.DocumentElement, Newtonsoft.Json.Formatting.Indented)
                            Return jsonString.Replace("""@", """_")
                        End If

                        Return String.Empty

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

            Public Function RedirectPage(ByRef myApi As Protean.API, ByRef inputJson As Newtonsoft.Json.Linq.JObject) As String
                Dim JsonResult As String = ""
                Dim redirectType As String = inputJson("redirectType").ToObject(Of String)
                Dim oldUrl As String = inputJson("oldUrl").ToObject(Of String)
                Dim newUrl As String = inputJson("newUrl").ToObject(Of String)
                'Dim bChildPageRedirect As String = inputJson("isParent").ToObject(Of String)
                Dim hiddenOldUrl As String = ""
                Dim pageId As String = inputJson("pageId").ToObject(Of String)
                Dim isParentPage As String = inputJson("isParent").ToObject(Of String)
                Dim sType As String = inputJson("pageType").ToObject(Of String)
                Try
                    If myApi.mbAdminMode Then
                        JsonResult = moAdminRedirect.RedirectPage(redirectType, oldUrl, newUrl, hiddenOldUrl, isParentPage, sType, pageId)
                    End If

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

