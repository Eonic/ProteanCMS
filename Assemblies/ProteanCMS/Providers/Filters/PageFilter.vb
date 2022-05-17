

Imports System.Data.SqlClient
Imports System.Xml
Imports Protean.Cms
Imports Protean.xForm

Namespace Providers
    Namespace Filter

        Public Class PageFilter


            Public Sub AddControl(ByRef aWeb As Cms, ByRef nPageId As Integer, ByRef oXform As xForm, ByRef oFromGroup As XmlElement)
                Try
                    Dim pageFilterSelect As XmlElement
                    Dim oDr As SqlDataReader

                    Dim sSql As String = "spGetPagesByParentPageId"
                    oDr = aWeb.moDbHelper.getDataReader(sSql, CommandType.StoredProcedure)
                    'Adding controls to the form like dropdown, radiobuttons
                    pageFilterSelect = oXform.addSelect(oFromGroup, "PageFilter", False, "Select By Page", "checkbox", ApperanceTypes.Full)
                    oXform.addOptionsFromSqlDataReader(pageFilterSelect, oDr, "cStructName", "nStructKey")

                Catch ex As Exception

                End Try
            End Sub

            Public Sub AddControlForPrice(ByRef aWeb As Cms, ByRef nPageId As Integer, ByRef oXform As xForm, ByRef oFromGroup As XmlElement)
                Try
                    Dim pageFilterSelect As XmlElement
                    Dim oDr As SqlDataReader


                    Dim sSql As String = "spGetPagesByParentPageId"
                    oDr = aWeb.moDbHelper.getDataReader(sSql, CommandType.StoredProcedure)
                    'Adding controls to the form like dropdown, radiobuttons
                    oXform.addRange(oFromGroup, "PriceFilter", True, "Price Range", 10, 30, 1)
                    pageFilterSelect = oXform.addSelect(oFromGroup, "PageFilter", False, "Select By Page", "Price Range", ApperanceTypes.Full)
                    oXform.addOptionsFromSqlDataReader(pageFilterSelect, oDr, "cStructName", "nStructKey")
                Catch ex As Exception

                End Try
            End Sub
            Public Sub ApplyFilter(ByRef aWeb As Cms, ByRef nPageId As Integer, ByRef oXform As xForm, ByRef oFromGroup As XmlElement)
                Try


                    Dim cWhereSql As String = String.Empty
                    Dim cPageIds As String = String.Empty
                    Dim cnt As Integer

                    If (oXform.Instance.SelectNodes("PageFilter") IsNot Nothing) Then
                        cPageIds = oXform.Instance.SelectNodes("PageFilter")(0).InnerText
                        If (aWeb.moSession("PageIds") Is Nothing) Then
                            aWeb.moSession("PageIds") = cPageIds
                        Else
                            aWeb.moSession("PageIds") = cPageIds
                            cPageIds = aWeb.moSession("PageIds")
                        End If

                    End If

                    If (cPageIds <> String.Empty) Then

                        If (cWhereSql = String.Empty) Then
                            cWhereSql = cWhereSql + cPageIds.ToString() + ","
                        End If
                        'call sp and return xml data
                        If (cWhereSql <> String.Empty) Then
                            cWhereSql = cWhereSql.Substring(0, cWhereSql.Length - 1)
                            cWhereSql = " nStructId IN (" + cWhereSql + ")"
                            aWeb.GetPageContentFromSelect(cWhereSql,,,,,,,,,,, "Product")
                        End If

                        Dim aPageId() As String = cPageIds.Split(",")
                        For cnt = 0 To aPageId.Length - 1 Step 1
                            If (aPageId(cnt) <> String.Empty) Then

                                ' oXform.addRepeat(oFromGroup, aPageId(cnt), "search-filter", aPageId(cnt))
                                oXform.addSubmit(oFromGroup, "removePage", aPageId(cnt), "submit", "", aPageId(cnt))

                            End If
                        Next

                    End If

                Catch ex As Exception

                End Try
            End Sub

            Public Sub RemovePageFromFilter(ByRef aWeb As Cms, ByVal cPageId As String)
                Try
                    Dim cnt As Integer
                    Dim cntPages As Integer = 0
                    Dim cPageIds As String = String.Empty
                    If (aWeb.moSession("PageIds") IsNot Nothing) Then
                        cPageIds = aWeb.moSession("PageIds")
                        cPageIds = cPageIds.Replace(cPageId, "")

                        Dim aPageId() As String = cPageIds.Split(",")
                        For cnt = 0 To aPageId.Length - 1 Step 1
                            If (aPageId(cnt) <> String.Empty) Then
                                If aPageId(cnt) <> "" Then
                                    cPageIds = cPageIds + aPageId(cnt) + ","
                                End If
                            End If
                        Next
                        aWeb.moSession("PageIds") = Left(cPageIds, cPageIds.Length - 1)
                    End If

                Catch ex As Exception

                End Try
            End Sub
            'Public Function RemovePageFromFilter(ByRef myApi As Protean.API, ByRef jObj As Newtonsoft.Json.Linq.JObject) As String
            '    Try
            '        If (myA.moSession("PageIds") IsNot Nothing) Then

            '            aWeb.moSession.Remove("PageIds")
            '        End If
            '    Catch ex As Exception

            '    End Try
            'End Function

        End Class
        ' End Class
    End Namespace
End Namespace


