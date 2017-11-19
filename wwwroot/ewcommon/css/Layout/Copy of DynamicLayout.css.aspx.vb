
Partial Class css_Layout_DynamicLayout
    Inherits System.Web.UI.Page

    Public Function rd(ByVal num As Double) As Integer
        Return CInt(Format(num, "#.00"))
    End Function

    Public Function getGoldenArray(Optional ByVal PageWidth As Integer = 972) As String()
        Dim currentValue As Double = PageWidth
        Dim csv As String = ""
        Do While currentValue >= 1
            csv = csv & rd(currentValue) & ","
            currentValue = currentValue / 1.62
        Loop

        Return Split(csv, ",")

    End Function

End Class
