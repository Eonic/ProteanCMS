Option Strict Off
Option Explicit On

Imports System.Collections.Specialized
Imports Eonic.Tools.Http.Utils
Imports System


Namespace Integration.Directory

    ''' <summary>
    ''' Twitter class
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Twitter

        Inherits Integration.Directory.BaseProvider
        Implements Integration.Directory.IPostable

        Public twitterConsumerKey As String = "c2h0VNmcE5c0Vu0fsHEfGg"
        Public twitterConsumerSecret As String = "5DzFnQtX7cNxuChpJCcz1ZzhGfPGTAh4MYWMuuJvxQ"

        Private Const _postLengthLimit As Integer = 140

        Public Shadows Event OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)


        Public Sub New(ByRef aWeb As Eonic.Web)
            MyBase.New(aWeb)
        End Sub
        Public Sub New(ByRef aWeb As Eonic.Web, ByRef directoryId As Long)
            MyBase.New(aWeb, directoryId)
        End Sub

        Public Function GetRequestToken() As String

            Try
                If IsAuthorisedUser Then


                    Dim callbackParameters As New NameValueCollection(3)
                    callbackParameters.Add("oAuthResp", "twitter")
                    callbackParameters.Add("integration", "Twitter.AccessTokens")
                    callbackParameters.Add("dirId", MyBase.DirectoryId.ToString())

                    Dim callback As Uri = BuildURIFromRequest(myWeb.moRequest, callbackParameters)

                    Dim twitterAPI As New Eonic.Tools.Integration.Twitter.TwitterVB2.TwitterAPI()
                    Dim authenticationLink As String = twitterAPI.GetAuthenticationLink(twitterConsumerKey, twitterConsumerSecret, callback.ToString())

                    myWeb.AddResponse("Twitter.AuthenticationLink", authenticationLink, , ResponseType.Redirect)
                    myWeb.msRedirectOnEnd = authenticationLink
                    Return authenticationLink
                Else

                    myWeb.AddResponse("Twitter.GetRequestToken.Unauthorised", "The user requesting this method is not authorised", , ResponseType.Alert)
                    Return ""

                End If

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(_moduleName, "GetRequestToken", ex, ""))
                Return ""
            End Try


        End Function

        Public Sub AccessTokens()
            Try
                Dim token As String = myWeb.moRequest("oauth_token")
                Dim verifier As String = myWeb.moRequest("oauth_verifier")
                AccessTokens(token, verifier)
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(_moduleName, "AccessTokens()", ex, ""))
            End Try
        End Sub
        Public Sub AccessTokens(ByVal token As String, ByVal verifier As String)

            Try

                If IsAuthorisedUser Then
                    Dim twitterAPI As New Eonic.Tools.Integration.Twitter.TwitterVB2.TwitterAPI()

                    If String.IsNullOrEmpty(token) Or String.IsNullOrEmpty(verifier) Then
                        ' token or Verifier are not populated
                        myWeb.AddResponse("Twitter.AccessTokens.NullArguments", "Either the token or the verifier were blank", , ResponseType.Alert)

                    Else
                        twitterAPI.GetAccessTokens(twitterConsumerKey, twitterConsumerSecret, token, verifier)

                        _credentials.AddSetting("OAuth_Token", twitterAPI.OAuth_Token)
                        _credentials.AddSetting("OAuth_TokenSecret", twitterAPI.OAuth_TokenSecret)

                        ' Get the user name
                        Try
                            Dim user As Eonic.Tools.Integration.Twitter.TwitterVB2.TwitterUser
                            twitterAPI.AuthenticateWith(twitterConsumerKey, twitterConsumerSecret, twitterAPI.OAuth_Token, twitterAPI.OAuth_TokenSecret)
                            user = twitterAPI.AccountInformation()
                            _credentials.AddSetting("Name", user.Name)
                            _credentials.AddSetting("ScreenName", user.ScreenName)
                        Catch ex As Exception

                        End Try

                        SaveCredentials()

                        myWeb.AddResponse("Twitter.AccessTokens.Success", "User account has been successfully linked to Twitter", , ResponseType.Hint)

                    End If
                Else

                    myWeb.AddResponse("Twitter.AccessTokens.Unauthorised", "The user requesting this method is not authorised", , ResponseType.Alert)

                End If



            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(_moduleName, "AccessTokens(String,String)", ex, ""))
            End Try

        End Sub

        Public Sub Update()
            Try


                Dim status As String = myWeb.moRequest("status")
                Update(status)

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(_moduleName, "Update(String)", ex, ""))
            End Try

        End Sub
        Public Sub Update(ByVal status As String)
            Try


                If Not IsAuthorisedUser Then
                    myWeb.AddResponse("Twitter.Update.Unauthorised", "The user requesting this method is not authorised", , ResponseType.Alert)

                ElseIf Not Me.IsProviderCredentialLoaded Then
                    myWeb.AddResponse("Twitter.Update.NoCredentials", "Could not load credentials for user Id" & DirectoryId, , ResponseType.Alert)

                Else
                    ' Load the credentials
                    Dim token As String = _credentials.GetSetting("OAuth_Token")
                    Dim secret As String = _credentials.GetSetting("OAuth_TokenSecret")

                    If String.IsNullOrEmpty(token) Or String.IsNullOrEmpty(token) Then
                        myWeb.AddResponse("Twitter.Update.NullArguments", "No valid credentials are available", , ResponseType.Alert)

                    ElseIf String.IsNullOrEmpty(status) Then
                        myWeb.AddResponse("Twitter.Update.NullStatus", "The status is blank", , ResponseType.Alert)

                    Else
                        Dim twitterAPI As New Eonic.Tools.Integration.Twitter.TwitterVB2.TwitterAPI()
                        twitterAPI.AuthenticateWith(twitterConsumerKey, twitterConsumerSecret, token, secret)
                        twitterAPI.Update(status)
                    End If


                End If

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(_moduleName, "Update(String)", ex, ""))
                myWeb.AddResponse("Twitter.Update.Failed", "The update failed - see error log for more detail", , ResponseType.Alert)
            End Try
        End Sub



        Public Overrides Sub Post(ByVal content As String, Optional ByVal trackbackUri As Uri = Nothing, Optional ByVal contentId As Long = 0)
            Try

                ' Is there something to post
                If Not String.IsNullOrEmpty(content) Then

                    Dim postLimitLength As Integer = _postLengthLimit

                    ' First shorten the URL, if it exists.
                    Dim shortenedURL As String = ""
                    If trackbackUri IsNot Nothing Then
                        shortenedURL = Tools.Http.Utils.ShortenURL(trackbackUri.ToString)
                        ' If shortening has failed check the original uri
                        If String.IsNullOrEmpty(shortenedURL) Then shortenedURL = trackbackUri.ToString
                        If Not String.IsNullOrEmpty(shortenedURL) Then shortenedURL = " " & shortenedURL
                    End If

                    postLimitLength -= shortenedURL.Length

                    ' Truncate the content
                    content = Tools.Text.TruncateString(content, postLimitLength)

                    Me.Update(content & shortenedURL)

                    myWeb.moDbHelper.logActivity(Web.dbHelper.ActivityType.IntegrationTwitterPost, Me.DirectoryId, 0, contentId, shortenedURL)

                End If

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(_moduleName, "Post()", ex, ""))
            End Try
        End Sub

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
