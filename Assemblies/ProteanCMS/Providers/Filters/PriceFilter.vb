

Imports System.Data.SqlClient
Imports System.Web.UI.WebControls
Imports System.Xml
Imports Lucene.Net.Index.SegmentReader
Imports Lucene.Net.Search
Imports Microsoft
Imports Microsoft.ClearScript.Util
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
                    Dim oMinPrice As XmlAttribute = oXform.moPageXML.CreateAttribute("MinPrice")
                    Dim oMaxPrice As XmlAttribute = oXform.moPageXML.CreateAttribute("MaxPrice")
                    Dim oSliderMinPrice As XmlAttribute = oXform.moPageXML.CreateAttribute("SliderMinPrice")
                    Dim oSliderMaxPrice As XmlAttribute = oXform.moPageXML.CreateAttribute("SliderMaxPrice")
                    Dim oStep As XmlAttribute = oXform.moPageXML.CreateAttribute("PriceStep")
                    Dim oProductCountList As XmlAttribute = oXform.moPageXML.CreateAttribute("PriceCountList")
                    Dim sProductCount As String = String.Empty
                    Dim cnt As Integer = 0
                    Dim cProductCountList As String = String.Empty
                    Dim nPageId As Integer = aWeb.mnPageId
                    Dim nMaxPRiceProduct As Integer = 0
                    If aWeb.moRequest.Form("MinPrice") IsNot Nothing Then

                        oMinPrice.Value = Convert.ToString(aWeb.moRequest.Form("MinPrice"))
                        oMaxPrice.Value = Convert.ToString(aWeb.moRequest.Form("MaxPrice"))

                    End If


                    If (FilterConfig.Attributes("name") IsNot Nothing) Then
                        sCotrolDisplayName = Convert.ToString(FilterConfig.Attributes("name").Value)
                    End If
                    arrParams.Add("MinPrice", FilterConfig.GetAttribute("fromPrice"))
                    arrParams.Add("MaxPrice", FilterConfig.GetAttribute("toPrice"))
                    arrParams.Add("Step", FilterConfig.GetAttribute("step"))
                    arrParams.Add("PageId", nPageId)

                    Using oDr As SqlDataReader = aWeb.moDbHelper.getDataReaderDisposable(sSql, CommandType.StoredProcedure, arrParams)
                        If (oDr.HasRows) Then
                            While oDr.Read
                                cnt = cnt + 1
                                nMaxPRiceProduct = oDr("MaxProductPrice")
                                sProductCount = Convert.ToString(oDr("ProductCount"))
                                cProductCountList = cProductCountList + cnt.ToString() + ":" + sProductCount + ","
                            End While

                        End If
                        oSliderMinPrice.Value = FilterConfig.GetAttribute("fromPrice")
                        oSliderMaxPrice.Value = nMaxPRiceProduct



                        oStep.Value = FilterConfig.GetAttribute("step")
                        oXml.Attributes.Append(oMinPrice)
                        oXml.Attributes.Append(oMaxPrice)
                        oXml.Attributes.Append(oSliderMinPrice)
                        oXml.Attributes.Append(oSliderMaxPrice)
                        oXml.Attributes.Append(oStep)


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

                    oXform.addInput(oFromGroup, "MinPrice", True, "", "hidden")
                    oXform.addInput(oFromGroup, "MaxPrice", True, "", "hidden")
                    oXform.addInput(oFromGroup, "SliderMinPrice", True, "", "hidden")
                    oXform.addInput(oFromGroup, "SliderMaxPrice", True, "", "hidden")
                    oXform.addInput(oFromGroup, "PriceStep", True, "", "hidden")
                    oXform.addInput(oFromGroup, "PriceListCount", True, "", "hidden")

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

                        Dim sText As String = "From £" + oMinPrice.Value.Trim() + " To £" + oMaxPrice.Value.Trim()
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


            Public Function ApplyFilter(ByRef aWeb As Cms, ByRef cWhereSql As String, ByRef oXform As xForm, ByRef oFromGroup As XmlElement, ByRef FilterConfig As XmlElement) As String
                Dim cProcessInfo As String = "ApplyFilter"
                Dim cPriceCond As String = ""
                Try
                    'Dim priceRange() As String
                    Dim cDefinitionName As String = "Price"
                    Dim cSelectedMinPrice As String = String.Empty
                    Dim cSelectedMaxPrice As String = String.Empty
                    Dim cPageIds As String = String.Empty
                    cSelectedMinPrice = Convert.ToString(oXform.Instance.SelectSingleNode("PriceFilter/@MinPrice").InnerText)
                    cSelectedMaxPrice = Convert.ToString(oXform.Instance.SelectSingleNode("PriceFilter/@MaxPrice").InnerText)
                    Dim bParentPageId As Boolean = False


                    If (oXform.Instance.SelectSingleNode("PageFilter") IsNot Nothing) Then
                        cPageIds = oXform.Instance.SelectSingleNode("PageFilter").InnerText

                    End If
                    If (cSelectedMinPrice <> String.Empty) Then
                        cPriceCond = " ci.nNumberValue between " + cSelectedMinPrice + " and " + cSelectedMaxPrice
                        If (cWhereSql <> String.Empty) Then
                            cWhereSql = cWhereSql + " AND "

                        ElseIf (cPageIds = String.Empty) Then

                        cPageIds = aWeb.moPageXml.SelectSingleNode("Page/@id").Value.ToString()
                            cWhereSql = " nStructId IN (select nStructKey from tblContentStructure where nStructParId in (" & cPageIds & ")) AND "
                        End If


                        cWhereSql = cWhereSql + " nContentKey in ( Select distinct ci.nContentId from tblContentIndex ci inner join tblContentIndexDef cid on cid.nContentIndexDefKey=ci.nContentIndexDefinitionKey "
                            cWhereSql = cWhereSql + " inner join tblAudit ca on ca.nAuditKey=cid.nAuditId and nStatus=1 where cid.cDefinitionName='" + cDefinitionName + "' AND ("
                            cWhereSql = cWhereSql + cPriceCond + "))"


                    End If

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(cProcessInfo, "PriceFilter", ex, ""))
                End Try
                Return cWhereSql
            End Function




        End Class

    End Namespace
End Namespace


