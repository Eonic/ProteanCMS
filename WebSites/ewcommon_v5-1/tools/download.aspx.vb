Imports Eonic.Web
Partial Class tools_download
    Inherits System.Web.UI.Page
    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        'Put user code to initialize the page here
        Dim oEw As Eonic.Web = New Eonic.Web
        oEw.InitializeVariables()
        oEw.returnDocumentFromItem(System.Web.HttpContext.Current)
    End Sub
End Class
