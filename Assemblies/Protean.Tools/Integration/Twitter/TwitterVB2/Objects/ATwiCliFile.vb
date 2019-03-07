Namespace Integration.Twitter.TwitterVB2
    Public MustInherit Class ATwiCliFile
#Region "TCF Constants"
        Public Const TCF_DIRECT_URL As String = "http://twic.li/api/"
#End Region
        Private m_strID As String
        Private m_strUsername As String
        Private m_strUserID As String
        Private m_strURL As String
        Private m_iComments As Integer
        Private m_iViews As Integer
        Private m_dtTimeStamp As DateTime

        Public Property UserID() As String
            Get
                Return m_strUserID

            End Get
            Set(ByVal value As String)
                m_strUserID = value

            End Set
        End Property
        Public Property ID() As String
            Get
                Return m_strID

            End Get
            Set(ByVal value As String)
                m_strID = value

            End Set
        End Property

        Public Property ScreenName() As String
            Get
                Return m_strUsername

            End Get
            Set(ByVal value As String)
                m_strUsername = value
            End Set
        End Property

        Public Property URL() As String
            Get
                Return m_strURL

            End Get
            Set(ByVal value As String)
                m_strURL = value
            End Set
        End Property

        Public Property Comments_Count() As Integer
            Get
                Return m_iComments

            End Get
            Set(ByVal value As Integer)
                m_iComments = value
            End Set
        End Property

        Public Property Views_Count() As Integer
            Get
                Return m_iViews

            End Get
            Set(ByVal value As Integer)
                m_iViews = value
            End Set
        End Property

        Public Property TimeStamp() As DateTime
            Get
                Return m_dtTimeStamp
            End Get
            Set(ByVal value As DateTime)
                m_dtTimeStamp = value
            End Set

        End Property

        Public Sub InsertUnixTime(ByVal p_dtTimeStamp As Double)
            Dim dtEpoch As New DateTime(1970, 1, 1, 0, 0, 0)
            m_dtTimeStamp = dtEpoch.AddSeconds(p_dtTimeStamp)

        End Sub
        Public MustOverride Function GetDirectURL() As String
        Public MustOverride Function GetExtension() As String

    End Class
End Namespace
