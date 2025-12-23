<%@ WebHandler Language="VB" Class="google_base" %>

Imports System
Imports System.Web
Imports System.xml

Public Class google_base : Implements IHttpHandler, IRequiresSessionState

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

        Dim oEw As Protean.Cms = New Protean.Cms()

        oEw.InitializeVariables()

        If context.Request("xml") <> "" Then
            oEw.mbOutputXml = True
        End If

        Dim cContentSchema As String = context.Request("contentType")
        Dim bShowRelated As Boolean = False
        Dim bContentDetail As Boolean = False
        Dim cRelatedSchemasToShow As String = ""

        'if there's no cContentSchema, use "product"
        If String.IsNullOrEmpty(cContentSchema) = True Then
            cContentSchema = "product"
        End If

        If String.IsNullOrEmpty(context.Request("showRelated")) = True Then
            bShowRelated = False
        Else
            bShowRelated = True
            'grab limiter for Related Schemas from showRelated, if specified
            Select Case LCase(context.Request("showRelated"))
                Case "yes", "true", "on"
                    cRelatedSchemasToShow = ""
                Case Else
                    cRelatedSchemasToShow = context.Request("showRelated")
            End Select

        End If

        'always return contentDetail xml for products
        If cContentSchema = "product" Then
            bContentDetail = True
        End If

        Dim GroupId As Int32 = 0
        If context.Request("groupId") <> "" Then
            GroupId = context.Request("groupId")
        End If

        If LCase(context.Request("showRelated")) = "sku" AndAlso context.Request("SaveFile") <> String.Empty Then
            If oEw.msException Is Nothing Then
                oEw.msException = String.Empty
            End If
            ' SKU feed XSLT
            Dim Skudoc As XmlDocument = oEw.BuildFeedXML(cContentSchema, bShowRelated, 0, True, bContentDetail, cRelatedSchemasToShow, GroupId)

            Dim StyleFile As String = oEw.goServer.MapPath("/xsl/feeds/google/product-specification.xsl")
            context.Response.ContentType = "application/xml"
            Dim oTransform As New Protean.XmlHelper.Transform(oEw, StyleFile, False, , False)
            Using sw As New System.IO.StreamWriter(System.IO.File.Open(oEw.goServer.MapPath("/feeds/" + context.Request("SaveFile") + ".xml"), System.IO.FileMode.OpenOrCreate))
                oTransform.Process(Skudoc, sw)
                oTransform.Process(Skudoc, context.Response)
            End Using
            'If context.Request("SaveFile") <> String.Empty Then
            '    Skudoc.Save(oEw.goServer.MapPath("/feeds/" + context.Request("SaveFile") + "Raw.xml"))
            'End If
        Else
            ' Existing product feed XSLT
            oEw.GetFeedXML(cContentSchema, bShowRelated, bContentDetail, cRelatedSchemasToShow, GroupId)
        End If

        oEw = Nothing

    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return True
        End Get
    End Property

End Class