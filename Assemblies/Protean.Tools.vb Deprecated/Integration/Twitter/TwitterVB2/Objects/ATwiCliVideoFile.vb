Namespace Integration.Twitter.TwitterVB2
    Public Class ATwiCliVideoFile
        Inherits ATwiCliFile
#Region "TCVF Constants"
        Public Const TCVF_VIDEO_STRING As String = "video.flv"
        Public Const TCVF_VIDEO_EXTENSION As String = ".flv"
#End Region
        Private m_strThumbNailURL As String

        Public Property ThumbNailURL() As String
            Get
                Return m_strThumbNailURL

            End Get
            Set(ByVal value As String)
                m_strThumbNailURL = value
            End Set
        End Property


        Public Overrides Function GetDirectURL() As String
            Return TCF_DIRECT_URL & TCVF_VIDEO_STRING & "?id=" & Me.ID & "&size=original"
        End Function

        Public Overrides Function GetExtension() As String
            Return TCVF_VIDEO_EXTENSION

        End Function
    End Class

End Namespace