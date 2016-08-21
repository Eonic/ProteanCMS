<%@ WebHandler Language="VB" Class="ewUploadFiles" %>

Imports System
Imports System.Web
Imports System.Web.SessionState

Public Class ewUploadFiles : Implements IHttpHandler, IRequiresSessionState
    
    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        
        'First lets check for the expected session variable so we don't get hacked
        If context.Session("allowUpload") = "True" Then
            
            context.Response.Write("Upload result:<br>") 'At least one symbol should be sent to response!!!
		
            Dim FolderToSave As String = context.Server.MapPath("\" & context.Request("targetUrl"))
		
		dim fsHelper as new Eonic.fsHelper
		
            Dim myFile As HttpPostedFile = context.Request.Files(0)
            If Not myFile Is Nothing AndAlso myFile.FileName <> "" Then
                myFile.SaveAs(FolderToSave & "\" & System.IO.Path.GetFileName(myFile.FileName))
                context.Response.Write("File " & myFile.FileName & " succesfully saved.<br>")
            Else
                context.Response.Write("No files sent. Script is OK!")  'Say to Flash that script exists and can receive files
            End If
         else
             context.Response.Write("No files sent. Session Invalid!")
        End If
       
        
    End Sub
 
    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class
