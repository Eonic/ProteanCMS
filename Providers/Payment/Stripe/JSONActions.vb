Option Strict Off
Option Explicit On
Imports System.Xml
Imports System.Collections
Imports System.Web.Configuration
Imports System.Data.SqlClient
Imports System.Web.HttpUtility
Imports VB = Microsoft.VisualBasic
Imports System.IO
Imports System.Net
Imports System.Text
Imports Eonic.Tools.Xml
Imports Eonic.Tools.Xml.XmlNodeState
Imports System



#Region "JSON Actions"

Public Class JSONActions
            Public Event OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)
            Private Const mcModuleName As String = "SandIQuote.JSONActions"
            Private moLmsConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("eonic/quote")
            Private myWeb As Eonic.Web
            Private myCart As Eonic.Web.Cart

            Public Sub New()
                Dim ctest As String = "this constructor is being hit" 'for testing
                myWeb = New Eonic.Web()
                myWeb.InitializeVariables()
                myWeb.Open()
        '  myCart = New Eonic.Web.Cart(myWeb)

    End Sub

    Public Function GetAuthURL(ByRef myApi As Eonic.API, ByRef jObj As Newtonsoft.Json.Linq.JObject) As String
        Try
            Dim AuthURL As String
            Dim Query As String = "response_type=code&client_id=SMu8cmSHLwPlZgldPiZhim6e0uJzLamkwjUWlAOekcD6v9Jio94ZxOqCFL_xWPJU&scope=read_write&redirect_uri=http%3A%2F%2Fwww.eonicweb.net%2Fgocardless%2Fcallback&state=q8wEr9yMohTP"

            Dim req As WebRequest = WebRequest.Create("https://connect-sandbox.gocardless.com/oauth/authorize?" & Query)
            req.ContentType = "application/json"
            req.Method = "GET"

            Dim response = req.GetResponse().GetResponseStream()

            Dim reader As New StreamReader(response)
            Dim res As String = reader.ReadToEnd()
            reader.Close()
            response.Close()


            Return res

        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "GetAuthURL", ex, ""))
            Return ex.Message
        End Try

    End Function

#End Region


End Class


