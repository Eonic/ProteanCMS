using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Configuration;

namespace Protean
{


    class Encryption
    {

        public string GenerateMD5Hash(string SourceText)
        {
            // This generates a PHP compatible MD5 Hex string for the source value.

            var md5 = MD5.Create();
            byte[] dataMd5 = md5.ComputeHash(Encoding.Default.GetBytes(SourceText));
            var sb = new StringBuilder();
            int i = 0;
            while (i < dataMd5.Length)
            {
                sb.AppendFormat("{0:x2}", dataMd5[i]);
                Math.Min(System.Threading.Interlocked.Increment(ref i), i - 1);
            }
            return sb.ToString();
        }

        internal class Utils
        {

            /// <summary>
        /// converts an array of bytes to a string Hex representation
        /// </summary>
            internal static string ToHex(byte[] ba)
            {
                if (ba is null || ba.Length == 0)
                {
                    return "";
                }
                const string HexFormat = "{0:X2}";
                var sb = new StringBuilder();
                foreach (byte b in ba)
                    sb.Append(string.Format(HexFormat, b));
                return sb.ToString();
            }

            /// <summary>
        /// converts from a string Hex representation to an array of bytes
        /// </summary>
            internal static byte[] FromHex(string hexEncoded)
            {
                if (hexEncoded is null || hexEncoded.Length == 0)
                {
                    return null;
                }
                try
                {
                    int l = Convert.ToInt32(hexEncoded.Length / 2d);
                    var b = new byte[l];
                    for (int i = 0, loopTo = l - 1; i <= loopTo; i++)
                        b[i] = Convert.ToByte(hexEncoded.Substring(i * 2, 2), 16);
                    return b;
                }
                catch (Exception ex)
                {
                    throw new FormatException("The provided string does not appear to be Hex encoded:" + Environment.NewLine + hexEncoded + Environment.NewLine, ex);
                }
            }

            /// <summary>
        /// converts from a string Base64 representation to an array of bytes
        /// </summary>
            internal static byte[] FromBase64(string base64Encoded)
            {
                if (base64Encoded is null || base64Encoded.Length == 0)
                {
                    return null;
                }
                try
                {
                    return Convert.FromBase64String(base64Encoded);
                }
                catch (FormatException ex)
                {
                    throw new FormatException("The provided string does not appear to be Base64 encoded:" + Environment.NewLine + base64Encoded + Environment.NewLine, ex);
                }
            }

            /// <summary>
        /// converts from an array of bytes to a string Base64 representation
        /// </summary>
            internal static string ToBase64(byte[] b)
            {
                if (b is null || b.Length == 0)
                {
                    return "";
                }
                return Convert.ToBase64String(b);
            }

            /// <summary>
        /// retrieve an element from an Xml string
        /// </summary>
            internal static string GetXmlElement(string Xml, string element)
            {
                Match m;
                m = Regex.Match(Xml, "<" + element + ">(?<Element>[^>]*)</" + element + ">", RegexOptions.IgnoreCase);
                if (m is null)
                {
                    throw new Exception("Could not find <" + element + "></" + element + "> in provided Public Key Xml.");
                }
                return m.Groups["Element"].ToString();
            }

            /// <summary>
        /// Returns the specified string value from the application .config file
        /// </summary>
            internal static string GetConfigString(string key, bool isRequired = true)
            {
                string strReturn = "";
                string s = WebConfigurationManager.AppSettings.Get(key);
                if (string.IsNullOrEmpty(s))
                {
                    if (isRequired)
                    {
                    }
                    // Throw New ConfigurationException("key <" & key & "> is missing from .config file")
                    else
                    {
                        strReturn = "";
                    }
                }
                else
                {
                    strReturn = s;
                }
                return strReturn;
            }

            internal static string WriteConfigKey(string key, string value)
            {
                string s = "<add key=\"{0}\" value=\"{1}\" />" + Environment.NewLine;
                return string.Format(s, key, value);
            }

            internal static string WriteXmlElement(string element, string value)
            {
                string s = "<{0}>{1}</{0}>" + Environment.NewLine;
                return string.Format(s, element, value);
            }

            internal static string WriteXmlNode(string element, bool isClosing = false)
            {
                string s;
                if (isClosing)
                {
                    s = "</{0}>" + Environment.NewLine;
                }
                else
                {
                    s = "<{0}>" + Environment.NewLine;
                }
                return string.Format(s, element);
            }

        }

    }
}