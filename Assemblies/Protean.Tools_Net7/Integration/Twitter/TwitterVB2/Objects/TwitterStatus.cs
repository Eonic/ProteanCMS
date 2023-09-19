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
    /// An individual Twitter post.
    /// </summary>
    /// <remarks></remarks>
    public partial class TwitterStatus : XmlObjectBase
    {

        private long _ID;
        private DateTime _CreatedAt;
        private string _Text = string.Empty;
        private bool _Favorited;
        private long _InReplyToStatusID;
        private string _InReplyToUserID = string.Empty;
        private string _InReplyToScreenName = string.Empty;
        private bool _IsDirectMessage = false;
        private string _Source = string.Empty;
        private bool _Truncated;
        private TwitterUser _User = default;
        private TwitterStatus _RetweetedStatus = null;
        private string _GeoLat = string.Empty;
        private string _GeoLong = string.Empty;

        /// <summary>
        /// The ID of the tweet.
        /// </summary>
        /// <value></value>
        /// <returns>An <c>Int64</c> representing the tweest ID.</returns>
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
        /// The date and time that the tweet was posted.
        /// </summary>
        /// <value></value>
        /// <returns>A <c>DateTime</c> representing the date and time the tweet was posted.</returns>
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
        /// The date and time that the tweet was posted.
        /// </summary>
        /// <value></value>
        /// <returns>A <c>DateTime</c> representing the date and time the tweet was posted.</returns>
        /// <remarks>
        /// This value is local time.
        /// <para/>
        /// This property is read-only because it is generated in TwitterVB, rather than the Twitter API.
        /// </remarks>
        public DateTime CreatedAtLocalTime
        {
            get
            {
                return CreatedAt.ToLocalTime();
            }
        }

        /// <summary>
        /// The text of the tweet.
        /// </summary>
        /// <value></value>
        /// <returns>A <c>string</c> representing the text of the tweet.</returns>
        /// <remarks></remarks>
        public string Text
        {
            get
            {
                return _Text;
            }
            set
            {
                _Text = value;
            }
        }

        /// <summary>
        /// Whether or not this tweet was favorited by the authenticating user.
        /// </summary>
        /// <value></value>
        /// <returns>A <c>boolean</c> indicating </returns>
        /// <remarks></remarks>
        public bool Favorited
        {
            get
            {
                return _Favorited;
            }
            set
            {
                _Favorited = value;
            }
        }

        /// <summary>
        /// The ID of the message to which this tweet is a reply.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>
        /// If this tweet is not a reply, this is zero.
        /// </remarks>
        public long InReplyToStatusID
        {
            get
            {
                return _InReplyToStatusID;
            }
            set
            {
                _InReplyToStatusID = value;
            }
        }

        /// <summary>
        /// The ID of the user to which this tweet is a reply.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>
        /// If this tweet is not a reply, this is zero.
        /// </remarks>
        public string InReplyToUserID
        {
            get
            {
                return _InReplyToUserID;
            }
            set
            {
                _InReplyToUserID = value;
            }
        }

        /// <summary>
        /// The screen name of the user to which this tweet is a reply.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>
        /// If this tweet is not a reply, this is zero.
        /// </remarks>
        public string InReplyToScreenName
        {
            get
            {
                return _InReplyToScreenName;
            }
            set
            {
                _InReplyToScreenName = value;
            }
        }

        /// <summary>
        /// The source of the tweet.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string Source
        {
            get
            {
                return _Source;
            }
            set
            {
                _Source = value;
            }
        }

        /// <summary>
        /// Whether or not the text of the tweet has been truncated.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool Truncated
        {
            get
            {
                return _Truncated;
            }
            set
            {
                _Truncated = value;
            }
        }

        /// <summary>
        /// A <c>TwitterUser</c> object representing the user that posted the tweet.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public TwitterUser User
        {
            get
            {
                return _User;
            }
            set
            {
                _User = value;
            }
        }

        /// <summary>
        /// Indicates whether or not the Tweet is a direct message.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>This property is not populated when the object is created.  It is populated by the methods involved with direct messages.  
        /// It will always be <c>False</c></remarks> unless set to <c>True</c> by an external method.
        public bool IsDirectMessage
        {
            get
            {
                return _IsDirectMessage;
            }
            set
            {
                _IsDirectMessage = value;
            }
        }

        /// <summary>
        /// The original tweet being retweeted.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>
        /// The Twitter API does not populate this on the Home Timeline.
        /// </remarks>
        public TwitterStatus RetweetedStatus
        {
            get
            {
                return _RetweetedStatus;
            }
            set
            {
                _RetweetedStatus = value;
            }
        }

        /// <summary>
        /// The latitude the tweet was sent from
        /// </summary>
        /// <value></value>
        /// <returns>A <c>string</c> representing the latitude the tweet was sent from.</returns>
        /// <remarks></remarks>
        public string GeoLat
        {
            get
            {
                return _GeoLat;
            }
            set
            {
                _GeoLat = value;
            }
        }

        /// <summary>
        /// The longitude the tweet was sent from.
        /// </summary>
        /// <value></value>
        /// <returns>A <c>string</c> representing the longitude the tweet was sent from.</returns>
        /// <remarks></remarks>
        public string GeoLong
        {
            get
            {
                return _GeoLong;
            }
            set
            {
                _GeoLong = value;
            }
        }

        /// <summary>
        /// Creates a new <c>TwitterStatus</c> object.
        /// </summary>
        /// <param name="StatusNode">An <c>XmlNode</c> from the Twitter API response representing a status.</param>
        /// <remarks></remarks>
        /// <exclude/>
        public TwitterStatus(System.Xml.XmlNode StatusNode)
        {

            CreatedAt = XmlDate_Get(StatusNode["created_at"]);
            Favorited = XmlBoolean_Get(StatusNode["favorited"]);
            ID = XmlInt64_Get(StatusNode["id"]);
            InReplyToScreenName = XmlString_Get(StatusNode["in_reply_to_screen_name"]);
            InReplyToStatusID = XmlInt64_Get(StatusNode["in_reply_to_status_id"]);
            InReplyToUserID = XmlString_Get(StatusNode["in_reply_to_user_id"]);
            Source = XmlString_Get(StatusNode["source"]);
            Text = XmlString_Get(StatusNode["text"]);
            Truncated = XmlBoolean_Get(StatusNode["truncated"]);
            if (StatusNode["user"] != null)
            {
                User = new TwitterUser(StatusNode["user"]);
            }

            if (StatusNode["retweeted_status"] != null)
            {
                RetweetedStatus = new TwitterStatus(StatusNode["retweeted_status"]);
            }

            // Geotag parsing
            string GeoTag = XmlString_Get(StatusNode["geo"]);
            if (!string.IsNullOrEmpty(GeoTag))
            {
                string[] LatLongArray = GeoTag.Split(new char[] { ' ' });
                if (LatLongArray.Length == 2)
                {
                    GeoLat = LatLongArray[0];
                    GeoLong = LatLongArray[1];
                }
            }
        }
    }

}
