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
    public partial class TwitterDirectMessage : XmlObjectBase
    {

        private long _ID;
        private long _SenderID;
        private string _Text = string.Empty;
        private long _RecipientID;
        private DateTime _CreatedAt;
        private string _SenderScreenName = string.Empty;
        private string _RecipientScreenName = string.Empty;
        private TwitterUser _Sender = new TwitterUser();
        private TwitterUser _Recipient = new TwitterUser();


        /// <summary>
        /// The ID of the direct message.
        /// </summary>
        /// <value></value>
        /// <returns>An <c>Int64</c> representing the direct message ID.</returns>
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
        /// The ID of the user who sent the direct message.
        /// </summary>
        /// <value></value>
        /// <returns>An <c>Int64</c> representing the sender's user ID.</returns>
        /// <remarks></remarks>
        public long SenderID
        {
            get
            {
                return _SenderID;
            }
            set
            {
                _SenderID = value;
            }
        }

        /// <summary>
        /// The text of the direct message.
        /// </summary>
        /// <value></value>
        /// <returns>A <c>String</c> representing the direct message.</returns>
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
        /// THe ID of the user to whom the message is being sent.
        /// </summary>
        /// <value></value>
        /// <returns>An <c>Int64</c> representing the recipient's user ID.</returns>
        /// <remarks></remarks>
        public long RecipientID
        {
            get
            {
                return _RecipientID;
            }
            set
            {
                _RecipientID = value;
            }
        }

        /// <summary>
        /// The date and time that the direct message was posted.
        /// </summary>
        /// <value></value>
        /// <returns>A <c>DateTime</c> representing the date and time the direct message was posted.</returns>
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
        /// The date and time that the direct message was posted.
        /// </summary>
        /// <value></value>
        /// <returns>A <c>DateTime</c> representing the date and time the direct message was posted.</returns>
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
        /// The screen name of the user sending the direct message.
        /// </summary>
        /// <value></value>
        /// <returns>A <c>String</c> representing the screen name of the user who sent the message.</returns>
        /// <remarks></remarks>
        public string SenderScreenName
        {
            get
            {
                return _SenderScreenName;
            }
            set
            {
                _SenderScreenName = value;
            }
        }

        /// <summary>
        /// The screen name of the user receiving the direct message.
        /// </summary>
        /// <value></value>
        /// <returns>A <c>String</c> representing the screen name of the user who received the message.</returns>
        /// <remarks></remarks>
        public string RecipientScreenName
        {
            get
            {
                return _RecipientScreenName;
            }
            set
            {
                _RecipientScreenName = value;
            }
        }

        /// <summary>
        /// The user sending the direct message.
        /// </summary>
        /// <value></value>
        /// <returns>A <c>TwitterUser</c> object representing the user who sent the message.</returns>
        /// <remarks></remarks>
        public TwitterUser Sender
        {
            get
            {
                return _Sender;
            }
            set
            {
                _Sender = value;
            }
        }

        /// <summary>
        /// The user receiving the direct message.
        /// </summary>
        /// <value></value>
        /// <returns>A <c>TwitterUser</c> object representing the user who received the message.</returns>
        /// <remarks></remarks>
        public TwitterUser Recipient
        {
            get
            {
                return _Recipient;
            }
            set
            {
                _Recipient = value;
            }
        }

        /// <summary>
        /// Creates a new <c>DirectMessage</c> object.
        /// </summary>
        /// <param name="DirectMessageNode">An <c>XmlNode</c> from the Twitter API response representing a direct message.</param>
        /// <remarks></remarks>
        /// <exclude/>
        public TwitterDirectMessage(System.Xml.XmlNode DirectMessageNode)
        {
            ID = XmlInt64_Get(DirectMessageNode["id"]);
            SenderID = XmlInt64_Get(DirectMessageNode["sender_id"]);
            Text = XmlString_Get(DirectMessageNode["text"]);
            RecipientID = XmlInt64_Get(DirectMessageNode["recipient_id"]);
            CreatedAt = XmlDate_Get(DirectMessageNode["created_at"]);
            SenderScreenName = XmlString_Get(DirectMessageNode["sender_screen_name"]);
            RecipientScreenName = XmlString_Get(DirectMessageNode["recipient_screen_name"]);

            if (DirectMessageNode["sender"] != null)
            {
                Sender = new TwitterUser(DirectMessageNode["sender"]);
            }

            if (DirectMessageNode["recipient"] != null)
            {
                Recipient = new TwitterUser(DirectMessageNode["recipient"]);
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <remarks></remarks>
        /// <exclude/>
        public TwitterDirectMessage()
        {
        }

    }
}
