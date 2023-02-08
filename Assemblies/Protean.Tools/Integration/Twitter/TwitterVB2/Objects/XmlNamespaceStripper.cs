
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
using System.Xml;
/// <summary>
/// Some of the Twitter XML payloads contain namespaces.  They are implemented poorly and inconsistently.  Rather than try to deal with
/// their sloppy namespacing, this class simply removes all of the namespaces and gives us a clean XML document.
/// </summary>
/// <exclude/>

namespace Protean.Tools.Integration.Twitter.TwitterVB2
{
    public partial class XmlNamespaceStripper
    {
        public XmlDocument StrippedXmlDocument;

        public XmlNamespaceStripper(XmlDocument SourceDocument)
        {
            StrippedXmlDocument = new XmlDocument();
            StrippedXmlDocument.PreserveWhitespace = true;
            foreach (XmlNode child in SourceDocument.ChildNodes)
                StrippedXmlDocument.AppendChild(StripNamespace(child));
        }

        private XmlNode StripNamespace(XmlNode inputNode)
        {
            var outputNode = StrippedXmlDocument.CreateNode(inputNode.NodeType, inputNode.LocalName, null);
            if (inputNode.Attributes != null)
            {
                foreach (XmlAttribute inputAttribute in inputNode.Attributes)
                {
                    if (!(inputAttribute.NamespaceURI == "http://www.w3.org/2000/xmlns/" || inputAttribute.LocalName == "xmlns"))
                    {
                        var outputAttribute = StrippedXmlDocument.CreateAttribute(inputAttribute.LocalName);
                        outputAttribute.Value = inputAttribute.Value;
                        outputNode.Attributes.Append(outputAttribute);
                    }
                }
            }

            foreach (XmlNode childNode in inputNode.ChildNodes)
                outputNode.AppendChild(StripNamespace(childNode));

            if (inputNode.Value != null)
            {
                outputNode.Value = inputNode.Value;
            }

            return outputNode;
        }
    }
}
