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
Imports System.Xml
''' <summary>
''' Some of the Twitter XML payloads contain namespaces.  They are implemented poorly and inconsistently.  Rather than try to deal with
''' their sloppy namespacing, this class simply removes all of the namespaces and gives us a clean XML document.
''' </summary>
''' <exclude/>
Public Class XmlNamespaceStripper
    Public StrippedXmlDocument As XmlDocument

    Public Sub New(ByVal SourceDocument As XmlDocument)
        StrippedXmlDocument = New XmlDocument()
        StrippedXmlDocument.PreserveWhitespace = True
        For Each child As XmlNode In SourceDocument.ChildNodes
            StrippedXmlDocument.AppendChild(StripNamespace(child))
        Next
    End Sub

    Private Function StripNamespace(ByVal inputNode As XmlNode) As XmlNode
        Dim outputNode As XmlNode = StrippedXmlDocument.CreateNode(inputNode.NodeType, inputNode.LocalName, Nothing)
        If inputNode.Attributes IsNot Nothing Then
            For Each inputAttribute As XmlAttribute In inputNode.Attributes
                If Not (inputAttribute.NamespaceURI = "http://www.w3.org/2000/xmlns/" OrElse inputAttribute.LocalName = "xmlns") Then
                    Dim outputAttribute As XmlAttribute = StrippedXmlDocument.CreateAttribute(inputAttribute.LocalName)
                    outputAttribute.Value = inputAttribute.Value
                    outputNode.Attributes.Append(outputAttribute)
                End If
            Next
        End If

        For Each childNode As XmlNode In inputNode.ChildNodes
            outputNode.AppendChild(StripNamespace(childNode))
        Next

        If inputNode.Value IsNot Nothing Then
            outputNode.Value = inputNode.Value
        End If

        Return outputNode
    End Function
End Class
