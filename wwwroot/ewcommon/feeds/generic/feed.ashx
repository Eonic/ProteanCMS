<%@ WebHandler Language="VB" Class="eonicGenericFeed" %>

Imports System
Imports System.Web
Imports System.xml

Public Class eonicGenericFeed : Implements IHttpHandler, IRequiresSessionState

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

        Dim oEw As Protean.Cms = New Protean.Cms
        Dim req As HttpRequest = context.Request
        oEw.InitializeVariables()

        If context.Request("xml") <> "" Then
            oEw.mbOutputXml = True
        End If

        ' Load in the configs
        Dim mimeMajorType As String = req("mimeMajorType")
        Dim mimeSubType As String = req("mimeSubType")
        Dim mimeType As String = ""
        If Not (String.IsNullOrEmpty(mimeMajorType) Or String.IsNullOrEmpty(mimeSubType)) Then
            mimeType = mimeMajorType & "/" & mimeSubType
        End If

        oEw.GetFeedXML( _
                        IIf(String.IsNullOrEmpty(req("contentSchema") & ""), IIf(String.IsNullOrEmpty(req("contentType") & ""), "product", req("contentType")), req("contentSchema")), _
                        (req("showRelated") = "yes" Or req("showRelated") = "true"), _
                        IIf(Not (String.IsNullOrEmpty(req("pageId") & "")) AndAlso IsNumeric(req("pageId")), CInt("0" & req("pageId")), 0), _
                        (req("includeChildPages") = "yes" Or req("includeChildPages") = "true"), _
                        mimeType, _
                        req("feedName") _
                        )

        oEw = Nothing

    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return True
        End Get
    End Property

End Class