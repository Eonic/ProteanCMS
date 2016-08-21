Imports System
Imports System.Net
Imports System.IO
Imports System.Text
Imports System.Text.ASCIIEncoding
Imports System.Net.Sockets
Imports System.Configuration
Imports System.Resources

Public Class FTPClient

#Region "Declarations"

    Private cHost As String
    Private cUserName As String
    Private cPassword As String
    Private Const mcModuleName As String = "Eonic.Tools.FTPClient"
    Public Event OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)

#End Region

#Region "Properties"

    Public ReadOnly Property Host() As String
        Get
            Return cHost
        End Get
    End Property

    Public ReadOnly Property Username() As String
        Get
            Return cUserName
        End Get
    End Property

    Public ReadOnly Property Password() As String
        Get
            Return cPassword
        End Get
    End Property

#End Region

#Region "Public Procedures"

    Public Sub New(ByVal HostAddress As String, ByVal HostUserName As String, ByVal HostPassword As String)
        Try
            cHost = HostAddress
            cUserName = HostUserName
            cPassword = HostPassword
            Connect()
        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""))
        End Try
    End Sub

    Public Sub Connect()
        Try

        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "Connect", ex, ""))
        End Try
    End Sub

    Public Sub Disconnect()
        Try

        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "Disconnect", ex, ""))
        End Try
    End Sub

    Public Function Download(ByVal LocalFile As String, ByVal RemoteFile As String) As Boolean
        Try
            Dim outputStream As FileStream = New FileStream(LocalFile, FileMode.Create)
            Dim reqFTP As FtpWebRequest = FtpWebRequest.Create(New Uri(Replace(cHost & RemoteFile, "\", "/")))
            reqFTP.Method = WebRequestMethods.Ftp.DownloadFile
            reqFTP.UseBinary = True
            reqFTP.KeepAlive = True
            reqFTP.Credentials = New NetworkCredential(cUserName, cPassword)
            Dim response As FtpWebResponse = reqFTP.GetResponse()
            Dim ftpstream As Stream = response.GetResponseStream()
            Dim cl As Long = response.ContentLength
            Dim buffersize As Integer = 2048
            Dim readcount As Integer
            Dim buffer(buffersize) As Byte
            readcount = ftpstream.Read(buffer, 0, buffersize)
            Do While (readcount > 0)
                outputStream.Write(buffer, 0, readcount)
                readcount = ftpstream.Read(buffer, 0, buffersize)
            Loop
            ftpstream.Close()
            outputStream.Close()
            response.Close()
            Return True
        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "Download", ex, ""))
            Return False
        End Try
    End Function

    Public Function UploadFile(ByVal LocalFile As String, ByVal RemoteFile As String) As Boolean
        Try
            Dim reqFTP As FtpWebRequest = FtpWebRequest.Create(New Uri(Replace(cHost & RemoteFile, "\", "/")))
            reqFTP.Method = WebRequestMethods.Ftp.UploadFile
            reqFTP.UseBinary = True
            reqFTP.Proxy = Nothing
            reqFTP.KeepAlive = True
            reqFTP.Credentials = New NetworkCredential(cUserName, cPassword)
            Dim sourceStream As New StreamReader(LocalFile)
            Dim fileContents() As Byte = System.Text.Encoding.UTF8.GetBytes(sourceStream.ReadToEnd())
            sourceStream.Close()
            reqFTP.ContentLength = fileContents.Length
            Dim requestStream As Stream = reqFTP.GetRequestStream()
            requestStream.Write(fileContents, 0, fileContents.Length)
            requestStream.Close()
            Return True
        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "Upload", ex, ""))
            Return False
        End Try
    End Function

    Public Function UploadText(ByVal FileText As String, ByVal RemotePath As String, ByVal FileName As String) As Boolean
        Try
            Dim ftpRequest As FtpWebRequest = FtpWebRequest.Create(New Uri(cHost & RemotePath & FileName))
            ftpRequest.Method = WebRequestMethods.Ftp.UploadFile
            ftpRequest.Proxy = Nothing
            ftpRequest.UseBinary = True
            ftpRequest.Credentials = New NetworkCredential(cUserName, cPassword)
            Dim fileContents As Byte() = System.Text.Encoding.UTF8.GetBytes(FileText)
            Dim writer As Stream = ftpRequest.GetRequestStream()
            Debug.WriteLine("[" & Now.ToLongTimeString & "] Sending File")
            writer.Write(fileContents, 0, fileContents.Length)
            Return True
        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "Upload", ex, ""))
            Return False
        End Try
    End Function

#End Region

End Class