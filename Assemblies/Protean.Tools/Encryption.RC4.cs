using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Protean.Tools
{
    // Move or copy your existing RC4 implementation here.
    // Preserve all comments and behavior so legacy tokens still work.
    public static class RC4
    {       
        /// <summary>
        ///         ''' Returns an encrypted string based on the provided message and passkey
        ///         ''' </summary>
        ///         ''' <param name="message"></param>
        ///         ''' <param name="key"></param>
        ///         ''' <returns></returns>
        ///         ''' <remarks></remarks>
        public static string Encrypt(string message, string key)
        {
            if (message == null || message.Length == 0)
                throw new ArgumentNullException("message");

            if (key == null || key.Length == 0)
                throw new ArgumentNullException("key");
            try
            {

                string returnValue = string.Empty;
                StringBuilder sb = new StringBuilder();

                returnValue = EnDeCrypt(message, key);

                returnValue = StringToHex(returnValue);
                returnValue = returnValue.Replace("-", "");
                return returnValue;
            }
            catch (Exception)
            {
                return "";
            }
        }

        /// <summary>
        ///         ''' Returns a decrypted string based on the provided message and passkey
        ///         ''' </summary>
        ///         ''' <param name="message"></param>
        ///         ''' <param name="key"></param>
        ///         ''' <returns></returns>
        ///         ''' <remarks></remarks>
        public static string Decrypt(string message, string key)
        {
            if (message == null || message.Length == 0)
                throw new ArgumentNullException("message");

            if (key == null || key.Length == 0)
                throw new ArgumentNullException("key");

            try
            {
                string returnValue = string.Empty;

                returnValue = HexToString(message);
                returnValue = EnDeCrypt(returnValue, key);

                return returnValue;
            }
            catch (Exception)
            {
                return "";
            }
        }

        /// <summary>
        ///         ''' RC4 encryption method
        ///         ''' </summary>
        ///         ''' <param name="message"></param>
        ///         ''' <param name="password"></param>
        ///         ''' <returns></returns>
        ///         ''' <remarks></remarks>
        private static string EnDeCrypt(string message, string password)
        {
            int i = 0;
            int j = 0;
            StringBuilder cipher = new StringBuilder();
            string returnCipher = string.Empty;

            int[] sbox = new int[257];
            int[] key = new int[257];

            int intLength = password.Length;

            int a = 0;
            while (a <= 255)
            {
                //char ctmp = password[(a % intLength)];
                //key[a] = (int)ctmp;

                char ctmp = (password.Substring((a % intLength), 1).ToCharArray()[0]);
                // key[a] = (int)ctmp;
                key[a] = Asc(ctmp);



                sbox[a] = a;
                System.Math.Max(System.Threading.Interlocked.Increment(ref a), a - 1);
            }

            int x = 0;

            int b = 0;
            while (b <= 255)
            {
                x = (x + sbox[b] + key[b]) % 256;
                int tempSwap = sbox[b];
                sbox[b] = sbox[x];
                sbox[x] = tempSwap;
                System.Math.Max(System.Threading.Interlocked.Increment(ref b), b - 1);
            }

            a = 1;

            while (a <= message.Length)
            {
                int itmp = 0;

                i = (i + 1) % 256;
                j = (j + sbox[i]) % 256;
                itmp = sbox[i];
                sbox[i] = sbox[j];
                sbox[j] = itmp;

                int k = sbox[(sbox[i] + sbox[j]) % 256];

                char ctmp = message.Substring(a - 1, 1).ToCharArray()[0];

                itmp = Asc(ctmp);

                int cipherby = itmp ^ k;

                cipher.Append(Chr(cipherby));


                //char ctmp = message[a - 1];

                //itmp = (int)ctmp;

                //int cipherby = itmp ^ k;

                // cipher.Append((char)cipherby);

                System.Math.Max(System.Threading.Interlocked.Increment(ref a), a - 1);
            }

            returnCipher = cipher.ToString();
            cipher.Length = 0;

            return returnCipher;
        }

        /// <summary>
        ///         ''' Turns the provided string value into a hex value (for encryption)
        ///         ''' </summary>
        ///         ''' <param name="message"></param>
        ///         ''' <returns></returns>
        ///         ''' <remarks></remarks>
        private static string StringToHex(string message)
        {
            //long index;
            long maxIndex;
            StringBuilder hexSb = new StringBuilder();
            string hexOut = string.Empty;

            maxIndex = message.Length;

            //for (index = 1; index <= maxIndex; index++)
            //    //hexSb.Append(Strings.Right("0" + Conversion.Hex(Strings.Asc(Strings.Mid(message, System.Convert.ToInt32(index), 1))), 2));
            //hexSb.Append(Strings.Right("0" + String.Format("0x{0:X}", (Strings.Asc(Strings.Mid(message, System.Convert.ToInt32(index), 1)))), 2));

            byte[] ba = Encoding.Default.GetBytes(message);
            hexOut = BitConverter.ToString(ba);

            // hexOut = hexSb.ToString();
            hexSb.Length = 0;

            return hexOut;
        }

        /// <summary>
        ///         ''' Turns the provided hex value into a string value (for decryption)
        ///         ''' </summary>
        ///         ''' <param name="hex"></param>
        ///         ''' <returns></returns>
        ///         ''' <remarks></remarks>
        private static string HexToString(string hex)
        {
            try
            {
                int index;
                long maxIndex;
                StringBuilder sb = new StringBuilder();
                string returnString = string.Empty;

                maxIndex = hex.Length / 2;

                for (index = 0; index < maxIndex; index++)
                {
                    sb.Append(Chr(Convert.ToInt32(hex.Substring(index * 2, 2), 16)));
                }


                //byte[] raw = new byte[hex.Length / 2];
                //for (int i = 0; i < raw.Length; i++)
                //{
                //    raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
                //}
                //returnString = Encoding.ASCII.GetString(raw);

                returnString = sb.ToString();
                sb.Length = 0;

                return returnString;

            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        /// <summary>
        /// Returns the character associated with the specified character code.
        /// </summary>
        /// 
        /// <returns>
        /// Returns the character associated with the specified character code.
        /// </returns>
        /// <param name="CharCode">Required. An Integer expression representing the <paramref name="code point"/>, or character code, for the character.</param><exception cref="T:System.ArgumentException"><paramref name="CharCode"/> &lt; 0 or &gt; 255 for Chr.</exception><filterpriority>1</filterpriority>


        public static char Chr(int CharCode)
        {
            if (CharCode < (int)short.MinValue || CharCode > (int)ushort.MaxValue)
                throw new ArgumentNullException("message");
            if (CharCode >= 0 && CharCode <= (int)sbyte.MaxValue)
                return Convert.ToChar(CharCode);
            try
            {
                Encoding encoding = Encoding.GetEncoding(GetLocaleCodePage());
                if (encoding.IsSingleByte && (CharCode < 0 || CharCode > (int)byte.MaxValue))
                    throw new ArgumentNullException("message");
                char[] chars = new char[2];
                byte[] bytes = new byte[2];
                Decoder decoder = encoding.GetDecoder();
                if (CharCode >= 0 && CharCode <= (int)byte.MaxValue)
                {
                    bytes[0] = checked((byte)(CharCode & (int)byte.MaxValue));
                    decoder.GetChars(bytes, 0, 1, chars, 0);
                }
                else
                {
                    bytes[0] = checked((byte)((CharCode & 65280) >> 8));
                    bytes[1] = checked((byte)(CharCode & (int)byte.MaxValue));
                    decoder.GetChars(bytes, 0, 2, chars, 0);
                }
                return chars[0];
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// Returns an Integer value representing the character code corresponding to a character.
        /// </summary>
        /// 
        /// <returns>
        /// Returns an Integer value representing the character code corresponding to a character.
        /// </returns>
        /// <param name="String">Required. Any valid Char or String expression. If <paramref name="String"/> is a String expression, only the first character of the string is used for input. If <paramref name="String"/> is Nothing or contains no characters, an <see cref="T:System.ArgumentException"/> error occurs.</param><filterpriority>1</filterpriority>
        internal static Encoding GetFileIOEncoding()
        {
            return Encoding.Default;
        }

        internal static int GetLocaleCodePage()
        {
            return Thread.CurrentThread.CurrentCulture.TextInfo.ANSICodePage;
        }


        public static int Asc(char String)
        {
            int num1 = Convert.ToInt32(String);
            if (num1 < 128)
                return num1;
            try
            {
                Encoding fileIoEncoding = GetFileIOEncoding();
                char[] chars = new char[1]
                {
      String
                };
                if (fileIoEncoding.IsSingleByte)
                {
                    byte[] bytes = new byte[1];
                    fileIoEncoding.GetBytes(chars, 0, 1, bytes, 0);
                    return (int)bytes[0];
                }
                byte[] bytes1 = new byte[2];
                if (fileIoEncoding.GetBytes(chars, 0, 1, bytes1, 0) == 1)
                    return (int)bytes1[0];
                if (BitConverter.IsLittleEndian)
                {
                    byte num2 = bytes1[0];
                    bytes1[0] = bytes1[1];
                    bytes1[1] = num2;
                }
                return (int)BitConverter.ToInt16(bytes1, 0);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}
