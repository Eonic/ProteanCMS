<%@ WebHandler Language="VB" Class="ewAjaxAdmin" %>

Imports System
Imports System.Web

Public Class ewAjaxAdmin : Implements IHttpHandler, IRequiresSessionState
    
    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        Dim oEw As Eonic.Web = New Eonic.Web
        oEw.InitializeVariables()

        If context.Request("xml") <> "" Then
            oEw.mbOutputXml = True
        End If
        oEw.mbAdminMode = True
        
        Dim type As Eonic.Web.dbHelper.ActivityType
        Select Case LCase(oEw.moRequest("type"))
            Case "email"
                type = Eonic.Web.dbHelper.ActivityType.Custom1
            Case "website"
                type = Eonic.Web.dbHelper.ActivityType.Custom2
            Case Else
                type = Eonic.Web.dbHelper.ActivityType.PageViewed
        End Select
        
        oEw.moDbHelper.logActivity(type, oEw.mnUserId, oEw.mnPageId, oEw.mnArtId, 0, oEw.moRequest("destination") & " " & oEw.moRequest.ServerVariables("REMOTE_ADDR") & " " & oEw.moRequest.ServerVariables("HTTP_USER_AGENT"))
               
        oEw = Nothing
        
    End Sub
    
    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class