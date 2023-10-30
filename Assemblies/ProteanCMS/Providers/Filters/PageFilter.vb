

Imports System.Data.SqlClient
Imports System.Xml
Imports Microsoft
Imports Microsoft.ClearScript.Util
Imports Protean.Cms
Imports Protean.xForm

Namespace Providers
    Namespace Filters

        Public Class PageFilter

            Public Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
            Public Sub AddControl(ByRef aWeb As Cms, ByRef FilterConfig As XmlElement, ByRef oXform As xForm, ByRef oFromGroup As XmlElement, ByRef oContentNode As XmlElement, ByVal cWhereSql As String)
                Dim cProcessInfo As String = "AddControl"
                Try
                    Dim pageFilterSelect As XmlElement
                    'Dim pageFilterButtons As XmlElement
                    Dim sCotrolDisplayName As String = "Page Filter"
                    'Parent page id flag used to populate the root level pages or pages under current page.
                    Dim bParentPageId As Boolean = False
                    Dim cFilterTarget As String = String.Empty

                    Dim nParentId As Integer = 1
                    Dim sSql As String = "spGetPagesByParentPageId"
                    Dim arrParams As New Hashtable
                    Dim oXml As XmlElement = oXform.moPageXML.CreateElement("PageFilter")
                    Dim oFilterElmt As XmlElement = Nothing
                    Dim className As String = String.Empty

                    If (oContentNode.Attributes("filterTarget") IsNot Nothing) Then
                        cFilterTarget = oContentNode.Attributes("filterTarget").Value
                    End If
                    If (aWeb.moRequest.Form("PageFilter") IsNot Nothing) Then
                        oXml.InnerText = Convert.ToString(aWeb.moRequest.Form("PageFilter"))

                    End If



                    oXform.Instance.AppendChild(oXml)


                    ' Adding a binding to the form bindings
                    oXform.addBind("PageFilter", "PageFilter", "false()", "string", oXform.model,)
                    If (FilterConfig.Attributes("name") IsNot Nothing) Then
                        sCotrolDisplayName = Convert.ToString(FilterConfig.Attributes("name").Value)
                    End If
                    'Get Parent page id flag and current id
                    If (FilterConfig.Attributes("parId") IsNot Nothing) Then
                        nParentId = Convert.ToInt32(FilterConfig.Attributes("parId").Value)
                    End If
                    If (FilterConfig.Attributes("parentPageId").Value IsNot Nothing) Then
                        bParentPageId = Convert.ToBoolean(Convert.ToInt32(FilterConfig.Attributes("parentPageId").Value))
                    End If
                    If (bParentPageId) Then
                        arrParams.Add("PageId", nParentId)
                        arrParams.Add("FilterTarget", cFilterTarget)
                        arrParams.Add("whereSql", cWhereSql)
                    End If


                    Using oDr As SqlDataReader = aWeb.moDbHelper.getDataReaderDisposable(sSql, CommandType.StoredProcedure, arrParams)  'Done by nita on 6/7/22
                        'Adding controls to the form like dropdown, radiobuttons
                        If (oDr IsNot Nothing AndAlso oDr.HasRows) Then

                            If (oXml.InnerText <> String.Empty) Then

                                pageFilterSelect = oXform.addSelect(oFromGroup, "PageFilter", False, sCotrolDisplayName, "checkbox SubmitPageFilter filter-selected", ApperanceTypes.Full)
                            Else
                                pageFilterSelect = oXform.addSelect(oFromGroup, "PageFilter", False, sCotrolDisplayName, "checkbox SubmitPageFilter", ApperanceTypes.Full)
                            End If

                            'oXform.addOptionsFromSqlDataReader(pageFilterSelect, oDr, "name", "nStructKey")
                            While oDr.Read
                                Dim name As String = Convert.ToString(oDr("cStructName")) + " <span class='ProductCount'>" + Convert.ToString(oDr("ContentCount")) + "</span>"
                                Dim value As String = Convert.ToString(oDr("nStructKey"))

                                oXform.addOption(pageFilterSelect, name, value, True)

                            End While
                        End If

                    End Using
                    If (oFromGroup.SelectSingleNode("select[@ref='PageFilter']/item") IsNot Nothing) Then
                        If (oXml.InnerText.Trim() <> "") Then
                            Dim sText As String
                            'Dim sValue As String
                            Dim cnt As Integer
                            Dim aPages() As String = oXml.InnerText.Split(",")
                            If (aPages.Length <> 0 And aPages.Length <> Nothing) Then
                                For cnt = 0 To aPages.Length - 1
                                    sText = oFromGroup.SelectSingleNode("select[@ref='PageFilter']/item[value='" + aPages(cnt) + "']").FirstChild().FirstChild().InnerText

                                    oXform.addSubmit(oFromGroup, sText, sText, "PageFilter_" & aPages(cnt), " btnCross filter-applied", "fa-times")

                                Next

                            Else

                                sText = oFromGroup.SelectSingleNode("select[@ref='PageFilter']/item[value='" + oXml.InnerText + "']").FirstChild().FirstChild().InnerText
                                oXform.addSubmit(oFromGroup, sText, sText, "PageFilter", " btnCross filter-applied", "fa-times")
                            End If
                        End If
                    End If
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(cProcessInfo, "PageFilter", ex, ""))
                End Try
            End Sub

            Public Function ApplyFilter(ByRef aWeb As Cms, ByRef cWhereSql As String, ByRef oXform As xForm, ByRef oFromGroup As XmlElement, ByRef FilterConfig As XmlElement, ByRef cFilterTarget As String) As String
                Dim cProcessInfo As String = "ApplyFilter"
                Try

                    'Get the filter type parent or child based on the value of the parentPageId attribute
                    Dim bParentPageId As Boolean = False
                    If (FilterConfig.Attributes("parentPageId").Value IsNot Nothing) Then
                        bParentPageId = Convert.ToBoolean(Convert.ToInt32(FilterConfig.Attributes("parentPageId").Value))
                    End If

                    Dim cPageIds As String = String.Empty

                    If (oXform.Instance.SelectSingleNode("PageFilter") IsNot Nothing) Then
                        cPageIds = oXform.Instance.SelectSingleNode("PageFilter").InnerText

                    End If



                    If (cPageIds <> String.Empty) Then



                        If (cWhereSql <> String.Empty) Then
                            cWhereSql = " AND "
                        End If
                        If (bParentPageId) Then
                            cWhereSql = " nStructId IN (" + cPageIds + ")"
                        Else
                            cWhereSql = " nStructId IN (select nStructKey from tblContentStructure where (nStructKey = " & cPageIds & " OR nStructParId = " & cPageIds & ")	)"
                            'nStructParId in (" & cPageIds & "))"
                        End If
                    End If
                    Return cWhereSql
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(cProcessInfo, "PageFilter", ex, ""))
                    Return Nothing
                End Try

            End Function


            Public Function GetFilterSQL(ByRef aWeb As Cms) As String
                Dim cWhereSql As String = String.Empty
                Dim cProcessInfo As String = "GetFilterSQL"
                Dim cPageIds As String = String.Empty
                Try
                    If (aWeb.moRequest.Form("PageFilter") IsNot Nothing) Then
                        '  cWhereSql = cWhereSql & "  nStructId IN(" + aWeb.moRequest.Form("PageFilter") & ")"
                        cWhereSql = " nStructId IN (select nStructKey from tblContentStructure where (nStructKey = " & aWeb.moRequest.Form("PageFilter") & " OR nStructParId = " & aWeb.moRequest.Form("PageFilter") & ")	)"

                    End If

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(cProcessInfo, "PageFilter", ex, ""))
                End Try
                Return cWhereSql
            End Function


        End Class

    End Namespace
End Namespace


