using System;
using System.Collections.Specialized;
using static Protean.stdTools;
using static Protean.Tools.Http.Utils;


namespace Protean.Integration.Directory
{

    /// <summary>
    /// Twitter class
    /// </summary>
    /// <remarks></remarks>

    public class Twitter : Directory.BaseProvider, Directory.IPostable
    {

        public string twitterConsumerKey = "c2h0VNmcE5c0Vu0fsHEfGg";
        public string twitterConsumerSecret = "5DzFnQtX7cNxuChpJCcz1ZzhGfPGTAh4MYWMuuJvxQ";

        private const int _postLengthLimit = 140;

        public new event OnErrorEventHandler OnError;

        public new delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);


        public Twitter(ref Cms aWeb) : base(ref aWeb)
        {
        }
        public Twitter(ref Cms aWeb, ref long directoryId) : base(ref aWeb, ref directoryId)
        {
        }

        public string GetRequestToken()
        {

            try
            {
                if (this.IsAuthorisedUser)
                {


                    var callbackParameters = new NameValueCollection(3);
                    callbackParameters.Add("oAuthResp", "twitter");
                    callbackParameters.Add("integration", "Twitter.AccessTokens");
                    callbackParameters.Add("dirId", base.DirectoryId.ToString());

                    var callback = Tools.Http.Utils.BuildURIFromRequest(this.myWeb.moRequest, callbackParameters);

                    var twitterAPI = new Tools.Integration.Twitter.TwitterVB2.TwitterAPI();
                    string authenticationLink = twitterAPI.GetAuthenticationLink(twitterConsumerKey, twitterConsumerSecret, callback.ToString());

                    this.myWeb.AddResponse("Twitter.AuthenticationLink", authenticationLink, default, ResponseType.Redirect);
                    this.myWeb.msRedirectOnEnd = authenticationLink;
                    return authenticationLink;
                }
                else
                {

                    this.myWeb.AddResponse("Twitter.GetRequestToken.Unauthorised", "The user requesting this method is not authorised", default, ResponseType.Alert);
                    return "";

                }
            }

            catch (Exception ex)
            {
                OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(this._moduleName, "GetRequestToken", ex, ""));
                return "";
            }


        }

        public void AccessTokens()
        {
            try
            {
                string token = this.myWeb.moRequest("oauth_token");
                string verifier = this.myWeb.moRequest("oauth_verifier");
                AccessTokens(token, verifier);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(this._moduleName, "AccessTokens()", ex, ""));
            }
        }
        public void AccessTokens(string token, string verifier)
        {

            try
            {

                if (this.IsAuthorisedUser)
                {
                    var twitterAPI = new Tools.Integration.Twitter.TwitterVB2.TwitterAPI();

                    if (string.IsNullOrEmpty(token) | string.IsNullOrEmpty(verifier))
                    {
                        // token or Verifier are not populated
                        this.myWeb.AddResponse("Twitter.AccessTokens.NullArguments", "Either the token or the verifier were blank", default, ResponseType.Alert);
                    }

                    else
                    {
                        twitterAPI.GetAccessTokens(twitterConsumerKey, twitterConsumerSecret, token, verifier);

                        this._credentials.AddSetting("OAuth_Token", twitterAPI.OAuth_Token);
                        this._credentials.AddSetting("OAuth_TokenSecret", twitterAPI.OAuth_TokenSecret);

                        // Get the user name
                        try
                        {
                            Tools.Integration.Twitter.TwitterVB2.TwitterUser user;
                            twitterAPI.AuthenticateWith(twitterConsumerKey, twitterConsumerSecret, twitterAPI.OAuth_Token, twitterAPI.OAuth_TokenSecret);
                            user = twitterAPI.AccountInformation();
                            this._credentials.AddSetting("Name", user.Name);
                            this._credentials.AddSetting("ScreenName", user.ScreenName);
                        }
                        catch (Exception ex)
                        {

                        }

                        this.SaveCredentials();

                        this.myWeb.AddResponse("Twitter.AccessTokens.Success", "User account has been successfully linked to Twitter", default, ResponseType.Hint);

                    }
                }
                else
                {

                    this.myWeb.AddResponse("Twitter.AccessTokens.Unauthorised", "The user requesting this method is not authorised", default, ResponseType.Alert);

                }
            }



            catch (Exception ex)
            {
                OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(this._moduleName, "AccessTokens(String,String)", ex, ""));
            }

        }

        public void Update()
        {
            try
            {


                string status = this.myWeb.moRequest("status");
                Update(status);
            }

            catch (Exception ex)
            {
                OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(this._moduleName, "Update(String)", ex, ""));
            }

        }
        public void Update(string status)
        {
            try
            {


                if (!this.IsAuthorisedUser)
                {
                    this.myWeb.AddResponse("Twitter.Update.Unauthorised", "The user requesting this method is not authorised", default, ResponseType.Alert);
                }

                else if (!this.IsProviderCredentialLoaded)
                {
                    this.myWeb.AddResponse("Twitter.Update.NoCredentials", "Could not load credentials for user Id" + this.DirectoryId, default, ResponseType.Alert);
                }

                else
                {
                    // Load the credentials
                    string token = this._credentials.GetSetting("OAuth_Token");
                    string secret = this._credentials.GetSetting("OAuth_TokenSecret");

                    if (string.IsNullOrEmpty(token) | string.IsNullOrEmpty(token))
                    {
                        this.myWeb.AddResponse("Twitter.Update.NullArguments", "No valid credentials are available", default, ResponseType.Alert);
                    }

                    else if (string.IsNullOrEmpty(status))
                    {
                        this.myWeb.AddResponse("Twitter.Update.NullStatus", "The status is blank", default, ResponseType.Alert);
                    }

                    else
                    {
                        var twitterAPI = new Tools.Integration.Twitter.TwitterVB2.TwitterAPI();
                        twitterAPI.AuthenticateWith(twitterConsumerKey, twitterConsumerSecret, token, secret);
                        twitterAPI.Update(status);
                    }


                }
            }

            catch (Exception ex)
            {
                OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(this._moduleName, "Update(String)", ex, ""));
                this.myWeb.AddResponse("Twitter.Update.Failed", "The update failed - see error log for more detail", default, ResponseType.Alert);
            }
        }



        public override void Post(string content, Uri trackbackUri = null, long contentId = 0L)
        {
            try
            {

                // Is there something to post
                if (!string.IsNullOrEmpty(content))
                {

                    int postLimitLength = _postLengthLimit;

                    // First shorten the URL, if it exists.
                    string shortenedURL = "";
                    if (trackbackUri is not null)
                    {
                        shortenedURL = ShortenURL(trackbackUri.ToString());
                        // If shortening has failed check the original uri
                        if (string.IsNullOrEmpty(shortenedURL))
                            shortenedURL = trackbackUri.ToString();
                        if (!string.IsNullOrEmpty(shortenedURL))
                            shortenedURL = " " + shortenedURL;
                    }

                    postLimitLength -= shortenedURL.Length;

                    // Truncate the content
                    content = Tools.Text.TruncateString(content, postLimitLength);

                    Update(content + shortenedURL);

                    this.myWeb.moDbHelper.logActivity(Cms.dbHelper.ActivityType.IntegrationTwitterPost, this.DirectoryId, 0, contentId, shortenedURL);

                }
            }

            catch (Exception ex)
            {
                OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(this._moduleName, "Post()", ex, ""));
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