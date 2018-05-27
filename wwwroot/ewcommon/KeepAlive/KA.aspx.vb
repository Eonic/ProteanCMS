
Partial Class KA
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Progress()
        Response.AddHeader("Refresh", 1)
    End Sub

    Public Sub Progress()

        Me.lblDescription.Text = ""
        Me.clDescription.Text = ""
        Me.clExtraInfo.Text = ""

        If Not HttpContext.Current.Session("KA_Description") = "" Then
            Me.lblDescription.Text = HttpContext.Current.Session("KA_Description")
        End If
        If Not HttpContext.Current.Session("KA_ExtraInfo") = "" Then
            Me.clExtraInfo.Text = HttpContext.Current.Session("KA_ExtraInfo")
        End If
        Dim nDone As Integer = 0
        Dim nTotal As Integer = 0
        If Not HttpContext.Current.Session("KA_NumberDone") = "" Then
            If IsNumeric(HttpContext.Current.Session("KA_NumberDone")) Then
                nDone = HttpContext.Current.Session("KA_NumberDone")
            End If
        End If
        If Not HttpContext.Current.Session("KA_NumberTotal") = "" Then
            If IsNumeric(HttpContext.Current.Session("KA_NumberTotal")) Then
                nTotal = HttpContext.Current.Session("KA_NumberTotal")
            End If
        End If
        If nTotal = 0 Then Exit Sub
        Dim nPerc As Double = (nDone / nTotal) * 100


        Me.clDescription.Text = FormatNumber(nPerc, 2) & "% (" & nDone & " / " & nTotal & ")"
        If nPerc > 5 Then Me.ProgressCell0.CssClass = "ProgressCellComplete"
        If nPerc > 10 Then Me.ProgressCell1.CssClass = "ProgressCellComplete"
        If nPerc > 15 Then Me.ProgressCell2.CssClass = "ProgressCellComplete"
        If nPerc > 20 Then Me.ProgressCell3.CssClass = "ProgressCellComplete"
        If nPerc > 25 Then Me.ProgressCell4.CssClass = "ProgressCellComplete"
        If nPerc > 30 Then Me.ProgressCell5.CssClass = "ProgressCellComplete"
        If nPerc > 35 Then Me.ProgressCell6.CssClass = "ProgressCellComplete"
        If nPerc > 40 Then Me.ProgressCell7.CssClass = "ProgressCellComplete"
        If nPerc > 45 Then Me.ProgressCell8.CssClass = "ProgressCellComplete"
        If nPerc > 50 Then Me.ProgressCell9.CssClass = "ProgressCellComplete"
        If nPerc > 55 Then Me.ProgressCell10.CssClass = "ProgressCellComplete"
        If nPerc > 60 Then Me.ProgressCell11.CssClass = "ProgressCellComplete"
        If nPerc > 65 Then Me.ProgressCell12.CssClass = "ProgressCellComplete"
        If nPerc > 70 Then Me.ProgressCell13.CssClass = "ProgressCellComplete"
        If nPerc > 75 Then Me.ProgressCell14.CssClass = "ProgressCellComplete"
        If nPerc > 80 Then Me.ProgressCell15.CssClass = "ProgressCellComplete"
        If nPerc > 85 Then Me.ProgressCell16.CssClass = "ProgressCellComplete"
        If nPerc > 90 Then Me.ProgressCell17.CssClass = "ProgressCellComplete"
        If nPerc > 95 Then Me.ProgressCell18.CssClass = "ProgressCellComplete"
        If nPerc >= 100 Then Me.ProgressCell19.CssClass = "ProgressCellComplete"

    End Sub
End Class
