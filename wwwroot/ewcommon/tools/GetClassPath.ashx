<%@ WebHandler Language="VB" Class="Handler" %>

Imports System
Imports System.Web

Public Class Handler : Implements IHttpHandler

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "text/plain"
        Dim oWeb As New Protean.Cms


        Dim pa As New Protean.Cms.Admin.JSONActions


        Dim type As Type = pa.GetType()
        Dim typeName As String = type.FullName
        context.Response.Write(type.FullName)

    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class