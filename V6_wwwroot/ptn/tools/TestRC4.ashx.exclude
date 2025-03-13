<%@ WebHandler Language="VB" Class="ewEnlargeImage" %>

Imports System
Imports System.Web
Imports System.Collections.Generic
Imports Protean.fsHelper

Public Class ewEnlargeImage : Implements IHttpHandler, IRequiresSessionState

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest


        Dim stringtoencrypt As String = "santosh"
        Dim key As String = "ptnAdLoX@T34"

        Dim oRC4 As New Protean.Tools.Encryption.RC4()
        Dim encrypted As String = oRC4.Encrypt(stringtoencrypt, key)


        context.Response.Write(encrypted)
        context.Response.Write("<br/>")

        Dim decrypted As String = oRC4.Decrypt(encrypted, key)

        context.Response.Write(decrypted)

        If decrypted = stringtoencrypt Then
            context.Response.Write("<br/>PASSSED!")
        End If

    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class