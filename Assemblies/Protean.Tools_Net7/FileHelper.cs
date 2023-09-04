using System;
using System.IO;

namespace Protean.Tools
{

    public class FileHelper
    {

        public static string ReplaceIllegalChars(string filePath, string replacementChar = "-")
        {

            try
            {

                // Replace illegal characters 
                foreach (char illegalChar in Path.GetInvalidFileNameChars())
                    filePath = filePath.Replace(illegalChar.ToString(), replacementChar);

                foreach (char illegalChar in Path.GetInvalidPathChars())
                    filePath = filePath.Replace(illegalChar.ToString(), replacementChar);

                return filePath;
            }

            catch (Exception ex)
            {
                return "";
            }

        }


        /// <summary>
        /// Returns a MIME type for a given extension
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string GetMIMEType(string extension)
        {

            string mimeType;

            switch (extension.ToUpper() ?? "")
            {
                // Common documents
                case "TXT":
                case "TEXT":
                case "JS":
                case "VBS":
                case "ASP":
                case "CGI":
                case "PL":
                case "NFO":
                case "ME":
                case "DTD":
                    {
                        mimeType = "text/plain";
                        break;
                    }
                case "HTM":
                case "HTML":
                case "HTA":
                case "HTX":
                case "MHT":
                    {
                        mimeType = "text/html";
                        break;
                    }
                case "CSV":
                    {
                        mimeType = "text/comma-separated-values";
                        break;
                    }
                case var @case when @case == "JS":
                    {
                        mimeType = "text/javascript";
                        break;
                    }
                case "CSS":
                    {
                        mimeType = "text/css";
                        break;
                    }
                case "PDF":
                    {
                        mimeType = "application/pdf";
                        break;
                    }
                case "RTF":
                    {
                        mimeType = "application/rtf";
                        break;
                    }
                case "XML":
                case "XSL":
                case "XSLT":
                    {
                        mimeType = "text/xml";
                        break;
                    }
                case "WPD":
                    {
                        mimeType = "application/wordperfect";
                        break;
                    }
                case "WRI":
                    {
                        mimeType = "application/mswrite";
                        break;
                    }
                case "XLS":
                case "XLS3":
                case "XLS4":
                case "XLS5":
                case "XLW":
                    {
                        mimeType = "application/msexcel";
                        break;
                    }
                case "DOC":
                    {
                        mimeType = "application/msword";
                        break;
                    }
                case "PPT":
                case "PPS":
                    {
                        mimeType = "application/mspowerpoint";
                        break;
                    }

                // WAP/WML files 
                case "WML":
                    {
                        mimeType = "text/vnd.wap.wml";
                        break;
                    }
                case "WMLS":
                    {
                        mimeType = "text/vnd.wap.wmlscript";
                        break;
                    }
                case "WBMP":
                    {
                        mimeType = "image/vnd.wap.wbmp";
                        break;
                    }
                case "WMLC":
                    {
                        mimeType = "application/vnd.wap.wmlc";
                        break;
                    }
                case "WMLSC":
                    {
                        mimeType = "application/vnd.wap.wmlscriptc";
                        break;
                    }

                // Images
                case "GIF":
                    {
                        mimeType = "image/gif";
                        break;
                    }
                case "JPG":
                case "JPE":
                case "JPEG":
                    {
                        mimeType = "image/jpeg";
                        break;
                    }
                case "PNG":
                    {
                        mimeType = "image/png";
                        break;
                    }
                case "BMP":
                    {
                        mimeType = "image/bmp";
                        break;
                    }
                case "TIF":
                case "TIFF":
                    {
                        mimeType = "image/tiff";
                        break;
                    }
                case "AI":
                case "EPS":
                case "PS":
                    {
                        mimeType = "application/postscript";
                        break;
                    }

                // Sound files
                case "AU":
                case "SND":
                    {
                        mimeType = "audio/basic";
                        break;
                    }
                case "WAV":
                    {
                        mimeType = "audio/wav";
                        break;
                    }
                case "RA":
                case "RM":
                case "RAM":
                    {
                        mimeType = "audio/x-pn-realaudio";
                        break;
                    }
                case "MID":
                case "MIDI":
                    {
                        mimeType = "audio/x-midi";
                        break;
                    }
                case "MP3":
                    {
                        mimeType = "audio/mp3";
                        break;
                    }
                case "M3U":
                    {
                        mimeType = "audio/m3u";
                        break;
                    }

                // Video/Multimedia files
                case "ASF":
                    {
                        mimeType = "video/x-ms-asf";
                        break;
                    }
                case "AVI":
                    {
                        mimeType = "video/avi";
                        break;
                    }
                case "MPG":
                case "MPEG":
                    {
                        mimeType = "video/mpeg";
                        break;
                    }
                case "QT":
                case "MOV":
                case "QTVR":
                    {
                        mimeType = "video/quicktime";
                        break;
                    }
                case "SWA":
                    {
                        mimeType = "application/x-director";
                        break;
                    }
                case "SWF":
                    {
                        mimeType = "application/x-shockwave-flash";
                        break;
                    }
                // Compressed/archives
                case "ZIP":
                    {
                        mimeType = "application/x-zip-compressed";
                        break;
                    }
                case "GZ":
                    {
                        mimeType = "application/x-gzip";
                        break;
                    }
                case "RAR":
                    {
                        mimeType = "application/x-rar-compressed";
                        break;
                    }

                // Miscellaneous
                case "COM":
                case "EXE":
                case "DLL":
                case "OCX":
                    {

                        // Unknown (send as binary stream)
                        mimeType = "application/octet-stream";
                        break;
                    }

                default:
                    {
                        mimeType = "application/octet-stream";
                        break;
                    }
            }

            return mimeType;

        }



    }
}
