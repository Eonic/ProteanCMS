<%@ WebHandler Language="VB" Class="ProductDetail" %>

Imports System
Imports System.Web
Imports System.Xml
Imports System.Data.SqlClient


Public Class ProductDetail : Implements IHttpHandler, IRequiresSessionState

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

        Dim oEw As Eonic.Web = New Eonic.Web
        Dim oPageXml As XmlDocument
        Dim nPageId As Long
        Dim cProductURL As String
        Try

            If context.Request("xml") <> "" Then
                oEw.mbOutputXml = True
            End If
            oEw.InitializeVariables()
            oEw.Open()

            Dim odb As Eonic.Web.dbHelper = oEw.moDbHelper
            'getTheContentId
            Dim nContentId As String = odb.getContentByRef(context.Request("fref"))

            'getThePageId
            'nPageId = odb.getPrimaryLocationByArtId(nContentId)

            Dim sSql As String
            Dim oDr As SqlDataReader
            sSql = "SELECT nStructId FROM tblContentLocation WHERE nContentId = " & nContentId & " and bPrimary = 1"
            oDr = odb.getDataReader(sSql)
            While oDr.Read
                nPageId = oDr(0)
            End While
            oDr.Close()
            oDr = Nothing

            oEw.mnPageId = nPageId
            oPageXml = oEw.GetPageXML()

            Dim pageNode As XmlElement = oPageXml.DocumentElement.SelectSingleNode("Menu/descendant-or-self::MenuItem[@id='" & nPageId & "']")
            Dim productNode As XmlElement = oPageXml.DocumentElement.SelectSingleNode("Contents/Content[@id='" & nContentId & "']")

            If pageNode Is Nothing Or productNode Is Nothing Then
                cProductURL = "/Page+Not+Found"
            Else
                cProductURL = pageNode.GetAttribute("url") & "/" & CStr(nContentId) & "-/" & Replace(productNode.GetAttribute("name"), " ", "-")
            End If

            'context.Response.Write(cProductURL)     

            context.Response.Status = "301 Moved Permanently"
            context.Response.AddHeader("Location", "http://" & context.Request.ServerVariables("HTTP_HOST") & cProductURL)

        Catch ex As Exception
            context.Response.Status = "500 Error"
        End Try

    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class