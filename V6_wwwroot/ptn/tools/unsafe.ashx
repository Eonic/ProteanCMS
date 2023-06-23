<%@ WebHandler Language="VB" Class="ewTidyTest" %>

Imports System
Imports System.Web

Public Class ewTidyTest : Implements IHttpHandler, IRequiresSessionState

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

        '' Dim ver = System.Reflection.Assembly.GetAssembly(TypeOf (System.Runtime.CompilerServices.Unsafe)).GetName().Version
        'Dim ver As String = System.Reflection.Assembly.GetAssembly(Type.GetType(System.Runtime.CompilerServices.Unsafe)).GetName().Version

        Dim htmltotest As String = "<h1>Unsafe Version</h1>"
        context.Response.ContentType = "text/html"
        context.Response.Write("System.Runtime.CompilerServices.Unsafe v" & ver)

    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class