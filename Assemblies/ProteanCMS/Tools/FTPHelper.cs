using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.FtpClient;
using System.Net.FtpClient.Extensions;
using Microsoft.VisualBasic.CompilerServices;

namespace Protean
{

    #region  FTPClient Helper 

    public class FTPHelper
    {

        // [ FTPClient Helper ]
        // 
        // // By Elektro

        #region  Variables 

        private FtpClient conn = new FtpClient();

        /// <summary>
    /// The FTP site.
    /// </summary>
        private string host { get; set; } = string.Empty;

        /// <summary>
    /// The user name.
    /// </summary>
        private string user { get; set; } = string.Empty;

        /// <summary>
    /// The user password.
    /// </summary>
        private string pass { get; set; } = string.Empty;

        // Friend m_reset As New ManualResetEvent(False) ' Use it for CallBacks

        #endregion

        #region  Constructor 

        /// <summary>
    /// .
    /// </summary>
    /// <param name="host">Indicates the ftp site.</param>
    /// <param name="user">Indicates the username.</param>
    /// <param name="pass">Indicates the password.</param>
        public FTPHelper(string host, string user, string pass)
        {

            if (!host.ToLower().StartsWith("ftp://"))
            {
                this.host = "ftp://" + host;
            }
            else
            {
                this.host = host;
            }

            if (Conversions.ToString(this.host.Last()) == "/")
            {
                this.host = this.host.Remove(this.host.Length - 1);
            }

            this.user = user;
            this.pass = pass;

            {
                ref var withBlock = ref conn;
                withBlock.Host = Conversions.ToString(host.Last()) == "/" ? host.Remove(host.Length - 1) : host;
                withBlock.Credentials = new NetworkCredential(this.user, this.pass);
            }

        }

        #endregion

        #region  Public Methods 

        /// <summary>
    /// Connects to server.
    /// </summary>
        public void Connect()
        {
            conn.Connect();
        }

        /// <summary>
    /// Disconnects from server.
    /// </summary>
        public void Disconnect()
        {
            conn.Disconnect();
        }

        /// <summary>
    /// Creates a directory on server.
    /// </summary>
    /// <param name="directorypath">Indicates the ftp directory path.</param>
    /// <param name="force">Try to force all non-existant pieces of the path to be created.</param>
        public void CreateDirectory(string directorypath, bool force)
        {
            conn.CreateDirectory(directorypath, force);
        }

        /// <summary>
    /// Creates a directory on server.
    /// </summary>
    /// <param name="directorypath">Indicates the ftp directory path.</param>
    /// <param name="force">Try to force all non-existant pieces of the path to be created.</param>
    /// <param name="FtpListOption">Options that dictate how a list is performed ans what information is gathered.</param>
        public void DeleteDirectory(string directorypath, bool force, FtpListOption FtpListOption = FtpListOption.AllFiles | FtpListOption.ForceList)
        {

            // Remove the directory and all objects beneath it. The last parameter
            // forces System.Net.FtpClient to use LIST -a for getting a list of objects
            // beneath the specified directory.
            conn.DeleteDirectory(directorypath, force, FtpListOption);

        }

        /// <summary>
    /// Deletes a file on server.
    /// </summary>
    /// <param name="filepath">Indicates the ftp file path.</param>
        public void DeleteFile(string filepath)
        {
            conn.DeleteFile(filepath);
        }

        /// <summary>
    /// Checks if a directory exist on server.
    /// </summary>
    /// <param name="directorypath">Indicates the ftp directory path.</param>
        public bool DirectoryExists(string directorypath)
        {
            return conn.DirectoryExists(directorypath);
        }

        /// <summary>
    /// Executes a command on server.
    /// </summary>
    /// <param name="command">Indicates the command to execute on the server.</param>
    /// <returns>Returns an object containing the server reply information.</returns>
        public FtpReply Execute(string command)
        {
            var argtarget = new FtpReply();
            return InlineAssignHelper(ref argtarget, conn.Execute(command));
        }

        /// <summary>
    /// Tries to execute a command on server.
    /// </summary>
    /// <param name="command">Indicates the command to execute on the server.</param>
    /// <returns>Returns TRUE if command execution successfull, otherwise returns False.</returns>
        public bool TryExecute(string command)
        {
            FtpReply reply = default;
            return InlineAssignHelper(ref reply, conn.Execute(command)).Success;
        }

        /// <summary>
    /// Checks if a file exist on server.
    /// </summary>
    /// <param name="filepath">Indicates the ftp file path.</param>
    /// <param name="FtpListOption">Options that dictate how a list is performed ans what information is gathered.</param>
        public bool FileExists(string filepath, FtpListOption FtpListOption = FtpListOption.AllFiles | FtpListOption.ForceList)
        {

            // The last parameter forces System.Net.FtpClient to use LIST -a 
            // for getting a list of objects in the parent directory.
            return conn.FileExists(filepath, FtpListOption);

        }

        /// <summary>
    /// Retrieves a checksum of the given file
    /// using a checksumming method that the server supports, if any.
    /// The algorithm used goes in this order: 
    /// 1. HASH command (server preferred algorithm).
    /// 2. MD5 / XMD5 commands 
    /// 3. XSHA1 command 
    /// 4. XSHA256 command 
    /// 5. XSHA512 command
    /// 6. XCRC command
    /// </summary>
    /// <param name="filepath">Indicates the ftp file path.</param>
        public FtpHash GetChecksum(string filepath)
        {
            return conn.GetChecksum(filepath);
        }

        /// <summary>
    /// Gets the checksum of file on server and compare it with the checksum of local file.
    /// </summary>
    /// <param name="filepath">Indicates the ftp file path.</param>
    /// <param name="localfilepath">Indicates the local disk file path.</param>
    /// <param name="algorithm">Indicates the algorithm that should be used to verify checksums.</param>
    /// <returns>Returns TRUE if both checksums are equal, otherwise returns False.</returns>
        public bool VerifyChecksum(string filepath, string localfilepath, FtpHashAlgorithm algorithm)
        {

            FtpHash hash = null;

            hash = conn.GetChecksum(filepath);
            // Make sure it returned a, to the best of our knowledge, valid hash object. 
            // The commands for retrieving checksums are
            // non-standard extensions to the protocol so we have to
            // presume that the response was in a format understood by
            // System.Net.FtpClient and parsed correctly.
            // 
            // In addition, there is no built-in support for verifying CRC hashes. 
            // You will need to write you own or use a third-party solution.
            if (hash.IsValid && hash.Algorithm != algorithm)
            {
                return hash.Verify(localfilepath);
            }
            else
            {
                return default;
            }

        }

        /// <summary>
    /// Gets the size of file.
    /// </summary>
    /// <param name="filepath">Indicates the ftp file path.</param>
        public long GetFileSize(string filepath)
        {
            return conn.GetFileSize(filepath);
        }

        /// <summary>
    /// Gets the currently HASH algorithm used for the HASH command on server.
    /// </summary>
        public FtpHashAlgorithm GetHashAlgorithm()
        {
            return conn.GetHashAlgorithm();
        }

        /// <summary>
    /// Gets the modified time of file.
    /// </summary>
    /// <param name="filepath">Indicates the ftp file path.</param>
        public DateTime GetModifiedTime(string filepath)
        {
            return conn.GetModifiedTime(filepath);
        }

        /// <summary>
    /// Returns a file/directory listing using the NLST command.
    /// </summary>
    /// <param name="directorypath">Indicates the ftp file path.</param>
        public string[] GetNameListing(string directorypath)
        {
            return conn.GetNameListing(directorypath);
        }

        /// <summary>
    /// Gets the current working directory on server.
    /// </summary>
        public string GetWorkingDirectory()
        {
            return conn.GetWorkingDirectory();
        }

        /// <summary>
    /// Opens the specified file to be appended to...
    /// </summary>
    /// <param name="filepath">Indicates the ftp file path.</param>
        public System.IO.Stream OpenAppend(string filepath)
        {
            return conn.OpenAppend(filepath);
        }

        /// <summary>
    /// Opens the specified file for reading.
    /// </summary>
    /// <param name="filepath">Indicates the ftp file path.</param>
        public System.IO.Stream OpenRead(string filepath)
        {
            return conn.OpenRead(filepath);
        }

        /// <summary>
    /// Opens the specified file for writing.
    /// </summary>
    /// <param name="filepath">Indicates the ftp file path.</param>
        public System.IO.Stream OpenWrite(string filepath)
        {
            return conn.OpenWrite(filepath);
        }

        /// <summary>
    /// Rename a file on the server.
    /// </summary>
    /// <param name="filepath">Indicates the ftp file path.</param>
    /// <param name="newfilepath">Indicates the new ftp file path.</param>
        public void RenameFile(string filepath, string newfilepath)
        {
            if (conn.FileExists(filepath))
            {
                conn.Rename(filepath, newfilepath);
            }
            else
            {
                throw new Exception(filepath + " File does not exist on server.");
            }
        }

        /// <summary>
    /// Rename a directory on the server.
    /// </summary>
    /// <param name="directorypath">Indicates the ftp file path.</param>
    /// <param name="newdirectorypath">Indicates the new ftp file path.</param>
        public void RenameDirectory(string directorypath, string newdirectorypath)
        {
            if (conn.DirectoryExists(directorypath))
            {
                conn.Rename(directorypath, newdirectorypath);
            }
            else
            {
                throw new Exception(directorypath + " Directory does not exist on server.");
            }
        }

        /// <summary>
    /// Tells the server wich hash algorithm to use for the HASH command.
    /// </summary>
    /// <param name="algorithm">Indicates the HASH algorithm.</param>
        public bool SetHashAlgorithm(FtpHashAlgorithm algorithm)
        {
            if (conn.HashAlgorithms.HasFlag(algorithm))
            {
                conn.SetHashAlgorithm(algorithm);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
    /// Sets the working directory on the server.
    /// </summary>
    /// <param name="directorypath">Indicates the ftp directory path.</param>
        public void SetWorkingDirectory(string directorypath)
        {
            conn.SetWorkingDirectory(directorypath);
        }

        /// <summary>
    /// Gets a directory list on the specified path.
    /// </summary>
    /// <param name="directorypath">Indicates the ftp directory path.</param>
    /// <param name="FtpListOption">Options that dictate how a list is performed ans what information is gathered.</param>
        public FtpListItem[] GetDirectories(string directorypath, FtpListOption FtpListOption = FtpListOption.AllFiles)
        {

            return (FtpListItem[])conn.GetListing(directorypath, FtpListOption).Where(item => item.Type == FtpFileSystemObjectType.Directory);

        }

        /// <summary>
    /// Gets a file list on the specified path.
    /// </summary>
    /// <param name="directorypath">Indicates the ftp directory path.</param>
    /// <param name="FtpListOption">Options that dictate how a list is performed ans what information is gathered.</param>
        public FtpListItem[] GetFiles(string directorypath, FtpListOption FtpListOption = FtpListOption.AllFiles)
        {

            return (FtpListItem[])conn.GetListing(directorypath, FtpListOption).Where(item => item.Type == FtpFileSystemObjectType.File);

        }

        /// <summary>
    /// Gets a link list on the specified path.
    /// </summary>
    /// <param name="directorypath">Indicates the ftp directory path.</param>
    /// <param name="FtpListOption">Options that dictate how a list is performed ans what information is gathered.</param>

        public FtpListItem[] GetLinks(string directorypath, FtpListOption FtpListOption = FtpListOption.AllFiles)
        {

            return (FtpListItem[])conn.GetListing(directorypath, FtpListOption).Where(item => item.Type == FtpFileSystemObjectType.Link);

        }

        /// <summary>
    /// Gets a file/folder list on the specified path.
    /// </summary>
    /// <param name="directorypath">Indicates the ftp directory path.</param>
    /// <param name="FtpListOption">Options that dictate how a list is performed ans what information is gathered.</param>
        public FtpListItem[] GetListing(string directorypath, FtpListOption FtpListOption = FtpListOption.AllFiles)
        {

            return conn.GetListing(directorypath, FtpListOption);

        }

        /// <summary>
    /// Log to a console window
    /// </summary>
        public void LogToConsole()
        {
            FtpTrace.AddListener(new ConsoleTraceListener());
            // now use System.Net.FtpCLient as usual and the server transactions
            // will be written to the Console window.
        }

        /// <summary>
    /// Log to a text file
    /// </summary>
    /// <param name="filepath">Indicates the file where to save the log.</param>
        public void LogToFile(string filepath)
        {
            FtpTrace.AddListener(new TextWriterTraceListener(filepath));
            // now use System.Net.FtpCLient as usual and the server transactions
            // will be written to the specified log file.
        }

        /// <summary>
    /// Uploads a file to FTP.
    /// </summary>
    /// <param name="UploadClient">Indicates the WebClient object to upload the file.</param>
    /// <param name="filepath">Indicates the ftp fle path.</param>
    /// <param name="localfilepath">Specifies the local path where to save the downloaded file.</param>
    /// <param name="Asynchronous">Indicates whether the download should be an Asynchronous operation, 
    /// to raise WebClient events.</param>
        public void UploadFile(ref WebClient UploadClient, string localfilepath, string filepath = null, bool Asynchronous = false)
        {

            if (filepath is null)
            {
                filepath = host + "/" + new System.IO.FileInfo(localfilepath).Name;
            }
            else if (filepath.StartsWith("/"))
            {
                filepath = host + filepath;
            }
            else
            {
                filepath = host + "/" + filepath;
            }

            UploadClient.Credentials = new NetworkCredential(user, pass);
            if (Asynchronous)
            {
                UploadClient.UploadFileAsync(new Uri(filepath), "STOR", localfilepath);
            }
            else
            {
                UploadClient.UploadFile(new Uri(filepath), "STOR", localfilepath);
            }

        }

        /// <summary>
    /// Downloads a file from FTP.
    /// </summary>
    /// <param name="DownloadClient">Indicates the WebClient object to download the file.</param>
    /// <param name="filepath">Indicates the ftp fle path.</param>
    /// <param name="localfilepath">Specifies the local path where to save the downloaded file.</param>
    /// <param name="Asynchronous">Indicates whether the download should be an Asynchronous operation, 
    /// to raise WebClient events.</param>
        public void DownloadFile(ref WebClient DownloadClient, string filepath, string localfilepath, bool Asynchronous = false)
        {

            if (filepath.StartsWith("/"))
            {
                filepath = host + filepath;
            }
            else
            {
                filepath = host + "/" + filepath;
            }

            DownloadClient.Credentials = new NetworkCredential(user, pass);
            if (Asynchronous)
            {
                DownloadClient.DownloadFileAsync(new Uri(filepath), localfilepath);
            }
            else
            {
                DownloadClient.DownloadFile(new Uri(filepath), localfilepath);
            }

        }

        #endregion

        #region  Miscellaneous methods 

        private static T InlineAssignHelper<T>(ref T target, T value)
        {
            target = value;
            return value;
        }

        #endregion

    }
}

#endregion



