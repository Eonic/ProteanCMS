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
    ''' An individual Twitter post.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class TwitterStatus
        Inherits XmlObjectBase

        Private _ID As Int64
        Private _CreatedAt As DateTime
        Private _Text As String = String.Empty
        Private _Favorited As Boolean
        Private _InReplyToStatusID As Int64
        Private _InReplyToUserID As String = String.Empty
        Private _InReplyToScreenName As String = String.Empty
        Private _IsDirectMessage As Boolean = False
        Private _Source As String = String.Empty
        Private _Truncated As Boolean
        Private _User As TwitterUser = Nothing
        Private _RetweetedStatus As TwitterStatus = Nothing
        Private _GeoLat As String = String.Empty
        Private _GeoLong As String = String.Empty

        ''' <summary>
        ''' The ID of the tweet.
        ''' </summary>
        ''' <value></value>
        ''' <returns>An <c>Int64</c> representing the tweest ID.</returns>
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
        ''' The date and time that the tweet was posted.
        ''' </summary>
        ''' <value></value>
        ''' <returns>A <c>DateTime</c> representing the date and time the tweet was posted.</returns>
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
        ''' The date and time that the tweet was posted.
        ''' </summary>
        ''' <value></value>
        ''' <returns>A <c>DateTime</c> representing the date and time the tweet was posted.</returns>
        ''' <remarks>
        ''' This value is local time.
        ''' <para/>
        ''' This property is read-only because it is generated in TwitterVB, rather than the Twitter API.
        ''' </remarks>
        Public ReadOnly Property CreatedAtLocalTime() As DateTime
            Get
                Return Me.CreatedAt.ToLocalTime
            End Get
        End Property

        ''' <summary>
        ''' The text of the tweet.
        ''' </summary>
        ''' <value></value>
        ''' <returns>A <c>string</c> representing the text of the tweet.</returns>
        ''' <remarks></remarks>
        Public Property Text() As String
            Get
                Return _Text
            End Get
            Set(ByVal value As String)
                _Text = value
            End Set
        End Property

        ''' <summary>
        ''' Whether or not this tweet was favorited by the authenticating user.
        ''' </summary>
        ''' <value></value>
        ''' <returns>A <c>boolean</c> indicating </returns>
        ''' <remarks></remarks>
        Public Property Favorited() As Boolean
            Get
                Return _Favorited
            End Get
            Set(ByVal value As Boolean)
                _Favorited = value
            End Set
        End Property

        ''' <summary>
        ''' The ID of the message to which this tweet is a reply.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>
        ''' If this tweet is not a reply, this is zero.
        ''' </remarks>
        Public Property InReplyToStatusID() As Int64
            Get
                Return _InReplyToStatusID
            End Get
            Set(ByVal value As Int64)
                _InReplyToStatusID = value
            End Set
        End Property

        ''' <summary>
        ''' The ID of the user to which this tweet is a reply.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>
        ''' If this tweet is not a reply, this is zero.
        ''' </remarks>
        Public Property InReplyToUserID() As String
            Get
                Return _InReplyToUserID
            End Get
            Set(ByVal value As String)
                _InReplyToUserID = value
            End Set
        End Property

        ''' <summary>
        ''' The screen name of the user to which this tweet is a reply.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>
        ''' If this tweet is not a reply, this is zero.
        ''' </remarks>
        Public Property InReplyToScreenName() As String
            Get
                Return _InReplyToScreenName
            End Get
            Set(ByVal value As String)
                _InReplyToScreenName = value
            End Set
        End Property

        ''' <summary>
        ''' The source of the tweet.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Source() As String
            Get
                Return _Source
            End Get
            Set(ByVal value As String)
                _Source = Value
            End Set
        End Property

        ''' <summary>
        ''' Whether or not the text of the tweet has been truncated.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Truncated() As Boolean
            Get
                Return _Truncated
            End Get
            Set(ByVal value As Boolean)
                _Truncated = Value
            End Set
        End Property

        ''' <summary>
        ''' A <c>TwitterUser</c> object representing the user that posted the tweet.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property User() As TwitterUser
            Get
                Return _User
            End Get
            Set(ByVal value As TwitterUser)
                _User = Value
            End Set
        End Property

        ''' <summary>
        ''' Indicates whether or not the Tweet is a direct message.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>This property is not populated when the object is created.  It is populated by the methods involved with direct messages.  
        ''' It will always be <c>False</c></remarks> unless set to <c>True</c> by an external method.
        Public Property IsDirectMessage() As Boolean
            Get
                Return _IsDirectMessage
            End Get
            Set(ByVal value As Boolean)
                _IsDirectMessage = value
            End Set
        End Property

        ''' <summary>
        ''' The original tweet being retweeted.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>
        ''' The Twitter API does not populate this on the Home Timeline.
        ''' </remarks>
        Public Property RetweetedStatus() As TwitterStatus
            Get
                Return _RetweetedStatus
            End Get
            Set(ByVal value As TwitterStatus)
                _RetweetedStatus = value
            End Set
        End Property

        ''' <summary>
        ''' The latitude the tweet was sent from
        ''' </summary>
        ''' <value></value>
        ''' <returns>A <c>string</c> representing the latitude the tweet was sent from.</returns>
        ''' <remarks></remarks>
        Public Property GeoLat() As String
            Get
                Return _GeoLat
            End Get
            Set(ByVal value As String)
                _GeoLat = value
            End Set
        End Property

        ''' <summary>
        ''' The longitude the tweet was sent from.
        ''' </summary>
        ''' <value></value>
        ''' <returns>A <c>string</c> representing the longitude the tweet was sent from.</returns>
        ''' <remarks></remarks>
        Public Property GeoLong() As String
            Get
                Return _GeoLong
            End Get
            Set(ByVal value As String)
                _GeoLong = value
            End Set
        End Property

        ''' <summary>
        ''' Creates a new <c>TwitterStatus</c> object.
        ''' </summary>
        ''' <param name="StatusNode">An <c>XmlNode</c> from the Twitter API response representing a status.</param>
        ''' <remarks></remarks>
        ''' <exclude/>
        Public Sub New(ByVal StatusNode As System.Xml.XmlNode)

            Me.CreatedAt = XmlDate_Get(StatusNode("created_at"))
            Me.Favorited = XmlBoolean_Get(StatusNode("favorited"))
            Me.ID = XmlInt64_Get(StatusNode("id"))
            Me.InReplyToScreenName = XmlString_Get(StatusNode("in_reply_to_screen_name"))
            Me.InReplyToStatusID = XmlInt64_Get(StatusNode("in_reply_to_status_id"))
            Me.InReplyToUserID = XmlString_Get(StatusNode("in_reply_to_user_id"))
            Me.Source = XmlString_Get(StatusNode("source"))
            Me.Text = XmlString_Get(StatusNode("text"))
            Me.Truncated = XmlBoolean_Get(StatusNode("truncated"))
            If StatusNode("user") IsNot Nothing Then
                Me.User = New TwitterUser(StatusNode("user"))
            End If

            If StatusNode("retweeted_status") IsNot Nothing Then
                Me.RetweetedStatus = New TwitterStatus(StatusNode("retweeted_status"))
            End If

            ' Geotag parsing
            Dim GeoTag As String = XmlString_Get(StatusNode("geo"))
            If Not String.IsNullOrEmpty(GeoTag) Then
                Dim LatLongArray() As String = GeoTag.Split(New Char() {" "c})
                If LatLongArray.Length = 2 Then
                    Me.GeoLat = LatLongArray(0)
                    Me.GeoLong = LatLongArray(1)
                End If
            End If
        End Sub
    End Class


End Namespace

