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

using System;

namespace Protean.Tools.Integration.Twitter.TwitterVB2.Objects
{
    /// <summary>
    /// An individual search result
    /// </summary>
    /// <remarks>
    /// The search API returns results that are very different than the rest of the API.  Although a <c>TwitterSearchResult</c> does represent a
    /// tweet, it is not the same thing as a <c>TwitterStatus</c>.
    /// </remarks>
    public partial class TwitterSearchResult
    {

        #region Prvate Members
        private string _ID = string.Empty;
        private DateTime _CreatedAt;
        private string _StatusUrl = string.Empty;
        private string _Title = string.Empty;
        private string _Content = string.Empty;
        private string _ProfileImageUrl = string.Empty;
        private string _Source = string.Empty;
        private string _AuthorName = string.Empty;
        private string _AuthorUrl = string.Empty;
        #endregion

        #region Public Properties
        /// <summary>
        /// The ID of the tweet.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public long ID
        {
            get
            {
                string[] Sections = _ID.Split(Convert.ToChar(":"));
                return long.Parse(Sections[2]);
            }
        }

        /// <summary>
        /// The date and time that the tweet was created.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>
        /// This is UTC time.
        /// </remarks>
        public DateTime CreatedAt
        {
            get
            {
                return _CreatedAt;
            }
        }

        /// <summary>
        /// The date and time that the tweet was created.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>
        /// This is local time.
        /// </remarks>
        public DateTime CreatedAtLocalTime
        {
            get
            {
                return _CreatedAt.ToLocalTime();
            }
        }

        /// <summary>
        /// The Url of the tweet.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string StatusUrl
        {
            get
            {
                return _StatusUrl;
            }
        }

        /// <summary>
        /// The actual text of the tweet.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string Title
        {
            get
            {
                return _Title;
            }
        }

        /// <summary>
        /// The text of the tweet, with some items rendered as HTML.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>
        /// Usernames are rendered as links in this text.
        /// </remarks>
        public string Content
        {
            get
            {
                return _Content;
            }
        }

        /// <summary>
        /// The Url of the avatar of the user who posted the tweet.
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
        }

        /// <summary>
        /// The source of the tweet.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>
        /// This is usually the name of the application that posted the tweet.
        /// </remarks>
        public string Source
        {
            get
            {
                return _Source;
            }
        }

        /// <summary>
        /// The screen name of the user who posted the tweet.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string AuthorName
        {
            get
            {
                return _AuthorName;
            }
        }

        /// <summary>
        /// The Url of the Twitter profile of the user who posted the tweet.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string AuthorUrl
        {
            get
            {
                return _AuthorUrl;
            }
        }

        #endregion

        /// <summary>
        /// Creates a <c>TwitterSearchResult</c> object.
        /// </summary>
        /// <param name="SearchResultNode">An <c>XmlNode</c> from the Twitter API response representing a search result.</param>
        /// <remarks></remarks>
        public TwitterSearchResult(System.Xml.XmlNode SearchResultNode)
        {
            _ID = SearchResultNode["id"].InnerText;

            if (SearchResultNode["published"] != null)
            {
                _CreatedAt = DateTime.Parse(SearchResultNode["published"].InnerText);
            }

            var Links = SearchResultNode.SelectNodes("link");
            if (Links[0].Attributes["href"] != null)
            {
                _StatusUrl = Links[0].Attributes["href"].Value;
            }
            if (Links[1].Attributes["href"] != null)
            {
                _ProfileImageUrl = Links[1].Attributes["href"].Value;
            }

            if (SearchResultNode["source"] != null)
            {
                _Source = SearchResultNode["source"].InnerText;
            }

            if (SearchResultNode["author"] != null)
            {
                string AuthorName = SearchResultNode["author"].ChildNodes[0].InnerText;
                _AuthorName = AuthorName.Substring(0, AuthorName.IndexOf(" "));

                _AuthorUrl = SearchResultNode["author"].ChildNodes[1].InnerText;
            }

            if (SearchResultNode["title"] != null)
            {
                _Title = SearchResultNode["title"].InnerText;
            }

            if (SearchResultNode["content"] != null)
            {
                _Content = SearchResultNode["content"].InnerText;
            }
        }
    }
}
