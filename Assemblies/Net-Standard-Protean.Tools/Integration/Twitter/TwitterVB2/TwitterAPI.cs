using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using Microsoft.VisualBasic; // Install-Package Microsoft.VisualBasic
using Protean.Tools.Integration.Twitter.TwitterVB2;
using Protean.Tools.Integration.Twitter.TwitterVB2.Objects;
using static Protean.Tools.Integration.Twitter.Globals;

namespace Protean.Tools.Integration.Twitter.TwitterVB2
{
    /// <summary>
    /// Provides access to methods that communicate with Twitter.
    /// </summary>
    /// <remarks></remarks>
    public partial class TwitterAPI
    {
        private TwitterOAuth Twitter_OAuth = new TwitterOAuth();
        private string Username = string.Empty;
        private string Password = string.Empty;
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <remarks></remarks>
        /// <exclude/>
        public TwitterAPI()
        {
        }


        #region Twitter API Method Constants
        // As Twitter begins rolling out changes to the API, some of these Urls will change.
        // Having them all in one place makes it easier to change them moving forward.
        private const string ACCOUNT_VERIFY_CREDENTIALS = "http://api.twitter.com/1/account/verify_credentials.xml";
        private const string BLOCKS_BLOCKING = "http://api.twitter.com/1/blocks/blocking.xml";
        private const string BLOCKS_BLOCKING_IDS = "http://api.twitter.com/1/blocks/blocking/ids.xml";
        private const string BLOCKS_CREATE = "http://api.twitter.com/1/blocks/create/{0}.xml";
        private const string BLOCKS_DESTROY = "http://api.twitter.com/1/blocks/destroy/{0}.xml";
        private const string DIRECT_MESSAGES = "http://api.twitter.com/1/direct_messages.xml";
        private const string DIRECT_MESSAGES_DESTROY = "http://api.twitter.com/1/direct_messages/destroy/{0}.xml";
        private const string DIRECT_MESSAGES_NEW = "http://api.twitter.com/1/direct_messages/new.xml";
        private const string DIRECT_MESSAGES_SENT = "http://api.twitter.com/1/direct_messages/sent.xml";
        private const string FAVORITES_CREATE = "http://api.twitter.com/1/favorites/create/{0}.xml";
        private const string FAVORITES_DESTROY = "http://api.twitter.com/1/favorites/destroy/{0}.xml";
        private const string FAVORITES_LIST = "http://api.twitter.com/1/favorites.xml";
        private const string FOLLOWERS_URL = "http://api.twitter.com/1/followers/ids.xml";
        private const string FRIENDS_URL = "http://api.twitter.com/1/friends/ids.xml";
        private const string FRIENDSHIP_CREATE = "http://api.twitter.com/1/friendships/create/{0}.xml";
        private const string FRIENDSHIP_SHOW = "http://api.twitter.com/1/friendships/show.xml?{0}={1}&{2}={3}";
        private const string FRIENDSHIP_DESTROY = "http://api.twitter.com/1/friendships/destroy/{0}.xml";
        private const string LISTS = "http://api.twitter.com/1/{0}/lists.xml";
        private const string LISTS_MEMBERS = "http://api.twitter.com/1/{0}/{1}/members.xml";
        private const string LISTS_MEMBERS_ID = "http://api.twitter.com/1/{0}/{1}/members/{2}.xml";
        private const string LISTS_MEMBERSHIPS = "http://api.twitter.com/1/{0}/lists/memberships.xml";
        private const string LISTS_SPECIFY = "http://api.twitter.com/1/{0}/lists/{1}.xml";
        private const string LISTS_STATUSES = "http://api.twitter.com/1/{0}/lists/{1}/statuses.xml";
        private const string LISTS_SUBSCRIBERS = "http://api.twitter.com/1/{0}/{1}/subscribers.xml";
        private const string LISTS_SUBSCRIBERS_ID = "http://api.twitter.com/1/{0}/{1}/subscribers/{2}.xml";
        private const string LISTS_SUBSCRIPTIONS = "http://api.twitter.com/1/{0}/lists/subscriptions.xml";
        private const string REPORT_SPAM = "http://api.twitter.com/1/report_spam.xml";
        private const string SEARCH_URL = "http://search.twitter.com/search.atom";
        private const string STATUSES_DESTROY = "http://api.twitter.com/1/statuses/destroy/{0}.xml";
        private const string STATUSES_FRIENDS = "http://api.twitter.com/1/statuses/friends.xml";
        private const string STATUSES_FOLLOWERS = "http://api.twitter.com/1/statuses/followers.xml";
        private const string STATUSES_FRIENDS_TIMELINE = "http://api.twitter.com/1/statuses/friends_timeline.xml";
        private const string STATUSES_HOME_TIMELINE = "http://api.twitter.com/1/statuses/home_timeline.xml";
        private const string STATUSES_MENTIONS = "http://api.twitter.com/1/statuses/mentions.xml";
        private const string STATUSES_PUBLIC_TIMELINE = "http://api.twitter.com/1/statuses/public_timeline.xml";
        private const string STATUSES_REPLIES = "http://api.twitter.com/1/statuses/replies.xml";
        private const string STATUSES_RETWEET = "http://api.twitter.com/1/statuses/retweet/{0}.xml";
        private const string STATUSES_RETWEETED_BY_ME = "http://api.twitter.com/1/statuses/retweeted_by_me.xml";
        private const string STATUSES_RETWEETED_TO_ME = "http://twitter.com/statuses/retweeted_to_me.xml";
        private const string STATUSES_RETWEETS = "http://api.twitter.com/1/statuses/retweets/id.xml";
        private const string STATUSES_RETWEETS_OF_ME = "http://api.twitter.com/1/statuses/retweets_of_me.xml";
        private const string STATUSES_SHOW = "http://api.twitter.com/1/statuses/show/{0}.xml";
        private const string STATUSES_UPDATE = "http://api.twitter.com/1/statuses/update.xml";
        private const string STATUSES_USER_TIMELINE = "http://api.twitter.com/1/statuses/user_timeline.xml";
        private const string TRENDS_CURRENT = "http://search.twitter.com/trends/current.json";
        private const string TRENDS_DAILY = "http://search.twitter.com/trends/daily.json";
        private const string TRENDS_URL = "http://search.twitter.com/trends.json";
        private const string TRENDS_WEEKLY = "http://search.twitter.com/trends/weekly.json";
        private const string TREND_LOCATIONS = "http://api.twitter.com/1/trends/available.xml";
        private const string TREND_LOCATION_TRENDS = "http://api.twitter.com/1/trends/{0}.json";
        private const string USERS_SEARCH = "http://api.twitter.com/1/users/search.xml";
        private const string USERS_SHOW = "http://api.twitter.com/1/users/show.xml";
        #endregion

        #region API Rate Limit Properties
        /// <summary>
        /// The number of API calls remaining in the current API period.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string RateLimit_RemainingHits
        {
            get
            {
                return Globals.API_RemainingHits;
            }
        }

        /// <summary>
        /// The total number of API calls allowed in the current API period.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string RateLimit_HourlyLimit
        {
            get
            {
                return Globals.API_HourlyLimit;
            }
        }

        /// <summary>
        /// The time when the next API period starts and <c>RemainingHits</c> will reset.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string RateLimit_Reset
        {
            get
            {
                return Globals.API_Reset;
            }
        }
        #endregion

        #region OAuth Properties
        /// <summary>
        /// The OAuth Token generated by the OAuth process.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string OAuth_Token
        {
            get
            {
                return Twitter_OAuth.Token;
            }
        }

        /// <summary>
        /// The OAuth Token Secret generated by the OAuth process.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string OAuth_TokenSecret
        {
            get
            {
                return Twitter_OAuth.TokenSecret;
            }
        }

        #endregion

        #region Proxy Authentication Properties
        /// <summary>
        /// The username that will be passed to the default proxy server.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string ProxyUsername
        {
            get
            {
                return Globals.Proxy_Username;
            }
            set
            {
                Globals.Proxy_Username = value;
            }
        }

        /// <summary>
        /// The password that will be passed to the default proxy server.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string ProxyPassword
        {
            get
            {
                return Globals.Proxy_Password;
            }
            set
            {
                Globals.Proxy_Password = value;
            }
        }
        #endregion

        #region Authentication Methods


        // Public Sub AuthenticateAs(ByVal Username As String, ByVal Password As String)
        // Me.Twitter_OAuth = Nothing
        // Me.Username = Username
        // Me.Password = Password
        // End Sub


        #endregion

        #region Xauth
        public void XAuth(string p_strUserName, string p_strPassword, string p_strConsumer, string p_strCKSecret)
        {
            Twitter_OAuth.ConsumerKey = p_strConsumer;
            Twitter_OAuth.ConsumerSecret = p_strCKSecret;

            Twitter_OAuth.GetXAccess(p_strUserName, p_strPassword);
            // If Twitter_OAuth.Token.Length > 0 Then
            // MsgBox("Success")
            // Else
            // MsgBox("Failure")
            // End If

        }
        #endregion

        #region Support Methods

        // 
        // This region holds general support and utility methods.
        // They are explicitly excluded from the API documentation.
        // 

        /// <exclude/>
        private string PerformWebRequest(string Url)
        {
            string ReturnValue = string.Empty;

            HttpWebRequest Request = (HttpWebRequest)System.Net.WebRequest.Create(new Uri(Url));
            Request.Method = "GET";
            Request.MaximumAutomaticRedirections = 4;
            Request.MaximumResponseHeadersLength = 4;
            Request.ContentLength = 0;

            try
            {
                string ResponseText = string.Empty;
                using (HttpWebResponse Response = (HttpWebResponse)Request.GetResponse())
                {
                    using (Stream ReceiveStream = Response.GetResponseStream())
                    {
                        using (var ReadStream = new StreamReader(ReceiveStream, Encoding.UTF8))
                        {
                            ReturnValue = ReadStream.ReadToEnd();
                        }
                    }
                }
            }
            catch
            {
                ReturnValue = string.Empty;
            }

            return ReturnValue;
        }

        /// <exclude/>
        private string PerformWebRequest(string Url, string HTTPMethod)
        {
            string ReturnValue = string.Empty;
            string AuthType = string.Empty;

            if (Twitter_OAuth != null)
            {
                // This is an OAuth request
                AuthType = "OAUTH";
                TwitterOAuth.Method OAuthMethod;
                if (HTTPMethod.ToUpper() == "GET")
                {
                    OAuthMethod = TwitterOAuth.Method.GET;
                }
                else
                {
                    OAuthMethod = TwitterOAuth.Method.POST;
                }

                ReturnValue = Twitter_OAuth.OAuthWebRequest(OAuthMethod, Url, string.Empty);
            }
            else
            {
                try
                {
                    // This is a Basic Auth request
                    AuthType = "BASIC";
                    HttpWebRequest Request = (HttpWebRequest)System.Net.WebRequest.Create(Url);
                    Request.Method = HTTPMethod;
                    Request.MaximumAutomaticRedirections = 4;
                    Request.MaximumResponseHeadersLength = 4;
                    Request.ContentLength = 0;
                    Request.Credentials = new NetworkCredential(Username, Password);

                    HttpWebResponse Response = (HttpWebResponse)Request.GetResponse();
                    Globals.API_HourlyLimit = Response.GetResponseHeader("X-RateLimit-Limit");
                    Globals.API_RemainingHits = Response.GetResponseHeader("X-RateLimit-Remaining");
                    Globals.API_Reset = Response.GetResponseHeader("X-RateLimit-Reset");
                    using (Stream ReceiveStream = Response.GetResponseStream())
                    {
                        using (var ReadStream = new StreamReader(ReceiveStream, Encoding.UTF8))
                        {
                            ReturnValue = ReadStream.ReadToEnd();
                        }
                    }
                    Response.Close();
                }

                catch (Exception ex)
                {
                    string Message = null;
                    if (ex is WebException)
                    {
                        try
                        {
                            var Doc = new XmlDocument();
                            ReturnValue = new StreamReader(((WebException)ex).Response.GetResponseStream(), Encoding.UTF8).ReadToEnd();
                            Doc.LoadXml(ReturnValue);
                            Message = Doc.SelectSingleNode("hash/error").InnerText;
                        }
                        catch
                        {
                        }
                    }
                    var tax = new TwitterAPIException(Message, ex);
                    tax.Url = Url;
                    tax.Method = HTTPMethod;
                    tax.AuthType = AuthType;
                    tax.Response = null;
                   // tax.Status=null;
                    throw tax;
                }
            }

            return ReturnValue;
        }

        /// <exclude/>
        private int EndingQuote(string Text, int Start)
        {
            int q = Start;
            do
            {
                q = Text.IndexOf('"', q);
                if (q > 0 && Text.Substring(q - 1, 1) != @"\")
                {
                    return q;
                }
                q += 1;
            }
            while (q < Text.Length);
            return default;
        }

        /// <exclude/>
        private string GetNextQuotedString(string Text, ref int Position)
        {

            int q1;
            int q2;

            q1 = Text.IndexOf('"', Position);
            Position = q1 + 1;

            if (q1 == -1)
            {
                return string.Empty;
            }
            do
            {
                q2 = Text.IndexOf('"', Position);
                Position = q2 + 1;
            }
            while (Text.Substring(q2 - 1, 1) == @"\");

            return Text.Substring(q1 + 1, q2 - q1 - 1);

        }

        /// <exclude/>
        private string CleanJsonText(string Text)
        {
            var sb = new StringBuilder(Text);
            sb.Replace(@"\""", "\"");
            sb.Replace(@"\\", @"\");
            sb.Replace(@"\/", "/");
            return sb.ToString();
        }

        /// <exclude/>
        private string GetImageContentType(string Filename)
        {
            if (Filename.ToUpper().EndsWith(".GIF"))
            {
                return "image/gif";
            }
            else if (Filename.ToUpper().EndsWith(".JPG"))
            {
                return "image/jpeg";
            }
            else if (Filename.ToUpper().EndsWith(".JPEG"))
            {
                return "image/jpeg";
            }
            else if (Filename.ToUpper().EndsWith(".PNG"))
            {
                return "image/png";
            }
            else
            {
                return string.Empty;
            }
        }

        #endregion

        #region Parsing Methods

        // 
        // The methods in this region are internal functions for parsing Twitter responses.
        // They are private and explicitly excluded from the API documentation.
        // 

        /// <exclude/>
        private List<TwitterRelationship> ParseRelationships(string Xml)
        {
            var XmlDoc = new XmlDocument();
            XmlDoc.LoadXml(Xml);

            var Relationships = new List<TwitterRelationship>();
            foreach (XmlNode RelationshipNode in XmlDoc.SelectNodes("//relationship"))
                Relationships.Add(new TwitterRelationship(RelationshipNode));

            return Relationships;
        }

        /// <exclude/>
        private List<TwitterDirectMessage> ParseDirectMessages(string Xml)
        {
            var XmlDoc = new XmlDocument();
            XmlDoc.LoadXml(Xml);

            var Messages = new List<TwitterDirectMessage>();
            foreach (XmlNode DirectMessageNode in XmlDoc.SelectNodes("//direct_message"))
                Messages.Add(new TwitterDirectMessage(DirectMessageNode));

            return Messages;
        }

        /// <exclude/>
        private List<TwitterList> ParseLists(string Xml)
        {
            var XmlDoc = new XmlDocument();
            XmlDoc.LoadXml(Xml);

            var Lists = new List<TwitterList>();
            foreach (XmlNode ListNode in XmlDoc.SelectNodes("//list"))
                Lists.Add(new TwitterList(ListNode));
            return Lists;
        }

        /// <exclude/>
        private List<TwitterList> ParseLists(string Xml, ref long NextCursor)
        {
            var XmlDoc = new XmlDocument();
            XmlDoc.LoadXml(Xml);

            var Lists = new List<TwitterList>();
            foreach (XmlNode ListNode in XmlDoc.SelectNodes("//list"))
                Lists.Add(new TwitterList(ListNode));

            NextCursor = Convert.ToInt64(XmlDoc.SelectSingleNode("//next_cursor").InnerText);
            return Lists;
        }

        /// <exclude/>
        private List<TwitterStatus> ParseStatuses(string Xml)
        {
            var XmlDoc = new XmlDocument();
            XmlDoc.LoadXml(Xml);

            var Stripper = new XmlNamespaceStripper(XmlDoc);
            XmlDoc = Stripper.StrippedXmlDocument;

            var Statuses = new List<TwitterStatus>();
            foreach (XmlNode StatusNode in XmlDoc.SelectNodes("//status"))
                Statuses.Add(new TwitterStatus(StatusNode));
            return Statuses;
        }

        /// <exclude/>
        private List<TwitterUser> ParseUsers(string Xml)
        {
            var XmlDoc = new XmlDocument();
            XmlDoc.LoadXml(Xml);

            var Users = new List<TwitterUser>();
            foreach (XmlNode UserNode in XmlDoc.SelectNodes("//user"))
                Users.Add(new TwitterUser(UserNode));
            return Users;
        }

        /// <exclude/>
        private List<TwitterUser> ParseUsers(string Xml, ref long NextCursor)
        {
            var XmlDoc = new XmlDocument();
            XmlDoc.LoadXml(Xml);

            var Users = new List<TwitterUser>();
            foreach (XmlNode UserNode in XmlDoc.SelectNodes("//user"))
                Users.Add(new TwitterUser(UserNode));

            NextCursor = Convert.ToInt64(XmlDoc.SelectSingleNode("//next_cursor").InnerText);
            return Users;

        }

        /// <exclude/>
        private List<long> ParseBlockedIDs(string Xml)
        {
            var XmlDoc = new XmlDocument();
            XmlDoc.LoadXml(Xml);

            var IDs = new List<long>();
            foreach (XmlNode IDNode in XmlDoc.SelectNodes("//id"))
                IDs.Add(long.Parse(IDNode.InnerText));
            return IDs;
        }

        /// <exclude/>
        private List<TwitterSearchResult> ParseSearchResults(string Xml)
        {
            var XmlDoc = new XmlDocument();
            XmlDoc.LoadXml(Xml);

            var Stripper = new XmlNamespaceStripper(XmlDoc);
            XmlDoc = Stripper.StrippedXmlDocument;

            var Results = new List<TwitterSearchResult>();
            foreach (XmlNode SearchResultNode in XmlDoc.SelectNodes("//entry"))
                Results.Add(new TwitterSearchResult(SearchResultNode));
            return Results;
        }

        /// <exclude/>
        private List<long> ParseSocialGraph(string Xml, ref long NextCursor)
        {
            var XmlDoc = new XmlDocument();
            XmlDoc.LoadXml(Xml);

            var SocialGraph = new List<long>();
            foreach (XmlNode IDNode in XmlDoc.SelectNodes("//id"))
                SocialGraph.Add(Convert.ToInt64(IDNode.InnerText));

            NextCursor = Convert.ToInt64(XmlDoc.SelectSingleNode("//next_cursor").InnerText);
            return SocialGraph;
        }

        private List<TwitterTrendLocation> ParseTrendLocations(string Xml)
        {
            var XmlDoc = new XmlDocument();
            XmlDoc.LoadXml(Xml);

            var Stripper = new XmlNamespaceStripper(XmlDoc);
            XmlDoc = Stripper.StrippedXmlDocument;

            var Results = new List<TwitterTrendLocation>();
            foreach (XmlNode SearchResultNode in XmlDoc.SelectNodes("//location"))
                Results.Add(new TwitterTrendLocation(SearchResultNode));
            return Results;
        }

        /// <exclude/>
        private List<TwitterTrend> ParseTrends(string Json, Globals.TrendFormat TrendType)
        {

            var ReturnValue = new List<TwitterTrend>();
            var AsOf = default(DateTime);
            var Position = default(int);
            string TrendName = string.Empty;
            string TrendText = string.Empty;

            switch (TrendType)
            {

                case var @case when @case == TrendFormat.Trends:
                    {
                        do
                        {
                            string Text = GetNextQuotedString(Json, ref Position);
                            bool exitDo = false;
                            switch (Text ?? "")
                            {
                                case "name":
                                    {
                                        TrendName = GetNextQuotedString(Json, ref Position);
                                        break;
                                    }

                                case "url":
                                    {
                                        TrendText = CleanJsonText(GetNextQuotedString(Json, ref Position));
                                        ReturnValue.Add(new TwitterTrend(TrendName, TrendText));
                                        break;
                                    }

                                case "as_of":
                                    {
                                        string AsOfString = GetNextQuotedString(Json, ref Position);
                                        string DateString = Strings.Replace(AsOfString, ",", "");
                                        var re = new Regex("(?<DayName>[^ ]+) (?<Day>[^ ]{1,2}) (?<MonthName>[^ ]+) (?<Year>[0-9]{4}) (?<Hour>[0-9]{1,2}):(?<Minute>[0-9]{1,2}):(?<Second>[0-9]{1,2}) (?<TimeZone>[+-][0-9]{4})");
                                        var CreatedAt = re.Match(DateString);
                                        AsOf = DateTime.Parse(string.Format("{0} {1} {2} {3}:{4}:{5}", CreatedAt.Groups["MonthName"].Value, CreatedAt.Groups["Day"].Value, CreatedAt.Groups["Year"].Value, CreatedAt.Groups["Hour"].Value, CreatedAt.Groups["Minute"].Value, CreatedAt.Groups["Second"].Value));





                                        exitDo = true;
                                        break;
                                    }
                            }

                            if (exitDo)
                            {
                                break;
                            }
                        }
                        while (true);

                        foreach (TwitterTrend t in ReturnValue)
                            t.AsOf = AsOf;
                        break;
                    }

                case var case1 when case1 == TrendFormat.Current:
                    {
                        do
                        {
                            string Text = GetNextQuotedString(Json, ref Position);
                            bool exitDo1 = false;
                            switch (Text ?? "")
                            {
                                case "trends":
                                    {
                                        AsOf = Convert.ToDateTime(GetNextQuotedString(Json, ref Position));
                                        break;
                                    }

                                case "name":
                                    {
                                        TrendName = GetNextQuotedString(Json, ref Position);
                                        break;
                                    }

                                case "query":
                                    {
                                        TrendText = CleanJsonText(GetNextQuotedString(Json, ref Position));
                                        var NewTrend = new TwitterTrend(TrendName, TrendText);
                                        NewTrend.AsOf = AsOf;
                                        ReturnValue.Add(NewTrend);
                                        break;
                                    }

                                case var case2 when case2 == "":
                                    {
                                        exitDo1 = true;
                                        break;
                                    }
                            }

                            if (exitDo1)
                            {
                                break;
                            }
                        }
                        while (true);
                        break;
                    }

                case var case3 when case3 == TrendFormat.Daily:
                    {
                        do
                        {
                            string Text = GetNextQuotedString(Json, ref Position);
                            bool exitDo2 = false;
                            switch (Text ?? "")
                            {
                                case "name":
                                    {
                                        TrendName = GetNextQuotedString(Json, ref Position);
                                        break;
                                    }

                                case "query":
                                    {
                                        TrendText = CleanJsonText(GetNextQuotedString(Json, ref Position));
                                        var NewTrend = new TwitterTrend(TrendName, TrendText);
                                        NewTrend.AsOf = AsOf;
                                        ReturnValue.Add(NewTrend);
                                        break;
                                    }

                                case var case4 when case4 == "":
                                    {
                                        exitDo2 = true;
                                        break;
                                    }

                                default:
                                    {
                                        DateTime ao;
                                        if (DateTime.TryParse(Text, out ao))
                                        {
                                            AsOf = ao;
                                        }

                                        break;
                                    }
                            }

                            if (exitDo2)
                            {
                                break;

                            }
                        }
                        while (true);
                        break;
                    }

                case var case5 when case5 == TrendFormat.Weekly:
                    {
                        do
                        {
                            string Text = GetNextQuotedString(Json, ref Position);
                            bool exitDo3 = false;
                            switch (Text ?? "")
                            {
                                case "name":
                                    {
                                        TrendName = GetNextQuotedString(Json, ref Position);
                                        break;
                                    }

                                case "query":
                                    {
                                        TrendText = CleanJsonText(GetNextQuotedString(Json, ref Position));
                                        var NewTrend = new TwitterTrend(TrendName, TrendText);
                                        NewTrend.AsOf = AsOf;
                                        ReturnValue.Add(NewTrend);
                                        break;
                                    }

                                case var case6 when case6 == "":
                                    {
                                        exitDo3 = true;
                                        break;
                                    }

                                default:
                                    {
                                        DateTime ao;
                                        if (DateTime.TryParse(Text, out ao))
                                        {
                                            AsOf = ao;
                                        }

                                        break;
                                    }
                            }

                            if (exitDo3)
                            {
                                break;

                            }
                        }
                        while (true);
                        break;
                    }
                case var case7 when case7 == TrendFormat.ByLocation:
                    {
                        do
                        {
                            string Text = GetNextQuotedString(Json, ref Position);
                            bool exitDo4 = false;
                            switch (Text ?? "")
                            {
                                case "as_of":
                                    {
                                        AsOf = Convert.ToDateTime(GetNextQuotedString(Json, ref Position));
                                        exitDo4 = true;
                                        break;
                                    }

                                case var case8 when case8 == "":
                                    {
                                        AsOf = DateTime.Now;
                                        exitDo4 = true;
                                        break;
                                    }
                            }

                            if (exitDo4)
                            {
                                break;
                            }
                        }
                        while (true);

                        Position = 0;
                        do
                        {
                            string Text = GetNextQuotedString(Json, ref Position);
                            bool exitDo5 = false;
                            switch (Text ?? "")
                            {
                                case "as_of":
                                    {
                                        exitDo5 = true;
                                        break;
                                    }
                                case "name":
                                    {
                                        TrendName = GetNextQuotedString(Json, ref Position);

                                        var NewTrend = new TwitterTrend(TrendName, TrendText);

                                        NewTrend.AsOf = AsOf;
                                        ReturnValue.Add(NewTrend);
                                        break;
                                    }
                                case "url":
                                    {
                                        TrendText = CleanJsonText(GetNextQuotedString(Json, ref Position));
                                        break;
                                    }

                                case var case9 when case9 == "":
                                    {
                                        exitDo5 = true;
                                        break;
                                    }
                            }

                            if (exitDo5)
                            {
                                break;

                            }
                        }
                        while (true);
                        break;
                    }
            }

            return ReturnValue;

        }
        #endregion

        #region Twitter API Methods

        // 
        // The methods in this region implement the methods documented in the Twitter API Documentation found at
        // http://apiwiki.twitter.com/Twitter-API-Documentation
        // 
        // These methods have been grouped into regions that mirrors the groupings in the API documentation so that
        // it is easier to find the method you are looking for (or where to put the method that you are writing).
        // 
        // The only methods that should be in this region are methods that directly implement a Twitter API method.
        // If in doubt, ask Duane. :)
        // 

        #region Search API Methods
        /// <summary>
        /// Performs a basic Twitter search for the provided text.
        /// </summary>
        /// <param name="SearchTerm">The text for which to search.</param>
        /// <returns>A <c>List(Of TwitterSearchResult)</c> representing the tweets returned by the search.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Basic Search" lang="vbnet" title="Performing a search"></code>
        /// </remarks>
        public List<TwitterSearchResult> Search(string SearchTerm)
        {
            var Parameters = new TwitterSearchParameters();
            Parameters.Add(TwitterSearchParameterNames.SearchTerm, SearchTerm);

            string Url = Parameters.BuildUrl(SEARCH_URL);
            return ParseSearchResults(PerformWebRequest(Url, "GET"));
        }

        /// <summary>
        /// Performs an advanced Twitter search.
        /// </summary>
        /// <param name="Parameters">A <seealso>TwitterSearchParameters</seealso> object that defines the search.</param>
        /// <returns>A <c>List(Of TwitterSearchResult)</c> that represents the list of tweets found by the search.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Advanced Search" lang="vbnet" title="Performing a search"></code>
        /// </remarks>
        public List<TwitterSearchResult> Search(TwitterSearchParameters Parameters)
        {
            string Url = Parameters.BuildUrl(SEARCH_URL);
            return ParseSearchResults(PerformWebRequest(Url, "GET"));
        }

        /// <summary>
        /// Returns the top ten topics that are currently trending on Twitter.  The response includes the time of the request, 
        /// the name of each trend, and the url to the Twitter Search results page for that topic.
        /// </summary>
        /// <returns>A <c>List(Of TwitterTrend)</c> representing the trends.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Trends" lang="vbnet" title="Retrieving trend information"></code>
        /// </remarks>
        public List<TwitterTrend> Trends()
        {
            return ParseTrends(PerformWebRequest(TRENDS_URL, "GET"), TrendFormat.Trends);
        }

        /// <summary>
        /// Returns the current top 10 trending topics on Twitter.  The response includes the time of the request, 
        /// the name of each trending topic, and query used on Twitter Search results page for that topic.
        /// </summary>
        /// <returns>A <c>List(Of TwitterTrend)</c> representing the trends.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Trends" lang="vbnet" title="Retrieving trend information"></code>
        /// </remarks>
        public List<TwitterTrend> TrendsCurrent()
        {
            return ParseTrends(PerformWebRequest(TRENDS_CURRENT, "GET"), TrendFormat.Current);
        }

        /// <summary>
        /// Returns the top 20 trending topics for each hour in a given day.
        /// </summary>
        /// <returns>A <c>List(Of TwitterTrend)</c> representing the trends.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Trends" lang="vbnet" title="Retrieving trend information"></code>
        /// </remarks>
        public List<TwitterTrend> TrendsDaily()
        {
            return ParseTrends(PerformWebRequest(TRENDS_DAILY, "GET"), TrendFormat.Daily);
        }

        /// <summary>
        /// Returns the top 30 trending topics for each day in a given week.
        /// </summary>
        /// <returns>A <c>List(Of TwitterTrend)</c> representing the trends.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Trends" lang="vbnet" title="Retrieving trend information"></code>
        /// </remarks>
        public List<TwitterTrend> TrendsWeekly()
        {
            return ParseTrends(PerformWebRequest(TRENDS_WEEKLY, "GET"), TrendFormat.Weekly);
        }
        #endregion

        #region Timeline Methods
        /// <summary>
        /// Gets the 20 most recent tweets from the public timeline
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Get Public Timeline" lang="vbnet" title="Getting the public timeline"></code>
        /// </remarks>
        public List<TwitterStatus> PublicTimeline()
        {
            return PublicTimeline(default);
        }

        /// <summary>
        /// Gets recent tweets from the public timeline
        /// </summary>
        /// <param name="Parameters">A <see>TwitterParameters</see> object defining how the request should be executed.</param>
        /// <returns></returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Get Public Timeline" lang="vbnet" title="Getting the public timeline"></code>
        /// </remarks>
        public List<TwitterStatus> PublicTimeline(TwitterParameters Parameters)
        {
            string Url = STATUSES_PUBLIC_TIMELINE;
            if (Parameters != null)
            {
                Url = Parameters.BuildUrl(Url);
            }
            return ParseStatuses(PerformWebRequest(Url, "GET"));
        }

        /// <summary>
        /// Retrieves tweets from the authenticated user and the authenticated user's friends
        /// </summary>
        /// <returns>A <c>List(Of TwitterStatus)</c> containing the requested tweets.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Get Home Timeline" lang="vbnet" title="Getting the home timeline"></code>
        /// When Twitter publishes the "retweet" changes to the API, this will replace <c>FriendsTimeline()</c>.
        /// See <a href="http://apiwiki.twitter.com/Twitter-REST-API-Method:-statuses-home_timeline">http://apiwiki.twitter.com/Twitter-REST-API-Method:-statuses-home_timeline</a> for details.
        /// </remarks>
        public List<TwitterStatus> HomeTimeline()
        {
            return HomeTimeline(default);
        }

        /// <summary>
        /// Retrieves tweets from the authenticated user and the authenticated user's friends
        /// </summary>
        /// <param name="Parameters">A <see>TwitterParameters</see> object defining how the request should be executed.</param>
        /// <returns>A <c>List(Of TwitterStatus)</c> containing the requested tweets.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Get Home Timeline" lang="vbnet" title="Getting the home timeline"></code>
        /// When Twitter publishes the "retweet" changes to the API, this will replace <c>FriendsTimeline()</c>.
        /// See <a href="http://apiwiki.twitter.com/Twitter-REST-API-Method:-statuses-home_timeline">http://apiwiki.twitter.com/Twitter-REST-API-Method:-statuses-home_timeline</a> for details.
        /// </remarks>
        public List<TwitterStatus> HomeTimeline(TwitterParameters Parameters)
        {
            string Url = STATUSES_HOME_TIMELINE;
            if (Parameters != null)
            {
                Url = Parameters.BuildUrl(Url);
            }
            return ParseStatuses(PerformWebRequest(Url, "GET"));
        }

        /// <summary>
        /// Retrieves tweets from the authenticated user and the authenticated user's friends
        /// </summary>
        /// <returns>A <c>List(Of TwitterStatus)</c> containing the requested tweets.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Get Friends Timeline" lang="vbnet" title="Getting the friends timeline"></code>
        /// </remarks>
        public List<TwitterStatus> FriendsTimeline()
        {
            return FriendsTimeline(default);
        }

        /// <summary>
        /// Retrieves tweets from the authenticated user and the authenticated user's friends
        /// </summary>
        /// <param name="Parameters">A <see>TwitterParameters</see> object defining how the request should be executed.</param>
        /// <returns>A <c>List(Of TwitterStatus)</c> containing the requested tweets.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Get Friends Timeline" lang="vbnet" title="Getting the friends timeline"></code>
        /// </remarks>
        public List<TwitterStatus> FriendsTimeline(TwitterParameters Parameters)
        {
            string Url = STATUSES_FRIENDS_TIMELINE;
            if (Parameters != null)
            {
                Url = Parameters.BuildUrl(Url);
            }
            return ParseStatuses(PerformWebRequest(Url, "GET"));
        }

        /// <summary>
        /// Returns a list of tweets from the authenticating user.
        /// </summary>
        /// <returns>A <c>List(Of TwitterStatus)</c> containing the requested tweets.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Get User Timeline" lang="vbnet" title="Getting the user timeline"></code>
        /// </remarks>
        public List<TwitterStatus> UserTimeline()
        {
            TwitterParameters Parameters = default;
            return UserTimeline(Parameters);
        }

        /// <summary>
        /// Returns a list of tweets from the specified user
        /// </summary>
        /// <param name="ScreenName">The screen name of the user whose tweets will be returned.</param>
        /// <returns></returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Get User Timeline" lang="vbnet" title="Getting the user timeline"></code>
        /// </remarks>
        public List<TwitterStatus> UserTimeline(string ScreenName)
        {
            var Parameters = new TwitterParameters();
            Parameters.Add(TwitterParameterNames.ScreenName, ScreenName);
            return UserTimeline(Parameters);
        }

        /// <summary>
        /// Returns a list of tweets from the specified user.
        /// </summary>
        /// <param name="Parameters">A <see>TwitterParameters</see> object defining how the request should be executed.</param>
        /// <returns>A <c>List(Of TwitterStatus)</c> containing the requested tweets.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Get User Timeline" lang="vbnet" title="Getting the user timeline"></code>
        /// </remarks>
        public List<TwitterStatus> UserTimeline(TwitterParameters Parameters)
        {
            string Url = STATUSES_USER_TIMELINE;
            if (Parameters != null)
            {
                Url = Parameters.BuildUrl(Url);
            }
            return ParseStatuses(PerformWebRequest(Url, "GET"));
        }

        /// <summary>
        /// Retrieves tweets that contain the screen name of the authenticating user.
        /// </summary>
        /// <returns>A <c>List(Of TwitterStatus)</c> containing the requested tweets.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Get Mentions" lang="vbnet" title="Getting mentions"></code>
        /// </remarks>
        public List<TwitterStatus> Mentions()
        {
            return Mentions(default);
        }

        /// <summary>
        /// Retrieves tweets that contain the screen name of the authenticating user.
        /// </summary>
        /// <param name="Parameters">A <see>TwitterParameters</see> object defining how the request should be executed.</param>
        /// <returns>A <c>List(Of TwitterStatus)</c> containing the requested tweets.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Get Mentions" lang="vbnet" title="Getting entions"></code>
        /// </remarks>
        public List<TwitterStatus> Mentions(TwitterParameters Parameters)
        {
            string Url = STATUSES_MENTIONS;
            if (Parameters != null)
            {
                Url = Parameters.BuildUrl(Url);
            }
            return ParseStatuses(PerformWebRequest(Url, "GET"));
        }

        /// <summary>
        /// Retrieves retweets posted by the authenticating user.
        /// </summary>
        /// <returns>A <c>List(Of TwitterStatus)</c> containing the requested retweets.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Retweeted By Me" lang="vbnet" title="Getting tweets you retweeted"></code>
        /// </remarks>
        public List<TwitterStatus> RetweetedByMe()
        {
            return RetweetedByMe(default);
        }

        /// <summary>
        /// Retrieves retweets posted by the authenticating user.
        /// </summary>
        /// <param name="Parameters">A <see>TwitterParameters</see> object defining how the request should be executed.</param>
        /// <returns>A <c>List(Of TwitterStatus)</c> containing the requested retweets.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Retweeted By Me" lang="vbnet" title="Getting tweets you retweeted"></code>
        /// </remarks>
        public List<TwitterStatus> RetweetedByMe(TwitterParameters Parameters)
        {
            string Url = STATUSES_RETWEETED_BY_ME;
            if (Parameters != null)
            {
                Url = Parameters.BuildUrl(Url);
            }

            return ParseStatuses(PerformWebRequest(Url, "GET"));
        }

        /// <summary>
        /// Retrieves retweets posted by friends of the authenticating user.
        /// </summary>
        /// <returns>A <c>List(Of TwitterStatus)</c> containing the requested retweets.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Retweeted To Me" lang="vbnet" title="Getting tweets you retweeted"></code>
        /// </remarks>
        public List<TwitterStatus> RetweetedToMe()
        {
            return RetweetedToMe(default);
        }

        /// <summary>
        /// Retrieves retweets posted by friends of the authenticating user.
        /// </summary>
        /// <param name="Parameters">A <see>TwitterParameters</see> object defining how the request should be executed.</param>
        /// <returns>A <c>List(Of TwitterStatus)</c> containing the requested retweets.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Retweeted To Me" lang="vbnet" title="Getting tweets you retweeted"></code>
        /// </remarks>
        public List<TwitterStatus> RetweetedToMe(TwitterParameters Parameters)
        {
            string Url = STATUSES_RETWEETED_TO_ME;
            if (Parameters != null)
            {
                Url = Parameters.BuildUrl(Url);
            }

            return ParseStatuses(PerformWebRequest(Url, "GET"));
        }

        /// <summary>
        /// Retrieves tweets from the authenticating user retweeted by others.
        /// </summary>
        /// <returns>A <c>List(Of TwitterStatus)</c> containing the requested retweets.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Retweets Of Me" lang="vbnet" title="Getting retweets of your tweets"></code>
        /// </remarks>
        public List<TwitterStatus> RetweetsOfMe()
        {
            return RetweetsOfMe(default);
        }

        /// <summary>
        /// Retrieves tweets from the authenticating user retweeted by others.
        /// </summary>
        /// <param name="Parameters">A <see>TwitterParameters</see> object defining how the request should be executed.</param>
        /// <returns>A <c>List(Of TwitterStatus)</c> containing the requested retweets.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Retweets Of Me" lang="vbnet" title="Getting retweets of your tweets"></code>
        /// </remarks>
        public List<TwitterStatus> RetweetsOfMe(TwitterParameters Parameters)
        {
            string Url = STATUSES_RETWEETS_OF_ME;
            if (Parameters != null)
            {
                Url = Parameters.BuildUrl(Url);
            }

            return ParseStatuses(PerformWebRequest(Url, "GET"));
        }
        #endregion

        #region Status Methods
        /// <summary>
        /// Retrieves a specific tweet.
        /// </summary>
        /// <param name="ID">The ID of the tweet to be retrieved.</param>
        /// <returns>A <c>TwitterStatus</c> object representing the requested tweet.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Show a Tweet" lang="vbnet" title="Retrieving a tweet"></code>
        /// </remarks>
        public TwitterStatus ShowUpdate(long ID)
        {
            return ParseStatuses(PerformWebRequest(string.Format(STATUSES_SHOW, ID), "GET"))[0];
        }

        /// <summary>
        /// Posts a tweet.
        /// </summary>
        /// <param name="Text">The text of the tweet to be posted.</param>
        /// <returns>A <c>TwitterStatus</c> object representing the posted tweet.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Post a Tweet" lang="vbnet" title="Posting a tweet"></code>
        /// </remarks>
        public TwitterStatus Update(string Text)
        {
            return ParseStatuses(PerformWebRequest(STATUSES_UPDATE + "?status=" + TwitterOAuth.OAuthUrlEncode(Text), "POST"))[0];
        }

        /// <summary>
        /// Posts a tweet as a reply to another tweet.
        /// </summary>
        /// <param name="Text">The text of the tweet to be posted.</param>
        /// <param name="ID">The ID of the tweet being replied to.</param>
        /// <returns>A <c>TwitterStatus</c> object representing the posted tweet.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Post a Reply" lang="vbnet" title="Posting a reply"></code>
        /// </remarks>
        public TwitterStatus ReplyToUpdate(string Text, long ID)
        {
            string Url = string.Format(STATUSES_UPDATE + "?status={0}&in_reply_to_status_id={1}", TwitterOAuth.OAuthUrlEncode(Text), ID.ToString());
            return ParseStatuses(PerformWebRequest(Url, "POST"))[0];
        }

        /// <summary>
        /// Deletes a tweet posted by the authenticating user.
        /// </summary>
        /// <param name="ID">The ID of the tweet to be deleted.</param>
        /// <returns>A <c>TwitterStatus</c> object representing the deleted tweet.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Delete a Tweet" lang="vbnet" title="Deleting a tweet"></code>
        /// </remarks>
        public TwitterStatus DeleteUpdate(long ID)
        {
            return ParseStatuses(PerformWebRequest(string.Format(STATUSES_DESTROY, ID), "POST"))[0];
        }

        /// <summary>
        /// Retweets a tweet.
        /// </summary>
        /// <param name="ID">Id ID of the tweet being retweeted.</param>
        /// <returns>A <c>TwitterStatus</c>representing the retweet.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Retweeting a Tweet" lang="vbnet" title="Retweeting a tweet"></code>
        /// </remarks>
        public TwitterStatus Retweet(long ID)
        {
            var tp = new TwitterParameters();
            tp.Add(TwitterParameterNames.ID, ID);
            string Url = tp.BuildUrl(string.Format(STATUSES_RETWEET, ID.ToString()));
            return ParseStatuses(PerformWebRequest(Url, "POST"))[0];
        }

        /// <summary>
        /// Retrieves up to the first 100 retweets of the specified tweet.
        /// </summary>
        /// <param name="ID">The tweet whose retweets will be retrieved.</param>
        /// <param name="Count">How many retweets to reteieve, up to 100.</param>
        /// <returns></returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Retrieving Retweets" lang="vbnet" title="Rerieveing retweets"></code>
        /// </remarks>
        public List<TwitterStatus> Retweets(long ID, long Count)
        {
            var tp = new TwitterParameters();
            tp.Add(TwitterParameterNames.ID, ID);
            tp.Add(TwitterParameterNames.Count, Count);
            string Url = tp.BuildUrl(STATUSES_RETWEETS);
            return ParseStatuses(PerformWebRequest(Url, "GET"));
        }

        /// <summary>
        /// Retireves of list of tweets that are replies to tweets posted by the selected user.
        /// </summary>
        /// <returns>A <c>List(Of TwitterStatus)</c> containing the requested tweets.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Get Replies" lang="vbnet" title="Getting replies"></code>
        /// </remarks>
        public List<TwitterStatus> Replies()
        {
            return Replies(default);
        }

        /// <summary>
        /// Retireves of list of tweets that are replies to tweets posted by the selected user.
        /// </summary>
        /// <param name="Parameters">A <see>TwitterParameters</see> object defining how the request should be executed.</param>
        /// <returns>A <c>List(Of TwitterStatus)</c> containing the requested tweets.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Get Replies" lang="vbnet" title="Getting replies"></code>
        /// </remarks>
        public List<TwitterStatus> Replies(TwitterParameters Parameters)
        {
            string Url = STATUSES_REPLIES;
            if (Parameters != null)
            {
                Url = Parameters.BuildUrl(Url);
            }
            return ParseStatuses(PerformWebRequest(Url, "GET"));
        }
        #endregion

        #region User Methods
        /// <summary>
        /// Retrieves data about a specific user.
        /// </summary>
        /// <param name="ID">The ID of the user whose information is being requested.</param>
        /// <returns>A <c>TwitterUser</c> object representing the requested user.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Get User Information" lang="vbnet" title="Retrieving user information"></code>       
        /// </remarks>
        public TwitterUser ShowUser(long ID)
        {
            var tp = new TwitterParameters();
            tp.Add(TwitterParameterNames.ID, ID);
            string Url = tp.BuildUrl(USERS_SHOW);
            return ParseUsers(PerformWebRequest(Url, "GET"))[0];
        }

        /// <summary>
        /// Retrieves data about a specific user.
        /// </summary>
        /// <param name="ScreenName">The screen name of the user whose information is being requested.</param>
        /// <returns>A <c>TwitterUser</c> object representing the requested user.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Get User Information" lang="vbnet" title="Retrieving user information"></code>       
        /// </remarks>
        public TwitterUser ShowUser(string ScreenName)
        {
            var tp = new TwitterParameters();
            tp.Add(TwitterParameterNames.ScreenName, ScreenName);
            string Url = tp.BuildUrl(USERS_SHOW);
            return ParseUsers(PerformWebRequest(Url, "GET"))[0];
        }

        /// <summary>
        /// Retrieves a list of all the users that are followed by the authenticated user.
        /// </summary>
        /// <returns>A <c>List(Of TwitterUser)</c> representing all the users that are followed by the authenticating user.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Get Friends" lang="vbnet" title="Retrieving friends"></code>
        /// Users who follow many users may experience long wait times because of the way Twitter handles large requests.
        /// When you call this method, you should let your users know that there may be a delay or use it in a separate thread.
        /// </remarks>
        public List<TwitterUser> Friends()
        {
            return Friends(default);
        }

        /// <summary>
        /// Retrieves a list of all the users that are followed by the specified user.
        /// </summary>
        /// <param name="Parameters">A <see>TwitterParameters</see> object defining how the request should be executed.</param>
        /// <returns>A <c>List(Of TwitterUser)</c> representing all the users that are followed by the specified user.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Get Friends" lang="vbnet" title="Retrieving friends"></code>
        /// Users who follow many users may experience long wait times because of the way Twitter handles large requests.
        /// When you call this method, you should let your users know that there may be a delay or use it in a separate thread.
        /// </remarks>
        public List<TwitterUser> Friends(TwitterParameters Parameters)
        {
            var ReturnValue = new List<TwitterUser>();
            long NextCursor = -1;

            do
            {
                string Url = STATUSES_FRIENDS;

                TwitterParameters LoopParameters;
                if (Parameters is null)
                {
                    LoopParameters = new TwitterParameters();
                }
                else
                {
                    LoopParameters = Parameters;
                }

                LoopParameters.Add(TwitterParameterNames.Cursor, NextCursor);
                Url = LoopParameters.BuildUrl(Url);

                var Batch = new List<TwitterUser>();
                Batch = ParseUsers(PerformWebRequest(Url, "GET"), ref NextCursor);
                ReturnValue.AddRange(Batch);
            }
            while (NextCursor != 0L);

            return ReturnValue;
        }

        /// <summary>
        /// Retrieves a list of all the users that follow the authenticated user.
        /// </summary>
        /// <returns>A <c>List(Of TwitterUser)</c> representing all the users that follow the authenticating user.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Get Followers" lang="vbnet" title="Retrieving followers"></code>
        /// Users who have many followers may experience long wait times because of the way Twitter handles large requests.
        /// When you call this method, you should let your users know that there may be a delay or use it in a separate thread.
        /// </remarks>
        //public List<TwitterUser> Followers()
        //{
        //    return Followers(default);
        //}

        /// <summary>
        /// Retrieves a list of all the users that follow the specified user.
        /// </summary>
        /// <param name="Parameters">A <see>TwitterParameters</see> object defining how the request should be executed.</param>
        /// <returns>A <c>List(Of TwitterUser)</c> representing all the users that follow the specified user.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Get Followers" lang="vbnet" title="Retrieving followers"></code>
        /// Users who have many followers may experience long wait times because of the way Twitter handles large requests.
        /// When you call this method, you should let your users know that there may be a delay or use it in a separate thread.
        /// </remarks>
        //public List<TwitterUser> Followers(TwitterParameters Parameters, PagedResults<TwitterUser> results)
        //{
        //    var Results = new PagedResults<TwitterUser>();

        //    while (Results.HasMore)
        //        Followers(Parameters, results);

        //    return Results;
        //}

        /// <summary>
        /// Retrieves a list of all the users that follow the specified user.
        /// </summary>
        /// <param name="Parameters">A <see>TwitterParameters</see> object defining how the request should be executed.</param>
        /// <param name="Results">The paged results to add the users to</param>
        /// <remarks>
        /// Users who have many followers may experience long wait times because of the way Twitter handles large requests.
        /// When you call this method, you should let your users know that there may be a delay or use it in a separate thread.
        /// </remarks>
        public void Followers(TwitterParameters Parameters, PagedResults<TwitterUser> Results)
        {
            string Url = STATUSES_FOLLOWERS;

            TwitterParameters LoopParameters;
            if (Parameters is null)
            {
                LoopParameters = new TwitterParameters();
            }
            else
            {
                LoopParameters = Parameters;
            }

            LoopParameters.Add(TwitterParameterNames.Cursor, Results.Cursor);
            Url = LoopParameters.BuildUrl(Url);
            long Cursor = Results.Cursor;
            Results.AddRange(ParseUsers(PerformWebRequest(Url, "GET"), ref Cursor));
        }

        /// <summary>
        /// Performs a people search and returns a list of matching users.
        /// </summary>
        /// <param name="SearchQuery">A <c>String</c> representing the search term or terms</param>
        /// <returns>A <c>List(Of TwitterUser) containing the search results.</c></returns>
        /// <remarks></remarks>
        public List<TwitterUser> UserSearch(string SearchQuery)
        {
            return UserSearch(SearchQuery, 1);
        }

        /// <summary>
        /// Performs a people search and returns a list of matching users.
        /// </summary>
        /// <param name="SearchQuery">A <c>String</c> representing the search term or terms</param>
        /// <param name="PageNumber">An <c>Integer</c> indicating which page of results should be returned.</param>
        /// <returns>A <c>List(Of TwitterUser) containing the search results.</c></returns>
        /// <remarks></remarks>
        public List<TwitterUser> UserSearch(string SearchQuery, int PageNumber)
        {
            var Parameters = new TwitterParameters();
            string Url;
            Parameters.Add(TwitterParameterNames.Page, PageNumber);
            Parameters.Add(TwitterParameterNames.SearchQuery, SearchQuery);
            Url = Parameters.BuildUrl(USERS_SEARCH);
            return ParseUsers(PerformWebRequest(Url, "GET"));
        }
        #endregion

        #region List Methods
        /// <summary>
        /// Creates a new list for the authenticated user.
        /// </summary>
        /// <param name="Username">The username of the authenticated user.</param>
        /// <param name="ListName">The name of the list.</param>
        /// <param name="Mode">Whether the list is public or private.</param>
        /// <returns>A <c>TwitterList</c> object representing the new list.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Create List" lang="vbnet" title="Creating a list"></code>
        /// </remarks>
        public TwitterList ListCreate(string Username, string ListName, Globals.ListMode Mode)
        {
            return ListCreate(Username, ListName, Mode, string.Empty);
        }

        /// <summary>
        /// Creates a new list for the authenticated user.
        /// </summary>
        /// <param name="Username">The username of the authenticated user.</param>
        /// <param name="ListName">The name of the list.</param>
        /// <param name="Mode">Whether the list is public or private.</param>
        /// <param name="Description">A description of the list.</param>
        /// <returns>A <c>TwitterList</c> object representing the new list.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Create List" lang="vbnet" title="Creating a list"></code>
        /// </remarks>
        public TwitterList ListCreate(string Username, string ListName, Globals.ListMode Mode, string Description)
        {
            string Url = string.Empty;
            if (string.IsNullOrEmpty(Description))
            {
                Url = string.Format(LISTS + "?name={1}&mode={2}", Username, ListName, Mode.ToString());
            }
            else
            {
                Url = string.Format(LISTS + "?name={1}&mode={2}&description={3}", Username, ListName, Mode.ToString(), Description);
            }

            return ParseLists(PerformWebRequest(Url, "POST"))[0];
        }

        /// <summary>
        /// Updates the specified list.
        /// </summary>
        /// <param name="Username">The username of the authenticated user.</param>
        /// <param name="ListID">The ID or slug of the list.</param>
        /// <param name="ListName">The name of the list.</param>
        /// <param name="Mode">Whether the list is public or private.</param>
        /// <param name="Description">A description of the list.</param>
        /// <returns>A <c>TwitterList</c> object representing the new list.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Update List" lang="vbnet" title="Updating a list"></code>
        /// </remarks>
        public TwitterList ListUpdate(string Username, string ListID, string ListName, Globals.ListMode Mode, string Description)
        {
            string Url = string.Format(LISTS_SPECIFY + "?name={2}&mode={3}&description={4}", Username, ListID, ListName, Mode.ToString(), Description);
            return ParseLists(PerformWebRequest(Url, "POST"))[0];
        }

        /// <summary>
        /// List the lists of the specified user. 
        /// </summary>
        /// <param name="Username">The username whose lists will be returned.</param>
        /// <returns>A <c>List(Of TwitterList)</c> representing the user's lists.  Private lists will be included if the authenticated users is the same as the user who'se lists are being returned.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Get Lists" lang="vbnet" title="Getting lists"></code>
        /// </remarks>
        public List<TwitterList> ListsGet(string Username)
        {
            var ReturnLists = new List<TwitterList>();
            long NextCursor = -1;

            do
            {
                string Url = string.Format(LISTS, Username);
                var LoopParameters = new TwitterParameters();
                LoopParameters.Add(TwitterParameterNames.Cursor, NextCursor);
                Url = LoopParameters.BuildUrl(Url);

                var Batch = new List<TwitterList>();
                Batch = ParseLists(PerformWebRequest(Url, "GET"), ref NextCursor);
                ReturnLists.AddRange(Batch);
            }
            while (NextCursor != 0L);

            return ReturnLists;
        }

        /// <summary>
        /// Show the specified list.
        /// </summary>
        /// <param name="Username">The user who owns the list.</param>
        /// <param name="ListID">The ID or slug of the list.</param>
        /// <returns>A <c>TwitterList</c> representing the list.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Get List" lang="vbnet" title="Getting a list"></code>
        /// </remarks>
        public TwitterList ListGet(string Username, string ListID)
        {
            string Url = string.Format(LISTS_SPECIFY, Username, ListID);
            return ParseLists(PerformWebRequest(Url, "GET"))[0];
        }

        /// <summary>
        /// Deletes the specified list. Must be owned by the authenticated user.
        /// </summary>
        /// <param name="Username">The username of the list owner.</param>
        /// <param name="ListID">The ID or slug of the list.</param>
        /// <returns>A <c>TwitterList</c> representing the list.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Delete List" lang="vbnet" title="Deleting a list"></code>
        /// </remarks>
        public TwitterList ListDelete(string Username, string ListID)
        {
            string Url = string.Format(LISTS_SPECIFY, Username, ListID);
            return ParseLists(PerformWebRequest(Url, "DELETE"))[0];
        }

        /// <summary>
        /// Show recent tweets from members of the specified list.
        /// </summary>
        /// <param name="Username">The username of the owner of list.</param>
        /// <param name="ListID">The ID or slug of the list.</param>
        /// <param name="Count">How many statuses to return.  Default is 20.  Maximum is 200.</param>
        /// <returns>A <c>List(Of TwitterStatus)</c> representing the requestsed tweets.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Get List Statuses" lang="vbnet" title="Getting list statuses"></code>
        /// </remarks>
        public List<TwitterStatus> ListStatuses(string Username, string ListID, long Count = 20L)
        {
            string Url = string.Format(LISTS_STATUSES + "?per_page={2}", Username, ListID, Count.ToString());
            return ParseStatuses(PerformWebRequest(Url, "GET"));
        }

        /// <summary>
        /// List the lists the specified user has been added to.
        /// </summary>
        /// <param name="Username">The user whose memberships will be returned.</param>
        /// <returns>A <c>List(Of TwitterList)</c> representing the requested lists.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Get List Memberships" lang="vbnet" title="Getting list memberships"></code>
        /// </remarks>
        //public List<TwitterList> ListMemberships(string Username, PagedResults<TwitterList> results)
        //{
        //    var Results = new PagedResults<TwitterList>();
        //    do
        //        ListMemberships(Username, Results);
        //    while (Results.HasMore != false);
        //    return Results;
        //}

        /// <summary>
        /// List the lists the specified user has been added to.
        /// </summary>
        /// <param name="Username">The user whose memberships will be returned.</param>
        /// <param name="Results">The paged results to add the lists to</param>
        public void ListMemberships(string Username, PagedResults<TwitterList> Results)
        {
            string Url = string.Format(LISTS_MEMBERSHIPS, Username);
            var Parameters = new TwitterParameters();
            Parameters.Add(TwitterParameterNames.Cursor, Results.Cursor);
            Url = Parameters.BuildUrl(Url);

            List<TwitterList> Lists;
            long Cursor = Results.Cursor;
            Lists = ParseLists(PerformWebRequest(Url, "GET"), ref Cursor);
            Results.AddRange(Lists);
        }

        /// <summary>
        /// List the lists the specified user follows.
        /// </summary>
        /// <param name="Username">The user whose followed lists will be returned.</param>
        /// <returns>A <c>List(Of TwitterList)</c> representing the requested lists.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Get List Subscriptions" lang="vbnet" title="Getting list subscriptions"></code>
        /// </remarks>
        public List<TwitterList> ListSubscriptions(string Username)
        {
            string Url = string.Format(LISTS_SUBSCRIPTIONS, Username);
            return ParseLists(PerformWebRequest(Url, "GET"));
        }
        #endregion

        #region List Members Methods
        /// <summary>
        /// Returns the members of the specified list.
        /// </summary>
        /// <param name="Username">The user who owns the list.</param>
        /// <param name="ListID">The ID or slug of the list.</param>
        /// <returns>A <c>List(Of TwitterUser)</c> representing the members of the list.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Get List Subscriptions" lang="vbnet" title="Getting list subscriptions"></code>
        /// </remarks>
        //public List<TwitterUser> ListMembers(string Username, string ListID)
        //{
        //    var Users = new PagedResults<TwitterUser>();

        //    do
        //        ListMembers(Username, ListID, Users);
        //    while (Users.HasMore != false);

        //    return Users;
        //}

        /// <summary>
        /// Returns the members of the specified list.
        /// </summary>
        /// <param name="Username">The user who owns the list.</param>
        /// <param name="ListID">The ID or slug of the list.</param>
        /// <param name="Results">The paged results to add the lists to</param>
        public void ListMembers(string Username, string ListID, PagedResults<TwitterUser> Results)
        {
            string Url = string.Format(LISTS_MEMBERS, Username, ListID);
            var LoopParameters = new TwitterParameters();
            LoopParameters.Add(TwitterParameterNames.Cursor, Results.Cursor);
            Url = LoopParameters.BuildUrl(Url);

            var Batch = new List<TwitterUser>();
            long Cursor = Results.Cursor;
            Batch = ParseUsers(PerformWebRequest(Url, "GET"), ref Cursor);
            Results.AddRange(Batch);
        }

        /// <summary>
        /// Add a member to a list.
        /// </summary>
        /// <param name="Username">The user who owns the list.</param>
        /// <param name="ListID">The ID or slug of the list.</param>
        /// <param name="UserID">The ID of the user to add to the list.</param>
        /// <returns>A <c>TwitterList</c> representing the list to which the user was added.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Add Member To List" lang="vbnet" title="Adding a member to a list"></code>
        /// </remarks>
        public TwitterList ListMembersAdd(string Username, string ListID, long UserID)
        {
            string Url = string.Format(LISTS_MEMBERS + "?id={2}", Username, ListID, UserID.ToString());
            return ParseLists(PerformWebRequest(Url, "POST"))[0];
        }

        /// <summary>
        /// Remove a member from a list.
        /// </summary>
        /// <param name="Username">The user who owns the list.</param>
        /// <param name="ListID">The ID or slug of the list.</param>
        /// <param name="UserID">The ID of the user to remove from the list.</param>
        /// <returns>A <c>TwitterList</c> representing the list from which the user was removed.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Remove Member From List" lang="vbnet" title="Removing a member from a list"></code>
        /// </remarks>
        public TwitterList ListMembersDelete(string Username, string ListID, long UserID)
        {
            string Url = string.Format(LISTS_MEMBERS + "?id={2}", Username, ListID, UserID.ToString());
            return ParseLists(PerformWebRequest(Url, "DELETE"))[0];
        }

        /// <summary>
        /// Check if a user is a member of the specified list.
        /// </summary>
        /// <param name="Username">The user who owns the list.</param>
        /// <param name="ListID">The ID or slug of the list.</param>
        /// <param name="UserID">The ID of the user to check.</param>
        /// <returns>A <c>Boolean</c> indicating whether or not the user is a member of the list.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Check User Membership" lang="vbnet" title="Checking a list for a particular user"></code>
        /// </remarks>
        public bool ListMembersCheck(string Username, string ListID, long UserID)
        {
            string Url = string.Format(LISTS_MEMBERS_ID, Username, ListID, UserID.ToString());
            return this.ParseUsers(PerformWebRequest(Url, "GET")).Count > 0;
        }
        #endregion

        #region List Subscribers Methods
        /// <summary>
        /// Returns the subscribers of the specified list.
        /// </summary>
        /// <param name="Username">The user who owns the list.</param>
        /// <param name="ListID">The ID or slug of the list.</param>
        /// <returns>A <c>List(Of TwitterUser)</c> representing the subscribers of the list.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Get List Subscribers" lang="vbnet" title="Getting list subscribers"></code>
        /// </remarks>
        public List<TwitterUser> ListSubscribers(string Username, string ListID)
        {
            var Users = new List<TwitterUser>();
            long NextCursor = -1;

            do
            {
                string Url = string.Format(LISTS_SUBSCRIBERS, Username, ListID);
                var LoopParameters = new TwitterParameters();
                LoopParameters.Add(TwitterParameterNames.Cursor, NextCursor);
                Url = LoopParameters.BuildUrl(Url);

                var Batch = new List<TwitterUser>();
                Batch = ParseUsers(PerformWebRequest(Url, "GET"), ref NextCursor);
                Users.AddRange(Batch);
            }
            while (NextCursor != 0L);

            return Users;
        }

        /// <summary>
        /// Subscribes the authenticated user to the specified list.
        /// </summary>
        /// <param name="Username">The user who owns the list.</param>
        /// <param name="ListID">The ID or slug of the list.</param>
        /// <returns>A <c>TwitterList</c> representing the list to which the user was added.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Subscribe To List" lang="vbnet" title="Subscribing to a list"></code>
        /// </remarks>
        public TwitterList ListSubscribe(string Username, string ListID)
        {
            string Url = string.Format(LISTS_SUBSCRIBERS, Username, ListID);
            return ParseLists(PerformWebRequest(Url, "POST"))[0];
        }

        /// <summary>
        /// Subscribes the authenticated user to the specified list.
        /// </summary>
        /// <param name="Username">The user who owns the list.</param>
        /// <param name="ListID">The ID or slug of the list.</param>
        /// <returns>A <c>TwitterList</c> representing the list to which the user was added.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Unsubscribe From List" lang="vbnet" title="Unsubscribing from a list"></code>
        /// </remarks>
        public TwitterList ListUnsubscribe(string Username, string ListID)
        {
            string Url = string.Format(LISTS_SUBSCRIBERS, Username, ListID);
            return ParseLists(PerformWebRequest(Url, "DELETE"))[0];
        }

        /// <summary>
        /// Check if a user is subscribed to the specified list.
        /// </summary>
        /// <param name="Username">The user who owns the list.</param>
        /// <param name="ListID">The ID or slug of the list.</param>
        /// <param name="UserID">The ID of the user to check.</param>
        /// <returns>A <c>Boolean</c> indicating whether or not the user is a subscriber of the list.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Check User Subscription" lang="vbnet" title="Checking a list for a particular subscriber"></code>
        /// </remarks>
        public bool ListSubscribersCheck(string Username, string ListID, long UserID)
        {
            string Url = string.Format(LISTS_SUBSCRIBERS_ID, Username, ListID, UserID.ToString());
            return this.ParseUsers(PerformWebRequest(Url, "GET")).Count > 0;
        }

        #endregion

        #region Direct Message Methods
        /// <summary>
        /// Retrieves the last 20 direct messages received by the authenticating user.
        /// </summary>
        /// <returns>A <c>List(Of <see>TwitterDirectMessage</see>)</c> representing the direct messages.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Get Direct Messages" lang="vbnet" title="Retrieving direct messages"></code>
        /// </remarks>
        public List<TwitterDirectMessage> DirectMessages()
        {
            return DirectMessages(default);
        }

        /// <summary>
        /// Retrieves the direct messages received by the authenticating user.
        /// </summary>
        /// <param name="Parameters">A <see>TwitterParameters</see> object detailing how the request will be executed.</param>
        /// <returns>A <c>List(Of <see>TwitterDirectMessage</see>)</c> representing the direct messages.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Get Direct Messages" lang="vbnet" title="Retrieving direct messages"></code>
        /// </remarks>
        public List<TwitterDirectMessage> DirectMessages(TwitterParameters Parameters)
        {
            string Url = DIRECT_MESSAGES;
            if (Parameters != null)
            {
                Url = Parameters.BuildUrl(Url);
            }
            return ParseDirectMessages(PerformWebRequest(Url, "GET"));
        }

        /// <summary>
        /// Retrieves the last 20 direct messages sent by the authenticating user.
        /// </summary>
        /// <returns>A <c>List(Of <see>TwitterDirectMessage</see>)</c> representing the direct messages.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Get Sent Direct Messages" lang="vbnet" title="Retrieving sent direct messages"></code>
        /// </remarks>
        public List<TwitterDirectMessage> DirectMessagesSent()
        {
            return DirectMessagesSent(default);
        }

        /// <summary>
        /// Retrieves the direct messages sent by the authenticating user.
        /// </summary>
        /// <param name="Parameters">A <see>TwitterParameters</see> object detailing how the request will be executed.</param>
        /// <returns>A <c>List(Of <see>TwitterDirectMessage</see>)</c> representing the direct messages.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Get Sent Direct Messages" lang="vbnet" title="Retrieving sent direct messages"></code>
        /// </remarks>
        public List<TwitterDirectMessage> DirectMessagesSent(TwitterParameters Parameters)
        {
            string Url = DIRECT_MESSAGES_SENT;
            if (Parameters != null)
            {
                Url = Parameters.BuildUrl(Url);
            }
            return ParseDirectMessages(PerformWebRequest(Url, "GET"));
        }

        /// <summary>
        /// Creates a new direct message
        /// </summary>
        /// <param name="User">The ID or screen name of the user who is the recipient.</param>
        /// <param name="Text">The text of the message.</param>
        /// <returns>A <c>TwitterDirectMessage</c> representing the sent message.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Send Direct Message" lang="vbnet" title="Sending a direct message"></code>
        /// </remarks>
        public TwitterDirectMessage SendDirectMessage(string User, string Text)
        {
            string Url = string.Format(DIRECT_MESSAGES_NEW + "?user={0}&text={1}", User, TwitterOAuth.OAuthUrlEncode(Text));
            var Message = ParseDirectMessages(PerformWebRequest(Url, "POST"))[0];
            return Message;
        }

        /// <summary>
        /// Deletes a direct message sent by the authenticating user.
        /// </summary>
        /// <param name="ID">The ID of the message to be deleted.</param>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Delete Direct Message" lang="vbnet" title="Deleting a direct message"></code>
        /// </remarks>
        public void DeleteDirectMessage(long ID)
        {
            PerformWebRequest(string.Format(DIRECT_MESSAGES_DESTROY, ID), "POST");
        }
        #endregion

        #region Friendship Methods
        /// <summary>
        /// Follows the specified user.
        /// </summary>
        /// <param name="ID">The ID of the user to follow</param>
        /// <returns>A <c>TwitterUser</c> object representing the followed user.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Follow User" lang="vbnet" title="Following a user"></code>
        /// </remarks>
        public TwitterUser Follow(long ID)
        {
            return ParseUsers(PerformWebRequest(string.Format(FRIENDSHIP_CREATE, ID), "POST"))[0];
        }

        /// <summary>
        /// Follows the specified user.
        /// </summary>
        /// <param name="ScreenName">The screen name of the user to follow</param>
        /// <returns></returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Follow User" lang="vbnet" title="Following a user"></code>
        /// </remarks>
        public TwitterUser Follow(string ScreenName)
        {
            return ParseUsers(PerformWebRequest(string.Format(FRIENDSHIP_CREATE, ScreenName), "POST"))[0];
        }

        /// <summary>
        /// Unfollows the specified user.
        /// </summary>
        /// <param name="ID">The ID of the user to stop following</param>
        /// <returns>A <c>TwitterUser</c> object representing the unfollowed user.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Unfollow User" lang="vbnet" title="Unfollowing a user"></code>
        /// </remarks>
        public TwitterUser UnFollow(long ID)
        {
            return ParseUsers(PerformWebRequest(string.Format(FRIENDSHIP_DESTROY, ID), "POST"))[0];
        }

        /// <summary>
        /// Unfollows the specified user.
        /// </summary>
        /// <param name="ScreenName">The screen name of the user to stop following</param>
        /// <returns>A <c>TwitterUser</c> object representing the unfollowed user.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Unfollow User" lang="vbnet" title="Unfollowing a user"></code>
        /// </remarks>
        public TwitterUser UnFollow(string ScreenName)
        {
            return ParseUsers(PerformWebRequest(string.Format(FRIENDSHIP_DESTROY, ScreenName), "POST"))[0];
        }

        /// <summary>
        /// Gets the relationship between two users.
        /// </summary>
        /// <param name="FollowerScreenName">The screen name of the user doing the following (the 'source' user)</param>
        /// <param name="FolloweeScreenName">The screen name of the user being followed (the 'target' user)</param>
        /// <returns>A <c>TwitterRelationship</c> object representing the friendship relationship between the two users</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Get Relationship" lang="vbnet" title="Determining the reltionship between two users"></code>
        /// </remarks>
        public TwitterRelationship Relationship(string FollowerScreenName, string FolloweeScreenName)
        {
            return ParseRelationships(PerformWebRequest(string.Format(FRIENDSHIP_SHOW, new string[] { "source_screen_name", FollowerScreenName, "target_screen_name", FolloweeScreenName }), "GET"))[0];
        }

        /// <summary>
        /// Gets the relationship between two users.
        /// </summary>
        /// <param name="FollowerID">The ID of the user doing the following (the 'source' user)</param>
        /// <param name="FolloweeID">The ID of the user being followed (the 'target' user)</param>
        /// <returns>A <c>TwitterRelationship</c> object representing the friendship relationship between the two users</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Get Relationship" lang="vbnet" title="Determining the reltionship between two users"></code>
        /// </remarks>
        public TwitterRelationship Relationship(long FollowerID, long FolloweeID)
        {
            return ParseRelationships(PerformWebRequest(string.Format(FRIENDSHIP_SHOW, new string[] { "source_id", FollowerID.ToString(), "target_id", FolloweeID.ToString() }), "GET"))[0];
        }
        #endregion

        #region Social Graph Methods
        /// <summary>
        /// Returns a list of numeric IDs of users the authenticated user is following.
        /// </summary>
        /// <returns>A <c>List(Of Int64)</c> containing the IDs of the requested users.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Social Graph Friends" lang="vbnet" title="Retrieving friends IDs"></code>
        /// Users who follow many users may experience long wait times because of the way Twitter handles large requests.
        /// When you call this method, you should let your users know that there may be a delay or use it in a separate thread.
        /// </remarks>
        public List<long> FriendsIDs()
        {
            var ReturnValue = new List<long>();
            long NextCursor = -1;

            do
            {
                string Url = FRIENDS_URL;
                var tp = new TwitterParameters();
                tp.Add(TwitterParameterNames.Cursor, NextCursor);
                Url = tp.BuildUrl(Url);

                var Batch = new List<long>();
                Batch = ParseSocialGraph(PerformWebRequest(Url, "GET"), ref NextCursor);
                ReturnValue.AddRange(Batch);
            }
            while (NextCursor != 0L);

            return ReturnValue;
        }

        /// <summary>
        /// Returns a list of numeric IDs of users the specified user is following.
        /// </summary>
        /// <param name="ID">The ID of the user whose friends are being requested.</param>
        /// <returns>A <c>List(Of Int64)</c> containing the IDs of the requested users.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Social Graph Friends" lang="vbnet" title="Retrieving friends IDs"></code>
        /// Users who follow many users may experience long wait times because of the way Twitter handles large requests.
        /// When you call this method, you should let your users know that there may be a delay or use it in a separate thread.
        /// </remarks>
        public List<long> FriendsIDs(long ID)
        {
            var ReturnValue = new List<long>();
            long NextCursor = -1;

            do
            {
                string Url = FRIENDS_URL;
                var tp = new TwitterParameters();
                tp.Add(TwitterParameterNames.Cursor, NextCursor);
                tp.Add(TwitterParameterNames.ID, ID);
                Url = tp.BuildUrl(Url);

                var Batch = new List<long>();
                Batch = ParseSocialGraph(PerformWebRequest(Url, "GET"), ref NextCursor);
                ReturnValue.AddRange(Batch);
            }
            while (NextCursor != 0L);

            return ReturnValue;
        }

        /// <summary>
        /// Returns a list of numeric IDs of users the specified user is following.
        /// </summary>
        /// <param name="ScreenName">The screen name of the user whose friends are being requested.</param>
        /// <returns>A <c>List(Of Int64)</c> containing the IDs of the requested users.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Social Graph Friends" lang="vbnet" title="Retrieving friends IDs"></code>
        /// Users who follow many users may experience long wait times because of the way Twitter handles large requests.
        /// When you call this method, you should let your users know that there may be a delay or use it in a separate thread.
        /// </remarks>
        public List<long> FriendsIDs(string ScreenName)
        {
            var ReturnValue = new List<long>();
            long NextCursor = -1;

            do
            {
                string Url = FRIENDS_URL;
                var tp = new TwitterParameters();
                tp.Add(TwitterParameterNames.Cursor, NextCursor);
                tp.Add(TwitterParameterNames.ScreenName, ScreenName);
                Url = tp.BuildUrl(Url);

                var Batch = new List<long>();
                Batch = ParseSocialGraph(PerformWebRequest(Url, "GET"), ref NextCursor);
                ReturnValue.AddRange(Batch);
            }
            while (NextCursor != 0L);

            return ReturnValue;
        }

        /// <summary>
        /// Returns a list of numeric IDs of users the authenticated user is followed by.
        /// </summary>
        /// <returns>A <c>List(Of Int64)</c> containing the IDs of the requested users.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Social Graph Followers" lang="vbnet" title="Retrieving follower IDs"></code>
        /// Users with many followers may experience long wait times because of the way Twitter handles large requests.
        /// When you call this method, you should let your users know that there may be a delay or use it in a separate thread.
        /// </remarks>
        public List<long> FollowersIDs()
        {
            var ReturnValue = new List<long>();
            long NextCursor = -1;

            do
            {
                string Url = FOLLOWERS_URL;
                var tp = new TwitterParameters();
                tp.Add(TwitterParameterNames.Cursor, NextCursor);
                Url = tp.BuildUrl(Url);

                var Batch = new List<long>();
                Batch = ParseSocialGraph(PerformWebRequest(Url, "GET"), ref NextCursor);
                ReturnValue.AddRange(Batch);
            }
            while (NextCursor != 0L);

            return ReturnValue;
        }

        /// <summary>
        /// Returns a list of numeric IDs of users the specified user is followed by.
        /// </summary>
        /// <param name="ID">The ID of the user whose followers are being requested.</param>
        /// <returns>A <c>List(Of Int64)</c> containing the IDs of the requested users.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Social Graph Followers" lang="vbnet" title="Retrieving followers IDs"></code>
        /// Users who have many followers may experience long wait times because of the way Twitter handles large requests.
        /// When you call this method, you should let your users know that there may be a delay or use it in a separate thread.
        /// </remarks>
        public List<long> FollowersIDs(long ID)
        {
            var ReturnValue = new List<long>();
            long NextCursor = -1;

            do
            {
                string Url = FOLLOWERS_URL;
                var tp = new TwitterParameters();
                tp.Add(TwitterParameterNames.Cursor, NextCursor);
                tp.Add(TwitterParameterNames.ID, ID);
                Url = tp.BuildUrl(Url);

                var Batch = new List<long>();
                Batch = ParseSocialGraph(PerformWebRequest(Url, "GET"), ref NextCursor);
                ReturnValue.AddRange(Batch);
            }
            while (NextCursor != 0L);

            return ReturnValue;
        }

        /// <summary>
        /// Returns a list of numeric IDs of users the specified user is followed by.
        /// </summary>
        /// <param name="ScreenName">The screen name of the user whose followers are being requested.</param>
        /// <returns>A <c>List(Of Int64)</c> containing the IDs of the requested users.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Social Graph Followers" lang="vbnet" title="Retrieving followers IDs"></code>
        /// Users who have many followers may experience long wait times because of the way Twitter handles large requests.
        /// When you call this method, you should let your users know that there may be a delay or use it in a separate thread.
        /// </remarks>
        public List<long> FollowersIDs(string ScreenName)
        {
            var ReturnValue = new List<long>();
            long NextCursor = -1;

            do
            {
                string Url = FOLLOWERS_URL;
                var tp = new TwitterParameters();
                tp.Add(TwitterParameterNames.Cursor, NextCursor);
                tp.Add(TwitterParameterNames.ScreenName, ScreenName);
                Url = tp.BuildUrl(Url);

                var Batch = new List<long>();
                Batch = ParseSocialGraph(PerformWebRequest(Url, "GET"), ref NextCursor);
                ReturnValue.AddRange(Batch);
            }
            while (NextCursor != 0L);

            return ReturnValue;
        }
        #endregion

        #region Account Methods
        /// <summary>
        /// Returns account information for the authenticated user.
        /// </summary>
        /// <returns>A <see>TwitterUser</see> representing the authenticated user.</returns>
        /// <remarks>
        /// This method works by calling Twitter's <a href="http://apiwiki.twitter.com/Twitter-REST-API-Method:-account%C2%A0verify_credentials">account/verify_credentials</a> method.
        /// The rate limits on this method are stricter than for other parts of the Twitter API.  Only call this when necessary and cache the results when you do.
        /// <code source="TwitterVB2\examples.vb" region="Get Account Information" lang="vbnet" title="Retrieving account information"></code>
        /// </remarks>
        public TwitterUser AccountInformation()
        {
            return ParseUsers(PerformWebRequest(ACCOUNT_VERIFY_CREDENTIALS, "GET"))[0];
        }
        #endregion

        #region Favorite Methods
        /// <summary>
        /// Retrieves the last 20 favorites marked by the authenticating user.
        /// </summary>
        /// <returns>A <c>List(Of <see>TwitterStatus</see>)</c> representing the favorites.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Get Favorites" lang="vbnet" title="Retrieving favorites"></code>
        /// </remarks>
        public List<TwitterStatus> Favorites()
        {
            return Favorites(default);
        }

        /// <summary>
        /// Retrieves the favorites marked by the authenticating user.
        /// </summary>
        /// <param name="Parameters">A <see>TwitterParameters</see> object detailing how the request will be executed.</param>
        /// <returns>A <c>List(Of <see>TwitterStatus</see>)</c> representing the favorites.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Get Favorites" lang="vbnet" title="Retrieving favorites"></code>
        /// </remarks>
        public List<TwitterStatus> Favorites(TwitterParameters Parameters)
        {
            string Url = FAVORITES_LIST;
            if (Parameters != null)
            {
                Url = Parameters.BuildUrl(Url);
            }
            return ParseStatuses(PerformWebRequest(Url, "GET"));
        }

        /// <summary>
        /// Creates a new favorite.
        /// </summary>
        /// <param name="ID">The ID of the message to be marked as a favorite.</param>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Create Favorite" lang="vbnet" title="Creating a favorite"></code>
        /// </remarks>
        public void AddFavorite(long ID)
        {
            PerformWebRequest(string.Format(FAVORITES_CREATE, ID), "POST");
        }

        /// <summary>
        /// Destroys a favorite created by the authenticating user.
        /// </summary>
        /// <param name="ID">The ID of the favorite to be deleted.</param>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Delete Favorite" lang="vbnet" title="Deleting a favorite"></code>
        /// </remarks>
        public void DeleteFavorite(long ID)
        {
            PerformWebRequest(string.Format(FAVORITES_DESTROY, ID), "POST");
        }
        #endregion

        #region Notification Methods
        // Nothing here yet
        #endregion

        #region Block Methods
        /// <summary>
        /// Blocks the specified user. Destroys a friendship to the blocked user if it exists.
        /// </summary>
        /// <param name="ID">The ID of the user to be blocked.</param>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Block User" lang="vbnet" title="Blocking a user"></code>
        /// You can read more about blocking at <a href="http://help.twitter.com/forums/10711/entries/15355">http://help.twitter.com/forums/10711/entries/15355</a>.
        /// </remarks>
        public void BlockUser(long ID)
        {
            PerformWebRequest(string.Format(BLOCKS_CREATE, ID), "POST");
        }

        /// <summary>
        /// Blocks the specified user. Destroys a friendship to the blocked user if it exists.
        /// </summary>
        /// <param name="ScreenName">The screen name of the user to be blocked.</param>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Block User" lang="vbnet" title="Blocking a user"></code>
        /// You can read more about blocking at <a href="http://help.twitter.com/forums/10711/entries/15355">http://help.twitter.com/forums/10711/entries/15355</a>.
        /// </remarks>
        public void BlockUser(string ScreenName)
        {
            PerformWebRequest(string.Format(BLOCKS_CREATE, ScreenName), "POST");
        }

        /// <summary>
        /// Un-blocks the specified user. 
        /// </summary>
        /// <param name="ID">The ID of the user to be un-blocked.</param>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Unblock User" lang="vbnet" title="Unblocking a user"></code>
        /// You can read more about blocking at <a href="http://help.twitter.com/forums/10711/entries/15355">http://help.twitter.com/forums/10711/entries/15355</a>.
        /// </remarks>
        public void UnblockUser(long ID)
        {
            PerformWebRequest(string.Format(BLOCKS_DESTROY, ID), "POST");
        }

        /// <summary>
        /// Un-blocks the specified user. 
        /// </summary>
        /// <param name="ScreenName">The screen name of the user to be un-blocked.</param>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Unblock User" lang="vbnet" title="Unblocking a user"></code>
        /// You can read more about blocking at <a href="http://help.twitter.com/forums/10711/entries/15355">http://help.twitter.com/forums/10711/entries/15355</a>.
        /// </remarks>
        public void UnblockUser(string ScreenName)
        {
            PerformWebRequest(string.Format(BLOCKS_DESTROY, ScreenName), "POST");
        }

        /// <summary>
        /// Returns a List of IDs blocked by the authenticated user.
        /// </summary>
        /// <returns>A <c>List(Of Int64)</c>representing the blocked IDs.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Get Blocked IDs" lang="vbnet" title="Getting blocked IDs"></code>
        /// You can read more about blocking at <a href="http://help.twitter.com/forums/10711/entries/15355">http://help.twitter.com/forums/10711/entries/15355</a>.
        /// </remarks>
        public List<long> BlockedIDs()
        {
            return ParseBlockedIDs(PerformWebRequest(BLOCKS_BLOCKING_IDS, "GET"));
        }

        /// <summary>
        /// Returns a list of users blocked by the authenticating user.
        /// </summary>
        /// <returns>A <c>List(Of TwitterUser)</c> representing the blocked users.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Get Blocked Users" lang="vbnet" title="Getting blocked users"></code>
        /// You can read more about blocking at <a href="http://help.twitter.com/forums/10711/entries/15355">http://help.twitter.com/forums/10711/entries/15355</a>.
        /// </remarks>
        public List<TwitterUser> BlockedUsers()
        {
            return BlockedUsers(1);
        }

        /// <summary>
        /// Returns a list of users blocked by the authenticating user.
        /// </summary>
        /// <param name="PageNumber">The page number</param>
        /// <returns>A <c>List(Of TwitterUser)</c> representing the blocked users.</returns>
        public List<TwitterUser> BlockedUsers(int PageNumber)
        {
            var Parameters = new TwitterParameters();
            string Url;
            Parameters.Add(TwitterParameterNames.Page, PageNumber);
            Url = Parameters.BuildUrl(BLOCKS_BLOCKING);
            return ParseUsers(PerformWebRequest(Url, "GET"));
        }
        #endregion

        #region Spam Reporting Methods
        /// <summary>
        /// Reports a user to Twitter as a spammer.
        /// </summary>
        /// <param name="ID">The ID of the user who is posting spam.</param>
        /// <returns>A <c>TwitterStatus</c> object representing the user being reported.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Reporting a Spammer" lang="vbnet" title="Reporting a Spammer"></code>
        /// </remarks>
        public TwitterUser ReportSpam(long ID)
        {
            var tp = new TwitterParameters();
            tp.Add(TwitterParameterNames.ID, ID);
            string Url = tp.BuildUrl(REPORT_SPAM);
            return ParseUsers(PerformWebRequest(Url, "POST"))[0];
        }

        /// <summary>
        /// Reports a user to Twitter as a spammer.
        /// </summary>
        /// <param name="ScreenName">The ID of the user who is posting spam.</param>
        /// <returns>A <c>TwitterStatus</c> object representing the user being reported.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="Reporting a Spammer" lang="vbnet" title="Reporting a Spammer"></code>
        /// </remarks>
        public TwitterUser ReportSpam(string ScreenName)
        {
            var tp = new TwitterParameters();
            tp.Add(TwitterParameterNames.ScreenName, ScreenName);
            string Url = tp.BuildUrl(REPORT_SPAM);
            return ParseUsers(PerformWebRequest(Url, "POST"))[0];
        }
        #endregion

        #region Saved Searches Methods
        // nothing here yet
        #endregion

        #region OAuth Methods
        /// <summary>
        /// Returns a URL that will allow users to authenticate your application with Twitter.
        /// </summary>
        /// <param name="ConsumerKey">The Consumer Key assigned to your application by Twitter.</param>
        /// <param name="ConsumerKeySecret">The Consumer Key Secret assigned to your application by Twitter.</param>
        /// <returns>A <c>String</c> containing the URL where the user can authenticate your application.</returns>
        public string GetAuthenticationLink(string ConsumerKey, string ConsumerKeySecret)
        {
            Twitter_OAuth = new TwitterOAuth(ConsumerKey, ConsumerKeySecret);
            return Twitter_OAuth.GetAuthenticationLink();
        }

        /// <summary>
        /// Returns a URL that will allow users to authenticate your application with Twitter.
        /// </summary>
        /// <param name="ConsumerKey">The Consumer Key assigned to your application by Twitter.</param>
        /// <param name="ConsumerKeySecret">The Consumer Key Secret assigned to your application by Twitter.</param>
        /// <param name="CallbackUrl">The Url where users are taken after successful authentication.</param>
        /// <returns>A <c>String</c> containing the URL where the user can authenticate your application.</returns>
        /// <remarks>
        /// This method should only be used when you need to specify a callback url that is different from the one in your Twitter application
        /// registration.  For example, you would probably pass a CallbackUrl of "http://localhost" to do local testing.
        /// </remarks>
        public string GetAuthenticationLink(string ConsumerKey, string ConsumerKeySecret, string CallbackUrl)
        {
            Twitter_OAuth = new TwitterOAuth(ConsumerKey, ConsumerKeySecret, CallbackUrl);
            return Twitter_OAuth.GetAuthenticationLink();
        }

        /// <summary>
        /// Returns a URL that will allow users to authorize your application with Twitter.
        /// </summary>
        /// <param name="ConsumerKey">The Consumer Key assigned to your application by Twitter.</param>
        /// <param name="ConsumerKeySecret">The Consumer Key Secret assigned to your application by Twitter.</param>
        /// <returns>A <c>String</c> containing the URL where the user can authorize your application.</returns>
        public string GetAuthorizationLink(string ConsumerKey, string ConsumerKeySecret)
        {
            Twitter_OAuth = new TwitterOAuth(ConsumerKey, ConsumerKeySecret);
            return Twitter_OAuth.GetAuthorizationLink();
        }

        /// <summary>
        /// Returns a URL that will allow users to authorize your application with Twitter.
        /// </summary>
        /// <param name="ConsumerKey">The Consumer Key assigned to your application by Twitter.</param>
        /// <param name="ConsumerKeySecret">The Consumer Key Secret assigned to your application by Twitter.</param>
        /// <param name="CallbackUrl">The Url where users are taken after successful authorization.</param>
        /// <returns>A <c>String</c> containing the URL where the user can authorize your application.</returns>
        /// <remarks>
        /// This method should only be used when you need to specify a callback url that is different from the one in your Twitter application
        /// registration.  For example, you would probably pass a CallbackUrl of "http://localhost" to do local testing.
        /// </remarks>
        public string GetAuthorizationLink(string ConsumerKey, string ConsumerKeySecret, string CallbackUrl)
        {
            Twitter_OAuth = new TwitterOAuth(ConsumerKey, ConsumerKeySecret, CallbackUrl);
            return Twitter_OAuth.GetAuthorizationLink();
        }

        /// <summary>
        /// Asks Twitter to confirm that the user has authorized your application.
        /// </summary>
        /// <param name="PIN">The PIN number given to your user when they authorized your application.</param>
        /// <returns>If the PIN is correct, <c>True</c>, otherwise, <c>False</c>.</returns>
        /// <remarks>
        /// <c>GetAuthorizationLink()</c> must be called before this method, and both methods must be executed against the same instance 
        /// of the <c>TwitterOAuth</c> object.  See the Twitter OAuth Tutorial at the TwitterVB web site for details on the OAuth process.
        /// </remarks>
        public bool ValidatePIN(string PIN)
        {
            return Twitter_OAuth.ValidatePIN(PIN);
        }

        /// <summary>
        /// Exchanges a request token for an access token from Twitter.
        /// </summary>
        /// <param name="ConsumerKey"></param>
        /// <param name="ConsumerKeySecret"></param>
        /// <param name="OAuthToken"></param>
        /// <param name="OAuthVerifier"></param>
        /// <remarks>
        /// <c>GetAuthorizationLink()</c> must be called before this method.  See the Twitter OAuth Tutorial at the TwitterVB web site for details on the OAuth process.
        /// </remarks>
        public void GetAccessTokens(string ConsumerKey, string ConsumerKeySecret, string OAuthToken, string OAuthVerifier)
        {
            {
                ref var withBlock = ref Twitter_OAuth;
                withBlock.ConsumerKey = ConsumerKey;
                withBlock.ConsumerSecret = ConsumerKeySecret;
                withBlock.GetAccessToken(OAuthToken, OAuthVerifier);
            }
        }

        /// <summary>
        /// Configures the TwitterAPI object to use OAuth authentication
        /// </summary>
        /// <param name="ConsumerKey">The Consumer Key assigned to your application by Twitter.</param>
        /// <param name="ConsumerKeySecret">The Consumer Key Secret assigned to your application by Twitter.</param>
        /// <param name="Token">The OAuth Token given to you by Twitter when the user authorized your application.</param>
        /// <param name="TokenSecret">The OAuth Token Secret given to you by Twitter when the user authorized your application.</param>
        /// <remarks>See the Twitter OAuth Tutorial at the Twitter web site for details on the OAuth process.</remarks>
        public void AuthenticateWith(string ConsumerKey, string ConsumerKeySecret, string Token, string TokenSecret)
        {
            Twitter_OAuth = new TwitterOAuth(ConsumerKey, ConsumerKeySecret, Token, TokenSecret);
        }
        #endregion

        #region Local Trends Methods

        public List<TwitterTrendLocation> TrendLocations()
        {
            return ParseTrendLocations(PerformWebRequest(TREND_LOCATIONS, "GET"));
        }


        public List<TwitterTrend> TrendsByLocation(string LocationID)
        {
            return ParseTrends(PerformWebRequest(string.Format(TREND_LOCATION_TRENDS, LocationID), "GET"), TrendFormat.ByLocation);
        }

        #endregion

        #endregion

        #region File Upload Methods

        // 
        // The methods in this region provide access to non-Twitter APIs for uploading and sharing files
        // 

        /// <summary>
        /// Uploads a photo from the user's hard drive to the TweetPhoto service
        /// </summary>
        /// <param name="Filename">The full path and filename of the image to be uploaded to Twitpic.</param>
        /// <param name="Message">The message, if any, to include with the picture.</param>
        /// <param name="Username">The user's Twitter username.</param>
        /// <param name="Password">The user's Twitter password.</param>
        /// <param name="APIKey">Your application's TweetPhoto API key..</param>
        /// <returns>A <c>String</c> containing the TweetPhoto Url of the picture.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="TweetPhoto" lang="vbnet" title="Posting an image with TweetPhoto"></code>
        /// TwitPic does not implement OAuth, so it is up to developers to prompt the users for their Twitter login credentials.
        /// </remarks>
        public string TweetPhotoUpload(string Filename, string Message, string Username, string Password, string APIKey)
        {

            System.Net.WebRequest request = HttpWebRequest.Create("http://tweetphotoapi.com/api/tpapi.svc/upload2");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Headers.Add("TPAPIKEY: " + APIKey);
            request.Headers.Add("TPAPI: " + Username + "," + Password);

            string FileContentType = string.Empty;
            if (Filename.ToUpper().EndsWith(".GIF"))
            {
                FileContentType = "image/gif";
            }
            else if (Filename.ToUpper().EndsWith(".JPG"))
            {
                FileContentType = "image/jpeg";
            }
            else if (Filename.ToUpper().EndsWith(".JPEG"))
            {
                FileContentType = "image/jpeg";
            }
            else if (Filename.ToUpper().EndsWith(".PNG"))
            {
                FileContentType = "image/png";
            }
            else
            {
                return string.Empty;
            }

            request.Headers.Add("TPMIMETYPE: " + FileContentType);
            request.Headers.Add("TPPOST:True");
            request.Headers.Add("TPMSG: " + Message);
            // request.Headers.Add("TPTAGS: comma,seperated,tags,here")

            // Read image file into a byte array
            byte[] content;

            try
            {
                content = File.ReadAllBytes(Filename);
                request.ContentLength = content.Length;

                using (Stream str = request.GetRequestStream())
                {
                    str.Write(content, 0, content.Length);
                }

                WebResponse response = request.GetResponse();
                StreamReader reader;
                reader = new StreamReader(response.GetResponseStream());

                return reader.ReadToEnd();
            }

            catch (Exception)
            {
                return string.Empty;

            }
        }

        /// <summary>
        /// Uploads a binary file to the site FileSocial.com and publishes the link to Twitter
        /// </summary>
        /// <param name="UserName">Twitter UserName</param>
        /// <param name="Password">Twitter Password</param>
        /// <param name="FileName">FileName that you want to upload (note 50M limit)</param>
        /// <param name="Message">Message with the file link</param>
        /// <returns>The StatusID that is created as a result of the update</returns>
        /// <remarks>In order to use this function you will need to register your account <a href="http://filesocial.com">with filesocial.com</a></remarks>
        public string FileSocialUpload(string UserName, string Password, string FileName, string Message)
        {
            string ReturnValue = string.Empty;
            try
            {
                byte[] BinaryData = File.ReadAllBytes(FileName);
                string boundary = Guid.NewGuid().ToString();
                string Header = string.Format("--{0}", boundary);
                string Footer = string.Format("--{0}--", boundary);
                HttpWebRequest Request = (HttpWebRequest)System.Net.WebRequest.Create("http://filesocial.com/api/uploadAndPost");
                Request.PreAuthenticate = true;
                Request.AllowWriteStreamBuffering = true;
                Request.ContentType = string.Format("multipart/form-data; boundary={0}", boundary);
                Request.Method = "POST";
                string FileContentType = "application/octet-stream";
                string FileHeader = string.Format("Content-Disposition: file; name=\"{0}\"; filename=\"{1}\"", "file", FileName);
                string FileData = Encoding.GetEncoding("iso-8859-1").GetString(BinaryData);

                var Contents = new StringBuilder();
                Contents.AppendLine(Header);

                Contents.AppendLine(FileHeader);
                Contents.AppendLine(string.Format("Content-Type: {0}", FileContentType));
                Contents.AppendLine();
                Contents.AppendLine(FileData);

                Contents.AppendLine(Header);
                Contents.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "username"));
                Contents.AppendLine();
                Contents.AppendLine(UserName);

                Contents.AppendLine(Header);
                Contents.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "password"));
                Contents.AppendLine();
                Contents.AppendLine(Password);


                if (!string.IsNullOrEmpty(Message))
                {
                    Contents.AppendLine(Header);
                    Contents.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "message"));
                    Contents.AppendLine();
                    Contents.AppendLine(Message);
                }

                Contents.AppendLine(Footer);

                byte[] Bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(Contents.ToString());
                Request.ContentLength = Bytes.Length;

                using (Stream requestStream = Request.GetRequestStream())
                {
                    requestStream.Write(Bytes, 0, Bytes.Length);

                    using (HttpWebResponse Response = (HttpWebResponse)Request.GetResponse())
                    {
                        using (var Reader = new StreamReader(Response.GetResponseStream()))
                        {
                            string Result = Reader.ReadToEnd();

                            var ResponseDoc = new XmlDocument();

                            // My.Computer.Clipboard.SetText(Result)
                            ResponseDoc.LoadXml(Result);
                            var rsp = ResponseDoc.SelectSingleNode("//rsp");

                            if (rsp.Attributes["status"].Value == "ok")
                            {


                                ReturnValue = rsp.SelectSingleNode("//mediaurl").InnerText;
                                // ReturnValue = Result

                            }
                        }
                    }
                }
            }

            catch (Exception)
            {

            }
            return ReturnValue;

        }

        /// <summary>
        /// Uploads a photo from the user's hard drive to the TwitPic service
        /// </summary>
        /// <param name="Filename">The full path and filename of the image to be uploaded to Twitpic.</param>
        /// <param name="Message">The message, if any, to include with the picture.</param>
        /// <param name="Username">The user's Twitter username.</param>
        /// <param name="Password">The user's Twitter password.</param>
        /// <param name="Source">The source of the post.  This will only work if your application has been registered with Twitpic.com.</param>
        /// <returns>A <c>String</c> containing the TwitPic Url of the picture.</returns>
        /// <remarks>
        /// <code source="TwitterVB2\examples.vb" region="TwitPic" lang="vbnet" title="Posting an image with TwitPic"></code>
        /// TwitPic does not implement OAuth, so it is up to developers to prompt the users for their Twitter login credentials.
        /// </remarks>
        public string TwitPicUpload(string Filename, string Message, string Username, string Password, string Source = "")
        {

            string ReturnValue = string.Empty;

            try
            {
                // Get the file into an array of bytes
                byte[] BinaryData = File.ReadAllBytes(Filename);

                string boundary = Guid.NewGuid().ToString();
                string Header = string.Format("--{0}", boundary);
                string Footer = string.Format("--{0}--", boundary);

                // Build the request
                HttpWebRequest Request = (HttpWebRequest)System.Net.WebRequest.Create("http://twitpic.com/api/uploadAndPost");

                Request.PreAuthenticate = true;
                Request.AllowWriteStreamBuffering = true;
                Request.ContentType = string.Format("multipart/form-data; boundary={0}", boundary);
                Request.Method = "POST";

                string FileContentType = string.Empty;
                if (Filename.ToUpper().EndsWith(".GIF"))
                {
                    FileContentType = "image/gif";
                }
                else if (Filename.ToUpper().EndsWith(".JPG"))
                {
                    FileContentType = "image/jpeg";
                }
                else if (Filename.ToUpper().EndsWith(".JPEG"))
                {
                    FileContentType = "image/jpeg";
                }
                else if (Filename.ToUpper().EndsWith(".PNG"))
                {
                    FileContentType = "image/png";
                }
                else
                {
                    return string.Empty;
                }

                string FileHeader = string.Format("Content-Disposition: file; name=\"{0}\"; filename=\"{1}\"", "media", Filename);
                string FileData = Encoding.GetEncoding("iso-8859-1").GetString(BinaryData);

                var Contents = new StringBuilder();
                Contents.AppendLine(Header);

                Contents.AppendLine(FileHeader);
                Contents.AppendLine(string.Format("Content-Type: {0}", FileContentType));
                Contents.AppendLine();
                Contents.AppendLine(FileData);

                Contents.AppendLine(Header);
                Contents.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "username"));
                Contents.AppendLine();
                Contents.AppendLine(Username);

                Contents.AppendLine(Header);
                Contents.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "password"));
                Contents.AppendLine();
                Contents.AppendLine(Password);

                if (!string.IsNullOrEmpty(Source))
                {
                    Contents.AppendLine(Header);
                    Contents.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "source"));
                    Contents.AppendLine();
                    Contents.AppendLine(Source);
                }

                if (!string.IsNullOrEmpty(Message))
                {
                    Contents.AppendLine(Header);
                    Contents.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "message"));
                    Contents.AppendLine();
                    Contents.AppendLine(Message);
                }

                Contents.AppendLine(Footer);

                byte[] Bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(Contents.ToString());
                Request.ContentLength = Bytes.Length;

                using (Stream requestStream = Request.GetRequestStream())
                {
                    requestStream.Write(Bytes, 0, Bytes.Length);

                    using (HttpWebResponse Response = (HttpWebResponse)Request.GetResponse())
                    {
                        using (var Reader = new StreamReader(Response.GetResponseStream()))
                        {
                            string Result = Reader.ReadToEnd();

                            var ResponseDoc = new XmlDocument();
                            ResponseDoc.LoadXml(Result);
                            var rsp = ResponseDoc.SelectSingleNode("//rsp");
                            if (rsp.Attributes["status"].Value == "ok")
                            {
                                ReturnValue = ResponseDoc.SelectSingleNode("//mediaurl").Value;
                            }
                        }
                    }
                }
            }

            catch (Exception)
            {
                // Deliberately doing nothing here.
                // We just want to swallow the exception so that the application doesn't see an exception
            }

            return ReturnValue;
        }
        #endregion

        #region Url Shortening Methods

        // 
        // The methods in this region support URL shortening
        // 

        /// <summary>
        /// Submits a Url to the selected Url shortening service.
        /// </summary>
        /// <param name="UrlToShorten">The Url that will be shortened.</param>
        /// <param name="Shortener">A <c>UrlShortener</c> indicating which shortener service will be used.</param>
        /// <param name="p_strBRST">Optional: if using the Br.ST service you must include this key. otherwise an exception will be raised</param>
        /// <param name="p_strbitlyKey">Optional: if using the bit.ly service, you must provide this key.</param>
        /// <param name="p_strbitlyUser">Optional: bit.ly user</param>
        /// <returns>The shortened Url.</returns>
        /// <remarks></remarks>

        public string ShortenUrl(string UrlToShorten, Globals.UrlShortener Shortener, string p_strBRST = "<noapi>", string p_strbitlyKey = "", string p_strbitlyUser = "")
        {
            string ReturnValue = string.Empty;

            switch (Shortener)
            {
                case var @case when @case == UrlShortener.IsGd:
                    {
                        ReturnValue = PerformWebRequest(string.Format("http://is.gd/api.php?longurl={0}", HttpUtility.UrlEncode(UrlToShorten)));
                        break;
                    }

                case var case1 when case1 == UrlShortener.TinyUrl:
                    {
                        ReturnValue = PerformWebRequest(string.Format("http://tinyurl.com/api-create.php?url={0}", HttpUtility.UrlEncode(UrlToShorten)));
                        break;
                    }


                // Case UrlShortener.BrSt
                // Dim oBRST As New st.br.Service
                // Dim oResult As st.br.GenerationResult = Nothing
                // If p_strBRST <> "<noapi>" Then
                // oResult = oBRST.URLGenerateAlias(UrlToShorten, Nothing, p_strBRST)
                // If oResult.wasSuccessful Then
                // Return oResult.aliasURL
                // Else
                // Return String.Empty
                // End If
                // Else
                // Return String.Empty
                // End If

                case var case2 when case2 == UrlShortener.BitLy:
                    {
                        ReturnValue = PerformWebRequest(string.Format("http://api.bit.ly/v3/shorten?login={0}&apiKey={1}&longUrl={2}&format=txt", p_strbitlyUser, p_strbitlyKey, UrlToShorten));
                        break;
                    }

            }

            return ReturnValue;
        }
        #endregion



    }
}
