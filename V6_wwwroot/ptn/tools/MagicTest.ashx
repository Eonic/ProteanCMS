<%@ WebHandler Language="VB" Class="ewTidyTest" %>

Imports System
Imports System.Web
Imports Protean

Public Class ewTidyTest : Implements IHttpHandler, IRequiresSessionState

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

        ' Initialise EonicWeb
        Dim oEw As Protean.Cms = New Protean.Cms
        '  oEw.mbAdminMode = True
        '  oEw.Open()

        oEw.InitializeVariables()
        Dim oFsh As New fsHelper(oEw.moCtx)
        oFsh.mcRoot = oEw.goServer.MapPath("/Images/~dis-150x150/")
        context.Response.Write(oFsh.OptimiseImages("/Images/~dis-150x150/", 0, 0, False))


    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class