

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
            Public Sub AddControl(ByRef aWeb As Cms, ByRef FilterConfig As XmlElement, ByRef oXform As xForm, ByRef oFromGroup As XmlElement, ByRef oContentNode As XmlElement)
                Dim cProcessInfo As String = "AddControl"
                Try
                    Dim pageFilterSelect As XmlElement
                    'Dim pageFilterButtons As XmlElement
                    Dim sCotrolDisplayName As String = "Page Filter"
                    'Parent page id flag used to populate the root level pages or pages under current page.
                    Dim bParentPageId As Boolean = False
                    Dim nParentId As Integer = 1
                    Dim sSql As String = "spGetPagesByParentPageId"
                    Dim arrParams As New Hashtable
                    Dim oXml As XmlElement = oXform.moPageXML.CreateElement("PageFilter")
                    Dim oFilterElmt As XmlElement = Nothing
                    Dim className As String = String.Empty

                    If (aWeb.moRequest.Form("PageFilter") IsNot Nothing) Then
                        oXml.InnerText = Convert.ToString(aWeb.moRequest.Form("PageFilter"))

                    End If


                    Dim cWhereSql As String = String.Empty

                    For Each oFilterElmt In oContentNode.SelectNodes("Content[@type='Filter' and @providerName!='']")
                        className = oFilterElmt.Attributes("className").Value.ToString()

                        If aWeb.moRequest.Form(className) Is Nothing Then
                            If className = "PriceFilter" AndAlso aWeb.moRequest.Form("MaxPrice") <> "" AndAlso aWeb.moRequest.Form("MaxPrice") IsNot Nothing Then
                                Dim cSelectedMinPrice As String = ""
                                Dim cSelectedMaxPrice As String = ""
                                cSelectedMinPrice = Convert.ToString(aWeb.moRequest.Form("MinPrice"))
                                cSelectedMaxPrice = Convert.ToString(aWeb.moRequest.Form("MaxPrice"))
                                cWhereSql = cWhereSql & " and  nContentKey in ( Select distinct ci.nContentId from tblContentIndex ci inner join tblContentIndexDef cid on cid.nContentIndexDefKey=ci.nContentIndexDefinitionKey "
                                cWhereSql = cWhereSql & " inner join tblAudit ca on ca.nAuditKey=cid.nAuditId and nStatus=1 where cid.cDefinitionName='Price' AND ("
                                cWhereSql = cWhereSql & "ci.nNumberValue between " & cSelectedMinPrice & " and " & cSelectedMaxPrice & "))"
                            End If
                            If className = "AgeFilter" AndAlso aWeb.moRequest.Form("MaxAge") <> "" AndAlso aWeb.moRequest.Form("MaxAge") IsNot Nothing Then
                                Dim cDefMinName As String = "Age"
                                Dim cDefMaxName As String = "Max Age"
                                Dim nAgeMin As String = Convert.ToString(aWeb.moRequest.Form("MinAge"))
                                Dim nAgeMax As String = Convert.ToString(aWeb.moRequest.Form("MaxAge"))
                                Dim cAgeMinCond As String = "(ci.nNumberValue >= " & Convert.ToString(nAgeMin) & ")"
                                Dim cAgeMaxCond As String = "(ci.nNumberValue >= " & Convert.ToString(nAgeMin) & "  and ci.nNumberValue <= " + Convert.ToString(nAgeMax) & ") "
                                cWhereSql = cWhereSql & " and nContentKey in (Select  cr.nContentParentId from tblContentIndex ci  "
                                cWhereSql = cWhereSql & " inner join tblContentIndexDef cid on cid.nContentIndexDefKey=ci.nContentIndexDefinitionKey and cid.cDefinitionName in ('" & cDefMinName & "','" & cDefMaxName & "')"
                                cWhereSql = cWhereSql & " inner join tblContent cs on ci.nContentId=cs.nContentKey and cs.cContentSchemaName='SKU' inner join tblContentRelation cr on cr.nContentChildId=cs.nContentKey   inner join tblAudit acr on acr.nAuditKey=cr.nAuditId and acr.nStatus=1 "
                                cWhereSql = cWhereSql & "  where ci.nNumberValue!=0    And  " & cAgeMinCond & " union "
                                cWhereSql = cWhereSql & "  Select  cr.nContentParentId from tblContentIndex ci  "
                                cWhereSql = cWhereSql & " inner join tblContentIndexDef cid on cid.nContentIndexDefKey=ci.nContentIndexDefinitionKey and cid.cDefinitionName in ('" & cDefMinName & "','" & cDefMaxName & "')"
                                cWhereSql = cWhereSql & " inner join tblContent cs on ci.nContentId=cs.nContentKey and cs.cContentSchemaName='SKU' inner join tblContentRelation cr on cr.nContentChildId=cs.nContentKey  inner join tblAudit acr on acr.nAuditKey=cr.nAuditId and acr.nStatus=1 "
                                cWhereSql = cWhereSql & "  where ci.nNumberValue!=0    And " & cAgeMaxCond & " )"
                            End If

                            If className = "WeightFilter" AndAlso aWeb.moRequest.Form("To") <> "" AndAlso aWeb.moRequest.Form("To") IsNot Nothing Then
                                Dim cDefinitionName As String = "weight"
                                cWhereSql = cWhereSql & " and  nContentKey in ( Select distinct ci.nContentId from tblContentIndex ci inner join tblContentIndexDef cid on cid.nContentIndexDefKey=ci.nContentIndexDefinitionKey "
                                cWhereSql = cWhereSql & " inner join tblAudit ca on ca.nAuditKey=cid.nAuditId and nStatus=1 and cid.cDefinitionName='" & cDefinitionName & "'"
                                cWhereSql = cWhereSql & " And ci.nNumberValue between " + Convert.ToString(aWeb.moRequest.Form("From")) & " and " + Convert.ToString(aWeb.moRequest.Form("To")) & ")"
                            End If
                        Else

                            If className = "GroupSizeFilter" Then
                                Dim cSelectedGroupSize As String = Convert.ToString(aWeb.moRequest.Form(className))
                                cWhereSql = cWhereSql & "and  nContentKey in ( Select distinct ci.nContentId from tblContentIndex ci inner join tblContentIndexDef cid on cid.nContentIndexDefKey=ci.nContentIndexDefinitionKey "
                                cWhereSql = cWhereSql & " inner join tblAudit ca on ca.nAuditKey=cid.nAuditId and nStatus=1 and cid.cDefinitionName='GroupSize'"
                                cWhereSql = cWhereSql & " And isNull(ci.nNumberValue,1) in (" & cSelectedGroupSize & "))"
                            End If
                            If className = "OccasionFilter" Then
                                cWhereSql = cWhereSql & " and  nContentId in (select nContentId from tblCartCatProductRelations c inner join tblAudit a on a.nAuditKey=c.nAuditId and nStatus=1"
                                cWhereSql = cWhereSql & " where c.nCatId in (" + aWeb.moRequest.Form(className) & ")) "
                            End If

                            If className = "OfferFilter" Then
                                cWhereSql = cWhereSql & " and  c.cContentSchemaName = 'Product' "
                                cWhereSql = cWhereSql & " and nContentKey in ("
                                cWhereSql = cWhereSql & " select distinct cr.nContentParentId from tblContent cn inner join tblContentRelation cr on cr.nContentParentId = cn.nContentKey and cn.cContentSchemaName = 'Product'"
                                cWhereSql = cWhereSql & " inner join tblAudit ac on ac.nAuditKey = cn.nAuditId and ac.nStatus = 1"
                                cWhereSql = cWhereSql & " inner join tblAudit ca on ca.nAuditKey = cr.nAuditId and ca.nStatus = 1"
                                cWhereSql = cWhereSql & " where cr.nContentParentId in "
                                cWhereSql = cWhereSql & " (select nContentId from tblCartCatProductRelations c inner join tblAudit a on a.nAuditKey=c.nAuditId and nStatus=1 where c.nCatId in (" + aWeb.moRequest.Form(className) & ")) "
                                cWhereSql = cWhereSql & " union "
                                cWhereSql = cWhereSql & " select  distinct cr.nContentParentId from tblContent cn inner join tblContentRelation cr on cr.nContentChildId = cn.nContentKey  and cn.cContentSchemaName = 'SKU' "
                                cWhereSql = cWhereSql & " inner join tblAudit sa on sa.nAuditKey = cn.nAuditId and sa.nStatus = 1 "
                                cWhereSql = cWhereSql & " inner join tblAudit sca on sca.nAuditKey = cr.nAuditId and sca.nStatus = 1 "
                                cWhereSql = cWhereSql & " where cr.nContentChildId in "
                                cWhereSql = cWhereSql & " (select nContentId from tblCartCatProductRelations c inner join tblAudit a on a.nAuditKey=c.nAuditId and nStatus=1 where c.nCatId in (" + aWeb.moRequest.Form(className) & ")) "
                                cWhereSql = cWhereSql & " )"
                            End If

                            'If className = "LocationFilter" AndAlso aWeb.moRequest.Form("Location") <> "" AndAlso aWeb.moRequest.Form("Location") IsNot Nothing Then
                            '    Dim cSelectedLocation As String = String.Empty
                            '    Dim cSelectedDistance As String = String.Empty
                            '    Dim Latitude As String = ""
                            '    Dim Longitude As String = ""
                            '    cSelectedLocation = aWeb.moRequest.Form("Location")
                            '    cSelectedDistance = aWeb.moRequest.Form("Distance")

                            '    'If cSelectedLocation <> "" Then

                            '    '    Dim commonSvc = New commans
                            '    '    Dim offerDistance = New OfferDistance()

                            '    '    If cSelectedLocation.Contains(",") Then
                            '    '        cSelectedLocation = cSelectedLocation.Replace(",", "")
                            '    '    End If

                            '    '    offerDistance = commonSvc.GetPostcodeDetails(cSelectedLocation.Trim())

                            '    '    If offerDistance.Location IsNot Nothing Then
                            '    '        Latitude = offerDistance.Latitude
                            '    '        Longitude = offerDistance.Longitude
                            '    '    End If
                            '    'End If

                            '    cWhereSql = cWhereSql & " and  nContentKey in (  select ncontentkey from tblContent tc left  join dbo.cfn_GetAllProductsDistance('" & Latitude & "','" & Longitude & "',0) pd on pd.nProductContentKey = tc.nContentKey "
                            '    cWhereSql = cWhereSql & " where tc.cContentSchemaName = 'Product' and    (pd.distance <=" & cSelectedDistance & ")) "
                            'End If
                        End If
                    Next
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
                        arrParams.Add("whereSql", cWhereSql)
                    End If


                    Using oDr As SqlDataReader = aWeb.moDbHelper.getDataReaderDisposable(sSql, CommandType.StoredProcedure, arrParams)  'Done by nita on 6/7/22
                        'Adding controls to the form like dropdown, radiobuttons

                        If (oXml.InnerText <> String.Empty) Then

                            pageFilterSelect = oXform.addSelect(oFromGroup, "PageFilter", False, sCotrolDisplayName, "checkbox SubmitPageFilter filter-selected", ApperanceTypes.Full)
                        Else
                            pageFilterSelect = oXform.addSelect(oFromGroup, "PageFilter", False, sCotrolDisplayName, "checkbox SubmitPageFilter", ApperanceTypes.Full)
                        End If

                        'oXform.addOptionsFromSqlDataReader(pageFilterSelect, oDr, "name", "nStructKey")
                        While oDr.Read
                            Dim name As String = Convert.ToString(oDr("cStructName")) + " <span class='ProductCount'>" + Convert.ToString(oDr("ProductCount")) + "</span>"
                            Dim value As String = Convert.ToString(oDr("nStructKey"))

                            oXform.addOption(pageFilterSelect, name, value, True)

                        End While

                    End Using
                    If (oFromGroup.SelectSingleNode("select[@ref='PageFilter']") IsNot Nothing) Then
                        If (oXml.InnerText.Trim() <> String.Empty) Then
                            Dim sText As String
                            'Dim sValue As String
                            Dim cnt As Integer
                            Dim aPages() As String = oXml.InnerText.Split(",")
                            If (aPages.Length <> 0) Then
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

            Public Function ApplyFilter(ByRef aWeb As Cms, ByRef cWhereSql As String, ByRef oXform As xForm, ByRef oFromGroup As XmlElement, ByRef FilterConfig As XmlElement) As String
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
                            cWhereSql = " nStructId IN (select nStructKey from tblContentStructure where nStructParId in (" & cPageIds & "))"
                        End If
                    End If
                    Return cWhereSql
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(cProcessInfo, "PageFilter", ex, ""))
                    Return Nothing
                End Try

            End Function

            'Public Sub RemovePageFromFilter(ByRef aWeb As Cms, ByVal cPageId As String)
            '    Dim cProcessInfo As String = "RemovePageFromFilter"
            '    Try
            '        Dim cnt As Integer
            '        Dim cntPages As Integer = 0
            '        Dim cPageIds As String = String.Empty
            '        If (aWeb.moSession("PageFilter") IsNot Nothing) Then
            '            cPageIds = aWeb.moSession("PageFilter")
            '            cPageIds = cPageIds.Replace(cPageId, "")

            '            Dim aPageId() As String = cPageIds.Split(",")
            '            For cnt = 0 To aPageId.Length - 1 Step 1
            '                If (aPageId(cnt) <> String.Empty) Then
            '                    If aPageId(cnt) <> "" Then
            '                        cPageIds = cPageIds + aPageId(cnt) + ","
            '                    End If
            '                End If
            '            Next
            '            aWeb.moSession("PageFilter") = Left(cPageIds, cPageIds.Length - 1)
            '        End If

            '    Catch ex As Exception
            '        RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(cProcessInfo, "PageFilter", ex, ""))
            '    End Try
            'End Sub

        End Class
        ' End Class
    End Namespace
End Namespace


