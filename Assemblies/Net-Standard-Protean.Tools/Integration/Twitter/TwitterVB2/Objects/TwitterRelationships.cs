using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protean.Tools.Integration.Twitter.TwitterVB2
{
    /// <summary>
    /// An object that encapsulates the relationship between two Twitter users. This object provides an interface to the <c>friendships/show</c> Twitter REST API method.
    /// An object that represents the relationship between two Twitter users. This object provides an interface to the <c>friendships/show</c> Twitter REST API method.
    /// </summary>
    /// <remarks></remarks>
    public partial class TwitterRelationship : XmlObjectBase
    {

        private TwitterRelationshipElement _Source;
        private TwitterRelationshipElement _Target;

        /// <summary>
        /// The relationship between the source and target users.
        /// </summary>
        /// <value></value>
        /// <returns>A <c>TwitterRelationshipElement</c> representing the relationship between the source and target users, in the context of the source user.</returns>
        /// <remarks></remarks>
        public TwitterRelationshipElement Source
        {
            get
            {
                return _Source;
            }
            set
            {
                _Source = value;
            }
        }

        /// <summary>
        /// The relationship between the source and target users.
        /// </summary>
        /// <value></value>
        /// <returns>A <c>TwitterRelationshipElement</c> representing the relationship between the source and target users, in the context of the target user.</returns>
        /// <remarks></remarks>
        public TwitterRelationshipElement Target
        {
            get
            {
                return _Target;
            }
            set
            {
                _Target = value;
            }
        }

        /// <summary>
        /// Creates a new <c>TwitterRelationshipElement</c> object.
        /// </summary>
        /// <param name="RelationshipNode">An <c>XmlNode</c> from the Twitter API response representing a relationship.</param>
        /// <remarks></remarks>
        /// <exclude/>
        public TwitterRelationship(System.Xml.XmlNode RelationshipNode)
        {

            if (RelationshipNode["source"] != null)
            {
                Source = new TwitterRelationshipElement(RelationshipNode["source"]);
            }
            if (RelationshipNode["target"] != null)
            {
                Target = new TwitterRelationshipElement(RelationshipNode["target"]);
            }

        }

        /// <summary>
        /// Encapsulates the relationship of one user to another
        /// </summary>
        /// <remarks></remarks>
        public partial class TwitterRelationshipElement : XmlObjectBase
        {

            private long _ID;
            private string _ScreenName;
            private bool _Following;
            private bool _FollowedBy;
            private bool _NotificationsEnabled;

            /// <summary>
            /// The Twitter ID of the in-context user
            /// </summary>
            /// <value></value>
            /// <returns>An <c>Int64</c> representing the Twitter ID of the in-context user.</returns>
            /// <remarks></remarks>
            public long ID
            {
                get
                {
                    return _ID;
                }
                set
                {
                    _ID = value;
                }
            }

            /// <summary>
            /// The Twitter screen name of the in-context user
            /// </summary>
            /// <value></value>
            /// <returns>An <c>string</c> representing the screen name of the in-context user.</returns>
            /// <remarks></remarks>
            public string ScreenName
            {
                get
                {
                    return _ScreenName;
                }
                set
                {
                    _ScreenName = value;
                }
            }

            /// <summary>
            /// Whether the in-context user follows the other user
            /// </summary>
            /// <value></value>
            /// <returns>An <c>boolean</c> that indicates if the in-context user follows the other user.</returns>
            /// <remarks></remarks>
            public bool Following
            {
                get
                {
                    return _Following;
                }
                set
                {
                    _Following = value;
                }
            }

            /// <summary>
            /// Whether the in-context user is followed by the other user
            /// </summary>
            /// <value></value>
            /// <returns>An <c>boolean</c> that indicates if the in-context user is followed by the other user.</returns>
            /// <remarks></remarks>
            public bool FollowedBy
            {
                get
                {
                    return _FollowedBy;
                }
                set
                {
                    _FollowedBy = value;
                }
            }

            /// <summary>
            /// Whether the in-context user receives device notifications regarding status updates of other user
            /// </summary>
            /// <value></value>
            /// <returns>An <c>boolean</c> that indicates if the in-context user receives device notifications regarding status updates of the other user.</returns>
            /// <remarks>Due to its private nature, the Twitter API only populates this property in the context of the source user and only if the source user is authenticated.<para />The <c>NotificationsEnabled</c> property of the <c>TwitterRelationshipElement</c> object in the context of the target user will always be Null, as will the <c>NotificationsEnabled</c> property of the <c>TwitterRelationshipElement</c> object in the context of the source user if the source user is unauthenticated.</remarks>
            public bool NotificationsEnabled
            {
                get
                {
                    return _NotificationsEnabled;
                }
                set
                {
                    _NotificationsEnabled = value;
                }
            }

            /// <summary>
            /// Creates a new <c>TwitterRelationshipElement</c> object.
            /// </summary>
            /// <param name="RelationshipElementNode">An <c>XmlNode</c> from the Twitter API response representing a relationship element.</param>
            /// <remarks>As exposed by the Twitter API, a relationship between two users is made up of two elements. Each element shows Following and FollowedBy relationship in the context of one particular user. Both elements will contain the same information, but are provided by the Twitter API for "clarity".</remarks>
            /// <exclude/>
            public TwitterRelationshipElement(System.Xml.XmlNode RelationshipElementNode)
            {
                ID = XmlInt64_Get(RelationshipElementNode["id"]);
                ScreenName = XmlString_Get(RelationshipElementNode["screen_name"]);
                Following = XmlBoolean_Get(RelationshipElementNode["following"]);
                FollowedBy = XmlBoolean_Get(RelationshipElementNode["followed_by"]);
                NotificationsEnabled = XmlBoolean_Get(RelationshipElementNode["notifications_enabled"]);
            }

        }

    }
}
