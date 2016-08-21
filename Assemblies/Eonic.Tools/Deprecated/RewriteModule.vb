'' ---------------------------------------------------------
'' Library   :   Eonic.Tools.httpModule
'' Author    :   Trevor Spink
'' Website   :   www.eonic.co.uk
''
'' (c)Copyright 2007 Eonic Ltd.
'' ---------------------------------------------------------

'Option Strict On
'Option Explicit On 

'Imports System
'Imports System.Web

'Namespace HttpModule
'    ' HttpModule Class. Responsible for handling any URL rewritting based on the rules defined within the URLRewrite.config configuration file.

'    Public MustInherit Class RewriteModule
'        Implements IHttpModule

'#Region "Public & Protected Methods"

'        Public Overridable Sub Init(ByVal HttpApplication As HttpApplication) Implements IHttpModule.Init
'            AddHandler HttpApplication.BeginRequest, AddressOf Me.HttpApplication_BeginRequest
'        End Sub

'        Protected Overridable Sub HttpApplication_BeginRequest(ByVal sender As Object, ByVal e As EventArgs)
'            Dim App As HttpApplication = CType(sender, HttpApplication)
'            Rewrite(App.Request.Path, App)
'        End Sub

'        Public Overridable Sub Dispose() Implements IHttpModule.Dispose
'        End Sub

'        Protected MustOverride Sub Rewrite(ByVal requestedPath As String, ByVal HttpApplication As HttpApplication)

'#End Region

'    End Class

'    Public Class RewriterModule
'        Inherits RewriteModule

'#Region "Protected Methods"

'        ''' Performs the URL Rewritting based on the rules within the XML rules file.

'        Protected Overrides Sub Rewrite(ByVal requestedPath As String, ByVal app As System.Web.HttpApplication)

'            RewriteUrl(app.Context, requestedPath)

'        End Sub

'#End Region

'#Region "Private Methods"

'        Private Overloads Shared Sub RewriteUrl(ByVal Context As System.Web.HttpContext, ByVal SendToUrl As String)

'            Dim x, y As String
'            RewriteUrl(Context, SendToUrl, x, y)

'        End Sub


'        ''' Private URL Rewritting method. Append any additional hardcoded querystring we may not wish to rewrite.

'        Private Overloads Shared Sub RewriteUrl(ByVal Context As System.Web.HttpContext, _
'        ByVal SendToUrl As String, ByRef SendToUrlLessQString As String, ByRef FilePath As String)

'            ' see if we need to add any extra querystring information
'            If Context.Request.QueryString.Count > 0 Then
'                If SendToUrl.IndexOf("?") <> -1 Then
'                    SendToUrl += "&" + Context.Request.QueryString.ToString()
'                Else
'                    SendToUrl += "?" + Context.Request.QueryString.ToString()
'                End If
'            End If

'            ' first strip the querystring, if any
'            Dim queryString As String = String.Empty
'            SendToUrlLessQString = SendToUrl
'            If SendToUrl.IndexOf("?") >= 0 Then
'                SendToUrlLessQString = SendToUrl.Substring(0, SendToUrl.IndexOf("?"))
'                queryString = SendToUrl.Substring(SendToUrl.IndexOf("?") + 1)
'            End If

'            Dim sIgnoreDirectories As String = "ewcommon"
'            Dim aIgnoreDirectories() As String = Split(sIgnoreDirectories)
'            Dim i As Integer
'            Dim ignoreRedirect As Boolean = False
'            For i = 0 To UBound(aIgnoreDirectories)
'                If SendToUrlLessQString.StartsWith("/" & aIgnoreDirectories(i)) Then
'                    ignoreRedirect = True
'                End If
'            Next
'            Dim PagePath As String = SendToUrlLessQString.Replace(".ashx", "").TrimStart("/".ToCharArray)

'            Dim pageId As String = ""

'            If Not PagePath.LastIndexOf("~".ToCharArray) = -1 Then
'                Dim finalPage As String = Right(PagePath, Len(PagePath) - PagePath.LastIndexOf("/".ToCharArray) - 1)
'                If InStr(finalPage, "~") > 0 Then
'                    pageId = finalPage.Substring(0, InStr(finalPage, "~") - 1)
'                    If IsNumeric(pageId) Then
'                        pageId = "&artId=" & pageId
'                        PagePath = PagePath.Substring(0, PagePath.LastIndexOf("/".ToCharArray))
'                    Else
'                        pageId = ""
'                    End If
'                End If
'            End If



'            If (Not SendToUrlLessQString = "/default.ashx") And ignoreRedirect = False Then
'                If queryString <> "" Then queryString = "&" & queryString
'                queryString = "path=" & PagePath & pageId & queryString
'                SendToUrlLessQString = "/default.ashx"
'            End If

'            Context.Current.RewritePath(SendToUrlLessQString, String.Empty, queryString)

'        End Sub

'#End Region

'    End Class

'End Namespace