Public Class EnumUtility


    ' .NET 4 Upgrade would provide this function natively.
    ' Try not to use too much as it's a bit ineffective.
    Shared Function TryParse(ByVal enumType As Type, ByVal value As String, ByVal ignoreCase As Boolean, ByRef output As Object) As Boolean

        Try
            output = [Enum].Parse(enumType, value, ignoreCase)
            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

End Class
