

Imports System.Data.SqlClient
Imports System.Xml
Imports Protean.Cms
Imports Protean.xForm

Namespace Providers

    Namespace Filters

        Public Class PriceFilter

            Public Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
            Public Sub AddControl(ByRef aWeb As Cms, ByRef FilterConfig As XmlElement, ByRef oXform As xForm, ByRef oFromGroup As XmlElement)
                Dim cProcessInfo As String = "AddControl"
                Try

                    oXform.Instance.AppendChild(oXform.moPageXML.CreateElement("PriceFilter"))

                    ' Adding a binding to the form bindings
                    oXform.addBind("PriceFilter", "PriceFilter", "false()", "string", oXform.model)


                    Dim nMinPrice As Double = Convert.ToDouble(FilterConfig.Attributes("fromPrice").Value)
                    Dim nMaxPrice As Double = Convert.ToDouble(FilterConfig.Attributes("toPrice").Value)
                    Dim nStep As Integer = Convert.ToDouble(FilterConfig.Attributes("step").Value)
                    Dim priceFilterRange As XmlElement
                    Dim cnt As Integer
                    'priceFilterRange = oXform.addRange(oFromGroup, "PriceFilter", True, "Price Range", nMinPrice, nMaxPrice, nStep)
                    priceFilterRange = oXform.addSelect1(oFromGroup, "PriceFilter", False, "Price Filter", "")
                    For cnt = nMinPrice To nMaxPrice

                        Dim optionName As String = cnt.ToString() + "-" + (cnt + nStep).ToString()
                        oXform.addOption(priceFilterRange, optionName.ToString(), optionName.ToString(), False, "")
                        cnt = cnt + nStep
                    Next

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(cProcessInfo, "PriceFilter", ex, ""))
                End Try
            End Sub


            Public Function ApplyFilter(ByRef aWeb As Cms, ByRef cWhereSql As String, ByRef oXform As xForm, ByRef oFromGroup As XmlElement) As String
                Dim cProcessInfo As String = "ApplyFilter"
                Try

                    Dim priceRange() As String
                    Dim cDefinitionName As String = "Price"
                    Dim cSelectedPrice As String = String.Empty
                    If (oXform.Instance.SelectNodes("PriceFilter") IsNot Nothing) Then
                        cSelectedPrice = Convert.ToString(oXform.Instance.SelectSingleNode("PriceFilter").InnerText)

                    End If
                    If (cSelectedPrice <> String.Empty) Then
                        priceRange = cSelectedPrice.Split("-")

                        If (cWhereSql <> String.Empty) Then
                            cWhereSql = cWhereSql + " AND "
                        End If

                        cWhereSql = cWhereSql + " nContentKey in ( Select distinct ci.nContentId from tblContentIndex ci inner join tblContentIndexDef cid on cid.nContentIndexDefKey=ci.nContentIndexDefinitionKey "
                        cWhereSql = cWhereSql + " inner join tblAudit ca on ca.nAuditKey=cid.nAuditId and nStatus=1 and cid.cDefinitionName='" + cDefinitionName + "'"
                        cWhereSql = cWhereSql + " And ci.nNumberValue between " + Convert.ToString(priceRange(0)) + " and " + Convert.ToString(priceRange(1)) + ")"


                    End If

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(cProcessInfo, "PriceFilter", ex, ""))
                End Try
                Return cWhereSql
            End Function




        End Class

    End Namespace
End Namespace


