using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using System.Web.Configuration;

using System.Xml;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using static Protean.stdTools;
using Protean.Providers.Membership;
using System.Runtime.InteropServices.ComTypes;
using static Protean.Cms.Admin;
using System.Web.UI.WebControls;
using System.Web;
using System.Linq.Expressions;

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
                catch (Exception)
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
                    string SecureMembershipAddress = (myWeb.moConfig["SecureMembershipAddress"]?? "").ToString();
                    string SecureMembershipDomain = (myWeb.moConfig["SecureMembershipDomain"] ?? "").ToString();



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
                        if (!string.IsNullOrEmpty(SecureMembershipDomain))
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
                string sProcessInfo = string.Empty;
                try
                {

                    var castObject = WebConfigurationManager.GetWebApplicationSection("protean/membershipProviders");
                    Protean.ProviderSectionHandler moPrvConfig = (Protean.ProviderSectionHandler)castObject;
                    foreach (ProviderSettingsCollection ourProvider in moPrvConfig.Providers)
                    {
                        if (ourProvider[actionName] != null)
                        {
                            Type calledType;
                            Assembly assemblyInstance;

                            if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(ourProvider["path"], "", false)))
                            {
                                assemblyInstance = Assembly.LoadFrom(myWeb.goServer.MapPath(Conversions.ToString(ourProvider["path"])));
                            }
                            else
                            {
                                assemblyInstance = Assembly.Load(ourProvider.GetType().ToString());
                            }
                            if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(ourProvider["rootClass"], "", false)))
                            {
                                calledType = assemblyInstance.GetType(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("Protean.Providers.Membership.", ourProvider["Name"]), "Tools.Actions")), true);
                            }
                            else
                            {
                                calledType = assemblyInstance.GetType(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(ourProvider["rootClass"], ".Providers.Membership."), ourProvider["Name"]), "Tools.Actions")), true);
                            }

                            var o = Activator.CreateInstance(calledType);

                            var args = new object[1];
                            args[0] = myWeb;

                            calledType.InvokeMember(Conversions.ToString(ourProvider[actionName]), BindingFlags.InvokeMethod, null, o, args);

                        }
                    }
                }


                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ProviderActions", ex, ""));
                }
            }


            public void RegistrationActions(string cmdPrefix = "") {
                string cProcessInfo = "";
                ReturnProvider RetProv;
                IMembershipProvider moMemProv;
                try
                {
                    RetProv = new Protean.Providers.Membership.ReturnProvider();
                    moMemProv = RetProv.Get(ref myWeb, myWeb.moConfig["MembershipProvider"]);
                    switch (myWeb.moConfig["RegisterBehaviour"] ?? "")
                    {
                        case "validateByEmail":
                            {
                              
                                // first wmyWebe set the user account to be pending
                                myWeb.moDbHelper.setObjectStatus(Cms.dbHelper.objectTypes.Directory, Cms.dbHelper.Status.Pending, myWeb.mnUserId);
                                var oMembership = new Membership(ref myWeb);
                                oMembership.OnError += myWeb.OnComponentError;
                                oMembership.AccountActivateLink((int)myWeb.mnUserId);
                                myWeb.moDbHelper.CommitLogToDB(Cms.dbHelper.ActivityType.Register, (int)myWeb.mnUserId, myWeb.moSession.SessionID, DateTime.Now, 0, 0, "Send Activation"); // Auto logon
                                break;
                            }

                        default:
                            {
                              if (myWeb.moSession != null)
                                    myWeb.moSession["nUserId"] = (object)myWeb.mnUserId;

                                myWeb.moDbHelper.CommitLogToDB(Cms.dbHelper.ActivityType.Register, (int)myWeb.mnUserId, myWeb.moSession.SessionID, DateTime.Now, 0, 0, "First Logon");
                                break;
                            }


                    }
                    moMemProv.Activities.sendRegistrationAlert(ref myWeb, myWeb.mnUserId, false, cmdPrefix);

                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "Logon", ex, ""));
                }
                finally
                {

                }
            }

            #endregion



           

            ~Membership()
            {
            }
        }
    }
}