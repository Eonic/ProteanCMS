using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;
using System.Xml;
using Newtonsoft.Json;

namespace Protean.Integration.Directory
{

    /// <summary>
    /// Twitter class
    /// </summary>
    /// <remarks></remarks>

    public class Facebook : Directory.BaseProvider, Directory.IPostable
    {

        public string facebookId;
        public string facebookKey;
        public string AccessToken;
        private const string mcModuleName = "Integration.Directory.Facebook";

        private const int _postLengthLimit = 140;

        public new event OnErrorEventHandler OnError;

        public new delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);

        public Facebook(ref Cms aWeb, string fbId, string fbKey) : base(ref aWeb)
        {
            facebookId = fbId;
            facebookKey = fbKey;
            this._providerName = "facebook";
        }
        public Facebook(ref Cms aWeb, ref long directoryId) : base(ref aWeb, ref directoryId)
        {
        }

        public long CreateUser(ref User fbUser)
        {
            try
            {
                // Check FB not allready exists if it does return that user id.
                long nUserId = this.ValidateExternalAuth(fbUser.id);
                if (nUserId > 0L)
                {
                    return nUserId;
                }
                else
                {
                    // Get user Instance from Xform
                    XmlElement UserXml = (XmlElement)this.GetUserSchemaXml().FirstChild;
                    // Populate Instance info from Facebook User
                    UserXml.SelectSingleNode("FirstName").InnerText = fbUser.first_name;
                    UserXml.SelectSingleNode("LastName").InnerText = fbUser.last_name;
                    UserXml.SelectSingleNode("Gender").InnerText = fbUser.gender;
                    UserXml.SelectSingleNode("Email").InnerText = fbUser.email;
                    // Create User as best we can
                    string cUserName = fbUser.email;
                    if (string.IsNullOrEmpty(cUserName))
                    {
                        cUserName = fbUser.first_name + fbUser.last_name;
                        // Check if username exists if so the add numbers till not
                        long counter = 1L;
                        while (this.myWeb.moDbHelper.checkUserUnique(cUserName) == false)
                        {
                            cUserName = cUserName + counter;
                            counter = counter + 1L;
                        }
                    }

                    // If data not complete set status as requires data
                    nUserId = base.myWeb.moDbHelper.insertDirectory("fb-" + fbUser.id, "User", cUserName, "", UserXml.OuterXml, default, default, fbUser.email);
                    // Add ExternalAuthInfo
                    this.CreateExternalAuth(fbUser.id, nUserId);
                    return nUserId;
                }
            }
            catch (Exception)
            {
                return 0L;
            }

        }


        public List<User> GetFacebookUserData(string code, string redirectURI)
        {
            try
            {


                // Exchange the code for an access token
                var targetUri = new Uri(Convert.ToString("https://graph.facebook.com/oauth/access_token?client_id=" + facebookId + "&client_secret=" + facebookKey + "&redirect_uri=" + redirectURI + "&code=") + code);
                HttpWebRequest at = (HttpWebRequest)WebRequest.Create(targetUri);

                var str = new StreamReader(at.GetResponse().GetResponseStream());
                string strResult = str.ReadToEnd();

                // TS commented out for move to C#
                // jsonResult = JsonConvert.DeserializeObject<Dictionary<string, object>>(strResult);
                string AccessToken = ""; //jsonResult.Item("access_token").ToString();

                // Exchange the code for an extended access token
                var eatTargetUri = new Uri("https://graph.facebook.com/oauth/access_token?grant_type=fb_exchange_token&client_id=" + facebookId + "&client_secret=" + facebookKey + "&fb_exchange_token=" + AccessToken);
                HttpWebRequest eat = (HttpWebRequest)WebRequest.Create(eatTargetUri);

                var eatStr = new StreamReader(eat.GetResponse().GetResponseStream());
                string eatToken = eatStr.ReadToEnd().ToString();

                // Split the access token and expiration from the single string
                string[] eatWords = eatToken.Split('&');
                string extendedAccessToken = eatWords[0];

                // Request the Facebook user information
                var targetUserUri = new Uri(Convert.ToString("https://graph.facebook.com/me?fields=first_name,last_name,email,gender,locale,link&access_token=") + AccessToken);
                HttpWebRequest user = (HttpWebRequest)WebRequest.Create(targetUserUri);

                // Read the returned JSON object response
                var userInfo = new StreamReader(user.GetResponse().GetResponseStream());
                string jsonResponse = string.Empty;
                jsonResponse = userInfo.ReadToEnd();

                // Deserialize and convert the JSON object to the Facebook.User object type
                var sr = new JavaScriptSerializer();
                string jsondata = jsonResponse;
                var converted = sr.Deserialize<User>(jsondata);

                // Write the user data to a List
                var currentUser = new List<User>();
                currentUser.Add(converted);

                // Return the current Facebook user
                return currentUser;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "Logon", ex, ""));
                return null;
            }
        }

        public class User
        {
            public string id
            {
                get
                {
                    return m_id;
                }
                set
                {
                    m_id = value;
                }
            }
            private string m_id;
            public string first_name
            {
                get
                {
                    return m_first_name;
                }
                set
                {
                    m_first_name = value;
                }
            }
            private string m_first_name;
            public string last_name
            {
                get
                {
                    return m_last_name;
                }
                set
                {
                    m_last_name = value;
                }
            }
            private string m_last_name;
            public string email
            {
                get
                {
                    return m_email;
                }
                set
                {
                    m_email = value;
                }
            }
            private string m_email;
            public string link
            {
                get
                {
                    return m_link;
                }
                set
                {
                    m_link = value;
                }
            }
            private string m_link;
            public string username
            {
                get
                {
                    return m_username;
                }
                set
                {
                    m_username = value;
                }
            }
            private string m_username;
            public string gender
            {
                get
                {
                    return m_gender;
                }
                set
                {
                    m_gender = value;
                }
            }
            private string m_gender;
            public string locale
            {
                get
                {
                    return m_locale;
                }
                set
                {
                    m_locale = value;
                }
            }
            private string m_locale;
        }


    }



}