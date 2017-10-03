Imports System.Security.Principal

Namespace Security
    Public Class Impersonate
        Public Event OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)

        Private Const LOGON32_PROVIDER_DEFAULT As Integer = 0
        Private Const LOGON32_LOGON_INTERACTIVE As Integer = 2
        Private Const LOGON32_LOGON_NETWORK As Integer = 3
        Private Const LOGON32_LOGON_BATCH As Integer = 4
        Private Const LOGON32_LOGON_SERVICE As Integer = 5
        Private Const LOGON32_LOGON_UNLOCK As Integer = 7
        Private Const LOGON32_LOGON_NETWORK_CLEARTEXT As Integer = 8
        Private Const LOGON32_LOGON_NEW_CREDENTIALS As Integer = 9

        Private Shared ImpersonationContext As WindowsImpersonationContext

        Declare Function LogonUserA Lib "advapi32.dll" ( _
                                ByVal lpszUsername As String, _
                                ByVal lpszDomain As String, _
                                ByVal lpszPassword As String, _
                                ByVal dwLogonType As Integer, _
                                ByVal dwLogonProvider As Integer, _
                                ByRef phToken As IntPtr) As Integer

        Declare Auto Function DuplicateToken Lib "advapi32.dll" ( _
                                ByVal ExistingTokenHandle As IntPtr, _
                                ByVal ImpersonationLevel As Integer, _
                                ByRef DuplicateTokenHandle As IntPtr) As Integer
        Declare Auto Function RevertToSelf Lib "advapi32.dll" () As Long
        Declare Auto Function CloseHandle Lib "kernel32.dll" (ByVal handle As IntPtr) As Long


        ' NOTE:
        ' The identity of the process that impersonates a specific user on a thread must have 
        ' "Act as part of the operating system" privilege. If the the Aspnet_wp.exe process runs
        ' under a the ASPNET account, this account does not have the required privileges to 
        ' impersonate a specific user. This information applies only to the .NET Framework 1.0. 
        ' This privilege is not required for the .NET Framework 1.1.
        '
        ' Sample call:
        '
        '    If impersonateValidUser("username", "domain", "password") Then
        '        'Insert your code here.
        '
        '        undoImpersonation()
        '    Else
        '        'Impersonation failed. Include a fail-safe mechanism here.
        '    End If
        '
        <System.Security.SecuritySafeCritical()>
        Public Function ImpersonateValidUser(ByVal strUserName As String, _
                    ByVal strDomain As String, _
                    ByVal strPassword As String, _
                    Optional ByVal bCheckAdmin As Boolean = False, _
                    Optional ByVal cInGroup As String = "") As Boolean

            'PerfMon.Log("Impersonate", "ImpersonateValidUser")
            Dim token As IntPtr = IntPtr.Zero
            Dim tokenDuplicate As IntPtr = IntPtr.Zero
            Dim tempWindowsIdentity As WindowsIdentity
            Dim isDomAdmin As Boolean = False
            If cInGroup = "" Then cInGroup = "Domain Admins"
            ImpersonateValidUser = False

            ' bCheckAdmin seems to be a bit of a hangover - it's a white elephant, so for now, I'll ignore it
            Try

                If cInGroup = "AzureWebApp" Then
                    Return True
                Else
                    If RevertToSelf() <> 0 Then
                        If LogonUserA(strUserName, strDomain, strPassword,
                           LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, token) <> 0 Then
                            If DuplicateToken(token, 2, tokenDuplicate) <> 0 Then
                                tempWindowsIdentity = New WindowsIdentity(tokenDuplicate)

                                If strDomain <> "" Then
                                    isDomAdmin = New WindowsPrincipal(tempWindowsIdentity).IsInRole(strDomain & "\" & cInGroup)
                                Else
                                    isDomAdmin = New WindowsPrincipal(tempWindowsIdentity).IsInRole(cInGroup)
                                End If

                                ImpersonationContext = tempWindowsIdentity.Impersonate()
                                If Not (ImpersonationContext Is Nothing) Then
                                    ''if we are checking for admin then return that value
                                    'If bCheckAdmin Then
                                    '    ImpersonateValidUser = True
                                    'Else
                                    '    Return isDomAdmin
                                    'End If

                                    Return isDomAdmin
                                End If
                            End If
                        Else
                            Return False
                        End If
                    End If

                    If Not tokenDuplicate.Equals(IntPtr.Zero) Then
                        CloseHandle(tokenDuplicate)
                    End If

                    If Not token.Equals(IntPtr.Zero) Then
                        CloseHandle(token)
                    End If
                End If

            Catch ex As Exception

                RaiseEvent OnError(Nothing, New Eonic.Tools.Errors.ErrorEventArgs("Impersonate", "ImpersonateValidUser", ex, ""))
                Return False
            End Try
        End Function



        Public Sub UndoImpersonation()
            If Not IsNothing(ImpersonationContext) Then
                ImpersonationContext.Undo()
            End If
        End Sub
    End Class
End Namespace


