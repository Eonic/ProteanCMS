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
Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.ComponentModel

Namespace Integration.Twitter.TwitterVB2

    ''' <summary>
    ''' The parameters that can be used to define a Twitter request.
    ''' </summary>
    ''' <remarks>
    ''' Not every TwitterVB method accepts every parameter.  If you are unsure, consult the <a href="http://apiwiki.twitter.com/Twitter-API-Documentation">Twitter API documentation.</a>
    ''' </remarks>
    Public Enum TwitterParameterNames

        ''' <summary>
        ''' The ID of the tweet you are requesting.
        ''' </summary>
        ''' <remarks></remarks>
        ID

        ''' <summary>
        ''' Returns tweets posted after a certain date.
        ''' </summary>
        ''' <remarks></remarks>
        Since

        ''' <summary>
        ''' Returns tweets posted after a certain tweet.
        ''' </summary>
        ''' <remarks></remarks>
        SinceID

        ''' <summary>
        ''' How many tweets should be returned by the request.
        ''' </summary>
        ''' <remarks>
        ''' This defaults to 20.
        ''' </remarks>
        Count

        ''' <summary>
        ''' Which page of results should be returned.
        ''' </summary>
        ''' <remarks></remarks>
        Page

        ''' <summary>
        ''' The screen name of the user being requested.
        ''' </summary>
        ''' <remarks></remarks>
        ScreenName

        '''' <summary>
        '''' The ID of the tweet being replied to
        '''' </summary>
        '''' <remarks></remarks>
        'InReplyToStatusID

        ''' <summary>
        ''' The Cursor for cursorbased Requests.
        ''' </summary>
        ''' <remarks>This Parmaeter is hidden because only used internal</remarks>
        <EditorBrowsable(EditorBrowsableState.Never)> _
        Cursor

        ''' <summary>
        ''' The query for user searches
        ''' </summary>
        ''' <remarks>This Parmaeter is hidden because only used internal</remarks>
        <EditorBrowsable(EditorBrowsableState.Never)> _
        SearchQuery


    End Enum

    ''' <summary>
    ''' A class that defines a Twitter request.
    ''' </summary>
    ''' <remarks>
    ''' Most TwitterVB actions can be accomplished by just calling the appropriate
    ''' methods.  If you are looking for more control over your request, you'll want
    ''' to pass a <c>TwitterParameters</c> object.
    ''' </remarks>
    Public Class TwitterParameters
        Inherits Dictionary(Of TwitterParameterNames, Object)

        ''' <summary>
        ''' Default constructor.
        ''' </summary>
        ''' <remarks></remarks>
        ''' <exclude/>
        Public Sub New()
        End Sub

        ''' <summary>
        ''' Builds the Url.
        ''' </summary>
        ''' <param name="Url">The base url that will be used to build the complete Url.</param>
        ''' <returns>A <c>String</c> containing the complete Url.</returns>
        ''' <remarks></remarks>
        ''' <exclude/>
        Public Function BuildUrl(ByVal Url As String) As String
            If Count = 0 Then
                Return Url
            End If

            Dim ParameterString As String = String.Empty

            For Each Key As TwitterParameterNames In Keys
                Select Case Key
                    Case TwitterParameterNames.Since
                        ParameterString = String.Format("{0}&since={1}", ParameterString, Me(Key))
                    Case TwitterParameterNames.SinceID
                        ParameterString = String.Format("{0}&since_id={1}", ParameterString, Me(Key))
                    Case TwitterParameterNames.Count
                        ParameterString = String.Format("{0}&count={1}", ParameterString, Me(Key))
                    Case TwitterParameterNames.Page
                        ParameterString = String.Format("{0}&page={1}", ParameterString, Me(Key))
                    Case TwitterParameterNames.ID
                        ParameterString = String.Format("{0}&id={1}", ParameterString, Me(Key))
                    Case TwitterParameterNames.ScreenName
                        ParameterString = String.Format("{0}&screen_name={1}", ParameterString, Me(Key))
                    Case TwitterParameterNames.Cursor
                        ParameterString = String.Format("{0}&cursor={1}", ParameterString, Me(Key))
                    Case TwitterParameterNames.SearchQuery
                        ParameterString = String.Format("{0}&q={1}", ParameterString, Me(Key))
                End Select
            Next

            If String.IsNullOrEmpty(ParameterString) Then
                Return Url
            End If

            ' First char of parameterString is a leading & that should be removed
            Return String.Format("{0}?{1}", Url, ParameterString.Remove(0, 1))

        End Function

        ''' <summary>
        ''' Adds a parameter to the collection.
        ''' </summary>
        ''' <param name="Key">The name of the parameter being added.</param>
        ''' <param name="Value">The value to be assigned to the parameter.</param>
        ''' <remarks></remarks>
        Public Shadows Sub Add(ByVal Key As TwitterParameterNames, ByVal Value As Object)

            Select Case Key
                Case TwitterParameterNames.Since
                    If Not (TypeOf Value Is DateTime) Then
                        Throw New ApplicationException("Value given for since was not a Date.")
                    End If

                    Dim DateValue As DateTime = CType(Value, DateTime)

                    ' RFC1123 date string
                    MyBase.Add(Key, DateValue.ToString("r"))

                Case Else
                    MyBase.Add(Key, Value.ToString)
            End Select
        End Sub
    End Class
End Namespace
