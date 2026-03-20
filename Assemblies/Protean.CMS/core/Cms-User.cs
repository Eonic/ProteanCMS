using System;
using System.Text.RegularExpressions;
using System.Xml;
using static Protean.stdTools;
using static Protean.Tools.Xml;
using static System.Web.HttpUtility;

namespace Protean
{

    public partial class Cms : Protean.Base, IDisposable
    {
        public virtual string MembershipProcess()
        {
            PerfMon.Log("Web", "MembershipProcess");
            string sProcessInfo = "";
            string sReturnValue = string.Empty;
            string cLogonCmd = string.Empty;
            try
            {

                Cms argmyWeb = this;
                return Convert.ToString(moMemProv.Activities.MembershipProcess(ref argmyWeb));

            }

            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "MembershipLogon", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "MembershipProcess", ex, sProcessInfo));
                return null;
            }
        }

        public virtual bool AlternativeAuthentication()
        {
            PerfMon.Log("Web", "AlternativeAuthentication");

            string cProcessInfo = "";
            // Dim bCheck As Boolean = False
            // Dim cToken As String = ""
            // Dim cKey As String = ""
            // Dim cDecrypted As String = ""
            // Dim nReturnId As Integer

            try
            {
                Cms argmyWeb = this;
                return Convert.ToBoolean(moMemProv.Activities.AlternativeAuthentication(ref argmyWeb));
            }

            // ' Look for the RC4 token
            // If moRequest("token") <> "" And moConfig("AlternativeAuthenticationKey") <> "" Then

            // cProcessInfo = "IP Address Checking"

            // Dim cIPList As String = CStr(moConfig("AlternativeAuthenticationIPList"))

            // If cIPList = "" OrElse Tools.Text.IsIPAddressInList(moRequest.UserHostAddress, cIPList) Then

            // cProcessInfo = "Decrypting token"
            // Dim oEnc As New Protean.Tools.Encryption.RC4()

            // cToken = moRequest("token")
            // cKey = moConfig("AlternativeAuthenticationKey")

            // ' There are two accepted formats to receive:
            // '  1. Email address
            // '  2. User ID

            // cDecrypted = Trim(Tools.Encryption.RC4.Decrypt(cToken, cKey))

            // If Tools.Text.IsEmail(cDecrypted) Then

            // ' Authentication is by way of email address
            // cProcessInfo = "Email authenctication: Retrieving user for email: " & cDecrypted
            // ' Get the user id based on the email address
            // nReturnId = moDbHelper.GetUserIDFromEmail(cDecrypted)

            // If nReturnId > 0 Then
            // bCheck = True
            // Me.mnUserId = nReturnId
            // End If

            // ElseIf IsNumeric(cDecrypted) AndAlso CInt(cDecrypted) > 0 Then

            // ' Authentication is by way of user ID
            // cProcessInfo = "User ID Authentication: " & cDecrypted
            // ' Get the user id based on the email address
            // bCheck = moDbHelper.IsValidUser(CInt(cDecrypted))
            // If bCheck Then Me.mnUserId = CInt(cDecrypted)

            // End If

            // End If

            // End If

            // Return bCheck

            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "AlternativeAuthentication", ex, cProcessInfo));
                return false;
            }

        }

        public XmlElement GetUserXML(long nUserId = 0L)
        {
            PerfMon.Log("Web", "GetUserXML");
            string sProcessInfo = "";
            try
            {
                Cms argmyWeb = this;
                return moMemProv.Activities.GetUserXML(ref argmyWeb, nUserId);
            }

            catch (Exception ex)
            {

                // returnException(msException, mcModuleName, "getUserXml", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetUserXml", ex, sProcessInfo));
                return null;
            }

        }

        public void RefreshUserXML()
        {
            PerfMon.Log("Web", "GetUserXML");
            string sProcessInfo = "";
            XmlElement oUserXml;
            try
            {
                if (mnUserId != 0)
                {
                    oUserXml = (XmlElement)moPageXml.SelectSingleNode("/Page/User");
                    if (oUserXml is null)
                    {

                        moPageXml.DocumentElement.AppendChild(moPageXml.ImportNode(GetUserXML((long)mnUserId), true));
                    }
                    // moPageXml.DocumentElement.AppendChild(GetUserXML(mnUserId))
                    else
                    {
                        moPageXml.DocumentElement.ReplaceChild(GetUserXML((long)mnUserId), oUserXml);
                    }
                }
            }
            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "RefreshUserXML", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "RefreshUserXML", ex, sProcessInfo));
            }

        }


        public virtual void LogOffProcess()
        {
            PerfMon.Log("Web", "LogOffProcess");
            string cProcessInfo = "";

            try
            {

                cProcessInfo = "Commit to Log";
                if (gbSingleLoginSessionPerUser)
                {
                    moDbHelper.logActivity(Cms.dbHelper.ActivityType.Logoff, (long)mnUserId, 0L);
                    if (moRequest.Cookies["ewslock"] != null)
                    {
                        moResponse.Cookies["ewslock"].Expires = DateTime.Now.AddDays(-1);
                    }
                }
                else
                {
                    moDbHelper.CommitLogToDB(Cms.dbHelper.ActivityType.Logoff, mnUserId, moSession.SessionID, DateTime.Now, 0, 0, "");
                }


                // Call this BEFORE clearing the user ID.
                cProcessInfo = "Clear Site Stucture";
                moDbHelper.clearStructureCacheUser();

                // Clear the user ID.
                mnUserId = 0;

                // Clear the cart
                if (moSession != null && gbCart)
                {
                    string cSql = "update tblCartOrder set cCartSessionId = 'OLD_' + cCartSessionId where (cCartSessionId = '" + moSession.SessionID + "' and cCartSessionId <> '')";
                    moDbHelper.ExeProcessSql(cSql);
                }

                if (moSession != null)
                {
                    cProcessInfo = "Abandon Session";
                    // AJG : Question - why does this not clear the Session ID?
                    moSession.Abandon();
                }

                if (moConfig["RememberMeMode"] != "KeepCookieAfterLogoff")
                {
                    cProcessInfo = "Clear Cookies";
                    moResponse.Cookies["RememberMeUserName"].Expires = DateTime.Now.AddDays(-1);
                    moResponse.Cookies["RememberMeUserId"].Expires = DateTime.Now.AddDays(-1);
                }
            }

            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "LogOffProcess", ex, cProcessInfo));
            }

        }

        public virtual string UserEditProcess()
        {
            PerfMon.Log("Web", "UserEditProcess");
            string sProcessInfo = "";
            string sReturnValue = string.Empty;
            try
            {


                return null;
            }

            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "UserEditProcess", ex, sProcessInfo));
                return null;
            }
        }

        public virtual void logonRedirect(string cLogonCmd)
        {
            string sProcessInfo = "";

            try
            {

                string sRedirectPath;

                // don't redirect if in cart process
                if (moRequest.QueryString["cartCmd"] != "Logon")
                {

                    if (moSession["LogonRedirect"] != null && !string.IsNullOrEmpty(moSession["LogonRedirect"].ToString()))
                    {
                        sRedirectPath = moSession["LogonRedirect"].ToString();
                        moSession["LogonRedirect"] = "";
                    }
                    else
                    {

                        if (!string.IsNullOrEmpty(moConfig["LogonRedirectPath"]))
                        {
                            // sRedirectPath = mcOriginalURL & moConfig("LogonRedirectPath") & cLogonCmd
                            // TS changed this remvoing mcOriginalURL because it breaks logon redirect "/" to go to root/
                            sRedirectPath = moConfig["LogonRedirectPath"] + cLogonCmd;
                        }
                        else
                        {
                            sRedirectPath = mcOriginalURL + cLogonCmd;
                        }

                        if (goLangConfig != null)
                        {
                            sRedirectPath = mcPageLanguageUrlPrefix + sRedirectPath;
                        }

                    }
                    if (sRedirectPath.Contains("token="))
                    {

                        sRedirectPath = Regex.Replace(sRedirectPath, "(token=.*?)&", "");
                    }


                    msRedirectOnEnd = sRedirectPath;
                }
            }

            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "logonRedirect", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "logonRedirect", ex, sProcessInfo));
            }
        }


    }
}