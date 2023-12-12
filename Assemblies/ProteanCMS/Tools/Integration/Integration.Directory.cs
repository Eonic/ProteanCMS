using System;
using System.Data.SqlClient;
using System.Xml;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using static Protean.stdTools;
using static Protean.Tools.Number;
using static Protean.Tools.Xml;

namespace Protean.Integration.Directory
{

    /// <summary>
    /// Abstract class for Directory based integrations
    /// 
    /// </summary>
    /// <remarks>
    /// Because the actual integration may differ in how it does what it does this is implemented
    /// as an abstract class rather than an interface
    /// The common default functionality is likely to be found in the user data retrieval and checks, but the 
    /// actual integration processes - e.g. authentication are likely to be bespoke.
    /// </remarks>
    public abstract class BaseProvider : IPostable
    {

        // Core class variables
        protected string _moduleName = "Protean.Integration.Directory";
        protected Cms myWeb;
        private string _diagnostics = "";
        protected string _providerName;


        // Core class default behaviour variables
        private long _directoryId = 0L;
        private XmlElement _directoryInstance;

        // Flags
        private bool _isDirectoryItemLoaded = false;
        private bool _isProviderCredentialLoaded = false;

        protected UserCredentials _credentials;

        public new event OnErrorEventHandler OnError;

        public new delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);

        private void _OnError(object sender, Tools.Errors.ErrorEventArgs e)
        {
            _diagnostics += Constants.vbCrLf + "Error:" + e.ToString();
            OnError?.Invoke(sender, e);
        }

        public BaseProvider(ref Cms aWeb)
        {
            try
            {
                if (aWeb is null)
                    throw new ArgumentNullException("Protean.Cms is not initialised");
                myWeb = aWeb;
                _directoryId = myWeb.mnUserId;
                _moduleName += "." + Name;
                if (IsAuthorisedUser)
                    LoadCredentials();
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(_moduleName, "New(Web)", ex, ""));
            }
        }

        public BaseProvider(ref Cms aWeb, ref long directoryId)
        {
            try
            {
                if (aWeb is null)
                    throw new ArgumentNullException("Protean.Cms is not initialised");
                myWeb = aWeb;
                _directoryId = directoryId;
                _moduleName += "." + Name;
                if (IsAuthorisedUser)
                    LoadCredentials();
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(_moduleName, "New(Web,Long)", ex, ""));
            }
        }

        /// <summary>
        /// The name of the Integration
        /// </summary>
        /// <returns>String</returns>
        /// <remarks></remarks>
        private string Name
        {
            get
            {
                return GetType().Name;
            }
        }

        /// <summary>
        /// The id of the directory item that the integration is using
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public long DirectoryId
        {
            get
            {
                return _directoryId;
            }
            set
            {
                _directoryId = Conversions.ToLong(Interaction.IIf(value < 0L, 0, value));
            }
        }

        /// <summary>
        /// Checks if the Protean.Cms object has the right context to make integration calls.
        /// At the moment it should the logged in user, or someone logged in as an admin
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsAuthorisedUser
        {
            get
            {
                return myWeb.mbAdminMode | myWeb.mnUserId == _directoryId;
            }
        }

        public bool IsDirectoryItemLoaded
        {
            get
            {
                return _isDirectoryItemLoaded;
            }
        }

        public bool IsProviderCredentialLoaded
        {
            get
            {
                return _isProviderCredentialLoaded;
            }
        }

        public long ValidateExternalAuth(string ExternalId)
        {
            try
            {
                // Dim oDr As SqlClient.SqlDataReader = myWeb.moDbHelper.getDataReader("select TOP 1 nDirectoryId from tblDirectoryExternalAuth where cProviderName = '" & _providerName & "' and cProviderId='" & ExternalId & "'")
                using (SqlDataReader oDr = myWeb.moDbHelper.getDataReaderDisposable("select TOP 1 nDirectoryId from tblDirectoryExternalAuth where cProviderName = '" + _providerName + "' and cProviderId='" + ExternalId + "'"))  // Done by nita on 6/7/22
                {
                    if (oDr.HasRows)
                    {
                        while (oDr.Read())
                            return Conversions.ToLong(oDr["nDirectoryId"]);
                    }
                    else
                    {
                        return 0L;
                    }
                }
            }
            catch (Exception ex)
            {
                return 0L;
            }

            return default;
        }

        public long CreateExternalAuth(string ExternalId, long nUserId)
        {
            try
            {
                long ExtAuthId = 0L;
                long checkId = ValidateExternalAuth(ExternalId);
                if (checkId != 0L)
                {
                    return checkId;
                }
                else
                {
                    string sSql = "insert into tblDirectoryExternalAuth (nDirectoryId,cProviderName,cProviderId) VALUES (" + nUserId + ", '" + _providerName + "','" + ExternalId + "')";
                    ExtAuthId = myWeb.moDbHelper.GetIdInsertSql(sSql);
                }
                if (ExtAuthId > 0L)
                {
                    return ExtAuthId;
                }
                else
                {
                    return 0L;
                }
            }

            catch (Exception ex)
            {
                return 0L;
            }
        }

        public XmlElement GetUserSchemaXml()
        {

            var oXform = new Protean.xForm(ref myWeb.msException);
            oXform.load("/xforms/directory/User.xml", myWeb.maCommonFolders);
            return (XmlElement)oXform.Instance.SelectSingleNode("tblDirectory/cDirXml");

        }
        /// <summary>
        /// Gets the user instance and loads the provider credentials if they exist.
        /// If they don't exist then a default credentials object is created.
        /// </summary>
        /// <remarks></remarks>
        protected void LoadCredentials()
        {

            _isProviderCredentialLoaded = false;

            // Load the user
            if (LoadDirectoryInstance())
            {
                _credentials = new UserCredentials(Name, _directoryInstance);
                _isProviderCredentialLoaded = true;
            }
            else
            {
                throw new Exception("Unable to load credentials as directory instance was not loaded.");
            }

        }

        private bool LoadDirectoryInstance()
        {
            string directoryInstance;

            try
            {

                _isDirectoryItemLoaded = false;

                // Load the directory item
                directoryInstance = myWeb.moDbHelper.getObjectInstance(objectTypes.Directory, _directoryId);

                // Validate it
                if (!string.IsNullOrEmpty(directoryInstance))
                {
                    _directoryInstance = myWeb.moPageXml.CreateElement("instance");
                    _directoryInstance.InnerXml = directoryInstance;
                    _isDirectoryItemLoaded = true;
                }

                return _isDirectoryItemLoaded;
            }

            catch (Exception ex)
            {
                return false;
            }
        }

        protected void SaveCredentials()
        {
            if (!_isProviderCredentialLoaded)
            {
                throw new Exception("Unable to save credentials as no credentials were instantiated.");
            }

            else if (!LoadDirectoryInstance())
            {
                throw new Exception("Unable to save credentials as directory instance was not loaded.");
            }

            else if (_credentials.SerializeToDirectoryInstance(ref _directoryInstance))
            {
                // Save the instance.
                myWeb.moDbHelper.setObjectInstance(objectTypes.Directory, _directoryInstance, _directoryId);
            }

        }

        public void DeleteCredentials()
        {
            if (!_isProviderCredentialLoaded)
            {
                throw new Exception("Unable to delete credentials as no credentials were instantiated.");
            }
            else if (!LoadDirectoryInstance())
            {
                throw new Exception("Unable to save credentials as directory instance was not loaded.");
            }

            else if (_credentials.RemoveFromDirectoryInstance(ref _directoryInstance))
            {
                // Save the instance.
                long userid = 0L;
                string updateStatus = myWeb.moDbHelper.setObjectInstance(objectTypes.Directory, _directoryInstance, _directoryId);
                bool localCheckAndReturnStringAsNumber() { object argnumberReturn = userid; var ret = CheckAndReturnStringAsNumber(updateStatus, ref argnumberReturn, typeof(long)); userid = Conversions.ToLong(argnumberReturn); return ret; }

                if (localCheckAndReturnStringAsNumber())
                {
                    myWeb.AddResponse(Name + ".DeleteCredentials.Success", "User account has been successfully unlinked from " + Name, default, ResponseType.Hint);
                }
                else
                {
                    throw new Exception("Unable to save credentials.");
                }
            }
        }

        public virtual void Post(string content, Uri trackbackUri = null, long contentId = 0L)
        {
            OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(_moduleName, "Post", new NotSupportedException("Post method is not available for this provider:" + Name), ""));
        }

    }

    /// <summary>
    /// Helper class for integrations
    /// </summary>
    /// <remarks></remarks>
    public class Helper
    {

        // Core class variables
        protected string _moduleName = "Protean.Integration.Directory.Helper";
        protected Cms myWeb;
        private string _diagnostics = "";

        private bool _integrationsEnabled = false;

        public new event OnErrorEventHandler OnError;

        public new delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);

        private void _OnError(object sender, Tools.Errors.ErrorEventArgs e)
        {
            _diagnostics += Constants.vbCrLf + "Error:" + e.ToString();
            OnError?.Invoke(sender, e);
        }

        public Helper(ref Cms aWeb)
        {
            try
            {
                if (aWeb is null)
                    throw new ArgumentNullException("Protean.Cms is not initialised");
                myWeb = aWeb;
                _integrationsEnabled = myWeb.moConfig("UserIntegrations") == "on";
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(_moduleName, "New(Web)", ex, ""));
            }
        }

        public bool Enabled
        {
            get
            {
                return _integrationsEnabled;
            }
        }

        public bool ContentCheckboxEnabled
        {
            get
            {
                return !(myWeb.moConfig("UserIntegrationsContentCheckbox") == "off");
            }
        }

        private string ContentCheckboxContentTypes
        {
            get
            {
                string types = myWeb.moConfig("UserIntegrationsContentCheckboxTypes") + "";
                return types.Trim();
            }
        }


        /// <summary>
        /// Takes a content ID, and works out if it can post this, based on the current user id
        /// </summary>
        /// <param name="contentId">The content ID to post</param>
        /// <param name="isUpdatedContent">Indicates if the content already exists and is being updated</param>
        /// <remarks></remarks>
        public void PostContent(long contentId, bool isUpdatedContent = true)
        {

            var postableContentDocument = new XmlDocument();

            try
            {
                if (Enabled)
                {

                    // Check the user
                    if (myWeb.mnUserId > 0)
                    {

                        var userXml = postableContentDocument.CreateElement("User");
                        string userInnerXml = myWeb.moDbHelper.getObjectInstance(objectTypes.Directory, myWeb.mnUserId);
                        userXml.InnerXml = userInnerXml;

                        // Get the content brief
                        XmlElement content = myWeb.GetContentBriefXml(default, contentId);
                        string contentSchema = "";
                        Tools.Xml.NodeState(ref content, "//cContentSchemaName", "", "", 1, null, "", contentSchema, bCheckTrimmedInnerText: false);

                        // Work out whether we have permission to do this.
                        // There are two places to look for permissions
                        // Credentials/Permissions/Permission[@contentType @add @edit]
                        // where ContentType matches and add or edit is true for the isUpdatedContent
                        // Or if Request posts "overrideAutomatedIntegrationPostings" then we look at 
                        // the Request item "postToIntegrations" to see which integrations to select.
                        string integrationsSelector = "";
                        if (myWeb.moRequest("overrideAutomatedIntegrationPostings") == "true")
                        {

                            // Override the settings - search for postToIntegrations
                            foreach (string post in (myWeb.moRequest("postToIntegrations") + "").Split(","))
                            {
                                if (!string.IsNullOrEmpty(post))
                                    integrationsSelector += "@provider='" + post + "' or ";
                            }
                            if (!string.IsNullOrEmpty(integrationsSelector))
                            {
                                integrationsSelector = "[" + integrationsSelector.Substring(0, integrationsSelector.Length - 4) + "]";
                            }
                        }

                        else
                        {

                            // Match up the permissions for this scenario
                            integrationsSelector = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("[Permissions/Permission[@type='postContent' and @contentType='" + contentSchema + "' and @", Interaction.IIf(isUpdatedContent, "edit", "add")), "='true']]"));
                        }

                        // Check for the presence of Credentials
                        var credentials = userXml.SelectNodes("//Credentials" + integrationsSelector);
                        if (credentials.Count > 0 & !string.IsNullOrEmpty(integrationsSelector))
                        {

                            // Check if the content is on a page that can be viewed by non-authenticated users.
                            bool tempAdminMode = myWeb.mbAdminMode;
                            myWeb.mbAdminMode = false;

                            // Check if the content is live
                            if (myWeb.moDbHelper.IsAuditedObjectLive(objectTypes.Directory, contentId))
                            {

                                // Get the primary location
                                long primaryLocation = myWeb.moDbHelper.GetDataValue("SELECT TOP 1 nStructId FROM tblContentLocation WHERE bPrimary = 1 AND nContentId=" + contentId, default, default, 0);
                                if (primaryLocation > 0L)
                                {

                                    XmlElement siteStructureForBeginners = myWeb.GetStructureXML(0);

                                    // Check that primary location is a live page
                                    if (NodeState(ref siteStructureForBeginners, "//MenuItem[@id=" + primaryLocation + "]") != XmlNodeState.NotInstantiated)
                                    {

                                        // All checks passed
                                        // Construct a basic XML to transform
                                        // It contains the content brief and the site structure
                                        XmlElement pageElement;
                                        XmlElement contentsElement;
                                        postableContentDocument.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                                        pageElement = postableContentDocument.CreateElement("Page");
                                        postableContentDocument.AppendChild(pageElement);
                                        pageElement.AppendChild(postableContentDocument.ImportNode(siteStructureForBeginners, true));
                                        contentsElement = postableContentDocument.CreateElement("Contents");
                                        pageElement.AppendChild(contentsElement);
                                        if (!string.IsNullOrEmpty(Cms.gcEwBaseUrl))
                                            pageElement.SetAttribute("baseUrl", Cms.gcEwBaseUrl);
                                        myWeb.GetRequestVariablesXml(pageElement);

                                        contentsElement.AppendChild(postableContentDocument.ImportNode(content, true));

                                        // Set the style file
                                        // TODO: AG Set a common integration
                                        string styleFile = "";
                                        if (System.IO.File.Exists(goServer.MapPath("/xsl/integrations/post.xsl")))
                                        {
                                            styleFile = goServer.MapPath("/xsl/integrations/post.xsl");
                                        }
                                        else
                                        {
                                            styleFile = goServer.MapPath("/ewcommon/xsl/integrations/post.xsl");

                                        }

                                        // Add all the credentials to the Page
                                        foreach (XmlElement credential in credentials)
                                            pageElement.AppendChild(postableContentDocument.ImportNode(credential, true));

                                        // Transform the xml into postable content via xsl, which returns xml
                                        // The xsl should pick up every credential (provider) and produce an  
                                        // appropriately formatted response for that provider.
                                        // The format is as follows:
                                        // <post provider="provider" url="url to content">content to post</post>
                                        var textWriter = new System.IO.StringWriter();
                                        var oTransform = new Protean.XmlHelper.Transform(ref myWeb, styleFile, false);
                                        oTransform.mbDebug = gbDebug;
                                        System.IO.TextWriter argoWriter = textWriter;
                                        oTransform.ProcessTimed(postableContentDocument, ref argoWriter);
                                        textWriter = (System.IO.StringWriter)argoWriter;
                                        oTransform.Close();
                                        oTransform = (Protean.XmlHelper.Transform)null;

                                        // Load the response.
                                        var response = new XmlDocument();
                                        response.LoadXml(textWriter.ToString());

                                        // iterate through the providers
                                        // only post those with content.
                                        string integrationBase = "Protean.Integration.Directory.";

                                        foreach (XmlElement post in response.SelectNodes("/posts/post"))
                                        {

                                            // Check the post content and URI validity
                                            if (post.InnerXml.Trim().Length > 0 && Uri.IsWellFormedUriString(post.GetAttribute("url"), UriKind.Absolute) && !string.IsNullOrEmpty(post.GetAttribute("provider")))


                                            {

                                                // Everything's good, let's try to post it.
                                                object[] constructorArguments = new object[] { myWeb, Convert.ToInt64(myWeb.mnUserId) };
                                                object[] methodArguments = new object[] { post.InnerXml.Trim(), new Uri(post.GetAttribute("url"), UriKind.Absolute), Convert.ToInt64(contentId) };
                                                Invoke.InvokeObjectMethod(integrationBase + post.GetAttribute("provider") + ".Post", constructorArguments, methodArguments, this, "_OnError", "OnError");

                                            }

                                        }

                                    }

                                }


                            }

                            // Important - revert back to the previous setting
                            myWeb.mbAdminMode = tempAdminMode;

                        }

                    }

                }
            }

            catch (Exception ex)
            {
                OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(_moduleName, "PostContent", ex, ""));
            }
        }

        /// <summary>
        /// Takes a content xForm and works out if it needs to add the checkboxes or alerts for the current user 
        /// </summary>
        /// <param name="form"></param>
        /// <remarks></remarks>
        public void PostContentCheckboxes(ref Global.Protean.Cms.Admin.AdminXforms form, string contentSchema, bool isContentBeingUpdated)
        {

            string message = "";

            try
            {

                if (Enabled && ContentCheckboxEnabled)
                {

                    // Get the user XML
                    XmlElement userXml = form.moXformElmt.OwnerDocument.CreateElement("User");
                    userXml.InnerXml = myWeb.moDbHelper.getObjectInstance(objectTypes.Directory, myWeb.mnUserId);

                    // Only continue if there are credentials
                    var credentials = userXml.SelectNodes("//Credentials");
                    if (credentials.Count > 0)
                    {

                        // Create a group to add all this to
                        XmlElement group = form.moXformElmt.OwnerDocument.CreateElement("group");
                        group.SetAttribute("class", "postContentToIntegrations");
                        addElement(ref group, "label", "Share Content");

                        // First search for automatic content postings for the content type
                        string automaticPostingsXPath = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("//Credentials[Permissions/Permission[@contentType='" + contentSchema + "' and @", Interaction.IIf(isContentBeingUpdated, "edit", "add")), "='true']]"));
                        var automaticPostingProviders = userXml.SelectNodes(automaticPostingsXPath);
                        string postingProviders = "";
                        if (automaticPostingProviders.Count > 0)
                        {

                            bool isFirst = true;
                            foreach (XmlElement credential in automaticPostingProviders)
                            {
                                if (isFirst)
                                {
                                    isFirst = false;
                                }
                                else
                                {
                                    postingProviders += ",";
                                }
                                postingProviders += credential.GetAttribute("provider");
                            }

                            message = "<p>According to your preferences, this content will be automatically posted to the following: <span id=\"autopost\">" + postingProviders + "</span>. </p>";

                        }

                        // Check if this content schema is postable
                        // By default look if the permission exists (not if it's set up for automated posting),
                        // but also check for an overrided list from the config
                        if (Array.IndexOf(ContentCheckboxContentTypes.Trim().ToLower().Split(','), contentSchema.ToLower()) > 0 || string.IsNullOrEmpty(ContentCheckboxContentTypes.Trim()) & userXml.SelectNodes("//Credentials[Permissions/Permission[@contentType='" + contentSchema + "']]").Count > 0)
                        {

                            XmlElement providersSelect = form.addSelect(group, "postToIntegrations", false, "", "checkboxes", Protean.xForm.ApperanceTypes.Full);
                            addElement(ref providersSelect, "value", postingProviders);
                            foreach (XmlElement provider in userXml.SelectNodes("//Credentials"))


                                form.addOption(providersSelect, "Share to " + provider.GetAttribute("provider"), provider.GetAttribute("provider"));

                            // Set the message
                            message = "<div msg-id=\"2311\"><p>You can share this content. </p>" + message + "<p>To share (or opt not to share) this content, please tick/untick the appropriate boxes below.</p></div>";
                            form.addNote(group, Protean.xForm.noteTypes.Hint, message, true);

                            XmlElement @override = form.addInput(group, "overrideAutomatedIntegrationPostings", false, "", "hidden");
                            addElement(ref @override, "value", "true");


                            XmlElement submission = form.moXformElmt.SelectSingleNode("//submit[not(following::submit)]");
                            submission.ParentNode.InsertBefore(group, submission);


                        }




                    }

                }
            }

            catch (Exception ex)
            {
                OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(_moduleName, "PostContentCheckboxes", ex, ""));
            }
        }
    }



    public interface IPostable
    {
        void Post(string content, Uri trackbackUrl = null, long contentId = 0L);
    }
}