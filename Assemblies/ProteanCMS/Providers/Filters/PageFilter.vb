

Imports System.Data.SqlClient
Imports System.Xml

Namespace Providers
    Namespace Filter

        'Extending the class path
        ' Public Class BaseProvider
        Public Class PageFilter

            Public Sub AddControl(ByRef aWeb As Cms, ByRef nPageId As Integer, ByRef oXform As xForm, ByRef oFromGroup As XmlElement)
                Try
                    Dim pageFilterSelect As XmlElement
                    Dim oDr As SqlDataReader
                    'Create Stored procedure 
                    'oDr
                    'Adding controls to the form like dropdown, radiobuttons
                    pageFilterSelect = oXform.addSelect1(oFromGroup, "PageFilter", False, "Select By Page")
                    oXform.addOptionsFromSqlDataReader(pageFilterSelect, oDr)




                Catch ex As Exception

                End Try
            End Sub


            Public Sub ApplyFilter(ByRef aWeb As Cms, ByRef nPageId As Integer, ByRef oXform As xForm, ByRef oFromGroup As XmlElement)
                Try

                    Dim cWhereSql As String = String.Empty
                    'If (oFilterNode.SelectNodes("Filter/PageId") Is Nothing) Then
                    '    nPageId = oFilterNode.SelectNodes("Filter/PageId").ToString()
                    'End If



                    Dim oMenuItem As XmlElement
                    If (nPageId <> 0) Then

                        'Dim oMenuElmt As XmlElement = aWeb.GetStructureXML(aWeb.mnUserId, nPageId, 0, "Site", False, False, False, True, False, "MenuItem", "Menu")
                        Dim oSubMenuList As XmlNodeList = aWeb.moPageXml.SelectSingleNode("/Page/Menu/MenuItem/descendant-or-self::MenuItem[@id='" & nPageId & "']").SelectNodes("MenuItem")
                        For Each oMenuItem In oSubMenuList
                            cWhereSql = cWhereSql + oMenuItem.Attributes("id").InnerText.ToString() + ","
                        Next
                        If (cWhereSql = String.Empty) Then
                            cWhereSql = cWhereSql + nPageId.ToString() + ","
                        End If
                        'call sp and return xml data
                        If (cWhereSql <> String.Empty) Then
                            cWhereSql = cWhereSql.Substring(0, cWhereSql.Length - 1)
                            cWhereSql = " nStructId IN (" + cWhereSql + ")"
                            aWeb.GetPageContentFromSelect(cWhereSql,,,,,,,,,,, "Product")
                        End If

                    End If

                Catch ex As Exception

                End Try
            End Sub

        End Class
        ' End Class
    End Namespace
End Namespace


