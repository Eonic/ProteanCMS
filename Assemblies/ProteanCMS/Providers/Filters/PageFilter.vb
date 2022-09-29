

Imports System.Data.SqlClient
Imports System.Xml
Imports Protean.Cms
Imports Protean.xForm

Namespace Providers
    Namespace Filters

        Public Class PageFilter

            Public Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
            Public Sub AddControl(ByRef aWeb As Cms, ByRef FilterConfig As XmlElement, ByRef oXform As xForm, ByRef oFromGroup As XmlElement)
                Dim cProcessInfo As String = "AddControl"
                Try
                    Dim pageFilterSelect As XmlElement
                    'Parent page id flag used to populate the root level pages or pages under current page.
                    Dim bParentPageId As Boolean = False
                    Dim nParentId As Integer = 1
                    Dim sSql As String = "spGetPagesByParentPageId"
                    Dim arrParams As New Hashtable
                    Dim oXml As XmlElement = oXform.moPageXML.CreateElement("PageFilter")
                    If (aWeb.moSession("PageFilter") IsNot Nothing) Then
                        oXml.InnerText = Convert.ToString(aWeb.moSession("PageFilter"))
                    End If
                    oXform.Instance.AppendChild(oXml)


                    ' Adding a binding to the form bindings
                    oXform.addBind("PageFilter", "PageFilter", "false()", "string", oXform.model,)


                    'Get Parent page id flag and current id
                    If (FilterConfig.Attributes("parId") IsNot Nothing) Then
                        nParentId = Convert.ToInt32(FilterConfig.Attributes("parId").Value)
                    End If
                    If (FilterConfig.Attributes("parentPageId").Value IsNot Nothing) Then
                        bParentPageId = Convert.ToBoolean(Convert.ToInt32(FilterConfig.Attributes("parentPageId").Value))
                    End If
                    If (bParentPageId) Then
                        arrParams.Add("PageId", nParentId)
                    End If
                    Using oDr As SqlDataReader = aWeb.moDbHelper.getDataReaderDisposable(sSql, CommandType.StoredProcedure, arrParams)  'Done by nita on 6/7/22
                        'Adding controls to the form like dropdown, radiobuttons
                        pageFilterSelect = oXform.addSelect(oFromGroup, "PageFilter", False, "Page Filter", "checkbox", ApperanceTypes.Full)
                        oXform.addOptionsFromSqlDataReader(pageFilterSelect, oDr, "name", "nStructKey")
                    End Using

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(cProcessInfo, "PageFilter", ex, ""))
                End Try
            End Sub

            Public Function ApplyFilter(ByRef aWeb As Cms, ByRef cWhereSql As String, ByRef oXform As xForm, ByRef oFromGroup As XmlElement) As String
                Dim cProcessInfo As String = "ApplyFilter"
                Try


                    Dim cPageIds As String = String.Empty

                    If (oXform.Instance.SelectSingleNode("PageFilter") IsNot Nothing) Then
                        cPageIds = oXform.Instance.SelectSingleNode("PageFilter").InnerText

                    End If

                    If (cPageIds <> String.Empty) Then

                        aWeb.moSession("PageFilter") = cPageIds

                        If (cWhereSql <> String.Empty) Then
                            cWhereSql = " AND "
                        End If
                        cWhereSql = " nStructId IN (" + cPageIds + ")"

                    End If
                    Return cWhereSql
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(cProcessInfo, "PageFilter", ex, ""))
                End Try
            End Function

            Public Sub RemovePageFromFilter(ByRef aWeb As Cms, ByVal cPageId As String)
                Dim cProcessInfo As String = "RemovePageFromFilter"
                Try
                    Dim cnt As Integer
                    Dim cntPages As Integer = 0
                    Dim cPageIds As String = String.Empty
                    If (aWeb.moSession("PageFilter") IsNot Nothing) Then
                        cPageIds = aWeb.moSession("PageFilter")
                        cPageIds = cPageIds.Replace(cPageId, "")

                        Dim aPageId() As String = cPageIds.Split(",")
                        For cnt = 0 To aPageId.Length - 1 Step 1
                            If (aPageId(cnt) <> String.Empty) Then
                                If aPageId(cnt) <> "" Then
                                    cPageIds = cPageIds + aPageId(cnt) + ","
                                End If
                            End If
                        Next
                        aWeb.moSession("PageFilter") = Left(cPageIds, cPageIds.Length - 1)
                    End If

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(cProcessInfo, "PageFilter", ex, ""))
                End Try
            End Sub

        End Class
        ' End Class
    End Namespace
End Namespace


