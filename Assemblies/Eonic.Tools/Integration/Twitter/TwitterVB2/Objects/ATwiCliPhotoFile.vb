Namespace Integration.Twitter.TwitterVB2
    Public Class ATwiCliPhotoFile
        Inherits ATwiCliFile

#Region "TCPF Constants"
        Const TCPF_PHOTO_STRING As String = "photo.jpg"
        Const TCPF_PHOTO_EXTENSION As String = ".jpg"
#End Region
        Private m_iUserTags As Integer
        Private m_dLongitude As Double
        Private m_dLatitude As Double
        Private m_bShowMap As Boolean
        Private m_strCameraMake As String
        Private m_strCameraModel As String
        Public Property UserTags_Count() As Integer
            Get
                Return m_iUserTags

            End Get
            Set(ByVal value As Integer)
                m_iUserTags = value

            End Set
        End Property

        Public Property Longitude() As Double
            Get
                Return m_dLongitude

            End Get
            Set(ByVal value As Double)
                m_dLongitude = value
            End Set
        End Property

        Public Property Latitude() As Double
            Get
                Return m_dLatitude

            End Get
            Set(ByVal value As Double)
                m_dLatitude = value

            End Set
        End Property

        Public Property ShowMap() As Boolean
            Get
                Return m_bShowMap

            End Get
            Set(ByVal value As Boolean)
                m_bShowMap = value

            End Set
        End Property

        Public Property CameraMake() As String
            Get
                Return m_strCameraMake

            End Get
            Set(ByVal value As String)
                m_strCameraMake = value

            End Set
        End Property

        Public Property CameraModel() As String
            Get
                Return m_strCameraModel

            End Get
            Set(ByVal value As String)
                m_strCameraModel = value

            End Set
        End Property

        Public Overrides Function GetDirectURL() As String
            GetDirectURL = TCF_DIRECT_URL & TCPF_PHOTO_STRING & "?id=" & Me.ID & "&size=original"
        End Function

        Public Overrides Function GetExtension() As String
            Return TCPF_PHOTO_EXTENSION

        End Function

        Public ReadOnly Property LargeURL() As String
            Get
                Return TCF_DIRECT_URL & TCPF_PHOTO_STRING & "?id=" & Me.ID & "&size=large"
            End Get
        End Property

        Public ReadOnly Property SmallURL() As String
            Get
                Return TCF_DIRECT_URL & TCPF_PHOTO_STRING & "?id=" & Me.ID & "&size=small"
            End Get
        End Property

        Public ReadOnly Property MediumURL() As String
            Get
                Return TCF_DIRECT_URL & TCPF_PHOTO_STRING & "?id=" & Me.ID & "&size=medium"
            End Get
        End Property

        Public ReadOnly Property SquareURL() As String
            Get
                Return TCF_DIRECT_URL & TCPF_PHOTO_STRING & "?id=" & Me.ID & "&size=square"
            End Get
        End Property

        Public ReadOnly Property TileURL() As String
            Get
                Return TCF_DIRECT_URL & TCPF_PHOTO_STRING & "?id=" & Me.ID & "&size=tile"
            End Get
        End Property

        Public ReadOnly Property LargestURL() As String
            Get
                Return TCF_DIRECT_URL & TCPF_PHOTO_STRING & "?id=" & Me.ID & "&size=largest_available"
            End Get
        End Property

    End Class

End Namespace