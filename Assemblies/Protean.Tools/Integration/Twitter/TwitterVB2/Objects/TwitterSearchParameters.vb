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
'* - Neither the name of Twitter nor the names of its contributors may be 
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

Namespace Integration.Twitter.TwitterVB2

    ''' <summary>
    ''' The parameters that can be used to define a search.
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum TwitterSearchParameterNames

        ''' <summary>
        ''' The text for which to search.
        ''' </summary>
        ''' <remarks></remarks>
        SearchTerm

        ''' <summary>
        ''' Restricts tweets to the given language.
        ''' </summary>
        ''' <remarks>
        ''' The language must be provided as an <a href="http://en.wikipedia.org/wiki/ISO_639-1">ISO 639-1 code</a>
        ''' </remarks>
        Lang

        ''' <summary>
        ''' Specify the language of the query you are sending (only ja is currently effective). This is intended for language-specific clients and the default should work in the majority of cases.
        ''' </summary>
        ''' <remarks></remarks>
        Locale

        ''' <summary>
        ''' The number of tweets to return per page, up to a max of 100.
        ''' </summary>
        ''' <remarks></remarks>
        Rpp

        ''' <summary>
        ''' The page number (starting at 1) to return, up to a max of roughly 1500 results (based on rpp).
        ''' </summary>
        ''' <remarks>
        ''' There are <a href="http://apiwiki.twitter.com/Things-Every-Developer-Should-Know#6Therearepaginationlimits">pagination limits.</a>
        ''' </remarks>
        Page

        ''' <summary>
        ''' Returns tweets with status ids greater than the given id.
        ''' </summary>
        ''' <remarks></remarks>
        SinceID

        ''' <summary>
        ''' Returns tweets after the given date.
        ''' </summary>
        ''' <remarks></remarks>
        Since

        ''' <summary>
        ''' Returns tweets before the given date.
        ''' </summary>
        ''' <remarks></remarks>
        Until

        ''' <summary>
        ''' Returns tweets excluded such from this user.
        ''' </summary>
        ''' <remarks></remarks>
        NotFromUser

        ''' <summary>
        ''' Returns tweets from this user.
        ''' </summary>
        ''' <remarks></remarks>
        FromUser

        ''' <summary>
        ''' Returns tweets addressed to this user.
        ''' </summary>
        ''' <remarks></remarks>
        ToUser


    End Enum

    ''' <summary>
    ''' A class that defines a search.
    ''' </summary>
    ''' <remarks>
    ''' Most search tasks can be accomplished by just calling the <c>Search</c> method of the <c>SearchMethods</c> object and passing it the
    ''' text you are searching for.  If you're looking for more control over your search results, you'll want to pass a <c>TwitterSearchParameters</c> object.
    ''' <para/>
    ''' You define a search by creating an instance of this class, and adding parameters to it.  At the least, you must define the <c>SearchTerm</c>.  See the example below.
    ''' <code source="TwitterVB2\examples.vb" region="Advanced Search" lang="vbnet" title="Using TwitterSearchParameters"></code>
    ''' </remarks>
    Public Class TwitterSearchParameters
        Inherits Dictionary(Of TwitterSearchParameterNames, Object)

        ''' <summary>
        ''' Default constructor
        ''' </summary>
        ''' <remarks></remarks>
        ''' <exclude/>
        Public Sub New()
        End Sub

        ''' <summary>
        ''' Builds the search Url
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
            Dim SearchTermString As String = String.Empty

            For Each Key As TwitterSearchParameterNames In Keys
                Select Case Key
                    Case TwitterSearchParameterNames.Since
                        ParameterString = String.Format("{0}&since={1}", ParameterString, MyBase.Item(Key).ToString)
                    Case TwitterSearchParameterNames.SinceID
                        ParameterString = String.Format("{0}&since_id={1}", ParameterString, MyBase.Item(Key).ToString)
                    Case TwitterSearchParameterNames.Until
                        ParameterString = String.Format("{0}&until={1}", ParameterString, MyBase.Item(Key).ToString)
                    Case TwitterSearchParameterNames.Rpp
                        ParameterString = String.Format("{0}&rpp={1}", ParameterString, MyBase.Item(Key).ToString)
                    Case TwitterSearchParameterNames.Page
                        ParameterString = String.Format("{0}&page={1}", ParameterString, MyBase.Item(Key).ToString)
                    Case TwitterSearchParameterNames.Lang
                        ParameterString = String.Format("{0}&lang={1}", ParameterString, MyBase.Item(Key).ToString)
                    Case TwitterSearchParameterNames.Locale
                        ParameterString = String.Format("{0}&locale={1}", ParameterString, MyBase.Item(Key).ToString)
                    Case TwitterSearchParameterNames.SearchTerm
                        SearchTermString = String.Format("{0}+{1}", SearchTermString, System.Uri.EscapeDataString(MyBase.Item(Key).ToString))
                    Case TwitterSearchParameterNames.NotFromUser
                        SearchTermString = String.Format("{0}+-from%3A{1}", SearchTermString, System.Uri.EscapeDataString(MyBase.Item(Key).ToString))
                    Case TwitterSearchParameterNames.FromUser
                        SearchTermString = String.Format("{0}+from%3A{1}", SearchTermString, System.Uri.EscapeDataString(MyBase.Item(Key).ToString))
                    Case TwitterSearchParameterNames.ToUser
                        SearchTermString = String.Format("{0}+to%3A{1}", SearchTermString, System.Uri.EscapeDataString(MyBase.Item(Key).ToString))

                End Select
            Next

            ParameterString = String.Format("{0}&q={1}", ParameterString, SearchTermString)


            If String.IsNullOrEmpty(ParameterString) Then
                Return Url
            End If

            ' First char of parameterString is a leading & that should be removed
            Debug.WriteLine(String.Format("{0}?{1}", Url, ParameterString.Remove(0, 1)))
            Return String.Format("{0}?{1}", Url, ParameterString.Remove(0, 1))

        End Function

        ''' <summary>
        ''' Adds a parameter to the collection.
        ''' </summary>
        ''' <param name="Key">The name of the parameter being added.</param>
        ''' <param name="Value">The value to be assigned to the parameter.</param>
        ''' <remarks></remarks>
        Public Shadows Sub Add(ByVal Key As TwitterSearchParameterNames, ByVal Value As Object)

            If MyBase.ContainsKey(Key) Then
                Throw New ApplicationException("This Parameter already exist.")
                Exit Sub
            End If

            Select Case Key
                Case TwitterSearchParameterNames.Since
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