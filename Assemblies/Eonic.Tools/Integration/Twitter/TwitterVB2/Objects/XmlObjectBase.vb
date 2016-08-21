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
Imports System.Text.RegularExpressions
Namespace Integration.Twitter.TwitterVB2

    ''' <summary>
    ''' Base class that provides Xml parsing methods to derived classes.
    ''' </summary>
    ''' <remarks></remarks>
    Public MustInherit Class XmlObjectBase

        ''' <summary>
        ''' Default constructor
        ''' </summary>
        ''' <remarks></remarks>
        ''' <exclude/>
        Public Sub New()
        End Sub

        ''' <summary>
        ''' Returns the numeric value of an Xml node.
        ''' </summary>
        ''' <param name="Node">An <c>XmlNode</c> from the Twitter API response.</param>
        ''' <returns>An <c>Int64</c> representing the value of the Xml node.</returns>
        ''' <remarks></remarks>
        Protected Function XmlInt64_Get(ByVal Node As System.Xml.XmlNode) As Int64
            Dim ReturnValue As Int64
            If Node Is Nothing Then
                ReturnValue = 0
            Else
                If Not Int64.TryParse(Node.InnerText, ReturnValue) Then
                    ReturnValue = 0
                End If
            End If
            Return ReturnValue
        End Function

        ''' <summary>
        ''' Returns the text of an Xml node.
        ''' </summary>
        ''' <param name="Node">An <c>XmlNode</c> from the Twitter API response.</param>
        ''' <returns>A <c>String</c> representing the value of the Xml node.</returns>
        ''' <remarks></remarks>
        ''' <exclude/>
        Protected Function XmlString_Get(ByVal Node As System.Xml.XmlNode) As String
            If Node Is Nothing Then
                Return String.Empty
            Else
                Return Node.InnerText
            End If
        End Function

        ''' <summary>
        ''' Returns the boolean value of an Xml node.
        ''' </summary>
        ''' <param name="Node">An <c>XmlNode</c> from the Twitter API response.</param>
        ''' <returns>A <c>Boolean</c> representing the value of the Xml node.</returns>
        ''' <remarks>
        ''' The the node does not exist or is empty, <c>False</c> is returned.
        ''' </remarks>
        ''' <exclude/>
        Protected Function XmlBoolean_Get(ByVal Node As System.Xml.XmlNode) As Boolean
            If Node Is Nothing Then
                Return False
            Else
                Select Case Node.InnerText.ToUpper
                    Case "FALSE"
                        Return False
                    Case "TRUE"
                        Return True
                    Case String.Empty
                        Return False
                End Select
            End If
        End Function

        ''' <summary>
        ''' Returns the date value of an Xml node.
        ''' </summary>
        ''' <param name="Node">An <c>XmlNode</c> from the Twitter API response.</param>
        ''' <returns>A <c>Boolean</c> representing the value of the Xml node.</returns>
        ''' <remarks>
        ''' The the node does not exist or is empty, <c>Date.MinValue()</c> is returned.
        ''' </remarks>
        ''' <exclude/>
        Protected Function XmlDate_Get(ByVal Node As System.Xml.XmlNode) As DateTime
            If Node Is Nothing Then
                Return DateTime.MinValue
            Else
                Dim DateString As String = Node.InnerText
                Dim re As New Regex("(?<DayName>[^ ]+) (?<MonthName>[^ ]+) (?<Day>[^ ]{1,2}) (?<Hour>[0-9]{1,2}):(?<Minute>[0-9]{1,2}):(?<Second>[0-9]{1,2}) (?<TimeZone>[+-][0-9]{4}) (?<Year>[0-9]{4})")
                Dim CreatedAt As Match = re.Match(DateString)
                Dim ParsedDate As DateTime = DateTime.Parse(String.Format("{0} {1} {2} {3}:{4}:{5}", _
                    CreatedAt.Groups("MonthName").Value, _
                    CreatedAt.Groups("Day").Value, _
                    CreatedAt.Groups("Year").Value, _
                    CreatedAt.Groups("Hour").Value, _
                    CreatedAt.Groups("Minute").Value, _
                    CreatedAt.Groups("Second").Value))

                Return ParsedDate
            End If
        End Function
    End Class
End Namespace
