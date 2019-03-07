<%@ WebHandler Language="VB" Class="ewAjaxAdmin" %>

Imports System
Imports System.Web
Imports System.Xml
Imports Eonic



Public Class ewAjaxAdmin : Implements IHttpHandler, IRequiresSessionState


    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        Dim oEw As Protean.Cms = New Protean.Cms
        Dim oEwCom As Protean.proteancms.com.ewAdminProxySoapClient = New Protean.proteancms.com.ewAdminProxySoapClient
        Dim sProcessInfo As String = ""
        Try
            oEw.InitializeVariables()
            oEw.moPageXml.CreateXmlDeclaration("1.0", "UTF-8", "yes")
            Dim xReader As XmlReader = oEwCom.GetPageXml(context.Request("fRef")).CreateReader()
            xReader.MoveToContent()
            oEw.moPageXml.LoadXml(xReader.ReadOuterXml())
            oEw.mcEwSiteXsl = "/ewcommon/xsl/admin/guidePage.xsl"
            oEw.mbAdminMode = False
            oEw.GetPageHTML()
            oEw = Nothing
        Catch ex As Exception
            context.Response.Write(ex.Message & ex.StackTrace)
        End Try


    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property


End Class