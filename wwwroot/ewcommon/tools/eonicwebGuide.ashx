<%@ WebHandler Language="VB" Class="ewAjaxAdmin" %>

Imports System
Imports System.Web
Imports System.Xml



Public Class ewAjaxAdmin : Implements IHttpHandler, IRequiresSessionState


    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        Dim oEw As Eonic.Web = New Eonic.Web
        Dim oEwCom As Eonic.proteancms.com.ewAdminProxySoapClient = New Eonic.proteancms.com.ewAdminProxySoapClient
        Dim sProcessInfo As String = ""

        oEw.InitializeVariables()
        oEw.moPageXml.CreateXmlDeclaration("1.0", "UTF-8", "yes")
        Dim xReader As XmlReader = oEwCom.GetPageXml(context.Request("fRef")).CreateReader()
        xReader.MoveToContent()
        oEw.moPageXml.LoadXml(xReader.ReadOuterXml())
        oEw.mcEwSiteXsl = "/ewcommon/xsl/admin/guidePage.xsl"
        oEw.mbAdminMode = False
        oEw.GetPageHTML()

        oEw = Nothing
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property


End Class