<%@ WebHandler Language="VB" Class="feed" %>

Imports System
Imports System.Web

Public Class feed : Implements IHttpHandler, IRequiresSessionState

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        Dim oEw As Protean.Cms = New Protean.Cms
        Dim strFileName As string = "vcal.ics"

        If context.Request("xml") <> "" Then
            oEw.mbOutputXml = True
        Else
            oEw.mcContentType = "text/calendar"
            context.Response.AddHeader("Content-Disposition", "attachment; filename=" & strFileName)
        End If

        'oEw.mbFeedMode = True
        oEw.InitializeVariables()

        If IO.File.Exists(context.Server.MapPath("/xsl/tools/vcalendar/vcalendarmac.xsl")) Then
            oEw.mcEwSiteXsl = "/xsl/tools/vcalendar/vcalendarmac.xsl"
        Else
            oEw.mcEwSiteXsl = "/ewcommon/xsl/tools/vcalendar/vcalendarmac.xsl"
        End If


        oEw.GetPageHTML()

        oEw = Nothing
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class