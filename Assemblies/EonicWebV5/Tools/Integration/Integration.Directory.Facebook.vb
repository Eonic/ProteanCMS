Option Strict Off
Option Explicit On

Imports System.Collections.Specialized
Imports Eonic.Tools.Http.Utils
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Net
Imports System.IO
Imports System.Web.Script.Serialization
Imports System.Web.Security
Imports System.Collections
Imports System.Configuration
Imports System.Xml


Namespace Integration.Directory

    ''' <summary>
    ''' Twitter class
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Facebook

        Inherits Integration.Directory.BaseProvider
        Implements Integration.Directory.IPostable

        Public facebookId As String
        Public facebookKey As String
        Public AccessToken As String
        Private Const mcModuleName As String = "Integration.Directory.Facebook"

        Private Const _postLengthLimit As Integer = 140

        Public Shadows Event OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)

        Public Sub New(ByRef aWeb As Eonic.Web, ByVal fbId As String, fbKey As String)
            MyBase.New(aWeb)
            facebookId = fbId
            facebookKey = fbKey
            _providerName = "facebook"
        End Sub
        Public Sub New(ByRef aWeb As Eonic.Web, ByRef directoryId As Long)
            MyBase.New(aWeb, directoryId)
        End Sub

        Function CreateUser(ByRef fbUser As Facebook.User) As Long
            Try
                'Check FB not allready exists if it does return that user id.
                Dim nUserId As Long = ValidateExternalAuth(fbUser.id)
                If nUserId > 0 Then
                    Return nUserId
                Else
                    'Get user Instance from Xform
                    ' Dim UserXml As XmlElement = GetUserSchemaXml()
                    'Populate Instance info from Facebook User

                    'Create User as best we can

                    'If data not complete set status as requires data

                    'Add ExternalAuthInfo

                    'Return New User Id.
                End If

            Catch ex As Exception

            End Try

        End Function


        Function GetFacebookUserData(code As String, ByVal redirectURI As String) As List(Of Facebook.User)
            Try


                ' Exchange the code for an access token
                Dim targetUri As New Uri(Convert.ToString("https://graph.facebook.com/oauth/access_token?client_id=" + facebookId + "&client_secret=" + facebookKey + "&redirect_uri=" + redirectURI + "&code=") & code)
                Dim at As HttpWebRequest = DirectCast(HttpWebRequest.Create(targetUri), HttpWebRequest)

                Dim str As New System.IO.StreamReader(at.GetResponse().GetResponseStream())
                Dim token As String = str.ReadToEnd().ToString().Replace("access_token=", "")

                ' Split the access token and expiration from the single string
                Dim combined As String() = token.Split("&"c)
                AccessToken = combined(0)

                ' Exchange the code for an extended access token
                Dim eatTargetUri As New Uri(Convert.ToString("https://graph.facebook.com/oauth/access_token?grant_type=fb_exchange_token&client_id=" + facebookId + "&client_secret=" + facebookKey + "&fb_exchange_token=") & accessToken)
                Dim eat As HttpWebRequest = DirectCast(HttpWebRequest.Create(eatTargetUri), HttpWebRequest)

                Dim eatStr As New StreamReader(eat.GetResponse().GetResponseStream())
                Dim eatToken As String = eatStr.ReadToEnd().ToString().Replace("access_token=", "")

                ' Split the access token and expiration from the single string
                Dim eatWords As String() = eatToken.Split("&"c)
                Dim extendedAccessToken As String = eatWords(0)

                ' Request the Facebook user information
                Dim targetUserUri As New Uri(Convert.ToString("https://graph.facebook.com/me?fields=first_name,last_name,email,gender,locale,link&access_token=") & accessToken)
                Dim user As HttpWebRequest = DirectCast(HttpWebRequest.Create(targetUserUri), HttpWebRequest)

                ' Read the returned JSON object response
                Dim userInfo As New StreamReader(user.GetResponse().GetResponseStream())
                Dim jsonResponse As String = String.Empty
                jsonResponse = userInfo.ReadToEnd()

                ' Deserialize and convert the JSON object to the Facebook.User object type
                Dim sr As New JavaScriptSerializer()
                Dim jsondata As String = jsonResponse
                Dim converted As Facebook.User = sr.Deserialize(Of Facebook.User)(jsondata)

                ' Write the user data to a List
                Dim currentUser As New List(Of Facebook.User)()
                currentUser.Add(converted)

                ' Return the current Facebook user
                Return currentUser
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "Logon", ex, ""))
            End Try
        End Function

        Public Class User
                Public Property id() As String
                    Get
                        Return m_id
                    End Get
                    Set
                        m_id = Value
                    End Set
                End Property
                Private m_id As String
                Public Property first_name() As String
                    Get
                        Return m_first_name
                    End Get
                    Set
                        m_first_name = Value
                    End Set
                End Property
                Private m_first_name As String
                Public Property last_name() As String
                    Get
                        Return m_last_name
                    End Get
                    Set
                        m_last_name = Value
                    End Set
                End Property
            Private m_last_name As String
            Public Property email() As String
                Get
                    Return m_email
                End Get
                Set
                    m_email = Value
                End Set
            End Property
            Private m_email As String
            Public Property link() As String
                    Get
                        Return m_link
                    End Get
                    Set
                        m_link = Value
                    End Set
                End Property
                Private m_link As String
                Public Property username() As String
                    Get
                        Return m_username
                    End Get
                    Set
                        m_username = Value
                    End Set
                End Property
                Private m_username As String
                Public Property gender() As String
                    Get
                        Return m_gender
                    End Get
                    Set
                        m_gender = Value
                    End Set
                End Property
                Private m_gender As String
                Public Property locale() As String
                    Get
                        Return m_locale
                    End Get
                    Set
                        m_locale = Value
                    End Set
                End Property
                Private m_locale As String
            End Class


        End Class



End Namespace
