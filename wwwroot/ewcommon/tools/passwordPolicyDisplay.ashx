<%@ WebHandler Language="VB" Class="ewAjaxAdmin" %>

Imports System
Imports System.Web
Imports System.Web.Configuration
Imports System.Xml
Imports System.Xml.Serialization
Imports System.Xml.XPath

Public Class ewAjaxAdmin : Implements IHttpHandler, IRequiresSessionState
    
    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        Dim moPolicy As xmlElement
        
        moPolicy = WebConfigurationManager.GetWebApplicationSection("eonic/PasswordPolicy")
        
       
        Dim styleFile As String = CStr(context.Server.MapPath("/ewcommon/xsl/tools/PasswordPolicy.xslt"))
        Dim oTransform As New Eonic.XmlHelper.Transform()
        oTransform.XSLFile = styleFile
        oTransform.Compiled = false
        oTransform.ProcessTimed(moPolicy.OwnerDocument, context.Response)

        oTransform.Close()
        oTransform = Nothing
        
        
    End Sub   
    
    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class