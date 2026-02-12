<%@ WebHandler Language="VB" Class="Handler" %>

Imports System
Imports System.Web

Imports System.Reflection

Public Class Handler : Implements IHttpHandler

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "text/plain"
        Dim oWeb As New Protean.Cms

        Dim apiLog = New Protean.Cms.dbHelper.utils.APILog(oWeb.moDbHelper)
        Dim jsa As New Protean.Cms.Cart.JSONActions(apiLog)
        context.Response.Write(jsa.GetType().ToString())

        ' Dim calledType As Type
        '  calledType = System.Type.GetType("Eonic.Web.Cart.JSONActions", True)

        '  Dim assemblyInstance As [Assembly] = [Assembly].Load("Eonic.Zygostatics")

        ' context.Response.Write(calledType.FullName)


    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class