using System.Security.Cryptography;
using System.Text;
using Serilog;

namespace TodoApi.Util
{
    public class Encryption
    {
        private static readonly IConfiguration _configuration = Startup.StaticConfiguration!;
        private static readonly string _EncryptionKey = _configuration.GetSection("Encryption:EncryptionKey").Value;
        private static readonly string _SaltKey = _configuration.GetSection("Encryption:EncryptionSalt").Value;
        private static readonly string _ClientEncryptionKey = _configuration.GetSection("Encryption:ClientEncryptionKey").Value;
        private static readonly string _ClientEncryptionSalt = _configuration.GetSection("Encryption:ClientEncryptionSalt").Value;
        
        public static string EncryptFileName(string FileName)
        {
            return Encrypt_CBC_256(FileName);
        }
        public static string DecryptFileName(string EncFileName)
        {
            return Decrypt_CBC_256(EncFileName);
        }
        public static string DecodeDecryptFileName(string Base64EncFileName)
        {
            byte[] data = Convert.FromBase64String(Base64EncFileName);  //it is B64 encoded from url
            string EncFileName = Encoding.UTF8.GetString(data);
            return Decrypt_CBC_256(EncFileName);
        }
        public static string DecryptClientFileName(string EncFileName)
        {
            return DecryptClient_CBC_256(EncFileName);
        }
        public static string Decode_URLParam(string cipherText)
        {
            byte[] data = Convert.FromBase64String(cipherText);  //it is B64 encoded from url
            cipherText = Encoding.UTF8.GetString(data);
            return cipherText;
        }

        private static string Encrypt_CBC_256(string PlainText, string EncryptionKey = "")
        {
            
            if(PlainText == "") 
                return "";

            string encryptString = "";
            try
            {
                if (EncryptionKey.Trim() == "") EncryptionKey = _EncryptionKey;  // You can overwrite default enc key
                var bsaltkey = Encoding.UTF8.GetBytes(_SaltKey);
                byte[] clearBytes = Encoding.UTF8.GetBytes(PlainText);
                
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new(EncryptionKey, bsaltkey);
                    encryptor.Key = pdb.GetBytes(32);  //256 bit Key
                    encryptor.IV = GenerateRandomBytes(16);
                    encryptor.Mode = CipherMode.CBC;
                    encryptor.Padding = PaddingMode.PKCS7;

                    using (MemoryStream ms = new())
                    {
                        using (CryptoStream cs = new(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(clearBytes, 0, clearBytes.Length);
                        }
                        byte[] result = MergeArrays(encryptor.IV, ms.ToArray());  //append IV to cipher, so cipher length will longer
                        encryptString = Convert.ToBase64String(result);

                    }
                }
                return encryptString;
            }
            catch (Exception ex)
            {
                Log.Error("Encrypt_CBC_256: " + ex.Message);
            }
            return encryptString;
        }
		
		private static string Decrypt_CBC_256(string cipherText, string EncryptionKey = "")
        {
            if(cipherText == "") 
                return "";

            string plainText = "";
            try
            {
                if (EncryptionKey.Trim() == "") EncryptionKey = _EncryptionKey;
                var bsaltkey = Encoding.UTF8.GetBytes(_SaltKey);
                
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                using (Aes encryptor = Aes.Create())
                {

                    Rfc2898DeriveBytes pdb = new(EncryptionKey, bsaltkey);
                    encryptor.Mode = CipherMode.CBC;
                    encryptor.Padding = PaddingMode.PKCS7;
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = cipherBytes.Take(16).ToArray();
                    cipherBytes = cipherBytes.Skip(16).ToArray();

                    using (MemoryStream ms = new())
                    {
                        using (CryptoStream cs = new(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                        }
                        plainText = Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
                return plainText;
            }
            catch (Exception ex)
            {
                Log.Error("Decrypt_CBC_256: " + ex.Message);
            }
            return plainText;
        }

        private static string DecryptClient_CBC_256(string cipherText)
        {
            if(cipherText == "")
                return "";

            string plainText = "";
            try
            {
                
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                using (Aes encryptor = Aes.Create())
                {
                    byte[] ClientSalt = Encoding.UTF8.GetBytes(_ClientEncryptionSalt);

                    Rfc2898DeriveBytes pdb = new(_ClientEncryptionKey, ClientSalt, 1000, HashAlgorithmName.SHA256);
                    
                    encryptor.Mode = CipherMode.CBC;
                    encryptor.Padding = PaddingMode.PKCS7;
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = cipherBytes.Take(16).ToArray();
                    cipherBytes = cipherBytes.Skip(16).ToArray();

                    using (MemoryStream ms = new())
                    {
                        using (CryptoStream cs = new(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                        }
                        plainText = Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
                return plainText;
            }
            catch (Exception ex)
            {
                Log.Error("DecryptClient_CBC_256: " + ex.Message);
            }
            return plainText;
        }

        private static byte[] GenerateRandomBytes(int numberOfBytes)
        {
            var randomBytes = new byte[numberOfBytes];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return randomBytes;
        }

        private static byte[] MergeArrays(params byte[][] arrays)
        {
            var merged = new byte[arrays.Sum(a => a.Length)];
            var mergeIndex = 0;
            for (int i = 0; i < arrays.GetLength(0); i++)
            {
                arrays[i].CopyTo(merged, mergeIndex);
                mergeIndex += arrays[i].Length;
            }

            return merged;
        }

    }
}	