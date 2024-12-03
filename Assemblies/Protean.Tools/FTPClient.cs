using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;

namespace Protean.Tools
{

    public class FTPClient
    {

        #region Declarations

        private string cHost;
        private string cUserName;
        private string cPassword;
        private const string mcModuleName = "Protean.Tools.FTPClient";
        public event OnErrorEventHandler OnError;

        public delegate void OnErrorEventHandler(object sender, Protean.Tools.Errors.ErrorEventArgs e);

        #endregion

        #region Properties

        public string Host
        {
            get
            {
                return cHost;
            }
        }

        public string Username
        {
            get
            {
                return cUserName;
            }
        }

        public string Password
        {
            get
            {
                return cPassword;
            }
        }

        #endregion

        #region Public Procedures

        public FTPClient(string HostAddress, string HostUserName, string HostPassword)
        {
            try
            {
                cHost = HostAddress;
                cUserName = HostUserName;
                cPassword = HostPassword;
                Connect();
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""));
            }
        }

        public void Connect()
        {
            try
            {
            }

            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Connect", ex, ""));
            }
        }

        public void Disconnect()
        {
            try
            {
            }

            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Disconnect", ex, ""));
            }
        }

        public bool Download(string LocalFile, string RemoteFile)
        {
            try
            {
                var outputStream = new FileStream(LocalFile, FileMode.Create);
                FtpWebRequest reqFTP = (FtpWebRequest)WebRequest.Create(new Uri(cHost + RemoteFile.Replace(@"\", "/")));
                reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                reqFTP.UseBinary = true;
                reqFTP.KeepAlive = true;
                reqFTP.Credentials = new NetworkCredential(cUserName, cPassword);
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpstream = response.GetResponseStream();
                long cl = response.ContentLength;
                int buffersize = 2048;
                int readcount;
                var buffer = new byte[buffersize + 1];
                readcount = ftpstream.Read(buffer, 0, buffersize);
                while (readcount > 0)
                {
                    outputStream.Write(buffer, 0, readcount);
                    readcount = ftpstream.Read(buffer, 0, buffersize);
                }
                ftpstream.Close();
                outputStream.Close();
                response.Close();
                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Download", ex, ""));
                return false;
            }
        }

        public bool UploadFile(string LocalFile, string RemoteFile)
        {
            try
            {
                FtpWebRequest reqFTP = (FtpWebRequest)WebRequest.Create(new Uri(cHost + RemoteFile.Replace(@"\", "/")));
                reqFTP.Method = WebRequestMethods.Ftp.UploadFile;
                reqFTP.UseBinary = true;
                reqFTP.Proxy = null;
                reqFTP.KeepAlive = true;
                reqFTP.Credentials = new NetworkCredential(cUserName, cPassword);
                var sourceStream = new StreamReader(LocalFile);
                byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
                sourceStream.Close();
                reqFTP.ContentLength = fileContents.Length;
                Stream requestStream = reqFTP.GetRequestStream();
                requestStream.Write(fileContents, 0, fileContents.Length);
                requestStream.Close();
                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Upload", ex, ""));
                return false;
            }
        }

        public bool UploadText(string FileText, string RemotePath, string FileName)
        {
            try
            {
                FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(new Uri(cHost + RemotePath + FileName));
                ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
                ftpRequest.Proxy = null;
                ftpRequest.UseBinary = true;
                ftpRequest.Credentials = new NetworkCredential(cUserName, cPassword);
                byte[] fileContents = Encoding.UTF8.GetBytes(FileText);
                Stream writer = ftpRequest.GetRequestStream();
                Debug.WriteLine("[" + DateTime.Now.ToLongTimeString() + "] Sending File");
                writer.Write(fileContents, 0, fileContents.Length);
                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Upload", ex, ""));
                return false;
            }
        }

        #endregion

    }
}
