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
Imports System.Net
Imports System.Web
Imports System.IO
Imports System.Text

Imports System.Text.RegularExpressions
Imports System.Collections.Specialized
Imports System.Xml


Namespace Integration.Twitter.TwitterVB2
    ''' <summary>
    ''' Provides access to methods that communicate with Twitter.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class TwitterAPI
        Private Twitter_OAuth As New TwitterOAuth
        Private Username As String = String.Empty
        Private Password As String = String.Empty
        ''' <summary>
        ''' Default constructor
        ''' </summary>
        ''' <remarks></remarks>
        ''' <exclude/>
        Public Sub New()
        End Sub


#Region "Twitter API Method Constants"
        ' As Twitter begins rolling out changes to the API, some of these Urls will change.
        ' Having them all in one place makes it easier to change them moving forward.
        Const ACCOUNT_VERIFY_CREDENTIALS As String = "http://api.twitter.com/1/account/verify_credentials.xml"
        Const BLOCKS_BLOCKING As String = "http://api.twitter.com/1/blocks/blocking.xml"
        Const BLOCKS_BLOCKING_IDS As String = "http://api.twitter.com/1/blocks/blocking/ids.xml"
        Const BLOCKS_CREATE As String = "http://api.twitter.com/1/blocks/create/{0}.xml"
        Const BLOCKS_DESTROY As String = "http://api.twitter.com/1/blocks/destroy/{0}.xml"
        Const DIRECT_MESSAGES As String = "http://api.twitter.com/1/direct_messages.xml"
        Const DIRECT_MESSAGES_DESTROY As String = "http://api.twitter.com/1/direct_messages/destroy/{0}.xml"
        Const DIRECT_MESSAGES_NEW As String = "http://api.twitter.com/1/direct_messages/new.xml"
        Const DIRECT_MESSAGES_SENT As String = "http://api.twitter.com/1/direct_messages/sent.xml"
        Const FAVORITES_CREATE As String = "http://api.twitter.com/1/favorites/create/{0}.xml"
        Const FAVORITES_DESTROY As String = "http://api.twitter.com/1/favorites/destroy/{0}.xml"
        Const FAVORITES_LIST As String = "http://api.twitter.com/1/favorites.xml"
        Const FOLLOWERS_URL As String = "http://api.twitter.com/1/followers/ids.xml"
        Const FRIENDS_URL As String = "http://api.twitter.com/1/friends/ids.xml"
        Const FRIENDSHIP_CREATE As String = "http://api.twitter.com/1/friendships/create/{0}.xml"
        Const FRIENDSHIP_SHOW As String = "http://api.twitter.com/1/friendships/show.xml?{0}={1}&{2}={3}"
        Const FRIENDSHIP_DESTROY As String = "http://api.twitter.com/1/friendships/destroy/{0}.xml"
        Const LISTS As String = "http://api.twitter.com/1/{0}/lists.xml"
        Const LISTS_MEMBERS As String = "http://api.twitter.com/1/{0}/{1}/members.xml"
        Const LISTS_MEMBERS_ID As String = "http://api.twitter.com/1/{0}/{1}/members/{2}.xml"
        Const LISTS_MEMBERSHIPS As String = "http://api.twitter.com/1/{0}/lists/memberships.xml"
        Const LISTS_SPECIFY As String = "http://api.twitter.com/1/{0}/lists/{1}.xml"
        Const LISTS_STATUSES As String = "http://api.twitter.com/1/{0}/lists/{1}/statuses.xml"
        Const LISTS_SUBSCRIBERS As String = "http://api.twitter.com/1/{0}/{1}/subscribers.xml"
        Const LISTS_SUBSCRIBERS_ID As String = "http://api.twitter.com/1/{0}/{1}/subscribers/{2}.xml"
        Const LISTS_SUBSCRIPTIONS As String = "http://api.twitter.com/1/{0}/lists/subscriptions.xml"
        Const REPORT_SPAM As String = "http://api.twitter.com/1/report_spam.xml"
        Const SEARCH_URL As String = "http://search.twitter.com/search.atom"
        Const STATUSES_DESTROY As String = "http://api.twitter.com/1/statuses/destroy/{0}.xml"
        Const STATUSES_FRIENDS As String = "http://api.twitter.com/1/statuses/friends.xml"
        Const STATUSES_FOLLOWERS As String = "http://api.twitter.com/1/statuses/followers.xml"
        Const STATUSES_FRIENDS_TIMELINE As String = "http://api.twitter.com/1/statuses/friends_timeline.xml"
        Const STATUSES_HOME_TIMELINE As String = "http://api.twitter.com/1/statuses/home_timeline.xml"
        Const STATUSES_MENTIONS As String = "http://api.twitter.com/1/statuses/mentions.xml"
        Const STATUSES_PUBLIC_TIMELINE As String = "http://api.twitter.com/1/statuses/public_timeline.xml"
        Const STATUSES_REPLIES As String = "http://api.twitter.com/1/statuses/replies.xml"
        Const STATUSES_RETWEET As String = "http://api.twitter.com/1/statuses/retweet/{0}.xml"
        Const STATUSES_RETWEETED_BY_ME As String = "http://api.twitter.com/1/statuses/retweeted_by_me.xml"
        Const STATUSES_RETWEETED_TO_ME As String = "http://twitter.com/statuses/retweeted_to_me.xml"
        Const STATUSES_RETWEETS As String = "http://api.twitter.com/1/statuses/retweets/id.xml"
        Const STATUSES_RETWEETS_OF_ME As String = "http://api.twitter.com/1/statuses/retweets_of_me.xml"
        Const STATUSES_SHOW As String = "http://api.twitter.com/1/statuses/show/{0}.xml"
        Const STATUSES_UPDATE As String = "http://api.twitter.com/1/statuses/update.xml"
        Const STATUSES_USER_TIMELINE As String = "http://api.twitter.com/1/statuses/user_timeline.xml"
        Const TRENDS_CURRENT As String = "http://search.twitter.com/trends/current.json"
        Const TRENDS_DAILY As String = "http://search.twitter.com/trends/daily.json"
        Const TRENDS_URL As String = "http://search.twitter.com/trends.json"
        Const TRENDS_WEEKLY As String = "http://search.twitter.com/trends/weekly.json"
        Const TREND_LOCATIONS As String = "http://api.twitter.com/1/trends/available.xml"
        Const TREND_LOCATION_TRENDS As String = "http://api.twitter.com/1/trends/{0}.json"
        Const USERS_SEARCH As String = "http://api.twitter.com/1/users/search.xml"
        Const USERS_SHOW As String = "http://api.twitter.com/1/users/show.xml"
#End Region

#Region "API Rate Limit Properties"
        ''' <summary>
        ''' The number of API calls remaining in the current API period.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property RateLimit_RemainingHits() As String
            Get
                Return Globals.API_RemainingHits
            End Get
        End Property

        ''' <summary>
        ''' The total number of API calls allowed in the current API period.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property RateLimit_HourlyLimit() As String
            Get
                Return Globals.API_HourlyLimit
            End Get
        End Property

        ''' <summary>
        ''' The time when the next API period starts and <c>RemainingHits</c> will reset.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property RateLimit_Reset() As String
            Get
                Return Globals.API_Reset
            End Get
        End Property
#End Region

#Region "OAuth Properties"
        ''' <summary>
        ''' The OAuth Token generated by the OAuth process.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property OAuth_Token() As String
            Get
                Return Me.Twitter_OAuth.Token
            End Get
        End Property

        ''' <summary>
        ''' The OAuth Token Secret generated by the OAuth process.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property OAuth_TokenSecret() As String
            Get
                Return Me.Twitter_OAuth.TokenSecret
            End Get
        End Property

#End Region

#Region "Proxy Authentication Properties"
        ''' <summary>
        ''' The username that will be passed to the default proxy server.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ProxyUsername() As String
            Get
                Return Globals.Proxy_Username
            End Get
            Set(ByVal value As String)
                Globals.Proxy_Username = value
            End Set
        End Property

        ''' <summary>
        ''' The password that will be passed to the default proxy server.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ProxyPassword() As String
            Get
                Return Globals.Proxy_Password
            End Get
            Set(ByVal value As String)
                Globals.Proxy_Password = value
            End Set
        End Property
#End Region

#Region "Authentication Methods"


        'Public Sub AuthenticateAs(ByVal Username As String, ByVal Password As String)
        '    Me.Twitter_OAuth = Nothing
        '    Me.Username = Username
        '    Me.Password = Password
        'End Sub


#End Region

#Region "Xauth"
        Public Sub XAuth(ByVal p_strUserName As String, ByVal p_strPassword As String, ByVal p_strConsumer As String, ByVal p_strCKSecret As String)
            Twitter_OAuth.ConsumerKey = p_strConsumer
            Twitter_OAuth.ConsumerSecret = p_strCKSecret

            Twitter_OAuth.GetXAccess(p_strUserName, p_strPassword)
            'If Twitter_OAuth.Token.Length > 0 Then
            '    MsgBox("Success")
            'Else
            '    MsgBox("Failure")
            'End If

        End Sub
#End Region

#Region "Support Methods"

        '
        ' This region holds general support and utility methods.
        ' They are explicitly excluded from the API documentation.
        '

        ''' <exclude/>
        Private Function PerformWebRequest(ByVal Url As String) As String
            Dim ReturnValue As String = String.Empty

            Dim Request As HttpWebRequest = CType(System.Net.WebRequest.Create(New Uri(Url)), HttpWebRequest)
            With Request
                .Method = "GET"
                .MaximumAutomaticRedirections = 4
                .MaximumResponseHeadersLength = 4
                .ContentLength = 0
            End With

            Try
                Dim ResponseText As String = String.Empty
                Using Response As HttpWebResponse = CType(Request.GetResponse, HttpWebResponse)
                    Using ReceiveStream As Stream = Response.GetResponseStream
                        Using ReadStream As StreamReader = New StreamReader(ReceiveStream, System.Text.Encoding.UTF8)
                            ReturnValue = ReadStream.ReadToEnd
                        End Using
                    End Using
                End Using
            Catch
                ReturnValue = String.Empty
            End Try

            Return ReturnValue
        End Function

        ''' <exclude/>
        Private Function PerformWebRequest(ByVal Url As String, ByVal HTTPMethod As String) As String
            Dim ReturnValue As String = String.Empty
            Dim AuthType As String = String.Empty

            If Me.Twitter_OAuth IsNot Nothing Then
                ' This is an OAuth request
                AuthType = "OAUTH"
                Dim OAuthMethod As TwitterOAuth.Method
                If HTTPMethod.ToUpper = "GET" Then
                    OAuthMethod = TwitterOAuth.Method.GET
                Else
                    OAuthMethod = TwitterOAuth.Method.POST
                End If

                ReturnValue = Me.Twitter_OAuth.OAuthWebRequest(OAuthMethod, Url, String.Empty)
            Else
                Try
                    ' This is a Basic Auth request
                    AuthType = "BASIC"
                    Dim Request As HttpWebRequest = CType(System.Net.WebRequest.Create(Url), HttpWebRequest)
                    With Request
                        .Method = HTTPMethod
                        .MaximumAutomaticRedirections = 4
                        .MaximumResponseHeadersLength = 4
                        .ContentLength = 0
                        .Credentials = New NetworkCredential(Me.Username, Me.Password)
                    End With

                    Dim Response As HttpWebResponse = CType(Request.GetResponse, HttpWebResponse)
                    Globals.API_HourlyLimit = Response.GetResponseHeader("X-RateLimit-Limit")
                    Globals.API_RemainingHits = Response.GetResponseHeader("X-RateLimit-Remaining")
                    Globals.API_Reset = Response.GetResponseHeader("X-RateLimit-Reset")
                    Using ReceiveStream As Stream = Response.GetResponseStream
                        Using ReadStream As New StreamReader(ReceiveStream, Encoding.UTF8)
                            ReturnValue = ReadStream.ReadToEnd
                        End Using
                    End Using
                    Response.Close()

                Catch ex As Exception
                    Dim Message As String = Nothing
                    If TypeOf ex Is WebException Then
                        Try
                            Dim Doc As New XmlDocument
                            ReturnValue = New StreamReader(CType(ex, WebException).Response.GetResponseStream, Encoding.UTF8).ReadToEnd
                            Doc.LoadXml(ReturnValue)
                            Message = Doc.SelectSingleNode("hash/error").InnerText
                        Catch
                        End Try
                    End If
                    Dim tax As New TwitterAPIException(Message, ex)
                    With tax
                        .Url = Url
                        .Method = HTTPMethod
                        .AuthType = AuthType
                        .Response = Nothing
                        .Status = Nothing
                    End With
                    Throw tax
                End Try
            End If

            Return ReturnValue
        End Function

        ''' <exclude/>
        Private Function EndingQuote(ByVal Text As String, ByVal Start As Integer) As Integer
            Dim q As Integer = Start
            Do
                q = Text.IndexOf(Chr(34), q)
                If q > 0 AndAlso Text.Substring(q - 1, 1) <> "\" Then
                    Return q
                End If
                q += 1
            Loop Until q >= Text.Length
        End Function

        ''' <exclude/>
        Private Function GetNextQuotedString(ByVal Text As String, ByRef Position As Integer) As String

            Dim q1 As Integer
            Dim q2 As Integer

            q1 = Text.IndexOf(Chr(34), Position)
            Position = q1 + 1

            If q1 = -1 Then
                Return String.Empty
            End If
            Do
                q2 = Text.IndexOf(Chr(34), Position)
                Position = q2 + 1
            Loop Until Text.Substring(q2 - 1, 1) <> "\"

            Return Text.Substring(q1 + 1, q2 - q1 - 1)

        End Function

        ''' <exclude/>
        Private Function CleanJsonText(ByVal Text As String) As String
            Dim sb As New System.Text.StringBuilder(Text)
            sb.Replace("\""", """")
            sb.Replace("\\", "\")
            sb.Replace("\/", "/")
            Return sb.ToString
        End Function

        ''' <exclude/>
        Private Function GetImageContentType(ByVal Filename As String) As String
            If Filename.ToUpper.EndsWith(".GIF") Then
                Return "image/gif"
            ElseIf Filename.ToUpper.EndsWith(".JPG") Then
                Return "image/jpeg"
            ElseIf Filename.ToUpper.EndsWith(".JPEG") Then
                Return "image/jpeg"
            ElseIf Filename.ToUpper.EndsWith(".PNG") Then
                Return "image/png"
            Else
                Return String.Empty
            End If
        End Function

#End Region

#Region "Parsing Methods"

        '
        ' The methods in this region are internal functions for parsing Twitter responses.
        ' They are private and explicitly excluded from the API documentation.
        ' 

        ''' <exclude/>
        Private Function ParseRelationships(ByVal Xml As String) As List(Of TwitterRelationship)
            Dim XmlDoc As New XmlDocument
            XmlDoc.LoadXml(Xml)

            Dim Relationships As New List(Of TwitterRelationship)
            For Each RelationshipNode As XmlNode In XmlDoc.SelectNodes("//relationship")
                Relationships.Add(New TwitterRelationship(RelationshipNode))
            Next

            Return Relationships
        End Function

        ''' <exclude/>
        Private Function ParseDirectMessages(ByVal Xml As String) As List(Of TwitterDirectMessage)
            Dim XmlDoc As New XmlDocument
            XmlDoc.LoadXml(Xml)

            Dim Messages As New List(Of TwitterDirectMessage)
            For Each DirectMessageNode As XmlNode In XmlDoc.SelectNodes("//direct_message")
                Messages.Add(New TwitterDirectMessage(DirectMessageNode))
            Next

            Return Messages
        End Function

        ''' <exclude/>
        Private Function ParseLists(ByVal Xml As String) As List(Of TwitterList)
            Dim XmlDoc As New XmlDocument
            XmlDoc.LoadXml(Xml)

            Dim Lists As New List(Of TwitterList)
            For Each ListNode As XmlNode In XmlDoc.SelectNodes("//list")
                Lists.Add(New TwitterList(ListNode))
            Next
            Return Lists
        End Function

        ''' <exclude/>
        Private Function ParseLists(ByVal Xml As String, ByRef NextCursor As Int64) As List(Of TwitterList)
            Dim XmlDoc As New XmlDocument
            XmlDoc.LoadXml(Xml)

            Dim Lists As New List(Of TwitterList)
            For Each ListNode As XmlNode In XmlDoc.SelectNodes("//list")
                Lists.Add(New TwitterList(ListNode))
            Next

            NextCursor = Convert.ToInt64(XmlDoc.SelectSingleNode("//next_cursor").InnerText)
            Return Lists
        End Function

        ''' <exclude/>
        Private Function ParseStatuses(ByVal Xml As String) As List(Of TwitterStatus)
            Dim XmlDoc As New XmlDocument
            XmlDoc.LoadXml(Xml)

            Dim Stripper As New XmlNamespaceStripper(XmlDoc)
            XmlDoc = Stripper.StrippedXmlDocument

            Dim Statuses As New List(Of TwitterStatus)
            For Each StatusNode As XmlNode In XmlDoc.SelectNodes("//status")
                Statuses.Add(New TwitterStatus(StatusNode))
            Next
            Return Statuses
        End Function

        ''' <exclude/>
        Private Function ParseUsers(ByVal Xml As String) As List(Of TwitterUser)
            Dim XmlDoc As New XmlDocument
            XmlDoc.LoadXml(Xml)

            Dim Users As New List(Of TwitterUser)
            For Each UserNode As XmlNode In XmlDoc.SelectNodes("//user")
                Users.Add(New TwitterUser(UserNode))
            Next
            Return Users
        End Function

        ''' <exclude/>
        Private Function ParseUsers(ByVal Xml As String, ByRef NextCursor As Int64) As List(Of TwitterUser)
            Dim XmlDoc As New XmlDocument
            XmlDoc.LoadXml(Xml)

            Dim Users As New List(Of TwitterUser)
            For Each UserNode As XmlNode In XmlDoc.SelectNodes("//user")
                Users.Add(New TwitterUser(UserNode))
            Next

            NextCursor = Convert.ToInt64(XmlDoc.SelectSingleNode("//next_cursor").InnerText)
            Return Users

        End Function

        ''' <exclude/>
        Private Function ParseBlockedIDs(ByVal Xml As String) As List(Of Int64)
            Dim XmlDoc As New XmlDocument
            XmlDoc.LoadXml(Xml)

            Dim IDs As New List(Of Int64)
            For Each IDNode As XmlNode In XmlDoc.SelectNodes("//id")
                IDs.Add(Int64.Parse(IDNode.InnerText))
            Next
            Return IDs
        End Function

        ''' <exclude/>
        Private Function ParseSearchResults(ByVal Xml As String) As List(Of TwitterSearchResult)
            Dim XmlDoc As New XmlDocument
            XmlDoc.LoadXml(Xml)

            Dim Stripper As New XmlNamespaceStripper(XmlDoc)
            XmlDoc = Stripper.StrippedXmlDocument

            Dim Results As New List(Of TwitterSearchResult)
            For Each SearchResultNode As XmlNode In XmlDoc.SelectNodes("//entry")
                Results.Add(New TwitterSearchResult(SearchResultNode))
            Next
            Return Results
        End Function

        ''' <exclude/>
        Private Function ParseSocialGraph(ByVal Xml As String, ByRef NextCursor As Int64) As List(Of Int64)
            Dim XmlDoc As New XmlDocument
            XmlDoc.LoadXml(Xml)

            Dim SocialGraph As New List(Of Int64)
            For Each IDNode As XmlNode In XmlDoc.SelectNodes("//id")
                SocialGraph.Add(Convert.ToInt64(IDNode.InnerText))
            Next

            NextCursor = Convert.ToInt64(XmlDoc.SelectSingleNode("//next_cursor").InnerText)
            Return SocialGraph
        End Function

        Private Function ParseTrendLocations(ByVal Xml As String) As List(Of TwitterTrendLocation)
            Dim XmlDoc As New XmlDocument
            XmlDoc.LoadXml(Xml)

            Dim Stripper As New XmlNamespaceStripper(XmlDoc)
            XmlDoc = Stripper.StrippedXmlDocument

            Dim Results As New List(Of TwitterTrendLocation)
            For Each SearchResultNode As XmlNode In XmlDoc.SelectNodes("//location")
                Results.Add(New TwitterTrendLocation(SearchResultNode))
            Next
            Return Results
        End Function

        ''' <exclude/>
        Private Function ParseTrends(ByVal Json As String, ByVal TrendType As Globals.TrendFormat) As List(Of TwitterTrend)

            Dim ReturnValue As New List(Of TwitterTrend)
            Dim AsOf As DateTime
            Dim Position As Integer
            Dim TrendName As String = String.Empty
            Dim TrendText As String = String.Empty

            Select Case TrendType

                Case TrendFormat.Trends
                    Do
                        Dim Text As String = GetNextQuotedString(Json, Position)
                        Select Case Text
                            Case "name"
                                TrendName = GetNextQuotedString(Json, Position)

                            Case "url"
                                TrendText = CleanJsonText(GetNextQuotedString(Json, Position))
                                ReturnValue.Add(New TwitterTrend(TrendName, TrendText))

                            Case "as_of"
                                Dim AsOfString As String = GetNextQuotedString(Json, Position)
                                Dim DateString As String = Replace(AsOfString, ",", "")
                                Dim re As New Regex("(?<DayName>[^ ]+) (?<Day>[^ ]{1,2}) (?<MonthName>[^ ]+) (?<Year>[0-9]{4}) (?<Hour>[0-9]{1,2}):(?<Minute>[0-9]{1,2}):(?<Second>[0-9]{1,2}) (?<TimeZone>[+-][0-9]{4})")
                                Dim CreatedAt As Match = re.Match(DateString)
                                AsOf = DateTime.Parse(String.Format("{0} {1} {2} {3}:{4}:{5}", _
                                    CreatedAt.Groups("MonthName").Value, _
                                    CreatedAt.Groups("Day").Value, _
                                    CreatedAt.Groups("Year").Value, _
                                    CreatedAt.Groups("Hour").Value, _
                                    CreatedAt.Groups("Minute").Value, _
                                    CreatedAt.Groups("Second").Value))
                                Exit Do
                        End Select
                    Loop

                    For Each t As TwitterTrend In ReturnValue
                        t.AsOf = AsOf
                    Next

                Case TrendFormat.Current
                    Do
                        Dim Text As String = GetNextQuotedString(Json, Position)
                        Select Case Text
                            Case "trends"
                                AsOf = Convert.ToDateTime(GetNextQuotedString(Json, Position))

                            Case "name"
                                TrendName = GetNextQuotedString(Json, Position)

                            Case "query"
                                TrendText = CleanJsonText(GetNextQuotedString(Json, Position))
                                Dim NewTrend As New TwitterTrend(TrendName, TrendText)
                                NewTrend.AsOf = AsOf
                                ReturnValue.Add(NewTrend)
                            Case String.Empty
                                Exit Do
                        End Select
                    Loop

                Case TrendFormat.Daily
                    Do
                        Dim Text As String = GetNextQuotedString(Json, Position)
                        Select Case Text
                            Case "name"
                                TrendName = GetNextQuotedString(Json, Position)

                            Case "query"
                                TrendText = CleanJsonText(GetNextQuotedString(Json, Position))
                                Dim NewTrend As New TwitterTrend(TrendName, TrendText)
                                NewTrend.AsOf = AsOf
                                ReturnValue.Add(NewTrend)
                            Case String.Empty
                                Exit Do

                            Case Else
                                Dim ao As DateTime
                                If DateTime.TryParse(Text, ao) Then
                                    AsOf = ao
                                End If

                        End Select
                    Loop

                Case TrendFormat.Weekly
                    Do
                        Dim Text As String = GetNextQuotedString(Json, Position)
                        Select Case Text
                            Case "name"
                                TrendName = GetNextQuotedString(Json, Position)

                            Case "query"
                                TrendText = CleanJsonText(GetNextQuotedString(Json, Position))
                                Dim NewTrend As New TwitterTrend(TrendName, TrendText)
                                NewTrend.AsOf = AsOf
                                ReturnValue.Add(NewTrend)
                            Case String.Empty
                                Exit Do

                            Case Else
                                Dim ao As DateTime
                                If DateTime.TryParse(Text, ao) Then
                                    AsOf = ao
                                End If

                        End Select
                    Loop
                Case TrendFormat.ByLocation
                    Do
                        Dim Text As String = GetNextQuotedString(Json, Position)
                        Select Case Text
                            Case "as_of"
                                AsOf = Convert.ToDateTime(GetNextQuotedString(Json, Position))
                                Exit Do
                            Case String.Empty
                                AsOf = Now
                                Exit Do
                        End Select
                    Loop

                    Position = 0
                    Do
                        Dim Text As String = GetNextQuotedString(Json, Position)
                        Select Case Text
                            Case "as_of"
                                Exit Do
                            Case "name"
                                TrendName = GetNextQuotedString(Json, Position)

                                Dim NewTrend As New TwitterTrend(TrendName, TrendText)

                                NewTrend.AsOf = AsOf
                                ReturnValue.Add(NewTrend)
                            Case "url"
                                TrendText = CleanJsonText(GetNextQuotedString(Json, Position))

                            Case String.Empty
                                Exit Do

                        End Select
                    Loop
            End Select

            Return ReturnValue

        End Function
#End Region

#Region "Twitter API Methods"

        '
        ' The methods in this region implement the methods documented in the Twitter API Documentation found at
        ' http://apiwiki.twitter.com/Twitter-API-Documentation
        '
        ' These methods have been grouped into regions that mirrors the groupings in the API documentation so that
        ' it is easier to find the method you are looking for (or where to put the method that you are writing).
        '
        ' The only methods that should be in this region are methods that directly implement a Twitter API method.
        ' If in doubt, ask Duane. :)
        '

#Region "Search API Methods"
        ''' <summary>
        ''' Performs a basic Twitter search for the provided text.
        ''' </summary>
        ''' <param name="SearchTerm">The text for which to search.</param>
        ''' <returns>A <c>List(Of TwitterSearchResult)</c> representing the tweets returned by the search.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Basic Search" lang="vbnet" title="Performing a search"></code>
        ''' </remarks>
        Public Function Search(ByVal SearchTerm As String) As List(Of TwitterSearchResult)
            Dim Parameters As New TwitterSearchParameters
            Parameters.Add(TwitterSearchParameterNames.SearchTerm, SearchTerm)

            Dim Url As String = Parameters.BuildUrl(SEARCH_URL)
            Return ParseSearchResults(PerformWebRequest(Url, "GET"))
        End Function

        ''' <summary>
        ''' Performs an advanced Twitter search.
        ''' </summary>
        ''' <param name="Parameters">A <seealso>TwitterSearchParameters</seealso> object that defines the search.</param>
        ''' <returns>A <c>List(Of TwitterSearchResult)</c> that represents the list of tweets found by the search.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Advanced Search" lang="vbnet" title="Performing a search"></code>
        ''' </remarks>
        Public Function Search(ByVal Parameters As TwitterSearchParameters) As List(Of TwitterSearchResult)
            Dim Url As String = Parameters.BuildUrl(SEARCH_URL)
            Return ParseSearchResults(PerformWebRequest(Url, "GET"))
        End Function

        ''' <summary>
        ''' Returns the top ten topics that are currently trending on Twitter.  The response includes the time of the request, 
        ''' the name of each trend, and the url to the Twitter Search results page for that topic.
        ''' </summary>
        ''' <returns>A <c>List(Of TwitterTrend)</c> representing the trends.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Trends" lang="vbnet" title="Retrieving trend information"></code>
        ''' </remarks>
        Public Function Trends() As List(Of TwitterTrend)
            Return ParseTrends(PerformWebRequest(TRENDS_URL, "GET"), TrendFormat.Trends)
        End Function

        ''' <summary>
        ''' Returns the current top 10 trending topics on Twitter.  The response includes the time of the request, 
        ''' the name of each trending topic, and query used on Twitter Search results page for that topic.
        ''' </summary>
        ''' <returns>A <c>List(Of TwitterTrend)</c> representing the trends.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Trends" lang="vbnet" title="Retrieving trend information"></code>
        ''' </remarks>
        Public Function TrendsCurrent() As List(Of TwitterTrend)
            Return ParseTrends(PerformWebRequest(TRENDS_CURRENT, "GET"), TrendFormat.Current)
        End Function

        ''' <summary>
        ''' Returns the top 20 trending topics for each hour in a given day.
        ''' </summary>
        ''' <returns>A <c>List(Of TwitterTrend)</c> representing the trends.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Trends" lang="vbnet" title="Retrieving trend information"></code>
        ''' </remarks>
        Public Function TrendsDaily() As List(Of TwitterTrend)
            Return ParseTrends(PerformWebRequest(TRENDS_DAILY, "GET"), TrendFormat.Daily)
        End Function

        ''' <summary>
        ''' Returns the top 30 trending topics for each day in a given week.
        ''' </summary>
        ''' <returns>A <c>List(Of TwitterTrend)</c> representing the trends.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Trends" lang="vbnet" title="Retrieving trend information"></code>
        ''' </remarks>
        Public Function TrendsWeekly() As List(Of TwitterTrend)
            Return ParseTrends(PerformWebRequest(TRENDS_WEEKLY, "GET"), TrendFormat.Weekly)
        End Function
#End Region

#Region "Timeline Methods"
        ''' <summary>
        ''' Gets the 20 most recent tweets from the public timeline
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Get Public Timeline" lang="vbnet" title="Getting the public timeline"></code>
        ''' </remarks>
        Public Function PublicTimeline() As List(Of TwitterStatus)
            Return PublicTimeline(Nothing)
        End Function

        ''' <summary>
        ''' Gets recent tweets from the public timeline
        ''' </summary>
        ''' <param name="Parameters">A <see>TwitterParameters</see> object defining how the request should be executed.</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Get Public Timeline" lang="vbnet" title="Getting the public timeline"></code>
        ''' </remarks>
        Public Function PublicTimeline(ByVal Parameters As TwitterParameters) As List(Of TwitterStatus)
            Dim Url As String = STATUSES_PUBLIC_TIMELINE
            If Parameters IsNot Nothing Then
                Url = Parameters.BuildUrl(Url)
            End If
            Return ParseStatuses(PerformWebRequest(Url, "GET"))
        End Function

        ''' <summary>
        ''' Retrieves tweets from the authenticated user and the authenticated user's friends
        ''' </summary>
        ''' <returns>A <c>List(Of TwitterStatus)</c> containing the requested tweets.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Get Home Timeline" lang="vbnet" title="Getting the home timeline"></code>
        ''' When Twitter publishes the "retweet" changes to the API, this will replace <c>FriendsTimeline()</c>.
        ''' See <a href="http://apiwiki.twitter.com/Twitter-REST-API-Method:-statuses-home_timeline">http://apiwiki.twitter.com/Twitter-REST-API-Method:-statuses-home_timeline</a> for details.
        ''' </remarks>
        Public Function HomeTimeline() As List(Of TwitterStatus)
            Return HomeTimeline(Nothing)
        End Function

        ''' <summary>
        ''' Retrieves tweets from the authenticated user and the authenticated user's friends
        ''' </summary>
        ''' <param name="Parameters">A <see>TwitterParameters</see> object defining how the request should be executed.</param>
        ''' <returns>A <c>List(Of TwitterStatus)</c> containing the requested tweets.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Get Home Timeline" lang="vbnet" title="Getting the home timeline"></code>
        ''' When Twitter publishes the "retweet" changes to the API, this will replace <c>FriendsTimeline()</c>.
        ''' See <a href="http://apiwiki.twitter.com/Twitter-REST-API-Method:-statuses-home_timeline">http://apiwiki.twitter.com/Twitter-REST-API-Method:-statuses-home_timeline</a> for details.
        ''' </remarks>
        Public Function HomeTimeline(ByVal Parameters As TwitterParameters) As List(Of TwitterStatus)
            Dim Url As String = STATUSES_HOME_TIMELINE
            If Parameters IsNot Nothing Then
                Url = Parameters.BuildUrl(Url)
            End If
            Return ParseStatuses(PerformWebRequest(Url, "GET"))
        End Function

        ''' <summary>
        ''' Retrieves tweets from the authenticated user and the authenticated user's friends
        ''' </summary>
        ''' <returns>A <c>List(Of TwitterStatus)</c> containing the requested tweets.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Get Friends Timeline" lang="vbnet" title="Getting the friends timeline"></code>
        ''' </remarks>
        Public Function FriendsTimeline() As List(Of TwitterStatus)
            Return FriendsTimeline(Nothing)
        End Function

        ''' <summary>
        ''' Retrieves tweets from the authenticated user and the authenticated user's friends
        ''' </summary>
        ''' <param name="Parameters">A <see>TwitterParameters</see> object defining how the request should be executed.</param>
        ''' <returns>A <c>List(Of TwitterStatus)</c> containing the requested tweets.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Get Friends Timeline" lang="vbnet" title="Getting the friends timeline"></code>
        ''' </remarks>
        Public Function FriendsTimeline(ByVal Parameters As TwitterParameters) As List(Of TwitterStatus)
            Dim Url As String = STATUSES_FRIENDS_TIMELINE
            If Parameters IsNot Nothing Then
                Url = Parameters.BuildUrl(Url)
            End If
            Return ParseStatuses(PerformWebRequest(Url, "GET"))
        End Function

        ''' <summary>
        ''' Returns a list of tweets from the authenticating user.
        ''' </summary>
        ''' <returns>A <c>List(Of TwitterStatus)</c> containing the requested tweets.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Get User Timeline" lang="vbnet" title="Getting the user timeline"></code>
        ''' </remarks>
        Public Function UserTimeline() As List(Of TwitterStatus)
            Dim Parameters As TwitterParameters = Nothing
            Return UserTimeline(Parameters)
        End Function

        ''' <summary>
        ''' Returns a list of tweets from the specified user
        ''' </summary>
        ''' <param name="ScreenName">The screen name of the user whose tweets will be returned.</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Get User Timeline" lang="vbnet" title="Getting the user timeline"></code>
        ''' </remarks>
        Public Function UserTimeline(ByVal ScreenName As String) As List(Of TwitterStatus)
            Dim Parameters As New TwitterParameters
            Parameters.Add(TwitterParameterNames.ScreenName, ScreenName)
            Return UserTimeline(Parameters)
        End Function

        ''' <summary>
        ''' Returns a list of tweets from the specified user.
        ''' </summary>
        ''' <param name="Parameters">A <see>TwitterParameters</see> object defining how the request should be executed.</param>
        ''' <returns>A <c>List(Of TwitterStatus)</c> containing the requested tweets.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Get User Timeline" lang="vbnet" title="Getting the user timeline"></code>
        ''' </remarks>
        Public Function UserTimeline(ByVal Parameters As TwitterParameters) As List(Of TwitterStatus)
            Dim Url As String = STATUSES_USER_TIMELINE
            If Parameters IsNot Nothing Then
                Url = Parameters.BuildUrl(Url)
            End If
            Return ParseStatuses(PerformWebRequest(Url, "GET"))
        End Function

        ''' <summary>
        ''' Retrieves tweets that contain the screen name of the authenticating user.
        ''' </summary>
        ''' <returns>A <c>List(Of TwitterStatus)</c> containing the requested tweets.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Get Mentions" lang="vbnet" title="Getting mentions"></code>
        ''' </remarks>
        Public Function Mentions() As List(Of TwitterStatus)
            Return Mentions(Nothing)
        End Function

        ''' <summary>
        ''' Retrieves tweets that contain the screen name of the authenticating user.
        ''' </summary>
        ''' <param name="Parameters">A <see>TwitterParameters</see> object defining how the request should be executed.</param>
        ''' <returns>A <c>List(Of TwitterStatus)</c> containing the requested tweets.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Get Mentions" lang="vbnet" title="Getting entions"></code>
        ''' </remarks>
        Public Function Mentions(ByVal Parameters As TwitterParameters) As List(Of TwitterStatus)
            Dim Url As String = STATUSES_MENTIONS
            If Parameters IsNot Nothing Then
                Url = Parameters.BuildUrl(Url)
            End If
            Return ParseStatuses(PerformWebRequest(Url, "GET"))
        End Function

        ''' <summary>
        ''' Retrieves retweets posted by the authenticating user.
        ''' </summary>
        ''' <returns>A <c>List(Of TwitterStatus)</c> containing the requested retweets.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Retweeted By Me" lang="vbnet" title="Getting tweets you retweeted"></code>
        ''' </remarks>
        Public Function RetweetedByMe() As List(Of TwitterStatus)
            Return RetweetedByMe(Nothing)
        End Function

        ''' <summary>
        ''' Retrieves retweets posted by the authenticating user.
        ''' </summary>
        ''' <param name="Parameters">A <see>TwitterParameters</see> object defining how the request should be executed.</param>
        ''' <returns>A <c>List(Of TwitterStatus)</c> containing the requested retweets.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Retweeted By Me" lang="vbnet" title="Getting tweets you retweeted"></code>
        ''' </remarks>
        Public Function RetweetedByMe(ByVal Parameters As TwitterParameters) As List(Of TwitterStatus)
            Dim Url As String = STATUSES_RETWEETED_BY_ME
            If Parameters IsNot Nothing Then
                Url = Parameters.BuildUrl(Url)
            End If

            Return ParseStatuses(PerformWebRequest(Url, "GET"))
        End Function

        ''' <summary>
        ''' Retrieves retweets posted by friends of the authenticating user.
        ''' </summary>
        ''' <returns>A <c>List(Of TwitterStatus)</c> containing the requested retweets.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Retweeted To Me" lang="vbnet" title="Getting tweets you retweeted"></code>
        ''' </remarks>
        Public Function RetweetedToMe() As List(Of TwitterStatus)
            Return RetweetedToMe(Nothing)
        End Function

        ''' <summary>
        ''' Retrieves retweets posted by friends of the authenticating user.
        ''' </summary>
        ''' <param name="Parameters">A <see>TwitterParameters</see> object defining how the request should be executed.</param>
        ''' <returns>A <c>List(Of TwitterStatus)</c> containing the requested retweets.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Retweeted To Me" lang="vbnet" title="Getting tweets you retweeted"></code>
        ''' </remarks>
        Public Function RetweetedToMe(ByVal Parameters As TwitterParameters) As List(Of TwitterStatus)
            Dim Url As String = STATUSES_RETWEETED_TO_ME
            If Parameters IsNot Nothing Then
                Url = Parameters.BuildUrl(Url)
            End If

            Return ParseStatuses(PerformWebRequest(Url, "GET"))
        End Function

        ''' <summary>
        ''' Retrieves tweets from the authenticating user retweeted by others.
        ''' </summary>
        ''' <returns>A <c>List(Of TwitterStatus)</c> containing the requested retweets.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Retweets Of Me" lang="vbnet" title="Getting retweets of your tweets"></code>
        ''' </remarks>
        Public Function RetweetsOfMe() As List(Of TwitterStatus)
            Return RetweetsOfMe(Nothing)
        End Function

        ''' <summary>
        ''' Retrieves tweets from the authenticating user retweeted by others.
        ''' </summary>
        ''' <param name="Parameters">A <see>TwitterParameters</see> object defining how the request should be executed.</param>
        ''' <returns>A <c>List(Of TwitterStatus)</c> containing the requested retweets.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Retweets Of Me" lang="vbnet" title="Getting retweets of your tweets"></code>
        ''' </remarks>
        Public Function RetweetsOfMe(ByVal Parameters As TwitterParameters) As List(Of TwitterStatus)
            Dim Url As String = STATUSES_RETWEETS_OF_ME
            If Parameters IsNot Nothing Then
                Url = Parameters.BuildUrl(Url)
            End If

            Return ParseStatuses(PerformWebRequest(Url, "GET"))
        End Function
#End Region

#Region "Status Methods"
        ''' <summary>
        ''' Retrieves a specific tweet.
        ''' </summary>
        ''' <param name="ID">The ID of the tweet to be retrieved.</param>
        ''' <returns>A <c>TwitterStatus</c> object representing the requested tweet.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Show a Tweet" lang="vbnet" title="Retrieving a tweet"></code>
        ''' </remarks>
        Public Function ShowUpdate(ByVal ID As Int64) As TwitterStatus
            Return ParseStatuses(PerformWebRequest(String.Format(STATUSES_SHOW, ID), "GET"))(0)
        End Function

        ''' <summary>
        ''' Posts a tweet.
        ''' </summary>
        ''' <param name="Text">The text of the tweet to be posted.</param>
        ''' <returns>A <c>TwitterStatus</c> object representing the posted tweet.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Post a Tweet" lang="vbnet" title="Posting a tweet"></code>
        ''' </remarks>
        Public Function Update(ByVal Text As String) As TwitterStatus
            Return ParseStatuses(PerformWebRequest(STATUSES_UPDATE & "?status=" & TwitterOAuth.OAuthUrlEncode(Text), "POST"))(0)
        End Function

        ''' <summary>
        ''' Posts a tweet as a reply to another tweet.
        ''' </summary>
        ''' <param name="Text">The text of the tweet to be posted.</param>
        ''' <param name="ID">The ID of the tweet being replied to.</param>
        ''' <returns>A <c>TwitterStatus</c> object representing the posted tweet.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Post a Reply" lang="vbnet" title="Posting a reply"></code>
        ''' </remarks>
        Public Function ReplyToUpdate(ByVal Text As String, ByVal ID As Int64) As TwitterStatus
            Dim Url As String = String.Format(STATUSES_UPDATE & "?status={0}&in_reply_to_status_id={1}", TwitterOAuth.OAuthUrlEncode(Text), ID.ToString)
            Return ParseStatuses(PerformWebRequest(Url, "POST"))(0)
        End Function

        ''' <summary>
        ''' Deletes a tweet posted by the authenticating user.
        ''' </summary>
        ''' <param name="ID">The ID of the tweet to be deleted.</param>
        ''' <returns>A <c>TwitterStatus</c> object representing the deleted tweet.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Delete a Tweet" lang="vbnet" title="Deleting a tweet"></code>
        ''' </remarks>
        Public Function DeleteUpdate(ByVal ID As Int64) As TwitterStatus
            Return ParseStatuses(PerformWebRequest(String.Format(STATUSES_DESTROY, ID), "POST"))(0)
        End Function

        ''' <summary>
        ''' Retweets a tweet.
        ''' </summary>
        ''' <param name="ID">Id ID of the tweet being retweeted.</param>
        ''' <returns>A <c>TwitterStatus</c>representing the retweet.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Retweeting a Tweet" lang="vbnet" title="Retweeting a tweet"></code>
        ''' </remarks>
        Public Function Retweet(ByVal ID As Int64) As TwitterStatus
            Dim tp As New TwitterParameters
            tp.Add(TwitterParameterNames.ID, ID)
            Dim Url As String = tp.BuildUrl(String.Format(STATUSES_RETWEET, ID.ToString))
            Return ParseStatuses(PerformWebRequest(Url, "POST"))(0)
        End Function

        ''' <summary>
        ''' Retrieves up to the first 100 retweets of the specified tweet.
        ''' </summary>
        ''' <param name="ID">The tweet whose retweets will be retrieved.</param>
        ''' <param name="Count">How many retweets to reteieve, up to 100.</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Retrieving Retweets" lang="vbnet" title="Rerieveing retweets"></code>
        ''' </remarks>
        Public Function Retweets(ByVal ID As Int64, ByVal Count As Int64) As List(Of TwitterStatus)
            Dim tp As New TwitterParameters
            tp.Add(TwitterParameterNames.ID, ID)
            tp.Add(TwitterParameterNames.Count, Count)
            Dim Url As String = tp.BuildUrl(STATUSES_RETWEETS)
            Return ParseStatuses(PerformWebRequest(Url, "GET"))
        End Function

        ''' <summary>
        ''' Retireves of list of tweets that are replies to tweets posted by the selected user.
        ''' </summary>
        ''' <returns>A <c>List(Of TwitterStatus)</c> containing the requested tweets.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Get Replies" lang="vbnet" title="Getting replies"></code>
        ''' </remarks>
        Public Function Replies() As List(Of TwitterStatus)
            Return Replies(Nothing)
        End Function

        ''' <summary>
        ''' Retireves of list of tweets that are replies to tweets posted by the selected user.
        ''' </summary>
        ''' <param name="Parameters">A <see>TwitterParameters</see> object defining how the request should be executed.</param>
        ''' <returns>A <c>List(Of TwitterStatus)</c> containing the requested tweets.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Get Replies" lang="vbnet" title="Getting replies"></code>
        ''' </remarks>
        Public Function Replies(ByVal Parameters As TwitterParameters) As List(Of TwitterStatus)
            Dim Url As String = STATUSES_REPLIES
            If Parameters IsNot Nothing Then
                Url = Parameters.BuildUrl(Url)
            End If
            Return ParseStatuses(PerformWebRequest(Url, "GET"))
        End Function
#End Region

#Region "User Methods"
        ''' <summary>
        ''' Retrieves data about a specific user.
        ''' </summary>
        ''' <param name="ID">The ID of the user whose information is being requested.</param>
        ''' <returns>A <c>TwitterUser</c> object representing the requested user.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Get User Information" lang="vbnet" title="Retrieving user information"></code>       
        ''' </remarks>
        Public Function ShowUser(ByVal ID As Int64) As TwitterUser
            Dim tp As New TwitterParameters
            tp.Add(TwitterParameterNames.ID, ID)
            Dim Url As String = tp.BuildUrl(USERS_SHOW)
            Return ParseUsers(PerformWebRequest(Url, "GET"))(0)
        End Function

        ''' <summary>
        ''' Retrieves data about a specific user.
        ''' </summary>
        ''' <param name="ScreenName">The screen name of the user whose information is being requested.</param>
        ''' <returns>A <c>TwitterUser</c> object representing the requested user.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Get User Information" lang="vbnet" title="Retrieving user information"></code>       
        ''' </remarks>
        Public Function ShowUser(ByVal ScreenName As String) As TwitterUser
            Dim tp As New TwitterParameters
            tp.Add(TwitterParameterNames.ScreenName, ScreenName)
            Dim Url As String = tp.BuildUrl(USERS_SHOW)
            Return ParseUsers(PerformWebRequest(Url, "GET"))(0)
        End Function

        ''' <summary>
        ''' Retrieves a list of all the users that are followed by the authenticated user.
        ''' </summary>
        ''' <returns>A <c>List(Of TwitterUser)</c> representing all the users that are followed by the authenticating user.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Get Friends" lang="vbnet" title="Retrieving friends"></code>
        ''' Users who follow many users may experience long wait times because of the way Twitter handles large requests.
        ''' When you call this method, you should let your users know that there may be a delay or use it in a separate thread.
        ''' </remarks>
        Public Function Friends() As List(Of TwitterUser)
            Return Friends(Nothing)
        End Function

        ''' <summary>
        ''' Retrieves a list of all the users that are followed by the specified user.
        ''' </summary>
        ''' <param name="Parameters">A <see>TwitterParameters</see> object defining how the request should be executed.</param>
        ''' <returns>A <c>List(Of TwitterUser)</c> representing all the users that are followed by the specified user.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Get Friends" lang="vbnet" title="Retrieving friends"></code>
        ''' Users who follow many users may experience long wait times because of the way Twitter handles large requests.
        ''' When you call this method, you should let your users know that there may be a delay or use it in a separate thread.
        ''' </remarks>
        Public Function Friends(ByVal Parameters As TwitterParameters) As List(Of TwitterUser)
            Dim ReturnValue As New List(Of TwitterUser)
            Dim NextCursor As Int64 = -1

            Do
                Dim Url As String = STATUSES_FRIENDS

                Dim LoopParameters As TwitterParameters
                If Parameters Is Nothing Then
                    LoopParameters = New TwitterParameters
                Else
                    LoopParameters = Parameters
                End If

                LoopParameters.Add(TwitterParameterNames.Cursor, NextCursor)
                Url = LoopParameters.BuildUrl(Url)

                Dim Batch As New List(Of TwitterUser)
                Batch = ParseUsers(PerformWebRequest(Url, "GET"), NextCursor)
                ReturnValue.AddRange(Batch)
            Loop Until NextCursor = 0

            Return ReturnValue
        End Function

        ''' <summary>
        ''' Retrieves a list of all the users that follow the authenticated user.
        ''' </summary>
        ''' <returns>A <c>List(Of TwitterUser)</c> representing all the users that follow the authenticating user.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Get Followers" lang="vbnet" title="Retrieving followers"></code>
        ''' Users who have many followers may experience long wait times because of the way Twitter handles large requests.
        ''' When you call this method, you should let your users know that there may be a delay or use it in a separate thread.
        ''' </remarks>
        Public Function Followers() As List(Of TwitterUser)
            Return Followers(Nothing)
        End Function

        ''' <summary>
        ''' Retrieves a list of all the users that follow the specified user.
        ''' </summary>
        ''' <param name="Parameters">A <see>TwitterParameters</see> object defining how the request should be executed.</param>
        ''' <returns>A <c>List(Of TwitterUser)</c> representing all the users that follow the specified user.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Get Followers" lang="vbnet" title="Retrieving followers"></code>
        ''' Users who have many followers may experience long wait times because of the way Twitter handles large requests.
        ''' When you call this method, you should let your users know that there may be a delay or use it in a separate thread.
        ''' </remarks>
        Public Function Followers(ByVal Parameters As TwitterParameters) As List(Of TwitterUser)
            Dim Results As New PagedResults(Of TwitterUser)

            Do While Results.HasMore
                Followers(Parameters, Results)
            Loop

            Return Results
        End Function

        ''' <summary>
        ''' Retrieves a list of all the users that follow the specified user.
        ''' </summary>
        ''' <param name="Parameters">A <see>TwitterParameters</see> object defining how the request should be executed.</param>
        ''' <param name="Results">The paged results to add the users to</param>
        ''' <remarks>
        ''' Users who have many followers may experience long wait times because of the way Twitter handles large requests.
        ''' When you call this method, you should let your users know that there may be a delay or use it in a separate thread.
        ''' </remarks>
        Public Sub Followers(ByVal Parameters As TwitterParameters, ByVal Results As PagedResults(Of TwitterUser))
            Dim Url As String = STATUSES_FOLLOWERS

            Dim LoopParameters As TwitterParameters
            If Parameters Is Nothing Then
                LoopParameters = New TwitterParameters
            Else
                LoopParameters = Parameters
            End If

            LoopParameters.Add(TwitterParameterNames.Cursor, Results.Cursor)
            Url = LoopParameters.BuildUrl(Url)

            Results.AddRange(ParseUsers(PerformWebRequest(Url, "GET"), Results.Cursor))
        End Sub

        ''' <summary>
        ''' Performs a people search and returns a list of matching users.
        ''' </summary>
        ''' <param name="SearchQuery">A <c>String</c> representing the search term or terms</param>
        ''' <returns>A <c>List(Of TwitterUser) containing the search results.</c></returns>
        ''' <remarks></remarks>
        Public Function UserSearch(ByVal SearchQuery As String) As List(Of TwitterUser)
            Return UserSearch(SearchQuery, 1)
        End Function

        ''' <summary>
        ''' Performs a people search and returns a list of matching users.
        ''' </summary>
        ''' <param name="SearchQuery">A <c>String</c> representing the search term or terms</param>
        ''' <param name="PageNumber">An <c>Integer</c> indicating which page of results should be returned.</param>
        ''' <returns>A <c>List(Of TwitterUser) containing the search results.</c></returns>
        ''' <remarks></remarks>
        Public Function UserSearch(ByVal SearchQuery As String, ByVal PageNumber As Integer) As List(Of TwitterUser)
            Dim Parameters As New TwitterParameters
            Dim Url As String
            Parameters.Add(TwitterParameterNames.Page, PageNumber)
            Parameters.Add(TwitterParameterNames.SearchQuery, SearchQuery)
            Url = Parameters.BuildUrl(USERS_SEARCH)
            Return ParseUsers(PerformWebRequest(Url, "GET"))
        End Function
#End Region

#Region "List Methods"
        ''' <summary>
        ''' Creates a new list for the authenticated user.
        ''' </summary>
        ''' <param name="Username">The username of the authenticated user.</param>
        ''' <param name="ListName">The name of the list.</param>
        ''' <param name="Mode">Whether the list is public or private.</param>
        ''' <returns>A <c>TwitterList</c> object representing the new list.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Create List" lang="vbnet" title="Creating a list"></code>
        ''' </remarks>
        Public Function ListCreate(ByVal Username As String, ByVal ListName As String, ByVal Mode As Globals.ListMode) As TwitterList
            Return ListCreate(Username, ListName, Mode, String.Empty)
        End Function

        ''' <summary>
        ''' Creates a new list for the authenticated user.
        ''' </summary>
        ''' <param name="Username">The username of the authenticated user.</param>
        ''' <param name="ListName">The name of the list.</param>
        ''' <param name="Mode">Whether the list is public or private.</param>
        ''' <param name="Description">A description of the list.</param>
        ''' <returns>A <c>TwitterList</c> object representing the new list.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Create List" lang="vbnet" title="Creating a list"></code>
        ''' </remarks>
        Public Function ListCreate(ByVal Username As String, ByVal ListName As String, ByVal Mode As Globals.ListMode, ByVal Description As String) As TwitterList
            Dim Url As String = String.Empty
            If Description = String.Empty Then
                Url = String.Format(LISTS & "?name={1}&mode={2}", Username, ListName, Mode.ToString)
            Else
                Url = String.Format(LISTS & "?name={1}&mode={2}&description={3}", Username, ListName, Mode.ToString, Description)
            End If

            Return ParseLists(PerformWebRequest(Url, "POST"))(0)
        End Function

        ''' <summary>
        ''' Updates the specified list.
        ''' </summary>
        ''' <param name="Username">The username of the authenticated user.</param>
        ''' <param name="ListID">The ID or slug of the list.</param>
        ''' <param name="ListName">The name of the list.</param>
        ''' <param name="Mode">Whether the list is public or private.</param>
        ''' <param name="Description">A description of the list.</param>
        ''' <returns>A <c>TwitterList</c> object representing the new list.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Update List" lang="vbnet" title="Updating a list"></code>
        ''' </remarks>
        Public Function ListUpdate(ByVal Username As String, ByVal ListID As String, ByVal ListName As String, ByVal Mode As Globals.ListMode, ByVal Description As String) As TwitterList
            Dim Url As String = String.Format(LISTS_SPECIFY & "?name={2}&mode={3}&description={4}", Username, ListID, ListName, Mode.ToString, Description)
            Return ParseLists(PerformWebRequest(Url, "POST"))(0)
        End Function

        ''' <summary>
        ''' List the lists of the specified user. 
        ''' </summary>
        ''' <param name="Username">The username whose lists will be returned.</param>
        ''' <returns>A <c>List(Of TwitterList)</c> representing the user's lists.  Private lists will be included if the authenticated users is the same as the user who'se lists are being returned.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Get Lists" lang="vbnet" title="Getting lists"></code>
        ''' </remarks>
        Public Function ListsGet(ByVal Username As String) As List(Of TwitterList)
            Dim ReturnLists As New List(Of TwitterList)
            Dim NextCursor As Int64 = -1

            Do
                Dim Url As String = String.Format(LISTS, Username)
                Dim LoopParameters As New TwitterParameters
                LoopParameters.Add(TwitterParameterNames.Cursor, NextCursor)
                Url = LoopParameters.BuildUrl(Url)

                Dim Batch As New List(Of TwitterList)
                Batch = ParseLists(PerformWebRequest(Url, "GET"), NextCursor)
                ReturnLists.AddRange(Batch)
            Loop Until NextCursor = 0

            Return ReturnLists
        End Function

        ''' <summary>
        ''' Show the specified list.
        ''' </summary>
        ''' <param name="Username">The user who owns the list.</param>
        ''' <param name="ListID">The ID or slug of the list.</param>
        ''' <returns>A <c>TwitterList</c> representing the list.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Get List" lang="vbnet" title="Getting a list"></code>
        ''' </remarks>
        Public Function ListGet(ByVal Username As String, ByVal ListID As String) As TwitterList
            Dim Url As String = String.Format(LISTS_SPECIFY, Username, ListID)
            Return ParseLists(PerformWebRequest(Url, "GET"))(0)
        End Function

        ''' <summary>
        ''' Deletes the specified list. Must be owned by the authenticated user.
        ''' </summary>
        ''' <param name="Username">The username of the list owner.</param>
        ''' <param name="ListID">The ID or slug of the list.</param>
        ''' <returns>A <c>TwitterList</c> representing the list.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Delete List" lang="vbnet" title="Deleting a list"></code>
        ''' </remarks>
        Public Function ListDelete(ByVal Username As String, ByVal ListID As String) As TwitterList
            Dim Url As String = String.Format(LISTS_SPECIFY, Username, ListID)
            Return ParseLists(PerformWebRequest(Url, "DELETE"))(0)
        End Function

        ''' <summary>
        ''' Show recent tweets from members of the specified list.
        ''' </summary>
        ''' <param name="Username">The username of the owner of list.</param>
        ''' <param name="ListID">The ID or slug of the list.</param>
        ''' <param name="Count">How many statuses to return.  Default is 20.  Maximum is 200.</param>
        ''' <returns>A <c>List(Of TwitterStatus)</c> representing the requestsed tweets.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Get List Statuses" lang="vbnet" title="Getting list statuses"></code>
        ''' </remarks>
        Public Function ListStatuses(ByVal Username As String, ByVal ListID As String, Optional ByVal Count As Int64 = 20) As List(Of TwitterStatus)
            Dim Url As String = String.Format(LISTS_STATUSES & "?per_page={2}", Username, ListID, Count.ToString)
            Return ParseStatuses(PerformWebRequest(Url, "GET"))
        End Function

        ''' <summary>
        ''' List the lists the specified user has been added to.
        ''' </summary>
        ''' <param name="Username">The user whose memberships will be returned.</param>
        ''' <returns>A <c>List(Of TwitterList)</c> representing the requested lists.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Get List Memberships" lang="vbnet" title="Getting list memberships"></code>
        ''' </remarks>
        Public Function ListMemberships(ByVal Username As String) As List(Of TwitterList)
            Dim Results As New PagedResults(Of TwitterList)
            Do
                ListMemberships(Username, Results)
            Loop Until Results.HasMore = False
            Return Results
        End Function

        ''' <summary>
        ''' List the lists the specified user has been added to.
        ''' </summary>
        ''' <param name="Username">The user whose memberships will be returned.</param>
        ''' <param name="Results">The paged results to add the lists to</param>
        Public Sub ListMemberships(ByVal Username As String, ByVal Results As PagedResults(Of TwitterList))
            Dim Url As String = String.Format(LISTS_MEMBERSHIPS, Username)
            Dim Parameters As New TwitterParameters
            Parameters.Add(TwitterParameterNames.Cursor, Results.Cursor)
            Url = Parameters.BuildUrl(Url)

            Dim Lists As List(Of TwitterList)
            Lists = ParseLists(PerformWebRequest(Url, "GET"), Results.Cursor)
            Results.AddRange(Lists)
        End Sub

        ''' <summary>
        ''' List the lists the specified user follows.
        ''' </summary>
        ''' <param name="Username">The user whose followed lists will be returned.</param>
        ''' <returns>A <c>List(Of TwitterList)</c> representing the requested lists.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Get List Subscriptions" lang="vbnet" title="Getting list subscriptions"></code>
        ''' </remarks>
        Public Function ListSubscriptions(ByVal Username As String) As List(Of TwitterList)
            Dim Url As String = String.Format(LISTS_SUBSCRIPTIONS, Username)
            Return ParseLists(PerformWebRequest(Url, "GET"))
        End Function
#End Region

#Region "List Members Methods"
        ''' <summary>
        ''' Returns the members of the specified list.
        ''' </summary>
        ''' <param name="Username">The user who owns the list.</param>
        ''' <param name="ListID">The ID or slug of the list.</param>
        ''' <returns>A <c>List(Of TwitterUser)</c> representing the members of the list.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Get List Subscriptions" lang="vbnet" title="Getting list subscriptions"></code>
        ''' </remarks>
        Public Function ListMembers(ByVal Username As String, ByVal ListID As String) As List(Of TwitterUser)
            Dim Users As New PagedResults(Of TwitterUser)

            Do
                ListMembers(Username, ListID, Users)
            Loop Until Users.HasMore = False

            Return Users
        End Function

        ''' <summary>
        ''' Returns the members of the specified list.
        ''' </summary>
        ''' <param name="Username">The user who owns the list.</param>
        ''' <param name="ListID">The ID or slug of the list.</param>
        ''' <param name="Results">The paged results to add the lists to</param>
        Public Sub ListMembers(ByVal Username As String, ByVal ListID As String, ByVal Results As PagedResults(Of TwitterUser))
            Dim Url As String = String.Format(LISTS_MEMBERS, Username, ListID)
            Dim LoopParameters As New TwitterParameters
            LoopParameters.Add(TwitterParameterNames.Cursor, Results.Cursor)
            Url = LoopParameters.BuildUrl(Url)

            Dim Batch As New List(Of TwitterUser)
            Batch = ParseUsers(PerformWebRequest(Url, "GET"), Results.Cursor)
            Results.AddRange(Batch)
        End Sub

        ''' <summary>
        ''' Add a member to a list.
        ''' </summary>
        ''' <param name="Username">The user who owns the list.</param>
        ''' <param name="ListID">The ID or slug of the list.</param>
        ''' <param name="UserID">The ID of the user to add to the list.</param>
        ''' <returns>A <c>TwitterList</c> representing the list to which the user was added.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Add Member To List" lang="vbnet" title="Adding a member to a list"></code>
        ''' </remarks>
        Public Function ListMembersAdd(ByVal Username As String, ByVal ListID As String, ByVal UserID As Int64) As TwitterList
            Dim Url As String = String.Format(LISTS_MEMBERS & "?id={2}", Username, ListID, UserID.ToString)
            Return ParseLists(PerformWebRequest(Url, "POST"))(0)
        End Function

        ''' <summary>
        ''' Remove a member from a list.
        ''' </summary>
        ''' <param name="Username">The user who owns the list.</param>
        ''' <param name="ListID">The ID or slug of the list.</param>
        ''' <param name="UserID">The ID of the user to remove from the list.</param>
        ''' <returns>A <c>TwitterList</c> representing the list from which the user was removed.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Remove Member From List" lang="vbnet" title="Removing a member from a list"></code>
        ''' </remarks>
        Public Function ListMembersDelete(ByVal Username As String, ByVal ListID As String, ByVal UserID As Int64) As TwitterList
            Dim Url As String = String.Format(LISTS_MEMBERS & "?id={2}", Username, ListID, UserID.ToString)
            Return ParseLists(PerformWebRequest(Url, "DELETE"))(0)
        End Function

        ''' <summary>
        ''' Check if a user is a member of the specified list.
        ''' </summary>
        ''' <param name="Username">The user who owns the list.</param>
        ''' <param name="ListID">The ID or slug of the list.</param>
        ''' <param name="UserID">The ID of the user to check.</param>
        ''' <returns>A <c>Boolean</c> indicating whether or not the user is a member of the list.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Check User Membership" lang="vbnet" title="Checking a list for a particular user"></code>
        ''' </remarks>
        Public Function ListMembersCheck(ByVal Username As String, ByVal ListID As String, ByVal UserID As Int64) As Boolean
            Dim Url As String = String.Format(LISTS_MEMBERS_ID, Username, ListID, UserID.ToString)
            Return (ParseUsers(PerformWebRequest(Url, "GET")).Count > 0)
        End Function
#End Region

#Region "List Subscribers Methods"
        ''' <summary>
        ''' Returns the subscribers of the specified list.
        ''' </summary>
        ''' <param name="Username">The user who owns the list.</param>
        ''' <param name="ListID">The ID or slug of the list.</param>
        ''' <returns>A <c>List(Of TwitterUser)</c> representing the subscribers of the list.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Get List Subscribers" lang="vbnet" title="Getting list subscribers"></code>
        ''' </remarks>
        Public Function ListSubscribers(ByVal Username As String, ByVal ListID As String) As List(Of TwitterUser)
            Dim Users As New List(Of TwitterUser)
            Dim NextCursor As Int64 = -1

            Do
                Dim Url As String = String.Format(LISTS_SUBSCRIBERS, Username, ListID)
                Dim LoopParameters As New TwitterParameters
                LoopParameters.Add(TwitterParameterNames.Cursor, NextCursor)
                Url = LoopParameters.BuildUrl(Url)

                Dim Batch As New List(Of TwitterUser)
                Batch = ParseUsers(PerformWebRequest(Url, "GET"), NextCursor)
                Users.AddRange(Batch)
            Loop Until NextCursor = 0

            Return Users
        End Function

        ''' <summary>
        ''' Subscribes the authenticated user to the specified list.
        ''' </summary>
        ''' <param name="Username">The user who owns the list.</param>
        ''' <param name="ListID">The ID or slug of the list.</param>
        ''' <returns>A <c>TwitterList</c> representing the list to which the user was added.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Subscribe To List" lang="vbnet" title="Subscribing to a list"></code>
        ''' </remarks>
        Public Function ListSubscribe(ByVal Username As String, ByVal ListID As String) As TwitterList
            Dim Url As String = String.Format(LISTS_SUBSCRIBERS, Username, ListID)
            Return ParseLists(PerformWebRequest(Url, "POST"))(0)
        End Function

        ''' <summary>
        ''' Subscribes the authenticated user to the specified list.
        ''' </summary>
        ''' <param name="Username">The user who owns the list.</param>
        ''' <param name="ListID">The ID or slug of the list.</param>
        ''' <returns>A <c>TwitterList</c> representing the list to which the user was added.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Unsubscribe From List" lang="vbnet" title="Unsubscribing from a list"></code>
        ''' </remarks>
        Public Function ListUnsubscribe(ByVal Username As String, ByVal ListID As String) As TwitterList
            Dim Url As String = String.Format(LISTS_SUBSCRIBERS, Username, ListID)
            Return ParseLists(PerformWebRequest(Url, "DELETE"))(0)
        End Function

        ''' <summary>
        ''' Check if a user is subscribed to the specified list.
        ''' </summary>
        ''' <param name="Username">The user who owns the list.</param>
        ''' <param name="ListID">The ID or slug of the list.</param>
        ''' <param name="UserID">The ID of the user to check.</param>
        ''' <returns>A <c>Boolean</c> indicating whether or not the user is a subscriber of the list.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Check User Subscription" lang="vbnet" title="Checking a list for a particular subscriber"></code>
        ''' </remarks>
        Public Function ListSubscribersCheck(ByVal Username As String, ByVal ListID As String, ByVal UserID As Int64) As Boolean
            Dim Url As String = String.Format(LISTS_SUBSCRIBERS_ID, Username, ListID, UserID.ToString)
            Return (ParseUsers(PerformWebRequest(Url, "GET")).Count > 0)
        End Function

#End Region

#Region "Direct Message Methods"
        ''' <summary>
        ''' Retrieves the last 20 direct messages received by the authenticating user.
        ''' </summary>
        ''' <returns>A <c>List(Of <see>TwitterDirectMessage</see>)</c> representing the direct messages.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Get Direct Messages" lang="vbnet" title="Retrieving direct messages"></code>
        ''' </remarks>
        Public Function DirectMessages() As List(Of TwitterDirectMessage)
            Return DirectMessages(Nothing)
        End Function

        ''' <summary>
        ''' Retrieves the direct messages received by the authenticating user.
        ''' </summary>
        ''' <param name="Parameters">A <see>TwitterParameters</see> object detailing how the request will be executed.</param>
        ''' <returns>A <c>List(Of <see>TwitterDirectMessage</see>)</c> representing the direct messages.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Get Direct Messages" lang="vbnet" title="Retrieving direct messages"></code>
        ''' </remarks>
        Public Function DirectMessages(ByVal Parameters As TwitterParameters) As List(Of TwitterDirectMessage)
            Dim Url As String = DIRECT_MESSAGES
            If Parameters IsNot Nothing Then
                Url = Parameters.BuildUrl(Url)
            End If
            Return ParseDirectMessages(PerformWebRequest(Url, "GET"))
        End Function

        ''' <summary>
        ''' Retrieves the last 20 direct messages sent by the authenticating user.
        ''' </summary>
        ''' <returns>A <c>List(Of <see>TwitterDirectMessage</see>)</c> representing the direct messages.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Get Sent Direct Messages" lang="vbnet" title="Retrieving sent direct messages"></code>
        ''' </remarks>
        Public Function DirectMessagesSent() As List(Of TwitterDirectMessage)
            Return DirectMessagesSent(Nothing)
        End Function

        ''' <summary>
        ''' Retrieves the direct messages sent by the authenticating user.
        ''' </summary>
        ''' <param name="Parameters">A <see>TwitterParameters</see> object detailing how the request will be executed.</param>
        ''' <returns>A <c>List(Of <see>TwitterDirectMessage</see>)</c> representing the direct messages.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Get Sent Direct Messages" lang="vbnet" title="Retrieving sent direct messages"></code>
        ''' </remarks>
        Public Function DirectMessagesSent(ByVal Parameters As TwitterParameters) As List(Of TwitterDirectMessage)
            Dim Url As String = DIRECT_MESSAGES_SENT
            If Parameters IsNot Nothing Then
                Url = Parameters.BuildUrl(Url)
            End If
            Return ParseDirectMessages(PerformWebRequest(Url, "GET"))
        End Function

        ''' <summary>
        ''' Creates a new direct message
        ''' </summary>
        ''' <param name="User">The ID or screen name of the user who is the recipient.</param>
        ''' <param name="Text">The text of the message.</param>
        ''' <returns>A <c>TwitterDirectMessage</c> representing the sent message.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Send Direct Message" lang="vbnet" title="Sending a direct message"></code>
        ''' </remarks>
        Public Function SendDirectMessage(ByVal User As String, ByVal Text As String) As TwitterDirectMessage
            Dim Url As String = String.Format(DIRECT_MESSAGES_NEW & "?user={0}&text={1}", User, TwitterOAuth.OAuthUrlEncode(Text))
            Dim Message As TwitterDirectMessage = ParseDirectMessages(PerformWebRequest(Url, "POST"))(0)
            Return Message
        End Function

        ''' <summary>
        ''' Deletes a direct message sent by the authenticating user.
        ''' </summary>
        ''' <param name="ID">The ID of the message to be deleted.</param>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Delete Direct Message" lang="vbnet" title="Deleting a direct message"></code>
        ''' </remarks>
        Public Sub DeleteDirectMessage(ByVal ID As Int64)
            PerformWebRequest(String.Format(DIRECT_MESSAGES_DESTROY, ID), "POST")
        End Sub
#End Region

#Region "Friendship Methods"
        ''' <summary>
        ''' Follows the specified user.
        ''' </summary>
        ''' <param name="ID">The ID of the user to follow</param>
        ''' <returns>A <c>TwitterUser</c> object representing the followed user.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Follow User" lang="vbnet" title="Following a user"></code>
        ''' </remarks>
        Public Function Follow(ByVal ID As Int64) As TwitterUser
            Return ParseUsers(PerformWebRequest(String.Format(FRIENDSHIP_CREATE, ID), "POST"))(0)
        End Function

        ''' <summary>
        ''' Follows the specified user.
        ''' </summary>
        ''' <param name="ScreenName">The screen name of the user to follow</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Follow User" lang="vbnet" title="Following a user"></code>
        ''' </remarks>
        Public Function Follow(ByVal ScreenName As String) As TwitterUser
            Return ParseUsers(PerformWebRequest(String.Format(FRIENDSHIP_CREATE, ScreenName), "POST"))(0)
        End Function

        ''' <summary>
        ''' Unfollows the specified user.
        ''' </summary>
        ''' <param name="ID">The ID of the user to stop following</param>
        ''' <returns>A <c>TwitterUser</c> object representing the unfollowed user.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Unfollow User" lang="vbnet" title="Unfollowing a user"></code>
        ''' </remarks>
        Public Function UnFollow(ByVal ID As Int64) As TwitterUser
            Return ParseUsers(PerformWebRequest(String.Format(FRIENDSHIP_DESTROY, ID), "POST"))(0)
        End Function

        ''' <summary>
        ''' Unfollows the specified user.
        ''' </summary>
        ''' <param name="ScreenName">The screen name of the user to stop following</param>
        ''' <returns>A <c>TwitterUser</c> object representing the unfollowed user.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Unfollow User" lang="vbnet" title="Unfollowing a user"></code>
        ''' </remarks>
        Public Function UnFollow(ByVal ScreenName As String) As TwitterUser
            Return ParseUsers(PerformWebRequest(String.Format(FRIENDSHIP_DESTROY, ScreenName), "POST"))(0)
        End Function

        ''' <summary>
        ''' Gets the relationship between two users.
        ''' </summary>
        ''' <param name="FollowerScreenName">The screen name of the user doing the following (the 'source' user)</param>
        ''' <param name="FolloweeScreenName">The screen name of the user being followed (the 'target' user)</param>
        ''' <returns>A <c>TwitterRelationship</c> object representing the friendship relationship between the two users</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Get Relationship" lang="vbnet" title="Determining the reltionship between two users"></code>
        ''' </remarks>
        Public Function Relationship(ByVal FollowerScreenName As String, ByVal FolloweeScreenName As String) As TwitterRelationship
            Return ParseRelationships(PerformWebRequest(String.Format(FRIENDSHIP_SHOW, New String() {"source_screen_name", FollowerScreenName, "target_screen_name", FolloweeScreenName}), "GET"))(0)
        End Function

        ''' <summary>
        ''' Gets the relationship between two users.
        ''' </summary>
        ''' <param name="FollowerID">The ID of the user doing the following (the 'source' user)</param>
        ''' <param name="FolloweeID">The ID of the user being followed (the 'target' user)</param>
        ''' <returns>A <c>TwitterRelationship</c> object representing the friendship relationship between the two users</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Get Relationship" lang="vbnet" title="Determining the reltionship between two users"></code>
        ''' </remarks>
        Public Function Relationship(ByVal FollowerID As Int64, ByVal FolloweeID As Int64) As TwitterRelationship
            Return ParseRelationships(PerformWebRequest(String.Format(FRIENDSHIP_SHOW, New String() {"source_id", FollowerID.ToString, "target_id", FolloweeID.ToString}), "GET"))(0)
        End Function
#End Region

#Region "Social Graph Methods"
        ''' <summary>
        ''' Returns a list of numeric IDs of users the authenticated user is following.
        ''' </summary>
        ''' <returns>A <c>List(Of Int64)</c> containing the IDs of the requested users.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Social Graph Friends" lang="vbnet" title="Retrieving friends IDs"></code>
        ''' Users who follow many users may experience long wait times because of the way Twitter handles large requests.
        ''' When you call this method, you should let your users know that there may be a delay or use it in a separate thread.
        ''' </remarks>
        Public Function FriendsIDs() As List(Of Int64)
            Dim ReturnValue As New List(Of Int64)
            Dim NextCursor As Int64 = -1

            Do
                Dim Url As String = FRIENDS_URL
                Dim tp As New TwitterParameters
                tp.Add(TwitterParameterNames.Cursor, NextCursor)
                Url = tp.BuildUrl(Url)

                Dim Batch As New List(Of Int64)
                Batch = ParseSocialGraph(PerformWebRequest(Url, "GET"), NextCursor)
                ReturnValue.AddRange(Batch)
            Loop Until NextCursor = 0

            Return ReturnValue
        End Function

        ''' <summary>
        ''' Returns a list of numeric IDs of users the specified user is following.
        ''' </summary>
        ''' <param name="ID">The ID of the user whose friends are being requested.</param>
        ''' <returns>A <c>List(Of Int64)</c> containing the IDs of the requested users.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Social Graph Friends" lang="vbnet" title="Retrieving friends IDs"></code>
        ''' Users who follow many users may experience long wait times because of the way Twitter handles large requests.
        ''' When you call this method, you should let your users know that there may be a delay or use it in a separate thread.
        ''' </remarks>
        Public Function FriendsIDs(ByVal ID As Int64) As List(Of Int64)
            Dim ReturnValue As New List(Of Int64)
            Dim NextCursor As Int64 = -1

            Do
                Dim Url As String = FRIENDS_URL
                Dim tp As New TwitterParameters
                tp.Add(TwitterParameterNames.Cursor, NextCursor)
                tp.Add(TwitterParameterNames.ID, ID)
                Url = tp.BuildUrl(Url)

                Dim Batch As New List(Of Int64)
                Batch = ParseSocialGraph(PerformWebRequest(Url, "GET"), NextCursor)
                ReturnValue.AddRange(Batch)
            Loop Until NextCursor = 0

            Return ReturnValue
        End Function

        ''' <summary>
        ''' Returns a list of numeric IDs of users the specified user is following.
        ''' </summary>
        ''' <param name="ScreenName">The screen name of the user whose friends are being requested.</param>
        ''' <returns>A <c>List(Of Int64)</c> containing the IDs of the requested users.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Social Graph Friends" lang="vbnet" title="Retrieving friends IDs"></code>
        ''' Users who follow many users may experience long wait times because of the way Twitter handles large requests.
        ''' When you call this method, you should let your users know that there may be a delay or use it in a separate thread.
        ''' </remarks>
        Public Function FriendsIDs(ByVal ScreenName As String) As List(Of Int64)
            Dim ReturnValue As New List(Of Int64)
            Dim NextCursor As Int64 = -1

            Do
                Dim Url As String = FRIENDS_URL
                Dim tp As New TwitterParameters
                tp.Add(TwitterParameterNames.Cursor, NextCursor)
                tp.Add(TwitterParameterNames.ScreenName, ScreenName)
                Url = tp.BuildUrl(Url)

                Dim Batch As New List(Of Int64)
                Batch = ParseSocialGraph(PerformWebRequest(Url, "GET"), NextCursor)
                ReturnValue.AddRange(Batch)
            Loop Until NextCursor = 0

            Return ReturnValue
        End Function

        ''' <summary>
        ''' Returns a list of numeric IDs of users the authenticated user is followed by.
        ''' </summary>
        ''' <returns>A <c>List(Of Int64)</c> containing the IDs of the requested users.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Social Graph Followers" lang="vbnet" title="Retrieving follower IDs"></code>
        ''' Users with many followers may experience long wait times because of the way Twitter handles large requests.
        ''' When you call this method, you should let your users know that there may be a delay or use it in a separate thread.
        ''' </remarks>
        Public Function FollowersIDs() As List(Of Int64)
            Dim ReturnValue As New List(Of Int64)
            Dim NextCursor As Int64 = -1

            Do
                Dim Url As String = FOLLOWERS_URL
                Dim tp As New TwitterParameters
                tp.Add(TwitterParameterNames.Cursor, NextCursor)
                Url = tp.BuildUrl(Url)

                Dim Batch As New List(Of Int64)
                Batch = ParseSocialGraph(PerformWebRequest(Url, "GET"), NextCursor)
                ReturnValue.AddRange(Batch)
            Loop Until NextCursor = 0

            Return ReturnValue
        End Function

        ''' <summary>
        ''' Returns a list of numeric IDs of users the specified user is followed by.
        ''' </summary>
        ''' <param name="ID">The ID of the user whose followers are being requested.</param>
        ''' <returns>A <c>List(Of Int64)</c> containing the IDs of the requested users.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Social Graph Followers" lang="vbnet" title="Retrieving followers IDs"></code>
        ''' Users who have many followers may experience long wait times because of the way Twitter handles large requests.
        ''' When you call this method, you should let your users know that there may be a delay or use it in a separate thread.
        ''' </remarks>
        Public Function FollowersIDs(ByVal ID As Int64) As List(Of Int64)
            Dim ReturnValue As New List(Of Int64)
            Dim NextCursor As Int64 = -1

            Do
                Dim Url As String = FOLLOWERS_URL
                Dim tp As New TwitterParameters
                tp.Add(TwitterParameterNames.Cursor, NextCursor)
                tp.Add(TwitterParameterNames.ID, ID)
                Url = tp.BuildUrl(Url)

                Dim Batch As New List(Of Int64)
                Batch = ParseSocialGraph(PerformWebRequest(Url, "GET"), NextCursor)
                ReturnValue.AddRange(Batch)
            Loop Until NextCursor = 0

            Return ReturnValue
        End Function

        ''' <summary>
        ''' Returns a list of numeric IDs of users the specified user is followed by.
        ''' </summary>
        ''' <param name="ScreenName">The screen name of the user whose followers are being requested.</param>
        ''' <returns>A <c>List(Of Int64)</c> containing the IDs of the requested users.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Social Graph Followers" lang="vbnet" title="Retrieving followers IDs"></code>
        ''' Users who have many followers may experience long wait times because of the way Twitter handles large requests.
        ''' When you call this method, you should let your users know that there may be a delay or use it in a separate thread.
        ''' </remarks>
        Public Function FollowersIDs(ByVal ScreenName As String) As List(Of Int64)
            Dim ReturnValue As New List(Of Int64)
            Dim NextCursor As Int64 = -1

            Do
                Dim Url As String = FOLLOWERS_URL
                Dim tp As New TwitterParameters
                tp.Add(TwitterParameterNames.Cursor, NextCursor)
                tp.Add(TwitterParameterNames.ScreenName, ScreenName)
                Url = tp.BuildUrl(Url)

                Dim Batch As New List(Of Int64)
                Batch = ParseSocialGraph(PerformWebRequest(Url, "GET"), NextCursor)
                ReturnValue.AddRange(Batch)
            Loop Until NextCursor = 0

            Return ReturnValue
        End Function
#End Region

#Region "Account Methods"
        ''' <summary>
        ''' Returns account information for the authenticated user.
        ''' </summary>
        ''' <returns>A <see>TwitterUser</see> representing the authenticated user.</returns>
        ''' <remarks>
        ''' This method works by calling Twitter's <a href="http://apiwiki.twitter.com/Twitter-REST-API-Method:-account%C2%A0verify_credentials">account/verify_credentials</a> method.
        ''' The rate limits on this method are stricter than for other parts of the Twitter API.  Only call this when necessary and cache the results when you do.
        ''' <code source="TwitterVB2\examples.vb" region="Get Account Information" lang="vbnet" title="Retrieving account information"></code>
        ''' </remarks>
        Public Function AccountInformation() As TwitterUser
            Return ParseUsers(PerformWebRequest(ACCOUNT_VERIFY_CREDENTIALS, "GET"))(0)
        End Function
#End Region

#Region "Favorite Methods"
        ''' <summary>
        ''' Retrieves the last 20 favorites marked by the authenticating user.
        ''' </summary>
        ''' <returns>A <c>List(Of <see>TwitterStatus</see>)</c> representing the favorites.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Get Favorites" lang="vbnet" title="Retrieving favorites"></code>
        ''' </remarks>
        Public Function Favorites() As List(Of TwitterStatus)
            Return Favorites(Nothing)
        End Function

        ''' <summary>
        ''' Retrieves the favorites marked by the authenticating user.
        ''' </summary>
        ''' <param name="Parameters">A <see>TwitterParameters</see> object detailing how the request will be executed.</param>
        ''' <returns>A <c>List(Of <see>TwitterStatus</see>)</c> representing the favorites.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Get Favorites" lang="vbnet" title="Retrieving favorites"></code>
        ''' </remarks>
        Public Function Favorites(ByVal Parameters As TwitterParameters) As List(Of TwitterStatus)
            Dim Url As String = FAVORITES_LIST
            If Parameters IsNot Nothing Then
                Url = Parameters.BuildUrl(Url)
            End If
            Return ParseStatuses(PerformWebRequest(Url, "GET"))
        End Function

        ''' <summary>
        ''' Creates a new favorite.
        ''' </summary>
        ''' <param name="ID">The ID of the message to be marked as a favorite.</param>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Create Favorite" lang="vbnet" title="Creating a favorite"></code>
        ''' </remarks>
        Public Sub AddFavorite(ByVal ID As Int64)
            PerformWebRequest(String.Format(FAVORITES_CREATE, ID), "POST")
        End Sub

        ''' <summary>
        ''' Destroys a favorite created by the authenticating user.
        ''' </summary>
        ''' <param name="ID">The ID of the favorite to be deleted.</param>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Delete Favorite" lang="vbnet" title="Deleting a favorite"></code>
        ''' </remarks>
        Public Sub DeleteFavorite(ByVal ID As Int64)
            PerformWebRequest(String.Format(FAVORITES_DESTROY, ID), "POST")
        End Sub
#End Region

#Region "Notification Methods"
        ' Nothing here yet
#End Region

#Region "Block Methods"
        ''' <summary>
        ''' Blocks the specified user. Destroys a friendship to the blocked user if it exists.
        ''' </summary>
        ''' <param name="ID">The ID of the user to be blocked.</param>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Block User" lang="vbnet" title="Blocking a user"></code>
        ''' You can read more about blocking at <a href="http://help.twitter.com/forums/10711/entries/15355">http://help.twitter.com/forums/10711/entries/15355</a>.
        ''' </remarks>
        Public Sub BlockUser(ByVal ID As Int64)
            PerformWebRequest(String.Format(BLOCKS_CREATE, ID), "POST")
        End Sub

        ''' <summary>
        ''' Blocks the specified user. Destroys a friendship to the blocked user if it exists.
        ''' </summary>
        ''' <param name="ScreenName">The screen name of the user to be blocked.</param>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Block User" lang="vbnet" title="Blocking a user"></code>
        ''' You can read more about blocking at <a href="http://help.twitter.com/forums/10711/entries/15355">http://help.twitter.com/forums/10711/entries/15355</a>.
        ''' </remarks>
        Public Sub BlockUser(ByVal ScreenName As String)
            PerformWebRequest(String.Format(BLOCKS_CREATE, ScreenName), "POST")
        End Sub

        ''' <summary>
        ''' Un-blocks the specified user. 
        ''' </summary>
        ''' <param name="ID">The ID of the user to be un-blocked.</param>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Unblock User" lang="vbnet" title="Unblocking a user"></code>
        ''' You can read more about blocking at <a href="http://help.twitter.com/forums/10711/entries/15355">http://help.twitter.com/forums/10711/entries/15355</a>.
        ''' </remarks>
        Public Sub UnblockUser(ByVal ID As Int64)
            PerformWebRequest(String.Format(BLOCKS_DESTROY, ID), "POST")
        End Sub

        ''' <summary>
        ''' Un-blocks the specified user. 
        ''' </summary>
        ''' <param name="ScreenName">The screen name of the user to be un-blocked.</param>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Unblock User" lang="vbnet" title="Unblocking a user"></code>
        ''' You can read more about blocking at <a href="http://help.twitter.com/forums/10711/entries/15355">http://help.twitter.com/forums/10711/entries/15355</a>.
        ''' </remarks>
        Public Sub UnblockUser(ByVal ScreenName As String)
            PerformWebRequest(String.Format(BLOCKS_DESTROY, ScreenName), "POST")
        End Sub

        ''' <summary>
        ''' Returns a List of IDs blocked by the authenticated user.
        ''' </summary>
        ''' <returns>A <c>List(Of Int64)</c>representing the blocked IDs.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Get Blocked IDs" lang="vbnet" title="Getting blocked IDs"></code>
        ''' You can read more about blocking at <a href="http://help.twitter.com/forums/10711/entries/15355">http://help.twitter.com/forums/10711/entries/15355</a>.
        ''' </remarks>
        Public Function BlockedIDs() As List(Of Int64)
            Return ParseBlockedIDs(PerformWebRequest(BLOCKS_BLOCKING_IDS, "GET"))
        End Function

        ''' <summary>
        ''' Returns a list of users blocked by the authenticating user.
        ''' </summary>
        ''' <returns>A <c>List(Of TwitterUser)</c> representing the blocked users.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Get Blocked Users" lang="vbnet" title="Getting blocked users"></code>
        ''' You can read more about blocking at <a href="http://help.twitter.com/forums/10711/entries/15355">http://help.twitter.com/forums/10711/entries/15355</a>.
        ''' </remarks>
        Public Function BlockedUsers() As List(Of TwitterUser)
            Return BlockedUsers(1)
        End Function

        ''' <summary>
        ''' Returns a list of users blocked by the authenticating user.
        ''' </summary>
        ''' <param name="PageNumber">The page number</param>
        ''' <returns>A <c>List(Of TwitterUser)</c> representing the blocked users.</returns>
        Public Function BlockedUsers(ByVal PageNumber As Integer) As List(Of TwitterUser)
            Dim Parameters As New TwitterParameters
            Dim Url As String
            Parameters.Add(TwitterParameterNames.Page, PageNumber)
            Url = Parameters.BuildUrl(BLOCKS_BLOCKING)
            Return ParseUsers(PerformWebRequest(Url, "GET"))
        End Function
#End Region

#Region "Spam Reporting Methods"
        ''' <summary>
        ''' Reports a user to Twitter as a spammer.
        ''' </summary>
        ''' <param name="ID">The ID of the user who is posting spam.</param>
        ''' <returns>A <c>TwitterStatus</c> object representing the user being reported.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Reporting a Spammer" lang="vbnet" title="Reporting a Spammer"></code>
        ''' </remarks>
        Public Function ReportSpam(ByVal ID As Int64) As TwitterUser
            Dim tp As New TwitterParameters
            tp.Add(TwitterParameterNames.ID, ID)
            Dim Url As String = tp.BuildUrl(REPORT_SPAM)
            Return ParseUsers(PerformWebRequest(Url, "POST"))(0)
        End Function

        ''' <summary>
        ''' Reports a user to Twitter as a spammer.
        ''' </summary>
        ''' <param name="ScreenName">The ID of the user who is posting spam.</param>
        ''' <returns>A <c>TwitterStatus</c> object representing the user being reported.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="Reporting a Spammer" lang="vbnet" title="Reporting a Spammer"></code>
        ''' </remarks>
        Public Function ReportSpam(ByVal ScreenName As String) As TwitterUser
            Dim tp As New TwitterParameters
            tp.Add(TwitterParameterNames.ScreenName, ScreenName)
            Dim Url As String = tp.BuildUrl(REPORT_SPAM)
            Return ParseUsers(PerformWebRequest(Url, "POST"))(0)
        End Function
#End Region

#Region "Saved Searches Methods"
        ' nothing here yet
#End Region

#Region "OAuth Methods"
        ''' <summary>
        ''' Returns a URL that will allow users to authenticate your application with Twitter.
        ''' </summary>
        ''' <param name="ConsumerKey">The Consumer Key assigned to your application by Twitter.</param>
        ''' <param name="ConsumerKeySecret">The Consumer Key Secret assigned to your application by Twitter.</param>
        ''' <returns>A <c>String</c> containing the URL where the user can authenticate your application.</returns>
        Public Function GetAuthenticationLink(ByVal ConsumerKey As String, ByVal ConsumerKeySecret As String) As String
            Me.Twitter_OAuth = New TwitterOAuth(ConsumerKey, ConsumerKeySecret)
            Return Me.Twitter_OAuth.GetAuthenticationLink
        End Function

        ''' <summary>
        ''' Returns a URL that will allow users to authenticate your application with Twitter.
        ''' </summary>
        ''' <param name="ConsumerKey">The Consumer Key assigned to your application by Twitter.</param>
        ''' <param name="ConsumerKeySecret">The Consumer Key Secret assigned to your application by Twitter.</param>
        ''' <param name="CallbackUrl">The Url where users are taken after successful authentication.</param>
        ''' <returns>A <c>String</c> containing the URL where the user can authenticate your application.</returns>
        ''' <remarks>
        ''' This method should only be used when you need to specify a callback url that is different from the one in your Twitter application
        ''' registration.  For example, you would probably pass a CallbackUrl of "http://localhost" to do local testing.
        ''' </remarks>
        Public Function GetAuthenticationLink(ByVal ConsumerKey As String, ByVal ConsumerKeySecret As String, ByVal CallbackUrl As String) As String
            Me.Twitter_OAuth = New TwitterOAuth(ConsumerKey, ConsumerKeySecret, CallbackUrl)
            Return Me.Twitter_OAuth.GetAuthenticationLink
        End Function

        ''' <summary>
        ''' Returns a URL that will allow users to authorize your application with Twitter.
        ''' </summary>
        ''' <param name="ConsumerKey">The Consumer Key assigned to your application by Twitter.</param>
        ''' <param name="ConsumerKeySecret">The Consumer Key Secret assigned to your application by Twitter.</param>
        ''' <returns>A <c>String</c> containing the URL where the user can authorize your application.</returns>
        Public Function GetAuthorizationLink(ByVal ConsumerKey As String, ByVal ConsumerKeySecret As String) As String
            Me.Twitter_OAuth = New TwitterOAuth(ConsumerKey, ConsumerKeySecret)
            Return Me.Twitter_OAuth.GetAuthorizationLink()
        End Function

        ''' <summary>
        ''' Returns a URL that will allow users to authorize your application with Twitter.
        ''' </summary>
        ''' <param name="ConsumerKey">The Consumer Key assigned to your application by Twitter.</param>
        ''' <param name="ConsumerKeySecret">The Consumer Key Secret assigned to your application by Twitter.</param>
        ''' <param name="CallbackUrl">The Url where users are taken after successful authorization.</param>
        ''' <returns>A <c>String</c> containing the URL where the user can authorize your application.</returns>
        ''' <remarks>
        ''' This method should only be used when you need to specify a callback url that is different from the one in your Twitter application
        ''' registration.  For example, you would probably pass a CallbackUrl of "http://localhost" to do local testing.
        ''' </remarks>
        Public Function GetAuthorizationLink(ByVal ConsumerKey As String, ByVal ConsumerKeySecret As String, ByVal CallbackUrl As String) As String
            Me.Twitter_OAuth = New TwitterOAuth(ConsumerKey, ConsumerKeySecret, CallbackUrl)
            Return Me.Twitter_OAuth.GetAuthorizationLink()
        End Function

        ''' <summary>
        ''' Asks Twitter to confirm that the user has authorized your application.
        ''' </summary>
        ''' <param name="PIN">The PIN number given to your user when they authorized your application.</param>
        ''' <returns>If the PIN is correct, <c>True</c>, otherwise, <c>False</c>.</returns>
        ''' <remarks>
        ''' <c>GetAuthorizationLink()</c> must be called before this method, and both methods must be executed against the same instance 
        ''' of the <c>TwitterOAuth</c> object.  See the Twitter OAuth Tutorial at the TwitterVB web site for details on the OAuth process.
        ''' </remarks>
        Public Function ValidatePIN(ByVal PIN As String) As Boolean
            Return Me.Twitter_OAuth.ValidatePIN(PIN)
        End Function

        ''' <summary>
        ''' Exchanges a request token for an access token from Twitter.
        ''' </summary>
        ''' <param name="ConsumerKey"></param>
        ''' <param name="ConsumerKeySecret"></param>
        ''' <param name="OAuthToken"></param>
        ''' <param name="OAuthVerifier"></param>
        ''' <remarks>
        ''' <c>GetAuthorizationLink()</c> must be called before this method.  See the Twitter OAuth Tutorial at the TwitterVB web site for details on the OAuth process.
        ''' </remarks>
        Public Sub GetAccessTokens(ByVal ConsumerKey As String, ByVal ConsumerKeySecret As String, ByVal OAuthToken As String, ByVal OAuthVerifier As String)
            With Me.Twitter_OAuth
                .ConsumerKey = ConsumerKey
                .ConsumerSecret = ConsumerKeySecret
                .GetAccessToken(OAuthToken, OAuthVerifier)
            End With
        End Sub

        ''' <summary>
        ''' Configures the TwitterAPI object to use OAuth authentication
        ''' </summary>
        ''' <param name="ConsumerKey">The Consumer Key assigned to your application by Twitter.</param>
        ''' <param name="ConsumerKeySecret">The Consumer Key Secret assigned to your application by Twitter.</param>
        ''' <param name="Token">The OAuth Token given to you by Twitter when the user authorized your application.</param>
        ''' <param name="TokenSecret">The OAuth Token Secret given to you by Twitter when the user authorized your application.</param>
        ''' <remarks>See the Twitter OAuth Tutorial at the Twitter web site for details on the OAuth process.</remarks>
        Public Sub AuthenticateWith(ByVal ConsumerKey As String, ByVal ConsumerKeySecret As String, ByVal Token As String, ByVal TokenSecret As String)
            Me.Twitter_OAuth = New TwitterOAuth(ConsumerKey, ConsumerKeySecret, Token, TokenSecret)
        End Sub
#End Region

#Region "Local Trends Methods"

        Public Function TrendLocations() As List(Of TwitterTrendLocation)
            Return ParseTrendLocations(PerformWebRequest(TREND_LOCATIONS, "GET"))
        End Function


        Public Function TrendsByLocation(ByVal LocationID As String) As List(Of TwitterTrend)
            Return ParseTrends(PerformWebRequest(String.Format(TREND_LOCATION_TRENDS, LocationID), "GET"), TrendFormat.ByLocation)
        End Function

#End Region

#End Region

#Region "File Upload Methods"

        '
        ' The methods in this region provide access to non-Twitter APIs for uploading and sharing files
        '

        ''' <summary>
        ''' Uploads a photo from the user's hard drive to the TweetPhoto service
        ''' </summary>
        ''' <param name="Filename">The full path and filename of the image to be uploaded to Twitpic.</param>
        ''' <param name="Message">The message, if any, to include with the picture.</param>
        ''' <param name="Username">The user's Twitter username.</param>
        ''' <param name="Password">The user's Twitter password.</param>
        ''' <param name="APIKey">Your application's TweetPhoto API key..</param>
        ''' <returns>A <c>String</c> containing the TweetPhoto Url of the picture.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="TweetPhoto" lang="vbnet" title="Posting an image with TweetPhoto"></code>
        ''' TwitPic does not implement OAuth, so it is up to developers to prompt the users for their Twitter login credentials.
        ''' </remarks>
        Public Function TweetPhotoUpload(ByVal Filename As String, ByVal Message As String, ByVal Username As String, ByVal Password As String, ByVal APIKey As String) As String

            Dim request As System.Net.WebRequest = HttpWebRequest.Create("http://tweetphotoapi.com/api/tpapi.svc/upload2")
            request.Method = "POST"
            request.ContentType = "application/x-www-form-urlencoded"
            request.Headers.Add("TPAPIKEY: " + APIKey)
            request.Headers.Add("TPAPI: " + Username + "," + Password)

            Dim FileContentType As String = String.Empty
            If Filename.ToUpper.EndsWith(".GIF") Then
                FileContentType = "image/gif"
            ElseIf Filename.ToUpper.EndsWith(".JPG") Then
                FileContentType = "image/jpeg"
            ElseIf Filename.ToUpper.EndsWith(".JPEG") Then
                FileContentType = "image/jpeg"
            ElseIf Filename.ToUpper.EndsWith(".PNG") Then
                FileContentType = "image/png"
            Else
                Return String.Empty
            End If

            request.Headers.Add("TPMIMETYPE: " & FileContentType)
            request.Headers.Add("TPPOST:True")
            request.Headers.Add("TPMSG: " & Message)
            'request.Headers.Add("TPTAGS: comma,seperated,tags,here")

            'Read image file into a byte array
            Dim content() As Byte

            Try
                content = File.ReadAllBytes(Filename)
                request.ContentLength = content.Length

                Using str As System.IO.Stream = request.GetRequestStream()
                    str.Write(content, 0, content.Length)
                End Using

                Dim response As WebResponse = request.GetResponse()
                Dim reader As StreamReader
                reader = New StreamReader(response.GetResponseStream())

                Return (reader.ReadToEnd())

            Catch ex As Exception
                Return String.Empty

            End Try
        End Function

        ''' <summary>
        ''' Uploads a binary file to the site FileSocial.com and publishes the link to Twitter
        ''' </summary>
        ''' <param name="UserName">Twitter UserName</param>
        ''' <param name="Password">Twitter Password</param>
        ''' <param name="FileName">FileName that you want to upload (note 50M limit)</param>
        ''' <param name="Message">Message with the file link</param>
        ''' <returns>The StatusID that is created as a result of the update</returns>
        ''' <remarks>In order to use this function you will need to register your account <a href="http://filesocial.com">with filesocial.com</a></remarks>
        Public Function FileSocialUpload(ByVal UserName As String, ByVal Password As String, ByVal FileName As String, ByVal Message As String) As String
            Dim ReturnValue As String = String.Empty
            Try
                Dim BinaryData As Byte() = System.IO.File.ReadAllBytes(FileName)
                Dim boundary As String = Guid.NewGuid.ToString()
                Dim Header As String = String.Format("--{0}", boundary)
                Dim Footer As String = String.Format("--{0}--", boundary)
                Dim Request As HttpWebRequest = DirectCast(System.Net.WebRequest.Create("http://filesocial.com/api/uploadAndPost"), HttpWebRequest)
                With Request
                    .PreAuthenticate = True
                    .AllowWriteStreamBuffering = True
                    .ContentType = String.Format("multipart/form-data; boundary={0}", boundary)
                    .Method = "POST"
                End With
                Dim FileContentType As String = "application/octet-stream"
                Dim FileHeader As String = [String].Format("Content-Disposition: file; name=""{0}""; filename=""{1}""", "file", FileName)
                Dim FileData As String = Encoding.GetEncoding("iso-8859-1").GetString(BinaryData)

                Dim Contents As New StringBuilder()
                With Contents
                    .AppendLine(Header)

                    .AppendLine(FileHeader)
                    .AppendLine([String].Format("Content-Type: {0}", FileContentType))
                    .AppendLine()
                    .AppendLine(FileData)

                    .AppendLine(Header)
                    .AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "username"))
                    .AppendLine()
                    .AppendLine(UserName)

                    .AppendLine(Header)
                    .AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "password"))
                    .AppendLine()
                    .AppendLine(Password)


                    If Not String.IsNullOrEmpty(Message) Then
                        .AppendLine(Header)
                        .AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "message"))
                        .AppendLine()
                        .AppendLine(Message)
                    End If

                    .AppendLine(Footer)

                    Dim Bytes As Byte() = Encoding.GetEncoding("iso-8859-1").GetBytes(Contents.ToString())
                    Request.ContentLength = Bytes.Length

                    Using requestStream As Stream = Request.GetRequestStream()
                        requestStream.Write(Bytes, 0, Bytes.Length)

                        Using Response As HttpWebResponse = DirectCast(Request.GetResponse(), HttpWebResponse)
                            Using Reader As New StreamReader(Response.GetResponseStream())
                                Dim Result As String = Reader.ReadToEnd()

                                Dim ResponseDoc As New XmlDocument

                                'My.Computer.Clipboard.SetText(Result)
                                ResponseDoc.LoadXml(Result)
                                Dim rsp As XmlNode = ResponseDoc.SelectSingleNode("//rsp")

                                If rsp.Attributes("status").Value = "ok" Then


                                    ReturnValue = rsp.SelectSingleNode("//mediaurl").InnerText
                                    'ReturnValue = Result

                                End If
                            End Using
                        End Using
                    End Using
                End With

            Catch ex As Exception

            End Try
            Return ReturnValue

        End Function

        ''' <summary>
        ''' Uploads a photo from the user's hard drive to the TwitPic service
        ''' </summary>
        ''' <param name="Filename">The full path and filename of the image to be uploaded to Twitpic.</param>
        ''' <param name="Message">The message, if any, to include with the picture.</param>
        ''' <param name="Username">The user's Twitter username.</param>
        ''' <param name="Password">The user's Twitter password.</param>
        ''' <param name="Source">The source of the post.  This will only work if your application has been registered with Twitpic.com.</param>
        ''' <returns>A <c>String</c> containing the TwitPic Url of the picture.</returns>
        ''' <remarks>
        ''' <code source="TwitterVB2\examples.vb" region="TwitPic" lang="vbnet" title="Posting an image with TwitPic"></code>
        ''' TwitPic does not implement OAuth, so it is up to developers to prompt the users for their Twitter login credentials.
        ''' </remarks>
        Public Function TwitPicUpload(ByVal Filename As String, ByVal Message As String, ByVal Username As String, ByVal Password As String, Optional ByVal Source As String = "") As String

            Dim ReturnValue As String = String.Empty

            Try
                ' Get the file into an array of bytes
                Dim BinaryData As Byte() = System.IO.File.ReadAllBytes(Filename)

                Dim boundary As String = Guid.NewGuid().ToString()
                Dim Header As String = String.Format("--{0}", boundary)
                Dim Footer As String = String.Format("--{0}--", boundary)

                ' Build the request
                Dim Request As HttpWebRequest = DirectCast(System.Net.WebRequest.Create("http://twitpic.com/api/uploadAndPost"), HttpWebRequest)

                With Request
                    .PreAuthenticate = True
                    .AllowWriteStreamBuffering = True
                    .ContentType = String.Format("multipart/form-data; boundary={0}", boundary)
                    .Method = "POST"
                End With

                Dim FileContentType As String = String.Empty
                If Filename.ToUpper.EndsWith(".GIF") Then
                    FileContentType = "image/gif"
                ElseIf Filename.ToUpper.EndsWith(".JPG") Then
                    FileContentType = "image/jpeg"
                ElseIf Filename.ToUpper.EndsWith(".JPEG") Then
                    FileContentType = "image/jpeg"
                ElseIf Filename.ToUpper.EndsWith(".PNG") Then
                    FileContentType = "image/png"
                Else
                    Return String.Empty
                End If

                Dim FileHeader As String = [String].Format("Content-Disposition: file; name=""{0}""; filename=""{1}""", "media", Filename)
                Dim FileData As String = Encoding.GetEncoding("iso-8859-1").GetString(BinaryData)

                Dim Contents As New StringBuilder()
                With Contents
                    .AppendLine(Header)

                    .AppendLine(FileHeader)
                    .AppendLine([String].Format("Content-Type: {0}", FileContentType))
                    .AppendLine()
                    .AppendLine(FileData)

                    .AppendLine(Header)
                    .AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "username"))
                    .AppendLine()
                    .AppendLine(Username)

                    .AppendLine(Header)
                    .AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "password"))
                    .AppendLine()
                    .AppendLine(Password)

                    If Source <> String.Empty Then
                        .AppendLine(Header)
                        .AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "source"))
                        .AppendLine()
                        .AppendLine(Source)
                    End If

                    If Not String.IsNullOrEmpty(Message) Then
                        .AppendLine(Header)
                        .AppendLine([String].Format("Content-Disposition: form-data; name=""{0}""", "message"))
                        .AppendLine()
                        .AppendLine(Message)
                    End If

                    .AppendLine(Footer)

                    Dim Bytes As Byte() = Encoding.GetEncoding("iso-8859-1").GetBytes(Contents.ToString())
                    Request.ContentLength = Bytes.Length

                    Using requestStream As Stream = Request.GetRequestStream()
                        requestStream.Write(Bytes, 0, Bytes.Length)

                        Using Response As HttpWebResponse = DirectCast(Request.GetResponse(), HttpWebResponse)
                            Using Reader As New StreamReader(Response.GetResponseStream())
                                Dim Result As String = Reader.ReadToEnd()

                                Dim ResponseDoc As New XmlDocument
                                ResponseDoc.LoadXml(Result)
                                Dim rsp As XmlNode = ResponseDoc.SelectSingleNode("//rsp")
                                If rsp.Attributes("status").Value = "ok" Then
                                    ReturnValue = ResponseDoc.SelectSingleNode("//mediaurl").Value
                                End If
                            End Using
                        End Using
                    End Using
                End With

            Catch ex As Exception
                ' Deliberately doing nothing here.
                ' We just want to swallow the exception so that the application doesn't see an exception

            End Try

            Return ReturnValue
        End Function
#End Region

#Region "Url Shortening Methods"

        '
        ' The methods in this region support URL shortening
        '

        ''' <summary>
        ''' Submits a Url to the selected Url shortening service.
        ''' </summary>
        ''' <param name="UrlToShorten">The Url that will be shortened.</param>
        ''' <param name="Shortener">A <c>UrlShortener</c> indicating which shortener service will be used.</param>
        ''' <param name="p_strBRST">Optional: if using the Br.ST service you must include this key. otherwise an exception will be raised</param>
        ''' <param name="p_strbitlyKey">Optional: if using the bit.ly service, you must provide this key.</param>
        ''' <param name="p_strbitlyUser">Optional: bit.ly user</param>
        ''' <returns>The shortened Url.</returns>
        ''' <remarks></remarks>
        ''' 

        Public Function ShortenUrl(ByVal UrlToShorten As String, ByVal Shortener As Globals.UrlShortener, Optional ByVal p_strBRST As String = "<noapi>", Optional ByVal p_strbitlyKey As String = "", Optional ByVal p_strbitlyUser As String = "") As String
            Dim ReturnValue As String = String.Empty

            Select Case Shortener
                Case UrlShortener.IsGd
                    ReturnValue = PerformWebRequest(String.Format("http://is.gd/api.php?longurl={0}", System.Web.HttpUtility.UrlEncode(UrlToShorten)))

                Case UrlShortener.TinyUrl
                    ReturnValue = PerformWebRequest(String.Format("http://tinyurl.com/api-create.php?url={0}", System.Web.HttpUtility.UrlEncode(UrlToShorten)))


                    'Case UrlShortener.BrSt
                    '    Dim oBRST As New st.br.Service
                    '    Dim oResult As st.br.GenerationResult = Nothing
                    '    If p_strBRST <> "<noapi>" Then
                    '        oResult = oBRST.URLGenerateAlias(UrlToShorten, Nothing, p_strBRST)
                    '        If oResult.wasSuccessful Then
                    '            Return oResult.aliasURL
                    '        Else
                    '            Return String.Empty
                    '        End If
                    '    Else
                    '        Return String.Empty
                    '    End If

                Case UrlShortener.BitLy
                    ReturnValue = PerformWebRequest(String.Format("http://api.bit.ly/v3/shorten?login={0}&apiKey={1}&longUrl={2}&format=txt", p_strbitlyUser, p_strbitlyKey, UrlToShorten))

            End Select

            Return ReturnValue
        End Function
#End Region



    End Class

End Namespace
