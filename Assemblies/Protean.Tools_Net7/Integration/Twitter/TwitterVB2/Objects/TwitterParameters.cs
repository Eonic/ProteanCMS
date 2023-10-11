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
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.VisualBasic.CompilerServices; // Install-Package Microsoft.VisualBasic


namespace Protean.Tools.Integration.Twitter.TwitterVB2.Objects
{
    /// <summary>
    /// The parameters that can be used to define a Twitter request.
    /// </summary>
    /// <remarks>
    /// Not every TwitterVB method accepts every parameter.  If you are unsure, consult the <a href="http://apiwiki.twitter.com/Twitter-API-Documentation">Twitter API documentation.</a>
    /// </remarks>
    public enum TwitterParameterNames
    {

        /// <summary>
        /// The ID of the tweet you are requesting.
        /// </summary>
        /// <remarks></remarks>
        ID,

        /// <summary>
        /// Returns tweets posted after a certain date.
        /// </summary>
        /// <remarks></remarks>
        Since,

        /// <summary>
        /// Returns tweets posted after a certain tweet.
        /// </summary>
        /// <remarks></remarks>
        SinceID,

        /// <summary>
        /// How many tweets should be returned by the request.
        /// </summary>
        /// <remarks>
        /// This defaults to 20.
        /// </remarks>
        Count,

        /// <summary>
        /// Which page of results should be returned.
        /// </summary>
        /// <remarks></remarks>
        Page,

        /// <summary>
        /// The screen name of the user being requested.
        /// </summary>
        /// <remarks></remarks>
        ScreenName,

        // ''' <summary>
        // ''' The ID of the tweet being replied to
        // ''' </summary>
        // ''' <remarks></remarks>
        // InReplyToStatusID

        /// <summary>
        /// The Cursor for cursorbased Requests.
        /// </summary>
        /// <remarks>This Parmaeter is hidden because only used internal</remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        Cursor,

        /// <summary>
        /// The query for user searches
        /// </summary>
        /// <remarks>This Parmaeter is hidden because only used internal</remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        SearchQuery


    }

    /// <summary>
    /// A class that defines a Twitter request.
    /// </summary>
    /// <remarks>
    /// Most TwitterVB actions can be accomplished by just calling the appropriate
    /// methods.  If you are looking for more control over your request, you'll want
    /// to pass a <c>TwitterParameters</c> object.
    /// </remarks>
    public partial class TwitterParameters : Dictionary<TwitterParameterNames, object>
    {

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <remarks></remarks>
        /// <exclude/>
        public TwitterParameters()
        {
        }

        /// <summary>
        /// Builds the Url.
        /// </summary>
        /// <param name="Url">The base url that will be used to build the complete Url.</param>
        /// <returns>A <c>String</c> containing the complete Url.</returns>
        /// <remarks></remarks>
        /// <exclude/>
        public string BuildUrl(string Url)
        {
            if (Count == 0)
            {
                return Url;
            }

            string ParameterString = string.Empty;

            foreach (TwitterParameterNames Key in Keys)
            {
                switch (Key)
                {
                    case TwitterParameterNames.Since:
                        {
                            ParameterString = string.Format("{0}&since={1}", ParameterString, this[Key]);
                            break;
                        }
                    case TwitterParameterNames.SinceID:
                        {
                            ParameterString = string.Format("{0}&since_id={1}", ParameterString, this[Key]);
                            break;
                        }
                    case TwitterParameterNames.Count:
                        {
                            ParameterString = string.Format("{0}&count={1}", ParameterString, this[Key]);
                            break;
                        }
                    case TwitterParameterNames.Page:
                        {
                            ParameterString = string.Format("{0}&page={1}", ParameterString, this[Key]);
                            break;
                        }
                    case TwitterParameterNames.ID:
                        {
                            ParameterString = string.Format("{0}&id={1}", ParameterString, this[Key]);
                            break;
                        }
                    case TwitterParameterNames.ScreenName:
                        {
                            ParameterString = string.Format("{0}&screen_name={1}", ParameterString, this[Key]);
                            break;
                        }
                    case TwitterParameterNames.Cursor:
                        {
                            ParameterString = string.Format("{0}&cursor={1}", ParameterString, this[Key]);
                            break;
                        }
                    case TwitterParameterNames.SearchQuery:
                        {
                            ParameterString = string.Format("{0}&q={1}", ParameterString, this[Key]);
                            break;
                        }
                }
            }

            if (string.IsNullOrEmpty(ParameterString))
            {
                return Url;
            }

            // First char of parameterString is a leading & that should be removed
            return string.Format("{0}?{1}", Url, ParameterString.Remove(0, 1));

        }

        /// <summary>
        /// Adds a parameter to the collection.
        /// </summary>
        /// <param name="Key">The name of the parameter being added.</param>
        /// <param name="Value">The value to be assigned to the parameter.</param>
        /// <remarks></remarks>
        public new void Add(TwitterParameterNames Key, object Value)
        {

            switch (Key)
            {
                case TwitterParameterNames.Since:
                    {
                        if (!(Value is DateTime))
                        {
                            throw new ApplicationException("Value given for since was not a Date.");
                        }

                        DateTime DateValue = Conversions.ToDate(Value);

                        // RFC1123 date string
                        base.Add(Key, DateValue.ToString("r"));
                        break;
                    }

                default:
                    {
                        base.Add(Key, Value.ToString());
                        break;
                    }
            }
        }
    }
}
