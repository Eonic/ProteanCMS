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
using System.Text.RegularExpressions;


namespace Protean.Tools.Integration.Twitter.TwitterVB2
{
    /// <summary>
    /// Base class that provides Xml parsing methods to derived classes.
    /// </summary>
    /// <remarks></remarks>
    public abstract partial class XmlObjectBase
    {

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <remarks></remarks>
        /// <exclude/>
        public XmlObjectBase()
        {
        }

        /// <summary>
        /// Returns the numeric value of an Xml node.
        /// </summary>
        /// <param name="Node">An <c>XmlNode</c> from the Twitter API response.</param>
        /// <returns>An <c>Int64</c> representing the value of the Xml node.</returns>
        /// <remarks></remarks>
        protected long XmlInt64_Get(System.Xml.XmlNode Node)
        {
            long ReturnValue;
            if (Node is null)
            {
                ReturnValue = 0L;
            }
            else if (!long.TryParse(Node.InnerText, out ReturnValue))
            {
                ReturnValue = 0L;
            }
            return ReturnValue;
        }

        /// <summary>
        /// Returns the text of an Xml node.
        /// </summary>
        /// <param name="Node">An <c>XmlNode</c> from the Twitter API response.</param>
        /// <returns>A <c>String</c> representing the value of the Xml node.</returns>
        /// <remarks></remarks>
        /// <exclude/>
        protected string XmlString_Get(System.Xml.XmlNode Node)
        {
            if (Node is null)
            {
                return string.Empty;
            }
            else
            {
                return Node.InnerText;
            }
        }

        /// <summary>
        /// Returns the boolean value of an Xml node.
        /// </summary>
        /// <param name="Node">An <c>XmlNode</c> from the Twitter API response.</param>
        /// <returns>A <c>Boolean</c> representing the value of the Xml node.</returns>
        /// <remarks>
        /// The the node does not exist or is empty, <c>False</c> is returned.
        /// </remarks>
        /// <exclude/>
        protected bool XmlBoolean_Get(System.Xml.XmlNode Node)
        {
            if (Node is null)
            {
                return false;
            }
            else
            {
                switch (Node.InnerText.ToUpper() ?? "")
                {
                    case "FALSE":
                        {
                            return false;
                        }
                    case "TRUE":
                        {
                            return true;
                        }

                    case var @case when @case == "":
                        {
                            return false;
                        }
                }
            }

            return default;
        }

        /// <summary>
        /// Returns the date value of an Xml node.
        /// </summary>
        /// <param name="Node">An <c>XmlNode</c> from the Twitter API response.</param>
        /// <returns>A <c>Boolean</c> representing the value of the Xml node.</returns>
        /// <remarks>
        /// The the node does not exist or is empty, <c>Date.MinValue()</c> is returned.
        /// </remarks>
        /// <exclude/>
        protected DateTime XmlDate_Get(System.Xml.XmlNode Node)
        {
            if (Node is null)
            {
                return DateTime.MinValue;
            }
            else
            {
                string DateString = Node.InnerText;
                var re = new Regex("(?<DayName>[^ ]+) (?<MonthName>[^ ]+) (?<Day>[^ ]{1,2}) (?<Hour>[0-9]{1,2}):(?<Minute>[0-9]{1,2}):(?<Second>[0-9]{1,2}) (?<TimeZone>[+-][0-9]{4}) (?<Year>[0-9]{4})");
                var CreatedAt = re.Match(DateString);
                var ParsedDate = DateTime.Parse(string.Format("{0} {1} {2} {3}:{4}:{5}", CreatedAt.Groups["MonthName"].Value, CreatedAt.Groups["Day"].Value, CreatedAt.Groups["Year"].Value, CreatedAt.Groups["Hour"].Value, CreatedAt.Groups["Minute"].Value, CreatedAt.Groups["Second"].Value));






                return ParsedDate;
            }
        }
    }
}
