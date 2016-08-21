'**************************************************************
' Copyright (c) Emmanuel KARTMANN 2002 (emmanuel@kartmann.org)
' $Revision: 15 $ $Date: 10/30/02 10:45a $
'**************************************************************

Imports System
Imports System.Text
' for StringBuilder class
Imports System.Diagnostics
' for Process class
Imports System.IO
' for StreamWriter/StreamReader classes
Imports System.Threading
' for Thread class
Namespace GnuPG

    ''' <summary>
    ''' Specific exception thrown whenever a PGP error occurs.
    ''' 
    ''' <p/>This class is a simple derivation from the Exception class.
    ''' </summary>
    Public Class GnuPGException
        Inherits Exception
        ''' <summary>
        ''' Constructor
        ''' </summary>
        ''' <param name="message">error message associated with exception</param>
        Public Sub New(message As String)
            MyBase.New(message)
        End Sub
    End Class

    ''' <summary>
    ''' List (enum) of available commands (sign, encrypt, sign and encrypt, etc...)
    ''' </summary>
    Public Enum Commands
        ''' <summary>
        ''' Make a signature
        ''' </summary>
        Sign
        ''' <summary>
        ''' Encrypt  data
        ''' </summary>
        Encrypt
        ''' <summary>
        ''' Sign and encrypt data
        ''' </summary>
        SignAndEncrypt
        ''' <summary>
        ''' Decrypt data
        ''' </summary>
        Decrypt
        ''' <summary>
        ''' Assume that input is a signature and verify it without generating any output
        ''' </summary>
        Verify
    End Enum
    ' TODO implement other GPG commands (--clearsign, --detach-sign, --symmetric, --store, --verify-files, --encrypt-files, --decrypt-files, --list-keys, --list-public-keys, --list-secret-keys, --list-sigs, --check-sigs, --fingerprint, --check-sigs, --list-packets, --gen-key, --edit-key, --sign-key, --lsign-key, --nrsign-key, --delete-key, --delete-secret-key, --delete-secret-and-public-key, --gen-revoke, --desig-revoke, --export, --send-keys, --export-all, --export-secret-keys, --export-secret-subkeys, --import, --fast-import, --recv-keys, --search-keys, --update-trustdb, --check-trustdb, --export-ownertrust, --import-ownertrust, --rebuild-keydb-caches, --print-md, --print-mds, --gen-random, --gen-prime mode, --version, --warranty, --help)

    ''' <summary>
    ''' List (enum) of available verbose levels (NoVerbose, Verbose, VeryVerbose)
    ''' </summary>
    Public Enum VerboseLevel
        ''' <summary>
        ''' Reset verbose level to 0 (no information shown during processing)
        ''' </summary>
        NoVerbose
        ''' <summary>
        ''' Give more information during processing.
        ''' </summary>
        Verbose
        ''' <summary>
        ''' Give full information during processing (the input data is listed in detail).
        ''' </summary>
        VeryVerbose
    End Enum

    ''' <summary>
    ''' This class is a wrapper class for GNU Privacy Guard (GnuPG). It execute the command 
    ''' line program (gpg.exe) in an different process, redirects standard input (stdin),
    ''' standard output (stdout) and standard error (stderr) streams, and monitors the 
    ''' streams to fetch the results of the encryption/signing operation.<p/>
    ''' 
    ''' Please note that you must have INSTALLED GnuPG AND generated/imported the 
    ''' appropriate keys before using this class.<p/>
    ''' 
    ''' GnuPG stands for GNU Privacy Guard and is GNU's tool for secure communication and 
    ''' data storage. It can be used to encrypt data and to create digital signatures. It 
    ''' includes an advanced key management facility and is compliant with the proposed 
    ''' OpenPGP Internet standard as described in RFC 2440. As such, GnuPG is a complete 
    ''' and free replacement for PGP.<p/>
    ''' 
    ''' This class has been developed and tested with GnuPG v1.2.0 (MingW32)<p/>
    ''' 
    ''' For more about GNU, please refer to http://www.gnu.org <br/>
    ''' For more about GnuPG, please refer to http://www.gnupg.org <br/>
    ''' For more about OpenPGP (RFC 2440), please refer to http://www.gnupg.org/rfc2440.html <br/>
    ''' </summary>
    Public Class GnuPGWrapper

        ''' <summary>
        ''' Default constructor
        ''' </summary>
        Public Sub New()
        End Sub

        ''' <summary>
        ''' Constructor
        ''' </summary>
        ''' <param name="homeDirectory">home directory for GnuPG (where keyrings AND gpg.exe are located)</param>
        Public Sub New(homeDirectory__1 As String)
            homedirectory = homeDirectory__1
        End Sub

        ''' <summary>
        ''' Command property: set the type of command to execute (sign, encrypt...)
        ''' 
        ''' <p/>Defaults to SignAndEncrypt.
        ''' </summary>
        Public WriteOnly Property command() As Commands
            Set(value As Commands)
                _command = value
            End Set
        End Property

        ''' <summary>
        ''' Boolean flag: if true, GnuPG creates ASCII armored output (text output). 
        ''' 
        ''' <p/>Defaults to true (ASCII ouput).
        ''' </summary>
        Public WriteOnly Property armor() As Boolean
            Set(value As Boolean)
                _armor = value
            End Set
        End Property

        ''' <summary>
        ''' Recipient email address - mandatory when <see cref="command">command</see> is Encrypt or SignAndEncrypt
        ''' 
        ''' <p/>GnuPG uses this parameter to find the associated public key. You must have imported 
        ''' this public key in your keyring before.
        ''' </summary>
        Public WriteOnly Property recipient() As String
            Set(value As String)
                _recipient = value
            End Set
        End Property

        ''' <summary>
        ''' Originator email address - recommended when <see cref="command">command</see> is Sign or SignAndEncrypt
        ''' 
        ''' <p/>GnuPG uses this parameter to find the associated secret key. You must have imported 
        ''' this secret key in your keyring before. Otherwise, GnuPG uses the first secret key 
        ''' in your keyring to sign messages. This property is mapped to the "--default-key" option.
        ''' </summary>
        Public WriteOnly Property originator() As String
            Set(value As String)
                _originator = value
            End Set
        End Property

        ''' <summary>
        ''' Boolean flag; if true, GnuPG assumes "yes" on most questions.
        ''' 
        ''' <p/>Defaults to true.
        ''' </summary>
        Public WriteOnly Property yes() As Boolean
            Set(value As Boolean)
                _yes = value
            End Set
        End Property

        ''' <summary>
        ''' Boolean flag; if true, GnuPG uses batch mode (Never ask, do not allow 
        ''' interactive commands).
        ''' 
        ''' <p/>Defaults to true.
        ''' </summary>
        Public WriteOnly Property batch() As Boolean
            Set(value As Boolean)
                _batch = value
            End Set
        End Property


        ''' <summary>
        ''' Passphrase for using your private key - mandatory when 
        ''' <see cref="command">command</see> is Sign or SignAndEncrypt.
        ''' </summary>
        Public WriteOnly Property passphrase() As String
            Set(value As String)
                _passphrase = value
                If _passphrase <> "" Then
                    ' stdin
                    _passphrasefd = "0"
                Else
                    _passphrasefd = ""
                End If
            End Set
        End Property

        ''' <summary>
        ''' name of the home directory (where keyrings AND gpg.exe are located)
        ''' </summary>
        Public WriteOnly Property homedirectory() As String
            Set(value As String)
                _homedirectory = value
                ' For now, let's assume the gpg.exe program is installed in the homedirectory too
                _bindirectory = value
            End Set
        End Property

        ''' <summary>
        ''' File descriptor for entering passphrase - defaults to 0 (standard input).
        ''' </summary>
        Public WriteOnly Property passphrasefd() As String
            Set(value As String)
                _passphrasefd = value
            End Set
        End Property

        ''' <summary>
        ''' Exit code from GnuPG process (0 = success; otherwise an error occured)
        ''' </summary>
        Public ReadOnly Property exitcode() As Integer
            Get
                Return (_exitcode)
            End Get
        End Property

        ''' <summary>
        ''' Verbose level (NoVerbose, Verbose, VeryVerbose). 
        ''' 
        ''' <p/>Defaults to NoVerbose.
        ''' </summary>
        Public Property verbose() As VerboseLevel
            Get
                Return (_verbose)
            End Get
            Set(value As VerboseLevel)
                _verbose = value
            End Set
        End Property

        ''' <summary>
        ''' Timeout for GnuPG process, in milliseconds.
        ''' 
        ''' <p/>If the process doesn't exit before the end of the timeout period, the process is terminated (killed).
        ''' 
        ''' <p/>Defaults to 10000 (10 seconds).
        ''' </summary>
        Public Property ProcessTimeOutMilliseconds() As Integer
            Get
                Return (_ProcessTimeOutMilliseconds)
            End Get
            Set(value As Integer)
                _ProcessTimeOutMilliseconds = value
            End Set
        End Property

        ''' <summary>
        ''' Generate a string of GnuPG command line arguments, based on the properties
        ''' set in this object (e.g. if the <see cref="armor">armor</see> property is true, 
        ''' this method generates the "--armor" argument).
        ''' </summary>
        ''' <returns>GnuPG command line arguments</returns>
        Protected Function BuildOptions() As String
            Dim optionsBuilder As New StringBuilder("", 255)
            Dim recipientNeeded As Boolean = False
            Dim passphraseNeeded As Boolean = False

            ' Home Directory?
            If _homedirectory IsNot Nothing AndAlso _homedirectory <> "" Then
                ' WARNING: directory path is between quotes
                ' TODO replace directory path with quotes by short path (with "~" for long names) - call GetShortPathName?
                optionsBuilder.Append("--homedir """)
                optionsBuilder.Append(_homedirectory)
                optionsBuilder.Append(""" ")
            End If

            ' Answer yes to all questions?
            If _yes Then
                optionsBuilder.Append("--yes ")
            End If

            ' batch mode?
            If _batch Then
                optionsBuilder.Append("--batch ")
            End If

            ' Command
            Select Case _command
                Case Commands.Sign
                    optionsBuilder.Append("--sign ")
                    passphraseNeeded = True
                    Exit Select
                Case Commands.Encrypt
                    optionsBuilder.Append("--encrypt ")
                    recipientNeeded = True
                    Exit Select
                Case Commands.SignAndEncrypt
                    optionsBuilder.Append("--sign ")
                    optionsBuilder.Append("--encrypt ")
                    recipientNeeded = True
                    passphraseNeeded = True
                    Exit Select
                Case Commands.Decrypt
                    optionsBuilder.Append("--decrypt ")
                    Exit Select
                Case Commands.Verify
                    optionsBuilder.Append("--verify ")
                    Exit Select
            End Select

            ' ASCII output?
            If _armor Then
                optionsBuilder.Append("--armor ")
            End If

            ' Recipient?
            If _recipient IsNot Nothing AndAlso _recipient <> "" Then
                optionsBuilder.Append("--recipient ")
                optionsBuilder.Append(_recipient)
                optionsBuilder.Append(" ")
            Else
                ' If you encrypt, you NEED a recipient!
                If recipientNeeded Then
                    Throw New GnuPGException("GPGNET: Missing 'recipient' parameter: cannot encrypt without a recipient")
                End If
            End If

            ' Originator?
            If _originator IsNot Nothing AndAlso _originator <> "" Then
                optionsBuilder.Append("--default-key ")
                optionsBuilder.Append(_originator)
                optionsBuilder.Append(" ")
            End If

            ' Passphrase?
            If _passphrase Is Nothing OrElse _passphrase = "" Then
                If passphraseNeeded Then
                    Throw New GnuPGException("GPGNET: Missing 'passphrase' parameter: cannot sign without a passphrase")
                End If
            End If

            ' Passphrase file descriptor?
            If _passphrasefd IsNot Nothing AndAlso _passphrasefd <> "" Then
                optionsBuilder.Append("--passphrase-fd ")
                optionsBuilder.Append(_passphrasefd)
                optionsBuilder.Append(" ")
            Else
                If passphraseNeeded AndAlso (_passphrase Is Nothing OrElse _passphrase = "") Then
                    Throw New GnuPGException("GPGNET: Missing 'passphrase' parameter: cannot sign without a passphrase")
                End If
            End If

            ' Command
            Select Case verbose
                Case VerboseLevel.NoVerbose
                    optionsBuilder.Append("--no-verbose ")
                    Exit Select
                Case VerboseLevel.Verbose
                    optionsBuilder.Append("--verbose ")
                    Exit Select
                Case VerboseLevel.VeryVerbose
                    optionsBuilder.Append("--verbose --verbose ")
                    Exit Select
            End Select

            Return (optionsBuilder.ToString())
        End Function

        ''' <summary>
        ''' Execute the GnuPG command defined by all parameters/options/properties.
        ''' 
        ''' <p/>Raise a GnuPGException whenever an error occurs.
        ''' </summary>
        ''' <param name="inputText"></param>
        ''' <param name="outputText"></param>
        Public Sub ExecuteCommand(inputText As String, outputText As String)
            outputText = ""

            Dim gpgOptions As String = BuildOptions()
            Dim gpgExecutable As String = _bindirectory + "\gpg.exe"

            ' TODO check existence of _bindirectory and gpgExecutable

            ' Create startinfo object
            Dim pInfo As New ProcessStartInfo(gpgExecutable, gpgOptions)
            pInfo.WorkingDirectory = _bindirectory
            pInfo.CreateNoWindow = True
            pInfo.UseShellExecute = False
            ' Redirect everything: 
            ' stdin to send the passphrase, stdout to get encrypted message, stderr in case of errors...
            pInfo.RedirectStandardInput = True
            pInfo.RedirectStandardOutput = True
            pInfo.RedirectStandardError = True
            _processObject = Process.Start(pInfo)

            ' Send pass phrase, if any
            If _passphrase IsNot Nothing AndAlso _passphrase <> "" Then
                _processObject.StandardInput.WriteLine(_passphrase)
                _processObject.StandardInput.Flush()
            End If

            ' Send input text
            _processObject.StandardInput.Write(inputText)
            _processObject.StandardInput.Flush()
            _processObject.StandardInput.Close()

            _outputString = ""
            _errorString = ""

            ' Create two threads to read both output/error streams without creating a deadlock
            Dim outputEntry As New ThreadStart(AddressOf StandardOutputReader)
            Dim outputThread As New Thread(outputEntry)
            outputThread.Start()
            Dim errorEntry As New ThreadStart(AddressOf StandardErrorReader)
            Dim errorThread As New Thread(errorEntry)
            errorThread.Start()

            If _processObject.WaitForExit(ProcessTimeOutMilliseconds) Then
                ' Process exited before timeout...
                ' Wait for the threads to complete reading output/error (but use a timeout!)
                If Not outputThread.Join(ProcessTimeOutMilliseconds / 2) Then
                    outputThread.Abort()
                End If
                If Not errorThread.Join(ProcessTimeOutMilliseconds / 2) Then
                    errorThread.Abort()
                End If
            Else
                ' Process timeout: PGP hung somewhere... kill it (as well as the threads!)
                _outputString = ""
                _errorString = "Timed out after " + ProcessTimeOutMilliseconds.ToString() + " milliseconds"
                _processObject.Kill()
                If outputThread.IsAlive Then
                    outputThread.Abort()
                End If
                If errorThread.IsAlive Then
                    errorThread.Abort()
                End If
            End If

            ' Check results and prepare output
            _exitcode = _processObject.ExitCode
            If _exitcode = 0 Then
                outputText = _outputString
            Else
                If _errorString = "" Then
                    _errorString = "GPGNET: [" + _processObject.ExitCode.ToString() + "]: Unknown error"
                End If
                Throw New GnuPGException(_errorString)
            End If
        End Sub

        ''' <summary>
        ''' Reader thread for standard output
        ''' 
        ''' <p/>Updates the private variable _outputString (locks it first)
        ''' </summary>
        Public Sub StandardOutputReader()
            Dim output As String = _processObject.StandardOutput.ReadToEnd()
            SyncLock Me
                _outputString = output
            End SyncLock
        End Sub

        ''' <summary>
        ''' Reader thread for standard error
        ''' 
        ''' <p/>Updates the private variable _errorString (locks it first)
        ''' </summary>
        Public Sub StandardErrorReader()
            Dim [error] As String = _processObject.StandardError.ReadToEnd()
            SyncLock Me
                _errorString = [error]
            End SyncLock
        End Sub

        ' Variables used to store property values (prefix: underscore "_")
        Private _command As Commands = Commands.SignAndEncrypt
        Private _armor As Boolean = True
        Private _yes As Boolean = True
        Private _recipient As String = ""
        Private _homedirectory As String = ""
        Private _bindirectory As String = ""
        Private _passphrase As String = ""
        Private _passphrasefd As String = ""
        Private _verbose As VerboseLevel = VerboseLevel.NoVerbose
        Private _batch As Boolean = True
        Private _originator As String = ""
        Private _ProcessTimeOutMilliseconds As Integer = 10000
        ' 10 seconds
        Private _exitcode As Integer = 0

        ' Variables used for monitoring external process and threads
        Private _processObject As Process
        Private _outputString As String
        Private _errorString As String

    End Class

End Namespace
