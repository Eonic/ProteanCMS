using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using Microsoft.VisualBasic.CompilerServices; // Install-Package Microsoft.VisualBasic

namespace Protean.Tools.Integration.Twitter.TwitterVB2
{
    public partial class ATwiCliConnection
    {

        #region TCC Constants
        private const string TCC_UPLOAD_PHOTO_URL = "http://twic.li/api/uploadPhoto";
        private const string TCC_UPLOAD_PHOTO_AND_TWEET_URL = "http://twic.li/api/uploadPhotoAndTweet";
        private const string TCC_UPLOAD_VIDEO_URL = "http://twic.li/api/uploadVideo";
        private const string TCC_UPLOAD_VIDEO_AND_TWEET_URL = "http://twic.li/api/uploadVideoAndTweet";
        private const string TCC_UPLOAD_AUDIO_URL = "http://twic.li/api/uploadAudio";
        private const string TCC_UPLOAD_AUDIO_AND_TWEET_URL = "http://twic.li/api/uploadAudioAndTweet";
        private const string TCC_GET_CONTENT_INFO = "http://twic.li/api/getContent";
        private const string TCC_GET_USERS_CONTENT_URL = "http://twic.li/api/getUsersContent";
        #endregion
        private string m_strUserName;
        private string m_strPassword;
        private string m_strAPIKey;

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

        public string APIKey
        {
            get
            {
                return m_strAPIKey;

            }
            set
            {
                m_strAPIKey = value;

            }
        }





        public ATwiCliConnection(string p_strUserName, string p_strPassword, string p_strAPIKey = "<nokey>")
        {
            UserName = p_strUserName;
            Password = p_strPassword;
            if (p_strAPIKey != "<nokey>")
            {
                APIKey = p_strAPIKey;
            }
        }


        /// <summary>
        /// Uploads a Photo to the twicli service without posting a tweet
        /// </summary>
        /// <param name="p_strFileName">The filename of the picture to upload (note, must be a jpg, gif, or png)</param>
        /// 
        /// <param name="p_bShowMap">Whether or not to show a google map (EXIF geodata requried)</param>
        /// <returns>An object representing the photograph now stored on the twic.li server</returns>
        /// <remarks></remarks>
        public ATwiCliPhotoFile UploadPhoto(string p_strFileName, bool p_bShowMap = false)
        {
            ATwiCliPhotoFile UploadPhotoRet = default;
            UploadPhotoRet = new ATwiCliPhotoFile();


            try
            {
                string strFileType;
                if (p_strFileName.ToLower().EndsWith(".jpg") | p_strFileName.ToLower().EndsWith(".jpeg"))
                {
                    strFileType = "image/jpeg";
                }
                else if (p_strFileName.ToLower().EndsWith(".gif"))
                {
                    strFileType = "image/gif";
                }
                else if (p_strFileName.ToLower().EndsWith(".png"))
                {
                    strFileType = "image/png";
                }
                else
                {
                    throw new Exception("Invalid photo file");

                }


                byte[] bPhoto = File.ReadAllBytes(p_strFileName);
                string strBoundary = Guid.NewGuid().ToString();
                string strHeader = string.Format("--{0}", strBoundary);
                string strFooter = string.Format("--{0}--", strBoundary);
                HttpWebRequest rqUploadPhoto = (HttpWebRequest)System.Net.WebRequest.Create(TCC_UPLOAD_PHOTO_URL);
                string strFileHeader = string.Format("Content-Disposition: file; name=\"{0}\"; filename=\"{1}\"", "photo", p_strFileName);
                string strFileData = Encoding.GetEncoding("iso-8859-1").GetString(bPhoto);
                var sbContents = new StringBuilder();
                rqUploadPhoto.PreAuthenticate = true;
                rqUploadPhoto.AllowWriteStreamBuffering = true;
                rqUploadPhoto.ContentType = string.Format("multipart/form-data; boundary={0}", strBoundary);
                rqUploadPhoto.Method = "POST";
                sbContents.AppendLine(strHeader);
                sbContents.AppendLine(strFileHeader);
                sbContents.AppendLine(string.Format("Content-Type: {0}", strFileType));
                sbContents.AppendLine();
                sbContents.AppendLine(strFileData);
                sbContents.AppendLine(strHeader);
                sbContents.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "username"));
                sbContents.AppendLine();
                sbContents.AppendLine(UserName);
                sbContents.AppendLine(strHeader);
                sbContents.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "password"));
                sbContents.AppendLine();
                sbContents.AppendLine(Password);
                sbContents.AppendLine(strHeader);
                sbContents.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "show_map"));
                sbContents.AppendLine();
                if (p_bShowMap == true)
                {
                    sbContents.AppendLine("1");
                }
                else
                {
                    sbContents.AppendLine("0");
                }
                if (APIKey.Length > 0)
                {
                    sbContents.AppendLine(strHeader);
                    sbContents.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "api_key"));
                    sbContents.AppendLine();
                    sbContents.AppendLine(APIKey);
                }
                sbContents.AppendLine(strFooter);

                byte[] bFormData = Encoding.GetEncoding("iso-8859-1").GetBytes(sbContents.ToString());
                rqUploadPhoto.ContentLength = bFormData.Length;
                Stream stPhotoRequest = rqUploadPhoto.GetRequestStream();
                stPhotoRequest.Write(bFormData, 0, bFormData.Length);
                HttpWebResponse rpUploadPhoto = (HttpWebResponse)rqUploadPhoto.GetResponse();
                var srUploadPhoto = new StreamReader(rpUploadPhoto.GetResponseStream());
                string strUploadResult = srUploadPhoto.ReadToEnd();
                var xUploadResult = new XmlDocument();
                xUploadResult.LoadXml(strUploadResult);
                var xnResponse = xUploadResult.SelectSingleNode("//response");
                if (xnResponse.Attributes["status"].InnerText == "ok")
                {
                    // UploadPhoto = xnResponse.SelectSingleNode("//url").InnerText
                    UploadPhotoRet.ID = xnResponse.SelectSingleNode("//id").InnerText;
                    UploadPhotoRet.ScreenName = xnResponse.SelectSingleNode("//screen_name").InnerText;
                    UploadPhotoRet.UserID = xnResponse.SelectSingleNode("//user_id").InnerText;
                    UploadPhotoRet.URL = xnResponse.SelectSingleNode("//url").InnerText;
                    UploadPhotoRet.Comments_Count = Conversions.ToInteger(xnResponse.SelectSingleNode("//num_comments").InnerText);
                    UploadPhotoRet.UserTags_Count = Conversions.ToInteger(xnResponse.SelectSingleNode("//num_user_tags").InnerText);
                    UploadPhotoRet.Views_Count = Conversions.ToInteger(xnResponse.SelectSingleNode("//num_views").InnerText);
                    if (p_bShowMap)
                    {
                        UploadPhotoRet.ShowMap = true;
                        UploadPhotoRet.Latitude = Conversions.ToDouble(xnResponse.SelectSingleNode("//latitude").InnerText);
                        UploadPhotoRet.Longitude = Conversions.ToDouble(xnResponse.SelectSingleNode("//longitude").InnerText);
                    }
                    else
                    {
                        UploadPhotoRet.ShowMap = false;
                    }
                    UploadPhotoRet.CameraMake = xnResponse.SelectSingleNode("//camera_make").InnerText;
                    UploadPhotoRet.CameraModel = xnResponse.SelectSingleNode("//camera_model").InnerText;
                    UploadPhotoRet.InsertUnixTime(Conversions.ToDouble(xnResponse.SelectSingleNode("//timestamp").InnerText));
                }



                else
                {
                    // UploadPhoto = xnResponse.SelectSingleNode("//error_text").InnerText
                    throw new Exception(xnResponse.SelectSingleNode("//error_text").InnerText);
                }
                return UploadPhotoRet;
            }

            catch (Exception ex)
            {

            }
            return UploadPhotoRet;
        }
        /// <summary>
        /// Uploads a photo AND updates the users status
        /// </summary>
        /// <param name="p_strFileName">File name of the picture</param>
        /// <param name="p_strTweet">status text</param>
        /// <param name="p_bShowMap">Whehter or not to show a google map beside the photo (EXIF data must be encoded if this option is selected)</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public ATwiCliPhotoFile UploadPhotoAndTweet(string p_strFileName, string p_strTweet, bool p_bShowMap = true)
        {
            ATwiCliPhotoFile UploadPhotoAndTweetRet = default;
            UploadPhotoAndTweetRet = new ATwiCliPhotoFile();
            try
            {
                string strFileType;
                if (p_strFileName.ToLower().EndsWith(".jpg") | p_strFileName.ToLower().EndsWith(".jpeg"))
                {
                    strFileType = "image/jpeg";
                }
                else if (p_strFileName.ToLower().EndsWith(".gif"))
                {
                    strFileType = "image/gif";
                }
                else if (p_strFileName.ToLower().EndsWith(".png"))
                {
                    strFileType = "image/png";
                }
                else
                {
                    // Return String.Empty
                    throw new Exception("Invalid file format");

                }


                byte[] bPhoto = File.ReadAllBytes(p_strFileName);
                string strBoundary = Guid.NewGuid().ToString();
                string strHeader = string.Format("--{0}", strBoundary);
                string strFooter = string.Format("--{0}--", strBoundary);
                HttpWebRequest rqUploadPhoto = (HttpWebRequest)System.Net.WebRequest.Create(TCC_UPLOAD_PHOTO_AND_TWEET_URL);
                string strFileHeader = string.Format("Content-Disposition: file; name=\"{0}\"; filename=\"{1}\"", "photo", p_strFileName);
                string strFileData = Encoding.GetEncoding("iso-8859-1").GetString(bPhoto);
                var sbContents = new StringBuilder();
                rqUploadPhoto.PreAuthenticate = true;
                rqUploadPhoto.AllowWriteStreamBuffering = true;
                rqUploadPhoto.ContentType = string.Format("multipart/form-data; boundary={0}", strBoundary);
                rqUploadPhoto.Method = "POST";
                sbContents.AppendLine(strHeader);
                sbContents.AppendLine(strFileHeader);
                sbContents.AppendLine(string.Format("Content-Type: {0}", strFileType));
                sbContents.AppendLine();
                sbContents.AppendLine(strFileData);
                sbContents.AppendLine(strHeader);
                sbContents.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "username"));
                sbContents.AppendLine();
                sbContents.AppendLine(UserName);
                sbContents.AppendLine(strHeader);
                sbContents.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "password"));
                sbContents.AppendLine();
                sbContents.AppendLine(Password);
                sbContents.AppendLine(strHeader);
                sbContents.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "tweet"));
                sbContents.AppendLine();
                sbContents.AppendLine(p_strTweet);
                sbContents.AppendLine(strHeader);
                sbContents.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "show_map"));
                sbContents.AppendLine();
                if (p_bShowMap == true)
                {
                    sbContents.AppendLine("1");
                }
                else
                {
                    sbContents.AppendLine("0");
                }
                if (APIKey.Length > 0)
                {
                    sbContents.AppendLine(strHeader);
                    sbContents.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "api_key"));
                    sbContents.AppendLine();
                    sbContents.AppendLine(APIKey);
                }
                sbContents.AppendLine(strFooter);

                byte[] bFormData = Encoding.GetEncoding("iso-8859-1").GetBytes(sbContents.ToString());
                rqUploadPhoto.ContentLength = bFormData.Length;
                Stream stPhotoRequest = rqUploadPhoto.GetRequestStream();
                stPhotoRequest.Write(bFormData, 0, bFormData.Length);
                HttpWebResponse rpUploadPhoto = (HttpWebResponse)rqUploadPhoto.GetResponse();
                var srUploadPhoto = new StreamReader(rpUploadPhoto.GetResponseStream());
                string strUploadResult = srUploadPhoto.ReadToEnd();
                var xUploadResult = new XmlDocument();
                xUploadResult.LoadXml(strUploadResult);
                var xnResponse = xUploadResult.SelectSingleNode("//response");
                if (xnResponse.Attributes["status"].InnerText == "ok")
                {
                    // UploadPhotoAndTweet = xnResponse.SelectSingleNode("//url").InnerText
                    UploadPhotoAndTweetRet.ID = xnResponse.SelectSingleNode("//id").InnerText;
                    UploadPhotoAndTweetRet.ScreenName = xnResponse.SelectSingleNode("//screen_name").InnerText;
                    UploadPhotoAndTweetRet.UserID = xnResponse.SelectSingleNode("//user_id").InnerText;
                    UploadPhotoAndTweetRet.URL = xnResponse.SelectSingleNode("//url").InnerText;
                    UploadPhotoAndTweetRet.Comments_Count = Conversions.ToInteger(xnResponse.SelectSingleNode("//num_comments").InnerText);
                    UploadPhotoAndTweetRet.UserTags_Count = Conversions.ToInteger(xnResponse.SelectSingleNode("//num_user_tags").InnerText);
                    UploadPhotoAndTweetRet.Views_Count = Conversions.ToInteger(xnResponse.SelectSingleNode("//num_views").InnerText);
                    if (p_bShowMap)
                    {
                        UploadPhotoAndTweetRet.ShowMap = true;
                        UploadPhotoAndTweetRet.Latitude = Conversions.ToDouble(xnResponse.SelectSingleNode("//latitude").InnerText);
                        UploadPhotoAndTweetRet.Longitude = Conversions.ToDouble(xnResponse.SelectSingleNode("//longitude").InnerText);
                    }
                    else
                    {
                        UploadPhotoAndTweetRet.ShowMap = false;
                    }
                    UploadPhotoAndTweetRet.CameraMake = xnResponse.SelectSingleNode("//camera_make").InnerText;
                    UploadPhotoAndTweetRet.CameraModel = xnResponse.SelectSingleNode("//camera_model").InnerText;
                    UploadPhotoAndTweetRet.InsertUnixTime(Conversions.ToDouble(xnResponse.SelectSingleNode("//timestamp").InnerText));
                }
                else
                {
                    // UploadPhotoAndTweet = xnResponse.SelectSingleNode("//error_text").InnerText
                    throw new Exception(xnResponse.SelectSingleNode("//error_text").InnerText);


                }
            }



            catch (Exception ex)
            {

            }
            return UploadPhotoAndTweetRet;
        }

        public ATwiCliVideoFile UploadVideo(string p_strFileName, bool p_bShowMap = false)
        {
            ATwiCliVideoFile UploadVideoRet = default;
            UploadVideoRet = new ATwiCliVideoFile();

            try
            {
                string strFileType;
                if (p_strFileName.ToLower().EndsWith(".mpg") | p_strFileName.ToLower().EndsWith(".mpeg"))
                {
                    strFileType = "video/mpeg";
                }
                else if (p_strFileName.ToLower().EndsWith(".3gp"))
                {
                    strFileType = "video/3gpp";
                }
                else if (p_strFileName.ToLower().EndsWith(".wmv"))
                {
                    strFileType = "application/octet-stream";
                }
                else if (p_strFileName.ToLower().EndsWith(".mp4"))
                {
                    strFileType = "application/octet-stream";
                }
                else if (p_strFileName.ToLower().EndsWith(".mov"))
                {
                    strFileType = "application/octet-stream";
                }
                else if (p_strFileName.ToLower().EndsWith(".flv"))
                {
                    strFileType = "applicaiton/octet-stream";
                }
                else
                {
                    throw new Exception("Invalid video format");

                }


                byte[] bVideo = File.ReadAllBytes(p_strFileName);
                string strBoundary = Guid.NewGuid().ToString();
                string strHeader = string.Format("--{0}", strBoundary);
                string strFooter = string.Format("--{0}--", strBoundary);
                HttpWebRequest rqUploadVideo = (HttpWebRequest)System.Net.WebRequest.Create(TCC_UPLOAD_VIDEO_URL);
                string strFileHeader = string.Format("Content-Disposition: file; name=\"{0}\"; filename=\"{1}\"", "video", p_strFileName);
                string strFileData = Encoding.GetEncoding("iso-8859-1").GetString(bVideo);
                var sbContents = new StringBuilder();
                rqUploadVideo.PreAuthenticate = true;
                rqUploadVideo.AllowWriteStreamBuffering = true;
                rqUploadVideo.ContentType = string.Format("multipart/form-data; boundary={0}", strBoundary);
                rqUploadVideo.Method = "POST";
                sbContents.AppendLine(strHeader);
                sbContents.AppendLine(strFileHeader);
                sbContents.AppendLine(string.Format("Content-Type: {0}", strFileType));
                sbContents.AppendLine();
                sbContents.AppendLine(strFileData);
                sbContents.AppendLine(strHeader);
                sbContents.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "username"));
                sbContents.AppendLine();
                sbContents.AppendLine(UserName);
                sbContents.AppendLine(strHeader);
                sbContents.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "password"));
                sbContents.AppendLine();
                sbContents.AppendLine(Password);
                sbContents.AppendLine(strHeader);
                sbContents.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "show_map"));
                sbContents.AppendLine();
                if (p_bShowMap == true)
                {
                    sbContents.AppendLine("1");
                }
                else
                {
                    sbContents.AppendLine("0");
                }
                if (APIKey.Length > 0)
                {
                    sbContents.AppendLine(strHeader);
                    sbContents.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "api_key"));
                    sbContents.AppendLine();
                    sbContents.AppendLine(APIKey);
                }
                sbContents.AppendLine(strFooter);

                byte[] bFormData = Encoding.GetEncoding("iso-8859-1").GetBytes(sbContents.ToString());
                rqUploadVideo.ContentLength = bFormData.Length;
                Stream stVideoRequest = rqUploadVideo.GetRequestStream();
                stVideoRequest.Write(bFormData, 0, bFormData.Length);
                HttpWebResponse rpUploadVideo = (HttpWebResponse)rqUploadVideo.GetResponse();
                var srUploadVideo = new StreamReader(rpUploadVideo.GetResponseStream());
                string strUploadResult = srUploadVideo.ReadToEnd();
                var xUploadResult = new XmlDocument();
                xUploadResult.LoadXml(strUploadResult);
                var xnResponse = xUploadResult.SelectSingleNode("//response");
                if (xnResponse.Attributes["status"].InnerText == "ok")
                {
                    // UploadVideo = xnResponse.SelectSingleNode("//url").InnerText
                    UploadVideoRet.ID = xnResponse.SelectSingleNode("//id").InnerText;
                    UploadVideoRet.ScreenName = xnResponse.SelectSingleNode("//screen_name").InnerText;
                    UploadVideoRet.UserID = xnResponse.SelectSingleNode("//user_id").InnerText;

                    UploadVideoRet.URL = xnResponse.SelectSingleNode("//url").InnerText;
                    UploadVideoRet.Comments_Count = Conversions.ToInteger(xnResponse.SelectSingleNode("//num_comments").InnerText);
                    UploadVideoRet.Views_Count = Conversions.ToInteger(xnResponse.SelectSingleNode("//num_views").InnerText);
                    UploadVideoRet.InsertUnixTime(Conversions.ToDouble(xnResponse.SelectSingleNode("//timestamp").InnerText));
                    UploadVideoRet.ThumbNailURL = xnResponse.SelectSingleNode("//thumb_image_url").InnerText;
                }

                else
                {
                    throw new Exception(xnResponse.SelectSingleNode("//error_text").InnerText);
                    // UploadVideo = xnResponse.SelectSingleNode("//error_text").InnerText
                }
            }



            catch (Exception ex)
            {

            }
            return UploadVideoRet;
        }

        /// <summary>
        /// Uploads a video and updates the user's status
        /// </summary>
        /// <param name="p_strFileName">The file name to be uploaded</param>
        /// <param name="p_strTweet">Tweet that should be uploaded along with the file. If empty just the URL is posted</param>
        /// <param name="p_bShowMap">Optional to show a google map along with the image.  Note if this is used the EXIF data must be present in the file</param>
        /// <returns>An ATwiCliVideo file containing information regarding the file</returns>
        /// <remarks></remarks>
        public ATwiCliVideoFile UploadVideoAndTweet(string p_strFileName, string p_strTweet, bool p_bShowMap = false)
        {
            ATwiCliVideoFile UploadVideoAndTweetRet = default;
            UploadVideoAndTweetRet = new ATwiCliVideoFile();

            try
            {
                string strFileType;
                if (p_strFileName.ToLower().EndsWith(".mpg") | p_strFileName.ToLower().EndsWith(".mpeg"))
                {
                    strFileType = "video/mpeg";
                }
                else if (p_strFileName.ToLower().EndsWith(".3gp"))
                {
                    strFileType = "video/3gpp";
                }
                else if (p_strFileName.ToLower().EndsWith(".wmv"))
                {
                    strFileType = "application/octet-stream";
                }
                else if (p_strFileName.ToLower().EndsWith(".mp4"))
                {
                    strFileType = "application/octet-stream";
                }
                else if (p_strFileName.ToLower().EndsWith(".mov"))
                {
                    strFileType = "application/octet-stream";
                }
                else if (p_strFileName.ToLower().EndsWith(".flv"))
                {
                    strFileType = "application/octet-stream";
                }
                else
                {
                    throw new Exception("Invalid file format");

                }


                byte[] bVideo = File.ReadAllBytes(p_strFileName);
                string strBoundary = Guid.NewGuid().ToString();
                string strHeader = string.Format("--{0}", strBoundary);
                string strFooter = string.Format("--{0}--", strBoundary);
                HttpWebRequest rqUploadVideo = (HttpWebRequest)System.Net.WebRequest.Create(TCC_UPLOAD_VIDEO_AND_TWEET_URL);
                string strFileHeader = string.Format("Content-Disposition: file; name=\"{0}\"; filename=\"{1}\"", "video", p_strFileName);
                string strFileData = Encoding.GetEncoding("iso-8859-1").GetString(bVideo);
                var sbContents = new StringBuilder();
                rqUploadVideo.PreAuthenticate = true;
                rqUploadVideo.AllowWriteStreamBuffering = true;
                rqUploadVideo.ContentType = string.Format("multipart/form-data; boundary={0}", strBoundary);
                rqUploadVideo.Method = "POST";
                sbContents.AppendLine(strHeader);
                sbContents.AppendLine(strFileHeader);
                sbContents.AppendLine(string.Format("Content-Type: {0}", strFileType));
                sbContents.AppendLine();
                sbContents.AppendLine(strFileData);
                sbContents.AppendLine(strHeader);
                sbContents.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "username"));
                sbContents.AppendLine();
                sbContents.AppendLine(UserName);
                sbContents.AppendLine(strHeader);
                sbContents.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "password"));
                sbContents.AppendLine();
                sbContents.AppendLine(Password);
                sbContents.AppendLine(strHeader);
                sbContents.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "tweet"));
                sbContents.AppendLine();
                sbContents.AppendLine(p_strTweet);
                sbContents.AppendLine(strHeader);
                sbContents.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "show_map"));
                sbContents.AppendLine();
                if (p_bShowMap == true)
                {
                    sbContents.AppendLine("1");
                }
                else
                {
                    sbContents.AppendLine("0");
                }
                if (APIKey.Length > 0)
                {
                    sbContents.AppendLine(strHeader);
                    sbContents.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "api_key"));
                    sbContents.AppendLine();
                    sbContents.AppendLine(APIKey);
                }
                sbContents.AppendLine(strFooter);

                byte[] bFormData = Encoding.GetEncoding("iso-8859-1").GetBytes(sbContents.ToString());
                rqUploadVideo.ContentLength = bFormData.Length;
                Stream stVideoRequest = rqUploadVideo.GetRequestStream();
                stVideoRequest.Write(bFormData, 0, bFormData.Length);
                HttpWebResponse rpUploadVideo = (HttpWebResponse)rqUploadVideo.GetResponse();
                var srUploadVideo = new StreamReader(rpUploadVideo.GetResponseStream());
                string strUploadResult = srUploadVideo.ReadToEnd();
                var xUploadResult = new XmlDocument();
                xUploadResult.LoadXml(strUploadResult);
                var xnResponse = xUploadResult.SelectSingleNode("//response");
                if (xnResponse.Attributes["status"].InnerText == "ok")
                {
                    // UploadVideoAndTweet = xnResponse.SelectSingleNode("//url").InnerText
                    UploadVideoAndTweetRet.ID = xnResponse.SelectSingleNode("//id").InnerText;
                    UploadVideoAndTweetRet.URL = xnResponse.SelectSingleNode("//url").InnerText;
                    UploadVideoAndTweetRet.UserID = xnResponse.SelectSingleNode("//user_id").InnerText;
                    UploadVideoAndTweetRet.ScreenName = xnResponse.SelectSingleNode("//screen_name").InnerText;
                    UploadVideoAndTweetRet.ThumbNailURL = xnResponse.SelectSingleNode("//thumb_image_url").InnerText;
                    UploadVideoAndTweetRet.Comments_Count = Conversions.ToInteger(xnResponse.SelectSingleNode("//num_comments").InnerText);
                    UploadVideoAndTweetRet.Views_Count = Conversions.ToInteger(xnResponse.SelectSingleNode("//num_views").InnerText);
                }

                else
                {
                    // UploadVideoAndTweet = xnResponse.SelectSingleNode("//error_text").InnerText
                }
                return UploadVideoAndTweetRet;
            }



            catch (Exception)
            {

            }

            return UploadVideoAndTweetRet;
        }
        /// <summary>
        /// Uploads an audiofile to Twic.li and puts it in your my music collection
        /// </summary>
        /// <param name="p_strFileName">The file to send (must be a .mp3, .m4a, or .wav file less than 50 MB</param>
        /// <returns>An ATwiAudioFile with the response back</returns>
        /// <remarks></remarks>
        public ATwiCliAudioFile UploadAudio(string p_strFileName)
        {
            ATwiCliAudioFile UploadAudioRet = default;
            UploadAudioRet = new ATwiCliAudioFile();
            string strContentType;


            if (p_strFileName.ToLower().EndsWith(".mp3") | p_strFileName.ToLower().EndsWith(".m4a") | p_strFileName.ToLower().EndsWith(".wav"))
            {
                strContentType = "application/octet-stream";
            }
            else
            {
                throw new Exception("invalid file format");
            }

            try
            {
                byte[] bSong = File.ReadAllBytes(p_strFileName);
                string strBoundary = Guid.NewGuid().ToString();
                string strHeader = string.Format("--{0}", strBoundary);
                string strFooter = string.Format("--{0}--", strBoundary);
                HttpWebRequest rqUploadAudio = (HttpWebRequest)System.Net.WebRequest.Create(TCC_UPLOAD_AUDIO_URL);
                string strFileHeader = string.Format("Content-disposition: file; name=\"{0}\";filename=\"{1}\"", "audio", p_strFileName);
                string strFileData = Encoding.GetEncoding("iso-8859-1").GetString(bSong);
                var sbContents = new StringBuilder();
                rqUploadAudio.PreAuthenticate = true;
                rqUploadAudio.Method = "POST";
                rqUploadAudio.AllowWriteStreamBuffering = true;
                rqUploadAudio.ContentType = string.Format("multipart/form-data; boundary={0}", strBoundary);
                sbContents.AppendLine(strHeader);
                sbContents.AppendLine(strFileHeader);
                sbContents.AppendLine(string.Format("Content-Type: {0}", strContentType));
                sbContents.AppendLine();
                sbContents.AppendLine(strFileData);
                sbContents.AppendLine(strHeader);
                sbContents.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "username"));
                sbContents.AppendLine();
                sbContents.AppendLine(UserName);
                sbContents.AppendLine(strHeader);
                sbContents.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "password"));
                sbContents.AppendLine();
                sbContents.AppendLine(Password);
                if (APIKey.Length > 0)
                {
                    sbContents.AppendLine(strHeader);
                    sbContents.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "api_key"));
                    sbContents.AppendLine();
                    sbContents.AppendLine(APIKey);
                }
                sbContents.AppendLine(strFooter);
                byte[] bUploadData = Encoding.GetEncoding("iso-8859-1").GetBytes(sbContents.ToString());
                rqUploadAudio.ContentLength = bUploadData.Length;
                Stream stAudioRequest = rqUploadAudio.GetRequestStream();
                stAudioRequest.Write(bUploadData, 0, bUploadData.Length);
                HttpWebResponse rsAudioupload = (HttpWebResponse)rqUploadAudio.GetResponse();
                var srAudioData = new StreamReader(rsAudioupload.GetResponseStream());
                string strAudioData;
                strAudioData = srAudioData.ReadToEnd();
                var xUploadresult = new XmlDocument();
                xUploadresult.LoadXml(strAudioData);
                var xnResult = xUploadresult.SelectSingleNode("//response");
                if (xnResult.Attributes["status"].InnerText == "ok")
                {
                    UploadAudioRet.ID = xnResult.SelectSingleNode("//id").InnerText;
                    UploadAudioRet.ScreenName = xnResult.SelectSingleNode("//screen_name").InnerText;
                    UploadAudioRet.UserID = xnResult.SelectSingleNode("//user_id").InnerText;
                    UploadAudioRet.URL = xnResult.SelectSingleNode("//url").InnerText;
                    UploadAudioRet.Views_Count = Conversions.ToInteger(xnResult.SelectSingleNode("//num_views").InnerText);
                    UploadAudioRet.Comments_Count = Conversions.ToInteger(xnResult.SelectSingleNode("//num_comments").InnerText);
                    UploadAudioRet.InsertUnixTime(Conversions.ToDouble(xnResult.SelectSingleNode("//timestamp").InnerText));
                }
                else
                {
                    throw new Exception(xnResult.SelectSingleNode("//error_text").InnerText);
                }
                return UploadAudioRet;
            }

            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }

        }
        /// <summary>
        /// uploads an audio file and updates the user's status
        /// </summary>
        /// <param name="p_strFileName">File name to upload (must be .mp3, .m4a, or .wav and no larger than 50MB</param>
        /// <param name="p_strTweet">Text of the tweet.  Keep down to 120 characters or less as this automatically appends the url</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public ATwiCliAudioFile UploadAudioAndTweet(string p_strFileName, string p_strTweet)
        {
            ATwiCliAudioFile UploadAudioAndTweetRet = default;
            UploadAudioAndTweetRet = new ATwiCliAudioFile();
            string strContentType;


            if (p_strFileName.ToLower().EndsWith(".mp3") | p_strFileName.ToLower().EndsWith(".m4a") | p_strFileName.ToLower().EndsWith(".wav"))
            {
                strContentType = "application/octet-stream";
            }
            else
            {
                throw new Exception("invalid file format");
            }

            try
            {
                byte[] bSong = File.ReadAllBytes(p_strFileName);
                string strBoundary = Guid.NewGuid().ToString();
                string strHeader = string.Format("--{0}", strBoundary);
                string strFooter = string.Format("--{0}--", strBoundary);
                HttpWebRequest rqUploadAudio = (HttpWebRequest)System.Net.WebRequest.Create(TCC_UPLOAD_AUDIO_AND_TWEET_URL);
                string strFileHeader = string.Format("Content-disposition: file; name=\"{0}\";filename=\"{1}\"", "audio", p_strFileName);
                string strFileData = Encoding.GetEncoding("iso-8859-1").GetString(bSong);
                var sbContents = new StringBuilder();
                rqUploadAudio.PreAuthenticate = true;
                rqUploadAudio.Method = "POST";
                rqUploadAudio.AllowWriteStreamBuffering = true;
                rqUploadAudio.ContentType = string.Format("multipart/form-data; boundary={0}", strBoundary);
                sbContents.AppendLine(strHeader);
                sbContents.AppendLine(strFileHeader);
                sbContents.AppendLine(string.Format("Content-Type: {0}", strContentType));
                sbContents.AppendLine();
                sbContents.AppendLine(strFileData);
                sbContents.AppendLine(strHeader);
                sbContents.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "username"));
                sbContents.AppendLine();
                sbContents.AppendLine(UserName);
                sbContents.AppendLine(strHeader);
                sbContents.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "password"));
                sbContents.AppendLine();
                sbContents.AppendLine(Password);
                sbContents.AppendLine(strHeader);
                sbContents.AppendLine(string.Format("Content-Disposition: form_name; name=\"{0}\"", "tweet"));
                sbContents.AppendLine();
                sbContents.AppendLine(p_strTweet);
                if (APIKey.Length > 0)
                {
                    sbContents.AppendLine(strHeader);
                    sbContents.AppendLine(string.Format("Content-Disposition: form-data; name=\"{0}\"", "api_key"));
                    sbContents.AppendLine();
                    sbContents.AppendLine(APIKey);
                }
                sbContents.AppendLine(strFooter);
                byte[] bUploadData = Encoding.GetEncoding("iso-8859-1").GetBytes(sbContents.ToString());
                rqUploadAudio.ContentLength = bUploadData.Length;
                Stream stAudioRequest = rqUploadAudio.GetRequestStream();
                stAudioRequest.Write(bUploadData, 0, bUploadData.Length);
                HttpWebResponse rsAudioupload = (HttpWebResponse)rqUploadAudio.GetResponse();
                var srAudioData = new StreamReader(rsAudioupload.GetResponseStream());
                string strAudioData;
                strAudioData = srAudioData.ReadToEnd();
                var xUploadresult = new XmlDocument();
                xUploadresult.LoadXml(strAudioData);
                var xnResult = xUploadresult.SelectSingleNode("//response");
                if (xnResult.Attributes["status"].InnerText == "ok")
                {
                    UploadAudioAndTweetRet.ID = xnResult.SelectSingleNode("//id").InnerText;
                    UploadAudioAndTweetRet.ScreenName = xnResult.SelectSingleNode("//screen_name").InnerText;
                    UploadAudioAndTweetRet.UserID = xnResult.SelectSingleNode("//user_id").InnerText;
                    UploadAudioAndTweetRet.URL = xnResult.SelectSingleNode("//url").InnerText;
                    UploadAudioAndTweetRet.Views_Count = Conversions.ToInteger(xnResult.SelectSingleNode("//num_views").InnerText);
                    UploadAudioAndTweetRet.Comments_Count = Conversions.ToInteger(xnResult.SelectSingleNode("//num_comments").InnerText);
                    UploadAudioAndTweetRet.InsertUnixTime(Conversions.ToDouble(xnResult.SelectSingleNode("//timestamp").InnerText));
                }
                else
                {
                    throw new Exception(xnResult.SelectSingleNode("//error_text").InnerText);
                }
                return UploadAudioAndTweetRet;
            }


            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }
        }

        public ATwiCliFile GetContentInfo(string p_strID)
        {

            ATwiCliFile ReturnValue = default;

            HttpWebRequest rqInfo = (HttpWebRequest)System.Net.WebRequest.Create(TCC_GET_CONTENT_INFO + "?id=" + p_strID);
            rqInfo.Method = "POST";
            HttpWebResponse rsInfo = (HttpWebResponse)rqInfo.GetResponse();
            var srContent = new StreamReader(rsInfo.GetResponseStream());
            string strContent;
            strContent = srContent.ReadToEnd();
            var xContent = new XmlDocument();
            xContent.LoadXml(strContent);
            var xnResponse = xContent.SelectSingleNode("//response");
            if (xnResponse.Attributes["status"].InnerText == "ok")
            {
                var xnContent = xnResponse.SelectSingleNode("//content");
                switch (xnContent.Attributes["type"].InnerText.ToLower() ?? "")
                {
                    case "photo":
                        {
                            var Photo = new ATwiCliPhotoFile();
                            Photo.ID = xnContent.SelectSingleNode("//id").InnerText;
                            Photo.ScreenName = xnContent.SelectSingleNode("//screen_name").InnerText;
                            Photo.UserID = xnContent.SelectSingleNode("//user_id").InnerText;
                            Photo.URL = xnContent.SelectSingleNode("//url").InnerText;
                            Photo.UserTags_Count = Conversions.ToInteger(xnContent.SelectSingleNode("//num_user_tags").InnerText);
                            Photo.Comments_Count = Conversions.ToInteger(xnContent.SelectSingleNode("//num_comments").InnerText);
                            Photo.Views_Count = Conversions.ToInteger(xnContent.SelectSingleNode("//num_views").InnerText);
                            if (xnContent.SelectSingleNode("//show_map").InnerText == "1")
                            {
                                Photo.ShowMap = true;
                                Photo.Latitude = Conversions.ToDouble(xnContent.SelectSingleNode("//latitude").InnerText);
                                Photo.Longitude = Conversions.ToDouble(xnContent.SelectSingleNode("//longitude").InnerText);
                            }
                            else
                            {
                                Photo.ShowMap = false;
                            }

                            Photo.CameraMake = xnContent.SelectSingleNode("//camera_make").InnerText;
                            Photo.CameraModel = xnContent.SelectSingleNode("//camera_model").InnerText;
                            Photo.InsertUnixTime(Conversions.ToDouble(xnContent.SelectSingleNode("//timestamp").InnerText));
                            ReturnValue = Photo;
                            break;
                        }

                    case "audio":
                        {
                            var Audio = new ATwiCliAudioFile();
                            Audio.ID = xnContent.SelectSingleNode("//id").InnerText;
                            Audio.URL = xnContent.SelectSingleNode("//url").InnerText;
                            Audio.UserID = xnContent.SelectSingleNode("//user_id").InnerText;
                            Audio.ScreenName = xnContent.SelectSingleNode("//screen_name").InnerText;
                            Audio.Comments_Count = Conversions.ToInteger(xnContent.SelectSingleNode("//num_comments").InnerText);
                            Audio.Views_Count = Conversions.ToInteger(xnContent.SelectSingleNode("//num_views").InnerText);
                            Audio.InsertUnixTime(Conversions.ToDouble(xnContent.SelectSingleNode("//timestamp").InnerText));
                            ReturnValue = Audio;
                            break;
                        }

                    case "video":
                        {
                            var video = new ATwiCliVideoFile();
                            video.ID = xnContent.SelectSingleNode("//id").InnerText;
                            video.URL = xnContent.SelectSingleNode("//url").InnerText;
                            video.UserID = xnContent.SelectSingleNode("//user_id").InnerText;
                            video.ScreenName = xnContent.SelectSingleNode("//screen_name").InnerText;
                            video.Comments_Count = Conversions.ToInteger(xnContent.SelectSingleNode("//num_comments").InnerText);
                            video.Views_Count = Conversions.ToInteger(xnContent.SelectSingleNode("//num_views").InnerText);
                            video.InsertUnixTime(Conversions.ToDouble(xnContent.SelectSingleNode("//timestamp").InnerText));
                            ReturnValue = video;
                            break;
                        }
                }
            }
            else
            {
                throw new Exception(xnResponse.SelectSingleNode("//error_text").InnerText);
            }

            return ReturnValue;

        }
        public List<ATwiCliPhotoFile> GetUsersPhotos(long p_ID)
        {
            List<ATwiCliPhotoFile> GetUsersPhotosRet = default;
            GetUsersPhotosRet = new List<ATwiCliPhotoFile>();
            string strURL = TCC_GET_USERS_CONTENT_URL + "?userid=" + p_ID + "&content_type=photos";

            try
            {
                HttpWebRequest rqUsersPhotos = (HttpWebRequest)System.Net.WebRequest.Create(strURL);
                rqUsersPhotos.Method = "GET";
                HttpWebResponse rsUsersPhotos = (HttpWebResponse)rqUsersPhotos.GetResponse();
                var srUsersPhotos = new StreamReader(rsUsersPhotos.GetResponseStream());
                string strUsersPhotos;
                strUsersPhotos = srUsersPhotos.ReadToEnd();
                var xdPhotos = new XmlDocument();
                xdPhotos.LoadXml(strUsersPhotos);
                foreach (XmlNode xPhoto in xdPhotos.SelectNodes("//content"))
                {
                    var Photo = new ATwiCliPhotoFile();
                    Photo.ID = xPhoto.SelectSingleNode("id").InnerText;
                    Photo.UserID = xPhoto.SelectSingleNode("user_id").InnerText;
                    Photo.ScreenName = xPhoto.SelectSingleNode("screen_name").InnerText;
                    Photo.URL = xPhoto.SelectSingleNode("url").InnerText;
                    Photo.Comments_Count = Conversions.ToInteger(xPhoto.SelectSingleNode("num_comments").InnerText);
                    Photo.UserTags_Count = Conversions.ToInteger(xPhoto.SelectSingleNode("num_user_tags").InnerText);
                    Photo.Views_Count = Conversions.ToInteger(xPhoto.SelectSingleNode("num_views").InnerText);
                    if (xPhoto.SelectSingleNode("show_map").InnerText == "1")
                    {
                        Photo.ShowMap = true;
                        Photo.Latitude = Conversions.ToDouble(xPhoto.SelectSingleNode("latitude").InnerText);
                        Photo.Longitude = Conversions.ToDouble(xPhoto.SelectSingleNode("longitude").InnerText);
                    }
                    else
                    {
                        Photo.ShowMap = false;
                    }
                    Photo.CameraMake = xPhoto.SelectSingleNode("camera_make").InnerText;
                    Photo.CameraModel = xPhoto.SelectSingleNode("camera_model").InnerText;
                    Photo.InsertUnixTime(Conversions.ToDouble(xPhoto.SelectSingleNode("timestamp").InnerText));
                    GetUsersPhotosRet.Add(Photo);

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return GetUsersPhotosRet;

        }
        public List<ATwiCliPhotoFile> GetUsersPhotos(string p_strUserName)
        {
            List<ATwiCliPhotoFile> GetUsersPhotosRet = default;
            GetUsersPhotosRet = new List<ATwiCliPhotoFile>();
            string strURL = TCC_GET_USERS_CONTENT_URL + "?username=" + p_strUserName + "&content_type=photos";

            try
            {
                HttpWebRequest rqUsersPhotos = (HttpWebRequest)System.Net.WebRequest.Create(strURL);
                rqUsersPhotos.Method = "GET";
                HttpWebResponse rsUsersPhotos = (HttpWebResponse)rqUsersPhotos.GetResponse();
                var srUsersPhotos = new StreamReader(rsUsersPhotos.GetResponseStream());
                string strUsersPhotos;
                strUsersPhotos = srUsersPhotos.ReadToEnd();
                var xdPhotos = new XmlDocument();
                xdPhotos.LoadXml(strUsersPhotos);
                foreach (XmlNode xPhoto in xdPhotos.SelectNodes("//content"))
                {
                    var Photo = new ATwiCliPhotoFile();
                    Photo.ID = xPhoto.SelectSingleNode("id").InnerText;
                    Photo.UserID = xPhoto.SelectSingleNode("user_id").InnerText;
                    Photo.ScreenName = xPhoto.SelectSingleNode("screen_name").InnerText;
                    Photo.URL = xPhoto.SelectSingleNode("url").InnerText;
                    Photo.Comments_Count = Conversions.ToInteger(xPhoto.SelectSingleNode("num_comments").InnerText);
                    Photo.UserTags_Count = Conversions.ToInteger(xPhoto.SelectSingleNode("num_user_tags").InnerText);
                    Photo.Views_Count = Conversions.ToInteger(xPhoto.SelectSingleNode("num_views").InnerText);
                    if (xPhoto.SelectSingleNode("show_map").InnerText == "1")
                    {
                        Photo.ShowMap = true;
                        Photo.Latitude = Conversions.ToDouble(xPhoto.SelectSingleNode("latitude").InnerText);
                        Photo.Longitude = Conversions.ToDouble(xPhoto.SelectSingleNode("longitude").InnerText);
                    }
                    else
                    {
                        Photo.ShowMap = false;
                    }
                    Photo.CameraMake = xPhoto.SelectSingleNode("camera_make").InnerText;
                    Photo.CameraModel = xPhoto.SelectSingleNode("camera_model").InnerText;
                    Photo.InsertUnixTime(Conversions.ToDouble(xPhoto.SelectSingleNode("timestamp").InnerText));
                    GetUsersPhotosRet.Add(Photo);

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return GetUsersPhotosRet;
        }

        public List<ATwiCliAudioFile> GetUsersAudios(long p_ID)
        {
            List<ATwiCliAudioFile> GetUsersAudiosRet = default;
            GetUsersAudiosRet = new List<ATwiCliAudioFile>();
            string strURL = TCC_GET_USERS_CONTENT_URL + "?userid=" + p_ID + "&content_type=audio";
            try
            {
                HttpWebRequest rqUsersAudios = (HttpWebRequest)System.Net.WebRequest.Create(strURL);
                rqUsersAudios.Method = "GET";
                HttpWebResponse rsUsersAudios = (HttpWebResponse)rqUsersAudios.GetResponse();
                var srUsersAudios = new StreamReader(rsUsersAudios.GetResponseStream());
                string strUsersAudios = srUsersAudios.ReadToEnd();
                var xdAudios = new XmlDocument();
                xdAudios.LoadXml(strUsersAudios);
                foreach (XmlNode xAudio in xdAudios.SelectNodes("//content"))
                {
                    var Audio = new ATwiCliAudioFile();
                    Audio.ID = xAudio.SelectSingleNode("id").InnerText;
                    Audio.URL = xAudio.SelectSingleNode("url").InnerText;
                    Audio.UserID = xAudio.SelectSingleNode("user_id").InnerText;
                    Audio.ScreenName = xAudio.SelectSingleNode("screen_name").InnerText;
                    Audio.Comments_Count = Conversions.ToInteger(xAudio.SelectSingleNode("num_comments").InnerText);
                    Audio.Views_Count = Conversions.ToInteger(xAudio.SelectSingleNode("num_views").InnerText);
                    Audio.InsertUnixTime(Conversions.ToDouble(xAudio.SelectSingleNode("timestamp").InnerText));
                    GetUsersAudiosRet.Add(Audio);
                }
            }

            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return GetUsersAudiosRet;

        }

        public List<ATwiCliAudioFile> GetUsersAudios(string p_strUserName)
        {
            List<ATwiCliAudioFile> GetUsersAudiosRet = default;
            GetUsersAudiosRet = new List<ATwiCliAudioFile>();
            string strURL = TCC_GET_USERS_CONTENT_URL + "?username=" + p_strUserName + "&content_type=audio";
            try
            {
                HttpWebRequest rqUsersAudios = (HttpWebRequest)System.Net.WebRequest.Create(strURL);
                rqUsersAudios.Method = "GET";
                HttpWebResponse rsUsersAudios = (HttpWebResponse)rqUsersAudios.GetResponse();
                var srUsersAudios = new StreamReader(rsUsersAudios.GetResponseStream());
                string strUsersAudios = srUsersAudios.ReadToEnd();
                var xdAudios = new XmlDocument();
                xdAudios.LoadXml(strUsersAudios);
                foreach (XmlNode xAudio in xdAudios.SelectNodes("//content"))
                {
                    var Audio = new ATwiCliAudioFile();
                    Audio.ID = xAudio.SelectSingleNode("id").InnerText;
                    Audio.URL = xAudio.SelectSingleNode("url").InnerText;
                    Audio.UserID = xAudio.SelectSingleNode("user_id").InnerText;
                    Audio.ScreenName = xAudio.SelectSingleNode("screen_name").InnerText;
                    Audio.Comments_Count = Conversions.ToInteger(xAudio.SelectSingleNode("num_comments").InnerText);
                    Audio.Views_Count = Conversions.ToInteger(xAudio.SelectSingleNode("num_views").InnerText);
                    Audio.InsertUnixTime(Conversions.ToDouble(xAudio.SelectSingleNode("timestamp").InnerText));
                    GetUsersAudiosRet.Add(Audio);
                }
            }

            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return GetUsersAudiosRet;

        }

        public List<ATwiCliVideoFile> GetUsersVideos(long p_ID)
        {
            List<ATwiCliVideoFile> GetUsersVideosRet = default;
            GetUsersVideosRet = new List<ATwiCliVideoFile>();
            string strURL = TCC_GET_USERS_CONTENT_URL + "?userid=" + p_ID + "&content_type=videos";
            try
            {
                HttpWebRequest rqUsersVideos = (HttpWebRequest)System.Net.WebRequest.Create(strURL);
                rqUsersVideos.Method = "GET";
                HttpWebResponse rsUsersVideos = (HttpWebResponse)rqUsersVideos.GetResponse();
                var srUsersVideos = new StreamReader(rsUsersVideos.GetResponseStream());
                string strUsersVideos = srUsersVideos.ReadToEnd();
                var xdVideos = new XmlDocument();
                xdVideos.LoadXml(strUsersVideos);
                foreach (XmlNode xVideo in xdVideos.SelectNodes("//content"))
                {
                    var video = new ATwiCliVideoFile();
                    video.ID = xVideo.SelectSingleNode("id").InnerText;
                    video.URL = xVideo.SelectSingleNode("url").InnerText;
                    video.UserID = xVideo.SelectSingleNode("user_id").InnerText;
                    video.ScreenName = xdVideos.SelectSingleNode("screen_name").InnerText;
                    video.Comments_Count = Conversions.ToInteger(xVideo.SelectSingleNode("num_comments").InnerText);
                    video.Views_Count = Conversions.ToInteger(xVideo.SelectSingleNode("num_views").InnerText);
                    video.InsertUnixTime(Conversions.ToDouble(xVideo.SelectSingleNode("timestamp").InnerText));
                    GetUsersVideosRet.Add(video);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return GetUsersVideosRet;

        }

        public List<ATwiCliVideoFile> GetUsersVideos(string p_strUserName)
        {
            List<ATwiCliVideoFile> GetUsersVideosRet = default;
            GetUsersVideosRet = new List<ATwiCliVideoFile>();
            string strURL = TCC_GET_USERS_CONTENT_URL + "?username=" + p_strUserName + "&content_type=videos";
            try
            {
                HttpWebRequest rqUsersVideos = (HttpWebRequest)System.Net.WebRequest.Create(strURL);
                rqUsersVideos.Method = "GET";
                HttpWebResponse rsUsersVideos = (HttpWebResponse)rqUsersVideos.GetResponse();
                var srUsersVideos = new StreamReader(rsUsersVideos.GetResponseStream());
                string strUsersVideos = srUsersVideos.ReadToEnd();
                var xdVideos = new XmlDocument();
                xdVideos.LoadXml(strUsersVideos);
                foreach (XmlNode xVideo in xdVideos.SelectNodes("//content"))
                {
                    var video = new ATwiCliVideoFile();
                    video.ID = xVideo.SelectSingleNode("id").InnerText;
                    video.URL = xVideo.SelectSingleNode("url").InnerText;
                    video.UserID = xVideo.SelectSingleNode("user_id").InnerText;
                    video.ScreenName = xdVideos.SelectSingleNode("screen_name").InnerText;
                    video.Comments_Count = Conversions.ToInteger(xVideo.SelectSingleNode("num_comments").InnerText);
                    video.Views_Count = Conversions.ToInteger(xVideo.SelectSingleNode("num_views").InnerText);
                    video.InsertUnixTime(Conversions.ToDouble(xVideo.SelectSingleNode("timestamp").InnerText));
                    GetUsersVideosRet.Add(video);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return GetUsersVideosRet;
        }
    }
}
