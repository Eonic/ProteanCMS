Namespace Integration.Twitter.TwitterVB2
    ''' <summary>
    ''' An object that encapsulates the relationship between two Twitter users. This object provides an interface to the <c>friendships/show</c> Twitter REST API method.
    ''' An object that represents the relationship between two Twitter users. This object provides an interface to the <c>friendships/show</c> Twitter REST API method.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class TwitterRelationship
        Inherits XmlObjectBase

        Private _Source As TwitterRelationshipElement
        Private _Target As TwitterRelationshipElement

        ''' <summary>
        ''' The relationship between the source and target users.
        ''' </summary>
        ''' <value></value>
        ''' <returns>A <c>TwitterRelationshipElement</c> representing the relationship between the source and target users, in the context of the source user.</returns>
        ''' <remarks></remarks>
        Public Property Source() As TwitterRelationshipElement
            Get
                Return _Source
            End Get
            Set(ByVal value As TwitterRelationshipElement)
                _Source = value
            End Set
        End Property

        ''' <summary>
        ''' The relationship between the source and target users.
        ''' </summary>
        ''' <value></value>
        ''' <returns>A <c>TwitterRelationshipElement</c> representing the relationship between the source and target users, in the context of the target user.</returns>
        ''' <remarks></remarks>
        Public Property Target() As TwitterRelationshipElement
            Get
                Return _Target
            End Get
            Set(ByVal value As TwitterRelationshipElement)
                _Target = value
            End Set
        End Property

        ''' <summary>
        ''' Creates a new <c>TwitterRelationshipElement</c> object.
        ''' </summary>
        ''' <param name="RelationshipNode">An <c>XmlNode</c> from the Twitter API response representing a relationship.</param>
        ''' <remarks></remarks>
        ''' <exclude/>
        Public Sub New(ByVal RelationshipNode As System.Xml.XmlNode)

            If RelationshipNode("source") IsNot Nothing Then
                Me.Source = New TwitterRelationshipElement(RelationshipNode("source"))
            End If
            If RelationshipNode("target") IsNot Nothing Then
                Me.Target = New TwitterRelationshipElement(RelationshipNode("target"))
            End If

        End Sub

        ''' <summary>
        ''' Encapsulates the relationship of one user to another
        ''' </summary>
        ''' <remarks></remarks>
        Public Class TwitterRelationshipElement
            Inherits XmlObjectBase

            Private _ID As Int64
            Private _ScreenName As String
            Private _Following As Boolean
            Private _FollowedBy As Boolean
            Private _NotificationsEnabled As Boolean

            ''' <summary>
            ''' The Twitter ID of the in-context user
            ''' </summary>
            ''' <value></value>
            ''' <returns>An <c>Int64</c> representing the Twitter ID of the in-context user.</returns>
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
            ''' The Twitter screen name of the in-context user
            ''' </summary>
            ''' <value></value>
            ''' <returns>An <c>string</c> representing the screen name of the in-context user.</returns>
            ''' <remarks></remarks>
            Public Property ScreenName() As String
                Get
                    Return _ScreenName
                End Get
                Set(ByVal value As String)
                    _ScreenName = value
                End Set
            End Property

            ''' <summary>
            ''' Whether the in-context user follows the other user
            ''' </summary>
            ''' <value></value>
            ''' <returns>An <c>boolean</c> that indicates if the in-context user follows the other user.</returns>
            ''' <remarks></remarks>
            Public Property Following() As Boolean
                Get
                    Return _Following
                End Get
                Set(ByVal value As Boolean)
                    _Following = value
                End Set
            End Property

            ''' <summary>
            ''' Whether the in-context user is followed by the other user
            ''' </summary>
            ''' <value></value>
            ''' <returns>An <c>boolean</c> that indicates if the in-context user is followed by the other user.</returns>
            ''' <remarks></remarks>
            Public Property FollowedBy() As Boolean
                Get
                    Return _FollowedBy
                End Get
                Set(ByVal value As Boolean)
                    _FollowedBy = value
                End Set
            End Property

            ''' <summary>
            ''' Whether the in-context user receives device notifications regarding status updates of other user
            ''' </summary>
            ''' <value></value>
            ''' <returns>An <c>boolean</c> that indicates if the in-context user receives device notifications regarding status updates of the other user.</returns>
            ''' <remarks>Due to its private nature, the Twitter API only populates this property in the context of the source user and only if the source user is authenticated.<para />The <c>NotificationsEnabled</c> property of the <c>TwitterRelationshipElement</c> object in the context of the target user will always be Null, as will the <c>NotificationsEnabled</c> property of the <c>TwitterRelationshipElement</c> object in the context of the source user if the source user is unauthenticated.</remarks>
            Public Property NotificationsEnabled() As Boolean
                Get
                    Return _NotificationsEnabled
                End Get
                Set(ByVal value As Boolean)
                    _NotificationsEnabled = value
                End Set
            End Property

            ''' <summary>
            ''' Creates a new <c>TwitterRelationshipElement</c> object.
            ''' </summary>
            ''' <param name="RelationshipElementNode">An <c>XmlNode</c> from the Twitter API response representing a relationship element.</param>
            ''' <remarks>As exposed by the Twitter API, a relationship between two users is made up of two elements. Each element shows Following and FollowedBy relationship in the context of one particular user. Both elements will contain the same information, but are provided by the Twitter API for "clarity".</remarks>
            ''' <exclude/>
            Public Sub New(ByVal RelationshipElementNode As System.Xml.XmlNode)
                Me.ID = XmlInt64_Get(RelationshipElementNode("id"))
                Me.ScreenName = XmlString_Get(RelationshipElementNode("screen_name"))
                Me.Following = XmlBoolean_Get(RelationshipElementNode("following"))
                Me.FollowedBy = XmlBoolean_Get(RelationshipElementNode("followed_by"))
                Me.NotificationsEnabled = XmlBoolean_Get(RelationshipElementNode("notifications_enabled"))
            End Sub

        End Class

    End Class

End Namespace
