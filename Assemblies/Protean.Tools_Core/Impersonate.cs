using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.Web.Configuration;

namespace Protean.Tools.Security
{
    public class Impersonate
    {
        public event OnErrorEventHandler OnError;

        public delegate void OnErrorEventHandler(object sender, Protean.Tools.Errors.ErrorEventArgs e);

        private const int LOGON32_PROVIDER_DEFAULT = 0;
        private const int LOGON32_LOGON_INTERACTIVE = 2;
        private const int LOGON32_LOGON_NETWORK = 3;
        private const int LOGON32_LOGON_BATCH = 4;
        private const int LOGON32_LOGON_SERVICE = 5;
        private const int LOGON32_LOGON_UNLOCK = 7;
        private const int LOGON32_LOGON_NETWORK_CLEARTEXT = 8;
        private const int LOGON32_LOGON_NEW_CREDENTIALS = 9;

        private static WindowsImpersonationContext ImpersonationContext;

        [DllImport("advapi32.dll")]
        static extern int LogonUserA(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);






        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        static extern int DuplicateToken(IntPtr ExistingTokenHandle, int ImpersonationLevel, ref IntPtr DuplicateTokenHandle);


        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        static extern long RevertToSelf();
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern long CloseHandle(IntPtr handle);


        // NOTE:
        // The identity of the process that impersonates a specific user on a thread must have 
        // "Act as part of the operating system" privilege. If the the Aspnet_wp.exe process runs
        // under a the ASPNET account, this account does not have the required privileges to 
        // impersonate a specific user. This information applies only to the .NET Framework 1.0. 
        // This privilege is not required for the .NET Framework 1.1.
        // 
        // Sample call:
        // 
        // If impersonateValidUser("username", "domain", "password") Then
        // 'Insert your code here.
        // 
        // undoImpersonation()
        // Else
        // 'Impersonation failed. Include a fail-safe mechanism here.
        // End If
        // 
        [SecuritySafeCritical()]
        public bool ImpersonateValidUser(string strUserName, string strDomain, string strPassword, bool bCheckAdmin = false, string cInGroup = "")



        {
            bool ImpersonateValidUserRet = default;

            // PerfMon.Log("Impersonate", "ImpersonateValidUser")
            var token = IntPtr.Zero;
            var tokenDuplicate = IntPtr.Zero;
            WindowsIdentity tempWindowsIdentity;
            bool isDomAdmin = false;
            if (string.IsNullOrEmpty(cInGroup))
                cInGroup = "Domain Admins";
            ImpersonateValidUserRet = false;

            // bCheckAdmin seems to be a bit of a hangover - it's a white elephant, so for now, I'll ignore it
            try
            {

                if (cInGroup == "AzureWebApp")
                {
                    System.Collections.Specialized.NameValueCollection goConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");
                    if ((strUserName ?? "") == (goConfig["AdminAcct"] ?? "") & (strPassword ?? "") == (goConfig["AdminPassword"] ?? ""))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (RevertToSelf() != 0L)
                    {
                        if (Impersonate.LogonUserA(strUserName, strDomain, strPassword, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, ref token) != 0)
                        {
                            if (DuplicateToken(token, 2, ref tokenDuplicate) != 0)
                            {
                                tempWindowsIdentity = new WindowsIdentity(tokenDuplicate);

                                if (!string.IsNullOrEmpty(strDomain))
                                {
                                    isDomAdmin = new WindowsPrincipal(tempWindowsIdentity).IsInRole(strDomain + @"\" + cInGroup);
                                }
                                else
                                {
                                    isDomAdmin = new WindowsPrincipal(tempWindowsIdentity).IsInRole(cInGroup);
                                }

                                ImpersonationContext = tempWindowsIdentity.Impersonate();
                                if (ImpersonationContext != null)
                                {
                                    // 'if we are checking for admin then return that value
                                    // If bCheckAdmin Then
                                    // ImpersonateValidUser = True
                                    // Else
                                    // Return isDomAdmin
                                    // End If

                                    return isDomAdmin;
                                }
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }

                    if (!tokenDuplicate.Equals(IntPtr.Zero))
                    {
                        CloseHandle(tokenDuplicate);
                    }

                    if (!token.Equals(IntPtr.Zero))
                    {
                        CloseHandle(token);
                    }
                }
            }

            catch (Exception ex)
            {

                OnError?.Invoke(default, new Protean.Tools.Errors.ErrorEventArgs("Impersonate", "ImpersonateValidUser", ex, ""));
                return false;
            }

            return ImpersonateValidUserRet;
        }



        public void UndoImpersonation()
        {
            if (!(ImpersonationContext == null))
            {
                ImpersonationContext.Undo();
            }
        }
    }
}
