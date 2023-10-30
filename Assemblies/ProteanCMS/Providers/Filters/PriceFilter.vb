

Imports System.Data.SqlClient
Imports System.Web
Imports System.Web.UI.WebControls
Imports System.Xml
Imports Lucene.Net.Index.SegmentReader
Imports Lucene.Net.Search
Imports Microsoft
Imports Microsoft.Ajax.Utilities
Imports Microsoft.ClearScript.Util
Imports Protean.Cms
Imports Protean.xForm



Namespace Providers

    Namespace Filters

        Public Class PriceFilter

            Public Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
            Public Sub AddControl(ByRef aWeb As Cms, ByRef FilterConfig As XmlElement, ByRef oXform As xForm, ByRef oFromGroup As XmlElement, ByRef oContentNode As XmlElement, ByVal cWhereSql As String)
                Dim cProcessInfo As String = "AddControl"
                Try
                    Dim sSql As String = "spGetPriceRange"
                    Dim arrParams As New Hashtable
                    Dim sCotrolDisplayName As String = "Price Filter"
                    Dim cFilterTarget As String = String.Empty

                    Dim oXml As XmlElement = oXform.moPageXML.CreateElement("PriceFilter")
                    Dim oMinPrice As XmlAttribute = oXform.moPageXML.CreateAttribute("MinPrice")
                    Dim oMaxPrice As XmlAttribute = oXform.moPageXML.CreateAttribute("MaxPrice")
                    Dim oSliderMinPrice As XmlAttribute = oXform.moPageXML.CreateAttribute("SliderMinPrice")
                    Dim oSliderMaxPrice As XmlAttribute = oXform.moPageXML.CreateAttribute("SliderMaxPrice")
                    Dim oStep As XmlAttribute = oXform.moPageXML.CreateAttribute("PriceStep")
                    Dim oProductCountList As XmlAttribute = oXform.moPageXML.CreateAttribute("PriceCountList")
                    Dim oProductTotalCount As XmlAttribute = oXform.moPageXML.CreateAttribute("PriceTotalCount")
                    Dim sProductCount As String = String.Empty
                    Dim cnt As Integer = 0
                    Dim cProductCountList As String = String.Empty
                    Dim nPageId As Integer = aWeb.mnPageId
                    Dim nMaxPRiceProduct As Integer = 0
                    Dim nMinPriceProduct As Integer = 0
                    Dim oFilterElmt As XmlElement = Nothing
                    Dim className As String = String.Empty
                    Dim cWhereQuery As String = String.Empty

                    If aWeb.moRequest.Form("MaxPrice") IsNot Nothing Then

                        oMinPrice.Value = Convert.ToString(aWeb.moRequest.Form("MinPrice"))
                        oMaxPrice.Value = Convert.ToString(aWeb.moRequest.Form("MaxPrice"))

                    End If
                    If (oContentNode.Attributes("filterTarget") IsNot Nothing) Then
                        cFilterTarget = oContentNode.Attributes("filterTarget").Value
                    End If


                    If (FilterConfig.Attributes("name") IsNot Nothing) Then
                        sCotrolDisplayName = Convert.ToString(FilterConfig.Attributes("name").Value)
                    End If

                    arrParams.Add("MinPrice", FilterConfig.GetAttribute("fromPrice"))
                    arrParams.Add("MaxPrice", FilterConfig.GetAttribute("toPrice"))
                    arrParams.Add("Step", FilterConfig.GetAttribute("step"))
                    arrParams.Add("PageId", nPageId)
                    arrParams.Add("whereSql", cWhereSql)
                    arrParams.Add("FilterTarget", cFilterTarget)
                    Using oDr As SqlDataReader = aWeb.moDbHelper.getDataReaderDisposable(sSql, CommandType.StoredProcedure, arrParams)
                        If (oDr IsNot Nothing) Then
                            While oDr.Read
                                cnt = cnt + 1
                                If cnt = 1 Then
                                    nMinPriceProduct = oDr("MinProductPrice")
                                End If
                                nMaxPRiceProduct = oDr("MaxProductPrice")
                                sProductCount = Convert.ToString(oDr("ContentCount"))
                                cProductCountList = cProductCountList + cnt.ToString() + ":" + sProductCount + ","
                            End While

                        End If
                        oSliderMinPrice.Value = FilterConfig.GetAttribute("fromPrice")
                        'oSliderMaxPrice.Value = FilterConfig.GetAttribute("toPrice")
                        'oProductTotalCount.Value = nMaxPRiceProduct
                        'oSliderMinPrice.Value = nMinPriceProduct

                        oSliderMaxPrice.Value = nMaxPRiceProduct
                        'oMaxPrice.Value = FilterConfig.GetAttribute("toPrice")


                        oStep.Value = FilterConfig.GetAttribute("step")
                        oXml.Attributes.Append(oMinPrice)
                        oXml.Attributes.Append(oMaxPrice)
                        oXml.Attributes.Append(oSliderMinPrice)
                        oXml.Attributes.Append(oSliderMaxPrice)
                        oXml.Attributes.Append(oStep)
                        oXml.Attributes.Append(oProductTotalCount)


                        '    'Adding controls to the form like dropdown, radiobuttons
                        '    'If (oXml.InnerText <> String.Empty) Then
                        '    '    priceFilterRange = oXform.addSelect(oFromGroup, "PriceFilter", False, sCotrolDisplayName, "checkbox filter-selected", ApperanceTypes.Full)
                        '    'Else
                        '    '    priceFilterRange = oXform.addSelect(oFromGroup, "PriceFilter", False, sCotrolDisplayName, "checkbox", ApperanceTypes.Full)
                        '    'End If

                        '    While oDr.Read
                        '        Dim name As String = aWeb.moCart.mcCurrencySymbol + Convert.ToString(oDr("minPrice")) + "-" + aWeb.moCart.mcCurrencySymbol + Convert.ToString(oDr("maxPrice")) + " <span class='ProductCount'>" + Convert.ToString(oDr("ProductCount")) + "</span>"
                        '        Dim value As String = Convert.ToString(oDr("minPrice")) + "-" + Convert.ToString(oDr("maxPrice"))

                        '         oXform.add(priceFilterRange, name, value, True)
                        '    End While
                    End Using


                    oProductCountList.Value = cProductCountList
                    oXml.Attributes.Append(oProductCountList)

                    oXform.Instance.AppendChild(oXml)


                    oXform.addBind("MinPrice", "PriceFilter/@MinPrice", "false()", "string", oXform.model)
                    oXform.addBind("MaxPrice", "PriceFilter/@MaxPrice", "false()", "string", oXform.model)
                    oXform.addBind("SliderMinPrice", "PriceFilter/@SliderMinPrice", "false()", "string", oXform.model)
                    oXform.addBind("SliderMaxPrice", "PriceFilter/@SliderMaxPrice", "false()", "string", oXform.model)
                    oXform.addBind("PriceStep", "PriceFilter/@PriceStep", "false()", "string", oXform.model)
                    oXform.addBind("PriceListCount", "PriceFilter/@PriceCountList", "false()", "string", oXform.model)
                    oXform.addBind("PriceFilter", "PriceFilter/@MaxPrice", "false()", "string", oXform.model)
                    ' oXform.addBind("PriceTotalCount", "PriceFilter/@PriceTotalCount", "false()", "string", oXform.model)

                    oXform.addInput(oFromGroup, "MinPrice", True, "", "hidden")
                    oXform.addInput(oFromGroup, "MaxPrice", True, "", "hidden")
                    oXform.addInput(oFromGroup, "SliderMinPrice", True, "", "hidden")
                    oXform.addInput(oFromGroup, "SliderMaxPrice", True, "", "hidden")
                    oXform.addInput(oFromGroup, "PriceStep", True, "", "hidden")
                    oXform.addInput(oFromGroup, "PriceListCount", True, "", "hidden")
                    oXform.addInput(oFromGroup, "PriceFilter", True, "", "hidden")
                    oXform.addSubmit(oFromGroup, "", "Apply", "PriceFilter", "  btnPriceSubmit hidden", "")
                    ' oXform.addInput(oFromGroup, "PriceTotalCount", True, "", "hidden")

                    'If (oFromGroup.SelectSingleNode("select[@ref='PriceFilter']") IsNot Nothing) Then
                    '    If (oXml.InnerText.Trim() <> String.Empty) Then
                    '        Dim sText As String

                    '        Dim aPrice() As String = oXml.InnerText.Split(",")
                    '        If (aPrice.Length <> 0) Then
                    '            For cnt = 0 To aPrice.Length - 1
                    '                sText = oFromGroup.SelectSingleNode("select[@ref='PriceFilter']/item[value='" + aPrice(cnt) + "']").FirstChild().FirstChild().InnerText
                    '                oXform.addSubmit(oFromGroup, sText, sText, "PriceFilter_" & aPrice(cnt), " filter-applied", "fa-times")
                    '            Next

                    '        Else

                    '            sText = oFromGroup.SelectSingleNode("select[@ref='PriceFilter']/item[value='" + oXml.InnerText + "']").FirstChild().FirstChild().InnerText
                    '            oXform.addSubmit(oFromGroup, sText, sText, "PriceFilter_" & aPrice(cnt), "filter-applied", "fa-times")
                    '        End If
                    '    End If
                    'End If

                    If (aWeb.moRequest.Form("MinPrice") IsNot Nothing And aWeb.moRequest.Form("MinPrice") <> "") Then

                        '  Dim sText As String = "From " + aWeb.moCart.mcCurrencySymbol + "" + oMinPrice.Value.Trim() + " to " + aWeb.moCart.mcCurrencySymbol + "" + oMaxPrice.Value.Trim()
                        Dim sText As String = "From " + oMinPrice.Value.Trim() + " to " + oMaxPrice.Value.Trim()
                        oXform.addSubmit(oFromGroup, sText, sText, "PriceFilter" + sText, "btnCrossForPrice filter-applied", "fa-times")

                    End If

                    If (aWeb.moRequest.Form("MinPrice") IsNot Nothing And aWeb.moRequest.Form("MinPrice") <> "") Then
                        oXform.addInput(oFromGroup, "", False, sCotrolDisplayName, "histogramSliderMainDivPrice filter-selected")
                    Else
                        oXform.addInput(oFromGroup, "", False, sCotrolDisplayName, "histogramSliderMainDivPrice")
                    End If

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(cProcessInfo, "PriceFilter", ex, ""))
                End Try
            End Sub


            Public Function ApplyFilter(ByRef aWeb As Cms, ByRef cWhereSql As String, ByRef oXform As xForm, ByRef oFromGroup As XmlElement, ByRef FilterConfig As XmlElement, ByRef cFilterTarget As String) As String
                Dim cProcessInfo As String = "ApplyFilter"
                Dim cPriceCond As String = ""
                Try
                    'Dim priceRange() As String
                    Dim cDefinitionName As String = "Price"
                    Dim cSelectedMinPrice As String = String.Empty
                    Dim cSelectedMaxPrice As String = String.Empty
                    Dim cPageIds As String = String.Empty
                    'cSelectedMinPrice = Convert.ToString(oXform.Instance.SelectSingleNode("PriceFilter/@MinPrice").InnerText)
                    'cSelectedMaxPrice = Convert.ToString(oXform.Instance.SelectSingleNode("PriceFilter/@MaxPrice").InnerText)
                    cSelectedMinPrice = Convert.ToString(aWeb.moRequest.Form("MinPrice")).Replace(aWeb.moCart.mcCurrencySymbol, "")
                    cSelectedMaxPrice = Convert.ToString(aWeb.moRequest.Form("MaxPrice")).Replace(aWeb.moCart.mcCurrencySymbol, "")
                    Dim bParentPageId As Boolean = False


                    'If (oXform.Instance.SelectSingleNode("PageFilter") IsNot Nothing) Then
                    '    cPageIds = oXform.Instance.SelectSingleNode("PageFilter").InnerText

                    'End If

                    If (cSelectedMaxPrice <> String.Empty) Then

                        If (cSelectedMinPrice = String.Empty) Then
                            cSelectedMinPrice = "0"
                        End If
                        ' cPriceCond = " ci.nNumberValue between " + cSelectedMinPrice + " and " + cSelectedMaxPrice
                        If (cWhereSql <> String.Empty) Then
                            cWhereSql = cWhereSql + " AND "
                        End If

                        'ElseIf (cPageIds = String.Empty) Then

                        '    cPageIds = aWeb.moPageXml.SelectSingleNode("Page/@id").Value.ToString()
                        '    cWhereSql = " nStructId IN (select nStructKey from tblContentStructure where nStructParId in (" & cPageIds & ")) AND "
                        'End If
                        cWhereSql = cWhereSql + GetFilterSQL(aWeb)




                        End If

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(cProcessInfo, "PriceFilter", ex, ""))
                End Try
                Return cWhereSql
            End Function


            Public Function GetFilterSQL(ByRef aWeb As Cms) As String
                Dim cWhereSql As String = String.Empty
                Dim cProcessInfo As String = "GetFilterSQL"
                Dim cIndexDefinationName As String = "Price"
                Try
                    Dim cSelectedMinPrice As String = ""
                    Dim cSelectedMaxPrice As String = ""
                    cSelectedMinPrice = Convert.ToString(aWeb.moRequest.Form("MinPrice")).Replace(aWeb.moCart.mcCurrency, "")
                    cSelectedMaxPrice = Convert.ToString(aWeb.moRequest.Form("MaxPrice")).Replace(aWeb.moCart.mcCurrency, "")
                    If cSelectedMaxPrice <> String.Empty Then
                        If (cSelectedMinPrice = String.Empty) Then
                            cSelectedMinPrice = "0"
                        End If
                        cWhereSql = cWhereSql & " nContentKey in ( Select distinct ci.nContentId from tblContentIndex ci inner join tblContentIndexDef cid on cid.nContentIndexDefKey=ci.nContentIndexDefinitionKey "
                        cWhereSql = cWhereSql & " inner join tblAudit ca on ca.nAuditKey=cid.nAuditId and nStatus=1 where cid.cDefinitionName='" + cIndexDefinationName + "' AND ("
                        cWhereSql = cWhereSql & "ci.nNumberValue between " & cSelectedMinPrice & " and " & cSelectedMaxPrice & "))"
                    End If
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(cProcessInfo, "PriceFilter", ex, ""))
                End Try
                Return cWhereSql
            End Function

        End Class

    End Namespace
End Namespace


