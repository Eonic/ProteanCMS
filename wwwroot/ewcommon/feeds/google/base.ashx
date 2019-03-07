<%@ WebHandler Language="VB" Class="google_base" %>

Imports System
Imports System.Web
Imports System.xml

Public Class google_base : Implements IHttpHandler, IRequiresSessionState

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

        Dim oEw As Protean.Cms = New Protean.Cms

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

        oEw.GetFeedXML(cContentSchema, bShowRelated, bContentDetail, cRelatedSchemasToShow, GroupId)

        oEw = Nothing

    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return True
        End Get
    End Property

End Class