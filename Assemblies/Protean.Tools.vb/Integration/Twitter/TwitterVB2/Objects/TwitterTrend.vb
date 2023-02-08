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
Namespace Integration.Twitter.TwitterVB2
    ''' <summary>
    ''' A trend currently tracked by Twitter
    ''' </summary>
    ''' <remarks></remarks>
    Public Class TwitterTrend
        Private _TrendName As String = String.Empty
        Private _TrendText As String = String.Empty
        Private _AsOf As DateTime

        ''' <summary>
        ''' The name of the trend, as displayed on the Twitter website
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property TrendName() As String
            Get
                Return _TrendName
            End Get
            Set(ByVal value As String)
                _TrendName = value
            End Set
        End Property

        ''' <summary>
        ''' Either the URL of a Twitter search, or the terms of a Twitter search, depending on which Trends method was called.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>
        ''' The Twitter API returns different information for different trends requests.
        ''' <para/>
        ''' If you called <c>Trends()</c>, this is a URL to a Twitter search.
        ''' <para/>
        ''' If you called <c>TrendsCurrent()</c>, <c>TrendsDaily()</c> or <c>TrendsWeekly()</c>, this is a Twitter search string.
        ''' </remarks>
        Public Property TrendText() As String
            Get
                Return _TrendText
            End Get
            Set(ByVal value As String)
                _TrendText = value
            End Set
        End Property

        ''' <summary>
        ''' The effective date of the Twitter trend.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>
        ''' If you called <c>TrendsDaily()</c> or <c>TrendsWeekly()</c>, you will usually see multiple trends in the list with different dates.
        ''' For more information, see <a href="http://apiwiki.twitter.com/Twitter-Search-API-Method:-trends-daily">http://apiwiki.twitter.com/Twitter-Search-API-Method:-trends-daily</a>
        ''' or <a href="http://apiwiki.twitter.com/Twitter-Search-API-Method:-trends-weekly">http://apiwiki.twitter.com/Twitter-Search-API-Method:-trends-weekly</a>
        ''' </remarks>
        Public Property AsOf() As DateTime
            Get
                Return _AsOf
            End Get
            Set(ByVal value As DateTime)
                _AsOf = value
            End Set
        End Property

        ''' <summary>
        ''' Creates a <c>TwitterTrend</c> object.
        ''' </summary>
        ''' <param name="Name">The name of the trend.</param>
        ''' <param name="Text">The text of the trend.</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal Name As String, ByVal Text As String)
            Me._TrendName = Name
            Me._TrendText = Text
        End Sub
    End Class
End Namespace
