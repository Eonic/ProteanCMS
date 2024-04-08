using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Xml;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using static Protean.stdTools;

namespace Protean
{


    public class xForm
    {

        public System.Web.HttpContext moCtx;

        public System.Web.HttpApplicationState goApp;
        public System.Web.HttpRequest goRequest;
        public System.Web.HttpResponse goResponse;
        public System.Web.SessionState.HttpSessionState goSession;
        public System.Web.HttpServerUtility goServer;

        public static string msException;

        private bool bValid = false;
        private string cValidationError = "";
        private string[] _formParameters = null;

        public string FormName = "form";

        public XmlDocument moPageXML = new XmlDocument();
        public XmlElement moXformElmt;
        public XmlElement model;
        private XmlElement oInstance;
        private XmlElement oInitialInstance;
        public long mnUserId;

        protected string cLanguage = "";

        public XmlDocument result;
        public bool bProcessRepeats = true;

        public System.Collections.Specialized.StringCollection BindsToSkip = new System.Collections.Specialized.StringCollection();

        public string SubmittedRef;

        private const string mcModuleName = "Protean.Cms.xForm";
        private bool bTriggered = false;
        private bool bDeleted = false;

        private bool _stopHackyLocalPathCheck = false;

        private string _preLoadedInstanceXml = "";

        private string[] sNoteTypes = new string[] { "help", "hint", "alert" };
        private string[] sBindAttributes = new string[] { "calculate", "constraint", "readonly", "relevant", "required", "type" };
       // private object this;

        public enum noteTypes
        {
            Help = 0,
            Hint = 1,
            Alert = 2
        }

        public enum ApperanceTypes
        {
            Full = 0,
            Minimal = 1
        }

        public enum BindAttributes
        {
            Calculate = 0,
            Constraint = 1,
            ReadOnlyBind = 2,
            Relevant = 3,
            Required = 4,
            Type = 5
        }

        public bool valid
        {
            get
            {
                return bValid;
            }
            set
            {
                bValid = value;
            }
        }

        public XmlElement Instance
        {
            get
            {
                return oInstance;
            }
            set
            {
                LoadInstance(value);

            }
        }

        public bool isTriggered
        {
            get
            {
                return bTriggered;
            }
            set
            {
                bTriggered = value;
            }
        }

        public XmlElement RootGroup
        {
            get
            {
                XmlElement oRootGroup = null;
                try
                {
                    if (Tools.Xml.NodeState(ref moXformElmt, "group") == Tools.Xml.XmlNodeState.NotInstantiated)
                    {

                        oRootGroup = null;
                    }
                    else
                    {
                        oRootGroup = (XmlElement)moXformElmt.SelectSingleNode("group");
                    }
                }
                catch (Exception ex)
                {
                    returnException(ref msException, mcModuleName, "RootGroup-xmlElement", ex, "", "", gbDebug);
                }
                return oRootGroup;
            }
        }

        public string[] FormParameters
        {
            get
            {
                return _formParameters;
            }
            set
            {
                _formParameters = value;
            }
        }

        public string PreLoadedInstanceXml
        {
            get
            {
                return _preLoadedInstanceXml;
            }
            set
            {
                _preLoadedInstanceXml = value;
            }
        }

        public string validationError
        {
            get
            {
                return cValidationError;
            }
        }


        public xForm(ref string sException) : this(System.Web.HttpContext.Current, ref sException)
        {

        }

        public xForm(System.Web.HttpContext Context, ref string sException)
        {

            string sProcessInfo = string.Empty;
            try
            {
                msException = sException;
                moCtx = Context;

                if (moCtx != null)
                {
                    goApp = moCtx.Application;
                    goRequest = moCtx.Request;
                    goResponse = moCtx.Response;
                    goSession = moCtx.Session;
                    goServer = moCtx.Server;
                }
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "New", ex, "", "", gbDebug);
            }
        }



        public void LoadInstance(XmlDocument xml)
        {
            try
            {
    // add instance to model from xmlDocument
                oInstance = xml.DocumentElement;

                if (oInitialInstance is null)
                {
                    oInitialInstance = (XmlElement)oInstance.Clone();
                }

                processRepeats(ref moXformElmt);
            }
            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "Loadinstance-xmlElement", ex, "", "", gbDebug);
            }


        }

        public void LoadInstance(XmlElement oElmt, bool resetInitial = false)
        {
            string cProcessInfo = "";

            try
            {
                // add instance to model from xmlDocument
                if (oInstance is null)
                {
                    oInstance = oElmt;
                }
                else
                {
                    oInstance.InnerXml = oElmt.InnerXml;
                }

                if (oInitialInstance is null | resetInitial == true)
                {
                    oInitialInstance = (XmlElement)oInstance.Clone();
                }

                processRepeats(ref moXformElmt);
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "Loadinstance-xmlElement", ex, "", cProcessInfo, gbDebug);
            }
        }

        public void LoadInstanceFromInnerXml(string xml)
        {
            // add instance to model from xmlDocument
            oInstance.InnerXml = xml;
            Instance = oInstance;
            // load(oInstance, bProcessRepeats)
            // If oInitialInstance Is Nothing Then
            // oInitialInstance = oInstance.Clone()
            // End If
            // processRepeats(moXformElmt)
        }

        public void LoadInstance(string path)
        {
            // add instance to model from file path
        }

        public void updateInstance(XmlElement oElmt)
        {
            string cProcessInfo = "";

            try
            {

                // Dim oSb As Text.StringBuilder = New Text.StringBuilder

                // Dim oWriter As Xml.XmlTextWriter = New Xml.XmlTextWriter(New StringWriter(oSb))

                // 'step through all nodes and attributes from the provided data and populate the instance allready specified.
                // 'using microsoft DiffPatch Tool.

                // Dim oDiff As Microsoft.XmlDiffPatch.XmlDiff = New Microsoft.XmlDiffPatch.XmlDiff
                // oDiff.Options = Microsoft.XmlDiffPatch.XmlDiffOptions.IgnorePI Or Microsoft.XmlDiffPatch.XmlDiffOptions.IgnoreComments

                // oDiff.Compare(instance, oElmt, oWriter)
                // Dim oXml As XmlDocument = New XmlDocument
                // Dim oNode As XmlElement

                // oXml.LoadXml(oSb.ToString)
                // Dim nsMgr As XmlNamespaceManager = getNsMgr(oXml.DocumentElement, oXml)
                // nsMgr.AddNamespace("xd", "http://schemas.microsoft.com/xmltools/2002/xmldiff")

                // 'step through and delete the remove instructions
                // For Each oNode In oXml.DocumentElement.SelectNodes("descendant-or-self::xd:remove", nsMgr)
                // oNode.ParentNode.RemoveChild(oNode)
                // Next

                // Dim oReader As Xml.XmlTextReader = New XmlTextReader(New StringReader(oXml.OuterXml))

                // Dim oPatch As Microsoft.XmlDiffPatch.XmlPatch = New Microsoft.XmlDiffPatch.XmlPatch
                // oPatch.Patch(instance, oReader)

                // oDiff = Nothing
                // oReader = Nothing
                // oPatch = Nothing
                // After all this we have an elaborate copy!

                // Dim ds As DataSet = New DataSet
                // Dim ds2 As DataSet = New DataSet
                // Dim oReader As Xml.XmlTextReader = New XmlTextReader(New StringReader(instance.InnerXml))
                // Dim oReader2 As Xml.XmlTextReader = New XmlTextReader(New StringReader(oElmt.InnerXml))
                // ds.ReadXml(oReader)
                // ds2.ReadXml(oReader2)
                // ds.Merge(ds2)
                // instance.InnerXml = ds.GetXml

                Instance = oElmt;
            }


            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "Patchinstance-xmlElement", ex, "", cProcessInfo, gbDebug);
            }
        }


        public void NewFrm(string sName = "")
        {
            XmlElement oFrmElmt;
            string cProcessInfo = "";
            try
            {
                oFrmElmt = moPageXML.CreateElement("Content");
                oFrmElmt.SetAttribute("type", "xform");
                if (!string.IsNullOrEmpty(sName))
                {
                    oFrmElmt.SetAttribute("name", sName);
                }
                model = moPageXML.CreateElement("model");
                oInstance = moPageXML.CreateElement("instance");
                model.AppendChild(oInstance);
                oFrmElmt.AppendChild(model);
                moXformElmt = oFrmElmt;
            }
            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "NewFrm", ex, "", cProcessInfo, gbDebug);
            }
        }

        public void submission(string id, string action, string method, string submitEvent = "")
        {
            XmlElement oSubElmt;
            string cProcessInfo = "";
            try
            {
                // If moXformElmt Is Nothing Then Me.NewFrm()
                if (moPageXML.SelectSingleNode("submission[@id='" + id + "']") is null)
                {
                    oSubElmt = moPageXML.CreateElement("submission");
                    oSubElmt.SetAttribute("id", id);
                    // TS change because instance not a child of model for some reason
                    model.InsertAfter(oSubElmt, model.SelectSingleNode("instance"));
                }
                // model.InsertAfter(oSubElmt, oInstance)
                else
                {
                    oSubElmt = (XmlElement)moPageXML.SelectSingleNode("submission[@id='" + id + "']");
                }

                oSubElmt.SetAttribute("action", action);
                oSubElmt.SetAttribute("method", method);
                oSubElmt.SetAttribute("event", submitEvent);
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "submission", ex, "", cProcessInfo, gbDebug);
            }
        }

        /// <summary>
        /// Loads an xform from a filepath
        /// </summary>
        /// <param name="cXformPath">The filepath relative to Server.MapPath</param>
        /// <returns>
        /// <para>Boolean</para>
        /// <list>
        /// <item>True - if the file exists and can be loaded into the xform</item>
        /// <item>False - if the file does not exist, or an error is encountered while loading the file.</item>
        /// </list></returns>
        /// <remarks></remarks>
        public virtual bool load(string cXformPath)
        {

            bool fileExists = false;
            bool returnValue = false;
            XmlElement oFrmElmt;
            var oXformDoc = new XmlDocument();
            string cProcessInfo = "Loading..." + cXformPath;

            try
            {
                // Test for existence of the filepath
                fileExists = File.Exists(goServer.MapPath(cXformPath));

                // oXformDoc.PreserveWhitespace = True

                // If no joy, then test for the filepath trimmed of leading /
                // Is this here to account for local path errors, or double slashes maybe?
                // AG note - I don't understand why this is here, it mollycoddles bad web.config inputs.
                // and prevents the client folder check working if the site is running from ewcommon say.
                // so I have added the variable _stopHackyLocalPathCheck to avoid if alternative path checks exists 
                if (!fileExists && cXformPath.StartsWith("/") && cXformPath.Length > 2 && !_stopHackyLocalPathCheck)


                {
                    cXformPath = cXformPath.Substring(1);
                    fileExists = File.Exists(goServer.MapPath(cXformPath));
                }

                // If we have found a file then try to load it into the xform
                if (fileExists)
                {
                    oXformDoc.Load(goServer.MapPath(cXformPath));
                    FormName = oXformDoc.DocumentElement.GetAttribute("name");
                    oFrmElmt = moPageXML.CreateElement("Content");
                    oFrmElmt.SetAttribute("type", "xform");
                    oFrmElmt.SetAttribute("name", FormName);
                    oFrmElmt.InnerXml = oXformDoc.DocumentElement.InnerXml;

                    moXformElmt = oFrmElmt;
                    processFormParameters();

                    // set the model and instance
                    model = (XmlElement)moXformElmt.SelectSingleNode("model");

                    Instance = (XmlElement)model.SelectSingleNode("instance");

                    // Not used at the moment - intended for repeats where the instance needs to be loaded pre load PreLoadInstance()
                    // processRepeats(moXformElmt)
                    if (bProcessRepeats & goSession != null)
                    {

                        if (goSession[$"tempInstance-{FormName}"] is null)
                        {
                            Instance = (XmlElement)model.SelectSingleNode("descendant-or-self::instance");
                        }
                        else
                        {
                            Instance = (XmlElement)goSession[$"tempInstance-{FormName}"];
                        }

                        if (isTriggered)
                        {
                            // we have clicked a trigger so we must update the instance
                            updateInstanceFromRequest();
                            // lets save the instance
                            goSession[$"tempInstance-{FormName}"] = Instance;
                        }
                        else
                        {
                            // This has moved into validate as we must ensure valid form prior to removal
                            // goSession("tempInstance") = Nothing

                        }
                    }

                    else
                    {
                        oInstance = (XmlElement)model.SelectSingleNode("descendant-or-self::instance");

                        // XformInclude Features....
                    }

                    System.Collections.Specialized.NameValueCollection moThemeConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/theme");
                    string currentTheme = "";
                    if (moThemeConfig != null)
                    {
                        currentTheme = moThemeConfig["CurrentTheme"];
                    }


                    foreach (XmlElement oInc in moXformElmt.SelectNodes("descendant-or-self::ewInclude"))
                    {

                        string filepath = oInc.GetAttribute("filePath").Replace("ewthemes", "ewthemes/" + currentTheme);
                        string xpath = oInc.GetAttribute("xPath");
                        var LoadDoc = new XmlDocument();
                        if (File.Exists(goServer.MapPath(filepath)))
                        {
                            LoadDoc.Load(goServer.MapPath(filepath));
                            var newElmt = moXformElmt.OwnerDocument.CreateElement("new");
                            if (LoadDoc.SelectSingleNode(xpath) != null)
                            {
                                newElmt.InnerXml = LoadDoc.SelectSingleNode(xpath).OuterXml;
                                oInc.ParentNode.ReplaceChild(newElmt.FirstChild, oInc);
                            }
                            else
                            {
                                oInc.InnerText = "xPath Not Found";
                            }
                        }
                        else if (filepath.Contains("ewthemes"))
                        {
                            oInc.ParentNode.RemoveChild(oInc);
                        }
                        else
                        {
                            oInc.InnerText = "File Not Found";
                        }
                    }

                    returnValue = true;
                }





                // Return the value
                return returnValue;
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "load", ex, "", cProcessInfo, gbDebug);
                // If any problems are encountered return false.
                // Return False
            }

            return default;
        }

        /// <summary>
        /// Loads an xform from a filepath, while checking some alternative paths
        /// </summary>
        /// <param name="xformPath">Is the path of the xform to be loaded e.g. /myxform.xml</param>
        /// <param name="AlternativePrefixFolders">Is a string arrary of possible prefixed folders to try out e.g. {"/test1","/test2"} would try /test1/myxform.xml etc. Note that this sets the preferred order to try out.  If a match is made, then no other folders are tried.</param>
        /// <returns>
        /// <para>Boolean</para>
        /// <list>
        /// <item>True - if the file was found either natively or in the modified prefix paths</item>
        /// <item>False - if the file was not found, or an error was encountered while loading the file.</item>
        /// </list></returns>
        /// <remarks></remarks>
        public bool load(string xformPath, string[] AlternativePrefixFolders)
        {

            string filePath = "";
            bool returnValue = false;
            try
            {

                if (AlternativePrefixFolders.Length > 1)
                    _stopHackyLocalPathCheck = true;

                // Test for local path
                filePath = xformPath;
                returnValue = load(filePath);

                // If no local path, then run through the Alternative in order
                if (!returnValue)
                {

                    // Tidy up the path for prefixing
                    xformPath = "/" + xformPath.TrimStart(@"/\".ToCharArray());


                    foreach (string AltFolder in AlternativePrefixFolders)
                    {
                        if (!string.IsNullOrEmpty(AltFolder))
                        {
                            filePath = AltFolder.TrimEnd(@"/\".ToCharArray()) + xformPath;
                            returnValue = load(filePath);

                            // If anything loads then let's get out of here
                            if (returnValue)
                                break;
                        }
                    }
                }

                return returnValue;
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "load(String,String())", ex, "", filePath, gbDebug);
                return false;
            }
        }


        public void load(ref XmlNode oNode, bool bWithRepeats = false)
        {
            string cProcessInfo = "";
            XmlElement oElmt;
            try
            {
                // loads both model and xform from a string of xml
                oElmt = (XmlElement)oNode;
                load(ref oElmt, bWithRepeats);
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "load", ex, "", cProcessInfo, gbDebug);
            }
        }

        public virtual void load(ref XmlElement oElmt, bool bWithRepeats = false)
        {
            string cProcessInfo = "";
            // Boolean to determine if the XML has been loaded from a file
            // Dim bXmlLoad As Boolean = False
            try
            {


                bProcessRepeats = bWithRepeats;

                // If SRC Node exists, then use that xform instead
                if (oElmt.SelectSingleNode("descendant-or-self::model") != null)
                {
                    if (oElmt.SelectSingleNode("descendant-or-self::model").Attributes.GetNamedItem("src") != null)
                    {
                        string cFilePath = oElmt.SelectSingleNode("descendant-or-self::model").Attributes.GetNamedItem("src").Value.ToString();
                        if (load(cFilePath))
                        {
                            // bXmlLoad = True
                            oElmt.InnerXml = moXformElmt.InnerXml;
                        }
                    }
                }


                // Moved Lower Section into this for now as is it needed, doesn't the file load path method do it?
                // Moved back again
                // If Not bXmlLoad Then
                moXformElmt = oElmt;
                // set the model and instance
                model = (XmlElement)moXformElmt.SelectSingleNode("descendant-or-self::model");
                if (model != null)
                {
                    if (bWithRepeats & goSession != null)
                    {

                        if (goSession[$"tempInstance-{FormName}"] is null)
                        {
                            Instance = (XmlElement)model.SelectSingleNode("descendant-or-self::instance");
                        }
                        else
                        {
                            Instance = (XmlElement)goSession[$"tempInstance-{FormName}"];
                        }

                        if (isTriggered)
                        {
                            // we have clicked a trigger so we must update the instance
                            updateInstanceFromRequest();
                            // lets save the instance
                            goSession[$"tempInstance-{FormName}"] = Instance;
                        }
                        else
                        {
                            // This has moved into validate as we must ensure valid form prior to removal
                            // goSession("tempInstance") = Nothing

                        }
                    }

                    else
                    {
                        oInstance = (XmlElement)model.SelectSingleNode("descendant-or-self::instance");
                    }

                }
            }
            // End If


            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "load", ex, "", cProcessInfo, gbDebug);
            }
        }

        public void loadtext(string cXform)
        {
            string cProcessInfo = "";
            XmlElement oFrmElmt;
            var oXformDoc = new XmlDocument();

            try
            {
                // loads both model and xform from a string of xml

                oXformDoc.Load(new StringReader(cXform));
                oFrmElmt = moPageXML.CreateElement("Content");
                oFrmElmt.InnerXml = oXformDoc.DocumentElement.OuterXml;
                moXformElmt = (XmlElement)oFrmElmt.FirstChild;


                // set the model and instance
                model = (XmlElement)moXformElmt.SelectSingleNode("model");
                Instance = (XmlElement)model.SelectSingleNode("instance");
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "submission", ex, "", cProcessInfo, gbDebug);
            }
        }

        public void validate()
        {

            bool bIsValid = true;
            bool bIsThisBindValid = true;
            XmlElement oBindElmt = null;
            string sAttribute;
            XmlElement updateElmt;
            // Dim sBind As String
            // Dim cNode As String
            // Dim cNsURI As String
            string sXpath;
            string sXpathNoAtt = "";
            System.Xml.XPath.XPathNodeIterator obj;
            object objValue;
            bool missedError = false;

            string cProcessInfo = "";
            try
            {

                // validates an html form submission against a bind requirements
                // updates xform or loads result.
                // get our bind node

                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

                var argoNode = oInstance.SelectSingleNode("*[1]");
                var nsMgr = Tools.Xml.getNsMgrRecursive(ref argoNode, ref moPageXML);

                // HANDLING FOR GOOGLE ReCAPTCHA
                if (moXformElmt.SelectSingleNode("descendant-or-self::*[contains(@class,'recaptcha') and not(ancestor::instance)]") != null)
                {
                    cValidationError = "<span class=\"msg-1032\">Please confirm you are not a robot</span>";
                    bIsValid = false;
                    missedError = true;
                }
                if (isSubmitted() == true)
                {
                    if (!string.IsNullOrEmpty(goRequest["g-recaptcha-response"]))
                    {

                        System.Collections.Specialized.NameValueCollection moConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");
                        var recap = new Tools.RecaptchaV2.Recaptcha();
                        var recapResult = recap.Validate(goRequest["g-recaptcha-response"], moConfig["ReCaptchaKeySecret"]);

                        if (Conversions.ToBoolean(Operators.OrObject(recapResult.Succeeded, Operators.ConditionalCompareObjectEqual(goSession["recaptcha"], 1, false))))
                        {
                            cValidationError = "";
                            bIsValid = true;
                            goSession["recaptcha"] = 1;
                            missedError = false;
                        }

                    }
                }

                // END HANDLING FOR GOOGLE ReCAPTCHA
                // 


                // lets get all the binds but check that they don't occur in the instance
                foreach (XmlNode oBindNode in model.SelectNodes("descendant-or-self::bind[not(ancestor::instance)]"))
                {
                    oBindElmt = (XmlElement)oBindNode;
                    sXpath = "";
                    bIsThisBindValid = true;
                    string sMessage = "";
                    // NB REMOVE THIS
                    if (oBindElmt.GetAttribute("type") == "fileUpload")
                    {
                        string DELETE = string.Empty;
                    }
                    // NB

                    var xPathNav = oInstance.CreateNavigator();
                    var xPathNav2 = oInstance.CreateNavigator();
                    System.Xml.XPath.XPathExpression expr;
                    sAttribute = "";
                    if (oBindElmt.SelectSingleNode("@nodeset") != null)
                    {

                        // if we are an attribute then get the parent xpath
                        if (Strings.InStr(oBindElmt.GetAttribute("nodeset"), "@") == 1)
                        {
                            sAttribute = Strings.Right(oBindElmt.GetAttribute("nodeset"), Strings.Len(oBindElmt.GetAttribute("nodeset")) - 1);
                        }

                        sXpathNoAtt = getBindXpath(ref oBindElmt);
                        sXpathNoAtt = Tools.Xml.addNsToXpath(sXpathNoAtt, ref nsMgr);
                        if (!string.IsNullOrEmpty(sAttribute))
                        {
                            sXpath = sXpathNoAtt + "/@" + sAttribute;
                        }
                        else
                        {
                            sXpath = sXpathNoAtt;
                        }

                        cProcessInfo = "Error Processing:" + sXpath;

                        // Get the current object value

                        expr = xPathNav.Compile(sXpath);
                        expr.SetContext(nsMgr);
                        obj = xPathNav.Select(expr);
                        obj.MoveNext();

                        if (Strings.LCase(obj.Current.Name) == "instance")
                        {
                            // object not found so we returns root instance we don't want all nodes returned in a dirty long string
                            objValue = "";
                        }
                        else
                        {
                            objValue = obj.Current.Value;
                        }
                    }
                    else
                    {
                        // nodeset has not been specified so value = submitted value
                        objValue = goRequest[oBindElmt.GetAttribute("id")];
                    }

                    // Evaulate the value by type
                    string cExtensions = oBindElmt.GetAttribute("allowedExt");

                    // NB : 23-01-2009 To support file upload checks
                    if (oBindElmt.GetAttribute("type") == "fileUpload")
                    {
                        if (string.IsNullOrEmpty(cExtensions))
                        {
                            cExtensions = "doc,docx,xls,xlsx,pdf,ppt,jpg,gif,png";
                        }
                    }
                    //oBindElmt.GetAttribute("type") <> "" And(oBindElmt.GetAttribute("required") = "true()" And objValue<> "")

                    if (oBindElmt.GetAttribute("type") != "" && (oBindElmt.GetAttribute("required") == "true()" && objValue.ToString() !=""))
                    {
                        sMessage = evaluateByType(Conversions.ToString(objValue), oBindElmt.GetAttribute("type"), cExtensions, Strings.LCase(oBindElmt.GetAttribute("required")) == "true()");
                    }
                    string labelText = oBindElmt.GetAttribute("id");
                    XmlElement oIptElmt = (XmlElement)moXformElmt.SelectSingleNode("descendant-or-self::*[@ref='" + oBindElmt.GetAttribute("id") + "' or @bind='" + oBindElmt.GetAttribute("id") + "']");
                    XmlElement inputLabel = null;
                    if (oIptElmt != null)
                    {
                        inputLabel = (XmlElement)oIptElmt.SelectSingleNode("label");
                        if (inputLabel != null)
                        {
                            labelText = oIptElmt.SelectSingleNode("label").InnerText;
                        }
                    }


                    if (!string.IsNullOrEmpty(sMessage))
                    {
                        bIsValid = false;
                        bIsThisBindValid = false;
                        if (addNoteFromBind(oBindElmt, noteTypes.Alert, BindAttributes.Type, sMessage) == false)
                        {
                            missedError = true;
                        }
                        cValidationError += "<span>" + labelText + " - " + sMessage + "</span>";
                    }

                    // case for calculate
                    if (oBindElmt.GetAttribute("calculate") != "" & bIsThisBindValid)
                    {

                        // Get the current object value
                        expr = xPathNav2.Compile(oBindElmt.GetAttribute("calculate"));
                        expr.SetContext(nsMgr);
                        string sValue2 = Conversions.ToString(xPathNav2.Evaluate(expr));

                        if (!string.IsNullOrEmpty(sAttribute))
                        {
                            updateElmt = (XmlElement)oInstance.SelectSingleNode(sXpathNoAtt, nsMgr);
                            updateElmt.SetAttribute(sAttribute, sValue2);
                        }
                        else if (!string.IsNullOrEmpty(sXpath) && oInstance.SelectSingleNode(sXpath) != null)
                        {
                            oInstance.SelectSingleNode(sXpath).InnerText = sValue2;
                            objValue = sValue2;
                        }
                    }

                    // case for required 
                    if (oBindElmt.GetAttribute("required") != "" & bIsThisBindValid)
                    {

                        cProcessInfo = cProcessInfo + " - Required Compile Error: " + oBindElmt.GetAttribute("required");
                        expr = xPathNav2.Compile(oBindElmt.GetAttribute("required"));

                        cProcessInfo = cProcessInfo + " - Required Expression Error: " + oBindElmt.GetAttribute("required");
                        expr.SetContext(nsMgr);

                        // Looking for true() or false()
                        if (Conversions.ToBoolean(xPathNav2.Evaluate(expr)))
                        {

                            // Look for data
                            if (objValue.ToString() == "")
                            {
                                // No data - error message
                                bIsValid = false;
                                bIsThisBindValid = false;
                                string sRef = oBindElmt.GetAttribute("id");
                                string validationMsg = "";
                                if (moXformElmt.SelectSingleNode("descendant-or-self::*[(@ref='" + sRef + "' or @bind='" + sRef + "') and not(@class='hidden')]/@validationMsg") != null) {
                                    validationMsg = moXformElmt.SelectSingleNode("descendant-or-self::*[(@ref='" + sRef + "' or @bind='" + sRef + "') and not(@class='hidden')]/@validationMsg").InnerText;
                                }
                                if (validationMsg != "") {
                                    if (moXformElmt.SelectSingleNode("descendant-or-self::*[(@ref='" + sRef + "' or @bind='" + sRef + "') and not(@class='hidden')]/label") != null)                                    
                                    {
                                        validationMsg = moXformElmt.SelectSingleNode("descendant-or-self::*[(@ref='" + sRef + "' or @bind='" + sRef + "') and not(@class='hidden')]/label").InnerText;
                                        if (validationMsg == "")
                                        {
                                            string innerHtml = moXformElmt.SelectSingleNode("descendant-or-self::*[(@ref='" + sRef + "' or @bind='" + sRef + "') and not(@class='hidden')]/label").InnerXml;
                                            validationMsg = $"<span class=\"term4053\">Please Enter</span>&#160;{innerHtml}";
                                        }
                                        else {
                                            validationMsg = $"<span class=\"term4053\">Please Enter</span>&#160;{validationMsg}";
                                        }
                                    }
                                }
                                if (addNoteFromBind(oBindElmt, noteTypes.Alert, BindAttributes.Required, "<span class=\"msg-1007\">" + validationMsg + " </span>") == false)
                                {
                                    missedError = true;
                                }
                                if (cValidationError.Contains(validationMsg) != true)
                                {
                                    cValidationError += validationMsg;
                                }
                            }
                        }
                    }

                    // case for constraint
                    if (oBindElmt.GetAttribute("constraint") != "" & bIsThisBindValid)
                    {

                        cProcessInfo = cProcessInfo + " - Constraint Compile Error: " + oBindElmt.GetAttribute("constraint");
                        string constraintXpath = Tools.Xml.addNsToXpath(oBindElmt.GetAttribute("constraint"), ref nsMgr);
                        // Dim constraintXpath As String = oBindElmt.GetAttribute("constraint")
                        expr = xPathNav2.Compile(constraintXpath);

                        cProcessInfo = cProcessInfo + " - Constraint Context Error: " + oBindElmt.GetAttribute("constraint");

                        expr.SetContext(nsMgr);

                        // Evaluate the constraint
                        if (Convert.ToBoolean(xPathNav2.Evaluate(expr)) == false)
                        {
                            // Constraint not met
                            bIsValid = false;
                            bIsThisBindValid = false;
                            string thisValidationError = "<span class=\"msg-1008\">This information must be valid</span>";

                            if (oBindElmt.SelectSingleNode("alert") != null)
                            {
                                thisValidationError = "<span>" + oBindElmt.SelectSingleNode("alert").InnerText + "</span>";
                            }

                            if (!string.IsNullOrEmpty(oBindElmt.GetAttribute("alert")))
                            {
                                thisValidationError = "<span>" + oBindElmt.GetAttribute("alert") + "</span>";
                            }

                            if (addNoteFromBind(oBindElmt, noteTypes.Alert, BindAttributes.Constraint, thisValidationError))
                            {
                                missedError = true;
                            }
                            cValidationError += "<span class=\"msg-1035\"><span class=\"labelName\">" + labelText + "</span> - " + thisValidationError + "</span>";

                        }
                    }



                    if (!string.IsNullOrEmpty(oBindElmt.GetAttribute("unique")) & bIsThisBindValid)
                    {
                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(objValue, "", false)))
                        {
                            if (isUnique(Conversions.ToString(objValue), oBindElmt.GetAttribute("unique")))
                            {
                            }
                            else
                            {
                                bIsValid = false;
                                bIsThisBindValid = false;
                                if (addNoteFromBind(oBindElmt, noteTypes.Alert, BindAttributes.Constraint, "<span class=\"msg-1008\">This must be unique</span>"))
                                {
                                    missedError = true;
                                }
                                cValidationError += "<span class=\"msg-1034\"><span class=\"labelName\">" + labelText + "</span> - This must be unique</span>";
                            }
                        }

                    }

                    // case for relevant
                    // case for read-only


                }


                // Validate any fileUpload binds that have a maximum file size specified against them.
                // note that "number(NODE)=number(NODE)" is an XPath test to see if NODE is a number
                foreach (XmlElement oFileCheck in model.SelectNodes("descendant-or-self::bind[not(ancestor::instance) and @type='fileUpload' and number(@maxSize)=number(@maxSize)]"))
                {
                    cProcessInfo = "Checking maximum file sizes";

                    XmlElement oIptElmt = (XmlElement)moXformElmt.SelectSingleNode("descendant-or-self::*[@ref='" + oBindElmt.GetAttribute("id") + "' or @bind='" + oBindElmt.GetAttribute("id") + "']");
                    string labelText = oBindElmt.GetAttribute("id");
                    XmlElement inputLabel = (XmlElement)oIptElmt.SelectSingleNode("label");
                    if (inputLabel != null)
                    {
                        labelText = oIptElmt.SelectSingleNode("label").InnerText;
                    }

                    // Check that the bind exists in the uploaded files
                    if (!string.IsNullOrEmpty(oFileCheck.GetAttribute("id")) && goRequest.Files[oFileCheck.GetAttribute("id")] != null)
                    {

                        // maxSize has been found and an item for that bind has been submitted.
                        // Compare the sizes.
                        if (goRequest.Files[oFileCheck.GetAttribute("id")].ContentLength > Conversions.ToInteger(oFileCheck.GetAttribute("maxSize")) * 1024)
                        {
                            if (oFileCheck is null)
                            {
                                missedError = true;
                            }
                            else
                            {
                                addNoteFromBind(oFileCheck, noteTypes.Alert, BindAttributes.Constraint, "<span class=\"msg-1009\">The file you are uploading is too large</span>");
                            }
                            bIsValid = false;
                            bIsThisBindValid = false;
                            cValidationError += "<span class=\"msg-1033\"><span class=\"labelName\">" + labelText + "</span> -  The file you are uploading is too large</span>";
                        }

                    }

                }

                if (bIsValid & bProcessRepeats)
                {
                    goSession[$"tempInstance-{FormName}"] = null;
                }

                if (!string.IsNullOrEmpty(cValidationError) & missedError)
                {
                    // the 2nd node should be group first should be the first group
                    XmlElement lastGroup = (XmlElement)moXformElmt.SelectSingleNode("*[2]/*[last()]");

                    if (lastGroup != null)
                    {
                        if (lastGroup.Name == "submit")
                        {
                            var argoNode1 = lastGroup.ParentNode;
                            addNote(ref argoNode1, noteTypes.Alert, cValidationError);
                        }
                        else
                        {
                            //XmlNode argoNode2 = lastGroup;
                            addNote(ref lastGroup, noteTypes.Alert, cValidationError);
                            //lastGroup = (XmlElement)argoNode2;
                        }
                    }

                }

                if (bIsValid & goSession != null)
                    goSession["formFileUploaded"] = null;

                oInstance.SetAttribute("valid", bIsValid.ToString().ToLower());

                valid = bIsValid;
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "validate", ex, "", cProcessInfo, gbDebug);
            }

        }

        public virtual string evaluateByType(string sValue, string sType, string cExtensions = "", bool isRequired = false)
        {
            string cProcessInfo = "";
            string cReturn = ""; // Set this as a clear return string

            try
            {
                // Only evaulate if there is data to evaluate against!
                if (!string.IsNullOrEmpty(sValue))
                {
                    switch (Strings.LCase(sType) ?? "")
                    {
                        case "float":
                        case "number":
                            {
                                // IsNumeric is a bit rubbish
                                if (!Tools.Number.IsReallyNumeric(sValue + ""))
                                    cReturn = "<span class=\"msg-1000\">This must be a number</span>";
                                break;
                            }
                        case "date":
                            {
                                if (!Information.IsDate(sValue))
                                    cReturn = "<span class=\"msg-1001\">This must be a valid date</span>";
                                break;
                            }
                        case "email":
                            {
                                if (!Tools.Text.IsEmail(sValue))
                                    cReturn = "<span class=\"msg-1002\">This must be a valid email address</span>";
                                break;
                            }
                        case "emails":
                            {
                                if (sValue.Contains(","))
                                {
                                    foreach (var sEmail in sValue.Split(','))
                                    {
                                        if (!Tools.Text.IsEmail(sEmail.Trim()))
                                            cReturn = "<span class=\"msg-1002a\">These all must be a valid email address</span>";
                                    }
                                }
                                else if (!Tools.Text.IsEmail(sValue))
                                    cReturn = "<span class=\"msg-1002\">This must be a valid email address</span>";
                                break;
                            }

                        case "imgverification":
                            {
                                if (Conversions.ToBoolean(!Operators.ConditionalCompareObjectEqual(Strings.LCase(sValue),(goSession["imgVerification"]), false)))
                                    cReturn = "<span class=\"msg-1003\">Please enter the correct letters and numbers as shown.</span>";
                                break;
                            }
                        case "strongpassword":
                            {
                                if (!strongPassword(sValue + ""))
                                    cReturn = "<span class=\"msg-1005\">This password must be stronger</span>";
                                break;
                            }
                        case "fileupload":
                            {
                                cProcessInfo = sValue;

                                string cExtension = Path.GetExtension(sValue);
                                if (!cExtensions.Contains(Strings.Right(cExtension, 3)))
                                    cReturn = "<span class=\"msg-1004\">Invalid File Extension</span>";
                                break;
                            }

                        default:
                            {
                                // RegExp hack: allow the user to specify a dynamic format for type
                                if (sType.StartsWith("format:"))
                                {
                                    // Extract the regexp and compare it.
                                    string cPattern = "";
                                    cPattern = Strings.Mid(sType, 8);
                                    if (!string.IsNullOrEmpty(cPattern))
                                    {
                                        try
                                        {
                                            // In case it's an invalid Regexp don't catch errors
                                            var oRe = new Regex(cPattern, RegexOptions.IgnoreCase);
                                            if (!oRe.IsMatch(sValue))
                                                cReturn = "<span class=\"msg-1005\">This must be a valid format</span>";
                                        }
                                        catch (Exception)
                                        {
                                            // Do Nothing
                                        }
                                    }
                                }

                                break;
                            }
                    }
                }
                else if (sType == "fileUpload" & isRequired)
                {
                    // Only show this if the file upload is required
                    if (goRequest.Files.Count > 0)
                        cReturn = "<span class=\"msg-1006\">No File Selected</span>";
                }
                return cReturn;
            }
            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "evaluateByType", ex, "", cProcessInfo, gbDebug);
                return "";
            }
        }


        public virtual bool isUnique(string sValue, string sPath)
        {
            string cProcessInfo = "";
            // Placeholder for overide
            try
            {
                return true;
            }
            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "isUnique", ex, "", cProcessInfo, gbDebug);
                return Conversions.ToBoolean("");
            }
        }

        public bool addNoteFromBind(XmlElement oBindElmt, noteTypes oNoteType, BindAttributes oBindType, string sDefaultMessage)
        {
            string sRef = oBindElmt.GetAttribute("id");
            string cProcessInfo = "error with field - " + sRef;
            // Dim sMessage As String
            try
            {

                if (moXformElmt.SelectSingleNode("descendant-or-self::*[(@ref='" + sRef + "' or @bind='" + sRef + "') and not(@class='hidden')]") != null)
                {
                    // Look for a child node in the bind element that has a matching note type and bind attribute type
                    string sXPath = sNoteTypes[(int)oNoteType] + "[@type='" + sBindAttributes[(int)oBindType] + "']";
                    XmlElement oElmt = (XmlElement)oBindElmt.SelectSingleNode(sXPath);

                    // If there's no match then check for a generic note (i.e. one without a type specified)
                    if (oElmt is null)
                        oElmt = (XmlElement)oBindElmt.SelectSingleNode(sNoteTypes[(int)oNoteType] + "[not(@type)]");

                    // Override the default message
                    if (oElmt != null)
                    {
                        sDefaultMessage = oElmt.InnerText;
                    }

                    // Add the note
                    addNote(oBindElmt.GetAttribute("id"), oNoteType, sDefaultMessage);
                    if (moXformElmt.SelectSingleNode("descendant-or-self::*[(@ref='" + sRef + "' or @bind='" + sRef + "') and not(@class='hidden')]") is null)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "addNoteFromBind", ex, "", cProcessInfo, gbDebug);
                return false;
            }

        }

        public void updateInstanceFromRequest()
        {
            // Dim sItem As String
            // Dim xPath As String
            // Dim value As String
            XmlNodeList oNodes;
            XmlElement oElmt;
            XmlElement oinstanceElmt;
            string sXpath;
            string sBind;
            string sRequest;
            XmlElement oBindElmt;
            string sAttribute;
            string sValue= string.Empty;
            bool bIsXml;
            string cProcessInfo = "";
            string sDataType;

            try
            {

                // scan each form item

                // add the soap namespace to the nametable of the xmlDocument to allow xpath to query the namespace
                var argoNode = oInstance.SelectSingleNode("*[1]");
                var nsMgr = Tools.Xml.getNsMgrRecursive(ref argoNode, ref moPageXML);

                oNodes = moXformElmt.SelectNodes("descendant::*[(@ref or @bind) and not(ancestor::model or ancestor::trigger or self::group or self::repeat)]");

                foreach (XmlNode oNode in oNodes)
                {
                    bIsXml = false;
                    oElmt = (XmlElement)oNode;
                    sAttribute = "";

                    // Readonly Textarea need to treat any value as Xml
                    if (oElmt.Name == "textarea" & oElmt.GetAttribute("class") == "readonly")
                        bIsXml = true;
                    if (oElmt.Name == "textarea" & Strings.InStr(oElmt.GetAttribute("class"), "xhtml") > 0)
                        bIsXml = true;
                    if (oElmt.Name == "textarea" & Strings.InStr(oElmt.GetAttribute("class"), "xml") > 0)
                        bIsXml = true;
                    if (oElmt.Name == "textarea" & Strings.InStr(oElmt.GetAttribute("class"), "xsledit") > 0)
                        bIsXml = true;
                    if (oElmt.Name == "input" & Strings.InStr(oElmt.GetAttribute("class"), "pickImage") > 0)
                        bIsXml = true;

                    // if the ref contains the instance xpath
                    sXpath = oElmt.GetAttribute("ref");
                    // unfinished, enter code for handling attribute paths
                    sRequest = sXpath;

                    // if not then there should be a binding
                    if (!!string.IsNullOrEmpty(sXpath))
                    {

                        sBind = oElmt.GetAttribute("bind");

                        // check for binds added in processRepeats

                        if (BindsToSkip.Contains(sBind))
                        {

                            cProcessInfo = "Skipped Bind " + sBind;
                        }
                        else
                        {

                            sRequest = sBind;
                            try
                            {
                                // get our bind node
                                foreach (XmlElement oBindNode in model.SelectNodes("descendant-or-self::bind[@id='" + sBind + "']"))
                                {
                                    oBindElmt = oBindNode;
                                    sDataType = oBindElmt.GetAttribute("type");
                                    string submittedValue = "" + goRequest[sRequest];

                                    // If we haven't specified a nodeset we are validating the value but not updating the instance.
                                    if (oBindElmt.SelectSingleNode("@nodeset") != null)
                                    {
                                        // if we are an attribute then get the parent xpath
                                        if (Strings.InStr(oBindElmt.GetAttribute("nodeset"), "@") == 1)
                                        {
                                            sAttribute = Strings.Right(oBindElmt.GetAttribute("nodeset"), Strings.Len(oBindElmt.GetAttribute("nodeset")) - 1);

                                        }
                                        sXpath = getBindXpath(ref oBindElmt);
                                        sXpath = Tools.Xml.addNsToXpath(sXpath, ref nsMgr);

                                        // Allow the submitted value to substitue in the Xpath

                                        // first we need to reset the other values
                                        if (Strings.InStr(sXpath, "$submittedValue") > 0)
                                        {
                                            string falseValue = oBindNode.GetAttribute("falseValue");
                                            if (string.IsNullOrEmpty(falseValue))
                                                falseValue = "false";
                                            foreach (XmlElement optionNode in oElmt.SelectNodes("item/value"))
                                            {
                                                string optionXPath = sXpath.Replace("$submittedValue", optionNode.InnerText);
                                                if (oInstance.SelectSingleNode(optionXPath, nsMgr) != null)
                                                {
                                                    oInstance.SelectSingleNode(optionXPath, nsMgr).InnerText = falseValue;
                                                }
                                            }

                                            // set the actual submitted value Xpath and update the submitted value with the option selected.
                                            sXpath = sXpath.Replace("$submittedValue", submittedValue);
                                            submittedValue = oBindNode.GetAttribute("value");
                                        }



                                        if (string.IsNullOrEmpty(sXpath))
                                            sXpath = ".";
                                        cProcessInfo = "sXPath:" + sXpath + " value:" + submittedValue;
                                        // update for each bind element match
                                        if (oInstance.SelectSingleNode(sXpath, nsMgr) is null)
                                        {
                                            sValue = "invalid path";
                                            cValidationError += cValidationError + "<p>The following xpath could not be located in the instance: " + sXpath + "</p>";
                                        }
                                        else if (!string.IsNullOrEmpty(sAttribute))
                                        {
                                            oinstanceElmt = (XmlElement)oInstance.SelectSingleNode(sXpath, nsMgr);
                                            oinstanceElmt.SetAttribute(sAttribute, submittedValue);
                                        }
                                        else
                                        {
                                            switch (sDataType ?? "")
                                            {

                                                case "xml-replace":
                                                    {
                                                        XmlElement oElmtTemp;
                                                        oElmtTemp = moPageXML.CreateElement("Temp");
                                                        if (submittedValue is null)
                                                        {
                                                            cProcessInfo = sRequest;
                                                        }
                                                        else if (string.IsNullOrEmpty(submittedValue))
                                                        {
                                                            oInstance.SelectSingleNode(sXpath, nsMgr).ParentNode.RemoveChild(oInstance.SelectSingleNode(sXpath, nsMgr));
                                                        }
                                                        else
                                                        {
                                                            try
                                                            {
                                                                oElmtTemp.InnerXml = (Tools.Xml.convertEntitiesToCodes(submittedValue) + "").Trim();
                                                            }
                                                            catch
                                                            {
                                                                oElmtTemp.InnerXml = Tools.Text.tidyXhtmlFrag((Tools.Xml.convertEntitiesToCodes(submittedValue) + "").Trim());
                                                            }
                                                            oInstance.SelectSingleNode(sXpath, nsMgr).ParentNode.ReplaceChild(oElmtTemp.FirstChild.Clone(), oInstance.SelectSingleNode(sXpath, nsMgr));
                                                        }
                                                        oElmtTemp = null;
                                                        break;
                                                    }

                                                case "xml-replace-img":
                                                    {

                                                        // Specific behaviour for an image tag.

                                                        XmlElement oElmtTemp;
                                                        oElmtTemp = moPageXML.CreateElement("Temp");
                                                        if (submittedValue is null)
                                                        {
                                                            cProcessInfo = sRequest;
                                                        }
                                                        else if (string.IsNullOrEmpty(submittedValue))
                                                        {
                                                            oInstance.SelectSingleNode(sXpath, nsMgr).ParentNode.RemoveChild(oInstance.SelectSingleNode(sXpath, nsMgr));
                                                        }
                                                        else
                                                        {
                                                            oElmtTemp.InnerXml = (Tools.Xml.convertEntitiesToCodes(submittedValue) + "").Trim();
                                                            oInstance.SelectSingleNode(sXpath, nsMgr).ParentNode.ReplaceChild(oElmtTemp.FirstChild.Clone(), oInstance.SelectSingleNode(sXpath, nsMgr));
                                                        }
                                                        oElmtTemp = null;
                                                        break;
                                                    }

                                                case "image":
                                                    {
                                                        // Submitted Image File - not image HTML
                                                        XmlElement argimgElmt = (XmlElement)oInstance.SelectSingleNode(sXpath, nsMgr);
                                                        var tmp = goRequest.Files;
                                                        var argpostedFile = tmp[sRequest];
                                                        updateImageElement(ref argimgElmt, ref oBindElmt, ref argpostedFile);
                                                        break;
                                                    }

                                                case "base64Binary":
                                                    {
                                                        XmlElement oElmtFile;
                                                        oElmtFile = (XmlElement)oInstance.SelectSingleNode(sXpath, nsMgr);

                                                        var oElmtFileStream = moPageXML.CreateElement(oElmtFile.Name + "Stream");
                                                        oElmtFile.ParentNode.InsertAfter(oElmtFileStream, oElmtFile);

                                                        System.Web.HttpPostedFile fUpld;
                                                        fUpld = goRequest.Files[sRequest];

                                                        oElmtFile.InnerText = Tools.Text.filenameFromPath(fUpld.FileName);

                                                        var body = fUpld.InputStream;

                                                        // body.Read(bodyBytes, 0, fUpld.ContentLength - 1)
                                                        // 
                                                        var encoding = System.Text.Encoding.Default;
                                                        var reader = new StreamReader(body);
                                                        byte[] bodyBytes = new System.Text.UTF8Encoding().GetBytes(reader.ReadToEnd());

                                                        oElmtFileStream.InnerText = Convert.ToBase64String(bodyBytes);
                                                        break;
                                                    }
                                                case "base64Binary_Leave":
                                                    {
                                                        // Do nothing, this is left to another part of the program to use
                                                        // all we will do is pput the name of the file in

                                                        oInstance.SelectSingleNode(sXpath, nsMgr).InnerXml = Tools.Xml.convertEntitiesToCodes(Tools.Text.filenameFromPath(goRequest.Files[sRequest].FileName) + "").Trim();
                                                        break;
                                                    }

                                                case "fileUpload":
                                                    {
                                                        string cSavePath = oBindElmt.GetAttribute("saveTo");
                                                        string cExtensions = oBindElmt.GetAttribute("allowedExt");
                                                        bool bAddTimeStamp = oBindElmt.GetAttribute("timeStamp") == "true";
                                                        string Filename;


                                                        if (string.IsNullOrEmpty(cExtensions))
                                                        {
                                                            cExtensions = "doc,docx,xls,xlsx,pdf,ppt,jpg,gif,png";
                                                        }

                                                        if (goRequest.Files.Count > 0 && !(goRequest.Files.Count == 1 & goRequest.Files[0].ContentLength == 0))
                                                        {
                                                            var oFile = goRequest.Files[0];

                                                            cSavePath = cSavePath.Replace("$userId$", mnUserId.ToString());
                                                            cSavePath = cSavePath.Replace("$id$", goRequest["id"]);
                                                            cSavePath.Replace("/", @"\");
                                                            if (!cSavePath.EndsWith(@"\"))
                                                            {
                                                                cSavePath += @"\";
                                                            }

                                                            if (bAddTimeStamp)
                                                            {
                                                                Filename = Path.GetFileNameWithoutExtension(oFile.FileName) + DateTime.Now.ToString("yyyyMMddhhmmss") + "." + Path.GetExtension(oFile.FileName);
                                                            }
                                                            else
                                                            {
                                                                Filename = Path.GetFileName(oFile.FileName);
                                                            }

                                                            bool upload = false;
                                                            if (!string.IsNullOrEmpty(cSavePath))
                                                            {
                                                                string cExtension = Path.GetExtension(Filename);
                                                                if (cExtensions.Contains(Strings.Right(cExtension, 3)))
                                                                {
                                                                    upload = true;
                                                                }
                                                                else
                                                                {
                                                                    sValue = "invalid file extension";
                                                                    cValidationError += cValidationError + "<p>Invalid File Extension: " + cExtension + "</p>";
                                                                    // NB Added to test bad extensions
                                                                    oInstance.SelectSingleNode(sXpath, nsMgr).InnerText = Filename;
                                                                }
                                                                // Else
                                                                // Why would we try to upload nothing?
                                                                // upload = True
                                                            }

                                                            if (upload)
                                                            {
                                                                // Need a way to check directory exists, if not then create it
                                                                string cFullPath = goServer.MapPath("/").TrimEnd(@"/\".ToCharArray());

                                                                // ensure the folder exists
                                                                var oFs = new Protean.fsHelper();
                                                                oFs.mcStartFolder = cFullPath;
                                                                cValidationError += oFs.CreatePath(cSavePath);
                                                                if (cValidationError == "1")
                                                                    cValidationError = "";

                                                                if (Directory.Exists(cFullPath + cSavePath) == true)
                                                                {

                                                                    string cFinalFullSavePath = oFs.getUniqueFilename(cFullPath + cSavePath + Filename);
                                                                    oFile.SaveAs(cFinalFullSavePath);
                                                                    oInstance.SelectSingleNode(sXpath, nsMgr).InnerText = cFinalFullSavePath.Replace(cFullPath, "");

                                                                    // Working on the assumption that only one file has been submitted, then store this in a session object
                                                                    if (goSession != null)
                                                                        goSession["formFileUploaded"] = cFinalFullSavePath.Replace(cFullPath, "");
                                                                }
                                                                else
                                                                {
                                                                    sValue = "invalid save path";
                                                                    cValidationError += cValidationError + "<p>Save Path does not exist: " + cFullPath + cSavePath + "</p>";
                                                                }

                                                            }
                                                        }

                                                        // No Files

                                                        // First check if there's a value in the session variable, which would indicate that 
                                                        // this has been uploaded but the form had to go through a couple of stages of validation
                                                        else if (goSession != null && !string.IsNullOrEmpty(Conversions.ToString(goSession["formFileUploaded"])))
                                                        {
                                                            oInstance.SelectSingleNode(sXpath, nsMgr).InnerText = Conversions.ToString(goSession["formFileUploaded"].ToString().Trim());
                                                        }
                                                        else
                                                        {
                                                            sValue = "no files attached";
                                                            cValidationError += cValidationError + "<p>No Files Have Been Attached For Upload</p>";


                                                        }

                                                        break;
                                                    }
                                                case "datetime":
                                                    {
                                                        oInstance.SelectSingleNode(sXpath, nsMgr).InnerXml = Tools.Xml.XmlDate(oInstance.SelectSingleNode(sXpath, nsMgr).InnerXml, true);
                                                        break;
                                                    }
                                                case "date":
                                                    {
                                                        oInstance.SelectSingleNode(sXpath, nsMgr).InnerXml =Protean.Tools.Xml.XmlDate(oInstance.SelectSingleNode(sXpath, nsMgr).InnerXml);
                                                        break;
                                                    }
                                                case "string-before-comma":
                                                    {
                                                        // pull the first value in an array and populate the instance
                                                        XmlElement oElmtTemp;
                                                        oElmtTemp = moPageXML.CreateElement("Temp");
                                                        if (Strings.InStr(1, submittedValue, ",") > 0)
                                                        {
                                                            string cFirstPath = Strings.Left(submittedValue, Strings.InStr(submittedValue, ","));
                                                            cFirstPath = cFirstPath.TrimEnd(',');
                                                            oElmtTemp.InnerXml = (Tools.Xml.convertEntitiesToCodes(cFirstPath) + "").Trim();
                                                            // oInstance.SelectSingleNode(sXpath, nsMgr).ParentNode.ReplaceChild(oElmtTemp.FirstChild.Clone, oInstance.SelectSingleNode(sXpath, nsMgr))
                                                            oInstance.SelectSingleNode(sXpath, nsMgr).InnerText = oElmtTemp.InnerXml;
                                                        }
                                                        else
                                                        {
                                                            oInstance.SelectSingleNode(sXpath, nsMgr).InnerText = submittedValue.Trim();
                                                        }

                                                        break;
                                                    }

                                                default:
                                                    {
                                                        // If goRequest(sRequest) <> "" Then "This is removed because we need to clear empty checkbox forms"
                                                        if (bIsXml)
                                                        {
                                                            if (!string.IsNullOrEmpty(submittedValue))
                                                            {
                                                                oInstance.SelectSingleNode(sXpath, nsMgr).InnerXml = Tools.Xml.convertEntitiesToCodes(submittedValue + "").Trim();
                                                            }
                                                            else
                                                            {
                                                                oInstance.SelectSingleNode(sXpath, nsMgr).InnerXml = "";
                                                            }
                                                        }
                                                        else
                                                        {
                                                            // take the form value over the querystring.
                                                            oInstance.SelectSingleNode(sXpath, nsMgr).InnerText = submittedValue.Trim();
                                                        }

                                                        break;
                                                    }
                                            }

                                        }
                                    }
                                }
                            }
                            catch (Exception ex2)
                            {
                                // no bind element found, do nothing
                                returnException(ref msException, mcModuleName, "updateInstanceFromRequest", ex2, "", cProcessInfo, gbDebug);
                            }
                        }
                    }
                    // We have an xpath to the instance, lets update the node
                    else if (oInstance.SelectSingleNode(sXpath, nsMgr) is null)
                    {
                        sValue = "invalid path";
                        if (sXpath != "submit")
                        {
                            // cValidationError = cValidationError & "<p>The following xpath could not be located in the instance: " & sXpath & "</p>"
                        }
                    }
                    else if (!string.IsNullOrEmpty(sAttribute))
                    {
                        oinstanceElmt = (XmlElement)oInstance.SelectSingleNode(sXpath, nsMgr);
                        oinstanceElmt.SetAttribute(sAttribute, (goRequest[sRequest] + "").Trim());
                    }
                    else
                    {
                        oInstance.SelectSingleNode(sXpath, nsMgr).InnerText = (goRequest[sRequest] + "").Trim();
                    }

                }

                if (!string.IsNullOrEmpty(cValidationError))
                {
                    // the 2nd node should be group first should be the first group
                    XmlElement firstGroup = (XmlElement)moXformElmt.SelectSingleNode("*[2]");

                    if (firstGroup is null)
                    {
                        //XmlNode argoNode1 = firstGroup;
                        addNote(ref firstGroup, noteTypes.Alert, cValidationError);
                        //firstGroup = (XmlElement)argoNode1;
                    }

                    valid = false;
                }
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "updateInstanceFromRequest", ex, "", cProcessInfo, gbDebug);
            }
        }

        public string getRequest(string sRequest)
        {

            string submittedValue = goRequest.Form[sRequest] + "";
            if (string.IsNullOrEmpty(submittedValue) & !string.IsNullOrEmpty(goRequest.QueryString[sRequest]))
            {
                submittedValue = goRequest.QueryString[sRequest];
            }
            return submittedValue;

        }





        public void updateInstanceFromRequestOnly()
        {
            // Dim sItem As String
            // Dim xPath As String
            // Dim value As String
            XmlElement oinstanceElmt;
            string sXpath;
            string sBind;
            XmlElement oBindElmt;
            string sAttribute = "";
            string sValue= string.Empty;
            string cProcessInfo = "";

            try
            {

                // add the soap namespace to the nametable of the xmlDocument to allow xpath to query the namespace
                var argoNode = oInstance.FirstChild;
                var nsMgr = Protean.Tools.Xml.getNsMgr(ref argoNode, ref moPageXML);

                // scan each form item
                foreach (var item in goRequest.Form)
                {
                    sBind = Conversions.ToString(item);

                    foreach (XmlElement oBindNode in model.SelectNodes("descendant-or-self::bind[@id='" + sBind + "']"))
                    {
                        oBindElmt = oBindNode;
                        // if we are an attribute then get the parent xpath
                        if (Strings.InStr(oBindElmt.GetAttribute("nodeset"), "@") == 1)
                        {
                            sAttribute = Strings.Right(oBindElmt.GetAttribute("nodeset"), Strings.Len(oBindElmt.GetAttribute("nodeset")) - 1);
                        }
                        sXpath = getBindXpath(ref oBindElmt);
                        sXpath = Tools.Xml.addNsToXpath(sXpath, ref nsMgr);

                        // update for each bind element match
                        if (oInstance.SelectSingleNode(sXpath, nsMgr) is null)
                        {
                            sValue = "invalid path";
                            cValidationError = cValidationError + "<p>The following xpath could not be located in the instance: " + sXpath + "</p>";
                        }
                        else if (!string.IsNullOrEmpty(sAttribute))
                        {
                            oinstanceElmt = (XmlElement)oInstance.SelectSingleNode(sXpath, nsMgr);
                            oinstanceElmt.SetAttribute(sAttribute, (goRequest[Conversions.ToString(item)] + "").Trim());
                        }
                        else
                        {
                            oInstance.SelectSingleNode(sXpath, nsMgr).InnerText = (goRequest[Conversions.ToString(item)] + "").Trim();
                        }
                    }
                }

                if (!string.IsNullOrEmpty(cValidationError))
                {
                    //XmlNode argoNode1 = moXformElmt;
                    addNote(ref moXformElmt, noteTypes.Alert, cValidationError);
                    //moXformElmt = (XmlElement)argoNode1;
                    valid = false;
                }
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "updateInstanceFromRequest", ex, "", cProcessInfo, gbDebug);
            }
        }

        /// <summary>
        ///     <para>Uploads a submitted image and adds the details to the xform</para>
        /// <list>
        /// <listheader>Bind node attribute for ewImageHandler are as follows:</listheader>
        /// <item>fileName - name of the file</item>
        /// <item>filePath - where to save the path</item>
        /// <item>uniqueFileName - when true, this will append a tiemstamp to the filename, hopefully making it unique.</item>
        /// <item>maxwidth - maximum width of the image</item>
        /// <item>maxheight - maximum height of the image</item>
        /// <item>crop - when resizing, crop image to the dimensions required rather than stretching it</item>
        /// <item>noStretch - when true, prevents images from being upscaled when dimensions are smaller than maxwidth or maxheight</item>
        /// <item>quality - compression level (0-100)</item>
        /// </list>
        /// </summary>
        /// <param name="imgElmt">The element in the xform instance to add references to</param>
        /// <param name="bindElmt">The bind element</param>
        /// <param name="postedFile">The file uploaded</param>
        /// <remarks></remarks>
        private void updateImageElement(ref XmlElement imgElmt, ref XmlElement bindElmt, ref System.Web.HttpPostedFile postedFile)
        {

            // TS To Do

            long maxWidth = 0L;
            long maxHeight = 0L;
            string filePath = @"\images";
            string fileName = "";
            bool makeFileNameUnique = false;
            string newFileName = "";
            string cProcessInfo = "";
            string cIsCrop = "";
            string cNoStretch = "";
            long nQuality = 0L;
            string savedImgPath = string.Empty;
            string alreadyUploaded = "";

            string imgPath = "";
            string imgWidth = "";
            string imgHeight = "";


            try
            {

                if (!string.IsNullOrEmpty(postedFile.FileName))
                {

                    XmlElement eonicImgElmt = (XmlElement)bindElmt.SelectSingleNode("ewImageHandler");

                    alreadyUploaded = eonicImgElmt.GetAttribute("alreadyUploaded");


                    // Avoid duplicate uploads
                    if (string.IsNullOrEmpty(alreadyUploaded))
                    {


                        var oFs = new Protean.fsHelper();

                        fileName = postedFile.FileName;
                        fileName = Strings.Right(fileName, fileName.Length - fileName.LastIndexOf(@"\") - 1);
                        fileName = Strings.Replace(fileName, " ", "-");

                        // lets load the settings from the bind node

                        // this sits in the bind node
                        // <ewImageHandler fileName="profile_tn.jpg" filePath="/images/profiles/$userId$" maxwidth="50" maxheight="50"/>
                        string idPath = eonicImgElmt.GetAttribute("idPath");
                        string uniqueId = "";
                        if (!string.IsNullOrEmpty(idPath))
                        {
                            uniqueId = bindElmt.SelectSingleNode("ancestor::model/instance/" + idPath).InnerText + "";
                        }

                        maxWidth = Conversions.ToLong("0" + eonicImgElmt.GetAttribute("maxwidth"));
                        maxHeight = Conversions.ToLong("0" + eonicImgElmt.GetAttribute("maxheight"));
                        cIsCrop = eonicImgElmt.GetAttribute("crop");
                        cNoStretch = eonicImgElmt.GetAttribute("noStretch");
                        newFileName = eonicImgElmt.GetAttribute("fileName");
                        filePath = eonicImgElmt.GetAttribute("filePath").Replace("$userId$", mnUserId.ToString());
                        if (!string.IsNullOrEmpty(uniqueId))
                            filePath = filePath.Replace("$id$", uniqueId);
                        nQuality = Conversions.ToLong("0" + eonicImgElmt.GetAttribute("quality"));
                        makeFileNameUnique = eonicImgElmt.GetAttribute("uniqueFileName").ToLower() == "true";


                        // first lets save our file

                        oFs.initialiseVariables(Protean.fsHelper.LibraryType.Image);

                        cProcessInfo = oFs.CreatePath(filePath);

                        if (cProcessInfo != "1")
                        {
                            Information.Err().Raise(1009, "updateImageElement", "EonicWeb Filesystem: you don't have permissions to write to " + filePath);
                        }

                        cProcessInfo = oFs.SaveFile(ref postedFile, filePath);

                        // now lets load the origonal into our image helper
                        var oIh = new Tools.Image(Protean.fsHelper.URLToPath(goServer.MapPath(@"\images") + filePath + @"\" + fileName));
                        // now we have loaded it into our image object we can delete it
                        oFs.DeleteFile(filePath, fileName);

                        // If Not (maxWidth = 0 Or maxHeight = 0) Then
                        if (cIsCrop == "true")
                        {
                            oIh.IsCrop = true;
                        }
                        if (cNoStretch == "true")
                        {
                            oIh.NoStretch = true;
                        }

                        // we always keep the aspect ratio
                        oIh.KeepXYRelation = true;

                        oIh.SetMaxSize((int)maxWidth, (int)maxHeight);
                        // and save


                        // If duplicate filename, then update the fileName
                        if (makeFileNameUnique && File.Exists(Protean.fsHelper.URLToPath(goServer.MapPath(@"\images") + filePath + @"\" + newFileName)))
                        {
                            // Create a new filename
                            newFileName = newFileName.Substring(0, newFileName.LastIndexOf(".")) + "-" + DateTime.Now.ToString("yyyyMMddhhmmss") + newFileName.Substring(newFileName.LastIndexOf("."));
                        }

                        string newFilePath = Protean.fsHelper.URLToPath(goServer.MapPath(@"\images") + filePath + @"\" + newFileName);

                        // now lets set the image element variables
                        imgPath = Protean.fsHelper.PathToURL(@"\images" + filePath + @"\" + newFileName);
                        imgWidth = oIh.Width.ToString();
                        imgHeight = oIh.Height.ToString();

                        if (nQuality > 0L)
                        {
                            oIh.Save(newFilePath, (int)nQuality);
                        }
                        else
                        {
                            oIh.Save(newFilePath);
                        }

                        // To avoid duplicate uploads for duplicate bind elements, check for ewImagehandlers that match ids
                        // By setting attributes we can pass these on to other handlers later on in processing 
                        foreach (XmlElement imgHandler in model.SelectNodes("//bind[@id='" + bindElmt.GetAttribute("id") + "' and ewImageHander/@maxwidth='" + maxWidth + "'  and ewImageHander/@maxheight='" + maxHeight + "']/ewImageHandler"))
                        {
                            imgHandler.SetAttribute("alreadyUploaded", imgPath);
                            imgHandler.SetAttribute("actualWidth", imgWidth);
                            imgHandler.SetAttribute("actualHeight", imgHeight);
                        }
                    }
                    else
                    {
                        // now lets set the image element variables
                        imgPath = eonicImgElmt.GetAttribute("alreadyUploaded");
                        imgWidth = eonicImgElmt.GetAttribute("actualWidth");
                        imgHeight = eonicImgElmt.GetAttribute("actualHeight");

                    }

                    imgElmt.SetAttribute("src", imgPath);
                    imgElmt.SetAttribute("width", imgWidth);
                    imgElmt.SetAttribute("height", imgHeight);

                }
            }


            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "updateImageElement", ex, "", cProcessInfo, gbDebug);
            }

        }

        public void addValues()
        {
            // takes the values from the instance and adds them as an attribute
            // This is non standard for XForm Specification but gets around the inability of xslt to evaluate dynamic xpath references 
            // This should no longer be requried in XPath2 or once xforms is available in standard browsers.

            // for this reason we are also unable to handle repeating elements in XSLT
            // We therefore use this to itterate through any repeat elements, 
            // look at the lines in the instance, and replicate using a group elements instead,
            // creating the equivenlent binds


            XmlNodeList oNodes;
            XmlElement oElmt;
            XmlElement oValElmt;
            XmlElement oinstanceElmt;
            string sXpath;
            string sBind;
            XmlElement oBindNode;
            XmlElement oBindElmt;
            string sAttribute;
            string sValue = "";
            bool bIsXml;
            bool bReadOnly = false;
            string cProcessInfo = "";
            string sDataType = "";
            string cleanXpath = "";

            try
            {

                oNodes = moXformElmt.SelectNodes("descendant::*[(@ref or @bind) and not(ancestor::label or ancestor::model or self::group or self::repeat or self::trigger or self::delete)]");


                // if the instance is empty we have no values to add.
                if (oInstance is null)
                    return;

                if (oInstance.SelectSingleNode("*[1]") is null)
                    return;

                var argoNode = oInstance.SelectSingleNode("*[1]");
                var nsMgr = Tools.Xml.getNsMgrRecursive(ref argoNode, ref moPageXML);

                foreach (XmlNode oNode in oNodes)
                {
                    bIsXml = false;
                    oElmt = (XmlElement)oNode;
                    sAttribute = "";
                    //bool populate = true;

                    // Readonly Textarea need to treat any value as Xml
                    if (oElmt.Name == "textarea" & oElmt.GetAttribute("class").Contains("readonly"))
                    {
                        bIsXml = true;
                        bReadOnly = true;
                    }

                    if (oElmt.Name == "textarea" & Strings.InStr(oElmt.GetAttribute("class"), "xhtml") > 0)
                        bIsXml = true;
                    if (oElmt.Name == "textarea" & Strings.InStr(oElmt.GetAttribute("class"), "xml") > 0)
                        bIsXml = true;
                    if (oElmt.Name == "textarea" & Strings.InStr(oElmt.GetAttribute("class"), "xsl") > 0)
                        bIsXml = true;

                    sXpath = oElmt.GetAttribute("ref");
                    if (!!string.IsNullOrEmpty(sXpath))
                    {
                        // case for bind
                        sBind = oElmt.GetAttribute("bind");
                        try
                        {
                            // if we have more than one bind node we want to select the last one found
                            var oBindNodes = model.SelectNodes("descendant-or-self::bind[@id='" + sBind + "' and not(@populate='false')]");
                            if (oBindNodes.Count > 0)
                            {
                                oBindNode = (XmlElement)oBindNodes[oBindNodes.Count - 1];
                            }
                            else
                            {
                                oBindNode = null;
                            }



                            // no bind element found, do nothing
                            if (oBindNode != null)
                            {
                                oBindElmt = oBindNode;
                                sDataType = oBindElmt.GetAttribute("type");

                                // If LCase(oBindElmt.GetAttribute("populate")) = "false" Then
                                // populate = False
                                // End If

                                // if we are an attribute then get the parent xpath
                                if (Strings.InStr(oBindElmt.GetAttribute("nodeset"), "@") == 1)
                                {
                                    sAttribute = Strings.Right(oBindElmt.GetAttribute("nodeset"), Strings.Len(oBindElmt.GetAttribute("nodeset")) - 1);
                                }
                                sXpath = getBindXpath(ref oBindElmt);
                                if (string.IsNullOrEmpty(sXpath))
                                    sXpath = ".";
                                cleanXpath = sXpath;

                                sXpath = Tools.Xml.addNsToXpath(sXpath, ref nsMgr);

                                // if bind has selected value then we need some clever stuff
                                // first we step through the possible values
                                if (Strings.InStr(sXpath, "$submittedValue") > 0)
                                {
                                    string oldXpath = sXpath;
                                    string modifiedXpath;
                                   // bool isModified = false;
                                    foreach (XmlElement valueElmt in oElmt.SelectNodes("item/value"))
                                    {
                                        modifiedXpath = oldXpath.Replace("$submittedValue", valueElmt.InnerText);
                                        if (Instance.SelectSingleNode(modifiedXpath, nsMgr) != null)
                                        {
                                            if ((oBindElmt.GetAttribute("value") ?? "") == (Instance.SelectSingleNode(modifiedXpath, nsMgr).InnerText ?? ""))
                                            {
                                                sXpath = modifiedXpath;
                                                sValue = valueElmt.InnerText;
                                                sDataType = "SelectedValueDropdown";
                                            }
                                        }

                                    }
                                    if (!(sDataType == "SelectedValueDropdown"))
                                    {
                                        sXpath = ".";
                                    }
                                }
                                cProcessInfo = "Error Binding to Node at:" + sXpath;
                            }
                        }
                        catch (Exception ex2)
                        {
                            returnException(ref msException, mcModuleName, "addValues", ex2, "", cProcessInfo, gbDebug);
                        }
                    }
                    if (!string.IsNullOrEmpty(sXpath))
                    {
                        try
                        {
                            if (Instance.SelectSingleNode(sXpath, nsMgr) is null)
                            {

                            }
                        }
                        catch (Exception)
                        {
                            cProcessInfo = sXpath + cleanXpath;
                        }

                        if (Instance.SelectSingleNode(sXpath, nsMgr) is null)
                        {
                            sValue = "invalid path";
                        }
                        else
                        {

                            switch (sDataType ?? "")
                            {
                                case "xml-replace":
                                    {
                                        sValue = Instance.SelectSingleNode(sXpath, nsMgr).OuterXml;
                                        bIsXml = true;
                                        break;
                                    }
                                case "SelectedValueDropdown":
                                    {
                                        // do nothing we allready have our value
                                        sValue = sValue;
                                        break;
                                    }

                                default:
                                    {
                                        if (bIsXml)
                                        {
                                            if (bReadOnly)
                                            {
                                                sValue = Instance.SelectSingleNode(sXpath, nsMgr).InnerXml;
                                            }
                                            else
                                            {
                                                sValue = Tools.Xml.XmlToForm(Instance.SelectSingleNode(sXpath, nsMgr).InnerXml);
                                            }
                                        }
                                        else if (!string.IsNullOrEmpty(sAttribute))
                                        {
                                            oinstanceElmt = (XmlElement)Instance.SelectSingleNode(sXpath, nsMgr);
                                            sValue = oinstanceElmt.GetAttribute(sAttribute);
                                        }
                                        else if (bIsXml)
                                        {
                                            sValue = Tools.Xml.XmlToForm(Instance.SelectSingleNode(sXpath, nsMgr).InnerXml);
                                        }
                                        else
                                        {

                                            sValue = Instance.SelectSingleNode(sXpath, nsMgr).InnerText;

                                            // NB 8th October 2009
                                            // New type - Unique Identifiers (currently for repeating elements in Polls)
                                            // If no value is present, then assign a unique identifier
                                            if (oElmt.GetAttribute("class").ToString().ToLower().Contains("generateuniqueid") & string.IsNullOrEmpty(sValue))
                                            {

                                                var Now = DateTime.Now;

                                                sValue = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("uID_", goSession["pgid"]), "_"), Now.Year.ToString()), Now.DayOfYear.ToString()), Now.Hour.ToString()), Now.Minute.ToString()), Now.Second.ToString()), Now.Millisecond.ToString()));

                                            }
                                            // If Not moXformElmt.OwnerDocument.SelectSingleNode("Page/Request/Form/Item[@name='" & oElmt.GetAttribute("bind") & "']") Is Nothing Then
                                            // Dim tempNode As XmlElement = moXformElmt.OwnerDocument.SelectSingleNode("Page/Request/Form/Item[@name='" & oElmt.GetAttribute("bind") & "']")
                                            // sValue = tempNode.InnerText
                                            // End If



                                        }

                                        break;
                                    }

                            }

                            string sValueXpath = string.Empty;
                            sValueXpath = "value";

                            // If populate Then
                            if (oElmt.SelectSingleNode("value") is null)
                            {
                                oValElmt = moPageXML.CreateElement("value");
                                if (bIsXml)
                                    oValElmt.InnerXml = sValue;
                                else
                                    oValElmt.InnerText = sValue;
                                oElmt.AppendChild(oValElmt);
                            }
                            else
                            {
                                oValElmt = (XmlElement)oElmt.SelectSingleNode("value");
                                if (bIsXml)
                                    oValElmt.InnerXml = sValue;
                                else
                                    oValElmt.InnerText = sValue;
                            }
                            // Else
                            // cProcessInfo = "no value set"

                            // End If

                            // NB: Added to clear the uniqueID value that seemed to spread to nearby nodes that had no value. Not sure why this wasn't clearing here originally
                            sValue = "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "addValues", ex, "", cProcessInfo, gbDebug);

            }

        }

        /// <summary>
        /// This has been superceded by Web.ProcessPageXMLForLanguage, but is left in for now for possibly making xforms a container class. 
        /// </summary>
        /// <remarks></remarks>
        protected void ProcessLanguageLabels_Superceded()
        {
            try
            {
                if (!string.IsNullOrEmpty(cLanguage))
                {
                    var nsMgr = new XmlNamespaceManager(RootGroup.OwnerDocument.NameTable);
                    nsMgr.AddNamespace("xml", "http://www.w3.org/XML/1998/namespace");
                    // Find all the labels which do not match the language
                    foreach (XmlElement node in RootGroup.SelectNodes("//label[@xml:lang and not(@xml:lang='" + cLanguage + "')]", nsMgr))
                        node.ParentNode.RemoveChild(node);
                }
            }
            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "ProcessLanguageLabels", ex, "", "", gbDebug);
            }
        }

        public void resetXFormUI()
        {
            // This does the opposite of addValues() - it looks for all the value nodes that are not in the instance, and removes them
            // Useful if you are wnating to run through an existing xForm object a second time after already calling addValues().

            XmlNodeList oNodes;
            XmlNode oOldNode;
            string cProcessInfo = "";
            try
            {

                // Note : avoiding item stops us from removing dropdown values!
                oNodes = moXformElmt.SelectNodes("//value[not(ancestor::instance) and not(ancestor::item)]");

                foreach (XmlNode oNode in oNodes)
                    oOldNode = oNode.ParentNode.RemoveChild(oNode);
            }
            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "resetXFormUI", ex, "", cProcessInfo, gbDebug);
            }

        }



        public XmlElement addGroup(ref XmlElement oContextNode, string sRef, string sClass = "", string sLabel = "", XmlElement oInsertBeforeNode = null)
        {
            XmlElement oGrpElmt;
            XmlElement oLabelElmt;
            string cProcessInfo = "";
            try
            {
                oGrpElmt = moPageXML.CreateElement("group");
                oGrpElmt.SetAttribute("ref", sRef);
                if (!string.IsNullOrEmpty(sClass))
                {
                    oGrpElmt.SetAttribute("class", sClass);
                }
                if (!string.IsNullOrEmpty(sLabel))
                {
                    oLabelElmt = moPageXML.CreateElement("label");
                    oLabelElmt.InnerXml = sLabel;
                    oGrpElmt.AppendChild(oLabelElmt);
                }

                if (oInsertBeforeNode is null)
                {
                    oContextNode.AppendChild(oGrpElmt);
                }
                else
                {
                    oContextNode.InsertBefore(oGrpElmt, oInsertBeforeNode);
                }

                return oGrpElmt;
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "addGroup", ex, "", cProcessInfo, gbDebug);
                return null;
            }
        }



        public XmlElement addSwitch(ref XmlElement oContextNode, string sRef, ref XmlElement oInsertBeforeNode, string sClass = "", string sLabel = "")
        {
            XmlElement oGrpElmt;
            XmlElement oLabelElmt;
            string cProcessInfo = "";
            try
            {
                oGrpElmt = moPageXML.CreateElement("switch");
                oGrpElmt.SetAttribute("ref", sRef);
                if (!string.IsNullOrEmpty(sClass))
                {
                    oGrpElmt.SetAttribute("class", "disable " + sClass);
                }
                else
                {
                    oGrpElmt.SetAttribute("class", "disable");
                }
                if (!string.IsNullOrEmpty(sLabel))
                {
                    oLabelElmt = moPageXML.CreateElement("label");
                    oLabelElmt.InnerXml = sLabel;
                    oGrpElmt.AppendChild(oLabelElmt);
                }

                if (oInsertBeforeNode is null)
                {
                    oContextNode.AppendChild(oGrpElmt);
                }
                else
                {
                    oContextNode.InsertBefore(oGrpElmt, oInsertBeforeNode);
                }

                return oGrpElmt;
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "addGroup", ex, "", cProcessInfo, gbDebug);
                return null;
            }
        }

        public XmlElement addCase(ref XmlElement oContextNode, string sId)
        {
            XmlElement oGrpElmt;
            string cProcessInfo = "";
            try
            {
                oGrpElmt = moPageXML.CreateElement("case");
                oGrpElmt.SetAttribute("id", sId);

                oContextNode.AppendChild(oGrpElmt);

                return oGrpElmt;
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "addGroup", ex, "", cProcessInfo, gbDebug);
                return null;
            }
        }

        public XmlElement addRepeat(ref XmlElement oContextNode, string sRef, string sClass = "", string sLabel = "")
        {
            XmlElement oGrpElmt;
            XmlElement oLabelElmt;
            string cProcessInfo = "";
            try
            {
                oGrpElmt = moPageXML.CreateElement("repeat");
                oGrpElmt.SetAttribute("ref", sRef);
                if (!string.IsNullOrEmpty(sClass))
                {
                    oGrpElmt.SetAttribute("class", sClass);
                }
                if (!string.IsNullOrEmpty(sLabel))
                {
                    oLabelElmt = moPageXML.CreateElement("label");
                    oLabelElmt.InnerText = sLabel;
                    oGrpElmt.AppendChild(oLabelElmt);
                }

                oContextNode.AppendChild(oGrpElmt);

                return oGrpElmt;
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "addRepeat", ex, "", cProcessInfo, gbDebug);
                return null;
            }
        }

        public XmlElement addInput(ref XmlElement oContextNode, string sRef, bool bBound, string sLabel)
        {

            string cProcessInfo = "";
            try
            {

                return addInput(ref oContextNode, sRef, bBound, sLabel, "");
            }
            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "addInput", ex, "", cProcessInfo, gbDebug);
                return null;
            }
        }

        public XmlElement addInput(ref XmlElement oContextNode, string sRef, bool bBound, string sLabel, string sClass)
        {

            string cProcessInfo = "";
            try
            {

                return addInput(ref oContextNode, sRef, bBound, sLabel, sClass, "");
            }
            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "addInput", ex, "", cProcessInfo, gbDebug);
                return null;
            }
        }


        public XmlElement addInput(ref XmlElement oContextNode, string sRef, bool bBound, string sLabel, string sClass, string sAutoComplete)
        {
            XmlElement addInputRet = default;
            XmlElement oIptElmt;
            XmlElement oLabelElmt;
            string cProcessInfo = "";
            try
            {
                oIptElmt = moPageXML.CreateElement("input");
                if (bBound)
                {
                    oIptElmt.SetAttribute("bind", sRef);
                }
                else
                {
                    oIptElmt.SetAttribute("ref", sRef);
                }
                if (!string.IsNullOrEmpty(sClass))
                {
                    oIptElmt.SetAttribute("class", sClass);
                }
                if (!string.IsNullOrEmpty(sAutoComplete))
                {
                    oIptElmt.SetAttribute("autocomplete", sAutoComplete);
                }
                if (!string.IsNullOrEmpty(sLabel))
                {
                    oLabelElmt = moPageXML.CreateElement("label");
                    oLabelElmt.InnerText = sLabel;
                    oIptElmt.AppendChild(oLabelElmt);
                }

                oContextNode.AppendChild(oIptElmt);
                addInputRet = oIptElmt;
            }
            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "addInput", ex, "", cProcessInfo, gbDebug);
                return null;
            }

            return addInputRet;
        }


        public XmlElement addClientSideValidation(ref XmlElement oInputNode, bool notEmpty, string notEmptyMessage)
        {
            // Dim oIptElmt As XmlElement
            // Dim oLabelElmt As XmlElement
            string cProcessInfo = "";
            try
            {
                if (notEmpty)
                {
                    oInputNode.SetAttribute("data-fv-not-empty", "true");
                }
                if (!string.IsNullOrEmpty(notEmptyMessage))
                {
                    oInputNode.SetAttribute("data-fv-not-empty___message", notEmptyMessage);
                }

                return oInputNode;
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "addInput", ex, "", cProcessInfo, gbDebug);
                return null;
            }
        }


        public XmlElement addBind(string sId, string sXpath, ref XmlElement oBindParent, string sRequired = "false()", string sType = "string", string sConstraint = "")
        {

            XmlElement oBindElmt;

            string cProcessInfo = "";
            try
            {
                // Optional code can be added here to nest bind elements.
                if (oBindParent is null)
                    oBindParent = model;

                // Or just add a bind element straight in
                oBindElmt = moPageXML.CreateElement("bind");
                if (!string.IsNullOrEmpty(sId))
                {
                    oBindElmt.SetAttribute("id", sId);
                }
                oBindElmt.SetAttribute("nodeset", sXpath);
                if (!string.IsNullOrEmpty(sId))
                {
                    oBindElmt.SetAttribute("required", sRequired);
                    oBindElmt.SetAttribute("type", sType);
                    if (!string.IsNullOrEmpty(sConstraint))
                        oBindElmt.SetAttribute("constraint", sConstraint);
                }
                oBindParent.AppendChild(oBindElmt);

                return oBindElmt;
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "addBind", ex, "", cProcessInfo, gbDebug);
                return null;
            }

        }

        public XmlElement addSecret(ref XmlElement oContextNode, string sRef, bool bBound, string sLabel, [Optional, DefaultParameterValue("")] ref string sClass)
        {
            XmlElement addSecretRet = default;
            XmlElement oIptElmt;
            XmlElement oLabelElmt;
            string cProcessInfo = "";
            try
            {
                oIptElmt = moPageXML.CreateElement("secret");
                if (bBound)
                {
                    oIptElmt.SetAttribute("bind", sRef);
                }
                else
                {
                    oIptElmt.SetAttribute("ref", sRef);
                }
                if (!string.IsNullOrEmpty(sClass))
                {
                    oIptElmt.SetAttribute("class", sClass);
                }
                if (!string.IsNullOrEmpty(sLabel))
                {
                    oLabelElmt = moPageXML.CreateElement("label");
                    oLabelElmt.InnerText = sLabel;
                    oIptElmt.AppendChild(oLabelElmt);
                }

                oContextNode.AppendChild(oIptElmt);

                addSecretRet = oIptElmt;
            }
            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "addSecret", ex, "", cProcessInfo, gbDebug);
                return null;
            }

            return addSecretRet;
        }

        public XmlElement addTextArea(ref XmlElement oContextNode, string sRef, bool bBound, string sLabel, ref string sClass, ref int nRows, ref int nCols)
        {
            XmlElement addTextAreaRet = default;
            XmlElement oIptElmt;
            XmlElement oLabelElmt;
            string cProcessInfo = "";
            try
            {
                oIptElmt = moPageXML.CreateElement("textarea");
                if (bBound)
                {
                    oIptElmt.SetAttribute("bind", sRef);
                }
                else
                {
                    oIptElmt.SetAttribute("ref", sRef);
                }
                if (!string.IsNullOrEmpty(sClass))
                {
                    oIptElmt.SetAttribute("class", sClass);
                }
                if (nRows > 0)
                {
                    oIptElmt.SetAttribute("rows", nRows.ToString());
                }
                if (nCols > 0)
                {
                    oIptElmt.SetAttribute("cols", nCols.ToString());
                }
                if (!string.IsNullOrEmpty(sLabel))
                {
                    oLabelElmt = moPageXML.CreateElement("label");
                    oLabelElmt.InnerText = sLabel;
                    oIptElmt.AppendChild(oLabelElmt);
                }

                oContextNode.AppendChild(oIptElmt);

                addTextAreaRet = oIptElmt;
            }
            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "addTextArea", ex, "", cProcessInfo, gbDebug);
                return null;
            }

            return addTextAreaRet;
        }



        public XmlElement addRange(ref XmlElement oContextNode, string sRef, bool bBound, string sLabel, string oStart, string oEnd, string oStep = "", string sClass = "")
        {
            XmlElement addRangeRet = default;
            XmlElement oIptElmt;
            XmlElement oLabelElmt;
            string cProcessInfo = "";
            try
            {
                oIptElmt = moPageXML.CreateElement("range");
                if (bBound)
                {
                    oIptElmt.SetAttribute("bind", sRef);
                }
                else
                {
                    oIptElmt.SetAttribute("ref", sRef);
                }
                if (!string.IsNullOrEmpty(sClass))
                {
                    oIptElmt.SetAttribute("class", sClass);
                }
                if (Information.IsDate(oStart))
                {
                    oIptElmt.SetAttribute("start", Tools.Xml.XmlDate(oStart));
                }
                else
                {
                    oIptElmt.SetAttribute("start", Conversions.ToString(oStart));
                }
                if (Information.IsDate(oEnd))
                {
                    oIptElmt.SetAttribute("end", Tools.Xml.XmlDate(oEnd));
                }
                else
                {
                    oIptElmt.SetAttribute("end", Conversions.ToString(oEnd));
                }
                oIptElmt.SetAttribute("step", Conversions.ToString(oStep));
                if (!string.IsNullOrEmpty(Convert.ToString(oStep)))
                {
                    oIptElmt.SetAttribute("step", Conversions.ToString(oStep));
                }
                if (!string.IsNullOrEmpty(sLabel))
                {
                    oLabelElmt = moPageXML.CreateElement("label");
                    oLabelElmt.InnerText = sLabel;
                    oIptElmt.AppendChild(oLabelElmt);
                }

                oContextNode.AppendChild(oIptElmt);

                addRangeRet = oIptElmt;
            }
            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "addRange", ex, "", cProcessInfo, gbDebug);
                return null;
            }

            return addRangeRet;
        }

        public XmlElement addUpload(ref XmlElement oContextNode, string sRef, bool bBound, string sMediaType, string sLabel, [Optional, DefaultParameterValue("")] ref string sClass)
        {
            XmlElement addUploadRet = default;
            XmlElement oIptElmt;
            XmlElement oLabelElmt;
            string cProcessInfo = "";
            try
            {
                oIptElmt = moPageXML.CreateElement("upload");
                if (bBound)
                {
                    oIptElmt.SetAttribute("bind", sRef);
                }
                else
                {
                    oIptElmt.SetAttribute("ref", sRef);
                }
                oIptElmt.SetAttribute("mediatype", sMediaType);
                if (!string.IsNullOrEmpty(sClass))
                {
                    oIptElmt.SetAttribute("class", sClass);
                }
                if (!string.IsNullOrEmpty(sLabel))
                {
                    oLabelElmt = moPageXML.CreateElement("label");
                    oLabelElmt.InnerText = sLabel;
                    oIptElmt.AppendChild(oLabelElmt);
                }
                oLabelElmt = moPageXML.CreateElement("filename");
                oLabelElmt.SetAttribute("ref", "@filename");
                oIptElmt.AppendChild(oLabelElmt);

                oLabelElmt = moPageXML.CreateElement("mediatype");
                oLabelElmt.SetAttribute("ref", "@mediatype");
                oIptElmt.AppendChild(oLabelElmt);

                oContextNode.AppendChild(oIptElmt);

                addUploadRet = oIptElmt;
            }
            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "addUpload", ex, "", cProcessInfo, gbDebug);
                return null;
            }

            return addUploadRet;
        }

        public XmlElement addSelect(ref XmlElement oContextNode, string sRef, bool bBound, string sLabel, string sClass = "", ApperanceTypes nAppearance = ApperanceTypes.Minimal)
        {
            XmlElement addSelectRet = default;
            XmlElement oIptElmt;
            XmlElement oLabelElmt;
            string cProcessInfo = "";
            try
            {
                oIptElmt = moPageXML.CreateElement("select");
                if (bBound)
                {
                    oIptElmt.SetAttribute("bind", sRef);
                }
                else
                {
                    oIptElmt.SetAttribute("ref", sRef);
                }
                if (!string.IsNullOrEmpty(sClass))
                {
                    oIptElmt.SetAttribute("class", sClass);
                }
                switch (nAppearance)
                {
                    case ApperanceTypes.Full:
                        {
                            oIptElmt.SetAttribute("appearance", "full");
                            break;
                        }
                    case ApperanceTypes.Minimal:
                        {
                            oIptElmt.SetAttribute("appearance", "minimal");
                            break;
                        }
                }

                if (!string.IsNullOrEmpty(sLabel))
                {
                    oLabelElmt = moPageXML.CreateElement("label");
                    oLabelElmt.InnerXml = sLabel;
                    oIptElmt.AppendChild(oLabelElmt);
                }

                // Removed choices as per request from Trevor.
                // oLabelElmt = moPageXML.CreateElement("choices")
                // oIptElmt.AppendChild(oLabelElmt)

                oContextNode.AppendChild(oIptElmt);

                addSelectRet = oIptElmt;
            }
            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "addSelect", ex, "", cProcessInfo, gbDebug);
                return null;
            }

            return addSelectRet;
        }

        public XmlElement addSelect1(ref XmlElement oContextNode, string sRef, bool bBound, string sLabel, string sClass = "", ApperanceTypes nAppearance = ApperanceTypes.Minimal)
        {
            string cProcessInfo = "";
            try
            {
                return addSelect1(ref oContextNode, sRef, bBound, sLabel, sClass, nAppearance, "");
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "addSelect1", ex, "", cProcessInfo, gbDebug);
                return null;
            }
        }

        public XmlElement addSelect1(ref XmlElement oContextNode, string sRef, bool bBound, string sLabel, string sClass = "")
        {
            string cProcessInfo = "";
            try
            {
                return addSelect1(ref oContextNode, sRef, bBound, sLabel, sClass, ApperanceTypes.Minimal, "");
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "addSelect1", ex, "", cProcessInfo, gbDebug);
                return null;
            }
        }

        public XmlElement addSelect1(ref XmlElement oContextNode, string sRef, bool bBound, string sLabel)
        {
            string cProcessInfo = "";
            try
            {
                return addSelect1(ref oContextNode, sRef, bBound, sLabel, "", ApperanceTypes.Minimal, "");
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "addSelect1", ex, "", cProcessInfo, gbDebug);
                return null;
            }
        }

        public XmlElement addSelect1(ref XmlElement oContextNode, string sRef, bool bBound, string sLabel, string sClass, ApperanceTypes nAppearance, string sAutoComplete)
        {
            XmlElement addSelect1Ret = default;
            XmlElement oIptElmt;
            XmlElement oLabelElmt;
            string cProcessInfo = "";
            try
            {
                oIptElmt = moPageXML.CreateElement("select1");
                if (bBound)
                {
                    oIptElmt.SetAttribute("bind", sRef);
                }
                else
                {
                    oIptElmt.SetAttribute("ref", sRef);
                }
                if (!string.IsNullOrEmpty(sClass))
                {
                    oIptElmt.SetAttribute("class", sClass);
                }
                if (!string.IsNullOrEmpty(sAutoComplete))
                {
                    oIptElmt.SetAttribute("autocomplete", sClass);
                }
                switch (nAppearance)
                {
                    case ApperanceTypes.Full:
                        {
                            oIptElmt.SetAttribute("appearance", "full");
                            break;
                        }
                    case ApperanceTypes.Minimal:
                        {
                            oIptElmt.SetAttribute("appearance", "minimal");
                            break;
                        }
                }
                if (!string.IsNullOrEmpty(sLabel))
                {
                    oLabelElmt = moPageXML.CreateElement("label");
                    oLabelElmt.InnerXml = sLabel;
                    oIptElmt.AppendChild(oLabelElmt);
                }

                // oLabelElmt = moPageXML.CreateElement("choices")
                // oIptElmt.AppendChild(oLabelElmt)

                oContextNode.AppendChild(oIptElmt);

                addSelect1Ret = oIptElmt;
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "addSelect1", ex, "", cProcessInfo, gbDebug);
                return null;
            }

            return addSelect1Ret;
        }

        public void addValue(ref XmlElement oInputNode, string sValue)
        {

            XmlElement oValueElmt;
            string cProcessInfo = "";
            try
            {
                oValueElmt = moPageXML.CreateElement("value");
                oValueElmt.InnerText = sValue;

                oInputNode.AppendChild(oValueElmt);
            }


            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "addValue", ex, "", cProcessInfo, gbDebug);
            }
        }


        public XmlElement addChoices(ref XmlElement oSelectNode, string sLabel)
        {
            XmlElement oOptElmt;
            XmlElement oLabelElmt;
            string cProcessInfo = "";
            try
            {
                oOptElmt = moPageXML.CreateElement("choices");
                if (!string.IsNullOrEmpty(sLabel))
                {
                    oLabelElmt = moPageXML.CreateElement("label");
                    oLabelElmt.InnerText = sLabel;
                    oOptElmt.AppendChild(oLabelElmt);
                }

                oSelectNode.AppendChild(oOptElmt);

                return oOptElmt;
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "addOption", ex, "", cProcessInfo, gbDebug);
                return null;
            }
        }

        public XmlElement addOption(ref XmlElement oSelectNode, string sLabel, string sValue, bool bXmlLabel = false, string ToggleCase = "")
        {
            XmlElement oOptElmt;
            XmlElement oLabelElmt;
            XmlElement oValueElmt;
            string cProcessInfo = "";
            try
            {
                oOptElmt = moPageXML.CreateElement("item");
                // If sLabel <> "" Then
                oLabelElmt = moPageXML.CreateElement("label");
                // if (sLabel.Contains("&")) {
                //      sLabel = sLabel;
                //  }
                if (bXmlLabel)
                {
                    oLabelElmt.InnerXml = Protean.Tools.Xml.convertEntitiesToCodes(sLabel) + " ";
                }
                else
                {
                    oLabelElmt.InnerText = sLabel + " ";
                   // removed as setting InnerText allready converts entitites.
                   // oLabelElmt.InnerText = Protean.Tools.Xml.convertEntitiesToCodes(sLabel) + " ";
                }
                oOptElmt.AppendChild(oLabelElmt);
                // End If
                // If sValue <> "" Then
                oValueElmt = moPageXML.CreateElement("value");
                oValueElmt.InnerText = sValue;
                oOptElmt.AppendChild(oValueElmt);
                // End If
                if (!string.IsNullOrEmpty(ToggleCase))
                {
                    var oToggleElmt = moPageXML.CreateElement("toggle");
                    oToggleElmt.SetAttribute("event", "DOMActivate");
                    oToggleElmt.SetAttribute("case", ToggleCase);
                    oOptElmt.AppendChild(oToggleElmt);
                }

                oSelectNode.AppendChild(oOptElmt);

                return oOptElmt;
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "addOption", ex, "", cProcessInfo, gbDebug);
                return null;

            }
        }

        public void addOptionsFromSqlDataReader(ref XmlElement oSelectNode, ref System.Data.SqlClient.SqlDataReader oDr, string sNameFld = "name", string sValueFld = "value")
        {

            string cProcessInfo = "";

            //bool ordinalsChecked = false;

            int nameOrdinal = 0;
            int valueOrdinal = 1;
            try
            {


                // AG - I'm adding this ordinal check in because previously this relied on the fields being called "name" and "value"
                // which is really annoying when you go to the trouble of passing only two column.
                // If name and value do not exist, then assume that the first column is name and the second is value.
                if (Tools.Database.HasColumn(oDr, sNameFld))
                {
                    nameOrdinal = oDr.GetOrdinal(sNameFld);
                }

                if (Tools.Database.HasColumn(oDr, sValueFld))
                {
                    valueOrdinal = oDr.GetOrdinal(sValueFld);
                }


                while (oDr.Read())
                    // update audit
                    // NB Change! Needs auth :S
                    addOption(ref oSelectNode, Strings.Replace(oDr[nameOrdinal].ToString(), "&amp;", "&"), Strings.Replace(oDr[valueOrdinal].ToString(), "&amp;", "&"));
                oDr.Close();
                oDr = null;
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "addOptionsFromSqlDataReader", ex, "", cProcessInfo, gbDebug);
            }
        }

        public void addUserOptionsFromSqlDataReader(ref XmlElement oSelectNode, ref System.Data.SqlClient.SqlDataReader oDr, string sNameFld = "name", string sValueFld = "value")
        {

            string cProcessInfo = "";
            string cName;
            try
            {

                while (oDr.Read())
                {

                    // hack for users to add full names
                    var oXml = new XmlDocument();
                    if (oDr.FieldCount > 2)
                    {
                        oXml.LoadXml(Conversions.ToString(oDr["detail"]));
                        if (oXml.DocumentElement.Name == "User")
                        {
                            cName = oXml.SelectSingleNode("User/LastName").InnerText + ", " + oXml.SelectSingleNode("User/FirstName").InnerText;
                        }
                        else
                        {
                            cName = oDr[sNameFld].ToString();
                        }
                    }
                    else
                    {
                        cName = oDr[sNameFld].ToString();
                    }


                    addOption(ref oSelectNode, cName, oDr[sValueFld].ToString());
                }
                oDr.Close();
                oDr = null;
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "addOptionsFromRecordSet", ex, "", cProcessInfo, gbDebug);
            }
        }

        public void addOptionsFilesFromDirectory(ref XmlElement oSelectNode, string DirectoryPath, string Extension = "")
        {

            string cProcessInfo = "";
            string fullDirPath = DirectoryPath;
            try
            {
                if (DirectoryPath.StartsWith("/"))
                {
                    fullDirPath = goServer.MapPath(DirectoryPath);
                }

                var dir = new DirectoryInfo(fullDirPath);
                FileInfo[] files = dir.GetFiles();


                if (!string.IsNullOrEmpty(Extension))
                {
                    if (!Extension.StartsWith("."))
                    {
                        Extension = "." + Extension;
                    }
                }

                foreach (var fi in files)
                {
                    if (string.IsNullOrEmpty(Extension) | (Extension ?? "") == (fi.Extension ?? ""))
                    {
                        addOption(ref oSelectNode, fi.Name, DirectoryPath + @"\" + fi.Name);
                    }
                }
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "addOptionsFromRecordSet", ex, "", cProcessInfo, gbDebug);
            }
        }

        public void addOptionsFoldersFromDirectory(ref XmlElement oSelectNode, string DirectoryPath)
        {

            string cProcessInfo = "";
            string fullDirPath = DirectoryPath;
            try
            {
                if (DirectoryPath.StartsWith("/"))
                {
                    fullDirPath = goServer.MapPath(DirectoryPath);
                }


                var dir = new DirectoryInfo(fullDirPath);
                DirectoryInfo[] folders = dir.GetDirectories();

                foreach (var fo in folders)
                {
                    if (!fo.Name.StartsWith("~") & !fo.Name.StartsWith("_vti"))
                    {
                        addOption(ref oSelectNode, DirectoryPath.Replace(@"\", "/") + "/" + fo.Name, DirectoryPath + "/" + fo.Name);
                        addOptionsFoldersFromDirectory(ref oSelectNode, DirectoryPath + @"\" + fo.Name);
                    }
                }
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "addOptionsFromRecordSet", ex, "", cProcessInfo, gbDebug);
            }
        }

        public void addNote(string sRef, noteTypes nTypes, string sMessage, bool bInsertFirst = false, string sClass = "")
        {

            XmlElement oIptElmt;
            XmlElement oNoteElmt = null;
            string cProcessInfo = "";
            try
            {
                if (moXformElmt.SelectSingleNode("descendant-or-self::*[@ref='" + sRef + "' or @bind='" + sRef + "']") != null)
                {
                    oIptElmt = (XmlElement)moXformElmt.SelectSingleNode("descendant-or-self::*[@ref='" + sRef + "' or @bind='" + sRef + "']");
                    switch (nTypes)
                    {
                        case 0:
                            {
                                oNoteElmt = moPageXML.CreateElement("hint");
                                break;
                            }
                        case (noteTypes)1:
                            {
                                oNoteElmt = moPageXML.CreateElement("help");
                                break;
                            }
                        case (noteTypes)2:
                            {
                                oNoteElmt = moPageXML.CreateElement("alert");
                                break;
                            }
                    }

                    if (!string.IsNullOrEmpty(sClass))
                    {
                        oNoteElmt.SetAttribute("class", sClass);
                    }

                    oNoteElmt.InnerXml = sMessage + "";
                    if (bInsertFirst & oIptElmt.FirstChild != null)
                    {
                        oIptElmt.InsertBefore(oNoteElmt, oIptElmt.FirstChild);
                    }
                    else
                    {
                        oIptElmt.AppendChild(oNoteElmt);
                    }

                }
            }
            catch (Exception ex)
            {
                cProcessInfo = sRef + " - " + ((int)nTypes).ToString() + " - " + sMessage;
                returnException(ref msException, mcModuleName, "addNote - by ref", ex, "", cProcessInfo, gbDebug);
            }
        }

        public void addNote(ref XmlElement oNode, noteTypes nTypes, string sMessage, bool bInsertFirst = false, string sClass = "")
        {
            valid = false;
            XmlNode frmNode = (XmlNode)moXformElmt;
            addNote(ref frmNode, nTypes, sMessage, bInsertFirst, sClass);
        }

            public void addNote(ref XmlNode oNode, noteTypes nTypes, string sMessage, bool bInsertFirst = false, string sClass = "")
        {
            if (sMessage is null)
                sMessage = "";
            XmlElement oNoteElmt = null;
            string cProcessInfo = sMessage;
            try
            {

                switch (nTypes)
                {
                    case 0:
                        {
                            oNoteElmt = moPageXML.CreateElement("hint");
                            break;
                        }
                    case (noteTypes)1:
                        {
                            oNoteElmt = moPageXML.CreateElement("help");
                            break;
                        }
                    case (noteTypes)2:
                        {
                            oNoteElmt = moPageXML.CreateElement("alert");
                            break;
                        }
                }

                Tools.Xml.SetInnerXmlThenInnerText(ref oNoteElmt, sMessage);

                if (!string.IsNullOrEmpty(sClass))
                {
                    oNoteElmt.SetAttribute("class", sClass);
                }


                if (oNode != null)
                {
                    if (bInsertFirst & oNode.FirstChild != null)
                    {
                        oNode.InsertBefore(oNoteElmt, oNode.FirstChild);
                    }
                    else
                    {
                        oNode.AppendChild(oNoteElmt);
                    }
                }
            }


            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "addNote - by node", ex, "", cProcessInfo, gbDebug);
            }
        }

        public object addDiv(ref XmlElement oContextNode, string sXhtml, string sClass = "principle", bool addAsXml = true)
        {
            object addDivRet = default;

            XmlElement oIptElmt;
            string cProcessInfo = "";
            try
            {
                oIptElmt = moPageXML.CreateElement("div");
                oIptElmt.SetAttribute("class", sClass);

                if (addAsXml)
                {
                    oIptElmt.InnerXml = sXhtml;
                }
                else
                {
                    oIptElmt.InnerText = sXhtml;
                }

                oContextNode.AppendChild(oIptElmt);

                addDivRet = oIptElmt;
            }
            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "addDiv", ex, "", cProcessInfo, gbDebug);
                return null;
            }

            return addDivRet;
        }




        public object addSubmit(ref XmlElement oContextNode, string sSubmission, string sLabel, string sRef = "submit", string sClass = "principle", string sIcon = "", string sValue = "")
        {
            object addSubmitRet = default;

            XmlElement oIptElmt;
            XmlElement oLabelElmt;
            string cProcessInfo = "";
            try
            {
                if(sRef == null)
                {
                    sRef = "submit";
                }
                if (sClass == null)
                {
                    sClass = "principle";
                }
                oIptElmt = oContextNode.OwnerDocument.CreateElement("submit");
                oIptElmt.SetAttribute("submission", sSubmission);
                oIptElmt.SetAttribute("ref", sRef);
                oIptElmt.SetAttribute("class", sClass);
                if (!string.IsNullOrEmpty(sIcon))
                {
                    oIptElmt.SetAttribute("icon", sIcon);
                }
                if (!string.IsNullOrEmpty(sValue))
                {
                    oIptElmt.SetAttribute("value", sValue);
                }

                if (!string.IsNullOrEmpty(sLabel))
                {
                    oLabelElmt = oContextNode.OwnerDocument.CreateElement("label");
                    oLabelElmt.InnerText = sLabel;
                    oIptElmt.AppendChild(oLabelElmt);
                }

                oContextNode.AppendChild(oIptElmt);

                addSubmitRet = oIptElmt;
            }
            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "addSubmit", ex, "", cProcessInfo, gbDebug);
                return null;
            }

            return addSubmitRet;
        }

        public virtual bool submit()
        {
            string cProcessInfo = "";
            XmlElement oElmt;
            string sServiceUrl;
            string sSoapAction;
            string sActionName;
            string SoapRequestXml = string.Empty;
            string soapBody;
            string sResponse;
            XmlElement oSoapElmt;
            string cNsURI;

            try
            {
                updateInstanceFromRequest();
                validate();

                if (valid)
                {
                    var soapClnt = new Protean.SoapClient();


                    foreach (XmlNode oNode in moXformElmt.SelectNodes("descendant-or-self::submit"))
                    {
                        // ok get the ref or the bind name of the button
                        oElmt = (XmlElement)oNode;
                        if (!string.IsNullOrEmpty(goRequest[oElmt.GetAttribute("submission")]))
                        {

                            foreach (XmlElement oSubElmt in model.SelectNodes("submission"))
                            {

                                sServiceUrl = oSubElmt.GetAttribute("action");
                                sSoapAction = oSubElmt.GetAttribute("SOAPAction");
                                sActionName = sSoapAction.Substring(sSoapAction.LastIndexOf("/") + 1, sSoapAction.Length - sSoapAction.LastIndexOf("/") - 1);

                                soapClnt.ServiceUrl = sServiceUrl;
                                if (Instance.SelectNodes("*").Count > 1)
                                {
                                    foreach (XmlElement newElmt in Instance.SelectNodes("*"))
                                    {
                                        if ((newElmt.GetAttribute("id") ?? "") == (oSubElmt.GetAttribute("id") ?? ""))
                                        {
                                            SoapRequestXml = newElmt.OuterXml;
                                        }
                                    }
                                }
                                // do this way to skip the query namespace issue
                                // SoapRequestXml = Instance.SelectSingleNode(sActionName & "[@id='" & oSubElmt.GetAttribute("id") & "']").OuterXml
                                else
                                {
                                    SoapRequestXml = Instance.SelectSingleNode("*").OuterXml;
                                }

                                soapBody = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + soapClnt.getSoapEnvelope(SoapRequestXml).OuterXml;

                                soapClnt.SendSoapRequest(sActionName, soapBody);

                                // replace the instance with the response

                                var nsmgr = new XmlNamespaceManager(moPageXML.NameTable);
                                nsmgr.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");

                                oSoapElmt = (XmlElement)soapClnt.results.SelectSingleNode("soap:Envelope/soap:Body", nsmgr).FirstChild;

                                try
                                {
                                    cNsURI = oSoapElmt.GetAttribute("xmlns");
                                    nsmgr.AddNamespace("ews", cNsURI);
                                    sResponse = oSoapElmt.SelectSingleNode("ews:" + sActionName + "Result", nsmgr).InnerText;
                                }
                                catch (Exception)
                                {
                                    if (oSoapElmt.SelectSingleNode(sActionName + "Result", nsmgr) is null)
                                    {
                                        sResponse = soapClnt.results.OuterXml.Replace("<", "[").Replace(">", "]");
                                    }
                                    else
                                    {
                                        sResponse = oSoapElmt.SelectSingleNode(sActionName + "Result", nsmgr).InnerText;
                                    }
                                }

                                // Try to add the response
                                try
                                {
                                    XmlNode grpNode = moXformElmt.SelectSingleNode("descendant-or-self::group[1]");
                                    addNote(ref grpNode, noteTypes.Alert, sResponse, true, "alert-success");

                                }
                                catch (XmlException)
                                {
                                    try
                                    {
                                        sResponse = Tools.Xml.encodeAllHTML(sResponse);
                                        //XmlNode argoNode1 = moXformElmt;
                                        addNote(ref moXformElmt, noteTypes.Alert, sResponse);
                                        //moXformElmt = (XmlElement)argoNode1;
                                    }
                                    catch (Exception)
                                    {
                                        //XmlNode argoNode2 = moXformElmt;
                                        addNote(ref moXformElmt, noteTypes.Alert, "Response could not be added due to an Xml Error");
                                        //moXformElmt = (XmlElement)argoNode2;
                                    }
                                }
                                catch (Exception)
                                {
                                    //XmlNode argoNode3 = moXformElmt;
                                    addNote(ref moXformElmt, noteTypes.Alert, "Response could not be added.");
                                    //moXformElmt = (XmlElement)argoNode3;
                                }
                            }

                        }
                    }

                }
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "submit", ex, "", cProcessInfo, gbDebug);
            }

            return default;

        }

        // Steps through all of the submit buttons to see if they have been pressed
        public virtual bool isSubmitted()
        {
            bool isSubmittedRet = default;
            XmlElement oElmt;

            string cProcessInfo = "";
            try
            {
                isSubmittedRet = false;
                if (moXformElmt is null)
                {
                    cProcessInfo = "xFormElement not set";
                }
                else
                {
                    foreach (XmlNode oNode in moXformElmt.SelectNodes("descendant-or-self::submit[not(ancestor::instance)]"))
                    {
                        // ok get the ref or the bind name of the button
                        oElmt = (XmlElement)oNode;
                        if (!string.IsNullOrEmpty(oElmt.GetAttribute("submission")) & !string.IsNullOrEmpty(goRequest.Form[oElmt.GetAttribute("submission")]))
                        {
                            isSubmittedRet = true;
                            SubmittedRef = oElmt.GetAttribute("submission");
                        }
                        else if (!string.IsNullOrEmpty(goRequest[oElmt.GetAttribute("ref")]))
                        {
                            isSubmittedRet = true;
                            SubmittedRef = oElmt.GetAttribute("ref");
                        }
                        else if (!string.IsNullOrEmpty(goRequest[oElmt.GetAttribute("bind")]) & (goRequest[oElmt.GetAttribute("bind")] ?? "") != (goRequest["ewCmd"] ?? ""))
                        {
                            isSubmittedRet = true;
                            SubmittedRef = oElmt.GetAttribute("bind");
                        }
                        else if (!string.IsNullOrEmpty(goRequest["ewSubmitClone_" + oElmt.GetAttribute("ref")]))
                        {
                            isSubmittedRet = true;
                            SubmittedRef = oElmt.GetAttribute("ref");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "getSubmitted", ex, "", cProcessInfo, gbDebug);
            }
            return isSubmittedRet;
        }

        // Steps through all of the submit buttons to see if they have been pressed
        public virtual bool isDeleted()
        {
            bool isDeletedRet = default;
            XmlElement oElmt;
            string cProcessInfo = "";
            try
            {
                // toggles bDeleted if case = true
                if (!bDeleted)
                {
                    foreach (XmlNode oNode in moXformElmt.SelectNodes("descendant-or-self::submit[@ref='delete']"))
                    {
                        // ok get the ref or the bind name of the button
                        oElmt = (XmlElement)oNode;
                        if (!string.IsNullOrEmpty(goRequest[oElmt.GetAttribute("ref")]))
                        {
                            bDeleted = true;
                        }
                    }
                }
                else
                {
                    bDeleted = false;
                }

                isDeletedRet = bDeleted;
            }


            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "getSubmitted", ex, "", cProcessInfo, gbDebug);
            }

            return isDeletedRet;

        }

        // Steps through all of the submit buttons to see if they have been pressed
        public string getSubmitted()
        {
            XmlElement oElmt;
            string strReturn = "";
            string cProcessInfo = "";
            try
            {

                foreach (XmlNode oNode in moXformElmt.SelectNodes("descendant-or-self::submit"))
                {
                    // ok get the ref or the bind name of the button
                    oElmt = (XmlElement)oNode;
                    if (!string.IsNullOrEmpty(goRequest[oElmt.GetAttribute("submission")]))
                    {
                        strReturn = oElmt.GetAttribute("submission");
                    }
                    else if (!string.IsNullOrEmpty(goRequest[oElmt.GetAttribute("ref")]))
                    {
                        strReturn = oElmt.GetAttribute("ref");
                    }
                    else if (!string.IsNullOrEmpty(goRequest[oElmt.GetAttribute("bind")]))
                    {
                        strReturn = oElmt.GetAttribute("bind");
                    }
                }
                return strReturn;
            }
            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "isSubmitted", ex, "", cProcessInfo, gbDebug);
                return null;
            }

        }

        private string getBindXpath(ref XmlElement oBindElmt, string sXpath = "")
        {

            XmlNode oBindParent;
            XmlElement oBindParentElmt;
            string sNodeSet;

            if (!string.IsNullOrEmpty(sXpath))
            {
                sNodeSet = sXpath;
            }
            else if (Strings.InStr(oBindElmt.GetAttribute("nodeset"), "@") == 1)
            {
                // ignore for attributes
                sNodeSet = "";
            }
            else
            {
                sNodeSet = oBindElmt.GetAttribute("nodeset");
            }
            if (oBindElmt.SelectSingleNode("parent::bind") != null)
            {
                oBindParent = oBindElmt.SelectSingleNode("parent::bind");
                oBindParentElmt = (XmlElement)oBindParent;
                if (string.IsNullOrEmpty(sNodeSet))
                {
                    XmlElement argoBindElmt = (XmlElement)oBindParent;
                    sNodeSet = getBindXpath(ref argoBindElmt, oBindParentElmt.GetAttribute("nodeset"));
                    oBindParent = argoBindElmt;
                }
                else
                {
                    XmlElement argoBindElmt1 = (XmlElement)oBindParent;
                    sNodeSet = getBindXpath(ref argoBindElmt1, oBindParentElmt.GetAttribute("nodeset") + "/" + sNodeSet);
                    oBindParent = argoBindElmt1;
                }
            }

            return sNodeSet;

        }

        public string getNewRef(string refPrefix)
        {
            XmlNodeList oNodeList;
            XmlElement oElmt;
            long nLastNum;
            string cProcessInfo = "";
            try
            {
                oNodeList = moXformElmt.SelectNodes("descendant-or-self::*[starts-with(@ref,'" + refPrefix + "') or starts-with(@bind,'" + refPrefix + "')]");
                nLastNum = 0L;
                foreach (XmlNode oNode in oNodeList)
                {
                    oElmt = (XmlElement)oNode;
                    if (string.IsNullOrEmpty(oElmt.GetAttribute("ref"))) // its not a ref its a bind
                    {
                        if (Conversions.ToLong("0" + Strings.Replace(oElmt.GetAttribute("bind"), refPrefix, "")) > nLastNum)
                        {
                            nLastNum = Conversions.ToLong("0" + Strings.Replace(oElmt.GetAttribute("bind"), refPrefix, ""));
                        }
                    }
                    else if (Conversions.ToLong("0" + Strings.Replace(oElmt.GetAttribute("ref"), refPrefix, "")) > nLastNum) // it is a ref
                    {
                        nLastNum = Conversions.ToLong("0" + Strings.Replace(oElmt.GetAttribute("ref"), refPrefix, ""));
                    }

                }
                return refPrefix + (nLastNum + 1L).ToString();
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "getNewRef", ex, "", cProcessInfo, gbDebug);
                return null;
            }
        }


        /// <summary>
        /// this deletes the specified nodes in the instance
        /// </summary>
        /// <remarks></remarks>
        private bool checkForDeleteCommand(ref XmlElement xmlForm)
        {

            string cProcessInfo = "";
            bool bDeletionDone = false;

            try
            {

                // check for delete command
                string cRequestForm = goRequest.Form.ToString();
                var rxDelete = new Regex(@"delete%3a(.+?)_(\d+)=Del");

                if (rxDelete.IsMatch(cRequestForm))
                {

                    // get the index of the options group and node from the delete command
                    string cControlName = rxDelete.Match(cRequestForm).Groups[1].ToString();
                    int nNodeIndex = Conversions.ToInteger(rxDelete.Match(cRequestForm).Groups[2].ToString());
                    string cXpathOptions;

                    foreach (XmlElement bindNode in xmlForm.SelectNodes("descendant-or-self::bind[@id='" + cControlName + "']"))
                    {
                        if (bindNode != null)
                        {
                            XmlElement xmlbindNodeElmt = (XmlElement)bindNode;
                            cXpathOptions = getBindXpath(ref xmlbindNodeElmt);
                            var argoNode = oInstance.SelectSingleNode("*[1]");
                            var nsMgr = Tools.Xml.getNsMgrRecursive(ref argoNode, ref moPageXML);
                            cXpathOptions = Protean.Tools.Xml.addNsToXpath(cXpathOptions, ref nsMgr);

                            XmlElement nodeToDelete = (XmlElement)oInstance.SelectSingleNode(cXpathOptions + "[position()=" + (nNodeIndex + 1) + "]", nsMgr);
                            if (nodeToDelete != null)
                            {
                                nodeToDelete.SetAttribute("deletefromXformInstance", "true");
                                bDeletionDone = true;
                            }
                        }
                    }
                }

                return bDeletionDone;
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "checkForDeleteCommand", ex, "", cProcessInfo, gbDebug);
            }

            return default;
        }


        private void processRepeats(ref XmlElement xFormElmt)
        {
            try
            {
                XmlElement oRptElmtCopy;
                XmlElement oRptElmtCopySub;
                string sBindXpath;
                XmlNodeList oInstanceNodeSet;
                XmlElement oNode;
                XmlElement oBindCopy;
                long nNodePosition;
                bool isInserted = false;

                if (bProcessRepeats)
                {

                    if (checkForDeleteCommand(ref xFormElmt))
                    {
                        isTriggered = true;
                    }

                    var argoNode = oInstance.SelectSingleNode("*[1]");
                    var nsMgr = Protean.Tools.Xml.getNsMgr(ref argoNode, ref moPageXML);

                    foreach (XmlElement oRptElmt in xFormElmt.SelectNodes("descendant-or-self::repeat[not(contains(@class,'relatedContent') or contains(@class,'repeated'))]"))
                    {
                        isInserted = false;
                        if (!string.IsNullOrEmpty(oRptElmt.GetAttribute("bind")))
                        {
                            if (model == null) {
                                model = (XmlElement)xFormElmt.SelectSingleNode("model");
                            }
                            // get the bind elements
                            foreach (XmlElement oBindNode in model.SelectNodes("descendant-or-self::bind[not(ancestor::instance) and @id='" + oRptElmt.GetAttribute("bind") + "']"))
                            {
                                // build the bind xpath
                                XmlElement xmloBindNode = (XmlElement)oBindNode;
                                sBindXpath = getBindXpath(ref xmloBindNode);
                                sBindXpath = Protean.Tools.Xml.addNsToXpath(sBindXpath, ref nsMgr);
                                // get the nodeset of repeating elements

                                bool updateSubsquentRequested = false;

                                if (!string.IsNullOrEmpty(goRequest["insert:" + oRptElmt.GetAttribute("bind")]))
                                {

                                    // get the first node in the instance to copy
                                    XmlElement oInitialNode = (XmlElement)oInitialInstance.SelectSingleNode(sBindXpath + "[position() = 1]", nsMgr);
                                    XmlElement oFirstNode = (XmlElement)oInstance.SelectSingleNode(sBindXpath + "[position() = 1]", nsMgr);
                                    XmlElement oNewNode = (XmlElement)oInitialNode.CloneNode(true);
                                    // strip values from new node
                                    foreach (XmlNode oEachNode in oNewNode.SelectNodes("descendant-or-self::*"))
                                    {
                                        if (oEachNode.SelectNodes("*").Count == 0)
                                        {
                                            oEachNode.InnerText = "";
                                        }
                                    }

                                    oFirstNode.ParentNode.InsertAfter(oNewNode, oInstance.SelectSingleNode(sBindXpath + "[last()]", nsMgr));
                                    isTriggered = true;
                                    isInserted = true;

                                }

                                oInstanceNodeSet = oInstance.SelectNodes(sBindXpath, nsMgr);
                                nNodePosition = 0L;
                                long nNodeCount = 0L;
                                if (oInstanceNodeSet.Count == 0)
                                {                                   
                                    addNote(ref moXformElmt, noteTypes.Alert, "The repeat with bind='" + oRptElmt.GetAttribute("bind") + "' could not find the node in the instance on xpath '" + sBindXpath + "'");                                    
                                }
                                else
                                {
                                    object oInstanceNodeSetCount = 0;
                                    bool bSkipBinds = false;
                                    foreach (XmlElement currentONode in oInstanceNodeSet)
                                    {
                                        oNode = currentONode;
                                        oInstanceNodeSetCount = Operators.AddObject(oInstanceNodeSetCount, 1);
                                        if (Conversions.ToBoolean(Operators.AndObject(Operators.ConditionalCompareObjectEqual(oInstanceNodeSet.Count, oInstanceNodeSetCount, false), isInserted)))
                                        {
                                            bSkipBinds = true;
                                        }
                                        if (!(oNode.GetAttribute("deletefromXformInstance") == "true"))
                                        {

                                            // step through and 
                                            // build the repeating binds
                                            oBindCopy = (XmlElement)oBindNode.CloneNode(true);
                                            // update the bindCopyId with position
                                            oBindCopy.SetAttribute("id", oBindNode.GetAttribute("id") + "_" + nNodePosition.ToString());
                                            oBindCopy.SetAttribute("nodeset", oBindNode.GetAttribute("nodeset") + "[position() = " + (nNodePosition + 1L).ToString() + "]");
                                            // update child bind id's
                                            foreach (XmlElement obindElmt in oBindCopy.SelectNodes("bind"))
                                            {
                                                string newBindId = obindElmt.GetAttribute("id") + "_" + nNodePosition.ToString();
                                                obindElmt.SetAttribute("id", newBindId);

                                                if (bSkipBinds)
                                                {
                                                    BindsToSkip.Add(newBindId);
                                                }

                                            }

                                            // test if the current node is one we want to delete, note that nodeCount is current nodes whereas nodePosition is the new numbering
                                            //bool bDelete = false;

                                            // build the repeating form groups
                                            oRptElmtCopy = (XmlElement)oRptElmt.CloneNode(true);
                                            // update the elmtId with position

                                            // On Repeated Form Controls make binds unique
                                            foreach (XmlElement currentORptElmtCopySub in oRptElmtCopy.SelectNodes("descendant-or-self::*[@bind!='']"))
                                            {
                                                oRptElmtCopySub = currentORptElmtCopySub;
                                                string cBindId = oRptElmtCopySub.GetAttribute("bind") + "_" + nNodePosition.ToString();
                                                oRptElmtCopySub.SetAttribute("bind", cBindId);
                                            }

                                            // make toggle case unique
                                            foreach (XmlElement currentORptElmtCopySub1 in oRptElmtCopy.SelectNodes("descendant-or-self::toggle[@case!='']"))
                                            {
                                                oRptElmtCopySub = currentORptElmtCopySub1;
                                                string cCase = oRptElmtCopySub.GetAttribute("case") + "_" + nNodePosition.ToString();
                                                oRptElmtCopySub.SetAttribute("case", cCase);
                                            }

                                            foreach (XmlElement currentORptElmtCopySub2 in oRptElmtCopy.SelectNodes("descendant-or-self::case[@id!='']"))
                                            {
                                                oRptElmtCopySub = currentORptElmtCopySub2;
                                                string cCase = oRptElmtCopySub.GetAttribute("id") + "_" + nNodePosition.ToString();
                                                oRptElmtCopySub.SetAttribute("id", cCase);
                                            }

                                            // NB Mark Nodes so if processRepeats is called again, they don't get stuck recursing themselves
                                            if (oRptElmtCopy.GetAttribute("class") != null)
                                            {
                                                oRptElmtCopy.SetAttribute("class", oRptElmtCopy.GetAttribute("class").ToString() + " repeated rpt-" + nNodePosition.ToString());
                                            }
                                            else
                                            {
                                                oRptElmtCopy.SetAttribute("class", "repeated rpt-" + nNodePosition.ToString());
                                            }

                                            // insert the repeated binds
                                            oBindNode.ParentNode.InsertAfter(oBindCopy, oBindNode.ParentNode.LastChild);
                                            // insert the repeated repeat but only once not for each bind
                                            if (oRptElmt.ParentNode != null)
                                            {
                                                // Adding the repeated controls but only once
                                                oRptElmt.ParentNode.InsertAfter(oRptElmtCopy, oRptElmt.ParentNode.LastChild);

                                                // set oForm to not be readonly
                                                var oForm = goRequest.Form;
                                                oForm = (System.Collections.Specialized.NameValueCollection)goRequest.GetType().GetField("_form", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(goRequest);
                                                var readOnlyInfo = oForm.GetType().GetProperty("IsReadOnly", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                                readOnlyInfo.SetValue(oForm, false, null);

                                                if (updateSubsquentRequested)
                                                {
                                                    // we have removed a repeat element therefore all binds 
                                                    // on the request object that come after must be reduced by one 
                                                    // so we can call updateinstancefromRequest and values are stored in the correct location.
                                                    // For Each oRptElmtCopySub In oRptElmtCopy.SelectNodes("descendant-or-self::*")
                                                    foreach (XmlElement currentORptElmtCopySub3 in oRptElmtCopy.SelectNodes("descendant::*[@bind!='' and name()!='delete']"))
                                                    {
                                                        oRptElmtCopySub = currentORptElmtCopySub3;
                                                        string cBindStart = oRptElmtCopySub.GetAttribute("bind").Split('_')[0];
                                                        string cOldBindId = cBindStart + "_" + (Conversions.ToDouble(nNodePosition.ToString()) + 1d);
                                                        string cNewBindId = cBindStart + "_" + nNodePosition.ToString();

                                                        oForm.Set(cNewBindId, oForm[cOldBindId]);
                                                    }
                                                }
                                                // Set oForm back to readonly
                                                readOnlyInfo.SetValue(oForm, true, null);

                                            }

                                            // increment our keys
                                            nNodePosition = nNodePosition + 1L;
                                            nNodeCount = nNodeCount + 1L;
                                        }

                                        else
                                        {
                                            updateSubsquentRequested = true;
                                        }
                                    }

                                    updateSubsquentRequested = false;

                                    // delete items from the instance that are set for deletion.
                                    foreach (XmlElement currentONode1 in oInstanceNodeSet)
                                    {
                                        oNode = currentONode1;
                                        if (oNode.GetAttribute("deletefromXformInstance") == "true")
                                        {
                                            oNode.ParentNode.RemoveChild(oNode);
                                        }
                                    }

                                    // delete the origional bind and repeat controls.
                                    oBindNode.ParentNode.RemoveChild(oBindNode);
                                    if (oRptElmt.ParentNode != null)
                                    {
                                        oRptElmt.ParentNode.RemoveChild(oRptElmt);
                                    }

                                }
                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "processRepeats", ex, "", bDebug: gbDebug);
            }
        }

        private void processFormParameters()
        {
            string processInfo = "";
            string formXml = "";
            try
            {

                if (_formParameters != null && _formParameters.Length > 0)
                {
                    formXml = string.Format(moXformElmt.InnerXml, _formParameters);
                    if ((formXml ?? "") != (moXformElmt.InnerXml ?? ""))
                    {
                        // Something has changed so let's update the references.
                        processInfo = "Parameters loaded, updating form xml";
                        moXformElmt.InnerXml = formXml;
                        model = (XmlElement)moXformElmt.SelectSingleNode("descendant-or-self::model");
                        if (model != null)
                        {
                            oInstance = (XmlElement)model.SelectSingleNode("descendant-or-self::instance");
                        }
                        else
                        {
                            Instance = null;
                            oInstance = null;
                        }
                    }

                }
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "processFormParameters", ex, "", processInfo, gbDebug);
            }
        }

        private void PreLoadInstance()
        {
            try
            {
                if (!string.IsNullOrEmpty(_preLoadedInstanceXml))
                {
                    // Instance has been preloaded
                    oInstance.InnerXml = _preLoadedInstanceXml;
                }
            }
            catch (Exception)
            {
            }
        }

        // Public Overridable Sub CombineInstance(ByRef oMasterInstance As XmlElement, ByRef oExistingInstance As XmlElement)
        // 'combines an existing instance with a new 
        // Try
        // 'firstly we will go through and find like for like
        // 'we will skip the immediate node below instance as this is usually fairly static
        // 'except for attributes

        // 'Attributes of Root
        // For i As Integer = 0 To oMasterInstance.FirstChild.Attributes.Count - 1
        // Dim oMainAtt As XmlAttribute = oMasterInstance.FirstChild.Attributes(i)
        // If Not oExistingInstance.Attributes.ItemOf(oMainAtt.Name).Value = "" Then
        // oMainAtt.Value = oExistingInstance.Attributes.ItemOf(oMainAtt.Name).Value
        // End If
        // Next

        // 'Navigate Down Elmts and find with same xpath
        // For Each oMainElmt As XmlElement In oMasterInstance.FirstChild.SelectNodes("*")
        // CombineInstance_Sub1(oMainElmt, oExistingInstance, oMasterInstance.FirstChild.Name)
        // Next

        // 'Now for any existing stuff that has not been brought across
        // For Each oExistElmt As XmlElement In oExistingInstance.FirstChild.SelectNodes("descendant-or-self::*[not(@ewCombineInstanceFound)]")
        // If Not oExistElmt.OuterXml = oExistingInstance.FirstChild.OuterXml Then
        // CombineInstance_Sub2(oExistElmt, oMasterInstance)
        // End If
        // Next

        // CombineInstance_MarkSubElmts(oMasterInstance, True)


        // Catch ex As Exception
        // returnException(msException, mcModuleName, "CombineInstance", ex, "", "", gbDebug)

        // End Try
        // End Sub

        // Protected Sub CombineInstance_Sub1(ByRef oMasterElmt As XmlElement, ByVal oExistingInstance As XmlElement, ByVal cXPath As String)
        // 'Looks for stuff with the same xpath
        // Try
        // If Not cXPath = "" Then cXPath = cXPath & "/"
        // cXPath &= oMasterElmt.Name

        // Dim oInstanceElmt As XmlElement = oExistingInstance.SelectSingleNode(cXPath)
        // If Not oInstanceElmt Is Nothing Then
        // 'Master Attributes first
        // For i As Integer = 0 To oMasterElmt.Attributes.Count - 1
        // Dim oMasterAtt As XmlAttribute = oMasterElmt.Attributes(i)
        // Dim oInstanceAtt As XmlAttribute = oInstanceElmt.Attributes.ItemOf(oMasterAtt.Name)
        // If Not oInstanceAtt Is Nothing Then oMasterAtt.Value = oInstanceAtt.Value
        // Next
        // 'Now Existing
        // For i As Integer = 0 To oExistingInstance.Attributes.Count - 1
        // Dim oInstanceAtt As XmlAttribute = oExistingInstance.Attributes(i)
        // Dim oMasterAtt As XmlAttribute = oMasterElmt.Attributes.ItemOf(oInstanceAtt.Name)
        // If oMasterAtt Is Nothing Then oMasterElmt.SetAttribute(oInstanceAtt.Name, oInstanceAtt.Value)
        // Next
        // 'Now we need to check child nodes
        // If oMasterElmt.SelectNodes("*").Count > 0 Then
        // For Each oChild As XmlElement In oMasterElmt.SelectNodes("*")
        // CombineInstance_Sub1(oChild, oExistingInstance, cXPath)
        // Next
        // ElseIf oInstanceElmt.SelectNodes("*").Count > 0 Then
        // For Each oChild As XmlElement In oInstanceElmt.SelectNodes("*")
        // oMasterElmt.AppendChild(oMasterElmt.OwnerDocument.ImportNode(oChild, True))
        // Next
        // CombineInstance_MarkSubElmts(oInstanceElmt)
        // ElseIf Not oInstanceElmt.InnerText = "" Then
        // oMasterElmt.InnerText = oInstanceElmt.InnerText
        // End If
        // 'now mark both elements as found
        // oMasterElmt.SetAttribute("ewCombineInstanceFound", "TRUE")
        // oInstanceElmt.SetAttribute("ewCombineInstanceFound", "TRUE")

        // 'We wont go for others just yet as we want to complete this before finding
        // 'other less obvious stuff

        // End If

        // Catch ex As Exception
        // returnException(msException, mcModuleName, "CombineInstance_Sub1", ex, "", "", gbDebug)
        // End Try
        // End Sub

        // Protected Sub CombineInstance_Sub2(ByRef oExisting As XmlElement, ByVal oMasterInstance As XmlElement)
        // 'Gets any unmarked in Existing data and copies over to new
        // Try
        // Dim cXPath As String = ""
        // CombineInstance_GetXPath(oExisting, cXPath)
        // Dim oParts() As String = Split(cXPath, "/")
        // Dim oRefPart As XmlElement = oMasterInstance
        // For i As Integer = 0 To oParts.Length - 2
        // If Not oRefPart.SelectSingleNode(oParts(i)) Is Nothing Then
        // oRefPart = oRefPart.SelectSingleNode(oParts(i))
        // Else
        // Dim oElmt As XmlElement = oMasterInstance.OwnerDocument.CreateElement(oParts(i))
        // oRefPart.AppendChild(oElmt)
        // oRefPart = oElmt
        // End If
        // Next
        // 'still need to create last part
        // oRefPart.AppendChild(oMasterInstance.OwnerDocument.ImportNode(oExisting, True))
        // CombineInstance_MarkSubElmts(oExisting)
        // Catch ex As Exception
        // returnException(msException, mcModuleName, "CombineInstance_Sub2", ex, "", "", gbDebug)
        // End Try
        // End Sub

        // Private Sub CombineInstance_GetXPath(ByVal oElmt As XmlElement, ByRef xPath As String)
        // Try
        // If xPath = "" Then xPath = oElmt.Name
        // If Not oElmt.ParentNode Is Nothing Then
        // If Not oElmt.ParentNode.Name = "instance" Then
        // xPath = oElmt.ParentNode.Name & "/" & xPath
        // CombineInstance_GetXPath(oElmt.ParentNode, xPath)
        // End If
        // End If
        // Catch ex As Exception
        // returnException(msException, mcModuleName, "CombineInstance_GetXPath", ex, "", , gbDebug)
        // End Try
        // End Sub

        // Private Sub CombineInstance_MarkSubElmts(ByRef oElmt As XmlElement, Optional ByVal bRemove As Boolean = False)
        // Try
        // If bRemove Then
        // oElmt.RemoveAttribute("ewCombineInstanceFound")
        // Else
        // oElmt.SetAttribute("ewCombineInstanceFound", "TRUE")
        // End If

        // For Each oChild As XmlElement In oElmt.SelectNodes("*")
        // CombineInstance_MarkSubElmts(oChild, bRemove)
        // Next
        // Catch ex As Exception
        // returnException(msException, mcModuleName, "CombineInstance_MarkSubElmts", ex, "", "", gbDebug)
        // End Try
        // End Sub

        public void AddValidationError(string errorText)
        {

            cValidationError += errorText;

        }

        public string getXpathFromQueryXml(XmlElement oInstance, string sXsltPath = "")
        {
            Protean.XmlHelper.Transform oTransform = new Protean.XmlHelper.Transform();

            System.IO.TextWriter sWriter = new System.IO.StringWriter();
            XmlWriter sWritertest = XmlWriter.Create(sWriter);
            if (sXsltPath != "")
            {
                oTransform.Compiled = false;
                oTransform.XSLFile = goServer.MapPath(sXsltPath);
                XmlDocument oXML = new XmlDocument();
                oXML.InnerXml = oInstance.OuterXml;
                XmlReader oXMLReader = new XmlNodeReader(oXML);
                oTransform.Process(oXMLReader,ref sWritertest);
                // Run transformation


                oInstance.InnerXml = sWriter.ToString();
                sWriter.Close();
                sWriter = null;
            }

            // build an xpath query based on the xform instance
            // Example:

            // <instance>
            // <Query ewCmd="xpathSearch" xPathMask="//*[VARating>$RequiredVA and Uptime>$Uptime and Format=$Format]">
            // <RequiredVA>100</RequiredVA>
            // <Redundancy>Yes</Redundancy>
            // <Uptime>20</Uptime>
            // <Format>Rack</Format>
            // </Query>
            // </instance>

            string sXpath;

            // :TODO [TS] handle the instance tranform if xsl provided

            sXpath = oInstance.SelectSingleNode("Query/@xPathMask").InnerText;
            //XmlElement oElmt;
            foreach (XmlElement oElmt in oInstance.SelectNodes("Query/*"))
                // step through each of the child nodes and replace values in the xPathMask
                sXpath = sXpath.Replace("$" + oElmt.Name, "\"" + oElmt.InnerText + "\"");

            return sXpath;
        }

    }
}