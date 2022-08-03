

Imports System.Data.SqlClient
Imports System.Xml
Imports Protean.Cms
Imports Protean.xForm

Namespace Providers

    Namespace Filters

        Public Class PriceFilter


            Public Sub AddControl(ByRef aWeb As Cms, ByRef FilterConfig As XmlElement, ByRef oXform As xForm, ByRef oFromGroup As XmlElement)
                Try
                    Dim nMinPrice As Double = 0
                    Dim nMaxPrice As Double = 0
                    Dim nStep As Integer = 0
                    Dim priceFilterRange As XmlElement
                    priceFilterRange = oXform.addRange(oFromGroup, "PriceFilter", True, "Price Range", nMinPrice, nMaxPrice, nStep)

                Catch ex As Exception
                End Try
            End Sub


            Public Sub ApplyFilter(ByRef aWeb As Cms, ByRef nSelectedPrice As Double, ByRef oXform As xForm, ByRef oFromGroup As XmlElement)
                Try


                    Dim cWhereSql As String = String.Empty
                    Dim nMinPrice As Double
                    Dim cDefinitionName As String = "Price"

                    If (oXform.Instance.SelectNodes("PriceFilter") IsNot Nothing) Then
                        nMinPrice = Convert.ToDouble(oXform.Instance.SelectSingleNode("PriceFilter").Attributes("FromPrice").InnerText)

                        If (aWeb.moSession("nMinPrice") Is Nothing) Then
                            aWeb.moSession("nMinPrice") = nMinPrice
                        Else
                            aWeb.moSession("nMinPrice") = nMinPrice
                            nMinPrice = aWeb.moSession("nMinPrice")
                        End If
                        If (aWeb.moSession("nSelectedPrice") Is Nothing) Then
                            aWeb.moSession("nSelectedPrice") = nSelectedPrice
                        Else
                            aWeb.moSession("nSelectedPrice") = nSelectedPrice
                            nSelectedPrice = aWeb.moSession("nSelectedPrice")
                        End If
                    End If

                    If (nMinPrice <> 0 And nSelectedPrice <> 0) Then

                        If (cWhereSql = String.Empty) Then
                            cWhereSql = cWhereSql + " ( Select distinct ci.nContentId from tblContentIndex ci inner join tblContentIndexDef cid on cid.nContentIndexDefKey=ci.nContentIndexDefinitionKey "
                            cWhereSql = cWhereSql + " inner join tblAudit ca on ca.nAuditKey=cid.nAuditId and nStatus=1 and cid.cDefinitionName='" + cDefinitionName + "'"
                            cWhereSql = cWhereSql + " And ci.nNumberValue between " + Convert.ToString(nMinPrice) + " and " + Convert.ToString(nSelectedPrice)
                        End If
                        'call sp and return xml data
                        If (cWhereSql <> String.Empty) Then
                            cWhereSql = cWhereSql.Substring(0, cWhereSql.Length - 1)
                            cWhereSql = " nContentKey In (" + cWhereSql + ")"
                            aWeb.GetPageContentFromSelect(cWhereSql,,,,,,,,,,, "Product")
                        End If

                    End If

                Catch ex As Exception

                End Try
            End Sub




        End Class

    End Namespace
End Namespace


