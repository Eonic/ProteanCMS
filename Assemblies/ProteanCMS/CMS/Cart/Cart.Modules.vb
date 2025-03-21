﻿Option Strict Off
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

                                    Dim newcode As String = ""
                                    If myWeb.mnUserId <> 0 Then
                                        newcode = myWeb.moDbHelper.IssueCode(CodeGroup, myWeb.mnUserId, False, copyContentNode)
                                    End If
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

            Public Sub RedeemTickets(ByRef myWeb As Protean.Cms, ByRef oContentNode As XmlElement)
                Try

                    ' Dim redeemticketsGroupId = myWeb.moConfig("TicketOfficeGroupId")
                    Dim userId As Long = myWeb.mnUserId
                    Dim mbIsTicketOffice As Boolean = myWeb.moDbHelper.checkUserRole("Ticket Office", "Role", userId)

                    oContentNode.RemoveAttribute("ticketValid")
                    oContentNode.SetAttribute("enteredTicketCode", myWeb.moRequest("code"))

                    Dim tktCode As String = myWeb.moRequest("code")
                    Dim tktKey As String = String.Empty

                    'get the key of the ticketcode - join against cartitem and cartorder
                    Dim cdchkStr As String = "select tblCodes.nCodeKey, tblCodes.dUseDate, tblCartOrder.nCartOrderKey, tblCartItem.nCartItemKey from tblCodes inner join tblCartItem on tblCodes.nUseId = tblCartItem.nCartItemKey "
                    cdchkStr = cdchkStr & "inner join tblCartOrder on tblCartItem.nCartOrderId = tblCartOrder.nCartOrderKey "
                    cdchkStr = cdchkStr & "where nCartStatus in (6, 9 ,17) and tblCodes.cCode = '" & tktCode & "'"
                    Using oDr As SqlDataReader = myWeb.moDbHelper.getDataReaderDisposable(cdchkStr)  'Done by nita on 6/7/22

                        oContentNode.SetAttribute("ticketValid", "invalid")

                        While oDr.Read()
                            If IsDBNull(oDr("dUseDate")) Then 'tkt has not been validated yet
                                'get event name, datetime and purchaser name
                                Dim tktDet As String = "select (CAST(cCartXml AS XML)).value('/Order[1]/Contact[1]/GivenName[1]', 'VARCHAR(255)') AS 'PurchaserName',"
                                tktDet += "(CAST(xItemXml AS XML)).value('/Content[1]/Name[1]', 'VARCHAR(255)') AS 'EventName', (CAST(xItemXml AS XML)).value('/Content[1]/Description[1]', 'VARCHAR(1000)') AS 'Venue',"
                                tktDet += "(CAST(xItemXml AS XML)).value('/Content[1]/StartDate[1]', 'VARCHAR(255)') + ' ' + (CAST(xItemXml AS XML)).value('/Content[1]/Times[1]/@start', 'VARCHAR(255)') AS 'Time',"
                                tktDet += "(CAST(xItemXml AS XML)).value('/Content[1]/StartDate[1]', 'VARCHAR(255)') AS 'EventDate'"
                                tktDet += " From tblCartItem inner Join tblCartOrder On tblCartOrder.nCartOrderKey = tblCartItem.nCartOrderId"
                                tktDet += " Where tblCartOrder.nCartOrderKey = " & oDr("nCartOrderKey") & " and tblCartItem.nCartItemKey = " & oDr("nCartItemKey")
                                Using oDr1 As SqlDataReader = myWeb.moDbHelper.getDataReaderDisposable(tktDet)  'Done by nita on 6/7/22
                                    While oDr1.Read()
                                        oContentNode.SetAttribute("PurchaserName", oDr1("PurchaserName"))
                                        oContentNode.SetAttribute("EventName", oDr1("EventName"))
                                        oContentNode.SetAttribute("Venue", oDr1("Venue"))
                                        oContentNode.SetAttribute("Time", oDr1("Time"))

                                        Dim eDay As Date = CDate(oDr1("EventDate"))
                                        If eDay <> DateTime.Today Then
                                            oContentNode.SetAttribute("ticketValid", "notToday")
                                            Return
                                        End If
                                    End While
                                End Using
                                If mbIsTicketOffice Then
                                    If myWeb.moDbHelper.RedeemCode(myWeb.moRequest("code")) Then 'if ticket got validated successfully
                                        oContentNode.SetAttribute("ticketValid", "validated")
                                    End If
                                Else
                                    oContentNode.SetAttribute("ticketValid", "valid")
                                End If
                            Else
                                Dim useStr As String = "select top 1 dUseDate from tblCodes where cCode = '" & tktCode & "'"
                                Using oDr2 As SqlDataReader = myWeb.moDbHelper.getDataReaderDisposable(useStr)  'Done by nita on 6/7/22
                                    While oDr2.Read()
                                        oContentNode.SetAttribute("lastUsedTime", oDr2("dUseDate"))
                                    End While
                                End Using
                                oContentNode.SetAttribute("ticketValid", "used")
                            End If
                        End While


                    End Using
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ListQuotes", ex, ""))
                End Try
            End Sub


        End Class
#End Region
    End Class

End Class

