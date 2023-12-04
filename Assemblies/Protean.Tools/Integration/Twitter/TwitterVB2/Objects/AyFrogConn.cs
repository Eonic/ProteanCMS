using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

namespace Protean.Tools.Integration.Twitter.TwitterVB2
{
    public partial class AyFrogConn
    {
        private string m_strUserName;
        private string m_strPassword;
        private string m_strKey;
        private string[] m_arAllowedPhotos;
        private string[] m_arAllowedVideos;

        public enum yfFileType
        {
            yfVideo,
            yfPhoto
        }

        #region YF_CONSTANT_URL
        private const string YP_UPLOAD_AND_POST = "http://yfrog.com/api/uploadAndPost";
        #endregion

        #region YF_FUNCTIONS
        /// <summary>
        /// Uploads an image to yFrog and updates the user's twitter status
        /// </summary>
        /// <param name="p_strFileName">The file that should be uploaded</param>
        /// <param name="p_strText">Text of the tweet</param>
        /// <param name="p_bPublic">Whether or not it is public. Default is not (private)</param>
        /// 
        /// <param name="p_strTags">List of the tags that are to be appended.  Geocode is accepted here</param>
        /// <returns>The status ID that contains the link.</returns>
        /// <remarks></remarks>
        public string UploadAndPost(string p_strFileName, string p_strText, yfFileType p_FileType, bool p_bPublic = false, string p_strTags = "")
        {
            string UploadAndPostRet = default;
            UploadAndPostRet = string.Empty;

            if (string.IsNullOrEmpty(UserName))
            {
                throw new Exception("No username specififed");
                //return UploadAndPostRet;

            }
            if (string.IsNullOrEmpty(Password))
            {
                throw new Exception("No password specified");
                return UploadAndPostRet;
            }

            HttpWebRequest rqUploadtoYFrog = (HttpWebRequest)System.Net.WebRequest.Create(YP_UPLOAD_AND_POST);
            string strBoundary = Guid.NewGuid().ToString();
            string strheader = string.Format("--{0}", strBoundary);
            string strFooter = string.Format("--{0}--", strBoundary);
            rqUploadtoYFrog.PreAuthenticate = true;
            rqUploadtoYFrog.AllowWriteStreamBuffering = true;
            rqUploadtoYFrog.ContentType = string.Format("multipart/form-data boundary={0}", strBoundary);

            rqUploadtoYFrog.Method = "POST";
            var sbContent = new StringBuilder();


            if (p_strFileName.StartsWith("http://"))
            {
                sbContent.AppendLine(strheader);
                sbContent.AppendLine(string.Format("Content-Disposition: form-data name=\"{0}\"", "URL"));
                sbContent.AppendLine();
                sbContent.AppendLine(p_strFileName);
            }


            else
            {
                bool b_ValidExtention;
                b_ValidExtention = false;
                if (p_FileType == yfFileType.yfPhoto)
                {
                    foreach (string strEnding in m_arAllowedPhotos)
                    {
                        if (p_strFileName.EndsWith(strEnding))
                        {
                            b_ValidExtention = true;
                            break;

                        }
                    }
                }
                else if (p_FileType == yfFileType.yfVideo)
                {
                    foreach (string strEnding in m_arAllowedVideos)
                    {
                        if (p_strFileName.EndsWith(strEnding))
                        {
                            b_ValidExtention = true;
                            break;

                        }
                    }
                }

                if (!b_ValidExtention)
                {
                    throw new Exception("Invalid file format");
                }

                byte[] bFile = File.ReadAllBytes(p_strFileName);
                sbContent.AppendLine(strheader);
                string strFileHeader;

                strFileHeader = string.Format("Content-Disposition: file; name=\"{0}\"; filename=\"{1}\"", "media", p_strFileName);
                sbContent.AppendLine(strFileHeader);
                string strFileData = Encoding.GetEncoding("iso-8859-1").GetString(bFile);
                sbContent.AppendLine(string.Format("Content-Type: {0}", "application/octet-stream"));
                sbContent.AppendLine();

                sbContent.AppendLine(strFileData);


            }
            sbContent.AppendLine(strheader);

            sbContent.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "username"));
            sbContent.AppendLine();
            sbContent.AppendLine(UserName);
            sbContent.AppendLine(strheader);
            sbContent.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "password"));
            sbContent.AppendLine();
            sbContent.AppendLine(Password);

            if (p_strText.Length > 0 & p_strText.Length < 120)
            {
                sbContent.AppendLine(strheader);
                sbContent.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "message"));
                sbContent.AppendLine();
                sbContent.AppendLine(p_strText);
            }
            // Dim bIsNothing As Boolean = p_ltTagsa Is Nothing

            if (p_strTags.Length > 0)
            {
                sbContent.AppendLine(strheader);
                sbContent.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "tags"));
                sbContent.AppendLine();
                sbContent.AppendLine(p_strTags);
            }
            sbContent.AppendLine(strheader);
            sbContent.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "public"));
            sbContent.AppendLine();
            if (p_bPublic == true)
            {
                sbContent.AppendLine("yes");
            }
            else
            {
                sbContent.AppendLine("no");
            }
            sbContent.AppendLine(strheader);
            sbContent.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "key"));
            sbContent.AppendLine();
            sbContent.AppendLine(Key);
            sbContent.AppendLine(strFooter);
            byte[] bFormData = Encoding.GetEncoding("iso-8859-1").GetBytes(sbContent.ToString());
            rqUploadtoYFrog.ContentLength = bFormData.Length;
            Stream stUploadRequest = rqUploadtoYFrog.GetRequestStream();
            stUploadRequest.Write(bFormData, 0, bFormData.Length);
            HttpWebResponse rspuploadtoyfrog = (HttpWebResponse)rqUploadtoYFrog.GetResponse();
            var srUploadResponse = new StreamReader(rspuploadtoyfrog.GetResponseStream());
            string strUploadResponse = srUploadResponse.ReadToEnd();
            var xUploadResponse = new XmlDocument();
            xUploadResponse.LoadXml(strUploadResponse);
            var xnResponseNode = xUploadResponse.SelectSingleNode("//rsp");
            if (xnResponseNode.Attributes["stat"].InnerText == "ok")
            {
                UploadAndPostRet = xnResponseNode.SelectSingleNode("//mediaid").InnerText;
            }
            else
            {
                throw new Exception(xnResponseNode.SelectSingleNode("//err").Attributes["msg"].InnerText);

            }

            return UploadAndPostRet;

        }

        public string Key
        {
            get
            {
                return m_strKey;

            }
            set
            {
                m_strKey = value;

            }
        }
        public string UserName
        {
            get
            {
                return m_strUserName;

            }
            set
            {
                m_strUserName = value;

            }
        }

        public string Password
        {
            get
            {
                return m_strPassword;

            }
            set
            {
                m_strPassword = value;

            }
        }
        public AyFrogConn() : this("", "", "")
        {

        }

        public AyFrogConn(string p_strUserName, string p_Password, string p_strKey = "")
        {
            if (p_strUserName.Length > 0)
            {
                UserName = p_strUserName;

            }
            if (p_Password.Length > 0)
            {
                Password = p_Password;

            }
            if (p_strKey.Length > 0)
            {
                Key = p_strKey;

            }
            m_arAllowedPhotos = new string[] { ".jpeg", ".png", ".bmp", ".gif", ".jpg" };
            m_arAllowedVideos = new string[] { ".flv", ".mpeg", ".mkv", ".wmv", ".mov", ".3gp", ".mp4", ".avi", ".mpg" };


        }
        #endregion

    }
}
