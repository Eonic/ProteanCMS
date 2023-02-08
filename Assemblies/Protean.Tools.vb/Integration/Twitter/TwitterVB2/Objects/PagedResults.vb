Namespace Integration.Twitter.TwitterVB2
    Public Class PagedResults(Of T)
        Inherits List(Of T)

        Public Sub New()
            LCursor = -1
        End Sub

        Public Sub New(ByVal Cursor As Long)
            LCursor = Cursor
        End Sub

        Dim LCursor As Long
        Public Property Cursor() As Long
            Get
                Return LCursor
            End Get
            Set(ByVal value As Long)
                LCursor = Value
            End Set
        End Property

        Public ReadOnly Property HasMore() As Boolean
            Get
                Return Cursor <> 0
            End Get
        End Property

    End Class
End Namespace
