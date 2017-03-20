<%@ WebHandler Language="VB" Class="ewEnlargeImage" %>

Imports System
Imports System.Web

Public Class ewEnlargeImage : Implements IHttpHandler, IRequiresSessionState
    
    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        Dim oEw As Eonic.Web = New Eonic.Web
        oEw.InitializeVariables()
        oEw.mcEwSiteXsl = "/ewcommon/xsl/tools/excel_2000.xsl"
        
        If context.Request("xml") <> "" Then
            oEw.mbOutputXml = True
        Else
            oEw.mcContentType = "application/vnd.ms-excel"
            context.Response.ContentType = "application/vnd.ms-excel"
            context.Response.AddHeader("Content-Disposition", "attachment;filename=" & context.Request("ewCmd") & ".xls")
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