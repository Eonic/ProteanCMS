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


Namespace Integration.Twitter.TwitterVB2
    ''' <summary>
    ''' A Twitter list.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class TwitterList
        Inherits XmlObjectBase

        Private _ID As Int64
        Private _Name As String = String.Empty
        Private _FullName As String = String.Empty
        Private _Slug As String = String.Empty
        Private _Description As String = String.Empty
        Private _SubscriberCount As Int64
        Private _Member_Count As Int64
        Private _Url As String = String.Empty
        Private _Mode As String = String.Empty
        Private _User As New TwitterUser

        ''' <summary>
        ''' Default constructor.
        ''' </summary>
        ''' <remarks></remarks>
        ''' <exclude/>
        Public Sub New()
        End Sub

        ''' <summary>
        ''' Creates a new <c>List</c> object.
        ''' </summary>
        ''' <param name="ListNode">An <c>XmlNode</c> from the Twitter API response representing a user.</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal ListNode As System.Xml.XmlNode)
            With Me
                .ID = XmlInt64_Get(ListNode("id"))
                .Name = XmlString_Get(ListNode("name"))
                .FullName = XmlString_Get(ListNode("full_name"))
                .Slug = XmlString_Get(ListNode("slug"))
                .Description = XmlString_Get(ListNode("description"))
                .SubscriberCount = XmlInt64_Get(ListNode("subscriber_count"))
                .MemberCount = XmlInt64_Get(ListNode("member_count"))
                .Mode = XmlString_Get(ListNode("mode"))
                .Url = XmlString_Get(ListNode("url"))
                .User = New TwitterUser(ListNode("user"))
            End With
        End Sub

        ''' <summary>
        ''' The ID of the list.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ID() As Int64
            Get
                Return _ID
            End Get
            Set(ByVal value As Int64)
                _ID = value
            End Set
        End Property

        ''' <summary>
        ''' The name of the list.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Name() As String
            Get
                Return _Name
            End Get
            Set(ByVal value As String)
                _Name = value
            End Set
        End Property

        ''' <summary>
        ''' The full name of the list.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property FullName() As String
            Get
                Return _FullName
            End Get
            Set(ByVal value As String)
                _FullName = value
            End Set
        End Property

        ''' <summary>
        ''' The slug of the list.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>Slug is an alternate name for the list.</remarks>
        Public Property Slug() As String
            Get
                Return _Slug
            End Get
            Set(ByVal value As String)
                _Slug = value
            End Set
        End Property

        ''' <summary>
        ''' The description of the list.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Description() As String
            Get
                Return _Description
            End Get
            Set(ByVal value As String)
                _Description = value
            End Set
        End Property

        ''' <summary>
        ''' The number of users who subscribe to the list.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property SubscriberCount() As Int64
            Get
                Return _SubscriberCount
            End Get
            Set(ByVal value As Int64)
                _SubscriberCount = value
            End Set
        End Property

        ''' <summary>
        ''' The number of people who are members of the list.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property MemberCount() As Int64
            Get
                Return _Member_Count
            End Get
            Set(ByVal value As Int64)
                _Member_Count = value
            End Set
        End Property

        ''' <summary>
        ''' The Url where the list can be found.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Url() As String
            Get
                Return _Url
            End Get
            Set(ByVal value As String)
                _Url = value
            End Set
        End Property

        ''' <summary>
        ''' Whether or not the list is public or private.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>"Public" and "Private" are the only valid values.</remarks>
        Public Property Mode() As String
            Get
                Return _Mode
            End Get
            Set(ByVal value As String)
                If value.ToUpper = "PUBLIC" Then
                    _Mode = "Public"
                ElseIf value.ToUpper = "PRIVATE" Then
                    _Mode = "Private"
                End If
            End Set
        End Property

        ''' <summary>
        ''' The user who is the owner of the list.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property User() As TwitterUser
            Get
                Return _User
            End Get
            Set(ByVal value As TwitterUser)
                _User = value
            End Set
        End Property

    End Class
End Namespace
