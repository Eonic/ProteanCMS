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
    /// A Twitter list.
    /// </summary>
    /// <remarks></remarks>
    public class TwitterList : XmlObjectBase
    {

        private long _ID;
        private string _Name = string.Empty;
        private string _FullName = string.Empty;
        private string _Slug = string.Empty;
        private string _Description = string.Empty;
        private long _SubscriberCount;
        private long _Member_Count;
        private string _Url = string.Empty;
        private string _Mode = string.Empty;
        private TwitterUser _User = new TwitterUser();

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <remarks></remarks>
        /// <exclude/>
        public TwitterList()
        {
        }

        /// <summary>
        /// Creates a new <c>List</c> object.
        /// </summary>
        /// <param name="ListNode">An <c>XmlNode</c> from the Twitter API response representing a user.</param>
        /// <remarks></remarks>
        public TwitterList(System.Xml.XmlNode ListNode)
        {
            {
                //ref var withBlock = ref this;
                //withBlock.ID = XmlInt64_Get(ListNode["id"]);
                //withBlock.Name = XmlString_Get(ListNode["name"]);
                //withBlock.FullName = XmlString_Get(ListNode["full_name"]);
                //withBlock.Slug = XmlString_Get(ListNode["slug"]);
                //withBlock.Description = XmlString_Get(ListNode["description"]);
                //withBlock.SubscriberCount = XmlInt64_Get(ListNode["subscriber_count"]);
                //withBlock.MemberCount = XmlInt64_Get(ListNode["member_count"]);
                //withBlock.Mode = XmlString_Get(ListNode["mode"]);
                //withBlock.Url = XmlString_Get(ListNode["url"]);
                //withBlock.User = new TwitterUser(ListNode["user"]);
            }
        }

        /// <summary>
        /// The ID of the list.
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
        /// The name of the list.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
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
        /// The full name of the list.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string FullName
        {
            get
            {
                return _FullName;
            }
            set
            {
                _FullName = value;
            }
        }

        /// <summary>
        /// The slug of the list.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>Slug is an alternate name for the list.</remarks>
        public string Slug
        {
            get
            {
                return _Slug;
            }
            set
            {
                _Slug = value;
            }
        }

        /// <summary>
        /// The description of the list.
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
        /// The number of users who subscribe to the list.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public long SubscriberCount
        {
            get
            {
                return _SubscriberCount;
            }
            set
            {
                _SubscriberCount = value;
            }
        }

        /// <summary>
        /// The number of people who are members of the list.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public long MemberCount
        {
            get
            {
                return _Member_Count;
            }
            set
            {
                _Member_Count = value;
            }
        }

        /// <summary>
        /// The Url where the list can be found.
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
        /// Whether or not the list is public or private.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>"Public" and "Private" are the only valid values.</remarks>
        public string Mode
        {
            get
            {
                return _Mode;
            }
            set
            {
                if (value.ToUpper() == "PUBLIC")
                {
                    _Mode = "Public";
                }
                else if (value.ToUpper() == "PRIVATE")
                {
                    _Mode = "Private";
                }
            }
        }

        /// <summary>
        /// The user who is the owner of the list.
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

    }
}
