<%@ WebHandler Language="VB" Class="ewEnlargeImage" %>

Imports System
Imports System.Web
Imports System.Collections.Generic
Imports Eonic.fsHelper

Public Class ewEnlargeImage : Implements IHttpHandler, IRequiresSessionState
    
    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        Dim oEw As Eonic.Web = New Eonic.Web
        oEw.InitializeVariables()
        
        'Dim fs As New Eonic.fsHelper()
        'Dim l As New System.Collections.Generic.List(Of String)
        'l = fs.GetFileListByTypeFromRelativePath("/foo", Eonic.fsHelper.LibraryType.Image, True)
        'context.Response.Write(String.Join(",", l.ToArray()))
        
        Dim s As New Eonic.Web.Synchronisation(oEw)
        s.SyncModuleContentFromFolder(61)
        'Dim vp As String = "/images/image1.jpg"
        'Dim wf As New Eonic.fsHelper.WebFile(context.Server.MapPath(vp), vp, True)

        'Dim wfList As New List(Of WebFile)

        'vp = "/images/image1.jpg"
        'wfList.Add(New WebFile(context.Server.MapPath(vp), vp, True))

        'vp = "/images/John_Lacoux_Detail.jpg"
        'wfList.Add(New WebFile(context.Server.MapPath(vp), vp, True))

        'vp = "/images/Home_Page_Painting2.jpg"
        'wfList.Add(New WebFile(context.Server.MapPath(vp), vp, True))


        'Dim st As String = Eonic.Tools.Xml.Serialize(Of List(Of WebFile))(wfList)

        'Dim s As Char = st.ToCharArray()(0)
        'Dim p As Char = st(0)


        'Dim stx As Xml.XmlElement = Eonic.Tools.Xml.SerializeToXml(Of List(Of WebFile))(wfList)

        
        
        ' context.Response.Write(st)
      
        
        
        oEw = Nothing
    End Sub
 
    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class