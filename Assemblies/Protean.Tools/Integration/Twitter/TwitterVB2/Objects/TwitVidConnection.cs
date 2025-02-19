using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

namespace Protean.Tools.Integration.Twitter.TwitterVB2
{
    public partial struct AGeoCode
    {
        public string Latitude;
        public string Longitude;
    }
    public partial class TwitVidConnection
    {
        private string m_strUsername = string.Empty;
        private string m_strPassword = string.Empty;
        private string m_strOauth = string.Empty;
        private DateTime m_dtTL;
        public string oAuthToken
        {
            get
            {
                return m_strOauth;
            }
        }

        public DateTime DateToLive
        {
            get
            {
                return m_dtTL;
            }
        }
        #region TwitVidConstants
        private const string TWITVID_AUTH_URL = "https://im.twitvid.com/api/authenticate";
        private const string TWITVID_UPLOAD_URL = "http://im.twitvid.com/api/upload";
        #endregion

        private void Authenticate()
        {
            try
            {
                HttpWebRequest tvAuthRequest;
                HttpWebResponse tvAuthResponse;

                string strPostData;
                byte[] bCredentials;

                var enCredentials = new ASCIIEncoding();

                strPostData = "username=" + m_strUsername + "&password=" + m_strPassword;

                bCredentials = enCredentials.GetBytes(strPostData);
                tvAuthRequest = (HttpWebRequest)System.Net.WebRequest.Create(TWITVID_AUTH_URL);
                tvAuthRequest.Method = "POST";
                tvAuthRequest.ContentType = "application/x-www-form-urlencoded";
                tvAuthRequest.ContentLength = strPostData.Length;

                using (var rwAuthenticate = new StreamWriter(tvAuthRequest.GetRequestStream()))
                {
                    rwAuthenticate.Write(strPostData);
                }
                tvAuthResponse = (HttpWebResponse)tvAuthRequest.GetResponse();
                var srAuth = new StreamReader(tvAuthResponse.GetResponseStream());
                string strResponse = srAuth.ReadToEnd();
                var xResponseAuth = new XmlDocument();
                xResponseAuth.LoadXml(strResponse);
                var xnRsp = xResponseAuth.SelectSingleNode("//rsp");
                if (xnRsp.Attributes["stat"].InnerText == "ok")
                {
                    m_strOauth = xnRsp.SelectSingleNode("//token").InnerText;
                    m_dtTL = DateTime.Now.AddMinutes(360d);

                }
            }



            catch (Exception)
            {

            }
        }
        public TwitVidConnection(string p_strUserName, string p_strPassword)
        {
            m_strUsername = p_strUserName;
            m_strPassword = p_strPassword;
            Authenticate();
        }
        /// <summary>
        /// uploads a video to the TwitVid service (without tweeting it)
        /// </summary>
        /// <param name="p_strFileName">FileName to upload</param>
        /// <param name="p_strMessage">Message to include</param>
        /// <returns>The MediaID for the file</returns>
        /// <remarks>This doesn't actually tweet out the video. The message is embedded in the video as metadata</remarks>
        public string Upload(string p_strFileName, string p_strMessage)
        {
            string UploadRet = default;
            UploadRet = string.Empty;

            if (DateTime.Now > m_dtTL)
            {
                Authenticate();
            }
            try
            {
                byte[] bMovieFile = File.ReadAllBytes(p_strFileName);
                string strBoundary = Guid.NewGuid().ToString();
                string strHeader = string.Format("--{0}", strBoundary);
                string strFooter = string.Format("--{0}--", strBoundary);
                HttpWebRequest rqUpload = (HttpWebRequest)System.Net.WebRequest.Create(TWITVID_UPLOAD_URL);
                rqUpload.PreAuthenticate = true;
                rqUpload.AllowWriteStreamBuffering = true;
                rqUpload.ContentType = string.Format("multipart/form-data; boundary={0}", strBoundary);
                rqUpload.Method = "POST";
                string strFileType = "application/octet-stream";

                string strFileHeader = string.Format("Content-Disposition: file; name=\"{0}\"; filename=\"{1}\"", "media", p_strFileName);
                string strFileData = Encoding.GetEncoding("iso-8859-1").GetString(bMovieFile);
                var strContents = new StringBuilder();
                strContents.AppendLine(strHeader);


                strContents.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "token"));
                strContents.AppendLine();
                strContents.AppendLine(m_strOauth);
                strContents.AppendLine(strHeader);
                strContents.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "message"));
                strContents.AppendLine();
                strContents.AppendLine(p_strMessage);
                strContents.AppendLine(strHeader);
                strContents.AppendLine(strFileHeader);
                strContents.AppendLine(string.Format("Content-Type: {0}", strFileType));
                strContents.AppendLine();
                strContents.AppendLine(strFileData);


                strContents.AppendLine(strFooter);

                byte[] bContents = Encoding.GetEncoding("iso-8859-1").GetBytes(strContents.ToString());
                rqUpload.ContentLength = bContents.Length;

                Stream rqStreamFile = rqUpload.GetRequestStream();
                rqStreamFile.Write(bContents, 0, bContents.Length);
                HttpWebResponse rspFileUpload = (HttpWebResponse)rqUpload.GetResponse();
                var rdrResponse = new StreamReader(rspFileUpload.GetResponseStream());
                string strResponse = rdrResponse.ReadToEnd();
                var xResponse = new XmlDocument();
                xResponse.LoadXml(strResponse);
                var xnRSP = xResponse.SelectSingleNode("//rsp");
                if (xnRSP.Attributes["stat"].Value == "ok")
                {
                    UploadRet = xnRSP.SelectSingleNode("//mediaurl").InnerText;
                }
                else
                {
                    UploadRet = strResponse;

                }
            }

            catch (Exception ex)
            {
                //Interaction.MsgBox(ex.Message);
            }
            return UploadRet;

        }
    }

}
