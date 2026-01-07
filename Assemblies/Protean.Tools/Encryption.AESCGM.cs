using System;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace Protean.Tools
{
    public static class AESCGM
    {
        // AES-GCM helpers (requires BouncyCastle NuGet)
        public static string EncryptAesGcm(string plaintext, string passphrase)
        {
            if (string.IsNullOrEmpty(plaintext)) return string.Empty;

            byte[] salt = new byte[16];
            byte[] iv = new byte[12];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
                rng.GetBytes(iv);
            }

            using (var kdf = new Rfc2898DeriveBytes(passphrase, salt, 10000))
            {
                byte[] key = kdf.GetBytes(32); // 256-bit

                var gcm = new GcmBlockCipher(new AesEngine());
                var parameters = new AeadParameters(new KeyParameter(key), 128, iv, null);
                gcm.Init(true, parameters);

                byte[] plainBytes = Encoding.UTF8.GetBytes(plaintext);
                byte[] output = new byte[gcm.GetOutputSize(plainBytes.Length)];
                int len1 = gcm.ProcessBytes(plainBytes, 0, plainBytes.Length, output, 0);
                int len2 = gcm.DoFinal(output, len1);
                int total = len1 + len2;

                byte[] result = new byte[16 + 12 + total];
                Array.Copy(salt, 0, result, 0, 16);
                Array.Copy(iv, 0, result, 16, 12);
                Array.Copy(output, 0, result, 28, total);
                return Convert.ToBase64String(result);
            }
        }

        public static string DecryptAesGcm(string base64Input, string passphrase)
        {
            if (string.IsNullOrEmpty(base64Input)) return string.Empty;
            try
            {
                byte[] all = Convert.FromBase64String(base64Input);
                if (all.Length < 16 + 12 + 16) return string.Empty; // salt+iv+tag minimum

                byte[] salt = new byte[16];
                byte[] iv = new byte[12];
                Array.Copy(all, 0, salt, 0, 16);
                Array.Copy(all, 16, iv, 0, 12);

                int cipherLen = all.Length - 28;
                byte[] cipherAndTag = new byte[cipherLen];
                Array.Copy(all, 28, cipherAndTag, 0, cipherLen);

                using (var kdf = new Rfc2898DeriveBytes(passphrase, salt, 10000))
                {
                    byte[] key = kdf.GetBytes(32);
                    var gcm = new GcmBlockCipher(new AesEngine());
                    var parameters = new AeadParameters(new KeyParameter(key), 128, iv, null);
                    gcm.Init(false, parameters);

                    byte[] output = new byte[gcm.GetOutputSize(cipherAndTag.Length)];
                    int len1 = gcm.ProcessBytes(cipherAndTag, 0, cipherAndTag.Length, output, 0);
                    int len2 = gcm.DoFinal(output, len1);
                    int total = len1 + len2;
                    return Encoding.UTF8.GetString(output, 0, total);
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        // Wrapper to support legacy RC4 hex tokens or new AES-GCM Base64 tokens.
        // IMPORTANT: callers must URL-decode the token before calling this method.
        public static string DecryptToken(string token, string passphrase)
        {
            if (string.IsNullOrEmpty(token)) return string.Empty;

            // Caller should URL-decode before calling. Try Base64/AES-GCM first:
            try
            {
                // Normalize spaces -> plus (common URL transport issue)
                var base64 = token.Replace(" ", "+");
                // Attempt to parse as Base64; will throw FormatException if not Base64
                Convert.FromBase64String(base64);
                // If parse succeeded, assume AES-GCM format
                return DecryptAesGcm(base64, passphrase);
            }
            catch (FormatException)
            {
                // Not Base64 → assume legacy RC4 hex token
                try
                {
                    return RC4.Decrypt(token, passphrase);
                }
                catch
                {
                    return string.Empty;
                }
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}