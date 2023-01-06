<%@ WebHandler Language="VB" Class="testEventLog" %>

Imports System
Imports System.Web
Imports System.Collections.Generic
Imports Protean.fsHelper

Public Class testEventLog : Implements IHttpHandler, IRequiresSessionState

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        Dim oEw As Protean.Cms = New Protean.Cms
        oEw.InitializeVariables()

        Try
            Throw New DivideByZeroException()
        Catch ex As DivideByZeroException
            Protean.stdTools.AddExceptionToEventLog(ex, "Test")
        End Try

        oEw = Nothing
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class