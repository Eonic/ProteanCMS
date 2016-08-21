Namespace Integration.Twitter.TwitterVB2
    Public Class ATwiCliAudioFile
        Inherits ATwiCliFile
#Region "TCAF Constants"
        Public Const TCAF_AUDIO_STRING As String = "audio.mp3"
        Public Const TCAF_AUDIO_EXTENSION As String = ".mp3"
#End Region

        Public Overrides Function GetDirectURL() As String
            Return TCF_DIRECT_URL & TCAF_AUDIO_STRING & "?id=" & Me.ID
        End Function

        Public Overrides Function GetExtension() As String
            Return TCAF_AUDIO_EXTENSION


        End Function
    End Class


End Namespace