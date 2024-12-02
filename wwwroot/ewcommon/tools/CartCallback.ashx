﻿<%@ WebHandler Language="VB" Class="CartCallback" %>

Imports System
Imports System.Web
Imports System.Data
Imports System.Data.SqlClient
Imports System.IO
Imports System.Web.Configuration

Public Class CartCallback : Implements IHttpHandler

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        Dim sSql As String
        Dim oEw As Protean.Cms = New Protean.Cms

        Dim moPaymentCfg As Object = WebConfigurationManager.GetWebApplicationSection("eonic/payment")

        Dim SellerNotes As String = ""

        oEw.InitializeVariables()
        oEw.Open()

        Try

            Dim Cart As Protean.Cms.Cart = New Protean.Cms.Cart(oEw)
            Select Case context.Request("provider")
                Case Is = "NetBanx"
                    Dim oNetBanxCfg = moPaymentCfg.SelectSingleNode("provider[@name='NetBanx']")
                    Dim oDictOpt = Protean.Tools.Xml.xmlToHashTable(oNetBanxCfg, "value")

                    Dim nCartId As String = context.Request("nbx_merchant_reference")
                    Dim cKey As String = oDictOpt("secretkey")
                    Dim cHash As String = context.Request("nbx_payment_amount") & context.Request("nbx_currency_code") & context.Request("nbx_merchant_reference") & context.Request("nbx_netbanx_reference") & cKey
                    If Protean.Tools.Encryption.HashString(cHash, Protean.Tools.Encryption.Hash.Provider.Sha1, True) = context.Request("nbx_checksum") Then


                        Select Case context.Request("nbx_status")
                            Case "passed"

                                If Cart.mnProcessId <> 6 Then
                                    SellerNotes = context.Request("nbx_payment_type") & " " & context.Request("nbx_netbanx_reference") & vbLf & Today & " " & TimeOfDay & ": changed to: (Payment Received) " & vbLf
                                    Cart.mcCartCmd = "ShowCallBackInvoice"
                                    Cart.mnCartId = CInt(context.Request("nbx_merchant_reference"))
                                    Cart.mnProcessId = 6
                                    Cart.mbQuitOnShowInvoice = False
                                    Cart.apply()
                                    context.Response.Write("NETBANXOK:1")
                                Else
                                    SellerNotes = context.Request("nbx_netbanx_reference") & vbLf & Today & " " & TimeOfDay & ": changed to: (Callback Repeated) " & vbLf
                                    context.Response.Write("NETBANXOK:1")
                                End If

                            Case Else
                                SellerNotes = context.Request("nbx_status") & vbLf & Today & " " & TimeOfDay & ": changed to: (Payment Failed) " & vbLf
                                context.Response.Write("NETBANXOK:1")
                        End Select
                        'update the auditId             
                    Else
                        SellerNotes = "hash[" & cHash & "][" & Protean.Tools.Encryption.HashString(cHash, Protean.Tools.Encryption.Hash.Provider.Sha1, True) & "]=[" & context.Request("nbx_checksum") & "]" & vbLf & Today & " " & TimeOfDay & ": changed to: (Checksum Failed) " & vbLf
                        context.Response.Write("CHECKSUMFAIL:0")
                    End If

                    oEw.moDbHelper.logActivity(Protean.Cms.dbHelper.ActivityType.Alert, 0, 0, 0, "CALLBACK : " & SellerNotes)

                    sSql = "update tblCartOrder set cSellerNotes = '" & Protean.Tools.Database.SqlFmt(SellerNotes) & "' where nCartOrderKey = " & nCartId
                    oEw.moDbHelper.ExeProcessSql(sSql)

                Case Is = "WorldPay"

                    Dim nCartId As String = context.Request("reference")
                    ' Dim cKey As String = oDictOpt("secretkey")
                    ' Dim cHash As String = context.Request("nbx_payment_amount") & context.Request("nbx_currency_code") & context.Request("nbx_merchant_reference") & context.Request("nbx_netbanx_reference") & cKey
                    ' If Eonic.Tools.Encryption.HashString(cHash, "SHA1", True) = context.Request("nbx_checksum") Then

                    Select Case context.Request("transStatus")
                        Case "N"
                            'Returned but transaction failed
                            SellerNotes = vbLf & Today & " " & TimeOfDay & ": changed to: (Payment Failed) " & vbLf
                            context.Response.Write("ResponseOK:0" & "<br/>")
                            'For Each item In context.Request.QueryString
                            '    SellerNotes = SellerNotes & "QS-" & CStr(item) & " - " & context.Request.QueryString(CStr(item)) & "<br/>"
                            'Next item

                            'For Each item In context.Request.Form
                            '    SellerNotes = SellerNotes & "Fm-" & CStr(item) & " - " & context.Request.Form(CStr(item)) & "<br/>"
                            'Next item
                            'context.Response.Write(SellerNotes)

                        Case "Y"
                            'successful transaction we have and ID !!!
                            If Cart.mnProcessId <> Protean.Cms.Cart.cartProcess.Complete Then

                                SellerNotes = context.Request("transId") & vbLf & Today & " " & TimeOfDay & ": changed to: (Payment Received) " & vbLf
                                Cart.mcCartCmd = "ShowCallBackInvoice"
                                Cart.mnCartId = CInt(context.Request("cartId"))
                                Cart.mnProcessId = Protean.Cms.Cart.cartProcess.Complete
                                Cart.mbQuitOnShowInvoice = False
                                Cart.apply()

                                Dim redirectURL As String = "http"
                                If context.Request.ServerVariables("HTTPS") = "on" Then redirectURL = redirectURL & "s"
                                redirectURL = redirectURL & "://" & context.Request.ServerVariables("SERVER_NAME") & "/?cartCmd=ShowInvoice"
                                Dim responseStr As String
                                responseStr = "<html><head><meta http-equiv=""refresh"" content=""0;URL='" & redirectURL & "'""/></head></html>"
                                'responseStr = "cartId:" & context.Request("reference") & "|" & SellerNotes
                                context.Response.Write(responseStr)
                            Else
                                oEw.moDbHelper.logActivity(Protean.Cms.dbHelper.ActivityType.Alert, 0, 0, 0, "CARTID NOT : 6 but " & Cart.mnProcessId)
                            End If

                        Case Else
                            SellerNotes = vbLf & Today & " " & TimeOfDay & ": changed to: (Payment Failed) " & vbLf
                            context.Response.Write("ResponseOK:0" & "<br/>")
                            Dim item As Object
                            For Each item In context.Request.QueryString
                                SellerNotes = SellerNotes & "QS-" & CStr(item) & " - " & context.Request.QueryString(CStr(item)) & "<br/>"
                            Next item

                            For Each item In context.Request.Form
                                SellerNotes = SellerNotes & "Fm-" & CStr(item) & " - " & context.Request.Form(CStr(item)) & "<br/>"
                            Next item
                            context.Response.Write(SellerNotes)

                    End Select

                    oEw.moDbHelper.logActivity(Protean.Cms.dbHelper.ActivityType.Alert, 0, 0, 0, "CALLBACK : " & SellerNotes)

                    sSql = "update tblCartOrder set cSellerNotes = '" & Protean.Tools.Database.SqlFmt(SellerNotes) & "' where nCartOrderKey = " & nCartId
                    oEw.moDbHelper.ExeProcessSql(sSql)
                    'context.Response.Write(sSql)

                Case Else
                    oEw.moDbHelper.logActivity(Protean.Cms.dbHelper.ActivityType.Alert, 0, 0, 0, "FAILED CALLBACK : " & context.Request.ServerVariables("HTTP_URL"))
            End Select

            oEw = Nothing

        Catch ex As Exception
            oEw.moDbHelper.logActivity(Protean.Cms.dbHelper.ActivityType.Alert, 0, 0, 0, "CALLBACK ERROR : " & ex.Message & ex.StackTrace)
            context.Response.Write("CALLBACK ERROR : " & ex.Message & ex.StackTrace)
        End Try

    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class