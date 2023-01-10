

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
                    Dim sSql As String = "spGetPriceRange"
                    Dim arrParams As New Hashtable
                    Dim sCotrolDisplayName As String = "Price Filter"
                    Dim oXml As XmlElement = oXform.moPageXML.CreateElement("PriceFilter")

                    If (aWeb.moRequest.Form("PriceFilter") IsNot Nothing) Then
                        oXml.InnerText = Convert.ToString(aWeb.moRequest.Form("PriceFilter"))

                    End If

                    oXform.Instance.AppendChild(oXml)

                    ' Adding a binding to the form bindings
                    oXform.addBind("PriceFilter", "PriceFilter", "false()", "string", oXform.model)


                    Dim nMinPrice As Double = Convert.ToDouble(FilterConfig.Attributes("fromPrice").Value)
                    Dim nMaxPrice As Double = Convert.ToDouble(FilterConfig.Attributes("toPrice").Value)
                    Dim nStep As Integer = Convert.ToDouble(FilterConfig.Attributes("step").Value)
                    Dim priceFilterRange As XmlElement
                    Dim cnt As Integer = 0

                    If (FilterConfig.Attributes("name") IsNot Nothing) Then
                        sCotrolDisplayName = Convert.ToString(FilterConfig.Attributes("name").Value)
                    End If

                    arrParams.Add("MinPrice", nMinPrice)
                    arrParams.Add("MaxPrice", nMaxPrice)
                    arrParams.Add("Step", nStep)
                    Using oDr As SqlDataReader = aWeb.moDbHelper.getDataReaderDisposable(sSql, CommandType.StoredProcedure, arrParams)
                        'Adding controls to the form like dropdown, radiobuttons
                        If (oXml.InnerText <> String.Empty) Then
                            priceFilterRange = oXform.addSelect(oFromGroup, "PriceFilter", False, sCotrolDisplayName, "checkbox filter-selected", ApperanceTypes.Full)
                        Else
                            priceFilterRange = oXform.addSelect(oFromGroup, "PriceFilter", False, sCotrolDisplayName, "checkbox", ApperanceTypes.Full)
                        End If

                        While oDr.Read
                            Dim name As String = aWeb.moCart.mcCurrencySymbol + Convert.ToString(oDr("minPrice")) + "-" + aWeb.moCart.mcCurrencySymbol + Convert.ToString(oDr("maxPrice")) + " <span class='ProductCount'>" + Convert.ToString(oDr("ProductCount")) + "</span>"
                            Dim value As String = Convert.ToString(oDr("minPrice")) + "-" + Convert.ToString(oDr("maxPrice"))

                            oXform.addOption(priceFilterRange, name, value, True)
                        End While
                    End Using
                    If (oFromGroup.SelectSingleNode("select[@ref='PriceFilter']") IsNot Nothing) Then
                        If (oXml.InnerText.Trim() <> String.Empty) Then
                            Dim sText As String

                            Dim aPrice() As String = oXml.InnerText.Split(",")
                            If (aPrice.Length <> 0) Then
                                For cnt = 0 To aPrice.Length - 1
                                    sText = oFromGroup.SelectSingleNode("select[@ref='PriceFilter']/item[value='" + aPrice(cnt) + "']").FirstChild().FirstChild().InnerText
                                    oXform.addSubmit(oFromGroup, sText, sText, "PriceFilter_" & aPrice(cnt), " filter-applied", "fa-times")
                                Next

                            Else

                                sText = oFromGroup.SelectSingleNode("select[@ref='PriceFilter']/item[value='" + oXml.InnerText + "']").FirstChild().FirstChild().InnerText
                                oXform.addSubmit(oFromGroup, sText, sText, "PriceFilter_" & aPrice(cnt), "filter-applied", "fa-times")
                            End If
                        End If
                    End If
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(cProcessInfo, "PriceFilter", ex, ""))
                End Try
            End Sub


            Public Function ApplyFilter(ByRef aWeb As Cms, ByRef cWhereSql As String, ByRef oXform As xForm, ByRef oFromGroup As XmlElement, ByRef FilterConfig As XmlElement) As String
                Dim cProcessInfo As String = "ApplyFilter"
                Dim cPriceCond As String = ""
                Try

                    Dim priceRange() As String
                    Dim cDefinitionName As String = "Price"
                    Dim cSelectedPrice As String = String.Empty
                    If (oXform.Instance.SelectNodes("PriceFilter") IsNot Nothing) Then
                        cSelectedPrice = Convert.ToString(oXform.Instance.SelectSingleNode("PriceFilter").InnerText)

                    End If

                    If (cSelectedPrice <> String.Empty) Then
                        If (cWhereSql <> String.Empty) Then
                            cWhereSql = cWhereSql + " AND "
                        End If

                        Dim cPriceLst As String() = cSelectedPrice.Split(New Char() {","c})
                        For Each cSchema As String In cPriceLst
                            priceRange = cSchema.Split("-")
                            If cPriceCond <> "" Then
                                cPriceCond = cPriceCond + " or ( ci.nNumberValue between  " + Convert.ToString(priceRange(0)) + " and " + Convert.ToString(priceRange(1)) + ")"
                            Else
                                cPriceCond = cPriceCond + " ( ci.nNumberValue between  " + Convert.ToString(priceRange(0)) + " and " + Convert.ToString(priceRange(1)) + ")"
                            End If

                        Next
                        cPriceCond = cPriceCond + ")"
                        cWhereSql = cWhereSql + " nContentKey in ( Select distinct ci.nContentId from tblContentIndex ci inner join tblContentIndexDef cid on cid.nContentIndexDefKey=ci.nContentIndexDefinitionKey "
                        cWhereSql = cWhereSql + " inner join tblAudit ca on ca.nAuditKey=cid.nAuditId and nStatus=1 where cid.cDefinitionName='" + cDefinitionName + "' AND ("
                        cWhereSql = cWhereSql + cPriceCond + ")"


                    End If

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(cProcessInfo, "PriceFilter", ex, ""))
                End Try
                Return cWhereSql
            End Function




        End Class

    End Namespace
End Namespace


