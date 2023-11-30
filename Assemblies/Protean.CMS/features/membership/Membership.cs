using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Web.Configuration;

using System.Xml;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

using static Protean.stdTools;

namespace Protean
{


    public partial class Cms
    {
        public partial class Membership
        {

            #region Declarations

            private Cms myWeb;
            public event OnErrorEventHandler OnError;

            public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);
            public event OnErrorWithWebEventHandler OnErrorWithWeb;

            public delegate void OnErrorWithWebEventHandler(ref Cms myweb, object sender, Tools.Errors.ErrorEventArgs e);
            private const string mcModuleName = "Eonic.EonicWeb.Membership.Membership";

            #endregion

            #region Public Procedures

            public Membership(ref Cms oWeb)
            {
                try
                {
                    myWeb = oWeb;
                }
                catch (Exception ex)
                {

                }
            }

            public string AccountResetLink(int AccountID)
            {
                try
                {
                    // RJP 7 Nov 2012. Added LCase to MembershipEncryption.
                    string cLink = Strings.Trim(Tools.Encryption.HashString(Strings.UCase(Conversions.ToString(DateTime.Now)), Strings.LCase(myWeb.moConfig["MembershipEncryption"]), true));
                    string cSQL = "UPDATE tblDirectory SET cDirPassword = '" + cLink + "' WHERE nDirKey = " + AccountID;
                    cLink = Tools.Text.AscString(cLink);
                    Debug.WriteLine(cLink);
                    if (Information.IsNumeric((object)myWeb.moDbHelper.ExeProcessSql(cSQL)))
                    {
                        return cLink;
                    }
                    else
                    {
                        return "";
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "AccountResetLink", ex, ""));
                    return "";
                }
            }

            /// <summary>
            /// 'We need to activate the link create a hash, We store the activation hash in the UserXML
            /// </summary>
            /// <param name="AccountID"></param>
            /// <returns></returns>
            /// <remarks>Cannot store in password field because we need this, We could store in the audit description, allthough don't think this is indexed. Storing in the userXml would be inefficient to search however it could be useful because it is contained in the email</remarks>

            public string AccountActivateLink(int AccountID)
            {
                try
                {
                    var oGuid = Guid.NewGuid();

                    string cLink = oGuid.ToString();

                    var oUserXml = new XmlDocument();
                    oUserXml.AppendChild(oUserXml.CreateElement("instance"));
                    oUserXml.DocumentElement.InnerXml = myWeb.moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.Directory, (long)AccountID);
                    XmlElement oUserElmt = (XmlElement)oUserXml.DocumentElement.SelectSingleNode("tblDirectory/cDirXml/User");
                    var ActivationKeyElmt = oUserXml.CreateElement("ActivationKey");

                    ActivationKeyElmt.InnerText = cLink;

                    oUserElmt.AppendChild(ActivationKeyElmt);
                    myWeb.moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Directory, oUserXml.DocumentElement, (long)AccountID);

                    return cLink;
                }


                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "AccountActivateLink", ex, ""));
                    return "";
                }
            }

            public int DecryptResetLink(int AccountID, string EncryptedString)
            {
                try
                {
                    EncryptedString = Tools.Text.DeAscString(EncryptedString);

                    string cSQL = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("SELECT tblDirectory.nDirKey FROM tblDirectory INNER JOIN tblAudit ON tblDirectory.nAuditId = tblAudit.nAuditKey WHERE cDirPassword = '", SqlFmt(EncryptedString)), "' AND nDirKey = "), AccountID));
                    return Conversions.ToInteger(myWeb.moDbHelper.GetDataValue(cSQL, CommandType.Text, null, (object)0));
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "DecryptResetLink", ex, ""));
                    return 0;
                }
            }

            public bool CheckPasswordHistory(int AccountID, string cPassword)
            {
                // Dim odr As SqlDataReader
                bool valid = true;
                try
                {

                    if (Strings.LCase(myWeb.moConfig["MembershipEncryption"]) == "md5salt")
                    {
                        string cSalt = Tools.Encryption.generateSalt();
                        string inputPassword = string.Concat(cSalt, cPassword); // Take the users password and add the salt at the front
                                                                                // Note leave the value below hard coded md5.
                        string md5Password = Tools.Encryption.HashString(inputPassword, "md5", true); // Md5 the marged string of the password and salt
                        string resultPassword = string.Concat(md5Password, ":", cSalt); // Adds the salt to the end of the hashed password
                        cPassword = resultPassword; // Store the resultant password with salt in the database
                    }
                    else
                    {

                        cPassword = Tools.Encryption.HashString(cPassword, Strings.LCase(myWeb.moConfig["MembershipEncryption"]), true);

                    } // plain - md5 - sha1

                    // ensure the password is XML safe
                    var oConvDoc = new XmlDocument();
                    var oConvElmt = oConvDoc.CreateElement("PW");
                    oConvElmt.InnerText = cPassword;
                    cPassword = oConvElmt.InnerXml;

                    XmlElement moPolicy;
                    moPolicy = (XmlElement)WebConfigurationManager.GetWebApplicationSection("protean/PasswordPolicy");
                    int nHistoricPasswordCount = Conversions.ToInteger("0" + moPolicy.FirstChild.SelectSingleNode("blockHistoricPassword").InnerText);

                    string sSql2 = "select cActivityDetail as password, dDatetime from tblActivityLog where nActivityType=" + ((int)Cms.dbHelper.ActivityType.HistoricPassword).ToString() + " and nUserDirId = " + AccountID + " Order By dDateTime Desc";

                    using (var oDr = myWeb.moDbHelper.getDataReaderDisposable(sSql2))  // Done by nita on 6/7/22
                    {
                        while (oDr.Read())
                        {
                            if (Conversions.ToBoolean(Operators.AndObject(nHistoricPasswordCount > 0, Operators.ConditionalCompareObjectEqual(cPassword, oDr["password"], false))))
                            {
                                valid = false;
                            }
                            nHistoricPasswordCount = nHistoricPasswordCount - 1;
                        }

                        return valid;
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "CheckPasswordHistory", ex, ""));
                    return false;
                }
            }



            public bool ReactivateAccount(int AccountID, string cPassword)
            {
                try
                {

                    // cPassword = Protean.Tools.Encryption.HashString(cPassword, myWeb.moConfig("MembershipEncryption"), True)

                    // RJP 7 Nov 2012. Added LCase to MembershipEncryption. Note leave the value below for md5Password hard coded as md5.
                    if (Strings.LCase(myWeb.moConfig["MembershipEncryption"]) == "md5salt")
                    {
                        string cSalt = Tools.Encryption.generateSalt();
                        string inputPassword = string.Concat(cSalt, cPassword); // Take the users password and add the salt at the front
                                                                                // Note leave the value below hard coded md5.
                        string md5Password = Tools.Encryption.HashString(inputPassword, "md5", true); // Md5 the marged string of the password and salt
                        string resultPassword = string.Concat(md5Password, ":", cSalt); // Adds the salt to the end of the hashed password
                        cPassword = resultPassword; // Store the resultant password with salt in the database
                    }
                    else
                    {
                        cPassword = Tools.Encryption.HashString(cPassword, Strings.LCase(myWeb.moConfig["MembershipEncryption"]), true);
                    } // plain - md5 - sha1

                    // ensure the password is XML safe
                    var oConvDoc = new XmlDocument();
                    var oConvElmt = oConvDoc.CreateElement("PW");
                    oConvElmt.InnerText = cPassword;
                    cPassword = oConvElmt.InnerXml;

                    XmlElement moPolicy;
                    moPolicy = (XmlElement)WebConfigurationManager.GetWebApplicationSection("protean/PasswordPolicy");
                    int nHistoricPasswordCount = Conversions.ToInteger("0" + moPolicy.FirstChild.SelectSingleNode("blockHistoricPassword").InnerText);
                    if (nHistoricPasswordCount > 0)
                    {
                        myWeb.moDbHelper.logActivity(Cms.dbHelper.ActivityType.HistoricPassword, (long)AccountID, 0L, 0L, cPassword, cForiegnRef: "");
                    }

                    string cSQL = "UPDATE tblDirectory SET cDirPassword = '" + cPassword + "' WHERE nDirKey = " + AccountID;
                    if (myWeb.moDbHelper.ExeProcessSql(cSQL) > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ReactivateAccount", ex, ""));
                    return false;
                }
            }

            public bool ActivateAccount(string cLink)
            {
                try
                {
                    long userId;

                    // lets get the userId form the hash supplied
                    string cSQL = "SELECT tblDirectory.nDirKey FROM tblDirectory INNER JOIN tblAudit ON tblDirectory.nAuditId = tblAudit.nAuditKey WHERE cDirXml LIKE '%<ActivationKey>" + cLink + "</ActivationKey>%'";
                    userId = Conversions.ToLong(myWeb.moDbHelper.GetDataValue(cSQL, CommandType.Text, null, (object)0));

                    if (userId > 0L)
                    {
                        // change the account status
                        myWeb.moDbHelper.setObjectStatus(Cms.dbHelper.objectTypes.Directory, Cms.dbHelper.Status.Live, userId);
                        // remove the activation key from userXML
                        var oUserXml = new XmlDocument();
                        var oUserInstance = oUserXml.CreateElement("Instance");
                        oUserXml.AppendChild(oUserInstance);
                        oUserInstance.InnerXml = myWeb.moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.Directory, userId);
                        XmlElement ActivationKeyElmt;
                        ActivationKeyElmt = (XmlElement)oUserInstance.FirstChild.SelectSingleNode("cDirXml/User/ActivationKey");
                        ActivationKeyElmt.ParentNode.RemoveChild(ActivationKeyElmt);
                        myWeb.moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Directory, oUserXml.DocumentElement, userId);

                        myWeb.moDbHelper.CommitLogToDB(Cms.dbHelper.ActivityType.Register, (int)userId, myWeb.moSession.SessionID, DateTime.Now, 0, 0, "Activate");
                        switch (myWeb.moConfig["ActivateBehaviour"] ?? "")
                        {
                            case "LogonReload":
                                {
                                    myWeb.mnUserId = (int)userId;
                                    if (myWeb.moSession != null)
                                    {
                                        myWeb.moSession["nUserId"] = (object)myWeb.mnUserId;
                                    }
                                    // Add subscribe to subscribe to the subscription if we have one
                                    if (myWeb.mnArtId > 0)
                                    {
                                        myWeb.msRedirectOnEnd = myWeb.mcPageURL + "?artid=" + myWeb.mnArtId + "&ewCmd=Subscribe";
                                    }
                                    else
                                    {

                                        myWeb.msRedirectOnEnd = myWeb.mcPageURL + "?ewCmd=Subscribe";
                                    }

                                    break;
                                }

                            // todo
                            case "LogonRedirect":
                                {
                                    break;
                                }

                            default:
                                {
                                    break;
                                }
                                // do nuffin
                        }
                        return true;
                    }

                    else
                    {
                        return false;
                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ReactivateAccount", ex, ""));
                    return false;
                }
            }

            #endregion

            #region Secure Membership

            public System.Web.HttpCookie SetCookie(string Name, object Value, DateTime Expires = default)
            {
                try
                {
                    var Cookie = myWeb.moResponse.Cookies[Name];
                    if (Cookie is null)
                        Cookie = new System.Web.HttpCookie(Name);
                    Cookie.Value = Conversions.ToString(Value);
                    if (!string.IsNullOrEmpty(myWeb.moConfig["SecureMembershipDomain"]))
                    {
                        Cookie.Domain = myWeb.moConfig["SecureMembershipDomain"];
                    }

                    if (Expires == default)
                        Cookie.Expires = Expires;
                    myWeb.moResponse.Cookies.Add(Cookie);
                    return Cookie;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "SetCookie", ex, ""));
                    return null;
                }
            }



            public object CookieValue(string Name, object ValueIfNothing = null)
            {
                try
                {
                    if (myWeb.moRequest.Cookies[Name] != null)
                    {
                        return myWeb.moRequest.Cookies[Name].Value;
                    }
                    else
                    {
                        return ValueIfNothing;
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "CookieValue", ex, ""));
                    return null;
                }
            }

            public void SecureMembershipProcess(string cForceCommand)
            {
                try
                {
                    // Variables
                    string ASPSessionName = Conversions.ToString(Interaction.IIf(string.IsNullOrEmpty(myWeb.moConfig["ASPSessionName"]), "ASP.NET_SessionId", myWeb.moConfig["ASPSessionName"]));
                    string UserCookieName = Conversions.ToString(Interaction.IIf(string.IsNullOrEmpty(myWeb.moConfig["UserCookieName"]), "nUserId", myWeb.moConfig["UserCookieName"]));
                    string SecureMembershipAddress = myWeb.moConfig["SecureMembershipAddress"];
                    string SecureMembershipDomain = myWeb.moConfig["SecureMembershipDomain"];

                    // Session Cookie
                    System.Web.HttpContext.Current.Response.Cookies[ASPSessionName].Value = System.Web.HttpContext.Current.Session.SessionID;
                    if (!string.IsNullOrEmpty(SecureMembershipDomain))
                    {
                        System.Web.HttpContext.Current.Response.Cookies[ASPSessionName].Domain = SecureMembershipDomain;
                    }
                    // Path
                    string cPath = "" + myWeb.moRequest.QueryString["path"];
                    for (int i = 0, loopTo = myWeb.moRequest.QueryString.Count - 1; i <= loopTo; i++)
                    {
                        if (!(myWeb.moRequest.QueryString.Keys[i] == "path"))
                        {
                            cPath += "&" + myWeb.moRequest.QueryString.Keys[i];
                            cPath += "=" + myWeb.moRequest.QueryString[i];
                        }
                    }
                    // Check Request Cookie first
                    int nCookieUser = Conversions.ToInteger(CookieValue(UserCookieName, -1));
                    // Check what we are doing
                    if (Strings.LCase(cForceCommand) == "logoff")
                    {
                        // redirect to the logoff with no cookie
                        SetCookie(UserCookieName, 0, DateTime.Now.AddMinutes(-20));
                        if (!Strings.LCase(cPath).Contains(Strings.LCase("ewCmd=LogOff")))
                        {
                            if (cPath.Contains("?"))
                            {
                                cPath += "&";
                            }
                            else
                            {
                                cPath += "?";
                            }
                            cPath += "ewCmd=LogOff";
                        }
                        // Ultimate logoff cmd
                        myWeb.moSession["nUserId"] = (object)0;
                        myWeb.moSession.Abandon();
                        myWeb.mnUserId = 0;
                        myWeb.msRedirectOnEnd = myWeb.moConfig["SecureMembershipAddress"] + myWeb.moConfig["ProjectPath"] + "/";
                    }
                    else if (Strings.LCase(cForceCommand) == "logoffimpersonate")
                    {
                        // redirect to the logoff with no cookie
                        SetCookie(UserCookieName, 0, DateTime.Now.AddMinutes(-20));
                        myWeb.moSession["nUserId"] = (object)0;
                        myWeb.mnUserId = 0;
                    }
                    else if (nCookieUser > 0)
                    {
                        // logon with this user
                        myWeb.mnUserId = nCookieUser;
                    }

                    // Check to see if we need to do a redirect
                    string cHost = Conversions.ToString(Operators.ConcatenateObject(Interaction.IIf(myWeb.moRequest.ServerVariables["HTTPS"] == "on", "https://", "http://"), myWeb.moRequest.ServerVariables["HTTP_HOST"]));
                    var oVariants = new Hashtable();
                    oVariants.Add("0", cHost);
                    oVariants.Add("1", cHost + "/");
                    oVariants.Add("2", cHost + myWeb.moConfig["ProjectPath"]);
                    oVariants.Add("3", cHost + myWeb.moConfig["ProjectPath"] + "/");

                    // oVariants.Add("2", "http://" & cHost)
                    // oVariants.Add("3", "http://" & cHost & "/")

                    // If oVariants.ContainsValue(SecureMembershipAddress) And myWeb.mnUserId = 0 Then
                    // If Left(cPath, 1) = "/" And Right(myWeb.moConfig("HomeUrl"), 1) = "/" Then cPath = cPath.Remove(0, 1)
                    // myWeb.moResponse.Redirect(myWeb.moConfig("HomeUrl") & cPath)
                    // Else
                    if (!oVariants.ContainsValue(SecureMembershipAddress) & (myWeb.mnUserId > 0 | myWeb.mbAdminMode | Strings.LCase(myWeb.moRequest["ewCmd"]) == "ar"))
                    {
                        SetCookie(UserCookieName, (object)myWeb.mnUserId, DateTime.Now.AddMinutes(20d));
                        if (!(Strings.Right(SecureMembershipAddress, 1) == "/"))
                            SecureMembershipAddress += "/";
                        if (Strings.Left(cPath, 1) == "/" & Strings.Right(SecureMembershipAddress, 1) == "/")
                            cPath = cPath.Remove(0, 1);
                        if (cPath.StartsWith("&"))
                            cPath = "?" + cPath.Substring(1);
                        myWeb.msRedirectOnEnd = SecureMembershipAddress + cPath;
                    }
                    // myWeb.moResponse.Redirect(SecureMembershipAddress & cPath)
                    else if (oVariants.ContainsValue(SecureMembershipAddress) & myWeb.mnUserId > 0)
                    {
                        if (myWeb.moPageXml != null)
                        {
                            if (myWeb.moPageXml.DocumentElement != null)
                            {
                                myWeb.moPageXml.DocumentElement.SetAttribute("baseUrl", SecureMembershipAddress + myWeb.moConfig["ProjectPath"] + "/");
                            }
                        }
                        SetCookie(UserCookieName, (object)myWeb.mnUserId, DateTime.Now.AddMinutes(20d));
                    }
                    else if (myWeb.moRequest.ServerVariables["HTTPS"] == "on")
                    {
                        if (myWeb.moPageXml != null)
                        {
                            if (myWeb.moPageXml.DocumentElement != null)
                            {
                                // don't want this for everything just the logon form
                                myWeb.moPageXml.DocumentElement.SetAttribute("baseUrl", SecureMembershipAddress + myWeb.moConfig["ProjectPath"]);
                            }
                        }
                    }
                }


                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "SecureMembershipProcess", ex, ""));
                }
            }

            public void ProviderActions(ref Cms myWeb, string actionName)
            {
                string sProcessInfo = "";
                try
                {

                    var castObject = WebConfigurationManager.GetWebApplicationSection("protean/membershipProviders");
                    Protean.ProviderSectionHandler moPrvConfig = (Protean.ProviderSectionHandler)castObject;
                    foreach (var ourProvider in moPrvConfig.Providers)
                    {
                        if (ourProvider.parameters(actionName) != null)
                        {
                            Type calledType;
                            Assembly assemblyInstance;

                            if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(ourProvider.parameters("path"), "", false)))
                            {
                                assemblyInstance = Assembly.LoadFrom(myWeb.goServer.MapPath(Conversions.ToString(ourProvider.parameters("path"))));
                            }
                            else
                            {
                                assemblyInstance = Assembly.Load(ourProvider.Type);
                            }
                            if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(ourProvider.parameters("rootClass"), "", false)))
                            {
                                calledType = assemblyInstance.GetType(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("Protean.Providers.Membership.", ourProvider.Name), "Tools.Actions")), true);
                            }
                            else
                            {
                                calledType = assemblyInstance.GetType(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(ourProvider.parameters("rootClass"), ".Providers.Membership."), ourProvider.Name), "Tools.Actions")), true);
                            }

                            var o = Activator.CreateInstance(calledType);

                            var args = new object[1];
                            args[0] = myWeb;

                            calledType.InvokeMember(Conversions.ToString(ourProvider.parameters(actionName)), BindingFlags.InvokeMethod, null, o, args);

                        }
                    }
                }


                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ProviderActions", ex, ""));
                }
            }

            #endregion

            #region Module Behaviour

            public class Modules
            {

                public event OnErrorEventHandler OnError;

                public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);
                private const string mcModuleName = "Protean.Cms.Membership.Modules";

                public Modules()
                {

                    // do nowt

                }

                // sReturnValue = "LogOn"
                // moDbHelper.logActivity(dbHelper.ActivityType.Logon, mnUserId, mnPageId, mnArtId)
                // myWeb.moDbHelper.CommitLogToDB(dbHelper.ActivityType.Logon, myWeb.mnUserId, myWeb.moSession.SessionID, Now, 0, 0, "")

                public void Logon(ref Cms myWeb, ref XmlElement oContentNode)
                {
                    try
                    {
                        var adXfm = myWeb.getAdminXform();
                        adXfm.open(myWeb.moPageXml);
                        XmlElement oXfmElmt;
                        string sReturnValue = null;
                        string cLogonCmd = "";

                        if (myWeb.mnUserId == 0 & (myWeb.moRequest["ewCmd"] != "passwordReminder" & myWeb.moRequest["ewCmd"] != "ActivateAccount"))
                        {

                            oXfmElmt = (XmlElement)adXfm.xFrmUserLogon();
                            bool bAdditionalChecks = false;
                            if (Conversions.ToBoolean(!adXfm.valid))
                            {
                                // Call in additional authentication checks
                                if (myWeb.moConfig["AlternativeAuthentication"] == "On")
                                {
                                    bAdditionalChecks = myWeb.AlternativeAuthentication();
                                }
                            }

                            if (Conversions.ToBoolean(Operators.OrObject(adXfm.valid, bAdditionalChecks)))
                            {
                                myWeb.moContentDetail = (XmlElement)null;
                                // mnUserId = adXfm.mnUserId
                                if (myWeb.moSession != null)
                                    myWeb.moSession["nUserId"] = (object)myWeb.mnUserId;
                                if (myWeb.moRequest["cRemember"] == "true")
                                {
                                    var oCookie = new System.Web.HttpCookie("RememberMe");
                                    oCookie.Value = myWeb.mnUserId.ToString();
                                    oCookie.Expires = DateAndTime.DateAdd(DateInterval.Day, 60d, DateTime.Now);
                                    myWeb.moResponse.Cookies.Add(oCookie);
                                }
                                // Now we want to reload as permissions have changed
                                if (myWeb.moSession != null)
                                {
                                    if (myWeb.moSession["cLogonCmd"] != null)
                                    {
                                        cLogonCmd = Strings.Split(Conversions.ToString(myWeb.moSession["cLogonCmd"]), "=")[0];
                                        if (myWeb.mcOriginalURL.Contains(cLogonCmd + "="))
                                        {
                                            cLogonCmd = "";
                                        }
                                        else if (myWeb.mcOriginalURL.Contains("="))
                                        {
                                            cLogonCmd = Conversions.ToString(Operators.ConcatenateObject("&", myWeb.moSession["cLogonCmd"]));
                                        }
                                        else
                                        {
                                            cLogonCmd = Conversions.ToString(Operators.ConcatenateObject("?", myWeb.moSession["cLogonCmd"]));
                                        }
                                    }
                                }
                                myWeb.moSession["RedirectReason"] = "logonSuccessful";

                                var oMembership = new Membership(ref myWeb);
                                oMembership.ProviderActions(ref myWeb, "logonAction");

                                myWeb.logonRedirect(cLogonCmd);
                            }
                            else
                            {
                                oContentNode.InnerXml = oXfmElmt.InnerXml;
                            }
                        }

                        else if (myWeb.moRequest["ewCmd"] == "passwordReminder")
                        {
                            // RJP 7 Nov 2012. Amended to use Lower Case to prevent against case sensitive entries in Protean.Cms.Config.
                            switch (Strings.LCase(myWeb.moConfig["MembershipEncryption"]) ?? "")
                            {
                                case "md5":
                                case "sha1":
                                case "sha256":
                                    {
                                        oXfmElmt = (XmlElement)adXfm.xFrmResetAccount();
                                        break;
                                    }

                                default:
                                    {
                                        oXfmElmt = (XmlElement)adXfm.xFrmPasswordReminder();
                                        break;
                                    }
                            }
                            oContentNode.InnerXml = oXfmElmt.InnerXml;
                        }

                        else if (myWeb.moRequest["ewCmd"] == "ActivateAccount")
                        {

                            oXfmElmt = (XmlElement)adXfm.xFrmActivateAccount();
                            oContentNode.InnerXml = oXfmElmt.InnerXml;

                        }
                    }

                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "Logon", ex, ""));
                    }
                }

                public void Register(ref Cms myWeb, ref XmlElement oContentNode)
                {
                    try
                    {
                        myWeb.bPageCache = false;
                        long mnUserId = (long)myWeb.mnUserId;
                        bool mbAdminMode = myWeb.mbAdminMode;
                        var moPageXml = myWeb.moPageXml;
                        var moDbHelper = myWeb.moDbHelper;
                        var moSession = myWeb.moSession;
                        var moConfig = myWeb.moConfig;
                        var moRequest = myWeb.moRequest;

                        var goServer = myWeb.goServer;

                        string cLogonCmd = "";
                        bool bRedirectStarted = false;
                        XmlElement oXfmElmt;
                        string sProcessInfo;

                        string AccountCreateForm = "UserRegister";
                        string AccountUpdateForm = "UserMyAccount";

                        if (!string.IsNullOrEmpty(oContentNode.GetAttribute("accountCreateFormName")))
                            AccountCreateForm = oContentNode.GetAttribute("accountCreateFormName");
                        if (!string.IsNullOrEmpty(oContentNode.GetAttribute("accountUpdateFormName")))
                            AccountUpdateForm = oContentNode.GetAttribute("accountUpdateFormName");

                        bool bLogon = false;

                        Cms argmyWeb = myWeb;
                        var oMembershipProv = new Providers.Membership.BaseProvider(ref argmyWeb, myWeb.moConfig["MembershipProvider"]);
                        myWeb = (Cms)argmyWeb;
                        oMembershipProv.AdminXforms adXfm = oMembershipProv.AdminXforms;
                        bool bRedirect = true;


                        // OAuth Functionality

                        if (!!string.IsNullOrEmpty(moRequest["oAuthResp"]))
                        {
                            if (!string.IsNullOrEmpty(moRequest["oAuthReg"]) & string.IsNullOrEmpty(myWeb.msRedirectOnEnd))
                            {
                                object sRedirectPath = "";
                                object appId = "";
                                object redirectURI = "https://" + moRequest.ServerVariables["SERVER_NAME"] + myWeb.mcPageURL + "?oAuthResp=" + moRequest["oAuthReg"];
                                switch (moRequest["oAuthReg"] ?? "")
                                {
                                    case "facebook":
                                        {
                                            sRedirectPath = "https://www.facebook.com/v2.8/dialog/oauth?";
                                            appId = moConfig["OauthFacebookId"];
                                            sRedirectPath = Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(sRedirectPath, "client_id="), appId), "&redirect_uri="), redirectURI);
                                            break;
                                        }
                                    case "twitter":
                                        {
                                            var twApi = new Integration.Directory.Twitter(ref myWeb);
                                            twApi.twitterConsumerKey = moConfig["OauthTwitterId"];
                                            twApi.twitterConsumerSecret = moConfig["OauthTwitterKey"];
                                            sRedirectPath = twApi.GetRequestToken();
                                            break;
                                        }
                                }
                                myWeb.msRedirectOnEnd = Conversions.ToString(sRedirectPath);
                            }
                            else
                            {

                            }
                        }
                        else
                        {
                            switch (moRequest["oAuthResp"] ?? "")
                            {
                                case "facebook":
                                    {
                                        object redirectURI = "http://" + moRequest.ServerVariables["SERVER_NAME"] + myWeb.mcPageURL + "?oAuthResp=facebook";
                                        sProcessInfo = "Facebook Response";
                                        var fbClient = new Integration.Directory.Facebook(ref myWeb, moConfig["OauthFacebookId"], moConfig["OauthFacebookKey"]);
                                        List<Integration.Directory.Facebook.User> fbUsers;
                                        fbUsers = fbClient.GetFacebookUserData(moRequest["code"], Conversions.ToString(redirectURI));
                                        sProcessInfo = fbUsers[0].first_name + " " + fbUsers[0].last_name;

                                        var tmp = fbUsers;
                                        var argfbUser = tmp[0];
                                        mnUserId = fbClient.CreateUser(ref argfbUser);
                                        tmp[0] = argfbUser;

                                        if (moSession != null)
                                            moSession["nUserId"] = (object)mnUserId;
                                        moDbHelper.CommitLogToDB(Cms.dbHelper.ActivityType.Register, (int)mnUserId, moSession.SessionID, DateTime.Now, 0, 0, "First Logon");

                                        bLogon = true;
                                        break;
                                    }

                                case "twitter":
                                    {
                                        sProcessInfo = "Twitter Response";
                                        var twApi = new Integration.Directory.Twitter(ref myWeb);
                                        twApi.twitterConsumerKey = moConfig["OauthTwitterId"];
                                        twApi.twitterConsumerSecret = moConfig["OauthTwitterKey"];
                                        break;
                                    }
                                    // Get twitter user
                                    // Dim twUsers As List(Of Protean.Integration.Directory.Twitter.User)   'Never used


                            }
                            if (mnUserId > 0L)
                            {



                            }


                        }

                        // If not in admin mode then base our choice on whether the user is logged in. 
                        // If in Admin Mode, then present it as WYSIWYG
                        if (!myWeb.mbAdminMode & myWeb.mnUserId > 0)
                        {

                            XmlElement oContentForm = (XmlElement)myWeb.moPageXml.SelectSingleNode("descendant-or-self::Content[@type='xform' and @name='UserMyAccount']");
                            if (oContentForm is null)
                            {
                                oXfmElmt = (XmlElement)adXfm.xFrmEditDirectoryItem(myWeb.mnUserId, "User", default, AccountUpdateForm);
                            }
                            else
                            {
                                oXfmElmt = (XmlElement)adXfm.xFrmEditDirectoryItem(myWeb.mnUserId, "User", default, AccountUpdateForm, oContentForm.OuterXml);
                                if (!myWeb.mbAdminMode)
                                    oContentForm.ParentNode.RemoveChild(oContentForm);
                            }
                            oContentNode.InnerXml = oXfmElmt.InnerXml;
                        }

                        else
                        {

                            XmlElement oContentForm = (XmlElement)moPageXml.SelectSingleNode("descendant-or-self::Content[@type='xform' and @name='UserRegister']");
                            if (oContentForm is null)
                            {
                                oXfmElmt = (XmlElement)adXfm.xFrmEditDirectoryItem(mnUserId, "User", default, AccountCreateForm);
                            }
                            else
                            {
                                oXfmElmt = (XmlElement)adXfm.xFrmEditDirectoryItem(mnUserId, "User", default, AccountCreateForm, oContentForm.OuterXml);
                                if (!mbAdminMode)
                                    oContentForm.ParentNode.RemoveChild(oContentForm);
                            }

                            if (!string.IsNullOrEmpty(myWeb.moConfig["SecureMembershipAddress"]) & myWeb.mbAdminMode == false)
                            {
                                XmlElement oSubElmt = (XmlElement)adXfm.moXformElmt.SelectSingleNode("descendant::submission");
                                oSubElmt.SetAttribute("action", myWeb.moConfig["SecureMembershipAddress"] + myWeb.mcPagePath);
                            }

                            // ok if the user is valid we then need to handle what happens next.
                            if (Conversions.ToBoolean(adXfm.valid))
                            {

                                switch (myWeb.moConfig["RegisterBehaviour"] ?? "")
                                {

                                    case "validateByEmail":
                                        {
                                            // don't redirect because we want to reuse this form
                                            bRedirect = false;

                                            // say thanks for registering and update the form

                                            // hide the current form
                                            XmlElement oFrmGrp = (XmlElement)adXfm.moXformElmt.SelectSingleNode("group");
                                            oFrmGrp.SetAttribute("class", "hidden");
                                            // create a new note
                                            XmlElement oFrmGrp2 = (XmlElement)adXfm.addGroup(adXfm.moXformElmt, "validateByEmail");
                                            adXfm.addNote(oFrmGrp2, Protean.xForm.noteTypes.Hint, "<span class=\"msg-1029\">Thanks for registering you have been sent an email with a link you must click to activate your account</span>", (object)true);

                                            // lets get the new userid from the instance
                                            mnUserId = Conversions.ToLong(adXfm.instance.SelectSingleNode("tblDirectory/nDirKey").InnerText);

                                            // first we set the user account to be pending
                                            moDbHelper.setObjectStatus(Cms.dbHelper.objectTypes.Directory, Cms.dbHelper.Status.Pending, mnUserId);

                                            var oMembership = new Membership(ref myWeb);
                                            oMembership.OnError += myWeb.OnComponentError;
                                            oMembership.AccountActivateLink((int)mnUserId);

                                            moDbHelper.CommitLogToDB(Cms.dbHelper.ActivityType.Register, (int)mnUserId, moSession.SessionID, DateTime.Now, 0, 0, "Send Activation"); // Auto logon
                                            break;
                                        }

                                    default:
                                        {
                                            mnUserId = Conversions.ToLong(adXfm.instance.SelectSingleNode("tblDirectory/nDirKey").InnerText);
                                            if (moSession != null)
                                                moSession["nUserId"] = (object)mnUserId;

                                            moDbHelper.CommitLogToDB(Cms.dbHelper.ActivityType.Register, (int)mnUserId, moSession.SessionID, DateTime.Now, 0, 0, "First Logon");

                                            bLogon = true;
                                            break;
                                        }

                                }

                                // send registration confirmation
                                string xsltPath = "/xsl/email/registration.xsl";
                                if (myWeb.moConfig["cssFramework"] == "bs5")
                                {
                                    xsltPath = "/xsl/registration.xsl";
                                }

                                if (System.IO.File.Exists(goServer.MapPath(xsltPath)))
                                {
                                    var oUserElmt = moDbHelper.GetUserXML(mnUserId);

                                    var oElmtPwd = moPageXml.CreateElement("Password");
                                    oElmtPwd.InnerText = moRequest["cDirPassword"];
                                    oUserElmt.AppendChild(oElmtPwd);

                                    oUserElmt.SetAttribute("Url", myWeb.mcOriginalURL);

                                    XmlElement oUserEmail = (XmlElement)oUserElmt.SelectSingleNode("Email");
                                    string fromName = moConfig["SiteAdminName"];
                                    string fromEmail = moConfig["SiteAdminEmail"];
                                    string recipientEmail = "";
                                    if (oUserEmail != null)
                                        recipientEmail = oUserEmail.InnerText;
                                    string SubjectLine = "Your Registration Details";
                                    var oMsg = new Protean.Messaging(ref myWeb.msException);
                                    // send an email to the new registrant
                                    if (!string.IsNullOrEmpty(recipientEmail))
                                    {
                                        Cms.dbHelper argodbHelper = null;
                                        sProcessInfo = Conversions.ToString(oMsg.emailer(oUserElmt, xsltPath, fromName, fromEmail, recipientEmail, SubjectLine, odbHelper: ref argodbHelper, "Message Sent", "Message Failed"));
                                    }

                                    // send an email to the webadmin
                                    if (string.IsNullOrEmpty(moConfig["RegistrationAlertEmail"]))
                                    {
                                        recipientEmail = moConfig["SiteAdminEmail"];
                                    }
                                    else
                                    {
                                        recipientEmail = moConfig["RegistrationAlertEmail"];
                                    }

                                    string xsltPathAlert = "/xsl/email/registrationAlert.xsl";
                                    if (myWeb.moConfig["cssFramework"] == "bs5")
                                    {
                                        xsltPath = "/email/registrationAlert.xsl";
                                    }

                                    if (System.IO.File.Exists(goServer.MapPath(moConfig["ProjectPath"] + xsltPathAlert)))
                                    {
                                        Cms.dbHelper argodbHelper1 = null;
                                        sProcessInfo = Conversions.ToString(oMsg.emailer(oUserElmt, moConfig["ProjectPath"] + xsltPathAlert, "New User", recipientEmail, fromEmail, SubjectLine, odbHelper: ref argodbHelper1, "Message Sent", "Message Failed"));
                                    }
                                    oMsg = (Protean.Messaging)null;
                                }

                                // redirect to this page or alternative page.
                                if (bRedirect)
                                {
                                    myWeb.msRedirectOnEnd = myWeb.mcOriginalURL;
                                }
                                else
                                {
                                    oContentNode.InnerXml = oXfmElmt.InnerXml;
                                }
                            }
                            else
                            {
                                oContentNode.InnerXml = oXfmElmt.InnerXml;
                            }
                        }

                        if (bLogon)
                        {
                            // Now we want to reload as permissions have changed

                            if (moSession != null)
                            {
                                if (moSession["cLogonCmd"] != null)
                                {
                                    cLogonCmd = Strings.Split(Conversions.ToString(moSession["cLogonCmd"]), "=")[0];
                                    if (myWeb.mcOriginalURL.Contains(cLogonCmd + "="))
                                    {
                                        cLogonCmd = "";
                                    }
                                    else if (myWeb.mcOriginalURL.Contains("="))
                                    {
                                        cLogonCmd = Conversions.ToString(Operators.ConcatenateObject("&", moSession["cLogonCmd"]));
                                    }
                                    else
                                    {
                                        cLogonCmd = Conversions.ToString(Operators.ConcatenateObject("?", moSession["cLogonCmd"]));
                                    }
                                }
                            }

                            moSession["RedirectReason"] = "registration";
                            myWeb.bRedirectStarted = true; // This acts as a local suppressant allowing for the sessio to pass through to the redirected page

                            string redirectId = myWeb.moConfig["RegisterRedirectPageId"];

                            if (!string.IsNullOrEmpty(oContentNode.GetAttribute("redirectPathId")))
                            {
                                redirectId = oContentNode.GetAttribute("redirectPathId");
                            }

                            if (!string.IsNullOrEmpty(redirectId))
                            {
                                // refresh the site strucutre with new userId
                                myWeb.mnUserId = (int)mnUserId;
                                myWeb.GetStructureXML("Site");
                                XmlElement oElmt = (XmlElement)myWeb.moPageXml.SelectSingleNode("/Page/Menu/descendant-or-self::MenuItem[@id = '" + redirectId + "']");
                                string redirectPath = myWeb.mcOriginalURL;
                                if (oElmt is null)
                                {
                                    myWeb.msRedirectOnEnd = redirectPath;
                                    bRedirect = true;
                                }
                                else
                                {
                                    redirectPath = oElmt.GetAttribute("url");
                                    myWeb.msRedirectOnEnd = redirectPath;
                                    bRedirect = false;
                                }
                            }
                        }
                    }

                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "Logon", ex, ""));
                    }
                }

                public void PasswordReminder(ref Cms myWeb, ref XmlElement oContentNode)
                {

                    var moConfig = myWeb.moConfig;
                    try
                    {
                        var adXfm = myWeb.getAdminXform();

                        XmlElement oXfmElmt;
                        if (myWeb.mnUserId == 0)
                        {
                            switch (Strings.LCase(moConfig["MembershipEncryption"]) ?? "")
                            {
                                case "md5salt":
                                case "md5":
                                case "sha1":
                                case "sha256":
                                    {
                                        if (myWeb.moRequest["ewCmd"] == "AR-MOD")
                                        {
                                            string cAccountHash = myWeb.moRequest["AI"];
                                            // Validate to prevent SQL injection
                                            // strip out any spaces to prevent SQL injection
                                            if (cAccountHash.Contains(" "))
                                            {
                                                cAccountHash = Strings.Left(cAccountHash, Strings.InStr(cAccountHash, " "));
                                            }
                                            if (cAccountHash.Contains("%20"))
                                            {
                                                cAccountHash = Strings.Left(cAccountHash, Strings.InStr(cAccountHash, "%20"));
                                            }
                                            var regex = new System.Text.RegularExpressions.Regex(@"[\d,]");

                                            if (!regex.Match(cAccountHash).Success)
                                            {
                                                cAccountHash = "";
                                            }

                                            if (!string.IsNullOrEmpty(cAccountHash))
                                            {
                                                oXfmElmt = (XmlElement)adXfm.xFrmConfirmPassword(cAccountHash);
                                            }
                                            else
                                            {
                                                oXfmElmt = (XmlElement)adXfm.xFrmResetAccount();
                                            }
                                        }
                                        else
                                        {
                                            oXfmElmt = (XmlElement)adXfm.xFrmResetAccount();
                                        }

                                        break;
                                    }

                                default:
                                    {
                                        oXfmElmt = (XmlElement)adXfm.xFrmPasswordReminder();
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            oXfmElmt = (XmlElement)adXfm.xFrmConfirmPassword(myWeb.mnUserId);
                        }


                        oContentNode.InnerXml = oXfmElmt.InnerXml;
                    }

                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "Logon", ex, ""));
                    }
                }

                public void GetUserContent(ref Cms myWeb, ref XmlElement oContentNode)
                {
                    string ContentType = oContentNode.GetAttribute("contentType");
                    string UserId = myWeb.mnUserId.ToString();
                    string WhereSql = " cContentSchemaName = '" + ContentType + "' and nInsertDirId = " + UserId;

                    try
                    {
                        if (Conversions.ToDouble(UserId) != 0d)
                        {
                            myWeb.mbAdminMode = true;
                            XmlElement argoPageDetail = null;
                            int ncount = 0;
                            myWeb.GetPageContentFromSelect(WhereSql,ref ncount, oContentsNode: ref oContentNode, oPageDetail: ref argoPageDetail,false, cOrderBy: "", distinct: true);
                            myWeb.mbAdminMode = false;
                        }
                    }

                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetUserContent", ex, ""));
                    }
                }


                public void GetUsersByGroup(ref Cms myWeb, ref XmlElement contentNode)
                {

                    try
                    {

                        string cSchemaName = contentNode.Attributes["parentSchemaType"].InnerText;
                        string sArrDirKeys = contentNode.Attributes["parentIds"].InnerText; // comma separated int array
                        int nStatus = Conversions.ToInteger(contentNode.Attributes["parentStatus"].InnerText);
                        string nParDirId = contentNode.Attributes["parentParId"].InnerText;

                        if (sArrDirKeys.Length > 0)
                        {

                            // make SQL statement
                            var strSql = new System.Text.StringBuilder();
                            strSql.Append("CREATE TABLE #tmpTblUsers ");
                            strSql.Append("(");
                            strSql.Append("[id] INT, ");
                            strSql.Append("[Status] INT, ");
                            strSql.Append("[Username] NVARCHAR(100), ");
                            strSql.Append("[UserXml] NVARCHAR(max), ");
                            strSql.Append("[Companies] NVARCHAR(100) ");
                            strSql.Append(") ");

                            // gets all users using standard stored proc
                            strSql.Append("INSERT INTO #tmpTblUsers ");
                            strSql.Append("EXEC spGetUsers " + nParDirId.ToString() + ", " + nStatus.ToString() + " ");

                            // narrows scope of users
                            strSql.Append("SELECT ");
                            strSql.Append("#tmpTblUsers.[id] ");
                            strSql.Append("FROM tblDirectory AS tblGroups ");
                            strSql.Append("INNER JOIN tblDirectoryRelation ON tblGroups.nDirKey = tblDirectoryRelation.nDirParentId ");
                            strSql.Append("INNER JOIN #tmpTblUsers ON tblDirectoryRelation.nDirChildId = #tmpTblUsers.id ");
                            strSql.Append("WHERE (tblGroups.cDirSchema = '" + cSchemaName + "') ");
                            strSql.Append(" AND (tblGroups.nDirKey in (" + sArrDirKeys + ")) ");

                            // add each result to content node xml
                            var oDsUsers = myWeb.moDbHelper.GetDataSet(strSql.ToString(), "User", "UserGroups");
                            foreach (DataRow oDrUser in oDsUsers.Tables[0].Rows)
                                contentNode.AppendChild(myWeb.GetUserXML(Conversions.ToLong(oDrUser["id"])).Clone());

                        }
                    }


                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "GetUsersByGroup", ex, "", "", gbDebug);
                        // Return Nothing
                    }

                }


                public void UserContact(ref Cms myWeb, ref XmlElement contentNode)
                {

                    try
                    {

                        if (myWeb.mnUserId > 0)
                        {

                            var adXfm = myWeb.getAdminXform();
                            adXfm.open(myWeb.moPageXml);
                            string memCmd = myWeb.moRequest["memCmd"];
                            if (string.IsNullOrEmpty(memCmd) & !string.IsNullOrEmpty(myWeb.moRequest["ewCmd"]))
                            {
                                memCmd = myWeb.moRequest["ewCmd"];
                            }

                            switch (memCmd ?? "")
                            {
                                case "addContact":
                                case "editContact":
                                    {
                                        XmlElement oXfmElmt = (XmlElement)adXfm.xFrmEditDirectoryContact(myWeb.moRequest["id"], myWeb.mnUserId);
                                        if (Conversions.ToBoolean(!adXfm.valid))
                                        {
                                            contentNode.AppendChild(oXfmElmt);
                                        }
                                        else
                                        {
                                            myWeb.RefreshUserXML();
                                        }

                                        break;
                                    }
                                case "delContact":
                                    {
                                        foreach (XmlElement oContactElmt in myWeb.GetUserXML((long)myWeb.mnUserId).SelectNodes("descendant-or-self::Contact"))
                                        {
                                            XmlElement oId = (XmlElement)oContactElmt.SelectSingleNode("nContactKey");
                                            if (oId != null)
                                            {
                                                if ((oId.InnerText ?? "") == (myWeb.moRequest["id"] ?? ""))
                                                {
                                                    myWeb.moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.CartContact, Conversions.ToLong(myWeb.moRequest["id"]));
                                                    myWeb.RefreshUserXML();
                                                }
                                            }
                                        }

                                        break;
                                    }
                            }
                        }
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "UserContacts", ex, "", "", gbDebug);
                        // Return Nothing
                    }

                }

                public void CompanyContact(ref Cms myWeb, ref XmlElement contentNode)
                {
                    long CompanyId = Conversions.ToLong(myWeb.moRequest["ParentDirId"]);
                    bool bUserValid = true;
                    try
                    {

                        // If myWeb.mnUserId > 0 And myWeb.moPageXml.SelectSingleNode("User/Company[@id='" & CompanyId & "']") IsNot Nothing Then

                        if (myWeb.mnUserId > 0)
                        {

                            var adXfm = myWeb.getAdminXform();
                            adXfm.open(myWeb.moPageXml);

                            if (Conversions.ToInteger("0" + myWeb.moRequest["id"]) > 0 & myWeb.moPageXml.SelectSingleNode("User/Company/Contacts/Contact/nContactKey[text()='" + myWeb.moRequest["id"] + "']") != null)
                            {
                                bUserValid = false;
                            }

                            if (bUserValid)
                            {
                                string memCmd = myWeb.moRequest["memCmd"];
                                if (string.IsNullOrEmpty(memCmd) & !string.IsNullOrEmpty(myWeb.moRequest["ewCmd"]))
                                {
                                    memCmd = myWeb.moRequest["ewCmd"];
                                }

                                switch (memCmd ?? "")
                                {
                                    case "addContact":
                                    case "editContact":
                                        {
                                            XmlElement oXfmElmt = (XmlElement)adXfm.xFrmEditDirectoryContact(myWeb.moRequest["id"], CompanyId, "/xforms/directory/CompanyContact.xml");
                                            if (Conversions.ToBoolean(!adXfm.valid))
                                            {
                                                contentNode.AppendChild(oXfmElmt);
                                            }
                                            else
                                            {
                                                myWeb.RefreshUserXML();
                                            }

                                            break;
                                        }
                                    case "delContact":
                                        {
                                            foreach (XmlElement oContactElmt in myWeb.GetUserXML((long)myWeb.mnUserId).SelectNodes("descendant-or-self::Contact"))
                                            {
                                                XmlElement oId = (XmlElement)oContactElmt.SelectSingleNode("nContactKey");
                                                if (oId != null)
                                                {
                                                    if ((oId.InnerText ?? "") == (myWeb.moRequest["id"] ?? ""))
                                                    {
                                                        myWeb.moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.CartContact, Conversions.ToLong(myWeb.moRequest["id"]));
                                                        myWeb.RefreshUserXML();
                                                    }
                                                }
                                            }

                                            break;
                                        }
                                }
                            }

                        }
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "CompanyContact", ex, "", "", gbDebug);
                        // Return Nothing
                    }

                }


                public void AddUserToGroup(ref Cms myWeb, ref XmlElement contentNode)
                {
                    try
                    {

                        if (myWeb.mnUserId > 0 & contentNode.SelectSingleNode("ancestor::Order") != null)
                        {

                            foreach (XmlElement groupNode in contentNode.SelectNodes("descendant-or-self::UserGroups/Group"))
                            {
                                string groupId = groupNode.GetAttribute("id");
                                if (Information.IsNumeric(groupId))
                                {
                                    // Dim cUserEmail As String = myWeb.moDbHelper.getNameByKey(dbHelper.objectTypes.Directory, myWeb.mnUserId)
                                    // Dim cGroupName As String = myWeb.moDbHelper.getNameByKey(dbHelper.objectTypes.Directory, CLng(groupId))

                                    myWeb.moDbHelper.maintainDirectoryRelation(Conversions.ToLong(groupId), (long)myWeb.mnUserId, false);
                                }
                            }

                        }
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "AddUserToGroup", ex, "", "", gbDebug);
                        // Return Nothing
                    }

                }

                public void JobApplication(ref Cms myWeb, ref XmlElement oContentNode)
                {

                    string cProcessInfo;
                    var moConfig = myWeb.moConfig;
                    XmlElement oElmt;
                    long JobId = Conversions.ToLong("0" + myWeb.moRequest["JobId"]);
                    long FormId = Conversions.ToLong("0" + myWeb.moRequest["FormId"]);
                    try
                    {
                        if (myWeb.mnUserId == 0)
                        {
                            Register(ref myWeb, ref oContentNode);

                            var logonContentNode = myWeb.moPageXml.CreateElement("Content");
                            oContentNode.ParentNode.AppendChild(logonContentNode);

                            this.Logon(ref myWeb, ref logonContentNode);
                        }

                        else if (string.IsNullOrEmpty(myWeb.moRequest["FormId"]))
                        {
                            // List User Applications
                            var strSql = new System.Text.StringBuilder();
                            strSql.Append("SELECT * FROM [tblEmailActivityLog] eal ");
                            strSql.Append("INNER JOIN tblActivityLog al on al.nOtherId = eal.nEmailActivityKey ");
                            strSql.Append("where al.nUserDirId = " + myWeb.mnUserId);
                            var oDsJobs = myWeb.moDbHelper.GetDataSet(strSql.ToString(), "Application", "JobApplications");
                            oContentNode.InnerXml = Strings.Replace(oDsJobs.GetXml(), "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "");
                            string sContent;
                            foreach (XmlElement oElmt2 in oContentNode.SelectNodes("descendant-or-self::cActivityXml | descendant-or-self::cActivityDetail"))
                            {
                                sContent = oElmt2.InnerText;
                                if (!string.IsNullOrEmpty(sContent))
                                {
                                    try
                                    {
                                        oElmt2.InnerXml = sContent;
                                    }
                                    catch
                                    {
                                        oElmt2.InnerXml = Tools.Text.tidyXhtmlFrag(sContent);
                                    }
                                }
                            }
                        }


                        else if (oContentNode.ParentNode.Name == "ContentDetail")
                        {
                            // remove application forms not required
                            foreach (XmlElement currentOElmt in oContentNode.SelectNodes("Content[@id!='" + myWeb.moRequest["FormId"] + "']"))
                            {
                                oElmt = currentOElmt;
                                oElmt.ParentNode.RemoveChild(oElmt);
                            }

                            foreach (XmlElement currentOElmt1 in oContentNode.SelectNodes("Content[@id='" + myWeb.moRequest["FormId"] + "']"))
                            {
                                oElmt = currentOElmt1;

                                // Form Handling
                                Protean.xForm oXform = (Protean.xForm)myWeb.getXform();

                                oXform.moPageXML = myWeb.moPageXml;
                                oXform.load(ref oElmt, true);

                                if (string.IsNullOrEmpty(myWeb.moRequest["stageButton"]) & !string.IsNullOrEmpty(myWeb.moRequest["stage"]))
                                {
                                    XmlElement passportNode2 = (XmlElement)oXform.Instance.SelectSingleNode("descendant-or-self::SkillsPassport");
                                    passportNode2.SetAttribute("stage", myWeb.moRequest["stage"]);
                                    myWeb.moSession["tempInstance"] = oXform.Instance;
                                }

                                var strSql = new System.Text.StringBuilder();
                                strSql.Append("SELECT eal.nEmailActivityKey FROM [tblEmailActivityLog] eal ");
                                strSql.Append("INNER JOIN tblActivityLog al on al.nOtherId = eal.nEmailActivityKey ");
                                strSql.Append("where al.nUserDirId = " + myWeb.mnUserId + " and al.nArtId = " + JobId);
                                long EmailActivityId = Conversions.ToLong(Operators.ConcatenateObject("0", myWeb.moDbHelper.GetDataValue(strSql.ToString())));
                                if (EmailActivityId > 0L)
                                {
                                    // load in a saved instance
                                    var strSql2 = new System.Text.StringBuilder();
                                    strSql2.Append("SELECT cActivityXml FROM [tblEmailActivityLog] eal ");
                                    strSql2.Append("INNER JOIN tblActivityLog al on al.nOtherId = eal.nEmailActivityKey ");
                                    strSql2.Append("where al.nUserDirId = " + myWeb.mnUserId + " and al.nArtId = " + JobId);

                                    string loadedInstance = Conversions.ToString(myWeb.moDbHelper.GetDataValue(strSql2.ToString()));
                                    if (!string.IsNullOrEmpty(loadedInstance) & myWeb.moSession["tempInstance"] is null)
                                    {
                                        var oLoadedInstance = myWeb.moPageXml.CreateElement("instance");
                                        oLoadedInstance.InnerXml = loadedInstance;
                                        // tidy up the stage editor

                                        oXform.Instance = (XmlElement)oLoadedInstance.FirstChild;
                                    }
                                }
                                else
                                {
                                    // set the job title
                                    XmlElement JobTitleNode = (XmlElement)oXform.Instance.SelectSingleNode("descendant-or-self::SkillsPassport/LearnerInfo/Headline/Type/Label");
                                    JobTitleNode.InnerText = myWeb.moRequest["JobApply"];
                                    XmlElement JobCodeNode = (XmlElement)oXform.Instance.SelectSingleNode("descendant-or-self::SkillsPassport/LearnerInfo/Headline/Type/Code");
                                    JobCodeNode.InnerText = myWeb.moRequest["JobId"];
                                }

                                if (oXform.isSubmitted())
                                {

                                    oXform.updateInstanceFromRequest();

                                    // tidy up the stage editor
                                    if (string.IsNullOrEmpty(myWeb.moRequest["stageButton"]))
                                    {
                                        XmlElement passportNode = (XmlElement)oXform.Instance.SelectSingleNode("descendant-or-self::SkillsPassport");
                                        passportNode.SetAttribute("stage", myWeb.moRequest["stage"]);
                                    }

                                    // save instace & progress
                                    if (EmailActivityId > 0L)
                                    {
                                        var strSql3 = new System.Text.StringBuilder();
                                        strSql3.Append(Operators.ConcatenateObject(Operators.ConcatenateObject("update tblEmailActivityLog set cActivityXml = '", stdTools.SqlFmt(oXform.Instance.OuterXml)), "'"));
                                        strSql3.Append("where nEmailActivityKey = " + EmailActivityId);
                                        myWeb.moDbHelper.ExeProcessSql(strSql3.ToString());
                                    }
                                    else
                                    {
                                        EmailActivityId = myWeb.moDbHelper.emailActivity((short)myWeb.mnUserId, cActivityXml: oXform.Instance.OuterXml);
                                        myWeb.moDbHelper.logActivity(Cms.dbHelper.ActivityType.JobApplication, (long)myWeb.mnUserId, 0L, JobId, EmailActivityId, "");
                                    }

                                    if (!string.IsNullOrEmpty(myWeb.moRequest["SaveExit"]))
                                    {

                                        myWeb.msRedirectOnEnd = myWeb.mcPagePath;

                                    }

                                    if (myWeb.moRequest["stageButton"] == "Validate")
                                    {

                                        // We validate the form

                                        XmlElement oSkillsPassport = (XmlElement)oXform.Instance.SelectSingleNode("emailer/oBodyXML/SkillsPassport");
                                        oSkillsPassport.SetAttribute("stage", "Validate");
                                        oXform.validate();

                                        if (oXform.valid)
                                        {
                                            string sMessage;

                                            var oMsg = new Protean.Messaging(ref myWeb.msException);

                                            Cms.dbHelper argodbHelper = null;
                                            sMessage = Conversions.ToString(oMsg.emailer((XmlElement)oXform.Instance.SelectSingleNode("emailer/oBodyXML"), oXform.Instance.SelectSingleNode("emailer/xsltPath").InnerText, oXform.Instance.SelectSingleNode("emailer/fromName").InnerText, oXform.Instance.SelectSingleNode("emailer/fromEmail").InnerText, oXform.Instance.SelectSingleNode("emailer/recipientEmail").InnerText, oXform.Instance.SelectSingleNode("emailer/SubjectLine").InnerText, ccRecipient: oXform.Instance.SelectSingleNode("emailer/ccRecipient").InnerText, bccRecipient: oXform.Instance.SelectSingleNode("emailer/bccRecipient").InnerText, cSeperator: "", odbHelper: ref argodbHelper));
                                            // Return sMessage
                                            XmlElement oEmailer = (XmlElement)oXform.Instance.SelectSingleNode("emailer");
                                            oEmailer.SetAttribute("SubmitMessage", sMessage);

                                            oSkillsPassport.SetAttribute("stage", "Complete");

                                        }

                                        var strSql3 = new System.Text.StringBuilder();
                                        strSql3.Append(Operators.ConcatenateObject(Operators.ConcatenateObject("update tblEmailActivityLog set cActivityXml = '", stdTools.SqlFmt(oXform.Instance.OuterXml)), "'"));
                                        strSql3.Append("where nEmailActivityKey = " + EmailActivityId);
                                        myWeb.moDbHelper.ExeProcessSql(strSql3.ToString());

                                    }
                                }

                                oXform.addValues();

                                oElmt.InnerXml = oXform.moXformElmt.InnerXml;
                                oXform = (Protean.xForm)null;

                            }
                        }
                        // Load Application Form
                        else
                        {
                            cProcessInfo = "only run on content detail";
                            // do nothing
                        }
                    }


                    // Dim adXfm As Object = myWeb.getAdminXform()
                    // Dim oXfmElmt As XmlElement

                    // oContentNode.InnerXml = oXfmElmt.InnerXml

                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "Logon", ex, ""));
                    }
                }

            }

            #endregion

            ~Membership()
            {
            }
        }
    }
}