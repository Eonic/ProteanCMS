using System;
using System.IO;
using System.Security;
using System.Text;
using Microsoft.VisualBasic;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Web.Configuration;

namespace Protean.Tools.Csharp
{
    public static class Encryption
    {
        private static byte[] m_Key = new byte[8];
        private static byte[] m_IV = new byte[8];
        public static event OnErrorEventHandler OnError;

        public delegate void OnErrorEventHandler(object sender, Protean.Tools.Errors.ErrorEventArgs e);

        public static string HashString(string OriginalString, Hash.Provider Provider, bool RemoveLineBreaks)
        {
            string cResult;

            try
            {
                if (OriginalString == "")
                    return "";
                Encryption.Hash oHash = new Encryption.Hash(Provider);
                var oEnc = new Encryption.EncData(OriginalString);
                oEnc = oHash.Calculate(oEnc);
                oHash.Dispose();
                // TS changed this to Hex from text
                switch (Provider)
                {
                    case Hash.Provider.Md5:
                        {
                            cResult = oEnc.Hex.ToLower();
                            break;
                        }

                    case Hash.Provider.Sha1:
                        {
                            cResult = oEnc.Hex.ToLower();
                            break;
                        }

                    case Hash.Provider.Sha256:
                        {
                            cResult = oEnc.Hex.ToLower();
                            break;
                        }

                    case Hash.Provider.Sha384:
                        {
                            cResult = oEnc.Hex.ToLower();
                            break;
                        }

                    case Hash.Provider.Sha512:
                        {
                            cResult = oEnc.Hex.ToLower();
                            break;
                        }

                    default:
                        {
                            cResult = oEnc.Text;
                            if (RemoveLineBreaks)
                            {
                                cResult = oEnc.Text.Replace(Constants.vbNewLine, ""); //Replace(oEnc.Text, Constants.vbNewLine, "");
                                cResult = oEnc.Text.Replace(Strings.Chr(13).ToString(), "");
                            }

                            break;
                        }
                }

                return cResult;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs("Encryption", "HashString", ex, ""));
                return OriginalString;
            }
        }

        public static string HashString(string OriginalString, string Provider, bool RemoveLineBreaks)
        {
            string cResult;

            try
            {
                switch (Strings.LCase(Provider))
                {
                    case "":
                    case "plain":
                        {
                            cResult = OriginalString;
                            break;
                        }

                    case "md5":
                    case "md5salt":
                        {
                            cResult = MD5Hash(OriginalString);
                            break;
                        }

                    default:
                        {
                            Encryption.Hash.Provider nProvider = Hash.Provider.Md5;

                            try
                            {
                                nProvider = (Hash.Provider)System.Enum.Parse(typeof(Hash.Provider), Provider, true);
                            }
                            catch (Exception ex)
                            {
                                cResult = OriginalString;
                            }

                            cResult = Encryption.HashString(OriginalString, nProvider, RemoveLineBreaks);
                            break;
                        }
                }


                return cResult;
            }

            catch (Exception ex)
            {
                OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs("Encryption", "HashString", ex, ""));
                return OriginalString;
            }
        }

        private static string MD5Hash(string cPassword)
        {
            try
            {
                StringBuilder strBuilderResult = new StringBuilder();
                int intCount;
                byte[] arrBytes;
                byte[] arrBytesHashed;
                HashAlgorithm hash = new MD5CryptoServiceProvider();

                // Convert text to byte array
                arrBytes = Encoding.UTF8.GetBytes(cPassword);

                // hash array
                arrBytesHashed = hash.ComputeHash(arrBytes);

                // convert hashed bytes into to hex value
                for (intCount = 0; intCount <= arrBytesHashed.Length - 1; intCount++)
                {
                    string newVal = Conversion.Hex(arrBytesHashed[intCount]);
                    switch (newVal.Length)
                    {
                        case 0:
                            {
                                newVal = "00";
                                break;
                            }

                        case 1:
                            {
                                newVal = "0" + newVal;
                                break;
                            }
                    }
                    strBuilderResult.Append(newVal);
                }

                return Strings.LCase(strBuilderResult.ToString());
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs("Encryption", "MD5Hash", ex, ""));
                return cPassword;
            }
        }

        public static string generateSalt()
        {
            string cDefaultSalt = "d2";
            try
            {
                StringBuilder strResult = new StringBuilder();
                int nCount;
                int nBaseTen;
                VBMath.Randomize();

                for (nCount = 0; nCount <= 1; nCount++)
                {
                    nBaseTen = System.Convert.ToInt32(Math.Ceiling(VBMath.Rnd() * 15)); // random number between 0 and 15
                    strResult.Append(Conversion.Hex(nBaseTen));
                }

                return Strings.LCase(strResult.ToString());
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs("Encryption", "generateSalt", ex, ""));
                return cDefaultSalt;
            }
        }

        public static string EncryptData(string strKey, string strData)
        {
            string strResult; // //Return Result

            // //1. String Length cannot exceed 90Kb. Otherwise, buffer will overflow. See point 3 for reasons
            if ((strData.Length > 92160))
            {
                strResult = "Error. Data String too large. Keep within 90Kb.";
                return strResult;
            }

            // //2. Generate the Keys
            if (!InitKey(strKey))
            {
                strResult = "Error. Fail to generate key for encryption";
                return strResult;
            }

            // //3. Prepare the String
            // //	The first 5 character of the string is formatted to store the actual length of the data.
            // //	This is the simplest way to remember to original length of the data, without resorting to complicated computations.
            // //	If anyone figure a good way to 'remember' the original length to facilite the decryption without having to use additional function parameters, pls let me know.
            strData = Strings.Format(strData.Length, "00000") + strData;


            // //4. Encrypt the Data
            byte[] rbData = new byte[strData.Length + 1];
            ASCIIEncoding aEnc = new ASCIIEncoding();
            aEnc.GetBytes(strData, 0, strData.Length, rbData, 0);

            DESCryptoServiceProvider descsp = new DESCryptoServiceProvider();

            ICryptoTransform desEncrypt = descsp.CreateEncryptor(m_Key, m_IV);


            // //5. Perpare the streams:
            // //	mOut is the output stream. 
            // //	mStream is the input stream.
            // //	cs is the transformation stream.
            MemoryStream mStream = new MemoryStream(rbData);
            CryptoStream cs = new CryptoStream(mStream, desEncrypt, CryptoStreamMode.Read);
            MemoryStream mOut = new MemoryStream();

            // //6. Start performing the encryption
            int bytesRead;
            byte[] output = new byte[1025];

            do
            {
                bytesRead = cs.Read(output, 0, 1024);
                if ((bytesRead != 0))
                    mOut.Write(output, 0, bytesRead);
            }
            while ((bytesRead > 0));

            // //7. Returns the encrypted result after it is base64 encoded
            // //	In this case, the actual result is converted to base64 so that it can be transported over the HTTP protocol without deformation.
            if ((mOut.Length == 0))
                strResult = "";
            else
                strResult = Convert.ToBase64String(mOut.GetBuffer(), 0, System.Convert.ToInt32(mOut.Length));

            return strResult;
        }

        public static string DecryptData(string strKey, string strData)
        {
            string strResult;

            // //1. Generate the Key used for decrypting
            if (!InitKey(strKey))
            {
                strResult = "Error. Failed to generate key for decryption";
                return strResult;
            }

            // //2. Initialize the service provider
            int nReturn = 0;
            DESCryptoServiceProvider descsp = new DESCryptoServiceProvider();
            ICryptoTransform desDecrypt = descsp.CreateDecryptor(m_Key, m_IV);

            // //3. Prepare the streams:
            // //	mOut is the output stream. 
            // //	cs is the transformation stream.
            MemoryStream mOut = new MemoryStream();
            CryptoStream cs = new CryptoStream(mOut, desDecrypt, CryptoStreamMode.Write);

            // //4. Remember to revert the base64 encoding into a byte array to restore the original encrypted data stream
            byte[] bPlain = new byte[strData.Length + 1];
            try
            {
                bPlain = Convert.FromBase64CharArray(strData.ToCharArray(), 0, strData.Length);
            }
            catch (Exception ex)
            {
                strResult = "Error. Input Data is not base64 encoded.";
                return strResult;
            }

            long lRead = 0;
            long lTotal = strData.Length;

            try
            {

                // //5. Perform the actual decryption
                while ((lTotal >= lRead))
                {
                    cs.Write(bPlain, 0, System.Convert.ToInt32(bPlain.Length));
                    // //descsp.BlockSize=64
                    lRead = mOut.Length + System.Convert.ToInt64(Convert.ToInt32(((bPlain.Length / (double)descsp.BlockSize) * descsp.BlockSize)));
                }

                ASCIIEncoding aEnc = new ASCIIEncoding();
                strResult = aEnc.GetString(mOut.GetBuffer(), 0, System.Convert.ToInt32(mOut.Length));

                // //6. Trim the string to return only the meaningful data
                // //	Remember that in the encrypt function, the first 5 character holds the length of the actual data
                // //	This is the simplest way to remember to original length of the data, without resorting to complicated computations.
                string strLen = strResult.Substring(0, 5);
                int nLen = Convert.ToInt32(strLen);
                strResult = strResult.Substring(5, nLen);
                nReturn = System.Convert.ToInt32(mOut.Length);

                return strResult;
            }
            catch (Exception ex)
            {
                strResult = "Error. Decryption Failed. Possibly due to incorrect Key or corrputed data";
                return strResult;
            }
        }

        // /////////////////////////////////////////////////////////////
        // //Private function to generate the keys into member variables
        private static bool InitKey(string strKey)
        {
            try
            {

                // // Convert Key to byte array
                byte[] bp = new byte[strKey.Length + 1];
                ASCIIEncoding aEnc = new ASCIIEncoding();
                aEnc.GetBytes(strKey, 0, strKey.Length, bp, 0);

                // //Hash the key using SHA1
                SHA1CryptoServiceProvider sha = new SHA1CryptoServiceProvider();
                byte[] bpHash = sha.ComputeHash(bp);

                int i;
                // // use the low 64-bits for the key value
                for (i = 0; i <= 7; i++)
                    m_Key[i] = bpHash[i];

                for (i = 8; i <= 15; i++)
                    m_IV[i - 8] = bpHash[i];

                return true;
            }
            catch (Exception ex)
            {

                // //Error Performing Operations
                return false;
            }
        }

        // A simple, string-oriented wrapper class for encryption functions, including 
        // Hashing, Symmetric Encryption, and Asymmetric Encryption.

        // Jeff Atwood
        // http://www.codinghorror.com/

        /// <summary>
        ///     ''' Hash functions are fundamental to modern cryptography. These functions map binary 
        ///     ''' strings of an arbitrary length to small binary strings of a fixed length, known as 
        ///     ''' hash values. A cryptographic hash function has the property that it is computationally
        ///     ''' infeasible to find two distinct inputs that hash to the same value. Hash functions 
        ///     ''' are commonly used with digital signatures and for EncData integrity.
        ///     ''' </summary>
        public class Hash : IDisposable
        {

            /// <summary>
            ///         ''' Type of hash; some are security oriented, others are fast and simple
            ///         ''' </summary>
            public enum Provider
            {
                /// <summary>
                ///             ''' Cyclic Redundancy Check provider, 32-bit
                ///             ''' </summary>
                Crc32,
                /// <summary>
                ///             ''' Secure Hashing Algorithm provider, SHA-1 variant, 160-bit
                ///             ''' </summary>
                Sha1,
                /// <summary>
                ///             ''' Secure Hashing Algorithm provider, SHA-2 variant, 256-bit
                ///             ''' </summary>
                Sha256,
                /// <summary>
                ///             ''' Secure Hashing Algorithm provider, SHA-2 variant, 384-bit
                ///             ''' </summary>
                Sha384,
                /// <summary>
                ///             ''' Secure Hashing Algorithm provider, SHA-2 variant, 512-bit
                ///             ''' </summary>
                Sha512,
                /// <summary>
                ///             ''' Message Digest algorithm 5, 128-bit
                ///             ''' </summary>
                Md5
            }

            private HashAlgorithm _Hash;
            private EncData _HashValue = new EncData();

            private Hash()
            {
            }

            /// <summary>
            ///         ''' Instantiate a new hash of the specified type
            ///         ''' </summary>
            public Hash(Provider provider)
            {
                switch (provider)
                {
                    case Provider.Crc32:
                        {
                            _Hash = new CRC32();
                            break;
                        }

                    case Provider.Md5:
                        {
                            _Hash = new MD5CryptoServiceProvider();
                            break;
                        }

                    case Provider.Sha1:
                        {
                            _Hash = new SHA1Managed();
                            break;
                        }

                    case Provider.Sha256:
                        {
                            _Hash = new SHA256Managed();
                            break;
                        }

                    case Provider.Sha384:
                        {
                            _Hash = new SHA384Managed();
                            break;
                        }

                    case Provider.Sha512:
                        {
                            _Hash = new SHA512Managed();
                            break;
                        }
                }
            }

            /// <summary>
            ///         ''' Returns the previously calculated hash
            ///         ''' </summary>
            public EncData Value
            {
                get
                {
                    return _HashValue;
                }
            }

            /// <summary>
            ///         ''' Calculates hash on a stream of arbitrary length
            ///         ''' </summary>
            public EncData Calculate(System.IO.Stream calculateStream)
            {
                _HashValue.Bytes = _Hash.ComputeHash(calculateStream);
                return _HashValue;
            }

            /// <summary>
            ///         ''' Calculates hash for fixed length <see cref="Data"/>
            ///         ''' </summary>
            public EncData Calculate(EncData data)
            {
                return CalculatePrivate(data.Bytes);
            }

            /// <summary>
            ///         ''' Calculates hash for a string with a prefixed salt value. 
            ///         ''' A "salt" is random EncData prefixed to every hashed value to prevent 
            ///         ''' common dictionary attacks.
            ///         ''' </summary>
            public EncData Calculate(EncData data, EncData salt)
            {
                byte[] nb = new byte[data.Bytes.Length + salt.Bytes.Length - 1 + 1];
                salt.Bytes.CopyTo(nb, 0);
                data.Bytes.CopyTo(nb, salt.Bytes.Length);
                return CalculatePrivate(nb);
            }

            /// <summary>
            ///         ''' Calculates hash for an array of bytes
            ///         ''' </summary>
            private EncData CalculatePrivate(byte[] b)
            {
                _HashValue.Bytes = _Hash.ComputeHash(b);
                return _HashValue;
            }

            private class CRC32 : HashAlgorithm
            {
                private uint result = 0xFFFFFFFF;

                protected override void HashCore(byte[] array, int ibStart, int cbSize)
                {
                    uint lookup;
                    for (int i = ibStart; i <= cbSize - 1; i++)
                    {
                        lookup = (result & 0xFF) ^ array[i];
                        result = ((result & 0xFFFFFF00) / 0x100) & 0xFFFFFF;
                        result = result ^ crcLookup[lookup];
                    }
                }

                protected override byte[] HashFinal()
                {
                    byte[] b = BitConverter.GetBytes(~result);
                    Array.Reverse(b);
                    return b;
                }

                public override void Initialize()
                {
                    result = 0xFFFFFFFF;
                }

                private uint[] crcLookup = new uint[] { 0x0, 0x77073096, 0xEE0E612, 0x990951BA, 0x76DC419, 0x706AF48F, 0xE963A535, 0x9E6495A3, 0xEDB8832, 0x79DCB8A4, 0xE0D5E91E, 0x97D2D988, 0x9B64C2B, 0x7EB17CBD, 0xE7B82D07, 0x90BF1D91, 0x1DB71064, 0x6AB020F2, 0xF3B97148, 0x84BE41DE, 0x1ADAD47D, 0x6DDDE4EB, 0xF4D4B551, 0x83D385C7, 0x136C9856, 0x646BA8C0, 0xFD62F97A, 0x8A65C9E, 0x14015C4F, 0x63066CD9, 0xFA0F3D63, 0x8D080DF5, 0x3B6E20C8, 0x4C69105E, 0xD56041E4, 0xA2677172, 0x3C03E4D1, 0x4B04D447, 0xD20D85FD, 0xA50AB56B, 0x35B5A8FA, 0x42B2986, 0xDBBBC9D6, 0xACBCF940, 0x32D86CE3, 0x45DF5C75, 0xDCD60DCF, 0xABD13D59, 0x26D930A, 0x51DE003A, 0xC8D75180, 0xBFD06116, 0x21B4F4B5, 0x56B3C423, 0xCFBA9599, 0xB8BDA50F, 0x2802B89E, 0x5F058808, 0xC60CD9B2, 0xB10BE924, 0x2F6F7C87, 0x58684C11, 0xC1611DAB, 0xB6662D3D, 0x76DC4190, 0x1DB7106, 0x98D220B, 0xEFD5102A, 0x71B18589, 0x6B6B51F, 0x9FBFE4A5, 0xE8B8D433, 0x7807C9A2, 0xF00F934, 0x9609A88E, 0xE10E9818, 0x7F6A0DBB, 0x86D3D2D, 0x91646C97, 0xE6635C01, 0x6B6B51F4, 0x1C6C6162, 0x856530D8, 0xF262004E, 0x6C0695ED, 0x1B01A57B, 0x8208F4C1, 0xF50FC457, 0x65B0D9C6, 0x12B7E950, 0x8BBEB8EA, 0xFCB9887, 0x62DD1DDF, 0x15DA2D49, 0x8CD37CF3, 0xFBD44C65, 0x4DB26158, 0x3AB551CE, 0xA3BC0074, 0xD4BB30E2, 0x4ADFA541, 0x3DD895D7, 0xA4D1C46D, 0xD3D6F4FB, 0x4369E96A, 0x346ED9F, 0xAD678846, 0xDA60B8D0, 0x44042D73, 0x33031DE5, 0xAA0A4C5F, 0xDD0D7CC9, 0x5005713, 0x270241AA, 0xBE0B1010, 0xC90C2086, 0x5768B525, 0x206F85B3, 0xB966D409, 0xCE61E49F, 0x5EDEF90E, 0x29D9C998, 0xB0D09822, 0xC7D7A8B4, 0x59B33D17, 0x2EB40D81, 0xB7BD5C3B, 0xC0BA6CAD, 0xEDB88320, 0x9ABFB3B6, 0x3B6E20, 0x74B1D29A, 0xEAD54739, 0x9DD277AF, 0x4DB2615, 0x73DC1683, 0xE3630B12, 0x94643B84, 0xD6D6A3E, 0x7A6A5AA8, 0xE40ECF0B, 0x9309FF9D, 0xA00AE27, 0x7D079EB1, 0xF00F9344, 0x8708A3D2, 0x1E01F268, 0x6906C2FE, 0xF762575D, 0x806567CB, 0x196C3671, 0x6E6B06E7, 0xFED41B76, 0x89D32BE0, 0x10DA7A5A, 0x67DD4AC, 0xF9B9DF6F, 0x8EBEEFF9, 0x17B7BE43, 0x60B08ED5, 0xD6D6A3E8, 0xA1D1937E, 0x38D8C2C4, 0x4FDFF252, 0xD1BB67F1, 0xA6BC5767, 0x3FB506DD, 0x48B2364B, 0xD80D2BDA, 0xAF0A1B4, 0x36034AF6, 0x41047A60, 0xDF60EFC3, 0xA867DF55, 0x316E8EEF, 0x4669BE79, 0xCB61B38, 0xBC66831A, 0x256FD2A0, 0x5268E236, 0xCC0C7795, 0xBB0B4703, 0x220216B9, 0x5505262F, 0xC5BA3BBE, 0xB2BD0B28, 0x2BB45A92, 0x5CB36A04, 0xC2D7FFA7, 0xB5D0CF31, 0x2CD99E8B, 0x5BDEAE1D, 0x9B64C2B0, 0xEC63F226, 0x756AA39, 0x26D930A, 0x9C0906A9, 0xEB0E363F, 0x72076785, 0x5005713, 0x95BF4A82, 0xE2B87A14, 0x7BB12BAE, 0xCB61B38, 0x92D28E9B, 0xE5D5BE0D, 0x7CDCEFB7, 0xBDBDF21, 0x86D3D2D4, 0xF1D4E242, 0x68DDB3F8, 0x1FDA836E, 0x81BE16CD, 0xF6B9265B, 0x6FB077E1, 0x18B74777, 0x88085AE6, 0xFF0F6A70, 0x66063BCA, 0x11010B5, 0x8F659EFF, 0xF862AE69, 0x616BFFD3, 0x166CCF45, 0xA00AE278, 0xD70DD2EE, 0x4E048354, 0x3903B3C2, 0xA7672661, 0xD06016F7, 0x4969474D, 0x3E6E77DB, 0xAED16A4A, 0xD9D65AD, 0x40DF0B66, 0x37D83BF0, 0xA9BCAE53, 0xDEBB9EC5, 0x47B2CF7F, 0x30B5FFE9, 0xBDBDF21, 0xCABAC28A, 0x53B39330, 0x24B4A3A6, 0xBAD03605, 0xCDD70693, 0x54DE5729, 0x23D967BF, 0xB3667A2E, 0xC4614AB8, 0x5D681B02, 0x2A6F2B94, 0xB40BBE37, 0xC30C8EA1, 0x5A05DF1B, 0x2D02EF8D };

                public override byte[] Hash
                {
                    get
                    {
                        byte[] b = BitConverter.GetBytes(~result);
                        Array.Reverse(b);
                        return b;
                    }
                }
            }


            protected virtual new void Dispose(bool disposing)
            {
                if (disposing)
                {
                }
            } // Dispose

            public new void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            } // Dispose
        }

        /// <summary>
        ///     ''' Symmetric encryption uses a single key to encrypt and decrypt. 
        ///     ''' Both parties (encryptor and decryptor) must share the same secret key.
        ///     ''' </summary>
        public class Symmetric : IDisposable
        {
            private const string _DefaultIntializationVector = "%1Az=-@qT";
            private const int _BufferSize = 2048;

            public enum Provider
            {
                /// <summary>
                ///             ''' The EncData Encryption Standard provider supports a 64 bit key only
                ///             ''' </summary>
                DES,
                /// <summary>
                ///             ''' The Rivest Cipher 2 provider supports keys ranging from 40 to 128 bits, default is 128 bits
                ///             ''' </summary>
                RC2,
                /// <summary>
                ///             ''' The Rijndael (also known as AES) provider supports keys of 128, 192, or 256 bits with a default of 256 bits
                ///             ''' </summary>
                Rijndael,
                /// <summary>
                ///             ''' The TripleDES provider (also known as 3DES) supports keys of 128 or 192 bits with a default of 192 bits
                ///             ''' </summary>
                TripleDES
            }

            // Private _data As EncData
            private EncData _key;
            private EncData _iv;
            private SymmetricAlgorithm _crypto;
            // Private _EncryptedBytes As Byte()
            // Private _UseDefaultInitializationVector As Boolean

            // Private Sub New()
            // End Sub

            /// <summary>
            ///         ''' Instantiates a new symmetric encryption object using the specified provider.
            ///         ''' </summary>
            public Symmetric(Provider provider)
            {
                switch (provider)
                {
                    case Provider.DES:
                        {
                            _crypto = new DESCryptoServiceProvider();
                            break;
                        }

                    case Provider.RC2:
                        {
                            _crypto = new RC2CryptoServiceProvider();
                            break;
                        }

                    case Provider.Rijndael:
                        {
                            _crypto = new RijndaelManaged();
                            _crypto.Padding = PaddingMode.PKCS7;
                            break;
                        }

                    case Provider.TripleDES:
                        {
                            _crypto = new TripleDESCryptoServiceProvider();
                            break;
                        }
                }

                // -- make sure key and IV are always set, no matter what
                this.Key = RandomKey();

                this.InitializationVector = new EncData(_DefaultIntializationVector);
            }

            public Symmetric(Provider provider, bool useDefaultInitializationVector)
            {
                switch (provider)
                {
                    case Provider.DES:
                        {
                            _crypto = new DESCryptoServiceProvider();
                            break;
                        }

                    case Provider.RC2:
                        {
                            _crypto = new RC2CryptoServiceProvider();
                            break;
                        }

                    case Provider.Rijndael:
                        {
                            _crypto = new RijndaelManaged();
                            _crypto.Padding = PaddingMode.PKCS7;
                            break;
                        }

                    case Provider.TripleDES:
                        {
                            _crypto = new TripleDESCryptoServiceProvider();
                            break;
                        }
                }

                // -- make sure key and IV are always set, no matter what
                this.Key = RandomKey();
                if (useDefaultInitializationVector)
                    this.InitializationVector = new EncData(_DefaultIntializationVector);
                else
                    this.InitializationVector = RandomInitializationVector();
            }

            /// <summary>
            ///         ''' Key size in bytes. We use the default key size for any given provider; if you 
            ///         ''' want to force a specific key size, set this property
            ///         ''' </summary>
            public int KeySizeBytes
            {
                get
                {
                    return _crypto.KeySize / 8;
                }
                set
                {
                    _crypto.KeySize = value * 8;
                    _key.MaxBytes = value;
                }
            }

            /// <summary>
            ///         ''' Key size in bits. We use the default key size for any given provider; if you 
            ///         ''' want to force a specific key size, set this property
            ///         ''' </summary>
            public int KeySizeBits
            {
                get
                {
                    return _crypto.KeySize;
                }
                set
                {
                    _crypto.KeySize = value;
                    _key.MaxBits = value;
                }
            }

            /// <summary>
            ///         ''' The key used to encrypt/decrypt EncData
            ///         ''' </summary>
            public EncData Key
            {
                get
                {
                    return _key;
                }
                set
                {
                    _key = value;
                    _key.MaxBytes = _crypto.LegalKeySizes[0].MaxSize / 8;
                    _key.MinBytes = _crypto.LegalKeySizes[0].MinSize / 8;
                    _key.StepBytes = _crypto.LegalKeySizes[0].SkipSize / 8;
                }
            }

            /// <summary>
            ///         ''' Using the default Cipher Block Chaining (CBC) mode, all EncData blocks are processed using
            ///         ''' the value derived from the previous block; the first EncData block has no previous EncData block
            ///         ''' to use, so it needs an InitializationVector to feed the first block
            ///         ''' </summary>
            public EncData InitializationVector
            {
                get
                {
                    return _iv;
                }
                set
                {
                    _iv = value;
                    _iv.MaxBytes = _crypto.BlockSize / 8;
                    _iv.MinBytes = _crypto.BlockSize / 8;
                }
            }

            /// <summary>
            ///         ''' generates a random Initialization Vector, if one was not provided
            ///         ''' </summary>
            public EncData RandomInitializationVector()
            {
                _crypto.GenerateIV();
                EncData d = new EncData(_crypto.IV);
                return d;
            }

            /// <summary>
            ///         ''' generates a random Key, if one was not provided
            ///         ''' </summary>
            public EncData RandomKey()
            {
                _crypto.GenerateKey();
                EncData d = new EncData(_crypto.Key);
                return d;
            }

            /// <summary>
            ///         ''' Ensures that _crypto object has valid Key and IV
            ///         ''' prior to any attempt to encrypt/decrypt anything
            ///         ''' </summary>
            private void ValidateKeyAndIv(bool isEncrypting)
            {
                if (_key.IsEmpty)
                {
                    if (isEncrypting)
                        _key = RandomKey();
                    else
                        throw new CryptographicException("No key was provided for the decryption operation!");
                }
                if (_iv.IsEmpty)
                {
                    if (isEncrypting)
                        _iv = RandomInitializationVector();
                    else
                        throw new CryptographicException("No initialization vector was provided for the decryption operation!");
                }
                _crypto.Key = _key.Bytes;
                _crypto.IV = _iv.Bytes;
            }

            /// <summary>
            ///         ''' Encrypts the specified EncData using provided key
            ///         ''' </summary>
            public EncData Encrypt(EncData data, EncData key)
            {
                this.Key = key;
                return Encrypt(data);
            }

            /// <summary>
            ///         ''' Encrypts the specified EncData using preset key and preset initialization vector
            ///         ''' </summary>
            public EncData Encrypt(EncData data)
            {
                System.IO.MemoryStream ms = new System.IO.MemoryStream();

                ValidateKeyAndIv(true);

                CryptoStream cs = new CryptoStream(ms, _crypto.CreateEncryptor(), CryptoStreamMode.Write);
                cs.Write(data.Bytes, 0, data.Bytes.Length);
                cs.Close();
                ms.Close();

                return new EncData(ms.ToArray());
            }

            /// <summary>
            ///         ''' Encrypts the stream to memory using provided key and provided initialization vector
            ///         ''' </summary>
            public EncData Encrypt(Stream stream, EncData key, EncData data)
            {
                this.InitializationVector = data;
                this.Key = key;
                return Encrypt(stream);
            }

            /// <summary>
            ///         ''' Encrypts the stream to memory using specified key
            ///         ''' </summary>
            public EncData Encrypt(Stream stream, EncData key)
            {
                this.Key = key;
                return Encrypt(stream);
            }

            /// <summary>
            ///         ''' Encrypts the specified stream to memory using preset key and preset initialization vector
            ///         ''' </summary>
            public EncData Encrypt(Stream stream)
            {
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                byte[] buffer = new byte[2049];
                int i;

                ValidateKeyAndIv(true);

                CryptoStream cs = new CryptoStream(ms, _crypto.CreateEncryptor(), CryptoStreamMode.Write);
                i = stream.Read(buffer, 0, _BufferSize);
                while (i > 0)
                {
                    cs.Write(buffer, 0, i);
                    i = stream.Read(buffer, 0, _BufferSize);
                }

                cs.Close();
                ms.Close();

                return new EncData(ms.ToArray());
            }

            /// <summary>
            ///         ''' Decrypts the specified EncData using provided key and preset initialization vector
            ///         ''' </summary>
            public EncData Decrypt(EncData encryptedData, EncData key)
            {
                this.Key = key;
                return Decrypt(encryptedData);
            }

            /// <summary>
            ///         ''' Decrypts the specified stream using provided key and preset initialization vector
            ///         ''' </summary>
            public EncData Decrypt(Stream encryptedStream, EncData key)
            {
                this.Key = key;
                return Decrypt(encryptedStream);
            }

            /// <summary>
            ///         ''' Decrypts the specified stream using preset key and preset initialization vector
            ///         ''' </summary>
            public EncData Decrypt(Stream encryptedStream)
            {
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                byte[] b = new byte[2049];

                ValidateKeyAndIv(false);
                CryptoStream cs = new CryptoStream(encryptedStream, _crypto.CreateDecryptor(), CryptoStreamMode.Read);

                int i;
                i = cs.Read(b, 0, _BufferSize);

                while (i > 0)
                {
                    ms.Write(b, 0, i);
                    i = cs.Read(b, 0, _BufferSize);
                }
                cs.Close();
                ms.Close();

                return new EncData(ms.ToArray());
            }

            /// <summary>
            ///         ''' Decrypts the specified EncData using preset key and preset initialization vector
            ///         ''' </summary>
            public EncData Decrypt(EncData encryptedData)
            {
                System.IO.MemoryStream ms = new System.IO.MemoryStream(encryptedData.Bytes, 0, encryptedData.Bytes.Length);
                byte[] b = new byte[encryptedData.Bytes.Length - 1 + 1];

                ValidateKeyAndIv(false);
                CryptoStream cs = new CryptoStream(ms, _crypto.CreateDecryptor(), CryptoStreamMode.Read);

                try
                {
                    cs.Read(b, 0, encryptedData.Bytes.Length - 1);
                }
                catch (CryptographicException ex)
                {
                    throw new CryptographicException("Unable to decrypt EncData. The provided key may be invalid.", ex);
                }
                finally
                {
                    cs.Close();
                }
                return new EncData(b);
            }

            protected virtual new void Dispose(bool disposing)
            {
                if (disposing)
                {
                    // dispose managed resources
                    if (_crypto != null)
                    {
                    }
                }
            } // Dispose


            public new void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            } // Dispose
        }

        /// <summary>
        ///     ''' Asymmetric encryption uses a pair of keys to encrypt and decrypt.
        ///     ''' There is a "public" key which is used to encrypt. Decrypting, on the other hand, 
        ///     ''' requires both the "public" key and an additional "private" key. The advantage is 
        ///     ''' that people can send you encrypted messages without being able to decrypt them.
        ///     ''' </summary>
        ///     ''' <remarks>
        ///     ''' The only provider supported is the <see cref="RSACryptoServiceProvider"/>
        ///     ''' </remarks>
        public class Asymmetric
        {
            private RSACryptoServiceProvider _rsa;
            private string _KeyContainerName = "Encryption.AsymmetricEncryption.DefaultContainerName";
            private bool _UseMachineKeystore = true;
            private int _KeySize = 1024;

            private const string _ElementParent = "RSAKeyValue";
            private const string _ElementModulus = "Modulus";
            private const string _ElementExponent = "Exponent";
            private const string _ElementPrimeP = "P";
            private const string _ElementPrimeQ = "Q";
            private const string _ElementPrimeExponentP = "DP";
            private const string _ElementPrimeExponentQ = "DQ";
            private const string _ElementCoefficient = "InverseQ";
            private const string _ElementPrivateExponent = "D";

            // -- http://forum.java.sun.com/thread.jsp?forum=9&thread=552022&tstart=0&trange=15 
            private const string _KeyModulus = "PublicKey.Modulus";
            private const string _KeyExponent = "PublicKey.Exponent";
            private const string _KeyPrimeP = "PrivateKey.P";
            private const string _KeyPrimeQ = "PrivateKey.Q";
            private const string _KeyPrimeExponentP = "PrivateKey.DP";
            private const string _KeyPrimeExponentQ = "PrivateKey.DQ";
            private const string _KeyCoefficient = "PrivateKey.InverseQ";
            private const string _KeyPrivateExponent = "PrivateKey.D";

            /// <summary>
            ///         ''' Represents a public encryption key. Intended to be shared, it 
            ///         ''' contains only the Modulus and Exponent.
            ///         ''' </summary>
            public class PublicKey
            {
                public string Modulus;
                public string Exponent;

                public PublicKey()
                {
                }

                public PublicKey(string keyXml)
                {
                    LoadFromXml(keyXml);
                }

                /// <summary>
                ///             ''' Load public key from App.config or Web.config file
                ///             ''' </summary>
                public void LoadFromConfig()
                {
                    this.Modulus = Utils.GetConfigString(_KeyModulus);
                    this.Exponent = Utils.GetConfigString(_KeyExponent);
                }

                /// <summary>
                ///             ''' Returns *.config file Xml section representing this public key
                ///             ''' </summary>
                public string ToConfigSection()
                {
                    StringBuilder sb = new StringBuilder();
                    {
                        var withBlock = sb;
                        withBlock.Append(Utils.WriteConfigKey(_KeyModulus, this.Modulus));
                        withBlock.Append(Utils.WriteConfigKey(_KeyExponent, this.Exponent));
                    }
                    return sb.ToString();
                }

                /// <summary>
                ///             ''' Writes the *.config file representation of this public key to a file
                ///             ''' </summary>
                public void ExportToConfigFile(string filePath)
                {
                    StreamWriter sw = new StreamWriter(filePath, false);
                    sw.Write(this.ToConfigSection());
                    sw.Close();
                }

                /// <summary>
                ///             ''' Loads the public key from its Xml string
                ///             ''' </summary>
                public void LoadFromXml(string keyXml)
                {
                    this.Modulus = Utils.GetXmlElement(keyXml, "Modulus");
                    this.Exponent = Utils.GetXmlElement(keyXml, "Exponent");
                }

                /// <summary>
                ///             ''' Converts this public key to an RSAParameters object
                ///             ''' </summary>
                public RSAParameters ToParameters()
                {
                    RSAParameters r = new RSAParameters();
                    r.Modulus = Convert.FromBase64String(this.Modulus);
                    r.Exponent = Convert.FromBase64String(this.Exponent);
                    return r;
                }

                /// <summary>
                ///             ''' Converts this public key to its Xml string representation
                ///             ''' </summary>
                public string ToXml()
                {
                    StringBuilder sb = new StringBuilder();
                    {
                        var withBlock = sb;
                        withBlock.Append(Utils.WriteXmlNode(_ElementParent));
                        withBlock.Append(Utils.WriteXmlElement(_ElementModulus, this.Modulus));
                        withBlock.Append(Utils.WriteXmlElement(_ElementExponent, this.Exponent));
                        withBlock.Append(Utils.WriteXmlNode(_ElementParent, true));
                    }
                    return sb.ToString();
                }

                /// <summary>
                ///             ''' Writes the Xml representation of this public key to a file
                ///             ''' </summary>
                public void ExportToXmlFile(string filePath)
                {
                    StreamWriter sw = new StreamWriter(filePath, false);
                    sw.Write(this.ToXml());
                    sw.Close();
                }
            }


            /// <summary>
            ///         ''' Represents a private encryption key. Not intended to be shared, as it 
            ///         ''' contains all the elements that make up the key.
            ///         ''' </summary>
            public class PrivateKey
            {
                public string Modulus;
                public string Exponent;
                public string PrimeP;
                public string PrimeQ;
                public string PrimeExponentP;
                public string PrimeExponentQ;
                public string Coefficient;
                public string PrivateExponent;

                public PrivateKey()
                {
                }

                public PrivateKey(string keyXml)
                {
                    LoadFromXml(keyXml);
                }

                /// <summary>
                ///             ''' Load private key from App.config or Web.config file
                ///             ''' </summary>
                public void LoadFromConfig()
                {
                    this.Modulus = Utils.GetConfigString(_KeyModulus);
                    this.Exponent = Utils.GetConfigString(_KeyExponent);
                    this.PrimeP = Utils.GetConfigString(_KeyPrimeP);
                    this.PrimeQ = Utils.GetConfigString(_KeyPrimeQ);
                    this.PrimeExponentP = Utils.GetConfigString(_KeyPrimeExponentP);
                    this.PrimeExponentQ = Utils.GetConfigString(_KeyPrimeExponentQ);
                    this.Coefficient = Utils.GetConfigString(_KeyCoefficient);
                    this.PrivateExponent = Utils.GetConfigString(_KeyPrivateExponent);
                }

                /// <summary>
                ///             ''' Converts this private key to an RSAParameters object
                ///             ''' </summary>
                public RSAParameters ToParameters()
                {
                    RSAParameters r = new RSAParameters();
                    r.Modulus = Convert.FromBase64String(this.Modulus);
                    r.Exponent = Convert.FromBase64String(this.Exponent);
                    r.P = Convert.FromBase64String(this.PrimeP);
                    r.Q = Convert.FromBase64String(this.PrimeQ);
                    r.DP = Convert.FromBase64String(this.PrimeExponentP);
                    r.DQ = Convert.FromBase64String(this.PrimeExponentQ);
                    r.InverseQ = Convert.FromBase64String(this.Coefficient);
                    r.D = Convert.FromBase64String(this.PrivateExponent);
                    return r;
                }

                /// <summary>
                ///             ''' Returns *.config file Xml section representing this private key
                ///             ''' </summary>
                public string ToConfigSection()
                {
                    StringBuilder sb = new StringBuilder();
                    {
                        var withBlock = sb;
                        withBlock.Append(Utils.WriteConfigKey(_KeyModulus, this.Modulus));
                        withBlock.Append(Utils.WriteConfigKey(_KeyExponent, this.Exponent));
                        withBlock.Append(Utils.WriteConfigKey(_KeyPrimeP, this.PrimeP));
                        withBlock.Append(Utils.WriteConfigKey(_KeyPrimeQ, this.PrimeQ));
                        withBlock.Append(Utils.WriteConfigKey(_KeyPrimeExponentP, this.PrimeExponentP));
                        withBlock.Append(Utils.WriteConfigKey(_KeyPrimeExponentQ, this.PrimeExponentQ));
                        withBlock.Append(Utils.WriteConfigKey(_KeyCoefficient, this.Coefficient));
                        withBlock.Append(Utils.WriteConfigKey(_KeyPrivateExponent, this.PrivateExponent));
                    }
                    return sb.ToString();
                }

                /// <summary>
                ///             ''' Writes the *.config file representation of this private key to a file
                ///             ''' </summary>
                public void ExportToConfigFile(string strFilePath)
                {
                    StreamWriter sw = new StreamWriter(strFilePath, false);
                    sw.Write(this.ToConfigSection());
                    sw.Close();
                }

                /// <summary>
                ///             ''' Loads the private key from its Xml string
                ///             ''' </summary>
                public void LoadFromXml(string keyXml)
                {
                    this.Modulus = Utils.GetXmlElement(keyXml, "Modulus");
                    this.Exponent = Utils.GetXmlElement(keyXml, "Exponent");
                    this.PrimeP = Utils.GetXmlElement(keyXml, "P");
                    this.PrimeQ = Utils.GetXmlElement(keyXml, "Q");
                    this.PrimeExponentP = Utils.GetXmlElement(keyXml, "DP");
                    this.PrimeExponentQ = Utils.GetXmlElement(keyXml, "DQ");
                    this.Coefficient = Utils.GetXmlElement(keyXml, "InverseQ");
                    this.PrivateExponent = Utils.GetXmlElement(keyXml, "D");
                }

                /// <summary>
                ///             ''' Converts this private key to its Xml string representation
                ///             ''' </summary>
                public string ToXml()
                {
                    StringBuilder sb = new StringBuilder();
                    {
                        var withBlock = sb;
                        withBlock.Append(Utils.WriteXmlNode(_ElementParent));
                        withBlock.Append(Utils.WriteXmlElement(_ElementModulus, this.Modulus));
                        withBlock.Append(Utils.WriteXmlElement(_ElementExponent, this.Exponent));
                        withBlock.Append(Utils.WriteXmlElement(_ElementPrimeP, this.PrimeP));
                        withBlock.Append(Utils.WriteXmlElement(_ElementPrimeQ, this.PrimeQ));
                        withBlock.Append(Utils.WriteXmlElement(_ElementPrimeExponentP, this.PrimeExponentP));
                        withBlock.Append(Utils.WriteXmlElement(_ElementPrimeExponentQ, this.PrimeExponentQ));
                        withBlock.Append(Utils.WriteXmlElement(_ElementCoefficient, this.Coefficient));
                        withBlock.Append(Utils.WriteXmlElement(_ElementPrivateExponent, this.PrivateExponent));
                        withBlock.Append(Utils.WriteXmlNode(_ElementParent, true));
                    }
                    return sb.ToString();
                }

                /// <summary>
                ///             ''' Writes the Xml representation of this private key to a file
                ///             ''' </summary>
                public void ExportToXmlFile(string filePath)
                {
                    StreamWriter sw = new StreamWriter(filePath, false);
                    sw.Write(this.ToXml());
                    sw.Close();
                }
            }


            /// <summary>
            ///         ''' Instantiates a new asymmetric encryption session using the default key size; 
            ///         ''' this is usally 1024 bits
            ///         ''' </summary>
            public Asymmetric()
            {
                _rsa = GetRSAProvider();
            }

            /// <summary>
            ///         ''' Instantiates a new asymmetric encryption session using a specific key size
            ///         ''' </summary>
            public Asymmetric(int keySize)
            {
                _KeySize = keySize;
                _rsa = GetRSAProvider();
            }

            /// <summary>
            ///         ''' Sets the name of the key container used to store this key on disk; this is an 
            ///         ''' unavoidable side effect of the underlying Microsoft CryptoAPI. 
            ///         ''' </summary>
            ///         ''' <remarks>
            ///         ''' http://support.microsoft.com/default.aspx?scid=http://support.microsoft.com:80/support/kb/articles/q322/3/71.asp&amp;NoWebContent=1
            ///         ''' </remarks>
            public string KeyContainerName
            {
                get
                {
                    return _KeyContainerName;
                }
                set
                {
                    _KeyContainerName = value;
                }
            }

            /// <summary>
            ///         ''' Returns the current key size, in bits
            ///         ''' </summary>
            public int KeySizeBits
            {
                get
                {
                    return _rsa.KeySize;
                }
            }

            /// <summary>
            ///         ''' Returns the maximum supported key size, in bits
            ///         ''' </summary>
            public int KeySizeMaxBits
            {
                get
                {
                    return _rsa.LegalKeySizes[0].MaxSize;
                }
            }

            /// <summary>
            ///         ''' Returns the minimum supported key size, in bits
            ///         ''' </summary>
            public int KeySizeMinBits
            {
                get
                {
                    return _rsa.LegalKeySizes[0].MinSize;
                }
            }

            /// <summary>
            ///         ''' Returns valid key step sizes, in bits
            ///         ''' </summary>
            public int KeySizeStepBits
            {
                get
                {
                    return _rsa.LegalKeySizes[0].SkipSize;
                }
            }

            /// <summary>
            ///         ''' Returns the default public key as stored in the *.config file
            ///         ''' </summary>
            public PublicKey DefaultPublicKey
            {
                get
                {
                    PublicKey pubkey = new PublicKey();
                    pubkey.LoadFromConfig();
                    return pubkey;
                }
            }

            /// <summary>
            ///         ''' Returns the default private key as stored in the *.config file
            ///         ''' </summary>
            public PrivateKey DefaultPrivateKey
            {
                get
                {
                    PrivateKey privkey = new PrivateKey();
                    privkey.LoadFromConfig();
                    return privkey;
                }
            }

            /// <summary>
            ///         ''' Generates a new public/private key pair as objects
            ///         ''' </summary>
            public void GenerateNewKeySet(ref PublicKey publicKey, ref PrivateKey privateKey)
            {
                string PublicKeyXml = "";
                string PrivateKeyXml = "";
                GenerateNewKeySet(ref PublicKeyXml, ref PrivateKeyXml);
                publicKey = new PublicKey(PublicKeyXml);
                privateKey = new PrivateKey(PrivateKeyXml);
            }

            /// <summary>
            ///         ''' Generates a new public/private key pair as Xml strings
            ///         ''' </summary>
            public void GenerateNewKeySet(ref string publicKeyXml, ref string privateKeyXml)
            {
                RSA rsa = RSA.Create();
                publicKeyXml = rsa.ToXmlString(false);
                privateKeyXml = rsa.ToXmlString(true);
            }

            /// <summary>
            ///         ''' Encrypts EncData using the default public key
            ///         ''' </summary>
            public EncData Encrypt(EncData data)
            {
                PublicKey PublicKey = DefaultPublicKey;
                return Encrypt(data, PublicKey);
            }

            /// <summary>
            ///         ''' Encrypts EncData using the provided public key
            ///         ''' </summary>
            public EncData Encrypt(EncData data, PublicKey publicKey)
            {
                _rsa.ImportParameters(publicKey.ToParameters());
                return EncryptPrivate(data);
            }

            /// <summary>
            ///         ''' Encrypts EncData using the provided public key as Xml
            ///         ''' </summary>
            public EncData Encrypt(EncData data, string publicKeyXml)
            {
                LoadKeyXml(publicKeyXml, false);
                return EncryptPrivate(data);
            }

            private EncData EncryptPrivate(EncData data)
            {
                try
                {
                    return new EncData(_rsa.Encrypt(data.Bytes, false));
                }
                catch (CryptographicException ex)
                {
                    if (ex.Message.ToLower(System.Globalization.CultureInfo.CurrentCulture).IndexOf("bad length", System.StringComparison.CurrentCulture) > -1)
                        throw new CryptographicException("Your EncData is too large; RSA encryption is designed to encrypt relatively small amounts of EncData. The exact byte limit depends on the key size. To encrypt more EncData, use symmetric encryption and then encrypt that symmetric key with asymmetric RSA encryption.", ex);
                    else
                        throw;
                }
            }

            /// <summary>
            ///         ''' Decrypts EncData using the default private key
            ///         ''' </summary>
            public EncData Decrypt(EncData encryptedData)
            {
                PrivateKey PrivateKey = new PrivateKey();
                PrivateKey.LoadFromConfig();
                return Decrypt(encryptedData, PrivateKey);
            }

            /// <summary>
            ///         ''' Decrypts EncData using the provided private key
            ///         ''' </summary>
            public EncData Decrypt(EncData encryptedData, PrivateKey PrivateKey)
            {
                _rsa.ImportParameters(PrivateKey.ToParameters());
                return DecryptPrivate(encryptedData);
            }

            /// <summary>
            ///         ''' Decrypts EncData using the provided private key as Xml
            ///         ''' </summary>
            public EncData Decrypt(EncData encryptedData, string privateKeyXml)
            {
                LoadKeyXml(privateKeyXml, true);
                return DecryptPrivate(encryptedData);
            }

            private void LoadKeyXml(string keyXml, bool isPrivate)
            {
                try
                {
                    _rsa.FromXmlString(keyXml);
                }
                catch (XmlSyntaxException ex)
                {
                    string stringFormat;
                    if (isPrivate)
                        stringFormat = "private";
                    else
                        stringFormat = "public";
                    throw new System.Security.XmlSyntaxException(string.Format("The provided {0} encryption key Xml does not appear to be valid.", stringFormat), ex);
                }
            }

            private EncData DecryptPrivate(EncData encryptedData)
            {
                return new EncData(_rsa.Decrypt(encryptedData.Bytes, false));
            }

            /// <summary>
            ///         ''' gets the default RSA provider using the specified key size; 
            ///         ''' note that Microsoft's CryptoAPI has an underlying file system dependency that is unavoidable
            ///         ''' </summary>
            ///         ''' <remarks>
            ///         ''' http://support.microsoft.com/default.aspx?scid=http://support.microsoft.com:80/support/kb/articles/q322/3/71.asp&amp;NoWebContent=1
            ///         ''' </remarks>
            private RSACryptoServiceProvider GetRSAProvider()
            {
                RSACryptoServiceProvider rsa = null;
                CspParameters csp = null;
                try
                {
                    csp = new CspParameters();
                    csp.KeyContainerName = _KeyContainerName;
                    rsa = new RSACryptoServiceProvider(_KeySize, csp);
                    rsa.PersistKeyInCsp = false;
                    RSACryptoServiceProvider.UseMachineKeyStore = true;
                    return rsa;
                }
                catch (CryptographicException ex)
                {
                    if (ex.Message.ToLower(System.Globalization.CultureInfo.CurrentCulture).IndexOf("csp for this implementation could not be acquired", System.StringComparison.CurrentCulture) > -1)
                        throw new Exception("Unable to obtain Cryptographic Service Provider. " + "Either the permissions are incorrect on the " + @"'C:\Documents and Settings\All Users\Application EncData\Microsoft\Crypto\RSA\MachineKeys' " + "folder, or the current security context '" + System.Security.Principal.WindowsIdentity.GetCurrent().Name + "'" + " does not have access to this folder.", ex);
                    else
                        throw;
                }
                finally
                {
                    if (rsa != null)
                        rsa = null;
                    if (csp != null)
                        csp = null;
                }
            }
        }

        /// <summary>
        ///     ''' represents Hex, Byte, Base64, or String EncData to encrypt/decrypt;
        ///     ''' use the .Text property to set/get a string representation 
        ///     ''' use the .Hex property to set/get a string-based Hexadecimal representation 
        ///     ''' use the .Base64 to set/get a string-based Base64 representation 
        ///     ''' </summary>
        public class EncData
        {
            private byte[] _b;
            private int _MaxBytes = 0;
            private int _MinBytes = 0;
            private int _StepBytes = 0;

            /// <summary>
            ///         ''' Determines the default text encoding across ALL EncData instances
            ///         ''' </summary>
            public static System.Text.Encoding DefaultEncoding = System.Text.Encoding.GetEncoding(1252);
            // Public Shared DefaultEncoding As System.Text.Encoding = System.Text.Encoding.GetEncoding("Windows-1252")
            /// <summary>
            ///         ''' Determines the default text encoding for this EncData instance
            ///         ''' </summary>
            public System.Text.Encoding Encoding = DefaultEncoding;

            /// <summary>
            ///         ''' Creates new, empty encryption EncData
            ///         ''' </summary>
            public EncData()
            {
            }

            /// <summary>
            ///         ''' Creates new encryption EncData with the specified byte array
            ///         ''' </summary>
            public EncData(byte[] b)
            {
                _b = b;
            }

            /// <summary>
            ///         ''' Creates new encryption EncData with the specified string; 
            ///         ''' will be converted to byte array using default encoding
            ///         ''' </summary>
            public EncData(string s)
            {
                this.Text = s;
            }

            /// <summary>
            ///         ''' Creates new encryption EncData using the specified string and the 
            ///         ''' specified encoding to convert the string to a byte array.
            ///         ''' </summary>
            public EncData(string s, System.Text.Encoding encoding)
            {
                this.Encoding = encoding;
                this.Text = s;
            }

            /// <summary>
            ///         ''' returns true if no EncData is present
            ///         ''' </summary>
            public bool IsEmpty
            {
                get
                {
                    if (_b == null)
                        return true;
                    if (_b.Length == 0)
                        return true;
                    return false;
                }
            }

            /// <summary>
            ///         ''' allowed step interval, in bytes, for this EncData; if 0, no limit
            ///         ''' </summary>
            public int StepBytes
            {
                get
                {
                    return _StepBytes;
                }
                set
                {
                    _StepBytes = value;
                }
            }

            /// <summary>
            ///         ''' allowed step interval, in bits, for this EncData; if 0, no limit
            ///         ''' </summary>
            public int StepBits
            {
                get
                {
                    return _StepBytes * 8;
                }
                set
                {
                    _StepBytes = value / 8;
                }
            }

            /// <summary>
            ///         ''' minimum number of bytes allowed for this EncData; if 0, no limit
            ///         ''' </summary>
            public int MinBytes
            {
                get
                {
                    return _MinBytes;
                }
                set
                {
                    _MinBytes = value;
                }
            }

            /// <summary>
            ///         ''' minimum number of bits allowed for this EncData; if 0, no limit
            ///         ''' </summary>
            public int MinBits
            {
                get
                {
                    return _MinBytes * 8;
                }
                set
                {
                    _MinBytes = value / 8;
                }
            }

            /// <summary>
            ///         ''' maximum number of bytes allowed for this EncData; if 0, no limit
            ///         ''' </summary>
            public int MaxBytes
            {
                get
                {
                    return _MaxBytes;
                }
                set
                {
                    _MaxBytes = value;
                }
            }

            /// <summary>
            ///         ''' maximum number of bits allowed for this EncData; if 0, no limit
            ///         ''' </summary>
            public int MaxBits
            {
                get
                {
                    return _MaxBytes * 8;
                }
                set
                {
                    _MaxBytes = value / 8;
                }
            }

            /// <summary>
            ///         ''' Returns the byte representation of the EncData; 
            ///         ''' This will be padded to MinBytes and trimmed to MaxBytes as necessary!
            ///         ''' </summary>
            public byte[] Bytes
            {
                get
                {
                    if (_MaxBytes > 0)
                    {
                        if (_b.Length > _MaxBytes)
                        {
                            byte[] b = new byte[_MaxBytes - 1 + 1];
                            Array.Copy(_b, b, b.Length);
                            _b = b;
                        }
                    }
                    if (_MinBytes > 0)
                    {
                        if (_b.Length < _MinBytes)
                        {
                            byte[] b = new byte[_MinBytes - 1 + 1];
                            Array.Copy(_b, b, _b.Length);
                            _b = b;
                        }
                    }
                    return _b;
                }
                set
                {
                    _b = value;
                }
            }

            /// <summary>
            ///         ''' Sets or returns text representation of bytes using the default text encoding
            ///         ''' </summary>
            public string Text
            {
                get
                {
                    if (_b == null)
                        return "";
                    else
                    {
                        // -- need to handle nulls here; oddly, C# will happily convert
                        // -- nulls into the string whereas VB stops converting at the
                        // -- first null!
                        int i = Array.IndexOf(_b, System.Convert.ToByte(0));
                        if (i >= 0)
                            return this.Encoding.GetString(_b, 0, i);
                        else
                            return this.Encoding.GetString(_b);
                    }
                }
                set
                {
                    _b = this.Encoding.GetBytes(value);
                }
            }

            /// <summary>
            ///         ''' Sets or returns Hex string representation of this EncData
            ///         ''' </summary>
            public string Hex
            {
                get
                {
                    return Utils.ToHex(_b);
                }
                set
                {
                    _b = Utils.FromHex(value);
                }
            }

            /// <summary>
            ///         ''' Sets or returns Base64 string representation of this EncData
            ///         ''' </summary>
            public string Base64
            {
                get
                {
                    return Utils.ToBase64(_b);
                }
                set
                {
                    _b = Utils.FromBase64(value);
                }
            }

            /// <summary>
            ///         ''' Returns text representation of bytes using the default text encoding
            ///         ''' </summary>
            public new string ToString()
            {
                return this.Text;
            }

            /// <summary>
            ///         ''' returns Base64 string representation of this EncData
            ///         ''' </summary>
            public string ToBase64()
            {
                return this.Base64;
            }

            /// <summary>
            ///         ''' returns Hex string representation of this EncData
            ///         ''' </summary>
            public string ToHex()
            {
                return this.Hex;
            }
        }

        /// <summary>
        ///     ''' Friend class for shared utility methods used by multiple Encryption classes
        ///     ''' </summary>
        internal class Utils
        {

            /// <summary>
            ///         ''' converts an array of bytes to a string Hex representation
            ///         ''' </summary>
            internal static string ToHex(byte[] ba)
            {
                if (ba == null || ba.Length == 0)
                    return "";
                const string HexFormat = "{0:X2}";
                StringBuilder sb = new StringBuilder();
                foreach (byte b in ba)
                    sb.Append(string.Format(HexFormat, b));
                return sb.ToString();
            }

            /// <summary>
            ///         ''' converts from a string Hex representation to an array of bytes
            ///         ''' </summary>
            internal static byte[] FromHex(string hexEncoded)
            {
                if (hexEncoded == null || hexEncoded.Length == 0)
                    return null;
                try
                {
                    int l = Convert.ToInt32(hexEncoded.Length / (double)2);
                    byte[] b = new byte[l - 1 + 1];
                    for (int i = 0; i <= l - 1; i++)
                        b[i] = Convert.ToByte(hexEncoded.Substring(i * 2, 2), 16);
                    return b;
                }
                catch (Exception ex)
                {
                    throw new System.FormatException("The provided string does not appear to be Hex encoded:" + Environment.NewLine + hexEncoded + Environment.NewLine, ex);
                }
            }

            /// <summary>
            ///         ''' converts from a string Base64 representation to an array of bytes
            ///         ''' </summary>
            internal static byte[] FromBase64(string base64Encoded)
            {
                if (base64Encoded == null || base64Encoded.Length == 0)
                    return null;
                try
                {
                    return Convert.FromBase64String(base64Encoded);
                }
                catch (FormatException ex)
                {
                    throw new System.FormatException("The provided string does not appear to be Base64 encoded:" + Environment.NewLine + base64Encoded + Environment.NewLine, ex);
                }
            }

            /// <summary>
            ///         ''' converts from an array of bytes to a string Base64 representation
            ///         ''' </summary>
            internal static string ToBase64(byte[] b)
            {
                if (b == null || b.Length == 0)
                    return "";
                return Convert.ToBase64String(b);
            }

            /// <summary>
            ///         ''' retrieve an element from an Xml string
            ///         ''' </summary>
            internal static string GetXmlElement(string Xml, string element)
            {
                Match m;
                m = Regex.Match(Xml, "<" + element + ">(?<Element>[^>]*)</" + element + ">", RegexOptions.IgnoreCase);
                if (m == null)
                    throw new Exception("Could not find <" + element + "></" + element + "> in provided Public Key Xml.");
                return m.Groups["Element"].ToString();
            }

            /// <summary>
            ///         ''' Returns the specified string value from the application .config file
            ///         ''' </summary>
            internal static string GetConfigString(string key, bool isRequired = true)
            {
                string strReturn = "";
                string s = System.Convert.ToString(WebConfigurationManager.AppSettings.Get(key));
                if (s == null)
                {
                    if (isRequired)
                    {
                    }
                    else
                        strReturn = "";
                }
                else
                    strReturn = s;
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
                    s = "</{0}>" + Environment.NewLine;
                else
                    s = "<{0}>" + Environment.NewLine;
                return string.Format(s, element);
            }
        }

        /// <summary>
        ///     ''' Utility class for handling encryption and hashing
        ///     ''' </summary>
        ///     ''' <remarks></remarks>
        public sealed class RC4
        {
            public RC4()
            {
            }

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

                    returnValue = EnDeCrypt(message, key);
                    returnValue = StringToHex(returnValue);

                    return returnValue;
                }
                catch (Exception ex)
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
                catch (Exception ex)
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
                    char ctmp = (password.Substring((a % intLength), 1).ToCharArray()[0]);

                    key[a] = Microsoft.VisualBasic.Strings.Asc(ctmp);
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

                    itmp = Strings.Asc(ctmp);

                    int cipherby = itmp ^ k;

                    cipher.Append(Strings.Chr(cipherby));
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
                long index;
                long maxIndex;
                StringBuilder hexSb = new StringBuilder();
                string hexOut = string.Empty;

                maxIndex = Strings.Len(message);

                for (index = 1; index <= maxIndex; index++)
                    hexSb.Append(Strings.Right("0" + Conversion.Hex(Strings.Asc(Strings.Mid(message, System.Convert.ToInt32(index), 1))), 2));

                hexOut = hexSb.ToString();
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
                long index;
                long maxIndex;
                StringBuilder sb = new StringBuilder();
                string returnString = string.Empty;

                maxIndex = Strings.Len(hex);

                for (index = 1; index <= maxIndex; index += 2)
                    sb.Append(Strings.Chr(System.Convert.ToInt32("&h" + Strings.Mid(hex, System.Convert.ToInt32(index), 2))));

                returnString = sb.ToString();
                sb.Length = 0;

                return returnString;
            }
        }
    }
}