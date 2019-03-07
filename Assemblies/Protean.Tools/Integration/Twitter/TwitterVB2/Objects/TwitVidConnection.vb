Imports System.Net
Imports System.Web
Imports System.IO
Imports System.Text

Imports System.Text.RegularExpressions
Imports System.Collections.Specialized
Imports System.Xml


Namespace Integration.Twitter.TwitterVB2
    Public Structure AGeoCode
        Public Latitude As String
        Public Longitude As String
    End Structure
    Public Class TwitVidConnection
        Private m_strUsername As String = String.Empty
        Private m_strPassword As String = String.Empty
        Private m_strOauth As String = String.Empty
        Private m_dtTL As DateTime
        Public ReadOnly Property oAuthToken() As String
            Get
                Return m_strOauth
            End Get
        End Property

        Public ReadOnly Property DateToLive() As DateTime
            Get
                Return m_dtTL
            End Get
        End Property
#Region "TwitVidConstants"
        Const TWITVID_AUTH_URL As String = "https://im.twitvid.com/api/authenticate"
        Const TWITVID_UPLOAD_URL As String = "http://im.twitvid.com/api/upload"
#End Region

        Private Sub Authenticate()
            Try
                Dim tvAuthRequest As HttpWebRequest
                Dim tvAuthResponse As HttpWebResponse

                Dim strPostData As String
                Dim bCredentials() As Byte

                Dim enCredentials As New System.Text.ASCIIEncoding()

                strPostData = "username=" & m_strUsername & "&password=" & m_strPassword

                bCredentials = enCredentials.GetBytes(strPostData)
                tvAuthRequest = CType(System.Net.WebRequest.Create(TWITVID_AUTH_URL), HttpWebRequest)
                tvAuthRequest.Method = "POST"
                tvAuthRequest.ContentType = "application/x-www-form-urlencoded"
                tvAuthRequest.ContentLength = strPostData.Length

                Using rwAuthenticate As New StreamWriter(tvAuthRequest.GetRequestStream())
                    rwAuthenticate.Write(strPostData)
                End Using
                tvAuthResponse = CType(tvAuthRequest.GetResponse(), HttpWebResponse)
                Dim srAuth As New StreamReader(tvAuthResponse.GetResponseStream())
                Dim strResponse As String = srAuth.ReadToEnd()
                Dim xResponseAuth As New XmlDocument
                xResponseAuth.LoadXml(strResponse)
                Dim xnRsp As XmlNode = xResponseAuth.SelectSingleNode("//rsp")
                If xnRsp.Attributes("stat").InnerText = "ok" Then
                    m_strOauth = xnRsp.SelectSingleNode("//token").InnerText
                    m_dtTL = DateTime.Now.AddMinutes(360)

                End If



            Catch ex As Exception

            End Try
        End Sub
        Public Sub New(ByVal p_strUserName As String, ByVal p_strPassword As String)
            m_strUsername = p_strUserName
            m_strPassword = p_strPassword
            Me.Authenticate()
        End Sub
        ''' <summary>
        ''' uploads a video to the TwitVid service (without tweeting it)
        ''' </summary>
        ''' <param name="p_strFileName">FileName to upload</param>
        ''' <param name="p_strMessage">Message to include</param>
        ''' <returns>The MediaID for the file</returns>
        ''' <remarks>This doesn't actually tweet out the video. The message is embedded in the video as metadata</remarks>
        Public Function Upload(ByVal p_strFileName As String, ByVal p_strMessage As String) As String
            Upload = String.Empty

            If DateTime.Now > m_dtTL Then
                Me.Authenticate()
            End If
            Try
                Dim bMovieFile() As Byte = System.IO.File.ReadAllBytes(p_strFileName)
                Dim strBoundary As String = Guid.NewGuid.ToString()
                Dim strHeader As String = String.Format("--{0}", strBoundary)
                Dim strFooter As String = String.Format("--{0}--", strBoundary)
                Dim rqUpload As HttpWebRequest = DirectCast(System.Net.WebRequest.Create(TWITVID_UPLOAD_URL), HttpWebRequest)
                With rqUpload
                    .PreAuthenticate = True
                    .AllowWriteStreamBuffering = True
                    .ContentType = String.Format("multipart/form-data; boundary={0}", strBoundary)
                    .Method = "POST"
                End With
                Dim strFileType As String = "application/octet-stream"

                Dim strFileHeader As String = [String].Format("Content-Disposition: file; name=""{0}""; filename=""{1}""", "media", p_strFileName)
                Dim strFileData As String = Encoding.GetEncoding("iso-8859-1").GetString(bMovieFile)
                Dim strContents As New StringBuilder()
                With strContents
                    .AppendLine(strHeader)


                    .AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "token"))
                    .AppendLine()
                    .AppendLine(m_strOauth)
                    .AppendLine(strHeader)
                    .AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "message"))
                    .AppendLine()
                    .AppendLine(p_strMessage)
                    .AppendLine(strHeader)
                    .AppendLine(strFileHeader)
                    .AppendLine([String].Format("Content-Type: {0}", strFileType))
                    .AppendLine()
                    .AppendLine(strFileData)


                    .AppendLine(strFooter)
                End With

                Dim bContents() As Byte = Encoding.GetEncoding("iso-8859-1").GetBytes(strContents.ToString())
                rqUpload.ContentLength = bContents.Length

                Dim rqStreamFile As Stream = rqUpload.GetRequestStream()
                rqStreamFile.Write(bContents, 0, bContents.Length)
                Dim rspFileUpload As HttpWebResponse = DirectCast(rqUpload.GetResponse, HttpWebResponse)
                Dim rdrResponse As New StreamReader(rspFileUpload.GetResponseStream())
                Dim strResponse As String = rdrResponse.ReadToEnd()
                Dim xResponse As New XmlDocument
                xResponse.LoadXml(strResponse)
                Dim xnRSP As XmlNode = xResponse.SelectSingleNode("//rsp")
                If xnRSP.Attributes("stat").Value = "ok" Then
                    Upload = xnRSP.SelectSingleNode("//mediaurl").InnerText
                Else
                    Upload = strResponse

                End If

            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
            Return Upload

        End Function
    End Class

End Namespace
