using System;
// *
// * This file is part of the TwitterVB software
// * Copyright (c) 2009, Duane Roelands <duane@getTwitterVB.com>
// * All rights reserved.
// *
// * TwitterVB is a port of the Twitterizer library <http://code.google.com/p/twitterizer/>
// * Copyright (c) 2008, Patrick "Ricky" Smith <ricky@digitally-born.com>
// * All rights reserved. 
// *
// * Redistribution and use in source and binary forms, with or without modification, are 
// * permitted provided that the following conditions are met:
// *
// * - Redistributions of source code must retain the above copyright notice, this list 
// *   of conditions and the following disclaimer.
// * - Redistributions in binary form must reproduce the above copyright notice, this list 
// *   of conditions and the following disclaimer in the documentation and/or other 
// *   materials provided with the distribution.
// * - Neither the name of TwitterVB nor the names of its contributors may be 
// *   used to endorse or promote products derived from this software without specific 
// *   prior written permission.
// *
// * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
// * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
// * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
// * IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
// * INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT 
// * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR 
// * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
// * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
// * POSSIBILITY OF SUCH DAMAGE.
// *

namespace Protean.Tools.Integration.Twitter.TwitterVB2
{
    /// <summary>
    /// An individual Twitter user.
    /// </summary>
    /// <remarks></remarks>
    public partial class TwitterUser : XmlObjectBase
    {

        #region Private Members
        private long _ID;
        private string _ScreenName = string.Empty;
        private DateTime _CreatedAt;
        private string _Description = string.Empty;
        private long _FavoritesCount;
        private long _FriendsCount;
        private long _FollowersCount;
        private string _Location = string.Empty;
        private string _Name = string.Empty;
        private bool _Notifications;
        private string _ProfileBackgroundColor = string.Empty;
        private string _ProfileBackgroundImageUrl = string.Empty;
        private string _ProfileImageUrl = string.Empty;
        private string _ProfileLinkColor = string.Empty;
        private string _ProfileSidebarBorderColor = string.Empty;
        private string _ProfileSidebarFillColor = string.Empty;
        private string _ProfileTextColor = string.Empty;
        private bool _Protected;
        private TwitterStatus _Status = default;
        private long _StatusesCount;
        private string _TimeZone = string.Empty;
        private string _Url = string.Empty;
        private string _UTCOffset = string.Empty;
        private bool _Verified;
        #endregion

        #region Public Properties

        /// <summary>
        /// The ID of the user.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public long ID
        {
            get
            {
                return _ID;
            }
            set
            {
                _ID = value;
            }
        }

        /// <summary>
        /// The screen name of the user
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string ScreenName
        {
            get
            {
                return _ScreenName;
            }
            set
            {
                _ScreenName = value;
            }
        }

        /// <summary>
        /// When the user account was created.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>
        /// This value is UTC time.
        /// </remarks>
        public DateTime CreatedAt
        {
            get
            {
                return _CreatedAt;
            }
            set
            {
                _CreatedAt = value;
            }
        }

        /// <summary>
        /// When the user account was created.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>
        /// This value is local time.
        /// <para/>
        /// This property is read-only because it is generated in TwitterVB, rather than the Twitter API.
        /// </remarks>
        public DateTime CreatedAtLocalTime
        {
            get
            {
                return _CreatedAt.ToLocalTime();
            }
        }

        /// <summary>
        /// The description field from the user's profile.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string Description
        {
            get
            {
                return _Description;
            }
            set
            {
                _Description = value;
            }
        }

        /// <summary>
        /// How many tweets the user has marked as a favorite.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public long FavoritesCount
        {
            get
            {
                return _FavoritesCount;
            }
            set
            {
                _FavoritesCount = value;
            }
        }

        /// <summary>
        /// How many people this user is following.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public long FriendsCount
        {
            get
            {
                return _FriendsCount;
            }
            set
            {
                _FriendsCount = value;
            }
        }

        /// <summary>
        /// How many people are following this user.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public long FollowersCount
        {
            get
            {
                return _FollowersCount;
            }
            set
            {
                _FollowersCount = value;
            }
        }

        /// <summary>
        /// The location from the user's profile.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>
        /// This contains no reliable data; it's just a text field.
        /// </remarks>
        public string Location
        {
            get
            {
                return _Location;
            }
            set
            {
                _Location = value;
            }
        }

        /// <summary>
        /// The full name of the user.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>
        /// This data may not be reliable, since the user can type anything (or nothing at all) here.
        /// </remarks>
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
            }
        }

        /// <summary>
        /// Whether or not the user has device notifications active.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool Notifications
        {
            get
            {
                return _Notifications;
            }
            set
            {
                _Notifications = value;
            }
        }

        /// <summary>
        /// The background color on the user's profile.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string ProfileBackgroundColor
        {
            get
            {
                return _ProfileBackgroundColor;
            }
            set
            {
                _ProfileBackgroundColor = value;
            }
        }

        /// <summary>
        /// The Url of the user's profile background image.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string ProfileBackgroundImageUrl
        {
            get
            {
                return _ProfileBackgroundImageUrl;
            }
            set
            {
                _ProfileBackgroundImageUrl = value;
            }
        }

        /// <summary>
        /// The user's profile image.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string ProfileImageUrl
        {
            get
            {
                return _ProfileImageUrl;
            }
            set
            {
                _ProfileImageUrl = value;
            }
        }

        /// <summary>
        /// The user's profile link color.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string ProfileLinkColor
        {
            get
            {
                return _ProfileLinkColor;
            }
            set
            {
                _ProfileLinkColor = value;
            }
        }

        /// <summary>
        /// The user's profile sidebar border color.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string ProfileSidebarBorderColor
        {
            get
            {
                return _ProfileSidebarBorderColor;
            }
            set
            {
                _ProfileSidebarBorderColor = value;
            }
        }

        /// <summary>
        /// The user's profile sidebar fill color.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string ProfileSidebarFillColor
        {
            get
            {
                return _ProfileSidebarFillColor;
            }
            set
            {
                _ProfileSidebarFillColor = value;
            }
        }

        /// <summary>
        /// The user's profile text color.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string ProfileTextColor
        {
            get
            {
                return _ProfileTextColor;
            }
            set
            {
                _ProfileTextColor = value;
            }
        }

        /// <summary>
        /// Whether or not this user's tweets are protected.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool Protected
        {
            get
            {
                return _Protected;
            }
            set
            {
                _Protected = value;
            }
        }

        /// <summary>
        /// A <c>TwitterStatus</c> object representing the user's last tweet.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public TwitterStatus Status
        {
            get
            {
                return _Status;
            }
            set
            {
                _Status = value;
            }
        }

        /// <summary>
        /// The number of tweets the user has posted.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public long StatusesCount
        {
            get
            {
                return _StatusesCount;
            }
            set
            {
                _StatusesCount = value;
            }
        }

        /// <summary>
        /// The user's time zone
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>
        /// This contains no reliable data; it's just a text field.
        /// </remarks>
        public string TimeZone
        {
            get
            {
                return _TimeZone;
            }
            set
            {
                _TimeZone = value;
            }
        }

        /// <summary>
        /// The Url in the user's profile.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string Url
        {
            get
            {
                return _Url;
            }
            set
            {
                _Url = value;
            }
        }

        /// <summary>
        /// The amount of time, in seconds, between the user's timezone and UTC time.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string UTCOffset
        {
            get
            {
                return _UTCOffset;
            }
            set
            {
                _UTCOffset = value;
            }
        }

        /// <summary>
        /// Whether or not the user is a verified user.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool Verified
        {
            get
            {
                return _Verified;
            }
            set
            {
                _Verified = value;
            }
        }

        #endregion

        /// <summary>
        /// Creates a new <c>TwitterUser</c> object.
        /// </summary>
        /// <param name="UserNode">An <c>XmlNode</c> from the Twitter API response representing a user.</param>
        /// <remarks></remarks>
        public TwitterUser(System.Xml.XmlNode UserNode)
        {
            ID = XmlInt64_Get(UserNode["id"]);
            ScreenName = XmlString_Get(UserNode["screen_name"]);
            CreatedAt = XmlDate_Get(UserNode["created_at"]);
            Description = XmlString_Get(UserNode["description"]);
            FavoritesCount = XmlInt64_Get(UserNode["favourites_count"]);
            FollowersCount = XmlInt64_Get(UserNode["followers_count"]);
            FriendsCount = XmlInt64_Get(UserNode["friends_count"]);
            Location = XmlString_Get(UserNode["location"]);
            Name = XmlString_Get(UserNode["name"]);
            Notifications = XmlBoolean_Get(UserNode["notifications"]);
            ProfileBackgroundColor = XmlString_Get(UserNode["profile_background_color"]);
            ProfileBackgroundImageUrl = XmlString_Get(UserNode["profile_background_image_url"]);
            ProfileImageUrl = XmlString_Get(UserNode["profile_image_url"]);
            ProfileLinkColor = XmlString_Get(UserNode["profile_link_color"]);
            ProfileSidebarBorderColor = XmlString_Get(UserNode["profile_sidebar_border_color"]);
            ProfileSidebarFillColor = XmlString_Get(UserNode["profile_sidebar_fill_color"]);
            ProfileTextColor = XmlString_Get(UserNode["profile_text_color"]);
            Protected = XmlBoolean_Get(UserNode["protected"]);
            if (UserNode["status"] != null)
            {
                Status = new TwitterStatus(UserNode["status"]);
            }
            StatusesCount = XmlInt64_Get(UserNode["statuses_count"]);
            TimeZone = XmlString_Get(UserNode["time_zone"]);
            Url = XmlString_Get(UserNode["url"]);
            UTCOffset = XmlString_Get(UserNode["utc_offset"]);
            Verified = XmlBoolean_Get(UserNode["verified"]);
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <remarks></remarks>
        /// <exclude/>
        public TwitterUser()
        {
        }
    }
}
