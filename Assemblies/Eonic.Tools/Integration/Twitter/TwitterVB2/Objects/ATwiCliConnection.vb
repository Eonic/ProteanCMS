Imports System.Net
Imports System.Web
Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic
Imports System.Security.Permissions


Imports System.Text.RegularExpressions
Imports System.Collections.Specialized
Imports System.Xml


Namespace Integration.Twitter.TwitterVB2
    Public Class ATwiCliConnection

#Region "TCC Constants"
        Const TCC_UPLOAD_PHOTO_URL As String = "http://twic.li/api/uploadPhoto"
        Const TCC_UPLOAD_PHOTO_AND_TWEET_URL As String = "http://twic.li/api/uploadPhotoAndTweet"
        Const TCC_UPLOAD_VIDEO_URL As String = "http://twic.li/api/uploadVideo"
        Const TCC_UPLOAD_VIDEO_AND_TWEET_URL As String = "http://twic.li/api/uploadVideoAndTweet"
        Const TCC_UPLOAD_AUDIO_URL As String = "http://twic.li/api/uploadAudio"
        Const TCC_UPLOAD_AUDIO_AND_TWEET_URL As String = "http://twic.li/api/uploadAudioAndTweet"
        Const TCC_GET_CONTENT_INFO As String = "http://twic.li/api/getContent"
        Const TCC_GET_USERS_CONTENT_URL As String = "http://twic.li/api/getUsersContent"
#End Region
        Private m_strUserName As String
        Private m_strPassword As String
        Private m_strAPIKey As String

        Public Property UserName() As String
            Get
                Return m_strUserName

            End Get
            Set(ByVal value As String)
                m_strUserName = value

            End Set
        End Property

        Public Property Password() As String
            Get
                Return m_strPassword

            End Get
            Set(ByVal value As String)
                m_strPassword = value
            End Set
        End Property

        Public Property APIKey() As String
            Get
                Return m_strAPIKey

            End Get
            Set(ByVal value As String)
                m_strAPIKey = value

            End Set
        End Property





        Public Sub New(ByVal p_strUserName As String, ByVal p_strPassword As String, Optional ByVal p_strAPIKey As String = "<nokey>")
            Me.UserName = p_strUserName
            Me.Password = p_strPassword
            If p_strAPIKey <> "<nokey>" Then
                Me.APIKey = p_strAPIKey
            End If
        End Sub


        ''' <summary>
        ''' Uploads a Photo to the twicli service without posting a tweet
        ''' </summary>
        ''' <param name="p_strFileName">The filename of the picture to upload (note, must be a jpg, gif, or png)</param>
        ''' 
        ''' <param name="p_bShowMap">Whether or not to show a google map (EXIF geodata requried)</param>
        ''' <returns>An object representing the photograph now stored on the twic.li server</returns>
        ''' <remarks></remarks>
        Public Function UploadPhoto(ByVal p_strFileName As String, Optional ByVal p_bShowMap As Boolean = False) As ATwiCliPhotoFile
            UploadPhoto = New ATwiCliPhotoFile


            Try
                Dim strFileType As String
                If p_strFileName.ToLower.EndsWith(".jpg") Or p_strFileName.ToLower.EndsWith(".jpeg") Then
                    strFileType = "image/jpeg"
                ElseIf p_strFileName.ToLower.EndsWith(".gif") Then
                    strFileType = "image/gif"
                ElseIf p_strFileName.ToLower.EndsWith(".png") Then
                    strFileType = "image/png"
                Else
                    Throw New Exception("Invalid photo file")

                End If


                Dim bPhoto() As Byte = System.IO.File.ReadAllBytes(p_strFileName)
                Dim strBoundary As String = Guid.NewGuid.ToString()
                Dim strHeader As String = String.Format("--{0}", strBoundary)
                Dim strFooter As String = String.Format("--{0}--", strBoundary)
                Dim rqUploadPhoto As HttpWebRequest = DirectCast(System.Net.WebRequest.Create(TCC_UPLOAD_PHOTO_URL), HttpWebRequest)
                Dim strFileHeader As String = [String].Format("Content-Disposition: file; name=""{0}""; filename=""{1}""", "photo", p_strFileName)
                Dim strFileData As String = Encoding.GetEncoding("iso-8859-1").GetString(bPhoto)
                Dim sbContents As New StringBuilder()
                With rqUploadPhoto
                    .PreAuthenticate = True
                    .AllowWriteStreamBuffering = True
                    .ContentType = String.Format("multipart/form-data; boundary={0}", strBoundary)
                    .Method = "POST"
                End With
                With sbContents
                    .AppendLine(strHeader)
                    .AppendLine(strFileHeader)
                    .AppendLine([String].Format("Content-Type: {0}", strFileType))
                    .AppendLine()
                    .AppendLine(strFileData)
                    .AppendLine(strHeader)
                    .AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "username"))
                    .AppendLine()
                    .AppendLine(Me.UserName)
                    .AppendLine(strHeader)
                    .AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "password"))
                    .AppendLine()
                    .AppendLine(Me.Password)
                    .AppendLine(strHeader)
                    .AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "show_map"))
                    .AppendLine()
                    If p_bShowMap = True Then
                        .AppendLine("1")
                    Else
                        .AppendLine("0")
                    End If
                    If Me.APIKey.Length > 0 Then
                        .AppendLine(strHeader)
                        .AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "api_key"))
                        .AppendLine()
                        .AppendLine(Me.APIKey)
                    End If
                    .AppendLine(strFooter)
                End With

                Dim bFormData() As Byte = Encoding.GetEncoding("iso-8859-1").GetBytes(sbContents.ToString())
                rqUploadPhoto.ContentLength = bFormData.Length
                Dim stPhotoRequest As Stream = rqUploadPhoto.GetRequestStream()
                stPhotoRequest.Write(bFormData, 0, bFormData.Length)
                Dim rpUploadPhoto As HttpWebResponse = DirectCast(rqUploadPhoto.GetResponse, HttpWebResponse)
                Dim srUploadPhoto As New StreamReader(rpUploadPhoto.GetResponseStream())
                Dim strUploadResult As String = srUploadPhoto.ReadToEnd()
                Dim xUploadResult As New XmlDocument
                xUploadResult.LoadXml(strUploadResult)
                Dim xnResponse As XmlNode = xUploadResult.SelectSingleNode("//response")
                If xnResponse.Attributes("status").InnerText = "ok" Then
                    'UploadPhoto = xnResponse.SelectSingleNode("//url").InnerText
                    UploadPhoto.ID = xnResponse.SelectSingleNode("//id").InnerText
                    UploadPhoto.ScreenName = xnResponse.SelectSingleNode("//screen_name").InnerText
                    UploadPhoto.UserID = xnResponse.SelectSingleNode("//user_id").InnerText
                    UploadPhoto.URL = xnResponse.SelectSingleNode("//url").InnerText
                    UploadPhoto.Comments_Count = CInt(xnResponse.SelectSingleNode("//num_comments").InnerText)
                    UploadPhoto.UserTags_Count = CInt(xnResponse.SelectSingleNode("//num_user_tags").InnerText)
                    UploadPhoto.Views_Count = CInt(xnResponse.SelectSingleNode("//num_views").InnerText)
                    If p_bShowMap Then
                        UploadPhoto.ShowMap = True
                        UploadPhoto.Latitude = CDbl(xnResponse.SelectSingleNode("//latitude").InnerText)
                        UploadPhoto.Longitude = CDbl(xnResponse.SelectSingleNode("//longitude").InnerText)
                    Else
                        UploadPhoto.ShowMap = False
                    End If
                    UploadPhoto.CameraMake = xnResponse.SelectSingleNode("//camera_make").InnerText
                    UploadPhoto.CameraModel = xnResponse.SelectSingleNode("//camera_model").InnerText
                    UploadPhoto.InsertUnixTime(CDbl(xnResponse.SelectSingleNode("//timestamp").InnerText))



                Else
                    'UploadPhoto = xnResponse.SelectSingleNode("//error_text").InnerText
                    Throw New Exception(xnResponse.SelectSingleNode("//error_text").InnerText)
                End If
                Return UploadPhoto

            Catch ex As Exception

            End Try
            Return UploadPhoto
        End Function
        ''' <summary>
        ''' Uploads a photo AND updates the users status
        ''' </summary>
        ''' <param name="p_strFileName">File name of the picture</param>
        ''' <param name="p_strTweet">status text</param>
        ''' <param name="p_bShowMap">Whehter or not to show a google map beside the photo (EXIF data must be encoded if this option is selected)</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function UploadPhotoAndTweet(ByVal p_strFileName As String, ByVal p_strTweet As String, Optional ByVal p_bShowMap As Boolean = True) As ATwiCliPhotoFile
            UploadPhotoAndTweet = New ATwiCliPhotoFile
            Try
                Dim strFileType As String
                If p_strFileName.ToLower.EndsWith(".jpg") Or p_strFileName.ToLower.EndsWith(".jpeg") Then
                    strFileType = "image/jpeg"
                ElseIf p_strFileName.ToLower.EndsWith(".gif") Then
                    strFileType = "image/gif"
                ElseIf p_strFileName.ToLower.EndsWith(".png") Then
                    strFileType = "image/png"
                Else
                    'Return String.Empty
                    Throw New Exception("Invalid file format")

                End If


                Dim bPhoto() As Byte = System.IO.File.ReadAllBytes(p_strFileName)
                Dim strBoundary As String = Guid.NewGuid.ToString()
                Dim strHeader As String = String.Format("--{0}", strBoundary)
                Dim strFooter As String = String.Format("--{0}--", strBoundary)
                Dim rqUploadPhoto As HttpWebRequest = DirectCast(System.Net.WebRequest.Create(TCC_UPLOAD_PHOTO_AND_TWEET_URL), HttpWebRequest)
                Dim strFileHeader As String = [String].Format("Content-Disposition: file; name=""{0}""; filename=""{1}""", "photo", p_strFileName)
                Dim strFileData As String = Encoding.GetEncoding("iso-8859-1").GetString(bPhoto)
                Dim sbContents As New StringBuilder()
                With rqUploadPhoto
                    .PreAuthenticate = True
                    .AllowWriteStreamBuffering = True
                    .ContentType = String.Format("multipart/form-data; boundary={0}", strBoundary)
                    .Method = "POST"
                End With
                With sbContents
                    .AppendLine(strHeader)
                    .AppendLine(strFileHeader)
                    .AppendLine([String].Format("Content-Type: {0}", strFileType))
                    .AppendLine()
                    .AppendLine(strFileData)
                    .AppendLine(strHeader)
                    .AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "username"))
                    .AppendLine()
                    .AppendLine(Me.UserName)
                    .AppendLine(strHeader)
                    .AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "password"))
                    .AppendLine()
                    .AppendLine(Me.Password)
                    .AppendLine(strHeader)
                    .AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "tweet"))
                    .AppendLine()
                    .AppendLine(p_strTweet)
                    .AppendLine(strHeader)
                    .AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "show_map"))
                    .AppendLine()
                    If p_bShowMap = True Then
                        .AppendLine("1")
                    Else
                        .AppendLine("0")
                    End If
                    If Me.APIKey.Length > 0 Then
                        .AppendLine(strHeader)
                        .AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "api_key"))
                        .AppendLine()
                        .AppendLine(Me.APIKey)
                    End If
                    .AppendLine(strFooter)
                End With

                Dim bFormData() As Byte = Encoding.GetEncoding("iso-8859-1").GetBytes(sbContents.ToString())
                rqUploadPhoto.ContentLength = bFormData.Length
                Dim stPhotoRequest As Stream = rqUploadPhoto.GetRequestStream()
                stPhotoRequest.Write(bFormData, 0, bFormData.Length)
                Dim rpUploadPhoto As HttpWebResponse = DirectCast(rqUploadPhoto.GetResponse, HttpWebResponse)
                Dim srUploadPhoto As New StreamReader(rpUploadPhoto.GetResponseStream())
                Dim strUploadResult As String = srUploadPhoto.ReadToEnd()
                Dim xUploadResult As New XmlDocument
                xUploadResult.LoadXml(strUploadResult)
                Dim xnResponse As XmlNode = xUploadResult.SelectSingleNode("//response")
                If xnResponse.Attributes("status").InnerText = "ok" Then
                    'UploadPhotoAndTweet = xnResponse.SelectSingleNode("//url").InnerText
                    UploadPhotoAndTweet.ID = xnResponse.SelectSingleNode("//id").InnerText
                    UploadPhotoAndTweet.ScreenName = xnResponse.SelectSingleNode("//screen_name").InnerText
                    UploadPhotoAndTweet.UserID = xnResponse.SelectSingleNode("//user_id").InnerText
                    UploadPhotoAndTweet.URL = xnResponse.SelectSingleNode("//url").InnerText
                    UploadPhotoAndTweet.Comments_Count = CInt(xnResponse.SelectSingleNode("//num_comments").InnerText)
                    UploadPhotoAndTweet.UserTags_Count = CInt(xnResponse.SelectSingleNode("//num_user_tags").InnerText)
                    UploadPhotoAndTweet.Views_Count = CInt(xnResponse.SelectSingleNode("//num_views").InnerText)
                    If p_bShowMap Then
                        UploadPhotoAndTweet.ShowMap = True
                        UploadPhotoAndTweet.Latitude = CDbl(xnResponse.SelectSingleNode("//latitude").InnerText)
                        UploadPhotoAndTweet.Longitude = CDbl(xnResponse.SelectSingleNode("//longitude").InnerText)
                    Else
                        UploadPhotoAndTweet.ShowMap = False
                    End If
                    UploadPhotoAndTweet.CameraMake = xnResponse.SelectSingleNode("//camera_make").InnerText
                    UploadPhotoAndTweet.CameraModel = xnResponse.SelectSingleNode("//camera_model").InnerText
                    UploadPhotoAndTweet.InsertUnixTime(CDbl(xnResponse.SelectSingleNode("//timestamp").InnerText))
                Else
                    'UploadPhotoAndTweet = xnResponse.SelectSingleNode("//error_text").InnerText
                    Throw New Exception(xnResponse.SelectSingleNode("//error_text").InnerText)


                End If



            Catch ex As Exception

            End Try
            Return UploadPhotoAndTweet
        End Function

        Public Function UploadVideo(ByVal p_strFileName As String, Optional ByVal p_bShowMap As Boolean = False) As ATwiCliVideoFile
            UploadVideo = New ATwiCliVideoFile

            Try
                Dim strFileType As String
                If p_strFileName.ToLower.EndsWith(".mpg") Or p_strFileName.ToLower.EndsWith(".mpeg") Then
                    strFileType = "video/mpeg"
                ElseIf p_strFileName.ToLower.EndsWith(".3gp") Then
                    strFileType = "video/3gpp"
                ElseIf p_strFileName.ToLower.EndsWith(".wmv") Then
                    strFileType = "application/octet-stream"
                ElseIf p_strFileName.ToLower.EndsWith(".mp4") Then
                    strFileType = "application/octet-stream"
                ElseIf p_strFileName.ToLower.EndsWith(".mov") Then
                    strFileType = "application/octet-stream"
                ElseIf p_strFileName.ToLower.EndsWith(".flv") Then
                    strFileType = "applicaiton/octet-stream"
                Else
                    Throw New Exception("Invalid video format")

                End If


                Dim bVideo() As Byte = System.IO.File.ReadAllBytes(p_strFileName)
                Dim strBoundary As String = Guid.NewGuid.ToString()
                Dim strHeader As String = String.Format("--{0}", strBoundary)
                Dim strFooter As String = String.Format("--{0}--", strBoundary)
                Dim rqUploadVideo As HttpWebRequest = DirectCast(System.Net.WebRequest.Create(TCC_UPLOAD_VIDEO_URL), HttpWebRequest)
                Dim strFileHeader As String = [String].Format("Content-Disposition: file; name=""{0}""; filename=""{1}""", "video", p_strFileName)
                Dim strFileData As String = Encoding.GetEncoding("iso-8859-1").GetString(bVideo)
                Dim sbContents As New StringBuilder()
                With rqUploadVideo
                    .PreAuthenticate = True
                    .AllowWriteStreamBuffering = True
                    .ContentType = String.Format("multipart/form-data; boundary={0}", strBoundary)
                    .Method = "POST"
                End With
                With sbContents
                    .AppendLine(strHeader)
                    .AppendLine(strFileHeader)
                    .AppendLine([String].Format("Content-Type: {0}", strFileType))
                    .AppendLine()
                    .AppendLine(strFileData)
                    .AppendLine(strHeader)
                    .AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "username"))
                    .AppendLine()
                    .AppendLine(Me.UserName)
                    .AppendLine(strHeader)
                    .AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "password"))
                    .AppendLine()
                    .AppendLine(Me.Password)
                    .AppendLine(strHeader)
                    .AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "show_map"))
                    .AppendLine()
                    If p_bShowMap = True Then
                        .AppendLine("1")
                    Else
                        .AppendLine("0")
                    End If
                    If Me.APIKey.Length > 0 Then
                        .AppendLine(strHeader)
                        .AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "api_key"))
                        .AppendLine()
                        .AppendLine(Me.APIKey)
                    End If
                    .AppendLine(strFooter)
                End With

                Dim bFormData() As Byte = Encoding.GetEncoding("iso-8859-1").GetBytes(sbContents.ToString())
                rqUploadVideo.ContentLength = bFormData.Length
                Dim stVideoRequest As Stream = rqUploadVideo.GetRequestStream()
                stVideoRequest.Write(bFormData, 0, bFormData.Length)
                Dim rpUploadVideo As HttpWebResponse = DirectCast(rqUploadVideo.GetResponse, HttpWebResponse)
                Dim srUploadVideo As New StreamReader(rpUploadVideo.GetResponseStream())
                Dim strUploadResult As String = srUploadVideo.ReadToEnd()
                Dim xUploadResult As New XmlDocument
                xUploadResult.LoadXml(strUploadResult)
                Dim xnResponse As XmlNode = xUploadResult.SelectSingleNode("//response")
                If xnResponse.Attributes("status").InnerText = "ok" Then
                    'UploadVideo = xnResponse.SelectSingleNode("//url").InnerText
                    UploadVideo.ID = xnResponse.SelectSingleNode("//id").InnerText
                    UploadVideo.ScreenName = xnResponse.SelectSingleNode("//screen_name").InnerText
                    UploadVideo.UserID = xnResponse.SelectSingleNode("//user_id").InnerText

                    UploadVideo.URL = xnResponse.SelectSingleNode("//url").InnerText
                    UploadVideo.Comments_Count = CInt(xnResponse.SelectSingleNode("//num_comments").InnerText)
                    UploadVideo.Views_Count = CInt(xnResponse.SelectSingleNode("//num_views").InnerText)
                    UploadVideo.InsertUnixTime(CDbl(xnResponse.SelectSingleNode("//timestamp").InnerText))
                    UploadVideo.ThumbNailURL = xnResponse.SelectSingleNode("//thumb_image_url").InnerText

                Else
                    Throw New Exception(xnResponse.SelectSingleNode("//error_text").InnerText)
                    'UploadVideo = xnResponse.SelectSingleNode("//error_text").InnerText
                End If



            Catch ex As Exception

            End Try
            Return UploadVideo
        End Function

        ''' <summary>
        ''' Uploads a video and updates the user's status
        ''' </summary>
        ''' <param name="p_strFileName">The file name to be uploaded</param>
        ''' <param name="p_strTweet">Tweet that should be uploaded along with the file. If empty just the URL is posted</param>
        ''' <param name="p_bShowMap">Optional to show a google map along with the image.  Note if this is used the EXIF data must be present in the file</param>
        ''' <returns>An ATwiCliVideo file containing information regarding the file</returns>
        ''' <remarks></remarks>
        Public Function UploadVideoAndTweet(ByVal p_strFileName As String, ByVal p_strTweet As String, Optional ByVal p_bShowMap As Boolean = False) As ATwiCliVideoFile
            UploadVideoAndTweet = New ATwiCliVideoFile

            Try
                Dim strFileType As String
                If p_strFileName.ToLower.EndsWith(".mpg") Or p_strFileName.ToLower.EndsWith(".mpeg") Then
                    strFileType = "video/mpeg"
                ElseIf p_strFileName.ToLower.EndsWith(".3gp") Then
                    strFileType = "video/3gpp"
                ElseIf p_strFileName.ToLower.EndsWith(".wmv") Then
                    strFileType = "application/octet-stream"
                ElseIf p_strFileName.ToLower.EndsWith(".mp4") Then
                    strFileType = "application/octet-stream"
                ElseIf p_strFileName.ToLower.EndsWith(".mov") Then
                    strFileType = "application/octet-stream"
                ElseIf p_strFileName.ToLower.EndsWith(".flv") Then
                    strFileType = "application/octet-stream"
                Else
                    Throw New Exception("Invalid file format")

                End If


                Dim bVideo() As Byte = System.IO.File.ReadAllBytes(p_strFileName)
                Dim strBoundary As String = Guid.NewGuid.ToString()
                Dim strHeader As String = String.Format("--{0}", strBoundary)
                Dim strFooter As String = String.Format("--{0}--", strBoundary)
                Dim rqUploadVideo As HttpWebRequest = DirectCast(System.Net.WebRequest.Create(TCC_UPLOAD_VIDEO_AND_TWEET_URL), HttpWebRequest)
                Dim strFileHeader As String = [String].Format("Content-Disposition: file; name=""{0}""; filename=""{1}""", "video", p_strFileName)
                Dim strFileData As String = Encoding.GetEncoding("iso-8859-1").GetString(bVideo)
                Dim sbContents As New StringBuilder()
                With rqUploadVideo
                    .PreAuthenticate = True
                    .AllowWriteStreamBuffering = True
                    .ContentType = String.Format("multipart/form-data; boundary={0}", strBoundary)
                    .Method = "POST"
                End With
                With sbContents
                    .AppendLine(strHeader)
                    .AppendLine(strFileHeader)
                    .AppendLine([String].Format("Content-Type: {0}", strFileType))
                    .AppendLine()
                    .AppendLine(strFileData)
                    .AppendLine(strHeader)
                    .AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "username"))
                    .AppendLine()
                    .AppendLine(Me.UserName)
                    .AppendLine(strHeader)
                    .AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "password"))
                    .AppendLine()
                    .AppendLine(Me.Password)
                    .AppendLine(strHeader)
                    .AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "tweet"))
                    .AppendLine()
                    .AppendLine(p_strTweet)
                    .AppendLine(strHeader)
                    .AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "show_map"))
                    .AppendLine()
                    If p_bShowMap = True Then
                        .AppendLine("1")
                    Else
                        .AppendLine("0")
                    End If
                    If Me.APIKey.Length > 0 Then
                        .AppendLine(strHeader)
                        .AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "api_key"))
                        .AppendLine()
                        .AppendLine(Me.APIKey)
                    End If
                    .AppendLine(strFooter)
                End With

                Dim bFormData() As Byte = Encoding.GetEncoding("iso-8859-1").GetBytes(sbContents.ToString())
                rqUploadVideo.ContentLength = bFormData.Length
                Dim stVideoRequest As Stream = rqUploadVideo.GetRequestStream()
                stVideoRequest.Write(bFormData, 0, bFormData.Length)
                Dim rpUploadVideo As HttpWebResponse = DirectCast(rqUploadVideo.GetResponse, HttpWebResponse)
                Dim srUploadVideo As New StreamReader(rpUploadVideo.GetResponseStream())
                Dim strUploadResult As String = srUploadVideo.ReadToEnd()
                Dim xUploadResult As New XmlDocument
                xUploadResult.LoadXml(strUploadResult)
                Dim xnResponse As XmlNode = xUploadResult.SelectSingleNode("//response")
                If xnResponse.Attributes("status").InnerText = "ok" Then
                    'UploadVideoAndTweet = xnResponse.SelectSingleNode("//url").InnerText
                    UploadVideoAndTweet.ID = xnResponse.SelectSingleNode("//id").InnerText
                    UploadVideoAndTweet.URL = xnResponse.SelectSingleNode("//url").InnerText
                    UploadVideoAndTweet.UserID = xnResponse.SelectSingleNode("//user_id").InnerText
                    UploadVideoAndTweet.ScreenName = xnResponse.SelectSingleNode("//screen_name").InnerText
                    UploadVideoAndTweet.ThumbNailURL = xnResponse.SelectSingleNode("//thumb_image_url").InnerText
                    UploadVideoAndTweet.Comments_Count = CInt(xnResponse.SelectSingleNode("//num_comments").InnerText)
                    UploadVideoAndTweet.Views_Count = CInt(xnResponse.SelectSingleNode("//num_views").InnerText)

                Else
                    'UploadVideoAndTweet = xnResponse.SelectSingleNode("//error_text").InnerText
                End If
                Return UploadVideoAndTweet



            Catch ex As Exception

            End Try
        End Function
        ''' <summary>
        ''' Uploads an audiofile to Twic.li and puts it in your my music collection
        ''' </summary>
        ''' <param name="p_strFileName">The file to send (must be a .mp3, .m4a, or .wav file less than 50 MB</param>
        ''' <returns>An ATwiAudioFile with the response back</returns>
        ''' <remarks></remarks>
        Public Function UploadAudio(ByVal p_strFileName As String) As ATwiCliAudioFile
            UploadAudio = New ATwiCliAudioFile
            Dim strContentType As String


            If p_strFileName.ToLower.EndsWith(".mp3") Or p_strFileName.ToLower.EndsWith(".m4a") Or p_strFileName.ToLower.EndsWith(".wav") Then
                strContentType = "application/octet-stream"
            Else
                Throw New Exception("invalid file format")
            End If

            Try
                Dim bSong() As Byte = System.IO.File.ReadAllBytes(p_strFileName)
                Dim strBoundary As String = Guid.NewGuid.ToString()
                Dim strHeader As String = String.Format("--{0}", strBoundary)
                Dim strFooter As String = String.Format("--{0}--", strBoundary)
                Dim rqUploadAudio As HttpWebRequest = DirectCast(System.Net.WebRequest.Create(TCC_UPLOAD_AUDIO_URL), HttpWebRequest)
                Dim strFileHeader As String = [String].Format("Content-disposition: file; name=""{0}"";filename=""{1}""", "audio", p_strFileName)
                Dim strFileData As String = Encoding.GetEncoding("iso-8859-1").GetString(bSong)
                Dim sbContents As New StringBuilder
                With rqUploadAudio
                    .PreAuthenticate = True
                    .Method = "POST"
                    .AllowWriteStreamBuffering = True
                    .ContentType = [String].Format("multipart/form-data; boundary={0}", strBoundary)
                End With
                With sbContents
                    .AppendLine(strHeader)
                    .AppendLine(strFileHeader)
                    .AppendLine([String].Format("Content-Type: {0}", strContentType))
                    .AppendLine()
                    .AppendLine(strFileData)
                    .AppendLine(strHeader)
                    .AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "username"))
                    .AppendLine()
                    .AppendLine(Me.UserName)
                    .AppendLine(strHeader)
                    .AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "password"))
                    .AppendLine()
                    .AppendLine(Me.Password)
                    If Me.APIKey.Length > 0 Then
                        .AppendLine(strHeader)
                        .AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "api_key"))
                        .AppendLine()
                        .AppendLine(Me.APIKey)
                    End If
                    .AppendLine(strFooter)
                End With
                Dim bUploadData() As Byte = Encoding.GetEncoding("iso-8859-1").GetBytes(sbContents.ToString())
                rqUploadAudio.ContentLength = bUploadData.Length()
                Dim stAudioRequest As Stream = rqUploadAudio.GetRequestStream()
                stAudioRequest.Write(bUploadData, 0, bUploadData.Length)
                Dim rsAudioupload As HttpWebResponse = DirectCast(rqUploadAudio.GetResponse(), HttpWebResponse)
                Dim srAudioData As New StreamReader(rsAudioupload.GetResponseStream)
                Dim strAudioData As String
                strAudioData = srAudioData.ReadToEnd()
                Dim xUploadresult As New XmlDocument
                xUploadresult.LoadXml(strAudioData)
                Dim xnResult As XmlNode = xUploadresult.SelectSingleNode("//response")
                If xnResult.Attributes("status").InnerText = "ok" Then
                    UploadAudio.ID = xnResult.SelectSingleNode("//id").InnerText
                    UploadAudio.ScreenName = xnResult.SelectSingleNode("//screen_name").InnerText
                    UploadAudio.UserID = xnResult.SelectSingleNode("//user_id").InnerText
                    UploadAudio.URL = xnResult.SelectSingleNode("//url").InnerText
                    UploadAudio.Views_Count = CInt(xnResult.SelectSingleNode("//num_views").InnerText)
                    UploadAudio.Comments_Count = CInt(xnResult.SelectSingleNode("//num_comments").InnerText)
                    UploadAudio.InsertUnixTime(CDbl(xnResult.SelectSingleNode("//timestamp").InnerText))
                Else
                    Throw New Exception(xnResult.SelectSingleNode("//error_text").InnerText)
                End If
                Return UploadAudio

            Catch ex As Exception
                Throw New Exception(ex.Message)

            End Try

        End Function
        ''' <summary>
        ''' uploads an audio file and updates the user's status
        ''' </summary>
        ''' <param name="p_strFileName">File name to upload (must be .mp3, .m4a, or .wav and no larger than 50MB</param>
        ''' <param name="p_strTweet">Text of the tweet.  Keep down to 120 characters or less as this automatically appends the url</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function UploadAudioAndTweet(ByVal p_strFileName As String, ByVal p_strTweet As String) As ATwiCliAudioFile
            UploadAudioAndTweet = New ATwiCliAudioFile
            Dim strContentType As String


            If p_strFileName.ToLower.EndsWith(".mp3") Or p_strFileName.ToLower.EndsWith(".m4a") Or p_strFileName.ToLower.EndsWith(".wav") Then
                strContentType = "application/octet-stream"
            Else
                Throw New Exception("invalid file format")
            End If

            Try
                Dim bSong() As Byte = System.IO.File.ReadAllBytes(p_strFileName)
                Dim strBoundary As String = Guid.NewGuid.ToString()
                Dim strHeader As String = String.Format("--{0}", strBoundary)
                Dim strFooter As String = String.Format("--{0}--", strBoundary)
                Dim rqUploadAudio As HttpWebRequest = DirectCast(System.Net.WebRequest.Create(TCC_UPLOAD_AUDIO_AND_TWEET_URL), HttpWebRequest)
                Dim strFileHeader As String = [String].Format("Content-disposition: file; name=""{0}"";filename=""{1}""", "audio", p_strFileName)
                Dim strFileData As String = Encoding.GetEncoding("iso-8859-1").GetString(bSong)
                Dim sbContents As New StringBuilder
                With rqUploadAudio
                    .PreAuthenticate = True
                    .Method = "POST"
                    .AllowWriteStreamBuffering = True
                    .ContentType = [String].Format("multipart/form-data; boundary={0}", strBoundary)
                End With
                With sbContents
                    .AppendLine(strHeader)
                    .AppendLine(strFileHeader)
                    .AppendLine([String].Format("Content-Type: {0}", strContentType))
                    .AppendLine()
                    .AppendLine(strFileData)
                    .AppendLine(strHeader)
                    .AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "username"))
                    .AppendLine()
                    .AppendLine(Me.UserName)
                    .AppendLine(strHeader)
                    .AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "password"))
                    .AppendLine()
                    .AppendLine(Me.Password)
                    .AppendLine(strHeader)
                    .AppendLine([String].Format("Content-Disposition: form_name; name=""{0}""", "tweet"))
                    .AppendLine()
                    .AppendLine(p_strTweet)
                    If Me.APIKey.Length > 0 Then
                        .AppendLine(strHeader)
                        .AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "api_key"))
                        .AppendLine()
                        .AppendLine(Me.APIKey)
                    End If
                    .AppendLine(strFooter)
                End With
                Dim bUploadData() As Byte = Encoding.GetEncoding("iso-8859-1").GetBytes(sbContents.ToString())
                rqUploadAudio.ContentLength = bUploadData.Length()
                Dim stAudioRequest As Stream = rqUploadAudio.GetRequestStream()
                stAudioRequest.Write(bUploadData, 0, bUploadData.Length)
                Dim rsAudioupload As HttpWebResponse = DirectCast(rqUploadAudio.GetResponse(), HttpWebResponse)
                Dim srAudioData As New StreamReader(rsAudioupload.GetResponseStream)
                Dim strAudioData As String
                strAudioData = srAudioData.ReadToEnd()
                Dim xUploadresult As New XmlDocument
                xUploadresult.LoadXml(strAudioData)
                Dim xnResult As XmlNode = xUploadresult.SelectSingleNode("//response")
                If xnResult.Attributes("status").InnerText = "ok" Then
                    UploadAudioAndTweet.ID = xnResult.SelectSingleNode("//id").InnerText
                    UploadAudioAndTweet.ScreenName = xnResult.SelectSingleNode("//screen_name").InnerText
                    UploadAudioAndTweet.UserID = xnResult.SelectSingleNode("//user_id").InnerText
                    UploadAudioAndTweet.URL = xnResult.SelectSingleNode("//url").InnerText
                    UploadAudioAndTweet.Views_Count = CInt(xnResult.SelectSingleNode("//num_views").InnerText)
                    UploadAudioAndTweet.Comments_Count = CInt(xnResult.SelectSingleNode("//num_comments").InnerText)
                    UploadAudioAndTweet.InsertUnixTime(CDbl(xnResult.SelectSingleNode("//timestamp").InnerText))
                Else
                    Throw New Exception(xnResult.SelectSingleNode("//error_text").InnerText)
                End If
                Return UploadAudioAndTweet


            Catch ex As Exception
                Throw New Exception(ex.Message)

            End Try
        End Function

        Public Function GetContentInfo(ByVal p_strID As String) As ATwiCliFile

            Dim ReturnValue As ATwiCliFile = Nothing

            Dim rqInfo As HttpWebRequest = DirectCast(System.Net.WebRequest.Create(TCC_GET_CONTENT_INFO & "?id=" & p_strID), HttpWebRequest)
            rqInfo.Method = "POST"
            Dim rsInfo As HttpWebResponse = DirectCast(rqInfo.GetResponse, HttpWebResponse)
            Dim srContent As New StreamReader(rsInfo.GetResponseStream())
            Dim strContent As String
            strContent = srContent.ReadToEnd()
            Dim xContent As New XmlDocument
            xContent.LoadXml(strContent)
            Dim xnResponse As XmlNode = xContent.SelectSingleNode("//response")
            If xnResponse.Attributes("status").InnerText = "ok" Then
                Dim xnContent As XmlNode = xnResponse.SelectSingleNode("//content")
                Select Case xnContent.Attributes("type").InnerText.ToLower()
                    Case "photo"
                        Dim Photo As New ATwiCliPhotoFile
                        Photo.ID = xnContent.SelectSingleNode("//id").InnerText
                        Photo.ScreenName = xnContent.SelectSingleNode("//screen_name").InnerText
                        Photo.UserID = xnContent.SelectSingleNode("//user_id").InnerText
                        Photo.URL = xnContent.SelectSingleNode("//url").InnerText
                        Photo.UserTags_Count = CInt(xnContent.SelectSingleNode("//num_user_tags").InnerText)
                        Photo.Comments_Count = CInt(xnContent.SelectSingleNode("//num_comments").InnerText)
                        Photo.Views_Count = CInt(xnContent.SelectSingleNode("//num_views").InnerText)
                        If xnContent.SelectSingleNode("//show_map").InnerText = "1" Then
                            Photo.ShowMap = True
                            Photo.Latitude = CDbl(xnContent.SelectSingleNode("//latitude").InnerText)
                            Photo.Longitude = CDbl(xnContent.SelectSingleNode("//longitude").InnerText)
                        Else
                            Photo.ShowMap = False
                        End If

                        Photo.CameraMake = xnContent.SelectSingleNode("//camera_make").InnerText
                        Photo.CameraModel = xnContent.SelectSingleNode("//camera_model").InnerText
                        Photo.InsertUnixTime(CDbl(xnContent.SelectSingleNode("//timestamp").InnerText))
                        ReturnValue = Photo

                    Case "audio"
                        Dim Audio As New ATwiCliAudioFile
                        Audio.ID = xnContent.SelectSingleNode("//id").InnerText
                        Audio.URL = xnContent.SelectSingleNode("//url").InnerText
                        Audio.UserID = xnContent.SelectSingleNode("//user_id").InnerText
                        Audio.ScreenName = xnContent.SelectSingleNode("//screen_name").InnerText
                        Audio.Comments_Count = CInt(xnContent.SelectSingleNode("//num_comments").InnerText)
                        Audio.Views_Count = CInt(xnContent.SelectSingleNode("//num_views").InnerText)
                        Audio.InsertUnixTime(CDbl(xnContent.SelectSingleNode("//timestamp").InnerText))
                        ReturnValue = Audio

                    Case "video"
                        Dim video As New ATwiCliVideoFile
                        video.ID = xnContent.SelectSingleNode("//id").InnerText
                        video.URL = xnContent.SelectSingleNode("//url").InnerText
                        video.UserID = xnContent.SelectSingleNode("//user_id").InnerText
                        video.ScreenName = xnContent.SelectSingleNode("//screen_name").InnerText
                        video.Comments_Count = CInt(xnContent.SelectSingleNode("//num_comments").InnerText)
                        video.Views_Count = CInt(xnContent.SelectSingleNode("//num_views").InnerText)
                        video.InsertUnixTime(CDbl(xnContent.SelectSingleNode("//timestamp").InnerText))
                        ReturnValue = video
                End Select
            Else
                Throw New Exception(xnResponse.SelectSingleNode("//error_text").InnerText)
            End If

            Return ReturnValue

        End Function
        Public Overloads Function GetUsersPhotos(ByVal p_ID As Int64) As List(Of ATwiCliPhotoFile)
            GetUsersPhotos = New List(Of ATwiCliPhotoFile)
            Dim strURL As String = TCC_GET_USERS_CONTENT_URL & "?userid=" & p_ID & "&content_type=photos"

            Try
                Dim rqUsersPhotos As HttpWebRequest = DirectCast(System.Net.WebRequest.Create(strURL), HttpWebRequest)
                rqUsersPhotos.Method = "GET"
                Dim rsUsersPhotos As HttpWebResponse = DirectCast(rqUsersPhotos.GetResponse, HttpWebResponse)
                Dim srUsersPhotos As New StreamReader(rsUsersPhotos.GetResponseStream)
                Dim strUsersPhotos As String
                strUsersPhotos = srUsersPhotos.ReadToEnd()
                Dim xdPhotos As New XmlDocument
                xdPhotos.LoadXml(strUsersPhotos)
                For Each xPhoto As XmlNode In xdPhotos.SelectNodes("//content")
                    Dim Photo As New ATwiCliPhotoFile
                    Photo.ID = xPhoto.SelectSingleNode("id").InnerText
                    Photo.UserID = xPhoto.SelectSingleNode("user_id").InnerText
                    Photo.ScreenName = xPhoto.SelectSingleNode("screen_name").InnerText
                    Photo.URL = xPhoto.SelectSingleNode("url").InnerText
                    Photo.Comments_Count = CInt(xPhoto.SelectSingleNode("num_comments").InnerText)
                    Photo.UserTags_Count = CInt(xPhoto.SelectSingleNode("num_user_tags").InnerText)
                    Photo.Views_Count = CInt(xPhoto.SelectSingleNode("num_views").InnerText)
                    If xPhoto.SelectSingleNode("show_map").InnerText = "1" Then
                        Photo.ShowMap = True
                        Photo.Latitude = CDbl(xPhoto.SelectSingleNode("latitude").InnerText)
                        Photo.Longitude = CDbl(xPhoto.SelectSingleNode("longitude").InnerText)
                    Else
                        Photo.ShowMap = False
                    End If
                    Photo.CameraMake = xPhoto.SelectSingleNode("camera_make").InnerText
                    Photo.CameraModel = xPhoto.SelectSingleNode("camera_model").InnerText
                    Photo.InsertUnixTime(CDbl(xPhoto.SelectSingleNode("timestamp").InnerText))
                    GetUsersPhotos.Add(Photo)

                Next
            Catch ex As Exception
                Throw New Exception(ex.Message)
            End Try
            Return GetUsersPhotos

        End Function
        Public Overloads Function GetUsersPhotos(ByVal p_strUserName As String) As List(Of ATwiCliPhotoFile)
            GetUsersPhotos = New List(Of ATwiCliPhotoFile)
            Dim strURL As String = TCC_GET_USERS_CONTENT_URL & "?username=" & p_strUserName & "&content_type=photos"

            Try
                Dim rqUsersPhotos As HttpWebRequest = DirectCast(System.Net.WebRequest.Create(strURL), HttpWebRequest)
                rqUsersPhotos.Method = "GET"
                Dim rsUsersPhotos As HttpWebResponse = DirectCast(rqUsersPhotos.GetResponse, HttpWebResponse)
                Dim srUsersPhotos As New StreamReader(rsUsersPhotos.GetResponseStream)
                Dim strUsersPhotos As String
                strUsersPhotos = srUsersPhotos.ReadToEnd()
                Dim xdPhotos As New XmlDocument
                xdPhotos.LoadXml(strUsersPhotos)
                For Each xPhoto As XmlNode In xdPhotos.SelectNodes("//content")
                    Dim Photo As New ATwiCliPhotoFile
                    Photo.ID = xPhoto.SelectSingleNode("id").InnerText
                    Photo.UserID = xPhoto.SelectSingleNode("user_id").InnerText
                    Photo.ScreenName = xPhoto.SelectSingleNode("screen_name").InnerText
                    Photo.URL = xPhoto.SelectSingleNode("url").InnerText
                    Photo.Comments_Count = CInt(xPhoto.SelectSingleNode("num_comments").InnerText)
                    Photo.UserTags_Count = CInt(xPhoto.SelectSingleNode("num_user_tags").InnerText)
                    Photo.Views_Count = CInt(xPhoto.SelectSingleNode("num_views").InnerText)
                    If xPhoto.SelectSingleNode("show_map").InnerText = "1" Then
                        Photo.ShowMap = True
                        Photo.Latitude = CDbl(xPhoto.SelectSingleNode("latitude").InnerText)
                        Photo.Longitude = CDbl(xPhoto.SelectSingleNode("longitude").InnerText)
                    Else
                        Photo.ShowMap = False
                    End If
                    Photo.CameraMake = xPhoto.SelectSingleNode("camera_make").InnerText
                    Photo.CameraModel = xPhoto.SelectSingleNode("camera_model").InnerText
                    Photo.InsertUnixTime(CDbl(xPhoto.SelectSingleNode("timestamp").InnerText))
                    GetUsersPhotos.Add(Photo)

                Next
            Catch ex As Exception
                Throw New Exception(ex.Message)
            End Try
            Return GetUsersPhotos
        End Function

        Public Overloads Function GetUsersAudios(ByVal p_ID As Int64) As List(Of ATwiCliAudioFile)
            GetUsersAudios = New List(Of ATwiCliAudioFile)
            Dim strURL As String = TCC_GET_USERS_CONTENT_URL & "?userid=" & p_ID & "&content_type=audio"
            Try
                Dim rqUsersAudios As HttpWebRequest = DirectCast(System.Net.WebRequest.Create(strURL), HttpWebRequest)
                rqUsersAudios.Method = "GET"
                Dim rsUsersAudios As HttpWebResponse = DirectCast(rqUsersAudios.GetResponse, HttpWebResponse)
                Dim srUsersAudios As New StreamReader(rsUsersAudios.GetResponseStream)
                Dim strUsersAudios As String = srUsersAudios.ReadToEnd
                Dim xdAudios As New XmlDocument
                xdAudios.LoadXml(strUsersAudios)
                For Each xAudio As XmlNode In xdAudios.SelectNodes("//content")
                    Dim Audio As New ATwiCliAudioFile
                    Audio.ID = xAudio.SelectSingleNode("id").InnerText
                    Audio.URL = xAudio.SelectSingleNode("url").InnerText
                    Audio.UserID = xAudio.SelectSingleNode("user_id").InnerText
                    Audio.ScreenName = xAudio.SelectSingleNode("screen_name").InnerText
                    Audio.Comments_Count = CInt(xAudio.SelectSingleNode("num_comments").InnerText)
                    Audio.Views_Count = CInt(xAudio.SelectSingleNode("num_views").InnerText)
                    Audio.InsertUnixTime(CDbl(xAudio.SelectSingleNode("timestamp").InnerText))
                    GetUsersAudios.Add(Audio)
                Next

            Catch ex As Exception
                Throw New Exception(ex.Message)
            End Try
            Return GetUsersAudios

        End Function

        Public Overloads Function GetUsersAudios(ByVal p_strUserName As String) As List(Of ATwiCliAudioFile)
            GetUsersAudios = New List(Of ATwiCliAudioFile)
            Dim strURL As String = TCC_GET_USERS_CONTENT_URL & "?username=" & p_strUserName & "&content_type=audio"
            Try
                Dim rqUsersAudios As HttpWebRequest = DirectCast(System.Net.WebRequest.Create(strURL), HttpWebRequest)
                rqUsersAudios.Method = "GET"
                Dim rsUsersAudios As HttpWebResponse = DirectCast(rqUsersAudios.GetResponse, HttpWebResponse)
                Dim srUsersAudios As New StreamReader(rsUsersAudios.GetResponseStream)
                Dim strUsersAudios As String = srUsersAudios.ReadToEnd
                Dim xdAudios As New XmlDocument
                xdAudios.LoadXml(strUsersAudios)
                For Each xAudio As XmlNode In xdAudios.SelectNodes("//content")
                    Dim Audio As New ATwiCliAudioFile
                    Audio.ID = xAudio.SelectSingleNode("id").InnerText
                    Audio.URL = xAudio.SelectSingleNode("url").InnerText
                    Audio.UserID = xAudio.SelectSingleNode("user_id").InnerText
                    Audio.ScreenName = xAudio.SelectSingleNode("screen_name").InnerText
                    Audio.Comments_Count = CInt(xAudio.SelectSingleNode("num_comments").InnerText)
                    Audio.Views_Count = CInt(xAudio.SelectSingleNode("num_views").InnerText)
                    Audio.InsertUnixTime(CDbl(xAudio.SelectSingleNode("timestamp").InnerText))
                    GetUsersAudios.Add(Audio)
                Next

            Catch ex As Exception
                Throw New Exception(ex.Message)
            End Try
            Return GetUsersAudios

        End Function

        Public Overloads Function GetUsersVideos(ByVal p_ID As Int64) As List(Of ATwiCliVideoFile)
            GetUsersVideos = New List(Of ATwiCliVideoFile)
            Dim strURL As String = TCC_GET_USERS_CONTENT_URL & "?userid=" & p_ID & "&content_type=videos"
            Try
                Dim rqUsersVideos As HttpWebRequest = DirectCast(System.Net.WebRequest.Create(strURL), HttpWebRequest)
                rqUsersVideos.Method = "GET"
                Dim rsUsersVideos As HttpWebResponse = DirectCast(rqUsersVideos.GetResponse, HttpWebResponse)
                Dim srUsersVideos As New StreamReader(rsUsersVideos.GetResponseStream)
                Dim strUsersVideos As String = srUsersVideos.ReadToEnd
                Dim xdVideos As New XmlDocument
                xdVideos.LoadXml(strUsersVideos)
                For Each xVideo As XmlNode In xdVideos.SelectNodes("//content")
                    Dim video As New ATwiCliVideoFile
                    video.ID = xVideo.SelectSingleNode("id").InnerText
                    video.URL = xVideo.SelectSingleNode("url").InnerText
                    video.UserID = xVideo.SelectSingleNode("user_id").InnerText
                    video.ScreenName = xdVideos.SelectSingleNode("screen_name").InnerText
                    video.Comments_Count = CInt(xVideo.SelectSingleNode("num_comments").InnerText)
                    video.Views_Count = CInt(xVideo.SelectSingleNode("num_views").InnerText)
                    video.InsertUnixTime(CDbl(xVideo.SelectSingleNode("timestamp").InnerText))
                    GetUsersVideos.Add(video)
                Next
            Catch ex As Exception
                Throw New Exception(ex.Message)
            End Try
            Return GetUsersVideos

        End Function

        Public Overloads Function GetUsersVideos(ByVal p_strUserName As String) As List(Of ATwiCliVideoFile)
            GetUsersVideos = New List(Of ATwiCliVideoFile)
            Dim strURL As String = TCC_GET_USERS_CONTENT_URL & "?username=" & p_strUserName & "&content_type=videos"
            Try
                Dim rqUsersVideos As HttpWebRequest = DirectCast(System.Net.WebRequest.Create(strURL), HttpWebRequest)
                rqUsersVideos.Method = "GET"
                Dim rsUsersVideos As HttpWebResponse = DirectCast(rqUsersVideos.GetResponse, HttpWebResponse)
                Dim srUsersVideos As New StreamReader(rsUsersVideos.GetResponseStream)
                Dim strUsersVideos As String = srUsersVideos.ReadToEnd
                Dim xdVideos As New XmlDocument
                xdVideos.LoadXml(strUsersVideos)
                For Each xVideo As XmlNode In xdVideos.SelectNodes("//content")
                    Dim video As New ATwiCliVideoFile
                    video.ID = xVideo.SelectSingleNode("id").InnerText
                    video.URL = xVideo.SelectSingleNode("url").InnerText
                    video.UserID = xVideo.SelectSingleNode("user_id").InnerText
                    video.ScreenName = xdVideos.SelectSingleNode("screen_name").InnerText
                    video.Comments_Count = CInt(xVideo.SelectSingleNode("num_comments").InnerText)
                    video.Views_Count = CInt(xVideo.SelectSingleNode("num_views").InnerText)
                    video.InsertUnixTime(CDbl(xVideo.SelectSingleNode("timestamp").InnerText))
                    GetUsersVideos.Add(video)
                Next
            Catch ex As Exception
                Throw New Exception(ex.Message)
            End Try
            Return GetUsersVideos
        End Function
    End Class

End Namespace
