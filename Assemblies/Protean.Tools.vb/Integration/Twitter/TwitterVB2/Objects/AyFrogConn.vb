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
    Public Class AyFrogConn
        Private m_strUserName As String
        Private m_strPassword As String
        Private m_strKey As String
        Private m_arAllowedPhotos() As String
        Private m_arAllowedVideos() As String

        Public Enum yfFileType
            yfVideo
            yfPhoto
        End Enum

#Region "YF_CONSTANT_URL"
        Const YP_UPLOAD_AND_POST As String = "http://yfrog.com/api/uploadAndPost"
#End Region

#Region "YF_FUNCTIONS"
        ''' <summary>
        ''' Uploads an image to yFrog and updates the user's twitter status
        ''' </summary>
        ''' <param name="p_strFileName">The file that should be uploaded</param>
        ''' <param name="p_strText">Text of the tweet</param>
        ''' <param name="p_bPublic">Whether or not it is public. Default is not (private)</param>
        ''' 
        ''' <param name="p_strTags">List of the tags that are to be appended.  Geocode is accepted here</param>
        ''' <returns>The status ID that contains the link.</returns>
        ''' <remarks></remarks>
        ''' 
        Public Function UploadAndPost(ByVal p_strFileName As String, ByVal p_strText As String, ByVal p_FileType As yfFileType, Optional ByVal p_bPublic As Boolean = False, Optional ByVal p_strTags As String = "") As String
            UploadAndPost = String.Empty

            If Me.UserName = "" Then
                Throw New Exception("No username specififed")
                Exit Function

            End If
            If Me.Password = "" Then
                Throw New Exception("No password specified")
                Exit Function
            End If

            Dim rqUploadtoYFrog As HttpWebRequest = DirectCast(System.Net.WebRequest.Create(YP_UPLOAD_AND_POST), HttpWebRequest)
            Dim strBoundary As String = Guid.NewGuid.ToString()
            Dim strheader As String = String.Format("--{0}", strBoundary)
            Dim strFooter As String = String.Format("--{0}--", strBoundary)
            With rqUploadtoYFrog
                .PreAuthenticate = True
                .AllowWriteStreamBuffering = True
                .ContentType = String.Format("multipart/form-data boundary={0}", strBoundary)
                .Method = "POST"

            End With
            Dim sbContent As New StringBuilder


            If p_strFileName.StartsWith("http://") Then
                sbContent.AppendLine(strheader)
                sbContent.AppendLine([String].Format("Content-Disposition: form-data name=""{0}""", "URL"))
                sbContent.AppendLine()
                sbContent.AppendLine(p_strFileName)


            Else
                Dim b_ValidExtention As Boolean
                b_ValidExtention = False
                If p_FileType = yfFileType.yfPhoto Then
                    For Each strEnding As String In m_arAllowedPhotos
                        If p_strFileName.EndsWith(strEnding) Then
                            b_ValidExtention = True
                            Exit For

                        End If
                    Next
                ElseIf p_FileType = yfFileType.yfVideo Then
                    For Each strEnding As String In m_arAllowedVideos
                        If p_strFileName.EndsWith(strEnding) Then
                            b_ValidExtention = True
                            Exit For

                        End If
                    Next
                End If

                If Not b_ValidExtention Then
                    Throw New Exception("Invalid file format")
                End If

                Dim bFile() As Byte = System.IO.File.ReadAllBytes(p_strFileName)
                sbContent.AppendLine(strheader)
                Dim strFileHeader As String

                strFileHeader = [String].Format("Content-Disposition: file; name=""{0}""; filename=""{1}""", "media", p_strFileName)
                sbContent.AppendLine(strFileHeader)
                Dim strFileData As String = Encoding.GetEncoding("iso-8859-1").GetString(bFile)
                sbContent.AppendLine([String].Format("Content-Type: {0}", "application/octet-stream"))
                sbContent.AppendLine()

                sbContent.AppendLine(strFileData)


            End If
            sbContent.AppendLine(strheader)

            sbContent.AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "username"))
            sbContent.AppendLine()
            sbContent.AppendLine(Me.UserName)
            sbContent.AppendLine(strheader)
            sbContent.AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "password"))
            sbContent.AppendLine()
            sbContent.AppendLine(Me.Password)

            If p_strText.Length > 0 And p_strText.Length < 120 Then
                sbContent.AppendLine(strheader)
                sbContent.AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "message"))
                sbContent.AppendLine()
                sbContent.AppendLine(p_strText)
            End If
            'Dim bIsNothing As Boolean = p_ltTagsa Is Nothing

            If p_strTags.Length > 0 Then
                sbContent.AppendLine(strheader)
                sbContent.AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "tags"))
                sbContent.AppendLine()
                sbContent.AppendLine(p_strTags)
            End If
            sbContent.AppendLine(strheader)
            sbContent.AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "public"))
            sbContent.AppendLine()
            If p_bPublic = True Then
                sbContent.AppendLine("yes")
            Else
                sbContent.AppendLine("no")
            End If
            sbContent.AppendLine(strheader)
            sbContent.AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "key"))
            sbContent.AppendLine()
            sbContent.AppendLine(Me.Key)
            sbContent.AppendLine(strFooter)
            Dim bFormData() As Byte = Encoding.GetEncoding("iso-8859-1").GetBytes(sbContent.ToString())
            rqUploadtoYFrog.ContentLength = bFormData.Length
            Dim stUploadRequest As Stream = rqUploadtoYFrog.GetRequestStream()
            stUploadRequest.Write(bFormData, 0, bFormData.Length)
            Dim rspuploadtoyfrog As HttpWebResponse = DirectCast(rqUploadtoYFrog.GetResponse, HttpWebResponse)
            Dim srUploadResponse As New StreamReader(rspuploadtoyfrog.GetResponseStream)
            Dim strUploadResponse As String = srUploadResponse.ReadToEnd()
            Dim xUploadResponse As New XmlDocument
            xUploadResponse.LoadXml(strUploadResponse)
            Dim xnResponseNode As XmlNode = xUploadResponse.SelectSingleNode("//rsp")
            If xnResponseNode.Attributes("stat").InnerText = "ok" Then
                UploadAndPost = xnResponseNode.SelectSingleNode("//mediaid").InnerText
            Else
                Throw New Exception(xnResponseNode.SelectSingleNode("//err").Attributes("msg").InnerText)

            End If

            Return UploadAndPost

        End Function

        Public Property Key() As String
            Get
                Return m_strKey

            End Get
            Set(ByVal value As String)
                m_strKey = value

            End Set
        End Property
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
        Public Sub New()
            Me.New("", "", "")

        End Sub

        Public Sub New(ByVal p_strUserName As String, ByVal p_Password As String, Optional ByVal p_strKey As String = "")
            If p_strUserName.Length > 0 Then
                Me.UserName = p_strUserName

            End If
            If p_Password.Length > 0 Then
                Me.Password = p_Password

            End If
            If p_strKey.Length > 0 Then
                Me.Key = p_strKey

            End If
            m_arAllowedPhotos = New String() {".jpeg", ".png", ".bmp", ".gif", ".jpg"}
            m_arAllowedVideos = New String() {".flv", ".mpeg", ".mkv", ".wmv", ".mov", ".3gp", ".mp4", ".avi", ".mpg"}


        End Sub
#End Region

    End Class
End Namespace