<%@ WebHandler Language="VB" Class="ewTidyTest" %>

Imports System
Imports System.Web

Public Class ewTidyTest : Implements IHttpHandler, IRequiresSessionState

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest



        Dim ptn As New Protean.Cms(context)
        Dim oImp As Protean.Tools.Security.Impersonate = New Protean.Tools.Security.Impersonate
        If oImp.ImpersonateValidUser(ptn.moConfig("AdminAcct"), ptn.moConfig("AdminDomain"), ptn.moConfig("AdminPassword"), , ptn.moConfig("AdminGroup")) Then
            context.Response.Write("User Logon Successful<br/><br/>")


            Dim htmltotest As String = "<h1>Test FS Write</h1>"
            context.Response.ContentType = "text/html"

            Dim oFS As New Protean.fsHelper(context)
            Dim filepath As String = "\ewCache\FSTEST"
            Dim sError As String = oFS.CreatePath(filepath)
            If sError = "1" Then
                context.Response.Write("Path Created: " & filepath & "<br/><br/>")

                Dim gcProjectPath = ""
                Try

                    '    Alphaleonis.Win32.Filesystem.File.WriteAllText("\\?\" & ptn.goServer.MapPath("/" & gcProjectPath) & filepath & "\test.html", htmltotest, System.Text.Encoding.UTF8)

                    context.Response.Write("file created<br/>")
                    context.Response.Write("<a href=""/ewCache/FSTest/Test.html"">Load File</a><br/>")

                Catch ex As Exception
                    context.Response.Write(ex.Message)
                End Try

            Else
                context.Response.Write(sError)
            End If
        Else
            context.Response.Write("User Logon Failed<br/><br/>")
        End If
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class