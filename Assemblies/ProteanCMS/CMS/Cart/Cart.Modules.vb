Option Strict Off
Option Explicit On
Imports System.Xml
Imports System.Collections
Imports System.Web.Configuration
Imports System.Data.SqlClient
Imports System.Web.HttpUtility
Imports VB = Microsoft.VisualBasic
Imports System.IO
Imports Protean.Tools.Xml
Imports Protean.Tools.Xml.XmlNodeState
Imports System

Partial Public Class Cms

    Partial Public Class Cart

#Region "Module Behaviour"

        Public Class Modules

            Public Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
            Private Const mcModuleName As String = "Protean.Cms.Membership.Modules"

            Public Sub New()

                'do nowt

            End Sub

            Public Sub ListOrders(ByRef myWeb As Protean.Cms, ByRef oContentNode As XmlElement)
                Try
                    If myWeb.mnUserId > 0 Then
                        myWeb.moDbHelper.mnUserId = myWeb.mnUserId
                        myWeb.moDbHelper.ListUserOrders(oContentNode, "Order")

                        If myWeb.moRequest("OrderId") <> "" Then
                            Dim oCart As Cart
                            oCart = New Cart(myWeb)
                            oCart.ListOrders(CInt("0" & myWeb.moRequest("OrderId")))
                            oCart = Nothing
                        End If

                    End If

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ListOrders", ex, ""))
                End Try
            End Sub


            Public Sub ListQuotes(ByRef myWeb As Protean.Cms, ByRef oContentNode As XmlElement)
                Try

                    If myWeb.mnUserId > 0 Then
                        myWeb.moDbHelper.mnUserId = myWeb.mnUserId
                        myWeb.moDbHelper.ListUserOrders(oContentNode, "Quote")

                        If myWeb.moRequest("QuoteId") <> "" Then
                            Dim oCart As Cart
                            oCart = New Cart(myWeb)
                            oCart.ListOrders(CInt("0" & myWeb.moRequest("QuoteId")))
                            oCart = Nothing
                        End If

                    End If

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ListQuotes", ex, ""))
                End Try
            End Sub

            Public Sub VoucherAction(ByRef myWeb As Protean.Cms, ByRef oContentNode As XmlElement)
                Try

                    'Check codes available

                    Select Case oContentNode.ParentNode.Name()
                        Case "Item" 'case for item in shopping cart

                            Dim CodeGroup As Integer = CInt("0" + oContentNode.SelectSingleNode("CodeGroup").InnerText)

                            If CodeGroup > 0 Then
                                'Save with current stock available
                                Dim sSql As String = "select count(nCodeKey) from tblCodes where nUseId is null and nIssuedDirId is null and nCodeParentId = " & CStr(CodeGroup)
                                Dim codesAvailable As Integer = myWeb.moDbHelper.ExeProcessSqlScalar(sSql)
                                Dim stockElmt As XmlElement = myWeb.moPageXml.CreateElement("Stock")
                                stockElmt.InnerText = CStr(CInt("0" + codesAvailable))
                                oContentNode.AppendChild(stockElmt)

                                'getQuantity
                                Dim ItemParent As XmlElement = oContentNode.ParentNode
                                Dim VoucherQuantity As Integer = ItemParent.GetAttribute("quantity")
                                Dim i As Integer
                                For i = 1 To VoucherQuantity
                                    'Get Any Product Specific Notes that have been completed and save against code
                                    Dim productName As String = oContentNode.SelectSingleNode("Name").InnerText
                                    Dim stockCode As String = oContentNode.SelectSingleNode("StockCode").InnerText
                                    Dim copyContentNode As XmlElement = oContentNode.CloneNode(True)

                                    Dim oNotes As XmlElement = oContentNode.ParentNode.ParentNode.SelectSingleNode("Notes/descendant-or-self::Item[@name='" & productName & "-" & stockCode & "' and @number='" & i & "']")
                                    If Not oNotes Is Nothing Then
                                        copyContentNode.AppendChild(oNotes.CloneNode(True))
                                    End If

                                    Dim newcode As String = myWeb.moDbHelper.IssueCode(CodeGroup, myWeb.mnUserId, False, copyContentNode)
                                    'Save Issued Code back to user
                                    Dim codeElmt As XmlElement = myWeb.moPageXml.CreateElement("IssuedCode")
                                    codeElmt.InnerText = newcode
                                    oContentNode.AppendChild(codeElmt)
                                Next

                            End If



                        Case "Contents"

                            Dim CodeGroup As Integer = CInt("0" + oContentNode.SelectSingleNode("CodeGroup").InnerText)

                            If CodeGroup > 0 Then
                                Dim sSql As String = "select count(nCodeKey) from tblCodes where nUseId is null and nCodeParentId = " & CStr(CodeGroup)
                                Dim codesAvailable As Integer = myWeb.moDbHelper.ExeProcessSqlScalar(sSql)
                                Dim stockElmt As XmlElement = myWeb.moPageXml.CreateElement("Stock")
                                stockElmt.InnerText = CStr(CInt("0" + codesAvailable))
                                oContentNode.AppendChild(stockElmt)
                            End If

                        Case "ContentDetail"

                    End Select


                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ListQuotes", ex, ""))
                End Try
            End Sub


            Public Sub ManageVouchers(ByRef myWeb As Protean.Cms, ByRef oContentNode As XmlElement)
                Try
                    If myWeb.mnUserId > 0 Then
                        myWeb.moDbHelper.mnUserId = myWeb.mnUserId
                        myWeb.moDbHelper.ListUserVouchers(oContentNode)
                    End If

                    If myWeb.moRequest("VoucherId") <> "" Then
                        'Edit Voucher Code here....
                        Dim moAdXfm As Admin.AdminXforms = myWeb.getAdminXform()

                        moAdXfm.xFrmVoucherCode(myWeb.moRequest("VoucherId"))

                        If Not moAdXfm.valid Then
                            If myWeb.moContentDetail Is Nothing Then
                                myWeb.moContentDetail = myWeb.moPageXml.CreateElement("ContentDetail")
                                myWeb.moPageXml.DocumentElement.AppendChild(myWeb.moContentDetail)
                            End If
                            myWeb.moContentDetail.AppendChild(moAdXfm.moXformElmt)
                        End If

                    End If

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ListQuotes", ex, ""))
                End Try
            End Sub

        End Class
#End Region
    End Class

End Class

