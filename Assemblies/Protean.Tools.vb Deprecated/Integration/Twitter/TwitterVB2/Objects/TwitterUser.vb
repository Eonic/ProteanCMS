'*
'* This file is part of the TwitterVB software
'* Copyright (c) 2009, Duane Roelands <duane@getTwitterVB.com>
'* All rights reserved.
'*
'* TwitterVB is a port of the Twitterizer library <http://code.google.com/p/twitterizer/>
'* Copyright (c) 2008, Patrick "Ricky" Smith <ricky@digitally-born.com>
'* All rights reserved. 
'*
'* Redistribution and use in source and binary forms, with or without modification, are 
'* permitted provided that the following conditions are met:
'*
'* - Redistributions of source code must retain the above copyright notice, this list 
'*   of conditions and the following disclaimer.
'* - Redistributions in binary form must reproduce the above copyright notice, this list 
'*   of conditions and the following disclaimer in the documentation and/or other 
'*   materials provided with the distribution.
'* - Neither the name of TwitterVB nor the names of its contributors may be 
'*   used to endorse or promote products derived from this software without specific 
'*   prior written permission.
'*
'* THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
'* ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
'* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
'* IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
'* INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT 
'* NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR 
'* PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
'* WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
'* ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
'* POSSIBILITY OF SUCH DAMAGE.
'*
Namespace Integration.Twitter.TwitterVB2

    ''' <summary>
    ''' An individual Twitter user.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class TwitterUser
        Inherits XmlObjectBase

#Region "Private Members"
        Private _ID As Int64
        Private _ScreenName As String = String.Empty
        Private _CreatedAt As DateTime
        Private _Description As String = String.Empty
        Private _FavoritesCount As Int64
        Private _FriendsCount As Int64
        Private _FollowersCount As Int64
        Private _Location As String = String.Empty
        Private _Name As String = String.Empty
        Private _Notifications As Boolean
        Private _ProfileBackgroundColor As String = String.Empty
        Private _ProfileBackgroundImageUrl As String = String.Empty
        Private _ProfileImageUrl As String = String.Empty
        Private _ProfileLinkColor As String = String.Empty
        Private _ProfileSidebarBorderColor As String = String.Empty
        Private _ProfileSidebarFillColor As String = String.Empty
        Private _ProfileTextColor As String = String.Empty
        Private _Protected As Boolean
        Private _Status As TwitterStatus = Nothing
        Private _StatusesCount As Int64
        Private _TimeZone As String = String.Empty
        Private _Url As String = String.Empty
        Private _UTCOffset As String = String.Empty
        Private _Verified As Boolean
#End Region

#Region "Public Properties"

        ''' <summary>
        ''' The ID of the user.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ID() As Int64
            Get
                Return _ID
            End Get
            Set(ByVal value As Int64)
                _ID = value
            End Set
        End Property

        ''' <summary>
        ''' The screen name of the user
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ScreenName() As String
            Get
                Return _ScreenName
            End Get
            Set(ByVal value As String)
                _ScreenName = value
            End Set
        End Property

        ''' <summary>
        ''' When the user account was created.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>
        ''' This value is UTC time.
        ''' </remarks>
        Public Property CreatedAt() As DateTime
            Get
                Return _CreatedAt
            End Get
            Set(ByVal value As DateTime)
                _CreatedAt = value
            End Set
        End Property

        ''' <summary>
        ''' When the user account was created.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>
        ''' This value is local time.
        ''' <para/>
        ''' This property is read-only because it is generated in TwitterVB, rather than the Twitter API.
        ''' </remarks>
        Public ReadOnly Property CreatedAtLocalTime() As DateTime
            Get
                Return _CreatedAt.ToLocalTime
            End Get
        End Property

        ''' <summary>
        ''' The description field from the user's profile.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Description() As String
            Get
                Return _Description
            End Get
            Set(ByVal value As String)
                _Description = value
            End Set
        End Property

        ''' <summary>
        ''' How many tweets the user has marked as a favorite.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property FavoritesCount() As Int64
            Get
                Return _FavoritesCount
            End Get
            Set(ByVal value As Int64)
                _FavoritesCount = value
            End Set
        End Property

        ''' <summary>
        ''' How many people this user is following.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property FriendsCount() As Int64
            Get
                Return _FriendsCount
            End Get
            Set(ByVal value As Int64)
                _FriendsCount = value
            End Set
        End Property

        ''' <summary>
        ''' How many people are following this user.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property FollowersCount() As Int64
            Get
                Return _FollowersCount
            End Get
            Set(ByVal value As Int64)
                _FollowersCount = value
            End Set
        End Property

        ''' <summary>
        ''' The location from the user's profile.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>
        ''' This contains no reliable data; it's just a text field.
        ''' </remarks>
        Public Property Location() As String
            Get
                Return _Location
            End Get
            Set(ByVal value As String)
                _Location = value
            End Set
        End Property

        ''' <summary>
        ''' The full name of the user.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>
        ''' This data may not be reliable, since the user can type anything (or nothing at all) here.
        ''' </remarks>
        Public Property Name() As String
            Get
                Return _Name
            End Get
            Set(ByVal value As String)
                _Name = value
            End Set
        End Property

        ''' <summary>
        ''' Whether or not the user has device notifications active.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Notifications() As Boolean
            Get
                Return _Notifications
            End Get
            Set(ByVal value As Boolean)
                _Notifications = value
            End Set
        End Property

        ''' <summary>
        ''' The background color on the user's profile.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ProfileBackgroundColor() As String
            Get
                Return _ProfileBackgroundColor
            End Get
            Set(ByVal value As String)
                _ProfileBackgroundColor = value
            End Set
        End Property

        ''' <summary>
        ''' The Url of the user's profile background image.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ProfileBackgroundImageUrl() As String
            Get
                Return _ProfileBackgroundImageUrl
            End Get
            Set(ByVal value As String)
                _ProfileBackgroundImageUrl = value
            End Set
        End Property

        ''' <summary>
        ''' The user's profile image.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ProfileImageUrl() As String
            Get
                Return _ProfileImageUrl
            End Get
            Set(ByVal value As String)
                _ProfileImageUrl = value
            End Set
        End Property

        ''' <summary>
        ''' The user's profile link color.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ProfileLinkColor() As String
            Get
                Return _ProfileLinkColor
            End Get
            Set(ByVal value As String)
                _ProfileLinkColor = value
            End Set
        End Property

        ''' <summary>
        ''' The user's profile sidebar border color.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ProfileSidebarBorderColor() As String
            Get
                Return _ProfileSidebarBorderColor
            End Get
            Set(ByVal value As String)
                _ProfileSidebarBorderColor = value
            End Set
        End Property

        ''' <summary>
        ''' The user's profile sidebar fill color.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ProfileSidebarFillColor() As String
            Get
                Return _ProfileSidebarFillColor
            End Get
            Set(ByVal value As String)
                _ProfileSidebarFillColor = value
            End Set
        End Property

        ''' <summary>
        ''' The user's profile text color.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ProfileTextColor() As String
            Get
                Return _ProfileTextColor
            End Get
            Set(ByVal value As String)
                _ProfileTextColor = value
            End Set
        End Property

        ''' <summary>
        ''' Whether or not this user's tweets are protected.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property [Protected]() As Boolean
            Get
                Return _Protected
            End Get
            Set(ByVal value As Boolean)
                _Protected = value
            End Set
        End Property

        ''' <summary>
        ''' A <c>TwitterStatus</c> object representing the user's last tweet.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Status() As TwitterStatus
            Get
                Return _Status
            End Get
            Set(ByVal value As TwitterStatus)
                _Status = value
            End Set
        End Property

        ''' <summary>
        ''' The number of tweets the user has posted.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property StatusesCount() As Int64
            Get
                Return _StatusesCount
            End Get
            Set(ByVal value As Int64)
                _StatusesCount = value
            End Set
        End Property

        ''' <summary>
        ''' The user's time zone
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>
        ''' This contains no reliable data; it's just a text field.
        ''' </remarks>
        Public Property TimeZone() As String
            Get
                Return _TimeZone
            End Get
            Set(ByVal value As String)
                _TimeZone = value
            End Set
        End Property

        ''' <summary>
        ''' The Url in the user's profile.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Url() As String
            Get
                Return _Url
            End Get
            Set(ByVal value As String)
                _Url = value
            End Set
        End Property

        ''' <summary>
        ''' The amount of time, in seconds, between the user's timezone and UTC time.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property UTCOffset() As String
            Get
                Return _UTCOffset
            End Get
            Set(ByVal value As String)
                _UTCOffset = value
            End Set
        End Property

        ''' <summary>
        ''' Whether or not the user is a verified user.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Verified() As Boolean
            Get
                Return _Verified
            End Get
            Set(ByVal value As Boolean)
                _Verified = value
            End Set
        End Property

#End Region

        ''' <summary>
        ''' Creates a new <c>TwitterUser</c> object.
        ''' </summary>
        ''' <param name="UserNode">An <c>XmlNode</c> from the Twitter API response representing a user.</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal UserNode As System.Xml.XmlNode)
            Me.ID = XmlInt64_Get(UserNode("id"))
            Me.ScreenName = XmlString_Get(UserNode("screen_name"))
            Me.CreatedAt = XmlDate_Get(UserNode("created_at"))
            Me.Description = XmlString_Get(UserNode("description"))
            Me.FavoritesCount = XmlInt64_Get(UserNode("favourites_count"))
            Me.FollowersCount = XmlInt64_Get(UserNode("followers_count"))
            Me.FriendsCount = XmlInt64_Get(UserNode("friends_count"))
            Me.Location = XmlString_Get(UserNode("location"))
            Me.Name = XmlString_Get(UserNode("name"))
            Me.Notifications = XmlBoolean_Get(UserNode("notifications"))
            Me.ProfileBackgroundColor = XmlString_Get(UserNode("profile_background_color"))
            Me.ProfileBackgroundImageUrl = XmlString_Get(UserNode("profile_background_image_url"))
            Me.ProfileImageUrl = XmlString_Get(UserNode("profile_image_url"))
            Me.ProfileLinkColor = XmlString_Get(UserNode("profile_link_color"))
            Me.ProfileSidebarBorderColor = XmlString_Get(UserNode("profile_sidebar_border_color"))
            Me.ProfileSidebarFillColor = XmlString_Get(UserNode("profile_sidebar_fill_color"))
            Me.ProfileTextColor = XmlString_Get(UserNode("profile_text_color"))
            Me.Protected = XmlBoolean_Get(UserNode("protected"))
            If UserNode("status") IsNot Nothing Then
                Me.Status = New TwitterStatus(UserNode("status"))
            End If
            Me.StatusesCount = XmlInt64_Get(UserNode("statuses_count"))
            Me.TimeZone = XmlString_Get(UserNode("time_zone"))
            Me.Url = XmlString_Get(UserNode("url"))
            Me.UTCOffset = XmlString_Get(UserNode("utc_offset"))
            Me.Verified = XmlBoolean_Get(UserNode("verified"))
        End Sub

        ''' <summary>
        ''' Default constructor
        ''' </summary>
        ''' <remarks></remarks>
        ''' <exclude/>
        Public Sub New()
        End Sub
    End Class
End Namespace
	
