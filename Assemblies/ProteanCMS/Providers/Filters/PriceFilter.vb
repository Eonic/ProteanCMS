

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
                    priceFilterRange = oXform.addRange(oFromGroup, "PriceFilter", True, "Price Range", nMinPrice, nMaxPrice, nStep)

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(cProcessInfo, "PriceFilter", ex, ""))
                End Try
            End Sub


            Public Function ApplyFilter(ByRef aWeb As Cms, ByRef cWhereSql As String, ByRef oXform As xForm, ByRef oFromGroup As XmlElement) As String
                Dim cProcessInfo As String = "ApplyFilter"
                Try
                    Dim nMinPrice As Double
                    Dim cDefinitionName As String = "Price"
                    Dim nSelectedPrice As Double = 0
                    If (oXform.Instance.SelectNodes("PriceFilter") IsNot Nothing) Then
                        nMinPrice = Convert.ToDouble(oXform.Instance.SelectSingleNode("PriceFilter").Attributes("FromPrice").InnerText)

                    End If

                    If (nMinPrice <> 0 And nSelectedPrice <> 0) Then

                        If (cWhereSql = String.Empty) Then
                            cWhereSql = cWhereSql + " ( Select distinct ci.nContentId from tblContentIndex ci inner join tblContentIndexDef cid on cid.nContentIndexDefKey=ci.nContentIndexDefinitionKey "
                            cWhereSql = cWhereSql + " inner join tblAudit ca on ca.nAuditKey=cid.nAuditId and nStatus=1 and cid.cDefinitionName='" + cDefinitionName + "'"
                            cWhereSql = cWhereSql + " And ci.nNumberValue between " + Convert.ToString(nMinPrice) + " and " + Convert.ToString(nSelectedPrice)
                        End If


                    End If
                    Return cWhereSql
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(cProcessInfo, "PriceFilter", ex, ""))
                End Try
            End Function




        End Class

    End Namespace
End Namespace


